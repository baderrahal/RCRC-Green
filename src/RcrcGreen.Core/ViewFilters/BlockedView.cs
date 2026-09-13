namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// A view the run leaves alone because its template owns the filters setting, named with
    /// the template so somebody can decide whether to release the setting there. The fourth
    /// edit: these used to throw and vanish into the catch.
    /// </summary>
    public sealed class BlockedView
    {
        public BlockedView(string viewName, string templateName)
        {
            ViewName = viewName ?? string.Empty;
            TemplateName = templateName ?? string.Empty;
        }

        public string ViewName { get; }

        public string TemplateName { get; }
    }
}
