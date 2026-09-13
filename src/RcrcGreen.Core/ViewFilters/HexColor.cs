using System.Globalization;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// #FF0000 or FF0000 into three bytes. Six hex digits and nothing else: a short form, a
    /// long one or a letter past F is a no rather than a nearest guess, which is the ported
    /// host's rule kept as written. The run wraps the bytes in Revit's own colour type and
    /// the pane paints its swatches from the same parse.
    /// </summary>
    public static class HexColor
    {
        public static bool TryParse(string hex, out byte red, out byte green, out byte blue)
        {
            red = 0;
            green = 0;
            blue = 0;

            if (string.IsNullOrWhiteSpace(hex)) return false;

            string held = hex.Trim().TrimStart('#');
            return held.Length == 6
                && byte.TryParse(held.Substring(0, 2), NumberStyles.HexNumber, null, out red)
                && byte.TryParse(held.Substring(2, 2), NumberStyles.HexNumber, null, out green)
                && byte.TryParse(held.Substring(4, 2), NumberStyles.HexNumber, null, out blue);
        }

        /// <summary>
        /// Three bytes back into #RRGGBB, upper case, which is what the colour picker writes
        /// into a row. One writer beside the one parser, so the two cannot part.
        /// </summary>
        public static string Written(byte red, byte green, byte blue)
        {
            return "#"
                + red.ToString("X2", CultureInfo.InvariantCulture)
                + green.ToString("X2", CultureInfo.InvariantCulture)
                + blue.ToString("X2", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// What a row's colour holds after somebody types. The typed text when it parses.
        /// The typed text again when it is blank, because clearing the box is a deliberate
        /// act and keeping the old colour behind a blank, ordinary looking field is how a
        /// colour somebody tried to remove would come back on the next Apply. Only text
        /// that is neither, a mistype, keeps the stored value, and the box paints its
        /// border by the same parse, so a warning edge means the text on screen is not the
        /// value the run would use.
        /// </summary>
        public static string Kept(string stored, string typed)
        {
            if (string.IsNullOrWhiteSpace(typed)) return typed ?? string.Empty;

            return TryParse(typed, out byte red, out byte green, out byte blue)
                ? typed
                : stored;
        }
    }
}
