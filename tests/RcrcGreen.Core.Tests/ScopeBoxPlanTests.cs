using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    public class ScopeBoxPlanTests
    {
        private static ViewScopeBoxState View(
            string name, bool canHold = true, string current = "", long id = 1)
        {
            return new ViewScopeBoxState(id, name, canHold, current);
        }

        private static ScopeBoxCase CaseFor(ViewScopeBoxState view, params string[] boxes)
        {
            return ScopeBoxPlan.Decide(new[] { view }, boxes).Decisions.Single().Outcome;
        }

        [Fact]
        public void AViewNameThatDoesNotParseIsCaseA()
        {
            Assert.Equal(
                ScopeBoxCase.NameDoesNotParse,
                CaseFor(View("SOFTSCAPE SCHEDULES"), "DM-41"));
            Assert.Equal(
                ScopeBoxCase.NameDoesNotParse,
                CaseFor(View("NG05"), "DM-41"));
        }

        [Fact]
        public void AViewThatCannotHoldAScopeBoxIsCaseB()
        {
            Assert.Equal(
                ScopeBoxCase.CannotHoldAScopeBox,
                CaseFor(View("DM-41-(010) Location Key Plan", canHold: false), "DM-41"));
        }

        [Fact]
        public void AViewWithNoScopeBoxAndAMatchingOneAvailableIsCaseC()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[] { View("DM-41-(010) Location Key Plan") },
                new[] { "PF-12", "DM-41" });

            ViewScopeBoxDecision decision = plan.Decisions.Single();

            Assert.Equal(ScopeBoxCase.ReadyToAssign, decision.Outcome);
            Assert.Equal("DM-41", decision.PlotId);
            Assert.Equal("DM-41", decision.ScopeBoxToAssign);
            Assert.Equal(new[] { decision }, plan.ToAssign);
        }

        [Fact]
        public void AViewWithNoScopeBoxAndNoneMatchingIsCaseD()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[] { View("DM-41-(010) Location Key Plan") },
                new[] { "PF-12" });

            ViewScopeBoxDecision decision = plan.Decisions.Single();

            Assert.Equal(ScopeBoxCase.NoMatchingScopeBox, decision.Outcome);
            Assert.Equal("DM-41", decision.PlotId);
            Assert.Empty(decision.ScopeBoxToAssign);
            Assert.Empty(plan.ToAssign);
        }

        [Fact]
        public void AViewAlreadyCarryingTheRightScopeBoxIsCaseE()
        {
            Assert.Equal(
                ScopeBoxCase.AlreadyRight,
                CaseFor(View("DM-41-(010) Location Key Plan", current: "DM-41"), "DM-41"));
        }

        [Fact]
        public void AViewCarryingADifferentScopeBoxIsCaseF()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[] { View("DM-41-(010) Location Key Plan", current: "PF-12") },
                new[] { "DM-41", "PF-12" });

            ViewScopeBoxDecision decision = plan.Decisions.Single();

            Assert.Equal(ScopeBoxCase.HoldsADifferentScopeBox, decision.Outcome);
            Assert.Equal("PF-12", decision.CurrentScopeBoxName);
            Assert.Equal("DM-41", decision.PlotId);
            Assert.Empty(decision.ScopeBoxToAssign);
        }

        [Fact]
        public void CaseCIsTheOnlyOneThatEverCarriesAScopeBoxToAssign()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[]
                {
                    View("SOFTSCAPE SCHEDULES", id: 1),
                    View("DM-41-(010) Location Key Plan", canHold: false, id: 2),
                    View("DM-41-(200) General Arrangement Layout", id: 3),
                    View("PF-99-(200) General Arrangement Layout", id: 4),
                    View("PF-12-(010) Location Key Plan", current: "PF-12", id: 5),
                    View("PF-12-(400) Landscape Cross Section", current: "DM-41", id: 6)
                },
                new[] { "DM-41", "PF-12" });

            Assert.All(
                plan.Decisions.Where(decision => decision.Outcome != ScopeBoxCase.ReadyToAssign),
                decision => Assert.Empty(decision.ScopeBoxToAssign));

            ViewScopeBoxDecision only = Assert.Single(plan.ToAssign);
            Assert.Equal(3, only.ViewId);
        }

        [Fact]
        public void EveryCaseIsCountedAndTheCountsAddUp()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[]
                {
                    View("SOFTSCAPE SCHEDULES", id: 1),
                    View("600QD", id: 2),
                    View("DM-41-(010) Location Key Plan", canHold: false, id: 3),
                    View("DM-41-(200) General Arrangement Layout", id: 4),
                    View("PF-99-(200) General Arrangement Layout", id: 5),
                    View("PF-12-(010) Location Key Plan", current: "PF-12", id: 6),
                    View("PF-12-(400) Landscape Cross Section", current: "DM-41", id: 7)
                },
                new[] { "DM-41", "PF-12" });

            Assert.Equal(2, plan.Count(ScopeBoxCase.NameDoesNotParse));
            Assert.Equal(1, plan.Count(ScopeBoxCase.CannotHoldAScopeBox));
            Assert.Equal(1, plan.Count(ScopeBoxCase.ReadyToAssign));
            Assert.Equal(1, plan.Count(ScopeBoxCase.NoMatchingScopeBox));
            Assert.Equal(1, plan.Count(ScopeBoxCase.AlreadyRight));
            Assert.Equal(1, plan.Count(ScopeBoxCase.HoldsADifferentScopeBox));
            Assert.Equal(7, plan.Decisions.Count);
        }

        [Fact]
        public void AScopeBoxNameIsMatchedExactlyAndCaseMatters()
        {
            Assert.Equal(
                ScopeBoxCase.NoMatchingScopeBox,
                CaseFor(View("DM-41-(010) Location Key Plan"), "dm-41"));
            Assert.Equal(
                ScopeBoxCase.NoMatchingScopeBox,
                CaseFor(View("DM-41-(010) Location Key Plan"), "DM-41 working"));
            Assert.Equal(
                ScopeBoxCase.NoMatchingScopeBox,
                CaseFor(View("DM-41-(010) Location Key Plan"), "DM-410"));
            Assert.Equal(
                ScopeBoxCase.ReadyToAssign,
                CaseFor(View("DM-41-(010) Location Key Plan"), "DM-41"));
        }

        [Fact]
        public void AScopeBoxThatDiffersOnlyInCaseFromTheOneHeldIsStillCaseF()
        {
            Assert.Equal(
                ScopeBoxCase.HoldsADifferentScopeBox,
                CaseFor(View("DM-41-(010) Location Key Plan", current: "dm-41"), "DM-41"));
        }

        [Fact]
        public void ANameThatDoesNotParseIsReadBeforeTheParameterIsAskedAbout()
        {
            Assert.Equal(
                ScopeBoxCase.NameDoesNotParse,
                CaseFor(View("SOFTSCAPE SCHEDULES", canHold: false), "DM-41"));
        }

        [Fact]
        public void NoViewsAndNoScopeBoxesGiveAnEmptyPlanRatherThanAThrow()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(null, null);

            Assert.Empty(plan.Decisions);
            Assert.Empty(plan.ToAssign);
            Assert.Equal(0, plan.Count(ScopeBoxCase.ReadyToAssign));
        }

        [Fact]
        public void ANullInsideEitherListIsSteppedOver()
        {
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(
                new[] { null, View("DM-41-(010) Location Key Plan") },
                new[] { null, "DM-41" });

            Assert.Equal(ScopeBoxCase.ReadyToAssign, plan.Decisions.Single().Outcome);
        }

        [Fact]
        public void SurroundingWhitespaceOnAViewNameDoesNotStopTheMatch()
        {
            Assert.Equal(
                ScopeBoxCase.ReadyToAssign,
                CaseFor(View("DM-41-(010) Location Key Plan\r\n"), "DM-41"));
        }
    }
}
