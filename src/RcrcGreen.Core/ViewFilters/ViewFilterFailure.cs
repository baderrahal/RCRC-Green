namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// One thing a run did not apply: what it was and the run's own sentence saying why.
    /// The sentence is the same string the runner logs, built once at the site and handed
    /// to both, so the log and the result panel can never word one failure two ways. What
    /// names the view for a view level failure and the target filter for a filter level
    /// one, because that is the thing somebody goes and looks at.
    /// </summary>
    public sealed class ViewFilterFailure
    {
        public ViewFilterFailure(string what, string why)
        {
            What = what ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public string What { get; }

        public string Why { get; }
    }
}
