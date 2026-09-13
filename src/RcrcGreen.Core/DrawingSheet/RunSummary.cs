using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One reason the run will not make something, and how many it covers.
    /// </summary>
    public sealed class RefusalCount
    {
        internal RefusalCount(RunRefusalKind kind, int howMany)
        {
            Kind = kind;
            HowMany = howMany;
        }

        public RunRefusalKind Kind { get; }

        public int HowMany { get; }

        public string InWords()
        {
            return HowMany + " " + RunSummary.ReasonInWords(Kind);
        }
    }

    /// <summary>
    /// What step 5 says before the Run button.
    ///
    /// It used to draw a card per sheet, and a run over 35 sub plots with 7 definitions drew
    /// 247 of them. The user asked for the drawing to go. What is left is the two numbers
    /// somebody presses Run on: how many sheets it will make, and how many things it will not,
    /// counted by reason rather than printed one line each.
    ///
    /// **Counted by reason, not by sentence.** Every refusal sentence names its plot, so five
    /// views refused for no scope box on five plots would read as five different reasons. The
    /// sentences are still there and a control opens them.
    /// </summary>
    public static class RunSummary
    {
        /// <summary>
        /// Every reason in the plan with its count, the commonest first, and the same count
        /// broken the same way as the list a control opens.
        /// </summary>
        public static IReadOnlyList<RefusalCount> ByReason(RunPlan plan)
        {
            if (plan == null) return new List<RefusalCount>();

            return plan.Refusals
                .Where(one => one != null)
                .GroupBy(one => one.Kind)
                .Select(group => new RefusalCount(group.Key, group.Count()))
                .OrderByDescending(one => one.HowMany)
                .ThenBy(one => (int)one.Kind)
                .ToList();
        }

        /// <summary>
        /// What the run will make, in one line.
        /// </summary>
        public static string Makes(RunPlan plan)
        {
            if (plan == null || plan.MakesNothing)
            {
                return "Nothing is marked or described yet, so this run would make nothing.";
            }

            var parts = new List<string>();
            Add(parts, plan.CountOf(RunItemKind.PlanView), "plan view", "plan views");
            Add(parts, plan.CountOf(RunItemKind.Section), "section", "sections");
            Add(parts, plan.CountOf(RunItemKind.Schedule), "schedule", "schedules");
            Add(parts, plan.CountOf(RunItemKind.Sheet), "sheet", "sheets");

            return string.Join(", ", parts.ToArray()) + ".";
        }

        /// <summary>
        /// What it will not make, counted by reason. Empty when there is nothing to say, so a
        /// clean run shows no line rather than a line saying nothing is wrong.
        /// </summary>
        public static string CannotMake(RunPlan plan)
        {
            IReadOnlyList<RefusalCount> counts = ByReason(plan);
            if (counts.Count == 0) return string.Empty;

            int all = counts.Sum(one => one.HowMany);

            return (all == 1 ? "1 thing cannot be made: " : all + " things cannot be made: ")
                + string.Join(", ", counts.Select(one => one.InWords()).ToArray()) + ".";
        }

        /// <summary>
        /// What the control over the shut list says, so somebody knows how much is behind it.
        /// </summary>
        public static string ListBehind(RunPlan plan)
        {
            int all = plan == null ? 0 : plan.Refusals.Count;

            if (all == 0) return "Nothing refused";

            return all == 1 ? "1 refusal" : all + " refusals";
        }

        /// <summary>
        /// The heading one reason is counted under. It is the category and never the sentence,
        /// which names a plot and would give a heading per plot.
        /// </summary>
        public static string ReasonInWords(RunRefusalKind kind)
        {
            switch (kind)
            {
                case RunRefusalKind.AlreadyInTheModel:
                    return "already in the model";

                case RunRefusalKind.NoScheduleToCaptureFrom:
                    return "with no schedule to build from";

                case RunRefusalKind.NoScopeBox:
                    return "on a sub plot with no scope box";

                case RunRefusalKind.SheetKindIncomplete:
                    return "on a sheet missing its title block";

                case RunRefusalKind.SheetRowIncomplete:
                    return "on a row missing its name or its number";

                case RunRefusalKind.SheetNumberClashes:
                    return "under a number the model already holds";

                case RunRefusalKind.SheetHasNoViewLeft:
                    return "on a sheet whose every view this run refused";

                default:
                    return "for a reason the run did not name";
            }
        }

        private static void Add(List<string> parts, int howMany, string one, string many)
        {
            if (howMany == 0) return;

            parts.Add(howMany + " " + (howMany == 1 ? one : many));
        }
    }
}
