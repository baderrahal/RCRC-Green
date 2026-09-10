using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 2 of the first audit, the open BLOCKS. **A reader that cannot find its column
    /// says so rather than falling back to a position.** Cell 0 is the image column, so a name
    /// read off it is a file name, and the last number in an eleven column row is L/DAY rather
    /// than a count. Both are plausible, so a workbook filled that way looks finished.
    ///
    /// Every expected string is written out by hand.
    /// </summary>
    public class ColumnRefusalTests
    {
        private static readonly string[] Phases = { "Existing", "Proposed" };

        [Fact]
        public void ASoftscapeScheduleNamingNoBotanicalColumnIsRefusedRatherThanReadOffTheImageCell()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "PLANT CODE", "COUNT (n)" },
                new[] { "Existing", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "13" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-12");

            Assert.False(reading.WasRead);
            Assert.Empty(reading.Species);
            Assert.Equal(
                "the heading row names no column holding BOTANIC. Its headings: IMAGE | PLANT CODE | COUNT (n)",
                Assert.Single(reading.Refusals));
        }

        /// <summary>
        /// The last number in the row is L/DAY, 432, which counts nothing.
        /// </summary>
        [Fact]
        public void ASoftscapeScheduleNamingNoCountColumnIsRefusedRatherThanTakingTheLastNumber()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "BOTANICAL NAME", "HEIGHT (m)", "L/DAY" },
                new[] { "Proposed", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "1.5", "432" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-12");

            Assert.False(reading.WasRead);
            Assert.Empty(reading.Species);
            Assert.Equal(
                "the heading row names no column holding COUNT. Its headings: IMAGE | BOTANICAL NAME | HEIGHT (m) | L/DAY",
                Assert.Single(reading.Refusals));
        }

        [Fact]
        public void BothMissingColumnsAreBothNamed()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "PLANT CODE" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-12");

            Assert.Equal(2, reading.Refusals.Count);
            Assert.Equal(
                "the heading row names no column holding BOTANIC. Its headings: IMAGE | PLANT CODE",
                reading.Refusals[0]);
            Assert.Equal(
                "the heading row names no column holding COUNT. Its headings: IMAGE | PLANT CODE",
                reading.Refusals[1]);
        }

        /// <summary>
        /// Told by the first cell, both empty image cells would have made the species row a
        /// subtotal and the group would have read 20 with nothing said.
        /// </summary>
        [Fact]
        public void AShrubsScheduleNamingNoBotanicalColumnIsRefusedRatherThanTellingASpeciesByItsImageCell()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "" },
                new[] { "", "20 m²", "30" },
                new[] { "", "35 m²", "46" });

            ShrubsAndLawnReading reading = ShrubsAndLawnRows.Read(schedule, new[] { KpiMerge.LawnHeading });

            Assert.False(reading.WasRead);
            Assert.Empty(reading.Subtotals);
            Assert.Equal(
                "the heading row names no column holding BOTANIC. Its headings: IMAGE | AREA  (sqm) | COUNT (n)",
                Assert.Single(reading.Refusals));
        }

        /// <summary>
        /// The area used to come back as an empty list with nothing said, which the first audit
        /// called the correct behaviour. It carries the line now, like the other three.
        /// </summary>
        [Fact]
        public void AShrubsScheduleNamingNoAreaColumnSaysSoRatherThanHandingBackNothing()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "GRASS", "", "" },
                new[] { "", "", "46" });

            ShrubsAndLawnReading reading = ShrubsAndLawnRows.Read(schedule, new[] { KpiMerge.LawnHeading });

            Assert.False(reading.WasRead);
            Assert.Equal(
                "the heading row names no column holding AREA. Its headings: IMAGE | BOTANICAL NAME | COUNT (n)",
                Assert.Single(reading.Refusals));
        }

        [Fact]
        public void AShrubsScheduleNamingNoCountColumnIsRefusedRatherThanCountingNought()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)" },
                new[] { "GRASS", "", "" },
                new[] { "", "", "35 m²" });

            ShrubsAndLawnReading reading = ShrubsAndLawnRows.Read(schedule, new[] { KpiMerge.LawnHeading });

            Assert.False(reading.WasRead);
            Assert.Equal(
                "the heading row names no column holding COUNT. Its headings: IMAGE | BOTANICAL NAME | AREA  (sqm)",
                Assert.Single(reading.Refusals));
        }

        /// <summary>
        /// Off the first cell this counted one named row, a file name. The group itself needs
        /// no column and is still found.
        /// </summary>
        [Fact]
        public void AGroupCounterWithNoBotanicalColumnFindsTheGroupAndRefusesToCountOffTheImageCell()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "PLANT CODE", "COUNT (n)" },
                new[] { "Proposed", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "13" },
                new[] { "", "", "13" });

            ScheduleGroup group = Assert.Single(ScheduleGroups.Of(schedule, Phases));

            Assert.Equal("Proposed", group.Name);
            Assert.Equal(2, group.RowsUnder);
            Assert.False(group.NamedRowsCounted);
            Assert.Equal(0, group.NamedRowsUnder);
            Assert.Equal(
                "the heading row names no column holding BOTANIC. Its headings: IMAGE | PLANT CODE | COUNT (n)",
                group.WhyNotCounted);
        }

        [Fact]
        public void AGroupCounterWithTheColumnStillCounts()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Proposed", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "13" },
                new[] { "", "", "13" });

            ScheduleGroup group = Assert.Single(ScheduleGroups.Of(schedule, Phases));

            Assert.True(group.NamedRowsCounted);
            Assert.Equal(1, group.NamedRowsUnder);
            Assert.Equal(string.Empty, group.WhyNotCounted);
        }

        /// <summary>
        /// The refusal reaches the write. It used to be a list of nothing, which the
        /// reconciliation read as a plot whose schedule listed no species.
        /// </summary>
        [Fact]
        public void ARefusedReadRefusesTheWriteAndNamesThePlotAndTheReason()
        {
            PlotReading reading = CreateFixture.Plot(
                "DM-12",
                readRefusals: new[]
                {
                    "DM-12-(600) SOFTSCAPE SCHEDULE: the heading row names no column holding BOTANIC. "
                    + "Its headings: IMAGE | PLANT CODE | COUNT (n)"
                });

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-12" }, new[] { reading }, null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.Equal(
                "DM-12: DM-12-(600) SOFTSCAPE SCHEDULE: the heading row names no column holding BOTANIC. "
                + "Its headings: IMAGE | PLANT CODE | COUNT (n)",
                Assert.Single(held.Refusals));
        }

        /// <summary>
        /// The report carries it twice, once among the reasons the workbook was not written
        /// and once under the plot, and the pane draws the reasons in red above Create.
        /// </summary>
        [Fact]
        public void ARefusedReadPrintsInTheReconciliationAndUnderThePlot()
        {
            KpiCreateRun run = CreateFixture.Run(new[]
            {
                CreateFixture.Plot("DM-12", readRefusals: new[]
                {
                    "DM-12-(600) SOFTSCAPE SCHEDULE: the heading row names no column holding COUNT. "
                    + "Its headings: IMAGE | BOTANICAL NAME"
                })
            });

            string[] lines = KpiCreateReport.Write(run, new DateTime(2026, 9, 10, 9, 28, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.Contains(
                "  DM-12: DM-12-(600) SOFTSCAPE SCHEDULE: the heading row names no column holding COUNT. "
                + "Its headings: IMAGE | BOTANICAL NAME",
                lines);
            Assert.Contains(
                "    REFUSED: DM-12-(600) SOFTSCAPE SCHEDULE: the heading row names no column holding COUNT. "
                + "Its headings: IMAGE | BOTANICAL NAME",
                lines);
            Assert.Contains("== RECONCILIATION (1) ==", lines);
        }

        [Fact]
        public void APlotWhoseReadWasRefusedIsNotSaidToHaveListedNoSpecies()
        {
            PlotReading reading = CreateFixture.Plot(
                "DM-12",
                readRefusals: new[] { "a refusal" },
                regions: new RegionArea[0],
                chosenRegion: string.Empty);

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-12" }, new[] { reading }, null, false, KpiTemplates.Mosques);

            PlotAndReason nothing = Assert.Single(held.ContributedNothing);
            Assert.Equal(
                "a schedule read on it was refused, see above, no filled region holding an area",
                nothing.Reason);
        }
    }

    /// <summary>
    /// Finding 30. **A digit after the number ends is a refusal, never a shorter number.**
    /// 1,234 m2 read as 1, and a one phase group over 999 passed its own add-up check because
    /// 1 equals 1. Which character a project groups digits with is a units setting this tool
    /// has never read, so nothing here parses a separator.
    /// </summary>
    public class CellNumberTests
    {
        /// <summary>
        /// Every printed value measured on the real model, so the ordinary path still passes.
        /// Revit prints the area unit with a superscript two, which is not a digit.
        /// </summary>
        [Theory]
        [InlineData("35 m²", 35.0)]
        [InlineData("70 m²", 70.0)]
        [InlineData("105 m²", 105.0)]
        [InlineData("820 m²", 820.0)]
        [InlineData("1161 m²", 1161.0)]
        [InlineData("3729 m²", 3729.0)]
        [InlineData("1131.72 m²", 1131.72)]
        [InlineData("46", 46.0)]
        [InlineData("1020", 1020.0)]
        [InlineData("0 m²", 0.0)]
        public void EveryValueMeasuredOnTheRealModelStillReads(string cell, double expected)
        {
            CellNumberRead read = CellNumber.Read(cell);

            Assert.True(read.IsNumber);
            Assert.False(read.IsRefused);
            Assert.Equal(expected, read.Value);
        }

        [Fact]
        public void AThousandsSeparatorIsRefusedAndTheCellIsNamed()
        {
            CellNumberRead read = CellNumber.Read("1,234 m²");

            Assert.True(read.IsRefused);
            Assert.False(read.IsNumber);
            Assert.False(read.IsEmpty);
            Assert.Equal(
                "holds '1,234 m²', which carries a digit after the number 1 ends. That is a "
                + "thousands separator or a decimal comma, this tool does not read the project's separator "
                + "setting, and a number read short passes its own checks, so the cell is refused rather "
                + "than read as 1.",
                read.Refusal);
        }

        [Fact]
        public void ADecimalCommaIsRefusedTheSameWay()
        {
            CellNumberRead read = CellNumber.Read("1131,72");

            Assert.True(read.IsRefused);
            Assert.Contains("'1131,72'", read.Refusal);
            Assert.Contains("after the number 1131 ends", read.Refusal);
        }

        [Fact]
        public void ASpaceUsedToGroupDigitsIsRefusedToo()
        {
            Assert.True(CellNumber.Read("1 234").IsRefused);
            Assert.True(CellNumber.Read("12 345,67 m²").IsRefused);
        }

        /// <summary>
        /// A unit spelt with a digit cannot be told from a separator by a reader that does not
        /// parse separators, and Revit never prints one, so it is refused with the cell named.
        /// </summary>
        [Fact]
        public void AUnitSpeltWithADigitIsRefusedBecauseNothingCanTellItFromASeparator()
        {
            CellNumberRead read = CellNumber.Read("35 m2");

            Assert.True(read.IsRefused);
            Assert.Contains("'35 m2'", read.Refusal);
        }

        [Fact]
        public void ADecimalPointIsANumberAndNotAWholeOne()
        {
            CellNumberRead fraction = CellNumber.Read("1131.72");
            Assert.True(fraction.IsNumber);
            Assert.False(fraction.IsWhole);

            CellNumberRead whole = CellNumber.Read("13");
            Assert.True(whole.IsWhole);
            Assert.Equal(13, whole.Whole);

            Assert.True(CellNumber.Read("13.0").IsWhole);
            Assert.False(CellNumber.Read("-1").IsWhole);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("COUNT (n)")]
        [InlineData("m²")]
        [InlineData("-")]
        public void ACellWithNoNumberAtTheFrontIsEmptyAndNotRefused(string cell)
        {
            CellNumberRead read = CellNumber.Read(cell);

            Assert.True(read.IsEmpty);
            Assert.False(read.IsRefused);
            Assert.False(read.IsNumber);
        }

        /// <summary>
        /// The case that defeated the check. 1,200 plus 1,300 is 2,500, and read short it was
        /// 1 plus 1 equals 2, which passes. The group is refused with the first bad cell named.
        /// </summary>
        [Fact]
        public void AGroupOverAThousandPrintedWithSeparatorsIsRefusedRatherThanAddingOneAndOne()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "EP-05",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "SHRUBS & GROUND COVER", "", "", "" },
                new[] { "Existing", "", "", "" },
                new[] { "", "SHRUBS: ACACIA", "1,200 m²", "1,500" },
                new[] { "", "", "1,200 m²", "1,500" },
                new[] { "Proposed", "", "", "" },
                new[] { "a.jpg", "SHRUBS: CARISSA", "1,300 m²", "1,600" },
                new[] { "", "", "1,300 m²", "1,600" },
                new[] { "", "", "2,500 m²", "3,100" });

            ShrubsAndLawnReading reading = ShrubsAndLawnRows.Read(schedule, new[] { KpiMerge.ShrubsHeading });

            Assert.False(reading.WasRead);
            Assert.Empty(reading.Subtotals);
            Assert.StartsWith("row 4: its AREA  (sqm) cell holds '1,200 m²'", reading.Refusals[0]);
            // One refusal per numeric row, the area cell first: two species rows, two phase
            // subtotals and the group total.
            Assert.Equal(5, reading.Refusals.Count);
        }

        [Fact]
        public void ASpeciesCountWithASeparatorIsRefusedRatherThanReadAsOne()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "EP-05",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Proposed", "", "" },
                new[] { "a.jpg", "ALBIZIA LEBBECK", "1,013" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, new[] { "Existing", "Proposed" }, "EP-05");

            Assert.False(reading.WasRead);
            Assert.Empty(reading.Species);
            Assert.StartsWith(
                "row 3, ALBIZIA LEBBECK: its COUNT (n) cell holds '1,013'",
                Assert.Single(reading.Refusals));
        }
    }

    /// <summary>
    /// Finding 31. **STREETS has no area cell, so on STREETS the area is not read and nothing
    /// about the area is refused on.** MM-03 and MM-04 are street plots reading one raw area,
    /// and the first 78 plot run would have ended asking the user to confirm an area the
    /// workbook has no cell for, then read all 78 again.
    /// </summary>
    public class StreetsAreaTests
    {
        private static PlotReading[] TwoStreetPlotsReadingOneArea()
        {
            return new[]
            {
                CreateFixture.Plot("MM-03", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                }),
                CreateFixture.Plot("MM-04", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                })
            };
        }

        [Fact]
        public void OnStreetsTwoPlotsReadingOneAreaDoNotRefuse()
        {
            Reconciliation held = Reconciliation.Of(
                new[] { "MM-03", "MM-04" }, TwoStreetPlotsReadingOneArea(), null, false, KpiTemplates.Streets);

            Assert.True(held.AddsUp);
            Assert.False(held.AreaWanted);
            Assert.Empty(held.IdenticalAreas);
            Assert.Empty(held.WithoutArea);
        }

        /// <summary>
        /// The same two plots on a template with an area cell still refuse, so the guard is
        /// where it was and only STREETS steps round it.
        /// </summary>
        [Fact]
        public void OnMosquesTheSameTwoPlotsStillRefuse()
        {
            Reconciliation held = Reconciliation.Of(
                new[] { "MM-03", "MM-04" }, TwoStreetPlotsReadingOneArea(), null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.True(held.AreaWanted);
            Assert.Contains(held.Refusals, one => one.Contains("report the same area"));
        }

        [Fact]
        public void OnStreetsTwoRegionsHoldingAnAreaAskNothing()
        {
            PlotReading reading = CreateFixture.Plot(
                "NS-19",
                regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                    CreateFixture.Region(CreateFixture.OutOfScope, 250.0)
                },
                chosenRegion: string.Empty);

            Assert.True(Reconciliation.Of(new[] { "NS-19" }, new[] { reading }, null, false, KpiTemplates.Streets).AddsUp);
            Assert.False(Reconciliation.Of(new[] { "NS-19" }, new[] { reading }, null, false, KpiTemplates.Mosques).AddsUp);
        }

        [Fact]
        public void OnStreetsAPlotWithNothingElseIsNotBlamedForTheArea()
        {
            PlotReading reading = CreateFixture.Plot(
                "NS-19",
                softscapeRead: false,
                shrubsAndLawnRead: false,
                regions: new RegionArea[0],
                chosenRegion: string.Empty);

            Reconciliation held = Reconciliation.Of(
                new[] { "NS-19" }, new[] { reading }, null, false, KpiTemplates.Streets);

            PlotAndReason nothing = Assert.Single(held.ContributedNothing);
            Assert.Equal("no softscape schedule, no shrubs and lawn schedule", nothing.Reason);
        }

        [Fact]
        public void TheReportSaysTheAreaWasNotReadAndWhy()
        {
            KpiCreateRun run = CreateFixture.Run(
                new[] { CreateFixture.Plot("MM-03", regions: new RegionArea[0], chosenRegion: string.Empty) },
                template: KpiTemplates.Streets);

            string[] lines = KpiCreateReport.Write(run, new DateTime(2026, 9, 10, 9, 28, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            const string why = "not read. This template takes no area. The road width and the total length "
                + "are typed by hand. No filled region was read for any plot and nothing about the area "
                + "was refused on.";

            Assert.Contains("  plots with an area   " + why, lines);
            Assert.Contains("  AREA, SQUARE METRES, " + why, lines);
            Assert.Contains("    area: not read, the template takes none", lines);
            Assert.Contains("  WHICH REGION EACH PLOT'S AREA CAME OFF: " + why, lines);
            Assert.DoesNotContain(lines, line => line.StartsWith("  MM-03 | ", StringComparison.Ordinal));
            Assert.DoesNotContain(lines, line => line.StartsWith("  AREA, SQUARE METRES, 1 plot", StringComparison.Ordinal));

            // The schedule claim still holds on STREETS and the area paragraph is about a read
            // that did not happen, so the first prints and the second does not.
            Assert.Contains("  The shrubs, the lawn and every tree quantity came off a row the schedule", lines);
            Assert.DoesNotContain(lines, line => line.StartsWith("  THE AREA IS NOT A SCHEDULE ROW", StringComparison.Ordinal));
        }

        [Fact]
        public void OnMosquesTheReportStillPrintsTheAreaWorking()
        {
            string[] lines = KpiCreateReport.Write(CreateFixture.Run(), new DateTime(2026, 9, 10, 9, 28, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.Contains("  AREA, SQUARE METRES, 1 plot", lines);
            Assert.Contains("  plots with an area   1", lines);
        }
    }

    /// <summary>
    /// Finding 32. **The TOTAL row is read and the species rows are held against it.** The
    /// first real workbook read 31 trees where the model held 39 and nothing in the tool said
    /// so. DM-12 as the 1521 scan printed it: five existing, three proposed, adding to 39,
    /// TOTAL 39.
    /// </summary>
    public class SoftscapeTotalTests
    {
        private static readonly string[] Phases = { "Existing", "Proposed" };

        private static readonly string[] Headings =
            { "IMAGE", "PLANT CODE", "BOQ CODE", "BOTANICAL NAME", "COUNT (n)" };

        private static ScannedSchedule Dm12(string total)
        {
            return CreateFixture.Softscape(
                "DM-12",
                Headings,
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
                new[] { "TOTAL", "", "", "", total });
        }

        /// <summary>
        /// 1 plus 1 plus 5 plus 2 plus 1 is 10 existing, 13 plus 6 plus 10 is 29 proposed, 39 in
        /// all, and the schedule prints TOTAL 39. One row, the existing subtotal 10, carries a
        /// count and no name and is passed over rather than dropped.
        /// </summary>
        [Fact]
        public void Dm12SpeciesRowsAddToThirtyNineAndTheTotalRowPrintsThirtyNine()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Dm12("39"), Phases, "DM-12");

            Assert.True(reading.WasRead);
            Assert.Equal(8, reading.Species.Count);
            Assert.Equal(39, reading.SpeciesSum);
            Assert.True(reading.TotalRead);
            Assert.Equal(39, reading.Total);
            Assert.Equal(1, reading.RowsPassedOver);
            Assert.DoesNotContain(reading.Species, one => one.BotanicalName == "TOTAL");
        }

        [Fact]
        public void SpeciesRowsThatAddToTheTotalWriteAndAreSaidInTheReport()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Dm12("39"), Phases, "DM-12");
            PlotReading plot = CreateFixture.Plot(
                "DM-12",
                species: reading.Species.ToArray(),
                softscapeTotalRead: reading.TotalRead,
                softscapeTotal: reading.Total,
                rowsPassedOver: reading.RowsPassedOver);

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-12" }, new[] { plot }, null, false, KpiTemplates.Mosques);
            Assert.True(held.AddsUp);

            string[] lines = KpiCreateReport.Write(CreateFixture.Run(new[] { plot }), new DateTime(2026, 9, 10, 9, 28, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.Contains("      species rows add to 39, the TOTAL row prints 39", lines);
            Assert.Contains("      1 row with a count and no botanical name passed over, the subtotals", lines);
        }

        /// <summary>
        /// The first real workbook: three rows short, 31 where the schedule prints 39. Written
        /// out by hand: 1 plus 1 plus 13 plus 6 plus 10 is 31.
        /// </summary>
        [Fact]
        public void SpeciesRowsShortOfTheTotalRefuseTheWriteAndNameBothNumbers()
        {
            PlotReading plot = CreateFixture.Plot(
                "DM-12",
                species: new[]
                {
                    CreateFixture.Species("ACACIA / VACHELLIA FARNESIANA", CreateFixture.Existing, 1),
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Existing, 1),
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13),
                    CreateFixture.Species("BAUHINIA PURPUREA", CreateFixture.Proposed, 6),
                    CreateFixture.Species("CASSIA GLAUCA", CreateFixture.Proposed, 10)
                },
                softscapeTotalRead: true,
                softscapeTotal: 39);

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-12" }, new[] { plot }, null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.Equal(
                "DM-12: its species rows add to 31 and its softscape schedule prints TOTAL 39.",
                Assert.Single(held.Refusals));
        }

        /// <summary>
        /// The reader reads what the schedule printed and the reconciliation compares, so a
        /// schedule printing TOTAL 39 over rows adding to 31 comes back read and is refused a
        /// step later.
        /// </summary>
        [Fact]
        public void TheReaderHandsBackBothNumbersAndTheReconciliationRefuses()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                Headings,
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "13" },
                new[] { "Cassia glauca.jpg", "CAS GLA", "M-329343-C09", "CASSIA GLAUCA", "10" },
                new[] { "TOTAL", "", "", "", "39" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-12");

            Assert.True(reading.WasRead);
            Assert.Equal(23, reading.SpeciesSum);
            Assert.Equal(39, reading.Total);

            PlotReading plot = CreateFixture.Plot(
                "DM-12",
                species: reading.Species.ToArray(),
                softscapeTotalRead: reading.TotalRead,
                softscapeTotal: reading.Total);

            Assert.Contains(
                "DM-12: its species rows add to 23 and its softscape schedule prints TOTAL 39.",
                Reconciliation.Of(new[] { "DM-12" }, new[] { plot }, null, false, KpiTemplates.Mosques).Refusals);
        }

        /// <summary>
        /// A schedule printing no TOTAL row has nothing to hold the sum against. That is said
        /// rather than refused, because a check with no subject is not a failure of the
        /// schedule, and it is said so nobody reads the silence as a check that passed.
        /// </summary>
        [Fact]
        public void NoTotalRowIsSaidRatherThanRefused()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                Headings,
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "6" },
                new[] { "", "", "", "", "6" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-11");

            Assert.True(reading.WasRead);
            Assert.False(reading.TotalRead);
            Assert.Equal(6, reading.SpeciesSum);

            PlotReading plot = CreateFixture.Plot("DM-11", species: reading.Species.ToArray());
            Assert.True(Reconciliation.Of(new[] { "DM-11" }, new[] { plot }, null, false, KpiTemplates.Mosques).AddsUp);

            string[] lines = KpiCreateReport.Write(CreateFixture.Run(new[] { plot }), new DateTime(2026, 9, 10, 9, 28, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);
            Assert.Contains("      species rows add to 6, no TOTAL row was found to hold that against", lines);
        }

        /// <summary>
        /// The TOTAL row is row 14 counting the heading row as row 1.
        /// </summary>
        [Fact]
        public void ATotalRowWhoseCountCannotBeReadRefuses()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Dm12(""), Phases, "DM-12");

            Assert.False(reading.WasRead);
            Assert.Equal(
                "row 14, the TOTAL row: its COUNT (n) cell holds '' and not a whole number.",
                Assert.Single(reading.Refusals));
        }

        /// <summary>
        /// The skip that used to be a bare continue. A row with a name and no count cannot be
        /// counted, so it refuses and names the row rather than dropping it.
        /// </summary>
        [Fact]
        public void ASpeciesRowWithNoCountIsRefusedAndNamedRatherThanDropped()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-12",
                Headings,
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "M-329343-A18", "ALBIZIA LEBBECK", "" },
                new[] { "TOTAL", "", "", "", "13" });

            SoftscapeReading reading = SoftscapeRows.Read(schedule, Phases, "DM-12");

            Assert.False(reading.WasRead);
            Assert.Equal(
                "row 3, ALBIZIA LEBBECK: its COUNT (n) cell holds '' and not a whole number, so the row cannot be counted.",
                Assert.Single(reading.Refusals));
        }
    }
}
