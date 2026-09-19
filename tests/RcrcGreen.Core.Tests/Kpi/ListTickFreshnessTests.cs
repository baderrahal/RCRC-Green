using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE LIST TICK IS THE LAST TICK.** Bader's rule of 17 September. Ticking a workbook row
    /// ticks that template's plots, so a row ticked after Tick the list adds plots the team's
    /// list never named, and a row unticked takes some of the list's plots off again. Either way
    /// the ticks are no longer the list, and the pane said nothing about it.
    ///
    /// **NOTHING IS TICKED OR UNTICKED BEHIND HIS BACK.** The pane says the list tick is out of
    /// date and offers the button. It presses nothing itself.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class ListTickFreshnessTests
    {
        /// <summary>
        /// **NOTHING IS OUT OF DATE BEFORE THE BUTTON HAS EVER BEEN PRESSED.** There is no press
        /// for a row to have moved out from under, and a line saying the list tick is stale on a
        /// pane where nobody pressed it is a line about nothing.
        /// </summary>
        [Fact]
        public void BeforeTheButtonIsEverPressedNothingIsOutOfDate()
        {
            ListTickFreshness never = ListTickFreshness.NeverPressed;

            Assert.False(never.EverPressed);
            Assert.False(never.OutOfDate(new[] { "MOSQUES.xlsx", "STREETS.xlsx" }));
            Assert.Equal(string.Empty, never.InWords(new[] { "MOSQUES.xlsx" }));
        }

        /// <summary>
        /// **THE ROWS AS THEY STOOD AT THE PRESS STILL STANDING IS FRESH.**
        /// </summary>
        [Fact]
        public void TheSameRowsAfterThePressAreStillFresh()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(
                new[] { "GRP KPI Checklist MOSQUES.xlsx", "GRP KPI Checklist STREETS.xlsx" });

            Assert.True(pressed.EverPressed);
            Assert.False(pressed.OutOfDate(
                new[] { "GRP KPI Checklist MOSQUES.xlsx", "GRP KPI Checklist STREETS.xlsx" }));
            Assert.Equal(string.Empty, pressed.InWords(
                new[] { "GRP KPI Checklist MOSQUES.xlsx", "GRP KPI Checklist STREETS.xlsx" }));
        }

        /// <summary>
        /// **THE ORDER IS NO PART OF IT.** Which row was ticked first says nothing about which
        /// plots are ticked, so two lists holding the same rows in two orders are one answer.
        /// </summary>
        [Fact]
        public void TheOrderOfTheRowsIsNoPartOfIt()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(
                new[] { "MOSQUES.xlsx", "STREETS.xlsx" });

            Assert.False(pressed.OutOfDate(new[] { "STREETS.xlsx", "MOSQUES.xlsx" }));
        }

        /// <summary>
        /// **A ROW TICKED AFTER THE PRESS IS OUT OF DATE.** This is the case Bader named: the
        /// press ticked the list's 154 plots and ticking SCHOOLS then adds every school plot on
        /// top of them.
        /// </summary>
        [Fact]
        public void ARowTickedAfterThePressIsOutOfDate()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" });

            Assert.True(pressed.OutOfDate(new[] { "MOSQUES.xlsx", "SCHOOLS.xlsx" }));

            Assert.Equal(
                "A workbook row has been ticked or unticked since Tick the list was pressed, so "
                + "the ticked plots are no longer the plots the list names. Press Tick the list "
                + "again to put them back. Nothing has been ticked or unticked for you.",
                pressed.InWords(new[] { "MOSQUES.xlsx", "SCHOOLS.xlsx" }));
        }

        /// <summary>
        /// **A ROW UNTICKED AFTER THE PRESS IS OUT OF DATE TOO**, because unticking a workbook
        /// row unticks that template's plots, which takes some of the list's own plots off.
        /// </summary>
        [Fact]
        public void ARowUntickedAfterThePressIsOutOfDate()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(
                new[] { "MOSQUES.xlsx", "STREETS.xlsx" });

            Assert.True(pressed.OutOfDate(new[] { "MOSQUES.xlsx" }));
        }

        /// <summary>
        /// **EVERY ROW UNTICKED IS OUT OF DATE.** An empty set is a different set, and it is the
        /// state where the list's plots have all come off again.
        /// </summary>
        [Fact]
        public void EveryRowUntickedIsOutOfDate()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" });

            Assert.True(pressed.OutOfDate(new string[0]));
        }

        /// <summary>
        /// **TICKED AND UNTICKED BACK IS FRESH, AND THAT IS THE DELIBERATE READING.** The
        /// question is whether the ticks the list press produced still stand, not whether
        /// anybody touched a box. Ticking SCHOOLS and taking it off again leaves exactly the
        /// plots the list named, so a line saying the list tick is stale would send somebody to
        /// press a button that would change nothing.
        /// </summary>
        [Fact]
        public void ARowTickedAndUntickedAgainIsFresh()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" });

            Assert.True(pressed.OutOfDate(new[] { "MOSQUES.xlsx", "SCHOOLS.xlsx" }));
            Assert.False(pressed.OutOfDate(new[] { "MOSQUES.xlsx" }));
        }

        /// <summary>
        /// **A ROW SWAPPED FOR ANOTHER IS OUT OF DATE**, even though the count did not move. A
        /// counter would have read this as fresh, which is why it is a set.
        /// </summary>
        [Fact]
        public void ARowSwappedForAnotherIsOutOfDateThoughTheCountDidNotMove()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" });

            Assert.True(pressed.OutOfDate(new[] { "SCHOOLS.xlsx" }));
        }

        /// <summary>
        /// **THE ROWS ARE COMPARED THE WAY THE PANE COMPARES THEM**, `StringComparer.Ordinal`,
        /// which is what `KpiPanel.TickFor` uses on a workbook's file name. A comparison that
        /// forgave case here would be a second rule for one question.
        /// </summary>
        [Fact]
        public void TheRowsAreComparedTheWayThePaneComparesThem()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(new[] { "MOSQUES.xlsx" });

            Assert.True(pressed.OutOfDate(new[] { "mosques.xlsx" }));
        }

        /// <summary>
        /// The rows at the press are kept as given, so a pane can say which they were.
        /// </summary>
        [Fact]
        public void TheRowsAtThePressAreKept()
        {
            ListTickFreshness pressed = ListTickFreshness.Pressed(
                new[] { "MOSQUES.xlsx", "STREETS.xlsx" });

            Assert.Equal(new[] { "MOSQUES.xlsx", "STREETS.xlsx" }, pressed.RowsAtThePress);
            Assert.Empty(ListTickFreshness.NeverPressed.RowsAtThePress);
        }
    }
}
