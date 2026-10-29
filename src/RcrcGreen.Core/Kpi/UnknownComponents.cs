using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One PRX_Component value no components.csv row and no rule places, the plots carrying it,
    /// and what the pane suggests for it.
    /// </summary>
    public sealed class UnknownComponent
    {
        public UnknownComponent(string value, IReadOnlyList<string> plots, KpiTemplate suggested, string folder)
        {
            Value = value ?? string.Empty;
            Plots = plots ?? new List<string>();
            Suggested = suggested;
            Folder = folder ?? string.Empty;
        }

        /// <summary>The value as the first plot carrying it spells it.</summary>
        public string Value { get; }

        public string Key
        {
            get { return ComponentMatch.Key(Value); }
        }

        public IReadOnlyList<string> Plots { get; }

        /// <summary>Null where no known template shares a word with the value.</summary>
        public KpiTemplate Suggested { get; }

        public string Folder { get; }
    }

    /// <summary>
    /// **A value no row and no rule places is never placed by a guess.** A rule is not a guess:
    /// it is Bader's words, written down in component-rules.csv. Its plots are refused as they
    /// always were, with the value named, and the pane shows one box per value after a read with
    /// a suggestion a person can take with one press. Taking it writes one line, and only then
    /// does a plot of that value go anywhere.
    ///
    /// The suggestion: the known template whose name shares the most words with the value, and
    /// the value itself as the folder, which is how every folder the team names reads. Words are
    /// compared through <see cref="ComponentMatch.Key"/>, with a plural S and a per cent sign off
    /// each word, so SCHOOL meets SCHOOLS and 60% meets 60. **A tie suggests none**, because
    /// DAILY MOSQUE BELOW 60% shares as many words with the over 60 template as with the under
    /// one, and one press would send its plots to the wrong targets. A value sharing no word
    /// suggests none either, and the person chooses. **It is a suggestion and nothing reads it
    /// but the button.**
    ///
    /// **A value some file already names is not offered a box**, even where it places nothing
    /// here: a row naming a template this machine does not have, or a value a file gives two
    /// answers. A line added for it would be a second answer and refuse it for everyone. Setup
    /// names that file instead.
    /// </summary>
    public static class UnknownComponents
    {
        /// <summary>
        /// Every value carried by a plot that the table places nowhere, in the order the plots
        /// list them, each value once however its plots spell it.
        /// </summary>
        public static IReadOnlyList<UnknownComponent> In(
            IEnumerable<string> plots, Func<string, string> componentOf, ComponentTable table, IReadOnlyList<KpiTemplate> templates)
        {
            if (componentOf == null) throw new ArgumentNullException("componentOf");
            ComponentTable reading = table ?? ComponentTable.InForce;
            IReadOnlyList<KpiTemplate> offered = templates ?? reading.Templates;

            var order = new List<string>();
            var spelt = new Dictionary<string, string>(StringComparer.Ordinal);
            var carrying = new Dictionary<string, List<string>>(StringComparer.Ordinal);

            foreach (string plot in plots ?? Enumerable.Empty<string>())
            {
                string value = (componentOf(plot) ?? string.Empty).Trim();
                string key = ComponentMatch.Key(value);
                if (key.Length == 0 || reading.Names(value)) continue;

                if (!carrying.ContainsKey(key))
                {
                    order.Add(key);
                    spelt[key] = value;
                    carrying[key] = new List<string>();
                }

                if (!carrying[key].Contains(plot, StringComparer.Ordinal)) carrying[key].Add(plot);
            }

            return order
                .Select(key => new UnknownComponent(spelt[key], carrying[key], Suggest(spelt[key], offered), FolderFor(spelt[key])))
                .ToList();
        }

        /// <summary>
        /// Every value carried by a plot that no exact row holds and a rule places, in the order
        /// the plots list them, each value once however its plots spell it, with what placed it.
        /// Step 2 counts these on one line, so a value placed by its words is seen and not only
        /// trusted.
        /// </summary>
        public static IReadOnlyList<KeyValuePair<string, ComponentPlacement>> PlacedByRule(
            IEnumerable<string> plots, Func<string, string> componentOf, ComponentTable table)
        {
            if (componentOf == null) throw new ArgumentNullException("componentOf");
            ComponentTable reading = table ?? ComponentTable.InForce;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var placed = new List<KeyValuePair<string, ComponentPlacement>>();

            foreach (string plot in plots ?? Enumerable.Empty<string>())
            {
                string value = (componentOf(plot) ?? string.Empty).Trim();
                string key = ComponentMatch.Key(value);
                if (key.Length == 0 || !seen.Add(key)) continue;

                ComponentPlacement how = reading.PlaceOf(value);
                if (how.Rule != null) placed.Add(new KeyValuePair<string, ComponentPlacement>(value, how));
            }

            return placed;
        }

        /// <summary>
        /// The template whose name shares the most words with the value, or null where none shares
        /// one or two share the most.
        /// </summary>
        public static KpiTemplate Suggest(string value, IEnumerable<KpiTemplate> templates)
        {
            HashSet<string> words = Words(value);
            KpiTemplate best = null;
            int most = 0;
            bool tied = false;

            foreach (KpiTemplate one in templates ?? Enumerable.Empty<KpiTemplate>())
            {
                if (one == null) continue;

                int shared = Words(one.Name).Count(words.Contains);
                if (shared > most)
                {
                    best = one;
                    most = shared;
                    tied = false;
                }
                else if (shared == most && shared > 0)
                {
                    tied = true;
                }
            }

            return tied ? null : best;
        }

        /// <summary>The value itself, edge whitespace off and every run of spaces one.</summary>
        public static string FolderFor(string value)
        {
            return string.Join(" ", (value ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        }

        private static HashSet<string> Words(string text)
        {
            // The plural S comes off here only, so SCHOOL meets SCHOOLS for a suggestion and never
            // for a rule.
            return new HashSet<string>(
                ComponentRules.WordsOf(text)
                    .Select(word => word.Length > 1 && word.EndsWith("S", StringComparison.Ordinal) ? word.Substring(0, word.Length - 1) : word),
                StringComparer.Ordinal);
        }
    }
}
