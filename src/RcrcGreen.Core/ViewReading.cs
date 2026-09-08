using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Turns one view, as plain values, into what it contributes to the grid.
    /// </summary>
    public static class ViewReading
    {
        /// <summary>
        /// A view fills the cell named by its own name and by nothing else.
        ///
        /// The panel used to file a view under the plot in PRX_Plot_ID while taking the view
        /// type from the name. A view named DM-12-(200) General Arrangement Layout carrying
        /// PRX_Plot_ID DM-11 then filled the DM-11 row, so deleting every DM-11 view left that
        /// cell still showing a view, and clicking it opened a view belonging to DM-12. The
        /// grid must never show a view as existing when it is not there, so the cell now comes
        /// from the name and the disagreement is counted instead.
        /// </summary>
        public static ViewOnAPlot Read(string plotIdParameter, string viewName, long viewId)
        {
            ViewPlotReading reading = ViewPlotReader.Read(plotIdParameter, viewName);

            ParsedViewName parsed;
            if (!ViewNameParser.TryParse(viewName, out parsed))
            {
                return new ViewOnAPlot(reading.PlotId, reading.Source, null, false);
            }

            var fills = new PlotViewPresence(parsed.PlotId, parsed.Type, viewId);

            bool disagree = reading.Source == PlotSourceOnView.Parameter
                && !string.Equals(reading.PlotId, parsed.PlotId, StringComparison.Ordinal);

            return new ViewOnAPlot(reading.PlotId, reading.Source, fills, disagree);
        }
    }
}
