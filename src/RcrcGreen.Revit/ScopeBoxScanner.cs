using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads the views and the scope boxes out of a document as plain values, so Core can
    /// decide what happens to each one without knowing what a Document is.
    /// </summary>
    internal static class ScopeBoxScanner
    {
        /// <summary>
        /// The parameter that holds a view's scope box.
        /// </summary>
        public const BuiltInParameter ScopeBoxParameter = BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP;

        public sealed class DocumentScopeBoxes
        {
            public List<ViewScopeBoxState> Views;
            public Dictionary<string, ElementId> BoxIdByName;
        }

        /// <summary>
        /// Null when the user stopped it part way, because a plan built from half the views
        /// would show counts that are wrong and offer to write from them.
        /// </summary>
        public static DocumentScopeBoxes Read(Document document, IScanWatcher watcher)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (watcher == null) throw new ArgumentNullException("watcher");

            var boxIdByName = new Dictionary<string, ElementId>(StringComparer.Ordinal);
            foreach (Element box in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType())
            {
                // Two scope boxes cannot share a name in Revit, so the first one wins and the
                // guard is only here to keep a duplicate from throwing.
                if (!boxIdByName.ContainsKey(box.Name))
                {
                    boxIdByName.Add(box.Name, box.Id);
                }
            }

            List<View> views = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                // A sheet is a View and so is a template. A template's scope box would push
                // onto every view using it, which is the opposite of assigning one view.
                .Where(view => !(view is ViewSheet) && !view.IsTemplate)
                .ToList();

            var states = new List<ViewScopeBoxState>(views.Count);
            int read = 0;

            foreach (View view in views)
            {
                if (watcher.Cancelled) return null;

                read++;
                watcher.Report(read, views.Count);

                Parameter holder = view.get_Parameter(ScopeBoxParameter);
                bool canHold = holder != null && !holder.IsReadOnly;

                string current = string.Empty;
                if (holder != null && holder.HasValue)
                {
                    Element box = document.GetElement(holder.AsElementId());
                    if (box != null) current = box.Name;
                }

                states.Add(new ViewScopeBoxState(view.Id.Value, view.Name, canHold, current));
            }

            return new DocumentScopeBoxes { Views = states, BoxIdByName = boxIdByName };
        }
    }
}
