using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// One word too loose, measured on the 1707 run. The tree list header row holds I Mature
    /// Height (m), J Average Mature Canopy Diameter (m) and K Mature Canopy Diameter (m). Both
    /// J and K hold DIAMETER, so the reader found two, refused to choose and wrote nothing into
    /// J. L84 reads J84, returned a space, M84 went #VALUE!, and the canopy guard deleted the
    /// output. Correct at every step, and the cause was one column choice.
    ///
    /// J is the column the workbook computes from: L reads J and nothing reads K. So where more
    /// than one column holds the word, the one the sheet's own formulas read is taken, worked
    /// out from the file and never from a letter. Where that still leaves more than one, or
    /// none, nothing is written and both are named.
    ///
    /// Every expected value is written out by hand. The fixture's lists run rows 4 to 9 with
    /// the total on row 10, so what lands on I84 and J84 on the real sheet lands on I5 and J5
    /// here.
    /// </summary>
    public class DiameterColumnTests
    {
        private const string Existing = KpiTemplates.ExistingTreesSheet;

        private const string SecondHeading = "Mature Canopy Diameter (m)";

        private static WorkbookFixture.TreeRow[] Albizia()
        {
            return new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") };
        }

        /// <summary>
        /// J and K both hold DIAMETER, L reads J, so J is the column and the choice is recorded.
        /// </summary>
        [Fact]
        public void OfTwoColumnsHoldingDiameterTheOneTheFormulasReadIsTaken()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    secondDiameterHeading: SecondHeading);

                SpeciesList list = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);

                Assert.True(list.WasRead, list.Refusal);
                Assert.Equal("J", list.DiameterColumn);
                Assert.Equal(string.Empty, list.WhyNoDiameterColumn);
                Assert.Equal("of J, K holding DIAMETER, the one the sheet's own formulas read", list.DiameterColumnChosen);
                Assert.Equal("I", list.HeightColumn);
                Assert.Equal("the one column of the header row holding HEIGHT", list.HeightColumnChosen);
                Assert.Equal("8", list.Rows.Single().Diameter);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// Two columns and formulas reading neither: nothing is chosen, both are named, and the
        /// reason says what the formulas read. Never the first one.
        /// </summary>
        [Fact]
        public void TwoColumnsTheFormulasDoNotReadStayUnchosenAndBothAreNamed()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    secondDiameterHeading: SecondHeading, canopyReads: "P");

                SpeciesList list = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);

                Assert.Equal(string.Empty, list.DiameterColumn);
                Assert.Equal(string.Empty, list.DiameterColumnChosen);
                Assert.Equal(
                    "the sheet's header row, row 3, names 2 columns holding DIAMETER, J, K, and its own formulas read none of them, so nothing says which",
                    list.WhyNoDiameterColumn);
                Assert.Equal("I", list.HeightColumn);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// Two columns and formulas reading both: still nothing says which, and the reason says
        /// both are read.
        /// </summary>
        [Fact]
        public void TwoColumnsTheFormulasBothReadStayUnchosen()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    secondDiameterHeading: SecondHeading, alsoReads: "K");

                SpeciesList list = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);

                Assert.Equal(string.Empty, list.DiameterColumn);
                Assert.Equal(
                    "the sheet's header row, row 3, names 2 columns holding DIAMETER, J, K, and its own formulas read both of them, so nothing says which",
                    list.WhyNoDiameterColumn);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// One column holding the word is taken as before, and a header naming none refuses
        /// as before, with no formula clause because there was nothing to choose between.
        /// </summary>
        [Fact]
        public void OneColumnIsTakenAndNoneIsRefusedAsBefore()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string one = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0]);
                SpeciesList single = SpeciesList.In(one, KpiTemplates.Mosques.ExistingTrees);
                Assert.Equal("J", single.DiameterColumn);
                Assert.Equal("the one column of the header row holding DIAMETER", single.DiameterColumnChosen);

                string none = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    "NONE.xlsx", heightHeading: "Size class", diameterHeading: "Spread class");
                SpeciesList unnamed = SpeciesList.In(none, KpiTemplates.Mosques.ExistingTrees);
                Assert.Equal(string.Empty, unnamed.DiameterColumn);
                Assert.Equal("the sheet's header row, row 3, names no column holding DIAMETER", unnamed.WhyNoDiameterColumn);
                Assert.Equal("the sheet's header row, row 3, names no column holding HEIGHT", unnamed.WhyNoHeightColumn);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The check asked for. On the two column sheet PHOENIX DACTYLIFERA writes 18 into the
        /// height column and 15 into J, and UNKNOWN writes neither, because DM-25 row 19 prints
        /// nothing for its height and 0 for its diameter. Then UNKNOWN's blank J still refuses
        /// the output through the canopy guard, so the guard keeps working.
        /// </summary>
        [Fact]
        public void PhoenixWritesEighteenAndFifteenAndUnknownTakesNoRowSoTheWorkbookLands()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string template = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    secondDiameterHeading: SecondHeading);
                SpeciesList existing = SpeciesList.In(template, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(template, KpiTemplates.Mosques.ProposedTrees);

                IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                    KpiMerge.Species(new[]
                    {
                        CreateFixture.Plot("DM-25", species: new[]
                        {
                            CreateFixture.Species("PHOENIX DACTYLIFERA", "Existing", 27, 18, "18", "15"),
                            CreateFixture.Species("UNKNOWN", "Existing", 16, 19, "-", "0")
                        })
                    }, CreateFixture.Counted),
                    KpiTemplates.Mosques, existing, proposed);

                KpiCreatePlan plan = KpiCreatePlan.Of(
                    KpiTemplates.Mosques, null, null, string.Empty, null, null, null, matches, null, null, null);

                // PHOENIX is sized and goes in whole. UNKNOWN is not sized, so it takes no row
                // at all and row 6 stays empty: no B6, no D6, and nothing for its measures.
                List<CellWrite> onTheSheet = plan.Writes.Where(one => one.SheetName == Existing).ToList();
                Assert.Equal(new[] { "B5", "D5", "I5", "J5" }, onTheSheet.Select(one => one.Cell.ToString()));
                Assert.Equal("18", onTheSheet.Single(one => one.Cell.ToString() == "I5").Stored);
                Assert.Equal("15", onTheSheet.Single(one => one.Cell.ToString() == "J5").Stored);
                Assert.DoesNotContain(onTheSheet, one => one.Cell.ToString().StartsWith("K", StringComparison.Ordinal));

                NotWritten skipped = Assert.Single(plan.Skipped.Where(one => one.SheetName == Existing));
                Assert.Equal(string.Empty, skipped.Cell);
                Assert.Equal("UNKNOWN 16 under Existing", skipped.What);
                Assert.StartsWith(
                    "the workbook's list does not hold this name, and a row written into an empty one carries only "
                    + "what the model prints, which is no canopy diameter a workbook can compute with, "
                    + "so no row was written: DM-25 row 19", skipped.Why, StringComparison.Ordinal);

                // The whole point of the round: the workbook lands, because nothing was written
                // that its own formulas cannot compute from. The guard is unchanged and finds
                // nothing to refuse.
                string output = Path.Combine(folder, "out.xlsx");
                PatchOutcome outcome = WorkbookPatcher.Patch(template, output, plan.Writes);

                Assert.True(outcome.Written, outcome.Refusal);
                Assert.True(File.Exists(output), "the output is written");
                Assert.False(outcome.Formulas.RefusesTheWrite);
                Assert.DoesNotContain(outcome.Formulas.AtRisk, one => one.FromWrittenRow);
                Assert.DoesNotContain(outcome.Formulas.AtRisk, one => one.SheetName == Existing && one.Cell == "L5" && one.IsAnError);
                Assert.DoesNotContain(outcome.Formulas.AtRisk, one => one.SheetName == Existing && one.Cell == "M5");
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The report says how each column was chosen beside the list it was read off.
        /// </summary>
        [Fact]
        public void TheReportSaysHowEachColumnWasChosen()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.Computing(folder, Albizia(), new WorkbookFixture.TreeRow[0],
                    secondDiameterHeading: SecondHeading);
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

                string report = KpiCreateReport.Write(
                    CreateFixture.Run(existingList: existing, proposedList: proposed), new DateTime(2026, 9, 10, 17, 7, 0));

                Assert.Contains(
                    "    height column: I, the one column of the header row holding HEIGHT\r\n"
                    + "    diameter column: J, of J, K holding DIAMETER, the one the sheet's own formulas read\r\n",
                    report);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }

    /// <summary>
    /// The 1707 run read 22 matched species whose height or diameter in Revit differs from the
    /// row the workbook holds, nearly every match. Both numbers stay named and nothing is
    /// changed, and one line says how many of the matches disagree so the size of it is
    /// visible without counting.
    /// </summary>
    public class MeasureDifferenceCountTests
    {
        /// <summary>
        /// Three matched, two differing, one of them in both measures: 2 of 3, and the section
        /// counts 3 lines because Phoenix differs twice.
        /// </summary>
        [Fact]
        public void TheLineCountsTheSpeciesThatDifferAgainstTheSpeciesMatched()
        {
            SpeciesList list = SpeciesList.Holding(new[]
            {
                new SpeciesListRow(4, "Phoenix dactylifera", "25", "8"),
                new SpeciesListRow(5, "Albizia lebbeck", "15", "8"),
                new SpeciesListRow(6, "Cassia glauca", "6", "5")
            }, 4, 9, "B10", "I", "J");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                SpeciesMatching.Against(
                    KpiMerge.Species(new[]
                    {
                        CreateFixture.Plot("FM-05", species: new[]
                        {
                            CreateFixture.Species("PHOENIX DACTYLIFERA", "Existing", 27, 4, "18", "15"),
                            CreateFixture.Species("ALBIZIA LEBBECK", "Existing", 10, 5, "12", "8"),
                            CreateFixture.Species("CASSIA GLAUCA", "Existing", 3, 6, "6", "5")
                        })
                    }, CreateFixture.Counted),
                    KpiTemplates.Mosques, list, list),
                null, null, null);

            Assert.Equal(3, plan.Differences.Count);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(matches: plan.Matches.ToArray(), existingList: list, proposedList: list),
                new DateTime(2026, 9, 10, 17, 7, 0));

            Assert.Contains(
                "== MATCHED SPECIES WHOSE HEIGHT OR DIAMETER IN REVIT DIFFERS FROM THE ROW'S (3) ==\r\n"
                + "named and CHANGED NOTHING, the client's row keeps its own number\r\n"
                + "  2 of 3 matched species differ in a height, a diameter or both, and nothing was changed on any row\r\n",
                report);
        }

        [Fact]
        public void NoMatchReadsNoughtOfNought()
        {
            string report = KpiCreateReport.Write(CreateFixture.Run(), new DateTime(2026, 9, 10, 17, 7, 0));

            Assert.Contains("  0 of 0 matched species differ in a height, a diameter or both, and nothing was changed on any row\r\n", report);
        }
    }
}
