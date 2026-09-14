using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The guard the round before could not build, on the two cells the WORKBOOK computes.
    ///
    /// **MEASURED BY BADER ON ALL SEVEN TEMPLATES, 14 September**, which is what closed it:
    ///
    /// <code>
    /// TOTAL GREEN COVER
    ///   EXISTING PARKS, FUTURE PARKS, STREETS    D9 = F9+F11+H11
    ///   HEALTHCARE, MOSQUES, PARKING, SCHOOLS    D8 = F8+F10+H10
    ///
    /// PERCENTAGE CANOPY, in section 3
    ///   HEALTHCARE, MOSQUES, PARKING, SCHOOLS    label C31, value E31 = IF(Area&lt;1," ",F8/Area)
    ///   STREETS                                  label C32, value E32 = IF(Area&lt;1," ",F9/Area)
    ///   EXISTING PARKS and FUTURE PARKS          NO SUCH LABEL AT ALL
    /// </code>
    ///
    /// The workbook built here is the MOSQUES shape, D8 and E31 with the labels at C8 and C31.
    /// **Nothing here names a letter to find a cell**: the two row layouts are exactly why both
    /// are found by their labels, the same lesson row 7 and row 5 already taught.
    /// </summary>
    public class ComputedCellTests : IDisposable
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
        /// A template of the MOSQUES shape, patched, with the labels present unless a case asks
        /// for one to be missing and the green cover formula drifted where one asks for that.
        /// </summary>
        private Built Build(
            bool greenCoverLabel = true,
            bool percentageLabel = true,
            string greenCoverFormula = null,
            string name = "one")
        {
            string template = WorkbookFixture.Computing(
                _folder,
                new WorkbookFixture.TreeRow[0],
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                fileName: "MOSQUES-" + name + ".xlsx",
                withGreenCoverLabel: greenCoverLabel,
                withPercentageLabel: percentageLabel,
                greenCoverFormula: greenCoverFormula);

            PatchOutcome outcome = WorkbookPatcher.Patch(
                template, Path.Combine(_folder, "filled-" + name + ".xlsx"),
                new[]
                {
                    CellWrite.Number(Main, "H7", 62023),
                    CellWrite.Text(Proposed, "D7", "BAUHINIA PURPUREA"),
                    CellWrite.Number(Proposed, "B7", 19),
                    CellWrite.Number(Proposed, "I7", 6),
                    CellWrite.Number(Proposed, "J7", 5)
                },
                KpiTemplates.Mosques.Cells.Select(cell => new WorkbookCell(Main, cell.Cell)).ToArray());

            return new Built(outcome, LabelledPlaces.In(template, KpiTemplates.Mosques, ComputedPlaces.All));
        }

        private sealed class Built
        {
            public Built(PatchOutcome outcome, LabelledCells computed)
            {
                Outcome = outcome;
                Computed = computed;
            }

            public PatchOutcome Outcome { get; }

            public LabelledCells Computed { get; }

            public SummaryCellCheck GreenCover()
            {
                return WorkbookArithmetic.GreenCoverCell(
                    Outcome.Formulas, Main, Computed.For(ComputedPlaces.GreenCoverName), "F10", "H10");
            }

            public SummaryCellCheck Percentage(string canopyCell)
            {
                return WorkbookArithmetic.PercentageCell(
                    Outcome.Formulas, Main, Computed.For(ComputedPlaces.PercentageName), canopyCell, "H7");
            }
        }

        /// <summary>
        /// **The label finds the cell and the cell holds this tool's own sum.** D8 reads
        /// F8+F10+H10, which is a canopy cell plus the map's planting cell F10 plus its lawn cell
        /// H10, so the third term F8 IS the canopy cell and is learnt rather than written in.
        /// </summary>
        [Fact]
        public void TheGreenCoverCellIsFoundByItsLabelAndNamesTheCanopyCell()
        {
            SummaryCellCheck found = Build().GreenCover();

            Assert.True(found.Agrees, found.Why);
            Assert.Equal("D8", found.Cell);
            Assert.Equal("F8+F10+H10", found.Formula);
            Assert.Equal("F8", found.CanopyCell);
            Assert.Equal(
                "Total Green cover: D8 reads F8+F10+H10, which is this tool's own sum",
                found.InWords);
        }

        /// <summary>
        /// **A green cover cell that has drifted blanks the field and names the cell and what its
        /// formula now says.** A sum short of the lawn is the case: the tool would write a number
        /// the workbook beside it does not compute.
        /// </summary>
        [Fact]
        public void AGreenCoverCellThatHasDriftedIsNamedAndWritesNothing()
        {
            SummaryCellCheck drifted = Build(greenCoverFormula: "F8+F10", name: "short").GreenCover();

            Assert.True(drifted.Checked);
            Assert.False(drifted.Agrees);
            Assert.Equal(
                "the workbook's Total Green cover is not the sum this tool works out. "
                + "<Mosques> D8 reads F8+F10, which takes F8, F10, where this tool adds a canopy "
                + "cell, the planting cell F10 and the lawn cell H10",
                drifted.Why);

            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot("EP-05", uid2: "ANH-006-NP-100002"),
                CountedGroups.Of(KpiTemplates.ExistingParks), null, new DateTime(2026, 9, 14), true,
                new PdfWorkbookNumbers(
                    CanopyArea.Of(new[] { new CanopyRow(Proposed, 7, "BAUHINIA PURPUREA", 19, 5.0) }),
                    410.0, 60.0, 771.0,
                    ArithmeticCheck.Agreeing(null), drifted,
                    SummaryCellCheck.WithNothingToCheck(
                        ComputedPlaces.PercentageName, WorkbookArithmetic.TheWorkbookHasNoPercentageCell)));

            Assert.Equal(drifted.Why,
                plan.Fields.Single(one => one.Value == PdfValue.TotalAreasToBeGreened).Why);
        }

        /// <summary>
        /// **A template naming the label nowhere writes nothing and says so**, because the label
        /// is the tool's only route to the cell and all seven were measured to carry it. The
        /// reason names the sheet it looked on and says what it could not then do.
        /// </summary>
        [Fact]
        public void ATemplateWithNoGreenCoverLabelWritesNothingAndSaysWhy()
        {
            SummaryCellCheck found = Build(greenCoverLabel: false, name: "nolabel").GreenCover();

            Assert.False(found.Agrees);
            Assert.False(found.NothingToCheck);
            Assert.Contains(
                "no cell on <Mosques> reads " + ComputedPlaces.GreenCoverLabel,
                found.Why);
            Assert.EndsWith(WorkbookArithmetic.NoGreenCoverLabel, found.Why, StringComparison.Ordinal);
        }

        /// <summary>
        /// **The percentage cell sits TWO columns right of its label, not one.** The label at C31
        /// reaches E31, and the cell one to the right is empty on all five templates that carry
        /// it. Its formula reads the canopy cell and whatever the defined name Area points at, so
        /// the name is checked as well as the cell.
        /// </summary>
        [Fact]
        public void ThePercentageCellSitsTwoColumnsRightAndDividesTheCanopyByTheArea()
        {
            Built built = Build(name: "percent");

            SummaryCellCheck found = built.Percentage(built.GreenCover().CanopyCell);

            Assert.True(found.Agrees, found.Why);
            Assert.Equal("E31", found.Cell);
            Assert.Equal("IF(Area<1,\" \",F8/Area)", found.Formula);
        }

        /// <summary>
        /// **An absence is not a drift.** EXISTING PARKS and FUTURE PARKS carry no canopy
        /// percentage cell at all, and the Parks PDF is the one form that asks for the number, so
        /// this is what every run of the tool meets today. The number is still written and the
        /// report says the workbook has no cell to hold it against.
        /// </summary>
        [Fact]
        public void ATemplateWithNoPercentageCellHasNothingToCheckAndTheNumberIsStillWritten()
        {
            Built built = Build(percentageLabel: false, name: "nopercent");

            SummaryCellCheck found = built.Percentage(built.GreenCover().CanopyCell);

            Assert.False(found.Checked);
            Assert.True(found.NothingToCheck);
            Assert.True(found.Usable);
            Assert.Equal(WorkbookArithmetic.TheWorkbookHasNoPercentageCell, found.Why);

            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot("EP-05", uid2: "ANH-006-NP-100002"),
                CountedGroups.Of(KpiTemplates.ExistingParks), null, new DateTime(2026, 9, 14), true,
                new PdfWorkbookNumbers(
                    CanopyArea.Of(new[] { new CanopyRow(Proposed, 4, "ALBIZIA LEBBECK", 11, 8.0) }),
                    0.0, 0.0, 771.0,
                    ArithmeticCheck.Agreeing(null), built.GreenCover(), found));

            Assert.Equal("71.34", plan.Fields.Single(one => one.Value == PdfValue.PercentageCanopy).Text);

            // And the report says which cell each was held against, so a computed number never
            // stands on nothing.
            Assert.Contains(
                "Percentage canopy: " + WorkbookArithmetic.TheWorkbookHasNoPercentageCell,
                plan.WhatWasChecked);
        }

        /// <summary>
        /// **The two computed places are read only and are NOT in the table the plan writes
        /// from.** A cell holding the client's own formula must never take a value, and keeping
        /// them out of LabelledPlaces.All is the one thing that makes it impossible.
        /// </summary>
        [Fact]
        public void TheComputedPlacesAreNeverInTheTableThatGetsWritten()
        {
            foreach (LabelledPlace place in ComputedPlaces.All)
            {
                Assert.DoesNotContain(LabelledPlaces.All, one =>
                    string.Equals(one.Name, place.Name, StringComparison.OrdinalIgnoreCase));
            }

            Assert.Equal(2, ComputedPlaces.All.Count);
            Assert.Equal(1, ComputedPlaces.All.Single(one => one.Name == ComputedPlaces.GreenCoverName).StepsRight);
            Assert.Equal(2, ComputedPlaces.All.Single(one => one.Name == ComputedPlaces.PercentageName).StepsRight);
        }

        /// <summary>
        /// **The label is not the one the client's note names.** The note says the cell to the
        /// right of `Total area covered by canopy`, and no template carries that label in section
        /// 1. The cell is in section 3 under `% of Total area covered by canopy`, two columns
        /// right. That is the second note on these forms measured to be wrong about its own
        /// subject, after the TOTAL Shrubs tooltip.
        /// </summary>
        [Fact]
        public void ThePercentageLabelIsNotTheOneTheNoteNames()
        {
            Assert.Equal("% of Total area covered by canopy", ComputedPlaces.PercentageLabel);
            Assert.NotEqual("Total area covered by canopy", ComputedPlaces.PercentageLabel);

            Assert.Contains(
                "Total area covered by canopy",
                PdfForms.Parks.FieldFor(PdfValue.PercentageCanopy).Note);
        }
    }
}
