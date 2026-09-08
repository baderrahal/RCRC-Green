using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One reading of the model, as plain values, held by the panel between refreshes.
    ///
    /// The panel keeps this and no Revit object at all. That is what lets it stay open while
    /// the user closes one document and opens another, because nothing in it goes stale in a
    /// way that can throw.
    /// </summary>
    public sealed class DrawingSheetSnapshot
    {
        public static readonly DrawingSheetSnapshot Nothing = new DrawingSheetSnapshot(
            string.Empty, null, null, null, null, 0, 0, 0, 0);

        public DrawingSheetSnapshot(
            string documentTitle,
            IEnumerable<string> plotIds,
            IEnumerable<ViewType> viewTypes,
            IEnumerable<PlotViewPresence> present,
            IEnumerable<string> plotsWithAScopeBox,
            int viewsRead,
            int fromParameter,
            int fromViewName,
            int withNoPlot)
        {
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            DocumentTitle = documentTitle;

            PlotIds = Clean(plotIds)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(plotId => plotId, NaturalOrder.Comparer)
                .ToList();

            ViewTypes = (viewTypes ?? Enumerable.Empty<ViewType>())
                .Where(viewType => viewType != null)
                .Distinct()
                .OrderBy(viewType => viewType)
                .ToList();

            Present = (present ?? Enumerable.Empty<PlotViewPresence>())
                .Where(one => one != null)
                .ToList();

            PlotsWithAScopeBox = Clean(plotsWithAScopeBox)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(plotId => plotId, NaturalOrder.Comparer)
                .ToList();

            ViewsRead = viewsRead;
            FromParameter = fromParameter;
            FromViewName = fromViewName;
            WithNoPlot = withNoPlot;
        }

        public string DocumentTitle { get; }

        /// <summary>
        /// Every plot the model actually holds. The panel offers nothing outside this list,
        /// which is how it can never act on a plot that does not exist.
        /// </summary>
        public IReadOnlyList<string> PlotIds { get; }

        public IReadOnlyList<ViewType> ViewTypes { get; }

        public IReadOnlyList<PlotViewPresence> Present { get; }

        public IReadOnlyList<string> PlotsWithAScopeBox { get; }

        public int ViewsRead { get; }

        /// <summary>
        /// How many views found their plot through PRX_Plot_ID, and how many through the name.
        /// Both are shown, because the split is the reason the parameter is read first.
        /// </summary>
        public int FromParameter { get; }

        public int FromViewName { get; }

        public int WithNoPlot { get; }

        public bool Empty
        {
            get { return PlotIds.Count == 0; }
        }

        private static IEnumerable<string> Clean(IEnumerable<string> values)
        {
            if (values == null) return Enumerable.Empty<string>();
            return values.Where(value => !string.IsNullOrEmpty(value));
        }
    }
}
