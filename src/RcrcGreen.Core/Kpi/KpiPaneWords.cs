using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every line the KPI pane shows, so the pane formats nothing of its own. The same rule
    /// as PanelSteps for the Drawing Sheet: a count written next to the control that shows it
    /// is a second record of one fact.
    /// </summary>
    public static class KpiPaneWords
    {
        public const string NoModelName = "No model open";

        public const string Waiting = "Open a model. This pane reads its name as soon as it is shown.";

        public const string NoModel = "No open document. Open a model.";

        /// <summary>
        /// Before any read has answered for this model. It names no button, because there
        /// is none: Create reads the model itself when it needs to.
        /// </summary>
        public const string NotScanned = "Not read yet.";

        public const string Scanning =
            "Reading the whole model. It reads every sheet, link and schedule, so it takes "
            + "longer than the Drawing Sheet read.";

        /// <summary>
        /// What one press writes, counted off the handler: per plot, its two folders where they
        /// are missing, the workbook and, where a forms folder is set, the PDF beside it, then
        /// the checklist report, and the scan report too when the press reads the model rather
        /// than reusing the scan it holds.
        /// </summary>
        public const string ReadOnly =
            "Create writes one workbook per plot, with a PDF beside it when a forms folder "
            + "is set, in a component folder and a UID2 folder it makes where they are missing "
            + "and never deletes. It writes the checklist report, and the scan report when it "
            + "reads the model. It creates nothing in the model and never writes to a template.";

        /// <summary>
        /// What the street reference file is, in full: the not set sentence, or what the file
        /// held. Step 1's tooltip and BEFORE THE PRESS both read this.
        /// </summary>
        public static string StreetReferenceSays(string path, StreetReferenceFile file)
        {
            if (string.IsNullOrWhiteSpace(path)) return KpiSteps.StreetReferenceNotSet + ".";
            return (file ?? StreetReferenceFile.NotSet).InWords;
        }

        /// <summary>
        /// What the team's plot list file is, in full: the not set sentence, its count, or why
        /// it could not be read. Step 1, the Plot list button and BEFORE THE PRESS read this.
        /// </summary>
        public static string PlotListSays(string path, PlotListRead list)
        {
            if (string.IsNullOrWhiteSpace(path)) return KpiSteps.PlotListNotSet + ".";

            PlotListRead held = list ?? PlotListRead.NotSet;
            return TickingTheList.Heading + " " + (held.Read ? held.InWords : held.Why);
        }

        public static string ModelNamed(string documentTitle)
        {
            return string.IsNullOrEmpty(documentTitle) ? NoModelName : documentTitle;
        }

        /// <summary>
        /// The line under the model name once a scan has run. The count and the time go with
        /// the name because they say which model and which state of it the file describes.
        /// </summary>
        public static string ReadAt(DateTime readAt, int elements, double seconds)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "Read at {0}, {1} elements in {2} seconds.",
                readAt.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                elements,
                seconds.ToString("0.0", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The status line after a scan. Counts of what was read, how many of the nine
        /// questions the file has something for, and where the file is.
        /// </summary>
        public static string Headline(KpiScan scan, string where)
        {
            if (scan == null) throw new ArgumentNullException("scan");

            int answered = KpiQuestions.Answers(scan).Count(answer => answer.Answered);
            int loaded = scan.Links.Instances.Count(link => link.IsLoaded);
            int regions = scan.Links.Contents.Sum(link => link.FilledRegionCount);

            string said = string.Format(
                CultureInfo.InvariantCulture,
                "{0} sheets, {1} title blocks, {2} links with {3} loaded, {4} schedules, {5} filled "
                + "regions read. {6} of 9 questions have something in the file",
                scan.TitleBlocks.SheetCount,
                scan.TitleBlocks.TitleBlockInstances,
                scan.Links.Instances.Count,
                loaded,
                scan.Schedules.Schedules.Count,
                regions,
                answered);

            if (scan.Skipped.Count > 0)
            {
                said += ", and " + scan.Skipped.Count + (scan.Skipped.Count == 1 ? " read" : " reads")
                    + " did not happen, named at the top of it";
            }

            return said + ". " + (where ?? string.Empty);
        }

        // **THE PANE'S OWN WORDS FROM THE REDESIGN OF OCTOBER ON.** The pane went to the whole
        // production team and read like a log, so every line it shows is at most ten words a
        // sentence and every explanation it used to carry is a tooltip or a report section. A
        // test walks every one of these with sample values, large counts included.

        public const string Title = "KPI Checklist";

        /// <summary>The header's third line, off the build stamp, <see cref="BuildStamp"/>.</summary>
        public static string Build(string stamp)
        {
            return "Build " + BuildStamp.Shown(stamp);
        }

        public const string Folders = "Folders";
        public const string Files = "Files";
        public const string WorkbookDetails = "Workbook details";
        public const string FolderLayoutTitle = "Folder layout";
        public const string Flat = "Flat";
        public const string Grouped = "Grouped";

        public const string FolderLayoutTip =
            "How the plot folders nest under the output folder. Nothing is picked until you pick "
            + "one, and Create stays off until then. It is remembered beside the installed add-in.";

        public const string FlatTip =
            @"Output\<value folder>\<UID2>, for example Output\FRIDAY MOSQUE MORE THAN 60%\<UID2>. "
            + @"Parks, healthcare and parking: Output\HEALTHCARE\<UID2>.";

        public const string GroupedTip =
            @"Output\<main>\<value folder>\<UID2>, for example "
            + @"Output\FRIDAY MOSQUE\FRIDAY MOSQUE MORE THAN 60%\<UID2> and "
            + @"Output\STREETS\NH STREET 20m ROW\<UID2>. Parks, healthcare and parking stay one level.";
        public const string Templates = "Templates";
        public const string Output = "Output";
        public const string PdfForms = "PDF forms";
        public const string StreetReference = "Street reference";
        public const string PlotList = "Plot list";
        public const string Optional = "Optional";
        public const string Browse = "Browse";
        public const string Date = "Date";
        public const string PreparedBy = "Prepared by";
        public const string Position = "Position";

        /// <summary>The hint under a path box nobody has pointed anywhere.</summary>
        public const string NotSet = "Not set";

        /// <summary>The hint under a path box whose remembered path has gone.</summary>
        public const string NotFound = "Not found";

        /// <summary>The hint under a file that is there and could not be read.</summary>
        public const string Unreadable = "Unreadable";

        public static string TemplatesFound(int howMany)
        {
            return Count(howMany, "template", "templates") + " found";
        }

        // A template the tool did not know by name, and a component value no file places. Each
        // box line stays under ten words, the rest is in its tooltip.

        public const string UnknownTemplateLayout = "Unknown template layout";

        public const string Use = "Use";

        public const string Skip = "Skip";

        public const string UseSuggestion = "Use suggestion";

        public const string Choose = "Choose";

        public const string Add = "Add";

        public static string NewTemplateSameAs(KpiTemplate sameAs)
        {
            return "New template, same layout as " + (sameAs == null ? string.Empty : sameAs.Name);
        }

        public static string PlacedByRule(int howMany)
        {
            return Count(howMany, "value placed by rule", "values placed by rule");
        }

        /// <summary>The line's tooltip, one value a line with the rule that placed it.</summary>
        public static string PlacedByRuleSays(IEnumerable<KeyValuePair<string, ComponentPlacement>> placed)
        {
            return string.Join("\n", (placed ?? Enumerable.Empty<KeyValuePair<string, ComponentPlacement>>())
                .Select(one => one.Key + ": rule " + one.Value.Rule.Words + ", "
                    + one.Value.TemplateShown(string.Empty) + ", folder " + one.Value.Folder)
                .ToArray())
                + "\n\nAn exact row in " + ComponentTable.FileName + " beats every rule.";
        }

        public static string UnknownValue(string value)
        {
            return "Unknown value " + value;
        }

        public static string PlotsCarryIt(int howMany)
        {
            return Count(howMany, "plot carries it", "plots carry it");
        }

        public static string Suggested(KpiTemplate template)
        {
            return template == null ? "No suggestion. Choose a template." : "Suggested: " + AssetTypeNames.Display(template);
        }

        public static string SuggestedFolder(string folder)
        {
            return "Folder: " + folder;
        }

        public static string AddsTo(bool shared)
        {
            return shared ? "Adds a line to the shared components.csv" : "Adds a line to your own components.csv";
        }

        /// <summary>The tooltip on an unknown value's box, every sentence the box leaves out.</summary>
        public static string UnknownValueSays(UnknownComponent unknown, string sharedPath, string personalPath)
        {
            return unknown.Value + " is in no components.csv and no rule places it, so its plots go into no workbook. "
                + "Nothing places a value by a guess.\n\n"
                + (unknown.Suggested == null
                    ? "No known template shares a word with it, so choose one."
                    : "Suggested template " + unknown.Suggested.Name + ", the known one sharing the most words with it.")
                + " Suggested folder " + unknown.Folder + ", the value itself.\n\n"
                + "Use suggestion or Choose writes one line to "
                + (sharedPath.Length > 0 ? sharedPath + ", or to " + personalPath + " if that cannot be written." : personalPath + ".")
                + " Skip leaves its plots refused until Revit closes.";
        }

        public static string NewTemplateSays(RecognisedWorkbook workbook)
        {
            return workbook.FileName + "\n\n" + workbook.Reason
                + "\n\nUse holds until Revit closes. A components.csv row naming " + (workbook.Proposed == null ? "it" : workbook.Proposed.Name)
                + " uses it on every start. Skip leaves the file unused.";
        }

        /// <summary>
        /// A components.csv that was there and was not read whole. Not read lets the files below
        /// it answer for every value, so it is said on the pane and not only in the report.
        /// </summary>
        public static string ComponentFileSays(ComponentFile file, bool namesAnUnknownTemplate = false)
        {
            string which = file == null ? "A" : char.ToUpperInvariant(ComponentTable.SourceWord(file.Source)[0]) + ComponentTable.SourceWord(file.Source).Substring(1);
            if (file != null && !file.Read) return which + " components.csv was not read";

            // A row naming a template this folder does not hold places nothing, and its value
            // gets no box of its own, so the file is named here.
            return which + " components.csv " + (file != null && file.Problems.Count == 0 && namesAnUnknownTemplate
                ? "names a template not found here"
                : "has lines not taken");
        }

        /// <summary>
        /// A component-rules.csv that was there and was not read, or names a template not found
        /// here. A shared file not read lets the shipped rules answer, so it is said on the pane.
        /// </summary>
        public static string RulesFileSays(ComponentRuleFile file)
        {
            string which = file != null && file.Source == ComponentSource.Shared ? "Shared" : "Shipped";
            return file != null && !file.Read
                ? which + " component-rules.csv was not read"
                : which + " component-rules.csv names a template not found here";
        }

        public const string AlreadyInAFile ="Not added. A components.csv already names it.";

        public static string LineWritten(string path)
        {
            return "Added to " + path;
        }

        // Why it was not added is the status line's tooltip, because it is an exception's text.
        public const string LineNotWritten = "Not added. The tooltip says why.";

        public const string PickATemplateAndAFolder = "Pick a template and a folder first.";

        public static string RowsHeld(int howMany)
        {
            return Count(howMany, "row", "rows");
        }

        public static string PlotsListed(int howMany)
        {
            return Count(howMany, "plot", "plots");
        }

        public const string NextRead = "Next: Read";

        public const string ModelRead = "Model read";
        public const string ReadModel = "Read model";
        public const string ReadAgain = "Read again";
        public const string NotReadYet = "Not read yet";

        // Bader's answer to question 4: step 2 before a read was a footer under an empty body.
        public const string ReadsSheetsAndSchedules = "Reads sheets and schedules. Nothing changes.";

        public const string NoModelOpen = "No model open";
        public const string ReadingTheModel = "Reading the model";
        public const string PlotsBox = "Plots";
        public const string ElementsBox = "Elements";
        public const string TimeBox = "Time";
        public const string InPlotList = "In plot list";
        public const string TemplatesMatched = "Templates matched";
        public const string ValuesFrom = "Values from";
        public const string Component = "Component";
        public const string Reference = "Reference";
        public const string Location = "Location";
        public const string NothingHoldsThatWord = "Nothing in this model holds that word.";
        public const string NextPlots = "Next: Plots";

        /// <summary>The Time box, 0.8 s. One place, so the box and the report agree.</summary>
        public static string Seconds(double seconds)
        {
            return seconds.ToString("0.0", CultureInfo.InvariantCulture) + " s";
        }

        /// <summary>A count in a box, with thousands grouped, 96,959.</summary>
        public static string Number(int howMany)
        {
            return howMany.ToString("#,0", CultureInfo.InvariantCulture);
        }

        public static string OfCount(int some, int all)
        {
            return some.ToString(CultureInfo.InvariantCulture) + " of " + all.ToString(CultureInfo.InvariantCulture);
        }

        public static string ReadAtClock(DateTime when)
        {
            return "Read at " + when.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public const string ShowInReport = "Show in report";
        public const string InTheReportAfterCreate = "In the report after Create";

        public const string AssetTypes = "Asset types";
        public const string PlotsSection = "Plots";
        public const string All = "All";
        public const string None = "None";
        public const string PlotListButton = "Plot list";
        public const string SearchTooltip = "Type to filter the list by plot id.";
        public const string NoType = "No type";
        public const string SharedId = "Shared id";
        public const string TickListAgain = "Tick list again";
        public const string NoTemplatesInTheFolder = "No templates in this folder.";
        public const string NextCreate = "Next: Create";

        public static string PlotsSelected(int howMany)
        {
            return Count(howMany, "plot", "plots") + " selected";
        }

        public const string ReadyToCreate = "Ready to create";
        public const string AssetTypeColumn = "Asset type";
        public const string PlotsColumn = "Plots";
        public const string RangeColumn = "Range";
        public const string Replaced = "Files with the same name are replaced.";
        public const string NothingInTheModelChanges = "Nothing in the model changes";

        /// <summary>
        /// The words beside the big number in step 4. **A forms folder set is what says a PDF
        /// goes beside each workbook**, and none set is the ordinary case of no PDF at all.
        /// </summary>
        public static string WorkbooksBeside(int workbooks, bool withPdf)
        {
            string unit = workbooks == 1 ? "workbook" : "workbooks";
            return withPdf ? unit + ", each with its PDF" : unit + ", no PDF";
        }

        public static string CreateWorkbooks(int workbooks)
        {
            return "Create " + Count(workbooks, "workbook", "workbooks");
        }

        public const string Failed = "Failed";
        public const string Created = "Created";
        public const string AllCreated = "All workbooks created";
        public const string ReadyToSend = "Ready to send";
        public const string OpenOutputFolder = "Open output folder";
        public const string OpenReport = "Open report";
        public const string RetryFailed = "Retry failed";
        public const string NewRun = "New run";
        public const string WriteAnyway = "Write anyway";

        /// <summary>Beside each region option, selects and zooms to it.</summary>
        public const string Show = "Show";

        public const string ShowTip = "Select it in the 00 link and zoom to it";

        public const string OpenAPlanShowingTheLink = "Open a plan that shows the 00 link";

        public const string RegionNotFound = "Region not found in a loaded 00 link";

        public const string RegionNotFoundTip =
            "No loaded 00 link holds this filled region now. Read the model again.";

        public const string NotAPlanTip =
            "The open view is not a plan. A sheet, a schedule or a 3D view does not "
            + "draw a filled region from the 00 link.";

        public const string LinkHiddenTip =
            "The open plan hides the 00 link, by its category, the link instance or its type.";

        public const string RegionNotDrawnTip =
            "The open plan draws the 00 link by host view, or through another of its "
            + "views, so this filled region is not drawn. A plan drawing the link by the "
            + "region's own linked view shows it.";

        public static string RegionShown(string typeName)
        {
            return "Selected " + (typeName ?? string.Empty).Trim();
        }

        /// <summary>The result's status line when Retry failed leaves plots out.</summary>
        public static string NeedAChangeFirst(int plots)
        {
            return Count(plots, "plot needs", "plots need") + " a change first";
        }

        public static string PdfsNotWritten(int missing)
        {
            return Count(missing, "PDF", "PDFs") + " not written";
        }

        public static string NotCreated(int failed)
        {
            return Count(failed, "workbook", "workbooks") + " not created";
        }

        /// <summary>The status line under the result, counted off the run's own outcomes.</summary>
        public static string ResultStatus(int created, int failed)
        {
            return Number(created) + " created, " + Number(failed) + " failed";
        }

        private static string Count(int howMany, string one, string many)
        {
            return Number(howMany) + " " + (howMany == 1 ? one : many);
        }
    }

    /// <summary>
    /// A line for the pane's status line and the full text behind it. **The short half is
    /// what the line shows and the detail is its tooltip**, because a status line carrying an
    /// exception message ran three lines down a 300 pixel pane.
    /// </summary>
    public sealed class StatusLine
    {
        public StatusLine(string text, string detail = null)
        {
            Text = text ?? string.Empty;
            Detail = detail ?? string.Empty;
        }

        public string Text { get; }

        public string Detail { get; }

        public static readonly StatusLine NoModel = new StatusLine(KpiPaneWords.NoModelOpen);

        public static StatusLine RevitRefused(string message)
        {
            return new StatusLine("Revit refused the request.", message);
        }

        public static StatusLine RevitWouldNot(string message)
        {
            return new StatusLine("Revit would not do that now.", message);
        }

        public static StatusLine BadArgument(string message)
        {
            return new StatusLine("Revit refused an argument.", message);
        }

        public static StatusLine ReportRefused(string message)
        {
            return new StatusLine("The report folder refused the write.", message);
        }

        public static StatusLine ReportNotWritten(string message)
        {
            return new StatusLine("The report could not be written.", message);
        }

        public static StatusLine Stopped(string kind, string message)
        {
            return new StatusLine(
                "The request failed and was stopped.",
                "That request failed and was stopped here rather than being let out. "
                    + (kind ?? string.Empty) + ". " + (message ?? string.Empty));
        }

        public static StatusLine NotRemembered(string pointerFileName)
        {
            return new StatusLine(
                "Could not remember that choice.",
                "The choice could not be remembered. " + (pointerFileName ?? string.Empty)
                    + " beside the installed add-in refused the write.");
        }

        public static StatusLine NotOpened(string path, string message)
        {
            return new StatusLine(
                "Could not open that.",
                "That could not be opened: " + (path ?? string.Empty) + ". " + (message ?? string.Empty));
        }

        public static readonly StatusLine Reading =
            new StatusLine(KpiPaneWords.ReadingTheModel, CreateWords.ReadingNow);

        public static readonly StatusLine Creating = new StatusLine(CreateWords.Creating);
    }

    /// <summary>
    /// The workbook rows' names on the pane, in sentence case, Existing parks rather than
    /// EXISTING PARKS. **The template's own name is unchanged everywhere else**, the report
    /// and the folder tables included, because those spellings are the client's.
    /// </summary>
    public static class AssetTypeNames
    {
        public static string Display(KpiTemplate template)
        {
            if (template == null) return string.Empty;

            string[] words = ComponentMatch.Key(template.Name).Split(' ');
            int last = words.Length;

            // MORE THAN 60 at the end of a name reads >60% on the pane and LESS THAN 60 reads
            // <60%, so the four 60 per cent rows fit the asset grid as Mosques >60%.
            string tail = string.Empty;
            if (last >= 4 && words[last - 2] == "THAN" && (words[last - 3] == "MORE" || words[last - 3] == "LESS"))
            {
                tail = " " + (words[last - 3] == "MORE" ? ">" : "<") + words[last - 1].TrimEnd('%') + "%";
                last -= 3;
            }

            string name = string.Join(" ", words.Take(last).ToArray()).ToLowerInvariant();
            return (name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name.Substring(1)) + tail;
        }
    }

    /// <summary>
    /// The first and last plot of a list in the order the pane lists them, EP-01 to EP-14,
    /// or the one id where there is one.
    /// </summary>
    public static class PlotSpan
    {
        public static string Of(System.Collections.Generic.IReadOnlyList<string> plots)
        {
            if (plots == null || plots.Count == 0) return string.Empty;
            if (plots.Count == 1) return plots[0];

            return plots[0] + " to " + plots[plots.Count - 1];
        }
    }

    /// <summary>
    /// Which half of a press the window is counting. **Two labels so neither count goes
    /// backwards**: the reads count up to the plots read and the writes count up again from
    /// one under their own word.
    /// </summary>
    public enum PressPhase
    {
        ReadingTheModel,
        Reading,
        Creating
    }

    /// <summary>
    /// How far a press has got, carried as values so the window never reads a number back out
    /// of a sentence.
    /// </summary>
    public sealed class PlotProgress
    {
        private PlotProgress(PressPhase phase, int number, int of, string plotId)
        {
            Phase = phase;
            Number = number;
            Of = of;
            PlotId = plotId ?? string.Empty;
        }

        /// <summary>The scan inside Create, which reads the whole model and has no plot.</summary>
        public static readonly PlotProgress TheModel = new PlotProgress(PressPhase.ReadingTheModel, 0, 0, string.Empty);

        public static PlotProgress Reading(int number, int of, string plotId)
        {
            return new PlotProgress(PressPhase.Reading, number, of, plotId);
        }

        public static PlotProgress Creating(int number, int of, string plotId)
        {
            return new PlotProgress(PressPhase.Creating, number, of, plotId);
        }

        public PressPhase Phase { get; }

        public int Number { get; }

        public int Of { get; }

        public string PlotId { get; }

        public string InWords
        {
            get
            {
                switch (Phase)
                {
                    case PressPhase.ReadingTheModel: return ProgressWords.ReadingTheModel;
                    case PressPhase.Reading: return ProgressWords.Reading(Number, Of);
                    default: return ProgressWords.Creating(Number, Of);
                }
            }
        }
    }
}
