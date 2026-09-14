using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The two numbers this tool COMPUTES, the units every field is written in, and the guard on
    /// the one workbook formula the computing copies.
    ///
    /// **These are the first numbers this tool produces that no schedule printed.** Adding
    /// printed numbers with the working shown was already allowed and this is that rule one step
    /// further, so every one of them carries its parts and the report prints them.
    /// </summary>
    public class PdfComputedTests : IDisposable
    {
        private static readonly DateTime Today = new DateTime(2026, 9, 14);

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

        private static GroupSubtotal Shrubs(params PhaseSubtotal[] phases)
        {
            return new GroupSubtotal(KpiMerge.ShrubsHeading, 0.0, 0, phases: phases);
        }

        private static PhaseSubtotal Phase(string name, double squareMetres)
        {
            return new PhaseSubtotal(name, 0, squareMetres, 0, true, string.Empty);
        }

        private static PdfPlan Plan(string plotId, KpiTemplate template, PdfWorkbookNumbers numbers)
        {
            return PdfFill.Of(
                CreateFixture.Plot(plotId, uid2: "ANH-006-NP-100002"),
                CountedGroups.Of(template), null, Today, true, numbers);
        }

        private static PdfFieldFill Field(PdfPlan plan, PdfValue value)
        {
            return plan.Fields.Single(one => one.Value == value);
        }

        /// <summary>
        /// The workbook agreeing with this tool on both computed cells, which is what the seven
        /// real templates were measured to do. The percentage answers nothing to check, which is
        /// what the two park templates really carry.
        /// </summary>
        private static SummaryCellCheck GreenCoverAgrees()
        {
            return SummaryCellCheck.Agreeing(
                ComputedPlaces.GreenCoverName, "D9", "F9+F11+H11", "F9");
        }

        private static SummaryCellCheck PercentageHasNoCell()
        {
            return SummaryCellCheck.WithNothingToCheck(
                ComputedPlaces.PercentageName, WorkbookArithmetic.TheWorkbookHasNoPercentageCell);
        }

        /// <summary>
        /// **THE CANOPY IS THE WORKBOOK'S OWN COLUMN, WORKED OUT THE WORKBOOK'S OWN WAY.**
        /// Measured on the MOSQUES template, Tree List - Proposed row 21:
        /// `L21 =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))` and `M21 =IF(ISBLANK(B21)," ",L21*B21)`.
        /// The rounding is INSIDE, per tree, so eight metres across is fifty square metres each
        /// and three of them are a hundred and fifty. Rounding the sum instead gives 151.
        /// </summary>
        [Fact]
        public void TheCanopyRoundsPerTreeTheWayTheWorkbookDoes()
        {
            var row = new CanopyRow(Proposed, 21, "ALBIZIA LEBBECK", 3, 8.0);

            Assert.Equal(150.0, row.SquareMetres);
            Assert.Equal(
                "Tree List - Proposed row 21, ALBIZIA LEBBECK: ROUND(PI()*(8/2)^2, 0) * 3 = 150",
                row.Working);

            // Five metres across is 19.63 square metres, which rounds to 20 per tree.
            Assert.Equal(380.0, new CanopyRow(Proposed, 7, "CASSIA GLAUCA", 19, 5.0).SquareMetres);
        }

        /// <summary>
        /// **The other half of that one measurement, written out as the file stores it.** The
        /// arithmetic above and this text are one fact in two languages, so each is pinned by
        /// hand off the same measured row and neither can drift alone.
        ///
        /// `L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))`, Bader on the MOSQUES template.
        /// **The diameter column is not written into the tool.** J is what that sheet uses and
        /// every sheet's own is found by its heading, so the caller hands the column in.
        /// </summary>
        [Fact]
        public void TheCanopyFormulaTextIsTheOneMeasuredOnRow21()
        {
            Assert.Equal(
                "IF(ISBLANK(J21),\" \",ROUND(PI()*(J21/2)^2,0))",
                WorkbookArithmetic.CanopyColumn("J", 21));

            Assert.Equal(
                "IF(ISBLANK(K7),\" \",ROUND(PI()*(K7/2)^2,0))",
                WorkbookArithmetic.CanopyColumn("K", 7));
        }

        /// <summary>
        /// A row this run wrote a count into that carries no canopy diameter **is named and is
        /// not counted as nought**, because its own canopy cell returns a space in the workbook
        /// too, so it adds nothing there either.
        /// </summary>
        [Fact]
        public void ARowWithNoDiameterIsNamedRatherThanCountedAsNought()
        {
            CanopyTotal total = CanopyArea.Of(new[]
            {
                new CanopyRow(Proposed, 21, "ALBIZIA LEBBECK", 3, 8.0),
                new CanopyRow(Proposed, 85, "UNKNOWN", 2, 0.0)
            });

            Assert.Equal(150.0, total.SquareMetres);
            Assert.Equal(
                "Tree List - Proposed row 85, UNKNOWN: no canopy diameter, so it adds no canopy "
                + "here and adds none in the workbook either",
                Assert.Single(total.Skipped));
        }

        /// <summary>
        /// **A MATCHED ROW CARRIES CANOPY, AND ITS DIAMETER COMES OFF THE WORKBOOK.**
        ///
        /// Measured on the 18:15 run: Total Green cover was the planting plus the lawn and
        /// nothing else on every plot of 150. ANH-007-MO-100001 wrote 0.000105 km², which is 105
        /// square metres, beside a workbook holding shrubs 70 and lawn 35, with twelve proposed
        /// trees contributing nothing. ANH-007-NP-100001 wrote 0.000849, and that same PDF's
        /// Lawn field reads 849, with 55 existing and 38 proposed trees contributing nothing.
        ///
        /// **BOTH CANDIDATES WERE CHECKED AND IT WAS THE FIRST.** The rows it read were not the
        /// rows it wrote: it took only matches with `Added` true, which is a species written into
        /// an empty row, so every row the client's list already held was left out whatever its
        /// diameter said. The second candidate is real on the same rows and was fixed with it:
        /// the tool writes a diameter only into a row it creates, so a matched row's canopy has
        /// to come off the client's own cell.
        /// </summary>
        [Fact]
        public void AMatchedRowCountsAndItsDiameterComesOffTheWorkbook()
        {
            var matched = new SpeciesMatch(
                CreateFixture.Merged("ALBIZIA LEBBECK", CreateFixture.Proposed, "DM-11", 12),
                KpiTemplates.ProposedTreesSheet, 21, "Albizia lebbeck", string.Empty,
                added: false,
                list: SpeciesList.Holding(
                    new[] { new SpeciesListRow(21, "Albizia lebbeck", "15", "8") }, 4, 83,
                    diameterColumn: "J"),
                listRow: new SpeciesListRow(21, "Albizia lebbeck", "15", "8"));

            // **THROUGH CanopyArea.From, which is the method that dropped them.** Asserting on a
            // CanopyRow built by hand would have stayed green over exactly this break.
            KpiCreateRun run = CreateFixture.Run(matches: new[] { matched });

            CanopyTotal canopy = CanopyArea.From(run.Plan);

            // Eight metres across is fifty square metres a tree, and twelve of them are 600.
            Assert.False(matched.Added);
            Assert.Equal("8", matched.WorkbookDiameter);
            Assert.Equal(600.0, canopy.SquareMetres);
            Assert.Equal(
                "Tree List - Proposed row 21, ALBIZIA LEBBECK: ROUND(PI()*(8/2)^2, 0) * 12 = 600",
                Assert.Single(canopy.Rows).Working);
        }

        /// <summary>
        /// **And a row this run created still counts, off the diameter this run wrote into it.**
        /// The two routes are one method and a fix to either must not drop the other.
        /// </summary>
        [Fact]
        public void ARowThisRunCreatedCountsOffTheDiameterTheRunWrote()
        {
            var added = new SpeciesMatch(
                CreateFixture.Merged("PHOENIX DACTYLIFERA", CreateFixture.Proposed, "DM-11", 3),
                KpiTemplates.ProposedTreesSheet, 85, string.Empty, string.Empty, added: true);

            CanopyTotal canopy = CanopyArea.From(CreateFixture.Run(matches: new[] { added }).Plan);

            // The fixture's species prints a diameter of 8, so fifty square metres a tree.
            Assert.True(added.Added);
            Assert.Equal(150.0, canopy.SquareMetres);
        }

        /// <summary>
        /// **Total Green cover is the canopy plus the planting plus the lawn**        /// <summary>
        /// **Total Green cover is the canopy plus the planting plus the lawn**, and the working
        /// names all three. Measured on the first real filled workbook, which recalculated to
        /// Total Green cover 1518 off canopy 1048, planting 410 and lawn 60.
        /// </summary>
        [Fact]
        public void TheGreenCoverAddsTheCanopyThePlantingAndTheLawnAndShowsIt()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 21, "ALBIZIA LEBBECK", 3, 8.0) });

            ComputedValue found = GreenCover.Total(canopy, 410.0, 60.0);

            Assert.True(found.Computed);
            Assert.Equal(620.0, found.Value);
            Assert.Equal(
                "canopy 150 plus planting 410 plus lawn 60 is 620 square metres, off 1 tree row",
                found.Working);
        }

        /// <summary>
        /// **THE WORKBOOK'S CELL HOLDS A RATIO AND THE FORM PRINTS THE SIGN**, so the ratio is
        /// multiplied by a hundred and no percent sign is written. A plot with no area divides
        /// by nought, so nothing is written and it says so.
        /// </summary>
        [Fact]
        public void ThePercentageIsTheRatioTimesAHundredWithNoSign()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 21, "ALBIZIA LEBBECK", 4, 8.0) });

            ComputedValue found = GreenCover.Percentage(canopy, 800.0);

            Assert.True(found.Computed);
            Assert.Equal(25.0, found.Value);
            Assert.Equal(
                "canopy 200 over area 800 is 0.25, written as 25 because the form prints the sign",
                found.Working);

            Assert.Equal(GreenCover.NoArea, GreenCover.Percentage(canopy, 0.0).Why);
        }

        /// <summary>
        /// **Checked against the client's own filled ANH-006-NP-100002**, which reads an area of
        /// 771, 0.000550 square kilometres greened and a percentage of 71. Eleven trees eight
        /// metres across are 550 square metres of canopy, so the square kilometres land on the
        /// client's own 0.00055 exactly, and the ratio is 0.7133, which their form prints as 71.
        /// </summary>
        [Fact]
        public void TheClientsOwnFilledFormIsWhatTheTwoAreCheckedAgainst()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 4, "ALBIZIA LEBBECK", 11, 8.0) });

            Assert.Equal(550.0, canopy.SquareMetres);
            Assert.Equal(550.0, GreenCover.Total(canopy, 0.0, 0.0).Value);
            Assert.Equal(0.00055, GreenCover.Total(canopy, 0.0, 0.0).Value / 1000000.0);
            Assert.Equal(71.34, Math.Round(GreenCover.Percentage(canopy, 771.0).Value, 2));
        }

        /// <summary>
        /// The two fields come out COMPUTED, in the form's own units, with their working beside
        /// them. Parks is the one form of the three carrying both.
        /// </summary>
        [Fact]
        public void TheTwoComputedFieldsCarryTheirUnitAndTheirWorking()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 4, "ALBIZIA LEBBECK", 11, 8.0) });

            PdfPlan plan = Plan("EP-05", KpiTemplates.ExistingParks, new PdfWorkbookNumbers(
                canopy, 0.0, 0.0, 771.0,
                ArithmeticCheck.Agreeing(new[] { "Tree List - Proposed L4 = a formula" }),
                GreenCoverAgrees(), PercentageHasNoCell()));

            PdfFieldFill greened = Field(plan, PdfValue.TotalAreasToBeGreened);
            Assert.Equal("0.00055", greened.Text);
            Assert.Equal("km²", greened.Unit);
            Assert.True(greened.Computed);
            Assert.Equal(
                "canopy 550 plus planting 0 plus lawn 0 is 550 square metres, off 1 tree row, "
                + "over 1,000,000 is 0.00055 square kilometres",
                greened.Working);

            PdfFieldFill percentage = Field(plan, PdfValue.PercentageCanopy);
            Assert.Equal("71.34", percentage.Text);
            Assert.Equal("%", percentage.Unit);
            Assert.True(percentage.Computed);
        }

        /// <summary>
        /// **The Roads form's note names the CANOPY cell where the other two name Total Green
        /// cover**, for a field all three call Total areas to be greened. Each form is filled
        /// from its own note and the working says which of the two the number is, so a person
        /// reading three forms side by side is not left to guess. Which the client means is an
        /// open question.
        /// </summary>
        [Fact]
        public void TheRoadsFormIsFilledFromItsOwnNoteAndSaysSo()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 4, "ALBIZIA LEBBECK", 11, 8.0) });
            var numbers = new PdfWorkbookNumbers(
                canopy, 410.0, 60.0, 771.0, ArithmeticCheck.Agreeing(null),
                GreenCoverAgrees(), PercentageHasNoCell());

            PdfFieldFill roads = Field(Plan("ST-05", KpiTemplates.Streets, numbers), PdfValue.TotalAreasToBeGreened);
            PdfFieldFill parks = Field(Plan("EP-05", KpiTemplates.ExistingParks, numbers), PdfValue.TotalAreasToBeGreened);

            // 550 alone against 550 plus 410 plus 60.
            Assert.Equal("0.00055", roads.Text);
            Assert.Equal("0.00102", parks.Text);
            Assert.Contains(PdfFill.RoadsNamesTheCanopyCell, roads.Working);
        }

        /// <summary>
        /// **A canopy column that has drifted stops BOTH numbers**, because both rest on it, and
        /// the cell and what its formula now says are named. Proven against a real workbook whose
        /// canopy column reads K where the tool's own text reads the diameter column J.
        /// </summary>
        [Fact]
        public void ACanopyColumnThatHasDriftedWritesNothingAndNamesTheFormula()
        {
            CanopyTotal canopy = CanopyArea.Of(new[] { new CanopyRow(Proposed, 7, "BAUHINIA PURPUREA", 19, 5.0) });
            var columns = new Dictionary<string, string> { { Proposed, "J" } };

            ArithmeticCheck agreeing = WorkbookArithmetic.Canopy(Formulas(null), canopy, columns);

            Assert.True(agreeing.Agrees, agreeing.Why);
            Assert.Equal(
                "Tree List - Proposed L7 = IF(ISBLANK(J7),\" \",ROUND(PI()*(J7/2)^2,0))",
                Assert.Single(agreeing.Read));

            ArithmeticCheck drifted = WorkbookArithmetic.Canopy(Formulas("K"), canopy, columns);

            Assert.True(drifted.Checked);
            Assert.False(drifted.Agrees);
            Assert.Equal(
                "the workbook's canopy column is not the one this tool works out. "
                + "Tree List - Proposed row 7: no cell on it carries "
                + "IF(ISBLANK(J7),\" \",ROUND(PI()*(J7/2)^2,0)). The row holds "
                + "L7 = IF(ISBLANK(K7),\" \",ROUND(PI()*(K7/2)^2,0)), M7 = IF(ISBLANK(B7),\" \",L7*B7).",
                drifted.Why);

            PdfPlan plan = Plan("EP-05", KpiTemplates.ExistingParks,
                new PdfWorkbookNumbers(
                    canopy, 410.0, 60.0, 771.0, drifted, GreenCoverAgrees(), PercentageHasNoCell()));

            Assert.Equal(drifted.Why, Field(plan, PdfValue.TotalAreasToBeGreened).Why);
            Assert.Equal(drifted.Why, Field(plan, PdfValue.PercentageCanopy).Why);
        }

        /// <summary>
        /// A workbook the run wrote no tree row into has no canopy formula in play at all, so its
        /// green cover is the planting and the lawn and it is still written. **That is told from
        /// a check that could not be made**, which writes nothing.
        /// </summary>
        [Fact]
        public void NothingToCheckIsNotTheSameAsACheckThatCouldNotBeMade()
        {
            ArithmeticCheck nothing = WorkbookArithmetic.Canopy(
                Formulas(null), CanopyTotal.Nothing, new Dictionary<string, string>());

            Assert.False(nothing.Checked);
            Assert.True(nothing.NothingToCheck);
            Assert.True(nothing.Usable);
            Assert.Equal(WorkbookArithmetic.NoRowsWritten, nothing.Why);

            PdfPlan plan = Plan("EP-05", KpiTemplates.ExistingParks,
                new PdfWorkbookNumbers(
                    CanopyTotal.Nothing, 410.0, 60.0, 771.0, nothing,
                    GreenCoverAgrees(), PercentageHasNoCell()));

            Assert.Equal("0.00047", Field(plan, PdfValue.TotalAreasToBeGreened).Text);

            ArithmeticCheck unread = WorkbookArithmetic.Canopy(null, CanopyTotal.Nothing, null);

            Assert.False(unread.Checked);
            Assert.False(unread.NothingToCheck);
            Assert.False(unread.Usable);
            Assert.Equal(WorkbookArithmetic.NoWorkbookRead, unread.Why);
        }

        /// <summary>
        /// A real workbook, patched, with its formulas read back off the output. The canopy
        /// column reads the diameter column by default and reads <paramref name="canopyReads"/>
        /// where one is given, which is a client whose arithmetic has moved.
        /// </summary>
        private FormulaCheck Formulas(string canopyReads)
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                fileName: "MOSQUES" + (canopyReads ?? "J") + ".xlsx",
                canopyReads: canopyReads);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template, Path.Combine(_folder, "filled" + (canopyReads ?? "J") + ".xlsx"),
                new[]
                {
                    CellWrite.Number(Main, "H7", 62023),
                    CellWrite.Text(Proposed, "D7", "BAUHINIA PURPUREA"),
                    CellWrite.Number(Proposed, "B7", 19),
                    CellWrite.Number(Proposed, "I7", 6),
                    CellWrite.Number(Proposed, "J7", 5),
                    CellWrite.Number(Proposed, "K7", 5)
                },
                KpiTemplates.Mosques.Cells
                    .Select(cell => new WorkbookCell(Main, cell.Cell))
                    .ToArray());

            return outcome.Formulas;
        }
    }
}
