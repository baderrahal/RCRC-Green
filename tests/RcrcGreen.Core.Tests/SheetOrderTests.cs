using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The team's sheet order, read off both measured models and confirmed by the user. It
    /// is also the sheet letter order: 010001A is TITLE SHEET and 010001B is LIST OF
    /// DRAWINGS because of the list.
    /// </summary>
    public class SheetOrderTests
    {
        [Fact]
        public void TheNineNamesSitInTheTeamsOrder()
        {
            Assert.Equal(
                new[]
                {
                    "TITLE SHEET",
                    "LIST OF DRAWINGS",
                    "PROJECT LOCATION KEY PLAN",
                    "OVERALL KEYPLAN",
                    "GENERAL ARRANGEMENT LAYOUT",
                    "COORDINATION LAYOUT",
                    "LANDSCAPE CROSS SECTION",
                    "HARDSCAPE SCHEDULES",
                    "SOFTSCAPE SCHEDULES"
                },
                SheetOrder.TheTeamsOrder.ToArray());

            for (int at = 0; at < SheetOrder.TheTeamsOrder.Count; at++)
            {
                Assert.Equal(at, SheetOrder.Position(SheetOrder.TheTeamsOrder[at]));
            }
        }

        /// <summary>
        /// A typed name arrives in whatever case and spacing somebody used, so the position
        /// forgives case and edge spaces and nothing else. OVERALL KEY PLAN with the space
        /// is not the list's OVERALL KEYPLAN and reads as unlisted: whether the two are one
        /// name is the team's to answer, not a string rule's.
        /// </summary>
        [Fact]
        public void ThePositionForgivesCaseAndEdgeSpacesOnly()
        {
            Assert.Equal(0, SheetOrder.Position("  title sheet "));
            Assert.Equal(1, SheetOrder.Position("List of Drawings"));

            int unlisted = SheetOrder.TheTeamsOrder.Count;
            Assert.Equal(unlisted, SheetOrder.Position("OVERALL KEY PLAN"));
            Assert.Equal(unlisted, SheetOrder.Position("HARDSCAPE SCHEDULE"));
            Assert.Equal(unlisted, SheetOrder.Position(string.Empty));
            Assert.Equal(unlisted, SheetOrder.Position(null));
        }

        [Fact]
        public void TheOrderingCodeIsTheFirstViewsAndEmptyForNone()
        {
            Assert.Equal(string.Empty, SheetOrder.CodeToOrderBy(null));
            Assert.Equal(string.Empty, SheetOrder.CodeToOrderBy(new ViewType[0]));

            Assert.Equal("010", SheetOrder.CodeToOrderBy(new[]
            {
                new ViewType("010", "Location Key Plan")
            }));

            Assert.Equal("600", SheetOrder.CodeToOrderBy(new[]
            {
                new ViewType("600", "HARDSCAPE SCHEDULE"),
                new ViewType("010", "Location Key Plan")
            }));
        }

        [Fact]
        public void ASheetWithNoCodeSortsAheadOfTheCodedOnes()
        {
            Assert.Equal(0, SheetOrder.CodeRank(string.Empty));
            Assert.Equal(0, SheetOrder.CodeRank(null));
            Assert.Equal(1, SheetOrder.CodeRank("010"));
        }

        /// <summary>
        /// The division hands its sheets back in the team's order however they were ticked:
        /// the code first, then the list within a code, then the ticked order for names the
        /// list does not hold.
        /// </summary>
        [Fact]
        public void TheDivisionReordersTickedSheetsIntoTheTeamsOrder()
        {
            var planned = SheetDivision.Of(new[]
            {
                new ViewType("600", "HARDSCAPE SCHEDULE"),
                new ViewType("010", "Location Key Plan"),
                new ViewType("400", "Landscape Cross Section")
            }, 1);

            Assert.Equal(
                new[] { "010", "400", "600" },
                planned.Select(one => one.SingleCode).ToArray());
        }

        [Fact]
        public void WithinACodeTheListDecidesAndTheTickedOrderBreaksItsTies()
        {
            var listed = SheetDivision.Of(new[]
            {
                new ViewType("010", "Project Location Key Plan"),
                new ViewType("010", "List of Drawings")
            }, 1);

            Assert.Equal(
                new[] { "LIST OF DRAWINGS", "PROJECT LOCATION KEY PLAN" },
                listed.Select(one => one.ProposedName).ToArray());

            // Neither schedule view's singular name is on the list, the plural sheet names
            // are, so these two keep the order they were ticked in.
            var unlisted = SheetDivision.Of(new[]
            {
                new ViewType("600", "KERBS SCHEDULE"),
                new ViewType("600", "FURNITURE SCHEDULE")
            }, 1);

            Assert.Equal(
                new[] { "KERBS SCHEDULE", "FURNITURE SCHEDULE" },
                unlisted.Select(one => one.ProposedName).ToArray());
        }
    }

    /// <summary>
    /// The same order at Run: the sheets are created plot by plot in the team's order, so
    /// the set stops coming out in whatever order the user ticked.
    /// </summary>
    public class SheetOrderAtRunTests
    {
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        private static readonly ViewType ListOfDrawings = new ViewType("010", "List of Drawings");

        [Fact]
        public void TheRunMakesSheetsInTheTeamsOrder()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Batch(new[] { Hardscape }, 1,
                    RunFixture.Row("DM-12", "600RA", "HARDSCAPE SCHEDULES", new[] { Hardscape }),
                    RunFixture.Row("DM-11", "600QA", "HARDSCAPE SCHEDULES", new[] { Hardscape })),
                RunFixture.Batch(new[] { ListOfDrawings }, 1,
                    RunFixture.Row("DM-11", "010QB", "LIST OF DRAWINGS", new[] { ListOfDrawings })),
                RunFixture.Batch(null, 1,
                    RunFixture.Row("DM-11", "010QA", "TITLE SHEET")));

            Assert.Equal(
                new[]
                {
                    "010QA TITLE SHEET",
                    "010QB LIST OF DRAWINGS",
                    "600QA HARDSCAPE SCHEDULES",
                    "600RA HARDSCAPE SCHEDULES"
                },
                plan.Items.Select(one => one.Name).ToArray());
        }

        /// <summary>
        /// The refusal for a row with no number carries the reason line when there is one,
        /// so why the number could not be built is said at Run and not only under the box.
        /// The title sheet holds no views, its number is typed, and until it is the refusal
        /// says exactly that.
        /// </summary>
        [Fact]
        public void ARefusalCarriesWhyTheNumberCouldNotBeBuilt()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-16" },
                RunFixture.Batch(null, 1,
                    new SheetToMake(
                        "DM-16", string.Empty, "TITLE SHEET", null, 1,
                        RunFixture.TitleBlockFamily, RunFixture.TitleBlockType, false, false,
                        SheetNumberRun.NoViewsWords())));

            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("still missing a number", only.Because);
            Assert.Contains(
                "This sheet holds no views, so its code comes from its title block",
                only.Because);
        }

        /// <summary>
        /// An empty sheet the user finished, name and number typed, is a real item. The
        /// writer already places nothing without complaint, so nothing else stands between
        /// the description and the title sheet.
        /// </summary>
        [Fact]
        public void AFinishedEmptySheetIsMadeAndAnUnfinishedOneIsRefused()
        {
            RunPlan made = RunFixture.WithSheets(
                new[] { "FP-39" },
                RunFixture.Batch(null, 1,
                    RunFixture.Row("FP-39", "010001A", "TITLE SHEET")));

            RunItem only = Assert.Single(made.Items);
            Assert.Equal("010001A TITLE SHEET", only.Name);
            Assert.Empty(only.Sheet.Views);

            RunPlan refused = RunFixture.WithSheets(
                new[] { "FP-39" },
                RunFixture.Batch(null, 1,
                    RunFixture.Row("FP-39", "", "TITLE SHEET")));

            Assert.Empty(refused.Items);
            Assert.Single(refused.Refusals);
        }
    }
}
