using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads a document and hands back plain numbers and strings. Nothing here opens a
    /// transaction, because nothing here writes.
    /// </summary>
    internal static class ModelScanner
    {
        /// <summary>
        /// The plot on a view or a sheet. The Sheet List filters on this one.
        /// </summary>
        public const string PlotIdParameterName = "PRX_Plot_ID";

        /// <summary>
        /// The plot on a model element. Every quantity schedule filters on this one, and the
        /// name really does carry spaces rather than underscores. A schedule built against
        /// PRX_Plot_ID by mistake comes back empty, which is why both are named here rather
        /// than one being assumed to cover the other.
        /// </summary>
        public const string RefPlotIdParameterName = "PRX_Ref Plot ID";

        /// <summary>
        /// False when the user stopped it. The scan is then incomplete, so nothing is handed
        /// back rather than a partial answer that reads like a whole one.
        /// </summary>
        public static bool TryRead(Document document, IScanWatcher watcher, out ModelScan scan)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (watcher == null) throw new ArgumentNullException("watcher");

            scan = null;

            List<ViewSheet> sheets = new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(sheet => !sheet.IsTemplate)
                .ToList();

            Dictionary<ElementId, string> sheetNumberByView = SheetNumberByView(document, sheets);

            var placedPerSheet = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string number in sheetNumberByView.Values)
            {
                int already;
                placedPerSheet.TryGetValue(number, out already);
                placedPerSheet[number] = already + 1;
            }

            var scannedSheets = new List<ScannedSheet>(sheets.Count);
            foreach (ViewSheet sheet in sheets)
            {
                int onThisSheet;
                placedPerSheet.TryGetValue(sheet.SheetNumber, out onThisSheet);
                scannedSheets.Add(new ScannedSheet(
                    sheet.SheetNumber,
                    sheet.Name,
                    onThisSheet,
                    ValueOf(sheet.LookupParameter(PlotIdParameterName))));
            }

            var scannedViews = new List<ScannedView>();
            var disagreements = new List<ScannedDisagreement>();

            foreach (View view in new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>())
            {
                // A ViewSheet is a View. It has its own section in the report, so leaving it
                // here as well would count every sheet twice and file it under views that are
                // not on a sheet, which reads as a mistake in the model.
                if (view is ViewSheet) continue;

                string sheetNumber;
                if (view.IsTemplate || !sheetNumberByView.TryGetValue(view.Id, out sheetNumber))
                {
                    sheetNumber = string.Empty;
                }

                // The family type is on every view because the scan is what shows where one view
                // type is built more than one way. A run made three (010) views with three
                // different family types and nothing in this file would have said so.
                scannedViews.Add(new ScannedView(
                    view.Name, view.ViewType.ToString(), view.IsTemplate, sheetNumber,
                    NameOfElement(document, view.GetTypeId())));

                if (view.IsTemplate) continue;

                // The panel has counted these since the grid stopped filing a view under the
                // wrong plot. A count says there is a problem and nothing about where, so the
                // scan names them. Same reading the panel does, so the two cannot disagree.
                string onTheView = ValueOf(view.LookupParameter(PlotIdParameterName));
                ViewOnAPlot read = ViewReading.Read(onTheView, view.Name, view.Id.Value);

                if (read.SourcesDisagree)
                {
                    disagreements.Add(new ScannedDisagreement(
                        view.Name, read.Fills.Where.PlotId, read.PlotId));
                }
            }

            PlotIdScan plotIds = ReadPlotIds(document, watcher);
            if (plotIds == null) return false;

            scan = new ModelScan(
                document.Title,
                scannedSheets,
                scannedViews,
                ReadViewFamilyTypes(document),
                ReadScopeBoxes(document),
                plotIds.Values,
                disagreements,
                plotIds.ElementsRead,
                plotIds.Seconds,
                ReadViewports(document, sheets));
            return true;
        }

        /// <summary>
        /// Every placement on every existing sheet, measured the same way the run report
        /// measures its own, so a sheet the tool makes can be held against one the team made
        /// instead of against a guess. Nothing that cannot be read is skipped quietly: it is
        /// recorded with the reason in its name.
        /// </summary>
        private static List<ViewportRecord> ReadViewports(
            Document document, IEnumerable<ViewSheet> sheets)
        {
            var sheetById = new Dictionary<ElementId, ViewSheet>();
            foreach (ViewSheet sheet in sheets) sheetById[sheet.Id] = sheet;

            // The size comes off the title block placed on each sheet, never off the type,
            // because Sheet Width and Sheet Height only exist once one is placed. The first
            // block found per sheet answers, the way the run measures its own new sheets.
            var footprints = new Dictionary<ElementId, SheetFootprint>();
            foreach (FamilyInstance block in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .OfType<FamilyInstance>())
            {
                if (footprints.ContainsKey(block.OwnerViewId)) continue;

                footprints[block.OwnerViewId] = new SheetFootprint
                {
                    WidthFeet = Number(block.get_Parameter(BuiltInParameter.SHEET_WIDTH)),
                    HeightFeet = Number(block.get_Parameter(BuiltInParameter.SHEET_HEIGHT))
                };
            }

            var placements = new List<ViewportRecord>();

            foreach (Viewport viewport in new FilteredElementCollector(document)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>())
            {
                ViewSheet sheet;
                if (!sheetById.TryGetValue(viewport.SheetId, out sheet)) continue;

                var view = document.GetElement(viewport.ViewId) as View;
                XYZ centre = viewport.GetBoxCenter();
                Outline box = viewport.GetBoxOutline();
                SheetFootprint footprint = FootprintOf(footprints, sheet.Id);

                placements.Add(new ViewportRecord(
                    sheet.SheetNumber,
                    sheet.Name,
                    view == null ? "(view not read)" : view.Name,
                    view == null ? 0 : view.Scale,
                    centre == null ? 0.0 : centre.X,
                    centre == null ? 0.0 : centre.Y,
                    box == null ? 0.0 : box.MaximumPoint.X - box.MinimumPoint.X,
                    box == null ? 0.0 : box.MaximumPoint.Y - box.MinimumPoint.Y,
                    footprint.WidthFeet,
                    footprint.HeightFeet,
                    false));
            }

            foreach (ScheduleSheetInstance placed in new FilteredElementCollector(document)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>())
            {
                if (placed.IsTitleblockRevisionSchedule) continue;

                ViewSheet sheet;
                if (!sheetById.TryGetValue(placed.OwnerViewId, out sheet)) continue;

                Element schedule = document.GetElement(placed.ScheduleId);
                BoundingBoxXYZ across = placed.get_BoundingBox(sheet);
                SheetFootprint footprint = FootprintOf(footprints, sheet.Id);

                placements.Add(new ViewportRecord(
                    sheet.SheetNumber,
                    sheet.Name,
                    schedule == null ? "(schedule not read)" : schedule.Name,
                    0,
                    across == null ? 0.0 : (across.Min.X + across.Max.X) / 2.0,
                    across == null ? 0.0 : (across.Min.Y + across.Max.Y) / 2.0,
                    across == null ? 0.0 : across.Max.X - across.Min.X,
                    across == null ? 0.0 : across.Max.Y - across.Min.Y,
                    footprint.WidthFeet,
                    footprint.HeightFeet,
                    true));
            }

            return placements;
        }

        private static SheetFootprint FootprintOf(
            Dictionary<ElementId, SheetFootprint> footprints, ElementId sheetId)
        {
            SheetFootprint found;
            return footprints.TryGetValue(sheetId, out found)
                ? found
                : new SheetFootprint();
        }

        private static double Number(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return 0.0;
            if (parameter.StorageType != StorageType.Double) return 0.0;

            return parameter.AsDouble();
        }

        /// <summary>
        /// A sheet's width and height in feet, zero when no placed title block answered, which
        /// prints as 0 mm rather than as a plausible size.
        /// </summary>
        private sealed class SheetFootprint
        {
            public double WidthFeet;

            public double HeightFeet;
        }

        /// <summary>
        /// Viewports carry drawings, schedules on a sheet are a different element, and a view
        /// counted through only one of the two comes back missing whole sheets of schedules.
        /// </summary>
        private static Dictionary<ElementId, string> SheetNumberByView(
            Document document, IEnumerable<ViewSheet> sheets)
        {
            var numberBySheetId = new Dictionary<ElementId, string>();
            foreach (ViewSheet sheet in sheets)
            {
                numberBySheetId[sheet.Id] = sheet.SheetNumber;
            }

            var found = new Dictionary<ElementId, string>();

            foreach (Viewport viewport in new FilteredElementCollector(document)
                .OfClass(typeof(Viewport))
                .Cast<Viewport>())
            {
                string number;
                if (numberBySheetId.TryGetValue(viewport.SheetId, out number))
                {
                    found[viewport.ViewId] = number;
                }
            }

            foreach (ScheduleSheetInstance placed in new FilteredElementCollector(document)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>())
            {
                // A revision schedule on a titleblock reports a schedule id that points at
                // nothing, so it is skipped rather than filed under a view that does not exist.
                if (placed.IsTitleblockRevisionSchedule) continue;

                string number;
                if (numberBySheetId.TryGetValue(placed.OwnerViewId, out number))
                {
                    found[placed.ScheduleId] = number;
                }
            }

            return found;
        }

        /// <summary>
        /// What a new view is actually made with, which the report used to leave out.
        ///
        /// The tool matched these by name and the belief that the type for a
        /// (010) Location Key Plan is called (010) Location Key Plan. It is called
        /// (010) Key Location Plan. Nothing in the scan would have shown that, so it took a
        /// Properties panel to find, and it cost three of the four refusals in the first run.
        /// </summary>
        private static List<ScannedViewFamilyType> ReadViewFamilyTypes(Document document)
        {
            var types = new List<ScannedViewFamilyType>();

            foreach (ViewFamilyType type in new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>())
            {
                types.Add(new ScannedViewFamilyType(type.Name, type.ViewFamily.ToString()));
            }

            return types;
        }

        private static List<ScannedScopeBox> ReadScopeBoxes(Document document)
        {
            var boxes = new List<ScannedScopeBox>();

            foreach (Element box in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType())
            {
                BoundingBoxXYZ extent = box.get_BoundingBox(null);
                if (extent == null)
                {
                    boxes.Add(ScannedScopeBox.WithoutBounds(box.Name));
                    continue;
                }

                boxes.Add(new ScannedScopeBox(
                    box.Name,
                    true,
                    extent.Min.X, extent.Min.Y, extent.Min.Z,
                    extent.Max.X, extent.Max.Y, extent.Max.Z));
            }

            return boxes;
        }

        private sealed class PlotIdScan
        {
            public List<ScannedParameterValue> Values;
            public int ElementsRead;
            public double Seconds;
        }

        /// <summary>
        /// Looked up by name rather than by a known identifier, so it works whether the team
        /// set PRX_Plot_ID up as shared or project, and on an instance or a type.
        /// </summary>
        private static PlotIdScan ReadPlotIds(Document document, IScanWatcher watcher)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            var byType = new Dictionary<ElementId, string>();
            int read = 0;

            Stopwatch clock = Stopwatch.StartNew();

            // The identifiers are taken first so the total is known. Without a total the bar
            // has nothing to fill and the user cannot tell a slow read from a stuck one.
            ICollection<ElementId> everyElement =
                new FilteredElementCollector(document).WhereElementIsNotElementType().ToElementIds();
            int total = everyElement.Count;

            foreach (ElementId id in everyElement)
            {
                if (watcher.Cancelled) return null;

                Element element = document.GetElement(id);
                if (element == null) continue;

                read++;
                watcher.Report(read, total);

                string value = ValueOf(element.LookupParameter(PlotIdParameterName));

                if (value == null)
                {
                    ElementId typeId = element.GetTypeId();
                    if (typeId != ElementId.InvalidElementId)
                    {
                        // Every instance of one type asks the same question, so the answer is
                        // kept. On a model with a hundred thousand elements this is the
                        // difference between seconds and minutes.
                        if (!byType.TryGetValue(typeId, out value))
                        {
                            value = ValueOf(document.GetElement(typeId)?.LookupParameter(PlotIdParameterName));
                            byType[typeId] = value;
                        }
                    }
                }

                if (value == null) continue;

                int already;
                counts.TryGetValue(value, out already);
                counts[value] = already + 1;
            }

            clock.Stop();

            return new PlotIdScan
            {
                Values = counts
                    .Select(pair => new ScannedParameterValue(pair.Key, pair.Value))
                    .ToList(),
                ElementsRead = read,
                Seconds = clock.Elapsed.TotalSeconds
            };
        }

        /// <summary>
        /// Null when the element does not carry the parameter at all, which is the ordinary
        /// case and has to stay apart from a parameter that is there and empty.
        /// </summary>
        private static string ValueOf(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return null;

            if (parameter.StorageType == StorageType.String)
            {
                return parameter.AsString() ?? string.Empty;
            }

            return parameter.AsValueString() ?? string.Empty;
        }

        private static string NameOfElement(Document document, ElementId id)
        {
            if (id == null || id == ElementId.InvalidElementId) return string.Empty;

            Element found = document.GetElement(id);
            return found == null ? string.Empty : found.Name;
        }
    }
}
