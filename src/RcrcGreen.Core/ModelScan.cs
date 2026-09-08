using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Everything one run of Scan Model read out of a document, as plain numbers and strings.
    /// The Revit side fills this in and hands it here. Nothing in it knows what a Document is.
    /// </summary>
    public sealed class ModelScan
    {
        public ModelScan(
            string documentTitle,
            IEnumerable<ScannedSheet> sheets,
            IEnumerable<ScannedView> views,
            IEnumerable<ScannedScopeBox> scopeBoxes,
            IEnumerable<ScannedParameterValue> plotIdValues,
            int elementsScanned,
            double scanSeconds)
        {
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            DocumentTitle = documentTitle;
            Sheets = Held(sheets);
            Views = Held(views);
            ScopeBoxes = Held(scopeBoxes);
            PlotIdValues = Held(plotIdValues);
            ElementsScanned = elementsScanned;
            ScanSeconds = scanSeconds;
        }

        public string DocumentTitle { get; }

        public IReadOnlyList<ScannedSheet> Sheets { get; }

        /// <summary>
        /// Views and view templates together, the way the collector gives them back. The
        /// report splits them.
        /// </summary>
        public IReadOnlyList<ScannedView> Views { get; }

        public IReadOnlyList<ScannedScopeBox> ScopeBoxes { get; }

        public IReadOnlyList<ScannedParameterValue> PlotIdValues { get; }

        /// <summary>
        /// How many elements were read looking for PRX_Plot_ID, and how long that took. Both
        /// go in the report because this is the part that gets slow on a real model.
        /// </summary>
        public int ElementsScanned { get; }

        public double ScanSeconds { get; }

        public IEnumerable<ScannedView> Templates
        {
            get { return Views.Where(view => view.IsTemplate); }
        }

        public IEnumerable<ScannedView> ViewsOnSheets
        {
            get { return Views.Where(view => !view.IsTemplate && view.OnSheet); }
        }

        public IEnumerable<ScannedView> ViewsNotOnSheets
        {
            get { return Views.Where(view => !view.IsTemplate && !view.OnSheet); }
        }

        /// <summary>
        /// A document with nothing in it gets told so rather than handed an empty file.
        /// </summary>
        public bool FoundNoViews
        {
            get { return Views.Count == 0; }
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
