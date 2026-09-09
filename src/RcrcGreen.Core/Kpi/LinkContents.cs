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
            IEnumerable<MeasuredValue> interventionAreas)
        {
            if (linkName == null) throw new ArgumentNullException("linkName");
            if (filledRegionCount < 0) throw new ArgumentOutOfRangeException("filledRegionCount");

            LinkName = linkName;
            DocumentTitle = documentTitle ?? string.Empty;
            FilledRegionCount = filledRegionCount;
            TypeCounts = Held(typeCounts);
            ViewCounts = Held(viewCounts);
            FirstRegions = Held(firstRegions);
            RegionParameters = Held(regionParameters);
            InterventionAreas = Held(interventionAreas);
        }

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
