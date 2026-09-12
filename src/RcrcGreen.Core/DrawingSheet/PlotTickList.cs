using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Step 1's list: every two letter prefix in the model is a plot the user ticks,
    /// several at once, with a From and To over the sub plot numbers and a tick per sub
    /// plot inside that range. One run covers every ticked sub plot across every ticked
    /// plot, which the single prefix and range this replaced could not say.
    ///
    /// **The range is free and the list is not.** From and To offer 01 to 99 whatever the
    /// open model holds, because the team works across models and a range that stopped at
    /// the current model's highest sub plot could not be set up before that model existed.
    /// The list under them still shows only the sub plots this model really holds inside
    /// the range, so nothing here invents a plot: a free range picker is not a plot, it is
    /// two numbers. The count line says how many of the range exist here.
    ///
    /// It composes the Shared pieces rather than restating their rules. PlotRange orders
    /// and narrows to a prefix, and each ticked plot carries its own PlotSelection, so a
    /// sub plot tick behaves exactly as the old flat list's did: changing a plot's range
    /// rebuilds its selection with everything ticked, and nothing outside the model can be
    /// ticked at all.
    /// </summary>
    public sealed class PlotTickList
    {
        private readonly IReadOnlyList<string> _plotIds;

        private readonly IReadOnlyList<string> _prefixes;

        private readonly Dictionary<string, TickedPlot> _ticked;

        private PlotTickList(
            IReadOnlyList<string> plotIds, Dictionary<string, TickedPlot> ticked)
        {
            _plotIds = plotIds;
            _prefixes = PlotRange.PrefixesIn(plotIds);
            _ticked = ticked;
        }

        public static readonly PlotTickList Nothing = Over(null);

        /// <summary>
        /// The lowest and highest number a range end can take. Two digits, because every
        /// sub plot on both measured models is two digits and the user asked for these.
        /// </summary>
        public const int LowestNumber = 1;

        public const int HighestNumber = 99;

        /// <summary>
        /// What the From and To lists offer, 01 to 99, the same on every model. The digits
        /// alone, because the plot is picked above them and repeating its letters on every
        /// row of a 99 line list says nothing.
        /// </summary>
        public static readonly IReadOnlyList<string> RangeEnds = BuildRangeEnds();

        private static IReadOnlyList<string> BuildRangeEnds()
        {
            var ends = new List<string>(HighestNumber);
            for (int number = LowestNumber; number <= HighestNumber; number++)
            {
                ends.Add(AsEnd(number));
            }

            return ends;
        }

        public static string AsEnd(int number)
        {
            return number.ToString("00", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The model's plots with nothing ticked yet. A fresh read starts here, the same
        /// way the old range started unpicked.
        /// </summary>
        public static PlotTickList Over(IEnumerable<string> plotIds)
        {
            var real = (plotIds ?? Enumerable.Empty<string>())
                .Where(plotId => !string.IsNullOrEmpty(plotId))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return new PlotTickList(
                real, new Dictionary<string, TickedPlot>(StringComparer.Ordinal));
        }

        /// <summary>
        /// The plots the list offers, every two letter prefix the model holds, in order.
        /// </summary>
        public IReadOnlyList<string> Prefixes
        {
            get { return _prefixes; }
        }

        public bool IsPlotTicked(string prefix)
        {
            return prefix != null && _ticked.ContainsKey(prefix);
        }

        public IReadOnlyList<string> TickedPlots
        {
            get { return _prefixes.Where(prefix => _ticked.ContainsKey(prefix)).ToList(); }
        }

        public int TickedPlotCount
        {
            get { return _prefixes.Count(prefix => _ticked.ContainsKey(prefix)); }
        }

        /// <summary>
        /// Every sub plot under one plot the model holds, in natural order, whether or not
        /// it is in the range. This is the model's list, not the range's.
        /// </summary>
        public IReadOnlyList<string> UnderPlot(string prefix)
        {
            return PlotRange.WithPrefix(_plotIds, prefix);
        }

        /// <summary>
        /// Ticking a plot on takes the whole 01 to 99 range with every sub plot the model
        /// holds inside it ticked, so one tick is one click's worth of work. Ticking it off
        /// forgets its range and its sub plot ticks, the same way changing the old range
        /// rebuilt the selection. A prefix the model does not hold is ignored rather than
        /// added, because a plot is never invented here.
        /// </summary>
        public PlotTickList TickingPlot(string prefix, bool ticked)
        {
            if (prefix == null || !_prefixes.Contains(prefix, StringComparer.Ordinal))
            {
                return this;
            }

            var now = new Dictionary<string, TickedPlot>(_ticked, StringComparer.Ordinal);

            if (!ticked)
            {
                if (!now.Remove(prefix)) return this;
                return new PlotTickList(_plotIds, now);
            }

            if (now.ContainsKey(prefix)) return this;

            string from = AsEnd(LowestNumber);
            string to = AsEnd(HighestNumber);
            now[prefix] = new TickedPlot(from, to, Selection(prefix, from, to));

            return new PlotTickList(_plotIds, now);
        }

        /// <summary>
        /// The sub plots inside one ticked plot's range, in order, the lines its block
        /// draws. Empty for a plot that is not ticked.
        /// </summary>
        public IReadOnlyList<string> InRangeOf(string prefix)
        {
            TickedPlot held;
            return prefix != null && _ticked.TryGetValue(prefix, out held)
                ? held.Picked.InRange
                : new List<string>();
        }

        /// <summary>
        /// The in-range sub plots of one plot whose identifier holds the search text, which
        /// is what the search box narrows to and what All and None act on. Case and edge
        /// spaces are forgiven, because somebody typing 2 to find DM-02 should not have to
        /// know how the model pads its numbers.
        /// </summary>
        public IReadOnlyList<string> Matching(string prefix, string search)
        {
            string wanted = (search ?? string.Empty).Trim();
            IReadOnlyList<string> inRange = InRangeOf(prefix);

            if (wanted.Length == 0) return inRange;

            return inRange
                .Where(plotId => plotId.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        /// <summary>
        /// The From end a ticked plot shows, empty for one that is not ticked.
        /// </summary>
        public string FromOf(string prefix)
        {
            TickedPlot held;
            return prefix != null && _ticked.TryGetValue(prefix, out held)
                ? held.From : string.Empty;
        }

        public string ToOf(string prefix)
        {
            TickedPlot held;
            return prefix != null && _ticked.TryGetValue(prefix, out held)
                ? held.To : string.Empty;
        }

        /// <summary>
        /// Changing a plot's range rebuilds its selection with every sub plot in the new
        /// range ticked, the rule the old range had, and leaves every other plot alone.
        /// The ends are kept as given even when they come back empty, reversed or hold no
        /// sub plot at all, so what is shown is what was picked rather than a correction.
        /// </summary>
        public PlotTickList Ranging(string prefix, string from, string to)
        {
            if (prefix == null || !_ticked.ContainsKey(prefix)) return this;

            var now = new Dictionary<string, TickedPlot>(_ticked, StringComparer.Ordinal);
            now[prefix] = new TickedPlot(
                from ?? string.Empty,
                to ?? string.Empty,
                Selection(prefix, from, to));

            return new PlotTickList(_plotIds, now);
        }

        /// <summary>
        /// How many numbers the ticked plots' ranges cover altogether, which is what the
        /// count line holds the model's own total against. A reversed or unreadable pair
        /// covers nothing.
        /// </summary>
        public int NumbersInRange
        {
            get
            {
                int covered = 0;
                foreach (string prefix in TickedPlots)
                {
                    TickedPlot held = _ticked[prefix];
                    int first = NumberIn(held.From);
                    int last = NumberIn(held.To);
                    if (first > 0 && last >= first) covered += (last - first) + 1;
                }

                return covered;
            }
        }

        /// <summary>
        /// Every sub plot in range across the ticked plots, plot order first and natural
        /// order within one, whatever order the plots were ticked in.
        /// </summary>
        public IReadOnlyList<string> InRange
        {
            get
            {
                var all = new List<string>();
                foreach (string prefix in TickedPlots)
                {
                    all.AddRange(_ticked[prefix].Picked.InRange);
                }

                return all;
            }
        }

        public IReadOnlyList<string> Ticked
        {
            get
            {
                var all = new List<string>();
                foreach (string prefix in TickedPlots)
                {
                    all.AddRange(_ticked[prefix].Picked.Ticked);
                }

                return all;
            }
        }

        public int InRangeCount
        {
            get { return _ticked.Values.Sum(one => one.Picked.InRangeCount); }
        }

        public int TickedCount
        {
            get { return _ticked.Values.Sum(one => one.Picked.TickedCount); }
        }

        public bool IsTicked(string plotId)
        {
            TickedPlot held;
            return plotId != null
                && _ticked.TryGetValue(PlotId.PrefixOf(plotId), out held)
                && held.Picked.IsTicked(plotId);
        }

        /// <summary>
        /// A sub plot tick routes to its own plot's selection, which ignores anything
        /// outside its range, so nothing the user cannot see can end up ticked and acted
        /// on. The rule is PlotSelection's, asked rather than repeated.
        /// </summary>
        public PlotTickList Ticking(string plotId, bool ticked)
        {
            if (plotId == null) return this;

            string prefix = PlotId.PrefixOf(plotId);
            TickedPlot held;
            if (!_ticked.TryGetValue(prefix, out held)) return this;

            var now = new Dictionary<string, TickedPlot>(_ticked, StringComparer.Ordinal);
            now[prefix] = new TickedPlot(held.From, held.To, held.Picked.Ticking(plotId, ticked));

            return new PlotTickList(_plotIds, now);
        }

        /// <summary>
        /// What All and None do: one tick each over the sub plots the search is showing,
        /// as one change rather than a redraw per line.
        /// </summary>
        public PlotTickList TickingThese(IEnumerable<string> plotIds, bool ticked)
        {
            PlotTickList list = this;
            foreach (string plotId in plotIds ?? Enumerable.Empty<string>())
            {
                list = list.Ticking(plotId, ticked);
            }

            return list;
        }

        /// <summary>
        /// The sub plots this model holds under one prefix whose number falls inside the
        /// range, all ticked. The range ends are two numbers rather than two sub plots now,
        /// so the model decides what exists and the user decides what is wanted.
        /// </summary>
        private PlotSelection Selection(string prefix, string from, string to)
        {
            int first = NumberIn(from);
            int last = NumberIn(to);
            if (first <= 0 || last < first) return PlotSelection.AllOf(null);

            return PlotSelection.AllOf(UnderPlot(prefix)
                .Where(plotId =>
                {
                    int number = NumberOf(plotId);
                    return number >= first && number <= last;
                })
                .ToList());
        }

        private static int NumberIn(string end)
        {
            int number;
            return int.TryParse(
                (end ?? string.Empty).Trim(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out number)
                ? number
                : 0;
        }

        /// <summary>
        /// The digits of a plot identifier, DM-02 giving 2. PlotId in Shared holds the
        /// shape and offers the prefix but not this half, and Shared is another round's
        /// change, so the read is here and guarded by PlotId's own check.
        /// </summary>
        private static int NumberOf(string plotId)
        {
            if (!PlotId.IsPlotId(plotId)) return 0;
            return NumberIn(plotId.Substring(plotId.IndexOf('-') + 1));
        }

        private sealed class TickedPlot
        {
            public TickedPlot(string from, string to, PlotSelection picked)
            {
                From = from;
                To = to;
                Picked = picked;
            }

            public string From { get; }

            public string To { get; }

            public PlotSelection Picked { get; }
        }
    }
}
