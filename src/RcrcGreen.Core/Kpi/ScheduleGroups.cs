using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Finds the group rows in a schedule as printed.
    ///
    /// A group row and a subtotal row look the same: one cell with text in it and the rest
    /// empty. What tells them apart is the text. A group row carries a value the model really
    /// holds, so the phase names read off the document decide it, and nothing here matches on
    /// the shape of a row or on the words Existing and Proposed. A model whose phases are
    /// named something else is read the same way.
    /// </summary>
    public static class ScheduleGroups
    {
        /// <summary>
        /// Every row of the schedule that names one of <paramref name="names"/> and nothing
        /// else, in the order they print, with what follows each one.
        /// </summary>
        public static IReadOnlyList<ScheduleGroup> Of(ScannedSchedule schedule, IEnumerable<string> names)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            var wanted = new List<string>(names ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            var found = new List<ScheduleGroup>();
            if (!schedule.RowsWereRead || wanted.Count == 0) return found;

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            var at = new List<int>();

            for (int index = 0; index < rows.Count; index++)
            {
                if (GroupNameIn(rows[index], wanted) != null) at.Add(index);
            }

            for (int which = 0; which < at.Count; which++)
            {
                int index = at[which];
                int ends = which + 1 < at.Count ? at[which + 1] : rows.Count;
                int under = ends - index - 1;
                int named = 0;

                for (int row = index + 1; row < ends; row++)
                {
                    if (FirstCellHoldsText(rows[row])) named++;
                }

                found.Add(new ScheduleGroup(GroupNameIn(rows[index], wanted), index, under, named));
            }

            return found;
        }

        /// <summary>
        /// The name a row is a group row for, or null. One cell with text in it, and that text
        /// one of the wanted names once the spaces are off, compared without case so a phase
        /// printed EXISTING is the phase named Existing.
        /// </summary>
        private static string GroupNameIn(IReadOnlyList<string> row, List<string> wanted)
        {
            if (row == null) return null;

            string only = null;
            foreach (string cell in row)
            {
                if (string.IsNullOrWhiteSpace(cell)) continue;
                if (only != null) return null;
                only = cell.Trim();
            }

            if (only == null) return null;

            return wanted.Any(name => string.Equals(name.Trim(), only, StringComparison.OrdinalIgnoreCase))
                ? only
                : null;
        }

        private static bool FirstCellHoldsText(IReadOnlyList<string> row)
        {
            return row != null && row.Count > 0 && !string.IsNullOrWhiteSpace(row[0]);
        }
    }
}
