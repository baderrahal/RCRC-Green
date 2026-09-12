using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The run drawn before it is made, one card per sheet.
    ///
    /// Every size below is written out by hand. The title block is 4 by 3 feet, so the drawing
    /// area is the sheet less the fifth the title strip takes: 3.2 by 3. At one view per sheet
    /// the cell is that whole area and its centre is 1.6, 1.5.
    /// </summary>
    public class RunPreviewTests
    {
        private const string Family = "AR-PRX-Title_Block_A1";
        private const string Block = "GA-DETAILED DESIGN";

        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static TitleBlockSizes Sizes(double wide = 4.0, double tall = 3.0)
        {
            return TitleBlockSizes.Of(new[]
            {
                new MeasuredTitleBlock(Family, Block, wide, tall, "010QE")
            });
        }

        private static RunPlan PlanOf(int perSheet, params ViewType[] views)
        {
            return RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Batch(
                    views,
                    perSheet,
                    RunFixture.Row("DM-11", "010DM11A", "LIST OF DRAWINGS", views, perSheet)));
        }

        [Fact]
        public void OneSheetGivesOneCardCarryingItsNumberAndName()
        {
            var cards = RunPreview.Of(PlanOf(1, Overall), Sizes(), PaperSizes.Nothing);

            PreviewedSheet only = Assert.Single(cards);

            Assert.Equal("DM-11", only.PlotId);
            Assert.Equal("010DM11A", only.SheetNumber);
            Assert.Equal("LIST OF DRAWINGS", only.SheetName);
            Assert.True(only.CanBeDrawn);
            Assert.Equal(1, only.OfHowMany);
            Assert.Equal(1, only.Which);
        }

        [Fact]
        public void TheOutlineIsTheTitleBlockAndTheAreaIsItLessTheStrip()
        {
            var cards = RunPreview.Of(PlanOf(1, Overall), Sizes(), PaperSizes.Nothing);

            Assert.Equal(4.0, cards[0].TitleBlock.WidthFeet);
            Assert.Equal(3.0, cards[0].TitleBlock.HeightFeet);
            Assert.Equal(3.2, cards[0].Placed.Area.WidthFeet, 6);
            Assert.Equal(3.0, cards[0].Placed.Area.HeightFeet, 6);
            Assert.True(cards[0].Placed.Area.StripTakenOff);
        }

        [Fact]
        public void AViewLandsWhereTheRunWouldPlaceItAndNowhereElse()
        {
            var cards = RunPreview.Of(PlanOf(1, Overall), Sizes(), PaperSizes.Nothing);

            DrawingArea area = DrawingArea.InsideTheTitleBlock(4.0, 3.0);
            IReadOnlyList<ViewportSpot> spots = SheetLayout.For(area, 1);

            Assert.Equal(spots[0].ToString(), cards[0].Placed.Views[0].Spot.ToString());
            Assert.Equal(1.6, cards[0].Placed.Views[0].Spot.CentreX, 6);
            Assert.Equal(1.5, cards[0].Placed.Views[0].Spot.CentreY, 6);
        }

        [Fact]
        public void AViewThisModelHoldsIsDrawnAtItsMeasuredSize()
        {
            PaperSizes measured = PaperSizes.Of(new[]
            {
                new ViewOnPaper("DM-11-(010) Overall Plan", 2.0, 1.5)
            });

            var cards = RunPreview.Of(PlanOf(1, Overall), Sizes(), measured);

            ViewOnPaper drawn = cards[0].Placed.Views[0].View;
            Assert.True(drawn.Measured);
            Assert.Equal(2.0, drawn.WidthFeet);
            Assert.False(cards[0].AnythingNotMeasured);
        }

        [Fact]
        public void AViewTheRunHasYetToMakeIsLaidOutAndMarkedNotMeasured()
        {
            var cards = RunPreview.Of(PlanOf(1, Hardscape), Sizes(), PaperSizes.Nothing);

            Assert.True(cards[0].AnythingNotMeasured);
            Assert.False(cards[0].Placed.Views[0].View.Measured);
            Assert.Contains(
                "1 view is drawn at a nominal size rather than measured: "
                    + "DM-11-(600) HARDSCAPE SCHEDULE.",
                cards[0].InWords());
        }

        [Fact]
        public void ASheetOnATitleBlockNoSheetUsesYetIsNotDrawnAndSaysWhy()
        {
            var cards = RunPreview.Of(PlanOf(1, Overall), TitleBlockSizes.Nothing, null);

            PreviewedSheet only = Assert.Single(cards);

            Assert.False(only.CanBeDrawn);
            Assert.Null(only.TitleBlock);
            Assert.Null(only.Placed);
            Assert.Equal(
                "No sheet in this model uses this title block yet, so its size is not known "
                    + "and nothing is drawn to scale.",
                only.InWords());
        }

        [Fact]
        public void ATitleBlockThatGaveNoSizeIsNotDrawnEither()
        {
            var cards = RunPreview.Of(
                PlanOf(1, Overall),
                TitleBlockSizes.Of(new[]
                {
                    new MeasuredTitleBlock(Family, Block, 0.0, 0.0, "010QE")
                }),
                null);

            Assert.False(cards[0].CanBeDrawn);
            Assert.Equal(
                "AR-PRX-Title_Block_A1 GA-DETAILED DESIGN gave no width or height, so nothing "
                    + "can be drawn at its size.",
                cards[0].InWords());
        }

        [Fact]
        public void OneRowWhoseViewsDoNotAllFitGivesACardPerSheetAndMarksTheCarriedOne()
        {
            // Three views at one per sheet is three sheets, and the second and third exist
            // only because the ones before them filled.
            var cards = RunPreview.Of(
                PlanOf(1, Overall, General, Hardscape), Sizes(), PaperSizes.Nothing);

            Assert.Equal(3, cards.Count);
            Assert.All(cards, one => Assert.Equal(3, one.OfHowMany));
            Assert.Equal(new[] { 1, 2, 3 }, cards.Select(one => one.Which).ToArray());

            Assert.False(cards[0].CarriesWhatDidNotFit);
            Assert.True(cards[1].CarriesWhatDidNotFit);
            Assert.True(cards[2].CarriesWhatDidNotFit);

            Assert.Contains(
                "This one carries what did not fit on the sheet before it.", cards[1].InWords());
        }

        [Fact]
        public void ASheetWithNoViewsIsDrawnEmptyAndSaysSo()
        {
            var cards = RunPreview.Of(PlanOf(1), Sizes(), PaperSizes.Nothing);

            PreviewedSheet only = Assert.Single(cards);

            Assert.True(only.HasNoViews);
            Assert.True(only.CanBeDrawn);
            Assert.Empty(only.Placed.Views);
            Assert.Contains("No views, the way a title sheet is.", only.InWords());
        }

        [Fact]
        public void TheCardSaysWhichSheetTheSizeWasMeasuredOff()
        {
            var cards = RunPreview.Of(PlanOf(1, Overall), Sizes(), PaperSizes.Nothing);

            Assert.Contains(
                "1219.2 by 914.4 mm, measured off sheet 010QE.", cards[0].InWords());
        }

        [Fact]
        public void ASizeWithNoSheetNamedStillSaysItCameOffThisModel()
        {
            var block = new MeasuredTitleBlock(Family, Block, 4.0, 3.0, "  ");

            Assert.Equal("1219.2 by 914.4 mm, off a sheet in this model.", block.InWords());
        }

        [Fact]
        public void OnlySheetsAreDrawnAndAViewOnItsOwnIsNotACard()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", Overall) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null);

            Assert.Equal(1, plan.CountOf(RunItemKind.PlanView));
            Assert.Empty(RunPreview.Of(plan, Sizes(), PaperSizes.Nothing));
        }

        [Fact]
        public void TheFirstMeasurementOfATypeWinsAndAnotherTypeIsItsOwn()
        {
            TitleBlockSizes sizes = TitleBlockSizes.Of(new[]
            {
                new MeasuredTitleBlock(Family, Block, 4.0, 3.0, "010QE"),
                new MeasuredTitleBlock(Family, Block, 9.0, 9.0, "200Q"),
                new MeasuredTitleBlock(Family, "COVER PAGE", 2.0, 1.0, "010QA")
            });

            Assert.Equal(2, sizes.Count);
            Assert.Equal(4.0, sizes.For(Family, Block).WidthFeet);
            Assert.Equal("010QE", sizes.For(Family, Block).OffSheetNumber);
            Assert.Equal(2.0, sizes.For(Family, "COVER PAGE").WidthFeet);
            Assert.Null(sizes.For("another family", Block));
        }

        [Fact]
        public void OnlyAMeasuredViewGoesIntoThePaperSizesAndTheRestComeBackUnmeasured()
        {
            PaperSizes sizes = PaperSizes.Of(new[]
            {
                new ViewOnPaper("one", 2.0, 1.0),
                ViewOnPaper.NotMeasured("two"),
                new ViewOnPaper("one", 9.0, 9.0)
            });

            Assert.Equal(1, sizes.Count);
            Assert.Equal(2.0, sizes.For("one").WidthFeet);
            Assert.False(sizes.For("two").Measured);
            Assert.Equal("two", sizes.For("two").ViewName);
            Assert.False(sizes.For(null).Measured);
        }

        [Fact]
        public void TheLineAboveTheCardsCountsThemAndWhatIsWrongWithThem()
        {
            Assert.Equal(
                "No sheet is described yet, so there is nothing to draw.",
                RunPreview.InWords(null));

            Assert.Equal(
                "1 sheet, drawn at the size its title block comes out.",
                RunPreview.InWords(
                    RunPreview.Of(PlanOf(1, Overall), Sizes(), PaperSizes.Nothing)));

            Assert.Equal(
                "3 sheets, drawn at the size their title blocks come out. 2 of them carry what "
                    + "did not fit on the sheet before them.",
                RunPreview.InWords(RunPreview.Of(
                    PlanOf(1, Overall, General, Hardscape), Sizes(), PaperSizes.Nothing)));

            Assert.Equal(
                "1 sheet, drawn at the size its title block comes out. 1 cannot be drawn, "
                    + "because no sheet in this model uses its title block yet.",
                RunPreview.InWords(
                    RunPreview.Of(PlanOf(1, Overall), TitleBlockSizes.Nothing, null)));
        }

        [Fact]
        public void APlanIsAlwaysNeeded()
        {
            Assert.Throws<ArgumentNullException>(() => RunPreview.Of(null, Sizes(), null));
        }

        [Fact]
        public void AViewIsNamedThePlotThenTheTypeAndOneBuilderSaysSo()
        {
            Assert.Equal("DM-11-(010) Overall Plan", ViewNaming.Of("DM-11", Overall));
            Assert.Equal("DM-11", ViewNaming.Of("DM-11", null));
            Assert.Equal("-(010) Overall Plan", ViewNaming.Of(null, Overall));
        }

        [Fact]
        public void TheNameTheRunPlanPrintsIsTheNameTheSizeIsLookedUpUnder()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", Overall) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null);

            Assert.Equal(
                ViewNaming.Of("DM-11", Overall),
                plan.Items.Single(one => one.Kind == RunItemKind.PlanView).Name);
        }
    }
}
