using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class TemplateWordsTests
    {
        [Fact]
        public void TheExistingParksBlockNamesEveryCellAndWhereItsValueComesFrom()
        {
            Assert.Equal(
                new[]
                {
                    "Main sheet <Park Name>:",
                    "  D3  PRX_COMPONENT, read off the title block",
                    "  C5  PRX_Plot_UID2, read off the title block",
                    "  E4  the neighbourhood name from Project Information",
                    "  D8  PRX_Intervention Area, totalled off the 00 link's filled regions",
                    "  F11  SHRUBS & GROUND COVER TOTAL AREA from the shrubs and lawn schedule",
                    "  H11  LAWN (GRASS) TOTAL AREA from the shrubs and lawn schedule",
                    "  E5, G5, H5  the date, the person and their position, typed by the team, never written",
                    "Tree List - Existing: quantities into B4 to B92, one per botanical name in column D",
                    "Tree List - Proposed: quantities into B4 to B84, one per botanical name in column D"
                },
                TemplateWords.WouldFill(KpiTemplates.ExistingParks));
        }

        [Fact]
        public void TheStreetsBlockNamesNoAreaCellAndSaysWhy()
        {
            IReadOnlyList<string> lines = TemplateWords.WouldFill(KpiTemplates.Streets);

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
