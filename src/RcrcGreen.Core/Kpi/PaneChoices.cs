using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which parameter a picker starts on.
    ///
    /// The name the workbook note asks for, when the model offers it, and otherwise the first
    /// the model offers. Preselecting by position alone put PRX_Plot_ID under Reference where
    /// the note names PRX_Plot_UID2, and put the first neighbourhood parameter under Location
    /// rather than Neighborhood Name.
    ///
    /// It is a preselection and not a decision. Every one of these is a picker the user can
    /// change, and the pane shows which is chosen.
    /// </summary>
    public static class Preselected
    {
        public static string From(IReadOnlyList<string> offered, string wanted)
        {
            if (offered == null || offered.Count == 0) return string.Empty;

            string found = offered.FirstOrDefault(
                one => string.Equals(one, wanted, StringComparison.Ordinal));

            return found ?? offered[0];
        }
    }
}
