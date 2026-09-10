using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class GridColumnsTests
    {
        private static readonly ViewType Location = new ViewType("010", "Location Key Plan");
        private static readonly ViewType Overall = new ViewType("010", "Overall Key Plan");
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Section = new ViewType("400", "Landscape Cross Section");
        private static readonly ViewType Furniture = new ViewType("600", "FURNITURE SCHEDULE");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static readonly ViewType[] Model =
        {
            Location, Overall, General, Section, Furniture, Hardscape
        };

        /// <summary>
        /// The panel used to open with every one of the 84 types ticked. Nobody works on 84 at
        /// once, and a hidden count of zero said nothing.
        /// </summary>
        [Fact]
        public void NothingIsTickedWhenTheModelIsFirstRead()
        {
            GridColumns columns = GridColumns.Over(Model);

            Assert.Equal(6, columns.All.Count);
            Assert.Equal(0, columns.ShownCount);
            Assert.Empty(columns.Shown);
        }

        /// <summary>
        /// The count and the list are the same fact. Written out by hand rather than derived
        /// with the same expression the code uses.
        /// </summary>
        [Fact]
        public void TheShownCountAlwaysEqualsTheNumberOfTickedEntries()
        {
            GridColumns columns = GridColumns.Over(Model)
                .Showing(General, true)
                .Showing(Section, true)
                .Showing(Furniture, true);

            Assert.Equal(3, columns.ShownCount);
            Assert.Equal(3, columns.Shown.Count);
            Assert.Equal(3, columns.All.Count(one => columns.IsShown(one)));
        }

        [Fact]
        public void ColumnsComeBackInCodeThenNameOrder()
        {
            GridColumns columns = GridColumns.Over(new[] { Section, Overall, General, Location })
                .ShowingThese(new[] { Section, Overall, General, Location }, true);

            Assert.Equal(
                new[]
                {
                    "(010) Location Key Plan",
                    "(010) Overall Key Plan",
                    "(200) General Arrangement Layout",
                    "(400) Landscape Cross Section"
                },
                columns.Shown.Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void SearchMatchesTheViewName()
        {
            GridColumns columns = GridColumns.Over(Model);

            Assert.Equal(
                new[] { "(600) FURNITURE SCHEDULE", "(600) HARDSCAPE SCHEDULE" },
                columns.Matching("schedule").Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void SearchMatchesTheCode()
        {
            GridColumns columns = GridColumns.Over(Model);

            Assert.Equal(
                new[] { "(010) Location Key Plan", "(010) Overall Key Plan" },
                columns.Matching("010").Select(one => one.ToString()).ToArray());
        }

        [Fact]
        public void SearchIgnoresCaseAndSurroundingSpace()
        {
            GridColumns columns = GridColumns.Over(Model);

            Assert.Equal(2, columns.Matching("  KEY plan ").Count);
        }

        [Fact]
        public void AnEmptySearchMatchesEverythingRatherThanNothing()
        {
            GridColumns columns = GridColumns.Over(Model);

            Assert.Equal(6, columns.Matching(string.Empty).Count);
            Assert.Equal(6, columns.Matching(null).Count);
            Assert.Equal(6, columns.Matching("   ").Count);
        }

        [Fact]
        public void ASearchThatMatchesNothingGivesNothing()
        {
            Assert.Empty(GridColumns.Over(Model).Matching("elevation"));
        }

        /// <summary>
        /// All acts on what the search is showing, not on the whole list. Ticking 84 to reach
        /// four is not a thing anyone should have to do.
        /// </summary>
        [Fact]
        public void AllTicksOnlyWhatTheFilterShows()
        {
            GridColumns columns = GridColumns.Over(Model);
            columns = columns.ShowingThese(columns.Matching("schedule"), true);

            Assert.Equal(2, columns.ShownCount);
            Assert.True(columns.IsShown(Furniture));
            Assert.True(columns.IsShown(Hardscape));
            Assert.False(columns.IsShown(General));
        }

        [Fact]
        public void NoneUntiksOnlyWhatTheFilterShows()
        {
            GridColumns columns = GridColumns.Over(Model)
                .ShowingThese(Model, true);
            columns = columns.ShowingThese(columns.Matching("schedule"), false);

            Assert.Equal(4, columns.ShownCount);
            Assert.False(columns.IsShown(Furniture));
            Assert.True(columns.IsShown(General));
        }

        [Fact]
        public void ACodeButtonTicksEveryTypeCarryingThatCode()
        {
            GridColumns columns = GridColumns.Over(Model).ShowingCode("010");

            Assert.Equal(2, columns.ShownCount);
            Assert.True(columns.IsShown(Location));
            Assert.True(columns.IsShown(Overall));
        }

        [Fact]
        public void ACodeButtonLeavesWhatIsAlreadyTickedAlone()
        {
            GridColumns columns = GridColumns.Over(Model)
                .Showing(General, true)
                .ShowingCode("600");

            Assert.Equal(3, columns.ShownCount);
            Assert.True(columns.IsShown(General));
            Assert.True(columns.IsShown(Furniture));
            Assert.True(columns.IsShown(Hardscape));
        }

        [Fact]
        public void TheCodesInUseComeFromTheModelInNumberOrder()
        {
            Assert.Equal(
                new[] { "010", "200", "400", "600" },
                GridColumns.Over(Model).CodesInUse.ToArray());
        }

        /// <summary>
        /// The tool must never invent a plot. Inventing a view type is what it is for.
        /// </summary>
        [Fact]
        public void AnAddedTypeIsTickedAndMarkedNew()
        {
            var wanted = new ViewType("600", "IRRIGATION SCHEDULE");
            GridColumns columns = GridColumns.Over(Model).Adding(wanted);

            Assert.Equal(7, columns.All.Count);
            Assert.True(columns.IsShown(wanted));
            Assert.True(columns.IsNew(wanted));
            Assert.False(columns.IsNew(Furniture));
            Assert.Equal(1, columns.ShownCount);
        }

        [Fact]
        public void AddingATypeTheModelAlreadyHoldsChangesNothing()
        {
            GridColumns columns = GridColumns.Over(Model)
                .Adding(new ViewType("600", "FURNITURE SCHEDULE"));

            Assert.Equal(6, columns.All.Count);
            Assert.False(columns.IsNew(Furniture));
            Assert.Equal(0, columns.ShownCount);
        }

        [Fact]
        public void AnAddedTypeSurvivesARefresh()
        {
            var wanted = new ViewType("600", "IRRIGATION SCHEDULE");
            GridColumns columns = GridColumns.Over(Model)
                .Adding(wanted)
                .OverTheseTypes(Model);

            Assert.Equal(7, columns.All.Count);
            Assert.True(columns.IsNew(wanted));
            Assert.True(columns.IsShown(wanted));
        }

        /// <summary>
        /// Once the run has made it, it is an ordinary column and stops being marked new.
        /// </summary>
        [Fact]
        public void AnAddedTypeStopsBeingNewOnceTheModelHoldsIt()
        {
            var wanted = new ViewType("600", "IRRIGATION SCHEDULE");
            GridColumns columns = GridColumns.Over(Model)
                .Adding(wanted)
                .OverTheseTypes(Model.Concat(new[] { wanted }));

            Assert.Equal(7, columns.All.Count);
            Assert.False(columns.IsNew(wanted));
            Assert.True(columns.IsShown(wanted));
        }

        [Fact]
        public void TicksSurviveARefresh()
        {
            GridColumns columns = GridColumns.Over(Model)
                .Showing(General, true)
                .OverTheseTypes(Model);

            Assert.Equal(1, columns.ShownCount);
            Assert.True(columns.IsShown(General));
        }

        [Fact]
        public void ATickedTypeThatLeavesTheModelIsForgotten()
        {
            GridColumns columns = GridColumns.Over(Model)
                .Showing(General, true)
                .OverTheseTypes(new[] { Location, Overall });

            Assert.Equal(2, columns.All.Count);
            Assert.Equal(0, columns.ShownCount);
        }

        [Fact]
        public void TickingATypeTheListDoesNotHoldChangesNothing()
        {
            GridColumns columns = GridColumns.Over(new[] { Location })
                .Showing(Section, true);

            Assert.Equal(0, columns.ShownCount);
            Assert.Single(columns.All);
        }

        [Fact]
        public void ADuplicateTypeIsOneColumn()
        {
            GridColumns columns = GridColumns.Over(
                new[] { General, new ViewType("200", "General Arrangement Layout") });

            Assert.Single(columns.All);
        }

        [Fact]
        public void NoTypesAtAllIsAnEmptyAnswerEverywhere()
        {
            GridColumns columns = GridColumns.Over(null);

            Assert.Empty(columns.All);
            Assert.Empty(columns.Shown);
            Assert.Empty(columns.CodesInUse);
            Assert.Empty(columns.Matching("anything"));
        }
    }
}
