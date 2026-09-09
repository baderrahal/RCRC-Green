using System;
using System.Collections.Generic;
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

        /// <summary>
        /// The four plot parameters the sheet really carries, all with values on the first
        /// model. The report prints them side by side because the workbook asks for one Ref
        /// and nothing in a report showing only one of the four can say which it is.
        /// </summary>
        public const string PlotId = "PRX_Plot_ID";

        public const string PlotUid = "PRX_Plot_UID";

        public const string PlotNh = "PRX_Plot_NH";

        public static readonly string[] PlotNamesOnSheets = { PlotId, PlotUid, PlotUid2, PlotNh };

        /// <summary>
        /// The plot on a model element, and on a filled region in the 00 link. One plot's
        /// regions read together is what settles which region type is its intervention area.
        /// </summary>
        public const string RefPlotId = "PRX_Ref Plot ID";

        public static readonly string[] NeighbourhoodNearMisses = { "NEIGH", "DISTRICT", "COMMUNITY", "LOCATION", "ZONE" };

        public static readonly string[] InterventionNearMisses = { "INTERVENTION", "AREA" };

        /// <summary>
        /// A schedule whose name holds any of these is one the workbook draws from, so it gets
        /// its fields, filters and rows printed rather than only its name. TREE is here for the
        /// two tree quantity notes, and HARDSCAPE is a real schedule in this model.
        /// </summary>
        public static readonly string[] ScheduleWords = { "SOFTSCAPE", "SHRUB", "LAWN", "HARDSCAPE", "TREE" };

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

        /// <summary>
        /// The word the workbook puts before FILLED REGION. Looked for as a whole run of
        /// letters, because Solid Fill, Grid and Hidden all hold the letters and none of them
        /// is it, and Solid Fill is the filled region type every Revit template ships with.
        /// </summary>
        public const string RegionMark = "ID";

        /// <summary>
        /// On every phased element in Revit, so their presence separates nothing. They are
        /// still printed, and they never make question 8 count as answered.
        /// </summary>
        public static readonly string[] BuiltInPhaseNames = { "Phase Created", "Phase Demolished" };

        public static bool HoldsAny(string name, params string[] words)
        {
            if (string.IsNullOrEmpty(name) || words == null) return false;

            return words.Any(word => Holds(name, word));
        }

        /// <summary>
        /// A name holds a word when one of its runs of letters starts with the word, compared
        /// without case. So PRX_COMPONENTS holds COMPONENT and Neighbourhood holds NEIGH, while
        /// Solid Fill does not hold ID and Guide Grid does not hold UID. A word with no letter
        /// in it, such as the 00 of the link, is looked for anywhere in the name.
        /// </summary>
        public static bool Holds(string name, string word)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(word)) return false;

            if (!word.Any(char.IsLetter))
            {
                return CultureInfo.InvariantCulture.CompareInfo.IndexOf(name, word, CompareOptions.IgnoreCase) >= 0;
            }

            return LetterRuns(name).Any(run => run.StartsWith(word, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> LetterRuns(string name)
        {
            int start = -1;
            for (int at = 0; at <= name.Length; at++)
            {
                bool letter = at < name.Length && char.IsLetter(name[at]);
                if (letter && start < 0) start = at;
                if (!letter && start >= 0)
                {
                    yield return name.Substring(start, at - start);
                    start = -1;
                }
            }
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
