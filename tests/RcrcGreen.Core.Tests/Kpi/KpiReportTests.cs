using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 9, 14, 5, 12);

        private static string[] LinesOf(string report)
        {
            return report.Split(new[] { "\r\n" }, StringSplitOptions.None);
        }

        private static string[] RealLines()
        {
            return LinesOf(KpiReport.Write(KpiFixture.RealShaped(), Noon));
        }

        private static int At(string[] lines, string line)
        {
            int at = Array.IndexOf(lines, line);
            Assert.True(at >= 0, "the report holds the line: " + line);
            return at;
        }

        private static string[] Following(string[] lines, string line, int howMany)
        {
            return lines.Skip(At(lines, line) + 1).Take(howMany).ToArray();
        }

        private static string[] UntilBlank(string[] lines, string line)
        {
            return lines.Skip(At(lines, line) + 1).TakeWhile(one => one.Length > 0).ToArray();
        }

        [Fact]
        public void EverySectionHeadingCarriesItsOwnCountAndTheyComeInOrder()
        {
            string[] headings = RealLines()
                .Where(line => line.StartsWith("== ", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "== READS THAT DID NOT HAPPEN (0) ==",
                    "== 1 DOCUMENT (96934) ==",
                    "== 2 PROJECT INFORMATION (4) ==",
                    "== 3 TITLE BLOCKS AND SHEETS (1385) ==",
                    "== 4 LINKED MODELS (2) ==",
                    "== 5 SCHEDULES (7) ==",
                    "== 6 SOFTSCAPE SCHEDULE FIELDS (2) ==",
                    "== 7 EXISTING AND PROPOSED (2) ==",
                    "== 8 AREAS AND UNITS (2) ==",
                    "== 9 THE NINE QUESTIONS (7) =="
                },
                headings);
        }

        [Fact]
        public void AnEmptyScanStillWritesEverySectionWithAZeroInIt()
        {
            string[] headings = LinesOf(KpiReport.Write(KpiFixture.Build(), Noon))
                .Where(line => line.StartsWith("== ", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "== READS THAT DID NOT HAPPEN (0) ==",
                    "== 1 DOCUMENT (0) ==",
                    "== 2 PROJECT INFORMATION (0) ==",
                    "== 3 TITLE BLOCKS AND SHEETS (0) ==",
                    "== 4 LINKED MODELS (0) ==",
                    "== 5 SCHEDULES (0) ==",
                    "== 6 SOFTSCAPE SCHEDULE FIELDS (0) ==",
                    "== 7 EXISTING AND PROPOSED (0) ==",
                    "== 8 AREAS AND UNITS (0) ==",
                    "== 9 THE NINE QUESTIONS (0) =="
                },
                headings);
        }

        [Fact]
        public void WhenEveryReadRanTheSkippedBlockSaysSo()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[] { "what was skipped, and why", "Every read ran. A zero anywhere below is a real zero." },
                Following(lines, "== READS THAT DID NOT HAPPEN (0) ==", 2));
        }

        [Fact]
        public void EverySkippedReadIsPrintedUnderItsOwnHeading()
        {
            KpiScan scan = KpiFixture.Build(skipped: new[]
            {
                "Links: RCRC_NG05_NU_01_ARCH_RVT24.rvt refused to open",
                "Schedules: the rows of DM-11-(610) SOFTSCAPE SCHEDULE were not read"
            });
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[]
                {
                    "what was skipped, and why",
                    "  Links: RCRC_NG05_NU_01_ARCH_RVT24.rvt refused to open",
                    "  Schedules: the rows of DM-11-(610) SOFTSCAPE SCHEDULE were not read"
                },
                Following(lines, "== READS THAT DID NOT HAPPEN (2) ==", 3));
            Assert.DoesNotContain("Every read ran. A zero anywhere below is a real zero.", lines);
        }

        [Fact]
        public void TheHeaderNamesTheDocumentTheMinuteAndThatNothingWasChanged()
        {
            string[] lines = RealLines();

            Assert.Equal("RCRC Green KPI scan", lines[0]);
            Assert.Equal("Document: RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached", lines[1]);
            Assert.Equal("Written: 2026-09-09 14:05", lines[2]);
            Assert.Equal("Read only. Nothing in the model was changed and no workbook was touched.", lines[3]);
        }

        [Fact]
        public void SectionOnePrintsTheUnitLabelItsIdAndItsRounding()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "elements that are not types",
                    "Title: RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                    "Path: C:\\Models\\RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached.rvt",
                    "Elements: 96934 instances and 11208 types",
                    "Read in 1.4 seconds",
                    "Area unit: Square meters, id autodesk.unit.unit:squareMeters-1.0.1, rounded to 0.01",
                    "Length unit: Millimeters, id autodesk.unit.unit:millimeters-1.0.1, rounded to 1"
                },
                Following(lines, "== 1 DOCUMENT (96934) ==", 7));
        }

        [Fact]
        public void AUnitThatCouldNotBeReadSaysSoRatherThanPrintingNothing()
        {
            string[] lines = LinesOf(KpiReport.Write(KpiFixture.Build(), Noon));

            Assert.Contains("Path: (not saved, so there is no path)", lines);
            Assert.Contains("Area unit: UNKNOWN, the unit setting could not be read", lines);
            Assert.Contains("Length unit: UNKNOWN, the unit setting could not be read", lines);
        }

        [Fact]
        public void SectionTwoListsEveryProjectParameterWithKindStorageGuidAndValue()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "Client Name | built-in | String | - | (empty)",
                    "PRX_Neighbourhood | shared | String | 8f0b1c2d-3e4f-4a5b-8c6d-7e8f9a0b1c2d | NU05",
                    "Project Name | built-in | String | - | NG05 Neighbourhood Unit",
                    "Project Number | shared | String | - | (no value)"
                },
                UntilBlank(lines, "Every parameter on the Project Information element, no cap. The neighbourhood name is one of these."));
        }

        [Fact]
        public void SectionTwoNamesTheNeighbourhoodNearMisses()
        {
            Assert.Contains(
                "Names holding NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE: PRX_Neighbourhood.",
                RealLines());
        }

        [Fact]
        public void SectionTwoSaysWhenNoNameHoldsANeighbourhoodWord()
        {
            KpiScan scan = KpiFixture.Build(projectInformation: new[] { KpiFixture.Parameter("Project Name", "NG05") });
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains(
                "No name holds NEIGH, DISTRICT, COMMUNITY, LOCATION or ZONE. The neighbourhood name is not in "
                + "Project Information under any of those words, so the team picks from the list above.",
                lines);
        }

        [Fact]
        public void SectionThreeCountsTheSheetsAndTheTitleBlockTypes()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "sheets",
                    "1385 sheets, of which 2 are placeholders with no title block.",
                    "Title block instances: 1383"
                },
                Following(lines, "== 3 TITLE BLOCKS AND SHEETS (1385) ==", 3));
            Assert.Equal(
                new[]
                {
                    "family | type | instances",
                    "AR-PRX-Title_Block_A1 | A1 Metric | 1300 instances",
                    "AR-PRX-Title_Block_A1 | A1 Metric Key Plan | 83 instances"
                },
                UntilBlank(lines, "TITLE BLOCK FAMILIES AND TYPES, 2"));
        }

        [Fact]
        public void SectionThreePrintsNotFoundForPlotUid2AndTheNearMissesUnderIt()
        {
            string[] lines = RealLines();

            // A near miss named and never shown is the answer withheld. PRX_COMPONENT does not
            // exist on the real model, the sheet carries PRX_Component, and the first report
            // named that near miss while printing not one of its 1384 values.
            Assert.Equal(
                new[]
                {
                    "  Names on the title block instance holding COMPONENT, PLOT or UID: PRX_COMPONENT, PRX_Plot_ID",
                    "  PRX_COMPONENT on the title block instance, showing 3 of 3, the ones with a value first:",
                    "  sheet number | sheet name | value",
                    "  DM-11-600QD | SOFTSCAPE SCHEDULES | SOFTSCAPE",
                    "  DM-11-610QD | HARDSCAPE SCHEDULES | HARDSCAPE",
                    "  010QE Copy 001 | Copy of key plan | (empty)",
                    "  PRX_Plot_ID on the title block instance: no value was read for it."
                },
                UntilBlank(lines, "PRX_Plot_UID2 on the title block instance: NOT FOUND"));
        }

        [Fact]
        public void SectionThreeTalliesEveryNameOnTheTitleBlockInstance()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "name | carrying it | with a value",
                    "PRX_COMPONENT | 1383 | 1201",
                    "PRX_Plot_ID | 1383 | 160",
                    "Sheet Height | 1383 | 1383",
                    "Sheet Width | 1383 | 1383"
                },
                UntilBlank(lines, "ON EVERY TITLE BLOCK INSTANCE, 4 parameter names over 1383 title block instances"));
        }

        [Fact]
        public void SectionThreeShowsTwentyExamplesAndSaysHowManyThereWere()
        {
            SheetValue[] values = Enumerable.Range(1, 25)
                .Select(n => KpiFixture.Value("PRX_COMPONENT", "S" + n, "Sheet " + n, n <= 15 ? "SOFTSCAPE" : ""))
                .ToArray();
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 25,
                titleBlockInstances: 25,
                onInstances: KpiFixture.OnInstances(25, new[] { KpiFixture.Tally("PRX_COMPONENT", 25, 15) }, values)));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            string[] shown = UntilBlank(lines,
                "PRX_COMPONENT on the title block instance, showing 20 of 25, the ones with a value first:");

            Assert.Equal("sheet number | sheet name | value", shown[0]);
            Assert.Equal(20, shown.Length - 1);
            Assert.Equal("S1 | Sheet 1 | SOFTSCAPE", shown[1]);
            Assert.Equal("S15 | Sheet 15 | SOFTSCAPE", shown[15]);
            Assert.Equal("S16 | Sheet 16 | (empty)", shown[16]);
            Assert.Equal("S20 | Sheet 20 | (empty)", shown[20]);
        }

        [Fact]
        public void SectionThreePrintsTheOnesWithAValueBeforeTheEmptyOnes()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 3,
                titleBlockInstances: 3,
                onInstances: KpiFixture.OnInstances(
                    3,
                    new[] { KpiFixture.Tally("PRX_COMPONENT", 3, 1) },
                    new[]
                    {
                        KpiFixture.Value("PRX_COMPONENT", "003", "C", ""),
                        KpiFixture.Value("PRX_COMPONENT", "002", "B", "HARDSCAPE"),
                        KpiFixture.Value("PRX_COMPONENT", "001", "A", "")
                    })));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[]
                {
                    "sheet number | sheet name | value",
                    "002 | B | HARDSCAPE",
                    "001 | A | (empty)",
                    "003 | C | (empty)"
                },
                UntilBlank(lines, "PRX_COMPONENT on the title block instance, showing 3 of 3, the ones with a value first:"));
        }

        [Fact]
        public void SectionThreeRepeatsTheBlockForTheTitleBlockTypeAndTheSheet()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "name | carrying it | with a value",
                    "Keynote | 2 | 0",
                    "Type Name | 2 | 2"
                },
                UntilBlank(lines, "ON EVERY TITLE BLOCK TYPE, 2 parameter names over 2 title block types"));
            Assert.Equal(
                new[] { "  No name on the title block type holds COMPONENT, PLOT or UID." },
                UntilBlank(lines, "PRX_COMPONENT on the title block type: NOT FOUND"));

            Assert.Equal(
                new[]
                {
                    "name | carrying it | with a value",
                    "PRX_Plot_ID | 1385 | 1200",
                    "Sheet Name | 1385 | 1385",
                    "Sheet Number | 1385 | 1385"
                },
                UntilBlank(lines, "ON EVERY SHEET, 3 parameter names over 1385 sheets"));
            Assert.Equal(
                new[]
                {
                    "  Names on the sheet holding COMPONENT, PLOT or UID: PRX_Plot_ID",
                    "  PRX_Plot_ID on the sheet: no value was read for it."
                },
                UntilBlank(lines, "PRX_Plot_UID2 on the sheet: NOT FOUND"));
        }

        [Fact]
        public void SectionFourMarksEveryNameHoldingTheLinkMark()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "link type | status | nested | name holds 00",
                    "RCRC_NG05_NU_00_SITE_RVT24.rvt | Loaded | not nested | holds 00",
                    "RCRC_NG05_NU_01_ARCH_RVT24.rvt | Unloaded | not nested | -"
                },
                Following(lines, "link types, 1 placed instance", 3));
            Assert.Equal(
                new[] { "RCRC_NG05_NU_00_SITE_RVT24.rvt : 1 | RCRC_NG05_NU_00_SITE_RVT24.rvt | loaded | holds 00" },
                UntilBlank(lines, "link instance | type | loaded | name holds 00"));
            Assert.Contains(
                "Links whose name holds 00: RCRC_NG05_NU_00_SITE_RVT24.rvt, RCRC_NG05_NU_00_SITE_RVT24.rvt : 1.",
                lines);
        }

        [Fact]
        public void SectionFourSaysWhenNoLinkIsLoaded()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType("RCRC_NG05_NU_00_SITE_RVT24.rvt", "Unloaded", isLoaded: false) }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains("None is loaded, so no filled region was read. Load the link and scan again.", lines);
            Assert.DoesNotContain(lines, line => line.StartsWith("LOADED LINK ", StringComparison.Ordinal));
        }

        /// <summary>
        /// A link type can read Loaded while no placed instance hands back a document. Telling
        /// somebody to load a link that is loaded sends them the wrong way, so the sentence is
        /// built from the two reads rather than asserted from an empty list.
        /// </summary>
        [Fact]
        public void SectionFourSaysWhenALoadedLinkTypeHasNoInstanceToRead()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType("RCRC_NG05_NU_00_SITE_RVT24.rvt", "Loaded", isLoaded: true) }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains(
                "1 link type reads Loaded and 0 of 0 placed instances handed back a document, so no filled region "
                + "was read. Place an instance and scan again.",
                lines);
        }

        [Fact]
        public void SectionFourSaysWhenTheModelHoldsNoLinkAtAll()
        {
            string[] lines = LinesOf(KpiReport.Write(KpiFixture.Build(), Noon));

            Assert.Contains("No link in the model, so there is no filled region to read.", lines);
        }

        [Fact]
        public void SectionFourPrintsTheRawAndPrintedInterventionAreaOnOneLine()
        {
            string[] lines = RealLines();

            Assert.Contains(
                "LOADED LINK RCRC_NG05_NU_00_SITE_RVT24.rvt : 1, document RCRC_NG05_NU_00_SITE_RVT24, 412 filled regions",
                lines);
            Assert.Equal(
                new[]
                {
                    "  region type | plot | measures | raw | printed",
                    "  The raw number is square feet only where measures reads Area. A number typed by hand measures nothing and prints with no unit.",
                    "  ID Intervention Area | (empty) | Area | 10763.91 | 1000.00 m²",
                    "  ID Intervention Area | (empty) | Area | 5381.96 | 500.00 m²"
                },
                UntilBlank(lines, "  PRX_Intervention Area: on 412 of 412 filled regions, 398 with a value, showing 2 of 2:"));
        }

        [Fact]
        public void SectionFourPrintsNotFoundAndTheNearMissesWhenALinkHasNoInterventionArea()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType("RCRC_NG05_NU_00_SITE_RVT24.rvt") },
                new[] { KpiFixture.LinkInstance("RCRC_NG05_NU_00_SITE_RVT24.rvt : 1", "RCRC_NG05_NU_00_SITE_RVT24.rvt") },
                new[]
                {
                    KpiFixture.Contents("RCRC_NG05_NU_00_SITE_RVT24.rvt : 1", filledRegionCount: 3,
                        regionParameters: new[] { KpiFixture.Tally("Comments", 3, 0), KpiFixture.Tally("Area", 3, 3) })
                }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[] { "  Filled region parameter names holding INTERVENTION or AREA: Area" },
                UntilBlank(lines, "  PRX_Intervention Area: NOT FOUND on any filled region in this link"));
        }

        [Fact]
        public void SectionFiveListsEveryScheduleWithItsWorkbookWordsOrADash()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "DM-11-(600) HARDSCAPE SCHEDULE | Floors | 3 fields | PRX_Ref Plot ID Equals DM-11; Type Comments Contains HARD | on a sheet | HARDSCAPE",
                    "DM-11-(610) SOFTSCAPE SCHEDULE | Planting | 5 fields | PRX_Ref Plot ID Equals DM-11 | on a sheet | SOFTSCAPE",
                    "DM-11-(620) SHRUBS AND LAWN SCHEDULE | Floors | 3 fields | PRX_Ref Plot ID Equals DM-11; Type Comments Contains SHRUB | on a sheet | SHRUB, LAWN",
                    "DM-12-(600) HARDSCAPE SCHEDULE | Floors | 3 fields | PRX_Ref Plot ID Equals DM-12; Type Comments Contains HARD | on a sheet | HARDSCAPE",
                    "DM-12-(610) SOFTSCAPE SCHEDULE | Planting | 5 fields | PRX_Ref Plot ID Equals DM-12 | on a sheet | SOFTSCAPE",
                    "DM-12-(620) SHRUBS AND LAWN SCHEDULE | Floors | 3 fields | PRX_Ref Plot ID Equals DM-12; Type Comments Contains SHRUB | on a sheet | SHRUB, LAWN",
                    "Sheet List | Sheets | 3 fields | no filter | not on a sheet | -"
                },
                UntilBlank(lines, "name | category | fields | filters | on a sheet | workbook words in the name"));
        }

        [Fact]
        public void SectionFiveGroupsTheNamesOnceThePlotIsTakenOff()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "A name that does not follow PlotID-(code) name is shown whole.",
                    "name without the plot | schedules | workbook words",
                    "(600) HARDSCAPE SCHEDULE | 2 schedules | HARDSCAPE",
                    "(610) SOFTSCAPE SCHEDULE | 2 schedules | SOFTSCAPE",
                    "(620) SHRUBS AND LAWN SCHEDULE | 2 schedules | SHRUB, LAWN",
                    "Sheet List | 1 schedule | -"
                },
                UntilBlank(lines, "NAMES ONCE THE PLOT IS TAKEN OFF, 4"));
        }

        [Fact]
        public void SectionFivePrintsTheFieldsAndFiltersOfTheScheduleReadInFull()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "",
                    "DM-11-(610) SOFTSCAPE SCHEDULE, category Planting, phase New Construction, phase filter Show All",
                    "  fields in order, 5: heading | parameter | field type | measures | unit | hidden",
                    "  BOTANICAL NAME | PRX_Botanical Name | Instance | - | project unit | shown",
                    "  COMMON NAME | PRX_Common Name | Instance | - | project unit | shown",
                    "  SIZE | PRX_Tree Size | ElementType | - | project unit | shown",
                    "  ENTER EACH PROPOSED TREE QUANTITY | Count | Count | - | project unit | shown",
                    "  PRX_Ref Plot ID | PRX_Ref Plot ID | Instance | - | project unit | hidden",
                    "  filters, 1: field | rule | value",
                    "  PRX_Ref Plot ID | Equals | DM-11",
                    ""
                },
                Following(lines, "READ IN FULL, 1, one per name the workbook draws from, the first in name order that lists an element", 11));
        }

        [Fact]
        public void SectionSixPrintsTheRowsAsPrintedJoinedWithABar()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "  The rows are as the schedule last regenerated, which can be older than the elements listed count below it when the model changed since and the schedule was not opened.",
                    "  BOTANICAL NAME | COMMON NAME | SIZE | ENTER EACH PROPOSED TREE QUANTITY",
                    "  Acacia tortilis | Umbrella thorn | 3 m | 30",
                    "  Ziziphus spina-christi | Sidr | 3 m | 27",
                    "  - | - | - | 57"
                },
                Following(lines, "  rows as printed, showing 4 of 4. The first row is usually the headings and the last usually the total. When fewer are shown than there are, the last one shown is the schedule's last row.", 5));
        }

        /// <summary>
        /// The cap is 200 because the workbook's softscape lists run 80 to 89 species and a
        /// cap of 30 lost about 55 of them from the one section this round exists to fill.
        /// </summary>
        [Fact]
        public void SectionSixShowsTwoHundredRowsAndSaysHowManyThereWere()
        {
            string[][] rows = Enumerable.Range(1, 205)
                .Select(n => new[] { "Tree " + n, n.ToString() })
                .ToArray();
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE",
                    fields: new[] { KpiFixture.Field("BOTANICAL NAME"), KpiFixture.Field("COUNT", "Count", "Count") },
                    rowsWereRead: true, bodyRowCount: 205, rows: rows)
            }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            string[] shown = UntilBlank(lines,
                "  rows as printed, showing 200 of 205. The first row is usually the headings and the last usually the total. When fewer are shown than there are, the last one shown is the schedule's last row.");

            Assert.Equal(201, shown.Length);
            Assert.Equal("  Tree 1 | 1", shown[1]);
            Assert.Equal("  Tree 200 | 200", shown[200]);
        }

        [Fact]
        public void AListOfEightyNineSpeciesPrintsWhole()
        {
            string[][] rows = Enumerable.Range(1, 89)
                .Select(n => new[] { "Species " + n, n.ToString() })
                .ToArray();
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE",
                    fields: new[] { KpiFixture.Field("BOTANICAL NAME") },
                    rowsWereRead: true, bodyRowCount: 89, rows: rows)
            }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains("  rows as printed, showing 89 of 89. The first row is usually the headings and the "
                + "last usually the total. When fewer are shown than there are, the last one shown is the "
                + "schedule's last row.", lines);
            Assert.Contains("  Species 89 | 89", lines);
        }

        [Fact]
        public void SectionSixNamesTheCountFields()
        {
            Assert.Contains("  Count fields: ENTER EACH PROPOSED TREE QUANTITY", RealLines());
        }

        [Fact]
        public void SectionSixSaysWhenThereIsNoCountField()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE",
                    fields: new[] { KpiFixture.Field("BOTANICAL NAME"), KpiFixture.Field("QTY", "PRX_Quantity") },
                    rowsWereRead: true)
            }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains("  No Count field.", lines);
        }

        [Fact]
        public void SectionSixPrintsTheValuesOfEveryParameterWhoseNameHoldsAPlantingWord()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "  PRX_Botanical Name on instances, 2 distinct values, counts add to 57 over 57 elements listed:",
                    "    Acacia tortilis | 30 elements",
                    "    Ziziphus spina-christi | 27 elements",
                    "  PRX_Tree Size on types, 1 distinct value, counts add to 57 over 57 elements listed:",
                    "    3 m | 57 elements"
                },
                UntilBlank(lines, "  Values of every parameter whose name holds BOTANIC, LATIN, SPECIES, NAME, QTY, QUANT, COUNT, NUMBER, SIZE or TREE:"));
        }

        [Fact]
        public void SectionSixListsTheFamilyTypesMostUsedFirst()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "  Tree : Acacia tortilis | 30 elements",
                    "  Tree : Ziziphus spina-christi | 27 elements"
                },
                Following(lines, "  family : type, showing 2 of 2", 2));
        }

        [Fact]
        public void SectionSixSaysWhenNoScheduleIsNamedForSoftscape()
        {
            string[] lines = LinesOf(KpiReport.Write(KpiFixture.Build(), Noon));

            Assert.Contains(
                "No schedule name holds SOFTSCAPE. Section 5 lists every name, so the real one can be picked.",
                lines);
        }

        [Fact]
        public void SectionSevenNumbersThePhasesInOrder()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[] { "  1. Existing", "  2. Proposed" },
                UntilBlank(lines, "phases in the model, in order"));
        }

        [Fact]
        public void SectionSevenPrintsThePhaseCreatedCountsOfTheSoftscapeElements()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "  elements by phase created: Proposed 45, Existing 12",
                    "  elements by phase demolished: None 57",
                    "  elements by workset: Planting 57",
                    "  elements by design option: Main Model 57",
                    "  Values of every parameter whose name holds EXIST, PROPOS, STATUS, RETAIN, REMOV, NEW, PHASE or CONDITION:",
                    "  Phase Created on instances, 2 distinct values, counts add to 57 over 57 elements listed:",
                    "    Proposed | 45 elements",
                    "    Existing | 12 elements"
                },
                UntilBlank(lines, "DM-11-(610) SOFTSCAPE SCHEDULE: phase New Construction, phase filter Show All, filters PRX_Ref Plot ID Equals DM-11"));
        }

        [Fact]
        public void SectionSevenListsTheColumnHeadingsHoldingExistingOrProposedOnce()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[] { "  (610) SOFTSCAPE SCHEDULE: ENTER EACH PROPOSED TREE QUANTITY" },
                UntilBlank(lines, "COLUMN HEADINGS HOLDING EXISTING OR PROPOSED, 1, plot taken off"));
        }

        [Fact]
        public void SectionEightPrintsRawSquareFeetTheSquareMetresWorkedOutAndThePrintedText()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "areas measured off elements the marked schedules list",
                    "Project area unit: Square meters, id autodesk.unit.unit:squareMeters-1.0.1, rounded to 0.01",
                    "schedule | parameter | element | raw, square feet | square metres worked out from the raw | as printed",
                    "DM-11-(600) HARDSCAPE SCHEDULE | Area | Floors 412563 | 1000 | 92.903 | 92.90 m²",
                    "DM-11-(620) SHRUBS AND LAWN SCHEDULE | Area | Floors 412580 | 500 | 46.4515 | 46.45 m²"
                },
                UntilBlank(lines, "== 8 AREAS AND UNITS (2) =="));
            Assert.Contains("ROWS AS PRINTED, 0 schedules, the totals as a sheet shows them", lines);
        }

        [Fact]
        public void SectionEightPrintsTheRowsOfAMarkedScheduleThatIsNotSoftscape()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("DM-11-(600) HARDSCAPE SCHEDULE", "Floors", rowsWereRead: true, bodyRowCount: 2,
                    rows: new[] { new[] { "Type", "Area" }, new[] { "Paving", "92.90 m²" } })
            }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[]
                {
                    "",
                    "DM-11-(600) HARDSCAPE SCHEDULE",
                    "  rows as printed, showing 2 of 2. The first row is usually the headings and the last usually the total. When fewer are shown than there are, the last one shown is the schedule's last row.",
                    "  The rows are as the schedule last regenerated, which can be older than the elements listed count below it when the model changed since and the schedule was not opened.",
                    "  Type | Area",
                    "  Paving | 92.90 m²"
                },
                Following(lines, "ROWS AS PRINTED, 1 schedules, the totals as a sheet shows them", 6));
        }

        [Fact]
        public void SectionNinePrintsFoundOrNotFoundBeforeEveryAnswer()
        {
            string[] lines = RealLines();
            int nine = At(lines, "== 9 THE NINE QUESTIONS (7) ==");
            string[] answers = lines.Skip(nine)
                .Where(line => line.StartsWith("   ", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(9, answers.Length);
            Assert.All(answers, line => Assert.True(
                line.StartsWith("   FOUND. ", StringComparison.Ordinal)
                || line.StartsWith("   NOT FOUND. ", StringComparison.Ordinal),
                line));
            Assert.Equal(7, answers.Count(line => line.StartsWith("   FOUND. ", StringComparison.Ordinal)));
            Assert.Equal(2, answers.Count(line => line.StartsWith("   NOT FOUND. ", StringComparison.Ordinal)));
        }

        [Fact]
        public void SectionNinePutsTheQuestionAboveItsAnswer()
        {
            string[] lines = RealLines();

            Assert.Equal(
                new[]
                {
                    "   NOT FOUND. PRX_COMPONENT on 1383 title block instances, 1201 with a value. PRX_Plot_UID2 NOT FOUND "
                    + "on any title block instance. Section 3 lists the near misses.",
                    "",
                    "2. Is PRX_COMPONENT on the title block instance or on the sheet itself?",
                    "   FOUND. PRX_COMPONENT is on the title block instance, 1383 of 1383 title block instances carry it "
                    + "and 1201 hold a value, not on the title block type, not on the sheet."
                },
                Following(lines, "1. Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2?", 4));
        }

        [Fact]
        public void EveryLineEndsWithCarriageReturnAndLineFeed()
        {
            string report = KpiReport.Write(KpiFixture.RealShaped(), Noon);

            Assert.EndsWith("\r\n", report);
            Assert.DoesNotContain("\n", report.Replace("\r\n", string.Empty));
            Assert.DoesNotContain("\r", report.Replace("\r\n", string.Empty));
        }

        [Fact]
        public void ANullScanIsRefused()
        {
            Assert.Throws<ArgumentNullException>(() => KpiReport.Write(null, Noon));
        }

        /// <summary>
        /// One name bound to the family and to the project puts the same tree under a value on
        /// the instance and a value on the type. Summed under one heading that counted 114
        /// values over 57 trees, so the two sides print apart with their own sums.
        /// </summary>
        [Fact]
        public void SectionSixKeepsInstanceValuesAndTypeValuesOfOneNameApart()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[]
                {
                    KpiFixture.Schedule(KpiFixture.Softscape, rowsWereRead: true,
                        fields: new[] { KpiFixture.Field("BOTANICAL NAME") })
                },
                elements: new[]
                {
                    KpiFixture.Elements(KpiFixture.Softscape,
                        elementCount: 57,
                        instanceParameters: new[] { KpiFixture.Tally("Botanical Name", 57, 0) },
                        typeParameters: new[] { KpiFixture.Tally("Botanical Name", 2, 2) },
                        wordValues: new[]
                        {
                            new ParameterValueCount("Botanical Name", "", 57, false),
                            new ParameterValueCount("Botanical Name", "Acacia tortilis", 30, true),
                            new ParameterValueCount("Botanical Name", "Ziziphus spina-christi", 27, true)
                        })
                }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[]
                {
                    "  Botanical Name on instances, 1 distinct value, counts add to 57 over 57 elements listed:",
                    "    (empty) | 57 elements",
                    "  Botanical Name on types, 2 distinct values, counts add to 57 over 57 elements listed:",
                    "    Acacia tortilis | 30 elements",
                    "    Ziziphus spina-christi | 27 elements"
                },
                UntilBlank(lines, "  Values of every parameter whose name holds BOTANIC, LATIN, SPECIES, NAME, QTY, QUANT, COUNT, NUMBER, SIZE or TREE:"));
        }

        [Fact]
        public void ARoundingFinerThanFourPlacesPrintsEveryPlace()
        {
            KpiScan scan = KpiFixture.Build(
                area: new ProjectUnit("Square meters", "autodesk.unit.unit:squareMeters-1.0.1", 0.00001));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains("Area unit: Square meters, id autodesk.unit.unit:squareMeters-1.0.1, rounded to 0.00001", lines);
        }

        /// <summary>
        /// A single space is how a label is blanked in a title block family. It is not a value,
        /// so it sorts after the real ones and prints as what it is rather than as a blank column.
        /// </summary>
        [Fact]
        public void AWhitespaceOnlyValueSortsAfterRealValuesAndSaysWhatItIs()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 3,
                titleBlockInstances: 3,
                onInstances: KpiFixture.OnInstances(3,
                    new[] { KpiFixture.Tally("PRX_COMPONENT", 3, 1) },
                    new[]
                    {
                        KpiFixture.Value("PRX_COMPONENT", "S1", "one", "   "),
                        KpiFixture.Value("PRX_COMPONENT", "S2", "two", "HARDSCAPE"),
                        KpiFixture.Value("PRX_COMPONENT", "S3", "three", "")
                    })));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[]
                {
                    "sheet number | sheet name | value",
                    "S2 | two | HARDSCAPE",
                    "S1 | one | (whitespace only)",
                    "S3 | three | (empty)"
                },
                UntilBlank(lines, "PRX_COMPONENT on the title block instance, showing 3 of 3, the ones with a value first:"));
        }

        /// <summary>
        /// A schedule chosen for a full read whose rows Revit refused still prints its block,
        /// because its elements were read and are the answer to two questions. Its rows line
        /// says where the reason is rather than printing zero of zero.
        /// </summary>
        [Fact]
        public void AScheduleWhoseRowsWereRefusedStillPrintsItsBlockWithoutRows()
        {
            KpiScan scan = KpiFixture.Build(
                schedules: KpiFixture.Schedules(
                    new[]
                    {
                        KpiFixture.Schedule(KpiFixture.Softscape, rowsRefused: true,
                            fields: new[] { KpiFixture.Field("BOTANICAL NAME") })
                    },
                    elements: new[] { KpiFixture.Elements(KpiFixture.Softscape, elementCount: 4,
                        categories: new[] { KpiFixture.Counted("Planting", 4) }) }),
                skipped: new[] { "The rows of DM-11-(610) SOFTSCAPE SCHEDULE were not read. The table refused." });
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Contains("  rows as printed: not read. READS THAT DID NOT HAPPEN at the top says why. The elements it lists were read and follow.", lines);
            Assert.Contains("  elements listed: 4, categories: Planting 4", lines);
            Assert.Contains("READ IN FULL, 1, one per name the workbook draws from, the first in name order that lists an element", lines);
            Assert.Contains("  The rows of DM-11-(610) SOFTSCAPE SCHEDULE were not read. The table refused.", lines);
        }

        /// <summary>
        /// A section whose read threw prints NOT READ and nothing that reads as a measured
        /// absence. NOT FOUND with the near misses was printed for an empty fallback, which
        /// said the model holds nothing when the truth was that nothing was looked for.
        /// </summary>
        [Fact]
        public void ASectionWhoseReadThrewPrintsNotReadAndNeverNotFound()
        {
            KpiScan scan = new KpiScan(
                new DocumentFacts("NG05", "", 0, 0, 0.0, null, null),
                null,
                TitleBlockFacts.NotRead("InvalidOperationException: refused"),
                LinkFacts.NotRead("InvalidOperationException: refused"),
                ScheduleFacts.NotRead("InvalidOperationException: refused"),
                new[] { "Section 3 TITLE BLOCKS AND SHEETS was not read. InvalidOperationException: refused" },
                false);
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(7, lines.Count(line => line.StartsWith(KpiReport.NotReadLine, StringComparison.Ordinal)));
            // The section 9 column line always says what NOT FOUND means, so the check is on
            // the answer lines and the section body lines, which are the two places a measured
            // absence would be claimed.
            Assert.DoesNotContain(lines, line => line.StartsWith("   NOT FOUND.", StringComparison.Ordinal) && !line.Contains("NOT READ"));
            Assert.DoesNotContain(lines, line => line.EndsWith(": NOT FOUND", StringComparison.Ordinal));
            Assert.Contains("   NOT FOUND. NOT READ. Section 3 TITLE BLOCKS AND SHEETS was not read, see READS THAT DID NOT HAPPEN at the top.", lines);
        }

        /// <summary>
        /// The list of headings holding EXISTING or PROPOSED is matched on the heading alone.
        /// A schedule named for PROPOSED with no such heading has nothing on it.
        /// </summary>
        [Fact]
        public void SectionSevenMatchesTheHeadingAloneAndNotTheScheduleName()
        {
            KpiScan scan = KpiFixture.Build(schedules: KpiFixture.Schedules(new[]
            {
                KpiFixture.Schedule("PROPOSED TREES: COUNT",
                    fields: new[] { KpiFixture.Field("BOTANICAL NAME"), KpiFixture.Field("QTY") }),
                KpiFixture.Schedule("DM-11-(610) SOFTSCAPE SCHEDULE",
                    fields: new[] { KpiFixture.Field("ENTER EACH EXISTING TREE QUANTITY") })
            }));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[] { "  (610) SOFTSCAPE SCHEDULE: ENTER EACH EXISTING TREE QUANTITY" },
                UntilBlank(lines, "COLUMN HEADINGS HOLDING EXISTING OR PROPOSED, 1, plot taken off"));
        }
    }
}
