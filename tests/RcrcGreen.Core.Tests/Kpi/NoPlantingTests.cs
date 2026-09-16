using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **MM-01, MM-06, MM-07 AND NS-23 READ 0 IN EVERY TREE, SHRUB, LAWN, WATER AND GREEN COVER
    /// BOX ON THE 16:37 PRESS**, because both of their schedules printed their heading and no
    /// rows. Writing 0 there is Bader's decision of 15 September and it stands. What was missing
    /// is the line that separates those four from a plot whose numbers happen to be small.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class NoPlantingTests
    {
        /// <summary>
        /// The heading row every schedule prints, written out as the 1548 scan measured it. It
        /// is the FIRST row, which is what the row readers already take it to be, and a schedule
        /// showing it and nothing else is a schedule that printed nothing.
        /// </summary>
        private static readonly string[] SoftscapeHeadings =
        {
            "IMAGE", "BOTANICAL NAME", "COUNT (n)"
        };

        private static readonly string[] ShrubsHeadings = { "BOTANICAL NAME", "AREA  (sqm)" };

        private static PlotReading Plot(string plotId, string[][] softscape, string[][] shrubs)
        {
            return CreateFixture.Plot(
                plotId,
                printedSchedules: new[]
                {
                    CreateFixture.Softscape(
                        plotId, new[] { SoftscapeHeadings }.Concat(softscape).ToArray()),
                    CreateFixture.ShrubsAndLawn(
                        plotId, new[] { ShrubsHeadings }.Concat(shrubs).ToArray())
                });
        }

        private static KpiCreateRunSet Press(params PlotReading[] readings)
        {
            return new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    readings.Select(one => one.PlotId).ToList(),
                    plotId => "FRIDAY MOSQUE",
                    new[] { KpiTemplates.Mosques }),
                new List<KpiCreateRun>
                {
                    CreateFixture.Run(readings: readings, template: KpiTemplates.Mosques)
                },
                new List<TemplateOutcome>());
        }

        /// <summary>
        /// **TWO PLOTS, ONE PRINTING ITS HEADING AND NOTHING ELSE IN EITHER SCHEDULE.** Only
        /// that one is named.
        ///
        /// **THIS IS THE SHAPE THE 21:38 PRESS CARRIED AND THE LINE NEVER FIRED ON.** MM-06,
        /// MM-07 and NS-23 each printed one row, the heading, with no group row, no species row
        /// and no TOTAL, and the count that was asked counts the heading, so all three read as
        /// plots whose schedules printed something.
        /// </summary>
        [Fact]
        public void OnlyThePlotWhoseTwoSchedulesPrintedNothingIsNamed()
        {
            KpiCreateRunSet set = Press(
                Plot("MM-01", new string[0][], new string[0][]),
                Plot(
                    "MM-05",
                    new[] { new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "13" } },
                    new[] { new[] { "GRASS", string.Empty } }));

            IReadOnlyList<string> named = NoPlanting.In(set);

            // **THE PLOT IS IN THE MESSAGE.** A failure reading `expected 1, got 0` says nothing
            // about which plot went out with five boxes of noughts and no line about it.
            Assert.True(
                named.Count == 1 && named[0] == "MM-01",
                "MM-01's softscape and shrubs and lawn schedules both printed a heading and no "
                + "rows, so every planting box on it was written 0, and it is the plot this line "
                + "exists to name. MM-05 printed a row in each and must not be named. What the "
                + "check found: "
                + (named.Count == 0 ? "no plot at all" : string.Join(", ", named.ToArray())));

            Assert.Equal(
                "THE PLOTS WITH NO PLANTING AT ALL: 1 plot's softscape and shrubs and lawn "
                + "schedules both printed a heading and no rows, so every planting box on it "
                + "was written 0. MM-01.",
                NoPlanting.InWords(named));
        }

        /// <summary>
        /// **A PLOT WITH ROWS IN ONE SCHEDULE AND NONE IN THE OTHER IS NOT NAMED.** It has
        /// planting, and calling it a plot with none would say something about the model that
        /// nothing measured.
        /// </summary>
        [Fact]
        public void OneScheduleWithRowsIsEnoughToKeepAPlotOutOfTheLine()
        {
            Assert.Empty(NoPlanting.In(Press(Plot(
                "MM-02",
                new[] { new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "13" } },
                new string[0][]))));
        }

        /// <summary>
        /// **A PLOT MISSING A SCHEDULE IS NOT THIS.** It is already named by its own refusal and
        /// by the PDF's reason, and a plot with no schedule and a plot whose schedule printed
        /// nothing are two different facts about a model.
        /// </summary>
        [Fact]
        public void APlotHoldingNoSoftscapeScheduleIsNotNamedHere()
        {
            Assert.Empty(NoPlanting.In(Press(CreateFixture.Plot(
                "MM-03",
                softscapeRead: false,
                printedSchedules: new[] { CreateFixture.ShrubsAndLawn("MM-03") }))));
        }

        /// <summary>
        /// **A SCHEDULE WHOSE ROWS REVIT REFUSED IS NOT A SCHEDULE THAT PRINTED NOTHING.** It
        /// holds no rows because nobody could read them, which is an absence and not an answer,
        /// and the plot is already named by the refusal that produced it.
        /// </summary>
        [Fact]
        public void ASchedulesWhoseRowsWereRefusedDoesNotNameThePlot()
        {
            KpiCreateRunSet set = Press(CreateFixture.Plot(
                "MM-04",
                printedSchedules: new[]
                {
                    KpiFixture.Schedule(CreateFixture.SoftscapeName("MM-04"), rowsRefused: true),
                    CreateFixture.ShrubsAndLawn("MM-04", ShrubsHeadings)
                }));

            Assert.Empty(NoPlanting.In(set));
        }

        /// <summary>
        /// **THE RECONCILIATION COUNTS A BODY THE SAME WAY.** It counted a schedule as having
        /// printed one when its body row count was above nought, and that count includes the
        /// heading, so MM-06's two schedules read 2 of 2 with a body on a plot holding no
        /// planting at all.
        /// </summary>
        [Fact]
        public void TheReconciliationCountsAHeadingOnlyScheduleAsPrintingNoBody()
        {
            PlotReading held = Plot("MM-06", new string[0][], new string[0][]);

            Reconciliation counted = Reconciliation.Of(
                new[] { "MM-06" }, new[] { held }, null, false, KpiTemplates.Mosques);

            Assert.Equal(2, counted.SchedulesPrinted);
            Assert.Equal(0, counted.SchedulesWithABody);
        }

        /// <summary>
        /// **AND ONE ROW UNDER THE HEADING IS A BODY.** The count moves with the row rather than
        /// with the heading.
        /// </summary>
        [Fact]
        public void TheReconciliationCountsAScheduleWithOneRowUnderItsHeading()
        {
            PlotReading held = Plot(
                "MM-05",
                new[] { new[] { "Albizia lebbeck.jpg", "ALBIZIA LEBBECK", "13" } },
                new string[0][]);

            Reconciliation counted = Reconciliation.Of(
                new[] { "MM-05" }, new[] { held }, null, false, KpiTemplates.Mosques);

            Assert.Equal(2, counted.SchedulesPrinted);
            Assert.Equal(1, counted.SchedulesWithABody);
        }

        /// <summary>
        /// **A PRESS WHERE EVERY PLOT HAS PLANTING SAYS SO**, because a line that disappears
        /// when there is nothing to report reads the same as one nobody wrote.
        /// </summary>
        [Fact]
        public void APressWhereEveryPlotHasPlantingSaysSo()
        {
            Assert.Equal(
                "THE PLOTS WITH NO PLANTING AT ALL: every plot this press read printed at least "
                + "one schedule row.",
                NoPlanting.InWords(new List<string>()));
        }

        /// <summary>
        /// **THE THREE PLOTS OF THE 21:38 PRESS, IN THE LINE AS IT WILL READ.** MM-06, MM-07 and
        /// NS-23 read 0 in every tree, shrub, lawn, water and green cover box and the glance
        /// said every plot this press read printed at least one schedule row.
        /// </summary>
        [Fact]
        public void TheThreePlotsOfThe2138PressReadAsOneLine()
        {
            Assert.Equal(
                "THE PLOTS WITH NO PLANTING AT ALL: 3 plots' softscape and shrubs and lawn "
                + "schedules both printed a heading and no rows, so every planting box on them "
                + "was written 0. MM-06, MM-07, NS-23.",
                NoPlanting.InWords(new[] { "MM-06", "MM-07", "NS-23" }));
        }

        /// <summary>
        /// The four plots the 16:37 press really carried, in the line as it will read.
        /// </summary>
        [Fact]
        public void TheFourPlotsOfThe1637PressReadAsOneLine()
        {
            Assert.Equal(
                "THE PLOTS WITH NO PLANTING AT ALL: 4 plots' softscape and shrubs and lawn "
                + "schedules both printed a heading and no rows, so every planting box on them "
                + "was written 0. MM-01, MM-06, MM-07, NS-23.",
                NoPlanting.InWords(new[] { "MM-01", "MM-06", "MM-07", "NS-23" }));
        }
    }
}
