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

        public const string SetUpFrom = "WHERE EACH NEW VIEW AND SHEET WAS SET UP FROM";

        public const string WhereViewportsLanded = "WHERE EACH VIEWPORT LANDED";

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
            Line(report, Headline(plan, outcome));
            Line(report, "The before count is what the plan decided. Every other count in this "
                + "file is of what happened, not of what was intended.");

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

            // Recorded after each placement returned, because the first sheets came out with
            // views the user called too small and this file could not say where anything had
            // landed or at what scale. The scan reads the same measurements off real sheets,
            // so the two can be held against each other.
            Section(report, WhereViewportsLanded, outcome.Placements.Select(one => one.InWords()));

            Closing(report);

            return report.ToString();
        }

        /// <summary>
        /// One number for everything not created, split the way the sections below split it.
        ///
        /// It used to count the run's refusals and the schedules left behind while the plan's
        /// refusals sat in a section of their own with a count of their own, so four refused
        /// before the run and two during it read 2 were not at the top and 6 under the
        /// headings. Two numbers for not created, in the file that exists because two of its
        /// numbers once disagreed.
        /// </summary>
        public static string Headline(RunPlan plan, RunOutcome outcome)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (outcome == null) throw new ArgumentNullException("outcome");

            int notMade = plan.Refusals.Count + outcome.NotCreatedCount;

            return (outcome.CreatedCount == 1 ? "1 was created" : outcome.CreatedCount + " were created")
                + " and " + (notMade == 1 ? "1 was not: " : notMade + " were not: ")
                + plan.Refusals.Count + " refused before the run, "
                + outcome.NotCreated.Count + " refused by Revit during it, "
                + outcome.LeftBehind.Count + " created wrong and still in the model.";
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
            Line(report, "family type, its view template, its level, Crop View and Crop Region");
            Line(report, "Visible all come from ONE view of the same type that this model already");
            Line(report, "holds on another plot, named above, so they are what the team built rather");
            Line(report, "than anything matched on a name.");
            Line(report, "ANNOTATION CROP IS NOT COPIED. It is on, on every plan view, and that is the");
            Line(report, "tool's setting. Copying it was tried and did not work, because the siblings");
            Line(report, "themselves disagree and one with it off passes the fault straight on. A view");
            Line(report, "with it off draws the section markers of neighbouring plots through itself.");
            Line(report, "Where the model uses more than one view family type for one view type,");
            Line(report, "which sibling gets picked decides which one a new view gets. Run Scan Model");
            Line(report, "and read VIEW FAMILY TYPE PER VIEW TYPE to see where that is happening.");
            Line(report, "A section is cut across the middle of the plot's scope box, the short way,");
            Line(report, "and looks "
                + SectionDepth.Metres.ToString("0.#", CultureInfo.InvariantCulture)
                + " metre. That is the tool's own setting and not a number read");
            Line(report, "off any view. It used to come from the sibling section. Four real ones in");
            Line(report, "this model read 0.93, 0.93, 1.53 and 12.83 metres, so there was no rule to");
            Line(report, "copy and the depth turned on which sibling happened to be picked.");
            Line(report, "A section is left with no scope box, because a real one in this model has");
            Line(report, "none and its own section box is what bounds it. The plot's box is still what");
            Line(report, "says where to cut. Whether a view type needs a section rather than a plan is");
            Line(report, "read off the kind of the view the model already holds for it.");
            Line(report, "CROP VIEW IS NOT COPIED ON A SECTION. It is on, on every section, and that is");
            Line(report, "the tool's setting. Copying it was tried and the siblings have it off, so the");
            Line(report, "new section drew as far as the model reaches, its viewport came out thirteen");
            Line(report, "metres wide on an A1 sheet, and its marker crossed other plots' plans. The");
            Line(report, "crop region it enforces is the section box this run computed from the plot's");
            Line(report, "scope box, so switching it off was undoing the run's own work. The region");
            Line(report, "visibility and the annotation crop are still the sibling's.");
            Line(report, "A schedule is captured from a plot that already has it and rebuilt for");
            Line(report, "the target plot, with only the filter naming the plot changed. It is built");
            Line(report, "on Revit's own number for the category rather than on the category name,");
            Line(report, "because Slab Edges could not be found by name and KERBS was refused on a");
            Line(report, "model that holds it. A filter value goes back as the kind it came out as,");
            Line(report, "so a Yes/No parameter goes back as 1 or 0 and not as the word, which is");
            Line(report, "what Revit stores and what it will take.");
            Line(report, "A field the new schedule could not take is named above with the reason. A");
            Line(report, "calculated field, meaning a formula, a percentage, a count or a combined");
            Line(report, "parameter, is defined inside the schedule that holds it, so Revit never");
            Line(report, "offers it to a new one and it has to be written again by hand.");
            Line(report, "A schedule that lost a FILTER is deleted again inside the same");
            Line(report, "transaction, because it would show every plot's elements and read as");
            Line(report, "correct on a drawing. When Revit refuses that delete, the schedule is in");
            Line(report, "the model and is named at the top of this report. A schedule short of a");
            Line(report, "FIELD is created and named above, because a missing column can be seen.");
            Line(report, "A sheet is described rather than copied. The title block type, the views");
            Line(report, "and how many go per sheet are shared, and the views divide into as many");
            Line(report, "sheets as they need, in the order they were ticked, so no view is ever");
            Line(report, "left off. A sheet holding one view is NAMED FROM THE SHEET NAME TABLE, the");
            Line(report, "user's own file first and the shipped one second, because four of DM-11's");
            Line(report, "eight sheet names differ from their view names and upper casing wrote the");
            Line(report, "wrong four. Only a view type neither file holds falls back to the view");
            Line(report, "name upper cased, and the panel says derived beside it when it does.");
            Line(report, "Its number is BUILT: the view code, then the plot's own identifier with");
            Line(report, "its dash dropped, DM-42 fronting 010DM42, then a sheet letter when the");
            Line(report, "code holds several sheets, counting the ones the plot already has so a");
            Line(report, "second run continues rather than collides. Nothing is set per plot and");
            Line(report, "nothing is ever read off the model's own numbers, which are copies on");
            Line(report, "every plot but one: the identifier is in the number, so no two plots can");
            Line(report, "collide and nothing runs out. Sheets numbered under the old marker");
            Line(report, "scheme keep their numbers beside these. Name and number can both be");
            Line(report, "overwritten, the sheet lines above say which were built and which were");
            Line(report, "typed, and a sheet still short of either is refused rather than guessed.");
            Line(report, "A SHEET HAS NO SCALE OF ITS OWN. The Scale a sheet shows is a readout of");
            Line(report, "the views placed on it, and each view's scale comes from its own view");
            Line(report, "template. Nothing here sets a scale anywhere, and the viewport lines");
            Line(report, "above are where to check what each view came out at.");
            Line(report, "Where a view sits is worked out from the size of the title");
            Line(report, "block placed on the sheet, which is read off that placed block and not off");
            Line(report, "the type, because Sheet Width and Sheet Height only exist once one is");
            Line(report, "placed. Reading the type is why three sheets came out empty. A sheet whose");
            Line(report, "size still cannot be read is refused rather than made empty. A view");
            Line(report, "already sitting on another sheet is refused rather than moved, because it");
            Line(report, "belongs to whoever put it there.");
            Line(report, "THE VIEWS DIVIDE THE DRAWING AREA rather than the whole sheet, because the");
            Line(report, "title strip down the right hand edge is not somewhere a view may sit. How");
            Line(report, "much of the width that strip takes is the tool's own setting, and the sheet");
            Line(report, "lines above say so against each sheet, because a placed title block reports");
            Line(report, "its Sheet Width and its Sheet Height and nothing about where its strip");
            Line(report, "begins. Dividing the whole sheet is what ran wide views across the strip.");
            Line(report, "A SCHEDULE IS PLACED BY ITS TOP LEFT CORNER and a viewport by its centre.");
            Line(report, "Both are handed a centre, so a schedule is measured after it is placed and");
            Line(report, "moved until its centre is the one it was asked for. Before that it landed");
            Line(report, "half its own size right and down, off the edge of the sheet on a long one.");
            Line(report, "A schedule Revit gives no bounding box for cannot be measured, so it is");
            Line(report, "left where it landed and named above rather than moved by a guess.");
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
            report.Append(text).Append(ScanReport.LineEnd);
        }
    }
}
