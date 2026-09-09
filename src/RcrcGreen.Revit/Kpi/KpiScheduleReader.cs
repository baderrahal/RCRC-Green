using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// Sections 5 to 8. Every schedule by name, category, fields and filters. Then, for a few
    /// plots of each name the workbook draws from, the rows exactly as a sheet prints them,
    /// the elements each lists with every parameter they carry, and the areas read off those
    /// elements raw and printed.
    ///
    /// A few copies per name and not every copy, because the real model holds six schedules
    /// per plot over 160 plots and regenerating a thousand schedules to print their rows is a
    /// read nobody waits for. How many is KpiReport.PlotsReadInFull, Core's number, because
    /// the report states the rule. The copies read are the first in name order that list
    /// anything, and the report says which.
    /// </summary>
    internal static class KpiScheduleReader
    {
        /// <summary>
        /// How many copies of one schedule name are tried before the first one is taken
        /// whether or not it lists anything.
        /// </summary>
        public const int CopiesTried = 10;

        /// <summary>
        /// How many plots of one name are read in full, read from Core so the report's own
        /// sentence and this loop can never describe two different rules.
        /// </summary>
        public static int PlotsReadInFull
        {
            get { return KpiReport.PlotsReadInFull; }
        }

        /// <summary>
        /// How many elements per schedule have their areas measured. Ten is enough to see
        /// the raw number and the printed one side by side.
        /// </summary>
        public const int ElementsMeasured = 10;

        public static ScheduleFacts Read(Document document, List<string> skipped)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (skipped == null) throw new ArgumentNullException("skipped");

            HashSet<ElementId> onSheets = OnSheets(document);

            var schedules = new List<ViewSchedule>();
            int templates = 0;

            foreach (ViewSchedule schedule in new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>())
            {
                if (schedule.IsTitleblockRevisionSchedule || schedule.IsInternalKeynoteSchedule) continue;

                if (schedule.IsTemplate)
                {
                    templates++;
                    continue;
                }

                schedules.Add(schedule);
            }

            HashSet<ElementId> readInFull = ChooseCopiesPerName(document, schedules, skipped);

            var scanned = new List<ScannedSchedule>();
            var elements = new List<ScheduleElements>();
            var areas = new List<MeasuredArea>();

            foreach (ViewSchedule schedule in schedules)
            {
                bool inFull = readInFull.Contains(schedule.Id);
                ScannedSchedule read = Scanned(document, schedule, onSheets.Contains(schedule.Id), inFull, skipped);
                scanned.Add(read);

                if (!inFull) continue;

                ScheduleElements listed = Guarded(skipped, "elements of " + schedule.Name,
                    () => Elements(document, schedule, read));
                if (listed != null) elements.Add(listed);

                if (read.IsSoftscape) continue;

                List<MeasuredArea> measured = Guarded(skipped, "areas of " + schedule.Name,
                    () => Areas(document, schedule, read));
                if (measured != null) areas.AddRange(measured);
            }

            return new ScheduleFacts(scanned, templates, Phases(document), elements, areas);
        }

        private static T Guarded<T>(List<string> skipped, string what, Func<T> read) where T : class
        {
            try
            {
                return read();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                skipped.Add("The " + what + " were not read. " + failed.Message);
                return null;
            }
            catch (InvalidOperationException failed)
            {
                skipped.Add("The " + what + " were not read. " + failed.Message);
                return null;
            }
            catch (ArgumentException failed)
            {
                skipped.Add("The " + what + " were not read. " + failed.Message);
                return null;
            }
        }

        private static HashSet<ElementId> OnSheets(Document document)
        {
            var placed = new HashSet<ElementId>();

            foreach (ScheduleSheetInstance instance in new FilteredElementCollector(document)
                .OfClass(typeof(ScheduleSheetInstance))
                .Cast<ScheduleSheetInstance>())
            {
                if (instance.IsTitleblockRevisionSchedule) continue;
                placed.Add(instance.ScheduleId);
            }

            return placed;
        }

        private static List<string> Phases(Document document)
        {
            var names = new List<string>();
            PhaseArray phases = document.Phases;
            if (phases == null) return names;

            foreach (Phase phase in phases)
            {
                names.Add(phase.Name);
            }

            return names;
        }

        /// <summary>
        /// For each name the workbook draws from, once the plot is taken off, the first
        /// PlotsReadInFull copies in name order that list at least one element. A plot with the
        /// schedule and no planting would otherwise be the one read, and a schedule with no
        /// rows answers nothing about its fields' values. How many copies is Core's number,
        /// read from KpiReport.PlotsReadInFull, because the report states the rule.
        /// </summary>
        private static HashSet<ElementId> ChooseCopiesPerName(Document document, List<ViewSchedule> schedules, List<string> skipped)
        {
            var chosen = new HashSet<ElementId>();

            IEnumerable<IGrouping<string, ViewSchedule>> byName = schedules
                .Where(schedule => KpiNames.HoldsAny(schedule.Name, KpiNames.ScheduleWords))
                .GroupBy(schedule => WithoutThePlot(schedule.Name), StringComparer.Ordinal);

            foreach (IGrouping<string, ViewSchedule> group in byName)
            {
                List<ViewSchedule> copies = group.OrderBy(schedule => schedule.Name, NaturalOrder.Comparer).ToList();
                var picked = new List<ViewSchedule>();

                // Held back rather than written straight into skipped, because the fallback
                // below reads the first copy after all. Recorded both ways, one schedule was
                // named as passed over and as read in full, two lines saying opposite things.
                var passedOver = new List<string>();

                foreach (ViewSchedule copy in copies.Take(CopiesTried))
                {
                    if (picked.Count == PlotsReadInFull) break;

                    if (CountListed(document, copy) > 0)
                    {
                        picked.Add(copy);
                        continue;
                    }

                    // Named, so the file records that DM-11's copy exists and was passed
                    // over rather than reading as if DM-14's were the first.
                    passedOver.Add(copy.Name + " lists no element, so it was not read in full.");
                }

                if (picked.Count == 0)
                {
                    picked.Add(copies[0]);
                    passedOver.RemoveAll(line => line.StartsWith(copies[0].Name + " lists no element",
                        StringComparison.Ordinal));
                    skipped.Add("None of the first " + Math.Min(CopiesTried, copies.Count) + " copies of "
                        + group.Key + " lists an element, so " + copies[0].Name
                        + " was read in full with nothing in it.");
                }

                skipped.AddRange(passedOver);

                foreach (ViewSchedule one in picked) chosen.Add(one.Id);
            }

            return chosen;
        }

        private static int CountListed(Document document, ViewSchedule schedule)
        {
            try
            {
                return new FilteredElementCollector(document, schedule.Id)
                    .WhereElementIsNotElementType()
                    .GetElementCount();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return 0;
            }
            catch (ArgumentException)
            {
                return 0;
            }
        }

        private static string WithoutThePlot(string name)
        {
            ParsedViewName parsed;
            return ViewNameParser.TryParse(name, out parsed) ? parsed.Type.ToString() : name;
        }

        private static ScannedSchedule Scanned(
            Document document, ViewSchedule schedule, bool onASheet, bool inFull, List<string> skipped)
        {
            Autodesk.Revit.DB.ScheduleDefinition definition = schedule.Definition;

            var fields = new List<ScheduleFieldRead>();
            var filters = new List<ScheduleFilterRead>();
            string category = string.Empty;

            if (definition != null)
            {
                Category found = definition.CategoryId == ElementId.InvalidElementId
                    ? null
                    : Category.GetCategory(document, definition.CategoryId);
                category = found == null
                    ? (definition.CategoryId == ElementId.InvalidElementId ? "(multi-category)" : string.Empty)
                    : found.Name;

                foreach (ScheduleFieldId fieldId in definition.GetFieldOrder())
                {
                    ScheduleField field = definition.GetField(fieldId);
                    if (field == null) continue;

                    fields.Add(new ScheduleFieldRead(
                        field.ColumnHeading,
                        FieldName(field),
                        field.FieldType.ToString(),
                        field.IsHidden,
                        Spec(field),
                        Unit(field)));
                }

                foreach (ScheduleFilter filter in definition.GetFilters())
                {
                    ScheduleField field = definition.GetField(filter.FieldId);
                    filters.Add(new ScheduleFilterRead(
                        field == null ? "(field not found)" : FieldName(field),
                        filter.FilterType.ToString(),
                        FilterValue(document, filter)));
                }
            }

            List<IReadOnlyList<string>> rows = null;
            int bodyRows = 0;
            bool rowsRead = false;

            if (inFull)
            {
                rows = Guarded(skipped, "rows of " + schedule.Name, () => Rows(schedule, out bodyRows));
                rowsRead = rows != null;
                if (!rowsRead) bodyRows = 0;
            }

            // Read in full is decided once, here, whether or not the rows came back. The
            // elements and areas of a chosen schedule are read either way and print either way.
            return new ScannedSchedule(
                schedule.Name,
                category,
                onASheet,
                fields,
                filters,
                ParameterReading.Printed(schedule.get_Parameter(BuiltInParameter.VIEW_PHASE)),
                ParameterReading.Printed(schedule.get_Parameter(BuiltInParameter.VIEW_PHASE_FILTER)),
                inFull,
                rowsRead,
                bodyRows,
                rows);
        }

        private static string FieldName(ScheduleField field)
        {
            try
            {
                return field.GetName();
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return "(name refused)";
            }
        }

        /// <summary>
        /// What the field measures, so an area column is known to be one without reading its
        /// heading. A text field has no spec and the API says so by throwing.
        /// </summary>
        private static string Spec(ScheduleField field)
        {
            try
            {
                ForgeTypeId spec = field.GetSpecTypeId();
                if (spec == null || spec.Empty()) return string.Empty;
                return LabelUtils.GetLabelForSpec(spec);
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The unit the column prints in when it overrides the project's, empty when it
        /// follows the project setting in section 1.
        /// </summary>
        private static string Unit(ScheduleField field)
        {
            try
            {
                FormatOptions options = field.GetFormatOptions();
                if (options == null || options.UseDefault) return string.Empty;
                return LabelUtils.GetLabelForUnit(options.GetUnitTypeId());
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }
            catch (ArgumentException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// A filter holds its value in whichever of four typed getters matches, and asking the
        /// wrong one throws. A filter with no value, such as Has Value, comes back empty.
        /// </summary>
        private static string FilterValue(Document document, ScheduleFilter filter)
        {
            try
            {
                if (filter.IsStringValue) return filter.GetStringValue();
                if (filter.IsIntegerValue) return filter.GetIntegerValue().ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (filter.IsDoubleValue) return filter.GetDoubleValue().ToString("0.########", System.Globalization.CultureInfo.InvariantCulture);
                if (filter.IsElementIdValue)
                {
                    ElementId id = filter.GetElementIdValue();
                    string name = ParameterReading.NameOf(document, id);
                    return name.Length == 0 ? "id " + id.Value : name;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return "(value refused)";
            }
            catch (InvalidOperationException)
            {
                return "(value refused)";
            }

            return string.Empty;
        }

        /// <summary>
        /// The body rows exactly as printed, cell by cell. The first rows and, when there are
        /// more than fit, the last one, because the last is where a schedule puts its total
        /// and the total is what the workbook asks for.
        /// </summary>
        private static List<IReadOnlyList<string>> Rows(ViewSchedule schedule, out int bodyRows)
        {
            TableData table = schedule.GetTableData();
            TableSectionData body = table.GetSectionData(SectionType.Body);

            bodyRows = body.NumberOfRows;
            int columns = body.NumberOfColumns;

            var rows = new List<IReadOnlyList<string>>();
            var wanted = new List<int>();

            if (bodyRows <= KpiReport.ShownRows)
            {
                for (int row = 0; row < bodyRows; row++) wanted.Add(row);
            }
            else
            {
                for (int row = 0; row < KpiReport.ShownRows - 1; row++) wanted.Add(row);
                wanted.Add(bodyRows - 1);
            }

            foreach (int row in wanted)
            {
                var cells = new List<string>(columns);
                for (int column = 0; column < columns; column++)
                {
                    cells.Add(schedule.GetCellText(SectionType.Body, row, column) ?? string.Empty);
                }
                rows.Add(cells);
            }

            return rows;
        }

        /// <summary>
        /// The elements one schedule lists, with every parameter they and their types carry,
        /// the phase each was created and demolished in, its workset and its design option,
        /// and the values of every parameter whose name holds a planting or a status word.
        /// </summary>
        private static ScheduleElements Elements(Document document, ViewSchedule schedule, ScannedSchedule read)
        {
            List<Element> listed = new FilteredElementCollector(document, schedule.Id)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            var categories = new Dictionary<string, int>(StringComparer.Ordinal);
            var familyTypes = new Dictionary<string, int>(StringComparer.Ordinal);
            var created = new Dictionary<string, int>(StringComparer.Ordinal);
            var demolished = new Dictionary<string, int>(StringComparer.Ordinal);
            var worksets = new Dictionary<string, int>(StringComparer.Ordinal);
            var options = new Dictionary<string, int>(StringComparer.Ordinal);
            var onInstances = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            var onTypes = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            var typesSeen = new Dictionary<ElementId, Element>();

            string[] words = KpiNames.PlantingWords.Concat(KpiNames.StatusWords).ToArray();
            WorksetTable worksetTable = document.IsWorkshared ? document.GetWorksetTable() : null;

            foreach (Element element in listed)
            {
                ParameterReading.Bump(categories, element.Category == null ? "(no category)" : element.Category.Name);
                ParameterReading.Bump(familyTypes, FamilyAndType(document, element));
                ParameterReading.Bump(created, PhaseName(document, element.CreatedPhaseId, "(none)"));
                ParameterReading.Bump(demolished, PhaseName(document, element.DemolishedPhaseId, "(not demolished)"));
                ParameterReading.Bump(worksets, WorksetName(worksetTable, element));
                ParameterReading.Bump(options, element.DesignOption == null ? "(main model)" : element.DesignOption.Name);

                Element type = null;
                ElementId typeId = element.GetTypeId();
                if (typeId != null && typeId != ElementId.InvalidElementId)
                {
                    if (!typesSeen.TryGetValue(typeId, out type))
                    {
                        type = document.GetElement(typeId);
                        typesSeen[typeId] = type;
                    }
                }

                // Counted per element for the type's parameters too, so a value on the type
                // counts once for every tree of that type and the counts add up to the trees.
                // Kept apart from the instance values, because one name bound to both put a
                // tree under two values.
                WordValuesOf(element, words, onInstances);
                if (type != null) WordValuesOf(type, words, onTypes);
            }

            var values = new List<ParameterValueCount>();
            foreach (KeyValuePair<string, Dictionary<string, int>> byName in onInstances)
            {
                foreach (KeyValuePair<string, int> byValue in byName.Value)
                {
                    values.Add(new ParameterValueCount(byName.Key, byValue.Key, byValue.Value, false));
                }
            }
            foreach (KeyValuePair<string, Dictionary<string, int>> byName in onTypes)
            {
                foreach (KeyValuePair<string, int> byValue in byName.Value)
                {
                    values.Add(new ParameterValueCount(byName.Key, byValue.Key, byValue.Value, true));
                }
            }

            return new ScheduleElements(
                read.Name,
                listed.Count,
                ParameterReading.Counted(categories),
                ParameterReading.Counted(familyTypes),
                ParameterReading.Tally(listed),
                ParameterReading.Tally(typesSeen.Values.Where(type => type != null)),
                ParameterReading.Counted(created),
                ParameterReading.Counted(demolished),
                ParameterReading.Counted(worksets),
                ParameterReading.Counted(options),
                values);
        }

        private static void WordValuesOf(Element element, string[] words, Dictionary<string, Dictionary<string, int>> into)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (Parameter parameter in element.Parameters)
            {
                Definition definition = parameter.Definition;
                string name = definition == null ? string.Empty : definition.Name;
                if (name.Length == 0 || !KpiNames.HoldsAny(name, words)) continue;
                if (!seen.Add(name)) continue;

                Dictionary<string, int> byValue;
                if (!into.TryGetValue(name, out byValue))
                {
                    byValue = new Dictionary<string, int>(StringComparer.Ordinal);
                    into[name] = byValue;
                }

                ParameterReading.Bump(byValue, ParameterReading.Printed(parameter));
            }
        }

        /// <summary>
        /// Every area parameter behind a field of the schedule, on the first few elements it
        /// lists, raw and printed. Which parameters are areas is read off the parameter's own
        /// data type and never off its heading.
        /// </summary>
        private static List<MeasuredArea> Areas(Document document, ViewSchedule schedule, ScannedSchedule read)
        {
            var measured = new List<MeasuredArea>();

            List<Element> listed = new FilteredElementCollector(document, schedule.Id)
                .WhereElementIsNotElementType()
                .ToElements()
                .Take(ElementsMeasured)
                .ToList();

            foreach (Element element in listed)
            {
                foreach (ScheduleFieldRead field in read.Fields)
                {
                    if (field.ParameterName.Length == 0) continue;

                    Parameter parameter = element.LookupParameter(field.ParameterName);
                    if (parameter == null || parameter.StorageType != StorageType.Double || !parameter.HasValue) continue;
                    if (!IsAnArea(parameter)) continue;

                    double raw = parameter.AsDouble();
                    if (double.IsNaN(raw) || double.IsInfinity(raw)) continue;

                    measured.Add(new MeasuredArea(
                        read.Name,
                        field.ParameterName,
                        FamilyAndType(document, element) + " id " + element.Id.Value,
                        raw,
                        ParameterReading.Printed(parameter)));
                }
            }

            return measured;
        }

        private static bool IsAnArea(Parameter parameter)
        {
            try
            {
                Definition definition = parameter.Definition;
                if (definition == null) return false;

                ForgeTypeId type = definition.GetDataType();
                return type != null && type == SpecTypeId.Area;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static string FamilyAndType(Document document, Element element)
        {
            var instance = element as FamilyInstance;
            if (instance != null && instance.Symbol != null)
            {
                return (instance.Symbol.FamilyName ?? string.Empty) + " : " + (instance.Symbol.Name ?? string.Empty);
            }

            string typeName = ParameterReading.NameOf(document, element.GetTypeId());
            return typeName.Length == 0 ? element.Name ?? "(no name)" : typeName;
        }

        private static string PhaseName(Document document, ElementId phaseId, string none)
        {
            string name = ParameterReading.NameOf(document, phaseId);
            return name.Length == 0 ? none : name;
        }

        private static string WorksetName(WorksetTable table, Element element)
        {
            if (table == null) return "(not workshared)";

            try
            {
                Workset workset = table.GetWorkset(element.WorksetId);
                return workset == null ? "(no workset)" : workset.Name;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return "(no workset)";
            }
        }
    }
}
