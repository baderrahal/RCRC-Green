using System;
using System.Collections.Generic;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the STREETS area cell holds, picked in Setup.
    /// </summary>
    public enum StreetsH8Option
    {
        /// <summary>The Revit area off the 00 link, what every press wrote before this option.</summary>
        InterventionArea,

        /// <summary>The cell holds the road width times the length, as a formula.</summary>
        RowTimesLength
    }

    /// <summary>
    /// **THE TWO WAYS TO FILL THE STREETS AREA CELL**, Bader's decision of 8 November, K79. The
    /// default is the Revit area, so no workbook changes for anybody who does not pick. The formula's cells
    /// come off the template's map like every other cell. The report's words name H8 for the
    /// reader, and nothing decides a cell off those words.
    /// </summary>
    public static class StreetsH8
    {
        // What the settings file holds. The pane's captions are KpiPaneWords' own, so rewording
        // one cannot turn every saved pick back into Option 1.
        public const string InterventionAreaName = "Intervention area";

        public const string RowTimesLengthName = "ROW x Length";

        /// <summary>
        /// A file that is missing, unreadable or says anything else is Option 1, Bader's default.
        /// </summary>
        public static StreetsH8Option Parse(string text)
        {
            return string.Equals((text ?? string.Empty).Trim(), RowTimesLengthName, StringComparison.OrdinalIgnoreCase)
                ? StreetsH8Option.RowTimesLength
                : StreetsH8Option.InterventionArea;
        }

        public static string Name(StreetsH8Option option)
        {
            return option == StreetsH8Option.RowTimesLength ? RowTimesLengthName : InterventionAreaName;
        }

        /// <summary>
        /// Whether this template holds all three cells of the row. The option means nothing on
        /// a template that does not, and only STREETS does.
        /// </summary>
        public static bool HasTheCells(KpiTemplate template)
        {
            return template != null
                && template.CellFor(KpiValue.Area) != null
                && template.CellFor(KpiValue.StreetsRoadWidth) != null
                && template.CellFor(KpiValue.StreetsTotalLength) != null;
        }

        /// <summary>The option this template runs under, Option 1 on every other template.</summary>
        public static StreetsH8Option Effective(KpiTemplate template, StreetsH8Option picked)
        {
            return picked == StreetsH8Option.RowTimesLength && HasTheCells(template)
                ? StreetsH8Option.RowTimesLength
                : StreetsH8Option.InterventionArea;
        }

        /// <summary>
        /// Whether a press reads the plot's filled regions. **Under ROW x Length nothing asks
        /// for one**, so a street plot is never refused over two regions holding an area.
        /// </summary>
        public static bool ReadsRegions(KpiTemplate template, StreetsH8Option picked)
        {
            if (template == null) throw new ArgumentNullException("template");

            return !template.TakesNoArea && Effective(template, picked) != StreetsH8Option.RowTimesLength;
        }

        /// <summary>
        /// The formula, the length cell times the width cell, in the order the client's sheet
        /// used to write it. Held without the equals sign, which a formula part does not carry.
        /// </summary>
        public static string Formula(KpiTemplate template)
        {
            if (!HasTheCells(template)) throw new ArgumentException("This template has no such row.", "template");

            return template.CellFor(KpiValue.StreetsTotalLength).Cell + "*" + template.CellFor(KpiValue.StreetsRoadWidth).Cell;
        }

        /// <summary>
        /// The area every number this tool works out takes: D8 x F8 under ROW x Length, the
        /// same number Excel shows, or the Revit area as before.
        ///
        /// **Nothing is taken from Revit where the street file has no usable answer**, so that
        /// plot's total is empty. The Roads PDF these plots take has no Area box at all, only
        /// Row and Length, so nothing on it reads this number.
        /// </summary>
        public static Totalled Area(string plotId, IEnumerable<PlotReading> readings, StreetReferenceAnswer street)
        {
            if (street == null || !street.UsesRowTimesLength) return KpiMerge.Area(readings);

            return street.AreaWorkedOut
                ? Totalled.Adding(new[] { new PlotNumber(plotId, street.RowTimesLength) })
                : Totalled.Nothing;
        }

        /// <summary>The report's line naming the option the press ran under.</summary>
        public static string InWords(StreetsH8Option option)
        {
            return option == StreetsH8Option.RowTimesLength
                ? "STREETS H8: ROW x Length, the formula =" + Formula(KpiTemplates.Streets)
                    + ". A street plot's area is D8 x F8, never the Revit area, and no filled region "
                    + "is read for it. Other templates are unchanged."
                : "STREETS H8: Intervention area, the Revit area off the 00 link.";
        }

        /// <summary>
        /// One plot's area in the report's words, which says where it came from or names what
        /// is missing, in one line.
        /// </summary>
        public static string AreaInWords(StreetReferenceAnswer street)
        {
            if (street == null) return string.Empty;

            // Said of the formula and not of a file, because a refused plot writes no workbook.
            string cell = "The H8 formula is =" + Formula(KpiTemplates.Streets);

            return street.AreaWorkedOut
                ? "from ROW x Length, " + Number(street.Width) + " x " + Number(street.Length) + " = "
                    + Number(street.RowTimesLength) + " m2. " + cell + " and no filled region was read."
                : "ROW x Length could not be worked out, " + street.WhyNoArea + ". " + cell
                    + " and nothing was taken from Revit.";
        }

        private static string Number(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
