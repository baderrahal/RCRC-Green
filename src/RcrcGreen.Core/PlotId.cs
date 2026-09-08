using System.Text.RegularExpressions;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A plot identifier is two letters, a dash, then digits. DM-41 and PF-12 are real ones.
    /// </summary>
    public static class PlotId
    {
        /// <summary>
        /// Shared with <see cref="ViewNameParser"/> so a plot found through a scope box and a
        /// plot found through a view name are held to the same shape.
        /// </summary>
        public const string Pattern = "[A-Za-z]{2}-[0-9]+";

        private static readonly Regex Whole = new Regex("^" + Pattern + "$", RegexOptions.CultureInvariant);

        public static bool IsPlotId(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;
            return Whole.IsMatch(candidate);
        }
    }
}
