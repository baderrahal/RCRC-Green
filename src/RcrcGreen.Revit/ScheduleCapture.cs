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
        /// What one capture pass found: the definitions a run can build from, and the
        /// schedule types that exist and cannot be built from, each with the reason its
        /// refusal prints.
        ///
        /// The plan and the writer both read Usable, and the panel's snapshot carries the
        /// same two lists from its own read, so the preview and the run follow one rule for
        /// which schedules can be made. The panel passed every schedule type as capturable
        /// once, and step 5 promised schedules the confirmation then refused.
        /// </summary>
        public sealed class CapturedSchedules
        {
            public CapturedSchedules(
                Dictionary<ViewType, CapturedSchedule> usable,
                List<UncapturableSchedule> refused,
                List<IgnoredName> notParsed)
            {
                Usable = usable ?? new Dictionary<ViewType, CapturedSchedule>();
                Refused = refused ?? new List<UncapturableSchedule>();
                NotParsed = notParsed ?? new List<IgnoredName>();
            }

            public Dictionary<ViewType, CapturedSchedule> Usable { get; }

            public IReadOnlyList<UncapturableSchedule> Refused { get; }

            /// <summary>
            /// Schedules skipped because their names do not parse, each with the reason. They
            /// were skipped with a bare continue, so a schedule named in the wrong case was
            /// silently uncapturable.
            /// </summary>
            public IReadOnlyList<IgnoredName> NotParsed { get; }
        }

        /// <summary>
        /// One definition per view type, taken from the first plot whose copy is usable.
        ///
        /// The first usable copy is as good as any because every plot's copy of a schedule
        /// differs only in the plot named in its filter, which is the one thing create
        /// replaces. A copy that lost a filter at capture or filters on no plot does not
        /// reserve the type, so a later plot's clean copy can still fill it, and the type is
        /// refused with the first copy's reason only when no copy anywhere is usable.
        /// </summary>
        public static CapturedSchedules Read(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            var usable = new Dictionary<ViewType, CapturedSchedule>();
            var whyNot = new Dictionary<ViewType, string>();
            var notParsed = new List<IgnoredName>();

            foreach (ViewSchedule schedule in new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(schedule => !schedule.IsTemplate))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(schedule.Name, out parsed))
                {
                    // Recorded the way every other loss here is, with the registry's own
                    // reason, so a schedule named dm-41-(600) FURNITURE SCHEDULE is named on
                    // the status line rather than skipped in silence.
                    notParsed.Add(new IgnoredName(
                        schedule.Name,
                        ViewNameParser.FailsOnlyOnPlotCase(schedule.Name)
                            ? IgnoredReason.WrongCase
                            : IgnoredReason.NotAPlotName));
                    continue;
                }

                if (usable.ContainsKey(parsed.Type)) continue;

                CapturedSchedule read = Of(document, schedule, parsed.Type);
                if (read == null)
                {
                    Why(whyNot, parsed.Type,
                        "That schedule exists and its definition could not be read at capture, "
                        + "so there is nothing to build from.");
                    continue;
                }

                // The lost filter is checked before the plot filter, because the filter that
                // could not be read may be the plot filter itself.
                if (read.LostAFilterAtCapture)
                {
                    Why(whyNot, parsed.Type, read.WhyTheCaptureLossRefusesIt());
                    continue;
                }

                if (!read.CanBeMadeForAnotherPlot)
                {
                    Why(whyNot, parsed.Type, read.WhyItCannotBeAimed());
                    continue;
                }

                usable.Add(parsed.Type, read);
                whyNot.Remove(parsed.Type);
            }

            var refused = whyNot
                .Where(pair => !usable.ContainsKey(pair.Key))
                .Select(pair => new UncapturableSchedule(pair.Key, pair.Value))
                .ToList();

            return new CapturedSchedules(usable, refused, notParsed);
        }

        private static void Why(Dictionary<ViewType, string> whyNot, ViewType type, string why)
        {
            if (!whyNot.ContainsKey(type)) whyNot.Add(type, why);
        }

        public static CapturedSchedule Of(Document document, ViewSchedule schedule, ViewType type)
        {
            Autodesk.Revit.DB.ScheduleDefinition definition = schedule.Definition;
            if (definition == null) return null;

            // Every read that fails is recorded on the definition rather than skipped, so a
            // loss at capture is as loud as one at write time. Both were bare continues once,
            // and a schedule that lost the filter telling HARDSCAPE from SHRUBS AND LAWN was
            // one unreadable value away from being built and reading as correct.
            var fields = new List<ScheduleFieldEntry>();
            var fieldsNotRead = new List<string>();
            int fieldAt = 0;
            foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
            {
                fieldAt++;

                ScheduleField field = definition.GetField(fieldId);
                if (field == null)
                {
                    fieldsNotRead.Add("the field at position " + fieldAt
                        + ", whose id resolved to nothing");
                    continue;
                }

                fields.Add(new ScheduleFieldEntry(field.GetName(), KindOf(field)));
            }

            var rules = new List<ScheduleFilterRule>();
            var filtersNotRead = new List<string>();
            int filterAt = 0;
            foreach (ScheduleFilter filter in definition.GetFilters())
            {
                filterAt++;

                ScheduleField field = definition.GetField(filter.FieldId);
                if (field == null)
                {
                    filtersNotRead.Add("the filter at position " + filterAt
                        + ", on a field that could not be resolved");
                    continue;
                }

                FilterValue value = ValueIn(filter);
                if (value == null)
                {
                    filtersNotRead.Add("the filter on " + field.GetName()
                        + ", whose value could not be read");
                    continue;
                }

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
                BuiltInValueOf(category),
                filtersNotRead,
                fieldsNotRead);
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
