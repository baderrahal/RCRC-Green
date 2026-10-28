using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace RcrcGreen.Core.Tests.Records
{
    /// <summary>
    /// The record caps of AI MAX version 4, held in the gate. Every session pays for these files
    /// at start, and the last one paid 190 million tokens with CLAUDE.md at 28 KB and a 623 KB
    /// log, so a pull request that grows one past its cap fails here rather than in the bill.
    ///
    /// A kilobyte is 1,000 bytes here, the stricter reading, so "at most 10 KB" holds however
    /// it is read. A file is measured as git stores it, with CRLF counted as one byte, so a
    /// Windows checkout and the Linux runner give the same answer.
    /// </summary>
    public class RecordCapsTests
    {
        private const int Kilobyte = 1000;

        // The caps start with version 4, so every entry dated from this day on is held to 4 KB,
        // not only the top one: a round that writes a long entry and then a short merged note
        // above it, the shape log-drawing.md already holds, would otherwise hide the long one.
        private static readonly DateTime CapsBegan = new DateTime(2026, 10, 27);

        // A top entry written before the caps that is over 4 KB, which may not be reworded to
        // fit. It is named whole, with its size measured on 2026-10-28, so it cannot grow and a
        // mistyped date cannot join it. It drops out when View Filters writes a newer entry.
        private static readonly (string Log, string Heading, int Bytes)[] OverBeforeTheCaps =
        {
            ("log-view-filters.md", "## 2026-09-19, third pass. The rail: five numbered steps, one open at a time", 12864),
        };

        private static readonly Regex EntryDate = new Regex(@"^## (\d{4}-\d{2}-\d{2})");

        [Fact]
        public void ClaudeMdIsAtMostTenKilobytes()
        {
            AtMost(Record("CLAUDE.md"), 10 * Kilobyte, "CLAUDE.md");
        }

        [Fact]
        public void ProgressIsAtMostSixtyLinesAndSixKilobytesWithNoLineOverTwoHundredCharacters()
        {
            string text = Record(Path.Combine("steps", "PROGRESS.md"));

            AtMost(text, 6 * Kilobyte, "steps/PROGRESS.md");
            Assert.True(Lines(text).Count <= 60,
                "steps/PROGRESS.md has " + Lines(text).Count + " lines, over the cap of 60.");

            List<string> over = Lines(text)
                .Select((line, at) => new { line, at })
                .Where(x => x.line.Length > 200)
                .Select(x => "line " + (x.at + 1) + " is " + x.line.Length + " characters")
                .ToList();
            Assert.True(over.Count == 0,
                "steps/PROGRESS.md has a line over 200 characters: " + string.Join(", ", over));
        }

        [Fact]
        public void StartIsUnderTwentyLines()
        {
            int lines = Lines(Record(Path.Combine("steps", "START.md"))).Count;
            Assert.True(lines < 20, "steps/START.md has " + lines + " lines, the cap is under 20.");
        }

        [Fact]
        public void TheTopEntryAndEveryEntrySinceTheCapsOfEveryLogIsAtMostFourKilobytes()
        {
            string[] logs = Directory.GetFiles(Path.Combine(Root(), "steps"), "log-*.md");
            Assert.NotEmpty(logs);

            List<string> over = logs.SelectMany(log => Faults(Path.GetFileName(log), Normalised(log))).ToList();
            Assert.True(over.Count == 0, "Over the 4 KB cap: " + string.Join(", ", over));
        }

        [Fact]
        public void ALongEntryUnderAShortMergedNoteIsStillCaught()
        {
            string log = "# Log\n\n## 2026-10-29, merged\n\nShort.\n\n## 2026-10-29, the round\n\n"
                + new string('x', 5000) + "\n";
            Assert.Single(Faults("log-kpi.md", log));
        }

        [Fact]
        public void TheOneOldEntryOverTheCapMayNotGrow()
        {
            string heading = "## 2026-09-19, third pass. The rail: five numbered steps, one open at a time\n";
            Assert.Empty(Faults("log-view-filters.md", heading + new string('x', 12000)));
            Assert.Single(Faults("log-view-filters.md", heading + new string('x', 13000)));
            Assert.Single(Faults("log-drawing.md", heading + new string('x', 12000)));
        }

        [Fact]
        public void AnOldTopEntryNotNamedAndAnUndatedOneAreBothCapped()
        {
            Assert.Single(Faults("log-drawing.md", "## 2026-09-19, the sixty fourth pass\n" + new string('x', 5000)));
            Assert.Single(Faults("log-kpi.md", "## A heading with no date\n" + new string('x', 5000)));
            Assert.Empty(Faults("log-drawing.md", "## 2026-09-19, a\n\nshort\n\n## 2026-09-18, b\n" + new string('x', 5000)));
        }

        // Under 200 lines is the skill's. Under 10 KB is the round prompt's of 2026-10-28, because
        // a session pays for every file here on every turn its paths match.
        [Fact]
        public void EveryFileInTheRulesFolderIsUnderTenKilobytesAndTwoHundredLines()
        {
            string[] rules = Directory.GetFiles(Path.Combine(Root(), ".claude", "rules"));
            Assert.NotEmpty(rules);

            List<string> over = new List<string>();
            foreach (string rule in rules)
            {
                string text = Normalised(rule);
                int bytes = Bytes(text);
                int lines = Lines(text).Count;
                if (bytes >= 10 * Kilobyte || lines >= 200)
                    over.Add(Path.GetFileName(rule) + " is " + bytes + " bytes and " + lines + " lines");
            }

            Assert.True(over.Count == 0, "Over the rules cap: " + string.Join(", ", over));
        }

        [Fact]
        public void EveryFileInDocsAndStepsOutsideHistoryIsAtMostOneHundredKilobytes()
        {
            List<string> over = new List<string>();
            int seen = 0;

            foreach (string folder in new[] { "docs", "steps" })
            {
                string top = Path.Combine(Root(), folder);
                if (!Directory.Exists(top)) continue;

                string history = Path.Combine(top, "history") + Path.DirectorySeparatorChar;
                foreach (string file in Directory.GetFiles(top, "*", SearchOption.AllDirectories))
                {
                    if (file.StartsWith(history, StringComparison.Ordinal)) continue;
                    seen++;

                    int bytes = Bytes(Normalised(file));
                    if (bytes > 100 * Kilobyte)
                        over.Add(folder + "/" + Path.GetRelativePath(top, file).Replace('\\', '/')
                            + " is " + bytes + " bytes");
                }
            }

            Assert.True(seen > 0, "No file was found under docs/ or steps/, so nothing was checked.");
            Assert.True(over.Count == 0, "Over the 100 KB cap: " + string.Join(", ", over));
        }

        [Fact]
        public void AnEntryRunsFromItsHeadingToTheNext()
        {
            string log = "# KPI log\n\nNewest entry first.\n\n---\n\n## 2026-10-28, new\n\nBody.\n\n## 2026-10-27, old\n\nOld.\n";
            Assert.Equal(
                new[] { "## 2026-10-28, new\n\nBody.\n\n", "## 2026-10-27, old\n\nOld.\n" },
                Entries(log));
            Assert.Empty(Entries("# Log\n\nNo entries here.\n"));
        }

        [Fact]
        public void CarriageReturnsAreNotCounted()
        {
            Assert.Equal(Bytes("a\nb\n"), Bytes("a\r\nb\r\n".Replace("\r\n", "\n")));
        }

        /// <summary>
        /// The top entry and every entry dated from the caps on, each over 4 KB, unless it is the
        /// one old entry named above and no larger than it was measured.
        /// </summary>
        internal static List<string> Faults(string logName, string log)
        {
            List<string> faults = new List<string>();
            List<string> entries = Entries(log);

            for (int at = 0; at < entries.Count; at++)
            {
                string entry = entries[at];
                if (at > 0 && !DatedFromTheCaps(entry)) continue;

                int bytes = Bytes(entry);
                string heading = entry.Split('\n')[0];
                int cap = OverBeforeTheCaps
                    .Where(x => x.Log == logName && x.Heading == heading)
                    .Select(x => x.Bytes)
                    .DefaultIfEmpty(4 * Kilobyte)
                    .First();

                if (bytes > cap)
                    faults.Add(logName + " entry '" + heading + "' is " + bytes + " bytes, over " + cap);
            }

            return faults;
        }

        internal static List<string> Entries(string log)
        {
            List<int> starts = Regex.Matches(log, "^## ", RegexOptions.Multiline).Select(m => m.Index).ToList();
            return starts
                .Select((from, i) => log.Substring(from, (i + 1 < starts.Count ? starts[i + 1] : log.Length) - from))
                .ToList();
        }

        internal static bool DatedFromTheCaps(string entry)
        {
            Match date = EntryDate.Match(entry);
            if (!date.Success) return false;

            DateTime when = DateTime.ParseExact(
                date.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return when >= CapsBegan;
        }

        private static void AtMost(string text, int cap, string name)
        {
            int bytes = Bytes(text);
            Assert.True(bytes <= cap, name + " is " + bytes + " bytes, over the cap of " + cap + ".");
        }

        private static int Bytes(string text) => Encoding.UTF8.GetByteCount(text);

        private static List<string> Lines(string text)
        {
            List<string> lines = text.Split('\n').ToList();
            if (lines.Count > 0 && lines[lines.Count - 1].Length == 0) lines.RemoveAt(lines.Count - 1);
            return lines;
        }

        private static string Record(string relative) => Normalised(Path.Combine(Root(), relative));

        private static string Normalised(string path) =>
            File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n");

        /// <summary>The same walk the shipped file tests use, up from the test assembly.</summary>
        private static string Root()
        {
            DirectoryInfo at = new DirectoryInfo(AppContext.BaseDirectory);

            while (at != null)
            {
                if (File.Exists(Path.Combine(at.FullName, "RcrcGreen.sln"))) return at.FullName;
                at = at.Parent;
            }

            throw new FileNotFoundException(
                "RcrcGreen.sln was not found above " + AppContext.BaseDirectory
                + ", so the records could not be measured.");
        }
    }
}
