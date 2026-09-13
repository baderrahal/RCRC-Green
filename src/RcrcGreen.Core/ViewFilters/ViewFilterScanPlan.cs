using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// Turns what the scan read into the grid the pane draws. The plot on a row comes off
    /// the view's own name through <see cref="ViewFilterPlotCode"/>, the same rule the run
    /// follows, and a cell's answer mirrors the run's own lookup order: the exact name, then
    /// the prefix with the plot at the end, then the exemplar. A blocked view puts no plot
    /// in the grid, because the run will not touch it, and it is listed underneath instead.
    /// </summary>
    public static class ViewFilterScanPlan
    {
        public static ViewFilterScanResult Of(
            string documentTitle,
            IReadOnlyList<ScannedFilterView> views,
            IReadOnlyList<string> filterNames,
            IReadOnlyList<ExemplarState> exemplars)
        {
            List<string> skipped = new List<string>();
            List<BlockedView> blocked = new List<BlockedView>();
            SortedSet<string> plots = new SortedSet<string>(NaturalOrder.Comparer);

            foreach (ScannedFilterView view in views ?? new List<ScannedFilterView>())
            {
                if (view.Blocked)
                {
                    blocked.Add(new BlockedView(view.ViewName, view.TemplateName));
                    continue;
                }

                string plotCode;
                if (!ViewFilterPlotCode.TryFromViewName(view.ViewName, out plotCode))
                {
                    skipped.Add(view.ViewName);
                    continue;
                }

                plots.Add(plotCode);
            }

            List<string> names = (filterNames ?? new List<string>()).ToList();
            List<ExemplarState> states = (exemplars ?? new List<ExemplarState>()).ToList();
            List<string> prefixes = states.Select(one => one.Prefix).ToList();

            Dictionary<string, IReadOnlyList<ScanCell>> cells =
                new Dictionary<string, IReadOnlyList<ScanCell>>();

            foreach (string plot in plots)
            {
                List<ScanCell> row = new List<ScanCell>();
                foreach (ExemplarState state in states)
                {
                    row.Add(CellFor(plot, state, names));
                }

                cells[plot] = row;
            }

            return new ViewFilterScanResult(
                documentTitle,
                plots.ToList(),
                prefixes,
                cells,
                skipped,
                blocked,
                views == null ? 0 : views.Count);
        }

        private static ScanCell CellFor(string plot, ExemplarState state, List<string> filterNames)
        {
            string target = ViewFilterNames.TargetFilterName(state.Prefix, plot);

            bool exists = filterNames.Any(name =>
                ViewFilterNames.IsExactMatch(name, target)
                || ViewFilterNames.IsPrefixAndPlotMatch(name, state.Prefix, plot));

            if (exists) return ScanCell.Exists;

            return state.CanCreate ? ScanCell.WillCreate : ScanCell.CannotCreate;
        }
    }
}
