using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ViewNameParserTests
    {
        [Fact]
        public void SplitsARealNameIntoPlotCodeAndViewName()
        {
            ParsedViewName parsed;
            bool ok = ViewNameParser.TryParse("DM-41-(010) Location Key Plan", out parsed);

            Assert.True(ok);
            Assert.Equal("DM-41", parsed.PlotId);
            Assert.Equal("010", parsed.Code);
            Assert.Equal("Location Key Plan", parsed.ViewName);
            Assert.Equal("DM-41-(010) Location Key Plan", parsed.Original);
        }

        [Fact]
        public void KeepsTheWholeViewNameWhenItRunsToSeveralWords()
        {
            ParsedViewName parsed;
            Assert.True(ViewNameParser.TryParse("PF-12-(200) General Arrangement Layout", out parsed));

            Assert.Equal("PF-12", parsed.PlotId);
            Assert.Equal("200", parsed.Code);
            Assert.Equal("General Arrangement Layout", parsed.ViewName);
        }

        [Fact]
        public void TwoNamesOnOneCodeAreTwoViewTypes()
        {
            ParsedViewName location;
            ParsedViewName overall;
            Assert.True(ViewNameParser.TryParse("DM-41-(010) Location Key Plan", out location));
            Assert.True(ViewNameParser.TryParse("DM-41-(010) Overall Key Plan", out overall));

            Assert.Equal(location.Code, overall.Code);
            Assert.NotEqual(location.Type, overall.Type);
        }

        [Fact]
        public void TheSameCodeAndViewNameOnTwoPlotsIsOneViewType()
        {
            ParsedViewName onDm41;
            ParsedViewName onPf12;
            Assert.True(ViewNameParser.TryParse("DM-41-(200) General Arrangement Layout", out onDm41));
            Assert.True(ViewNameParser.TryParse("PF-12-(200) General Arrangement Layout", out onPf12));

            Assert.Equal(onDm41.Type, onPf12.Type);
        }

        [Theory]
        [InlineData("Site Plan")]
        [InlineData("DM-41 Location Key Plan")]
        [InlineData("DM-41-(010)Location Key Plan")]
        [InlineData("DM-41-(010) ")]
        [InlineData("DM-41-(0A0) Location Key Plan")]
        [InlineData("D-41-(010) Location Key Plan")]
        [InlineData("DMX-41-(010) Location Key Plan")]
        [InlineData("DM-(010) Location Key Plan")]
        [InlineData("")]
        [InlineData(null)]
        public void AnythingOffThePatternComesBackFalseWithoutThrowing(string name)
        {
            ParsedViewName parsed;
            bool ok = ViewNameParser.TryParse(name, out parsed);

            Assert.False(ok);
            Assert.Null(parsed);
        }

        [Fact]
        public void AStrayNewlineDoesNotGetTrimmedAwayIntoAValidName()
        {
            ParsedViewName parsed;

            Assert.False(ViewNameParser.TryParse("DM-41-(010) Location Key Plan\n", out parsed));
            Assert.False(ViewNameParser.TryParse("\nDM-41-(010) Location Key Plan", out parsed));
        }

        [Fact]
        public void AViewNameHoldingBracketsOfItsOwnStillParses()
        {
            ParsedViewName parsed;
            Assert.True(ViewNameParser.TryParse("DM-41-(400) Landscape Cross Section (north)", out parsed));

            Assert.Equal("400", parsed.Code);
            Assert.Equal("Landscape Cross Section (north)", parsed.ViewName);
        }
    }
}
