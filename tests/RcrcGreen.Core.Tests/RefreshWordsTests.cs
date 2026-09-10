using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The clauses the refresh line adds about what the read could not file. Every expected
    /// line is written out by hand, and the words for a wrong case name are the registry's own.
    /// </summary>
    public class RefreshWordsTests
    {
        private static IgnoredName WrongCase(string text)
        {
            return new IgnoredName(text, IgnoredReason.WrongCase);
        }

        private static IgnoredName NotAPlot(string text)
        {
            return new IgnoredName(text, IgnoredReason.NotAPlotName);
        }

        [Fact]
        public void AWrongCaseNameIsSaidInTheRegistrysOwnWords()
        {
            Assert.Equal(
                "1 name is a plot in the wrong case and was not read as one: dm-41 (plot "
                + "identifiers are two uppercase letters).",
                RefreshWords.WrongCase(new[] { WrongCase("dm-41") }));
        }

        /// <summary>
        /// A view name that is not plot shaped at all is the ordinary case, thousands per
        /// model, and says nothing here. Only the wrong case entries are a fault somebody can
        /// fix.
        /// </summary>
        [Fact]
        public void OnlyTheWrongCaseEntriesAreListed()
        {
            Assert.Equal(
                "2 names are a plot in the wrong case and were not read as one: dm-41 (plot "
                + "identifiers are two uppercase letters), dm-12-(010) Location Key Plan (plot "
                + "identifiers are two uppercase letters).",
                RefreshWords.WrongCase(new[]
                {
                    NotAPlot("Site Plan Working"),
                    WrongCase("dm-41"),
                    NotAPlot("Level 1"),
                    WrongCase("dm-12-(010) Location Key Plan")
                }));

            Assert.Equal(string.Empty, RefreshWords.WrongCase(new[] { NotAPlot("Level 1") }));
            Assert.Equal(string.Empty, RefreshWords.WrongCase(null));
        }

        [Fact]
        public void PastThreeNamesTheRestAreCounted()
        {
            Assert.Equal(
                "5 names are a plot in the wrong case and were not read as one: dm-1 (plot "
                + "identifiers are two uppercase letters), dm-2 (plot identifiers are two "
                + "uppercase letters), dm-3 (plot identifiers are two uppercase letters) and 2 "
                + "more.",
                RefreshWords.WrongCase(new[]
                {
                    WrongCase("dm-1"), WrongCase("dm-2"), WrongCase("dm-3"),
                    WrongCase("dm-4"), WrongCase("dm-5")
                }));
        }

        [Fact]
        public void AParameterThatIsNotAPlotIsCountedAndShown()
        {
            Assert.Equal(
                "1 view carries a PRX_Plot_ID that is not a plot, such as N/A. It still fills "
                + "its cell from its name where the name parses.",
                RefreshWords.ParameterNotAPlot(1, new[] { "N/A" }));

            Assert.Equal(
                "3 views carry a PRX_Plot_ID that is not a plot, such as N/A, Plot 12. Each "
                + "still fills its cell from its name where the name parses.",
                RefreshWords.ParameterNotAPlot(3, new[] { "N/A", "Plot 12", " N/A " }));

            Assert.Equal(string.Empty, RefreshWords.ParameterNotAPlot(0, new[] { "N/A" }));
        }

        [Fact]
        public void ACountWithNoValuesKeptStillSaysTheCount()
        {
            Assert.Equal(
                "2 views carry a PRX_Plot_ID that is not a plot. Each still fills its cell from "
                + "its name where the name parses.",
                RefreshWords.ParameterNotAPlot(2, null));
        }

        /// <summary>
        /// A Sheet List and a door schedule are not named for a plot and never will be, so
        /// they are counted and not named. The one in the wrong case is the one to fix.
        /// </summary>
        [Fact]
        public void SkippedSchedulesAreCountedWholeAndTheWrongCaseOnesNamed()
        {
            Assert.Equal(
                "3 schedules were not captured because their names do not parse, 1 of them a "
                + "plot in the wrong case: dm-41-(600) FURNITURE SCHEDULE (plot identifiers are "
                + "two uppercase letters).",
                RefreshWords.SchedulesNotCaptured(new[]
                {
                    NotAPlot("Sheet List"),
                    WrongCase("dm-41-(600) FURNITURE SCHEDULE"),
                    NotAPlot("Door Schedule")
                }));

            Assert.Equal(
                "1 schedule was not captured because its name does not parse.",
                RefreshWords.SchedulesNotCaptured(new[] { NotAPlot("Sheet List") }));

            Assert.Equal(string.Empty, RefreshWords.SchedulesNotCaptured(null));
        }
    }
}
