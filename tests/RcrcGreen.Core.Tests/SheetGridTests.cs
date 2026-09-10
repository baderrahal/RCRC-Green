using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class SheetGridTests
    {
        private static ViewType KeyPlan()
        {
            return new ViewType("010", "Location Key Plan");
        }

        private static ViewType Layout()
        {
            return new ViewType("200", "General Arrangement Layout");
        }

        private static ViewType CrossSection()
        {
            return new ViewType("400", "Landscape Cross Section");
        }

        private static SheetGridCell CellFor(SheetGrid grid, string plotId, ViewType column)
        {
            return grid.Rows
                .Single(row => row.PlotId == plotId)
                .Cells
                .Single(cell => cell.ViewType.Equals(column));
        }

        [Fact]
        public void OnlyThePlotsInRangeGetARow()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11", "DM-28" },
                new[] { KeyPlan() },
                new[] { new PlotViewPresence("DM-41", KeyPlan(), 900) },
                null,
                null);

            Assert.Equal(new[] { "DM-11", "DM-28" }, grid.Rows.Select(row => row.PlotId));
        }

        [Fact]
        public void RowsComeOutWithTheNumberPartInNumberOrder()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-100", "DM-2", "DM-9" },
                new[] { KeyPlan() },
                null,
                null,
                null);

            Assert.Equal(new[] { "DM-2", "DM-9", "DM-100" }, grid.Rows.Select(row => row.PlotId));
        }

        [Fact]
        public void AColumnIsAddedOnlyBecauseTheCallerAskedForIt()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan() },
                new[] { new PlotViewPresence("DM-11", CrossSection(), 900) },
                null,
                null);

            Assert.Equal(new[] { KeyPlan() }, grid.Columns);
            Assert.Single(grid.Rows.Single().Cells);
        }

        [Fact]
        public void ACellHoldingAViewSaysSoAndCarriesTheIdentifierToSelectIt()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan(), Layout() },
                new[] { new PlotViewPresence("DM-11", KeyPlan(), 4211) },
                null,
                null);

            SheetGridCell filled = CellFor(grid, "DM-11", KeyPlan());
            SheetGridCell empty = CellFor(grid, "DM-11", Layout());

            Assert.Equal(SheetCellState.Exists, filled.State);
            Assert.Equal(4211, filled.ViewId);

            Assert.Equal(SheetCellState.Missing, empty.State);
            Assert.Equal(0, empty.ViewId);
        }

        [Fact]
        public void AMarkedCellReadsAsMarkedRatherThanMissing()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan(), Layout() },
                null,
                null,
                new[] { new PlotViewKey("DM-11", Layout()) });

            Assert.Equal(SheetCellState.Missing, CellFor(grid, "DM-11", KeyPlan()).State);
            Assert.Equal(SheetCellState.Marked, CellFor(grid, "DM-11", Layout()).State);
            Assert.Equal(1, grid.MarkedCount);
        }

        /// <summary>
        /// The marks the run is handed are the marked squares on screen, in row order. A mark
        /// on a column not shown, on a plot out of range or on a square holding a view is not
        /// among them, and the count is the length of that same list.
        /// </summary>
        [Fact]
        public void MarkedListsTheMarkedSquaresAndNothingOffTheGrid()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-28", "DM-11" },
                new[] { KeyPlan(), Layout() },
                new[] { new PlotViewPresence("DM-28", Layout(), 4211) },
                null,
                new[]
                {
                    new PlotViewKey("DM-28", KeyPlan()),
                    new PlotViewKey("DM-11", Layout()),
                    new PlotViewKey("DM-11", CrossSection()),
                    new PlotViewKey("DM-28", Layout()),
                    new PlotViewKey("DM-41", KeyPlan())
                });

            Assert.Equal(
                new[] { new PlotViewKey("DM-11", Layout()), new PlotViewKey("DM-28", KeyPlan()) },
                grid.Marked);
            Assert.Equal(2, grid.MarkedCount);
        }

        [Fact]
        public void AViewThatExistsCannotBeMarked()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan() },
                new[] { new PlotViewPresence("DM-11", KeyPlan(), 4211) },
                null,
                new[] { new PlotViewKey("DM-11", KeyPlan()) });

            Assert.Equal(SheetCellState.Exists, CellFor(grid, "DM-11", KeyPlan()).State);
            Assert.Equal(0, grid.MarkedCount);
        }

        [Fact]
        public void AMarkOnAPlotOutsideTheRangeChangesNothingInside()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan() },
                null,
                null,
                new[] { new PlotViewKey("DM-28", KeyPlan()) });

            Assert.Equal(SheetCellState.Missing, CellFor(grid, "DM-11", KeyPlan()).State);
            Assert.Equal(0, grid.MarkedCount);
        }

        [Fact]
        public void TheRowSaysWhenNoScopeBoxCarriesThatPlotName()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11", "DM-28" },
                new[] { KeyPlan() },
                null,
                new[] { "DM-11" },
                null);

            Assert.True(grid.Rows.Single(row => row.PlotId == "DM-11").HasScopeBox);
            Assert.False(grid.Rows.Single(row => row.PlotId == "DM-28").HasScopeBox);
        }

        [Fact]
        public void ColumnsComeOutWithTheCodeReadAsANumber()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { new ViewType("1000", "Long Section"), Layout(), new ViewType("90", "Setting Out") },
                null,
                null,
                null);

            Assert.Equal(
                new[] { "90", "200", "1000" },
                grid.Columns.Select(column => column.Code));
        }

        [Fact]
        public void TwoViewsOfOneTypeOnOnePlotStillGiveOneCell()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan() },
                new[]
                {
                    new PlotViewPresence("DM-11", KeyPlan(), 4211),
                    new PlotViewPresence("DM-11", KeyPlan(), 9000)
                },
                null,
                null);

            SheetGridCell only = Assert.Single(grid.Rows.Single().Cells);

            Assert.Equal(SheetCellState.Exists, only.State);
            Assert.Equal(4211, only.ViewId);
        }

        [Fact]
        public void ADuplicatePlotInTheRangeGetsOneRow()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11", "DM-11" },
                new[] { KeyPlan() },
                null,
                null,
                null);

            Assert.Single(grid.Rows);
        }

        [Fact]
        public void NoRangeAndNoColumnsGiveAnEmptyGridRatherThanAThrow()
        {
            SheetGrid grid = SheetGrid.Build(null, null, null, null, null);

            Assert.Empty(grid.Rows);
            Assert.Empty(grid.Columns);
            Assert.Equal(0, grid.MarkedCount);
        }

        [Fact]
        public void ARangeWithNoColumnsChosenStillShowsEveryRow()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11", "DM-28" },
                null,
                null,
                null,
                null);

            Assert.Equal(2, grid.Rows.Count);
            Assert.All(grid.Rows, row => Assert.Empty(row.Cells));
        }
    }
}
