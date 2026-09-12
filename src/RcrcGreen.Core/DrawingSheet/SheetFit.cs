using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view's size on paper, in the same unit the drawing area is in.
    ///
    /// Not every view can be measured before it is placed. A plan or a section has an
    /// outline Revit will give for the asking, and a schedule's size is not known until
    /// Revit has drawn it. An unmeasured view is carried as one, so the report can say
    /// which sizes went into the fit rather than presenting a guess as a measurement.
    /// </summary>
    public sealed class ViewOnPaper
    {
        public ViewOnPaper(string viewName, double widthFeet, double heightFeet)
        {
            ViewName = viewName ?? string.Empty;
            WidthFeet = widthFeet;
            HeightFeet = heightFeet;
            Measured = IsReal(widthFeet) && IsReal(heightFeet);
        }

        /// <summary>
        /// A view whose size could not be read. It fits whatever it is put in, because
        /// nothing here knows any better, and it is named wherever that matters.
        /// </summary>
        public static ViewOnPaper NotMeasured(string viewName)
        {
            return new ViewOnPaper(viewName, 0.0, 0.0);
        }

        public string ViewName { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        public bool Measured { get; }

        /// <summary>
        /// Whether this view fits a cell of the given size. An unmeasured one says yes,
        /// which is the only answer available and is why the report names it.
        /// </summary>
        public bool FitsIn(double cellWidthFeet, double cellHeightFeet)
        {
            if (!Measured) return true;

            return WidthFeet <= cellWidthFeet && HeightFeet <= cellHeightFeet;
        }

        public string SizeInWords()
        {
            if (!Measured) return "size not read";

            return Lengths.InMillimetres(WidthFeet).ToString("0.#", CultureInfo.InvariantCulture)
                + " by "
                + Lengths.InMillimetres(HeightFeet).ToString("0.#", CultureInfo.InvariantCulture)
                + " mm";
        }

        private static bool IsReal(double length)
        {
            return !double.IsNaN(length) && !double.IsInfinity(length) && length > 0.0;
        }
    }

    /// <summary>
    /// One sheet the fit decided on: which of the asked for views go on it, in order, and
    /// whether it holds a view too big for the cell it is in.
    /// </summary>
    public sealed class FittedSheet
    {
        internal FittedSheet(IReadOnlyList<ViewOnPaper> views, bool overflowed, bool tooBig)
        {
            Views = views;
            CarriedOver = overflowed;
            HoldsOneTooBig = tooBig;
        }

        public IReadOnlyList<ViewOnPaper> Views { get; }

        /// <summary>
        /// True when this sheet exists because the one before it was full. The report names
        /// every one of these, because the user described one sheet and got two.
        /// </summary>
        public bool CarriedOver { get; }

        /// <summary>
        /// True when a view on this sheet is bigger than the cell it will be placed in. It
        /// is still placed, alone on the sheet, because leaving it off would lose it and
        /// nothing else is there for it to cover.
        /// </summary>
        public bool HoldsOneTooBig { get; }
    }

    /// <summary>
    /// Which views really fit on a sheet, and what to do with the ones that do not.
    ///
    /// Two faults on a real sheet, both from placing views in cells without ever asking
    /// whether they fit. Two views laid side by side overlapped, because each was wider
    /// than half the drawing area, and that is what stacking them fixed. A view too big for
    /// its cell was placed anyway and ran over its neighbour.
    ///
    /// The rule: views in the order they were ticked, each into the next cell of the sheet,
    /// and a view that does not fit that cell starts a fresh sheet instead. Nothing is ever
    /// dropped. A view that does not fit even on a fresh sheet goes on one of its own with
    /// nothing beside it, so it overlaps nothing, and it is named.
    /// </summary>
    public static class SheetFit
    {
        public static IReadOnlyList<FittedSheet> Of(
            DrawingArea area, int viewsPerSheet, IEnumerable<ViewOnPaper> views)
        {
            if (area == null) throw new ArgumentNullException("area");

            int each = SheetLayout.IsACount(viewsPerSheet) ? viewsPerSheet : 1;

            List<ViewOnPaper> wanted = (views ?? Enumerable.Empty<ViewOnPaper>())
                .Where(one => one != null)
                .ToList();

            var sheets = new List<FittedSheet>();
            if (wanted.Count == 0) return sheets;

            // The cells are the same division SheetLayout hands back spots for, so what is
            // measured against is what the view is really placed in. Two rules here would
            // mean the fit and the placement were different maths, which is the shape this
            // repo keeps paying for.
            int across = each == 4 ? 2 : 1;
            int down = each == 1 ? 1 : 2;
            double cellWidth = area.WidthFeet / across;
            double cellHeight = area.HeightFeet / down;

            var onThisSheet = new List<ViewOnPaper>();
            bool carried = false;
            bool tooBig = false;

            foreach (ViewOnPaper view in wanted)
            {
                bool fits = view.FitsIn(cellWidth, cellHeight);

                if (!fits && onThisSheet.Count > 0)
                {
                    sheets.Add(new FittedSheet(onThisSheet, carried, tooBig));
                    onThisSheet = new List<ViewOnPaper>();
                    carried = true;
                    tooBig = false;

                    // A fresh sheet of its own. Whether it fits there is asked again below,
                    // because a sheet holding one view still divides into the same cells.
                    fits = view.FitsIn(cellWidth, cellHeight);
                }

                onThisSheet.Add(view);

                if (!fits)
                {
                    // Too big for a cell even on a sheet of its own. It is placed rather
                    // than dropped, and it is the only thing on the sheet, so it runs over
                    // nothing.
                    tooBig = true;
                    sheets.Add(new FittedSheet(onThisSheet, carried, true));
                    onThisSheet = new List<ViewOnPaper>();
                    carried = true;
                    tooBig = false;
                    continue;
                }

                if (onThisSheet.Count == each)
                {
                    sheets.Add(new FittedSheet(onThisSheet, carried, tooBig));
                    onThisSheet = new List<ViewOnPaper>();
                    carried = true;
                    tooBig = false;
                }
            }

            if (onThisSheet.Count > 0)
            {
                sheets.Add(new FittedSheet(onThisSheet, carried, tooBig));
            }

            return sheets;
        }

        /// <summary>
        /// What the report says about one sheet: how many views it was asked for and how
        /// many of them fitted on it. The two differ whenever a sheet was carried over,
        /// which is the thing somebody reading the report needs to see.
        /// </summary>
        public static string InWords(int asked, int fitted)
        {
            if (asked == fitted)
            {
                return asked + (asked == 1 ? " view was asked for and it fitted."
                    : " views were asked for and all of them fitted.");
            }

            return asked + (asked == 1 ? " view was" : " views were") + " asked for and "
                + fitted + (fitted == 1 ? " fitted on this sheet." : " fitted on this sheet.")
                + " The rest are on a sheet of their own.";
        }

        /// <summary>
        /// Said against a sheet the fit created, so a number nobody typed is explained
        /// where it appears rather than looking like a sheet the user forgot describing.
        /// </summary>
        public static string CarriedOverInWords(string fromSheetNumber)
        {
            return "This sheet was made because " + fromSheetNumber + " was full. The views "
                + "that did not fit at their own scale are on it rather than being dropped "
                + "or laid over one another.";
        }

        /// <summary>
        /// Said against a view bigger than the cell it will sit in, which is placed alone
        /// rather than left off.
        ///
        /// A sheet holding fewer views than its count still lays them out in the cells of
        /// that count, so a view too big for its cell is alone on the sheet and covers
        /// nothing, but it can still reach past the cell it was given.
        /// </summary>
        public static string TooBigInWords(string viewName, string size, int viewsPerSheet)
        {
            return viewName + " is " + size + ", which is bigger than the cell a view sits "
                + "in on a sheet laid out for " + viewsPerSheet
                + (viewsPerSheet == 1 ? " view." : " views.")
                + " It is alone on its sheet, so it covers nothing, and it reaches past its "
                + "cell. Change its scale, its crop, or how many views go on the sheet.";
        }
    }
}
