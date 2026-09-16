using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE DIVISION CHECK MISSED A DIVISION BY A COUNT.** The 16:37 glance says the formula
    /// check found no #DIV/0! anywhere in the press. The FP-24 and SC-06 workbooks, recalculated,
    /// show one in Tree List - Existing S70 and one in T70, and 30 plots of that press hold every
    /// existing tree on rows 84 to 101. S70 reads `IF(TotTrees&lt;1," ",S69/COUNT(B4:B83))` and T70
    /// the same over T69, on all seven templates and on both tree list tabs. The check matched a
    /// division by ONE CELL only, so a division by a count over a range was never looked at.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class DivisionByACountTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        private static readonly string Sheet = KpiTemplates.ExistingTreesSheet;

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
        /// One press over the fixture, writing the cells a test names. B102 is what `TotTrees`
        /// points at, so a test sets the guard by writing it.
        /// </summary>
        private FormulaCheck Checked(string fileName, params CellWrite[] writes)
        {
            string template = WorkbookFixture.DividingByACount(_folder, fileName);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled-" + fileName),
                writes,
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            return outcome.Formulas;
        }

        /// <summary>
        /// **A COUNT IN B85 ONLY, WHICH IS PAST THE RANGE THE COUNT COVERS.** 17 trees on the
        /// plot, `TotTrees` reading 17 so the guard does not hold, and COUNT(B4:B83) over a range
        /// holding nothing. Both S70 and T70 are #DIV/0!.
        /// </summary>
        [Fact]
        public void ACountPastTheRangeIsADivideByZeroOnS70()
        {
            FormulaCheck check = Checked(
                "past.xlsx",
                CellWrite.Number(Sheet, "B85", 17),
                CellWrite.Number(Sheet, "B102", 17));

            var divisions = check.AtRisk.Where(one => one.IsDivideByZero).ToList();

            // **S70 IS IN THE MESSAGE.** A failure reading `expected 2, got 0` is the whole
            // fault in two numbers and says nothing about which cell went unreported.
            Assert.True(
                divisions.Any(one => one.Cell == "S70"),
                "S70 reads IF(TotTrees<1,\" \",S69/COUNT(B4:B83)), this plot's 17 trees are all "
                + "on row 85, outside B4:B83, and the check reported no #DIV/0! on S70. That is "
                + "what the 16:37 glance said about the whole press while FP-24 and SC-06 "
                + "recalculate with one there. The divisions it did find: "
                + (divisions.Count == 0
                    ? "none"
                    : string.Join(", ", divisions.Select(one => one.Cell).ToArray())));

            Assert.Equal(2, divisions.Count);

            FormulaAtRisk s70 = Assert.Single(divisions, one => one.Cell == "S70");

            Assert.Equal(Sheet, s70.SheetName);
            Assert.Equal(
                "#DIV/0!: COUNT over B4:B83 is 0 and this formula divides by it. "
                + "This run wrote nothing into that range.",
                s70.Reason);

            Assert.Contains(divisions, one => one.Cell == "T70");
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **A COUNT INSIDE THE RANGE GIVES NO LINE.** B10 is between B4 and B83, so COUNT is 1
        /// and the division is a real number.
        /// </summary>
        [Fact]
        public void ACountInsideTheRangeGivesNoLine()
        {
            FormulaCheck check = Checked(
                "inside.xlsx",
                CellWrite.Number(Sheet, "B10", 17),
                CellWrite.Number(Sheet, "B102", 17));

            Assert.DoesNotContain(check.AtRisk, one => one.IsDivideByZero);
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **NO COUNTS AT ALL GIVES NO LINE, BECAUSE THE IF HOLDS.** `TotTrees` is nought, so
        /// the formula returns a space and never reaches the division. A plot with no trees
        /// really has none, and calling that a #DIV/0! would be a line on every such plot.
        /// </summary>
        [Fact]
        public void NoCountsAtAllGivesNoLineBecauseTheGuardHolds()
        {
            FormulaCheck check = Checked("none.xlsx", CellWrite.Number(Sheet, "B102", 0));

            Assert.DoesNotContain(check.AtRisk, one => one.IsDivideByZero);
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **A GUARD CELL NOBODY WROTE IS NOUGHT TO EXCEL, SO THE GUARD HOLDS.** A blank cell
        /// compares below one, which is the ordinary state of a template nothing has filled.
        /// </summary>
        [Fact]
        public void AGuardCellLeftBlankHoldsTheGuard()
        {
            FormulaCheck check = Checked("blank.xlsx", CellWrite.Number(Sheet, "B85", 17));

            Assert.DoesNotContain(check.AtRisk, one => one.IsDivideByZero);
        }

        /// <summary>
        /// **A DIVISION THE CHECK CANNOT WORK OUT IS COUNTED AS NOT EVALUATED**, never as one
        /// that is fine. A cell of the range holding a formula has no value in this output at
        /// all, because the patcher drops every cached result on purpose.
        /// </summary>
        [Fact]
        public void ARangeHoldingAFormulaIsNotEvaluatedAndSaysWhy()
        {
            string template = WorkbookFixture.DividingByACount(
                _folder, "formula.xlsx", formulaInRangeAt: 20);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled-formula.xlsx"),
                new[] { CellWrite.Number(Sheet, "B102", 17) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            Assert.DoesNotContain(outcome.Formulas.AtRisk, one => one.IsDivideByZero);

            string said = Assert.Single(outcome.Formulas.DivisionsNotEvaluated,
                one => one.Contains("S70"));

            Assert.Contains(
                "B20 in B4:B83 holds a formula, whose value this file does not carry, so what "
                + "COUNT over that range comes to could not be worked out",
                said);
        }

        /// <summary>
        /// **THE SAME NAME DEFINED TWICE MEANS TWO THINGS, and the sheet the formula sits on
        /// says which.** `TotTrees` is at workbook level and again on Tree List - Proposed, where
        /// it points at a cell holding nought. A reader keeping the first definition by name
        /// alone would read the proposed sheet's nought here and call the guard held, so the
        /// #DIV/0! on the existing sheet would go unreported.
        /// </summary>
        [Fact]
        public void ANameScopedToAnotherSheetDoesNotAnswerForThisOne()
        {
            string template = WorkbookFixture.DividingByACount(
                _folder, "scoped.xlsx", totTreesLocalToProposedFirst: true);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled-scoped.xlsx"),
                new[]
                {
                    CellWrite.Number(Sheet, "B85", 17),
                    CellWrite.Number(Sheet, "B102", 17)
                },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            Assert.Equal(
                2,
                outcome.Formulas.AtRisk.Count(one => one.IsDivideByZero));
        }

        /// <summary>
        /// **THE GLANCE SAYS HOW MANY COULD NOT BE WORKED OUT**, rather than saying none was
        /// found. The 16:37 line said none and two workbooks of that press recalculate with two
        /// each.
        /// </summary>
        [Fact]
        public void TheGlanceCountsTheDivisionsItCouldNotWorkOut()
        {
            var glance = new DivisionGlance(
                0, 0, new string[0], new[] { "FP-24 | Tree List - Existing S70 = ...: it holds a formula" });

            Assert.Equal(
                "THE DIVISIONS: the formula check found no #DIV/0! anywhere in this press. "
                + "1 division was looked at and could not be worked out, so nothing here says "
                + "whether it is a #DIV/0!.",
                glance.InWords);
        }

        /// <summary>
        /// **AND A PRESS THAT WORKED EVERY ONE OF THEM OUT SAYS NOTHING ABOUT IT**, because a
        /// count of 0 in a sentence about what could not be done is a line the team reads past
        /// on every press.
        /// </summary>
        [Fact]
        public void APressThatEvaluatedEveryDivisionSaysNothingAboutIt()
        {
            Assert.Equal(
                "THE DIVISIONS: the formula check found no #DIV/0! anywhere in this press.",
                new DivisionGlance(0, 0, new string[0]).InWords);
        }
    }
}
