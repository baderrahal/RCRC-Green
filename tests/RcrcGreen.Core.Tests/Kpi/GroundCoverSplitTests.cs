using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The SHRUBS AND GROUND COVER group split by the prefix its species names carry.
    ///
    /// **BOTH PLOTS ARE READ OFF THE SCHEDULES ON SCREEN AND EVERY EXPECTED NUMBER IS WRITTEN
    /// OUT BY HAND.** They are opposite ends of the same shape: DM-11's group is every species
    /// `SHRUBS:` and DM-14's is every species `GROUND COVER:`.
    /// </summary>
    public class GroundCoverSplitTests
    {
        /// <summary>
        /// The real shrubs and lawn heading row. `AREA (sqm)` and `BOTANICAL NAME` are the two
        /// columns this reads, both found by their headings.
        /// </summary>
        private static readonly string[] Headings =
        {
            "IMAGE", "#", "PLANT CODE", "BOTANICAL NAME", "AREA (sqm)", "COUNT (n)",
            "HEIGHT (m)", "SPREAD (m)", "WATER DEMAND", "WATER L/SQM/DAY", "L/DAY"
        };

        private static string[] Species(string name, string area, string count)
        {
            return new[] { name.ToLowerInvariant() + ".jpg", "1", "COD E", name, area, count, "0.4", "0.5", "LOW", "3", "12" };
        }

        private static string[] Structure(string text)
        {
            return new[] { text, "", "", "", "", "", "", "", "", "", "" };
        }

        private static string[] Subtotal(string area, string count)
        {
            return new[] { "", "", "", "", area, count, "", "", "", "", "" };
        }

        private static GroupSubtotal Read(ScannedSchedule schedule, string heading)
        {
            ShrubsAndLawnReading read = ShrubsAndLawnRows.Read(
                schedule,
                new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading },
                CreateFixture.Counted,
                ProjectUnit.Unknown);

            Assert.Empty(read.Refusals);

            return read.Subtotals.Single(one => one.Heading == heading);
        }

        /// <summary>
        /// DM-11 as the schedule prints it. Its shrubs group is two `SHRUBS:` species, 36 and 34,
        /// and its group total is 70.
        /// </summary>
        private static ScannedSchedule Dm11()
        {
            return CreateFixture.ShrubsAndLawn(
                "DM-11",
                Headings,
                Structure("GRASS"),
                Structure("Proposed"),
                Species("GRASS: PENNISETUM SETACEUM", "35 m²", "46"),
                Subtotal("35 m²", "46"),
                Subtotal("35 m²", "46"),
                Structure(KpiMerge.ShrubsHeading),
                Structure("Proposed"),
                Species("SHRUBS: BOUGAINVILLEA GLABRA", "36 m²", "46"),
                Species("SHRUBS: CARISSA MACROCARPA", "34 m²", "12"),
                Subtotal("70 m²", "58"),
                Subtotal("70 m²", "58"),
                new[] { "TOTAL", "", "", "", "105 m²", "104", "", "", "", "", "24" });
        }

        /// <summary>
        /// DM-14 as the schedule prints it. Its shrubs group is two `GROUND COVER:` species, 270
        /// and 198, and its group total is 468. **Every species is ground cover and none is
        /// shrubs**, which is the plot the old rule got wrong.
        /// </summary>
        private static ScannedSchedule Dm14()
        {
            return CreateFixture.ShrubsAndLawn(
                "DM-14",
                Headings,
                Structure("GRASS"),
                Structure("Proposed"),
                Species("GRASS: STENOTAPHRUM SECUNDATUM", "175 m²", "60"),
                Subtotal("175 m²", "60"),
                Subtotal("175 m²", "60"),
                Structure(KpiMerge.ShrubsHeading),
                Structure("Proposed"),
                Species("GROUND COVER: CARISSA MACROCAPA", "270 m²", "90"),
                Species("GROUND COVER: LAMPRANTHUS AUREUS", "198 m²", "66"),
                Subtotal("468 m²", "156"),
                Subtotal("468 m²", "156"),
                new[] { "TOTAL", "", "", "", "643 m²", "216", "", "", "", "", "30" });
        }

        /// <summary>
        /// **DM-11: group total 70, shrubs 70, ground cover 0.** Its figure must not move.
        /// </summary>
        [Fact]
        public void OnDm11TheWholeGroupIsShrubsAndItsFigureDoesNotMove()
        {
            GroupSubtotal group = Read(Dm11(), KpiMerge.ShrubsHeading);
            GroundCoverSplit split = GroundCoverSplit.Of("DM-11", group, CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.Equal(70.0, split.GroupTotal);
            Assert.Equal(70.0, split.TotalShrubsSquareMetres);
            Assert.Equal(70.0, split.ProposedShrubsSquareMetres);
            Assert.Equal(0.0, split.ExistingShrubsSquareMetres);
            Assert.Equal(0.0, split.GroundCoverSquareMetres);
            Assert.Empty(split.Unplaced);
            Assert.True(split.AddsUp, split.Refusal);
        }

        /// <summary>
        /// **DM-14: group total 468, shrubs 0, ground cover 468.** It must go from 468 in the
        /// shrubs box to 0, and from nothing in the ground cover box to 468.
        /// </summary>
        [Fact]
        public void OnDm14TheWholeGroupIsGroundCoverAndTheShrubsBoxGoesToNought()
        {
            GroupSubtotal group = Read(Dm14(), KpiMerge.ShrubsHeading);
            GroundCoverSplit split = GroundCoverSplit.Of("DM-14", group, CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.Equal(468.0, split.GroupTotal);
            Assert.Equal(0.0, split.TotalShrubsSquareMetres);
            Assert.Equal(0.0, split.ProposedShrubsSquareMetres);
            Assert.Equal(468.0, split.GroundCoverSquareMetres);
            Assert.Empty(split.Unplaced);
            Assert.True(split.AddsUp, split.Refusal);
        }

        /// <summary>
        /// **AND THE TWO REACH THE FORM'S OWN BOXES.** The whole point is which box a client
        /// reads the number in.
        /// </summary>
        [Fact]
        public void TheTwoPlotsFillOppositeBoxesOnTheForm()
        {
            Assert.Equal("70", Wrote("DM-11", Dm11(), PdfValue.ProposedShrubs));
            Assert.Equal("70", Wrote("DM-11", Dm11(), PdfValue.TotalShrubs));
            Assert.Equal("0", Wrote("DM-11", Dm11(), PdfValue.GroundCover));

            Assert.Equal("0", Wrote("DM-14", Dm14(), PdfValue.ProposedShrubs));
            Assert.Equal("0", Wrote("DM-14", Dm14(), PdfValue.TotalShrubs));
            Assert.Equal("468", Wrote("DM-14", Dm14(), PdfValue.GroundCover));
        }

        /// <summary>
        /// **THE GRASS GROUP IS UNTOUCHED.** Its species carry `GRASS:`, it goes to lawn through
        /// its own heading, and nothing in the split reads it. DM-11's lawn is 35 and DM-14's is
        /// 175.
        /// </summary>
        [Fact]
        public void TheGrassGroupIsUntouchedAndStillGoesToLawn()
        {
            Assert.Equal("35", Wrote("DM-11", Dm11(), PdfValue.Lawn));
            Assert.Equal("175", Wrote("DM-14", Dm14(), PdfValue.Lawn));

            GroupSubtotal grass = Read(Dm11(), KpiMerge.LawnHeading);
            Assert.Equal(35.0, grass.SquareMetres);
        }

        /// <summary>
        /// **A SPECIES WITH NO PREFIX GOES NOWHERE AND IS NAMED**, with its plot, its row, its
        /// area and why. It is not guessed into either box, and it still counts towards the
        /// group total so the check stays honest.
        /// </summary>
        [Fact]
        public void AnUnprefixedSpeciesGoesNowhereAndIsNamed()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "FM-05",
                Headings,
                Structure(KpiMerge.ShrubsHeading),
                Structure("Proposed"),
                Species("SHRUBS: BOUGAINVILLEA GLABRA", "36 m²", "46"),
                Species("LANTANA CAMARA", "9 m²", "10"),
                Subtotal("45 m²", "56"),
                Subtotal("45 m²", "56"));

            GroundCoverSplit split = GroundCoverSplit.Of(
                "FM-05", Read(schedule, KpiMerge.ShrubsHeading), CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.Equal(36.0, split.ProposedShrubsSquareMetres);
            Assert.Equal(0.0, split.GroundCoverSquareMetres);

            UnplacedSpecies one = Assert.Single(split.Unplaced);
            Assert.Equal("FM-05", one.PlotId);
            Assert.Equal("LANTANA CAMARA", one.BotanicalName);
            Assert.Equal(9.0, one.SquareMetres);
            Assert.Equal(5, one.RowNumber);
            Assert.Equal(
                "its name carries no prefix before a colon, so nothing says whether it is "
                + "SHRUBS or GROUND COVER",
                one.Why);

            // 36 plus 9 is 45, the group total, so the split still adds up and the boxes are
            // written. The nine is in neither of them and is on the page.
            Assert.True(split.AddsUp, split.Refusal);
            Assert.Equal("FM-05 row 5, LANTANA CAMARA, 9 m²: " + one.Why, one.InWords);
        }

        /// <summary>
        /// A prefix that is neither of the two is treated exactly as no prefix is: placed
        /// nowhere and named with what it read. Nothing reaches for a near miss.
        /// </summary>
        [Fact]
        public void APrefixThatIsNeitherIsAlsoPlacedNowhereAndNamedWithWhatItRead()
        {
            ScannedSchedule schedule = CreateFixture.ShrubsAndLawn(
                "FM-06",
                Headings,
                Structure(KpiMerge.ShrubsHeading),
                Structure("Proposed"),
                Species("CLIMBERS: BOUGAINVILLEA GLABRA", "12 m²", "8"),
                Subtotal("12 m²", "8"),
                Subtotal("12 m²", "8"));

            GroundCoverSplit split = GroundCoverSplit.Of(
                "FM-06", Read(schedule, KpiMerge.ShrubsHeading), CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.Equal(0.0, split.TotalShrubsSquareMetres);
            Assert.Equal(0.0, split.GroundCoverSquareMetres);
            Assert.Equal(
                "its prefix reads CLIMBERS, which is neither SHRUBS nor GROUND COVER",
                Assert.Single(split.Unplaced).Why);
        }

        /// <summary>
        /// **A SPLIT THAT DOES NOT ADD UP TO THE GROUP TOTAL REFUSES AND NAMES ALL THREE.** Here
        /// the schedule prints a group total of 100 over species adding to 70, which is an area
        /// of 30 the tool cannot see, and putting the 70 in would be a form short of it with
        /// nothing saying so.
        /// </summary>
        [Fact]
        public void ASplitThatDoesNotAddUpToTheGroupTotalRefusesAndNamesAllThree()
        {
            var group = new GroupSubtotal(
                KpiMerge.ShrubsHeading, 100.0, 58, 2, double.NaN, null, 0, null,
                new[] { new PhaseSubtotal("Proposed", 3, 100.0, 58, true, string.Empty) },
                true, 100.0, 58, null,
                new[]
                {
                    new ShrubSpecies("SHRUBS: BOUGAINVILLEA GLABRA", 36.0, "Proposed", 4),
                    new ShrubSpecies("SHRUBS: CARISSA MACROCARPA", 34.0, "Proposed", 5)
                });

            GroundCoverSplit split = GroundCoverSplit.Of("DM-99", group, CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.False(split.AddsUp);
            Assert.Equal(
                "the split does not add up to the group total the schedule printed. SHRUBS 70 m², "
                + "GROUND COVER 0 m² and 0 species placed nowhere add to 70 m² against its "
                + "printed group total of 100 m², off by 30 m², and the project's area "
                + "rounding step was not read, so no rounding room was allowed.",
                split.Refusal);
        }

        /// <summary>
        /// **AND A REFUSED SPLIT WRITES NONE OF THE FOUR BOXES**, each carrying the same reason.
        /// Writing three and blanking one would leave a form whose own numbers disagree with the
        /// schedule behind it.
        /// </summary>
        [Fact]
        public void ARefusedSplitLeavesAllFourBoxesBlankWithTheSameReason()
        {
            var group = new GroupSubtotal(
                KpiMerge.ShrubsHeading, 100.0, 58, 2, double.NaN, null, 0, null,
                new[] { new PhaseSubtotal("Proposed", 3, 100.0, 58, true, string.Empty) },
                true, 100.0, 58, null,
                new[] { new ShrubSpecies("SHRUBS: BOUGAINVILLEA GLABRA", 36.0, "Proposed", 4) });

            PdfPlan plan = PdfFill.Of(
                CreateFixture.Plot("DM-99", uid2: "ANH-008-MO-100006", subtotals: new[] { group }),
                CountedGroups.Of(KpiTemplates.Mosques), null,
                new DateTime(2026, 9, 15), true, PdfWorkbookNumbers.None, ProjectUnit.Unknown);

            foreach (PdfValue value in new[]
            {
                PdfValue.ExistingShrubs, PdfValue.ProposedShrubs, PdfValue.TotalShrubs, PdfValue.GroundCover
            })
            {
                PdfFieldFill field = plan.Fields.Single(one => one.Value == value);

                Assert.False(field.Written, value + " was written on a split that does not add up");
                Assert.StartsWith("the split does not add up to the group total", field.Why);
            }
        }

        /// <summary>
        /// A group the schedule printed with no species row under it cannot be split, and the
        /// refusal says that rather than writing nought into both boxes.
        /// </summary>
        [Fact]
        public void AGroupWithNoSpeciesRowCannotBeSplitAndSaysSo()
        {
            var group = new GroupSubtotal(
                KpiMerge.ShrubsHeading, 70.0, 58, 2, double.NaN, null, 0, null,
                new[] { new PhaseSubtotal("Proposed", 3, 70.0, 58, true, string.Empty) },
                true, 70.0, 58);

            GroundCoverSplit split = GroundCoverSplit.Of("DM-99", group, CreateFixture.Counted, ProjectUnit.Unknown);

            Assert.False(split.AddsUp);
            Assert.False(split.SpeciesRead);
            Assert.Equal(
                "its group total reads 70 m² and the schedule printed no species row under it, "
                + "so nothing says how much of it is SHRUBS and how much is GROUND COVER.",
                split.Refusal);
        }

        /// <summary>
        /// The prefix is the text before the FIRST colon, without case and with edge whitespace
        /// off, and the inside untouched. Every expectation written out by hand.
        /// </summary>
        [Theory]
        [InlineData("SHRUBS: BOUGAINVILLEA GLABRA", "SHRUBS")]
        [InlineData("GROUND COVER: CARISSA MACROCAPA", "GROUND COVER")]
        [InlineData("GRASS: PENNISETUM SETACEUM", "GRASS")]
        [InlineData("  SHRUBS : CARISSA  ", "SHRUBS")]
        [InlineData("shrubs: carissa", "shrubs")]
        [InlineData("GROUND COVER: A: B", "GROUND COVER")]
        [InlineData("LANTANA CAMARA", "")]
        [InlineData("", "")]
        public void ThePrefixIsTheTextBeforeTheFirstColon(string name, string expected)
        {
            Assert.Equal(expected, SpeciesPrefix.Of(name));
        }

        [Theory]
        [InlineData("SHRUBS: CARISSA", true, false)]
        [InlineData("shrubs: carissa", true, false)]
        [InlineData(" SHRUBS : carissa", true, false)]
        [InlineData("GROUND COVER: CARISSA", false, true)]
        [InlineData("ground cover: carissa", false, true)]
        [InlineData("GROUND  COVER: CARISSA", false, false)]
        [InlineData("GRASS: PENNISETUM", false, false)]
        [InlineData("CARISSA", false, false)]
        public void OnlyTheTwoMeasuredPrefixesAnswerForAnything(string name, bool shrubs, bool cover)
        {
            Assert.Equal(shrubs, SpeciesPrefix.IsShrubs(name));
            Assert.Equal(cover, SpeciesPrefix.IsGroundCover(name));
        }

        /// <summary>
        /// **THE REPORT PRINTS BOTH PLOTS AND NAMES EVERY UNPLACED SPECIES.** A number in the
        /// wrong box is what this round exists to stop, so the page a person checks it on is
        /// part of the work.
        /// </summary>
        [Fact]
        public void TheReportPrintsTheSplitPerPlotAndNamesWhatItCouldNotPlace()
        {
            string report = Report(Dm11(), Dm14());

            Assert.Contains("== " + KpiCreateReport.GroundCoverHeading + " (2) ==", report);
            Assert.Contains("DM-11 | 0 m² | 70 m² | 0 m² | none | 70 m² | YES", report);
            Assert.Contains("DM-14 | 0 m² | 0 m² | 468 m² | none | 468 m² | YES", report);
        }

        private static string Wrote(string plotId, ScannedSchedule schedule, PdfValue value)
        {
            PdfPlan plan = PdfFill.Of(
                Reading(plotId, schedule),
                CountedGroups.Of(KpiTemplates.Mosques), null,
                new DateTime(2026, 9, 15), true, PdfWorkbookNumbers.None, ProjectUnit.Unknown);

            return plan.Fields.Single(one => one.Value == value).Text;
        }

        private static PlotReading Reading(string plotId, ScannedSchedule schedule)
        {
            ShrubsAndLawnReading read = ShrubsAndLawnRows.Read(
                schedule,
                new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading },
                CreateFixture.Counted,
                ProjectUnit.Unknown);

            return CreateFixture.Plot(
                plotId, uid2: "ANH-008-MO-100006", subtotals: read.Subtotals.ToArray());
        }

        private static string Report(params ScannedSchedule[] schedules)
        {
            var readings = schedules
                .Select(one => Reading(one.Name.Substring(0, 5), one))
                .ToArray();

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
    }
}
