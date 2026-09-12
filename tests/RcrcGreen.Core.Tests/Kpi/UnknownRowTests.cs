using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **UNKNOWN was not written on the 1552 run and two things stopped it.**
    ///
    /// Revit prints a species called UNKNOWN, 16 trees. The MOSQUES Existing list holds a row
    /// for it at 101, named Unknown Tree, measured with D101 Unknown Tree, I101 0, J101 0 as a
    /// plain value and not a blank, K101 0, L101 0 as a plain value rather than the
    /// IF(ISBLANK(J)) formula the other rows carry, and M101 empty. So the row is safe to
    /// write: the client put it there with zeroes, which is them saying an unsized tree counts
    /// in the total and contributes no canopy.
    ///
    /// The two are separate and only one of them is code this round changes.
    ///
    /// **The name does not match**, UNKNOWN against Unknown Tree, without case and with the
    /// edge spaces off they still differ. That is a question about names and the rule is the
    /// user's to choose, so nothing here widens the match.
    ///
    /// **The empty row route then withheld it for having no diameter**, which is the right
    /// rule in the wrong place. These tests pin where that rule lives: on the empty row route
    /// only, never on a row the workbook already holds. A matched row is the client's row and
    /// its own cells decide.
    ///
    /// Every expected value below is written out by hand.
    /// </summary>
    public class UnknownRowTests
    {
        private static SpeciesList Holding(params SpeciesListRow[] rows)
        {
            return SpeciesList.Holding(rows, 4, 120, "B121", "I", "J");
        }

        private static IReadOnlyList<SpeciesMatch> Against(SpeciesList list, params SpeciesRow[] species)
        {
            return SpeciesMatching.Against(
                KpiMerge.Species(
                    new[] { CreateFixture.Plot("DM-12", species: species) },
                    CreateFixture.Counted),
                KpiTemplates.Mosques, list, list);
        }


        /// <summary>
        /// **The rule this round pins.** A species the list HOLDS, printing no diameter, is
        /// still written: its count goes into the quantity column and the client's own cells
        /// are left exactly as they are. Row 101 carries zeroes the client put there, so the
        /// canopy comes out nought and nothing breaks.
        /// </summary>
        [Fact]
        public void AMatchedRowWithNoDiameterIsStillWritten()
        {
            // Named rows running unbroken to the one that holds it, so the row is inside the
            // list rather than past its first empty row, which is its own rule below.
            SpeciesList list = Holding(
                new SpeciesListRow(4, "Albizia lebbeck", "15", "8"),
                new SpeciesListRow(5, "UNKNOWN", "0", "0"));

            SpeciesMatch match = Assert.Single(Against(
                list, CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0")));

            Assert.True(match.Placed, match.Why);
            Assert.False(match.Added);
            Assert.Equal(5, match.Row);
            Assert.Equal(string.Empty, match.Why);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Mosques, null, null, string.Empty, null, null, null,
                new[] { match }, null, null, null);

            // The count reaches the sheet, and NOTHING is written over the client's cells.
            CellWrite quantity = Assert.Single(plan.Writes, one => one.Cell.ToString() == "B5");
            Assert.Equal("16", quantity.Stored);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "I5");
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "J5");
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D5");
        }

        /// <summary>
        /// **A THIRD THING CAN STOP IT, and it is not the name and not the diameter.** A name
        /// the workbook holds PAST THE FIRST EMPTY ROW of its list is refused, matched exactly
        /// or not: the list's names run to its first gap and anything below that is neither
        /// matched nor written in a second time. So whether naming UNKNOWN correctly is enough
        /// to write row 101 depends on where the Existing list's first empty row sits, which
        /// the report already prints under THE WORKBOOK'S OWN TREE LISTS.
        /// </summary>
        [Fact]
        public void ANameHeldBelowTheFirstEmptyRowIsRefusedEvenWhenItMatchesExactly()
        {
            // Named at 4, nothing at 5 to 100, named again at 101: the shape the MOSQUES list
            // has if its species stop well before row 101.
            SpeciesList list = Holding(
                new SpeciesListRow(4, "Albizia lebbeck", "15", "8"),
                new SpeciesListRow(101, "UNKNOWN", "0", "0"));

            SpeciesMatch match = Assert.Single(Against(
                list, CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0")));

            Assert.False(match.Placed);
            Assert.Equal(101, match.Row);
            Assert.Equal(
                "the workbook holds this name on row 101, below the first empty row of its list at row 5, "
                + "so it was neither matched nor written in a second time",
                match.Why);
        }

        /// <summary>
        /// The same species with the same absent diameter, where the list does NOT hold the
        /// name, is still withheld. The tool would be creating a row the workbook has no size
        /// for, and a row written with a name and a count alone put seven error formulas into
        /// the 1836 workbook.
        /// </summary>
        [Fact]
        public void AnEmptyRowWithNoDiameterIsStillWithheld()
        {
            SpeciesList list = Holding(new SpeciesListRow(4, "Albizia lebbeck", "15", "8"));

            SpeciesMatch match = Assert.Single(Against(
                list, CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0")));

            Assert.False(match.Placed);
            Assert.False(match.Added);
            Assert.Equal(0, match.Row);
            Assert.Contains("no canopy diameter a workbook can compute with", match.Why);
        }

        /// <summary>
        /// The nearest name is printed so a person can see whether the fault is one name or a
        /// family of them. UNKNOWN and Unknown Tree share seven characters without case.
        /// </summary>
        [Fact]
        public void TheNearestNameIsFoundBySharedOpening()
        {
            SpeciesList list = Holding(
                new SpeciesListRow(4, "Albizia lebbeck", "15", "8"),
                new SpeciesListRow(101, "Unknown Tree", "0", "0"));

            Assert.Equal("Unknown Tree", SpeciesMatching.ClosestName(list, "UNKNOWN"));
            Assert.Equal("Albizia lebbeck", SpeciesMatching.ClosestName(list, "ALBIZIA LEBBECK"));

            // Nothing shared at the opening is nothing offered, rather than the least unlike
            // name in the list pushed at somebody as an answer.
            Assert.Equal(string.Empty, SpeciesMatching.ClosestName(list, "Phoenix dactylifera"));
            Assert.Equal(string.Empty, SpeciesMatching.ClosestName(list, string.Empty));
            Assert.Equal(string.Empty, SpeciesMatching.ClosestName(null, "UNKNOWN"));
        }

        /// <summary>
        /// Longer shared opening wins, so a name is never offered a shorter cousin of itself
        /// when a nearer one is in the list.
        /// </summary>
        [Fact]
        public void TheLongestSharedOpeningWins()
        {
            SpeciesList list = Holding(
                new SpeciesListRow(4, "Acacia", "1", "1"),
                new SpeciesListRow(5, "Acacia / Vachellia farnesiana", "2", "2"),
                new SpeciesListRow(6, "Bougainvillea glabra", "3", "3"));

            Assert.Equal(
                "Acacia / Vachellia farnesiana",
                SpeciesMatching.ClosestName(list, "ACACIA / VACHELLIA FARNESIANA"));
            Assert.Equal(
                "Bougainvillea glabra",
                SpeciesMatching.ClosestName(list, "BOUGAINVILLEA GLABRA 'PINK PIXIE'"));
        }

        /// <summary>
        /// A species the list holds carries no nearest name, because there is nothing to
        /// wonder about: it matched.
        /// </summary>
        [Fact]
        public void AMatchedSpeciesOffersNoNearestName()
        {
            SpeciesList list = Holding(new SpeciesListRow(101, "UNKNOWN", "0", "0"));

            SpeciesMatch match = Assert.Single(Against(
                list, CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0")));

            Assert.Equal(string.Empty, match.NearestInTheList);
        }

        /// <summary>
        /// And a species it does not hold carries the nearest name into the report, which is
        /// the whole point: a run answers whether UNKNOWN is one name or a family of them.
        /// </summary>
        [Fact]
        public void AnUnmatchedSpeciesCarriesTheNearestNameIntoTheReport()
        {
            SpeciesList list = Holding(
                new SpeciesListRow(4, "Albizia lebbeck", "15", "8"),
                new SpeciesListRow(101, "Unknown Tree", "0", "0"));

            IReadOnlyList<SpeciesMatch> matches = Against(
                list, CreateFixture.Species("UNKNOWN", "Existing", 16, 5, "-", "0"));

            Assert.Equal("Unknown Tree", matches[0].NearestInTheList);

            string report = KpiCreateReport.Write(
                CreateFixture.Run(matches: matches.ToArray(), existingList: list, proposedList: list),
                new System.DateTime(2026, 9, 12, 15, 52, 0));

            Assert.Contains("  Revit name | nearest name in the list | group | merged | from |", report);
            Assert.Contains("  UNKNOWN | Unknown Tree | Existing | 16 | DM-12 16 | NOWHERE,", report);
        }
    }
}
