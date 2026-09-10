using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// What a grouped schedule really prints, measured on the 0928 run over 20 mosque plots.
    ///
    /// **A group prints ONE SUBTOTAL PER PHASE, then the GROUP TOTAL.** The old rule came off
    /// DM-11 alone, where every group holds one phase and so prints two equal rows, and it read
    /// that as one subtotal printed twice. Taking the first row then took one phase and called
    /// it the group: 30 where the group is 84, and 361 where it is 820.
    ///
    /// Every expected value below is written out by hand off the run report, not worked out
    /// with the rule the code uses.
    /// </summary>
    public class SubtotalShapeTests
    {
        private static readonly string[] Headings =
            { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" };

        private static string[] Species(string name, double area, int count)
        {
            return new[] { name + ".jpg", name, Metres(area), count.ToString() };
        }

        private static string[] Subtotal(double area, int count)
        {
            return new[] { string.Empty, string.Empty, Metres(area), count.ToString() };
        }

        private static string[] Structure(string text)
        {
            return new[] { text, string.Empty, string.Empty, string.Empty };
        }

        private static string Metres(double area)
        {
            return area.ToString("0.##") + " m²";
        }

        private static GroupSubtotal Only(string plot, string heading, params string[][] rows)
        {
            var all = new List<string[]> { Headings, Structure(heading) };
            all.AddRange(rows);

            return Assert.Single(ShrubsAndLawnRows.SubtotalsIn(
                CreateFixture.ShrubsAndLawn(plot, all.ToArray()),
                new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }));
        }

        /// <summary>
        /// DM-16 SHRUBS AND GROUND COVER: 30 over 39, then 54 over 69, then 84 over 108.
        /// 30 plus 54 is 84 and 39 plus 69 is 108. The old rule took 30.
        /// </summary>
        [Fact]
        public void Dm16ShrubsIsEightyFourAndNotThirty()
        {
            GroupSubtotal group = Only(
                "DM-16",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 30.0, 39),
                Subtotal(30.0, 39),
                Structure("Proposed"),
                Species("Bougainvillea", 54.0, 69),
                Subtotal(54.0, 69),
                Subtotal(84.0, 108));

            Assert.Equal(84.0, group.SquareMetres);
            Assert.Equal(108, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(3, group.Repeats);
        }

        /// <summary>
        /// DM-25 SHRUBS AND GROUND COVER: 13 over 9, then 228 over 286, then 241 over 295.
        /// 13 plus 228 is 241 and 9 plus 286 is 295. The old rule took 13.
        /// </summary>
        [Fact]
        public void Dm25ShrubsIsTwoHundredAndFortyOneAndNotThirteen()
        {
            GroupSubtotal group = Only(
                "DM-25",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 13.0, 9),
                Subtotal(13.0, 9),
                Structure("Proposed"),
                Species("Bougainvillea", 228.0, 286),
                Subtotal(228.0, 286),
                Subtotal(241.0, 295));

            Assert.Equal(241.0, group.SquareMetres);
            Assert.Equal(295, group.ItemCount);
            Assert.True(group.Agrees);
        }

        /// <summary>
        /// FM-05 GRASS: 96 over 117, then 69 over 84, then 165 over 201.
        /// 96 plus 69 is 165 and 117 plus 84 is 201. The old rule took 96.
        /// </summary>
        [Fact]
        public void Fm05GrassIsOneHundredAndSixtyFiveAndNotNinetySix()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.LawnHeading,
                Structure("Existing"),
                Species("Pennisetum", 96.0, 117),
                Subtotal(96.0, 117),
                Structure("Proposed"),
                Species("Cynodon", 69.0, 84),
                Subtotal(69.0, 84),
                Subtotal(165.0, 201));

            Assert.Equal(165.0, group.SquareMetres);
            Assert.Equal(201, group.ItemCount);
            Assert.True(group.Agrees);
        }

        /// <summary>
        /// FM-05 SHRUBS AND GROUND COVER: 361 over 450, then 459 over 570, then 820 over 1020.
        /// 361 plus 459 is 820 and 450 plus 570 is 1020. The old rule took 361.
        /// </summary>
        [Fact]
        public void Fm05ShrubsIsEightHundredAndTwentyAndNotThreeHundredAndSixtyOne()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 361.0, 450),
                Subtotal(361.0, 450),
                Structure("Proposed"),
                Species("Carissa", 459.0, 570),
                Subtotal(459.0, 570),
                Subtotal(820.0, 1020));

            Assert.Equal(820.0, group.SquareMetres);
            Assert.Equal(1020, group.ItemCount);
            Assert.True(group.Agrees);
        }

        /// <summary>
        /// **The one phase case, which is what DM-11 is and what the old rule was written from.**
        /// Two equal rows, and the answer is that number either way, so the plot the old rule
        /// came off was the one plot it could not be wrong on.
        /// </summary>
        [Fact]
        public void AGroupHoldingOnePhasePrintsTwoEqualRowsAndTheAnswerIsThatNumber()
        {
            GroupSubtotal group = Only(
                "DM-11",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 35.0, 46),
                Subtotal(35.0, 46),
                Subtotal(35.0, 46));

            Assert.Equal(35.0, group.SquareMetres);
            Assert.Equal(46, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(2, group.Repeats);
        }

        /// <summary>
        /// Three phases, which the shape allows and no plot has been seen to print. The rule is
        /// the same one and nothing in it counts phases: the last row is the group and the rows
        /// above it must add to it.
        /// </summary>
        [Fact]
        public void AGroupHoldingThreePhasesPrintsFourRowsAndTheLastIsStillTheGroup()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 10.0, 4),
                Subtotal(10.0, 4),
                Structure("Proposed"),
                Species("Carissa", 20.0, 7),
                Subtotal(20.0, 7),
                Structure("Demolished"),
                Species("Lantana", 5.0, 2),
                Subtotal(5.0, 2),
                Subtotal(35.0, 13));

            Assert.Equal(35.0, group.SquareMetres);
            Assert.Equal(13, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(4, group.Repeats);
        }

        /// <summary>
        /// A last row that does not equal the rows above it is a real disagreement, and it
        /// refuses the write rather than being picked between. 30 plus 54 is 84, not 90.
        /// </summary>
        [Fact]
        public void AGroupTotalThatDoesNotEqualItsPhasesRefuses()
        {
            GroupSubtotal group = Only(
                "DM-16",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 30.0, 39),
                Subtotal(30.0, 39),
                Structure("Proposed"),
                Species("Bougainvillea", 54.0, 69),
                Subtotal(54.0, 69),
                Subtotal(90.0, 108));

            Assert.False(group.Agrees);
            Assert.Contains("its group total reads 90 over 108", group.Disagreement);
            Assert.Contains("2 rows above it add to 84 over 108", group.Disagreement);
        }

        /// <summary>
        /// The item count is checked as well as the area, because a group total right in metres
        /// and wrong in items is still a schedule that does not add up.
        /// </summary>
        [Fact]
        public void AnItemCountThatDoesNotAddUpRefusesTooEvenWhenTheAreaDoes()
        {
            GroupSubtotal group = Only(
                "DM-16",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 30.0, 39),
                Subtotal(30.0, 39),
                Structure("Proposed"),
                Species("Bougainvillea", 54.0, 69),
                Subtotal(54.0, 69),
                Subtotal(84.0, 999));

            Assert.False(group.Agrees);
            Assert.Contains("84 over 999", group.Disagreement);
            Assert.Contains("add to 84 over 108", group.Disagreement);
        }

        /// <summary>
        /// A group printing one row has nothing above it to check against. It is taken rather
        /// than refused, because a check with no subject is not a failure of the schedule.
        /// </summary>
        [Fact]
        public void AGroupPrintingOneRowIsTakenWithNothingToCheckItAgainst()
        {
            GroupSubtotal group = Only(
                "DM-11",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 35.0, 46),
                Subtotal(35.0, 46));

            Assert.Equal(35.0, group.SquareMetres);
            Assert.True(group.Agrees);
            Assert.Equal(1, group.Repeats);
        }
    }

    /// <summary>
    /// The checklist report says how long the run took.
    ///
    /// **The 0928 run over 20 plots took about five minutes and no file recorded a duration.**
    /// The scan report has carried elements and seconds at the top since its first round, so
    /// the pattern existed and only this half of the tool was missing it.
    /// </summary>
    public class RunTimingTests
    {
        private static readonly System.DateTime At =
            new System.DateTime(2026, 9, 10, 9, 28, 0, System.DateTimeKind.Unspecified);

        private static string ReportFor(RunTiming timing, double plotSeconds)
        {
            return KpiCreateReport.Write(
                CreateFixture.Run(
                    new[] { CreateFixture.Plot("DM-12", readSeconds: plotSeconds) },
                    timing: timing),
                At);
        }

        [Fact]
        public void TheHeaderSaysTheWholeRunTheReadAndWhatIsLeft()
        {
            string report = ReportFor(RunTiming.Of(312.4, 298.1), 15.2);

            Assert.Contains(
                "Run: 312.4 seconds, of which reading the model took 298.1 seconds and "
                + "everything after it 14.3 seconds.",
                report);
        }

        [Fact]
        public void EveryPlotCarriesItsOwnRead()
        {
            Assert.Contains("read in 15.2 seconds", ReportFor(RunTiming.Of(312.4, 298.1), 15.2));
        }

        /// <summary>
        /// A run nothing timed says so rather than printing nought seconds, because a zero reads
        /// as an answer and this one would mean a five minute run took no time at all.
        /// </summary>
        [Fact]
        public void ARunThatWasNotTimedSaysSoRatherThanPrintingNought()
        {
            string report = ReportFor(RunTiming.NotTimed, 0.0);

            Assert.Contains("Run: NOT TIMED. Nothing recorded a duration for this run.", report);
            Assert.DoesNotContain("Run: 0.0 seconds", report);
        }

        /// <summary>
        /// The read and the rest are two numbers rather than one, because the read is the part
        /// that grows with the plots ticked and a total on its own cannot say which half to
        /// look at. Written out by hand: 312.4 less 298.1 is 14.3.
        /// </summary>
        [Fact]
        public void TheRestIsTheWholeLessTheRead()
        {
            RunTiming timing = RunTiming.Of(312.4, 298.1);

            Assert.True(timing.WasTimed);
            Assert.Equal(14.3, timing.RestSeconds, 3);
            Assert.False(RunTiming.NotTimed.WasTimed);
        }
    }
}
