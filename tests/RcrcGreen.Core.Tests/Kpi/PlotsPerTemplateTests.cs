using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Which ticked plot belongs to which ticked template. Every expected value here is written
    /// out by hand rather than worked out with the same rule the code uses.
    ///
    /// The components are the measured values off the 1548 scan, the prefixes the table the team
    /// confirmed: FM and DM are mosques, SC is schools, NS, ST and MM are streets, EP existing
    /// parks.
    /// </summary>
    public class PlotsPerTemplateTests
    {
        private static Func<string, string> Components(params string[] pairs)
        {
            var held = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int at = 0; at + 1 < pairs.Length; at += 2) held[pairs[at]] = pairs[at + 1];

            return plotId =>
            {
                string component;
                return held.TryGetValue(plotId, out component) ? component : string.Empty;
            };
        }

        /// <summary>
        /// The ordinary case, and the one the round is for: twenty mosque plots and seventy
        /// eight street plots ticked together come out as two workbooks with nothing shared.
        /// </summary>
        [Fact]
        public void TwoTemplatesTickedTogetherSplitIntoTwoWorkbooks()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-12", "FM-05", "ST-05", "NS-19" },
                Components(
                    "DM-12", "DAILY MOSQUE",
                    "FM-05", "FRIDAY MOSQUE",
                    "ST-05", "STREET 36m ROW",
                    "NS-19", "NH STRT 20m ROW"),
                new[] { KpiTemplates.Mosques, KpiTemplates.Streets });

            Assert.True(split.AddsUp);
            Assert.Equal(2, split.TemplatesTicked);
            Assert.Equal(2, split.TemplatesWithPlots);
            Assert.Equal(0, split.TemplatesWithNothing);
            Assert.Empty(split.Unplaced);

            TemplateShare mosques = split.Shares.Single(one => one.Template.Name == "MOSQUES");
            TemplateShare streets = split.Shares.Single(one => one.Template.Name == "STREETS");

            Assert.Equal(new[] { "DM-12", "FM-05" }, mosques.Plots.ToArray());
            Assert.Equal(new[] { "ST-05", "NS-19" }, streets.Plots.ToArray());
        }

        /// <summary>
        /// A ticked template no ticked plot belongs to writes nothing, says so, and STAYS in
        /// the list. Bader's decision: it is not hidden and not unticked.
        /// </summary>
        [Fact]
        public void ATickedTemplateWithNoPlotOfItsOwnStaysListedAndSaysWhy()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-12" },
                Components("DM-12", "DAILY MOSQUE"),
                new[] { KpiTemplates.Mosques, KpiTemplates.Schools });

            Assert.True(split.AddsUp);
            Assert.Equal(2, split.TemplatesTicked);
            Assert.Equal(1, split.TemplatesWithPlots);
            Assert.Equal(1, split.TemplatesWithNothing);

            TemplateShare schools = split.Shares.Single(one => one.Template.Name == "SCHOOLS");
            Assert.False(schools.WillWrite);
            Assert.Equal(
                "no ticked plot belongs to SCHOOLS, so it will write nothing. "
                + "Tick a plot of it, or leave it and it stays listed writing nothing.",
                schools.WhyNothing);
        }

        /// <summary>
        /// A plot whose template is not ticked is not read and is NAMED, which is the half of
        /// the accounting that stops a plot list going in longer than it comes out.
        /// </summary>
        [Fact]
        public void APlotWhoseTemplateIsNotTickedIsNamedAndNotRead()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-12", "SC-03" },
                Components("DM-12", "DAILY MOSQUE", "SC-03", "SCHOOL"),
                new[] { KpiTemplates.Mosques });

            Assert.Equal(1, split.TemplatesTicked);
            PlotTemplate left = Assert.Single(split.Unplaced);
            Assert.Equal("SC-03", left.PlotId);
            Assert.Equal(
                "SCHOOL placed it in SCHOOLS, and the plot prefix agrees, "
                + "and SCHOOLS is not ticked, so it was not read",
                left.Why);

            Assert.Equal(new[] { "DM-12" },
                split.Shares.Single().Plots.ToArray());
        }

        /// <summary>
        /// PRX_Component decides. A plot with no sheet has none, and then the PREFIX is the only
        /// thing that can place it, which is what EP-05, EP-11, EP-12 and EP-13 are.
        /// </summary>
        [Fact]
        public void APlotWithNoComponentIsPlacedByItsPrefixAndTheRouteIsSaid()
        {
            PlotTemplate answer = PlotsPerTemplate.For("EP-05", string.Empty);

            Assert.Equal(KpiTemplates.ExistingParks, answer.Template);
            Assert.Equal(TemplateRoute.Prefix, answer.Route);
            Assert.Equal(
                "no component is on its sheets, so the PLOT PREFIX placed it in EXISTING PARKS",
                answer.Why);
        }

        /// <summary>
        /// Where the two routes disagree NEITHER of them decides, which is the rule this file
        /// already carried for preselecting a template and is now what places a plot.
        /// </summary>
        [Fact]
        public void WhereTheComponentAndThePrefixDisagreeNeitherPlacesThePlot()
        {
            PlotTemplate answer = PlotsPerTemplate.For("DM-12", "SCHOOL");

            Assert.Null(answer.Template);
            Assert.Equal(TemplateRoute.Disagree, answer.Route);
            Assert.Equal(
                "SCHOOL means SCHOOLS and its prefix says MOSQUES, so NEITHER placed it",
                answer.Why);
        }

        /// <summary>
        /// A component value the table does not hold places nothing, and the prefix does not
        /// answer in its place. The route that decides gave an answer nobody knows, so a plot
        /// going into a workbook on the cross check alone would be a guess.
        /// </summary>
        [Fact]
        public void AComponentTheTableDoesNotHoldPlacesNothingAndThePrefixDoesNotStandIn()
        {
            PlotTemplate answer = PlotsPerTemplate.For("DM-12", "MARKET");

            Assert.Null(answer.Template);
            Assert.Equal(TemplateRoute.Nothing, answer.Route);
            Assert.Equal(
                "MARKET is not one of the component values this tool knows, so the component placed "
                + "it nowhere and the prefix does not answer in its place. Its prefix says MOSQUES",
                answer.Why);
        }

        /// <summary>
        /// No component and no prefix the table holds is a plot nothing can place, and it is
        /// named rather than dropped.
        /// </summary>
        [Fact]
        public void APlotNeitherRouteCanPlaceIsNamed()
        {
            PlotTemplate answer = PlotsPerTemplate.For("ZZ-01", string.Empty);

            Assert.Null(answer.Template);
            Assert.Equal(TemplateRoute.Nothing, answer.Route);
            Assert.Equal(
                "no component is on its sheets and its prefix names no template this tool knows",
                answer.Why);
        }

        /// <summary>
        /// The check that cannot be reasoned away. A plot in two shares refuses the whole run
        /// with both templates named, whatever built the shares.
        /// </summary>
        [Fact]
        public void APlotInTwoWorkbooksRefusesTheWholeRunAndNamesBoth()
        {
            var split = new TemplateSplit(
                new[]
                {
                    new TemplateShare(KpiTemplates.Mosques, new[] { "DM-12", "FM-05" }, string.Empty),
                    new TemplateShare(KpiTemplates.Streets, new[] { "DM-12", "ST-05" }, string.Empty)
                },
                new List<PlotTemplate>(),
                new List<PlotTemplate>());

            Assert.False(split.AddsUp);
            Assert.Equal(new[] { "DM-12" }, split.CountedTwice.ToArray());
            Assert.Equal(
                "DM-12 was counted into more than one workbook, MOSQUES and STREETS. "
                + "A plot has one template and one only, so nothing was written.",
                Assert.Single(split.Refusals));
        }

        /// <summary>
        /// The split the code really builds can never do that, and this is what says so, because
        /// the check above proves the guard and this proves the rule under it.
        /// </summary>
        [Fact]
        public void TheSplitItselfPutsNoPlotInTwoWorkbooks()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "DM-12", "FM-05", "ST-05", "SC-03", "EP-05" },
                Components(
                    "DM-12", "DAILY MOSQUE",
                    "FM-05", "FRIDAY MOSQUE",
                    "ST-05", "STREET 36m ROW",
                    "SC-03", "SCHOOL"),
                KpiTemplates.All);

            Assert.True(split.AddsUp);
            Assert.Empty(split.CountedTwice);
            Assert.Equal(7, split.TemplatesTicked);
            Assert.Equal(4, split.TemplatesWithPlots);
            Assert.Equal(3, split.TemplatesWithNothing);
            Assert.Empty(split.Unplaced);

            // Five plots ticked, five plots placed, no plot in two lists.
            Assert.Equal(5, split.Shares.Sum(one => one.Plots.Count));
        }

        /// <summary>
        /// The shares come out in the order KpiTemplates lists them rather than in the order the
        /// user ticked, so two runs over the same ticks print the same report.
        /// </summary>
        [Fact]
        public void TheSharesComeOutInTheTemplateListsOwnOrder()
        {
            TemplateSplit split = PlotsPerTemplate.Split(
                new[] { "ST-05", "DM-12" },
                Components("ST-05", "STREET 36m ROW", "DM-12", "DAILY MOSQUE"),
                new[] { KpiTemplates.Streets, KpiTemplates.Mosques });

            Assert.Equal(
                KpiTemplates.All.Where(one => one.Name == "MOSQUES" || one.Name == "STREETS")
                    .Select(one => one.Name).ToArray(),
                split.Shares.Select(one => one.Template.Name).ToArray());
        }
    }
}
