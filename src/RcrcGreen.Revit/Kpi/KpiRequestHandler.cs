using System;
using System.Collections.Generic;
using System.IO;
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
        Scan
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
        /// Called back on the Revit thread. The pane marshals to its own thread itself.
        /// </summary>
        public Action<string> Named { get; set; }

        public Action<KpiScan, DateTime> Scanned { get; set; }

        public Action<string> Told { get; set; }

        public void Ask(KpiRequest wanted)
        {
            lock (_asking)
            {
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

            try
            {
                switch (wanted)
                {
                    case KpiRequest.WhichModel:
                        Named?.Invoke(document.Title);
                        break;
                    case KpiRequest.Scan:
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
