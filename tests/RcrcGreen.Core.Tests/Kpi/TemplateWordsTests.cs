using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class TemplateWordsTests
    {
        /// <summary>
        /// The three parameters the pane's pickers hold on the first real model. PRX_Component
        /// with the capital C only, because PRX_COMPONENT is in no model anywhere.
        /// </summary>
        private static readonly ChosenParameters Picked =
            new ChosenParameters("PRX_Component", "PRX_Plot_UID2", "Neighborhood Name");

        /// <summary>
        /// This block used to print the workbook's note as if it were the tool's behaviour, and
        /// every part of the first two lines was wrong: PRX_COMPONENT is in no model, the value
        /// is PRX_Component on the sheet, and PRX_Plot_UID2 holds a value on none of the 1233
        /// title block instances that carry it.
        /// </summary>
        [Fact]
        public void TheExistingParksBlockNamesEveryCellAndWhereItsValueComesFrom()
        {
            Assert.Equal(
                new[]
                {
                    "Main sheet <Park Name>:",
                    "  D3  PRX_Component, read off the plot's first sheet",
                    "  C5  PRX_Plot_UID2, read off the plot's first sheet",
                    "  E4  Neighborhood Name, read off Project Information",
                    "  D8  PRX_Intervention Area, totalled off the chosen filled regions in the 00 link",
                    "  F11  SHRUBS & GROUND COVER TOTAL AREA from the shrubs and lawn schedule",
                    "  H11  LAWN (GRASS) TOTAL AREA from the shrubs and lawn schedule",
                    "  E5, G5, H5  the date, the person and their position, typed by the team, never written",
                    "Tree List - Existing: quantities into B4 to B92, one per botanical name in column D",
                    "Tree List - Proposed: quantities into B4 to B84, one per botanical name in column D"
                },
                TemplateWords.WouldFill(KpiTemplates.ExistingParks, Picked));
        }

        /// <summary>
        /// Before a model has been read nothing has been picked, so the line points at the
        /// picker rather than naming a parameter nobody chose.
        /// </summary>
        [Fact]
        public void WithNothingPickedTheLinesPointAtThePickers()
        {
            IReadOnlyList<string> lines = TemplateWords.WouldFill(
                KpiTemplates.ExistingParks, ChosenParameters.NonePicked);

            Assert.Contains("  D3  the parameter picked under Component, read off the plot's first sheet", lines);
            Assert.Contains("  C5  the parameter picked under Reference, read off the plot's first sheet", lines);
            Assert.Contains("  E4  the parameter picked under Location, read off Project Information", lines);
        }

        /// <summary>
        /// The name a workbook note asks for is never printed as though the tool reads it.
        /// PRX_COMPONENT exists in no model and the title block is not where either of the two
        /// sheet values is read.
        /// </summary>
        [Fact]
        public void NoLineNamesPrxComponentOrTheTitleBlock()
        {
            foreach (KpiTemplate template in KpiTemplates.All)
            {
                foreach (string line in TemplateWords.WouldFill(template, Picked))
                {
                    Assert.DoesNotContain("PRX_COMPONENT", line);
                    Assert.DoesNotContain("title block", line);
                }
            }
        }

        [Fact]
        public void TheStreetsBlockNamesNoAreaCellAndSaysWhy()
        {
            IReadOnlyList<string> lines = TemplateWords.WouldFill(KpiTemplates.Streets, Picked);

            Assert.Contains(
                "  No area cell. The road width and the total length are typed by hand and the "
                + "sheet works the area out. Those cells are left alone.",
                lines);

            // H7 and D8 are the two area cells anywhere in the map, so neither may appear.
            Assert.All(lines, line => Assert.DoesNotContain("H7", line));
            Assert.All(lines, line => Assert.DoesNotContain("D8", line));
        }

        [Fact]
        public void TheOutputLineNamesTheFolderAndSaysTheOverwriteOutLoud()
        {
            Assert.Equal(
                "Written to C:\\models. A file already there under this name is overwritten, "
                + "with no confirmation and no second copy.",
                TemplateWords.Output("C:\\models"));
        }

        [Fact]
        public void WithNoModelFolderTheOutputLineSaysBesideTheModel()
        {
            Assert.Equal(
                "Written to beside the open Revit model. A file already there under this name "
                + "is overwritten, with no confirmation and no second copy.",
                TemplateWords.Output(""));
        }

        [Fact]
        public void ListedCountsTheWorkbooksAndSaysWorkbookInTheSingular()
        {
            Assert.Equal("3 workbooks in the folder, 2 recognised.", TemplateWords.Listed(3, 2));
            Assert.Equal("1 workbook in the folder, 0 recognised.", TemplateWords.Listed(1, 0));
        }

        [Fact]
        public void TheFixedLinesSayWhatIsMissingAndWhatToDoAboutIt()
        {
            Assert.Equal(
                "No template folder is set. Press Browse and point at the folder holding the "
                + "GRP KPI Checklist templates.",
                TemplateWords.NoFolder);
            Assert.Equal("No .xlsx in this folder.", TemplateWords.EmptyFolder);
            Assert.Equal(
                "The open model has never been saved, so there is no folder to write beside. "
                + "Save the model first.",
                TemplateWords.NoModelPath);
        }
    }
}
