using System;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class KpiPaneWordsTests
    {
        private const string Where = "Written to C:\\Users\\bader\\Desktop\\RCRC-Green-KPI_NG05_2026-09-09_1405.txt";

        [Fact]
        public void ReadAtNamesTheTimeTheCountAndTheSeconds()
        {
            Assert.Equal(
                "Read at 14:05:12, 96934 elements in 1.4 seconds.",
                KpiPaneWords.ReadAt(new DateTime(2026, 9, 9, 14, 5, 12), 96934, 1.4));
        }

        [Fact]
        public void ReadAtPrintsWholeSecondsWithOneDecimal()
        {
            Assert.Equal(
                "Read at 09:00:00, 12 elements in 3.0 seconds.",
                KpiPaneWords.ReadAt(new DateTime(2026, 9, 9, 9, 0, 0), 12, 3.0));
        }

        [Fact]
        public void ModelNamedFallsBackWhenThereIsNoTitle()
        {
            Assert.Equal("No model open", KpiPaneWords.ModelNamed(null));
            Assert.Equal("No model open", KpiPaneWords.ModelNamed(string.Empty));
            Assert.Equal("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached", KpiPaneWords.ModelNamed("RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached"));
        }

        [Fact]
        public void HeadlineCountsEverythingReadAndAppendsWhereTheFileIs()
        {
            Assert.Equal(
                "1385 sheets, 1383 title blocks, 1 links with 1 loaded, 7 schedules, 412 filled regions read. "
                + "7 of 9 questions have something in the file. " + Where,
                KpiPaneWords.Headline(KpiFixture.RealShaped(), Where));
        }

        [Fact]
        public void HeadlineOfAnEmptyScanIsAllZeros()
        {
            Assert.Equal(
                "0 sheets, 0 title blocks, 0 links with 0 loaded, 0 schedules, 0 filled regions read. "
                + "0 of 9 questions have something in the file. " + Where,
                KpiPaneWords.Headline(KpiFixture.Build(), Where));
        }

        [Fact]
        public void HeadlineNamesOneSkippedRead()
        {
            KpiScan scan = KpiFixture.Build(skipped: new[] { "Links: the link refused to open" });

            Assert.Equal(
                "0 sheets, 0 title blocks, 0 links with 0 loaded, 0 schedules, 0 filled regions read. "
                + "0 of 9 questions have something in the file, and 1 read did not happen, named at the top of it. "
                + Where,
                KpiPaneWords.Headline(scan, Where));
        }

        [Fact]
        public void HeadlineNamesTwoSkippedReadsInThePlural()
        {
            KpiScan scan = KpiFixture.Build(skipped: new[]
            {
                "Links: the link refused to open",
                "Schedules: the rows were not read"
            });

            Assert.Equal(
                "0 sheets, 0 title blocks, 0 links with 0 loaded, 0 schedules, 0 filled regions read. "
                + "0 of 9 questions have something in the file, and 2 reads did not happen, named at the top of it. "
                + Where,
                KpiPaneWords.Headline(scan, Where));
        }

        [Fact]
        public void HeadlineCountsOnlyTheLoadedInstancesAsLoaded()
        {
            KpiScan scan = KpiFixture.Build(links: KpiFixture.Links(
                new[] { KpiFixture.LinkType("RCRC_NG05_NU_00_SITE_RVT24.rvt") },
                new[]
                {
                    KpiFixture.LinkInstance("RCRC_NG05_NU_00_SITE_RVT24.rvt : 1", "RCRC_NG05_NU_00_SITE_RVT24.rvt"),
                    KpiFixture.LinkInstance("RCRC_NG05_NU_00_SITE_RVT24.rvt : 2", "RCRC_NG05_NU_00_SITE_RVT24.rvt", isLoaded: false),
                    KpiFixture.LinkInstance("RCRC_NG05_NU_01_ARCH_RVT24.rvt : 1", "RCRC_NG05_NU_01_ARCH_RVT24.rvt", isLoaded: false)
                }));

            Assert.StartsWith("0 sheets, 0 title blocks, 3 links with 1 loaded, ", KpiPaneWords.Headline(scan, Where));
        }

        [Fact]
        public void HeadlineWithNoWhereStillEndsTheSentence()
        {
            Assert.EndsWith("0 of 9 questions have something in the file. ", KpiPaneWords.Headline(KpiFixture.Build(), null));
        }

        [Fact]
        public void HeadlineRefusesANullScan()
        {
            Assert.Throws<ArgumentNullException>(() => KpiPaneWords.Headline(null, Where));
        }

        [Fact]
        public void TheFixedLinesSayTheToolReadsAndWritesAFileOnly()
        {
            Assert.Equal("No model open", KpiPaneWords.NoModelName);

            // No line names a button any more. The scan is a step inside Create, so a line
            // telling somebody to press KPI Scan would name a control the pane does not hold.
            Assert.Equal("Not read yet.", KpiPaneWords.NotScanned);
            Assert.Equal(
                "Create reads the model and writes a workbook and two text files. It creates "
                + "nothing in the model and never writes to a template.",
                KpiPaneWords.ReadOnly);
            Assert.Equal("No open document. Open a model.", KpiPaneWords.NoModel);
            Assert.DoesNotContain("KPI Scan", KpiPaneWords.NotScanned + KpiPaneWords.ReadOnly + KpiPaneWords.NoModel);
        }
    }
}
