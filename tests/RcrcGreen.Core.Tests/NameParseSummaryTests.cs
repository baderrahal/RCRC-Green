using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class NameParseSummaryTests
    {
        [Fact]
        public void TheThreeNamesTheFirstRealModelBrokeAreAllReportedAsNotParsed()
        {
            ModelScan scan = ScanFixture.Build(
                sheets: new[]
                {
                    new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 3, "DM-11"),
                    new ScannedSheet("DM-41-(200) General Arrangement Layout", "DM-41-(010) Location Key Plan", 1, "DM-11")
                },
                views: new[]
                {
                    ScanFixture.View("NG05", "FloorPlan"),
                    ScanFixture.View("DM-41-(400) Landscape Cross Section", "Section")
                });

            NameParseSummary summary = NameParseSummary.Of(scan);

            Assert.Contains("600QD", summary.For(NameParseSummary.SheetNumberKind).NotParsed);
            Assert.Contains("SOFTSCAPE SCHEDULES", summary.For(NameParseSummary.SheetNameKind).NotParsed);
            Assert.Contains("NG05", summary.For(NameParseSummary.ViewNameKind).NotParsed);
        }

        [Fact]
        public void EachKindIsCountedOnItsOwn()
        {
            ModelScan scan = ScanFixture.Build(
                sheets: new[]
                {
                    new ScannedSheet("600QD", "SOFTSCAPE SCHEDULES", 0, "DM-11"),
                    new ScannedSheet("DM-41-(200) General Arrangement Layout", "PF-12-(200) General Arrangement Layout", 0, "DM-11")
                },
                views: new[]
                {
                    ScanFixture.View("DM-41-(010) Location Key Plan", "FloorPlan"),
                    ScanFixture.View("DM-41-(010) Overall Key Plan", "FloorPlan"),
                    ScanFixture.View("NG05", "ThreeD")
                });

            NameParseSummary summary = NameParseSummary.Of(scan);

            NameParseTally viewNames = summary.For(NameParseSummary.ViewNameKind);
            Assert.Equal(2, viewNames.Parsed);
            Assert.Equal(new[] { "NG05" }, viewNames.NotParsed);
            Assert.Equal(3, viewNames.Total);

            NameParseTally sheetNames = summary.For(NameParseSummary.SheetNameKind);
            Assert.Equal(1, sheetNames.Parsed);
            Assert.Equal(new[] { "SOFTSCAPE SCHEDULES" }, sheetNames.NotParsed);

            NameParseTally sheetNumbers = summary.For(NameParseSummary.SheetNumberKind);
            Assert.Equal(1, sheetNumbers.Parsed);
            Assert.Equal(new[] { "600QD" }, sheetNumbers.NotParsed);

            Assert.Equal(4, summary.ParsedTotal);
            Assert.Equal(3, summary.NotParsedTotal);
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
        public void AnEmptyModelTalliesZeroOfEachKindRatherThanThrowing()
        {
            NameParseSummary summary = NameParseSummary.Of(ScanFixture.Build());

            Assert.Equal(3, summary.Tallies.Count);
            Assert.All(summary.Tallies, tally => Assert.Equal(0, tally.Total));
            Assert.Equal(0, summary.ParsedTotal);
            Assert.Equal(0, summary.NotParsedTotal);
        }

        [Fact]
        public void AskingForAKindThatWasNeverCountedIsAnError()
        {
            NameParseSummary summary = NameParseSummary.Of(ScanFixture.Build());

            Assert.Throws<System.ArgumentException>(() => summary.For("family name"));
        }
    }
}
