namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What the RESULTS step holds after a run: the two counts, the eight count lines
    /// exactly as <see cref="ViewFilterWords.ResultLines"/> writes them, one line per
    /// failed item, and whether there is a report to open. Decided here rather than in the
    /// pane, so a test reaches every one of them.
    /// </summary>
    public sealed class ViewFilterResult
    {
        private ViewFilterResult(
            int applied,
            ViewFilterFailure[] failures,
            string[] lines,
            string reportPlace)
        {
            Applied = applied;
            Failures = failures;
            Lines = lines;
            ReportPlace = reportPlace ?? string.Empty;
        }

        /// <summary>How many filters the run configured, the run's own count.</summary>
        public int Applied { get; }

        /// <summary>
        /// How many things were not applied. It is the failure list's own length rather
        /// than a second count kept beside it, so the number and the lines under it can
        /// never disagree.
        /// </summary>
        public int NotApplied
        {
            get { return Failures.Length; }
        }

        /// <summary>The eight count lines, unchanged from the results block before the rail.</summary>
        public string[] Lines { get; }

        public ViewFilterFailure[] Failures { get; }

        /// <summary>
        /// The written report's own path, empty when none was written, so the button that
        /// opens it opens the file the run wrote rather than a sentence about it.
        /// </summary>
        public string ReportPlace { get; }

        public bool HasReport
        {
            get { return ReportPlace.Length > 0; }
        }

        public static ViewFilterResult Of(Output output, string reportPlace)
        {
            Output held = output ?? new Output();

            return new ViewFilterResult(
                held.FiltersConfigured,
                held.Failures ?? new ViewFilterFailure[0],
                ViewFilterWords.ResultLines(held),
                reportPlace);
        }
    }
}
