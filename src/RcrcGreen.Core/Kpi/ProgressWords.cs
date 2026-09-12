using System;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The lines the status line shows while a scan or a create is running, driven by what is
    /// actually done, never by a timer and never by an estimate. A scan took 123 seconds on a
    /// 104,031 element model and the pane showed one line that did not move, which is what a
    /// hung tool looks like.
    ///
    /// The section lines take the scan report's own headings, 4 LINKED MODELS, so the numbers
    /// on screen are the report's numbers and not a second record of them. One reader covers
    /// sections 5 to 8 in one pass, so those four are announced as a span rather than lied
    /// about one by one. A percentage appears only where the total is known, is floored, and
    /// is driven by a count that only grows, so it cannot go backwards.
    ///
    /// The words live here with tests and the pane shows them. Whether a line set mid run
    /// repaints on a real dockable pane is a fact about Revit's hosting that only a run can
    /// show, and it is recorded as open in the log.
    /// </summary>
    public static class ProgressWords
    {
        /// <summary>
        /// The scan report's sections, whose count is the leading digit of its last heading.
        /// A test holds this against <see cref="KpiReport.TheNineQuestions"/> so the two
        /// cannot part.
        /// </summary>
        public const int SectionCount = 9;

        public const string CopyingTheTemplate = "Copying the template.";

        public const string WritingTheCells = "Writing the cells.";

        public const string ReadingThemBack = "Reading the written cells back.";

        public const string CheckingTheFormulas = "Checking the workbook's own formulas.";

        public const string ReusingTheReadings = "Reusing the readings already held.";

        /// <summary>
        /// The scan already held answers this press, so the model was not read again. Said
        /// for the same reason the readings say it: a press that skips two minutes of work
        /// looks like a press that skipped the work.
        /// </summary>
        public const string ReusingTheScan = "Reusing the scan already held.";

        /// <summary>
        /// What the progress window is called while a press runs. The window shows the same
        /// lines the status line does, because two wordings for one run is two records of
        /// one fact.
        /// </summary>
        public const string WindowTitle = "RCRC Green KPI";

        public const string AddingUp = "Adding the plots up.";

        public const string WritingTheReport = "Writing the report.";

        /// <summary>
        /// Section 4 of 9, linked models. The heading is the report's own, 4 LINKED MODELS,
        /// so the number and the name are read off it rather than held a second time.
        /// </summary>
        public static string Section(string reportHeading)
        {
            return "Section " + NumberOf(reportHeading) + " of "
                + SectionCount.ToString(CultureInfo.InvariantCulture) + ", " + NameOf(reportHeading) + ".";
        }

        /// <summary>
        /// Sections 5 to 8 of 9, schedules to areas and units. One reader covers the four in
        /// one pass, so they are announced together rather than pretended apart.
        /// </summary>
        public static string SectionSpan(string fromHeading, string toHeading)
        {
            return Span(fromHeading, toHeading) + ", " + NameOf(fromHeading) + " to " + NameOf(toHeading) + ".";
        }

        /// <summary>
        /// Sections 5 to 8 of 9, schedules, 400 of 951, 42%. The count is schedules read so
        /// far, so the percentage only grows.
        /// </summary>
        public static string SectionSpan(string fromHeading, string toHeading, int done, int of)
        {
            return Span(fromHeading, toHeading) + ", " + NameOf(fromHeading) + ", "
                + done.ToString(CultureInfo.InvariantCulture) + " of " + of.ToString(CultureInfo.InvariantCulture)
                + Percent(done, of) + ".";
        }

        /// <summary>
        /// Reading DM-44, plot 3 of 18, 11%. The percentage is of plots finished, two of the
        /// eighteen when the third begins, so it never goes backwards and reaches 100 only
        /// when the last plot is done and the line moves on.
        /// </summary>
        public static string ReadingPlot(string plotId, int number, int of)
        {
            return "Reading " + (plotId ?? string.Empty) + ", plot "
                + number.ToString(CultureInfo.InvariantCulture) + " of " + of.ToString(CultureInfo.InvariantCulture)
                + Percent(number - 1, of) + ".";
        }

        /// <summary>
        /// The percentage, floored, or nothing where the total is not known, because a count
        /// alone is honest and a percentage over nothing is not.
        /// </summary>
        public static string Percent(int done, int of)
        {
            if (of <= 0 || done < 0 || done > of) return string.Empty;

            long floored = (long)done * 100L / of;
            return ", " + floored.ToString(CultureInfo.InvariantCulture) + "%";
        }

        private static string Span(string fromHeading, string toHeading)
        {
            return "Sections " + NumberOf(fromHeading) + " to " + NumberOf(toHeading)
                + " of " + SectionCount.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The leading digits of a report heading, 4 off 4 LINKED MODELS. A heading with none
        /// is refused, because a made up section number on the status line is worse than a
        /// thrown line in a log.
        /// </summary>
        private static string NumberOf(string reportHeading)
        {
            string held = (reportHeading ?? string.Empty).Trim();
            int at = 0;
            while (at < held.Length && char.IsDigit(held[at])) at++;
            if (at == 0) throw new ArgumentException("The heading opens with no section number: " + held, "reportHeading");

            return held.Substring(0, at);
        }

        private static string NameOf(string reportHeading)
        {
            string held = (reportHeading ?? string.Empty).Trim();
            int at = 0;
            while (at < held.Length && char.IsDigit(held[at])) at++;

            return held.Substring(at).Trim().ToLowerInvariant();
        }
    }
}
