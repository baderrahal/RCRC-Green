using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    public enum RunItemKind
    {
        PlanView,
        Section,
        Schedule,
        Sheet
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
            if (kind == RunItemKind.Sheet)
            {
                throw new ArgumentException("A sheet is made by ForSheet, which carries the "
                    + "number and the name the user typed.", "kind");
            }

            PlotId = plotId;
            Type = type;
            Kind = kind;
            SheetNumber = string.Empty;
            SheetName = string.Empty;
        }

        private RunItem(string plotId, string sheetNumber, string sheetName)
        {
            PlotId = plotId;
            Type = null;
            Kind = RunItemKind.Sheet;
            SheetNumber = sheetNumber;
            SheetName = sheetName;
        }

        /// <summary>
        /// A sheet carries no view type. It is named by what the user typed, and that is the
        /// only name it can have, because the tool invents neither half.
        /// </summary>
        public static RunItem ForSheet(SheetRequest wanted)
        {
            if (wanted == null) throw new ArgumentNullException("wanted");
            if (!wanted.Complete)
            {
                throw new ArgumentException(
                    "That sheet request is missing " + wanted.WhatIsMissing + ".", "wanted");
            }

            return new RunItem(wanted.PlotId, wanted.SheetNumber, wanted.SheetName);
        }

        public string PlotId { get; }

        /// <summary>
        /// Null on a sheet and never null on anything else.
        /// </summary>
        public ViewType Type { get; }

        public RunItemKind Kind { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public string Name
        {
            get
            {
                return Kind == RunItemKind.Sheet
                    ? SheetNumber + " " + SheetName
                    : PlotId + "-(" + Type.Code + ") " + Type.ViewName;
            }
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
            Name = type == null ? PlotId : PlotId + "-(" + type.Code + ") " + type.ViewName;
        }

        private RunRefusal(string plotId, string name, string because)
        {
            PlotId = plotId ?? string.Empty;
            Type = null;
            Because = because ?? string.Empty;
            Name = name ?? string.Empty;
        }

        public static RunRefusal ForSheet(SheetRequest wanted, string because)
        {
            if (wanted == null) throw new ArgumentNullException("wanted");

            string named = wanted.SheetNumber.Length > 0 || wanted.SheetName.Length > 0
                ? (wanted.SheetNumber + " " + wanted.SheetName).Trim()
                : wanted.PlotId + " sheet";

            return new RunRefusal(wanted.PlotId, named, because);
        }

        public string PlotId { get; }

        public ViewType Type { get; }

        public string Because { get; }

        /// <summary>
        /// The same name the item would have carried had it been made. It is the same string on
        /// both sides on purpose, so a report claiming a thing was both created and refused can
        /// be caught by looking rather than by reasoning about it.
        /// </summary>
        public string Name { get; }

        public override string ToString()
        {
            return Name + ". " + Because;
        }
    }

    /// <summary>
    /// What a run would make, worked out before anything is opened.
    ///
    /// Nothing is created for a plot that is not ticked or a cell that is not marked, and a
    /// refusal is written down rather than silently skipped, so the confirmation and the report
    /// both say the same thing and neither has to guess.
    ///
    /// This is what the run INTENDED. What it managed is <see cref="RunOutcome"/>, and the two
    /// are separate types because printing the plan under a heading reading created is exactly
    /// the fault that made a report list four views as both made and refused.
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

        public int CountOf(RunItemKind kind)
        {
            return Items.Count(item => item.Kind == kind);
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
        /// on this project, and a section has nowhere to cut, so both are refused.</param>
        /// <param name="scheduleTypes">Which types are schedules rather than views.</param>
        /// <param name="sectionTypes">Which types are sections rather than plan views. This
        /// comes from the kind of the view the model already holds for that type, never from
        /// the code in the name. (400) is a section in this model and that is a fact about this
        /// model, not a rule.</param>
        /// <param name="capturableScheduleTypes">Schedule types some plot in the model already
        /// has, so there is something to capture a definition from.</param>
        /// <param name="sheetsWanted">One per plot, holding what the user typed.</param>
        /// <param name="haveASheetDefinition">Whether a source sheet was captured. Without one
        /// there is no title block and no layout, so no sheet can be built.</param>
        public static RunPlan Of(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> sectionTypes,
            IEnumerable<ViewType> capturableScheduleTypes,
            IEnumerable<SheetRequest> sheetsWanted,
            bool haveASheetDefinition)
        {
            var stillTicked = new HashSet<string>(
                (ticked ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);
            var withScopeBox = new HashSet<string>(
                (plotsWithAScopeBox ?? Enumerable.Empty<string>()).Where(plotId => plotId != null),
                StringComparer.Ordinal);
            var schedules = new HashSet<ViewType>(
                (scheduleTypes ?? Enumerable.Empty<ViewType>()).Where(type => type != null));
            var sections = new HashSet<ViewType>(
                (sectionTypes ?? Enumerable.Empty<ViewType>()).Where(type => type != null));
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
                        sections.Contains(key.ViewType)
                            ? "No scope box is named " + key.PlotId + ". A section is cut across "
                                + "the middle of the plot's scope box, so with no box there is "
                                + "nowhere to cut."
                            : "No scope box is named " + key.PlotId + ". A view with no scope box "
                                + "is useless on this project, so it is not created."));
                    continue;
                }

                items.Add(new RunItem(
                    key.PlotId,
                    key.ViewType,
                    sections.Contains(key.ViewType) ? RunItemKind.Section : RunItemKind.PlanView));
            }

            AddSheets(sheetsWanted, stillTicked, haveASheetDefinition, items, refusals);

            return new RunPlan(items, refusals);
        }

        private static void AddSheets(
            IEnumerable<SheetRequest> sheetsWanted,
            HashSet<string> stillTicked,
            bool haveASheetDefinition,
            List<RunItem> items,
            List<RunRefusal> refusals)
        {
            IEnumerable<SheetRequest> asked = (sheetsWanted ?? Enumerable.Empty<SheetRequest>())
                .Where(one => one != null)
                .Where(one => stillTicked.Contains(one.PlotId))
                // A plot with both boxes empty is a plot the user did not ask for a sheet on.
                // Refusing that would fill the report with rows nobody asked about.
                .Where(one => !one.Blank)
                .OrderBy(one => one.PlotId, NaturalOrder.Comparer);

            foreach (SheetRequest one in asked)
            {
                if (!one.Complete)
                {
                    refusals.Add(RunRefusal.ForSheet(one,
                        "No sheet was made for " + one.PlotId + ", because it is missing "
                        + one.WhatIsMissing + ". The tool invents neither."));
                    continue;
                }

                if (!haveASheetDefinition)
                {
                    refusals.Add(RunRefusal.ForSheet(one,
                        "No source sheet was captured, so there is no title block to use and no "
                        + "layout to copy. Pick a sheet to copy from and run again."));
                    continue;
                }

                items.Add(RunItem.ForSheet(one));
            }
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
                    : "This run would make nothing. " + Refusals.Count + " things cannot be made.";
            }

            var said = new List<string>();
            Say(said, RunItemKind.PlanView, "plan view", "plan views");
            Say(said, RunItemKind.Section, "section", "sections");
            Say(said, RunItemKind.Schedule, "schedule", "schedules");
            Say(said, RunItemKind.Sheet, "sheet", "sheets");

            string counts = Listed(said);
            if (Refusals.Count == 0) return "This run would make " + counts + ".";

            return "This run would make " + counts + ". " + Refusals.Count
                + " things cannot be made and are named in the report.";
        }

        private void Say(List<string> said, RunItemKind kind, string one, string many)
        {
            int howMany = CountOf(kind);
            if (howMany == 0) return;

            said.Add(howMany == 1 ? "1 " + one : howMany + " " + many);
        }

        private static string Listed(List<string> said)
        {
            if (said.Count == 1) return said[0];
            if (said.Count == 2) return said[0] + " and " + said[1];

            return string.Join(", ", said.Take(said.Count - 1).ToArray())
                + " and " + said[said.Count - 1];
        }
    }
}
