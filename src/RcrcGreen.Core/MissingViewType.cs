using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A view type one plot does not have, with the number of plots that do have it.
    /// </summary>
    public sealed class MissingViewType
    {
        public MissingViewType(ViewType viewType, int plotsHoldingType)
        {
            if (viewType == null) throw new ArgumentNullException("viewType");

            ViewType = viewType;
            PlotsHoldingType = plotsHoldingType;
        }

        public ViewType ViewType { get; }

        public int PlotsHoldingType { get; }

        public override string ToString()
        {
            return ViewType + " on " + PlotsHoldingType + " plots";
        }
    }
}
