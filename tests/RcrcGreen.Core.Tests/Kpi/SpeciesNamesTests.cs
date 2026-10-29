using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Bader's slash rule of 29 October, K36. Every name below that a workbook holds is one of
    /// the eight slash rows on all fourteen tree sheets of the seven R3 templates, measured on
    /// 29 October.
    /// </summary>
    public class SpeciesNamesTests
    {
        private static IReadOnlyList<SpeciesMatch> Matching(
            SpeciesRow[] rows, params string[] workbookHolds)
        {
            PlotReading plot = CreateFixture.Plot("DM-12", species: rows);
            SpeciesList list = CreateFixture.WorkbookList(workbookHolds);

            return SpeciesMatching.Against(
                KpiMerge.Species(new[] { plot }, CreateFixture.Counted), KpiTemplates.ExistingParks, list, list);
        }

        [Theory]
        [InlineData("Acacia / Vachellia farnesiana", "ACACIA VACHELLIA FARNESIANA")]
        [InlineData("Acacia saligna / cyanophylla", "ACACIA SALIGNA CYANOPHYLLA")]
        [InlineData("  Acacia /  Vachellia\ttortilis   raddiana ", "ACACIA VACHELLIA TORTILIS RADDIANA")]
        [InlineData("Acacia/ Vachellia seyal", "ACACIA VACHELLIA SEYAL")]
        [InlineData("Bougainvillea glabra 'Pink Pixie'", "BOUGAINVILLEA GLABRA 'PINK PIXIE'")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void TheKeyTakesOutEverySlashAndReadsEveryRunOfSpaceAsOne(string name, string key)
        {
            Assert.Equal(key, SpeciesNames.Key(name));
        }

        /// <summary>
        /// The rule as decided removes the slash and nothing else, so a slash with no space
        /// either side joins its two words. Pinned so a change to it is a decision, Q20.
        /// </summary>
        [Fact]
        public void ASlashWithNoSpaceEitherSideJoinsItsTwoWords()
        {
            Assert.Equal("ACACIAVACHELLIA SEYAL", SpeciesNames.Key("Acacia/Vachellia seyal"));
            Assert.False(SpeciesNames.Same("Acacia / Vachellia seyal", "ACACIA/VACHELLIA SEYAL"));
        }

        [Theory]
        [InlineData("Acacia / Vachellia farnesiana", "ACACIA VACHELLIA FARNESIANA")]
        [InlineData("Acacia / Vachellia seyal", "ACACIA  VACHELLIA SEYAL")]
        [InlineData("Acacia / Vachellia tortilis raddiana", "acacia / vachellia tortilis raddiana")]
        [InlineData("Acacia / Vachellia tortilis spirocarpa", "ACACIA /VACHELLIA TORTILIS SPIROCARPA")]
        [InlineData("Acacia saligna / cyanophylla", "ACACIA SALIGNA CYANOPHYLLA")]
        public void ASlashTreeTheWorkbookHoldsIsMatchedHoweverRevitPlacesTheSlash(string workbook, string revit)
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species(revit, CreateFixture.Existing, 3) }, workbook));

            Assert.True(match.Matched, match.Why);
            Assert.Equal(workbook, match.WorkbookName);
            Assert.Equal(4, match.Row);
            Assert.Equal(3, match.Species.Quantity);
        }

        /// <summary>
        /// Two workbook rows the tidy makes one name are refused, as two rows of one name always
        /// were, with both cells named.
        /// </summary>
        [Fact]
        public void TwoRowsTheTidyMakesOneNameAreRefusedNamingBoth()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("ACACIA VACHELLIA SEYAL", CreateFixture.Existing, 2) },
                "Acacia / Vachellia seyal", "Acacia Vachellia seyal"));

            Assert.False(match.Placed);
            Assert.Equal(
                "the workbook holds this name on more than one row, so nothing can say which and "
                + "the count was not written: Tree List - Existing holds it in D4 and D5",
                match.Why);
        }

        /// <summary>
        /// Two plots spelling one tree two ways are one species before the list is asked, so
        /// one count of 7 lands on the row rather than 3 and then 4 aimed at the same cell.
        /// </summary>
        [Fact]
        public void TwoSpellingsOnTwoPlotsMergeIntoOneSpecies()
        {
            PlotReading one = CreateFixture.Plot("ST-01", species: new[]
            {
                CreateFixture.Species("ACACIA / VACHELLIA FARNESIANA", CreateFixture.Existing, 3)
            });
            PlotReading other = CreateFixture.Plot("ST-02", species: new[]
            {
                CreateFixture.Species("ACACIA VACHELLIA FARNESIANA", CreateFixture.Existing, 4)
            });

            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(new[] { one, other }, CreateFixture.Counted);
            MergedSpecies tree = Assert.Single(merged);

            Assert.Equal(7, tree.Quantity);
            Assert.Equal("ACACIA / VACHELLIA FARNESIANA", tree.BotanicalName);
        }

        /// <summary>
        /// One plot printing one tree two ways under one group is the schedule printing a species
        /// on two rows, which refuses, rather than an addition nobody sees.
        /// </summary>
        [Fact]
        public void TwoSpellingsOnOnePlotUnderOneGroupAreOneSpeciesOnTwoRows()
        {
            PlotReading reading = CreateFixture.Plot("ST-01", species: new[]
            {
                CreateFixture.Species("ACACIA / VACHELLIA SEYAL", CreateFixture.Existing, 3),
                CreateFixture.Species("ACACIA VACHELLIA SEYAL", CreateFixture.Existing, 4)
            });

            RepeatedSpecies repeated = Assert.Single(reading.SpeciesPrintedOnMoreThanOneRow);
            Assert.Equal("ACACIA / VACHELLIA SEYAL", repeated.BotanicalName);
        }

        /// <summary>
        /// UNKNOWN still reaches Unknown Tree through its alias and through nothing else, and the
        /// apostrophe is still part of a name. Nothing past the slash and the spaces changed.
        /// </summary>
        [Fact]
        public void UnknownAndTheApostropheAreAsTheyWere()
        {
            SpeciesMatch unknown = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 2) }, "Unknown Tree"));
            Assert.True(unknown.ThroughAnAlias);

            SpeciesMatch pixie = Assert.Single(Matching(
                new[] { CreateFixture.Species("BOUGAINVILLEA GLABRA PINK PIXIE", CreateFixture.Existing, 4) },
                "Bougainvillea glabra 'Pink Pixie'"));
            Assert.False(pixie.Matched);
        }
    }
}
