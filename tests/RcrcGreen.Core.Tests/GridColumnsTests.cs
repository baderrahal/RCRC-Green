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

        [Fact]
        public void EveryTypeInTheModelIsAColumnToBeginWith()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { General, Location, Section });

            Assert.Equal(3, columns.Shown.Count);
            Assert.Equal(0, columns.HiddenCount);
        }

        /// <summary>
        /// Written out by hand rather than sorted with the same comparer the code uses. Code
        /// 010 before 200 before 400, and the two 010 types apart by their view name.
        /// </summary>
        [Fact]
        public void ColumnsComeBackInCodeThenNameOrder()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Section, Overall, General, Location });

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
        public void HidingOneTakesItOutOfShownAndCountsIt()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location, General, Section })
                .Showing(General, false);

            Assert.Equal(new[] { "(010) Location Key Plan", "(400) Landscape Cross Section" },
                columns.Shown.Select(one => one.ToString()).ToArray());
            Assert.Equal(1, columns.HiddenCount);
            Assert.False(columns.IsShown(General));
        }

        [Fact]
        public void AHiddenColumnStaysInTheFullList()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location, General })
                .Showing(General, false);

            Assert.Equal(2, columns.All.Count);
        }

        [Fact]
        public void ShowingItAgainBringsItBack()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location, General })
                .Showing(General, false)
                .Showing(General, true);

            Assert.Equal(2, columns.Shown.Count);
            Assert.Equal(0, columns.HiddenCount);
        }

        [Fact]
        public void HidingATypeTheModelDoesNotHoldChangesNothing()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location })
                .Showing(Section, false);

            Assert.Equal(1, columns.Shown.Count);
            Assert.Equal(0, columns.HiddenCount);
        }

        [Fact]
        public void HidingIsKeptAcrossARefreshThatStillHoldsTheType()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location, General, Section })
                .Showing(General, false)
                .OverTheseTypes(new[] { Location, General, Section, Overall });

            Assert.Equal(4, columns.All.Count);
            Assert.Equal(1, columns.HiddenCount);
            Assert.False(columns.IsShown(General));
        }

        /// <summary>
        /// A hidden type that leaves the model is forgotten, so the hidden count cannot report
        /// a column nothing can show again.
        /// </summary>
        [Fact]
        public void AHiddenTypeThatLeavesTheModelIsForgotten()
        {
            GridColumns columns = GridColumns.ShowingAll(new[] { Location, General })
                .Showing(General, false)
                .OverTheseTypes(new[] { Location });

            Assert.Equal(1, columns.All.Count);
            Assert.Equal(0, columns.HiddenCount);
        }

        [Fact]
        public void ADuplicateTypeIsOneColumn()
        {
            GridColumns columns = GridColumns.ShowingAll(
                new[] { General, new ViewType("200", "General Arrangement Layout") });

            Assert.Equal(1, columns.All.Count);
        }

        [Fact]
        public void NoTypesAtAllIsAnEmptyAnswerEverywhere()
        {
            GridColumns columns = GridColumns.ShowingAll(null);

            Assert.Empty(columns.All);
            Assert.Empty(columns.Shown);
            Assert.Equal(0, columns.HiddenCount);
        }
    }
}
