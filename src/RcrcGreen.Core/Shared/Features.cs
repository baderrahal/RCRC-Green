using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which tools the ribbon builds, read off features.txt beside the installed assemblies.
    ///
    /// One build serves two installs. Bader's carries all three tools and has no such file, so
    /// no file and an empty file both mean all three and his install does not change. The team
    /// kit writes KPI alone, so the production team sees one panel and Revit registers no pane
    /// for the other two.
    ///
    /// It lives in Shared because the application that reads it belongs to every task and no
    /// task owns which tools a machine shows.
    /// </summary>
    public sealed class Features
    {
        public const string FileName = "features.txt";

        public const string DrawingSheet = "Drawing Sheet";
        public const string Kpi = "KPI";
        public const string ViewFilters = "View Filters";

        /// <summary>Every tool, in the order the ribbon puts them.</summary>
        public static readonly IReadOnlyList<string> All = new[] { DrawingSheet, Kpi, ViewFilters };

        private Features(IReadOnlyList<string> shown, IReadOnlyList<string> unknown, bool noneNamed)
        {
            Shown = shown;
            Unknown = unknown;
            NoneNamed = noneNamed;
        }

        /// <summary>The tools the ribbon builds, in ribbon order.</summary>
        public IReadOnlyList<string> Shown { get; }

        /// <summary>Lines naming no tool, edge whitespace off, in file order. Ignored.</summary>
        public IReadOnlyList<string> Unknown { get; }

        /// <summary>
        /// True where the file named no tool it knows, which shows all three. CHOSEN: a file
        /// whose every line is a typo would otherwise leave the tab empty with nothing on it to
        /// say why, and three tools shown is a fault somebody sees and reports.
        /// </summary>
        public bool NoneNamed { get; }

        public bool Shows(string tool)
        {
            return Shown.Contains(tool, StringComparer.Ordinal);
        }

        /// <summary>
        /// The file's lines, or null where there is no file. A line is matched whole, edge
        /// whitespace off and without case, so a line reading kpi shows the KPI panel. Blank
        /// lines are skipped.
        /// </summary>
        public static Features Of(IEnumerable<string> lines)
        {
            var named = new HashSet<string>(StringComparer.Ordinal);
            var unknown = new List<string>();

            foreach (string raw in lines ?? Enumerable.Empty<string>())
            {
                string line = (raw ?? string.Empty).Trim();
                if (line.Length == 0) continue;

                string tool = All.FirstOrDefault(one => string.Equals(one, line, StringComparison.OrdinalIgnoreCase));
                if (tool == null) unknown.Add(line);
                else named.Add(tool);
            }

            bool noneNamed = named.Count == 0;
            IReadOnlyList<string> shown = noneNamed ? All : All.Where(named.Contains).ToList();
            return new Features(shown, unknown, noneNamed);
        }

        /// <summary>
        /// What goes into Revit's journal at startup, one line per name the file holds that is
        /// not a tool, and one more where that left no tool named. Nothing where all is well.
        /// </summary>
        public IReadOnlyList<string> JournalLines()
        {
            var lines = new List<string>();
            foreach (string name in Unknown)
            {
                lines.Add("RCRC Green: " + FileName + " names '" + name + "', which is not a tool. "
                    + "It is ignored. The tools are " + string.Join(", ", All) + ".");
            }

            if (NoneNamed && Unknown.Count > 0)
            {
                lines.Add("RCRC Green: " + FileName + " names no tool, so all three are shown.");
            }

            return lines;
        }
    }
}
