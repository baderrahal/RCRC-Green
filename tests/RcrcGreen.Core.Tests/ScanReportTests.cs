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
                    new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 2, "DM-11"),
                    new ScannedSheet("DM-41-(200) General Arrangement Layout", "DM-41-(200) General Arrangement Layout", 1, "DM-11")
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
            Assert.Equal(
                "== VIEWPORTS ON EXISTING SHEETS (0) ==",
                HeadingIn(report, "VIEWPORTS ON EXISTING SHEETS"));
            Assert.Equal("== SCOPE BOXES (2) ==", HeadingIn(report, "SCOPE BOXES"));
            Assert.Equal("== PRX_Plot_ID VALUES (2) ==", HeadingIn(report, "PRX_Plot_ID VALUES"));
            Assert.Equal("== PARSE SUMMARY (5) ==", HeadingIn(report, "PARSE SUMMARY"));
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
                    "== VIEW FAMILY TYPES (0) ==",
                    "== VIEW FAMILY TYPE PER VIEW TYPE (2) ==",
                    "== VIEWPORTS ON EXISTING SHEETS (0) ==",
                    "== SCOPE BOXES (2) ==",
                    "== PRX_Plot_ID VALUES (2) ==",
                    "== VIEWS THAT DISAGREE WITH THEMSELVES (0) ==",
                    "== PARSE SUMMARY (5) =="
                },
                headings);
        }

        /// <summary>
        /// The placements read off real sheets, in millimetres, next to the sheet's own size.
        /// One foot is 304.8 millimetres, and every expected number below is that arithmetic
        /// done by hand.
        /// </summary>
        [Fact]
        public void APlacementOnAnExistingSheetIsListedInMillimetres()
        {
            ModelScan scan = ScanFixture.Build(viewports: new[]
            {
                new ViewportRecord(
                    "200Q", "GENERAL ARRANGEMENT LAYOUT",
                    "DM-11-(200) General Arrangement Layout",
                    250, 1.0, 0.5, 2.0, 1.0, 2.0, 1.0, false)
            });

            string report = ScanReport.Write(scan, Noon);

            Assert.Contains("== VIEWPORTS ON EXISTING SHEETS (1) ==", report);
            Assert.Contains(
                "200Q | DM-11-(200) General Arrangement Layout | 1:250 | 304.8 by 152.4 mm | "
                + "609.6 by 304.8 mm | 609.6 by 304.8 mm",
                report);
            Assert.Contains("What the team's own sheets look like", report);
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
                sheets: new[] { new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 1, "DM-11") },
                plotIdValues: new[] { new ScannedParameterValue("DM-41", 1) });

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));

            Assert.Contains("600QD | SOFTSCAPE SCHEDULES | 1 view | DM-11", lines);
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
        public void TheParseSummaryCountsViewNamesAndNamesWhatItRefused()
        {
            string[] lines = LinesOf(ScanReport.Write(RealShapedScan(), Noon));

            Assert.Contains("view name: 2 of 5 parsed, 3 did not", lines);
            Assert.Contains("  NG05", lines);

            // Sheets are not tallied. Their names and numbers are not shaped like a view
            // name, so the tally read 0 of 1385 parsed on every scan and named a fault in the
            // model that was not one.
            Assert.DoesNotContain(lines, line => line.StartsWith("sheet name:", StringComparison.Ordinal));
            Assert.DoesNotContain(lines, line => line.StartsWith("sheet number:", StringComparison.Ordinal));
            Assert.DoesNotContain("  600QD", lines);
            Assert.DoesNotContain("  SOFTSCAPE SCHEDULES", lines);
            Assert.Contains(
                "Sheet names and sheet numbers are not read here either. A sheet is named after "
                + "its view with no plot and no code, and numbered by code, plot letter and sheet "
                + "letter, so neither is shaped like a view name and a tally of them counted "
                + "failures nobody could act on.",
                lines);
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
                    new ScannedSheet("DM-100", "one", 0, "DM-11"),
                    new ScannedSheet("DM-2", "two", 0, "DM-11"),
                    new ScannedSheet("DM-9", "three", 0, "DM-11")
                });

            string[] lines = LinesOf(ScanReport.Write(scan, Noon));
            string[] rows = lines
                .Where(line => line.StartsWith("DM-", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "DM-2 | two | 0 views | DM-11",
                    "DM-9 | three | 0 views | DM-11",
                    "DM-100 | one | 0 views | DM-11"
                },
                rows);
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

    /// <summary>
    /// The two sections the first real run showed were missing. The scan listed view templates
    /// and not view family types, so a name mismatch that cost three refusals could only be
    /// found in a Properties panel, and it counted disagreeing views without naming one.
    /// </summary>
    public class ScanReportNewSectionsTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 8, 12, 0, 0);

        [Fact]
        public void EveryViewFamilyTypeIsListedWithItsViewFamily()
        {
            string report = ScanReport.Write(
                ScanFixture.Build(viewFamilyTypes: new[]
                {
                    new ScannedViewFamilyType("(200) General Arrangement Layout", "FloorPlan"),
                    new ScannedViewFamilyType("(010) Key Location Plan", "FloorPlan"),
                    new ScannedViewFamilyType("(400) Landscape Cross Section", "Section")
                }),
                Noon);

            Assert.Contains("== VIEW FAMILY TYPES (3) ==", report);
            Assert.Contains("view family | type name", report);
            Assert.Contains("Section | (400) Landscape Cross Section", report);
            Assert.Contains("FloorPlan | (010) Key Location Plan", report);
        }

        /// <summary>
        /// The mismatch that cost the first run. The view is called Location Key Plan and its
        /// family type is called Key Location Plan, and nothing but this section shows it.
        /// </summary>
        [Fact]
        public void TheTypeNameDoesNotHaveToMatchTheViewNamesThatUseIt()
        {
            string report = ScanReport.Write(
                ScanFixture.Build(
                    views: new[] { ScanFixture.View("DM-11-(010) Location Key Plan", "FloorPlan") },
                    viewFamilyTypes: new[]
                    {
                        new ScannedViewFamilyType("(010) Key Location Plan", "FloorPlan")
                    }),
                Noon);

            Assert.Contains("FloorPlan | (010) Key Location Plan", report);
            Assert.Contains("A type name does not have to match", report);
        }

        [Fact]
        public void EveryDisagreeingViewIsNamedWithBothPlots()
        {
            string report = ScanReport.Write(
                ScanFixture.Build(disagreements: new[]
                {
                    new ScannedDisagreement("DM-12-(200) General Arrangement Layout", "DM-12", "DM-11"),
                    new ScannedDisagreement("DM-11-(010) Location Key Plan", "DM-11", "DM-41")
                }),
                Noon);

            Assert.Contains("== VIEWS THAT DISAGREE WITH THEMSELVES (2) ==", report);
            Assert.Contains("view name | plot in the name | plot in PRX_Plot_ID", report);
            Assert.Contains("DM-12-(200) General Arrangement Layout | DM-12 | DM-11", report);
            Assert.Contains("DM-11-(010) Location Key Plan | DM-11 | DM-41", report);
        }

        /// <summary>
        /// Plots read as numbers, so DM-2 comes before DM-100 in the list somebody works down.
        /// </summary>
        [Fact]
        public void TheyComeInPlotOrderWithTheNumberReadAsANumber()
        {
            string report = ScanReport.Write(
                ScanFixture.Build(disagreements: new[]
                {
                    new ScannedDisagreement("DM-100-(200) A", "DM-100", "DM-1"),
                    new ScannedDisagreement("DM-2-(200) A", "DM-2", "DM-1"),
                    new ScannedDisagreement("DM-11-(200) A", "DM-11", "DM-1")
                }),
                Noon);

            int two = report.IndexOf("DM-2-(200) A", StringComparison.Ordinal);
            int eleven = report.IndexOf("DM-11-(200) A", StringComparison.Ordinal);
            int hundred = report.IndexOf("DM-100-(200) A", StringComparison.Ordinal);

            Assert.True(two < eleven);
            Assert.True(eleven < hundred);
        }

        /// <summary>
        /// The scan reads and never writes, so the section has to say plainly that it changed
        /// none of them and that which value is right is not the tool's to decide.
        /// </summary>
        [Fact]
        public void TheSectionSaysNothingWasChanged()
        {
            string report = ScanReport.Write(ScanFixture.Build(), Noon);

            Assert.Contains("== VIEWS THAT DISAGREE WITH THEMSELVES (0) ==", report);
            Assert.Contains("Nothing here was changed.", report);
        }
    }

    /// <summary>
    /// Two counts about the shape of the sheet set. Only plot DM-11 has real sheet numbers in
    /// the first model. Every other plot carries numbers like 010QE Copy 001, and a group of
    /// sheets carries no PRX_Plot_ID at all, which is what the Sheet List filters on.
    /// </summary>
    public class ScanReportSheetShapeTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 8, 12, 0, 0);

        private static string Report(params ScannedSheet[] sheets)
        {
            return ScanReport.Write(ScanFixture.Build(sheets: sheets), Noon);
        }

        [Fact]
        public void SheetsNumberedAsADuplicateAreCounted()
        {
            string report = Report(
                new ScannedSheet("DM-11-010", "Location Key Plan", 1, "DM-11"),
                new ScannedSheet("010QE Copy 001", "Location Key Plan", 0, "DM-12"),
                new ScannedSheet("400Q Copy 009", "Cross Section", 0, "DM-13"));

            Assert.Contains(
                "2 of them are numbered as a duplicate, meaning the number holds Copy.", report);
        }

        [Fact]
        public void SheetsCarryingNoPlotAreCounted()
        {
            string report = Report(
                new ScannedSheet("DM-11-010", "Location Key Plan", 1, "DM-11"),
                new ScannedSheet("Z-001", "Cover", 0, string.Empty),
                new ScannedSheet("Z-002", "Notes", 0, string.Empty));

            Assert.Contains(
                "2 of them carry no PRX_Plot_ID, so the Sheet List will not find them under any "
                + "plot.", report);
        }

        [Fact]
        public void BothCountsReadZeroOnASetWithNeitherProblem()
        {
            string report = Report(new ScannedSheet("DM-11-010", "Location Key Plan", 1, "DM-11"));

            Assert.Contains("0 of them are numbered as a duplicate", report);
            Assert.Contains("0 of them carry no PRX_Plot_ID", report);
        }

        /// <summary>
        /// Copy is matched exactly, so a sheet whose name happens to hold the word copyright or
        /// a lower case copy is not counted as a duplicate.
        /// </summary>
        [Fact]
        public void OnlyTheNumberIsSearchedAndOnlyForThatExactWord()
        {
            string report = Report(
                new ScannedSheet("DM-11-010", "Copy of the key plan", 0, "DM-11"),
                new ScannedSheet("DM-11-020", "copy 3", 0, "DM-11"),
                new ScannedSheet("DM-11-copy-030", "Notes", 0, "DM-11"));

            Assert.Contains("0 of them are numbered as a duplicate", report);
        }

        [Fact]
        public void TheRowSaysWhichPlotEachSheetCarriesAndNoneWhenItCarriesNothing()
        {
            string report = Report(
                new ScannedSheet("DM-11-010", "Location Key Plan", 1, "DM-11"),
                new ScannedSheet("Z-001", "Cover", 0, string.Empty));

            Assert.Contains("DM-11-010 | Location Key Plan | 1 view | DM-11", report);
            Assert.Contains("Z-001 | Cover | 0 views | (none)", report);
        }

        [Fact]
        public void TheSectionSaysItChangedNothing()
        {
            Assert.Contains("Nothing here was changed. This is a count of what is there.", Report());
        }
    }
}

