using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    public enum SheetCellState
    {
        /// <summary>
        /// The view is in the model and sits on a sheet. Clicking it selects it.
        /// </summary>
        Exists,

        /// <summary>
        /// The view is in the model and sits on NO sheet. Clicking it selects it the same
        /// way, because it is a view like any other, and it is drawn apart because a view
        /// nobody put on a sheet is a different job from a view that is missing.
        ///
        /// On the first real model 2,430 views are on no sheet against 953 that are, so this
        /// is the ordinary state rather than the exception.
        /// </summary>
        ExistsNoSheet,

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
        public SheetGridCell(
            ViewType viewType, SheetCellState state, long viewId, string sheetNumber = null)
        {
            if (viewType == null) throw new ArgumentNullException("viewType");

            ViewType = viewType;
            State = state;
            ViewId = viewId;
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
        }

        public ViewType ViewType { get; }

        public SheetCellState State { get; }

        /// <summary>
        /// Zero unless the cell holds a view that exists.
        /// </summary>
        public long ViewId { get; }

        /// <summary>
        /// The number of the sheet this view sits on, empty when it sits on none.
        ///
        /// It is on the cell because it is what tells somebody at a glance that a sub plot is
        /// running on copy numbers, which every sub plot but DM-11 does on the first real
        /// model. A count of sheets could never say that.
        /// </summary>
        public string SheetNumber { get; }

        /// <summary>
        /// True when the model holds a view for this cell, on a sheet or not. The two states
        /// are one answer to whether the cell can be marked and whether clicking it opens
        /// something, so that question is asked here rather than compared against two states
        /// at every call site.
        /// </summary>
        public bool IsInTheModel
        {
            get
            {
                return State == SheetCellState.Exists || State == SheetCellState.ExistsNoSheet;
            }
        }
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

            var presenceByCell = new Dictionary<PlotViewKey, PlotViewPresence>();
            foreach (PlotViewPresence one in (present ?? Enumerable.Empty<PlotViewPresence>()).Where(one => one != null))
            {
                // A plot can hold two views of the same type. The first found wins, because
                // the cell only needs something to select and the duplicate is a model
                // problem the reports already show.
                if (!presenceByCell.ContainsKey(one.Where)) presenceByCell.Add(one.Where, one);
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

                    PlotViewPresence found;
                    if (presenceByCell.TryGetValue(cell, out found))
                    {
                        // A view that exists cannot be marked. Marking is a note that one is
                        // wanted, and this one is already there.
                        cells.Add(new SheetGridCell(
                            column,
                            found.SheetNumber.Length == 0
                                ? SheetCellState.ExistsNoSheet
                                : SheetCellState.Exists,
                            found.ViewId,
                            found.SheetNumber));
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
