using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Marking more than one cell at a time.
    ///
    /// 17 plots across 8 view types is 136 clicks, and the first person to use the grid in
    /// anger did exactly that. Every cell that comes back here is one that a single click on
    /// the grid would have marked, so the two can never mean different things.
    ///
    /// **Only missing cells on ticked plots.** A cell holding a view cannot be marked, because
    /// marking says a view is wanted and that one is already there. An unticked plot is out of
    /// the run, so marking it in bulk would leave marks the run then drops without saying so.
    /// A single click on one cell is still allowed anywhere, because that is a deliberate act
    /// on one cell rather than a sweep.
    /// </summary>
    public static class BulkMarking
    {
        public static IReadOnlyList<PlotViewKey> EveryMissing(
            SheetGrid grid, IEnumerable<string> ticked)
        {
            return Missing(grid, ticked, row => true, cell => true);
        }

        /// <summary>
        /// One plot's whole row, which is what clicking the plot name does.
        /// </summary>
        public static IReadOnlyList<PlotViewKey> WholeRow(
            SheetGrid grid, IEnumerable<string> ticked, string plotId)
        {
            if (plotId == null) return new List<PlotViewKey>();

            return Missing(
                grid,
                ticked,
                row => string.Equals(row.PlotId, plotId, StringComparison.Ordinal),
                cell => true);
        }

        /// <summary>
        /// One view type down every plot, which is what clicking the column header does.
        /// </summary>
        public static IReadOnlyList<PlotViewKey> WholeColumn(
            SheetGrid grid, IEnumerable<string> ticked, ViewType type)
        {
            if (type == null) return new List<PlotViewKey>();

            return Missing(grid, ticked, row => true, cell => cell.ViewType.Equals(type));
        }

        /// <summary>
        /// What the status line says after a sweep. The panel formats none of it, the same rule
        /// every other count on the panel follows.
        /// </summary>
        public static string InWords(int added, int markedNow)
        {
            if (added == 0)
            {
                return "Nothing there to mark. Every cell in it is already marked, already holds "
                    + "a view, or belongs to a plot that is not ticked.";
            }

            return Count(added, "cell") + " marked, " + Count(markedNow, "in total")
                + ". Marking records intent and changes nothing until Run.";
        }

        public static string ClearedInWords(int cleared)
        {
            return cleared == 0
                ? "Nothing was marked."
                : Count(cleared, "mark") + " cleared. Nothing in the model has changed.";
        }

        private static IReadOnlyList<PlotViewKey> Missing(
            SheetGrid grid,
            IEnumerable<string> ticked,
            Func<SheetGridRow, bool> wantedRow,
            Func<SheetGridCell, bool> wantedCell)
        {
            if (grid == null) return new List<PlotViewKey>();

            var inTheRun = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>()).Where(one => one != null),
                StringComparer.Ordinal);

            return grid.Rows
                .Where(row => inTheRun.Contains(row.PlotId))
                .Where(wantedRow)
                .SelectMany(row => row.Cells
                    .Where(cell => cell.State == SheetCellState.Missing)
                    .Where(wantedCell)
                    .Select(cell => new PlotViewKey(row.PlotId, cell.ViewType)))
                .ToList();
        }

        private static string Count(int howMany, string thing)
        {
            string word = thing == "in total"
                ? thing
                : thing + (howMany == 1 ? string.Empty : "s");

            return howMany.ToString(CultureInfo.InvariantCulture) + " " + word;
        }
    }
}
