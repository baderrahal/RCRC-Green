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
        /// Wide along X, narrow along Y. The short way across it is the Y direction.
        /// </summary>
        private static PlotBox WideBox()
        {
            return new PlotBox("DM-41", 0, 0, 0, 100, 20, 10);
        }

        [Fact]
        public void TheShortAxisRunsTheShortWayAcrossAWideBox()
        {
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, SomeDepth);

            Assert.Equal(20.0, placement.Length, Places);
            Assert.Equal(50.0, placement.Start.X, Places);
            Assert.Equal(0.0, placement.Start.Y, Places);
            Assert.Equal(50.0, placement.End.X, Places);
            Assert.Equal(20.0, placement.End.Y, Places);
        }

        [Fact]
        public void TheLongAxisRunsTheOtherWayAcrossTheSameBox()
        {
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.LongSide, SomeDepth);

            Assert.Equal(100.0, placement.Length, Places);
            Assert.Equal(0.0, placement.Start.X, Places);
            Assert.Equal(10.0, placement.Start.Y, Places);
            Assert.Equal(100.0, placement.End.X, Places);
            Assert.Equal(10.0, placement.End.Y, Places);
        }

        [Fact]
        public void TheShortAxisFollowsTheBoxRatherThanAFixedDirection()
        {
            PlotBox tall = new PlotBox("PF-12", 0, 0, 0, 20, 100, 10);

            SectionPlacement placement = SectionPlacement.Across(tall, SectionAxis.ShortSide, SomeDepth);

            Assert.Equal(20.0, placement.Length, Places);
            Assert.Equal(0.0, placement.Start.X, Places);
            Assert.Equal(20.0, placement.End.X, Places);
            Assert.Equal(50.0, placement.Start.Y, Places);
        }

        [Fact]
        public void TheLineCrossesTheCentreOfTheBoxOnBothAxes()
        {
            PlotBox offOrigin = new PlotBox("DM-41", 10, -30, 4, 110, -10, 8);

            foreach (SectionAxis axis in new[] { SectionAxis.ShortSide, SectionAxis.LongSide })
            {
                SectionPlacement placement = SectionPlacement.Across(offOrigin, axis, SomeDepth);

                Assert.Equal(60.0, placement.Midpoint.X, Places);
                Assert.Equal(-20.0, placement.Midpoint.Y, Places);
                Assert.Equal(6.0, placement.Midpoint.Z, Places);
            }
        }

        [Fact]
        public void BothEndsSitAtTheMiddleHeightOfTheBox()
        {
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, SomeDepth);

            Assert.Equal(5.0, placement.Start.Z, Places);
            Assert.Equal(5.0, placement.End.Z, Places);
        }

        [Fact]
        public void TheViewLooksAcrossTheLineAndStaysHorizontal()
        {
            SectionPlacement shortWay = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, SomeDepth);
            SectionPlacement longWay = SectionPlacement.Across(WideBox(), SectionAxis.LongSide, SomeDepth);

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

            Assert.Equal(depth, SectionPlacement.Across(box, SectionAxis.ShortSide, depth).Depth, Places);
            Assert.Equal(depth, SectionPlacement.Across(box, SectionAxis.LongSide, depth).Depth, Places);
        }

        [Fact]
        public void TheDepthDoesNotMoveWhenTheBoxChangesSize()
        {
            PlotBox small = new PlotBox("DM-41", 0, 0, 0, 100, 20, 10);
            PlotBox large = new PlotBox("DM-41", 0, 0, 0, 4000, 900, 10);

            Assert.Equal(
                SectionPlacement.Across(small, SectionAxis.ShortSide, SomeDepth).Depth,
                SectionPlacement.Across(large, SectionAxis.ShortSide, SomeDepth).Depth,
                Places);
        }

        [Theory]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void ADepthThatIsNotARealNumberIsRefused(double depth)
        {
            Assert.Throws<ArgumentException>(
                () => SectionPlacement.Across(WideBox(), SectionAxis.ShortSide, depth));
        }

        [Fact]
        public void ASquareBoxTakesTheShortSideAlongYSoTheAnswerDoesNotMoveBetweenRuns()
        {
            PlotBox square = new PlotBox("AB-7", 0, 0, 0, 50, 50, 5);

            SectionPlacement shortWay = SectionPlacement.Across(square, SectionAxis.ShortSide, SomeDepth);
            SectionPlacement longWay = SectionPlacement.Across(square, SectionAxis.LongSide, SomeDepth);

            Assert.Equal(25.0, shortWay.Start.X, Places);
            Assert.Equal(0.0, shortWay.Start.Y, Places);
            Assert.Equal(0.0, longWay.Start.X, Places);
            Assert.Equal(25.0, longWay.Start.Y, Places);
        }

        [Fact]
        public void AFlatScopeBoxIsRefusedRatherThanGivenAZeroLengthLine()
        {
            PlotBox flat = new PlotBox("DM-41", 0, 5, 0, 100, 5, 10);

            Assert.Throws<ArgumentException>(
                () => SectionPlacement.Across(flat, SectionAxis.ShortSide, SomeDepth));
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

            SectionPlacement placement = SectionPlacement.Across(tiny, SectionAxis.ShortSide, SomeDepth);

            // Compared as a ratio, because rounding to a fixed number of decimal places
            // cannot say anything about a number this small.
            Assert.Equal(1.0, placement.Length / 1e-200, Places);
            Assert.True(placement.Length > 0.0);
        }

        [Fact]
        public void ALineFarLargerThanAnyModelDoesNotReportInfinity()
        {
            PlotBox huge = new PlotBox("DM-41", 0, 0, 0, 1e200, 4e200, 1e200);

            SectionPlacement placement = SectionPlacement.Across(huge, SectionAxis.ShortSide, SomeDepth);

            Assert.False(double.IsInfinity(placement.Length));
            Assert.Equal(1.0, placement.Length / 1e200, Places);
        }
    }
}
