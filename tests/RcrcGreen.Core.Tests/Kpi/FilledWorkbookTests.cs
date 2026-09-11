using System;
using System.Collections.Generic;
using System.IO;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 39 and the correction on it. Recognition is the first sheet's name, a filled
    /// MOSQUES output's first sheet is still Mosques, and writing into the templates folder is
    /// allowed, so the next redraw offered MOSQUES DM-12.xlsx as a template beside the client's.
    ///
    /// **The first test for it was one cell against one string and it was wrong.** It read a
    /// workbook as filled when E5 held anything other than the angle bracketed placeholder. Two
    /// measured sets say that cannot decide: the KPI CHECKLIST R1 set holds a placeholder in E5,
    /// G5, H5 and C5, and an earlier production set holds nothing in E5, G5 or C5 with real
    /// values at D3 and H5. A clean template from the second set was withheld for it.
    ///
    /// So a workbook is filled when a cell the tool writes holds something the tool would have
    /// written, and the cell that decided is said in the reason and in the report.
    /// </summary>
    public class FilledWorkbookTests
    {
        private const string Mosques = "<Mosques>";

        private const string ParkName = "<Park Name>";

        private static IReadOnlyDictionary<string, string> Cells(params string[] pairs)
        {
            var held = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int at = 0; at + 1 < pairs.Length; at += 2) held[pairs[at]] = pairs[at + 1];
            return held;
        }

        /// <summary>
        /// Two cells can decide it and both are cells the tool writes: the date the team types
        /// and the plot reference read off the plot's first sheet. The reference cell comes off
        /// the template rather than a second copy of the map.
        /// </summary>
        [Fact]
        public void TheMarksAreTheCellsTheToolWrites()
        {
            Assert.Equal(new[] { "E5", "C5" }, FilledMarks.CellsRead);

            IReadOnlyList<FilledMark> marks = FilledMarks.For(KpiTemplates.Mosques);
            Assert.Equal(2, marks.Count);
            Assert.Equal("E5", marks[0].Cell);
            Assert.Equal("a date", marks[0].What);
            Assert.Equal("C5", marks[1].Cell);
            Assert.Equal("a plot reference", marks[1].What);
            Assert.Equal(KpiTemplates.TypedByTheTeam[0], FilledMarks.Date.Cell);
        }

        [Fact]
        public void ADateInTheDateCellIsFilledAndTheCellIsNamed()
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { Mosques }, null, Cells("E5", "2026-09-10"));

            Assert.False(filled.IsMatched);
            Assert.False(filled.NeedsAPick);
            Assert.True(filled.IsFilled);
            Assert.Null(filled.Template);
            Assert.Equal("MOSQUES", filled.FilledAs.Name);
            Assert.Equal("E5", filled.DecidedBy.Mark.Cell);
            Assert.Equal("2026-09-10", filled.DecidedBy.Holds);
            Assert.Equal(
                "It is a filled MOSQUES checklist, not a template. E5 holds 2026-09-10, which is a date the tool writes.",
                filled.Reason);
            Assert.Equal("filled, not offered. " + filled.Reason, filled.InWords);
        }

        /// <summary>
        /// The reference cell decides on its own, so a filled workbook nobody typed a date into
        /// is still named as filled.
        /// </summary>
        [Fact]
        public void APlotReferenceInTheReferenceCellIsFilledAndTheCellIsNamed()
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { Mosques }, null, Cells("C5", "ANH-007-MO-100019"));

            Assert.True(filled.IsFilled);
            Assert.Equal("C5", filled.DecidedBy.Mark.Cell);
            Assert.Equal(
                "It is a filled MOSQUES checklist, not a template. C5 holds ANH-007-MO-100019, which is a plot reference the tool writes.",
                filled.Reason);
        }

        /// <summary>
        /// The KPI CHECKLIST R1 set, measured: a placeholder in every cell the team fills. Every
        /// one of them is offered, and the old rule offered this one too.
        /// </summary>
        [Fact]
        public void TheR1SetsPlaceholdersAreNotWhatTheToolWrites()
        {
            RecognisedWorkbook template = RecognisedWorkbook.Recognise(
                "KPI CHECKLIST - R1 MOSQUES.xlsx", new[] { Mosques }, null,
                Cells("E5", "<Date>", "G5", "<Name>", "H5", "<Position>", "C5", "<UID>"));

            Assert.True(template.IsMatched);
            Assert.False(template.IsFilled);
            Assert.Null(template.DecidedBy);
            Assert.Equal("MOSQUES", template.Template.Name);
        }

        /// <summary>
        /// **The earlier production set, measured, and the one the old rule withheld.** Nothing
        /// at E5, G5 or C5 and real values at D3 and H5, its D3 reading Future Park and its E4
        /// KING ABDULLAH South. A clean template must be offered.
        /// </summary>
        [Fact]
        public void TheEarlierSetsEmptyCellsAndRealValuesAreNotWhatTheToolWrites()
        {
            RecognisedWorkbook template = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - FUTURE PARKS.xlsx", new[] { ParkName }, null,
                Cells("E5", string.Empty, "G5", string.Empty, "C5", string.Empty,
                    "D3", "Future Park", "E4", "KING ABDULLAH South", "H5", "Landscape Architect"));

            Assert.True(template.IsMatched);
            Assert.False(template.IsFilled);
            Assert.Equal("FUTURE PARKS", template.Template.Name);
        }

        /// <summary>
        /// A cell that is not in the file at all is not a filled file either, which is the same
        /// rule as an empty one: nothing is withheld on a cell the tool has never written.
        /// </summary>
        [Fact]
        public void NoCellsReadAtAllChangesNothing()
        {
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { Mosques }, null, Cells()).IsMatched);
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { Mosques }, null, null).IsMatched);
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { Mosques }, null).IsMatched);
        }

        /// <summary>
        /// What the date cell can hold in a template nobody has filled. DATE OF THE DAY is the
        /// annotated set's words, and a lone number is a date to the invariant culture's parser
        /// and not to a person.
        /// </summary>
        [Theory]
        [InlineData("<Date>")]
        [InlineData("Date")]
        [InlineData("DATE OF THE DAY")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("10")]
        [InlineData("March")]
        [InlineData("TBC")]
        public void TheseAreNotADateSoTheTemplateIsOffered(string held)
        {
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { Mosques }, null, Cells("E5", held)).IsMatched);
        }

        [Theory]
        [InlineData("2026-09-10")]
        [InlineData("09/09/2026")]
        [InlineData("9 September 2026")]
        [InlineData("2026-09-09 14:07")]
        public void TheseAreADateSoTheWorkbookIsFilled(string held)
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { Mosques }, null, Cells("E5", held));

            Assert.True(filled.IsFilled);
            Assert.Equal("E5", filled.DecidedBy.Mark.Cell);
        }

        /// <summary>
        /// What the reference cell can hold in a template nobody has filled. A placeholder is
        /// bracketed, a name has a space in it, and neither a word alone nor a number alone is
        /// a plot reference.
        /// </summary>
        [Theory]
        [InlineData("<UID>")]
        [InlineData("")]
        [InlineData("KING ABDULLAH South")]
        [InlineData("Future Park")]
        [InlineData("TBC")]
        [InlineData("100019")]
        [InlineData("ANH 007 MO 100019")]
        public void TheseAreNotAPlotReferenceSoTheTemplateIsOffered(string held)
        {
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { Mosques }, null, Cells("C5", held)).IsMatched);
        }

        /// <summary>
        /// Both parameters the team picks for the reference, measured on the 1548 scan.
        /// </summary>
        [Theory]
        [InlineData("ANH-007-MO-100019")]
        [InlineData("DM-12")]
        [InlineData("FM-05")]
        public void TheseAreAPlotReferenceSoTheWorkbookIsFilled(string held)
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { Mosques }, null, Cells("C5", held));

            Assert.True(filled.IsFilled);
            Assert.Equal("C5", filled.DecidedBy.Mark.Cell);
        }

        /// <summary>
        /// **What the rule does with a placeholder shaped like the thing it stands for, which
        /// is an open question rather than a measurement.** Neither measured set holds one:
        /// the R1 set brackets every placeholder and the earlier set leaves the cell empty. A
        /// client set that hinted the shape instead, DM-00 or REF-0001 at C5, would be named as
        /// filled and withheld. This test says what happens today rather than that it is right,
        /// and the report prints C5 holds DM-00 so a person sees it in one line. Whether such a
        /// set exists is for Bader.
        /// </summary>
        [Theory]
        [InlineData("DM-00")]
        [InlineData("REF-0001")]
        public void AnUnbracketedPlaceholderShapedLikeAReferenceReadsAsFilledToday(string held)
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - MOSQUES.xlsx", new[] { Mosques }, null, Cells("C5", held));

            Assert.True(filled.IsFilled);
            Assert.Equal("C5 holds " + held + ", which is a plot reference the tool writes.", filled.DecidedBy.InWords);
        }

        /// <summary>
        /// The date is read before the reference, so a workbook holding both is decided by the
        /// date and the report names one cell rather than two.
        /// </summary>
        [Fact]
        public void ADateAndAReferenceTogetherAreDecidedByTheDate()
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { Mosques }, null,
                Cells("E5", "2026-09-10", "C5", "ANH-007-MO-100019"));

            Assert.Equal("E5", filled.DecidedBy.Mark.Cell);
            Assert.Equal("2026-09-10", filled.DecidedBy.Holds);
        }

        [Fact]
        public void AFilledParkFileIsNotOfferedAndNeedsNoPick()
        {
            RecognisedWorkbook named = RecognisedWorkbook.Recognise(
                "EXISTING PARKS EP-05.xlsx", new[] { ParkName }, null, Cells("E5", "2026-09-10"));
            Assert.True(named.IsFilled);
            Assert.False(named.NeedsAPick);
            Assert.Equal("EXISTING PARKS", named.FilledAs.Name);
            Assert.Equal(
                "It is a filled EXISTING PARKS checklist, not a template. E5 holds 2026-09-10, which is a date the tool writes.",
                named.Reason);

            RecognisedWorkbook future = RecognisedWorkbook.Recognise(
                "FUTURE PARKS FP-03.xlsx", new[] { ParkName }, null, Cells("C5", "FP-03"));
            Assert.True(future.IsFilled);
            Assert.Equal("FUTURE PARKS", future.FilledAs.Name);
            Assert.Equal("C5", future.DecidedBy.Mark.Cell);
        }

        /// <summary>
        /// A filled park file whose name names neither park, or both, is filled and not
        /// offered, and which park it was for is not guessed.
        /// </summary>
        [Theory]
        [InlineData("GRP KPI Checklist - PARKS.xlsx")]
        [InlineData("EXISTING AND FUTURE PARKS.xlsx")]
        public void AFilledParkFileWhoseNameCannotTellThePark_NamesNeither(string fileName)
        {
            RecognisedWorkbook unnamed = RecognisedWorkbook.Recognise(
                fileName, new[] { ParkName }, null, Cells("E5", "2026-09-10"));

            Assert.True(unnamed.IsFilled);
            Assert.False(unnamed.NeedsAPick);
            Assert.False(unnamed.IsMatched);
            Assert.Null(unnamed.FilledAs);
            Assert.Equal(
                "It is a filled EXISTING PARKS or FUTURE PARKS checklist, which its file name cannot tell apart, not a template. "
                + "E5 holds 2026-09-10, which is a date the tool writes.",
                unnamed.Reason);
            Assert.Equal("filled, not offered. " + unnamed.Reason, unnamed.InWords);
        }

        /// <summary>
        /// A park template whose name cannot tell the two apart still needs a pick, which is
        /// the answer the filled check must not swallow.
        /// </summary>
        [Fact]
        public void AnUnfilledParkTemplateStillNeedsAPick()
        {
            RecognisedWorkbook pick = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - PARKS.xlsx", new[] { ParkName }, null, Cells("E5", "<Date>", "C5", "<UID>"));

            Assert.True(pick.NeedsAPick);
            Assert.False(pick.IsFilled);
        }

        [Fact]
        public void AReadRefusalStillWinsOverAFilledCell()
        {
            RecognisedWorkbook refused = RecognisedWorkbook.Recognise(
                "MOSQUES.xlsx", new[] { Mosques }, "It could not be opened.", Cells("E5", "2026-09-10"));

            Assert.Equal("It could not be opened.", refused.Reason);
            Assert.False(refused.IsFilled);
        }

        [Fact]
        public void TheMarkCellsAreEmptyWhenTheTemplateHasNeither()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                PeekedWorkbook peeked = PeekedWorkbook.Of(WorkbookFixture.Create(folder));

                Assert.True(peeked.WasRead, peeked.Refusal);
                Assert.Equal(new[] { WorkbookFixture.MainSheet, WorkbookFixture.TreesSheet }, peeked.SheetNames);
                Assert.Empty(peeked.FirstSheetCells);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The tool writes E5 as an inline string. What it wrote is what the peek reads back,
        /// and a fresh template beside it still reads none.
        /// </summary>
        [Fact]
        public void TheMarkCellsReadWhatThePatcherWroteIntoThem()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string template = WorkbookFixture.Create(folder);
                string output = Path.Combine(folder, "MOSQUES DM-12.xlsx");
                PatchOutcome outcome = WorkbookPatcher.Patch(template, output, new[]
                {
                    CellWrite.Text(WorkbookFixture.MainSheet, "E5", "2026-09-10"),
                    CellWrite.Text(WorkbookFixture.MainSheet, "C5", "ANH-007-MO-100019")
                });
                Assert.True(outcome.Written, outcome.Refusal);

                IReadOnlyDictionary<string, string> written = PeekedWorkbook.Of(output).FirstSheetCells;
                Assert.Equal("2026-09-10", written["E5"]);
                Assert.Equal("ANH-007-MO-100019", written["C5"]);
                Assert.Empty(PeekedWorkbook.Of(template).FirstSheetCells);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// An Excel re-save turns the cell into a shared string, and the peek resolves it to
        /// its text rather than reading its index.
        /// </summary>
        [Fact]
        public void ASharedStringMarkCellIsResolvedToItsText()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.WithSharedStringAt(folder, "resaved.xlsx", "E5", "2026-09-10");

                Assert.Equal("2026-09-10", PeekedWorkbook.Of(path).FirstSheetCells["E5"]);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// The report names every withheld workbook and the cell that decided it, so a template
        /// wrongly withheld is traced in one line rather than by opening the file.
        /// </summary>
        [Fact]
        public void TheReportNamesEveryWithheldWorkbookAndTheCellThatDecidedIt()
        {
            TemplateListing listed = TemplateListing.Nothing.For(
                @"C:\templates",
                new[] { @"C:\templates\GRP KPI Checklist - MOSQUES.xlsx", @"C:\templates\MOSQUES DM-12.xlsx" },
                path => path.EndsWith("DM-12.xlsx", StringComparison.Ordinal)
                    ? RecognisedWorkbook.Recognise("MOSQUES DM-12.xlsx", new[] { Mosques }, null, Cells("C5", "ANH-007-MO-100019"))
                    : RecognisedWorkbook.Recognise("GRP KPI Checklist - MOSQUES.xlsx", new[] { Mosques }, null, Cells("E5", "<Date>")));

            string report = KpiCreateReport.Write(
                CreateFixture.Run(templatesListed: listed), new DateTime(2026, 9, 11, 9, 0, 0));

            Assert.Contains(
                "  templates folder: 2 workbooks in the folder, opened 2 times over 1 redraw since the folder was listed.\r\n"
                + "    not offered: MOSQUES DM-12.xlsx, C5 holds ANH-007-MO-100019, which is a plot reference the tool writes.\r\n",
                report);
            Assert.DoesNotContain("not offered: GRP KPI Checklist - MOSQUES.xlsx", report);
        }
    }
}
