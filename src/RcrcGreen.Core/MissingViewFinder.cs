using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Turns the grid into a per plot list of what is not there yet.
    /// </summary>
    public static class MissingViewFinder
    {
        /// <summary>
        /// Every plot gets an entry, including the ones with nothing missing, so the caller
        /// can tell a finished plot apart from a plot that was never in the grid.
        ///
        /// A type is ordered by how many other plots hold it. The plot in hand does not hold
        /// the type, so that count is the same as the grid count for it.
        /// </summary>
        public static IReadOnlyList<PlotMissingViews> Find(PlotViewGrid grid)
        {
            if (grid == null) throw new ArgumentNullException("grid");

            var reports = new List<PlotMissingViews>(grid.PlotIds.Count);
            foreach (string plotId in grid.PlotIds)
            {
                List<MissingViewType> missing = grid.MissingFor(plotId)
                    .Select(type => new MissingViewType(type, grid.PlotsHolding(type)))
                    .OrderByDescending(entry => entry.PlotsHoldingType)
                    .ThenBy(entry => entry.ViewType)
                    .ToList();

                reports.Add(new PlotMissingViews(plotId, missing));
            }

            return reports;
        }
    }
}
