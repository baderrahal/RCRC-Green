using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The alias is a TABLE and not a rule, and these are its three guards plus the two
    /// ordinary cases around it. Every expected value here is written out by hand.
    ///
    /// Measured on the workbook the 1552 run wrote on the NG03 model, which is not in this
    /// repository: MOSQUES Tree List - Existing holds 98 names on rows 4 to 101 with no empty
    /// row inside the list, Unknown Tree is row 101, and the Proposed list holds 80 names on
    /// rows 4 to 83 and no Unknown Tree at all.
    /// </summary>
    public class SpeciesAliasTests
    {
        /// <summary>
        /// The Existing list as the measured file has it, shortened to what a test can hold:
        /// a species that matches on its own name, then Unknown Tree last, with no gap between
        /// them and room past the end for a species written in.
        /// </summary>
        private static SpeciesList ExistingList(params string[] names)
        {
            var rows = new List<SpeciesListRow>();
            int at = 4;
            foreach (string name in names) rows.Add(new SpeciesListRow(at++, name));

            return SpeciesList.Holding(rows, 4, at + 2, KpiTemplates.QuantityColumn + (at + 3));
        }

        private static IReadOnlyList<SpeciesMatch> Matching(SpeciesRow[] rows, SpeciesList list)
        {
            PlotReading plot = CreateFixture.Plot("DM-12", species: rows);

            return SpeciesMatching.Against(
                KpiMerge.Species(new[] { plot }, CreateFixture.Counted), KpiTemplates.Mosques, list, list);
        }

        /// <summary>
        /// The whole point of the round. UNKNOWN reaches the row the client's list calls
        /// Unknown Tree, and the count goes on it.
        /// </summary>
        [Fact]
        public void UnknownReachesTheRowTheListCallsUnknownTree()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 16, 19, "-", "0") },
                ExistingList("Albizia lebbeck", "Unknown Tree")));

            Assert.True(match.Matched);
            Assert.True(match.ThroughAnAlias);
            Assert.False(match.Added);
            Assert.Equal(5, match.Row);
            Assert.Equal("Unknown Tree", match.WorkbookName);
            Assert.Equal(16, match.Species.Quantity);
            Assert.Equal("Tree List - Existing", match.SheetName);
        }

        /// <summary>
        /// Guard a. Two rows carrying the alias target is a refusal naming both, because
        /// nothing can say which of the client's rows the count belongs on.
        /// </summary>
        [Fact]
        public void AnAliasThatReachesTwoRowsIsRefusedAndNamesBoth()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 16, 19, "-", "0") },
                ExistingList("Unknown Tree", "Albizia lebbeck", "Unknown Tree")));

            Assert.False(match.Placed);
            Assert.False(match.ThroughAnAlias);
            Assert.False(match.Added);
            Assert.Equal(0, match.Row);
            Assert.Equal(
                "UNKNOWN means Unknown Tree, and the workbook holds that name on rows 4, 6, "
                + "so nothing can say which one the count belongs on and it was not written",
                match.Why);
        }

        /// <summary>
        /// Guard b. A name the list holds word for word is matched on its own name and the
        /// table is not consulted at all, so the exact row wins even where an alias for the
        /// same Revit name points somewhere else on the sheet.
        /// </summary>
        [Fact]
        public void AnExactMatchWinsAndTheAliasIsNotConsulted()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 16, 19, "-", "0") },
                ExistingList("Unknown Tree", "UNKNOWN")));

            Assert.True(match.Matched);
            Assert.False(match.ThroughAnAlias);
            Assert.Null(match.Alias);
            Assert.Equal(5, match.Row);
            Assert.Equal("UNKNOWN", match.WorkbookName);
        }

        /// <summary>
        /// Guard c, the report half. A count through an alias is named in its own block with
        /// the alias and the row it reached, and the matched table says how each one got there,
        /// so an aliased count can never read the same as one that matched on its own name.
        /// </summary>
        [Fact]
        public void TheReportNamesTheAliasAndTheRowItReached()
        {
            SpeciesList list = ExistingList("Albizia lebbeck", "Unknown Tree");
            IReadOnlyList<SpeciesMatch> matches = Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Existing, 16, 19, "-", "0") },
                list);

            string[] lines = KpiCreateReport.Write(
                CreateFixture.Run(matches: matches.ToArray(), existingList: list, proposedList: list),
                new System.DateTime(2026, 9, 12, 14, 0, 0)).Split('\n');

            Assert.Contains(lines, one => one.Contains("SPECIES MATCHED THROUGH AN ALIAS")
                && one.Contains("1"));
            Assert.Contains(lines, one => one.Trim()
                == "UNKNOWN | UNKNOWN means Unknown Tree | Tree List - Existing | B5 | 16");
            Assert.Contains(lines, one => one.Contains("THROUGH THE ALIAS UNKNOWN means Unknown Tree"));
        }

        /// <summary>
        /// The ordinary case beside the guards: a species that matches on its own name says so
        /// in the same column, so the how column is a real answer for every matched row rather
        /// than a note that only appears when an alias fired.
        /// </summary>
        [Fact]
        public void AnOrdinaryMatchSaysItMatchedOnItsOwnName()
        {
            SpeciesList list = ExistingList("Albizia lebbeck");
            IReadOnlyList<SpeciesMatch> matches = Matching(
                new[] { CreateFixture.Species("ALBIZIA LEBBECK", CreateFixture.Existing, 13) },
                list);

            string[] lines = KpiCreateReport.Write(
                CreateFixture.Run(matches: matches.ToArray(), existingList: list, proposedList: list),
                new System.DateTime(2026, 9, 12, 14, 0, 0)).Split('\n');

            Assert.Contains(lines, one => one.Trim()
                == "Tree List - Existing | B4 | Albizia lebbeck | ALBIZIA LEBBECK | Existing | 13 | DM-12 13 | matched on its own name");
            Assert.Contains(lines, one => one.Contains("SPECIES MATCHED THROUGH AN ALIAS")
                && one.Contains("0"));
        }

        /// <summary>
        /// A species with no alias and no row in the list is still reported and still not
        /// written, which is the rule the alias must not have widened. This one carries no
        /// canopy diameter, so it takes no empty row either and the reason names the measures.
        /// </summary>
        [Fact]
        public void ASpeciesWithNoAliasAndNoRowIsStillReportedAndNotWritten()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("PHOENIX DACTYLIFERA", CreateFixture.Existing, 5, 22, "-", "0") },
                ExistingList("Albizia lebbeck", "Unknown Tree")));

            Assert.False(match.Matched);
            Assert.False(match.Placed);
            Assert.False(match.Added);
            Assert.False(match.ThroughAnAlias);
            Assert.Null(match.Alias);
            Assert.Equal(0, match.Row);
            Assert.Equal(5, match.Species.Quantity);
            Assert.Equal(
                "the workbook's list does not hold this name, and a row written into an empty one carries only "
                + "what the model prints, which is no canopy diameter a workbook can compute with, "
                + "so no row was written: DM-12 row 22 prints 0, which is no size",
                match.Why);
        }

        /// <summary>
        /// The alias target on no row of that sheet is nothing at all. The MOSQUES Proposed
        /// list holds no Unknown Tree, so an UNKNOWN under Proposed goes down the empty row
        /// route exactly as it did before the table existed, where the report already names it.
        /// </summary>
        [Fact]
        public void AnAliasTargetTheSheetDoesNotHoldChangesNothing()
        {
            SpeciesMatch match = Assert.Single(Matching(
                new[] { CreateFixture.Species("UNKNOWN", CreateFixture.Proposed, 16, 19, "15", "8") },
                ExistingList("Albizia lebbeck", "Cassia glauca")));

            Assert.True(match.Added);
            Assert.False(match.ThroughAnAlias);
            Assert.Null(match.Alias);
            Assert.Equal(SpeciesMatching.WrittenIn, match.Why);
        }

        /// <summary>
        /// The table itself, so a second entry added later cannot arrive without somebody
        /// changing a count written out by hand.
        /// </summary>
        [Fact]
        public void TheTableHoldsOneEntryToday()
        {
            Assert.Single(SpeciesAliases.All);
            Assert.Equal("UNKNOWN", SpeciesAliases.All[0].RevitName);
            Assert.Equal("Unknown Tree", SpeciesAliases.All[0].WorkbookName);
            Assert.Equal("UNKNOWN means Unknown Tree", SpeciesAliases.All[0].InWords);
        }

        /// <summary>
        /// The lookup is the one comparison this tool makes anywhere, without case and with
        /// edge spaces off, and NEVER a prefix. UNKNOWN TREE SPECIES is not UNKNOWN.
        /// </summary>
        [Fact]
        public void TheLookupIsExactWithoutCaseAndIsNeverAPrefix()
        {
            Assert.NotNull(SpeciesAliases.For("UNKNOWN"));
            Assert.NotNull(SpeciesAliases.For("  unknown  "));
            Assert.Null(SpeciesAliases.For("UNKNOWN TREE SPECIES"));
            Assert.Null(SpeciesAliases.For("UNKNO"));
            Assert.Null(SpeciesAliases.For("CONOCARPUS"));
            Assert.Null(SpeciesAliases.For(string.Empty));
            Assert.Null(SpeciesAliases.For(null));
        }
    }
}
