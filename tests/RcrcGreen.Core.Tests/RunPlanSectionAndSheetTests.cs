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
        public void OneSheetDescribedMakesOnePerPlotWithANumber()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Sheet("GENERAL ARRANGEMENT LAYOUT", new[] { General }, 1,
                    "DM-11", "L-211", "DM-12", "L-212"));

            Assert.Equal(2, plan.CountOf(RunItemKind.Sheet));
            Assert.Equal(
                new[] { "L-211 GENERAL ARRANGEMENT LAYOUT", "L-212 GENERAL ARRANGEMENT LAYOUT" },
                plan.Items.Select(item => item.Name).ToArray());
        }

        /// <summary>
        /// The point of describing a sheet once. Two definitions over two plots make four
        /// sheets, so one press gives a plot its list of drawings and its layout together.
        /// </summary>
        [Fact]
        public void TwoSheetsDescribedMakeTwoEach()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Sheet("LIST OF DRAWINGS", new[] { KeyPlan }, 1,
                    "DM-11", "L-201", "DM-12", "L-202"),
                RunFixture.Sheet("GENERAL ARRANGEMENT LAYOUT", new[] { General }, 1,
                    "DM-11", "L-211", "DM-12", "L-212"));

            Assert.Equal(4, plan.CountOf(RunItemKind.Sheet));
            Assert.Equal(
                new[] { "L-201 LIST OF DRAWINGS", "L-202 LIST OF DRAWINGS",
                        "L-211 GENERAL ARRANGEMENT LAYOUT", "L-212 GENERAL ARRANGEMENT LAYOUT" },
                plan.Items.Select(item => item.Name).ToArray());
        }

        /// <summary>
        /// The tool invents no number, so a plot without one is refused by name and told why.
        /// </summary>
        [Fact]
        public void APlotWithNoNumberIsRefusedByName()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Sheet("LIST OF DRAWINGS", null, 1, "DM-11", "L-201", "DM-12", ""));

            Assert.Single(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("no sheet number was typed in for it", only.Because);
            Assert.Contains("invents neither", only.Because);
            Assert.Equal("DM-12", only.PlotId);
        }

        [Fact]
        public void APlotNotNamedAtAllIsRefusedTheSameWay()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                RunFixture.Sheet("LIST OF DRAWINGS", null, 1, "DM-11", "L-201"));

            Assert.Single(plan.Items);
            Assert.Single(plan.Refusals);
        }

        /// <summary>
        /// A definition short of a type or a name is one thing to go and fix, not seventeen, so
        /// it is refused once rather than once per plot.
        /// </summary>
        [Fact]
        public void AnUnfinishedSheetIsRefusedOnceRatherThanOncePerPlot()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12", "DM-13" },
                RunFixture.SheetMissingAName("DM-11", "L-201", "DM-12", "L-202", "DM-13", "L-203"));

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("missing a sheet name", only.Because);
            Assert.Contains("No sheet of this kind was made on any plot", only.Because);
        }

        [Fact]
        public void ASheetOnAnUntickedPlotIsDroppedWithoutARefusal()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Sheet("LIST OF DRAWINGS", null, 1, "DM-11", "L-201", "DM-99", "L-299"));

            Assert.Single(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// An empty sheet is a real thing to ask for. Nothing ticked still makes one.
        /// </summary>
        [Fact]
        public void ASheetWithNoViewsTickedIsStillMade()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Sheet("LIST OF DRAWINGS", null, 1, "DM-11", "L-201"));

            RunItem sheet = Assert.Single(plan.Items);
            Assert.Equal(RunItemKind.Sheet, sheet.Kind);
            Assert.Empty(sheet.Sheet.Views);
            Assert.Empty(plan.Refusals);
        }

        [Fact]
        public void TheDefinitionTravelsWithTheItemSoTheWriterKnowsWhatGoesOnIt()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                RunFixture.Sheet("GA", new[] { General, KeyPlan }, 2, "DM-11", "L-211"));

            RunItem sheet = Assert.Single(plan.Items);

            Assert.Equal(2, sheet.Sheet.ViewsPerSheet);
            Assert.Equal(2, sheet.Sheet.Placed.Count);
            Assert.Equal("GA", sheet.Sheet.SheetName);
            Assert.Null(sheet.Type);
        }

        [Fact]
        public void ASheetNeedsNoScopeBox()
        {
            RunPlan plan = RunPlan.Of(
                null, new[] { "DM-13" }, new string[0], null, null, null,
                new[] { RunFixture.Sheet("LIST OF DRAWINGS", null, 1, "DM-13", "L-203") });

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
                new[] { RunFixture.Sheet("GA", null, 1, "DM-11", "L-211", "DM-12", "L-212") });

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
                new[] { RunFixture.Sheet("GA", null, 1, "DM-11", "L-211") });

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
                new[] { RunFixture.Sheet("GA", null, 1, "DM-11", "L-211") });

            Assert.Equal(
                new[] { RunItemKind.PlanView, RunItemKind.Sheet },
                plan.Items.Select(item => item.Kind).ToArray());
        }
    }
}
