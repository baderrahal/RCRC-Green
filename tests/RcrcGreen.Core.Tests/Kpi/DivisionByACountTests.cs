using System;
using System.Collections.Generic;
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
        /// A press over the shape all seven templates really carry: B102 reading
        /// `SUM(B4:B101)` and S69 a COUNTIFS, so the guard has to be followed to be answered.
        /// </summary>
        private FormulaCheck Summing(string fileName, params CellWrite[] writes)
        {
            string template = WorkbookFixture.DividingByACount(
                _folder, fileName, totTreesSums: true);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled-" + fileName),
                writes,
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            return outcome.Formulas;
        }

        /// <summary>
        /// One press over the fixture, writing the cells a test names. B102 is not in that file
        /// at all, so a test sets the guard by writing it.
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

            DivisionNotEvaluated held = Assert.Single(
                outcome.Formulas.DivisionsNotEvaluated, one => one.Cell == "S70");

            Assert.Equal(Sheet, held.SheetName);

            string said = held.InWords;

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
        /// **THE GUARD'S OWN CELL IS A SUM AND FOLLOWING IT IS THE WHOLE OF THIS.** `TotTrees`
        /// points at B102, B102 reads `SUM(B4:B101)`, and a guard that stopped at a formula cell
        /// answered not evaluated for every division on every plot. The 21:38 press printed 284
        /// such lines and its glance still said the check found no #DIV/0! anywhere.
        ///
        /// 3 trees in B85 is outside `COUNT(B4:B83)`, so the count is nought and S70 and T70 are
        /// both #DIV/0!. The SUM reaches row 101, so the guard reads 3 and does not hold.
        /// </summary>
        [Fact]
        public void ACountPastTheRangeIsFoundThroughTheGuardsOwnSum()
        {
            FormulaCheck check = Summing("sum-past.xlsx", CellWrite.Number(Sheet, "B85", 3));

            var divisions = check.AtRisk.Where(one => one.IsDivideByZero).ToList();

            Assert.True(
                divisions.Any(one => one.Cell == "S70"),
                "TotTrees points at B102, which reads SUM(B4:B101), and this plot's 3 trees sit "
                + "on row 85, so the guard reads 3 and the division is reached with "
                + "COUNT(B4:B83) at nought. The check reported no #DIV/0! on S70 and instead "
                + "said the guard could not be evaluated, which is what the 21:38 press said "
                + "284 times. The divisions it did find: "
                + (divisions.Count == 0
                    ? "none"
                    : string.Join(", ", divisions.Select(one => one.Cell).ToArray()))
                + ". What it could not work out: "
                + (check.DivisionsNotEvaluated.Count == 0
                    ? "nothing"
                    : string.Join(", ", check.DivisionsNotEvaluated
                        .Select(one => one.InWords).ToArray())));

            Assert.Equal(2, divisions.Count);
            Assert.Contains(divisions, one => one.Cell == "T70");
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **5 IN B10 IS INSIDE THE RANGE, SO NOTHING IS SAID.** The guard reads 5 and does not
        /// hold, the division is reached, and `COUNT(B4:B83)` is 1 rather than nought.
        /// </summary>
        [Fact]
        public void ACountInsideTheRangeGivesNothingThroughTheGuardsOwnSum()
        {
            FormulaCheck check = Summing("sum-inside.xlsx", CellWrite.Number(Sheet, "B10", 5));

            Assert.DoesNotContain(check.AtRisk, one => one.IsDivideByZero);
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **NO COUNTS AT ALL GIVES NOTHING, BECAUSE THE GUARD REALLY HOLDS.** The SUM over an
        /// empty range is nought, nought is below one, and the formula returns a space without
        /// ever reaching the division. A plot with no trees has no average and gets no line.
        /// </summary>
        [Fact]
        public void NoCountsAtAllHoldsTheGuardThroughItsOwnSum()
        {
            // A press writes something or there is nothing to patch, and a plot with no trees
            // still writes its planting cells, so this is one of those on the first tab and no
            // count anywhere on the tree list.
            FormulaCheck check = Summing(
                "sum-none.xlsx", CellWrite.Number(WorkbookFixture.MainSheet, "F10", 0));

            Assert.DoesNotContain(check.AtRisk, one => one.IsDivideByZero);
            Assert.Empty(check.DivisionsNotEvaluated);
        }

        /// <summary>
        /// **A DIVISION NOBODY COULD WORK OUT MAKES READY READ NO AND NAMES THE CELL.** The
        /// 21:38 press held 284 of them and every one of its 71 written plots read READY YES.
        /// </summary>
        [Fact]
        public void ADivisionThatCouldNotBeCheckedMakesReadyReadNoAndNamesTheCell()
        {
            // B20 sits inside B4:B83 and holds a formula, whose value this output does not
            // carry, so what the count comes to cannot be worked out at all.
            string template = WorkbookFixture.DividingByACount(
                _folder, "notchecked.xlsx", formulaInRangeAt: 20);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template,
                Path.Combine(_folder, "filled-notchecked.xlsx"),
                new[] { CellWrite.Number(Sheet, "B102", 3) },
                new WorkbookCell[0]);

            Assert.True(outcome.Written, outcome.Refusal);

            Assert.Equal(2, outcome.Formulas.DivisionsNotEvaluated.Count);

            DivisionNotEvaluated first = outcome.Formulas.DivisionsNotEvaluated[0];
            Assert.Equal(Sheet, first.SheetName);
            Assert.Equal("S70", first.Cell);

            KpiCreateRun run = CreateFixture.Run(
                readings: new[] { CreateFixture.Plot("NS-29", component: "STREET 36m ROW") },
                template: KpiTemplates.Streets,
                outcome: outcome);

            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new[] { "NS-29" }, plotId => "STREET 36m ROW", new[] { KpiTemplates.Streets }),
                new[] { run },
                new List<TemplateOutcome>());

            Assert.Equal(
                "a division could not be checked, Tree List - Existing S70, "
                + "Tree List - Existing T70",
                PlotReady.DivisionsNotChecked(set, "NS-29"));
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
                0, 0, new string[0], new[] { "FP-24 | Tree List - Existing S70" });

            Assert.Equal(
                "THE DIVISIONS: the formula check found no #DIV/0! anywhere in this press. "
                + "1 division was looked at and could not be worked out, so nothing here says "
                + "whether it is a #DIV/0!. It is named under this line and every one of them "
                + "is in its own plot's block below.",
                glance.InWords);
        }

        /// <summary>
        /// **AND IT NAMES AT MOST FIVE.** The 21:38 press held 284, S70 and T70 on both tree
        /// lists of every one of its 71 written plots, and the glance printed all 284 of them.
        /// </summary>
        [Fact]
        public void TheGlanceNamesAtMostFiveOfThemAndSaysWhereTheRestAre()
        {
            var lines = new List<string>();
            for (int at = 1; at <= 7; at++) lines.Add("ST-1" + at + " | Tree List - Existing S70");

            var glance = new DivisionGlance(0, 0, new string[0], lines);

            Assert.Equal(7, glance.NotEvaluated.Count);

            Assert.Equal(
                new[]
                {
                    "ST-11 | Tree List - Existing S70",
                    "ST-12 | Tree List - Existing S70",
                    "ST-13 | Tree List - Existing S70",
                    "ST-14 | Tree List - Existing S70",
                    "ST-15 | Tree List - Existing S70"
                },
                glance.NotEvaluatedNamed.ToArray());

            Assert.Contains(
                "7 divisions were looked at and could not be worked out, so nothing here says "
                + "whether they are a #DIV/0!. The first 5 are named under this line and every "
                + "one of them is in its own plot's block below.",
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
