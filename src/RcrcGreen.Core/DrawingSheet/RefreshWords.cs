using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The clauses the refresh status line adds about what the read could not file.
    ///
    /// Three facts were worked out on every refresh and thrown away. The registry classified a
    /// scope box named dm-41 as a plot in the wrong case and the reader kept the plots and
    /// dropped the rest, so a mistyped box made its plot vanish, or turned a plot with a box
    /// into a plot with none, with the reason discarded. A view whose PRX_Plot_ID held
    /// something that is not a plot was counted as having no plot at all, with the value kept
    /// on the reading and shown nowhere. And a schedule whose name did not parse was skipped
    /// at capture with nothing written down, so one named in the wrong case was silently
    /// uncapturable.
    /// </summary>
    public static class RefreshWords
    {
        /// <summary>
        /// How many names a clause lists before it counts the rest. This is a status line, not
        /// a report, and a model can hold a hundred schedules that are not named for a plot.
        /// </summary>
        public const int Shown = 3;

        /// <summary>
        /// Every name that is a plot identifier in the wrong case, from any source: a view
        /// name, PRX_Plot_ID on a view or an element, or a scope box. Each is said in the words
        /// the registry already had for it.
        /// </summary>
        public static string WrongCase(IEnumerable<IgnoredName> names)
        {
            List<IgnoredName> wrong = (names ?? Enumerable.Empty<IgnoredName>())
                .Where(one => one != null && one.Reason == IgnoredReason.WrongCase)
                .ToList();

            if (wrong.Count == 0) return string.Empty;

            return Count(wrong.Count, "name is", "names are")
                + " a plot in the wrong case and " + (wrong.Count == 1 ? "was" : "were")
                + " not read as one: " + Listed(wrong.Select(one => one.ToString())) + ".";
        }

        /// <summary>
        /// Views whose PRX_Plot_ID holds something that is not a plot. They used to be counted
        /// under with no plot at all, which is a different problem: this value was filled in,
        /// and wrongly.
        /// </summary>
        public static string ParameterNotAPlot(int howMany, IEnumerable<string> values)
        {
            if (howMany <= 0) return string.Empty;

            List<string> examples = (values ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .Select(one => one.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();

            string said = Count(howMany, "view carries", "views carry")
                + " a PRX_Plot_ID that is not a plot";

            if (examples.Count > 0) said += ", such as " + Listed(examples);

            return said + ". " + (howMany == 1 ? "It" : "Each")
                + " still fills its cell from its name where the name parses.";
        }

        /// <summary>
        /// Schedules the capture skipped because their names do not parse, counted whole and
        /// named only where the name is a plot in the wrong case, because that is the one a
        /// person can fix and a Sheet List is not.
        /// </summary>
        public static string SchedulesNotCaptured(IEnumerable<IgnoredName> names)
        {
            List<IgnoredName> skipped = (names ?? Enumerable.Empty<IgnoredName>())
                .Where(one => one != null)
                .ToList();

            if (skipped.Count == 0) return string.Empty;

            List<IgnoredName> wrongCase = skipped
                .Where(one => one.Reason == IgnoredReason.WrongCase)
                .ToList();

            string said = Count(skipped.Count, "schedule was", "schedules were")
                + " not captured because " + (skipped.Count == 1 ? "its name does" : "their names do")
                + " not parse";

            if (wrongCase.Count == 0) return said + ".";

            return said + ", " + Count(wrongCase.Count, "of them", "of them")
                + " a plot in the wrong case: " + Listed(wrongCase.Select(one => one.ToString())) + ".";
        }

        private static string Listed(IEnumerable<string> names)
        {
            List<string> all = names.ToList();

            string shown = string.Join(", ", all.Take(Shown).ToArray());
            int more = all.Count - Shown;

            return more <= 0 ? shown : shown + " and " + more.ToString(CultureInfo.InvariantCulture) + " more";
        }

        private static string Count(int howMany, string one, string many)
        {
            return howMany.ToString(CultureInfo.InvariantCulture) + " " + (howMany == 1 ? one : many);
        }
    }
}
