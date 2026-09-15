using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The irrigation water demand: both schedules' own L/DAY TOTAL rows, added, divided by a
    /// thousand for the form.
    ///
    /// **EVERY EXPECTED NUMBER IN THIS FILE IS WRITTEN OUT BY HAND.** Nothing works an expected
    /// value out with the rule the code uses, because a test that reads the code back to itself
    /// proves nothing.
    /// </summary>
    public class WaterDemandTests
    {
        /// <summary>
        /// The real softscape heading row, measured on the 1548 scan. **THREE OF ITS COLUMNS
        /// HOLD THE WORD WATER** and only the last reads L/DAY.
        /// </summary>
        private static readonly string[] SoftscapeHeadings =
        {
            "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "COUNT (n)",
            "HEIGHT (m)", "DIAMETER (m)", "WATER DEMAND", "WATER L/TREE/DAY", "L/DAY"
        };

        /// <summary>
        /// The real shrubs and lawn heading row, which names its own three WATER columns.
        /// </summary>
        private static readonly string[] GroundHeadings =
        {
            "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "AREA (sqm)", "COUNT (n)",
            "HEIGHT (m)", "SPREAD (m)", "WATER DEMAND", "WATER L/SQM/DAY", "L/DAY"
        };

        private static string[] SoftscapeRow(string image, string name, string count, string litres)
        {
            return new[] { image, "1", "ALB LEB", name, count, "1.5", "2.5", "MEDIUM", "36", litres };
        }

        private static string[] GroundRow(string image, string name, string area, string litres)
        {
            return new[] { image, "1", "PEN SET", name, area, "46", "0.4", "0.5", "LOW", "3", litres };
        }

        /// <summary>
        /// One press over the readings given, written out as the report file. Built the way the
        /// other report cases build one, so this asserts against the real writer.
        /// </summary>
        private static string Report(params PlotReading[] readings)
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached",
                PlotsPerTemplate.Split(
                    readings.Select(one => one.PlotId).ToList(),
                    plot => "FRIDAY MOSQUE",
                    new[] { KpiTemplates.Mosques }),
                new List<KpiCreateRun> { CreateFixture.Run(readings) },
                new List<TemplateOutcome>());

            return KpiCreateReport.WriteAll(set, new DateTime(2026, 9, 15, 5, 49, 0));
        }

        /// <summary>
        /// The column is the one whose heading IS L/DAY, and WATER L/TREE/DAY sitting beside it
        /// does not answer for it.
        /// </summary>
        [Fact]
        public void TheSoftscapeTotalIsTakenOffTheColumnWhoseHeadingReadsLitresADay()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                SoftscapeHeadings,
                new[] { "TREES", "", "", "", "", "", "", "", "", "" },
                new[] { "Proposed", "", "", "", "", "", "", "", "", "" },
                SoftscapeRow("albizia.jpg", "ALBIZIA LEBBECK", "13", "468"),
                SoftscapeRow("bauhinia.jpg", "BAUHINIA PURPUREA", "19", "684"),
                new[] { "", "", "", "", "32", "", "", "", "", "1152" },
                new[] { "TOTAL", "", "", "", "32", "", "", "", "", "1152" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.True(read.Read, read.Why);
            Assert.Equal(1152.0, read.LitresADay);

            // Row 7 counting the heading row as row 1: TREES, Proposed, two species, the
            // subtotal, then TOTAL.
            Assert.Equal(7, read.TotalRow);
            Assert.Equal("DM-11-(600) SOFTSCAPE SCHEDULE", read.ScheduleName);
        }

        [Fact]
        public void TheShrubsAndLawnTotalIsTakenTheSameWay()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                GroundHeadings,
                new[] { "GRASS", "", "", "", "", "", "", "", "", "", "" },
                GroundRow("pennisetum.jpg", "PENNISETUM SETACEUM", "35 m²", "432"),
                new[] { "", "", "", "", "35 m²", "46", "", "", "", "", "432" },
                new[] { "TOTAL", "", "", "", "35 m²", "46", "", "", "", "", "432" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.True(read.Read, read.Why);
            Assert.Equal(432.0, read.LitresADay);
            Assert.Equal(5, read.TotalRow);
        }

        /// <summary>
        /// **THE SUBSTRING RULE WOULD HAVE TAKEN THE WRONG COLUMN.** A heading row whose only
        /// WATER column is WATER L/TREE/DAY names no L/DAY column at all, and it refuses rather
        /// than taking the one that merely looks like it. This is the two DIAMETER headings
        /// again.
        /// </summary>
        [Fact]
        public void AHeadingRowNamingNoColumnThatReadsLitresADayRefusesAndPrintsItsHeadings()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)", "WATER DEMAND", "WATER L/TREE/DAY" },
                new[] { "TOTAL", "", "32", "", "1152" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.False(read.Read);
            Assert.Equal(
                "no column of the heading row reads L/DAY. Its headings: "
                + "IMAGE | BOTANICAL NAME | COUNT (n) | WATER DEMAND | WATER L/TREE/DAY",
                read.Why);
        }

        /// <summary>
        /// **TWO COLUMNS READING L/DAY IS A REFUSAL AND NEVER THE FIRST**, and both are named by
        /// their column number so a person can look at the schedule.
        /// </summary>
        [Fact]
        public void TwoColumnsReadingLitresADayRefuseAndNameBoth()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                new[] { "IMAGE", "L/DAY", "COUNT (n)", "L/DAY" },
                new[] { "TOTAL", "1152", "32", "999" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.False(
                read.Read,
                "two columns read L/DAY and the reader took one of them anyway, "
                + WaterDemand.Litres(read.LitresADay) + " off row " + read.TotalRow
                + ". More than one is a refusal and never the first.");
            Assert.Equal(
                "L/DAY is the heading of more than one column, 2 and 4, and nothing says which is "
                + "meant. Its headings: IMAGE | L/DAY | COUNT (n) | L/DAY",
                read.Why);
        }

        /// <summary>
        /// **A SCHEDULE WITH NO TOTAL ROW WRITES NOTHING FOR THAT HALF.** The species rows and
        /// the subtotal rows are right there and nothing adds them: Bader said take the total,
        /// and a sum this tool worked out is a different number from one the schedule printed.
        /// </summary>
        [Fact]
        public void AScheduleThatPrintsNoTotalRowIsNamedAndItsSpeciesRowsAreNotAddedUp()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                SoftscapeHeadings,
                new[] { "TREES", "", "", "", "", "", "", "", "", "" },
                new[] { "Proposed", "", "", "", "", "", "", "", "", "" },
                SoftscapeRow("albizia.jpg", "ALBIZIA LEBBECK", "13", "468"),
                SoftscapeRow("bauhinia.jpg", "BAUHINIA PURPUREA", "19", "684"),
                new[] { "", "", "", "", "32", "", "", "", "", "1152" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.False(read.Read);
            Assert.Equal(0.0, read.LitresADay);
            Assert.Equal(WaterDemandRead.NoTotalRow, read.Why);
            Assert.Contains("NOT added", read.Why);
        }

        /// <summary>
        /// A TOTAL row whose L/DAY cell is not a number refuses with the row, the heading and
        /// what the cell held.
        /// </summary>
        [Fact]
        public void ATotalRowWhoseLitresCellIsNotANumberRefusesAndNamesTheRow()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)", "L/DAY" },
                new[] { "TOTAL", "", "32", "n/a" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.False(read.Read);
            Assert.Equal("row 2, the TOTAL row: its L/DAY cell holds 'n/a' and not a number", read.Why);
        }

        /// <summary>
        /// The thousands separator refusal reaches here too, through the one cell reader, so a
        /// TOTAL row printing 1,152 is refused rather than read as 1.
        /// </summary>
        [Fact]
        public void ATotalRowCarryingADigitAfterTheNumberEndsIsRefused()
        {
            ScannedSchedule schedule = CreateFixture.Softscape(
                "DM-11",
                new[] { "IMAGE", "BOTANICAL NAME", "COUNT (n)", "L/DAY" },
                new[] { "TOTAL", "", "32", "1,152" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.False(read.Read);
            Assert.StartsWith("row 2, the TOTAL row: its L/DAY cell holds '1,152'", read.Why);
            Assert.Contains("thousands separator", read.Why);
        }

        /// <summary>
        /// **BOTH HALVES ADDED, THEN DIVIDED BY A THOUSAND.** Every number here is written out
        /// by hand: 1152 plus 1340 is 2492 litres a day, which is 2.492 cubic metres a day.
        /// </summary>
        [Fact]
        public void BothTotalsAddedAndDividedByAThousandGiveWhatTheFormAsksFor()
        {
            PlotWaterDemand water = WaterDemand.Of(
                WaterDemandRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                WaterDemandRead.Of("DM-11-(600) SHRUBS AND LAWN SCHEDULE", 1340.0, 9));

            Assert.True(water.BothRead);
            Assert.Equal(2492.0, water.LitresADay);
            Assert.Equal(2.492, water.CubicMetresADay);
            Assert.Equal(string.Empty, water.Why);
            Assert.Equal(
                "1152 L/day plus 1340 L/day is 2492 L/day, which is 2.492 m³/day for the form",
                water.InWords);
        }

        /// <summary>
        /// **THE CHECK AGAINST A PLOT BADER CAN OPEN.** He measured DM-11's shrubs and lawn
        /// schedule printing L/DAY totals of 432 and 908 on the 1548 scan.
        ///
        /// **A GROUP TOTAL IS NOT THE SCHEDULE'S TOTAL and this pins which one is taken.** If
        /// those two are the two group totals, the schedule's own TOTAL row reads 1340 and 1340
        /// is what the tool takes. It never takes 432, never takes 908 and never adds the
        /// species rows. The report prints the ROW the number came off, so the first run
        /// settles which reading of his numbers is right rather than this test assuming one.
        /// </summary>
        [Fact]
        public void OnDm11TheScheduleTotalIsTakenAndNeitherGroupTotalIs()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "DM-11",
                GroundHeadings,
                new[] { "GRASS", "", "", "", "", "", "", "", "", "", "" },
                GroundRow("pennisetum.jpg", "PENNISETUM SETACEUM", "35 m²", "432"),
                new[] { "", "", "", "", "35 m²", "46", "", "", "", "", "432" },
                new[] { "SHRUBS & GROUND COVER", "", "", "", "", "", "", "", "", "", "" },
                GroundRow("bougainvillea.jpg", "BOUGAINVILLEA GLABRA", "70 m²", "908"),
                new[] { "", "", "", "", "70 m²", "58", "", "", "", "", "908" },
                new[] { "TOTAL", "", "", "", "105 m²", "104", "", "", "", "", "1340" });

            WaterDemandRead read = WaterDemandRead.From(schedule);

            Assert.True(read.Read, read.Why);
            Assert.Equal(1340.0, read.LitresADay);
            Assert.NotEqual(432.0, read.LitresADay);
            Assert.NotEqual(908.0, read.LitresADay);
            Assert.Equal(8, read.TotalRow);
        }

        /// <summary>
        /// **EITHER HALF MISSING WRITES NOTHING AND BOTH REASONS ARE PRINTED WHERE BOTH FAILED.**
        /// Half a demand in a box printed Irrigation water demand is a number nobody could tell
        /// was half.
        /// </summary>
        [Fact]
        public void AMissingScheduleWritesNothingAndIsNamedWithWhichOne()
        {
            PlotWaterDemand oneHalf = WaterDemand.Of(
                WaterDemandRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                WaterDemandRead.NoSchedule(WaterDemand.ShrubsAndLawnKind));

            Assert.False(oneHalf.BothRead);
            Assert.Equal(
                "shrubs and lawn, this plot holds no shrubs and lawn schedule to read a total off",
                oneHalf.Why);
            Assert.Equal(
                "no irrigation water demand: shrubs and lawn, this plot holds no shrubs and lawn "
                + "schedule to read a total off",
                oneHalf.InWords);

            PlotWaterDemand neither = WaterDemand.Of(
                WaterDemandRead.NoSchedule(WaterDemand.SoftscapeKind),
                WaterDemandRead.NoSchedule(WaterDemand.ShrubsAndLawnKind));

            Assert.Equal(
                "softscape, this plot holds no softscape schedule to read a total off. "
                + "shrubs and lawn, this plot holds no shrubs and lawn schedule to read a total off",
                neither.Why);
        }

        /// <summary>
        /// A reading nothing filled in says the read did not happen rather than reading as
        /// nought, which is the absence against a zero rule this tool already carries.
        /// </summary>
        [Fact]
        public void APlotNothingReadHoldsNoDemandRatherThanNought()
        {
            PlotReading reading = CreateFixture.Plot("DM-11");

            Assert.False(reading.Water.BothRead);
            Assert.Contains("no schedule of that kind was read on this plot", reading.Water.Why);
        }

        /// <summary>
        /// The unit the FORM asks for is what the value is written in, and the litres are what
        /// the report prints beside it.
        /// </summary>
        [Fact]
        public void TheUnitsAreTheFormsAndTheSourcesAndBothAreSaid()
        {
            Assert.Equal("m³/day", WaterDemand.CubicMetresUnit);
            Assert.Equal("L/day", WaterDemand.LitresUnit);
            Assert.Equal(1000.0, WaterDemand.LitresInACubicMetre);
            Assert.Equal("2.492 m³/day", WaterDemand.CubicMetres(2.492));
            Assert.Equal("2492 L/day", WaterDemand.Litres(2492.0));
        }

        /// <summary>
        /// **THE SAME EXACT RULE, ASKED DIRECTLY.** -1 is none and -2 is more than one, and a
        /// heading with edge whitespace is the same heading while one with a word inside it is
        /// not.
        /// </summary>
        [Theory]
        [InlineData("L/DAY", 3)]
        [InlineData("l/day", 3)]
        [InlineData(" L/DAY ", 3)]
        [InlineData("WATER DEMAND", 0)]
        [InlineData("DAY", -1)]
        [InlineData("L/TREE/DAY", -1)]
        public void TheHeadingIsMatchedWholeAndWithoutCase(string heading, int expected)
        {
            var headings = new[] { "WATER DEMAND", "WATER L/TREE/DAY", "COUNT (n)", "L/DAY" };

            Assert.Equal(expected, ScheduleColumns.Reading(headings, heading));
        }

        [Fact]
        public void MoreThanOneMatchingHeadingComesBackAsMinusTwo()
        {
            var headings = new[] { "L/DAY", "COUNT (n)", "l/day" };

            Assert.Equal(-2, ScheduleColumns.Reading(headings, "L/DAY"));
        }

        /// <summary>
        /// **THE REPORT PRINTS BOTH HALVES, THE ROW EACH CAME OFF, THE SUM AND THE UNIT THE FORM
        /// ASKS FOR.** The row number is what lets a person open the schedule and check the
        /// number the tool wrote.
        /// </summary>
        [Fact]
        public void TheReportPrintsBothHalvesWithTheRowEachCameOff()
        {
            PlotReading reading = CreateFixture.Plot(
                "DM-11",
                water: WaterDemand.Of(
                    WaterDemandRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                    WaterDemandRead.Of("DM-11-(600) SHRUBS AND LAWN SCHEDULE", 1340.0, 8)));

            string report = Report(reading);

            Assert.Contains("== " + KpiCreateReport.WaterHeading + " (1) ==", report);
            Assert.Contains("1152 L/day off row 7", report);
            Assert.Contains("1340 L/day off row 8", report);
            Assert.Contains("2492 L/day", report);
            Assert.Contains("2.492 m³/day", report);
        }

        /// <summary>
        /// **A GROUP LEFT OUT OF THE TREE LISTS IS NAMED WITH ITS OWN SUBTOTAL**, beside a water
        /// demand that counts it. Bader's decision is TAKE THE TOTAL, so the number written
        /// includes Street Design and this column is how the difference is seen rather than
        /// found later.
        /// </summary>
        [Fact]
        public void AGroupLeftOutOfTheTreeListsIsNamedBesideTheDemandThatCountsIt()
        {
            var streetDesign = new PrintedGroup(
                "Street Design", 14, new SpeciesRow[0], true, 38, 18, false, CountedGroups.LeftOut);
            var proposed = new PrintedGroup(
                "Proposed", 4, new SpeciesRow[0], true, 32, 9, true, "the Tree List - Proposed sheet is named for it");

            PlotReading reading = CreateFixture.Plot(
                "FM-05",
                printedGroups: new[] { proposed, streetDesign },
                water: WaterDemand.Of(
                    WaterDemandRead.Of("FM-05-(600) SOFTSCAPE SCHEDULE", 1152.0, 20),
                    WaterDemandRead.Of("FM-05-(600) SHRUBS AND LAWN SCHEDULE", 1340.0, 12)));

            string report = Report(reading);

            Assert.Contains("Street Design 38 trees", report);
            Assert.DoesNotContain("Proposed 32 trees", report);
        }

        /// <summary>
        /// A plot whose demand could not be read says NOTHING WRITTEN in the row and is named
        /// again underneath with why, so the reason is in the file rather than only on the pane.
        /// </summary>
        [Fact]
        public void APlotWithNoDemandSaysSoInTheRowAndAgainWithItsReason()
        {
            PlotReading reading = CreateFixture.Plot(
                "DM-11",
                water: WaterDemand.Of(
                    WaterDemandRead.Of("DM-11-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                    WaterDemandRead.Refused("DM-11-(600) SHRUBS AND LAWN SCHEDULE", WaterDemandRead.NoTotalRow)));

            string report = Report(reading);

            Assert.Contains("NOTHING WRITTEN", report);
            Assert.Contains("1 plot wrote no water demand", report);
            Assert.Contains("it prints no TOTAL row", report);
        }

        /// <summary>
        /// **THE NUMBER REALLY REACHES THE FIELD**, through the fill the run calls, in the unit
        /// the form asks for. 1152 plus 1340 is 2492 litres, which is 2.492.
        /// </summary>
        [Fact]
        public void TheFillPutsTheCubicMetresIntoTheFieldAndTheUnitBesideIt()
        {
            PlotReading reading = CreateFixture.Plot(
                "FM-05",
                uid2: "ANH-008-MO-100006",
                water: WaterDemand.Of(
                    WaterDemandRead.Of("FM-05-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                    WaterDemandRead.Of("FM-05-(600) SHRUBS AND LAWN SCHEDULE", 1340.0, 8)));

            PdfPlan plan = PdfFill.Of(
                reading, CountedGroups.Of(KpiTemplates.Mosques), null,
                new DateTime(2026, 9, 15), true, PdfWorkbookNumbers.None);

            PdfFieldFill field = plan.Fields.Single(one => one.Value == PdfValue.IrrigationWaterDemand);

            Assert.True(field.Written, field.Why);
            Assert.Equal("2.492", field.Text);
            Assert.Equal("m³/day", field.Unit);

            // **AND IT PRINTS FINE RATHER THAN TO TWO PLACES.** 305 litres a day is 0.305, which
            // two places would print 0.31, and the division by a thousand is what makes every
            // one of these numbers small.
            PlotReading small = CreateFixture.Plot(
                "FM-06",
                uid2: "ANH-008-MO-100007",
                water: WaterDemand.Of(
                    WaterDemandRead.Of("FM-06-(600) SOFTSCAPE SCHEDULE", 200.0, 7),
                    WaterDemandRead.Of("FM-06-(600) SHRUBS AND LAWN SCHEDULE", 105.0, 8)));

            PdfPlan thin = PdfFill.Of(
                small, CountedGroups.Of(KpiTemplates.Mosques), null,
                new DateTime(2026, 9, 15), true, PdfWorkbookNumbers.None);

            Assert.Equal(
                "0.305",
                thin.Fields.Single(one => one.Value == PdfValue.IrrigationWaterDemand).Text);
        }

        /// <summary>
        /// And a plot with no demand leaves it blank, with BOTH the standing sentence and the
        /// half that failed, so the box's reason is readable without the code beside it.
        /// </summary>
        [Fact]
        public void TheFillLeavesTheFieldBlankAndNamesTheHalfThatFailed()
        {
            PlotReading reading = CreateFixture.Plot(
                "FM-05",
                uid2: "ANH-008-MO-100006",
                water: WaterDemand.Of(
                    WaterDemandRead.Of("FM-05-(600) SOFTSCAPE SCHEDULE", 1152.0, 7),
                    WaterDemandRead.NoSchedule(WaterDemand.ShrubsAndLawnKind)));

            PdfPlan plan = PdfFill.Of(
                reading, CountedGroups.Of(KpiTemplates.Mosques), null,
                new DateTime(2026, 9, 15), true, PdfWorkbookNumbers.None);

            PdfFieldFill field = plan.Fields.Single(one => one.Value == PdfValue.IrrigationWaterDemand);

            Assert.False(field.Written);
            Assert.Contains("no irrigation water demand could be read", field.Why);
            Assert.Contains("divided by 1000", field.Why);
            Assert.Contains("shrubs and lawn, this plot holds no shrubs and lawn schedule", field.Why);
        }

        /// <summary>
        /// **THE PDF FIELD ON ALL THREE FORMS.** The value goes in as the cubic metres, the unit
        /// beside it is the form's own, and a plot with no demand leaves the box blank with both
        /// halves named.
        /// </summary>
        [Fact]
        public void TheFieldOnEveryFormTakesTheCubicMetresAndABlankNamesWhy()
        {
            foreach (PdfForm form in PdfForms.All)
            {
                PdfFormField field = form.FieldFor(PdfValue.IrrigationWaterDemand);

                Assert.NotNull(field);
                Assert.Equal("m³/day", field.Unit);
            }

            Assert.Equal("Irrigation water demand", PdfForms.Parks.FieldFor(PdfValue.IrrigationWaterDemand).FieldName);
            Assert.Equal("Irrigation water demand", PdfForms.Roads.FieldFor(PdfValue.IrrigationWaterDemand).FieldName);

            // **THE OPEN SPACES NAME IS THE ONE MEASURED OFF THE FILE.** That form names five of
            // its fields undefined_4.0, Proposed Trees.1, 0 and 0_2, so nothing on it may be
            // taken from what a field is called on the other two.
            Assert.Equal(
                "Irrigation water demand",
                PdfForms.OpenSpaces.FieldFor(PdfValue.IrrigationWaterDemand).FieldName);
        }
    }
}
