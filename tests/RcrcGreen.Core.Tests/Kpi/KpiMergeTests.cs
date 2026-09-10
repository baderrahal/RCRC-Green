using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiMergeTests
    {
        [Fact]
        public void OnePlotAddsNothingAndKeepsItsOwnNumbers()
        {
            PlotReading plot = CreateFixture.Plot(
                "DM-11",
                subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.LawnHeading, 35.0, 46),
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 70.0, 58)
                });

            Totalled shrubs = KpiMerge.Shrubs(new[] { plot });
            Totalled lawn = KpiMerge.Lawn(new[] { plot });

            Assert.Equal(70.0, shrubs.Total);
            Assert.Equal(35.0, lawn.Total);
            Assert.Equal(new[] { "DM-11" }, shrubs.PerPlot.Select(one => one.PlotId));
            Assert.True(shrubs.Adds);
            Assert.True(lawn.Adds);
        }

        [Fact]
        public void TwoPlotsAddTheirShrubsAndTheirLawnSeparately()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-11", subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 70.0, 58),
                    CreateFixture.Subtotal(KpiMerge.LawnHeading, 35.0, 46)
                }),
                CreateFixture.Plot("DM-12", subtotals: new[]
                {
                    CreateFixture.Subtotal(KpiMerge.ShrubsHeading, 30.0, 20),
                    CreateFixture.Subtotal(KpiMerge.LawnHeading, 5.0, 4)
                })
            };

            Assert.Equal(100.0, KpiMerge.Shrubs(plots).Total);
            Assert.Equal(40.0, KpiMerge.Lawn(plots).Total);
        }

        /// <summary>
        /// ALBIZIA LEBBECK proposed on DM-12 is 13 and on DM-13 is 5, so the merged proposed
        /// row is 18. Measured on the 1355 run.
        /// </summary>
        [Fact]
        public void TwoPlotsSharingASpeciesInTheSameGroupMergeIntoOneRow()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-12", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13)
                }),
                CreateFixture.Plot("DM-13", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 5)
                })
            };

            MergedSpecies merged = Assert.Single(KpiMerge.Species(plots, CreateFixture.Counted));

            Assert.Equal("ALBIZIA LEBBECK", merged.BotanicalName);
            Assert.Equal("Proposed", merged.GroupName);
            Assert.Equal(18, merged.Quantity);
            Assert.Equal(new[] { "DM-12", "DM-13" }, merged.Plots);
        }

        /// <summary>
        /// The same name under two groups is two rows and never one. ALBIZIA LEBBECK on DM-12
        /// is 1 existing and 13 proposed, and collapsing them would put 14 in one tree sheet.
        /// </summary>
        [Fact]
        public void TheSameSpeciesInTwoGroupsNeverMerges()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-12", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Existing, 1),
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13)
                })
            };

            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(plots, CreateFixture.Counted);

            Assert.Equal(2, merged.Count);
            Assert.Equal(1, merged.Single(one => one.GroupName == "Existing").Quantity);
            Assert.Equal(13, merged.Single(one => one.GroupName == "Proposed").Quantity);
        }

        [Fact]
        public void ASpeciesNameDifferingOnlyByCaseOrSpacingIsOneMergedRow()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-12", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13)
                }),
                CreateFixture.Plot("DM-13", species: new[]
                {
                    CreateFixture.Species("  albizia lebbeck ", CreateFixture.Proposed, 5)
                })
            };

            MergedSpecies merged = Assert.Single(KpiMerge.Species(plots, CreateFixture.Counted));

            Assert.Equal(18, merged.Quantity);
            Assert.Equal("ALBIZIA LEBBECK", merged.BotanicalName);
        }

        [Fact]
        public void ASpeciesRowUnderNoGroupIsHeldOutOfTheMergeAndNamedWithItsPlot()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-12", species: new[]
                {
                    CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Proposed, 13),
                    CreateFixture.Species("CASSIA GLAUCA", string.Empty, 10)
                })
            };

            Assert.Single(KpiMerge.Species(plots, CreateFixture.Counted));

            SpeciesRow loose = Assert.Single(KpiMerge.Ungrouped(plots));
            Assert.Equal("CASSIA GLAUCA", loose.BotanicalName);
            Assert.Equal(10, loose.Quantity);
            Assert.Equal("DM-12", loose.PlotId);
        }

        [Fact]
        public void APlotWithNoAreaContributesNothingToTheAreaTotal()
        {
            var plots = new[]
            {
                CreateFixture.Plot("DM-11", regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, 1000.0) }),
                CreateFixture.Plot("DM-12", regions: new RegionArea[0], chosenRegion: string.Empty)
            };

            Totalled area = KpiMerge.Area(plots);

            Assert.Equal(1000.0, area.Total);
            Assert.Equal(new[] { "DM-11" }, area.PerPlot.Select(one => one.PlotId));
        }

        /// <summary>
        /// MM-03 and MM-04 both read 12182.05561411 in the 00 link, the same to eight decimals.
        /// </summary>
        [Fact]
        public void TwoPlotsReportingAnIdenticalRawAreaAreFlaggedTogether()
        {
            var plots = new[]
            {
                CreateFixture.Plot("MM-03", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                }),
                CreateFixture.Plot("MM-04", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                }),
                CreateFixture.Plot("MM-05", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 900.0, 9687.5)
                })
            };

            IdenticalArea shared = Assert.Single(KpiMerge.IdenticalAreas(plots));

            Assert.Equal(new[] { "MM-03", "MM-04" }, shared.Plots);
            Assert.Equal(12182.05561411, shared.RawSquareFeet);
        }

        [Fact]
        public void TwoPlotsWhoseAreasDifferAreNotFlagged()
        {
            var plots = new[]
            {
                CreateFixture.Plot("MM-03", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561411)
                }),
                CreateFixture.Plot("MM-04", regions: new[]
                {
                    CreateFixture.Region(CreateFixture.Cadastral, 1131.7, 12182.05561412)
                })
            };

            Assert.Empty(KpiMerge.IdenticalAreas(plots));
        }

        [Fact]
        public void PlotsAgreeingOnTheComponentGiveTheValueAndPlotsDisagreeingGiveNone()
        {
            var agreeing = new[]
            {
                CreateFixture.Plot("FM-05", component: "FRIDAY MOSQUE"),
                CreateFixture.Plot("FM-06", component: "FRIDAY MOSQUE")
            };

            AgreedValue agreed = KpiMerge.Component(agreeing);
            Assert.True(agreed.Agrees);
            Assert.Equal("FRIDAY MOSQUE", agreed.Value);

            var differing = new[]
            {
                CreateFixture.Plot("FM-05", component: "FRIDAY MOSQUE"),
                CreateFixture.Plot("SC-03", component: "SCHOOL")
            };

            AgreedValue split = KpiMerge.Component(differing);
            Assert.False(split.Agrees);
            Assert.Equal(string.Empty, split.Value);
            Assert.Equal(new[] { "FRIDAY MOSQUE", "SCHOOL" }, split.Distinct);
        }

        [Fact]
        public void ATotalThatDoesNotEqualItsPartsSaysSo()
        {
            var perPlot = new[] { new PlotNumber("DM-11", 70.0), new PlotNumber("DM-12", 30.0) };

            Assert.True(new Totalled(perPlot, 100.0).Adds);
            Assert.False(new Totalled(perPlot, 105.0).Adds);
        }
    }
}
