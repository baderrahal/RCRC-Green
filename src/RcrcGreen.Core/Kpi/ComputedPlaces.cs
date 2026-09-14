using System.Collections.Generic;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The two cells the WORKBOOK computes that the PDF also asks for, found by their labels and
    /// **read only, never written**.
    ///
    /// **MEASURED BY BADER ON ALL SEVEN TEMPLATES, 14 September.** The round before could not
    /// build this guard, because the text of neither formula was measured anywhere and the one
    /// thing near it was H9 reading H8/Area on EXISTING PARKS, one template. It is measured now:
    ///
    /// <code>
    /// TOTAL GREEN COVER, the same shape on all seven in two row layouts
    ///   EXISTING PARKS, FUTURE PARKS, STREETS    D9 = F9+F11+H11
    ///   HEALTHCARE, MOSQUES, PARKING, SCHOOLS    D8 = F8+F10+H10
    ///
    /// PERCENTAGE CANOPY, in SECTION 3 and not section 1
    ///   HEALTHCARE, MOSQUES, PARKING, SCHOOLS    label C31, value E31 = IF(Area&lt;1," ",F8/Area)
    ///   STREETS                                  label C32, value E32 = IF(Area&lt;1," ",F9/Area)
    ///   EXISTING PARKS and FUTURE PARKS          NO SUCH LABEL AT ALL
    /// </code>
    ///
    /// Canopy plus planting plus lawn, and canopy over area, which is what this tool already
    /// computes. **The two row layouts are why neither cell is a letter here**, exactly as row 7
    /// and row 5 taught: a map putting the green cover at D8 because four templates out of seven
    /// do would read the wrong row on the other three.
    ///
    /// **THE NOTE IS WRONG ABOUT WHERE THE PERCENTAGE IS.** The client's own PDF note says the
    /// cell to the right of `Total area covered by canopy`. **There is no such label in section 1
    /// on any template.** It is in section 3, labelled `% of Total area covered by canopy`, and
    /// its value sits TWO columns right of the label rather than one, the cell one to the right
    /// being empty on all five that carry it. That is the second note on these forms measured to
    /// be wrong about its own subject, after the TOTAL Shrubs tooltip.
    ///
    /// **AND THE ONE FORM THAT ASKS FOR THE PERCENTAGE IS FED BY THE TWO TEMPLATES THAT DO NOT
    /// CARRY IT.** Percentage Total area covered by canopy is on the Parks PDF alone, which EP
    /// and FP plots reach, and EXISTING PARKS and FUTURE PARKS have no such cell. So this check
    /// answers nothing to check on every run the tool makes today. That is a fact about the
    /// client's files rather than a fault to fix: **the tool still computes and writes the
    /// number**, because it holds the canopy and the area, and the report says the workbook has
    /// no cell to hold it against. The check is built because it is right and because it fires
    /// the day a park template grows the cell or another form grows the field.
    /// </summary>
    public static class ComputedPlaces
    {
        public const string GreenCoverName = "Total Green cover";

        public const string PercentageName = "Percentage canopy";

        /// <summary>
        /// **Taken from the client's own PDF note**, which reads
        /// `excel the cell on the right of  "Total Green cover (m²) "`, and confirmed by where it
        /// lands: the cell one column right of it is D9 on three templates and D8 on four, which
        /// is what was measured. The report prints the cell the label chose on every run, so a
        /// template whose label reads anything else is one line rather than a silence.
        /// </summary>
        public const string GreenCoverLabel = "Total Green cover (m²)";

        /// <summary>
        /// **Spelt exactly as the cell reads**, per cent sign and all. It is NOT
        /// `Total area covered by canopy`, which the note names and no template carries in
        /// section 1.
        /// </summary>
        public const string PercentageLabel = "% of Total area covered by canopy";

        /// <summary>
        /// **The value sits TWO columns right of the percentage label and one right of the green
        /// cover label.** The distance is measured rather than assumed, and the cell one right of
        /// the percentage label is empty on all five templates that carry it.
        /// </summary>
        public static readonly IReadOnlyList<LabelledPlace> All = new[]
        {
            new LabelledPlace(GreenCoverName, GreenCoverLabel, 1),
            new LabelledPlace(PercentageName, PercentageLabel, 2)
        };
    }
}
