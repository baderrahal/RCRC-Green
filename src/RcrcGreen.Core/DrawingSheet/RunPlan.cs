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
                    : ViewNaming.Of(PlotId, Type);
            }
        }
    }

    /// <summary>
    /// Why the run will not make something, as a category rather than as a sentence.
    ///
    /// The sentence names the plot, so counting by it gives a group of one per plot: five
    /// views refused for no scope box on five plots read as five reasons. Step 5 counts by
    /// this instead, and prints the sentences only when somebody opens the list.
    /// </summary>
    public enum RunRefusalKind
    {
        /// <summary>Set by a caller that did not say. Counted under its own heading rather
        /// than folded into another, because a miscounted reason is worse than an unnamed
        /// one.</summary>
        Unsaid = 0,

        AlreadyInTheModel = 1,

        NoScheduleToCaptureFrom = 2,

        NoScopeBox = 3,

        /// <summary>The definition itself is short of its title block, so it makes nothing on
        /// any plot.</summary>
        SheetKindIncomplete = 4,

        /// <summary>One plot's row is short of a name or a number.</summary>
        SheetRowIncomplete = 5,

        /// <summary>The number the row carries is one Revit will not take.</summary>
        SheetNumberClashes = 6,

        /// <summary>
        /// Every view the sheet was to carry was refused by this same run, so the sheet would
        /// come out empty under a number nobody can reuse.
        /// </summary>
        SheetHasNoViewLeft = 7
    }

    /// <summary>
    /// One thing the run will not make, and why.
    /// </summary>
    public sealed class RunRefusal
    {
        public RunRefusal(
            string plotId,
            ViewType type,
            string because,
            RunRefusalKind kind = RunRefusalKind.Unsaid)
        {
            PlotId = plotId ?? string.Empty;
            Type = type;
            Because = because ?? string.Empty;
            Kind = kind;
            Name = ViewNaming.Of(PlotId, type);
        }

        private RunRefusal(string plotId, string name, string because, RunRefusalKind kind)
        {
            PlotId = plotId ?? string.Empty;
            Type = null;
            Because = because ?? string.Empty;
            Kind = kind;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// A sheet that was not made. It is named the way the made one would have been, so the
        /// two sides of the report can still be held against each other.
        /// </summary>
        public static RunRefusal ForSheet(
            string plotId,
            string sheetNumber,
            string sheetName,
            string because,
            RunRefusalKind kind = RunRefusalKind.Unsaid)
        {
            string named = (sheetNumber + " " + sheetName).Trim();
            if (named.Length == 0) named = (plotId ?? string.Empty) + " sheet";

            return new RunRefusal(plotId, named, because, kind);
        }

        public string PlotId { get; }

        public ViewType Type { get; }

        public string Because { get; }

        public RunRefusalKind Kind { get; }

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
        /// <param name="sheetNumbersInUse">The sheet numbers the model holds at the moment
        /// the run is worked out, read fresh rather than taken from the panel. Three runs in
        /// a row asked Revit for numbers a previous run had already created, because the
        /// only number check lived on the panel and read its last snapshot.</param>
        public static RunPlan Of(
            IEnumerable<PlotViewKey> marked,
            IEnumerable<string> ticked,
            IEnumerable<string> plotsWithAScopeBox,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> sectionTypes,
            IEnumerable<ViewType> capturableScheduleTypes,
            IEnumerable<SheetBatch> sheetsWanted,
            IEnumerable<PlotViewKey> alreadyInTheModel = null,
            IEnumerable<UncapturableSchedule> schedulesThatCannotBeCaptured = null,
            IEnumerable<string> sheetNumbersInUse = null)
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
                        + "last read it, so nothing is made over it. Press Refresh to see it.",
                        RunRefusalKind.AlreadyInTheModel));
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

                        refusals.Add(new RunRefusal(
                            key.PlotId,
                            key.ViewType,
                            why,
                            RunRefusalKind.NoScheduleToCaptureFrom));
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
                                + "is useless on this project, so it is not created.",
                        RunRefusalKind.NoScopeBox));
                    continue;
                }

                items.Add(new RunItem(
                    key.PlotId,
                    key.ViewType,
                    sections.Contains(key.ViewType) ? RunItemKind.Section : RunItemKind.PlanView));
            }

            // The views are all decided by now, so a sheet can be asked whether anything it
            // carries survived. Passed rather than re-derived, because working out a second
            // time which views were refused is a second record of this run's own answer.
            var refusedViews = new HashSet<PlotViewKey>(
                refusals.Where(one => one.Type != null)
                    .Select(one => new PlotViewKey(one.PlotId, one.Type)));

            AddSheets(
                sheetsWanted, stillTicked, sheetNumbersInUse, items, refusals, refusedViews);

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
            IEnumerable<string> sheetNumbersInUse,
            List<RunItem> items,
            List<RunRefusal> refusals,
            HashSet<PlotViewKey> refusedViews)
        {
            var arrivals = new List<SheetArrival>();
            int batchAt = 0;

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
                        + batch.Definition.WhatIsMissing + ". The tool invents neither.",
                        RunRefusalKind.SheetKindIncomplete));
                    batchAt++;
                    continue;
                }

                int rowAt = 0;
                foreach (SheetToMake row in batch.Rows)
                {
                    if (stillTicked.Contains(row.PlotId))
                    {
                        arrivals.Add(new SheetArrival(row, batchAt, rowAt));
                    }

                    rowAt++;
                }

                batchAt++;
            }

            // The team's order, which is also the order the sheets are created in: the plot,
            // the code, the order list within a code, then how the sheets were described for
            // anything the list does not hold. Left in described order, the set came out in
            // whatever order the user ticked, which on a real plot put the schedules ahead
            // of the title sheet.
            // Every number this run asks for, so two rows wanting one number are both
            // caught here the way the panel catches them, whatever the panel showed.
            List<string> askedThisRun = arrivals
                .Where(one => one.Row.HasNumber)
                .Select(one => one.Row.SheetNumber)
                .ToList();

            foreach (SheetArrival one in arrivals
                .OrderBy(one => one.Row.PlotId, NaturalOrder.Comparer)
                .ThenBy(one => SheetOrder.CodeRank(SheetOrder.CodeToOrderBy(one.Row.Views)))
                .ThenBy(one => SheetOrder.CodeToOrderBy(one.Row.Views), NaturalOrder.Comparer)
                .ThenBy(one => SheetOrder.Position(one.Row.SheetName))
                .ThenBy(one => one.BatchAt)
                .ThenBy(one => one.RowAt))
            {
                SheetToMake row = one.Row;

                if (row.Views.Count > 0
                    && row.Views.All(view => refusedViews.Contains(
                        new PlotViewKey(row.PlotId, view))))
                {
                    // Every view it was to carry was refused a moment ago, so the sheet would
                    // be made empty and its number spent. The run of 2026-09-13 did exactly
                    // that: five views refused for no scope box on DM-02, five sheets created
                    // anyway, each reported as made without the view it was waiting for, and
                    // five numbers now taken in the model.
                    //
                    // A sheet with no views BY DESIGN is untouched. The cover page carries
                    // none and is still made.
                    refusals.Add(RunRefusal.ForSheet(
                        row.PlotId,
                        row.SheetNumber,
                        row.SheetName.Length == 0 ? row.ViewsInWords() : row.SheetName,
                        "No sheet was made for " + row.PlotId + ", because this run refused "
                        + (row.Views.Count == 1 ? "the view it carries, " : "every view it "
                            + "carries, ")
                        + string.Join(", ", row.Views
                            .Select(view => ViewNaming.Of(row.PlotId, view)).ToArray())
                        + ". It would have come out empty under a number nothing could "
                        + "reuse.",
                        RunRefusalKind.SheetHasNoViewLeft));
                    continue;
                }

                if (!row.CanBeMade)
                {
                    // The reason line rides along when the number could not be built, mixed
                    // codes or no views, so the refusal says why and not only that.
                    refusals.Add(RunRefusal.ForSheet(
                        row.PlotId,
                        row.SheetNumber,
                        row.SheetName.Length == 0 ? row.ViewsInWords() : row.SheetName,
                        "No sheet was made for " + row.PlotId + " holding "
                        + row.ViewsInWords() + ", because it is still missing "
                        + row.WhatIsMissing + ". The tool invents neither."
                        + (row.WhyTheNumberIsMissing.Length == 0
                            ? string.Empty
                            : " " + row.WhyTheNumberIsMissing),
                        RunRefusalKind.SheetRowIncomplete));
                    continue;
                }

                // Checked against the numbers read at Run, not against the panel's last
                // snapshot. A run that trusted the panel asked Revit for numbers its own
                // previous run had created, three runs in a row, and every refusal arrived
                // from Revit after the transaction was open instead of on this list.
                SheetNumberFault fault = SheetNumbers.FaultIn(
                    row.SheetNumber, sheetNumbersInUse, askedThisRun);
                if (fault != SheetNumberFault.None)
                {
                    refusals.Add(RunRefusal.ForSheet(
                        row.PlotId,
                        row.SheetNumber,
                        row.SheetName,
                        "No sheet was made for " + row.PlotId + ", because "
                        + SheetNumbers.FaultInWords(fault)
                        + ", read off the model as the run was worked out. Renumber the row.",
                        RunRefusalKind.SheetNumberClashes));
                    continue;
                }

                items.Add(RunItem.ForSheet(row));
            }
        }

        /// <summary>
        /// One sheet row with where it arrived, so the order can fall back to how the sheets
        /// were described when the list holds no answer.
        /// </summary>
        private sealed class SheetArrival
        {
            public SheetArrival(SheetToMake row, int batchAt, int rowAt)
            {
                Row = row;
                BatchAt = batchAt;
                RowAt = rowAt;
            }

            public SheetToMake Row { get; }

            public int BatchAt { get; }

            public int RowAt { get; }
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
