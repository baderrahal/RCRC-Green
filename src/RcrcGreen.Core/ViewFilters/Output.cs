namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What one press really did, in counts the results area and the report both print.
    /// Ported from the argus host. Skipped and Blocked are the fourth edit's additions:
    /// Skipped counts views whose names do not start with a plot id, Blocked counts views
    /// whose templates own the filters setting, and both used to vanish without a word.
    /// </summary>
    public class Output
    {
        public int ViewsEvaluated { get; set; }

        public int ViewsModified { get; set; }

        public int FiltersAdded { get; set; }

        public int FiltersConfigured { get; set; }

        public int FiltersCreatedInDoc { get; set; }

        public int FiltersNotFoundInDoc { get; set; }

        public int Skipped { get; set; }

        public int Blocked { get; set; }

        public string[] Logs { get; set; }

        /// <summary>
        /// One entry per thing the run did not apply, each carrying the same sentence the
        /// run logged for it, so the result panel and the log cannot word one failure two
        /// ways. Filled at the same sites that already count the failures.
        /// </summary>
        public ViewFilterFailure[] Failures { get; set; }
    }
}
