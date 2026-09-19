using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One row of THE PLOT LIST: one plot the team sent, and what this press did with it.
    ///
    /// **IT CARRIES FACTS AND NOT PRINTED CELLS.** The report writes YES, NO and NOT READ into
    /// its columns and the pane writes its own words, and both read these. A row holding the
    /// report's spelling would make the pane print the report's table into a pane 300 pixels
    /// wide, and a row holding the pane's would put the pane's wording in the client's file.
    /// </summary>
    public sealed class PlotListRow
    {
        internal PlotListRow(
            string plotId,
            string uid2,
            bool plotsRead,
            bool inTheModel,
            bool ticked,
            bool written,
            bool pdf,
            bool ready,
            string trees,
            string whyNot)
        {
            PlotId = plotId ?? string.Empty;
            Uid2 = uid2 ?? string.Empty;
            PlotsRead = plotsRead;
            InTheModel = inTheModel;
            Ticked = ticked;
            Written = written;
            Pdf = pdf;
            Ready = ready;
            Trees = trees ?? string.Empty;
            WhyNot = whyNot ?? string.Empty;
        }

        public string PlotId { get; }

        /// <summary>
        /// This plot's PRX_Plot_UID2, off the path the press built for it, which is the name of
        /// its folder and of its file. **Empty where the path was refused**, which is a plot
        /// whose UID2 was never read, and the row says so rather than reading as a plot whose
        /// value happens to be blank.
        /// </summary>
        public string Uid2 { get; }

        /// <summary>
        /// Whether the press read the model's own plot list at all. A press that did not read it
        /// cannot say whether a plot is in the model, which is a different answer from NO.
        /// </summary>
        public bool PlotsRead { get; }

        public bool InTheModel { get; }

        /// <summary>
        /// Whether this plot was ticked for the press, read off whether it has an outcome at
        /// all rather than off the ticks, because the outcome is what really happened.
        /// </summary>
        public bool Ticked { get; }

        public bool Written { get; }

        /// <summary>Whether a PDF was written beside the workbook.</summary>
        public bool Pdf { get; }

        /// <summary>
        /// The question the team is actually asking, off <see cref="PlotReady.For"/> and never
        /// worked out again here.
        /// </summary>
        public bool Ready { get; }

        /// <summary>
        /// How many of this plot's trees reached no row, in words, empty where none did.
        /// </summary>
        public string Trees { get; }

        /// <summary>
        /// Every reason this plot is not ready, joined, empty where it is ready.
        /// </summary>
        public string WhyNot { get; }

        /// <summary>
        /// What the pane shows for one plot that is not ready: the plot, its UID2 and its
        /// reason, in the words the plot list already uses.
        ///
        /// **THE REASON IS THE ONE THE REPORT PRINTS, WORD FOR WORD**, because a pane and a
        /// report that say a plot failed for two different reasons send somebody looking for a
        /// second fault that is not there.
        /// </summary>
        public string InWords
        {
            get
            {
                var cells = new List<string> { PlotId };

                // **AN ABSENCE AND AN ANSWER NOBODY LOOKED FOR ARE TWO DIFFERENT THINGS.** A
                // plot nobody ticked has no path, so it has no UID2 here, and saying it holds
                // none is a sentence about the MODEL that nothing measured. Only a plot the
                // press really tried to file says so.
                if (Uid2.Length > 0) cells.Add(Uid2);
                else if (Ticked) cells.Add(NoUid2);

                if (WhyNot.Length > 0) cells.Add(WhyNot);

                return string.Join(" | ", cells.ToArray());
            }
        }

        public const string NoUid2 = "no " + KpiNames.PlotUid2;
    }

    /// <summary>
    /// THE PLOT LIST, one row per plot the team's own file names, in the file's own order.
    ///
    /// **THE REPORT AND THE PANE READ THESE SAME ROWS.** The report built them inline for
    /// nine rounds, which was right while the report was the only thing printing them. A pane
    /// that shows how many plots are ready needs the same answer, and a second loop working it
    /// out beside the button is two records of one fact, the shape this repo has met eight
    /// times and pays for every time.
    /// </summary>
    public static class PlotListRows
    {
        /// <summary>
        /// Every row, in the team's list's order. **Empty where no plot list file was set or
        /// the file could not be read**, because a row about a plot nobody listed is a row
        /// about nothing, and the two cases each have their own line elsewhere.
        /// </summary>
        public static IReadOnlyList<PlotListRow> Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            PlotListRead list = set.PlotList;
            if (list == null || !list.Set || !list.Read) return new List<PlotListRow>();

            var byPlot = set.PlotOutcomes.ToList();

            // Walked once for the whole press rather than once per plot, because it reads every
            // run's own matches and the list runs to 154 plots.
            IReadOnlyList<PlotTreesNotWritten> lost = TreesNotWritten.Of(set);

            var rows = new List<PlotListRow>();

            foreach (string plotId in list.Plots)
            {
                string held = plotId;

                PlotOutcome outcome = byPlot.FirstOrDefault(
                    one => string.Equals(one.PlotId, held, StringComparison.Ordinal));

                bool inTheModel = set.Plots != null && set.Plots.Holds(held);
                bool written = outcome != null && outcome.Written;
                bool pdf = outcome != null && outcome.Pdf != null && outcome.Pdf.Written;

                ReadyAnswer can = PlotReady.For(set, held, lost, inTheModel, set.Plots != null);

                rows.Add(new PlotListRow(
                    held,
                    outcome == null ? string.Empty : outcome.Where.Uid2,
                    set.Plots != null,
                    inTheModel,
                    outcome != null,
                    written,
                    pdf,
                    can.Ready,
                    TreesNotWritten.For(lost, held),
                    can.WhyInWords));
            }

            return rows;
        }
    }
}
