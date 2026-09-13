using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **The read back is what the outcome carries, not a separate method that agrees with it.**
    ///
    /// Proved by the audit: replacing the <c>CellText</c> read of the output with
    /// <c>write.Stored</c>, so every landed cell reported what was SENT, left the whole suite
    /// green. The one test that looked like it covered this checks
    /// <see cref="WorkbookPatcher.ReadBack"/>, a separate public method, rather than what
    /// <c>Patch</c> put on the outcome, so it passes either way.
    ///
    /// The rule that a report prints what landed and never what was sent is the one the Drawing
    /// Sheet was rebuilt around after four views printed as both created and not created. These
    /// cases are the only thing holding it up here.
    ///
    /// Each one writes the workbook, changes a written cell in the file BEHIND THE TOOL'S BACK,
    /// and then reads the outcome for that cell. A read back that hands over what it sent cannot
    /// tell the difference and reddens.
    /// </summary>
    public class ReadBackNoticesTests : IDisposable
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

        private string Source()
        {
            return WorkbookFixture.Create(_folder, "template.xlsx");
        }

        private string Output()
        {
            return Path.Combine(_folder, "filled.xlsx");
        }

        private static CellWrite[] Writes()
        {
            return new[]
            {
                CellWrite.Text(WorkbookFixture.MainSheet, "D3", "GR-NG05-DM-11"),
                CellWrite.Number(WorkbookFixture.MainSheet, "D8", 14250.75)
            };
        }

        /// <summary>
        /// Reaches into the written file and puts something else in the cell, the way anything
        /// outside this tool could. The value goes in as an inline string, which is what the
        /// patcher writes text as, so the file stays one Excel would open.
        /// </summary>
        private static void ChangeBehindItsBack(string path, string sheetPart, string cell, string toThis)
        {
            using (FileStream updating = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(updating, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry = zip.GetEntry(sheetPart);
                Assert.True(entry != null, "the fixture's sheet part " + sheetPart + " is not in the output");

                XDocument sheet;
                using (Stream reading = entry.Open())
                {
                    sheet = XDocument.Load(reading);
                }

                XNamespace ns = sheet.Root.Name.Namespace;
                XElement found = sheet.Descendants(ns + "c")
                    .FirstOrDefault(one =>
                        one.Attribute("r") != null && one.Attribute("r").Value == cell);
                Assert.True(found != null, "no cell " + cell + " in " + sheetPart);

                found.RemoveNodes();
                found.SetAttributeValue("t", "inlineStr");
                found.Add(new XElement(ns + "is", new XElement(ns + "t", toThis)));

                using (Stream writing = entry.Open())
                {
                    writing.SetLength(0);
                    sheet.Save(writing);
                }
            }
        }

        /// <summary>
        /// The fixture lays its main sheet down as sheet1.xml, which is what this reaches into.
        /// Core's own part reader is internal and stays that way: widening it so a test can
        /// call it would be the test changing the code it is meant to be checking.
        /// </summary>
        private const string MainSheetPart = "xl/worksheets/sheet1.xml";

        private static LandedCell Landed(PatchOutcome outcome, string cell)
        {
            LandedCell found = outcome.Landed.SingleOrDefault(one => one.Cell == cell);
            Assert.True(found != null, "the outcome carries no landed cell for " + cell);
            return found;
        }

        /// <summary>
        /// The plain case, so the two that follow cannot pass by the outcome being empty.
        /// </summary>
        [Fact]
        public void AnUntouchedOutputReportsWhatWasSent()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(Source(), Output(), Writes());

            Assert.True(outcome.Written);
            Assert.Equal("GR-NG05-DM-11", Landed(outcome, "D3").Value);
            Assert.Equal("14250.75", Landed(outcome, "D8").Value);
        }

        /// <summary>
        /// **THE CASE THAT CATCHES IT.** Two writes for one cell. The patcher applies them in
        /// order, so the FILE holds the second, while the outcome carries one landed cell per
        /// write. Reading the file gives the second value for both. Reporting what was sent
        /// gives the first value for the first entry, and that is the difference.
        ///
        /// It is not a contrived shape either: two writes for one cell is exactly the plan bug
        /// a read back exists to make visible, and the outcome has to describe the workbook the
        /// team opens rather than the list the tool assembled.
        /// </summary>
        [Fact]
        public void ACellWrittenTwiceIsReportedAsTheFileHoldsIt()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(
                Source(),
                Output(),
                new[]
                {
                    CellWrite.Text(WorkbookFixture.MainSheet, "D3", "THE FIRST VALUE"),
                    CellWrite.Text(WorkbookFixture.MainSheet, "D3", "THE SECOND VALUE")
                });

            Assert.True(outcome.Written);

            string[] said = outcome.Landed
                .Where(one => one.Cell == "D3")
                .Select(one => one.Value)
                .ToArray();

            Assert.Equal(new[] { "THE SECOND VALUE", "THE SECOND VALUE" }, said);
            Assert.Equal("THE SECOND VALUE", WorkbookPatcher.ReadBack(Output(), WorkbookFixture.MainSheet, "D3"));
        }

        /// <summary>
        /// The same for a number, so neither branch of the write can drift on its own.
        /// </summary>
        [Fact]
        public void ANumberCellWrittenTwiceIsReportedAsTheFileHoldsIt()
        {
            PatchOutcome outcome = WorkbookPatcher.Patch(
                Source(),
                Output(),
                new[]
                {
                    CellWrite.Number(WorkbookFixture.MainSheet, "D8", 111.5),
                    CellWrite.Number(WorkbookFixture.MainSheet, "D8", 222.25)
                });

            Assert.Equal(
                new[] { "222.25", "222.25" },
                outcome.Landed.Where(one => one.Cell == "D8").Select(one => one.Value).ToArray());
        }

        /// <summary>
        /// A cell changed in the file behind the tool's back, read back off it. This does NOT
        /// catch a read back that hands over what it sent, because the cell is not one this run
        /// wrote, and that is worth keeping written down: it is the shape the first attempt at
        /// this test took, and the break stayed green under it.
        /// </summary>
        [Fact]
        public void ACellChangedInTheFileReadsAsTheFileHoldsIt()
        {
            string output = Output();
            WorkbookPatcher.Patch(Source(), output, Writes());

            ChangeBehindItsBack(output, MainSheetPart, "D3", "SOMETHING ELSE ENTIRELY");

            Assert.Equal(
                "SOMETHING ELSE ENTIRELY",
                WorkbookPatcher.ReadBack(output, WorkbookFixture.MainSheet, "D3"));
        }

    }
}
