namespace RcrcGreen.Core
{
    /// <summary>
    /// Works out which plot a view belongs to.
    /// </summary>
    public static class ViewPlotReader
    {
        /// <summary>
        /// The parameter wins when it holds anything at all. The name is only read when
        /// PRX_Plot_ID is empty or absent on that view.
        /// </summary>
        /// <param name="plotIdParameter">PRX_Plot_ID from the view, null when the view does not carry it.</param>
        /// <param name="viewName">The view name, read only when the parameter gave nothing.</param>
        public static ViewPlotReading Read(string plotIdParameter, string viewName)
        {
            string raw = plotIdParameter == null ? string.Empty : plotIdParameter.Trim();

            if (raw.Length > 0)
            {
                string fromParameter;
                if (PlotId.TryRead(raw, out fromParameter))
                {
                    return new ViewPlotReading(fromParameter, PlotSourceOnView.Parameter, raw);
                }

                return new ViewPlotReading(string.Empty, PlotSourceOnView.ParameterNotAPlot, raw);
            }

            ParsedViewName parsed;
            if (ViewNameParser.TryParse(viewName, out parsed))
            {
                return new ViewPlotReading(parsed.PlotId, PlotSourceOnView.ViewName, string.Empty);
            }

            return new ViewPlotReading(string.Empty, PlotSourceOnView.None, string.Empty);
        }
    }
}
