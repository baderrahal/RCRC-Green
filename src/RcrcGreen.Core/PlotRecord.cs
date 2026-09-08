using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One plot, and every source it was found through.
    /// </summary>
    public sealed class PlotRecord
    {
        public PlotRecord(string plotId, IEnumerable<PlotSource> sources)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (sources == null) throw new ArgumentNullException("sources");

            PlotId = plotId;
            Sources = sources.Distinct().OrderBy(s => (int)s).ToList();
        }

        public string PlotId { get; }

        public IReadOnlyList<PlotSource> Sources { get; }

        public bool FoundIn(PlotSource source)
        {
            return Sources.Contains(source);
        }

        public override string ToString()
        {
            return PlotId + " [" + string.Join(", ", Sources.Select(s => s.ToString()).ToArray()) + "]";
        }
    }
}
