using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// component-rules.csv, Bader's rules of 29 Oct 2026, and the order a value is placed in:
    /// its exact row, then the first rule whose words it holds, then nothing. Every expected
    /// answer is written out by hand.
    /// </summary>
    public class ComponentRulesTests
    {
        /// <summary>The shipped file, read through the add-in's reader, against the fourteen by hand.</summary>
        [Fact]
        public void TheShippedFileHoldsBadersFourteenInHisOrder()
        {
            ComponentRuleFile file = ComponentRuleFile.From(ComponentSource.Shipped, InstallRulesFile());

            Assert.True(file.Read, file.Why);
            Assert.Equal(
                new[]
                {
                    "MOSQUE MORE 60|MOSQUES MORE THAN 60|value|base",
                    "MOSQUE LESS 60|MOSQUES LESS THAN 60|value|base",
                    "MOSQUE|MOSQUES|value|base",
                    "SCHOOL MORE 60|SCHOOLS MORE THAN 60|value|base",
                    "SCHOOL LESS 60|SCHOOLS LESS THAN 60|value|base",
                    "SCHOOL|SCHOOLS|value|base",
                    "HEALTH|HEALTHCARE|HEALTHCARE|",
                    "PARKING|PARKING|PARKING LOT|",
                    "EXISTING PARK|EXISTING PARKS|EXISTING PARK|",
                    "FUTURE PARK|FUTURE PARKS|FUTURE PARKS|",
                    "STREET|STREETS|value|STREETS",
                    "STRT|STREETS|value|STREETS",
                    "WALKWAY|STREETS|value|STREETS",
                    "ROW|STREETS|value|STREETS"
                },
                file.Rules.Select(one => one.Words + "|" + one.Template + "|" + one.Folder + "|" + one.Main).ToArray());

            Assert.Empty(ComponentTable.Shipped.UnmatchedRules);
        }

        /// <summary>
        /// **The 22 values**: the fifteen components.csv held, which must reach the template their
        /// rows gave and, since K78, the street values their own tidied folder rather than
        /// STREETS, and the seven new ones, which must reach what Bader listed.
        /// Every one is placed by a rule, because the shipped file holds no row.
        /// </summary>
        [Theory]
        [InlineData("DAILY MOSQUE", "MOSQUES", "DAILY MOSQUE", "MOSQUE")]
        [InlineData("FRIDAY MOSQUE", "MOSQUES", "FRIDAY MOSQUE", "MOSQUE")]
        [InlineData("SCHOOL", "SCHOOLS", "SCHOOL", "SCHOOL")]
        [InlineData("HEALTH", "HEALTHCARE", "HEALTHCARE", "HEALTH")]
        [InlineData("PARKING LOT", "PARKING", "PARKING LOT", "PARKING")]
        [InlineData("EXISTING PARK", "EXISTING PARKS", "EXISTING PARK", "EXISTING PARK")]
        [InlineData("FUTURE PARK", "FUTURE PARKS", "FUTURE PARKS", "FUTURE PARK")]
        [InlineData("NH STRT LESS 20m ROW", "STREETS", "NH STREET LESS THAN 20m ROW", "STRT")]
        [InlineData("NH STRT 20m ROW", "STREETS", "NH STREET 20m ROW", "STRT")]
        [InlineData("STREET 30m ROW", "STREETS", "STREET 30m ROW", "STREET")]
        [InlineData("STREET 36m ROW", "STREETS", "STREET 36m ROW", "STREET")]
        [InlineData("DAILY MOSQUE MORE THAN 60%", "MOSQUES MORE THAN 60", "DAILY MOSQUE MORE THAN 60%", "MOSQUE MORE 60")]
        [InlineData("DAILY MOSQUE LESS THAN 60%", "MOSQUES LESS THAN 60", "DAILY MOSQUE LESS THAN 60%", "MOSQUE LESS 60")]
        [InlineData("SCHOOL MORE THAN 60%", "SCHOOLS MORE THAN 60", "SCHOOL MORE THAN 60%", "SCHOOL MORE 60")]
        [InlineData("SCHOOL LESS THAN 60%", "SCHOOLS LESS THAN 60", "SCHOOL LESS THAN 60%", "SCHOOL LESS 60")]
        [InlineData("FRIDAY MOSQUE MORE THAN 60%", "MOSQUES MORE THAN 60", "FRIDAY MOSQUE MORE THAN 60%", "MOSQUE MORE 60")]
        [InlineData("FRIDAY MOSQUE LESS THAN 60%", "MOSQUES LESS THAN 60", "FRIDAY MOSQUE LESS THAN 60%", "MOSQUE LESS 60")]
        [InlineData("NH STREET 20m ROW", "STREETS", "NH STREET 20m ROW", "STREET")]
        [InlineData("NH STREET LESS THAN 20m ROW", "STREETS", "NH STREET LESS THAN 20m ROW", "STREET")]
        [InlineData("NH STREET 25m ROW", "STREETS", "NH STREET 25m ROW", "STREET")]
        [InlineData("NH STREET 30m ROW", "STREETS", "NH STREET 30m ROW", "STREET")]
        [InlineData("WALKWAY", "STREETS", "WALKWAY", "WALKWAY")]
        public void EachOfTheTwentyTwoReachesItsTemplateAndFolderByItsRule(string value, string template, string folder, string rule)
        {
            ComponentPlacement placed = ComponentTable.Shipped.PlaceOf(value);

            Assert.Null(placed.Row);
            Assert.Equal(rule, placed.Rule.Words);
            Assert.Equal(template, placed.Template.Name);
            Assert.Equal(folder, placed.Folder);

            // The static lookups every caller uses read the same answer.
            Assert.Equal(template, ComponentTemplates.For(value).Name);
            Assert.Equal(folder, ComponentFolders.For(value));
        }

        /// <summary>
        /// The traps the whole value rule kept shut, each still shut under the word rules.
        /// </summary>
        [Fact]
        public void ParkNeverMatchesParkingLotAndParkingLotFallsToNoPark()
        {
            Assert.Null(ComponentTable.Shipped.PlaceOf("PARK").Rule);
            Assert.Equal("PARKING", ComponentTable.Shipped.PlaceOf("PARKING LOT").Template.Name);
            Assert.Equal("PARKING", ComponentTable.Shipped.PlaceOf("EXISTING PARKING LOT").Template.Name);
            Assert.Equal("PARKING", ComponentTable.Shipped.PlaceOf("FUTURE PARKING").Template.Name);
        }

        /// <summary>
        /// Two street values are two values and, since 29 October, two folders, each the value
        /// tidied, both grouped under STREETS.
        /// </summary>
        [Fact]
        public void TheTwoStreetValuesStayTwoValuesAndBothReachStreets()
        {
            Assert.False(ComponentMatch.Same("NH STRT 20m ROW", "NH STRT LESS 20m ROW"));

            IReadOnlyList<KeyValuePair<string, ComponentPlacement>> placed = UnknownComponents.PlacedByRule(
                new[] { "NS-01", "NS-02" },
                plot => plot == "NS-01" ? "NH STRT 20m ROW" : "NH STRT LESS 20m ROW",
                ComponentTable.Shipped);

            Assert.Equal(new[] { "NH STRT 20m ROW", "NH STRT LESS 20m ROW" }, placed.Select(one => one.Key).ToArray());
            Assert.All(placed, one => Assert.Equal("STREETS", one.Value.Template.Name));
            Assert.Equal(new[] { "NH STREET 20m ROW", "NH STREET LESS THAN 20m ROW" }, placed.Select(one => one.Value.Folder).ToArray());
            Assert.All(placed, one => Assert.Equal("STREETS", one.Value.Main));
        }

        [Theory]
        [InlineData("FRIDAY MOSQUE MORE THAN HALF")]
        [InlineData("DAILY MOSQUE MORE THAN 600%")]
        [InlineData("DAILY MOSQUE MORE")]
        public void AMosqueWithMoreButNo60ReachesMosques(string value)
        {
            ComponentPlacement placed = ComponentTable.Shipped.PlaceOf(value);

            Assert.Equal("MOSQUE", placed.Rule.Words);
            Assert.Equal("MOSQUES", placed.Template.Name);
        }

        [Theory]
        [InlineData("GOVERMENT BUILDING")]
        [InlineData("PUMP STATION")]
        [InlineData("HEALTHCARE CENTRE")]
        [InlineData("SCHOOLS")]
        public void AValueWithNoRuleWordReachesNothing(string value)
        {
            ComponentPlacement placed = ComponentTable.Shipped.PlaceOf(value);

            Assert.False(placed.Placed);
            Assert.Null(placed.Template);
            Assert.Equal(string.Empty, placed.Folder);
            Assert.Equal("nothing", placed.How);
            Assert.False(ComponentTable.Shipped.Names(value));
        }

        /// <summary>
        /// The spacing rule runs first, so a value typed LESSTHAN or with a space before the per
        /// cent sign holds LESS and 60, and files in the one folder of that value.
        /// </summary>
        [Theory]
        [InlineData("DAILY MOSQUE LESSTHAN 60%", "MOSQUES LESS THAN 60", "DAILY MOSQUE LESS THAN 60%")]
        [InlineData("daily mosque more than 60 %", "MOSQUES MORE THAN 60", "DAILY MOSQUE MORE THAN 60%")]
        [InlineData("  FRIDAY   MOSQUE ", "MOSQUES", "FRIDAY MOSQUE")]
        public void TheSpacingRuleRunsFirstAndTheFolderIsOneFolderPerValue(string value, string template, string folder)
        {
            ComponentPlacement placed = ComponentTable.Shipped.PlaceOf(value);

            Assert.Equal(template, placed.Template.Name);
            Assert.Equal(folder, placed.Folder);
        }

        /// <summary>An exact row beats every rule, for the exceptions.</summary>
        [Fact]
        public void AnExactRowBeatsEveryRule()
        {
            ComponentTable table = ComponentTable.Layered(
                    ComponentFile.Parse(ComponentSource.Shared, "s.csv", new[]
                    {
                        "Component,Template,Folder",
                        "WALKWAY,PARKING,WALKS"
                    }))
                .WithRules(Rules(ComponentSource.Shipped, "r.csv", "Words,Template,Folder,Main", "WALKWAY,STREETS,STREETS"));

            ComponentPlacement placed = table.PlaceOf("walkway");

            Assert.Null(placed.Rule);
            Assert.Equal("PARKING", placed.Template.Name);
            Assert.Equal("WALKS", placed.Folder);
            Assert.Equal("row, the shared components.csv line 2", placed.How);
            Assert.Equal("s.csv", placed.FromPath);
        }

        /// <summary>
        /// A value a components.csv refused, two answers in one file, is not answered by a rule
        /// either, because the file that should decide could not.
        /// </summary>
        [Fact]
        public void AValueAComponentsFileRefusedIsNotAnsweredByARule()
        {
            ComponentTable table = ComponentTable.Layered(
                    ComponentFile.Parse(ComponentSource.Shared, "s.csv", new[]
                    {
                        "Component,Template,Folder",
                        "WALKWAY,PARKING,A",
                        "WALKWAY,STREETS,B"
                    }))
                .WithRules(Rules(ComponentSource.Shipped, "r.csv", "Words,Template,Folder,Main", "WALKWAY,STREETS,STREETS"));

            ComponentPlacement placed = table.PlaceOf("WALKWAY");

            Assert.False(placed.Placed);
            Assert.True(placed.Refused);
            Assert.True(table.Names("WALKWAY"));
        }

        /// <summary>The first rule whose words are all there wins, never the longest.</summary>
        [Fact]
        public void TheFirstRuleWinsInTheFilesOrder()
        {
            ComponentTable table = ComponentTable.Layered().WithRules(Rules(ComponentSource.Shipped, "r.csv",
                "Words,Template,Folder,Main", "ROW,STREETS,STREETS", "MOSQUE,MOSQUES,value"));

            Assert.Equal("ROW", table.PlaceOf("MOSQUE ROW").Rule.Words);
            Assert.Equal("STREETS", table.PlaceOf("MOSQUE ROW").Template.Name);
        }

        /// <summary>
        /// The shared copy read whole replaces the shipped list. One that is not read lets the
        /// shipped list answer and the report says so.
        /// </summary>
        [Fact]
        public void TheSharedRulesReplaceTheShippedOnesAndOneNotReadIsSaid()
        {
            ComponentRuleFile shipped = Rules(ComponentSource.Shipped, "shipped.csv", "Words,Template,Folder,Main", "WALKWAY,STREETS,STREETS");
            ComponentRuleFile shared = Rules(ComponentSource.Shared, "shared.csv", "Words,Template,Folder,Main", "PUMP,PARKING,PUMPS");
            ComponentRuleFile broken = Rules(ComponentSource.Shared, "shared.csv", "Words,Template,Folder,Main", "PUMP,PARKING");

            ComponentTable replaced = ComponentTable.Layered().WithRules(shipped, shared);
            Assert.False(replaced.PlaceOf("WALKWAY").Placed);
            Assert.Equal("PUMPS", replaced.PlaceOf("PUMP STATION").Folder);
            Assert.Equal("rule PUMP, the shared component-rules.csv line 2", replaced.PlaceOf("PUMP STATION").How);
            Assert.Contains("the shipped component-rules.csv, shipped.csv: 1 rule, not used, the shared rules are in force", replaced.InWords());

            ComponentTable fellBack = ComponentTable.Layered().WithRules(shipped, broken);
            Assert.Equal("STREETS", fellBack.PlaceOf("WALKWAY").Template.Name);
            Assert.False(fellBack.PlaceOf("PUMP STATION").Placed);
            Assert.Contains(fellBack.InWords(), line => line.StartsWith(
                "the shared component-rules.csv, shared.csv: NOT READ. Line 2 has 2 fields, not 4", StringComparison.Ordinal));
            Assert.Equal("Shared component-rules.csv was not read", KpiPaneWords.RulesFileSays(broken));
        }

        /// <summary>
        /// **One unreadable line refuses the whole file**, because a dropped MOSQUE MORE 60 line
        /// would send an over 60 mosque to MOSQUES with nothing said.
        /// </summary>
        [Theory]
        [InlineData("MOSQUE MORE 60,MOSQUES MORE THAN 60")]
        [InlineData("MOSQUE MORE 60,,value")]
        [InlineData("%,MOSQUES MORE THAN 60,value")]
        [InlineData("MOSQUE,MORE,60,value,base")]
        public void OneUnreadableLineRefusesTheWholeFile(string bad)
        {
            ComponentRuleFile file = Rules(ComponentSource.Shared, "s.csv", "Words,Template,Folder,Main", bad, "MOSQUE,MOSQUES,value");

            Assert.False(file.Read);
            Assert.Empty(file.Rules);
            Assert.StartsWith("Line 2 ", file.Why);
        }

        [Fact]
        public void AFileWithoutTheHeaderIsNotReadAndExcelsSemicolonsAre()
        {
            Assert.False(Rules(ComponentSource.Shared, "s.csv", "MOSQUE,MOSQUES,value").Read);
            Assert.False(Rules(ComponentSource.Shared, "s.csv", "Component,Template,Folder", "MOSQUE,MOSQUES,value").Read);

            // The three column header of 29 October says nothing about Main, so it is refused
            // whole and the shipped rules answer, rather than every value filing one level.
            ComponentRuleFile old = Rules(ComponentSource.Shared, "s.csv", "Words,Template,Folder", "MOSQUE,MOSQUES,value");
            Assert.False(old.Read);
            Assert.Equal("Line 1 should be the header Words,Template,Folder,Main and reads Words,Template,Folder.", old.Why);

            ComponentRuleFile semicolons = Rules(ComponentSource.Shared, "s.csv", "Words;Template;Folder;Main", "PUMP;PARKING;\"PUMPS; NORTH\";", ";;;");
            Assert.True(semicolons.Read, semicolons.Why);
            Assert.Equal("PUMPS; NORTH", semicolons.Rules.Single().Folder);
        }

        /// <summary>
        /// A rule naming a template this machine does not have places the value nowhere and is
        /// named, never moved on to the next rule.
        /// </summary>
        [Fact]
        public void ARuleNamingAnUnknownTemplatePlacesNothingAndIsNamed()
        {
            ComponentTable table = ComponentTable.Layered().WithRules(Rules(ComponentSource.Shared, "s.csv",
                "Words,Template,Folder,Main", "PUMP,PUMP STATIONS,PUMPS", "STATION,PARKING,PARKING LOT"));

            ComponentPlacement placed = table.PlaceOf("PUMP STATION");

            Assert.True(placed.Placed);
            Assert.Null(placed.Template);
            Assert.Equal("PUMP STATIONS", placed.TemplateNamed);
            Assert.Single(table.UnmatchedRules);
            Assert.Contains(table.InWords(), line => line.StartsWith("  rule PUMP names template PUMP STATIONS,", StringComparison.Ordinal));
        }

        /// <summary>
        /// A plot with no component goes to its template's one folder where there is one. A rule
        /// filing under each value reaches a folder per value, so MOSQUES and SCHOOLS have none.
        /// </summary>
        [Theory]
        [InlineData("EXISTING PARKS", "EXISTING PARK")]
        [InlineData("FUTURE PARKS", "FUTURE PARKS")]
        [InlineData("HEALTHCARE", "HEALTHCARE")]
        [InlineData("PARKING", "PARKING LOT")]
        [InlineData("STREETS", "")]
        [InlineData("MOSQUES", "")]
        [InlineData("SCHOOLS", "")]
        public void APlotWithNoComponentFilesUnderItsTemplatesOneFolderWhereThereIsOne(string template, string folder)
        {
            Assert.Equal(folder, ComponentFolders.OnlyFolderFor(KpiTemplates.Named(template)));
        }

        [Fact]
        public void TheKnownFoldersAreTheRulesNamedOnes()
        {
            Assert.Equal(
                new[] { "HEALTHCARE", "PARKING LOT", "EXISTING PARK", "FUTURE PARKS" },
                ComponentFolders.Folders.ToArray());
        }

        /// <summary>Step 2's line and its tooltip, each value once, in the plots' order.</summary>
        [Fact]
        public void StepTwoSaysHowManyValuesARulePlacedAndTheTooltipNamesEach()
        {
            var values = new Dictionary<string, string>
            {
                { "FM-01", "FRIDAY MOSQUE MORE THAN 60%" },
                { "FM-02", "friday mosque more than 60 %" },
                { "NS-01", "WALKWAY" },
                { "GB-01", "GOVERMENT BUILDING" },
                { "XX-01", "" }
            };

            IReadOnlyList<KeyValuePair<string, ComponentPlacement>> placed =
                UnknownComponents.PlacedByRule(values.Keys, plot => values[plot], ComponentTable.Shipped);

            Assert.Equal("2 values placed by rule", KpiPaneWords.PlacedByRule(placed.Count));
            Assert.Equal("1 value placed by rule", KpiPaneWords.PlacedByRule(1));
            Assert.True(KpiPaneWords.PlacedByRule(placed.Count).Split(' ').Length <= 10);
            Assert.Equal(
                "FRIDAY MOSQUE MORE THAN 60%: rule MOSQUE MORE 60, MOSQUES MORE THAN 60, folder FRIDAY MOSQUE MORE THAN 60%\n"
                + "WALKWAY: rule WALKWAY, STREETS, folder WALKWAY\n\n"
                + "An exact row in components.csv beats every rule.",
                KpiPaneWords.PlacedByRuleSays(placed));

            // The unknown value box stays for what no row and no rule places.
            Assert.Equal(
                new[] { "GOVERMENT BUILDING" },
                UnknownComponents.In(values.Keys, plot => values[plot], ComponentTable.Shipped, null).Select(one => one.Value).ToArray());
        }

        internal static ComponentRuleFile Rules(ComponentSource source, string path, params string[] lines)
        {
            return ComponentRuleFile.Parse(source, path, lines);
        }

        internal static string InstallRulesFile()
        {
            for (DirectoryInfo at = new DirectoryInfo(AppContext.BaseDirectory); at != null; at = at.Parent)
            {
                string here = Path.Combine(at.FullName, "install", ComponentRules.FileName);
                if (File.Exists(here)) return here;
            }

            throw new InvalidOperationException("install/component-rules.csv was not found above " + AppContext.BaseDirectory);
        }
    }

    /// <summary>
    /// **WALKWAY and the street values: the gaps are named and the workbook is still written.**
    /// The road width and the length come from the street reference file by UID2 and the PDF
    /// form from the plot prefix. Nothing here changed in the round that added the rules, and
    /// these hold that the code already did what Bader asked.
    /// </summary>
    public class StreetGapsTests
    {
        [Fact]
        public void AWalkwayPlotMissingFromTheStreetFileStillGetsItsWorkbookWithTheGapNamed()
        {
            KpiTemplate template = ComponentTable.Shipped.PlaceOf("WALKWAY").Template;
            StreetReferenceFile file = StreetReferenceFile.Holding("scope.xlsx", "Sheet1", 1,
                new[] { new StreetReferenceRow("ANH-007-ST-100210", 2, "330.658499", "m", "20") });
            StreetReferenceAnswer street = file.For("ANH-007-WW-000001");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                template,
                new AgreedValue(new[] { new PlotText("WW-01", "WALKWAY") }),
                new AgreedValue(new[] { new PlotText("WW-01", "ANH-007-WW-000001") }),
                "KING FAHD",
                null,
                Totalled.Adding(new[] { new PlotNumber("WW-01", 361.0) }),
                Totalled.Adding(new[] { new PlotNumber("WW-01", 96.0) }),
                null,
                "2026-10-29", "B RAHAL", "BIM COORDINATOR",
                street, CreateFixture.NoLabels);

            Assert.Equal("STREETS", template.Name);
            Assert.Equal("WALKWAY", plan.Writes.Single(one => one.Cell.ToString() == "D3").Stored);
            Assert.Contains(plan.Writes, one => one.Cell.ToString() == "F11");

            Assert.Equal(
                new[]
                {
                    "D8|ANH-007-WW-000001 is not in the street reference file, so the road width and the total length are left empty",
                    "F8|ANH-007-WW-000001 is not in the street reference file, so the road width and the total length are left empty"
                },
                plan.Skipped.Where(one => one.What.StartsWith("Streets", StringComparison.Ordinal))
                    .Select(one => one.Cell + "|" + one.Why).ToArray());
        }

        [Fact]
        public void APrefixWithNoFormWritesNoPdfAndSaysSoInOneLine()
        {
            Assert.Same(PdfForms.Roads, PdfForms.ForPlot("NS-01"));
            Assert.Same(PdfForms.Roads, PdfForms.ForPlot("ST-01"));
            Assert.Same(PdfForms.Roads, PdfForms.ForPlot("MM-01"));

            Assert.Null(PdfForms.ForPlot("WW-01"));
            Assert.Equal("no form is named for the plot prefix WW, so no PDF was written", PdfForms.NoFormFor("WW-01"));
        }
    }
}
