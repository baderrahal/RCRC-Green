using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE CANOPY CHECK MUST ALSO READ THE TOTAL CANOPY COLUMN.** FP-18 has 2 Ziziphus
    /// spina-christi on Tree List - Existing row 83. L83 carries the canopy formula and M83 is
    /// empty in the EXISTING PARKS and FUTURE PARKS templates. The PDF says 2,631 m2 greened and
    /// 67.68 percent canopy, the recalculated Excel 2,531 and 63.97. Nothing warned, because the
    /// check and the usable empty rows both looked at the canopy formula alone.
    ///
    /// **THE COLUMN IS READ OFF THE FILE.** Measured by Bader on all seven templates on
    /// 16 September: column M is Total Mature Canopy Area, `=IF(ISBLANK(B85)," ",L85*B85)` on a
    /// complete row, Tree List - Existing M102 is `=SUM(M4:M101)`, Tree List - Proposed M93 is
    /// `=SUM(M4:M92)`, and the first tab's Canopy Area cell is
    /// `='Tree List - Existing'!M102+'Tree List - Proposed'!M93`. This answers the open question
    /// the ninetieth pass logged, that no record held what the canopy area column's formula is.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class TotalCanopyColumnTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        private static readonly string Existing = KpiTemplates.ExistingTreesSheet;

        private static readonly string Proposed = KpiTemplates.ProposedTreesSheet;

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
        /// The fixture's chain is the measured one one size down: the green cover cell D8 reads
        /// F8 plus the planting and the lawn, F8 reads M10 on each tree sheet, and M10 is
        /// SUM(M4:M9).
        /// </summary>
        private string Template(string fileName, int[] withoutTotalCanopy = null)
        {
            return WorkbookFixture.Computing(
                _folder,
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(7, "Ziziphus spina-christi", "6", "5")
                },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: fileName,
                withGreenCoverLabel: true,
                withoutTotalCanopy: withoutTotalCanopy);
        }

        private IReadOnlyList<TotalCanopyColumn> Columns(string path)
        {
            LabelledCells computed = LabelledPlaces.In(path, KpiTemplates.Mosques, ComputedPlaces.All);

            return TotalCanopyColumns.In(
                path, KpiTemplates.Mosques, computed.For(ComputedPlaces.GreenCoverName));
        }

        /// <summary>
        /// **THE COLUMN AND THE ROWS, OFF THE CHAIN AND OFF NO LETTER.** M, added by M10 over
        /// rows 4 to 9, on both tree list sheets.
        /// </summary>
        [Fact]
        public void TheColumnIsReadOffTheCanopyCellAndItsTwoTotals()
        {
            IReadOnlyList<TotalCanopyColumn> columns = Columns(Template("chain.xlsx"));

            TotalCanopyColumn existing = TotalCanopyColumns.For(columns, Existing);

            Assert.True(existing.Found, existing.Why);
            Assert.Equal("M", existing.Column);
            Assert.Equal("M10", existing.TotalCell);
            Assert.Equal(4, existing.FirstRow);
            Assert.Equal(9, existing.LastRow);
            Assert.Equal(
                "Tree List - Existing: the canopy total M10 adds column M over rows 4 to 9",
                existing.InWords);

            TotalCanopyColumn proposed = TotalCanopyColumns.For(columns, Proposed);

            Assert.True(proposed.Found, proposed.Why);
            Assert.Equal("M", proposed.Column);
        }

        /// <summary>
        /// **A ROW WITH THE CANOPY FORMULA AND NO TOTAL CANOPY FORMULA IS NOT USABLE.** Row 7
        /// here is FP-18's row 83: L7 carries `IF(ISBLANK(J7)," ",ROUND(PI()*(J7/2)^2,0))` and M7
        /// is empty.
        /// </summary>
        [Fact]
        public void ARowCarryingTheCanopyAndNotTheTotalIsNotUsable()
        {
            string path = Template("noM.xlsx", new[] { 7 });
            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            SpeciesList list = SpeciesList.In(
                path, KpiTemplates.Mosques.ExistingTrees, TotalCanopyColumns.For(columns, Existing));

            Assert.True(list.WasRead, list.Refusal);
            Assert.True(list.CanopyRowsRead);
            Assert.True(list.TotalCanopyRowsRead);

            // Rows 4 to 9 all carry the canopy formula. Row 7 is the one carrying no total.
            Assert.Equal(new[] { 4, 5, 6, 7, 8, 9 }, list.RowsWithTheCanopyFormula);
            Assert.Equal(new[] { 4, 5, 6, 8, 9 }, list.RowsWithTheTotalCanopyFormula);

            // Rows 5, 6, 8 and 9 name nothing, so they are the empty ones, and row 7 is named.
            Assert.Equal(new[] { 5, 6, 8, 9 }, list.EmptyRows);
            Assert.Equal(new[] { 5, 6, 8, 9 }, list.UsableEmptyRows);
        }

        /// <summary>
        /// **AND AN EMPTY ROW WITH NO TOTAL CANOPY FORMULA IS NOT OFFERED TO A NEW SPECIES.**
        /// Row 5 is empty and carries no M5, so the next usable row is 6.
        /// </summary>
        [Fact]
        public void AnEmptyRowWithNoTotalCanopyFormulaIsNotOffered()
        {
            string path = Template("noM5.xlsx", new[] { 5 });
            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            SpeciesList list = SpeciesList.In(
                path, KpiTemplates.Mosques.ExistingTrees, TotalCanopyColumns.For(columns, Existing));

            Assert.Equal(new[] { 5, 6, 8, 9 }, list.EmptyRows);
            Assert.Equal(new[] { 6, 8, 9 }, list.UsableEmptyRows);
        }

        /// <summary>
        /// **THE GUARD BLANKS THE GREEN COVER AND NAMES M7.** A row this run wrote a count into
        /// that computes a canopy per tree and adds none.
        /// </summary>
        [Fact]
        public void TheGuardNamesTheCellThatWouldHaveAddedTheCanopy()
        {
            string path = Template("guard.xlsx", new[] { 7 });
            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                path,
                Path.Combine(_folder, "filled-guard.xlsx"),
                new[] { CellWrite.Number(Existing, "B7", 2) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            ArithmeticCheck check = WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(Existing, 7, "ZIZIPHUS SPINA-CHRISTI", 2, 5.0, true)
                }),
                new Dictionary<string, string> { { Existing, "J" } },
                "GRP_-_KPI_Checklist_-_DD_EXISTING PARKS.xlsx",
                columns);

            Assert.False(check.Agrees);
            Assert.Contains("M7 does not carry IF(ISBLANK(B7),\" \",L7*B7)", check.Why);
            Assert.Contains("which is the column the canopy total M10 adds", check.Why);
            Assert.Contains("M7 is empty.", check.Why);

            // **THE REASON OPENS WITH THE M7 FACT.** It opened with the canopy column sentence,
            // which is the column L wording, on a row whose canopy column is exactly the one
            // this tool works out. The first thing a person read about M7 was about a column
            // that had not moved.
            Assert.StartsWith(
                "a row computes a canopy per tree and adds none to the canopy total. ",
                check.Why);

            Assert.DoesNotContain(
                "the workbook's canopy column is not the one this tool works out",
                check.Why);
        }

        /// <summary>
        /// **THE SAME ROW WITH ITS M FORMULA PASSES.** The canopy reaches the total, so nothing
        /// is blanked and the line reads as a row that agrees.
        /// </summary>
        [Fact]
        public void TheSameRowCarryingItsTotalCanopyFormulaAgrees()
        {
            string path = Template("agrees.xlsx");
            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                path,
                Path.Combine(_folder, "filled-agrees.xlsx"),
                new[] { CellWrite.Number(Existing, "B7", 2) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            ArithmeticCheck check = WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(Existing, 7, "ZIZIPHUS SPINA-CHRISTI", 2, 5.0, true)
                }),
                new Dictionary<string, string> { { Existing, "J" } },
                "GRP_-_KPI_Checklist_-_DD_EXISTING PARKS.xlsx",
                columns);

            Assert.True(check.Agrees, check.Why);
            Assert.Equal(
                "Tree List - Existing L7 = IF(ISBLANK(J7),\" \",ROUND(PI()*(J7/2)^2,0)), "
                + "a row the template's own list already held",
                Assert.Single(check.Read));
        }

        /// <summary>
        /// **FP-18 AT ITS OWN ROW 83, which is the measurement.** 2 Ziziphus spina-christi on
        /// Tree List - Existing row 83, L83 carrying the canopy formula and M83 empty, and the
        /// green cover left blank naming M83. The PDF read 2,631 m2 greened where the
        /// recalculated Excel reads 2,531, and the canopy percentage 67.68 against 63.97.
        /// </summary>
        [Fact]
        public void TheFp18RowLeavesTheGreenCoverBlankAndNamesM83()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(83, "Ziziphus spina-christi", "6", "5")
                },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "fp18.xlsx",
                withGreenCoverLabel: true,
                withoutTotalCanopy: new[] { 83 },
                lastRow: 83);

            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            Assert.Equal("M84", TotalCanopyColumns.For(columns, Existing).TotalCell);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                path,
                Path.Combine(_folder, "filled-fp18.xlsx"),
                new[] { CellWrite.Number(Existing, "B83", 2) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            ArithmeticCheck check = WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(Existing, 83, "ZIZIPHUS SPINA-CHRISTI", 2, 5.0, true)
                }),
                new Dictionary<string, string> { { Existing, "J" } },
                "GRP_-_KPI_Checklist_-_DD_EXISTING PARKS.xlsx",
                columns);

            // **M83 IS IN THE MESSAGE.** A failure that says only the check disagreed leaves
            // somebody opening the template to find out which cell is missing, which is the whole
            // question FP-18 asked.
            Assert.False(
                check.Agrees,
                "FP-18's Tree List - Existing row 83 carries L83 and no M83, so its 2 Ziziphus "
                + "spina-christi compute a canopy per tree that the canopy total M84 never adds. "
                + "The check agreed anyway, which is what sent 2,631 m2 to the client against the "
                + "workbook's own 2,531.");

            Assert.Contains(
                "GRP_-_KPI_Checklist_-_DD_EXISTING PARKS.xlsx, Tree List - Existing row 83",
                check.Why);
            Assert.Contains("M83 does not carry IF(ISBLANK(B83),\" \",L83*B83)", check.Why);
            Assert.Contains("which is the column the canopy total M84 adds", check.Why);
            Assert.Contains("M83 is empty.", check.Why);
        }

        /// <summary>
        /// **A COLUMN THAT COULD NOT BE READ IS NOT A CHECK THAT PASSED.** The canopy cell holds
        /// a typed number and no formula, so nothing says which cells its total comes off and no
        /// tree list sheet's column can be read. The green cover and the canopy percentage used
        /// to be written anyway, with every empty row still offered to a new species and the
        /// chain's own reason printed nowhere.
        ///
        /// Today's seven templates read fine. The team is editing them, which is exactly when a
        /// guard that switches itself off costs something.
        /// </summary>
        [Fact]
        public void ATemplateWhoseCanopyCellIsTypedBlanksTheGreenCoverAndNamesTheReason()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "typedcanopy.xlsx",
                canopyCellIsTyped: true,
                withGreenCoverLabel: true);

            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            TotalCanopyColumn existing = TotalCanopyColumns.For(columns, Existing);

            Assert.False(existing.Found);
            Assert.False(existing.TheTemplateNamesNoGreenCover);
            Assert.Equal(
                "the canopy cell holds no formula, so nothing says which cells its total comes off",
                existing.Why);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                path,
                Path.Combine(_folder, "filled-typedcanopy.xlsx"),
                new[] { CellWrite.Number(Existing, "B4", 2) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            ArithmeticCheck check = WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(Existing, 4, "ALBIZIA LEBBECK", 2, 8.0, true)
                }),
                new Dictionary<string, string> { { Existing, "J" } },
                "GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx",
                columns);

            // **THE TEMPLATE AND THE REASON ARE IN THE MESSAGE.** A failure saying only that the
            // check agreed leaves somebody opening seven workbooks to find out which one stopped
            // reading and where.
            Assert.False(
                check.Agrees,
                "GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx has a typed number in its canopy cell, so "
                + "no tree list sheet's total canopy column could be read, and the check agreed "
                + "anyway. That is a guard switching itself off, which reads exactly like a "
                + "guard that passed. What it said: "
                + (check.Why.Length == 0 ? "nothing at all" : check.Why));

            Assert.Contains(
                "GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx, Tree List - Existing row 4",
                check.Why);
            Assert.Contains(
                "the total canopy column on Tree List - Existing could not be read, so nothing "
                + "says whether this row's canopy reaches a total: the canopy cell holds no "
                + "formula, so nothing says which cells its total comes off",
                check.Why);

            // **AND NO EMPTY ROW OF THAT SHEET IS OFFERED TO A NEW SPECIES.** Nothing says
            // whether one carries the formula the canopy total adds, and a count written into
            // one would put a number where the workbook computes no canopy.
            SpeciesList list = SpeciesList.In(
                path, KpiTemplates.Mosques.ExistingTrees, existing);

            Assert.NotEmpty(list.EmptyRows);
            Assert.Empty(list.UsableEmptyRows);
            Assert.Equal(
                "the canopy cell holds no formula, so nothing says which cells its total comes off",
                list.TotalCanopyUnreadable);

            // The list names the file it was really read off, which is the fixture's own name
            // here rather than the client's, because nothing hands it a second one.
            Assert.Equal(
                "in typedcanopy.xlsx, Tree List - Existing the total canopy column could not be "
                + "read, so nothing says whether an empty row carries the formula the canopy "
                + "total adds and none of them is offered: the canopy cell holds no formula, so "
                + "nothing says which cells its total comes off",
                list.NoUsableEmptyRow(Existing, "J"));
        }

        /// <summary>
        /// **A TEMPLATE NAMING NO GREEN COVER CELL IS NOT CHECKED AND IS NOT REFUSED.** It holds
        /// no canopy total at all, so a row of it reaches none by construction and there is
        /// nothing to check. Bader's decision of 16 September keeps this one case exactly as it
        /// was while every other unreadable column now blanks the green cover.
        ///
        /// The reason moved from the general `NoCanopyCell` to `NoGreenCoverCell`, which is its
        /// own constant and its own flag, because a check that could not be made and a check
        /// there is nothing to make are two different facts and one of them holds rows back.
        /// </summary>
        [Fact]
        public void ATemplateWithNoGreenCoverLabelNamesNoColumnAndRefusesNothing()
        {
            string path = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "nolabel.xlsx");

            IReadOnlyList<TotalCanopyColumn> columns = Columns(path);

            TotalCanopyColumn existing = TotalCanopyColumns.For(columns, Existing);

            Assert.False(existing.Found);
            Assert.Equal(TotalCanopyColumns.NoGreenCoverCell, existing.Why);
            Assert.True(existing.TheTemplateNamesNoGreenCover);

            Assert.Equal(
                "the template names no Total Green cover cell, so it holds no canopy total for "
                + "a row's canopy to reach and there is nothing here to check",
                existing.Why);

            // **AND THE ROWS ARE NOT NARROWED BY A CHECK THERE IS NOTHING TO MAKE.** This is the
            // one refusal that holds no row back, and the flag rather than the words is what
            // decides it.
            SpeciesList list = SpeciesList.In(
                path, KpiTemplates.Mosques.ExistingTrees, existing);

            Assert.False(list.TotalCanopyRowsRead);
            Assert.Equal(string.Empty, list.TotalCanopyUnreadable);
            Assert.Equal(list.EmptyRows, list.UsableEmptyRows);
        }
    }
}
