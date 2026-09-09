using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What one loaded link holds by way of filled regions, and what PRX_Intervention Area
    /// looks like on them.
    /// </summary>
    public sealed class LinkContents
    {
        public LinkContents(
            string linkName,
            string documentTitle,
            int filledRegionCount,
            IEnumerable<NameCount> typeCounts,
            IEnumerable<NameCount> viewCounts,
            IEnumerable<FilledRegionRead> firstRegions,
            IEnumerable<ParameterTally> regionParameters,
            IEnumerable<MeasuredValue> interventionAreas,
            IEnumerable<NameCount> typesCarryingAPlot = null,
            IEnumerable<MeasuredValue> regionsCarryingAPlot = null,
            int plotWithValue = 0,
            int plotCarriedBlank = 0,
            int plotNotCarried = 0)
        {
            if (linkName == null) throw new ArgumentNullException("linkName");
            if (filledRegionCount < 0) throw new ArgumentOutOfRangeException("filledRegionCount");
            if (plotWithValue < 0) throw new ArgumentOutOfRangeException("plotWithValue");
            if (plotCarriedBlank < 0) throw new ArgumentOutOfRangeException("plotCarriedBlank");
            if (plotNotCarried < 0) throw new ArgumentOutOfRangeException("plotNotCarried");

            LinkName = linkName;
            DocumentTitle = documentTitle ?? string.Empty;
            FilledRegionCount = filledRegionCount;
            TypeCounts = Held(typeCounts);
            ViewCounts = Held(viewCounts);
            FirstRegions = Held(firstRegions);
            RegionParameters = Held(regionParameters);
            InterventionAreas = Held(interventionAreas);
            TypesCarryingAPlot = Held(typesCarryingAPlot);
            RegionsCarryingAPlot = Held(regionsCarryingAPlot);
            PlotWithValue = plotWithValue;
            PlotCarriedBlank = plotCarriedBlank;
            PlotNotCarried = plotNotCarried;
        }

        /// <summary>
        /// What a region carrying a plot and no intervention area shows in the raw and printed
        /// columns. It is a word rather than an empty cell, because an empty cell in a row of
        /// numbers reads as a number that failed to print.
        /// </summary>
        public const string NoAreaValue = "(no value)";

        public string LinkName { get; }

        public string DocumentTitle { get; }

        public int FilledRegionCount { get; }

        public IReadOnlyList<NameCount> TypeCounts { get; }

        public IReadOnlyList<NameCount> ViewCounts { get; }

        public IReadOnlyList<FilledRegionRead> FirstRegions { get; }

        /// <summary>
        /// Every parameter name on any filled region in the link, tallied, which is where a
        /// near miss for PRX_Intervention Area is looked for.
        /// </summary>
        public IReadOnlyList<ParameterTally> RegionParameters { get; }

        /// <summary>
        /// One entry per filled region carrying PRX_Intervention Area with a value in it. The
        /// report caps what it prints.
        /// </summary>
        public IReadOnlyList<MeasuredValue> InterventionAreas { get; }

        /// <summary>
        /// How many regions of each type carry PRX_Ref Plot ID. A type whose regions all carry
        /// a plot is a candidate for the plot's intervention area, and one whose regions carry
        /// none is not, which is the question the report exists to put in front of somebody.
        /// </summary>
        public IReadOnlyList<NameCount> TypesCarryingAPlot { get; }

        /// <summary>
        /// One entry per filled region carrying PRX_Ref Plot ID, whether or not its
        /// intervention area holds a value. <see cref="InterventionAreas"/> is a list of area
        /// values, so grouping that one by plot drops the regions carrying a plot and no area
        /// and then calls what is left the plot's regions. This is what the per plot section
        /// groups, so its counts agree with the per type table above it.
        /// </summary>
        public IReadOnlyList<MeasuredValue> RegionsCarryingAPlot { get; }

        /// <summary>
        /// How many regions carry PRX_Ref Plot ID with a value, how many carry it with nothing
        /// in it, and how many do not carry it at all. The last two both print as an empty
        /// plot in a row and only one of them is a value somebody forgot to type, so the
        /// report counts them apart. These are counts rather than the length of
        /// <see cref="RegionsCarryingAPlot"/>, the same way the intervention area count comes
        /// off the tally rather than off the examples printed under it.
        /// </summary>
        public int PlotWithValue { get; }

        public int PlotCarriedBlank { get; }

        public int PlotNotCarried { get; }

        public ParameterTally InterventionTally
        {
            get
            {
                return RegionParameters.FirstOrDefault(
                    tally => string.Equals(tally.Name, KpiNames.InterventionArea, StringComparison.Ordinal));
            }
        }

        public bool HoldsInterventionArea
        {
            get { return InterventionTally != null; }
        }

        public IReadOnlyList<string> InterventionNearMisses
        {
            get
            {
                return RegionParameters
                    .Select(tally => tally.Name)
                    .Where(name => KpiNames.HoldsAny(name, KpiNames.InterventionNearMisses))
                    .OrderBy(name => name, NaturalOrder.Comparer)
                    .ToList();
            }
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
