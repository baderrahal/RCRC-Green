using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The copy that keeps the pane's record of a press safe from the run. The ported run
    /// trims a row's prefix on the row object itself, and with a shared object that trim
    /// rewrote what the scan was compared against, so Apply sat on Scan again forever for
    /// a prefix typed with a stray space.
    /// </summary>
    public class InputsCopyTests
    {
        [Fact]
        public void MutatingTheCopyLeavesTheOriginalAlone()
        {
            Inputs original = new Inputs
            {
                ViewKeywords = "Location Key Plan",
                Filters = new[] { new FilterConfig { Prefix = " (215) Borders Edging ", LineWeight = 3 } }
            };

            Inputs copy = InputsCopy.Deep(original);
            copy.Filters[0].Prefix = copy.Filters[0].Prefix.Trim();
            copy.Filters[0].LineWeight = 9;

            Assert.Equal(" (215) Borders Edging ", original.Filters[0].Prefix);
            Assert.Equal(3, original.Filters[0].LineWeight);
        }

        [Fact]
        public void TheCopySaysWhatTheOriginalSays()
        {
            Inputs original = new Inputs
            {
                ViewKeywords = "Overall Key Plan",
                Filters = new[]
                {
                    new FilterConfig
                    {
                        Prefix = "(200-260) Presentation",
                        Enabled = true,
                        Visible = true,
                        OverrideLineColor = true,
                        LineColor = "#FF0000",
                        LineWeight = 5,
                        OverrideForegroundPattern = true,
                        ForegroundPatternType = "solid",
                        ForegroundPatternColor = "#00FF00",
                        OverrideBackgroundPattern = true,
                        BackgroundPatternType = "none",
                        BackgroundPatternColor = "#0000FF",
                        Halftone = true
                    }
                }
            };

            Assert.True(ApplyGate.SameInputs(original, InputsCopy.Deep(original)));
        }

        [Fact]
        public void NothingCopiesToNothingRatherThanAThrow()
        {
            Assert.Null(InputsCopy.Deep(null));
            Assert.Null(InputsCopy.Deep(new Inputs()).Filters);
        }
    }
}
