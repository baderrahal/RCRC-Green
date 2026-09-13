using System;
using System.Collections.Generic;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The two lines a shut sheet definition shows.
    ///
    /// The run that asked for this drew 7 definitions over 35 sub plots: 245 plot rows and 7
    /// full view type checklists in one pane.
    /// </summary>
    public class SheetDefinitionLinesTests
    {
        private const string Family = "AR-PRX-Title_Block_A1";

        private static readonly ViewType Drawings = new ViewType("010", "LIST OF DRAWINGS");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static SheetDefinition Of(string typeName, int perSheet, params ViewType[] views)
        {
            return new SheetDefinition(Family, typeName, views, perSheet);
        }

        [Fact]
        public void TheFirstLineReadsAsOneSentence()
        {
            Assert.Equal(
                "AR-PRX-Title_Block_A1 LOD, 1 view per sheet, (010) LIST OF DRAWINGS.",
                SheetDefinitionLines.What(Of("LOD", 1, Drawings)));

            Assert.Equal(
                "AR-PRX-Title_Block_A1 LOD, 2 views per sheet, (010) LIST OF DRAWINGS, "
                    + "(600) HARDSCAPE SCHEDULE.",
                SheetDefinitionLines.What(Of("LOD", 2, Drawings, Hardscape)));
        }

        [Fact]
        public void ADefinitionShortOfItsBlockOrItsViewsSaysWhichIsMissing()
        {
            Assert.Equal(
                "No title block yet, 1 view per sheet, no views ticked.",
                SheetDefinitionLines.What(new SheetDefinition(string.Empty, string.Empty, null, 1)));

            Assert.Equal(
                "AR-PRX-Title_Block_A1 COVER PAGE, 1 view per sheet, no views ticked.",
                SheetDefinitionLines.What(Of("COVER PAGE", 1)));
        }

        [Fact]
        public void NoDefinitionAtAllSaysNothingRatherThanThrowing()
        {
            Assert.Equal(string.Empty, SheetDefinitionLines.What(null));
        }

        [Fact]
        public void TheSecondLineCountsPerPlotAndInAll()
        {
            Assert.Equal(
                "6 sheets on each of 35 sub plots, 210 in all. Numbers are built.",
                SheetDefinitionLines.Makes(6, 35, 0, 0));

            Assert.Equal(
                "1 sheet on each of 1 sub plot, 1 in all. Numbers are built.",
                SheetDefinitionLines.Makes(1, 1, 0, 0));
        }

        [Fact]
        public void WhatIsStillMissingIsCountedRatherThanListed()
        {
            Assert.Equal(
                "6 sheets on each of 35 sub plots, 210 in all. 1 has no name.",
                SheetDefinitionLines.Makes(6, 35, 1, 0));

            Assert.Equal(
                "6 sheets on each of 35 sub plots, 210 in all. 34 have no name, "
                    + "2 have no number.",
                SheetDefinitionLines.Makes(6, 35, 34, 2));

            Assert.Equal(
                "2 sheets on each of 3 sub plots, 6 in all. 1 has no number.",
                SheetDefinitionLines.Makes(2, 3, 0, 1));
        }

        [Fact]
        public void ADefinitionMakingNothingSaysWhichOfTheTwoReasonsItIs()
        {
            Assert.Equal(
                "No sub plot is ticked in step 1, so this makes nothing yet.",
                SheetDefinitionLines.Makes(6, 0, 0, 0));

            Assert.Equal(
                "Nothing is ticked to go on it, so it makes no sheets.",
                SheetDefinitionLines.Makes(0, 35, 0, 0));
        }

        [Fact]
        public void TheShutChecklistNamesWhatIsTickedAndHowManyWereOffered()
        {
            Assert.Equal(
                "(010) LIST OF DRAWINGS, (600) HARDSCAPE SCHEDULE. 2 of 12 ticked in step 2.",
                SheetDefinitionLines.Ticked(new List<ViewType> { Drawings, Hardscape }, 12));

            Assert.Equal(
                "Nothing ticked, out of 12 in step 2.",
                SheetDefinitionLines.Ticked(new List<ViewType>(), 12));

            Assert.Equal(
                "No view type is ticked in step 2, so there is nothing to put on this.",
                SheetDefinitionLines.Ticked(null, 0));
        }

        [Fact]
        public void TheControlOverTheShutRowsSaysHowManyAreBehindIt()
        {
            Assert.Equal("35 plot rows", SheetDefinitionLines.RowsBehind(35));
            Assert.Equal("1 plot row", SheetDefinitionLines.RowsBehind(1));
            Assert.Equal("No rows yet", SheetDefinitionLines.RowsBehind(0));
        }
    }
}
