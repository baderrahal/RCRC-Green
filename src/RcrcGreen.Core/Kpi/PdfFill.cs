using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One field of one plot's PDF: the value to write, or the reason nothing is written.
    /// </summary>
    public sealed class PdfFieldFill
    {
        private PdfFieldFill(PdfValue value, string fieldName, string text, string why, string unit, string working)
        {
            Value = value;
            FieldName = fieldName ?? string.Empty;
            Text = text ?? string.Empty;
            Why = why ?? string.Empty;
            Unit = unit ?? string.Empty;
            Working = working ?? string.Empty;
        }

        public static PdfFieldFill Writing(PdfValue value, string fieldName, string text, string unit = null, string working = null)
        {
            return new PdfFieldFill(value, fieldName, text, string.Empty, unit, working);
        }

        public static PdfFieldFill Blank(PdfValue value, string fieldName, string why)
        {
            return new PdfFieldFill(value, fieldName, string.Empty, why, null, null);
        }

        /// <summary>
        /// The unit the value is written IN, printed beside it in the report. **A number in the
        /// wrong unit is plausible and a number with its unit beside it is checkable.**
        /// </summary>
        public string Unit { get; }

        /// <summary>
        /// How a computed value was worked out, empty for one that was read. **The first numbers
        /// this tool produces that no schedule printed carry their working**, so a person can
        /// check them against the workbook once Excel has opened it.
        /// </summary>
        public string Working { get; }

        public bool Computed
        {
            get { return Working.Length > 0; }
        }

        public PdfValue Value { get; }

        public string FieldName { get; }

        public string Text { get; }

        /// <summary>Empty on a field this run writes. Never empty on one it leaves.</summary>
        public string Why { get; }

        public bool Written
        {
            get { return Why.Length == 0; }
        }
    }

    /// <summary>
    /// What one plot's PDF gets, worked out before a byte is written.
    ///
    /// **THE EXCEL IS WRITTEN FIRST AND THE PDF SECOND.** Two fields read a cell out of the
    /// workbook this run has just written, so the ordering is a rule rather than an accident,
    /// and it is why this takes the workbook's own path rather than its plan.
    /// </summary>
    public sealed class PdfPlan
    {
        public PdfPlan(
            string plotId, PdfForm form, IEnumerable<PdfFieldFill> fields, string why,
            IEnumerable<string> whatWasChecked = null)
        {
            PlotId = plotId ?? string.Empty;
            Form = form;
            Fields = (fields ?? Enumerable.Empty<PdfFieldFill>()).ToList();
            Why = why ?? string.Empty;
            WhatWasChecked = (whatWasChecked ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>
        /// What the two computed numbers were held against in the workbook, one line each, so the
        /// report says which cell was read and what its formula says rather than leaving a
        /// computed number standing on nothing.
        /// </summary>
        public IReadOnlyList<string> WhatWasChecked { get; }

        public static PdfPlan NoForm(string plotId, string why)
        {
            return new PdfPlan(plotId, null, null, why);
        }

        public string PlotId { get; }

        /// <summary>Null where no form is named for the plot's prefix.</summary>
        public PdfForm Form { get; }

        public IReadOnlyList<PdfFieldFill> Fields { get; }

        /// <summary>Empty where a PDF is planned. Never empty where none is.</summary>
        public string Why { get; }

        public bool Wanted
        {
            get { return Form != null && Why.Length == 0; }
        }

        public IEnumerable<PdfFieldFill> Writing
        {
            get { return Fields.Where(one => one.Written); }
        }

        public IEnumerable<PdfFieldFill> Blank
        {
            get { return Fields.Where(one => !one.Written); }
        }
    }

    /// <summary>
    /// The numbers the workbook this run just wrote is holding, handed to the PDF rather than
    /// worked out a second time.
    ///
    /// **TWO OF THE FORM'S FIELDS ARE COMPUTED FROM THEM.** The workbook computes a total green
    /// cover and a canopy percentage of its own, and the patcher drops every cached formula
    /// result on purpose so Excel recalculates, so neither number is in the file the run wrote.
    /// Opening a hundred and fifty workbooks by hand is not a workflow and relaxing the cache
    /// rule brings the stale zeros back, so the tool works both out from what it itself wrote
    /// and read, and **shows its working**.
    /// </summary>
    public sealed class PdfWorkbookNumbers
    {
        public PdfWorkbookNumbers(
            CanopyTotal canopy,
            double plantingSquareMetres,
            double lawnSquareMetres,
            double areaSquareMetres,
            ArithmeticCheck arithmetic,
            SummaryCellCheck greenCoverCell = null,
            SummaryCellCheck percentageCell = null)
        {
            Canopy = canopy ?? CanopyTotal.Nothing;
            PlantingSquareMetres = plantingSquareMetres;
            LawnSquareMetres = lawnSquareMetres;
            AreaSquareMetres = areaSquareMetres;
            Arithmetic = arithmetic ?? ArithmeticCheck.NotChecked(WorkbookArithmetic.NoWorkbookRead);
            GreenCoverCell = greenCoverCell ?? SummaryCellCheck.NotChecked(
                ComputedPlaces.GreenCoverName, WorkbookArithmetic.NoWorkbookRead);
            PercentageCell = percentageCell ?? SummaryCellCheck.NotChecked(
                ComputedPlaces.PercentageName, WorkbookArithmetic.NoWorkbookRead);
        }

        /// <summary>Nothing was written, so there is nothing to compute from.</summary>
        public static readonly PdfWorkbookNumbers None = new PdfWorkbookNumbers(
            null, 0.0, 0.0, 0.0, null);

        public CanopyTotal Canopy { get; }

        /// <summary>What went into the workbook's planting cell, in square metres.</summary>
        public double PlantingSquareMetres { get; }

        /// <summary>What went into the workbook's lawn cell, in square metres.</summary>
        public double LawnSquareMetres { get; }

        /// <summary>What went into the workbook's area cell, in square metres.</summary>
        public double AreaSquareMetres { get; }

        /// <summary>
        /// Whether the workbook's own canopy column is still the one this tool works out.
        /// **A column that has drifted blanks both computed fields**, because both rest on it.
        /// </summary>
        public ArithmeticCheck Arithmetic { get; }

        /// <summary>
        /// Whether the workbook's own Total Green cover cell is still the sum this tool works
        /// out, found by its label and read off the file. **A cell that has drifted blanks the
        /// field**, because the number the PDF carries would then not be the number the workbook
        /// beside it computes.
        /// </summary>
        public SummaryCellCheck GreenCoverCell { get; }

        /// <summary>
        /// The same for the canopy percentage cell. **The two parks templates carry no such cell
        /// and the Parks PDF is the one form that asks for it**, so this answers nothing to check
        /// on every run today, the number is still written, and the report says so.
        /// </summary>
        public SummaryCellCheck PercentageCell { get; }
    }

    public static class PdfFill
    {
        public const string TheWorkbookWasNotWritten =
            "this plot's workbook was not written, so there is nothing to compute it from";

        /// <summary>
        /// **No schedule prints ground cover apart from shrubs.** The group the shrubs and lawn
        /// schedule prints is SHRUBS & GROUND COVER, one heading over one set of rows, measured
        /// on every scan this project has taken. Nothing is derived from it: splitting one
        /// printed number into two would be a number nobody measured.
        /// </summary>
        public const string GroundCoverIsNotPrintedApart =
            "the schedule prints SHRUBS & GROUND COVER as one group and nothing prints ground "
            + "cover on its own, so it is not derived";

        /// <summary>
        /// **The tool has never read a water demand column off any schedule.** The shrubs and
        /// lawn schedule prints L/DAY as its last column and nothing reads it, so there is no
        /// number to write. Adding that read is a round of its own.
        /// </summary>
        public const string WaterDemandIsNotReadYet =
            "no water demand is read off any schedule yet, so there is nothing to write";

        public const string NoRegionChosen =
            "no filled region was chosen for this plot, so it has no intervention area";

        public const string NotAStreetPlot = "this form takes no road width or length";

        /// <summary>
        /// **The Roads form's note names a DIFFERENT CELL from the other two forms' for a field
        /// all three call Total areas to be greened.** Parks and open spaces name the cell beside
        /// Total Green cover and roads names the one beside Total area covered by canopy. Each
        /// form is filled from its OWN note, which is what the file says, and the report names
        /// which of the two the number is so a person reading three forms side by side is not
        /// left to guess. Which the client means is an open question in `steps/log-kpi.md`.
        /// </summary>
        public const string RoadsNamesTheCanopyCell =
            "this form's note names the canopy cell where the other two name Total Green cover, "
            + "so what is written here is the canopy";

        /// <summary>
        /// One plot's plan. Every value comes off the reading, the street reference answer and
        /// the workbook that was already written, and nothing is worked out twice.
        ///
        /// **The workbook numbers are a REQUIRED argument.** A default that reads as a
        /// deliberate empty is how a whole link in a chain goes missing without a word, which
        /// this tool has already paid for once with the three the team types.
        /// </summary>
        public static PdfPlan Of(
            PlotReading reading,
            CountedGroups counted,
            StreetReferenceAnswer street,
            DateTime today,
            bool workbookWritten,
            PdfWorkbookNumbers workbook)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            if (counted == null) throw new ArgumentNullException("counted");
            if (workbook == null) throw new ArgumentNullException("workbook");

            PdfForm form = PdfForms.ForPlot(reading.PlotId);
            if (form == null) return PdfPlan.NoForm(reading.PlotId, PdfForms.NoFormFor(reading.PlotId));

            PhaseSplit shrubs = ShrubsByPhase.Of(reading, KpiMerge.ShrubsHeading, counted);
            GroupSubtotal lawn = reading.SubtotalHeaded(KpiMerge.LawnHeading);

            var fields = new List<PdfFieldFill>();

            foreach (PdfFormField wanted in form.Fields)
            {
                fields.Add(One(wanted, form, reading, counted, street, today, workbookWritten, workbook, shrubs, lawn));
            }

            return new PdfPlan(reading.PlotId, form, fields, string.Empty, new[]
            {
                workbook.GreenCoverCell.InWords,
                workbook.PercentageCell.InWords
            });
        }

        private static PdfFieldFill One(
            PdfFormField wanted,
            PdfForm form,
            PlotReading reading,
            CountedGroups counted,
            StreetReferenceAnswer street,
            DateTime today,
            bool workbookWritten,
            PdfWorkbookNumbers workbook,
            PhaseSplit shrubs,
            GroupSubtotal lawn)
        {
            string name = wanted.FieldName;
            string unit = wanted.Unit;

            switch (wanted.Value)
            {
                case PdfValue.ProjectType:
                    // **EXACTLY AS REVIT HOLDS IT, capitals and all.** Bader, 14 September.
                    return reading.Component.Length == 0
                        ? PdfFieldFill.Blank(wanted.Value, name, "this plot's sheet carries no " + KpiNames.Component)
                        : PdfFieldFill.Writing(wanted.Value, name, reading.Component, unit);

                case PdfValue.Uid:
                    return reading.Uid2.Length == 0
                        ? PdfFieldFill.Blank(wanted.Value, name, "this plot's sheet carries no " + KpiNames.PlotUid2)
                        : PdfFieldFill.Writing(wanted.Value, name, reading.Uid2, unit);

                case PdfValue.ReportDate:
                    // Day, month, year. Bader, 14 September.
                    return PdfFieldFill.Writing(wanted.Value, name,
                        today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), unit);

                case PdfValue.Area:
                    // The form asks m2 and the reading is already m2, converted once by
                    // AreaUnits at the Revit boundary. No conversion here, checked rather than
                    // assumed.
                    return reading.ChosenRegion == null
                        ? PdfFieldFill.Blank(wanted.Value, name, NoRegionChosen)
                        : PdfFieldFill.Writing(wanted.Value, name, Number(reading.ChosenRegion.SquareMetres), unit);

                case PdfValue.Row:
                    // The form's unit column prints m beside this box and the reference file's
                    // QUANTITY UNIT reads m. Same unit, no conversion.
                    return street == null || !street.Found
                        ? PdfFieldFill.Blank(wanted.Value, name,
                            street == null ? NotAStreetPlot : street.Why)
                        : PdfFieldFill.Writing(wanted.Value, name, Number(street.Width), unit);

                case PdfValue.Length:
                    return Length(wanted, street, unit);

                case PdfValue.TotalAreasToBeGreened:
                    return Greened(wanted, form, workbookWritten, workbook, unit);

                case PdfValue.PercentageCanopy:
                    return Percentage(wanted, workbookWritten, workbook, unit);

                case PdfValue.IrrigationWaterDemand:
                    return PdfFieldFill.Blank(wanted.Value, name, WaterDemandIsNotReadYet);

                case PdfValue.ExistingTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ExistingTreesSheet);

                case PdfValue.ProposedTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ProposedTreesSheet);

                case PdfValue.TotalTrees:
                    return PdfFieldFill.Writing(wanted.Value, name, Whole(
                        Counted(reading, counted, KpiTemplates.ExistingTreesSheet)
                        + Counted(reading, counted, KpiTemplates.ProposedTreesSheet)), unit);

                case PdfValue.ExistingShrubs:
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.ExistingSquareMetres), unit)
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.ProposedShrubs:
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.ProposedSquareMetres), unit)
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.TotalShrubs:
                    // **Existing plus proposed, on all three forms.** Never the group total row,
                    // which holds every phase the schedule printed, Street Design among them.
                    //
                    // **THE NOTE ON THIS FIELD IS WRONG ON THE PARKS FORM AND MUST NOT BE
                    // TRUSTED.** Its tooltip reads Existing Shrubs on the row the page labels
                    // TOTAL Shrubs Area, so a tool matching by note would put the existing area
                    // into a box printed TOTAL in a client document. Bader confirmed with the
                    // client on 14 September that all three forms mean the sum of the two above.
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.TotalSquareMetres), unit)
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.GroundCover:
                    return PdfFieldFill.Blank(wanted.Value, name, GroundCoverIsNotPrintedApart);

                case PdfValue.Lawn:
                    return lawn == null
                        ? PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.LawnHeading))
                        : PdfFieldFill.Writing(wanted.Value, name, Number(lawn.SquareMetres), unit);

                default:
                    return PdfFieldFill.Blank(wanted.Value, name,
                        "nothing in this tool knows what " + wanted.Value + " is, which is a bug");
            }
        }

        /// <summary>
        /// **THE FORM ASKS km AND THE SOURCE GIVES m.** The reference file's QUANTITY UNIT reads
        /// m, the Roads page prints km in its unit column beside this box, and the workbook's own
        /// Streets Total Length (m) cell takes the metres unchanged. So the metres are divided by
        /// a thousand FOR THE PDF ALONE and the workbook is left exactly as it was.
        ///
        /// A row in any unit but m never reaches here: <see cref="StreetReferenceFile.For"/>
        /// refuses it, naming the plot, the row and what the unit said, and this writes nothing
        /// and carries that reason.
        /// </summary>
        private static PdfFieldFill Length(PdfFormField wanted, StreetReferenceAnswer street, string unit)
        {
            if (street == null || !street.Found)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName,
                    street == null ? NotAStreetPlot : street.Why);
            }

            double kilometres = street.Length / 1000.0;

            return PdfFieldFill.Writing(wanted.Value, wanted.FieldName, Fine(kilometres), unit,
                Number(street.Length) + " metres over 1000 is " + Fine(kilometres)
                + " kilometres, because the form asks km and the reference file's "
                + StreetReferenceFile.UnitColumn + " reads " + StreetReferenceFile.Metres);
        }

        /// <summary>
        /// Total areas to be greened, in square kilometres.
        ///
        /// **COMPUTED, NOT READ.** Parks and open spaces name the cell beside Total Green cover,
        /// which is the canopy plus the planting plus the lawn, and roads names the canopy cell
        /// on its own. Each form gets what its own note names.
        /// </summary>
        private static PdfFieldFill Greened(
            PdfFormField wanted, PdfForm form, bool workbookWritten, PdfWorkbookNumbers workbook, string unit)
        {
            string stopped = WhyNothingCanBeComputed(workbookWritten, workbook);
            if (stopped.Length > 0) return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, stopped);

            // **The workbook's own Total Green cover cell is held against this tool's sum**, and
            // a cell that has drifted blanks the field on every form. Roads writes the canopy
            // alone rather than the sum, and it is gated the same way on purpose: a green cover
            // cell that has moved says the workbook's arithmetic moved under the tool, and a
            // field written through that is a number nobody can check.
            if (workbookWritten && !workbook.GreenCoverCell.Usable)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, workbook.GreenCoverCell.Why);
            }

            bool canopyAlone = ReferenceEquals(form, PdfForms.Roads);

            ComputedValue found = canopyAlone
                ? ComputedValue.Of(workbook.Canopy.SquareMetres,
                    "canopy " + Number(workbook.Canopy.SquareMetres) + " square metres. "
                    + RoadsNamesTheCanopyCell)
                : GreenCover.Total(workbook.Canopy, workbook.PlantingSquareMetres, workbook.LawnSquareMetres);

            if (!found.Computed) return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, found.Why);

            double squareKilometres = found.Value / 1000000.0;

            return PdfFieldFill.Writing(wanted.Value, wanted.FieldName, Fine(squareKilometres), unit,
                found.Working + ", over 1,000,000 is " + Fine(squareKilometres) + " square kilometres");
        }

        /// <summary>
        /// The canopy percentage. **The workbook's cell holds a RATIO and the form prints a
        /// percent sign in its own unit column**, so the ratio is multiplied by a hundred and no
        /// sign is written. Checked against the client's filled ANH-006-NP-100002, which reads
        /// an area of 771, 0.000550 square kilometres greened and a percentage of 71.
        /// </summary>
        private static PdfFieldFill Percentage(
            PdfFormField wanted, bool workbookWritten, PdfWorkbookNumbers workbook, string unit)
        {
            string stopped = WhyNothingCanBeComputed(workbookWritten, workbook);
            if (stopped.Length > 0) return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, stopped);

            // **A template with no canopy percentage cell has nothing to check**, which is both
            // templates this form is ever fed by, so the number goes in and the report says the
            // workbook holds no cell for it. Only a cell that is there and has drifted blanks it.
            if (workbookWritten && !workbook.PercentageCell.Usable)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, workbook.PercentageCell.Why);
            }

            ComputedValue found = GreenCover.Percentage(workbook.Canopy, workbook.AreaSquareMetres);
            if (!found.Computed) return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, found.Why);

            return PdfFieldFill.Writing(wanted.Value, wanted.FieldName, Number(found.Value), unit, found.Working);
        }

        /// <summary>
        /// Why neither computed number can be produced, or empty. **Both rest on the canopy**,
        /// so a workbook that was not written and a canopy column that has drifted stop both.
        /// </summary>
        private static string WhyNothingCanBeComputed(bool workbookWritten, PdfWorkbookNumbers workbook)
        {
            if (!workbookWritten) return TheWorkbookWasNotWritten;

            return workbook.Arithmetic.Usable ? string.Empty : workbook.Arithmetic.Why;
        }

        private static PdfFieldFill Trees(
            PdfFormField wanted, PlotReading reading, CountedGroups counted, string sheet)
        {
            return PdfFieldFill.Writing(
                wanted.Value, wanted.FieldName, Whole(Counted(reading, counted, sheet)), wanted.Unit);
        }

        /// <summary>
        /// The trees of one tree list sheet, off the softscape groups the plot printed. **The
        /// same rule the species merge follows**, so Street Design counts as Proposed on STREETS
        /// and is left out elsewhere with nothing written twice.
        /// </summary>
        private static int Counted(PlotReading reading, CountedGroups counted, string sheet)
        {
            int found = 0;
            foreach (PrintedGroup group in reading.PrintedGroups)
            {
                if (!group.SubtotalPrinted) continue;
                if (!string.Equals(counted.SheetFor(group.Name), sheet, StringComparison.OrdinalIgnoreCase)) continue;

                found = found + group.Subtotal;
            }

            return found;
        }

        private static string NoGroup(string heading)
        {
            return "this plot's shrubs and lawn schedule printed no " + heading + " group";
        }

        private static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A number small enough that two places would print it as nought. Square kilometres and
        /// kilometres are both that: 550 square metres is 0.00055, and the client's own filled
        /// example prints 0.000550.
        /// </summary>
        private static string Fine(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }

        private static string Whole(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
