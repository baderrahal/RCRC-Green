using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiDataTests
    {
        [Fact]
        public void ATallyCannotHaveMoreValuesThanCarriers()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterTally("PRX_COMPONENT", 2, 3));
        }

        [Fact]
        public void ATallyRefusesNegativeCountsAndANullName()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterTally("PRX_COMPONENT", -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterTally("PRX_COMPONENT", 0, -1));
            Assert.Throws<ArgumentNullException>(() => new ParameterTally(null, 0, 0));
        }

        [Fact]
        public void ANameCountRefusesANullNameAndANegativeCount()
        {
            Assert.Throws<ArgumentNullException>(() => new NameCount(null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new NameCount("Existing", -1));
        }

        [Fact]
        public void AMeasuredAreaRefusesANumberThatIsNotANumber()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new MeasuredArea("DM-11-(600) HARDSCAPE SCHEDULE", "Area", "Floors 1", double.NaN, "?"));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new MeasuredArea("DM-11-(600) HARDSCAPE SCHEDULE", "Area", "Floors 1", double.PositiveInfinity, "?"));
            Assert.Throws<ArgumentNullException>(
                () => new MeasuredArea(null, "Area", "Floors 1", 1.0, "?"));
            Assert.Throws<ArgumentNullException>(
                () => new MeasuredArea("DM-11-(600) HARDSCAPE SCHEDULE", null, "Floors 1", 1.0, "?"));
        }

        [Fact]
        public void AMeasuredAreaWorksItsSquareMetresOutFromTheRawNumber()
        {
            var area = new MeasuredArea("DM-11-(600) HARDSCAPE SCHEDULE", "Area", "Floors 1", 1000.0, "92.90 m²");

            Assert.Equal(92.90304, area.SquareMetres, 5);
            Assert.Equal("92.90 m²", area.Printed);
        }

        [Fact]
        public void TheOtherConstructorsRefuseNegativeCountsAndNullNames()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentFacts("NG05", "", -1, 0, 0.0, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentFacts("NG05", "", 0, -1, 0.0, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentFacts("NG05", "", 0, 0, -0.5, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentFacts("NG05", "", 0, 0, double.NaN, null, null));
            Assert.Throws<ArgumentNullException>(() => new DocumentFacts(null, "", 0, 0, 0.0, null, null));
            Assert.Throws<ArgumentNullException>(() => new ProjectUnit(null, "", 0.01));
            Assert.Throws<ArgumentNullException>(() => new KpiScan(null, null, null, null, null, null, true));
            Assert.Throws<ArgumentNullException>(() => new ParameterHome(null, 0, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterHome("sheet", -1, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterValueCount("Phase Created", "Existing", -1, false));
            Assert.Throws<ArgumentNullException>(() => new ParameterValueCount(null, "Existing", 1, false));
            Assert.Throws<ArgumentNullException>(() => new ReadParameter(null, "shared", "String", "", false, "", ""));
            Assert.Throws<ArgumentNullException>(() => new SheetValue(null, "001", "A", "DM-41"));
            Assert.Throws<ArgumentNullException>(() => new TitleBlockCount(null, "A1", 1));
            Assert.Throws<ArgumentNullException>(() => new TitleBlockCount("AR-PRX-Title_Block_A1", null, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TitleBlockCount("AR-PRX-Title_Block_A1", "A1", -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TitleBlockFacts(-1, 0, 0, null, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TitleBlockFacts(0, 0, -1, null, null, null, null));
            Assert.Throws<ArgumentNullException>(() => new ScannedLinkType(null, "Loaded", true, false));
            Assert.Throws<ArgumentNullException>(() => new ScannedLinkInstance(null, "type", true));
            Assert.Throws<ArgumentNullException>(() => new LinkContents(null, "", 0, null, null, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LinkContents("link", "", -1, null, null, null, null, null));
            Assert.Throws<ArgumentNullException>(
                () => new ScannedSchedule(null, "Planting", false, null, null, "", "", false, false, 0, null));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ScannedSchedule("Sheet List", "Sheets", false, null, null, "", "", false, false, -1, null));
            Assert.Throws<ArgumentNullException>(
                () => new ScheduleElements(null, 0, null, null, null, null, null, null, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new ScheduleElements("Sheet List", -1, null, null, null, null, null, null, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ScheduleFacts(null, -1, null, null, null));
        }

        [Fact]
        public void APlaceholderCountAboveTheSheetCountIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TitleBlockFacts(2, 3, 0, null, null, null, null));
        }

        [Fact]
        public void AnAnswerNumberOutsideOneToNineIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new KpiAnswer(0, "q", "a", false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new KpiAnswer(10, "q", "a", false));
            Assert.Throws<ArgumentNullException>(() => new KpiAnswer(1, null, "a", false));
            Assert.Throws<ArgumentNullException>(() => new KpiAnswer(1, "q", null, false));
        }

        [Fact]
        public void NullListsBecomeEmptyListsAndNullPartsBecomeEmptyParts()
        {
            var scan = new KpiScan(
                new DocumentFacts("NG05", null, 0, 0, 0.0, null, null),
                null, null, null, null, null, true);

            Assert.Empty(scan.ProjectInformation);
            Assert.Empty(scan.Skipped);
            Assert.Equal(string.Empty, scan.Document.Path);
            Assert.Equal("UNKNOWN", scan.Document.Area.Label);
            Assert.False(scan.Document.Length.IsKnown);
            Assert.Equal(0, scan.TitleBlocks.SheetCount);
            Assert.Empty(scan.TitleBlocks.TitleBlocks);
            Assert.Empty(scan.Links.Types);
            Assert.Empty(scan.Links.Instances);
            Assert.Empty(scan.Links.Contents);
            Assert.Empty(scan.Schedules.Schedules);
            Assert.Empty(scan.Schedules.Phases);
            Assert.Empty(scan.Schedules.Elements);
            Assert.Empty(scan.Schedules.Areas);
        }

        [Fact]
        public void EveryListHolderTakesNullForItsLists()
        {
            var home = new ParameterHome("sheet", 0, null, null);
            var contents = new LinkContents("link", null, 0, null, null, null, null, null);
            var schedule = new ScannedSchedule("Sheet List", null, false, null, null, null, null, false, false, 0, null);
            var elements = new ScheduleElements("Sheet List", 0, null, null, null, null, null, null, null, null, null);
            var region = new FilledRegionRead(null, null, null);

            Assert.Empty(home.Tallies);
            Assert.Empty(home.Values);
            Assert.Empty(contents.TypeCounts);
            Assert.Empty(contents.InterventionAreas);
            Assert.Equal(string.Empty, contents.DocumentTitle);
            Assert.Empty(schedule.Fields);
            Assert.Empty(schedule.Rows);
            Assert.Equal(string.Empty, schedule.CategoryName);
            Assert.Empty(elements.WordValues);
            Assert.Empty(elements.CreatedPhases);
            Assert.Empty(region.Parameters);
            Assert.Equal(string.Empty, region.TypeName);
        }

        [Fact]
        public void ANullEntryInsideAListIsDroppedRatherThanKept()
        {
            var home = new ParameterHome("sheet", 1, new[] { KpiFixture.Tally("PRX_COMPONENT", 1, 1), null }, null);
            var scan = KpiFixture.Build(skipped: new[] { "Links: refused", null, string.Empty });

            Assert.Single(home.Tallies);
            Assert.Equal(new[] { "Links: refused" }, scan.Skipped);
        }

        [Fact]
        public void TheThreeHomesOfAnEmptyTitleBlockFactsAreStillNamed()
        {
            TitleBlockFacts facts = TitleBlockFacts.Nothing();

            Assert.Equal(
                new[] { "title block instance", "title block type", "sheet" },
                facts.Homes.Select(home => home.Where));
        }

        [Fact]
        public void NamesHoldingIgnoresCaseAndComesBackSorted()
        {
            ParameterHome home = KpiFixture.OnInstances(4, new[]
            {
                KpiFixture.Tally("PRX_Plot_ID", 4, 4),
                KpiFixture.Tally("prx_component", 4, 4),
                KpiFixture.Tally("Sheet Width", 4, 4),
                KpiFixture.Tally("PRX_PLOT_UID", 4, 4)
            });

            Assert.Equal(
                new[] { "PRX_PLOT_UID", "PRX_Plot_ID", "prx_component" },
                home.NamesHolding("COMPONENT", "PLOT", "UID"));
        }

        [Fact]
        public void HoldsAndTallyForCompareTheExactName()
        {
            ParameterHome home = KpiFixture.OnInstances(4, new[] { KpiFixture.Tally("PRX_COMPONENT", 4, 2) });

            Assert.True(home.Holds("PRX_COMPONENT"));
            Assert.False(home.Holds("PRX_Component"));
            Assert.Equal(2, home.TallyFor("PRX_COMPONENT").WithValue);
            Assert.Null(home.TallyFor("PRX_Plot_UID2"));
        }

        [Fact]
        public void ValuesOfPicksOnlyTheNamedParameter()
        {
            ParameterHome home = KpiFixture.OnSheets(2, null, new[]
            {
                KpiFixture.Value("PRX_COMPONENT", "001", "A", "SOFTSCAPE"),
                KpiFixture.Value("PRX_Plot_UID2", "001", "A", "DM-41"),
                KpiFixture.Value("PRX_COMPONENT", "002", "B", "")
            });

            Assert.Equal(new[] { "SOFTSCAPE", "" }, home.ValuesOf("PRX_COMPONENT").Select(value => value.Value));
        }

        [Fact]
        public void NameWithoutThePlotStripsThePlotAndKeepsTheCodeAndTheName()
        {
            Assert.Equal("(610) SOFTSCAPE SCHEDULE", KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE").NameWithoutThePlot);
            Assert.Equal("(620) SHRUBS AND LAWN SCHEDULE", KpiFixture.Schedule("DM-41-(620) SHRUBS AND LAWN SCHEDULE").NameWithoutThePlot);
        }

        [Fact]
        public void ANameThatDoesNotParseIsLeftWhole()
        {
            Assert.Equal("Sheet List", KpiFixture.Schedule("Sheet List").NameWithoutThePlot);
            Assert.Equal("dm-11-(610) SOFTSCAPE SCHEDULE", KpiFixture.Schedule("dm-11-(610) SOFTSCAPE SCHEDULE").NameWithoutThePlot);
        }

        [Fact]
        public void AScheduleIsMarkedByTheWorkbookWordsInItsName()
        {
            ScannedSchedule shrubs = KpiFixture.Schedule("DM-11-(620) SHRUBS AND LAWN SCHEDULE");
            ScannedSchedule sheets = KpiFixture.Schedule("Sheet List");

            Assert.True(shrubs.IsMarked);
            Assert.Equal("SHRUB, LAWN", shrubs.MarkedFor);
            Assert.Equal("TREE", KpiFixture.Schedule("DM-11-(630) TREE SURVEY").MarkedFor);
            Assert.False(shrubs.IsSoftscape);
            Assert.True(KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE").IsSoftscape);
            Assert.False(sheets.IsMarked);
            Assert.Equal(string.Empty, sheets.MarkedFor);
        }

        [Fact]
        public void FilteredOnKeepsTheRuleAndShowsAnEmptyValue()
        {
            ScannedSchedule schedule = KpiFixture.Schedule("DM-11-(620) SHRUBS AND LAWN SCHEDULE", filters: new[]
            {
                KpiFixture.Filter("PRX_Ref Plot ID", "Equals", "DM-11"),
                KpiFixture.Filter("Type Comments", "Contains", "")
            });

            Assert.Equal("PRX_Ref Plot ID Equals DM-11; Type Comments Contains (empty)", schedule.FilteredOn);
        }

        [Fact]
        public void ACountFieldIsToldByItsFieldTypeWhateverItsCase()
        {
            Assert.True(KpiFixture.Field("QTY", "Count", "Count").IsCount);
            Assert.True(KpiFixture.Field("QTY", "Count", "count").IsCount);
            Assert.False(KpiFixture.Field("QTY", "PRX_Quantity", "Instance").IsCount);
        }

        [Fact]
        public void ALinkTypeHoldsTheMarkWhenItsNameHoldsTwoZeros()
        {
            Assert.True(KpiFixture.LinkType("RCRC_NG05_00_LINK").HoldsTheMark);
            Assert.False(KpiFixture.LinkType("RCRC_NG05_01_LINK").HoldsTheMark);
        }

        [Fact]
        public void ALinkInstanceHoldsTheMarkThroughItsOwnNameOrItsTypeName()
        {
            Assert.True(KpiFixture.LinkInstance("Site : 1", "RCRC_NG05_00_LINK").HoldsTheMark);
            Assert.True(KpiFixture.LinkInstance("RCRC_NG05_00_LINK : 1", "Site").HoldsTheMark);
            Assert.False(KpiFixture.LinkInstance("Site : 1", "RCRC_NG05_01_LINK").HoldsTheMark);
        }

        [Fact]
        public void NamesHoldingTheMarkComeBackOnceEachAndSorted()
        {
            LinkFacts links = KpiFixture.Links(
                new[] { KpiFixture.LinkType("RCRC_NG05_00_SITE"), KpiFixture.LinkType("RCRC_NG05_00_LINK") },
                new[]
                {
                    KpiFixture.LinkInstance("RCRC_NG05_00_LINK", "RCRC_NG05_00_LINK"),
                    KpiFixture.LinkInstance("RCRC_NG05_01_LINK : 1", "RCRC_NG05_01_LINK")
                });

            Assert.Equal(new[] { "RCRC_NG05_00_LINK", "RCRC_NG05_00_SITE" }, links.NamesHoldingTheMark);
        }

        [Fact]
        public void HoldsAnyIgnoresCaseAndRefusesNothing()
        {
            Assert.True(KpiNames.HoldsAny("prx_component", "COMPONENT"));
            Assert.True(KpiNames.HoldsAny("PRX_Plot_ID", "COMPONENT", "PLOT"));
            Assert.False(KpiNames.HoldsAny("Sheet Width", "COMPONENT", "PLOT"));
            Assert.False(KpiNames.HoldsAny(null, "COMPONENT"));
            Assert.False(KpiNames.HoldsAny(string.Empty, "COMPONENT"));
            Assert.False(KpiNames.HoldsAny("PRX_COMPONENT", null));
            Assert.False(KpiNames.HoldsAny("PRX_COMPONENT", string.Empty));
        }

        [Fact]
        public void WordsInJoinsTheWordsFoundInTheOrderTheyWereAskedFor()
        {
            Assert.Equal(
                "SHRUB, LAWN",
                KpiNames.WordsIn("DM-11-(620) SHRUBS AND LAWN SCHEDULE", "SOFTSCAPE", "SHRUB", "LAWN", "HARDSCAPE"));
            Assert.Equal("LAWN, SHRUB", KpiNames.WordsIn("DM-11-(620) SHRUBS AND LAWN SCHEDULE", "LAWN", "SHRUB"));
            Assert.Equal(string.Empty, KpiNames.WordsIn("Sheet List", "SOFTSCAPE", "SHRUB", "LAWN", "HARDSCAPE"));
            Assert.Equal(string.Empty, KpiNames.WordsIn("Sheet List", null));
        }

        [Fact]
        public void TheInterventionTallyIsMatchedExactlyAndTheNearMissesWithoutCase()
        {
            LinkContents exact = KpiFixture.Contents("link", regionParameters: new[]
            {
                KpiFixture.Tally("Comments", 3, 0),
                KpiFixture.Tally("PRX_Intervention Area", 3, 2),
                KpiFixture.Tally("Area", 3, 3)
            });
            LinkContents shouted = KpiFixture.Contents("link", regionParameters: new[]
            {
                KpiFixture.Tally("PRX_INTERVENTION AREA", 3, 2)
            });

            Assert.True(exact.HoldsInterventionArea);
            Assert.Equal(2, exact.InterventionTally.WithValue);
            Assert.Equal(new[] { "Area", "PRX_Intervention Area" }, exact.InterventionNearMisses);
            Assert.False(shouted.HoldsInterventionArea);
            Assert.Equal(new[] { "PRX_INTERVENTION AREA" }, shouted.InterventionNearMisses);
        }

        [Fact]
        public void ParameterNamesHoldingSpansInstanceAndTypeNamesOnceEach()
        {
            ScheduleElements elements = KpiFixture.Elements(
                "DM-11-(610) SOFTSCAPE SCHEDULE",
                instanceParameters: new[] { KpiFixture.Tally("PRX_Botanical Name", 3, 3), KpiFixture.Tally("Comments", 3, 0) },
                typeParameters: new[] { KpiFixture.Tally("PRX_Botanical Name", 1, 1), KpiFixture.Tally("PRX_Latin Name", 1, 1) });

            Assert.Equal(
                new[] { "PRX_Botanical Name", "PRX_Latin Name" },
                elements.ParameterNamesHolding("BOTANIC", "LATIN"));
        }

        [Fact]
        public void ValuesOfNamesHoldingKeepsOnlyTheMatchingParameters()
        {
            ScheduleElements elements = KpiFixture.Elements(
                "DM-11-(610) SOFTSCAPE SCHEDULE",
                wordValues: new[]
                {
                    new ParameterValueCount("Phase Created", "Existing", 12, false),
                    new ParameterValueCount("PRX_Botanical Name", "Acacia tortilis", 30, false)
                });

            Assert.Equal(
                new[] { "Existing" },
                elements.ValuesOfNamesHolding("PHASE").Select(one => one.Value));
        }

        [Fact]
        public void AProjectUnitIsKnownWhenItHasAnIdOrARounding()
        {
            Assert.False(ProjectUnit.Unknown.IsKnown);
            Assert.False(new ProjectUnit("Square meters", "", double.NaN).IsKnown);
            Assert.True(new ProjectUnit("Square meters", "autodesk.unit.unit:squareMeters-1.0.1", double.NaN).IsKnown);
            Assert.True(new ProjectUnit("Square meters", "", 0.01).IsKnown);
        }

        [Fact]
        public void AReadParameterIsNamedByAnExactMatchOnly()
        {
            ReadParameter plotId = KpiFixture.Parameter("PRX_Plot_ID", "DM-41");

            Assert.True(plotId.IsNamed("PRX_Plot_ID"));
            Assert.False(plotId.IsNamed("PRX_Ref Plot ID"));
            Assert.False(plotId.IsNamed("prx_plot_id"));
        }

        [Fact]
        public void MarkedAndSoftscapePickOutTheSchedulesByName()
        {
            ScheduleFacts facts = KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE"),
                KpiFixture.Schedule("DM-11-(600) HARDSCAPE SCHEDULE"),
                KpiFixture.Schedule("Sheet List")
            });

            Assert.Equal(
                new[] { "DM-11-(610) SOFTSCAPE SCHEDULE", "DM-11-(600) HARDSCAPE SCHEDULE" },
                facts.Marked.Select(one => one.Name));
            Assert.Equal(new[] { "DM-11-(610) SOFTSCAPE SCHEDULE" }, facts.Softscape.Select(one => one.Name));
        }

        /// <summary>
        /// A word is held when a run of letters starts with it. Solid Fill holds the letters
        /// ID and is not it, PRX_COMPONENTS holds COMPONENT, and the 00 of the link, which has
        /// no letter, is looked for anywhere.
        /// </summary>
        [Fact]
        public void AWordIsHeldByARunOfLettersStartingWithItAndNotByLettersInsideAnother()
        {
            Assert.False(KpiNames.Holds("Solid Fill", "ID"));
            Assert.False(KpiNames.Holds("Grid Plan", "ID"));
            Assert.False(KpiNames.Holds("Guide Grid", "UID"));
            Assert.True(KpiNames.Holds("ID Intervention Area", "ID"));
            Assert.True(KpiNames.Holds("PRX_Plot_UID2", "UID"));
            Assert.True(KpiNames.Holds("PRX_COMPONENTS", "COMPONENT"));
            Assert.True(KpiNames.Holds("Neighbourhood Name", "NEIGH"));
            Assert.True(KpiNames.Holds("SHRUBS&LAWN SCHEDULE", "LAWN"));
            Assert.True(KpiNames.Holds("RCRC_NG05_00_LINK", "00"));
            Assert.False(KpiNames.Holds("RCRC_NG05_01_LINK", "00"));
        }
    }
}
