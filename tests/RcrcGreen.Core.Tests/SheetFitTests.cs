using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Which views really fit on a sheet. Two faults on a real sheet, both from placing a
    /// view in a cell without asking whether it fits: two side by side overlapped, and a
    /// view bigger than its cell ran over its neighbour.
    ///
    /// Every size below is written out by hand against an 800 by 600 area. At one per
    /// sheet the cell is the whole 800 by 600, at two it is 800 by 300 because two stack,
    /// and at four it is 400 by 300.
    /// </summary>
    public class SheetFitTests
    {
        private static readonly DrawingArea Whole = DrawingArea.WholeSheet(800.0, 600.0);

        private static ViewOnPaper View(string name, double wide, double tall)
        {
            return new ViewOnPaper(name, wide, tall);
        }

        private static string[] NamesOn(FittedSheet sheet)
        {
            return sheet.Views.Select(one => one.ViewName).ToArray();
        }

        /// <summary>
        /// Everything fits, so the division is the old one: two per sheet, four views, two
        /// sheets, and nothing carried over.
        /// </summary>
        [Fact]
        public void ViewsThatFitDivideTheWayTheyAlwaysDid()
        {
            var fitted = SheetFit.Of(Whole, 2, new[]
            {
                View("one", 700.0, 250.0),
                View("two", 700.0, 250.0),
                View("three", 700.0, 250.0),
                View("four", 700.0, 250.0)
            });

            Assert.Equal(2, fitted.Count);
            Assert.Equal(new[] { "one", "two" }, NamesOn(fitted[0]));
            Assert.Equal(new[] { "three", "four" }, NamesOn(fitted[1]));
            Assert.False(fitted[0].CarriedOver);
            Assert.True(fitted[1].CarriedOver);
            Assert.All(fitted, sheet => Assert.False(sheet.HoldsOneTooBig));
        }

        /// <summary>
        /// The real fault. Two views at two per sheet, each 400 tall where the cell is 300,
        /// so the second cannot go under the first. It goes onto a sheet of its own rather
        /// than over the top of it.
        ///
        /// Both are still bigger than the cell they sit in, and both say so. A sheet holding
        /// one view still lays it out in the first cell of the count it was described with,
        /// which is the rule the division has always followed, so alone is not the same as
        /// fitting.
        /// </summary>
        [Fact]
        public void AViewThatDoesNotFitTheCellStartsAFreshSheet()
        {
            var fitted = SheetFit.Of(Whole, 2, new[]
            {
                View("tall one", 700.0, 400.0),
                View("tall two", 700.0, 400.0)
            });

            Assert.Equal(2, fitted.Count);
            Assert.Equal(new[] { "tall one" }, NamesOn(fitted[0]));
            Assert.Equal(new[] { "tall two" }, NamesOn(fitted[1]));
            Assert.True(fitted[1].CarriedOver);
            Assert.All(fitted, sheet => Assert.True(sheet.HoldsOneTooBig));
        }

        /// <summary>
        /// The same two views described one per sheet fit, because the cell is the whole
        /// area. Nothing about the views changed, only how many the sheet was described to
        /// hold.
        /// </summary>
        [Fact]
        public void TheSameViewsFitWhenTheSheetHoldsOne()
        {
            var fitted = SheetFit.Of(Whole, 1, new[]
            {
                View("tall one", 700.0, 400.0),
                View("tall two", 700.0, 400.0)
            });

            Assert.Equal(2, fitted.Count);
            Assert.All(fitted, sheet => Assert.False(sheet.HoldsOneTooBig));
        }

        /// <summary>
        /// Nothing is ever dropped. Five views at four per sheet with one of them too tall
        /// still come back as five, in the order they were ticked.
        /// </summary>
        [Fact]
        public void NoViewIsEverLeftOff()
        {
            var fitted = SheetFit.Of(Whole, 4, new[]
            {
                View("a", 300.0, 200.0),
                View("b", 300.0, 200.0),
                View("c", 300.0, 500.0),
                View("d", 300.0, 200.0),
                View("e", 300.0, 200.0)
            });

            Assert.Equal(
                new[] { "a", "b", "c", "d", "e" },
                fitted.SelectMany(NamesOn).ToArray());
        }

        /// <summary>
        /// A view bigger than the whole area goes on a sheet of its own with nothing beside
        /// it, so it covers nothing, and the sheet says it holds one too big. Leaving it off
        /// would lose it, which is the other half of the fault.
        /// </summary>
        [Fact]
        public void AViewTooBigForTheWholeAreaGoesOnItsOwnAndIsSaid()
        {
            var fitted = SheetFit.Of(Whole, 2, new[]
            {
                View("ordinary", 700.0, 250.0),
                View("enormous", 4000.0, 250.0),
                View("another", 700.0, 250.0)
            });

            Assert.Equal(3, fitted.Count);
            Assert.Equal(new[] { "ordinary" }, NamesOn(fitted[0]));
            Assert.Equal(new[] { "enormous" }, NamesOn(fitted[1]));
            Assert.Equal(new[] { "another" }, NamesOn(fitted[2]));

            Assert.False(fitted[0].HoldsOneTooBig);
            Assert.True(fitted[1].HoldsOneTooBig);
            Assert.False(fitted[2].HoldsOneTooBig);
        }

        /// <summary>
        /// A view that cannot be measured fits whatever it is put in, because nothing here
        /// knows any better. A schedule's size is not known until Revit has drawn it.
        /// </summary>
        [Fact]
        public void AnUnmeasuredViewFitsAndSaysSo()
        {
            ViewOnPaper schedule = ViewOnPaper.NotMeasured("DM-11-(600) HARDSCAPE SCHEDULE");

            Assert.False(schedule.Measured);
            Assert.True(schedule.FitsIn(1.0, 1.0));
            Assert.Equal("size not read", schedule.SizeInWords());

            var fitted = SheetFit.Of(Whole, 2, new[]
            {
                schedule,
                ViewOnPaper.NotMeasured("another schedule")
            });

            Assert.Single(fitted);
            Assert.Equal(2, fitted[0].Views.Count);
        }

        /// <summary>
        /// A size that is zero, negative or not a number is not a size. It reads as
        /// unmeasured rather than as a view that fits nothing, the same rule SheetSize
        /// follows at the other end of the run.
        /// </summary>
        [Fact]
        public void ASizeThatIsNotASizeReadsAsUnmeasured()
        {
            Assert.False(View("zero", 0.0, 100.0).Measured);
            Assert.False(View("negative", -5.0, 100.0).Measured);
            Assert.False(View("nan", double.NaN, 100.0).Measured);
            Assert.False(View("infinite", double.PositiveInfinity, 100.0).Measured);
            Assert.True(View("real", 1.0, 1.0).Measured);
        }

        /// <summary>
        /// At four per sheet the cell is a quarter, 400 by 300, so a 500 wide view does not
        /// fit beside anything even though it fits the sheet.
        /// </summary>
        [Fact]
        public void TheCellIsTheDivisionTheLayoutReallyUses()
        {
            var fitted = SheetFit.Of(Whole, 4, new[]
            {
                View("narrow", 390.0, 290.0),
                View("wide", 500.0, 290.0)
            });

            Assert.Equal(2, fitted.Count);
            Assert.Equal(new[] { "narrow" }, NamesOn(fitted[0]));
            Assert.Equal(new[] { "wide" }, NamesOn(fitted[1]));
        }

        [Fact]
        public void NoViewsMakeNoSheetsAndNoAreaIsRefused()
        {
            Assert.Empty(SheetFit.Of(Whole, 2, null));
            Assert.Empty(SheetFit.Of(Whole, 2, new ViewOnPaper[0]));
            Assert.Throws<ArgumentNullException>(() => SheetFit.Of(null, 2, null));
        }

        /// <summary>
        /// The size in the report is millimetres, because that is what the rest of the
        /// sheet lines are in. 800 feet is 243840 mm, written out by hand.
        /// </summary>
        [Fact]
        public void TheSizeIsSaidInMillimetres()
        {
            Assert.Equal("243840 by 182880 mm", View("one", 800.0, 600.0).SizeInWords());
        }

        /// <summary>
        /// The per sheet line the report carries, which is what the round asked for: how
        /// many views were asked for and how many fitted.
        /// </summary>
        [Fact]
        public void TheReportLineSaysAskedAndFitted()
        {
            Assert.Equal(
                "4 views were asked for and all of them fitted.", SheetFit.InWords(4, 4));
            Assert.Equal("1 view was asked for and it fitted.", SheetFit.InWords(1, 1));
            Assert.Equal(
                "4 views were asked for and 2 fitted on this sheet. The rest are on a sheet "
                + "of their own.",
                SheetFit.InWords(4, 2));
        }

        [Fact]
        public void TheCarriedOverAndTooBigLinesNameWhatTheyAreAbout()
        {
            Assert.Equal(
                "This sheet was made because 600DM42A was full. The views that did not fit "
                + "at their own scale are on it rather than being dropped or laid over one "
                + "another.",
                SheetFit.CarriedOverInWords("600DM42A"));

            Assert.Equal(
                "DM-11-(400) LANDSCAPE CROSS SECTION is 13250.5 by 400 mm, which is bigger "
                + "than the cell a view sits in on a sheet laid out for 2 views. It is alone "
                + "on its sheet, so it covers nothing, and it reaches past its cell. Change "
                + "its scale, its crop, or how many views go on the sheet.",
                SheetFit.TooBigInWords(
                    "DM-11-(400) LANDSCAPE CROSS SECTION", "13250.5 by 400 mm", 2));
        }
    }
}
