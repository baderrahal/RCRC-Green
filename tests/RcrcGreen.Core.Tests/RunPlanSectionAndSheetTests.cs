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

    public class RunPlanSheetTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");

        [Fact]
        public void APlotWithBothBoxesFilledInGetsASheet()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                new[] { new SheetRequest("DM-11", "L-201", "General Arrangement") },
                true);

            RunItem sheet = Assert.Single(plan.Items);
            Assert.Equal(RunItemKind.Sheet, sheet.Kind);
            Assert.Equal("L-201 General Arrangement", sheet.Name);
            Assert.Equal("DM-11", sheet.PlotId);
            Assert.Null(sheet.Type);
        }

        /// <summary>
        /// The tool invents neither half, so a half filled row is refused by name rather than
        /// completed with something plausible.
        /// </summary>
        [Fact]
        public void AHalfFilledRowIsRefusedAndSaysWhichHalf()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                new[] { new SheetRequest("DM-11", "L-201", string.Empty) },
                true);

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("missing a sheet name", only.Because);
            Assert.Contains("invents neither", only.Because);
        }

        [Fact]
        public void APlotWithBothBoxesEmptyIsNotARefusal()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11", "DM-12" },
                new[]
                {
                    new SheetRequest("DM-11", null, null),
                    new SheetRequest("DM-12", "L-202", "Layout")
                },
                true);

            Assert.Single(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// No source sheet means no title block and no layout. Nothing about the sheet can be
        /// guessed, so the row is refused and the message says what to do about it.
        /// </summary>
        [Fact]
        public void WithNoSourceSheetCapturedEveryRowIsRefused()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                new[] { new SheetRequest("DM-11", "L-201", "General Arrangement") },
                false);

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Contains("No source sheet was captured", only.Because);
            Assert.Equal("L-201 General Arrangement", only.Name);
        }

        [Fact]
        public void ASheetOnAnUntickedPlotIsDroppedWithoutARefusal()
        {
            RunPlan plan = RunFixture.WithSheets(
                new[] { "DM-11" },
                new[] { new SheetRequest("DM-12", "L-202", "Layout") },
                true);

            Assert.Empty(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// A sheet needs no scope box. It carries views, and whether those views could be made
        /// is a question about them.
        /// </summary>
        [Fact]
        public void ASheetIsMadeOnAPlotWithNoScopeBox()
        {
            RunPlan plan = RunPlan.Of(
                null,
                new[] { "DM-13" },
                new string[0],
                null,
                null,
                null,
                new[] { new SheetRequest("DM-13", "L-203", "Layout") },
                true);

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
                null,
                null,
                null,
                new[]
                {
                    new SheetRequest("DM-11", "L-201", "Layout"),
                    new SheetRequest("DM-12", "L-202", "Layout")
                },
                true);

            Assert.Equal(1, plan.CountOf(RunItemKind.PlanView));
            Assert.Equal(2, plan.CountOf(RunItemKind.Sheet));
            Assert.Equal("This run would make 1 plan view and 2 sheets.", plan.InWords());
        }

        /// <summary>
        /// Three kinds at once, because the joining word between the last two is the part that
        /// reads wrong if nobody writes the expected string out by hand.
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
                new[] { new SheetRequest("DM-11", "L-201", "Layout") },
                true);

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
                null,
                null,
                null,
                new[] { new SheetRequest("DM-11", "L-201", "Layout") },
                true);

            Assert.Equal(
                new[] { RunItemKind.PlanView, RunItemKind.Sheet },
                plan.Items.Select(item => item.Kind).ToArray());
        }
    }
}
