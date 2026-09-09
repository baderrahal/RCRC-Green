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
        /// Called back on the Revit thread with the document title and the folder the model
        /// file sits in, empty for a model that has never been saved. The pane marshals to
        /// its own thread itself. The folder is here because the filled workbook goes beside
        /// the model, and only the Revit side can say where that is.
        /// </summary>
        public Action<string, string> Named { get; set; }

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
        /// One slot. A Scan already waiting is kept when the pane asks for the model name,
        /// because the pane asks every time it is shown and Revit can take a while to get to
        /// the event, and a scan that the name request had overwritten left the status line
        /// reading Scanning with nothing written. A scan names the model on its own.
        /// </summary>
        public void Ask(KpiRequest wanted)
        {
            lock (_asking)
            {
                if (_wanted == KpiRequest.Scan && wanted == KpiRequest.WhichModel) return;
                if (_wanted == KpiRequest.Create && wanted == KpiRequest.WhichModel) return;
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
                Named?.Invoke(string.Empty, string.Empty);
                Told?.Invoke(KpiPaneWords.NoModel);
                return;
            }

            try
            {
                switch (wanted)
                {
                    case KpiRequest.WhichModel:
                        Named?.Invoke(document.Title, FolderOf(document));
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
        /// Empty for a model that has never been saved. A cloud model's path is not a folder
        /// on disk either, so anything that does not exist as a directory comes back empty
        /// and the pane says to save the model first.
        /// </summary>
        private static string FolderOf(Document document)
        {
            try
            {
                string path = document.PathName;
                if (string.IsNullOrEmpty(path)) return string.Empty;

                string folder = System.IO.Path.GetDirectoryName(path);
                return !string.IsNullOrEmpty(folder) && System.IO.Directory.Exists(folder)
                    ? folder
                    : string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
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
            string first = plots.All.Count == 0 ? string.Empty : plots.All[0];

            FoundPlots?.Invoke(new KpiPlotFacts(
                plots,
                componentNames,
                KpiPlotReader.LocationNames(document),
                FolderOf(document),
                KpiPlotReader.ValuePerPlot(document, componentNames.Count == 0 ? string.Empty : componentNames[0]),
                KpiPlotReader.ReferenceChoices(document, first)));
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
            if (asked == null || asked.Template == null)
            {
                Told?.Invoke(CreateWords.CannotCreate(true, false, true));
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
                    SpeciesMatching.Against(merged, asked.Template, existing, proposed));

                outputPath = Path.Combine(FolderOf(document), OutputName.Final(asked.OutputName));
                outcome = Patched(asked.TemplatePath, outputPath, plan.Writes);
            }
            else
            {
                plan = KpiCreatePlan.Of(
                    asked.Template, component, reference, location, area, shrubs, lawn, null);
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
