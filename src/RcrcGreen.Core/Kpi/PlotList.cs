using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every plot the model holds, from the two places that name one, with the disagreement
    /// between them kept rather than resolved.
    ///
    /// PRX_Plot_ID on the sheets and the PRX_Ref Plot ID filter value on the schedules are two
    /// records of one fact, which is the shape that has been the fault six times in this repo.
    /// This is a seventh place they could part, so both lists are kept, the union is what the
    /// user picks from, and a plot that only one of them names is shown as such. Nothing here
    /// picks a winner and nothing drops a plot for being in only one list.
    /// </summary>
    public sealed class PlotsInTheModel
    {
        private PlotsInTheModel(
            IReadOnlyList<string> all,
            IReadOnlyList<string> onSheetsOnly,
            IReadOnlyList<string> onSchedulesOnly)
        {
            All = all;
            OnSheetsOnly = onSheetsOnly;
            OnSchedulesOnly = onSchedulesOnly;
        }

        /// <summary>
        /// The union of both lists, sorted so DM-2 comes before DM-100. This is what the picker
        /// offers, because a plot named by either source is a plot the model holds.
        /// </summary>
        public IReadOnlyList<string> All { get; }

        public IReadOnlyList<string> OnSheetsOnly { get; }

        public IReadOnlyList<string> OnSchedulesOnly { get; }

        public bool Agree
        {
            get { return OnSheetsOnly.Count == 0 && OnSchedulesOnly.Count == 0; }
        }

        public bool Holds(string plotId)
        {
            return plotId != null
                && All.Any(one => string.Equals(one, plotId, StringComparison.Ordinal));
        }

        public static PlotsInTheModel Of(IEnumerable<string> fromSheets, IEnumerable<string> fromSchedules)
        {
            List<string> sheets = Cleaned(fromSheets);
            List<string> schedules = Cleaned(fromSchedules);

            var all = sheets
                .Concat(schedules)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();

            return new PlotsInTheModel(
                all,
                Missing(sheets, schedules),
                Missing(schedules, sheets));
        }

        private static IReadOnlyList<string> Missing(List<string> from, List<string> against)
        {
            var other = new HashSet<string>(against, StringComparer.Ordinal);

            return from
                .Where(one => !other.Contains(one))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();
        }

        private static List<string> Cleaned(IEnumerable<string> plots)
        {
            return (plots ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .ToList();
        }
    }

    /// <summary>
    /// Which plots the user has ticked, held as the one record of that choice.
    ///
    /// Every plot here came out of the model. Nothing can add one, there is no free text box,
    /// and a plot the model does not hold cannot be ticked, the same rule the Drawing Sheet
    /// follows: inventing a view type is allowed and inventing a plot is not.
    ///
    /// **Which plots belong to one checklist is not written down anywhere.** Nothing here
    /// decides it by the plot prefix, by the component or by anything else. The user ticks them,
    /// and OnlyFor is one press that ticks a whole prefix group rather than a rule that picks
    /// one: what Create adds up is still exactly what is ticked when it is pressed.
    /// </summary>
    public sealed class PlotTicks
    {
        private readonly HashSet<string> _ticked;

        public PlotTicks(PlotsInTheModel plots, IEnumerable<string> ticked = null)
        {
            if (plots == null) throw new ArgumentNullException("plots");

            Plots = plots;
            _ticked = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>()).Where(plots.Holds),
                StringComparer.Ordinal);
        }

        public PlotsInTheModel Plots { get; }

        public IReadOnlyList<string> Ticked
        {
            get
            {
                return Plots.All.Where(one => _ticked.Contains(one)).ToList();
            }
        }

        public int Count
        {
            get { return _ticked.Count; }
        }

        public bool IsTicked(string plotId)
        {
            return plotId != null && _ticked.Contains(plotId);
        }

        public PlotTicks With(string plotId)
        {
            if (!Plots.Holds(plotId)) return this;

            var next = new HashSet<string>(_ticked, StringComparer.Ordinal) { plotId };
            return new PlotTicks(Plots, next);
        }

        public PlotTicks Without(string plotId)
        {
            var next = new HashSet<string>(_ticked, StringComparer.Ordinal);
            next.Remove(plotId ?? string.Empty);
            return new PlotTicks(Plots, next);
        }

        public PlotTicks Toggled(string plotId)
        {
            return IsTicked(plotId) ? Without(plotId) : With(plotId);
        }

        public PlotTicks All()
        {
            return new PlotTicks(Plots, Plots.All);
        }

        /// <summary>
        /// Every plot of one template's prefixes ticked and nothing else, which is what a
        /// grouping button does. It REPLACES the ticks rather than adding to them, because one
        /// checklist is one template and a group added to what was already ticked would mix two.
        ///
        /// A plot whose prefix the table does not hold is reached by no group and is ticked by
        /// hand, which is why the pane names those separately.
        /// </summary>
        public PlotTicks OnlyFor(KpiTemplate template)
        {
            return new PlotTicks(Plots, PlotPrefixes.PlotsFor(Plots.All, template));
        }

        public PlotTicks None()
        {
            return new PlotTicks(Plots, null);
        }

        /// <summary>
        /// The count the pane shows at all times, so nobody presses Create wondering how many
        /// plots are about to be added together.
        /// </summary>
        public string InWords
        {
            get
            {
                if (Plots.All.Count == 0) return "No plot in this model.";

                return Count + " of " + Plots.All.Count + (Plots.All.Count == 1 ? " plot" : " plots")
                    + " ticked.";
            }
        }
    }
}
