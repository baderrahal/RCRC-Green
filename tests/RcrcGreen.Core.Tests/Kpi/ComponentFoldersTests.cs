using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The folder each component value is filed under. Every expected value written out by hand,
    /// because one wrong letter makes a second folder beside the team's and nobody sees it.
    /// </summary>
    public class ComponentFoldersTests
    {
        /// <summary>
        /// All eleven, each spelt out. SCHOOL singular, EXISTING PARK singular, FUTURE PARKS
        /// plural: that is not a pattern, which is why it is a table.
        /// </summary>
        [Theory]
        [InlineData("DAILY MOSQUE", "DAILY MOSQUE")]
        [InlineData("FRIDAY MOSQUE", "FRIDAY MOSQUE")]
        [InlineData("SCHOOL", "SCHOOL")]
        [InlineData("HEALTH", "HEALTHCARE")]
        [InlineData("PARKING LOT", "PARKING LOT")]
        [InlineData("EXISTING PARK", "EXISTING PARK")]
        [InlineData("FUTURE PARK", "FUTURE PARKS")]
        [InlineData("NH STRT 20m ROW", "STREETS")]
        [InlineData("NH STRT LESS 20m ROW", "STREETS")]
        [InlineData("STREET 30m ROW", "STREETS")]
        [InlineData("STREET 36m ROW", "STREETS")]
        public void EveryComponentValueReachesItsFolder(string component, string folder)
        {
            Assert.Equal(folder, ComponentFolders.For(component));
        }

        /// <summary>
        /// Eleven values and the folders they reach, written out here rather than counted off
        /// the table. **The round message said nine folders and the table it gave reaches
        /// eight**, so the number is asserted rather than left to be read off the list. The
        /// team's snip also holds GOVERMENT BUILDING, which no plot in either measured model
        /// carries a component for, and it is deliberately not in the table.
        /// </summary>
        [Fact]
        public void ElevenValuesReachEightFolders()
        {
            Assert.Equal(11, ComponentFolders.All.Count);

            Assert.Equal(
                new[]
                {
                    "DAILY MOSQUE", "FRIDAY MOSQUE", "SCHOOL", "HEALTHCARE",
                    "PARKING LOT", "EXISTING PARK", "FUTURE PARKS", "STREETS"
                },
                ComponentFolders.Folders.ToArray());
        }

        /// <summary>
        /// Four street values share one folder and the two mosque values do not share theirs,
        /// which is the whole reason this is a second table beside the template one.
        /// </summary>
        [Fact]
        public void TheMosqueValuesSplitWhereTheStreetValuesJoin()
        {
            Assert.Equal(
                new[] { "NH STRT 20m ROW", "NH STRT LESS 20m ROW", "STREET 30m ROW", "STREET 36m ROW" },
                ComponentFolders.All.Where(one => one.Folder == "STREETS")
                    .Select(one => one.Component).ToArray());

            // Both mosque values fill the one MOSQUES workbook and are filed apart.
            Assert.Equal("MOSQUES", ComponentTemplates.For("DAILY MOSQUE").Name);
            Assert.Equal("MOSQUES", ComponentTemplates.For("FRIDAY MOSQUE").Name);
            Assert.Equal("DAILY MOSQUE", ComponentFolders.For("DAILY MOSQUE"));
            Assert.Equal("FRIDAY MOSQUE", ComponentFolders.For("FRIDAY MOSQUE"));
        }

        /// <summary>
        /// Every value the template table holds is in this one too, so a plot that can be filled
        /// can be filed. They are separate tables and nothing keeps them level but this.
        /// </summary>
        [Fact]
        public void EveryComponentThatPicksATemplateAlsoPicksAFolder()
        {
            Assert.All(
                ComponentTemplates.All,
                one => Assert.NotEqual(string.Empty, ComponentFolders.For(one.Component)));
        }

        /// <summary>
        /// A value the table does not hold places nothing and is named. Nothing falls back to
        /// the template name, so GOVERMENT BUILDING builds no folder until somebody measures it.
        /// </summary>
        [Fact]
        public void AValueTheTableDoesNotHoldPlacesNothingAndIsNamed()
        {
            Assert.Equal(string.Empty, ComponentFolders.For("GOVERMENT BUILDING"));
            Assert.Equal(string.Empty, ComponentFolders.For("PUMP STATION"));
            Assert.Equal(string.Empty, ComponentFolders.For(string.Empty));
            Assert.Equal(string.Empty, ComponentFolders.For(null));

            Assert.Equal(
                "the component folder table does not hold GOVERMENT BUILDING, so nothing says "
                + "which folder this plot is filed under",
                ComponentFolders.NoFolderFor("GOVERMENT BUILDING"));
            Assert.Equal(
                "the component folder table does not hold an empty component, so nothing says "
                + "which folder this plot is filed under",
                ComponentFolders.NoFolderFor("   "));
        }

        /// <summary>
        /// The whole value is compared, never an opening. NH STRT 20m ROW is the start of
        /// nothing and NH STRT LESS 20m ROW is a different value that happens to share five
        /// characters with it.
        /// </summary>
        [Fact]
        public void TheWholeValueIsComparedAndNeverAnOpening()
        {
            Assert.Equal("STREETS", ComponentFolders.For("  nh strt less 20m row  "));
            Assert.Equal(string.Empty, ComponentFolders.For("NH STRT"));
            Assert.Equal(string.Empty, ComponentFolders.For("NH STRT 20m ROW EXTRA"));
        }
    }

    /// <summary>
    /// The tree one plot's checklist goes into. The separator is the platform's own, so the
    /// segments are written out by hand and joined with it rather than a path being typed with
    /// a backslash that no test machine here would match.
    /// </summary>
    public class PlotWorkbookPathTests
    {
        private static string Joined(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        /// <summary>
        /// The team's own folders, off the snip: the root, FRIDAY MOSQUE, the UID2, and the
        /// workbook named after its folder.
        /// </summary>
        [Fact]
        public void ThePathIsTheRootTheFolderTheUidAndTheUidAgain()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("C:", "RCRC", "MUGHARAZAT"), "FRIDAY MOSQUE", "ANH-008-MO-100006");

            Assert.True(path.Ok);
            Assert.Equal("FRIDAY MOSQUE", path.Folder);
            Assert.Equal("ANH-008-MO-100006", path.Uid2);
            Assert.Equal(
                Joined("C:", "RCRC", "MUGHARAZAT", "FRIDAY MOSQUE", "ANH-008-MO-100006"),
                path.FolderPath);
            Assert.Equal(
                Joined("C:", "RCRC", "MUGHARAZAT", "FRIDAY MOSQUE", "ANH-008-MO-100006",
                    "ANH-008-MO-100006.xlsx"),
                path.FilePath);
            Assert.Equal(string.Empty, path.Why);
        }

        /// <summary>
        /// A street plot, where four component values land in the one folder and the plot's own
        /// folder is still its own.
        /// </summary>
        [Fact]
        public void FourStreetValuesFileIntoOneFolderAndKeepTheirOwnPlotFolders()
        {
            PlotWorkbookPath wide = PlotWorkbookPath.For(
                Joined("R"), "STREET 36m ROW", "ANH-007-ST-100210");
            PlotWorkbookPath narrow = PlotWorkbookPath.For(
                Joined("R"), "NH STRT LESS 20m ROW", "ANH-007-ST-100050");

            Assert.Equal(Joined("R", "STREETS", "ANH-007-ST-100210", "ANH-007-ST-100210.xlsx"),
                wide.FilePath);
            Assert.Equal(Joined("R", "STREETS", "ANH-007-ST-100050", "ANH-007-ST-100050.xlsx"),
                narrow.FilePath);
        }

        [Fact]
        public void NoRootIsARefusalThatSaysSo()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For("  ", "FRIDAY MOSQUE", "ANH-008-MO-100006");

            Assert.False(path.Ok);
            Assert.Equal(string.Empty, path.FilePath);
            Assert.Equal(
                "no output root is set, so there is nowhere to build the folder tree", path.Why);
        }

        [Fact]
        public void AComponentTheTableDoesNotHoldWritesNothingAndIsNamed()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(
                Joined("R"), "GOVERMENT BUILDING", "ANH-007-GB-100001");

            Assert.False(path.Ok);
            Assert.Equal(
                "the component folder table does not hold GOVERMENT BUILDING, so nothing says "
                + "which folder this plot is filed under",
                path.Why);
        }

        [Fact]
        public void NoUidMeansNoFolderAndNoFileBecauseBothAreNamedAfterIt()
        {
            PlotWorkbookPath path = PlotWorkbookPath.For(Joined("R"), "SCHOOL", "   ");

            Assert.False(path.Ok);
            Assert.Equal(
                "no PRX_Plot_UID2 was read off this plot's first sheet, and the folder and the "
                + "file are both named after it",
                path.Why);
        }

        /// <summary>
        /// **A UID2 carrying a separator refuses rather than being cleaned.** Cleaned, it would
        /// file the plot under a name the team never searches for, and nothing would say so.
        /// Both separators refuse whichever machine this runs on.
        /// </summary>
        [Fact]
        public void AUidThatWouldNotSitInAPathRefusesRatherThanBeingCleaned()
        {
            foreach (string separator in new[] { "/", "\\" })
            {
                PlotWorkbookPath slashed = PlotWorkbookPath.For(
                    Joined("R"), "SCHOOL", "ANH" + separator + "007");

                Assert.False(slashed.Ok);
                Assert.Equal(
                    "PRX_Plot_UID2 reads ANH" + separator + "007, which holds a path separator, "
                    + "so it cannot name a folder",
                    slashed.Why);
            }
        }

        /// <summary>
        /// **The refused characters are Windows's, written out, and not the running machine's.**
        /// `Path.GetInvalidFileNameChars` names nine and the control characters on Windows, where
        /// Revit runs, and two on the Linux runner this gate uses. Asked of the platform, this
        /// test would pass on the runner while the tool refused the same name on a real machine.
        /// </summary>
        [Fact]
        public void TheRefusedCharactersAreWindowsOwnRatherThanTheRunningMachines()
        {
            Assert.Equal(
                new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' },
                PlotWorkbookPath.RefusedInAName);

            PlotWorkbookPath starred = PlotWorkbookPath.For(Joined("R"), "SCHOOL", "ANH*007?");

            Assert.False(starred.Ok);
            Assert.Equal(
                "PRX_Plot_UID2 reads ANH*007?, which holds '*' '?', so it cannot name a folder",
                starred.Why);

            PlotWorkbookPath colon = PlotWorkbookPath.For(Joined("R"), "SCHOOL", "ANH:007");

            Assert.False(colon.Ok);
            Assert.Equal(
                "PRX_Plot_UID2 reads ANH:007, which holds ':', so it cannot name a folder",
                colon.Why);
        }
    }
}
