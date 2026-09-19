using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RcrcGreen.Core;
using RcrcGreen.Core.ViewFilters;

namespace RcrcGreen.Revit.ViewFilters
{
    internal enum ViewFiltersRequest
    {
        Nothing,
        Scan,
        Apply
    }

    /// <summary>
    /// The only route from the View Filters pane to the Revit API. Its own handler and its
    /// own external event, so nothing this pane asks for goes through the Drawing Sheet's or
    /// the KPI's and a failure in one pane can never cost another.
    ///
    /// Two requests. Scan reads the views, the filters and the exemplars and opens no
    /// transaction, because it writes nothing. Apply is the ported run, one transaction and
    /// one undo, and it writes its report through ReportFile the way every run here does.
    /// </summary>
    internal sealed class ViewFiltersRequestHandler : IExternalEventHandler
    {
        private readonly object _asking = new object();

        private ViewFiltersRequest _wanted = ViewFiltersRequest.Nothing;

        // The model the pane's grid was read from, empty until a scan has run. Apply refuses
        // when this does not name the document it would write to, because a grid read off
        // one model says nothing about another.
        private string _scannedTitle = string.Empty;

        /// <summary>What the next Scan or Apply acts on. Set by the pane immediately before
        /// it asks, and read once on the Revit thread.</summary>
        public Inputs Asked { get; set; }

        public Action<ViewFilterScanResult> Scanned { get; set; }

        /// <summary>
        /// What one press of Apply really did, decided in Core so the RESULTS step is
        /// testable, beside the report sentence the pane has always shown.
        /// </summary>
        public Action<ViewFilterResult, string> Applied { get; set; }

        /// <summary>The run's end line, whatever ended it. It closes the progress window.</summary>
        public Action<string> Told { get; set; }

        public Action<string> Progressed { get; set; }

        /// <summary>One line of the run's own log, raised as it happens.</summary>
        public Action<string> Logged { get; set; }

        public void Ask(ViewFiltersRequest wanted)
        {
            if (wanted == ViewFiltersRequest.Nothing) return;

            lock (_asking)
            {
                _wanted = wanted;
            }
        }

        public string GetName()
        {
            return "RCRC Green View Filters";
        }

        /// <summary>
        /// Nothing may leave here. An exception out of an IExternalEventHandler does not
        /// raise a dialog, it ends Revit along with whatever the user had not saved.
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
            ViewFiltersRequest wanted;
            lock (_asking)
            {
                wanted = _wanted;
                _wanted = ViewFiltersRequest.Nothing;
            }

            if (wanted == ViewFiltersRequest.Nothing) return;

            UIDocument open = application.ActiveUIDocument;
            Document document = open == null ? null : open.Document;

            if (document == null)
            {
                Told?.Invoke(ViewFilterWords.NoModel);
                return;
            }

            try
            {
                switch (wanted)
                {
                    case ViewFiltersRequest.Scan:
                        Scan(document);
                        break;
                    case ViewFiltersRequest.Apply:
                        Apply(document);
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
                Told?.Invoke("The report could not be written. " + denied.Message);
            }
            catch (IOException failed)
            {
                Told?.Invoke("The report could not be written. " + failed.Message);
            }
        }

        private void Scan(Document document)
        {
            Progressed?.Invoke(ViewFilterWords.Scanning);

            ViewFiltersRunner runner = new ViewFiltersRunner(document, null, null, null);
            ViewFilterScanResult result = runner.Scan(Asked ?? new Inputs());

            _scannedTitle = document.Title ?? string.Empty;

            Scanned?.Invoke(result);
            Told?.Invoke(ViewFilterWords.ScannedLine(result));
        }

        private void Apply(Document document)
        {
            // The pane greys Apply on what it owns, its own boxes against its own scan. The
            // model is not the pane's to hold a copy of, so whether it is still the scanned
            // one is decided here, off the live document at the moment Apply runs.
            if (_scannedTitle.Length == 0 || _scannedTitle != (document.Title ?? string.Empty))
            {
                Told?.Invoke(ViewFilterWords.ModelMoved);
                return;
            }

            List<string> loggedLines = new List<string>();
            ViewFiltersRunner runner = new ViewFiltersRunner(
                document,
                line =>
                {
                    loggedLines.Add(line);
                    Logged?.Invoke(line);
                },
                (line, done, of) => Progressed?.Invoke(line),
                null);

            Output output = runner.Run(Asked ?? new Inputs());

            // The run may have created filters, so the grid the pane holds is older than the
            // model this run just changed. A fresh read costs one collection pass and keeps
            // the panel honest, the rule the Drawing Sheet's own run learned the hard way.
            // It runs before the report write, so a report that cannot be written still
            // leaves the grid telling the truth about the model the run changed.
            ViewFilterScanResult reread = runner.Scan(Asked ?? new Inputs());
            Scanned?.Invoke(reread);

            Progressed?.Invoke(ViewFilterWords.WritingTheReport);
            DateTime writtenAt = DateTime.Now;
            IReadOnlyList<string> written = ReportFile.Write(
                ViewFiltersFileName.For(document.Title, writtenAt),
                ViewFiltersReport.Write(output, loggedLines, document.Title, writtenAt));

            // The result carries the report's own path, not the sentence about it, so the
            // button that opens it opens the file. The sentence still rides beside it for
            // the line the pane has always shown.
            string reportPath = written.Count == 0 ? string.Empty : written[0];
            Applied?.Invoke(ViewFilterResult.Of(output, reportPath), ReportPlaces.Written(written));
            Told?.Invoke(ViewFilterWords.AppliedLine(output, ReportPlaces.Written(written)));
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
