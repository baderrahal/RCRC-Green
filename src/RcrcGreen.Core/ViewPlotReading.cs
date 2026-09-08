using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view's plot, and which of the two places it came from.
    /// </summary>
    public sealed class ViewPlotReading
    {
        public ViewPlotReading(string plotId, PlotSourceOnView source, string rawParameterValue)
        {
            PlotId = plotId ?? string.Empty;
            Source = source;
            RawParameterValue = rawParameterValue ?? string.Empty;
        }

        public string PlotId { get; }

        public PlotSourceOnView Source { get; }

        /// <summary>
        /// What PRX_Plot_ID actually held, kept so a value that is present and wrong can be
        /// shown rather than only counted.
        /// </summary>
        public string RawParameterValue { get; }

        public bool Found
        {
            get { return PlotId.Length > 0; }
        }
    }
}
