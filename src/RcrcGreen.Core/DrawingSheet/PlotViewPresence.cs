using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A view that exists in the model, and which cell of the grid it fills.
    /// </summary>
    public sealed class PlotViewPresence
    {
        public PlotViewPresence(PlotViewKey where, long viewId)
        {
            if (where == null) throw new ArgumentNullException("where");

            Where = where;
            ViewId = viewId;
        }

        public PlotViewPresence(string plotId, ViewType viewType, long viewId)
            : this(new PlotViewKey(plotId, viewType), viewId)
        {
        }

        public PlotViewKey Where { get; }

        /// <summary>
        /// What the Revit side uses to select the view when the cell is clicked.
        /// </summary>
        public long ViewId { get; }
    }
}
