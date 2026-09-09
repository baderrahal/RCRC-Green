using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One species and how many of it, as the tree list sheets want them: the botanical name
    /// matched against column D and the quantity written into column B.
    /// </summary>
    public sealed class SpeciesCount
    {
        public SpeciesCount(string botanicalName, int quantity)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");
            if (quantity < 0) throw new ArgumentOutOfRangeException("quantity");

            BotanicalName = botanicalName;
            Quantity = quantity;
        }

        public string BotanicalName { get; }

        public int Quantity { get; }
    }

    /// <summary>
    /// Everything one fill writes: the six values and the two species lists.
    ///
    /// This is the seam the next round plugs into. Nothing in this round constructs one,
    /// because the six values arrive from the model once the scanner has run on the real
    /// file and answered where each of them lives. The three areas are square metres, which
    /// is what the workbook's own formulas take.
    /// </summary>
    public sealed class KpiFillValues
    {
        public KpiFillValues(
            string component,
            string reference,
            string location,
            double areaSquareMetres,
            double shrubsSquareMetres,
            double lawnSquareMetres,
            IEnumerable<SpeciesCount> existingTrees,
            IEnumerable<SpeciesCount> proposedTrees)
        {
            Component = component ?? string.Empty;
            Reference = reference ?? string.Empty;
            Location = location ?? string.Empty;
            AreaSquareMetres = areaSquareMetres;
            ShrubsSquareMetres = shrubsSquareMetres;
            LawnSquareMetres = lawnSquareMetres;
            ExistingTrees = Held(existingTrees);
            ProposedTrees = Held(proposedTrees);
        }

        public string Component { get; }

        public string Reference { get; }

        public string Location { get; }

        public double AreaSquareMetres { get; }

        public double ShrubsSquareMetres { get; }

        public double LawnSquareMetres { get; }

        public IReadOnlyList<SpeciesCount> ExistingTrees { get; }

        public IReadOnlyList<SpeciesCount> ProposedTrees { get; }

        private static IReadOnlyList<SpeciesCount> Held(IEnumerable<SpeciesCount> items)
        {
            if (items == null) return new List<SpeciesCount>();
            return items.Where(item => item != null).ToList();
        }
    }
}
