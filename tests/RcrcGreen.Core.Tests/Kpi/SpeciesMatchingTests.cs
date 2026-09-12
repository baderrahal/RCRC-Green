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
                KpiMerge.Species(new[] { plot }, CreateFixture.Counted), KpiTemplates.ExistingParks, list, list);
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
        ///
        /// **It is also the one species measured to carry no size**, a dash for its height and
        /// 0 for its canopy diameter, so it takes no empty row either and the reason names the
        /// measures rather than the name. Both halves are what the model prints.
        /// </summary>
        [Fact]
        public void UnknownIsNeverMatchedToUnknownTreeAndTakesNoRowEither()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 2, 19, "-", "0") },
                "Unknown Tree", "Unknown Tree", "Unknown Tree", "Unknown Tree"));

            // Not matched, not written, and named with its count. The four Unknown Tree rows
            // are the client's and nothing may put a quantity on one of them by guessing.
            Assert.False(match.Matched);
            Assert.False(match.Added);
            Assert.False(match.Placed);
            Assert.Equal(0, match.Row);
            Assert.Equal(2, match.Species.Quantity);
            Assert.Equal(
                "the workbook's list does not hold this name, and a row written into an empty one carries only "
                + "what the model prints, which is no canopy diameter a workbook can compute with, "
                + "so no row was written: DM-12 row 19 prints 0, which is no size",
                match.Why);
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
            Assert.True(missed.Added);
            Assert.Equal(SpeciesMatching.WrittenIn, missed.Why);
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
            Assert.True(missed.Added);
            Assert.Equal(SpeciesMatching.WrittenIn, missed.Why);
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
                KpiMerge.Species(new[] { plot }, CreateFixture.Counted), KpiTemplates.ExistingParks, list, list);

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

        /// <summary>
        /// The three DM-12 measured, all under Existing, with the sizes the runs measured:
        /// PHOENIX DACTYLIFERA 18 and 15, WASHINGTONIA ROBUSTA 25 and 5, and UNKNOWN a dash and
        /// a 0. The MOSQUES list holds 80 species and none of them, so the two the model sizes
        /// are written into the empty rows under the list rather than left out, which is what
        /// made the workbook read 31 trees where the model held 39.
        ///
        /// **UNKNOWN sits between them and takes no row with it**, which is the order the rule
        /// depends on: the size is asked before a row is taken, so rows 6 and 7 go to the two
        /// that can use them rather than 6 and 8.
        /// </summary>
        [Fact]
        public void EveryUnmatchedSpeciesTheModelSizesGoesIntoAnEmptyRowInOrder()
        {
            IReadOnlyList<SpeciesMatch> matches = Matching(
                new[]
                {
                    CreateFixture.Species("PHOENIX DACTYLIFERA", CreateFixture.Existing, 5, 18, "18", "15"),
                    CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 2, 19, "-", "0"),
                    CreateFixture.Species("WASHINGTONIA ROBUSTA", CreateFixture.Existing, 1, 20, "25", "5")
                },
                "Albizia lebbeck", "Cassia glauca");

            Assert.All(matches, one => Assert.False(one.Matched));

            // Merged order is by name: PHOENIX, UNKNOWN, WASHINGTONIA. Rows 4 and 5 hold the
            // two names the list has, so the empty rows are 6, 7 and 8, and 8 is never reached.
            Assert.Equal(new[] { "PHOENIX DACTYLIFERA", "UNKNOWN", "WASHINGTONIA ROBUSTA" },
                matches.Select(one => one.Species.BotanicalName));
            Assert.Equal(new[] { true, false, true }, matches.Select(one => one.Added));
            Assert.Equal(new[] { 6, 0, 7 }, matches.Select(one => one.Row));
            Assert.Equal(new[] { 5, 2, 1 }, matches.Select(one => one.Species.Quantity));
        }

        /// <summary>
        /// More unmatched species than empty rows: what fits goes in, the rest are named, and
        /// the reason says the sheet ran out of room rather than blaming the name.
        /// </summary>
        [Fact]
        public void MoreUnmatchedSpeciesThanEmptyRowsWritesWhatFitsAndNamesTheRest()
        {
            SpeciesList list = CreateFixture.WorkbookListWithRoomFor(1, "Albizia lebbeck");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                new[]
                {
                    CreateFixture.Merged("PHOENIX DACTYLIFERA", CreateFixture.Existing, "DM-12", 5),
                    CreateFixture.Merged("WASHINGTONIA ROBUSTA", CreateFixture.Existing, "DM-12", 1)
                },
                KpiTemplates.Mosques, list, list);

            Assert.Equal(5, matches[0].Row);
            Assert.True(matches[0].Added);

            Assert.Equal(0, matches[1].Row);
            Assert.False(matches[1].Placed);
            Assert.Equal(SpeciesMatching.NoEmptyRowLeft, matches[1].Why);
        }

        /// <summary>
        /// A list with no empty row at all places nothing and says why, rather than writing
        /// into a row the total does not sum.
        /// </summary>
        [Fact]
        public void AListWithNoRoomPlacesNothingAndSaysWhy()
        {
            SpeciesList list = CreateFixture.WorkbookListWithRoomFor(0, "Albizia lebbeck");

            SpeciesMatch match = Assert.Single(SpeciesMatching.Against(
                new[] { CreateFixture.Merged("PHOENIX DACTYLIFERA", CreateFixture.Existing, "DM-12", 2) },
                KpiTemplates.Mosques, list, list));

            Assert.False(match.Placed);
            Assert.Equal(SpeciesMatching.NoEmptyRowLeft, match.Why);
        }

        /// <summary>
        /// **A species the model does not size gets no row at all.** A row written into an
        /// empty one carries only what the model prints, and the client's own formulas read
        /// the canopy diameter column, so a name and a count with no diameter leaves them
        /// computing on a blank. Measured on the 1836 run: UNKNOWN went into Tree List -
        /// Proposed row 85 that way, seven formulas would have read an error, and the workbook
        /// was deleted. It is reported with its count instead and the run goes through.
        /// </summary>
        [Fact]
        public void ASpeciesTheModelDoesNotSizeTakesNoRowAndIsNamedWithItsCount()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Proposed, 1, 19, string.Empty, "0") },
                "Albizia lebbeck"));

            Assert.False(match.Placed);
            Assert.False(match.Added);
            Assert.False(match.Matched);
            Assert.Equal(0, match.Row);
            Assert.Equal(1, match.Species.Quantity);
            Assert.Equal(
                "the workbook's list does not hold this name, and a row written into an empty one carries only "
                + "what the model prints, which is no canopy diameter a workbook can compute with, "
                + "so no row was written: DM-12 row 19 prints 0, which is no size",
                match.Why);
        }

        /// <summary>
        /// The size is asked before a row is taken, so a species refused for it leaves the
        /// first empty row for the next species rather than using one up. The queue is what
        /// stops two species landing on one row, and a refusal must not disturb it.
        /// </summary>
        [Fact]
        public void ASpeciesRefusedForItsSizeLeavesTheEmptyRowForTheNextOne()
        {
            SpeciesList list = CreateFixture.WorkbookListWithRoomFor(1, "Albizia lebbeck");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                KpiMerge.Species(new[]
                {
                    CreateFixture.Plot("DM-12", species: new[]
                    {
                        CreateFixture.Species("CASSIA GLAUCA", CreateFixture.Proposed, 1, 19, "-", "0"),
                        CreateFixture.Species("PHOENIX DACTYLIFERA", CreateFixture.Proposed, 5, 20, "18", "15")
                    })
                }, CreateFixture.Counted),
                KpiTemplates.Mosques, list, list);

            // The merge orders by name, so the unsized one is reached FIRST. That is what makes
            // this a test of the order: a check made after the row was taken would leave the
            // sized species with nowhere to go, and the assertion below would fail.
            Assert.Equal(new[] { "CASSIA GLAUCA", "PHOENIX DACTYLIFERA" }, matches.Select(one => one.Species.BotanicalName));

            SpeciesMatch unsized = matches[0];
            Assert.False(unsized.Placed);
            Assert.Equal(0, unsized.Row);

            // Row 5 is the one empty row, and it went to the species that can use it.
            SpeciesMatch phoenix = matches[1];
            Assert.True(phoenix.Added);
            Assert.Equal(5, phoenix.Row);
        }

        /// <summary>
        /// Two plots printing two different diameters for one species writes nothing into that
        /// column, which is the rule that was already there, so such a species cannot be
        /// written into an empty row either.
        /// </summary>
        [Fact]
        public void RowsThatDisagreeOnAMeasureTakeNoRowEither()
        {
            SpeciesList list = CreateFixture.WorkbookListWithRoomFor(3, "Albizia lebbeck");

            SpeciesMatch match = Assert.Single(SpeciesMatching.Against(
                KpiMerge.Species(new[]
                {
                    CreateFixture.Plot("DM-12", species: new[] { CreateFixture.Species("PHOENIX DACTYLIFERA", CreateFixture.Proposed, 1, 19, "18", "15") }),
                    CreateFixture.Plot("DM-13", species: new[] { CreateFixture.Species("PHOENIX DACTYLIFERA", CreateFixture.Proposed, 2, 21, "18", "9") })
                }, CreateFixture.Counted),
                KpiTemplates.Mosques, list, list));

            Assert.False(match.Placed);
            Assert.Contains("no canopy diameter a workbook can compute with", match.Why);
            Assert.Contains("DM-12 row 19 prints 15", match.Why);
            Assert.Contains("DM-13 row 21 prints 9", match.Why);
        }

        /// <summary>
        /// A sheet whose total the reader cannot find writes nothing for anybody, which is the
        /// larger fact, so it is still said first for a species that is also unsized.
        /// </summary>
        [Fact]
        public void ASheetWithNoTotalIsSaidBeforeTheSize()
        {
            SpeciesList list = SpeciesList.WithNoTotal(new[] { new SpeciesListRow(4, "Albizia lebbeck") });

            SpeciesMatch match = Assert.Single(SpeciesMatching.Against(
                KpiMerge.Species(new[]
                {
                    CreateFixture.Plot("DM-12", species: new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Proposed, 1, 19, "-", "0") })
                }, CreateFixture.Counted),
                KpiTemplates.Mosques, list, list));

            Assert.False(match.Placed);
            Assert.Equal(SpeciesMatching.NoTotalToReach(list), match.Why);
            Assert.DoesNotContain("no canopy diameter", match.Why);
        }
    }
}
