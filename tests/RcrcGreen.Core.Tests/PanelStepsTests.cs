using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What each step says while it is shut, and whether it can be used at all.
    ///
    /// Every expected string is written out by hand. Working one out with the same rule the
    /// code uses would only prove the rule agrees with itself.
    /// </summary>
    public class PanelStepsTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        /// <summary>
        /// A panel that has read a model, picked DM-11 to DM-28, and done nothing else. Each
        /// test moves one thing off this.
        /// </summary>
        private static PanelSteps After(
            bool readOnce = true,
            int plotsInModel = 160,
            string first = "DM-11",
            string last = "DM-28",
            int plotsInRange = 17,
            int plotsTicked = 17,
            int typesTicked = 0,
            int typesInModel = 84,
            int marked = 0,
            int sheetsInModel = 1385,
            string sourceSheet = "",
            int sheetsAsked = 0,
            RunPlan plan = null)
        {
            return PanelSteps.Of(
                readOnce, plotsInModel, first, last, plotsInRange, plotsTicked,
                typesTicked, typesInModel, marked, sheetsInModel, sourceSheet, sheetsAsked, plan);
        }

        [Fact]
        public void TheFiveStepsComeBackNumberedInOrder()
        {
            PanelSteps steps = After();

            Assert.Equal(
                new[] { 1, 2, 3, 4, 5 },
                steps.All.Select(one => one.Number).ToArray());

            Assert.Equal(
                new[] { "PLOTS", "VIEW TYPES", "MARK", "SHEETS", "RUN" },
                steps.All.Select(one => one.Title).ToArray());
        }

        /// <summary>
        /// The header is what somebody reads with the step shut, so it has to carry the state
        /// on its own.
        /// </summary>
        [Fact]
        public void TheHeaderCarriesTheNumberTheTitleAndTheSummary()
        {
            StepState plots = After().For(PanelStep.Plots);

            Assert.Equal("DM-11 to DM-28, 17 of 17 ticked", plots.Summary);
            Assert.Equal("1  PLOTS   DM-11 to DM-28, 17 of 17 ticked", plots.Header);
        }

        [Fact]
        public void AStepWithNothingToSayIsJustItsNumberAndTitle()
        {
            StepState mark = After(typesTicked: 0).For(PanelStep.Mark);

            Assert.False(mark.Usable);
            Assert.Equal(string.Empty, mark.Summary);
            Assert.Equal("3  MARK", mark.Header);
        }

        [Fact]
        public void TheViewTypeHeaderCountsTicksAgainstTheWholeList()
        {
            Assert.Equal(
                "2  VIEW TYPES   4 of 84 ticked",
                After(typesTicked: 4).For(PanelStep.ViewTypes).Header);
        }

        [Fact]
        public void OneMarkedReadsAsOneAndNoneSaysSo()
        {
            Assert.Equal("nothing marked", After(typesTicked: 1, marked: 0).For(PanelStep.Mark).Summary);
            Assert.Equal("1 marked", After(typesTicked: 1, marked: 1).For(PanelStep.Mark).Summary);
            Assert.Equal("7 marked", After(typesTicked: 1, marked: 7).For(PanelStep.Mark).Summary);
        }

        [Fact]
        public void TheSheetHeaderNamesTheSheetBeingCopied()
        {
            Assert.Equal(
                "4  SHEETS   copying L-201, 3 plots filled in",
                After(sourceSheet: "L-201", sheetsAsked: 3).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   copying L-201, 1 plot filled in",
                After(sourceSheet: "L-201", sheetsAsked: 1).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   copying L-201, no plot filled in",
                After(sourceSheet: "L-201", sheetsAsked: 0).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   no sheet picked to copy",
                After().For(PanelStep.Sheets).Header);
        }

        /// <summary>
        /// A step greyed out with no reason is worse than one that is not there at all, so
        /// every unusable step has to carry a line saying why.
        /// </summary>
        [Fact]
        public void EveryUnusableStepSaysWhyAndEveryUsableOneDoesNot()
        {
            foreach (StepState step in After(readOnce: false, plotsInModel: 0).All)
            {
                Assert.False(step.Usable);
                Assert.NotEqual(string.Empty, step.WhyNot);
            }

            foreach (StepState step in After(typesTicked: 4, marked: 2).All.Where(one => one.Usable))
            {
                Assert.Equal(string.Empty, step.WhyNot);
            }
        }

        [Fact]
        public void NothingIsUsableBeforeTheModelIsRead()
        {
            PanelSteps steps = After(readOnce: false, plotsInModel: 0, first: "", last: "", plotsInRange: 0, plotsTicked: 0);

            Assert.All(steps.All, step => Assert.False(step.Usable));
            Assert.Contains("Nothing has been read yet", steps.For(PanelStep.Plots).WhyNot);
        }

        [Fact]
        public void AModelWithNoPlotsSaysSoRatherThanOfferingAnEmptyRange()
        {
            StepState plots = After(plotsInModel: 0, first: "", last: "", plotsInRange: 0, plotsTicked: 0)
                .For(PanelStep.Plots);

            Assert.False(plots.Usable);
            Assert.Contains("holds no plots", plots.WhyNot);
        }

        /// <summary>
        /// Everything below the plots acts on the plots in the range, so none of it means
        /// anything until there is one.
        /// </summary>
        [Fact]
        public void UntickingEveryPlotShutsTheStepsBelow()
        {
            PanelSteps steps = After(plotsTicked: 0);

            Assert.True(steps.For(PanelStep.Plots).Usable);
            Assert.False(steps.For(PanelStep.ViewTypes).Usable);
            Assert.False(steps.For(PanelStep.Mark).Usable);
            Assert.False(steps.For(PanelStep.Sheets).Usable);
        }

        [Fact]
        public void MarkingCannotBeUsedUntilAViewTypeIsTicked()
        {
            StepState mark = After(typesTicked: 0).For(PanelStep.Mark);

            Assert.False(mark.Usable);
            Assert.Contains("Tick at least one view type in step 2", mark.WhyNot);
        }

        [Fact]
        public void SheetsCannotBeUsedWhenTheModelHoldsNone()
        {
            StepState sheets = After(sheetsInModel: 0).For(PanelStep.Sheets);

            Assert.False(sheets.Usable);
            Assert.Contains("holds no sheets", sheets.WhyNot);
        }

        [Fact]
        public void RunCannotBeUsedUntilSomethingIsAskedFor()
        {
            StepState run = After().For(PanelStep.Run);

            Assert.False(run.Usable);
            Assert.Contains("Mark a cell in step 3", run.WhyNot);
        }

        /// <summary>
        /// A sheet asked for with no source picked cannot be made, and saying so on the Run
        /// step is better than letting somebody press it and read the refusal afterwards.
        /// </summary>
        [Fact]
        public void RunSaysToPickASourceSheetWhenOneIsAskedForWithoutIt()
        {
            StepState run = After(sheetsAsked: 2).For(PanelStep.Run);

            Assert.False(run.Usable);
            Assert.Contains("Pick the sheet to copy in step 4", run.WhyNot);
        }

        [Fact]
        public void RunCarriesTheCountsOnceThereIsSomethingToMake()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", General) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                new ViewType[0],
                new ViewType[0]);

            StepState run = After(typesTicked: 1, marked: 1, plan: plan).For(PanelStep.Run);

            Assert.True(run.Usable);
            Assert.Equal("5  RUN   1 plan view", run.Header);
        }

        /// <summary>
        /// Finishing a step opens the next one that can be used, so a model with no sheets
        /// takes somebody from marking to running rather than to a step that would only say
        /// why it is shut.
        /// </summary>
        [Fact]
        public void FinishingAStepOpensTheNextUsableOne()
        {
            PanelSteps steps = After(typesTicked: 4, marked: 2, sourceSheet: "L-201", sheetsAsked: 1);

            Assert.Equal(PanelStep.ViewTypes, steps.OpenAfter(PanelStep.Plots));
            Assert.Equal(PanelStep.Mark, steps.OpenAfter(PanelStep.ViewTypes));
            Assert.Equal(PanelStep.Sheets, steps.OpenAfter(PanelStep.Mark));
            Assert.Equal(PanelStep.Run, steps.OpenAfter(PanelStep.Sheets));
        }

        [Fact]
        public void AStepThatCannotBeUsedIsSkippedOver()
        {
            PanelSteps steps = After(typesTicked: 4, marked: 2, sheetsInModel: 0);

            Assert.False(steps.For(PanelStep.Sheets).Usable);
            Assert.Equal(PanelStep.Run, steps.OpenAfter(PanelStep.Mark));
        }

        /// <summary>
        /// Nothing further to open leaves the panel where it is. Shutting everything would look
        /// like it had lost its place.
        /// </summary>
        [Fact]
        public void WithNothingFurtherToOpenTheAnswerIsNothing()
        {
            Assert.Null(After().OpenAfter(PanelStep.Run));
            Assert.Null(After(typesTicked: 0, sheetsInModel: 0).OpenAfter(PanelStep.ViewTypes));
        }

        /// <summary>
        /// A second read of the same model lands where the work is, rather than throwing
        /// somebody back to the top of a range they already picked.
        /// </summary>
        [Fact]
        public void TheFirstUnfinishedStepIsWhereAReadLands()
        {
            Assert.Equal(PanelStep.Plots, After(plotsTicked: 0).FirstUnfinished);
            Assert.Equal(PanelStep.ViewTypes, After().FirstUnfinished);
            Assert.Equal(PanelStep.Mark, After(typesTicked: 4).FirstUnfinished);
            Assert.Equal(PanelStep.Sheets, After(typesTicked: 4, marked: 2).FirstUnfinished);
        }

        /// <summary>
        /// Nothing usable lands on step 1, because that is where the reason is written. A shut
        /// Run with no explanation next to it tells somebody nothing.
        /// </summary>
        [Fact]
        public void APanelWithNothingReadLandsOnStepOneWhereTheReasonIs()
        {
            Assert.Equal(PanelStep.Plots, After(readOnce: false, plotsInModel: 0).FirstUnfinished);
        }

        [Fact]
        public void AskingForAStepThatIsNotOneRefuses()
        {
            Assert.Throws<System.ArgumentException>(() => After().For((PanelStep)9));
        }

        /// <summary>
        /// Before a range is picked the header still says how much there is to pick from,
        /// because an empty summary next to 160 plots reads as an empty model.
        /// </summary>
        [Fact]
        public void WithNoRangePickedTheHeaderStillSaysWhatIsThere()
        {
            StepState plots = After(first: "", last: "", plotsInRange: 0, plotsTicked: 0)
                .For(PanelStep.Plots);

            Assert.True(plots.Usable);
            Assert.False(plots.Done);
            Assert.Equal("1  PLOTS   160 in the model, none picked", plots.Header);
        }
    }
}
