using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Section 4. Every link type and instance, and the filled regions inside each loaded
    /// link with what PRX_Intervention Area looks like on them.
    ///
    /// The workbook says REVIT 00 LINK / ID FILLED REGION and nothing more. So the link is
    /// found by the 00 in its name, and ID is looked for in both places it could be, the
    /// region's type name and the view it is drawn in. Neither is assumed.
    /// </summary>
    internal static class KpiLinkReader
    {
        /// <summary>
        /// How many filled regions get every parameter printed. Enough to see what a region
        /// carries, few enough that a link with thousands does not fill the file.
        /// </summary>
        public const int RegionsReadInFull = 10;

        public static LinkFacts Read(Document document, List<string> skipped)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (skipped == null) throw new ArgumentNullException("skipped");

            var types = new List<ScannedLinkType>();
            foreach (RevitLinkType type in new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>())
            {
                types.Add(new ScannedLinkType(
                    type.Name,
                    Status(type),
                    RevitLinkType.IsLoaded(document, type.Id),
                    type.IsNestedLink));
            }

            var instances = new List<ScannedLinkInstance>();
            var documentsSeen = new Dictionary<string, string>(StringComparer.Ordinal);
            var contents = new List<LinkContents>();

            foreach (RevitLinkInstance instance in new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>())
            {
                Document linked = instance.GetLinkDocument();
                string typeName = ParameterReading.NameOf(document, instance.GetTypeId());

                instances.Add(new ScannedLinkInstance(instance.Name, typeName, linked != null));

                if (linked == null) continue;

                // Two instances of one link share one document. Reading it twice would print
                // every region twice and double the counts, so the second says which instance
                // already covered it.
                string key = (linked.PathName ?? string.Empty) + "|" + linked.Title;
                string coveredBy;
                if (documentsSeen.TryGetValue(key, out coveredBy))
                {
                    skipped.Add("Link instance " + instance.Name + " shares its document with " + coveredBy
                        + ", so its filled regions are counted once, under that one.");
                    continue;
                }

                documentsSeen[key] = instance.Name;
                contents.Add(Contents(instance.Name, linked));
            }

            return new LinkFacts(types, instances, contents);
        }

        private static string Status(RevitLinkType type)
        {
            try
            {
                return type.GetLinkedFileStatus().ToString();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                return "status refused: " + failed.Message;
            }
        }

        private static LinkContents Contents(string instanceName, Document linked)
        {
            List<FilledRegion> regions = new FilteredElementCollector(linked)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();

            var perType = new Dictionary<string, int>(StringComparer.Ordinal);
            var perView = new Dictionary<string, int>(StringComparer.Ordinal);
            var first = new List<FilledRegionRead>();
            var areas = new List<MeasuredValue>();

            foreach (FilledRegion region in regions)
            {
                string typeName = ParameterReading.NameOf(linked, region.GetTypeId());
                string viewName = ParameterReading.NameOf(linked, region.OwnerViewId);

                ParameterReading.Bump(perType, typeName.Length == 0 ? "(no type name)" : typeName);
                ParameterReading.Bump(perView, viewName.Length == 0 ? "(no view name)" : viewName);

                if (first.Count < RegionsReadInFull)
                {
                    first.Add(new FilledRegionRead(typeName, viewName, ParameterReading.ReadAll(region)));
                }

                Parameter area = region.LookupParameter(KpiNames.InterventionArea);
                if (ParameterReading.HoldsAValue(area))
                {
                    areas.Add(new MeasuredValue(typeName, ParameterReading.Raw(area), ParameterReading.Printed(area)));
                }
            }

            return new LinkContents(
                instanceName,
                linked.Title,
                regions.Count,
                ParameterReading.Counted(perType),
                ParameterReading.Counted(perView),
                first,
                ParameterReading.Tally(regions.Cast<Element>()),
                areas);
        }
    }
}
