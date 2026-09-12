using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A view that exists in the model, and which cell of the grid it fills.
    /// </summary>
    public sealed class PlotViewPresence
    {
        public PlotViewPresence(PlotViewKey where, long viewId, string sheetNumber = null)
        {
            if (where == null) throw new ArgumentNullException("where");

            Where = where;
            ViewId = viewId;
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
        }

        public PlotViewPresence(
            string plotId, ViewType viewType, long viewId, string sheetNumber = null)
            : this(new PlotViewKey(plotId, viewType), viewId, sheetNumber)
        {
        }

        public PlotViewKey Where { get; }

        /// <summary>
        /// What the Revit side uses to select the view when the cell is clicked.
        /// </summary>
        public long ViewId { get; }

        /// <summary>
        /// The number of the sheet this view is placed on, empty when it is on none. A view
        /// can only be on one sheet in Revit, so this is one value rather than a list.
        /// </summary>
        public string SheetNumber { get; }
    }
}
