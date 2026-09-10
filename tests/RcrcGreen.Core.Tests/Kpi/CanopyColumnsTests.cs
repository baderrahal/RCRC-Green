using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// A species written into an empty row carried a name and a count and nothing else, and
    /// the workbook's canopy formula turned the blank diameter into a space and the cell
    /// beside it into #VALUE!. The softscape schedule prints HEIGHT (m) and DIAMETER (m), and
    /// on six species sitting in both they read the same as the workbook's Mature Height and
    /// Average Mature Canopy Diameter. So the two are read off the schedule by heading and
    /// written into the sheet's columns by heading, and nothing else is.
    ///
    /// Every value here is written out by hand off the 1428 measurement: ALBIZIA LEBBECK 15
    /// and 8, BAUHINIA PURPUREA 6 and 5, WASHINGTONIA ROBUSTA 25 and 5, UNKNOWN a dash and 0,
    /// and PHOENIX DACTYLIFERA 15 across in the model against 8 on the client's row.
    /// </summary>
    public class CanopyColumnsTests
    {
        private static readonly string[] Headings =
        {
            "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "COUNT (n)", "HEIGHT (m)", "DIAMETER (m)",
            "WATER DEMAND", "WATER L/TREE/DAY", "L/DAY"
        };

        private static string[] Row(string image, string number, string code, string name, string count,
            string height, string diameter)
        {
            return new[] { image, number, code, name, count, height, diameter, "-", "-", "-" };
        }

        private static string[] Blank(string first = "")
        {
            return new[] { first, "", "", "", "", "", "", "", "", "" };
        }

        /// <summary>
        /// The measured shape, with UNKNOWN printing a dash for its height and 0 for its
        /// diameter, and the heading row read for the two columns.
        /// </summary>
        private static ScannedSchedule Measured(string plot)
        {
            return CreateFixture.Softscape(
                plot,
                Headings,
                Blank("TREES"),
                Blank("Existing"),
                Row("-", "1", "PHO DAC", "PHOENIX DACTYLIFERA", "27", "25", "15"),
                Row("-", "2", "UNK", "UNKNOWN", "16", "-", "0"),
                new[] { "", "", "", "", "43", "", "", "", "", "" },
                Blank("Proposed"),
                Row("Albizia lebbeck.jpg", "3", "ALB LEB", "ALBIZIA LEBBECK", "10", "15", "8"),
                Row("Bauhinia purpurea.jpg", "4", "BAU PUR", "BAUHINIA PURPUREA", "19", "6", "5"),
                new[] { "", "", "", "", "29", "", "", "", "", "" },
                new[] { "TOTAL", "", "", "", "72", "", "", "", "", "" });
        }

        [Fact]
        public void TheHeightAndTheDiameterAreReadOffTheColumnsTheHeadingRowNames()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Measured("FM-05"), CreateFixture.Counted, "FM-05");

            Assert.True(reading.WasRead, string.Join(" ", reading.Refusals));
            SpeciesRow albizia = reading.Species.Single(one => one.BotanicalName == "ALBIZIA LEBBECK");
            Assert.Equal(8, albizia.RowNumber);
            Assert.True(albizia.Height.Held);
            Assert.Equal(15.0, albizia.Height.Value);
            Assert.Equal("15", albizia.Height.Printed);
            Assert.True(albizia.Diameter.Held);
            Assert.Equal(8.0, albizia.Diameter.Value);

            SpeciesRow phoenix = reading.Species.Single(one => one.BotanicalName == "PHOENIX DACTYLIFERA");
            Assert.Equal(4, phoenix.RowNumber);
            Assert.Equal(25.0, phoenix.Height.Value);
            Assert.Equal(15.0, phoenix.Diameter.Value);
        }

        /// <summary>
        /// UNKNOWN prints a dash and a nought, which is exactly the blank the canopy formula
        /// cannot handle. Neither is held, each says what it printed, and the row still counts.
        /// </summary>
        [Fact]
        public void ADashAndANoughtAreNotHeldAndSayWhatTheyPrinted()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Measured("FM-05"), CreateFixture.Counted, "FM-05");

            SpeciesRow unknown = reading.Species.Single(one => one.BotanicalName == "UNKNOWN");
            Assert.Equal(16, unknown.Quantity);
            Assert.False(unknown.Height.Held);
            Assert.Equal("prints '-'", unknown.Height.WhyNotHeld);
            Assert.False(unknown.Diameter.Held);
            Assert.Equal("prints 0, which is no size", unknown.Diameter.WhyNotHeld);
        }

        /// <summary>
        /// By heading and never by position. The two columns swapped over still read right, and
        /// a schedule naming neither still counts its species and says the measure was not
        /// there.
        /// </summary>
        [Fact]
        public void TheColumnsAreFoundByHeadingAndASheetNamingNeitherStillCounts()
        {
            ScannedSchedule swapped = CreateFixture.Softscape(
                "FM-05",
                new[] { "IMAGE", "BOTANICAL NAME", "DIAMETER (m)", "HEIGHT (m)", "COUNT (n)" },
                new[] { "Proposed", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "8", "15", "10" });

            SpeciesRow albizia = Assert.Single(SoftscapeRows.Read(swapped, CreateFixture.Counted, "FM-05").Species);
            Assert.Equal(15.0, albizia.Height.Value);
            Assert.Equal(8.0, albizia.Diameter.Value);

            ScannedSchedule bare = CreateFixture.Softscape(
                "FM-05",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Proposed", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "10" });

            SpeciesRow counted = Assert.Single(SoftscapeRows.Read(bare, CreateFixture.Counted, "FM-05").Species);
            Assert.Equal(10, counted.Quantity);
            Assert.False(counted.Height.Held);
            Assert.Equal("the heading row names no column holding HEIGHT", counted.Height.WhyNotHeld);
            Assert.Equal("the heading row names no column holding DIAMETER", counted.Diameter.WhyNotHeld);
        }

        /// <summary>
        /// The same species off two plots printing the same height and diameter writes them.
        /// </summary>
        [Fact]
        public void RowsThatAgreeGiveOneValueToWrite()
        {
            MergedSpecies albizia = Assert.Single(KpiMerge.Species(new[]
            {
                CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 10, 8, "15", "8") }),
                CreateFixture.Plot("FM-06", species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 15, 6, "15", "8") })
            }, CreateFixture.Counted));

            Assert.True(albizia.Height.Write);
            Assert.Equal(15.0, albizia.Height.Value);
            Assert.Equal(string.Empty, albizia.Height.Why);
            Assert.True(albizia.Diameter.Write);
            Assert.Equal(8.0, albizia.Diameter.Value);
            Assert.Equal(new[] { "FM-05 row 8 prints 8", "FM-06 row 6 prints 8" }, albizia.Diameter.Found);
        }

        /// <summary>
        /// Two plots, two heights: nothing is written into that column, every value is named,
        /// and nothing is averaged or taken first. The diameter, on which they agree, is still
        /// written.
        /// </summary>
        [Fact]
        public void RowsThatDisagreeWriteNothingAndNameEveryValue()
        {
            MergedSpecies albizia = Assert.Single(KpiMerge.Species(new[]
            {
                CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 10, 8, "15", "8") }),
                CreateFixture.Plot("FM-06", species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 15, 6, "12", "8") })
            }, CreateFixture.Counted));

            Assert.False(albizia.Height.Write);
            Assert.Equal(
                "the rows disagree on the height: FM-05 row 8 prints 15, FM-06 row 6 prints 12, so nothing is written",
                albizia.Height.Why);
            Assert.True(albizia.Diameter.Write);

            MergedSpecies bauhinia = Assert.Single(KpiMerge.Species(new[]
            {
                CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("BAUHINIA PURPUREA", "Proposed", 19, 9, "6", "5") }),
                CreateFixture.Plot("FM-06", species: new[] { CreateFixture.Species("BAUHINIA PURPUREA", "Proposed", 20, 7, "6", "4") })
            }, CreateFixture.Counted));

            Assert.False(bauhinia.Diameter.Write);
            Assert.Equal(
                "the rows disagree on the diameter: FM-05 row 9 prints 5, FM-06 row 7 prints 4, "
                + "so nothing is written and the row will not compute its canopy",
                bauhinia.Diameter.Why);
        }

        /// <summary>
        /// A row printing a dash is named and does not stop the rows that hold a value agreeing.
        /// Every row printing a dash or a nought writes nothing.
        /// </summary>
        [Fact]
        public void ARowPrintingADashIsNamedAndTheOthersStillAgree()
        {
            MergedSpecies unknown = Assert.Single(KpiMerge.Species(new[]
            {
                CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0") })
            }, CreateFixture.Counted));

            Assert.False(unknown.Height.Write);
            Assert.Equal(
                "no row printed a height a workbook can compute with: FM-05 row 5 prints '-', so nothing is written",
                unknown.Height.Why);
            Assert.Equal(
                "no row printed a diameter a workbook can compute with: FM-05 row 5 prints 0, which is no size, "
                + "so nothing is written and the row will not compute its canopy",
                unknown.Diameter.Why);

            MergedSpecies mixed = Assert.Single(KpiMerge.Species(new[]
            {
                CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("WASHINGTONIA ROBUSTA", "Existing", 19, 6, "-", "5") }),
                CreateFixture.Plot("FM-06", species: new[] { CreateFixture.Species("WASHINGTONIA ROBUSTA", "Existing", 3, 4, "25", "5") })
            }, CreateFixture.Counted));

            Assert.True(mixed.Height.Write);
            Assert.Equal(25.0, mixed.Height.Value);
            Assert.Equal("FM-05 row 6 prints '-', and every row that holds a height prints 25", mixed.Height.Why);
        }

        /// <summary>
        /// The sheet's own columns are found by what its header row calls them. Measured I and
        /// J on MOSQUES, and the same headings in N and O are found in N and O.
        /// </summary>
        [Fact]
        public void TheSheetsHeightAndDiameterColumnsAreFoundByTheirHeadingsNotTheirLetters()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string measured = WorkbookFixture.Computing(folder,
                    new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "25", "8") },
                    new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") });

                SpeciesList existing = SpeciesList.In(measured, KpiTemplates.Mosques.ExistingTrees);
                Assert.Equal("I", existing.HeightColumn);
                Assert.Equal("J", existing.DiameterColumn);
                Assert.Equal("25", existing.Rows.Single().Height);
                Assert.Equal("8", existing.Rows.Single().Diameter);

                string moved = WorkbookFixture.Computing(folder,
                    new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "25", "8") },
                    new WorkbookFixture.TreeRow[0],
                    "MOVED.xlsx", heightColumn: "N", diameterColumn: "O");

                SpeciesList elsewhere = SpeciesList.In(moved, KpiTemplates.Mosques.ExistingTrees);
                Assert.Equal("N", elsewhere.HeightColumn);
                Assert.Equal("O", elsewhere.DiameterColumn);

                string unnamed = WorkbookFixture.Computing(folder,
                    new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera") },
                    new WorkbookFixture.TreeRow[0],
                    "UNNAMED.xlsx", heightHeading: "Size class", diameterHeading: "Spread class");

                SpeciesList none = SpeciesList.In(unnamed, KpiTemplates.Mosques.ExistingTrees);
                Assert.Equal(string.Empty, none.HeightColumn);
                Assert.Equal("the sheet's header row, row 3, names no column holding HEIGHT", none.WhyNoHeightColumn);
                Assert.Equal("the sheet's header row, row 3, names no column holding DIAMETER", none.WhyNoDiameterColumn);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// A species written in writes four things and no more: the name into D, the count
        /// into B, the height into the height column and the diameter into the diameter
        /// column. Family, genus, native and every code column stay empty.
        /// </summary>
        [Fact]
        public void AnAddedSpeciesWritesItsNameItsCountItsHeightAndItsDiameterAndNothingElse()
        {
            SpeciesList list = SpeciesList.Holding(
                new[] { new SpeciesListRow(4, "Albizia lebbeck", "15", "8") }, 4, 9, "B10", "I", "J");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                KpiMerge.Species(new[]
                {
                    CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("BAUHINIA PURPUREA", "Proposed", 19, 9, "6", "5") })
                }, CreateFixture.Counted),
                KpiTemplates.Mosques, list, list);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null, matches, null, null, null);

            List<CellWrite> onTheSheet = plan.Writes.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet).ToList();
            Assert.Equal(new[] { "B5", "D5", "I5", "J5" }, onTheSheet.Select(one => one.Cell.ToString()));
            Assert.Equal(new[] { "19", "BAUHINIA PURPUREA", "6", "5" }, onTheSheet.Select(one => one.Stored));
            Assert.Empty(plan.Skipped.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet));
        }

        /// <summary>
        /// UNKNOWN written in gets its name and its count, and its height and diameter cells are
        /// named under CELLS NOT WRITTEN with what the schedule printed.
        /// </summary>
        [Fact]
        public void ASpeciesWithNoMeasureHasItsTwoCellsNamedAndNotWritten()
        {
            SpeciesList list = SpeciesList.Holding(
                new[] { new SpeciesListRow(4, "Albizia lebbeck", "15", "8") }, 4, 9, "B10", "I", "J");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                SpeciesMatching.Against(
                    KpiMerge.Species(new[]
                    {
                        CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("UNKNOWN", "Proposed", 16, 5, "-", "0") })
                    }, CreateFixture.Counted),
                    KpiTemplates.Mosques, list, list),
                null, null, null);

            Assert.Equal(new[] { "B5", "D5" },
                plan.Writes.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet).Select(one => one.Cell.ToString()));

            List<NotWritten> skipped = plan.Skipped.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet).ToList();
            Assert.Equal(new[] { "I5", "J5" }, skipped.Select(one => one.Cell));
            Assert.Equal("UNKNOWN height", skipped[0].What);
            Assert.Equal(
                "no row printed a height a workbook can compute with: FM-05 row 5 prints '-', so nothing is written",
                skipped[0].Why);
            Assert.Contains("will not compute its canopy", skipped[1].Why);
        }

        /// <summary>
        /// A sheet whose header row names no height column has both cells named with that
        /// reason rather than written by a letter somebody assumed.
        /// </summary>
        [Fact]
        public void ASheetNamingNoSuchColumnWritesNeitherAndSaysSo()
        {
            SpeciesList list = SpeciesList.Holding(new[] { new SpeciesListRow(4, "Albizia lebbeck") }, 4, 9, "B10");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                SpeciesMatching.Against(
                    KpiMerge.Species(new[]
                    {
                        CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("BAUHINIA PURPUREA", "Proposed", 19, 9, "6", "5") })
                    }, CreateFixture.Counted),
                    KpiTemplates.Mosques, list, list),
                null, null, null);

            Assert.Equal(new[] { "B5", "D5" },
                plan.Writes.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet).Select(one => one.Cell.ToString()));
            List<NotWritten> skipped = plan.Skipped.Where(one => one.SheetName == KpiTemplates.ProposedTreesSheet).ToList();
            Assert.Equal(2, skipped.Count);
            Assert.Equal("the sheet's header row, row 3, names no column holding HEIGHT", skipped[0].Why);
            Assert.Equal("the sheet's header row, row 3, names no column holding DIAMETER", skipped[1].Why);
        }

        /// <summary>
        /// Phoenix dactylifera reads 15 metres across in the model and 8 on the client's row.
        /// Named, both numbers, and the row's own number left where it is. The height agrees
        /// and is not named.
        /// </summary>
        [Fact]
        public void AMatchedSpeciesMeasuringDifferentlyIsNamedAndNothingIsChanged()
        {
            SpeciesList list = SpeciesList.Holding(
                new[] { new SpeciesListRow(4, "Phoenix dactylifera", "25", "8") }, 4, 9, "B10", "I", "J");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                SpeciesMatching.Against(
                    KpiMerge.Species(new[]
                    {
                        CreateFixture.Plot("FM-05", species: new[] { CreateFixture.Species("PHOENIX DACTYLIFERA", "Existing", 27, 4, "25", "15") })
                    }, CreateFixture.Counted),
                    KpiTemplates.Mosques, list, list),
                null, null, null);

            // The count alone goes on a matched row.
            Assert.Equal(new[] { "B4" },
                plan.Writes.Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet).Select(one => one.Cell.ToString()));

            MeasureDifference difference = Assert.Single(plan.Differences);
            Assert.Equal(KpiTemplates.ExistingTreesSheet, difference.SheetName);
            Assert.Equal(4, difference.Row);
            Assert.Equal("Phoenix dactylifera", difference.WorkbookName);
            Assert.Equal("diameter", difference.What);
            Assert.Equal("15", difference.RevitPrints);
            Assert.Equal("8", difference.WorkbookHolds);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(matches: plan.Matches.ToArray(), existingList: list, proposedList: list),
                new System.DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("== MATCHED SPECIES WHOSE HEIGHT OR DIAMETER IN REVIT DIFFERS FROM THE ROW'S (1) ==", report);
            Assert.Contains("  Tree List - Existing | 4 | Phoenix dactylifera | diameter | 15 | 8 | CHANGED NOTHING", report);
        }

        /// <summary>
        /// The report says, per species written in, what went into the height and diameter
        /// cells or why nothing did.
        /// </summary>
        [Fact]
        public void TheReportSaysWhatWentIntoEachMeasureCell()
        {
            SpeciesList list = SpeciesList.Holding(
                new[] { new SpeciesListRow(4, "Albizia lebbeck", "15", "8") }, 4, 9, "B10", "I", "J");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                KpiMerge.Species(new[]
                {
                    CreateFixture.Plot("FM-05", species: new[]
                    {
                        CreateFixture.Species("BAUHINIA PURPUREA", "Proposed", 19, 9, "6", "5"),
                        CreateFixture.Species("UNKNOWN", "Proposed", 16, 5, "-", "0")
                    })
                }, CreateFixture.Counted),
                KpiTemplates.Mosques, list, list);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(matches: matches.ToArray(), existingList: list, proposedList: list),
                new System.DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("  BAUHINIA PURPUREA | Proposed | 19 | FM-05 19 | Tree List - Proposed D5 and B5 | 6 into I5 | 5 into J5 | "
                + SpeciesMatching.WrittenIn, report);
            Assert.Contains("  UNKNOWN | Proposed | 16 | FM-05 16 | Tree List - Proposed D6 and B6 | not written: no row printed a height", report);
            Assert.Contains("  A written row carries the botanical name, the count, and the height and the canopy", report);
        }
    }
}
