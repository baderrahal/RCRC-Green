using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **A FORMULA THE RUN DID NOT AFFECT IS NOT AT RISK FROM THE RUN**, and the same formula
    /// filled down a column is one finding rather than one per row.
    ///
    /// Measured on the 18:15 report: 10,708 lines, of which WHAT THE WORKBOOK WILL COMPUTE FROM
    /// THIS was 5,180, under a heading reading (0) over a body of 526. Most of the 526 were G31
    /// to G36 saying one sentence per row per template about cells the run never wrote into, and
    /// each line said so itself.
    /// </summary>
    public class FormulaRepeatsTests : IDisposable
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

        private const string Main = "<Mosques>";

        private const string Proposed = "Tree List - Proposed";

        /// <summary>
        /// The 1428 fault in miniature: a species written into row 7 with no diameter, so L7
        /// returns a space and everything reading it carries the error.
        /// </summary>
        private FormulaCheck Checked()
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") });

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template, Path.Combine(_folder, "filled.xlsx"),
                new[]
                {
                    CellWrite.Number(Main, "H7", 62023),
                    CellWrite.Text(Proposed, "D7", "UNKNOWN"),
                    CellWrite.Number(Proposed, "B7", 2)
                },
                KpiTemplates.Mosques.Cells.Select(cell => new WorkbookCell(Main, cell.Cell)).ToArray());

            return outcome.Formulas;
        }

        /// <summary>
        /// **Only the formulas reading a cell on a row this run wrote into.** The client's own
        /// workbook doing what it did before this tool opened it is not this run's finding.
        /// </summary>
        [Fact]
        public void AFormulaTheRunDidNotAffectIsNotAtRiskFromTheRun()
        {
            FormulaCheck check = Checked();

            IReadOnlyList<FormulaAtRisk> mine = FormulaRepeats.FromWhatTheRunWrote(check.AtRisk);

            Assert.NotEmpty(check.AtRisk);
            Assert.All(mine, one => Assert.True(one.FromWrittenRow));
            Assert.DoesNotContain(mine, one => !one.FromWrittenRow);
        }

        /// <summary>
        /// **THE HEADING COUNT AND THE BODY COME OFF ONE LIST.** A heading of 0 above 526 lines
        /// is a section nobody can trust, and a real risk inside it would be invisible.
        /// </summary>
        [Fact]
        public void TheHeadingCountsExactlyWhatTheBodyPrints()
        {
            FormulaCheck check = Checked();
            IReadOnlyList<RepeatedFormula> shapes = FormulaRepeats.Of(check.AtRisk);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: PatchOutcome.RefusedAfterWriting("held", check)),
                new DateTime(2026, 9, 14, 18, 15, 0));

            Assert.Contains("== " + KpiCreateReport.FormulasHeading + " (" + shapes.Count + ") ==", report);
            Assert.Contains("  FORMULAS AT RISK, " + shapes.Count + ":", report);
        }

        /// <summary>
        /// **Where the same formula repeats across rows for the same reason, say it once with the
        /// rows listed.** Written out by hand: three cells down one column with no gap read as a
        /// span, and the same three with a gap read as a list, so nothing claims to cover a cell
        /// it does not.
        /// </summary>
        [Fact]
        public void OneShapeOnSeveralRowsIsSaidOnceWithItsRowsListed()
        {
            var run = new RepeatedFormula(
                "<Mosques>", "G31", "IFS(F31=\"YES\",1,TRUE,0)", "reads a cell in error",
                new[] { "G31", "G32", "G33" });

            Assert.True(run.Repeats);
            Assert.Equal("G31 to G33", run.Where);
            Assert.Equal(3, run.Cells.Count);

            var gap = new RepeatedFormula(
                "<Mosques>", "G31", "IFS(F31=\"YES\",1,TRUE,0)", "reads a cell in error",
                new[] { "G31", "G33" });

            Assert.Equal("G31, G33", gap.Where);

            var across = new RepeatedFormula(
                "<Mosques>", "G31", "IFS(F31=\"YES\",1,TRUE,0)", "reads a cell in error",
                new[] { "G31", "H31" });

            Assert.Equal("G31, H31", across.Where);

            var alone = new RepeatedFormula(
                "<Mosques>", "G31", "IFS(F31=\"YES\",1,TRUE,0)", "reads a cell in error",
                new[] { "G31" });

            Assert.False(alone.Repeats);
            Assert.Equal("G31", alone.Where);
        }

        /// <summary>
        /// And the grouping is on the shape rather than on the text, so the same formula filled
        /// down a column, whose cell references move with the row, is one entry.
        /// </summary>
        [Fact]
        public void TheSameFormulaFilledDownAColumnIsOneEntry()
        {
            FormulaCheck check = Checked();

            IReadOnlyList<FormulaAtRisk> mine = FormulaRepeats.FromWhatTheRunWrote(check.AtRisk);
            IReadOnlyList<RepeatedFormula> shapes = FormulaRepeats.Of(check.AtRisk);

            Assert.True(shapes.Count <= mine.Count);
            Assert.Equal(mine.Count, shapes.Sum(one => one.Cells.Count));
        }
    }
}
