using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The keyword split the run and the scan share, the ported host's own: a comma, a
    /// newline or a semicolon separates, entries are trimmed and blanks dropped.
    /// </summary>
    public class ViewKeywordsTests
    {
        [Fact]
        public void CommasSeparate()
        {
            Assert.Equal(
                new[] { "Location Key Plan", "Overall Key Plan" },
                ViewKeywords.Split("Location Key Plan, Overall Key Plan"));
        }

        [Fact]
        public void NewlinesSeparateAndACarriageReturnIsTrimmedOff()
        {
            Assert.Equal(
                new[] { "Location Key Plan", "Overall Key Plan", "General Arrangement Layout" },
                ViewKeywords.Split("Location Key Plan\r\nOverall Key Plan\r\nGeneral Arrangement Layout"));
        }

        [Fact]
        public void TheSemicolonCharacterSeparates()
        {
            Assert.Equal(
                new[] { "Location Key Plan", "Overall Key Plan" },
                ViewKeywords.Split("Location Key Plan;Overall Key Plan"));
        }

        [Fact]
        public void BlankEntriesAreDropped()
        {
            Assert.Equal(
                new[] { "Location Key Plan" },
                ViewKeywords.Split(",,  ,Location Key Plan,   "));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NothingTypedIsNoKeywordsRatherThanAThrow(string text)
        {
            Assert.Empty(ViewKeywords.Split(text));
        }
    }
}
