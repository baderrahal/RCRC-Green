using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The order the team's sheets come in, read off both measured models and confirmed by
    /// the user. It is also the sheet letter order: 010001A is TITLE SHEET and 010001B is
    /// LIST OF DRAWINGS because of this list, not because of when anybody ticked anything.
    ///
    /// Sheets order by their view code first, then by this list within a code, then anything
    /// not on the list in the order it was ticked. The list sits here rather than in the
    /// panel so it can change without touching a control.
    /// </summary>
    public static class SheetOrder
    {
        public static readonly IReadOnlyList<string> TheTeamsOrder = new[]
        {
            "TITLE SHEET",
            "LIST OF DRAWINGS",
            "PROJECT LOCATION KEY PLAN",
            "OVERALL KEYPLAN",
            "GENERAL ARRANGEMENT LAYOUT",
            "COORDINATION LAYOUT",
            "LANDSCAPE CROSS SECTION",
            "HARDSCAPE SCHEDULES",
            "SOFTSCAPE SCHEDULES"
        };

        /// <summary>
        /// Where a sheet name sits in the team's order. A name not on the list reads as the
        /// list's length, so everything listed comes before anything that is not, and the
        /// unlisted keep the order they arrived in.
        /// </summary>
        public static int Position(string sheetName)
        {
            string wanted = (sheetName ?? string.Empty).Trim().ToUpper(CultureInfo.InvariantCulture);

            for (int at = 0; at < TheTeamsOrder.Count; at++)
            {
                if (string.Equals(TheTeamsOrder[at], wanted, StringComparison.Ordinal)) return at;
            }

            return TheTeamsOrder.Count;
        }

        /// <summary>
        /// The code a sheet orders under: the first view's code, empty for a sheet with no
        /// views. A sheet mixing codes gets no NUMBER built, but for order it still sits
        /// somewhere, and its first view's code is the least invented answer.
        /// </summary>
        public static string CodeToOrderBy(IReadOnlyList<ViewType> views)
        {
            return views == null || views.Count == 0 ? string.Empty : views[0].Code;
        }

        /// <summary>
        /// A sheet with no code sorts ahead of the coded ones, because the one such sheet the
        /// team makes is the title sheet and it is the first sheet of both measured sets,
        /// 010001A and 010QE. Where a codeless sheet that is not a title sheet should sit is
        /// not written down anywhere and is the team's to answer.
        /// </summary>
        public static int CodeRank(string code)
        {
            return string.IsNullOrEmpty(code) ? 0 : 1;
        }
    }
}
