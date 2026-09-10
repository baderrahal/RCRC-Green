using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The copy and patch, proven on a workbook the test builds itself. The failure this
    /// guards against is silent: a loaded and resaved client file lost 21 of 37 parts and
    /// still opened, so the assertions here are about what survives, not what was sent.
    /// </summary>
    public class WorkbookPatcherTests : IDisposable
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

        private string Source(bool withCalcPr = false)
        {
            return WorkbookFixture.Create(_folder, "template.xlsx", withCalcPr);
        }

        private string Output()
        {
            return Path.Combine(_folder, "filled.xlsx");
        }

        private static CellWrite[] OrdinaryWrites()
        {
            return new[]
            {
                CellWrite.Text(WorkbookFixture.MainSheet, "D3", "GR-NG05-DM-11"),
                CellWrite.Number(WorkbookFixture.MainSheet, "D8", 14250.75),
                CellWrite.Number(WorkbookFixture.TreesSheet, "B4", 30)
            };
        }

        /// <summary>
        /// One part fewer, and it is the calculation chain, which this tool removes on purpose.
        /// Anything else missing is the resave failure wearing this tool's name.
        /// </summary>
        [Fact]
        public void EveryPartOfTheSourceIsInTheOutputExceptTheOneRemovedOnPurpose()
        {
            string source = Source();
            PatchOutcome outcome = WorkbookPatcher.Patch(source, Output(), OrdinaryWrites());

            Assert.True(outcome.Written);
            Assert.Equal(9, outcome.PartsInSource);
            Assert.Equal(8, outcome.PartsInOutput);
            Assert.Equal(1, outcome.PartsDeliberatelyRemoved);
            Assert.True(outcome.KeptEveryPart);
            Assert.Equal(8, WorkbookFixture.PartCount(Output()));
            Assert.Null(WorkbookFixture.PartText(Output(), WorkbookPatcher.CalcChainPart));
        }

        /// <summary>
        /// Excel showed 0 for seven computed cells while the inputs beside them were right. The
        /// values were never wrong: every formula cell still carried the template's own cached
        /// result and Excel trusted it. **fullCalcOnLoad was already on that file**, so the flag
        /// alone is not the fix.
        /// </summary>
        [Fact]
        public void EveryCachedResultIsDroppedAndTheFormulaIsLeftAlone()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(withCalcPr: true), Output(), OrdinaryWrites());

            string main = WorkbookFixture.PartText(Output(), "xl/worksheets/sheet1.xml");
            string trees = WorkbookFixture.PartText(Output(), "xl/worksheets/sheet2.xml");

            Assert.Contains("<f>SUM(D4:D8)</f>", main);
            Assert.DoesNotContain("<f>SUM(D4:D8)</f><v>", main);
            Assert.Contains("<f>SUM(B4:B8)</f>", trees);
            Assert.DoesNotContain("<f>SUM(B4:B8)</f><v>", trees);

            Assert.Equal(2, outcome.Cache.CachedValuesDropped);
            Assert.Equal(0, outcome.Cache.FormulaCellsCarryingACachedValue);
        }

        /// <summary>
        /// All three together, read back off the output. Any one of them alone leaves Excel free
        /// to trust the cache, which is how a workbook opens showing stale zeros beside correct
        /// inputs and looks finished.
        /// </summary>
        [Fact]
        public void TheOutputIsCheckedForAllThreeThingsThatMakeExcelRecalculate()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(withCalcPr: true), Output(), OrdinaryWrites());

            Assert.True(outcome.Cache.RecalculatesOnOpen);
            Assert.Equal("0", outcome.Cache.CalcId);
            Assert.True(outcome.Cache.CalcIdCleared);
            Assert.Equal(0, outcome.Cache.FormulaCellsCarryingACachedValue);
            Assert.True(outcome.Cache.CalcChainRemoved);
            Assert.True(outcome.Cache.WillRecalculate);
        }

        /// <summary>
        /// A refused patch checked nothing, so nothing here may read as a check that passed.
        /// </summary>
        [Fact]
        public void ARefusedPatchCarriesNoCacheCheckThatPassed()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), new[]
            {
                CellWrite.Number("No Such Sheet", "B4", 1)
            });

            Assert.False(outcome.Written);
            Assert.False(outcome.Cache.WillRecalculate);
            Assert.False(outcome.Cache.CalcIdCleared);
            Assert.False(outcome.Cache.CalcChainRemoved);
        }

        [Fact]
        public void AnUntouchedPartComesThroughByteForByte()
        {
            string source = Source();
            WorkbookPatcher.Patch(source, Output(), OrdinaryWrites());

            Assert.Equal(WorkbookFixture.ImageBytes, WorkbookFixture.PartBytes(Output(), "xl/media/image1.png"));
            Assert.Equal(
                WorkbookFixture.PartBytes(source, "docProps/custom.xml"),
                WorkbookFixture.PartBytes(Output(), "docProps/custom.xml"));
            Assert.Equal(
                WorkbookFixture.PartBytes(source, "[Content_Types].xml"),
                WorkbookFixture.PartBytes(Output(), "[Content_Types].xml"));
        }

        [Fact]
        public void OnlyTheSheetsThatReceivedValuesAndTheWorkbookPartChanged()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());

            Assert.Equal(
                new[] { "xl/worksheets/sheet1.xml", "xl/worksheets/sheet2.xml", "xl/workbook.xml" },
                outcome.ChangedParts);
        }

        /// <summary>
        /// What landed is read back off the output file, never echoed from the input. The
        /// number is the invariant round trip form the file really stores.
        /// </summary>
        [Fact]
        public void EveryWrittenCellIsReadBackOffTheOutput()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());

            Assert.Equal(3, outcome.Landed.Count);
            Assert.Equal("GR-NG05-DM-11", outcome.Landed[0].Value);
            Assert.Equal("14250.75", outcome.Landed[1].Value);
            Assert.Equal("30", outcome.Landed[2].Value);

            Assert.Equal("GR-NG05-DM-11", WorkbookPatcher.ReadBack(Output(), WorkbookFixture.MainSheet, "D3"));
            Assert.Equal("14250.75", WorkbookPatcher.ReadBack(Output(), WorkbookFixture.MainSheet, "D8"));
            Assert.Null(WorkbookPatcher.ReadBack(Output(), WorkbookFixture.MainSheet, "Z99"));
        }

        [Fact]
        public void ACellThatAlreadyExistedKeepsItsStyleAndLosesItsOldValue()
        {
            WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());
            string sheet = WorkbookFixture.PartText(Output(), "xl/worksheets/sheet1.xml");

            Assert.Contains("s=\"2\"", sheet);
            Assert.DoesNotContain("stale", sheet);
            Assert.Contains("GR-NG05-DM-11", sheet);
        }

        [Fact]
        public void AFormulaCellNoWriteNamesKeepsItsFormulaAndItsCachedResult()
        {
            WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());
            string sheet = WorkbookFixture.PartText(Output(), "xl/worksheets/sheet1.xml");

            Assert.Contains("SUM(D4:D8)", sheet);
        }

        [Fact]
        public void TheWorkbookIsSetToRecalculateOnOpen()
        {
            WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());
            string workbook = WorkbookFixture.PartText(Output(), "xl/workbook.xml");

            Assert.Contains("fullCalcOnLoad=\"1\"", workbook);
            Assert.Contains("TheArea", workbook);
            Assert.True(
                workbook.IndexOf("<calcPr", StringComparison.Ordinal)
                    > workbook.IndexOf("definedNames", StringComparison.Ordinal),
                "calcPr sits after the defined names, where the schema puts it");
        }

        [Fact]
        public void AWorkbookThatAlreadyHasCalcPrGetsTheFlagWithoutASecondCalcPr()
        {
            WorkbookPatcher.Patch(Source(withCalcPr: true), Output(), OrdinaryWrites());
            string workbook = WorkbookFixture.PartText(Output(), "xl/workbook.xml");

            Assert.Contains("fullCalcOnLoad=\"1\"", workbook);

            // The template's own 191029 tells Excel the cache was written by an engine as new
            // as its own, which is exactly why it trusted the zeros. Zero replaces it.
            Assert.DoesNotContain("calcId=\"191029\"", workbook);
            Assert.Contains("calcId=\"0\"", workbook);
            Assert.Equal(2, workbook.Split(new[] { "calcPr" }, StringSplitOptions.None).Length);
        }

        /// <summary>
        /// Rows ascending and cells within a row ascending by column, because a cell out of
        /// order is a file Excel offers to repair.
        /// </summary>
        [Fact]
        public void NewRowsAndCellsComeOutInSheetOrder()
        {
            WorkbookPatcher.Patch(Source(), Output(), new[]
            {
                CellWrite.Number(WorkbookFixture.MainSheet, "D8", 1),
                CellWrite.Number(WorkbookFixture.MainSheet, "B3", 2),
                CellWrite.Number(WorkbookFixture.MainSheet, "F12", 3)
            });
            string sheet = WorkbookFixture.PartText(Output(), "xl/worksheets/sheet1.xml");

            int row3 = sheet.IndexOf("<row r=\"3\"", StringComparison.Ordinal);
            int row8 = sheet.IndexOf("<row r=\"8\"", StringComparison.Ordinal);
            int row9 = sheet.IndexOf("<row r=\"9\"", StringComparison.Ordinal);
            int row12 = sheet.IndexOf("<row r=\"12\"", StringComparison.Ordinal);
            Assert.True(row3 > 0 && row3 < row8 && row8 < row9 && row9 < row12, "rows are in order");

            int a3 = sheet.IndexOf("\"A3\"", StringComparison.Ordinal);
            int b3 = sheet.IndexOf("\"B3\"", StringComparison.Ordinal);
            int d3 = sheet.IndexOf("\"D3\"", StringComparison.Ordinal);
            Assert.True(a3 > 0 && a3 < b3 && b3 < d3, "cells within the row are in column order");
        }

        [Fact]
        public void AWriteNamingASheetTheWorkbookDoesNotHaveRefusesAndWritesNoFile()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), new[]
            {
                CellWrite.Text("Not There", "A1", "x")
            });

            Assert.False(outcome.Written);
            Assert.Equal("The workbook has no sheet named Not There, so nothing was written.", outcome.Refusal);
            Assert.False(File.Exists(Output()));
        }

        [Fact]
        public void ASourceThatIsNotAZipRefusesAndWritesNoFile()
        {
            string fake = Path.Combine(_folder, "fake.xlsx");
            File.WriteAllText(fake, "not a workbook at all");

            PatchOutcome outcome = WorkbookPatcher.Patch(fake, Output(), OrdinaryWrites());

            Assert.False(outcome.Written);
            Assert.Equal("The source is not a zip, so it is not an .xlsx however it is named.", outcome.Refusal);
            Assert.False(File.Exists(Output()));
        }

        [Fact]
        public void NothingToWriteIsARefusalRatherThanAnEmptyCopy()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), new CellWrite[0]);

            Assert.False(outcome.Written);
            Assert.False(File.Exists(Output()));
        }

        [Fact]
        public void AnOutputAlreadyThereIsOverwrittenSilently()
        {
            File.WriteAllText(Output(), "the old copy");
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), OrdinaryWrites());

            Assert.True(outcome.Written);
            Assert.Equal("GR-NG05-DM-11", WorkbookPatcher.ReadBack(Output(), WorkbookFixture.MainSheet, "D3"));
        }
    }

    public class WorkbookPeekTests : IDisposable
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

        [Fact]
        public void SheetNamesComeBackInWorkbookOrder()
        {
            string path = WorkbookFixture.Create(_folder);
            PeekedWorkbook peeked = PeekedWorkbook.Of(path);

            Assert.True(peeked.WasRead);
            Assert.Equal(new[] { "Main", "Trees" }, peeked.SheetNames);
        }

        [Fact]
        public void AFileThatIsNotAZipIsRefusedWithTheReason()
        {
            string fake = Path.Combine(_folder, "fake.xlsx");
            File.WriteAllText(fake, "plain text");

            PeekedWorkbook peeked = PeekedWorkbook.Of(fake);

            Assert.False(peeked.WasRead);
            Assert.Equal("It is not a zip, so it is not an .xlsx however it is named.", peeked.Refusal);
        }
    }

    public class CellRefTests
    {
        [Theory]
        [InlineData("A1", "A", 1, 1)]
        [InlineData("D8", "D", 4, 8)]
        [InlineData("H11", "H", 8, 11)]
        [InlineData("Z10", "Z", 26, 10)]
        [InlineData("AA10", "AA", 27, 10)]
        public void AReferenceSplitsIntoItsColumnAndItsRow(string reference, string column, int columnNumber, int row)
        {
            CellRef parsed = CellRef.Parse(reference);

            Assert.Equal(column, parsed.Column);
            Assert.Equal(columnNumber, parsed.ColumnNumber);
            Assert.Equal(row, parsed.Row);
            Assert.Equal(reference, parsed.ToString());
        }

        [Theory]
        [InlineData("8D")]
        [InlineData("D0")]
        [InlineData("D")]
        [InlineData("11")]
        [InlineData("d8")]
        [InlineData("ABCD1")]
        [InlineData("")]
        [InlineData(null)]
        public void AnythingThatIsNotACellReferenceIsRefused(string reference)
        {
            Assert.Null(CellRef.TryParse(reference));
            Assert.Throws<ArgumentException>(() => CellRef.Parse(reference));
        }
    }
}
