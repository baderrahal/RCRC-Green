using System;
using System.Globalization;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Names the file a report is written to: a task's own prefix, the document title made
    /// safe, and the minute. Every task goes through here so its files sort next to the
    /// others on the Desktop and clean up the same way. The prefixes themselves are each
    /// task's own and live with that task, because a report name is a thing one task means.
    /// </summary>
    public static class ScanFileName
    {
        public const string Extension = ".txt";

        /// <summary>
        /// A Revit document title comes from a file name, so it can carry a colon, a slash or
        /// a character Windows will not take in a path. Anything outside letters, digits, a
        /// dash, a dot and a space becomes a dash, and runs of dashes collapse, so two titles
        /// that differ only in punctuation do not both come out as one row of dashes.
        /// </summary>
        public static string For(string prefix, string documentTitle, DateTime writtenAt)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");

            return prefix
                + Safe(documentTitle)
                + "_"
                + writtenAt.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture)
                + Extension;
        }

        /// <summary>
        /// The cleaning on its own, for a name that is not a report's. The KPI output name
        /// goes through this, because a second copy of the cleaning rule is the fault this
        /// repo has hit six times.
        /// </summary>
        public static string Cleaned(string name)
        {
            return Safe(name);
        }

        private static string Safe(string documentTitle)
        {
            if (string.IsNullOrEmpty(documentTitle)) return "untitled";

            var kept = new StringBuilder(documentTitle.Length);
            bool lastWasDash = false;

            foreach (char c in documentTitle.Trim())
            {
                bool plain = (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || c == '.'
                    || c == ' ';

                if (plain)
                {
                    kept.Append(c);
                    lastWasDash = false;
                    continue;
                }

                if (!lastWasDash)
                {
                    kept.Append('-');
                    lastWasDash = true;
                }
            }

            string safe = kept.ToString().Trim(' ', '-', '.');
            return safe.Length == 0 ? "untitled" : safe;
        }
    }
}
