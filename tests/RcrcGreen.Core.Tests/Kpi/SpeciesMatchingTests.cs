using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class SpeciesMatchingTests
    {
        private static IReadOnlyList<SpeciesMatch> Matching(
            SpeciesRow[] rows, params string[] workbookHolds)
        {
            PlotReading plot = CreateFixture.Plot("DM-12", species: rows);
            SpeciesList list = CreateFixture.WorkbookList(workbookHolds);

            return SpeciesMatching.Against(
                KpiMerge.Species(new[] { plot }), KpiTemplates.ExistingParks, list, list);
        }

        /// <summary>
        /// Revit prints ALBIZIA LEBBECK and the workbook holds Albizia lebbeck.
        /// </summary>
        [Fact]
        public void ANameMatchesWithoutCase()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13) },
                "Albizia lebbeck"));

            Assert.True(match.Matched);
            Assert.Equal("Albizia lebbeck", match.WorkbookName);
            Assert.Equal(4, match.Row);
            Assert.Equal(KpiTemplates.ProposedTreesSheet, match.SheetName);
            Assert.Equal(13, match.Species.Quantity);
        }

        [Fact]
        public void ATrailingSpaceOnEitherSideStillMatches()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("ALBIZIA LEBBECK ", CreateFixture.Proposed, 13) },
                " Albizia lebbeck"));

            Assert.True(match.Matched);
        }

        /// <summary>
        /// The model prints a species called UNKNOWN. The workbook holds four rows all named
        /// Unknown Tree. Nothing can match those on name, so it matches none of them and is
        /// reported with its count for a person to place.
        /// </summary>
        [Fact]
        public void UnknownIsNeverMatchedToUnknownTree()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 2) },
                "Unknown Tree", "Unknown Tree", "Unknown Tree", "Unknown Tree"));

            Assert.False(match.Matched);
            Assert.Equal(0, match.Row);
            Assert.Equal(2, match.Species.Quantity);
            Assert.Equal(SpeciesMatching.NotInTheList, match.Why);
        }

        /// <summary>
        /// A name the workbook holds on more than one row cannot be placed either, because
        /// nothing says which of them the quantity belongs on.
        /// </summary>
        [Fact]
        public void ANameTheWorkbookHoldsTwiceIsReportedRatherThanPlacedOnTheFirst()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("Unknown Tree", CreateFixture.Existing, 2) },
                "Unknown Tree", "Unknown Tree"));

            Assert.False(match.Matched);
            Assert.Equal(SpeciesMatching.MoreThanOneRow, match.Why);
        }

        /// <summary>
        /// ACACIA / VACHELLIA FARNESIANA carries a slash. It is matched plainly or not at all,
        /// never split on the slash and tried twice.
        /// </summary>
        [Fact]
        public void ASlashInTheNameIsMatchedPlainlyAndNeverSplit()
        {
            SpeciesMatch matched = Assert.Single(Matching(
                new[] { CreateFixture.Species("ACACIA / VACHELLIA FARNESIANA", CreateFixture.Existing, 1) },
                "Acacia / Vachellia farnesiana"));

            Assert.True(matched.Matched);

            SpeciesMatch missed = Assert.Single(Matching(
                new[] { CreateFixture.Species("ACACIA / VACHELLIA FARNESIANA", CreateFixture.Existing, 1) },
                "Acacia farnesiana"));

            Assert.False(missed.Matched);
            Assert.Equal(SpeciesMatching.NotInTheList, missed.Why);
        }

        /// <summary>
        /// BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an apostrophe, which is part of the name.
        /// </summary>
        [Fact]
        public void AnApostropheInTheNameIsPartOfIt()
        {
            SpeciesMatch matched = Assert.Single(Matching(
                new[] { CreateFixture.Species("BOUGAINVILLEA GLABRA 'PINK PIXIE'", CreateFixture.Proposed, 4) },
                "Bougainvillea glabra 'Pink Pixie'"));

            Assert.True(matched.Matched);

            SpeciesMatch missed = Assert.Single(Matching(
                new[] { CreateFixture.Species("BOUGAINVILLEA GLABRA 'PINK PIXIE'", CreateFixture.Proposed, 4) },
                "Bougainvillea glabra PINK PIXIE"));

            Assert.False(missed.Matched);
        }

        /// <summary>
        /// The shrub rows are prefixed SHRUBS: and GRASS: where the workbook's list is not, so
        /// they do not match and are reported rather than stripped into a match.
        /// </summary>
        [Fact]
        public void APrefixedShrubRowDoesNotMatchTheUnprefixedList()
        {
            SpeciesMatch missed = Assert.Single(Matching(
                new[] { CreateFixture.Species("SHRUBS: BOUGAINVILLEA GLABRA", CreateFixture.Proposed, 4) },
                "Bougainvillea glabra"));

            Assert.False(missed.Matched);
            Assert.Equal(SpeciesMatching.NotInTheList, missed.Why);
        }

        [Fact]
        public void TheGroupDecidesTheSheetAndNothingElseDoes()
        {
            var plot = CreateFixture.Plot("DM-12", species: new[]
            {
                CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Existing, 1),
                CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13)
            });
            SpeciesList list = CreateFixture.WorkbookList("Albizia lebbeck");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                KpiMerge.Species(new[] { plot }), KpiTemplates.ExistingParks, list, list);

            Assert.Equal(2, matches.Count);
            Assert.Equal(KpiTemplates.ExistingTreesSheet,
                matches.Single(one => one.Species.GroupName == "Existing").SheetName);
            Assert.Equal(KpiTemplates.ProposedTreesSheet,
                matches.Single(one => one.Species.GroupName == "Proposed").SheetName);
        }

        [Fact]
        public void AGroupNamingNeitherSheetIsReportedRatherThanPlaced()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Demolished", 3) },
                "Albizia lebbeck"));

            Assert.False(match.Matched);
            Assert.Equal(SpeciesMatching.NoSheetForTheGroup, match.Why);
        }

        /// <summary>
        /// A species the workbook holds and Revit does not is left empty and needs no line.
        /// </summary>
        [Fact]
        public void ASpeciesOnlyTheWorkbookHoldsProducesNoMatchLineAtAll()
        {
            IReadOnlyList<SpeciesMatch> matches = Matching(
                new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13) },
                "Albizia lebbeck", "Cassia glauca", "Phoenix dactylifera");

            Assert.Single(matches);
        }
    }
}
