using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Sheet numbers are built: the view code, then the plot identifier with its dash
    /// dropped, then a sheet letter when the code holds several sheets.
    ///
    /// Every expected number below is written out by hand from the user's worked example.
    /// DM-42 reads 010DM42A TITLE SHEET, 010DM42B LIST OF DRAWINGS, 200DM42 GENERAL
    /// ARRANGEMENT LAYOUT, 400DM42 LANDSCAPE CROSS SECTION, 600DM42A HARDSCAPE SCHEDULES
    /// and 600DM42B SOFTSCAPE SCHEDULES. The per-plot marker this replaced is deleted
    /// rather than left beside it: the identifier is in the number, so nothing is set in
    /// step 1, nothing is reserved and no two plots can collide.
    /// </summary>
    public class SheetNumberRunTests
    {
        private static readonly string[] DmFortyTwoNumbers =
        {
            "010DM42A", "010DM42B", "200DM42", "400DM42", "600DM42A", "600DM42B"
        };

        private static string[] Built(SheetNumberRun run, int howMany)
        {
            var numbers = new List<string>();
            for (int at = 0; at < howMany; at++) numbers.Add(run.Next().Number);
            return numbers.ToArray();
        }

        /// <summary>
        /// The worked example: two 010 sheets lettered from A, the single 200 sheet bare
        /// because a code holding one sheet carries no letter, and the two 600 schedules
        /// lettered again.
        /// </summary>
        [Fact]
        public void AFreshPlotIsNumberedTheWayTheWorkedExampleReads()
        {
            Assert.Equal(
                new[] { "010DM42A", "010DM42B" },
                Built(new SheetNumberRun("010", "DM-42", new string[0], 2), 2));

            Assert.Equal(
                new[] { "200DM42" },
                Built(new SheetNumberRun("200", "DM-42", new string[0], 1), 1));

            Assert.Equal(
                new[] { "600DM42A", "600DM42B" },
                Built(new SheetNumberRun("600", "DM-42", new string[0], 2), 2));
        }

        /// <summary>
        /// A second run continues after the letters the plot already holds. DM-42 holding
        /// 600DM42A and 600DM42B gets C and D for two more 600 sheets rather than a
        /// collision on A.
        /// </summary>
        [Fact]
        public void ASecondRunContinuesAfterThePlotsOwnLetters()
        {
            Assert.Equal(
                new[] { "600DM42C", "600DM42D" },
                Built(new SheetNumberRun("600", "DM-42", DmFortyTwoNumbers, 2), 2));

            Assert.Equal(
                new[] { "010DM42C" },
                Built(new SheetNumberRun("010", "DM-42", DmFortyTwoNumbers, 1), 1));
        }

        /// <summary>
        /// A bare number holds the first letter's place. The model's own 200DM42 stays as
        /// it is, because renaming a sheet the model holds is not this tool's to do, and
        /// the next 200 sheet reads B.
        /// </summary>
        [Fact]
        public void ABareNumberOccupiesTheFirstLettersPlace()
        {
            Assert.Equal(
                new[] { "200DM42B" },
                Built(new SheetNumberRun("200", "DM-42", DmFortyTwoNumbers, 1), 1));

            Assert.Equal(
                new[] { "400DM42D" },
                Built(new SheetNumberRun(
                    "400", "DM-42", new[] { "400DM42", "400DM42C" }, 1), 1));
        }

        /// <summary>
        /// A number typed on the panel occupies its slot the same way a model number does,
        /// so a typed 010DM42A pushes the first built one to B.
        /// </summary>
        [Fact]
        public void ATypedNumberPushesTheBuiltOnesPastIt()
        {
            Assert.Equal(
                new[] { "010DM42B", "010DM42C" },
                Built(new SheetNumberRun("010", "DM-42", new[] { "010DM42A" }, 2), 2));
        }

        /// <summary>
        /// The two measured models are numbered the old marker way, 010QE on NG05 and
        /// 010001A on NG03. Neither starts with the new front, so neither holds a slot:
        /// old sheets are never renumbered, the built numbers letter from A beside them,
        /// and the two schemes side by side on one plot is the user's decision rather
        /// than a fault. An exact collision is still refused, by SheetNumbers.FaultIn
        /// below and again at Run.
        /// </summary>
        [Fact]
        public void OldSchemeNumbersHoldNoSlotAndAreNeverRenumbered()
        {
            Assert.Equal(
                new[] { "010DM11A", "010DM11B" },
                Built(new SheetNumberRun(
                    "010", "DM-11",
                    new[] { "010QE", "010QF", "010QG", "010QH", "200Q" }, 2), 2));

            Assert.Equal(
                new[] { "010FP39" },
                Built(new SheetNumberRun(
                    "010", "FP-39",
                    new[] { "010001A", "010001B", "010001C", "010001D" }, 1), 1));
        }

        /// <summary>
        /// A copy number occupies nothing. 010DM42A Copy 001 is what Revit writes when a
        /// sheet is duplicated, and counting it would letter a fresh plot as if it already
        /// had sheets.
        /// </summary>
        [Fact]
        public void ACopyNumberOccupiesNoSlot()
        {
            Assert.Equal(
                new[] { "010DM42" },
                Built(new SheetNumberRun(
                    "010", "DM-42", new[] { "010DM42A Copy 001" }, 1), 1));
        }

        /// <summary>
        /// DM-4 and DM-42 share every character up to DM-4's end, so DM-42's numbers start
        /// with DM-4's front. The tail there is DM-42's extra digit, not a sheet letter,
        /// and it holds no slot on DM-4's run.
        /// </summary>
        [Fact]
        public void ALongerPlotsNumberHoldsNoSlotOnAShorterPlotsRun()
        {
            Assert.Equal(
                new[] { "010DM4" },
                Built(new SheetNumberRun(
                    "010", "DM-4", new[] { "010DM42", "010DM42A" }, 1), 1));
        }

        [Fact]
        public void RunningOutOfLettersIsSaidRatherThanInvented()
        {
            SheetNumberProposal past = new SheetNumberRun(
                "200", "DM-42", new[] { "200DM42Z" }, 1).Next();

            Assert.False(past.Offered);
            Assert.Equal(
                "Every letter to Z after 200DM42 is used, so no number could be built. Type "
                + "the number.",
                past.WhyNot);
        }

        /// <summary>
        /// The stem rule, on its own because step 1 shows it beside every plot. It answers
        /// empty for anything that is not a plot identifier, the trim matching what
        /// PlotId.TryRead accepts, so nothing builds a number on a name the model never
        /// gave.
        /// </summary>
        [Fact]
        public void TheStemIsTheIdentifierWithItsDashDropped()
        {
            Assert.Equal("DM42", SheetNumberRun.StemOf("DM-42"));
            Assert.Equal("NS6", SheetNumberRun.StemOf(" NS-6 "));
            Assert.Equal(string.Empty, SheetNumberRun.StemOf("dm-42"));
            Assert.Equal(string.Empty, SheetNumberRun.StemOf("Q"));
            Assert.Equal(string.Empty, SheetNumberRun.StemOf(string.Empty));
            Assert.Equal(string.Empty, SheetNumberRun.StemOf(null));
        }

        /// <summary>
        /// The old scheme's marker, Q, is not a plot identifier and cannot start a run any
        /// more. Refusing it here is what stops a second numbering scheme creeping back in
        /// through a caller.
        /// </summary>
        [Fact]
        public void ARunNeedsItsCodeAndItsPlot()
        {
            Assert.Throws<ArgumentException>(
                () => new SheetNumberRun(string.Empty, "DM-42", new string[0], 1));
            Assert.Throws<ArgumentException>(
                () => new SheetNumberRun("200", string.Empty, new string[0], 1));
            Assert.Throws<ArgumentException>(
                () => new SheetNumberRun("200", "Q", new string[0], 1));
        }

        /// <summary>
        /// The reasons a row gets no built number, word for word. Both go under the box
        /// and into the run refusal.
        /// </summary>
        [Fact]
        public void TheReasonsSayWhatStopsTheNumber()
        {
            Assert.Equal(
                "This sheet's views carry different codes, so no single code can front its "
                + "number. Type the number.",
                SheetNumberRun.MixedCodesWords());

            Assert.Equal(
                "This sheet holds no views, so there is no view code to build its number "
                + "from. Type the number.",
                SheetNumberRun.NoViewsWords());
        }
    }

    /// <summary>
    /// Sheet numbers that will work, and the ones that will not.
    ///
    /// Three sheets were refused in one run, every one with "a sheet numbered 010EA is already
    /// in this model". The refusal was right. The offer was wrong: the dropdown listed the
    /// numbers already in use, so every entry in it was certain to be rejected.
    /// </summary>
    public class SheetNumberFaultTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        private static readonly string[] InTheModel = { "010EA", "L-211" };

        private static SheetBatch Batch(string sheetName, params string[] plotAndNumber)
        {
            var rows = new List<SheetToMake>();
            for (int at = 0; at + 1 < plotAndNumber.Length; at += 2)
            {
                rows.Add(RunFixture.Row(
                    plotAndNumber[at], plotAndNumber[at + 1], sheetName, new[] { General }));
            }

            return RunFixture.Batch(new[] { General }, 1, rows.ToArray());
        }

        [Fact]
        public void ANumberAlreadyInTheModelIsCaught()
        {
            var problems = SheetNumbers.Problems(
                new[] { Batch("GA", "DM-11", "010EA", "DM-12", "L-999") },
                InTheModel);

            SheetNumberProblem only = Assert.Single(problems);

            Assert.Equal("DM-11", only.PlotId);
            Assert.Equal("010EA", only.SheetNumber);
            Assert.Equal(SheetNumberFault.AlreadyInTheModel, only.Fault);
            Assert.Equal(
                "DM-11 010EA for GA, a sheet in this model already has that number",
                only.InWords());
        }

        /// <summary>
        /// Two rows given one number. The first would be created and the second refused, which
        /// is the same fault arriving a second later.
        /// </summary>
        [Fact]
        public void TwoRowsGivenOneNumberAreBothCaught()
        {
            var problems = SheetNumbers.Problems(
                new[] { Batch("GA", "DM-11", "L-500", "DM-12", "L-500") },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.All(problems, one =>
                Assert.Equal(SheetNumberFault.UsedTwiceInThisRun, one.Fault));
        }

        /// <summary>
        /// Two sheets described separately, both asking for one number, clash just as hard as
        /// two rows of one sheet do. They go into one model.
        /// </summary>
        [Fact]
        public void TwoDescribedSheetsAskingForOneNumberClashToo()
        {
            var problems = SheetNumbers.Problems(
                new[]
                {
                    Batch("GA", "DM-11", "L-500"),
                    Batch("LIST OF DRAWINGS", "DM-11", "L-500")
                },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.Equal(
                new[] { "GA", "LIST OF DRAWINGS" },
                problems.Select(one => one.SheetName).ToArray());
        }

        [Fact]
        public void AFreeNumberIsNoProblemAtAll()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[] { Batch("GA", "DM-11", "L-500", "DM-12", "L-501") },
                InTheModel));
        }

        [Fact]
        public void ARowWithNoNumberIsNotAClash()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[] { Batch("GA", "DM-11", "", "DM-12", "   ") },
                InTheModel));
        }

        /// <summary>
        /// An unusable definition makes no sheets, so its rows cannot clash with anything.
        /// </summary>
        [Fact]
        public void RowsOfAnUnusableDefinitionAreNotCounted()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[]
                {
                    RunFixture.BatchMissingItsType(
                        RunFixture.Row("DM-11", "010EA", "GA", new[] { General }))
                },
                InTheModel));
        }

        /// <summary>
        /// Already in the model wins over used twice, because both are true of a number typed
        /// twice against a sheet that exists and the model is the one somebody goes and looks at.
        /// </summary>
        [Fact]
        public void AlreadyInTheModelIsSaidBeforeUsedTwice()
        {
            var problems = SheetNumbers.Problems(
                new[] { Batch("GA", "DM-11", "010EA", "DM-12", "010EA") },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.All(problems, one =>
                Assert.Equal(SheetNumberFault.AlreadyInTheModel, one.Fault));
        }

        /// <summary>
        /// The same answer the panel puts under one box, so the line and the summary can never
        /// disagree.
        /// </summary>
        [Fact]
        public void OneNumberCanBeAskedAboutOnItsOwn()
        {
            Assert.Equal(
                SheetNumberFault.AlreadyInTheModel,
                SheetNumbers.FaultIn("010EA", InTheModel, new[] { "010EA" }));

            Assert.Equal(
                SheetNumberFault.UsedTwiceInThisRun,
                SheetNumbers.FaultIn("L-500", InTheModel, new[] { "L-500", "L-500" }));

            Assert.Equal(
                SheetNumberFault.None,
                SheetNumbers.FaultIn("L-500", InTheModel, new[] { "L-500" }));

            Assert.Equal(
                SheetNumberFault.None,
                SheetNumbers.FaultIn(string.Empty, InTheModel, new string[0]));
        }

        [Fact]
        public void TheRunSummarySaysHowManyWillBeRefused()
        {
            Assert.Equal(string.Empty, SheetNumbers.InWords(0));

            Assert.Equal(
                "1 sheet number will be refused, and that sheet will not be made. It is marked "
                + "in step 4.",
                SheetNumbers.InWords(1));

            Assert.Equal(
                "3 sheet numbers will be refused, and those sheets will not be made. They are "
                + "marked in step 4.",
                SheetNumbers.InWords(3));
        }

        [Fact]
        public void EachFaultHasItsOwnWords()
        {
            Assert.Equal(
                "a sheet in this model already has that number",
                SheetNumbers.FaultInWords(SheetNumberFault.AlreadyInTheModel));

            Assert.Equal(
                "another sheet in this run asks for the same number",
                SheetNumbers.FaultInWords(SheetNumberFault.UsedTwiceInThisRun));
        }
    }

    /// <summary>
    /// The number check at Run, against the numbers read off the model as the run is worked
    /// out. Three runs in a row asked Revit for numbers a previous run had created, because
    /// the only check lived on the panel and read its last snapshot, and every refusal
    /// arrived from Revit inside the transaction instead of on the plan.
    /// </summary>
    public class RunTimeNumberGuardTests
    {
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");

        [Fact]
        public void ANumberTheModelHoldsIsRefusedBeforeAnythingOpens()
        {
            RunPlan plan = RunPlan.Of(
                null,
                new[] { "DM-11" },
                null,
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(new[] { KeyPlan }, 1,
                        RunFixture.Row("DM-11", "010QE", "PROJECT LOCATION KEY PLAN", new[] { KeyPlan }))
                },
                sheetNumbersInUse: new[] { "010QE" });

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("a sheet in this model already has that number", only.Because);
            Assert.Contains("read off the model as the run was worked out", only.Because);
        }

        [Fact]
        public void TwoRowsAskingForOneNumberAreBothRefusedAtRun()
        {
            RunPlan plan = RunPlan.Of(
                null,
                new[] { "DM-11", "DM-12" },
                null,
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(new[] { KeyPlan }, 1,
                        RunFixture.Row("DM-11", "010QA", "PROJECT LOCATION KEY PLAN", new[] { KeyPlan }),
                        RunFixture.Row("DM-12", "010QA", "PROJECT LOCATION KEY PLAN", new[] { KeyPlan }))
                },
                sheetNumbersInUse: new string[0]);

            Assert.Empty(plan.Items);
            Assert.Equal(2, plan.Refusals.Count);
            Assert.All(plan.Refusals, one =>
                Assert.Contains("another sheet in this run asks for the same number", one.Because));
        }

        [Fact]
        public void AFreeNumberStillBecomesAnItem()
        {
            RunPlan plan = RunPlan.Of(
                null,
                new[] { "DM-11" },
                null,
                null,
                null,
                null,
                new[]
                {
                    RunFixture.Batch(new[] { KeyPlan }, 1,
                        RunFixture.Row("DM-11", "010QI", "PROJECT LOCATION KEY PLAN", new[] { KeyPlan }))
                },
                sheetNumbersInUse: new[] { "010QE", "010QF" });

            Assert.Empty(plan.Refusals);
            RunItem only = Assert.Single(plan.Items);
            Assert.Equal("010QI PROJECT LOCATION KEY PLAN", only.Name);
        }
    }

    /// <summary>
    /// Annotation crop on a new plan view.
    ///
    /// Copying it from the sibling was last round's answer and it did not work. The report said
    /// so plainly: DM-11-(010) Overall Plan was set up from PL-17-(010) Overall Plan, which has
    /// it off, so the new view got it off.
    /// </summary>
    public class AnnotationCropChoiceTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");

        private static SiblingView Sibling(string name, bool annotationCrop)
        {
            return new SiblingView(
                name, Overall, SiblingKind.Plan, "TYPE", "TEMPLATE", "Level 1",
                new ViewCrop(true, false, annotationCrop));
        }

        /// <summary>
        /// On whatever the sibling has. That is the whole change.
        /// </summary>
        [Fact]
        public void ItIsOnEvenWhenTheSiblingHasItOff()
        {
            Assert.True(AnnotationCropChoice.ForAPlanView(Sibling("PL-17-(010) Overall Plan", false)).On);
            Assert.True(AnnotationCropChoice.ForAPlanView(Sibling("DM-16-(010) Overall Plan", true)).On);
            Assert.True(AnnotationCropChoice.ForAPlanView(null).On);
        }

        /// <summary>
        /// The real case, word for word. It says whose setting it is, the same way the section
        /// depth does, so nobody reads it as something found in the model.
        /// </summary>
        [Fact]
        public void ItSaysWhoseSettingItIsAndWhatTheSiblingHad()
        {
            Assert.Equal(
                "Annotation crop is on, which is the tool's setting on every plan view rather "
                + "than anything read off a view. PL-17-(010) Overall Plan has it off, and a "
                + "view with it off draws the section markers of neighbouring plots through "
                + "itself.",
                AnnotationCropChoice.ForAPlanView(Sibling("PL-17-(010) Overall Plan", false)).InWords());
        }

        [Fact]
        public void ASiblingThatAlreadyHasItOnIsSaidPlainly()
        {
            AnnotationCropChoice choice =
                AnnotationCropChoice.ForAPlanView(Sibling("DM-16-(010) Overall Plan", true));

            Assert.True(choice.TheSiblingHadItOn);
            Assert.Equal(
                "Annotation crop is on, which is the tool's setting on every plan view rather "
                + "than anything read off a view. DM-16-(010) Overall Plan has it on as well.",
                choice.InWords());
        }

        [Fact]
        public void WithNoSiblingItStillSaysWhoseSettingItIs()
        {
            Assert.Equal(
                "Annotation crop is on, which is the tool's setting on every plan view rather "
                + "than anything read off a view.",
                AnnotationCropChoice.ForAPlanView(null).InWords());
        }
    }
}
