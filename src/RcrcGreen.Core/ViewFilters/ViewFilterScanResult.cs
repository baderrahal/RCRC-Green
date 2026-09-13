using System.Collections.Generic;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What one scan read: the plots down the side, the prefixes across the top, a cell per
    /// pair, and the two lists the grid leaves out, the skipped views and the blocked ones.
    /// It carries the title of the model it was read from, so a press of Apply on another
    /// model can be refused rather than trusted to a stale grid.
    /// </summary>
    public sealed class ViewFilterScanResult
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyList<ScanCell>> _cells;

        public ViewFilterScanResult(
            string documentTitle,
            IReadOnlyList<string> plots,
            IReadOnlyList<string> prefixes,
            IReadOnlyDictionary<string, IReadOnlyList<ScanCell>> cells,
            IReadOnlyList<string> skippedViews,
            IReadOnlyList<BlockedView> blockedViews,
            int viewsMatched)
        {
            DocumentTitle = documentTitle ?? string.Empty;
            Plots = plots;
            Prefixes = prefixes;
            _cells = cells;
            SkippedViews = skippedViews;
            BlockedViews = blockedViews;
            ViewsMatched = viewsMatched;
        }

        public string DocumentTitle { get; }

        /// <summary>Grid rows, the plots the run would act on, in natural order.</summary>
        public IReadOnlyList<string> Plots { get; }

        /// <summary>Grid columns, one per filter row, in row order.</summary>
        public IReadOnlyList<string> Prefixes { get; }

        public IReadOnlyList<string> SkippedViews { get; }

        public IReadOnlyList<BlockedView> BlockedViews { get; }

        /// <summary>How many views the keywords matched, blocked and skipped ones included.</summary>
        public int ViewsMatched { get; }

        public ScanCell CellAt(string plot, int prefixIndex)
        {
            return _cells[plot][prefixIndex];
        }
    }
}
