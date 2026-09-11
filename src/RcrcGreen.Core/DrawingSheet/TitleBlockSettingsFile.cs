using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What one settings file holds, and every line of it that could not be read.
    ///
    /// A line that does not parse is kept rather than dropped. A settings file one line short
    /// reads exactly like a settings file that never had that line, and this repo has paid for
    /// a silent skip twice.
    /// </summary>
    public sealed class TitleBlockFileContents
    {
        internal TitleBlockFileContents(
            IReadOnlyList<TitleBlockPairing> pairings, IReadOnlyList<string> notRead)
        {
            Pairings = pairings;
            NotRead = notRead;
        }

        public IReadOnlyList<TitleBlockPairing> Pairings { get; }

        /// <summary>
        /// Every line that is not blank, is not a note, and is not four fields. Each is said as
        /// its line number and the line itself.
        /// </summary>
        public IReadOnlyList<string> NotRead { get; }

        public string InWords()
        {
            if (NotRead.Count == 0)
            {
                return Pairings.Count == 1
                    ? "1 pairing read."
                    : Pairings.Count + " pairings read.";
            }

            return Pairings.Count + " pairings read and "
                + (NotRead.Count == 1 ? "1 line" : NotRead.Count + " lines")
                + " could not be: " + string.Join("; ", NotRead.ToArray());
        }
    }

    /// <summary>
    /// Reading and writing a title block settings file.
    ///
    /// **Tab separated, four fields: the view code, the view name, the title block family and
    /// the title block type.** Tab rather than anything else because a title block type in this
    /// model is named `LOD /  HARDSCAPE SCHEDULES`, with two spaces in the middle, and a format
    /// that splits on whitespace or trims fields would quietly turn it into a name the model
    /// does not hold. The code and the name are separate fields for the same reason: writing
    /// them as `(010) Overall Plan` would put a second copy of that format here, and the one
    /// that matters lives on <see cref="ViewType"/>.
    ///
    /// Nothing here touches a file. The caller reads and writes the text, because where the two
    /// files sit is the one part of this that belongs to the machine rather than to the rule.
    /// </summary>
    public static class TitleBlockSettingsFile
    {
        public const char Separator = '\t';

        /// <summary>
        /// A line starting with this is a note for whoever opens the file, not a pairing.
        /// </summary>
        public const char NoteMark = '#';

        public static TitleBlockFileContents Read(string text)
        {
            var pairings = new List<TitleBlockPairing>();
            var notRead = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return new TitleBlockFileContents(pairings, notRead);
            }

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int at = 0; at < lines.Length; at++)
            {
                string line = lines[at];
                if (line.Trim().Length == 0) continue;
                if (line.TrimStart().Length > 0 && line.TrimStart()[0] == NoteMark) continue;

                string[] fields = line.Split(Separator);
                if (fields.Length != 4 || fields[0].Trim().Length == 0 || fields[1].Length == 0)
                {
                    notRead.Add("line "
                        + (at + 1).ToString(CultureInfo.InvariantCulture) + ", " + line);
                    continue;
                }

                // Only the code is trimmed. Everything else is a name the model holds exactly,
                // spaces and all, and a name trimmed here is a name nothing will match.
                pairings.Add(new TitleBlockPairing(
                    new ViewType(fields[0].Trim(), fields[1]),
                    fields[2],
                    fields[3],
                    TitleBlockSource.Unset));
            }

            return new TitleBlockFileContents(pairings, notRead);
        }

        /// <summary>
        /// The text of a settings file, with a note at the top saying what it is, since the one
        /// who opens it will be somebody looking for why a title block filled itself in.
        /// </summary>
        public static string Write(IEnumerable<TitleBlockPairing> pairings)
        {
            var text = new StringBuilder();

            text.Append("# RCRC Green, which title block goes with which view type.").Append(Line);
            text.Append("# One pairing per line, tab separated: view code, view name, title ")
                .Append("block family, title block type.").Append(Line);
            text.Append("# Written by the Drawing Sheet panel whenever a title block is changed.")
                .Append(Line);

            foreach (TitleBlockPairing one in (pairings ?? Enumerable.Empty<TitleBlockPairing>())
                .Where(one => one != null)
                .OrderBy(one => one.Type))
            {
                text.Append(one.Type.Code).Append(Separator)
                    .Append(one.Type.ViewName).Append(Separator)
                    .Append(one.FamilyName).Append(Separator)
                    .Append(one.TypeName).Append(Line);
            }

            return text.ToString();
        }

        private const string Line = "\r\n";
    }
}
