using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;

using ViewType = RcrcGreen.Core.ViewType;

// Autodesk.Revit.DB has a ScheduleDefinition of its own, and both are in scope here because
// this file is the join between them. The Core one is the plain values that survive a save.
using CapturedSchedule = RcrcGreen.Core.ScheduleDefinition;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads a schedule that already exists into a <see cref="ScheduleDefinition"/>.
    ///
    /// This is the read half of building a schedule. Nothing here writes and nothing here opens
    /// a transaction. What comes back holds only strings and values, so Core can reason about
    /// it and so the same definition could one day be written to a file and loaded into a model
    /// that has no schedule to copy from.
    /// </summary>
    internal static class ScheduleCapture
    {
        /// <summary>
        /// One definition per view type, taken from the first plot that has it.
        ///
        /// The first is as good as any because every plot's copy of a schedule differs only in
        /// the plot named in its filter, which is the one thing create replaces.
        /// </summary>
        public static Dictionary<ViewType, CapturedSchedule> ByType(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var found = new Dictionary<ViewType, CapturedSchedule>();

            foreach (ViewSchedule schedule in new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(schedule => !schedule.IsTemplate))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(schedule.Name, out parsed)) continue;
                if (found.ContainsKey(parsed.Type)) continue;

                CapturedSchedule read = Of(document, schedule, parsed.Type);

                // A schedule that filters on no plot cannot be aimed at another one, so keeping
                // it here would offer the user something that cannot be built.
                if (read != null && read.CanBeMadeForAnotherPlot) found.Add(parsed.Type, read);
            }

            return found;
        }

        public static CapturedSchedule Of(Document document, ViewSchedule schedule, ViewType type)
        {
            Autodesk.Revit.DB.ScheduleDefinition definition = schedule.Definition;
            if (definition == null) return null;

            var fields = new List<string>();
            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                ScheduleField field = definition.GetField(fieldId);
                if (field != null) fields.Add(field.GetName());
            }

            var rules = new List<ScheduleFilterRule>();
            foreach (ScheduleFilter filter in definition.GetFilters())
            {
                ScheduleField field = definition.GetField(filter.FieldId);
                if (field == null) continue;

                string value = ValueIn(filter);
                if (value == null) continue;

                rules.Add(new ScheduleFilterRule(field.GetName(), value));
            }

            Category category = Category.GetCategory(document, definition.CategoryId);

            return new CapturedSchedule(
                type,
                category == null ? string.Empty : category.Name,
                fields,
                rules,
                definition.IncludeLinkedFiles,
                definition.CategoryId == new ElementId(BuiltInCategory.OST_Sheets));
        }

        /// <summary>
        /// A schedule filter holds its value in whichever of four typed getters matches the
        /// parameter, and asking the wrong one throws. Only the string form is of any use for
        /// naming a plot, and the others come back as text so a rule telling two schedules
        /// apart is not lost.
        /// </summary>
        private static string ValueIn(ScheduleFilter filter)
        {
            try
            {
                if (filter.IsStringValue) return filter.GetStringValue();
                if (filter.IsIntegerValue) return filter.GetIntegerValue().ToString();
                if (filter.IsDoubleValue) return filter.GetDoubleValue().ToString();
                if (filter.IsElementIdValue) return filter.GetElementIdValue().ToString();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                // A filter whose value cannot be read is left out rather than guessed at. It
                // means the definition will not rebuild that schedule, which is reported.
            }
            catch (InvalidOperationException)
            {
            }

            return null;
        }
    }
}
