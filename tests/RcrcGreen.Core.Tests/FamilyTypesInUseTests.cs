using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Which view family type each view type is really built with.
    ///
    /// The real case, from one run. Three (010) views were set up from three different siblings
    /// and came out with three different family types, every one copied faithfully:
    ///
    ///   DM-11-(010) Overall Plan       family type (200) General Arrangement Layout
    ///   DM-11-(010) Location Key Plan  family type (010) Key Location Plan
    ///   DM-11-(010) Overall Key Plan   family type (010) Key Plan
    ///
    /// The model has no rule. This counts what is there and picks no winner.
    /// </summary>
    public class FamilyTypesInUseTests
    {
        private static ScannedView View(string name, string familyType)
        {
            return new ScannedView(name, "FloorPlan", false, string.Empty, familyType);
        }

        private static ScannedView Template(string name, string familyType)
        {
            return new ScannedView(name, "FloorPlan", true, string.Empty, familyType);
        }

        [Fact]
        public void OneFamilyTypeEverywhereIsOneRowAndNoDisagreement()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(200) General Arrangement Layout", "(200) General Arrangement Layout"),
                View("DM-12-(200) General Arrangement Layout", "(200) General Arrangement Layout"),
                View("DM-13-(200) General Arrangement Layout", "(200) General Arrangement Layout")
            });

            FamilyTypesForViewType only = Assert.Single(inUse);

            Assert.Equal("(200) General Arrangement Layout", only.Type.ToString());
            Assert.False(only.Disagrees);
            Assert.Equal(3, only.Views);
            Assert.Equal("(200) General Arrangement Layout", only.Counts[0].FamilyTypeName);
            Assert.Equal(3, only.Counts[0].Views);
            Assert.Equal(0, FamilyTypesInUse.Disagreeing(inUse));
        }

        [Fact]
        public void TwoFamilyTypesForOneViewTypeIsWhatTheUserNeedsToSee()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(010) Overall Plan", "(200) General Arrangement Layout"),
                View("DM-12-(010) Overall Plan", "(010) Overall Plan"),
                View("DM-13-(010) Overall Plan", "(010) Overall Plan")
            });

            FamilyTypesForViewType only = Assert.Single(inUse);

            Assert.True(only.Disagrees);
            Assert.Equal(3, only.Views);
            Assert.Equal(1, FamilyTypesInUse.Disagreeing(inUse));
        }

        /// <summary>
        /// Most used first, because that is the order somebody reads a list of two in. It says
        /// nothing about which is right and nothing picks it.
        /// </summary>
        [Fact]
        public void TheMostUsedFamilyTypeIsListedFirst()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(010) Overall Plan", "(200) General Arrangement Layout"),
                View("DM-12-(010) Overall Plan", "(010) Overall Plan"),
                View("DM-13-(010) Overall Plan", "(010) Overall Plan")
            });

            Assert.Equal(
                new[] { "(010) Overall Plan", "(200) General Arrangement Layout" },
                inUse[0].Counts.Select(one => one.FamilyTypeName).ToArray());

            Assert.Equal(new[] { 2, 1 }, inUse[0].Counts.Select(one => one.Views).ToArray());
        }

        /// <summary>
        /// A view type that disagrees comes above one that does not, whatever the code order.
        /// A report that buries the one thing worth acting on is a report nobody reads twice.
        /// </summary>
        [Fact]
        public void DisagreeingViewTypesComeFirst()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(200) General Arrangement Layout", "(200) General Arrangement Layout"),
                View("DM-11-(400) Landscape Cross Section", "(400) Section A"),
                View("DM-12-(400) Landscape Cross Section", "(400) Section B")
            });

            Assert.Equal(2, inUse.Count);
            Assert.Equal("(400) Landscape Cross Section", inUse[0].Type.ToString());
            Assert.True(inUse[0].Disagrees);
            Assert.False(inUse[1].Disagrees);
        }

        [Fact]
        public void TwoViewTypesSharingACodeAreCountedApart()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(010) Location Key Plan", "(010) Key Location Plan"),
                View("DM-11-(010) Overall Key Plan", "(010) Key Plan")
            });

            Assert.Equal(2, inUse.Count);
            Assert.All(inUse, one => Assert.False(one.Disagrees));
            Assert.Equal(
                new[] { "(010) Location Key Plan", "(010) Overall Key Plan" },
                inUse.Select(one => one.Type.ToString()).OrderBy(name => name).ToArray());
        }

        /// <summary>
        /// A template is not a view on a plot and has its own section in the report, so counting
        /// it here would make a view type look as though it disagreed with itself.
        /// </summary>
        [Fact]
        public void TemplatesAreLeftOut()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(200) General Arrangement Layout", "(200) General Arrangement Layout"),
                Template("DM-11-(200) General Arrangement Layout", "SOMETHING ELSE")
            });

            FamilyTypesForViewType only = Assert.Single(inUse);
            Assert.False(only.Disagrees);
            Assert.Equal(1, only.Views);
        }

        [Fact]
        public void ANameThatDoesNotParseHasNoViewTypeToCountUnder()
        {
            Assert.Empty(FamilyTypesInUse.Of(new[]
            {
                View("Site Plan", "(200) General Arrangement Layout"),
                View("Level 1", "(200) General Arrangement Layout")
            }));

            Assert.Empty(FamilyTypesInUse.Of(null));
        }

        /// <summary>
        /// A view whose family type could not be read is still counted, under an empty name, so
        /// the number of views in the section always matches the number in the model.
        /// </summary>
        [Fact]
        public void AViewWithNoFamilyTypeReadIsStillCounted()
        {
            var inUse = FamilyTypesInUse.Of(new[]
            {
                View("DM-11-(200) General Arrangement Layout", null),
                View("DM-12-(200) General Arrangement Layout", "(200) General Arrangement Layout")
            });

            FamilyTypesForViewType only = Assert.Single(inUse);

            Assert.True(only.Disagrees);
            Assert.Equal(2, only.Views);
            Assert.Contains(only.Counts, one => one.FamilyTypeName.Length == 0);
        }
    }
}
