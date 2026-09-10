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
            @"\A(?<plot>" + PlotId.Pattern + @")-\((?<code>[0-9]+)\) (?<view>.+)\z",
            RegexOptions.CultureInvariant);

        private static readonly Regex ShapeAnyCase = new Regex(
            @"\A(?<plot>" + PlotId.PatternAnyCase + @")-\((?<code>[0-9]+)\) (?<view>.+)\z",
            RegexOptions.CultureInvariant);

        /// <summary>
        /// Returns false for anything that does not match. Models hold plenty of views that
        /// were never named to this pattern, so a name that does not fit is an ordinary
        /// result rather than a fault.
        ///
        /// Surrounding whitespace is dropped from the name and from the view name inside it.
        /// The same view type on two plots carries the same text, so a stray trailing space
        /// would otherwise put two columns on the grid that print identically.
        /// </summary>
        public static bool TryParse(string name, out ParsedViewName parsed)
        {
            parsed = null;
            if (string.IsNullOrEmpty(name)) return false;

            Match match = Shape.Match(name.Trim());
            if (!match.Success) return false;

            string viewName = match.Groups["view"].Value.Trim();
            if (viewName.Length == 0) return false;

            parsed = new ParsedViewName(
                match.Groups["plot"].Value,
                match.Groups["code"].Value,
                viewName,
                name);
            return true;
        }

        /// <summary>
        /// True when the name is shaped right and the only thing wrong is the case of the two
        /// letters in the plot identifier.
        /// </summary>
        public static bool FailsOnlyOnPlotCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            string trimmed = name.Trim();
            return !Shape.IsMatch(trimmed) && ShapeAnyCase.IsMatch(trimmed);
        }
    }
}
