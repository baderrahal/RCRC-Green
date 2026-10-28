using System.Collections.Generic;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class FeaturesTests
    {
        [Fact]
        public void NoFileShowsAllThreeSoBadersInstallDoesNotChange()
        {
            Features read = Features.Of(null);
            Assert.Equal(new[] { "Drawing Sheet", "KPI", "View Filters" }, read.Shown);
            Assert.Empty(read.JournalLines());
        }

        [Fact]
        public void AnEmptyFileShowsAllThreeAndSaysNothing()
        {
            Features read = Features.Of(new[] { "", "   " });
            Assert.Equal(new[] { "Drawing Sheet", "KPI", "View Filters" }, read.Shown);
            Assert.Empty(read.JournalLines());
        }

        [Fact]
        public void TheTeamKitsFileShowsKpiAlone()
        {
            Features read = Features.Of(new[] { "KPI" });
            Assert.Equal(new[] { "KPI" }, read.Shown);
            Assert.True(read.Shows("KPI"));
            Assert.False(read.Shows("Drawing Sheet"));
            Assert.False(read.Shows("View Filters"));
        }

        [Fact]
        public void ALineIsMatchedWholeWithoutCaseAndEdgeWhitespaceOff()
        {
            Features read = Features.Of(new[] { "  kpi ", "view filters" });
            Assert.Equal(new[] { "KPI", "View Filters" }, read.Shown);
            Assert.Empty(read.Unknown);
        }

        [Fact]
        public void TheRibbonOrderHoldsWhateverOrderTheFileUses()
        {
            Features read = Features.Of(new[] { "View Filters", "Drawing Sheet" });
            Assert.Equal(new[] { "Drawing Sheet", "View Filters" }, read.Shown);
        }

        [Fact]
        public void AnUnknownNameIsIgnoredAndJournalled()
        {
            Features read = Features.Of(new[] { "KPI", "Coordination Layout" });
            Assert.Equal(new[] { "KPI" }, read.Shown);
            Assert.Equal(new[] { "Coordination Layout" }, read.Unknown);
            Assert.Equal(
                new List<string>
                {
                    "RCRC Green: features.txt names 'Coordination Layout', which is not a tool. "
                    + "It is ignored. The tools are Drawing Sheet, KPI, View Filters."
                },
                read.JournalLines());
        }

        [Fact]
        public void AFileNamingNoToolItKnowsShowsAllThreeAndSaysWhy()
        {
            Features read = Features.Of(new[] { "KPl" });
            Assert.True(read.NoneNamed);
            Assert.Equal(new[] { "Drawing Sheet", "KPI", "View Filters" }, read.Shown);
            Assert.Equal(2, read.JournalLines().Count);
            Assert.Equal("RCRC Green: features.txt names no tool, so all three are shown.", read.JournalLines()[1]);
        }

        [Fact]
        public void APartialNameIsNotATool()
        {
            Features read = Features.Of(new[] { "KPI Checklist", "Drawing" });
            Assert.True(read.NoneNamed);
            Assert.Equal(new[] { "KPI Checklist", "Drawing" }, read.Unknown);
        }
    }
}
