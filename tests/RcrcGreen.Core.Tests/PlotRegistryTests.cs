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
            Assert.Equal(
                new[] { "Site Plan", "Working box", "" },
                result.Ignored.Select(entry => entry.Text));
            Assert.All(result.Ignored, entry => Assert.Equal(IgnoredReason.NotAPlotName, entry.Reason));
        }

        [Fact]
        public void ALowerCaseIdentifierIsNotASecondPlotAndSaysWhyItWasIgnored()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "dm-41-(200) General Arrangement Layout" },
                new[] { "dm-41" },
                new[] { "Dm-41" });

            Assert.Empty(result.Plots);
            Assert.Equal(
                new[] { "dm-41-(200) General Arrangement Layout", "dm-41", "Dm-41" },
                result.Ignored.Select(entry => entry.Text));
            Assert.All(result.Ignored, entry => Assert.Equal(IgnoredReason.WrongCase, entry.Reason));
        }

        [Fact]
        public void AnUpperCaseScopeBoxAndALowerCaseViewNameDoNotBecomeTwoPlots()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "dm-41-(200) General Arrangement Layout" },
                new[] { "DM-41" },
                null);

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal("DM-41", only.PlotId);
            Assert.Equal(new[] { PlotSource.ScopeBox }, only.Sources);
            Assert.Equal(IgnoredReason.WrongCase, Assert.Single(result.Ignored).Reason);
        }

        [Fact]
        public void SurroundingWhitespaceOnAnIdentifierDoesNotMakeASecondPlot()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "DM-41-(200) General Arrangement Layout\n" },
                new[] { " DM-41", "DM-41\t" },
                new[] { "DM-41\r\n" });

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal("DM-41", only.PlotId);
            Assert.Equal(
                new[] { PlotSource.ViewName, PlotSource.ScopeBox, PlotSource.ElementParameter },
                only.Sources);
            Assert.Empty(result.Ignored);
        }

        [Fact]
        public void PlotsComeBackWithTheNumberPartInNumberOrder()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                null,
                new[] { "DM-100", "DM-2", "DM-9", "DM-41", "AB-10", "AB-2", "DM-1" },
                null);

            Assert.Equal(
                new[] { "AB-2", "AB-10", "DM-1", "DM-2", "DM-9", "DM-41", "DM-100" },
                result.Plots.Select(plot => plot.PlotId));
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

        /// <summary>
        /// PRX_Plot_ID on a view is its own source, because 1,269 views on the real model
        /// have a plot in the parameter and a name that does not parse, and a plot found that
        /// way still has views. Filing it under elements would mark it as needing everything.
        /// </summary>
        [Fact]
        public void APlotFoundOnlyThroughAViewParameterStillHasViews()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "Site Plan" },
                new[] { "DM-41" },
                null,
                null);

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal("DM-41", only.PlotId);
            Assert.Equal(new[] { PlotSource.ViewParameter }, only.Sources);
            Assert.True(only.HasViews);
            Assert.Equal(string.Empty, only.NoViewsInWords());
        }

        [Fact]
        public void TheNameAndTheParameterAreTwoSourcesOnOnePlot()
        {
            PlotRegistryResult result = PlotRegistry.Build(
                new[] { "DM-41-(200) General Arrangement Layout" },
                new[] { "DM-41" },
                null,
                null);

            PlotRecord only = Assert.Single(result.Plots);

            Assert.Equal(
                new[] { PlotSource.ViewName, PlotSource.ViewParameter }, only.Sources);
        }

        /// <summary>
        /// The suffix on a plot's row in step 1, written out by hand. A plot with views says
        /// nothing, because the quiet case is the ordinary one.
        /// </summary>
        [Fact]
        public void APlotWithNoViewsSaysWhatItHasInstead()
        {
            PlotRecord boxOnly = new PlotRecord("AB-7", new[] { PlotSource.ScopeBox });
            PlotRecord elementsOnly = new PlotRecord("AB-8", new[] { PlotSource.ElementParameter });
            PlotRecord both = new PlotRecord(
                "AB-9", new[] { PlotSource.ScopeBox, PlotSource.ElementParameter });

            Assert.Equal("box only, no views", boxOnly.NoViewsInWords());
            Assert.Equal("elements only, no views", elementsOnly.NoViewsInWords());
            Assert.Equal("box and elements, no views", both.NoViewsInWords());
        }

        [Fact]
        public void EverySourceHasItsTooltipWords()
        {
            PlotRecord all = new PlotRecord("DM-41", new[]
            {
                PlotSource.ViewName,
                PlotSource.ViewParameter,
                PlotSource.ScopeBox,
                PlotSource.ElementParameter
            });

            Assert.Equal(
                "view names, a scope box, PRX_Plot_ID on elements, PRX_Plot_ID on views",
                all.SourcesInWords());
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
            Assert.Equal(new[] { "DM-41 working" }, result.Ignored.Select(entry => entry.Text));
        }
    }
}
