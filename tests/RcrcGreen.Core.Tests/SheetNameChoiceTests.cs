using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Which of the four places a sheet's name comes from.
    ///
    /// The reason this exists: a sheet holding more than one view asked for its name once per
    /// plot, so a run over 35 sub plots wanted the same words 35 times and the sixth definition
    /// read 34 rows still needing one after the user had typed it.
    /// </summary>
    public class SheetNameChoiceTests
    {
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");
        private static readonly ViewType Furniture = new ViewType("600", "FURNITURE SCHEDULE");

        private static SheetNameSettings Named(string name)
        {
            return SheetNameSettings.Of(
                new[] { new SheetNamePairing(Hardscape, name, SheetNameSource.UserFile) }, null);
        }

        private static PlannedSheet OneView(SheetNameSettings names = null)
        {
            return new PlannedSheet(new[] { Hardscape }, names);
        }

        private static PlannedSheet TwoViews()
        {
            return new PlannedSheet(new[] { Hardscape, Furniture });
        }

        [Fact]
        public void NothingTypedOnASheetThatNamesItselfTakesTheTableName()
        {
            SheetNameChoice choice = SheetNameChoice.Of(
                null, null, OneView(Named("HARDSCAPE SCHEDULES")));

            Assert.Equal("HARDSCAPE SCHEDULES", choice.Name);
            Assert.Equal(SheetNameFrom.Proposed, choice.Source);
            Assert.False(choice.WasTyped);
            Assert.True(choice.HasName);
            Assert.Equal(string.Empty, choice.InWords());
        }

        [Fact]
        public void NothingTypedOnASheetOfTwoViewsHasNoNameAtAll()
        {
            SheetNameChoice choice = SheetNameChoice.Of(null, null, TwoViews());

            Assert.Equal(string.Empty, choice.Name);
            Assert.Equal(SheetNameFrom.Nothing, choice.Source);
            Assert.False(choice.HasName);
            Assert.False(choice.WasTyped);
        }

        [Fact]
        public void TheDefinitionsNameIsUsedWhereverNoPlotOverridesIt()
        {
            SheetNameChoice choice = SheetNameChoice.Of(
                null, "HARDSCAPE SCHEDULES", TwoViews());

            Assert.Equal("HARDSCAPE SCHEDULES", choice.Name);
            Assert.Equal(SheetNameFrom.Definition, choice.Source);
            Assert.True(choice.WasTyped);
            Assert.Equal("From this sheet's name, used on every plot.", choice.InWords());
        }

        [Fact]
        public void APlotsOwnNameBeatsTheDefinitions()
        {
            SheetNameChoice choice = SheetNameChoice.Of(
                "DM-02 SCHEDULES", "HARDSCAPE SCHEDULES", TwoViews());

            Assert.Equal("DM-02 SCHEDULES", choice.Name);
            Assert.Equal(SheetNameFrom.OnThisPlot, choice.Source);
            Assert.Equal("Typed for this plot, over the name on the sheet.", choice.InWords());
        }

        [Fact]
        public void ClearingAPlotsBoxBeatsTheDefinitionRatherThanFallingBackToIt()
        {
            // An empty string is somebody emptying the box. Treating it as untouched would
            // make the clearing do nothing, which is the shape of a control that ignores you.
            SheetNameChoice choice = SheetNameChoice.Of(
                string.Empty, "HARDSCAPE SCHEDULES", TwoViews());

            Assert.Equal(string.Empty, choice.Name);
            Assert.Equal(SheetNameFrom.OnThisPlot, choice.Source);
            Assert.True(choice.WasTyped);
            Assert.False(choice.HasName);
        }

        [Fact]
        public void ClearingTheDefinitionsBoxDoesNotFallBackToTheTable()
        {
            SheetNameChoice choice = SheetNameChoice.Of(
                null, string.Empty, OneView(Named("HARDSCAPE SCHEDULES")));

            Assert.Equal(string.Empty, choice.Name);
            Assert.Equal(SheetNameFrom.Definition, choice.Source);
            Assert.False(choice.HasName);
        }

        [Fact]
        public void ADefinitionsNameBeatsTheTableOnASheetThatNamesItself()
        {
            SheetNameChoice choice = SheetNameChoice.Of(
                null, "SOMETHING ELSE", OneView(Named("HARDSCAPE SCHEDULES")));

            Assert.Equal("SOMETHING ELSE", choice.Name);
            Assert.Equal(SheetNameFrom.Definition, choice.Source);
        }

        [Fact]
        public void NoPlannedSheetAtAllIsNothingRatherThanAThrow()
        {
            SheetNameChoice choice = SheetNameChoice.Of(null, null, null);

            Assert.Equal(SheetNameFrom.Nothing, choice.Source);
            Assert.Equal(string.Empty, choice.Name);
        }

        [Fact]
        public void TheLineAboveTheRowsCountsThePlotsTheOneNameGoesOn()
        {
            Assert.Equal(
                "Named HARDSCAPE SCHEDULES on 35 ticked sub plots.",
                SheetNameChoice.OnEveryPlotInWords("HARDSCAPE SCHEDULES", 35));

            Assert.Equal(
                "Named HARDSCAPE SCHEDULES on the 1 ticked sub plot.",
                SheetNameChoice.OnEveryPlotInWords("HARDSCAPE SCHEDULES", 1));

            Assert.Equal(
                "This sheet has no name yet, so nothing is made on 35 ticked sub plots.",
                SheetNameChoice.OnEveryPlotInWords("   ", 35));

            Assert.Equal(
                "No plot is ticked, so this name goes on nothing yet.",
                SheetNameChoice.OnEveryPlotInWords("HARDSCAPE SCHEDULES", 0));
        }
    }
}
