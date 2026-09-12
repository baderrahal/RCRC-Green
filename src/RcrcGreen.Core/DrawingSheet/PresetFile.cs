using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What one preset file holds, and every line of it that could not be read.
    ///
    /// A line that does not parse is kept rather than dropped. A preset one line short reads
    /// exactly like a preset that never had that line, and a preset short of one sheet
    /// definition would leave a run making five sheets where six were saved, with nothing on
    /// screen to say so.
    /// </summary>
    public sealed class PresetFileContents
    {
        internal PresetFileContents(
            IReadOnlyList<Preset> presets, IReadOnlyList<string> notRead)
        {
            Presets = presets;
            NotRead = notRead;
        }

        public IReadOnlyList<Preset> Presets { get; }

        /// <summary>
        /// Every line that is not blank, is not a note, and is not a record this can read. Each
        /// is said as its line number and the line itself.
        /// </summary>
        public IReadOnlyList<string> NotRead { get; }

        public string InWords()
        {
            if (NotRead.Count == 0)
            {
                return Presets.Count == 1 ? "1 preset read." : Presets.Count + " presets read.";
            }

            return Presets.Count + " presets read and "
                + (NotRead.Count == 1 ? "1 line" : NotRead.Count + " lines")
                + " could not be: " + string.Join("; ", NotRead.ToArray());
        }
    }

    /// <summary>
    /// Reading and writing a preset file.
    ///
    /// **Tab separated, the first field saying what the line is.** Four kinds:
    ///
    /// <code>
    /// preset  A starting point, from DM-11
    /// type    010     LIST OF DRAWINGS
    /// sheet   AR-PRX-Title_Block_A1   LOD     1
    /// on      010     LIST OF DRAWINGS
    /// </code>
    ///
    /// A `type` line is a view type ticked in step 2. A `sheet` line opens a sheet definition
    /// and the `on` lines under it are its views, in the order they go onto sheets. Every line
    /// belongs to the `preset` line above it and every `on` line to the `sheet` line above it,
    /// so a record with nothing above it to belong to is not read and says so rather than
    /// attaching itself to the wrong preset.
    ///
    /// Tab rather than anything else for the reason the title block file gives: a title block
    /// type in this model is named `LOD /  HARDSCAPE SCHEDULES`, with two spaces in the middle,
    /// and a format that splits on whitespace would quietly turn it into a name the model does
    /// not hold. Only the record word, the view code and the count are trimmed. Nothing else is,
    /// because everything else is a name the model holds exactly.
    ///
    /// Nothing here touches a file. The caller reads and writes the text, because where the two
    /// files sit is the one part of this that belongs to the machine rather than to the rule.
    /// </summary>
    public static class PresetFile
    {
        public const char Separator = '\t';

        /// <summary>
        /// A line starting with this is a note for whoever opens the file, not a record.
        /// </summary>
        public const char NoteMark = '#';

        public const string PresetWord = "preset";
        public const string TypeWord = "type";
        public const string SheetWord = "sheet";
        public const string OnWord = "on";

        public static PresetFileContents Read(string text)
        {
            var presets = new List<Preset>();
            var notRead = new List<string>();

            if (string.IsNullOrEmpty(text))
            {
                return new PresetFileContents(presets, notRead);
            }

            string name = null;
            var ticked = new List<ViewType>();
            var sheets = new List<PresetSheetBeingRead>();

            string[] lines = text.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int at = 0; at < lines.Length; at++)
            {
                string line = lines[at];
                if (line.Trim().Length == 0) continue;
                if (line.TrimStart().Length > 0 && line.TrimStart()[0] == NoteMark) continue;

                string[] fields = line.Split(Separator);
                string word = fields[0].Trim();

                if (string.Equals(word, PresetWord, StringComparison.OrdinalIgnoreCase))
                {
                    if (fields.Length != 2 || fields[1].Trim().Length == 0)
                    {
                        notRead.Add(Said(at, line));
                        continue;
                    }

                    if (name != null) presets.Add(Finished(name, ticked, sheets));

                    name = fields[1].Trim();
                    ticked = new List<ViewType>();
                    sheets = new List<PresetSheetBeingRead>();
                    continue;
                }

                if (name == null)
                {
                    // A record before the first preset line has nothing to belong to. Attaching
                    // it to the preset that comes next would put somebody's view type on a
                    // preset they did not save it under.
                    notRead.Add(Said(at, line));
                    continue;
                }

                if (string.Equals(word, TypeWord, StringComparison.OrdinalIgnoreCase))
                {
                    ViewType type = TypeIn(fields);
                    if (type == null) notRead.Add(Said(at, line));
                    else ticked.Add(type);
                    continue;
                }

                if (string.Equals(word, SheetWord, StringComparison.OrdinalIgnoreCase))
                {
                    int perSheet;
                    if (fields.Length != 4
                        || !int.TryParse(
                            fields[3].Trim(),
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out perSheet)
                        || !SheetLayout.IsACount(perSheet))
                    {
                        notRead.Add(Said(at, line));
                        continue;
                    }

                    sheets.Add(new PresetSheetBeingRead(fields[1], fields[2], perSheet));
                    continue;
                }

                if (string.Equals(word, OnWord, StringComparison.OrdinalIgnoreCase))
                {
                    ViewType type = TypeIn(fields);
                    if (type == null || sheets.Count == 0) notRead.Add(Said(at, line));
                    else sheets[sheets.Count - 1].Views.Add(type);
                    continue;
                }

                notRead.Add(Said(at, line));
            }

            if (name != null) presets.Add(Finished(name, ticked, sheets));

            return new PresetFileContents(presets, notRead);
        }

        /// <summary>
        /// The text of a preset file, with a note at the top saying what it is, since the one
        /// who opens it will be somebody looking for why steps 2 and 4 filled themselves in.
        /// </summary>
        public static string Write(IEnumerable<Preset> presets)
        {
            var text = new StringBuilder();

            text.Append("# RCRC Green, saved answers to steps 2 and 4 of the Drawing Sheet panel.")
                .Append(Line);
            text.Append("# Tab separated. preset opens one, type is a ticked view type, sheet ")
                .Append("opens a sheet").Append(Line);
            text.Append("# definition and the on lines under it are its views, in the order they ")
                .Append("go on.").Append(Line);
            text.Append("# A preset holds no plot, no sub plot and no sheet number.").Append(Line);

            foreach (Preset one in (presets ?? Enumerable.Empty<Preset>())
                .Where(one => one != null && one.IsNamed))
            {
                text.Append(PresetWord).Append(Separator).Append(one.Name).Append(Line);

                foreach (ViewType type in one.Ticked)
                {
                    text.Append(TypeWord).Append(Separator)
                        .Append(type.Code).Append(Separator)
                        .Append(type.ViewName).Append(Line);
                }

                foreach (PresetSheet sheet in one.Sheets)
                {
                    text.Append(SheetWord).Append(Separator)
                        .Append(sheet.TitleBlockFamilyName).Append(Separator)
                        .Append(sheet.TitleBlockTypeName).Append(Separator)
                        .Append(sheet.ViewsPerSheet.ToString(CultureInfo.InvariantCulture))
                        .Append(Line);

                    foreach (ViewType type in sheet.Views)
                    {
                        text.Append(OnWord).Append(Separator)
                            .Append(type.Code).Append(Separator)
                            .Append(type.ViewName).Append(Line);
                    }
                }
            }

            return text.ToString();
        }

        private static ViewType TypeIn(string[] fields)
        {
            if (fields.Length != 3 || fields[1].Trim().Length == 0 || fields[2].Length == 0)
            {
                return null;
            }

            // Only the code is trimmed. The view name is what the model holds exactly.
            return new ViewType(fields[1].Trim(), fields[2]);
        }

        private static Preset Finished(
            string name, List<ViewType> ticked, List<PresetSheetBeingRead> sheets)
        {
            return new Preset(
                name,
                ticked,
                sheets.Select(one => new PresetSheet(
                    one.FamilyName, one.TypeName, one.ViewsPerSheet, one.Views)),
                PresetSource.Unset);
        }

        private static string Said(int at, string line)
        {
            return "line " + (at + 1).ToString(CultureInfo.InvariantCulture) + ", " + line;
        }

        private const string Line = "\r\n";

        /// <summary>
        /// One sheet definition while its on lines are still arriving. A PresetSheet is fixed
        /// once it is made, so the views are gathered here first.
        /// </summary>
        private sealed class PresetSheetBeingRead
        {
            public PresetSheetBeingRead(string familyName, string typeName, int viewsPerSheet)
            {
                FamilyName = familyName;
                TypeName = typeName;
                ViewsPerSheet = viewsPerSheet;
                Views = new List<ViewType>();
            }

            public string FamilyName { get; }

            public string TypeName { get; }

            public int ViewsPerSheet { get; }

            public List<ViewType> Views { get; }
        }
    }
}
