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
        [InlineData("#GGGGGG")]
        public void AnythingElseIsANoRatherThanAGuess(string hex)
        {
            Assert.False(HexColor.TryParse(hex, out byte red, out byte green, out byte blue));
        }

        /// <summary>
        /// The writer the colour picker goes through: upper case, a leading hash, two hex
        /// digits per byte. Written out by hand rather than through the parser's own rule.
        /// </summary>
        [Fact]
        public void ThreeBytesWriteAsUpperCaseHexWithAHash()
        {
            Assert.Equal("#FF9900", HexColor.Written(255, 153, 0));
            Assert.Equal("#000000", HexColor.Written(0, 0, 0));
            Assert.Equal("#0A0B0C", HexColor.Written(10, 11, 12));
        }

        [Fact]
        public void AColourSurvivesTheRoundTripAndComesBackUpperCase()
        {
            Assert.True(HexColor.TryParse("#ff9900", out byte red, out byte green, out byte blue));
            Assert.Equal("#FF9900", HexColor.Written(red, green, blue));

            Assert.True(HexColor.TryParse(HexColor.Written(18, 52, 86), out byte r2, out byte g2, out byte b2));
            Assert.Equal(18, r2);
            Assert.Equal(52, g2);
            Assert.Equal(86, b2);
        }

        /// <summary>
        /// The colour box's own rule, three ways. A valid hex is taken as typed. A mistype
        /// keeps the stored value, and the box paints its border in the warning colour so
        /// the screen says the text is not the value. A cleared box really clears, because
        /// the round's breaker found the old colour surviving behind a blank field that
        /// looked untouched, with Apply still green, and a colour somebody tried to remove
        /// coming back on the next press is worse than either other case.
        /// </summary>
        [Theory]
        [InlineData("#FF0000", "00FF00", "00FF00")]
        [InlineData("#FF0000", "#ff9900", "#ff9900")]
        [InlineData("#FF0000", "#GGGGGG", "#FF0000")]
        [InlineData("#FF0000", "#FF00", "#FF0000")]
        [InlineData("#FF0000", "", "")]
        [InlineData("#FF0000", null, "")]
        public void TypingTakesValidClearsBlankAndKeepsTheRest(string stored, string typed, string kept)
        {
            Assert.Equal(kept, HexColor.Kept(stored, typed));
        }
    }
}
