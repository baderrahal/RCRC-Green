namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a view's plot was found.
    ///
    /// The first real model settled the order. 1,269 views were skipped by the scope box run
    /// only because their name does not parse, while PRX_Plot_ID sits on the view itself and
    /// holds the plot directly. The parameter is asked first for that reason. The name is
    /// still read, because the parser was confirmed correct against the same model.
    /// </summary>
    public enum PlotSourceOnView
    {
        /// <summary>
        /// Neither the parameter nor the name gave a plot.
        /// </summary>
        None,

        /// <summary>
        /// PRX_Plot_ID on the view.
        /// </summary>
        Parameter,

        /// <summary>
        /// The view name, read through <see cref="ViewNameParser"/>.
        /// </summary>
        ViewName,

        /// <summary>
        /// The parameter held something, and it is not shaped like a plot identifier. The
        /// name is not tried in that case, because a value that is present and wrong is a
        /// different problem from one that was never filled in, and it needs seeing.
        /// </summary>
        ParameterNotAPlot
    }
}
