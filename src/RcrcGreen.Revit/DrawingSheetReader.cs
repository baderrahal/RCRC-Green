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
    /// One pass over the views gives the columns, which cells are filled and the scope box
    /// state of every view. The panel then works the six case counts out again on every tick
    /// without asking Revit anything, which is what makes those counts follow the tick boxes
    /// instantly.
    ///
    /// The plot list is the union of four places a plot shows up: view names, PRX_Plot_ID on
    /// views, scope boxes and PRX_Plot_ID on elements. It was built from views alone once, and a plot existing only
    /// as a scope box with tagged elements, which is the plot with everything missing, never
    /// got a row. The element walk is what pays for that: 96,934 elements came back in 1.4
    /// seconds on the first real model, so there is still no progress window here.
    /// </summary>
    internal static class DrawingSheetReader
    {
        public static DrawingSheetSnapshot Read(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var plotIds = new List<string>();
            var viewNames = new List<string>();
            var viewPlotParameterValues = new List<string>();
            var viewTypes = new List<ViewType>();
            var present = new List<PlotViewPresence>();
            var states = new List<ViewScopeBoxState>();
            var scheduleTypes = new List<ViewType>();
            var sectionTypes = new List<ViewType>();

            int viewsRead = 0;
            int fromParameter = 0;
            int fromViewName = 0;
            int withNoPlot = 0;
            int parameterNotAPlot = 0;
            int disagree = 0;
            var valuesThatAreNotPlots = new List<string>();

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

                // Both raw readings go to the registry as well, which owns the union that
                // makes the plot list. The per-view decision below still answers which plot
                // each view belongs to, and the registry never contradicts it because both
                // run the same parser and the same identifier rule.
                viewNames.Add(view.Name);
                if (onTheView != null) viewPlotParameterValues.Add(onTheView);

                if (read.Found)
                {
                    plotIds.Add(read.PlotId);
                    if (read.Source == PlotSourceOnView.Parameter) fromParameter++;
                    else fromViewName++;
                }
                else if (read.Source == PlotSourceOnView.ParameterNotAPlot)
                {
                    // Counted apart from the views with nothing, because a value that is
                    // present and wrong needs seeing rather than only counting. It used to
                    // land under with no plot at all while its cell filled from the name.
                    parameterNotAPlot++;
                    valuesThatAreNotPlots.Add(read.RawParameterValue);
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

                // The one reader of a view's scope box state, shared with the assignment so
                // the count on screen and the write can never read the model two ways.
                states.Add(ScopeBoxScanner.StateOf(document, view));
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

            // The union of every place a plot shows up, with the sources kept per plot so the
            // panel can mark the ones no view carries. The walk over every element is what
            // brings in a plot that exists only on tagged elements.
            ModelScanner.ElementPlotIdRead elements =
                ModelScanner.PlotIdValuesAcrossElements(document, null);

            PlotRegistryResult registry = PlotRegistry.Build(
                viewNames,
                viewPlotParameterValues,
                scopeBoxNames,
                elements == null ? null : elements.Counts.Keys);

            // The same capture the run builds from, run here so the plan preview reads the
            // same rule for which schedules can be made rather than a rule of its own.
            ScheduleCapture.CapturedSchedules captured = ScheduleCapture.Read(document);

            // The three dropdowns a view type with no example is answered through. The
            // templates come off their own pass because the view loop above skips them on
            // purpose, and the family type carries its kind so the panel can say which picks
            // create a section and take no level.
            var familyTypes = new List<ScannedViewFamilyType>();
            foreach (ViewFamilyType familyType in new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>())
            {
                familyTypes.Add(new ScannedViewFamilyType(
                    familyType.Name ?? string.Empty, familyType.ViewFamily.ToString()));
            }

            var templateNames = new List<string>();
            foreach (View template in new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => view.IsTemplate))
            {
                templateNames.Add(template.Name);
            }

            var levelNames = new List<string>();
            foreach (Level level in new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>())
            {
                levelNames.Add(level.Name);
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
                numbersByPlot,
                registry.Plots,
                captured.Usable.Keys,
                captured.Refused,
                parameterNotAPlot,
                valuesThatAreNotPlots,
                // The registry classified these and the reader used to drop them. A scope box
                // named dm-41 made its plot vanish with the reason worked out and thrown away.
                registry.Ignored.Where(one => one.Reason == IgnoredReason.WrongCase),
                captured.NotParsed,
                familyTypes,
                templateNames,
                levelNames);
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
