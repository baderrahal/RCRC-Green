using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE TWO WAYS TO FILL STREETS H8**, K79. Made up numbers on a workbook shaped like the
    /// STREETS template. The hashes in the first test came off this same build, run on the
    /// code at 43c52ae before any of this round's files changed.
    /// </summary>
    public class StreetsH8Tests : IDisposable
    {
        private const string Sheet = "<Streets>";

        private const string Plot = "ST-01";

        private const string Uid = "ANH-000-ST-100001";

        private static readonly DateTime Today = new DateTime(2026, 11, 8);

        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private static StreetReferenceAnswer Street(StreetsH8Option option)
        {
            return StreetReferenceAnswer.Of(20.0, 330.5, "20", "330.5").WithH8(option);
        }

        private static KpiCreatePlan Plan(StreetReferenceAnswer street)
        {
            return KpiCreatePlan.Of(
                KpiTemplates.Streets,
                new AgreedValue(new[] { new PlotText(Plot, "STREET") }),
                new AgreedValue(new[] { new PlotText(Plot, Uid) }),
                "KING FAHD",
                StreetsH8.Area(Plot, new[] { WithRegion() }, street),
                Totalled.Adding(new[] { new PlotNumber(Plot, 40.0) }),
                Totalled.Adding(new[] { new PlotNumber(Plot, 12.0) }),
                null,
                "2026-11-08", "B RAHAL", "LANDSCAPE ARCHITECT",
                street,
                CreateFixture.NoLabels);
        }

        private static PlotReading WithRegion()
        {
            return CreateFixture.Plot(
                Plot,
                component: "STREET",
                uid2: Uid,
                regions: new[] { CreateFixture.Region(RegionChoice.TheNoteNames, 5120.75) });
        }

        private PatchOutcome Patched(KpiCreatePlan plan, out string output)
        {
            string source = StreetsWorkbookFixture.Create(_folder, "template.xlsx");
            output = Path.Combine(_folder, "filled.xlsx");

            PatchOutcome outcome = WorkbookPatcher.Patch(source, output, plan.Writes, plan.ComputesFrom);
            Assert.True(outcome.Written, outcome.Refusal);
            return outcome;
        }

        private static string Stored(KpiCreatePlan plan, string cell)
        {
            return plan.Writes.Single(one => one.Cell.ToString() == cell).Stored;
        }

        private PdfPlan Pdf(StreetReferenceAnswer street)
        {
            return PdfFill.Of(
                WithRegion(),
                KpiTemplates.Streets,
                CountedGroups.Of(KpiTemplates.Streets),
                street,
                Today,
                true,
                PdfWorkbookNumbers.None);
        }

        // ---------------------------------------------------------------- Option 1 unchanged

        /// <summary>
        /// **OPTION 1 IS BYTE FOR BYTE WHAT MAIN WROTE.** Every part of the built workbook
        /// against the SHA-256 main produced at 43c52ae, before this round touched a file, line
        /// endings made one kind because the gate saves the XML on Linux.
        /// </summary>
        [Fact]
        public void OptionOneWritesEveryPartExactlyAsMainDid()
        {
            string output;
            Patched(Plan(Street(StreetsH8Option.InterventionArea)), out output);

            var expected = new SortedDictionary<string, string>(StringComparer.Ordinal)
            {
                { "[Content_Types].xml", "cb2334425daa55f85e86c42b59d0e28d59fe1e9666eecd8043a559af776bdf44" },
                { "_rels/.rels", "b2d2291c3daf8f4f0a9cf5a5c34d71a846584595011ff864594cf5b039cfc99a" },
                { "xl/_rels/workbook.xml.rels", "de404f2e52a302a6ca104f0b37999513faf31fa85e8ba1e9831fb23bb385ddbc" },
                { "xl/workbook.xml", "67f4416b83f2b0b64477ca540142069b4c1d5332697ccc263eb98d996d7a20fa" },
                { "xl/worksheets/sheet1.xml", "690ec4decca4d9c2b36b56b3c0bf0d3d2b1e1e6866590aa78af985c6779aa2d1" }
            };

            Assert.Equal(expected, StreetsWorkbookFixture.PartHashes(output));
        }

        [Fact]
        public void AnAnswerNobodySetAnOptionOnIsOptionOne()
        {
            StreetReferenceAnswer plain = StreetReferenceAnswer.Of(20.0, 330.5, "20", "330.5");

            Assert.Equal(StreetsH8Option.InterventionArea, plain.H8);
            Assert.False(plain.UsesRowTimesLength);
            Assert.Equal("5120.75", Stored(Plan(plain), "H8"));
        }

        // ---------------------------------------------------------------- the formula write

        [Fact]
        public void AFormulaCellHoldsTheFormulaAndNoResult()
        {
            string output;
            PatchOutcome outcome = Patched(Plan(Street(StreetsH8Option.RowTimesLength)), out output);

            string main = WorkbookFixture.PartText(output, StreetsWorkbookFixture.MainSheetPart);

            Assert.Contains("<f>F8*D8</f>", main);
            Assert.DoesNotContain("<f>F8*D8</f><v>", main);
            Assert.DoesNotContain("<v>6610</v>", main);
            Assert.Contains(outcome.Landed, one => one.Cell == "H8" && one.Value == "=F8*D8");
        }

        /// <summary>
        /// Excel only works a formula out on open if the file says so, and K79 leans on the
        /// patcher's own three flags rather than adding anything.
        /// </summary>
        [Fact]
        public void TheWorkbookRecalculatesFullyOnOpen()
        {
            string output;
            PatchOutcome outcome = Patched(Plan(Street(StreetsH8Option.RowTimesLength)), out output);

            Assert.True(outcome.Cache.RecalculatesOnOpen);
            Assert.Equal("0", outcome.Cache.CalcId);
            Assert.Equal(0, outcome.Cache.FormulaCellsCarryingACachedValue);
            Assert.True(outcome.Cache.WillRecalculate);
        }

        /// <summary>
        /// The client's cell style stays on the cell.
        /// </summary>
        [Fact]
        public void TheFormulaKeepsTheCellsStyle()
        {
            string output;
            Patched(Plan(Street(StreetsH8Option.RowTimesLength)), out output);

            Assert.Contains("<c r=\"H8\" s=\"9\">", WorkbookFixture.PartText(output, StreetsWorkbookFixture.MainSheetPart));
        }

        [Theory]
        [InlineData("=F8*D8")]
        [InlineData("F8*D8")]
        [InlineData("  =F8*D8 ")]
        public void AFormulaIsStoredWithNoEqualsSign(string given)
        {
            CellWrite write = CellWrite.Formula(Sheet, "H8", given);

            Assert.Equal("F8*D8", write.Stored);
            Assert.True(write.IsFormula);
            Assert.False(write.IsText);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("=")]
        public void AnEmptyFormulaIsRefused(string given)
        {
            Assert.Throws<ArgumentException>(() => CellWrite.Formula(Sheet, "H8", given));
        }

        [Fact]
        public void NeitherOfTheOtherTwoWritesIsAFormula()
        {
            Assert.False(CellWrite.Number(Sheet, "D8", 20).IsFormula);
            Assert.False(CellWrite.Text(Sheet, "D3", "STREET").IsFormula);
        }

        // ---------------------------------------------------------------- Option 2

        [Fact]
        public void OptionTwoWritesTheFormulaAndTheTwoCellsItReads()
        {
            KpiCreatePlan plan = Plan(Street(StreetsH8Option.RowTimesLength));

            CellWrite area = plan.Writes.Single(one => one.Cell.ToString() == "H8");
            Assert.True(area.IsFormula);
            Assert.Equal("F8*D8", area.Stored);
            Assert.Equal("20", Stored(plan, "D8"));
            Assert.Equal("330.5", Stored(plan, "F8"));
        }

        /// <summary>
        /// **THE REVIT AREA IS NEVER THE ANSWER UNDER OPTION 2.** 20 x 330.5 is 6610, written out
        /// by hand, and the plot's region holds 5120.75 so a slip shows.
        /// </summary>
        [Fact]
        public void OptionTwoTakesDTimesFForTheToolsOwnArea()
        {
            StreetReferenceAnswer street = Street(StreetsH8Option.RowTimesLength);

            Totalled area = StreetsH8.Area(Plot, new[] { WithRegion() }, street);

            Assert.Equal(6610.0, area.Total);
            Assert.Equal(new[] { Plot }, area.PerPlot.Select(one => one.PlotId));
            Assert.True(area.Adds);
        }

        /// <summary>
        /// **THE ROADS FORM HAS NO AREA BOX**, only Row and Length, so the two numbers of the
        /// formula are what a STREETS PDF carries whichever option is picked, and it is the
        /// same form under both.
        /// </summary>
        [Theory]
        [InlineData(StreetsH8Option.InterventionArea)]
        [InlineData(StreetsH8Option.RowTimesLength)]
        public void TheRoadsPdfCarriesRowAndLengthAndNoAreaBox(StreetsH8Option option)
        {
            PdfPlan plan = Pdf(Street(option));

            Assert.DoesNotContain(plan.Fields, one => one.Value == PdfValue.Area);
            Assert.Equal("20", plan.Fields.Single(one => one.Value == PdfValue.Row).Text);
            Assert.Equal("0.3305", plan.Fields.Single(one => one.Value == PdfValue.Length).Text);
        }

        // ---------------------------------------------------------------- only STREETS

        [Fact]
        public void TheOptionMeansNothingOnAnyOtherTemplate()
        {
            foreach (KpiTemplate template in KpiTemplates.All.Where(one => !ReferenceEquals(one, KpiTemplates.Streets)))
            {
                Assert.False(StreetsH8.HasTheCells(template), template.Name);
                Assert.Equal(
                    StreetsH8Option.InterventionArea,
                    StreetsH8.Effective(template, StreetsH8Option.RowTimesLength));
                Assert.True(StreetsH8.ReadsRegions(template, StreetsH8Option.RowTimesLength), template.Name);
            }
        }

        [Fact]
        public void TheFormulaIsReadOffTheMap()
        {
            Assert.Equal("F8*D8", StreetsH8.Formula(KpiTemplates.Streets));
        }

        // ---------------------------------------------------------------- the region pick

        [Fact]
        public void OptionOneStillReadsRegionsOnStreets()
        {
            Assert.True(StreetsH8.ReadsRegions(KpiTemplates.Streets, StreetsH8Option.InterventionArea));
        }

        [Fact]
        public void OptionTwoReadsNoRegionOnStreets()
        {
            Assert.False(StreetsH8.ReadsRegions(KpiTemplates.Streets, StreetsH8Option.RowTimesLength));
        }

        private static PlotReading WithTwoDifferentRegions()
        {
            return CreateFixture.Plot(
                Plot,
                component: "STREET",
                uid2: Uid,
                regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                    CreateFixture.Region("RCRC_OTHER", 1200.0)
                });
        }

        /// <summary>
        /// **"TWO REGIONS HOLD AN AREA" IS NEVER ASKED UNDER OPTION 2.** Two regions of different
        /// areas, neither the note's type, on a run told no area is wanted.
        /// </summary>
        [Fact]
        public void OptionTwoNeverRefusesOverTwoRegions()
        {
            PlotReading reading = WithTwoDifferentRegions();
            Assert.Equal(2, reading.RegionsHoldingAnArea.Count);

            Reconciliation asked = Reconciliation.Of(
                new[] { Plot }, new[] { reading }, null, false, KpiTemplates.Streets,
                StreetsH8.ReadsRegions(KpiTemplates.Streets, StreetsH8Option.RowTimesLength), true);

            Assert.True(asked.AddsUp, string.Join(" ", asked.Refusals));
            Assert.Empty(asked.WithoutArea);
            Assert.False(asked.AreaWanted);
        }

        [Fact]
        public void OptionOneStillRefusesOverTwoRegions()
        {
            Reconciliation asked = Reconciliation.Of(
                new[] { Plot }, new[] { WithTwoDifferentRegions() }, null, false, KpiTemplates.Streets,
                StreetsH8.ReadsRegions(KpiTemplates.Streets, StreetsH8Option.InterventionArea), false);

            Assert.False(asked.AddsUp);
            Assert.Contains(asked.Refusals, one => one.Contains("Pick the one that is its intervention area."));
        }

        /// <summary>
        /// A street plot with no planting that the file gave an area for did contribute. Only
        /// the street file's own area keeps it off the list, never the formula alone.
        /// </summary>
        [Theory]
        [InlineData(true, 0)]
        [InlineData(false, 1)]
        public void APlotWithOnlyAStreetAreaHasContributed(bool areaWorkedOut, int listed)
        {
            PlotReading bare = CreateFixture.Plot(
                Plot, component: "STREET", uid2: Uid, regions: new RegionArea[0],
                softscapeRead: false, shrubsAndLawnRead: false);

            Reconciliation asked = Reconciliation.Of(
                new[] { Plot }, new[] { bare }, null, false, KpiTemplates.Streets, false, areaWorkedOut);

            Assert.Equal(listed, asked.ContributedNothing.Count);
        }

        /// <summary>
        /// The formula check said H8 held nothing, because a formula with no cached result has
        /// no text. It names what is there.
        /// </summary>
        [Fact]
        public void TheFormulaCheckSaysH8HoldsAFormula()
        {
            string output;
            PatchOutcome outcome = Patched(Plan(Street(StreetsH8Option.RowTimesLength)), out output);

            ComputedFrom area = outcome.Formulas.ComputesFrom.Single(one => one.Input.Cell.ToString() == "H8");
            Assert.True(area.Present);
            Assert.Equal("a formula", area.Holds);
        }

        [Fact]
        public void HeldReadingsAreNotReusedAcrossAChangeOfOption()
        {
            KpiCreateRun heldUnderOptionOne = CreateFixture.Run();

            ReadingsSource source = HeldReadings.Decide(
                heldUnderOptionOne, heldUnderOptionOne.DocumentTitle, heldUnderOptionOne.Template,
                heldUnderOptionOne.TemplatePath, heldUnderOptionOne.ComponentParameter,
                heldUnderOptionOne.ReferenceParameter, heldUnderOptionOne.Reconciliation.Ticked,
                StreetsH8Option.RowTimesLength);

            Assert.False(source.Reused);
            Assert.Equal("the STREETS H8 option changed since the run before, so the model was read again", source.Why);
        }

        // ---------------------------------------------------------------- nothing usable

        private static IEnumerable<object[]> NoUsableAnswer()
        {
            yield return new object[] { StreetReferenceAnswer.Nothing("ANH-000-ST-100001 is not in the street reference file") };
            yield return new object[] { StreetReferenceAnswer.WidthOnly(8.0, "8", "its ES_QUANTITY is no length") };
            yield return new object[] { StreetReferenceFile.NotSet.For(Uid) };
        }

        public static IEnumerable<object[]> NoUsableAnswers
        {
            get { return NoUsableAnswer(); }
        }

        [Theory]
        [MemberData(nameof(NoUsableAnswers))]
        public void WithNoUsableAnswerTheFormulaIsStillWritten(StreetReferenceAnswer answer)
        {
            KpiCreatePlan plan = Plan(answer.WithH8(StreetsH8Option.RowTimesLength));

            CellWrite area = plan.Writes.Single(one => one.Cell.ToString() == "H8");
            Assert.True(area.IsFormula);
            Assert.Equal("F8*D8", area.Stored);
        }

        /// <summary>
        /// **NOTHING IS TAKEN FROM REVIT FOR THAT PLOT.** The reading holds a region of 5120.75
        /// and the area stays empty.
        /// </summary>
        [Theory]
        [MemberData(nameof(NoUsableAnswers))]
        public void WithNoUsableAnswerTheAreaIsEmptyAndNothingComesFromRevit(StreetReferenceAnswer answer)
        {
            StreetReferenceAnswer street = answer.WithH8(StreetsH8Option.RowTimesLength);

            Totalled area = StreetsH8.Area(Plot, new[] { WithRegion() }, street);

            Assert.Empty(area.PerPlot);
            Assert.Equal(0.0, area.Total);
            Assert.Equal(answer.Why, street.WhyNoArea);
        }

        /// <summary>
        /// A width of nought is a number and no area, so the formula is written and the reason
        /// names the two cells rather than reporting an area of 0 as though it were measured.
        /// </summary>
        [Fact]
        public void ANoughtWidthGivesNoArea()
        {
            StreetReferenceAnswer street = StreetReferenceAnswer.Of(0.0, 330.5, "0", "330.5")
                .WithH8(StreetsH8Option.RowTimesLength);

            Assert.False(street.AreaWorkedOut);
            Assert.Equal("ROAD_WIDTH reads 0 and ES_QUANTITY reads 330.5, which gives no area", street.WhyNoArea);
            Assert.Empty(StreetsH8.Area(Plot, new[] { WithRegion() }, street).PerPlot);
        }

        /// <summary>
        /// A formula over two empty cells is a live workbook, and a percentage dividing by it
        /// must not refuse the write, because the team fills those two cells by hand.
        /// </summary>
        [Fact]
        public void AFormulaOverEmptyCellsDoesNotRefuseTheWrite()
        {
            StreetReferenceAnswer street = StreetReferenceAnswer.Nothing("no row").WithH8(StreetsH8Option.RowTimesLength);

            string output;
            PatchOutcome outcome = Patched(Plan(street), out output);

            Assert.True(outcome.Written, outcome.Refusal);
            Assert.False(outcome.Formulas.RefusesTheWrite);
            Assert.Contains("<f>F8*D8</f>", WorkbookFixture.PartText(output, StreetsWorkbookFixture.MainSheetPart));
        }

        // ---------------------------------------------------------------- the setting and the words

        [Theory]
        [InlineData("ROW x Length", StreetsH8Option.RowTimesLength)]
        [InlineData("row x length", StreetsH8Option.RowTimesLength)]
        [InlineData("  ROW x Length\r\n", StreetsH8Option.RowTimesLength)]
        [InlineData("Intervention area", StreetsH8Option.InterventionArea)]
        [InlineData("", StreetsH8Option.InterventionArea)]
        [InlineData(null, StreetsH8Option.InterventionArea)]
        [InlineData("anything else", StreetsH8Option.InterventionArea)]
        public void TheFileReadsBackAsTheOptionAndDefaultsToOne(string held, StreetsH8Option expected)
        {
            Assert.Equal(expected, StreetsH8.Parse(held));
        }

        [Theory]
        [InlineData(StreetsH8Option.InterventionArea, "Intervention area")]
        [InlineData(StreetsH8Option.RowTimesLength, "ROW x Length")]
        public void TheNameRoundTrips(StreetsH8Option option, string name)
        {
            Assert.Equal(name, StreetsH8.Name(option));
            Assert.Equal(option, StreetsH8.Parse(StreetsH8.Name(option)));
        }

        /// <summary>The pane rule: no line over 10 words, longer text in the tooltip.</summary>
        [Fact]
        public void ThePaneLinesAreTenWordsOrLess()
        {
            foreach (string line in new[]
            {
                KpiPaneWords.StreetsH8Title,
                KpiPaneWords.InterventionArea,
                KpiPaneWords.RowTimesLength
            })
            {
                Assert.True(line.Split(' ').Length <= 10, line);
            }
        }

        [Fact]
        public void TheReportsFirstLinesNameTheOption()
        {
            Assert.Equal(
                "STREETS H8: ROW x Length, the formula =F8*D8. A street plot's area is D8 x F8, never "
                + "the Revit area, and no filled region is read for it. Other templates are unchanged.",
                StreetsH8.InWords(StreetsH8Option.RowTimesLength));
            Assert.Equal(
                "STREETS H8: Intervention area, the Revit area off the 00 link.",
                StreetsH8.InWords(StreetsH8Option.InterventionArea));
        }

        [Fact]
        public void TheReportSaysTheAreaCameFromRowTimesLength()
        {
            Assert.Equal(
                "from ROW x Length, 20 x 330.5 = 6610 m2. The H8 formula is =F8*D8 and no filled region was read.",
                StreetsH8.AreaInWords(Street(StreetsH8Option.RowTimesLength)));
        }

        [Fact]
        public void TheReportNamesWhatIsMissingInOneLine()
        {
            string words = StreetsH8.AreaInWords(
                StreetReferenceAnswer.WidthOnly(8.0, "8", "its ES_QUANTITY is no length")
                    .WithH8(StreetsH8Option.RowTimesLength));

            Assert.Equal(
                "ROW x Length could not be worked out, its ES_QUANTITY is no length. The H8 formula is =F8*D8 "
                + "and nothing was taken from Revit.",
                words);
            Assert.DoesNotContain("\n", words);
        }

        private static string ReportOf(StreetReferenceAnswer street)
        {
            KpiCreateRun run = CreateFixture.Run(
                readings: new[] { WithRegion() },
                template: KpiTemplates.Streets,
                street: street);

            return KpiCreateReport.Write(run, new DateTime(2026, 11, 8, 9, 0, 0));
        }

        /// <summary>
        /// The three places a run says where its area came from, none of them calling the area
        /// missing or the template one that takes none.
        /// </summary>
        [Fact]
        public void TheRunsReportSaysRowTimesLengthWhereTheAreaIsPrinted()
        {
            const string Said = "from ROW x Length, 20 x 330.5 = 6610 m2. The H8 formula is =F8*D8 and no filled region was read.";

            string report = ReportOf(Street(StreetsH8Option.RowTimesLength));

            Assert.Contains("    area: " + Said, report);
            Assert.Contains("  plots with an area   " + Said, report);
            Assert.Contains("  AREA, SQUARE METRES, " + Said, report);
            Assert.Contains("  WHICH REGION EACH PLOT'S AREA CAME OFF: " + Said, report);
            Assert.DoesNotContain("the template takes none", report);
            Assert.DoesNotContain(CreateWords.TakesNoArea, report);
        }

        [Fact]
        public void AnOptionOneRunReportsItsRevitAreaAsBefore()
        {
            string report = ReportOf(Street(StreetsH8Option.InterventionArea));

            Assert.Contains("    area: " + RegionChoice.TheNoteNames + " | 5120.75", report);
            Assert.DoesNotContain("ROW x Length", report);
        }

        [Fact]
        public void TheRunsReportNamesWhatIsMissing()
        {
            string report = ReportOf(
                StreetReferenceAnswer.Nothing("ANH-000-ST-100001 is not in the street reference file")
                    .WithH8(StreetsH8Option.RowTimesLength));

            Assert.Contains(
                "    area: ROW x Length could not be worked out, ANH-000-ST-100001 is not in the street reference file. "
                + "The H8 formula is =F8*D8 and nothing was taken from Revit.",
                report);
        }

        [Fact]
        public void ThePressesFirstLinesNameTheOptionItRanUnder()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05", new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(), new List<TemplateOutcome>(),
                streetsH8: StreetsH8Option.RowTimesLength);

            string[] lines = KpiCreateReport.WriteAll(set, new DateTime(2026, 11, 8, 9, 0, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.StartsWith("Folder layout: ", lines[0]);
            Assert.StartsWith("STREETS H8: ROW x Length, ", lines[1]);
        }

        [Fact]
        public void TheSourceLineFollowsTheOption()
        {
            string[] rowTimes = TemplateWords.WouldFill(
                KpiTemplates.Streets, ChosenParameters.NonePicked, StreetsH8Option.RowTimesLength).ToArray();
            string[] revit = TemplateWords.WouldFill(
                KpiTemplates.Streets, ChosenParameters.NonePicked, StreetsH8Option.InterventionArea).ToArray();

            Assert.Contains("  H8  the formula =F8*D8, the road width times the length, picked in Setup", rowTimes);
            Assert.Contains("  H8  PRX_Intervention Area, off the chosen filled region in the 00 link", revit);
        }

        [Fact]
        public void AnotherTemplatesSourceLineIgnoresTheOption()
        {
            string[] lines = TemplateWords.WouldFill(
                KpiTemplates.Mosques, ChosenParameters.NonePicked, StreetsH8Option.RowTimesLength).ToArray();

            Assert.Contains("  H7  PRX_Intervention Area, off the chosen filled region in the 00 link", lines);
        }
    }
}
