using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads every view that could be a sibling into plain values, once per run, and hands the
    /// choosing to Core.
    ///
    /// A new view takes its family type, its level, its view template and two of its three
    /// crop settings. All of them come off ONE view. This is the only place that view is chosen,
    /// and it is chosen by <see cref="SiblingChoice"/> so a test can hold the chosen family type
    /// against the chosen template and prove they belong together.
    ///
    /// Reading the whole list once also stops the writer scanning every view in the document
    /// again for each item it makes.
    /// </summary>
    internal sealed class SiblingReader
    {
        private readonly Dictionary<string, View> _byName =
            new Dictionary<string, View>(StringComparer.Ordinal);

        private readonly List<SiblingView> _facts = new List<SiblingView>();

        private SiblingReader()
        {
        }

        public static SiblingReader Of(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var reader = new SiblingReader();

            foreach (View view in new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => !view.IsTemplate && !(view is ViewSheet)))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(view.Name, out parsed)) continue;

                // Revit keeps view names unique, so the name is enough to get back from the
                // plain values Core chose between to the element itself.
                if (reader._byName.ContainsKey(view.Name)) continue;

                reader._byName.Add(view.Name, view);
                reader._facts.Add(FactsAbout(document, view, parsed.Type));
            }

            return reader;
        }

        /// <summary>
        /// The chosen view and the facts that were chosen with it, or null when the model holds
        /// no view of that type. Both halves come back together so the caller cannot pair the
        /// facts of one view with the element of another.
        /// </summary>
        public Sibling For(ViewType type, SiblingKind wanted)
        {
            SiblingView chosen = SiblingChoice.For(type, wanted, _facts);
            if (chosen == null) return null;

            View view;
            return _byName.TryGetValue(chosen.ViewName, out view) ? new Sibling(view, chosen) : null;
        }

        private static SiblingView FactsAbout(Document document, View view, ViewType type)
        {
            var plan = view as ViewPlan;
            var section = view as ViewSection;

            SiblingKind kind = plan != null
                ? SiblingKind.Plan
                : section != null ? SiblingKind.Section : SiblingKind.Other;

            return new SiblingView(
                view.Name,
                type,
                kind,
                NameOf(document, view.GetTypeId()),
                NameOf(document, view.ViewTemplateId),
                plan == null || plan.GenLevel == null ? string.Empty : plan.GenLevel.Name,
                CropOf(view));
        }

        /// <summary>
        /// Crop View, Crop Region Visible and Annotation Crop.
        ///
        /// The first two are properties. Annotation Crop is not, so it comes off the parameter,
        /// and a view that does not carry it reads as off rather than throwing. Every view the
        /// tool created had it off while DM-16 and DM-14 both have it on, which is what lets a
        /// neighbouring plot's section markers draw through a new view.
        /// </summary>
        private static ViewCrop CropOf(View view)
        {
            Parameter annotation = view.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE);

            return new ViewCrop(
                view.CropBoxActive,
                view.CropBoxVisible,
                annotation != null
                    && annotation.StorageType == StorageType.Integer
                    && annotation.AsInteger() == 1);
        }

        private static string NameOf(Document document, ElementId id)
        {
            if (id == null || id == ElementId.InvalidElementId) return string.Empty;

            Element found = document.GetElement(id);
            return found == null ? string.Empty : found.Name;
        }
    }

    /// <summary>
    /// One view, and the plain facts Core read off it. They travel together on purpose.
    /// </summary>
    internal sealed class Sibling
    {
        public Sibling(View view, SiblingView facts)
        {
            View = view;
            Facts = facts;
        }

        public View View { get; }

        public SiblingView Facts { get; }
    }
}
