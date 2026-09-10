using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class ScheduleRowsTests
    {
        private static readonly string[] Phases = { "Existing", "Proposed" };

        /// <summary>
        /// DM-12 as the 1355 run printed it. TREES, then a group row per phase, then the
        /// species under it, then a subtotal, then TOTAL.
        /// </summary>
        private static ScannedSchedule TheRealDm12()
        {
            return CreateFixture.Softscape(
                "DM-12",
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "TREES", "" },
                new[] { "Existing", "" },
                new[] { "ACACIA / VACHELLIA FARNESIANA", "1" },
                new[] { "ALBIZIA LEBBECK", "1" },
                new[] { "PHOENIX DACTYLIFERA", "5" },
                new[] { "UNKNOWN", "2" },
                new[] { "WASHINGTONIA ROBUSTA", "1" },
                new[] { "", "10" },
                new[] { "Proposed", "" },
                new[] { "ALBIZIA LEBBECK", "13" },
                new[] { "BAUHINIA PURPUREA", "6" },
                new[] { "CASSIA GLAUCA", "10" });
        }

        [Fact]
        public void EverySpeciesRowComesBackWithTheGroupItSatUnder()
        {
            IReadOnlyList<SpeciesRow> species =
                SoftscapeRows.SpeciesIn(TheRealDm12(), Phases, "DM-12");

            Assert.Equal(8, species.Count);
            Assert.Equal(
                new[]
                {
                    "ACACIA / VACHELLIA FARNESIANA", "ALBIZIA LEBBECK", "PHOENIX DACTYLIFERA",
                    "UNKNOWN", "WASHINGTONIA ROBUSTA", "ALBIZIA LEBBECK", "BAUHINIA PURPUREA",
                    "CASSIA GLAUCA"
                },
                species.Select(one => one.BotanicalName));
            Assert.Equal(
                new[]
                {
                    "Existing", "Existing", "Existing", "Existing", "Existing",
                    "Proposed", "Proposed", "Proposed"
                },
                species.Select(one => one.GroupName));
            Assert.Equal(new[] { 1, 1, 5, 2, 1, 13, 6, 10 }, species.Select(one => one.Quantity));
            Assert.All(species, one => Assert.Equal("DM-12", one.PlotId));
        }

        /// <summary>
        /// The category row holds one cell and no number, the headings row holds two cells and
        /// its second is COUNT (n), and a subtotal holds a number and no name. None is a
        /// species and none reaches the tree lists.
        /// </summary>
        [Fact]
        public void TheHeadingsRowTheCategoryRowAndTheSubtotalAreNoneOfThemASpecies()
        {
            IReadOnlyList<SpeciesRow> species =
                SoftscapeRows.SpeciesIn(TheRealDm12(), Phases, "DM-12");

            Assert.DoesNotContain(species, one => one.BotanicalName == "BOTANICAL NAME");
            Assert.DoesNotContain(species, one => one.BotanicalName == "TREES");
            Assert.DoesNotContain(species, one => one.Quantity == 10 && one.BotanicalName.Length == 0);
        }

        [Fact]
        public void ASpeciesRowAboveEveryGroupRowComesBackWithNoGroup()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "CASSIA GLAUCA", "4" },
                new[] { "Proposed", "" },
                new[] { "ALBIZIA LEBBECK", "13" });

            IReadOnlyList<SpeciesRow> species = SoftscapeRows.SpeciesIn(schedule, Phases, "DM-12");

            Assert.Equal(2, species.Count);
            Assert.Equal(string.Empty, species[0].GroupName);
            Assert.False(species[0].HasGroup);
            Assert.Equal("Proposed", species[1].GroupName);
        }

        [Fact]
        public void AScheduleWhoseRowsWereNotReadGivesNoSpecies()
        {
            ScannedSchedule refused = KpiFixture.Schedule("DM-12-(600) SOFTSCAPE SCHEDULE", rowsRefused: true);

            Assert.Empty(SoftscapeRows.SpeciesIn(refused, Phases, "DM-12"));
        }

        /// <summary>
        /// DM-11-(600) SHRUBS &amp; LAWN SCHEDULE exactly as the 1355 scan printed it, eleven
        /// columns wide. That report is not in this repository, because nothing under reports/
        /// is ever committed, so these rows are the record of the shape.
        ///
        /// An empty cell is an empty string here. The report renders one as a dash, which is
        /// how these rows were transcribed, and the dash is not in the data.
        /// </summary>
        private static ScannedSchedule TheRealDm11()
        {
            return CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)",
                        "HEIGHT (m)", "SPREAD (m)", "WATER DEMAND", "WATER L/SQM/DAY", "L/DAY" },
                new[] { "GRASS", "", "", "", "", "", "", "", "", "", "" },
                new[] { "Proposed", "", "", "", "", "", "", "", "", "", "" },
                new[] { "Pennisetum Setaceum.jpg", "PEN SET", "M-329313-063",
                        "GRASS: PENNISETUM SETACEUM", "35 m\u00b2", "46", "1", "1", "MEDIUM", "12", "432" },
                new[] { "", "", "", "", "35 m\u00b2", "46", "", "", "", "", "432" },
                new[] { "", "", "", "", "35 m\u00b2", "46", "", "", "", "", "432" },
                new[] { "SHRUBS & GROUND COVER", "", "", "", "", "", "", "", "", "", "" },
                new[] { "Proposed", "", "", "", "", "", "", "", "", "", "" },
                new[] { "Bougainvillea glabra Pink Pixie.jpg", "BOU PLL", "M-329333-014",
                        "SHRUBS: BOUGAINVILLEA GLABRA 'PINK PIXIE'", "36 m\u00b2", "46", "1.5", "1",
                        "LOW", "12", "432" },
                new[] { "Carissa macrocarpa - grandiflora.jpg", "CAR GRA", "M-329333-024",
                        "SHRUBS: CARISSA MACROCARPA / GRANDIFLORA", "34 m\u00b2", "12", "1.5", "1.5",
                        "MEDIUM", "14", "476" },
                new[] { "", "", "", "", "70 m\u00b2", "58", "", "", "", "", "908" },
                new[] { "", "", "", "", "70 m\u00b2", "58", "", "", "", "", "908" },
                new[] { "TOTAL", "", "", "", "105 m\u00b2", "104", "", "", "", "", "1340" });
        }

        private static IReadOnlyList<GroupSubtotal> SubtotalsOfDm11()
        {
            return ShrubsAndLawnRows.SubtotalsIn(
                TheRealDm11(), new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading });
        }

        /// <summary>
        /// The group heading sits on its own row with every other cell empty, and a phase row
        /// sits under it. Neither carries the numbers, which is why reading them off the
        /// heading row found nothing at all.
        /// </summary>
        [Fact]
        public void BothWantedGroupsComeBackFromTheRealRows()
        {
            IReadOnlyList<GroupSubtotal> subtotals = SubtotalsOfDm11();

            Assert.Equal(2, subtotals.Count);
            Assert.Equal("GRASS", subtotals[0].Heading);
            Assert.Equal(35.0, subtotals[0].SquareMetres);
            Assert.Equal(46, subtotals[0].ItemCount);
            Assert.Equal("SHRUBS & GROUND COVER", subtotals[1].Heading);
            Assert.Equal(70.0, subtotals[1].SquareMetres);
            Assert.Equal(58, subtotals[1].ItemCount);
        }

        /// <summary>
        /// DM-11's groups each hold ONE PHASE, so each prints its phase subtotal and then the
        /// group total, two equal rows, 35 then 35 and 70 then 70. That is what was read as one
        /// subtotal printed twice, and it is why the old rule was right on this plot and wrong
        /// on the 20 of the 0928 run. The last row is taken and here it is the same number.
        /// </summary>
        [Fact]
        public void Dm11sGroupsEachHoldOnePhaseSoEachPrintsTwoEqualRows()
        {
            IReadOnlyList<GroupSubtotal> subtotals = SubtotalsOfDm11();

            Assert.Equal(2, subtotals[0].Repeats);
            Assert.Equal(2, subtotals[1].Repeats);
            Assert.Equal(35.0, subtotals[0].SquareMetres);
            Assert.Equal(70.0, subtotals[1].SquareMetres);
            Assert.All(subtotals, one => Assert.True(one.Agrees));
        }

        /// <summary>
        /// 36 plus 34 is 70, so the subtotal can be held against the species rows it came from.
        /// </summary>
        [Fact]
        public void TheSpeciesRowsOfAGroupAddUpToItsSubtotal()
        {
            IReadOnlyList<GroupSubtotal> subtotals = SubtotalsOfDm11();

            Assert.Equal(35.0, subtotals[0].SpeciesSum);
            Assert.Equal(70.0, subtotals[1].SpeciesSum);
        }

        /// <summary>
        /// TOTAL carries numbers, so it is not a structure row, and its first cell holds text,
        /// so it is not a subtotal. It needs no special case and 105 reaches nothing.
        /// </summary>
        [Fact]
        public void TheTotalRowIsNeitherAGroupNorASubtotal()
        {
            IReadOnlyList<GroupSubtotal> subtotals = SubtotalsOfDm11();

            Assert.DoesNotContain(subtotals, one => one.Heading == "TOTAL");
            Assert.DoesNotContain(subtotals, one => one.SquareMetres == 105.0);
        }

        /// <summary>
        /// The phase row under a group heading is the same shape as the heading, so it must not
        /// open a group of its own.
        /// </summary>
        [Fact]
        public void ThePhaseRowUnderAGroupHeadingOpensNoGroup()
        {
            Assert.DoesNotContain(SubtotalsOfDm11(), one => one.Heading == "Proposed");
        }

        [Fact]
        public void AGroupTotalThatDoesNotEqualTheRowsAboveItIsNamedRatherThanChosenBetween()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "a.jpg", "GRASS: PENNISETUM SETACEUM", "35 m\u00b2", "46" },
                new[] { "", "", "35 m\u00b2", "46" },
                new[] { "", "", "37 m\u00b2", "46" });

            GroupSubtotal only = Assert.Single(ShrubsAndLawnRows.SubtotalsIn(
                schedule, new[] { KpiMerge.LawnHeading }));

            Assert.False(only.Agrees);
            Assert.Contains("its group total reads 37 over 46", only.Disagreement);
            Assert.Contains("1 row above it add to 35 over 46", only.Disagreement);
        }

        /// <summary>
        /// The hardscape schedule prints 0 m2, which is the number nought rather than a cell
        /// holding nothing, so a group whose area is really zero comes back as zero.
        /// </summary>
        /// <summary>
        /// The real softscape shape, from the 1521 scan. An existing species prints with no
        /// photo, so its image cell is empty, and a proposed one prints with a file name there.
        ///
        /// Counting what sits under a group off the first cell read the image column, so
        /// DM-12 Existing came back as 0 named rows of 6 while section 6 of the same file
        /// printed its five species.
        /// </summary>
        private static ScannedSchedule TheRealDm12WithImages()
        {
            return CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "PLANT CODE", "BOQ CODE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "TREES", "", "", "", "" },
                new[] { "Existing", "", "", "", "" },
                new[] { "", "ACA FAR", "NO BOQ CODE AVAILABLE", "ACACIA / VACHELLIA FARNESIANA", "1" },
                new[] { "", "ALB LEB", "NO BOQ CODE AVAILABLE", "ALBIZIA LEBBECK", "1" },
                new[] { "", "PHO DAC", "NO BOQ CODE AVAILABLE", "PHOENIX DACTYLIFERA", "5" },
                new[] { "", "UNK", "NO BOQ CODE AVAILABLE", "UNKNOWN", "2" },
                new[] { "", "WAS ROB", "NO BOQ CODE AVAILABLE", "WASHINGTONIA ROBUSTA", "1" },
                new[] { "", "", "", "", "10" },
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "13" },
                new[] { "Bauhinia purpurea.jpg", "BAU PUR", "M-329343-B22", "BAUHINIA PURPUREA", "6" },
                new[] { "Cassia glauca.jpg", "CAS GLA", "M-329343-C09", "CASSIA GLAUCA", "10" },
                new[] { "TOTAL", "", "", "", "39" });
        }

        /// <summary>
        /// Measured: DM-12 Existing has 5 species and Proposed 3. Existing prints 6 rows under
        /// it, the five species and a subtotal.
        /// </summary>
        [Fact]
        public void AGroupCountsItsSpeciesOffTheBotanicalColumnAndNotTheImageColumn()
        {
            IReadOnlyList<ScheduleGroup> groups =
                ScheduleGroups.Of(TheRealDm12WithImages(), Phases);

            Assert.Equal(2, groups.Count);
            Assert.Equal("Existing", groups[0].Name);
            Assert.Equal(6, groups[0].RowsUnder);
            Assert.Equal(5, groups[0].NamedRowsUnder);
            Assert.Equal("Proposed", groups[1].Name);
            Assert.Equal(3, groups[1].NamedRowsUnder);
        }

        /// <summary>
        /// Measured: DM-13 Existing has 1 species and Proposed 2, and Existing prints 2 rows
        /// under it, the species and a subtotal.
        /// </summary>
        [Fact]
        public void TheMeasuredDm13CountsComeBackAsOneAndTwo()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-13",
                new[] { "IMAGE", "PLANT CODE", "BOQ CODE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "TREES", "", "", "", "" },
                new[] { "Existing", "", "", "", "" },
                new[] { "", "ALB LEB", "NO BOQ CODE AVAILABLE", "ALBIZIA LEBBECK", "5" },
                new[] { "", "", "", "", "5" },
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "5" },
                new[] { "Cassia glauca.jpg", "CAS GLA", "M-329343-C09", "CASSIA GLAUCA", "4" });

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, Phases);

            Assert.Equal(2, groups[0].RowsUnder);
            Assert.Equal(1, groups[0].NamedRowsUnder);
            Assert.Equal(2, groups[1].NamedRowsUnder);
        }

        /// <summary>
        /// Measured: DM-11 Proposed has 3 species.
        /// </summary>
        [Fact]
        public void TheMeasuredDm11ProposedCountComesBackAsThree()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                new[] { "IMAGE", "PLANT CODE", "BOQ CODE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "TREES", "", "", "", "" },
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "6" },
                new[] { "Bauhinia purpurea.jpg", "BAU PUR", "M-329343-B22", "BAUHINIA PURPUREA", "2" },
                new[] { "Cassia glauca.jpg", "CAS GLA", "M-329343-C09", "CASSIA GLAUCA", "4" },
                new[] { "", "", "", "", "12" });

            ScheduleGroup only = Assert.Single(ScheduleGroups.Of(schedule, Phases));

            Assert.Equal("Proposed", only.Name);
            Assert.Equal(3, only.NamedRowsUnder);
            Assert.Equal(4, only.RowsUnder);
        }

        /// <summary>
        /// The species reader and the group counter have to agree about what a species is, so
        /// the same rows are read both ways and held against each other.
        /// </summary>
        [Fact]
        public void TheSpeciesReaderAndTheGroupCounterAgreeOnTheSameRows()
        {
            ScannedSchedule schedule = TheRealDm12WithImages();

            IReadOnlyList<SpeciesRow> species = SoftscapeRows.SpeciesIn(schedule, Phases, "DM-12");
            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, Phases);

            Assert.Equal(
                groups[0].NamedRowsUnder, species.Count(one => one.GroupName == "Existing"));
            Assert.Equal(
                groups[1].NamedRowsUnder, species.Count(one => one.GroupName == "Proposed"));
            Assert.Equal(10, species.Where(one => one.GroupName == "Existing").Sum(one => one.Quantity));
        }

        /// <summary>
        /// The same fault in the shrubs reader, which has not been seen because DM-11's shrubs
        /// are all Proposed. An existing shrub prints with no photo, so telling a subtotal from
        /// a species by the image cell would count the species as a subtotal.
        /// </summary>
        [Fact]
        public void AnExistingShrubWithNoPhotoIsASpeciesRatherThanASubtotal()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "Existing", "", "", "" },
                new[] { "", "GRASS: PENNISETUM SETACEUM", "20 m\u00b2", "30" },
                new[] { "", "", "35 m\u00b2", "46" },
                new[] { "", "", "35 m\u00b2", "46" });

            GroupSubtotal only = Assert.Single(ShrubsAndLawnRows.SubtotalsIn(
                schedule, new[] { KpiMerge.LawnHeading }));

            Assert.Equal(35.0, only.SquareMetres);
            Assert.Equal(2, only.Repeats);
            Assert.True(only.Agrees);
            Assert.Equal(20.0, only.SpeciesSum);
        }

        [Fact]
        public void AZeroAreaIsANumberAndNotAnEmptyCell()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "", "", "0 m\u00b2", "0" });

            GroupSubtotal only = Assert.Single(ShrubsAndLawnRows.SubtotalsIn(
                schedule, new[] { KpiMerge.LawnHeading }));

            Assert.Equal(0.0, only.SquareMetres);
            Assert.Equal(0, only.ItemCount);
            Assert.Equal(1, only.Repeats);
        }

        /// <summary>
        /// Eleven columns wide, so the first number in the row is the area and the last is
        /// L/DAY. Neither position can be assumed and the heading row decides.
        /// </summary>
        [Fact]
        public void TheAreaAndTheCountComeOffTheColumnsTheHeadingRowNames()
        {
            var headings = new[] { "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "AREA  (sqm)",
                                   "COUNT (n)", "HEIGHT (m)", "SPREAD (m)", "WATER DEMAND",
                                   "WATER L/SQM/DAY", "L/DAY" };

            Assert.Equal(4, ScheduleColumns.Holding(headings, ScheduleColumns.AreaWord));
            Assert.Equal(5, ScheduleColumns.Holding(headings, ScheduleColumns.CountWord));
            Assert.Equal(3, ScheduleColumns.Holding(headings, ScheduleColumns.BotanicalWord));
            Assert.Equal(-1, ScheduleColumns.Holding(headings, "VOLUME"));
        }

        [Fact]
        public void TheNumberIsReadAsFarAsItGoesAndTheUnitIsNotStripped()
        {
            double value;

            Assert.True(CellNumber.In("35 m2", out value));
            Assert.Equal(35.0, value);

            Assert.True(CellNumber.In("1131.72 m2", out value));
            Assert.Equal(1131.72, value);

            Assert.True(CellNumber.In("0 m\u00b2", out value));
            Assert.Equal(0.0, value);

            Assert.False(CellNumber.In("COUNT (n)", out value));
            Assert.False(CellNumber.In("", out value));
            Assert.False(CellNumber.In("m2", out value));
        }
    }
}
