using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class SectionPlacementTests
    {
        private const int Places = 9;

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
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide);

            Assert.Equal(20.0, placement.Length, Places);
            Assert.Equal(50.0, placement.Start.X, Places);
            Assert.Equal(0.0, placement.Start.Y, Places);
            Assert.Equal(50.0, placement.End.X, Places);
            Assert.Equal(20.0, placement.End.Y, Places);
        }

        [Fact]
        public void TheLongAxisRunsTheOtherWayAcrossTheSameBox()
        {
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.LongSide);

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

            SectionPlacement placement = SectionPlacement.Across(tall, SectionAxis.ShortSide);

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
                SectionPlacement placement = SectionPlacement.Across(offOrigin, axis);

                Assert.Equal(60.0, placement.Midpoint.X, Places);
                Assert.Equal(-20.0, placement.Midpoint.Y, Places);
                Assert.Equal(6.0, placement.Midpoint.Z, Places);
            }
        }

        [Fact]
        public void BothEndsSitAtTheMiddleHeightOfTheBox()
        {
            SectionPlacement placement = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide);

            Assert.Equal(5.0, placement.Start.Z, Places);
            Assert.Equal(5.0, placement.End.Z, Places);
        }

        [Fact]
        public void TheViewLooksAcrossTheLineAndStaysHorizontal()
        {
            SectionPlacement shortWay = SectionPlacement.Across(WideBox(), SectionAxis.ShortSide);
            SectionPlacement longWay = SectionPlacement.Across(WideBox(), SectionAxis.LongSide);

            Assert.Equal(1.0, shortWay.ViewDirection.X, Places);
            Assert.Equal(0.0, shortWay.ViewDirection.Y, Places);
            Assert.Equal(0.0, shortWay.ViewDirection.Z, Places);

            Assert.Equal(0.0, longWay.ViewDirection.X, Places);
            Assert.Equal(-1.0, longWay.ViewDirection.Y, Places);
            Assert.Equal(0.0, longWay.ViewDirection.Z, Places);
        }

        [Fact]
        public void DepthReachesTheFaceOfTheBoxTheViewIsPointedAt()
        {
            Assert.Equal(50.0, SectionPlacement.Across(WideBox(), SectionAxis.ShortSide).Depth, Places);
            Assert.Equal(10.0, SectionPlacement.Across(WideBox(), SectionAxis.LongSide).Depth, Places);
        }

        [Fact]
        public void ASquareBoxTakesTheShortSideAlongYSoTheAnswerDoesNotMoveBetweenRuns()
        {
            PlotBox square = new PlotBox("AB-7", 0, 0, 0, 50, 50, 5);

            SectionPlacement shortWay = SectionPlacement.Across(square, SectionAxis.ShortSide);
            SectionPlacement longWay = SectionPlacement.Across(square, SectionAxis.LongSide);

            Assert.Equal(25.0, shortWay.Start.X, Places);
            Assert.Equal(0.0, shortWay.Start.Y, Places);
            Assert.Equal(0.0, longWay.Start.X, Places);
            Assert.Equal(25.0, longWay.Start.Y, Places);
        }

        [Fact]
        public void AFlatScopeBoxIsRefusedRatherThanGivenAZeroLengthLine()
        {
            PlotBox flat = new PlotBox("DM-41", 0, 5, 0, 100, 5, 10);

            Assert.Throws<ArgumentException>(() => SectionPlacement.Across(flat, SectionAxis.ShortSide));
        }

        [Fact]
        public void ABoxWithItsMaximumBelowItsMinimumIsRefused()
        {
            Assert.Throws<ArgumentException>(() => new PlotBox("DM-41", 0, 0, 0, 100, -20, 10));
        }
    }
}
