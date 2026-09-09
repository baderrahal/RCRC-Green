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
    /// One pass over the views gives the plot list, the columns, which cells are filled and the
    /// scope box state of every view. The panel then works the six case counts out again on
    /// every tick without asking Revit anything, which is what makes those counts follow the
    /// tick boxes instantly.
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
            var states = new List<ViewScopeBoxState>();
            var scheduleTypes = new List<ViewType>();
            var sectionTypes = new List<ViewType>();

            int viewsRead = 0;
            int fromParameter = 0;
            int fromViewName = 0;
            int withNoPlot = 0;
            int disagree = 0;

            foreach (View view in new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                // A sheet is a View and so is a template. Neither is a view of a plot, and a
                // template's scope box would push onto every view using it.
                .Where(view => !(view is ViewSheet) && !view.IsTemplate))
            {
                viewsRead++;

                string onTheView = ValueOf(view.LookupParameter(ModelScanner.PlotIdParameterName));
                ViewOnAPlot read = ViewReading.Read(onTheView, view.Name, view.Id.Value);

                if (read.Found)
                {
                    plotIds.Add(read.PlotId);
                    if (read.Source == PlotSourceOnView.Parameter) fromParameter++;
                    else fromViewName++;
                }
                else
                {
                    withNoPlot++;
                }

                if (read.Fills != null)
                {
                    // A ViewSchedule is a View, so it arrives in the same collector. It is a
                    // different thing to build and it filters on a different parameter, so the
                    // grid has to be able to tell the user which columns are schedules.
                    if (view is ViewSchedule) scheduleTypes.Add(read.Fills.Where.ViewType);

                    // Which types need a section rather than a plan comes off the kind of the
                    // views the model already holds, never off the code in the name. (400) is a
                    // section on this model and that is a fact about this model. ViewPlan.Create
                    // can never make one, which is what refused every (400) in the first run.
                    if (view is ViewSection) sectionTypes.Add(read.Fills.Where.ViewType);

                    // The plot on the cell comes from the name, so it can differ from the row
                    // plot above. Both are plots the model really holds, and both belong in
                    // the list, or a filled cell would have no row to sit in.
                    plotIds.Add(read.Fills.Where.PlotId);
                    viewTypes.Add(read.Fills.Where.ViewType);
                    present.Add(read.Fills);
                }

                if (read.SourcesDisagree) disagree++;

                states.Add(ScopeBoxStateOf(document, view));
            }

            var scopeBoxNames = new List<string>();
            foreach (Element box in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_VolumeOfInterest)
                .WhereElementIsNotElementType())
            {
                scopeBoxNames.Add(box.Name);
            }

            var sheetNames = new List<string>();
            var sheetNumbers = new List<string>();
            var numbersByPlot = new List<SheetOnAPlot>();
            foreach (ViewSheet sheet in new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>()
                .Where(sheet => !sheet.IsTemplate))
            {
                sheetNames.Add(sheet.Name);
                sheetNumbers.Add(sheet.SheetNumber);

                // The plot letter a new number continues is read off the numbers the plot
                // already has, found through PRX_Plot_ID on the sheet. A sheet carrying no
                // plot belongs to no plot's pattern.
                Parameter plot = sheet.LookupParameter(ModelScanner.PlotIdParameterName);
                string plotId = plot != null && plot.HasValue ? plot.AsString() : null;
                if (!string.IsNullOrWhiteSpace(plotId))
                {
                    numbersByPlot.Add(new SheetOnAPlot(plotId, sheet.SheetNumber));
                }
            }

            // Types rather than instances. A model can hold a title block type no sheet uses
            // yet, and that is exactly the one somebody is about to start using.
            var titleBlocks = new List<TitleBlockType>();
            foreach (FamilySymbol symbol in new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>())
            {
                titleBlocks.Add(new TitleBlockType(symbol.FamilyName ?? string.Empty, symbol.Name));
            }

            return new DrawingSheetSnapshot(
                document.Title,
                DateTime.Now,
                plotIds,
                viewTypes,
                present,
                scheduleTypes,
                sectionTypes,
                scopeBoxNames,
                states,
                titleBlocks,
                sheetNames,
                sheetNumbers,
                viewsRead,
                fromParameter,
                fromViewName,
                withNoPlot,
                disagree,
                numbersByPlot);
        }

        private static ViewScopeBoxState ScopeBoxStateOf(Document document, View view)
        {
            Parameter holder = view.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);
            bool canHold = holder != null && !holder.IsReadOnly;

            string current = string.Empty;
            if (holder != null && holder.HasValue)
            {
                Element box = document.GetElement(holder.AsElementId());
                if (box != null) current = box.Name;
            }

            return new ViewScopeBoxState(view.Id.Value, view.Name, canHold, current);
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
