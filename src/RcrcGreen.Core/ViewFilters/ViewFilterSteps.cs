using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The five steps of the View Filters rail: which can be used, which is finished, what
    /// each says while it is shut. The pane draws them and decides none of them, the same
    /// rule PanelSteps holds for the Drawing Sheet, because a reachability rule written
    /// next to the control it greys is two records of one fact.
    ///
    /// Every answer is worked out by comparing what is on the pane now against what the
    /// last scan read, through <see cref="ApplyGate"/>, which stays the one copy of that
    /// comparison. Nothing here is a flag: an edit undone by hand reads as undone.
    /// </summary>
    public sealed class ViewFilterSteps
    {
        private readonly IReadOnlyList<ViewFilterStepState> _steps;

        private ViewFilterSteps(IReadOnlyList<ViewFilterStepState> steps)
        {
            _steps = steps;
        }

        public IReadOnlyList<ViewFilterStepState> All
        {
            get { return _steps; }
        }

        public ViewFilterStepState For(ViewFilterStep step)
        {
            ViewFilterStepState found = _steps.FirstOrDefault(one => one.Step == step);
            if (found == null)
            {
                throw new ArgumentException("No such step.", "step");
            }

            return found;
        }

        /// <summary>
        /// Which step to open once one is finished: the next one down that can be used,
        /// or nothing, which leaves the pane where it is.
        /// </summary>
        public ViewFilterStep? OpenAfter(ViewFilterStep finished)
        {
            return _steps
                .Where(one => one.Number > (int)finished)
                .Where(one => one.Usable)
                .Select(one => (ViewFilterStep?)one.Step)
                .FirstOrDefault();
        }

        /// <summary>
        /// Where the pane lands after a scan comes back: the first usable step with work
        /// left in it. With everything finished, which is what a run leaves behind, it is
        /// the results, and with nothing usable at all it is step 1, where the reason is
        /// written.
        /// </summary>
        public ViewFilterStep FirstUnfinished
        {
            get
            {
                ViewFilterStepState found = _steps.FirstOrDefault(one => one.Usable && !one.Done);
                if (found != null) return found.Step;

                return _steps.Any(one => one.Usable)
                    ? ViewFilterStep.Results
                    : ViewFilterStep.Keywords;
            }
        }

        /// <summary>
        /// The five steps off what the pane holds right now. Current is what the boxes
        /// say, scanned is what the last answered scan was asked with and null before one,
        /// scan is what that scan read, and run is the last answered run and null before
        /// one. FilterRows.Kept trims prefixes on the rows it is handed, so it runs over a
        /// deep copy here and never over the pane's own records, which is the fault that
        /// jammed Apply for a round.
        /// </summary>
        public static ViewFilterSteps Of(
            Inputs current,
            Inputs scanned,
            ViewFilterScanResult scan,
            ViewFilterResult run)
        {
            int keywords = ViewKeywords.Split(current == null ? null : current.ViewKeywords).Length;

            Inputs copied = InputsCopy.Deep(current);
            int rows = current == null || current.Filters == null ? 0 : current.Filters.Length;
            int withAPrefix = FilterRows.Kept(copied == null ? null : copied.Filters).Length;

            bool keywordsDone = keywords > 0;
            bool rowsDone = withAPrefix > 0;
            bool scanAnswered = scanned != null;
            bool boxesMatchTheScan = ApplyGate.SameInputs(scanned, current);
            bool runAnswered = run != null;

            var steps = new List<ViewFilterStepState>
            {
                Keywords(keywords, keywordsDone),
                Rows(rows, withAPrefix, keywordsDone, rowsDone),
                Scan(scan, rowsDone, scanAnswered, boxesMatchTheScan),
                Apply(scanAnswered, boxesMatchTheScan, runAnswered),
                Results(run, runAnswered)
            };

            return new ViewFilterSteps(steps);
        }

        private static ViewFilterStepState Keywords(int keywords, bool done)
        {
            string summary = keywords == 0
                ? string.Empty
                : Count(keywords) + (keywords == 1 ? " keyword" : " keywords");

            return new ViewFilterStepState(
                ViewFilterStep.Keywords, "KEYWORDS", summary, true, string.Empty, done);
        }

        private static ViewFilterStepState Rows(int rows, int withAPrefix, bool usable, bool done)
        {
            string summary;
            if (rows == 0)
            {
                summary = string.Empty;
            }
            else
            {
                string held = withAPrefix == 0
                    ? "none with a prefix"
                    : Count(withAPrefix) + " with a prefix";
                summary = Count(rows) + (rows == 1 ? " row, " : " rows, ") + held;
            }

            return new ViewFilterStepState(
                ViewFilterStep.Rows,
                "FILTER ROWS",
                summary,
                usable,
                usable ? string.Empty : "Type at least one keyword in step 1.",
                done);
        }

        private static ViewFilterStepState Scan(
            ViewFilterScanResult scan, bool usable, bool scanAnswered, bool boxesMatchTheScan)
        {
            string summary = scan == null
                ? string.Empty
                : Count(scan.Plots.Count) + (scan.Plots.Count == 1 ? " plot, " : " plots, ")
                    + Count(scan.SkippedViews.Count) + " skipped, "
                    + Count(scan.BlockedViews.Count) + " blocked";

            return new ViewFilterStepState(
                ViewFilterStep.Scan,
                "SCAN",
                summary,
                usable,
                usable ? string.Empty : "Give at least one row a prefix in step 2.",
                scanAnswered && boxesMatchTheScan);
        }

        private static ViewFilterStepState Apply(
            bool scanAnswered, bool boxesMatchTheScan, bool runAnswered)
        {
            bool usable = scanAnswered && boxesMatchTheScan;

            // The same pick the greyed Apply button has always made: no scan at all asks
            // for one, a scan the boxes have moved off asks for it again.
            string whyNot = usable
                ? string.Empty
                : (scanAnswered ? ViewFilterWords.ScanAgain : ViewFilterWords.NeedAScan);

            return new ViewFilterStepState(
                ViewFilterStep.Apply, "APPLY", string.Empty, usable, whyNot, runAnswered);
        }

        private static ViewFilterStepState Results(ViewFilterResult run, bool runAnswered)
        {
            string summary = run == null
                ? string.Empty
                : Count(run.Applied) + " applied, " + Count(run.NotApplied) + " not applied";

            return new ViewFilterStepState(
                ViewFilterStep.Results,
                "RESULTS",
                summary,
                runAnswered,
                runAnswered ? string.Empty : "Nothing has run yet.",
                runAnswered);
        }

        private static string Count(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
