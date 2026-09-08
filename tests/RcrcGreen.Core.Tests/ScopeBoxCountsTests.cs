using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScopeBoxCountsTests
    {
        /// <summary>
        /// Eight views over three plots, laid out so every case has a known occupant and the
        /// expected numbers below are read off this list by hand.
        ///
        /// DM-11 has A one, C one, E one.
        /// DM-12 has B one, D one, F one.
        /// DM-13 has C one.
        /// The unparsed name belongs to no plot at all.
        /// </summary>
        private static readonly ViewScopeBoxState[] Views =
        {
            new ViewScopeBoxState(1, "DM-11-(010) Location Key Plan", true, string.Empty),
            new ViewScopeBoxState(2, "DM-11-(200) General Arrangement Layout", true, "DM-11"),
            new ViewScopeBoxState(3, "DM-12-(010) Location Key Plan", false, string.Empty),
            new ViewScopeBoxState(4, "DM-12-(200) General Arrangement Layout", true, string.Empty),
            new ViewScopeBoxState(5, "DM-12-(400) Landscape Cross Section", true, "DM-41"),
            new ViewScopeBoxState(6, "DM-13-(010) Location Key Plan", true, string.Empty),
            new ViewScopeBoxState(7, "Working Plan For Coordination", true, string.Empty),
            new ViewScopeBoxState(8, "DM-11-(400) Landscape Cross Section", true, string.Empty)
        };

        // No box named DM-12, which is what puts the DM-12 view into D.
        private static readonly string[] Boxes = { "DM-11", "DM-13", "DM-41", "Site Overall" };

        [Fact]
        public void OnePlotCountsOnlyItsOwnViews()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, new[] { "DM-11" });

            Assert.Equal(3, counts.Considered);
            Assert.Equal(2, counts.Of(ScopeBoxCase.ReadyToAssign));
            Assert.Equal(1, counts.Of(ScopeBoxCase.AlreadyRight));
            Assert.Equal(0, counts.Of(ScopeBoxCase.NameDoesNotParse));
        }

        [Fact]
        public void EachOfTheSixCasesIsFoundWhereItWasPutByHand()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, new[] { "DM-11", "DM-12", "DM-13" });

            Assert.Equal(7, counts.Considered);
            Assert.Equal(0, counts.Of(ScopeBoxCase.NameDoesNotParse));
            Assert.Equal(1, counts.Of(ScopeBoxCase.CannotHoldAScopeBox));
            Assert.Equal(3, counts.Of(ScopeBoxCase.ReadyToAssign));
            Assert.Equal(1, counts.Of(ScopeBoxCase.NoMatchingScopeBox));
            Assert.Equal(1, counts.Of(ScopeBoxCase.AlreadyRight));
            Assert.Equal(1, counts.Of(ScopeBoxCase.HoldsADifferentScopeBox));
        }

        /// <summary>
        /// The whole reason the counts are on screen. Unticking DM-12 has to take its three
        /// views out of what Assign would do.
        /// </summary>
        [Fact]
        public void UntickingAPlotTakesItsViewsOutOfTheCounts()
        {
            ScopeBoxCounts everything = ScopeBoxCounts.For(Views, Boxes, new[] { "DM-11", "DM-12", "DM-13" });
            ScopeBoxCounts without = ScopeBoxCounts.For(Views, Boxes, new[] { "DM-11", "DM-13" });

            Assert.Equal(7, everything.Considered);
            Assert.Equal(4, without.Considered);
            Assert.Equal(3, without.Of(ScopeBoxCase.ReadyToAssign));
            Assert.Equal(0, without.Of(ScopeBoxCase.NoMatchingScopeBox));
            Assert.Equal(0, without.Of(ScopeBoxCase.CannotHoldAScopeBox));
            Assert.Equal(0, without.Of(ScopeBoxCase.HoldsADifferentScopeBox));
        }

        [Fact]
        public void NoPlotsTickedCountsNothing()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, new string[0]);

            Assert.Equal(0, counts.Considered);
            Assert.Equal(0, counts.ReadyToAssign);
        }

        /// <summary>
        /// A view whose name does not parse belongs to no plot, so no tick can bring it in.
        /// </summary>
        [Fact]
        public void AViewWithAnUnparsedNameIsNeverCounted()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(
                Views, Boxes, new[] { "DM-11", "DM-12", "DM-13", "DM-41" });

            Assert.Equal(7, counts.Considered);
            Assert.DoesNotContain(
                ScopeBoxCounts.Narrow(Views, new[] { "DM-11", "DM-12", "DM-13", "DM-41" }),
                view => view.ViewId == 7);
        }

        [Fact]
        public void NarrowGivesBackTheSameViewsTheCountsWereWorkedOutOver()
        {
            var narrowed = ScopeBoxCounts.Narrow(Views, new[] { "DM-13" }).ToList();

            Assert.Single(narrowed);
            Assert.Equal(6, narrowed[0].ViewId);
        }

        [Fact]
        public void APlotWithNoViewsAtAllCountsNothingRatherThanThrowing()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(Views, Boxes, new[] { "PF-3" });

            Assert.Equal(0, counts.Considered);
        }

        [Fact]
        public void NoViewsAndNoBoxesIsAnEmptyAnswer()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(null, null, new[] { "DM-11" });

            Assert.Equal(0, counts.Considered);
            Assert.Equal(0, counts.ReadyToAssign);
        }

        /// <summary>
        /// Case sensitive, the same rule the plot identifier follows. A box named dm-11 is not
        /// the one DM-11 wants, so its view stays in D.
        /// </summary>
        [Fact]
        public void AScopeBoxWithTheWrongCaseDoesNotMatch()
        {
            ScopeBoxCounts counts = ScopeBoxCounts.For(
                new[] { new ViewScopeBoxState(1, "DM-11-(010) Location Key Plan", true, string.Empty) },
                new[] { "dm-11" },
                new[] { "DM-11" });

            Assert.Equal(0, counts.Of(ScopeBoxCase.ReadyToAssign));
            Assert.Equal(1, counts.Of(ScopeBoxCase.NoMatchingScopeBox));
        }
    }
}
