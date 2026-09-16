using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Whether one plot came out of this press ready to send, and every reason it did not.
    /// </summary>
    public sealed class ReadyAnswer
    {
        public ReadyAnswer(string plotId, IEnumerable<string> why)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            Why = (why ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .ToList();
        }

        public string PlotId { get; }

        /// <summary>Every reason this plot is not ready, short, each naming its box or its cell.</summary>
        public IReadOnlyList<string> Why { get; }

        public bool Ready
        {
            get { return Why.Count == 0; }
        }

        /// <summary>
        /// The reasons as one cell of the plot list's row. **Every one of them**, because a row
        /// short of the second reads exactly like a plot that had one.
        /// </summary>
        public string WhyInWords
        {
            get { return string.Join(". ", Why.ToArray()); }
        }
    }

    /// <summary>
    /// **ALL 154 ROWS OF THE PLOT LIST READ YES ON THE 16:37 PRESS**, four columns of YES with an
    /// empty why not, and Bader had the ready list worked out by hand at 33. The four columns
    /// each answer a true and narrow question, whether the plot is in the model, whether it was
    /// ticked, whether a workbook was written and whether a PDF was, and NONE of them is the
    /// question the team is actually asking, which is whether this plot can go to the client.
    ///
    /// That press wrote both files for the plots whose workbooks had replaced each other, for 41
    /// whose Total areas to be greened came out blank, for FP-18 whose PDF and Excel disagree
    /// about the canopy, and for the plots whose workbooks recalculate with a #DIV/0!. Every one
    /// of those read YES four times over.
    ///
    /// **A BOX THIS TOOL HAS NO SOURCE FOR DOES NOT COUNT AGAINST READY.** The forms carry
    /// sidewalks, kiosks, bridges and water tanks, none of which is in the table, and a plot held
    /// back over a box nothing was ever going to fill is a column the team learns to read past.
    /// </summary>
    public static class PlotReady
    {
        public const string Column = "READY";

        public const string NotTicked = "it was not ticked for this press";

        public const string NotInTheModel =
            "the model does not name this plot, so it could not be ticked";

        public const string NoPdfPlanned = "no PDF was planned beside the workbook";

        /// <summary>
        /// One plot's answer, read off what the press RECORDED rather than worked out again. The
        /// trees are handed in because <see cref="TreesNotWritten.Of"/> walks every run and the
        /// caller already has it for the column beside this one.
        /// </summary>
        public static ReadyAnswer For(
            KpiCreateRunSet set,
            string plotId,
            IEnumerable<PlotTreesNotWritten> lost = null,
            bool inTheModel = true,
            bool plotsRead = false)
        {
            if (set == null) throw new ArgumentNullException("set");

            string held = (plotId ?? string.Empty).Trim();
            var why = new List<string>();

            PlotOutcome outcome = set.PlotOutcomes
                .FirstOrDefault(one => string.Equals(one.PlotId, held, StringComparison.Ordinal));

            // **THE SHARED VALUE IS THE WHOLE REASON WHERE IT FIRES**, so the absent workbook and
            // the absent PDF under it are not said again. A row naming three consequences of one
            // cause reads as three faults.
            string shared = SharedUid2.StoppedShort(set.Sharing, held);
            if (shared.Length > 0)
            {
                why.Add(shared);
            }
            else if (outcome == null)
            {
                why.Add(plotsRead && !inTheModel ? NotInTheModel : NotTicked);
            }
            else
            {
                if (!outcome.Written) why.Add(outcome.Why);

                if (outcome.Pdf == null) why.Add(NoPdfPlanned);
                else if (!outcome.Pdf.Written) why.Add(outcome.Pdf.Refusal);
                else
                {
                    foreach (PdfFieldFill blank in outcome.Pdf.Blank)
                    {
                        why.Add("PDF " + blank.FieldName + " blank, " + blank.Why);
                    }
                }
            }

            string trees = TreesNotWritten.For(lost ?? TreesNotWritten.Of(set), held);
            if (trees.Length > 0) why.Add(trees);

            string divisions = Divisions(set, held);
            if (divisions.Length > 0) why.Add(divisions);

            return new ReadyAnswer(held, why);
        }

        /// <summary>
        /// Every #DIV/0! the formula check found in this plot's own workbook, named by its cell.
        ///
        /// **IT IS A REPORT LINE AND NEVER A REFUSAL**, which is unchanged: a plot with no trees
        /// really has no average and the client's own arithmetic is what divides. What is new is
        /// that a workbook holding one does not read as ready to send.
        /// </summary>
        public static string Divisions(KpiCreateRunSet set, string plotId)
        {
            if (set == null) throw new ArgumentNullException("set");

            string held = (plotId ?? string.Empty).Trim();

            var cells = new List<string>();

            foreach (KpiCreateRun run in set.Runs)
            {
                if (run == null || run.Outcome == null) continue;

                bool mine = run.Readings.Any(
                    one => one != null && string.Equals(one.PlotId, held, StringComparison.Ordinal));
                if (!mine) continue;

                foreach (FormulaAtRisk risk in run.Outcome.Formulas.AtRisk)
                {
                    if (!risk.IsDivideByZero) continue;

                    cells.Add(risk.SheetName + " " + risk.Cell);
                }
            }

            return cells.Count == 0 ? string.Empty : "#DIV/0!, " + string.Join(", ", cells.ToArray());
        }

        /// <summary>
        /// The whole plot list's answers, in the order the plots were given.
        /// </summary>
        public static IReadOnlyList<ReadyAnswer> Of(
            KpiCreateRunSet set, IEnumerable<string> plotIds)
        {
            if (set == null) throw new ArgumentNullException("set");

            IReadOnlyList<PlotTreesNotWritten> lost = TreesNotWritten.Of(set);

            return (plotIds ?? Enumerable.Empty<string>())
                .Select(one => For(
                    set, one, lost,
                    set.Plots != null && set.Plots.Holds(one),
                    set.Plots != null))
                .ToList();
        }

        public const string Heading = "THE PLOTS READY TO SEND";

        /// <summary>
        /// The one line the glance carries. **A press where every plot is ready still says so**,
        /// because a line that disappears when there is nothing to report reads the same as one
        /// nobody wrote.
        /// </summary>
        public static string InWords(IEnumerable<ReadyAnswer> answers)
        {
            List<ReadyAnswer> held = (answers ?? Enumerable.Empty<ReadyAnswer>()).ToList();

            if (held.Count == 0)
            {
                return Heading + ": no plot list file was set, so nothing here says how many "
                    + "plots came out ready.";
            }

            int ready = held.Count(one => one.Ready);

            return Heading + ": ready " + ready + " of " + held.Count + ".";
        }
    }
}
