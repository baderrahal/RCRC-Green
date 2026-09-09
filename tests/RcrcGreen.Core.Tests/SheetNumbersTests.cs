using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Sheet numbers that will work, and the ones that will not.
    ///
    /// Three sheets were refused in one run, every one with "a sheet numbered 010EA is already
    /// in this model". The refusal was right. The offer was wrong: the dropdown listed the
    /// numbers already in use, so every entry in it was certain to be rejected.
    ///
    /// Every expected number below is written out by hand from the real ones in that model.
    /// </summary>
    public class FreeSheetNumbersTests
    {
        [Fact]
        public void ANumberOffTheEndIsSteppedOnByOne()
        {
            Assert.Equal(new[] { "L-212" }, SheetNumbers.Free(new[] { "L-211" }).ToArray());
        }

        /// <summary>
        /// The three shapes this model really uses. 010EA has its digits at the front, so the
        /// last run of digits is the only one there and stepping it gives 011EA.
        /// </summary>
        [Fact]
        public void EveryShapeInTheModelSteppedOn()
        {
            var free = SheetNumbers.Free(new[] { "010EA", "010QE Copy 001", "L-211" });

            Assert.Contains("011EA", free);
            Assert.Contains("010QE Copy 002", free);
            Assert.Contains("L-212", free);
        }

        /// <summary>
        /// Padding is kept, so 010 gives 011 and not 11. A number that changed width would not
        /// sort with the rest of the set and would read as a different scheme.
        /// </summary>
        [Fact]
        public void TheWidthOfTheDigitsIsKept()
        {
            Assert.Equal(new[] { "L-002" }, SheetNumbers.Free(new[] { "L-001" }).ToArray());
            Assert.Equal(new[] { "L-100" }, SheetNumbers.Free(new[] { "L-099" }).ToArray());
        }

        /// <summary>
        /// The whole point. Every number offered is one no sheet in the model carries, so
        /// picking off the list can never be refused for being a duplicate.
        /// </summary>
        [Fact]
        public void NothingOfferedIsAlreadyInUse()
        {
            var inUse = new[] { "L-211", "L-212", "L-213", "010EA", "011EA" };

            var free = SheetNumbers.Free(inUse);

            Assert.NotEmpty(free);
            Assert.All(free, one => Assert.DoesNotContain(one, inUse));
        }

        [Fact]
        public void ARunOfTakenNumbersIsSteppedPast()
        {
            Assert.Equal(
                new[] { "L-214" },
                SheetNumbers.Free(new[] { "L-211", "L-212", "L-213" }).ToArray());
        }

        /// <summary>
        /// A number with no digits at all cannot be stepped on, so it offers nothing rather than
        /// having a digit invented on the end of it.
        /// </summary>
        [Fact]
        public void ANumberWithNoDigitsOffersNothing()
        {
            Assert.Empty(SheetNumbers.Free(new[] { "COVER" }));
            Assert.Empty(SheetNumbers.Free(new string[0]));
            Assert.Empty(SheetNumbers.Free(null));
        }

        [Fact]
        public void BlanksAreNotNumbers()
        {
            Assert.Empty(SheetNumbers.Free(new[] { "   ", string.Empty, null }));
        }
    }

    /// <summary>
    /// The letter that stands for the plot in its own sheet numbers.
    ///
    /// The numbers below are plot DM-11's real ones: 010QE, 010QF, 010QG, 010QH, 200Q, 400Q,
    /// 600QC, 600QD. Every expected letter is written by hand off that list.
    /// </summary>
    public class PlotLetterTests
    {
        private static readonly string[] DmElevenNumbers =
        {
            "010QE", "010QF", "010QG", "010QH", "200Q", "400Q", "600QC", "600QD"
        };

        [Fact]
        public void ThePlotLetterIsReadOffThePlotsOwnNumbers()
        {
            Assert.Equal("Q", SheetNumbers.PlotLetter(DmElevenNumbers));
        }

        /// <summary>
        /// A number that does not start with digits carries no letter to read, so L-211 and a
        /// bare word contribute nothing rather than a wrong answer.
        /// </summary>
        [Fact]
        public void ANumberNotStartingWithDigitsContributesNoLetter()
        {
            Assert.Equal("Q", SheetNumbers.PlotLetter(new[] { "L-211", "200Q", "COVER" }));
            Assert.Equal(string.Empty, SheetNumbers.PlotLetter(new[] { "L-211", "COVER" }));
        }

        /// <summary>
        /// 010QE Copy 001 still reads as Q, because the letter sits right after the leading
        /// digits and the copy suffix is behind it.
        /// </summary>
        [Fact]
        public void ACopyNumberStillGivesItsLetter()
        {
            Assert.Equal("Q", SheetNumbers.PlotLetter(new[] { "010QE Copy 001" }));
        }

        /// <summary>
        /// Disagreeing numbers give no letter at all. Picking a side would put a wrong number
        /// into a drawing register.
        /// </summary>
        [Fact]
        public void DisagreeingNumbersGiveNoLetter()
        {
            Assert.Equal(string.Empty, SheetNumbers.PlotLetter(new[] { "010QE", "200R" }));
            Assert.Equal(string.Empty, SheetNumbers.PlotLetter(new string[0]));
            Assert.Equal(string.Empty, SheetNumbers.PlotLetter(null));
        }

        [Fact]
        public void ALowerCaseLetterIsReadAsItsCapital()
        {
            Assert.Equal("Q", SheetNumbers.PlotLetter(new[] { "010q" }));
        }
    }

    /// <summary>
    /// The number proposed for a new sheet: the view code, the plot's own letter, then the
    /// first sheet letter not in use anywhere. Expected values written by hand from DM-11's
    /// real numbers.
    /// </summary>
    public class SheetNumberProposalTests
    {
        private static readonly string[] DmElevenNumbers =
        {
            "010QE", "010QF", "010QG", "010QH", "200Q", "400Q", "600QC", "600QD"
        };

        /// <summary>
        /// The sheet letter starts at A even though the plot already uses E to H, because A is
        /// the first letter no sheet anywhere carries. The brief asks for the first free
        /// letter, not the next after the highest.
        /// </summary>
        [Fact]
        public void TheFirstFreeLetterFromAIsOffered()
        {
            SheetNumberProposal offered = SheetNumbers.Propose(
                "010", DmElevenNumbers, DmElevenNumbers);

            Assert.True(offered.Offered);
            Assert.Equal("010QA", offered.Number);
            Assert.Equal(string.Empty, offered.WhyNot);
        }

        [Fact]
        public void ATakenLetterIsSteppedOver()
        {
            var inUse = new[] { "010QA", "010QB", "200Q" };

            Assert.Equal("010QC", SheetNumbers.Propose("010", inUse, inUse).Number);
        }

        /// <summary>
        /// Taken anywhere in the model counts, not only on this plot, because Revit keeps
        /// sheet numbers unique across the whole document.
        /// </summary>
        [Fact]
        public void ANumberTakenByAnotherPlotIsNotOffered()
        {
            SheetNumberProposal offered = SheetNumbers.Propose(
                "010",
                new[] { "010QA", "200Q" },
                new[] { "200Q" });

            Assert.Equal("010QB", offered.Number);
        }

        [Fact]
        public void APlotWithNoNumbersGetsNoProposalAndSaysWhy()
        {
            SheetNumberProposal nothing = SheetNumbers.Propose(
                "010", DmElevenNumbers, new string[0]);

            Assert.False(nothing.Offered);
            Assert.Equal(string.Empty, nothing.Number);
            Assert.Equal(
                "This plot has no sheet numbers yet, so there is no plot letter to continue. "
                + "Type the number.",
                nothing.WhyNot);
        }

        [Fact]
        public void APlotWhoseNumbersDisagreeGetsNoProposal()
        {
            SheetNumberProposal nothing = SheetNumbers.Propose(
                "010", DmElevenNumbers, new[] { "010QE", "200R" });

            Assert.False(nothing.Offered);
            Assert.Equal(
                "This plot's own sheet numbers disagree about their plot letter, so none can "
                + "be continued. Type the number.",
                nothing.WhyNot);
        }

        [Fact]
        public void NoCodeMeansNoProposal()
        {
            Assert.Equal(
                "There is no view code to number from. Type the number.",
                SheetNumbers.Propose(string.Empty, DmElevenNumbers, DmElevenNumbers).WhyNot);

            Assert.Equal(
                "There is no view code to number from. Type the number.",
                SheetNumbers.Propose(null, DmElevenNumbers, DmElevenNumbers).WhyNot);
        }

        [Fact]
        public void EveryLetterTakenIsSaidRatherThanInvented()
        {
            var everyLetter = new List<string>();
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                everyLetter.Add("010Q" + letter);
            }

            SheetNumberProposal nothing = SheetNumbers.Propose(
                "010", everyLetter, new[] { "010QA" });

            Assert.False(nothing.Offered);
            Assert.Equal(
                "Every number from 010QA to 010QZ is taken. Type the number.",
                nothing.WhyNot);
        }
    }

    /// <summary>
    /// Which of the numbers the run asks for will be refused, before Run rather than after.
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
