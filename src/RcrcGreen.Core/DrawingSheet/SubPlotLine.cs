using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One line of step 1's sub plot list, and what it says about that sub plot.
    ///
    /// The range offered 01 to 99 and the list under it showed only sub plots the model holds,
    /// so widening the range changed nothing on screen and the user reported it as not working.
    /// The list holds every sub plot the range covers now, and each line says which it is.
    ///
    /// **A sub plot the user types into a range is not one the tool invented.** The rule that
    /// the tool never invents a plot is about the tool, and it still holds: nothing here adds a
    /// sub plot nobody asked for. DM-02 is the case that settles it. It holds no views and no
    /// scope box, and the team has already made sheets for it.
    /// </summary>
    public sealed class SubPlotLine
    {
        internal SubPlotLine(string plotId, bool inTheModel, bool hasScopeBox, int views)
        {
            PlotId = plotId ?? string.Empty;
            InTheModel = inTheModel;
            HasScopeBox = hasScopeBox;
            Views = views;
        }

        public string PlotId { get; }

        /// <summary>
        /// Whether the model holds this sub plot at all, on a view, a scope box, a sheet or a
        /// tagged element. False is ordinary: it is a sub plot the range covers and the model
        /// has not reached yet.
        /// </summary>
        public bool InTheModel { get; }

        public bool HasScopeBox { get; }

        public int Views { get; }

        /// <summary>
        /// What the line says after the identifier. The most limiting fact first, because that
        /// is what decides whether a run on it will make anything.
        /// </summary>
        public string InWords()
        {
            if (!InTheModel) return "not in the model";

            string counted = Views == 1 ? "1 view" : Views + " views";
            if (Views == 0) counted = "no views";

            return HasScopeBox ? counted : "no scope box, " + counted;
        }

        public override string ToString()
        {
            return PlotId + "   " + InWords();
        }
    }

    /// <summary>
    /// The lines of one plot's sub plot list, over the sub plots its range covers.
    /// </summary>
    public static class SubPlotLines
    {
        public static IReadOnlyList<SubPlotLine> Over(
            IEnumerable<string> inRange,
            IEnumerable<string> inTheModel,
            IEnumerable<string> withAScopeBox,
            IEnumerable<PlotViewPresence> present)
        {
            var held = new HashSet<string>(
                (inTheModel ?? Enumerable.Empty<string>()).Where(one => one != null),
                StringComparer.Ordinal);

            var boxed = new HashSet<string>(
                (withAScopeBox ?? Enumerable.Empty<string>()).Where(one => one != null),
                StringComparer.Ordinal);

            var counted = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (PlotViewPresence one in
                (present ?? Enumerable.Empty<PlotViewPresence>()).Where(one => one != null))
            {
                int already;
                counted[one.Where.PlotId] =
                    counted.TryGetValue(one.Where.PlotId, out already) ? already + 1 : 1;
            }

            var lines = new List<SubPlotLine>();
            foreach (string plotId in
                (inRange ?? Enumerable.Empty<string>()).Where(one => !string.IsNullOrEmpty(one)))
            {
                int views;
                if (!counted.TryGetValue(plotId, out views)) views = 0;

                lines.Add(new SubPlotLine(
                    plotId, held.Contains(plotId), boxed.Contains(plotId), views));
            }

            return lines;
        }

        /// <summary>
        /// What the count line over the list says. Both numbers, because a range covering 99
        /// sub plots of which the model holds 16 is the thing somebody needs to see before
        /// ticking all of them.
        /// </summary>
        public static string InWords(IReadOnlyList<SubPlotLine> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return "This range covers no sub plot. Check the From and To.";
            }

            int held = lines.Count(one => one.InTheModel);

            if (held == lines.Count)
            {
                return lines.Count == 1
                    ? "1 sub plot, and the model holds it."
                    : lines.Count + " sub plots, and the model holds them all.";
            }

            return lines.Count + " sub plots, " + held + " of them in the model. The rest can "
                + "still be ticked, and sheets can be made for them.";
        }
    }
}
