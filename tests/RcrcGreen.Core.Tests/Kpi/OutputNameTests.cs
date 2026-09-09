using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class OutputNameTests
    {
        [Theory]
        [InlineData("plan", "plan.xlsx")]
        [InlineData("plan.xlsx", "plan.xlsx")]
        [InlineData("plan.XLSX", "plan.xlsx")]
        [InlineData("NG05/Landscape", "NG05-Landscape.xlsx")]
        public void FinalCleansTheTypedNameAndPutsTheExtensionBack(string typed, string expected)
        {
            Assert.Equal(expected, OutputName.Final(typed));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(".xlsx")]
        public void ANameThatLeavesNothingBehindComesOutAsUntitled(string typed)
        {
            Assert.Equal("untitled.xlsx", OutputName.Final(typed));
        }

        [Fact]
        public void TheSuggestionIsTheTemplatesOwnFileName()
        {
            Assert.Equal(
                "GRP KPI Checklist - EXISTING PARKS.xlsx",
                OutputName.Suggested("GRP KPI Checklist - EXISTING PARKS.xlsx"));
        }
    }
}
