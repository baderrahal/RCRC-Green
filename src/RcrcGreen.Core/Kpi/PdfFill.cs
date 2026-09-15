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
        private PdfFieldFill(
            PdfValue value, string fieldName, string text, string why, string unit, string working,
            bool noughtForAnAbsentGroup = false)
        {
            Value = value;
            FieldName = fieldName ?? string.Empty;
            Text = text ?? string.Empty;
            Why = why ?? string.Empty;
            Unit = unit ?? string.Empty;
            Working = working ?? string.Empty;
            NoughtForAnAbsentGroup = noughtForAnAbsentGroup;
        }

        public static PdfFieldFill Writing(PdfValue value, string fieldName, string text, string unit = null, string working = null)
        {
            return new PdfFieldFill(value, fieldName, text, string.Empty, unit, working);
        }

        /// <summary>
        /// A 0 written because the plot's shrubs and lawn schedule was read and printed no such
        /// group. **The flag is what the glance counts**, rather than the working being searched
        /// for a phrase: a signal that travels in the printed words is not a signal, which this
        /// repository has already paid for once, and the words themselves are rewritten in this
        /// same round.
        /// </summary>
        public static PdfFieldFill WritingNoughtForAnAbsentGroup(
            PdfValue value, string fieldName, string text, string unit, string working)
        {
            return new PdfFieldFill(value, fieldName, text, string.Empty, unit, working, true);
        }

        /// <summary>
        /// Whether this 0 came from a read schedule holding no such group rather than from a
        /// group whose subtotal really is nought. **Bader's decision of 15 September turned 96
        /// blank boxes into noughts, so how many of the run's numbers are that nought is the one
        /// thing the glance has to say about it.**
        /// </summary>
        public bool NoughtForAnAbsentGroup { get; }

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
        // **GroundCoverIsNotPrintedApart IS DELETED and this is the second reason a thing gets
        // deleted rather than the first.** It read that the schedule prints the group as one and
        // nothing prints ground cover on its own, so it is not derived. The SHAPE it recorded is
        // gone, not its last caller: every species row in that group begins SHRUBS: or GROUND
        // COVER:, measured over 156 plots, so the two are printed apart and the number is read
        // rather than derived. A constant recording a claim the data disproves is worse than no
        // constant, which is what OutputName.Suggested bought this project.

        /// <summary>
        /// **IT IS READ NOW, off both schedules' own TOTAL rows.** Bader, 15 September: take the
        /// total of all, add the two, divide by a thousand. The read is
        /// <see cref="WaterDemandRead.From"/> and this is only what the blank says when it could
        /// not be done.
        ///
        /// **The form asks m³/day and both schedules print litres a day**, measured on 14
        /// September, so the division happens for the PDF alone. The sentence keeps both units in
        /// it because a blank that named only one would leave a person guessing which end failed.
        /// </summary>
        public const string WaterDemandNotRead =
            "no irrigation water demand could be read. Both schedules' L/DAY TOTAL rows are "
            + "needed, added and divided by 1000, because the form asks m³/day and the "
            + "schedules print litres a day";

        public const string NoRegionChosen =
            "no filled region was chosen for this plot, so it has no intervention area";

        public const string NotAStreetPlot = "this form takes no road width or length";

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
            PdfWorkbookNumbers workbook,
            ProjectUnit areaUnit = null)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            if (counted == null) throw new ArgumentNullException("counted");
            if (workbook == null) throw new ArgumentNullException("workbook");

            PdfForm form = PdfForms.ForPlot(reading.PlotId);
            if (form == null) return PdfPlan.NoForm(reading.PlotId, PdfForms.NoFormFor(reading.PlotId));

            PhaseSplit shrubs = ShrubsByPhase.Of(reading, KpiMerge.ShrubsHeading, counted, areaUnit);
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
                    return Greened(wanted, reading, workbookWritten, workbook, unit);

                case PdfValue.PercentageCanopy:
                    return Percentage(wanted, workbookWritten, workbook, unit);

                case PdfValue.IrrigationWaterDemand:
                    // **BOTH SCHEDULES OR NOTHING**, and the reason names which half failed.
                    // The value is the two TOTAL rows added and divided by a thousand, worked
                    // out once on the reading so the report and the form cannot print two
                    // different numbers.
                    //
                    // **IT PRINTS FINE AND NOT TO TWO PLACES.** Dividing litres by a thousand is
                    // what makes it small, the same way square metres divided by a million make
                    // the green cover small: 2492 L/day is 2.492 and two places would send 2.49
                    // to the client, which is two litres a day thrown away on every plot.
                    return reading.Water.BothRead
                        ? PdfFieldFill.Writing(wanted.Value, name, Fine(reading.Water.CubicMetresADay), unit)
                        : PdfFieldFill.Blank(wanted.Value, name, WaterDemandNotRead + ". " + reading.Water.Why);

                case PdfValue.ExistingTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ExistingTreesSheet);

                case PdfValue.ProposedTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ProposedTreesSheet);

                case PdfValue.TotalTrees:
                    return reading.SoftscapeRead
                        ? PdfFieldFill.Writing(wanted.Value, name, Whole(
                            Counted(reading, counted, KpiTemplates.ExistingTreesSheet)
                            + Counted(reading, counted, KpiTemplates.ProposedTreesSheet)), unit)
                        : PdfFieldFill.Blank(wanted.Value, name, NoSoftscapeRead(reading));

                case PdfValue.ExistingShrubs:
                    // **THE SHRUBS SPECIES OF THE EXISTING PHASES, not the phase rows.** A phase
                    // row holds shrubs and ground cover added together, and DM-14's holds 468 of
                    // which none is shrubs.
                    return Shrubs(wanted, name, reading, shrubs, shrubs.ByPrefix.ExistingShrubsSquareMetres, unit);

                case PdfValue.ProposedShrubs:
                    return Shrubs(wanted, name, reading, shrubs, shrubs.ByPrefix.ProposedShrubsSquareMetres, unit);

                case PdfValue.TotalShrubs:
                    // **Existing plus proposed, on all three forms.** Never the group total row,
                    // which holds every phase the schedule printed, Street Design among them.
                    //
                    // **THE NOTE ON THIS FIELD IS WRONG ON THE PARKS FORM AND MUST NOT BE
                    // TRUSTED.** Its tooltip reads Existing Shrubs on the row the page labels
                    // TOTAL Shrubs Area, so a tool matching by note would put the existing area
                    // into a box printed TOTAL in a client document. Bader confirmed with the
                    // client on 14 September that all three forms mean the sum of the two above.
                    return Shrubs(wanted, name, reading, shrubs, shrubs.ByPrefix.TotalShrubsSquareMetres, unit);

                case PdfValue.GroundCover:
                    // **IT IS PRINTED APART, BY THE PREFIX ITS SPECIES CARRY.** The group is
                    // SHRUBS AND GROUND COVER and every species name in it begins SHRUBS: or
                    // GROUND COVER:, measured over 156 plots on the 05:49 run.
                    return Shrubs(wanted, name, reading, shrubs, shrubs.ByPrefix.GroundCoverSquareMetres, unit);

                case PdfValue.Lawn:
                    // **A SCHEDULE THAT WAS READ AND PRINTS NO SUCH GROUP IS A NOUGHT**, Bader's
                    // decision of 15 September, off a run where 81 plots left this box blank
                    // saying the schedule printed no GRASS group. A plot with that schedule read
                    // and no GRASS group in it HAS no lawn, and a blank box on a form reads as a
                    // number nobody filled in.
                    //
                    // **A PLOT WITH NO SUCH SCHEDULE AT ALL IS STILL BLANK**, because that is an
                    // absence and not a measurement, which is the rule one line down.
                    if (lawn != null)
                    {
                        return PdfFieldFill.Writing(wanted.Value, name, Number(lawn.SquareMetres), unit);
                    }

                    return reading.ShrubsAndLawnRead
                        ? PdfFieldFill.WritingNoughtForAnAbsentGroup(
                            wanted.Value, name, Number(0.0), unit, NoGroupIsNought(KpiMerge.LawnHeading))
                        : PdfFieldFill.Blank(wanted.Value, name, NoScheduleRead(reading));

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
        /// **COMPUTED, NOT READ.** It is the canopy plus the planting plus the lawn, divided by
        /// a million, **on all three forms**.
        ///
        /// **BADER'S DECISION, 15 September, and it closes the open question.** The Roads form's
        /// own note names the canopy cell where the other two name Total Green cover, and the
        /// client meant the same number on all three. The note is still held exactly as the file
        /// carries it, because <see cref="PdfFormCheck"/> compares notes and a tidied one would
        /// refuse the form, and it is no longer what decides the value. The streets template
        /// computes D9 = F9+F11+H11, which is that same sum, so the form and the workbook beside
        /// it now agree.
        ///
        /// Measured off ANH-007-ST-100308: canopy 984, planting 69, lawn empty, Total Green
        /// cover 1,053. The field reads 0.001053 and not 0.000984.
        /// </summary>
        private static PdfFieldFill Greened(
            PdfFormField wanted, PlotReading reading, bool workbookWritten,
            PdfWorkbookNumbers workbook, string unit)
        {
            // **THE CANOPY IS COUNTED OFF THE TREE ROWS, so no softscape schedule means no
            // canopy, and a green cover computed without one is short by however many trees the
            // plot really holds.** FM-07 wrote 0 here on the 13:32 run against 57 trees in the
            // model. It is left empty and named for the same reason its three tree boxes are.
            if (!reading.SoftscapeRead)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, NoSoftscapeRead(reading));
            }

            string stopped = WhyNothingCanBeComputed(workbookWritten, workbook);
            if (stopped.Length > 0) return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, stopped);

            // **The workbook's own Total Green cover cell is held against this tool's sum**, and
            // a cell that has drifted blanks the field on every form, roads included: a green
            // cover cell that has moved says the workbook's arithmetic moved under the tool, and
            // a field written through that is a number nobody can check. The canopy guard above
            // is unchanged too, so a form whose plot has a tree on rows 85, 92 or 99 still writes
            // nothing here, which is Bader's decision of 15 September and stands.
            if (workbookWritten && !workbook.GreenCoverCell.Usable)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, workbook.GreenCoverCell.Why);
            }

            ComputedValue found = GreenCover.Total(
                workbook.Canopy, workbook.PlantingSquareMetres, workbook.LawnSquareMetres);

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
            // **A PLOT WITH NO SOFTSCAPE SCHEDULE HAS NO COUNT, AND NO COUNT IS NOT NOUGHT.**
            // `Counted` walks the printed groups, and a plot that printed none adds nothing up
            // to 0, which reads on the form exactly like a plot with no trees in it. FM-07 went
            // to the team reading 0, 0 and 0 with 57 trees in the model.
            if (!reading.SoftscapeRead)
            {
                return PdfFieldFill.Blank(wanted.Value, wanted.FieldName, NoSoftscapeRead(reading));
            }

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

        /// <summary>
        /// One of the four figures the shrubs and ground cover group feeds, with the two ways it
        /// can write nothing said once rather than four times.
        ///
        /// **A SPLIT THAT DOES NOT ADD UP WRITES NO FIGURE AT ALL**, and all four boxes carry the
        /// same refusal, because writing three of them and blanking one would leave a form whose
        /// own numbers disagree with the schedule behind it.
        /// </summary>
        private static PdfFieldFill Shrubs(
            PdfFormField wanted, string name, PlotReading reading, PhaseSplit shrubs, double value, string unit)
        {
            if (!shrubs.GroupFound)
            {
                // **READ AND NO SUCH GROUP IS A NOUGHT, no schedule at all is a blank.** 15 plots
                // of the 13:32 run left all four of these blank saying the schedule printed no
                // SHRUBS & GROUND COVER group, and a plot whose schedule was read and holds none
                // has no shrubs and no ground cover.
                return reading.ShrubsAndLawnRead
                    ? PdfFieldFill.WritingNoughtForAnAbsentGroup(
                        wanted.Value, name, Number(0.0), unit, NoGroupIsNought(KpiMerge.ShrubsHeading))
                    : PdfFieldFill.Blank(wanted.Value, name, NoScheduleRead(reading));
            }

            if (!shrubs.ByPrefix.AddsUp)
            {
                return PdfFieldFill.Blank(wanted.Value, name, shrubs.ByPrefix.Refusal);
            }

            return PdfFieldFill.Writing(wanted.Value, name, Number(value), unit);
        }

        /// <summary>
        /// **A GROUP A READ SCHEDULE DOES NOT PRINT IS A NOUGHT AND SAYS SO.** Bader's decision
        /// of 15 September. The working travels with the value so a nought on a form can be told
        /// from a nought the schedule printed.
        /// </summary>
        public static string NoGroupIsNought(string heading)
        {
            return "the shrubs and lawn schedule was read and printed no " + heading
                + " group, so this plot has none and the box is written 0";
        }

        /// <summary>
        /// **NO SCHEDULE OF THAT KIND IS AN ABSENCE AND NEVER A NOUGHT.** Nothing was read, so
        /// nothing says the plot has none, and a 0 on a form is a measurement a person would act
        /// on. The names of every schedule found are said, because two of a kind and none of a
        /// kind are different sentences about different models.
        /// </summary>
        public static string NoScheduleRead(PlotReading reading)
        {
            return reading.MoreThanOneShrubsAndLawn
                ? "this plot holds " + reading.ShrubsAndLawnSchedules.Count
                    + " shrubs and lawn schedules, " + string.Join(", ", reading.ShrubsAndLawnSchedules.ToArray())
                    + ", and nothing says which is the real one, so none was read and this box is "
                    + "left empty rather than written 0"
                : "this plot holds no shrubs and lawn schedule, so nothing was read and this box "
                    + "is left empty rather than written 0";
        }

        /// <summary>
        /// **NO SOFTSCAPE SCHEDULE MEANS THE TREE COUNTS ARE UNKNOWN, NOT NOUGHT.** Measured on
        /// the 13:32 run: FM-07 is on a sheet and on no schedule, its PDF read Existing Trees 0,
        /// Proposed Trees 0 and TOTAL trees 0, and the model's own KPI% schedules list 57 trees
        /// for it. Three noughts went to the team as a measurement of a plot nothing had read.
        /// </summary>
        public static string NoSoftscapeRead(PlotReading reading)
        {
            return reading.MoreThanOneSoftscape
                ? "this plot holds " + reading.SoftscapeSchedules.Count + " softscape schedules, "
                    + string.Join(", ", reading.SoftscapeSchedules.ToArray())
                    + ", and nothing says which is the real one, so no tree was counted and this "
                    + "box is left empty rather than written 0"
                : "this plot holds no softscape schedule, so no tree was counted and this box is "
                    + "left empty rather than written 0";
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
