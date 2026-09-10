using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScopeBoxReportTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 8, 14, 5, 0);

        private static string[] LinesOf(string report)
        {
            return report.Split(new[] { ScanReport.LineEnd }, StringSplitOptions.None);
        }

        private static string HeadingIn(string report, string title)
        {
            return LinesOf(report).Single(line => line.StartsWith("== " + title + " (", StringComparison.Ordinal));
        }

        private static ScopeBoxPlan SixCases()
        {
            return ScopeBoxPlan.Decide(
                new[]
                {
                    new ViewScopeBoxState(1, "SOFTSCAPE SCHEDULES", true, ""),
                    new ViewScopeBoxState(2, "600QD", true, ""),
                    new ViewScopeBoxState(3, "DM-41-(010) Location Key Plan", false, ""),
                    new ViewScopeBoxState(4, "DM-41-(200) General Arrangement Layout", true, ""),
                    new ViewScopeBoxState(5, "AB-7-(200) General Arrangement Layout", true, ""),
                    new ViewScopeBoxState(6, "PF-12-(010) Location Key Plan", true, "PF-12"),
                    new ViewScopeBoxState(7, "PF-12-(400) Landscape Cross Section", true, "DM-41")
                },
                new[] { "DM-41", "PF-12" });
        }

        [Fact]
        public void EverySectionHeadingCarriesItsLetterAndItsCount()
        {
            string report = ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null);

            Assert.Equal("== A, NAME DOES NOT PARSE (2) ==", HeadingIn(report, "A, NAME DOES NOT PARSE"));
            Assert.Equal("== B, CANNOT HOLD A SCOPE BOX (1) ==", HeadingIn(report, "B, CANNOT HOLD A SCOPE BOX"));
            Assert.Equal("== C, ASSIGNED (1) ==", HeadingIn(report, "C, ASSIGNED"));
            Assert.Equal("== D, NO SCOPE BOX FOR THAT PLOT (1) ==", HeadingIn(report, "D, NO SCOPE BOX FOR THAT PLOT"));
            Assert.Equal("== E, ALREADY RIGHT (1) ==", HeadingIn(report, "E, ALREADY RIGHT"));
            Assert.Equal("== F, HOLDS A DIFFERENT SCOPE BOX (1) ==", HeadingIn(report, "F, HOLDS A DIFFERENT SCOPE BOX"));
        }

        [Fact]
        public void TheSectionsComeOutInCaseOrder()
        {
            string[] headings = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null))
                .Where(line => line.StartsWith("== ", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "== A, NAME DOES NOT PARSE (2) ==",
                    "== B, CANNOT HOLD A SCOPE BOX (1) ==",
                    "== C, ASSIGNED (1) ==",
                    "== D, NO SCOPE BOX FOR THAT PLOT (1) ==",
                    "== E, ALREADY RIGHT (1) ==",
                    "== F, HOLDS A DIFFERENT SCOPE BOX (1) =="
                },
                headings);
        }

        [Fact]
        public void CaseBIsACountAndNothingElse()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null));

            int at = Array.FindIndex(lines, line => line.StartsWith("== B,", StringComparison.Ordinal));

            Assert.Equal("Counted only. A view with no scope box parameter is not a fault.", lines[at + 1]);
            Assert.Empty(lines[at + 2]);
            Assert.DoesNotContain(lines, line => line.StartsWith("DM-41-(010) Location Key Plan", StringComparison.Ordinal));
        }

        [Fact]
        public void CaseDNamesTheViewAndThePlotItWanted()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null));

            Assert.Contains("AB-7-(200) General Arrangement Layout | AB-7", lines);
        }

        [Fact]
        public void CaseFNamesTheBoxItHasAndTheBoxItExpected()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null));

            Assert.Contains("PF-12-(400) Landscape Cross Section | DM-41 | PF-12", lines);
        }

        [Fact]
        public void AConfirmedRunSaysWhatItAssigned()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null));

            Assert.Contains("1 view was given a scope box. Nothing else was changed.", lines);
            Assert.Contains("DM-41-(200) General Arrangement Layout | DM-41 | assigned", lines);
        }

        [Fact]
        public void TwoAssignmentsAreCountedInThePlural()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[]
                {
                    new ViewScopeBoxState(1, "DM-41-(010) Location Key Plan", true, ""),
                    new ViewScopeBoxState(2, "DM-41-(200) General Arrangement Layout", true, "")
                },
                new[] { "DM-41" });

            string[] lines = LinesOf(ScopeBoxReport.Write(plan, "NG05", Noon, true, null));

            Assert.Contains("2 views were given a scope box. Nothing else was changed.", lines);
        }

        [Fact]
        public void ARunTheUserSaidNoToWritesTheFileAndChangesNothing()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, false, null));

            Assert.Contains("Nothing was changed. The assignment was not confirmed.", lines);
            Assert.Contains(
                "DM-41-(200) General Arrangement Layout | DM-41 | not confirmed, nothing written",
                lines);
            Assert.Equal("== C, ASSIGNED (1) ==", HeadingIn(string.Join(ScanReport.LineEnd, lines), "C, ASSIGNED"));
        }

        [Fact]
        public void AViewThatRefusedTheAssignmentIsNamedRatherThanCountedAsDone()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, new long[] { 4 }));

            Assert.Contains("DM-41-(200) General Arrangement Layout | DM-41 | refused by the view", lines);
            Assert.Contains("1 of those were refused by the view and did not change.", lines);
            Assert.Contains("0 views were given a scope box. Nothing else was changed.", lines);
        }

        [Fact]
        public void TheHeaderSaysWhichCasesWriteAndHowMatchingWorks()
        {
            string[] lines = LinesOf(ScopeBoxReport.Write(SixCases(), "NG05", Noon, true, null));

            Assert.Contains("Document: NG05", lines);
            Assert.Contains("Written: 2026-09-08 14:05", lines);
            Assert.Contains("Only case C writes. D and F are reported and left alone.", lines);
            Assert.Contains("A scope box matches a plot on an exact, case sensitive name.", lines);
        }

        [Fact]
        public void RowsComeOutWithTheNumberPartInNumberOrder()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[]
                {
                    new ViewScopeBoxState(1, "DM-100-(010) Location Key Plan", true, ""),
                    new ViewScopeBoxState(2, "DM-2-(010) Location Key Plan", true, ""),
                    new ViewScopeBoxState(3, "DM-9-(010) Location Key Plan", true, "")
                },
                new string[0]);

            string[] rows = LinesOf(ScopeBoxReport.Write(plan, "NG05", Noon, false, null))
                .Where(line => line.StartsWith("DM-", StringComparison.Ordinal))
                .ToArray();

            Assert.Equal(
                new[]
                {
                    "DM-2-(010) Location Key Plan | DM-2",
                    "DM-9-(010) Location Key Plan | DM-9",
                    "DM-100-(010) Location Key Plan | DM-100"
                },
                rows);
        }

        [Fact]
        public void AnEmptyPlanStillWritesEverySectionWithAZeroInIt()
        {
            string report = ScopeBoxReport.Write(ScopeBoxPlan.Decide(null, null), "NG05", Noon, false, null);

            Assert.Equal("== A, NAME DOES NOT PARSE (0) ==", HeadingIn(report, "A, NAME DOES NOT PARSE"));
            Assert.Equal("== B, CANNOT HOLD A SCOPE BOX (0) ==", HeadingIn(report, "B, CANNOT HOLD A SCOPE BOX"));
            Assert.Equal("== C, ASSIGNED (0) ==", HeadingIn(report, "C, ASSIGNED"));
            Assert.Equal("== D, NO SCOPE BOX FOR THAT PLOT (0) ==", HeadingIn(report, "D, NO SCOPE BOX FOR THAT PLOT"));
            Assert.Equal("== E, ALREADY RIGHT (0) ==", HeadingIn(report, "E, ALREADY RIGHT"));
            Assert.Equal("== F, HOLDS A DIFFERENT SCOPE BOX (0) ==", HeadingIn(report, "F, HOLDS A DIFFERENT SCOPE BOX"));
        }

        [Fact]
        public void TheFileNameCarriesTheScopeBoxPrefix()
        {
            Assert.Equal(
                "RCRC-Green-ScopeBox_NG05_2026-09-08_1405.txt",
                ScanFileName.For(ReportFileNames.ScopeBoxPrefix, "NG05", Noon));
        }
    }
}
