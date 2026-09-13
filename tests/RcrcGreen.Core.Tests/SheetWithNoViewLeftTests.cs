using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// A sheet whose every view this same run refused is not made.
    ///
    /// From the run of 2026-09-13 08:51. Five views were refused before the run, each "No scope
    /// box is named DM-02". Their five sheets were created anyway, each recorded as "was made
    /// without the view, because that view is not in the model and was not marked to be made".
    /// Five empty sheets, and five numbers now taken in the model.
    /// </summary>
    public class SheetWithNoViewLeftTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");

        /// <summary>
        /// DM-02 has no scope box, so its views are refused, and its sheet asks for them.
        /// </summary>
        private static RunPlan OnDmZeroTwo(params ViewType[] onTheSheet)
        {
            return RunPlan.Of(
                new[] { new PlotViewKey("DM-02", Overall), new PlotViewKey("DM-02", General) },
                new[] { "DM-02" },
                null,
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(
                        onTheSheet,
                        1,
                        RunFixture.Row("DM-02", "010DM02A", "OVERALL KEYPLAN", onTheSheet, 1))
                });
        }

        [Fact]
        public void TheSheetIsRefusedAndTheViewItWasWaitingForIsNamed()
        {
            RunPlan plan = OnDmZeroTwo(Overall);

            Assert.Equal(0, plan.CountOf(RunItemKind.Sheet));

            RunRefusal refused = Assert.Single(
                plan.Refusals, one => one.Kind == RunRefusalKind.SheetHasNoViewLeft);

            Assert.Equal(
                "No sheet was made for DM-02, because this run refused the view it carries, "
                    + "DM-02-(010) Overall Plan. It would have come out empty under a number "
                    + "nothing could reuse.",
                refused.Because);
        }

        [Fact]
        public void EveryViewRefusedMeansTheSheetIsRefusedAndAllOfThemAreNamed()
        {
            RunPlan plan = OnDmZeroTwo(Overall, General);

            RunRefusal refused = Assert.Single(
                plan.Refusals, one => one.Kind == RunRefusalKind.SheetHasNoViewLeft);

            Assert.Contains("every view it carries", refused.Because);
            Assert.Contains("DM-02-(010) Overall Plan", refused.Because);
            Assert.Contains("DM-02-(200) General Arrangement Layout", refused.Because);
        }

        /// <summary>
        /// A sheet with no views BY DESIGN is untouched. The cover page carries none and both
        /// measured models number their title sheets, so it is a real sheet.
        /// </summary>
        [Fact]
        public void ACoverPageWithNoViewsIsStillMade()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-02", Overall) },
                new[] { "DM-02" },
                null,
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(
                        null,
                        1,
                        RunFixture.Row("DM-02", "010DM02A", "TITLE SHEET"))
                });

            Assert.Equal(1, plan.CountOf(RunItemKind.Sheet));
            Assert.DoesNotContain(
                plan.Refusals, one => one.Kind == RunRefusalKind.SheetHasNoViewLeft);
        }

        /// <summary>
        /// One view refused out of two leaves the sheet something to carry, so it is still
        /// made and the writer names what did not go on it.
        /// </summary>
        [Fact]
        public void ASheetKeepingOneOfItsTwoViewsIsStillMade()
        {
            // DM-11 has its scope box and DM-02 does not, so only DM-02's view is refused.
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-11", Overall), new PlotViewKey("DM-11", General) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(
                        new[] { Overall, General },
                        2,
                        RunFixture.Row(
                            "DM-11", "010DM11A", "OVERALL KEYPLAN",
                            new[] { Overall, General }, 2))
                });

            Assert.Equal(1, plan.CountOf(RunItemKind.Sheet));
            Assert.DoesNotContain(
                plan.Refusals, one => one.Kind == RunRefusalKind.SheetHasNoViewLeft);
        }

        /// <summary>
        /// A view already in the model is not refused, so a sheet carrying only that one is
        /// made. This is the case that would break if the rule asked whether the run MADE the
        /// view rather than whether it refused it.
        /// </summary>
        [Fact]
        public void ASheetCarryingAViewTheModelAlreadyHoldsIsMade()
        {
            RunPlan plan = RunPlan.Of(
                null,
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(
                        new[] { Overall },
                        1,
                        RunFixture.Row("DM-11", "010DM11A", "OVERALL KEYPLAN", new[] { Overall }, 1))
                },
                new[] { new PlotViewKey("DM-11", Overall) });

            Assert.Equal(1, plan.CountOf(RunItemKind.Sheet));
            Assert.DoesNotContain(
                plan.Refusals, one => one.Kind == RunRefusalKind.SheetHasNoViewLeft);
        }

        [Fact]
        public void TheSummaryCountsTheseUnderTheirOwnReason()
        {
            Assert.Equal(
                "on a sheet whose every view this run refused",
                RunSummary.ReasonInWords(RunRefusalKind.SheetHasNoViewLeft));

            Assert.Contains(
                RunSummary.ByReason(OnDmZeroTwo(Overall)),
                one => one.Kind == RunRefusalKind.SheetHasNoViewLeft && one.HowMany == 1);
        }
    }
}
