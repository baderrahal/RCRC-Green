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
            IEnumerable<ScannedViewFamilyType> viewFamilyTypes,
            IEnumerable<ScannedScopeBox> scopeBoxes,
            IEnumerable<ScannedParameterValue> plotIdValues,
            IEnumerable<ScannedDisagreement> disagreements,
            int elementsScanned,
            double scanSeconds,
            IEnumerable<ViewportRecord> viewports = null)
        {
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            DocumentTitle = documentTitle;
            Sheets = Held(sheets);
            Views = Held(views);
            ViewFamilyTypes = Held(viewFamilyTypes);
            ScopeBoxes = Held(scopeBoxes);
            PlotIdValues = Held(plotIdValues);
            Disagreements = Held(disagreements);
            ElementsScanned = elementsScanned;
            ScanSeconds = scanSeconds;
            Viewports = Held(viewports);
        }

        public string DocumentTitle { get; }

        public IReadOnlyList<ScannedSheet> Sheets { get; }

        /// <summary>
        /// Views and view templates together, the way the collector gives them back. The
        /// report splits them.
        /// </summary>
        public IReadOnlyList<ScannedView> Views { get; }

        /// <summary>
        /// Every view family type in the model, which is what a new view is made with. The
        /// report listed the templates and not these, and the one mismatch that hid there cost
        /// three of the four refusals in the first real run.
        /// </summary>
        public IReadOnlyList<ScannedViewFamilyType> ViewFamilyTypes { get; }

        public IReadOnlyList<ScannedScopeBox> ScopeBoxes { get; }

        public IReadOnlyList<ScannedParameterValue> PlotIdValues { get; }

        /// <summary>
        /// Every viewport already on a sheet, with its scale, centre and size next to the size
        /// of the sheet. It is the yardstick a placement the tool makes is held against,
        /// because without a real one to compare with there is no way to say whether a
        /// placement is right.
        /// </summary>
        public IReadOnlyList<ViewportRecord> Viewports { get; }

        /// <summary>
        /// Views named for one plot while PRX_Plot_ID holds another. Named rather than counted,
        /// so somebody can go and fix them.
        /// </summary>
        public IReadOnlyList<ScannedDisagreement> Disagreements { get; }

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
