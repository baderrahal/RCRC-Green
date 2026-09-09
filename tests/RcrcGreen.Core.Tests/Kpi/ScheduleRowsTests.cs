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
        /// The subtotal prints twice, 35 then 35 and 70 then 70. Adding a group's subtotal rows
        /// would give 70 and 140.
        /// </summary>
        [Fact]
        public void TheSubtotalPrintsTwiceAndOnlyOneIsTaken()
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
        public void TwoSubtotalRowsThatDisagreeAreNamedRatherThanChosenBetween()
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
            Assert.Contains("2 subtotal rows disagree", only.Disagreement);
            Assert.Contains("35 over 46", only.Disagreement);
            Assert.Contains("37 over 46", only.Disagreement);
        }

        /// <summary>
        /// The hardscape schedule prints 0 m2, which is the number nought rather than a cell
        /// holding nothing, so a group whose area is really zero comes back as zero.
        /// </summary>
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
