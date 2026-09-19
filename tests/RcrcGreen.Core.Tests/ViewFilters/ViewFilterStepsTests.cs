using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.ViewFilters;
using Xunit;

namespace RcrcGreen.Core.Tests.ViewFilters
{
    /// <summary>
    /// What each rail step says while it is shut, and whether it can be used at all.
    ///
    /// Every expected string is written out by hand. Working one out with the same rule the
    /// code uses would only prove the rule agrees with itself.
    /// </summary>
    public class ViewFilterStepsTests
    {
        private const string TwoKeywords = "Location Key Plan\nOverall Key Plan";

        private static Inputs Boxes(string keywords, params string[] prefixes)
        {
            return new Inputs
            {
                ViewKeywords = keywords,
                Filters = prefixes.Select(prefix => new FilterConfig
                {
                    Prefix = prefix,
                    Enabled = true,
                    Visible = true
                }).ToArray()
            };
        }

        private static ViewFilterScanResult Read(int plots, int skipped, int blocked)
        {
            return new ViewFilterScanResult(
                "RCRC_NG05",
                Enumerable.Range(11, plots).Select(at => "DM-" + at).ToList(),
                new List<string>(),
                new Dictionary<string, IReadOnlyList<ScanCell>>(),
                Enumerable.Range(0, skipped).Select(at => "Skipped view " + at).ToList(),
                Enumerable.Range(0, blocked)
                    .Select(at => new BlockedView("Blocked view " + at, "Template " + at))
                    .ToList(),
                plots + skipped + blocked);
        }

        private static ViewFilterResult Run(int applied, int failures)
        {
            return ViewFilterResult.Of(
                new Output
                {
                    FiltersConfigured = applied,
                    Failures = Enumerable.Range(0, failures)
                        .Select(at => new ViewFilterFailure("Thing " + at, "Why " + at))
                        .ToArray()
                },
                string.Empty);
        }

        /// <summary>
        /// A pane that has the default keywords, one row with a prefix, and has pressed
        /// nothing yet. Each test moves one thing off this.
        /// </summary>
        private static ViewFilterSteps After(
            Inputs current = null,
            Inputs scanned = null,
            ViewFilterScanResult scan = null,
            ViewFilterResult run = null)
        {
            return ViewFilterSteps.Of(
                current ?? Boxes(TwoKeywords, "(200-260) Presentation"),
                scanned,
                scan,
                run);
        }

        [Fact]
        public void TheFiveStepsComeBackNumberedInOrder()
        {
            ViewFilterSteps steps = After();

            Assert.Equal(
                new[] { 1, 2, 3, 4, 5 },
                steps.All.Select(one => one.Number).ToArray());

            Assert.Equal(
                new[] { "KEYWORDS", "FILTER ROWS", "SCAN", "APPLY", "RESULTS" },
                steps.All.Select(one => one.Title).ToArray());
        }

        [Fact]
        public void NothingTypedAtAllLeavesOnlyStepOneUsable()
        {
            ViewFilterSteps steps = After(Boxes(string.Empty));

            ViewFilterStepState keywords = steps.For(ViewFilterStep.Keywords);
            Assert.True(keywords.Usable);
            Assert.False(keywords.Done);
            Assert.Equal(string.Empty, keywords.Summary);
            Assert.Equal("1  KEYWORDS", keywords.Header);

            Assert.False(steps.For(ViewFilterStep.Rows).Usable);
            Assert.False(steps.For(ViewFilterStep.Scan).Usable);
            Assert.False(steps.For(ViewFilterStep.Apply).Usable);
            Assert.False(steps.For(ViewFilterStep.Results).Usable);

            Assert.Equal(ViewFilterStep.Keywords, steps.FirstUnfinished);
        }

        /// <summary>
        /// A step greyed out with no reason is worse than one that is not there at all, so
        /// every unusable step has to carry a line saying why, and a usable one none.
        /// </summary>
        [Fact]
        public void EveryUnusableStepSaysWhyAndEveryUsableOneDoesNot()
        {
            foreach (ViewFilterStepState step in After(Boxes(string.Empty)).All)
            {
                if (step.Usable) Assert.Equal(string.Empty, step.WhyNot);
                else Assert.NotEqual(string.Empty, step.WhyNot);
            }

            Inputs same = Boxes(TwoKeywords, "(200-260) Presentation");
            foreach (ViewFilterStepState step in After(
                same, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0),
                Run(4, 0)).All)
            {
                Assert.True(step.Usable);
                Assert.Equal(string.Empty, step.WhyNot);
            }
        }

