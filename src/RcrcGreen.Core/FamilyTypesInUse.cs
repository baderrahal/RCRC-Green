using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view family type and how many views of a view type were made with it.
    /// </summary>
    public sealed class FamilyTypeCount
    {
        public FamilyTypeCount(string familyTypeName, int views)
        {
            FamilyTypeName = familyTypeName ?? string.Empty;
            Views = views;
        }

        public string FamilyTypeName { get; }

        public int Views { get; }

        public override string ToString()
        {
            return (FamilyTypeName.Length == 0 ? "(none)" : FamilyTypeName) + " x " + Views;
        }
    }

    /// <summary>
    /// Every view family type in use for one view type, most used first.
    /// </summary>
    public sealed class FamilyTypesForViewType
    {
        public FamilyTypesForViewType(ViewType type, IReadOnlyList<FamilyTypeCount> counts)
        {
            if (type == null) throw new ArgumentNullException("type");

            Type = type;
            Counts = counts ?? new List<FamilyTypeCount>();
        }

        public ViewType Type { get; }

        public IReadOnlyList<FamilyTypeCount> Counts { get; }

        public int Views
        {
            get { return Counts.Sum(one => one.Views); }
        }

        /// <summary>
        /// The whole reason this exists. A view type built with one family type everywhere has
        /// an answer to copy. One built with several has not, and whichever sibling is picked
        /// decides what a new view gets.
        /// </summary>
        public bool Disagrees
        {
            get { return Counts.Count > 1; }
        }
    }

    /// <summary>
    /// Which view family type each view type is actually built with, across the whole model.
    ///
    /// Three (010) views were created in one run and came out with three different family
    /// types, because they were set up from three different siblings and each was copied
    /// faithfully:
    ///
    ///   DM-11-(010) Overall Plan       from PL-17  family type (200) General Arrangement Layout
    ///   DM-11-(010) Location Key Plan  from FM-05  family type (010) Key Location Plan
    ///   DM-11-(010) Overall Key Plan   from FM-05  family type (010) Key Plan
    ///
    /// The tool is not wrong there and neither is the sibling. The model has no rule, and
    /// nothing anywhere showed that. Picking the most common one would hide it behind a winner
    /// the team never chose, so this counts them and says nothing about which is right.
    /// </summary>
    public static class FamilyTypesInUse
    {
        /// <summary>
        /// Disagreeing view types first, because they are the ones somebody has to go and
        /// settle. Templates and names that do not parse are left out, the first because a
        /// template is not a view on a plot and the second because there is no view type to
        /// file it under.
        /// </summary>
        public static IReadOnlyList<FamilyTypesForViewType> Of(IEnumerable<ScannedView> views)
        {
            var byType = new Dictionary<ViewType, Dictionary<string, int>>();

            foreach (ScannedView view in (views ?? Enumerable.Empty<ScannedView>())
                .Where(one => one != null && !one.IsTemplate))
            {
                ParsedViewName parsed;
                if (!ViewNameParser.TryParse(view.Name, out parsed)) continue;

                Dictionary<string, int> counts;
                if (!byType.TryGetValue(parsed.Type, out counts))
                {
                    counts = new Dictionary<string, int>(StringComparer.Ordinal);
                    byType.Add(parsed.Type, counts);
                }

                int already;
                counts[view.FamilyTypeName] =
                    counts.TryGetValue(view.FamilyTypeName, out already) ? already + 1 : 1;
            }

            return byType
                .Select(pair => new FamilyTypesForViewType(pair.Key, Ordered(pair.Value)))
                .OrderByDescending(one => one.Disagrees)
                .ThenBy(one => one.Type.ToString(), NaturalOrder.Comparer)
                .ToList();
        }

        public static int Disagreeing(IEnumerable<FamilyTypesForViewType> all)
        {
            return (all ?? Enumerable.Empty<FamilyTypesForViewType>()).Count(one => one.Disagrees);
        }

        private static IReadOnlyList<FamilyTypeCount> Ordered(Dictionary<string, int> counts)
        {
            return counts
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key, NaturalOrder.Comparer)
                .Select(pair => new FamilyTypeCount(pair.Key, pair.Value))
                .ToList();
        }
    }
}
