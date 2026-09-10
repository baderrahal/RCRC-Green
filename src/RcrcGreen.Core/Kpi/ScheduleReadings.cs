using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What reading the number at the front of one printed cell came back with. Three answers
    /// rather than a bool, because a cell holding no number at all is ordinary, a heading or an
    /// empty cell, and a cell holding a number that cannot be read whole is not. 1,234 m2 used
    /// to read as 1, and a reader handed a bool could only skip a cell or take it.
    /// </summary>
    public sealed class CellNumberRead
    {
        private CellNumberRead(bool isNumber, double value, string refusal)
        {
            IsNumber = isNumber;
            Value = value;
            Refusal = refusal ?? string.Empty;
        }

        /// <summary>
        /// No number at the front of the cell. A heading, a name, a dash or nothing at all.
        /// </summary>
        public static readonly CellNumberRead Empty = new CellNumberRead(false, 0.0, string.Empty);

        public static CellNumberRead Number(double value)
        {
            return new CellNumberRead(true, value, string.Empty);
        }

        public static CellNumberRead Refused(string why)
        {
            if (string.IsNullOrWhiteSpace(why)) throw new ArgumentNullException("why");

            return new CellNumberRead(false, 0.0, why);
        }

        public bool IsNumber { get; }

        public double Value { get; }

        /// <summary>
        /// Why the cell was refused, naming what it held. Empty for a number and for an empty
        /// cell, which are the two ordinary answers.
        /// </summary>
        public string Refusal { get; }

        public bool IsRefused
        {
            get { return Refusal.Length > 0; }
        }

        public bool IsEmpty
        {
            get { return !IsNumber && !IsRefused; }
        }

        public bool IsWhole
        {
            get
            {
                return IsNumber
                    && Value >= 0.0
                    && Value <= int.MaxValue
                    && Math.Abs(Value - Math.Round(Value)) == 0.0;
            }
        }

        public int Whole
        {
            get
            {
                if (!IsWhole) throw new InvalidOperationException("The cell does not hold a whole number.");

                return (int)Math.Round(Value);
            }
        }
    }

    /// <summary>
    /// What reading a softscape schedule as printed handed back: the species rows, or every
    /// reason the read was refused, and never both.
    ///
    /// **A reader that cannot find its column says so rather than falling back to a position**,
    /// which is the rule in CLAUDE.md. Cell 0 is the image column, so a name read off it is a
    /// file name, and the last number in an eleven column row is L/DAY rather than a count.
    /// Both are plausible, so a workbook filled that way looks finished. The refusal carries
    /// which column the schedule named nothing for, and it reaches the report and the pane.
    ///
    /// The TOTAL row is read too, off the row whose first cell holds that word, so the species
    /// rows can be held against what the schedule says they add to. The first real workbook
    /// read 31 trees where the model held 39 and nothing in the tool could say so.
    /// </summary>
    public sealed class SoftscapeReading
    {
        private SoftscapeReading(
            IEnumerable<SpeciesRow> species,
            IEnumerable<string> refusals,
            bool totalRead,
            int total,
            int rowsPassedOver,
            int totalRow,
            IEnumerable<PrintedGroup> groups)
        {
            Species = (species ?? Enumerable.Empty<SpeciesRow>()).Where(one => one != null).ToList();
            Refusals = (refusals ?? Enumerable.Empty<string>()).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
            TotalRead = totalRead;
            Total = total;
            RowsPassedOver = rowsPassedOver;
            TotalRow = totalRow;
            Groups = (groups ?? Enumerable.Empty<PrintedGroup>()).Where(one => one != null).ToList();
        }

        /// <summary>
        /// A schedule whose rows were never read, or that printed none. Nothing to refuse and
        /// nothing to hand back.
        /// </summary>
        public static readonly SoftscapeReading Nothing = new SoftscapeReading(null, null, false, 0, 0, 0, null);

        public static SoftscapeReading Refused(IEnumerable<string> why)
        {
            var held = new SoftscapeReading(null, why, false, 0, 0, 0, null);
            if (held.Refusals.Count == 0) throw new ArgumentException("A refusal needs a reason.", "why");

            return held;
        }

        public static SoftscapeReading Of(
            IEnumerable<SpeciesRow> species,
            bool totalRead,
            int total,
            int rowsPassedOver,
            int totalRow = 0,
            IEnumerable<PrintedGroup> groups = null)
        {
            if (rowsPassedOver < 0) throw new ArgumentOutOfRangeException("rowsPassedOver");
            if (totalRow < 0) throw new ArgumentOutOfRangeException("totalRow");

            return new SoftscapeReading(species, null, totalRead, total, rowsPassedOver, totalRead ? totalRow : 0, groups);
        }

        public IReadOnlyList<SpeciesRow> Species { get; }

        public IReadOnlyList<string> Refusals { get; }

        public bool WasRead
        {
            get { return Refusals.Count == 0; }
        }

        /// <summary>
        /// True when a row whose first cell holds TOTAL was found and its count read whole.
        /// </summary>
        public bool TotalRead { get; }

        public int Total { get; }

        /// <summary>
        /// The printed row the TOTAL was read off, counting the heading row as 1, or nought.
        /// </summary>
        public int TotalRow { get; }

        /// <summary>
        /// Every group row the schedule printed, in order, with its rows and whether it was
        /// taken. <see cref="Species"/> holds the rows of the groups taken and nothing else.
        /// </summary>
        public IReadOnlyList<PrintedGroup> Groups { get; }

        public IReadOnlyList<SpeciesRow> LeftOut
        {
            get { return Groups.Where(one => !one.Counted).SelectMany(one => one.Species).ToList(); }
        }

        /// <summary>
        /// Rows carrying a count and no botanical name, which are the subtotal a group prints
        /// under its species. Counted rather than dropped, so a skip is written down.
        /// </summary>
        public int RowsPassedOver { get; }

        /// <summary>
        /// The species rows added up, which is adding printed numbers and allowed. Held against
        /// <see cref="Total"/> by the reconciliation, and a disagreement refuses the write.
        /// </summary>
        public int SpeciesSum
        {
            get { return Species.Sum(one => one.Quantity); }
        }
    }

    /// <summary>
    /// What reading a shrubs and lawn schedule as printed handed back: the group subtotals, or
    /// every reason the read was refused, and never both.
    /// </summary>
    public sealed class ShrubsAndLawnReading
    {
        private ShrubsAndLawnReading(IEnumerable<GroupSubtotal> subtotals, IEnumerable<string> refusals)
        {
            Subtotals = (subtotals ?? Enumerable.Empty<GroupSubtotal>()).Where(one => one != null).ToList();
            Refusals = (refusals ?? Enumerable.Empty<string>()).Where(one => !string.IsNullOrWhiteSpace(one)).ToList();
        }

        public static readonly ShrubsAndLawnReading Nothing = new ShrubsAndLawnReading(null, null);

        public static ShrubsAndLawnReading Refused(IEnumerable<string> why)
        {
            var held = new ShrubsAndLawnReading(null, why);
            if (held.Refusals.Count == 0) throw new ArgumentException("A refusal needs a reason.", "why");

            return held;
        }

        public static ShrubsAndLawnReading Of(IEnumerable<GroupSubtotal> subtotals)
        {
            return new ShrubsAndLawnReading(subtotals, null);
        }

        public IReadOnlyList<GroupSubtotal> Subtotals { get; }

        public IReadOnlyList<string> Refusals { get; }

        public bool WasRead
        {
            get { return Refusals.Count == 0; }
        }
    }
}
