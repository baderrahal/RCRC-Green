using System;
using System.Linq;
using RcrcGreen.Core;
using Xunit;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// What the run did, which is a different question from what it set out to do. Keeping the
    /// two apart is the whole fix for a report that listed four views as created and refused at
    /// the same time.
    /// </summary>
    public class RunOutcomeTests
    {
        private static readonly ViewType General = new ViewType("200", "General Arrangement Layout");
        private static readonly ViewType Hardscape = new ViewType("600", "HARDSCAPE SCHEDULE");

        [Fact]
        public void ARunThatWroteNothingHasCreatedNothing()
        {
            RunOutcome outcome = RunOutcome.NothingWasWritten();

            Assert.Empty(outcome.Created);
            Assert.Equal(0, outcome.CreatedCount);
            Assert.Equal(0, outcome.NotCreatedCount);
            Assert.Empty(outcome.BothWays);
        }

        [Fact]
        public void ThingsAreCountedByKind()
        {
            RunOutcome outcome = RunFixture.Outcome(made: new[]
            {
                new RunItem("DM-11", General, RunItemKind.PlanView),
                new RunItem("DM-12", General, RunItemKind.PlanView),
                new RunItem("DM-11", Hardscape, RunItemKind.Schedule),
                RunFixture.SheetItem("DM-11", "L-201", "Layout")
            });

            Assert.Equal(2, outcome.CreatedOfKind(RunItemKind.PlanView).Count);
            Assert.Single(outcome.CreatedOfKind(RunItemKind.Schedule));
            Assert.Single(outcome.CreatedOfKind(RunItemKind.Sheet));
            Assert.Empty(outcome.CreatedOfKind(RunItemKind.Section));
            Assert.Equal(4, outcome.CreatedCount);
        }

        /// <summary>
        /// A schedule left in the model is not created and is not refused either. It counts as
        /// not created, because the thing that was wanted is not there.
        /// </summary>
        [Fact]
        public void SomethingLeftInTheModelCountsAsNotCreated()
        {
            RunOutcome outcome = RunFixture.Outcome(
                refused: new[] { new RunRefusal("DM-11", General, "Revit refused it.") },
                leftBehind: new[] { new RunRefusal("DM-12", Hardscape, "STILL IN THE MODEL.") });

            Assert.Equal(2, outcome.NotCreatedCount);
            Assert.Single(outcome.NotCreated);
            Assert.Single(outcome.LeftBehind);
        }

        /// <summary>
        /// Needing attention is not the same as not being created. The thing is in the model
        /// and it is wrong, so counting it as not created would be a second lie.
        /// </summary>
        [Fact]
        public void SomethingNeedingAttentionIsStillCreated()
        {
            RunOutcome outcome = RunFixture.Outcome(
                made: new[] { new RunItem("DM-11", General, RunItemKind.PlanView) },
                attention: new[] { new RunRefusal("DM-11", General, "Created with no view template.") });

            Assert.Equal(1, outcome.CreatedCount);
            Assert.Equal(0, outcome.NotCreatedCount);
            Assert.Single(outcome.Attention);
            Assert.Empty(outcome.BothWays);
        }

        /// <summary>
        /// A refusal carries the same name string an item would have. That is what makes the
        /// contradiction a comparison rather than an argument about two naming schemes.
        /// </summary>
        [Fact]
        public void ARefusalAndAnItemNameTheSameThingTheSameWay()
        {
            var item = new RunItem("DM-11", General, RunItemKind.PlanView);
            var refusal = new RunRefusal("DM-11", General, "Revit refused it.");

            Assert.Equal("DM-11-(200) General Arrangement Layout", item.Name);
            Assert.Equal("DM-11-(200) General Arrangement Layout", refusal.Name);
            Assert.Equal("DM-11-(200) General Arrangement Layout. Revit refused it.", refusal.ToString());
        }

        [Fact]
        public void OneNameOnBothSidesComesBackFromBothWays()
        {
            RunOutcome outcome = RunFixture.Outcome(
                made: new[]
                {
                    new RunItem("DM-11", General, RunItemKind.PlanView),
                    new RunItem("DM-12", General, RunItemKind.PlanView)
                },
                refused: new[] { new RunRefusal("DM-12", General, "Revit refused it.") });

            Assert.Equal(new[] { "DM-12-(200) General Arrangement Layout" }, outcome.BothWays.ToArray());
        }

        [Fact]
        public void ASheetNamedByTheUserIsFoundOnBothSidesToo()
        {
            RunOutcome outcome = RunFixture.Outcome(
                made: new[] { RunFixture.SheetItem("DM-11", "L-201", "General Arrangement") },
                leftBehind: new[]
                {
                    RunRefusal.ForSheet("DM-11", "L-201", "General Arrangement", "Something went wrong.")
                });

            Assert.Equal(new[] { "L-201 General Arrangement" }, outcome.BothWays.ToArray());
        }

        [Fact]
        public void NothingIsEverRecordedAsNull()
        {
            RunOutcome outcome = RunOutcome.NothingWasWritten();

            Assert.Throws<ArgumentNullException>(() => outcome.Made(null));
            Assert.Throws<ArgumentNullException>(() => outcome.Refused(null));
            Assert.Throws<ArgumentNullException>(() => outcome.NeedsAttention(null));
            Assert.Throws<ArgumentNullException>(() => outcome.LeftInTheModel(null));
        }

        /// <summary>
        /// A sheet has no view type, so building one through the view constructor would give it
        /// a name it cannot have. It is refused rather than allowed through with a null.
        /// </summary>
        [Fact]
        public void ASheetCannotBeBuiltThroughTheViewConstructor()
        {
            Assert.Throws<ArgumentException>(
                () => new RunItem("DM-11", General, RunItemKind.Sheet));

            Assert.Throws<ArgumentException>(
                () => RunFixture.SheetItem("DM-11", string.Empty, "Layout"));

            Assert.Throws<ArgumentException>(
                () => RunFixture.SheetItem("DM-11", "L-201", string.Empty));
        }
    }
}
