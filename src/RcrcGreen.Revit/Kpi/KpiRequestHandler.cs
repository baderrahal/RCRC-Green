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
            double readSeconds = 0.0;

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

                OneTemplate(document, asked, pick, share, root, streets, runs, outcomes,
                    plotOutcomes, ref readSeconds, ref plotsRead, plotsToRead);
            }

            // **Every ticked plot is accounted for**, including the ones no template took, which
            // would otherwise be counted by the split and by nothing else.
            foreach (PlotTemplate left in split.Unplaced)
            {
                plotOutcomes.Add(PlotOutcome.WroteNothing(
                    left.PlotId, left.Template, PlotWorkbookPath.Refused(left.Why), false, left.Why));
            }

            var set = new KpiCreateRunSet(
                document.Title, split, runs, outcomes,
                RunTiming.Of(whole.Elapsed.TotalSeconds, readSeconds),
                plotOutcomes, streets);

            Progressed?.Invoke(ProgressWords.WritingTheReport);
            DateTime writtenAt = DateTime.Now;

            // **ONE REPORT FOR THE RUN, not one per workbook.**
            IReadOnlyList<string> written = ReportFile.Write(
                KpiFile.NameFor(document.Title + "_checklist", writtenAt),
                KpiCreateReport.WriteAll(set, writtenAt));

            CreatedAcross?.Invoke(set, ReportPlaces.Written(written));
            Told?.Invoke(CreateWords.WroteAcross(set, ReportPlaces.Written(written)));
        }

        /// <summary>
        /// One ticked template's whole run, exactly as one press used to be. **NOTHING ABOUT THE
        /// PER TEMPLATE LOGIC CHANGES**: its own map, its own tree lists read off its own file,
        /// its own Street Design rule, its own group rules, its own canopy check, its own read
        /// back, its own cache fix and its own alias list.
        ///
        /// **A refusal on one template does not stop the others.** Everything that can end one
        /// workbook ends this method and leaves an outcome behind, and the loop above carries on
        /// to the next.
        /// </summary>
        private void OneTemplate(
            Document document,
            KpiCreateAsk asked,
            TemplatePick pick,
            TemplateShare share,
            string root,
            StreetReferenceFile streets,
            List<KpiCreateRun> runs,
            List<TemplateOutcome> outcomes,
            List<PlotOutcome> plotOutcomes,
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

            Progressed?.Invoke(ProgressWords.AddingUp);

            if (!source.Reused) readSeconds += reading.Elapsed.TotalSeconds;

            // **ONE WORKBOOK PER PLOT.** The read above is unchanged, share and all, because the
            // held readings, the progress count and the area unit are all decided once per
            // template. What changed is below it: each plot's own reading is filled, patched and
            // filed on its own rather than added into one workbook with its neighbours.
            string location = KpiPlotReader.Location(document, asked.LocationParameter);
            SpeciesList existing = SpeciesList.In(pick.TemplatePath, pick.Template.ExistingTrees);
            SpeciesList proposed = SpeciesList.In(pick.TemplatePath, pick.Template.ProposedTrees);

            // Character and Context go into the cell right of their label on this template's own
            // sheet, so the labels are found ONCE per template off the template file, beside the
            // two tree lists, rather than once per plot off files that are all copies of it.
            LabelledCells labels = LabelledPlaces.In(pick.TemplatePath, pick.Template);

            var wrote = new List<string>();
            var why = new List<string>();

            foreach (PlotReading one in readings)
            {
                OnePlot(
                    document, asked, pick, counted, one, location, existing, proposed, labels,
                    areaUnit, source, reading.Elapsed.TotalSeconds, root, streets,
                    runs, plotOutcomes, wrote, why);
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
            ProjectUnit areaUnit,
            ReadingsSource source,
            double readSeconds,
            string root,
            StreetReferenceFile streets,
            List<KpiCreateRun> runs,
            List<PlotOutcome> plotOutcomes,
            List<string> wrote,
            List<string> why)
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

            // **The folder comes off the plot's OWN component, not the template's.** DAILY MOSQUE
            // and FRIDAY MOSQUE fill one workbook and are filed in two folders, so a folder read
            // off the template would put half the mosque plots in the wrong place. The template
            // is passed for the one case that has no component at all, where its own folder is
            // the only thing that can file the plot.
            PlotWorkbookPath where = PlotWorkbookPath.For(root, pick.Template, component.Value, held.Uid2);

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
                folderMade = MadeTheFolder(where, out string folderRefusal);
                if (!folderMade) outcome = PatchOutcome.Refused(folderRefusal);
                else
                {
                    outcome = Patched(
                        pick.TemplatePath, where.FilePath, plan.Writes, plan.ComputesFrom, Progressed);
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

            if (run.Wrote)
            {
                wrote.Add(held.PlotId);
                plotOutcomes.Add(PlotOutcome.Wrote(held.PlotId, pick.Template, where));
                return;
            }

            string refusal = where.Ok ? CreateWords.WhyThisOneWroteNothing(run) : where.Why;
            why.Add(held.PlotId + ": " + refusal);
            plotOutcomes.Add(PlotOutcome.WroteNothing(
                held.PlotId, pick.Template, where, folderMade, refusal));
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
