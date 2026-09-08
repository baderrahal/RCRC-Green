using System;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScanFileNameTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 8, 14, 5, 0);

        [Fact]
        public void TheNameCarriesThePrefixTheTitleAndTheMinute()
        {
            Assert.Equal(
                "RCRC-Green-Scan_NG05 Landscape_2026-09-08_1405.txt",
                ScanFileName.For("NG05 Landscape", Noon));
        }

        [Fact]
        public void AMidnightScanDoesNotComeOutWithATwentyFourInIt()
        {
            Assert.Equal(
                "RCRC-Green-Scan_NG05_2026-09-08_0000.txt",
                ScanFileName.For("NG05", new DateTime(2026, 9, 8, 0, 0, 0)));
        }

        [Theory]
        [InlineData("NG05/Landscape", "RCRC-Green-Scan_NG05-Landscape_2026-09-08_1405.txt")]
        [InlineData("C:\\models\\NG05.rvt", "RCRC-Green-Scan_C-models-NG05.rvt_2026-09-08_1405.txt")]
        [InlineData("NG05 <draft>", "RCRC-Green-Scan_NG05 -draft_2026-09-08_1405.txt")]
        [InlineData("NG05***05", "RCRC-Green-Scan_NG05-05_2026-09-08_1405.txt")]
        public void CharactersWindowsWillNotTakeInAPathBecomeADash(string title, string expected)
        {
            Assert.Equal(expected, ScanFileName.For(title, Noon));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("///")]
        public void ATitleThatLeavesNothingBehindStillGivesAUsableName(string title)
        {
            Assert.Equal("RCRC-Green-Scan_untitled_2026-09-08_1405.txt", ScanFileName.For(title, Noon));
        }
    }
}
