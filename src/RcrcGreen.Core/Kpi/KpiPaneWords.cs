using System;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every line the KPI pane shows, so the pane formats nothing of its own. The same rule
    /// as PanelSteps for the Drawing Sheet: a count written next to the control that shows it
    /// is a second record of one fact.
    /// </summary>
    public static class KpiPaneWords
    {
        public const string NoModelName = "No model open";

        public const string Waiting = "Open a model. This pane reads its name as soon as it is shown.";

        public const string NoModel = "No open document. Open a model.";

        /// <summary>
        /// Before any read has answered for this model. It names no button, because there
        /// is none: Create reads the model itself when it needs to.
        /// </summary>
        public const string NotScanned = "Not read yet.";

        public const string Scanning =
            "Reading the whole model. It reads every sheet, link and schedule, so it takes "
            + "longer than the Drawing Sheet read.";

        public const string ReadOnly =
            "Create reads the model and writes a workbook and two text files. It creates "
            + "nothing in the model and never writes to a template.";

        public static string ModelNamed(string documentTitle)
        {
            return string.IsNullOrEmpty(documentTitle) ? NoModelName : documentTitle;
        }

        /// <summary>
        /// The line under the model name once a scan has run. The count and the time go with
        /// the name because they say which model and which state of it the file describes.
        /// </summary>
        public static string ReadAt(DateTime readAt, int elements, double seconds)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Read at {0}, {1} elements in {2} seconds.",
                readAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                elements,
                seconds.ToString("0.0", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The status line after a scan. Counts of what was read, how many of the nine
        /// questions the file has something for, and where the file is.
        /// </summary>
        public static string Headline(KpiScan scan, string where)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            int answered = KpiQuestions.Answers(scan).Count(answer => answer.Answered);
            int loaded = scan.Links.Instances.Count(link => link.IsLoaded);
            int regions = scan.Links.Contents.Sum(link => link.FilledRegionCount);

            string said = string.Format(
                CultureInfo.InvariantCulture,
                "{0} sheets, {1} title blocks, {2} links with {3} loaded, {4} schedules, {5} filled "
                + "regions read. {6} of 9 questions have something in the file",
                scan.TitleBlocks.SheetCount,
                scan.TitleBlocks.TitleBlockInstances,
                scan.Links.Instances.Count,
                loaded,
                scan.Schedules.Schedules.Count,
                regions,
                answered);

            if (scan.Skipped.Count > 0)
            {
                said += ", and " + scan.Skipped.Count + (scan.Skipped.Count == 1 ? " read" : " reads")
                    + " did not happen, named at the top of it";
            }

            return said + ". " + (where ?? string.Empty);
        }
    }
}
