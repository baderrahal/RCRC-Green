using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The report one press of Apply writes: the counts, the run's own summary lines, then
    /// every line the run logged, so what the pane's log list showed and what the file holds
    /// are one record. The counts come first because a reader wants the size of the run
    /// before the detail of it.
    /// </summary>
    public static class ViewFiltersReport
    {
        public static string Write(
            Output output,
            IReadOnlyList<string> loggedLines,
            string documentTitle,
            DateTime writtenAt)
        {
            StringBuilder report = new StringBuilder();

            report.AppendLine("RCRC GREEN VIEW FILTERS RUN");
            report.AppendLine("Model: " + (documentTitle ?? string.Empty));
            report.AppendLine(
                "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            report.AppendLine(
                "One transaction, named RCRC Green - View Filters, so the whole run is one undo.");
            report.AppendLine();

            report.AppendLine("WHAT THE RUN DID");
            foreach (string line in ViewFilterWords.ResultLines(output))
            {
                report.AppendLine(line);
            }

            report.AppendLine();
            report.AppendLine("THE RUN'S OWN SUMMARY");
            foreach (string line in output.Logs ?? new string[0])
            {
                report.AppendLine(line);
            }

            report.AppendLine();
            report.AppendLine("EVERY LINE THE RUN LOGGED");
            if (loggedLines == null || loggedLines.Count == 0)
            {
                report.AppendLine("Nothing was logged.");
            }
            else
            {
                foreach (string line in loggedLines)
                {
                    report.AppendLine(line);
                }
            }

            return report.ToString();
        }
    }
}
