using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One line of component-rules.csv: the words a value must hold, the template it then fills
    /// and the folder it is filed under, with the file and line it came off.
    /// </summary>
    public sealed class ComponentRule
    {
        /// <summary>The Folder column's word for a folder named as the value itself.</summary>
        public const string ValueFolder = "value";

        public ComponentRule(string words, string template, string folder, ComponentSource source, string path, int line)
        {
            if (string.IsNullOrWhiteSpace(words)) throw new ArgumentNullException("words");

            Words = string.Join(" ", words.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
            Template = (template ?? string.Empty).Trim();
            Folder = (folder ?? string.Empty).Trim();
            Source = source;
            Path = path ?? string.Empty;
            Line = line;
            WordSet = ComponentRules.WordsOf(Words);
        }

        /// <summary>The words as the file spells them, runs of spaces made one.</summary>
        public string Words { get; }

        public string Template { get; }

        /// <summary>A folder name as the file spells it, or <see cref="ValueFolder"/>.</summary>
        public string Folder { get; }

        public ComponentSource Source { get; }

        public string Path { get; }

        public int Line { get; }

        public IReadOnlyCollection<string> WordSet { get; }

        public bool FolderIsTheValue
        {
            get { return string.Equals(Folder, ValueFolder, StringComparison.OrdinalIgnoreCase); }
        }

        public bool Matches(string value)
        {
            HashSet<string> held = ComponentRules.WordsOf(value);
            return WordSet.Count > 0 && WordSet.All(held.Contains);
        }

        /// <summary>
        /// The folder for a value this rule placed. A folder named as the value is the value
        /// through <see cref="ComponentMatch.Key"/>, so a model that types one value two ways,
        /// LESSTHAN on one sheet and LESS THAN on the next, still files every plot of it in one
        /// folder rather than two.
        /// </summary>
        public string FolderFor(string value)
        {
            return FolderIsTheValue ? ComponentMatch.Key(value) : Folder;
        }
    }

    /// <summary>
    /// What one component-rules.csv held, or why it could not be read.
    /// </summary>
    public sealed class ComponentRuleFile
    {
        private ComponentRuleFile(ComponentSource source, string path, bool there, string why, IReadOnlyList<ComponentRule> rules)
        {
            Source = source;
            Path = path ?? string.Empty;
            There = there;
            Why = why ?? string.Empty;
            Rules = rules ?? new List<ComponentRule>();
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

        /// <summary>In the file's order, which is the order they are tried in.</summary>
        public IReadOnlyList<ComponentRule> Rules { get; }

        public static ComponentRuleFile Missing(ComponentSource source, string path)
        {
            return new ComponentRuleFile(source, path, false, string.Empty, null);
        }

        public static ComponentRuleFile Unreadable(ComponentSource source, string path, string why)
        {
            return new ComponentRuleFile(source, path, true, why, null);
        }

        public static ComponentRuleFile From(ComponentSource source, string path)
        {
            return ComponentTable.ReadFile(path,
                lines => Parse(source, path, lines),
                () => Missing(source, path),
                why => Unreadable(source, path, why));
        }

        /// <summary>
        /// The lines of one file, header first, the same csv as components.csv.
        ///
        /// **One line that cannot be read refuses the whole file**, where components.csv refuses
        /// only that line's value. The rules are tried in order and the first match wins, so a
        /// dropped MOSQUE MORE 60 line would send every over 60 mosque to MOSQUES with nothing
        /// on screen. A file that refuses lets the shipped rules answer, and says so.
        /// </summary>
        public static ComponentRuleFile Parse(ComponentSource source, string path, IEnumerable<string> lines)
        {
            var rules = new List<ComponentRule>();
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
                    if (!ComponentTable.ReadHeader(line, ComponentRules.HeaderFields, out between))
                    {
                        return Unreadable(source, path, "Line " + number + " should be the header "
                            + ComponentRules.Header + " and reads " + line.Trim() + ".");
                    }

                    headed = true;
                    continue;
                }

                List<string> fields = ComponentTable.Fields(line, between);
                if (ComponentTable.IsBlankRow(fields)) continue;

                string bad = ComponentTable.RowShapeFault(fields);
                if (bad.Length == 0 && ComponentRules.WordsOf(fields[0]).Count == 0) bad = "holds no word to match";

                if (bad.Length > 0)
                {
                    return Unreadable(source, path, "Line " + number + " " + bad + ": " + line.Trim()
                        + ". The rules are tried in order, so none of this file is used.");
                }

                rules.Add(new ComponentRule(fields[0], fields[1], fields[2], source, path, number));
            }

            if (!headed)
            {
                return Unreadable(source, path, "It holds no header line, " + ComponentRules.Header + ".");
            }

            return new ComponentRuleFile(source, path, true, string.Empty, rules);
        }
    }

    /// <summary>
    /// What placed one value: a components.csv row, a rule, or nothing, and why nothing.
    /// </summary>
    public sealed class ComponentPlacement
    {
        private ComponentPlacement(ComponentRow row, ComponentRule rule, KpiTemplate template, string folder, bool refused)
        {
            Row = row;
            Rule = rule;
            Template = template;
            Folder = folder ?? string.Empty;
            Refused = refused;
        }

        public static ComponentPlacement ByRow(ComponentRow row, KpiTemplate template)
        {
            return new ComponentPlacement(row, null, template, row.Folder, false);
        }

        public static ComponentPlacement ByRule(ComponentRule rule, KpiTemplate template, string value)
        {
            return new ComponentPlacement(null, rule, template, rule.FolderFor(value), false);
        }

        public static readonly ComponentPlacement Nothing = new ComponentPlacement(null, null, null, string.Empty, false);

        public static readonly ComponentPlacement RefusedByAFile = new ComponentPlacement(null, null, null, string.Empty, true);

        /// <summary>The exact row that placed it, null where a rule or nothing did.</summary>
        public ComponentRow Row { get; }

        /// <summary>The first rule whose words it holds, null where a row or nothing did.</summary>
        public ComponentRule Rule { get; }

        /// <summary>Null where nothing placed it or the template named is not known here.</summary>
        public KpiTemplate Template { get; }

        /// <summary>Empty where nothing placed it.</summary>
        public string Folder { get; }

        /// <summary>A components.csv gave the value two answers or a line it could not read.</summary>
        public bool Refused { get; }

        /// <summary>The template's name as the row or rule spells it, empty where neither.</summary>
        public string TemplateNamed
        {
            get { return Row != null ? Row.Template : Rule != null ? Rule.Template : string.Empty; }
        }

        /// <summary>
        /// The template column of both reports and the pane's tooltip: the template, or a row or
        /// rule naming one not found here, or <paramref name="nothing"/> where nothing placed it.
        /// </summary>
        public string TemplateShown(string nothing)
        {
            if (Template != null) return Template.Name;
            return Placed ? "NONE, " + TemplateNamed + " is not a template this tool knows" : nothing;
        }

        public bool Placed
        {
            get { return Row != null || Rule != null; }
        }

        /// <summary>The words for what placed it, for both reports and the pane's tooltip.</summary>
        public string How
        {
            get
            {
                if (Row != null) return "row, " + ComponentTable.ReadFromLine(Row);
                if (Rule != null) return "rule " + Rule.Words + ", " + ComponentRules.ReadFromLine(Rule);
                return Refused ? "nothing, a " + ComponentTable.FileName + " gave it no usable answer" : "nothing";
            }
        }

        /// <summary>The file the answer came off, empty where nothing placed it.</summary>
        public string FromPath
        {
            get { return Row != null ? Row.Path : Rule != null ? Rule.Path : string.Empty; }
        }
    }

    /// <summary>
    /// **component-rules.csv: a new PRX_Component value is placed by its words**, so the team can
    /// add values without a line each. Bader's rules, 29 Oct 2026, tried top to bottom, the
    /// first whose words ALL appear in the value as whole words wins. Words are compared through
    /// <see cref="ComponentMatch.Key"/> with the per cent sign off each word, so LESSTHAN 60%
    /// holds LESS and 60.
    ///
    /// **An exact components.csv row beats every rule**, for the exceptions. A value no row and
    /// no rule places is placed nowhere, as before.
    ///
    /// Two copies, the shipped one beside the DLLs and the shared one in the templates folder.
    /// The shared one, read whole, replaces the shipped list rather than adding to it, because
    /// two ordered lists have no one order. One not read lets the shipped list answer and says
    /// so.
    ///
    /// Whole words is what keeps the old traps shut: PARK is not a word of PARKING LOT, and 60
    /// is not a word of 600.
    /// </summary>
    public static class ComponentRules
    {
        public const string FileName = "component-rules.csv";

        public const string Header = "Words,Template,Folder";

        internal static readonly string[] HeaderFields = { "Words", "Template", "Folder" };

        /// <summary>
        /// The whole words of a value or a rule, case blind, LESSTHAN apart, % off. The pane's
        /// suggestion for an unknown value reads its words here too.
        /// </summary>
        public static HashSet<string> WordsOf(string text)
        {
            return new HashSet<string>(
                ComponentMatch.Key(text).Split(' ')
                    .Select(word => word.Replace("%", string.Empty))
                    .Where(word => word.Length > 0),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// The rules in force: the shared file's where it was read, else the shipped file's.
        /// </summary>
        public static ComponentRuleFile InForce(ComponentRuleFile shipped, ComponentRuleFile shared)
        {
            if (shared != null && shared.Read) return shared;
            return shipped;
        }

        public static ComponentRule FirstMatching(IEnumerable<ComponentRule> rules, string value)
        {
            if (ComponentMatch.Key(value).Length == 0) return null;
            return (rules ?? Enumerable.Empty<ComponentRule>()).FirstOrDefault(rule => rule.Matches(value));
        }

        public static string ReadFromLine(ComponentRule rule)
        {
            if (rule == null) return "no " + FileName + " holds it";
            return ComponentTable.SourceWord(rule.Source) + " " + FileName + " line " + rule.Line;
        }
    }
}
