using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One section of a written report, how many lines it came to and how many blocks it is
    /// spread over.
    /// </summary>
    public sealed class ReportSection
    {
        public ReportSection(string name, int lines, int blocks)
        {
            Name = name ?? string.Empty;
            Lines = lines;
            Blocks = blocks;
        }

        public string Name { get; }

        /// <summary>
        /// Every line under this section's heading, the heading itself counted, added over every
        /// block carrying it.
        /// </summary>
        public int Lines { get; }

        /// <summary>
        /// How many times the heading appears. A per plot section appears once per plot, so a
        /// section's size and its per block size are told apart rather than guessed at.
        /// </summary>
        public int Blocks { get; }

        public int LinesPerBlock
        {
            get { return Blocks == 0 ? 0 : Lines / Blocks; }
        }
    }

    /// <summary>
    /// What a report file came to, section by section, counted off the text itself.
    ///
    /// **A REPORT NOBODY CAN OPEN IS NOT A RECORD.** The 19:52 run wrote 82,048 lines, of which
    /// <see cref="KpiCreateReport.FormulasHeading"/> was 42,570, which is 52 percent of the file
    /// and 273 lines for each of the 156 plots. The run before it was 10,708 lines over seven
    /// plot blocks, so the per plot fix multiplied every block by 156 and nothing said which
    /// block was the expensive one.
    ///
    /// **THE NEXT CUT IS MADE ON NUMBERS RATHER THAN ON A GUESS.** This counts what the run just
    /// wrote and the report prints it at the top, so the size of every section is a measurement
    /// in the file rather than something somebody works out by scrolling.
    ///
    /// It reads the report's own headings and nothing else: a line reading `== NAME (n) ==` or
    /// `== NAME ==` opens a section and every line after it belongs to that section until the
    /// next heading. **Nothing here knows what any section is**, so a section added or renamed
    /// is counted with no change on this side.
    /// </summary>
    public static class ReportSections
    {
        /// <summary>
        /// Where the lines above the first heading are counted. The file's own opening is real
        /// text and dropping it would leave the parts not adding up to the whole.
        /// </summary>
        public const string Opening = "(the opening, above the first heading)";

        public static IReadOnlyList<ReportSection> Of(string report)
        {
            var order = new List<string>();
            var lines = new Dictionary<string, int>(StringComparer.Ordinal);
            var blocks = new Dictionary<string, int>(StringComparer.Ordinal);

            string under = Opening;

            foreach (string line in Split(report))
            {
                string heading = HeadingIn(line);
                if (heading.Length > 0)
                {
                    under = heading;
                    Add(order, blocks, under, 1);
                }

                Add(order, lines, under, 1);
            }

            return order
                .Where(name => lines.ContainsKey(name))
                .Select(name => new ReportSection(
                    name, lines[name], blocks.ContainsKey(name) ? blocks[name] : 0))
                .OrderByDescending(one => one.Lines)
                .ThenBy(one => one.Name, StringComparer.Ordinal)
                .ToList();
        }

        public static int LinesIn(string report)
        {
            return Split(report).Count;
        }

        /// <summary>
        /// The section a heading line names, or empty for an ordinary line. The count in the
        /// brackets is taken off, so a section whose count moves between blocks is still one
        /// section.
        /// </summary>
        public static string HeadingIn(string line)
        {
            string held = (line ?? string.Empty).Trim();
            if (held.Length < 6 || !held.StartsWith("== ", StringComparison.Ordinal)
                || !held.EndsWith(" ==", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            string name = held.Substring(3, held.Length - 6).Trim();
            if (!name.EndsWith(")", StringComparison.Ordinal)) return name;

            int bracket = name.LastIndexOf(" (", StringComparison.Ordinal);
            if (bracket <= 0) return name;

            // **ONLY A COUNT COMES OFF.** The brackets hold how many rows the section found, so
            // two blocks of one section are one section here. A name really ending in brackets
            // keeps them, because taking off whatever sits in a bracket would rename a section on
            // the strength of its punctuation.
            string inside = name.Substring(bracket + 2, name.Length - bracket - 3);
            if (inside.Length == 0 || !inside.All(char.IsDigit)) return name;

            return name.Substring(0, bracket).Trim();
        }

        private static void Add(List<string> order, IDictionary<string, int> counts, string name, int howMany)
        {
            int held;
            if (!counts.TryGetValue(name, out held))
            {
                held = 0;
                if (!order.Contains(name)) order.Add(name);
            }

            counts[name] = held + howMany;
        }

        /// <summary>
        /// The report is written with a carriage return and a line feed, and a file ending in
        /// one does not carry an empty last line. Splitting on the line feed alone and dropping
        /// a trailing empty keeps the count the same as a text editor's.
        /// </summary>
        private static IReadOnlyList<string> Split(string report)
        {
            if (string.IsNullOrEmpty(report)) return new List<string>();

            var held = (report ?? string.Empty)
                .Replace("\r\n", "\n")
                .Split('\n')
                .ToList();

            if (held.Count > 0 && held[held.Count - 1].Length == 0) held.RemoveAt(held.Count - 1);

            return held;
        }

        /// <summary>
        /// One line per section for the report's own contents block, widest count first.
        /// </summary>
        public static string InWords(ReportSection section)
        {
            if (section == null) return string.Empty;

            return Padded(section.Lines) + " | " + Padded(section.Blocks) + " | "
                + Padded(section.LinesPerBlock) + " | " + section.Name;
        }

        private static string Padded(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture).PadLeft(7);
        }
    }
}
