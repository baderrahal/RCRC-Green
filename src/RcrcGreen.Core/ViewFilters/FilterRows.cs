using System.Linq;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// The rows a press acts on: prefixes trimmed, rows with a blank prefix dropped. This is
    /// the ported host's own normalisation, moved here whole so the run and the scan read
    /// one rule. It trims the prefix on the row object itself, which is what the host did.
    /// </summary>
    public static class FilterRows
    {
        public static FilterConfig[] Kept(FilterConfig[] filters)
        {
            return (filters ?? new FilterConfig[0])
                .Where(f => f != null && !string.IsNullOrWhiteSpace(f.Prefix))
                .Select(f => { f.Prefix = f.Prefix.Trim(); return f; })
                .ToArray();
        }
    }
}
