using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;
using CapturedSchedule = RcrcGreen.Core.ScheduleDefinition;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Makes the views and schedules a run decided on.
    ///
    /// Everything here runs inside one transaction opened by the caller, so the whole run is
    /// one undo. Nothing here decides what to make. <see cref="RunPlan"/> did that before the
    /// transaction was opened, which is what lets the confirmation offer a real count.
    ///
    /// Nothing is ever skipped quietly. Every field that would not resolve, every filter that
    /// could not be applied and every lookup that came back empty ends up in one of the two
    /// lists the caller hands in, and both are printed in the report.
    /// </summary>
    internal static class ModelWriter
    {
        public static void Make(
            Document document,
            RunPlan plan,
            Dictionary<ViewType, CapturedSchedule> definitions,
            Dictionary<string, ElementId> boxIdByName,
            List<RunRefusal> refused,
            List<RunRefusal> attention,
            List<RunRefusal> leftBehind)
        {
            foreach (RunItem item in plan.Items)
            {
                try
                {
                    if (item.Kind == RunItemKind.Schedule)
                    {
                        MakeSchedule(document, item, definitions, refused, attention, leftBehind);
                    }
                    else
                    {
                        MakePlanView(document, item, boxIdByName, refused, attention);
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
        /// Three things about it are read from the model rather than chosen here. The view
        /// family type is the one named after the view type. The level is the one an existing
        /// view of the same type sits on. The view template is the one whose name starts with
        /// the view type, and it carries the scale, the detail level, the discipline, the
        /// visibility overrides and the phase filter, so setting it is how all of those follow.
        /// </summary>
        private static void MakePlanView(
            Document document,
            RunItem item,
            Dictionary<string, ElementId> boxIdByName,
            List<RunRefusal> refused,
            List<RunRefusal> attention)
        {
            string familyTypeName = ViewTypeNaming.FamilyTypeNameFor(item.Type);

            ViewFamilyType familyType = new FilteredElementCollector(document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(type => string.Equals(type.Name, familyTypeName, StringComparison.Ordinal));

            if (familyType == null)
            {
                // No fallback to the first floor plan type. A view made with the wrong family
                // type looks finished and is wrong, which is worse than not making it.
                refused.Add(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "No view family type is named " + familyTypeName + ". The view was not created, "
                    + "because a view made with a different family type would be wrong and would "
                    + "look finished."));
                return;
            }

            ViewPlan sameTypeElsewhere = ExistingViewOfType(document, item.Type);
            if (sameTypeElsewhere == null || sameTypeElsewhere.GenLevel == null)
            {
                refused.Add(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "No plot in this model has a " + item.Type + " to take a level from, so there "
                    + "is nothing to say which level it belongs on. Picking one would be a guess."));
                return;
            }

            ViewPlan made = ViewPlan.Create(document, familyType.Id, sameTypeElsewhere.GenLevel.Id);
            made.Name = item.Name;

            ApplyTemplate(document, made, item, attention);

            Parameter plot = made.LookupParameter(ModelScanner.PlotIdParameterName);
            if (plot != null && !plot.IsReadOnly) plot.Set(item.PlotId);

            ElementId boxId;
            if (boxIdByName.TryGetValue(item.PlotId, out boxId))
            {
                Parameter holder = made.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);
                if (holder != null && !holder.IsReadOnly) holder.Set(boxId);
            }
        }

        private static void ApplyTemplate(
            Document document, ViewPlan made, RunItem item, List<RunRefusal> attention)
        {
            List<View> templates = new FilteredElementCollector(document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(view => view.IsTemplate)
                .ToList();

            TemplateMatch match = ViewTypeNaming.TemplateFor(item.Type, templates.Select(one => one.Name));

            if (match.Found)
            {
                View wanted = templates.First(one => string.Equals(one.Name, match.Name, StringComparison.Ordinal));
                made.ViewTemplateId = wanted.Id;
                return;
            }

            if (match.Ambiguous)
            {
                attention.Add(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created with no view template. " + match.Candidates.Count + " templates start "
                    + "with that view type and picking one would be a guess. The candidates are "
                    + string.Join(", ", match.Candidates.ToArray()) + "."));
                return;
            }

            attention.Add(new RunRefusal(
                item.PlotId,
                item.Type,
                "Created with no view template. No template name starts with " + item.Type
                + ", so the scale, detail level, discipline and phase filter are whatever a new "
                + "view gets by default."));
        }

        /// <summary>
        /// Any view of the same type on any plot. The level it sits on is what the team chose,
        /// which is a better answer than the lowest level in the model.
        /// </summary>
        private static ViewPlan ExistingViewOfType(Document document, ViewType type)
        {
            foreach (ViewPlan view in new FilteredElementCollector(document)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .Where(view => !view.IsTemplate))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(view.Name, out parsed)) continue;
                if (parsed.Type.Equals(type)) return view;
            }

            return null;
        }

        /// <summary>
        /// A schedule built from a captured definition with the plot swapped.
        ///
        /// A filter that will not go on is fatal and a field that will not go on is not, and
        /// the two are handled differently on purpose. A quantity schedule missing its plot
        /// filter shows every plot's elements and reads as correct on a drawing, so one is
        /// deleted again inside the same transaction and reported as refused. A schedule short
        /// of a column is visibly short, so it is kept and named in the report.
        /// </summary>
        private static void MakeSchedule(
            Document document,
            RunItem item,
            Dictionary<ViewType, CapturedSchedule> definitions,
            List<RunRefusal> refused,
            List<RunRefusal> attention,
            List<RunRefusal> leftBehind)
        {
            CapturedSchedule captured;
            if (!definitions.TryGetValue(item.Type, out captured))
            {
                refused.Add(new RunRefusal(
                    item.PlotId, item.Type, "No definition was captured for that schedule."));
                return;
            }

            CapturedSchedule wanted = captured.ForPlot(item.PlotId);

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
            var missingFields = new List<string>();

            foreach (string name in wanted.FieldsInOrder)
            {
                if (fieldByName.ContainsKey(name)) continue;

                SchedulableField schedulable;
                if (!available.TryGetValue(name, out schedulable))
                {
                    missingFields.Add(name);
                    continue;
                }

                fieldByName.Add(name, definition.AddField(schedulable));
            }

            var missingFilters = new List<string>();
            foreach (ScheduleFilterRule rule in wanted.Filters)
            {
                ScheduleField field;
                if (!fieldByName.TryGetValue(rule.ParameterName, out field))
                {
                    missingFilters.Add(rule.ToString());
                    continue;
                }

                definition.AddFilter(new ScheduleFilter(field.FieldId, ScheduleFilterType.Equal, rule.Value));
            }

            if (missingFilters.Count > 0)
            {
                string lost = string.Join(", ", missingFilters.ToArray());
                string named = made.Name;

                // The delete is guarded on its own rather than left to the catch around the
                // whole item. If Revit refuses it there, the report would say the schedule was
                // deleted while the model still held it, and a report that disagrees with the
                // model is the one thing worth more than the schedule.
                if (Deleted(document, made))
                {
                    refused.Add(new RunRefusal(
                        item.PlotId,
                        item.Type,
                        "Created and then deleted again, because " + missingFilters.Count
                        + " of its filters could not be applied. Not applied: " + lost
                        + ". A schedule missing a filter shows every plot's elements and reads as "
                        + "correct on a drawing, so it is not left in the model."));
                    return;
                }

                leftBehind.Add(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "STILL IN THE MODEL, named " + named + ". It is missing " + missingFilters.Count
                    + " of its filters, not applied: " + lost + ". Revit refused to delete it "
                    + "again, so it is there and it is wrong. It shows every plot's elements and "
                    + "will read as correct on a drawing. Delete it by hand."));
                return;
            }

            if (missingFields.Count > 0)
            {
                attention.Add(new RunRefusal(
                    item.PlotId,
                    item.Type,
                    "Created without " + missingFields.Count + " of its fields, because they are "
                    + "not schedulable for this category here. Missing: "
                    + string.Join(", ", missingFields.ToArray())
                    + ". The schedule is short of those columns."));
            }
        }

        /// <summary>
        /// True only when the schedule really is gone. Deleting an element made earlier in the
        /// same transaction ought to work and has never been run, so what happens when it does
        /// not is written down rather than assumed away.
        /// </summary>
        private static bool Deleted(Document document, ViewSchedule made)
        {
            try
            {
                ICollection<ElementId> gone = document.Delete(made.Id);
                return gone != null && gone.Count > 0;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
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
    }
}
