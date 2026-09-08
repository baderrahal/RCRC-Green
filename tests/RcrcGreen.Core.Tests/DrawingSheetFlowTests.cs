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
            Assert.Equal(new[] { "Site Plan", "Level 00" }, plots.Ignored.Select(entry => entry.Text));
            Assert.All(plots.Ignored, entry => Assert.Equal(IgnoredReason.NotAPlotName, entry.Reason));

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
            // Written out rather than worked out from the box, so a change to the rule that
            // picks the short side shows up here instead of being followed by the test.
            var expected = new[]
            {
                new
                {
                    Box = new PlotBox("DM-41", 0, 0, 0, 300, 80, 20),
                    Start = new Point3D(150, 0, 10),
                    End = new Point3D(150, 80, 10),
                    Direction = new Vector3D(1, 0, 0),
                    Length = 80.0
                },
                new
                {
                    Box = new PlotBox("PF-12", 400, 0, 0, 480, 350, 20),
                    Start = new Point3D(400, 175, 10),
                    End = new Point3D(480, 175, 10),
                    Direction = new Vector3D(0, -1, 0),
                    Length = 80.0
                },
                new
                {
                    Box = new PlotBox("AB-7", -100, -100, -5, -40, -30, 5),
                    Start = new Point3D(-100, -65, 0),
                    End = new Point3D(-40, -65, 0),
                    Direction = new Vector3D(0, -1, 0),
                    Length = 60.0
                }
            };

            const double depth = 32.808398950131235;

            foreach (var one in expected)
            {
                SectionPlacement placement = SectionPlacement.Across(one.Box, SectionAxis.ShortSide, depth);

                Assert.Equal(one.Box.PlotName, placement.PlotName);
                Assert.Equal(one.Length, placement.Length, 9);
                Assert.Equal(one.Start.X, placement.Start.X, 9);
                Assert.Equal(one.Start.Y, placement.Start.Y, 9);
                Assert.Equal(one.Start.Z, placement.Start.Z, 9);
                Assert.Equal(one.End.X, placement.End.X, 9);
                Assert.Equal(one.End.Y, placement.End.Y, 9);
                Assert.Equal(one.End.Z, placement.End.Z, 9);
                Assert.Equal(one.Direction.X, placement.ViewDirection.X, 9);
                Assert.Equal(one.Direction.Y, placement.ViewDirection.Y, 9);
                Assert.Equal(one.Direction.Z, placement.ViewDirection.Z, 9);
                Assert.Equal(depth, placement.Depth, 9);
            }
        }
    }
}