        [Fact]
        public void TheShutStepsNameTheStepThatOpensThem()
        {
            ViewFilterSteps steps = After(Boxes(string.Empty));

            Assert.Equal(
                "Type at least one keyword in step 1.",
                steps.For(ViewFilterStep.Rows).WhyNot);
            Assert.Equal(
                "Give at least one row a prefix in step 2.",
                steps.For(ViewFilterStep.Scan).WhyNot);
            Assert.Equal(
                "Scan first, so what Apply will do has been seen.",
                steps.For(ViewFilterStep.Apply).WhyNot);
            Assert.Equal(
                "Nothing has run yet.",
                steps.For(ViewFilterStep.Results).WhyNot);
        }

        [Fact]
        public void TheKeywordHeaderCountsWhatTheBoxHolds()
        {
            Assert.Equal(
                "1  KEYWORDS   2 keywords",
                After().For(ViewFilterStep.Keywords).Header);

            Assert.Equal(
                "1  KEYWORDS   1 keyword",
                After(Boxes("Location Key Plan", "(215) Borders Edging"))
                    .For(ViewFilterStep.Keywords).Header);
        }

        [Fact]
        public void TheRowsHeaderCountsTheRowsAndThePrefixes()
        {
            Assert.Equal(
                "2  FILTER ROWS   1 row, 1 with a prefix",
                After().For(ViewFilterStep.Rows).Header);

            Assert.Equal(
                "2  FILTER ROWS   2 rows, 1 with a prefix",
                After(Boxes(TwoKeywords, "(215) Borders Edging", "  "))
                    .For(ViewFilterStep.Rows).Header);

            Assert.Equal(
                "2  FILTER ROWS   1 row, none with a prefix",
                After(Boxes(TwoKeywords, string.Empty)).For(ViewFilterStep.Rows).Header);
        }

        /// <summary>
        /// A row whose prefix is only spaces counts as none, because FilterRows.Kept is
        /// what decides a prefix and it drops a blank one. The rule is called, not copied.
        /// </summary>
        [Fact]
        public void ARowOfSpacesDoesNotCountAsAPrefix()
        {
            ViewFilterSteps steps = After(Boxes(TwoKeywords, "   "));

            Assert.False(steps.For(ViewFilterStep.Rows).Done);
            Assert.False(steps.For(ViewFilterStep.Scan).Usable);
        }

        /// <summary>
        /// FilterRows.Kept trims the rows it is handed, so ViewFilterSteps.Of must run it
        /// over a copy. Trimming the pane's own record is the fault that jammed Apply on
        /// Scan again for a round, whatever the user rescanned.
        /// </summary>
        [Fact]
        public void OfNeverTrimsThePanesOwnRows()
        {
            Inputs current = Boxes(TwoKeywords, "  (215) Borders Edging  ");

            After(current);

            Assert.Equal("  (215) Borders Edging  ", current.Filters[0].Prefix);
        }

        [Fact]
        public void TheScanHeaderCarriesTheCountsOnceOneHasBeenRead()
        {
            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");

            Assert.Equal(
                "3  SCAN",
                After(boxes).For(ViewFilterStep.Scan).Header);

            Assert.Equal(
                "3  SCAN   2 plots, 1 skipped, 3 blocked",
                After(boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 1, 3))
                    .For(ViewFilterStep.Scan).Header);

