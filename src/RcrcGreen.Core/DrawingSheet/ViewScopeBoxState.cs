using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view as the Revit side found it, in plain values. The identifier is whatever number
    /// the caller uses to find the view again, and Core never looks inside it.
    /// </summary>
    public sealed class ViewScopeBoxState
    {
        public ViewScopeBoxState(long viewId, string viewName, bool canHoldAScopeBox, string currentScopeBoxName)
        {
            if (viewName == null) throw new ArgumentNullException("viewName");

            ViewId = viewId;
            ViewName = viewName;
            CanHoldAScopeBox = canHoldAScopeBox;
            CurrentScopeBoxName = currentScopeBoxName ?? string.Empty;
        }

        public long ViewId { get; }

        public string ViewName { get; }

        /// <summary>
        /// False when the view has no scope box parameter at all, or has one that will not
        /// take a value.
        /// </summary>
        public bool CanHoldAScopeBox { get; }

        /// <summary>
        /// The scope box the view already carries, or empty when it carries none.
        /// </summary>
        public string CurrentScopeBoxName { get; }

        public bool HasAScopeBox
        {
            get { return CurrentScopeBoxName.Length > 0; }
        }
    }
}
