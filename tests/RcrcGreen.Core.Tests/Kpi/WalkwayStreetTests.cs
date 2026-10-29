using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE WALKWAY'S ROAD WIDTH**, K14. Measured 29 October on Scope_Validation_21072026: of
    /// 6,244 STREET rows two are in sqm and both are CL_SUBCATEGORY Pedestrian Walk. The rows
    /// below are shaped like those two and are not them: made up UIDs and numbers, written the
    /// way the file writes them. The unit used to refuse the whole row, so the walkway lost a
    /// width the file holds. The unit is the quantity's, so it now decides the length alone.
    /// </summary>
    public class WalkwayStreetTests : IDisposable
    {
        private const string WalkwayUid = "ANH-000-ST-100002";

        private const string NoWidthUid = "ANH-000-ST-100003";

        private const string LengthWhy =
            "ANH-000-ST-100002 is on row 3 with QUANTITY UNIT reading sqm rather than m, so its "
            + "ES_QUANTITY is no length and the total length is left empty";

        private static readonly DateTime Today = new DateTime(2026, 10, 29);

        private readonly string _folder = StreetReferenceFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private StreetReferenceAnswer Answer(string uid)
        {
            string path = StreetReferenceFixture.Create(
                _folder,
                new[]
                {
                    new StreetReferenceFixture.Row("ANH-000-ST-100001", "248.316027", "m", "15"),
                    new StreetReferenceFixture.Row(WalkwayUid, "250", "sqm", "8"),
                    new StreetReferenceFixture.Row(NoWidthUid, "402.13958700000001", "sqm", "&lt;Null&gt;")
                },
                "walkway.xlsx");

            return StreetReferenceFile.In(path).For(uid);
        }

        [Fact]
        public void AWalkwayRowInSqmGivesItsWidthAndNoLength()
        {
            StreetReferenceAnswer answer = Answer(WalkwayUid);

            Assert.True(answer.WidthFound);
            Assert.False(answer.LengthFound);
            Assert.False(answer.Found);
            Assert.Equal(8.0, answer.Width);
            Assert.Equal("8", answer.PrintedWidth);
            Assert.Equal(string.Empty, answer.PrintedLength);
            Assert.Equal(LengthWhy, answer.Why);
        }

        /// <summary>
        /// The other walkway row holds no width either, so it is refused in the words it always
        /// was. A street row in m is untouched: both cells.
        /// </summary>
        [Fact]
        public void ARowInSqmWithNoWidthAndARowInMetresAreAsTheyWere()
        {
            StreetReferenceAnswer none = Answer(NoWidthUid);
            Assert.False(none.WidthFound);
            Assert.False(none.LengthFound);
            Assert.Equal(
                "ANH-000-ST-100003 is on row 4 with QUANTITY UNIT reading sqm rather than m, so the row is not used",
                none.Why);

            StreetReferenceAnswer street = Answer("ANH-000-ST-100001");
            Assert.True(street.Found);
            Assert.Equal(15.0, street.Width);
            Assert.Equal(248.316027, street.Length, 6);
        }

        /// <summary>
        /// The workbook: D8, Streets ROW (m), takes 8. F8, Streets Total Length (m), stays empty
        /// and carries the length's own reason.
        /// </summary>
        [Fact]
        public void TheWalkwaysWorkbookTakesTheWidthAndNamesTheLength()
        {
            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Streets,
                new AgreedValue(new[] { new PlotText("WW-01", "WALKWAY") }),
                new AgreedValue(new[] { new PlotText("WW-01", WalkwayUid) }),
                "KING FAHD",
                null,
                Totalled.Adding(new[] { new PlotNumber("WW-01", 40.0) }),
                Totalled.Adding(new[] { new PlotNumber("WW-01", 12.0) }),
                null,
                "2026-10-29", "B RAHAL", "LANDSCAPE ARCHITECT",
                Answer(WalkwayUid), CreateFixture.NoLabels);

            Assert.Equal("8", plan.Writes.Single(one => one.Cell.ToString() == "D8").Stored);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "F8");

            NotWritten length = Assert.Single(plan.Skipped, one => one.Cell == "F8");
            Assert.Equal(LengthWhy, length.Why);
            Assert.DoesNotContain(plan.Skipped, one => one.Cell == "D8");
        }

        /// <summary>
        /// The Roads PDF: Row takes 8, Length is blank with the same reason.
        /// </summary>
        [Fact]
        public void TheWalkwaysPdfTakesTheWidthAndNamesTheLength()
        {
            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot("WW-01", component: "WALKWAY", uid2: WalkwayUid),
                KpiTemplates.Streets,
                CountedGroups.Of(KpiTemplates.Streets),
                Answer(WalkwayUid),
                Today,
                true,
                PdfWorkbookNumbers.None);

            Assert.Equal("8", plan.Fields.Single(one => one.Value == PdfValue.Row).Text);

            PdfFieldFill length = plan.Fields.Single(one => one.Value == PdfValue.Length);
            Assert.Equal(string.Empty, length.Text);
            Assert.Equal(LengthWhy, length.Why);
        }

        /// <summary>
        /// A STREETS plot's outcome carrying the answer the press got for its UID2, the way the
        /// handler builds it. A refused path carries no UID2 of its own.
        /// </summary>
        private PlotOutcome Street(string plotId, string uid2, bool written, bool pathRefused = false)
        {
            PlotWorkbookPath where = pathRefused
                ? PlotWorkbookPath.Refused("its PRX_Plot_UID2 holds a character Windows refuses")
                : PlotWorkbookPath.For(FolderLayout.Flat, "C:\\out", KpiTemplates.Streets, "WALKWAY", uid2);

            PlotOutcome outcome = written
                ? PlotOutcome.Wrote(plotId, KpiTemplates.Streets, where)
                : PlotOutcome.WroteNothing(plotId, KpiTemplates.Streets, where, true, "the folder already holds a workbook");

            return outcome.WithStreet(Answer(uid2));
        }

        private static string[] StreetSection(StreetReferenceFile file, params PlotOutcome[] plots)
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new System.Collections.Generic.List<TemplateShare>(),
                    new System.Collections.Generic.List<PlotTemplate>(), new System.Collections.Generic.List<PlotTemplate>()),
                null, null, plotOutcomes: plots, streets: file, layout: FolderLayout.Flat);

            string[] lines = KpiCreateReport.WriteAll(set, new DateTime(2026, 10, 29, 9, 0, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            int at = Array.FindIndex(lines, one => one.StartsWith("== " + KpiCreateReport.StreetsNotAnsweredHeading, StringComparison.Ordinal));
            Assert.True(at >= 0, "no street section");

            return lines.Skip(at).TakeWhile(one => one.Length > 0).ToArray();
        }

        /// <summary>
        /// **The report's street section prints the press's own answer**, breaker pass 1 and
        /// reviewer pass 2 on this round. It used to list street plots whose workbook was not
        /// written, so the walkway, written with no length, was missing under a heading that read
        /// 0, and a plot refused for its folder was listed although the file answered it. Asking
        /// the file again off the path then blamed a missing UID2 on a refused path, ST-03 here,
        /// which the file answered in full.
        /// </summary>
        [Fact]
        public void TheReportListsTheStreetPlotsTheFileLeftACellEmptyFor()
        {
            PlotOutcome walkway = Street("WW-01", WalkwayUid, true);
            StreetReferenceFile file = StreetReferenceFile.In(Path.Combine(_folder, "walkway.xlsx"));

            string[] section = StreetSection(
                file,
                walkway,
                Street("ST-01", "ANH-000-ST-100001", false),
                Street("ST-02", "ANH-000-ST-999999", true),
                Street("ST-03", "ANH-000-ST-100001", false, pathRefused: true),
                Street("ST-04", NoWidthUid, false, pathRefused: true));

            Assert.Equal(
                new[]
                {
                    "== STREET PLOTS THE REFERENCE FILE COULD NOT ANSWER FOR (3) ==",
                    "each cell named is left empty and nothing is estimated from the component value",
                    "  WW-01 | road width answered, total length not. " + LengthWhy,
                    "  ST-02 | neither answered. ANH-000-ST-999999 is not in the street "
                        + "reference file, so the road width and the total length are left empty",
                    "  ST-04 | neither answered. ANH-000-ST-100003 is on row 4 with QUANTITY UNIT reading "
                        + "sqm rather than m, so the row is not used"
                },
                section);
        }

        /// <summary>
        /// A plot of another template is never in the section, whatever the press asked, and a
        /// STREETS plot the press never asked, one refused before it was read, is in its own row
        /// in ONE WORKBOOK PER PLOT and not here.
        /// </summary>
        [Fact]
        public void APlotThePressNeverAskedIsNotListed()
        {
            Answer(WalkwayUid);
            PlotWorkbookPath where = PlotWorkbookPath.Refused("the plot could not be read");

            string[] section = StreetSection(
                StreetReferenceFile.In(Path.Combine(_folder, "walkway.xlsx")),
                PlotOutcome.WroteNothing("ST-05", KpiTemplates.Streets, where, false, "the plot could not be read"),
                PlotOutcome.Wrote("DM-11", KpiTemplates.Mosques, where)
                    .WithStreet(StreetReferenceAnswer.Nothing(KpiCreatePlan.NotAStreetTemplate)));

            Assert.Equal("== STREET PLOTS THE REFERENCE FILE COULD NOT ANSWER FOR (0) ==", section[0]);
            Assert.Equal(2, section.Length);
        }

        /// <summary>A file that was not read is said once and every street plot is counted.</summary>
        [Fact]
        public void AFileNotReadIsSaidOnceForEveryStreetPlot()
        {
            string[] section = StreetSection(
                StreetReferenceFile.NotSet,
                Street("WW-01", WalkwayUid, true),
                Street("ST-01", "ANH-000-ST-100001", true));

            Assert.Equal(3, section.Length);
            Assert.Equal("== STREET PLOTS THE REFERENCE FILE COULD NOT ANSWER FOR (2) ==", section[0]);
            Assert.StartsWith("  every one, because the file was not read. ", section[2]);
        }
    }
}
