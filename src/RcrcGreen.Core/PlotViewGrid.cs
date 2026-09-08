using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Plots down the side, view types across the top, present or missing in each cell.
    /// </summary>
    public sealed class PlotViewGrid
    {
        private readonly Dictionary<string, HashSet<ViewType>> _presentByPlot;
        private readonly Dictionary<ViewType, int> _plotsHoldingType;

        private PlotViewGrid(
            IReadOnlyList<string> plotIds,
            IReadOnlyList<ViewType> viewTypes,
            Dictionary<string, HashSet<ViewType>> presentByPlot,
            Dictionary<ViewType, int> plotsHoldingType)
        {
            PlotIds = plotIds;
            ViewTypes = viewTypes;
            _presentByPlot = presentByPlot;
            _plotsHoldingType = plotsHoldingType;
        }

        public IReadOnlyList<string> PlotIds { get; }

        public IReadOnlyList<ViewType> ViewTypes { get; }

        /// <summary>
        /// Rows are the plot list joined with any plot seen in a view name, so a plot the
        /// caller did not pass in still gets a row rather than losing its views. Columns are
        /// every view type found anywhere in the names.
        /// </summary>
        public static PlotViewGrid Build(IEnumerable<ParsedViewName> parsedNames, IEnumerable<string> plotIds)
        {
            List<ParsedViewName> names = (parsedNames ?? Enumerable.Empty<ParsedViewName>())
                .Where(name => name != null)
                .ToList();

            var rows = new SortedSet<string>(NaturalOrder.Comparer);
            foreach (string id in (plotIds ?? Enumerable.Empty<string>()).Where(id => id != null))
            {
                rows.Add(id);
            }
            foreach (ParsedViewName name in names)
            {
                rows.Add(name.PlotId);
            }

            var presentByPlot = new Dictionary<string, HashSet<ViewType>>(StringComparer.Ordinal);
            foreach (string plot in rows)
            {
                presentByPlot.Add(plot, new HashSet<ViewType>());
            }

            var columns = new SortedSet<ViewType>();
            foreach (ParsedViewName name in names)
            {
                ViewType type = name.Type;
                columns.Add(type);
                presentByPlot[name.PlotId].Add(type);
            }

            Dictionary<ViewType, int> plotsHoldingType = columns.ToDictionary(
                type => type,
                type => presentByPlot.Values.Count(present => present.Contains(type)));

            return new PlotViewGrid(rows.ToList(), columns.ToList(), presentByPlot, plotsHoldingType);
        }

        public bool IsPresent(string plotId, ViewType viewType)
        {
            RequireColumn(viewType);
            return Present(plotId).Contains(viewType);
        }

        /// <summary>
        /// How many plots in this grid hold the given view type. The missing list is sorted
        /// by this number.
        /// </summary>
        public int PlotsHolding(ViewType viewType)
        {
            RequireColumn(viewType);
            return _plotsHoldingType[viewType];
        }

        public IReadOnlyList<ViewType> MissingFor(string plotId)
        {
            HashSet<ViewType> present = Present(plotId);
            return ViewTypes.Where(type => !present.Contains(type)).ToList();
        }

        /// <summary>
        /// An unknown plot is a caller mistake rather than an empty row. Answering false for
        /// it would read as a plot with nothing in it, which is a real state this grid reports.
        /// </summary>
        private HashSet<ViewType> Present(string plotId)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            HashSet<ViewType> present;
            if (!_presentByPlot.TryGetValue(plotId, out present))
            {
                throw new ArgumentException("Plot " + plotId + " has no row in this grid.", "plotId");
            }
            return present;
        }

        private void RequireColumn(ViewType viewType)
        {
            if (viewType == null) throw new ArgumentNullException("viewType");
            if (!_plotsHoldingType.ContainsKey(viewType))
            {
                throw new ArgumentException("View type " + viewType + " has no column in this grid.", "viewType");
            }
        }
    }
}
