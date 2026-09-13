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
    }
}
