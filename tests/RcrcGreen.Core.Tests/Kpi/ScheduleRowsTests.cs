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
        /// DM-11 gives GRASS 35 m2 46, then SHRUBS and GROUND COVER 70 m2 58, then TOTAL. Only
        /// the two headings the workbook asks for are looked for, so the schedule's own TOTAL
        /// never has to be told apart from a group.
        /// </summary>
        [Fact]
        public void TheTwoWantedGroupSubtotalsComeBackAndTheTotalDoesNot()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "TYPE", "AREA", "COUNT" },
                new[] { "GRASS", "35 m2", "46" },
                new[] { "SHRUBS & GROUND COVER", "70 m2", "58" },
                new[] { "TOTAL", "105 m2", "104" });

            IReadOnlyList<GroupSubtotal> subtotals = ShrubsAndLawnRows.SubtotalsIn(
                schedule, new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading });

            Assert.Equal(2, subtotals.Count);
            Assert.Equal("GRASS", subtotals[0].Heading);
            Assert.Equal(35.0, subtotals[0].SquareMetres);
            Assert.Equal(46, subtotals[0].ItemCount);
            Assert.Equal("SHRUBS & GROUND COVER", subtotals[1].Heading);
            Assert.Equal(70.0, subtotals[1].SquareMetres);
            Assert.Equal(58, subtotals[1].ItemCount);
            Assert.DoesNotContain(subtotals, one => one.Heading == "TOTAL");
        }

        [Fact]
        public void AGroupHeadingSpeltInADifferentCaseIsStillFound()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "TYPE", "AREA", "COUNT" },
                new[] { "grass", "35 m2", "46" });

            GroupSubtotal only = Assert.Single(ShrubsAndLawnRows.SubtotalsIn(
                schedule, new[] { KpiMerge.LawnHeading }));

            Assert.Equal("grass", only.Heading);
            Assert.Equal(35.0, only.SquareMetres);
        }

        [Fact]
        public void TheNumberIsReadAsFarAsItGoesAndTheUnitIsNotStripped()
        {
            double value;

            Assert.True(CellNumber.In("35 m2", out value));
            Assert.Equal(35.0, value);

            Assert.True(CellNumber.In("1131.72 m2", out value));
            Assert.Equal(1131.72, value);

            Assert.False(CellNumber.In("COUNT (n)", out value));
            Assert.False(CellNumber.In("", out value));
            Assert.False(CellNumber.In("m2", out value));
        }
    }
}
