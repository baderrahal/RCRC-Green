using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which of the two routes placed a plot, or why neither did. Printed rather than reasoned
    /// about, because a plot that went into a workbook on the strength of a cross check must not
    /// read the same as one its own component named.
    /// </summary>
    public enum TemplateRoute
    {
        /// <summary>Nothing placed it and it belongs to no workbook.</summary>
        Nothing,

        /// <summary>PRX_Component named it, which is the route that decides.</summary>
        Component,

        /// <summary>No component to read, so the plot prefix placed it.</summary>
        Prefix,

        /// <summary>The two routes named different templates, so NEITHER of them decides.</summary>
        Disagree
    }

    /// <summary>
    /// One plot and the template it belongs to, with the route and the words for the report.
    /// </summary>
    public sealed class PlotTemplate
    {
        public PlotTemplate(string plotId, KpiTemplate template, TemplateRoute route, string why)
        {
            if (string.IsNullOrWhiteSpace(plotId)) throw new ArgumentNullException("plotId");

            PlotId = plotId.Trim();
            Template = template;
            Route = route;
            Why = why ?? string.Empty;
        }

        public string PlotId { get; }

        /// <summary>
        /// Null where nothing placed it. Such a plot is read for no workbook and is named in
        /// the report rather than quietly absent.
        /// </summary>
        public KpiTemplate Template { get; }

        public TemplateRoute Route { get; }

        public string Why { get; }

        public bool Placed
        {
            get { return Template != null; }
        }
    }

    /// <summary>
    /// One ticked template and the ticked plots that belong to it.
    /// </summary>
    public sealed class TemplateShare
    {
        public TemplateShare(KpiTemplate template, IReadOnlyList<string> plots, string whyNothing)
        {
            if (template == null) throw new ArgumentNullException("template");

            Template = template;
            Plots = plots ?? new List<string>();
            WhyNothing = whyNothing ?? string.Empty;
        }

        public KpiTemplate Template { get; }

        /// <summary>
        /// The ticked plots this workbook gets, in the order they were ticked. **No plot is in
        /// more than one of these**, and the split checks it rather than trusting it.
        /// </summary>
        public IReadOnlyList<string> Plots { get; }

        public bool WillWrite
        {
            get { return Plots.Count > 0; }
        }

        /// <summary>
        /// Why this template writes nothing, empty where it will write. **A ticked template no
        /// ticked plot belongs to stays ticked and stays listed**, and says this before the
        /// press. Bader's decision: the tool does not hide it and does not untick it for them.
        /// </summary>
        public string WhyNothing { get; }
    }

    /// <summary>
    /// Which ticked plots belong to which ticked template, worked out once for the whole run.
    ///
    /// **A plot has one template and one only.** The rule is the one this file already carries
    /// for preselecting: PRX_Component decides, the plot prefix is a cross check, and where the
    /// two disagree NEITHER of them does. Three things follow, and each of them is a plot the
    /// report names rather than a plot that quietly lands somewhere:
    ///
    /// **A component the table does not hold places nothing.** The route that decides gave an
    /// answer nobody knows, so the cross check does not get to answer in its place. The plot is
    /// named with the value it holds and with what its prefix says, and it goes into no workbook.
    ///
    /// **A plot with no component at all is placed by its prefix**, which is the only thing that
    /// can place one. The 1548 scan found four such plots on a schedule and on no sheet, EP-05,
    /// EP-11, EP-12 and EP-13, and no sheet means no PRX_Component.
    ///
    /// **A plot whose two routes disagree is placed by neither**, and both are named.
    ///
    /// **A PLOT COUNTED INTO TWO WORKBOOKS IS A FAULT.** Every plot resolves to at most one
    /// template here, so it cannot happen by construction, and it is checked anyway: 20 mosque
    /// plots and 78 street plots read once and split two ways is exactly where a plot lands in
    /// both or in neither with every total still looking plausible. A construction that cannot
    /// go wrong is not a check, and the check is what survives the next change to the split.
    /// </summary>
    public static class PlotsPerTemplate
    {
        public const string NoTemplateTicked =
            "no template is ticked, so nothing can be written";

        public static string NoPlotBelongs(KpiTemplate template)
        {
            return "no ticked plot belongs to " + template.Name + ", so it will write nothing. "
                + "Tick a plot of it, or leave it and it stays listed writing nothing.";
        }

        public static string CountedTwice(string plotId, IEnumerable<TemplateShare> shares)
        {
            string[] names = (shares ?? Enumerable.Empty<TemplateShare>())
                .Select(one => one.Template.Name)
                .ToArray();

            return plotId + " was counted into more than one workbook, " + string.Join(" and ", names)
                + ". A plot has one template and one only, so nothing was written.";
        }

        /// <summary>
        /// One plot's answer. The component is what the model holds on that plot's sheets, empty
        /// where no sheet carries one.
        /// </summary>
        public static PlotTemplate For(string plotId, string component)
        {
            string held = (component ?? string.Empty).Trim();
            KpiTemplate byPrefix = PlotPrefixes.For(plotId);
            string prefixSays = byPrefix == null
                ? "its prefix names no template this tool knows"
                : "its prefix says " + byPrefix.Name;

            if (held.Length == 0)
            {
                if (byPrefix == null)
                {
                    return new PlotTemplate(plotId, null, TemplateRoute.Nothing,
                        "no component is on its sheets and " + prefixSays);
                }

                return new PlotTemplate(plotId, byPrefix, TemplateRoute.Prefix,
                    "no component is on its sheets, so the PLOT PREFIX placed it in " + byPrefix.Name);
            }

            KpiTemplate meant = ComponentTemplates.For(held);

            if (meant == null)
            {
                return new PlotTemplate(plotId, null, TemplateRoute.Nothing,
                    held + " " + TemplateForComponent.NotInTheTable
                    + ", so the component placed it nowhere and the prefix does not answer in its "
                    + "place. " + Capitalised(prefixSays));
            }

            if (byPrefix != null && !ReferenceEquals(byPrefix, meant))
            {
                return new PlotTemplate(plotId, null, TemplateRoute.Disagree,
                    held + " means " + meant.Name + " and " + prefixSays
                    + ", so NEITHER placed it");
            }

            return new PlotTemplate(plotId, meant, TemplateRoute.Component,
                held + " placed it in " + meant.Name
                + (byPrefix == null ? string.Empty : ", and the plot prefix agrees"));
        }

        /// <summary>
        /// The whole split. One share per ticked template in the order
        /// <see cref="KpiTemplates.All"/> lists them, whether or not any plot belongs to it, and
        /// every ticked plot that belongs to none of them named with its reason.
        /// </summary>
        public static TemplateSplit Split(
            IEnumerable<string> ticked,
            Func<string, string> componentOf,
            IEnumerable<KpiTemplate> tickedTemplates)
        {
            if (componentOf == null) throw new ArgumentNullException("componentOf");

            List<string> plots = (ticked ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            List<KpiTemplate> wanted = KpiTemplates.All
                .Where(template => (tickedTemplates ?? Enumerable.Empty<KpiTemplate>())
                    .Any(one => ReferenceEquals(one, template)))
                .ToList();

            var answers = plots.Select(one => For(one, componentOf(one))).ToList();
            var shares = new List<TemplateShare>();

            foreach (KpiTemplate template in wanted)
            {
                List<string> mine = answers
                    .Where(one => ReferenceEquals(one.Template, template))
                    .Select(one => one.PlotId)
                    .ToList();

                shares.Add(new TemplateShare(template, mine,
                    mine.Count == 0 ? NoPlotBelongs(template) : string.Empty));
            }

            List<PlotTemplate> unplaced = answers
                .Where(one => !one.Placed || !wanted.Any(template => ReferenceEquals(one.Template, template)))
                .Select(one => one.Placed
                    ? new PlotTemplate(one.PlotId, null, one.Route,
                        one.Why + ", and " + one.Template.Name + " is not ticked, so it was not read")
                    : one)
                .ToList();

            return new TemplateSplit(shares, answers, unplaced);
        }

        private static string Capitalised(string line)
        {
            return line.Length == 0 ? line : char.ToUpperInvariant(line[0]) + line.Substring(1);
        }
    }

    /// <summary>
    /// What the split came to, with its own refusals. It is the run level accounting that sits
    /// above each template's own reconciliation.
    /// </summary>
    public sealed class TemplateSplit
    {
        public TemplateSplit(
            IReadOnlyList<TemplateShare> shares,
            IReadOnlyList<PlotTemplate> answers,
            IReadOnlyList<PlotTemplate> unplaced)
        {
            Shares = shares ?? new List<TemplateShare>();
            Answers = answers ?? new List<PlotTemplate>();
            Unplaced = unplaced ?? new List<PlotTemplate>();

            var refusals = new List<string>();
            var twice = new List<string>();

            // Asked of the split as it really came out rather than of the rule that built it.
            foreach (string plotId in Shares.SelectMany(one => one.Plots).Distinct(StringComparer.Ordinal))
            {
                string held = plotId;
                List<TemplateShare> holding = Shares
                    .Where(one => one.Plots.Contains(held, StringComparer.Ordinal))
                    .ToList();

                if (holding.Count <= 1) continue;

                twice.Add(held);
                refusals.Add(PlotsPerTemplate.CountedTwice(held, holding));
            }

            CountedTwice = twice;
            Refusals = refusals;
        }

        public IReadOnlyList<TemplateShare> Shares { get; }

        /// <summary>
        /// Every ticked plot's answer, placed or not, so the report can print the route each one
        /// took beside its template.
        /// </summary>
        public IReadOnlyList<PlotTemplate> Answers { get; }

        /// <summary>
        /// The ticked plots that went into no workbook, each with why. **A plot whose template
        /// is not ticked is not read and is named here**, and so is one no route could place.
        /// </summary>
        public IReadOnlyList<PlotTemplate> Unplaced { get; }

        public IReadOnlyList<string> CountedTwice { get; }

        public IReadOnlyList<string> Refusals { get; }

        /// <summary>
        /// False stops the whole run rather than one workbook, because a plot in two workbooks
        /// is a double count and a double count nobody sees is the worst thing this tool can
        /// produce.
        /// </summary>
        public bool AddsUp
        {
            get { return Refusals.Count == 0; }
        }

        public int TemplatesTicked
        {
            get { return Shares.Count; }
        }

        public int TemplatesWithPlots
        {
            get { return Shares.Count(one => one.WillWrite); }
        }

        public int TemplatesWithNothing
        {
            get { return Shares.Count(one => !one.WillWrite); }
        }

        public IReadOnlyList<TemplateShare> Writing
        {
            get { return Shares.Where(one => one.WillWrite).ToList(); }
        }
    }
}
