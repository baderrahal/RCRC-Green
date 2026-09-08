using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class SheetDefinitionTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static SheetDefinition Sheet(params SheetViewPlacement[] views)
        {
            return new SheetDefinition("A1 Title Block", "A1 Landscape", 3.5, 2.3, views);
        }

        [Fact]
        public void TheTitleBlockAndTheSizeComeBackAsGiven()
        {
            SheetDefinition sheet = Sheet();

            Assert.Equal("A1 Title Block", sheet.TitleBlockFamilyName);
            Assert.Equal("A1 Landscape", sheet.TitleBlockTypeName);
            Assert.Equal(3.5, sheet.SheetWidth);
            Assert.Equal(2.3, sheet.SheetHeight);
        }

        /// <summary>
        /// A new sheet is created with a title block and nothing else, so a source sheet
        /// carrying none cannot be copied at all.
        /// </summary>
        [Fact]
        public void ASheetWithNoTitleBlockCannotBeUsed()
        {
            Assert.False(new SheetDefinition(string.Empty, string.Empty, 0, 0, null).CanBeUsed);
            Assert.False(new SheetDefinition("A1 Title Block", string.Empty, 0, 0, null).CanBeUsed);
            Assert.True(Sheet().CanBeUsed);
        }

        /// <summary>
        /// An empty source sheet is a real thing to copy. It makes an empty sheet, which is
        /// what somebody laying out a set by hand may want.
        /// </summary>
        [Fact]
        public void ASheetWithNoViewsOnItIsStillUsable()
        {
            SheetDefinition sheet = Sheet();

            Assert.True(sheet.CanBeUsed);
            Assert.Empty(sheet.Views);
            Assert.Equal(
                "Title block A1 Title Block A1 Landscape, no views on it. A new sheet would be empty.",
                sheet.InWords());
        }

        [Fact]
        public void ThePlacementsComeBackInViewTypeOrder()
        {
            SheetDefinition sheet = Sheet(
                new SheetViewPlacement(Hardscape, 1.0, 2.0, true),
                new SheetViewPlacement(General, 3.0, 4.0, false),
                new SheetViewPlacement(KeyPlan, 5.0, 6.0, false));

            Assert.Equal(
                new[] { "(010) Location Key Plan", "(200) General Arrangement Layout", "(600) HARDSCAPE SCHEDULE" },
                sheet.ViewTypes.Select(one => one.ToString()).ToArray());
        }

        /// <summary>
        /// One position per view type. A source sheet holding the same type twice gives no way
        /// to say which position a new one takes, and placing both would put two views on top
        /// of each other.
        /// </summary>
        [Fact]
        public void TheSameViewTypeTwiceKeepsTheFirstPosition()
        {
            SheetDefinition sheet = Sheet(
                new SheetViewPlacement(General, 1.0, 2.0, false),
                new SheetViewPlacement(General, 9.0, 9.0, false));

            SheetViewPlacement only = Assert.Single(sheet.Views);
            Assert.Equal(1.0, only.CentreX);
            Assert.Equal(2.0, only.CentreY);
        }

        [Fact]
        public void APlacementIsFoundByItsViewType()
        {
            SheetDefinition sheet = Sheet(new SheetViewPlacement(General, 1.25, 0.75, false));

            SheetViewPlacement found = sheet.PlacementFor(
                new ViewType("200", "General Arrangement Layout"));

            Assert.NotNull(found);
            Assert.Equal(1.25, found.CentreX);
            Assert.False(found.IsASchedule);

            Assert.Null(sheet.PlacementFor(KeyPlan));
            Assert.Null(sheet.PlacementFor(null));
        }

        /// <summary>
        /// A schedule on a sheet is a different element from a drawing on a sheet and is placed
        /// by a different call. Losing that flag would drop every schedule off a copied layout.
        /// </summary>
        [Fact]
        public void AScheduleOnTheSheetIsMarkedAsOne()
        {
            SheetDefinition sheet = Sheet(
                new SheetViewPlacement(General, 1.0, 1.0, false),
                new SheetViewPlacement(Hardscape, 2.0, 2.0, true));

            Assert.False(sheet.PlacementFor(General).IsASchedule);
            Assert.True(sheet.PlacementFor(Hardscape).IsASchedule);
        }

        [Fact]
        public void TheWordsNameTheTitleBlockAndEveryViewOnIt()
        {
            SheetDefinition sheet = Sheet(
                new SheetViewPlacement(General, 1.0, 1.0, false),
                new SheetViewPlacement(KeyPlan, 2.0, 2.0, false));

            Assert.Equal(
                "Title block A1 Title Block A1 Landscape, 2 views on it: "
                + "(010) Location Key Plan, (200) General Arrangement Layout",
                sheet.InWords());
        }

        [Fact]
        public void ASheetThatCannotBeUsedSaysSoRatherThanListingNothing()
        {
            Assert.Equal(
                "That sheet carries no title block, so nothing can be copied from it.",
                new SheetDefinition(string.Empty, string.Empty, 0, 0, null).InWords());
        }

        /// <summary>
        /// A position that is not a number survives every later check and then places a view
        /// nowhere, so it is refused at the door the same way a scope box bound is.
        /// </summary>
        [Fact]
        public void APositionThatIsNotANumberIsRefused()
        {
            Assert.Throws<ArgumentException>(
                () => new SheetViewPlacement(General, double.NaN, 0.0, false));
            Assert.Throws<ArgumentException>(
                () => new SheetViewPlacement(General, 0.0, double.PositiveInfinity, false));
            Assert.Throws<ArgumentNullException>(
                () => new SheetViewPlacement(null, 0.0, 0.0, false));
        }
    }

    public class SheetRequestTests
    {
        [Fact]
        public void BothBoxesFilledInIsComplete()
        {
            var wanted = new SheetRequest("DM-11", "L-201", "General Arrangement");

            Assert.True(wanted.Complete);
            Assert.False(wanted.Blank);
            Assert.Equal(string.Empty, wanted.WhatIsMissing);
        }

        /// <summary>
        /// Which box is empty, not just that one is. Otherwise somebody reads the report and
        /// then hunts across two columns to find out which.
        /// </summary>
        [Fact]
        public void AMissingBoxIsNamed()
        {
            Assert.Equal("a sheet name", new SheetRequest("DM-11", "L-201", "  ").WhatIsMissing);
            Assert.Equal("a sheet number", new SheetRequest("DM-11", string.Empty, "Layout").WhatIsMissing);
            Assert.Equal(
                "a sheet number and a sheet name",
                new SheetRequest("DM-11", null, null).WhatIsMissing);
        }

        /// <summary>
        /// Both boxes empty is a plot the user did not ask for a sheet on, which is different
        /// from one they filled in badly. Refusing it would fill the report with rows nobody
        /// asked about.
        /// </summary>
        [Fact]
        public void BothBoxesEmptyIsBlankRatherThanIncomplete()
        {
            Assert.True(new SheetRequest("DM-11", null, null).Blank);
            Assert.False(new SheetRequest("DM-11", "L-201", null).Blank);
        }

        [Fact]
        public void SurroundingSpaceIsNotAValue()
        {
            var wanted = new SheetRequest("DM-11", "  L-201  ", "  General Arrangement ");

            Assert.Equal("L-201", wanted.SheetNumber);
            Assert.Equal("General Arrangement", wanted.SheetName);
            Assert.True(wanted.Complete);
        }
    }
}
