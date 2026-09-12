using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Names the file a scan of the model writes. It goes through the same naming as the other three
    /// reports so the four sort together on the Desktop, with a prefix of its own so a KPI
    /// file is never mistaken for a Drawing Sheet scan of the same model at the same minute.
    /// </summary>
    public static class KpiFile
    {
        public const string Prefix = "RCRC-Green-KPI_";

        public static string NameFor(string documentTitle, DateTime writtenAt)
        {
            return ScanFileName.For(Prefix, documentTitle, writtenAt);
        }
    }
}
