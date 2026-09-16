using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **ALL 154 ROWS OF THE PLOT LIST READ YES ON THE 16:37 PRESS OF 16 SEPTEMBER**, four
    /// columns of YES with an empty why not, and Bader had the ready list worked out by hand at
    /// 33. Among those 154 rows were the 7 plots whose workbooks had replaced each other, the 41
    /// whose PDF Total areas to be greened came out blank, FP-18 whose PDF and Excel disagree
    /// about the canopy, and the plots whose workbooks recalculate with a #DIV/0!.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class PlotReadyTests
    {
        private static readonly DateTime When = new DateTime(2026, 9, 16, 16, 37, 0);

        private static PdfForm Healthcare
        {
            get { return PdfForms.ForPlot("HF-01"); }
        }

        private static PlotWorkbookPath Where(string uid2)
        {
            return PlotWorkbookPath.For("C:\\out", KpiTemplates.Healthcare, "HEALTH", uid2);
        }

        /// <summary>
        /// One plot that wrote both files, with whatever the PDF left blank handed in.
        /// </summary>
        private static PlotOutcome Wrote(string plotId, params PdfFieldFill[] blank)
        {
            PlotWorkbookPath where = Where("ANH-007-HF-10000" + plotId.Substring(plotId.Length - 1));

            return PlotOutcome.Wrote(plotId, KpiTemplates.Healthcare, where)
                .WithPdf(PdfOutcome.Wrote(
                    plotId, Healthcare, PdfChecklist.Beside(where), null,
                    new List<PdfLandedField>(), blank ?? new PdfFieldFill[0]));
        }

        private static KpiCreateRunSet Press(
            IEnumerable<string> listed,
            IEnumerable<PlotOutcome> outcomes,
            IEnumerable<KpiCreateRun> runs = null,
            IEnumerable<SharedUid2Group> sharing = null)
        {
            List<string> plots = listed.ToList();

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    plots, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                (runs ?? Enumerable.Empty<KpiCreateRun>()).ToList(),
                new List<TemplateOutcome>(),
                null,
                outcomes.ToList(),
                null,
                PlotsInTheModel.Of(plots, plots),
                PlotListRead.Of("C:\\lists\\154.txt", plots),
                (sharing ?? Enumerable.Empty<SharedUid2Group>()).ToList());
        }

        /// <summary>
        /// **A PLOT WITH EVERY SOURCED BOX WRITTEN READS YES.** Both files written, no tree
        /// written nowhere, nothing blank, its own PRX_Plot_UID2 and no #DIV/0!.
        /// </summary>
        [Fact]
        public void APlotWithEverySourcedBoxWrittenReadsYes()
        {
            ReadyAnswer can = PlotReady.For(
                Press(new[] { "HF-01" }, new[] { Wrote("HF-01") }), "HF-01");

            Assert.True(
                can.Ready,
                "HF-01 wrote both files, lost no tree, left no sourced box blank, holds its own "
                + "PRX_Plot_UID2 and its workbook divides by nothing, so nothing is left for a "
                + "person to check. READY said NO because: " + can.WhyInWords);

            Assert.Empty(can.Why);
            Assert.Equal(string.Empty, can.WhyInWords);
        }

        /// <summary>
        /// **41 PLOTS OF THE 16:37 PRESS WENT TO THE CLIENT WITH A BLANK Total areas to be
        /// greened**, EP-01, EP-06, EP-09, EP-14 and 37 street plots, and every one of them read
        /// YES four times over.
        /// </summary>
        [Fact]
        public void APlotWhoseGreenCoverBoxIsBlankReadsNoAndNamesTheBox()
        {
            ReadyAnswer can = PlotReady.For(
                Press(
                    new[] { "HF-01" },
                    new[]
                    {
                        Wrote("HF-01", PdfFieldFill.Blank(
                            PdfValue.TotalAreasToBeGreened,
                            "Total areas to be greened",
                            "GRP_-_KPI_Checklist_-_DD_STREETS.xlsx, Tree List - Existing row 85 "
                            + "carries no canopy formula"))
                    }),
                "HF-01");

            Assert.False(
                can.Ready,
                "HF-01's Total areas to be greened came out blank and READY read YES, which is "
                + "what all 154 rows of the 16:37 press did while 41 of those plots went to the "
                + "client with an empty green cover box.");

            Assert.Equal(
                "PDF Total areas to be greened blank, "
                + "GRP_-_KPI_Checklist_-_DD_STREETS.xlsx, Tree List - Existing row 85 "
                + "carries no canopy formula",
                Assert.Single(can.Why));
        }

        /// <summary>
        /// **A WORKBOOK THAT RECALCULATES WITH A #DIV/0! IS NOT READY**, and the row names the
        /// cell. FP-24 and SC-06 both hold one at Tree List - Existing S70 and T70.
        ///
        /// It stays a report line and never stops a write, which is unchanged. What is new is
        /// that the file does not read as ready to send.
        /// </summary>
        [Fact]
        public void APlotWhoseWorkbookDividesByNoughtReadsNoAndNamesTheCell()
        {
            // **A REAL WORKBOOK RATHER THAN A HAND BUILT FINDING.** The check is what puts the
            // #DIV/0! on the run, so a test handing one in would pass over a check that found
            // nothing. This is the FP-24 shape: 17 trees on row 85 and COUNT over B4:B83.
            string folder = WorkbookFixture.Folder();

            try
            {
                PatchOutcome outcome = WorkbookPatcher.Patch(
                    WorkbookFixture.DividingByACount(folder, "ready.xlsx"),
                    System.IO.Path.Combine(folder, "filled-ready.xlsx"),
                    new[]
                    {
                        CellWrite.Number(KpiTemplates.ExistingTreesSheet, "B85", 17),
                        CellWrite.Number(KpiTemplates.ExistingTreesSheet, "B102", 17)
                    },
                    new WorkbookCell[0]);

                Assert.True(outcome.Written, outcome.Refusal);

                KpiCreateRun run = CreateFixture.Run(
                    readings: new[] { CreateFixture.Plot("HF-01", component: "HEALTH") },
                    template: KpiTemplates.Healthcare,
                    outcome: outcome);

                ReadyAnswer can = PlotReady.For(
                    Press(new[] { "HF-01" }, new[] { Wrote("HF-01") }, new[] { run }), "HF-01");

                Assert.False(
                    can.Ready,
                    "HF-01's workbook recalculates with a #DIV/0! at Tree List - Existing S70 "
                    + "and READY read YES. The 16:37 glance said the formula check found none "
                    + "anywhere in the press while FP-24 and SC-06 both hold one there.");

                Assert.Equal(
                    "#DIV/0!, Tree List - Existing S70, Tree List - Existing T70",
                    Assert.Single(can.Why));
            }
            finally
            {
                try
                {
                    System.IO.Directory.Delete(folder, true);
                }
                catch (System.IO.IOException)
                {
                }
            }
        }

        /// <summary>
        /// **A PLOT WRITTEN NOWHERE BECAUSE ANOTHER TICKED PLOT SHARES ITS FOLDER READS NO**,
        /// and the shared value is the whole reason rather than three consequences of it.
        /// </summary>
        [Fact]
        public void APlotStoppedByASharedUid2ReadsNoAndNamesTheValue()
        {
            IReadOnlyList<SharedUid2Group> sharing = SharedUid2.Of(PlotFilings.Of(
                "C:\\out",
                new[] { "MM-01", "MM-09" },
                plotId => "STREET 36m ROW",
                plotId => "ANH-007-ST-100213"));

            ReadyAnswer can = PlotReady.For(
                Press(new[] { "MM-01" }, new List<PlotOutcome>(), null, sharing), "MM-01");

            Assert.False(can.Ready);

            Assert.Equal(
                "PRX_Plot_UID2 ANH-007-ST-100213 is also MM-09's, so no file is written for any "
                + "of them",
                Assert.Single(can.Why));
        }

        /// <summary>
        /// **THE ROW NAMES EVERY REASON.** A row short of the second reads exactly like a plot
        /// that had one, which is the rule this report already follows everywhere else.
        /// </summary>
        [Fact]
        public void ARowNamesEveryReasonAndNotTheFirstOne()
        {
            ReadyAnswer can = PlotReady.For(
                Press(
                    new[] { "HF-01" },
                    new[]
                    {
                        Wrote(
                            "HF-01",
                            PdfFieldFill.Blank(PdfValue.TotalAreasToBeGreened,
                                "Total areas to be greened", "the canopy check is not usable"),
                            PdfFieldFill.Blank(PdfValue.Lawn, "Lawn", "no such group was printed"))
                    }),
                "HF-01");

            Assert.Equal(2, can.Why.Count);

            Assert.Equal(
                "PDF Total areas to be greened blank, the canopy check is not usable. "
                + "PDF Lawn blank, no such group was printed",
                can.WhyInWords);
        }

        /// <summary>
        /// The one line the glance carries, both ways round, and the press with no list.
        /// </summary>
        [Fact]
        public void TheGlanceLineCountsTheReadyPlotsOfTheList()
        {
            KpiCreateRunSet set = Press(
                new[] { "HF-01", "HF-02", "HF-03" },
                new[]
                {
                    Wrote("HF-01"),
                    Wrote("HF-02", PdfFieldFill.Blank(
                        PdfValue.TotalAreasToBeGreened, "Total areas to be greened", "no canopy"))
                });

            Assert.Equal(
                "THE PLOTS READY TO SEND: ready 1 of 3.",
                PlotReady.InWords(PlotReady.Of(set, new[] { "HF-01", "HF-02", "HF-03" })));

            Assert.Equal(
                "THE PLOTS READY TO SEND: no plot list file was set, so nothing here says how "
                + "many plots came out ready.",
                PlotReady.InWords(null));
        }
    }
}
