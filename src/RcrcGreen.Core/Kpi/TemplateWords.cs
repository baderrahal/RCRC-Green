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
        /// <summary>
        /// Said under the output folder line when it is the templates folder.
        ///
        /// **Before Create is pressed, not after.** The two folders being one is what makes the
        /// name box, which is prefilled with the template's own file name, one press away from
        /// naming the template itself. The per file guard refuses that press, and this is what
        /// stops the user reaching it in the first place. Writing a differently named workbook
        /// into that folder is allowed and this does not refuse it.
        /// </summary>
        /// It carries no path, because the line directly above it is the path.
        public const string OutputIsTheTemplateFolder =
            "THIS IS THE TEMPLATES FOLDER. A filled workbook written here sits beside the "
            + "templates, and a name matching a template's is refused rather than written, "
            + "because the tool never writes to a template. Pick another folder unless you "
            + "meant this.";

        /// <summary>
        /// The forms folder line, under the output folder. **A note and never a refusal**: a
        /// press with none set writes every workbook and no PDF, and says so before it starts
        /// rather than 102 times afterwards.
        /// </summary>
        public static string FormsFolder(string formsFolder)
        {
            if (string.IsNullOrWhiteSpace(formsFolder))
            {
                return "Note. No forms folder is set, so the workbooks are written and no PDF is. "
                    + "Browse to the folder holding the client's three Projects Basic Data forms.";
            }

            return "One PDF per plot beside its workbook, filled from " + formsFolder
                + ". The form is keyed on the plot prefix, the client's own bytes are copied "
                + "whole, and a form whose fields have moved is left alone and named.";
        }

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

            if (template.TakesNoArea)
            {
                lines.Add("  " + CreateWords.TakesNoArea);
            }

            lines.Add("  " + ReferenceOnThisSheet(chosen));
            lines.Add("  " + TypedOnThisPane);

            lines.Add(TreeListLine(template.ExistingTrees));
            lines.Add(TreeListLine(template.ProposedTrees));
            lines.Add("  " + RowsReadOffTheFile);

            return lines;
        }

        /// <summary>
        /// The plot reference used to be one of the mapped cells and read C5 on this line. It is
        /// found by its own label now, so the cell is not known until the template is opened.
        /// </summary>
        public static string ReferenceOnThisSheet(ChosenParameters chosen)
        {
            return "The plot reference is "
                + KpiTemplates.SourceOf(KpiValue.Reference, chosen)
                + ", and it goes into the cell the " + LabelledPlaces.ReferenceLabel
                + " label chooses on the template's own sheet when Create is pressed.";
        }

        /// <summary>
        /// **This line used to name three cells, E5, G5 and H5.** It cannot name a cell any more:
        /// the three are found by their labels on the template's own sheet when Create is
        /// pressed, so which cell each lands in is not known until the file is opened, and the
        /// report says what it found. A line about what the tool does is checked against what it
        /// does, which is a rule this project has paid for twice.
        /// </summary>
        public const string TypedOnThisPane =
            "The date, the person and their position are typed by the team on this pane and "
            + "copied through, from no model, into the cells the Date: and Prepared By: labels "
            + "choose on the template's own sheet when Create is pressed.";

        /// <summary>
        /// **This line used to print a row range off the map, B4 to B83, and the map was wrong.**
        /// The names on the MOSQUES existing list run to row 101 and its total reaches row 92.
        /// Nothing here holds a number now. The rows and the total are read off the file when
        /// Create is pressed and the report says what was found.
        /// </summary>
        public const string RowsReadOffTheFile =
            "Which rows hold a name and which rows the total reaches are read off the file when "
            + "Create is pressed, never off a range in this tool, and the report says what it found.";

        private static string TreeListLine(TreeSheet sheet)
        {
            return sheet.SheetName + ": one quantity into column " + KpiTemplates.QuantityColumn
                + " per botanical name in column " + KpiTemplates.BotanicalColumn
                + ", over every row that names one";
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

        /// <summary>
        /// How often the folder's workbooks were opened against how often the pane drew the
        /// list, since the folder was listed. Seven opens over 158 redraws is the fix, 1,106
        /// would be the fault back.
        /// </summary>
        public static string Opened(int workbooks, int opened, int drawn)
        {
            return workbooks + (workbooks == 1 ? " workbook" : " workbooks") + " in the folder, opened "
                + opened + (opened == 1 ? " time" : " times") + " over " + drawn + (drawn == 1 ? " redraw" : " redraws")
                + " since the folder was listed.";
        }
    }
}
