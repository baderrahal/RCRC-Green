using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The WPF access key escape, moved from the KPI tests with its class. The one test that
    /// walks every KPI name stays with the KPI tests, because the names are KPI's.
    /// </summary>
    public class PaneLabelTests
    {
        /// <summary>
        /// The five the KPI pane really printed. Every one of them is a name no model holds,
        /// and both tools turn on exact parameter names.
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
            Assert.Equal("Select all", PaneLabel.Escaped("Select all"));
            Assert.Equal("Clear", PaneLabel.Escaped("Clear"));
            Assert.Equal("Create", PaneLabel.Escaped("Create"));
            Assert.Equal("EXISTING PARKS", PaneLabel.Escaped("EXISTING PARKS"));
        }

        [Fact]
        public void NothingIsAnEmptyStringRatherThanAThrow()
        {
            Assert.Equal(string.Empty, PaneLabel.Escaped(null));
            Assert.Equal(string.Empty, PaneLabel.Escaped(string.Empty));
        }
    }
}
