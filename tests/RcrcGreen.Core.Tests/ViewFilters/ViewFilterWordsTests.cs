using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The pane's own lines. The default keywords are the three the round named, the cell
    /// words are the three states the grid draws, and the results block prints every count
    /// the Output carries, the two new ones included.
    /// </summary>
    public class ViewFilterWordsTests
    {
        [Fact]
        public void TheDefaultKeywordsAreTheThreeTheRoundNamed()
        {
            Assert.Equal(
                "Location Key Plan\nOverall Key Plan\nGeneral Arrangement Layout",
                ViewFilterWords.DefaultKeywords);
        }

        [Fact]
        public void TheThreeCellStatesHaveTheRoundsOwnWords()
        {
            Assert.Equal("Exists", ViewFilterWords.CellWords(ScanCell.Exists));
            Assert.Equal("Will create", ViewFilterWords.CellWords(ScanCell.WillCreate));
            Assert.Equal("Cannot create", ViewFilterWords.CellWords(ScanCell.CannotCreate));
        }

        [Fact]
        public void TheStaleLineSaysScanAgain()
        {
            Assert.Equal("Scan again", ViewFilterWords.ScanAgain);
        }

        [Fact]
        public void TheResultsBlockCarriesEveryCount()
        {
            Output output = new Output
            {
                ViewsEvaluated = 12,
                ViewsModified = 7,
                FiltersAdded = 5,
                FiltersConfigured = 21,
                FiltersCreatedInDoc = 3,
                FiltersNotFoundInDoc = 2,
                Skipped = 4,
                Blocked = 1
            };

            Assert.Equal(
                new[]
                {
                    "Views evaluated 12",
                    "Views modified 7",
                    "Filters added to views 5",
                    "Filters configured 21",
                    "Filters created in the document 3",
                    "Filter lookups with nothing to use 2",
                    "Views skipped, no plot id 4",
                    "Views blocked by their template 1"
                },
                ViewFilterWords.ResultLines(output));
        }
    }
}
