using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Narrows the plot list down to the stretch the user is working on.
    ///
    /// The first real model holds 160 plots. A grid of all of them at once is unreadable and
    /// slow to build, so the panel works on a prefix and a run of numbers inside it. Every
    /// plot offered here comes from the model, so the tool can never act on a plot that does
    /// not exist.
    /// </summary>
    public static class PlotRange
    {
        /// <summary>
        /// The two letter prefixes actually present, in order. Anything in the list that is
        /// not shaped like a plot identifier has no prefix and is left out.
        /// </summary>
        public static IReadOnlyList<string> PrefixesIn(IEnumerable<string> plotIds)
        {
            var found = new SortedSet<string>(StringComparer.Ordinal);

            foreach (string plotId in Real(plotIds))
            {
                string prefix = PlotId.PrefixOf(plotId);
                if (prefix.Length > 0) found.Add(prefix);
            }

            return found.ToList();
        }

        /// <summary>
        /// Every plot under one prefix, with the number part read as a number, so DM-2 comes
        /// before DM-100. These are what the From and To lists are filled with.
        /// </summary>
        public static IReadOnlyList<string> WithPrefix(IEnumerable<string> plotIds, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return new List<string>();

            return Real(plotIds)
                .Where(plotId => string.Equals(PlotId.PrefixOf(plotId), prefix, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(plotId => plotId, NaturalOrder.Comparer)
                .ToList();
        }

        /// <summary>
        /// The plots from one to another, both ends included.
        ///
        /// Empty rather than an error when the two ends are the wrong way round, or when
        /// either end is not in the model. The panel fills both lists from the model so
        /// neither should happen, and an empty grid is a better answer than a thrown
        /// exception from a modeless panel that cannot show one.
        /// </summary>
        public static IReadOnlyList<string> Between(
            IEnumerable<string> plotIds, string prefix, string from, string to)
        {
            IReadOnlyList<string> underPrefix = WithPrefix(plotIds, prefix);

            int first = IndexOf(underPrefix, from);
            int last = IndexOf(underPrefix, to);

            if (first < 0 || last < 0 || first > last) return new List<string>();

            return underPrefix.Skip(first).Take(last - first + 1).ToList();
        }

        private static int IndexOf(IReadOnlyList<string> plotIds, string wanted)
        {
            if (wanted == null) return -1;

            for (int at = 0; at < plotIds.Count; at++)
            {
                if (string.Equals(plotIds[at], wanted, StringComparison.Ordinal)) return at;
            }
            return -1;
        }

        private static IEnumerable<string> Real(IEnumerable<string> plotIds)
        {
            if (plotIds == null) return Enumerable.Empty<string>();
            return plotIds.Where(plotId => !string.IsNullOrEmpty(plotId));
        }
    }
}
