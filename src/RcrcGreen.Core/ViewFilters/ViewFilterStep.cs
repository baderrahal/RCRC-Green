namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The five steps of the View Filters pane, numbered as the rail shows them. The titles
    /// are the pane's own section headings, held on <see cref="ViewFilterSteps"/>, so the
    /// rail renames nothing the pane already said. The same shape as PanelStep, and its own
    /// enum rather than that one, because the two panes' steps are two facts that happen to
    /// both count to five.
    /// </summary>
    public enum ViewFilterStep
    {
        Keywords = 1,

        Rows = 2,

        Scan = 3,

        Apply = 4,

        Results = 5
    }
}
