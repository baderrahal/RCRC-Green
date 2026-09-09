using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Where a sheet's width and height were read from, which is the part worth printing.
    /// </summary>
    public enum SheetSizeSource
    {
        /// <summary>
        /// Nothing readable came back. No view can be placed, so the sheet is refused rather
        /// than made empty.
        /// </summary>
        NotRead = 0,

        /// <summary>
        /// The Sheet Width and Sheet Height of the title block placed on the sheet.
        /// </summary>
        TitleBlockParameters = 1,

        /// <summary>
        /// Measured across the title block on the sheet, for a title block whose family does
        /// not drive those two parameters.
        /// </summary>
        TitleBlockOutline = 2
    }

    /// <summary>
    /// How big a sheet is, in feet, and which read produced it.
    ///
    /// Three sheets were created empty and the report said their title block reported no width
    /// or height. AR-PRX-Title_Block_A1 is a real A1 sheet, so it has a size. Sheet Width and
    /// Sheet Height are read-only INSTANCE parameters, which exist on a title block placed on a
    /// sheet and not on the title block type. They were being read off the type, which returned
    /// nothing, and nothing turned into zero.
    ///
    /// So the source is recorded next to the number, the same way the sibling a view is set up
    /// from is recorded next to the view. A size that came from nowhere reads exactly like a
    /// size that was measured.
    /// </summary>
    public sealed class SheetSize
    {
        private SheetSize(SheetSizeSource source, double widthFeet, double heightFeet, string titleBlock)
        {
            Source = source;
            WidthFeet = widthFeet;
            HeightFeet = heightFeet;
            TitleBlock = titleBlock ?? string.Empty;
        }

        public static SheetSize NotRead(string titleBlock)
        {
            return new SheetSize(SheetSizeSource.NotRead, 0.0, 0.0, titleBlock);
        }

        /// <summary>
        /// A size is only a size when both sides are a real positive length. A NaN survives an
        /// ordering check and then turns every viewport centre into NaN, which comes back
        /// looking like a placement rather than a failure.
        /// </summary>
        public static SheetSize Of(
            SheetSizeSource source, double widthFeet, double heightFeet, string titleBlock)
        {
            if (source == SheetSizeSource.NotRead) return NotRead(titleBlock);

            if (!Real(widthFeet) || !Real(heightFeet)) return NotRead(titleBlock);

            return new SheetSize(source, widthFeet, heightFeet, titleBlock);
        }

        public SheetSizeSource Source { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        public string TitleBlock { get; }

        public bool CanBeUsed
        {
            get { return Source != SheetSizeSource.NotRead; }
        }

        /// <summary>
        /// The line the report carries for a sheet that was made. Millimetres, because that is
        /// what a person calls an A1, and the source, because the last two rounds both turned
        /// on a value nobody could trace.
        /// </summary>
        public string InWords()
        {
            if (!CanBeUsed)
            {
                return "Size not read from " + Named(TitleBlock) + ".";
            }

            string measured = Millimetres(WidthFeet) + " by " + Millimetres(HeightFeet) + " mm";

            return Source == SheetSizeSource.TitleBlockParameters
                ? measured + ", from the Sheet Width and Sheet Height of " + Named(TitleBlock) + "."
                : measured + ", measured across " + Named(TitleBlock)
                    + ", which does not set Sheet Width and Sheet Height.";
        }

        /// <summary>
        /// Why a sheet with views on it was refused. It names the two things somebody would go
        /// and look at, rather than saying the size is missing and stopping there.
        /// </summary>
        public string WhyNotInWords(string sheetName)
        {
            return (sheetName ?? string.Empty)
                + " was not made. Its title block " + Named(TitleBlock) + " gave no size. "
                + "Sheet Width and Sheet Height are read off the title block placed on the "
                + "sheet, and measuring across that block gave nothing either, so there is "
                + "nowhere worked out to put a view. Views were ticked for this sheet, so an "
                + "empty one is not what was asked for.";
        }

        private static string Millimetres(double feet)
        {
            return Lengths.InMillimetres(feet).ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string Named(string what)
        {
            return string.IsNullOrEmpty(what) ? "that title block" : what;
        }

        private static bool Real(double length)
        {
            return !double.IsNaN(length) && !double.IsInfinity(length) && length > 0.0;
        }

        public override string ToString()
        {
            return InWords();
        }
    }
}
