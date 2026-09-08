using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The file a run writes, whether it created anything or not.
    ///
    /// A run that made nothing still writes one, because the reason it made nothing is the
    /// thing worth keeping.
    /// </summary>
    public static class RunReport
    {
        private const string LineEnd = "\r\n";

        public static string Write(
            RunPlan plan,
            string documentTitle,
            DateTime writtenAt,
            bool applied,
            IEnumerable<RunRefusal> failed)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            IReadOnlyList<RunRefusal> refusedWhileWriting =
                (failed ?? Enumerable.Empty<RunRefusal>()).Where(one => one != null).ToList();

            var report = new StringBuilder();

            Line(report, "RCRC GREEN, DRAWING SHEET RUN");
            Line(report, documentTitle);
            Line(report, writtenAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            Line(report, string.Empty);
            Line(report, applied ? "The run was confirmed and written." : "Nothing was written.");
            Line(report, plan.InWords());
            Line(report, string.Empty);

            Section(report, "PLAN VIEWS", plan.Items
                .Where(item => item.Kind == RunItemKind.PlanView)
                .Select(item => item.Name));

            Section(report, "SCHEDULES", plan.Items
                .Where(item => item.Kind == RunItemKind.Schedule)
                .Select(item => item.Name));

            Section(report, "NOT CREATED, DECIDED BEFORE THE RUN",
                plan.Refusals.Select(one => one.ToString()));

            Section(report, "NOT CREATED, REFUSED BY REVIT DURING THE RUN",
                refusedWhileWriting.Select(one => one.ToString()));

            Line(report, "SHEETS, 0");
            Line(report, "  No sheet was created and none can be yet.");
            Line(report, "  The team has said the user types the sheet number and the sheet name");
            Line(report, "  and chooses one view per sheet or several. Three things are still open:");
            Line(report, "  which title block a new sheet takes, where a view sits on it, and how");
            Line(report, "  several views lay out together. Guessing any of them would put the wrong");
            Line(report, "  drawing in front of a reviewer, so nothing is guessed.");
            Line(report, string.Empty);
            Line(report, "A plan view is created fresh. Nothing is copied or duplicated, and a");
            Line(report, "new view carries no annotation, dimensions, tags or detailing.");
            Line(report, "A schedule is captured from a plot that already has it and rebuilt for");
            Line(report, "the target plot, with only the filter naming the plot changed.");

            return report.ToString();
        }

        private static void Section(StringBuilder report, string heading, IEnumerable<string> lines)
        {
            List<string> all = lines.ToList();

            Line(report, heading + ", " + all.Count);
            if (all.Count == 0)
            {
                Line(report, "  none");
            }
            else
            {
                foreach (string line in all) Line(report, "  " + line);
            }

            Line(report, string.Empty);
        }

        private static void Line(StringBuilder report, string text)
        {
            report.Append(text).Append(LineEnd);
        }
    }
}
