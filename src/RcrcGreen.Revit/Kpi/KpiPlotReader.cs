using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// One plot parameter on a sheet and what it holds there, so the user picks the reference
    /// by looking at the value rather than at the name. Measured on sheet 010EA: PRX_Plot_ID
    /// FM-05, PRX_Plot_UID 120000247, PRX_Plot_UID2 ANH-007-MO-100019, PRX_Plot_NH
    /// GP.NH.Z2.052-DES042.
    /// </summary>
    internal sealed class PlotParameterValue
    {
        public PlotParameterValue(string name, string value)
        {
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
        }

        public string Name { get; }

        public string Value { get; }

        public string InWords
        {
            get { return Name + "   " + (Value.Length == 0 ? "(empty)" : Value); }
        }
    }

    /// <summary>
    /// Reads one plot at a time, in plain strings and numbers, and hands them to Core.
    ///
    /// Everything under a plot is per plot: 155 copies of each schedule name, each filtered
    /// PRX_Ref Plot ID Equal the plot, and the filled regions in the 00 link each carrying the
    /// same parameter. So every read here happens once per chosen plot and the merging happens
    /// afterwards in Core, never by reading across plots in one go.
    /// </summary>
    internal static class KpiPlotReader
    {
        /// <summary>
        /// The plots the model holds, from the two places that name one. The sheets and the
        /// schedules are kept apart rather than merged behind the user's back, because two
        /// records of one fact is the shape that has been the fault six times here.
        /// </summary>
        public static PlotsInTheModel Plots(Document document)
        {
            if (document == null) throw new ArgumentNullException("document");

            return PlotsInTheModel.Of(OnSheets(document), OnSchedules(document));
        }

        public static IReadOnlyList<string> OnSheets(Document document)
        {
            var found = new List<string>();

            foreach (ViewSheet sheet in Sheets(document))
            {
                int howMany;
                Parameter plot = ParameterReading.Named(sheet, KpiNames.PlotId, out howMany);
                if (!ParameterReading.HoldsAValue(plot)) continue;

                found.Add(ParameterReading.Printed(plot));
            }

            return found;
        }

        /// <summary>
        /// What each plot's first sheet holds for one parameter. Read in one pass over the
        /// sheets rather than once per plot, because this runs before anything is ticked.
        /// </summary>
        public static IDictionary<string, string> ValuePerPlot(Document document, string parameterName)
        {
            var found = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(parameterName)) return found;

            foreach (ViewSheet sheet in Sheets(document).OrderBy(one => one.SheetNumber, NaturalOrder.Comparer))
            {
                string plot = Held(sheet, KpiNames.PlotId);
                if (plot.Length == 0 || found.ContainsKey(plot)) continue;

                found[plot] = Held(sheet, parameterName);
            }

            return found;
        }

        public static IReadOnlyList<string> OnSchedules(Document document)
        {
            var found = new List<string>();

            foreach (ViewSchedule schedule in Schedules(document))
            {
                string plot = PlotFilteredOn(document, schedule);
                if (plot.Length > 0) found.Add(plot);
            }

            return found;
        }

        /// <summary>
        /// Every sheet parameter whose name holds COMPONENT, built from the model. PRX_COMPONENT
        /// does not exist here and PRX_Component does, so the list comes from the document and
        /// the user confirms which one it is.
        /// </summary>
        public static IReadOnlyList<string> ComponentNames(Document document)
        {
            var found = new List<string>();

            foreach (ViewSheet sheet in Sheets(document))
            {
                foreach (Parameter parameter in sheet.Parameters.Cast<Parameter>())
                {
                    string name = parameter.Definition == null ? string.Empty : parameter.Definition.Name;
                    if (!KpiNames.HoldsAny(name, KpiNames.ComponentNearMisses)) continue;
                    if (found.Contains(name, StringComparer.Ordinal)) continue;

                    found.Add(name);
                }
            }

            return found.OrderBy(one => one, NaturalOrder.Comparer).ToList();
        }

        /// <summary>
        /// The four plot parameters and what each holds, for EVERY plot, off that plot's first
        /// sheet. One pass over the sheets rather than one per plot, the same shape
        /// <see cref="ValuePerPlot"/> uses, because this runs before anything is ticked.
        ///
        /// **It used to read one plot, the first in the model's list.** The pane then showed
        /// DM-11's four values with DM-12 ticked, and the block exists so a person picks the
        /// reference by looking at its value. Reading every plot is what lets the pane show the
        /// ticked one without asking Revit again.
        /// </summary>
        public static IDictionary<string, IReadOnlyList<PlotParameterValue>> ReferenceValuesPerPlot(
            Document document)
        {
            var found = new Dictionary<string, IReadOnlyList<PlotParameterValue>>(StringComparer.Ordinal);

            foreach (ViewSheet sheet in Sheets(document).OrderBy(one => one.SheetNumber, NaturalOrder.Comparer))
            {
                string plot = Held(sheet, KpiNames.PlotId);
                if (plot.Length == 0 || found.ContainsKey(plot)) continue;

                found[plot] = KpiNames.PlotNamesOnSheets
                    .Select(name => new PlotParameterValue(name, Held(sheet, name)))
                    .ToList();
            }

            return found;
        }

        /// <summary>
        /// The neighbourhood off Project Information, spelt the American way without a u. One
        /// read with no plot involved, because the workbook's location does not vary by plot.
        /// </summary>
        public static string Location(Document document, string parameterName)
        {
            Element information = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_ProjectInformation)
                .FirstOrDefault();

            if (information == null) return string.Empty;

            return Held(information, parameterName);
        }

        public static IReadOnlyList<string> LocationNames(Document document)
        {
            Element information = new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_ProjectInformation)
                .FirstOrDefault();

            if (information == null) return new List<string>();

            return information.Parameters.Cast<Parameter>()
                .Select(one => one.Definition == null ? string.Empty : one.Definition.Name)
                .Where(name => KpiNames.HoldsAny(name, KpiNames.NeighbourhoodNearMisses))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(one => one, NaturalOrder.Comparer)
                .ToList();
        }

        /// <summary>
        /// Every filled region in the 00 link carrying this plot, with the area on each. Which
        /// of a plot's two regions holds the area varies by plot, so both are offered and the
        /// type name decides nothing.
        /// </summary>
        public static IReadOnlyList<RegionArea> RegionsFor(Document document, string plotId)
        {
            var found = new List<RegionArea>();

            foreach (Document linked in LinkedDocuments(document))
            {
                foreach (FilledRegion region in new FilteredElementCollector(linked)
                    .OfClass(typeof(FilledRegion))
                    .Cast<FilledRegion>())
                {
                    int howMany;
                    Parameter plot = ParameterReading.Named(region, KpiNames.RefPlotId, out howMany);
                    if (!ParameterReading.HoldsAValue(plot)) continue;
                    if (!string.Equals(ParameterReading.Printed(plot), plotId, StringComparison.Ordinal)) continue;

                    int alsoNamed;
                    Parameter area = ParameterReading.Named(region, KpiNames.InterventionArea, out alsoNamed);
                    double raw = RawOf(area);

                    found.Add(new RegionArea(
                        ParameterReading.NameOf(linked, region.GetTypeId()),
                        raw,
                        AreaUnits.SquareMetresFromSquareFeet(raw),
                        ParameterReading.HoldsAValue(area)
                            ? ParameterReading.Printed(area)
                            : LinkContents.NoAreaValue));
                }
            }

            return found;
        }

        /// <summary>
        /// One plot's whole contribution. The schedules are found by the plot their filter
        /// names rather than by the plot in their own name, because the filter is what really
        /// ties a schedule to a plot on this model. A name that disagrees with the filter is
        /// noted rather than resolved.
        /// </summary>
        public static PlotReading Read(
            Document document,
            string plotId,
            string componentParameter,
            string referenceParameter,
            CountedGroups counted,
            string chosenRegionTypeName,
            IReadOnlyList<RegionArea> regions,
            double regionSeconds)
        {
            if (document == null) throw new ArgumentNullException("document");
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (counted == null) throw new ArgumentNullException("counted");

            // The regions were read before this was called, so their cost is handed in and added
            // rather than left out of the plot's own number.
            var clock = System.Diagnostics.Stopwatch.StartNew();

            var notes = new List<string>();
            ViewSheet sheet = FirstSheetOf(document, plotId);
            if (sheet == null) notes.Add("No sheet carries " + KpiNames.PlotId + " " + plotId + ".");

            var species = new List<SpeciesRow>();
            var subtotals = new List<GroupSubtotal>();
            var refusals = new List<string>();
            var printed = new List<ScannedSchedule>();
            var groups = new List<PrintedGroup>();
            bool totalRead = false;
            int total = 0;
            int totalRow = 0;
            int passedOver = 0;

            // Every schedule of each kind is found first and counted. One is read. Two or more
            // are named and none of them is read, because nothing could say which of two is the
            // real one. No plot has been measured holding two: FM-05 holds one softscape
            // schedule, and the double it printed was a third group, Street Design, printed
            // after Proposed's own subtotal row. That diagnosis of two schedules was wrong and
            // the guard stands for the case it was built for.
            var softscape = new List<ViewSchedule>();
            var ground = new List<ViewSchedule>();

            foreach (ViewSchedule schedule in Schedules(document))
            {
                if (!string.Equals(PlotFilteredOn(document, schedule), plotId, StringComparison.Ordinal)) continue;

                ParsedViewName parsed;
                if (ViewNameParser.TryParse(schedule.Name, out parsed)
                    && !string.Equals(parsed.PlotId, plotId, StringComparison.Ordinal))
                {
                    notes.Add(schedule.Name + " filters on " + plotId + " and is named for "
                        + parsed.PlotId + ". Both are recorded and neither is resolved.");
                }

                if (KpiNames.HoldsAny(schedule.Name, KpiNames.SoftscapeWords))
                {
                    softscape.Add(schedule);
                    continue;
                }

                if (KpiNames.HoldsAny(schedule.Name, "SHRUB", "LAWN"))
                {
                    ground.Add(schedule);
                }
            }

            // A refused read travels with the schedule's name and refuses the write. It used
            // to be a list of nothing, which the reconciliation read as a plot whose schedule
            // listed no species.
            // The rows as printed travel with the reading, so the report can show what was read
            // and not only what was made of it.
            if (softscape.Count == 1)
            {
                ViewSchedule schedule = softscape[0];
                ScannedSchedule rows = Printed(schedule);
                printed.Add(rows);
                SoftscapeReading trees = SoftscapeRows.Read(rows, counted, plotId);
                species.AddRange(trees.Species);
                groups.AddRange(trees.Groups);
                refusals.AddRange(trees.Refusals.Select(why => schedule.Name + ": " + why));
                if (trees.TotalRead)
                {
                    totalRead = true;
                    total = trees.Total;
                    totalRow = trees.TotalRow;
                }

                passedOver = trees.RowsPassedOver;
            }

            if (ground.Count == 1)
            {
                ViewSchedule schedule = ground[0];
                ScannedSchedule rows = Printed(schedule);
                printed.Add(rows);
                ShrubsAndLawnReading read = ShrubsAndLawnRows.Read(
                    rows, new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }, counted);
                subtotals.AddRange(read.Subtotals);
                refusals.AddRange(read.Refusals.Select(why => schedule.Name + ": " + why));
            }

            return new PlotReading(
                plotId,
                sheet == null ? string.Empty : Held(sheet, componentParameter),
                sheet == null ? string.Empty : Held(sheet, referenceParameter),
                softscape.Select(one => one.Name),
                species,
                ground.Select(one => one.Name),
                subtotals,
                regions,
                regionSeconds + clock.Elapsed.TotalSeconds,
                chosenRegionTypeName,
                notes,
                refusals,
                totalRead,
                total,
                passedOver,
                printed,
                totalRow,
                groups);
        }

        /// <summary>
        /// The rows exactly as the schedule prints them, wrapped in the plain Core type so the
        /// row readers that turn them into species and subtotals are the tested ones rather
        /// than a second copy written here.
        /// </summary>
        private static ScannedSchedule Printed(ViewSchedule schedule)
        {
            var rows = new List<IReadOnlyList<string>>();
            int bodyRows = 0;

            try
            {
                TableData table = schedule.GetTableData();
                TableSectionData body = table.GetSectionData(SectionType.Body);
                bodyRows = body.NumberOfRows;
                int columns = body.NumberOfColumns;

                for (int row = 0; row < bodyRows; row++)
                {
                    var cells = new List<string>(columns);
                    for (int column = 0; column < columns; column++)
                    {
                        cells.Add(schedule.GetCellText(SectionType.Body, row, column) ?? string.Empty);
                    }

                    rows.Add(cells);
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return new ScannedSchedule(schedule.Name, string.Empty, false, null, null,
                    string.Empty, string.Empty, true, false, 0, null);
            }

            return new ScannedSchedule(schedule.Name, string.Empty, false, null, null,
                string.Empty, string.Empty, true, true, bodyRows, rows);
        }

        private static string PlotFilteredOn(Document document, ViewSchedule schedule)
        {
            try
            {
                Autodesk.Revit.DB.ScheduleDefinition definition = schedule.Definition;
                if (definition == null) return string.Empty;

                foreach (ScheduleFilter filter in definition.GetFilters())
                {
                    ScheduleField field = definition.GetField(filter.FieldId);
                    if (field == null) continue;
                    if (!string.Equals(field.GetName(), KpiNames.RefPlotId, StringComparison.Ordinal)) continue;
                    if (!filter.IsStringValue) continue;

                    return filter.GetStringValue() ?? string.Empty;
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                return string.Empty;
            }
            catch (InvalidOperationException)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        private static ViewSheet FirstSheetOf(Document document, string plotId)
        {
            foreach (ViewSheet sheet in Sheets(document).OrderBy(one => one.SheetNumber, NaturalOrder.Comparer))
            {
                if (string.Equals(Held(sheet, KpiNames.PlotId), plotId, StringComparison.Ordinal)) return sheet;
            }

            return null;
        }

        private static IEnumerable<ViewSheet> Sheets(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewSheet))
                .Cast<ViewSheet>();
        }

        private static IEnumerable<ViewSchedule> Schedules(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .Where(one => !one.IsTemplate);
        }

        private static IEnumerable<Document> LinkedDocuments(Document document)
        {
            var seen = new List<string>();

            foreach (RevitLinkInstance instance in new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>())
            {
                Document linked = instance.GetLinkDocument();
                if (linked == null) continue;
                if (!KpiNames.Holds(instance.Name, KpiNames.LinkMark)) continue;
                if (seen.Contains(linked.Title, StringComparer.Ordinal)) continue;

                seen.Add(linked.Title);
                yield return linked;
            }
        }

        private static double RawOf(Parameter parameter)
        {
            if (parameter == null || !parameter.HasValue) return 0.0;
            if (parameter.StorageType != StorageType.Double) return 0.0;

            return parameter.AsDouble();
        }

        private static string Held(Element element, string name)
        {
            if (element == null || string.IsNullOrEmpty(name)) return string.Empty;

            int howMany;
            Parameter parameter = ParameterReading.Named(element, name, out howMany);
            return ParameterReading.HoldsAValue(parameter) ? ParameterReading.Printed(parameter) : string.Empty;
        }
    }
}
