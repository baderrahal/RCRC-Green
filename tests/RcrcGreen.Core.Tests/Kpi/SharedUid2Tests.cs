using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **TWO PLOTS, ONE FOLDER, AND THE LAST ONE WRITTEN REPLACED THE OTHERS.**
    ///
    /// Measured on the 16:37 press of 16 September: NS-01 and NS-42 both carried PRX_Plot_UID2
    /// ANH-007-ST-100210, and MM-01 and MM-09 to MM-15 all carried ANH-007-ST-100213. ONE
    /// WORKBOOK PER PLOT gave each group one path, THE PLOT LIST read YES for all ten, and each
    /// group took one street reference row, so MM-01 and MM-09 to MM-15 all read ROW 10 and
    /// length 0.06108. On the 16:06 press three more groups collided, and the empty DM-29 files
    /// replaced DM-11's.
    ///
    /// **EVERY EXPECTED VALUE HERE IS WRITTEN OUT BY HAND.**
    /// </summary>
    public class SharedUid2Tests
    {
        private const string Root = "C:\\out";

        private const string Street = "STREET 36m ROW";

        /// <summary>
        /// The path a plot's workbook lands at, written out of four pieces by hand rather than
        /// asked of the rule under test. The separator comes off the platform so the expectation
        /// reads the same on the Linux runner the gate uses and on the Windows machine Revit
        /// runs on.
        /// </summary>
        private static string At(string folder, string uid2)
        {
            return Path.Combine(Root, folder, uid2, uid2 + ".xlsx");
        }

        /// <summary>
        /// **MM-01 AND MM-09 SHARE ANH-007-ST-100213 AND MM-05 DOES NOT.** Both of the first two
        /// write nothing and each names the other and the value. MM-05 writes.
        /// </summary>
        [Fact]
        public void TwoPlotsSharingOneUid2WriteNothingAndEachNamesTheOther()
        {
            IReadOnlyList<SharedUid2Group> groups = SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "MM-05", "MM-09" },
                plotId => Street,
                plotId => plotId == "MM-05" ? "ANH-007-ST-100003" : "ANH-007-ST-100213"));

            // **THE VALUE IS IN THE MESSAGE.** A failure reading `expected true, got false` says
            // nothing about which plots the press would have written over each other.
            Assert.True(
                SharedUid2.Stops(groups, "MM-01"),
                "MM-01 and MM-09 both carry ANH-007-ST-100213, so ONE WORKBOOK PER PLOT files "
                + "both at " + At("STREETS", "ANH-007-ST-100213") + " and the last one written "
                + "replaces the other. That is what the 16:37 press did to ten plots with every "
                + "row of THE PLOT LIST reading YES. The values this check found: "
                + (groups.Count == 0
                    ? "none"
                    : string.Join(", ", groups.Select(one => one.Uid2).ToArray())));

            Assert.True(SharedUid2.Stops(groups, "MM-09"));
            Assert.False(SharedUid2.Stops(groups, "MM-05"));

            Assert.Equal(
                "this plot's PRX_Plot_UID2 is ANH-007-ST-100213, and MM-09 carries it too. "
                + "One workbook per plot files all 2 files at "
                + At("STREETS", "ANH-007-ST-100213")
                + ", so the last one written would replace the others. No file is written for "
                + "any of them, and nothing already in that folder is touched.",
                SharedUid2.WhyStopped(groups, "MM-01"));

            Assert.Equal(
                "this plot's PRX_Plot_UID2 is ANH-007-ST-100213, and MM-01 carries it too. "
                + "One workbook per plot files all 2 files at "
                + At("STREETS", "ANH-007-ST-100213")
                + ", so the last one written would replace the others. No file is written for "
                + "any of them, and nothing already in that folder is touched.",
                SharedUid2.WhyStopped(groups, "MM-09"));

            Assert.Equal(string.Empty, SharedUid2.WhyStopped(groups, "MM-05"));
        }

        /// <summary>
        /// **THE SAME VALUE IN TWO COMPONENT FOLDERS IS NAMED AND STILL WRITES.** Nothing
        /// collides, so refusing would cost the team two workbooks over a value that files apart.
        /// SC-03 is a school and MM-01 is a street, so one value reaches two paths.
        /// </summary>
        [Fact]
        public void OneValueInTwoComponentFoldersIsNamedAndBothStillWrite()
        {
            IReadOnlyList<SharedUid2Group> groups = SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "SC-03" },
                plotId => plotId == "SC-03" ? "SCHOOL" : Street,
                plotId => "ANH-007-ST-100213"));

            SharedUid2Group group = Assert.Single(groups);

            Assert.True(group.FiledApart);
            Assert.Empty(group.StoppedPlots);
            Assert.False(SharedUid2.Stops(groups, "MM-01"));
            Assert.False(SharedUid2.Stops(groups, "SC-03"));

            Assert.Equal(
                "this plot's PRX_Plot_UID2 is ANH-007-ST-100213, which SC-03 carries too, and "
                + "no file of this plot's collides with any of theirs, so it is written.",
                SharedUid2.FiledApartFrom(groups, "MM-01"));
        }

        /// <summary>
        /// **A GROUP CAN HOLD BOTH KINDS AT ONCE.** MM-01 and MM-09 collide with each other on
        /// ANH-007-ST-100213 and SC-03 carries the same value into the SCHOOL folder. The first
        /// two are stopped and SC-03 writes, and the line SC-03 gets says only what is true of
        /// SC-03.
        /// </summary>
        [Fact]
        public void AGroupHoldingBothKindsNamesEachPlotForItself()
        {
            IReadOnlyList<SharedUid2Group> groups = SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "MM-09", "SC-03" },
                plotId => plotId == "SC-03" ? "SCHOOL" : Street,
                plotId => "ANH-007-ST-100213"));

            SharedUid2Group group = Assert.Single(groups);

            Assert.False(group.FiledApart);
            Assert.Equal(new[] { "MM-01", "MM-09" }, group.StoppedPlots.ToArray());
            Assert.False(SharedUid2.Stops(groups, "SC-03"));

            Assert.Equal(
                "this plot's PRX_Plot_UID2 is ANH-007-ST-100213, which MM-01 and MM-09 carry "
                + "too, and no file of this plot's collides with any of theirs, so it is written.",
                SharedUid2.FiledApartFrom(groups, "SC-03"));

            Assert.Equal(
                "this plot's PRX_Plot_UID2 is ANH-007-ST-100213, and MM-09 carries it too. "
                + "One workbook per plot files all 2 files at "
                + At("STREETS", "ANH-007-ST-100213")
                + ", so the last one written would replace the others. No file is written for "
                + "any of them, and nothing already in that folder is touched.",
                SharedUid2.WhyStopped(groups, "MM-01"));
        }

        /// <summary>
        /// **A PLOT WITH NO PRX_Plot_UID2 IS LEFT OUT.** An empty value shared by five plots is
        /// not one value, and such a plot is already refused by its own path.
        /// </summary>
        [Fact]
        public void PlotsWithNoUid2AreNotAGroup()
        {
            Assert.Empty(SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "MM-05" },
                plotId => Street,
                plotId => string.Empty)));
        }

        /// <summary>
        /// The one line the glance carries, both ways round.
        /// </summary>
        [Fact]
        public void TheGlanceLineCountsTheStoppedPlotsAndNamesTheValues()
        {
            IReadOnlyList<SharedUid2Group> groups = SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "MM-09", "NS-01", "NS-42" },
                plotId => Street,
                plotId => plotId.StartsWith("MM") ? "ANH-007-ST-100213" : "ANH-007-ST-100210"));

            Assert.Equal(
                "THE PLOTS SHARING ONE PRX_Plot_UID2: 4 plots were written nowhere because "
                + "other ticked plots share its PRX_Plot_UID2 and its folder. The values: "
                + "ANH-007-ST-100213, ANH-007-ST-100210.",
                SharedUid2.InWords(groups));

            Assert.Equal(
                "THE PLOTS SHARING ONE PRX_Plot_UID2: every ticked plot's PRX_Plot_UID2 files "
                + "it on a path of its own.",
                SharedUid2.InWords(new List<SharedUid2Group>()));
        }

        /// <summary>
        /// **WHAT AN EARLIER PRESS LEFT IN THAT FOLDER IS NAMED AND LEFT WHERE IT IS.** Deleting
        /// it destroys the evidence and the team's own earlier work, and saying nothing about it
        /// leaves somebody opening a folder tree.
        /// </summary>
        [Fact]
        public void TheFilesAlreadyThereAreNamedAndNothingDeletesThem()
        {
            Assert.Equal(
                "an earlier press left 2 files in that folder, ANH-007-ST-100213.xlsx, "
                + "ANH-007-ST-100213.pdf, and nothing here deletes or moves any of them",
                SharedUid2.AlreadyThere(new[]
                {
                    "ANH-007-ST-100213.xlsx",
                    "ANH-007-ST-100213.pdf"
                }));

            Assert.Equal(
                "that folder holds no file of this plot's from an earlier press",
                SharedUid2.AlreadyThere(new string[0]));
        }

        /// <summary>
        /// **THE SHORT LINE AND THE LONG ONE ANSWER THE SAME QUESTION.** The plot list's row
        /// carries the short one and the report block the long one, and a row saying a plot was
        /// held back beside a press that wrote its file is the shape this repository keeps
        /// paying for.
        /// </summary>
        [Fact]
        public void TheShortLineFiresExactlyWhereTheLongOneDoes()
        {
            IReadOnlyList<SharedUid2Group> groups = SharedUid2.Of(PlotFilings.Of(
                Root,
                new[] { "MM-01", "MM-05", "MM-09" },
                plotId => Street,
                plotId => plotId == "MM-05" ? "ANH-007-ST-100003" : "ANH-007-ST-100213"));

            Assert.Equal(
                "PRX_Plot_UID2 ANH-007-ST-100213 is also MM-09's, so no file is written for any "
                + "of them",
                SharedUid2.StoppedShort(groups, "MM-01"));

            Assert.Equal(string.Empty, SharedUid2.StoppedShort(groups, "MM-05"));
        }
    }
}
