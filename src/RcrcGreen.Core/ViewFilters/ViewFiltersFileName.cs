using System;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// Names the file a run's report is written to, through the Shared cleaning and date so
    /// it sorts beside the other tasks' reports, with a prefix of its own so a View Filters
    /// run is never mistaken for a Drawing Sheet run of the same model at the same minute.
    /// </summary>
    public static class ViewFiltersFileName
    {
        public const string Prefix = "RCRC-Green-ViewFilters_";

        public static string For(string documentTitle, DateTime writtenAt)
        {
            return ScanFileName.For(Prefix, documentTitle, writtenAt);
        }
    }
}
