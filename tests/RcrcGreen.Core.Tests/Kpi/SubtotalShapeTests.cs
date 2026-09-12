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
    /// it the group: 30 where the group is 84.
    ///
    /// **The phases a tree list sheet is named for are added, and the group total is the
    /// check.** FM-05 prints a third phase, Street Design, which is somebody else's scope by
    /// Bader's decision, so its row is left out and named: GRASS 96 taken, 69 left out, 165
    /// printed. The rule before this one took the group total, 165, and counted the street.
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

            return Assert.Single(ShrubsAndLawnRows.Read(
                CreateFixture.ShrubsAndLawn(plot, all.ToArray()),
                new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading },
                CreateFixture.Counted, ProjectUnit.Unknown).Subtotals);
        }

        /// <summary>
        /// The project that measured FM-21 and FM-22 rounds areas to the metre, which its own
        /// scan prints as rounded to 1.
        /// </summary>
        private static readonly ProjectUnit RoundedToTheMetre =
            new ProjectUnit("Square meters", "autodesk.unit.unit:squareMeters-1.0.1", 1.0);

        private static GroupSubtotal OnlyRounded(string plot, string heading, ProjectUnit unit, params string[][] rows)
        {
            var all = new List<string[]> { Headings, Structure(heading) };
            all.AddRange(rows);

            return Assert.Single(ShrubsAndLawnRows.Read(
                CreateFixture.ShrubsAndLawn(plot, all.ToArray()),
                new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading },
                CreateFixture.Counted, unit).Subtotals);
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
        /// FM-05 GRASS as the 1536 report printed it: Proposed 96 over 117, then Street Design
        /// 69 over 84, then 165 over 201. 96 plus 69 is 165 and 117 plus 84 is 201, so the
        /// group total checks, and the value is the one phase a tree list sheet is named for:
        /// 96. The rule before this one wrote 165 and counted the street.
        /// </summary>
        [Fact]
        public void Fm05GrassIsNinetySixWithStreetDesignLeftOutAndTheGroupTotalChecked()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 96.0, 117),
                Subtotal(96.0, 117),
                Structure("Street Design"),
                Species("Cynodon", 69.0, 84),
                Subtotal(69.0, 84),
                Subtotal(165.0, 201));

            Assert.Equal(96.0, group.SquareMetres);
            Assert.Equal(117, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(3, group.Repeats);
            Assert.True(group.GroupTotalPrinted);
            Assert.Equal(165.0, group.GroupTotalSquareMetres);
            Assert.Equal(201, group.GroupTotalItemCount);

            PhaseSubtotal left = Assert.Single(group.PhasesLeftOut);
            Assert.Equal("Street Design", left.Name);
            Assert.Equal(69.0, left.SquareMetres);
            Assert.Equal(84, left.ItemCount);
            Assert.Equal(8, left.RowNumber);
            Assert.Equal("no tree list sheet is named for it, so it is out of scope", left.Why);
        }

        /// <summary>
        /// FM-05 SHRUBS AND GROUND COVER: Proposed 361 over 450, then Street Design 459 over
        /// 570, then 820 over 1020. 361 plus 459 is 820 and 450 plus 570 is 1020. The value is
        /// 361 and the rule before this one wrote 820.
        /// </summary>
        [Fact]
        public void Fm05ShrubsIsThreeHundredAndSixtyOneWithStreetDesignLeftOut()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.ShrubsHeading,
                Structure("Proposed"),
                Species("Acacia", 361.0, 450),
                Subtotal(361.0, 450),
                Structure("Street Design"),
                Species("Carissa", 459.0, 570),
                Subtotal(459.0, 570),
                Subtotal(820.0, 1020));

            Assert.Equal(361.0, group.SquareMetres);
            Assert.Equal(450, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(2, group.Phases.Count);
            Assert.Equal("Proposed", group.Phases[0].Name);
            Assert.True(group.Phases[0].Counted);
            Assert.Equal("Tree List - Proposed is named for it", group.Phases[0].Why);
            Assert.False(group.Phases[1].Counted);
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
        /// Three phases, two of them the ones a sheet is named for. The two are added, 10 plus
        /// 20 is 30 and 4 plus 7 is 11, the third is left out and named, and the group total
        /// is checked against all three: 10 plus 20 plus 5 is 35 and 4 plus 7 plus 2 is 13.
        /// </summary>
        [Fact]
        public void AGroupHoldingAThirdPhaseAddsTheTwoNamedForAndLeavesTheThirdOut()
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

            Assert.Equal(30.0, group.SquareMetres);
            Assert.Equal(11, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Equal(4, group.Repeats);
            Assert.Equal("Demolished", Assert.Single(group.PhasesLeftOut).Name);
        }

        /// <summary>
        /// A group total that does not equal every phase added, the ones left out included, is
        /// a schedule that does not add up, whatever the phases taken come to. 96 plus 69 is
        /// 165, not 170.
        /// </summary>
        [Fact]
        public void AGroupTotalIsCheckedAgainstThePhasesLeftOutAsWellAsThoseTaken()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 96.0, 117),
                Subtotal(96.0, 117),
                Structure("Street Design"),
                Species("Cynodon", 69.0, 84),
                Subtotal(69.0, 84),
                Subtotal(170.0, 201));

            Assert.Equal(96.0, group.SquareMetres);
            Assert.False(group.Agrees);
            Assert.Contains("its group total reads 170 over 201", group.Disagreement);
            Assert.Contains("2 rows above it add to 165 over 201", group.Disagreement);
            Assert.Contains("Every row: Proposed 96 over 117, Street Design 69 over 84, 170 over 201", group.Disagreement);
        }

        /// <summary>
        /// A group whose every phase is out of scope is nought, said so, with the group total
        /// still checked. Nothing is taken because nothing was left.
        /// </summary>
        [Fact]
        public void AGroupHoldingOnlyAPhaseLeftOutIsNoughtAndSaysSo()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.LawnHeading,
                Structure("Street Design"),
                Species("Cynodon", 69.0, 84),
                Subtotal(69.0, 84),
                Subtotal(69.0, 84));

            Assert.Equal(0.0, group.SquareMetres);
            Assert.Equal(0, group.ItemCount);
            Assert.True(group.Agrees);
            Assert.Single(group.PhasesLeftOut);
            Assert.Equal(69.0, group.GroupTotalSquareMetres);
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
        /// <summary>
        /// **FM-21 off the 1208 run on RCRC_NG03_EZ, measured, and the refusal that was wrong.**
        /// Existing 2 over 0, Proposed 51 over 11, group total 52 over 11. The counts match
        /// exactly and 2 plus 51 is 53 against a printed 52, because the project rounds areas
        /// to the metre and a sum of rounded numbers need not equal a rounded sum. Two rows
        /// rounded to 1 may be off by up to 1, so this goes through and is noted.
        /// </summary>
        [Fact]
        public void Fm21OffByOneWithinTheMetreIsNotedRatherThanRefused()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(52.0, 11));

            Assert.True(group.Agrees);
            Assert.Equal(string.Empty, group.Disagreement);
            Assert.Equal(
                "the 2 rows above it add to 53 over 11 against its printed 52 over 11, "
                + "off by 1 in area with every count exact, within the 1 that 2 rows rounded to 1 allow, "
                + "so it is noted rather than refused",
                group.RoundingNote);
            Assert.Equal(53.0, group.SquareMetres);
            Assert.Equal(11, group.ItemCount);
        }

        /// <summary>
        /// **FM-22 off the same run**, one high where FM-21 is one low. Existing 2 over 0,
        /// Proposed 80 over 46, group total 83 over 46, and 2 plus 80 is 82.
        /// </summary>
        [Fact]
        public void Fm22OffByOneTheOtherWayIsNotedToo()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-22",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 80.0, 46),
                Subtotal(80.0, 46),
                Subtotal(83.0, 46));

            Assert.True(group.Agrees);
            Assert.Equal(
                "the 2 rows above it add to 82 over 46 against its printed 83 over 46, "
                + "off by 1 in area with every count exact, within the 1 that 2 rows rounded to 1 allow, "
                + "so it is noted rather than refused",
                group.RoundingNote);
        }

        /// <summary>
        /// The room is the unit's, never a constant: rounding to 0.01 gets a tighter allowance,
        /// so the same off by one refuses there, naming the room it is outside of.
        /// </summary>
        [Fact]
        public void AProjectRoundingToACentimetreAllowsNoMetre()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                new ProjectUnit("Square meters", "id", 0.01),
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(52.0, 11));

            Assert.False(group.Agrees);
            Assert.Equal(string.Empty, group.RoundingNote);
            Assert.Contains("add to 53 over 11, which is more than the 0.01 that 2 rows rounded to 0.01 allow", group.Disagreement);
        }

        /// <summary>
        /// A count is an integer and gets no room at all. The same areas with the group total
        /// count one high still refuse, whatever the rounding step.
        /// </summary>
        [Fact]
        public void ACountOffByOneStillRefusesWhateverTheRounding()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(53.0, 12));

            Assert.False(group.Agrees);
            Assert.Equal(string.Empty, group.RoundingNote);
            Assert.Contains("its group total reads 53 over 12 and the 2 rows above it add to 53 over 11", group.Disagreement);
            Assert.DoesNotContain("rounded to", group.Disagreement);
        }

        /// <summary>
        /// Outside the room still refuses: 84 against 90 is off by 6 where 2 rows rounded to
        /// the metre allow 1, and the refusal names the room so a person sees why the note was
        /// not enough.
        /// </summary>
        [Fact]
        public void OffBySixIsOutsideTheMetresRoomAndStillRefuses()
        {
            GroupSubtotal group = OnlyRounded(
                "DM-16",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Existing"),
                Species("Acacia", 30.0, 39),
                Subtotal(30.0, 39),
                Structure("Proposed"),
                Species("Bougainvillea", 54.0, 69),
                Subtotal(54.0, 69),
                Subtotal(90.0, 108));

            Assert.False(group.Agrees);
            Assert.Equal(string.Empty, group.RoundingNote);
            Assert.Contains("add to 84 over 108, which is more than the 1 that 2 rows rounded to 1 allow", group.Disagreement);
        }

        /// <summary>
        /// A step nobody read allows nothing, because a check that cannot see its subject must
        /// not quietly widen. The refusal says so, so the FM-21 shape on a project whose unit
        /// was not read is traced in one line.
        /// </summary>
        [Fact]
        public void AStepThatWasNotReadAllowsNothingAndSaysSo()
        {
            GroupSubtotal group = Only(
                "FM-21",
                KpiMerge.ShrubsHeading,
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(52.0, 11));

            Assert.False(group.Agrees);
            Assert.Equal(string.Empty, group.RoundingNote);
            Assert.Contains(
                "add to 53 over 11, and the project's area rounding step was not read, so no rounding room was allowed",
                group.Disagreement);
        }

        /// <summary>
        /// One row rounded to the metre allows half, said in the singular, so a one phase
        /// group off by one refuses while the two phase groups above go through.
        /// </summary>
        [Fact]
        public void AOnePhaseGroupOffByOneIsOutsideItsHalfMetre()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(52.0, 11));

            Assert.False(group.Agrees);
            Assert.Contains("add to 51 over 11, which is more than the 0.5 that 1 row rounded to 1 allows", group.Disagreement);
        }

        /// <summary>
        /// The note is a line in the report, beside the group total row it is about, so a real
        /// fault growing slowly is visible while correct data goes through. The row numbers
        /// count the heading row as 1.
        /// </summary>
        [Fact]
        public void TheReportPrintsTheNoteBesideTheGroupTotalRow()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                RoundedToTheMetre,
                Structure("Existing"),
                Species("Acacia", 2.0, 0),
                Subtotal(2.0, 0),
                Structure("Proposed"),
                Species("Bougainvillea", 51.0, 11),
                Subtotal(51.0, 11),
                Subtotal(52.0, 11));

            string report = KpiCreateReport.Write(
                CreateFixture.Run(new[] { CreateFixture.Plot("FM-21", subtotals: new[] { group }) }),
                new System.DateTime(2026, 9, 12, 12, 8, 0));

            Assert.Contains(
                "        group total row 9 prints 52 over 11, and the 2 rows above it add to 53 over 11 "
                + "against its printed 52 over 11, off by 1 in area with every count exact, "
                + "within the 1 that 2 rows rounded to 1 allow, so it is noted rather than refused",
                report);
        }

        /// <summary>
        /// The species rows against the group's own value, recorded rather than enforced, the
        /// forty seventh pass's decision. The docstring said printed and nothing printed it,
        /// which the review of every summed check found, so the record finally exists: 96 plus
        /// 69 is 165 against a group total row printing 170.
        /// </summary>
        [Fact]
        public void TheSpeciesRowsRecordFinallyPrintsWhenTheyDisagree()
        {
            GroupSubtotal group = Only(
                "FM-05",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 96.0, 117),
                Subtotal(96.0, 117),
                Structure("Street Design"),
                Species("Cynodon", 69.0, 84),
                Subtotal(69.0, 84),
                Subtotal(170.0, 201));

            string report = KpiCreateReport.Write(
                CreateFixture.Run(new[] { CreateFixture.Plot("FM-05", subtotals: new[] { group }) }),
                new System.DateTime(2026, 9, 12, 12, 8, 0));

            Assert.Contains(
                "        its species rows add to 165 in area against the 170 its group total row prints, "
                + "recorded rather than enforced, because every printed area is already rounded",
                report);
        }

        /// <summary>
        /// A phased group that prints no total row gives the check nothing to hold the phase
        /// rows against. It is still taken, but taken silently it reads exactly like a group
        /// whose total was checked and agreed, so the report says the check never ran.
        /// </summary>
        [Fact]
        public void APhasedGroupWithNoTotalRowIsSaidToGoUnchecked()
        {
            GroupSubtotal group = Only(
                "DM-11",
                KpiMerge.LawnHeading,
                Structure("Proposed"),
                Species("Pennisetum", 35.0, 46),
                Subtotal(35.0, 46));

            string report = KpiCreateReport.Write(
                CreateFixture.Run(new[] { CreateFixture.Plot("DM-11", subtotals: new[] { group }) }),
                new System.DateTime(2026, 9, 12, 12, 8, 0));

            Assert.True(group.Agrees);
            Assert.Contains(
                "        the group printed no total row after its phase rows, "
                + "so nothing checked what they add to",
                report);
        }

        /// <summary>
        /// The note's numbers print at the scale of the step they are held against. On a
        /// project rounding to 0.001 the old two place format said off by 0 within the 0 it
        /// allows, a sentence at war with itself over a comparison the code got right.
        /// </summary>
        [Fact]
        public void ANoteOnAFineStepPrintsItsNumbersRatherThanNought()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-21",
                KpiMerge.ShrubsHeading,
                new ProjectUnit("Square meters", "id", 0.001),
                Structure("Existing"),
                new[] { "Acacia.jpg", "Acacia", "10.001 m²", "3" },
                new[] { string.Empty, string.Empty, "10.001 m²", "3" },
                Structure("Proposed"),
                new[] { "Bougainvillea.jpg", "Bougainvillea", "20.003 m²", "4" },
                new[] { string.Empty, string.Empty, "20.003 m²", "4" },
                new[] { string.Empty, string.Empty, "30.003 m²", "7" });

            Assert.True(group.Agrees);
            Assert.Equal(
                "the 2 rows above it add to 30.004 over 7 against its printed 30.003 over 7, "
                + "off by 0.001 in area with every count exact, within the 0.001 that 2 rows rounded to 0.001 allow, "
                + "so it is noted rather than refused",
                group.RoundingNote);
        }

        /// <summary>
        /// The species record speaks on any real difference, with numbers fine enough to show
        /// it. The 0.005 that used to gate this line was a constant pretending to be a
        /// rounding room, and under it 169.996 against 170 was silently swallowed.
        /// </summary>
        [Fact]
        public void ASpeciesSumOffByLessThanACellIsStillRecorded()
        {
            GroupSubtotal group = OnlyRounded(
                "FM-05",
                KpiMerge.LawnHeading,
                RoundedToTheMetre,
                Structure("Proposed"),
                new[] { "Pennisetum.jpg", "Pennisetum", "96.246 m²", "117" },
                new[] { "Cynodon.jpg", "Cynodon", "73.75 m²", "84" },
                new[] { string.Empty, string.Empty, "169.996 m²", "201" },
                new[] { string.Empty, string.Empty, "170 m²", "201" });

            string report = KpiCreateReport.Write(
                CreateFixture.Run(new[] { CreateFixture.Plot("FM-05", subtotals: new[] { group }) }),
                new System.DateTime(2026, 9, 12, 12, 8, 0));

            Assert.Contains(
                "        its species rows add to 169.996 in area against the 170 its group total row prints, "
                + "recorded rather than enforced, because every printed area is already rounded",
                report);
        }

        /// <summary>
        /// A project rounding areas coarser than the metre is hypothetical, both measured
        /// models round to 1, and the room it earns has no ceiling yet, so the report says so
        /// at the top before anybody reads a number. The first project that prints this line
        /// hands the team a real figure to decide a ceiling against.
        /// </summary>
        [Fact]
        public void AStepCoarserThanTheMetreIsSaidBeforeAnyNumber()
        {
            string report = KpiCreateReport.Write(
                CreateFixture.Run(areaUnit: new ProjectUnit("Square meters", "id", 2.0)),
                new System.DateTime(2026, 9, 12, 14, 0, 0));

            Assert.Contains(
                "THE PROJECT ROUNDS AREAS COARSER THAN THE METRE: its step is 2, "
                + "so the group total check allows 1 square metre of room for every row summed.",
                report);
            Assert.True(
                report.IndexOf("COARSER THAN THE METRE", System.StringComparison.Ordinal)
                    < report.IndexOf("RECONCILIATION", System.StringComparison.Ordinal),
                "the coarse step line must come before the first number");
        }

        /// <summary>
        /// The room is half the step per row, so a step of 10 earns 5, said in the plural.
        /// </summary>
        [Fact]
        public void AStepOfTenSaysItsFiveInThePlural()
        {
            string report = KpiCreateReport.Write(
                CreateFixture.Run(areaUnit: new ProjectUnit("Square meters", "id", 10.0)),
                new System.DateTime(2026, 9, 12, 14, 0, 0));

            Assert.Contains(
                "THE PROJECT ROUNDS AREAS COARSER THAN THE METRE: its step is 10, "
                + "so the group total check allows 5 square metres of room for every row summed.",
                report);
        }

        /// <summary>
        /// The metre, anything finer and an unread step print no coarse line, because the
        /// line is a warning about room this project does not earn.
        /// </summary>
        [Fact]
        public void TheMetreAndFinerAndAnUnreadStepPrintNoCoarseLine()
        {
            var at = new System.DateTime(2026, 9, 12, 14, 0, 0);

            Assert.DoesNotContain("COARSER THAN THE METRE",
                KpiCreateReport.Write(CreateFixture.Run(areaUnit: RoundedToTheMetre), at));
            Assert.DoesNotContain("COARSER THAN THE METRE",
                KpiCreateReport.Write(CreateFixture.Run(areaUnit: new ProjectUnit("Square meters", "id", 0.01)), at));
            Assert.DoesNotContain("COARSER THAN THE METRE",
                KpiCreateReport.Write(CreateFixture.Run(), at));
        }

        /// <summary>
        /// A note only the report file holds is a note nobody reads, so the status line counts
        /// them beside the written cells. FM-21 and FM-22 off the 1208 run each earn one.
        /// </summary>
        [Fact]
        public void TheStatusLineSaysHowManyRoundingNotesTheRunMade()
        {
            GroupSubtotal fm21 = OnlyRounded(
                "FM-21", KpiMerge.ShrubsHeading, RoundedToTheMetre,
                Structure("Existing"), Species("Acacia", 2.0, 0), Subtotal(2.0, 0),
                Structure("Proposed"), Species("Bougainvillea", 51.0, 11), Subtotal(51.0, 11),
                Subtotal(52.0, 11));
            GroupSubtotal fm22 = OnlyRounded(
                "FM-22", KpiMerge.ShrubsHeading, RoundedToTheMetre,
                Structure("Existing"), Species("Acacia", 2.0, 0), Subtotal(2.0, 0),
                Structure("Proposed"), Species("Bougainvillea", 80.0, 46), Subtotal(80.0, 46),
                Subtotal(83.0, 46));

            KpiCreateRun both = CreateFixture.Run(
                readings: new[]
                {
                    CreateFixture.Plot("FM-21", subtotals: new[] { fm21 }),
                    CreateFixture.Plot("FM-22", subtotals: new[] { fm22 })
                },
                outcome: PatchOutcome.Done(2, 1, null,
                    new[] { new LandedCell("KPI CHECKLIST - R1", "E5", "2026-09-12") }, null));

            Assert.Contains(
                "not written. 2 rounding notes are in the report. Workbook:",
                CreateWords.Wrote(both, @"C:\reports\r.txt"));

            KpiCreateRun one = CreateFixture.Run(
                readings: new[] { CreateFixture.Plot("FM-21", subtotals: new[] { fm21 }) },
                outcome: PatchOutcome.Done(2, 1, null,
                    new[] { new LandedCell("KPI CHECKLIST - R1", "E5", "2026-09-12") }, null));

            Assert.Contains(
                "not written. 1 rounding note is in the report. Workbook:",
                CreateWords.Wrote(one, @"C:\reports\r.txt"));
        }

        /// <summary>
        /// A run whose group totals all added exactly says nothing about notes, because a
        /// line counting nought is noise beside the count that matters.
        /// </summary>
        [Fact]
        public void NoNotesPutNoNoteLineBesideTheWrittenCount()
        {
            GroupSubtotal exact = OnlyRounded(
                "DM-16", KpiMerge.ShrubsHeading, RoundedToTheMetre,
                Structure("Existing"), Species("Acacia", 30.0, 39), Subtotal(30.0, 39),
                Structure("Proposed"), Species("Bougainvillea", 54.0, 69), Subtotal(54.0, 69),
                Subtotal(84.0, 108));

            string line = CreateWords.Wrote(
                CreateFixture.Run(
                    readings: new[] { CreateFixture.Plot("DM-16", subtotals: new[] { exact }) },
                    outcome: PatchOutcome.Done(2, 1, null,
                        new[] { new LandedCell("KPI CHECKLIST - R1", "E5", "2026-09-12") }, null)),
                @"C:\reports\r.txt");

            Assert.Contains("cell written from", line);
            Assert.DoesNotContain("rounding note", line);
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
