using System;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The exact names the KPI workbook notes point at, and the words a near miss is looked
    /// for under when an exact name is not there.
    ///
    /// The notes in the workbook cannot be matched against the model. One schedule is written
    /// two ways in the same file and COMPONENTS is misspelt in several notes, so the real names
    /// have to come from the model. These are only the names the scan goes looking for, and a
    /// NOT FOUND against any of them is a finding rather than a failure.
    /// </summary>
    public static class KpiNames
    {
        public const string Component = "PRX_COMPONENT";

        /// <summary>
        /// A third plot name. CLAUDE.md records PRX_Plot_ID on views and sheets and PRX_Ref Plot
        /// ID on elements, and this is neither, so nothing here assumes it is either.
        /// </summary>
        public const string PlotUid2 = "PRX_Plot_UID2";

        public const string InterventionArea = "PRX_Intervention Area";

        /// <summary>
        /// The two names the title block and sheet sections look for, in the order they print.
        /// </summary>
        public static readonly string[] OnSheets = { Component, PlotUid2 };

        public static readonly string[] SheetNearMisses = { "COMPONENT", "PLOT", "UID" };

        public static readonly string[] NeighbourhoodNearMisses = { "NEIGH", "DISTRICT", "COMMUNITY", "LOCATION", "ZONE" };

        public static readonly string[] InterventionNearMisses = { "INTERVENTION", "AREA" };

        /// <summary>
        /// A schedule whose name holds any of these is one the workbook draws from, so it gets
        /// its fields, filters and rows printed rather than only its name.
        /// </summary>
        public static readonly string[] ScheduleWords = { "SOFTSCAPE", "SHRUB", "LAWN", "HARDSCAPE" };

        public static readonly string[] SoftscapeWords = { "SOFTSCAPE" };

        /// <summary>
        /// The words a botanical name or a quantity is likely to hide under on a planting
        /// element. Question 7 is answered by listing the values of every parameter holding
        /// one of them, so the person reading picks the field and the tool does not.
        /// </summary>
        public static readonly string[] PlantingWords =
            { "BOTANIC", "LATIN", "SPECIES", "NAME", "QTY", "QUANT", "COUNT", "NUMBER", "SIZE", "TREE" };

        /// <summary>
        /// The words that might separate an existing tree from a proposed one.
        /// </summary>
        public static readonly string[] StatusWords =
            { "EXIST", "PROPOS", "STATUS", "RETAIN", "REMOV", "NEW", "PHASE", "CONDITION" };

        /// <summary>
        /// The link the workbook calls REVIT 00 LINK. Nothing more is known about it than that
        /// its name holds these two characters.
        /// </summary>
        public const string LinkMark = "00";

        public static bool HoldsAny(string name, params string[] words)
        {
            if (string.IsNullOrEmpty(name) || words == null) return false;

            return words.Any(word => !string.IsNullOrEmpty(word)
                && CultureInfo.InvariantCulture.CompareInfo.IndexOf(name, word, CompareOptions.IgnoreCase) >= 0);
        }

        public static bool Holds(string name, string word)
        {
            return HoldsAny(name, word);
        }

        /// <summary>
        /// The words in the name, so a report line can say why a schedule was picked out.
        /// </summary>
        public static string WordsIn(string name, params string[] words)
        {
            if (words == null) return string.Empty;

            return string.Join(", ", words.Where(word => Holds(name, word)).ToArray());
        }
    }
}
