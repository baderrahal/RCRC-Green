using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What step 5 says in place of the 247 preview cards.
    ///
    /// The counting-by-reason rule is the one that matters: every refusal sentence names its
    /// plot, so the five views refused on the run of 2026-09-13 for "No scope box is named
    /// DM-02" would read as five different reasons if the sentence were the key.
    /// </summary>
    public class RunSummaryTests
    {
        private static readonly ViewType Overall = new ViewType("010", "Overall Plan");
        private static readonly ViewType General =
            new ViewType("200", "General Arrangement Layout");

        private static RunPlan NoScopeBoxOn(params string[] plots)
        {
            return RunFixture.Of(
                plots.Select(plot => new PlotViewKey(plot, Overall)),
                plots,
                null,
                null,
                null);
        }

        [Fact]
        public void FiveViewsRefusedOnFivePlotsForOneReasonCountAsOneReason()
        {
            RunPlan plan = NoScopeBoxOn("DM-01", "DM-02", "DM-03", "DM-04", "DM-05");

            RefusalCount only = Assert.Single(RunSummary.ByReason(plan));

            Assert.Equal(RunRefusalKind.NoScopeBox, only.Kind);
            Assert.Equal(5, only.HowMany);
            Assert.Equal("5 on a sub plot with no scope box", only.InWords());
        }

        [Fact]
        public void TheLineCountsEverythingAndThenBreaksItDown()
        {
            Assert.Equal(
                "5 things cannot be made: 5 on a sub plot with no scope box.",
                RunSummary.CannotMake(NoScopeBoxOn("DM-01", "DM-02", "DM-03", "DM-04", "DM-05")));

            Assert.Equal(
                "1 thing cannot be made: 1 on a sub plot with no scope box.",
                RunSummary.CannotMake(NoScopeBoxOn("DM-01")));
        }

        [Fact]
        public void ARunWithNothingWrongSaysNothingRatherThanSayingNothingIsWrong()
        {
            RunPlan clean = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", Overall) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                null);

            Assert.Empty(RunSummary.ByReason(clean));
            Assert.Equal(string.Empty, RunSummary.CannotMake(clean));
            Assert.Equal("Nothing refused", RunSummary.ListBehind(clean));
        }

        [Fact]
        public void TwoReasonsComeBackCommonestFirst()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-01", Overall),
                    new PlotViewKey("DM-02", Overall),
                    new PlotViewKey("DM-11", General)
                },
                new[] { "DM-01", "DM-02", "DM-11" },
                new[] { "DM-01", "DM-02", "DM-11" },
                null,
                null);

            // DM-11's General Arrangement is already in the model, and the other two have
            // their scope boxes, so only the one reason shows.
            RunPlan mixed = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-01", Overall),
                    new PlotViewKey("DM-02", Overall),
                    new PlotViewKey("DM-11", General)
                },
                new[] { "DM-01", "DM-02", "DM-11" },
                new[] { "DM-11" },
                null,
                null);

            Assert.Empty(RunSummary.ByReason(plan));

            RefusalCount first = RunSummary.ByReason(mixed)[0];
            Assert.Equal(RunRefusalKind.NoScopeBox, first.Kind);
            Assert.Equal(2, first.HowMany);
        }

        [Fact]
        public void WhatTheRunMakesIsOneLineCountingEachKind()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-11", Overall),
                    new PlotViewKey("DM-12", Overall)
                },
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                null,
                null);

            Assert.Equal("2 plan views.", RunSummary.Makes(plan));
        }

        [Fact]
        public void ARunThatMakesNothingSaysSo()
        {
            RunPlan nothing = RunFixture.Of(null, null, null, null, null);

            Assert.Equal(
                "Nothing is marked or described yet, so this run would make nothing.",
                RunSummary.Makes(nothing));

            Assert.Equal(
                "Nothing is marked or described yet, so this run would make nothing.",
                RunSummary.Makes(null));
        }

        [Fact]
        public void TheControlOverTheShutListSaysHowManyAreBehindIt()
        {
            Assert.Equal(
                "5 refusals",
                RunSummary.ListBehind(NoScopeBoxOn("DM-01", "DM-02", "DM-03", "DM-04", "DM-05")));

            Assert.Equal("1 refusal", RunSummary.ListBehind(NoScopeBoxOn("DM-01")));
            Assert.Equal("Nothing refused", RunSummary.ListBehind(null));
        }

        [Fact]
        public void EveryKindHasItsOwnHeadingAndNoneShareOne()
        {
            var kinds = new List<RunRefusalKind>
            {
                RunRefusalKind.Unsaid,
                RunRefusalKind.AlreadyInTheModel,
                RunRefusalKind.NoScheduleToCaptureFrom,
                RunRefusalKind.NoScopeBox,
                RunRefusalKind.SheetKindIncomplete,
                RunRefusalKind.SheetRowIncomplete,
                RunRefusalKind.SheetNumberClashes
            };

            Assert.Equal(
                System.Enum.GetValues(typeof(RunRefusalKind)).Length, kinds.Count);

            List<string> said = kinds.Select(RunSummary.ReasonInWords).ToList();

            Assert.Equal(said.Count, said.Distinct().Count());
            Assert.DoesNotContain(said, one => one.Length == 0);
        }
    }
}
