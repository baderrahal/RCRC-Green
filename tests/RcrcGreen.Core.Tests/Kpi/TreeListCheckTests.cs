using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **NONE OF THIS SHOWED UNTIL A PLOT HIT A BAD ROW.** Measured in the 16 September
    /// workbooks, after the team's template edits of 15 September: Tree List - Existing L85, L88
    /// and L90 to L101 still typed numbers in EXISTING PARKS and STREETS, M83 empty in EXISTING
    /// PARKS and FUTURE PARKS, O90 to O94 and O96 to O100 empty and N85 and N88 to N101 empty in
    /// all seven, MOSQUES L101 typed, Tree List - Proposed L84 to L92 deleted on both park
    /// templates, and the Native and Adaptive SUMIFs on the first tab stopping at row 91 where
    /// the list they count runs to 92.
    ///
    /// A press over 154 plots lands on a different set of those rows every time the model
    /// changes, so the team learnt about each one from a workbook that had already gone out.
    /// **THE CHECK READS BOTH TREE LISTS OF EACH TICKED TEMPLATE ONCE, AT THE PRESS, BEFORE ANY
    /// PLOT IS WRITTEN**, and names every cell. It is a report section and stops no write.
    ///
    /// **WHERE THIS FIXTURE DEPARTS FROM THE MEASURED SHAPE, AND WHY.** The round named row 84
    /// as both an empty row carrying no canopy formula and one of the two rows holding one name.
    /// A row cannot be both: <see cref="SpeciesList"/> reads the list down column D until the
    /// names stop, so a row holding a name is never an empty row. The duplicate keeps rows 22
    /// and 84 as measured and **the empty row moves to 93**, the first row past this list's last
    /// name. Every other cell is the measured one.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class TreeListCheckTests : IDisposable
    {
        private readonly string _folder = WorkbookFixture.Folder();

        private static readonly string Existing = KpiTemplates.ExistingTreesSheet;

        private static readonly string Proposed = KpiTemplates.ProposedTreesSheet;

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
        /// Rows 4 to 92 named with no gap, which is the real MOSQUES shape one list short: the
        /// names stop at 92 and the sheet runs to 101 with the total reaching all of it.
        /// </summary>
        private static WorkbookFixture.TreeRow[] NamedToRow92(bool oneNameTwice)
        {
            var rows = new List<WorkbookFixture.TreeRow>();
            for (int row = 4; row <= 92; row++)
            {
                string name = oneNameTwice && (row == 22 || row == 84)
                    ? "Albizia lebbeck"
                    : "Species " + row;

                rows.Add(new WorkbookFixture.TreeRow(row, name, "15", "8"));
            }

            return rows.ToArray();
        }

        private string Template(
            string fileName,
            bool oneNameTwice = false,
            int[] typedCanopyAt = null,
            int[] withoutCanopy = null,
            int[] withoutTotalCanopy = null,
            bool withWater = true,
            int[] withoutTotalWater = null,
            int[] withoutWaterPerTree = null,
            int sumifTo = 0)
        {
            return WorkbookFixture.Computing(
                _folder,
                NamedToRow92(oneNameTwice),
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: fileName,
                withGreenCoverLabel: true,
                withoutCanopy: withoutCanopy,
                withoutTotalCanopy: withoutTotalCanopy,
                typedCanopyAt: typedCanopyAt,
                withWater: withWater,
                withoutTotalWater: withoutTotalWater,
                withoutWaterPerTree: withoutWaterPerTree,
                sumifTo: sumifTo,
                lastRow: 101);
        }

        /// <summary>
        /// The one file every question fires on: L85 typed, M83 empty, O90 empty, N88 empty,
        /// row 93 empty and carrying neither formula, one name on rows 22 and 84, and a SUMIF
        /// on the first tab reading rows 3 to 91 of a list whose names reach 92.
        /// </summary>
        private string EveryFault(string fileName)
        {
            return Template(
                fileName,
                oneNameTwice: true,
                typedCanopyAt: new[] { 85 },
                withoutCanopy: new[] { 93 },
                withoutTotalCanopy: new[] { 83, 93 },
                withoutTotalWater: new[] { 90 },
                withoutWaterPerTree: new[] { 88 },
                sumifTo: 91);
        }

        private IReadOnlyList<TreeListSheetCheck> Checked(string path)
        {
            LabelledCells computed = LabelledPlaces.In(path, KpiTemplates.Mosques, ComputedPlaces.All);

            IReadOnlyList<TotalCanopyColumn> columns = TotalCanopyColumns.In(
                path, KpiTemplates.Mosques, computed.For(ComputedPlaces.GreenCoverName));

            SpeciesList existing = SpeciesList.In(
                path, KpiTemplates.Mosques.ExistingTrees, TotalCanopyColumns.For(columns, Existing));
            SpeciesList proposed = SpeciesList.In(
                path, KpiTemplates.Mosques.ProposedTrees, TotalCanopyColumns.For(columns, Proposed));

            Assert.True(existing.WasRead, existing.Refusal);
            Assert.True(proposed.WasRead, proposed.Refusal);

            return TreeListCheck.In(path, KpiTemplates.Mosques, columns, existing, proposed);
        }

        private static TreeListSheetCheck Sheet(IReadOnlyList<TreeListSheetCheck> checks, string sheetName)
        {
            return checks.Single(one => string.Equals(one.SheetName, sheetName, StringComparison.Ordinal));
        }

        /// <summary>
        /// **EVERY ONE OF THE SEVEN QUESTIONS NAMES ITS OWN CELL.** The cells are written out
        /// here one by one, in the order the check asks them, and none of them is worked out
        /// with the rule the check uses.
        /// </summary>
        [Fact]
        public void EveryQuestionNamesItsOwnCellOnTheSheetThatHoldsIt()
        {
            TreeListSheetCheck sheet = Sheet(Checked(EveryFault("faults.xlsx")), Existing);

            Assert.Equal(
                new[] { "L85", "M83", "N88", "O90", "B93", "D22 and D84", "<Mosques> D12" },
                sheet.Faults.Select(one => one.Cell).ToArray());

            Assert.Empty(sheet.NotRead);
        }

        /// <summary>
        /// **AND EACH ONE SAYS WHAT IS WRONG WITH THAT CELL.** A cell named with no reason sends
        /// somebody to open the file to find out what they are looking at.
        /// </summary>
        [Fact]
        public void EachNamedCellCarriesItsOwnReason()
        {
            TreeListSheetCheck sheet = Sheet(Checked(EveryFault("reasons.xlsx")), Existing);

            var why = sheet.Faults.ToDictionary(one => one.Cell, one => one.Why, StringComparer.Ordinal);

            // **THE TYPED NUMBER IS PRINTED.** 50 sitting in a canopy cell looks like an answer.
            Assert.Equal("it holds 50 and no formula", why["L85"]);

            // FP-18's row 83, one row up: the canopy per tree is computed and added nowhere.
            Assert.Equal("it is empty", why["M83"]);

            Assert.Equal("it is empty", why["O90"]);

            // **THE WATER PAIR IS READ OFF THE SHEET'S OWN FORMULAS**, so the reason can name
            // the cell that multiplies it. Nothing here was told that N and O are the water
            // columns.
            Assert.Equal(
                "it is empty, and it is the column O88 multiplies by the count",
                why["N88"]);

            Assert.Equal(
                "the total reaches this row and it carries neither the canopy formula nor the "
                + "total canopy formula, so no species the list does not hold can go in it",
                why["B93"]);

            Assert.Equal(
                "Albizia lebbeck is on 2 rows, so nothing can say which row a count belongs on",
                why["D22 and D84"]);

            Assert.Equal(
                "it reads H3 to H91 and this list names a species as far as row 92, so every "
                + "row past the range is left out of it",
                why["<Mosques> D12"]);
        }

        /// <summary>
        /// **THE SEVEN CATEGORIES ARE WHAT THE REPORT GROUPS ON**, so each fault carries which
        /// question named it rather than the reader reading its words.
        /// </summary>
        [Fact]
        public void EachFaultCarriesTheQuestionThatNamedIt()
        {
            TreeListSheetCheck sheet = Sheet(Checked(EveryFault("kinds.xlsx")), Existing);

            var kinds = sheet.Faults.ToDictionary(one => one.Cell, one => one.Kind, StringComparer.Ordinal);

            Assert.Equal("the canopy cell is typed or missing", kinds["L85"]);
            Assert.Equal("the total canopy cell is missing", kinds["M83"]);
            Assert.Equal("the total water cell is missing", kinds["O90"]);
            Assert.Equal("the water per tree cell is empty", kinds["N88"]);
            Assert.Equal("an empty row cannot take a new species", kinds["B93"]);
            Assert.Equal("this name is on more than one row", kinds["D22 and D84"]);
            Assert.Equal(
                "a formula reads a range that stops before the list ends",
                kinds["<Mosques> D12"]);
        }

        /// <summary>
        /// **THE SECOND SHEET IS ANSWERED ON ITS OWN.** The fixture builds both tree lists from
        /// one shape, so the rows missing a formula are missing on both, and the proposed sheet
        /// names three empty rows a new species cannot go into and nothing else. Its own list
        /// holds one name, so no row of it reaches the other five questions, and the SUMIF on
        /// the first tab reads the EXISTING sheet and is not this sheet's.
        /// </summary>
        [Fact]
        public void TheProposedSheetNamesItsOwnCellsAndNotTheOtherSheets()
        {
            TreeListSheetCheck sheet = Sheet(Checked(EveryFault("second.xlsx")), Proposed);

            Assert.Equal(
                new[] { "B83", "B85", "B93" },
                sheet.Faults.Select(one => one.Cell).ToArray());

            Assert.Empty(sheet.NotRead);
        }

        /// <summary>
        /// **A CLEAN TEMPLATE READS CLEAN**, which is the half that says the check is not simply
        /// finding something on every file it opens.
        /// </summary>
        [Fact]
        public void ACleanTemplateReadsCleanOnBothTreeLists()
        {
            IReadOnlyList<TreeListSheetCheck> checks = Checked(Template("clean.xlsx"));

            Assert.Equal(new[] { Existing, Proposed }, checks.Select(one => one.SheetName).ToArray());

            foreach (TreeListSheetCheck sheet in checks)
            {
                Assert.True(
                    sheet.Clean,
                    sheet.SheetName + " read " + sheet.Faults.Count + " faults and "
                    + sheet.NotRead.Count + " unmade checks on a file built with none: "
                    + string.Join("; ", sheet.Faults.Select(one => one.InWords)
                        .Concat(sheet.NotRead).ToArray()));
            }

            Assert.Equal("Tree List - Existing: clean", checks[0].InWords);
            Assert.Equal("MOSQUES: clean.", TreeListCheck.InWords("MOSQUES", checks));
        }

        /// <summary>
        /// **A COLUMN THAT CANNOT BE READ IS NAMED AS NOT READ, NEVER SKIPPED IN SILENCE.** A
        /// sheet whose formulas hold no second product column has no water pair to check, and a
        /// check nobody made reads exactly like a check that passed.
        /// </summary>
        [Fact]
        public void ASheetWithNoWaterPairSaysTheCheckCouldNotBeMade()
        {
            IReadOnlyList<TreeListSheetCheck> checks = Checked(Template("nowater.xlsx", withWater: false));

            TreeListSheetCheck sheet = Sheet(checks, Existing);

            Assert.Empty(sheet.Faults);
            Assert.False(sheet.Clean, "a sheet whose water pair was never read came back clean");

            Assert.Equal(
                new[]
                {
                    "no column on this sheet multiplies another column by the count except the "
                    + "canopy, so the water pair could not be read and no row's water cells were "
                    + "checked"
                },
                sheet.NotRead.ToArray());

            Assert.Equal(
                "Tree List - Existing: no cell named, and 1 check could not be made",
                sheet.InWords);

            Assert.Equal(
                "MOSQUES: no cell named, and 2 checks could not be made.",
                TreeListCheck.InWords("MOSQUES", checks));
        }

        /// <summary>
        /// **A FILE THAT COULD NOT BE OPENED IS NOT A CLEAN TEMPLATE.** Both sheets come back
        /// naming the read that did not happen, which is the rule the KPI scan's own READS THAT
        /// DID NOT HAPPEN already follows.
        /// </summary>
        [Fact]
        public void ATemplateThatCannotBeOpenedNamesTheReadRatherThanReadingClean()
        {
            IReadOnlyList<TreeListSheetCheck> checks = TreeListCheck.In(
                Path.Combine(_folder, "nothing-here.xlsx"),
                KpiTemplates.Mosques,
                new List<TotalCanopyColumn>(),
                null,
                null);

            Assert.Equal(new[] { Existing, Proposed }, checks.Select(one => one.SheetName).ToArray());

            foreach (TreeListSheetCheck sheet in checks)
            {
                Assert.False(sheet.Clean);
                Assert.Empty(sheet.Faults);
                Assert.Single(sheet.NotRead);
                Assert.Contains("nothing-here.xlsx could not be opened", sheet.NotRead[0]);
            }
        }

        /// <summary>
        /// **A SECTION BUILT AND PRINTED NOWHERE IS A SECTION NOBODY WROTE.** This repository has
        /// paid for that once already, with `TickingATemplate.SomeOfThem`, which was green and
        /// had never reached a screen. So the check is driven through the real report writer.
        /// </summary>
        [Fact]
        public void TheReportPrintsTheSectionAndTheGlanceCarriesTheTemplatesLine()
        {
            IReadOnlyList<TreeListSheetCheck> checks = Checked(EveryFault("report.xlsx"));

            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                new TemplateSplit(
                    new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                null,
                null,
                null,
                null,
                null,
                checks);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 16, 17, 4, 0));

            // The heading carries its own count, which is every other heading's rule in this
            // report, so a section that found nothing reads differently from one nobody filled in.
            Assert.Contains("THE TEMPLATES' OWN TREE LISTS, CELL BY CELL (10)", report);

            // The glance line, above the section, one per template.
            Assert.Contains("MOSQUES: 10 cells named.", report);

            // And the cells themselves, under the question that named each.
            Assert.Contains("MOSQUES, Tree List - Existing: 7 cells named", report);
            Assert.Contains("    the canopy cell is typed or missing, 1:", report);
            Assert.Contains("      L85: it holds 50 and no formula", report);
            Assert.Contains("      M83: it is empty", report);
        }

        /// <summary>
        /// **THE GLANCE CARRIES ONE LINE PER TEMPLATE AND NEVER ONE PER CELL.** Over seven
        /// templates and 154 plots a line per cell is a page nobody reads, and the cells are in
        /// the section below it.
        /// </summary>
        [Fact]
        public void TheGlanceLineCountsTheCellsOverBothSheetsOfOneTemplate()
        {
            IReadOnlyList<TreeListSheetCheck> checks = Checked(EveryFault("glance.xlsx"));

            // Seven on Tree List - Existing and three on Tree List - Proposed.
            Assert.Equal("MOSQUES: 10 cells named.", TreeListCheck.InWords("MOSQUES", checks));

            Assert.Equal(
                "MOSQUES: its tree lists were not read.",
                TreeListCheck.InWords("MOSQUES", new List<TreeListSheetCheck>()));
        }
    }
}
