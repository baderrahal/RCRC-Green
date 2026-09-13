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

        /// <summary>
        /// **THE SHAPE THE WORKING RUNS WROTE.** The several templates round named each row
        /// after the template alone, so the boxes read MOSQUES and a run would have written
        /// MOSQUES.xlsx, which nobody recognises in a folder three months later. These two are
        /// what the runs that worked really put on disk.
        /// </summary>
        [Fact]
        public void TheSuggestionIsTheTemplateFilesOwnNameAndKeepsItsShape()
        {
            Assert.Equal(
                "GRP-KPI-Checklist-DD-MOSQUES.xlsx",
                OutputName.Suggested("GRP_KPI_Checklist_DD_MOSQUES.xlsx"));

            Assert.Equal(
                "GRP-KPI-Checklist-DD-STREETS.xlsx",
                OutputName.Suggested("GRP_KPI_Checklist_DD_STREETS.xlsx"));

            // A file already named with dashes comes through unchanged, and one named with
            // spaces keeps them, because the cleaning takes out what Windows will not have and
            // nothing else.
            Assert.Equal(
                "GRP-KPI-Checklist-DD-MOSQUES.xlsx",
                OutputName.Suggested("GRP-KPI-Checklist-DD-MOSQUES.xlsx"));
            Assert.Equal(
                "GRP KPI Checklist MOSQUES.xlsx",
                OutputName.Suggested("GRP KPI Checklist MOSQUES.xlsx"));
        }
    }
}
