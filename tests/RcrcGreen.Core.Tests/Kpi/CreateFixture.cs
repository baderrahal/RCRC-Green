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
        /// <summary>
        /// What a test that is not a street run hands the plan. It is a REQUIRED argument, so
        /// every one of these call sites had to say something, which is the point: the three
        /// the team types defaulted to null once and the handler quietly never passed them.
        /// </summary>
        public static readonly StreetReferenceAnswer NoStreetFile =
            StreetReferenceAnswer.Nothing("this run reads no street reference file");

        /// <summary>
        /// What a test hands the plan when the template was never opened. Required, like the
        /// street answer and the three the team types, so no caller can drop it in silence.
        /// </summary>
        public static readonly LabelledCells NoLabels = LabelledCells.NotRead;

        /// <summary>
        /// **Row 5 as Bader measured it on all seven templates on 14 September**, which is what
        /// a real press finds: REF : at B5, Date: at D5 and Prepared By: at F5, reaching C5, E5,
        /// G5 and H5. The value cells are empty here, because what a template holds in them is
        /// the one thing that differs between the two measured sets and a fixture that picked
        /// one would be asserting a set rather than a layout.
        ///
        /// **Row 7 is not in here.** It differs across the three templates it has been measured
        /// on, and the two park templates have never been looked at, so a test that wants it
        /// names it itself.
        /// </summary>
        public static LabelledCells RowFive(string mainSheetName)
        {
            return LabelledCells.Holding(
                mainSheetName,
                new[]
                {
                    LabelledCell.At(LabelledPlaces.ReferenceName, LabelledPlaces.ReferenceLabel, "B5", "C5", string.Empty),
                    LabelledCell.At(LabelledPlaces.DateName, LabelledPlaces.DateLabel, "D5", "E5", string.Empty),
                    LabelledCell.At(LabelledPlaces.PreparedByName, LabelledPlaces.PreparedByLabel, "F5", "G5", string.Empty),
                    LabelledCell.At(LabelledPlaces.PositionName, LabelledPlaces.PreparedByLabel, "F5", "H5", string.Empty)
                });
        }

        public const string Existing = "Existing";

        public const string Proposed = "Proposed";

        public const string Cadastral = "RCRC_CADASTRAL LIMIT";

        public const string OutOfScope = "RCRC_OUT OF SCOPE (PRESENTATION)";

        /// <summary>
        /// A type no model has shown, for the pair the client's note does NOT settle.
        ///
        /// **Since Bader's decision of 14 September a pair holding the note's type chooses
        /// itself**, so a test that wants the old refusal has to build a pair without it. This
        /// is not a name from a model and is not a rule, it is a stand in for whatever a future
        /// model calls its other regions.
        /// </summary>
        public const string NotTheNote = "RCRC_SOMETHING NOBODY HAS MEASURED";

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
        /// **A RUN THAT REALLY WROTE.** Every other run this fixture builds carries a refused
        /// outcome or none, so <see cref="PatchOutcome.Done"/> was built in no test file but the
        /// patcher's own and the section whose heading says every cell was read back off the
        /// output was the one section no test reached.
        ///
        /// It patches a workbook for real rather than hand building an outcome, so what the
        /// report prints is what a run would put there. The caller owns the folder and deletes
        /// it.
        /// </summary>
        public static KpiCreateRun RunThatWrote(string folder, params CellWrite[] writes)
        {
            if (folder == null) throw new System.ArgumentNullException("folder");

            CellWrite[] held = writes != null && writes.Length > 0
                ? writes
                : new[]
                {
                    CellWrite.Text(WorkbookFixture.MainSheet, "D3", "FRIDAY MOSQUE"),
                    CellWrite.Number(WorkbookFixture.MainSheet, "D8", 3728.757),
                    CellWrite.Number(WorkbookFixture.TreesSheet, "B4", 31)
                };

            string template = WorkbookFixture.Create(folder, "template.xlsx");
            string output = System.IO.Path.Combine(folder, "ANH-008-MO-100006.xlsx");

            PatchOutcome outcome = WorkbookPatcher.Patch(template, output, held);
            if (!outcome.Written)
            {
                throw new System.InvalidOperationException(
                    "the fixture's own patch was refused: " + outcome.Refusal);
            }

            return Run(outcome: outcome);
        }

        /// <summary>
        /// One press of Create as the handler would hand it over, with the DM-12 numbers the
        /// first real run measured. Enough of it to write the report against.
        /// </summary>
        public static KpiCreateRun Run(
            PlotReading[] readings = null,
            string outputRoot = null,
            PatchOutcome outcome = null,
            RunTiming timing = null,
            KpiTemplate template = null,
            SpeciesMatch[] matches = null,
            SpeciesList existingList = null,
            SpeciesList proposedList = null,
            ReadingsSource readingsSource = null,
            TemplateListing templatesListed = null,
            string[] ticked = null,
            ProjectUnit areaUnit = null,
            LinksLoaded links = null,
            LabelledCells labels = null)
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
                PlotWorkbookPath.For(
                    outputRoot ?? @"C:\models", KpiTemplates.Mosques, "FRIDAY MOSQUE", "ANH-008-MO-100006"),
                "PRX_Component",
                "PRX_Plot_UID2",
                "KING FAHD",
                held,
                Reconciliation.Of(ticked ?? held.Select(one => one.PlotId).ToArray(), held, null, false, which),
                KpiCreatePlan.Of(which, null, null, "KING FAHD", area, null, null,
                    matches, "2026-09-09", "xx", "bb", CreateFixture.NoStreetFile,
                    labels ?? RowFive(which.MainSheetName)),
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
                areaUnit,
                links);
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
            PrintedGroup[] printedGroups = null,
            string uid2 = null)
        {
            RegionArea[] held = regions ?? new[] { Region(OutOfScope, 1000.0) };

            // THE SAME RULE THE HANDLER RUNS, called rather than written out again. This used
            // to take held[0] whatever it held and however many there were, which is not what
            // the running tool does, so every test built on it was built on a choice that
            // could not happen.
            RegionPick chosen = RegionChoice.Pick(held, chosenRegion);

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
                printedGroups,
                uid2);
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
