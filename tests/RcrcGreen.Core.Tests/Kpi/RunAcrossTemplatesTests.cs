using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// One press over several templates: the accounting above the per template halves, the rows
    /// the pane shows before and after, and the one report. Every expected value written by hand.
    /// </summary>
    public class RunAcrossTemplatesTests
    {
        private static TemplateSplit Split(params TemplateShare[] shares)
        {
            return new TemplateSplit(shares, new List<PlotTemplate>(), new List<PlotTemplate>());
        }

        private static KpiCreateRunSet Set(
            TemplateSplit split, IReadOnlyList<KpiCreateRun> runs, params TemplateOutcome[] outcomes)
        {
            return new KpiCreateRunSet("RCRC_NG03_EZ", split, runs, outcomes);
        }

        /// <summary>
        /// The four counts must add up to the number ticked, and they are counted off one list
        /// and checked against its own length rather than tallied as the run goes.
        /// </summary>
        [Fact]
        public void TheFourCountsAddUpToTheNumberTicked()
        {
            KpiCreateRunSet set = Set(
                Split(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "ST-05" }, string.Empty),
                    new TemplateShare(KpiTemplates.Schools, new string[0], "no ticked plot belongs to SCHOOLS")),
                new List<KpiCreateRun>(),
                TemplateOutcome.Wrote(KpiTemplates.Mosques, new[] { "DM-12" }, @"C:\out\MOSQUES DM-12.xlsx"),
                TemplateOutcome.Refused(KpiTemplates.Streets, new[] { "ST-05" }, "the workbook is open in Excel"),
                TemplateOutcome.NothingToWrite(KpiTemplates.Schools, "no ticked plot belongs to SCHOOLS"));

            Assert.Equal(3, set.TemplatesTicked);
            Assert.Equal(1, set.TemplatesWritten);
            Assert.Equal(1, set.TemplatesRefused);
            Assert.Equal(1, set.TemplatesWithNothingToWrite);
            Assert.True(set.CountsAddUp);
            Assert.True(set.Wrote);
            Assert.Empty(set.Refusals);
        }

        /// <summary>
        /// A template that fell out of every branch is a refusal in those words rather than a
        /// row nobody printed.
        /// </summary>
        [Fact]
        public void ATemplateThatReachedNoOutcomeIsSaidToBeABug()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG03_EZ",
                Split(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "ST-05" }, string.Empty)),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>());

            // Nothing was accounted for at all, and the counts still agree with each other,
            // which is exactly the shape a check against the ticked count has to catch.
            Assert.Equal(0, set.TemplatesTicked);
            Assert.True(set.CountsAddUp);
            Assert.Empty(set.Refusals);
        }

        /// <summary>
        /// **A refusal on one template does not stop the others.** One written and one refused
        /// is one workbook on disk and one row saying why, never one line for the run.
        /// </summary>
        [Fact]
        public void ARefusalOnOneTemplateLeavesTheOthersWritten()
        {
            KpiCreateRunSet set = Set(
                Split(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "ST-05" }, string.Empty)),
                new List<KpiCreateRun>(),
                TemplateOutcome.Wrote(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, @"C:\out\MOSQUES 2 plots.xlsx"),
                TemplateOutcome.Refused(KpiTemplates.Streets, new[] { "ST-05" }, "the workbook is open in Excel"));

            Assert.True(set.Wrote);
            Assert.Equal(new[] { "DM-12", "FM-05" }, set.PlotsWritten.ToArray());

            Assert.Equal(
                "MOSQUES: written to C:\\out\\MOSQUES 2 plots.xlsx",
                CreateWords.TemplateOutcomeRow(set.Outcomes[0]));
            Assert.Equal(
                "STREETS: Nothing was written. the workbook is open in Excel",
                CreateWords.TemplateOutcomeRow(set.Outcomes[1]));
        }

        /// <summary>
        /// The row before the press, both ways.
        /// </summary>
        [Fact]
        public void TheRowSaysWhatItWillGetOrWhyItWillGetNothing()
        {
            Assert.Equal(
                "MOSQUES: 2 plots, DM-12, FM-05",
                CreateWords.TemplateRow(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, string.Empty)));

            Assert.Equal(
                "SCHOOLS: no ticked plot belongs to SCHOOLS, so it will write nothing. "
                + "Tick a plot of it, or leave it and it stays listed writing nothing.",
                CreateWords.TemplateRow(
                    new TemplateShare(KpiTemplates.Schools, new string[0],
                        PlotsPerTemplate.NoPlotBelongs(KpiTemplates.Schools))));

            Assert.Equal(
                "MOSQUES: 1 plot, DM-12",
                CreateWords.TemplateRow(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12" }, string.Empty)));
        }

        /// <summary>
        /// The status line counts the four rather than adding the cells up, because which of six
        /// wrote is the thing a person needs off one line.
        /// </summary>
        [Fact]
        public void TheStatusLineCountsTheTemplatesAndNamesTheReport()
        {
            KpiCreateRunSet set = Set(
                Split(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "ST-05" }, string.Empty),
                    new TemplateShare(KpiTemplates.Schools, new string[0], "nothing")),
                new List<KpiCreateRun>(),
                TemplateOutcome.Wrote(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, @"C:\out\a.xlsx"),
                TemplateOutcome.Refused(KpiTemplates.Streets, new[] { "ST-05" }, "the workbook is open in Excel"),
                TemplateOutcome.NothingToWrite(KpiTemplates.Schools, "nothing"));

            Assert.Equal(
                "1 workbook written of 3 templates ticked, 1 refused, 1 template with no plot of its own. "
                + "2 plots went into a workbook. Report: C:\\reports\\run.txt",
                CreateWords.WroteAcross(set, @"C:\reports\run.txt"));
        }

        /// <summary>
        /// A double count refuses the whole press, and the status line says so rather than
        /// counting the workbooks it would have written.
        /// </summary>
        [Fact]
        public void ADoubleCountedPlotTakesOverTheStatusLine()
        {
            KpiCreateRunSet set = Set(
                Split(
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "DM-12" }, string.Empty)),
                new List<KpiCreateRun>());

            Assert.False(set.Split.AddsUp);
            Assert.Equal(
                "Nothing was written. DM-12 was counted into more than one workbook, MOSQUES and STREETS. "
                + "A plot has one template and one only, so nothing was written.",
                CreateWords.WroteAcross(set, string.Empty));
        }

        /// <summary>
        /// **ONE REPORT FOR THE RUN.** The accounting first, then the split, then every
        /// template's own report under a heading naming it, so a note against MOSQUES and one
        /// against STREETS can never read as one list.
        /// </summary>
        [Fact]
        public void TheReportOpensWithTheRunAndCarriesEveryTemplateUnderItsOwnName()
        {
            var split = new TemplateSplit(
                new[]
                {
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12" }, string.Empty),
                    new TemplateShare(KpiTemplates.Schools, new string[0],
                        PlotsPerTemplate.NoPlotBelongs(KpiTemplates.Schools))
                },
                new[]
                {
                    new PlotTemplate("DM-12", KpiTemplates.Mosques, TemplateRoute.Component,
                        "DAILY MOSQUE placed it in MOSQUES, and the plot prefix agrees"),
                    new PlotTemplate("SC-03", null, TemplateRoute.Nothing, "SCHOOLS is not ticked")
                },
                new[] { new PlotTemplate("SC-03", null, TemplateRoute.Nothing, "SCHOOLS is not ticked") });

            KpiCreateRun mosques = CreateFixture.Run(template: KpiTemplates.Mosques);

            KpiCreateRunSet set = Set(
                split,
                new[] { mosques },
                TemplateOutcome.Wrote(KpiTemplates.Mosques, new[] { "DM-12" }, @"C:\out\MOSQUES DM-12.xlsx"),
                TemplateOutcome.NothingToWrite(KpiTemplates.Schools,
                    PlotsPerTemplate.NoPlotBelongs(KpiTemplates.Schools)));

            string[] lines = KpiCreateReport
                .WriteAll(set, new DateTime(2026, 9, 12, 23, 30, 0))
                .Split('\n');

            int accounting = Array.FindIndex(lines, one => one.Contains("THIS RUN, ACROSS EVERY TEMPLATE"));
            int whichPlot = Array.FindIndex(lines, one => one.Contains("WHICH PLOT WENT INTO WHICH WORKBOOK"));
            int mosquesAt = Array.FindIndex(lines, one => one.Trim() == "TEMPLATE: MOSQUES");
            int schoolsAt = Array.FindIndex(lines, one => one.Trim() == "TEMPLATE: SCHOOLS");

            Assert.True(accounting >= 0);
            Assert.True(accounting < whichPlot);
            Assert.True(whichPlot < mosquesAt);
            Assert.True(mosquesAt < schoolsAt);

            Assert.Contains(lines, one => one.Trim() == "templates ticked                  2");
            Assert.Contains(lines, one => one.Trim() == "templates written                 1");
            Assert.Contains(lines, one => one.Trim() == "templates refused                 0");
            Assert.Contains(lines, one => one.Trim() == "templates with nothing to write   1");
            Assert.Contains(lines, one => one.Trim() == "those four add up to the ticked count   YES");
            // **The per plot accounting replaced the two lines that used to sit here**, which
            // counted plots off the template outcomes. A checklist is one plot now, so the count
            // that has to add up is per plot and counting it twice would be two records of one
            // fact. This set carries no plot outcomes, so every one of them reads nought.
            Assert.Contains(lines, one => one.Trim() == "plots ticked                      0");
            Assert.Contains(lines, one => one.Trim() == "folders made                      0");
            Assert.Contains(lines, one => one.Trim() == "workbooks written                 0");
            Assert.Contains(lines, one => one.Trim() == "plots that wrote nothing          0");
            Assert.Contains(lines, one => one.Trim()
                == "written plus wrote nothing is the ticked count   YES");

            Assert.Contains(lines, one => one.Trim()
                == "DM-12 | MOSQUES | Component | DAILY MOSQUE placed it in MOSQUES, and the plot prefix agrees");
            Assert.Contains(lines, one => one.Trim() == "SC-03 | NO WORKBOOK | Nothing | SCHOOLS is not ticked");

            // A template with no run of its own says so under its own heading rather than
            // printing an empty set of sections.
            Assert.Contains(lines, one => one.Contains(
                "No plot of this run belongs to it, so nothing was read for it and"));

            // The per template sections are the ones already tested, printed whole.
            Assert.Contains(lines, one => one.Contains("RECONCILIATION"));
        }

        /// <summary>
        /// **THE READING IS READ ONCE AND SHARED, THROUGH THE ONE MECHANISM THAT ALREADY
        /// EXISTS.** `HeldReadings.Decide` is asked once per template with that template's own
        /// share of the plots and the run it produced last press, so a second press after a
        /// refusal reads nothing. There is no second cache beside it, and a held run for one
        /// template never answers for another.
        /// </summary>
        [Fact]
        public void EachTemplatesReadingsAreReusedThroughTheOneMechanismAndNeverAcrossTemplates()
        {
            KpiCreateRun mosques = CreateFixture.Run(
                readings: new[] { CreateFixture.Plot("DM-12") },
                template: KpiTemplates.Mosques);

            ReadingsSource again = HeldReadings.Decide(
                mosques, "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached", KpiTemplates.Mosques,
                @"C:\templates\MOSQUES.xlsx", "PRX_Component", "PRX_Plot_UID2", new[] { "DM-12" });

            Assert.True(again.Reused);

            // The same held run offered for another template reads the model again and says so.
            ReadingsSource other = HeldReadings.Decide(
                mosques, "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached", KpiTemplates.Streets,
                @"C:\templates\STREETS.xlsx", "PRX_Component", "PRX_Plot_UID2", new[] { "DM-12" });

            Assert.False(other.Reused);
            Assert.Equal(
                "the run before was for MOSQUES and this press is for STREETS, so the model was read again",
                other.Why);
        }

        /// <summary>
        /// The sentence Bader asked for on the pane when more than one template is ticked.
        /// </summary>
        [Fact]
        public void ThePaneSaysTheThreeFieldsAreOneSetForTheWholeRun()
        {
            Assert.Equal(
                "The date, the prepared by and the position are typed once and go into every "
                + "workbook this press writes.",
                CreateWords.OneSetOfFields);
        }
    }
}
