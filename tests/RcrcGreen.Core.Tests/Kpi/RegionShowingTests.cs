using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Show on a region option: shown, not found, or a view that cannot show it, and the one
    /// line the status line carries for each. Bader's words for the last.
    /// </summary>
    public sealed class RegionShowingTests
    {
        [Fact]
        public void AFoundRegionDrawnInAPlanIsShown()
        {
            Assert.Equal(RegionShowAnswer.Shown, RegionShowing.Of(true, true, true, true));
            Assert.Equal("Selected RCRC_CADASTRAL LIMIT",
                RegionShowing.Words(RegionShowAnswer.Shown, " RCRC_CADASTRAL LIMIT "));
            Assert.Equal(string.Empty, RegionShowing.Detail(RegionShowAnswer.Shown));
        }

        /// <summary>
        /// A sheet, a schedule or any view that is not a plan, a plan hiding the link, and a
        /// plan drawing the link without this region, all say Bader's one line and change
        /// nothing. The tooltip behind it says which.
        /// </summary>
        [Fact]
        public void AViewThatCannotShowItSaysToOpenAPlan()
        {
            Assert.Equal(RegionShowAnswer.NotAPlan, RegionShowing.Of(true, false, true, true));
            Assert.Equal(RegionShowAnswer.LinkHidden, RegionShowing.Of(true, true, false, true));
            Assert.Equal(RegionShowAnswer.NotDrawn, RegionShowing.Of(true, true, true, false));
            Assert.Equal(RegionShowAnswer.NotAPlan, RegionShowing.Of(true, false, false, false));

            foreach (RegionShowAnswer answer in new[] { RegionShowAnswer.NotAPlan, RegionShowAnswer.LinkHidden, RegionShowAnswer.NotDrawn })
            {
                Assert.Equal("Open a plan that shows the 00 link", RegionShowing.Words(answer, "RCRC_CADASTRAL LIMIT"));
            }

            Assert.StartsWith("The open view is not a plan.", RegionShowing.Detail(RegionShowAnswer.NotAPlan));
            Assert.StartsWith("The open plan hides the 00 link", RegionShowing.Detail(RegionShowAnswer.LinkHidden));
            Assert.StartsWith("The open plan draws the 00 link by host view", RegionShowing.Detail(RegionShowAnswer.NotDrawn));
        }

        /// <summary>
        /// A region gone from every loaded 00 link is said before the view, because opening a
        /// plan would not find it either.
        /// </summary>
        [Fact]
        public void ARegionNoLinkHoldsIsNotFoundWhateverTheView()
        {
            Assert.Equal(RegionShowAnswer.NotFound, RegionShowing.Of(false, false, false, false));
            Assert.Equal(RegionShowAnswer.NotFound, RegionShowing.Of(false, true, true, true));
            Assert.Equal("Region not found in a loaded 00 link",
                RegionShowing.Words(RegionShowAnswer.NotFound, "RCRC_CADASTRAL LIMIT"));
        }

        /// <summary>The pane rule: no line over 10 words.</summary>
        [Fact]
        public void EveryShowLineIsTenWordsAtMost()
        {
            foreach (string line in new[]
            {
                KpiPaneWords.OpenAPlanShowingTheLink,
                KpiPaneWords.RegionNotFound,
                KpiPaneWords.RegionShown("RCRC_OUT OF SCOPE (PRESENTATION)"),
                KpiPaneWords.Show
            })
            {
                Assert.True(line.Split(' ').Length <= 10, line);
            }
        }
    }
}
