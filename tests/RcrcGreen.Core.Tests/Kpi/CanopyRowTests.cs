using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **NEVER WRITE A NEW SPECIES INTO A ROW WITH NO CANOPY FORMULA.** Bader's decision of 15
    /// September, off the 13:32 run: CONOCARPUS LANCIFOLIUS, 2 trees, went into
    /// `GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx`, Tree List - Proposed, D85 and B85. That row
    /// carries C85, M85 and O85 and no L85 canopy formula, so the canopy guard blanked FP-23's
    /// Total areas to be greened and its canopy percentage. A row the workbook cannot compute a
    /// canopy from is not a row this tool may use.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.** Nothing works one out with the rule
    /// the code uses.
    /// </summary>
    public class CanopyRowTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try
            {
                Directory.Delete(_folder, true);
            }
            catch (IOException)
            {
            }
        }

        /// <summary>
        /// The FUTURE PARKS shape as the run measured it: names down to row 84, the total
        /// reaching row 86, so rows 85 and 86 are both empty, and only 86 carries the canopy
        /// formula. Every named row carries one too, which is what a real template does.
        /// </summary>
        private static SpeciesList FutureParks(params int[] canopyRows)
        {
            var named = new List<SpeciesListRow>();
            for (int row = 4; row <= 84; row++) named.Add(new SpeciesListRow(row, "Species " + row));

            return SpeciesList.Holding(
                named, 4, 86, canopyRows, "I", "J", "B87",
                "GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx");
        }

        private static IReadOnlyList<SpeciesMatch> Matching(SpeciesList proposed)
        {
            return SpeciesMatching.Against(
                new[] { CreateFixture.Merged("CONOCARPUS LANCIFOLIUS", CreateFixture.Proposed, "FP-23", 2) },
                KpiTemplates.FutureParks,
                SpeciesList.Refused("this test writes to the proposed sheet only"),
                proposed);
        }

        /// <summary>
        /// **ROW 85 IS SKIPPED AND ROW 86 IS TAKEN.** The rule is the next USABLE empty row the
        /// total reaches, not the next empty one.
        /// </summary>
        [Fact]
        public void ANewSpeciesSkipsTheEmptyRowWithNoCanopyFormulaAndTakesTheNextOne()
        {
            SpeciesList proposed = FutureParks(86);

            Assert.Equal(new[] { 85, 86 }, proposed.EmptyRows);
            Assert.Equal(new[] { 86 }, proposed.UsableEmptyRows);

            SpeciesMatch match = Assert.Single(Matching(proposed));

            Assert.True(match.Added, match.Why);
            Assert.True(match.Placed, match.Why);

            // **THE ROW IS IN THE MESSAGE.** A failure reading `expected 86, got 85` is the
            // whole fault in two numbers, and this says which row and what is wrong with it.
            Assert.True(
                match.Row == 86,
                "CONOCARPUS LANCIFOLIUS landed on row " + match.Row + ". Row 85 carries no "
                + "canopy formula, so a count written there leaves the canopy and the green "
                + "cover short, which is what blanked FP-23's Total areas to be greened.");

            Assert.Equal(86, match.Row);
            Assert.Equal(KpiTemplates.ProposedTreesSheet, match.SheetName);
        }

        /// <summary>
        /// **AND WHERE NO EMPTY ROW IS USABLE THE SPECIES IS NOT WRITTEN AT ALL**, named with
        /// every row that was checked, because a refusal that does not say which rows it looked
        /// at cannot be argued with and the rows are what the team's fix has to reach.
        /// </summary>
        [Fact]
        public void WhereNoEmptyRowCarriesTheFormulaNothingIsWrittenAndEveryRowCheckedIsNamed()
        {
            SpeciesList proposed = FutureParks(new int[0]);

            Assert.Empty(proposed.UsableEmptyRows);

            SpeciesMatch match = Assert.Single(Matching(proposed));

            Assert.False(match.Placed);
            Assert.False(match.Added);
            Assert.Equal(0, match.Row);
            Assert.Equal(2, match.Species.Quantity);
            Assert.Equal(
                "in GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx, Tree List - Proposed the total "
                + "reaches 2 empty rows, 85, 86, and not one of them carries the canopy formula "
                + "IF(ISBLANK(J85),\" \",ROUND(PI()*(J85/2)^2,0)) for its own row, so a count "
                + "written on any of them would leave the canopy and the green cover short",
                match.Why);
        }

        /// <summary>
        /// **A SHEET WHOSE CANOPY COLUMN IS ONE SHARED FORMULA IS READ ON EVERY ROW OF IT.**
        /// Excel stores a column of one formula as the text on the master cell and an index on
        /// every cell under it, so a reader taking the text alone would see the canopy on row 4
        /// and on none of the rows below. The fixture's tree sheets run rows 4 to 9 with the
        /// canopy shared from L4, and all six rows are read.
        /// </summary>
        [Fact]
        public void TheCanopyRowsAreReadOffASharedFormulaColumnAndNotOffItsMasterAlone()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(5, string.Empty),
                    new WorkbookFixture.TreeRow(6, string.Empty),
                    new WorkbookFixture.TreeRow(7, string.Empty),
                    new WorkbookFixture.TreeRow(8, string.Empty),
                    new WorkbookFixture.TreeRow(9, string.Empty)
                },
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8"),
                    new WorkbookFixture.TreeRow(5, string.Empty),
                    new WorkbookFixture.TreeRow(6, string.Empty),
                    new WorkbookFixture.TreeRow(7, string.Empty),
                    new WorkbookFixture.TreeRow(8, string.Empty),
                    new WorkbookFixture.TreeRow(9, string.Empty)
                });

            SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

            Assert.True(proposed.WasRead, proposed.Refusal);
            Assert.True(proposed.CanopyRowsRead);
            Assert.Equal(new[] { 4, 5, 6, 7, 8, 9 }, proposed.RowsWithTheCanopyFormula);

            // Rows 5 to 9 name nothing and the total reaches all of them, so every one of them
            // is both empty and usable.
            Assert.Equal(new[] { 5, 6, 7, 8, 9 }, proposed.EmptyRows);
            Assert.Equal(new[] { 5, 6, 7, 8, 9 }, proposed.UsableEmptyRows);
            Assert.Equal("MOSQUES.xlsx", proposed.FileName);
        }

        /// <summary>
        /// **THE GUARD NAMES THE ONE CELL THAT WOULD HAVE CARRIED THE CANOPY.** Bader's
        /// decision of 15 September: the team reading the report does not know which client is
        /// meant, so every printed line names the file, the sheet, the row and the cell. Row 7
        /// here is the FUTURE PARKS row 85 shape, a row with no canopy formula on a sheet whose
        /// other rows carry one in column L.
        ///
        /// **THE CELL RIGHT OF IT IS NOT NAMED.** The tool holds no record of what the canopy
        /// area column's formula is, so naming M7 as empty would be a rule nobody measured. It
        /// is an open question in the log.
        /// </summary>
        [Fact]
        public void TheGuardNamesTheFileTheSheetTheRowAndTheCanopyCell()
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(7, "Bauhinia purpurea", "6", "5")
                },
                fileName: "PARKS.xlsx",
                withoutCanopy: new[] { 7 });

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled.xlsx"),
                new[] { CellWrite.Number(KpiTemplates.ProposedTreesSheet, "B7", 19) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            ArithmeticCheck check = WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(KpiTemplates.ProposedTreesSheet, 7, "BAUHINIA PURPUREA", 19, 5.0)
                }),
                new Dictionary<string, string> { { KpiTemplates.ProposedTreesSheet, "J" } },
                "GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx");

            Assert.False(check.Agrees);
            Assert.Contains(
                "GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx, Tree List - Proposed row 7",
                check.Why);
            Assert.Contains(
                "On Tree List - Proposed the canopy formula sits in column L, and L7 is empty.",
                check.Why);

            // **NO LINE ANYWHERE SAYS CLIENT.**
            Assert.DoesNotContain("client", check.Why, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// **A SHEET NAMING NO DIAMETER COLUMN IS NOT CHECKED AT ALL, AND THE FLAG SAYS SO.**
        /// The canopy formula is written around that column, so with none chosen there is no
        /// formula to look for, and calling every row unusable would refuse every write on a
        /// shape nobody has measured.
        /// </summary>
        [Fact]
        public void ASheetWithNoDiameterColumnLeavesTheCheckUndoneRatherThanRefusingEveryRow()
        {
            string path = WorkbookFixture.TreeLists(
                _folder,
                new WorkbookFixture.TreeSheetShape(
                    KpiTemplates.ExistingTreesSheet,
                    new Dictionary<int, string> { { 4, "Albizia lebbeck" } }, 4, 6, 7),
                new WorkbookFixture.TreeSheetShape(
                    KpiTemplates.ProposedTreesSheet,
                    new Dictionary<int, string> { { 4, "Phoenix dactylifera" } }, 4, 6, 7));

            SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

            Assert.True(proposed.WasRead, proposed.Refusal);
            Assert.Equal(string.Empty, proposed.DiameterColumn);
            Assert.False(proposed.CanopyRowsRead);
            Assert.Equal(new[] { 5, 6 }, proposed.EmptyRows);
            Assert.Equal(new[] { 5, 6 }, proposed.UsableEmptyRows);
        }
    }
}
