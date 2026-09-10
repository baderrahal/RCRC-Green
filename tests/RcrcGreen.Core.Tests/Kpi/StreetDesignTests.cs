using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Street Design counts as Proposed on the STREETS template and nowhere else, by Bader's
    /// decision. No template has a Tree List - Street Design sheet, so the rule before this
    /// one left the group out everywhere, streets included, and on a street plot that group
    /// is the plot's own work.
    ///
    /// ST-05, a street plot, measured off its softscape schedule on screen: Existing 369 over
    /// thirteen species, Proposed 2 which is ALBIZIA LEBBECK 2, Street Design 68 which is
    /// ALBIZIA LEBBECK 6 and CASSIA GLAUCA 62, TOTAL 439. Its proposed trees are 2 plus 68,
    /// 70, its existing are 369, and 369 plus 70 is 439, the TOTAL the schedule prints. That
    /// is the test. Every species and count below is measured, none assumed. CASSIA GLAUCA
    /// sits under Existing at 1 and under Street Design at 62, so one species in a named
    /// group and a by-decision group at once is covered too.
    ///
    /// It is keyed on the TEMPLATE and never on the plot prefix: the same schedule read for
    /// the MOSQUES template leaves the group out, and a mosque plot read for STREETS counts it.
    /// </summary>
    public class StreetDesignTests
    {
        private static readonly CountedGroups Streets = CountedGroups.Of(KpiTemplates.Streets);

        private static readonly string[] Headings =
            { "IMAGE", "PLANT CODE", "BOTANICAL NAME", "COUNT (n)" };

        private static string[] Structure(string text)
        {
            return new[] { text, "", "", "" };
        }

        private static string[] Species(string name, int count)
        {
            return new[] { name + ".jpg", "", name, count.ToString() };
        }

        private static string[] Subtotal(int count)
        {
            return new[] { "", "", "", count.ToString() };
        }

        private static ScannedSchedule Softscape(string plot)
        {
            return CreateFixture.Softscape(
                plot,
                Headings,
                Structure("TREES"),
                Structure("Existing"),
                Species("ACACIA / VACHELLIA FARNESIANA", 25),
                Species("AZADIRACHTA INDICA", 7),
                Species("CASSIA GLAUCA", 1),
                Species("CONOCARPUS ERECTUS", 76),
                Species("CONOCARPUS LANCIFOLIUS", 129),
                Species("FICUS BENJAMINA", 13),
                Species("HIBISCUS TILIACEUS", 4),
                Species("MORINGA OLEIFERA", 3),
                Species("PHOENIX DACTYLIFERA", 18),
                Species("PROSOPIS JULIFLORA", 16),
                Species("UNKNOWN", 20),
                Species("WASHINGTONIA ROBUSTA", 48),
                Species("ZIZIPHUS SPINA-CHRISTI", 9),
                Subtotal(369),
                Structure("Proposed"),
                Species("ALBIZIA LEBBECK", 2),
                Subtotal(2),
                Structure("Street Design"),
                Species("ALBIZIA LEBBECK", 6),
                Species("CASSIA GLAUCA", 62),
                Subtotal(68),
                new[] { "TOTAL", "", "", "439" });
        }

        /// <summary>
        /// A street's shrubs and lawn schedule in the same shape: GRASS Proposed 96 over 117
        /// and Street Design 69 over 84, the group total 165 over 201. Those are FM-05's
        /// numbers, borrowed for the shape, because no street plot's were in the note.
        /// </summary>
        private static ScannedSchedule Ground(string plot)
        {
            return CreateFixture.ShrubsAndLawn(
                plot,
                new[] { "IMAGE", "BOTANICAL NAME", "AREA  (sqm)", "COUNT (n)" },
                new[] { "GRASS", "", "", "" },
                new[] { "Proposed", "", "", "" },
                new[] { "-", "GRASS: CYNODON DACTYLON", "96 m²", "117" },
                new[] { "", "", "96 m²", "117" },
                new[] { "Street Design", "", "", "" },
                new[] { "Pennisetum setaceum.jpg", "GRASS: PENNISETUM SETACEUM", "69 m²", "84" },
                new[] { "", "", "69 m²", "84" },
                new[] { "", "", "165 m²", "201" },
                new[] { "TOTAL", "", "165 m²", "201" });
        }

        private static PlotReading Reading(string plot, CountedGroups counted)
        {
            SoftscapeReading trees = SoftscapeRows.Read(Softscape(plot), counted, plot);
            Assert.True(trees.WasRead, string.Join(" ", trees.Refusals));
            ShrubsAndLawnReading ground = ShrubsAndLawnRows.Read(
                Ground(plot), new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }, counted);
            Assert.True(ground.WasRead, string.Join(" ", ground.Refusals));

            return CreateFixture.Plot(
                plot,
                component: "STREET 36m ROW",
                species: trees.Species.ToArray(),
                subtotals: ground.Subtotals.ToArray(),
                regions: new RegionArea[0],
                softscapeTotalRead: trees.TotalRead,
                softscapeTotal: trees.Total,
                rowsPassedOver: trees.RowsPassedOver,
                printedSchedules: new[] { Softscape(plot), Ground(plot) },
                softscapeTotalRow: trees.TotalRow,
                printedGroups: trees.Groups.ToArray());
        }

        private static string Report(KpiTemplate template, params PlotReading[] readings)
        {
            return KpiCreateReport.Write(
                CreateFixture.Run(readings: readings, template: template), new DateTime(2026, 9, 10, 16, 0, 0));
        }

        /// <summary>
        /// The one decision, as data on the template: STREETS counts Street Design as Proposed
        /// and no other template counts anything beyond its two sheets. A third name is added
        /// here only when the team says so.
        /// </summary>
        [Fact]
        public void OnlyStreetsCountsAGroupByDecisionAndItIsStreetDesign()
        {
            Assert.Equal(new[] { "Street Design" }, KpiTemplates.Streets.GroupsCountedAsProposed);
            Assert.Equal(1, KpiTemplates.All.Sum(one => one.GroupsCountedAsProposed.Count));

            GroupByDecision decided = Assert.Single(Streets.ByDecision);
            Assert.Equal("Street Design", decided.GroupName);
            Assert.Equal("Tree List - Proposed", decided.SheetName);
            Assert.Empty(CountedGroups.Of(KpiTemplates.Mosques).ByDecision);
        }

        [Fact]
        public void StreetDesignCountsOnStreetsAndOnNoOtherTemplate()
        {
            Assert.True(Streets.Counts("Street Design"));
            Assert.True(Streets.Counts("STREET DESIGN"));
            Assert.True(Streets.Counts(" street design "));
            Assert.Equal("Tree List - Proposed", Streets.SheetFor("Street Design"));
            Assert.Equal("Tree List - Proposed takes it on STREETS by decision, as that plot's own work", Streets.Why("Street Design"));
            Assert.Equal("Tree List - Proposed is named for it", Streets.Why("Proposed"));

            foreach (KpiTemplate template in KpiTemplates.All.Where(one => one != KpiTemplates.Streets))
            {
                CountedGroups counted = CountedGroups.Of(template);
                Assert.False(counted.Counts("Street Design"), template.Name);
                Assert.Null(counted.SheetFor("Street Design"));
                Assert.Equal(CountedGroups.LeftOut, counted.Why("Street Design"));
            }
        }

        /// <summary>
        /// ST-05 on STREETS: three groups, all taken, sixteen species rows adding to 439,
        /// nothing left out, TOTAL 439 at row 25. Rows by hand: thirteen existing species on
        /// rows 4 to 16, their subtotal on 17, Proposed on 18 with its row on 19, Street Design
        /// on 21 with its rows on 22 and 23 and its subtotal on 24.
        /// </summary>
        [Fact]
        public void St05ReadsEveryGroupAndNothingIsLeftOut()
        {
            SoftscapeReading reading = SoftscapeRows.Read(Softscape("ST-05"), Streets, "ST-05");

            Assert.True(reading.WasRead, string.Join(" ", reading.Refusals));
            Assert.Equal(16, reading.Species.Count);
            Assert.Equal(
                new[] { 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 19, 22, 23 },
                reading.Species.Select(one => one.RowNumber));
            Assert.Equal(439, reading.SpeciesSum);
            Assert.Empty(reading.LeftOut);
            Assert.Equal(439, reading.Total);
            Assert.Equal(25, reading.TotalRow);

            Assert.Equal(3, reading.Groups.Count);
            Assert.All(reading.Groups, one => Assert.True(one.Counted));
            Assert.Equal("Existing", reading.Groups[0].Name);
            Assert.Equal(13, reading.Groups[0].Species.Count);
            Assert.Equal(369, reading.Groups[0].SpeciesSum);
            Assert.Equal(369, reading.Groups[0].Subtotal);
            Assert.Equal(17, reading.Groups[0].SubtotalRow);
            Assert.Equal("Street Design", reading.Groups[2].Name);
            Assert.Equal(21, reading.Groups[2].RowNumber);
            Assert.Equal(68, reading.Groups[2].SpeciesSum);
            Assert.Equal(68, reading.Groups[2].Subtotal);
            Assert.Equal("Tree List - Proposed takes it on STREETS by decision, as that plot's own work", reading.Groups[2].Why);
        }

        /// <summary>
        /// 369 existing, 70 proposed, 439 in all. ALBIZIA LEBBECK is one merged row of 8, off
        /// two rows on one plot, going to Tree List - Proposed, and it says both groups. CASSIA
        /// GLAUCA is two merged rows, 1 on Tree List - Existing and 62 on Tree List - Proposed,
        /// because Existing and Street Design go to different sheets.
        /// </summary>
        [Fact]
        public void St05ProposedIsSeventyExistingIsThreeSixtyNineAndTheyReachTheTotal()
        {
            PlotReading reading = Reading("ST-05", Streets);
            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(new[] { reading }, Streets);

            Assert.Equal(369, merged.Where(one => one.SheetName == "Tree List - Existing").Sum(one => one.Quantity));
            Assert.Equal(70, merged.Where(one => one.SheetName == "Tree List - Proposed").Sum(one => one.Quantity));
            Assert.Equal(439, merged.Sum(one => one.Quantity));
            Assert.Equal(reading.SoftscapeTotal, merged.Sum(one => one.Quantity));

            MergedSpecies albizia = Assert.Single(merged, one => one.BotanicalName == "ALBIZIA LEBBECK");
            Assert.Equal(8, albizia.Quantity);
            Assert.Equal("ST-05 8 (2 rows, 2 + 6)", albizia.Working);
            Assert.Equal("Proposed and Street Design", albizia.GroupName);
            Assert.Equal("Tree List - Proposed", albizia.SheetName);

            List<MergedSpecies> cassia = merged.Where(one => one.BotanicalName == "CASSIA GLAUCA").ToList();
            Assert.Equal(2, cassia.Count);
            MergedSpecies cassiaExisting = Assert.Single(cassia, one => one.SheetName == "Tree List - Existing");
            Assert.Equal(1, cassiaExisting.Quantity);
            Assert.Equal("Existing", cassiaExisting.GroupName);
            MergedSpecies cassiaProposed = Assert.Single(cassia, one => one.SheetName == "Tree List - Proposed");
            Assert.Equal(62, cassiaProposed.Quantity);
            Assert.Equal("Street Design", cassiaProposed.GroupName);
        }

        /// <summary>
        /// ALBIZIA LEBBECK on row 8 under Proposed and row 11 under Street Design is two
        /// groups whose counts add, not one species printed twice under one group, so the
        /// same group refusal does not trip and the accounting passes with nothing left out.
        /// </summary>
        [Fact]
        public void TwoRowsUnderProposedAndStreetDesignAddAndDoNotTripTheSameGroupRefusal()
        {
            PlotReading reading = Reading("ST-05", Streets);

            // ALBIZIA LEBBECK under Proposed and Street Design, CASSIA GLAUCA under Existing and
            // Street Design: two groups each, and neither is a species printed twice under one.
            Assert.Equal(2, reading.Species.Count(one => one.BotanicalName == "CASSIA GLAUCA"));
            Assert.Empty(reading.SpeciesPrintedOnMoreThanOneRow);
            Assert.False(reading.HoldsAGroupLeftOut);

            Reconciliation held = Reconciliation.Of(new[] { "ST-05" }, new[] { reading }, null, false, KpiTemplates.Streets);
            Assert.True(held.AddsUp, string.Join(" ", held.Refusals));
            Assert.Empty(held.WithAGroupLeftOut);
            Assert.Equal(0, held.SchedulesWithAGroupLeftOut);
        }

        /// <summary>
        /// The matcher places the merged row on the Proposed list with its 8, through the same
        /// resolver the reader used, and the count that lands is 8 and not 2.
        /// </summary>
        [Fact]
        public void TheMatcherPlacesAStreetDesignSpeciesOnTheProposedList()
        {
            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(new[] { Reading("ST-05", Streets) }, Streets);
            SpeciesList existing = CreateFixture.WorkbookList("Phoenix dactylifera");
            SpeciesList proposed = CreateFixture.WorkbookList("Albizia lebbeck", "Cassia glauca");

            IReadOnlyList<SpeciesMatch> matches = SpeciesMatching.Against(merged, KpiTemplates.Streets, existing, proposed);

            SpeciesMatch albizia = Assert.Single(matches, one => one.Species.BotanicalName == "ALBIZIA LEBBECK");
            Assert.True(albizia.Matched, albizia.Why);
            Assert.Equal("Tree List - Proposed", albizia.SheetName);
            Assert.Equal(8, albizia.Species.Quantity);

            SpeciesMatch cassia = Assert.Single(matches,
                one => one.Species.BotanicalName == "CASSIA GLAUCA" && one.SheetName == "Tree List - Proposed");
            Assert.True(cassia.Matched, cassia.Why);
            Assert.Equal(62, cassia.Species.Quantity);

            SpeciesMatch phoenix = Assert.Single(matches, one => one.Species.BotanicalName == "PHOENIX DACTYLIFERA");
            Assert.Equal("Tree List - Existing", phoenix.SheetName);
            Assert.Equal(18, phoenix.Species.Quantity);
        }

        /// <summary>
        /// The same schedule read for MOSQUES leaves Street Design out, 68 named, and a mosque
        /// plot read for STREETS counts it. The template decides, the prefix nothing.
        /// </summary>
        [Fact]
        public void TheTemplateDecidesAndThePlotPrefixDecidesNothing()
        {
            CountedGroups mosques = CountedGroups.Of(KpiTemplates.Mosques);

            SoftscapeReading streetPlotOnMosques = SoftscapeRows.Read(Softscape("ST-05"), mosques, "ST-05");
            Assert.Equal(371, streetPlotOnMosques.SpeciesSum);
            Assert.Equal(14, streetPlotOnMosques.Species.Count);
            Assert.Equal(68, streetPlotOnMosques.LeftOut.Sum(one => one.Quantity));
            Assert.False(streetPlotOnMosques.Groups[2].Counted);
            Assert.Equal(CountedGroups.LeftOut, streetPlotOnMosques.Groups[2].Why);

            SoftscapeReading mosquePlotOnStreets = SoftscapeRows.Read(Softscape("FM-05"), Streets, "FM-05");
            Assert.Equal(439, mosquePlotOnStreets.SpeciesSum);
            Assert.Empty(mosquePlotOnStreets.LeftOut);
            Assert.True(mosquePlotOnStreets.Groups[2].Counted);

            IReadOnlyList<MergedSpecies> merged = KpiMerge.Species(new[] { Reading("FM-05", Streets) }, Streets);
            Assert.Equal(8, Assert.Single(merged, one => one.BotanicalName == "ALBIZIA LEBBECK").Quantity);
        }

        /// <summary>
        /// The street's areas count too: GRASS is 96 plus 69, 165, off both phase rows, with the
        /// group total checked and no phase left out.
        /// </summary>
        [Fact]
        public void AStreetsShrubsAndLawnPhaseCountsAndItsAreaAdds()
        {
            ShrubsAndLawnReading ground = ShrubsAndLawnRows.Read(
                Ground("ST-05"), new[] { KpiMerge.ShrubsHeading, KpiMerge.LawnHeading }, Streets);

            GroupSubtotal grass = Assert.Single(ground.Subtotals);
            Assert.Equal(165.0, grass.SquareMetres);
            Assert.Equal(201, grass.ItemCount);
            Assert.True(grass.Agrees);
            Assert.Empty(grass.PhasesLeftOut);
            Assert.Equal(2, grass.Phases.Count);
            Assert.True(grass.Phases[1].Counted);
            Assert.Equal("Tree List - Proposed takes it on STREETS by decision, as that plot's own work", grass.Phases[1].Why);
        }

        /// <summary>
        /// The report says which route each group took, so a Street Design group counted on
        /// STREETS reads differently from one left out on MOSQUES.
        /// </summary>
        [Fact]
        public void TheReportSaysWhichRouteTheGroupTook()
        {
            string streets = Report(KpiTemplates.Streets, Reading("ST-05", Streets));
            Assert.Contains(
                "        row 3 Existing: 13 species rows adding to 369, subtotal row 17 prints 369, TAKEN, "
                + "Tree List - Existing is named for it\r\n"
                + "        row 18 Proposed: 1 species row adding to 2, subtotal row 20 prints 2, TAKEN, "
                + "Tree List - Proposed is named for it\r\n"
                + "        row 21 Street Design: 2 species rows adding to 68, subtotal row 24 prints 68, TAKEN, "
                + "Tree List - Proposed takes it on STREETS by decision, as that plot's own work\r\n",
                streets);
            Assert.Contains(
                "        row 8 Street Design: 69 over 84, TAKEN, Tree List - Proposed takes it on STREETS by decision, as that plot's own work\r\n",
                streets);
            Assert.Contains("  schedules holding a group no tree list sheet is named for   0\r\n", streets);

            string mosques = Report(KpiTemplates.Mosques, Reading("ST-05", CountedGroups.Of(KpiTemplates.Mosques)));
            Assert.Contains(
                "        row 21 Street Design: 2 species rows adding to 68, subtotal row 24 prints 68, LEFT OUT, "
                + "no tree list sheet is named for it, so it is out of scope\r\n",
                mosques);
            Assert.Contains("  schedules holding a group no tree list sheet is named for   2, on ST-05.", mosques);
        }
    }

    /// <summary>
    /// The note above the Create button. The accounting line in the report is right and stays,
    /// and the user also needs it where they are looking when they press. It names plots and
    /// schedules and never species, and it is a note: the workbook is written.
    /// </summary>
    public class GroupsLeftOutNoteTests
    {
        private static PrintedGroup LeftOutGroup(string name, int row)
        {
            return new PrintedGroup(name, row, new SpeciesRow[0], true, 0, row + 1, false, CountedGroups.LeftOut);
        }

        private static PrintedGroup TakenGroup(string name, int row)
        {
            return new PrintedGroup(name, row, new SpeciesRow[0], true, 0, row + 1, true, "Tree List - Proposed is named for it");
        }

        private static GroupSubtotal GrassWith(params PhaseSubtotal[] phases)
        {
            return new GroupSubtotal(KpiMerge.LawnHeading, 96.0, 117, 3, 96.0, string.Empty, 9, new[] { 5, 8, 9 }, phases, true, 165.0, 201);
        }

        private static PhaseSubtotal Taken(string name, int row)
        {
            return new PhaseSubtotal(name, row, 96.0, 117, true, "Tree List - Proposed is named for it");
        }

        private static PhaseSubtotal LeftOut(string name, int row)
        {
            return new PhaseSubtotal(name, row, 69.0, 84, false, CountedGroups.LeftOut);
        }

        private static PlotReading Plot(string plot, bool inSoftscape, bool inShrubs, double area)
        {
            return CreateFixture.Plot(
                plot,
                regions: new[] { CreateFixture.Region(CreateFixture.OutOfScope, area) },
                printedGroups: inSoftscape
                    ? new[] { TakenGroup("Proposed", 3), LeftOutGroup("Street Design", 9) }
                    : new[] { TakenGroup("Proposed", 3) },
                subtotals: inShrubs
                    ? new[] { GrassWith(Taken("Proposed", 5), LeftOut("Street Design", 8)) }
                    : new[] { GrassWith(Taken("Proposed", 5)) });
        }

        /// <summary>
        /// Written out by hand from the shape asked for: the first plot spells both schedules
        /// out and the next says in both.
        /// </summary>
        [Fact]
        public void TwoPlotsInBothSchedulesReadAsAsked()
        {
            string note = CreateWords.GroupsLeftOut(
                new[] { Plot("DM-16", true, true, 700.0), Plot("FM-05", true, true, 900.0) }, KpiTemplates.Mosques);

            Assert.Equal(
                "Street Design found on 2 plots on MOSQUES, which has no sheet for it: "
                + "DM-16 in its softscape and its shrubs and lawn schedules, FM-05 in both. "
                + "Those rows were left out. Fix them in the model.",
                note);
        }

        [Fact]
        public void OnePlotInOneScheduleNamesTheSchedule()
        {
            Assert.Equal(
                "Street Design found on 1 plot on MOSQUES, which has no sheet for it: "
                + "FM-05 in its softscape schedule. Those rows were left out. Fix them in the model.",
                CreateWords.GroupsLeftOut(new[] { Plot("FM-05", true, false, 900.0) }, KpiTemplates.Mosques));

            Assert.Equal(
                "Street Design found on 1 plot on MOSQUES, which has no sheet for it: "
                + "FM-05 in its shrubs and lawn schedule. Those rows were left out. Fix them in the model.",
                CreateWords.GroupsLeftOut(new[] { Plot("FM-05", false, true, 900.0) }, KpiTemplates.Mosques));
        }

        /// <summary>
        /// Plots in the model's order, and a plot with nothing left out is not in the note.
        /// </summary>
        [Fact]
        public void PlotsAreInNaturalOrderAndAPlotWithNothingLeftOutIsNotNamed()
        {
            string note = CreateWords.GroupsLeftOut(
                new[] { Plot("FM-10", true, false, 700.0), Plot("FM-2", false, false, 800.0), Plot("FM-5", false, true, 900.0) },
                KpiTemplates.Mosques);

            Assert.StartsWith("Street Design found on 2 plots on MOSQUES, which has no sheet for it: FM-5 in its shrubs and lawn schedule, FM-10 in its softscape schedule.", note);
            Assert.DoesNotContain("FM-2 ", note);
        }

        [Fact]
        public void NothingLeftOutIsNoNote()
        {
            Assert.Equal(string.Empty, CreateWords.GroupsLeftOut(new[] { Plot("FM-05", false, false, 900.0) }, KpiTemplates.Mosques));
            Assert.Equal(string.Empty, CreateWords.GroupsLeftOut(new PlotReading[0], KpiTemplates.Mosques));
            Assert.Throws<ArgumentNullException>(() => CreateWords.GroupsLeftOut(new PlotReading[0], null));
        }

        /// <summary>
        /// Two names left out are both named and the sheet words say them.
        /// </summary>
        [Fact]
        public void TwoGroupNamesAreBothNamed()
        {
            PlotReading plot = CreateFixture.Plot(
                "FM-05",
                printedGroups: new[] { LeftOutGroup("Street Design", 9), LeftOutGroup("Demolished", 14) });

            Assert.Equal(
                "Street Design and Demolished found on 1 plot on MOSQUES, which has no sheet for them: "
                + "FM-05 in its softscape schedule. Those rows were left out. Fix them in the model.",
                CreateWords.GroupsLeftOut(new[] { plot }, KpiTemplates.Mosques));
        }
    }
}
