using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class PlotSelectionTests
    {
        private static readonly string[] Range =
        {
            "DM-11", "DM-12", "DM-13", "DM-14", "DM-15", "DM-16", "DM-17", "DM-18", "DM-19",
            "DM-20", "DM-21"
        };

        [Fact]
        public void EverythingInRangeStartsTicked()
        {
            PlotSelection picked = PlotSelection.AllOf(Range);

            Assert.Equal(11, picked.InRangeCount);
            Assert.Equal(11, picked.TickedCount);
        }

        /// <summary>
        /// The case the user described. DM-11 to DM-28 but leave out DM-15 and DM-20.
        /// </summary>
        [Fact]
        public void UntickingTwoLeavesTheRestAndTakesThoseTwoOut()
        {
            PlotSelection picked = PlotSelection.AllOf(Range)
                .Ticking("DM-15", false)
                .Ticking("DM-20", false);

            Assert.Equal(
                new[] { "DM-11", "DM-12", "DM-13", "DM-14", "DM-16", "DM-17", "DM-18", "DM-19", "DM-21" },
                picked.Ticked);
            Assert.Equal(9, picked.TickedCount);
            Assert.Equal(11, picked.InRangeCount);
        }

        [Fact]
        public void AnUntickedPlotIsStillInRange()
        {
            PlotSelection picked = PlotSelection.AllOf(Range).Ticking("DM-15", false);

            Assert.Contains("DM-15", picked.InRange);
            Assert.False(picked.IsTicked("DM-15"));
        }

        [Fact]
        public void TickingItAgainPutsItBack()
        {
            PlotSelection picked = PlotSelection.AllOf(Range)
                .Ticking("DM-15", false)
                .Ticking("DM-15", true);

            Assert.True(picked.IsTicked("DM-15"));
            Assert.Equal(11, picked.TickedCount);
        }

        [Fact]
        public void UntickingTheSameOneTwiceIsStillOneUntick()
        {
            PlotSelection picked = PlotSelection.AllOf(Range)
                .Ticking("DM-15", false)
                .Ticking("DM-15", false);

            Assert.Equal(10, picked.TickedCount);
        }

        /// <summary>
        /// Nothing outside the range can be ticked, so a stale click cannot put a plot the user
        /// cannot see into what gets written.
        /// </summary>
        [Fact]
        public void APlotOutsideTheRangeCannotBeTicked()
        {
            PlotSelection picked = PlotSelection.AllOf(Range).Ticking("PF-3", true);

            Assert.False(picked.IsTicked("PF-3"));
            Assert.DoesNotContain("PF-3", picked.Ticked);
            Assert.Equal(11, picked.InRangeCount);
        }

        [Fact]
        public void ANewRangeStartsAllTickedAgain()
        {
            PlotSelection picked = PlotSelection.AllOf(Range).Ticking("DM-15", false);
            PlotSelection afterTheRangeChanged = PlotSelection.AllOf(new[] { "DM-15", "DM-16" });

            Assert.Equal(2, afterTheRangeChanged.TickedCount);
            Assert.True(afterTheRangeChanged.IsTicked("DM-15"));
            Assert.Equal(10, picked.TickedCount);
        }

        [Fact]
        public void UntickingEveryPlotLeavesNothingTicked()
        {
            PlotSelection picked = PlotSelection.AllOf(new[] { "DM-11", "DM-12" })
                .Ticking("DM-11", false)
                .Ticking("DM-12", false);

            Assert.Equal(0, picked.TickedCount);
            Assert.Empty(picked.Ticked);
            Assert.Equal(2, picked.InRangeCount);
        }

        [Fact]
        public void NoRangeAtAllIsEmptyRatherThanAThrow()
        {
            Assert.Equal(0, PlotSelection.Nothing.InRangeCount);
            Assert.Equal(0, PlotSelection.Nothing.TickedCount);
            Assert.False(PlotSelection.Nothing.IsTicked("DM-11"));
        }

        [Fact]
        public void ARangeListedTwiceHoldsEachPlotOnce()
        {
            PlotSelection picked = PlotSelection.AllOf(new[] { "DM-11", "DM-11", "DM-12" });

            Assert.Equal(2, picked.InRangeCount);
        }
    }
}
