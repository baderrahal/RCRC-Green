using System;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// **Each value in its own folder, flat or grouped, K78.** Bader's rule of 29 October. The
    /// expected folders are his table, typed in by hand, and every path is built off the shipped
    /// install/component-rules.csv the add-in reads.
    /// </summary>
    public class FolderLayoutTests
    {
        private const string Uid2 = "ANH-008-MO-100006";

        private static string Joined(params string[] parts)
        {
            return string.Join(Path.DirectorySeparatorChar.ToString(), parts);
        }

        /// <summary>The prompt's table, written with backslashes and read on this machine's separator.</summary>
        private static string Local(string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar);
        }

        [Theory]
        [InlineData("FRIDAY MOSQUE", @"FRIDAY MOSQUE", @"FRIDAY MOSQUE\FRIDAY MOSQUE")]
        [InlineData("FRIDAY MOSQUE MORE THAN 60%", @"FRIDAY MOSQUE MORE THAN 60%", @"FRIDAY MOSQUE\FRIDAY MOSQUE MORE THAN 60%")]
        [InlineData("DAILY MOSQUE LESS THAN 60%", @"DAILY MOSQUE LESS THAN 60%", @"DAILY MOSQUE\DAILY MOSQUE LESS THAN 60%")]
        [InlineData("SCHOOL MORE THAN 60%", @"SCHOOL MORE THAN 60%", @"SCHOOL\SCHOOL MORE THAN 60%")]
        [InlineData("NH STRT 20m ROW", @"NH STREET 20m ROW", @"STREETS\NH STREET 20m ROW")]
        [InlineData("NH STREET 20m ROW", @"NH STREET 20m ROW", @"STREETS\NH STREET 20m ROW")]
        [InlineData("NH STRT LESS 20m ROW", @"NH STREET LESS THAN 20m ROW", @"STREETS\NH STREET LESS THAN 20m ROW")]
        [InlineData("WALKWAY", @"WALKWAY", @"STREETS\WALKWAY")]
        [InlineData("HEALTH", @"HEALTHCARE", @"HEALTHCARE")]
        [InlineData("FUTURE PARK", @"FUTURE PARKS", @"FUTURE PARKS")]
        [InlineData("PARKING LOT", @"PARKING LOT", @"PARKING LOT")]
        public void BadersTableHoldsInBothLayouts(string value, string flat, string grouped)
        {
            KpiTemplate template = ComponentTemplates.For(value);
            Assert.NotNull(template);

            PlotWorkbookPath inFlat = PlotWorkbookPath.For(FolderLayout.Flat, "Output", template, value, Uid2);
            PlotWorkbookPath inGroups = PlotWorkbookPath.For(FolderLayout.Grouped, "Output", template, value, Uid2);

            Assert.True(inFlat.Ok, inFlat.Why);
            Assert.True(inGroups.Ok, inGroups.Why);
            Assert.Equal(Local(flat), inFlat.Folder);
            Assert.Equal(Local(grouped), inGroups.Folder);

            // Every one then UID2, with the workbook named after it inside, and the PDF beside it.
            Assert.Equal(Joined("Output", Local(flat), Uid2, Uid2 + ".xlsx"), inFlat.FilePath);
            Assert.Equal(Joined("Output", Local(grouped), Uid2, Uid2 + ".xlsx"), inGroups.FilePath);
            Assert.Equal(Joined("Output", Local(grouped), Uid2), inGroups.FolderPath);
        }

        /// <summary>The same value typed two ways shares one folder in both layouts.</summary>
        [Fact]
        public void TwoSpellingsOfOneStreetValueShareOneFolder()
        {
            Assert.Equal(
                PlotWorkbookPath.For(FolderLayout.Grouped, "R", KpiTemplates.Streets, "NH STRT LESS 20m ROW", Uid2).FilePath,
                PlotWorkbookPath.For(FolderLayout.Grouped, "R", KpiTemplates.Streets, "NH STREET LESS THAN 20m ROW", Uid2).FilePath);
            Assert.Equal(
                PlotWorkbookPath.For(FolderLayout.Flat, "R", KpiTemplates.Streets, "nh strt 20M row", Uid2).FilePath,
                PlotWorkbookPath.For(FolderLayout.Flat, "R", KpiTemplates.Streets, "NH STREET 20m ROW", Uid2).FilePath);
        }

        /// <summary>No layout picked builds no path and says so, the refusal Create is held by.</summary>
        [Fact]
        public void NoLayoutBuildsNoPath()
        {
            PlotWorkbookPath none = PlotWorkbookPath.For(FolderLayout.None, "R", KpiTemplates.Mosques, "FRIDAY MOSQUE", Uid2);

            Assert.False(none.Ok);
            Assert.Equal(PathRefusal.NoFolderLayout, none.RefusedAs);
            Assert.Equal("no folder layout is picked in Setup, so nothing says how the folders nest", none.Why);
            Assert.Equal(NotWrittenKind.NoFolderLayout, NotWrittenKinds.ForPath(none));
        }

        /// <summary>
        /// A plot with no component keeps its rule: a one level template files under its one
        /// folder in both layouts, and STREETS, a folder per value now, is refused.
        /// </summary>
        [Fact]
        public void APlotWithNoComponentFilesOneLevelOrIsRefused()
        {
            Assert.Equal(Joined("R", "HEALTHCARE", Uid2), PlotWorkbookPath.For(FolderLayout.Grouped, "R", KpiTemplates.Healthcare, "", Uid2).FolderPath);
            Assert.Equal(Joined("R", "EXISTING PARK", Uid2), PlotWorkbookPath.For(FolderLayout.Flat, "R", KpiTemplates.ExistingParks, null, Uid2).FolderPath);

            PlotWorkbookPath street = PlotWorkbookPath.For(FolderLayout.Grouped, "R", KpiTemplates.Streets, "", Uid2);
            Assert.False(street.Ok);
            Assert.Equal(PathRefusal.TemplateReachesTwoFolders, street.RefusedAs);
        }
    }

    /// <summary>The tidy words and the base, in one place, K78.</summary>
    public class ComponentFolderNameTests
    {
        [Theory]
        [InlineData("NH STRT 20m ROW", "NH STREET 20m ROW")]
        [InlineData("NH STREET 20m ROW", "NH STREET 20m ROW")]
        [InlineData("NH STRT LESS 20m ROW", "NH STREET LESS THAN 20m ROW")]
        [InlineData("NH STREET LESS THAN 20m ROW", "NH STREET LESS THAN 20m ROW")]
        [InlineData("nh strt less 20M row", "NH STREET LESS THAN 20m ROW")]
        [InlineData("STREET 36m ROW", "STREET 36m ROW")]
        [InlineData("NH STREET LESS 12.5m ROW", "NH STREET LESS THAN 12.5m ROW")]
        [InlineData("DAILY MOSQUE LESSTHAN 60 %", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("  FRIDAY   MOSQUE ", "FRIDAY MOSQUE")]
        [InlineData("STRTS", "STRTS")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void AValueIsTidiedIntoItsFolderName(string value, string folder)
        {
            Assert.Equal(folder, ComponentFolderName.Tidy(value));
        }

        /// <summary>LESS before anything but a width in metres is left alone, so LESS 60 stays.</summary>
        [Fact]
        public void LessIsReadAsLessThanOnlyBeforeAWidth()
        {
            Assert.Equal("SCHOOL LESS 60%", ComponentFolderName.Tidy("SCHOOL LESS 60%"));
            Assert.Equal("NH LESS ROW", ComponentFolderName.Tidy("NH LESS ROW"));
        }

        [Theory]
        [InlineData("FRIDAY MOSQUE", "FRIDAY MOSQUE")]
        [InlineData("FRIDAY MOSQUE MORE THAN 60%", "FRIDAY MOSQUE")]
        [InlineData("DAILY MOSQUE LESS THAN 60%", "DAILY MOSQUE")]
        [InlineData("DAILY MOSQUE LESSTHAN 60 %", "DAILY MOSQUE")]
        [InlineData("SCHOOL MORE THAN 60%", "SCHOOL")]
        [InlineData("SCHOOL MORE 60", "SCHOOL")]
        [InlineData("FRIDAY MOSQUE MORE THAN HALF", "FRIDAY MOSQUE MORE THAN HALF")]
        [InlineData("MORE THAN 60%", "MORE THAN 60%")]
        public void TheBaseIsTheValueWithoutItsSixtyPerCentPart(string value, string main)
        {
            Assert.Equal(main, ComponentFolderName.Base(value));
        }
    }

    /// <summary>The Main column, read off a rules file, and the layout's own words.</summary>
    public class FolderLayoutRulesTests
    {
        private static ComponentTable Table(params string[] rules)
        {
            return ComponentTable.Layered().WithRules(ComponentRuleFile.Parse(
                ComponentSource.Shipped, "r.csv", new[] { ComponentRules.Header }.Concat(rules).ToArray()));
        }

        [Fact]
        public void MainIsAFixedNameTheBaseOrEmpty()
        {
            ComponentTable table = Table(
                "MOSQUE,MOSQUES,value,base",
                "ROW,STREETS,value,STREETS",
                "HEALTH,HEALTHCARE,HEALTHCARE,",
                "PARKING,PARKING,PARKING LOT");

            Assert.Equal("FRIDAY MOSQUE", table.PlaceOf("FRIDAY MOSQUE MORE THAN 60%").Main);
            Assert.Equal("STREETS", table.PlaceOf("NH STRT 20m ROW").Main);
            Assert.Equal(string.Empty, table.PlaceOf("HEALTH").Main);
            Assert.Equal(string.Empty, table.PlaceOf("PARKING LOT").Main);

            ComponentPlacement mosque = table.PlaceOf("FRIDAY MOSQUE MORE THAN 60%");
            ComponentPlacement health = table.PlaceOf("HEALTH");
            Assert.Equal(Path.Combine("FRIDAY MOSQUE", "FRIDAY MOSQUE MORE THAN 60%"),
                FolderLayouts.Under(FolderLayout.Grouped, mosque.Folder, mosque.Main));
            Assert.Equal("FRIDAY MOSQUE MORE THAN 60%", FolderLayouts.Under(FolderLayout.Flat, mosque.Folder, mosque.Main));
            Assert.Equal("HEALTHCARE", FolderLayouts.Under(FolderLayout.Grouped, health.Folder, health.Main));
            Assert.Equal(string.Empty, FolderLayouts.Under(FolderLayout.None, health.Folder, health.Main));
        }

        /// <summary>
        /// A template whose one folder sits under one fixed main is still one folder, and a
        /// plot with no component goes under that main in Grouped.
        /// </summary>
        [Fact]
        public void OneFolderUnderOneMainIsStillOneFilingAndTwoMainsAreNot()
        {
            string main;
            Assert.Equal("PARKING LOT", Table("PARKING,PARKING,PARKING LOT,CARS").OnlyFolderFor(KpiTemplates.Parking, out main));
            Assert.Equal("CARS", main);

            Assert.Equal(string.Empty, Table("PARKING,PARKING,PARKING LOT,CARS", "LOT,PARKING,PARKING LOT,").OnlyFolderFor(KpiTemplates.Parking, out main));
            Assert.Equal(string.Empty, main);
        }

        [Fact]
        public void TheLayoutIsRememberedByNameAndNothingElseIsALayout()
        {
            Assert.Equal(FolderLayout.Flat, FolderLayouts.Parse(" flat "));
            Assert.Equal(FolderLayout.Grouped, FolderLayouts.Parse("Grouped"));
            Assert.Equal(FolderLayout.None, FolderLayouts.Parse(""));
            Assert.Equal(FolderLayout.None, FolderLayouts.Parse(null));
            Assert.Equal(FolderLayout.None, FolderLayouts.Parse("Nested"));
            Assert.Equal("Flat", FolderLayouts.Name(FolderLayout.Flat));
            Assert.Equal("Grouped", FolderLayouts.Name(FolderLayout.Grouped));
            Assert.Equal(string.Empty, FolderLayouts.Name(FolderLayout.None));
        }

        /// <summary>The unknown value box suggests the same folder a rule filing under the value gives.</summary>
        [Fact]
        public void TheSuggestedFolderIsTheTidyName()
        {
            Assert.Equal("NH STREET LESS THAN 25m ROW", UnknownComponents.FolderFor(" nh strt less 25m row "));
        }
    }

    /// <summary>No default layout: Create is off with Bader's line until one is picked, K78.</summary>
    public class FolderLayoutGateTests
    {
        [Fact]
        public void CreateIsOffUntilALayoutIsPicked()
        {
            KpiSteps none = KpiSteps.Of(true, true, true, true, true, true, 154, 1, 154, false, FolderLayout.None);

            Assert.Equal("Pick a folder layout in Setup", none.CreateBlockedBy);
            Assert.False(none.For(KpiStep.Setup).Done);
            Assert.True(none.For(KpiStep.Read).Usable);
            Assert.Equal("Setup: 1 not set.", none.For(KpiStep.Setup).Tooltip);
            Assert.True(CreateBlocked.NoLayout.Split(' ').Length <= 10);

            KpiSteps flat = KpiSteps.Of(true, true, true, true, true, true, 154, 1, 154, false, FolderLayout.Flat);
            Assert.Equal(string.Empty, flat.CreateBlockedBy);
            Assert.True(flat.For(KpiStep.Setup).Done);
        }

        /// <summary>
        /// The example Setup and BEFORE THE PRESS print shows both trees, so a Grouped press does
        /// not print the Flat one as its own.
        /// </summary>
        [Fact]
        public void TheExamplePathShowsBothLayouts()
        {
            string said = CreateWords.WhereTheWorkbooksGo(@"C:\out");

            Assert.EndsWith(
                @"Flat: C:\out\FRIDAY MOSQUE\ANH-008-MO-100006\ANH-008-MO-100006.xlsx. "
                + @"Grouped: C:\out\FRIDAY MOSQUE\FRIDAY MOSQUE\ANH-008-MO-100006\ANH-008-MO-100006.xlsx.",
                said);
        }

        /// <summary>The setup line names the layout among what is not set, and the press refuses on it.</summary>
        [Fact]
        public void TheSetupLineAndThePressBothNameIt()
        {
            Assert.Equal(
                "Not set: the folder layout is not picked, Flat or Grouped.",
                KpiSteps.StillNotSet(KpiSteps.NotSet(true, true, true, true, true, false)));

            Assert.Equal(
                "Cannot create. No folder layout is picked in Setup, Flat or Grouped, so nothing says how the folders nest.",
                CreateWords.CannotCreate(OpenModel.Of("NG05"), "C:\\out", true, true, FolderLayout.None));
            Assert.Equal(string.Empty, CreateWords.CannotCreate(OpenModel.Of("NG05"), "C:\\out", true, true, FolderLayout.Grouped));

            // A missing output folder still comes first on the short line.
            Assert.Equal(CreateBlocked.NoOutputFolder, CreateBlocked.Reason(true, true, false, true, true, true, string.Empty, false));
            Assert.Equal(CreateBlocked.NoLayout, CreateBlocked.Reason(true, true, true, true, true, true, string.Empty, false));
        }

        /// <summary>The create report's first lines name the layout the press used.</summary>
        [Theory]
        [InlineData(FolderLayout.Flat, @"Folder layout: Flat. Output\<value folder>\<UID2>, and the one level types Output\<folder>\<UID2>.")]
        [InlineData(FolderLayout.Grouped, @"Folder layout: Grouped. Output\<main>\<value folder>\<UID2>, and the one level types Output\<folder>\<UID2>.")]
        [InlineData(FolderLayout.None, "Folder layout: none picked, so no path was built.")]
        public void TheReportsFirstLinesNameTheLayout(FolderLayout layout, string line)
        {
            var set = new KpiCreateRunSet(
                "RCRC_NG05",
                new TemplateSplit(new System.Collections.Generic.List<TemplateShare>(),
                    new System.Collections.Generic.List<PlotTemplate>(), new System.Collections.Generic.List<PlotTemplate>()),
                null, null, layout: layout);

            string[] lines = KpiCreateReport.WriteAll(set, new DateTime(2026, 10, 29, 9, 0, 0))
                .Split(new[] { "\r\n" }, StringSplitOptions.None);

            Assert.Equal(line, lines[0]);
            Assert.Equal("== WHAT IS IN THIS FILE (12) ==", lines[1]);
            Assert.Single(lines, one => one.StartsWith("Folder layout: ", StringComparison.Ordinal));
        }
    }
}
