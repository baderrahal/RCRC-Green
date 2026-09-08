using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Where the views sit on a sheet.
    ///
    /// Every expected number is written out by hand from the sheet size, not worked out with
    /// the same division the code uses.
    /// </summary>
    public class SheetLayoutTests
    {
        /// <summary>
        /// An A1 sheet is 841 by 594 millimetres. The numbers below are that in tenths, so the
        /// arithmetic stays exact and readable rather than carrying a unit conversion.
        /// </summary>
        private const double Wide = 800.0;

        private const double Tall = 600.0;

        [Fact]
        public void OneViewSitsInTheMiddle()
        {
            var spots = SheetLayout.For(Wide, Tall, 1).ToArray();

            Assert.Single(spots);
            Assert.Equal(400.0, spots[0].CentreX);
            Assert.Equal(300.0, spots[0].CentreY);
        }

        /// <summary>
        /// Two side by side. Each has a quarter of the width either side of it, so the margin at
        /// the edge is the same as half the gap down the middle.
        /// </summary>
        [Fact]
        public void TwoSitSideBySideAtTheSameHeight()
        {
            var spots = SheetLayout.For(Wide, Tall, 2).ToArray();

            Assert.Equal(2, spots.Length);
            Assert.Equal(200.0, spots[0].CentreX);
            Assert.Equal(600.0, spots[1].CentreX);
            Assert.Equal(300.0, spots[0].CentreY);
            Assert.Equal(300.0, spots[1].CentreY);
        }

        [Fact]
        public void FourMakeATwoByTwoGrid()
        {
            var spots = SheetLayout.For(Wide, Tall, 4).ToArray();

            Assert.Equal(4, spots.Length);

            Assert.Equal(200.0, spots[0].CentreX);
            Assert.Equal(450.0, spots[0].CentreY);
            Assert.Equal(600.0, spots[1].CentreX);
            Assert.Equal(450.0, spots[1].CentreY);
            Assert.Equal(200.0, spots[2].CentreX);
            Assert.Equal(150.0, spots[2].CentreY);
            Assert.Equal(600.0, spots[3].CentreX);
            Assert.Equal(150.0, spots[3].CentreY);
        }

        /// <summary>
        /// Reading order, left to right then top to bottom, so views go on in the order the user
        /// ticked them. Revit counts Y up from the bottom, so the first row is the higher one.
        /// </summary>
        [Fact]
        public void TheyComeBackInReadingOrder()
        {
            var spots = SheetLayout.For(Wide, Tall, 4).ToArray();

            Assert.True(spots[0].CentreX < spots[1].CentreX);
            Assert.True(spots[0].CentreY > spots[2].CentreY);
        }

        /// <summary>
        /// The margin round the outside is the same measurement as the gap down the middle,
        /// which is what an even division means. Held against the sheet edges by hand.
        /// </summary>
        [Fact]
        public void TheMarginIsEvenOnEverySide()
        {
            var spots = SheetLayout.For(Wide, Tall, 4).ToArray();

            double leftEdge = spots[0].CentreX;
            double rightEdge = Wide - spots[1].CentreX;
            double topEdge = Tall - spots[0].CentreY;
            double bottomEdge = spots[2].CentreY;

            Assert.Equal(200.0, leftEdge);
            Assert.Equal(200.0, rightEdge);
            Assert.Equal(150.0, topEdge);
            Assert.Equal(150.0, bottomEdge);
        }

        [Fact]
        public void OnlyOneTwoAndFourAreCounts()
        {
            Assert.Equal(new[] { 1, 2, 4 }, SheetLayout.Counts.ToArray());

            Assert.True(SheetLayout.IsACount(1));
            Assert.True(SheetLayout.IsACount(2));
            Assert.True(SheetLayout.IsACount(4));
            Assert.False(SheetLayout.IsACount(3));
            Assert.False(SheetLayout.IsACount(0));

            Assert.Throws<ArgumentOutOfRangeException>(() => SheetLayout.For(Wide, Tall, 3));
        }

        /// <summary>
        /// A title block reporting no size has nowhere worked out to put a view, so it is
        /// refused at the door rather than placing everything at the origin.
        /// </summary>
        [Fact]
        public void ASheetWithNoSizeIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SheetLayout.For(0.0, Tall, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => SheetLayout.For(Wide, -1.0, 1));
            Assert.Throws<ArgumentException>(() => SheetLayout.For(double.NaN, Tall, 1));
            Assert.Throws<ArgumentException>(() => SheetLayout.For(Wide, double.PositiveInfinity, 1));
        }
    }

    public class SheetDefinitionTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static SheetDefinition Sheet(
            int perSheet = 1, string name = "GENERAL ARRANGEMENT LAYOUT", params ViewType[] views)
        {
            return new SheetDefinition(
                "AR-PRX-Title_Block_A1", "GA-DETAILED DESIGN", name, views, perSheet);
        }

        [Fact]
        public void TheTitleBlockAndTheNameComeBackAsGiven()
        {
            SheetDefinition sheet = Sheet();

            Assert.Equal("AR-PRX-Title_Block_A1", sheet.TitleBlockFamilyName);
            Assert.Equal("GA-DETAILED DESIGN", sheet.TitleBlockTypeName);
            Assert.Equal("AR-PRX-Title_Block_A1 GA-DETAILED DESIGN", sheet.TitleBlock);
            Assert.Equal("GENERAL ARRANGEMENT LAYOUT", sheet.SheetName);
        }

        /// <summary>
        /// A sheet is created with a title block and given a name, so neither can be guessed.
        /// The message names which one is missing rather than saying incomplete.
        /// </summary>
        [Fact]
        public void ASheetMissingATypeOrANameSaysWhichOne()
        {
            Assert.False(new SheetDefinition(string.Empty, string.Empty, "NAME", null, 1).CanBeUsed);
            Assert.Equal(
                "a sheet type",
                new SheetDefinition(string.Empty, string.Empty, "NAME", null, 1).WhatIsMissing);

            Assert.Equal(
                "a sheet name",
                new SheetDefinition("F", "T", "   ", null, 1).WhatIsMissing);

            Assert.Equal(
                "a sheet type and a sheet name",
                new SheetDefinition(string.Empty, string.Empty, string.Empty, null, 1).WhatIsMissing);

            Assert.True(Sheet().CanBeUsed);
            Assert.Equal(string.Empty, Sheet().WhatIsMissing);
        }

        /// <summary>
        /// A sheet with nothing ticked is a real thing to ask for. It makes an empty sheet and
        /// says so before it runs, rather than being refused.
        /// </summary>
        [Fact]
        public void ASheetWithNoViewsIsUsableAndSaysItWillBeEmpty()
        {
            SheetDefinition sheet = Sheet();

            Assert.True(sheet.CanBeUsed);
            Assert.Empty(sheet.Views);
            Assert.Equal(
                "GENERAL ARRANGEMENT LAYOUT on AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, with no "
                + "views ticked, so every sheet it makes will be empty.",
                sheet.InWords());
        }

        [Fact]
        public void TheViewsComeBackInViewTypeOrderWithNoDuplicates()
        {
            SheetDefinition sheet = Sheet(4, "S", Hardscape, General, KeyPlan, General);

            Assert.Equal(
                new[] { "(010) Location Key Plan", "(200) General Arrangement Layout", "(600) HARDSCAPE SCHEDULE" },
                sheet.Views.Select(one => one.ToString()).ToArray());
        }

        /// <summary>
        /// More ticked than fit is not an error. The first few go on and the rest are named, so
        /// nothing is dropped without being said.
        /// </summary>
        [Fact]
        public void MoreViewsThanFitAreLeftOffAndNamed()
        {
            SheetDefinition sheet = Sheet(2, "S", KeyPlan, General, Hardscape);

            Assert.Equal(2, sheet.Placed.Count);
            Assert.Single(sheet.LeftOff);
            Assert.Equal("(600) HARDSCAPE SCHEDULE", sheet.LeftOff[0].ToString());
            Assert.Contains("1 more ticked than fit, left off: (600) HARDSCAPE SCHEDULE.", sheet.InWords());
        }

        [Fact]
        public void ExactlyEnoughLeavesNothingOff()
        {
            SheetDefinition sheet = Sheet(2, "S", KeyPlan, General);

            Assert.Equal(2, sheet.Placed.Count);
            Assert.Empty(sheet.LeftOff);
            Assert.DoesNotContain("left off", sheet.InWords());
        }

        [Fact]
        public void ACountThatIsNotOneTwoOrFourFallsBackToOne()
        {
            Assert.Equal(1, Sheet(3, "S", KeyPlan).ViewsPerSheet);
            Assert.Equal(4, Sheet(4, "S", KeyPlan).ViewsPerSheet);
        }

        [Fact]
        public void TheWordsNameTheSheetTheBlockAndEveryViewOnIt()
        {
            Assert.Equal(
                "GENERAL ARRANGEMENT LAYOUT on AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, "
                + "2 views per sheet: (010) Location Key Plan, (200) General Arrangement Layout.",
                Sheet(2, "GENERAL ARRANGEMENT LAYOUT", KeyPlan, General).InWords());
        }

        [Fact]
        public void SurroundingSpaceIsNotAName()
        {
            Assert.Equal("LIST OF DRAWINGS", Sheet(1, "  LIST OF DRAWINGS  ").SheetName);
        }
    }

    public class SheetOrderTests
    {
        private static SheetDefinition Sheet()
        {
            return new SheetDefinition("F", "T", "LIST OF DRAWINGS", null, 1);
        }

        [Fact]
        public void ANumberIsFoundByItsPlot()
        {
            var order = new SheetOrder(Sheet(), new[]
            {
                new SheetRequest("DM-11", "L-211"),
                new SheetRequest("DM-12", "L-212")
            });

            Assert.Equal("L-211", order.NumberFor("DM-11").SheetNumber);
            Assert.Equal("L-212", order.NumberFor("DM-12").SheetNumber);
            Assert.Null(order.NumberFor("DM-13"));
            Assert.Null(order.NumberFor(null));
        }

        /// <summary>
        /// One number per plot. A second row for the same plot would make two sheets differing
        /// only by a number nobody meant to type twice.
        /// </summary>
        [Fact]
        public void TheSamePlotTwiceKeepsTheFirstNumber()
        {
            var order = new SheetOrder(Sheet(), new[]
            {
                new SheetRequest("DM-11", "L-211"),
                new SheetRequest("DM-11", "L-999")
            });

            Assert.Single(order.Numbers);
            Assert.Equal("L-211", order.NumberFor("DM-11").SheetNumber);
        }

        [Fact]
        public void OnlyPlotsWithANumberAreCountedAsFilledIn()
        {
            var order = new SheetOrder(Sheet(), new[]
            {
                new SheetRequest("DM-11", "L-211"),
                new SheetRequest("DM-12", string.Empty),
                new SheetRequest("DM-13", "   ")
            });

            Assert.Equal(1, order.FilledIn);
        }

        [Fact]
        public void ASheetOrderAlwaysHasADefinition()
        {
            Assert.Throws<ArgumentNullException>(() => new SheetOrder(null, null));
            Assert.Empty(new SheetOrder(Sheet(), null).Numbers);
        }
    }
}
