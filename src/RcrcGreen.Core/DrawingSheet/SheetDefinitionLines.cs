using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// What a sheet definition says when it is shut.
    ///
    /// Step 4 drew every definition open: a title block dropdown, a views per sheet radio, the
    /// whole view type checklist about three hundred pixels tall, and a table with a row per
    /// ticked sub plot. Seven definitions over 35 sub plots is 245 rows and seven checklists,
    /// which is the whole pane and then some. The user's words were that the tool turns into a
    /// nightmare when working many plots.
    ///
    /// So a definition that is not being edited is two lines. **They say what it is and what it
    /// makes**, which are the two questions somebody scrolling past seven of them is asking,
    /// and opening one shows everything it showed before.
    /// </summary>
    public static class SheetDefinitionLines
    {
        /// <summary>
        /// The first line: the title block, how many views go on a sheet, and which view types
        /// are ticked.
        /// </summary>
        public static string What(SheetDefinition definition)
        {
            if (definition == null) return string.Empty;

            string block = definition.TitleBlock.Length == 0
                ? "No title block yet"
                : definition.TitleBlock;

            string each = definition.ViewsPerSheet == 1
                ? "1 view per sheet"
                : definition.ViewsPerSheet + " views per sheet";

            string views = definition.Views.Count == 0
                ? "no views ticked"
                : string.Join(", ", definition.Views.Select(one => one.ToString()).ToArray());

            return block + ", " + each + ", " + views + ".";
        }

        /// <summary>
        /// The second line: how many sheets it makes, and what is still missing across every
        /// row behind it.
        ///
        /// The count is per plot and in all, because on 35 sub plots those are very different
        /// numbers and only one of them tells somebody what the run will do.
        /// </summary>
        public static string Makes(
            int sheetsPerPlot, int plots, int rowsWithoutAName, int rowsWithoutANumber)
        {
            if (plots <= 0)
            {
                return "No sub plot is ticked in step 1, so this makes nothing yet.";
            }

            if (sheetsPerPlot <= 0)
            {
                return "Nothing is ticked to go on it, so it makes no sheets.";
            }

            int all = sheetsPerPlot * plots;

            string made = (sheetsPerPlot == 1 ? "1 sheet" : sheetsPerPlot + " sheets")
                + " on each of "
                + (plots == 1 ? "1 sub plot" : plots + " sub plots")
                + ", " + all + " in all.";

            var missing = new List<string>();
            if (rowsWithoutAName > 0)
            {
                missing.Add(rowsWithoutAName == 1
                    ? "1 has no name"
                    : rowsWithoutAName + " have no name");
            }

            if (rowsWithoutANumber > 0)
            {
                missing.Add(rowsWithoutANumber == 1
                    ? "1 has no number"
                    : rowsWithoutANumber + " have no number");
            }

            return missing.Count == 0
                ? made + " Numbers are built."
                : made + " " + string.Join(", ", missing.ToArray()) + ".";
        }

        /// <summary>
        /// What the shut view type checklist says in its place. The list inside a definition is
        /// the same twelve lines in all seven of them, and only the ticks differ.
        /// </summary>
        public static string Ticked(IReadOnlyList<ViewType> views, int offered)
        {
            int ticked = views == null ? 0 : views.Count;

            if (ticked == 0)
            {
                return offered == 0
                    ? "No view type is ticked in step 2, so there is nothing to put on this."
                    : "Nothing ticked, out of " + offered + " in step 2.";
            }

            return string.Join(", ", views.Select(one => one.ToString()).ToArray())
                + ". " + ticked + " of " + offered + " ticked in step 2.";
        }

        /// <summary>
        /// What the control over the shut per-plot rows says, so somebody knows how much is
        /// behind it before opening it.
        /// </summary>
        public static string RowsBehind(int rows)
        {
            if (rows <= 0) return "No rows yet";

            return rows == 1 ? "1 plot row" : rows + " plot rows";
        }
    }
}
