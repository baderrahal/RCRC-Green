using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What step 4 says a press would write, counted per template, said once above the rows.
    ///
    /// **IT ADDS A COUNT AND CHANGES NO WORDING.** `CreateWords.TemplateRow` is the per template
    /// row and every word of it stands exactly as it did. What was missing is the one number a
    /// person wants before they spend twenty minutes: how many workbooks this press will write
    /// in total. Reading it off six rows by hand is what a count is for.
    ///
    /// **NOTHING HERE COUNTS A PDF.** Whether a plot gets one depends on the forms folder and on
    /// `PdfForms.ForPlot`, and `TickingTheList.Lines` already names every listed plot the press
    /// would put into no PDF. A second count of the same thing worked out another way is the
    /// shape this repository keeps paying for.
    /// </summary>
    public static class KpiWillWrite
    {
        public const string NothingTicked =
            "No workbook is ticked, so this press would write nothing.";

        public const string NoPlotPlaced =
            "No ticked plot belongs to any ticked workbook, so this press would write nothing.";

        /// <summary>
        /// The one line above the per template rows: how many workbooks over how many templates,
        /// and how many ticked plots go into none of them.
        ///
        /// **THE PLOTS THAT GO NOWHERE ARE IN THE SAME SENTENCE AS THE ONES THAT GO SOMEWHERE**,
        /// because a count of what will be written with the dropped plots one screen below it is
        /// how twelve plots went unnoticed on the 16:06 press.
        /// </summary>
        public static string InWords(TemplateSplit split)
        {
            if (split == null) throw new ArgumentNullException("split");

            if (split.Shares.Count == 0) return NothingTicked;

            int workbooks = split.Shares.Sum(one => one.Plots.Count);
            if (workbooks == 0) return NoPlotPlaced;

            int writing = split.Shares.Count(one => one.WillWrite);

            string said = "This press would write " + Count(workbooks, "workbook")
                + " from " + Count(writing, "workbook template") + ".";

            if (split.Unplaced.Count > 0)
            {
                said = said + " " + Count(split.Unplaced.Count, "ticked plot")
                    + " named below " + (split.Unplaced.Count == 1 ? "goes" : "go")
                    + " into no workbook at all.";
            }

            return said;
        }

        /// <summary>
        /// One row per ticked template, through `CreateWords.TemplateRow`, which is the wording
        /// the pane has printed since the round that cut it down to a line. **This hands the
        /// rows back in the split's own order and writes none of them.**
        /// </summary>
        public static IReadOnlyList<string> Rows(TemplateSplit split)
        {
            if (split == null) throw new ArgumentNullException("split");

            return split.Shares.Select(CreateWords.TemplateRow).ToList();
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture) + " " + thing
                + (howMany == 1 ? string.Empty : "s");
        }
    }
}
