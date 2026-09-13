using System;
using System.Text.RegularExpressions;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The plot id at the front of a view name: two letters, a dash, digits, anchored at the
    /// start, as in DM-41. This is the first edit to the ported run. The host it came from
    /// fell back to the whole view name when nothing parsed, which created filters named
    /// (215) Borders Edging General Arrangement Layout in a real model, so a name that does
    /// not start with a plot id is skipped by name rather than guessed at.
    ///
    /// The pattern takes either case, which is looser than the Shared PlotId, where lowercase
    /// is invalid. That is the port's rule kept as given, and whether to tighten it is a
    /// question for the team, in the log.
    /// </summary>
    public static class ViewFilterPlotCode
    {
        public const string Pattern = "^[A-Za-z]{2}-\\d+";

        private static readonly Regex AtTheStart = new Regex(Pattern);

        public static bool StartsWithAPlotId(string text)
        {
            return text != null && AtTheStart.IsMatch(text);
        }

        /// <summary>
        /// The plot code off a view name, or false for a name the run must skip. The text
        /// before the first "-(" is the code when the name holds one, checked against the
        /// pattern, so DM-41-(010) Location Key Plan gives DM-41. A name with no "-(" gives
        /// the pattern's own match, so DM-41 alone is DM-41 and General Arrangement Layout
        /// is a skip. The run and the scan both come here, so the two cannot part.
        /// </summary>
        public static bool TryFromViewName(string viewName, out string plotCode)
        {
            plotCode = string.Empty;
            if (string.IsNullOrEmpty(viewName)) return false;

            int dashParen = viewName.IndexOf("-(", StringComparison.Ordinal);
            if (dashParen > 0)
            {
                string before = viewName.Substring(0, dashParen).Trim();
                if (!AtTheStart.IsMatch(before)) return false;

                plotCode = before;
                return true;
            }

            Match found = AtTheStart.Match(viewName.Trim());
            if (!found.Success) return false;

            plotCode = found.Value;
            return true;
        }
    }
}
