using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Everything in the report was what the tool concluded and nothing was what it read, so
    /// the only way to check it was to open Revit, and a plot printed on two rows survived two
    /// rounds because nothing showed the rows. The last section prints every schedule this run
    /// read as the schedule prints it, every column aligned, with what was read off it.
    /// </summary>
    public class SchedulesAsPrintedTests
    {
        private static readonly string[] Phases = { "Existing", "Proposed" };

        private static ScannedSchedule Softscape()
        {
            return CreateFixture.Softscape(
                "FM-05",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)", "HEIGHT (m)" },
                new[] { "TREES", "", "", "" },
                new[] { "Existing", "", "", "" },
                new[] { "-", "PHOENIX DACTYLIFERA", "27", "25" },
                new[] { "-", "UNKNOWN", "16", "-" },
                new[] { "", "", "43", "" },
                new[] { "Proposed", "", "", "" },
                new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "10", "15" },
                new[] { "", "", "10", "" },
                new[] { "TOTAL", "", "53", "" });
        }

        /// <summary>
        /// FM-05 GRASS as measured: Existing 96 over 117, Proposed 69 over 84, the group 165
        /// over 201, and the last row is the one taken.
        /// </summary>
        private static ScannedSchedule ShrubsAndLawn()
        {
            return CreateFixture.ShrubsAndLawn(
                "FM-05",
                new[] { "IMAGE", "BOTANICAL NAME", "AREA (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "Existing", "", "", "" },
                new[] { "-", "GRASS: CYNODON DACTYLON", "96 m\u00b2", "117" },
                new[] { "", "", "96 m\u00b2", "117" },
                new[] { "Proposed", "", "", "" },
                new[] { "Pennisetum setaceum.jpg", "GRASS: PENNISETUM SETACEUM", "69 m\u00b2", "84" },
                new[] { "", "", "69 m\u00b2", "84" },
                new[] { "", "", "165 m\u00b2", "201" },
                new[] { "TOTAL", "", "165 m\u00b2", "201" });
        }

        private static PlotReading Fm05()
        {
            SoftscapeReading trees = SoftscapeRows.Read(Softscape(), Phases, "FM-05");
            ShrubsAndLawnReading ground = ShrubsAndLawnRows.Read(ShrubsAndLawn(), new[] { KpiMerge.LawnHeading });

            return CreateFixture.Plot("FM-05",
                species: trees.Species.ToArray(),
                subtotals: ground.Subtotals.ToArray(),
                softscapeTotalRead: trees.TotalRead, softscapeTotal: trees.Total, rowsPassedOver: trees.RowsPassedOver,
                printedSchedules: new[] { Softscape(), ShrubsAndLawn() }, softscapeTotalRow: trees.TotalRow);
        }

        private static string Spaces(int howMany)
        {
            return new string(' ', howMany);
        }

        private static string Report(params PlotReading[] readings)
        {
            return KpiCreateReport.Write(CreateFixture.Run(readings: readings), new DateTime(2026, 9, 10, 14, 28, 0));
        }

        [Fact]
        public void TheTopOfTheReportSaysTheSectionIsThereAndItIsLast()
        {
            string report = Report(Fm05());

            Assert.Contains(
                "Every schedule this run read is printed as the schedule prints it, at the end of this\r\n"
                + "file under EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT, so every number above it can be held against the drawing.\r\n",
                report);

            int section = report.IndexOf("== EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT (2) ==", StringComparison.Ordinal);
            int choices = report.IndexOf("== WHERE EVERY VALUE CAME FROM", StringComparison.Ordinal);
            Assert.True(section > choices, "the schedules come after the last of the other sections");
            Assert.DoesNotContain("== ", report.Substring(section + 10));
        }

        /// <summary>
        /// The block names the schedule, says which rows were read as what, and prints every
        /// row with every column aligned, numbering the heading row 1 the way every reader
        /// here does.
        /// </summary>
        [Fact]
        public void ASoftscapeScheduleIsPrintedAlignedWithWhatWasReadOffIt()
        {
            string report = Report(Fm05());

            Assert.Contains(
                "  FM-05, FM-05-(600) SOFTSCAPE SCHEDULE\r\n"
                + "    read as species rows: 3, rows 4, 5, 8, passed over as subtotals: 2, TOTAL row: row 10\r\n"
                + "    rows printed 10, shown 10\r\n"
                + "     1 | IMAGE               | BOTANICAL NAME      | COUNT (n) | HEIGHT (m)\r\n"
                + "     2 | TREES               | -                   | -         | -         \r\n"
                + "     3 | Existing            | -                   | -         | -         \r\n"
                + "     4 | -                   | PHOENIX DACTYLIFERA | 27        | 25        \r\n"
                + "     5 | -                   | UNKNOWN             | 16        | -         \r\n"
                + "     6 | -                   | -                   | 43        | -         \r\n"
                + "     7 | Proposed            | -                   | -         | -         \r\n"
                + "     8 | Albizia lebbeck.jpg | ALBIZIA LEBBECK     | 10        | 15        \r\n"
                + "     9 | -                   | -                   | 10        | -         \r\n"
                + "    10 | TOTAL               | -                   | 53        | -         \r\n",
                report);
        }

        /// <summary>
        /// For the shrubs and lawn schedule, WHICH SUBTOTAL ROW WAS TAKEN and why the others
        /// were not: the last of the group's subtotal rows, the ones above it being the phase
        /// subtotals that add to it.
        /// </summary>
        [Fact]
        public void AShrubsAndLawnScheduleSaysWhichSubtotalRowWasTakenAndWhy()
        {
            string report = Report(Fm05());

            Assert.Contains(
                "  FM-05, FM-05-(600) SHRUBS AND LAWN SCHEDULE\r\n"
                + "    read as group values: GRASS taken off row 9, the last of its 3 subtotal rows (5, 8, 9), "
                + "the rows above it are the phase subtotals that add to it and are not taken\r\n"
                + "    rows printed 10, shown 10\r\n",
                report);
            // Widths by hand: the longest image name is 23, the longest botanical name 26, the
            // area heading 10 and the count heading 9.
            Assert.Contains("     9 | -" + Spaces(22) + " | -" + Spaces(25) + " | 165 m\u00b2     | 201      \r\n", report);
        }

        [Fact]
        public void TheSubtotalRowsTravelOnTheGroupValue()
        {
            ShrubsAndLawnReading ground = ShrubsAndLawnRows.Read(ShrubsAndLawn(), new[] { KpiMerge.LawnHeading });

            GroupSubtotal grass = Assert.Single(ground.Subtotals);
            Assert.Equal(165.0, grass.SquareMetres);
            Assert.Equal(9, grass.RowNumber);
            Assert.Equal(new[] { 5, 8, 9 }, grass.RowsConsidered);
        }

        /// <summary>
        /// Two hundred rows a schedule and no more, said in those numbers.
        /// </summary>
        [Fact]
        public void AScheduleLongerThanTwoHundredRowsIsCutAndSaysHowManyOfHowMany()
        {
            var rows = new List<string[]> { new[] { "BOTANICAL NAME", "COUNT (n)" }, new[] { "Proposed", "" } };
            for (int at = 0; at < 248; at++) rows.Add(new[] { "Made up tree " + at, "1" });
            ScannedSchedule schedule = CreateFixture.Softscape("EP-05", rows.ToArray());

            PlotReading reading = CreateFixture.Plot("EP-05",
                species: SoftscapeRows.Read(schedule, Phases, "EP-05").Species.ToArray(),
                printedSchedules: new[] { schedule });

            string report = Report(reading);

            Assert.Contains("    rows printed 250, shown 200 of 250, the rest are cut off here\r\n", report);
            Assert.Contains("\r\n    200 | Made up tree 197 | 1        \r\n", report);
            Assert.DoesNotContain("Made up tree 198", report);
        }

        [Fact]
        public void APlotThatReadNoScheduleAddsNoBlock()
        {
            string report = Report(CreateFixture.Plot("FM-07", softscapeRead: false, shrubsAndLawnRead: false));

            Assert.Contains("== EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT (0) ==", report);
            Assert.DoesNotContain("  FM-07, ", report);
        }
    }
}
