using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The map: seven templates, one per asset type, each cell measured off the annotated set.
    ///
    /// The production templates the team fills carry no note saying where a value comes from,
    /// measured at zero note cells in all seven. A separate annotated set holds the mapping in
    /// green text and is not what the team fills, so the tool carries the map as data and
    /// never reads it out of a workbook. The team decided that after comparing the two sets.
    /// </summary>
    public static class KpiTemplates
    {
        /// <summary>
        /// Every main sheet name is wrapped in angle brackets, and the brackets are part of the
        /// name rather than placeholder notation. They were stripped when the map was first
        /// written, so all seven workbooks came back unrecognised on the first real folder.
        /// </summary>
        public const string MainSheetOpens = "<";

        public const string MainSheetCloses = ">";

        public const string ExistingTreesSheet = "Tree List - Existing";

        public const string ProposedTreesSheet = "Tree List - Proposed";

        public const string CriteriaSheet = "Criteria";

        public const string GreenStrategySheet = "Green Strategy KPI's";

        /// <summary>
        /// On both tree list sheets: the header is row 3, the botanical name is read from column
        /// D and the quantity is written into column B. The row under the header is where the
        /// list is read down from, and it is the one number about a tree list the map still
        /// holds, measured at row 3 on all seven templates. Where the names stop and which rows
        /// the total reaches are read off the file and are in no map.
        /// </summary>
        public const string QuantityColumn = "B";

        public const string BotanicalColumn = "D";

        public const int TreeHeaderRow = 3;

        /// <summary>
        /// The same in every template and not from Revit: E5 the date, G5 the person, H5
        /// their position. The team types them on the pane and the tool copies them through,
        /// so a filled checklist carries who filled it and when. Nothing reads them from a
        /// model, which is why they are here rather than in a template's mapped cells.
        /// </summary>
        public static readonly string[] TypedByTheTeam = { "E5", "G5", "H5" };

        // What a template holds in the cells the team fills is not one thing, measured on two
        // sets, so no placeholder is recorded here and none decides anything. The rule that
        // tells a filled checklist from a template is in FilledMarks, next to the measurement.

        public static readonly KpiTemplate ExistingParks = Parks("EXISTING PARKS");

        public static readonly KpiTemplate FutureParks = Parks("FUTURE PARKS");

        public static readonly KpiTemplate Healthcare = Standard("HEALTHCARE", "<Healthcare>");

        public static readonly KpiTemplate Mosques = Standard("MOSQUES", "<Mosques>");

        public static readonly KpiTemplate Parking = Standard("PARKING", "<Parking Plots>");

        public static readonly KpiTemplate Schools = Standard("SCHOOLS", "<Schools>");

        /// <summary>
        /// The one group a template counts by decision rather than by a sheet name. ST-05
        /// prints Existing 369, Proposed 2 and Street Design 68 under a TOTAL of 439, and on a
        /// street its Street Design trees are its own proposed trees. Bader's decision, and it
        /// holds on STREETS only.
        /// </summary>
        public const string StreetDesignGroup = "Street Design";

        public static readonly KpiTemplate Streets = new KpiTemplate(
            "STREETS",
            "<Streets>",
            new[]
            {
                new MappedCell(KpiValue.Component, "D3"),
                new MappedCell(KpiValue.Reference, "C5"),
                new MappedCell(KpiValue.Location, "E4"),
                new MappedCell(KpiValue.Shrubs, "F11"),
                new MappedCell(KpiValue.Lawn, "H11")
            },
            new TreeSheet(ExistingTreesSheet),
            new TreeSheet(ProposedTreesSheet),
            new[] { StreetDesignGroup });

        public static readonly IReadOnlyList<KpiTemplate> All = new[]
        {
            ExistingParks, FutureParks, Healthcare, Mosques, Parking, Schools, Streets
        };

        /// <summary>
        /// EXISTING PARKS and FUTURE PARKS share the Park Name sheet and every cell, so the
        /// sheet name cannot tell them apart and the file name breaks the tie.
        /// </summary>
        private static KpiTemplate Parks(string name)
        {
            return new KpiTemplate(
                name,
                "<Park Name>",
                new[]
                {
                    new MappedCell(KpiValue.Component, "D3"),
                    new MappedCell(KpiValue.Reference, "C5"),
                    new MappedCell(KpiValue.Location, "E4"),
                    new MappedCell(KpiValue.Area, "D8"),
                    new MappedCell(KpiValue.Shrubs, "F11"),
                    new MappedCell(KpiValue.Lawn, "H11")
                },
                new TreeSheet(ExistingTreesSheet),
                new TreeSheet(ProposedTreesSheet));
        }

        private static KpiTemplate Standard(string name, string mainSheetName)
        {
            return new KpiTemplate(
                name,
                mainSheetName,
                new[]
                {
                    new MappedCell(KpiValue.Component, "D3"),
                    new MappedCell(KpiValue.Reference, "C5"),
                    new MappedCell(KpiValue.Location, "E4"),
                    new MappedCell(KpiValue.Area, "H7"),
                    new MappedCell(KpiValue.Shrubs, "F10"),
                    new MappedCell(KpiValue.Lawn, "H10")
                },
                new TreeSheet(ExistingTreesSheet),
                new TreeSheet(ProposedTreesSheet));
        }

        /// <summary>
        /// Where each value really comes from, as the pane says it beside the cell.
        ///
        /// **This used to be the workbook's note shown as if it were the tool's behaviour.** It
        /// read PRX_COMPONENT and PRX_Plot_UID2 read off the title block, and all three parts of
        /// that were wrong. PRX_COMPONENT exists nowhere in the model, the value is PRX_Component
        /// on the sheet, and PRX_Plot_UID2 sits on the title block on 1233 instances holding a
        /// value on none of them while the sheet holds 1384 of them. The reader was corrected
        /// rounds ago and this text was not.
        ///
        /// The first three name the parameter the user picked on the pane, because that is the
        /// one that will be read.
        /// </summary>
        public static string SourceOf(KpiValue value, ChosenParameters chosen)
        {
            ChosenParameters picked = chosen ?? ChosenParameters.NonePicked;

            switch (value)
            {
                case KpiValue.Component:
                    return Picked(picked.Component, "Component") + ", read off the plot's first sheet";
                case KpiValue.Reference:
                    return Picked(picked.Reference, "Reference") + ", read off the plot's first sheet";
                case KpiValue.Location:
                    return Picked(picked.Location, "Location") + ", read off Project Information";
                case KpiValue.Area:
                    return "PRX_Intervention Area, totalled off the chosen filled regions in the 00 link";
                case KpiValue.Shrubs:
                    return "SHRUBS & GROUND COVER TOTAL AREA from the shrubs and lawn schedule";
                default:
                    return "LAWN (GRASS) TOTAL AREA from the shrubs and lawn schedule";
            }
        }

        /// <summary>
        /// The chosen parameter by name, or the picker to look at when nothing is chosen yet.
        /// Never a name nobody picked.
        /// </summary>
        private static string Picked(string name, string picker)
        {
            return string.IsNullOrWhiteSpace(name)
                ? "the parameter picked under " + picker
                : name;
        }
    }
}
