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
            int titleBlockTypes = 6,
            int sheetsDescribed = 0,
            int sheetsAsked = 0,
            int sheetsIncomplete = 0,
            int rowsIncomplete = 0,
            RunPlan plan = null)
        {
            return PanelSteps.Of(
                readOnce, plotsInModel, first, last, plotsInRange, plotsTicked,
                typesTicked, typesInModel, marked, titleBlockTypes, sheetsDescribed,
                sheetsAsked, sheetsIncomplete, rowsIncomplete, plan);
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
        public void TheSheetHeaderCountsWhatWasAddedAndWhatWillBeMade()
        {
            Assert.Equal(
                "4  SHEETS   1 described, 3 to make",
                After(sheetsDescribed: 1, sheetsAsked: 3).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   1 described, 1 to make",
                After(sheetsDescribed: 1, sheetsAsked: 1).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   1 described, none to make yet",
                After(sheetsDescribed: 1, sheetsAsked: 0).For(PanelStep.Sheets).Header);

            Assert.Equal(
                "4  SHEETS   none added",
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
        /// One sentence for a model with no plots, on step 1 and on the grid, naming all four
        /// sources the list is the union of. The panel held a second copy written out, and
        /// both still named two sources after the registry made it four.
        /// </summary>
        [Fact]
        public void AModelWithNoPlotsNamesAllFourSourcesInOneSentence()
        {
            StepState plots = After(plotsInModel: 0, first: "", last: "", plotsInRange: 0, plotsTicked: 0)
                .For(PanelStep.Plots);

            Assert.Equal(
                "This model holds no plots. No view name, no PRX_Plot_ID on a view or an element "
                + "and no scope box gives one.",
                plots.WhyNot);
            Assert.Equal(PanelSteps.NoPlotsInTheModel, plots.WhyNot);
            Assert.StartsWith(PanelSteps.NoPlotsInTheModel, PanelSteps.NothingToDraw(true, true, false));
        }

        /// <summary>
        /// Six messages the panel used to format itself, against the rule that every summary
        /// and reason lives here with a test. Each expected line is written out by hand.
        /// </summary>
        [Fact]
        public void TheMessagesThePanelUsedToFormatItselfLiveHere()
        {
            Assert.Equal("No plots in this model.", PanelSteps.PlotsLine(true, 0, 0, 0));
            Assert.Equal(
                "15 of 17 plots in range ticked, 160 in the model. Untick one to leave it out of "
                + "the counts and out of anything that writes.",
                PanelSteps.PlotsLine(false, 15, 17, 160));

            Assert.Equal(
                "42 views across 17 ticked plots. Only C is written.",
                PanelSteps.ScopeBoxLine(42, 17));
            Assert.Equal(
                "1 view across 1 ticked plot. Only C is written.",
                PanelSteps.ScopeBoxLine(1, 1));

            Assert.Equal(
                "No plots are ticked, so there is nothing to run.",
                PanelSteps.NoPlotsTicked("run"));
            Assert.Equal(
                "No plots are ticked, so there is nothing to assign.",
                PanelSteps.NoPlotsTicked("assign"));

            Assert.Equal(
                "Nothing is marked and no sheet can be made. Click an empty cell in step 3, or add "
                + "a sheet in step 4 and give it views, a name and a number.",
                PanelSteps.NothingToRunYet);

            Assert.Equal(
                "3 marked. Marking records intent and changes nothing until Run.",
                PanelSteps.MarkedLine(3));
        }

        [Fact]
        public void TheGridSaysWhyItHasNoRowsInTheWordsOfTheState()
        {
            Assert.Equal("Reading the model.", PanelSteps.NothingToDraw(false, true, false));
            Assert.Equal(
                "This model holds no plots. No view name, no PRX_Plot_ID on a view or an element "
                + "and no scope box gives one. Open the model you meant and press Refresh.",
                PanelSteps.NothingToDraw(true, true, false));
            Assert.Equal(
                "Pick a prefix in step 1. From and To fill themselves with the plots under it, "
                + "and the grid follows.",
                PanelSteps.NothingToDraw(true, false, false));
            Assert.Equal(
                "No plots in that range. Widen From and To in step 1.",
                PanelSteps.NothingToDraw(true, false, true));
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
        public void SheetsCannotBeUsedWhenTheModelHoldsNoTitleBlockType()
        {
            StepState sheets = After(titleBlockTypes: 0).For(PanelStep.Sheets);

            Assert.False(sheets.Usable);
            Assert.Contains("holds no title block types", sheets.WhyNot);
        }

        [Fact]
        public void RunCannotBeUsedUntilSomethingIsAskedFor()
        {
            StepState run = After().For(PanelStep.Run);

            Assert.False(run.Usable);
            Assert.Contains("Mark a cell in step 3", run.WhyNot);
        }

        /// <summary>
        /// Sheets added but none with a title block. Saying so on the Run step beats letting
        /// somebody press it and read the refusal afterwards.
        /// </summary>
        [Fact]
        public void RunSaysWhichSheetsStillNeedATitleBlock()
        {
            StepState run = After(sheetsDescribed: 2, sheetsIncomplete: 2).For(PanelStep.Run);

            Assert.False(run.Usable);
            Assert.Equal(
                "2 sheets in step 4 still need a title block, so no sheet can be made yet. "
                + "Finish that in step 4, or mark a cell in step 3.",
                run.WhyNot);

            Assert.Equal(
                "1 sheet in step 4 still needs a title block, so no sheet can be made yet. "
                + "Finish that in step 4, or mark a cell in step 3.",
                After(sheetsDescribed: 1, sheetsIncomplete: 1).For(PanelStep.Run).WhyNot);
        }

        /// <summary>
        /// A sheet described with its title block and its views, whose rows still lack names.
        /// It used to have nothing marked, nothing asked and nothing incomplete, so Run told
        /// somebody who had just added a sheet to add a sheet.
        /// </summary>
        [Fact]
        public void RunSaysFinishTheRowsWhenASheetHasItsTitleBlockAndRowsLackNames()
        {
            StepState run = After(sheetsDescribed: 1, rowsIncomplete: 3).For(PanelStep.Run);

            Assert.False(run.Usable);
            Assert.Equal(
                "3 rows in step 4 still need a name or a number, so no sheet can be made yet. "
                + "Finish that in step 4, or mark a cell in step 3.",
                run.WhyNot);

            Assert.Equal(
                "1 row in step 4 still needs a name or a number, so no sheet can be made yet. "
                + "Finish that in step 4, or mark a cell in step 3.",
                After(sheetsDescribed: 1, rowsIncomplete: 1).For(PanelStep.Run).WhyNot);
        }

        [Fact]
        public void RunNamesBothWhenASheetLacksItsTitleBlockAndRowsLackNames()
        {
            StepState run = After(sheetsDescribed: 2, sheetsIncomplete: 1, rowsIncomplete: 2)
                .For(PanelStep.Run);

            Assert.Equal(
                "1 sheet in step 4 still needs a title block, and 2 rows still need a name or "
                + "a number, so no sheet can be made yet. Finish that in step 4, or mark a cell "
                + "in step 3.",
                run.WhyNot);
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
            PanelSteps steps = After(typesTicked: 4, marked: 2, sheetsDescribed: 1, sheetsAsked: 1);

            Assert.Equal(PanelStep.ViewTypes, steps.OpenAfter(PanelStep.Plots));
            Assert.Equal(PanelStep.Mark, steps.OpenAfter(PanelStep.ViewTypes));
            Assert.Equal(PanelStep.Sheets, steps.OpenAfter(PanelStep.Mark));
            Assert.Equal(PanelStep.Run, steps.OpenAfter(PanelStep.Sheets));
        }

        [Fact]
        public void AStepThatCannotBeUsedIsSkippedOver()
        {
            PanelSteps steps = After(typesTicked: 4, marked: 2, titleBlockTypes: 0);

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
            Assert.Null(After(typesTicked: 0, titleBlockTypes: 0).OpenAfter(PanelStep.ViewTypes));
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
