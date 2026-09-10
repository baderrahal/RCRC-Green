namespace RcrcGreen.Core
{
    /// <summary>
    /// Everything one view contributes to the grid, worked out in one place.
    ///
    /// Two different questions get two different answers here, which is the whole reason this
    /// type exists. Which plot a view belongs to is answered by PRX_Plot_ID first, because on
    /// the real model 1,269 views have a plot in the parameter and a name that does not parse.
    /// Which cell a view fills is answered by the name alone, because the name is the only
    /// thing that carries a view type, and because a cell filled from a disagreeing parameter
    /// is a claim about a plot that no view in the model backs up.
    /// </summary>
    public sealed class ViewOnAPlot
    {
        internal ViewOnAPlot(
            string plotId,
            PlotSourceOnView source,
            PlotViewPresence fills,
            bool sourcesDisagree,
            string rawParameterValue)
        {
            PlotId = plotId ?? string.Empty;
            Source = source;
            Fills = fills;
            SourcesDisagree = sourcesDisagree;
            RawParameterValue = rawParameterValue ?? string.Empty;
        }

        /// <summary>
        /// The plot this view says it belongs to, empty when neither source gave one. This
        /// goes into the list of plots the model holds.
        /// </summary>
        public string PlotId { get; }

        public PlotSourceOnView Source { get; }

        /// <summary>
        /// What PRX_Plot_ID held, so a value that is present and not a plot can be shown on
        /// the status line rather than only counted. Empty when the parameter gave nothing.
        /// </summary>
        public string RawParameterValue { get; }

        /// <summary>
        /// The cell this view fills, or null when its name does not parse and so carries no
        /// view type. The plot on it always comes from the name, never from the parameter.
        /// </summary>
        public PlotViewPresence Fills { get; }

        /// <summary>
        /// True when the name parses to one plot and PRX_Plot_ID holds a different one. Worth
        /// counting rather than hiding, because it is a model problem and because it is what
        /// used to put a view under the wrong plot.
        /// </summary>
        public bool SourcesDisagree { get; }

        public bool Found
        {
            get { return PlotId.Length > 0; }
        }
    }
}
