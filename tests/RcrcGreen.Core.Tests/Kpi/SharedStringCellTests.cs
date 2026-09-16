using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **EVERY PRINTED CELL TEXT RESOLVES A SHARED STRING TO ITS TEXT.** Measured on the 16:37
    /// press: the canopy check line read `D99 holds 419 and no formula, E99 holds 122 and no
    /// formula`, where D99 holds Prosopis Juliflora. A cell holding a shared string stores an
    /// INDEX and not the text, and the formula reader kept the raw value, so the report printed
    /// a row of index numbers at the team.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class SharedStringCellTests : IDisposable
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
        /// The guard's line for a row with no canopy formula names the typed cells, and the
        /// botanical name is one of them. Row 7 here is the row 99 shape: a name in D, a count
        /// in B and no canopy formula.
        /// </summary>
        private ArithmeticCheck Checked(bool namesAsSharedStrings)
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[]
                {
                    new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8"),
                    new WorkbookFixture.TreeRow(7, "Prosopis Juliflora", "6", "5")
                },
                fileName: "MOSQUES" + (namesAsSharedStrings ? "-shared" : "-inline") + ".xlsx",
                withoutCanopy: new[] { 7 },
                namesAsSharedStrings: namesAsSharedStrings);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled" + (namesAsSharedStrings ? "-shared" : "-inline") + ".xlsx"),
                new[] { CellWrite.Number(KpiTemplates.ProposedTreesSheet, "B7", 19) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            return WorkbookArithmetic.Canopy(
                outcome.Formulas,
                CanopyArea.Of(new[]
                {
                    new CanopyRow(KpiTemplates.ProposedTreesSheet, 7, "PROSOPIS JULIFLORA", 19, 5.0)
                }),
                new Dictionary<string, string> { { KpiTemplates.ProposedTreesSheet, "J" } },
                "GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx");
        }

        /// <summary>
        /// **THE NAME, NOT ITS INDEX.** D7 holds shared string 1, whose string is
        /// Prosopis Juliflora, and the line reads the name.
        /// </summary>
        [Fact]
        public void ASharedStringCellPrintsItsTextAndNeverItsIndex()
        {
            ArithmeticCheck check = Checked(true);

            Assert.False(check.Agrees);
            Assert.Contains("D7 holds Prosopis Juliflora and no formula", check.Why);

            // **AND NO INDEX REACHES THE LINE.** Albizia lebbeck is index 0 and
            // Prosopis Juliflora is index 1, so a reader taking the raw value would have
            // printed `D7 holds 1 and no formula`.
            Assert.DoesNotContain("D7 holds 1 and no formula", check.Why);
        }

        /// <summary>
        /// **AN INLINE STRING WAS ALREADY RIGHT AND STILL IS.** The two shapes are told apart by
        /// the cell's own `t` attribute, so the fix could not have moved this one.
        /// </summary>
        [Fact]
        public void AnInlineStringCellStillPrintsItsText()
        {
            Assert.Contains("D7 holds Prosopis Juliflora and no formula", Checked(false).Why);
        }

        /// <summary>
        /// **A NUMBER IS STILL A NUMBER.** B7 holds 19, written by this run as a plain value, and
        /// it prints as 19 rather than being looked up in the string table.
        /// </summary>
        [Fact]
        public void ANumberCellIsUntouchedByTheSharedStringRule()
        {
            Assert.Contains("B7 holds 19 and no formula", Checked(true).Why);
        }
    }
}
