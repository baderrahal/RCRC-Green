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
        Lawn
    }

    /// <summary>
    /// One field of one form: which value goes in it, what the field is really called in the
    /// file, and the note the file carried when it was measured.
    /// </summary>
    public sealed class PdfFormField
    {
        public PdfFormField(PdfValue value, string fieldName, string note)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException("fieldName");

            Value = value;
            FieldName = fieldName;
            Note = note ?? string.Empty;
        }

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
                new PdfFormField(PdfValue.Uid, "UID", Uid2),
                new PdfFormField(PdfValue.ReportDate, "Report Date", DateOfTheDay),
                new PdfFormField(PdfValue.Area, "Area", RegionArea),
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "excel the cell on the right of  \"Total Green cover (m²) \" /1000000"),
                new PdfFormField(PdfValue.PercentageCanopy, "Percentage Total area covered by canopy",
                    "excel the cell on the right of  \"Total area covered by canopy \""),
                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "Revit / softscape & shrubs & lawn schedule / total water demand /1000"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees", TreesProposed),
                new PdfFormField(PdfValue.TotalTrees, "TOTAL trees", "sum of the above or from revit"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ EXISTING SHRUBS  TOTAL AREA"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ PROPOSED SHRUBS  TOTAL AREA"),
                new PdfFormField(PdfValue.TotalShrubs, "TOTAL Shrubs Area (m²)",
                    "Sum of the above or from revit"),
                new PdfFormField(PdfValue.GroundCover, "Ground Cover",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/GROUND COVER TOTAL AREA"),
                new PdfFormField(PdfValue.Lawn, "Lawn",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA")
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
                    "REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT"),
                new PdfFormField(PdfValue.Uid, "undefined_4.1", Uid2),
                new PdfFormField(PdfValue.ReportDate, "undefined_4.2", DateOfTheDay),
                new PdfFormField(PdfValue.Area, "Area", RegionArea),
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "Excel file/ the cel beside Total Green cover (m²)/1000000"),
                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "REVIT/softscape & shrubs schedule total Water demand / 1000"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees.0", TreesProposed),
                new PdfFormField(PdfValue.TotalTrees, "Proposed Trees.1", "SUM THE ABOVE OF FROM REVIT"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs.1",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/ EXISTING SHRUBS TOTAL AREA"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs.1.0",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/ PROPOSED SHRUBS TOTAL AREA"),
                new PdfFormField(PdfValue.TotalShrubs, "Proposed Shrubs.1.1", "sum of the above or from revit"),
                new PdfFormField(PdfValue.GroundCover, "0",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/GROUND COVER TOTAL AREA"),
                new PdfFormField(PdfValue.Lawn, "0_2",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA")
            });

        public static readonly PdfForm Roads = new PdfForm(
            "Projects Basic Data - Roads", 34, new[] { "NS", "ST", "MM" }, new[]
            {
                new PdfFormField(PdfValue.Uid, "UID", Uid2),
                new PdfFormField(PdfValue.ReportDate, "Report Date", DateOfTheDay),
                new PdfFormField(PdfValue.Row, "Row",
                    "EXCEL FILE \"Scope Validation 21072026\" /COLUMN O1 \"ROAD_WIDTH\""),
                new PdfFormField(PdfValue.Length, "Length",
                    "EXCEL FILE \"Scope Validation 21072026\" /COLUMN H1 \"ES_QUANTITY\""),

                // **THIS NOTE NAMES THE CANOPY CELL AND THE OTHER TWO FORMS NAME TOTAL GREEN
                // COVER**, for a field all three call Total areas to be greened. It is recorded
                // here exactly as the file holds it, so the check does not refuse the form over
                // the client's own copy and paste, and the disagreement is an open question in
                // the log rather than a number chosen quietly.
                new PdfFormField(PdfValue.TotalAreasToBeGreened, "Total areas to be greened",
                    "excel the cell on the right of  \"Total area covered by canopy \""),

                new PdfFormField(PdfValue.IrrigationWaterDemand, "Irrigation water demand",
                    "Revit / softscape & shrubs & lawn schedule / total water demand /1000"),
                new PdfFormField(PdfValue.ExistingTrees, "Existing Trees", TreesExisting),
                new PdfFormField(PdfValue.ProposedTrees, "Proposed Trees", TreesProposed),
                new PdfFormField(PdfValue.TotalTrees, "TOTAL trees", "sum of the above or from revit"),
                new PdfFormField(PdfValue.ExistingShrubs, "Existing Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ EXISTING SHRUBS  TOTAL AREA"),
                new PdfFormField(PdfValue.ProposedShrubs, "Proposed Shrubs Area (m²)",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/ PROPOSED SHRUBS  TOTAL AREA"),
                new PdfFormField(PdfValue.TotalShrubs, "TOTAL Shrubs Area (m²)",
                    "sum of the above or from revit"),
                new PdfFormField(PdfValue.GroundCover, "Ground Cover",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/GROUND COVER TOTAL AREA"),
                new PdfFormField(PdfValue.Lawn, "Lawn",
                    "REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS) TOTAL AREA")
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
