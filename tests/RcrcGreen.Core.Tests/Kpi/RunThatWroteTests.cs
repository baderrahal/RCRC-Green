using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **The report and the status line of a run that actually wrote.**
    ///
    /// Proved by the audit: printing CELLS WRITTEN off the plan's stored values rather than off
    /// the landed cells left the whole suite green. <see cref="PatchOutcome.Done"/> was built in
    /// no test file but the patcher's own, <c>CreateFixture.Run</c> always handed over a refused
    /// outcome or none, and the status line's "N cells written from M plots" was asserted
    /// nowhere.
    ///
    /// So the section whose heading says every cell was read back off the output file and never
    /// as it was sent was the one section no test reached. This is that section, held against
    /// what landed.
    /// </summary>
    public class RunThatWroteTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        private static readonly DateTime Written = new DateTime(2026, 9, 14, 9, 0, 0);

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
        /// The values here are deliberately unlike the ones the plan would carry, so a report
        /// printing the plan rather than the file cannot match them by coincidence.
        /// </summary>
        private KpiCreateRun Wrote()
        {
            return CreateFixture.RunThatWrote(
                _folder,
                CellWrite.Text(WorkbookFixture.MainSheet, "D3", "FRIDAY MOSQUE"),
                CellWrite.Number(WorkbookFixture.MainSheet, "D8", 3728.757),
                CellWrite.Number(WorkbookFixture.TreesSheet, "B4", 31));
        }

        [Fact]
        public void TheFixtureCanBuildARunThatWrote()
        {
            KpiCreateRun run = Wrote();

            Assert.True(run.Wrote);
            Assert.NotNull(run.Outcome);
            Assert.True(run.Outcome.Written);
            Assert.Equal(3, run.Outcome.Landed.Count);
        }

        /// <summary>
        /// **One assertion per landed cell.** The report prints the sheet, the cell and the
        /// value as it landed, and each is bound to its own value here, so a section printed off
        /// the plan reddens rather than matching a count.
        /// </summary>
        [Fact]
        public void EveryCellIsPrintedAsItLandedInTheFile()
        {
            KpiCreateRun run = Wrote();

            Assert.Equal(
                new[]
                {
                    WorkbookFixture.MainSheet + "|D3|FRIDAY MOSQUE",
                    WorkbookFixture.MainSheet + "|D8|3728.757",
                    WorkbookFixture.TreesSheet + "|B4|31"
                },
                run.Outcome.Landed
                    .Select(one => one.SheetName + "|" + one.Cell + "|" + one.Value)
                    .ToArray());
        }

        /// <summary>
        /// The heading says what the section is, and the run that wrote is the only one that
        /// can prove the words under it are the file's.
        /// </summary>
        [Fact]
        public void TheReportSaysEveryCellWasReadBackOffTheOutput()
        {
            string report = KpiCreateReport.Write(Wrote(), Written);

            Assert.Contains("== CELLS WRITTEN (3) ==", report);
            Assert.Contains(
                "every one read back off the output file, never as it was sent",
                report);
            Assert.Contains("  sheet | cell | value as it landed", report);
        }

        /// <summary>
        /// **The lines the audit's break would have changed.** Each names its cell and its
        /// value, off the file, so printing the plan's stored values instead reddens here.
        /// </summary>
        [Fact]
        public void TheReportPrintsOneLinePerLandedCell()
        {
            string report = KpiCreateReport.Write(Wrote(), Written);

            Assert.Contains("  " + WorkbookFixture.MainSheet + " | D3 | FRIDAY MOSQUE", report);
            Assert.Contains("  " + WorkbookFixture.MainSheet + " | D8 | 3728.757", report);
            Assert.Contains("  " + WorkbookFixture.TreesSheet + " | B4 | 31", report);
        }

        /// <summary>
        /// **The status line, asserted nowhere before this.** It counts the cells that landed
        /// and the plots that were read, and it is the one line a person sees after the press.
        /// </summary>
        [Fact]
        public void TheStatusLineCountsTheCellsThatLandedAndThePlotsRead()
        {
            KpiCreateRun run = Wrote();
            string said = CreateWords.Wrote(run, @"C:\reports\kpi.txt");

            Assert.StartsWith("3 cells written from 1 plot, ", said);
            Assert.Contains(@"Report: C:\reports\kpi.txt", said);
            Assert.Contains("Workbook: " + run.OutputPath, said);
        }

        /// <summary>
        /// A run that wrote nothing still says why, and it reads differently from one that did,
        /// so neither case can be mistaken for the other in a report file.
        /// </summary>
        [Fact]
        public void ARunThatWroteNothingReadsDifferently()
        {
            string refused = KpiCreateReport.Write(CreateFixture.Run(), Written);

            Assert.DoesNotContain("every one read back off the output file", refused);
            Assert.Contains("  sheet | cell | value as it landed", KpiCreateReport.Write(Wrote(), Written));
        }

        /// <summary>
        /// The part accounting is the other half of the outcome and no test reached it through a
        /// run either. One part fewer is the calculation chain, removed on purpose.
        /// </summary>
        [Fact]
        public void ThePartCountsComeOffTheRunThatWrote()
        {
            KpiCreateRun run = Wrote();
            string report = KpiCreateReport.Write(run, Written);

            Assert.Equal(run.Outcome.PartsInSource - 1, run.Outcome.PartsInOutput);
            Assert.Contains(
                "  parts in the template " + run.Outcome.PartsInSource
                + ", parts in the output " + run.Outcome.PartsInOutput,
                report);
        }
    }
}
