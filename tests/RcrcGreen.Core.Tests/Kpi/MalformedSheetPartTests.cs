using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **AUDIT 4 FINDING 67. FIVE CLASSES OPEN AN .xlsx AND ONLY THE TWO THAT WRITE CAUGHT
    /// `System.Xml.XmlException`.** A template whose `xl/workbook.xml` is fine and whose
    /// `Tree List - Existing` sheet part is malformed passes the peek, is recognised, is listed
    /// in the pane and is offered. At the press the parse threw past every catch in `Run` to the
    /// one that catches everything, so the pane read that the request failed and was stopped
    /// here rather than being let out, naming no template, no file and no plot, with no report
    /// written at all. A truncated download or a file another tool has rewritten is how a sheet
    /// part goes bad, and the peek that would have caught it does not read that part.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class MalformedSheetPartTests : IDisposable
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

        /// <summary>
        /// A template built the ordinary way, with one sheet part's XML cut off mid element. The
        /// zip is valid and every other part is valid, which is exactly the state a truncated
        /// download leaves behind.
        /// </summary>
        private string WithABrokenPart(string fileName, string sheetName)
        {
            string good = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "good-" + fileName,
                withGreenCoverLabel: true);

            string broken = Path.Combine(_folder, fileName);
            File.Copy(good, broken, true);

            // **THE PART IS FOUND THE WAY THE FIXTURE NAMES IT**, sheet1 for the main sheet and
            // sheet2 and sheet3 for the two tree lists, which is what `WorkbookFixture.Computing`
            // writes. Asking the package reader would need it public for a test alone.
            string partPath = sheetName == KpiTemplates.ExistingTreesSheet
                ? "xl/worksheets/sheet2.xml"
                : sheetName == KpiTemplates.ProposedTreesSheet
                    ? "xl/worksheets/sheet3.xml"
                    : "xl/worksheets/sheet1.xml";

            using (FileStream writing = new FileStream(broken, FileMode.Open, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(writing, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry = zip.GetEntry(partPath);
                entry.Delete();

                using (var stream = new StreamWriter(zip.CreateEntry(partPath).Open()))
                {
                    stream.Write("<?xml version=\"1.0\"?><worksheet><sheetData><row r=\"4\"><c r=");
                }
            }

            return broken;
        }

        /// <summary>
        /// **THE FILE AND THE SHEET ARE IN THE REFUSAL.** The whole press used to stop here
        /// naming neither.
        /// </summary>
        [Fact]
        public void AMalformedSheetPartRefusesTheSpeciesListAndNamesTheFileAndTheSheet()
        {
            string path = WithABrokenPart("broken.xlsx", KpiTemplates.ExistingTreesSheet);

            // **THE THROW IS CAUGHT HERE ON PURPOSE.** Without the catch in `SpeciesList` the
            // parse throws straight out, and an xUnit failure that is just the raw
            // `XmlException` names no file at all, which is exactly the fault: the whole press
            // used to stop naming no template, no file and no plot. Catching it here is what
            // makes this test's own failure name broken.xlsx either way.
            SpeciesList list;
            try
            {
                list = SpeciesList.In(path, KpiTemplates.Mosques.ExistingTrees);
            }
            catch (System.Xml.XmlException failed)
            {
                throw new Xunit.Sdk.XunitException(
                    "reading broken.xlsx, whose Tree List - Existing sheet part is malformed, "
                    + "threw XmlException out of SpeciesList.In rather than refusing: "
                    + failed.Message
                    + ". Run's catch list does not hold XmlException, so this reached the catch "
                    + "of everything and the pane said the request failed and was stopped here "
                    + "rather than being let out, naming no file and writing no report.");
            }

            Assert.False(
                list.WasRead,
                "broken.xlsx has a malformed Tree List - Existing sheet part, and the read came "
                + "back as though it had worked.");

            Assert.Contains("The sheet part for Tree List - Existing in broken.xlsx", list.Refusal);
            Assert.Contains("is not well formed XML", list.Refusal);
        }

        /// <summary>
        /// **ANOTHER TEMPLATE IN THE SAME PRESS STILL READS.** A refusal on one file is one
        /// file's refusal, which is the rule a refusal on one template already follows.
        /// </summary>
        [Fact]
        public void AnotherTemplateInTheSamePressStillReads()
        {
            WithABrokenPart("broken-two.xlsx", KpiTemplates.ExistingTreesSheet);

            string sound = WorkbookFixture.Computing(
                _folder,
                new[] { new WorkbookFixture.TreeRow(4, "Albizia lebbeck", "15", "8") },
                new[] { new WorkbookFixture.TreeRow(4, "Phoenix dactylifera", "15", "8") },
                fileName: "sound.xlsx",
                withGreenCoverLabel: true);

            SpeciesList list = SpeciesList.In(sound, KpiTemplates.Mosques.ExistingTrees);

            Assert.True(list.WasRead, list.Refusal);
            Assert.Equal(new[] { "Albizia lebbeck" }, list.Rows.Select(one => one.BotanicalName).ToArray());
        }

        /// <summary>
        /// **THE OTHER THREE READERS ANSWER THE SAME WAY**, each with the refusal it already has
        /// words for. All four used to catch `IOException`, `UnauthorizedAccessException` and
        /// `InvalidDataException` and stop there.
        /// </summary>
        [Fact]
        public void TheLabelledCellsAndTheCanopyColumnsRefuseTheSameFile()
        {
            string path = WithABrokenPart("broken-three.xlsx", KpiTemplates.Mosques.MainSheetName);

            LabelledCells labels = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.False(labels.Read, "the main sheet part is malformed and the read came back as read");
            Assert.Contains("is not well formed XML", labels.Why);
            Assert.Contains("broken-three.xlsx", labels.Why);

            // The canopy columns read the same file and answer per sheet, both refused.
            IReadOnlyList<TotalCanopyColumn> columns = TotalCanopyColumns.In(
                path,
                KpiTemplates.Mosques,
                LabelledCell.NotFound(
                    ComputedPlaces.GreenCoverName, "a label", "nothing opened the template"));

            Assert.All(columns, one => Assert.False(one.Found));
        }

        /// <summary>
        /// **AND THE STREET REFERENCE FILE TOO.** It is the fourth of the four and the only one
        /// whose subject is not a template.
        /// </summary>
        [Fact]
        public void TheStreetReferenceFileRefusesAMalformedSheetPart()
        {
            string path = WithABrokenPart("broken-four.xlsx", KpiTemplates.Mosques.MainSheetName);

            StreetReferenceFile file = StreetReferenceFile.In(path);

            Assert.False(file.Read, "a malformed first sheet came back as read");
            Assert.Contains("is not well formed XML", file.Why);
        }
    }
}
