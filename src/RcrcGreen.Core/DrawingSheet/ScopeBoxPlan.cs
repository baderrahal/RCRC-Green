using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Sorts every view into one of the six cases, and says which of them are safe to write.
    ///
    /// A new view with no scope box is useless on this project, so this is part of the Drawing
    /// Sheet work rather than a tool of its own. Only <see cref="ScopeBoxCase.ReadyToAssign"/>
    /// writes anything. A view that already carries a scope box is never overwritten, whether
    /// that box is the right one or not.
    /// </summary>
    public sealed class ScopeBoxPlan
    {
        private ScopeBoxPlan(IReadOnlyList<ViewScopeBoxDecision> decisions)
        {
            Decisions = decisions;
        }

        public IReadOnlyList<ViewScopeBoxDecision> Decisions { get; }

        public IEnumerable<ViewScopeBoxDecision> ToAssign
        {
            get { return Of(ScopeBoxCase.ReadyToAssign); }
        }

        /// <summary>
        /// Views and scope box names come in as plain values. Matching a scope box to a plot is
        /// exact and case sensitive, the same rule the plot identifier itself follows, so a
        /// scope box named dm-41 is not the one DM-41 is looking for.
        /// </summary>
        public static ScopeBoxPlan Decide(
            IEnumerable<ViewScopeBoxState> views,
            IEnumerable<string> scopeBoxNames)
        {
            var boxes = new HashSet<string>(StringComparer.Ordinal);
            foreach (string name in (scopeBoxNames ?? Enumerable.Empty<string>()).Where(name => name != null))
            {
                boxes.Add(name);
            }

            var decisions = new List<ViewScopeBoxDecision>();
            foreach (ViewScopeBoxState view in (views ?? Enumerable.Empty<ViewScopeBoxState>()).Where(view => view != null))
            {
                decisions.Add(DecideOne(view, boxes));
            }

            return new ScopeBoxPlan(decisions);
        }

        public IEnumerable<ViewScopeBoxDecision> Of(ScopeBoxCase outcome)
        {
            return Decisions.Where(decision => decision.Outcome == outcome);
        }

        public int Count(ScopeBoxCase outcome)
        {
            return Decisions.Count(decision => decision.Outcome == outcome);
        }

        private static ViewScopeBoxDecision DecideOne(ViewScopeBoxState view, HashSet<string> boxes)
        {
            ParsedViewName parsed;
            if (!ViewNameParser.TryParse(view.ViewName, out parsed))
            {
                // Checked before the parameter, so a view nobody is looking for reads as A
                // rather than being filed under a parameter problem it does not have.
                return Decision(view, ScopeBoxCase.NameDoesNotParse, string.Empty, string.Empty);
            }

            string plotId = parsed.PlotId;

            if (!view.CanHoldAScopeBox)
            {
                return Decision(view, ScopeBoxCase.CannotHoldAScopeBox, plotId, string.Empty);
            }

            if (!view.HasAScopeBox)
            {
                return boxes.Contains(plotId)
                    ? Decision(view, ScopeBoxCase.ReadyToAssign, plotId, plotId)
                    : Decision(view, ScopeBoxCase.NoMatchingScopeBox, plotId, string.Empty);
            }

            return string.Equals(view.CurrentScopeBoxName, plotId, StringComparison.Ordinal)
                ? Decision(view, ScopeBoxCase.AlreadyRight, plotId, string.Empty)
                : Decision(view, ScopeBoxCase.HoldsADifferentScopeBox, plotId, string.Empty);
        }

        private static ViewScopeBoxDecision Decision(
            ViewScopeBoxState view, ScopeBoxCase outcome, string plotId, string toAssign)
        {
            return new ViewScopeBoxDecision(
                view.ViewId, view.ViewName, outcome, plotId, view.CurrentScopeBoxName, toAssign);
        }
    }
}
