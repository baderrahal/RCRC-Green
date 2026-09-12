using System;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The status line while a scan or a create runs, driven by what is actually done. A scan
    /// took 123 seconds on a 104,031 element model behind one line that did not move, and a
    /// run over 78 street plots is minutes of the same. Every expected value below is written
    /// out by hand.
    /// </summary>
    public class ProgressWordsTests
    {
        /// <summary>
        /// The section lines take the report's own headings, so the numbers on screen are the
        /// report's numbers rather than a second record of them.
        /// </summary>
        [Fact]
        public void ASectionLineIsBuiltOffTheReportsOwnHeading()
        {
            Assert.Equal("Section 2 of 9, project information.", ProgressWords.Section(KpiReport.ProjectInformation));
            Assert.Equal("Section 3 of 9, title blocks and sheets.", ProgressWords.Section(KpiReport.TitleBlocksAndSheets));
            Assert.Equal("Section 4 of 9, linked models.", ProgressWords.Section(KpiReport.LinkedModels));
        }

        /// <summary>
        /// One reader covers sections 5 to 8 in one pass, so they are announced as a span
        /// rather than pretended apart.
        /// </summary>
        [Fact]
        public void TheScheduleSectionsAreASpan()
        {
            Assert.Equal(
                "Sections 5 to 8 of 9, schedules to areas and units.",
                ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits));
        }

        /// <summary>
        /// The schedules loop is the part of the scan that grows with the model, 951 on the
        /// first real one, so it counts. 400 of 951 is 42 percent, floored.
        /// </summary>
        [Fact]
        public void TheScheduleCountCarriesAFlooredPercentage()
        {
            Assert.Equal(
                "Sections 5 to 8 of 9, schedules, 400 of 951, 42%.",
                ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits, 400, 951));
            Assert.Equal(
                "Sections 5 to 8 of 9, schedules, 950 of 951, 99%.",
                ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits, 950, 951));
            Assert.Equal(
                "Sections 5 to 8 of 9, schedules, 951 of 951, 100%.",
                ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits, 951, 951));
        }

        /// <summary>
        /// The plot line names the plot and the count, and its percentage is of plots
        /// finished, two of eighteen when the third begins, so it never goes backwards.
        /// </summary>
        [Fact]
        public void ThePlotLineNamesThePlotTheCountAndTheFinishedShare()
        {
            Assert.Equal("Reading DM-44, plot 3 of 18, 11%.", ProgressWords.ReadingPlot("DM-44", 3, 18));
            Assert.Equal("Reading FM-21, plot 1 of 18, 0%.", ProgressWords.ReadingPlot("FM-21", 1, 18));
            Assert.Equal("Reading NS-06, plot 18 of 18, 94%.", ProgressWords.ReadingPlot("NS-06", 18, 18));
        }

        /// <summary>
        /// Where the total is not known there is no percentage at all, because a count alone
        /// is honest and a percentage over nothing is not.
        /// </summary>
        [Fact]
        public void NoTotalMeansNoPercentage()
        {
            Assert.Equal(string.Empty, ProgressWords.Percent(3, 0));
            Assert.Equal(string.Empty, ProgressWords.Percent(-1, 18));
            Assert.Equal(string.Empty, ProgressWords.Percent(19, 18));
            Assert.Equal(
                "Sections 5 to 8 of 9, schedules, 0 of 0.",
                ProgressWords.SectionSpan(KpiReport.Schedules, KpiReport.AreasAndUnits, 0, 0));
        }

        /// <summary>
        /// Driven by a count that only grows, the percentage cannot go backwards, checked over
        /// every value of the real 951.
        /// </summary>
        [Fact]
        public void ThePercentageNeverGoesBackwards()
        {
            string before = ProgressWords.Percent(0, 951);
            for (int done = 1; done <= 951; done++)
            {
                string now = ProgressWords.Percent(done, 951);
                Assert.True(
                    int.Parse(now.Trim(',', ' ', '%')) >= int.Parse(before.Trim(',', ' ', '%')),
                    done + " read " + now + " after " + before);
                before = now;
            }

            Assert.Equal(", 100%", before);
        }

        /// <summary>
        /// The section count is the leading digit of the report's own last heading, held here
        /// so the two cannot part without a test going red.
        /// </summary>
        [Fact]
        public void TheSectionCountIsTheReportsOwn()
        {
            Assert.Equal(9, ProgressWords.SectionCount);
            Assert.StartsWith("9 ", KpiReport.TheNineQuestions, StringComparison.Ordinal);
        }

        /// <summary>
        /// A heading with no section number is refused, because a made up number on the
        /// status line is worse than a thrown line in a log.
        /// </summary>
        [Fact]
        public void AHeadingWithNoNumberIsRefused()
        {
            Assert.Throws<ArgumentException>(() => ProgressWords.Section(KpiReport.NotFound));
            Assert.Throws<ArgumentException>(() => ProgressWords.Section(string.Empty));
        }

        /// <summary>
        /// The writing steps name themselves, quick as they are, so the line never sits on a
        /// finished count while the workbook is still being made.
        /// </summary>
        [Fact]
        public void TheWritingStepsNameThemselves()
        {
            Assert.Equal("Reusing the readings already held.", ProgressWords.ReusingTheReadings);
            Assert.Equal("Adding the plots up.", ProgressWords.AddingUp);
            Assert.Equal("Copying the template.", ProgressWords.CopyingTheTemplate);
            Assert.Equal("Writing the cells.", ProgressWords.WritingTheCells);
            Assert.Equal("Reading the written cells back.", ProgressWords.ReadingThemBack);
            Assert.Equal("Checking the workbook's own formulas.", ProgressWords.CheckingTheFormulas);
            Assert.Equal("Writing the report.", ProgressWords.WritingTheReport);
        }

        /// <summary>
        /// The patcher raises the four steps in order on a real write, so the words reach the
        /// pane from the work itself rather than from a narration beside it.
        /// </summary>
        [Fact]
        public void ThePatcherRaisesTheFourStepsInOrder()
        {
            string folder = WorkbookFixture.Folder();
            try
            {
                string template = WorkbookFixture.Create(folder);
                var steps = new System.Collections.Generic.List<string>();

                PatchOutcome outcome = WorkbookPatcher.Patch(
                    template, System.IO.Path.Combine(folder, "out.xlsx"),
                    new[] { CellWrite.Text(WorkbookFixture.MainSheet, "E5", "2026-09-12") },
                    null, steps.Add);

                Assert.True(outcome.Written, outcome.Refusal);
                Assert.Equal(
                    new[]
                    {
                        "Copying the template.",
                        "Writing the cells.",
                        "Reading the written cells back.",
                        "Checking the workbook's own formulas."
                    },
                    steps);
            }
            finally
            {
                System.IO.Directory.Delete(folder, true);
            }
        }
    }
}
