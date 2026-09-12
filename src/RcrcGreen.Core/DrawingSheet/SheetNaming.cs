using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The FALLBACK name for a sheet holding one view, used only when neither sheet name file
    /// holds the view type.
    ///
    /// This used to be the rule, read off three examples that happened to match: 010QF LIST
    /// OF DRAWINGS, 200Q GENERAL ARRANGEMENT LAYOUT, 400Q LANDSCAPE CROSS SECTION. Four of
    /// DM-11's eight sheets disprove it. OVERALL KEYPLAN, PROJECT LOCATION KEY PLAN,
    /// HARDSCAPE SCHEDULES and SOFTSCAPE SCHEDULES all differ from their view names upper
    /// cased, so the name comes from <see cref="SheetNameSettings"/> now and this is what a
    /// view type falls back to when neither file names it, said as derived on the panel.
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
