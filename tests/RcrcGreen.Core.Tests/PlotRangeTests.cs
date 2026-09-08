using System.Collections.Generic;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class PlotRangeTests
    {
        private static readonly string[] InTheModel =
        {
            "DM-11", "DM-2", "DM-100", "DM-28", "DM-9", "DM-41", "PF-12", "PF-3", "AB-7"
        };

        [Fact]
        public void OnlyThePrefixesActuallyInTheModelAreOffered()
        {
            Assert.Equal(new[] { "AB", "DM", "PF" }, PlotRange.PrefixesIn(InTheModel));
        }

        [Fact]
        public void AnythingThatIsNotAPlotIdentifierHasNoPrefixAndIsLeftOut()
        {
            Assert.Equal(
                new[] { "DM" },
                PlotRange.PrefixesIn(new[] { "DM-41", "SOFTSCAPE SCHEDULES", "600QD", "NG05", "dm-41", "" }));
        }

        [Fact]
        public void PlotsUnderAPrefixComeBackWithTheNumberReadAsANumber()
        {
            Assert.Equal(
                new[] { "DM-2", "DM-9", "DM-11", "DM-28", "DM-41", "DM-100" },
                PlotRange.WithPrefix(InTheModel, "DM"));
        }

        [Fact]
        public void ARangeReturnsOnlyThePlotsInsideItAndNothingOutside()
        {
            Assert.Equal(
                new[] { "DM-11", "DM-28" },
                PlotRange.Between(InTheModel, "DM", "DM-11", "DM-28"));
        }

        [Fact]
        public void BothEndsOfTheRangeAreIncluded()
        {
            Assert.Equal(
                new[] { "DM-2", "DM-9", "DM-11" },
                PlotRange.Between(InTheModel, "DM", "DM-2", "DM-11"));
        }

        [Fact]
        public void OnePlotToItselfIsARangeOfOne()
        {
            Assert.Equal(new[] { "DM-41" }, PlotRange.Between(InTheModel, "DM", "DM-41", "DM-41"));
        }

        [Fact]
        public void AWholePrefixCanBeSelectedEndToEnd()
        {
            Assert.Equal(
                PlotRange.WithPrefix(InTheModel, "DM"),
                PlotRange.Between(InTheModel, "DM", "DM-2", "DM-100"));
        }

        [Fact]
        public void ARangeWithTheEndsTheWrongWayRoundIsEmptyRatherThanAThrow()
        {
            Assert.Empty(PlotRange.Between(InTheModel, "DM", "DM-28", "DM-11"));
        }

        [Fact]
        public void ARangeThatNamesAPlotNotInTheModelIsEmpty()
        {
            Assert.Empty(PlotRange.Between(InTheModel, "DM", "DM-11", "DM-999"));
            Assert.Empty(PlotRange.Between(InTheModel, "DM", "DM-999", "DM-28"));
        }

        [Fact]
        public void ARangeNeverReachesIntoAnotherPrefix()
        {
            IReadOnlyList<string> ranged = PlotRange.Between(InTheModel, "DM", "DM-2", "DM-100");

            Assert.DoesNotContain("PF-3", ranged);
            Assert.DoesNotContain("PF-12", ranged);
            Assert.DoesNotContain("AB-7", ranged);
        }

        [Fact]
        public void APrefixWithOnlyOnePlotStillWorks()
        {
            Assert.Equal(new[] { "AB-7" }, PlotRange.WithPrefix(InTheModel, "AB"));
            Assert.Equal(new[] { "AB-7" }, PlotRange.Between(InTheModel, "AB", "AB-7", "AB-7"));
        }

        [Fact]
        public void APrefixThatIsNotInTheModelGivesNothingToChooseFrom()
        {
            Assert.Empty(PlotRange.WithPrefix(InTheModel, "ZZ"));
            Assert.Empty(PlotRange.Between(InTheModel, "ZZ", "ZZ-1", "ZZ-9"));
        }

        [Fact]
        public void ADuplicatePlotIsOfferedOnce()
        {
            Assert.Equal(
                new[] { "DM-2", "DM-41" },
                PlotRange.WithPrefix(new[] { "DM-41", "DM-2", "DM-41" }, "DM"));
        }

        [Fact]
        public void NothingAtAllIsAnEmptyAnswerEverywhere()
        {
            Assert.Empty(PlotRange.PrefixesIn(null));
            Assert.Empty(PlotRange.WithPrefix(null, "DM"));
            Assert.Empty(PlotRange.WithPrefix(InTheModel, null));
            Assert.Empty(PlotRange.Between(null, "DM", "DM-2", "DM-9"));
            Assert.Empty(PlotRange.Between(InTheModel, "DM", null, "DM-9"));
            Assert.Empty(PlotRange.Between(InTheModel, "DM", "DM-2", null));
        }
    }
}
