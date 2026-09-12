using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Step 1's list: the plots are the model's two letter prefixes, several tickable at
    /// once, each ticked one carrying a From and To over its sub plots and a tick per sub
    /// plot. Every expectation is written out by hand.
    /// </summary>
    public class PlotTickListTests
    {
        private static readonly string[] Model =
        {
            "DM-41", "DM-42", "DM-43", "FP-1", "FP-2"
        };

        [Fact]
        public void NothingIsTickedToBeginWith()
        {
            PlotTickList list = PlotTickList.Over(Model);

            Assert.Equal(new[] { "DM", "FP" }, list.Prefixes);
            Assert.Equal(0, list.TickedPlotCount);
            Assert.Empty(list.InRange);
            Assert.Empty(list.Ticked);
            Assert.False(list.IsPlotTicked("DM"));
            Assert.False(list.IsTicked("DM-41"));
        }

        /// <summary>
        /// One tick is one click's worth of work: the whole span, every sub plot ticked,
        /// the ends filled in.
        /// </summary>
        [Fact]
        public void TickingAPlotTakesItsWholeSpanTicked()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("DM", true);

            Assert.Equal(new[] { "DM" }, list.TickedPlots);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.InRange);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);
            Assert.Equal("DM-41", list.FromOf("DM"));
            Assert.Equal("DM-43", list.ToOf("DM"));
            Assert.Equal(string.Empty, list.FromOf("FP"));
        }

        /// <summary>
        /// The list runs in plot order however the plots were ticked, so the grid and the
        /// run read one order.
        /// </summary>
        [Fact]
        public void TwoPlotsTickedRunInPlotOrderNotTickOrder()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("FP", true)
                .TickingPlot("DM", true);

            Assert.Equal(
                new[] { "DM-41", "DM-42", "DM-43", "FP-1", "FP-2" }, list.InRange);
            Assert.Equal(2, list.TickedPlotCount);
            Assert.Equal(5, list.InRangeCount);
            Assert.Equal(5, list.TickedCount);
        }

        /// <summary>
        /// The rule the old range had: changing it rebuilds that plot's selection with
        /// everything ticked. The other plot is left exactly as it was.
        /// </summary>
        [Fact]
        public void NarrowingARangeResetsThatPlotsTicksAndNoOthers()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .TickingPlot("FP", true)
                .Ticking("FP-2", false)
                .Ticking("DM-42", false);

            Assert.Equal(new[] { "DM-41", "DM-43", "FP-1" }, list.Ticked);

            list = list.Ranging("DM", "DM-41", "DM-42");

            Assert.Equal(new[] { "DM-41", "DM-42", "FP-1" }, list.Ticked);
            Assert.Equal("DM-42", list.ToOf("DM"));
            Assert.False(list.IsTicked("FP-2"));
            Assert.Equal(new[] { "DM-41", "DM-42" }, list.InRangeOf("DM"));
            Assert.Empty(PlotTickList.Over(Model).InRangeOf("DM"));
        }

        [Fact]
        public void UntickingAPlotForgetsItsRangeAndItsTicks()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "DM-41", "DM-42")
                .Ticking("DM-41", false)
                .TickingPlot("DM", false);

            Assert.Empty(list.InRange);
            Assert.Equal(string.Empty, list.FromOf("DM"));

            list = list.TickingPlot("DM", true);

            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);
        }

        [Fact]
        public void ASubPlotTickRoutesToItsOwnPlot()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .TickingPlot("FP", true)
                .Ticking("FP-2", false);

            Assert.False(list.IsTicked("FP-2"));
            Assert.True(list.IsTicked("FP-1"));
            Assert.True(list.IsTicked("DM-42"));
            Assert.Equal(4, list.TickedCount);
            Assert.Equal(5, list.InRangeCount);
        }

        /// <summary>
        /// Every plot offered comes from the model and nothing anywhere can add one, the
        /// rule in core-rules.md. An unknown prefix, an unknown sub plot and a sub plot
        /// whose plot is not ticked all change nothing.
        /// </summary>
        [Fact]
        public void NothingOutsideTheModelOrTheTickedPlotsCanBeTicked()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("ZZ", true);
            Assert.Equal(0, list.TickedPlotCount);

            list = list.TickingPlot("DM", true).Ticking("DM-99", true).Ticking("FP-1", false);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);
            Assert.False(list.IsTicked("DM-99"));
            Assert.False(list.IsTicked("FP-1"));

            Assert.Empty(PlotTickList.Over(null).Prefixes);
            Assert.Same(PlotTickList.Nothing, PlotTickList.Nothing.Ticking(null, true));
        }

        /// <summary>
        /// Reversed ends keep what was picked and list nothing, the old range's answer,
        /// because an empty grid is a better answer than a corrected pick nobody made.
        /// </summary>
        [Fact]
        public void ReversedEndsShowAsPickedAndListNothing()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "DM-43", "DM-41");

            Assert.True(list.IsPlotTicked("DM"));
            Assert.Equal("DM-43", list.FromOf("DM"));
            Assert.Equal("DM-41", list.ToOf("DM"));
            Assert.Empty(list.InRange);
        }

        /// <summary>
        /// Natural order inside a plot, the PlotRange rule asked rather than repeated, so
        /// DM-2 comes before DM-30 and DM-30 before DM-100.
        /// </summary>
        [Fact]
        public void SubPlotsRunInNaturalOrder()
        {
            PlotTickList list = PlotTickList
                .Over(new[] { "DM-100", "DM-2", "DM-30" })
                .TickingPlot("DM", true);

            Assert.Equal(new[] { "DM-2", "DM-30", "DM-100" }, list.UnderPlot("DM"));
            Assert.Equal(new[] { "DM-2", "DM-30", "DM-100" }, list.InRange);
        }

        [Fact]
        public void RangingAnUntickedPlotChangesNothing()
        {
            PlotTickList list = PlotTickList.Over(Model);

            Assert.Same(list, list.Ranging("DM", "DM-41", "DM-42"));
            Assert.Same(list, list.TickingPlot("DM", false));
        }
    }
}
