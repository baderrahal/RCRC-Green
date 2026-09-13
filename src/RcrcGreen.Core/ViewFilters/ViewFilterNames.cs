using System;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// How a filter name is built and matched: the row's prefix, one space, the plot code,
    /// as in (200-260) Presentation DM-41. Matching forgives case and edge spaces, which is
    /// the ported host's own rule. The run and the scan both ask here, so a cell the scan
    /// calls Exists is a filter the run will find, and a second copy of any of these rules
    /// is the fault this repo keeps paying for.
    /// </summary>
    public static class ViewFilterNames
    {
        public static string TargetFilterName(string prefix, string plotCode)
        {
            return ((prefix ?? string.Empty) + " " + (plotCode ?? string.Empty)).Trim();
        }

        public static bool IsExactMatch(string filterName, string targetFilterName)
        {
            return string.Compare(
                (filterName ?? string.Empty).Trim(),
                targetFilterName,
                StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsPrefixAndPlotMatch(string filterName, string prefix, string plotCode)
        {
            string held = (filterName ?? string.Empty).Trim();
            return held.StartsWith(prefix ?? string.Empty, StringComparison.OrdinalIgnoreCase)
                && held.EndsWith(plotCode ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public static bool HasPrefix(string filterName, string prefix)
        {
            return (filterName ?? string.Empty)
                .Trim()
                .StartsWith(prefix ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The tail of an exemplar's name once the prefix is off, which the host took for the
        /// exemplar's own plot code. A name no longer than its prefix has no tail, and the
        /// second edit refuses to create from one rather than letting Substring throw into
        /// the catch below it.
        /// </summary>
        public static string ExemplarPlotCode(string exemplarName, string prefix)
        {
            string held = (exemplarName ?? string.Empty).Trim();
            int off = (prefix ?? string.Empty).Length;
            if (held.Length <= off) return string.Empty;

            return held.Substring(off).Trim();
        }
    }
}
