using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **TREES WRITTEN NOWHERE MUST SHOW AT THE TOP.** Bader's decision of 15 September, off the
    /// 13:32 run: FP-17 36, FP-21 25 and FP-20 5 AZADIRACHTA INDICA went nowhere because the
    /// FUTURE PARKS workbook holds that name on more than one row, and FP-23 lost 1 tree named
    /// UNKNOWN. THE PLOT LIST read YES and YES for all four and the glance said nothing, so 67
    /// trees left the building in rows that looked exactly like the rows of a plot with nothing
    /// wrong with it.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class TreesNotWrittenTests
    {
        private static readonly DateTime When = new DateTime(2026, 9, 15, 13, 32, 0);

        /// <summary>
        /// The FUTURE PARKS proposed list as the run met it: AZADIRACHTA INDICA on two rows, 22
        /// and 84, which is what nothing can choose between.
        /// </summary>
        private static SpeciesList TwoRows()
        {
            var named = new List<SpeciesListRow>();
            for (int row = 4; row <= 84; row++)
            {
                named.Add(new SpeciesListRow(
                    row, row == 22 || row == 84 ? "Azadirachta indica" : "Species " + row));
            }

            return SpeciesList.Holding(
                named, 4, 86, new[] { 85, 86 }, "I", "J", "B87",
                "GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx");
        }

        private static KpiCreateRunSet Press(params PlotReading[] readings)
        {
            SpeciesList list = TwoRows();

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                KpiMerge.Species(readings, CountedGroups.Of(KpiTemplates.FutureParks)),
                KpiTemplates.FutureParks,
                list,
                list);

            KpiCreateRun run = CreateFixture.Run(
                readings: readings,
                template: KpiTemplates.FutureParks,
                matches: matches.ToArray(),
                existingList: list,
                proposedList: list);

            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    readings.Select(one => one.PlotId).ToList(),
                    plot => "FUTURE PARK",
                    new[] { KpiTemplates.FutureParks }),
                new List<KpiCreateRun> { run },
                new List<TemplateOutcome>());
        }

        /// <summary>
        /// **36 TREES ON ONE PLOT, NAMED WITH BOTH CELLS.** The count and the species go in the
        /// plot list's own column and the cells go in the refusal the report prints under the
        /// plot, so a person can open the template at D22 and D84.
        /// </summary>
        [Fact]
        public void APlotWhoseSpeciesIsOnTwoRowsCarriesItsCountAndBothCells()
        {
            KpiCreateRunSet set = Press(CreateFixture.Plot(
                "FP-17",
                component: "FUTURE PARK",
                species: new[]
                {
                    CreateFixture.Species("AZADIRACHTA INDICA", CreateFixture.Proposed, 36)
                }));

            PlotTreesNotWritten lost = Assert.Single(TreesNotWritten.Of(set));

            Assert.Equal("FP-17", lost.PlotId);
            Assert.Equal(36, lost.Trees);
            Assert.Equal("36 AZADIRACHTA INDICA", lost.InWords);

            TreeNotWritten one = Assert.Single(lost.Species);

            Assert.Equal(
                "the workbook holds this name on more than one row, so nothing can say which and "
                + "the count was not written: GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx, "
                + "Tree List - Proposed holds it in D22 and D84",
                one.Why);
        }

        /// <summary>
        /// **THE PLOT LIST ROW CARRIES IT AND THE GLANCE COUNTS IT.** 36 plus 25 plus 5 is 66
        /// trees on three plots, written out by hand.
        /// </summary>
        [Fact]
        public void ThePlotListRowAndTheGlanceBothNameWhatWasLost()
        {
            var lines = new[] { "FP-17", "FP-20", "FP-21" };
            string path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(), "rcrc-trees-" + Guid.NewGuid().ToString("N") + ".txt");
            System.IO.File.WriteAllLines(path, lines);

            try
            {
                KpiCreateRunSet press = Press(
                    CreateFixture.Plot("FP-17", component: "FUTURE PARK", species: new[]
                    {
                        CreateFixture.Species("AZADIRACHTA INDICA", CreateFixture.Proposed, 36)
                    }),
                    CreateFixture.Plot("FP-20", component: "FUTURE PARK", species: new[]
                    {
                        CreateFixture.Species("AZADIRACHTA INDICA", CreateFixture.Proposed, 5)
                    }),
                    CreateFixture.Plot("FP-21", component: "FUTURE PARK", species: new[]
                    {
                        CreateFixture.Species("AZADIRACHTA INDICA", CreateFixture.Proposed, 25)
                    }));

                var set = new KpiCreateRunSet(
                    press.DocumentTitle,
                    press.Split,
                    press.Runs,
                    press.Outcomes,
                    null,
                    new List<PlotOutcome>
                    {
                        PlotOutcome.Wrote("FP-17", KpiTemplates.FutureParks, Where("ANH-007-PA-100017")),
                        PlotOutcome.Wrote("FP-20", KpiTemplates.FutureParks, Where("ANH-007-PA-100020")),
                        PlotOutcome.Wrote("FP-21", KpiTemplates.FutureParks, Where("ANH-007-PA-100021"))
                    },
                    null,
                    PlotsInTheModel.Of(lines, lines),
                    PlotListFile.In(path));

                string report = KpiCreateReport.WriteAll(set, When);

                // **READY READS NO ON ALL THREE AND THE TREES ARE WHY.** Their workbooks were
                // written, and a workbook 36 trees short is not a file anybody should send.
                Assert.Contains("FP-17 | YES | YES | YES | NO | NO | 36 AZADIRACHTA INDICA |", report);
                Assert.Contains("FP-20 | YES | YES | YES | NO | NO | 5 AZADIRACHTA INDICA |", report);
                Assert.Contains("FP-21 | YES | YES | YES | NO | NO | 25 AZADIRACHTA INDICA |", report);

                Assert.Contains(
                    "THE TREES WRITTEN NOWHERE: 66 trees on 3 plots were written nowhere, so "
                    + "those plots' workbooks are short by that many.",
                    report);
            }
            finally
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// **A PRESS THAT LOST NOTHING SAYS SO.** A line that disappears when there is nothing
        /// to report reads the same as one nobody wrote.
        /// </summary>
        [Fact]
        public void APressThatLostNoTreeSaysEveryOneReachedARow()
        {
            Assert.Equal(
                "THE TREES WRITTEN NOWHERE: every tree this press counted reached a row.",
                TreesNotWritten.InWords(new List<PlotTreesNotWritten>()));
        }

        /// <summary>
        /// **A PLOT ON NO SOFTSCAPE SCHEDULE SAYS SO EVEN THOUGH BOTH FILES WERE WRITTEN.**
        /// FM-07 is on a sheet and on no schedule, and on the 13:32 run its row read YES and
        /// YES with an empty last column while its PDF carried three tree counts of 0 and a
        /// Total areas to be greened of 0 against 57 trees in the model.
        /// </summary>
        [Fact]
        public void APlotWithNoSoftscapeScheduleSaysSoInItsWhyNotColumn()
        {
            PlotReading reading = CreateFixture.Plot("FM-07", softscapeRead: false);

            Assert.Equal(
                "both files were written and this plot holds no softscape schedule, so no tree "
                + "was counted and this box is left empty rather than written 0",
                KpiCreateReport.NoSoftscapeOnTheList(reading));

            // A plot whose schedule WAS read carries nothing, so an ordinary row does not gain
            // a sentence about a schedule that is there.
            Assert.Equal(
                string.Empty,
                KpiCreateReport.NoSoftscapeOnTheList(CreateFixture.Plot("FM-05")));
        }

        private static PlotWorkbookPath Where(string uid2)
        {
            return PlotWorkbookPath.For(
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "rcrc-out"),
                KpiTemplates.FutureParks, "FUTURE PARK", uid2);
        }
    }
}
