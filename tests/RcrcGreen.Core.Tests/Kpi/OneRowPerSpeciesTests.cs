using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The species rows of the 1428 run read FM-05 10, FM-05 10, FM-06 15 for ALBIZIA LEBBECK
    /// proposed, FM-05 19, FM-05 20 for BAUHINIA PURPUREA and FM-05 3, FM-05 4 for CASSIA
    /// GLAUCA, while the accounting read one softscape schedule on every plot. Both are right:
    /// there is one schedule, and it prints those three species on two rows each under
    /// Proposed. 19 and 20 are not one row read twice. Nothing says whether two rows for one
    /// species under one group are two types of it or one counted twice, so the accounting
    /// refuses naming the rows and the counts, and the rows are printed at the end of the
    /// report for a person to look at.
    /// </summary>
    public class OneRowPerSpeciesTests
    {
        private static readonly string[] Headings =
            { "IMAGE", "PLANT CODE", "BOTANICAL NAME", "COUNT (n)", "HEIGHT (m)", "DIAMETER (m)" };

        /// <summary>
        /// FM-05 as the run measured it, in miniature: rows 4 and 5 both ALBIZIA LEBBECK, 6
        /// and 7 both BAUHINIA PURPUREA, 8 and 9 both CASSIA GLAUCA, one subtotal and TOTAL 66.
        /// </summary>
        private static ScannedSchedule Fm05()
        {
            return CreateFixture.Softscape(
                "FM-05",
                Headings,
                new[] { "TREES", "", "", "", "", "" },
                new[] { "Proposed", "", "", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB", "ALBIZIA LEBBECK", "10", "15", "8" },
                new[] { "Albizia lebbeck.jpg", "ALB LEB 2", "ALBIZIA LEBBECK", "10", "12", "6" },
                new[] { "Bauhinia purpurea.jpg", "BAU PUR", "BAUHINIA PURPUREA", "19", "6", "5" },
                new[] { "Bauhinia purpurea.jpg", "BAU PUR 2", "BAUHINIA PURPUREA", "20", "6", "5" },
                new[] { "Cassia glauca.jpg", "CAS GLA", "CASSIA GLAUCA", "3", "6", "5" },
                new[] { "Cassia glauca.jpg", "CAS GLA 2", "CASSIA GLAUCA", "4", "6", "5" },
                new[] { "", "", "", "66", "", "" },
                new[] { "TOTAL", "", "", "66", "", "" });
        }

        private static PlotReading Fm05Reading()
        {
            SoftscapeReading trees = SoftscapeRows.Read(Fm05(), CreateFixture.Counted, "FM-05");
            return CreateFixture.Plot("FM-05", species: trees.Species.ToArray(),
                softscapeTotalRead: trees.TotalRead, softscapeTotal: trees.Total, rowsPassedOver: trees.RowsPassedOver,
                printedSchedules: new[] { Fm05() }, softscapeTotalRow: trees.TotalRow, printedGroups: trees.Groups.ToArray());
        }

        /// <summary>
        /// Six species rows off one schedule, each with its row, adding to the printed TOTAL. So
        /// the reader read what the schedule printed and nothing twice.
        /// </summary>
        [Fact]
        public void OneScheduleReadsSixRowsThatAddToItsTotal()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Fm05(), CreateFixture.Counted, "FM-05");

            Assert.True(reading.WasRead, string.Join(" ", reading.Refusals));
            Assert.Equal(6, reading.Species.Count);
            Assert.Equal(new[] { 4, 5, 6, 7, 8, 9 }, reading.Species.Select(one => one.RowNumber));
            Assert.Equal(new[] { 10, 10, 19, 20, 3, 4 }, reading.Species.Select(one => one.Quantity));
            Assert.Equal(66, reading.SpeciesSum);
            Assert.True(reading.TotalRead);
            Assert.Equal(66, reading.Total);
            Assert.Equal(11, reading.TotalRow);
        }

        [Fact]
        public void EachSpeciesOnTwoRowsIsNamedWithItsRowsAndCounts()
        {
            IReadOnlyList<RepeatedSpecies> repeated = Fm05Reading().SpeciesPrintedOnMoreThanOneRow;

            Assert.Equal(3, repeated.Count);
            Assert.Equal("ALBIZIA LEBBECK", repeated[0].BotanicalName);
            Assert.Equal("Proposed", repeated[0].GroupName);
            Assert.Equal("rows 4 and 5, counting 10 and 10", repeated[0].InWords);
            Assert.Equal("rows 6 and 7, counting 19 and 20", repeated[1].InWords);
            Assert.Equal("rows 8 and 9, counting 3 and 4", repeated[2].InWords);
        }

        /// <summary>
        /// Refused, with the plot, the species, the group, the rows and the counts, and where
        /// to look. Not taken first, not added in silence.
        /// </summary>
        [Fact]
        public void ASpeciesOnTwoRowsUnderOneGroupRefusesTheWriteNamingTheRows()
        {
            Reconciliation held = Reconciliation.Of(
                new[] { "FM-05" }, new[] { Fm05Reading() }, null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.Equal(3, held.Refusals.Count);
            Assert.Equal(
                "FM-05 prints ALBIZIA LEBBECK on 2 rows under Proposed, rows 4 and 5, counting 10 and 10. "
                + "Nothing says whether that is two types of one species or one counted twice, so the write is "
                + "refused until somebody says which. The rows are printed at the end of the report.",
                held.Refusals[0]);
            Assert.Contains("FM-05 prints CASSIA GLAUCA on 2 rows under Proposed, rows 8 and 9, counting 3 and 4.", held.Refusals[2]);

            // Still one schedule on the plot. The count was right and the diagnosis was wrong.
            Assert.Empty(held.WithMoreThanOneSoftscape);
            Assert.Empty(held.WithoutSoftscape);
        }

        /// <summary>
        /// The same species once on each of two plots is the ordinary case and refuses nothing.
        /// The same name under two groups on one plot is two species and refuses nothing.
        /// </summary>
        [Fact]
        public void OneRowPerPlotAndOneRowPerGroupRefuseNothing()
        {
            var readings = new[]
            {
                CreateFixture.Plot("FM-05", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", "Existing", 1, 4, "15", "8"),
                    CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 10, 8, "15", "8")
                }, regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 900.0) }),
                CreateFixture.Plot("FM-06", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 15, 6, "15", "8")
                }, regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 250.0) })
            };

            Reconciliation held = Reconciliation.Of(new[] { "FM-05", "FM-06" }, readings, null, false, KpiTemplates.Mosques);

            Assert.True(held.AddsUp, string.Join(" ", held.Refusals));
            Assert.All(readings, one => Assert.Empty(one.SpeciesPrintedOnMoreThanOneRow));
        }

        /// <summary>
        /// A merged row says FM-05 20 (2 rows, 10 + 10) rather than FM-05 10, FM-05 10, so one
        /// plot with two rows no longer reads as one plot appearing twice.
        /// </summary>
        [Fact]
        public void TheWorkingGroupsTheRowsOfOnePlot()
        {
            MergedSpecies albizia = KpiMerge.Species(new[]
            {
                Fm05Reading(),
                CreateFixture.Plot("FM-06", species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", "Proposed", 15, 6, "15", "8") })
            }).Single(one => one.BotanicalName == "ALBIZIA LEBBECK");

            Assert.Equal(35, albizia.Quantity);
            Assert.Equal("FM-05 20 (2 rows, 10 + 10), FM-06 15", albizia.Working);
            Assert.Equal(3, albizia.Rows.Count);
        }

        /// <summary>
        /// Under the plot in the report, beside the schedule it came off, so nobody has to
        /// reach the merged rows to see it.
        /// </summary>
        [Fact]
        public void TheReportSaysItUnderThePlot()
        {
            string report = KpiCreateReport.Write(
                CreateFixture.Run(readings: new[] { Fm05Reading() }), new System.DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("    softscape schedule: FM-05-(600) SOFTSCAPE SCHEDULE, 6 species rows read\r\n"
                + "      species rows add to 66, the TOTAL row prints 66 at row 11\r\n"
                + "      1 row with a count and no botanical name passed over, the subtotals\r\n"
                + "      group rows: 1\r\n"
                + "        row 3 Proposed: 6 species rows adding to 66, subtotal row 10 prints 66, TAKEN, Tree List - Proposed is named for it\r\n"
                + "      ALBIZIA LEBBECK under Proposed is printed on 2 rows, rows 4 and 5, counting 10 and 10\r\n"
                + "      BAUHINIA PURPUREA under Proposed is printed on 2 rows, rows 6 and 7, counting 19 and 20\r\n"
                + "      CASSIA GLAUCA under Proposed is printed on 2 rows, rows 8 and 9, counting 3 and 4\r\n", report);
            Assert.Contains("  plots with one softscape schedule   1\r\n", report);
        }
    }
}
