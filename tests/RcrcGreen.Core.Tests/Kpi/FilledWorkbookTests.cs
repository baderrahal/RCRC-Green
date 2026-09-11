using System;
using System.IO;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Finding 39. Recognition is the first sheet's name, a filled MOSQUES output's first sheet
    /// is still Mosques, and writing into the templates folder is allowed, so the next redraw
    /// offered MOSQUES DM-12.xlsx as a template beside the client's. The tool can tell,
    /// because it is what wrote it: E5 is the cell it writes the date into, and a workbook whose
    /// E5 no longer holds the template's placeholder has been filled. It is named as filled in
    /// the same list, with the reason, and not offered. Nothing is deleted or moved.
    /// </summary>
    public class FilledWorkbookTests
    {
        [Fact]
        public void ThePlaceholderIsTheOneTheFirstRealWorkbookHeld()
        {
            Assert.Equal("<Date>", KpiTemplates.DatePlaceholder);
            Assert.Equal("E5", KpiTemplates.TypedByTheTeam[0]);
        }

        [Fact]
        public void AMatchedSheetWhoseDateCellIsNotThePlaceholderIsNamedAsFilled()
        {
            RecognisedWorkbook filled = RecognisedWorkbook.Recognise(
                "MOSQUES DM-12.xlsx", new[] { "<Mosques>" }, null, "2026-09-10");

            Assert.False(filled.IsMatched);
            Assert.False(filled.NeedsAPick);
            Assert.True(filled.IsFilled);
            Assert.Null(filled.Template);
            Assert.Equal("MOSQUES", filled.FilledAs.Name);
            Assert.Equal(
                "It is a filled MOSQUES checklist, not a template. E5 holds 2026-09-10 where a template holds <Date>.",
                filled.Reason);
            Assert.Equal("filled, not offered. " + filled.Reason, filled.InWords);
        }

        [Fact]
        public void ThePlaceholderInTheDateCellKeepsTheTemplateOffered()
        {
            RecognisedWorkbook template = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - MOSQUES.xlsx", new[] { "<Mosques>" }, null, "<Date>");

            Assert.True(template.IsMatched);
            Assert.False(template.IsFilled);
            Assert.Equal("MOSQUES", template.Template.Name);
        }

        /// <summary>
        /// A cell that is not there, or empty, is not a filled file. A template must not be
        /// withheld on a cell it never had.
        /// </summary>
        [Fact]
        public void AnAbsentOrEmptyDateCellChangesNothing()
        {
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { "<Mosques>" }, null, null).IsMatched);
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { "<Mosques>" }, null, string.Empty).IsMatched);
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { "<Mosques>" }, null, "  ").IsMatched);
            Assert.True(RecognisedWorkbook.Recognise("MOSQUES.xlsx", new[] { "<Mosques>" }, null).IsMatched);
        }

        [Fact]
        public void AFilledParkFileIsNotOfferedAndNeedsNoPick()
        {
            RecognisedWorkbook named = RecognisedWorkbook.Recognise(
                "EXISTING PARKS EP-05.xlsx", new[] { "<Park Name>" }, null, "2026-09-10");
            Assert.True(named.IsFilled);
            Assert.False(named.NeedsAPick);
            Assert.Equal("EXISTING PARKS", named.FilledAs.Name);
            Assert.Equal(
                "It is a filled EXISTING PARKS checklist, not a template. E5 holds 2026-09-10 where a template holds <Date>.",
                named.Reason);

            RecognisedWorkbook future = RecognisedWorkbook.Recognise(
                "FUTURE PARKS FP-03.xlsx", new[] { "<Park Name>" }, null, "2026-09-10");
            Assert.True(future.IsFilled);
            Assert.Equal("FUTURE PARKS", future.FilledAs.Name);
        }

        /// <summary>
        /// A filled park file whose name names neither park, or both, is filled and not
        /// offered, and which park it was for is not guessed. The class says nothing is ever
        /// filled on a best guess, and a reason line naming EXISTING PARKS for a file that
        /// could as well be FUTURE PARKS was one.
        /// </summary>
        [Theory]
        [InlineData("GRP KPI Checklist - PARKS.xlsx")]
        [InlineData("EXISTING AND FUTURE PARKS.xlsx")]
        public void AFilledParkFileWhoseNameCannotTellThePark_NamesNeither(string fileName)
        {
            RecognisedWorkbook unnamed = RecognisedWorkbook.Recognise(
                fileName, new[] { "<Park Name>" }, null, "2026-09-10");

            Assert.True(unnamed.IsFilled);
            Assert.False(unnamed.NeedsAPick);
            Assert.False(unnamed.IsMatched);
            Assert.Null(unnamed.FilledAs);
            Assert.Equal(
                "It is a filled EXISTING PARKS or FUTURE PARKS checklist, which its file name cannot tell apart, not a template. "
                + "E5 holds 2026-09-10 where a template holds <Date>.",
                unnamed.Reason);
            Assert.Equal("filled, not offered. " + unnamed.Reason, unnamed.InWords);
        }

        [Fact]
        public void AReadRefusalStillWinsOverAFilledCell()
        {
            RecognisedWorkbook refused = RecognisedWorkbook.Recognise(
                "MOSQUES.xlsx", new[] { "<Mosques>" }, "It could not be opened.", "2026-09-10");

            Assert.Equal("It could not be opened.", refused.Reason);
            Assert.False(refused.IsFilled);
        }

        [Fact]
        public void TheDateCellIsNullWhenTheTemplateHasNoE5()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                PeekedWorkbook peeked = PeekedWorkbook.Of(WorkbookFixture.Create(folder));

                Assert.True(peeked.WasRead, peeked.Refusal);
                Assert.Equal(new[] { WorkbookFixture.MainSheet, WorkbookFixture.TreesSheet }, peeked.SheetNames);
                Assert.Null(peeked.FirstSheetDateCell);
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
        public void TheDateCellReadsWhatThePatcherWroteIntoE5()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string template = WorkbookFixture.Create(folder);
                string output = Path.Combine(folder, "MOSQUES DM-12.xlsx");
                PatchOutcome outcome = WorkbookPatcher.Patch(template, output,
                    new[] { CellWrite.Text(WorkbookFixture.MainSheet, "E5", "2026-09-10") });
                Assert.True(outcome.Written, outcome.Refusal);

                Assert.Equal("2026-09-10", PeekedWorkbook.Of(output).FirstSheetDateCell);
                Assert.Null(PeekedWorkbook.Of(template).FirstSheetDateCell);
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
        public void ASharedStringDateCellIsResolvedToItsText()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string path = WorkbookFixture.WithSharedStringAt(folder, "RESAVED.xlsx", "E5", "2026-09-10");

                Assert.Equal("2026-09-10", PeekedWorkbook.Of(path).FirstSheetDateCell);
            }
            finally
            {
                Directory.Delete(folder, true);
            }
        }
    }
}
