using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What was decided about one view, and everything the report needs to say why.
    /// </summary>
    public sealed class ViewScopeBoxDecision
    {
        public ViewScopeBoxDecision(
            long viewId,
            string viewName,
            ScopeBoxCase outcome,
            string plotId,
            string currentScopeBoxName,
            string scopeBoxToAssign)
        {
            if (viewName == null) throw new ArgumentNullException("viewName");

            ViewId = viewId;
            ViewName = viewName;
            Outcome = outcome;
            PlotId = plotId ?? string.Empty;
            CurrentScopeBoxName = currentScopeBoxName ?? string.Empty;
            ScopeBoxToAssign = scopeBoxToAssign ?? string.Empty;
        }

        public long ViewId { get; }

        public string ViewName { get; }

        public ScopeBoxCase Outcome { get; }

        /// <summary>
        /// The plot read off the front of the view name, or empty when the name did not parse.
        /// </summary>
        public string PlotId { get; }

        public string CurrentScopeBoxName { get; }

        /// <summary>
        /// Filled in only for <see cref="ScopeBoxCase.ReadyToAssign"/>. Every other case leaves
        /// it empty, because every other case changes nothing.
        /// </summary>
        public string ScopeBoxToAssign { get; }
    }
}
