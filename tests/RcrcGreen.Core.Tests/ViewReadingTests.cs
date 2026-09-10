using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ViewReadingTests
    {
        /// <summary>
        /// The fault the user hit. Every DM-11 view was deleted and the grid still showed the
        /// DM-11 row with views in it, because a view named for DM-12 carrying PRX_Plot_ID
        /// DM-11 was filed under DM-11.
        /// </summary>
        [Fact]
        public void AViewFillsTheCellOfThePlotInItsOwnNameNotTheOneInTheParameter()
        {
            ViewOnAPlot read = ViewReading.Read("DM-11", "DM-12-(200) General Arrangement Layout", 91);

            Assert.Equal("DM-12", read.Fills.Where.PlotId);
            Assert.Equal("200", read.Fills.Where.ViewType.Code);
            Assert.Equal("General Arrangement Layout", read.Fills.Where.ViewType.ViewName);
            Assert.Equal(91, read.Fills.ViewId);
        }

        [Fact]
        public void ThatDisagreementIsCountedRatherThanHidden()
        {
            ViewOnAPlot read = ViewReading.Read("DM-11", "DM-12-(200) General Arrangement Layout", 91);

            Assert.True(read.SourcesDisagree);
        }

        [Fact]
        public void TheRowPlotStillComesFromTheParameterWhenTheTwoDisagree()
        {
            ViewOnAPlot read = ViewReading.Read("DM-11", "DM-12-(200) General Arrangement Layout", 91);

            Assert.Equal("DM-11", read.PlotId);
            Assert.Equal(PlotSourceOnView.Parameter, read.Source);
        }

        [Fact]
        public void TwoSourcesThatAgreeAreNotADisagreement()
        {
            ViewOnAPlot read = ViewReading.Read("DM-41", "DM-41-(010) Location Key Plan", 4);

            Assert.False(read.SourcesDisagree);
            Assert.Equal("DM-41", read.PlotId);
            Assert.Equal("DM-41", read.Fills.Where.PlotId);
        }

        /// <summary>
        /// The 1,269 views on the real model that Scope Box skipped. The parameter gives the
        /// plot so the row appears, and the name gives no view type so no cell is filled.
        /// </summary>
        [Fact]
        public void ANameThatDoesNotParseGivesARowAndFillsNoCell()
        {
            ViewOnAPlot read = ViewReading.Read("DM-11", "Site Plan Working", 7);

            Assert.Equal("DM-11", read.PlotId);
            Assert.Null(read.Fills);
            Assert.False(read.SourcesDisagree);
        }

        [Fact]
        public void AParsingNameWithNoParameterFillsItsOwnCell()
        {
            ViewOnAPlot read = ViewReading.Read(null, "PF-12-(200) General Arrangement Layout", 12);

            Assert.Equal("PF-12", read.PlotId);
            Assert.Equal(PlotSourceOnView.ViewName, read.Source);
            Assert.Equal("PF-12", read.Fills.Where.PlotId);
            Assert.False(read.SourcesDisagree);
        }

        /// <summary>
        /// A parameter holding something that is not a plot leaves the view with no row plot,
        /// which is the answer ViewPlotReader already gave. The cell is still filled from the
        /// name, because the name is true whatever the parameter says.
        /// </summary>
        [Fact]
        public void AParameterThatIsNotAPlotStillLeavesTheNameFillingItsCell()
        {
            ViewOnAPlot read = ViewReading.Read("N/A", "DM-41-(400) Landscape Cross Section", 5);

            Assert.Equal(PlotSourceOnView.ParameterNotAPlot, read.Source);
            Assert.False(read.Found);
            Assert.Equal("DM-41", read.Fills.Where.PlotId);
            Assert.False(read.SourcesDisagree);

            // Kept on the reading so the status line can show the value, not only count it.
            Assert.Equal("N/A", read.RawParameterValue);
        }

        [Fact]
        public void NeitherSourceGivingAnythingFillsNothingAndClaimsNothing()
        {
            ViewOnAPlot read = ViewReading.Read(null, "Level 1", 3);

            Assert.False(read.Found);
            Assert.Null(read.Fills);
            Assert.Equal(PlotSourceOnView.None, read.Source);
        }

        [Fact]
        public void CaseOnThePlotInTheNameIsNotAPlot()
        {
            ViewOnAPlot read = ViewReading.Read(null, "dm-41-(010) Location Key Plan", 8);

            Assert.False(read.Found);
            Assert.Null(read.Fills);
        }
    }
}
