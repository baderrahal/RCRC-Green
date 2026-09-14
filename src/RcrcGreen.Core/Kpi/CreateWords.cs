using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The open document as Revit last answered for it. Its title, and nothing else.
    ///
    /// **THE MODEL'S FOLDER USED TO BE ON HERE AND IS GONE.** The workbook was written beside
    /// the model, so a model that had never been saved could not be used at all, and a detached
    /// one is exactly what the team works on. The workbook goes to a folder the user browses for
    /// now, remembered the way the template folder is, and Create no longer asks whether the
    /// model has been saved.
    ///
    /// The folder is not kept for anything else. A value on the screen that decides nothing is
    /// how one stale string became a dead end here once already.
    ///
    /// A title is empty only when there is no document.
    /// </summary>
    public sealed class OpenModel
    {
        private OpenModel(string title)
        {
            Title = title ?? string.Empty;
        }

        /// <summary>
        /// No document. What the pane holds before Revit has answered and what Revit answers
        /// when the model is closed while the pane is still on screen.
        /// </summary>
        public static readonly OpenModel Nothing = new OpenModel(string.Empty);

        public static OpenModel Of(string documentTitle)
        {
            return new OpenModel(documentTitle);
        }

        public string Title { get; }

        public bool IsOpen
        {
            get { return Title.Length > 0; }
        }

        public bool Is(OpenModel other)
        {
            return other != null && string.Equals(Title, other.Title, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Every line the plot picker and the Create block show. The pane draws them and formats
    /// none of them, the same rule PanelSteps follows for the Drawing Sheet, because a summary
    /// written next to the control that shows it is two records of one fact.
    /// </summary>
    public static class CreateWords
    {
        public const string Heading = "Plots";

        public const string SelectAll = "Select all";

        public const string Clear = "Clear";

        /// <summary>
        /// The press that reads the plots. **Nothing heavy runs without one.** It is the only
        /// control on this pane that starts a read apart from Create, which reads what it needs
        /// itself, and it exists because the read that used to start on its own held the model
        /// for minutes on every open.
        /// </summary>
        public const string ReadThisModel = "Read this model";

        /// <summary>
        /// The status line the moment that press is made, because on a model this size the read
        /// is long enough that a pane saying nothing reads as a pane that took no notice.
        /// </summary>
        public const string ReadingNow =
            "Reading the plots from this model. On a large model this takes a moment.";

        public const string Create = "Create";

        /// <summary>
        /// The line the moment Create is pressed, before the run has decided anything. It
        /// claims no read, because whether the model is read or the scan and the readings
        /// already held answer this press is the run's to say a moment later, and a press
        /// that reuses both would have been opened with a sentence about reading.
        /// </summary>
        public const string Creating = "Creating.";

        /// <summary>
        /// What a template's row says when every one of its plots wrote. **It names the count and
        /// the root rather than one path**, because a template is many workbooks now.
        /// </summary>
        public static string WorkbooksUnder(KpiTemplate template, int written, string root)
        {
            if (template == null) throw new ArgumentNullException("template");

            return Count(written, "workbook") + " under " + (root ?? string.Empty);
        }

        /// <summary>
        /// What a template's row says when some of its plots wrote nothing. Every plot that did
        /// not is named with its own reason, because a count alone sends somebody to the report
        /// to find out which.
        /// </summary>
        public static string SomePlotsWroteNothing(int written, int ticked, IReadOnlyList<string> why)
        {
            string opening = written + " of " + Count(ticked, "plot") + " wrote a workbook.";
            if (why == null || why.Count == 0) return opening + " " + NoReasonRecorded;

            return opening + " " + string.Join(" ", why.ToArray());
        }

        /// <summary>
        /// Where the workbooks go and how each is named, said once in place of the name box.
        ///
        /// **The box was a lie.** It still read GRP-KPI-Checklist-DD-MOSQUES.xlsx after the round
        /// that made a workbook one plot, and no file is called that any more: every one is named
        /// from its plot's own PRX_Plot_UID2. A box a person can type into, whose text nothing
        /// reads, is worse than no box.
        ///
        /// The example is a real path off the team's own folders, because a shape described in
        /// words and a shape shown are two different amounts of help.
        /// </summary>
        public static string WhereTheWorkbooksGo(string root)
        {
            string where = string.IsNullOrWhiteSpace(root) ? "the output folder" : root.Trim();

            return "One workbook per plot, under " + where
                + ", in a folder named for the plot: the component folder, then the plot's "
                + KpiNames.PlotUid2 + ", then the workbook named after that folder. "
                + where + Separator + "FRIDAY MOSQUE" + Separator + "ANH-008-MO-100006"
                + Separator + "ANH-008-MO-100006" + OutputName.Extension;
        }

        /// <summary>
        /// The separator the example path is written with. Revit runs on Windows, so the example
        /// reads the way a person will see it in Explorer rather than the way the machine this
        /// was built on happens to spell it.
        /// </summary>
        public const string Separator = "\\";

        public const string NoPlots =
            "No plot in this model. Open a model that holds one.";

        public const string NoModel = "No model is open.";

        /// <summary>
        /// The refusal that replaced the never saved one. **Whether the model has been saved is
        /// no longer asked**, because writing beside the model meant a detached model could not
        /// be used at all, and that cost the team most of an afternoon.
        ///
        /// The words are TemplateWords.NoOutputFolder, which the output folder line already
        /// shows, because two sentences for one condition is two records of one fact.
        /// </summary>
        public const string NoOutputFolder = TemplateWords.NoOutputFolder;

        public const string NoTemplate = "No template ticked.";

        public const string NoPlotTicked = "No plot ticked.";

        /// <summary>
        /// **Bader's decision: the date, the prepared by and the position are typed once and
        /// the same three values go into every workbook.** Said on the pane when more than one
        /// template is ticked, because three boxes above six named rows read as three boxes for
        /// whichever row is nearest them.
        /// </summary>
        public const string OneSetOfFields =
            "The date, the prepared by and the position are typed once and go into every workbook "
            + "this press writes.";

        /// <summary>
        /// One ticked template's row before the press: which plots it will get, or why it will
        /// write nothing. **A ticked template no ticked plot belongs to stays ticked and stays
        /// listed**, saying this rather than being hidden or unticked for the user.
        /// </summary>
        public static string TemplateRow(TemplateShare share)
        {
            if (share == null) throw new ArgumentNullException("share");

            if (!share.WillWrite) return share.Template.Name + ": " + share.WhyNothing;

            // **NEVER EVERY PLOT.** This listed all 78 street plots by name and the block ran
            // off the screen. The count and the range say the same thing in one line, and the
            // report holds the list.
            return share.Template.Name + ": " + Count(share.Plots.Count, "plot")
                + Range(share.Plots) + ". The report names them.";
        }

        /// <summary>
        /// The span a list of plots covers, first to last in the order they came, or nothing at
        /// all for a list short enough to read. Three plots named is shorter than three plots,
        /// DM-12 to FM-05.
        /// </summary>
        public static string Range(IReadOnlyList<string> plots)
        {
            if (plots == null || plots.Count == 0) return string.Empty;
            if (plots.Count <= ShownByName) return ", " + string.Join(", ", plots.ToArray());

            return ", " + plots[0] + " to " + plots[plots.Count - 1];
        }

        /// <summary>
        /// How many plots are named outright before a row falls back to the range. Four fits a
        /// line and twenty does not, and 78 is what ran off the screen.
        /// </summary>
        public const int ShownByName = 4;

        /// <summary>
        /// **The 09:18 run read six plots and dropped them at the last step**, after the read,
        /// with no word of it before the press. EP-05, EP-11, EP-12, EP-13, EP-15 and FM-08 have
        /// no sheet, so no component, so the plot prefix placed them into a template exactly as
        /// its own rule says, and then the folder table had nothing for them.
        ///
        /// Five of the six file under their template's own folder now. The one that cannot is
        /// named HERE, before the press, rather than after its read: **reading a plot and
        /// throwing it away is work nobody asked for.**
        /// </summary>
        public static IReadOnlyList<string> PlotsWithNoComponent(TemplateSplit split)
        {
            var lines = new List<string>();
            if (split == null) return lines;

            List<PlotTemplate> byPrefix = split.Answers
                .Where(one => one.Route == TemplateRoute.Prefix)
                .ToList();
            if (byPrefix.Count == 0) return lines;

            List<string> filed = byPrefix
                .Where(one => ComponentFolders.OnlyFolderFor(one.Template).Length > 0)
                .Select(one => one.PlotId)
                .ToList();

            List<PlotTemplate> nowhere = byPrefix
                .Where(one => ComponentFolders.OnlyFolderFor(one.Template).Length == 0)
                .ToList();

            lines.Add(byPrefix.Count.ToString(CultureInfo.InvariantCulture)
                + (byPrefix.Count == 1 ? " ticked plot carries" : " ticked plots carry")
                + " no component, because it is on no sheet, and the plot prefix placed "
                + (byPrefix.Count == 1 ? "it" : "each one") + Range(byPrefix.Select(one => one.PlotId).ToList()) + ".");

            if (filed.Count > 0)
            {
                lines.Add(filed.Count.ToString(CultureInfo.InvariantCulture) + " of those file under "
                    + (filed.Count == 1 ? "its" : "their") + " template's own folder"
                    + Range(filed) + ".");
            }

            foreach (PlotTemplate one in nowhere)
            {
                lines.Add(one.PlotId + " WILL BE READ AND WRITTEN NOWHERE: "
                    + ComponentFolders.NoFolderForTemplate(one.Template)
                    + ". Untick it, or give it a sheet carrying a component.");
            }

            return lines;
        }

        /// <summary>
        /// The same row after the press: written with its path, or not written with its reason.
        /// Never one line for the run that hides which of six failed.
        /// </summary>
        public static string TemplateOutcomeRow(TemplateOutcome outcome)
        {
            if (outcome == null) throw new ArgumentNullException("outcome");

            // **What happened, never what was planned.** Some wrote and some did not is its own
            // answer: the row used to say Nothing was written beside twenty workbooks on disk.
            if (outcome.WroteSomeOfThem)
            {
                return outcome.Template.Name + ": " + outcome.OutputPath + ". "
                    + (outcome.Why.Length == 0 ? NoReasonRecorded : outcome.Why);
            }

            if (outcome.Written) return outcome.Template.Name + ": " + outcome.OutputPath;

            return outcome.Template.Name + ": " + NothingWritten + " "
                + (outcome.Why.Length == 0 ? NoReasonRecorded : outcome.Why);
        }

        /// <summary>
        /// Why one template of several wrote nothing, for its own row. The same answer the
        /// status line gives for a single template run, without the report line, because the
        /// report is named once for the whole press rather than once per row.
        /// </summary>
        public static string WhyThisOneWroteNothing(KpiCreateRun run)
        {
            if (run == null) throw new ArgumentNullException("run");

            return WhyNothingWasWritten(run, string.Empty);
        }

        /// <summary>
        /// The status line after a press that covered several templates. It counts the four
        /// the run's accounting counts rather than adding the cells up, because which of six
        /// wrote is the thing a person needs off one line, and the rows under it carry each
        /// one's own answer.
        /// </summary>
        public static string WroteAcross(KpiCreateRunSet set, string reportWhere)
        {
            if (set == null) throw new ArgumentNullException("set");

            // **COUNT WORKBOOKS WHERE THE UNIT IS A WORKBOOK.** This read 1 workbook written of
            // 2 templates ticked on a press that wrote 98, because it counted templates and
            // called them workbooks.
            var said = new List<string>
            {
                Count(set.WorkbooksWritten, "workbook") + " written of "
                    + Count(set.PlotsTicked, "plot") + " ticked"
            };

            if (set.PlotsThatWroteNothing > 0)
            {
                said.Add(Count(set.PlotsThatWroteNothing, "plot") + " wrote nothing");
            }

            if (set.TemplatesWithNothingToWrite > 0)
            {
                said.Add(Count(set.TemplatesWithNothingToWrite, "template") + " with no plot of its own");
            }

            string line = string.Join(", ", said.ToArray()) + ", over "
                + Count(set.TemplatesTicked, "template") + ".";

            if (!set.Split.AddsUp)
            {
                line = NothingWritten + " " + string.Join(" ", set.Refusals.ToArray());
            }

            return string.IsNullOrWhiteSpace(reportWhere) ? line : line + " Report: " + reportWhere;
        }

        /// <summary>
        /// **It said typed by hand and they are not.** The road width and the total length come
        /// off the team's scope validation file, matched on the plot's own PRX_Plot_UID2, since
        /// the round that added it. A line about what the tool does is checked against what it
        /// does.
        /// </summary>
        /// <summary>
        /// Said of a template whose map names no area cell, which is none of the seven today.
        ///
        /// **It read that STREETS works its area out from the road width and the total length,
        /// and that stopped being true when the client emptied H8.** The words say what the tool
        /// does rather than what a sheet once did, so the next template that names no area cell
        /// gets a line that is true of it.
        /// </summary>
        public const string TakesNoArea =
            "This template names no area cell, so no filled region is read for it and nothing "
            + "about the area is refused on.";

        /// <summary>
        /// Under the Reference picker with nothing ticked. The values shown there belong to a
        /// plot, so with no plot chosen there is nothing to show and the block says so rather
        /// than showing some other plot's.
        /// </summary>
        public const string NoPlotForTheReferenceValues =
            "No plot is ticked, so there is no value to show here. Tick a plot to see what each "
            + "of the four holds on it.";

        /// <summary>
        /// The heading over the four values, naming the plot they belong to.
        ///
        /// **The plot is named because the block was showing DM-11's values with DM-12 ticked.**
        /// It read the first plot in the model's list rather than the first ticked one, and the
        /// whole point of the block is that a person picks the reference by looking at its
        /// value. A value belonging to a plot they did not choose is worse than no value.
        /// </summary>
        public static string ReferenceValuesOn(string plotId)
        {
            return "What each holds on " + (string.IsNullOrWhiteSpace(plotId) ? "(no plot)" : plotId.Trim())
                + ", the first ticked plot:";
        }

        /// <summary>
        /// One refusal listing everything that is missing, rather than one per thing. Pressing
        /// Create three times to be told three separate halves of the same answer is worse than
        /// being told all of it once.
        ///
        /// **It no longer asks whether the model has been saved.** It asks whether there is
        /// somewhere to write, which is the browsed output folder. The two used to be one
        /// question because the workbook went beside the model, and a detached model was refused
        /// with No model is open next to a header counting its 96,959 elements.
        ///
        /// The folder is passed in rather than held, and the Revit side reads it off disk at the
        /// moment Create is pressed, so nothing here can be deciding on a copy taken earlier.
        /// </summary>
        public static string CannotCreate(
            OpenModel model, string outputFolder, bool hasTemplate, bool hasAPlot)
        {
            OpenModel open = model ?? OpenModel.Nothing;

            var missing = new List<string>();
            if (!open.IsOpen) missing.Add(NoModel);
            if (string.IsNullOrWhiteSpace(outputFolder)) missing.Add(NoOutputFolder);
            if (!hasTemplate) missing.Add(NoTemplate);
            if (!hasAPlot) missing.Add(NoPlotTicked);

            if (missing.Count == 0) return string.Empty;

            return "Cannot create. " + string.Join(" ", missing.ToArray());
        }

        /// <summary>
        /// A model is open and nothing has read it. NOT open a model, because a model is open
        /// and the header names it, and NOT reading them, because nothing is reading: **the
        /// read waits for a press.** A docked pane is restored visible at Revit startup, so a
        /// read that started itself here fired on every model anybody opened and held NG05 for
        /// minutes with nobody having asked for anything.
        /// </summary>
        public const string NothingHasReadThisModel =
            "This model has not been read yet. Reading it walks every sheet and schedule, so it "
            + "waits for a press.";

        /// <summary>
        /// No document at all, the one place open a model is the right thing to say. The pane
        /// reads a model's plots as soon as one is open, so it says so.
        /// </summary>
        public const string NoModelToReadPlotsFrom =
            "No model open. Open one and this pane reads its name, which costs nothing.";

        /// <summary>
        /// The lead lines of the plots block, one line per state, because open a model on a
        /// model that is open sent the team to Revit for an hour. FOUR states, each its own
        /// line: no document says open one, a document whose plots have not come back yet says
        /// it is reading them, a document answered with no plots says the model holds none, and
        /// a document answered with plots hands off to <see cref="PlotSources"/> for the count.
        /// The not answered state is told apart from the no document one by whether a document
        /// is open, which the pane knows off the live document it reads every draw, never a
        /// held copy. A null plots means the answer has not come back, which is not the same as
        /// an answer of no plots.
        /// </summary>
        public static IReadOnlyList<string> PlotsBlock(bool documentOpen, PlotsInTheModel plots)
        {
            if (plots != null) return PlotSources(plots);

            return new List<string> { documentOpen ? NothingHasReadThisModel : NoModelToReadPlotsFrom };
        }

        /// <summary>
        /// What the two lists of plots disagree about, said on screen rather than resolved.
        /// PRX_Plot_ID on the sheets and the PRX_Ref Plot ID filter on the schedules are two
        /// records of one fact and this is where they are held apart.
        /// </summary>
        public static IReadOnlyList<string> PlotSources(PlotsInTheModel plots)
        {
            if (plots == null) throw new ArgumentNullException("plots");

            var said = new List<string>();
            if (plots.All.Count == 0)
            {
                said.Add(NoPlots);
                return said;
            }

            if (plots.Agree)
            {
                said.Add("The sheets and the schedules name the same "
                    + Count(plots.All.Count, "plot") + ".");
                return said;
            }

            said.Add("The sheets and the schedules do not name the same plots. Both lists are offered.");

            if (plots.OnSheetsOnly.Count > 0)
            {
                said.Add("  On a sheet and on no schedule, " + plots.OnSheetsOnly.Count + ": "
                    + string.Join(", ", plots.OnSheetsOnly.ToArray()));
            }

            if (plots.OnSchedulesOnly.Count > 0)
            {
                said.Add("  On a schedule and on no sheet, " + plots.OnSchedulesOnly.Count + ": "
                    + string.Join(", ", plots.OnSchedulesOnly.ToArray()));
            }

            return said;
        }

        /// <summary>
        /// The line that asks a person to confirm before two identical areas are added.
        /// </summary>
        public static IReadOnlyList<string> ConfirmIdentical(IReadOnlyList<IdenticalArea> identical)
        {
            var said = new List<string>();
            if (identical == null || identical.Count == 0) return said;

            said.Add("CONFIRM BEFORE WRITING. " + Count(identical.Count, "set")
                + " of chosen plots report the same area.");

            foreach (IdenticalArea shared in identical)
            {
                said.Add("  " + string.Join(", ", shared.Plots.ToArray()) + " all read " + shared.Printed
                    + ". Either they are the same size or one region is counted twice.");
            }

            return said;
        }

        public const string NothingWritten = "Nothing was written.";

        /// <summary>
        /// The last resort, said when a run ended with no file and nothing anywhere recorded a
        /// reason. It should never print, and it says so, because a status line that goes blank
        /// after a press reads as success and is the worst thing it can do.
        /// </summary>
        public const string NoReasonRecorded =
            "No reason was recorded for it, which is a bug in this tool. The report has what "
            + "the run knew.";

        /// <summary>
        /// **THE FILE IS USUALLY OPEN IN EXCEL.** That is what the delete before the copy meets,
        /// and the message Windows gives for it names a process rather than a thing to do. The
        /// thing to do is said first and the system's own words are kept after it.
        /// </summary>
        public static string CouldNotBeWritten(string detail)
        {
            return "The workbook could not be written. If it is open in Excel, close it and "
                + "press Create again."
                + (string.IsNullOrWhiteSpace(detail) ? string.Empty : " " + detail.Trim());
        }

        /// <summary>
        /// The refusal both template guards give, so the two say one sentence rather than two.
        ///
        /// It names the file it would have overwritten and says what that file is, because
        /// "the workbook could not be written" was all the user got when the delete had already
        /// taken the client's template.
        /// </summary>
        public static string WouldOverwriteTheTemplate(string outputPath, SamePath answer)
        {
            string named = string.IsNullOrWhiteSpace(outputPath) ? "The output file" : outputPath.Trim();

            if (answer == SamePath.Unreadable)
            {
                return NothingWritten + " " + named + " could not be checked against the "
                    + "template it would be copied from, so the run was refused rather than "
                    + "risk writing over it.";
            }

            return NothingWritten + " " + named + " IS the template this run was about to read. "
                + "The tool never writes to a template. Change the output folder or the name in "
                + "the box and press Create again.";
        }

        /// <summary>
        /// The note above the Create button naming every plot and schedule where a group no
        /// sheet takes was found, Street Design on a mosque plot. It is a note and not a
        /// refusal: the workbook is written with those rows left out, and this says where the
        /// model needs correcting, where the user is looking when they press. Plots and
        /// schedules, never species, because on a run of 78 plots a long list is not read.
        /// Empty when nothing was left out, and empty on STREETS for Street Design, which
        /// counts there.
        /// </summary>
        public static string GroupsLeftOut(IEnumerable<PlotReading> readings, KpiTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");

            var names = new List<string>();
            var plots = new List<string>();
            bool spelled = false;

            foreach (PlotReading reading in (readings ?? Enumerable.Empty<PlotReading>())
                .Where(one => one != null)
                .OrderBy(one => one.PlotId, NaturalOrder.Comparer))
            {
                List<string> inSoftscape = reading.PrintedGroups.Where(one => !one.Counted).Select(one => one.Name).ToList();
                List<string> inShrubs = reading.Subtotals
                    .SelectMany(one => one.Phases).Where(one => !one.Counted).Select(one => one.Name).ToList();
                if (inSoftscape.Count == 0 && inShrubs.Count == 0) continue;

                foreach (string name in inSoftscape.Concat(inShrubs))
                {
                    if (!names.Any(one => string.Equals(one, name, StringComparison.OrdinalIgnoreCase))) names.Add(name);
                }

                string where;
                if (inSoftscape.Count > 0 && inShrubs.Count > 0)
                {
                    where = spelled ? "in both" : "in its softscape and its shrubs and lawn schedules";
                    spelled = true;
                }
                else
                {
                    where = inSoftscape.Count > 0 ? "in its softscape schedule" : "in its shrubs and lawn schedule";
                }

                plots.Add(reading.PlotId + " " + where);
            }

            if (plots.Count == 0) return string.Empty;

            return string.Join(" and ", names.ToArray()) + " found on " + Count(plots.Count, "plot") + " on "
                + template.Name + ", which has no sheet for " + (names.Count == 1 ? "it" : "them") + ": "
                + string.Join(", ", plots.ToArray()) + ". Those rows were left out. Fix them in the model.";
        }

        public static string Refused(Reconciliation reconciliation)
        {
            if (reconciliation == null) throw new ArgumentNullException("reconciliation");
            if (reconciliation.AddsUp) return string.Empty;

            return NothingWritten + " " + string.Join(" ", reconciliation.Refusals.ToArray());
        }

        /// <summary>
        /// The status line after one press of Create, counting what landed rather than what was
        /// planned, and saying why when nothing landed.
        ///
        /// **It used to go blank.** A run whose accounting passed and whose patch was refused
        /// fell to <see cref="Refused"/>, which answers the empty string when the accounting
        /// added up, so the pane went from Creating to nothing at all. The commonest cause is
        /// the output workbook still open in Excel from the run before. Silence after a press
        /// reads as success, so every path that ends with no file now says why.
        /// </summary>
        public static string Wrote(KpiCreateRun run, string reportWhere)
        {
            if (run == null) throw new ArgumentNullException("run");

            if (!run.Wrote) return WhyNothingWasWritten(run, reportWhere);

            return Count(run.Outcome.Landed.Count, "cell") + " written from "
                + Count(run.Readings.Count, "plot") + ", "
                + Count(run.Plan.Skipped.Count, "cell") + " not written. "
                + RoundingNotes(run)
                + "Workbook: " + run.OutputPath + ". Report: " + reportWhere;
        }

        /// <summary>
        /// How many group totals went through on the rounding room, beside the written count,
        /// because a note only the report file holds is a note nobody reads. The detail stays
        /// in the report, where each note sits beside its group total row.
        /// </summary>
        private static string RoundingNotes(KpiCreateRun run)
        {
            int notes = run.Readings
                .SelectMany(one => one.Subtotals)
                .Count(one => one.RoundingNote.Length > 0);
            if (notes == 0) return string.Empty;

            return Count(notes, "rounding note") + (notes == 1 ? " is" : " are") + " in the report. ";
        }

        /// <summary>
        /// Where the accounting's own refusals are already on the screen. The pane prints them
        /// in red above the Create button, which is where the user is looking when they press,
        /// so the status line counts them and points there rather than repeating all four word
        /// for word. **The 0928 run printed the same four lines twice on one screen.**
        /// </summary>
        public static string ReasonsAreAbove(int howMany)
        {
            return NothingWritten + " " + Count(howMany, "reason")
                + ", shown in full above the Create button.";
        }

        /// <summary>
        /// Never empty. The accounting speaks first because it refuses before anything is
        /// copied, then the patch, and then the line that says nobody recorded a reason.
        ///
        /// The accounting's reasons are counted rather than repeated, because the pane has them
        /// in red directly above the button. The patch's refusal is said in full, because
        /// nothing else on the pane carries it.
        /// </summary>
        private static string WhyNothingWasWritten(KpiCreateRun run, string reportWhere)
        {
            string why;

            if (!run.Reconciliation.AddsUp)
            {
                why = ReasonsAreAbove(run.Reconciliation.Refusals.Count);
            }
            else
            {
                string refusal = run.Outcome == null ? string.Empty : run.Outcome.Refusal;
                why = NothingWritten + " "
                    + (string.IsNullOrWhiteSpace(refusal) ? NoReasonRecorded : refusal.Trim());
            }

            return string.IsNullOrWhiteSpace(reportWhere) ? why : why + " Report: " + reportWhere;
        }

        /// <summary>
        /// The name offered in the box: the template name and the component, falling back to
        /// the plot when there is one and to the ticked count when there are several.
        /// </summary>
        public static string SuggestedName(KpiTemplate template, string component, IReadOnlyList<string> ticked)
        {
            string front = template == null ? "KPI" : template.Name;
            IReadOnlyList<string> plots = ticked ?? new List<string>();

            if (!string.IsNullOrWhiteSpace(component)) return front + " " + component.Trim();
            if (plots.Count == 1) return front + " " + plots[0];
            if (plots.Count > 1) return front + " " + plots.Count + " plots";

            return front;
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }
    }
}
