using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One ticked plot, its PRX_Plot_UID2 and the file the press would write for it.
    ///
    /// **THE FILE PATH IS WHAT COLLIDES, NOT THE VALUE.** Two plots carrying one UID2 in two
    /// different component folders are filed apart, so nothing overwrites anything and both are
    /// written. The path is built by <see cref="PlotWorkbookPath"/>, the one rule that decides
    /// where a plot's workbook goes, so this check and the write cannot part.
    /// </summary>
    public sealed class PlotFiling
    {
        public PlotFiling(string plotId, string uid2, PlotWorkbookPath where)
        {
            if (string.IsNullOrWhiteSpace(plotId)) throw new ArgumentNullException("plotId");

            PlotId = plotId.Trim();
            Uid2 = (uid2 ?? string.Empty).Trim();
            Where = where;
        }

        public string PlotId { get; }

        public string Uid2 { get; }

        /// <summary>Where the press would file it, or a refusal. Null where nothing worked it out.</summary>
        public PlotWorkbookPath Where { get; }

        public string FolderPath
        {
            get { return Where != null && Where.Ok ? Where.FolderPath : string.Empty; }
        }

        public string FilePath
        {
            get { return Where != null && Where.Ok ? Where.FilePath : string.Empty; }
        }
    }

    /// <summary>
    /// **WHERE EVERY TICKED PLOT'S WORKBOOK WOULD GO, BUILT ONCE.** The press and the pane both
    /// ask this, and both of them reach <see cref="PlotWorkbookPath.For"/> through it rather than
    /// working a path out beside the thing that uses one. Two records of one fact is the shape
    /// this repository keeps paying for, and a collision check reading a different path from the
    /// writer would report on files nobody writes.
    /// </summary>
    public static class PlotFilings
    {
        /// <summary>
        /// One plot's filing, for a caller that already knows which template the plot belongs to.
        /// The press is that caller: the split placed the plot before the read began.
        /// </summary>
        public static PlotFiling One(
            string root, KpiTemplate template, string plotId, string component, string uid2)
        {
            return new PlotFiling(plotId, uid2, PlotWorkbookPath.For(root, template, component, uid2));
        }

        /// <summary>
        /// Every plot's filing, for a caller that knows only what the model holds per plot. The
        /// pane is that caller: nothing has been read for a press yet, so the template comes off
        /// <see cref="PlotsPerTemplate.For"/>, which is the rule the split itself uses.
        ///
        /// **A PLOT NO ROUTE PLACES IS CARRIED WITH THE REFUSAL IT ALREADY HAD**, rather than
        /// dropped, so it is counted where it is counted and collides with nothing.
        /// </summary>
        public static IReadOnlyList<PlotFiling> Of(
            string root,
            IEnumerable<string> plotIds,
            Func<string, string> componentOf,
            Func<string, string> uid2Of)
        {
            var found = new List<PlotFiling>();

            foreach (string plotId in (plotIds ?? Enumerable.Empty<string>()))
            {
                if (string.IsNullOrWhiteSpace(plotId)) continue;

                string component = (componentOf == null ? string.Empty : componentOf(plotId)) ?? string.Empty;
                string uid2 = (uid2Of == null ? string.Empty : uid2Of(plotId)) ?? string.Empty;

                PlotTemplate placed = PlotsPerTemplate.For(plotId, component);

                found.Add(placed.Template == null
                    ? new PlotFiling(plotId, uid2, PlotWorkbookPath.Refused(placed.Why))
                    : One(root, placed.Template, plotId, component, uid2));
            }

            return found;
        }

        /// <summary>This plot's filing, or null where the list does not hold it.</summary>
        public static PlotFiling For(IEnumerable<PlotFiling> filings, string plotId)
        {
            string held = (plotId ?? string.Empty).Trim();

            return (filings ?? Enumerable.Empty<PlotFiling>())
                .Where(one => one != null)
                .FirstOrDefault(one => string.Equals(one.PlotId, held, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// Every ticked plot carrying one PRX_Plot_UID2, and whether their files land on one path.
    /// </summary>
    public sealed class SharedUid2Group
    {
        public SharedUid2Group(string uid2, IEnumerable<PlotFiling> plots)
        {
            Uid2 = (uid2 ?? string.Empty).Trim();
            Plots = (plots ?? Enumerable.Empty<PlotFiling>()).Where(one => one != null).ToList();
        }

        public string Uid2 { get; }

        public IReadOnlyList<PlotFiling> Plots { get; }

        /// <summary>
        /// The plots of this group whose file really lands on one path, keyed on that path. A
        /// path only one plot reaches is not in here, because nothing of it collides.
        ///
        /// **THE PATH IS WHAT DECIDES, AND IT IS COMPARED WITHOUT CASE.** Windows folders ignore
        /// letter case, so `ANH-007-ST-100213` and `anh-007-st-100213` name one folder and one
        /// file, and a comparison that read them as two values would let a press write one over
        /// the other with every row of THE PLOT LIST reading YES.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<PlotFiling>> Colliding
        {
            get
            {
                return Plots
                    .Where(one => one.FilePath.Length > 0)
                    .GroupBy(one => one.FilePath, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group => (IReadOnlyList<PlotFiling>)group.ToList())
                    .ToList();
            }
        }

        public IReadOnlyList<string> StoppedPlots
        {
            get { return Colliding.SelectMany(one => one).Select(one => one.PlotId).ToList(); }
        }

        /// <summary>
        /// Whether this group's plots are filed apart, which is the case the tool names and lets
        /// write. True only when nothing in it collides.
        /// </summary>
        public bool FiledApart
        {
            get { return Colliding.Count == 0; }
        }
    }

    /// <summary>
    /// **TWO PLOTS, ONE FOLDER, AND THE LAST ONE WRITTEN REPLACED THE OTHERS.** Bader's decision
    /// of 16 September.
    ///
    /// Measured on the 16:37 press: NS-01 and NS-42 both carry PRX_Plot_UID2
    /// ANH-007-ST-100210, and MM-01 and MM-09 to MM-15 all carry ANH-007-ST-100213. ONE WORKBOOK
    /// PER PLOT gives each group one path, THE PLOT LIST read YES for all ten, and each group
    /// took one street reference row, so MM-01 and MM-09 to MM-15 all read ROW 10 and length
    /// 0.06108. On the 16:06 press three more groups collided: DM-11 with DM-21 and DM-29 on
    /// ANH-007-MO-100001, FM-05 with FM-08 and FM-09 on ANH-007-MO-100019, and EP-01 with EP-05,
    /// EP-11, EP-12 and EP-13 on ANH-007-NP-100015. **The empty DM-29 files replaced DM-11's.**
    ///
    /// **NO FILE IS WRITTEN FOR ANY PLOT IN SUCH A GROUP.** Writing one of them and refusing the
    /// rest would pick a plot nobody chose, and writing all of them is what happened.
    ///
    /// **FILES ALREADY IN THAT FOLDER FROM AN EARLIER PRESS ARE LEFT WHERE THEY ARE**, the same
    /// rule the crash row already follows: deleting them destroys the evidence and the team's
    /// own earlier work, and saying nothing about them leaves somebody opening a folder tree.
    ///
    /// **THE WHOLE TICKED SET IS CHECKED BEFORE THE FIRST FILE OF A PRESS IS WRITTEN.** The press
    /// reads plots per template and writes that template before it reads the next, so a check
    /// inside one template's loop would let the first template's files land before the second
    /// template's collision was known.
    /// </summary>
    public static class SharedUid2
    {
        public const string Heading = "THE PLOTS SHARING ONE PRX_Plot_UID2";

        /// <summary>
        /// Every PRX_Plot_UID2 two or more ticked plots carry, in the order the plots came.
        /// **A plot with no UID2 is left out**, because a plot with none is refused by its own
        /// path already and an empty value shared by five plots is not one value.
        ///
        /// **THE GROUPING IGNORES LETTER CASE, BECAUSE A WINDOWS FOLDER DOES.** It was
        /// <see cref="StringComparer.Ordinal"/>, so `ANH-007-ST-100213` and
        /// `anh-007-st-100213` were two values to this check and one folder to Windows: the two
        /// plots never met, nothing compared their paths, and the press wrote one workbook over
        /// the other. **What really collides is the path**, which
        /// <see cref="SharedUid2Group.Colliding"/> decides inside each group and also without
        /// case, and the path is the component folder and the trimmed value, so two plots reach
        /// one path exactly when they are in one group here. Grouping on the value with the
        /// path deciding inside it keeps the plots that share a value across two component
        /// folders together, which is the case that is named and still writes.
        ///
        /// **EVERY PLOT KEEPS ITS OWN SPELLING.** The group carries the first value seen for its
        /// own line, and every line about a plot prints that plot's value exactly as the model
        /// holds it, because a report that tidied one of two spellings would hide the very
        /// difference somebody has to go and fix.
        /// </summary>
        public static IReadOnlyList<SharedUid2Group> Of(IEnumerable<PlotFiling> plots)
        {
            var order = new List<string>();
            var byUid2 = new Dictionary<string, List<PlotFiling>>(StringComparer.OrdinalIgnoreCase);

            foreach (PlotFiling one in (plots ?? Enumerable.Empty<PlotFiling>()).Where(one => one != null))
            {
                if (one.Uid2.Length == 0) continue;

                List<PlotFiling> already;
                if (!byUid2.TryGetValue(one.Uid2, out already))
                {
                    already = new List<PlotFiling>();
                    byUid2[one.Uid2] = already;
                    order.Add(one.Uid2);
                }

                already.Add(one);
            }

            return order
                .Where(one => byUid2[one].Count > 1)
                .Select(one => new SharedUid2Group(byUid2[one][0].Uid2, byUid2[one]))
                .ToList();
        }

        /// <summary>Whether this plot's file would land on a path another ticked plot reaches.</summary>
        public static bool Stops(IEnumerable<SharedUid2Group> groups, string plotId)
        {
            return Group(groups, plotId, true) != null;
        }

        /// <summary>
        /// Why no file is written for this plot, naming this plot's own value, every other
        /// ticked plot on that path WITH ITS OWN VALUE, and the one path they would all land on.
        /// Empty for a plot nothing stops.
        ///
        /// **WHERE THE SPELLINGS DIFFER THE LINE SAYS SO.** Two values that differ only in
        /// letter case are one folder on Windows and look like two values on a screen, so the
        /// sentence that explains why they collide is the one thing a person needs to go and fix
        /// the model.
        /// </summary>
        public static string WhyStopped(IEnumerable<SharedUid2Group> groups, string plotId)
        {
            SharedUid2Group group = Group(groups, plotId, true);
            if (group == null) return string.Empty;

            IReadOnlyList<PlotFiling> colliding = group.Colliding
                .First(one => one.Any(plot => string.Equals(plot.PlotId, plotId, StringComparison.Ordinal)));

            PlotFiling mine = colliding
                .First(one => string.Equals(one.PlotId, plotId, StringComparison.Ordinal));

            var others = colliding
                .Where(one => !string.Equals(one.PlotId, plotId, StringComparison.Ordinal))
                .ToList();

            return "this plot's " + KpiNames.PlotUid2 + " is " + mine.Uid2 + ", and "
                + NamedWithValues(others) + ". " + WhereTheSpellingsDiffer(mine, others)
                + "One workbook per plot files all " + Count(colliding.Count)
                + " at " + colliding[0].FilePath
                + ", so the last one written would replace the others. No file is written for any "
                + "of them, and nothing already in that folder is touched.";
        }

        /// <summary>
        /// The sentence that is printed only where two of these values are not spelt the same.
        /// Empty where they all are, because a line about nothing is one the team reads past on
        /// every other press.
        /// </summary>
        private static string WhereTheSpellingsDiffer(PlotFiling mine, IReadOnlyList<PlotFiling> others)
        {
            bool differ = others.Any(
                one => !string.Equals(one.Uid2, mine.Uid2, StringComparison.Ordinal));

            return differ
                ? "Those spellings differ only in letter case, and Windows files them in one "
                    + "folder under one name anyway. "
                : string.Empty;
        }

        /// <summary>
        /// Each plot named with the value it really carries. **Nothing tidies a spelling**,
        /// because the difference between two of them is what somebody has to go and correct.
        /// </summary>
        private static string NamedWithValues(IReadOnlyList<PlotFiling> plots)
        {
            var said = plots
                .Select(one => one.PlotId + " carries " + one.Uid2)
                .ToList();

            if (said.Count == 0) return "no other ticked plot carries it";
            if (said.Count == 1) return said[0];

            return string.Join(", ", said.Take(said.Count - 1).ToArray())
                + " and " + said[said.Count - 1];
        }

        /// <summary>
        /// The same refusal as one line of a table, for the plot list's own row. Empty for a plot
        /// nothing stops. **It asks <see cref="Stops"/> rather than deciding again**, so a row
        /// saying a plot is held back and a press that wrote its file cannot both be true.
        /// </summary>
        public static string StoppedShort(IEnumerable<SharedUid2Group> groups, string plotId)
        {
            SharedUid2Group group = Group(groups, plotId, true);
            if (group == null) return string.Empty;

            IReadOnlyList<PlotFiling> colliding = group.Colliding
                .First(one => one.Any(plot => string.Equals(plot.PlotId, plotId, StringComparison.Ordinal)));

            PlotFiling mine = colliding
                .First(one => string.Equals(one.PlotId, plotId, StringComparison.Ordinal));

            var others = colliding
                .Where(one => !string.Equals(one.PlotId, plotId, StringComparison.Ordinal))
                .ToList();

            return KpiNames.PlotUid2 + " " + mine.Uid2 + " files this plot where "
                + NamedWithValues(others) + " files, so no file is written for any of them";
        }

        /// <summary>
        /// **THE SAME VALUE IN DIFFERENT COMPONENT FOLDERS IS NAMED AND STILL WRITES.** Nothing
        /// of this plot's collides, so refusing would cost the team a workbook over a value that
        /// files apart. Empty for a plot whose value is its own and for one that is stopped.
        ///
        /// **IT SAYS ONLY WHAT IS TRUE OF THIS PLOT.** A group can hold two plots colliding with
        /// each other and a third filed somewhere else entirely, and a line reading that every
        /// one of them is written would be wrong about two of the three.
        /// </summary>
        public static string FiledApartFrom(IEnumerable<SharedUid2Group> groups, string plotId)
        {
            SharedUid2Group group = Group(groups, plotId, false);
            if (group == null) return string.Empty;

            PlotFiling mine = group.Plots
                .First(one => string.Equals(one.PlotId, plotId, StringComparison.Ordinal));

            var others = group.Plots
                .Where(one => !string.Equals(one.PlotId, plotId, StringComparison.Ordinal))
                .ToList();

            return "this plot's " + KpiNames.PlotUid2 + " is " + mine.Uid2 + ", and "
                + NamedWithValues(others) + ". " + WhereTheSpellingsDiffer(mine, others)
                + "No file of this plot's collides with any of theirs, so it is written.";
        }

        /// <summary>
        /// The one line the glance carries: how many plots were stopped, and the values. **A
        /// press that stopped none says so**, because a section that disappears when there is
        /// nothing to report reads the same as one nobody wrote.
        /// </summary>
        public static string InWords(IEnumerable<SharedUid2Group> groups)
        {
            List<SharedUid2Group> held = (groups ?? Enumerable.Empty<SharedUid2Group>()).ToList();
            var stopped = held.SelectMany(one => one.StoppedPlots).ToList();

            if (stopped.Count == 0)
            {
                return Heading + ": every ticked plot's " + KpiNames.PlotUid2
                    + " files it on a path of its own.";
            }

            var values = held
                .Where(one => one.Colliding.Count > 0)
                .Select(one => one.Uid2)
                .ToList();

            return Heading + ": " + stopped.Count
                + (stopped.Count == 1 ? " plot was" : " plots were")
                + " written nowhere because " + (values.Count == 1 ? "another ticked plot shares"
                    : "other ticked plots share")
                + " its " + KpiNames.PlotUid2 + " and its folder. "
                + (values.Count == 1 ? "The value: " : "The values: ")
                + string.Join(", ", values.ToArray()) + ".";
        }

        /// <summary>
        /// What an earlier press left in that folder, named and left alone. **Nothing is
        /// deleted**, so this is a list of what to go and look at.
        /// </summary>
        public static string AlreadyThere(IEnumerable<string> paths)
        {
            List<string> held = (paths ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .ToList();

            if (held.Count == 0) return "that folder holds no file of this plot's from an earlier press";

            return "an earlier press left " + Count(held.Count) + " in that folder, "
                + string.Join(", ", held.ToArray())
                + ", and nothing here deletes or moves any of them";
        }

        private static SharedUid2Group Group(
            IEnumerable<SharedUid2Group> groups, string plotId, bool colliding)
        {
            string held = (plotId ?? string.Empty).Trim();
            if (held.Length == 0) return null;

            foreach (SharedUid2Group group in (groups ?? Enumerable.Empty<SharedUid2Group>()))
            {
                bool holds = group.Plots.Any(
                    one => string.Equals(one.PlotId, held, StringComparison.Ordinal));
                if (!holds) continue;

                bool stops = group.StoppedPlots.Any(
                    one => string.Equals(one, held, StringComparison.Ordinal));

                if (stops == colliding) return group;
            }

            return null;
        }

        private static string Count(int number)
        {
            return number.ToString(CultureInfo.InvariantCulture)
                + (number == 1 ? " file" : " files");
        }
    }
}
