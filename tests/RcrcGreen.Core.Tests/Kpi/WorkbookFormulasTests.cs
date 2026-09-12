using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The 1428 workbook passed every cache check and carried nine #VALUE! cells that survive a
    /// full recalculation, from three rows written with a name and a count and no diameter.
    /// The check that would have caught it reads the output's formulas for what they read and
    /// evaluates nothing. The workbook it is proven on is built here in the same shape: the
    /// canopy column reads IF(ISBLANK(J), " ", ...), the area column multiplies that by the
    /// count, a total sums the areas, and the main sheet computes from that total, from the
    /// mapped cells and from a defined name.
    ///
    /// The same file opened into a manual Excel session showing every formula cell blank,
    /// because calcPr carried no calcMode. It is set to auto and checked as the fifth thing.
    /// Nothing here runs Excel: the check is over what the file says.
    /// </summary>
    public class WorkbookFormulasTests : IDisposable
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

        private string Template(bool withSheetCalcPr = false)
        {
            return WorkbookFixture.Computing(_folder,
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "25", "8"),
                    new WorkbookFixture.TreeRow(5, "Albizia lebbeck", "15", "8")
                },
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(5, "Cassia glauca", "6", "5")
                },
                withSheetCalcPr: withSheetCalcPr);
        }

        private string Output()
        {
            return Path.Combine(_folder, "filled.xlsx");
        }

        private static WorkbookCell[] MappedCells()
        {
            return KpiTemplates.Mosques.Cells
                .Select(cell => new WorkbookCell(KpiTemplates.Mosques.MainSheetName, cell.Cell))
                .ToArray();
        }

        private const string Main = "<Mosques>";

        private const string Proposed = "Tree List - Proposed";

        /// <summary>
        /// The 1428 fault, in miniature: a species written into row 7 of the proposed list with
        /// its name and its count and no diameter. L7 returns a space, M7 is #VALUE!, and the
        /// error runs through M10, F8, D8, D9, E31 and the KPI row. The output is deleted again.
        /// </summary>
        [Fact]
        public void ARowWrittenWithoutADiameterBreaksTheCanopyMathsAndIsRefused()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023),
                CellWrite.Number(Main, "F10", 3258),
                CellWrite.Number(Main, "H10", 1127),
                CellWrite.Text(Proposed, "D7", "UNKNOWN"),
                CellWrite.Number(Proposed, "B7", 2)
            }, MappedCells());

            Assert.False(outcome.Written);
            Assert.False(File.Exists(Output()), "the output stays deleted");
            Assert.StartsWith("The output was written, checked and deleted again. 8 formulas would read an error off a row this run wrote into: "
                + "Tree List - Proposed M7", outcome.Refusal, StringComparison.Ordinal);
            Assert.Contains("Tree List - Proposed L7: J7 is blank, so ISBLANK is true and the formula returns the text \" \", "
                + "and a formula beside it does arithmetic on that.", outcome.Refusal);
            Assert.EndsWith("A cell written where no formula can compute from it is a refusal and not a note.", outcome.Refusal, StringComparison.Ordinal);

            FormulaCheck check = outcome.Formulas;
            Assert.True(check.WasChecked);
            Assert.True(check.RefusesTheWrite);

            FormulaAtRisk space = check.AtRisk.Single(one => one.SheetName == Proposed && one.Cell == "L7");
            Assert.Equal(1, space.Level);
            Assert.True(space.FromWrittenRow);
            Assert.Equal("IF(ISBLANK(J7),\" \",ROUND(PI()*(J7/2)^2,0))", space.Text);

            FormulaAtRisk value = check.AtRisk.Single(one => one.SheetName == Proposed && one.Cell == "M7");
            Assert.Equal(2, value.Level);
            Assert.Equal("#VALUE!: L7 returns text and this formula does arithmetic on it", value.Reason);

            Assert.Equal("carries the error from M7", check.AtRisk.Single(one => one.SheetName == Proposed && one.Cell == "M10").Reason);
            Assert.Equal("carries the error from Tree List - Proposed!M10", check.AtRisk.Single(one => one.SheetName == Main && one.Cell == "F8").Reason);
            Assert.Equal(
                new[] { "D8", "D9", "E31", "F31", "G31" },
                check.AtRisk.Where(one => one.SheetName == Main && one.Cell != "F8").Select(one => one.Cell).OrderBy(one => one, StringComparer.Ordinal));
            Assert.All(check.AtRisk.Where(one => one.IsAnError), one => Assert.True(one.FromWrittenRow));
        }

        /// <summary>
        /// The template's own empty rows return a space from the same formula and that is not
        /// an error: the cell beside them guards the blank count. They are named at level 1 and
        /// not from a written row, and nothing refuses.
        /// </summary>
        [Fact]
        public void ARowWrittenWithItsDiameterComputesAndTheEmptyRowsAreNotAnError()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023),
                CellWrite.Text(Proposed, "D7", "BAUHINIA PURPUREA"),
                CellWrite.Number(Proposed, "B7", 19),
                CellWrite.Number(Proposed, "I7", 6),
                CellWrite.Number(Proposed, "J7", 5)
            }, MappedCells());

            Assert.True(outcome.Written, outcome.Refusal);
            Assert.True(File.Exists(Output()));

            FormulaCheck check = outcome.Formulas;
            Assert.False(check.RefusesTheWrite);
            Assert.DoesNotContain(check.AtRisk, one => one.IsAnError);

            // Rows 8 and 9 of both sheets, and rows 4 to 6 where no count was written: their
            // L and M return a space and stay a space.
            Assert.All(check.AtRisk, one => Assert.Equal(1, one.Level));
            Assert.All(check.AtRisk, one => Assert.False(one.FromWrittenRow));
            Assert.DoesNotContain(check.AtRisk, one => one.SheetName == Proposed && one.Cell == "L7");
            Assert.DoesNotContain(check.AtRisk, one => one.SheetName == Proposed && one.Cell == "M7");
        }

        /// <summary>
        /// Every formula whose text reads a cell on a row this run wrote into, through which
        /// reference, a shared formula's text shifted to its own row.
        /// </summary>
        [Fact]
        public void EveryFormulaReadingAWrittenRowIsNamedWithTheReference()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023),
                CellWrite.Text(Proposed, "D7", "BAUHINIA PURPUREA"),
                CellWrite.Number(Proposed, "B7", 19),
                CellWrite.Number(Proposed, "J7", 5)
            }, MappedCells());

            FormulaCheck check = outcome.Formulas;

            FormulaCell canopy = check.ReadingWrittenRows.Single(one => one.SheetName == Proposed && one.Cell == "L7");
            Assert.Equal("IF(ISBLANK(J7),\" \",ROUND(PI()*(J7/2)^2,0))", canopy.Text);
            Assert.Equal("L4", canopy.SharedWith);
            Assert.Equal(new[] { "row 7 of Tree List - Proposed through J7", "row 7 of Tree List - Proposed through J7" }, canopy.ReadsWritten);

            FormulaCell total = check.ReadingWrittenRows.Single(one => one.SheetName == Proposed && one.Cell == "B10");
            Assert.Equal("SUM(B4:B9)", total.Text);
            Assert.Equal(new[] { "row 7 of Tree List - Proposed through B4:B9" }, total.ReadsWritten);

            FormulaCell divide = check.ReadingWrittenRows.Single(one => one.SheetName == Main && one.Cell == "D9");
            Assert.Equal(new[] { "row 7 of <Mosques> through H7" }, divide.ReadsWritten);

            // H9 reads the area through the defined name and is named for it.
            FormulaCell named = check.ReadingWrittenRows.Single(one => one.SheetName == Main && one.Cell == "H9");
            Assert.Equal("H8/Area", named.Text);
            Assert.Equal(new[] { "row 7 of <Mosques> through H7" }, named.ReadsWritten);
        }

        /// <summary>
        /// The six cells the map names, present or not, and the formulas reading each with the
        /// blanks among their inputs.
        /// </summary>
        [Fact]
        public void TheCellsTheWorkbookComputesFromAreEachSaidPresentOrNot()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Text(Main, "D3", "FRIDAY MOSQUE"),
                CellWrite.Number(Main, "H7", 62023),
                CellWrite.Number(Main, "F10", 3258)
            }, MappedCells());

            FormulaCheck check = outcome.Formulas;
            Assert.Equal(6, check.ComputesFrom.Count);

            ComputedFrom component = check.ComputesFrom.Single(one => one.Input.Cell.ToString() == "D3");
            Assert.True(component.Present);
            Assert.Equal("FRIDAY MOSQUE", component.Holds);
            Assert.Empty(component.Readers);

            ComputedFrom reference = check.ComputesFrom.Single(one => one.Input.Cell.ToString() == "C5");
            Assert.False(reference.Present);

            ComputedFrom area = check.ComputesFrom.Single(one => one.Input.Cell.ToString() == "H7");
            Assert.True(area.Present);
            Assert.Equal("62023", area.Holds);
            // F31 reads the area through the defined name, so it counts as a reader too.
            Assert.Equal(3, area.Readers.Count);
            Assert.Equal("<Mosques> D9 = D8/H7", area.Readers[0]);
            Assert.Equal("<Mosques> H9 = H8/Area", area.Readers[1]);
            Assert.StartsWith("<Mosques> F31 = _xlfn.IFS(Area<1", area.Readers[2], StringComparison.Ordinal);
            Assert.Equal(new[] { "<Mosques> H9 reads H8, which is blank" }, area.BlankInputs);

            ComputedFrom lawn = check.ComputesFrom.Single(one => one.Input.Cell.ToString() == "H10");
            Assert.True(lawn.Present, "the template holds a 0 there");
            Assert.Equal(new[] { "<Mosques> D8 = F8+F10+H10" }, lawn.Readers);
        }

        /// <summary>
        /// Two IFS formulas stored as _xlfn.IFS, counted by function and by cell, and named as
        /// needing a version of Excel that has them. Which version is not worked out.
        /// </summary>
        [Fact]
        public void TheFunctionsOlderExcelDoesNotHaveAreCountedByCell()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023)
            }, MappedCells());

            FunctionUse ifs = Assert.Single(outcome.Formulas.FunctionsExcelMayNotHave);
            Assert.Equal("IFS", ifs.Name);
            Assert.Equal(2, ifs.Cells);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: outcome, outputPath: Output()), new DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("  FUNCTIONS THE READER'S EXCEL MAY NOT HAVE, 1.", report);
            Assert.Contains("    _xlfn.IFS in 2 cells. Those cells need a version of Excel that has IFS.", report);
        }

        [Fact]
        public void TheFormulaCountAndTheReadersOfWrittenRowsAreInTheReport()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023),
                CellWrite.Text(Proposed, "D7", "UNKNOWN"),
                CellWrite.Number(Proposed, "B7", 2)
            }, MappedCells());

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: outcome, outputPath: Output()), new DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("== " + KpiCreateReport.FormulasHeading + " (8) ==", report);
            // Seven on the main sheet, D8, F8, D9, H9, E31, F31 and G31, and fourteen on each tree
            // sheet, L4 to L9, M4 to M9, B10 and M10.
            Assert.Contains("  formulas in the output   35, of which ", report);
            Assert.Contains("  THIS RUN IS REFUSED ON WHAT FOLLOWS. The output was written, checked and deleted again.", report);
            Assert.Contains("  Tree List - Proposed | M7 | IF(ISBLANK(B7),\" \",L7*B7) | #VALUE!: L7 returns text and this formula does arithmetic on it", report);
            Assert.Contains("  <Mosques> H7 | present, holds 62023 | read by 3 formulas", report);
            Assert.Contains("== CELLS WRITTEN (0) ==\r\nNOTHING WAS WRITTEN\r\n  The output was written, checked and deleted again.", report);
        }

        /// <summary>
        /// The fifth check. calcMode is written as auto, read back, and required.
        /// </summary>
        [Fact]
        public void CalcModeIsSetToAutoAndIsTheFifthThingChecked()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023)
            }, MappedCells());

            Assert.Contains("calcMode=\"auto\"", WorkbookFixture.PartText(Output(), "xl/workbook.xml"));
            Assert.Equal("auto", outcome.Cache.CalcMode);
            Assert.True(outcome.Cache.CalcModeAuto);
            Assert.True(outcome.Cache.WillRecalculate);

            var withoutIt = new CacheCheck(true, "0", 0, true, 12);
            Assert.False(withoutIt.CalcModeAuto);
            Assert.False(withoutIt.WillRecalculate);

            var manual = new CacheCheck(true, "0", 0, true, 12, "manual");
            Assert.False(manual.WillRecalculate);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: outcome, outputPath: Output()), new DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("  WILL EXCEL RECALCULATE THIS FILE: YES, five things checked off the output file, over what the file says and not over what Excel does with it", report);
            Assert.Contains("    calcMode              auto\r\n", report);
            Assert.Contains("    other calculation settings in the package: none found\r\n      looked for: " + CacheCheck.LookedFor, report);
        }

        /// <summary>
        /// A sheet's own sheetCalcPr is a place the workbook part does not cover. Found, it is
        /// named. Not found, the report says what was looked for.
        /// </summary>
        [Fact]
        public void ASheetCalcPrInThePackageIsFoundAndNamed()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(withSheetCalcPr: true), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 62023)
            }, MappedCells());

            string found = Assert.Single(outcome.Cache.OtherCalculationSettings);
            Assert.Equal("xl/worksheets/sheet1.xml carries sheetCalcPr fullCalcOnLoad=\"1\"", found);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: outcome, outputPath: Output()), new DateTime(2026, 9, 10, 14, 28, 0));

            Assert.Contains("    other calculation settings in the package: 1\r\n      xl/worksheets/sheet1.xml carries sheetCalcPr fullCalcOnLoad=\"1\"\r\n", report);
        }
    }
}
