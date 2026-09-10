using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class RunPlanTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType CrossSection = new ViewType("400", "Landscape Cross Section");
        private static readonly ViewType Furniture = new ViewType("600", "FURNITURE SCHEDULE");
        private static readonly ViewType Irrigation = new ViewType("600", "IRRIGATION SCHEDULE");

        private static readonly ViewType[] Schedules = { Furniture, Irrigation };

        /// <summary>
        /// A mark can go stale while the panel sits open in a shared model. The plan refuses
        /// it against the freshest read rather than letting the writer create over the
        /// existing name and leave an orphan view behind the refused rename.
        /// </summary>
        [Fact]
        public void AMarkWhoseViewNowExistsIsRefusedRatherThanAttempted()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-11", General), new PlotViewKey("DM-12", General) },
                new[] { "DM-11", "DM-12" },
                new[] { "DM-11", "DM-12" },
                null,
                null,
                null,
                null,
                new[] { new PlotViewKey("DM-11", General) });

            RunItem only = Assert.Single(plan.Items);
            Assert.Equal("DM-12", only.PlotId);

            RunRefusal refused = Assert.Single(plan.Refusals);
            Assert.Equal("DM-11", refused.PlotId);
            Assert.Equal(
                "A view with this name is already in the model, added since the panel last "
                + "read it, so nothing is made over it. Press Refresh to see it.",
                refused.Because);
        }

        /// <summary>
        /// The reason travels with the type. Every uncapturable schedule used to be answered
        /// with no plot has that schedule, which is false for a schedule that exists and
        /// filters on no plot, and sent the user looking for the wrong thing.
        /// </summary>
        [Fact]
        public void AnUncapturableScheduleIsRefusedWithItsOwnReason()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-12", Furniture) },
                new[] { "DM-12" },
                new[] { "DM-12" },
                Schedules,
                null,
                null,
                null,
                null,
                new[]
                {
                    new UncapturableSchedule(
                        Furniture,
                        "That schedule exists in this model and filters on no plot, so there "
                        + "is no plot filter to swap and it cannot be aimed at another plot. "
                        + "Add the plot filter to the source schedule and refresh.")
                });

            Assert.Empty(plan.Items);
            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.StartsWith("That schedule exists in this model and filters on no plot", only.Because);
        }

        [Fact]
        public void AScheduleWithNoReasonStillGetsTheOldWording()
        {
            RunPlan plan = RunPlan.Of(
                new[] { new PlotViewKey("DM-12", Furniture) },
                new[] { "DM-12" },
                new[] { "DM-12" },
                Schedules,
                null,
                null,
                null);

            RunRefusal only = Assert.Single(plan.Refusals);
            Assert.Equal(
                "No plot in this model has that schedule, so there is no definition to "
                + "capture and nothing to build from.",
                only.Because);
        }

        [Fact]
        public void OnlyMarkedCellsOnTickedPlotsAreMade()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-12", General),
                    new PlotViewKey("DM-13", CrossSection)
                },
                new[] { "DM-11", "DM-13" },
                new[] { "DM-11", "DM-12", "DM-13" },
                Schedules,
                new[] { Furniture });

            Assert.Equal(
                new[] { "DM-11-(200) General Arrangement Layout", "DM-13-(400) Landscape Cross Section" },
                plan.Items.Select(item => item.Name).ToArray());
        }

        /// <summary>
        /// A mark on a view type unticked in step 2 is remembered so it comes back with the
        /// column, but the run is handed the marks the grid shows and nothing else. The panel
        /// used to hand its whole memory over, so a view type taken off the grid was still
        /// made, and the MARK header counted marks nobody could see.
        /// </summary>
        [Fact]
        public void AMarkOnAHiddenColumnNeverReachesThePlan()
        {
            SheetGrid shown = SheetGrid.Build(
                new[] { "DM-11" },
                new[] { General },
                null,
                new[] { "DM-11" },
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-11", CrossSection)
                });

            RunPlan plan = RunFixture.Of(
                shown.Marked,
                new[] { "DM-11" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Equal(
                new[] { "DM-11-(200) General Arrangement Layout" },
                plan.Items.Select(item => item.Name).ToArray());
            Assert.Equal(1, shown.MarkedCount);
        }

        /// <summary>
        /// Unticking a plot is the user saying they do not want it, so a mark left on it is
        /// dropped without a refusal. Nothing has gone wrong there.
        /// </summary>
        [Fact]
        public void AMarkOnAnUntickedPlotIsDroppedWithoutARefusal()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-12", General) },
                new[] { "DM-11" },
                new[] { "DM-11", "DM-12" },
                Schedules,
                new[] { Furniture });

            Assert.Empty(plan.Items);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// A view with no scope box is useless on this project, so it is refused rather than
        /// made and the refusal names the plot.
        /// </summary>
        [Fact]
        public void APlanViewOnAPlotWithNoScopeBoxIsRefused()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-13", General) },
                new[] { "DM-13" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Empty(plan.Items);
            Assert.Single(plan.Refusals);
            Assert.Contains("No scope box is named DM-13", plan.Refusals[0].Because);
        }

        /// <summary>
        /// A schedule does not need a scope box. It filters on a parameter and shows a table.
        /// </summary>
        [Fact]
        public void AScheduleIsMadeOnAPlotWithNoScopeBox()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-13", Furniture) },
                new[] { "DM-13" },
                new string[0],
                Schedules,
                new[] { Furniture });

            Assert.Single(plan.Items);
            Assert.Equal(RunItemKind.Schedule, plan.Items[0].Kind);
            Assert.Empty(plan.Refusals);
        }

        /// <summary>
        /// A schedule type no plot in the model has cannot be captured, so it cannot be built.
        /// Loading a definition from a file is a later round. Until then this is refused by name
        /// rather than half made.
        /// </summary>
        [Fact]
        public void AScheduleTypeNoPlotHasIsRefusedByName()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", Irrigation) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Empty(plan.Items);
            Assert.Single(plan.Refusals);
            Assert.Contains("no definition to capture", plan.Refusals[0].Because);
            Assert.Equal(Irrigation, plan.Refusals[0].Type);
        }

        [Fact]
        public void PlanViewsAndSchedulesAreCountedApart()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-11", General),
                    new PlotViewKey("DM-11", CrossSection),
                    new PlotViewKey("DM-11", Furniture)
                },
                new[] { "DM-11" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Equal(2, plan.CountOf(RunItemKind.PlanView));
            Assert.Equal(1, plan.CountOf(RunItemKind.Schedule));
            Assert.Equal("This run would make 2 plan views and 1 schedule.", plan.InWords());
        }

        [Fact]
        public void OneOfEachReadsAsSingular()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", General), new PlotViewKey("DM-11", Furniture) },
                new[] { "DM-11" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Equal("This run would make 1 plan view and 1 schedule.", plan.InWords());
        }

        [Fact]
        public void RefusalsAreCountedInTheWordsTheConfirmationUses()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-11", General), new PlotViewKey("DM-13", General) },
                new[] { "DM-11", "DM-13" },
                new[] { "DM-11" },
                Schedules,
                new[] { Furniture });

            Assert.Equal(
                "This run would make 1 plan view. 1 things cannot be made and are named in the report.",
                plan.InWords());
        }

        [Fact]
        public void NothingMarkedMakesNothingAndSaysSo()
        {
            RunPlan plan = RunFixture.Of(null, new[] { "DM-11" }, new[] { "DM-11" }, Schedules, Schedules);

            Assert.True(plan.MakesNothing);
            Assert.Equal(
                "Nothing is marked on a ticked plot, so this run would make nothing.",
                plan.InWords());
        }

        [Fact]
        public void EverythingRefusedMakesNothingAndSaysHowMany()
        {
            RunPlan plan = RunFixture.Of(
                new[] { new PlotViewKey("DM-13", General), new PlotViewKey("DM-14", General) },
                new[] { "DM-13", "DM-14" },
                new string[0],
                Schedules,
                new[] { Furniture });

            Assert.True(plan.MakesNothing);
            Assert.Equal(
                "This run would make nothing. 2 things cannot be made.",
                plan.InWords());
        }

        /// <summary>
        /// Plots read in number order rather than as text, so DM-2 comes before DM-100 in the
        /// report and in the order things get made.
        /// </summary>
        [Fact]
        public void ItemsComeBackInPlotThenTypeOrder()
        {
            RunPlan plan = RunFixture.Of(
                new[]
                {
                    new PlotViewKey("DM-100", General),
                    new PlotViewKey("DM-2", CrossSection),
                    new PlotViewKey("DM-2", General)
                },
                new[] { "DM-2", "DM-100" },
                new[] { "DM-2", "DM-100" },
                Schedules,
                new[] { Furniture });

            Assert.Equal(
                new[]
                {
                    "DM-2-(200) General Arrangement Layout",
                    "DM-2-(400) Landscape Cross Section",
                    "DM-100-(200) General Arrangement Layout"
                },
                plan.Items.Select(item => item.Name).ToArray());
        }
    }
}
