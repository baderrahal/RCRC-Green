using System;
using System.Collections.Generic;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The plot list, and the strings that were handed in but do not read as a plot
    /// identifier. The second list exists so a mistyped scope box name is visible instead
    /// of quietly vanishing from the count.
    /// </summary>
    public sealed class PlotRegistryResult
    {
        public PlotRegistryResult(IReadOnlyList<PlotRecord> plots, IReadOnlyList<string> ignored)
        {
            if (plots == null) throw new ArgumentNullException("plots");
            if (ignored == null) throw new ArgumentNullException("ignored");

            Plots = plots;
            Ignored = ignored;
        }

        public IReadOnlyList<PlotRecord> Plots { get; }

        public IReadOnlyList<string> Ignored { get; }
    }
}
