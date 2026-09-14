using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE TOOL COULD NOT SEE A DIVIDE BY ZERO.**
    ///
    /// The 09:18 run wrote two workbooks that recalculate with 2 #DIV/0! each,
    /// ANH-007-SC-100004 and ANH-007-ST-100130, neither on the main sheet and both on plots with
    /// few trees. The report named none of them, because the formula check knew one shape only:
    /// a formula returning text off ISBLANK and the arithmetic on it. **Which sheet and which
    /// cells those four are cannot be worked out from this repository**, because no client
    /// workbook is in it and none ever will be. So the tool is taught to say it, and the next
    /// run answers the question by itself.
    ///
    /// **It is reported and never refused on.** A plot with no trees really has no average, so
    /// the divide by zero is the client's own arithmetic over a real number, and deleting a
    /// correct workbook over it is worse than printing a line. The reason says whether THIS RUN
    /// wrote the cell being divided by, which is the half that would make it the tool's doing.
    /// </summary>
    public class DivideByZeroTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private const string Main = "<Mosques>";

        /// <summary>
        /// The fixture's own main sheet holds H7 as a real numeric nought and D9 as D8/H7, which
        /// is the shape a client template carries wherever it computes a rate off an input.
        /// </summary>
        private string Template()
        {
            return WorkbookFixture.Computing(_folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Cassia glauca", "6", "5") });
        }

        private string Output()
        {
            return Path.Combine(_folder, "filled.xlsx");
        }

        private static FormulaAtRisk On(FormulaCheck check, string cell)
        {
            return check.AtRisk.FirstOrDefault(one => one.Cell == cell);
        }

        /// <summary>
        /// **A cell the run left at nought is named, and the line says the run did not write
        /// it.** That is the answer to whether the tool put the zero there, printed rather than
        /// worked out by a person reading two files side by side.
        /// </summary>
        [Fact]
        public void ADivisorTheRunLeftAtNoughtIsNamedAndSaysTheRunDidNotWriteIt()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Text(Main, "D3", "FRIDAY MOSQUE")
            });

            Assert.True(outcome.Written, outcome.Refusal);

            FormulaAtRisk divided = On(outcome.Formulas, "D9");
            Assert.True(divided != null, "D9 divides by H7, which holds 0, and nothing named it");
            Assert.Equal(Main, divided.SheetName);
            Assert.Equal(
                "#DIV/0!: H7 holds 0, and this formula divides by it. "
                + "This run wrote nothing into that cell.",
                divided.Reason);
            Assert.True(divided.IsAnError);
        }

        /// <summary>
        /// **And a nought THIS RUN wrote reads differently**, which is the half that would make
        /// the error the tool's doing rather than the client's arithmetic.
        /// </summary>
        [Fact]
        public void ADivisorThisRunWroteAsNoughtSaysSoInCapitals()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 0.0)
            });

            Assert.True(outcome.Written, outcome.Refusal);

            Assert.Equal(
                "#DIV/0!: H7 holds 0, and this formula divides by it. THIS RUN WROTE THAT CELL.",
                On(outcome.Formulas, "D9").Reason);
        }

        /// <summary>
        /// **A real number in the divisor is not a risk**, so the check cannot read every rate
        /// on every workbook as an error.
        /// </summary>
        [Fact]
        public void ADivisorHoldingARealNumberIsNotAtRisk()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Number(Main, "H7", 3728.757)
            });

            Assert.True(outcome.Written, outcome.Refusal);
            Assert.Null(On(outcome.Formulas, "D9"));
        }

        /// <summary>
        /// **IT NEVER REFUSES THE WRITE, and that is deliberate.** The workbook is written with
        /// the line in the report beside it, whether or not this run wrote the nought, because
        /// a plot with no trees genuinely has no average and deleting a correct workbook over
        /// the client's own arithmetic is worse than printing a line.
        /// </summary>
        [Fact]
        public void ADivideByZeroIsReportedAndNeverRefusesTheWrite()
        {
            foreach (CellWrite[] writes in new[]
            {
                new[] { CellWrite.Text(Main, "D3", "FRIDAY MOSQUE") },
                new[] { CellWrite.Number(Main, "H7", 0.0) }
            })
            {
                string output = Path.Combine(_folder, Guid.NewGuid().ToString("N") + ".xlsx");
                PatchOutcome outcome = WorkbookPatcher.Patch(Template(), output, writes);

                Assert.True(outcome.Written, outcome.Refusal);
                Assert.True(File.Exists(output), "the output was deleted over a divide by zero");
                Assert.False(outcome.Formulas.RefusesTheWrite);
                Assert.Equal(string.Empty, outcome.Formulas.Refusal);
            }
        }

        /// <summary>
        /// The line reaches the report, under the section whose whole job is what the workbook
        /// will compute from this.
        /// </summary>
        [Fact]
        public void TheReportPrintsTheDivideByZeroWithItsCellAndItsReason()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Template(), Output(), new[]
            {
                CellWrite.Text(Main, "D3", "FRIDAY MOSQUE")
            });

            string report = KpiCreateReport.Write(
                CreateFixture.Run(outcome: outcome), new DateTime(2026, 9, 14, 9, 18, 0));

            Assert.Contains(
                "#DIV/0!: H7 holds 0, and this formula divides by it. "
                + "This run wrote nothing into that cell.",
                report);
        }
    }
}
