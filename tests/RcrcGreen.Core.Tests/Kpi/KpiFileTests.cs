using System;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiFileTests
    {
        [Fact]
        public void TheNameCarriesTheKpiPrefixTheTitleAndTheMinute()
        {
            Assert.Equal(
                "RCRC-Green-KPI_NG05_2026-09-09_1405.txt",
                KpiFile.NameFor("NG05", new DateTime(2026, 9, 9, 14, 5, 0)));
        }

        [Fact]
        public void TheUnderscoresInTheRealTitleBecomeDashesLikeEveryOtherReport()
        {
            Assert.Equal(
                "RCRC-Green-KPI_RCRC-NG05-NU-MAIN-RVT24-SHEETS-detached_2026-09-09_0900.txt",
                KpiFile.NameFor("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached", new DateTime(2026, 9, 9, 9, 0, 0)));
        }

        [Fact]
        public void ATitleWithNothingUsableInItStillNamesAFile()
        {
            Assert.Equal(
                "RCRC-Green-KPI_untitled_2026-09-09_1405.txt",
                KpiFile.NameFor(string.Empty, new DateTime(2026, 9, 9, 14, 5, 0)));
        }
    }
}
