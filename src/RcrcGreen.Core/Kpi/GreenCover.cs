using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// A number this tool worked out rather than read, with every part it was worked out from.
    ///
    /// **The report prints the working beside the number**, so a person can check it against the
    /// workbook the moment Excel has opened and recalculated it. A computed number with no
    /// working is a number nobody can argue with, which is worse than no number.
    /// </summary>
    public sealed class ComputedValue
    {
        private ComputedValue(double value, bool computed, string working, string why)
        {
            Value = value;
            Computed = computed;
            Working = working ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public static ComputedValue Of(double value, string working)
        {
            return new ComputedValue(value, true, working, string.Empty);
        }

        public static ComputedValue Refused(string why)
        {
            return new ComputedValue(0.0, false, string.Empty, why);
        }

        public double Value { get; }

        public bool Computed { get; }

        /// <summary>The parts, in the order they were added, and what came out.</summary>
        public string Working { get; }

        /// <summary>Empty where it was computed. Never empty where it was not.</summary>
        public string Why { get; }
    }

    /// <summary>
    /// The two cells the workbook computes and the PDF wants, worked out from what this run
    /// wrote and read.
    ///
    /// ```
    /// Total Green cover = canopy + planting + lawn
    /// Percentage canopy = canopy / area
    /// ```
    ///
    /// Planting, lawn and area are values this run wrote into the workbook. Canopy is built by
    /// <see cref="CanopyArea"/> from the rows this run wrote a count into, using the workbook's
    /// own column formula.
    /// </summary>
    public static class GreenCover
    {
        public const string NoArea =
            "the plot has no area, so a canopy percentage would divide by nought";

        public static ComputedValue Total(CanopyTotal canopy, double plantingSquareMetres, double lawnSquareMetres)
        {
            if (canopy == null) throw new ArgumentNullException("canopy");

            double found = canopy.SquareMetres + plantingSquareMetres + lawnSquareMetres;

            return ComputedValue.Of(found,
                "canopy " + Number(canopy.SquareMetres)
                + " plus planting " + Number(plantingSquareMetres)
                + " plus lawn " + Number(lawnSquareMetres)
                + " is " + Number(found) + " square metres, off "
                + canopy.Rows.Count.ToString(CultureInfo.InvariantCulture)
                + (canopy.Rows.Count == 1 ? " tree row" : " tree rows"));
        }

        /// <summary>
        /// **The workbook's cell holds a RATIO and the form prints a percent sign in its unit
        /// column**, so the ratio is multiplied by 100 and no sign is written. Measured on the
        /// filled example ANH-006-NP-100002: area 771 and a canopy percentage of 71, where the
        /// ratio is 0.71.
        /// </summary>
        public static ComputedValue Percentage(CanopyTotal canopy, double areaSquareMetres)
        {
            if (canopy == null) throw new ArgumentNullException("canopy");
            if (areaSquareMetres <= 0.0) return ComputedValue.Refused(NoArea);

            double ratio = canopy.SquareMetres / areaSquareMetres;

            return ComputedValue.Of(ratio * 100.0,
                "canopy " + Number(canopy.SquareMetres) + " over area " + Number(areaSquareMetres)
                + " is " + ratio.ToString("0.######", CultureInfo.InvariantCulture)
                + ", written as " + Number(ratio * 100.0) + " because the form prints the sign");
        }

        private static string Number(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
