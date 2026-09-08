using System;
using System.Globalization;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Names the file a command writes its report to. Both commands go through here so the
    /// two files sort next to each other on the Desktop and clean up the same way.
    /// </summary>
    public static class ScanFileName
    {
        public const string ScanPrefix = "RCRC-Green-Scan_";

        public const string ScopeBoxPrefix = "RCRC-Green-ScopeBox_";

        public const string RunPrefix = "RCRC-Green-Run_";

        public const string Extension = ".txt";

        /// <summary>
        /// A Revit document title comes from a file name, so it can carry a colon, a slash or
        /// a character Windows will not take in a path. Anything outside letters, digits, a
        /// dash, a dot and a space becomes a dash, and runs of dashes collapse, so two titles
        /// that differ only in punctuation do not both come out as one row of dashes.
        /// </summary>
        public static string For(string documentTitle, DateTime writtenAt)
        {
            return For(ScanPrefix, documentTitle, writtenAt);
        }

        public static string For(string prefix, string documentTitle, DateTime writtenAt)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");

            return prefix
                + Safe(documentTitle)
                + "_"
                + writtenAt.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture)
                + Extension;
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
