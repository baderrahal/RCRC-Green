using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Sheet numbers are built: the view code, the plot's marker, then a sheet letter when
    /// the code holds several sheets.
    ///
    /// Every expected number below is written out by hand from the two measured models.
    /// FP-39 on NG03, marker 001, reads 010001A to 010001D and 200001. DM-11 on NG05,
    /// marker Q, reads 010QE to 010QH, 200Q, 400Q, 600QC and 600QD. The old rules that read
    /// a plot letter off the plot's own numbers and stepped free numbers off the ones in use
    /// are gone, because every other plot in NG05 carries only copy numbers and both rules
    /// answered nothing there.
    /// </summary>
    public class SheetNumberRunTests
    {
        private static readonly string[] DmElevenNumbers =
        {
            "010QE", "010QF", "010QG", "010QH", "200Q", "400Q", "600QC", "600QD"
        };

        private static string[] Built(SheetNumberRun run, int howMany)
        {
            var numbers = new List<string>();
            for (int at = 0; at < howMany; at++) numbers.Add(run.Next().Number);
            return numbers.ToArray();
        }

        /// <summary>
        /// FP-39 as the model holds it: four 010 sheets lettered from A, and the single 200
        /// sheet bare, because a code holding one sheet carries no letter.
        /// </summary>
        [Fact]
        public void AFreshPlotIsNumberedTheWayFpThirtyNineIs()
        {
            Assert.Equal(
                new[] { "010001A", "010001B", "010001C", "010001D" },
                Built(new SheetNumberRun("010", "001", new string[0], 4), 4));

            Assert.Equal(
                new[] { "200001" },
                Built(new SheetNumberRun("200", "001", new string[0], 1), 1));
        }

        /// <summary>
        /// Several sheets under an empty code are lettered from the first. Bare is only for a
        /// code whose one sheet is its whole set.
        /// </summary>
        [Fact]
        public void TwoNewSheetsUnderOneCodeAreBothLettered()
        {
            Assert.Equal(
                new[] { "600RA", "600RB" },
                Built(new SheetNumberRun("600", "R", new string[0], 2), 2));
        }

        /// <summary>
        /// A second run continues after the letters the plot already holds. DM-11 holds 600QC
        /// and 600QD, so two more 600 sheets read E and F rather than colliding with C.
        /// </summary>
        [Fact]
        public void ASecondRunContinuesAfterThePlotsOwnLetters()
        {
            Assert.Equal(
                new[] { "600QE", "600QF" },
                Built(new SheetNumberRun("600", "Q", DmElevenNumbers, 2), 2));

            Assert.Equal(
                new[] { "010QI" },
                Built(new SheetNumberRun("010", "Q", DmElevenNumbers, 1), 1));
        }

        /// <summary>
        /// A bare number holds the first letter's place. DM-11's 200Q stays as it is, because
        /// renaming the model's own sheet is not this tool's to do, and the next 200 sheet
        /// reads B.
        /// </summary>
        [Fact]
        public void ABareNumberOccupiesTheFirstLettersPlace()
        {
            Assert.Equal(
                new[] { "200QB" },
                Built(new SheetNumberRun("200", "Q", DmElevenNumbers, 1), 1));

            Assert.Equal(
                new[] { "200QD" },
                Built(new SheetNumberRun("200", "Q", new[] { "200Q", "200QC" }, 1), 1));
        }

        /// <summary>
        /// A number typed on the panel occupies its slot the same way a model number does, so
        /// a typed 010001A pushes the first built one to B.
        /// </summary>
        [Fact]
        public void ATypedNumberPushesTheBuiltOnesPastIt()
        {
            Assert.Equal(
                new[] { "010001B", "010001C", "010001D" },
                Built(new SheetNumberRun("010", "001", new[] { "010001A" }, 3), 3));
        }

        /// <summary>
        /// A copy number occupies nothing. 010QE Copy 001 is what Revit writes when a sheet
        /// is duplicated, and counting it would letter a fresh plot as if it already had
        /// sheets.
        /// </summary>
        [Fact]
        public void ACopyNumberOccupiesNoSlot()
        {
            Assert.Equal(
                new[] { "010Q" },
                Built(new SheetNumberRun(
                    "010", "Q", new[] { "010QE Copy 001", "400Q Copy 009" }, 1), 1));
        }

        [Fact]
        public void RunningOutOfLettersIsSaidRatherThanInvented()
        {
            SheetNumberProposal past = new SheetNumberRun(
                "200", "Q", new[] { "200QZ" }, 1).Next();

            Assert.False(past.Offered);
            Assert.Equal(
                "Every letter to Z after 200Q is used, so no number could be built. Type the "
                + "number.",
                past.WhyNot);
        }

        [Fact]
        public void ARunNeedsItsCodeAndItsMarker()
        {
            Assert.Throws<ArgumentException>(
                () => new SheetNumberRun(string.Empty, "Q", new string[0], 1));
            Assert.Throws<ArgumentException>(
                () => new SheetNumberRun("200", string.Empty, new string[0], 1));
        }

        /// <summary>
        /// The reasons a row gets no built number, word for word. The no-marker one names the
        /// plot, because it is also what the run refusal carries and the fix is in step 1.
        /// </summary>
        [Fact]
        public void TheReasonsNameThePlotAndThePlace()
        {
            Assert.Equal(
                "No marker is set for DM-16 in step 1, so no number could be built. Set it "
                + "there or type the number.",
                SheetNumberRun.NoMarkerWords("DM-16"));

            Assert.Equal(
                "This sheet's views carry different codes, so no single code can front its "
                + "number. Type the number.",
                SheetNumberRun.MixedCodesWords());
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
