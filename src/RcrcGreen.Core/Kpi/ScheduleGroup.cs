using System;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One group row in a schedule as printed, and what sits under it.
    ///
    /// A grouped Revit schedule prints a row holding only the group's value, then the rows in
    /// that group, then a subtotal. In the softscape schedule the grouping is the phase, so
    /// these rows are the only thing in the model that separates an existing tree from a
    /// proposed one. The elements cannot say it: the schedule lists RVT Link instances,
    /// because the plants live in the linked component models, so every one of them comes back
    /// on the same phase.
    /// </summary>
    public sealed class ScheduleGroup
    {
        public ScheduleGroup(string name, int rowIndex, int rowsUnder, int namedRowsUnder)
            : this(name, rowIndex, rowsUnder, namedRowsUnder, true, null)
        {
        }

        public ScheduleGroup(
            string name,
            int rowIndex,
            int rowsUnder,
            int namedRowsUnder,
            bool namedRowsCounted,
            string whyNotCounted)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (rowIndex < 0) throw new ArgumentOutOfRangeException("rowIndex");
            if (rowsUnder < 0) throw new ArgumentOutOfRangeException("rowsUnder");
            if (namedRowsUnder < 0) throw new ArgumentOutOfRangeException("namedRowsUnder");
            if (namedRowsUnder > rowsUnder) throw new ArgumentOutOfRangeException("namedRowsUnder");
            if (!namedRowsCounted && namedRowsUnder != 0) throw new ArgumentOutOfRangeException("namedRowsUnder");
            if (!namedRowsCounted && string.IsNullOrWhiteSpace(whyNotCounted))
            {
                throw new ArgumentNullException("whyNotCounted");
            }

            Name = name;
            RowIndex = rowIndex;
            RowsUnder = rowsUnder;
            NamedRowsUnder = namedRowsUnder;
            NamedRowsCounted = namedRowsCounted;
            WhyNotCounted = namedRowsCounted ? string.Empty : whyNotCounted;
        }

        /// <summary>
        /// The text the group row carries, exactly as the schedule printed it.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Where the row sits in the rows that were read, counted from nothing, so the report
        /// can point at it rather than describe it.
        /// </summary>
        public int RowIndex { get; }

        /// <summary>
        /// Every row between this group row and the next one, or the end. A subtotal row is
        /// among them where the schedule prints one.
        /// </summary>
        public int RowsUnder { get; }

        /// <summary>
        /// Those of them whose botanical column holds text, which is a species in the
        /// softscape schedule. A subtotal leaves that cell empty and is counted only in
        /// <see cref="RowsUnder"/>. Nought when the count was refused, and
        /// <see cref="NamedRowsCounted"/> says which nought that is.
        /// </summary>
        public int NamedRowsUnder { get; }

        /// <summary>
        /// False when the heading row named no botanical column, so nothing was counted off a
        /// cell that happened to be there. A nought with this false is a refused count and not
        /// a group with no species under it.
        /// </summary>
        public bool NamedRowsCounted { get; }

        public string WhyNotCounted { get; }
    }
}
