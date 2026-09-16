using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE 16:06 PRESS TICKED 166 PLOTS AGAINST A LIST OF 154.** The twelve extra plots were
    /// read, written and filed, three of that press's five shared value collisions came from
    /// them, and neither the pane nor THE PLOT LIST said they were ticked.
    ///
    /// It is the other direction from IN THE MODEL AND NOT ON THE LIST, which is about the
    /// MODEL: a plot the model holds and the list does not is ordinary, and a plot somebody
    /// ticked that the list does not hold is work nobody asked for.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class TickedNotOnTheListTests : IDisposable
    {
        private readonly string _path = Path.Combine(
            Path.GetTempPath(), "rcrc-ticked-" + Guid.NewGuid().ToString("N") + ".txt");

        public void Dispose()
        {
            try
            {
                File.Delete(_path);
            }
            catch (IOException)
            {
            }
        }

        private PlotListRead ListOf(params string[] plots)
        {
            File.WriteAllLines(_path, plots);
            return PlotListFile.In(_path);
        }

        /// <summary>
        /// **A LIST HOLDING HF-01, WITH HF-01 AND NS-41 TICKED.** NS-41 is named above Create.
        /// </summary>
        [Fact]
        public void ATickedPlotTheListDoesNotHoldIsNamedAboveCreate()
        {
            PlotListRead list = ListOf("HF-01");

            IReadOnlyList<string> lines = TickingTheList.Lines(
                list,
                PlotsInTheModel.Of(new[] { "HF-01", "NS-41" }, new[] { "HF-01", "NS-41" }),
                plotId => plotId.StartsWith("HF", StringComparison.Ordinal) ? "HEALTH" : "STREET 36m ROW",
                new[] { "HF-01", "NS-41" });

            // **NS-41 IS IN THE MESSAGE.** A failure reading `expected 2, got 1` says nothing
            // about which ticked plot the pane went on saying nothing about.
            Assert.True(
                lines.Any(one => one.Contains("NS-41")),
                "NS-41 is ticked and the list holds only HF-01, so the press will write a "
                + "workbook and a PDF for a plot the team did not ask for. The 16:06 press did "
                + "that twelve times over with nothing said. The lines the pane would show: "
                + (lines.Count == 0 ? "none" : string.Join(" / ", lines.ToArray())));

            Assert.Equal(
                "  NS-41 is ticked and is not on the list, so this press will write a workbook "
                + "and a PDF for a plot the team did not ask for.",
                lines.Single(one => one.Contains("NS-41")));
        }

        /// <summary>
        /// **WITH ONLY THE LIST TICKED, NO PLOT IS NAMED.** A line about nothing is one the team
        /// reads past on every other press, so the count line is all that is left.
        /// </summary>
        [Fact]
        public void WithOnlyTheListTickedNothingIsNamed()
        {
            PlotListRead list = ListOf("HF-01", "HF-02");

            IReadOnlyList<string> lines = TickingTheList.Lines(
                list,
                PlotsInTheModel.Of(new[] { "HF-01", "HF-02" }, new[] { "HF-01", "HF-02" }),
                plotId => "HEALTH",
                new[] { "HF-01", "HF-02" });

            Assert.Equal(TickingTheList.Heading + " " + list.InWords, Assert.Single(lines));
        }

        /// <summary>
        /// **THE TICKS ARE COMPARED THE WAY THE PANE COMPARES THEM**, which is the Ordinal
        /// comparison PlotTicks and PlotsInTheModel already use. A second comparison here would
        /// be two rules for one question.
        /// </summary>
        [Fact]
        public void TheExtraPlotsAreTheTickedOnesTheListDoesNotHold()
        {
            PlotListRead list = ListOf("HF-01", "HF-02");

            Assert.Equal(
                new[] { "NS-41", "MM-08" },
                TickingTheList.TickedAndNotOnTheList(
                    new[] { "HF-01", "NS-41", "HF-02", "MM-08" }, list).ToArray());
        }

        /// <summary>
        /// **NO LIST MEANS NOTHING IS OFF IT.** A press with no plot list file set says nothing
        /// about a list, which is the rule this section already follows.
        /// </summary>
        [Fact]
        public void WithNoListNothingIsCountedAsOffIt()
        {
            Assert.Empty(TickingTheList.TickedAndNotOnTheList(
                new[] { "HF-01", "NS-41" }, PlotListRead.NotSet));
        }

        /// <summary>
        /// **THE REPORT CARRIES A BLOCK OF ITS OWN, naming each plot with what was written for
        /// it.** The 16:06 report named none of its twelve anywhere in the file.
        /// </summary>
        [Fact]
        public void TheReportBlockNamesTheTickedPlotAndWhatWasWrittenForIt()
        {
            PlotListRead list = ListOf("HF-01");

            PlotWorkbookPath where = PlotWorkbookPath.For(
                "C:\\out", KpiTemplates.Streets, "STREET 36m ROW", "ANH-007-ST-100210");

            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new[] { "HF-01", "NS-41" },
                    plotId => plotId.StartsWith("HF", StringComparison.Ordinal)
                        ? "HEALTH"
                        : "STREET 36m ROW",
                    new[] { KpiTemplates.Healthcare, KpiTemplates.Streets }),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new List<PlotOutcome>
                {
                    PlotOutcome.Wrote("NS-41", KpiTemplates.Streets, where)
                },
                null,
                PlotsInTheModel.Of(new[] { "HF-01", "NS-41" }, new[] { "HF-01", "NS-41" }),
                list);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 16, 16, 6, 0));

            Assert.Contains(
                KpiCreateReport.TickedNotListedHeading + ", 1. A workbook and a PDF were "
                + "written for each of these and the team did not ask for them.",
                report);

            Assert.Contains("NS-41 | workbook " + where.FilePath + " | no PDF", report);
        }
    }
}
