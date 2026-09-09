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
        public const string TitleBlockFamily = "AR-PRX-Title_Block_A1";

        public const string TitleBlockType = "GA-DETAILED DESIGN";

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
                null);
        }

        public static RunPlan WithSections(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> sectionTypes)
        {
            return RunPlan.Of(
                marked, ticked, plotsWithAScopeBox, null, sectionTypes, null, null);
        }

        public static RunPlan WithSheets(
            IEnumerable<string> ticked,
            params SheetBatch[] sheetsWanted)
        {
            return RunPlan.Of(null, ticked, null, null, null, null, sheetsWanted);
        }

        /// <summary>
        /// One described sheet and its rows, the way the panel hands them over.
        /// </summary>
        public static SheetBatch Batch(
            IEnumerable<ViewType> views, int perSheet, params SheetToMake[] rows)
        {
            return new SheetBatch(
                new SheetDefinition(TitleBlockFamily, TitleBlockType, views, perSheet),
                rows);
        }

        /// <summary>
        /// A definition still short of its title block, which is the one thing a definition
        /// can be missing now that names and numbers live on the rows.
        /// </summary>
        public static SheetBatch BatchMissingItsType(params SheetToMake[] rows)
        {
            return new SheetBatch(
                new SheetDefinition(string.Empty, string.Empty, null, 1), rows);
        }

        /// <summary>
        /// One row of the step 4 table. Pass an empty number or name for a row still short of
        /// one.
        /// </summary>
        public static SheetToMake Row(
            string plotId,
            string number,
            string name,
            IEnumerable<ViewType> views = null,
            int perSheet = 1)
        {
            return new SheetToMake(
                plotId, number, name, views, perSheet,
                TitleBlockFamily, TitleBlockType, false, false);
        }

        /// <summary>
        /// One made sheet, named the way the run would name it.
        /// </summary>
        public static RunItem SheetItem(string plotId, string number, string name)
        {
            return RunItem.ForSheet(Row(plotId, number, name));
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
