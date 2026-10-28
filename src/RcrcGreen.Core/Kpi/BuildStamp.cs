namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The build stamp as the KPI pane and its reports print it.
    ///
    /// The stamp is the commit's short hash and the time the build ran, baked into the add-in's
    /// product version by the Revit project, with the word changed after it where the build
    /// came from a working copy holding uncommitted changes. A screenshot or a report sent back
    /// from somebody's machine then says which build made it, and the team kit's version is
    /// read off the same stamp rather than typed.
    /// </summary>
    public static class BuildStamp
    {
        public const string NotStamped = "not stamped";

        /// <summary>The stamp, or <see cref="NotStamped"/> where the build carries none.</summary>
        public static string Shown(string stamp)
        {
            return string.IsNullOrWhiteSpace(stamp) ? NotStamped : stamp.Trim();
        }

        public static string ReportLine(string stamp)
        {
            return "Build: " + Shown(stamp);
        }

        /// <summary>
        /// A report with the stamp as its first line, in the CRLF every KPI report is written
        /// in, so the stamp is the first thing anybody opening it reads.
        /// </summary>
        public static string Stamped(string stamp, string report)
        {
            return ReportLine(stamp) + "\r\n" + (report ?? string.Empty);
        }
    }
}
