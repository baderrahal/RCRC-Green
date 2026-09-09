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

            var fields = new List<ScheduleFieldEntry>();
            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                ScheduleField field = definition.GetField(fieldId);
                if (field != null) fields.Add(new ScheduleFieldEntry(field.GetName(), KindOf(field)));
            }

            var rules = new List<ScheduleFilterRule>();
            foreach (ScheduleFilter filter in definition.GetFilters())
            {
                ScheduleField field = definition.GetField(filter.FieldId);
                if (field == null) continue;

                FilterValue value = ValueIn(filter);
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
                definition.CategoryId == new ElementId(BuiltInCategory.OST_Sheets),
                BuiltInValueOf(category));
        }

        /// <summary>
        /// Revit's own number for the category, which is what a new schedule is built from.
        ///
        /// KERBS is built on Slab Edges. Looking that name up in Document.Settings.Categories
        /// found nothing and the schedule was refused with "this model has no category named
        /// Slab Edges", on a model that plainly has it. Its number is OST_EdgeSlab and that
        /// resolves whatever the name reads as, in any language, at either level of the
        /// category tree.
        /// </summary>
        private static long BuiltInValueOf(Category category)
        {
            if (category == null) return 0L;

            BuiltInCategory builtIn = category.BuiltInCategory;
            return builtIn == BuiltInCategory.INVALID ? 0L : (long)builtIn;
        }

        /// <summary>
        /// Whether a field is a parameter of the scheduled elements or something the schedule
        /// works out for itself.
        ///
        /// A calculated field is defined inside the schedule that holds it, so it never appears
        /// in GetSchedulableFields and no amount of name matching will find it. Three schedules
        /// came out short of one and the report blamed the category, which sends somebody to
        /// look in the wrong place.
        /// </summary>
        private static ScheduleFieldKind KindOf(ScheduleField field)
        {
            switch (field.FieldType)
            {
                case ScheduleFieldType.Formula:
                case ScheduleFieldType.Percentage:
                case ScheduleFieldType.Count:
                case ScheduleFieldType.CombinedParameter:
                    return ScheduleFieldKind.Calculated;

                default:
                    return ScheduleFieldKind.AParameter;
            }
        }

        /// <summary>
        /// A schedule filter holds its value in whichever of four typed getters matches the
        /// parameter, and asking the wrong one throws.
        ///
        /// The kind travels with the value now. Flattening all four to text lost two schedules:
        /// both filter on PRX_Included In Budget equals Yes, which is a Yes/No parameter that
        /// Revit holds as the integer 1, and handing back the word made Revit answer that the
        /// filter value is not valid for the field and filter type.
        /// </summary>
        private static FilterValue ValueIn(ScheduleFilter filter)
        {
            try
            {
                if (filter.IsStringValue) return FilterValue.Text(filter.GetStringValue());
                if (filter.IsIntegerValue) return FilterValue.OfWholeNumber(filter.GetIntegerValue());
                if (filter.IsDoubleValue) return FilterValue.OfNumber(filter.GetDoubleValue());
                if (filter.IsElementIdValue)
                {
                    return FilterValue.OfElementReference(filter.GetElementIdValue().Value);
                }
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
