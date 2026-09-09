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
    /// Which of the numbers the user typed will be refused, before Run rather than after.
    /// </summary>
    public class SheetNumberFaultTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        private static SheetOrder Order(string sheetName, params string[] plotAndNumber)
        {
            var numbers = new List<SheetRequest>();
            for (int at = 0; at + 1 < plotAndNumber.Length; at += 2)
            {
                numbers.Add(new SheetRequest(plotAndNumber[at], plotAndNumber[at + 1]));
            }

            return new SheetOrder(
                new SheetDefinition("AR-PRX-Title_Block_A1", "GA-DETAILED DESIGN", sheetName,
                    new[] { General }, 1),
                numbers);
        }

        private static readonly string[] InTheModel = { "010EA", "L-211" };

        [Fact]
        public void ANumberAlreadyInTheModelIsCaught()
        {
            var problems = SheetNumbers.Problems(
                new[] { Order("GA", "DM-11", "010EA", "DM-12", "L-999") },
                new[] { "DM-11", "DM-12" },
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
        /// Two ticked plots given one number. The first would be created and the second refused,
        /// which is the same fault arriving a second later.
        /// </summary>
        [Fact]
        public void TwoPlotsGivenOneNumberAreBothCaught()
        {
            var problems = SheetNumbers.Problems(
                new[] { Order("GA", "DM-11", "L-500", "DM-12", "L-500") },
                new[] { "DM-11", "DM-12" },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.All(problems, one =>
                Assert.Equal(SheetNumberFault.UsedTwiceInThisRun, one.Fault));
        }

        /// <summary>
        /// Two sheets described separately, both asking for one number, clash just as hard as
        /// two plots do. They go into one model.
        /// </summary>
        [Fact]
        public void TwoSheetsAskingForOneNumberClashToo()
        {
            var problems = SheetNumbers.Problems(
                new[]
                {
                    Order("GA", "DM-11", "L-500"),
                    Order("LIST OF DRAWINGS", "DM-11", "L-500")
                },
                new[] { "DM-11" },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.Equal(
                new[] { "GA", "LIST OF DRAWINGS" },
                problems.Select(one => one.SheetName).ToArray());
        }

        [Fact]
        public void AFreeNumberOnATickedPlotIsNoProblemAtAll()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[] { Order("GA", "DM-11", "L-500", "DM-12", "L-501") },
                new[] { "DM-11", "DM-12" },
                InTheModel));
        }

        /// <summary>
        /// An unticked plot makes no sheet, so its number cannot clash with anything and is not
        /// counted against the run.
        /// </summary>
        [Fact]
        public void AnUntickedPlotIsNotCounted()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[] { Order("GA", "DM-99", "010EA") },
                new[] { "DM-11" },
                InTheModel));
        }

        [Fact]
        public void APlotWithNoNumberIsNotAClash()
        {
            Assert.Empty(SheetNumbers.Problems(
                new[] { Order("GA", "DM-11", string.Empty, "DM-12", "   ") },
                new[] { "DM-11", "DM-12" },
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
                new[] { Order("GA", "DM-11", "010EA", "DM-12", "010EA") },
                new[] { "DM-11", "DM-12" },
                InTheModel);

            Assert.Equal(2, problems.Count);
            Assert.All(problems, one =>
                Assert.Equal(SheetNumberFault.AlreadyInTheModel, one.Fault));
        }

        /// <summary>
        /// The faults come back one set per sheet described, keyed by plot, because that is what
        /// puts a line next to the right box.
        /// </summary>
        [Fact]
        public void TheFaultsComeBackKeyedByPlotForEachSheet()
        {
            var faults = SheetNumbers.Faults(
                new[]
                {
                    Order("GA", "DM-11", "010EA", "DM-12", "L-500"),
                    Order("LIST", "DM-11", "L-501")
                },
                new[] { "DM-11", "DM-12" },
                InTheModel);

            Assert.Equal(2, faults.Count);
            Assert.Equal(SheetNumberFault.AlreadyInTheModel, faults[0]["DM-11"]);
            Assert.False(faults[0].ContainsKey("DM-12"));
            Assert.Empty(faults[1]);
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
