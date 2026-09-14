using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **EVERY PLOT GETS ITS OWN BLOCK, AND A SECTION'S COUNT MATCHES WHAT IS UNDER IT.**
    ///
    /// Measured on the 18:15 run over NG05, 156 plots and 150 workbooks. The detail blocks
    /// covered exactly seven plots, EP-01, FP-16, HF-01, DM-11, PL-17, SC-03 and MM-01, which is
    /// the first of each template, so the species list, the schedule print, the cells written
    /// and the reconciliation covered 4 percent of the run. And the file was 10,708 lines of
    /// which WHAT THE WORKBOOK WILL COMPUTE FROM THIS was 5,180, under a heading reading (0).
    /// </summary>
    public class ReportPerPlotTests
    {
        private static readonly DateTime Written = new DateTime(2026, 9, 14, 18, 15, 0);

        private static TemplateSplit Split(params string[] plots)
        {
            return new TemplateSplit(
                new[] { new TemplateShare(KpiTemplates.Mosques, plots, string.Empty) },
                new List<PlotTemplate>(),
                new List<PlotTemplate>());
        }

        private static KpiCreateRunSet Set(params string[] plots)
        {
            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                Split(plots),
                plots.Select(plotId => CreateFixture.Run(
                    readings: new[] { CreateFixture.Plot(plotId) },
                    ticked: new[] { plotId })).ToList(),
                new[] { TemplateOutcome.Wrote(KpiTemplates.Mosques, plots, plots.Length, null) },
                null,
                plots.Select(plotId => PlotOutcome.Wrote(plotId, KpiTemplates.Mosques, null)).ToList());
        }

        /// <summary>
        /// **THE BLOCKS COVER EVERY PLOT, NOT THE FIRST OF EACH TEMPLATE.** It used to take the
        /// first run of each template and print that one, so 149 of 156 plots had no species
        /// list, no schedule print, no cells written and no reconciliation anywhere in the file.
        /// </summary>
        [Fact]
        public void EveryPlotOfEveryTemplateGetsItsOwnBlock()
        {
            string report = KpiCreateReport.WriteAll(Set("DM-11", "DM-12", "FM-05"), Written);

            Assert.Contains("PLOT: DM-11   TEMPLATE: MOSQUES", report);
            Assert.Contains("PLOT: DM-12   TEMPLATE: MOSQUES", report);
            Assert.Contains("PLOT: FM-05   TEMPLATE: MOSQUES", report);

            Assert.Contains("  3 plots of this press belong to it, and each has its own block below.", report);

            // Three blocks, so three reconciliations and three schedule sections rather than one.
            Assert.Equal(3, Times(report, "== " + KpiCreateReport.SchedulesHeading));
        }

        /// <summary>
        /// **THE COUNTS STAY COUNTS OF THE RUN.** A block's own reconciliation reads one plot
        /// because a block IS one plot, and the press's own numbers sit above every block in the
        /// run accounting. That is why a block per plot was chosen over one set of blocks
        /// carrying every plot: nothing has to be re-counted and no number moves.
        /// </summary>
        [Fact]
        public void TheRunsOwnCountsSitAboveTheBlocksAndCountThePress()
        {
            string report = KpiCreateReport.WriteAll(Set("DM-11", "DM-12", "FM-05"), Written);

            Assert.Contains("This press covered 3 plots over 1 template", report);

            // And the accounting is above the first block rather than inside one.
            Assert.True(
                report.IndexOf("This press covered 3 plots", StringComparison.Ordinal)
                    < report.IndexOf("PLOT: DM-11", StringComparison.Ordinal),
                "the press's own counts come before the first plot block");
        }

        /// <summary>
        /// A template no plot of the press belongs to still gets its heading and still says so,
        /// which is unchanged.
        /// </summary>
        [Fact]
        public void ATemplateWithNoPlotStillSaysSo()
        {
            var set = new KpiCreateRunSet(
                "NG05",
                Split(),
                new List<KpiCreateRun>(),
                new[] { TemplateOutcome.NothingToWrite(KpiTemplates.Schools, "no ticked plot belongs to it") },
                null,
                new List<PlotOutcome>());

            string report = KpiCreateReport.WriteAll(set, Written);

            Assert.Contains("TEMPLATE: SCHOOLS", report);
            Assert.Contains("  No plot of this run belongs to it, so nothing was read for it and", report);
        }

        private static int Times(string text, string what)
        {
            int found = 0;
            int at = 0;
            while ((at = text.IndexOf(what, at, StringComparison.Ordinal)) >= 0)
            {
                found = found + 1;
                at = at + what.Length;
            }

            return found;
        }
    }
}
