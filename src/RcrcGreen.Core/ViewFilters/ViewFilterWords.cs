using System.Globalization;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// Every line the View Filters pane shows, so the pane formats nothing of its own. The
    /// same rule KpiPaneWords and PanelSteps follow: a summary written next to the control
    /// that shows it is two records of one fact.
    /// </summary>
    public static class ViewFilterWords
    {
        /// <summary>
        /// The keyword box's starting text. Three view names the team filters, one per line,
        /// and the box is free text so anything can be typed over them.
        /// </summary>
        public const string DefaultKeywords =
            "Location Key Plan\nOverall Key Plan\nGeneral Arrangement Layout";

        public const string NoModel = "No model is open. Open one, then scan.";

        public const string Scanning = "Reading the views.";

        public const string Applying = "Applying the view filters.";

        public const string WritingTheReport = "Writing the report.";

        /// <summary>Under a greyed Apply whose scan is older than the boxes above it.</summary>
        public const string ScanAgain = "Scan again";

        public const string NeedAScan = "Scan first, so what Apply will do has been seen.";

        public const string ModelMoved =
            "The model is not the one the scan read. Scan again.";

        public static string DefaultsLine(string fileName)
        {
            return "The rows below are the defaults from " + fileName
                + " beside the installed add-in.";
        }

        public static string MissingSettings(string fileName)
        {
            return fileName + " was not found beside the installed add-in, so the rows below "
                + "start empty. Run install.ps1 again to put the shipped defaults back.";
        }

        public static string BrokenSettings(string fileName, string problem)
        {
            return fileName + " could not be read, so the rows below start empty. " + problem;
        }

        public static string CellWords(ScanCell cell)
        {
            switch (cell)
            {
                case ScanCell.Exists: return "Exists";
                case ScanCell.WillCreate: return "Will create";
                default: return "Cannot create";
            }
        }

        public static string ScannedLine(ViewFilterScanResult result)
        {
            return "Read " + Count(result.ViewsMatched) + " matching views on "
                + result.DocumentTitle + ": " + Count(result.Plots.Count) + " plots, "
                + Count(result.SkippedViews.Count) + " skipped, "
                + Count(result.BlockedViews.Count) + " blocked.";
        }

        public static string SkippedHeading(int count)
        {
            return "Skipped, " + Count(count) + " views whose names do not start with a plot id";
        }

        public static string BlockedHeading(int count)
        {
            return "Blocked, " + Count(count) + " views whose templates own the filters setting";
        }

        public static string AppliedLine(Output output, string reportPlace)
        {
            string line = "Done. " + Count(output.ViewsModified) + " of "
                + Count(output.ViewsEvaluated) + " views changed.";
            return string.IsNullOrEmpty(reportPlace) ? line : line + " " + reportPlace;
        }

        /// <summary>
        /// The results block, one line per count, in the order the round asked for them.
        /// </summary>
        public static string[] ResultLines(Output output)
        {
            return new string[]
            {
                "Views evaluated " + Count(output.ViewsEvaluated),
                "Views modified " + Count(output.ViewsModified),
                "Filters added to views " + Count(output.FiltersAdded),
                "Filters configured " + Count(output.FiltersConfigured),
                "Filters created in the document " + Count(output.FiltersCreatedInDoc),
                "Filter lookups with nothing to use " + Count(output.FiltersNotFoundInDoc),
                "Views skipped, no plot id " + Count(output.Skipped),
                "Views blocked by their template " + Count(output.Blocked)
            };
        }

        private static string Count(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
