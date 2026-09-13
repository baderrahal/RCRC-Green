using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Ticking a workbook row ticks the plots that will go into it, and unticking it takes them
    /// off again.
    ///
    /// **The specification this replaces was wrong.** The first press over several templates
    /// ticked MOSQUES, PARKING, SCHOOLS and STREETS and six plots, all of them SC, and three of
    /// the four templates read no ticked plot belongs to them. The tool did exactly what it was
    /// told. A person who ticks MOSQUES has already said which plots they mean, and making them
    /// find a grouping button and press that too is the same fact asked for twice.
    ///
    /// **IT TICKS BY THE SPLIT'S OWN RULE AND NEVER BY THE PREFIX.** The grouping buttons this
    /// replaces gathered plots with <see cref="PlotPrefixes.For"/>, which reads the two letters
    /// at the front of a plot identifier and nothing else, while the split that decides which
    /// workbook a plot really goes into reads PRX_Component first. Two rules for one question is the
    /// fault this repository keeps paying for: a plot ticked by the prefix could then land in no
    /// workbook at all, and the row's count would be a number nothing else agreed with. So this
    /// asks <see cref="PlotsPerTemplate.For"/>, the one rule, and the row's count is what will
    /// really go in.
    ///
    /// **A PLOT TICKED OR UNTICKED BY HAND WINS.** Ticking a template is a starting point rather
    /// than a lock, so a plot the user took off by hand stays off when the template it belongs
    /// to is ticked, and the row then says 19 of its 20 plots rather than 20. The hand list is
    /// what the pane holds and this reads it.
    /// </summary>
    public static class TickingATemplate
    {
        /// <summary>
        /// Every plot the model holds that belongs to one template, by the split's own rule, in
        /// the order the model lists them.
        /// </summary>
        public static IReadOnlyList<string> PlotsOf(
            KpiTemplate template, IEnumerable<string> plots, Func<string, string> componentOf)
        {
            if (componentOf == null) throw new ArgumentNullException("componentOf");
            if (template == null) return new List<string>();

            return (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Where(one => ReferenceEquals(PlotsPerTemplate.For(one, componentOf(one)).Template, template))
                .ToList();
        }

        /// <summary>
        /// The ticks after a template row is ticked: what was ticked already, plus every plot of
        /// that template the user has not held off by hand. It ADDS rather than replaces,
        /// because several templates can be ticked at once now and a replacement would empty the
        /// row ticked before it.
        /// </summary>
        public static PlotTicks Ticked(
            PlotTicks ticks,
            KpiTemplate template,
            Func<string, string> componentOf,
            IEnumerable<string> heldOffByHand)
        {
            if (ticks == null) throw new ArgumentNullException("ticks");

            var off = new HashSet<string>(
                heldOffByHand ?? Enumerable.Empty<string>(), StringComparer.Ordinal);

            PlotTicks next = ticks;
            foreach (string plotId in PlotsOf(template, ticks.Plots.All, componentOf))
            {
                if (off.Contains(plotId)) continue;
                next = next.With(plotId);
            }

            return next;
        }

        /// <summary>
        /// The ticks after a template row is unticked: its plots come off and every other row's
        /// stay. A plot that belongs to no other ticked template simply goes.
        /// </summary>
        public static PlotTicks Unticked(
            PlotTicks ticks, KpiTemplate template, Func<string, string> componentOf)
        {
            if (ticks == null) throw new ArgumentNullException("ticks");

            PlotTicks next = ticks;
            foreach (string plotId in PlotsOf(template, ticks.Plots.All, componentOf))
            {
                next = next.Without(plotId);
            }

            return next;
        }

        /// <summary>
        /// What a row says about how many of its plots are really going in, for a template whose
        /// plots the user has edited by hand. **The count is what will actually go in, never how
        /// many the template could take.**
        /// </summary>
        public static string SomeOfThem(KpiTemplate template, int going, int couldGo)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (going >= couldGo) return string.Empty;

            return template.Name + ": " + going + " of the " + couldGo
                + " plots that belong to it, the rest unticked by hand.";
        }
    }
}
