using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The elements one schedule lists, read from the model rather than off the schedule, so
    /// the parameters a tree really carries are visible whatever the schedule chose to show.
    /// </summary>
    public sealed class ScheduleElements
    {
        public ScheduleElements(
            string scheduleName,
            int elementCount,
            IEnumerable<NameCount> categories,
            IEnumerable<NameCount> familyTypes,
            IEnumerable<ParameterTally> instanceParameters,
            IEnumerable<ParameterTally> typeParameters,
            IEnumerable<NameCount> createdPhases,
            IEnumerable<NameCount> demolishedPhases,
            IEnumerable<NameCount> worksets,
            IEnumerable<NameCount> designOptions,
            IEnumerable<ParameterValueCount> wordValues)
        {
            if (scheduleName == null) throw new ArgumentNullException("scheduleName");
            if (elementCount < 0) throw new ArgumentOutOfRangeException("elementCount");

            ScheduleName = scheduleName;
            ElementCount = elementCount;
            Categories = Held(categories);
            FamilyTypes = Held(familyTypes);
            InstanceParameters = Held(instanceParameters);
            TypeParameters = Held(typeParameters);
            CreatedPhases = Held(createdPhases);
            DemolishedPhases = Held(demolishedPhases);
            Worksets = Held(worksets);
            DesignOptions = Held(designOptions);
            WordValues = Held(wordValues);
        }

        public string ScheduleName { get; }

        public int ElementCount { get; }

        public IReadOnlyList<NameCount> Categories { get; }

        /// <summary>
        /// Family and type together, one line per distinct pair, which on a planting schedule
        /// is one line per kind of tree.
        /// </summary>
        public IReadOnlyList<NameCount> FamilyTypes { get; }

        public IReadOnlyList<ParameterTally> InstanceParameters { get; }

        public IReadOnlyList<ParameterTally> TypeParameters { get; }

        public IReadOnlyList<NameCount> CreatedPhases { get; }

        public IReadOnlyList<NameCount> DemolishedPhases { get; }

        public IReadOnlyList<NameCount> Worksets { get; }

        public IReadOnlyList<NameCount> DesignOptions { get; }

        /// <summary>
        /// The distinct values of every parameter whose name holds one of the planting words
        /// or one of the status words, with counts. This is what answers which field is the
        /// botanical name and what separates existing from proposed, by showing rather than
        /// deciding.
        /// </summary>
        public IReadOnlyList<ParameterValueCount> WordValues { get; }

        public IEnumerable<string> ParameterNamesHolding(params string[] words)
        {
            return InstanceParameters.Select(tally => tally.Name)
                .Concat(TypeParameters.Select(tally => tally.Name))
                .Where(name => KpiNames.HoldsAny(name, words))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(name => name, NaturalOrder.Comparer);
        }

        public IEnumerable<ParameterValueCount> ValuesOfNamesHolding(params string[] words)
        {
            return WordValues.Where(one => KpiNames.HoldsAny(one.ParameterName, words));
        }

        /// <summary>
        /// The values of one name on one side, instances or types, so the two are never summed
        /// under one heading.
        /// </summary>
        public IReadOnlyList<ParameterValueCount> ValuesOf(string parameterName, bool onType)
        {
            return WordValues
                .Where(one => one.OnType == onType
                    && string.Equals(one.ParameterName, parameterName, StringComparison.Ordinal))
                .ToList();
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
