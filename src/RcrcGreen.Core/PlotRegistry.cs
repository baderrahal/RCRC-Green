using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Gathers the plot list from all three places a plot shows up in the model.
    /// The list that matters is the union, not any one source.
    /// </summary>
    public static class PlotRegistry
    {
        /// <summary>
        /// A plot that only has a scope box and some tagged elements has no views at all,
        /// which is exactly the plot the team needs to see. It has to survive into the result.
        /// </summary>
        /// <param name="viewNames">Full view or sheet names. The plot is read off the front.</param>
        /// <param name="scopeBoxNames">Scope box names. A scope box is named with the plot identifier.</param>
        /// <param name="elementPlotIdValues">Values read from the PRX_Plot_ID parameter on elements.</param>
        public static PlotRegistryResult Build(
            IEnumerable<string> viewNames,
            IEnumerable<string> scopeBoxNames,
            IEnumerable<string> elementPlotIdValues)
        {
            var sourcesByPlot = new Dictionary<string, SortedSet<int>>(StringComparer.Ordinal);
            var ignored = new List<IgnoredName>();

            foreach (string name in Safe(viewNames))
            {
                ParsedViewName parsed;
                if (ViewNameParser.TryParse(name, out parsed))
                {
                    Record(sourcesByPlot, parsed.PlotId, PlotSource.ViewName);
                }
                else
                {
                    ignored.Add(new IgnoredName(
                        name,
                        ViewNameParser.FailsOnlyOnPlotCase(name)
                            ? IgnoredReason.WrongCase
                            : IgnoredReason.NotAPlotName));
                }
            }

            AddDirect(sourcesByPlot, ignored, scopeBoxNames, PlotSource.ScopeBox);
            AddDirect(sourcesByPlot, ignored, elementPlotIdValues, PlotSource.ElementParameter);

            List<PlotRecord> plots = sourcesByPlot
                .OrderBy(pair => pair.Key, NaturalOrder.Comparer)
                .Select(pair => new PlotRecord(pair.Key, pair.Value.Select(v => (PlotSource)v)))
                .ToList();

            return new PlotRegistryResult(plots, ignored);
        }

        private static void AddDirect(
            Dictionary<string, SortedSet<int>> sourcesByPlot,
            List<IgnoredName> ignored,
            IEnumerable<string> candidates,
            PlotSource source)
        {
            foreach (string candidate in Safe(candidates))
            {
                string plotId;
                if (PlotId.TryRead(candidate, out plotId))
                {
                    Record(sourcesByPlot, plotId, source);
                }
                else
                {
                    ignored.Add(new IgnoredName(
                        candidate,
                        PlotId.FailsOnlyOnCase(candidate.Trim())
                            ? IgnoredReason.WrongCase
                            : IgnoredReason.NotAPlotName));
                }
            }
        }

        private static void Record(Dictionary<string, SortedSet<int>> sourcesByPlot, string plotId, PlotSource source)
        {
            SortedSet<int> found;
            if (!sourcesByPlot.TryGetValue(plotId, out found))
            {
                found = new SortedSet<int>();
                sourcesByPlot.Add(plotId, found);
            }
            found.Add((int)source);
        }

        private static IEnumerable<string> Safe(IEnumerable<string> values)
        {
            if (values == null) return Enumerable.Empty<string>();
            return values.Where(v => v != null);
        }
    }
}
