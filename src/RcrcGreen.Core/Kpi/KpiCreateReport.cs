using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The report one press of Create writes.
    ///
    /// The reconciliation comes first, because a number further down is worth nothing if the
    /// plots that went in are not the plots that came out. Every heading carries its own count,
    /// so a section that found nothing reads differently from one that was never filled in, and
    /// every count is of what happened rather than what was intended.
    /// </summary>
    public static class KpiCreateReport
    {
        public const string Refused = "NOTHING WAS WRITTEN";

        /// <summary>
        /// Printed where a species reached no row at all. It is the one case where a count does
        /// not reach the sheet's total, so it is said in those words rather than left blank.
        /// </summary>
        public const string NowhereAtAll = "NOWHERE, so its count is not in the total";

        public const string SchedulesHeading = "EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT";

        public const string FormulasHeading = "WHAT THE WORKBOOK WILL COMPUTE FROM THIS";

        /// <summary>
        /// The most rows one schedule prints in the report, the same cap the scan report uses.
        /// </summary>
        public const int ShownRows = 200;

        /// <summary>
        /// **ONE REPORT FOR THE RUN, not one per workbook.** The run's own accounting first,
        /// then the split plot by plot, then each template's report whole, under a heading
        /// naming it.
        ///
        /// Each template's half goes through <see cref="Write(KpiCreateRun, DateTime)"/>, the
        /// section writer that already had every layout tested, so a Street Design note on
        /// MOSQUES and a rounding note on STREETS sit under their own template and can never
        /// read as one list. Nothing about the per template sections changed.
        /// </summary>
        public static string WriteAll(KpiCreateRunSet set, DateTime writtenAt)
        {
            if (set == null) throw new ArgumentNullException("set");

            var report = new StringBuilder();

            Line(report, "RCRC Green KPI checklist");
            Line(report, "Document: " + Shown(set.DocumentTitle));
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, "Read only. Nothing in the model was changed and no template was touched.");
            Line(report, "This press covered " + set.PlotsTicked
                + (set.PlotsTicked == 1 ? " plot" : " plots")
                + " over " + set.TemplatesTicked
                + (set.TemplatesTicked == 1 ? " template" : " templates")
                + ", ONE WORKBOOK PER PLOT in a folder of its own, and every template's own "
                + "sections are below under its name.");
            Line(report, string.Empty);

            TheTeamsList(report, set);
            ThePlotList(report, set);
            TheGlance(report, set);
            TheTreeLists(report, set);
            TheRunAccounting(report, set);
            TheSplit(report, set);
            TheWaterDemand(report, set);
            TheShrubsAndGroundCover(report, set);

            // **EVERY PLOT GETS ITS OWN BLOCK.** It used to take the FIRST run of each
            // template and print that one, so the 18:15 run over 156 plots carried detail for
            // seven of them: EP-01, FP-16, HF-01, DM-11, PL-17, SC-03 and MM-01, the first of
            // each. The species list, the schedule print, the cells written and the
            // reconciliation, which are the sections that make this tool checkable at all,
            // covered 4 percent of the run.
            //
            // **The counts stay counts of the RUN**, because they already are: THIS RUN AT A
            // GLANCE and the accounting above both count over every run of the press, and a
            // block's own reconciliation reads 1 of 1 because a block IS one plot. That is why
            // a block per plot was chosen over one set of blocks carrying every plot: nothing
            // has to be re-counted and no number moves.
            foreach (TemplateOutcome outcome in set.Outcomes)
            {
                TemplateOutcome held = outcome;
                var mine = set.Runs.Where(one => ReferenceEquals(one.Template, held.Template)).ToList();

                Line(report, string.Empty);
                Line(report, "================================================================");
                Line(report, "TEMPLATE: " + held.Template.Name);
                Line(report, CreateWords.TemplateOutcomeRow(held));
                Line(report, "================================================================");
                Line(report, string.Empty);

                if (mine.Count == 0)
                {
                    Line(report, "  No plot of this run belongs to it, so nothing was read for it and");
                    Line(report, "  nothing was written. It stays ticked and stays listed.");
                    Line(report, string.Empty);
                    continue;
                }

                Line(report, "  " + mine.Count + (mine.Count == 1 ? " plot" : " plots")
                    + " of this press belong to it, and each has its own block below.");
                Line(report, string.Empty);

                foreach (KpiCreateRun run in mine)
                {
                    Line(report, "----------------------------------------------------------------");
                    Line(report, "PLOT: " + Shown(OnePlotOf(run)) + "   TEMPLATE: " + held.Template.Name);
                    Line(report, "----------------------------------------------------------------");

                    report.Append(Write(run, writtenAt, false));
                }
            }

            // **THE CONTENTS ARE COUNTED OFF THE BODY THAT WAS JUST WRITTEN**, so they are a
            // measurement of this file rather than a claim about it, and they go above it.
            string body = report.ToString();
            var contents = new StringBuilder();
            TheContents(contents, body);

            return contents + body;
        }

        public const string ContentsHeading = "WHAT IS IN THIS FILE";

        /// <summary>
        /// Every section of the body, how many lines it came to, how many blocks it is spread
        /// over and what one block of it costs.
        ///
        /// **A REPORT NOBODY CAN OPEN IS NOT A RECORD.** The 18:15 run wrote 10,708 lines over
        /// seven plot blocks. The 19:52 run wrote 82,048 over 156, and 42,570 of them were one
        /// section, which is 52 percent of the file. Nothing in either file said which section
        /// cost what, so the round that cut it had to work it out by scrolling.
        ///
        /// **THE NEXT CUT IS MADE ON NUMBERS.** This is the instrument rather than the cut: it
        /// decides nothing, leaves out nothing and names no section, and a section added or
        /// renamed appears in it with no change here.
        /// </summary>
        private static void TheContents(StringBuilder report, string body)
        {
            IReadOnlyList<ReportSection> sections = ReportSections.Of(body);

            Heading(report, ContentsHeading, sections.Count,
                "every section of the body below, counted off the text this run just wrote, widest first");

            Line(report, "    lines |  blocks |    each | section");
            foreach (ReportSection one in sections) Line(report, "  " + ReportSections.InWords(one));

            Line(report, "  " + ReportSections.LinesIn(body)
                + " lines in the body below. This block is above them and is not in the counts.");
            Line(report, "  A section printed once per plot has one block per plot, so its own "
                + "size is the third column.");
            Line(report, string.Empty);
        }

        public const string TeamsListHeading = "THE PLOT LIST";

        public const string TickedNotListedHeading = "TICKED AND NOT ON THE LIST";

        /// <summary>
        /// **THE TEAM SENT 154 PLOTS TO EXPORT AND WANTS EVERY ONE EXPORTED WITH NONE SKIPPED.**
        /// This is their own list, in their own order, with what happened to each.
        ///
        /// **NOTHING ON THE LIST DROPS OUT WITHOUT A LINE.** A plot the model does not name, a
        /// plot that went into no workbook, a plot whose PDF was not written: each is a row with
        /// its reason rather than an absence somebody has to notice.
        ///
        /// It sits beside <see cref="ThePlotList"/>, which is every plot the TOOL offered. The
        /// two are different questions and they are printed apart: one is the model's list and
        /// this is the team's, and where they disagree that is the answer rather than a fault.
        ///
        /// **A press with no plot list file set prints nothing at all**, because a section about
        /// a file nobody chose is one the team reads past on every other press.
        /// </summary>
        private static void TheTeamsList(StringBuilder report, KpiCreateRunSet set)
        {
            PlotListRead list = set.PlotList;
            if (list == null || !list.Set) return;

            Heading(report, TeamsListHeading, list.Plots.Count,
                "the team's own plot list file, in its own order, with what this press did with "
                + "each of its plots");

            Line(report, "  file: " + list.Path);

            if (!list.Read)
            {
                Line(report, "  " + list.Why);
                Line(report, string.Empty);
                return;
            }

            Line(report, "  " + list.InWords);

            foreach (PlotListFault one in list.NotPlots) Line(report, "    " + one.InWords);
            foreach (PlotListFault one in list.Repeated) Line(report, "    " + one.InWords);

            Line(report, string.Empty);
            Line(report, "  plot | in the model | ticked | workbook | PDF | " + PlotReady.Column
                + " | trees not written | why not");

            var byPlot = set.PlotOutcomes.ToList();

            // **THE TREES THAT REACHED NO ROW, PER PLOT.** Counted off the runs' own matches, so
            // the column and the refusal that produced it are one record. On the 13:32 run FP-17,
            // FP-20, FP-21 and FP-23 all read YES and YES here with 67 trees between them written
            // nowhere and not a word about it in the row.
            IReadOnlyList<PlotTreesNotWritten> lost = TreesNotWritten.Of(set);
            int workbooks = 0;
            int pdfs = 0;
            int ready = 0;

            foreach (string plotId in list.Plots)
            {
                string held = plotId;
                PlotOutcome outcome = byPlot.FirstOrDefault(
                    one => string.Equals(one.PlotId, held, StringComparison.Ordinal));

                bool inTheModel = set.Plots != null && set.Plots.Holds(held);
                bool written = outcome != null && outcome.Written;
                bool pdf = outcome != null && outcome.Pdf != null && outcome.Pdf.Written;

                if (written) workbooks++;
                if (pdf) pdfs++;

                string trees = TreesNotWritten.For(lost, held);

                // **THE FOUR COLUMNS BEFORE THIS ONE ALL READ YES ON EVERY ROW OF THE 16:37
                // PRESS**, and 41 of those plots went to the client with a blank green cover
                // box. Each of the four answers a true and narrow question and none of them is
                // the one the team is asking.
                ReadyAnswer can = PlotReady.For(set, held, lost, inTheModel, set.Plots != null);
                if (can.Ready) ready++;

                Line(report, "  " + Join(
                    held,
                    set.Plots == null ? "NOT READ" : (inTheModel ? "YES" : "NO"),
                    outcome == null ? "NO" : "YES",
                    written ? "YES" : "NO",
                    pdf ? "YES" : "NO",
                    can.Ready ? "YES" : "NO",
                    trees.Length == 0 ? "none" : trees,
                    can.WhyInWords));
            }

            IReadOnlyList<string> missing = TickingTheList.NotOnTheList(set.Plots, list);

            Line(report, string.Empty);
            Line(report, "  IN THE MODEL AND NOT ON THE LIST, " + missing.Count
                + ". Whether any of these should have been on it is for the team.");
            foreach (string one in missing) Line(report, "    " + one);

            // **THE 16:06 PRESS TICKED 166 PLOTS AGAINST THIS LIST OF 154.** The twelve extra
            // plots were written and filed, three of that press's shared value collisions came
            // from them, and nothing in the file said they were ticked. This is the other
            // direction from the block above: that one is about the MODEL and this one is about
            // what somebody ticked.
            IReadOnlyList<string> extra = TickingTheList.TickedAndNotOnTheList(
                set.PlotOutcomes.Select(one => one.PlotId), list);

            Line(report, string.Empty);
            Line(report, "  " + TickedNotListedHeading + ", " + extra.Count
                + ". A workbook and a PDF were written for each of these and the team did not "
                + "ask for them.");

            foreach (string one in extra)
            {
                PlotOutcome outcome = byPlot.FirstOrDefault(
                    two => string.Equals(two.PlotId, one, StringComparison.Ordinal));

                Line(report, "    " + Join(
                    one,
                    outcome != null && outcome.Written
                        ? "workbook " + outcome.Where.FilePath
                        : "no workbook",
                    outcome != null && outcome.Pdf != null && outcome.Pdf.Written
                        ? "PDF " + outcome.Pdf.Path
                        : "no PDF"));
            }

            Line(report, string.Empty);
            Line(report, "  listed: " + list.Plots.Count);
            Line(report, "  workbooks written: " + workbooks);
            Line(report, "  PDFs written: " + pdfs);
            Line(report, "  ready: " + ready);
            Line(report, string.Empty);
        }

        /// <summary>
        /// **A PLOT ON NO SOFTSCAPE SCHEDULE SAYS SO EVEN THOUGH BOTH FILES WERE WRITTEN.**
        /// Bader's decision of 15 September. FM-07 is on a sheet and on no schedule, and on the
        /// 13:32 run its row read YES and YES with an empty last column while its PDF carried
        /// Existing Trees 0, Proposed Trees 0, TOTAL trees 0 and Total areas to be greened 0
        /// against 57 trees the model's own KPI% schedules list for it. A row with nothing in
        /// its last column is the row of a plot that came out right.
        ///
        /// **NOTHING IN src CALLS THIS ANY MORE, AND IT IS NAMED RATHER THAN DELETED**, the way
        /// the six uncalled members in the KPI rules already are. The row's last column carries
        /// <see cref="PlotReady"/>'s reasons since 16 September, and a plot on no softscape
        /// schedule has all three of its tree boxes blank with <see cref="PdfFill.NoSoftscapeRead"/>
        /// as the reason, so READY names it three times over already. A fourth line would be a
        /// fourth record of one fact. Its tests are what keep this sentence honest.
        /// </summary>
        public static string NoSoftscapeOnTheList(PlotReading reading)
        {
            if (reading == null || reading.SoftscapeRead) return string.Empty;

            return "both files were written and " + PdfFill.NoSoftscapeRead(reading);
        }

        public const string PlotListHeading = "EVERY PLOT THE TOOL OFFERED";

        /// <summary>
        /// Every plot the model names, where each came from and whether it was ticked.
        ///
        /// **A PLOT THE TOOL OFFERS THAT THE MODEL DOES NOT HOLD IS A PLOT SOMEBODY WILL TICK**,
        /// and until this section nothing in the file named a plot that was not ticked. The 05:49
        /// run listed 165 and ticked 156, and MM-09 to MM-15 sat unticked and appeared nowhere in
        /// 57,143 lines, so seven plots could be argued about for a round with no record to read.
        ///
        /// It is the FIRST section of the body, above the glance, because it is the list every
        /// other number in the file is a subset of.
        /// </summary>
        private static void ThePlotList(StringBuilder report, KpiCreateRunSet set)
        {
            PlotOriginList origins = set.Origins;

            Heading(report, PlotListHeading, origins.Listed,
                "every plot the model names and every plot that was ticked, read off the live "
                + "document when Create was pressed");

            // **THE SCHEDULE HALF OF THIS LIST NAMES WHAT IT COULD NOT READ**, audit 4 finding
            // 65. A schedule whose plot filter threw used to drop out of it in silence, which
            // read as a schedule belonging to no plot, so it is said here before the counts that
            // are short because of it.
            if (set.Plots != null && set.Plots.SchedulesNotRead.Count > 0)
            {
                Line(report, "  " + SchedulePlotReads.Heading + ", "
                    + set.Plots.SchedulesNotRead.Count + ":");
                foreach (string one in set.Plots.SchedulesNotRead) Line(report, "    " + one);
                Line(report, string.Empty);
            }

            if (!origins.Counts)
            {
                Line(report, "  " + PlotOriginWords.NothingRead);
                Line(report, string.Empty);
                return;
            }

            Line(report, "  " + origins.InWords);
            Line(report, "  " + origins.WhatTheTwoLinesCount);
            Line(report, "  every plot is in exactly one source row and one tick state   "
                + (origins.AddsUp ? "YES" : "NO, WHICH IS A BUG IN THIS TOOL"));

            // **The plots nobody ticked are the ones nothing else in this file says a word
            // about.** The ticked ones have a block each below, so they are listed here for the
            // count and named up there for the detail.
            Line(report, string.Empty);
            Line(report, "  NOT TICKED, " + origins.NotTicked
                + ". Nothing else in this file mentions these plots.");
            foreach (PlotOrigin one in origins.NotTickedRows) Line(report, "    " + one.InWords);

            Line(report, string.Empty);
            Line(report, "  plot | where it came from | ticked");
            foreach (PlotOrigin one in origins.Rows) Line(report, "  " + one.InWords);

            Line(report, string.Empty);
        }

        public const string GlanceHeading = "THIS RUN AT A GLANCE";

        /// <summary>
        /// **THE QUESTIONS THIS PRESS ANSWERS, EACH IN ONE LINE AND A SHORT LIST.** They were
        /// all answerable before and all of them were spread over hundreds of lines: the streets
        /// area over one block per plot, the region type over one row per plot, the divisions
        /// over one per template formula section, and the two computed numbers over one blank
        /// field's reason per plot.
        ///
        /// **THE SUBTITLE USED TO SAY THREE AND FOUR WERE PRINTED.** The PDFs were added under
        /// it and the word was left, which is a constant standing in for a count in the one
        /// section built so nobody has to count. It names none now.
        ///
        /// It is the FIRST section of the file, above the run's own accounting, because these
        /// are what a person opens the report to check. **Nothing here is a second record of
        /// anything**: every number is counted by <see cref="RunAtAGlance"/> off the same runs
        /// the sections below print from, and the detail stays where it is.
        /// </summary>
        private static void TheGlance(StringBuilder report, KpiCreateRunSet set)
        {
            RunGlance glance = RunAtAGlance.Of(set);

            // **THE ONLY HEADING HERE THAT CARRIES NO COUNT.** Every other one prints how many
            // rows are under it, so a section that found nothing reads differently from one
            // nobody filled in. Three is not a count of anything this run found, it is how many
            // questions there are, and a constant in the place a count goes is a number that
            // reads as a measurement.
            Line(report, "== " + GlanceHeading + " ==");
            Line(report, "the questions this press answers, each in one line, with the "
                + "detail left where it is");

            Line(report, "  " + glance.StreetsArea.InWords);
            foreach (string one in glance.StreetsArea.Without) Line(report, "    " + one);

            Line(report, string.Empty);
            Line(report, "  " + glance.Regions.InWords);
            Line(report, "    type | plots | against the area cell's note");
            foreach (RegionTypeCount one in glance.Regions.Types)
            {
                Line(report, "    " + Join(
                    one.TypeName,
                    one.Plots.ToString(CultureInfo.InvariantCulture),
                    RegionChoice.AgainstTheNote(one.TypeName)));
            }

            Line(report, string.Empty);
            Line(report, "  " + glance.Divisions.InWords);
            if (glance.Divisions.Found > 0)
            {
                Line(report, "    plot | sheet and cell | which cell it divides by");
                foreach (string one in glance.Divisions.Where) Line(report, "    " + one);
            }

            foreach (string one in glance.Divisions.NotEvaluated) Line(report, "    " + one);

            Line(report, string.Empty);
            Line(report, "  " + glance.Pdfs.InWords);
            foreach (string one in glance.Pdfs.WithNoPdf) Line(report, "    " + one);
            foreach (string one in glance.Pdfs.FormsThatDidNotMatch) Line(report, "    " + one);

            // **THE 13:32 RUN IS WHY THIS LINE EXISTS.** FP-17 lost 36 trees, FP-21 25, FP-20 5
            // and FP-23 1, each to a refusal recorded under its own plot, and THE PLOT LIST read
            // YES and YES for all four while the glance said nothing at all. 67 trees left the
            // building and the file's first two sections were silent about it.
            IReadOnlyList<PlotTreesNotWritten> lost = TreesNotWritten.Of(set);

            Line(report, string.Empty);
            Line(report, "  " + TreesNotWritten.InWords(lost));
            foreach (PlotTreesNotWritten one in lost)
            {
                Line(report, "    " + one.PlotId + ": " + one.InWords);
            }

            // **THE 19:52 RUN IS WHY THIS LINE EXISTS.** Both parks templates wrote neither
            // computed number on any of their plots, the reason was on each of those plots'
            // fields, and nobody could see it without reading 82,048 lines. The reasons are
            // counted and said once here and the detail stays under each plot.
            Line(report, string.Empty);
            Line(report, "  " + glance.Computed.InWords);
            Line(report, "    " + glance.Computed.Greened.InWords);
            foreach (BlankedFor one in glance.Computed.Greened.Blanked) Line(report, "      " + one.InWords);
            Line(report, "    " + glance.Computed.Percentage.InWords);
            foreach (BlankedFor one in glance.Computed.Percentage.Blanked) Line(report, "      " + one.InWords);

            // **ANH-007-MO-100011 IS WHY THIS LINE EXISTS.** Its Area read 2797.6 cut off at the
            // box edge and its tree counts were drawn taller than their boxes. How big a value
            // was written is this tool's decision now, so it is counted here, and only the boxes
            // somebody has to look at are named one by one.
            Line(report, string.Empty);
            Line(report, "  " + glance.TextSizes.InWords);
            foreach (PdfFieldFit one in glance.TextSizes.Named) Line(report, "    " + one.InWords);
            foreach (PdfFieldFit one in glance.TextSizes.DidNotLand)
            {
                Line(report, "    " + one.FieldName + ": this run wrote "
                    + one.Size.ToString("0.###", CultureInfo.InvariantCulture)
                    + " pt into its /DA and the written file reads "
                    + (one.LandedRead
                        ? one.Landed.ToString("0.###", CultureInfo.InvariantCulture) + " pt"
                        : "no /DA at all")
                    + ". That is a bug in the tool.");
            }

            // **THE 16:37 PRESS FILED TEN PLOTS AT FIVE PATHS AND SAID NOTHING.** NS-01 and
            // NS-42 share one PRX_Plot_UID2 and MM-01 with MM-09 to MM-15 share another, so the
            // last plot written replaced the others and every one of their rows read YES.
            Line(report, string.Empty);
            Line(report, "  " + glance.Sharing);
            foreach (SharedUid2Group one in set.Sharing)
            {
                // **EVERY PLOT OF THE GROUP GETS ITS OWN LINE**, stopped or filed apart, because
                // a group can hold two plots colliding with each other and a third filed
                // somewhere else, and that third is named by neither a group wide line nor a
                // refusal it does not have.
                foreach (PlotFiling filed in one.Plots)
                {
                    string said = SharedUid2.Stops(set.Sharing, filed.PlotId)
                        ? SharedUid2.WhyStopped(set.Sharing, filed.PlotId)
                        : SharedUid2.FiledApartFrom(set.Sharing, filed.PlotId);

                    Line(report, "    " + filed.PlotId + ": " + said);
                }
            }

            // **MM-01, MM-06, MM-07 AND NS-23 READ 0 IN EVERY PLANTING BOX ON THE 16:37 PRESS**,
            // and the noughts are right: both of their schedules printed a heading and no rows.
            // The line is what separates them from a plot whose numbers happen to be small.
            Line(report, string.Empty);
            Line(report, "  " + glance.NoPlanting);

            // **A CHECK THAT SWITCHED ITSELF OFF READ EXACTLY LIKE ONE THAT PASSED.** Where a
            // template's canopy total column could not be read, the green cover used to be
            // written with no total canopy check at all and the reason was printed nowhere.
            Line(report, string.Empty);
            Line(report, "  " + glance.UnreadCanopy);
            foreach (UnreadCanopyColumn one in UnreadableCanopyColumns.In(set))
            {
                Line(report, "    " + one.InWords);
            }

            // **NONE OF THE TEMPLATE FAULTS SHOWED UNTIL A PLOT HIT A BAD ROW.** One line per
            // ticked template, and the cells themselves in their own section below.
            Line(report, string.Empty);
            Line(report, "  " + TreeListCheck.Heading + ":");
            foreach (string one in glance.TreeLists) Line(report, "    " + one);

            // **ALL 154 ROWS OF THE PLOT LIST READ YES FOUR TIMES ON THAT SAME PRESS**, with 41
            // blank green cover boxes and seven replaced workbooks among them. Ready is the
            // question the team is really asking and this is the count of it.
            Line(report, string.Empty);
            Line(report, "  " + glance.Ready);

            Line(report, string.Empty);
        }

        /// <summary>
        /// **EVERY CELL OF EVERY TICKED TEMPLATE'S TWO TREE LISTS THAT THIS TOOL CAN NAME.**
        /// Read once at the press, before any plot was written, so a team editing the templates
        /// sees what a press would hit rather than finding out one plot at a time.
        ///
        /// **IT STOPS NOTHING.** What the canopy guard and the total canopy check already stop
        /// is unchanged, and everything here is a line.
        /// </summary>
        private static void TheTreeLists(StringBuilder report, KpiCreateRunSet set)
        {
            if (set.TreeLists.Count == 0) return;

            Heading(report, TreeListCheck.Heading,
                set.TreeLists.Sum(one => one.Faults.Count),
                "both tree lists of every ticked template, read once at the press before any "
                + "plot was written");

            foreach (TreeListSheetCheck sheet in set.TreeLists)
            {
                Line(report, "  " + sheet.TemplateName + ", " + sheet.InWords);

                foreach (string one in sheet.NotRead) Line(report, "    NOT READ: " + one);

                foreach (IGrouping<string, TreeListFault> kind in sheet.Faults
                    .GroupBy(one => one.Kind, StringComparer.Ordinal))
                {
                    Line(report, "    " + kind.Key + ", " + kind.Count() + ":");
                    foreach (TreeListFault one in kind) Line(report, "      " + one.InWords);
                }
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// Templates ticked, written, refused and with nothing to write. **The four must add up
        /// to the number ticked**, and a run where they do not says so in those words, because
        /// a template that fell out of every branch would otherwise be a row nobody printed.
        /// </summary>
        private static void TheRunAccounting(StringBuilder report, KpiCreateRunSet set)
        {
            Heading(report, "THIS RUN, ACROSS EVERY TEMPLATE", set.Refusals.Count,
                "what was ticked and what came of it");

            Line(report, "  templates ticked                  " + set.TemplatesTicked);
            Line(report, "  templates written                 " + set.TemplatesWritten);
            Line(report, "  templates refused                 " + set.TemplatesRefused);
            Line(report, "  templates with nothing to write   " + set.TemplatesWithNothingToWrite);
            Line(report, "  those four add up to the ticked count   "
                + (set.CountsAddUp ? "YES" : "NO, WHICH IS A BUG IN THIS TOOL"));

            // **A CHECKLIST IS ONE PLOT**, so the count that matters is per plot. It is the one
            // the round asks for and the one that has to add up.
            Line(report, string.Empty);
            Line(report, "  plots ticked                      " + set.PlotsTicked);
            Line(report, "  folders made                      " + set.FoldersMade);
            Line(report, "  workbooks written                 " + set.WorkbooksWritten);
            Line(report, "  plots that wrote nothing          " + set.PlotsThatWroteNothing);
            Line(report, "  written plus wrote nothing is the ticked count   "
                + (set.PlotCountsAddUp ? "YES" : "NO, WHICH IS A BUG IN THIS TOOL"));
            Line(report, "  a folder is made where it is not there and is never deleted, so it is");
            Line(report, "  counted beside those two rather than among them");

            foreach (string refusal in set.Refusals) Line(report, "  REFUSED: " + refusal);

            Line(report, string.Empty);
            Line(report, "  " + set.Streets.InWords);

            Line(report, string.Empty);
            Line(report, "  template | plots | outcome");
            foreach (TemplateOutcome outcome in set.Outcomes)
            {
                Line(report, "  " + Join(
                    outcome.Template.Name,
                    outcome.Plots.Count.ToString(CultureInfo.InvariantCulture),
                    CreateWords.TemplateOutcomeRow(outcome)));
            }

            Line(report, string.Empty);
            ThePlots(report, set);
        }

        /// <summary>
        /// One row per ticked plot: its component, the folder it was filed under, its UID2 and
        /// the path that was written, or the reason nothing was.
        ///
        /// **Every ticked plot has a row**, including the ones no template took, because a plot
        /// list that goes in longer than it comes out is the failure this whole section exists
        /// to catch.
        /// </summary>
        private static void ThePlots(StringBuilder report, KpiCreateRunSet set)
        {
            Heading(report, "ONE WORKBOOK PER PLOT", set.PlotOutcomes.Count,
                "the folder tree is the root, the component folder, the plot's UID2, and the "
                + "workbook named after its folder");

            Line(report, "  plot | template | folder | UID2 | written to, or why not");
            foreach (PlotOutcome one in set.PlotOutcomes)
            {
                Line(report, "  " + Join(
                    one.PlotId,
                    Shown(one.TemplateName),
                    Shown(one.Where.Folder),
                    Shown(one.Where.Uid2),
                    one.Written ? one.Where.FilePath : one.Why));
            }

            Line(report, string.Empty);
            ThePdfs(report, set);

            var missed = set.PlotOutcomes
                .Where(one => !one.Written && one.Where.Uid2.Length > 0
                    && one.Template != null && one.Template.Name == KpiTemplates.Streets.Name)
                .ToList();

            Heading(report, "STREET PLOTS THE REFERENCE FILE COULD NOT ANSWER FOR", missed.Count,
                "their road width and total length cells are left empty and nothing is estimated "
                + "from the component value");
            foreach (PlotOutcome one in missed)
            {
                Line(report, "  " + Join(one.PlotId, one.Where.Uid2, one.Why));
            }

            Line(report, string.Empty);
        }

        public const string PdfHeading = "ONE PDF PER PLOT, BESIDE ITS WORKBOOK";

        /// <summary>
        /// Per plot: which form, which fields were written with the values that LANDED, which
        /// were left blank and why, and whether the form matched what this tool knows.
        ///
        /// **Everything here is read back off the file that was written**, never off the plan,
        /// which is the rule every written cell of the workbook already follows.
        /// </summary>
        public const string GroundCoverHeading = "SHRUBS AGAINST GROUND COVER, PER PLOT";

        /// <summary>
        /// The one group the form asks for in two boxes, split by the prefix its species names
        /// carry, with the group total the schedule printed beside it.
        ///
        /// **THE TOOL WROTE THE WHOLE GROUP INTO PROPOSED SHRUBS AND LEFT GROUND COVER BLANK**,
        /// and on DM-14 every species of that group is `GROUND COVER:` and none is `SHRUBS:`, so
        /// 468 m2 went into the wrong box of a client document. This section is what makes the
        /// split checkable against the schedule rather than trusted.
        ///
        /// **EVERY SPECIES THE SPLIT COULD PLACE IN NEITHER BOX IS NAMED**, with its plot, its
        /// row, its area and what its prefix read. Nothing is guessed into either figure, and
        /// the group total is what catches a species nobody noticed.
        /// </summary>
        private static void TheShrubsAndGroundCover(StringBuilder report, KpiCreateRunSet set)
        {
            var rows = set.Runs
                .SelectMany(one => one.Readings.Select(reading => new
                {
                    Reading = reading,
                    Split = ShrubsByPhase.Of(reading, KpiMerge.ShrubsHeading, CountedGroups.Of(one.Template), one.AreaUnit)
                }))
                .Where(one => one.Reading != null && one.Split.GroupFound)
                .ToList();

            Heading(report, GroundCoverHeading, rows.Count,
                "the group is " + KpiMerge.ShrubsHeading + " and it holds both. The prefix each "
                + "species name carries before its colon decides the box, and the two plus "
                + "anything placed nowhere must equal the group total the schedule printed");

            if (rows.Count == 0)
            {
                Line(report, "  No plot of this press printed that group.");
                Line(report, string.Empty);
                return;
            }

            var unplaced = rows.SelectMany(one => one.Split.ByPrefix.Unplaced).ToList();

            // **THE NAMES FIRST, BECAUSE THE NAMES ARE THE QUESTION.** Four plots of the 08:38
            // run lost their whole group to species the prefix rule could not place, and nothing
            // in that file said what those species were CALLED. A fifth prefix is a line in a
            // table and a naming mess is a different job, and only the names tell them apart.
            TheUnplacedNames(report, unplaced);

            // **A ROW WITH NO NAME THAT REACHED A CLIENT'S BOX IS NAMED HERE.** It sits beside
            // the names above because they are the same question asked twice: what the tool did
            // with a species it could not read off its name.
            TheDashRows(report, rows.SelectMany(one => one.Split.ByPrefix.DashRows).ToList());

            Line(report, "  plot | existing shrubs | proposed shrubs | ground cover | COULD NOT BE READ "
                + "| left out by this template | the four boxes would have read | group total printed "
                + "| adds up");

            foreach (var one in rows)
            {
                GroundCoverSplit split = one.Split.ByPrefix;

                Line(report, "  " + Join(
                    one.Reading.PlotId,
                    GroundCoverSplit.Area(split.ExistingShrubsSquareMetres),
                    GroundCoverSplit.Area(split.ProposedShrubsSquareMetres),
                    GroundCoverSplit.Area(split.GroundCoverSquareMetres),
                    split.Unplaced.Count == 0 ? "none" : GroundCoverSplit.Area(split.UnplacedSquareMetres),
                    split.OutOfScope.Count == 0 ? "none" : GroundCoverSplit.Area(split.OutOfScopeSquareMetres),
                    GroundCoverSplit.Area(split.WouldHaveWrittenSquareMetres),
                    split.GroupTotalPrinted ? GroundCoverSplit.Area(split.GroupTotal) : "none printed",
                    split.AddsUp ? "YES" : "NO, NOTHING WAS WRITTEN"));
            }

            if (unplaced.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  " + Count(unplaced.Count, "species row")
                    + " COULD NOT BE READ into either box, and every plot holding one wrote NOTHING "
                    + "into any of its four boxes. Nothing was guessed for any of them:");
                foreach (UnplacedSpecies species in unplaced)
                {
                    Line(report, "    " + species.InWords);
                }
            }

            // **READ AND DELIBERATELY LEFT OUT IS A DIFFERENT LIST AND NOT A REFUSAL.** Street
            // Design on a mosque plot is somebody else's scope by decision, so its area is a term
            // of the sum rather than an area nobody can account for. Keeping the two apart is
            // what stops the fix for a silent loss becoming a refusal on every plot that has one.
            var left = rows.SelectMany(one => one.Split.ByPrefix.OutOfScope).ToList();
            if (left.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  " + Count(left.Count, "species row")
                    + " was read and left out because this template does not take its phase. "
                    + "That is a decision rather than a fault, so the boxes are still written:");
                foreach (UnplacedSpecies species in left)
                {
                    Line(report, "    " + species.InWords);
                }
            }

            var refused = rows.Where(one => !one.Split.ByPrefix.AddsUp).ToList();
            if (refused.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  " + Count(refused.Count, "plot")
                    + " wrote none of the four shrub and ground cover boxes, each with why:");
                foreach (var one in refused)
                {
                    Line(report, "    " + Join(one.Reading.PlotId, one.Split.ByPrefix.Refusal));
                }
            }

            Line(report, string.Empty);
        }

        public const string WaterHeading = "THE IRRIGATION WATER DEMAND, PER PLOT";

        /// <summary>
        /// Both schedules' L/DAY TOTAL rows, added, and what the form was given.
        ///
        /// **IT IS ITS OWN SECTION AND NOT A LINE IN THE PDF BLOCK.** The PDF block prints what
        /// LANDED in a box. This prints where the number came from, which is two rows of two
        /// schedules a person can open, and the two questions are answered by different people.
        ///
        /// **THE TOTAL ROW COUNTS EVERY GROUP AND THE TREE LISTS DO NOT.** A mosque plot's
        /// Street Design group is out of scope for the tree lists by Bader's decision, and its
        /// water is still in the schedule's TOTAL. That may well be right, because the water is
        /// used whoever is paying for it, and it is not this tool's to decide. So the column is
        /// here: every plot says whether it had a group left out of its tree lists and what that
        /// group's own subtotal was, and the difference is on the page rather than found later.
        /// **The number written is Bader's, TAKE THE TOTAL, and nothing here subtracts anything.**
        /// </summary>
        private static void TheWaterDemand(StringBuilder report, KpiCreateRunSet set)
        {
            var readings = set.Runs
                .SelectMany(one => one.Readings)
                .Where(one => one != null)
                .ToList();

            Heading(report, WaterHeading, readings.Count,
                "both schedules' own TOTAL rows, added, then divided by 1000 because the form "
                + "asks " + WaterDemand.CubicMetresUnit + " and the schedules print "
                + WaterDemand.LitresUnit + ". Nothing adds the species rows up instead");

            if (readings.Count == 0)
            {
                Line(report, "  No plot was read in this press.");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  plot | softscape TOTAL | shrubs and lawn TOTAL | added | "
                + WaterDemand.CubicMetresUnit + " written | groups left out of the tree lists");

            foreach (PlotReading reading in readings)
            {
                PlotWaterDemand water = reading.Water;

                Line(report, "  " + Join(
                    reading.PlotId,
                    Half(water.Softscape),
                    Half(water.ShrubsAndLawn),
                    water.BothRead ? WaterDemand.Litres(water.LitresADay) : "-",
                    water.BothRead ? WaterDemand.CubicMetres(water.CubicMetresADay) : "NOTHING WRITTEN",
                    LeftOutOfTheTreeLists(reading)));
            }

            var refused = readings.Where(one => !one.Water.BothRead).ToList();
            if (refused.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  " + Count(refused.Count, "plot") + " wrote no water demand, each with why:");
                foreach (PlotReading reading in refused)
                {
                    Line(report, "    " + Join(reading.PlotId, reading.Water.Why));
                }
            }

            Line(report, string.Empty);
        }

        public const string UnplacedNamesHeading = "  THE SPECIES NO PREFIX PLACED, BY NAME";

        /// <summary>
        /// **EVERY DISTINCT NAME THE PREFIX RULE COULD NOT PLACE, WITH HOW MANY ROWS AND HOW MUCH
        /// AREA CARRY IT, GROUPED BY THE PREFIX EACH READ.**
        ///
        /// **It goes at the TOP of the section because it is the question rather than the
        /// detail.** `GRASS:`, `SHRUBS:` and `GROUND COVER:` are the three prefixes ever
        /// measured. If a run's unplaced rows all read one prefix nobody has seen, the answer is
        /// a line in a table and not a refusal, and that is the team's call to make once they
        /// can see the name. If they read a dozen different things, it is a naming job in the
        /// model. **The two look identical in a count and different in a list.**
        /// </summary>
        public const string DashRowsHeading = "  THE ROWS WHOSE BOTANICAL NAME IS A DASH, COUNTED AS SHRUBS";

        /// <summary>
        /// **EVERY DASH ROW, WITH THE BOX IT WENT INTO.** Bader's decision of 15 September counts
        /// a row whose botanical name is a single dash as SHRUBS and sends it by its phase, and
        /// 44 rows across the model carry one.
        ///
        /// **A row with no name reaching a client's box is exactly what this tool refuses
        /// everywhere else**, so it is allowed only on the record: the plot, the row, the phase,
        /// the area and the box. A person can open the schedule at that row and see what the
        /// tool counted.
        /// </summary>
        private static void TheDashRows(StringBuilder report, IReadOnlyList<DashRow> dashRows)
        {
            if (dashRows.Count == 0) return;

            Line(report, DashRowsHeading + " (" + dashRows.Count
                + (dashRows.Count == 1 ? " row, " : " rows, ")
                + GroundCoverSplit.Area(dashRows.Sum(one => one.SquareMetres)) + ")");
            Line(report, "  plot | row | phase | area | the box it went into");

            foreach (DashRow one in dashRows)
            {
                Line(report, "  " + Join(
                    one.PlotId,
                    one.RowNumber.ToString(CultureInfo.InvariantCulture),
                    one.Phase.Length == 0 ? "under no phase row" : one.Phase,
                    GroundCoverSplit.Area(one.SquareMetres),
                    one.Box));
            }

            Line(report, string.Empty);
        }

        private static void TheUnplacedNames(StringBuilder report, IReadOnlyList<UnplacedSpecies> unplaced)
        {
            if (unplaced.Count == 0) return;

            var byPrefix = unplaced
                .GroupBy(one => SpeciesPrefix.Of(one.BotanicalName), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(one => one.Count())
                .ToList();

            Line(report, UnplacedNamesHeading + " (" + byPrefix.Count
                + (byPrefix.Count == 1 ? " prefix)" : " prefixes)"));
            Line(report, "  the prefix each read | distinct names | rows | area | the names");

            foreach (var prefix in byPrefix)
            {
                var names = prefix
                    .GroupBy(one => one.BotanicalName, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(one => one.Count())
                    .ThenBy(one => one.Key, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                Line(report, "  " + Join(
                    prefix.Key.Length == 0 ? "(no colon in the name)" : prefix.Key,
                    names.Count.ToString(CultureInfo.InvariantCulture),
                    prefix.Count().ToString(CultureInfo.InvariantCulture),
                    GroundCoverSplit.Area(prefix.Sum(one => one.SquareMetres)),
                    string.Join(", ", names
                        .Select(one => one.Key + " x" + one.Count())
                        .ToArray())));
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// One half, the number and the row it came off, or why there is none. The row number is
        /// printed so a person can open that schedule and look at the row this read.
        /// </summary>
        private static string Half(WaterDemandRead read)
        {
            if (!read.Read) return "NOT READ, " + read.Why;

            return WaterDemand.Litres(read.LitresADay) + " off row "
                + read.TotalRow.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The groups this plot's softscape schedule printed that no tree list sheet is named
        /// for, each with the subtotal it printed. **Their water IS in the total that was
        /// written**, and this column is what lets that be checked rather than assumed.
        /// </summary>
        private static string LeftOutOfTheTreeLists(PlotReading reading)
        {
            var left = reading.PrintedGroups.Where(one => !one.Counted).ToList();
            if (left.Count == 0) return "none";

            return string.Join("; ", left
                .Select(one => one.Name + " " + (one.SubtotalPrinted
                    ? one.Subtotal.ToString(CultureInfo.InvariantCulture) + " trees"
                    : "printed no subtotal"))
                .ToArray());
        }

        private static void ThePdfs(StringBuilder report, KpiCreateRunSet set)
        {
            var held = set.PlotOutcomes.Where(one => one.Pdf != null).ToList();

            Heading(report, PdfHeading, held.Count,
                "the form is keyed on the plot prefix, the file is named after the same "
                + KpiNames.PlotUid2 + " as the workbook, and the form file's own bytes are "
                + "copied whole with the values appended after them");

            if (held.Count == 0)
            {
                Line(report, "  No PDF was planned in this press.");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  plot | form | written to, or why not | the form matched");
            foreach (PlotOutcome one in held)
            {
                Line(report, "  " + Join(
                    one.PlotId,
                    Shown(one.Pdf.FormName),
                    one.Pdf.Written ? one.Pdf.Path : one.Pdf.Refusal,
                    one.Pdf.Check == null ? "the file was never opened" : one.Pdf.Check.Matched ? "YES" : "NO"));
            }

            foreach (PlotOutcome one in held)
            {
                if (one.Pdf.Landed.Count == 0 && one.Pdf.Blank.Count == 0) continue;

                Line(report, string.Empty);
                Line(report, "  " + one.PlotId + ", " + Shown(one.Pdf.FormName));

                if (one.Pdf.Landed.Count > 0)
                {
                    // **THE UNIT IS PRINTED BESIDE EVERY WRITTEN VALUE.** A number in the wrong
                    // unit reads exactly like one nobody checked, and two of this form's fields
                    // are converted on the way in, the road length from metres to kilometres and
                    // the green cover from square metres to square kilometres.
                    Line(report, "    field | unit | what was sent | what landed");
                    foreach (PdfLandedField field in one.Pdf.Landed)
                    {
                        Line(report, "    " + Join(
                            field.FieldName,
                            Shown(field.Unit),
                            Shown(field.Sent),
                            Shown(field.Landed) + (field.Agrees ? string.Empty : "   THESE DIFFER")));
                    }
                }

                TheComputed(report, one.Pdf);
                TheEmptied(report, one.Pdf);

                if (one.Pdf.Blank.Count == 0) continue;

                Line(report, "    left blank | why");
                foreach (PdfFieldFill field in one.Pdf.Blank)
                {
                    Line(report, "    " + Join(field.FieldName, field.Why));
                }
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// The fields this run COMPUTED rather than read, each with the parts it was worked out
        /// from.
        ///
        /// **THEY ARE THE FIRST NUMBERS THIS TOOL PRODUCES THAT NO SCHEDULE PRINTED.** The
        /// workbook computes a total green cover and a canopy percentage of its own and the
        /// patcher drops every cached formula result on purpose, so neither is in the file the
        /// run wrote. Adding printed numbers with the working shown is already the rule here and
        /// this is that rule one step further, so the working is not a courtesy: it is the only
        /// way anybody can hold these two against the workbook once Excel has opened it.
        ///
        /// **And what each was held against in the workbook is said beside it.** The Total Green
        /// cover cell and the canopy percentage cell are found by their labels and read off the
        /// file, so the report names the cell, its formula and whether it agrees. A template with
        /// no canopy percentage cell, which is both templates this form is ever fed by, says so
        /// rather than reading as a check that passed.
        /// </summary>
        private static void TheComputed(StringBuilder report, PdfOutcome pdf)
        {
            var computed = pdf.Landed.Where(one => one.Computed).ToList();
            if (computed.Count == 0) return;

            Line(report, "    COMPUTED, not read | how");
            foreach (PdfLandedField field in computed)
            {
                Line(report, "    " + Join(field.FieldName, field.Working));
            }

            if (pdf.WhatWasChecked.Count == 0) return;

            Line(report, "    what the workbook was held against");
            foreach (string line in pdf.WhatWasChecked)
            {
                Line(report, "      " + line);
            }
        }

        /// <summary>
        /// Every field this run CLEARED, with why.
        ///
        /// **THE CLIENT'S DEFAULT VALUES ARE NOTES FOR WHOEVER FILLS THE FORM BY HAND.** Measured
        /// on the 18:15 run: 150 PDFs went out with `Revit / softscape &amp; shrubs &amp; lawn
        /// schedule / total water demand /1000` printed inside the irrigation box. Every text
        /// field the tool does not write is emptied now, and **a blank box is a decision on the
        /// record** rather than something a reader has to take on trust.
        /// </summary>
        private static void TheEmptied(StringBuilder report, PdfOutcome pdf)
        {
            if (pdf.Emptied.Count == 0) return;

            Line(report, "    EMPTIED, " + pdf.Emptied.Count + " | why | what landed");
            foreach (PdfLandedField field in pdf.Emptied)
            {
                Line(report, "    " + Join(
                    field.FieldName,
                    field.Why,
                    field.Landed.Length == 0 ? "empty" : Shown(field.Landed) + "   STILL HOLDS THIS"));
            }
        }

        /// <summary>
        /// Which plots went to which template and by which route, and every ticked plot that
        /// went nowhere with why. **A plot whose template is not ticked is not read and is named
        /// here**, so a plot list that goes in longer than it comes out is visible at the top of
        /// the file rather than worked out from the per template sections.
        /// </summary>
        private static void TheSplit(StringBuilder report, KpiCreateRunSet set)
        {
            Heading(report, "WHICH PLOT WENT INTO WHICH WORKBOOK", set.Split.Answers.Count,
                "PRX_Component decides, the plot prefix is a cross check, and where they disagree neither does");

            Line(report, "  plot | template | route | why");
            foreach (PlotTemplate answer in set.Split.Answers)
            {
                Line(report, "  " + Join(
                    answer.PlotId,
                    answer.Template == null ? "NO WORKBOOK" : answer.Template.Name,
                    answer.Route.ToString(),
                    answer.Why));
            }

            Line(report, string.Empty);

            Heading(report, "PLOTS TICKED THAT WENT INTO NO WORKBOOK", set.Split.Unplaced.Count,
                "named with the reason, never dropped in silence");
            foreach (PlotTemplate answer in set.Split.Unplaced)
            {
                Line(report, "  " + Join(answer.PlotId, answer.Why));
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// The plot a run is for, off its own reconciliation, which is the one record of which
        /// plots the run covered. Empty where the run carries none, said rather than guessed.
        /// </summary>
        private static string OnePlotOf(KpiCreateRun run)
        {
            return run.Reconciliation == null || run.Reconciliation.Ticked.Count == 0
                ? "UNKNOWN"
                : string.Join(", ", run.Reconciliation.Ticked.ToArray());
        }

        public static string Write(KpiCreateRun run, DateTime writtenAt)
        {
            return Write(run, writtenAt, true);
        }

        /// <summary>
        /// One run's sections. <paramref name="withTheFilePreamble"/> is false for a block
        /// inside the whole press's file, where the document, the time and the read only line
        /// are already at the top and repeating them 156 times says nothing.
        /// </summary>
        public static string Write(KpiCreateRun run, DateTime writtenAt, bool withTheFilePreamble)
        {
            if (run == null) throw new ArgumentNullException("run");

            var report = new StringBuilder();

            if (withTheFilePreamble)
            {
                Line(report, "RCRC Green KPI checklist");
                Line(report, "Document: " + Shown(run.DocumentTitle));
                Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
                Line(report, "Read only. Nothing in the model was changed and the template was not touched.");
            }

            TheClock(report, run);
            TheLinks(report, run);
            TheCoarseStep(report, run);
            Line(report, "Every schedule this run read is printed as the schedule prints it, at the end of this");
            Line(report, "file under " + SchedulesHeading + ", so every number above it can be held against the drawing.");
            Line(report, string.Empty);

            TheReconciliation(report, run);
            ThePlots(report, run);
            TheCellsWritten(report, run);
            TheCellsWrittenOver(report, run);
            TheCellsNotWritten(report, run);
            TheFormulas(report, run);
            TheSpecies(report, run);
            TheTreeLists(report, run);
            TheChoices(report, run);
            TheSchedules(report, run);

            return report.ToString();
        }

        /// <summary>
        /// How long it took, at the top, because the scan report has carried elements and
        /// seconds since its first round and the other half of the tool carried nothing.
        ///
        /// **A run over 20 plots took about five minutes and no file recorded it.** The read is
        /// printed apart from the rest because the read is the part that grows with the plots
        /// ticked, and a total on its own cannot say which half to look at.
        /// </summary>
        private static void TheClock(StringBuilder report, KpiCreateRun run)
        {
            if (!run.Timing.WasTimed)
            {
                Line(report, "Run: NOT TIMED. Nothing recorded a duration for this run.");
                TheReadings(report, run);
                return;
            }

            Line(report, "Run: " + Seconds(run.Timing.TotalSeconds) + ", of which reading the model took "
                + Seconds(run.Timing.ReadSeconds) + " and everything after it "
                + Seconds(run.Timing.RestSeconds) + ".");
            TheReadings(report, run);
            Line(report, "Every plot's own read is beside it under EVERY PLOT THAT WENT IN.");
        }

        /// <summary>
        /// **The reason a run found nothing, at the top, before the reconciliation.** The
        /// 16:06 STREETS run over 78 plots ended with 78 identical lines saying each plot
        /// contributed nothing, and the one fact that explained all of them, that not one of
        /// the six link instances was loaded, appeared nowhere in this file. The scan report
        /// said it in section 4 and the checklist report said nothing about links at all.
        ///
        /// A note and never a refusal: a model with no link loaded is a legitimate thing to
        /// open, and the run still reads and still writes what it can.
        /// </summary>
        private static void TheLinks(StringBuilder report, KpiCreateRun run)
        {
            if (!run.Links.Worth) return;

            Line(report, run.Links.Warning);
        }

        /// <summary>
        /// One line before anybody reads a number, only when the project rounds areas coarser
        /// than the metre. Both measured models round to 1, so a coarser step is hypothetical
        /// and the room has no ceiling yet: a ceiling chosen today would be a constant
        /// pretending to be a rule. The first project that prints this line hands the team a
        /// real figure to decide one against.
        /// </summary>
        private static void TheCoarseStep(StringBuilder report, KpiCreateRun run)
        {
            double step = run.AreaUnit.Accuracy;
            if (double.IsNaN(step) || step <= 1.0) return;

            double room = step / 2.0;
            Line(report, "THE PROJECT ROUNDS AREAS COARSER THAN THE METRE: its step is " + KpiReport.Step(step)
                + ", so the group total check allows " + Fine(room) + " square metre"
                + (room == 1.0 ? string.Empty : "s") + " of room for every row summed.");
        }

        /// <summary>
        /// A read of 0.0 seconds is true on a press that reused the run before, and only this
        /// line under it says why, so the two stay together.
        /// </summary>
        private static void TheReadings(StringBuilder report, KpiCreateRun run)
        {
            Line(report, "Readings: " + (run.ReadingsSource.Reused
                ? "reused, nothing was read from the model on this press, "
                : "read from the model on this press, ") + run.ReadingsSource.Why + ".");
        }

        private static void TheReconciliation(StringBuilder report, KpiCreateRun run)
        {
            Reconciliation held = run.Reconciliation;

            Heading(report, "RECONCILIATION", held.Refusals.Count,
                held.AddsUp
                    ? "everything ticked is accounted for below"
                    : "reasons the workbook was not written");

            foreach (string refusal in held.Refusals) Line(report, "  " + refusal);
            if (held.Refusals.Count > 0) Line(report, string.Empty);

            Line(report, "  plots ticked            " + held.Ticked.Count);
            Line(report, "  plots read              " + held.Read.Count);
            Line(report, "  " + Counted("plots with one softscape schedule",
                held.Read.Count - held.WithoutSoftscape.Count - held.WithMoreThanOneSoftscape.Count,
                held.WithoutSoftscape, held.WithMoreThanOneSoftscape));
            Line(report, "  " + Counted("plots with one shrubs and lawn schedule",
                held.Read.Count - held.WithoutShrubsAndLawn.Count - held.WithMoreThanOneShrubsAndLawn.Count,
                held.WithoutShrubsAndLawn, held.WithMoreThanOneShrubsAndLawn));
            Line(report, held.AreaWanted
                ? "  " + Counted("plots with an area", held.Read.Count - held.WithoutArea.Count, held.WithoutArea)
                : "  plots with an area   " + AreaNotRead);

            // One line that would have shown Street Design on the first twenty plot run rather
            // than the fourth.
            Line(report, "  schedules holding a group no tree list sheet is named for   " + held.SchedulesWithAGroupLeftOut
                + (held.WithAGroupLeftOut.Count == 0 ? string.Empty : ", on " + string.Join(", ", held.WithAGroupLeftOut.ToArray()))
                + (held.SchedulesWithAGroupLeftOut == 0 ? string.Empty : ". Their rows are left out and named under the plot."));

            // The two a zero cannot fake. The line above reads 0 when the rule worked and 0
            // when nothing was found at all, and on the 16:06 run it read green over a run
            // where all 156 schedules printed one row, the header, and no body.
            Line(report, "  group rows found across the run   " + held.GroupRowsFound);
            Line(report, "  schedules that printed a body   " + held.SchedulesWithABody
                + " of " + held.SchedulesPrinted);

            Line(report, "  plots that contributed nothing at all   " + held.ContributedNothing.Count);
            foreach (PlotAndReason nothing in held.ContributedNothing)
            {
                Line(report, "    " + nothing.PlotId + ": " + nothing.Reason);
            }

            if (held.IdenticalAreas.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, "  PLOTS REPORTING AN IDENTICAL AREA, " + held.IdenticalAreas.Count);
                Line(report, "  Either they are the same size or one region is counted twice.");
                foreach (IdenticalArea shared in held.IdenticalAreas)
                {
                    Line(report, "    " + string.Join(", ", shared.Plots.ToArray())
                        + " all read " + shared.Printed
                        + ", raw " + shared.RawSquareFeet.ToString("R", CultureInfo.InvariantCulture));
                }
            }

            Line(report, string.Empty);
            if (held.AreaWanted)
            {
                TheWorking(report, "AREA, SQUARE METRES", run.Area);
            }
            else
            {
                Line(report, "  AREA, SQUARE METRES, " + AreaNotRead);
                Line(report, string.Empty);
            }

            TheWorking(report, "SHRUBS, SQUARE METRES", run.Shrubs);
            TheWorking(report, "LAWN, SQUARE METRES", run.Lawn);
        }

        /// <summary>
        /// Said wherever the area would have printed on a template that takes none, so the
        /// section reads as a read that did not happen and never as a plot with no area. The
        /// words are the pane's own line for the same condition, with why nothing was refused.
        /// </summary>
        public static readonly string AreaNotRead = "not read. " + CreateWords.TakesNoArea;

        /// <summary>
        /// Each plot's own number and the total underneath it, so the arithmetic can be checked
        /// by eye without opening Revit.
        /// </summary>
        private static void TheWorking(StringBuilder report, string what, Totalled total)
        {
            Line(report, "  " + what + ", " + Count(total.PerPlot.Count, "plot"));
            foreach (PlotNumber one in total.PerPlot)
            {
                Line(report, "    " + one.PlotId + " | " + Number(one.Value));
            }

            Line(report, "    TOTAL | " + Number(total.Total)
                + (total.Adds ? string.Empty : "   THE PARTS ADD TO " + Number(total.Sum)
                    + ", WHICH IS A BUG IN THIS TOOL"));
            Line(report, string.Empty);
        }

        private static void ThePlots(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "EVERY PLOT THAT WENT IN", run.Readings.Count, "and what each contributed");

            foreach (PlotReading reading in run.Readings)
            {
                Line(report, "  " + reading.PlotId);
                Line(report, "    " + run.ComponentParameter + ": " + Shown(reading.Component));
                Line(report, "    " + run.ReferenceParameter + ": " + Shown(reading.Reference));

                // Which schedule each number came off, by name. FM-05 holds two whose names
                // hold SOFTSCAPE and a line saying only that species rows were read could not
                // show which, or that both had been.
                Line(report, "    " + Schedules("softscape", reading.SoftscapeSchedules,
                    Count(reading.Species.Count, "species row") + " read"));

                // The species rows against the TOTAL the schedule printed, so a row the reader
                // dropped is visible here rather than only in a workbook short of trees.
                if (reading.SoftscapeRead)
                {
                    Line(report, "      species rows add to " + reading.SpeciesSum
                        + (reading.LeftOutSum > 0 ? ", " + reading.LeftOutSum + " left out" : string.Empty) + ", "
                        + (reading.SoftscapeTotalRead
                            ? "the TOTAL row prints " + reading.SoftscapeTotal
                                + (reading.SoftscapeTotalRow > 0 ? " at row " + reading.SoftscapeTotalRow : string.Empty)
                            : "no TOTAL row was found to hold that against"));
                    if (reading.SoftscapeRowsPassedOver > 0)
                    {
                        Line(report, "      " + Count(reading.SoftscapeRowsPassedOver, "row")
                            + " with a count and no botanical name passed over, the subtotals");
                    }

                    // Every group row, always, in the order printed, taken or left out and why.
                    // A schedule with the ordinary two reads differently from one nobody has
                    // checked, and the third group on FM-05 went unseen for four runs without
                    // this.
                    Line(report, "      group rows: " + reading.PrintedGroups.Count);
                    foreach (PrintedGroup group in reading.PrintedGroups)
                    {
                        Line(report, "        row " + group.RowNumber + " " + group.Name + ": "
                            + Count(group.Species.Count, "species row") + " adding to " + group.SpeciesSum + ", "
                            + (group.SubtotalPrinted ? "subtotal row " + group.SubtotalRow + " prints " + group.Subtotal : "no subtotal row")
                            + ", " + (group.Counted ? "TAKEN, " : "LEFT OUT, ") + group.Why);
                        if (group.Counted) continue;

                        Line(report, "          left out: " + string.Join(", ", group.Species
                            .Select(one => one.BotanicalName + " " + one.Quantity.ToString(CultureInfo.InvariantCulture)).ToArray()));
                    }

                    // A species on two rows under one group is what FM-05 10, FM-05 10 was, and
                    // it is said here beside the plot rather than left to be read off a merged row.
                    foreach (RepeatedSpecies repeated in reading.SpeciesPrintedOnMoreThanOneRow)
                    {
                        Line(report, "      " + repeated.BotanicalName + " under " + repeated.GroupName
                            + " is printed on " + repeated.Rows.Count + " rows, " + repeated.InWords);
                    }
                }

                Line(report, "    " + Schedules("shrubs and lawn", reading.ShrubsAndLawnSchedules,
                    Count(reading.Subtotals.Count, "group") + " read"));

                foreach (string refused in reading.ReadRefusals)
                {
                    Line(report, "    REFUSED: " + refused);
                }

                foreach (GroupSubtotal subtotal in reading.Subtotals)
                {
                    Line(report, "      " + subtotal.Heading + " | " + Number(subtotal.SquareMetres)
                        + " | " + subtotal.ItemCount + " items"
                        + (subtotal.Phases.Count == 0 ? string.Empty : ", the phase rows taken added together"));
                    foreach (PhaseSubtotal phase in subtotal.Phases)
                    {
                        Line(report, "        row " + phase.RowNumber + " " + phase.Name + ": " + Number(phase.SquareMetres)
                            + " over " + phase.ItemCount + ", " + (phase.Counted ? "TAKEN, " : "LEFT OUT, ") + phase.Why);
                    }

                    if (subtotal.Phases.Count > 0 && subtotal.GroupTotalPrinted)
                    {
                        Line(report, "        group total row " + subtotal.RowNumber + " prints " + Number(subtotal.GroupTotalSquareMetres)
                            + " over " + subtotal.GroupTotalItemCount + ", "
                            + (subtotal.RoundingNote.Length > 0
                                ? "and " + subtotal.RoundingNote
                                : subtotal.Agrees ? "and the phase rows taken and left out add to it" : "AND THE PHASE ROWS DO NOT ADD TO IT"));
                    }
                    else if (subtotal.RoundingNote.Length > 0)
                    {
                        // A one phase group carries the same note against its own last row.
                        Line(report, "        " + subtotal.RoundingNote);
                    }
                    else if (subtotal.Phases.Count > 0)
                    {
                        // A phased group that prints no total row gives the check nothing to
                        // hold the phase rows against. Taken silently it reads exactly like a
                        // group whose total was checked and agreed, so the skip is said.
                        Line(report, "        the group printed no total row after its phase rows, "
                            + "so nothing checked what they add to");
                    }

                    // The species rows against the group's own value, recorded rather than
                    // enforced, the forty seventh pass's decision. The docstring said printed
                    // and nothing printed it, so a drift here was invisible until this line.
                    // A record needs no room, only a guard against the last bits of a double,
                    // so any real difference is said. The 0.005 that used to sit here was a
                    // constant pretending to be a rounding room, which is the very shape the
                    // group total check was just cured of.
                    double against = subtotal.GroupTotalPrinted ? subtotal.GroupTotalSquareMetres : subtotal.SquareMetres;
                    if (!double.IsNaN(subtotal.SpeciesSum)
                        && Math.Abs(subtotal.SpeciesSum - against) > Totalled.Tolerance * Math.Max(1.0, Math.Abs(against)))
                    {
                        Line(report, "        its species rows add to " + Fine(subtotal.SpeciesSum) + " in area against the "
                            + Fine(against)
                            + (subtotal.GroupTotalPrinted ? " its group total row prints" : " it holds")
                            + ", recorded rather than enforced, because every printed area is already rounded");
                    }
                }

                RegionArea chosen = reading.ChosenRegion;
                Line(report, "    area: " + (!run.Reconciliation.AreaWanted
                    ? "not read, the template takes none"
                    : chosen == null
                        ? "none chosen"
                        : chosen.TypeName + " | " + Number(chosen.SquareMetres) + " | raw "
                            + chosen.RawSquareFeet.ToString("R", CultureInfo.InvariantCulture)));

                Line(report, "    read in " + Seconds(reading.ReadSeconds));

                foreach (string note in reading.Notes) Line(report, "    " + note);
                Line(report, string.Empty);
            }
        }

        private static void TheCellsWritten(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<LandedCell> landed = run.Outcome == null
                ? new List<LandedCell>()
                : run.Outcome.Landed;

            Heading(report, "CELLS WRITTEN", landed.Count, run.Wrote
                ? "every one read back off the output file, never as it was sent"
                : Refused);

            if (!run.Wrote)
            {
                Line(report, "  " + (run.Outcome == null
                    ? "The accounting refused this run, so no file was copied."
                    : run.Outcome.Refusal));
                Line(report, "  " + Count(run.Plan == null ? 0 : run.Plan.Writes.Count, "cell")
                    + " would have been written.");
                Line(report, string.Empty);
                return;
            }

            Line(report, "  sheet | cell | value as it landed");
            foreach (LandedCell cell in landed)
            {
                Line(report, "  " + cell.SheetName + " | " + cell.Cell + " | " + Shown(cell.Value));
            }

            Line(report, string.Empty);
            Line(report, "  parts in the template " + run.Outcome.PartsInSource
                + ", parts in the output " + run.Outcome.PartsInOutput
                + ", " + Count(run.Outcome.ChangedParts.Count, "part") + " changed");

            // One part fewer is expected and the reason is said, so the count does not read as
            // a loss. Anything else short is the resave failure wearing this tool's name.
            if (run.Outcome.PartsDeliberatelyRemoved > 0)
            {
                Line(report, "  " + WorkbookPatcher.CalcChainPart + " was removed on purpose, which is "
                    + "the one part fewer. It is Excel's record of");
                Line(report, "  what order to work the formulas out in, written against the cached results "
                    + "this run dropped.");
                Line(report, "  Excel rebuilds it on the first recalculation.");
            }

            if (!run.Outcome.KeptEveryPart)
            {
                Line(report, "  THE OUTPUT DOES NOT HOLD EVERY PART THE TEMPLATE HELD. That is a bug.");
            }

            TheCache(report, run.Outcome.Cache);
            Line(report, string.Empty);
        }

        /// <summary>
        /// **Every cell this run wrote that was not empty, and what it held.**
        ///
        /// Measured on 14 September: both park templates already hold the text
        /// " Architect Engineer" at H5, which is the position cell, and the mosque template came
        /// holding Urban Area Zone at D7 and Urban at F7. The run writes over all of them, which
        /// is right, and **overwriting somebody's text has to be visible rather than silent.**
        ///
        /// Only cells found by a label are here, because those are the ones whose previous
        /// contents are read on the way to choosing them, and only cells this run really wrote,
        /// so a template naming no label is under CELLS NOT WRITTEN with its own reason instead.
        ///
        /// The column header is printed only when there is a row under it, because a header over
        /// an empty table is the shape the third audit counted six of.
        /// </summary>
        private static void TheCellsWrittenOver(StringBuilder report, KpiCreateRun run)
        {
            List<LabelledCell> over = WrittenOver(run);

            Heading(report, "CELLS WRITTEN OVER SOMETHING THE TEMPLATE ALREADY HELD", over.Count,
                "each one found by its label, with what the template held there before this run");

            if (over.Count > 0)
            {
                Line(report, "  value | label | label cell | cell written | what it held");
                foreach (LabelledCell cell in over)
                {
                    Line(report, "  " + Join(
                        cell.Name, cell.Label, cell.LabelCell, cell.ValueCell, cell.Holds));
                }
            }

            Line(report, string.Empty);
        }

        private static List<LabelledCell> WrittenOver(KpiCreateRun run)
        {
            KpiCreatePlan plan = run.Plan;
            if (plan == null) return new List<LabelledCell>();

            var written = new HashSet<string>(
                plan.Writes
                    .Where(one => string.Equals(one.SheetName, plan.Template.MainSheetName, StringComparison.Ordinal))
                    .Select(one => one.Cell.ToString()),
                StringComparer.OrdinalIgnoreCase);

            return plan.Labelled
                .Where(one => one.WasNotEmpty && written.Contains(one.ValueCell))
                .ToList();
        }

        /// <summary>
        /// Whether the output will really recalculate, read back off it.
        ///
        /// **Excel showed 0 for seven computed cells while the inputs beside them were right.**
        /// The values were never wrong: the template's own cached results were still there and
        /// Excel trusted them. fullCalcOnLoad was already on, so the flag alone is not the fix,
        /// and this prints all three things that are.
        /// </summary>
        private static void TheCache(StringBuilder report, CacheCheck cache)
        {
            Line(report, string.Empty);
            Line(report, "  WILL EXCEL RECALCULATE THIS FILE: " + (cache.WillRecalculate ? "YES" : "NO")
                + ", five things checked off the output file, over what the file says and not over what Excel does with it");
            Line(report, "    recalculate on open   " + (cache.RecalculatesOnOpen ? "set" : "NOT SET"));
            Line(report, "    calcId                " + Shown(cache.CalcId)
                + (cache.CalcIdCleared ? string.Empty : "   NOT CLEARED, so Excel may trust the cache"));
            Line(report, "    cached results left   " + cache.FormulaCellsCarryingACachedValue
                + ", dropped " + cache.CachedValuesDropped);
            Line(report, "    " + WorkbookPatcher.CalcChainPart + "     "
                + (cache.CalcChainRemoved ? "removed" : "STILL THERE"));

            // The 1428 file passed the four above and opened into a manual session showing every
            // formula cell blank, because calcPr carried no calcMode and Excel takes the mode
            // from the first workbook it opens.
            Line(report, "    calcMode              " + (cache.CalcMode.Length == 0 ? "NOT SET" : cache.CalcMode)
                + (cache.CalcModeAuto ? string.Empty : "   NOT AUTO, so a manual Excel session opens it without calculating"));
            Line(report, "    other calculation settings in the package: "
                + (cache.OtherCalculationSettings.Count == 0 ? "none found" : cache.OtherCalculationSettings.Count.ToString(CultureInfo.InvariantCulture)));
            foreach (string setting in cache.OtherCalculationSettings) Line(report, "      " + setting);
            Line(report, "      looked for: " + CacheCheck.LookedFor);

            if (cache.WillRecalculate) return;

            Line(report, "  THE FILE MAY OPEN SHOWING THE TEMPLATE'S OWN CACHED NUMBERS RATHER THAN THESE.");
            Line(report, "  Press Ctrl Alt F9 in Excel to force it, and treat this as a bug in the tool.");
        }

        /// <summary>
        /// What the output's formulas will make of the cells that landed, read off the output's
        /// own text. This is the section that would have caught the canopy fault without anyone
        /// opening Excel: a written row's neighbouring formula returned a space and the cell
        /// beside it multiplied that space by the count.
        /// </summary>
        private static void TheFormulas(StringBuilder report, KpiCreateRun run)
        {
            FormulaCheck check = run.Outcome == null ? FormulaCheck.NotChecked : run.Outcome.Formulas;

            // **THE HEADING COUNT AND THE BODY COME OFF ONE LIST.** The heading used to count
            // the errors and the body printed every risk, so the 18:15 report carried a heading
            // of (0) over 526 lines. A section nobody can trust hides the real risk inside it.
            IReadOnlyList<RepeatedFormula> risks = FormulaRepeats.Of(check.AtRisk);

            Heading(report, FormulasHeading, risks.Count,
                "formula shapes at risk on a row this run wrote into, checked off the output file over what its formulas read and never by evaluating one");

            if (!check.WasChecked)
            {
                Line(report, "  not checked. " + (run.Outcome == null
                    ? "The accounting refused this run, so no file was written to check."
                    : "The patch was refused before a file was written."));
                Line(report, string.Empty);
                return;
            }

            Line(report, "  formulas in the output   " + check.FormulaCount
                + ", of which " + check.ReadingWrittenRows.Count + " read a cell on a row this run wrote into");
            if (check.RefusesTheWrite)
            {
                Line(report, "  THIS RUN IS REFUSED ON WHAT FOLLOWS. " + check.Refusal);
            }

            Line(report, string.Empty);
            Line(report, "  FORMULAS AT RISK, " + risks.Count
                + ": a formula returning text where a number was expected, the #VALUE! that arithmetic on it gives,");
            Line(report, "  a #DIV/0! off a divisor holding nought or nothing, and every formula that reads one of them.");
            Line(report, "  A divide by zero is REPORTED and never refused on, and its line says whether this run wrote the cell it divides by.");

            // **ONLY WHAT THE RUN IS ANSWERABLE FOR, AND ONE LINE PER SHAPE.** The 18:15 report
            // printed 526, most of them G31 to G36 saying one sentence per row per template
            // about cells this run never wrote into. A formula the run did not affect is not at
            // risk from the run, and the same formula filled down a column is one finding.
            Line(report, "  Only formulas reading a cell on a row this run wrote into are here. "
                + check.AtRisk.Count + " were found in all, of which "
                + FormulaRepeats.FromWhatTheRunWrote(check.AtRisk).Count + " read such a cell.");
            Line(report, "  sheet | cells | formula | why");
            foreach (RepeatedFormula one in risks)
            {
                Line(report, "  " + Join(one.SheetName, one.Where, one.Text,
                    one.Reason + (one.Repeats
                        ? "   THE SAME SHAPE ON " + one.Cells.Count + " CELLS, said once"
                        : string.Empty)));
            }

            Line(report, string.Empty);
            Line(report, "  THE CELLS THE WORKBOOK COMPUTES FROM, " + check.ComputesFrom.Count
                + ", every cell the map names on the main sheet and whether the formulas reading it have their inputs");
            foreach (ComputedFrom one in check.ComputesFrom)
            {
                Line(report, "  " + Join(
                    one.Input.ToString(),
                    one.Present ? "present, holds " + Shown(one.Holds) : "NOT PRESENT, nothing was written there",
                    "read by " + Count(one.Readers.Count, "formula")));
                foreach (string reader in one.Readers) Line(report, "      " + reader);
                foreach (string blank in one.BlankInputs) Line(report, "      " + blank);
            }

            // **ONE FORMULA FILLED DOWN A COLUMN IS ONE FINDING HERE TOO.** This list was one
            // line per cell, 321 of them on one plot of the 18:15 run, and the per plot fix
            // multiplied it by 156. It is the same three column tree list formulas repeating,
            // and the rule that already governs the risks above governs it now.
            IReadOnlyList<RepeatedFormula> reading = FormulaRepeats.Reading(check.ReadingWrittenRows);

            Line(report, string.Empty);
            Line(report, "  FORMULAS READING A ROW THIS RUN WROTE INTO, " + check.ReadingWrittenRows.Count
                + " over " + reading.Count + (reading.Count == 1 ? " shape" : " shapes") + ", said once per shape");
            Line(report, "  sheet | cells | formula | reads");
            foreach (RepeatedFormula one in reading)
            {
                Line(report, "  " + Join(one.SheetName, one.Where, one.Text,
                    one.Reason + (one.Repeats
                        ? "   THE SAME SHAPE ON " + one.Cells.Count + " CELLS, said once"
                        : string.Empty)));
            }

            Line(report, string.Empty);
            Line(report, "  FUNCTIONS THE READER'S EXCEL MAY NOT HAVE, " + check.FunctionsExcelMayNotHave.Count
                + ". The file stores a function with the _xlfn. prefix when an older Excel does not have it,");
            Line(report, "  and such a cell reads #NAME? there. The tool writes no formula and did not put these here.");
            foreach (FunctionUse one in check.FunctionsExcelMayNotHave)
            {
                Line(report, "    _xlfn." + one.Name + " in " + Count(one.Cells, "cell")
                    + ". Those cells need a version of Excel that has " + one.Name + ".");
            }

            Line(report, string.Empty);
        }

        private static void TheCellsNotWritten(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<NotWritten> skipped = run.Plan == null
                ? new List<NotWritten>()
                : run.Plan.Skipped;

            Heading(report, "CELLS NOT WRITTEN", skipped.Count, "each with the reason");

            foreach (NotWritten one in skipped)
            {
                Line(report, "  " + Join(
                    Shown(one.SheetName),
                    one.Cell.Length == 0 ? "-" : one.Cell,
                    one.What,
                    one.Why));
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// How a matched species reached its row. **A count that arrived through an alias must
        /// never read the same as one that matched word for word**, because the first rests on
        /// a decision of the team's and the second on the name the model printed.
        /// </summary>
        private static string How(SpeciesMatch match)
        {
            return match.ThroughAnAlias
                ? "THROUGH THE ALIAS " + match.Alias.InWords
                : "matched on its own name";
        }

        /// <summary>
        /// Every count resting on an alias, in its own block with the row it reached, so
        /// somebody reading the report can see which numbers rest on a decision rather than on
        /// a name without reading every line of the section above.
        /// </summary>
        private static void TheAliases(StringBuilder report, IEnumerable<SpeciesMatch> matched)
        {
            List<SpeciesMatch> through = matched.Where(one => one.ThroughAnAlias).ToList();

            Heading(report, "SPECIES MATCHED THROUGH AN ALIAS", through.Count,
                "the name the model prints is not the name the list holds, and the team said the two are one thing");
            Line(report, "  Revit name | the alias | sheet | row | merged");
            foreach (SpeciesMatch match in through)
            {
                Line(report, "  " + Join(
                    match.Species.BotanicalName,
                    match.Alias.InWords,
                    match.SheetName,
                    KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture)));
            }

            Line(report, string.Empty);
        }

        private static void TheSpecies(StringBuilder report, KpiCreateRun run)
        {
            IReadOnlyList<SpeciesMatch> matches = run.Plan == null
                ? new List<SpeciesMatch>()
                : run.Plan.Matches;

            List<SpeciesMatch> matched = matches.Where(one => one.Matched).ToList();
            List<SpeciesMatch> unreached = matches.Where(one => one.NotReachedByTheTotal).ToList();
            List<SpeciesMatch> missed = matches.Where(one => !one.Matched && !one.NotReachedByTheTotal).ToList();

            Heading(report, "SPECIES MATCHED", matched.Count,
                "the workbook row against the merged count, with the plots it came from");
            Line(report, "  sheet | row | workbook name | Revit name | group | merged | from | how");
            foreach (SpeciesMatch match in matched)
            {
                Line(report, "  " + Join(
                    match.SheetName,
                    KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture),
                    match.WorkbookName,
                    match.Species.BotanicalName,
                    match.Species.GroupName,
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    How(match)));
            }

            Line(report, string.Empty);

            TheAliases(report, matched);

            // Two numbers for one species, the client's and the model's. Named, and the client's
            // row left as it is, because which is right is a question for the team.
            IReadOnlyList<MeasureDifference> differences = run.Plan == null
                ? new List<MeasureDifference>()
                : run.Plan.Differences;
            Heading(report, "MATCHED SPECIES WHOSE HEIGHT OR DIAMETER IN REVIT DIFFERS FROM THE ROW'S", differences.Count,
                "named and CHANGED NOTHING, the workbook's row keeps its own number");

            // How many of the matches disagree, so the size of it is visible without counting.
            // The 1707 run read 22 of its matches differing, nearly every one.
            int differing = differences.Select(one => one.SheetName + "!" + one.Row.ToString(CultureInfo.InvariantCulture)).Distinct().Count();
            Line(report, "  " + differing + " of " + matched.Count + " matched species"
                + " differ in a height, a diameter or both, and nothing was changed on any row");
            Line(report, "  sheet | row | workbook name | what | Revit prints | the row holds");
            foreach (MeasureDifference one in differences)
            {
                Line(report, "  " + Join(one.SheetName, one.Row.ToString(CultureInfo.InvariantCulture),
                    one.WorkbookName, one.What, one.RevitPrints, one.WorkbookHolds, "CHANGED NOTHING"));
            }

            Line(report, string.Empty);

            // The list holds the name and its total does not reach the row. The count is not
            // written there, because a count on the sheet that no total adds reads as complete
            // and is short, and this is where a person sees which rows the client's total is
            // short of.
            Heading(report, "SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH", unreached.Count,
                "the count was NOT written, so it is not in the total, and the row is named");
            Line(report, "  sheet | row | workbook name | Revit name | group | merged | from | why");
            foreach (SpeciesMatch match in unreached)
            {
                Line(report, "  " + Join(
                    match.SheetName,
                    match.Row.ToString(CultureInfo.InvariantCulture),
                    Shown(match.WorkbookName),
                    match.Species.BotanicalName,
                    match.Species.GroupName,
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    match.Why));
            }

            Line(report, string.Empty);

            // Where each one landed, not that it was written nowhere. A row here says the count
            // reaches the sheet's total, and an empty one says it did not.
            Heading(report, "SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT", missed.Count,
                "named with the count, never dropped, and written into an empty row where there was one");
            Line(report, "  Revit name | nearest name in the list | group | merged | from | where it landed | height | diameter | why");
            foreach (SpeciesMatch match in missed)
            {
                Line(report, "  " + Join(
                    match.Species.BotanicalName,
                    // PRINTED AND NEVER MATCHED ON. UNKNOWN against Unknown Tree is a question
                    // about names that a report saying only not held cannot answer, so the run
                    // says what each was nearest to and a person decides whether it is one name
                    // or a family of them.
                    Shown(match.NearestInTheList),
                    Shown(match.Species.GroupName),
                    match.Species.Quantity.ToString(CultureInfo.InvariantCulture),
                    Working(match.Species),
                    match.Placed
                        ? match.SheetName + " " + KpiTemplates.BotanicalColumn
                            + match.Row.ToString(CultureInfo.InvariantCulture) + " and "
                            + KpiTemplates.QuantityColumn + match.Row.ToString(CultureInfo.InvariantCulture)
                        : NowhereAtAll,
                    match.Placed ? Measured(match.Species.Height, match.HeightColumn, match.WhyNoHeightColumn, match.Row) : "-",
                    match.Placed ? Measured(match.Species.Diameter, match.DiameterColumn, match.WhyNoDiameterColumn, match.Row) : "-",
                    match.Why));
            }

            // A workbook one tree short and a workbook eighty five short read the same without
            // this. It counts every species the run merged, this section's and every other, so
            // it is the shortfall of the whole run rather than of the rows above.
            Line(report, "  NOT WRITTEN, THE WHOLE RUN: "
                + Count(matches.Where(one => !one.Placed).Sum(one => one.Species.Quantity), "tree")
                + " of " + matches.Sum(one => one.Species.Quantity).ToString(CultureInfo.InvariantCulture)
                + ", over every species this run merged.");

            if (missed.Any(one => one.Placed))
            {
                Line(report, "  A written row carries the botanical name, the count, and the height and the canopy");
                Line(report, "  diameter the schedule printed beside it, into the columns the sheet's header row names,");
                Line(report, "  and NOTHING ELSE. Family, genus, native and every code column come from no model,");
                Line(report, "  so they stay empty and any KPI that needs one still cannot see this species.");
            }

            Line(report, string.Empty);

            Heading(report, "SPECIES ROWS UNDER NO GROUP", run.Ungrouped.Count,
                "reported and never assumed into a group, because the group decides the sheet");
            foreach (SpeciesRow row in run.Ungrouped)
            {
                Line(report, "  " + Join(row.PlotId, row.BotanicalName,
                    row.Quantity.ToString(CultureInfo.InvariantCulture)));
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// The two tree lists as they were read off the template, so what the tool matched
        /// against can be checked against the file by eye.
        ///
        /// **Three answers to where a list ends were measured on one file**: the map said row
        /// 83, the total said SUM(B4:B92), and the names ran to row 101. The map's answer is
        /// gone, and the other two are printed here side by side so the next disagreement is
        /// read off the report rather than found in a workbook 85 trees short.
        /// </summary>
        private static void TheTreeLists(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "THE WORKBOOK'S OWN TREE LISTS", 2,
                "read off the template when Create was pressed, never off a row range in this tool");

            TheTreeList(report, run.Template == null ? KpiTemplates.ExistingTreesSheet
                : run.Template.ExistingTrees.SheetName, run.ExistingList);
            TheTreeList(report, run.Template == null ? KpiTemplates.ProposedTreesSheet
                : run.Template.ProposedTrees.SheetName, run.ProposedList);

            Line(report, string.Empty);
        }

        public const string ListsNotRead = "not read. The accounting refused before the template was opened.";

        private static void TheTreeList(StringBuilder report, string sheetName, SpeciesList list)
        {
            Line(report, "  " + sheetName);

            if (list == null)
            {
                Line(report, "    " + ListsNotRead);
                return;
            }

            if (!list.WasRead)
            {
                Line(report, "    NOT READ: " + list.Refusal);
                return;
            }

            Line(report, "    botanical names in column " + KpiTemplates.BotanicalColumn + ": "
                + (list.Rows.Count == 0
                    ? "none, the row under the header is empty"
                    : Count(list.Rows.Count, "name") + ", rows " + list.Rows[0].Row + " to "
                        + list.Rows[list.Rows.Count - 1].Row));
            Line(report, "    the quantity total: " + list.TotalInWords
                + (list.TotalFound ? ", reaching rows " + list.TotalFirstRow + " to " + list.TotalLastRow : string.Empty));
            Line(report, "    empty rows the total reaches, for a species the list does not hold: "
                + list.EmptyRows.Count
                + (list.EmptyRows.Count == 0 ? string.Empty : ", rows " + Rows(list.EmptyRows)));
            Line(report, "    height column: " + (list.HeightColumn.Length > 0
                ? list.HeightColumn + ", " + list.HeightColumnChosen
                : "none, " + list.WhyNoHeightColumn));
            Line(report, "    diameter column: " + (list.DiameterColumn.Length > 0
                ? list.DiameterColumn + ", " + list.DiameterColumnChosen
                : "none, " + list.WhyNoDiameterColumn));

            if (list.OutsideTheTotal.Count > 0)
            {
                Line(report, "    NAMES THE TOTAL DOES NOT REACH: " + list.OutsideTheTotal.Count + ", rows "
                    + Rows(list.OutsideTheTotal.Select(one => one.Row).ToList())
                    + ". A count written there would never reach the total, so a species matching");
                Line(report, "    one of these is not written and is named under SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH.");
            }

            if (list.BelowTheList.Count > 0)
            {
                Line(report, "    names below the first empty row of the list, at row " + list.FirstGapRow + ": "
                    + list.BelowTheList.Count + ", rows " + Rows(list.BelowTheList.Select(one => one.Row).ToList())
                    + ". These are not the list and were not matched against.");
            }
        }

        /// <summary>
        /// A run of rows as 93 to 101, or the rows one by one when they do not run.
        /// </summary>
        private static string Rows(IReadOnlyList<int> rows)
        {
            if (rows.Count == 0) return string.Empty;
            if (rows.Count == 1) return rows[0].ToString(CultureInfo.InvariantCulture);

            bool contiguous = true;
            for (int at = 1; at < rows.Count; at++)
            {
                if (rows[at] != rows[at - 1] + 1) contiguous = false;
            }

            return contiguous
                ? rows[0] + " to " + rows[rows.Count - 1]
                : string.Join(", ", rows.Select(one => one.ToString(CultureInfo.InvariantCulture)).ToArray());
        }

        /// <summary>
        /// One kind of schedule on one plot: the name and what was read off it, or none, or
        /// every name found when there was more than one and none was read.
        /// </summary>
        private static string Schedules(string kind, IReadOnlyList<string> names, string read)
        {
            if (names.Count == 0) return kind + " schedule: none filters on this plot, so nothing was read";
            if (names.Count == 1) return kind + " schedule: " + names[0] + ", " + read;

            return kind + " schedules: " + names.Count + " FOUND AND NONE READ, "
                + string.Join(" | ", names.ToArray());
        }

        private static void TheChoices(StringBuilder report, KpiCreateRun run)
        {
            Heading(report, "WHERE EVERY VALUE CAME FROM", run.Readings.Count,
                "a number with no source is a number nobody can check");

            Line(report, "  template: " + (run.Template == null ? "(none)" : run.Template.Name)
                + ", read from " + Shown(run.TemplatePath));
            Line(report, "  templates folder: " + (run.TemplatesListed.IsNothing
                ? "NOT LISTED. Nothing recorded how many times a workbook was opened."
                : run.TemplatesListed.InWords));

            // A workbook the tool withheld is named here with the cell that decided it, so a
            // template wrongly withheld is traced in one line rather than by opening the file.
            foreach (RecognisedWorkbook withheld in run.TemplatesListed.Workbooks.Where(one => one.IsFilled))
            {
                Line(report, "    not offered: " + withheld.FileName + ", " + withheld.DecidedBy.InWords);
            }
            Line(report, "  written to: " + Shown(run.OutputPath));
            Line(report, "  component parameter, chosen by the user: " + Shown(run.ComponentParameter));
            Line(report, "  reference parameter, chosen by the user: " + Shown(run.ReferenceParameter));
            Line(report, "  location: " + Shown(run.Location)
                + ", off Project Information, one read with no plot involved");

            if (run.Component != null && !run.Component.Agrees && run.Component.Distinct.Count > 1)
            {
                Line(report, "  the chosen plots disagreed on the component: "
                    + string.Join(", ", run.Component.Distinct.ToArray()));
            }

            if (run.Reference != null && !run.Reference.Agrees && run.Reference.Distinct.Count > 1)
            {
                Line(report, "  the chosen plots disagreed on the reference: "
                    + string.Join(", ", run.Reference.Distinct.ToArray()));
            }

            Line(report, string.Empty);
            if (run.Reconciliation.AreaWanted)
            {
                TheRegions(report, run);
            }
            else
            {
                Line(report, "  WHICH REGION EACH PLOT'S AREA CAME OFF: " + AreaNotRead);
            }

            Line(report, string.Empty);
            Line(report, "  The shrubs, the lawn and every tree quantity came off a row the schedule");
            Line(report, "  printed. Nothing anywhere is worked out from the elements a schedule lists,");
            Line(report, "  because on this model those elements are RVT Link instances and the plants");
            Line(report, "  live inside them.");

            // The paragraph about the area is about a read that did not happen on a template
            // that takes none, so it is left off rather than printed about nothing.
            if (!run.Reconciliation.AreaWanted) return;

            Line(report, string.Empty);
            Line(report, "  THE AREA IS NOT A SCHEDULE ROW. It is " + KpiNames.InterventionArea
                + " read off the");
            Line(report, "  chosen filled region in the 00 link, raw in square feet, converted here to");
            Line(report, "  square metres. Both are in the row above with the value the model prints");
            Line(report, "  beside them, which is rounded to the metre, so the two can be held against");
            Line(report, "  each other: what was written is that same measurement at full precision and");
            Line(report, "  not a different number.");
        }

        private static void TheRegions(StringBuilder report, KpiCreateRun run)
        {
            Line(report, "  WHICH REGION EACH PLOT'S AREA CAME OFF, AND WHAT IT READ");
            Line(report, "  plot | chosen type | how it was chosen | against the area cell's note | "
                + "raw square feet | written square metres | as the model prints it | offered");
            foreach (PlotReading reading in run.Readings)
            {
                RegionArea chosen = reading.ChosenRegion;

                Line(report, "  " + Join(
                    reading.PlotId,
                    reading.ChosenRegionTypeName.Length == 0 ? "(none)" : reading.ChosenRegionTypeName,
                    reading.ChosenRegionPick.InWords,
                    RegionChoice.AgainstTheNote(reading.ChosenRegionTypeName),
                    chosen == null ? "(none)" : Exactly(chosen.RawSquareFeet),
                    chosen == null ? "(none)" : Exactly(chosen.SquareMetres),
                    chosen == null ? "(none)" : Shown(chosen.Printed),
                    reading.Regions.Count == 0
                        ? "(none)"
                        : string.Join(", ", reading.Regions
                            .Select(one => one.TypeName + " " + Number(one.SquareMetres)).ToArray())));
            }

            // **The 14:29 run settled the note and Bader then made it decide a tie.** The how
            // column is what separates a choice the client's note made from one a person made
            // on the pane, because the two carry different weight and a type name alone says
            // neither.
            Line(report, "  " + RegionChoice.TheNote + " names "
                + RegionChoice.TheNoteNames + ". One region holding an area still decides");
            Line(report, "  by itself whatever it is called. Where MORE THAN ONE holds an area "
                + "and the note's type is");
            Line(report, "  among them, the note decides, which is Bader's decision of "
                + "14 September, and the how column");
            Line(report, "  says so. Where it is not among them the run still asks, and the "
                + "column reads chosen by hand.");
        }

        /// <summary>
        /// A number with nothing taken off it, so a raw square foot reading can be compared
        /// against what the model prints beside it. <see cref="Number"/> rounds for reading and
        /// would hide the difference this row exists to show.
        /// </summary>
        /// <summary>
        /// A duration for reading, to a tenth. Nothing is decided off it and nothing compares
        /// two of these, so a tenth is as fine as a person needs.
        /// </summary>
        private static string Seconds(double value)
        {
            return value.ToString("0.0", CultureInfo.InvariantCulture) + " seconds";
        }

        private static string Exactly(double value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string Working(MergedSpecies species)
        {
            return species.Working;
        }

        /// <summary>
        /// 15 into I84, or why nothing went there, with every value found so the reader can see
        /// what the rows printed.
        /// </summary>
        private static string Measured(MeasureAnswer answer, string column, string whyNoColumn, int row)
        {
            if (string.IsNullOrEmpty(column)) return "not written: " + whyNoColumn;
            if (!answer.Write) return "not written: " + answer.Why;

            return Number(answer.Value) + " into " + column + row.ToString(CultureInfo.InvariantCulture)
                + (answer.Why.Length == 0 ? string.Empty : " (" + answer.Why + ")");
        }

        /// <summary>
        /// Every schedule this run read, as the schedule prints it, last in the file because it
        /// is the longest and named at the top because it is what makes the rest checkable.
        /// Nothing above it was what the tool read, and a plot counted on two rows survived two
        /// rounds because nothing showed the rows.
        /// </summary>
        private static void TheSchedules(StringBuilder report, KpiCreateRun run)
        {
            int howMany = run.Readings.Sum(one => one.PrintedSchedules.Count);
            Heading(report, SchedulesHeading, howMany,
                "one block per schedule, every column, row numbers counting the heading row as 1, so it can be held against the drawing");

            foreach (PlotReading reading in run.Readings)
            {
                foreach (ScannedSchedule schedule in reading.PrintedSchedules)
                {
                    Line(report, string.Empty);
                    Line(report, "  " + reading.PlotId + ", " + schedule.Name);
                    Line(report, "    " + WhatWasRead(reading, schedule));

                    int shown = Math.Min(ShownRows, schedule.Rows.Count);
                    Line(report, "    rows printed " + schedule.Rows.Count + ", shown " + shown
                        + (shown < schedule.Rows.Count ? " of " + schedule.Rows.Count + ", the rest are cut off here" : string.Empty)
                        + (schedule.BodyRowCount != schedule.Rows.Count ? ", the model holds " + schedule.BodyRowCount : string.Empty));

                    foreach (string line in Aligned(schedule.Rows.Take(shown).ToList())) Line(report, "    " + line);
                }
            }

            Line(report, string.Empty);
        }

        /// <summary>
        /// Which rows were read as what, per kind: the species rows and the TOTAL row of a
        /// softscape schedule, and for a shrubs and lawn schedule WHICH SUBTOTAL ROW WAS TAKEN
        /// for each group and why the others were not.
        /// </summary>
        private static string WhatWasRead(PlotReading reading, ScannedSchedule schedule)
        {
            if (schedule.IsSoftscape)
            {
                List<int> rows = reading.Species.Where(one => one.RowNumber > 0).Select(one => one.RowNumber).OrderBy(one => one).ToList();
                List<int> left = reading.LeftOutSpecies.Where(one => one.RowNumber > 0).Select(one => one.RowNumber).OrderBy(one => one).ToList();
                return "group rows: " + (reading.PrintedGroups.Count == 0 ? "none" : Rows(reading.PrintedGroups.Select(one => one.RowNumber).ToList()))
                    + ", read as species rows: " + rows.Count
                    + (rows.Count == 0 ? string.Empty : ", rows " + Rows(rows))
                    + (left.Count == 0 ? string.Empty : ", left out: " + left.Count + ", rows " + Rows(left))
                    + ", passed over as subtotals: " + reading.SoftscapeRowsPassedOver
                    + ", TOTAL row: " + (reading.SoftscapeTotalRead ? "row " + reading.SoftscapeTotalRow : "none found");
            }

            if (reading.Subtotals.Count == 0) return "read as group values: none";

            return "read as group values: " + string.Join("; ", reading.Subtotals.Select(one =>
                one.Phases.Count == 0
                    ? one.Heading + " taken off row " + one.RowNumber + ", the last of its "
                        + Count(one.RowsConsidered.Count, "subtotal row") + " (" + Rows(one.RowsConsidered) + ")"
                        + (one.RowsConsidered.Count > 1 ? ", no phase row, so the rows above it must add to it" : ", nothing above it to check against")
                    : one.Heading + " off " + string.Join(" and ", one.Phases.Select(phase =>
                            phase.Name + " row " + phase.RowNumber + (phase.Counted ? " taken" : " left out")).ToArray())
                        + (one.GroupTotalPrinted ? ", group total row " + one.RowNumber + " checked" : ", no group total row to check against")).ToArray());
        }

        /// <summary>
        /// The rows with every column padded to the widest cell in it, so a column can be read
        /// down. A cell longer than the cap is cut and marked.
        /// </summary>
        private static IEnumerable<string> Aligned(IReadOnlyList<IReadOnlyList<string>> rows)
        {
            const int Cap = 40;
            int columns = rows.Count == 0 ? 0 : rows.Max(row => row.Count);
            var widths = new int[columns];
            for (int column = 0; column < columns; column++)
            {
                widths[column] = rows.Max(row => Cell(row, column, Cap).Length);
            }

            int rowWidth = rows.Count.ToString(CultureInfo.InvariantCulture).Length;
            for (int index = 0; index < rows.Count; index++)
            {
                var cells = new List<string>();
                for (int column = 0; column < columns; column++)
                {
                    cells.Add(Cell(rows[index], column, Cap).PadRight(widths[column]));
                }

                yield return (index + 1).ToString(CultureInfo.InvariantCulture).PadLeft(rowWidth) + " | " + string.Join(" | ", cells.ToArray());
            }
        }

        private static string Cell(IReadOnlyList<string> row, int column, int cap)
        {
            string text = column < row.Count && !string.IsNullOrEmpty(row[column]) ? row[column] : "-";
            return text.Length <= cap ? text : text.Substring(0, cap - 1) + "~";
        }

        private static string Counted(
            string what, int howMany, IReadOnlyList<string> without, IReadOnlyList<string> withMoreThanOne = null)
        {
            string said = what + "   " + howMany;
            if (without.Count > 0) said += ", without: " + string.Join(", ", without.ToArray());
            if (withMoreThanOne != null && withMoreThanOne.Count > 0)
            {
                said += ", with more than one: " + string.Join(", ", withMoreThanOne.ToArray());
            }

            return said;
        }

        private static void Heading(StringBuilder report, string name, int howMany, string said)
        {
            Line(report, "== " + name + " (" + howMany + ") ==");
            Line(report, said);
        }

        private static string Join(params string[] cells)
        {
            return string.Join(" | ", cells);
        }

        private static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Six places for the species record, whose whole job is a difference two places can
        /// swallow. 169.996 against 170 printed through Number reads as 170 against 170,
        /// recorded, a sentence at war with itself.
        /// </summary>
        private static string Fine(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }

        private static string Shown(string value)
        {
            return string.IsNullOrEmpty(value) ? "(empty)" : value;
        }

        private static void Line(StringBuilder report, string text)
        {
            report.Append(text).Append("\r\n");
        }
    }
}
