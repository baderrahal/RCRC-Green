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

        public static SpeciesRow Species(string name, string group, int quantity)
        {
            return new SpeciesRow(name, group, quantity);
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
            PlotReading[] readings = null, string outputPath = null, PatchOutcome outcome = null)
        {
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
                KpiTemplates.Mosques,
                @"C:\templates\MOSQUES.xlsx",
                outputPath ?? @"C:\models\MOSQUES DM-12.xlsx",
                "PRX_Component",
                "PRX_Plot_UID2",
                "KING FAHD",
                held,
                Reconciliation.Of(held.Select(one => one.PlotId).ToList(), held, null, false),
                KpiCreatePlan.Of(KpiTemplates.Mosques, null, null, "KING FAHD", area, null, null,
                    null, "2026-09-09", "xx", "bb"),
                area,
                Totalled.Nothing,
                Totalled.Nothing,
                null,
                null,
                null,
                null,
                outcome);
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
            string[] notes = null)
        {
            RegionArea[] held = regions ?? new[] { Region(OutOfScope, 1000.0) };
            string chosen = chosenRegion ?? (held.Length > 0 ? held[0].TypeName : null);

            return new PlotReading(
                plotId,
                component,
                reference,
                softscapeRead,
                species ?? new SpeciesRow[0],
                shrubsAndLawnRead,
                subtotals ?? new GroupSubtotal[0],
                held,
                chosen,
                notes);
        }

        /// <summary>
        /// A softscape schedule as the model prints one, so the row reader is tested against
        /// the shape rather than against a list of species handed to it.
        /// </summary>
        public static ScannedSchedule Softscape(string plot, params string[][] rows)
        {
            return KpiFixture.Schedule(
                plot + "-(600) SOFTSCAPE SCHEDULE",
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
                plot + "-(600) SHRUBS AND LAWN SCHEDULE",
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
        /// It starts at row 4 with three empty rows under it that the total sums, which is the
        /// shape the real MOSQUES lists have: 80 names, then rows 84 to 92 empty, and B93
        /// summing B4 to B92.
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

            var free = new List<int>();
            for (int more = 0; more < emptyRows; more++) free.Add(at++);

            return SpeciesList.Holding(rows, free);
        }

        public static IReadOnlyList<string> Plots(params PlotReading[] readings)
        {
            return readings.Select(one => one.PlotId).ToList();
        }
    }
}
