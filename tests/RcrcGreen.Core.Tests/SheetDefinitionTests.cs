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

        /// <summary>
        /// The whole sheet, so these hold the even division on its own. What the title
        /// strip takes off it is DrawingArea's, and is tested there.
        /// </summary>
        private static readonly DrawingArea Whole = DrawingArea.WholeSheet(Wide, Tall);

        [Fact]
        public void OneViewSitsInTheMiddle()
        {
            var spots = SheetLayout.For(Whole, 1).ToArray();

            Assert.Single(spots);
            Assert.Equal(400.0, spots[0].CentreX);
            Assert.Equal(300.0, spots[0].CentreY);
        }

        /// <summary>
        /// Two ONE ABOVE THE OTHER, the first on top, each with a quarter of the height
        /// above and below it. Side by side is what overlapped on a real sheet: every view
        /// on it was wider than half the drawing area, and a landscape drawing on a
        /// landscape sheet always will be.
        /// </summary>
        [Fact]
        public void TwoSitOneAboveTheOtherAtTheSameWidth()
        {
            var spots = SheetLayout.For(Whole, 2).ToArray();

            Assert.Equal(2, spots.Length);
            Assert.Equal(400.0, spots[0].CentreX);
            Assert.Equal(400.0, spots[1].CentreX);
            Assert.Equal(450.0, spots[0].CentreY);
            Assert.Equal(150.0, spots[1].CentreY);
        }

        [Fact]
        public void FourMakeATwoByTwoGrid()
        {
            var spots = SheetLayout.For(Whole, 4).ToArray();

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
            var spots = SheetLayout.For(Whole, 4).ToArray();

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
            var spots = SheetLayout.For(Whole, 4).ToArray();

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

            Assert.Throws<ArgumentOutOfRangeException>(() => SheetLayout.For(Whole, 3));
        }

        /// <summary>
        /// No area is not an area. It used to take two lengths and refuse them here, and those
        /// refusals moved with them.
        /// </summary>
        [Fact]
        public void NoAreaIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => SheetLayout.For(null, 1));
        }
    }

    public class SheetDefinitionTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static SheetDefinition Sheet(int perSheet = 1, params ViewType[] views)
        {
            return new SheetDefinition(
                "AR-PRX-Title_Block_A1", "GA-DETAILED DESIGN", views, perSheet);
        }

        [Fact]
        public void TheTitleBlockComesBackAsGiven()
        {
            SheetDefinition sheet = Sheet();

            Assert.Equal("AR-PRX-Title_Block_A1", sheet.TitleBlockFamilyName);
            Assert.Equal("GA-DETAILED DESIGN", sheet.TitleBlockTypeName);
            Assert.Equal("AR-PRX-Title_Block_A1 GA-DETAILED DESIGN", sheet.TitleBlock);
        }

        /// <summary>
        /// The title block is the one thing left for a definition to be missing, now that
        /// names and numbers live on the rows.
        /// </summary>
        [Fact]
        public void ASheetMissingItsTypeSaysSo()
        {
            var bare = new SheetDefinition(string.Empty, string.Empty, null, 1);

            Assert.False(bare.CanBeUsed);
            Assert.Equal("a title block", bare.WhatIsMissing);
            Assert.Equal("This sheet is missing a title block, so none is made.", bare.InWords());

            Assert.True(Sheet().CanBeUsed);
            Assert.Equal(string.Empty, Sheet().WhatIsMissing);
        }

        /// <summary>
        /// Ticked order, not sorted order, because that is the order views go onto sheets.
        /// Hardscape was ticked first here and stays first.
        /// </summary>
        [Fact]
        public void TheViewsKeepTheOrderTheyWereTickedInWithNoDuplicates()
        {
            SheetDefinition sheet = Sheet(4, Hardscape, General, KeyPlan, General);

            Assert.Equal(
                new[]
                {
                    "(600) HARDSCAPE SCHEDULE",
                    "(200) General Arrangement Layout",
                    "(010) Location Key Plan"
                },
                sheet.Views.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void ACountThatIsNotOneTwoOrFourFallsBackToOne()
        {
            Assert.Equal(1, Sheet(3, KeyPlan).ViewsPerSheet);
            Assert.Equal(4, Sheet(4, KeyPlan).ViewsPerSheet);
        }

        /// <summary>
        /// The words say how many sheets the ticked views divide into, because that is the
        /// count a person needs before pressing anything. Two views at two per sheet is one
        /// sheet, three at one per sheet is three.
        /// </summary>
        [Fact]
        public void TheWordsSayTheBlockTheDivisionAndEveryView()
        {
            Assert.Equal(
                "On AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, 2 views per sheet, 1 sheet per "
                + "ticked plot: (010) Location Key Plan, (200) General Arrangement Layout.",
                Sheet(2, KeyPlan, General).InWords());

            Assert.Equal(
                "On AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, 1 view per sheet, 3 sheets per "
                + "ticked plot: (010) Location Key Plan, (200) General Arrangement Layout, "
                + "(600) HARDSCAPE SCHEDULE.",
                Sheet(1, KeyPlan, General, Hardscape).InWords());
        }

        /// <summary>
        /// No views plans ONE empty sheet, which is the title sheet's shape: 010001A and
        /// 010QE both read 0 views on the measured models. No views planned no rows for two
        /// rounds, so the one sheet the team starts a set with could not be described at
        /// all. The row is refused until its typed name and number arrive, so a definition
        /// somebody has only half filled in still makes nothing.
        /// </summary>
        [Fact]
        public void ADefinitionWithNoViewsPlansOneEmptySheet()
        {
            SheetDefinition sheet = Sheet();

            Assert.True(sheet.CanBeUsed);

            PlannedSheet empty = Assert.Single(sheet.Planned);
            Assert.Empty(empty.Views);
            Assert.False(empty.NamedFromItsView);
            Assert.Equal(string.Empty, empty.ProposedName);
            Assert.Equal(string.Empty, empty.SingleCode);
            Assert.Equal(
                "Holds no views, the way a title sheet does, so the name is typed rather "
                + "than proposed.",
                empty.WhyNothingIsProposed());

            Assert.Equal(
                "On AR-PRX-Title_Block_A1 GA-DETAILED DESIGN, no views, 1 empty sheet per "
                + "ticked plot, the way a title sheet is. Type its name and its number.",
                sheet.InWords());
        }
    }

    /// <summary>
    /// The name a sheet gets from the one view it holds. Real sheets read 200Q GENERAL
    /// ARRANGEMENT LAYOUT, so the expected strings below are written from that pattern by hand.
    /// </summary>
    public class SheetNamingTests
    {
        [Fact]
        public void TheViewNameIsUpperCasedAndTheCodeNeverAppears()
        {
            Assert.Equal(
                "GENERAL ARRANGEMENT LAYOUT",
                SheetNaming.FromView(new ViewType("200", "General Arrangement Layout")));

            Assert.Equal(
                "LOCATION KEY PLAN",
                SheetNaming.FromView(new ViewType("010", "Location Key Plan")));

            Assert.Equal(
                "LANDSCAPE CROSS SECTION",
                SheetNaming.FromView(new ViewType("400", "Landscape Cross Section")));
        }

        [Fact]
        public void ANameAlreadyInCapitalsComesThroughUntouched()
        {
            Assert.Equal(
                "HARDSCAPE SCHEDULE",
                SheetNaming.FromView(new ViewType("600", "HARDSCAPE SCHEDULE")));
        }

        [Fact]
        public void ThereIsNoNameWithoutAView()
        {
            Assert.Throws<ArgumentNullException>(() => SheetNaming.FromView(null));
        }
    }

    /// <summary>
    /// How the ticked views divide into sheets. Every expected grouping is written out by
    /// hand, because the division is the rule under test.
    /// </summary>
    public class SheetDivisionTests
    {
        private static readonly ViewType[] Six =
        {
            new ViewType("010", "Location Key Plan"),
            new ViewType("010", "Overall Key Plan"),
            new ViewType("200", "General Arrangement Layout"),
            new ViewType("400", "Landscape Cross Section"),
            new ViewType("600", "HARDSCAPE SCHEDULE"),
            new ViewType("600", "SHRUBS AND LAWN SCHEDULE")
        };

        private static string[][] Grouped(int perSheet, params ViewType[] views)
        {
            return SheetDivision.Of(views, perSheet)
                .Select(sheet => sheet.Views.Select(one => one.ToString()).ToArray())
                .ToArray();
        }

        [Fact]
        public void SixViewsAtOnePerSheetMakeSixSheets()
        {
            string[][] sheets = Grouped(1, Six);

            Assert.Equal(6, sheets.Length);
            Assert.All(sheets, one => Assert.Single(one));
            Assert.Equal("(010) Location Key Plan", sheets[0][0]);
            Assert.Equal("(600) SHRUBS AND LAWN SCHEDULE", sheets[5][0]);
        }

        [Fact]
        public void SixAtTwoPerSheetMakeThree()
        {
            string[][] sheets = Grouped(2, Six);

            Assert.Equal(3, sheets.Length);
            Assert.Equal(
                new[] { "(010) Location Key Plan", "(010) Overall Key Plan" }, sheets[0]);
            Assert.Equal(
                new[] { "(200) General Arrangement Layout", "(400) Landscape Cross Section" },
                sheets[1]);
            Assert.Equal(
                new[] { "(600) HARDSCAPE SCHEDULE", "(600) SHRUBS AND LAWN SCHEDULE" },
                sheets[2]);
        }

        /// <summary>
        /// The last sheet holds what is left, two here, and they sit in the first cells of the
        /// same four-view grid.
        /// </summary>
        [Fact]
        public void SixAtFourPerSheetMakeTwoWithTheSecondHoldingTwo()
        {
            string[][] sheets = Grouped(4, Six);

            Assert.Equal(2, sheets.Length);
            Assert.Equal(4, sheets[0].Length);
            Assert.Equal(
                new[] { "(600) HARDSCAPE SCHEDULE", "(600) SHRUBS AND LAWN SCHEDULE" },
                sheets[1]);
        }

        /// <summary>
        /// The whole point of the division. Flattening the sheets back gives every ticked view
        /// exactly once, whatever the count per sheet. The six here are ticked in the team's
        /// own order, so the sorted sheets read back in the ticked order too. Ticks out of
        /// that order are reordered, which SheetOrderTests holds.
        /// </summary>
        [Fact]
        public void NoViewIsEverLeftOffAndTheOrderIsTheTickedOrder()
        {
            string[] ticked = Six.Select(one => one.ToString()).ToArray();

            Assert.Equal(ticked, Grouped(1, Six).SelectMany(one => one).ToArray());
            Assert.Equal(ticked, Grouped(2, Six).SelectMany(one => one).ToArray());
            Assert.Equal(ticked, Grouped(4, Six).SelectMany(one => one).ToArray());
        }

        /// <summary>
        /// No views plans one empty sheet, the title sheet's shape, rather than none. The
        /// row it makes is refused until its typed name and number arrive, so nothing is
        /// made by accident.
        /// </summary>
        [Fact]
        public void NoViewsPlanOneEmptySheet()
        {
            PlannedSheet ofNull = Assert.Single(SheetDivision.Of(null, 1));
            Assert.Empty(ofNull.Views);

            PlannedSheet ofNone = Assert.Single(SheetDivision.Of(new ViewType[0], 2));
            Assert.Empty(ofNone.Views);
        }

        [Fact]
        public void ACountThatIsNotOneTwoOrFourFallsBackToOnePerSheet()
        {
            Assert.Equal(3, SheetDivision.Of(Six.Take(3), 3).Count);
        }

        /// <summary>
        /// A sheet holding one view is named after it, upper cased with the code gone. One
        /// holding more is not, and the words say why its boxes start empty.
        /// </summary>
        [Fact]
        public void OnlyASheetHoldingOneViewProposesAName()
        {
            PlannedSheet alone = SheetDivision.Of(Six.Take(1), 1)[0];
            PlannedSheet pair = SheetDivision.Of(Six.Take(2), 2)[0];

            Assert.True(alone.NamedFromItsView);
            Assert.Equal("LOCATION KEY PLAN", alone.ProposedName);
            Assert.Equal(string.Empty, alone.WhyNothingIsProposed());

            Assert.False(pair.NamedFromItsView);
            Assert.Equal(string.Empty, pair.ProposedName);
            Assert.Equal(
                "Holds 2 views, so the name is typed rather than proposed.",
                pair.WhyNothingIsProposed());
        }

        /// <summary>
        /// The signature is what a typed name or number is filed under, so it has to name the
        /// views and their order and nothing else.
        /// </summary>
        [Fact]
        public void TheSignatureIsTheViewsInOrder()
        {
            PlannedSheet pair = SheetDivision.Of(Six.Take(2), 2)[0];

            Assert.Equal("(010) Location Key Plan|(010) Overall Key Plan", pair.Signature);
            Assert.Equal("(010) Location Key Plan, (010) Overall Key Plan", pair.ViewsInWords());
        }
    }

    /// <summary>
    /// One row of the step 4 table: one sheet, one plot, one name, one number.
    /// </summary>
    public class SheetToMakeTests
    {
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");

        [Fact]
        public void ARowWithEverythingCanBeMade()
        {
            SheetToMake row = RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN",
                new[] { KeyPlan });

            Assert.True(row.CanBeMade);
            Assert.Equal(string.Empty, row.WhatIsMissing);
            Assert.Equal("010QA LOCATION KEY PLAN", row.ToString());
        }

        [Fact]
        public void ARowSaysWhichOfTheTwoBlanksIsStillEmpty()
        {
            Assert.Equal(
                "a name and a number",
                RunFixture.Row("DM-11", "", "").WhatIsMissing);
            Assert.Equal("a name", RunFixture.Row("DM-11", "010QA", "").WhatIsMissing);
            Assert.Equal("a number", RunFixture.Row("DM-11", "", "LAYOUT").WhatIsMissing);

            Assert.False(RunFixture.Row("DM-11", "010QA", "").CanBeMade);
        }

        [Fact]
        public void SurroundingSpaceIsNeitherANameNorANumber()
        {
            SheetToMake row = RunFixture.Row("DM-11", "  010QA  ", "  LAYOUT  ");

            Assert.Equal("010QA", row.SheetNumber);
            Assert.Equal("LAYOUT", row.SheetName);
            Assert.False(RunFixture.Row("DM-11", "   ", "LAYOUT").HasNumber);
        }

        /// <summary>
        /// Generated only means anything while there is a value. A cleared box is typed
        /// nothing, not generated nothing.
        /// </summary>
        [Fact]
        public void AnEmptyValueIsNeverCalledGenerated()
        {
            var row = new SheetToMake(
                "DM-11", string.Empty, string.Empty, null, 1,
                RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, true, true);

            Assert.False(row.NameWasGenerated);
            Assert.False(row.NumberWasGenerated);
        }

        /// <summary>
        /// The report says which of the two the user typed and which the tool proposed, word
        /// for word, so a generated value is checkable rather than mistaken for a choice.
        /// </summary>
        [Fact]
        public void TheProvenanceIsSaidInWords()
        {
            var generated = new SheetToMake(
                "DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }, 1,
                RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, true, true);

            Assert.Equal(
                "The name was built from its view and the number was built from the view code "
                + "and the plot identifier.",
                generated.ProvenanceInWords());

            Assert.Equal(
                "The name was typed and the number was typed.",
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN").ProvenanceInWords());
        }

        [Fact]
        public void TheViewsKeepTheOrderTheyWereGivenIn()
        {
            SheetToMake row = RunFixture.Row("DM-11", "010QA", "N", new[]
            {
                new ViewType("200", "General Arrangement Layout"),
                KeyPlan
            });

            Assert.Equal(
                "(200) General Arrangement Layout, (010) Location Key Plan",
                row.ViewsInWords());
        }

        [Fact]
        public void ARowWithNoViewsSaysSo()
        {
            Assert.Equal("no views", RunFixture.Row("DM-11", "010QA", "N").ViewsInWords());
        }
    }

    /// <summary>
    /// Everything one described sheet makes across the ticked plots.
    /// </summary>
    public class SheetBatchTests
    {
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");

        [Fact]
        public void TheHeaderCountsWhatThisDefinitionMakes()
        {
            SheetBatch batch = RunFixture.Batch(new[] { KeyPlan }, 1,
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                RunFixture.Row("DM-12", "010RA", "LOCATION KEY PLAN", new[] { KeyPlan }));

            Assert.Equal(2, batch.WillBeMade);
            Assert.Equal(0, batch.StillMissingSomething);
            Assert.Equal("Makes 2 sheets across the ticked plots.", batch.InWords());
        }

        [Fact]
        public void ARowStillShortOfSomethingIsCountedAndSaid()
        {
            SheetBatch batch = RunFixture.Batch(new[] { KeyPlan }, 1,
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                RunFixture.Row("DM-12", "", "LOCATION KEY PLAN", new[] { KeyPlan }));

            Assert.Equal(1, batch.WillBeMade);
            Assert.Equal(1, batch.StillMissingSomething);
            Assert.Equal(
                "Makes 1 sheet across the ticked plots. 1 row still needs a name or a number.",
                batch.InWords());
        }

        [Fact]
        public void AnUnusableDefinitionMakesNothingWhateverItsRowsSay()
        {
            SheetBatch batch = RunFixture.BatchMissingItsType(
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }));

            Assert.Equal(0, batch.WillBeMade);
            Assert.Equal("Missing a title block, so it makes nothing.", batch.InWords());
        }

        /// <summary>
        /// The count the Run step names when nothing is asked for. A row short of a name or a
        /// number counts whatever the title block says, and a finished row never does.
        /// </summary>
        [Fact]
        public void RowsShortOfANameOrANumberAreCountedWhateverTheTitleBlockSays()
        {
            SheetBatch batch = RunFixture.Batch(new[] { KeyPlan }, 1,
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                RunFixture.Row("DM-12", "", "LOCATION KEY PLAN", new[] { KeyPlan }),
                RunFixture.Row("DM-13", "010RA", "", new[] { KeyPlan }),
                RunFixture.Row("DM-14", "", "", new[] { KeyPlan }));

            Assert.Equal(3, batch.RowsShortOfANameOrANumber);

            SheetBatch noTitleBlock = RunFixture.BatchMissingItsType(
                RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                RunFixture.Row("DM-12", "", "LOCATION KEY PLAN", new[] { KeyPlan }));

            Assert.Equal(1, noTitleBlock.RowsShortOfANameOrANumber);
        }

        [Fact]
        public void NoRowsMeansNothingToMakeAndTheWordsSayWhatToDo()
        {
            SheetBatch batch = RunFixture.Batch(null, 1);

            Assert.Equal(0, batch.WillBeMade);
            Assert.Equal(
                "Makes no sheets. Tick a view for it, and tick a plot in step 1.",
                batch.InWords());
        }

        [Fact]
        public void ABatchAlwaysHasADefinition()
        {
            Assert.Throws<ArgumentNullException>(() => new SheetBatch(null, null));
            Assert.Empty(RunFixture.Batch(null, 1).Rows);
        }

        private static SheetToMake Proposed(string plotId, string number, string name)
        {
            return new SheetToMake(
                plotId, number, name, new[] { KeyPlan }, 1,
                RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, true, true);
        }

        /// <summary>
        /// Remove asks only when a row carries something typed. A proposal comes back the
        /// moment the sheet is described again and typed text does not, and Remove sits next
        /// to the sheet heading where a slip costs every row's typing.
        /// </summary>
        [Fact]
        public void RemoveAsksOnlyWhenARowCarriesTypedText()
        {
            SheetBatch proposed = RunFixture.Batch(new[] { KeyPlan }, 1,
                Proposed("DM-11", "010QA", "LOCATION KEY PLAN"),
                Proposed("DM-12", "010RA", "LOCATION KEY PLAN"));

            Assert.Equal(0, proposed.RowsWithTypedText);
            Assert.Equal(string.Empty, proposed.WhyRemovalAsks());

            SheetBatch oneTyped = RunFixture.Batch(new[] { KeyPlan }, 1,
                Proposed("DM-11", "010QA", "LOCATION KEY PLAN"),
                RunFixture.Row("DM-12", "010RA", "LOCATION KEY PLAN", new[] { KeyPlan }));

            Assert.Equal(1, oneTyped.RowsWithTypedText);
            Assert.Equal(
                "Removing this sheet loses the names or numbers typed on 1 row. Remove it anyway?",
                oneTyped.WhyRemovalAsks());
        }

        /// <summary>
        /// A row typed on either box counts once, and an empty box is not typed text. Two rows
        /// each with one typed box read as two rows.
        /// </summary>
        [Fact]
        public void ARowCountsOnceWhicheverBoxWasTypedAndAnEmptyBoxDoesNotCount()
        {
            SheetBatch batch = RunFixture.Batch(new[] { KeyPlan }, 1,
                new SheetToMake("DM-11", "010QA", "LIST OF DRAWINGS", new[] { KeyPlan }, 1,
                    RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, false, true),
                new SheetToMake("DM-12", "010RB", "LOCATION KEY PLAN", new[] { KeyPlan }, 1,
                    RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, true, false),
                RunFixture.Row("DM-13", "", "", new[] { KeyPlan }));

            Assert.Equal(2, batch.RowsWithTypedText);
            Assert.Equal(
                "Removing this sheet loses the names or numbers typed on 2 rows. Remove it anyway?",
                batch.WhyRemovalAsks());
        }
    }
}
