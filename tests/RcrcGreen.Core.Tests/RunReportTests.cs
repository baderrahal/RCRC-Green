using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The report is the only record a person reads afterwards, so what it says about a thing
    /// left in the model is worth more than the thing itself.
    /// </summary>
    public class RunReportTests
    {
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        private static readonly DateTime When = new DateTime(2026, 9, 8, 14, 7, 0);

        private static RunPlan OnePlanView()
        {
            return RunFixture.Of(
                new[] { new PlotViewKey("DM-14", General) },
                new[] { "DM-14" },
                new[] { "DM-14" },
                new ViewType[0],
                new ViewType[0]);
        }

        private static string Report(params RunRefusal[] leftBehind)
        {
            return RunReport.Write(
                OnePlanView(),
                RunFixture.Outcome(leftBehind: leftBehind),
                "RCRC_NG05",
                When,
                true);
        }

        [Fact]
        public void AScheduleStillInTheModelIsNamedUnderItsOwnHeading()
        {
            string written = Report(new RunRefusal(
                "DM-14", Hardscape, "STILL IN THE MODEL, named DM-14-(600) HARDSCAPE SCHEDULE."));

            Assert.Contains("CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND, 1", written);
            Assert.Contains("  DM-14-(600) HARDSCAPE SCHEDULE. STILL IN THE MODEL, named", written);
        }

        /// <summary>
        /// The banner is above the sections. Somebody who stops reading after the first screen
        /// still finds out, which is the whole reason it is there.
        /// </summary>
        [Fact]
        public void TheBannerComesBeforeTheSectionThatNamesIt()
        {
            string written = Report(new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL."));

            int banner = written.IndexOf("READ THIS FIRST", StringComparison.Ordinal);
            int section = written.IndexOf(RunReport.StillInTheModel, StringComparison.Ordinal);

            Assert.True(banner > 0);
            Assert.True(banner < section);
        }

        [Fact]
        public void OneReadsAsOneAndTwoReadAsTwo()
        {
            Assert.Contains(
                "READ THIS FIRST. 1 wrong schedule is in the model and could not",
                Report(new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL.")));

            Assert.Contains(
                "READ THIS FIRST. 2 wrong schedules are in the model and could not",
                Report(
                    new RunRefusal("DM-14", Hardscape, "STILL IN THE MODEL."),
                    new RunRefusal("DM-15", Hardscape, "STILL IN THE MODEL.")));
        }

        /// <summary>
        /// The usual run. No banner at all, because a warning that shows every time is a
        /// warning nobody reads on the day it means something.
        /// </summary>
        [Fact]
        public void ARunThatLeftNothingBehindHasNoBanner()
        {
            string written = Report();

            Assert.DoesNotContain("READ THIS FIRST", written);
            Assert.Contains(RunReport.StillInTheModel + ", 0", written);
            Assert.Contains("  none", written);
        }

        /// <summary>
        /// The fault the first real run showed. The plan held four plan views, the run made
        /// none of them, and the report printed the plan under PLAN VIEWS and the failures
        /// under NOT CREATED, so the same four names were in both.
        /// </summary>
        [Fact]
        public void APlanThatMadeNothingPrintsNothingAsCreated()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-12", General),
                    new PlotViewKey("DM-13", General),
                    new PlotViewKey("DM-14", General)
                },
                new[] { "DM-11", "DM-12", "DM-13", "DM-14" },
                new[] { "DM-11", "DM-12", "DM-13", "DM-14" },
                new ViewType[0],
                new ViewType[0]);

            RunOutcome outcome = RunFixture.Outcome(refused: plan.Items
                .Select(item => new RunRefusal(item.PlotId, item.Type, "Revit refused it."))
                .ToList());

            string written = RunReport.Write(plan, outcome, "RCRC_NG05", When, true);

            Assert.Equal(4, plan.Items.Count);
            Assert.Contains(RunReport.CreatedPlanViews + ", 0", written);
            Assert.Contains(RunReport.NotCreatedDuring + ", 4", written);
            Assert.Contains("0 were created and 4 were not.", written);
        }

        /// <summary>
        /// The check the round asked for, written against the file rather than against the
        /// object, because the file is what somebody reads. Every created section is held
        /// against every not-created section by name.
        ///
        /// It is run over a report built to be as awkward as a real one: four kinds created,
        /// three kinds of failure, and two names that differ only by their plot.
        /// </summary>
        [Fact]
        public void NothingAppearsAsBothCreatedAndNotCreated()
        {
            var made = new List<RunItem>
            {
                new RunItem("DM-11", General, RunItemKind.PlanView),
                new RunItem("DM-12", General, RunItemKind.PlanView),
                new RunItem("DM-11", new ViewType("400", "Landscape Cross Section"), RunItemKind.Section),
                new RunItem("DM-11", Hardscape, RunItemKind.Schedule),
                RunItem.ForSheet(new SheetRequest("DM-11", "L-201", "General Arrangement"))
            };

            var refused = new List<RunRefusal>
            {
                new RunRefusal("DM-13", General, "Revit refused it."),
                RunRefusal.ForSheet(new SheetRequest("DM-14", "L-204", string.Empty), "No sheet name.")
            };

            var leftBehind = new List<RunRefusal>
            {
                new RunRefusal("DM-15", Hardscape, "STILL IN THE MODEL.")
            };

            string written = RunReport.Write(
                RunFixture.Of(null, null, null, null, null),
                RunFixture.Outcome(made: made, refused: refused, leftBehind: leftBehind),
                "RCRC_NG05",
                When,
                true);

            List<string> createdNames = RunReport.CreatedHeadings
                .SelectMany(heading => Entries(written, heading))
                .ToList();

            List<string> notCreatedNames = RunReport.NotCreatedHeadings
                .SelectMany(heading => Entries(written, heading))
                .Select(NameOnly)
                .ToList();

            Assert.Equal(5, createdNames.Count);
            Assert.Equal(3, notCreatedNames.Count);
            Assert.Empty(createdNames.Intersect(notCreatedNames, StringComparer.Ordinal));
            Assert.DoesNotContain("CONTRADICTS ITSELF", written);
        }

        /// <summary>
        /// The same check with the fault put back on purpose. A test for a thing that cannot
        /// happen proves nothing until it has been watched failing, so this is the failure.
        /// </summary>
        [Fact]
        public void TheSameNameOnBothSidesIsCaughtAndSaidOutLoud()
        {
            var item = new RunItem("DM-11", General, RunItemKind.PlanView);

            RunOutcome outcome = RunFixture.Outcome(
                made: new[] { item },
                refused: new[] { new RunRefusal("DM-11", General, "Revit refused it.") });

            Assert.Equal(new[] { "DM-11-(200) General Arrangement Layout" }, outcome.BothWays.ToArray());

            string written = RunReport.Write(
                RunFixture.Of(null, null, null, null, null), outcome, "RCRC_NG05", When, true);

            List<string> createdNames = RunReport.CreatedHeadings
                .SelectMany(heading => Entries(written, heading))
                .ToList();

            List<string> notCreatedNames = RunReport.NotCreatedHeadings
                .SelectMany(heading => Entries(written, heading))
                .Select(NameOnly)
                .ToList();

            Assert.Single(createdNames.Intersect(notCreatedNames, StringComparer.Ordinal));
            Assert.Contains("THIS REPORT CONTRADICTS ITSELF AND IS A BUG IN THE TOOL", written);
        }

        [Fact]
        public void EveryKindOfThingMadeGetsItsOwnCountedSection()
        {
            var made = new List<RunItem>
            {
                new RunItem("DM-11", General, RunItemKind.PlanView),
                new RunItem("DM-11", new ViewType("400", "Landscape Cross Section"), RunItemKind.Section),
                new RunItem("DM-11", Hardscape, RunItemKind.Schedule),
                new RunItem("DM-12", Hardscape, RunItemKind.Schedule),
                RunItem.ForSheet(new SheetRequest("DM-11", "L-201", "General Arrangement"))
            };

            string written = RunReport.Write(
                RunFixture.Of(null, null, null, null, null),
                RunFixture.Outcome(made: made),
                "RCRC_NG05",
                When,
                true);

            Assert.Contains(RunReport.CreatedPlanViews + ", 1", written);
            Assert.Contains(RunReport.CreatedSections + ", 1", written);
            Assert.Contains(RunReport.CreatedSchedules + ", 2", written);
            Assert.Contains(RunReport.CreatedSheets + ", 1", written);
            Assert.Contains("  L-201 General Arrangement", written);
            Assert.Contains("  DM-11-(400) Landscape Cross Section", written);
        }

        [Fact]
        public void TheClosingNoteSaysWhatHappensWhenTheDeleteIsRefused()
        {
            string written = Report();

            Assert.Contains("deleted again inside the same", written);
            Assert.Contains("When Revit refuses that delete, the schedule is in", written);
            Assert.DoesNotContain("is not created at all", written);
        }

        /// <summary>
        /// The three lookups a new view needs used to be matched on names and both name matches
        /// were wrong. The note has to say where they come from now, or it explains a rule the
        /// tool stopped following.
        /// </summary>
        [Fact]
        public void TheClosingNoteSaysTheSetupComesFromASiblingView()
        {
            string written = Report();

            Assert.Contains("family type, its level and its view template all come from a view of the", written);
            Assert.DoesNotContain("No sheet was created and none can be yet", written);
            Assert.Contains("looks 10 metres", written);
        }

        [Fact]
        public void TheModelAndTheTimeAreAtTheTop()
        {
            string[] lines = Report().Split('\n').Select(one => one.TrimEnd('\r')).ToArray();

            Assert.Equal("RCRC GREEN, DRAWING SHEET RUN", lines[0]);
            Assert.Equal("RCRC_NG05", lines[1]);
            Assert.Equal("2026-09-08 14:07:00", lines[2]);
        }

        /// <summary>
        /// The entries under one heading, read out of the file the way a person reads it. A
        /// section ends at the blank line, and "  none" is not an entry.
        /// </summary>
        private static IEnumerable<string> Entries(string report, string heading)
        {
            string[] lines = report.Split('\n').Select(one => one.TrimEnd('\r')).ToArray();

            int at = Array.FindIndex(lines, line => line.StartsWith(heading + ", ", StringComparison.Ordinal));
            Assert.True(at >= 0, "The report has no section headed " + heading + ".");

            var found = new List<string>();
            for (int line = at + 1; line < lines.Length; line++)
            {
                if (lines[line].Length == 0) break;
                if (!lines[line].StartsWith("  ", StringComparison.Ordinal)) break;
                if (lines[line] == "  none") continue;

                found.Add(lines[line].Substring(2));
            }

            return found;
        }

        /// <summary>
        /// A refusal prints its name, then a full stop, then why. The name is the part that has
        /// to be held against the created sections.
        /// </summary>
        private static string NameOnly(string entry)
        {
            int stop = entry.IndexOf(". ", StringComparison.Ordinal);
            return stop < 0 ? entry : entry.Substring(0, stop);
        }
    }
}
