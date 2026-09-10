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

        /// <summary>
        /// The live seam: the registry's union makes the plot list, the grid draws a row for
        /// every plot in it, and the plot found through no view at all is the row with
        /// nothing in it. That plot was silently dropped when the list came from views alone.
        /// </summary>
        [Fact]
        public void ThePlotWithOnlyAScopeBoxAndTaggedElementsIsTheOneWithEverythingMissing()
        {
            PlotRegistryResult plots = PlotRegistry.Build(
                ViewNamesInModel, null, ScopeBoxNames, ElementPlotIds);

            Assert.Equal(new[] { "AB-7", "DM-41", "PF-12" }, plots.Plots.Select(plot => plot.PlotId));
            Assert.Equal(new[] { "Site Plan", "Level 00" }, plots.Ignored.Select(entry => entry.Text));
            Assert.All(plots.Ignored, entry => Assert.Equal(IgnoredReason.NotAPlotName, entry.Reason));

            PlotRecord ab7 = plots.Plots.Single(plot => plot.PlotId == "AB-7");
            Assert.Equal(new[] { PlotSource.ScopeBox, PlotSource.ElementParameter }, ab7.Sources);
            Assert.False(ab7.HasViews);
            Assert.Equal("box and elements, no views", ab7.NoViewsInWords());
            Assert.Equal("a scope box, PRX_Plot_ID on elements", ab7.SourcesInWords());

            Assert.Equal(string.Empty, plots.Plots.Single(plot => plot.PlotId == "DM-41").NoViewsInWords());

            var columns = new[]
            {
                new ViewType("010", "Location Key Plan"),
                new ViewType("010", "Overall Key Plan"),
                new ViewType("200", "General Arrangement Layout"),
                new ViewType("400", "Landscape Cross Section")
            };

            // Filled the way the reader fills cells, one presence per parseable view name.
            var present = new[]
            {
                new PlotViewPresence("DM-41", columns[0], 1),
                new PlotViewPresence("DM-41", columns[1], 2),
                new PlotViewPresence("DM-41", columns[2], 3),
                new PlotViewPresence("DM-41", columns[3], 4),
                new PlotViewPresence("PF-12", columns[2], 5)
            };

            SheetGrid grid = SheetGrid.Build(
                plots.Plots.Select(plot => plot.PlotId),
                columns,
                present,
                ScopeBoxNames,
                null);

            SheetGridRow ab7Row = grid.Rows.Single(row => row.PlotId == "AB-7");
            Assert.All(ab7Row.Cells, cell => Assert.Equal(SheetCellState.Missing, cell.State));

            SheetGridRow dm41Row = grid.Rows.Single(row => row.PlotId == "DM-41");
            Assert.All(dm41Row.Cells, cell => Assert.Equal(SheetCellState.Exists, cell.State));

            // PF-12 holds the general arrangement layout and nothing else, so it lacks the
            // two key plans and the cross section.
            SheetGridRow pf12Row = grid.Rows.Single(row => row.PlotId == "PF-12");
            Assert.Equal(
                new[]
                {
                    SheetCellState.Missing,
                    SheetCellState.Missing,
                    SheetCellState.Exists,
                    SheetCellState.Missing
                },
                pf12Row.Cells.Select(cell => cell.State));
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
