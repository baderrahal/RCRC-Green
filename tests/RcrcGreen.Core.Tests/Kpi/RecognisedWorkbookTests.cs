using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class RecognisedWorkbookTests
    {
        [Theory]
        [InlineData("Healthcare", "HEALTHCARE")]
        [InlineData("Mosques", "MOSQUES")]
        [InlineData("Parking Plots", "PARKING")]
        [InlineData("Schools", "SCHOOLS")]
        [InlineData("Streets", "STREETS")]
        public void TheFirstSheetSettlesFiveTemplatesWhateverTheFileNameSays(string firstSheet, string expected)
        {
            // The file name says EXISTING PARKS on purpose, so a match here proves the sheet won.
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - EXISTING PARKS.xlsx", new[] { firstSheet }, null);

            Assert.True(book.IsMatched);
            Assert.False(book.NeedsAPick);
            Assert.Equal(expected, book.Template.Name);
            Assert.Equal(expected, book.InWords);
        }

        [Fact]
        public void AParkNameSheetWithExistingInTheFileNameIsTheExistingParks()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - EXISTING PARKS.xlsx", new[] { "Park Name" }, null);

            Assert.True(book.IsMatched);
            Assert.Equal("EXISTING PARKS", book.Template.Name);
            Assert.Equal("GRP KPI Checklist - EXISTING PARKS.xlsx", book.FileName);
        }

        [Fact]
        public void AParkNameSheetWithFutureInTheFileNameIsTheFutureParks()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - FUTURE PARKS.xlsx", new[] { "Park Name" }, null);

            Assert.True(book.IsMatched);
            Assert.Equal("FUTURE PARKS", book.Template.Name);
        }

        [Fact]
        public void LowercaseExistingInTheFileNameStillMatches()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "grp kpi checklist - existing parks.xlsx", new[] { "Park Name" }, null);

            Assert.True(book.IsMatched);
            Assert.Equal("EXISTING PARKS", book.Template.Name);
        }

        [Fact]
        public void AParkNameSheetNamedForNeitherParkNeedsAPick()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - PARKS.xlsx", new[] { "Park Name" }, null);

            Assert.False(book.IsMatched);
            Assert.True(book.NeedsAPick);
            Assert.Null(book.Template);
            Assert.Equal(
                new[] { "EXISTING PARKS", "FUTURE PARKS" },
                book.Candidates.Select(candidate => candidate.Name));
            Assert.Equal("EXISTING PARKS or FUTURE PARKS. Pick one, nothing is guessed.", book.InWords);
        }

        [Fact]
        public void AFileNameHoldingBothParkWordsNeedsAPickToo()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - EXISTING AND FUTURE PARKS.xlsx", new[] { "Park Name" }, null);

            Assert.True(book.NeedsAPick);
            Assert.Null(book.Template);
        }

        [Fact]
        public void AWordIsHeldByAWholeRunOfLettersNotByLettersInsideAnother()
        {
            // The letter-run rule in KpiNames.Holds, so REFURBISHED does not hold FUTURE.
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "REFURBISHED.xlsx", new[] { "Park Name" }, null);

            Assert.False(book.IsMatched);
            Assert.True(book.NeedsAPick);
        }

        [Fact]
        public void AFirstSheetNoTemplateUsesIsNotOffered()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise("Numbers.xlsx", new[] { "Sheet1" }, null);

            Assert.False(book.IsMatched);
            Assert.False(book.NeedsAPick);
            Assert.Equal("Its first sheet is named Sheet1, which no template uses.", book.Reason);
            Assert.Equal("not offered. Its first sheet is named Sheet1, which no template uses.", book.InWords);
        }

        [Fact]
        public void AReadRefusalIsPassedThroughAndNothingIsMatched()
        {
            RecognisedWorkbook book = RecognisedWorkbook.Recognise(
                "GRP KPI Checklist - MOSQUES.xlsx",
                new[] { "Mosques" },
                "The file is open in another program.");

            Assert.False(book.IsMatched);
            Assert.False(book.NeedsAPick);
            Assert.Equal("The file is open in another program.", book.Reason);
            Assert.Equal("not offered. The file is open in another program.", book.InWords);
        }

        [Fact]
        public void AWorkbookWithNoReadableSheetIsNotOffered()
        {
            RecognisedWorkbook none = RecognisedWorkbook.Recognise("book.xlsx", new string[0], null);
            RecognisedWorkbook missing = RecognisedWorkbook.Recognise("book.xlsx", null, null);

            Assert.False(none.IsMatched);
            Assert.False(none.NeedsAPick);
            Assert.Equal("No sheet could be read from it.", none.Reason);
            Assert.Equal("No sheet could be read from it.", missing.Reason);
        }
    }
}
