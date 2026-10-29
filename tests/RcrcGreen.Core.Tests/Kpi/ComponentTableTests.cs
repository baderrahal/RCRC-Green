using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The spacing rule, written out by hand. It forgives how a value was typed and never lets a
    /// part of one match.
    /// </summary>
    public class ComponentMatchTests
    {
        [Theory]
        [InlineData("daily mosque", "DAILY MOSQUE")]
        [InlineData("  SCHOOL  ", "SCHOOL")]
        [InlineData("SCHOOL   MORE  THAN 60%", "SCHOOL MORE THAN 60%")]
        [InlineData("SCHOOL MORE THAN 60 %", "SCHOOL MORE THAN 60%")]
        [InlineData("DAILY MOSQUE LESSTHAN 60%", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("daily mosque morethan 60 %", "DAILY MOSQUE MORE THAN 60%")]
        [InlineData("NH STRT 20m ROW", "NH STRT 20M ROW")]
        [InlineData("SCHOOL\tLESS THAN 60%", "SCHOOL LESS THAN 60%")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void TheKeyIsTheValueAsTyped(string value, string key)
        {
            Assert.Equal(key, ComponentMatch.Key(value));
        }

        [Theory]
        [InlineData("DAILY MOSQUE LESSTHAN 60%", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("SCHOOL MORE THAN 60 %", "school more than 60%")]
        [InlineData(" FRIDAY  MOSQUE ", "FRIDAY MOSQUE")]
        public void TheseAreOneValue(string one, string other)
        {
            Assert.True(ComponentMatch.Same(one, other));
        }

        /// <summary>
        /// The two pairs that must stay apart, kept from the tables this rule replaced. PARK is
        /// inside PARKING and NH STRT 20m ROW opens NH STRT LESS 20m ROW's way. A word glued to a
        /// word other than THAN is not pulled apart either.
        /// </summary>
        [Theory]
        [InlineData("PARK", "PARKING LOT")]
        [InlineData("NH STRT 20m ROW", "NH STRT LESS 20m ROW")]
        [InlineData("SCHOOL MORE THAN 60%", "SCHOOL LESS THAN 60%")]
        [InlineData("DAILY MOSQUE", "DAILY MOSQUE MORE THAN 60%")]
        [InlineData("DAILYMOSQUE", "DAILY MOSQUE")]
        [InlineData("", "")]
        public void TheseAreNot(string one, string other)
        {
            Assert.False(ComponentMatch.Same(one, other));
        }

        [Fact]
        public void PartOfAValueFindsNoTemplateAndNoFolder()
        {
            Assert.Null(ComponentTemplates.For("PARK"));
            Assert.Equal(string.Empty, ComponentFolders.For("PARK"));
            Assert.Equal("PARKING", ComponentTemplates.For("parking  lot").Name);
        }
    }

    /// <summary>
    /// components.csv, the one table, read off a file and layered.
    /// </summary>
    public class ComponentTableTests
    {
        /// <summary>
        /// The shipped file holds exceptions only, and on 29 Oct a rule answers every one of the
        /// fifteen it held the same way, so it is its header and nothing else. A row added later
        /// is an exception, and this test is where it is written out by hand.
        /// </summary>
        [Fact]
        public void TheShippedFileHoldsOnlyExceptionsAndTodayThatIsNone()
        {
            ComponentFile file = ComponentFile.From(ComponentSource.Shipped, InstallFile());

            Assert.True(file.Read, file.Why);
            Assert.Empty(file.Problems);
            Assert.Empty(file.Rows);
        }

        /// <summary>
        /// The copies the build puts beside RcrcGreen.Core.dll are the same files byte for byte,
        /// so the add-in and the tests read what install/ holds and nothing else.
        /// </summary>
        [Fact]
        public void TheCopiesBesideTheAssemblyAreTheInstallFiles()
        {
            Assert.True(File.Exists(ComponentTable.ShippedPath), ComponentTable.ShippedPath);
            Assert.Equal(File.ReadAllBytes(InstallFile()), File.ReadAllBytes(ComponentTable.ShippedPath));
            Assert.True(File.Exists(ComponentTable.ShippedRulesPath), ComponentTable.ShippedRulesPath);
            Assert.Equal(File.ReadAllBytes(ComponentRulesTests.InstallRulesFile()), File.ReadAllBytes(ComponentTable.ShippedRulesPath));
            Assert.True(ComponentTable.Shipped.Files.Single().Read);
            Assert.Empty(ComponentTable.Shipped.Rows);
            Assert.Equal(14, ComponentTable.Shipped.Rules.Count);
        }

        [Theory]
        [InlineData("DAILY MOSQUE MORE THAN 60%", "MOSQUES MORE THAN 60", "DAILY MOSQUE MORE THAN 60%")]
        [InlineData("DAILY MOSQUE LESSTHAN 60%", "MOSQUES LESS THAN 60", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("DAILY MOSQUE LESS THAN 60 %", "MOSQUES LESS THAN 60", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("SCHOOL MORETHAN 60%", "SCHOOLS MORE THAN 60", "SCHOOL MORE THAN 60%")]
        [InlineData("school less than 60%", "SCHOOLS LESS THAN 60", "SCHOOL LESS THAN 60%")]
        [InlineData("DAILY MOSQUE", "MOSQUES", "DAILY MOSQUE")]
        [InlineData("SCHOOL", "SCHOOLS", "SCHOOL")]
        public void EachValueFillsItsOwnTemplateAndFilesUnderTheFolderAsTheFileSpellsIt(
            string held, string template, string folder)
        {
            Assert.Equal(template, ComponentTemplates.For(held).Name);
            Assert.Equal(folder, ComponentFolders.For(held));
        }

        [Fact]
        public void TheSharedCopyWinsForAnyValueItHoldsAndTheReportSaysWhichFile()
        {
            ComponentTable table = ComponentTable.Layered(
                Shipped(),
                Parse(ComponentSource.Personal, "p.csv", "Component,Template,Folder", "PUMP STATION,PARKING,PUMPS", "SCHOOL,STREETS,MINE"),
                Parse(ComponentSource.Shared, "s.csv", "component , template , folder", "SCHOOL,SCHOOLS,SCHOOLS SHARED"));

            Assert.Equal("SCHOOLS SHARED", table.FolderFor("SCHOOL"));
            Assert.Equal(ComponentSource.Shared, table.RowFor("SCHOOL").Source);
            Assert.Equal("the shared components.csv line 2, s.csv", ComponentTable.ReadFrom(table.RowFor("SCHOOL")));

            Assert.Equal("PARKING", table.TemplateFor("PUMP STATION").Name);
            Assert.Equal("your own components.csv line 2, p.csv", ComponentTable.ReadFrom(table.RowFor("PUMP STATION")));

            Assert.Equal("MOSQUES", table.TemplateFor("DAILY MOSQUE").Name);
            Assert.Equal(ComponentSource.Shipped, table.RowFor("DAILY MOSQUE").Source);

            // One row per value, the shipped order first, then each higher file's new ones.
            Assert.Equal(16, table.Rows.Count);
            Assert.Equal("PUMP STATION", table.Rows.Last().Component);
        }

        /// <summary>
        /// A file that gives one value two answers decides neither, and a lower file does not
        /// answer in its place, because the file that should decide could not.
        /// </summary>
        [Fact]
        public void TwoAnswersInOneFileAreNeitherAndTheShippedOneDoesNotStepIn()
        {
            ComponentFile shared = Parse(ComponentSource.Shared, "s.csv",
                "Component,Template,Folder", "SCHOOL,SCHOOLS,A", "SCHOOL,SCHOOLS,B", "school ,SCHOOLS,A");

            ComponentTable table = ComponentTable.Layered(Shipped(), shared);

            Assert.Null(table.RowFor("SCHOOL"));
            Assert.Null(table.TemplateFor("SCHOOL"));
            Assert.Equal("lines 2 and 3 give SCHOOL two answers, so neither is used", shared.Problems.Single());
        }

        [Fact]
        public void TheSameAnswerTwiceIsOneRow()
        {
            ComponentFile file = Parse(ComponentSource.Shared, "s.csv",
                "Component,Template,Folder", "SCHOOL,SCHOOLS,A", "SCHOOL,schools,A");

            Assert.Single(file.Rows);
            Assert.Empty(file.Problems);
        }

        [Fact]
        public void AFileWithoutTheHeaderIsNotReadAtAll()
        {
            ComponentFile file = Parse(ComponentSource.Shared, "s.csv", "SCHOOL,SCHOOLS,SCHOOL");

            Assert.False(file.Read);
            Assert.Equal("Line 1 should be the header Component,Template,Folder and reads SCHOOL,SCHOOLS,SCHOOL.", file.Why);
            Assert.Empty(file.Rows);
        }

        [Fact]
        public void BlankLinesNotesAndAByteOrderMarkAreSkippedAndABadLineIsNamed()
        {
            ComponentFile file = Parse(ComponentSource.Shared, "s.csv",
                "﻿# the team's additions", "", "Component,Template,Folder",
                "\"HEALTH, NEW\",HEALTHCARE,\"HEALTH \"\"B\"\"\"", "ONLY TWO,FIELDS", "EMPTY,,FOLDER");

            Assert.True(file.Read);
            Assert.Equal("HEALTH, NEW", file.Rows.Single().Component);
            Assert.Equal("HEALTH \"B\"", file.Rows.Single().Folder);
            Assert.Equal(
                new[]
                {
                    "line 5 has 2 fields, not 3: ONLY TWO,FIELDS, so ONLY TWO is not used",
                    "line 6 leaves a field empty: EMPTY,,FOLDER, so EMPTY is not used"
                },
                file.Problems.ToArray());
        }

        [Fact]
        public void ARowNamingNoKnownTemplatePlacesNothingAndIsNamed()
        {
            ComponentTable table = ComponentTable.Layered(
                Parse(ComponentSource.Shared, "s.csv", "Component,Template,Folder", "PUMP STATION,PUMPS,PUMPS"));

            Assert.Null(table.TemplateFor("PUMP STATION"));
            Assert.Equal("PUMPS", table.FolderFor("PUMP STATION"));
            Assert.Contains(table.InWords(), line => line.Contains("PUMP STATION names template PUMPS, which is not a template this tool knows"));
        }

        [Fact]
        public void AMissingFileIsSaidAndOnlyTheShippedOneIsAFault()
        {
            ComponentTable table = ComponentTable.Layered(
                ComponentFile.Missing(ComponentSource.Shipped, "a.csv"),
                ComponentFile.Missing(ComponentSource.Shared, "b.csv"));

            Assert.Empty(table.Rows);
            Assert.Equal(
                new[]
                {
                    "the shipped components.csv, a.csv: not there. Every value is unknown until it is. Run the install again.",
                    "the shared components.csv, b.csv: not there."
                },
                table.InWords().ToArray());
        }

        /// <summary>
        /// A shared row the file cannot read refuses its value rather than let the shipped row
        /// answer, the same as two answers do, because the team's file said something about it.
        /// </summary>
        [Fact]
        public void ASharedRowThatCannotBeReadIsNotAnsweredByTheShippedOne()
        {
            ComponentFile shared = Parse(ComponentSource.Shared, "s.csv",
                "Component,Template,Folder", "SCHOOL,SCHOOLS", "HEALTH,,HEALTHCARE", "SCHOOL,SCHOOLS,LATER");

            ComponentTable table = ComponentTable.Layered(Shipped(), shared);

            Assert.Null(table.RowFor("SCHOOL"));
            Assert.Null(table.RowFor("HEALTH"));
            Assert.Equal("MOSQUES", table.TemplateFor("DAILY MOSQUE").Name);
        }

        /// <summary>
        /// What Excel writes: empty fields after the third once a fourth column was touched, and
        /// a semicolon between fields where that is the list separator.
        /// </summary>
        [Fact]
        public void ACsvExcelSavedIsRead()
        {
            ComponentFile commas = Parse(ComponentSource.Shared, "s.csv",
                "Component,Template,Folder,,", "PUMP STATION,PARKING,PUMPS,,", ",,,,");
            ComponentFile semicolons = Parse(ComponentSource.Shared, "s.csv",
                "Component;Template;Folder", "PUMP STATION;PARKING;\"PUMPS; NORTH\"");

            Assert.True(commas.Read, commas.Why);
            Assert.Equal("PUMPS", commas.Rows.Single().Folder);
            Assert.Empty(commas.Problems);
            Assert.True(semicolons.Read, semicolons.Why);
            Assert.Equal("PUMPS; NORTH", semicolons.Rows.Single().Folder);
        }

        [Fact]
        public void TwoFoldersDifferingOnlyInCaseAreOneAnswer()
        {
            ComponentFile file = Parse(ComponentSource.Shared, "s.csv",
                "Component,Template,Folder", "DAILY MOSQUE,MOSQUES,DAILY MOSQUE", "Daily Mosque,mosques,Daily Mosque");

            Assert.Equal("DAILY MOSQUE", file.Rows.Single().Folder);
            Assert.Empty(file.Problems);
        }

        /// <summary>
        /// The shared file open in Excel on somebody's machine is still read. A read that would
        /// not share the file would drop the team's table for as long as it stayed open.
        /// </summary>
        [Fact]
        public void AFileHeldOpenForWritingIsStillRead()
        {
            string path = Path.Combine(Path.GetTempPath(), "rcrc-components-" + Guid.NewGuid().ToString("N") + ".csv");
            File.WriteAllText(path, "Component,Template,Folder\r\nPUMP STATION,PARKING,PUMPS\r\n");

            try
            {
                using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                {
                    ComponentFile held = ComponentFile.From(ComponentSource.Shared, path);

                    Assert.True(held.Read, held.Why);
                    Assert.Equal("PUMPS", held.Rows.Single().Folder);
                }
            }
            finally
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// The fifteen rows the shipped file held before the rules, written out by hand, so the
        /// layering tests keep a lower file that answers.
        /// </summary>
        private static ComponentFile Shipped()
        {
            return Parse(ComponentSource.Shipped, "shipped.csv",
                "Component,Template,Folder",
                "DAILY MOSQUE,MOSQUES,DAILY MOSQUE",
                "FRIDAY MOSQUE,MOSQUES,FRIDAY MOSQUE",
                "SCHOOL,SCHOOLS,SCHOOL",
                "HEALTH,HEALTHCARE,HEALTHCARE",
                "PARKING LOT,PARKING,PARKING LOT",
                "EXISTING PARK,EXISTING PARKS,EXISTING PARK",
                "FUTURE PARK,FUTURE PARKS,FUTURE PARKS",
                "NH STRT LESS 20m ROW,STREETS,STREETS",
                "NH STRT 20m ROW,STREETS,STREETS",
                "STREET 30m ROW,STREETS,STREETS",
                "STREET 36m ROW,STREETS,STREETS",
                "DAILY MOSQUE MORE THAN 60%,MOSQUES MORE THAN 60,DAILY MOSQUE MORE THAN 60%",
                "DAILY MOSQUE LESS THAN 60%,MOSQUES LESS THAN 60,DAILY MOSQUE LESS THAN 60%",
                "SCHOOL MORE THAN 60%,SCHOOLS MORE THAN 60,SCHOOL MORE THAN 60%",
                "SCHOOL LESS THAN 60%,SCHOOLS LESS THAN 60,SCHOOL LESS THAN 60%");
        }

        private static ComponentFile Parse(ComponentSource source, string path, params string[] lines)
        {
            return ComponentFile.Parse(source, path, lines);
        }

        internal static string InstallFile()
        {
            for (DirectoryInfo at = new DirectoryInfo(AppContext.BaseDirectory); at != null; at = at.Parent)
            {
                string here = Path.Combine(at.FullName, "install", ComponentTable.FileName);
                if (File.Exists(here)) return here;
            }

            throw new InvalidOperationException("install/components.csv was not found above " + AppContext.BaseDirectory);
        }
    }

    /// <summary>
    /// The four 60 per cent values from the plot to the workbook row, through the prefix cross
    /// check that compares asset types.
    /// </summary>
    public class SixtyPercentTests
    {
        [Theory]
        [InlineData("DM-41", "DAILY MOSQUE MORE THAN 60%", "MOSQUES MORE THAN 60")]
        [InlineData("DM-42", "DAILY MOSQUE LESSTHAN 60%", "MOSQUES LESS THAN 60")]
        [InlineData("SC-03", "SCHOOL MORE THAN 60%", "SCHOOLS MORE THAN 60")]
        [InlineData("SC-04", "SCHOOL LESS THAN 60%", "SCHOOLS LESS THAN 60")]
        public void TheComponentPlacesItAndThePrefixAgrees(string plot, string held, string template)
        {
            PlotTemplate placed = PlotsPerTemplate.For(plot, held);

            Assert.Equal(TemplateRoute.Component, placed.Route);
            Assert.Equal(template, placed.Template.Name);
            Assert.EndsWith("and the plot prefix agrees", placed.Why);
        }

        /// <summary>
        /// A school plot carrying a mosque value is still two routes disagreeing, so the version
        /// check is on the asset type and not a pass for everything.
        /// </summary>
        [Fact]
        public void APrefixOfAnotherKindStillDisagrees()
        {
            PlotTemplate placed = PlotsPerTemplate.For("SC-03", "DAILY MOSQUE MORE THAN 60%");

            Assert.Equal(TemplateRoute.Disagree, placed.Route);
            Assert.Null(placed.Template);
            Assert.Equal("DAILY MOSQUE MORE THAN 60% means MOSQUES MORE THAN 60 and its prefix says SCHOOLS, so NEITHER placed it", placed.Why);
        }

        [Fact]
        public void ThePreselectionAgreesWithTheMosquePrefix()
        {
            TemplateChoice choice = TemplateForComponent.For(
                new AgreedValue(new[] { new PlotText("DM-41", "DAILY MOSQUE MORE THAN 60%") }), new[] { "DM-41" }, null);

            Assert.False(choice.NeedsAPick);
            Assert.Equal("MOSQUES MORE THAN 60", choice.Preselected.Name);
            Assert.EndsWith("The plot prefix agrees.", choice.Why);
        }

        /// <summary>
        /// The preselection makes the same check: a mosque version on school plots is two routes
        /// disagreeing, and the user picks.
        /// </summary>
        [Fact]
        public void ThePreselectionRefusesAVersionOfAnotherKind()
        {
            TemplateChoice choice = TemplateForComponent.For(
                new AgreedValue(new[] { new PlotText("SC-03", "DAILY MOSQUE MORE THAN 60%") }), new[] { "SC-03" }, null);

            Assert.True(choice.NeedsAPick);
            Assert.Equal(
                "The component and the plot prefix do not agree, so nothing is preselected: DAILY MOSQUE MORE THAN 60% "
                + "means MOSQUES MORE THAN 60 and the plot prefix says SCHOOLS. Pick the template.",
                choice.Why);
        }

        [Fact]
        public void APlotWithNoComponentStillGoesToTheKindItsPrefixNames()
        {
            Assert.Equal("MOSQUES", PlotsPerTemplate.For("DM-41", string.Empty).Template.Name);
        }

        [Fact]
        public void EachOfTheFourIsSplitIntoItsOwnWorkbook()
        {
            var components = new Dictionary<string, string>
            {
                { "DM-41", "DAILY MOSQUE MORE THAN 60%" },
                { "DM-42", "DAILY MOSQUE LESS THAN 60%" },
                { "DM-12", "DAILY MOSQUE" },
                { "SC-03", "SCHOOL MORE THAN 60%" },
                { "SC-04", "SCHOOL LESS THAN 60%" }
            };

            TemplateSplit split = PlotsPerTemplate.Split(components.Keys, one => components[one], KpiTemplates.All);

            Assert.True(split.AddsUp);
            Assert.Empty(split.Unplaced);
            Assert.Equal(
                new[]
                {
                    "MOSQUES DM-12", "MOSQUES MORE THAN 60 DM-41", "MOSQUES LESS THAN 60 DM-42",
                    "SCHOOLS MORE THAN 60 SC-03", "SCHOOLS LESS THAN 60 SC-04"
                },
                split.Writing.Select(one => one.Template.Name + " " + string.Join(",", one.Plots.ToArray())).ToArray());
        }

        /// <summary>
        /// The file names the client issued, and the old names, each to the right template. The
        /// file name breaks the tie because all three mosque files open on &lt;Mosques&gt;.
        /// </summary>
        [Theory]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES MORE THAN 60.xlsx", "<Mosques>", "MOSQUES MORE THAN 60")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES LESS THAN 60.xlsx", "<Mosques>", "MOSQUES LESS THAN 60")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_SCHOOLS MORE THAN 60.xlsx", "<Schools>", "SCHOOLS MORE THAN 60")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_SCHOOLS LESS THAN 60.xlsx", "<Schools>", "SCHOOLS LESS THAN 60")]
        [InlineData("schools_lessthan_60.xlsx", "<Schools>", "SCHOOLS LESS THAN 60")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_SCHOOLS MORE THAN 60%.xlsx", "<Schools>", "SCHOOLS MORE THAN 60")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx", "<Mosques>", "MOSQUES")]
        [InlineData("GRP_-_KPI_Checklist_-_DD_SCHOOLS.xlsx", "<Schools>", "SCHOOLS")]
        [InlineData("MOSQUES MORE THAN 600.xlsx", "<Mosques>", "MOSQUES")]
        [InlineData("book.xlsx", "<Mosques>", "MOSQUES")]
        public void TheFileNameSaysWhichVersion(string fileName, string mainSheet, string template)
        {
            RecognisedWorkbook found = RecognisedWorkbook.Recognise(fileName, Sheets(mainSheet), null);

            Assert.True(found.IsMatched, found.InWords);
            Assert.Equal(template, found.Template.Name);
        }

        [Fact]
        public void BothVersionsInOneNameIsAPick()
        {
            RecognisedWorkbook found = RecognisedWorkbook.Recognise("MOSQUES MORE THAN 60 LESS THAN 60.xlsx", Sheets("<Mosques>"), null);

            Assert.True(found.NeedsAPick);
            Assert.Equal("MOSQUES MORE THAN 60 or MOSQUES LESS THAN 60. Pick one, nothing is guessed.", found.InWords);
        }

        [Theory]
        [InlineData("MOSQUES MORE THAN 60", "Mosques >60%")]
        [InlineData("MOSQUES LESS THAN 60", "Mosques <60%")]
        [InlineData("SCHOOLS MORE THAN 60", "Schools >60%")]
        [InlineData("SCHOOLS LESS THAN 60", "Schools <60%")]
        [InlineData("EXISTING PARKS", "Existing parks")]
        [InlineData("HEALTHCARE", "Healthcare")]
        public void TheAssetTypeLabelIsShort(string name, string label)
        {
            Assert.Equal(label, AssetTypeNames.Display(KpiTemplates.Named(name)));
        }

        private static List<string> Sheets(string main)
        {
            return new List<string> { main, "Tree List - Existing", "Tree List - Proposed", "Criteria", "Green Strategy KPI's" };
        }
    }
}

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// The create report says, per value on the ticked plots, which components.csv placed it.
    /// </summary>
    public class ComponentTableReportTests
    {
        [Fact]
        public void TheReportNamesTheFileEachValueWasReadFrom()
        {
            var components = new System.Collections.Generic.Dictionary<string, string>
            {
                { "DM-41", "DAILY MOSQUE LESSTHAN 60%" },
                { "DM-42", "DAILY MOSQUE LESS THAN 60%" },
                { "SC-03", "SCHOOL" },
                { "PS-01", "PUMP STATION" }
            };
            var set = new KpiCreateRunSet(
                "NG05",
                PlotsPerTemplate.Split(components.Keys, one => components[one], KpiTemplates.All),
                new System.Collections.Generic.List<KpiCreateRun>(),
                new System.Collections.Generic.List<TemplateOutcome>());

            ComponentTable table = ComponentTable.Layered(
                    ComponentFile.From(ComponentSource.Shipped, ComponentTableTests.InstallFile()),
                    ComponentFile.Parse(ComponentSource.Shared, @"T:\templates\components.csv",
                        new[] { "Component,Template,Folder", "SCHOOL,SCHOOLS,SCHOOL SHARED" }))
                .WithRules(ComponentRuleFile.From(ComponentSource.Shipped, ComponentRulesTests.InstallRulesFile()));

            var report = new System.Text.StringBuilder();
            KpiCreateReport.TheComponentTable(report, set, table);
            string[] lines = report.ToString().Replace("\r", string.Empty).Split('\n');

            Assert.Equal("== WHERE EACH COMPONENT VALUE WAS READ (3) ==", lines[0]);
            Assert.Contains("  value | template | folder | main | placed by | file", lines);
            Assert.Contains(lines, line => line.StartsWith(
                "  DAILY MOSQUE LESSTHAN 60% | MOSQUES LESS THAN 60 | DAILY MOSQUE LESS THAN 60% | DAILY MOSQUE | rule MOSQUE LESS 60, the shipped component-rules.csv line 11 | ",
                StringComparison.Ordinal));
            Assert.Contains("  PUMP STATION | NONE | NONE | NONE | nothing | NONE", lines);
            Assert.Contains(@"  SCHOOL | SCHOOLS | SCHOOL SHARED | one level | row, the shared components.csv line 2 | T:\templates\components.csv", lines);
            Assert.Contains(@"  the shared components.csv, T:\templates\components.csv: 1 row", lines);
            Assert.Contains(lines, line => line.StartsWith("  the shipped component-rules.csv, ", StringComparison.Ordinal)
                && line.EndsWith(": 14 rules, the rules in force", StringComparison.Ordinal));

            // The two spellings of one value are one line, the model's first spelling shown.
            Assert.Single(lines, line => line.Contains("| MOSQUES LESS THAN 60 |"));
        }
    }
}
