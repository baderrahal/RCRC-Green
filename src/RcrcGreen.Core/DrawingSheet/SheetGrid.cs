using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    public enum SheetCellState
    {
        /// <summary>
        /// The view is in the model. Clicking it selects it.
        /// </summary>
        Exists,

        /// <summary>
        /// No view for this plot and type. Clicking it marks it.
        /// </summary>
        Missing,

        /// <summary>
        /// The user has marked it. Nothing is created this round, the mark is only recorded.
        /// </summary>
        Marked
    }

    public sealed class SheetGridCell
    {
        public SheetGridCell(ViewType viewType, SheetCellState state, long viewId)
        {
            if (viewType == null) throw new ArgumentNullException("viewType");

            ViewType = viewType;
            State = state;
            ViewId = viewId;
        }

        public ViewType ViewType { get; }

        public SheetCellState State { get; }

        /// <summary>
        /// Zero unless the cell holds a view that exists.
        /// </summary>
        public long ViewId { get; }
    }

    public sealed class SheetGridRow
    {
        public SheetGridRow(string plotId, bool hasScopeBox, IReadOnlyList<SheetGridCell> cells)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (cells == null) throw new ArgumentNullException("cells");

            PlotId = plotId;
            HasScopeBox = hasScopeBox;
            Cells = cells;
        }

        public string PlotId { get; }

        /// <summary>
        /// False when no scope box in the model carries this plot's name. The row label is
        /// marked in that case, because a view without one is useless on this project.
        /// </summary>
        public bool HasScopeBox { get; }

        public IReadOnlyList<SheetGridCell> Cells { get; }
    }

    /// <summary>
    /// Plots down the side, view types across the top, one cell each.
    ///
    /// The rows are the range the user picked and nothing else, so a model of 160 plots is
    /// never drawn at once. The columns are the types the user has chosen to look at, which
    /// is why they are handed in rather than worked out from what happens to exist.
    /// </summary>
    public sealed class SheetGrid
    {
        private SheetGrid(IReadOnlyList<ViewType> columns, IReadOnlyList<SheetGridRow> rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public IReadOnlyList<ViewType> Columns { get; }

        public IReadOnlyList<SheetGridRow> Rows { get; }

        public int MissingCount
        {
            get { return Rows.Sum(row => row.Cells.Count(cell => cell.State == SheetCellState.Missing)); }
        }

        /// <summary>
        /// The marked cells this grid shows, and the only marks the run is handed. The panel
        /// remembers a mark on a view type that has since been unticked, so it comes back when
        /// the column does, but a cell that is not drawn cannot be marked and cannot reach the
        /// plan. The MARK header counts this list, so the number and the squares agree.
        /// </summary>
        public IReadOnlyList<PlotViewKey> Marked
        {
            get
            {
                return Rows
                    .SelectMany(row => row.Cells
                        .Where(cell => cell.State == SheetCellState.Marked)
                        .Select(cell => new PlotViewKey(row.PlotId, cell.ViewType)))
                    .ToList();
            }
        }

        public int MarkedCount
        {
            get { return Marked.Count; }
        }

        public static SheetGrid Build(
            IEnumerable<string> plotsInRange,
            IEnumerable<ViewType> columns,
            IEnumerable<PlotViewPresence> present,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<PlotViewKey> marked)
        {
            List<string> rowsWanted = (plotsInRange ?? Enumerable.Empty<string>())
                .Where(plotId => plotId != null)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(plotId => plotId, NaturalOrder.Comparer)
                .ToList();

            List<ViewType> columnsWanted = (columns ?? Enumerable.Empty<ViewType>())
                .Where(column => column != null)
                .Distinct()
                .OrderBy(column => column)
                .ToList();

            var viewIdByCell = new Dictionary<PlotViewKey, long>();
            foreach (PlotViewPresence one in (present ?? Enumerable.Empty<PlotViewPresence>()).Where(one => one != null))
            {
                // A plot can hold two views of the same type. The first found wins, because
                // the cell only needs something to select and the duplicate is a model
                // problem the reports already show.
                if (!viewIdByCell.ContainsKey(one.Where)) viewIdByCell.Add(one.Where, one.ViewId);
            }

            var withScopeBox = new HashSet<string>(
                (plotsWithAScopeBox ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);

            var marks = new HashSet<PlotViewKey>(
                (marked ?? Enumerable.Empty<PlotViewKey>()).Where(mark => mark != null));

            var rows = new List<SheetGridRow>(rowsWanted.Count);
            foreach (string plotId in rowsWanted)
            {
                var cells = new List<SheetGridCell>(columnsWanted.Count);

                foreach (ViewType column in columnsWanted)
                {
                    var cell = new PlotViewKey(plotId, column);

                    long viewId;
                    if (viewIdByCell.TryGetValue(cell, out viewId))
                    {
                        // A view that exists cannot be marked. Marking is a note that one is
                        // wanted, and this one is already there.
                        cells.Add(new SheetGridCell(column, SheetCellState.Exists, viewId));
                        continue;
                    }

                    cells.Add(new SheetGridCell(
                        column,
                        marks.Contains(cell) ? SheetCellState.Marked : SheetCellState.Missing,
                        0));
                }

                rows.Add(new SheetGridRow(plotId, withScopeBox.Contains(plotId), cells));
            }

            return new SheetGrid(columnsWanted, rows);
        }
    }
}
