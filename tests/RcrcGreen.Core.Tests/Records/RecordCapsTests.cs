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

        // The caps start with version 4. An entry written before it was never held to 4 KB, and
        // rewording one to fit is refused by the same rules, so the cap reads the entries from
        // this date on. The View Filters log's top entry, 12,864 bytes on 19 September, is the
        // one this exempts today.
        private static readonly DateTime CapsBegan = new DateTime(2026, 10, 27);

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
        public void TheTopEntryOfEveryLogIsAtMostFourKilobytes()
        {
            string[] logs = Directory.GetFiles(Path.Combine(Root(), "steps"), "log-*.md");
            Assert.NotEmpty(logs);

            List<string> over = new List<string>();
            foreach (string log in logs)
            {
                string top = TopEntry(Normalised(log));
                if (top == null || WrittenBeforeTheCaps(top)) continue;

                int bytes = Bytes(top);
                if (bytes > 4 * Kilobyte)
                    over.Add(Path.GetFileName(log) + " top entry is " + bytes + " bytes");
            }

            Assert.True(over.Count == 0, "Over the 4 KB cap: " + string.Join(", ", over));
        }

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
        public void AnEntryBeforeTheCapsIsTheOnlyOneExempt()
        {
            Assert.True(WrittenBeforeTheCaps("## 2026-09-19, third pass\n"));
            Assert.False(WrittenBeforeTheCaps("## 2026-10-27, a round\n"));
            Assert.False(WrittenBeforeTheCaps("## A heading with no date\n"));
        }

        [Fact]
        public void TheTopEntryRunsFromTheFirstHeadingToTheNext()
        {
            string log = "# KPI log\n\nNewest entry first.\n\n---\n\n## 2026-10-28, new\n\nBody.\n\n## 2026-10-27, old\n\nOld.\n";
            Assert.Equal("## 2026-10-28, new\n\nBody.\n\n", TopEntry(log));
            Assert.Null(TopEntry("# Log\n\nNo entries here.\n"));
        }

        [Fact]
        public void CarriageReturnsAreNotCounted()
        {
            Assert.Equal(Bytes("a\nb\n"), Bytes("a\r\nb\r\n".Replace("\r\n", "\n")));
        }

        internal static string TopEntry(string log)
        {
            MatchCollection headings = Regex.Matches(log, "^## ", RegexOptions.Multiline);
            if (headings.Count == 0) return null;

            int from = headings[0].Index;
            int to = headings.Count > 1 ? headings[1].Index : log.Length;
            return log.Substring(from, to - from);
        }

        internal static bool WrittenBeforeTheCaps(string entry)
        {
            Match date = EntryDate.Match(entry);
            if (!date.Success) return false;

            DateTime when = DateTime.ParseExact(
                date.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return when < CapsBegan;
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
