using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One column header: what fits across the top, and what it stands for.
    /// </summary>
    public sealed class GridColumnLabel
    {
        public GridColumnLabel(ViewType type, string shortLabel)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            Short = shortLabel ?? string.Empty;
        }

        public ViewType Type { get; }

        /// <summary>
        /// What the column header shows.
        /// </summary>
        public string Short { get; }

        /// <summary>
        /// The whole view type, for the tooltip and the key under the grid.
        /// </summary>
        public string Full
        {
            get { return Type.ToString(); }
        }

        /// <summary>
        /// True when the short label leaves something out, so the key only lists the columns
        /// that need it.
        /// </summary>
        public bool Shortened
        {
            get { return !string.Equals(Short, Full, StringComparison.Ordinal); }
        }

        public override string ToString()
        {
            return Short;
        }
    }

    /// <summary>
    /// Column headers narrow enough to read across a docked pane.
    ///
    /// Eight ticked view types ran off the right edge, and scrolling sideways was the only way
    /// to reach the last of them. A header reading (200) General Arrangement Layout is 30
    /// characters wide for a column of one square.
    ///
    /// So the header is the code. Code 010 appears twice in the real model with two different
    /// view names, which is the whole reason `ViewType` holds both, so a code on its own cannot
    /// be the answer. Where a code is shared, the label takes as many leading words of the view
    /// name as it needs to tell the sharers apart, and no more.
    /// </summary>
    public static class GridColumnLabels
    {
        public static IReadOnlyList<GridColumnLabel> For(IEnumerable<ViewType> shown)
        {
            List<ViewType> columns = (shown ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .ToList();

            return columns
                .Select(one => new GridColumnLabel(one, Label(one, SharingTheCode(columns, one))))
                .ToList();
        }

        private static List<ViewType> SharingTheCode(List<ViewType> columns, ViewType one)
        {
            return columns
                .Where(other => !other.Equals(one))
                .Where(other => string.Equals(other.Code, one.Code, StringComparison.Ordinal))
                .ToList();
        }

        private static string Label(ViewType one, List<ViewType> sharing)
        {
            string code = "(" + one.Code + ")";
            if (sharing.Count == 0) return code;

            string[] words = one.ViewName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            for (int howMany = 1; howMany <= words.Length; howMany++)
            {
                string tried = code + " " + string.Join(" ", words.Take(howMany).ToArray());

                if (sharing.All(other => !StartsTheSame(code, other, howMany, tried)))
                {
                    return tried;
                }
            }

            // One name is the start of another, word for word. Nothing shorter than the whole
            // thing separates them, so the whole thing is what the header carries.
            return one.ToString();
        }

        private static bool StartsTheSame(string code, ViewType other, int howMany, string tried)
        {
            string[] words = other.ViewName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < howMany) return false;

            return string.Equals(
                code + " " + string.Join(" ", words.Take(howMany).ToArray()),
                tried,
                StringComparison.Ordinal);
        }
    }
}
