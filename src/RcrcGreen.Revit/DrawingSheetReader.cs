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
    /// Reads a document into a <see cref="DrawingSheetSnapshot"/>. Nothing here writes and
    /// nothing here opens a transaction.
    ///
    /// There is no progress window on this read. The first real model, 96,934 elements, came
    /// back in 1.4 seconds, and this one touches only views and scope boxes rather than every
    /// element. A progress window on a read that fast is a flicker, not information.
    /// </summary>
    internal static class DrawingSheetReader
    {
        public static DrawingSheetSnapshot Read(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var plotIds = new List<string>();
            var viewTypes = new List<ViewType>();
            var present = new List<PlotViewPresence>();

            int viewsRead = 0;
            int fromParameter = 0;
            int fromViewName = 0;
            int withNoPlot = 0;

            foreach (View view in new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => !(view is ViewSheet) && !view.IsTemplate))
            {
                viewsRead++;

                string onTheView = ValueOf(view.LookupParameter(ModelScanner.PlotIdParameterName));
                ViewPlotReading reading = ViewPlotReader.Read(onTheView, view.Name);

                if (!reading.Found)
                {
                    withNoPlot++;
                    continue;
                }

                if (reading.Source == PlotSourceOnView.Parameter)
                {
                    fromParameter++;
                }
                else
                {
                    fromViewName++;
                }

                plotIds.Add(reading.PlotId);

                // The column a view fills still comes from its name, because the code and the
                // view name together are what a view type is and PRX_Plot_ID holds neither.
                // A view whose plot came from the parameter but whose name does not parse has
                // a row and fills no column, which is the honest answer.
                ParsedViewName parsed;
                if (ViewNameParser.TryParse(view.Name, out parsed))
                {
                    viewTypes.Add(parsed.Type);
                    present.Add(new PlotViewPresence(reading.PlotId, parsed.Type, view.Id.Value));
                }
            }

            var scopeBoxNames = new List<string>();
            foreach (Element box in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType())
            {
                string plotId;
                if (PlotId.TryRead(box.Name, out plotId)) scopeBoxNames.Add(plotId);
            }

            return new DrawingSheetSnapshot(
                document.Title,
                plotIds,
                viewTypes,
                present,
                scopeBoxNames,
                viewsRead,
                fromParameter,
                fromViewName,
                withNoPlot);
        }

        private static string ValueOf(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return null;

            if (parameter.StorageType == StorageType.String)
            {
                return parameter.AsString();
            }

            return parameter.AsValueString();
        }
    }
}
