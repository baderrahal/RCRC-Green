using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value a PDF form field takes. **Not the same list as <see cref="KpiValue"/>**: the
    /// workbook and the form ask for different things off one reading, and the form asks for
    /// two the workbook never has, the shrubs split by phase and the ground cover on its own.
    /// </summary>
    public enum PdfValue
    {
        /// <summary>PRX_Component, EXACTLY as Revit holds it. Bader, 14 September.</summary>
        ProjectType,

        Uid,

        /// <summary>Today, day/month/year. Bader, 14 September.</summary>
        ReportDate,

        /// <summary>PRX_Intervention Area off the chosen filled region.</summary>
        Area,

        /// <summary>ROAD_WIDTH out of the team's scope validation file.</summary>
        Row,

        /// <summary>ES_QUANTITY out of the team's scope validation file.</summary>
        Length,

        /// <summary>Total Green cover off the workbook this run wrote, divided by 1,000,000.</summary>
        TotalAreasToBeGreened,

        /// <summary>Total area covered by canopy off the workbook this run wrote.</summary>
        PercentageCanopy,

        /// <summary>Total water demand off the schedules, divided by 1000.</summary>
        IrrigationWaterDemand,

        ExistingTrees,
        ProposedTrees,

        /// <summary>The two above added.</summary>
        TotalTrees,

        ExistingShrubs,
        ProposedShrubs,

        /// <summary>The two above added, on all three forms.</summary>
        TotalShrubs,

        GroundCover,
        Lawn,

        /// <summary>
        /// A field on the form that this tool names nowhere. It gets EMPTIED rather than left,
        /// because the client's default value in it is their own note to whoever fills the form
        /// by hand, and a note printed in a box reads as an answer.
        /// </summary>
        NotOne
    }

    /// <summary>
    /// One field of one form: which value goes in it, what the field is really called in the
    /// file, and the note the file carried when it was measured.
    /// </summary>
    public sealed class PdfFormField
    {
        public PdfFormField(PdfValue value, string fieldName, string note, double x, double y, string unit)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            Value = value;
            FieldName = fieldName;
            Note = note ?? string.Empty;
            X = x;
            Y = y;
            Unit = unit ?? string.Empty;
        }

        /// <summary>
        /// Where the field sits on the page, measured off the file on 14 September.
        ///
        /// **THE POSITION IS THE THIRD RECORD AND IT IS THE ONE A PERSON READS.** The name and
        /// the note were checked and the position was not, and a field that MOVES to another row
        /// keeps both while meaning something else. On the open spaces form every shrub row
        /// carries two boxes, a quantity at x=500.5 and an area at x=548.1, so a field that
        /// slipped one column would take a number into the wrong box with its name and its note
        /// intact.
        /// </summary>
        public double X { get; }

        public double Y { get; }

        /// <summary>
        /// The unit the FORM asks for, **read off the page by position rather than off a note**,
        /// on 14 September. It is what the report prints beside every written value, never the
        /// unit the source gave, because printing the source's would make a converted value look
        /// unconverted.
        ///
        /// **FOUR FIELDS CONVERT AND EVERY OTHER IS WRITTEN IN THE UNIT IT WAS READ IN.** The
        /// road length is metres into km, Total areas to be greened is square metres into km²,
        /// the canopy percentage is a ratio times a hundred, and the irrigation water demand
        /// would be litres a day into m³/day, which nothing reaches because no water demand is
        /// read off any schedule yet.
        ///
        /// The three fields carrying no measured quantity, the UID, the project type and the
        /// report date, have no unit printed beside them on any form, so this says what the
        /// value IS rather than inventing one.
        /// </summary>
        public string Unit { get; }

        public PdfValue Value { get; }

        /// <summary>
        /// The field's FULL name in the file, built through its parent chain. **On the open
        /// spaces form these are not the labels a person reads**: the UID is a field called
        /// undefined_4.1 and the lawn area is a field called 0_2.
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// What the field's value held when the three forms were read on 14 September. The
        /// client writes the source of each value into the field itself, and this is that text.
        ///
        /// **It is checked and never used to decide anything.** A form whose note has moved is
        /// a form this tool does not know, and it writes nothing into one.
        /// </summary>
        public string Note { get; }
    }

    /// <summary>
    /// One of the client's three Projects Basic Data forms.
    ///
    /// **MEASURED ON 14 SEPTEMBER, OFF THE FILES THEMSELVES.** Every field name and every note
    /// below was read out of the PDF rather than copied from a list, and the round message's own
    /// list is what somebody wrote down about them. Where the two differed the file won, and the
    /// differences are named in `.claude/rules/kpi-rules.md`.
    ///
    /// **The source of each value is in the field's VALUE, not its default value.** Only the
    /// four stage tick boxes carry a default value at all, and on two of the three forms that
    /// default is a tick on all four while the real state sits in the value.
    /// </summary>
    public sealed class PdfForm
    {
        public PdfForm(string name, int fieldCount, IEnumerable<string> prefixes, IEnumerable<PdfFormField> fields)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");

            Name = name;
            FieldCount = fieldCount;
            Prefixes = (prefixes ?? Enumerable.Empty<string>()).ToList();
            Fields = (fields ?? Enumerable.Empty<PdfFormField>()).ToList();
        }

        public string Name { get; }

        /// <summary>
        /// How many fields the file held when it was measured, the tick boxes and the reset
        /// button included. A file offering a different count is not refused on that alone, but
        /// the report says it.
        /// </summary>
        public int FieldCount { get; }

        /// <summary>
        /// The plot prefixes this form is for. **This is the first thing the prefix decides on
        /// its own** rather than cross checking a component: which form a plot gets is the
        /// team's filing and PRX_Component says nothing about it.
        /// </summary>
        public IReadOnlyList<string> Prefixes { get; }

        public IReadOnlyList<PdfFormField> Fields { get; }

        public PdfFormField FieldFor(PdfValue value)
        {
            return Fields.FirstOrDefault(one => one.Value == value);
        }

        public bool Takes(PdfValue value)
        {
            return FieldFor(value) != null;
        }
    }

    /// <summary>
    /// The three forms, which prefixes reach each, and every field this tool fills.
    ///
    /// **A FIELD WITH NO NOTE IS NOT FILLED.** Bader, 14 September: what the client did not
    /// annotate is what they do not need. The forms carry many such fields, sidewalks, medians,
    /// water tanks, toilets, kiosks, seating, play areas, bridges and a catwalk, and none of
    /// them is in the table below.
    /// </summary>
    public static class PdfForms
    {
        public const string ParkType = "Park";

        public const string RoadType = "Road";

        /// <summary>
        /// What the three typed fields hold already, so the check can say the file still carries
        /// them. **The tool writes none of these**: the client typed them and they are right.
        /// </summary>
        public const string ProjectName = "Neighborhood Landscape Design - Zone #2";

        public const string ConsultantName = "SAPL";

        public const string ContractReference = "GP.NH.Z2.052-DES042";

        private const string TreesExisting =
            "REVIT SHEET/REVIT COPONENTS LINK /SOFTSCAPE SCHEDULE/ENTER EACH EXISTING TREE QUANTITY";

        private const string TreesProposed =
            "REVIT SHEET/REVIT COPONENTS LINK /SOFTSCAPE SCHEDULE/ENTER EACH PROPOSED TREE QUANTITY";

        private const string RegionArea = "REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area";

        private const string Uid2 = "REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2";

        private const string DateOfTheDay = "DATE OF THE DAY";

        public static readonly PdfForm Parks = new PdfForm(
            "Projects Basic Data - Parks", 42, new[] { "EP", "FP" }, new[]
            {
                new PdfFormField(PdfValue.Uid, "UID", Uid2, 132.2, 671.6, "text"),
                new PdfFormField(PdfValue.ReportDate, "Report Date", DateOfTheDay, 132.2, 660.4, "a date"),
                new PdfFormField(PdfValue.Area, "Area", RegionArea, 215.2, 611.3, "m²"),
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "excel the cell on the right of  \"Total Green cover (m²) \" /1000000", 215.2, 580.3, "km²"),
                new PdfFormField(PdfValue.PercentageCanopy, "Percentage Total area covered by canopy",
                    "excel the cell on the right of  \"Total area covered by canopy \"", 215.2, 549.4, "%"),
                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "Revit / softscape & shrubs & lawn schedule / total water demand /1000", 476.9, 497.8, "m³/day"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting, 477.0, 611.3, "count"),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees", TreesProposed, 477.0, 601.0, "count"),
                new PdfFormField(PdfValue.TotalTrees, "TOTAL trees", "sum of the above or from revit", 477.0, 590.6, "count"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ EXISTING SHRUBS  TOTAL AREA", 519.2, 580.3, "m²"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ PROPOSED SHRUBS  TOTAL AREA", 519.2, 570.2, "m²"),
                new PdfFormField(PdfValue.TotalShrubs, "TOTAL Shrubs Area (m²)",
                    "Sum of the above or from revit", 519.2, 560.3, "m²"),
                new PdfFormField(PdfValue.GroundCover, "Ground Cover",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/GROUND COVER TOTAL AREA", 519.6, 549.4, "m²"),
                new PdfFormField(PdfValue.Lawn, "Lawn",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA", 519.6, 539.0, "m²")
            });

        /// <summary>
        /// **THIS FORM'S FIELD NAMES ARE BROKEN AND THE TOOL MUST NOT TRUST THEM.** The UID is
        /// undefined_4.1, the lawn area is 0_2, the ground cover is 0, and the total trees is a
        /// field called Proposed Trees.1. Five of the nine plot prefixes use this form. Bader has
        /// asked the client to fix it and they have not.
        /// </summary>
        public static readonly PdfForm OpenSpaces = new PdfForm(
            "Projects Basic Data - Open spaces associated to buildings", 49,
            new[] { "HF", "FM", "DM", "PL", "SC" }, new[]
            {
                new PdfFormField(PdfValue.ProjectType, "undefined_4.0",
                    "REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT", 108.7, 664.3, "text"),
                new PdfFormField(PdfValue.Uid, "undefined_4.1", Uid2, 108.7, 650.3, "text"),
                new PdfFormField(PdfValue.ReportDate, "undefined_4.2", DateOfTheDay, 108.7, 636.6, "a date"),
                new PdfFormField(PdfValue.Area, "Area", RegionArea, 203.2, 551.2, "m²"),
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "Excel file/ the cel beside Total Green cover (m²)/1000000", 203.2, 510.4, "km²"),
                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "REVIT/softscape & shrubs schedule total Water demand / 1000", 500.4, 551.2, "m³/day"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting, 500.5, 449.2, "count"),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees.0", TreesProposed, 500.5, 428.9, "count"),
                new PdfFormField(PdfValue.TotalTrees, "Proposed Trees.1", "SUM THE ABOVE OF FROM REVIT", 500.5, 408.9, "count"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs.1",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/ EXISTING SHRUBS TOTAL AREA", 548.1, 387.9, "m²"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs.1.0",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/ PROPOSED SHRUBS TOTAL AREA", 548.1, 367.4, "m²"),
                new PdfFormField(PdfValue.TotalShrubs, "Proposed Shrubs.1.1", "sum of the above or from revit", 548.1, 346.9, "m²"),
                new PdfFormField(PdfValue.GroundCover, "0",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/GROUND COVER TOTAL AREA", 548.5, 326.6, "m²"),
                new PdfFormField(PdfValue.Lawn, "0_2",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA", 548.5, 306.2, "m²")
            });

        public static readonly PdfForm Roads = new PdfForm(
            "Projects Basic Data - Roads", 34, new[] { "NS", "ST", "MM" }, new[]
            {
                new PdfFormField(PdfValue.Uid, "UID", Uid2, 82.2, 634.9, "text"),
                new PdfFormField(PdfValue.ReportDate, "Report Date", DateOfTheDay, 82.2, 619.2, "a date"),
                new PdfFormField(PdfValue.Row, "Row",
                    "EXCEL FILE \"Scope Validation 21072026\" /COLUMN O1 \"ROAD_WIDTH\"", 191.9, 538.9, "m"),
                new PdfFormField(PdfValue.Length, "Length",
                    "EXCEL FILE \"Scope Validation 21072026\" /COLUMN H1 \"ES_QUANTITY\"", 191.9, 521.0, "km"),

                // **THIS NOTE NAMES THE CANOPY CELL AND THE OTHER TWO FORMS NAME TOTAL GREEN
                // COVER**, for a field all three call Total areas to be greened. It is recorded
                // here exactly as the file holds it, so the check does not refuse the form over
                // the client's own copy and paste, and the disagreement is an open question in
                // the log rather than a number chosen quietly.
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "excel the cell on the right of  \"Total area covered by canopy \"", 191.9, 503.2, "km²"),

                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "Revit / softscape & shrubs & lawn schedule / total water demand /1000", 191.9, 360.0, "m³/day"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting, 500.0, 538.9, "count"),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees", TreesProposed, 500.0, 521.0, "count"),
                new PdfFormField(PdfValue.TotalTrees, "TOTAL trees", "sum of the above or from revit", 500.0, 503.2, "count"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ EXISTING SHRUBS  TOTAL AREA", 547.7, 485.1, "m²"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ PROPOSED SHRUBS  TOTAL AREA", 547.7, 467.3, "m²"),
                new PdfFormField(PdfValue.TotalShrubs, "TOTAL Shrubs Area (m²)",
                    "sum of the above or from revit", 547.7, 449.5, "m²"),
                new PdfFormField(PdfValue.GroundCover, "Ground Cover",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/GROUND COVER TOTAL AREA", 548.0, 431.6, "m²"),
                new PdfFormField(PdfValue.Lawn, "Lawn",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA", 548.0, 413.8, "m²")
            });

        public static readonly IReadOnlyList<PdfForm> All = new[] { Parks, OpenSpaces, Roads };

        /// <summary>
        /// The form a plot's prefix reaches, or null. **A prefix the table does not hold writes
        /// no PDF and is named**, the same rule a component the folder table does not hold
        /// follows.
        /// </summary>
        public static PdfForm For(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return null;

            string held = prefix.Trim();

            return All.FirstOrDefault(form => form.Prefixes.Any(
                one => string.Equals(one, held, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// The form a plot reaches, off the two letters at the front of its identifier.
        /// </summary>
        public static PdfForm ForPlot(string plotId)
        {
            return For(PlotPrefixes.Of(plotId));
        }

        public static string NoFormFor(string plotId)
        {
            string prefix = PlotPrefixes.Of(plotId);

            return prefix.Length == 0
                ? plotId + " carries no plot prefix, so no form is named for it and no PDF was written"
                : "no form is named for the plot prefix " + prefix + ", so no PDF was written";
        }
    }
}
