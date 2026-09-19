using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **AFTER A PRESS THE PANE SHOWS THE RESULT RATHER THAN SENDING BADER TO THE REPORT FILE.**
    /// His layout of 17 September: two counts side by side, then the plots that are not ready,
    /// each on one line with its plot, its UID2 and its reason.
    ///
    /// **THE ROWS ARE THE REPORT'S OWN.** `PlotListRows` builds them once and both
    /// `KpiCreateReport` and this read the same ones, so the pane's two counts and the report's
    /// `ready:` line cannot disagree. A second walk over the outcomes counting ready its own way
    /// is the shape this repository has paid for eight times.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class KpiResultsTests
    {
        private static PlotWorkbookPath Where(string uid2)
        {
            return PlotWorkbookPath.For("C:\\out", KpiTemplates.Healthcare, "HEALTH", uid2);
        }

        private static string Uid2Of(string plotId)
        {
            return "ANH-007-HF-1000" + plotId.Substring(3);
        }

        /// <summary>
        /// A plot that wrote both files with nothing blank, which is READY YES.
        /// </summary>
        private static PlotOutcome Ready(string plotId)
        {
            PlotWorkbookPath where = Where(Uid2Of(plotId));

            return PlotOutcome.Wrote(plotId, KpiTemplates.Healthcare, where)
                .WithPdf(PdfOutcome.Wrote(
                    plotId, PdfForms.ForPlot(plotId), PdfChecklist.Beside(where), null,
                    new List<PdfLandedField>(), new PdfFieldFill[0]));
        }

        /// <summary>
        /// A plot whose workbook and PDF were written and whose green cover box came out blank.
        /// </summary>
        private static PlotOutcome GreenCoverBlank(string plotId)
        {
            PlotWorkbookPath where = Where(Uid2Of(plotId));

            return PlotOutcome.Wrote(plotId, KpiTemplates.Healthcare, where)
                .WithPdf(PdfOutcome.Wrote(
                    plotId, PdfForms.ForPlot(plotId), PdfChecklist.Beside(where), null,
                    new List<PdfLandedField>(),
                    new[]
                    {
                        PdfFieldFill.Blank(
                            PdfValue.TotalAreasToBeGreened,
                            "Total areas to be greened",
                            "Tree List - Existing row 85 carries no canopy formula")
                    }));
        }

        /// <summary>
        /// A plot whose workbook was refused by its own path, which is the EP-05 shape.
        /// </summary>
        private static PlotOutcome NoWorkbook(string plotId)
        {
            return PlotOutcome.WroteNothing(
                plotId,
                KpiTemplates.Healthcare,
                PlotWorkbookPath.Refused(PlotWorkbookPath.NoUid2),
                false,
                PlotWorkbookPath.NoUid2);
        }

        private static KpiCreateRunSet Press(
            IEnumerable<string> listed, IEnumerable<PlotOutcome> outcomes)
        {
            List<string> plots = listed.ToList();

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    plots, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                outcomes.ToList(),
                null,
                PlotsInTheModel.Of(plots, plots),
                PlotListRead.Of("C:\\lists\\154.txt", plots));
        }

        /// <summary>
        /// The 154 plot press, built to Bader's numbers: 122 read READY YES and 32 read NO.
        ///
        /// The first three of the 32 carry three different reasons, so the panel is checked on a
        /// press holding more than one kind of fault rather than 32 copies of one.
        /// </summary>
        private static KpiCreateRunSet TheFullPress()
        {
            var listed = new List<string>();
            for (int at = 1; at <= 154; at++)
            {
                listed.Add("HF-" + at.ToString("000", CultureInfo.InvariantCulture));
            }

            var outcomes = new List<PlotOutcome>();

            // HF-001 wrote both files with its green cover box blank.
            outcomes.Add(GreenCoverBlank("HF-001"));

            // HF-002 was never ticked, so it has no outcome at all and is left out here.

            // HF-003's own path refused it, so no workbook and no PDF.
            outcomes.Add(NoWorkbook("HF-003"));

            // HF-004 to HF-032, 29 more plots nobody ticked, each with no outcome.

            // HF-033 to HF-154 all wrote both files cleanly. 154 less 32 is 122.
            for (int at = 33; at <= 154; at++)
            {
                outcomes.Add(Ready("HF-" + at.ToString("000", CultureInfo.InvariantCulture)));
            }

            return Press(listed, outcomes);
        }

        /// <summary>
        /// **THE TWO COUNTS, OFF A PRESS OF 154 WITH 122 READY.** 154 less 122 is 32, and every
        /// one of the 32 is a row the panel shows.
        /// </summary>
        [Fact]
        public void ThePanelCountsOneHundredAndTwentyTwoReadyAndThirtyTwoNot()
        {
            KpiResults results = KpiResults.Of(TheFullPress());

            Assert.True(results.ListWasSet);
            Assert.Equal(154, results.Rows.Count);
            Assert.Equal(122, results.Ready);
            Assert.Equal(32, results.NotReady);
            Assert.Equal(32, results.NotReadyRows.Count);

            Assert.Equal(
                "ready 122, not ready 32, of 154 plots on the list",
                results.CountsInWords);

            Assert.Equal("32 plots are not ready to send:", results.NotReadyInWords);
        }

        /// <summary>
        /// **THE COUNTS ARE THE REPORT'S OWN.** The report prints `ready: 122` off the same rows,
        /// so the pane and the file the team opens beside it cannot say two different numbers.
        /// </summary>
        [Fact]
        public void ThePanelsCountAndTheReportsReadyLineAreTheSameNumber()
        {
            KpiCreateRunSet set = TheFullPress();

            string report = KpiCreateReport.WriteAll(
                set, new System.DateTime(2026, 9, 17, 15, 55, 0));

            Assert.Equal(122, KpiResults.Of(set).Ready);
            Assert.Contains("ready: 122", report);
            Assert.Contains("listed: 154", report);
        }

        /// <summary>
        /// **THE FIRST THREE REASONS, WRITTEN OUT BY HAND.** Each names its own fault in the
        /// words the plot list already uses, and each carries the plot's UID2 beside it.
        /// </summary>
        [Fact]
        public void TheFirstThreeNotReadyLinesNameThePlotTheUidAndTheReason()
        {
            IReadOnlyList<PlotListRow> notReady = KpiResults.Of(TheFullPress()).NotReadyRows;

            Assert.Equal(
                "HF-001 | ANH-007-HF-1000001 | PDF Total areas to be greened blank, "
                + "Tree List - Existing row 85 carries no canopy formula",
                notReady[0].InWords);

            // **A PLOT NOBODY TICKED IS NOT A PLOT WITH NO PRX_Plot_UID2.** Nothing looked
            // for one, so saying it holds none is a sentence about the model that nothing
            // measured. The row names the plot and the reason and claims nothing else.
            Assert.Equal(
                "HF-002 | it was not ticked for this press",
                notReady[1].InWords);

            Assert.Equal(
                "HF-003 | no PRX_Plot_UID2 | no PRX_Plot_UID2 was read off this plot's first "
                + "sheet, and the folder and the file are both named after it. no PDF was "
                + "planned beside the workbook",
                notReady[2].InWords);
        }

        /// <summary>
        /// **THE ROWS COME BACK IN THE TEAM'S OWN ORDER**, because the report reads down that
        /// file beside their copy and the pane has to read the same way.
        /// </summary>
        [Fact]
        public void TheRowsAreInTheListsOwnOrder()
        {
            IReadOnlyList<PlotListRow> notReady = KpiResults.Of(TheFullPress()).NotReadyRows;

            Assert.Equal("HF-001", notReady[0].PlotId);
            Assert.Equal("HF-002", notReady[1].PlotId);
            Assert.Equal("HF-003", notReady[2].PlotId);
            Assert.Equal("HF-004", notReady[3].PlotId);
            Assert.Equal("HF-032", notReady[31].PlotId);
        }

        /// <summary>
        /// **A PLOT THE PRESS TRIED TO FILE AND COULD NOT STILL SAYS IT HOLDS NO UID2**, which
        /// is the case the row really measured: HF-003's own path was refused for exactly that.
        /// </summary>
        [Fact]
        public void APlotWhosePathWasRefusedStillSaysItHoldsNoUid()
        {
            PlotListRow row = KpiResults.Of(Press(
                new[] { "HF-003" }, new[] { NoWorkbook("HF-003") })).Rows[0];

            Assert.StartsWith("HF-003 | no PRX_Plot_UID2 | ", row.InWords);
        }

        /// <summary>
        /// **A FILE THAT WAS SET AND COULD NOT BE READ IS NOT A FILE NOBODY CHOSE.** They tick
        /// exactly the same nothing, which is the rule `PlotListFile` already carries, and the
        /// panel said the same sentence for both while the report printed the file's own
        /// reason beside the file's own name.
        /// </summary>
        [Fact]
        public void APlotListThatCouldNotBeReadIsNotAPlotListNobodySet()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new[] { "HF-001" }, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new List<PlotOutcome> { Ready("HF-001") },
                null,
                PlotsInTheModel.Of(new[] { "HF-001" }, new[] { "HF-001" }),
                PlotListFile.In("C:\\lists\\gone.txt"));

            KpiResults results = KpiResults.Of(set);

            Assert.True(results.ListWasSet);
            Assert.False(results.ListWasRead);

            Assert.Equal(
                "The plot list file was set and could not be read, so there is no list to "
                + "count ready against. The report names the file and says why.",
                results.CountsInWords);

            Assert.Equal(string.Empty, results.NotReadyInWords);
        }

        /// <summary>
        /// **A LIST THAT NAMES NO PLOT IS NOT A LIST WHERE EVERY PLOT IS READY.** An empty list
        /// counted nought not ready and said every plot on it is ready, which is a sentence
        /// about nothing that reads as a clean press.
        /// </summary>
        [Fact]
        public void APlotListThatNamesNoPlotSaysSoRatherThanSayingEveryPlotIsReady()
        {
            KpiResults results = KpiResults.Of(Press(new string[0], new PlotOutcome[0]));

            Assert.True(results.ListWasSet);
            Assert.True(results.ListWasRead);
            Assert.Empty(results.Rows);

            Assert.Equal(
                "The plot list file was read and names no plot, so there is nothing to count "
                + "ready against.",
                results.CountsInWords);

            Assert.Equal(string.Empty, results.NotReadyInWords);
        }

        /// <summary>
        /// **A PRESS WITH NO PLOT LIST FILE COUNTS NOTHING AND SAYS SO**, because ready is a
        /// question about the team's list rather than about the model. Counting nought of nought
        /// would read as a press where nothing came out ready.
        /// </summary>
        [Fact]
        public void APressWithNoPlotListFileCountsNothingAndSaysSo()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new[] { "HF-001" }, plotId => "HEALTH", new[] { KpiTemplates.Healthcare }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new List<PlotOutcome> { Ready("HF-001") },
                null,
                PlotsInTheModel.Of(new[] { "HF-001" }, new[] { "HF-001" }));

            KpiResults results = KpiResults.Of(set);

            Assert.False(results.ListWasSet);
            Assert.Empty(results.Rows);
            Assert.Equal(0, results.Ready);
            Assert.Equal(0, results.NotReady);

            Assert.Equal(
                "No plot list file was set for this press, so there is no list to count ready "
                + "against. The report says what each ticked plot did.",
                results.CountsInWords);

            Assert.Equal(string.Empty, results.NotReadyInWords);
        }

        /// <summary>
        /// **A PRESS WHERE EVERY PLOT IS READY SAYS SO.** A block that disappears when there is
        /// nothing to report reads the same as one nobody wrote.
        /// </summary>
        [Fact]
        public void APressWhereEveryPlotIsReadySaysSo()
        {
            KpiResults results = KpiResults.Of(Press(
                new[] { "HF-001", "HF-002" },
                new[] { Ready("HF-001"), Ready("HF-002") }));

            Assert.Equal(2, results.Ready);
            Assert.Equal(0, results.NotReady);
            Assert.Empty(results.NotReadyRows);

            Assert.Equal("ready 2, not ready 0, of 2 plots on the list", results.CountsInWords);
            Assert.Equal("Every plot on the list is ready.", results.NotReadyInWords);
        }

        /// <summary>
        /// **ONE PLOT NOT READY READS IN THE SINGULAR**, and one plot on the list does too.
        /// </summary>
        [Fact]
        public void OnePlotReadsInTheSingular()
        {
            KpiResults results = KpiResults.Of(Press(
                new[] { "HF-001" }, new[] { GreenCoverBlank("HF-001") }));

            Assert.Equal("ready 0, not ready 1, of 1 plot on the list", results.CountsInWords);
            Assert.Equal("1 plot is not ready to send:", results.NotReadyInWords);
        }

        /// <summary>
        /// **A READY PLOT'S ROW STILL CARRIES ITS UID2**, so the panel can name the folder a
        /// person goes and opens without asking anything a second time.
        /// </summary>
        [Fact]
        public void AReadyRowCarriesItsUidAndNoReason()
        {
            PlotListRow row = KpiResults.Of(Press(
                new[] { "HF-001" }, new[] { Ready("HF-001") })).Rows[0];

            Assert.True(row.Ready);
            Assert.Equal("ANH-007-HF-1000001", row.Uid2);
            Assert.Equal(string.Empty, row.WhyNot);
            Assert.Equal("HF-001 | ANH-007-HF-1000001", row.InWords);
        }
    }
}
