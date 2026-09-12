using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Builds the plain values the Revit side would hand over for one plot, so a test names
    /// only the part it cares about. Every expected value in the tests is written out by hand
    /// rather than worked out with the same rule the code uses.
    /// </summary>
    internal static class CreateFixture
    {
        public const string Existing = "Existing";

        public const string Proposed = "Proposed";

        public const string Cadastral = "RCRC_CADASTRAL LIMIT";

        public const string OutOfScope = "RCRC_OUT OF SCOPE (PRESENTATION)";

        /// <summary>
        /// The groups a MOSQUES checklist counts, which are the two its tree list sheets are
        /// named for. A group the workbook has no sheet for, Street Design on FM-05, is left out.
        /// </summary>
        public static readonly CountedGroups Counted = CountedGroups.Of(KpiTemplates.Mosques);

        /// <summary>
        /// One merged species off one plot, sized, for a test about placement rather than
        /// about measures. Built from a row rather than from a plot number, because a species
        /// built from numbers alone carries no measures and is no longer written into an empty
        /// row at all.
        /// </summary>
        public static MergedSpecies Merged(string name, string group, string plotId, int quantity)
        {
            return MergedSpecies.FromRows(name, group, new[] { Species(name, group, quantity).OnPlot(plotId) });
        }

        /// <summary>
        /// A species the model sizes, which is the ordinary case. The height and the canopy
        /// diameter are here because a species carrying neither is no longer written into an
        /// empty row at all, so a fixture without them would test the refusal rather than
        /// whatever the test is about. The unsized case is built by naming the two measures.
        /// </summary>
        public static SpeciesRow Species(string name, string group, int quantity)
        {
            return new SpeciesRow(name, group, quantity, null, 0, PrintedMeasure.Of("15"), PrintedMeasure.Of("8"));
        }

        /// <summary>
        /// A species row with the row it printed on and the height and diameter beside it, as
        /// the softscape schedule prints them. A nought or a dash is what UNKNOWN prints.
        /// </summary>
        public static SpeciesRow Species(string name, string group, int quantity, int row, string height, string diameter)
        {
            return new SpeciesRow(name, group, quantity, null, row, PrintedMeasure.Of(height), PrintedMeasure.Of(diameter));
        }

        public static GroupSubtotal Subtotal(string heading, double squareMetres, int items)
        {
            return new GroupSubtotal(heading, squareMetres, items);
        }

        public static RegionArea Region(string typeName, double squareMetres, double rawSquareFeet = 0.0)
        {
            return new RegionArea(
                typeName,
                rawSquareFeet == 0.0 ? squareMetres * 10.763910416709722 : rawSquareFeet,
                squareMetres,
                squareMetres.ToString("0.##") + " m2");
        }

        /// <summary>
        /// One press of Create as the handler would hand it over, with the DM-12 numbers the
        /// first real run measured. Enough of it to write the report against.
        /// </summary>
        public static KpiCreateRun Run(
            PlotReading[] readings = null,
            string outputPath = null,
            PatchOutcome outcome = null,
            RunTiming timing = null,
            KpiTemplate template = null,
            SpeciesMatch[] matches = null,
            SpeciesList existingList = null,
            SpeciesList proposedList = null,
            ReadingsSource readingsSource = null,
            TemplateListing templatesListed = null,
            string[] ticked = null,
            ProjectUnit areaUnit = null)
        {
            KpiTemplate which = template ?? KpiTemplates.Mosques;

            IReadOnlyList<PlotReading> held = readings ?? new[]
            {
                Plot("DM-12", regions: new[] { Region(OutOfScope, 3728.7570000000005, 40136.006313679296) })
            };

            var area = Totalled.Adding(held
                .Select(one => new PlotNumber(one.PlotId,
                    one.ChosenRegion == null ? 0.0 : one.ChosenRegion.SquareMetres))
                .ToList());

            return new KpiCreateRun(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                which,
                @"C:\templates\MOSQUES.xlsx",
                outputPath ?? @"C:\models\MOSQUES DM-12.xlsx",
                "PRX_Component",
                "PRX_Plot_UID2",
                "KING FAHD",
                held,
                Reconciliation.Of(ticked ?? held.Select(one => one.PlotId).ToArray(), held, null, false, which),
                KpiCreatePlan.Of(which, null, null, "KING FAHD", area, null, null,
                    matches, "2026-09-09", "xx", "bb"),
                area,
                Totalled.Nothing,
                Totalled.Nothing,
                null,
                null,
                null,
                null,
                outcome,
                timing ?? RunTiming.NotTimed,
                existingList,
                proposedList,
                readingsSource,
                templatesListed,
                areaUnit);
        }

        public static PlotReading Plot(
            string plotId,
            string component = "FRIDAY MOSQUE",
            string reference = "ANH-007-MO-100019",
            SpeciesRow[] species = null,
            GroupSubtotal[] subtotals = null,
            RegionArea[] regions = null,
            string chosenRegion = null,
            bool softscapeRead = true,
            bool shrubsAndLawnRead = true,
            string[] notes = null,
            double readSeconds = 0.0,
            string[] readRefusals = null,
            bool softscapeTotalRead = false,
            int softscapeTotal = 0,
            int rowsPassedOver = 0,
            string[] softscapeSchedules = null,
            string[] shrubsAndLawnSchedules = null,
            ScannedSchedule[] printedSchedules = null,
            int softscapeTotalRow = 0,
            PrintedGroup[] printedGroups = null)
        {
            RegionArea[] held = regions ?? new[] { Region(OutOfScope, 1000.0) };
            string chosen = chosenRegion ?? (held.Length > 0 ? held[0].TypeName : null);

            // A plot read has one schedule of each kind unless a test names otherwise, and the
            // names are the model's own shape.
            return new PlotReading(
                plotId,
                component,
                reference,
                softscapeSchedules ?? (softscapeRead ? new[] { SoftscapeName(plotId) } : new string[0]),
                species ?? new SpeciesRow[0],
                shrubsAndLawnSchedules ?? (shrubsAndLawnRead ? new[] { ShrubsAndLawnName(plotId) } : new string[0]),
                subtotals ?? new GroupSubtotal[0],
                held,
                readSeconds,
                chosen,
                notes,
                readRefusals,
                softscapeTotalRead,
                softscapeTotal,
                rowsPassedOver,
                printedSchedules,
                softscapeTotalRow,
                printedGroups);
        }

        public static string SoftscapeName(string plot)
        {
            return plot + "-(600) SOFTSCAPE SCHEDULE";
        }

        public static string ShrubsAndLawnName(string plot)
        {
            return plot + "-(600) SHRUBS AND LAWN SCHEDULE";
        }

        /// <summary>
        /// A softscape schedule as the model prints one, so the row reader is tested against
        /// the shape rather than against a list of species handed to it.
        /// </summary>
        public static ScannedSchedule Softscape(string plot, params string[][] rows)
        {
            return KpiFixture.Schedule(
                SoftscapeName(plot),
                fields: new[]
                {
                    KpiFixture.Field("BOTANICAL NAME"),
                    KpiFixture.Field("COUNT (n)", "Count", "Count")
                },
                rowsWereRead: true,
                bodyRowCount: rows.Length,
                rows: rows);
        }

        public static ScannedSchedule ShrubsAndLawn(string plot, params string[][] rows)
        {
            return KpiFixture.Schedule(
                ShrubsAndLawnName(plot),
                categoryName: "Floors",
                fields: new[] { KpiFixture.Field("TYPE"), KpiFixture.Field("AREA") },
                rowsWereRead: true,
                bodyRowCount: rows.Length,
                rows: rows);
        }

        /// <summary>
        /// The workbook's own species list, as the four measured hard cases have it. The model
        /// prints UNKNOWN and the workbook holds four rows named Unknown Tree, which is why
        /// nothing can match those on name.
        ///
        /// It starts at row 4 with three empty rows under it that the total reaches, which is
        /// the shape the first MOSQUES list had: 80 names, then rows 84 to 92 empty, and B93
        /// summing B4 to B92. The empty rows are not handed in. They follow from the names and
        /// the total's range, the way they do off a file.
        /// </summary>
        public static SpeciesList WorkbookList(params string[] names)
        {
            return WorkbookListWithRoomFor(3, names);
        }

        public static SpeciesList WorkbookListWithRoomFor(int emptyRows, params string[] names)
        {
            var rows = new List<SpeciesListRow>();
            int at = 4;
            foreach (string name in names) rows.Add(new SpeciesListRow(at++, name));

            int last = at + emptyRows - 1;
            return SpeciesList.Holding(rows, 4, last, KpiTemplates.QuantityColumn + (last + 1));
        }

        public static IReadOnlyList<string> Plots(params PlotReading[] readings)
        {
            return readings.Select(one => one.PlotId).ToList();
        }
    }
}
