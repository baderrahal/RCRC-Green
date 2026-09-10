using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The escape itself is in Shared with its own tests. This is the KPI pane's use of it,
    /// over the KPI pane's names.
    /// </summary>
    public class KpiPaneLabelTests
    {
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
