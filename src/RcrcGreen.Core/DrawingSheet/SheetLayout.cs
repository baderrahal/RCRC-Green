using System;
using System.Collections.Generic;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where one viewport goes on a sheet, measured from the sheet origin in the same unit the
    /// title block was given in.
    /// </summary>
    public sealed class ViewportSpot
    {
        internal ViewportSpot(double centreX, double centreY)
        {
            CentreX = centreX;
            CentreY = centreY;
        }

        public double CentreX { get; }

        public double CentreY { get; }

        public override string ToString()
        {
            return CentreX.ToString("0.###", CultureInfo.InvariantCulture) + " "
                + CentreY.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Where the views sit on a sheet, given the area they may sit in and how many go on it.
    ///
    /// One is centred. TWO SIT ONE ABOVE THE OTHER. Four make a two by two grid. Every spot
    /// is the centre of one cell of an even division of that area, so the margins round the
    /// outside and the gap between are the same measurement.
    ///
    /// Two used to sit side by side, and on a real sheet the two views overlapped, because
    /// each was wider than half the drawing area. A landscape sheet is wider than it is
    /// tall and the drawings on it are wider than they are tall as well, so halving the
    /// width is the cut that runs out first. Halving the height is the one that fits.
    /// Four keeps the two by two grid, because halving both is the only way to make four
    /// cells at all.
    ///
    /// The sheet used to be copied from one the user had already laid out, which meant reading
    /// a position off an existing viewport. It is worked out here now, so a plot can be given a
    /// sheet without one already existing to copy.
    ///
    /// **It divides the drawing area and not the whole sheet.** Dividing the whole title block
    /// put wide views across the title strip down the right hand edge, which every schedule the
    /// first real run placed showed. What the area is, and whose measurement the strip is, are
    /// in <see cref="DrawingArea"/>.
    /// </summary>
    public static class SheetLayout
    {
        /// <summary>
        /// The only counts a sheet can hold. Three would leave a hole, and anything above four
        /// has not been asked for and would be a rule invented here.
        /// </summary>
        public static readonly IReadOnlyList<int> Counts = new List<int> { 1, 2, 4 };

        public static bool IsACount(int howMany)
        {
            return howMany == 1 || howMany == 2 || howMany == 4;
        }

        /// <summary>
        /// The centres, in reading order: left to right, then top to bottom. A caller placing
        /// views in the order the user ticked them gets them in that order on the sheet, so
        /// the first of two is the top one.
        ///
        /// Every spot is measured from the sheet origin rather than from the corner of the area,
        /// because that is what Revit wants and what the report prints.
        /// </summary>
        /// <param name="area">The part of the sheet a view may sit in.</param>
        /// <param name="howMany">1, 2 or 4.</param>
        public static IReadOnlyList<ViewportSpot> For(DrawingArea area, int howMany)
        {
            if (area == null) throw new ArgumentNullException("area");

            if (!IsACount(howMany))
            {
                throw new ArgumentOutOfRangeException(
                    "howMany", howMany, "A sheet holds 1, 2 or 4 views.");
            }

            int across = howMany == 4 ? 2 : 1;
            int down = howMany == 1 ? 1 : 2;

            var spots = new List<ViewportSpot>(howMany);

            // The centre of each cell of an even division. Halving the cell and stepping by a
            // whole one is what leaves the same margin outside as the gap between, rather than
            // a wide border on one side and none on the other.
            double cellWidth = area.WidthFeet / across;
            double cellHeight = area.HeightFeet / down;

            for (int row = 0; row < down; row++)
            {
                for (int column = 0; column < across; column++)
                {
                    spots.Add(new ViewportSpot(
                        area.LeftFeet + (cellWidth * column) + (cellWidth / 2.0),

                        // Y counts up from the bottom of the sheet in Revit, and the spots come
                        // back in reading order, so the first row is the top one.
                        area.BottomFeet + area.HeightFeet
                            - ((cellHeight * row) + (cellHeight / 2.0))));
                }
            }

            return spots;
        }
    }
}
