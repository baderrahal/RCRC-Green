using System;
using System.Collections.Generic;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The plot list, and the strings that were handed in but do not read as a plot
    /// identifier. The second list exists so a mistyped scope box name or parameter value is
    /// visible instead of quietly vanishing from the count. Plenty of ordinary views are not
    /// named to the pattern at all, so on the view name side this list is normally long.
    /// </summary>
    public sealed class PlotRegistryResult
    {
        public PlotRegistryResult(IReadOnlyList<PlotRecord> plots, IReadOnlyList<IgnoredName> ignored)
        {
            if (plots == null) throw new ArgumentNullException("plots");
            if (ignored == null) throw new ArgumentNullException("ignored");

            Plots = plots;
            Ignored = ignored;
        }

        public IReadOnlyList<PlotRecord> Plots { get; }

        public IReadOnlyList<IgnoredName> Ignored { get; }
    }
}
