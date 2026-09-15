using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **NOTHING KEPT THE HAND CHOICES AND THE REAL TICKS IN STEP, AND NOTHING EVER PRINTED
    /// THEM.** Three routes had come apart by the eighty third pass, and they are one fault.
    /// Unticking a workbook row removed every plot of that template without asking the hand list
    /// and without recording what it took. Select all and Clear replaced every tick and left the
    /// list standing, so the next row press dropped a plot the person had just put back. And the
    /// sentence that would have shown any of it on the row was built, tested and called nowhere.
    ///
    /// Every expected value here is written out by hand.
    /// </summary>
    public class HandTicksTests
    {
        private static readonly string[] InTheModel =
        {
            "DM-12", "DM-14", "FM-05", "SC-03", "ST-05"
        };

        private static readonly Func<string, string> OnTheSheets = plotId =>
        {
            var held = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "DM-12", "DAILY MOSQUE" },
                { "DM-14", "DAILY MOSQUE" },
                { "FM-05", "FRIDAY MOSQUE" },
                { "SC-03", "SCHOOL" },
                { "ST-05", "STREET 36m ROW" }
            };

            string component;
            return held.TryGetValue(plotId, out component) ? component : string.Empty;
        };

        private static PlotTicks Nothing()
        {
            return new PlotTicks(PlotsInTheModel.Of(InTheModel, InTheModel));
        }

        /// <summary>
        /// A plot is in one direction or neither, never both, so the second press over one plot
        /// replaces the first rather than leaving two records of it.
        /// </summary>
        [Fact]
        public void APlotIsTakenOffOrPutOnAndNeverBoth()
        {
            HandTicks off = HandTicks.None.TakenOff("DM-14");

            Assert.True(off.IsOff("DM-14"));
            Assert.False(off.IsOn("DM-14"));
            Assert.Equal(new[] { "DM-14" }, off.Off.ToArray());
            Assert.Empty(off.On);

            HandTicks on = off.PutOn("DM-14");

            Assert.False(on.IsOff("DM-14"));
            Assert.True(on.IsOn("DM-14"));
            Assert.Empty(on.Off);
            Assert.Equal(new[] { "DM-14" }, on.On.ToArray());

            Assert.Equal(1, on.Count);
            Assert.Equal("1 plot picked by hand: 1 on, 0 off.", on.InWords);
            Assert.Equal(HandTicks.NothingByHand, HandTicks.None.InWords);
            Assert.Equal(
                "2 plots picked by hand: 1 on, 1 off.",
                HandTicks.None.PutOn("DM-12").TakenOff("DM-14").InWords);
        }

        /// <summary>
        /// **ITEM 1. UNTICKING A ROW WAS NOT SYMMETRIC WITH TICKING IT.** A hand untick survived
        /// a row tick and a hand tick did not survive a row untick, so a person who picked three
        /// mosque plots, then ticked the MOSQUES row for the rest, then changed their mind about
        /// the row, lost their three with nothing said.
        /// </summary>
        [Fact]
        public void UntickingARowKeepsAPlotThePersonPutOnByHand()
        {
            HandTicks byHand = HandTicks.None.PutOn("DM-12");
            PlotTicks picked = Nothing().With("DM-12");

            PlotTicks row = TickingATemplate.Ticked(
                picked, KpiTemplates.Mosques, OnTheSheets, byHand);

            Assert.Equal(new[] { "DM-12", "DM-14", "FM-05" }, row.Ticked.ToArray());

            PlotTicks after = TickingATemplate.Unticked(
                row, KpiTemplates.Mosques, OnTheSheets, byHand);

            // DM-12 was the person's own and stays. The two the row brought in go.
            Assert.Equal(new[] { "DM-12" }, after.Ticked.ToArray());
        }

        /// <summary>
        /// **AND THE ROUND TRIP RESTORES THE SAME STATE**, which is what symmetric means here.
        /// Ticking a row and unticking it leaves the ticks exactly as they were, in both hand
        /// directions at once.
        /// </summary>
        [Fact]
        public void TickingARowAndUntickingItLeavesTheTicksWhereTheyWere()
        {
            HandTicks byHand = HandTicks.None.PutOn("DM-12").TakenOff("DM-14");
            PlotTicks before = Nothing().With("DM-12").With("ST-05");

            PlotTicks row = TickingATemplate.Ticked(
                before, KpiTemplates.Mosques, OnTheSheets, byHand);

            // DM-14 is held off by hand, so the row brings in FM-05 alone.
            Assert.Equal(new[] { "DM-12", "FM-05", "ST-05" }, row.Ticked.ToArray());

            PlotTicks after = TickingATemplate.Unticked(
                row, KpiTemplates.Mosques, OnTheSheets, byHand);

            Assert.Equal(before.Ticked.ToArray(), after.Ticked.ToArray());
            Assert.Equal(new[] { "DM-12", "ST-05" }, after.Ticked.ToArray());
        }

        /// <summary>
        /// **ITEM 2. SELECT ALL AND CLEAR FORGET EVERY HAND CHOICE.** Pressing Select all is a
        /// person saying they want everything and pressing Clear is a person starting over, and
        /// both replace every tick, so a per plot choice left standing behind them is a record
        /// that disagrees with what is on screen.
        /// </summary>
        [Fact]
        public void SelectAllAndClearForgetEveryHandChoice()
        {
            HandTicks byHand = HandTicks.None.PutOn("DM-12").TakenOff("DM-14");

            Assert.Equal(2, byHand.Count);
            Assert.Same(HandTicks.None, byHand.Forgotten());
            Assert.Equal(0, byHand.Forgotten().Count);

            // Select all: every plot ticked and nothing held off, so the next row press brings
            // in all three mosque plots rather than dropping DM-14 again.
            PlotTicks all = Nothing().All();
            PlotTicks after = TickingATemplate.Ticked(
                all, KpiTemplates.Mosques, OnTheSheets, byHand.Forgotten());

            Assert.Equal(
                new[] { "DM-12", "DM-14", "FM-05", "SC-03", "ST-05" }, after.Ticked.ToArray());

            // **WHAT IT DID BEFORE.** Carrying the old record past Select all drops DM-14 on the
            // very next row press, which is the fault this closes.
            PlotTicks dropped = TickingATemplate.Unticked(
                TickingATemplate.Ticked(all, KpiTemplates.Mosques, OnTheSheets, byHand),
                KpiTemplates.Mosques, OnTheSheets, byHand);

            Assert.DoesNotContain("DM-14", dropped.Ticked);
        }

        /// <summary>
        /// **ITEM 3. THE ROW'S OWN LINE, COUNTED IN CORE.** The pane counts nothing: it asks for
        /// the line and draws it, and a row where every plot is going in gets no line at all,
        /// because a line about nothing is one the team reads past on every other press.
        /// </summary>
        [Fact]
        public void TheRowSaysHowManyOfItsPlotsAreGoingInAndSaysNothingWhenTheyAllAre()
        {
            PlotTicks whole = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, OnTheSheets, HandTicks.None);

            Assert.Equal(
                string.Empty, TickingATemplate.RowLine(KpiTemplates.Mosques, whole, OnTheSheets));

            PlotTicks short2 = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, OnTheSheets, HandTicks.None.TakenOff("DM-14"));

            Assert.Equal(
                "MOSQUES: 2 of the 3 plots that belong to it, the rest unticked by hand.",
                TickingATemplate.RowLine(KpiTemplates.Mosques, short2, OnTheSheets));

            // A row whose plots are all off says so rather than saying nothing, which is the
            // state a second row settled as the same template can leave behind.
            Assert.Equal(
                "MOSQUES: 0 of the 3 plots that belong to it, the rest unticked by hand.",
                TickingATemplate.RowLine(KpiTemplates.Mosques, Nothing(), OnTheSheets));

            Assert.Equal(
                string.Empty, TickingATemplate.RowLine(KpiTemplates.Mosques, null, OnTheSheets));
        }
    }
}
