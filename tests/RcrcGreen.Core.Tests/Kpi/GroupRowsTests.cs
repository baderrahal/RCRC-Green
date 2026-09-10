using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// FM-05's softscape schedule prints THREE groups, measured off the 1536 report: Existing
    /// at row 3 with four species adding to 6, Proposed at row 9 with ALBIZIA LEBBECK 10,
    /// BAUHINIA PURPUREA 19 and CASSIA GLAUCA 3 adding to 32, and Street Design at row 14 with
    /// ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4 and CONOCARPUS 4 adding to 38,
    /// then TOTAL 76. Two rounds read the third group's rows as a second schedule and then as
    /// a species printed twice under Proposed, and both were wrong.
    ///
    /// **Bader has decided that Street Design is somebody else's scope.** Its rows are left
    /// out, named, and checked: the groups taken plus the groups left out must equal the
    /// TOTAL row, 6 plus 32 plus 38 is 76. Nothing here matches a word: a group is taken when
    /// a tree list sheet of the template is named for it, and the sheets are Tree List -
    /// Existing and Tree List - Proposed.
    ///
    /// The four existing species and their split of the 6 are not in the note the numbers
    /// came from, so the names below are the ones earlier runs measured on mosque plots and
    /// the split is one, two, two and one. Everything else is the schedule as printed.
    /// </summary>
    public class GroupRowsTests
    {
        private static readonly string[] Headings =
            { "IMAGE", "PLANT CODE", "BOTANICAL NAME", "COUNT (n)" };

        private static string[] Structure(string text)
        {
            return new[] { text, "", "", "" };
        }

        private static string[] Species(string name, int count)
        {
            return new[] { name + ".jpg", "", name, count.ToString() };
        }

        private static string[] Subtotal(int count)
        {
            return new[] { "", "", "", count.ToString() };
        }

        private static ScannedSchedule Fm05(string total = "76", string proposedSubtotal = "32")
        {
            return CreateFixture.Softscape(
                "FM-05",
                Headings,
                Structure("TREES"),
                Structure("Existing"),
                Species("ACACIA / VACHELLIA FARNESIANA", 1),
                Species("PHOENIX DACTYLIFERA", 2),
                Species("UNKNOWN", 2),
                Species("WASHINGTONIA ROBUSTA", 1),
                Subtotal(6),
                Structure("Proposed"),
                Species("ALBIZIA LEBBECK", 10),
                Species("BAUHINIA PURPUREA", 19),
                Species("CASSIA GLAUCA", 3),
                new[] { "", "", "", proposedSubtotal },
                Structure("Street Design"),
                Species("ALBIZIA LEBBECK", 10),
                Species("BAUHINIA PURPUREA", 20),
                Species("CASSIA GLAUCA", 4),
                Species("CONOCARPUS", 4),
                Subtotal(38),
                new[] { "TOTAL", "", "", total });
        }

        /// <summary>
        /// FM-05's shrubs and lawn schedule as the 1536 report printed it: GRASS Proposed 96
        /// over 117 and Street Design 69 over 84, total 165 over 201, then SHRUBS AND GROUND
        /// COVER Proposed 361 over 450 and Street Design 459 over 570, total 820 over 1020.
        /// </summary>
        private static ScannedSchedule Fm05Ground(string shrubsTotal = "820 m²")
        {
            return CreateFixture.ShrubsAndLawn(
                "FM-05",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "Proposed", "", "", "" },
                new[] { "-", "GRASS: CYNODON DACTYLON", "96 m²", "117" },
                new[] { "", "", "96 m²", "117" },
                new[] { "Street Design", "", "", "" },
                new[] { "Pennisetum setaceum.jpg", "GRASS: PENNISETUM SETACEUM", "69 m²", "84" },
                new[] { "", "", "69 m²", "84" },
                new[] { "", "", "165 m²", "201" },
                new[] { "SHRUBS & GROUND COVER", "", "", "" },
                new[] { "Proposed", "", "", "" },
                new[] { "Carissa.jpg", "SHRUBS: CARISSA MACROCARPA", "361 m²", "450" },
                new[] { "", "", "361 m²", "450" },
                new[] { "Street Design", "", "", "" },
                new[] { "Lantana.jpg", "SHRUBS: LANTANA CAMARA", "459 m²", "570" },
                new[] { "", "", "459 m²", "570" },
                new[] { "", "", shrubsTotal, "1020" },
                new[] { "TOTAL", "", "985 m²", "1221" });
        }

        private static PlotReading Fm05Reading(ScannedSchedule softscape = null, ScannedSchedule ground = null)
        {
            ScannedSchedule trees = softscape ?? Fm05();
            ScannedSchedule shrubs = ground ?? Fm05Ground();
            SoftscapeReading read = SoftscapeRows.Read(trees, CreateFixture.Counted, "FM-05");
            Assert.True(read.WasRead, string.Join(" ", read.Refusals));
            ShrubsAndLawnReading groundRead = ShrubsAndLawnRows.Read(
                shrubs, new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }, CreateFixture.Counted);
            Assert.True(groundRead.WasRead, string.Join(" ", groundRead.Refusals));

            return CreateFixture.Plot(
                "FM-05",
                species: read.Species.ToArray(),
                subtotals: groundRead.Subtotals.ToArray(),
                softscapeTotalRead: read.TotalRead,
                softscapeTotal: read.Total,
                rowsPassedOver: read.RowsPassedOver,
                printedSchedules: new[] { trees, shrubs },
                softscapeTotalRow: read.TotalRow,
                printedGroups: read.Groups.ToArray());
        }

        private static Reconciliation Held(params PlotReading[] readings)
        {
            return Reconciliation.Of(readings.Select(one => one.PlotId), readings, null, false, KpiTemplates.Mosques);
        }

        private static string Report(params PlotReading[] readings)
        {
            return KpiCreateReport.Write(CreateFixture.Run(readings: readings), new DateTime(2026, 9, 10, 15, 36, 0));
        }

        /// <summary>
        /// Seven species rows come back, the four under Existing and the three under Proposed,
        /// adding to 38. The four under Street Design add to 38 as well and are held apart.
        /// TOTAL 76 at row 20.
        /// </summary>
        [Fact]
        public void Fm05ReadsSevenSpeciesRowsAndLeavesStreetDesignsFourOut()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Fm05(), CreateFixture.Counted, "FM-05");

            Assert.True(reading.WasRead, string.Join(" ", reading.Refusals));
            Assert.Equal(7, reading.Species.Count);
            Assert.Equal(new[] { 4, 5, 6, 7, 10, 11, 12 }, reading.Species.Select(one => one.RowNumber));
            Assert.Equal(38, reading.SpeciesSum);
            Assert.Equal(4, reading.LeftOut.Count);
            Assert.Equal(new[] { 15, 16, 17, 18 }, reading.LeftOut.Select(one => one.RowNumber));
            Assert.Equal(38, reading.LeftOut.Sum(one => one.Quantity));
            Assert.True(reading.TotalRead);
            Assert.Equal(76, reading.Total);
            Assert.Equal(20, reading.TotalRow);
            Assert.Equal(3, reading.RowsPassedOver);
        }

        /// <summary>
        /// Every group row, in printed order, with its row, its subtotal row and whether it was
        /// taken. The heading TREES is not a group and is not in the list.
        /// </summary>
        [Fact]
        public void EveryGroupRowIsNamedWithItsSubtotalAndWhetherItWasTaken()
        {
            IReadOnlyList<PrintedGroup> groups = SoftscapeRows.Read(Fm05(), CreateFixture.Counted, "FM-05").Groups;

            Assert.Equal(3, groups.Count);

            Assert.Equal("Existing", groups[0].Name);
            Assert.Equal(3, groups[0].RowNumber);
            Assert.Equal(4, groups[0].Species.Count);
            Assert.Equal(6, groups[0].SpeciesSum);
            Assert.True(groups[0].SubtotalPrinted);
            Assert.Equal(6, groups[0].Subtotal);
            Assert.Equal(8, groups[0].SubtotalRow);
            Assert.True(groups[0].Counted);
            Assert.Equal("Tree List - Existing is named for it", groups[0].Why);

            Assert.Equal("Proposed", groups[1].Name);
            Assert.Equal(9, groups[1].RowNumber);
            Assert.Equal(32, groups[1].SpeciesSum);
            Assert.Equal(13, groups[1].SubtotalRow);
            Assert.True(groups[1].Counted);
            Assert.Equal("Tree List - Proposed is named for it", groups[1].Why);

            Assert.Equal("Street Design", groups[2].Name);
            Assert.Equal(14, groups[2].RowNumber);
            Assert.Equal(4, groups[2].Species.Count);
            Assert.Equal(38, groups[2].SpeciesSum);
            Assert.Equal(38, groups[2].Subtotal);
            Assert.Equal(19, groups[2].SubtotalRow);
            Assert.False(groups[2].Counted);
            Assert.Equal("no tree list sheet is named for it, so it is out of scope", groups[2].Why);
            Assert.All(groups, one => Assert.Equal(string.Empty, one.Disagreement));
        }

        /// <summary>
        /// A species row remembers the group row it sat under, which is what tells two Existing
        /// groups apart on DM-25.
        /// </summary>
        [Fact]
        public void ASpeciesRowCarriesTheRowOfTheGroupItSatUnder()
        {
            IReadOnlyList<SpeciesRow> species = SoftscapeRows.Read(Fm05(), CreateFixture.Counted, "FM-05").Species;

            Assert.Equal(3, species[0].GroupRowNumber);
            Assert.Equal(3, species[3].GroupRowNumber);
            Assert.Equal(9, species[4].GroupRowNumber);
            Assert.Equal(9, species[6].GroupRowNumber);
        }

        /// <summary>
        /// The accounting passes, 6 plus 32 plus 38 is 76, names the plot as one holding a
        /// group left out, and counts two schedules on it, the softscape and the shrubs and
        /// lawn, because both printed Street Design.
        /// </summary>
        [Fact]
        public void TheAccountingAddsUpAndNamesThePlotAndCountsBothSchedules()
        {
            Reconciliation held = Held(Fm05Reading());

            Assert.True(held.AddsUp, string.Join(" ", held.Refusals));
            Assert.Equal(new[] { "FM-05" }, held.WithAGroupLeftOut);
            Assert.Equal(2, held.SchedulesWithAGroupLeftOut);
        }

        /// <summary>
        /// What reaches the workbook: 6 existing and 32 proposed trees, ALBIZIA LEBBECK 10 and
        /// not 20, grass 96 and shrubs 361. The rule before this one refused the plot, and the
        /// one before that wrote 165 and 820.
        /// </summary>
        [Fact]
        public void Fm05WritesSixExistingThirtyTwoProposedGrassNinetySixAndShrubsThreeSixtyOne()
        {
            PlotReading[] readings = { Fm05Reading() };

            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(readings, CreateFixture.Counted);
            Assert.Equal(6, merged.Where(one => one.GroupName == "Existing").Sum(one => one.Quantity));
            Assert.Equal(32, merged.Where(one => one.GroupName == "Proposed").Sum(one => one.Quantity));
            Assert.Equal(10, merged.Single(one => one.BotanicalName == "ALBIZIA LEBBECK").Quantity);
            Assert.DoesNotContain(merged, one => one.BotanicalName == "CONOCARPUS");
            Assert.DoesNotContain(merged, one => one.GroupName == "Street Design");

            Assert.Equal(96.0, KpiMerge.Lawn(readings).Total);
            Assert.Equal(361.0, KpiMerge.Shrubs(readings).Total);
        }

        /// <summary>
        /// A TOTAL the taken and left out rows do not reach is refused with all three numbers.
        /// 38 plus 38 is 76, not 80.
        /// </summary>
        [Fact]
        public void ATotalTheTakenAndLeftOutRowsDoNotReachRefusesNamingAllThree()
        {
            Reconciliation held = Held(Fm05Reading(Fm05(total: "80")));

            Assert.False(held.AddsUp);
            Assert.Equal(
                "FM-05: its species rows add to 38, 38 more were left out under groups no tree list sheet is named for, "
                + "and its softscape schedule prints TOTAL 80.",
                Assert.Single(held.Refusals));
        }

        /// <summary>
        /// Each group is held against its own subtotal row. Proposed adds to 32 and a subtotal
        /// row printing 33 is a refusal naming the group, its row and both numbers.
        /// </summary>
        [Fact]
        public void AGroupWhoseSubtotalRowDisagreesWithItsSpeciesRowsRefuses()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Fm05(proposedSubtotal: "33"), CreateFixture.Counted, "FM-05");
            Assert.Equal("its species rows add to 32 and its subtotal row 13 prints 33", reading.Groups[1].Disagreement);

            Reconciliation held = Held(Fm05Reading(Fm05(proposedSubtotal: "33")));

            Assert.False(held.AddsUp);
            Assert.Equal("FM-05, Proposed at row 9: its species rows add to 32 and its subtotal row 13 prints 33.", held.Refusals[0]);
        }

        /// <summary>
        /// ALBIZIA LEBBECK on row 10 under Proposed and row 15 under Street Design is two
        /// groups, not one species printed twice, so it refuses nothing. That is exactly the
        /// FM-05 10, FM-05 10 the last two rounds refused on.
        /// </summary>
        [Fact]
        public void OneSpeciesUnderProposedAndUnderStreetDesignIsNotARefusal()
        {
            PlotReading reading = Fm05Reading();

            Assert.Empty(reading.SpeciesPrintedOnMoreThanOneRow);
            Assert.True(Held(reading).AddsUp);
        }

        /// <summary>
        /// The same species twice under one group is still the refusal it was, with the rows
        /// and the counts named.
        /// </summary>
        [Fact]
        public void OneSpeciesTwiceUnderProposedStillRefusesNamingTheRows()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "FM-05",
                Headings,
                Structure("TREES"),
                Structure("Proposed"),
                Species("ALBIZIA LEBBECK", 10),
                Species("ALBIZIA LEBBECK", 10),
                Subtotal(20),
                Structure("Street Design"),
                Species("ALBIZIA LEBBECK", 10),
                Subtotal(10),
                new[] { "TOTAL", "", "", "30" });

            PlotReading reading = Fm05Reading(schedule);
            RepeatedSpecies repeated = Assert.Single(reading.SpeciesPrintedOnMoreThanOneRow);

            Assert.Equal("rows 4 and 5, counting 10 and 10", repeated.InWords);
            Assert.Contains("FM-05 prints ALBIZIA LEBBECK on 2 rows under Proposed, rows 4 and 5, counting 10 and 10.", Held(reading).Refusals[0]);
        }

        /// <summary>
        /// DM-25 prints Existing, then Proposed, then Existing again. Nothing guesses which
        /// Existing is meant: both are group rows, both are named for by the same sheet, both
        /// are taken, and the second says it is the second.
        /// </summary>
        [Fact]
        public void AGroupNamePrintedTwiceIsTakenBothTimesAndTheSecondSaysSo()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-25",
                Headings,
                Structure("TREES"),
                Structure("Existing"),
                Species("PHOENIX DACTYLIFERA", 1),
                Subtotal(1),
                Structure("Proposed"),
                Species("ALBIZIA LEBBECK", 5),
                Subtotal(5),
                Structure("Existing"),
                Species("WASHINGTONIA ROBUSTA", 2),
                Subtotal(2),
                new[] { "TOTAL", "", "", "8" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, CreateFixture.Counted, "DM-25");

            Assert.Equal(3, reading.Groups.Count);
            Assert.Equal(new[] { 3, 6, 9 }, reading.Groups.Select(one => one.RowNumber));
            Assert.All(reading.Groups, one => Assert.True(one.Counted));
            Assert.Equal("Tree List - Existing is named for it", reading.Groups[0].Why);
            Assert.Equal(
                "Tree List - Existing is named for it, and it is the 2nd group row so named on this schedule, taken as well",
                reading.Groups[2].Why);
            Assert.Equal(3, reading.Species.Count);
            Assert.Equal(8, reading.SpeciesSum);
            Assert.Empty(reading.LeftOut);
        }

        /// <summary>
        /// One species under both Existing groups is the same species under one group name
        /// twice, and nothing says whether that is one counted twice, so it refuses, naming
        /// the two group rows so the person can see it is two groups and not one.
        /// </summary>
        [Fact]
        public void OneSpeciesUnderBothExistingGroupsRefusesNamingBothGroupRows()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-25",
                Headings,
                Structure("TREES"),
                Structure("Existing"),
                Species("PHOENIX DACTYLIFERA", 1),
                Subtotal(1),
                Structure("Proposed"),
                Species("ALBIZIA LEBBECK", 5),
                Subtotal(5),
                Structure("Existing"),
                Species("PHOENIX DACTYLIFERA", 2),
                Subtotal(2),
                new[] { "TOTAL", "", "", "8" });

            SoftscapeReading read = SoftscapeRows.Read(schedule, CreateFixture.Counted, "DM-25");
            PlotReading reading = CreateFixture.Plot("DM-25", species: read.Species.ToArray(),
                softscapeTotalRead: true, softscapeTotal: 8, printedGroups: read.Groups.ToArray());

            RepeatedSpecies repeated = Assert.Single(reading.SpeciesPrintedOnMoreThanOneRow);
            Assert.Equal("PHOENIX DACTYLIFERA", repeated.BotanicalName);
            Assert.Equal(
                "rows 4 and 10, counting 1 and 2, under 2 group rows both named Existing, rows 3 and 9",
                repeated.InWords);
            Assert.False(Held(reading).AddsUp);
        }

        /// <summary>
        /// GRASS is 96 off the Proposed row with the street's 69 left out, and the group total
        /// row 165 is checked against both. SHRUBS the same: 361 taken, 459 left out, 820
        /// checked.
        /// </summary>
        [Fact]
        public void Fm05GrassIsNinetySixAndShrubsThreeSixtyOneWithTheGroupTotalsChecked()
        {
            ShrubsAndLawnReading reading = ShrubsAndLawnRows.Read(
                Fm05Ground(), new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }, CreateFixture.Counted);

            Assert.True(reading.WasRead, string.Join(" ", reading.Refusals));
            Assert.Equal(2, reading.Subtotals.Count);

            GroupSubtotal grass = reading.Subtotals[0];
            Assert.Equal("GRASS", grass.Heading);
            Assert.Equal(96.0, grass.SquareMetres);
            Assert.Equal(117, grass.ItemCount);
            Assert.Equal(9, grass.RowNumber);
            Assert.True(grass.Agrees);
            Assert.Equal(165.0, grass.GroupTotalSquareMetres);
            Assert.Equal("Street Design", Assert.Single(grass.PhasesLeftOut).Name);

            GroupSubtotal shrubs = reading.Subtotals[1];
            Assert.Equal(361.0, shrubs.SquareMetres);
            Assert.Equal(450, shrubs.ItemCount);
            Assert.Equal(17, shrubs.RowNumber);
            Assert.True(shrubs.Agrees);
            Assert.Equal(820.0, shrubs.GroupTotalSquareMetres);
            Assert.Equal(1020, shrubs.GroupTotalItemCount);
        }

        /// <summary>
        /// A group total the phases do not add to refuses, whether the phase that breaks it
        /// was taken or left out. 361 plus 459 is 820, not 830.
        /// </summary>
        [Fact]
        public void AGroupTotalThePhaseRowsDoNotAddToRefusesTheWrite()
        {
            Reconciliation held = Held(Fm05Reading(ground: Fm05Ground(shrubsTotal: "830 m²")));

            Assert.False(held.AddsUp);
            Assert.Contains(held.Refusals, one =>
                one.Contains("SHRUBS & GROUND COVER")
                && one.Contains("its group total reads 830 over 1020 and the 2 rows above it add to 820 over 1020"));
        }

        /// <summary>
        /// The report names every group row under the plot, in printed order, with its row,
        /// its subtotal, taken or left out, and why. The rows left out are listed by name and
        /// count under the group. Rows by hand off the two fixtures: the shrubs and lawn
        /// subtotal rows are 5, 8 and 9 for GRASS and 13, 16 and 17 for SHRUBS.
        /// </summary>
        [Fact]
        public void TheReportNamesEveryGroupRowUnderThePlot()
        {
            string report = Report(Fm05Reading());

            Assert.Contains(
                "    softscape schedule: FM-05-(600) SOFTSCAPE SCHEDULE, 7 species rows read\r\n"
                + "      species rows add to 38, 38 left out, the TOTAL row prints 76 at row 20\r\n"
                + "      3 rows with a count and no botanical name passed over, the subtotals\r\n"
                + "      group rows: 3\r\n"
                + "        row 3 Existing: 4 species rows adding to 6, subtotal row 8 prints 6, TAKEN, Tree List - Existing is named for it\r\n"
                + "        row 9 Proposed: 3 species rows adding to 32, subtotal row 13 prints 32, TAKEN, Tree List - Proposed is named for it\r\n"
                + "        row 14 Street Design: 4 species rows adding to 38, subtotal row 19 prints 38, LEFT OUT, no tree list sheet is named for it, so it is out of scope\r\n"
                + "          left out: ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4, CONOCARPUS 4\r\n"
                + "    shrubs and lawn schedule: FM-05-(600) SHRUBS AND LAWN SCHEDULE, 2 groups read\r\n"
                + "      GRASS | 96 | 117 items, the phase rows taken added together\r\n"
                + "        row 5 Proposed: 96 over 117, TAKEN, Tree List - Proposed is named for it\r\n"
                + "        row 8 Street Design: 69 over 84, LEFT OUT, no tree list sheet is named for it, so it is out of scope\r\n"
                + "        group total row 9 prints 165 over 201, and the phase rows taken and left out add to it\r\n"
                + "      SHRUBS & GROUND COVER | 361 | 450 items, the phase rows taken added together\r\n"
                + "        row 13 Proposed: 361 over 450, TAKEN, Tree List - Proposed is named for it\r\n"
                + "        row 16 Street Design: 459 over 570, LEFT OUT, no tree list sheet is named for it, so it is out of scope\r\n"
                + "        group total row 17 prints 820 over 1020, and the phase rows taken and left out add to it\r\n",
                report);
        }

        /// <summary>
        /// The accounting at the top says how many schedules held such a group and on which
        /// plots, so the street shows on the first page of the first run and not on the fourth.
        /// </summary>
        [Fact]
        public void TheAccountingLineCountsTheSchedulesAndNamesThePlots()
        {
            Assert.Contains(
                "  schedules holding a group no tree list sheet is named for   2, on FM-05. Their rows are left out and named under the plot.\r\n",
                Report(Fm05Reading()));

            Assert.Contains(
                "  schedules holding a group no tree list sheet is named for   0\r\n",
                Report(CreateFixture.Plot("DM-12")));
        }

        /// <summary>
        /// The printed section says which rows were group rows, which were read and which were
        /// left out, by row number, so the drawing can be held against it.
        /// </summary>
        [Fact]
        public void ThePrintedSectionSaysWhichRowsWereLeftOut()
        {
            Assert.Contains(
                "  FM-05, FM-05-(600) SOFTSCAPE SCHEDULE\r\n"
                + "    group rows: 3, 9, 14, read as species rows: 7, rows 4, 5, 6, 7, 10, 11, 12, left out: 4, rows 15 to 18, "
                + "passed over as subtotals: 3, TOTAL row: row 20\r\n",
                Report(Fm05Reading()));
        }

        /// <summary>
        /// A group counts when a tree list sheet's name ends in the group's name, word for
        /// word and without case. Nothing looser: Tree and List are words of both sheet names
        /// and are not what either sheet is for, and TREES is the heading over the groups.
        /// </summary>
        [Fact]
        public void AGroupCountsWhenATreeListSheetIsNamedForItAndNotOnAWord()
        {
            CountedGroups counted = CountedGroups.Of(KpiTemplates.Mosques);

            Assert.Equal(new[] { "Tree List - Existing", "Tree List - Proposed" }, counted.SheetNames);
            Assert.True(counted.Counts("Existing"));
            Assert.True(counted.Counts("EXISTING"));
            Assert.True(counted.Counts(" Proposed "));
            Assert.False(counted.Counts("Street Design"));
            Assert.False(counted.Counts("Tree"));
            Assert.False(counted.Counts("List"));
            Assert.False(counted.Counts("TREES"));
            Assert.False(counted.Counts("Demolished"));
            Assert.False(counted.Counts(string.Empty));
            Assert.False(counted.Counts(null));
            Assert.Equal("Tree List - Existing is named for it", counted.Why("Existing"));
            Assert.Equal(CountedGroups.LeftOut, counted.Why("Street Design"));
        }

        /// <summary>
        /// Every template's two sheets count the same two groups, because the sheet names are
        /// the same on all seven. Measured on the map, not on a workbook. Street Design counts
        /// on STREETS alone, by decision, which is in its own test file.
        /// </summary>
        [Fact]
        public void EveryTemplateCountsExistingAndProposedAndOnlyStreetsCountsMore()
        {
            foreach (KpiTemplate template in KpiTemplates.All)
            {
                CountedGroups counted = CountedGroups.Of(template);
                Assert.True(counted.Counts("Existing"), template.Name);
                Assert.True(counted.Counts("Proposed"), template.Name);
                Assert.Equal(template == KpiTemplates.Streets, counted.Counts("Street Design"));
            }
        }

        /// <summary>
        /// A schedule with no group row at all, species straight under the heading row, reads
        /// every species with no group, the way it did before this round.
        /// </summary>
        [Fact]
        public void AScheduleWithNoGroupRowReadsEverySpeciesWithNoGroup()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "EP-05",
                Headings,
                Species("ALBIZIA LEBBECK", 5),
                Species("CASSIA GLAUCA", 2),
                new[] { "TOTAL", "", "", "7" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, CreateFixture.Counted, "EP-05");

            Assert.Equal(2, reading.Species.Count);
            Assert.All(reading.Species, one => Assert.False(one.HasGroup));
            Assert.Empty(reading.Groups);
            Assert.Equal(7, reading.SpeciesSum);
        }
    }
}
