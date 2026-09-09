using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class ReconciliationTests
    {
        /// <summary>
        /// A plot with something of everything on it. The area differs per plot on purpose,
        /// because two plots of one size are a finding rather than a background condition.
        /// </summary>
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
        public void EveryPlotTickedAndReadWithNumbersInItAddsUp()
        {
            var readings = new[] { Full("DM-11"), Full("DM-12", 250.0) };

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-11", "DM-12" }, readings, new[] { KpiMerge.Shrubs(readings) }, false);

            Assert.True(held.AddsUp);
            Assert.Empty(held.Refusals);
            Assert.Equal(2, held.Ticked.Count);
            Assert.Equal(2, held.Read.Count);
            Assert.Empty(held.ContributedNothing);
        }

        /// <summary>
        /// A plot list that goes in longer than it comes out is the failure the accounting
        /// exists to catch.
        /// </summary>
        [Fact]
        public void APlotTickedAndNotReadRefusesTheWriteAndIsNamed()
        {
            Reconciliation held = Reconciliation.Of(
                new[] { "DM-11", "DM-12", "DM-13" }, new[] { Full("DM-11") }, null, false);

            Assert.False(held.AddsUp);
            string refusal = Assert.Single(held.Refusals);
            Assert.Contains("3 plots were ticked and 1 were read", refusal);
            Assert.Contains("DM-12, DM-13", refusal);
        }

        [Fact]
        public void APlotReadThatWasNeverTickedRefusesTheWriteToo()
        {
            Reconciliation held = Reconciliation.Of(
                new[] { "DM-11" }, new[] { Full("DM-11"), Full("DM-99", 250.0) }, null, false);

            Assert.False(held.AddsUp);
            Assert.Contains(held.Refusals, one => one.Contains("not ticked") && one.Contains("DM-99"));
        }

        /// <summary>
        /// The schedules print their own subtotal and TOTAL rows and those are forbidden as a
        /// source. A total that does not equal the per plot numbers printed beside it is the
        /// sign one of them was reached for, so the write is refused.
        /// </summary>
        [Fact]
        public void ATotalThatDoesNotEqualItsPartsRefusesTheWrite()
        {
            var wrong = new Totalled(
                new[] { new PlotNumber("DM-11", 70.0), new PlotNumber("DM-12", 30.0) },
                105.0);

            Reconciliation held = Reconciliation.Of(
                new[] { "DM-11" }, new[] { Full("DM-11") }, new[] { wrong }, false);

            Assert.False(held.AddsUp);
            Assert.Contains(held.Refusals, one => one.Contains("does not equal the plot numbers"));
        }

        [Fact]
        public void TwoPlotsWithAnIdenticalAreaRefuseTheWriteUntilSomebodyConfirms()
        {
            var readings = new[]
            {
                CreateFixture.Plot("MM-03", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                }),
                CreateFixture.Plot("MM-04", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                })
            };
            var ticked = new[] { "MM-03", "MM-04" };

            Reconciliation refusing = Reconciliation.Of(ticked, readings, null, false);
            Assert.False(refusing.AddsUp);
            Assert.Contains(refusing.Refusals, one => one.Contains("report the same area"));
            Assert.Single(refusing.IdenticalAreas);

            Reconciliation confirmed = Reconciliation.Of(ticked, readings, null, true);
            Assert.True(confirmed.AddsUp);
            Assert.Single(confirmed.IdenticalAreas);
        }

        [Fact]
        public void APlotWithNoSoftscapeScheduleIsNamedAndDoesNotRefuseTheWrite()
        {
            var readings = new[]
            {
                Full("DM-11"),
                CreateFixture.Plot("DM-12", softscapeRead: false, subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 30.0, 20)
                }, regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 250.0) })
            };

            Reconciliation held = Reconciliation.Of(new[] { "DM-11", "DM-12" }, readings, null, false);

            Assert.True(held.AddsUp);
            Assert.Equal(new[] { "DM-12" }, held.WithoutSoftscape);
            Assert.Empty(held.WithoutShrubsAndLawn);
        }

        /// <summary>
        /// Which of a plot's two regions carries the area varies by plot and the type name
        /// cannot decide it, so two regions holding an area is a question for the user rather
        /// than a number the tool picks.
        /// </summary>
        [Fact]
        public void APlotWithTwoRegionsHoldingAnAreaRefusesUntilOneIsPicked()
        {
            var reading = CreateFixture.Plot(
                "NS-19",
                regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                    CreateFixture.Region(CreateFixture.OutOfScope, 250.0)
                },
                chosenRegion: string.Empty);

            Reconciliation refusing = Reconciliation.Of(new[] { "NS-19" }, new[] { reading }, null, false);

            Assert.False(refusing.AddsUp);
            Assert.Contains(refusing.Refusals,
                one => one.Contains("NS-19 has 2 filled regions holding an area"));

            var picked = CreateFixture.Plot(
                "NS-19",
                regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0),
                    CreateFixture.Region(CreateFixture.OutOfScope, 250.0)
                },
                chosenRegion: CreateFixture.Cadastral);

            Assert.True(Reconciliation.Of(new[] { "NS-19" }, new[] { picked }, null, false).AddsUp);
        }

        [Fact]
        public void APlotWithNoAreaIsNamedInWithoutArea()
        {
            var readings = new[]
            {
                Full("DM-11"),
                CreateFixture.Plot("DM-12",
                    species: new[] { CreateFixture.Species("CASSIA GLAUCA", CreateFixture.Proposed, 4) },
                    regions: new RegionArea[0],
                    chosenRegion: string.Empty)
            };

            Reconciliation held = Reconciliation.Of(new[] { "DM-11", "DM-12" }, readings, null, false);

            Assert.Equal(new[] { "DM-12" }, held.WithoutArea);
            Assert.True(held.AddsUp);
        }

        /// <summary>
        /// A plot that gave nothing at all is named with its reason, never quietly absent.
        /// </summary>
        [Fact]
        public void APlotThatContributedNothingIsNamedWithEveryReason()
        {
            var readings = new[]
            {
                Full("DM-11"),
                CreateFixture.Plot("DM-12",
                    softscapeRead: false,
                    shrubsAndLawnRead: false,
                    regions: new RegionArea[0],
                    chosenRegion: string.Empty)
            };

            Reconciliation held = Reconciliation.Of(new[] { "DM-11", "DM-12" }, readings, null, false);

            PlotAndReason nothing = Assert.Single(held.ContributedNothing);
            Assert.Equal("DM-12", nothing.PlotId);
            Assert.Equal(
                Reconciliation.NoSoftscape + ", " + Reconciliation.NoShrubsAndLawn + ", " + Reconciliation.NoArea,
                nothing.Reason);
        }
    }
}
