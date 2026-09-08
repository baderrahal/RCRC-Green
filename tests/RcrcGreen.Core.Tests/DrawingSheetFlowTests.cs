using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The whole Core path in one go, on the shape of data the add-in will hand it. The pieces
    /// each have their own tests. This one covers the seams between them.
    /// </summary>
    public class DrawingSheetFlowTests
    {
        private static readonly string[] ViewNamesInModel =
        {
            "DM-41-(010) Location Key Plan",
            "DM-41-(010) Overall Key Plan",
            "DM-41-(200) General Arrangement Layout",
            "DM-41-(400) Landscape Cross Section",
            "PF-12-(200) General Arrangement Layout",
            "Site Plan",
            "Level 00"
        };

        private static readonly string[] ScopeBoxNames = { "DM-41", "PF-12", "AB-7" };

        private static readonly string[] ElementPlotIds = { "DM-41", "AB-7", "AB-7" };

        [Fact]
        public void ThePlotWithOnlyAScopeBoxAndTaggedElementsIsTheOneWithEverythingMissing()
        {
            PlotRegistryResult plots = PlotRegistry.Build(ViewNamesInModel, ScopeBoxNames, ElementPlotIds);

            List<ParsedViewName> parsedNames = ViewNamesInModel
                .Select(name =>
                {
                    ParsedViewName parsed;
                    return ViewNameParser.TryParse(name, out parsed) ? parsed : null;
                })
                .Where(parsed => parsed != null)
                .ToList();

            PlotViewGrid grid = PlotViewGrid.Build(parsedNames, plots.Plots.Select(plot => plot.PlotId));
            IReadOnlyList<PlotMissingViews> reports = MissingViewFinder.Find(grid);

            Assert.Equal(new[] { "AB-7", "DM-41", "PF-12" }, plots.Plots.Select(plot => plot.PlotId));
            Assert.Equal(new[] { "Site Plan", "Level 00" }, plots.Ignored);

            PlotRecord ab7 = plots.Plots.Single(plot => plot.PlotId == "AB-7");
            Assert.Equal(new[] { PlotSource.ScopeBox, PlotSource.ElementParameter }, ab7.Sources);

            PlotMissingViews ab7Report = reports.Single(report => report.PlotId == "AB-7");
            Assert.Equal(grid.ViewTypes.Count, ab7Report.Missing.Count);
            Assert.Equal(4, ab7Report.Missing.Count);

            Assert.True(reports.Single(report => report.PlotId == "DM-41").IsComplete);

            // PF-12 holds the general arrangement layout and nothing else, so it lacks the two
            // key plans and the cross section.
            Assert.Equal(
                new[]
                {
                    new ViewType("010", "Location Key Plan"),
                    new ViewType("010", "Overall Key Plan"),
                    new ViewType("400", "Landscape Cross Section")
                },
                reports.Single(report => report.PlotId == "PF-12").Missing.Select(entry => entry.ViewType));
        }

        [Fact]
        public void EveryPlotWithAScopeBoxGetsASectionAcrossTheMiddleOfIt()
        {
            var boxes = new[]
            {
                new PlotBox("DM-41", 0, 0, 0, 300, 80, 20),
                new PlotBox("PF-12", 400, 0, 0, 480, 350, 20),
                new PlotBox("AB-7", -100, -100, -5, -40, -30, 5)
            };

            foreach (PlotBox box in boxes)
            {
                SectionPlacement placement = SectionPlacement.Across(box, SectionAxis.ShortSide);

                double shorter = box.WidthX < box.WidthY ? box.WidthX : box.WidthY;

                Assert.Equal(box.PlotName, placement.PlotName);
                Assert.Equal(shorter, placement.Length, 9);
                Assert.Equal(box.CentreX, placement.Midpoint.X, 9);
                Assert.Equal(box.CentreY, placement.Midpoint.Y, 9);
                Assert.Equal(box.CentreZ, placement.Midpoint.Z, 9);
                Assert.True(placement.Depth > 0.0);
            }
        }
    }
}
