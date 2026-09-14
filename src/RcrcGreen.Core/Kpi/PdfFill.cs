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
        private PdfFieldFill(PdfValue value, string fieldName, string text, string why)
        {
            Value = value;
            FieldName = fieldName ?? string.Empty;
            Text = text ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public static PdfFieldFill Writing(PdfValue value, string fieldName, string text)
        {
            return new PdfFieldFill(value, fieldName, text, string.Empty);
        }

        public static PdfFieldFill Blank(PdfValue value, string fieldName, string why)
        {
            return new PdfFieldFill(value, fieldName, string.Empty, why);
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
        public PdfPlan(string plotId, PdfForm form, IEnumerable<PdfFieldFill> fields, string why)
        {
            PlotId = plotId ?? string.Empty;
            Form = form;
            Fields = (fields ?? Enumerable.Empty<PdfFieldFill>()).ToList();
            Why = why ?? string.Empty;
        }

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

    public static class PdfFill
    {
        /// <summary>
        /// **The workbook's own cell is not in the workbook.** Total Green cover and Total area
        /// covered by canopy are formulas, and the patcher drops the cached result of every
        /// formula cell on purpose so Excel recalculates rather than opening on stale zeros. So
        /// the number does not exist in the file this run wrote, and reading it back would read
        /// an empty cell.
        ///
        /// **The field is left blank and named rather than computed here**, because working the
        /// client's own formula out again is the one thing this tool never does.
        /// </summary>
        public const string TheWorkbookHasNotComputedItYet =
            "it is a formula in the workbook and this tool drops every cached formula result so "
            + "Excel recalculates, so the number is not in the file yet";

        public const string TheWorkbookWasNotWritten =
            "this plot's workbook was not written, so there is no cell to read it from";

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
        /// One plot's plan. Every value comes off the reading, the street reference answer and
        /// the workbook that was already written, and nothing is worked out twice.
        /// </summary>
        public static PdfPlan Of(
            PlotReading reading,
            CountedGroups counted,
            StreetReferenceAnswer street,
            DateTime today,
            bool workbookWritten)
        {
            if (reading == null) throw new ArgumentNullException("reading");
            if (counted == null) throw new ArgumentNullException("counted");

            PdfForm form = PdfForms.ForPlot(reading.PlotId);
            if (form == null) return PdfPlan.NoForm(reading.PlotId, PdfForms.NoFormFor(reading.PlotId));

            PhaseSplit shrubs = ShrubsByPhase.Of(reading, KpiMerge.ShrubsHeading, counted);
            GroupSubtotal lawn = reading.SubtotalHeaded(KpiMerge.LawnHeading);

            var fields = new List<PdfFieldFill>();

            foreach (PdfFormField wanted in form.Fields)
            {
                fields.Add(One(wanted, reading, counted, street, today, workbookWritten, shrubs, lawn));
            }

            return new PdfPlan(reading.PlotId, form, fields, string.Empty);
        }

        private static PdfFieldFill One(
            PdfFormField wanted,
            PlotReading reading,
            CountedGroups counted,
            StreetReferenceAnswer street,
            DateTime today,
            bool workbookWritten,
            PhaseSplit shrubs,
            GroupSubtotal lawn)
        {
            string name = wanted.FieldName;

            switch (wanted.Value)
            {
                case PdfValue.ProjectType:
                    // **EXACTLY AS REVIT HOLDS IT, capitals and all.** Bader, 14 September.
                    return reading.Component.Length == 0
                        ? PdfFieldFill.Blank(wanted.Value, name, "this plot's sheet carries no " + KpiNames.Component)
                        : PdfFieldFill.Writing(wanted.Value, name, reading.Component);

                case PdfValue.Uid:
                    return reading.Uid2.Length == 0
                        ? PdfFieldFill.Blank(wanted.Value, name, "this plot's sheet carries no " + KpiNames.PlotUid2)
                        : PdfFieldFill.Writing(wanted.Value, name, reading.Uid2);

                case PdfValue.ReportDate:
                    // Day, month, year. Bader, 14 September.
                    return PdfFieldFill.Writing(wanted.Value, name,
                        today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

                case PdfValue.Area:
                    return reading.ChosenRegion == null
                        ? PdfFieldFill.Blank(wanted.Value, name, NoRegionChosen)
                        : PdfFieldFill.Writing(wanted.Value, name, Number(reading.ChosenRegion.SquareMetres));

                case PdfValue.Row:
                    return street == null || !street.Found
                        ? PdfFieldFill.Blank(wanted.Value, name,
                            street == null ? NotAStreetPlot : street.Why)
                        : PdfFieldFill.Writing(wanted.Value, name, Number(street.Width));

                case PdfValue.Length:
                    return street == null || !street.Found
                        ? PdfFieldFill.Blank(wanted.Value, name,
                            street == null ? NotAStreetPlot : street.Why)
                        : PdfFieldFill.Writing(wanted.Value, name, Number(street.Length));

                case PdfValue.TotalAreasToBeGreened:
                case PdfValue.PercentageCanopy:
                    return PdfFieldFill.Blank(wanted.Value, name,
                        workbookWritten ? TheWorkbookHasNotComputedItYet : TheWorkbookWasNotWritten);

                case PdfValue.IrrigationWaterDemand:
                    return PdfFieldFill.Blank(wanted.Value, name, WaterDemandIsNotReadYet);

                case PdfValue.ExistingTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ExistingTreesSheet);

                case PdfValue.ProposedTrees:
                    return Trees(wanted, reading, counted, KpiTemplates.ProposedTreesSheet);

                case PdfValue.TotalTrees:
                    return PdfFieldFill.Writing(wanted.Value, name, Whole(
                        Counted(reading, counted, KpiTemplates.ExistingTreesSheet)
                        + Counted(reading, counted, KpiTemplates.ProposedTreesSheet)));

                case PdfValue.ExistingShrubs:
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.ExistingSquareMetres))
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.ProposedShrubs:
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.ProposedSquareMetres))
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.TotalShrubs:
                    // **Existing plus proposed, on all three forms.** Never the group total row,
                    // which holds every phase the schedule printed, Street Design among them.
                    return shrubs.GroupFound
                        ? PdfFieldFill.Writing(wanted.Value, name, Number(shrubs.TotalSquareMetres))
                        : PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.ShrubsHeading));

                case PdfValue.GroundCover:
                    return PdfFieldFill.Blank(wanted.Value, name, GroundCoverIsNotPrintedApart);

                case PdfValue.Lawn:
                    return lawn == null
                        ? PdfFieldFill.Blank(wanted.Value, name, NoGroup(KpiMerge.LawnHeading))
                        : PdfFieldFill.Writing(wanted.Value, name, Number(lawn.SquareMetres));

                default:
                    return PdfFieldFill.Blank(wanted.Value, name,
                        "nothing in this tool knows what " + wanted.Value + " is, which is a bug");
            }
        }

        private static PdfFieldFill Trees(
            PdfFormField wanted, PlotReading reading, CountedGroups counted, string sheet)
        {
            return PdfFieldFill.Writing(wanted.Value, wanted.FieldName, Whole(Counted(reading, counted, sheet)));
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

        private static string Whole(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
