using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One line of the team's plot list file, with the line number it was on.
    ///
    /// **THE LINE NUMBER IS THE WHOLE POINT.** A line that is not a plot is reported back to a
    /// person who has the file open in front of them, and a plot named twice is two line numbers
    /// rather than a count.
    /// </summary>
    public sealed class PlotListLine
    {
        public PlotListLine(int number, string text)
        {
            Number = number;
            Text = (text ?? string.Empty).Trim();
        }

        /// <summary>The line's own number in the file, counting the first line as 1.</summary>
        public int Number { get; }

        /// <summary>The line with its edge whitespace off. Never empty, blanks are skipped.</summary>
        public string Text { get; }

        public bool IsPlot
        {
            get { return PlotId.IsPlotId(Text); }
        }
    }

    /// <summary>
    /// One thing wrong with the file, named with every line it is on.
    /// </summary>
    public sealed class PlotListFault
    {
        public PlotListFault(string what, IEnumerable<int> lines, string why)
        {
            What = (what ?? string.Empty).Trim();
            Lines = (lines ?? Enumerable.Empty<int>()).ToList();
            Why = (why ?? string.Empty).Trim();
        }

        public string What { get; }

        public IReadOnlyList<int> Lines { get; }

        public string Why { get; }

        public string InWords
        {
            get
            {
                return "'" + What + "' on "
                    + (Lines.Count == 1 ? "line " : "lines ")
                    + string.Join(" and ", Lines.Select(
                        one => one.ToString(CultureInfo.InvariantCulture)).ToArray())
                    + ": " + Why;
            }
        }
    }

    /// <summary>
    /// **THE TEAM SENT 154 PLOTS TO EXPORT AND WANTS EVERY ONE EXPORTED WITH NONE SKIPPED.** The
    /// pane takes a plain text file of them, one plot per line.
    ///
    /// **THE FILE NEVER ENTERS THIS REPOSITORY.** It is the client's own list of which plots are
    /// in scope, this repository is public, and every test below builds its own in the temp
    /// folder, the same rule the workbooks and the PDFs already follow.
    ///
    /// Six rules, all tested.
    ///
    /// **EDGE WHITESPACE COMES OFF EACH LINE AND BLANK LINES ARE SKIPPED**, because a list typed
    /// or pasted by a person carries both and neither is a plot.
    ///
    /// **THE FILE'S ORDER IS KEPT.** The report reads down it beside the team's own copy, so a
    /// sorted list would be a second order nobody asked for.
    ///
    /// **A LINE THAT IS NOT A PLOT IDENTIFIER IS NAMED WITH ITS LINE NUMBER** and is not a plot.
    /// It never becomes one and nothing is guessed out of it.
    ///
    /// **A PLOT LISTED TWICE IS NAMED WITH BOTH LINE NUMBERS** and is counted once. A list of 154
    /// that holds one plot twice is a list of 153 plots and somebody has to know which.
    ///
    /// **PLOTS ARE COMPARED THE WAY THE PANE'S OWN PLOT LIST COMPARES THEM**, which is
    /// <see cref="StringComparer.Ordinal"/>, because that is what <see cref="PlotTicks"/> and
    /// <see cref="PlotsInTheModel.Holds"/> use. A second comparison here would be two rules for
    /// one question, which is the fault this repository keeps paying for. **A plot in the wrong
    /// case is not a plot at all**, which is the Shared rule and is why `dm-11` lands among the
    /// lines that are not plots rather than quietly becoming DM-11.
    ///
    /// **A FILE THAT CANNOT BE READ IS A REFUSAL WITH ITS REASON, never an empty list**, because
    /// an empty list and a file nobody could open tick exactly the same nothing.
    /// </summary>
    public sealed class PlotListRead
    {
        private PlotListRead(
            string path, bool read, string why, IEnumerable<PlotListLine> lines,
            IEnumerable<string> plots, IEnumerable<PlotListFault> notPlots,
            IEnumerable<PlotListFault> repeated)
        {
            Path = (path ?? string.Empty).Trim();
            Read = read;
            Why = (why ?? string.Empty).Trim();
            Lines = (lines ?? Enumerable.Empty<PlotListLine>()).ToList();
            Plots = (plots ?? Enumerable.Empty<string>()).ToList();
            NotPlots = (notPlots ?? Enumerable.Empty<PlotListFault>()).ToList();
            Repeated = (repeated ?? Enumerable.Empty<PlotListFault>()).ToList();
        }

        public const string NoFileSet =
            "no plot list file is set, so every plot is ticked by hand or by a template row";

        public static readonly PlotListRead NotSet =
            new PlotListRead(string.Empty, false, NoFileSet, null, null, null, null);

        public static PlotListRead Refused(string path, string why)
        {
            if (string.IsNullOrWhiteSpace(why)) throw new ArgumentException("A refusal needs a reason.", "why");

            return new PlotListRead(path, false, why, null, null, null, null);
        }

        /// <summary>Whether a file was set at all, told apart from one that could not be read.</summary>
        public bool Set
        {
            get { return Path.Length > 0; }
        }

        public string Path { get; }

        public bool Read { get; }

        /// <summary>Empty on a file that was read. Never empty on one that was not.</summary>
        public string Why { get; }

        /// <summary>Every line that held anything, in the file's own order.</summary>
        public IReadOnlyList<PlotListLine> Lines { get; }

        /// <summary>
        /// The plots the file names, in the file's order, each once however many lines carry it.
        /// </summary>
        public IReadOnlyList<string> Plots { get; }

        /// <summary>Every line that is not a plot identifier, with its line number.</summary>
        public IReadOnlyList<PlotListFault> NotPlots { get; }

        /// <summary>Every plot the file names more than once, with all of its line numbers.</summary>
        public IReadOnlyList<PlotListFault> Repeated { get; }

        public bool Faultless
        {
            get { return Read && NotPlots.Count == 0 && Repeated.Count == 0; }
        }

        /// <summary>Which line numbers name this plot, so a repeat can be shown beside it.</summary>
        public IReadOnlyList<int> LinesFor(string plotId)
        {
            return Lines
                .Where(one => string.Equals(one.Text, plotId, StringComparison.Ordinal))
                .Select(one => one.Number)
                .ToList();
        }

        public string InWords
        {
            get
            {
                if (!Read) return Why;

                return Plots.Count + (Plots.Count == 1 ? " plot" : " plots")
                    + " over " + Lines.Count + (Lines.Count == 1 ? " line" : " lines")
                    + (NotPlots.Count == 0
                        ? string.Empty
                        : ", " + NotPlots.Count + (NotPlots.Count == 1 ? " line that is not a plot" : " lines that are not plots"))
                    + (Repeated.Count == 0
                        ? string.Empty
                        : ", " + Repeated.Count + (Repeated.Count == 1 ? " plot listed twice" : " plots listed more than once"))
                    + ".";
            }
        }

        /// <summary>
        /// The whole rule, over the lines exactly as the file holds them.
        /// </summary>
        public static PlotListRead Of(string path, IEnumerable<string> lines)
        {
            var held = new List<PlotListLine>();
            int number = 0;

            foreach (string one in lines ?? Enumerable.Empty<string>())
            {
                number++;

                var line = new PlotListLine(number, one);
                if (line.Text.Length == 0) continue;

                held.Add(line);
            }

            var plots = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var notPlots = new List<PlotListFault>();

            foreach (PlotListLine one in held)
            {
                if (!one.IsPlot)
                {
                    notPlots.Add(new PlotListFault(one.Text, new[] { one.Number }, NotAPlot(one.Text)));
                    continue;
                }

                if (seen.Add(one.Text)) plots.Add(one.Text);
            }

            var repeated = plots
                .Select(one => new
                {
                    Plot = one,
                    Lines = held.Where(line => string.Equals(line.Text, one, StringComparison.Ordinal))
                        .Select(line => line.Number)
                        .ToList()
                })
                .Where(one => one.Lines.Count > 1)
                .Select(one => new PlotListFault(
                    one.Plot, one.Lines,
                    "this plot is listed " + one.Lines.Count + " times and is counted once"))
                .ToList();

            return new PlotListRead(path, true, string.Empty, held, plots, notPlots, repeated);
        }

        /// <summary>
        /// Why a line is not a plot, in words a person can act on with the file open. **A
        /// lowercase identifier is told apart from text that was never one**, through the Shared
        /// rule, because they need different answers.
        /// </summary>
        public static string NotAPlot(string text)
        {
            return PlotId.FailsOnlyOnCase(text)
                ? "a plot identifier is two UPPERCASE letters, a dash and digits, so this is not "
                    + "one. Nothing changes its case, because a plot in the wrong case is not a "
                    + "different plot, it is invalid"
                : "this is not a plot identifier, which is two uppercase letters, a dash and digits";
        }
    }

    /// <summary>
    /// Where the plot list file is read from.
    /// </summary>
    public static class PlotListFile
    {
        public static PlotListRead In(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return PlotListRead.NotSet;

            try
            {
                return PlotListRead.Of(path, File.ReadAllLines(path));
            }
            catch (IOException failed)
            {
                return PlotListRead.Refused(path,
                    "the plot list file could not be read. " + failed.Message);
            }
            catch (UnauthorizedAccessException denied)
            {
                return PlotListRead.Refused(path,
                    "the plot list file was refused. " + denied.Message);
            }
        }
    }
}
