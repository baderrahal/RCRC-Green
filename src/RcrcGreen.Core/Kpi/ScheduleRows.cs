using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The number at the front of a printed cell. A schedule prints an area as 35 m² and a
    /// count as 46, so the unit is taken off by reading only as far as the number goes rather
    /// than by stripping characters, which would turn 1.234 m² into something else.
    /// </summary>
    public static class CellNumber
    {
        public static bool In(string cell, out double value)
        {
            value = 0.0;
            if (string.IsNullOrWhiteSpace(cell)) return false;

            string text = cell.Trim();
            int at = 0;
            if (at < text.Length && (text[at] == '-' || text[at] == '+')) at++;

            int digits = 0;
            bool point = false;
            while (at < text.Length)
            {
                if (char.IsDigit(text[at]))
                {
                    digits++;
                    at++;
                    continue;
                }

                if (text[at] == '.' && !point)
                {
                    point = true;
                    at++;
                    continue;
                }

                break;
            }

            if (digits == 0) return false;

            return double.TryParse(
                text.Substring(0, at), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static bool WholeIn(string cell, out int value)
        {
            value = 0;

            double number;
            if (!In(cell, out number)) return false;
            if (number < 0 || number > int.MaxValue) return false;
            if (Math.Abs(number - Math.Round(number)) > 0.0) return false;

            value = (int)Math.Round(number);
            return true;
        }
    }

    /// <summary>
    /// The species rows of a softscape schedule, each with the group row it sat under.
    ///
    /// A row is a species when its first cell holds text and a later cell holds a whole
    /// number. That is what tells a species from the two rows around it: the category row
    /// TREES holds one cell and no number, and a subtotal holds a number and no name. The
    /// headings row holds two cells and its second is COUNT (n), which is not a number.
    ///
    /// A grand total row carrying its own word in the first cell would read as a species here.
    /// It has not been seen on this model and nothing guesses at its wording, so if one ever
    /// arrives it comes out as a species the workbook's list does not hold, named in the report
    /// with its count and written nowhere. That is visible, which a dropped row would not be.
    /// </summary>
    public static class SoftscapeRows
    {
        public static IReadOnlyList<SpeciesRow> SpeciesIn(
            ScannedSchedule schedule, IEnumerable<string> phaseNames, string plotId)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            var found = new List<SpeciesRow>();
            if (!schedule.RowsWereRead) return found;

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, phaseNames);
            var groupAt = new Dictionary<int, string>();
            foreach (ScheduleGroup group in groups) groupAt[group.RowIndex] = group.Name;

            string carrying = string.Empty;
            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();

            for (int index = 0; index < rows.Count; index++)
            {
                string named;
                if (groupAt.TryGetValue(index, out named))
                {
                    carrying = named;
                    continue;
                }

                IReadOnlyList<string> row = rows[index];
                if (row == null || row.Count == 0) continue;
                if (string.IsNullOrWhiteSpace(row[0])) continue;

                int quantity;
                if (!LastWholeNumberIn(row, out quantity)) continue;

                found.Add(new SpeciesRow(row[0].Trim(), carrying, quantity, plotId));
            }

            return found;
        }

        /// <summary>
        /// The count column is the last one on this schedule, so the last whole number in the
        /// row is the quantity. Taking the first would take a size or a code.
        /// </summary>
        private static bool LastWholeNumberIn(IReadOnlyList<string> row, out int quantity)
        {
            quantity = 0;
            bool any = false;

            for (int at = 1; at < row.Count; at++)
            {
                int number;
                if (!CellNumber.WholeIn(row[at], out number)) continue;

                quantity = number;
                any = true;
            }

            return any;
        }
    }

    /// <summary>
    /// The group subtotals of a shrubs and lawn schedule.
    ///
    /// This schedule prints its group heading and that group's numbers on ONE row, which is a
    /// different shape from the softscape schedule where the group row stands alone. DM-11
    /// prints GRASS 35 m² 46, then SHRUBS &amp; GROUND COVER 70 m² 58, then TOTAL 105 m² 104.
    /// Only the two headings the workbook asks for are looked for, so the schedule's own TOTAL
    /// is never a source and never has to be told apart from a group.
    /// </summary>
    public static class ShrubsAndLawnRows
    {
        public static IReadOnlyList<GroupSubtotal> SubtotalsIn(
            ScannedSchedule schedule, IEnumerable<string> headings)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            var wanted = (headings ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .ToList();

            var found = new List<GroupSubtotal>();
            if (!schedule.RowsWereRead || wanted.Count == 0) return found;

            foreach (IReadOnlyList<string> row in schedule.Rows)
            {
                if (row == null || row.Count == 0) continue;
                if (string.IsNullOrWhiteSpace(row[0])) continue;

                string heading = row[0].Trim();
                if (!wanted.Any(one => string.Equals(one.Trim(), heading, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                double area;
                if (!FirstNumberIn(row, out area)) continue;

                int items;
                LastWholeNumberIn(row, out items);

                found.Add(new GroupSubtotal(heading, area, items));
            }

            return found;
        }

        private static bool FirstNumberIn(IReadOnlyList<string> row, out double area)
        {
            for (int at = 1; at < row.Count; at++)
            {
                if (CellNumber.In(row[at], out area)) return true;
            }

            area = 0.0;
            return false;
        }

        private static bool LastWholeNumberIn(IReadOnlyList<string> row, out int items)
        {
            items = 0;
            bool any = false;

            for (int at = 1; at < row.Count; at++)
            {
                int number;
                if (!CellNumber.WholeIn(row[at], out number)) continue;

                items = number;
                any = true;
            }

            return any;
        }
    }
}
