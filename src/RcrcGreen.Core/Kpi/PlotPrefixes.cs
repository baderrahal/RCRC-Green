using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One plot prefix and the template its plots belong to.
    /// </summary>
    public sealed class PlotPrefix
    {
        public PlotPrefix(string prefix, KpiTemplate template)
        {
            if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentNullException("prefix");
            if (template == null) throw new ArgumentNullException("template");

            Prefix = prefix;
            Template = template;
        }

        /// <summary>
        /// The two letters at the front of a plot identifier, as DM-41 carries DM.
        /// </summary>
        public string Prefix { get; }

        public KpiTemplate Template { get; }
    }

    /// <summary>
    /// The table: which template each plot prefix belongs to. Confirmed by the team, all seven
    /// templates and all eight prefixes.
    ///
    /// **THIS IS THE SECOND ROUTE TO A TEMPLATE AND IT DOES NOT DECIDE.** `ComponentTemplates`,
    /// read off PRX_Component, is the one that does. Two records of one fact is the fault this
    /// repo has met eight times, so the two do not get equal standing: the prefix is a cross
    /// check, and where they disagree the pane says so, names both and preselects nothing.
    ///
    /// The two agree prefix by prefix with the eleven component values measured on the 1548
    /// scan, with nothing left over on either side, and a test says so.
    ///
    /// **What the prefix was once for was grouping**, a row of buttons beside Select all and
    /// Clear that ticked a template's plots in one press. The workbook rows do that now, through
    /// <see cref="TickingATemplate"/>, and they ask the split rather than the prefix, so nothing
    /// here gathers plots any more. The table itself is unchanged and is still both routes'
    /// cross check.
    ///
    /// It is also the only thing that can place a plot with no sheet. The 1548 scan found four
    /// on a schedule and on none: EP-05, EP-11, EP-12 and EP-13. No sheet means no
    /// PRX_Component, and the pane says the component could not be read and the prefix was used
    /// rather than preselecting in silence.
    /// </summary>
    public static class PlotPrefixes
    {
        public static readonly IReadOnlyList<PlotPrefix> All = new[]
        {
            new PlotPrefix("NS", KpiTemplates.Streets),
            new PlotPrefix("ST", KpiTemplates.Streets),
            new PlotPrefix("MM", KpiTemplates.Streets),
            new PlotPrefix("PL", KpiTemplates.Parking),
            new PlotPrefix("FM", KpiTemplates.Mosques),
            new PlotPrefix("DM", KpiTemplates.Mosques),
            new PlotPrefix("SC", KpiTemplates.Schools),
            new PlotPrefix("EP", KpiTemplates.ExistingParks),
            new PlotPrefix("FP", KpiTemplates.FutureParks),
            new PlotPrefix("HF", KpiTemplates.Healthcare)
        };

        /// <summary>
        /// The two letters at the front of a plot identifier, or empty when it does not carry
        /// two. `PlotId` already holds what a plot identifier looks like and this reads the
        /// front of one rather than parsing it again.
        /// </summary>
        public static string Of(string plotId)
        {
            if (string.IsNullOrWhiteSpace(plotId)) return string.Empty;

            string held = plotId.Trim();
            if (held.Length < 2) return string.Empty;
            if (!char.IsLetter(held[0]) || !char.IsLetter(held[1])) return string.Empty;

            return held.Substring(0, 2).ToUpperInvariant();
        }

        /// <summary>
        /// The template a plot's prefix belongs to, or null when the table does not hold it.
        /// Nothing guesses: a prefix nobody has confirmed means no answer.
        /// </summary>
        public static KpiTemplate For(string plotId)
        {
            string prefix = Of(plotId);
            if (prefix.Length == 0) return null;

            PlotPrefix found = All.FirstOrDefault(
                one => string.Equals(one.Prefix, prefix, StringComparison.OrdinalIgnoreCase));

            return found == null ? null : found.Template;
        }

        /// <summary>
        /// Every prefix that belongs to one template, in the order the table lists them.
        ///
        /// **Kept because it is the only record of the many to one shape, and nothing calls it.**
        /// Git says it never had a caller outside the tests at all, in any round. What it records
        /// is that three prefixes mean STREETS and two mean MOSQUES while HEALTHCARE has one,
        /// which is what makes the prefix a cross check rather than a key: read the other way,
        /// off <see cref="For"/>, a template is reached one prefix at a time and the many to one
        /// is invisible. One test writes those three lists out by hand and another asserts that
        /// every template is reached by at least one prefix, which is what the agreement with
        /// the component table rests on.
        /// </summary>
        public static IReadOnlyList<string> PrefixesFor(KpiTemplate template)
        {
            if (template == null) return new List<string>();

            return All
                .Where(one => ReferenceEquals(one.Template, template))
                .Select(one => one.Prefix)
                .ToList();
        }

        /// <summary>
        /// Which templates the chosen plots point at by prefix, one entry per template with the
        /// plots that named it, so a disagreement can be shown rather than resolved.
        /// </summary>
        public static IReadOnlyList<TemplateByPrefix> Across(IEnumerable<string> plots)
        {
            var found = new List<TemplateByPrefix>();

            foreach (string plotId in (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one)))
            {
                KpiTemplate template = For(plotId);

                TemplateByPrefix already = found.FirstOrDefault(one =>
                    ReferenceEquals(one.Template, template));

                if (already == null)
                {
                    found.Add(new TemplateByPrefix(template, new List<string> { plotId.Trim() }));
                    continue;
                }

                already.Add(plotId.Trim());
            }

            return found;
        }
    }

    /// <summary>
    /// One template the chosen plots point at by prefix, and which of them named it. A null
    /// template is the plots whose prefix the table does not hold.
    /// </summary>
    public sealed class TemplateByPrefix
    {
        private readonly List<string> _plots;

        public TemplateByPrefix(KpiTemplate template, List<string> plots)
        {
            Template = template;
            _plots = plots ?? new List<string>();
        }

        public KpiTemplate Template { get; }

        public IReadOnlyList<string> Plots
        {
            get { return _plots; }
        }

        public string Name
        {
            get { return Template == null ? "(no prefix this tool knows)" : Template.Name; }
        }

        public void Add(string plotId)
        {
            _plots.Add(plotId);
        }
    }
}
