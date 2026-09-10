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
    /// Where the views sit on a sheet, given the title block's size and how many go on it.
    ///
    /// One is centred. Two sit side by side. Four make a two by two grid. Every spot is the
    /// centre of one cell of an even division of the sheet, so the margins round the outside
    /// and the gap down the middle are the same measurement.
    ///
    /// The sheet used to be copied from one the user had already laid out, which meant reading
    /// a position off an existing viewport. It is worked out here now, so a plot can be given a
    /// sheet without one already existing to copy.
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
        /// views in the order the user ticked them gets them in that order on the sheet.
        /// </summary>
        /// <param name="width">The title block's width, greater than zero.</param>
        /// <param name="height">The title block's height, greater than zero.</param>
        /// <param name="howMany">1, 2 or 4.</param>
        public static IReadOnlyList<ViewportSpot> For(double width, double height, int howMany)
        {
            if (!IsACount(howMany))
            {
                throw new ArgumentOutOfRangeException(
                    "howMany", howMany, "A sheet holds 1, 2 or 4 views.");
            }

            RequireASize(width, "width");
            RequireASize(height, "height");

            int across = howMany == 1 ? 1 : 2;
            int down = howMany == 4 ? 2 : 1;

            var spots = new List<ViewportSpot>(howMany);

            // The centre of each cell of an even division. Halving the cell and stepping by a
            // whole one is what leaves the same margin outside as the gap between, rather than
            // a wide border on one side and none on the other.
            double cellWidth = width / across;
            double cellHeight = height / down;

            for (int row = 0; row < down; row++)
            {
                for (int column = 0; column < across; column++)
                {
                    spots.Add(new ViewportSpot(
                        (cellWidth * column) + (cellWidth / 2.0),

                        // Y counts up from the bottom of the sheet in Revit, and the spots come
                        // back in reading order, so the first row is the top one.
                        height - ((cellHeight * row) + (cellHeight / 2.0))));
                }
            }

            return spots;
        }

        private static void RequireASize(double what, string named)
        {
            if (double.IsNaN(what) || double.IsInfinity(what))
            {
                throw new ArgumentException("The " + named + " is not a real length.", named);
            }

            if (what <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    named, what, "A title block with no " + named + " has nowhere to put a view.");
            }
        }
    }
}
