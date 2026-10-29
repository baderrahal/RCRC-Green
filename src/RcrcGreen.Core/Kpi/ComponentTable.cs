using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which of the three components.csv files a row came from, lowest standing first.
    /// </summary>
    public enum ComponentSource
    {
        /// <summary>Beside the add-in's DLLs, copied by every install. The team's starting table.</summary>
        Shipped,

        /// <summary>%APPDATA%\RCRC Green, optional, for one person's own lines.</summary>
        Personal,

        /// <summary>In the templates folder the person set in Setup. It wins for any value it holds.</summary>
        Shared
    }

    /// <summary>
    /// One line of a components.csv: a PRX_Component value, the template it fills and the folder
    /// its checklist is filed under, with the file and line it came off.
    /// </summary>
    public sealed class ComponentRow
    {
        public ComponentRow(string component, string template, string folder, ComponentSource source, string path, int line)
        {
            if (string.IsNullOrWhiteSpace(component)) throw new ArgumentNullException("component");

            Component = component.Trim();
            Template = (template ?? string.Empty).Trim();
            Folder = (folder ?? string.Empty).Trim();
            Source = source;
            Path = path ?? string.Empty;
            Line = line;
        }

        /// <summary>The value as the file spells it.</summary>
        public string Component { get; }

        /// <summary>The template's name as the file spells it, matched to a template by name.</summary>
        public string Template { get; }

        /// <summary>
        /// The folder exactly as the file spells it. **It is used as written**, never off the
        /// model's spelling, so a model typed LESSTHAN still files beside every other plot of
        /// that value rather than in a second folder.
        /// </summary>
        public string Folder { get; }

        public ComponentSource Source { get; }

        public string Path { get; }

        public int Line { get; }

        public string Key
        {
            get { return ComponentMatch.Key(Component); }
        }
    }

    /// <summary>
    /// What one components.csv held, or why it could not be read. A file that is not there is
    /// not a fault for the personal and shared copies, which are optional, and is said for the
    /// shipped one, which every install puts beside the DLLs.
    /// </summary>
    public sealed class ComponentFile
    {
        private ComponentFile(ComponentSource source, string path, bool there, string why,
            IReadOnlyList<ComponentRow> rows, IReadOnlyList<string> problems, IReadOnlyList<string> refused)
        {
            Source = source;
            Path = path ?? string.Empty;
            There = there;
            Why = why ?? string.Empty;
            Rows = rows ?? new List<ComponentRow>();
            Problems = problems ?? new List<string>();
            RefusedKeys = refused ?? new List<string>();
        }

        public ComponentSource Source { get; }

        public string Path { get; }

        public bool There { get; }

        /// <summary>Why the whole file was not used, empty where it was read.</summary>
        public string Why { get; }

        public bool Read
        {
            get { return There && Why.Length == 0; }
        }

        public IReadOnlyList<ComponentRow> Rows { get; }

        /// <summary>Every line that was not taken, each with its line number and why.</summary>
        public IReadOnlyList<string> Problems { get; }

        /// <summary>
        /// Values this file names twice with two answers. **Neither answer is used, and no lower
        /// file answers in its place**, because the file that should decide could not.
        /// </summary>
        public IReadOnlyList<string> RefusedKeys { get; }

        public static ComponentFile Missing(ComponentSource source, string path)
        {
            return new ComponentFile(source, path, false, string.Empty, null, null, null);
        }

        public static ComponentFile Unreadable(ComponentSource source, string path, string why)
        {
            return new ComponentFile(source, path, true, why, null, null, null);
        }

        /// <summary>
        /// The file off disk. Anything that stops it being read is said rather than thrown.
        /// </summary>
        public static ComponentFile From(ComponentSource source, string path)
        {
            return ComponentTable.ReadFile(path,
                lines => Parse(source, path, lines),
                () => Missing(source, path),
                why => Unreadable(source, path, why));
        }

        /// <summary>
        /// The lines of one file. The first line that is not blank and not a # note must be the
        /// header, Component,Template,Folder, or the whole file is refused: a file whose columns
        /// cannot be named is a file whose every row might be read the wrong way round.
        ///
        /// Two things Excel does to a csv are taken: empty fields after the third, which it
        /// writes once any cell in a fourth column was touched, and a semicolon between fields,
        /// which it writes where the list separator is one. The header line says which.
        ///
        /// **A row that cannot be read refuses its value**, the same as two answers do, and no
        /// lower file answers in its place. The file that should decide could not.
        /// </summary>
        public static ComponentFile Parse(ComponentSource source, string path, IEnumerable<string> lines)
        {
            var rows = new List<ComponentRow>();
            var problems = new List<string>();
            var refused = new List<string>();
            bool headed = false;
            char between = ',';
            int number = 0;

            foreach (string raw in lines ?? Enumerable.Empty<string>())
            {
                number++;
                string line = ComponentTable.WithoutMark(raw);
                if (ComponentTable.IsNote(line)) continue;

                if (!headed)
                {
                    if (!ComponentTable.ReadHeader(line, ComponentTable.HeaderFields, out between))
                    {
                        return Unreadable(source, path, "Line " + number + " should be the header "
                            + ComponentTable.Header + " and reads " + line.Trim() + ".");
                    }

                    headed = true;
                    continue;
                }

                List<string> fields = ComponentTable.Fields(line, between);
                if (ComponentTable.IsBlankRow(fields)) continue;

                string bad = ComponentTable.RowShapeFault(fields);

                if (bad.Length > 0)
                {
                    string key = fields.Count > 0 ? ComponentMatch.Key(fields[0]) : string.Empty;
                    if (key.Length > 0)
                    {
                        rows.RemoveAll(one => one.Key == key);
                        if (!refused.Contains(key)) refused.Add(key);
                    }

                    problems.Add("line " + number + " " + bad + ": " + line.Trim()
                        + (key.Length > 0 ? ", so " + fields[0].Trim() + " is not used" : string.Empty));
                    continue;
                }

                var row = new ComponentRow(fields[0], fields[1], fields[2], source, path, number);
                ComponentRow before = rows.FirstOrDefault(one => one.Key == row.Key);

                if (refused.Contains(row.Key)) continue;

                if (before == null)
                {
                    rows.Add(row);
                    continue;
                }

                if (SameAnswer(before, row)) continue;

                rows.Remove(before);
                refused.Add(row.Key);
                problems.Add("lines " + before.Line + " and " + number + " give " + row.Component
                    + " two answers, so neither is used");
            }

            if (!headed)
            {
                return Unreadable(source, path, "It holds no header line, " + ComponentTable.Header + ".");
            }

            return new ComponentFile(source, path, true, string.Empty, rows, problems, refused);
        }

        private static bool SameAnswer(ComponentRow one, ComponentRow other)
        {
            // Windows holds two folder names differing in case as one folder.
            return ComponentMatch.Same(one.Template, other.Template)
                && string.Equals(one.Folder, other.Folder, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// **The one table PRX_Component is read against**: which template each value fills and which
    /// folder its checklist is filed under. It replaces two tables in code that were keyed on the
    /// same eleven values and compared them twice.
    ///
    /// Three files, read lowest standing first, and a value takes its row from the highest file
    /// that names it: the shipped components.csv beside the DLLs, the personal one in
    /// %APPDATA%\RCRC Green, and the shared one in the templates folder. The shared copy wins
    /// because it is the team's and everybody reads it. Every row keeps the file it came off,
    /// so the report can say where each answer was read.
    ///
    /// **A value no row holds is tried against the rules**, component-rules.csv, under
    /// <see cref="ComponentRules"/>, and <see cref="PlaceOf"/> is the one place that order is
    /// decided. A value no row and no rule places places nothing, exactly as before.
    /// </summary>
    public sealed class ComponentTable
    {
        public const string FileName = "components.csv";

        public const string Header = "Component,Template,Folder";

        internal static readonly string[] HeaderFields = { "Component", "Template", "Folder" };

        private static volatile ComponentTable _inForce;

        private static readonly Lazy<ComponentTable> ShippedTable = new Lazy<ComponentTable>(
            () => Layered(ComponentFile.From(ComponentSource.Shipped, ShippedPath))
                .WithRules(ComponentRuleFile.From(ComponentSource.Shipped, ShippedRulesPath)));

        private ComponentTable(IReadOnlyList<ComponentFile> files, IReadOnlyList<ComponentRow> rows, IReadOnlyList<KpiTemplate> templates,
            IReadOnlyList<ComponentRuleFile> ruleFiles, ComponentRuleFile rulesInForce)
        {
            Files = files;
            Rows = rows;
            Templates = templates;
            RuleFiles = ruleFiles ?? new List<ComponentRuleFile>();
            RulesFile = rulesInForce;
        }

        /// <summary>The files as read, lowest standing first, every one of them, read or not.</summary>
        public IReadOnlyList<ComponentFile> Files { get; }

        /// <summary>
        /// One row per value, the winning one, in the order the shipped file lists them and then
        /// each higher file's own new values in its order.
        /// </summary>
        public IReadOnlyList<ComponentRow> Rows { get; }

        /// <summary>The templates a row's Template column is matched against.</summary>
        public IReadOnlyList<KpiTemplate> Templates { get; }

        /// <summary>The rules files as read, shipped first, read or not.</summary>
        public IReadOnlyList<ComponentRuleFile> RuleFiles { get; }

        /// <summary>The rules file whose rules are tried, null where none was read.</summary>
        public ComponentRuleFile RulesFile { get; }

        /// <summary>The rules tried, in order. Empty where no rules file was read.</summary>
        public IReadOnlyList<ComponentRule> Rules
        {
            get { return RulesFile != null ? RulesFile.Rules : new List<ComponentRule>(); }
        }

        /// <summary>
        /// Where the shipped file sits: beside the Core assembly, which is beside the add-in once
        /// installed and beside the tests once built, because the Core project copies
        /// install/components.csv into its output. One file in the repo, read both places.
        /// </summary>
        public static string ShippedPath
        {
            get
            {
                string here = Path.GetDirectoryName(typeof(ComponentTable).Assembly.Location) ?? string.Empty;
                return Path.Combine(here, FileName);
            }
        }

        /// <summary>component-rules.csv beside the Core assembly, copied there the same way.</summary>
        public static string ShippedRulesPath
        {
            get
            {
                string here = Path.GetDirectoryName(typeof(ComponentTable).Assembly.Location) ?? string.Empty;
                return Path.Combine(here, ComponentRules.FileName);
            }
        }

        /// <summary>The shipped components.csv and the shipped rules, read once.</summary>
        public static ComponentTable Shipped
        {
            get { return ShippedTable.Value; }
        }

        /// <summary>
        /// The table every static lookup reads: the shipped one until the pane puts the three
        /// files in force. One reference swapped whole, so a reader never sees half a table.
        /// </summary>
        public static ComponentTable InForce
        {
            get { return _inForce ?? Shipped; }
        }

        /// <summary>
        /// Puts a table in force for every lookup in this Revit session. Only the Revit side
        /// calls it, the pane on every redraw and the handler at the press, through
        /// ComponentFiles. A test builds a table and asks it directly instead.
        /// </summary>
        public static void Use(ComponentTable table)
        {
            _inForce = table;
        }

        public static ComponentTable Layered(params ComponentFile[] lowestFirst)
        {
            return Layered(KpiTemplates.All, lowestFirst);
        }

        public static ComponentTable Layered(IReadOnlyList<KpiTemplate> templates, params ComponentFile[] lowestFirst)
        {
            List<ComponentFile> files = (lowestFirst ?? new ComponentFile[0]).Where(one => one != null).ToList();
            var rows = new List<ComponentRow>();

            foreach (ComponentFile file in files)
            {
                foreach (string key in file.RefusedKeys) rows.RemoveAll(one => one.Key == key);

                foreach (ComponentRow row in file.Rows)
                {
                    int at = rows.FindIndex(one => one.Key == row.Key);
                    if (at >= 0) rows[at] = row;
                    else rows.Add(row);
                }
            }

            return new ComponentTable(files, rows, templates ?? KpiTemplates.All, null, null);
        }

        /// <summary>
        /// The same rows with rules: the shipped rules file, then the shared one, which replaces
        /// the shipped list where it was read. A table built without this tries no rule.
        /// </summary>
        public ComponentTable WithRules(ComponentRuleFile shipped, ComponentRuleFile shared = null)
        {
            List<ComponentRuleFile> files = new[] { shipped, shared }.Where(one => one != null).ToList();
            ComponentRuleFile inForce = ComponentRules.InForce(shipped, shared);
            return new ComponentTable(Files, Rows, Templates, files, inForce != null && inForce.Read ? inForce : null);
        }

        /// <summary>
        /// **What places a value, in Bader's order**: its exact row in a components.csv first,
        /// then nothing where a components.csv refused it, because the file that should decide
        /// could not, then the first rule whose words it holds, then nothing. Every lookup on a
        /// component, template and folder alike, comes through here.
        /// </summary>
        public ComponentPlacement PlaceOf(string component)
        {
            string key = ComponentMatch.Key(component);
            if (key.Length == 0) return ComponentPlacement.Nothing;

            ComponentRow row = RowFor(component);
            if (row != null) return ComponentPlacement.ByRow(row, KpiTemplates.Named(row.Template, Templates));

            if (Files.Any(file => file.RefusedKeys.Contains(key))) return ComponentPlacement.RefusedByAFile;

            ComponentRule rule = ComponentRules.FirstMatching(Rules, component);
            return rule == null
                ? ComponentPlacement.Nothing
                : ComponentPlacement.ByRule(rule, KpiTemplates.Named(rule.Template, Templates), component);
        }

        /// <summary>The winning row for a value, or null when no file holds it.</summary>
        public ComponentRow RowFor(string component)
        {
            string key = ComponentMatch.Key(component);
            if (key.Length == 0) return null;

            return Rows.FirstOrDefault(one => one.Key == key);
        }

        /// <summary>
        /// Whether any file says something about a value: a row in force, a rule it matches, or
        /// a refusal in one of the files. A value no file names is the only kind the pane offers
        /// a line for.
        /// </summary>
        public bool Names(string component)
        {
            ComponentPlacement placed = PlaceOf(component);
            return placed.Placed || placed.Refused;
        }

        /// <summary>
        /// The template a value fills, or null where nothing places the value or what places it
        /// names a template this table does not know.
        /// </summary>
        public KpiTemplate TemplateFor(string component)
        {
            return PlaceOf(component).Template;
        }

        /// <summary>The folder a value files under, or empty where nothing places it.</summary>
        public string FolderFor(string component)
        {
            return PlaceOf(component).Folder;
        }

        /// <summary>
        /// Every folder the rows and the rules of one template reach, and whether one of its
        /// rules names the folder or its main after each value, which reaches a folder per value
        /// and so no one folder at all.
        /// </summary>
        public IReadOnlyList<string> FoldersOf(KpiTemplate template, out bool perValue)
        {
            string main;
            return Filings(template, out perValue, out main);
        }

        /// <summary>
        /// The one folder every plot of a template is filed under, and its main, or empty where
        /// the template reaches more than one, or a folder per value. A row files one level.
        /// </summary>
        public string OnlyFolderFor(KpiTemplate template, out string main)
        {
            bool perValue;
            IReadOnlyList<string> folders = Filings(template, out perValue, out main);
            if (folders.Count == 1 && !perValue && main != null) return folders[0];

            main = string.Empty;
            return string.Empty;
        }

        /// <summary>
        /// The named folders, the per value flag, and the main shared by every one of them, null
        /// where two answers name two mains, so one folder under two mains is not one filing.
        /// </summary>
        private IReadOnlyList<string> Filings(KpiTemplate template, out bool perValue, out string main)
        {
            perValue = false;
            main = string.Empty;
            var seen = new List<string>();
            var mains = new List<string>();
            if (template == null) return seen;

            foreach (ComponentRow row in Rows)
            {
                if (!ReferenceEquals(KpiTemplates.Named(row.Template, Templates), template)) continue;
                if (!seen.Contains(row.Folder, StringComparer.Ordinal)) seen.Add(row.Folder);
                if (!mains.Contains(string.Empty, StringComparer.Ordinal)) mains.Add(string.Empty);
            }

            foreach (ComponentRule rule in Rules)
            {
                if (!ReferenceEquals(KpiTemplates.Named(rule.Template, Templates), template)) continue;
                if (rule.FolderIsTheValue || rule.MainIsTheBase) perValue = true;
                if (rule.FolderIsTheValue) continue;

                if (!seen.Contains(rule.Folder, StringComparer.Ordinal)) seen.Add(rule.Folder);
                if (!mains.Contains(rule.Main, StringComparer.Ordinal)) mains.Add(rule.Main);
            }

            main = mains.Count <= 1 ? (mains.Count == 0 ? string.Empty : mains[0]) : null;
            return seen;
        }

        /// <summary>Rules whose Template column names no template this table knows.</summary>
        public IReadOnlyList<ComponentRule> UnmatchedRules
        {
            get { return Rules.Where(one => KpiTemplates.Named(one.Template, Templates) == null).ToList(); }
        }

        /// <summary>
        /// Rows whose Template column names no template this table knows. They place nothing,
        /// and the report names each with its file and line.
        /// </summary>
        public IReadOnlyList<ComponentRow> Unmatched
        {
            get { return Rows.Where(one => KpiTemplates.Named(one.Template, Templates) == null).ToList(); }
        }

        /// <summary>
        /// The words for where a row was read, the file's path and its line.
        /// </summary>
        public static string ReadFrom(ComponentRow row)
        {
            return row == null ? ReadFromLine(row) : ReadFromLine(row) + ", " + row.Path;
        }

        /// <summary>
        /// The same without the path, for a table whose files are listed once below it.
        /// </summary>
        public static string ReadFromLine(ComponentRow row)
        {
            if (row == null) return "no " + FileName + " holds it";

            return SourceWord(row.Source) + " " + FileName + " line " + row.Line;
        }

        public static string SourceWord(ComponentSource source)
        {
            switch (source)
            {
                case ComponentSource.Shared: return "the shared";
                case ComponentSource.Personal: return "your own";
                default: return "the shipped";
            }
        }

        /// <summary>
        /// Every line about the files, for the report: each file, whether it was read, how many
        /// rows it gave, and every line it did not take.
        /// </summary>
        public IReadOnlyList<string> InWords()
        {
            var lines = new List<string>();

            foreach (ComponentFile file in Files)
            {
                string where = SourceWord(file.Source) + " " + FileName + ", "
                    + (file.Path.Length == 0 ? "no path" : file.Path);

                if (!file.There)
                {
                    lines.Add(where + ": not there" + (file.Source == ComponentSource.Shipped
                        ? ". Every value is unknown until it is. Run the install again." : "."));
                    continue;
                }

                if (!file.Read)
                {
                    lines.Add(where + ": NOT READ. " + file.Why);
                    continue;
                }

                lines.Add(where + ": " + file.Rows.Count + (file.Rows.Count == 1 ? " row" : " rows"));
                foreach (string problem in file.Problems) lines.Add("  not taken, " + problem);
            }

            foreach (ComponentRow row in Unmatched)
            {
                lines.Add("  " + row.Component + " names template " + row.Template
                    + ", which is not a template this tool knows, so it places nothing. " + ReadFrom(row));
            }

            foreach (ComponentRuleFile file in RuleFiles)
            {
                string where = SourceWord(file.Source) + " " + ComponentRules.FileName + ", "
                    + (file.Path.Length == 0 ? "no path" : file.Path);

                if (!file.There)
                {
                    lines.Add(where + ": not there" + (file.Source == ComponentSource.Shipped
                        ? ". No value is placed by a rule until it is. Run the install again." : "."));
                }
                else if (!file.Read)
                {
                    lines.Add(where + ": NOT READ. " + file.Why);
                }
                else
                {
                    lines.Add(where + ": " + file.Rules.Count + (file.Rules.Count == 1 ? " rule" : " rules")
                        + (ReferenceEquals(file, RulesFile) ? ", the rules in force" : ", not used, the shared rules are in force"));
                }
            }

            foreach (ComponentRule rule in UnmatchedRules)
            {
                lines.Add("  rule " + rule.Words + " names template " + rule.Template
                    + ", which is not a template this tool knows, so a value it places goes nowhere. "
                    + ComponentRules.ReadFromLine(rule) + ", " + rule.Path);
            }

            return lines;
        }

        /// <summary>
        /// One csv file off disk, for components.csv and component-rules.csv alike. Anything that
        /// stops it being read is said rather than thrown.
        /// </summary>
        internal static T ReadFile<T>(string path, Func<IEnumerable<string>, T> parse, Func<T> missing, Func<string, T> unreadable)
        {
            if (string.IsNullOrWhiteSpace(path)) return missing();

            try
            {
                if (!File.Exists(path)) return missing();

                // Excel holds a file it has open for writing, and a read that refused to share
                // would drop the team's whole shared table for as long as one person had it open.
                var lines = new List<string>();
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (var reader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null) lines.Add(line);
                }

                return parse(lines);
            }
            catch (IOException failed)
            {
                return unreadable("It could not be read. " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return unreadable("It could not be read. " + failed.Message);
            }
        }

        internal static string WithoutMark(string raw)
        {
            return (raw ?? string.Empty).TrimStart('﻿');
        }

        /// <summary>A blank line or a # note, which neither file reads.</summary>
        internal static bool IsNote(string line)
        {
            return line.Trim().Length == 0 || line.TrimStart().StartsWith("#", StringComparison.Ordinal);
        }

        /// <summary>
        /// Why a row of either file is not three fields with something in each, or empty where
        /// it is. What a bad row costs is each file's own: one value, or the whole rules file.
        /// </summary>
        internal static string RowShapeFault(List<string> fields)
        {
            if (fields.Count != 3) return "has " + fields.Count + " fields, not 3";
            return fields.Any(one => one.Trim().Length == 0) ? "leaves a field empty" : string.Empty;
        }

        /// <summary>A row of nothing but separators is Excel's blank row.</summary>
        internal static bool IsBlankRow(List<string> fields)
        {
            return fields.All(one => one.Trim().Length == 0);
        }

        /// <summary>
        /// Whether a line is the header naming these fields, and the separator it uses: a
        /// semicolon where Excel's list separator is one, else a comma.
        /// </summary>
        internal static bool ReadHeader(string line, string[] names, out char between)
        {
            between = IsHeader(Fields(line, ';'), names) && !IsHeader(Fields(line, ','), names) ? ';' : ',';
            return IsHeader(Fields(line, between), names);
        }

        private static bool IsHeader(List<string> fields, string[] names)
        {
            return fields.Count == names.Length
                && fields.Select(one => one.Trim()).SequenceEqual(names, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// One line split on its separator, a field in double quotes allowed to hold one and a
        /// doubled quote standing for one, which is how Excel saves a csv. Empty fields after the
        /// third are dropped, because Excel writes them once a fourth column was ever touched.
        /// </summary>
        internal static List<string> Fields(string line, char between = ',')
        {
            var fields = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;

            for (int at = 0; at < line.Length; at++)
            {
                char here = line[at];

                if (quoted)
                {
                    if (here == '"' && at + 1 < line.Length && line[at + 1] == '"') { field.Append('"'); at++; }
                    else if (here == '"') quoted = false;
                    else field.Append(here);
                }
                else if (here == '"') quoted = true;
                else if (here == between) { fields.Add(field.ToString()); field.Clear(); }
                else field.Append(here);
            }

            fields.Add(field.ToString());
            while (fields.Count > HeaderFields.Length && fields[fields.Count - 1].Trim().Length == 0) fields.RemoveAt(fields.Count - 1);
            return fields;
        }

        /// <summary>
        /// What to append to a file holding <paramref name="existing"/>, null where it is not
        /// there, so it ends holding the line: the header first into an empty or missing file,
        /// and a line break first where the file does not end in one. Nothing already in the
        /// file is rewritten.
        /// </summary>
        public static string TextToAppend(string existing, string line)
        {
            if (string.IsNullOrWhiteSpace(existing)) return Header + "\r\n" + line + "\r\n";

            bool ends = existing.EndsWith("\n", StringComparison.Ordinal) || existing.EndsWith("\r", StringComparison.Ordinal);
            return (ends ? string.Empty : "\r\n") + line + "\r\n";
        }

        /// <summary>
        /// One row written as a csv line for the pane to add, quoted only where a field needs it.
        /// </summary>
        public static string Line(string component, string template, string folder)
        {
            return string.Join(",", new[] { component, template, folder }.Select(Quoted).ToArray());
        }

        private static string Quoted(string field)
        {
            string held = (field ?? string.Empty).Trim();
            return held.IndexOfAny(new[] { ',', '"', ';' }) >= 0 ? "\"" + held.Replace("\"", "\"\"") + "\"" : held;
        }
    }
}
