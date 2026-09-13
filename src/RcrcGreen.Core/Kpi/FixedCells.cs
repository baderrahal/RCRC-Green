using System.Collections.Generic;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The two cells that are the same on every plot of every template.
    ///
    /// **Bader's answer: Character is always Urban Area Zone and Context is always Urban.** They
    /// come from no model, no schedule and no file, so there is nothing to read and nothing to
    /// agree with. That makes them data, here, beside the reason.
    ///
    /// **WHICH CELL EACH GOES IN IS MEASURED NOWHERE AND IS UNKNOWN.** The map was built off the
    /// annotated set cell by cell and neither word appears anywhere in it, in the rules, or in
    /// any run this repository records. The round message says the STREETS sheet is laid out
    /// differently from the mosque one, Category and Character against Character and Context, so
    /// the templates do not even carry the same pair. So <see cref="KpiTemplates"/> names no cell
    /// for either, every template reports both as not written with that reason, and the moment
    /// somebody measures them it is one line per template in the map and nothing else.
    ///
    /// A cell reference guessed here would put Urban Area Zone into whatever D4 happens to be on
    /// seven client templates, and the workbook would look filled.
    /// </summary>
    public static class FixedCells
    {
        public const string CharacterValue = "Urban Area Zone";

        public const string ContextValue = "Urban";

        /// <summary>
        /// What the pane and the report say about where these two come from. They are the
        /// team's answer rather than anything the tool found, said the way the section depth and
        /// the annotation crop are said on the other tool.
        /// </summary>
        public const string SourceInWords =
            "the same on every plot, the team's answer rather than anything read";

        /// <summary>
        /// Why a template writes neither. It names the value rather than the template, because
        /// the gap is one measurement and not a property of any one workbook.
        /// </summary>
        public const string NoCellMeasured =
            "no cell has been measured for it on any template, so nothing is written and nothing "
            + "is guessed";

        public static string ValueOf(KpiValue value)
        {
            return value == KpiValue.Character ? CharacterValue : ContextValue;
        }

        /// <summary>
        /// The two, in the order the report prints them.
        /// </summary>
        public static readonly IReadOnlyList<KpiValue> Both =
            new[] { KpiValue.Character, KpiValue.Context };
    }
}
