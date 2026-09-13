using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The first edit to the ported run. The host fell back to the whole view name when
    /// nothing parsed and created a filter named (215) Borders Edging General Arrangement
    /// Layout in a real model, so a name that does not start with a plot id is a skip.
    /// </summary>
    public class ViewFilterPlotCodeTests
    {
        [Theory]
        [InlineData("DM-41-(010) Location Key Plan", "DM-41")]
        [InlineData("PF-12-(200) General Arrangement Layout", "PF-12")]
        [InlineData("DM-41", "DM-41")]
        public void ANameStartingWithAPlotIdGivesThePlotCode(string viewName, string plotCode)
        {
            Assert.True(ViewFilterPlotCode.TryFromViewName(viewName, out string found));
            Assert.Equal(plotCode, found);
        }

        [Theory]
        [InlineData("General Arrangement Layout")]
        [InlineData("Working View-(010) Section")]
        [InlineData("D-41-(010) Location Key Plan")]
        [InlineData("DM-(010) Location Key Plan")]
        [InlineData("")]
        [InlineData(null)]
        public void ANameWithoutAPlotIdAtTheFrontIsASkip(string viewName)
        {
            Assert.False(ViewFilterPlotCode.TryFromViewName(viewName, out string found));
            Assert.Equal("", found);
        }

        /// <summary>
        /// A bare name with no "-(" takes the pattern's own match, so trailing words after
        /// the digits do not travel into the plot code.
        /// </summary>
        [Fact]
        public void ABareNameTakesOnlyTheMatchItself()
        {
            Assert.True(ViewFilterPlotCode.TryFromViewName("DM-41 working copy", out string found));
            Assert.Equal("DM-41", found);
        }

        /// <summary>
        /// The pattern is the round's own, [A-Za-z]{2}-digits, so lowercase passes here even
        /// though the Shared PlotId calls lowercase invalid. That looseness is recorded in
        /// the log as a question for the team, and this test pins today's behaviour.
        /// </summary>
        [Fact]
        public void LowercaseLettersPassThePortedPattern()
        {
            Assert.True(ViewFilterPlotCode.StartsWithAPlotId("dm-41"));
        }

        [Theory]
        [InlineData("DM-41", true)]
        [InlineData("DM-41 East", true)]
        [InlineData("41-DM", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void TheCheckIsAnchoredAtTheStartOnly(string text, bool starts)
        {
            Assert.Equal(starts, ViewFilterPlotCode.StartsWithAPlotId(text));
        }
    }
}
