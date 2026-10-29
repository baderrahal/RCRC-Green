using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE PANE WORKS IN FOUR STEPS, ONE AT A TIME.** Bader's layout of 17 September: Setup,
    /// Read, Plots, Create, where Plots was called Tick until October. Which step may be
    /// worked on is decided here so a test can reach the copy that runs, and the pane draws it
    /// and decides none of it.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class KpiStepsTests
    {
        /// <summary>
        /// Nothing set at all, which is what a person sees the first time the pane is opened
        /// on a machine where nothing has been browsed for.
        /// </summary>
        private static KpiSteps NothingSet()
        {
            return KpiSteps.Of(false, false, false, false, false, false, null, 0, 0, false, FolderLayout.Flat);
        }

        /// <summary>
        /// Every folder and file set, a model open, and nothing read yet.
        /// </summary>
        private static KpiSteps AllSetNothingRead()
        {
            return KpiSteps.Of(true, true, true, true, true, true, null, 0, 0, false, FolderLayout.Flat);
        }

        /// <summary>
        /// A read of 154 plots landed and nothing is ticked.
        /// </summary>
        private static KpiSteps ReadNothingTicked()
        {
            return KpiSteps.Of(true, true, true, true, true, true, 154, 0, 0, false, FolderLayout.Flat);
        }

        /// <summary>
        /// One workbook ticked and 154 plots ticked, which is the state a press goes from.
        /// </summary>
        private static KpiSteps Ticked()
        {
            return KpiSteps.Of(true, true, true, true, true, true, 154, 1, 154, false, FolderLayout.Flat);
        }

        /// <summary>
        /// **NOTHING SET AT ALL.** Setup can be worked on, because it is where everything below
        /// is fixed. The other three cannot, and each says why in its own words rather than
        /// being a dead cell.
        /// </summary>
        [Fact]
        public void WithNothingSetOnlySetupCanBeWorkedOn()
        {
            KpiSteps steps = NothingSet();

            Assert.True(steps.For(KpiStep.Setup).Usable, "Setup is where every reason below is written, so it is always usable.");
            Assert.False(steps.For(KpiStep.Read).Usable);
            Assert.False(steps.For(KpiStep.Plots).Usable);
            Assert.False(steps.For(KpiStep.Create).Usable);

            Assert.False(steps.For(KpiStep.Setup).Done);

            Assert.Equal(
                "Set the templates and output folders first.",
                steps.For(KpiStep.Read).WhyNot);

            Assert.Equal("Read the model first.", steps.For(KpiStep.Plots).WhyNot);
            Assert.Equal("No model open", steps.For(KpiStep.Create).WhyNot);

            Assert.Equal(KpiStep.Setup, steps.FirstUnfinished);
        }

        /// <summary>
        /// **THE LINE NAMES ALL FIVE, AND ONLY TWO OF THEM STOP ANYTHING.** The forms folder,
        /// the street reference and the plot list are each recorded in `kpi-rules.md` as a note
        /// and never a refusal, so they are said and they gate nothing.
        /// </summary>
        [Fact]
        public void TheLineNamesEveryFolderAndFileThatIsNotSet()
        {
            IReadOnlyList<string> notSet = KpiSteps.NotSet(false, false, false, false, false, true);

            Assert.Equal(5, notSet.Count);
            Assert.Equal("the templates folder is not set", notSet[0]);
            Assert.Equal("the output folder is not set", notSet[1]);
            Assert.Equal(
                "the forms folder is not set, so no PDF will be written", notSet[2]);
            Assert.Equal(
                "the street reference file is not set, so every street plot's road width and "
                + "total length will be left empty",
                notSet[3]);
            Assert.Equal(
                "the plot list file is not set, so Plot list cannot be pressed and the "
                + "report prints no plot list section",
                notSet[4]);
        }

        /// <summary>
        /// **THE THREE THAT ARE NOTES DO NOT STOP THE READ.** A press with no forms folder, no
        /// street reference and no plot list writes every workbook, so holding step 2 shut over
        /// one of them would hold back a press this tool is willing to make.
        /// </summary>
        [Fact]
        public void TheThreeNotesAreNamedAndStopNothing()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, false, false, false, true, null, 0, 0, false, FolderLayout.Flat);

            Assert.True(steps.For(KpiStep.Setup).Done);
            Assert.True(steps.For(KpiStep.Read).Usable);
            Assert.Equal(string.Empty, steps.For(KpiStep.Read).WhyNot);

            Assert.Equal("3 not set", steps.For(KpiStep.Setup).Summary);

            Assert.Equal(
                "Not set: the forms folder is not set, so no PDF will be written. the street "
                + "reference file is not set, so every street plot's road width and total "
                + "length will be left empty. the plot list file is not set, so Plot list "
                + "cannot be pressed and the report prints no plot list section.",
                KpiSteps.StillNotSet(KpiSteps.NotSet(true, true, false, false, false, true)));
        }

        /// <summary>
        /// **EVERYTHING SET AND NOTHING READ.** Setup is done, Read can be worked on and is not,
        /// and Plots still says what to do.
        /// </summary>
        [Fact]
        public void WithEverythingSetAndNothingReadTheReadStepIsTheOneToWorkOn()
        {
            KpiSteps steps = AllSetNothingRead();

            Assert.True(steps.For(KpiStep.Setup).Done);

            // **THE HEADER CARRIES A COUNT AND THE STEP CARRIES THE NAMING.** Both used to
            // carry the whole sentence, which is the same words twice on one screen.
            Assert.Equal("all set", steps.For(KpiStep.Setup).Summary);
            Assert.Equal(
                "Every folder and file this pane is pointed at is set.",
                KpiSteps.StillNotSet(KpiSteps.NotSet(true, true, true, true, true, true)));

            Assert.True(steps.For(KpiStep.Read).Usable);
            Assert.False(steps.For(KpiStep.Read).Done);
            Assert.Equal("not read", steps.For(KpiStep.Read).Summary);

            Assert.False(steps.For(KpiStep.Plots).Usable);
            Assert.False(steps.For(KpiStep.Create).Usable);

            Assert.Equal(KpiStep.Read, steps.FirstUnfinished);
        }

        /// <summary>
        /// **A MODEL NOBODY HAS OPENED SAYS SO RATHER THAN SAYING NOT READ.** A read that cannot
        /// happen and a read that has not happened are different things to go and do.
        /// </summary>
        [Fact]
        public void WithNoModelOpenTheReadStepSaysSo()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, false, null, 0, 0, false, FolderLayout.Flat);

            Assert.Equal("no model is open", steps.For(KpiStep.Read).Summary);
            Assert.True(steps.For(KpiStep.Read).Usable);
        }

        /// <summary>
        /// **ONE READ, NOTHING TICKED.** Plots can be worked on, Create cannot, and Create names
        /// the first thing missing, in the one order the button below it reads too.
        /// </summary>
        [Fact]
        public void WithOneReadAndNothingTickedCreateNamesTheFirstMissingTick()
        {
            KpiSteps steps = ReadNothingTicked();

            Assert.True(steps.For(KpiStep.Read).Done);
            Assert.Equal("154 plots read", steps.For(KpiStep.Read).Summary);

            Assert.True(steps.For(KpiStep.Plots).Usable);
            Assert.False(steps.For(KpiStep.Plots).Done);
            Assert.Equal("nothing ticked", steps.For(KpiStep.Plots).Summary);

            Assert.False(steps.For(KpiStep.Create).Usable);
            Assert.Equal(
                "No asset type selected",
                steps.For(KpiStep.Create).WhyNot);

            Assert.Equal(KpiStep.Plots, steps.FirstUnfinished);
        }

        /// <summary>
        /// **A WORKBOOK TICKED WITH NO PLOT NAMES ONLY THE PLOT.** Naming the workbook too would
        /// send somebody to tick a box that is already ticked.
        /// </summary>
        [Fact]
        public void AWorkbookTickedWithNoPlotNamesOnlyThePlot()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, true, 154, 1, 0, false, FolderLayout.Flat);

            Assert.False(steps.For(KpiStep.Create).Usable);
            Assert.Equal("No plot selected", steps.For(KpiStep.Create).WhyNot);
            Assert.Equal("1 asset type, 0 plots", steps.For(KpiStep.Plots).Summary);
        }

        /// <summary>
        /// **PLOTS TICKED WITH NO WORKBOOK NAMES ONLY THE WORKBOOK.**
        /// </summary>
        [Fact]
        public void PlotsTickedWithNoWorkbookNameOnlyTheWorkbook()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, true, 154, 0, 154, false, FolderLayout.Flat);

            Assert.False(steps.For(KpiStep.Create).Usable);
            Assert.Equal("No asset type selected", steps.For(KpiStep.Create).WhyNot);
            Assert.Equal("0 asset types, 154 plots", steps.For(KpiStep.Plots).Summary);
        }

        /// <summary>
        /// **SOMETHING TICKED OPENS CREATE.** Bader's rule: Create cannot be worked on before
        /// something is ticked, and the Create button's own gate is a template and a plot, so
        /// the step and the button agree rather than being two rules.
        /// </summary>
        [Fact]
        public void WithSomethingTickedCreateCanBeWorkedOn()
        {
            KpiSteps steps = Ticked();

            Assert.True(steps.For(KpiStep.Plots).Done);
            Assert.Equal("1 asset type, 154 plots", steps.For(KpiStep.Plots).Summary);

            Assert.True(steps.For(KpiStep.Create).Usable);
            Assert.Equal(string.Empty, steps.For(KpiStep.Create).WhyNot);
            Assert.False(steps.For(KpiStep.Create).Done);

            Assert.Equal(KpiStep.Create, steps.FirstUnfinished);
        }

        /// <summary>
        /// **A FINISHED PRESS MARKS CREATE DONE**, so the bar says the press happened rather
        /// than looking exactly as it did before it.
        /// </summary>
        [Fact]
        public void AFinishedPressMarksCreateDone()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, true, 154, 1, 154, true, FolderLayout.Flat);

            Assert.True(steps.For(KpiStep.Create).Done);
            Assert.Equal("pressed", steps.For(KpiStep.Create).Summary);
            Assert.Equal(KpiStep.Create, steps.FirstUnfinished);
        }

        /// <summary>
        /// **THE CELL IS THE NUMBER, THE NAME AND THE MARK**, short enough for four of them
        /// across a pane about 300 pixels wide. The summary is a line of its own under the bar.
        ///
        /// **THE MARK IS THE WORD `done` AND NOT A TICK CHARACTER.** The tick is code point
        /// 2713 in hex, and `.claude/hooks/writing-check.sh` line 47 refuses everything from
        /// 2600 to 27BF as an emoji, so a commit carrying one would not land. This file names
        /// the code point and carries no such character, which is why the assertion below
        /// builds the tick rather than writing it.
        /// </summary>
        [Fact]
        public void TheBarCellIsTheNumberTheNameAndTheWordDone()
        {
            KpiSteps steps = Ticked();

            // **THE CELL CARRIES THE NUMBER AND THE NAME AND NOTHING ELSE.** `3 Tick done` in
            // a cell about 70 pixels wide trimmed the mark away first, so a finished step read
            // SHORTER than an unfinished one. The mark is its own line under the name.
            Assert.Equal("1 Setup", steps.For(KpiStep.Setup).Cell);
            Assert.Equal("2 Read", steps.For(KpiStep.Read).Cell);
            Assert.Equal("3 Plots", steps.For(KpiStep.Plots).Cell);
            Assert.Equal("4 Create", steps.For(KpiStep.Create).Cell);

            Assert.Equal("done", steps.For(KpiStep.Setup).Mark);
            Assert.Equal(string.Empty, steps.For(KpiStep.Create).Mark);

            foreach (KpiStepState step in steps.All)
            {
                Assert.DoesNotContain(((char)0x2713).ToString(), step.Cell);
            }
        }

        /// <summary>
        /// **A STEP THAT CANNOT BE WORKED ON CANNOT BE DONE.** A model closing leaves the ticks
        /// standing and takes the read away, and the bar read `3 Tick done` beside a step
        /// saying the model had not been read, with step 4 open on the strength of it.
        /// </summary>
        [Fact]
        public void AStepThatCannotBeWorkedOnIsNotMarkedDone()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, true, null, 1, 154, false, FolderLayout.Flat);

            Assert.False(steps.For(KpiStep.Plots).Usable);
            Assert.False(
                steps.For(KpiStep.Plots).Done,
                "A workbook row and 154 plots are ticked and nothing has been read, so Plots "
                + "cannot be worked on, and a mark on a step nobody can open is a claim about "
                + "a step that is shut.");

            Assert.Equal("3 Plots", steps.For(KpiStep.Plots).Cell);
            Assert.Equal(string.Empty, steps.For(KpiStep.Plots).Mark);

            Assert.False(
                steps.For(KpiStep.Create).Usable,
                "And step 4 must not open on the strength of ticks the read behind them has "
                + "gone.");

            Assert.Equal(KpiStep.Read, steps.FirstUnfinished);
        }

        /// <summary>
        /// Four steps, numbered one to four, in the order a person does them.
        /// </summary>
        [Fact]
        public void ThereAreFourStepsInOrder()
        {
            IReadOnlyList<KpiStepState> all = NothingSet().All;

            Assert.Equal(4, all.Count);
            Assert.Equal(new[] { 1, 2, 3, 4 }, all.Select(one => one.Number).ToArray());
            Assert.Equal(
                new[] { "Setup", "Read", "Plots", "Create" },
                all.Select(one => one.Title).ToArray());
        }

        /// <summary>
        /// **ONE PLOT READS AS ONE PLOT.** A count that says 1 plots is a count nobody wrote by
        /// hand, and this pane's whole job is exact wording.
        /// </summary>
        [Fact]
        public void OnePlotAndOneWorkbookReadInTheSingular()
        {
            KpiSteps steps = KpiSteps.Of(
                true, true, true, true, true, true, 1, 1, 1, false, FolderLayout.Flat);

            Assert.Equal("1 plot read", steps.For(KpiStep.Read).Summary);
            Assert.Equal("1 asset type, 1 plot", steps.For(KpiStep.Plots).Summary);
        }
    }
}
