using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class SectionPlacementTests
    {
        private const int Places = 9;

        /// <summary>
        /// Any depth will do for the tests that are about the line rather than the view, so
        /// they all pass the same one and it is not a round number that could hide a mix up
        /// with a box dimension.
        /// </summary>
        private const double SomeDepth = 32.8;

        /// <summary>
        /// The cut length these tests use, picked so it is neither a box dimension nor a
        /// half of one and every expected coordinate below is worked out by hand from it.
        /// The real number the tool applies is SectionCutLength.Metres.
        /// </summary>
        private const double SomeCut = 12.0;

        /// <summary>
        /// Wide along X, narrow along Y. The short way across it is the Y direction.
        /// </summary>
        private static PlotBox WideBox()
        {
            return new PlotBox("DM-41", 0, 0, 0, 100, 20, 10);
        }

        /// <summary>
        /// The box is 100 by 20 with its centre at 50, 10. The short way is along Y, and a
        /// 12 long cut runs from 4 to 16 rather than from edge to edge.
        /// </summary>
        [Fact]
        public void TheShortAxisRunsTheShortWayThroughTheCentre()
        {
            SectionPlacement placement = SectionPlacement.Across(
                WideBox(), SectionAxis.ShortSide, SomeDepth, SomeCut);

            Assert.Equal(12.0, placement.Length, Places);
            Assert.Equal(50.0, placement.Start.X, Places);
            Assert.Equal(4.0, placement.Start.Y, Places);
            Assert.Equal(50.0, placement.End.X, Places);
            Assert.Equal(16.0, placement.End.Y, Places);
        }

        /// <summary>
        /// The cut used to run the whole width of the box, which is what made the viewport
        /// on a real sheet wider than the drawing area. A box ten times the size now gives
        /// exactly the same line.
        /// </summary>
        [Fact]
        public void TheCutIsTheLengthItIsGivenWhateverTheBoxMeasures()
        {
            SectionPlacement small = SectionPlacement.Across(
                WideBox(), SectionAxis.ShortSide, SomeDepth, SomeCut);

            SectionPlacement large = SectionPlacement.Across(
                new PlotBox("DM-41", 0, 0, 0, 1000, 200, 10),
                SectionAxis.ShortSide, SomeDepth, SomeCut);

            Assert.Equal(12.0, small.Length, Places);
            Assert.Equal(12.0, large.Length, Places);
            Assert.Equal(94.0, large.Start.Y, Places);
            Assert.Equal(106.0, large.End.Y, Places);
        }

        [Fact]
        public void TheLongAxisRunsTheOtherWayAcrossTheSameBox()
        {
            SectionPlacement placement = SectionPlacement.Across(
                WideBox(), SectionAxis.LongSide, SomeDepth, SomeCut);

            Assert.Equal(12.0, placement.Length, Places);
            Assert.Equal(44.0, placement.Start.X, Places);
            Assert.Equal(10.0, placement.Start.Y, Places);
            Assert.Equal(56.0, placement.End.X, Places);
            Assert.Equal(10.0, placement.End.Y, Places);
        }

        [Fact]
        public void TheShortAxisFollowsTheBoxRatherThanAFixedDirection()
        {
            PlotBox tall = new PlotBox("PF-12", 0, 0, 0, 20, 100, 10);

            SectionPlacement placement = SectionPlacement.Across(
                tall, SectionAxis.ShortSide, SomeDepth, SomeCut);

            Assert.Equal(12.0, placement.Length, Places);
            Assert.Equal(4.0, placement.Start.X, Places);
            Assert.Equal(16.0, placement.End.X, Places);
            Assert.Equal(50.0, placement.Start.Y, Places);
        }

        [Fact]
        public void TheLineCrossesTheCentreOfTheBoxOnBothAxes()
        {
            PlotBox offOrigin = new PlotBox("DM-41", 10, -30, 4, 110, -10, 8);

            foreach (SectionAxis axis in new[] { SectionAxis.ShortSide, SectionAxis.LongSide })
            {
                SectionPlacement placement = SectionPlacement.Across(
                    offOrigin, axis, SomeDepth, SomeCut);

                Assert.Equal(60.0, placement.Midpoint.X, Places);
                Assert.Equal(-20.0, placement.Midpoint.Y, Places);
                Assert.Equal(6.0, placement.Midpoint.Z, Places);
            }
        }

        [Fact]
        public void BothEndsSitAtTheMiddleHeightOfTheBox()
        {
            SectionPlacement placement = SectionPlacement.Across(
                WideBox(), SectionAxis.ShortSide, SomeDepth, SomeCut);

            Assert.Equal(5.0, placement.Start.Z, Places);
            Assert.Equal(5.0, placement.End.Z, Places);
        }

        [Fact]
        public void TheViewLooksAcrossTheLineAndStaysHorizontal()
        {
            SectionPlacement shortWay = SectionPlacement.Across(
                WideBox(), SectionAxis.ShortSide, SomeDepth, SomeCut);
            SectionPlacement longWay = SectionPlacement.Across(
                WideBox(), SectionAxis.LongSide, SomeDepth, SomeCut);

            Assert.Equal(1.0, shortWay.ViewDirection.X, Places);
            Assert.Equal(0.0, shortWay.ViewDirection.Y, Places);
            Assert.Equal(0.0, shortWay.ViewDirection.Z, Places);

            Assert.Equal(0.0, longWay.ViewDirection.X, Places);
            Assert.Equal(-1.0, longWay.ViewDirection.Y, Places);
            Assert.Equal(0.0, longWay.ViewDirection.Z, Places);
        }

        [Theory]
        [InlineData(32.808398950131235)]
        [InlineData(1.0)]
        [InlineData(0.0)]
        [InlineData(-4.5)]
        public void TheSuppliedDepthComesBackUntouchedOnEitherAxis(double depth)
        {
            PlotBox box = WideBox();

            Assert.Equal(
                depth,
                SectionPlacement.Across(box, SectionAxis.ShortSide, depth, SomeCut).Depth,
                Places);
            Assert.Equal(
                depth,
                SectionPlacement.Across(box, SectionAxis.LongSide, depth, SomeCut).Depth,
                Places);
        }

        [Fact]
        public void TheDepthDoesNotMoveWhenTheBoxChangesSize()
        {
            PlotBox small = new PlotBox("DM-41", 0, 0, 0, 100, 20, 10);
            PlotBox large = new PlotBox("DM-41", 0, 0, 0, 4000, 900, 10);

            Assert.Equal(
                SectionPlacement.Across(small, SectionAxis.ShortSide, SomeDepth, SomeCut).Depth,
                SectionPlacement.Across(large, SectionAxis.ShortSide, SomeDepth, SomeCut).Depth,
                Places);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ADepthThatIsNotARealNumberIsRefused(double depth)
        {
            Assert.Throws<ArgumentException>(
                () => SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, depth, SomeCut));
        }

        /// <summary>
        /// A cut length that is not a real length is refused the same way, zero included,
        /// because a zero long cut is a section that draws nothing and would come back
        /// looking like a placement.
        /// </summary>
        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void ACutLengthThatIsNotARealLengthIsRefused(double cut)
        {
            Assert.Throws<ArgumentException>(
                () => SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, SomeDepth, cut));
        }

        [Fact]
        public void ASquareBoxTakesTheShortSideAlongYSoTheAnswerDoesNotMoveBetweenRuns()
        {
            PlotBox square = new PlotBox("AB-7", 0, 0, 0, 50, 50, 5);

            SectionPlacement shortWay = SectionPlacement.Across(
                square, SectionAxis.ShortSide, SomeDepth, SomeCut);
            SectionPlacement longWay = SectionPlacement.Across(
                square, SectionAxis.LongSide, SomeDepth, SomeCut);

            Assert.Equal(25.0, shortWay.Start.X, Places);
            Assert.Equal(19.0, shortWay.Start.Y, Places);
            Assert.Equal(19.0, longWay.Start.X, Places);
            Assert.Equal(25.0, longWay.Start.Y, Places);
        }

        [Fact]
        public void AFlatScopeBoxIsRefusedRatherThanGivenAZeroLengthLine()
        {
            PlotBox flat = new PlotBox("DM-41", 0, 5, 0, 100, 5, 10);

            Assert.Throws<ArgumentException>(
                () => SectionPlacement.Across(flat, SectionAxis.ShortSide, SomeDepth, SomeCut));
        }

        [Fact]
        public void ABoxWithItsMaximumBelowItsMinimumIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PlotBox("DM-41", 0, 0, 0, 100, -20, 10));
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ABoundThatIsNotARealNumberIsRefusedRatherThanTurningTheCentreIntoNaN(double bound)
        {
            Assert.Throws<ArgumentException>(() => new PlotBox("DM-41", 0, 0, 0, bound, 20, 10));
            Assert.Throws<ArgumentException>(() => new PlotBox("DM-41", bound, 0, 0, 100, 20, 10));
        }

        [Fact]
        public void ALineFarSmallerThanOneUnitStillReportsALength()
        {
            PlotBox tiny = new PlotBox("DM-41", 0, 0, 0, 1e-200, 4e-200, 1e-200);

            SectionPlacement placement = SectionPlacement.Across(
                tiny, SectionAxis.ShortSide, SomeDepth, 1e-200);

            // Compared as a ratio, because rounding to a fixed number of decimal places
            // cannot say anything about a number this small.
            Assert.Equal(1.0, placement.Length / 1e-200, Places);
            Assert.True(placement.Length > 0.0);
        }

        [Fact]
        public void ALineFarLargerThanAnyModelDoesNotReportInfinity()
        {
            PlotBox huge = new PlotBox("DM-41", 0, 0, 0, 1e200, 4e200, 1e200);

            SectionPlacement placement = SectionPlacement.Across(
                huge, SectionAxis.ShortSide, SomeDepth, 1e200);

            Assert.False(double.IsInfinity(placement.Length));
            Assert.Equal(1.0, placement.Length / 1e200, Places);
        }
    }
}
