using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class BuildStampTests
    {
        [Fact]
        public void ThePaneHeaderReadsBuildThenTheStamp()
        {
            Assert.Equal("Build 394f914 2026-10-28 14:05", KpiPaneWords.Build("394f914 2026-10-28 14:05"));
        }

        [Fact]
        public void ABuildWithNoStampSaysSoRatherThanShowingAVersionNumber()
        {
            Assert.Equal("Build not stamped", KpiPaneWords.Build(null));
            Assert.Equal("Build not stamped", KpiPaneWords.Build("  "));
        }

        [Fact]
        public void AReportOpensOnTheStampLine()
        {
            Assert.Equal(
                "Build: 394f914 2026-10-28 14:05 changed\r\nRCRC Green KPI checklist\r\n",
                BuildStamp.Stamped(" 394f914 2026-10-28 14:05 changed ", "RCRC Green KPI checklist\r\n"));
        }

        [Fact]
        public void AReportFromAnUnstampedBuildSaysSo()
        {
            Assert.Equal("Build: not stamped\r\n", BuildStamp.Stamped(null, null));
        }
    }
}
