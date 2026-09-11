using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 35. The loop over the ticked plots read each one under no guard of its own, so
    /// a throw on plot 60 of 78 fell to the run's catches, which said Revit would not do that
    /// now with no plot named and wrote nothing. A guard per plot now records the refusal on
    /// that plot's reading with what was thrown, the run carries on, the reconciliation refuses
    /// the write naming the plot, and the report is written either way. The guard itself runs
    /// only in Revit. What Core can prove is the reading it hands back and everything that
    /// reading does downstream, every expected value written out by hand.
    /// </summary>
    public class PlotReadThrewTests
    {
        private const string Thrown = "InvalidOperationException: The schedule is not valid.";

        private const string Refusal = "the read threw and nothing on this plot was read. " + Thrown;

        private static PlotReading Full(string plotId, double squareMetres = 1000.0)
        {
            return CreateFixture.Plot(
                plotId,
                species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13) },
                subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 70.0, 58),
                    CreateFixture.Subtotal(KpiMerge.LawnHeading, 35.0, 46)
                },
                regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, squareMetres) });
        }

        [Fact]
        public void APlotWhoseReadThrewCarriesWhatWasThrownAndNothingElse()
        {
            PlotReading reading = PlotReading.NotRead("DM-60", Thrown, 2.5);

            Assert.Equal("DM-60", reading.PlotId);
            Assert.Equal(new[] { Refusal }, reading.ReadRefusals);
            Assert.Empty(reading.SoftscapeSchedules);
            Assert.Empty(reading.ShrubsAndLawnSchedules);
            Assert.Empty(reading.Species);
            Assert.Empty(reading.Subtotals);
            Assert.Empty(reading.Regions);
            Assert.Null(reading.ChosenRegion);
            Assert.Equal(string.Empty, reading.Component);
            Assert.Equal(string.Empty, reading.Reference);
            Assert.Equal(2.5, reading.ReadSeconds);
            Assert.False(reading.SoftscapeRead);
            Assert.False(reading.ShrubsAndLawnRead);

            Assert.Throws<ArgumentException>(() => PlotReading.NotRead("DM-60", " ", 0.0));
        }

        /// <summary>
        /// Three ticked, three read, one of them refused: the write is refused naming the plot
        /// and the throw, the two refusals for a short or a long list stay silent, and the plot
        /// is named under contributed nothing with the refusal first.
        /// </summary>
        [Fact]
        public void APlotWhoseReadThrewRefusesTheWriteNamingThePlotAndTheRestAreStillRead()
        {
            var readings = new[] { Full("DM-59"), PlotReading.NotRead("DM-60", Thrown, 2.5), Full("DM-61", 250.0) };

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-59", "DM-60", "DM-61" }, readings, null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.Equal(new[] { "DM-60: " + Refusal }, held.Refusals);
            Assert.Equal(3, held.Ticked.Count);
            Assert.Equal(3, held.Read.Count);
            Assert.DoesNotContain(held.Refusals, one => one.Contains("were ticked and"));

            PlotAndReason nothing = Assert.Single(held.ContributedNothing);
            Assert.Equal("DM-60", nothing.PlotId);
            Assert.StartsWith("a schedule read on it was refused, see above", nothing.Reason, StringComparison.Ordinal);
            Assert.Equal(new[] { "DM-60" }, held.WithoutSoftscape);
            Assert.Equal(new[] { "DM-60" }, held.WithoutArea);
        }

        /// <summary>
        /// The plots around the one that threw still add up on their own: 1000 plus 250 is
        /// 1250 over two plots, 70 plus 70 is 140, and 13 plus 13 is 26. The empty component
        /// of the plot that threw does not make the others disagree.
        /// </summary>
        [Fact]
        public void TheOtherPlotsStillAddUpAroundThePlotThatThrew()
        {
            var readings = new[] { Full("DM-59"), PlotReading.NotRead("DM-60", Thrown, 2.5), Full("DM-61", 250.0) };

            Assert.Equal(1250.0, KpiMerge.Area(readings).Total);
            Assert.Equal(2, KpiMerge.Area(readings).PerPlot.Count);
            Assert.Equal(140.0, KpiMerge.Shrubs(readings).Total);

            MergedSpecies albizia = Assert.Single(KpiMerge.Species(readings, CreateFixture.Counted));
            Assert.Equal(26, albizia.Quantity);
            Assert.True(KpiMerge.Component(readings).Agrees);
            Assert.Equal("FRIDAY MOSQUE", KpiMerge.Component(readings).Value);
        }

        /// <summary>
        /// The report is written either way and says it in both places: among the reasons,
        /// and under the plot. Two ticked and two read, because the plot was read and refused
        /// rather than skipped.
        /// </summary>
        [Fact]
        public void APlotWhoseReadThrewPrintsUnderThePlotAndInTheReconciliation()
        {
            KpiCreateRun run = CreateFixture.Run(new[] { Full("DM-59"), PlotReading.NotRead("DM-60", Thrown, 2.5) });

            string report = KpiCreateReport.Write(run, new DateTime(2026, 9, 11, 9, 0, 0));

            Assert.Contains("== RECONCILIATION (1) ==", report);
            Assert.Contains("  DM-60: " + Refusal + "\r\n", report);
            Assert.Contains("    REFUSED: " + Refusal + "\r\n", report);
            Assert.Contains("  plots ticked            2\r\n", report);
            Assert.Contains("  plots read              2\r\n", report);
        }

        [Fact]
        public void TheStatusLineCountsTheThrowAsAReasonShownAboveTheButton()
        {
            KpiCreateRun run = CreateFixture.Run(new[] { Full("DM-59"), PlotReading.NotRead("DM-60", Thrown, 2.5) });

            string line = CreateWords.Wrote(run, @"C:\reports\r.txt");

            Assert.StartsWith(CreateWords.NothingWritten + " 1 reason, shown in full above the Create button.", line, StringComparison.Ordinal);
            Assert.EndsWith("Report: C:\\reports\\r.txt", line, StringComparison.Ordinal);
        }
    }
}
