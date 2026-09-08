using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which view types the grid shows.
    ///
    /// Every type found in the model is a column from the start. Adding them one at a time was
    /// the wrong way round on a model that holds dozens, because the user opens the panel to
    /// find out what is missing and an empty grid answers nothing. Hiding is the exception, so
    /// hiding is what the interface offers.
    /// </summary>
    public sealed class GridColumns
    {
        private readonly HashSet<ViewType> _hidden;

        private GridColumns(IReadOnlyList<ViewType> all, HashSet<ViewType> hidden)
        {
            All = all;
            _hidden = hidden;
        }

        public static GridColumns ShowingAll(IEnumerable<ViewType> inTheModel)
        {
            return new GridColumns(Ordered(inTheModel), new HashSet<ViewType>());
        }

        /// <summary>
        /// Every type the model holds, in column order, hidden ones included.
        /// </summary>
        public IReadOnlyList<ViewType> All { get; }

        public IReadOnlyList<ViewType> Shown
        {
            get { return All.Where(one => !_hidden.Contains(one)).ToList(); }
        }

        public int HiddenCount
        {
            get { return All.Count(one => _hidden.Contains(one)); }
        }

        public bool IsShown(ViewType one)
        {
            return one != null && !_hidden.Contains(one);
        }

        /// <summary>
        /// A new set with one type shown or hidden. Asking about a type the model does not
        /// hold changes nothing, so a stale click cannot invent a column.
        /// </summary>
        public GridColumns Showing(ViewType one, bool shown)
        {
            if (one == null || !All.Contains(one)) return this;

            var hidden = new HashSet<ViewType>(_hidden);
            if (shown) hidden.Remove(one);
            else hidden.Add(one);

            return new GridColumns(All, hidden);
        }

        /// <summary>
        /// Keeps whatever is still in the model hidden and forgets the rest, so a refresh that
        /// drops a view type does not carry a hidden entry nothing can ever show again.
        /// </summary>
        public GridColumns OverTheseTypes(IEnumerable<ViewType> inTheModel)
        {
            IReadOnlyList<ViewType> all = Ordered(inTheModel);
            var hidden = new HashSet<ViewType>(all.Where(one => _hidden.Contains(one)));
            return new GridColumns(all, hidden);
        }

        private static IReadOnlyList<ViewType> Ordered(IEnumerable<ViewType> types)
        {
            return (types ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Distinct()
                .OrderBy(one => one)
                .ToList();
        }
    }
}
