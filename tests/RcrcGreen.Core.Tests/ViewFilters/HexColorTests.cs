using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The hex parse the run's colours and the pane's swatches share, the ported host's own
    /// rule: six hex digits, a leading hash allowed, and anything else is a no.
    /// </summary>
    public class HexColorTests
    {
        [Fact]
        public void AHashAndSixDigitsParse()
        {
            Assert.True(HexColor.TryParse("#FF0000", out byte red, out byte green, out byte blue));
            Assert.Equal(255, red);
            Assert.Equal(0, green);
            Assert.Equal(0, blue);
        }

        [Fact]
        public void SixDigitsWithNoHashParse()
        {
            Assert.True(HexColor.TryParse("FF0000", out byte red, out byte green, out byte blue));
            Assert.Equal(255, red);
            Assert.Equal(0, green);
            Assert.Equal(0, blue);
        }

        [Fact]
        public void LowercaseDigitsParse()
        {
            Assert.True(HexColor.TryParse("#ff9900", out byte red, out byte green, out byte blue));
            Assert.Equal(255, red);
            Assert.Equal(153, green);
            Assert.Equal(0, blue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("#GGG")]
        [InlineData("#FF00")]
        [InlineData("#FF00000")]
        [InlineData("#GG0000")]
        public void AnythingElseIsANoRatherThanAGuess(string hex)
        {
            Assert.False(HexColor.TryParse(hex, out byte red, out byte green, out byte blue));
        }
    }
}
