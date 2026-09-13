using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class PlotPrefixesTests
    {
        /// <summary>
        /// The eight the team confirmed, each written out by hand. Eight prefixes to seven
        /// templates, because STREETS takes three and MOSQUES takes two.
        /// </summary>
        [Theory]
        [InlineData("NS-19", "STREETS")]
        [InlineData("ST-11", "STREETS")]
        [InlineData("MM-03", "STREETS")]
        [InlineData("PL-17", "PARKING")]
        [InlineData("FM-05", "MOSQUES")]
        [InlineData("DM-12", "MOSQUES")]
        [InlineData("SC-03", "SCHOOLS")]
        [InlineData("EP-05", "EXISTING PARKS")]
        [InlineData("FP-02", "FUTURE PARKS")]
        [InlineData("HF-01", "HEALTHCARE")]
        public void EveryConfirmedPrefixResolves(string plotId, string templateName)
        {
            KpiTemplate found = PlotPrefixes.For(plotId);

            Assert.NotNull(found);
            Assert.Equal(templateName, found.Name);
        }

        [Fact]
        public void EveryTemplateIsReachedByAtLeastOnePrefix()
        {
            Assert.All(KpiTemplates.All, one => Assert.NotEmpty(PlotPrefixes.PrefixesFor(one)));
        }

        /// <summary>
        /// Many to one, said as the lists rather than left to be read off the table.
        /// </summary>
        [Fact]
        public void ThreePrefixesMeanStreetsAndTwoMeanMosques()
        {
            Assert.Equal(new[] { "NS", "ST", "MM" }, PlotPrefixes.PrefixesFor(KpiTemplates.Streets));
            Assert.Equal(new[] { "FM", "DM" }, PlotPrefixes.PrefixesFor(KpiTemplates.Mosques));
            Assert.Equal(new[] { "HF" }, PlotPrefixes.PrefixesFor(KpiTemplates.Healthcare));
        }

        /// <summary>
        /// The two routes agree prefix by prefix with the eleven component values measured on
        /// the 1548 scan, with nothing left over on either side. Written out by hand: this is
        /// the claim the cross check rests on, so it is asserted rather than assumed.
        /// </summary>
        [Fact]
        public void ThePrefixTableAndTheComponentTableNameTheSameSevenTemplates()
        {
            var byPrefix = new HashSet<string>(
                PlotPrefixes.All.Select(one => one.Template.Name));
            var byComponent = new HashSet<string>(
                ComponentTemplates.All.Select(one => one.Template.Name));

            Assert.Equal(7, byPrefix.Count);
            Assert.Equal(7, byComponent.Count);
            Assert.True(byPrefix.SetEquals(byComponent));
        }

        [Fact]
        public void APrefixTheTableDoesNotHoldMeansNoTemplate()
        {
            Assert.Null(PlotPrefixes.For("ZZ-01"));
            Assert.Null(PlotPrefixes.For("12-34"));
            Assert.Null(PlotPrefixes.For("D"));
            Assert.Null(PlotPrefixes.For(null));
            Assert.Null(PlotPrefixes.For("   "));
        }

        [Fact]
        public void ThePrefixIsTheTwoLettersAtTheFront()
        {
            Assert.Equal("DM", PlotPrefixes.Of("DM-12"));
            Assert.Equal("DM", PlotPrefixes.Of("  dm-12  "));
            Assert.Equal(string.Empty, PlotPrefixes.Of("1M-12"));
        }

        [Fact]
        public void AcrossNamesEveryTemplateTheChosenPlotsPointAt()
        {
            IReadOnlyList<TemplateByPrefix> across = PlotPrefixes.Across(
                new[] { "DM-12", "FM-05", "NS-19", "ZZ-01" });

            Assert.Equal(3, across.Count);
            Assert.Equal(new[] { "DM-12", "FM-05" }, across[0].Plots);
            Assert.Equal("MOSQUES", across[0].Name);
            Assert.Equal("STREETS", across[1].Name);
            Assert.Equal("(no prefix this tool knows)", across[2].Name);
        }
    }

    public class TemplateFromTwoRoutesTests
    {
        private static AgreedValue Component(string value, params string[] plots)
        {
            return new AgreedValue(plots.Select(plot => new PlotText(plot, value)));
        }

        /// <summary>
        /// Where the two routes agree the component decides and the prefix is named as agreeing,
        /// so a reader can see the cross check ran.
        /// </summary>
        [Fact]
        public void WhereBothRoutesAgreeTheComponentDecidesAndThePrefixIsNamed()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component("FRIDAY MOSQUE", "DM-12"), new[] { "DM-12" }, null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("MOSQUES", choice.Preselected.Name);
            Assert.Contains("The plot prefix agrees", choice.Why);
        }

        /// <summary>
        /// Where they disagree NEITHER decides. Two records of one fact is the fault this repo
        /// has met eight times, so the pane names both and preselects nothing.
        /// </summary>
        [Fact]
        public void WhereTheRoutesDisagreeNothingIsPreselectedAndBothAreNamed()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component("SCHOOL", "DM-12"), new[] { "DM-12" }, null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains("SCHOOL means SCHOOLS", choice.Why);
            Assert.Contains("the plot prefix says MOSQUES", choice.Why);
        }

        /// <summary>
        /// The 1548 scan found EP-05, EP-11, EP-12 and EP-13 on a schedule and on no sheet. No
        /// sheet means no PRX_Component, so the prefix is the only thing that can place them,
        /// and the pane says which route the answer took rather than preselecting in silence.
        /// </summary>
        [Fact]
        public void WithNoComponentAtAllThePrefixPlacesThePlotAndSaysSo()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component(string.Empty, "EP-05", "EP-11"), new[] { "EP-05", "EP-11" }, null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("EXISTING PARKS", choice.Preselected.Name);
            Assert.Contains(TemplateForComponent.ByThePrefix, choice.Why);
        }

        /// <summary>
        /// No component and prefixes that point two ways is still the user's pick.
        /// </summary>
        [Fact]
        public void WithNoComponentAndPrefixesPointingTwoWaysTheUserPicks()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component(string.Empty, "EP-05", "DM-12"), new[] { "EP-05", "DM-12" }, null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(TemplateForComponent.NoComponent, choice.Why);
        }

        /// <summary>
        /// A plot whose prefix the table does not hold leaves the cross check with no answer,
        /// which is different from an answer that disagrees. The component still decides.
        /// </summary>
        [Fact]
        public void APrefixTheTableDoesNotHoldCrossChecksNothing()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component("SCHOOL", "ZZ-01"), new[] { "ZZ-01" }, null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("SCHOOLS", choice.Preselected.Name);
            Assert.DoesNotContain("plot prefix", choice.Why);
        }

        /// <summary>
        /// A component the table does not hold is still the user's pick, and what the prefix
        /// says is offered as information rather than acted on.
        /// </summary>
        [Fact]
        public void AnUnknownComponentIsStillThePickWithThePrefixNamed()
        {
            TemplateChoice choice = TemplateForComponent.For(
                Component("PUMP STATION", "DM-12"), new[] { "DM-12" }, null);

            Assert.True(choice.NeedsAPick);
            Assert.Contains(TemplateForComponent.NotInTheTable, choice.Why);
            Assert.Contains("The plot prefix says MOSQUES", choice.Why);
        }
    }
}
