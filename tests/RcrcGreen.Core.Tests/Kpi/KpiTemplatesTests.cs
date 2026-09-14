using System;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiTemplatesTests
    {
        [Fact]
        public void TheMapHoldsSevenCompleteTemplatesAndEveryCellParses()
        {
            Assert.Equal(7, KpiTemplates.All.Count);
            Assert.Equal(7, KpiTemplates.All.Select(template => template.Name).Distinct().Count());

            foreach (KpiTemplate template in KpiTemplates.All)
            {
                Assert.NotNull(template.CellFor(KpiValue.Component));
                Assert.NotNull(template.CellFor(KpiValue.Location));

                // **The plot reference is not in the map any more.** It went in at C5 on all
                // seven and is found by the REF : label on the sheet now, the same way the date,
                // the person and their position are.
                Assert.Null(template.CellFor(KpiValue.Reference));
                Assert.NotNull(template.CellFor(KpiValue.Shrubs));
                Assert.NotNull(template.CellFor(KpiValue.Lawn));

                // **Every template names an area cell**, STREETS included since the client
                // emptied its H8 and said it comes off the 00 link.
                Assert.NotNull(template.CellFor(KpiValue.Area));
                Assert.False(template.TakesNoArea);

                foreach (MappedCell cell in template.Cells)
                {
                    Assert.Equal(cell.Cell, CellRef.Parse(cell.Cell).ToString());
                }

                Assert.Equal(
                    template.Cells.Count,
                    template.Cells.Select(cell => cell.Value).Distinct().Count());

                Assert.NotNull(template.ExistingTrees);
                Assert.NotNull(template.ProposedTrees);
            }
        }

        [Fact]
        public void TheSevenNamesAndTheirMainSheetsComeInTheBriefsOrder()
        {
            Assert.Equal(
                new[] { "EXISTING PARKS", "FUTURE PARKS", "HEALTHCARE", "MOSQUES", "PARKING", "SCHOOLS", "STREETS" },
                KpiTemplates.All.Select(template => template.Name));
            Assert.Equal(
                new[] { "<Park Name>", "<Park Name>", "<Healthcare>", "<Mosques>", "<Parking Plots>", "<Schools>", "<Streets>" },
                KpiTemplates.All.Select(template => template.MainSheetName));
        }

        [Theory]
        [InlineData("EXISTING PARKS", "D3", "E4", "D8", "F11", "H11")]
        [InlineData("FUTURE PARKS", "D3", "E4", "D8", "F11", "H11")]
        [InlineData("HEALTHCARE", "D3", "E4", "H7", "F10", "H10")]
        [InlineData("MOSQUES", "D3", "E4", "H7", "F10", "H10")]
        [InlineData("PARKING", "D3", "E4", "H7", "F10", "H10")]
        [InlineData("SCHOOLS", "D3", "E4", "H7", "F10", "H10")]
        [InlineData("STREETS", "D3", "E4", "H8", "F11", "H11")]
        public void EachValueGoesIntoTheCellMeasuredOffTheAnnotatedSet(
            string name, string component, string location, string area, string shrubs, string lawn)
        {
            KpiTemplate template = Named(name);

            Assert.Equal(component, template.CellFor(KpiValue.Component).Cell);
            Assert.Equal(location, template.CellFor(KpiValue.Location).Cell);
            Assert.Equal(shrubs, template.CellFor(KpiValue.Shrubs).Cell);
            Assert.Equal(lawn, template.CellFor(KpiValue.Lawn).Cell);

            if (area == null)
            {
                Assert.Null(template.CellFor(KpiValue.Area));
            }
            else
            {
                Assert.Equal(area, template.CellFor(KpiValue.Area).Cell);
            }
        }

        /// <summary>
        /// The map names the two sheets and nothing else about a tree list. It used to carry a
        /// last row per template, 83 on MOSQUES, and the names on the real sheet ran to 101.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS")]
        [InlineData("FUTURE PARKS")]
        [InlineData("HEALTHCARE")]
        [InlineData("MOSQUES")]
        [InlineData("PARKING")]
        [InlineData("SCHOOLS")]
        [InlineData("STREETS")]
        public void TheMapNamesTheTwoTreeListSheetsAndNoRowOnEither(string name)
        {
            KpiTemplate template = Named(name);

            Assert.Equal("Tree List - Existing", template.ExistingTrees.SheetName);
            Assert.Equal("Tree List - Proposed", template.ProposedTrees.SheetName);
        }

        /// <summary>
        /// Nothing on a tree sheet entry but its name, so a row range cannot come back into the
        /// map without this going red.
        /// </summary>
        [Fact]
        public void ATreeSheetEntryCarriesItsNameAndNoNumber()
        {
            Assert.Equal(
                new[] { "SheetName" },
                typeof(TreeSheet).GetProperties().Select(property => property.Name));
        }

        /// <summary>
        /// **E5, G5 and H5 used to be an array here, one for all seven templates.** They are
        /// found by their labels on each template's own sheet now, so no map may name them, and
        /// this is what goes red if one is ever put back.
        /// </summary>
        [Fact]
        public void NoTemplateMapsTheCellsTheLabelsNowChoose()
        {
            foreach (KpiTemplate template in KpiTemplates.All)
            {
                Assert.DoesNotContain(template.Cells, one =>
                    one.Cell == "E5" || one.Cell == "G5" || one.Cell == "H5");
            }
        }

        [Fact]
        public void FillValuesHoldTheSixValuesAndTheTwoSpeciesLists()
        {
            var values = new KpiFillValues(
                "SOFTSCAPE",
                "DM-41",
                "AL NARJIS",
                1200.5,
                300.25,
                150.75,
                new[] { new SpeciesCount("Acacia tortilis", 12) },
                new[] { new SpeciesCount("Phoenix dactylifera", 3) });

            Assert.Equal("SOFTSCAPE", values.Component);
            Assert.Equal("DM-41", values.Reference);
            Assert.Equal("AL NARJIS", values.Location);
            Assert.Equal(1200.5, values.AreaSquareMetres);
            Assert.Equal(300.25, values.ShrubsSquareMetres);
            Assert.Equal(150.75, values.LawnSquareMetres);
            Assert.Equal("Acacia tortilis", values.ExistingTrees.Single().BotanicalName);
            Assert.Equal(12, values.ExistingTrees.Single().Quantity);
            Assert.Equal("Phoenix dactylifera", values.ProposedTrees.Single().BotanicalName);
            Assert.Equal(3, values.ProposedTrees.Single().Quantity);
        }

        [Fact]
        public void NullFillValuesBecomeEmptyStringsAndEmptyLists()
        {
            var values = new KpiFillValues(null, null, null, 0.0, 0.0, 0.0, null, null);

            Assert.Equal(string.Empty, values.Component);
            Assert.Equal(string.Empty, values.Reference);
            Assert.Equal(string.Empty, values.Location);
            Assert.Empty(values.ExistingTrees);
            Assert.Empty(values.ProposedTrees);
        }

        [Fact]
        public void ASpeciesCountRefusesANegativeQuantityAndANullName()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpeciesCount("Acacia tortilis", -1));
            Assert.Throws<ArgumentNullException>(() => new SpeciesCount(null, 1));
        }

        private static KpiTemplate Named(string name)
        {
            return KpiTemplates.All.Single(template => template.Name == name);
        }

        /// <summary>
        /// The brackets are part of the sheet name, not placeholder notation. They were
        /// stripped when the map was first written, so all seven real workbooks came back
        /// unrecognised the first time the pane was pointed at the client's folder. Measured
        /// on the real EXISTING PARKS file: its first sheet is named exactly "&lt;Park Name&gt;".
        /// </summary>
        [Fact]
        public void EveryMainSheetNameIsWrappedInAngleBrackets()
        {
            Assert.Equal(7, KpiTemplates.All.Count);

            foreach (KpiTemplate template in KpiTemplates.All)
            {
                Assert.StartsWith("<", template.MainSheetName, StringComparison.Ordinal);
                Assert.EndsWith(">", template.MainSheetName, StringComparison.Ordinal);
                Assert.True(template.MainSheetName.Length > 2,
                    template.Name + " has a name between its brackets");
            }
        }

        /// <summary>
        /// The four sheet names that do not change, measured off the real workbook.
        /// </summary>
        [Fact]
        public void TheFourFixedSheetNamesAreTheOnesTheRealWorkbookCarries()
        {
            Assert.Equal("Tree List - Existing", KpiTemplates.ExistingTreesSheet);
            Assert.Equal("Tree List - Proposed", KpiTemplates.ProposedTreesSheet);
            Assert.Equal("Criteria", KpiTemplates.CriteriaSheet);
            Assert.Equal("Green Strategy KPI's", KpiTemplates.GreenStrategySheet);
        }
    }
}
