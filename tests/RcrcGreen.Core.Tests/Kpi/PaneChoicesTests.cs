using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class PaneLabelTests
    {
        /// <summary>
        /// The five the pane really printed. Every one of them is a name no model holds, and
        /// the whole tool turns on exact parameter names.
        /// </summary>
        [Theory]
        [InlineData("PRX_Component", "PRX__Component")]
        [InlineData("PRX_Plot_ID", "PRX__Plot__ID")]
        [InlineData("PRX_Plot_UID", "PRX__Plot__UID")]
        [InlineData("PRX_Plot_UID2", "PRX__Plot__UID2")]
        [InlineData("PRX_Plot_NH", "PRX__Plot__NH")]
        public void EveryUnderscoreIsDoubledSoWpfShowsIt(string name, string onScreen)
        {
            Assert.Equal(onScreen, PaneLabel.Escaped(name));
        }

        /// <summary>
        /// Written out by hand rather than worked out with the escape's own rule: what WPF
        /// renders is the escaped text with each doubled underscore read back as one.
        /// </summary>
        [Theory]
        [InlineData("PRX_Component")]
        [InlineData("PRX_Ref Plot ID")]
        [InlineData("PRX_Intervention Area")]
        [InlineData("RCRC_NG05_NU_MAIN_RVT24_00.rvt")]
        [InlineData("Neighborhood Name")]
        public void WhatWpfRendersIsTheNameItself(string name)
        {
            Assert.Equal(name, PaneLabel.Escaped(name).Replace("__", "_"));
        }

        [Fact]
        public void TextWithNoUnderscoreIsUntouched()
        {
            Assert.Equal("KPI Scan", PaneLabel.Escaped("KPI Scan"));
            Assert.Equal("Select all", PaneLabel.Escaped(CreateWords.SelectAll));
            Assert.Equal("Clear", PaneLabel.Escaped(CreateWords.Clear));
            Assert.Equal("Create", PaneLabel.Escaped(CreateWords.Create));
            Assert.Equal("EXISTING PARKS", PaneLabel.Escaped(KpiTemplates.ExistingParks.Name));
        }

        [Fact]
        public void NothingIsAnEmptyStringRatherThanAThrow()
        {
            Assert.Equal(string.Empty, PaneLabel.Escaped(null));
            Assert.Equal(string.Empty, PaneLabel.Escaped(string.Empty));
        }

        /// <summary>
        /// Every parameter name the KPI pane can put on a button comes back readable. A name
        /// this misses is a name the pane shows wrongly, so the list is the whole of KpiNames
        /// rather than the five that were noticed.
        /// </summary>
        [Fact]
        public void EveryNameThePaneCanShowSurvivesTheEscape()
        {
            var everyName = new List<string>(KpiNames.PlotNamesOnSheets)
            {
                KpiNames.Component,
                KpiNames.RefPlotId,
                KpiNames.InterventionArea,
                KpiNames.NeighbourhoodName
            };

            Assert.All(everyName, name => Assert.Equal(name, PaneLabel.Escaped(name).Replace("__", "_")));
        }
    }

    public class PreselectedTests
    {
        /// <summary>
        /// Reference showed PRX_Plot_ID, which is first in the list and is not what the workbook
        /// note names. The note names PRX_Plot_UID2 and the model offers all four.
        /// </summary>
        [Fact]
        public void ReferenceStartsOnTheParameterTheNoteNamesAndNotTheFirstOffered()
        {
            Assert.Equal(
                KpiNames.PlotUid2,
                Preselected.From(KpiNames.PlotNamesOnSheets.ToList(), KpiNames.PlotUid2));

            Assert.Equal(KpiNames.PlotId, KpiNames.PlotNamesOnSheets[0]);
        }

        /// <summary>
        /// Neighborhood Group sorts before Neighborhood Name and holds GROUP 5. The location is
        /// the name.
        /// </summary>
        [Fact]
        public void LocationStartsOnNeighborhoodNameAndNotOnTheGroup()
        {
            var offered = new List<string> { "Neighborhood Group", "Neighborhood Name" };

            Assert.Equal("Neighborhood Name", Preselected.From(offered, KpiNames.NeighbourhoodName));
        }

        [Fact]
        public void AWantedNameTheModelDoesNotOfferFallsBackToTheFirst()
        {
            var offered = new List<string> { "PRX_Component", "KPI COMPONENT S/H" };

            Assert.Equal("PRX_Component", Preselected.From(offered, KpiNames.Component));
        }

        /// <summary>
        /// The comparison is the whole name with its case, the same plainness everything else
        /// here keeps. A model spelling it differently is offering a different parameter.
        /// </summary>
        [Fact]
        public void TheNameIsMatchedWholeAndWithItsCase()
        {
            var offered = new List<string> { "PRX_Plot_ID", "prx_plot_uid2" };

            Assert.Equal("PRX_Plot_ID", Preselected.From(offered, KpiNames.PlotUid2));
        }

        [Fact]
        public void NothingOfferedPreselectsNothing()
        {
            Assert.Equal(string.Empty, Preselected.From(new List<string>(), KpiNames.PlotUid2));
            Assert.Equal(string.Empty, Preselected.From(null, KpiNames.PlotUid2));
        }
    }

    public class PlotTicksStartTests
    {
        private static PlotsInTheModel OneHundredAndFiftyFive()
        {
            var plots = new List<string>();
            for (int at = 1; at <= 155; at++) plots.Add("DM-" + at);

            return PlotsInTheModel.Of(plots, plots);
        }

        /// <summary>
        /// Nothing is ticked to begin with. Adding every plot in the model into one workbook is
        /// one press of Select all, and it is almost never what anyone wants.
        /// </summary>
        [Fact]
        public void AFreshPickerHasNothingTicked()
        {
            var ticks = new PlotTicks(OneHundredAndFiftyFive());

            Assert.Equal(0, ticks.Count);
            Assert.Empty(ticks.Ticked);
            Assert.Equal("0 of 155 plots ticked.", ticks.InWords);
        }

        /// <summary>
        /// The pane rebuilds the picker when the model is read, carrying the ticks across. From
        /// nothing that has to stay nothing, which is the path a freshly opened pane takes.
        /// </summary>
        [Fact]
        public void ReadingTheModelCarriesNothingAcrossAndTicksNothing()
        {
            var before = new PlotTicks(PlotsInTheModel.Of(null, null));
            var after = new PlotTicks(OneHundredAndFiftyFive(), before.Ticked);

            Assert.Equal(0, after.Count);
            Assert.Equal("0 of 155 plots ticked.", after.InWords);
        }

        [Fact]
        public void SelectAllIsTheOnlyWayToGetAllOfThem()
        {
            var ticks = new PlotTicks(OneHundredAndFiftyFive());

            Assert.Equal(155, ticks.All().Count);
            Assert.Equal(0, ticks.All().None().Count);
        }
    }
}
