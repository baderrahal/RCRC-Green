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
        /// On both tree list sheets: the header is row 3, data runs from row 4, the botanical
        /// name is read from column D and the quantity is written into column B.
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

        public static readonly KpiTemplate ExistingParks = Parks("EXISTING PARKS");

        public static readonly KpiTemplate FutureParks = Parks("FUTURE PARKS");

        public static readonly KpiTemplate Healthcare = Standard("HEALTHCARE", "<Healthcare>");

        public static readonly KpiTemplate Mosques = Standard("MOSQUES", "<Mosques>");

        public static readonly KpiTemplate Parking = Standard("PARKING", "<Parking Plots>");

        public static readonly KpiTemplate Schools = Standard("SCHOOLS", "<Schools>");

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
            new TreeRows(ExistingTreesSheet, 4, 83),
            new TreeRows(ProposedTreesSheet, 4, 83));

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
                new TreeRows(ExistingTreesSheet, 4, 92),
                new TreeRows(ProposedTreesSheet, 4, 84));
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
                new TreeRows(ExistingTreesSheet, 4, 83),
                new TreeRows(ProposedTreesSheet, 4, 83));
        }

        /// <summary>
        /// Where each value comes from, as the pane says it beside the cell. The words are the
        /// map's second half: a person checks D3 against PRX_COMPONENT, not against a guess.
        /// </summary>
        public static string SourceOf(KpiValue value)
        {
            switch (value)
            {
                case KpiValue.Component: return "PRX_COMPONENT, read off the title block";
                case KpiValue.Reference: return "PRX_Plot_UID2, read off the title block";
                case KpiValue.Location: return "the neighbourhood name from Project Information";
                case KpiValue.Area: return "PRX_Intervention Area, totalled off the 00 link's filled regions";
                case KpiValue.Shrubs: return "SHRUBS & GROUND COVER TOTAL AREA from the shrubs and lawn schedule";
                default: return "LAWN (GRASS) TOTAL AREA from the shrubs and lawn schedule";
            }
        }
    }
}
