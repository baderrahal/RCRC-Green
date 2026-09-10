using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One reading of the model, as plain values, held by the panel between refreshes.
    ///
    /// The panel keeps this and no Revit object at all. That is what lets it stay open while
    /// the user closes one document and opens another, because nothing in it goes stale in a
    /// way that can throw.
    ///
    /// It carries the scope box state of every view as well, so the six case counts can be
    /// worked out again on every tick without going back to Revit for them.
    /// </summary>
    public sealed class DrawingSheetSnapshot
    {
        public static readonly DrawingSheetSnapshot Nothing = new DrawingSheetSnapshot(
            string.Empty, default(DateTime), null, null, null, null, null, null, null,
            null, null, null, 0, 0, 0, 0, 0);

        public DrawingSheetSnapshot(
            string documentTitle,
            DateTime readAt,
            IEnumerable<string> plotIds,
            IEnumerable<ViewType> viewTypes,
            IEnumerable<PlotViewPresence> present,
            IEnumerable<ViewType> scheduleTypes,
            IEnumerable<ViewType> sectionTypes,
            IEnumerable<string> scopeBoxNames,
            IEnumerable<ViewScopeBoxState> viewStates,
            IEnumerable<TitleBlockType> titleBlockTypes,
            IEnumerable<string> sheetNamesInUse,
            IEnumerable<string> sheetNumbersInUse,
            int viewsRead,
            int fromParameter,
            int fromViewName,
            int withNoPlot,
            int sourcesDisagree,
            IEnumerable<SheetOnAPlot> sheetNumbersByPlot = null,
            IEnumerable<PlotRecord> plots = null,
            IEnumerable<ViewType> capturableScheduleTypes = null,
            IEnumerable<UncapturableSchedule> uncapturableSchedules = null)
        {
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            DocumentTitle = documentTitle;
            ReadAt = readAt;

            Plots = (plots ?? Enumerable.Empty<PlotRecord>())
                .Where(one => one != null)
                .ToList();

            _recordByPlot = new Dictionary<string, PlotRecord>(StringComparer.Ordinal);
            foreach (PlotRecord record in Plots)
            {
                if (!_recordByPlot.ContainsKey(record.PlotId)) _recordByPlot.Add(record.PlotId, record);
            }

            // The records and the id list both come from one registry call in the reader, and
            // the union here is the guard against them ever drifting apart.
            PlotIds = Clean(plotIds)
                .Concat(Plots.Select(one => one.PlotId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(plotId => plotId, NaturalOrder.Comparer)
                .ToList();

            ViewTypes = (viewTypes ?? Enumerable.Empty<ViewType>())
                .Where(viewType => viewType != null)
                .Distinct()
                .OrderBy(viewType => viewType)
                .ToList();

            Present = (present ?? Enumerable.Empty<PlotViewPresence>())
                .Where(one => one != null)
                .ToList();

            ScheduleTypes = (scheduleTypes ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Distinct()
                .OrderBy(one => one)
                .ToList();

            SectionTypes = (sectionTypes ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Distinct()
                .OrderBy(one => one)
                .ToList();

            TitleBlockTypes = (titleBlockTypes ?? Enumerable.Empty<TitleBlockType>())
                .Where(one => one != null)
                .OrderBy(one => one)
                .ToList();

            SheetNamesInUse = Clean(sheetNamesInUse)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, NaturalOrder.Comparer)
                .ToList();

            SheetNumbersInUse = Clean(sheetNumbersInUse)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(number => number, NaturalOrder.Comparer)
                .ToList();

            _numbersByPlot = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (SheetOnAPlot one in (sheetNumbersByPlot ?? Enumerable.Empty<SheetOnAPlot>())
                .Where(one => one != null && one.PlotId.Length > 0 && one.SheetNumber.Length > 0))
            {
                List<string> held;
                if (!_numbersByPlot.TryGetValue(one.PlotId, out held))
                {
                    held = new List<string>();
                    _numbersByPlot.Add(one.PlotId, held);
                }

                held.Add(one.SheetNumber);
            }

            ScopeBoxNames = Clean(scopeBoxNames)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, NaturalOrder.Comparer)
                .ToList();

            ViewStates = (viewStates ?? Enumerable.Empty<ViewScopeBoxState>())
                .Where(one => one != null)
                .ToList();

            // A scope box only names a plot when its name is exactly a plot identifier. The
            // real model holds 406 boxes for 160 plots, so most of them name nothing.
            PlotsWithAScopeBox = ScopeBoxNames
                .Where(PlotId.IsPlotId)
                .ToList();

            CapturableScheduleTypes = (capturableScheduleTypes ?? Enumerable.Empty<ViewType>())
                .Where(one => one != null)
                .Distinct()
                .OrderBy(one => one)
                .ToList();

            UncapturableSchedules = (uncapturableSchedules ?? Enumerable.Empty<UncapturableSchedule>())
                .Where(one => one != null)
                .ToList();

            ViewsRead = viewsRead;
            FromParameter = fromParameter;
            FromViewName = fromViewName;
            WithNoPlot = withNoPlot;
            SourcesDisagree = sourcesDisagree;
        }

        /// <summary>
        /// Schedule types whose captured definition can really be aimed at another plot. The
        /// plan preview reads this, the run reads the same rule off a fresh capture, so the
        /// preview and the confirmation follow one rule and only the data can differ.
        /// </summary>
        public IReadOnlyList<ViewType> CapturableScheduleTypes { get; }

        /// <summary>
        /// Schedule types that exist and cannot be captured, each with the reason its refusal
        /// prints.
        /// </summary>
        public IReadOnlyList<UncapturableSchedule> UncapturableSchedules { get; }

        public string DocumentTitle { get; }

        /// <summary>
        /// When the read ran. Shown in the panel, because the first time a refresh looked
        /// wrong there was no way to tell whether it had run at all.
        /// </summary>
        public DateTime ReadAt { get; }

        /// <summary>
        /// Every plot the model actually holds. The panel offers nothing outside this list,
        /// which is how it can never act on a plot that does not exist.
        /// </summary>
        public IReadOnlyList<string> PlotIds { get; }

        /// <summary>
        /// Each plot with every source it was found through: view names, PRX_Plot_ID on
        /// views, a scope box, PRX_Plot_ID on elements. The union of all of them is the plot
        /// list, because a plot with only a scope box and tagged elements has no views at all
        /// and is exactly the plot the team needs to see. The list came from views alone once
        /// and silently dropped that plot.
        /// </summary>
        public IReadOnlyList<PlotRecord> Plots { get; }

        private readonly Dictionary<string, PlotRecord> _recordByPlot;

        /// <summary>
        /// The record for one plot, or null when the snapshot was built without records.
        /// </summary>
        public PlotRecord RecordOf(string plotId)
        {
            PlotRecord found;
            return plotId != null && _recordByPlot.TryGetValue(plotId, out found) ? found : null;
        }

        public IReadOnlyList<ViewType> ViewTypes { get; }

        public IReadOnlyList<PlotViewPresence> Present { get; }

        /// <summary>
        /// The view types that are schedules rather than plan views. Six of the things under a
        /// plot in the real model sit under Schedules and Quantities, they are built by a
        /// different call, and they filter on a different parameter, so the grid has to show
        /// which is which.
        /// </summary>
        public IReadOnlyList<ViewType> ScheduleTypes { get; }

        public bool IsASchedule(ViewType type)
        {
            return type != null && ScheduleTypes.Contains(type);
        }

        /// <summary>
        /// The view types the model holds as sections rather than plan views. On this model
        /// (400) Landscape Cross Section is one, and its template is the only one of the listed
        /// templates whose kind is Section.
        ///
        /// This is read off the kind of the views the model already has, never off the code in
        /// the name. ViewPlan.Create can never make a section, which is what the first real run
        /// found out with a message that blamed the level.
        /// </summary>
        public IReadOnlyList<ViewType> SectionTypes { get; }

        public bool IsASection(ViewType type)
        {
            return type != null && SectionTypes.Contains(type);
        }

        /// <summary>
        /// The title block types a new sheet can be made with. No sheet can be made without one,
        /// and the list is read from the model rather than written down.
        /// </summary>
        public IReadOnlyList<TitleBlockType> TitleBlockTypes { get; }

        /// <summary>
        /// The names and numbers already in use, offered as lists the user can pick from or type
        /// past. They are a convenience and never a restriction, because a new sheet usually
        /// carries a number no sheet has yet.
        /// </summary>
        public IReadOnlyList<string> SheetNamesInUse { get; }

        public IReadOnlyList<string> SheetNumbersInUse { get; }

        private readonly Dictionary<string, List<string>> _numbersByPlot;

        /// <summary>
        /// The sheet numbers already on one plot, read off PRX_Plot_ID on the sheets. They are
        /// what the plot letter is worked out from, so a plot whose sheets carry no parameter
        /// reads as having no numbers and gets no proposal.
        /// </summary>
        public IReadOnlyList<string> NumbersOnPlot(string plotId)
        {
            List<string> held;
            return plotId != null && _numbersByPlot.TryGetValue(plotId, out held)
                ? (IReadOnlyList<string>)held
                : new List<string>();
        }

        /// <summary>
        /// Numbers no sheet in this model carries, which is what the dropdown offers.
        ///
        /// It used to offer the numbers already in use, so every entry in it was certain to be
        /// refused. Three sheets were lost to that in one run. Worked out here rather than in
        /// the panel, like every other list it shows.
        /// </summary>
        public IReadOnlyList<string> FreeSheetNumbers
        {
            get { return SheetNumbers.Free(SheetNumbersInUse); }
        }

        /// <summary>
        /// Every scope box name in the model, whether or not it is shaped like a plot.
        /// <see cref="ScopeBoxPlan"/> matches on the whole name, so it needs all of them.
        /// </summary>
        public IReadOnlyList<string> ScopeBoxNames { get; }

        public IReadOnlyList<ViewScopeBoxState> ViewStates { get; }

        public IReadOnlyList<string> PlotsWithAScopeBox { get; }

        public int ViewsRead { get; }

        /// <summary>
        /// How many views found their plot through PRX_Plot_ID, and how many through the name.
        /// Both are shown, because the split is the reason the parameter is read first.
        /// </summary>
        public int FromParameter { get; }

        public int FromViewName { get; }

        public int WithNoPlot { get; }

        /// <summary>
        /// Views whose name parses to one plot while PRX_Plot_ID holds another. Shown rather
        /// than hidden, because this is the model problem that used to put a view under a plot
        /// no view of that plot existed for.
        /// </summary>
        public int SourcesDisagree { get; }

        public bool Empty
        {
            get { return PlotIds.Count == 0; }
        }

        private static IEnumerable<string> Clean(IEnumerable<string> values)
        {
            if (values == null) return Enumerable.Empty<string>();
            return values.Where(value => !string.IsNullOrEmpty(value));
        }
    }

    /// <summary>
    /// One sheet number and the plot its sheet carries in PRX_Plot_ID. Plain values, so the
    /// snapshot can group them without holding anything from the model.
    /// </summary>
    public sealed class SheetOnAPlot
    {
        public SheetOnAPlot(string plotId, string sheetNumber)
        {
            PlotId = (plotId ?? string.Empty).Trim();
            SheetNumber = (sheetNumber ?? string.Empty).Trim();
        }

        public string PlotId { get; }

        public string SheetNumber { get; }
    }
}
