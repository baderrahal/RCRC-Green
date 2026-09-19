using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RcrcGreen.Core;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    internal enum KpiRequest
    {
        Nothing,
        WhichModel,
        Plots,
        Create
    }

    /// <summary>
    /// The only route from the KPI pane to the Revit API. Its own handler and its own
    /// external event, so nothing the KPI pane asks for goes through the Drawing Sheet's and
    /// a failure in one can never cost the other.
    ///
    /// Three requests. WhichModel reads the document title and nothing else, so the pane can
    /// name the model the moment it is shown. Plots reads the plots, the dropdown names and
    /// the element count the header shows. Create fills the workbook, and the scan of the
    /// whole model is a step inside it rather than a request of its own, because pressing
    /// Create used to want a scan the user had to know to do first. None of them opens a
    /// transaction, because none writes to the model, and adding one would be the first step
    /// toward a tool that changes a model while claiming to read it.
    /// </summary>
    internal sealed class KpiRequestHandler : IExternalEventHandler
    {
        private readonly object _asking = new object();

        private KpiRequest _wanted = KpiRequest.Nothing;

        // The model the scan held was read from, empty until one has run. Create reads the
        // model when this does not name the document it is filling for, which is what makes
        // the scan a step inside Create rather than a button somebody has to know to press.
        private string _scannedTitle = string.Empty;

        // What the linked models were doing when that scan read. It travels onto the run so
        // the checklist report can open with the reason a run found nothing, which the 16:06
        // STREETS run over 78 plots had nowhere to say.
        private LinksLoaded _scannedLinks = LinksLoaded.NotRead;

        /// <summary>
        /// Which file in the forms folder is which form, held for the press. **Deciding it
        /// means opening and scanning a PDF**, and on 78 street plots that is 78 opens of one
        /// file. Keyed on the folder and the form together, so browsing elsewhere decides again.
        /// </summary>
        private readonly Dictionary<string, string> _formFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Called back on the Revit thread with the document title, empty when no document is
        /// open. The pane marshals to its own thread itself.
        ///
        /// **The model's folder used to travel with it and does not any more.** The filled
        /// workbook went beside the model, so a detached model could not be used at all. It
        /// goes to a browsed folder now and nothing asks the document where it sits.
        /// </summary>
        public Action<string> Named { get; set; }

        public Action<KpiScan, DateTime> Scanned { get; set; }

        /// <summary>
        /// The plots the model holds and the names the two dropdowns offer, read before
        /// anything is ticked.
        /// </summary>
        public Action<KpiPlotFacts> FoundPlots { get; set; }

        /// <summary>
        /// What one press of Create really did. The run carries its own outcome, so the pane
        /// counts nothing of its own.
        /// </summary>
        public Action<KpiCreateRunSet, string> CreatedAcross { get; set; }

        public Action<string> Told { get; set; }

        /// <summary>
        /// A line the pane shows while a scan or a create runs, raised as the work it names
        /// begins. The words and the counting are Core's, in ProgressWords, and the run's own
        /// end line always follows through Told, so the last thing on screen is never a count
        /// that stopped moving.
        /// </summary>
        public Action<string> Progressed { get; set; }

        /// <summary>
        /// What the next Create is to do. Set by the pane immediately before it asks, and read
        /// once on the Revit thread.
        /// </summary>
        public KpiCreateAsk Asked { get; set; }

        /// <summary>
        /// One slot, and WhichModel never takes it from anything.
        ///
        /// The pane asks for WhichModel on every draw, so it is the request most likely to
        /// arrive on top of another. It used to displace Plots, which the pane asks for in the
        /// same breath when it is shown, so the plot list never arrived. Losing a WhichModel
        /// costs nothing instead, because **every answer from here carries the model state**
        /// whatever was asked for.
        /// </summary>
        public void Ask(KpiRequest wanted)
        {
            if (wanted == KpiRequest.Nothing) return;

            lock (_asking)
            {
                if (wanted == KpiRequest.WhichModel && _wanted != KpiRequest.Nothing) return;

                _wanted = wanted;
            }
        }

        public string GetName()
        {
            return "RCRC Green KPI";
        }

        /// <summary>
        /// Nothing may leave here. An exception out of an IExternalEventHandler does not raise
        /// a dialog, it ends Revit along with whatever the user had not saved.
        /// </summary>
        public void Execute(UIApplication application)
        {
            try
            {
                Run(application);
            }
            catch (Exception failed)
            {
                Stop(failed);
            }
        }

        private void Run(UIApplication application)
        {
            KpiRequest wanted;
            lock (_asking)
            {
                wanted = _wanted;
                _wanted = KpiRequest.Nothing;
            }

            if (wanted == KpiRequest.Nothing) return;

            UIDocument open = application.ActiveUIDocument;
            Document document = open == null ? null : open.Document;

            if (document == null)
            {
                // The document can be closed while the pane is still on screen. That is
                // ordinary, so it is said rather than thrown.
                Named?.Invoke(string.Empty);
                Told?.Invoke(KpiPaneWords.NoModel);
                return;
            }

            // Every answer carries the model state, whatever was asked for, read off the live
            // document at the moment it is read. THE PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
            // FOR: a model saved while the pane sat open left Create refusing on a folder that
            // had been read once and never again.
            Named?.Invoke(document.Title);

            try
            {
                switch (wanted)
                {
                    case KpiRequest.WhichModel:
                        // Said above. This request exists to ask for that and nothing else.
                        break;
                    case KpiRequest.Plots:
                        ReadThePlots(document);
                        break;
                    case KpiRequest.Create:
                        Create(document);
                        break;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                Told?.Invoke("Revit refused that. " + failed.Message);
            }
            catch (InvalidOperationException failed)
            {
                Told?.Invoke("Revit would not do that now. " + failed.Message);
            }
            catch (ArgumentException failed)
            {
                Told?.Invoke("Revit refused that as a bad argument. " + failed.Message);
            }
            catch (UnauthorizedAccessException denied)
            {
                Told?.Invoke("The Desktop folder refused the report. " + denied.Message);
            }
            catch (IOException failed)
            {
                Told?.Invoke("The report could not be written. " + failed.Message);
            }
        }

        /// <summary>
        /// The report is written even when parts of the read were refused, because the parts
        /// that were read are the measurements this round exists for, and the file names every
        /// part that was not.
        /// </summary>
        private void Scan(Document document)
        {
            KpiScan scan = KpiReader.Read(document, Progressed);

            Progressed?.Invoke(ProgressWords.WritingTheReport);
            DateTime writtenAt = DateTime.Now;
            IReadOnlyList<string> written = ReportFile.Write(
                KpiFile.NameFor(scan.Document.Title, writtenAt),
                KpiReport.Write(scan, writtenAt));

            // What was read, so a second Create on the same model does not read it again.
            _scannedTitle = scan.Document.Title ?? string.Empty;
            _scannedLinks = LinksLoaded.Of(scan.Links);

            Scanned?.Invoke(scan, writtenAt);

            // Through Progressed rather than Told. This runs INSIDE Create now, and Told is
            // the run's end line: saying the scan headline there would end the press on
            // screen while the workbook was still being filled, and would shut the progress
            // window with the work still running.
            Progressed?.Invoke(KpiPaneWords.Headline(scan, ReportPlaces.Written(written)));
        }

        /// <summary>
        /// The plots and the two lists the dropdowns offer, read in one pass. The component
        /// per plot comes with them, because it is what preselects a template and asking for
        /// it a second time would read the sheets twice.
        /// </summary>
        private void ReadThePlots(Document document)
        {
            var clock = Stopwatch.StartNew();

            IReadOnlyList<string> componentNames = KpiPlotReader.ComponentNames(document);
            PlotsInTheModel plots = KpiPlotReader.Plots(document);
            IReadOnlyList<string> locationNames = KpiPlotReader.LocationNames(document);
            IDictionary<string, string> perPlot = KpiPlotReader.ValuePerPlot(
                document, componentNames.Count == 0 ? string.Empty : componentNames[0]);
            IDictionary<string, IReadOnlyList<PlotParameterValue>> references =
                KpiPlotReader.ReferenceValuesPerPlot(document);

            // The header's element count comes back with the plots, so the model's name has a
            // number under it as soon as the pane is shown. It used to come off the scan alone,
            // which meant the header sat empty until somebody pressed a button, and the button
            // is gone. Counted the same way the scan counts it, instances and not types.
            int instances = new FilteredElementCollector(document).WhereElementIsNotElementType().GetElementCount();

            // The link state comes back with the plots so the pane can say, BEFORE anybody
            // presses Create, that no link is loaded and the schedules will list nothing. Read
            // off the instances alone, never their contents, because what each link holds is
            // the scan's job and costs minutes.
            LinksLoaded links = LinksLoaded.Of(new LinkFacts(null, LinkInstances(document), null));
            clock.Stop();

            FoundPlots?.Invoke(new KpiPlotFacts(
                plots,
                componentNames,
                locationNames,
                perPlot,
                references,
                instances,
                clock.Elapsed.TotalSeconds,
                links));
        }

        /// <summary>
        /// Every link instance with whether it handed back a document, which is what loaded
        /// means here. The same read <see cref="KpiLinkReader"/> makes, without the contents,
        /// because the pane needs the state and never the regions.
        /// </summary>
        private static IEnumerable<ScannedLinkInstance> LinkInstances(Document document)
        {
            var found = new List<ScannedLinkInstance>();
            foreach (RevitLinkInstance instance in new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>())
            {
                found.Add(new ScannedLinkInstance(
                    instance.Name,
                    ParameterReading.NameOf(document, instance.GetTypeId()),
                    instance.GetLinkDocument() != null));
            }

            return found;
        }

        /// <summary>
        /// One press of Create, in order: read once per chosen plot, merge, reconcile, and
        /// only then copy the template and patch it. The reconciliation runs before anything
        /// is copied, so a refusal leaves no file behind at all.
        ///
        /// Nothing here opens a transaction. This creates nothing in the model, and the
        /// template is opened for reading and never written to.
        /// </summary>
        private void Create(Document document)
        {
            KpiCreateAsk asked = Asked;

            // **The refusal is decided here, at the moment Create runs**, off the live document
            // and off the pointer file rather than off anything the pane read earlier. A pane
            // deciding on a folder it had read once is what left Create refusing after the model
            // was saved, and the output folder can be browsed for a moment before this runs.
            // **The root of the folder tree**, which is the folder the user already browses for.
            // It is one remembered folder rather than two, because the workbooks go under it now
            // and a second folder deciding nothing is how a stale string became a dead end here
            // already.
            string root = OutputFolder.Read();

            // The reference file is read ONCE for the whole press. It is 8,353 rows and every
            // street plot asks the same copy of it.
            StreetReferenceFile streets = StreetReferenceFile.In(StreetReferenceFileSetting.Read());

            string cannot = CreateWords.CannotCreate(
                OpenModel.Of(document.Title),
                root,
                asked != null && asked.Templates.Count > 0,
                asked != null && asked.Ticked.Count > 0);

            if (cannot.Length > 0)
            {
                Told?.Invoke(cannot);
                return;
            }

            // **NOTHING TIMED THIS RUN AND IT TOOK ABOUT FIVE MINUTES OVER 20 PLOTS.** The
            // whole press, the read apart from it, and each plot's own share, so the next run
            // says which part is slow rather than leaving it to be reasoned about.
            var whole = Stopwatch.StartNew();

            // **THE SCAN IS A STEP INSIDE CREATE.** Pressing Create used to want a scan the
            // user had to know to do first, which is the tool's business and not theirs. The
            // model is read when there is nothing held or what is held is of another model,
            // and the scan already held answers a second press on the same model, the same
            // rule the per plot readings follow. Core decides it and says which happened, so
            // the screen is never silent about a press that skipped two minutes of work.
            ScanSource scanned = ScanNeeded.Decide(_scannedTitle, document.Title);
            if (scanned.Held) Progressed?.Invoke(ProgressWords.ReusingTheScan);
            else Scan(document);

            // **EACH WORKBOOK GETS ONLY THE PLOTS WHOSE OWN TEMPLATE IS THAT ONE.** PRX_Component
            // decides, the plot prefix is the cross check, and a plot neither can place goes into
            // no workbook and is named. A plot has one template and one only, which is why a plot
            // read for MOSQUES is never read again for STREETS: the split is what makes the read
            // once, and no cache sits beside it.
            TemplateSplit split = PlotsPerTemplate.Split(
                asked.Ticked, asked.ComponentOn, asked.Templates.Select(one => one.Template));

            var runs = new List<KpiCreateRun>();
            var outcomes = new List<TemplateOutcome>();
            var plotOutcomes = new List<PlotOutcome>();
            var reads = new List<TemplateReading>();
            var treeLists = new List<TreeListSheetCheck>();
            double readSeconds = 0.0;

            // **Each press decides which file is which form once.** Held across the press so 78
            // street plots open one file once, and cleared at the start of it so a corrected
            // form dropped into the folder between two presses is seen on the second.
            _formFiles.Clear();

            // **THE COUNT ONLY GROWS, ACROSS THE WHOLE PRESS.** Counted per template it would
            // restart at 1 on the second workbook, and a progress line that goes backwards is
            // the one thing the rule about it forbids. The total is every plot every ticked
            // template will read, worked out before the first one is.
            int plotsToRead = split.Writing.Sum(one => one.Plots.Count);
            int plotsRead = 0;

            foreach (TemplatePick pick in asked.Templates)
            {
                TemplateShare share = split.Shares.FirstOrDefault(
                    one => ReferenceEquals(one.Template, pick.Template));

                // **A TICKED TEMPLATE THAT NO TICKED PLOT BELONGS TO WRITES NOTHING AND SAYS SO.**
                // It is not a refusal and not a failure. It stays ticked and stays listed.
                if (share == null || !share.WillWrite)
                {
                    outcomes.Add(TemplateOutcome.NothingToWrite(
                        pick.Template,
                        share == null
                            ? PlotsPerTemplate.NoPlotBelongs(pick.Template)
                            : share.WhyNothing));
                    continue;
                }

                // **A double count refuses the whole press before anything is copied**, because a
                // workbook written from a plot counted twice is the worst thing this tool can
                // produce and the totals still look plausible.
                if (!split.AddsUp)
                {
                    outcomes.Add(TemplateOutcome.Refused(pick.Template, share.Plots,
                        string.Join(" ", split.Refusals.ToArray())));
                    continue;
                }

                reads.Add(ReadOneTemplate(
                    document, asked, pick, share, ref readSeconds, ref plotsRead, plotsToRead));
            }

            // **THE WHOLE TICKED SET IS CHECKED BEFORE THE FIRST FILE OF THE PRESS IS WRITTEN.**
            // The press used to read one template and write it before it read the next, so the
            // first template's files had already landed when the second template's collision
            // became knowable. On the 16:37 press NS-01 and NS-42 shared one PRX_Plot_UID2 and
            // MM-01 with MM-09 to MM-15 shared another, ONE WORKBOOK PER PLOT gave each group one
            // path, and the last plot written replaced the others.
            //
            // **THE PATH IS BUILT HERE AND READ BY THE WRITE**, through the one rule that decides
            // where a plot's workbook goes, so a check reporting on a path the writer does not
            // use cannot happen.
            var filings = new List<PlotFiling>();
            foreach (TemplateReading one in reads)
            {
                foreach (PlotReading reading in one.Readings)
                {
                    filings.Add(PlotFilings.One(
                        root, one.Pick.Template, reading.PlotId,
                        KpiMerge.Component(new[] { reading }).Value, reading.Uid2));
                }
            }

            IReadOnlyList<SharedUid2Group> sharing = SharedUid2.Of(filings);

            foreach (TemplateReading one in reads)
            {
                WriteOneTemplate(
                    document, asked, one, root, streets, filings, sharing, runs, outcomes,
                    plotOutcomes, treeLists);
            }

            // **Every ticked plot is accounted for**, including the ones no template took, which
            // would otherwise be counted by the split and by nothing else.
            foreach (PlotTemplate left in split.Unplaced)
            {
                plotOutcomes.Add(PlotOutcome.WroteNothing(
                    left.PlotId, left.Template, PlotWorkbookPath.Refused(left.Why), false, left.Why));
            }

            // **THE PLOT LIST IS READ OFF THE LIVE DOCUMENT AT THE PRESS**, not taken off the
            // pane, because the report is a record of the model rather than of what a pane was
            // holding. It is the same read the plots button makes, one pass over the sheets and
            // one over the schedules, under a second on NG05.
            var set = new KpiCreateRunSet(
                document.Title, split, runs, outcomes,
                RunTiming.Of(whole.Elapsed.TotalSeconds, readSeconds),
                plotOutcomes, streets, KpiPlotReader.Plots(document),
                PlotListFile.In(PlotListFileSetting.Read()), sharing, treeLists);

            Progressed?.Invoke(ProgressWords.WritingTheReport);
            DateTime writtenAt = DateTime.Now;

            // **ONE REPORT FOR THE RUN, not one per workbook.**
            IReadOnlyList<string> written = ReportFile.Write(
                KpiFile.NameFor(document.Title + "_checklist", writtenAt),
                KpiCreateReport.WriteAll(set, writtenAt));

            // **THE PANE WANTS THE PATH AND THE STATUS LINE WANTS THE SENTENCE.**
            // `ReportPlaces.Written` builds `Report at <path>.`, which is right on the line
            // below and is not a path, so the pane's Open the report button tested it with
            // `File.Exists` and was dead on every press.
            CreatedAcross?.Invoke(set, written.FirstOrDefault() ?? string.Empty);
            Told?.Invoke(CreateWords.WroteAcross(set, ReportPlaces.Written(written)));
        }

        /// <summary>
        /// One ticked template's read, with nothing written. **NOTHING ABOUT READING A PLOT
        /// CHANGES**: its own counted groups, its own area rule, its own held readings, its own
        /// progress count and its own area unit, all decided once per template exactly as they
        /// were when the read and the write were one method.
        ///
        /// **THE READS ARE ALL MADE BEFORE ANY OF THEM IS WRITTEN**, which is the whole of this
        /// split. Two plots sharing one PRX_Plot_UID2 file at one path, and which plots share
        /// one cannot be known while the press is still reading the templates after this one.
        /// </summary>
        private TemplateReading ReadOneTemplate(
            Document document,
            KpiCreateAsk asked,
            TemplatePick pick,
            TemplateShare share,
            ref double readSeconds,
            ref int plotsRead,
            int plotsToRead)
        {
            var reading = Stopwatch.StartNew();

            // The groups that count are the ones a tree list sheet of the template is named for.
            // The document's phases used to decide it, and a third group the phases did not name,
            // Street Design, went unseen for four runs.
            CountedGroups counted = CountedGroups.Of(pick.Template);
            var readings = new List<PlotReading>();

            // **A TEMPLATE THAT NAMES NO AREA CELL HAS ITS REGIONS LEFT UNREAD**, which is none
            // of the seven since the client emptied STREETS H8 and said it comes off the 00
            // link. STREETS reads its regions like every other template now, and it does so
            // because this asks the MAP rather than the template's name.
            bool areaWanted = !pick.Template.TakesNoArea;

            // **A CHOICE MADE AFTER A REFUSAL IS APPLIED TO WHAT WAS ALREADY READ.** The run this
            // template produced last press is the record, Core says whether it can answer this
            // one, and the model is read only when it cannot. One mechanism, asked once per
            // template with that template's own share of the plots.
            ReadingsSource source = HeldReadings.Decide(
                asked.HeldRunFor(pick.Template), document.Title, pick.Template, pick.TemplatePath,
                asked.ComponentParameter, asked.ReferenceParameter, share.Plots);

            ProjectUnit areaUnit;
            if (source.Reused)
            {
                Progressed?.Invoke(ProgressWords.ReusingTheReadings);
                plotsRead += share.Plots.Count;
                areaUnit = asked.HeldRunFor(pick.Template).AreaUnit;
                readings.AddRange(HeldReadings.Applied(
                    asked.HeldRunFor(pick.Template).Readings, asked.RegionChosenFor));
            }
            else
            {
                // Read once per press: every plot's group total check allows the same room.
                areaUnit = KpiReader.AreaUnit(document);
                foreach (string plotId in share.Plots)
                {
                    plotsRead++;
                    Progressed?.Invoke(ProgressWords.ReadingPlot(plotId, plotsRead, plotsToRead));
                    readings.Add(GuardedRead(document, asked, plotId, counted, areaWanted, areaUnit));
                }
            }

            if (!source.Reused) readSeconds += reading.Elapsed.TotalSeconds;

            return new TemplateReading(
                pick, share, counted, readings, areaUnit, source, reading.Elapsed.TotalSeconds);
        }

        /// <summary>
        /// One ticked template's read, held between the read half of a press and its write half.
        /// **It carries what the read decided and nothing the write decides**, so a value on it
        /// is one the read really produced.
        /// </summary>
        private sealed class TemplateReading
        {
            public TemplateReading(
                TemplatePick pick,
                TemplateShare share,
                CountedGroups counted,
                IReadOnlyList<PlotReading> readings,
                ProjectUnit areaUnit,
                ReadingsSource source,
                double readSeconds)
            {
                Pick = pick;
                Share = share;
                Counted = counted;
                Readings = readings;
                AreaUnit = areaUnit;
                Source = source;
                ReadSeconds = readSeconds;
            }

            public TemplatePick Pick { get; }

            public TemplateShare Share { get; }

            public CountedGroups Counted { get; }

            public IReadOnlyList<PlotReading> Readings { get; }

            public ProjectUnit AreaUnit { get; }

            public ReadingsSource Source { get; }

            public double ReadSeconds { get; }
        }

        /// <summary>
        /// One ticked template's write, off the read half above. **NOTHING ABOUT THE PER TEMPLATE
        /// LOGIC CHANGES**: its own map, its own tree lists read off its own file, its own Street
        /// Design rule, its own group rules, its own canopy check, its own read back, its own
        /// cache fix and its own alias list.
        ///
        /// **A refusal on one template does not stop the others.** Everything that can end one
        /// workbook ends this method and leaves an outcome behind, and the loop above carries on
        /// to the next.
        /// </summary>
        private void WriteOneTemplate(
            Document document,
            KpiCreateAsk asked,
            TemplateReading read,
            string root,
            StreetReferenceFile streets,
            IReadOnlyList<PlotFiling> filings,
            IReadOnlyList<SharedUid2Group> sharing,
            List<KpiCreateRun> runs,
            List<TemplateOutcome> outcomes,
            List<PlotOutcome> plotOutcomes,
            List<TreeListSheetCheck> treeLists)
        {
            TemplatePick pick = read.Pick;
            TemplateShare share = read.Share;

            Progressed?.Invoke(ProgressWords.AddingUp);

            // **ONE WORKBOOK PER PLOT.** The read this runs on is `ReadOneTemplate`'s, share and
            // all, because the held readings, the progress count and the area unit are decided
            // once per template there. Here each plot's own reading is filled, patched and filed
            // on its own rather than added into one workbook with its neighbours.
            string location = KpiPlotReader.Location(document, asked.LocationParameter);

            // Character and Context go into the cell right of their label on this template's own
            // sheet, so the labels are found ONCE per template off the template file, beside the
            // two tree lists, rather than once per plot off files that are all copies of it.
            LabelledCells labels = LabelledPlaces.In(pick.TemplatePath, pick.Template);

            // **The two cells the workbook COMPUTES are found the same way and NEVER written.**
            // Their row differs by template, D9 on three and D8 on four, so a letter could not
            // reach both, and the percentage sits two columns right of its label rather than one.
            // Read once per template beside the rest.
            LabelledCells computed = LabelledPlaces.In(
                pick.TemplatePath, pick.Template, ComputedPlaces.All);

            // **WHICH COLUMN EACH TREE SHEET'S CANOPY TOTAL ADDS, read off the file.** The green
            // cover cell names the canopy cell, the canopy cell names the two totals, and each
            // total's own SUM range names the column and the rows. It is read BEFORE the two tree
            // lists because each list wants its own column, so a row that computes a canopy per
            // tree and adds none is not offered to a new species. FP-18 row 83 is why.
            IReadOnlyList<TotalCanopyColumn> totalCanopy = TotalCanopyColumns.In(
                pick.TemplatePath, pick.Template, computed.For(ComputedPlaces.GreenCoverName));

            SpeciesList existing = SpeciesList.In(
                pick.TemplatePath, pick.Template.ExistingTrees,
                TotalCanopyColumns.For(totalCanopy, pick.Template.ExistingTrees.SheetName));
            SpeciesList proposed = SpeciesList.In(
                pick.TemplatePath, pick.Template.ProposedTrees,
                TotalCanopyColumns.For(totalCanopy, pick.Template.ProposedTrees.SheetName));

            // **BOTH TREE LISTS OF THIS TEMPLATE, CELL BY CELL, BEFORE ANY PLOT OF IT IS
            // WRITTEN.** The team edited the templates on 15 September and the 16 September
            // workbooks still carried typed canopy cells, empty total canopy cells, empty water
            // cells and SUMIF ranges stopping short of the lists' own last rows, and none of it
            // showed until a plot hit a bad row. The two species lists and the canopy column are
            // the ones read above, so nothing is read a second way.
            treeLists.AddRange(TreeListCheck.In(
                pick.TemplatePath, pick.Template, totalCanopy, existing, proposed));

            var wrote = new List<string>();
            var why = new List<string>();

            // **EVERY PLOT'S WRITE IS GUARDED, audit 4 finding 66.** `GuardedRead` put a guard
            // round each plot's READ and this is a different loop, added when a workbook became
            // one plot, and it had none. A throw on plot 100 of 154 unwound out of `Create` to
            // `Run`'s catches, which say Revit refused that with no plot named and write NO
            // REPORT AT ALL, leaving 99 workbooks and 99 PDFs in the client's folder tree and no
            // record of which plots those were. The run carries on now, the plot is named with
            // the exception's type and message, and the report is written either way, which is
            // the same shape `GuardedRead` already uses.
            foreach (PlotReading one in read.Readings)
            {
                PlotFiling filing = PlotFilings.For(filings, one.PlotId);

                // **NO FILE IS WRITTEN FOR ANY PLOT SHARING A PATH WITH ANOTHER TICKED PLOT.**
                // Writing one of them and refusing the rest would pick a plot nobody chose, and
                // writing all of them is what the 16:37 press did. **What an earlier press left
                // in that folder is named and left exactly where it is**, the same rule the
                // crash row already follows: deleting it destroys the only evidence there is.
                if (SharedUid2.Stops(sharing, one.PlotId))
                {
                    string refusal = SharedUid2.WhyStopped(sharing, one.PlotId) + " "
                        + SharedUid2.AlreadyThere(AlreadyInThatFolder(filing)) + ".";

                    why.Add(one.PlotId + ": " + refusal);
                    plotOutcomes.Add(PlotOutcome.WroteNothing(
                        one.PlotId, pick.Template,
                        filing == null ? PlotWorkbookPath.Refused(refusal) : filing.Where,
                        filing != null && filing.FolderPath.Length > 0
                            && Directory.Exists(filing.FolderPath),
                        refusal));
                    continue;
                }

                GuardedWrite(
                    document, asked, pick, read.Counted, one, location, existing, proposed, labels,
                    computed, totalCanopy, read.AreaUnit, read.Source, read.ReadSeconds, filing,
                    streets, runs, plotOutcomes, wrote, why);
            }

            // **THE ROW COUNTS WHAT HAPPENED, NEVER WHAT WAS PLANNED.** A template whose plots
            // wrote some workbooks wrote them, and only one where NOTHING was written is
            // refused. The row used to read Nothing was written beside 20 of 21 plots wrote a
            // workbook, with twenty workbooks on disk.
            string where = CreateWords.WorkbooksUnder(pick.Template, wrote.Count, root);

            if (wrote.Count == 0)
            {
                outcomes.Add(TemplateOutcome.Refused(pick.Template, share.Plots,
                    CreateWords.SomePlotsWroteNothing(0, share.Plots.Count, why)));
            }
            else if (wrote.Count == share.Plots.Count)
            {
                outcomes.Add(TemplateOutcome.Wrote(pick.Template, share.Plots, wrote.Count, where));
            }
            else
            {
                outcomes.Add(TemplateOutcome.WroteSome(
                    pick.Template, share.Plots, wrote.Count, where,
                    CreateWords.SomePlotsWroteNothing(wrote.Count, share.Plots.Count, why)));
            }
        }

        /// <summary>
        /// Every file already sitting in the folder a stopped plot would have written into, by
        /// name. **NOTHING HERE DELETES OR MOVES ANY OF THEM**, so this is a list of what to go
        /// and look at rather than a tidy up.
        /// </summary>
        private static IReadOnlyList<string> AlreadyInThatFolder(PlotFiling filing)
        {
            if (filing == null || filing.FolderPath.Length == 0) return new List<string>();

            try
            {
                return Directory.Exists(filing.FolderPath)
                    ? Directory.GetFiles(filing.FolderPath).Select(Path.GetFileName).ToList()
                    : new List<string>();
            }
            catch (Exception)
            {
                // A folder that cannot be listed is not a reason to lose the refusal above it,
                // and the words it feeds already read as an absence rather than as an answer.
                return new List<string>();
            }
        }

        /// <summary>
        /// One plot's write, under a guard of its own.
        ///
        /// **A THROW ON ONE PLOT NAMES THE PLOT AND THE RUN CARRIES ON**, the same rule
        /// <see cref="GuardedRead"/> already follows for the read half, and for the same reason:
        /// twenty minutes that end with a report naming the bad plot are worth something and
        /// twenty minutes that end with one sentence are not. Every exception type is caught on
        /// purpose, because a partial report that names the bad plot is the point.
        ///
        /// **What is already on disk when it throws stays on disk**, and the row NAMES IT.
        /// Bader's decision of 15 September: nothing is deleted after a crash, the row says
        /// which step threw and which of the plot's own files the disk really holds, and the
        /// folder flag is read off the disk rather than handed in as false. A run over 154 plots
        /// that throws on one leaves that plot's folder made and perhaps a half written workbook
        /// beside it, and deleting them would destroy the only evidence about what went wrong.
        ///
        /// The step is taken off <see cref="PlotWriteTrail"/>, which <see cref="OnePlot"/> moves
        /// on as it reaches each one, so the row says where the write really got to rather than
        /// working it out backwards from what is on disk.
        ///
        /// **NOTHING HERE HAS BEEN RUN IN REVIT.** The words and the row are Core's and have
        /// tests. This wiring has not, and cannot be, from this session.
        /// </summary>
        private void GuardedWrite(
            Document document,
            KpiCreateAsk asked,
            TemplatePick pick,
            CountedGroups counted,
            PlotReading held,
            string location,
            SpeciesList existing,
            SpeciesList proposed,
            LabelledCells labels,
            LabelledCells computed,
            IReadOnlyList<TotalCanopyColumn> totalCanopy,
            ProjectUnit areaUnit,
            ReadingsSource source,
            double readSeconds,
            PlotFiling filing,
            StreetReferenceFile streets,
            List<KpiCreateRun> runs,
            List<PlotOutcome> plotOutcomes,
            List<string> wrote,
            List<string> why)
        {
            var trail = new PlotWriteTrail();

            try
            {
                OnePlot(
                    document, asked, pick, counted, held, location, existing, proposed, labels,
                    computed, totalCanopy, areaUnit, source, readSeconds, filing, streets,
                    runs, plotOutcomes, wrote, why, trail);
            }
            catch (Exception failed)
            {
                string refusal = PlotCrash.Row(
                    trail.Step, failed.GetType().Name, failed.Message, trail.OnDisk());

                why.Add(held.PlotId + ": " + refusal);

                // **THE FOLDER FLAG IS THE DISK'S ANSWER.** It was handed false here whatever
                // was on disk, so a press that made the folder and threw a step later counted
                // one folder fewer than the tree really holds.
                plotOutcomes.Add(PlotOutcome.WroteNothing(
                    held.PlotId, pick.Template,
                    trail.Where ?? PlotWorkbookPath.Refused(refusal),
                    trail.FolderExists(),
                    refusal));
            }
        }

        /// <summary>
        /// Where one plot's write got to, and the paths it was going to use, recorded AS THEY
        /// ARE REACHED. A step worked out afterwards from what is on disk is a guess, and this
        /// repository has paid for that shape before.
        ///
        /// It lives in the Revit project because only this side touches the disk. Everything it
        /// is printed through is Core's and has tests.
        /// </summary>
        private sealed class PlotWriteTrail
        {
            public CreateStep Step { get; private set; } = CreateStep.BeforeAnythingWasWritten;

            public PlotWorkbookPath Where { get; private set; }

            public string PdfPath { get; private set; } = string.Empty;

            public void Reached(CreateStep step)
            {
                Step = step ?? Step;
            }

            public void Filing(PlotWorkbookPath where)
            {
                Where = where;
            }

            public void WritingThePdf(string path)
            {
                PdfPath = path ?? string.Empty;
                Step = CreateStep.ThePdf;
            }

            public bool FolderExists()
            {
                return Where != null && Where.FolderPath.Length > 0 && Directory.Exists(Where.FolderPath);
            }

            /// <summary>
            /// Every file of this plot's own, with whether the disk holds it, read at the moment
            /// the row is built rather than remembered from earlier.
            /// </summary>
            public IReadOnlyList<CrashFile> OnDisk()
            {
                var found = new List<CrashFile>();
                if (Where == null || !Where.Ok) return found;

                found.Add(new CrashFile("the folder", Where.FolderPath, Directory.Exists(Where.FolderPath)));
                found.Add(new CrashFile("the workbook", Where.FilePath, File.Exists(Where.FilePath)));

                string pdf = PdfPath.Length > 0 ? PdfPath : PdfChecklist.Beside(Where);
                if (pdf.Length > 0) found.Add(new CrashFile("the PDF", pdf, File.Exists(pdf)));

                return found;
            }
        }

        /// <summary>
        /// A plot the press read and whose filing the press never built. It cannot happen, since
        /// the filings are built over the same readings this loop walks, and a path worked out
        /// nowhere reaching a writer as a silent empty is how a link in a chain goes missing, so
        /// it refuses and says which of the two it is.
        /// </summary>
        public const string NoFilingWasBuilt =
            "this plot's folder and file name were not worked out before the press began, "
            + "which is a bug in this tool";

        /// <summary>
        /// One plot, one workbook, one folder. **NOTHING ABOUT READING A PLOT CHANGES**: the
        /// reading was taken above by the same reader with the same rules, and this fills that
        /// one reading exactly as a share of one would have been filled.
        ///
        /// A refusal on one plot does not stop the rest, the same rule a refusal on one template
        /// already followed, so everything that can end this plot ends this method and leaves an
        /// outcome behind.
        /// </summary>
        private void OnePlot(
            Document document,
            KpiCreateAsk asked,
            TemplatePick pick,
            CountedGroups counted,
            PlotReading held,
            string location,
            SpeciesList existing,
            SpeciesList proposed,
            LabelledCells labels,
            LabelledCells computed,
            IReadOnlyList<TotalCanopyColumn> totalCanopy,
            ProjectUnit areaUnit,
            ReadingsSource source,
            double readSeconds,
            PlotFiling filing,
            StreetReferenceFile streets,
            List<KpiCreateRun> runs,
            List<PlotOutcome> plotOutcomes,
            List<string> wrote,
            List<string> why,
            PlotWriteTrail trail)
        {
            var readings = new List<PlotReading> { held };
            var mine = new[] { held.PlotId };

            Totalled area = KpiMerge.Area(readings);
            Totalled shrubs = KpiMerge.Shrubs(readings);
            Totalled lawn = KpiMerge.Lawn(readings);
            AgreedValue component = KpiMerge.Component(readings);
            AgreedValue reference = KpiMerge.Reference(readings);

            Reconciliation reconciliation = Reconciliation.Of(
                mine, readings, new[] { area, shrubs, lawn }, asked.IdenticalAreasConfirmed,
                pick.Template);

            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(readings, counted);

            // **THE PATH WAS BUILT BEFORE THE PRESS WROTE ANYTHING AND IS READ HERE.** It comes
            // off the plot's OWN component rather than the template's, because DAILY MOSQUE and
            // FRIDAY MOSQUE fill one workbook and are filed in two folders. What changed is only
            // WHEN it is worked out: the shared value check needs every ticked plot's path before
            // the first file lands, and a check reading one path while the write uses another is
            // the shape this repository keeps paying for.
            PlotWorkbookPath where = filing == null
                ? PlotWorkbookPath.Refused(NoFilingWasBuilt)
                : filing.Where;

            trail.Filing(where);

            // Only STREETS asks the reference file, and it is asked per plot on this plot's own
            // UID2 rather than on whatever the user picked under Reference.
            StreetReferenceAnswer street = ReferenceEquals(pick.Template, KpiTemplates.Streets)
                ? streets.For(held.Uid2)
                : StreetReferenceAnswer.Nothing(KpiCreatePlan.NotAStreetTemplate);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                pick.Template, component, reference, location, area, shrubs, lawn,
                reconciliation.AddsUp && where.Ok
                    ? SpeciesMatching.Against(merged, pick.Template, existing, proposed)
                    : null,
                asked.Date, asked.PreparedBy, asked.Position, street, labels);

            PatchOutcome outcome = null;
            bool folderMade = false;

            if (reconciliation.AddsUp && where.Ok)
            {
                trail.Reached(CreateStep.TheFolder);
                folderMade = MadeTheFolder(where, out string folderRefusal);
                if (!folderMade) outcome = PatchOutcome.Refused(folderRefusal);
                else
                {
                    // **THE COPY AND THE PATCH ARE TWO STEPS AND THE ROW TELLS THEM APART.** The
                    // patcher copies the template over the output and then opens the copy for
                    // update, so a throw before the file exists is the copy and one after it is
                    // the patch. The step moves on the file's own existence, read here rather
                    // than inferred in the catch.
                    trail.Reached(CreateStep.TheWorkbookCopy);
                    outcome = Patched(
                        pick.TemplatePath, where.FilePath, plan.Writes, plan.ComputesFrom,
                        said =>
                        {
                            if (File.Exists(where.FilePath)) trail.Reached(CreateStep.TheWorkbookPatch);
                            Progressed?.Invoke(said);
                        });
                }
            }

            var run = new KpiCreateRun(
                document.Title, pick.Template, pick.TemplatePath, where,
                asked.ComponentParameter, asked.ReferenceParameter, location,
                readings, reconciliation, plan, area, shrubs, lawn, component, reference,
                merged, KpiMerge.Ungrouped(readings), outcome,
                RunTiming.Of(readSeconds, source.Reused ? 0.0 : readSeconds),
                existing, proposed, source, asked.TemplatesListed, areaUnit, _scannedLinks);

            runs.Add(run);

            // **THE EXCEL IS WRITTEN FIRST AND THE PDF SECOND**, because two of the PDF's fields
            // read cells out of the workbook this run has just written. The ordering is a rule
            // and this is the line that keeps it.
            trail.WritingThePdf(PdfChecklist.Beside(where));

            PdfOutcome pdf = ThePdf(
                held, counted, street, where, run.Wrote, folderMade,
                WorkbookNumbers(
                    pick.Template, plan, outcome, existing, proposed, area, shrubs, lawn, computed,
                    totalCanopy),
                areaUnit);

            if (run.Wrote)
            {
                wrote.Add(held.PlotId);
                plotOutcomes.Add(PlotOutcome.Wrote(held.PlotId, pick.Template, where).WithPdf(pdf));
                return;
            }

            string refusal = where.Ok ? CreateWords.WhyThisOneWroteNothing(run) : where.Why;
            why.Add(held.PlotId + ": " + refusal);
            plotOutcomes.Add(PlotOutcome.WroteNothing(
                held.PlotId, pick.Template, where, folderMade, refusal).WithPdf(pdf));
        }

        /// <summary>
        /// What the PDF's two computed fields are worked out from, off this plot's own workbook.
        ///
        /// **THE EXCEL IS WRITTEN FIRST AND THE PDF SECOND**, so every part of this is a number
        /// this run really wrote: the canopy off the rows it wrote counts into, the planting,
        /// the lawn and the area off the same three totals the workbook's cells took. The
        /// formula check is the output's own, read back after the patch, and it is what says
        /// whether the workbook still computes the canopy the way this tool does.
        /// </summary>
        /// <summary>
        /// The template file the tree lists were read out of, for a line about one of its rows.
        /// **The team fixes the template**, so that is the file a row's line has to name, and
        /// both lists came out of the same one.
        /// </summary>
        private static string TemplateFileName(SpeciesList existing, SpeciesList proposed)
        {
            if (existing != null && existing.FileName.Length > 0) return existing.FileName;

            return proposed == null ? string.Empty : proposed.FileName;
        }

        private static PdfWorkbookNumbers WorkbookNumbers(
            KpiTemplate template,
            KpiCreatePlan plan,
            PatchOutcome outcome,
            SpeciesList existing,
            SpeciesList proposed,
            Totalled area,
            Totalled shrubs,
            Totalled lawn,
            LabelledCells computed,
            IReadOnlyList<TotalCanopyColumn> totalCanopy)
        {
            if (outcome == null || !outcome.Written) return PdfWorkbookNumbers.None;

            CanopyTotal canopy = CanopyArea.From(plan);

            // The diameter column is chosen per sheet off that sheet's own heading row, so the
            // canopy formula this tool expects is built with the column the list really used
            // rather than with a letter written in here.
            var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { template.ExistingTrees.SheetName, existing == null ? string.Empty : existing.DiameterColumn },
                { template.ProposedTrees.SheetName, proposed == null ? string.Empty : proposed.DiameterColumn }
            };

            // **The green cover cell is read first, because it is what names the canopy cell.**
            // The percentage check then holds its own formula against that same cell rather than
            // against a letter, so the two cannot name two different canopies.
            SummaryCellCheck greenCover = WorkbookArithmetic.GreenCoverCell(
                outcome.Formulas, template.MainSheetName,
                computed.For(ComputedPlaces.GreenCoverName),
                MappedTo(template, KpiValue.Shrubs), MappedTo(template, KpiValue.Lawn));

            SummaryCellCheck percentage = WorkbookArithmetic.PercentageCell(
                outcome.Formulas, template.MainSheetName,
                computed.For(ComputedPlaces.PercentageName),
                greenCover.CanopyCell, MappedTo(template, KpiValue.Area));

            return new PdfWorkbookNumbers(
                canopy, shrubs.Total, lawn.Total, area.Total,
                WorkbookArithmetic.Canopy(
                    outcome.Formulas, canopy, columns, TemplateFileName(existing, proposed), totalCanopy),
                greenCover, percentage);
        }

        /// <summary>
        /// The cell the template's own map names for one value, or empty where it names none.
        /// **Empty never matches a cell**, so a template with no such entry fails the check
        /// naming what the formula really read rather than passing on a null.
        /// </summary>
        private static string MappedTo(KpiTemplate template, KpiValue value)
        {
            MappedCell held = template.CellFor(value);

            return held == null ? string.Empty : held.Cell;
        }

        /// <summary>
        /// One plot's PDF, beside its workbook.
        ///
        /// **A PLOT WHOSE WORKBOOK WAS NOT WRITTEN STILL GETS ONE.** Bader's decision: the
        /// fields that read the workbook are left blank and named, and everything that comes
        /// from Revit still goes in. What it needs is the plot's own folder, which is made for
        /// the workbook and is never deleted.
        ///
        /// **No forms folder set is a NOTE and not a refusal.** Every plot says so once in its
        /// own row and the press goes through.
        /// </summary>
        private PdfOutcome ThePdf(
            PlotReading held,
            CountedGroups counted,
            StreetReferenceAnswer street,
            PlotWorkbookPath where,
            bool workbookWritten,
            bool folderMade,
            PdfWorkbookNumbers numbers,
            ProjectUnit areaUnit)
        {
            // **THE AREA UNIT GOES IN because the prefix split is held against a printed row.**
            // The species rows are added and checked against the group total the schedule
            // printed, and every printed area is already rounded, so the check earns the same
            // room per row the phase rows already earn rather than a constant written here.
            PdfPlan plan = PdfFill.Of(held, counted, street, DateTime.Today, workbookWritten, numbers, areaUnit);

            if (!plan.Wanted) return PdfOutcome.WroteNothing(held.PlotId, plan.Form, plan.Why, null);

            string folder = FormsFolder.Read();
            if (string.IsNullOrWhiteSpace(folder))
            {
                return PdfOutcome.WroteNothing(held.PlotId, plan.Form, PdfChecklist.NoFormFolder, null);
            }

            if (!folderMade || !where.Ok)
            {
                return PdfOutcome.WroteNothing(held.PlotId, plan.Form,
                    "this plot has no folder to write into. " + where.Why, null);
            }

            string why;
            string formPath = FormFileFor(folder, plan.Form, out why);
            if (formPath.Length == 0)
            {
                return PdfOutcome.WroteNothing(held.PlotId, plan.Form, why, null);
            }

            return PdfChecklist.Write(formPath, PdfChecklist.Beside(where), plan, held.PlotNh);
        }

        /// <summary>
        /// Which file in the browsed folder is this form, **decided by the fields it holds and
        /// never by its name**, and held for the press so 78 street plots open one file once.
        /// </summary>
        private string FormFileFor(string folder, PdfForm form, out string why)
        {
            why = string.Empty;
            string key = folder + "|" + form.Name;
            if (_formFiles.TryGetValue(key, out string already))
            {
                if (already.Length == 0) why = PdfChecklist.NoFormFile + form.Name;
                return already;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(folder, "*" + PdfChecklist.Extension);
            }
            catch (IOException failed)
            {
                why = "the forms folder could not be read. " + failed.Message;
                return string.Empty;
            }
            catch (UnauthorizedAccessException denied)
            {
                why = "the forms folder was refused. " + denied.Message;
                return string.Empty;
            }

            string found = PdfChecklist.FileFor(files, form, out why);
            _formFiles[key] = found;
            return found;
        }

        /// <summary>
        /// The plot's folder, made where it is not there. **It is never deleted and nothing in it
        /// is touched**, so a press into a folder somebody already has writes its file beside
        /// whatever is in there.
        /// </summary>
        private static bool MadeTheFolder(PlotWorkbookPath where, out string refusal)
        {
            refusal = string.Empty;

            try
            {
                Directory.CreateDirectory(where.FolderPath);
                return true;
            }
            catch (IOException failed)
            {
                refusal = "the folder " + where.FolderPath + " could not be made. " + failed.Message;
                return false;
            }
            catch (UnauthorizedAccessException denied)
            {
                refusal = "the folder " + where.FolderPath + " was refused. " + denied.Message;
                return false;
            }
        }


        /// <summary>
        /// One plot's read under a guard of its own. A throw on plot 60 of 78 used to fall to
        /// Run's catches, which say Revit would not do that now with no plot named and write no
        /// report. Every exception type is caught here on purpose, the same as the scan side's
        /// guard: the plot, the type and the message go on the reading as a refusal, the run
        /// carries on to the rest, the reconciliation refuses the write naming the plot, and
        /// the report is written either way. Twenty minutes that end with a report naming the
        /// bad plot are worth something, and twenty minutes that end with one sentence are not.
        /// </summary>
        private static PlotReading GuardedRead(Document document, KpiCreateAsk asked, string plotId, CountedGroups counted, bool areaWanted, ProjectUnit areaUnit)
        {
            var perPlot = Stopwatch.StartNew();
            try
            {
                IReadOnlyList<RegionArea> regions = areaWanted
                    ? KpiPlotReader.RegionsFor(document, plotId)
                    : new List<RegionArea>();

                // The rule is RegionChoice.Pick in Core, where the tests reach the copy that
                // runs. It used to be written out here and again in the test fixture, and the
                // two were not the same rule. The pick carries HOW it was chosen, so the report
                // can say the client's note decided rather than a person.
                RegionPick chosen = areaWanted
                    ? RegionChoice.Pick(regions, asked.RegionChosenFor(plotId))
                    : RegionPick.Nothing;

                return KpiPlotReader.Read(
                    document, plotId, asked.ComponentParameter, asked.ReferenceParameter,
                    counted, chosen, regions, areaUnit, perPlot.Elapsed.TotalSeconds);
            }
            catch (Exception failed)
            {
                return PlotReading.NotRead(plotId, failed.GetType().Name + ": " + failed.Message, perPlot.Elapsed.TotalSeconds);
            }
        }

        /// <summary>
        /// An existing file of that name is overwritten, which the pane says once under the
        /// name box. The delete is here rather than left to the patcher so the overwrite is a
        /// deliberate line rather than a side effect of how a zip happens to open.
        ///
        /// **THE DELETE IS WHAT MAKES THE TEMPLATE GUARD URGENT.** With the output folder
        /// browsed for, it can be the templates folder, and the name box is prefilled with the
        /// template's own file name. One press then removed the client's GRP KPI Checklist and
        /// the run ended saying only that the workbook could not be written. The guard stands
        /// BEFORE the delete, and a second one stands inside the patcher, because either alone
        /// is one refactor from being bypassed.
        /// </summary>
        private static PatchOutcome Patched(
            string templatePath, string outputPath, IReadOnlyList<CellWrite> writes, IReadOnlyList<WorkbookCell> computesFrom,
            Action<string> step)
        {
            SamePath answer = FilePaths.Compare(templatePath, outputPath);
            if (answer != SamePath.Different)
            {
                return PatchOutcome.Refused(CreateWords.WouldOverwriteTheTemplate(outputPath, answer));
            }

            try
            {
                if (File.Exists(outputPath)) File.Delete(outputPath);

                return WorkbookPatcher.Patch(templatePath, outputPath, writes, computesFrom, step);
            }
            catch (UnauthorizedAccessException denied)
            {
                return PatchOutcome.Refused("The folder refused the workbook. " + denied.Message);
            }
            catch (IOException failed)
            {
                return PatchOutcome.Refused(CreateWords.CouldNotBeWritten(failed.Message));
            }
        }

        /// <summary>
        /// The last thing before Revit would have gone down. The pane may already be gone by
        /// now, so a throw out of the report itself is caught too.
        /// </summary>
        private void Stop(Exception failed)
        {
            try
            {
                Told?.Invoke(
                    "That request failed and was stopped here rather than being let out. "
                    + failed.GetType().Name + ". " + failed.Message);
            }
            catch (Exception)
            {
            }
        }
    }
}
