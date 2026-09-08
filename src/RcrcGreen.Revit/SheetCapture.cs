using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

// Autodesk.Revit.DB carries a ViewType enum of its own. Here the word means the code and the
// view name together, which is the Core one.
using ViewType = RcrcGreen.Core.ViewType;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads a sheet that already exists into a <see cref="SheetDefinition"/>.
    ///
    /// The read half of building a sheet, and the same shape as <see cref="ScheduleCapture"/>
    /// for the same reason. Nothing here writes and nothing here opens a transaction. What
    /// comes back holds only strings and numbers, so Core can reason about it and so the layout
    /// could one day be saved to a file and used on a model that has no sheet to copy.
    /// </summary>
    internal static class SheetCapture
    {
        public static SheetDefinition Of(Document document, ElementId sheetId)
        {
            if (document == null) throw new ArgumentNullException("document");

            var sheet = document.GetElement(sheetId) as ViewSheet;
            if (sheet == null) return null;

            FamilyInstance block = new FilteredElementCollector(document, sheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .FirstOrDefault();

            string family = string.Empty;
            string type = string.Empty;
            double width = 0.0;
            double height = 0.0;

            if (block != null && block.Symbol != null)
            {
                family = block.Symbol.FamilyName ?? string.Empty;
                type = block.Symbol.Name ?? string.Empty;
                width = Number(block.get_Parameter(BuiltInParameter.SHEET_WIDTH));
                height = Number(block.get_Parameter(BuiltInParameter.SHEET_HEIGHT));
            }

            return new SheetDefinition(family, type, width, height, Placements(document, sheet));
        }

        /// <summary>
        /// Drawings and schedules both, because they are different elements on a sheet and a
        /// capture that read only the first would drop every schedule off a copied layout
        /// without saying so.
        /// </summary>
        private static List<SheetViewPlacement> Placements(Document document, ViewSheet sheet)
        {
            var found = new List<SheetViewPlacement>();

            foreach (ElementId viewportId in sheet.GetAllViewports())
            {
                var viewport = document.GetElement(viewportId) as Viewport;
                if (viewport == null) continue;

                ViewType type = TypeOfViewNamed(document, viewport.ViewId);
                if (type == null) continue;

                XYZ centre = viewport.GetBoxCenter();
                found.Add(new SheetViewPlacement(type, centre.X, centre.Y, false));
            }

            foreach (ScheduleSheetInstance placed in new FilteredElementCollector(document)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>()
                .Where(one => one.OwnerViewId == sheet.Id))
            {
                // A revision schedule on the title block reports a schedule that points at
                // nothing, and it comes with the title block anyway.
                if (placed.IsTitleblockRevisionSchedule) continue;

                ViewType type = TypeOfViewNamed(document, placed.ScheduleId);
                if (type == null) continue;

                found.Add(new SheetViewPlacement(type, placed.Point.X, placed.Point.Y, true));
            }

            return found;
        }

        /// <summary>
        /// Null when the view's name does not parse, which means it carries no view type and so
        /// there is no way to say which view on another plot corresponds to it.
        /// </summary>
        private static ViewType TypeOfViewNamed(Document document, ElementId viewId)
        {
            var view = document.GetElement(viewId) as View;
            if (view == null) return null;

            ParsedViewName parsed;
            return ViewNameParser.TryParse(view.Name, out parsed) ? parsed.Type : null;
        }

        private static double Number(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return 0.0;
            if (parameter.StorageType != StorageType.Double) return 0.0;

            return parameter.AsDouble();
        }
    }
}
