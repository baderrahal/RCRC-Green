using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which plots inside the range the user is actually working on.
    ///
    /// The range picks a block. Real work drops plots out of the middle of it, DM-11 to DM-28
    /// but not DM-15 and not DM-20, and doing that by narrowing the range is impossible. So
    /// every plot in range carries a tick, everything starts ticked, and unticking takes a
    /// plot out of the counts and out of anything that writes.
    /// </summary>
    public sealed class PlotSelection
    {
        public static readonly PlotSelection Nothing = AllOf(null);

        private readonly HashSet<string> _inRange;
        private readonly HashSet<string> _off;

        private PlotSelection(IReadOnlyList<string> inRange, HashSet<string> off)
        {
            InRange = inRange;
            _inRange = new HashSet<string>(inRange, StringComparer.Ordinal);
            _off = off;
        }

        /// <summary>
        /// Everything ticked. The range changing builds one of these afresh, which is what
        /// resets the ticks.
        /// </summary>
        public static PlotSelection AllOf(IEnumerable<string> inRange)
        {
            return new PlotSelection(Real(inRange), new HashSet<string>(StringComparer.Ordinal));
        }

        public IReadOnlyList<string> InRange { get; }

        public IReadOnlyList<string> Ticked
        {
            get { return InRange.Where(plotId => !_off.Contains(plotId)).ToList(); }
        }

        public int InRangeCount
        {
            get { return InRange.Count; }
        }

        public int TickedCount
        {
            get { return InRange.Count(plotId => !_off.Contains(plotId)); }
        }

        /// <summary>
        /// False for a plot that is not in the range at all. A plot the user cannot see is not
        /// ticked, whatever else is true of it.
        /// </summary>
        public bool IsTicked(string plotId)
        {
            return plotId != null && _inRange.Contains(plotId) && !_off.Contains(plotId);
        }

        /// <summary>
        /// A plot outside the range is ignored rather than added, so nothing the user cannot
        /// see can end up ticked and acted on.
        /// </summary>
        public PlotSelection Ticking(string plotId, bool ticked)
        {
            if (plotId == null || !_inRange.Contains(plotId)) return this;

            var off = new HashSet<string>(_off, StringComparer.Ordinal);
            if (ticked) off.Remove(plotId);
            else off.Add(plotId);

            return new PlotSelection(InRange, off);
        }

        private static IReadOnlyList<string> Real(IEnumerable<string> plotIds)
        {
            return (plotIds ?? Enumerable.Empty<string>())
                .Where(plotId => !string.IsNullOrEmpty(plotId))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
    }
}
