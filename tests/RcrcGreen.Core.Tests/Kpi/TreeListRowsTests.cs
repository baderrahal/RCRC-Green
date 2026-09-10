using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The tree list had three row ranges and the tool trusted the shortest. Measured on the
    /// MOSQUES workbook the first twenty plot run wrote, 2026-09-10: the map said B4 to B83,
    /// the sheet's total said SUM(B4:B92), and the botanical names ran from row 4 to row 101,
    /// 98 of them. Four species with a row waiting past 83 were reported as having nowhere to
    /// go, and rows 93 to 101 sit past the total's reach.
    ///
    /// Every row number and every count here is written out by hand off that measurement. The
    /// workbook itself is not in this repository and its names are not in this file: the five
    /// species the run named are at the rows the run measured them on, and every other row
    /// carries a made up name.
    /// </summary>
    public class TreeListRowsTests
    {
        private const string Conocarpus = "Conocarpus lancifolius";

        private const string Phoenix = "Phoenix dactylifera";

        private const string Washingtonia = "Washingtonia robusta";

        private const string Ficus = "Ficus benjamina";

        private const string Prosopis = "Prosopis juliflora";

        /// <summary>
        /// Tree List - Existing as measured: names on rows 4 to 101, the total at B93 reaching
        /// rows 4 to 92.
        /// </summary>
        private static WorkbookFixture.TreeSheetShape MeasuredExisting()
        {
            return new WorkbookFixture.TreeSheetShape(
                KpiTemplates.ExistingTreesSheet,
                Named(4, 101, new Dictionary<int, string>
                {
                    { 84, Conocarpus },
                    { 86, Phoenix },
                    { 87, Washingtonia },
                    { 89, Ficus },
                    { 99, Prosopis }
                }),
                4, 92, 93);
        }

        /// <summary>
        /// Tree List - Proposed as measured: names on rows 4 to 86, the same total.
        /// </summary>
        private static WorkbookFixture.TreeSheetShape MeasuredProposed()
        {
            return new WorkbookFixture.TreeSheetShape(
                KpiTemplates.ProposedTreesSheet, Named(4, 86, new Dictionary<int, string>()), 4, 92, 93);
        }

        private static IDictionary<int, string> Named(int first, int last, IDictionary<int, string> real)
        {
            var names = new Dictionary<int, string>();
            for (int row = first; row <= last; row++)
            {
                names[row] = real.ContainsKey(row) ? real[row] : "Made up tree " + row;
            }

            return names;
        }

        private static string Measured(string folder)
        {
            return WorkbookFixture.TreeLists(folder, MeasuredExisting(), MeasuredProposed());
        }

        [Fact]
        public void TheExistingListIsReadToRow101AndItsTotalReachesRow92()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                SpeciesList list = SpeciesList.In(Measured(folder), KpiTemplates.Mosques.ExistingTrees);

                Assert.True(list.WasRead, list.Refusal);
                Assert.Equal(98, list.Rows.Count);
                Assert.Equal(4, list.Rows[0].Row);
                Assert.Equal(101, list.Rows[97].Row);
                Assert.Equal(102, list.FirstGapRow);
                Assert.Empty(list.BelowTheList);

                Assert.True(list.TotalFound);
                Assert.Equal(4, list.TotalFirstRow);
                Assert.Equal(92, list.TotalLastRow);
                Assert.Equal("B93", list.TotalCell);
                Assert.Equal("SUM(B4:B92) at B93", list.TotalInWords);

                // Every row the total reaches is named, so there is nowhere to write a species
                // the list does not hold.
                Assert.Empty(list.EmptyRows);

                // Rows 93 to 101, nine species the total never adds.
                Assert.Equal(new[] { 93, 94, 95, 96, 97, 98, 99, 100, 101 },
                    list.OutsideTheTotal.Select(one => one.Row));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [Fact]
        public void TheProposedListIsReadToRow86AndHasSixEmptyRowsTheTotalReaches()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                SpeciesList list = SpeciesList.In(Measured(folder), KpiTemplates.Mosques.ProposedTrees);

                Assert.Equal(83, list.Rows.Count);
                Assert.Equal(4, list.Rows[0].Row);
                Assert.Equal(86, list.Rows[82].Row);
                Assert.Equal(87, list.FirstGapRow);
                Assert.Equal(new[] { 87, 88, 89, 90, 91, 92 }, list.EmptyRows);
                Assert.Empty(list.OutsideTheTotal);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The header at D3 is not a species and is not read as one, whatever it says.
        /// </summary>
        [Fact]
        public void TheHeaderRowIsNotAName()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                SpeciesList list = SpeciesList.In(Measured(folder), KpiTemplates.Mosques.ExistingTrees);

                Assert.DoesNotContain(list.Rows, one => one.Row == 3);
                Assert.DoesNotContain(list.Rows, one => one.BotanicalName == "BOTANICAL NAME");
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The four the run reported as having nowhere to go, and the fifth on a row the total
        /// does not reach. Counts as the run measured them: 17, 27, 19, 3 and 3, with UNKNOWN 16
        /// genuinely absent from the list.
        /// </summary>
        [Fact]
        public void TheFiveSpeciesPastRow83MatchTheirRowsAndUnknownIsStillAbsent()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = Measured(folder);
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

                IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                    new[]
                    {
                        Existing("CONOCARPUS LANCIFOLIUS", 17),
                        Existing("PHOENIX DACTYLIFERA", 27),
                        Existing("WASHINGTONIA ROBUSTA", 19),
                        Existing("FICUS BENJAMINA", 3),
                        Existing("PROSOPIS JULIFLORA", 3),
                        Existing("UNKNOWN", 16)
                    },
                    KpiTemplates.Mosques, existing, proposed);

                Assert.Equal(6, matches.Count);

                AssertMatchedOn(matches[0], 84, Conocarpus);
                AssertMatchedOn(matches[1], 86, Phoenix);
                AssertMatchedOn(matches[2], 87, Washingtonia);
                AssertMatchedOn(matches[3], 89, Ficus);

                // Row 99 is past SUM(B4:B92). The row is kept, nothing is written there, and
                // the reason names both.
                SpeciesMatch prosopis = matches[4];
                Assert.False(prosopis.Placed);
                Assert.False(prosopis.Matched);
                Assert.True(prosopis.NotReachedByTheTotal);
                Assert.Equal(99, prosopis.Row);
                Assert.Equal(Prosopis, prosopis.WorkbookName);
                Assert.Equal(
                    "the workbook holds this name on row 99 and the sheet's total, SUM(B4:B92) at B93, "
                    + "reaches rows 4 to 92 and not that one, so the count was not written where no total would add it",
                    prosopis.Why);

                // Absent, and no empty row the total reaches to write it into.
                SpeciesMatch unknown = matches[5];
                Assert.False(unknown.Placed);
                Assert.False(unknown.NotReachedByTheTotal);
                Assert.Equal(0, unknown.Row);
                Assert.Equal(SpeciesMatching.NoEmptyRowLeft, unknown.Why);

                // 17 + 27 + 19 + 3, the trees that reach the total.
                Assert.Equal(66, matches.Where(one => one.Placed).Sum(one => one.Species.Quantity));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The plan writes the four and names the fifth with its cell, B99, so the line points
        /// at where the count was not put.
        /// </summary>
        [Fact]
        public void ThePlanWritesTheFourReachedRowsAndNamesTheCellOfTheFifth()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = Measured(folder);
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

                KpiCreatePlan plan = KpiCreatePlan.Of(
                    KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                    SpeciesMatching.Against(
                        new[]
                        {
                            Existing("CONOCARPUS LANCIFOLIUS", 17),
                            Existing("PHOENIX DACTYLIFERA", 27),
                            Existing("WASHINGTONIA ROBUSTA", 19),
                            Existing("FICUS BENJAMINA", 3),
                            Existing("PROSOPIS JULIFLORA", 3)
                        },
                        KpiTemplates.Mosques, existing, proposed),
                    null, null, null);

                List<CellWrite> onTheSheet = plan.Writes
                    .Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet)
                    .ToList();

                Assert.Equal(new[] { "B84", "B86", "B87", "B89" }, onTheSheet.Select(one => one.Cell.ToString()));
                Assert.Equal(new[] { "17", "27", "19", "3" }, onTheSheet.Select(one => one.Stored));

                NotWritten prosopis = Assert.Single(plan.Skipped.Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet));
                Assert.Equal("B99", prosopis.Cell);
                Assert.Equal("PROSOPIS JULIFLORA 3 under Existing", prosopis.What);
                Assert.Contains("row 99", prosopis.Why);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// A species the proposed list does not hold goes into row 87, the first row past the
        /// names that the total still reaches. The map used to say the list ended at 83.
        /// </summary>
        [Fact]
        public void AnUnmatchedProposedSpeciesGoesIntoRow87()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = Measured(folder);
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

                IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                    new[]
                    {
                        new MergedSpecies("UNKNOWN", CreateFixture.Proposed, new[] { new PlotNumber("FM-05", 2) }),
                        new MergedSpecies("SHRUBS: BOUGAINVILLEA GLABRA", CreateFixture.Proposed, new[] { new PlotNumber("FM-05", 4) })
                    },
                    KpiTemplates.Mosques, existing, proposed);

                Assert.Equal(new[] { 87, 88 }, matches.Select(one => one.Row));
                Assert.All(matches, one => Assert.True(one.Added));
                Assert.All(matches, one => Assert.True(one.Placed));
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The report prints both lists as read, so the next disagreement between the names and
        /// the total is read off the report rather than found in a short workbook.
        /// </summary>
        [Fact]
        public void TheReportPrintsBothListsAsReadOffTheFile()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = Measured(folder);
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(path, KpiTemplates.Mosques.ProposedTrees);

                SpeciesMatch[] matches = SpeciesMatching.Against(
                    new[] { Existing("PROSOPIS JULIFLORA", 3), Existing("FICUS BENJAMINA", 3) },
                    KpiTemplates.Mosques, existing, proposed).ToArray();

                string report = KpiCreateReport.Write(
                    CreateFixture.Run(matches: matches, existingList: existing, proposedList: proposed),
                    new System.DateTime(2026, 9, 10, 11, 16, 0));

                Assert.Contains("== THE WORKBOOK'S OWN TREE LISTS (2) ==", report);
                Assert.Contains("read off the template when Create was pressed, never off a row range in this tool", report);
                Assert.Contains("  Tree List - Existing\r\n    botanical names in column D: 98 names, rows 4 to 101\r\n"
                    + "    the quantity total: SUM(B4:B92) at B93, reaching rows 4 to 92\r\n"
                    + "    empty rows the total reaches, for a species the list does not hold: 0\r\n"
                    + "    NAMES THE TOTAL DOES NOT REACH: 9, rows 93 to 101. A count written there would never reach the total, so a species matching\r\n", report);
                Assert.Contains("  Tree List - Proposed\r\n    botanical names in column D: 83 names, rows 4 to 86\r\n"
                    + "    the quantity total: SUM(B4:B92) at B93, reaching rows 4 to 92\r\n"
                    + "    empty rows the total reaches, for a species the list does not hold: 6, rows 87 to 92\r\n", report);

                Assert.Contains("== SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH (1) ==", report);
                Assert.Contains("  Tree List - Existing | 99 | Prosopis juliflora | PROSOPIS JULIFLORA | Existing | 3 | DM-11 3 | "
                    + "the workbook holds this name on row 99", report);
                Assert.Contains("== SPECIES MATCHED (1) ==", report);
                Assert.Contains("  Tree List - Existing | B89 | Ficus benjamina | FICUS BENJAMINA | Existing | 3 | DM-11 3", report);
                Assert.Contains("== SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT (0) ==", report);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        [Fact]
        public void TheReportSaysTheListsWereNotReadWhenTheAccountingRefused()
        {
            string report = KpiCreateReport.Write(CreateFixture.Run(), new System.DateTime(2026, 9, 10));

            Assert.Contains("  Tree List - Existing\r\n    " + KpiCreateReport.ListsNotRead + "\r\n", report);
            Assert.Contains("  Tree List - Proposed\r\n    " + KpiCreateReport.ListsNotRead + "\r\n", report);
        }

        /// <summary>
        /// With no SUM over column B on the sheet nothing says which rows a count reaches, so a
        /// matched name and an unmatched one are both refused with the row named where there
        /// is one.
        /// </summary>
        [Fact]
        public void ASheetWithNoTotalWritesNothingAndSaysWhy()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                var noTotal = new WorkbookFixture.TreeSheetShape(
                    KpiTemplates.ExistingTreesSheet,
                    new Dictionary<int, string> { { 4, "Albizia lebbeck" }, { 5, "Cassia glauca" } },
                    0, 0, 0);
                string path = WorkbookFixture.TreeLists(folder, noTotal, MeasuredProposed());
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);

                Assert.False(existing.TotalFound);
                Assert.Equal(2, existing.Rows.Count);
                Assert.Empty(existing.EmptyRows);
                Assert.Empty(existing.OutsideTheTotal);
                Assert.Equal(SpeciesList.NoTotalFound, existing.TotalInWords);

                IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                    new[] { Existing("ALBIZIA LEBBECK", 1), Existing("UNKNOWN", 2) },
                    KpiTemplates.Mosques, existing, existing);

                Assert.True(matches[0].NotReachedByTheTotal);
                Assert.Equal(4, matches[0].Row);
                Assert.False(matches[0].Placed);
                Assert.Equal(
                    "no cell in column B holding SUM over column B was found on the sheet, so nothing says "
                    + "which rows a count reaches and it was not written",
                    matches[0].Why);

                Assert.False(matches[1].Placed);
                Assert.Equal(0, matches[1].Row);
                Assert.Equal(matches[0].Why, matches[1].Why);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The list stops at the first empty row. A name further down is not the list, is not
        /// matched against, and a species carrying it is refused by name rather than written in
        /// a second time above it. Nothing measured holds such a row, so the shape is stated
        /// here rather than assumed.
        /// </summary>
        [Fact]
        public void ANameBelowTheFirstEmptyRowIsNotTheListAndIsNamed()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                var gapped = new WorkbookFixture.TreeSheetShape(
                    KpiTemplates.ExistingTreesSheet,
                    new Dictionary<int, string>
                    {
                        { 4, "Albizia lebbeck" }, { 5, "Cassia glauca" }, { 6, "Ficus benjamina" }, { 8, "Ziziphus spina-christi" }
                    },
                    4, 10, 11);
                string path = WorkbookFixture.TreeLists(folder, gapped, MeasuredProposed());
                SpeciesList existing = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);

                Assert.Equal(new[] { 4, 5, 6 }, existing.Rows.Select(one => one.Row));
                Assert.Equal(7, existing.FirstGapRow);
                Assert.Equal(8, Assert.Single(existing.BelowTheList).Row);
                Assert.Equal(new[] { 7, 9, 10 }, existing.EmptyRows);
                Assert.Empty(existing.OutsideTheTotal);

                IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                    new[] { Existing("ZIZIPHUS SPINA-CHRISTI", 5), Existing("UNKNOWN", 2) },
                    KpiTemplates.Mosques, existing, existing);

                Assert.False(matches[0].Placed);
                Assert.True(matches[0].NotReachedByTheTotal);
                Assert.Equal(8, matches[0].Row);
                Assert.Equal(
                    "the workbook holds this name on row 8, below the first empty row of its list at row 7, "
                    + "so it was neither matched nor written in a second time",
                    matches[0].Why);

                Assert.True(matches[1].Added);
                Assert.Equal(7, matches[1].Row);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// A name outside the total counts in the accounting the way a species written nowhere
        /// does: it is in CELLS NOT WRITTEN and not in the writes.
        /// </summary>
        [Fact]
        public void ANameOutsideTheTotalIsNotAmongTheWrites()
        {
            SpeciesList list = SpeciesList.Holding(
                new[] { new SpeciesListRow(4, "Albizia lebbeck"), new SpeciesListRow(5, "Cassia glauca") },
                4, 4, "B6");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(
                new[] { Existing("CASSIA GLAUCA", 4), Existing("ALBIZIA LEBBECK", 1) },
                KpiTemplates.Mosques, list, list);

            Assert.True(matches[0].NotReachedByTheTotal);
            Assert.Equal(5, matches[0].Row);
            Assert.True(matches[1].Matched);
            Assert.Equal(4, matches[1].Row);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null, matches, null, null, null);

            Assert.Single(plan.Writes.Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet));
            Assert.Equal("B5", Assert.Single(plan.Skipped.Where(one => one.SheetName == KpiTemplates.ExistingTreesSheet)).Cell);
        }

        private static MergedSpecies Existing(string name, int count)
        {
            return new MergedSpecies(name, CreateFixture.Existing, new[] { new PlotNumber("DM-11", count) });
        }

        private static void AssertMatchedOn(SpeciesMatch match, int row, string workbookName)
        {
            Assert.True(match.Matched, match.Species.BotanicalName + ": " + match.Why);
            Assert.Equal(row, match.Row);
            Assert.Equal(workbookName, match.WorkbookName);
            Assert.Equal(KpiTemplates.ExistingTreesSheet, match.SheetName);
        }
    }
}
