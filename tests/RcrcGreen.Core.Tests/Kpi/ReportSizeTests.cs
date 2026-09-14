using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **A REPORT NOBODY CAN OPEN IS NOT A RECORD.** The 18:15 run wrote 10,708 lines over seven
    /// plot blocks and the 19:52 run wrote 82,048 over 156, of which 42,570 were one section.
    /// These cover the two answers to that: one line per formula shape rather than one per cell,
    /// and a contents block counting every section off the file the run just wrote.
    /// </summary>
    public sealed class ReportSizeTests : IDisposable
    {
        private const string Main = "<Mosques>";
        private const string Proposed = "Tree List - Proposed";

        private readonly string _folder = Path.Combine(
            Path.GetTempPath(), "rcrc-report-size-" + Guid.NewGuid().ToString("N"));

        public ReportSizeTests()
        {
            Directory.CreateDirectory(_folder);
        }

        public void Dispose()
        {
            if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
        }

        /// <summary>
        /// **ONE FORMULA FILLED DOWN A COLUMN IS ONE FINDING.** Four tree rows written into one
        /// sheet give the same three column formulas four times over, twelve cells, and the
        /// section printed one line for each of them. It is three lines now, one per column, and
        /// each says which cells it covers and how many.
        /// </summary>
        [Fact]
        public void TheFormulasReadingAWrittenRowAreSaidOncePerShapeAndNotOncePerCell()
        {
            FormulaCheck check = FourWrittenRows();

            // Ten cells: the canopy and the area formula on each of the four written rows, and
            // the sheet's own two totals, which read every one of those rows through a range.
            Assert.Equal(10, check.ReadingWrittenRows.Count);

            IReadOnlyList<RepeatedFormula> shapes = FormulaRepeats.Reading(check.ReadingWrittenRows);

            Assert.Equal(4, shapes.Count);

            // **THE PRINTED LINE CARRIES THE REAL CELLS**, as a span because these run down one
            // column with no gap, so no line reads as covering a cell it does not.
            Assert.Equal(
                new[] { "L4 to L7", "M4 to M7", "B10", "M10" },
                shapes.Select(one => one.Where).ToArray());
            Assert.Equal(new[] { 4, 4, 1, 1 }, shapes.Select(one => one.Cells.Count).ToArray());
            Assert.Equal(new[] { true, true, false, false }, shapes.Select(one => one.Repeats).ToArray());

            Assert.Equal(Proposed, shapes[0].SheetName);
            Assert.Equal("IF(ISBLANK(J4),\" \",ROUND(PI()*(J4/2)^2,0))", shapes[0].Text);

            // **J4 IS NAMED TWICE IN THAT FORMULA AND IS ONE READ.**
            Assert.Equal("reads row 4 of " + Proposed + " through J4", shapes[0].Reason);
            Assert.Equal("IF(ISBLANK(B4),\" \",L4*B4)", shapes[1].Text);
            Assert.Equal(
                "reads row 4 of " + Proposed + " through B4, row 4 of " + Proposed + " through L4",
                shapes[1].Reason);
        }

        /// <summary>
        /// A formula reading another column is another shape, so the grouping cannot swallow a
        /// difference. The normalisation replaces digit runs alone.
        /// </summary>
        [Fact]
        public void AFormulaReadingAnotherColumnIsAnotherShape()
        {
            IReadOnlyList<RepeatedFormula> shapes = FormulaRepeats.Reading(
                FourWrittenRows("K").ReadingWrittenRows);

            Assert.Equal(4, shapes.Count);
            Assert.Equal("IF(ISBLANK(K4),\" \",ROUND(PI()*(K4/2)^2,0))", shapes[0].Text);
            Assert.Equal("L4 to L7", shapes[0].Where);
        }

        [Fact]
        public void NoFormulaAtAllGivesNoShapes()
        {
            Assert.Empty(FormulaRepeats.Reading(null));
            Assert.Empty(FormulaRepeats.Reading(new List<FormulaCell>()));
        }

        /// <summary>
        /// **THE CONTENTS ARE COUNTED OFF THE TEXT AND NOT CLAIMED ABOUT IT.** Every section's
        /// lines add up to the body's own line count, so nothing is left out and nothing is
        /// counted twice, and a section printed once per plot carries one block per plot.
        /// </summary>
        [Fact]
        public void EverySectionIsCountedAndTheyAddUpToTheFile()
        {
            string body =
                "RCRC Green KPI checklist\r\n"
                + "Document: RCRC_NG05\r\n"
                + "== THIS RUN AT A GLANCE ==\r\n"
                + "the questions this press answers\r\n"
                + "  a line\r\n"
                + "== WHAT THE WORKBOOK WILL COMPUTE FROM THIS (2) ==\r\n"
                + "  one\r\n"
                + "  two\r\n"
                + "== WHAT THE WORKBOOK WILL COMPUTE FROM THIS (5) ==\r\n"
                + "  three\r\n";

            IReadOnlyList<ReportSection> sections = ReportSections.Of(body);

            Assert.Equal(10, ReportSections.LinesIn(body));
            Assert.Equal(10, sections.Sum(one => one.Lines));

            // Widest first, and the count in the brackets is off, so two blocks of one section
            // are one section.
            ReportSection formulas = sections[0];
            Assert.Equal("WHAT THE WORKBOOK WILL COMPUTE FROM THIS", formulas.Name);
            Assert.Equal(5, formulas.Lines);
            Assert.Equal(2, formulas.Blocks);
            Assert.Equal(2, formulas.LinesPerBlock);

            ReportSection glance = sections.Single(one => one.Name == "THIS RUN AT A GLANCE");
            Assert.Equal(3, glance.Lines);
            Assert.Equal(1, glance.Blocks);

            // **The opening is counted rather than dropped**, or the parts would not add to the
            // whole and the block would read as a measurement that is short.
            ReportSection opening = sections.Single(one => one.Name == ReportSections.Opening);
            Assert.Equal(2, opening.Lines);
            Assert.Equal(0, opening.Blocks);
        }

        [Fact]
        public void AHeadingIsReadOffItsOwnMarkersAndNothingElse()
        {
            Assert.Equal("CELLS NOT WRITTEN", ReportSections.HeadingIn("== CELLS NOT WRITTEN (3) =="));
            Assert.Equal("THIS RUN AT A GLANCE", ReportSections.HeadingIn("== THIS RUN AT A GLANCE =="));

            // A name really ending in a bracket keeps it, because only a trailing count comes off.
            Assert.Equal("A NAME (WITH BRACKETS)", ReportSections.HeadingIn("== A NAME (WITH BRACKETS) =="));

            Assert.Equal(string.Empty, ReportSections.HeadingIn("TEMPLATE: MOSQUES"));
            Assert.Equal(string.Empty, ReportSections.HeadingIn("================================"));
            Assert.Equal(string.Empty, ReportSections.HeadingIn("  a line == with == in it"));
            Assert.Equal(string.Empty, ReportSections.HeadingIn(null));
        }

        /// <summary>
        /// The block a real report carries, at the top, naming the sections that file really
        /// holds and the body's own line count.
        /// </summary>
        [Fact]
        public void TheReportOpensWithWhatIsInIt()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new List<PlotOutcome>());

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 14, 19, 52, 0));

            Assert.StartsWith("== " + KpiCreateReport.ContentsHeading + " (", report);
            Assert.Contains("    lines |  blocks |    each | section", report);
            Assert.Contains(KpiCreateReport.GlanceHeading, report);
            Assert.Contains("lines in the body below. This block is above them and is not in the counts.", report);

            // **The contents count the BODY**, so the file is longer than the number it prints
            // and the line says so rather than the two silently disagreeing.
            int said = ReportSections.Of(report)
                .Where(one => one.Name != KpiCreateReport.ContentsHeading)
                .Sum(one => one.Lines);

            Assert.True(said < ReportSections.LinesIn(report));
        }

        private FormulaCheck FourWrittenRows(string canopyReads = null)
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(5, "Bauhinia purpurea", "6", "5"),
                    new WorkbookFixture.TreeRow(6, "Cassia glauca", "6", "5"),
                    new WorkbookFixture.TreeRow(7, "Hibiscus tiliaceus", "6", "5")
                },
                fileName: "MOSQUES" + (canopyReads ?? "J") + ".xlsx",
                canopyReads: canopyReads);

            var written = new List<CellWrite>();
            for (int row = 4; row <= 7; row++)
            {
                written.Add(CellWrite.Number(Proposed, "B" + row, row));
            }

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template, Path.Combine(_folder, "filled" + (canopyReads ?? "J") + ".xlsx"),
                written,
                KpiTemplates.Mosques.Cells
                    .Select(cell => new WorkbookCell(Main, cell.Cell))
                    .ToArray());

            return outcome.Formulas;
        }
    }
}
