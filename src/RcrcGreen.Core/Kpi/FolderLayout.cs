using System;
using System.IO;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// How the plot folders nest under the output folder. **There is no default**, Bader's
    /// decision of 29 October: Setup offers both with neither ticked and Create stays off until
    /// one is picked, because a layout nobody chose is a folder tree nobody looks in.
    /// </summary>
    public enum FolderLayout
    {
        None,

        /// <summary>Output\&lt;value folder&gt;\&lt;UID2&gt;.</summary>
        Flat,

        /// <summary>Output\&lt;main&gt;\&lt;value folder&gt;\&lt;UID2&gt;, one level types alone.</summary>
        Grouped
    }

    public static class FolderLayouts
    {
        public const string FlatName = "Flat";

        public const string GroupedName = "Grouped";

        /// <summary>A layout off its remembered text, case blind, and None for anything else.</summary>
        public static FolderLayout Parse(string text)
        {
            string held = (text ?? string.Empty).Trim();
            if (string.Equals(held, FlatName, StringComparison.OrdinalIgnoreCase)) return FolderLayout.Flat;
            if (string.Equals(held, GroupedName, StringComparison.OrdinalIgnoreCase)) return FolderLayout.Grouped;
            return FolderLayout.None;
        }

        /// <summary>The text a layout is remembered as, empty for None.</summary>
        public static string Name(FolderLayout layout)
        {
            switch (layout)
            {
                case FolderLayout.Flat: return FlatName;
                case FolderLayout.Grouped: return GroupedName;
                default: return string.Empty;
            }
        }

        /// <summary>
        /// The folder under the output folder for one placed value, before the plot's own UID2.
        /// Grouped puts the value folder under its main, and a value with no main, the one level
        /// types, is its folder alone in both layouts. Empty for None, which files nothing.
        /// </summary>
        public static string Under(FolderLayout layout, string folder, string main)
        {
            string held = (folder ?? string.Empty).Trim();
            string over = (main ?? string.Empty).Trim();
            if (layout == FolderLayout.None || held.Length == 0) return string.Empty;

            return layout == FolderLayout.Grouped && over.Length > 0
                ? Path.Combine(over, held)
                : held;
        }

        /// <summary>The first lines of the create report name the layout the press used.</summary>
        public static string InWords(FolderLayout layout)
        {
            switch (layout)
            {
                case FolderLayout.Flat:
                    return "Folder layout: Flat. Output\\<value folder>\\<UID2>, and the one level "
                        + "types Output\\<folder>\\<UID2>.";
                case FolderLayout.Grouped:
                    return "Folder layout: Grouped. Output\\<main>\\<value folder>\\<UID2>, and the "
                        + "one level types Output\\<folder>\\<UID2>.";
                default:
                    return "Folder layout: none picked, so no path was built.";
            }
        }
    }
}
