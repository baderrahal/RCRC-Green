using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Step 1's list: every two letter prefix in the model is a plot the user ticks,
    /// several at once, with a From and To over each ticked plot's sub plots and a tick
    /// per sub plot inside that range. One run covers every ticked sub plot across every
    /// ticked plot, which the single prefix and range this replaced could not say.
    ///
    /// It composes the Shared pieces rather than restating their rules. PlotRange narrows
    /// and orders, and each ticked plot carries its own PlotSelection, so a sub plot tick
    /// behaves exactly as the old flat list's did: changing a plot's range rebuilds its
    /// selection with everything ticked, and nothing outside the model can be ticked at
    /// all, because inventing a plot is the one thing this tool never does.
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
        /// Every sub plot under one plot, in natural order, whether or not it is in the
        /// range. These fill the From and To lists.
        /// </summary>
        public IReadOnlyList<string> UnderPlot(string prefix)
        {
            return PlotRange.WithPrefix(_plotIds, prefix);
        }

        /// <summary>
        /// Ticking a plot on takes its whole span with every sub plot ticked, so one tick
        /// is one click's worth of work. Ticking it off forgets its range and its sub plot
        /// ticks, the same way changing the old range rebuilt the selection. A prefix the
        /// model does not hold is ignored rather than added.
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

            IReadOnlyList<string> under = UnderPlot(prefix);
            string from = under.Count > 0 ? under[0] : string.Empty;
            string to = under.Count > 0 ? under[under.Count - 1] : string.Empty;

            now[prefix] = new TickedPlot(
                from, to, PlotSelection.AllOf(PlotRange.Between(_plotIds, prefix, from, to)));

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
        /// The ends are kept as given even when they come back empty, reversed or gone
        /// from the model, so what is shown is what was picked rather than a correction.
        /// </summary>
        public PlotTickList Ranging(string prefix, string from, string to)
        {
            if (prefix == null || !_ticked.ContainsKey(prefix)) return this;

            var now = new Dictionary<string, TickedPlot>(_ticked, StringComparer.Ordinal);
            now[prefix] = new TickedPlot(
                from ?? string.Empty,
                to ?? string.Empty,
                PlotSelection.AllOf(PlotRange.Between(_plotIds, prefix, from, to)));

            return new PlotTickList(_plotIds, now);
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
