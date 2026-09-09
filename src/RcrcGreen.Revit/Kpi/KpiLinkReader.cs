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

        /// <summary>
        /// What a region carrying a plot and no intervention area prints in the raw and
        /// printed columns. Core holds the word so the reader and the report cannot spell the
        /// same absence two ways.
        /// </summary>
        private static string NoAreaValue
        {
            get { return LinkContents.NoAreaValue; }
        }

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
                contents.Add(Contents(instance.Name, linked, skipped));
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

        private static LinkContents Contents(string instanceName, Document linked, List<string> skipped)
        {
            List<FilledRegion> regions = new FilteredElementCollector(linked)
                .OfClass(typeof(FilledRegion))
                .Cast<FilledRegion>()
                .ToList();

            var perType = new Dictionary<string, int>(StringComparer.Ordinal);
            var perView = new Dictionary<string, int>(StringComparer.Ordinal);
            var first = new List<FilledRegionRead>();
            var areas = new List<MeasuredValue>();
            var withAPlot = new List<MeasuredValue>();
            var carryingAPlot = new Dictionary<string, int>(StringComparer.Ordinal);
            int twiceNamed = 0;
            int plotTwiceNamed = 0;
            int plotNotCarried = 0;
            int plotBlank = 0;

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

                // The plot the region carries, read the same way, because one plot's regions
                // read together is what settles which type is its intervention area. Not
                // carried and carried blank are counted apart, because both read as (empty)
                // in a row and only one of them is a model somebody has to go and fix.
                int plotNamed;
                Parameter plot = ParameterReading.Named(region, KpiNames.RefPlotId, out plotNamed);
                string plotId = ParameterReading.HoldsAValue(plot) ? ParameterReading.Printed(plot) : string.Empty;
                if (plotNamed > 1) plotTwiceNamed++;
                if (plotNamed == 0) plotNotCarried++;
                else if (plotId.Length == 0) plotBlank++;

                // Read the way the tally counts, so the count of regions with a value and
                // the list of values cannot be two different numbers.
                int howMany;
                Parameter area = ParameterReading.Named(region, KpiNames.InterventionArea, out howMany);
                if (howMany > 1) twiceNamed++;
                bool areaHoldsAValue = ParameterReading.HoldsAValue(area);
                if (areaHoldsAValue)
                {
                    areas.Add(new MeasuredValue(
                        typeName, ParameterReading.Spec(area), ParameterReading.Raw(area),
                        ParameterReading.Printed(area), plotId));
                }

                // Every region carrying a plot, whether or not its area holds a value. The
                // areas list above is a list of area values, so grouping it by plot loses the
                // regions that carry a plot and no area and calls what is left the plot's
                // regions. This list is what the per plot section groups.
                if (plotId.Length > 0)
                {
                    ParameterReading.Bump(carryingAPlot, typeName.Length == 0 ? "(no type name)" : typeName);
                    withAPlot.Add(new MeasuredValue(
                        typeName,
                        areaHoldsAValue ? ParameterReading.Spec(area) : string.Empty,
                        areaHoldsAValue ? ParameterReading.Raw(area) : NoAreaValue,
                        areaHoldsAValue ? ParameterReading.Printed(area) : NoAreaValue,
                        plotId));
                }
            }

            if (twiceNamed > 0)
            {
                skipped.Add(twiceNamed + " filled regions in " + instanceName + " carry more than one parameter named "
                    + KpiNames.InterventionArea + ". The first holding a value was read on each.");
            }

            if (plotTwiceNamed > 0)
            {
                skipped.Add(plotTwiceNamed + " filled regions in " + instanceName + " carry more than one parameter named "
                    + KpiNames.RefPlotId + ". The first holding a value was read on each.");
            }

            return new LinkContents(
                instanceName,
                linked.Title,
                regions.Count,
                ParameterReading.Counted(perType),
                ParameterReading.Counted(perView),
                first,
                ParameterReading.Tally(regions.Cast<Element>()),
                areas,
                ParameterReading.Counted(carryingAPlot),
                withAPlot,
                withAPlot.Count,
                plotBlank,
                plotNotCarried);
        }
    }
}
