using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Makes the views and schedules a run decided on.
    ///
    /// Everything here runs inside one transaction opened by the caller, so the whole run is
    /// one undo. Nothing here decides what to make. <see cref="RunPlan"/> did that before the
    /// transaction was opened, which is what lets the confirmation offer a real count.
    ///
    /// A refusal from Revit on one item is recorded and the run carries on, so one awkward plot
    /// does not roll back everything else that worked.
    /// </summary>
    internal static class ModelWriter
    {
        public static void Make(
            Document document,
            RunPlan plan,
            Dictionary<ViewType, RcrcGreen.Core.ScheduleDefinition> definitions,
            Dictionary<string, ElementId> boxIdByName,
            List<RunRefusal> refused)
        {
            ViewFamilyType floorPlanType = PlanViewTypeIn(document);

            foreach (RunItem item in plan.Items)
            {
                try
                {
                    if (item.Kind == RunItemKind.Schedule)
                    {
                        MakeSchedule(document, item, definitions);
                    }
                    else
                    {
                        MakePlanView(document, item, floorPlanType, boxIdByName);
                    }
                }
                catch (Autodesk.Revit.Exceptions.ApplicationException failed)
                {
                    refused.Add(new RunRefusal(item.PlotId, item.Type, "Revit refused it. " + failed.Message));
                }
                catch (InvalidOperationException failed)
                {
                    refused.Add(new RunRefusal(item.PlotId, item.Type, "Revit would not do it. " + failed.Message));
                }
                catch (ArgumentException failed)
                {
                    refused.Add(new RunRefusal(item.PlotId, item.Type, "Revit refused an argument. " + failed.Message));
                }
            }
        }

        /// <summary>
        /// A fresh plan view. Never a duplicate of another plot's, so it carries no annotation,
        /// no dimensions, no tags and no detailing, which is what the team asked for.
        ///
        /// The scope box goes on through the same parameter the Scope Box logic writes, so a
        /// created view is in the state that logic would have put it in anyway.
        /// </summary>
        private static void MakePlanView(
            Document document,
            RunItem item,
            ViewFamilyType floorPlanType,
            Dictionary<string, ElementId> boxIdByName)
        {
            if (floorPlanType == null)
            {
                throw new InvalidOperationException(
                    "This model holds no floor plan view family type, so a plan view cannot be created.");
            }

            Level level = LowestLevelIn(document);
            if (level == null)
            {
                throw new InvalidOperationException(
                    "This model holds no level, and a plan view has to be made on one.");
            }

            ViewPlan made = ViewPlan.Create(document, floorPlanType.Id, level.Id);
            made.Name = item.Name;

            Parameter plot = made.LookupParameter(ModelScanner.PlotIdParameterName);
            if (plot != null && !plot.IsReadOnly) plot.Set(item.PlotId);

            ElementId boxId;
            if (boxIdByName.TryGetValue(item.PlotId, out boxId))
            {
                Parameter holder = made.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);
                if (holder != null && !holder.IsReadOnly) holder.Set(boxId);
            }
        }

        /// <summary>
        /// A schedule built from a captured definition with the plot swapped, as
        /// <see cref="ScheduleDefinition"/> sets out. Nothing is duplicated.
        /// </summary>
        private static void MakeSchedule(
            Document document,
            RunItem item,
            Dictionary<ViewType, RcrcGreen.Core.ScheduleDefinition> definitions)
        {
            RcrcGreen.Core.ScheduleDefinition captured;
            if (!definitions.TryGetValue(item.Type, out captured))
            {
                throw new InvalidOperationException(
                    "No definition was captured for " + item.Type + ".");
            }

            RcrcGreen.Core.ScheduleDefinition wanted = captured.ForPlot(item.PlotId);

            ViewSchedule made = wanted.IsASheetList
                ? ViewSchedule.CreateSheetList(document)
                : ViewSchedule.CreateSchedule(document, CategoryIdFor(document, wanted.CategoryName));

            made.Name = wanted.NameFor(item.PlotId);

            Autodesk.Revit.DB.ScheduleDefinition definition = made.Definition;
            definition.IncludeLinkedFiles = wanted.IncludesLinkedFiles;

            Dictionary<string, SchedulableField> available = definition.GetSchedulableFields()
                .GroupBy(field => field.GetName(document), StringComparer.Ordinal)
                .ToDictionary(byName => byName.Key, byName => byName.First(), StringComparer.Ordinal);

            // Added in the captured order, because the order is what the schedule looks like and
            // a field list in a different order is a different schedule to the person reading it.
            var fieldByName = new Dictionary<string, ScheduleField>(StringComparer.Ordinal);
            foreach (string name in wanted.FieldsInOrder)
            {
                SchedulableField schedulable;
                if (!available.TryGetValue(name, out schedulable)) continue;
                if (fieldByName.ContainsKey(name)) continue;

                fieldByName.Add(name, definition.AddField(schedulable));
            }

            foreach (ScheduleFilterRule rule in wanted.Filters)
            {
                ScheduleField field;
                if (!fieldByName.TryGetValue(rule.ParameterName, out field)) continue;

                definition.AddFilter(new ScheduleFilter(field.FieldId, ScheduleFilterType.Equal, rule.Value));
            }
        }

        private static ElementId CategoryIdFor(Document document, string categoryName)
        {
            foreach (Category category in document.Settings.Categories.Cast<Category>())
            {
                if (string.Equals(category.Name, categoryName, StringComparison.Ordinal))
                {
                    return category.Id;
                }
            }

            throw new InvalidOperationException(
                "This model has no category named " + categoryName + ", so that schedule cannot be built.");
        }

        /// <summary>
        /// Any floor plan type will do. The team has not said which, and picking one here would
        /// be inventing a rule, so the first is used and the log says so.
        /// </summary>
        private static ViewFamilyType PlanViewTypeIn(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(type => type.ViewFamily == ViewFamily.FloorPlan);
        }

        private static Level LowestLevelIn(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(level => level.Elevation)
                .FirstOrDefault();
        }
    }
}
