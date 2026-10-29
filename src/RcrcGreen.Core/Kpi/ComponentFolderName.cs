using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// **The folder a PRX_Component value is filed under, and the main folder it groups under.**
    /// Bader's rule of 29 October: every value gets its own folder, named as the value tidied, so
    /// two spellings of one value share one folder.
    ///
    /// <code>
    /// the spacing rule      ComponentMatch.Key, upper case, one space, no space before %
    /// STRT                  read as STREET
    /// LESS 20m              read as LESS THAN 20m, LESS before a width in metres only
    /// 20M                   written 20m, so a width reads the way the team types it
    /// </code>
    ///
    /// So NH STRT 20m ROW and NH STREET 20m ROW share NH STREET 20m ROW, and NH STRT LESS 20m ROW
    /// and NH STREET LESS THAN 20m ROW share NH STREET LESS THAN 20m ROW. **These tidy words live
    /// here and nowhere else.** The metre m is written lower case because Bader's own folder names
    /// carry it so, and upper casing the rest keeps one value typed two ways in one folder, which
    /// is what Windows does with letter case anyway.
    /// </summary>
    public static class ComponentFolderName
    {
        private static readonly Regex Width = new Regex(@"^\d+(\.\d+)?M$", RegexOptions.CultureInvariant);

        private static readonly Regex Number = new Regex(@"^\d+(\.\d+)?%?$", RegexOptions.CultureInvariant);

        private static readonly IReadOnlyDictionary<string, string> ReadAs =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "STRT", "STREET" }
            };

        /// <summary>The folder name of a value. Empty for a value with nothing in it.</summary>
        public static string Tidy(string value)
        {
            List<string> words = Words(value);
            var tidy = new List<string>();

            for (int at = 0; at < words.Count; at++)
            {
                string word = words[at];
                if (ReadAs.TryGetValue(word, out string read)) word = read;

                if (Width.IsMatch(word)) word = word.Substring(0, word.Length - 1) + "m";

                tidy.Add(word);

                bool nextIsAWidth = at + 1 < words.Count && Width.IsMatch(words[at + 1]);
                if (word == "LESS" && nextIsAWidth) tidy.Add("THAN");
            }

            return Joined(tidy);
        }

        /// <summary>
        /// **The value with its 60 per cent part taken off**, for a rule whose Main column reads
        /// base: FRIDAY MOSQUE MORE THAN 60% groups under FRIDAY MOSQUE. The part is MORE or LESS,
        /// THAN where it is written, and a number with or without its per cent sign. A value
        /// with no such part is its own base, and one that is nothing else keeps its tidy name
        /// rather than grouping under nothing.
        /// </summary>
        public static string Base(string value)
        {
            string tidy = Tidy(value);
            List<string> words = tidy.Split(' ').Where(word => word.Length > 0).ToList();
            var kept = new List<string>();

            for (int at = 0; at < words.Count; at++)
            {
                string word = words[at];
                if (word == "MORE" || word == "LESS")
                {
                    int next = at + 1;
                    if (next < words.Count && words[next] == "THAN") next++;
                    if (next < words.Count && Number.IsMatch(words[next]))
                    {
                        at = next;
                        continue;
                    }
                }

                kept.Add(word);
            }

            return kept.Count == 0 ? tidy : Joined(kept);
        }

        private static List<string> Words(string value)
        {
            string key = ComponentMatch.Key(value);
            return key.Length == 0
                ? new List<string>()
                : key.Split(' ').Where(word => word.Length > 0).ToList();
        }

        /// <summary>
        /// Words joined with one space. The key already put each per cent sign against its
        /// number, so no word here starts with one.
        /// </summary>
        private static string Joined(IEnumerable<string> words)
        {
            return string.Join(" ", words.ToArray());
        }
    }
}
