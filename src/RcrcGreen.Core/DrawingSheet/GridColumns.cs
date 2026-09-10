using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Which view types the grid shows, out of every one the model holds plus any the user has
    /// added.
    ///
    /// Nothing is ticked when the model is first read. The real model holds 84 types and
    /// nobody works on 84 at once, so starting with all of them on gave a grid too wide to
    /// read and a hidden count of zero that said nothing.
    ///
    /// This is the only record of what is ticked. The interface renders itself from here every
    /// time it changes rather than letting a tick box remember its own state, because the count
    /// and the list disagreed once already and that is how.
    /// </summary>
    public sealed class GridColumns
    {
        private readonly HashSet<ViewType> _shown;
        private readonly HashSet<ViewType> _added;

        private GridColumns(IReadOnlyList<ViewType> all, HashSet<ViewType> shown, HashSet<ViewType> added)
        {
            All = all;
            _shown = shown;
            _added = added;
        }

        /// <summary>
        /// Every type the model holds, none of them ticked.
        /// </summary>
        public static GridColumns Over(IEnumerable<ViewType> inTheModel)
        {
            return new GridColumns(Ordered(inTheModel), new HashSet<ViewType>(), new HashSet<ViewType>());
        }

        /// <summary>
        /// Every type in column order, ticked or not, and whether the model holds it or the
        /// user asked for it.
        /// </summary>
        public IReadOnlyList<ViewType> All { get; }

        public IReadOnlyList<ViewType> Shown
        {
            get { return All.Where(one => _shown.Contains(one)).ToList(); }
        }

        public int ShownCount
        {
            get { return All.Count(one => _shown.Contains(one)); }
        }

        public bool IsShown(ViewType one)
        {
            return one != null && _shown.Contains(one);
        }

        /// <summary>
        /// True for a type the user asked for that no view in the model carries yet. It draws
        /// missing on every plot, which is right, and it is marked so nobody reads an empty
        /// column as a model problem.
        /// </summary>
        public bool IsNew(ViewType one)
        {
            return one != null && _added.Contains(one);
        }

        /// <summary>
        /// The codes actually in use, in number order. These fill the code buttons and the
        /// code list on the add row, so a code the model has never used cannot be picked.
        /// </summary>
        public IReadOnlyList<string> CodesInUse
        {
            get
            {
                return All.Select(one => one.Code)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(code => code, NaturalOrder.Comparer)
                    .ToList();
            }
        }

        /// <summary>
        /// The types whose code or view name holds the text, ignoring case. An empty search
        /// matches everything, so the list is never blank because nothing was typed.
        /// </summary>
        public IReadOnlyList<ViewType> Matching(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return All;

            string wanted = search.Trim();
            return All.Where(one =>
                    one.Code.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0
                    || one.ViewName.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        public GridColumns Showing(ViewType one, bool shown)
        {
            return ShowingThese(new[] { one }, shown);
        }

        /// <summary>
        /// All and None act on what the search is showing, not on the whole list. Turning off
        /// 84 types to reach the four wanted is not a thing anyone should have to do.
        /// </summary>
        public GridColumns ShowingThese(IEnumerable<ViewType> these, bool shown)
        {
            var shownNow = new HashSet<ViewType>(_shown);
            bool changed = false;

            foreach (ViewType one in (these ?? Enumerable.Empty<ViewType>()).Where(one => one != null))
            {
                if (!All.Contains(one)) continue;

                changed |= shown ? shownNow.Add(one) : shownNow.Remove(one);
            }

            return changed ? new GridColumns(All, shownNow, _added) : this;
        }

        /// <summary>
        /// Ticks every type carrying that code, leaving whatever else is ticked alone. On this
        /// model that is how someone gets all six 600 schedules in one click.
        /// </summary>
        public GridColumns ShowingCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return this;

            return ShowingThese(
                All.Where(one => string.Equals(one.Code, code, StringComparison.Ordinal)),
                true);
        }

        /// <summary>
        /// A view type the user wants that the model does not hold. This is what the tool is
        /// for, so it is allowed. Inventing a plot is not, and nothing here can.
        /// </summary>
        public GridColumns Adding(ViewType one)
        {
            if (one == null || All.Contains(one)) return this;

            var all = Ordered(All.Concat(new[] { one }));
            var shown = new HashSet<ViewType>(_shown) { one };
            var added = new HashSet<ViewType>(_added) { one };

            return new GridColumns(all, shown, added);
        }

        /// <summary>
        /// Keeps the ticks and the added types across a refresh.
        ///
        /// An added type that the model now holds stops being new, because the run that made it
        /// worked and it is an ordinary column from here on.
        /// </summary>
        public GridColumns OverTheseTypes(IEnumerable<ViewType> inTheModel)
        {
            IReadOnlyList<ViewType> fromTheModel = Ordered(inTheModel);
            var inTheModelNow = new HashSet<ViewType>(fromTheModel);

            var added = new HashSet<ViewType>(_added.Where(one => !inTheModelNow.Contains(one)));
            IReadOnlyList<ViewType> all = Ordered(fromTheModel.Concat(added));

            var shown = new HashSet<ViewType>(all.Where(one => _shown.Contains(one)));
            return new GridColumns(all, shown, added);
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
