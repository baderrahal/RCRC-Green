using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE TEAM SENT 154 PLOTS TO EXPORT AND WANTS EVERY ONE EXPORTED WITH NONE SKIPPED.**
    ///
    /// **THE FILE NEVER ENTERS THIS REPOSITORY.** It is the client's own list of which plots are
    /// in scope, this repository is public, and every case below writes its own into the temp
    /// folder, the same rule the workbooks and the PDFs already follow.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class PlotListFileTests : IDisposable
    {
        private readonly string _folder = PdfFixture.Folder();

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

        private string Written(params string[] lines)
        {
            string path = Path.Combine(_folder, "plots.txt");
            File.WriteAllLines(path, lines);
            return path;
        }

        private static PlotsInTheModel Model(params string[] plots)
        {
            return PlotsInTheModel.Of(plots, plots);
        }

        /// <summary>
        /// **THE ROUND'S OWN WORKED EXAMPLE, WRITTEN OUT BY HAND.** A list whose lines are HF-01,
        /// then NS-41 with spaces round it, then a blank line, then NS-41, then XX-99, then the
        /// words not a plot. Against a model naming HF-01, NS-41 and EP-05 it ticks HF-01 and
        /// NS-41, names NS-41 as listed on lines 2 and 4, names XX-99 as not in the model, names
        /// line 6 as not a plot, and names EP-05 as in the model and not on the list.
        /// </summary>
        [Fact]
        public void TheWorkedExampleReadsExactlyAsItWasWrittenOut()
        {
            PlotListRead read = PlotListFile.In(Written(
                "HF-01", "  NS-41  ", string.Empty, "NS-41", "XX-99", "not a plot"));

            Assert.True(read.Read, read.Why);

            // Three plots, in the file's own order, NS-41 counted once.
            Assert.Equal(new[] { "HF-01", "NS-41", "XX-99" }, read.Plots.ToArray());

            // Five lines held anything. The blank third line is skipped and is not one of them.
            Assert.Equal(5, read.Lines.Count);

            PlotListFault repeated = Assert.Single(read.Repeated);
            Assert.Equal("NS-41", repeated.What);
            Assert.Equal(new[] { 2, 4 }, repeated.Lines.ToArray());
            Assert.Contains("listed 2 times", repeated.Why);

            PlotListFault notAPlot = Assert.Single(read.NotPlots);
            Assert.Equal("not a plot", notAPlot.What);
            Assert.Equal(new[] { 6 }, notAPlot.Lines.ToArray());

            PlotsInTheModel model = Model("HF-01", "NS-41", "EP-05");

            var ticks = new PlotTicks(model);
            PlotTicks after = TickingTheList.Ticked(ticks, read);

            Assert.Equal(new[] { "HF-01", "NS-41" }, after.Ticked.ToArray());
            Assert.Equal(new[] { "XX-99" }, TickingTheList.NotInTheModel(model, read).ToArray());
            Assert.Equal(new[] { "EP-05" }, TickingTheList.NotOnTheList(model, read).ToArray());
        }

        /// <summary>
        /// **AND EVERY ONE OF THOSE IS A NAMED LINE ABOVE CREATE.** Nothing on the list drops out
        /// without a line, because a plot that quietly fell off a list of 154 is the whole thing
        /// this exists to stop.
        /// </summary>
        [Fact]
        public void EveryFaultIsANamedLineBeforeThePress()
        {
            PlotListRead read = PlotListFile.In(Written(
                "HF-01", "  NS-41  ", string.Empty, "NS-41", "XX-99", "not a plot"));

            string lines = string.Join("\n", TickingTheList.Lines(
                read, Model("HF-01", "NS-41", "EP-05"), plotId => "HEALTH"));

            Assert.Contains("3 plots over 5 lines", lines);
            Assert.Contains("1 line that is not a plot", lines);
            Assert.Contains("1 plot listed twice", lines);
            Assert.Contains("'NS-41' on lines 2 and 4", lines);
            Assert.Contains("'not a plot' on line 6", lines);
            Assert.Contains("XX-99 is on the list and the model does not name it", lines);
        }

        /// <summary>
        /// **A LIST WITH NOTHING WRONG WITH IT SAYS ONE LINE AND NO MORE**, because a line about
        /// nothing is one the team reads past on every other press.
        /// </summary>
        [Fact]
        public void ACleanListSaysItsCountAndNothingElse()
        {
            PlotListRead read = PlotListFile.In(Written("HF-01", "HF-02"));

            string line = Assert.Single(TickingTheList.Lines(
                read, Model("HF-01", "HF-02"), plotId => "HEALTH"));

            Assert.Equal(TickingTheList.Heading + " 2 plots over 2 lines.", line);
        }

        /// <summary>
        /// **A LISTED PLOT THAT WOULD GET NO WORKBOOK OR NO PDF IS NAMED BEFORE THE PRESS.**
        /// FM-09's component says SCHOOL and its prefix says MOSQUES, so neither places it, which
        /// is the split's own rule. UN-01 carries a prefix no form is named for.
        /// </summary>
        [Fact]
        public void APlotThatWouldWriteNothingIsNamedBeforeThePress()
        {
            Assert.Contains(
                "FM-09 would get NO workbook",
                TickingTheList.WouldWriteNothing("FM-09", plotId => "SCHOOL"));

            Assert.Contains(
                "UN-01 would get NO workbook and NO PDF",
                TickingTheList.WouldWriteNothing("UN-01", plotId => string.Empty));

            // FM-09 with the component its prefix agrees with is placed and writes both.
            Assert.Equal(
                string.Empty,
                TickingTheList.WouldWriteNothing("FM-09", plotId => "FRIDAY MOSQUE"));
        }

        /// <summary>
        /// **A PLOT IN THE WRONG CASE IS NOT A PLOT AT ALL**, which is the Shared rule, and it is
        /// told apart from text that was never one because the two need different answers.
        /// </summary>
        [Fact]
        public void ALowercaseIdentifierIsNotAPlotAndSaysWhy()
        {
            PlotListRead read = PlotListFile.In(Written("dm-11", "banana"));

            Assert.Empty(read.Plots);
            Assert.Equal(2, read.NotPlots.Count);

            Assert.Contains("two UPPERCASE letters", read.NotPlots[0].Why);
            Assert.Contains("Nothing changes its case", read.NotPlots[0].Why);
            Assert.DoesNotContain("UPPERCASE", read.NotPlots[1].Why);
        }

        /// <summary>
        /// **A FILE THAT CANNOT BE READ IS A REFUSAL WITH ITS REASON, never an empty list**,
        /// because an empty list and a file nobody could open tick exactly the same nothing.
        /// </summary>
        [Fact]
        public void AFileThatCannotBeReadRefusesAndTicksNothing()
        {
            PlotListRead read = PlotListFile.In(Path.Combine(_folder, "there-is-no-such-file.txt"));

            Assert.True(read.Set);
            Assert.False(read.Read);
            Assert.NotEqual(string.Empty, read.Why);
            Assert.Empty(read.Plots);

            PlotsInTheModel model = Model("HF-01");
            PlotTicks after = TickingTheList.Ticked(new PlotTicks(model, new[] { "HF-01" }), read);

            Assert.Empty(after.Ticked);
            Assert.Contains(read.Why, string.Join("\n", TickingTheList.Lines(read, model, plotId => string.Empty)));
        }

        /// <summary>
        /// **NO FILE SET SAYS NOTHING AT ALL.** It is the ordinary state and a line about it
        /// would be on every press.
        /// </summary>
        [Fact]
        public void NoFileSetSaysNothing()
        {
            PlotListRead read = PlotListFile.In(string.Empty);

            Assert.False(read.Set);
            Assert.Equal(PlotListRead.NoFileSet, read.Why);
            Assert.Empty(TickingTheList.Lines(read, Model("HF-01"), plotId => string.Empty));
        }

        /// <summary>
        /// **TICK THE LIST REPLACES EVERY TICK**, exactly as Select all and Clear do. A plot
        /// ticked by hand and not on the list comes off.
        /// </summary>
        [Fact]
        public void TickTheListReplacesEveryTickRatherThanAddingToThem()
        {
            PlotsInTheModel model = Model("HF-01", "HF-02", "HF-03");
            var ticks = new PlotTicks(model, new[] { "HF-03" });

            PlotTicks after = TickingTheList.Ticked(ticks, PlotListFile.In(Written("HF-01", "HF-02")));

            Assert.Equal(new[] { "HF-01", "HF-02" }, after.Ticked.ToArray());
            Assert.False(after.IsTicked("HF-03"));
        }

        /// <summary>
        /// **THE FILE'S ORDER IS KEPT AND NEVER SORTED**, because the report reads down it beside
        /// the team's own copy.
        /// </summary>
        [Fact]
        public void TheFilesOrderIsKept()
        {
            PlotListRead read = PlotListFile.In(Written("ST-09", "EP-01", "DM-11", "NS-02"));

            Assert.Equal(new[] { "ST-09", "EP-01", "DM-11", "NS-02" }, read.Plots.ToArray());
        }

        /// <summary>
        /// **A LINE THAT IS NOT A PLOT NEVER BECOMES ONE.** Nothing is guessed out of it and it
        /// ticks nothing, however much of a plot it looks like.
        /// </summary>
        [Fact]
        public void ALineThatIsNotAPlotTicksNothing()
        {
            PlotsInTheModel model = Model("DM-11");
            PlotListRead read = PlotListFile.In(Written("DM-11 Location Key Plan", "DM_11", "DM-"));

            Assert.Empty(read.Plots);
            Assert.Equal(3, read.NotPlots.Count);
            Assert.Empty(TickingTheList.Ticked(new PlotTicks(model), read).Ticked);
        }

        /// <summary>
        /// **THE REPORT OPENS WITH THE PLOT LIST, in the file's order, with what happened to each
        /// and three counts under it.** A list that goes in longer than it comes out is the
        /// failure this section exists to catch.
        /// </summary>
        [Fact]
        public void TheReportReadsDownTheTeamsOwnListAndCountsWhatWasWritten()
        {
            PlotsInTheModel model = Model("DM-11", "DM-12", "DM-14");
            PlotListRead read = PlotListFile.In(Written("DM-12", "DM-11", "XX-99"));

            var outcomes = new List<PlotOutcome>
            {
                PlotOutcome.Wrote("DM-12", KpiTemplates.Mosques,
                    PlotWorkbookPath.For("C:\\out", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-008-MO-100006")),
                PlotOutcome.WroteNothing("DM-11", KpiTemplates.Mosques,
                    PlotWorkbookPath.Refused("no PRX_Plot_UID2 was read off this plot's first sheet"),
                    false, "no PRX_Plot_UID2 was read off this plot's first sheet")
            };

            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new[] { "DM-11", "DM-12" }, plotId => "FRIDAY MOSQUE", new[] { KpiTemplates.Mosques }),
                new List<KpiCreateRun>(), new List<TemplateOutcome>(),
                null, outcomes, null, model, read);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 15, 5, 49, 0));

            Assert.Contains("== " + KpiCreateReport.TeamsListHeading + " (3) ==", report);

            // **THE TREES NOT WRITTEN COLUMN IS NEW**, Bader's decision of 15 September, because
            // FP-17, FP-20, FP-21 and FP-23 read YES and YES on the 13:32 run with 67 trees
            // between them written nowhere.
            //
            // **AND READY IS NEWER STILL**, Bader's decision of 16 September, because all 154
            // rows of the 16:37 press read YES four times over with 41 blank green cover boxes
            // and seven replaced workbooks among them.
            Assert.Contains(
                "plot | in the model | ticked | workbook | PDF | READY | trees not written | why not",
                report);

            // The file's order: DM-12 first, DM-11 second, XX-99 last. This press made no run,
            // so no plot lost a tree and every row reads none.
            //
            // **NOT ONE OF THE THREE IS READY.** DM-12's workbook was written and no PDF was
            // planned beside it, DM-11 got no workbook at all, and XX-99 is not in the model.
            Assert.Contains(
                "DM-12 | YES | YES | YES | NO | NO | none | no PDF was planned beside the workbook",
                report);
            Assert.Contains(
                "DM-11 | YES | YES | NO | NO | NO | none | no PRX_Plot_UID2 was read off this "
                + "plot's first sheet. no PDF was planned beside the workbook",
                report);
            Assert.Contains(
                "XX-99 | NO | NO | NO | NO | NO | none | the model does not name this plot, so it could not be ticked",
                report);

            Assert.Contains("IN THE MODEL AND NOT ON THE LIST, 1", report);
            Assert.Contains("listed: 3", report);
            Assert.Contains("workbooks written: 1", report);
            Assert.Contains("PDFs written: 0", report);
            Assert.Contains("ready: 0", report);

            Assert.True(
                report.IndexOf(KpiCreateReport.TeamsListHeading, StringComparison.Ordinal)
                    < report.IndexOf(KpiCreateReport.PlotListHeading, StringComparison.Ordinal),
                "THE PLOT LIST must sit above EVERY PLOT THE TOOL OFFERED, because it is the "
                + "list the team sent and the one they read the run against.");
        }

        /// <summary>
        /// **A PRESS WITH NO PLOT LIST FILE PRINTS NO SUCH SECTION**, because a section about a
        /// file nobody chose is one the team reads past on every other press.
        /// </summary>
        [Fact]
        public void APressWithNoPlotListFilePrintsNoSuchSection()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    new string[0], plotId => string.Empty, new KpiTemplate[0]),
                new List<KpiCreateRun>(), new List<TemplateOutcome>());

            Assert.DoesNotContain(
                KpiCreateReport.TeamsListHeading,
                KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 15, 5, 49, 0)));
        }
    }
}
