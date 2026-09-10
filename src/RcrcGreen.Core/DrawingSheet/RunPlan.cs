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

        private RunItem(SheetToMake sheet)
        {
            PlotId = sheet.PlotId;
            Type = null;
            Kind = RunItemKind.Sheet;
            SheetNumber = sheet.SheetNumber;
            SheetName = sheet.SheetName;
            Sheet = sheet;
        }

        /// <summary>
        /// A sheet carries no view type. Its name and its number sit on its own row, generated
        /// or typed, and a row short of either never becomes an item.
        /// </summary>
        public static RunItem ForSheet(SheetToMake sheet)
        {
            if (sheet == null) throw new ArgumentNullException("sheet");
            if (!sheet.CanBeMade)
            {
                throw new ArgumentException(
                    "That sheet is missing " + sheet.WhatIsMissing + ".", "sheet");
            }

            return new RunItem(sheet);
        }

        public string PlotId { get; }

        /// <summary>
        /// Null on a sheet and never null on anything else.
        /// </summary>
        public ViewType Type { get; }

        public RunItemKind Kind { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        /// <summary>
        /// What goes on this sheet and how it lays out. Null on anything that is not a sheet.
        /// </summary>
        public SheetToMake Sheet { get; }

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

        /// <summary>
        /// A sheet that was not made. It is named the way the made one would have been, so the
        /// two sides of the report can still be held against each other.
        /// </summary>
        public static RunRefusal ForSheet(string plotId, string sheetNumber, string sheetName, string because)
        {
            string named = (sheetNumber + " " + sheetName).Trim();
            if (named.Length == 0) named = (plotId ?? string.Empty) + " sheet";

            return new RunRefusal(plotId, named, because);
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
        /// <param name="capturableScheduleTypes">Schedule types whose captured definition can
        /// really be aimed at another plot. The panel and the run both read this off the same
        /// capture rule, so the preview can never promise a schedule the run refuses.</param>
        /// <param name="sheetsWanted">The sheets the user described, each already divided into
        /// the rows it makes across the ticked plots, one row per sheet.</param>
        /// <param name="alreadyInTheModel">The cells whose view exists in the freshest read. A
        /// mark can go stale while the panel sits open in a shared model, and creating over an
        /// existing name leaves an orphan view behind a rename Revit refuses.</param>
        /// <param name="schedulesThatCannotBeCaptured">Schedule types that exist and cannot be
        /// captured, each with the reason its refusal prints. Without this every one was
        /// answered with no plot has that schedule, which is false for all of them.</param>
        public static RunPlan Of(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> sectionTypes,
            IEnumerable<ViewType> capturableScheduleTypes,
            IEnumerable<SheetBatch> sheetsWanted,
            IEnumerable<PlotViewKey> alreadyInTheModel = null,
            IEnumerable<UncapturableSchedule> schedulesThatCannotBeCaptured = null)
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

            var present = new HashSet<PlotViewKey>(
                (alreadyInTheModel ?? Enumerable.Empty<PlotViewKey>()).Where(key => key != null));

            var whyNotCapturable = new Dictionary<ViewType, string>();
            foreach (UncapturableSchedule one in (schedulesThatCannotBeCaptured
                ?? Enumerable.Empty<UncapturableSchedule>()).Where(one => one != null))
            {
                if (!whyNotCapturable.ContainsKey(one.Type)) whyNotCapturable.Add(one.Type, one.Why);
            }

            var items = new List<RunItem>();
            var refusals = new List<RunRefusal>();

            IEnumerable<PlotViewKey> wanted = (marked ?? Enumerable.Empty<PlotViewKey>())
                .Where(key => key != null)
                .Where(key => stillTicked.Contains(key.PlotId))
                .OrderBy(key => key.PlotId, NaturalOrder.Comparer)
                .ThenBy(key => key.ViewType);

            foreach (PlotViewKey key in wanted)
            {
                // Checked before anything else, because every kind clashes the same way. The
                // grid never offers an existing cell for marking, so a mark that lands here
                // went stale while the panel sat open and somebody else filled the cell.
                if (present.Contains(key))
                {
                    refusals.Add(new RunRefusal(
                        key.PlotId,
                        key.ViewType,
                        "A view with this name is already in the model, added since the panel "
                        + "last read it, so nothing is made over it. Press Refresh to see it."));
                    continue;
                }

                if (schedules.Contains(key.ViewType))
                {
                    if (capturable.Contains(key.ViewType))
                    {
                        items.Add(new RunItem(key.PlotId, key.ViewType, RunItemKind.Schedule));
                    }
                    else
                    {
                        string why;
                        if (!whyNotCapturable.TryGetValue(key.ViewType, out why))
                        {
                            why = "No plot in this model has that schedule, so there is no "
                                + "definition to capture and nothing to build from.";
                        }

                        refusals.Add(new RunRefusal(key.PlotId, key.ViewType, why));
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

            AddSheets(sheetsWanted, stillTicked, items, refusals);

            return new RunPlan(items, refusals);
        }

        /// <summary>
        /// Every row of every described sheet, and a refusal for every row that cannot be made.
        ///
        /// A definition short of a title block is refused once, not once per row, because it is
        /// one thing the user has to go and fix rather than seventeen. A row short of a name or
        /// a number is refused by itself, naming its plot and its views, because the row next
        /// to it may be complete.
        /// </summary>
        private static void AddSheets(
            IEnumerable<SheetBatch> sheetsWanted,
            HashSet<string> stillTicked,
            List<RunItem> items,
            List<RunRefusal> refusals)
        {
            foreach (SheetBatch batch in (sheetsWanted ?? Enumerable.Empty<SheetBatch>())
                .Where(one => one != null))
            {
                if (!batch.Definition.CanBeUsed)
                {
                    refusals.Add(RunRefusal.ForSheet(
                        string.Empty,
                        string.Empty,
                        batch.Definition.TitleBlock,
                        "No sheet of this kind was made on any plot, because it is missing "
                        + batch.Definition.WhatIsMissing + ". The tool invents neither."));
                    continue;
                }

                foreach (SheetToMake row in batch.Rows)
                {
                    if (!stillTicked.Contains(row.PlotId)) continue;

                    if (!row.CanBeMade)
                    {
                        refusals.Add(RunRefusal.ForSheet(
                            row.PlotId,
                            row.SheetNumber,
                            row.SheetName.Length == 0 ? row.ViewsInWords() : row.SheetName,
                            "No sheet was made for " + row.PlotId + " holding "
                            + row.ViewsInWords() + ", because it is still missing "
                            + row.WhatIsMissing + ". The tool invents neither."));
                        continue;
                    }

                    items.Add(RunItem.ForSheet(row));
                }
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
                    : "This run would make nothing. " + CannotBeMade() + ".";
            }

            string counts = CountsInWords();
            if (Refusals.Count == 0) return "This run would make " + counts + ".";

            return "This run would make " + counts + ". " + CannotBeMade()
                + (Refusals.Count == 1
                    ? " and is named in the report."
                    : " and are named in the report.");
        }

        private string CannotBeMade()
        {
            return Refusals.Count == 1
                ? "1 thing cannot be made"
                : Refusals.Count + " things cannot be made";
        }

        /// <summary>
        /// The counts on their own, with no sentence round them, for a place that has room for
        /// a phrase rather than a line. The step headers use it.
        /// </summary>
        public string CountsInWords()
        {
            if (MakesNothing) return "nothing to make";

            var said = new List<string>();
            Say(said, RunItemKind.PlanView, "plan view", "plan views");
            Say(said, RunItemKind.Section, "section", "sections");
            Say(said, RunItemKind.Schedule, "schedule", "schedules");
            Say(said, RunItemKind.Sheet, "sheet", "sheets");

            return Listed(said);
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
