using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which of a plot's filled regions carries its intervention area.
    ///
    /// **Every plot has two filled regions in the 00 link**, one CADASTRAL LIMIT and one OUT OF
    /// SCOPE (PRESENTATION), and which of them holds the area varies by plot. DM-11, DM-12 and
    /// DM-13 hold it on OUT OF SCOPE with cadastral at 0, and NS-19 and NS-06 hold it on
    /// cadastral. **The type name cannot decide it**, so nothing here reads one.
    ///
    /// **This rule lived in two places**, once in the Revit handler and once in the test
    /// fixture, and the two were not the same rule. The fixture took the FIRST region whatever
    /// it held and however many there were, so every reconciliation and merge test was written
    /// against a choice the running tool would not have made. It is here now, the handler and
    /// the fixture both call it, and the tests reach the one that runs.
    /// </summary>
    public static class RegionChoice
    {
        /// <summary>
        /// The type name of the region to take the area from, or empty when nobody can say.
        ///
        /// A person's pick wins outright, because it is an answer and this is only a guess at
        /// one. Otherwise **exactly one region holding an area answers itself**. Two of them is
        /// a question the type name cannot settle, so it stays unchosen and the reconciliation
        /// refuses until a person picks. None of them is the same: there is nothing to choose.
        /// </summary>
        public static string For(IEnumerable<RegionArea> regions, string chosenByHand)
        {
            if (!string.IsNullOrEmpty(chosenByHand)) return chosenByHand;

            if (regions == null) return string.Empty;

            List<RegionArea> holding = regions
                .Where(one => one != null && one.HoldsAnArea)
                .ToList();

            return holding.Count == 1 ? holding[0].TypeName : string.Empty;
        }

        /// <summary>
        /// Why nothing was chosen, for a report that would otherwise print an empty cell and
        /// leave somebody guessing which of the two cases it was.
        /// </summary>
        public static string WhyUnchosen(IEnumerable<RegionArea> regions)
        {
            List<RegionArea> held = (regions ?? Enumerable.Empty<RegionArea>())
                .Where(one => one != null)
                .ToList();

            List<RegionArea> holding = held.Where(one => one.HoldsAnArea).ToList();

            if (held.Count == 0) return "the plot has no filled region in the link";
            if (holding.Count == 0) return "no filled region on this plot holds an area";
            if (holding.Count > 1)
            {
                return Count(holding.Count) + " hold an area, "
                    + string.Join(" and ", holding.Select(one => one.TypeName).ToArray())
                    + ", and the type name cannot say which is the plot's";
            }

            return string.Empty;
        }

        private static string Count(int many)
        {
            return many + " filled regions";
        }
    }
}
