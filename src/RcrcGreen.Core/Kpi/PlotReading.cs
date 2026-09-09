using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One species row as the softscape schedule printed it, with the group row it sat under.
    ///
    /// The group travels with the row because a species is not unique in the schedule.
    /// ALBIZIA LEBBECK on DM-12 is 1 under Existing and 13 under Proposed, and a row that
    /// arrives without its group collapses those two numbers into one in the wrong sheet.
    /// </summary>
    public sealed class SpeciesRow
    {
        public SpeciesRow(string botanicalName, string groupName, int quantity, string plotId = null)
        {
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");
            if (quantity < 0) throw new ArgumentOutOfRangeException("quantity");

            BotanicalName = botanicalName;
            GroupName = groupName ?? string.Empty;
            Quantity = quantity;
            PlotId = plotId ?? string.Empty;
        }

        public string BotanicalName { get; }

        /// <summary>
        /// Empty when the row sat under no group row at all. Such a row is reported and never
        /// assumed into a group, because guessing puts a count in the wrong sheet.
        /// </summary>
        public string GroupName { get; }

        public int Quantity { get; }

        public string PlotId { get; }

        public bool HasGroup
        {
            get { return GroupName.Length > 0; }
        }
    }

    /// <summary>
    /// One filled region in the 00 link that could be a plot's intervention area, with the
    /// area it holds. Which of a plot's two regions carries the area varies by plot, so both
    /// are offered and the type name decides nothing.
    /// </summary>
    public sealed class RegionArea
    {
        public RegionArea(string typeName, double rawSquareFeet, double squareMetres, string printed)
        {
            if (typeName == null) throw new ArgumentNullException("typeName");

            TypeName = typeName;
            RawSquareFeet = rawSquareFeet;
            SquareMetres = squareMetres;
            Printed = printed ?? string.Empty;
        }

        public string TypeName { get; }

        /// <summary>
        /// What Revit holds, square feet whatever the project displays. The identical area
        /// check compares this rather than the rounded metres, because MM-03 and MM-04 read
        /// 12182.05561411 alike to eight decimals and rounding would hide a narrower match.
        /// </summary>
        public double RawSquareFeet { get; }

        public double SquareMetres { get; }

        public string Printed { get; }

        public bool HoldsAnArea
        {
            get { return SquareMetres > 0.0; }
        }
    }

    /// <summary>
    /// One group subtotal off a schedule that prints in groups, exactly as the row read.
    /// The workbook wants the two group subtotals of SHRUBS AND LAWN and never the total.
    ///
    /// **The subtotal prints twice.** DM-11 gives 35 then 35 for GRASS and 70 then 70 for
    /// SHRUBS &amp; GROUND COVER, so adding a group's subtotal rows gives double. One is taken.
    /// Two that disagree are not a number to pick between, so the disagreement is carried here
    /// and refuses the write.
    /// </summary>
    public sealed class GroupSubtotal
    {
        public GroupSubtotal(
            string heading,
            double squareMetres,
            int itemCount,
            int repeats = 1,
            double speciesSum = double.NaN,
            string disagreement = null)
        {
            if (heading == null) throw new ArgumentNullException("heading");
            if (repeats < 0) throw new ArgumentOutOfRangeException("repeats");

            Heading = heading;
            SquareMetres = squareMetres;
            ItemCount = itemCount;
            Repeats = repeats;
            SpeciesSum = speciesSum;
            Disagreement = disagreement ?? string.Empty;
        }

        public string Heading { get; }

        public double SquareMetres { get; }

        public int ItemCount { get; }

        /// <summary>
        /// How many subtotal rows the group printed. Two on the measured model.
        /// </summary>
        public int Repeats { get; }

        /// <summary>
        /// The species rows of the group added up, or NaN when the schedule named no botanical
        /// column to find them in. DM-11's shrubs are 36 and 34 against a subtotal of 70, so
        /// the two can be held against each other. It is printed rather than enforced, because
        /// every one of those numbers is already rounded to the metre on the way out of Revit
        /// and a sum of rounded numbers need not equal a rounded sum.
        /// </summary>
        public double SpeciesSum { get; }

        /// <summary>
        /// Empty unless the repeated subtotal rows disagreed with each other.
        /// </summary>
        public string Disagreement { get; }

        public bool Agrees
        {
            get { return Disagreement.Length == 0; }
        }
    }

    /// <summary>
    /// Everything one chosen plot contributed, read once, in plain values. The Revit side
    /// builds one of these per plot and Core merges them afterwards. Nothing is ever read
    /// across plots in one go, because every schedule and every filled region in this model is
    /// per plot and filtered on PRX_Ref Plot ID.
    ///
    /// A read that did not happen is a false on the flag rather than an absent number, so a
    /// plot with no softscape schedule is a named plot in the reconciliation rather than a
    /// zero nobody notices.
    /// </summary>
    public sealed class PlotReading
    {
        public PlotReading(
            string plotId,
            string component,
            string reference,
            bool softscapeRead,
            IEnumerable<SpeciesRow> species,
            bool shrubsAndLawnRead,
            IEnumerable<GroupSubtotal> subtotals,
            IEnumerable<RegionArea> regions,
            string chosenRegionTypeName = null,
            IEnumerable<string> notes = null)
        {
            if (plotId == null) throw new ArgumentNullException("plotId");

            PlotId = plotId;
            Component = component ?? string.Empty;
            Reference = reference ?? string.Empty;
            SoftscapeRead = softscapeRead;
            Species = Held(species);
            ShrubsAndLawnRead = shrubsAndLawnRead;
            Subtotals = Held(subtotals);
            Regions = Held(regions);
            ChosenRegionTypeName = chosenRegionTypeName ?? string.Empty;
            Notes = Held(notes);
        }

        public string PlotId { get; }

        public string Component { get; }

        public string Reference { get; }

        public bool SoftscapeRead { get; }

        public IReadOnlyList<SpeciesRow> Species { get; }

        public bool ShrubsAndLawnRead { get; }

        public IReadOnlyList<GroupSubtotal> Subtotals { get; }

        /// <summary>
        /// Every filled region carrying this plot, whatever its area. The chosen one is named
        /// separately so the report can trace a wrong number back to the choice that made it.
        /// </summary>
        public IReadOnlyList<RegionArea> Regions { get; }

        public string ChosenRegionTypeName { get; }

        public IReadOnlyList<string> Notes { get; }

        public RegionArea ChosenRegion
        {
            get
            {
                return Regions.FirstOrDefault(one =>
                    string.Equals(one.TypeName, ChosenRegionTypeName, StringComparison.Ordinal));
            }
        }

        public IReadOnlyList<RegionArea> RegionsHoldingAnArea
        {
            get { return Regions.Where(one => one.HoldsAnArea).ToList(); }
        }

        public GroupSubtotal SubtotalHeaded(string heading)
        {
            if (heading == null) return null;

            return Subtotals.FirstOrDefault(one =>
                string.Equals(one.Heading.Trim(), heading.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
