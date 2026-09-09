using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One plot and why it gave nothing. A plot that contributed nothing is named, never
    /// quietly absent, because a plot list that goes in longer than it comes out is the
    /// failure the accounting exists to catch.
    /// </summary>
    public sealed class PlotAndReason
    {
        public PlotAndReason(string plotId, string reason)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            Reason = reason ?? string.Empty;
        }

        public string PlotId { get; }

        public string Reason { get; }
    }

    /// <summary>
    /// Every plot chosen, accounted for on the way out, with the reasons a write is refused.
    ///
    /// Three things can refuse it. A plot that was ticked and not read, because the tool would
    /// then be adding up fewer plots than the user chose. A total that does not equal the per
    /// plot numbers printed beside it, because then the arithmetic on the page is not the
    /// arithmetic in the file. And two plots reporting an identical area with nobody having
    /// confirmed it, because that is either two plots of one size or one region counted twice
    /// and the tool cannot tell which.
    ///
    /// It refuses. It does not write a total with a note attached.
    /// </summary>
    public sealed class Reconciliation
    {
        public const string NoSoftscape = "no softscape schedule";

        public const string NoShrubsAndLawn = "no shrubs and lawn schedule";

        public const string NoArea = "no filled region holding an area";

        public const string ChooseTheRegion =
            "more than one of its filled regions holds an area and none has been picked";

        private Reconciliation(
            IReadOnlyList<string> ticked,
            IReadOnlyList<string> read,
            IReadOnlyList<string> withoutSoftscape,
            IReadOnlyList<string> withoutShrubsAndLawn,
            IReadOnlyList<string> withoutArea,
            IReadOnlyList<PlotAndReason> contributedNothing,
            IReadOnlyList<IdenticalArea> identicalAreas,
            IReadOnlyList<string> refusals)
        {
            Ticked = ticked;
            Read = read;
            WithoutSoftscape = withoutSoftscape;
            WithoutShrubsAndLawn = withoutShrubsAndLawn;
            WithoutArea = withoutArea;
            ContributedNothing = contributedNothing;
            IdenticalAreas = identicalAreas;
            Refusals = refusals;
        }

        public IReadOnlyList<string> Ticked { get; }

        public IReadOnlyList<string> Read { get; }

        public IReadOnlyList<string> WithoutSoftscape { get; }

        public IReadOnlyList<string> WithoutShrubsAndLawn { get; }

        public IReadOnlyList<string> WithoutArea { get; }

        public IReadOnlyList<PlotAndReason> ContributedNothing { get; }

        public IReadOnlyList<IdenticalArea> IdenticalAreas { get; }

        /// <summary>
        /// Empty when the write may go ahead. Every line is a reason it may not.
        /// </summary>
        public IReadOnlyList<string> Refusals { get; }

        public bool AddsUp
        {
            get { return Refusals.Count == 0; }
        }

        public static Reconciliation Of(
            IEnumerable<string> ticked,
            IEnumerable<PlotReading> readings,
            IEnumerable<Totalled> totals,
            bool identicalAreasConfirmed)
        {
            List<string> wanted = (ticked ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            List<PlotReading> held = (readings ?? Enumerable.Empty<PlotReading>())
                .Where(one => one != null)
                .ToList();

            List<string> read = held.Select(one => one.PlotId).ToList();
            var refusals = new List<string>();

            List<string> missing = wanted
                .Where(one => !read.Contains(one, StringComparer.Ordinal))
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();

            if (missing.Count > 0)
            {
                refusals.Add(wanted.Count + " plots were ticked and " + read.Count
                    + " were read. Not read: " + string.Join(", ", missing.ToArray()) + ".");
            }

            List<string> extra = read
                .Where(one => !wanted.Contains(one, StringComparer.Ordinal))
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();

            if (extra.Count > 0)
            {
                refusals.Add("Plots were read that were not ticked: "
                    + string.Join(", ", extra.ToArray()) + ".");
            }

            int which = 0;
            foreach (Totalled total in (totals ?? Enumerable.Empty<Totalled>()).Where(one => one != null))
            {
                which++;
                if (total.Adds) continue;

                refusals.Add("A total does not equal the plot numbers printed beside it. Total "
                    + which + " reads " + total.Total + " and its parts add to " + total.Sum + ".");
            }

            // More than one region holding an area is the plot asking a question the type name
            // cannot answer, because which of a plot's two regions carries it varies by plot.
            // The user picks and presses Create again, the same way an identical area is
            // confirmed. Nothing here picks the larger, the first or the cadastral one.
            foreach (PlotReading reading in held
                .Where(one => one.ChosenRegion == null && one.RegionsHoldingAnArea.Count > 1)
                .OrderBy(one => one.PlotId, NaturalOrder.Comparer))
            {
                refusals.Add(reading.PlotId + " has " + reading.RegionsHoldingAnArea.Count
                    + " filled regions holding an area, "
                    + string.Join(", ", reading.RegionsHoldingAnArea
                        .Select(one => one.TypeName + " " + one.Printed).ToArray())
                    + ". Pick the one that is its intervention area.");
            }

            IReadOnlyList<IdenticalArea> identical = KpiMerge.IdenticalAreas(held);
            if (identical.Count > 0 && !identicalAreasConfirmed)
            {
                foreach (IdenticalArea shared in identical)
                {
                    refusals.Add("These plots report the same area, "
                        + string.Join(" and ", shared.Plots.ToArray())
                        + " both reading " + shared.Printed
                        + ". Either they are the same size or one region is counted twice. "
                        + "Confirm before writing.");
                }
            }

            return new Reconciliation(
                wanted.OrderBy(one => one, NaturalOrder.Comparer).ToList(),
                read.OrderBy(one => one, NaturalOrder.Comparer).ToList(),
                Named(held, one => !one.SoftscapeRead),
                Named(held, one => !one.ShrubsAndLawnRead),
                Named(held, one => one.ChosenRegion == null || !one.ChosenRegion.HoldsAnArea),
                Nothing(held),
                identical,
                refusals);
        }

        private static IReadOnlyList<string> Named(
            List<PlotReading> readings, Func<PlotReading, bool> matching)
        {
            return readings
                .Where(matching)
                .Select(one => one.PlotId)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();
        }

        private static IReadOnlyList<PlotAndReason> Nothing(List<PlotReading> readings)
        {
            var found = new List<PlotAndReason>();

            foreach (PlotReading reading in readings.OrderBy(one => one.PlotId, NaturalOrder.Comparer))
            {
                bool trees = reading.SoftscapeRead && reading.Species.Count > 0;
                bool ground = reading.ShrubsAndLawnRead && reading.Subtotals.Count > 0;
                bool area = reading.ChosenRegion != null && reading.ChosenRegion.HoldsAnArea;
                if (trees || ground || area) continue;

                var why = new List<string>();
                if (!reading.SoftscapeRead) why.Add(NoSoftscape);
                else if (reading.Species.Count == 0) why.Add("its softscape schedule listed no species");
                if (!reading.ShrubsAndLawnRead) why.Add(NoShrubsAndLawn);
                else if (reading.Subtotals.Count == 0) why.Add("its shrubs and lawn schedule held neither group");
                if (!area) why.Add(NoArea);

                found.Add(new PlotAndReason(reading.PlotId, string.Join(", ", why.ToArray())));
            }

            return found;
        }
    }
}
