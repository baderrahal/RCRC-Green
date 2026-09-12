using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What one sheet name file holds, and every line of it that could not be read. An
    /// unreadable line is kept and said, never dropped, because a file one line short reads
    /// exactly like a file that never had the line.
    /// </summary>
    public sealed class SheetNameFileContents
    {
        internal SheetNameFileContents(
            IReadOnlyList<SheetNamePairing> pairings, IReadOnlyList<string> notRead)
        {
            Pairings = pairings;
            NotRead = notRead;
        }

        public IReadOnlyList<SheetNamePairing> Pairings { get; }

        public IReadOnlyList<string> NotRead { get; }
    }

    /// <summary>
    /// Reading and writing a sheet name settings file.
    ///
    /// Tab separated, three fields: the view code, the view name and the sheet name, the
    /// same shape as the title block file. Only the code is trimmed: a view name is matched
    /// exactly against the model and a sheet name is the team's own words, and a name tidied
    /// up here is a name nothing will match. Nothing here touches a file, because where the
    /// two files sit belongs to the machine rather than to the rule.
    /// </summary>
    public static class SheetNameFile
    {
        public const char Separator = '\t';

        public const char NoteMark = '#';

        public static SheetNameFileContents Read(string text)
        {
            var pairings = new List<SheetNamePairing>();
            var notRead = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return new SheetNameFileContents(pairings, notRead);
            }

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int at = 0; at < lines.Length; at++)
            {
                string line = lines[at];
                if (line.Trim().Length == 0) continue;
                if (line.TrimStart().Length > 0 && line.TrimStart()[0] == NoteMark) continue;

                string[] fields = line.Split(Separator);
                if (fields.Length != 3
                    || fields[0].Trim().Length == 0
                    || fields[1].Length == 0
                    || fields[2].Trim().Length == 0)
                {
                    notRead.Add("line "
                        + (at + 1).ToString(CultureInfo.InvariantCulture) + ", " + line);
                    continue;
                }

                pairings.Add(new SheetNamePairing(
                    new ViewType(fields[0].Trim(), fields[1]),
                    fields[2],
                    SheetNameSource.Unset));
            }

            return new SheetNameFileContents(pairings, notRead);
        }

        /// <summary>
        /// The text of a settings file, with a note at the top for whoever opens it looking
        /// for why a sheet was named something.
        /// </summary>
        public static string Write(IEnumerable<SheetNamePairing> pairings)
        {
            var text = new StringBuilder();

            text.Append("# RCRC Green, which sheet name goes with which view type.").Append(Line);
            text.Append("# One pairing per line, tab separated: view code, view name, sheet ")
                .Append("name.").Append(Line);
            text.Append("# Written by the Drawing Sheet panel whenever a sheet name is typed ")
                .Append("over.").Append(Line);

            foreach (SheetNamePairing one in (pairings ?? Enumerable.Empty<SheetNamePairing>())
                .Where(one => one != null && one.SheetName.Trim().Length > 0)
                .OrderBy(one => one.Type))
            {
                text.Append(one.Type.Code).Append(Separator)
                    .Append(one.Type.ViewName).Append(Separator)
                    .Append(one.SheetName).Append(Line);
            }

            return text.ToString();
        }

        private const string Line = "\r\n";
    }
}
