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
                new[] { new PlotViewPresence("DM-11", KeyPlan(), 4211, "010QE") },
                null,
                null);

            SheetGridCell filled = CellFor(grid, "DM-11", KeyPlan());
            SheetGridCell empty = CellFor(grid, "DM-11", Layout());

            Assert.Equal(SheetCellState.Exists, filled.State);
            Assert.Equal(4211, filled.ViewId);
            Assert.Equal("010QE", filled.SheetNumber);
            Assert.True(filled.IsInTheModel);

            Assert.Equal(SheetCellState.Missing, empty.State);
            Assert.Equal(0, empty.ViewId);
            Assert.Equal(string.Empty, empty.SheetNumber);
            Assert.False(empty.IsInTheModel);
        }

        /// <summary>
        /// The fourth state. A view that is in the model and on no sheet is not the same job
        /// as a view that is missing, and it is the ordinary case: the first real model holds
        /// 2,430 views on no sheet against 953 that are placed.
        /// </summary>
        [Fact]
        public void AViewOnNoSheetReadsAsItsOwnStateRatherThanAsDone()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan(), Layout(), CrossSection() },
                new[]
                {
                    new PlotViewPresence("DM-11", KeyPlan(), 4211, "010QE"),
                    new PlotViewPresence("DM-11", Layout(), 4212, string.Empty),
                    new PlotViewPresence("DM-11", CrossSection(), 4213, "   ")
                },
                null,
                null);

            Assert.Equal(SheetCellState.Exists, CellFor(grid, "DM-11", KeyPlan()).State);

            SheetGridCell loose = CellFor(grid, "DM-11", Layout());
            Assert.Equal(SheetCellState.ExistsNoSheet, loose.State);
            Assert.Equal(4212, loose.ViewId);
            Assert.Equal(string.Empty, loose.SheetNumber);

            // A number of nothing but spaces is no number. It reads as on no sheet rather
            // than as a sheet whose number is blank.
            Assert.Equal(
                SheetCellState.ExistsNoSheet, CellFor(grid, "DM-11", CrossSection()).State);
        }

        /// <summary>
        /// Both existing states open their view and neither can be marked, which is one
        /// question with one answer rather than two states compared at every call site.
        /// </summary>
        [Fact]
        public void BothExistingStatesCountAsBeingInTheModel()
        {
            SheetGrid grid = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { KeyPlan(), Layout() },
                new[]
                {
                    new PlotViewPresence("DM-11", KeyPlan(), 4211, "010QE"),
                    new PlotViewPresence("DM-11", Layout(), 4212, null)
                },
                null,
                // A mark on a cell that holds a view is ignored, whichever existing state
                // that cell is in.
                new[]
                {
                    new PlotViewKey("DM-11", KeyPlan()),
                    new PlotViewKey("DM-11", Layout())
                });

            Assert.All(grid.Rows.Single().Cells, cell => Assert.True(cell.IsInTheModel));
            Assert.Empty(grid.Marked);
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
                new[] { new PlotViewPresence("DM-11", KeyPlan(), 4211, "010QE") },
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
                    new PlotViewPresence("DM-11", KeyPlan(), 4211, "010QE"),
                    new PlotViewPresence("DM-11", KeyPlan(), 9000, "010QF")
                },
                null,
                null);

            SheetGridCell only = Assert.Single(grid.Rows.Single().Cells);

            Assert.Equal(SheetCellState.Exists, only.State);
            Assert.Equal(4211, only.ViewId);

            // The first found wins whole. Taking the id from one duplicate and the number
            // from the other would be two records of one cell.
            Assert.Equal("010QE", only.SheetNumber);
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
