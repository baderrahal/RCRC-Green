using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// The snapshot feeds the panel everything, and it had no test at all. An audit broke
    /// three of its rules on purpose and the suite stayed green every time, so each of the
    /// three is pinned here: the numbers grouped per plot, the scope box names narrowed to
    /// plot identifiers, and the free numbers built from the numbers rather than the names.
    /// </summary>
    public class DrawingSheetSnapshotTests
    {
        private static DrawingSheetSnapshot Snapshot(
            IEnumerable<string> plotIds = null,
            IEnumerable<string> scopeBoxNames = null,
            IEnumerable<string> sheetNamesInUse = null,
            IEnumerable<string> sheetNumbersInUse = null,
            IEnumerable<SheetOnAPlot> sheetNumbersByPlot = null,
            IEnumerable<PlotRecord> plots = null,
            IEnumerable<ViewType> capturableScheduleTypes = null,
            IEnumerable<UncapturableSchedule> uncapturableSchedules = null)
        {
            return new DrawingSheetSnapshot(
                "NG05",
                new DateTime(2026, 9, 10, 12, 0, 0),
                plotIds,
                null,
                null,
                null,
                null,
                scopeBoxNames,
                null,
                null,
                sheetNamesInUse,
                sheetNumbersInUse,
                0,
                0,
                0,
                0,
                0,
                sheetNumbersByPlot,
                plots,
                capturableScheduleTypes,
                uncapturableSchedules);
        }

        /// <summary>
        /// The plot letter a proposed sheet number continues is read off these lists, so a
        /// grouping that came back empty would silently kill every number proposal.
        /// </summary>
        [Fact]
        public void TheNumbersAreGroupedByTheirOwnPlot()
        {
            DrawingSheetSnapshot snapshot = Snapshot(sheetNumbersByPlot: new[]
            {
                new SheetOnAPlot("DM-11", "010QE"),
                new SheetOnAPlot("DM-11", "200Q"),
                new SheetOnAPlot("DM-12", "010RA")
            });

            Assert.Equal(new[] { "010QE", "200Q" }, snapshot.NumbersOnPlot("DM-11"));
            Assert.Equal(new[] { "010RA" }, snapshot.NumbersOnPlot("DM-12"));
        }

        [Fact]
        public void APlotWithNoSheetsHasNoNumbersRatherThanAThrow()
        {
            DrawingSheetSnapshot snapshot = Snapshot(sheetNumbersByPlot: new[]
            {
                new SheetOnAPlot("DM-11", "010QE")
            });

            Assert.Empty(snapshot.NumbersOnPlot("DM-13"));
            Assert.Empty(snapshot.NumbersOnPlot(null));
        }

        [Fact]
        public void ABlankPlotOrNumberNeverBecomesAGroup()
        {
            DrawingSheetSnapshot snapshot = Snapshot(sheetNumbersByPlot: new[]
            {
                new SheetOnAPlot("", "010QE"),
                new SheetOnAPlot("DM-11", "   ")
            });

            Assert.Empty(snapshot.NumbersOnPlot(""));
            Assert.Empty(snapshot.NumbersOnPlot("DM-11"));
        }

        /// <summary>
        /// 406 scope boxes stand for 160 plots on the real model, so most box names are not
        /// plots at all. Handing the run every box name would refuse and promise the wrong
        /// plots, and the filter dropped in the audit left the suite green.
        /// </summary>
        [Fact]
        public void OnlyABoxNamedExactlyAsAPlotCountsAsAPlotsScopeBox()
        {
            DrawingSheetSnapshot snapshot = Snapshot(
                scopeBoxNames: new[] { "DM-41", "Site Box", "dm-7", "DM-41 working" });

            Assert.Equal(new[] { "DM-41" }, snapshot.PlotsWithAScopeBox);
        }

        /// <summary>
        /// The dropdown offers numbers stepped on from the NUMBERS in use. Fed the names, it
        /// would offer LIST OF DRAWINGS 10, and the audit proved nothing failed when it was.
        /// </summary>
        [Fact]
        public void TheFreeNumbersComeFromTheNumbersAndNeverTheNames()
        {
            DrawingSheetSnapshot snapshot = Snapshot(
                sheetNamesInUse: new[] { "LIST OF DRAWINGS 9" },
                sheetNumbersInUse: new[] { "L-211" });

            Assert.Equal(new[] { "L-212" }, snapshot.FreeSheetNumbers);
        }

        /// <summary>
        /// The plot records carry the sources the panel shows, and the id list can never lose
        /// a plot the records hold, whichever of the two the reader filled first.
        /// </summary>
        [Fact]
        public void ThePlotListIsTheUnionOfTheIdsAndTheRecords()
        {
            DrawingSheetSnapshot snapshot = Snapshot(
                plotIds: new[] { "DM-41" },
                plots: new[]
                {
                    new PlotRecord("AB-7", new[] { PlotSource.ScopeBox, PlotSource.ElementParameter })
                });

            Assert.Equal(new[] { "AB-7", "DM-41" }, snapshot.PlotIds);
            Assert.Equal("box and elements, no views", snapshot.RecordOf("AB-7").NoViewsInWords());
            Assert.Null(snapshot.RecordOf("DM-41"));
        }

        [Fact]
        public void TheCapturableAndUncapturableSchedulesComeBackAsGiven()
        {
            var hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");
            var kerbs = new ViewType("600", "KERBS SCHEDULE");

            DrawingSheetSnapshot snapshot = Snapshot(
                capturableScheduleTypes: new[] { hardscape, hardscape, null },
                uncapturableSchedules: new[]
                {
                    new UncapturableSchedule(kerbs, "That schedule exists and filters on no plot.")
                });

            Assert.Equal(new[] { hardscape }, snapshot.CapturableScheduleTypes);
            UncapturableSchedule only = Assert.Single(snapshot.UncapturableSchedules);
            Assert.Equal(kerbs, only.Type);
            Assert.Equal("That schedule exists and filters on no plot.", only.Why);
        }
    }
}
