using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view whose name says it belongs to one plot while PRX_Plot_ID says another.
    ///
    /// The panel has counted these since the round that stopped the grid filing a view under
    /// the wrong plot. A count tells somebody there is a problem and nothing about where it is,
    /// so the scan names them. Nothing here changes one. Which of the two is right is a
    /// question about the project, and the tool does not know the answer.
    /// </summary>
    public sealed class ScannedDisagreement
    {
        public ScannedDisagreement(string viewName, string plotInTheName, string plotInTheParameter)
        {
            if (viewName == null) throw new ArgumentNullException("viewName");
            if (plotInTheName == null) throw new ArgumentNullException("plotInTheName");
            if (plotInTheParameter == null) throw new ArgumentNullException("plotInTheParameter");

            ViewName = viewName;
            PlotInTheName = plotInTheName;
            PlotInTheParameter = plotInTheParameter;
        }

        public string ViewName { get; }

        /// <summary>
        /// What the view is called. The grid follows this one, because a cell filled from a
        /// disagreeing parameter is a claim about a plot that no view backs up.
        /// </summary>
        public string PlotInTheName { get; }

        public string PlotInTheParameter { get; }
    }
}
