using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// FM-05 holds two schedules whose names hold SOFTSCAPE. The reader appended both and the
    /// merge added them by name and group, so the first twenty plot run printed FM-05 twice in
    /// one species row, FM-05 10, FM-05 10, and counted that plot's trees twice. The shrubs and
    /// lawn read had the other half: the first group with the heading was taken and a second
    /// schedule was dropped in silence.
    ///
    /// One of a kind is read. Two or more of a kind are named, none is read, and the write is
    /// refused naming the plot, the kind and every schedule found.
    /// </summary>
    public class OneScheduleOfAKindTests
    {
        private const string First = "FM-05-(600) SOFTSCAPE SCHEDULE";

        private const string Second = "FM-05-(600) SOFTSCAPE SCHEDULE Copy 1";

        private const string Ground = "FM-05-(600) SHRUBS AND LAWN SCHEDULE";

        private const string GroundCopy = "FM-05-(600) SHRUBS AND LAWN SCHEDULE Copy 1";

        private static PlotReading TwoOfEach()
        {
            return CreateFixture.Plot(
                "FM-05",
                softscapeSchedules: new[] { First, Second },
                shrubsAndLawnSchedules: new[] { Ground, GroundCopy });
        }

        /// <summary>
        /// The area differs per plot on purpose, because two plots of one size are a finding
        /// of their own and would refuse the write for a reason this file is not about.
        /// </summary>
        private static PlotReading OneOfEach(string plotId, int albizia, double squareMetres = 250.0)
        {
            return CreateFixture.Plot(
                plotId,
                species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, albizia) },
                subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 820.0, 459),
                    CreateFixture.Subtotal(KpiMerge.LawnHeading, 165.0, 46)
                },
                regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, squareMetres) });
        }

        /// <summary>
        /// Numbers off two schedules of one kind are the doubled count, so a reading cannot be
        /// built holding species rows beside two softscape names, or subtotals beside two
        /// shrubs and lawn names, or either beside none.
        /// </summary>
        [Fact]
        public void AReadingCannotCarryNumbersOffTwoSchedulesOfOneKindOrOffNone()
        {
            Assert.Throws<ArgumentException>(() => CreateFixture.Plot(
                "FM-05",
                species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 10) },
                softscapeSchedules: new[] { First, Second }));

            Assert.Throws<ArgumentException>(() => CreateFixture.Plot(
                "FM-05",
                subtotals: new[] { CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 820.0, 459) },
                shrubsAndLawnSchedules: new[] { Ground, GroundCopy }));

            Assert.Throws<ArgumentException>(() => CreateFixture.Plot(
                "FM-05",
                species: new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 10) },
                softscapeRead: false));
        }

        [Fact]
        public void OneOfEachIsReadAndTwoOfEachIsNeither()
        {
            PlotReading one = OneOfEach("FM-06", 15);
            Assert.True(one.SoftscapeRead);
            Assert.True(one.ShrubsAndLawnRead);
            Assert.False(one.MoreThanOneSoftscape);
            Assert.Equal(new[] { "FM-06-(600) SOFTSCAPE SCHEDULE" }, one.SoftscapeSchedules);
            Assert.Equal(new[] { "FM-06-(600) SHRUBS AND LAWN SCHEDULE" }, one.ShrubsAndLawnSchedules);

            PlotReading two = TwoOfEach();
            Assert.False(two.SoftscapeRead);
            Assert.False(two.ShrubsAndLawnRead);
            Assert.True(two.MoreThanOneSoftscape);
            Assert.True(two.MoreThanOneShrubsAndLawn);
            Assert.Empty(two.Species);
            Assert.Empty(two.Subtotals);
        }

        [Fact]
        public void TwoSoftscapeSchedulesOnOnePlotRefuseTheWriteNamingBoth()
        {
            var readings = new[] { TwoOfEach(), OneOfEach("FM-06", 15) };

            Reconciliation held = Reconciliation.Of(
                new[] { "FM-05", "FM-06" }, readings, null, false, KpiTemplates.Mosques);

            Assert.False(held.AddsUp);
            Assert.Contains(
                "FM-05 holds 2 softscape schedules, FM-05-(600) SOFTSCAPE SCHEDULE and "
                + "FM-05-(600) SOFTSCAPE SCHEDULE Copy 1, and nothing says which is the real one, so none of them was read.",
                held.Refusals);
            Assert.Contains(
                "FM-05 holds 2 shrubs and lawn schedules, FM-05-(600) SHRUBS AND LAWN SCHEDULE and "
                + "FM-05-(600) SHRUBS AND LAWN SCHEDULE Copy 1, and nothing says which is the real one, so none of them was read.",
                held.Refusals);
            Assert.Equal(2, held.Refusals.Count);

            // A plot holding two is neither with one nor without.
            Assert.Equal(new[] { "FM-05" }, held.WithMoreThanOneSoftscape);
            Assert.Equal(new[] { "FM-05" }, held.WithMoreThanOneShrubsAndLawn);
            Assert.Empty(held.WithoutSoftscape);
            Assert.Empty(held.WithoutShrubsAndLawn);
        }

        [Fact]
        public void APlotWithOneOfEachStillAddsUp()
        {
            var readings = new[] { OneOfEach("FM-05", 10, 900.0), OneOfEach("FM-06", 15, 250.0) };

            Reconciliation held = Reconciliation.Of(
                new[] { "FM-05", "FM-06" }, readings, null, false, KpiTemplates.Mosques);

            Assert.True(held.AddsUp, string.Join(" ", held.Refusals));
            Assert.Empty(held.WithMoreThanOneSoftscape);
            Assert.Empty(held.WithMoreThanOneShrubsAndLawn);
        }

        /// <summary>
        /// FM-05 appears once in the merged row, with FM-06's number, because nothing was read
        /// off FM-05. The run printed FM-05 10, FM-05 10, FM-06 15 and a total of 35 where the
        /// plot list held 25 real trees at most.
        /// </summary>
        [Fact]
        public void APlotWithTwoSoftscapeSchedulesContributesNoSpeciesToTheMerge()
        {
            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(new[] { TwoOfEach(), OneOfEach("FM-06", 15) });

            MergedSpecies albizia = Assert.Single(merged);
            Assert.Equal(15, albizia.Quantity);
            Assert.Equal(new[] { "FM-06" }, albizia.Plots);
        }

        [Fact]
        public void APlotWithTwoShrubsAndLawnSchedulesContributesNoAreaToEitherTotal()
        {
            var readings = new[] { TwoOfEach(), OneOfEach("FM-06", 15) };

            Assert.Equal(new[] { "FM-06" }, KpiMerge.Shrubs(readings).PerPlot.Select(one => one.PlotId));
            Assert.Equal(820.0, KpiMerge.Shrubs(readings).Total);
            Assert.Equal(165.0, KpiMerge.Lawn(readings).Total);
        }

        /// <summary>
        /// Per plot, which schedule each number came off, by name. The report used to say
        /// "softscape schedule: 11 species rows read" and could not say which of FM-05's two.
        /// </summary>
        [Fact]
        public void TheReportNamesTheScheduleEachNumberCameOffPerPlot()
        {
            var readings = new[]
            {
                TwoOfEach(),
                OneOfEach("FM-06", 15),
                CreateFixture.Plot("FM-07", softscapeRead: false, shrubsAndLawnRead: false)
            };

            string report = KpiCreateReport.Write(
                CreateFixture.Run(readings: readings), new DateTime(2026, 9, 10, 11, 16, 0));

            Assert.Contains(
                "  FM-05\r\n    PRX_Component: FRIDAY MOSQUE\r\n    PRX_Plot_UID2: ANH-007-MO-100019\r\n"
                + "    softscape schedules: 2 FOUND AND NONE READ, FM-05-(600) SOFTSCAPE SCHEDULE | FM-05-(600) SOFTSCAPE SCHEDULE Copy 1\r\n"
                + "    shrubs and lawn schedules: 2 FOUND AND NONE READ, FM-05-(600) SHRUBS AND LAWN SCHEDULE | FM-05-(600) SHRUBS AND LAWN SCHEDULE Copy 1\r\n",
                report);

            Assert.Contains(
                "  FM-06\r\n    PRX_Component: FRIDAY MOSQUE\r\n    PRX_Plot_UID2: ANH-007-MO-100019\r\n"
                + "    softscape schedule: FM-06-(600) SOFTSCAPE SCHEDULE, 1 species row read\r\n"
                + "      species rows add to 15, no TOTAL row was found to hold that against\r\n"
                + "    shrubs and lawn schedule: FM-06-(600) SHRUBS AND LAWN SCHEDULE, 2 groups read\r\n",
                report);

            Assert.Contains(
                "  FM-07\r\n    PRX_Component: FRIDAY MOSQUE\r\n    PRX_Plot_UID2: ANH-007-MO-100019\r\n"
                + "    softscape schedule: none filters on this plot, so nothing was read\r\n"
                + "    shrubs and lawn schedule: none filters on this plot, so nothing was read\r\n",
                report);

            Assert.Contains("  plots with one softscape schedule   1, without: FM-07, with more than one: FM-05\r\n", report);
            Assert.Contains("  plots with one shrubs and lawn schedule   1, without: FM-07, with more than one: FM-05\r\n", report);
        }

        /// <summary>
        /// A plot that gave nothing because it held two of each says so, in the reason beside
        /// its name, rather than reading as a plot with no schedule.
        /// </summary>
        [Fact]
        public void APlotThatGaveNothingBecauseItHeldTwoOfEachSaysSo()
        {
            var reading = CreateFixture.Plot(
                "FM-05",
                softscapeSchedules: new[] { First, Second },
                shrubsAndLawnSchedules: new[] { Ground, GroundCopy },
                regions: new RegionArea[0],
                chosenRegion: string.Empty);

            Reconciliation held = Reconciliation.Of(new[] { "FM-05" }, new[] { reading }, null, false, KpiTemplates.Mosques);

            PlotAndReason nothing = Assert.Single(held.ContributedNothing);
            Assert.Equal(
                "2 softscape schedules and none read, see above, 2 shrubs and lawn schedules and none read, see above, "
                + Reconciliation.NoArea,
                nothing.Reason);
        }
    }
}
