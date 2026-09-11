using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The six case counts for one set of plots, so the panel can show what Assign would do
    /// before anyone presses it.
    ///
    /// The narrowing reads a view's plot from its name, which is the rule
    /// <see cref="ScopeBoxPlan"/> itself follows. Two rules would mean the number shown and the
    /// number written were different numbers, and the whole point of putting these on screen is
    /// that they are the same one.
    /// </summary>
    public sealed class ScopeBoxCounts
    {
        private readonly Dictionary<ScopeBoxCase, IReadOnlyList<ViewScopeBoxDecision>> _byCase;

        private ScopeBoxCounts(
            Dictionary<ScopeBoxCase, IReadOnlyList<ViewScopeBoxDecision>> byCase, int considered)
        {
            _byCase = byCase;
            Considered = considered;
        }

        public static ScopeBoxCounts For(
            IEnumerable<ViewScopeBoxState> views,
            IEnumerable<string> scopeBoxNames,
            IEnumerable<string> plotsWanted)
        {
            IReadOnlyList<ViewScopeBoxState> narrowed = Narrow(views, plotsWanted);
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(narrowed, scopeBoxNames);

            // The views themselves are kept, not just how many. F equals 1 on the real model
            // and the useful thing in that number is which view it is, which a count cannot say.
            var byCase = new Dictionary<ScopeBoxCase, IReadOnlyList<ViewScopeBoxDecision>>();
            foreach (ScopeBoxCase outcome in (ScopeBoxCase[])Enum.GetValues(typeof(ScopeBoxCase)))
            {
                byCase[outcome] = plan.Of(outcome)
                    .OrderBy(decision => decision.PlotId, NaturalOrder.Comparer)
                    .ThenBy(decision => decision.ViewName, StringComparer.Ordinal)
                    .ToList();
            }

            return new ScopeBoxCounts(byCase, narrowed.Count);
        }

        /// <summary>
        /// The views belonging to those plots, by the name on the view. The Revit side runs the
        /// same call before it writes, so what was counted is what gets decided.
        /// </summary>
        public static IReadOnlyList<ViewScopeBoxState> Narrow(
            IEnumerable<ViewScopeBoxState> views, IEnumerable<string> plotsWanted)
        {
            var wanted = new HashSet<string>(
                (plotsWanted ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);

            var narrowed = new List<ViewScopeBoxState>();
            if (wanted.Count == 0) return narrowed;

            foreach (ViewScopeBoxState view in
                (views ?? Enumerable.Empty<ViewScopeBoxState>()).Where(view => view != null))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(view.ViewName, out parsed)) continue;
                if (wanted.Contains(parsed.PlotId)) narrowed.Add(view);
            }

            return narrowed;
        }

        /// <summary>
        /// How many views these counts were worked out over. A view whose name does not parse
        /// belongs to no plot, so it is not in this number at all.
        /// </summary>
        public int Considered { get; }

        public int Of(ScopeBoxCase outcome)
        {
            return In(outcome).Count;
        }

        /// <summary>
        /// The views in one case, plot order then name order, so a person can open the one they
        /// need rather than opening the report file to find out which it is.
        /// </summary>
        public IReadOnlyList<ViewScopeBoxDecision> In(ScopeBoxCase outcome)
        {
            IReadOnlyList<ViewScopeBoxDecision> found;
            return _byCase.TryGetValue(outcome, out found)
                ? found
                : (IReadOnlyList<ViewScopeBoxDecision>)new List<ViewScopeBoxDecision>();
        }

        public int ReadyToAssign
        {
            get { return Of(ScopeBoxCase.ReadyToAssign); }
        }
    }
}
