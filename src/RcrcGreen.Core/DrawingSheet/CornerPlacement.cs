using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// How far something has to move to put its centre where it was asked for.
    ///
    /// **A viewport is placed by its centre and a schedule on a sheet is placed by its top left
    /// corner.** Both were handed the centre this code works out, so on the first real run every
    /// plan view landed correctly and every schedule landed half its own size to the right and
    /// half its own size down. Measured on 010QA: a schedule 207.4 by 187.4 mm was asked for a
    /// centre at 420.5 by 297.0 and its centre read 522.1 by 203.3, which is 93.7 mm low, exactly
    /// half its height.
    ///
    /// Everything here stays in centres. The correction is made once, on the Revit side, after
    /// the placement, because how big a schedule comes out on a sheet is not known until Revit
    /// has drawn it. Measuring where it landed also means nothing here has to assume which
    /// corner Revit used.
    /// </summary>
    public static class CornerPlacement
    {
        /// <summary>
        /// The move that takes a placement from where its centre landed to where its centre was
        /// wanted. The unit is whatever the two were given in, since a difference carries the
        /// unit of its parts.
        /// </summary>
        public static SheetMove MoveToPutTheCentreAt(
            double landedCentreX, double landedCentreY, double wantedCentreX, double wantedCentreY)
        {
            RequireANumber(landedCentreX, "landedCentreX");
            RequireANumber(landedCentreY, "landedCentreY");
            RequireANumber(wantedCentreX, "wantedCentreX");
            RequireANumber(wantedCentreY, "wantedCentreY");

            return new SheetMove(wantedCentreX - landedCentreX, wantedCentreY - landedCentreY);
        }

        private static void RequireANumber(double what, string named)
        {
            if (double.IsNaN(what) || double.IsInfinity(what))
            {
                throw new ArgumentException("The " + named + " is not a real length.", named);
            }
        }
    }

    /// <summary>
    /// How far across and how far up, in the unit the centres were given in. Y counts up from
    /// the bottom of the sheet, which is Revit's own convention, so a placement sitting too low
    /// moves up by a positive number.
    /// </summary>
    public sealed class SheetMove
    {
        internal SheetMove(double across, double up)
        {
            Across = across;
            Up = up;
        }

        public double Across { get; }

        public double Up { get; }

        /// <summary>
        /// Nothing to move. Worth asking rather than moving by nought, because a move Revit does
        /// not need is a regeneration nobody needs either.
        /// </summary>
        public bool Nowhere
        {
            get { return Across == 0.0 && Up == 0.0; }
        }

        public override string ToString()
        {
            return Across.ToString("0.###", CultureInfo.InvariantCulture) + " across, "
                + Up.ToString("0.###", CultureInfo.InvariantCulture) + " up";
        }
    }
}
