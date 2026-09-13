namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What one press hands the run: the keyword box as typed and the filter rows as edited.
    /// Ported from the argus host with the same name and the same properties.
    /// </summary>
    public class Inputs
    {
        public string ViewKeywords { get; set; }

        public FilterConfig[] Filters { get; set; }
    }
}
