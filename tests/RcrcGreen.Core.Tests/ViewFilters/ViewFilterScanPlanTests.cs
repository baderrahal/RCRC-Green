using System.Collections.Generic;
using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The scan grid: plots down the side by the run's own plot id rule, a cell per plot
    /// and prefix answering Exists, Will create or Cannot create by the run's own lookup,
    /// and the skipped and blocked views listed rather than silently absent.
    /// </summary>
    public class ViewFilterScanPlanTests
    {
        private static ViewFilterScanResult Planned(
            IReadOnlyList<string> filterNames, params ExemplarState[] exemplars)
        {
            List<ScannedFilterView> views = new List<ScannedFilterView>
            {
                new ScannedFilterView("DM-41-(010) Location Key Plan", false, ""),
                new ScannedFilterView("DM-2-(010) Location Key Plan", false, ""),
                new ScannedFilterView("General Arrangement Layout", false, ""),
                new ScannedFilterView("PF-12-(200) General Arrangement Layout", true, "PRX-Template 200")
            };

            return ViewFilterScanPlan.Of("NG05", views, filterNames, exemplars);
        }

        [Fact]
        public void PlotsComeOffTheViewNamesInNaturalOrder()
        {
            ViewFilterScanResult result = Planned(
                new List<string>(),
                ExemplarState.None("(215) Borders Edging"));

            Assert.Equal(new[] { "DM-2", "DM-41" }, result.Plots);
        }

        [Fact]
        public void AViewWithoutAPlotIdIsListedAsSkipped()
        {
            ViewFilterScanResult result = Planned(
                new List<string>(),
                ExemplarState.None("(215) Borders Edging"));

            Assert.Equal(new[] { "General Arrangement Layout" }, result.SkippedViews);
        }

        /// <summary>
        /// A blocked view puts no plot in the grid, because the run will not touch it. PF-12
        /// appears only in the blocked list, with its template named.
        /// </summary>
        [Fact]
        public void ABlockedViewIsListedWithItsTemplateAndFeedsNoRow()
        {
            ViewFilterScanResult result = Planned(
                new List<string>(),
                ExemplarState.None("(215) Borders Edging"));

            Assert.Single(result.BlockedViews);
            Assert.Equal("PF-12-(200) General Arrangement Layout", result.BlockedViews[0].ViewName);
            Assert.Equal("PRX-Template 200", result.BlockedViews[0].TemplateName);
            Assert.DoesNotContain("PF-12", result.Plots);
        }

        [Fact]
        public void AFilterAlreadyInTheDocumentReadsExists()
        {
            ViewFilterScanResult result = Planned(
                new List<string> { "(215) Borders Edging DM-41" },
                ExemplarState.None("(215) Borders Edging"));

            Assert.Equal(ScanCell.Exists, result.CellAt("DM-41", 0));
            Assert.Equal(ScanCell.CannotCreate, result.CellAt("DM-2", 0));
        }

        [Fact]
        public void AMissingFilterWithAGoodExemplarReadsWillCreate()
        {
            ViewFilterScanResult result = Planned(
                new List<string> { "(215) Borders Edging DM-11" },
                ExemplarState.Of("(215) Borders Edging", "(215) Borders Edging DM-11", true));

            Assert.Equal(ScanCell.WillCreate, result.CellAt("DM-41", 0));
        }

        /// <summary>
        /// The second edit's guards, mirrored: no exemplar, an exemplar with no plot id in
        /// its tail, and an exemplar whose rules cannot be copied all read Cannot create.
        /// </summary>
        [Fact]
        public void EverySecondEditRefusalReadsCannotCreate()
        {
            ViewFilterScanResult noExemplar = Planned(
                new List<string>(),
                ExemplarState.None("(215) Borders Edging"));
            ViewFilterScanResult badTail = Planned(
                new List<string> { "(215) Borders Edging OLD" },
                ExemplarState.Of("(215) Borders Edging", "(215) Borders Edging OLD", true));
            ViewFilterScanResult noRules = Planned(
                new List<string> { "(215) Borders Edging DM-11" },
                ExemplarState.Of("(215) Borders Edging", "(215) Borders Edging DM-11", false));

            Assert.Equal(ScanCell.CannotCreate, noExemplar.CellAt("DM-41", 0));
            Assert.Equal(ScanCell.CannotCreate, badTail.CellAt("DM-41", 0));
            Assert.Equal(ScanCell.CannotCreate, noRules.CellAt("DM-41", 0));
        }

        [Fact]
        public void ThePrefixColumnsKeepTheRowOrder()
        {
            ViewFilterScanResult result = Planned(
                new List<string>(),
                ExemplarState.None("(200-260) Presentation"),
                ExemplarState.None("(215) Borders Edging"));

            Assert.Equal(new[] { "(200-260) Presentation", "(215) Borders Edging" }, result.Prefixes);
        }
    }
}
