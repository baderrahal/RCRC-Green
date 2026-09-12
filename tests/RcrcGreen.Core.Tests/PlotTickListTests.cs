using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Step 1's list: the plots are the model's two letter prefixes, several tickable at
    /// once, each ticked one carrying a From and To over the sub plot numbers and a tick
    /// per sub plot. Every expectation is written out by hand.
    ///
    /// The range ends are 01 to 99 whatever the model holds, because the team works across
    /// models. What the model holds decides the LIST, never the range.
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
            Assert.Equal(0, list.NumbersInRange);
            Assert.False(list.IsPlotTicked("DM"));
            Assert.False(list.IsTicked("DM-41"));
        }

        /// <summary>
        /// The ends are the same 99 on every model, digits only, so a range can be set up
        /// for sub plots a later model will hold.
        /// </summary>
        [Fact]
        public void TheRangeEndsAreZeroOneToNinetyNineWhateverTheModelHolds()
        {
            Assert.Equal(99, PlotTickList.RangeEnds.Count);
            Assert.Equal("01", PlotTickList.RangeEnds[0]);
            Assert.Equal("02", PlotTickList.RangeEnds[1]);
            Assert.Equal("09", PlotTickList.RangeEnds[8]);
            Assert.Equal("10", PlotTickList.RangeEnds[9]);
            Assert.Equal("99", PlotTickList.RangeEnds[98]);

            Assert.Equal("01", PlotTickList.AsEnd(1));
            Assert.Equal("07", PlotTickList.AsEnd(7));
            Assert.Equal("70", PlotTickList.AsEnd(70));

            // A model holding one sub plot still offers all 99, which is the whole point.
            PlotTickList small = PlotTickList.Over(new[] { "DM-41" }).TickingPlot("DM", true);
            Assert.Equal("01", small.FromOf("DM"));
            Assert.Equal("99", small.ToOf("DM"));
            Assert.Equal(new[] { "DM-41" }, small.InRangeOf("DM"));
        }

        /// <summary>
        /// One tick is one click's worth of work: the whole 01 to 99 range, every sub plot
        /// the model holds inside it ticked, the ends filled in.
        /// </summary>
        [Fact]
        public void TickingAPlotTakesTheWholeRangeTicked()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("DM", true);

            Assert.Equal(new[] { "DM" }, list.TickedPlots);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.InRange);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);
            Assert.Equal("01", list.FromOf("DM"));
            Assert.Equal("99", list.ToOf("DM"));
            Assert.Equal(99, list.NumbersInRange);
            Assert.Equal(string.Empty, list.FromOf("FP"));
        }

        /// <summary>
        /// The whole point of a free range: a range the open model holds nothing in is
        /// still picked and still shown, because the model it was set up for is not open
        /// yet. It lists nothing and it invents nothing.
        /// </summary>
        [Fact]
        public void ARangeTheModelHoldsNothingInIsKeptAndListsNothing()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "50", "60");

            Assert.True(list.IsPlotTicked("DM"));
            Assert.Equal("50", list.FromOf("DM"));
            Assert.Equal("60", list.ToOf("DM"));
            Assert.Empty(list.InRangeOf("DM"));
            Assert.Equal(11, list.NumbersInRange);
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
            Assert.Equal(198, list.NumbersInRange);
        }

        /// <summary>
        /// The rule the old range had: changing it rebuilds that plot's selection with
        /// everything ticked. The other plot is left exactly as it was. The ends are
        /// numbers now, so 41 to 42 keeps two of DM's three.
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

            list = list.Ranging("DM", "41", "42");

            Assert.Equal(new[] { "DM-41", "DM-42", "FP-1" }, list.Ticked);
            Assert.Equal("42", list.ToOf("DM"));
            Assert.False(list.IsTicked("FP-2"));
            Assert.Equal(new[] { "DM-41", "DM-42" }, list.InRangeOf("DM"));
            Assert.Empty(PlotTickList.Over(Model).InRangeOf("DM"));
        }

        /// <summary>
        /// A one digit sub plot sits at its own number, so 01 to 09 finds FP-1 and FP-2
        /// however the model spells them.
        /// </summary>
        [Fact]
        public void ASingleDigitSubPlotIsFoundByItsNumber()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("FP", true)
                .Ranging("FP", "01", "09");

            Assert.Equal(new[] { "FP-1", "FP-2" }, list.InRangeOf("FP"));

            Assert.Empty(list.Ranging("FP", "03", "09").InRangeOf("FP"));
        }

        [Fact]
        public void UntickingAPlotForgetsItsRangeAndItsTicks()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "41", "42")
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
        /// What the search box narrows to. The filter reads the whole identifier, so 4
        /// finds DM-41 to DM-43 and 42 finds one, and it forgives case and edge spaces.
        /// </summary>
        [Fact]
        public void TheSearchNarrowsToWhatTheLineHolds()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("DM", true);

            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", string.Empty));
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", " 4 "));
            Assert.Equal(new[] { "DM-42" }, list.Matching("DM", "42"));
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", "dm"));
            Assert.Empty(list.Matching("DM", "99"));
            Assert.Empty(list.Matching("FP", "1"));
        }

        /// <summary>
        /// All and None act on what the filter is showing, which is the point of having
        /// them. Everything the search hides keeps the tick it had.
        /// </summary>
        [Fact]
        public void AllAndNoneActOnWhatTheSearchShows()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("DM", true);

            list = list.TickingThese(list.Matching("DM", "4"), false);
            Assert.Empty(list.Ticked);

            list = list.TickingThese(list.Matching("DM", "42"), true);
            Assert.Equal(new[] { "DM-42" }, list.Ticked);

            list = list.TickingThese(list.Matching("DM", string.Empty), true);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);

            Assert.Same(list, list.TickingThese(null, false));
        }

        /// <summary>
        /// Every plot offered comes from the model and nothing anywhere can add one, the
        /// rule in core-rules.md. An unknown prefix, an unknown sub plot and a sub plot
        /// whose plot is not ticked all change nothing. The free range picker does not
        /// bend this: it offers two numbers, never a plot.
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
        /// Reversed or unreadable ends cover nothing and list nothing, the old range's
        /// answer, because an empty list is a better answer than a corrected pick nobody
        /// made.
        /// </summary>
        [Fact]
        public void ReversedOrUnreadableEndsShowAsPickedAndListNothing()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "43", "41");

            Assert.True(list.IsPlotTicked("DM"));
            Assert.Equal("43", list.FromOf("DM"));
            Assert.Equal("41", list.ToOf("DM"));
            Assert.Empty(list.InRange);
            Assert.Equal(0, list.NumbersInRange);

            PlotTickList unreadable = list.Ranging("DM", string.Empty, "99");
            Assert.Empty(unreadable.InRange);
            Assert.Equal(0, unreadable.NumbersInRange);
        }

        /// <summary>
        /// Natural order inside a plot, the PlotRange rule asked rather than repeated, so
        /// DM-2 comes before DM-30. DM-100 is outside every range the picker offers, which
        /// is the known cost of two digit ends and is said in the log.
        /// </summary>
        [Fact]
        public void SubPlotsRunInNaturalOrderAndAboveNinetyNineIsOutOfRange()
        {
            PlotTickList list = PlotTickList
                .Over(new[] { "DM-100", "DM-2", "DM-30" })
                .TickingPlot("DM", true);

            Assert.Equal(new[] { "DM-2", "DM-30", "DM-100" }, list.UnderPlot("DM"));
            Assert.Equal(new[] { "DM-2", "DM-30" }, list.InRange);
        }

        [Fact]
        public void RangingAnUntickedPlotChangesNothing()
        {
            PlotTickList list = PlotTickList.Over(Model);

            Assert.Same(list, list.Ranging("DM", "01", "99"));
            Assert.Same(list, list.TickingPlot("DM", false));
        }
    }
}
