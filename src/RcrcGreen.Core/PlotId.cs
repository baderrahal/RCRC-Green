using System.Text.RegularExpressions;

namespace RcrcGreen.Core
{
    /// <summary>
    /// A plot identifier is two uppercase letters, a dash, then digits. DM-41 and PF-12 are
    /// real ones. Lowercase is not a different plot, it is invalid.
    /// </summary>
    public static class PlotId
    {
        /// <summary>
        /// Shared with <see cref="ViewNameParser"/> so a plot found through a scope box and a
        /// plot found through a view name are held to the same shape.
        /// </summary>
        public const string Pattern = "[A-Z]{2}-[0-9]+";

        /// <summary>
        /// The same shape with the case rule dropped. Nothing accepts a match on this. It is
        /// only used to tell a plot identifier typed in the wrong case apart from a name that
        /// was never a plot identifier at all.
        /// </summary>
        public const string PatternAnyCase = "[A-Za-z]{2}-[0-9]+";

        private static readonly Regex Whole =
            new Regex(@"\A" + Pattern + @"\z", RegexOptions.CultureInvariant);

        private static readonly Regex WholeAnyCase =
            new Regex(@"\A" + PatternAnyCase + @"\z", RegexOptions.CultureInvariant);

        public static bool IsPlotId(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;
            return Whole.IsMatch(candidate);
        }

        /// <summary>
        /// True when the only thing wrong with the candidate is the case of its two letters.
        /// A scope box named dm-41 is a mistyped DM-41, not a second plot, and the caller
        /// needs to be able to say so.
        /// </summary>
        public static bool FailsOnlyOnCase(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return false;
            return !Whole.IsMatch(candidate) && WholeAnyCase.IsMatch(candidate);
        }

        /// <summary>
        /// Reads a scope box name or a PRX_Plot_ID value. Surrounding whitespace is dropped
        /// first, because a trailing space on a text parameter is ordinary in a shared model
        /// and it must not turn one plot into two.
        /// </summary>
        public static bool TryRead(string candidate, out string plotId)
        {
            plotId = null;
            if (candidate == null) return false;

            string trimmed = candidate.Trim();
            if (!IsPlotId(trimmed)) return false;

            plotId = trimmed;
            return true;
        }
    }
}
