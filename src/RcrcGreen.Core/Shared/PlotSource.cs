namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a plot was found. A plot can turn up in more than one of these.
    /// </summary>
    public enum PlotSource
    {
        ViewName,
        ScopeBox,
        ElementParameter,

        /// <summary>
        /// PRX_Plot_ID on a view. It is the first place the grid looks for a view's plot, and
        /// it is its own source here so a plot found only this way still reads as having
        /// views, which a plot found only on model elements does not.
        /// </summary>
        ViewParameter
    }
}
