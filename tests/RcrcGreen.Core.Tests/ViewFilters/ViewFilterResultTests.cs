using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// What the RESULTS step holds after a run. The count of things not applied is the
    /// failure list's own length, never a second number kept beside it, and the eight
    /// count lines are the results block's, unchanged by the rail.
    /// </summary>
    public class ViewFilterResultTests
    {
        private static Output Counted()
        {
            return new Output
            {
                ViewsEvaluated = 42,
                ViewsModified = 7,
                FiltersAdded = 3,
                FiltersConfigured = 12,
                FiltersCreatedInDoc = 2,
                FiltersNotFoundInDoc = 4,
                Skipped = 5,
                Blocked = 6
            };
        }

        [Fact]
        public void NotAppliedIsTheFailureListsOwnLength()
        {
            Output failed = Counted();
            failed.Failures = new[]
            {
                new ViewFilterFailure("(200-260) Presentation DM-41", "Why one"),
                new ViewFilterFailure("DM-11-(010) Location Key Plan", "Why two"),
                new ViewFilterFailure("(215) Borders Edging DM-12", "Why three")
            };

            ViewFilterResult with = ViewFilterResult.Of(failed, string.Empty);
            Assert.Equal(3, with.NotApplied);
            Assert.Equal(with.Failures.Length, with.NotApplied);

            Output clean = Counted();
            clean.Failures = new ViewFilterFailure[0];

            ViewFilterResult without = ViewFilterResult.Of(clean, string.Empty);
            Assert.Equal(0, without.NotApplied);
            Assert.Equal(without.Failures.Length, without.NotApplied);
        }

        /// <summary>
        /// A run built before the failure list existed hands back no array at all, and
        /// that reads as no failures rather than as a throw.
        /// </summary>
        [Fact]
        public void NoFailureArrayAtAllReadsAsNoFailures()
        {
            ViewFilterResult result = ViewFilterResult.Of(Counted(), string.Empty);

            Assert.Equal(0, result.NotApplied);
            Assert.Empty(result.Failures);
        }

        [Fact]
        public void AppliedIsTheRunsOwnConfiguredCount()
        {
            Assert.Equal(12, ViewFilterResult.Of(Counted(), string.Empty).Applied);
        }

        /// <summary>
        /// The eight count lines, written out by hand, character for character. The rail
        /// moved the results into step 5 and changed no wording, which is what this pins.
        /// </summary>
        [Fact]
        public void TheEightCountLinesAreUnchangedByTheRail()
        {
            ViewFilterResult result = ViewFilterResult.Of(Counted(), string.Empty);

            Assert.Equal(
                new[]
                {
                    "Views evaluated 42",
                    "Views modified 7",
                    "Filters added to views 3",
                    "Filters configured 12",
                    "Filters created in the document 2",
                    "Filter lookups with nothing to use 4",
                    "Views skipped, no plot id 5",
                    "Views blocked by their template 6"
                },
                result.Lines);
        }

        [Fact]
        public void HasReportReadsOffTheReportPath()
        {
            Assert.False(ViewFilterResult.Of(Counted(), string.Empty).HasReport);
            Assert.False(ViewFilterResult.Of(Counted(), null).HasReport);

            ViewFilterResult with = ViewFilterResult.Of(
                Counted(),
                "C:\\repo\\reports\\RCRC-Green-ViewFilters_Model_2026-09-19_10-00-00.txt");
            Assert.True(with.HasReport);
            Assert.Equal(
                "C:\\repo\\reports\\RCRC-Green-ViewFilters_Model_2026-09-19_10-00-00.txt",
                with.ReportPlace);
        }

        /// <summary>
        /// The failure line is the run's own sentence, handed through whole, so the log
        /// and the result panel can never word one failure two ways.
        /// </summary>
        [Fact]
        public void AFailureKeepsTheRunnersOwnSentence()
        {
            Output output = Counted();
            output.Failures = new[]
            {
                new ViewFilterFailure(
                    "DM-41-(010) Location Key Plan",
                    "Skipped 'DM-41-(010) Location Key Plan', its name does not start with a plot id.")
            };

            ViewFilterFailure kept = ViewFilterResult.Of(output, string.Empty).Failures[0];
            Assert.Equal("DM-41-(010) Location Key Plan", kept.What);
            Assert.Equal(
                "Skipped 'DM-41-(010) Location Key Plan', its name does not start with a plot id.",
                kept.Why);
        }

        [Fact]
        public void AFailureBuiltOnNothingSaysNothingRatherThanThrowing()
        {
            ViewFilterFailure empty = new ViewFilterFailure(null, null);

            Assert.Equal(string.Empty, empty.What);
            Assert.Equal(string.Empty, empty.Why);
        }
    }
}
