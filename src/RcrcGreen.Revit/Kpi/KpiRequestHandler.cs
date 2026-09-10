using System;
using System.Collections.Generic;
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
        Scan,
        Plots,
        Create
    }

    /// <summary>
    /// The only route from the KPI pane to the Revit API. Its own handler and its own
    /// external event, so nothing the KPI pane asks for goes through the Drawing Sheet's and
    /// a failure in one can never cost the other.
    ///
    /// Two requests. WhichModel reads the document title and nothing else, so the pane can
    /// name the model the moment it is shown. Scan reads the whole document into a
    /// <see cref="KpiScan"/> and writes the report. Neither opens a transaction, because
    /// neither writes to the model, and adding one would be the first step toward a tool that
    /// changes a model while claiming to read it.
    /// </summary>
    internal sealed class KpiRequestHandler : IExternalEventHandler
    {
        private readonly object _asking = new object();

        private KpiRequest _wanted = KpiRequest.Nothing;

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
        public Action<KpiCreateRun, string> Created { get; set; }

        public Action<string> Told { get; set; }

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
                    case KpiRequest.Scan:
                        Scan(document);
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
            KpiScan scan = KpiReader.Read(document);

            DateTime writtenAt = DateTime.Now;
            IReadOnlyList<string> written = ReportFile.Write(
                KpiFile.NameFor(scan.Document.Title, writtenAt),
                KpiReport.Write(scan, writtenAt));

            Scanned?.Invoke(scan, writtenAt);
            Told?.Invoke(KpiPaneWords.Headline(scan, ReportPlaces.Written(written)));
        }

        /// <summary>
        /// The plots and the two lists the dropdowns offer, read in one pass. The component
        /// per plot comes with them, because it is what preselects a template and asking for
        /// it a second time would read the sheets twice.
        /// </summary>
        private void ReadThePlots(Document document)
        {
            IReadOnlyList<string> componentNames = KpiPlotReader.ComponentNames(document);
            PlotsInTheModel plots = KpiPlotReader.Plots(document);

            FoundPlots?.Invoke(new KpiPlotFacts(
                plots,
                componentNames,
                KpiPlotReader.LocationNames(document),
                KpiPlotReader.ValuePerPlot(document, componentNames.Count == 0 ? string.Empty : componentNames[0]),
                KpiPlotReader.ReferenceValuesPerPlot(document)));
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
            string outputFolder = OutputFolder.Read();

            string cannot = CreateWords.CannotCreate(
                OpenModel.Of(document.Title),
                outputFolder,
                asked != null && asked.Template != null,
                asked != null && asked.Ticked.Count > 0);

            if (cannot.Length > 0)
            {
                Told?.Invoke(cannot);
                return;
            }

            IReadOnlyList<string> phases = PhaseNames(document);
            var readings = new List<PlotReading>();

            foreach (string plotId in asked.Ticked)
            {
                IReadOnlyList<RegionArea> regions = KpiPlotReader.RegionsFor(document, plotId);

                // One region holding an area answers itself. More than one is a question the
                // type name cannot settle, so it is left unchosen and the reconciliation
                // refuses until a person picks.
                string chosen = asked.RegionChosenFor(plotId);
                if (chosen.Length == 0)
                {
                    List<RegionArea> holding = regions.Where(one => one.HoldsAnArea).ToList();
                    if (holding.Count == 1) chosen = holding[0].TypeName;
                }

                readings.Add(KpiPlotReader.Read(
                    document, plotId, asked.ComponentParameter, asked.ReferenceParameter,
                    phases, chosen, regions));
            }

            Totalled area = KpiMerge.Area(readings);
            Totalled shrubs = KpiMerge.Shrubs(readings);
            Totalled lawn = KpiMerge.Lawn(readings);
            AgreedValue component = KpiMerge.Component(readings);
            AgreedValue reference = KpiMerge.Reference(readings);

            Reconciliation reconciliation = Reconciliation.Of(
                asked.Ticked, readings, new[] { area, shrubs, lawn }, asked.IdenticalAreasConfirmed);

            string location = KpiPlotReader.Location(document, asked.LocationParameter);
            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(readings);

            KpiCreatePlan plan = null;
            PatchOutcome outcome = null;
            string outputPath = string.Empty;

            if (reconciliation.AddsUp)
            {
                SpeciesList existing = SpeciesList.In(asked.TemplatePath, asked.Template.ExistingTrees);
                SpeciesList proposed = SpeciesList.In(asked.TemplatePath, asked.Template.ProposedTrees);

                plan = KpiCreatePlan.Of(
                    asked.Template, component, reference, location, area, shrubs, lawn,
                    SpeciesMatching.Against(merged, asked.Template, existing, proposed),
                    asked.Date, asked.PreparedBy, asked.Position);

                outputPath = Path.Combine(outputFolder, OutputName.Final(asked.OutputName));
                outcome = Patched(asked.TemplatePath, outputPath, plan.Writes);
            }
            else
            {
                plan = KpiCreatePlan.Of(
                    asked.Template, component, reference, location, area, shrubs, lawn, null,
                    asked.Date, asked.PreparedBy, asked.Position);
            }

            var run = new KpiCreateRun(
                document.Title, asked.Template, asked.TemplatePath, outputPath,
                asked.ComponentParameter, asked.ReferenceParameter, location,
                readings, reconciliation, plan, area, shrubs, lawn, component, reference,
                merged, KpiMerge.Ungrouped(readings), outcome);

            DateTime writtenAt = DateTime.Now;
            IReadOnlyList<string> written = ReportFile.Write(
                KpiFile.NameFor(document.Title + "_checklist", writtenAt),
                KpiCreateReport.Write(run, writtenAt));

            Created?.Invoke(run, ReportPlaces.Written(written));
            Told?.Invoke(CreateWords.Wrote(run, ReportPlaces.Written(written)));
        }

        /// <summary>
        /// An existing file of that name is overwritten, which the pane says once under the
        /// name box. The delete is here rather than left to the patcher so the overwrite is a
        /// deliberate line rather than a side effect of how a zip happens to open.
        /// </summary>
        private static PatchOutcome Patched(string templatePath, string outputPath, IReadOnlyList<CellWrite> writes)
        {
            try
            {
                if (File.Exists(outputPath)) File.Delete(outputPath);

                return WorkbookPatcher.Patch(templatePath, outputPath, writes);
            }
            catch (UnauthorizedAccessException denied)
            {
                return PatchOutcome.Refused("The folder refused the workbook. " + denied.Message);
            }
            catch (IOException failed)
            {
                return PatchOutcome.Refused("The workbook could not be written. " + failed.Message);
            }
        }

        /// <summary>
        /// The document's own phase names, which is what tells a group row from a subtotal in
        /// a printed schedule. The words Existing and Proposed are nowhere in this code.
        /// </summary>
        private static IReadOnlyList<string> PhaseNames(Document document)
        {
            var found = new List<string>();

            foreach (Phase phase in document.Phases)
            {
                if (phase != null && !string.IsNullOrWhiteSpace(phase.Name)) found.Add(phase.Name);
            }

            return found;
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
