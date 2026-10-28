using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The one rule that says whether two PRX_Component values are the same value. Every table
    /// lookup on a component goes through <see cref="Key"/>, the template, the folder and the
    /// line a person adds in the pane, so the three cannot drift apart.
    ///
    /// **It is still the whole value.** Nothing here makes a part of a value match, so PARK never
    /// finds PARKING LOT and NH STRT 20m ROW is never NH STRT LESS 20m ROW. What it forgives is
    /// how the value was typed:
    ///
    /// <code>
    /// case                    daily mosque          is DAILY MOSQUE
    /// edge whitespace         " SCHOOL "            is SCHOOL
    /// a run of spaces         SCHOOL  MORE THAN 60% is SCHOOL MORE THAN 60%
    /// a space before %        MORE THAN 60 %        is MORE THAN 60%
    /// a word run together     LESSTHAN, MORETHAN    is LESS THAN, MORE THAN
    /// </code>
    ///
    /// The last two are what Bader typed for the team's four new values, DAILY MOSQUE LESSTHAN
    /// 60%, and the NG03 reports of 12 Sep print LESS THAN with the space. Which the model holds
    /// today is UNKNOWN, so both are one value.
    /// </summary>
    public static class ComponentMatch
    {
        private static readonly IReadOnlyDictionary<string, string> RunTogether =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "LESSTHAN", "LESS THAN" },
                { "MORETHAN", "MORE THAN" }
            };

        /// <summary>
        /// The form two values are compared in. Empty for a value with nothing in it.
        /// </summary>
        public static string Key(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            string[] words = value.ToUpperInvariant()
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                .Select(word => RunTogether.TryGetValue(word, out string apart) ? apart : word)
                .ToArray();

            var joined = new StringBuilder();
            foreach (string word in words)
            {
                if (joined.Length > 0 && !word.StartsWith("%", StringComparison.Ordinal)) joined.Append(' ');
                joined.Append(word);
            }

            return joined.ToString();
        }

        public static bool Same(string one, string other)
        {
            string a = Key(one);
            return a.Length > 0 && string.Equals(a, Key(other), StringComparison.Ordinal);
        }
    }
}
