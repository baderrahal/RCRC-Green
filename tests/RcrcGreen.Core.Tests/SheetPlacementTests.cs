using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The fit and the spots together, which is what the run places from and what the preview
    /// draws from. One set of positions, read twice.
    ///
    /// Every number below is written out by hand against an 800 by 600 area. At one per sheet
    /// the cell is the whole 800 by 600 and its centre is 400, 300. At two the cells are 800 by
    /// 300, stacked, so the centres are 400, 450 and 400, 150. At four they are 400 by 300, so
    /// the centres are 200, 450 then 600, 450 then 200, 150 then 600, 150.
    /// </summary>
    public class SheetPlacementTests
    {
        private static readonly DrawingArea Whole = DrawingArea.WholeSheet(800.0, 600.0);

        private static ViewOnPaper View(string name, double wide, double tall)
        {
            return new ViewOnPaper(name, wide, tall);
        }

        [Fact]
        public void OneViewIsCentredOnTheArea()
        {
            var placed = SheetPlacement.Of(Whole, 1, new[] { View("one", 700.0, 500.0) });

            PlacedSheet sheet = Assert.Single(placed);
            PlacedView only = Assert.Single(sheet.Views);

            Assert.Equal(400.0, only.Spot.CentreX);
            Assert.Equal(300.0, only.Spot.CentreY);
            Assert.Equal(800.0, only.CellWidthFeet);
            Assert.Equal(600.0, only.CellHeightFeet);
        }

        [Fact]
        public void TwoViewsSitOneAboveTheOtherAndTheFirstIsTheTop()
        {
            var placed = SheetPlacement.Of(Whole, 2, new[]
            {
                View("top", 700.0, 250.0),
                View("bottom", 700.0, 250.0)
            });

            PlacedSheet sheet = Assert.Single(placed);

            Assert.Equal(new[] { "top", "bottom" },
                sheet.Views.Select(one => one.View.ViewName).ToArray());

            Assert.Equal(450.0, sheet.Views[0].Spot.CentreY);
            Assert.Equal(150.0, sheet.Views[1].Spot.CentreY);
            Assert.Equal(400.0, sheet.Views[0].Spot.CentreX);
            Assert.Equal(400.0, sheet.Views[1].Spot.CentreX);
            Assert.Equal(300.0, sheet.Views[0].CellHeightFeet);
        }

        [Fact]
        public void FourViewsMakeATwoByTwoGridInReadingOrder()
        {
            var placed = SheetPlacement.Of(Whole, 4, new[]
            {
                View("a", 300.0, 250.0), View("b", 300.0, 250.0),
                View("c", 300.0, 250.0), View("d", 300.0, 250.0)
            });

            PlacedSheet sheet = Assert.Single(placed);

            Assert.Equal(
                new[] { "200 450", "600 450", "200 150", "600 150" },
                sheet.Views.Select(one => one.Spot.ToString()).ToArray());

            Assert.Equal(400.0, sheet.Views[0].CellWidthFeet);
            Assert.Equal(300.0, sheet.Views[0].CellHeightFeet);
        }

        [Fact]
        public void TheSpotsAreExactlyTheOnesSheetLayoutGives()
        {
            var placed = SheetPlacement.Of(Whole, 4, new[]
            {
                View("a", 10.0, 10.0), View("b", 10.0, 10.0),
                View("c", 10.0, 10.0), View("d", 10.0, 10.0)
            });

            Assert.Equal(
                SheetLayout.For(Whole, 4).Select(one => one.ToString()).ToArray(),
                placed[0].Views.Select(one => one.Spot.ToString()).ToArray());
        }

        [Fact]
        public void TheDivisionIsExactlyTheOneSheetFitGives()
        {
            ViewOnPaper[] views =
            {
                View("wide", 700.0, 250.0),
                View("wider", 900.0, 250.0),
                View("third", 700.0, 250.0)
            };

            var placed = SheetPlacement.Of(Whole, 2, views);
            var fitted = SheetFit.Of(Whole, 2, views);

            Assert.Equal(fitted.Count, placed.Count);

            for (int at = 0; at < fitted.Count; at++)
            {
                Assert.Equal(
                    fitted[at].Views.Select(one => one.ViewName).ToArray(),
                    placed[at].Views.Select(one => one.View.ViewName).ToArray());

                Assert.Equal(fitted[at].CarriedOver, placed[at].CarriedOver);
                Assert.Equal(fitted[at].HoldsOneTooBig, placed[at].HoldsOneTooBig);
            }
        }

        [Fact]
        public void ASheetCarryingWhatDidNotFitOnTheOneBeforeItSaysSo()
        {
            var placed = SheetPlacement.Of(Whole, 2, new[]
            {
                View("one", 700.0, 250.0),
                View("two", 700.0, 250.0),
                View("three", 700.0, 250.0)
            });

            Assert.Equal(2, placed.Count);
            Assert.False(placed[0].CarriedOver);
            Assert.True(placed[1].CarriedOver);
        }

        [Fact]
        public void AViewBiggerThanItsCellSaysSoOnItsOwnRatherThanOnlyOnTheSheet()
        {
            var placed = SheetPlacement.Of(Whole, 4, new[] { View("huge", 700.0, 500.0) });

            PlacedSheet sheet = Assert.Single(placed);

            Assert.True(sheet.HoldsOneTooBig);
            Assert.True(sheet.Views[0].BiggerThanItsCell);
        }

        [Fact]
        public void AViewThatWasNotMeasuredIsLaidOutAndMarkedRatherThanGuessedAt()
        {
            var placed = SheetPlacement.Of(Whole, 2, new[]
            {
                View("measured", 700.0, 250.0),
                ViewOnPaper.NotMeasured("a schedule")
            });

            PlacedSheet sheet = Assert.Single(placed);

            Assert.True(sheet.AnythingNotMeasured);
            Assert.False(sheet.Views[0].BiggerThanItsCell);
            Assert.False(sheet.Views[1].BiggerThanItsCell);
            Assert.Equal(150.0, sheet.Views[1].Spot.CentreY);
            Assert.Equal("size not read", sheet.Views[1].View.SizeInWords());
        }

        [Fact]
        public void ASheetWhoseViewsAreAllMeasuredSaysNothingIsMissing()
        {
            var placed = SheetPlacement.Of(Whole, 1, new[] { View("one", 100.0, 100.0) });

            Assert.False(placed[0].AnythingNotMeasured);
        }

        [Fact]
        public void NoViewsMakesNoSheetsAndEmptyMakesTheOneTitleSheet()
        {
            Assert.Empty(SheetPlacement.Of(Whole, 1, null));

            PlacedSheet title = Assert.Single(SheetPlacement.Empty(Whole, 1));
            Assert.Empty(title.Views);
            Assert.False(title.CarriedOver);
            Assert.False(title.AnythingNotMeasured);
            Assert.Equal(800.0, title.Area.WidthFeet);
        }

        [Fact]
        public void ACountNobodyOffersIsLaidOutAsOneRatherThanRefused()
        {
            var placed = SheetPlacement.Of(Whole, 3, new[] { View("one", 100.0, 100.0) });

            Assert.Equal(1, placed[0].ViewsPerSheet);
            Assert.Equal(400.0, placed[0].Views[0].Spot.CentreX);

            Assert.Equal(1, SheetPlacement.Empty(Whole, 7)[0].ViewsPerSheet);
        }

        [Fact]
        public void AnAreaIsAlwaysNeeded()
        {
            Assert.Throws<ArgumentNullException>(() => SheetPlacement.Of(null, 1, null));
            Assert.Throws<ArgumentNullException>(() => SheetPlacement.Empty(null, 1));
        }

        [Fact]
        public void TheStripIsTakenOffTheAreaTheViewsAreLaidOutIn()
        {
            DrawingArea inside = DrawingArea.InsideTheTitleBlock(800.0, 600.0);
            var placed = SheetPlacement.Of(inside, 1, new[] { View("one", 10.0, 10.0) });

            Assert.True(placed[0].Area.StripTakenOff);
            Assert.Equal(inside.WidthFeet / 2.0, placed[0].Views[0].Spot.CentreX);
        }
    }
}
