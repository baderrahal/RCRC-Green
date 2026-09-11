using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The part of a sheet a view may sit in.
    ///
    /// The numbers are the same tenths the layout tests use, 800 by 600 for an A1, so the
    /// arithmetic stays exact and every expected value can be written out by hand.
    /// </summary>
    public class DrawingAreaTests
    {
        private const double Wide = 800.0;

        private const double Tall = 600.0;

        [Fact]
        public void TheWholeSheetIsTheWholeSheet()
        {
            DrawingArea area = DrawingArea.WholeSheet(Wide, Tall);

            Assert.Equal(0.0, area.LeftFeet);
            Assert.Equal(0.0, area.BottomFeet);
            Assert.Equal(800.0, area.WidthFeet);
            Assert.Equal(600.0, area.HeightFeet);
            Assert.False(area.StripTakenOff);
        }

        /// <summary>
        /// A fifth of 800 is 160, so the area is 640 wide and its right edge is where the strip
        /// begins. The height is untouched, because the strip runs down the side rather than
        /// across the foot.
        /// </summary>
        [Fact]
        public void TheStripComesOffTheWidthAndNothingElse()
        {
            DrawingArea area = DrawingArea.InsideTheTitleBlock(Wide, Tall);

            Assert.Equal(640.0, area.WidthFeet);
            Assert.Equal(600.0, area.HeightFeet);
            Assert.Equal(0.0, area.LeftFeet);
            Assert.Equal(0.0, area.BottomFeet);
            Assert.Equal(640.0, area.LeftFeet + area.WidthFeet);
            Assert.True(area.StripTakenOff);
        }

        /// <summary>
        /// One view centred in the drawing area rather than in the sheet. 640 halved is 320,
        /// where the whole sheet put it at 400 and a wide view crossed the strip.
        /// </summary>
        [Fact]
        public void AViewCentresInTheAreaAndNotInTheSheet()
        {
            var spots = SheetLayout.For(DrawingArea.InsideTheTitleBlock(Wide, Tall), 1).ToArray();

            Assert.Single(spots);
            Assert.Equal(320.0, spots[0].CentreX);
            Assert.Equal(300.0, spots[0].CentreY);
        }

        /// <summary>
        /// Two side by side inside 640: quarters at 160 and 480. Both are left of the strip,
        /// which is the whole point of taking it off.
        /// </summary>
        [Fact]
        public void TwoSitSideBySideInsideTheAreaAndNeitherReachesTheStrip()
        {
            var spots = SheetLayout.For(DrawingArea.InsideTheTitleBlock(Wide, Tall), 2).ToArray();

            Assert.Equal(160.0, spots[0].CentreX);
            Assert.Equal(480.0, spots[1].CentreX);
            Assert.True(spots[1].CentreX < 640.0);
        }

        /// <summary>
        /// The strip is the tool's setting and the words have to say so. A line that reads as a
        /// measurement off the title block is the fault the KPI pane already had once, where the
        /// workbook's own note was printed as though it were what the tool did.
        /// </summary>
        [Fact]
        public void TheWordsSayWhoseSettingTheStripIs()
        {
            string said = DrawingArea.InsideTheTitleBlock(Wide, Tall).InWords();

            Assert.Contains("the tool's own setting", said);
            Assert.Contains("20 percent", said);
            Assert.Contains("Sheet Width and Sheet Height", said);

            // A foot is 304.8 mm and three quarters of one is 228.6, both written out by hand.
            // The whole sheet says so plainly rather than describing a strip it did not take.
            Assert.Equal(
                "Laid out across the whole sheet, 304.8 by 228.6 mm.",
                DrawingArea.WholeSheet(1.0, 0.75).InWords());
        }

        [Fact]
        public void ASheetWithNoSizeIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DrawingArea.WholeSheet(0.0, Tall));
            Assert.Throws<ArgumentOutOfRangeException>(() => DrawingArea.WholeSheet(Wide, -1.0));
            Assert.Throws<ArgumentException>(() => DrawingArea.WholeSheet(double.NaN, Tall));
            Assert.Throws<ArgumentException>(
                () => DrawingArea.WholeSheet(Wide, double.PositiveInfinity));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => DrawingArea.InsideTheTitleBlock(0.0, Tall));
            Assert.Throws<ArgumentException>(
                () => DrawingArea.InsideTheTitleBlock(double.NaN, Tall));
        }
    }
}
