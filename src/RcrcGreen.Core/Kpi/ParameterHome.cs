using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every parameter name found on one kind of element, with a tally each, and the values of
    /// the names the KPI workbook asks for.
    ///
    /// There are three of these: title block instances, title block types and the sheets
    /// themselves. Whether PRX_COMPONENT lives on the title block or on the sheet is one of the
    /// nine questions, and Sheet Width turned out to live on the instance and not the type, so
    /// all three are read rather than the likely one.
    /// </summary>
    public sealed class ParameterHome
    {
        public ParameterHome(
            string where,
            int elementCount,
            IEnumerable<ParameterTally> tallies,
            IEnumerable<SheetValue> values)
        {
            if (where == null) throw new ArgumentNullException("where");
            if (elementCount < 0) throw new ArgumentOutOfRangeException("elementCount");

            Where = where;
            ElementCount = elementCount;
            Tallies = Held(tallies);
            Values = Held(values);
        }

        /// <summary>
        /// The kind of element, in words, as the report prints it.
        /// </summary>
        public string Where { get; }

        public int ElementCount { get; }

        public IReadOnlyList<ParameterTally> Tallies { get; }

        /// <summary>
        /// One entry per element carrying one of the wanted names, whatever it holds. The
        /// report caps how many it prints and says how many there were.
        /// </summary>
        public IReadOnlyList<SheetValue> Values { get; }

        public ParameterTally TallyFor(string exactName)
        {
            return Tallies.FirstOrDefault(tally => string.Equals(tally.Name, exactName, StringComparison.Ordinal));
        }

        public bool Holds(string exactName)
        {
            return TallyFor(exactName) != null;
        }

        public IReadOnlyList<SheetValue> ValuesOf(string exactName)
        {
            return Values
                .Where(value => string.Equals(value.ParameterName, exactName, StringComparison.Ordinal))
                .ToList();
        }

        /// <summary>
        /// Names holding any of the words, compared without case, so a near miss such as
        /// PRX_Component or PRX_PLOT_UID is visible next to the NOT FOUND it explains.
        /// </summary>
        public IReadOnlyList<string> NamesHolding(params string[] words)
        {
            return Tallies
                .Select(tally => tally.Name)
                .Where(name => KpiNames.HoldsAny(name, words))
                .OrderBy(name => name, NaturalOrder.Comparer)
                .ToList();
        }

        public static ParameterHome Empty(string where)
        {
            return new ParameterHome(where, 0, null, null);
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
