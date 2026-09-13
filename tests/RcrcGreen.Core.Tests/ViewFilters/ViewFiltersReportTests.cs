using System;
using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// The report one Apply writes: the counts, the run's own summary, then every logged
    /// line, so the file and the pane's log list are one record.
    /// </summary>
    public class ViewFiltersReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 13, 14, 5, 0);

        [Fact]
        public void TheReportCarriesTheCountsTheSummaryAndEveryLoggedLine()
        {
            Output output = new Output
            {
                ViewsEvaluated = 2,
                ViewsModified = 1,
                Logs = new[] { "Evaluated 2 views." }
            };

            string report = ViewFiltersReport.Write(
                output,
                new[] { "Created missing filter '(215) Borders Edging DM-41' in document." },
                "NG05",
                Noon);

            Assert.Contains("RCRC GREEN VIEW FILTERS RUN", report, StringComparison.Ordinal);
            Assert.Contains("Model: NG05", report, StringComparison.Ordinal);
            Assert.Contains("Written: 2026-09-13 14:05", report, StringComparison.Ordinal);
            Assert.Contains("Views evaluated 2", report, StringComparison.Ordinal);
            Assert.Contains("Evaluated 2 views.", report, StringComparison.Ordinal);
            Assert.Contains(
                "Created missing filter '(215) Borders Edging DM-41' in document.",
                report,
                StringComparison.Ordinal);
        }

        [Fact]
        public void ARunThatLoggedNothingSaysSoRatherThanEndingEmpty()
        {
            string report = ViewFiltersReport.Write(new Output(), null, "NG05", Noon);

            Assert.Contains("Nothing was logged.", report, StringComparison.Ordinal);
        }

        [Fact]
        public void TheFileNameCarriesThePrefixTheTitleAndTheMinute()
        {
            Assert.Equal(
                "RCRC-Green-ViewFilters_NG05_2026-09-13_1405.txt",
                ViewFiltersFileName.For("NG05", Noon));
        }
    }
}
