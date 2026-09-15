using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Ticking a template row ticks the plots that will go into its workbook. Every expected
    /// value written out by hand.
    ///
    /// Measured on the first press over several templates, NG05 at 08:37: MOSQUES, PARKING,
    /// SCHOOLS and STREETS ticked, six plots ticked, every one of them SC, and three of the
    /// four templates read no ticked plot belongs to them. The tool did what it was told and
    /// the specification was wrong.
    /// </summary>
    public class TickingATemplateTests
    {
        private static readonly string[] InTheModel =
        {
            "DM-12", "DM-14", "FM-05", "SC-03", "ST-05", "EP-05"
        };

        private static Func<string, string> Components(params string[] pairs)
        {
            var held = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int at = 0; at + 1 < pairs.Length; at += 2) held[pairs[at]] = pairs[at + 1];

            return plotId =>
            {
                string component;
                return held.TryGetValue(plotId, out component) ? component : string.Empty;
            };
        }

        private static readonly Func<string, string> OnTheSheets = Components(
            "DM-12", "DAILY MOSQUE",
            "DM-14", "DAILY MOSQUE",
            "FM-05", "FRIDAY MOSQUE",
            "SC-03", "SCHOOL",
            "ST-05", "STREET 36m ROW");

        private static PlotTicks Nothing()
        {
            return new PlotTicks(PlotsInTheModel.Of(InTheModel, InTheModel));
        }

        /// <summary>
        /// The fault this fixes. Ticking MOSQUES says which plots are meant, and the tool no
        /// longer waits to be told the same fact twice.
        /// </summary>
        [Fact]
        public void TickingATemplateTicksEveryPlotThatBelongsToIt()
        {
            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, OnTheSheets, null);

            Assert.Equal(new[] { "DM-12", "DM-14", "FM-05" }, after.Ticked.ToArray());
        }

        /// <summary>
        /// Unticking takes them off again, and leaves every other row's alone, because several
        /// templates can be ticked at once now.
        /// </summary>
        [Fact]
        public void UntickingATemplateTakesItsOwnPlotsOffAndLeavesTheRest()
        {
            PlotTicks both = TickingATemplate.Ticked(
                TickingATemplate.Ticked(Nothing(), KpiTemplates.Mosques, OnTheSheets, HandTicks.None),
                KpiTemplates.Streets, OnTheSheets, HandTicks.None);

            Assert.Equal(new[] { "DM-12", "DM-14", "FM-05", "ST-05" }, both.Ticked.ToArray());

            PlotTicks after = TickingATemplate.Unticked(
                both, KpiTemplates.Mosques, OnTheSheets, HandTicks.None);

            Assert.Equal(new[] { "ST-05" }, after.Ticked.ToArray());
        }

        /// <summary>
        /// Guard a. **A plot unticked by hand stays unticked**, so ticking a template is a
        /// starting point rather than a lock.
        /// </summary>
        [Fact]
        public void APlotUntickedByHandStaysUntickedWhenItsTemplateIsTicked()
        {
            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, OnTheSheets, HandTicks.None.TakenOff("DM-14"));

            Assert.Equal(new[] { "DM-12", "FM-05" }, after.Ticked.ToArray());
        }

        /// <summary>
        /// Guard b. **The count on the row is what will actually go in**, not how many the
        /// template could take. Two of the three, with DM-14 held off by hand.
        /// </summary>
        [Fact]
        public void TheRowCountsWhatWillGoInRatherThanWhatItCouldTake()
        {
            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, OnTheSheets, HandTicks.None.TakenOff("DM-14"));

            TemplateSplit split = PlotsPerTemplate.Split(
                after.Ticked, OnTheSheets, new[] { KpiTemplates.Mosques });

            TemplateShare share = Assert.Single(split.Shares);
            Assert.Equal(2, share.Plots.Count);
            Assert.Equal("MOSQUES: 2 plots, DM-12, FM-05. The report names them.", CreateWords.TemplateRow(share));

            Assert.Equal(
                "MOSQUES: 2 of the 3 plots that belong to it, the rest unticked by hand.",
                TickingATemplate.SomeOfThem(KpiTemplates.Mosques, 2, 3));

            // Nothing held off says nothing extra, because the row's own line already carries
            // the count and a second sentence saying 3 of 3 is noise.
            Assert.Equal(string.Empty, TickingATemplate.SomeOfThem(KpiTemplates.Mosques, 3, 3));
        }

        /// <summary>
        /// Guard c. **A template with no plots at all still ticks and still writes nothing**,
        /// exactly as it did, and the row says so.
        /// </summary>
        [Fact]
        public void ATemplateWithNoPlotsAtAllTicksAndStillSaysItWillWriteNothing()
        {
            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Healthcare, OnTheSheets, null);

            Assert.Empty(after.Ticked);

            TemplateSplit split = PlotsPerTemplate.Split(
                after.Ticked, OnTheSheets, new[] { KpiTemplates.Healthcare });

            TemplateShare share = Assert.Single(split.Shares);
            Assert.False(share.WillWrite);
            Assert.Equal(
                "HEALTHCARE: no ticked plot belongs to HEALTHCARE, so it will write nothing. "
                + "Tick a plot of it, or leave it and it stays listed writing nothing.",
                CreateWords.TemplateRow(share));
        }

        /// <summary>
        /// **IT TICKS BY THE SPLIT'S OWN RULE AND NEVER BY THE PREFIX.** The grouping buttons
        /// this replaces gathered plots by the two letters at the front of the identifier, and
        /// the split reads PRX_Component first. On a plot whose component and prefix disagree
        /// the two answers part: the prefix says MOSQUES and the split places it nowhere, so
        /// ticking by the prefix would tick a plot that lands in no workbook at all.
        /// </summary>
        [Fact]
        public void ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook()
        {
            Func<string, string> disagreeing = Components(
                "DM-12", "SCHOOL",
                "DM-14", "DAILY MOSQUE",
                "FM-05", "FRIDAY MOSQUE");

            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.Mosques, disagreeing, null);

            // DM-12 is not ticked, because its component says SCHOOLS and its prefix says
            // MOSQUES, so NEITHER places it and no workbook would take it.
            Assert.Equal(new[] { "DM-14", "FM-05" }, after.Ticked.ToArray());

            // The two answers for DM-12, side by side. The prefix route on its own still says
            // MOSQUES, which is what a grouping button would have ticked it on, and the split
            // places it nowhere.
            Assert.Same(KpiTemplates.Mosques, PlotPrefixes.For("DM-12"));
            Assert.Null(PlotsPerTemplate.For("DM-12", disagreeing("DM-12")).Template);
        }

        /// <summary>
        /// A plot with no component at all is placed by its prefix, so the EXISTING PARKS row
        /// ticks EP-05, which is one of the four the 1548 scan found on a schedule and on no
        /// sheet.
        /// </summary>
        [Fact]
        public void APlotWithNoComponentIsStillTickedByItsOwnTemplatesRow()
        {
            PlotTicks after = TickingATemplate.Ticked(
                Nothing(), KpiTemplates.ExistingParks, OnTheSheets, null);

            Assert.Equal(new[] { "EP-05" }, after.Ticked.ToArray());
        }
    }
}
