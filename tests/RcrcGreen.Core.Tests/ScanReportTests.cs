using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScanReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 8, 14, 5, 0);

        private static string[] LinesOf(string report)
        {
            return report.Split(new[] { ScanReport.LineEnd }, StringSplitOptions.None);
        }

        private static string HeadingIn(string report, string title)
        {
            return LinesOf(report).Single(line => line.StartsWith("== " + title + " (", StringComparison.Ordinal));
        }

        private static ModelScan RealShapedScan()
        {
            return ScanFixture.Build(
                documentTitle: "NG05",
                sheets: new[]
                {
                    new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 2),
                    new ScannedSheet("DM-41-(200) General Arrangement Layout", "DM-41-(200) General Arrangement Layout", 1)
                },
                views: new[]
                {
                    ScanFixture.ViewOn("600QD", "Softscape Schedule 01", "Schedule"),
                    ScanFixture.ViewOn("600QD", "Softscape Schedule 02", "Schedule"),
                    ScanFixture.ViewOn("DM-41-(200) General Arrangement Layout", "DM-41-(200) General Arrangement Layout", "FloorPlan"),
                    ScanFixture.View("NG05", "ThreeD"),
                    ScanFixture.View("DM-41-(010) Location Key Plan", "FloorPlan"),
                    ScanFixture.Template("Landscape Plan Template", "FloorPlan")
                },
                scopeBoxes: new[]
                {
                    ScanFixture.Box("DM-41", 0, 0, 300, 80),
                    ScannedScopeBox.WithoutBounds("PF-12")
                },
                plotIdValues: new[]
                {
                    new ScannedParameterValue("DM-41", 812),
                    new ScannedParameterValue("", 4)
                },
                elementsScanned: 128431,
                scanSeconds: 4.23);
        }

        [Fact]
        public void EverySectionHeadingCarriesItsOwnCount()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);

            Assert.Equal("== SHEETS (2) ==", HeadingIn(report, "SHEETS"));
            Assert.Equal("== VIEWS ON SHEETS (3) ==", HeadingIn(report, "VIEWS ON SHEETS"));
            Assert.Equal("== VIEWS NOT ON SHEETS (2) ==", HeadingIn(report, "VIEWS NOT ON SHEETS"));
            Assert.Equal("== VIEW TEMPLATES (1) ==", HeadingIn(report, "VIEW TEMPLATES"));
            Assert.Equal("== SCOPE BOXES (2) ==", HeadingIn(report, "SCOPE BOXES"));
            Assert.Equal("== PRX_Plot_ID VALUES (2) ==", HeadingIn(report, "PRX_Plot_ID VALUES"));
            Assert.Equal("== PARSE SUMMARY (9) ==", HeadingIn(report, "PARSE SUMMARY"));
        }

        [Fact]
        public void TheSectionsComeInTheOrderTheBriefAsksFor()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);

            string[] headings = LinesOf(report)
                .Where(line => line.StartsWith("== ", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "== SHEETS (2) ==",
                    "== VIEWS ON SHEETS (3) ==",
                    "== VIEWS NOT ON SHEETS (2) ==",
                    "== VIEW TEMPLATES (1) ==",
                    "== SCOPE BOXES (2) ==",
                    "== PRX_Plot_ID VALUES (2) ==",
                    "== PARSE SUMMARY (9) =="
                },
                headings);
        }

        [Fact]
        public void ATemplateIsNeverCountedAmongTheViews()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);
            string[] lines = LinesOf(report);

            int templatesAt = Array.FindIndex(lines, line => line.StartsWith("== VIEW TEMPLATES", StringComparison.Ordinal));
            int templateLine = Array.FindIndex(lines, line => line.StartsWith("Landscape Plan Template |", StringComparison.Ordinal));

            Assert.True(templateLine > templatesAt, "the template is listed under the templates heading");
            Assert.Single(lines, line => line.StartsWith("Landscape Plan Template |", StringComparison.Ordinal));
        }

        [Fact]
        public void AViewOnASheetIsListedWithItsSheetNumberAndNotAmongTheLooseOnes()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);
            string[] lines = LinesOf(report);

            Assert.Contains("600QD | Softscape Schedule 01 | Schedule", lines);
            Assert.DoesNotContain("Softscape Schedule 01 | Schedule", lines);
        }

        [Fact]
        public void AScopeBoxWithNoBoundingBoxSaysSoRatherThanPrintingZeros()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);
            string[] lines = LinesOf(report);

            Assert.Contains("DM-41 | 0 0 0 | 300 80 30", lines);
            Assert.Contains("PF-12 | no bounding box | no bounding box", lines);
        }

        [Fact]
        public void AnEmptyPlotIdValueIsShownRatherThanLeftAsABlankColumn()
        {
            string[] lines = LinesOf(ScanReport.Write(RealShapedScan(), Noon));

            Assert.Contains("(empty) | 4 elements", lines);
            Assert.Contains("DM-41 | 812 elements", lines);
        }

        [Fact]
        public void OneOfAThingIsNotCalledOneThings()
        {
            ModelScan scan = ScanFixture.Build(
                sheets: new[] { new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 1) },
                plotIdValues: new[] { new ScannedParameterValue("DM-41", 1) });

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));

            Assert.Contains("600QD | SOFTSCAPE SCHEDULES | 1 view", lines);
            Assert.Contains("DM-41 | 1 element", lines);
        }

        [Fact]
        public void TheHeaderNamesTheDocumentTheTimeAndWhatTheScanCost()
        {
            string[] lines = LinesOf(ScanReport.Write(RealShapedScan(), Noon));

            Assert.Contains("Document: NG05", lines);
            Assert.Contains("Written: 2026-09-08 14:05", lines);
            Assert.Contains("Read only. Nothing in the model was changed.", lines);
            Assert.Contains("Elements read looking for PRX_Plot_ID: 128431, in 4.2 seconds", lines);
        }

        [Fact]
        public void TheParseSummaryCountsEachKindAndNamesWhatItRefused()
        {
            string[] lines = LinesOf(ScanReport.Write(RealShapedScan(), Noon));

            Assert.Contains("view name: 2 of 5 parsed, 3 did not", lines);
            Assert.Contains("sheet name: 1 of 2 parsed, 1 did not", lines);
            Assert.Contains("sheet number: 1 of 2 parsed, 1 did not", lines);

            Assert.Contains("  600QD", lines);
            Assert.Contains("  SOFTSCAPE SCHEDULES", lines);
            Assert.Contains("  NG05", lines);
        }

        [Fact]
        public void TheRefusedListStopsAtTwentyAndSaysHowManyThereWere()
        {
            ModelScan scan = ScanFixture.Build(
                views: Enumerable.Range(1, 45)
                    .Select(number => ScanFixture.View("Working view " + number, "FloorPlan"))
                    .ToArray());

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));

            Assert.Contains("Did not parse, view name, showing 20 of 45:", lines);
            Assert.Equal(20, lines.Count(line => line.StartsWith("  Working view ", StringComparison.Ordinal)));
        }

        [Fact]
        public void AKindWithNothingRefusedGetsNoListAtAll()
        {
            ModelScan scan = ScanFixture.Build(
                views: new[] { ScanFixture.View("DM-41-(010) Location Key Plan", "FloorPlan") });

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));

            Assert.Contains("view name: 1 of 1 parsed, 0 did not", lines);
            Assert.DoesNotContain(lines, line => line.StartsWith("Did not parse,", StringComparison.Ordinal));
        }

        [Fact]
        public void RowsComeOutWithTheNumberPartInNumberOrder()
        {
            ModelScan scan = ScanFixture.Build(
                sheets: new[]
                {
                    new ScannedSheet("DM-100", "one", 0),
                    new ScannedSheet("DM-2", "two", 0),
                    new ScannedSheet("DM-9", "three", 0)
                });

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));
            string[] rows = lines
                .Where(line => line.StartsWith("DM-", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(new[] { "DM-2 | two | 0 views", "DM-9 | three | 0 views", "DM-100 | one | 0 views" }, rows);
        }

        [Fact]
        public void AnEmptyModelStillWritesEverySectionWithAZeroInIt()
        {
            string report = ScanReport.Write(ScanFixture.Build(), Noon);

            Assert.Equal("== SHEETS (0) ==", HeadingIn(report, "SHEETS"));
            Assert.Equal("== VIEWS ON SHEETS (0) ==", HeadingIn(report, "VIEWS ON SHEETS"));
            Assert.Equal("== VIEWS NOT ON SHEETS (0) ==", HeadingIn(report, "VIEWS NOT ON SHEETS"));
            Assert.Equal("== VIEW TEMPLATES (0) ==", HeadingIn(report, "VIEW TEMPLATES"));
            Assert.Equal("== SCOPE BOXES (0) ==", HeadingIn(report, "SCOPE BOXES"));
            Assert.Equal("== PRX_Plot_ID VALUES (0) ==", HeadingIn(report, "PRX_Plot_ID VALUES"));
            Assert.Equal("== PARSE SUMMARY (0) ==", HeadingIn(report, "PARSE SUMMARY"));
        }

        [Fact]
        public void EveryLineEndsTheWayNotepadOnWindowsWantsIt()
        {
            string report = ScanReport.Write(RealShapedScan(), Noon);

            Assert.EndsWith(ScanReport.LineEnd, report);
            Assert.DoesNotContain(report.Replace(ScanReport.LineEnd, string.Empty), "\n");
        }
    }
}
