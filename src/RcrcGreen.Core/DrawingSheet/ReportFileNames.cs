using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The names of the three files Drawing Sheet writes: the scan, the scope box report and
    /// the run report.
    ///
    /// The prefixes sat on <see cref="ScanFileName"/> in Shared, so renaming a Drawing Sheet
    /// report was a Shared change that stopped every other session, for three constants no
    /// other task reads. The cleaning of the title and the date stay in Shared, where KPI
    /// names its own files through them.
    /// </summary>
    public static class ReportFileNames
    {
        public const string ScanPrefix = "RCRC-Green-Scan_";

        public const string ScopeBoxPrefix = "RCRC-Green-ScopeBox_";

        public const string RunPrefix = "RCRC-Green-Run_";

        public static string ForScan(string documentTitle, DateTime writtenAt)
        {
            return ScanFileName.For(ScanPrefix, documentTitle, writtenAt);
        }
    }
}
