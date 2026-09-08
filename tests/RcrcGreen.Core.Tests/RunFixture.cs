using System.Collections.Generic;
using RcrcGreen.Core;

namespace RcrcGreen.Core.Tests
{
    /// <summary>
    /// Builds a run the way the Revit side would, so a test can name only the part it cares
    /// about. Nothing here works anything out. Every expected value in a test is written by
    /// hand, because a test that recomputes the rule proves the rule agrees with itself.
    /// </summary>
    internal static class RunFixture
    {
        /// <summary>
        /// The shape most tests want. No sections and no sheets, which is the run the panel
        /// made before this round.
        /// </summary>
        public static RunPlan Of(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> capturableScheduleTypes)
        {
            return RunPlan.Of(
                marked,
                ticked,
                plotsWithAScopeBox,
                scheduleTypes,
                null,
                capturableScheduleTypes,
                null,
                false);
        }

        public static RunPlan WithSections(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> sectionTypes)
        {
            return RunPlan.Of(
                marked, ticked, plotsWithAScopeBox, null, sectionTypes, null, null, false);
        }

        public static RunPlan WithSheets(
            IEnumerable<string> ticked,
            IEnumerable<SheetRequest> sheetsWanted,
            bool haveASheetDefinition)
        {
            return RunPlan.Of(
                null, ticked, null, null, null, null, sheetsWanted, haveASheetDefinition);
        }

        public static RunOutcome Outcome(
            IEnumerable<RunItem> made = null,
            IEnumerable<RunRefusal> refused = null,
            IEnumerable<RunRefusal> attention = null,
            IEnumerable<RunRefusal> leftBehind = null)
        {
            RunOutcome outcome = RunOutcome.NothingWasWritten();

            if (made != null) foreach (RunItem one in made) outcome.Made(one);
            if (refused != null) foreach (RunRefusal one in refused) outcome.Refused(one);
            if (attention != null) foreach (RunRefusal one in attention) outcome.NeedsAttention(one);
            if (leftBehind != null) foreach (RunRefusal one in leftBehind) outcome.LeftInTheModel(one);

            return outcome;
        }
    }
}
