using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What the pane draws once a run has happened.
    ///
    /// Every number here comes off the outcome and none of it off the plan. The report that
    /// listed four views as created and as not created at the same time read one from each,
    /// and this is the place that bit.
    ///
    /// Every expected string is written out by hand.
    /// </summary>
    public class RunResultTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private const string Report = @"C:\RCRC\RCRC-Green\reports\run-NG05-1355.txt";

        private static readonly string[] Written = { Report };

        [Fact]
        public void TheWorkedCountIsTheOutcomesCreatedCount()
        {
            RunOutcome outcome = RunFixture.Outcome(made: new[]
            {
                new RunItem("DM-11", General, RunItemKind.PlanView),
                new RunItem("DM-12", General, RunItemKind.PlanView),
                RunFixture.SheetItem("DM-11", "200DM11", "GENERAL ARRANGEMENT LAYOUT")
            });

            RunResult result = RunResult.Of(outcome, Written);

            Assert.Equal(3, result.Worked);
            Assert.Equal(outcome.CreatedCount, result.Worked);
        }

        /// <summary>
        /// Not created counts the refused and the ones left in the model together, which is
        /// what the line of them is built from, so the count and the list cannot part.
        /// </summary>
        [Fact]
        public void TheFailedCountIsTheOutcomesNotCreatedCount()
        {
            RunOutcome outcome = RunFixture.Outcome(
                made: new[] { new RunItem("DM-11", General, RunItemKind.PlanView) },
                refused: new[]
                {
                    new RunRefusal("DM-13", General, "DM-13 has no scope box.",
                        RunRefusalKind.NoScopeBox),
                    new RunRefusal("DM-14", General, "DM-14 already holds that view.",
                        RunRefusalKind.AlreadyInTheModel)
                },
                leftBehind: new[]
                {
                    new RunRefusal("DM-15", Hardscape,
                        "It was created short of a column and the delete was refused.")
                });

            RunResult result = RunResult.Of(outcome, Written);

            Assert.Equal(3, result.Failed);
            Assert.Equal(outcome.NotCreatedCount, result.Failed);
            Assert.Equal(result.Failed, result.FailedLines.Count);
            Assert.Equal("1 created, 3 not created.", result.CountsInWords);
        }

        /// <summary>
        /// The refused first, then the ones left in the model, and the second group is marked
        /// because those are in the model right now.
        /// </summary>
        [Fact]
        public void TheFailedLinesAreTheRefusedAndThenTheLeftBehind()
        {
            RunResult result = RunResult.Of(
                RunFixture.Outcome(
                    refused: new[]
                    {
                        new RunRefusal("DM-13", General, "DM-13 has no scope box.",
                            RunRefusalKind.NoScopeBox)
                    },
                    leftBehind: new[]
                    {
                        new RunRefusal("DM-15", Hardscape, "The delete was refused.")
                    }),
                Written);

            Assert.Equal(
                new[]
                {
                    "DM-13-(200) General Arrangement Layout. DM-13 has no scope box.",
                    "DM-15-(600) HARDSCAPE SCHEDULE. The delete was refused."
                },
                result.FailedLines.Select(one => one.InWords()).ToArray());

            Assert.Equal(
                new[] { false, true },
                result.FailedLines.Select(one => one.LeftInTheModel).ToArray());
        }

        /// <summary>
        /// A caller that gave no sentence still gets one, worded from the category rather than
        /// written a second time here. A line with a name and nothing after it reads as a
        /// thing that failed for no reason.
        /// </summary>
        [Fact]
        public void EveryFailedLineCarriesAReasonAndNoneIsBlank()
        {
            RunResult result = RunResult.Of(
                RunFixture.Outcome(refused: new[]
                {
                    new RunRefusal("DM-13", General, string.Empty, RunRefusalKind.NoScopeBox),
                    new RunRefusal("DM-14", General, "   ", RunRefusalKind.Unsaid),
                    new RunRefusal(string.Empty, null, string.Empty, RunRefusalKind.AlreadyInTheModel)
                }),
                Written);

            Assert.Equal(
                new[]
                {
                    "Refused on a sub plot with no scope box.",
                    "Refused for a reason the run did not name.",
                    "Refused already in the model."
                },
                result.FailedLines.Select(one => one.Because).ToArray());

            Assert.Equal(
                "Something the run did not name",
                result.FailedLines[2].Name);

            Assert.DoesNotContain(result.FailedLines, one => one.InWords().Trim().Length == 0);
        }

        /// <summary>
        /// Empty in a run that behaved. A run that produces one has a bug in this code, which
        /// is worth shouting about rather than blaming a model for.
        /// </summary>
        [Fact]
        public void ARunThatBehavedRaisesNothing()
        {
            RunResult result = RunResult.Of(
                RunFixture.Outcome(
                    made: new[] { new RunItem("DM-11", General, RunItemKind.PlanView) },
                    refused: new[] { new RunRefusal("DM-13", General, "No scope box.") }),
                Written);

            Assert.Equal(string.Empty, result.Alarm);
            Assert.Equal(string.Empty, result.Loud);
        }

        [Fact]
        public void SomethingCountedBothWaysIsRaisedLoudly()
        {
            RunOutcome outcome = RunFixture.Outcome(
                made: new[] { new RunItem("DM-11", General, RunItemKind.PlanView) },
                refused: new[] { new RunRefusal("DM-11", General, "Revit refused it.") });

            RunResult result = RunResult.Of(outcome, Written);

            Assert.Single(outcome.BothWays);
            Assert.Equal(
                "THIS IS A BUG IN THE TOOL, NOT IN THE MODEL. 1 thing is counted as created "
                + "and as not created: DM-11-(200) General Arrangement Layout. Nothing here "
                + "can be trusted until that is fixed.",
                result.Alarm);
        }

        /// <summary>
        /// The one list that gets worse the longer nobody reads it, in the words the status
        /// line has always used for it.
        /// </summary>
        [Fact]
        public void AScheduleLeftInTheModelIsShouted()
        {
            RunResult one = RunResult.Of(
                RunFixture.Outcome(leftBehind: new[]
                {
                    new RunRefusal("DM-15", Hardscape, "The delete was refused.")
                }),
                Written);

            RunResult two = RunResult.Of(
                RunFixture.Outcome(leftBehind: new[]
                {
                    new RunRefusal("DM-15", Hardscape, "The delete was refused."),
                    new RunRefusal("DM-16", Hardscape, "The delete was refused.")
                }),
                Written);

            Assert.Equal(
                "1 WRONG SCHEDULE IS IN THE MODEL AND MUST BE DELETED BY HAND.", one.Loud);
            Assert.Equal(
                "2 WRONG SCHEDULES ARE IN THE MODEL AND MUST BE DELETED BY HAND.", two.Loud);
        }

        /// <summary>
        /// A run nobody confirmed wrote nothing, and that is still worth drawing. An empty
        /// area says the pane broke.
        /// </summary>
        [Fact]
        public void ARunThatMadeNothingStillHasSomethingToDraw()
        {
            RunResult result = RunResult.Of(RunOutcome.NothingWasWritten(), Written);

            Assert.Equal(0, result.Worked);
            Assert.Equal(0, result.Failed);
            Assert.Empty(result.FailedLines);
            Assert.Equal("0 created, 0 not created.", result.CountsInWords);
            Assert.Equal("Report at " + Report + ".", result.Where);
        }

        /// <summary>
        /// No pointer file beside the add-in means no report and no folder, said in the words
        /// ReportPlaces already owns rather than in a second sentence of the pane's own.
        /// </summary>
        [Fact]
        public void WithNoReportPathThePaneShowsTheOneSentenceAndOffersNoButtons()
        {
            RunResult result = RunResult.Of(
                RunFixture.Outcome(made: new[]
                {
                    new RunItem("DM-11", General, RunItemKind.PlanView)
                }),
                new List<string>());

            Assert.False(result.HasReport);
            Assert.Equal(string.Empty, result.ReportPath);
            Assert.Equal(string.Empty, result.FolderPath);
            Assert.Equal(ReportPlaces.Written(new List<string>()), result.Where);
            Assert.StartsWith("NO REPORT WAS WRITTEN, because reports-folder.txt", result.Where);
        }

        [Fact]
        public void TheFolderIsTheOneTheReportLandedIn()
        {
            RunResult result = RunResult.Of(RunOutcome.NothingWasWritten(), Written);

            Assert.True(result.HasReport);
            Assert.Equal(Report, result.ReportPath);
            Assert.Equal(@"C:\RCRC\RCRC-Green\reports", result.FolderPath);
        }

        /// <summary>
        /// An empty path in the list is not a path. The write returns nothing rather than a
        /// blank when there is no pointer file, and a blank reaching here would draw two
        /// buttons that open nothing.
        /// </summary>
        [Fact]
        public void AnEmptyPathIsNotAReport()
        {
            RunResult result = RunResult.Of(
                RunOutcome.NothingWasWritten(), new[] { string.Empty, null });

            Assert.False(result.HasReport);
        }
    }
}
