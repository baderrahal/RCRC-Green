using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiCreateReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 9, 16, 8, 14);

        private static string[] LinesOf(KpiCreateRun run)
        {
            return KpiCreateReport.Write(run, Noon).Split(new[] { "\r\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// The measured DM-12 numbers. H7 took 3728.7570000000005 converted from the raw
        /// 40136.006313679296 square feet, and the schedule prints 3729.
        ///
        /// **The report used to end saying every number above came off a row the schedule
        /// printed.** The area did not. It came off a filled region parameter, and the sentence
        /// was wrong about the one number that needed explaining.
        /// </summary>
        [Fact]
        public void TheAreaRowShowsTheRawTheConvertedAndWhatTheModelPrints()
        {
            string[] lines = LinesOf(CreateFixture.Run());

            Assert.Contains(
                "  plot | chosen type | raw square feet | written square metres | "
                + "as the model prints it | offered",
                lines);

            string row = lines.Single(line => line.StartsWith("  DM-12 | ", StringComparison.Ordinal));

            Assert.Contains("40136.006313679296", row);
            Assert.Contains("3728.7570000000005", row);
            Assert.Contains("3728.76 m2", row);
        }

        /// <summary>
        /// No line may claim the area came off a schedule row, because it did not.
        /// </summary>
        [Fact]
        public void NoLineSaysEveryNumberCameOffAScheduleRow()
        {
            string[] lines = LinesOf(CreateFixture.Run());

            Assert.DoesNotContain(lines,
                line => line.Contains("Every number above came off a row the schedule printed"));
        }

        [Fact]
        public void TheReportSaysWhatTheAreaReallyCameOff()
        {
            string[] lines = LinesOf(CreateFixture.Run());

            Assert.Contains(
                "  THE AREA IS NOT A SCHEDULE ROW. It is PRX_Intervention Area read off the",
                lines);
            Assert.Contains(
                "  chosen filled region in the 00 link, raw in square feet, converted here to",
                lines);
        }

        /// <summary>
        /// The claim that does hold is kept and narrowed to the numbers it is true of.
        /// </summary>
        [Fact]
        public void TheScheduleClaimIsKeptForTheNumbersItIsTrueOf()
        {
            string[] lines = LinesOf(CreateFixture.Run());

            Assert.Contains(
                "  The shrubs, the lawn and every tree quantity came off a row the schedule",
                lines);
            Assert.Contains(lines, line => line.Contains("RVT Link instances"));
        }

        /// <summary>
        /// A raw reading is printed with nothing taken off it. Rounding it for reading would
        /// hide the very difference the row exists to show.
        /// </summary>
        [Fact]
        public void TheRawNumberIsNotRoundedForReading()
        {
            string[] lines = LinesOf(CreateFixture.Run());
            string row = lines.Single(line => line.StartsWith("  DM-12 | ", StringComparison.Ordinal));

            Assert.DoesNotContain("40136.01", row);
            Assert.DoesNotContain("3728.76 | ", row);
        }

        /// <summary>
        /// A plot with no region chosen says so in every one of the three columns rather than
        /// printing a zero, which reads as a measurement.
        /// </summary>
        [Fact]
        public void APlotWithNoRegionChosenSaysNoneRatherThanZero()
        {
            KpiCreateRun run = CreateFixture.Run(new[]
            {
                CreateFixture.Plot("NS-19", regions: new RegionArea[0], chosenRegion: string.Empty)
            });

            string[] lines = LinesOf(run);
            string row = lines.Single(line => line.StartsWith("  NS-19 | ", StringComparison.Ordinal));

            Assert.Equal("  NS-19 | (none) | (none) | (none) | (none) | (none)", row);
        }

        /// <summary>
        /// The three the team types land in the report's written cells, so a filled checklist
        /// can be checked against who filled it and when.
        /// </summary>
        [Fact]
        public void WhatTheTeamTypedIsAmongTheCellsTheRunWouldWrite()
        {
            KpiCreateRun run = CreateFixture.Run();

            Assert.Equal("2026-09-09", run.Plan.Writes.Single(one => one.Cell.ToString() == "E5").Stored);
            Assert.Equal("xx", run.Plan.Writes.Single(one => one.Cell.ToString() == "G5").Stored);
            Assert.Equal("bb", run.Plan.Writes.Single(one => one.Cell.ToString() == "H5").Stored);
        }
    }
}