            Assert.Equal(
                "3  SCAN   1 plot, 0 skipped, 0 blocked",
                After(boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(1, 0, 0))
                    .For(ViewFilterStep.Scan).Header);
        }

        [Fact]
        public void ApplyOpensOnlyOnceAScanHasBeenAnsweredAndStillHolds()
        {
            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");

            ViewFilterSteps before = After(boxes);
            Assert.False(before.For(ViewFilterStep.Scan).Done);
            Assert.False(before.For(ViewFilterStep.Apply).Usable);
            Assert.Equal(
                "Scan first, so what Apply will do has been seen.",
                before.For(ViewFilterStep.Apply).WhyNot);

            ViewFilterSteps scanned = After(
                boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0));
            Assert.True(scanned.For(ViewFilterStep.Scan).Done);
            Assert.True(scanned.For(ViewFilterStep.Apply).Usable);
            Assert.False(scanned.For(ViewFilterStep.Apply).Done);
        }

        /// <summary>
        /// The stale scan rule, driven through ApplyGate: an edit after a scan shuts Apply
        /// and un-finishes the scan, and undoing the edit by hand opens both again with no
        /// flag anybody has to reset.
        /// </summary>
        [Fact]
        public void AnEditAfterAScanShutsApplyAndUndoingItOpensApplyAgain()
        {
            Inputs current = Boxes(TwoKeywords, "(200-260) Presentation");
            Inputs scanned = Boxes(TwoKeywords, "(200-260) Presentation");
            ViewFilterScanResult read = Read(2, 0, 0);

            current.Filters[0].Prefix = "(215) Borders Edging";
            ViewFilterSteps moved = After(current, scanned, read);
            Assert.False(moved.For(ViewFilterStep.Scan).Done);
            Assert.False(moved.For(ViewFilterStep.Apply).Usable);
            Assert.Equal("Scan again", moved.For(ViewFilterStep.Apply).WhyNot);

            current.Filters[0].Prefix = "(200-260) Presentation";
            ViewFilterSteps restored = After(current, scanned, read);
            Assert.True(restored.For(ViewFilterStep.Scan).Done);
            Assert.True(restored.For(ViewFilterStep.Apply).Usable);
        }

        [Fact]
        public void ResultsIsShutWithAReasonBeforeARunAndTickedAfterOne()
        {
            ViewFilterStepState before = After().For(ViewFilterStep.Results);
            Assert.False(before.Usable);
            Assert.False(before.Done);
            Assert.Equal("Nothing has run yet.", before.WhyNot);

            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");
            ViewFilterStepState after = After(
                    boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0),
                    Run(12, 1))
                .For(ViewFilterStep.Results);
            Assert.True(after.Usable);
            Assert.True(after.Done);
            Assert.Equal("5  RESULTS   12 applied, 1 not applied", after.Header);
        }

        [Fact]
        public void FinishingApplyOpensResults()
        {
            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");
            ViewFilterSteps steps = After(
                boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0),
                Run(4, 0));

            Assert.Equal(ViewFilterStep.Results, steps.OpenAfter(ViewFilterStep.Apply));
        }

        /// <summary>
        /// A shut step is stepped over, so finishing one never opens a step that would
        /// only say why it cannot be used. A scan answered over rows that have since lost
        /// their prefixes leaves SCAN shut while APPLY still holds, which is the one such
        /// state the pane can reach.
        /// </summary>
        [Fact]
        public void AStepThatCannotBeUsedIsSkippedOver()
        {
            Inputs current = Boxes(TwoKeywords, string.Empty);
            Inputs scanned = Boxes(TwoKeywords, string.Empty);
            ViewFilterSteps steps = After(current, scanned, Read(0, 2, 0));

            Assert.False(steps.For(ViewFilterStep.Scan).Usable);
            Assert.True(steps.For(ViewFilterStep.Apply).Usable);
            Assert.Equal(ViewFilterStep.Apply, steps.OpenAfter(ViewFilterStep.Rows));
        }

        /// <summary>
        /// Nothing further to open leaves the pane where it is. Shutting everything would
        /// look like it had lost its place.
        /// </summary>
        [Fact]
        public void WithNothingFurtherToOpenTheAnswerIsNothing()
        {
            Assert.Null(After().OpenAfter(ViewFilterStep.Scan));
            Assert.Null(After().OpenAfter(ViewFilterStep.Results));
        }

        [Fact]
        public void TheFirstUnfinishedStepIsWhereThePaneLands()
        {
            Assert.Equal(ViewFilterStep.Keywords, After(Boxes(string.Empty)).FirstUnfinished);

            Assert.Equal(
                ViewFilterStep.Rows,
                After(Boxes(TwoKeywords, string.Empty)).FirstUnfinished);

            Assert.Equal(ViewFilterStep.Scan, After().FirstUnfinished);

            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");
            Assert.Equal(
                ViewFilterStep.Apply,
                After(boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0))
                    .FirstUnfinished);
        }

        [Fact]
        public void AfterARunTheFirstUnfinishedStepIsResults()
        {
            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");
            ViewFilterSteps steps = After(
                boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0),
                Run(4, 0));

            Assert.Equal(ViewFilterStep.Results, steps.FirstUnfinished);
        }

        /// <summary>
        /// The rail shows a number and nothing else, so the tooltip is the only place a
        /// cell says what it is. It is never empty: the title, then the summary while the
        /// step is usable, or the reason while it is shut.
        /// </summary>
        [Fact]
        public void TheTipIsNeverEmptyAndSaysTheStateBehindTheNumber()
        {
            ViewFilterSteps steps = After();
            foreach (ViewFilterStepState step in steps.All)
            {
                Assert.NotEqual(string.Empty, step.Tip);
            }

            Assert.Equal("KEYWORDS   2 keywords", steps.For(ViewFilterStep.Keywords).Tip);
            Assert.Equal(
                "APPLY   Scan first, so what Apply will do has been seen.",
                steps.For(ViewFilterStep.Apply).Tip);

            Inputs boxes = Boxes(TwoKeywords, "(200-260) Presentation");
            ViewFilterSteps scanned = After(
                boxes, Boxes(TwoKeywords, "(200-260) Presentation"), Read(2, 0, 0));

            // Usable with nothing to say is the title alone, which still names the cell.
            Assert.Equal("APPLY", scanned.For(ViewFilterStep.Apply).Tip);

            foreach (ViewFilterStepState step in After(Boxes(string.Empty)).All)
            {
                Assert.NotEqual(string.Empty, step.Tip);
            }
        }

        [Fact]
        public void AskingForAStepThatIsNotOneRefuses()
        {
            Assert.Throws<ArgumentException>(() => After().For((ViewFilterStep)9));
        }
    }
}
