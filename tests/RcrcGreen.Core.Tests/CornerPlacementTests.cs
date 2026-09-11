using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Putting a placement's centre where it was asked for.
    ///
    /// Every number here is off sheet 010QA of the run of 2026-09-11, in millimetres as the
    /// report printed them, and written out by hand. A difference carries the unit of its parts,
    /// so the arithmetic holds in millimetres exactly as it does in feet.
    /// </summary>
    public class CornerPlacementTests
    {
        /// <summary>
        /// What the run asked for. Every plan view on a one view sheet landed here, which is
        /// what says the placement maths was right and the schedule was not.
        /// </summary>
        private const double WantedX = 420.5;

        private const double WantedY = 297.0;

        /// <summary>
        /// Where the schedule's centre really came out.
        /// </summary>
        private const double LandedX = 522.1;

        private const double LandedY = 203.3;

        private const double ScheduleWide = 207.4;

        private const double ScheduleTall = 187.4;

        [Fact]
        public void TheScheduleOn010QAMovesLeftAndUp()
        {
            SheetMove move = CornerPlacement.MoveToPutTheCentreAt(
                LandedX, LandedY, WantedX, WantedY);

            Assert.Equal(-101.6, move.Across, 3);
            Assert.Equal(93.7, move.Up, 3);
            Assert.False(move.Nowhere);
        }

        /// <summary>
        /// The evidence that this is the corner and not something else. The schedule came out
        /// 93.7 low and half its own height is 93.7, which is exactly what happens when the
        /// point Revit is handed is the top left corner and the caller hands it a centre.
        ///
        /// Across, half the width is 103.7 and the schedule came out 101.6 over, 2.1 short. The
        /// bounding box a schedule reports is not exactly the width of what it draws, so the two
        /// are held against each other here rather than asserted equal.
        /// </summary>
        [Fact]
        public void HowFarItMovedIsHalfItsOwnSize()
        {
            Assert.Equal(93.7, ScheduleTall / 2.0, 3);
            Assert.Equal(103.7, ScheduleWide / 2.0, 3);
            Assert.Equal(2.1, (ScheduleWide / 2.0) - 101.6, 3);
        }

        /// <summary>
        /// A viewport was handed the same centre and landed on it, so it needs no move at all.
        /// Asking beats moving by nought, because a move Revit does not need is a regeneration
        /// nobody needs either.
        /// </summary>
        [Fact]
        public void SomethingAlreadyInTheRightPlaceMovesNowhere()
        {
            SheetMove move = CornerPlacement.MoveToPutTheCentreAt(
                WantedX, WantedY, WantedX, WantedY);

            Assert.True(move.Nowhere);
            Assert.Equal(0.0, move.Across);
            Assert.Equal(0.0, move.Up);
        }

        [Fact]
        public void TheMoveReadsAcrossThenUp()
        {
            SheetMove move = CornerPlacement.MoveToPutTheCentreAt(10.0, 20.0, 12.5, 17.0);

            Assert.Equal("2.5 across, -3 up", move.ToString());
        }

        /// <summary>
        /// A bounding box Revit could not read comes back as nothing and turns every centre into
        /// NaN, which would move a schedule somewhere nobody can find. Refused at the door, the
        /// same way PlotBox refuses a bound that is not a number.
        /// </summary>
        [Fact]
        public void ACentreThatIsNotANumberIsRefused()
        {
            Assert.Throws<ArgumentException>(
                () => CornerPlacement.MoveToPutTheCentreAt(double.NaN, LandedY, WantedX, WantedY));
            Assert.Throws<ArgumentException>(
                () => CornerPlacement.MoveToPutTheCentreAt(LandedX, LandedY, WantedX, double.PositiveInfinity));
        }
    }
}
