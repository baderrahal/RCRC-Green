using System.Collections.Generic;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    public class ScheduleGroupsTests
    {
        private static ScannedSchedule Printing(params string[][] rows)
        {
            return KpiFixture.Schedule("DM-12-(600) SOFTSCAPE SCHEDULE",
                fields: new[] { KpiFixture.Field("BOTANICAL NAME") },
                rowsWereRead: true,
                bodyRowCount: rows.Length,
                rows: rows);
        }

        /// <summary>
        /// A group row and a subtotal row are the same shape, one cell with text and the rest
        /// empty. Only the text tells them apart, so the phases the document really holds
        /// decide it. A rule on the shape alone would call every subtotal a group.
        /// </summary>
        [Fact]
        public void ASubtotalRowIsNotAGroupRowEvenThoughItHasOneCellFilled()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Existing", "" },
                new[] { "ALBIZIA LEBBECK", "1" },
                new[] { "", "10" },
                new[] { "Proposed", "" },
                new[] { "ALBIZIA LEBBECK", "13" },
                new[] { "", "39" });

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, new[] { "Existing", "Proposed" });

            Assert.Equal(2, groups.Count);
            Assert.Equal("Existing", groups[0].Name);
            Assert.Equal(1, groups[0].RowIndex);
            Assert.Equal(2, groups[0].RowsUnder);
            Assert.Equal(1, groups[0].NamedRowsUnder);
            Assert.Equal("Proposed", groups[1].Name);
            Assert.Equal(4, groups[1].RowIndex);
            Assert.Equal(2, groups[1].RowsUnder);
            Assert.Equal(1, groups[1].NamedRowsUnder);
        }

        /// <summary>
        /// TREES is a group row too, for the category rather than the phase, and it is not one
        /// of the phases so it is not counted. Nothing here knows the word TREES.
        /// </summary>
        [Fact]
        public void ARowNamingSomethingThatIsNotAPhaseIsNotAGroupRow()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "TREES", "" },
                new[] { "Existing", "" },
                new[] { "ALBIZIA LEBBECK", "1" });

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, new[] { "Existing", "Proposed" });

            Assert.Single(groups);
            Assert.Equal("Existing", groups[0].Name);
            Assert.Equal(2, groups[0].RowIndex);
        }

        /// <summary>
        /// The phases come off the document, so a project whose phases are named anything else
        /// is read the same way. Existing and Proposed are not written into the code.
        /// </summary>
        [Fact]
        public void ThePhaseNamesTheDocumentHoldsAreWhatIsMatched()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Phase 1", "" },
                new[] { "ALBIZIA LEBBECK", "1" });

            Assert.Single(ScheduleGroups.Of(schedule, new[] { "Phase 1" }));
            Assert.Empty(ScheduleGroups.Of(schedule, new[] { "Existing", "Proposed" }));
        }

        [Fact]
        public void APhasePrintedInCapitalsIsStillThePhase()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "EXISTING", "" },
                new[] { "ALBIZIA LEBBECK", "1" });

            Assert.Equal("EXISTING", ScheduleGroups.Of(schedule, new[] { "Existing" })[0].Name);
        }

        [Fact]
        public void ARowWithTwoCellsFilledIsNeverAGroupRow()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Existing", "4" });

            Assert.Empty(ScheduleGroups.Of(schedule, new[] { "Existing" }));
        }

        [Fact]
        public void AScheduleWhoseRowsWereNotReadHasNoGroups()
        {
            ScannedSchedule schedule = KpiFixture.Schedule("DM-12-(600) SOFTSCAPE SCHEDULE", rowsRefused: true);

            Assert.Empty(ScheduleGroups.Of(schedule, new[] { "Existing" }));
        }

        [Fact]
        public void NoPhaseNamesMeansNoGroupsRatherThanEveryOneCellRow()
        {
            ScannedSchedule schedule = Printing(
                new[] { "BOTANICAL NAME", "COUNT (n)" },
                new[] { "Existing", "" });

            Assert.Empty(ScheduleGroups.Of(schedule, null));
        }
    }
}
