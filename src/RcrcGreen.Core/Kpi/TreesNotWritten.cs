using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One species of one plot whose count reached no row of any workbook.
    /// </summary>
    public sealed class TreeNotWritten
    {
        public TreeNotWritten(string plotId, string botanicalName, int trees, string why)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            BotanicalName = (botanicalName ?? string.Empty).Trim();
            Trees = trees;
            Why = (why ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string BotanicalName { get; }

        public int Trees { get; }

        public string Why { get; }

        /// <summary>36 AZADIRACHTA INDICA, which is the count and the name and nothing else.</summary>
        public string InWords
        {
            get { return Trees.ToString(CultureInfo.InvariantCulture) + " " + BotanicalName; }
        }
    }

    /// <summary>
    /// One plot's trees that were written nowhere, for the plot list's own column.
    /// </summary>
    public sealed class PlotTreesNotWritten
    {
        public PlotTreesNotWritten(string plotId, IEnumerable<TreeNotWritten> species)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            Species = (species ?? Enumerable.Empty<TreeNotWritten>()).ToList();
        }

        public string PlotId { get; }

        public IReadOnlyList<TreeNotWritten> Species { get; }

        public int Trees
        {
            get { return Species.Sum(one => one.Trees); }
        }

        /// <summary>
        /// What the plot list's column carries: the count and the species, in one cell. Empty
        /// for a plot that had none, so an ordinary row does not carry a word about it.
        /// </summary>
        public string InWords
        {
            get
            {
                if (Species.Count == 0) return string.Empty;

                return string.Join(", ", Species.Select(one => one.InWords).ToArray());
            }
        }
    }

    /// <summary>
    /// **TREES WRITTEN NOWHERE MUST SHOW AT THE TOP.** Bader's decision of 15 September, off the
    /// 13:32 run: FP-17 36, FP-21 25 and FP-20 5 AZADIRACHTA INDICA each went nowhere because
    /// the FUTURE PARKS workbook holds that name on more than one row, and FP-23 lost 1 tree
    /// named UNKNOWN. **THE PLOT LIST read YES and YES for all four and the glance said nothing**,
    /// so 67 trees left the building in four rows that looked exactly like the rows of a plot
    /// with nothing wrong with it.
    ///
    /// **NOTHING HERE CHOOSES BETWEEN TWO ROWS.** The tool still refuses a name the workbook
    /// holds twice, which is Bader's decision and the team's template to fix. This only counts
    /// what that refusal cost and says it where a person is looking.
    ///
    /// **IT IS COUNTED OFF THE RUNS' OWN MATCHES**, which is where the refusal was recorded, so
    /// the count and the reason are one record and not two.
    /// </summary>
    public static class TreesNotWritten
    {
        /// <summary>
        /// Every plot of this press that lost a tree, in the order the plots' runs were made,
        /// with the plots that lost none left out.
        /// </summary>
        public static IReadOnlyList<PlotTreesNotWritten> Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            var order = new List<string>();
            var byPlot = new Dictionary<string, List<TreeNotWritten>>(StringComparer.Ordinal);

            foreach (KpiCreateRun run in set.Runs)
            {
                if (run == null || run.Plan == null) continue;

                foreach (SpeciesMatch match in run.Plan.Matches)
                {
                    if (match == null || match.Placed) continue;

                    foreach (PlotNumber one in match.Species.PerPlot)
                    {
                        int trees = (int)one.Value;
                        if (trees == 0) continue;

                        List<TreeNotWritten> already;
                        if (!byPlot.TryGetValue(one.PlotId, out already))
                        {
                            already = new List<TreeNotWritten>();
                            byPlot[one.PlotId] = already;
                            order.Add(one.PlotId);
                        }

                        already.Add(new TreeNotWritten(
                            one.PlotId, match.Species.BotanicalName, trees, match.Why));
                    }
                }
            }

            return order.Select(one => new PlotTreesNotWritten(one, byPlot[one])).ToList();
        }

        /// <summary>
        /// What one plot lost, for the plot list's column, or empty where it lost nothing.
        /// </summary>
        public static string For(IEnumerable<PlotTreesNotWritten> plots, string plotId)
        {
            PlotTreesNotWritten found = (plots ?? Enumerable.Empty<PlotTreesNotWritten>())
                .FirstOrDefault(one => string.Equals(one.PlotId, plotId, StringComparison.Ordinal));

            return found == null ? string.Empty : found.InWords;
        }

        /// <summary>
        /// The one line the glance carries: how many trees on how many plots were written
        /// nowhere. **A press that lost none says so**, because a section that disappears when
        /// there is nothing to say reads the same as one nobody wrote.
        /// </summary>
        public static string InWords(IEnumerable<PlotTreesNotWritten> plots)
        {
            List<PlotTreesNotWritten> held = (plots ?? Enumerable.Empty<PlotTreesNotWritten>()).ToList();
            int trees = held.Sum(one => one.Trees);

            if (trees == 0)
            {
                return Heading + ": every tree this press counted reached a row.";
            }

            return Heading + ": " + trees + (trees == 1 ? " tree on " : " trees on ")
                + held.Count + (held.Count == 1 ? " plot was" : " plots were")
                + " written nowhere, so those plots' workbooks are short by that many.";
        }

        public const string Heading = "THE TREES WRITTEN NOWHERE";
    }
}
