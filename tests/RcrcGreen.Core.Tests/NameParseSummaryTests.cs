using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class NameParseSummaryTests
    {
        [Fact]
        public void TheModelNameTheFirstRealModelBrokeIsReportedAsNotParsed()
        {
            ModelScan scan = ScanFixture.Build(
                views: new[]
                {
                    ScanFixture.View("NG05", "FloorPlan"),
                    ScanFixture.View("DM-41-(400) Landscape Cross Section", "Section")
                });

            NameParseSummary summary = NameParseSummary.Of(scan);

            Assert.Contains("NG05", summary.For(NameParseSummary.ViewNameKind).NotParsed);
        }

        /// <summary>
        /// Sheet names and sheet numbers are not tallied. A sheet is named after its view with
        /// no plot and no code and numbered by code, plot letter and sheet letter, so the
        /// tally read 0 of 1,385 parsed on every scan and named a fault in the model that was
        /// not one. The sheets here would have been two failures each.
        /// </summary>
        [Fact]
        public void OnlyViewNamesAreCountedAndSheetsAreLeftAlone()
        {
            ModelScan scan = ScanFixture.Build(
                sheets: new[]
                {
                    new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 0, "DM-11"),
                    new ScannedSheet("010QF", "LIST OF DRAWINGS", 0, "DM-11")
                },
                views: new[]
                {
                    ScanFixture.View("DM-41-(010) Location Key Plan", "FloorPlan"),
                    ScanFixture.View("DM-41-(010) Overall Key Plan", "FloorPlan"),
                    ScanFixture.View("NG05", "ThreeD")
                });

            NameParseSummary summary = NameParseSummary.Of(scan);

            NameParseTally only = Assert.Single(summary.Tallies);
            Assert.Equal(NameParseSummary.ViewNameKind, only.Kind);
            Assert.Equal(2, only.Parsed);
            Assert.Equal(new[] { "NG05" }, only.NotParsed);
            Assert.Equal(3, only.Total);

            Assert.Equal(2, summary.ParsedTotal);
            Assert.Equal(1, summary.NotParsedTotal);
        }

        [Fact]
        public void AViewTemplateIsNotRunThroughTheParser()
        {
            ModelScan scan = ScanFixture.Build(
                views: new[]
                {
                    ScanFixture.View("DM-41-(010) Location Key Plan", "FloorPlan"),
                    ScanFixture.Template("Landscape Plan Template", "FloorPlan")
                });

            NameParseTally viewNames = NameParseSummary.Of(scan).For(NameParseSummary.ViewNameKind);

            Assert.Equal(1, viewNames.Total);
            Assert.Empty(viewNames.NotParsed);
        }

        [Fact]
        public void NothingIsDroppedFromTheTallyEvenWhenTheReportOnlyShowsSome()
        {
            string[] refused = Enumerable.Range(1, 45)
                .Select(number => "Sheet " + number)
                .ToArray();

            ModelScan scan = ScanFixture.Build(
                views: refused.Select(name => ScanFixture.View(name, "FloorPlan")).ToArray());

            NameParseTally viewNames = NameParseSummary.Of(scan).For(NameParseSummary.ViewNameKind);

            Assert.Equal(45, viewNames.NotParsed.Count);
            Assert.Equal(refused, viewNames.NotParsed);
        }

        [Fact]
        public void AnEmptyModelTalliesZeroRatherThanThrowing()
        {
            NameParseSummary summary = NameParseSummary.Of(ScanFixture.Build());

            NameParseTally only = Assert.Single(summary.Tallies);
            Assert.Equal(0, only.Total);
            Assert.Equal(0, summary.ParsedTotal);
            Assert.Equal(0, summary.NotParsedTotal);
        }

        [Fact]
        public void AskingForAKindThatWasNeverCountedIsAnError()
        {
            NameParseSummary summary = NameParseSummary.Of(ScanFixture.Build());

            Assert.Throws<System.ArgumentException>(() => summary.For("sheet number"));
        }
    }
}
