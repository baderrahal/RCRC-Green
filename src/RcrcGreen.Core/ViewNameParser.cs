using System.Text.RegularExpressions;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Reads the naming used for every view and sheet in the model:
    /// PlotID, a dash, the code in round brackets, one space, then the view name.
    /// </summary>
    public static class ViewNameParser
    {
        private static readonly Regex Shape = new Regex(
            "^(?<plot>" + PlotId.Pattern + ")-\\((?<code>[0-9]+)\\) (?<view>.+)$",
            RegexOptions.CultureInvariant);

        /// <summary>
        /// Returns false for anything that does not match. Models hold plenty of views that
        /// were never named to this pattern, so a name that does not fit is an ordinary
        /// result rather than a fault.
        /// </summary>
        public static bool TryParse(string name, out ParsedViewName parsed)
        {
            parsed = null;
            if (string.IsNullOrEmpty(name)) return false;

            Match match = Shape.Match(name);
            if (!match.Success) return false;

            parsed = new ParsedViewName(
                match.Groups["plot"].Value,
                match.Groups["code"].Value,
                match.Groups["view"].Value,
                name);
            return true;
        }
    }
}
