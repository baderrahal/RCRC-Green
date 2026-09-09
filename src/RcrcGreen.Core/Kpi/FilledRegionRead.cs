using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One filled region inside a linked model, with every parameter on it. Only the first few
    /// are read this deep, because the point is to see what a filled region carries rather
    /// than to list thousands.
    /// </summary>
    public sealed class FilledRegionRead
    {
        public FilledRegionRead(string typeName, string viewName, IEnumerable<ReadParameter> parameters)
        {
            TypeName = typeName ?? string.Empty;
            ViewName = viewName ?? string.Empty;
            Parameters = (parameters ?? Enumerable.Empty<ReadParameter>()).Where(one => one != null).ToList();
        }

        public string TypeName { get; }

        /// <summary>
        /// The view the region is drawn in. The workbook says ID FILLED REGION and nothing more,
        /// and the view name is one of the two places that word could be.
        /// </summary>
        public string ViewName { get; }

        public IReadOnlyList<ReadParameter> Parameters { get; }
    }
}
