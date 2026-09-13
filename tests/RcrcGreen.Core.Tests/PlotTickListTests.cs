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
    /// models. **The range decides the LIST and the model decides the TICKS.** It was the
    /// other way round, so widening the range changed nothing on screen.
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

            // A model holding one sub plot lists all 99 and ticks the one it holds.
            PlotTickList small = PlotTickList.Over(new[] { "DM-41" }).TickingPlot("DM", true);
            Assert.Equal("01", small.FromOf("DM"));
            Assert.Equal("99", small.ToOf("DM"));
            Assert.Equal(99, small.InRangeOf("DM").Count);
            Assert.Equal("DM-01", small.InRangeOf("DM")[0]);
            Assert.Equal("DM-99", small.InRangeOf("DM")[98]);
            Assert.Equal(new[] { "DM-41" }, small.Ticked);
        }

        /// <summary>
        /// One tick is one click's worth of work: the whole 01 to 99 range listed, and the
        /// sub plots the model holds inside it ticked. The other 96 are listed and not
        /// ticked, or a single tick on a plot would queue a run over sub plots nobody has
        /// looked at.
        /// </summary>
        [Fact]
        public void TickingAPlotListsTheWholeRangeAndTicksWhatTheModelHolds()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("DM", true);

            Assert.Equal(new[] { "DM" }, list.TickedPlots);
            Assert.Equal(99, list.InRange.Count);
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Ticked);
            Assert.Equal("01", list.FromOf("DM"));
            Assert.Equal("99", list.ToOf("DM"));
            Assert.Equal(99, list.NumbersInRange);
            Assert.Equal(string.Empty, list.FromOf("FP"));

            Assert.True(list.IsTicked("DM-41"));
            Assert.False(list.IsTicked("DM-01"));
        }

        /// <summary>
        /// A range the open model holds nothing in lists every sub plot it covers and ticks
        /// none of them. This listed nothing, which is what made the range read as broken.
        /// </summary>
        [Fact]
        public void ARangeTheModelHoldsNothingInListsItsCoverAndTicksNone()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "50", "53");

            Assert.True(list.IsPlotTicked("DM"));
            Assert.Equal("50", list.FromOf("DM"));
            Assert.Equal("53", list.ToOf("DM"));
            Assert.Equal(
                new[] { "DM-50", "DM-51", "DM-52", "DM-53" }, list.InRangeOf("DM"));
            Assert.Empty(list.Ticked);
            Assert.Equal(4, list.NumbersInRange);

            // And it can be ticked, which is the whole point. DM-02 holds no views and no
            // scope box on the real model and the team has made its sheets.
            Assert.True(list.Ticking("DM-51", true).IsTicked("DM-51"));
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

            Assert.Equal(2, list.TickedPlotCount);
            Assert.Equal(198, list.InRangeCount);
            Assert.Equal(5, list.TickedCount);
            Assert.Equal(198, list.NumbersInRange);

            // Plot order however they were ticked, so the grid and the run read one order.
            Assert.Equal(
                new[] { "DM-41", "DM-42", "DM-43", "FP-1", "FP-2" }, list.Ticked);
            Assert.Equal("DM-01", list.InRange[0]);
            Assert.Equal("FP-99", list.InRange[197]);
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
        /// A one digit sub plot sits at its own number, and the model's own spelling is what
        /// the line carries. Generating FP-01 beside a model that spells it FP-1 would give
        /// two lines for one sub plot and leave the views on the one nobody ticked.
        /// </summary>
        [Fact]
        public void TheModelsOwnSpellingWinsOverTheGeneratedOne()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("FP", true)
                .Ranging("FP", "01", "04");

            Assert.Equal(
                new[] { "FP-1", "FP-2", "FP-03", "FP-04" }, list.InRangeOf("FP"));
            Assert.Equal(new[] { "FP-1", "FP-2" }, list.Ticked);

            Assert.Equal(
                new[] { "FP-03", "FP-04" }, list.Ranging("FP", "03", "04").InRangeOf("FP"));

            Assert.Equal("DM-07", PlotTickList.Generated("DM", 7));
            Assert.Equal("DM-70", PlotTickList.Generated("DM", 70));
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
            Assert.Equal(198, list.InRangeCount);
        }

        /// <summary>
        /// What the search box narrows to. The filter reads the whole identifier, so 4
        /// finds DM-41 to DM-43 and 42 finds one, and it forgives case and edge spaces.
        /// </summary>
        [Fact]
        public void TheSearchNarrowsToWhatTheLineHolds()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "41", "43");

            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", string.Empty));
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", " 4 "));
            Assert.Equal(new[] { "DM-42" }, list.Matching("DM", "42"));
            Assert.Equal(new[] { "DM-41", "DM-42", "DM-43" }, list.Matching("DM", "dm"));
            Assert.Empty(list.Matching("DM", "99"));
            Assert.Empty(list.Matching("FP", "1"));

            // The search reaches a generated sub plot too, or a wide range would be
            // unsearchable at exactly the size that needs searching.
            Assert.Equal(
                new[] { "DM-50" },
                list.Ranging("DM", "49", "51").Matching("DM", "50"));
        }

        /// <summary>
        /// All and None act on what the filter is showing, which is the point of having
        /// them. Everything the search hides keeps the tick it had.
        /// </summary>
        [Fact]
        public void AllAndNoneActOnWhatTheSearchShows()
        {
            PlotTickList list = PlotTickList.Over(Model)
                .TickingPlot("DM", true)
                .Ranging("DM", "41", "43");

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
        /// rule in core-rules.md, and it is about the PLOT and not the sub plot. An unknown
        /// prefix and a sub plot whose plot is not ticked still change nothing. A sub plot
        /// inside a ticked plot's range is the user's to tick, whether the model holds it or
        /// not, which is what changed this round.
        /// </summary>
        [Fact]
        public void AnUnknownPlotCannotBeTickedAndAGeneratedSubPlotCan()
        {
            PlotTickList list = PlotTickList.Over(Model).TickingPlot("ZZ", true);
            Assert.Equal(0, list.TickedPlotCount);

            list = list.TickingPlot("DM", true).Ticking("DM-99", true).Ticking("FP-1", false);
            Assert.Equal(
                new[] { "DM-41", "DM-42", "DM-43", "DM-99" },
                list.Ticked.OrderBy(one => one, NaturalOrder.Comparer).ToArray());

            Assert.True(list.IsTicked("DM-99"));

            // FP is not ticked, so nothing under it can be.
            Assert.False(list.IsTicked("FP-1"));

            // Outside the range, so still nothing to tick.
            Assert.False(list.Ranging("DM", "41", "43").IsTicked("DM-99"));

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

            // DM-100 is outside every range the two digit picker offers, so it is not on the
            // list and cannot be ticked. Known, and the cost of two digit ends.
            Assert.Equal(99, list.InRange.Count);
            Assert.DoesNotContain("DM-100", list.InRange);
            Assert.Equal(new[] { "DM-2", "DM-30" }, list.Ticked);
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
