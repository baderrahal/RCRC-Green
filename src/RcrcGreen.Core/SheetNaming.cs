using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The name a sheet gets from the one view it holds.
    ///
    /// Real sheets in this model read 010QF LIST OF DRAWINGS, 200Q GENERAL ARRANGEMENT LAYOUT,
    /// 400Q LANDSCAPE CROSS SECTION: the view type's name with the bracketed code removed, in
    /// upper case. A run that put one typed name onto three sheets produced a sheet called
    /// GENERAL ARRANGEMENT LAYOUT holding a location key plan, so the name comes from the view
    /// now and the typed name is only for a sheet holding more than one.
    /// </summary>
    public static class SheetNaming
    {
        /// <summary>
        /// Upper case of the view name alone. The code never appears, because it is already the
        /// front of the sheet number.
        /// </summary>
        public static string FromView(ViewType type)
        {
            if (type == null) throw new ArgumentNullException("type");

            return type.ViewName.Trim().ToUpper(CultureInfo.InvariantCulture);
        }
    }
}
