using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view, where it lands, and the cell it lands in.
    /// </summary>
    public sealed class PlacedView
    {
        internal PlacedView(
            ViewOnPaper view, ViewportSpot spot, double cellWidthFeet, double cellHeightFeet)
        {
            View = view;
            Spot = spot;
            CellWidthFeet = cellWidthFeet;
            CellHeightFeet = cellHeightFeet;
        }

        public ViewOnPaper View { get; }

        /// <summary>
        /// The centre the viewport is created at, measured from the sheet origin. This is the
        /// number the run hands Revit and the number a drawing of the sheet reads off.
        /// </summary>
        public ViewportSpot Spot { get; }

        public double CellWidthFeet { get; }

        public double CellHeightFeet { get; }

        /// <summary>
        /// True for a view measured and bigger than the cell it is going in. It is placed
        /// rather than dropped, on a sheet of its own so it runs over nothing, and said.
        /// </summary>
        public bool BiggerThanItsCell
        {
            get { return View.Measured && !View.FitsIn(CellWidthFeet, CellHeightFeet); }
        }
    }

    /// <summary>
    /// One sheet's worth: which views go on it and where each one lands.
    /// </summary>
    public sealed class PlacedSheet
    {
        internal PlacedSheet(
            IReadOnlyList<PlacedView> views,
            DrawingArea area,
            int viewsPerSheet,
            bool carriedOver,
            bool holdsOneTooBig)
        {
            Views = views;
            Area = area;
            ViewsPerSheet = viewsPerSheet;
            CarriedOver = carriedOver;
            HoldsOneTooBig = holdsOneTooBig;
        }

        public IReadOnlyList<PlacedView> Views { get; }

        public DrawingArea Area { get; }

        public int ViewsPerSheet { get; }

        /// <summary>
        /// True for a sheet that exists because the one before it filled up. It is the mark
        /// the preview puts on a card and the line the run report prints.
        /// </summary>
        public bool CarriedOver { get; }

        public bool HoldsOneTooBig { get; }

        /// <summary>
        /// True when any view on it has no measured size, which is every view the run has yet
        /// to create and every schedule. A sheet drawn from this says so rather than drawing a
        /// rectangle that looks measured.
        /// </summary>
        public bool AnythingNotMeasured
        {
            get { return Views.Any(one => !one.View.Measured); }
        }
    }

    /// <summary>
    /// Where every view on a sheet definition lands: the fit and the spots, together, once.
    ///
    /// The run worked this out in two calls, <see cref="SheetFit"/> then
    /// <see cref="SheetLayout"/>, and the preview would have been a third place asking the
    /// same two questions. **One set of positions, read by the run and by the preview.** A
    /// preview that laid the sheet out its own way would agree with the run on the day it was
    /// written and drift from it the first time either changed, and a drawing of a sheet that
    /// is not the sheet is worse than no drawing.
    ///
    /// It computes nothing of its own. The division is <see cref="SheetFit"/>'s and the
    /// centres are <see cref="SheetLayout"/>'s, and this pairs them up so a caller cannot hold
    /// one without the other.
    /// </summary>
    public static class SheetPlacement
    {
        /// <summary>
        /// The sheets the views need, each with its views at their spots.
        ///
        /// A view whose size could not be read is an ordinary case rather than a fault. Every
        /// view the run is about to create has no size yet, and so does every schedule, so
        /// pass <see cref="ViewOnPaper.NotMeasured"/> for those and they are laid out and
        /// marked rather than guessed at.
        /// </summary>
        public static IReadOnlyList<PlacedSheet> Of(
            DrawingArea area, int viewsPerSheet, IEnumerable<ViewOnPaper> views)
        {
            if (area == null) throw new ArgumentNullException("area");

            int each = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;

            IReadOnlyList<FittedSheet> fitted = SheetFit.Of(area, each, views);
            IReadOnlyList<ViewportSpot> spots = SheetLayout.For(area, each);

            int across = each == 4 ? 2 : 1;
            int down = each == 1 ? 1 : 2;
            double cellWidth = area.WidthFeet / across;
            double cellHeight = area.HeightFeet / down;

            var sheets = new List<PlacedSheet>(fitted.Count);

            foreach (FittedSheet one in fitted)
            {
                var placed = new List<PlacedView>(one.Views.Count);

                for (int at = 0; at < one.Views.Count; at++)
                {
                    // The fit never puts more on a sheet than the layout lays out, so this
                    // cannot run off the end. It is asked rather than assumed, because the run
                    // already reported that case once as a bug in the fit.
                    if (at >= spots.Count) break;

                    placed.Add(new PlacedView(one.Views[at], spots[at], cellWidth, cellHeight));
                }

                sheets.Add(new PlacedSheet(
                    placed, area, each, one.CarriedOver, one.HoldsOneTooBig));
            }

            return sheets;
        }

        /// <summary>
        /// The sheet a definition with no views makes: one, empty, the way a title sheet is.
        /// It has a drawing area and no view in it, so a drawing of it is the outline and the
        /// strip and nothing else.
        /// </summary>
        public static IReadOnlyList<PlacedSheet> Empty(DrawingArea area, int viewsPerSheet)
        {
            if (area == null) throw new ArgumentNullException("area");

            return new List<PlacedSheet>
            {
                new PlacedSheet(
                    new List<PlacedView>(),
                    area,
                    SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1,
                    false,
                    false)
            };
        }
    }
}
