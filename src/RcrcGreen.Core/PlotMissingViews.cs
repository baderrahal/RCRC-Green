using System;
using System.Collections.Generic;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One plot and everything it lacks, most widely held view type first.
    /// </summary>
    public sealed class PlotMissingViews
    {
        public PlotMissingViews(string plotId, IReadOnlyList<MissingViewType> missing)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (missing == null) throw new ArgumentNullException("missing");

            PlotId = plotId;
            Missing = missing;
        }

        public string PlotId { get; }

        public IReadOnlyList<MissingViewType> Missing { get; }

        public bool IsComplete
        {
            get { return Missing.Count == 0; }
        }
    }
}
