using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The four things this round put in the scan report: a near miss shown as well as named,
    /// the four plot parameters side by side, the plot on a filled region, and three schedules
    /// of one name read in full rather than one.
    ///
    /// Every expected line is written out by hand. Working one out with the rule the report
    /// uses would read the code back to itself.
    /// </summary>
    public class KpiScanAdditionsTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 9, 14, 5, 12);

        private static string[] LinesOf(string report)
        {
            return report.Split(new[] { "\r\n" }, StringSplitOptions.None);
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

        /// <summary>
        /// PRX_COMPONENT is not on the first real model. The sheet carries PRX_Component, one
        /// capital apart, and the report named it while printing not one of its values.
        /// </summary>
        private static KpiScan ANearMissWithValues()
        {
            SheetValue[] values = Enumerable.Range(1, 25)
                .Select(n => KpiFixture.Value("PRX_Component", "S" + n, "Sheet " + n, n <= 15 ? "SOFTSCAPE" : ""))
                .ToArray();

            return KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 25,
                titleBlockInstances: 25,
                onInstances: KpiFixture.OnInstances(
                    25,
                    new[] { KpiFixture.Tally("PRX_Component", 25, 15), KpiFixture.Tally("PRX_Plot_UID", 25, 0) },
                    values)));
        }

        [Fact]
        public void ANearMissNamedUnderANotFoundIsShownWithItsOwnValues()
        {
            string[] lines = LinesOf(KpiReport.Write(ANearMissWithValues(), Noon));

            string[] shown = UntilBlank(lines, "PRX_COMPONENT on the title block instance: NOT FOUND");

            Assert.Equal(
                "  Names on the title block instance holding COMPONENT, PLOT or UID: PRX_Component, PRX_Plot_UID",
                shown[0]);
            Assert.Equal(
                "  PRX_Component on the title block instance, showing 20 of 25, the ones with a value first:",
                shown[1]);
            Assert.Equal("  sheet number | sheet name | value", shown[2]);
        }

        [Fact]
        public void ANearMissShowsTwentyOfItsValuesWithTheOnesHoldingAValueFirst()
        {
            string[] lines = LinesOf(KpiReport.Write(ANearMissWithValues(), Noon));

            string[] shown = UntilBlank(lines, "PRX_COMPONENT on the title block instance: NOT FOUND");
            string[] examples = shown.Skip(3).Take(20).ToArray();

            Assert.Equal(24, shown.Length);
            Assert.Equal("  S1 | Sheet 1 | SOFTSCAPE", examples[0]);
            Assert.Equal("  S15 | Sheet 15 | SOFTSCAPE", examples[14]);
            Assert.Equal("  S16 | Sheet 16 | (empty)", examples[15]);
            Assert.Equal("  S20 | Sheet 20 | (empty)", examples[19]);
        }

        [Fact]
        public void ANearMissWithATallyAndNoValueSaysSoRatherThanPrintingNothing()
        {
            string[] lines = LinesOf(KpiReport.Write(ANearMissWithValues(), Noon));

            string[] shown = UntilBlank(lines, "PRX_COMPONENT on the title block instance: NOT FOUND");

            Assert.Equal("  PRX_Plot_UID on the title block instance: no value was read for it.", shown[23]);
        }

        /// <summary>
        /// One sheet carrying all four, one carrying two and one carrying a plot name that is
        /// there and empty. The empty one is listed first so the order under test is the
        /// report's and not the order the values arrived in.
        /// </summary>
        private static KpiScan FourPlotNamesOnThreeSheets()
        {
            return KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 3,
                onSheets: KpiFixture.OnSheets(
                    3,
                    new[]
                    {
                        KpiFixture.Tally("PRX_Plot_ID", 3, 3),
                        KpiFixture.Tally("PRX_Plot_UID", 1, 1),
                        KpiFixture.Tally("PRX_Plot_UID2", 2, 1),
                        KpiFixture.Tally("PRX_Plot_NH", 2, 2)
                    },
                    new[]
                    {
                        KpiFixture.Value("PRX_Plot_ID", "020QE", "PLANTING PLAN", "DM-12"),
                        KpiFixture.Value("PRX_Plot_UID2", "020QE", "PLANTING PLAN", ""),
                        KpiFixture.Value("PRX_Plot_ID", "010QE", "KEY PLAN", "DM-11"),
                        KpiFixture.Value("PRX_Plot_UID", "010QE", "KEY PLAN", "NG05-DM-11"),
                        KpiFixture.Value("PRX_Plot_UID2", "010QE", "KEY PLAN", "DM-11-A"),
                        KpiFixture.Value("PRX_Plot_NH", "010QE", "KEY PLAN", "NU05"),
                        KpiFixture.Value("PRX_Plot_ID", "030QE", "HARDSCAPE PLAN", "DM-41"),
                        KpiFixture.Value("PRX_Plot_NH", "030QE", "HARDSCAPE PLAN", "NU05")
                    })));
        }

        [Fact]
        public void TheFourPlotParametersPrintUnderOneHeadingAndOneColumnLine()
        {
            string[] lines = LinesOf(KpiReport.Write(FourPlotNamesOnThreeSheets(), Noon));

            Assert.Contains("THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE, 3 sheets, showing 3", lines);
            Assert.Equal(
                new[] { "  sheet number | PRX_Plot_ID | PRX_Plot_UID | PRX_Plot_UID2 | PRX_Plot_NH" },
                Following(lines, "THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE, 3 sheets, showing 3", 1));
        }

        /// <summary>
        /// A reader tells the four apart on a row that carries them all, so a full row goes
        /// above a partial one. A name that is not on the sheet and a name that is on it and
        /// empty are two different findings and print as two different words.
        /// </summary>
        [Fact]
        public void TheSheetCarryingAllFourPlotNamesPrintsBeforeThePartialOnes()
        {
            string[] lines = LinesOf(KpiReport.Write(FourPlotNamesOnThreeSheets(), Noon));

            Assert.Equal(
                new[]
                {
                    "  sheet number | PRX_Plot_ID | PRX_Plot_UID | PRX_Plot_UID2 | PRX_Plot_NH",
                    "  010QE | DM-11 | NG05-DM-11 | DM-11-A | NU05",
                    "  030QE | DM-41 | (not on it) | (not on it) | NU05",
                    "  020QE | DM-12 | (not on it) | (empty) | (not on it)"
                },
                UntilBlank(lines, "THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE, 3 sheets, showing 3"));
        }

        [Fact]
        public void TwentySideBySideRowsPrintWhereTwentyFiveSheetsCarryAPlotName()
        {
            SheetValue[] values = Enumerable.Range(1, 25)
                .Select(n => KpiFixture.Value("PRX_Plot_ID", "S" + n, "Sheet " + n, "DM-" + n))
                .ToArray();
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 25,
                onSheets: KpiFixture.OnSheets(25, new[] { KpiFixture.Tally("PRX_Plot_ID", 25, 25) }, values)));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            string[] shown = UntilBlank(lines,
                "THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE, 25 sheets, showing 20");

            Assert.Equal(21, shown.Length);
            Assert.Equal("  S1 | DM-1 | (not on it) | (not on it) | (not on it)", shown[1]);
            Assert.Equal("  S20 | DM-20 | (not on it) | (not on it) | (not on it)", shown[20]);
        }

        [Fact]
        public void NoPlotNameOnTheSheetSaysSoRatherThanPrintingAnEmptyTable()
        {
            KpiScan scan = KpiFixture.Build(titleBlocks: KpiFixture.TitleBlocks(
                sheetCount: 1,
                onSheets: KpiFixture.OnSheets(
                    1,
                    new[] { KpiFixture.Tally("PRX_COMPONENT", 1, 1) },
                    new[] { KpiFixture.Value("PRX_COMPONENT", "010QE", "KEY PLAN", "SOFTSCAPE") })));
            string[] lines = LinesOf(KpiReport.Write(scan, Noon));

            Assert.Equal(
                new[] { "  None of PRX_Plot_ID, PRX_Plot_UID, PRX_Plot_UID2, PRX_Plot_NH was read on the sheet." },
                UntilBlank(lines, "THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE, 0 sheets, showing 0"));
        }

        /// <summary>
        /// Six filled regions over four plots, two types, and one region carrying no plot at
        /// all. Which type is a plot's intervention area is settled by reading one plot's
        /// regions together, so the plot travels on every row.
        /// </summary>
        private static KpiScan RegionsCarryingTheirPlot()
        {
            var link = new LinkContents(
                KpiFixture.LinkInstanceName,
                "RCRC_NG05_NU_00_SITE_RVT24",
                6,
                new[] { KpiFixture.Counted("ID Intervention Area", 4), KpiFixture.Counted("Diagonal Hatch", 2) },
                new[] { KpiFixture.Counted("Site Plan", 6) },
                null,
                new[] { KpiFixture.Tally("PRX_Intervention Area", 6, 5), KpiFixture.Tally("Area", 6, 6) },
                new[]
                {
                    new MeasuredValue("ID Intervention Area", "Area", "10763.91", "1000.00 m²", "DM-11"),
                    new MeasuredValue("ID Intervention Area", "Area", "5381.96", "500.00 m²", "DM-11"),
                    new MeasuredValue("ID Intervention Area", "Area", "3229.17", "300.00 m²", "DM-12"),
                    new MeasuredValue("ID Intervention Area", "Area", "2152.78", "200.00 m²", "DM-41"),
                    new MeasuredValue("Diagonal Hatch", "Area", "1076.39", "100.00 m²", "PF-12"),
                    new MeasuredValue("Diagonal Hatch", "Area", "538.20", "50.00 m²", "")
                },
                new[] { KpiFixture.Counted("ID Intervention Area", 4), KpiFixture.Counted("Diagonal Hatch", 1) });

            return KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType(KpiFixture.LinkTypeName) },
                new[] { KpiFixture.LinkInstance(KpiFixture.LinkInstanceName, KpiFixture.LinkTypeName) },
                new[] { link }));
        }

        [Fact]
        public void AFilledRegionRowCarriesItsPlotBetweenTheTypeAndTheMeasure()
        {
            string[] lines = LinesOf(KpiReport.Write(RegionsCarryingTheirPlot(), Noon));

            Assert.Equal(
                new[]
                {
                    "  region type | plot | measures | raw | printed",
                    "  The raw number is square feet only where measures reads Area. A number typed by hand measures nothing and prints with no unit.",
                    "  ID Intervention Area | DM-11 | Area | 10763.91 | 1000.00 m²",
                    "  ID Intervention Area | DM-11 | Area | 5381.96 | 500.00 m²",
                    "  ID Intervention Area | DM-12 | Area | 3229.17 | 300.00 m²",
                    "  ID Intervention Area | DM-41 | Area | 2152.78 | 200.00 m²",
                    "  Diagonal Hatch | PF-12 | Area | 1076.39 | 100.00 m²",
                    "  Diagonal Hatch | (empty) | Area | 538.20 | 50.00 m²"
                },
                UntilBlank(lines, "  PRX_Intervention Area: on 6 of 6 filled regions, 5 with a value, showing 6 of 6:"));
        }

        [Fact]
        public void RegionsOfEachTypeCarryingThePlotPrintBesideTheCountOfThatType()
        {
            string[] lines = LinesOf(KpiReport.Write(RegionsCarryingTheirPlot(), Noon));

            Assert.Equal(
                new[]
                {
                    "  filled region type | regions with a plot | regions of that type",
                    "  Diagonal Hatch | 1 | 2",
                    "  ID Intervention Area | 4 | 4"
                },
                UntilBlank(lines, "  REGIONS OF EACH TYPE CARRYING PRX_Ref Plot ID, 2 types"));
        }

        [Fact]
        public void OnePlotsRegionsPrintTogetherAndOnlyThreePlotsPrint()
        {
            string[] lines = LinesOf(KpiReport.Write(RegionsCarryingTheirPlot(), Noon));

            Assert.Equal(
                new[]
                {
                    "  DM-11, 2 regions:",
                    "    ID Intervention Area | 10763.91 | 1000.00 m²",
                    "    ID Intervention Area | 5381.96 | 500.00 m²",
                    "  DM-12, 1 region:",
                    "    ID Intervention Area | 3229.17 | 300.00 m²",
                    "  DM-41, 1 region:",
                    "    ID Intervention Area | 2152.78 | 200.00 m²"
                },
                UntilBlank(lines, "  ONE PLOT'S REGIONS TOGETHER, first 3 of 4 plots"));
            Assert.DoesNotContain(lines, line => line.StartsWith("  PF-12, ", StringComparison.Ordinal));
        }

        /// <summary>
        /// Three plots of one schedule name, each read in full. The reader used to read one
        /// copy per name, and one plot's numbers cannot say whether the next plot's schedule
        /// is built the same way.
        /// </summary>
        private static KpiScan ThreePlotsOfOneScheduleName()
        {
            return KpiFixture.Build(schedules: KpiFixture.Schedules(
                new[] { OnePlotsSoftscape("DM-11"), OnePlotsSoftscape("DM-12"), OnePlotsSoftscape("DM-41") }));
        }

        private static ScannedSchedule OnePlotsSoftscape(string plot)
        {
            return KpiFixture.Schedule(
                plot + "-(610) SOFTSCAPE SCHEDULE",
                fields: new[] { KpiFixture.Field("BOTANICAL NAME") },
                filters: new[] { KpiFixture.Filter("PRX_Ref Plot ID", "Equals", plot) },
                phaseName: "New Construction",
                phaseFilterName: "Show All",
                rowsWereRead: true,
                bodyRowCount: 1,
                rows: new[] { new[] { "Acacia tortilis" } });
        }

        [Fact]
        public void ThreeSchedulesOfOneNameAreAllListedAsReadInFull()
        {
            string[] lines = LinesOf(KpiReport.Write(ThreePlotsOfOneScheduleName(), Noon));

            Assert.Equal(
                new[]
                {
                    "",
                    "DM-11-(610) SOFTSCAPE SCHEDULE, category Planting, phase New Construction, phase filter Show All",
                    "  fields in order, 1: heading | parameter | field type | measures | unit | hidden",
                    "  BOTANICAL NAME | BOTANICAL NAME | Instance | - | project unit | shown",
                    "  filters, 1: field | rule | value",
                    "  PRX_Ref Plot ID | Equals | DM-11",
                    "",
                    "DM-12-(610) SOFTSCAPE SCHEDULE, category Planting, phase New Construction, phase filter Show All",
                    "  fields in order, 1: heading | parameter | field type | measures | unit | hidden",
                    "  BOTANICAL NAME | BOTANICAL NAME | Instance | - | project unit | shown",
                    "  filters, 1: field | rule | value",
                    "  PRX_Ref Plot ID | Equals | DM-12",
                    "",
                    "DM-41-(610) SOFTSCAPE SCHEDULE, category Planting, phase New Construction, phase filter Show All",
                    "  fields in order, 1: heading | parameter | field type | measures | unit | hidden",
                    "  BOTANICAL NAME | BOTANICAL NAME | Instance | - | project unit | shown",
                    "  filters, 1: field | rule | value",
                    "  PRX_Ref Plot ID | Equals | DM-41"
                },
                Following(lines,
                    "READ IN FULL, 3, one per name the workbook draws from, the first in name order that lists an element",
                    18));
        }

        [Fact]
        public void SectionSixPrintsItsOwnBlockForEachOfTheThreeSchedules()
        {
            string[] lines = LinesOf(KpiReport.Write(ThreePlotsOfOneScheduleName(), Noon));

            Assert.Contains("== 6 SOFTSCAPE SCHEDULE FIELDS (3) ==", lines);
            foreach (string plot in new[] { "DM-11", "DM-12", "DM-41" })
            {
                string name = plot + "-(610) SOFTSCAPE SCHEDULE";
                Assert.Equal(1, lines.Count(line => string.Equals(line, name, StringComparison.Ordinal)));
                Assert.Equal(
                    new[]
                    {
                        "  fields in order: heading | parameter | field type | measures",
                        "  BOTANICAL NAME | BOTANICAL NAME | Instance | -",
                        "  No Count field."
                    },
                    Following(lines, name, 3));
            }
        }
    }
}
