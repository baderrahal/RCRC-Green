using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    internal enum DrawingSheetRequest
    {
        Nothing,
        Refresh,
        SelectView,
        AssignScopeBoxes,
        ScanModel
    }

    /// <summary>
    /// The only route from the panel to the Revit API.
    ///
    /// A dockable panel is modeless. Its own code runs whenever Windows feels like raising an
    /// event, which is almost never a moment when Revit will accept an API call. Everything
    /// the panel wants doing is asked for here and Revit runs it when it is ready. Nothing in
    /// the panel reads a Document or opens a Transaction, and there is no second way in.
    /// </summary>
    internal sealed class DrawingSheetRequestHandler : IExternalEventHandler
    {
        private readonly object _asking = new object();

        private DrawingSheetRequest _wanted = DrawingSheetRequest.Nothing;
        private long _viewToSelect;
        private IReadOnlyList<string> _plotsTicked = new List<string>();

        /// <summary>
        /// Called back on the Revit thread when a request finishes. The panel marshals to its
        /// own thread itself.
        /// </summary>
        public Action<DrawingSheetSnapshot> Read { get; set; }

        public Action<string> Told { get; set; }

        public void Ask(DrawingSheetRequest wanted)
        {
            lock (_asking)
            {
                _wanted = wanted;
            }
        }

        public void AskToSelect(long viewId)
        {
            lock (_asking)
            {
                _wanted = DrawingSheetRequest.SelectView;
                _viewToSelect = viewId;
            }
        }

        public void AskToAssign(IReadOnlyList<string> plotsTicked)
        {
            lock (_asking)
            {
                _wanted = DrawingSheetRequest.AssignScopeBoxes;
                _plotsTicked = plotsTicked ?? new List<string>();
            }
        }

        public string GetName()
        {
            return "RCRC Green Drawing Sheet";
        }

        /// <summary>
        /// Nothing may leave here. An exception out of an IExternalEventHandler does not raise
        /// a dialog, it ends Revit along with whatever the user had not saved. The failures
        /// worth naming are named in <see cref="Run"/>. This is what catches the rest.
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
            DrawingSheetRequest wanted;
            long viewToSelect;
            IReadOnlyList<string> plotsTicked;

            lock (_asking)
            {
                wanted = _wanted;
                viewToSelect = _viewToSelect;
                plotsTicked = _plotsTicked;
                _wanted = DrawingSheetRequest.Nothing;
            }

            if (wanted == DrawingSheetRequest.Nothing) return;

            UIDocument open = application.ActiveUIDocument;
            Document document = open == null ? null : open.Document;

            if (document == null)
            {
                // The document can be closed while the panel is still on screen. That is
                // ordinary, so it is said rather than thrown, and the panel empties itself.
                Read?.Invoke(DrawingSheetSnapshot.Nothing);
                Told?.Invoke("No open document. Open a model and press Refresh.");
                return;
            }

            try
            {
                switch (wanted)
                {
                    case DrawingSheetRequest.Refresh:
                        Refresh(document);
                        break;
                    case DrawingSheetRequest.SelectView:
                        Select(open, document, viewToSelect);
                        break;
                    case DrawingSheetRequest.AssignScopeBoxes:
                        Assign(document, plotsTicked);
                        break;
                    case DrawingSheetRequest.ScanModel:
                        Scan(document);
                        break;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                Told?.Invoke("Revit refused that. " + failed.Message);
            }
            catch (InvalidOperationException failed)
            {
                // The Revit API throws the plain .NET exceptions as well as its own, and a bad
                // argument or a wrong moment arrives as one of these two rather than as
                // anything under Autodesk.Revit.Exceptions.
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
        /// The last thing before Revit would have gone down. The panel may already be gone by
        /// now, so a throw out of the report itself is caught too. There is nowhere further to
        /// pass it and rethrowing would be the crash all of this exists to stop.
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

        private void Refresh(Document document)
        {
            DrawingSheetSnapshot snapshot = DrawingSheetReader.Read(document);
            Read?.Invoke(snapshot);

            var said = new StringBuilder();
            said.Append(snapshot.ViewsRead).Append(" views read at ");
            said.Append(snapshot.ReadAt.ToString("HH:mm:ss")).Append(". ");
            said.Append(snapshot.PlotIds.Count).Append(" plots, ");
            said.Append(snapshot.FromParameter).Append(" found through PRX_Plot_ID, ");
            said.Append(snapshot.FromViewName).Append(" through the name, ");
            said.Append(snapshot.WithNoPlot).Append(" with no plot at all.");

            if (snapshot.SourcesDisagree > 0)
            {
                said.Append(" ").Append(snapshot.SourcesDisagree);
                said.Append(" views are named for one plot and carry PRX_Plot_ID for another. ");
                said.Append("The grid follows the name.");
            }

            Told?.Invoke(said.ToString());
        }

        private void Select(UIDocument open, Document document, long viewId)
        {
            var id = new ElementId(viewId);
            var view = document.GetElement(id) as View;

            if (view == null)
            {
                Told?.Invoke("That view is no longer in the model. Press Refresh.");
                return;
            }

            try
            {
                open.ActiveView = view;
            }
            catch (InvalidOperationException)
            {
                // Revit will not make a view template or a legend active, among others. One
                // click on one grid cell is not worth a crash dialog.
                Told?.Invoke(view.Name + " cannot be opened. Revit will not make that kind of "
                    + "view active.");
                return;
            }

            open.Selection.SetElementIds(new List<ElementId> { id });
            Told?.Invoke("Showing " + view.Name + ".");
        }

        /// <summary>
        /// The same case A to F logic the panel shows the counts for, over the ticked plots
        /// only.
        ///
        /// The model is read again here rather than the panel's snapshot being trusted. The
        /// snapshot is as old as the last Refresh, and a write built on a stale read is how a
        /// model ends up with a scope box on a view somebody else already changed.
        /// </summary>
        private void Assign(Document document, IReadOnlyList<string> plotsTicked)
        {
            if (plotsTicked.Count == 0)
            {
                Told?.Invoke("No plots are ticked, so there was nothing to assign.");
                return;
            }

            ScopeBoxScanner.DocumentScopeBoxes found =
                ScopeBoxScanner.Read(document, NeverCancels.Watcher);

            if (found == null)
            {
                Told?.Invoke("The read did not finish, so nothing was changed.");
                return;
            }

            // Narrowed by the plot in the view's own name, which is the rule ScopeBoxPlan
            // follows and the rule the counts on screen were worked out with. Two rules would
            // mean the number shown and the number written were different numbers.
            IReadOnlyList<ViewScopeBoxState> narrowed =
                ScopeBoxCounts.Narrow(found.Views, plotsTicked);

            ScopeBoxPlan plan = ScopeBoxPlan.Decide(narrowed, found.BoxIdByName.Keys);
            int ready = plan.Count(ScopeBoxCase.ReadyToAssign);

            bool applied = false;
            var refused = new List<long>();

            if (ready > 0 && AssignScopeBoxCommand.Confirmed(plan, ready))
            {
                AssignScopeBoxCommand.Assign(document, plan, found.BoxIdByName, refused);
                applied = true;
            }

            string path = AssignScopeBoxCommand.WriteReport(plan, document.Title, applied, refused);
            Told?.Invoke(applied
                ? (ready - refused.Count) + " views given a scope box over " + plotsTicked.Count
                    + " ticked plots. Report at " + path
                : "Nothing was changed. Report at " + path);
        }

        /// <summary>
        /// Scan Model, reachable from the panel now that it is off the ribbon. It reads the
        /// whole document rather than a range, which is what makes it the check the panel is
        /// measured against.
        /// </summary>
        private void Scan(Document document)
        {
            ModelScan scan;
            if (!ModelScanner.TryRead(document, NeverCancels.Watcher, out scan))
            {
                Told?.Invoke("The scan did not finish, so no file was written.");
                return;
            }

            if (scan.FoundNoViews)
            {
                Told?.Invoke("No views were found in " + document.Title + ", so no file was written.");
                return;
            }

            DateTime writtenAt = DateTime.Now;
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                ScanFileName.For(scan.DocumentTitle, writtenAt));

            File.WriteAllText(path, ScanReport.Write(scan, writtenAt), new UTF8Encoding(false));
            Told?.Invoke("Scan written to " + path);
        }

        /// <summary>
        /// The panel has no Cancel button of its own. The read it runs is the fast one.
        /// </summary>
        private sealed class NeverCancels : IScanWatcher
        {
            public static readonly IScanWatcher Watcher = new NeverCancels();

            public bool Cancelled
            {
                get { return false; }
            }

            public void Report(int done, int total)
            {
            }
        }
    }
}
