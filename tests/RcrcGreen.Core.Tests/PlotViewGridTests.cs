using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class PlotViewGridTests
    {
        private static readonly string[] ModelNames =
        {
            "DM-41-(010) Location Key Plan",
            "DM-41-(010) Overall Key Plan",
            "DM-41-(200) General Arrangement Layout",
            "DM-41-(400) Landscape Cross Section",
            "PF-12-(010) Location Key Plan",
            "PF-12-(200) General Arrangement Layout"
        };

        private static List<ParsedViewName> Parse(params string[] names)
        {
            var parsedNames = new List<ParsedViewName>();
            foreach (string name in names)
            {
                ParsedViewName parsed;
                Assert.True(ViewNameParser.TryParse(name, out parsed), name + " did not parse.");
                parsedNames.Add(parsed);
            }
            return parsedNames;
        }

        private static ViewType Type(string code, string viewName)
        {
            return new ViewType(code, viewName);
        }

        [Fact]
        public void ColumnsAreCodeAndViewNameTogetherSoOneCodeCanHoldTwoTypes()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "DM-41", "PF-12" });

            Assert.Equal(4, grid.ViewTypes.Count);
            Assert.Equal(2, grid.ViewTypes.Count(type => type.Code == "010"));
        }

        [Fact]
        public void APlotWithNoViewsGetsARowAndEveryColumnReadsMissing()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "AB-7", "DM-41", "PF-12" });

            // Spelled out rather than read back off the grid. Comparing the grid against
            // itself passes just as happily when the columns have all gone missing.
            ViewType[] everyKnownType =
            {
                Type("010", "Location Key Plan"),
                Type("010", "Overall Key Plan"),
                Type("200", "General Arrangement Layout"),
                Type("400", "Landscape Cross Section")
            };

            Assert.Contains("AB-7", grid.PlotIds);
            Assert.Equal(everyKnownType, grid.ViewTypes);
            Assert.Equal(everyKnownType, grid.MissingFor("AB-7"));
            Assert.All(everyKnownType, type => Assert.False(grid.IsPresent("AB-7", type)));
        }

        [Fact]
        public void ACellReadsPresentWhenThatPlotHasThatExactViewType()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "DM-41", "PF-12" });

            Assert.True(grid.IsPresent("DM-41", Type("010", "Overall Key Plan")));
            Assert.False(grid.IsPresent("PF-12", Type("010", "Overall Key Plan")));
            Assert.True(grid.IsPresent("PF-12", Type("010", "Location Key Plan")));
        }

        [Fact]
        public void APlotSeenOnlyInAViewNameStillGetsARow()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), Array.Empty<string>());

            Assert.Equal(new[] { "DM-41", "PF-12" }, grid.PlotIds);
        }

        [Fact]
        public void AskingAboutAPlotThatHasNoRowIsAnError()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "DM-41", "PF-12" });

            Assert.Throws<ArgumentException>(() => grid.MissingFor("ZZ-99"));
        }

        [Fact]
        public void APlotMissingATypeEveryOtherPlotHasIsReportedMissing()
        {
            PlotViewGrid grid = PlotViewGrid.Build(
                Parse(
                    "DM-41-(200) General Arrangement Layout",
                    "PF-12-(200) General Arrangement Layout",
                    "AB-7-(010) Location Key Plan"),
                new[] { "AB-7", "DM-41", "PF-12" });

            ViewType generalArrangement = Type("200", "General Arrangement Layout");
            ViewType locationKeyPlan = Type("010", "Location Key Plan");

            Assert.Equal(2, grid.PlotsHolding(generalArrangement));

            // The whole list, so a missing lookup that answers everything is missing fails
            // here. AB-7 holds the key plan, and that is the entry that must not appear.
            Assert.Equal(new[] { generalArrangement }, grid.MissingFor("AB-7"));
            Assert.DoesNotContain(locationKeyPlan, grid.MissingFor("AB-7"));
            Assert.True(grid.IsPresent("AB-7", locationKeyPlan));
        }

        [Fact]
        public void TheMissingListIsOrderedByHowManyPlotsHoldTheType()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "AB-7", "DM-41", "PF-12" });

            PlotMissingViews forAb7 = MissingViewFinder.Find(grid).Single(report => report.PlotId == "AB-7");

            Assert.Equal(
                new[] { 2, 2, 1, 1 },
                forAb7.Missing.Select(entry => entry.PlotsHoldingType));
            Assert.Equal(
                new[]
                {
                    Type("010", "Location Key Plan"),
                    Type("200", "General Arrangement Layout"),
                    Type("010", "Overall Key Plan"),
                    Type("400", "Landscape Cross Section")
                },
                forAb7.Missing.Select(entry => entry.ViewType));
        }

        [Fact]
        public void EveryPlotGetsAReportIncludingTheOneWithNothingMissing()
        {
            PlotViewGrid grid = PlotViewGrid.Build(Parse(ModelNames), new[] { "AB-7", "DM-41", "PF-12" });

            IReadOnlyList<PlotMissingViews> reports = MissingViewFinder.Find(grid);

            Assert.Equal(new[] { "AB-7", "DM-41", "PF-12" }, reports.Select(report => report.PlotId));
            Assert.True(reports.Single(report => report.PlotId == "DM-41").IsComplete);
            Assert.Equal(
                new[] { Type("010", "Overall Key Plan"), Type("400", "Landscape Cross Section") },
                reports.Single(report => report.PlotId == "PF-12").Missing.Select(entry => entry.ViewType));
        }

        [Fact]
        public void RowsAndColumnsPutTheNumberPartInNumberOrder()
        {
            PlotViewGrid grid = PlotViewGrid.Build(
                Parse(
                    "DM-100-(1000) Long Section",
                    "DM-2-(200) General Arrangement Layout",
                    "DM-9-(90) Setting Out"),
                new[] { "DM-100", "DM-2", "DM-9" });

            Assert.Equal(new[] { "DM-2", "DM-9", "DM-100" }, grid.PlotIds);
            Assert.Equal(
                new[] { "90", "200", "1000" },
                grid.ViewTypes.Select(type => type.Code));
        }

        [Fact]
        public void AGridWithNoNamesAtAllHasRowsAndNoColumns()
        {
            PlotViewGrid grid = PlotViewGrid.Build(
                Array.Empty<ParsedViewName>(),
                new[] { "AB-7", "DM-41" });

            Assert.Equal(new[] { "AB-7", "DM-41" }, grid.PlotIds);
            Assert.Empty(grid.ViewTypes);
            Assert.All(MissingViewFinder.Find(grid), report => Assert.True(report.IsComplete));
        }
    }
}
