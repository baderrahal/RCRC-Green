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

        /// <summary>
        /// True when at least one view carries this plot, in its name or in PRX_Plot_ID. A
        /// plot without one is the plot with everything missing, which is the case the team
        /// most needs to see and the case the list used to drop.
        /// </summary>
        public bool HasViews
        {
            get { return FoundIn(PlotSource.ViewName) || FoundIn(PlotSource.ViewParameter); }
        }

        /// <summary>
        /// The suffix on the plot's row when no view carries it. Empty for a plot with views,
        /// so the ordinary rows stay quiet and only the plots that need starting from nothing
        /// say anything.
        /// </summary>
        public string NoViewsInWords()
        {
            if (HasViews) return string.Empty;

            bool box = FoundIn(PlotSource.ScopeBox);
            bool elements = FoundIn(PlotSource.ElementParameter);

            if (box && elements) return "box and elements, no views";
            if (box) return "box only, no views";
            return "elements only, no views";
        }

        /// <summary>
        /// Every source, in the words the tooltip uses.
        /// </summary>
        public string SourcesInWords()
        {
            return string.Join(", ", Sources.Select(Word).ToArray());
        }

        private static string Word(PlotSource source)
        {
            switch (source)
            {
                case PlotSource.ViewName: return "view names";
                case PlotSource.ViewParameter: return "PRX_Plot_ID on views";
                case PlotSource.ScopeBox: return "a scope box";
                default: return "PRX_Plot_ID on elements";
            }
        }

        public override string ToString()
        {
            return PlotId + " [" + string.Join(", ", Sources.Select(s => s.ToString()).ToArray()) + "]";
        }
    }
}
