using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Text on its way onto a button or a tick box, made safe to read.
    ///
    /// WPF treats an underscore in a button's text as an access key marker: it swallows the
    /// first one and underlines the letter after it. The pane showed PRXComponent, PRXPlot_ID,
    /// PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH, none of which is a name this model holds.
    /// **The whole tool turns on exact parameter names**, so a pane printing a name that is not
    /// in the model is worse here than almost anywhere.
    ///
    /// Doubling the underscore is the escape WPF itself defines. Nothing else is changed, and
    /// the string a comparison uses is never this one: only what goes on screen.
    ///
    /// This runs on the KPI pane. The Drawing Sheet has the same fault on its own labels and is
    /// not touched, which is written down in steps/log.md rather than left to be rediscovered.
    /// </summary>
    public static class PaneLabel
    {
        public const char AccessKeyMarker = '_';

        public static string Escaped(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

            return text.Replace(
                AccessKeyMarker.ToString(),
                new string(AccessKeyMarker, 2));
        }
    }

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
