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

            int workbooks = Workbooks(split);
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

        /// <summary>
        /// Step 4's big number: one workbook per plot the split places, the same sum
        /// <see cref="InWords"/> says in a sentence.
        /// </summary>
        public static int Workbooks(TemplateSplit split)
        {
            if (split == null) throw new ArgumentNullException("split");
            return split.Shares.Sum(one => one.Plots.Count);
        }

        /// <summary>
        /// **WHAT THE PRESS WOULD REALLY WRITE**, step 4's big number and the Create button's
        /// count: the placed plots, less every plot whose path is refused and every plot whose
        /// folder another ticked plot shares. Both are read off the filings the step 4 boxes
        /// read, through <see cref="SharedUid2.Of"/> and the path itself, so the number and the
        /// boxes beside it cannot disagree. FM-08 is the one MOSQUES files nowhere.
        /// </summary>
        public static int Workbooks(TemplateSplit split, IReadOnlyList<PlotFiling> filings)
        {
            if (split == null) throw new ArgumentNullException("split");

            IReadOnlyList<SharedUid2Group> sharing = SharedUid2.Of(filings ?? new List<PlotFiling>());

            return split.Shares
                .SelectMany(one => one.Plots)
                .Count(plotId =>
                {
                    PlotFiling filing = PlotFilings.For(filings, plotId);
                    return filing != null && filing.Where.Ok && !SharedUid2.Stops(sharing, plotId);
                });
        }

        /// <summary>
        /// Step 4's table, one row per ticked asset type that has plots, in the split's order.
        /// A ticked type with none is a warning box instead, because a row reading 0 beside a
        /// blank range says less than the sentence about why.
        /// </summary>
        public static IReadOnlyList<WillWriteRow> Table(TemplateSplit split)
        {
            if (split == null) throw new ArgumentNullException("split");

            return split.Shares
                .Where(one => one.WillWrite)
                .Select(one => new WillWriteRow(
                    AssetTypeNames.Display(one.Template), one.Plots.Count, PlotSpan.Of(one.Plots)))
                .ToList();
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture) + " " + thing
                + (howMany == 1 ? string.Empty : "s");
        }
    }

    public sealed class WillWriteRow
    {
        public WillWriteRow(string assetType, int plots, string range)
        {
            AssetType = assetType ?? string.Empty;
            Plots = plots;
            Range = range ?? string.Empty;
        }

        public string AssetType { get; }

        public int Plots { get; }

        public string Range { get; }
    }
}
