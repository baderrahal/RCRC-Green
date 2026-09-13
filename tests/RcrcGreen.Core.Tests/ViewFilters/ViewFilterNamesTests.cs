using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The name rules the run and the scan share: the target name is the prefix, one space,
    /// the plot code, and matching forgives case and edge spaces the way the ported host
    /// did.
    /// </summary>
    public class ViewFilterNamesTests
    {
        [Fact]
        public void TheTargetNameIsThePrefixASpaceAndThePlotId()
        {
            Assert.Equal(
                "(200-260) Presentation DM-41",
                ViewFilterNames.TargetFilterName("(200-260) Presentation", "DM-41"));
        }

        [Fact]
        public void AnEmptyPlotCodeLeavesNoTrailingSpace()
        {
            Assert.Equal("(215) Borders Edging", ViewFilterNames.TargetFilterName("(215) Borders Edging", ""));
        }

        [Theory]
        [InlineData("(200-260) Presentation DM-41")]
        [InlineData("  (200-260) PRESENTATION dm-41  ")]
        public void AnExactMatchForgivesCaseAndEdgeSpaces(string filterName)
        {
            Assert.True(ViewFilterNames.IsExactMatch(filterName, "(200-260) Presentation DM-41"));
        }

        [Fact]
        public void APrefixAndPlotMatchTakesAnyMiddle()
        {
            Assert.True(ViewFilterNames.IsPrefixAndPlotMatch(
                "(200-260) Presentation old DM-41", "(200-260) Presentation", "DM-41"));
            Assert.False(ViewFilterNames.IsPrefixAndPlotMatch(
                "(215) Borders Edging DM-41", "(200-260) Presentation", "DM-41"));
        }

        [Fact]
        public void TheExemplarPlotCodeIsTheTailPastThePrefix()
        {
            Assert.Equal(
                "DM-11",
                ViewFilterNames.ExemplarPlotCode("(200-260) Presentation DM-11", "(200-260) Presentation"));
        }

        /// <summary>
        /// The second edit's guard case. A filter named exactly like its prefix has no tail,
        /// and the host's Substring threw into the catch instead of saying so.
        /// </summary>
        [Theory]
        [InlineData("(200-260) Presentation")]
        [InlineData("(200-260)")]
        [InlineData("")]
        [InlineData(null)]
        public void AnExemplarNoLongerThanItsPrefixHasAnEmptyTail(string exemplarName)
        {
            Assert.Equal("", ViewFilterNames.ExemplarPlotCode(exemplarName, "(200-260) Presentation"));
        }
    }
}
