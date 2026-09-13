using System.Collections.Generic;

namespace RcrcGreen.Core.ViewFilters
{
    /// <summary>
    /// What reading ViewFilters.json gave: the rows, or the reason there are none. The two
    /// never travel together, because a settings file that fails open reads exactly like one
    /// holding no rows, and a check that cannot see its subject has to say so.
    /// </summary>
    public sealed class ViewFiltersFileRead
    {
        private ViewFiltersFileRead(IReadOnlyList<FilterConfig> rows, string problem)
        {
            Rows = rows;
            Problem = problem ?? string.Empty;
        }

        public IReadOnlyList<FilterConfig> Rows { get; }

        /// <summary>Empty when the rows are good.</summary>
        public string Problem { get; }

        public bool WasRead
        {
            get { return Problem.Length == 0; }
        }

        public static ViewFiltersFileRead Good(IReadOnlyList<FilterConfig> rows)
        {
            return new ViewFiltersFileRead(rows, string.Empty);
        }

        public static ViewFiltersFileRead Refused(string problem)
        {
            return new ViewFiltersFileRead(new List<FilterConfig>(), problem);
        }
    }
}
