using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class PlotRegistryTests
    {
        [Fact]
        public void APlotWithNoViewsAtAllStillReachesTheList()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "DM-41-(200) General Arrangement Layout" },
                new[] { "PF-12" },
                new[] { "PF-12" });

            PlotRecord pf12 = result.Plots.Single(plot => plot.PlotId == "PF-12");

            Assert.Equal(new[] { PlotSource.ScopeBox, PlotSource.ElementParameter }, pf12.Sources);
            Assert.False(pf12.FoundIn(PlotSource.ViewName));
        }

        [Fact]
        public void OnePlotSeenThreeWaysIsListedOnce()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "DM-41-(010) Location Key Plan", "DM-41-(200) General Arrangement Layout" },
                new[] { "DM-41" },
                new[] { "DM-41", "DM-41" });

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal("DM-41", only.PlotId);
            Assert.Equal(
                new[] { PlotSource.ViewName, PlotSource.ScopeBox, PlotSource.ElementParameter },
                only.Sources);
        }

        [Fact]
        public void TheListIsTheUnionOfAllThreeSources()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "DM-41-(010) Location Key Plan" },
                new[] { "PF-12" },
                new[] { "AB-7" });

            Assert.Equal(new[] { "AB-7", "DM-41", "PF-12" }, result.Plots.Select(plot => plot.PlotId));
        }

        [Fact]
        public void EachSourceIsRecordedOnceEvenWhenTheSameNameRepeats()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                null,
                new[] { "DM-41", "DM-41", "DM-41" },
                null);

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal(new[] { PlotSource.ScopeBox }, only.Sources);
        }

        [Fact]
        public void NamesThatAreNotPlotIdentifiersAreReportedRatherThanDropped()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "Site Plan" },
                new[] { "Working box" },
                new[] { "" });

            Assert.Empty(result.Plots);
            Assert.Equal(new[] { "Site Plan", "Working box", "" }, result.Ignored);
        }

        [Fact]
        public void EmptyInputGivesAnEmptyListRatherThanAThrow()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>());

            Assert.Empty(result.Plots);
            Assert.Empty(result.Ignored);
        }

        [Fact]
        public void ANullInsideAListIsSteppedOverRatherThanCountedOrThrownOn()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { null, "DM-41-(010) Location Key Plan" },
                new string[] { null },
                new[] { "PF-12", null });

            Assert.Equal(new[] { "DM-41", "PF-12" }, result.Plots.Select(plot => plot.PlotId));
            Assert.Empty(result.Ignored);
        }

        [Fact]
        public void AScopeBoxNamedWithSomethingLongerThanThePlotIsNotAPlot()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                null,
                new[] { "DM-41 working", "DM-41" },
                null);

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal("DM-41", only.PlotId);
            Assert.Equal(new[] { "DM-41 working" }, result.Ignored);
        }
    }
}
