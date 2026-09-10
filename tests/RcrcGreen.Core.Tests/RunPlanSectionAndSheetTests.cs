using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Which marked cells need a section rather than a plan view, and which plots get a sheet.
    /// Both are decided here so the Revit side only has to carry them out.
    /// </summary>
    public class RunPlanSectionTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType CrossSection = new ViewType("400", "Landscape Cross Section");

        /// <summary>
        /// The fault the first real run hit. Every (400) was handed to ViewPlan.Create, which
        /// can never make a section, and the refusal blamed the level.
        /// </summary>
        [Fact]
        public void AViewTypeTheModelHoldsAsASectionIsPlannedAsASection()
        {
            RunPlan plan = RunFixture.WithSections(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-11", CrossSection)
                },
                new[] { "DM-11" },
                new[] { "DM-11" },
                new[] { CrossSection });

            Assert.Equal(1, plan.CountOf(RunItemKind.PlanView));
            Assert.Equal(1, plan.CountOf(RunItemKind.Section));

            RunItem section = plan.Items.Single(item => item.Kind == RunItemKind.Section);
            Assert.Equal("DM-11-(400) Landscape Cross Section", section.Name);
        }

        /// <summary>
        /// The code is not the rule. A model where 400 is drawn as a plan view gets a plan
        /// view, because what decides this is the kind of the view the model already holds.
        /// </summary>
        [Fact]
        public void TheCodeAloneNeverMakesSomethingASection()
        {
            RunPlan plan = RunFixture.WithSections(
                new[] { new PlotViewKey("DM-11", CrossSection) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                new ViewType[0]);

            Assert.Equal(1, plan.CountOf(RunItemKind.PlanView));
            Assert.Equal(0, plan.CountOf(RunItemKind.Section));
        }

        /// <summary>
        /// A section is cut across the middle of the plot's scope box, so a plot without one
        /// has nowhere to cut. The refusal says that rather than repeating the plan view line.
        /// </summary>
        [Fact]
        public void ASectionOnAPlotWithNoScopeBoxIsRefusedForItsOwnReason()
        {
            RunPlan plan = RunFixture.WithSections(
                new[] { new PlotViewKey("DM-13", CrossSection) },
                new[] { "DM-13" },
                new string[0],
                new[] { CrossSection });

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("nowhere to cut", only.Because);
            Assert.Equal("DM-13-(400) Landscape Cross Section", only.Name);
        }

        [Fact]
        public void SectionsAreCountedApartInTheWordsTheConfirmationUses()
        {
            RunPlan plan = RunFixture.WithSections(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-11", CrossSection),
                    new PlotViewKey("DM-12", CrossSection)
                },
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                new[] { CrossSection });

            Assert.Equal("This run would make 1 plan view and 2 sections.", plan.InWords());
        }
    }

    /// <summary>
    /// A sheet is described once and repeated across every ticked plot that has a number.
    ///
    /// It used to be copied off a sheet that already existed. The user picked one with no views
    /// on it and got an empty sheet, and copying was not how they wanted to work anyway.
    /// </summary>
    public class RunPlanSheetTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType KeyPlan = new ViewType("010", "Location Key Plan");

        [Fact]
        public void EveryCompleteRowBecomesOneSheet()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Batch(new[] { General }, 1,
                    RunFixture.Row("DM-11", "200QA", "GENERAL ARRANGEMENT LAYOUT", new[] { General }),
                    RunFixture.Row("DM-12", "200RA", "GENERAL ARRANGEMENT LAYOUT", new[] { General })));

            Assert.Equal(2, plan.CountOf(RunItemKind.Sheet));
            Assert.Equal(
                new[]
                {
                    "200QA GENERAL ARRANGEMENT LAYOUT",
                    "200RA GENERAL ARRANGEMENT LAYOUT"
                },
                plan.Items.Select(item => item.Name).ToArray());
        }

        /// <summary>
        /// The division hands one definition several rows per plot, and every one becomes its
        /// own sheet. Two views at one per sheet over two plots is four sheets and no view is
        /// ever left off.
        /// </summary>
        [Fact]
        public void OneDefinitionCanMakeSeveralSheetsPerPlot()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Batch(new[] { KeyPlan, General }, 1,
                    RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                    RunFixture.Row("DM-11", "200QA", "GENERAL ARRANGEMENT LAYOUT", new[] { General }),
                    RunFixture.Row("DM-12", "010RA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                    RunFixture.Row("DM-12", "200RA", "GENERAL ARRANGEMENT LAYOUT", new[] { General })));

            Assert.Equal(4, plan.CountOf(RunItemKind.Sheet));
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// The tool invents neither a name nor a number, so a row short of one is refused by
        /// name, with its plot and its views, and the row next to it is still made.
        /// </summary>
        [Fact]
        public void ARowShortOfANumberIsRefusedNamingItsPlotAndViews()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Batch(new[] { KeyPlan }, 1,
                    RunFixture.Row("DM-11", "010QA", "LOCATION KEY PLAN", new[] { KeyPlan }),
                    RunFixture.Row("DM-12", "", "LOCATION KEY PLAN", new[] { KeyPlan })));

            Assert.Single(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Equal("DM-12", only.PlotId);
            Assert.Equal(
                "No sheet was made for DM-12 holding (010) Location Key Plan, because it is "
                + "still missing a number. The tool invents neither.",
                only.Because);
        }

        [Fact]
        public void ARowShortOfANameIsRefusedTheSameWay()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Batch(new[] { KeyPlan, General }, 2,
                    RunFixture.Row("DM-11", "010QA", "", new[] { KeyPlan, General }, 2)));

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("still missing a name", only.Because);
            Assert.Contains("(010) Location Key Plan, (200) General Arrangement Layout", only.Because);
        }

        /// <summary>
        /// A definition short of its title block is one thing to go and fix, not one per row,
        /// so it is refused once.
        /// </summary>
        [Fact]
        public void AnUnfinishedDefinitionIsRefusedOnceRatherThanOncePerRow()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12", "DM-13" },
                RunFixture.BatchMissingItsType(
                    RunFixture.Row("DM-11", "010QA", "N", new[] { KeyPlan }),
                    RunFixture.Row("DM-12", "010RA", "N", new[] { KeyPlan }),
                    RunFixture.Row("DM-13", "010SA", "N", new[] { KeyPlan })));

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("missing a title block", only.Because);
            Assert.Contains("No sheet of this kind was made on any plot", only.Because);
        }

        [Fact]
        public void ARowOnAnUntickedPlotIsDroppedWithoutARefusal()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Batch(new[] { KeyPlan }, 1,
                    RunFixture.Row("DM-11", "010QA", "N", new[] { KeyPlan }),
                    RunFixture.Row("DM-99", "010ZA", "N", new[] { KeyPlan })));

            Assert.Single(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// A definition with no views ticked makes no rows and so no sheets. It used to make
        /// an empty sheet on purpose, and the divided sheets superseded that.
        /// </summary>
        [Fact]
        public void ADefinitionWithNoRowsMakesNothingAndRefusesNothing()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Batch(null, 1));

            Assert.Empty(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        [Fact]
        public void TheRowTravelsWithTheItemSoTheWriterKnowsWhatGoesOnIt()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Batch(new[] { General, KeyPlan }, 2,
                    RunFixture.Row("DM-11", "200QA", "GA", new[] { General, KeyPlan }, 2)));

            RunItem sheet = Assert.Single(plan.Items);

            Assert.Equal(2, sheet.Sheet.ViewsPerSheet);
            Assert.Equal(
                "(200) General Arrangement Layout, (010) Location Key Plan",
                sheet.Sheet.ViewsInWords());
            Assert.Equal("GA", sheet.Sheet.SheetName);
            Assert.Null(sheet.Type);
        }

        [Fact]
        public void ASheetNeedsNoScopeBox()
        {
            RunPlan plan = RunPlan.Of(
                null, new[] { "DM-13" }, new string[0], null, null, null,
                new[]
                {
                    RunFixture.Batch(new[] { KeyPlan }, 1,
                        RunFixture.Row("DM-13", "010SA", "N", new[] { KeyPlan }))
                });

            Assert.Single(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        [Fact]
        public void SheetsAreCountedApartFromViews()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-11", General) },
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                null, null, null,
                new[]
                {
                    RunFixture.Batch(new[] { General }, 1,
                        RunFixture.Row("DM-11", "200QA", "GA", new[] { General }),
                        RunFixture.Row("DM-12", "200RA", "GA", new[] { General }))
                });

            Assert.Equal(1, plan.CountOf(RunItemKind.PlanView));
            Assert.Equal(2, plan.CountOf(RunItemKind.Sheet));
            Assert.Equal("This run would make 1 plan view and 2 sheets.", plan.InWords());
        }

        /// <summary>
        /// Three kinds at once, because the joining word between the last two reads wrong if
        /// nobody writes the expected string out by hand.
        /// </summary>
        [Fact]
        public void ThreeKindsReadAsAList()
        {
            RunPlan plan = RunPlan.Of(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-11", new ViewType("400", "Landscape Cross Section"))
                },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null,
                new[] { new ViewType("400", "Landscape Cross Section") },
                null,
                new[]
                {
                    RunFixture.Batch(new[] { General }, 1,
                        RunFixture.Row("DM-11", "200QA", "GA", new[] { General }))
                });

            Assert.Equal(
                "This run would make 1 plan view, 1 section and 1 sheet.",
                plan.InWords());
        }

        [Fact]
        public void SheetsComeAfterTheViewsSoTheyCanBePlacedOnThem()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-11", General) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                null, null, null,
                new[]
                {
                    RunFixture.Batch(new[] { General }, 1,
                        RunFixture.Row("DM-11", "200QA", "GA", new[] { General }))
                });

            Assert.Equal(
                new[] { RunItemKind.PlanView, RunItemKind.Sheet },
                plan.Items.Select(item => item.Kind).ToArray());
        }
    }
}
