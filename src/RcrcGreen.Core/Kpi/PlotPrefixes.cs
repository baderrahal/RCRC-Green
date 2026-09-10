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
    /// **What the prefix is really for is grouping.** It lets the user tick every plot for one
    /// template in one action, which is what the picker offers beside Select all and Clear.
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
        /// Every prefix that belongs to one template, in the order the table lists them, so the
        /// pane can say which prefixes a grouping button gathers.
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
        /// Every plot in the list whose prefix belongs to one template, in the order they came,
        /// for the grouping buttons beside Select all and Clear.
        /// </summary>
        public static IReadOnlyList<string> PlotsFor(IEnumerable<string> plots, KpiTemplate template)
        {
            if (template == null) return new List<string>();

            return (plots ?? Enumerable.Empty<string>())
                .Where(one => ReferenceEquals(For(one), template))
                .ToList();
        }

        /// <summary>
        /// One entry per template the model's plots point at, in the order KpiTemplates lists
        /// them, for the grouping buttons beside Select all and Clear. A template no plot points
        /// at is left out rather than offered as a button that would tick nothing, and the plots
        /// whose prefix the table does not hold are not in here at all: WithNoKnownPrefix is
        /// what names those, because a plot no button reaches has to be visible.
        /// </summary>
        public static IReadOnlyList<TemplateByPrefix> Grouped(IEnumerable<string> plots)
        {
            IReadOnlyList<TemplateByPrefix> across = Across(plots);

            return KpiTemplates.All
                .Select(template => across.FirstOrDefault(one => ReferenceEquals(one.Template, template)))
                .Where(one => one != null)
                .ToList();
        }

        /// <summary>
        /// The plots the table has no prefix for, in the order they came. They are named on the
        /// pane rather than left out in silence, because a plot no grouping button reaches is
        /// one somebody would tick by hand and never think to look for.
        /// </summary>
        public static IReadOnlyList<string> WithNoKnownPrefix(IEnumerable<string> plots)
        {
            TemplateByPrefix unknown = Across(plots).FirstOrDefault(one => one.Template == null);

            return unknown == null ? new List<string>() : unknown.Plots;
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
