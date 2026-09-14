using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// One plot's PDF end to end: the form is read, checked, filled, written and read back, and
    /// the report says what LANDED rather than what was sent.
    /// </summary>
    public class PdfChecklistTests : IDisposable
    {
        private readonly string _folder = PdfFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private static readonly DateTime Today = new DateTime(2026, 9, 14);

        /// <summary>
        /// A file shaped like the roads form: every field this tool fills, holding the note the
        /// tool knows. The client's own unfilled fields are left off, because nothing here
        /// writes into them and the check never looks at them.
        /// </summary>
        private string RoadsForm(string fileName = "roads.pdf", PdfValue moved = (PdfValue)(-1))
        {
            var fields = PdfForms.Roads.Fields
                .Select(one => PdfFixture.Field(
                    one.FieldName,
                    one.Value == moved ? "THE CLIENT CHANGED THIS" : one.Note,
                    one.X, one.Y))
                .ToList();

            return PdfFixture.Form(_folder, fileName, fields);
        }

        private PdfPlan RoadPlan(bool workbookWritten = true)
        {
            return PdfFill.Of(
                CreateFixture.Plot("ST-05", component: "STREET 30m ROW", uid2: "ANH-007-ST-100210"),
                CountedGroups.Of(KpiTemplates.Streets),
                StreetReferenceAnswer.Of(20.0, 330.66, "20", "330.66"),
                Today,
                workbookWritten,
                PdfWorkbookNumbers.None);
        }

        [Fact]
        public void APdfIsWrittenAndEveryFieldIsReadBackOffTheOutput()
        {
            string output = Path.Combine(_folder, "ANH-007-ST-100210.pdf");
            PdfOutcome outcome = PdfChecklist.Write(RoadsForm(), output, RoadPlan());

            Assert.True(outcome.Written, outcome.Refusal);
            Assert.Equal(output, outcome.Path);
            Assert.True(File.Exists(output));
            Assert.True(outcome.Check.Matched, outcome.Check.Why);

            // **What landed, never what was sent.**
            Assert.Empty(outcome.Disagreeing);
            Assert.Equal("ANH-007-ST-100210",
                outcome.Landed.Single(one => one.Value == PdfValue.Uid).Landed);
            Assert.Equal("14/09/2026",
                outcome.Landed.Single(one => one.Value == PdfValue.ReportDate).Landed);
            Assert.Equal("20", outcome.Landed.Single(one => one.Value == PdfValue.Row).Landed);

            // **The form asks km and the reference file gives m**, so 330.66 metres lands as
            // 0.33066 kilometres and the unit travels beside it.
            PdfLandedField length = outcome.Landed.Single(one => one.Value == PdfValue.Length);
            Assert.Equal("0.33066", length.Landed);
            Assert.Equal("km", length.Unit);
            Assert.Equal("m", outcome.Landed.Single(one => one.Value == PdfValue.Row).Unit);
        }

        /// <summary>
        /// The fields left blank travel with the outcome, each with its reason, so the report
        /// can name them per plot.
        /// </summary>
        [Fact]
        public void TheFieldsLeftBlankTravelWithTheirReasons()
        {
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm(), Path.Combine(_folder, "out.pdf"), RoadPlan());

            Assert.Contains(outcome.Blank, one => one.Value == PdfValue.GroundCover);
            Assert.Contains(outcome.Blank, one => one.Value == PdfValue.TotalAreasToBeGreened);
            Assert.Contains(outcome.Blank, one => one.Value == PdfValue.IrrigationWaterDemand);

            foreach (PdfFieldFill one in outcome.Blank)
            {
                Assert.False(string.IsNullOrWhiteSpace(one.Why), one.FieldName + " was left blank with no reason");
            }
        }

        /// <summary>
        /// **A FORM REISSUED WITH A NOTE MOVED WRITES NOTHING AND LEAVES NO FILE.** Writing a
        /// road width into a box that has become something else is exactly the silent wrong
        /// number this tool exists to prevent.
        /// </summary>
        [Fact]
        public void AFormWhoseNoteMovedWritesNothingAndLeavesNoFile()
        {
            string output = Path.Combine(_folder, "nothing.pdf");
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm("moved.pdf", PdfValue.Row), output, RoadPlan());

            Assert.False(outcome.Written);
            Assert.False(File.Exists(output));
            Assert.True(outcome.FormDidNotMatch);
            Assert.Contains("Row holds \"THE CLIENT CHANGED THIS\"", outcome.Refusal);
        }

        /// <summary>
        /// **A plot whose workbook was not written still gets a PDF.** Bader's decision:
        /// everything that comes from Revit goes in and the fields that read the workbook are
        /// named as not written.
        /// </summary>
        [Fact]
        public void APlotWhoseWorkbookWasNotWrittenStillGetsItsPdf()
        {
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm(), Path.Combine(_folder, "refused.pdf"), RoadPlan(false));

            Assert.True(outcome.Written, outcome.Refusal);
            Assert.Equal("ANH-007-ST-100210",
                outcome.Landed.Single(one => one.Value == PdfValue.Uid).Landed);
            Assert.Equal(PdfFill.TheWorkbookWasNotWritten,
                outcome.Blank.Single(one => one.Value == PdfValue.TotalAreasToBeGreened).Why);
        }

        /// <summary>
        /// A plot no form is named for writes nothing and says so, and no file is opened at all.
        /// </summary>
        [Fact]
        public void APlotNoFormIsNamedForWritesNothing()
        {
            PdfOutcome outcome = PdfChecklist.Write(
                RoadsForm(),
                Path.Combine(_folder, "never.pdf"),
                PdfFill.Of(CreateFixture.Plot("ZZ-01"), CountedGroups.Of(KpiTemplates.Mosques), null, Today, true,
                    PdfWorkbookNumbers.None));

            Assert.False(outcome.Written);
            Assert.Null(outcome.Check);
            Assert.Equal("no form is named for the plot prefix ZZ, so no PDF was written", outcome.Refusal);
        }

        /// <summary>
        /// **Which file is which form is decided by the fields it holds and never by its name.**
        /// A folder holding a renamed form still works.
        /// </summary>
        [Fact]
        public void TheFormFileIsFoundByItsFieldsAndNotByItsName()
        {
            string renamed = RoadsForm("something-nobody-would-guess.pdf");
            PdfFixture.Form(_folder, "decoy.pdf", new[] { PdfFixture.Field("UID", "a note") });

            string why;
            string found = PdfChecklist.FileFor(
                Directory.GetFiles(_folder, "*.pdf"), PdfForms.Roads, out why);

            Assert.Equal(renamed, found);
            Assert.Equal(string.Empty, why);
        }

        /// <summary>
        /// A folder holding no form this tool recognises names the form it wanted rather than
        /// failing quietly.
        /// </summary>
        [Fact]
        public void AFolderWithNoFormNamesTheFormItWanted()
        {
            PdfFixture.Form(_folder, "decoy.pdf", new[] { PdfFixture.Field("UID", "a note") });

            string why;
            string found = PdfChecklist.FileFor(
                Directory.GetFiles(_folder, "*.pdf"), PdfForms.Parks, out why);

            Assert.Equal(string.Empty, found);
            Assert.Equal(
                "the forms folder holds no file this tool recognises as Projects Basic Data - Parks",
                why);
        }

        /// <summary>
        /// The three lines at the top of the report count the PDFs, the plots that got a workbook
        /// and no PDF, and the forms that did not match, and every one of them is named.
        /// </summary>
        [Fact]
        public void TheGlanceCountsThePdfsTheGapsAndTheFormsThatDidNotMatch()
        {
            PdfOutcome wrote = PdfChecklist.Write(
                RoadsForm(), Path.Combine(_folder, "one.pdf"), RoadPlan());
            PdfOutcome moved = PdfChecklist.Write(
                RoadsForm("moved.pdf", PdfValue.Row), Path.Combine(_folder, "two.pdf"), RoadPlan());

            PlotWorkbookPath where = PlotWorkbookPath.For(
                Path.Combine(_folder, "root"), KpiTemplates.Streets, "STREET 30m ROW", "ANH-007-ST-100210");

            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new[]
                {
                    PlotOutcome.Wrote("ST-05", KpiTemplates.Streets, where).WithPdf(wrote),
                    PlotOutcome.Wrote("ST-06", KpiTemplates.Streets, where).WithPdf(moved)
                });

            PdfGlance glance = RunAtAGlance.Of(set).Pdfs;

            Assert.Equal(1, glance.Written);
            Assert.Single(glance.WithNoPdf);
            Assert.Single(glance.FormsThatDidNotMatch);
            Assert.Equal(
                "THE PDFS: 1 PDF was written, 1 of the plots that got a workbook got no PDF, and "
                + "1 form did not match what this tool knows.",
                glance.InWords);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 14, 14, 29, 0));

            Assert.Contains(glance.InWords, report);
            Assert.Contains(KpiCreateReport.PdfHeading, report);
            Assert.Contains("ST-05 | Projects Basic Data - Roads", report);
            Assert.Contains("field | unit | what was sent | what landed", report);
            Assert.Contains("left blank | why", report);
            Assert.Contains(PdfFill.GroundCoverIsNotPrintedApart, report);
        }
    }
}
