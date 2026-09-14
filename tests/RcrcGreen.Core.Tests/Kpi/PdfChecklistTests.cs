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

            fields.AddRange(PdfFixture.ClientHeader());

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

        private string ParksForm(string fileName = "parks.pdf")
        {
            var fields = PdfForms.Parks.Fields
                .Select(one => PdfFixture.Field(one.FieldName, one.Note, one.X, one.Y))
                .ToList();

            fields.AddRange(PdfFixture.ClientHeader());

            return PdfFixture.Form(_folder, fileName, fields);
        }

        private PdfPlan ParkPlan(string plotId, string uid2, PdfWorkbookNumbers numbers)
        {
            return PdfFill.Of(
                CreateFixture.Plot(plotId, component: "EXISTING PARK", uid2: uid2),
                CountedGroups.Of(KpiTemplates.ExistingParks),
                null,
                Today,
                true,
                numbers);
        }

        /// <summary>
        /// **THE 19:52 RUN IS WHY.** Every EXISTING PARKS and FUTURE PARKS form wrote neither
        /// computed number while the other five templates wrote both, and the reason was in the
        /// file once per plot among 82,048 lines. One guard blanks both, so the two are counted
        /// together, and **the reason is said once with its plots named** rather than once per
        /// plot, which is the thing this section exists to save.
        /// </summary>
        [Fact]
        public void TheGlanceCountsTheTwoComputedNumbersAndSaysWhyOnceRatherThanPerPlot()
        {
            var canopy = new CanopyTotal(
                new[] { new CanopyRow("Tree List - Proposed", 21, "ALBIZIA LEBBECK", 3, 8.0) }, null);

            var computes = new PdfWorkbookNumbers(
                canopy, 410.0, 60.0, 771.0, ArithmeticCheck.Agreeing(null),
                SummaryCellCheck.Agreeing(ComputedPlaces.GreenCoverName, "D9", "F9+F11+H11", "F9"),
                SummaryCellCheck.WithNothingToCheck(
                    ComputedPlaces.PercentageName, WorkbookArithmetic.TheWorkbookHasNoPercentageCell));

            string form = ParksForm();

            PdfOutcome filled = PdfChecklist.Write(
                form, Path.Combine(_folder, "one.pdf"), ParkPlan("EP-01", "ANH-007-NP-100001", computes));
            PdfOutcome first = PdfChecklist.Write(
                form, Path.Combine(_folder, "two.pdf"),
                ParkPlan("EP-02", "ANH-007-NP-100002", PdfWorkbookNumbers.None));
            PdfOutcome second = PdfChecklist.Write(
                form, Path.Combine(_folder, "three.pdf"),
                ParkPlan("EP-03", "ANH-007-NP-100003", PdfWorkbookNumbers.None));

            PlotWorkbookPath where = PlotWorkbookPath.For(
                Path.Combine(_folder, "root"), KpiTemplates.ExistingParks,
                "EXISTING PARK", "ANH-007-NP-100001");

            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new[]
                {
                    PlotOutcome.Wrote("EP-01", KpiTemplates.ExistingParks, where).WithPdf(filled),
                    PlotOutcome.Wrote("EP-02", KpiTemplates.ExistingParks, where).WithPdf(first),
                    PlotOutcome.Wrote("EP-03", KpiTemplates.ExistingParks, where).WithPdf(second)
                });

            ComputedGlance glance = RunAtAGlance.Of(set).Computed;

            // Canopy 50 a tree over 3 trees is 150, plus planting 410 plus lawn 60 is 620.
            Assert.Equal("0.00062", filled.Landed
                .Single(one => one.Value == PdfValue.TotalAreasToBeGreened).Landed);

            Assert.Equal(1, glance.Greened.Written);
            Assert.Equal(2, glance.Greened.NotWritten);
            Assert.Equal(3, glance.Greened.Plots);
            Assert.Equal(
                "Total Green cover: 1 of 3 forms got it and 2 did not. 1 reason under this line.",
                glance.Greened.InWords);

            // **ONE LINE FOR TWO PLOTS**, the reason said once with both named.
            Assert.Equal(
                "2 plots, EP-02, EP-03: " + WorkbookArithmetic.NoWorkbookRead,
                Assert.Single(glance.Greened.Blanked).InWords);

            Assert.Equal(1, glance.Percentage.Written);
            Assert.Equal(
                "Percentage canopy: 1 of 3 forms got it and 2 did not. 1 reason under this line.",
                glance.Percentage.InWords);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 14, 19, 52, 0));

            Assert.Contains(glance.InWords, report);
            Assert.Contains("    " + glance.Greened.InWords, report);
            Assert.Contains("      2 plots, EP-02, EP-03: " + WorkbookArithmetic.NoWorkbookRead, report);
            Assert.Contains("    " + glance.Percentage.InWords, report);
        }

        /// <summary>
        /// Past four plots the count stands for the rest, so a reason that fired on 78 street
        /// plots is still one line. Four is what fits a line, the same number
        /// <see cref="CreateWords.TemplateRow"/> names outright.
        /// </summary>
        [Fact]
        public void AReasonThatFiredOnManyPlotsNamesFourAndCountsTheRest()
        {
            var blanked = new BlankedFor(
                "no workbook was read", new[] { "ST-01", "ST-02", "ST-03", "ST-04", "ST-05", "ST-06" });

            Assert.Equal(
                "6 plots, ST-01, ST-02, ST-03, ST-04 and 2 more: no workbook was read",
                blanked.InWords);

            Assert.Equal(
                "4 plots, ST-01, ST-02, ST-03, ST-04: no workbook was read",
                new BlankedFor("no workbook was read",
                    new[] { "ST-01", "ST-02", "ST-03", "ST-04" }).InWords);

            Assert.Equal(
                "1 plot, ST-01: no workbook was read",
                new BlankedFor("no workbook was read", new[] { "ST-01" }).InWords);
        }
    }
}
