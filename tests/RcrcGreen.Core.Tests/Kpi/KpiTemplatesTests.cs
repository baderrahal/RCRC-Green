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
                Assert.NotNull(template.CellFor(KpiValue.Reference));
                Assert.NotNull(template.CellFor(KpiValue.Location));
                Assert.NotNull(template.CellFor(KpiValue.Shrubs));
                Assert.NotNull(template.CellFor(KpiValue.Lawn));

                if (template.Name == "STREETS")
                {
                    Assert.Null(template.CellFor(KpiValue.Area));
                    Assert.True(template.AreaIsTypedByHand);
                }
                else
                {
                    Assert.NotNull(template.CellFor(KpiValue.Area));
                    Assert.False(template.AreaIsTypedByHand);
                }

                foreach (MappedCell cell in template.Cells)
                {
                    Assert.Equal(cell.Cell, CellRef.Parse(cell.Cell).ToString());
                }

                Assert.Equal(
                    template.Cells.Count,
                    template.Cells.Select(cell => cell.Value).Distinct().Count());

                Assert.NotNull(template.ExistingTrees);
                Assert.NotNull(template.ProposedTrees);
                Assert.Equal(4, template.ExistingTrees.FirstRow);
                Assert.Equal(4, template.ProposedTrees.FirstRow);
            }
        }

        [Fact]
        public void TheSevenNamesAndTheirMainSheetsComeInTheBriefsOrder()
        {
            Assert.Equal(
                new[] { "EXISTING PARKS", "FUTURE PARKS", "HEALTHCARE", "MOSQUES", "PARKING", "SCHOOLS", "STREETS" },
                KpiTemplates.All.Select(template => template.Name));
            Assert.Equal(
                new[] { "Park Name", "Park Name", "Healthcare", "Mosques", "Parking Plots", "Schools", "Streets" },
                KpiTemplates.All.Select(template => template.MainSheetName));
        }

        [Theory]
        [InlineData("EXISTING PARKS", "D3", "C5", "E4", "D8", "F11", "H11")]
        [InlineData("FUTURE PARKS", "D3", "C5", "E4", "D8", "F11", "H11")]
        [InlineData("HEALTHCARE", "D3", "C5", "E4", "H7", "F10", "H10")]
        [InlineData("MOSQUES", "D3", "C5", "E4", "H7", "F10", "H10")]
        [InlineData("PARKING", "D3", "C5", "E4", "H7", "F10", "H10")]
        [InlineData("SCHOOLS", "D3", "C5", "E4", "H7", "F10", "H10")]
        [InlineData("STREETS", "D3", "C5", "E4", null, "F11", "H11")]
        public void EachValueGoesIntoTheCellMeasuredOffTheAnnotatedSet(
            string name, string component, string reference, string location, string area, string shrubs, string lawn)
        {
            KpiTemplate template = Named(name);

            Assert.Equal(component, template.CellFor(KpiValue.Component).Cell);
            Assert.Equal(reference, template.CellFor(KpiValue.Reference).Cell);
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

        [Theory]
        [InlineData("EXISTING PARKS", 92, 84)]
        [InlineData("FUTURE PARKS", 92, 84)]
        [InlineData("HEALTHCARE", 83, 83)]
        [InlineData("MOSQUES", 83, 83)]
        [InlineData("PARKING", 83, 83)]
        [InlineData("SCHOOLS", 83, 83)]
        [InlineData("STREETS", 83, 83)]
        public void TheTreeListsRunFromRowFourToTheMeasuredLastRow(string name, int existingLast, int proposedLast)
        {
            KpiTemplate template = Named(name);

            Assert.Equal("Tree List - Existing", template.ExistingTrees.SheetName);
            Assert.Equal("Tree List - Proposed", template.ProposedTrees.SheetName);
            Assert.Equal(4, template.ExistingTrees.FirstRow);
            Assert.Equal(4, template.ProposedTrees.FirstRow);
            Assert.Equal(existingLast, template.ExistingTrees.LastRow);
            Assert.Equal(proposedLast, template.ProposedTrees.LastRow);
        }

        [Fact]
        public void TreeRowsPrintTheQuantityColumnAndBothEnds()
        {
            Assert.Equal("B4 to B92", KpiTemplates.ExistingParks.ExistingTrees.InWords);
        }

        [Fact]
        public void TheTeamTypesTheDateThePersonAndThePosition()
        {
            Assert.Equal(new[] { "E5", "G5", "H5" }, KpiTemplates.TypedByTheTeam);
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
    }
}
