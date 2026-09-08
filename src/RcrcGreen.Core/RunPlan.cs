using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    public enum RunItemKind
    {
        PlanView,
        Schedule
    }

    /// <summary>
    /// One thing the run will make.
    /// </summary>
    public sealed class RunItem
    {
        public RunItem(string plotId, ViewType type, RunItemKind kind)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");
            if (type == null) throw new ArgumentNullException("type");

            PlotId = plotId;
            Type = type;
            Kind = kind;
        }

        public string PlotId { get; }

        public ViewType Type { get; }

        public RunItemKind Kind { get; }

        public string Name
        {
            get { return PlotId + "-(" + Type.Code + ") " + Type.ViewName; }
        }
    }

    /// <summary>
    /// One thing the run will not make, and why.
    /// </summary>
    public sealed class RunRefusal
    {
        public RunRefusal(string plotId, ViewType type, string because)
        {
            PlotId = plotId ?? string.Empty;
            Type = type;
            Because = because ?? string.Empty;
        }

        public string PlotId { get; }

        public ViewType Type { get; }

        public string Because { get; }

        public override string ToString()
        {
            string what = Type == null ? PlotId : PlotId + " " + Type;
            return what + ". " + Because;
        }
    }

    /// <summary>
    /// What a run would make, worked out before anything is opened.
    ///
    /// Nothing is created for a plot that is not ticked or a cell that is not marked, and a
    /// refusal is written down rather than silently skipped, so the confirmation and the report
    /// both say the same thing and neither has to guess.
    /// </summary>
    public sealed class RunPlan
    {
        private RunPlan(IReadOnlyList<RunItem> items, IReadOnlyList<RunRefusal> refusals)
        {
            Items = items;
            Refusals = refusals;
        }

        public IReadOnlyList<RunItem> Items { get; }

        public IReadOnlyList<RunRefusal> Refusals { get; }

        public int PlanViewCount
        {
            get { return Items.Count(item => item.Kind == RunItemKind.PlanView); }
        }

        public int ScheduleCount
        {
            get { return Items.Count(item => item.Kind == RunItemKind.Schedule); }
        }

        public bool MakesNothing
        {
            get { return Items.Count == 0; }
        }

        /// <summary>
        /// Works out the run.
        /// </summary>
        /// <param name="marked">The cells the user marked. Nothing else is ever created.</param>
        /// <param name="ticked">The plots still ticked. A mark on an unticked plot is dropped
        /// without a refusal, because unticking is the user saying they do not want it.</param>
        /// <param name="plotsWithAScopeBox">A plan view on a plot with no scope box is useless
        /// on this project, so it is refused rather than made.</param>
        /// <param name="scheduleTypes">Which types are schedules rather than plan views.</param>
        /// <param name="capturableScheduleTypes">Schedule types some plot in the model already
        /// has, so there is something to capture a definition from. One that exists nowhere
        /// cannot be built from nothing and is refused by name.</param>
        public static RunPlan Of(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> capturableScheduleTypes)
        {
            var stillTicked = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);
            var withScopeBox = new HashSet<string>(
                (plotsWithAScopeBox ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);
            var schedules = new HashSet<ViewType>(
                (scheduleTypes ?? Enumerable.Empty<ViewType>()).Where(type => type != null));
            var capturable = new HashSet<ViewType>(
                (capturableScheduleTypes ?? Enumerable.Empty<ViewType>()).Where(type => type != null));

            var items = new List<RunItem>();
            var refusals = new List<RunRefusal>();

            IEnumerable<PlotViewKey> wanted = (marked ?? Enumerable.Empty<PlotViewKey>())
                .Where(key => key != null)
                .Where(key => stillTicked.Contains(key.PlotId))
                .OrderBy(key => key.PlotId, NaturalOrder.Comparer)
                .ThenBy(key => key.ViewType);

            foreach (PlotViewKey key in wanted)
            {
                if (schedules.Contains(key.ViewType))
                {
                    if (capturable.Contains(key.ViewType))
                    {
                        items.Add(new RunItem(key.PlotId, key.ViewType, RunItemKind.Schedule));
                    }
                    else
                    {
                        refusals.Add(new RunRefusal(
                            key.PlotId,
                            key.ViewType,
                            "No plot in this model has that schedule, so there is no definition "
                            + "to capture and nothing to build from."));
                    }

                    continue;
                }

                if (!withScopeBox.Contains(key.PlotId))
                {
                    refusals.Add(new RunRefusal(
                        key.PlotId,
                        key.ViewType,
                        "No scope box is named " + key.PlotId + ". A view with no scope box is "
                        + "useless on this project, so it is not created."));
                    continue;
                }

                items.Add(new RunItem(key.PlotId, key.ViewType, RunItemKind.PlanView));
            }

            return new RunPlan(items, refusals);
        }

        /// <summary>
        /// What the confirmation says, counted by kind. Someone about to write to a model needs
        /// the number before the dialog, not after.
        /// </summary>
        public string InWords()
        {
            if (MakesNothing)
            {
                return Refusals.Count == 0
                    ? "Nothing is marked on a ticked plot, so this run would make nothing."
                    : "This run would make nothing. " + Refusals.Count + " marked cells cannot be made.";
            }

            var said = new List<string>();
            if (PlanViewCount > 0)
            {
                said.Add(PlanViewCount == 1 ? "1 plan view" : PlanViewCount + " plan views");
            }
            if (ScheduleCount > 0)
            {
                said.Add(ScheduleCount == 1 ? "1 schedule" : ScheduleCount + " schedules");
            }

            string counts = string.Join(" and ", said.ToArray());
            if (Refusals.Count == 0) return "This run would make " + counts + ".";

            return "This run would make " + counts + ". " + Refusals.Count
                + " marked cells cannot be made and are named in the report.";
        }
    }
}
