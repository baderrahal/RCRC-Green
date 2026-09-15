using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **THE PLOT LIST HOLDS PLOTS NOTHING IN THE REPORT EVER MENTIONED.** The 05:49 run on NG05
    /// offered 165 plots and ticked 156, and MM-09 to MM-15 sat unticked in that list and
    /// appeared nowhere in the 57,143 line report, because every section of the file is over the
    /// ticked plots.
    ///
    /// Three things are pinned here. Nothing invents a plot identifier. A plot named by BOTH
    /// sources is in NEITHER of the pane's two disagreement lines, so their count and the
    /// unticked count are different questions. And every plot offered is one row of the report
    /// with where it came from and whether it was ticked.
    ///
    /// Every expected value is written out by hand.
    /// </summary>
    public class PlotOriginTests
    {
        /// <summary>
        /// **THE HYPOTHESIS TO RULE OUT FIRST.** A run of MM numbers ending at 15 looks like a
        /// range rather than a reading, so this hands the union exactly the eight MM plots the
        /// report named and asks whether the seven the report did not can come out of it.
        /// </summary>
        [Fact]
        public void TheListIsTheUnionOfWhatItWasGivenAndNothingInventsAPlot()
        {
            var eight = new[] { "MM-01", "MM-02", "MM-03", "MM-04", "MM-05", "MM-06", "MM-07", "MM-08" };

            PlotsInTheModel plots = PlotsInTheModel.Of(eight, eight);

            Assert.Equal(eight, plots.All.ToArray());

            foreach (string never in new[] { "MM-09", "MM-10", "MM-11", "MM-12", "MM-13", "MM-14", "MM-15" })
            {
                Assert.False(plots.Holds(never), never + " came out of a list that was never given it");
            }

            // A plot a caller tries to tick that the list does not hold is refused, so nothing
            // downstream can put one in either.
            Assert.Empty(new PlotTicks(plots, new[] { "MM-09" }).Ticked);
        }

        /// <summary>
        /// **WHAT THE TWO DISAGREEMENT LINES ACTUALLY COUNT.** They hold the plots named by
        /// exactly ONE source, so a plot named by both is in neither of them, and on a model of
        /// 165 plots they say nothing at all about 156 of them.
        /// </summary>
        [Fact]
        public void APlotNamedByBothSourcesIsInNeitherDisagreementLine()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "MM-08", "MM-09", "FM-07" },
                new[] { "MM-08", "MM-09", "EP-05" });

            Assert.Equal(new[] { "EP-05", "FM-07", "MM-08", "MM-09" }, plots.All.ToArray());
            Assert.Equal(new[] { "FM-07" }, plots.OnSheetsOnly.ToArray());
            Assert.Equal(new[] { "EP-05" }, plots.OnSchedulesOnly.ToArray());

            // Four plots offered and two named in the two lines. The other two are named by both
            // and the pane prints not one word about them.
            Assert.Equal(4, plots.All.Count);
            Assert.Equal(2, plots.OnSheetsOnly.Count + plots.OnSchedulesOnly.Count);

            PlotOriginList origins = PlotOrigins.Of(plots, new[] { "MM-08", "EP-05", "FM-07" });

            Assert.Equal(2, origins.FromBoth);
            Assert.Equal(
                "The pane's two disagreement lines count the 2 plots named by exactly ONE source. "
                + "The 2 named by both are in neither line, and being named by one source is a "
                + "different question from being ticked, so those two counts answer nothing about "
                + "each other.",
                origins.WhatTheTwoLinesCount);
        }

        /// <summary>
        /// One row per plot, its source and its tick state, and the counts off those rows.
        /// </summary>
        [Fact]
        public void EveryPlotOfferedIsARowWithWhereItCameFromAndWhetherItWasTicked()
        {
            PlotsInTheModel plots = PlotsInTheModel.Of(
                new[] { "MM-08", "MM-09", "FM-07" },
                new[] { "MM-08", "MM-09", "EP-05" });

            PlotOriginList origins = PlotOrigins.Of(plots, new[] { "MM-08", "FM-07" });

            Assert.Equal(
                new[]
                {
                    "EP-05 | on a schedule and on no sheet | NOT TICKED",
                    "FM-07 | on a sheet and on no schedule | ticked",
                    "MM-08 | on a sheet and on a schedule | ticked",
                    "MM-09 | on a sheet and on a schedule | NOT TICKED"
                },
                origins.Rows.Select(one => one.InWords).ToArray());

            Assert.Equal(4, origins.Listed);
            Assert.Equal(2, origins.Ticked);
            Assert.Equal(2, origins.NotTicked);
            Assert.Equal(2, origins.FromBoth);
            Assert.Equal(1, origins.FromASheetOnly);
            Assert.Equal(1, origins.FromAScheduleOnly);
            Assert.Equal(0, origins.FromNeither);
            Assert.True(origins.AddsUp);

            Assert.Equal(
                "THE PLOT LIST: 4 plots offered, 2 ticked and 2 not. 2 named by a sheet AND a "
                + "schedule, 1 by a sheet alone, 1 by a schedule alone.",
                origins.InWords);

            Assert.Equal(
                new[] { "EP-05", "MM-09" },
                origins.NotTickedRows.Select(one => one.PlotId).ToArray());
        }

        /// <summary>
        /// **A PLOT THAT WAS TICKED AND THAT THE READ AT THE PRESS DOES NOT NAME IS A ROW.** The
        /// list is the union of the two reads, so this cannot happen for a plot read off the
        /// model. It is the row that would fire the day something put a plot into the list that
        /// no parameter and no filter ever held, and the line says it is a bug rather than a
        /// state.
        /// </summary>
        [Fact]
        public void APlotTickedThatNeitherSourceNamesIsARowSayingSo()
        {
            PlotOriginList origins = PlotOrigins.Of(
                PlotsInTheModel.Of(new[] { "MM-08" }, new[] { "MM-08" }),
                new[] { "MM-08", "MM-09" });

            Assert.Equal(
                new[]
                {
                    "MM-08 | on a sheet and on a schedule | ticked",
                    "MM-09 | NAMED BY NEITHER SOURCE | ticked"
                },
                origins.Rows.Select(one => one.InWords).ToArray());

            Assert.Equal(1, origins.FromNeither);
            Assert.True(origins.AddsUp);
            Assert.Equal(
                "THE PLOT LIST: 2 plots offered, 2 ticked and 0 not. 1 named by a sheet AND a "
                + "schedule, 0 by a sheet alone, 0 by a schedule alone, and 1 BY NEITHER, WHICH "
                + "IS A BUG IN THIS TOOL.",
                origins.InWords);
        }

        [Fact]
        public void ARunBuiltWithNoPlotListSaysSoRatherThanReadingAsAModelWithNoPlot()
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>());

            Assert.Equal(0, set.Origins.Listed);
            Assert.False(set.Origins.Counts);

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 15, 5, 49, 0));

            Assert.Contains(PlotOriginWords.NothingRead, report);
        }

        /// <summary>
        /// The section is the FIRST of the body, above the glance, and it names the plots nothing
        /// else in the file mentions.
        /// </summary>
        [Fact]
        public void TheReportOpensWithEveryPlotTheToolOfferedAndNamesTheOnesNobodyTicked()
        {
            PlotWorkbookPath where = PlotWorkbookPath.For(
                @"C:\out", KpiTemplates.Streets, "STREET 36m ROW", "ANH-007-ST-100210");

            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new List<TemplateShare>(), new List<PlotTemplate>(), new List<PlotTemplate>()),
                new List<KpiCreateRun>(),
                new List<TemplateOutcome>(),
                null,
                new[] { PlotOutcome.Wrote("MM-08", KpiTemplates.Streets, where) },
                null,
                PlotsInTheModel.Of(
                    new[] { "MM-08", "MM-09" }, new[] { "MM-08", "MM-09", "EP-05" }));

            string report = KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 15, 5, 49, 0));

            Assert.Contains("== " + KpiCreateReport.PlotListHeading + " (3) ==", report);
            Assert.Contains("  MM-09 | on a sheet and on a schedule | NOT TICKED", report);
            Assert.Contains("  EP-05 | on a schedule and on no sheet | NOT TICKED", report);
            Assert.Contains("  NOT TICKED, 2. Nothing else in this file mentions these plots.", report);
            Assert.Contains("  every plot is in exactly one source row and one tick state   YES", report);

            // **ABOVE THE GLANCE**, because it is the list every other number in the file is a
            // subset of. The contents block above the body names both sections, so the heading
            // line is what is compared rather than the bare name.
            Assert.True(
                report.IndexOf("== " + KpiCreateReport.PlotListHeading + " (", StringComparison.Ordinal)
                    < report.IndexOf("== " + KpiCreateReport.GlanceHeading + " ==", StringComparison.Ordinal),
                "the plot list must be above the glance");
        }
    }
}
