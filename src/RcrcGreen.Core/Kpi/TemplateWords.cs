using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every line the template block of the KPI pane shows, so the pane formats nothing of
    /// its own. Same rule as KpiPaneWords for the scan block, which is left as it is.
    /// </summary>
    public static class TemplateWords
    {
        public const string NoFolder =
            "No template folder is set. Press Browse and point at the folder holding the GRP "
            + "KPI Checklist templates.";

        public const string EmptyFolder = "No .xlsx in this folder.";

        /// <summary>
        /// **This replaced the never saved line.** The workbook used to be written beside the
        /// Revit model, so a detached model, which is what the team works on, could not be used
        /// at all. The folder is browsed for and remembered the way the template folder is, and
        /// whether the model has been saved is asked nowhere.
        /// </summary>
        public const string NoOutputFolder =
            "No output folder is set. Press Browse beside Output folder and point at where the "
            + "filled workbooks should be written.";

        /// <summary>
        /// Said once, under the name box, so nobody is surprised by the overwrite. The team
        /// asked for no confirmation and no second copy.
        ///
        /// The folder is the browsed one. It takes no fallback wording for an empty folder,
        /// because with none set NoOutputFolder is what shows and the name box has nowhere to
        /// write to yet.
        /// </summary>
        public static string Output(string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(outputFolder)) return NoOutputFolder;

            return "Written to " + outputFolder + ". A file already there under this name is "
                + "overwritten, with no confirmation and no second copy.";
        }

        /// <summary>
        /// What the pick would fill, line by line. This is the whole point of the round: a
        /// person checks the map against the annotated workbook before anything is written.
        /// </summary>
        public static IReadOnlyList<string> WouldFill(KpiTemplate template, ChosenParameters chosen)
        {
            if (template == null) throw new ArgumentNullException("template");

            var lines = new List<string>();
            lines.Add("Main sheet " + template.MainSheetName + ":");

            foreach (MappedCell cell in template.Cells)
            {
                lines.Add("  " + cell.Cell + "  " + KpiTemplates.SourceOf(cell.Value, chosen));
            }

            if (template.AreaIsTypedByHand)
            {
                lines.Add("  No area cell. The road width and the total length are typed by "
                    + "hand and the sheet works the area out. Those cells are left alone.");
            }

            lines.Add("  " + string.Join(", ", KpiTemplates.TypedByTheTeam)
                + "  the date, the person and their position, typed by the team, never written");

            lines.Add(template.ExistingTrees.SheetName + ": quantities into "
                + template.ExistingTrees.InWords + ", one per botanical name in column "
                + KpiTemplates.BotanicalColumn);
            lines.Add(template.ProposedTrees.SheetName + ": quantities into "
                + template.ProposedTrees.InWords + ", one per botanical name in column "
                + KpiTemplates.BotanicalColumn);

            return lines;
        }

        /// <summary>
        /// The line under the list when a file needs the user to pick between the two park
        /// templates.
        /// </summary>
        public static string PickBetween(RecognisedWorkbook workbook)
        {
            if (workbook == null) throw new ArgumentNullException("workbook");

            return workbook.FileName + " is named for neither park template or for both, so "
                + "the file name settles nothing. Pick which it is.";
        }

        public static string Listed(int workbooks, int matched)
        {
            return workbooks + (workbooks == 1 ? " workbook" : " workbooks") + " in the folder, "
                + matched + " recognised.";
        }
    }
}
