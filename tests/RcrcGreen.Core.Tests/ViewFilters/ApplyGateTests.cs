using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The stale scan rule: Apply is usable only while the boxes still say what the scan
    /// read. Worked out by comparing, never by a flag, so an edit undone by hand brings
    /// Apply back on its own.
    /// </summary>
    public class ApplyGateTests
    {
        private static Inputs Typical()
        {
            return new Inputs
            {
                ViewKeywords = "Location Key Plan\nOverall Key Plan",
                Filters = new[]
                {
                    new FilterConfig
                    {
                        Prefix = "(200-260) Presentation",
                        Enabled = true,
                        Visible = true,
                        LineColor = "#FF0000",
                        ForegroundPatternType = "none",
                        ForegroundPatternColor = "#00FF00",
                        BackgroundPatternType = "none",
                        BackgroundPatternColor = "#00FF00"
                    }
                }
            };
        }

        [Fact]
        public void UnchangedInputsLeaveApplyUsable()
        {
            Assert.True(ApplyGate.CanApply(Typical(), Typical()));
        }

        [Fact]
        public void NoScanMeansNoApply()
        {
            Assert.False(ApplyGate.CanApply(null, Typical()));
        }

        [Fact]
        public void AKeywordEditGreysApplyOut()
        {
            Inputs edited = Typical();
            edited.ViewKeywords = "Location Key Plan";

            Assert.False(ApplyGate.CanApply(Typical(), edited));
        }

        [Fact]
        public void EditingARowAfterAScanGreysApplyOut()
        {
            Inputs edited = Typical();
            edited.Filters[0].Halftone = true;

            Assert.False(ApplyGate.CanApply(Typical(), edited));
        }

        [Fact]
        public void ChangingAColourGreysApplyOut()
        {
            Inputs edited = Typical();
            edited.Filters[0].LineColor = "#FF0001";

            Assert.False(ApplyGate.CanApply(Typical(), edited));
        }

        [Fact]
        public void AddingARowGreysApplyOut()
        {
            Inputs edited = Typical();
            edited.Filters = new[] { edited.Filters[0], new FilterConfig { Prefix = "(215) Borders Edging" } };

            Assert.False(ApplyGate.CanApply(Typical(), edited));
        }

        [Fact]
        public void RemovingEveryRowGreysApplyOut()
        {
            Inputs edited = Typical();
            edited.Filters = new FilterConfig[0];

            Assert.False(ApplyGate.CanApply(Typical(), edited));
        }

        /// <summary>
        /// An empty hex box and one never filled mean one thing on screen, so a null and an
        /// empty string are the same answer.
        /// </summary>
        [Fact]
        public void ANullStringAndAnEmptyOneAreTheSameAnswer()
        {
            Inputs scanned = Typical();
            scanned.Filters[0].ForegroundPatternColor = null;

            Inputs current = Typical();
            current.Filters[0].ForegroundPatternColor = "";

            Assert.True(ApplyGate.CanApply(scanned, current));
        }
    }
}
