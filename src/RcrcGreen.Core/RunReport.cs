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
    ///
    /// Every counted section reads <see cref="RunOutcome"/>. None of them reads the plan. The
    /// plan says what was intended and appears once, in the line near the top that begins This
    /// run would make, and nowhere else. That separation is the whole point of this file after
    /// the first real run listed four views as created and refused at the same time.
    /// </summary>
    public static class RunReport
    {
        private const string LineEnd = "\r\n";

        /// <summary>
        /// Headings used by both the report and the test that holds one against the other, so
        /// renaming a section cannot quietly take it out of that check.
        /// </summary>
        public const string CreatedPlanViews = "CREATED, PLAN VIEWS";

        public const string CreatedSections = "CREATED, SECTIONS";

        public const string CreatedSchedules = "CREATED, SCHEDULES";

        public const string CreatedSheets = "CREATED, SHEETS";

        public const string NotCreatedBefore = "NOT CREATED, DECIDED BEFORE THE RUN";

        public const string NotCreatedDuring = "NOT CREATED, REFUSED BY REVIT DURING THE RUN";

        public const string StillInTheModel = "CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND";

        public const string NeedsAttention = "CREATED, BUT NEEDS ATTENTION";

        public const string SetUpFrom = "WHERE EACH NEW VIEW WAS SET UP FROM";

        public static readonly IReadOnlyList<string> CreatedHeadings = new List<string>
        {
            CreatedPlanViews, CreatedSections, CreatedSchedules, CreatedSheets
        };

        public static readonly IReadOnlyList<string> NotCreatedHeadings = new List<string>
        {
            NotCreatedBefore, NotCreatedDuring, StillInTheModel
        };

        public static string Write(
            RunPlan plan,
            RunOutcome outcome,
            string documentTitle,
            DateTime writtenAt,
            bool applied)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (outcome == null) throw new ArgumentNullException("outcome");
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            var report = new StringBuilder();

            Line(report, "RCRC GREEN, DRAWING SHEET RUN");
            Line(report, documentTitle);
            Line(report, writtenAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            Line(report, string.Empty);
            Line(report, applied ? "The run was confirmed and written." : "Nothing was written.");
            Line(report, plan.InWords());
            Line(report, outcome.CreatedCount + " were created and " + outcome.NotCreatedCount
                + " were not. Every count below is of what happened, not of what was intended.");

            Banner(report, outcome);

            Line(report, string.Empty);

            Section(report, StillInTheModel, outcome.LeftBehind.Select(one => one.ToString()));

            Section(report, CreatedPlanViews, Named(outcome, RunItemKind.PlanView));
            Section(report, CreatedSections, Named(outcome, RunItemKind.Section));
            Section(report, CreatedSchedules, Named(outcome, RunItemKind.Schedule));
            Section(report, CreatedSheets, Named(outcome, RunItemKind.Sheet));

            Section(report, NotCreatedBefore, plan.Refusals.Select(one => one.ToString()));
            Section(report, NotCreatedDuring, outcome.NotCreated.Select(one => one.ToString()));

            Section(report, NeedsAttention, outcome.Attention.Select(one => one.ToString()));

            // Named rather than assumed. A view came out with a template and a family type that
            // looked as though they had come from two different places, and nothing in the file
            // said which view either had come from, so it could not be checked at all.
            Section(report, SetUpFrom, outcome.SetUp.Select(one => one.ToString()));

            Closing(report);

            return report.ToString();
        }

        private static IEnumerable<string> Named(RunOutcome outcome, RunItemKind kind)
        {
            return outcome.CreatedOfKind(kind).Select(item => item.Name);
        }

        /// <summary>
        /// Two things go above the sections. A wrong schedule left in the model, which gets
        /// worse the longer it sits there, and a report that contradicts itself, which means
        /// nothing else in the file can be trusted.
        /// </summary>
        private static void Banner(StringBuilder report, RunOutcome outcome)
        {
            IReadOnlyList<string> bothWays = outcome.BothWays;
            if (bothWays.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "THIS REPORT CONTRADICTS ITSELF AND IS A BUG IN THE TOOL. "
                    + bothWays.Count);
                Line(report, "of these are listed as created and as not created at the same time:");
                foreach (string name in bothWays) Line(report, "  " + name);
                Line(report, "Believe the model, not this file, and report it.");
            }

            if (outcome.LeftBehind.Count == 0) return;

            Line(report, string.Empty);
            Line(report, "READ THIS FIRST. " + outcome.LeftBehind.Count
                + (outcome.LeftBehind.Count == 1 ? " wrong schedule is" : " wrong schedules are")
                + " in the model and could not");
            Line(report, "be removed. Each one is named below and has to be deleted by hand.");
        }

        private static void Closing(StringBuilder report)
        {
            Line(report, "A plan view is created fresh. Nothing is copied or duplicated, and a");
            Line(report, "new view carries no annotation, dimensions, tags or detailing. Its view");
            Line(report, "family type, its level and its view template all come from a view of the");
            Line(report, "same type that this model already holds on another plot, so they are what");
            Line(report, "the team built rather than anything matched on a name.");
            Line(report, "A section is cut across the middle of the plot's scope box, the short way.");
            Line(report, "How far it looks is the far clip offset of the sibling section, and only");
            Line(report, "when that sibling has none does it fall back to the "
                + SectionDepth.Metres.ToString("0.#", CultureInfo.InvariantCulture) + " metres the team");
            Line(report, "named. The section listed above says which of the two was used for each one.");
            Line(report, "A section is left with no scope box, because a real one in this model has");
            Line(report, "none and its own section box is what bounds it. The plot's box is still what");
            Line(report, "says where to cut. Whether a view type needs a section rather than a plan is");
            Line(report, "read off the kind of the view the model already holds for it.");
            Line(report, "A schedule is captured from a plot that already has it and rebuilt for");
            Line(report, "the target plot, with only the filter naming the plot changed.");
            Line(report, "A schedule that lost a FILTER is deleted again inside the same");
            Line(report, "transaction, because it would show every plot's elements and read as");
            Line(report, "correct on a drawing. When Revit refuses that delete, the schedule is in");
            Line(report, "the model and is named at the top of this report. A schedule short of a");
            Line(report, "FIELD is created and named above, because a missing column can be seen.");
            Line(report, "A sheet copies the title block and the layout of the sheet the user");
            Line(report, "picked. The sheet number and the sheet name are typed by the user and");
            Line(report, "are never invented. A view already sitting on another sheet is refused");
            Line(report, "rather than moved, because it belongs to whoever put it there.");
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
