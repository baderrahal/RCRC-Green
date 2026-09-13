namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// One view the keyword match found, as plain values: its name, whether its template
    /// owns the filters setting, and that template's name when it does. The plot code is not
    /// on it because <see cref="ViewFilterScanPlan"/> works it out through the same rule the
    /// run uses, so the two cannot part.
    /// </summary>
    public sealed class ScannedFilterView
    {
        public ScannedFilterView(string viewName, bool blocked, string templateName)
        {
            ViewName = viewName ?? string.Empty;
            Blocked = blocked;
            TemplateName = templateName ?? string.Empty;
        }

        public string ViewName { get; }

        public bool Blocked { get; }

        public string TemplateName { get; }
    }
}
