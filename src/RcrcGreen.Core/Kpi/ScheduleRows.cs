using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The number at the front of a printed cell. A schedule prints an area as 35 m2 with its
    /// unit attached and a count as 46 with none, so the unit comes off by reading only as far
    /// as the number goes rather than by stripping characters, which would turn 1.234 m2 into
    /// something else.
    ///
    /// A real area can be zero. The hardscape schedule prints 0 m2, which reads as the number
    /// nought and not as a cell holding nothing.
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
    /// Which column of a printed schedule holds what, read off its own heading row.
    ///
    /// These schedules are eleven columns wide. Taking the first number in a row would take the
    /// area and taking the last would take L/DAY, so neither position can be assumed and the
    /// headings decide. A schedule whose headings name none of them says so by handing back
    /// nothing rather than by pointing at a column that happens to be there.
    /// </summary>
    public static class ScheduleColumns
    {
        public const string AreaWord = "AREA";

        public const string CountWord = "COUNT";

        public const string BotanicalWord = "BOTANIC";

        public static int Holding(IReadOnlyList<string> headings, string word)
        {
            if (headings == null) return -1;

            for (int at = 0; at < headings.Count; at++)
            {
                if (KpiNames.Holds(headings[at], word)) return at;
            }

            return -1;
        }

        public static string At(IReadOnlyList<string> row, int column)
        {
            if (row == null || column < 0 || column >= row.Count) return string.Empty;

            return row[column] ?? string.Empty;
        }

        /// <summary>
        /// A row carrying text in its first cell and nothing anywhere else. A group heading and
        /// a phase row are both this shape, and a row carrying numbers never is, which is what
        /// keeps the TOTAL row out.
        /// </summary>
        public static bool IsStructureRow(IReadOnlyList<string> row)
        {
            if (row == null || row.Count == 0) return false;
            if (string.IsNullOrWhiteSpace(row[0])) return false;

            for (int at = 1; at < row.Count; at++)
            {
                if (!string.IsNullOrWhiteSpace(row[at])) return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The species rows of a softscape schedule, each with the group row it sat under.
    ///
    /// The botanical name and the quantity come off the columns the heading row names, and fall
    /// back to the first cell and the last whole number only where it names neither. The real
    /// schedules are eleven columns wide with an image in the first, so a name read off cell
    /// nought there would be a file name.
    /// </summary>
    public static class SoftscapeRows
    {
        public static IReadOnlyList<SpeciesRow> SpeciesIn(
            ScannedSchedule schedule, IEnumerable<string> phaseNames, string plotId)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            var found = new List<SpeciesRow>();
            if (!schedule.RowsWereRead) return found;

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            if (rows.Count == 0) return found;

            int nameColumn = ScheduleColumns.Holding(rows[0], ScheduleColumns.BotanicalWord);
            int countColumn = ScheduleColumns.Holding(rows[0], ScheduleColumns.CountWord);

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, phaseNames);
            var groupAt = new Dictionary<int, string>();
            foreach (ScheduleGroup group in groups) groupAt[group.RowIndex] = group.Name;

            string carrying = string.Empty;

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

                string botanical = nameColumn >= 0
                    ? ScheduleColumns.At(row, nameColumn)
                    : (row.Count > 0 ? row[0] : string.Empty);
                if (string.IsNullOrWhiteSpace(botanical)) continue;

                int quantity;
                if (!QuantityIn(row, countColumn, out quantity)) continue;

                found.Add(new SpeciesRow(botanical.Trim(), carrying, quantity, plotId));
            }

            return found;
        }

        private static bool QuantityIn(IReadOnlyList<string> row, int countColumn, out int quantity)
        {
            if (countColumn >= 0)
            {
                return CellNumber.WholeIn(ScheduleColumns.At(row, countColumn), out quantity);
            }

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
    /// The shape is measured, off the 1355 scan report, which is not in this repository because
    /// nothing under reports/ is ever committed. DM-11-(600) SHRUBS &amp; LAWN SCHEDULE prints
    /// eleven columns and this:
    ///
    /// <code>
    /// IMAGE | # | PLANT CODE | BOTANICAL NAME | AREA (sqm) | COUNT (n) | ...
    /// GRASS                                                                   group heading
    /// Proposed                                                                phase
    /// Pennisetum Setaceum.jpg | PEN SET | ... | 35 m2 | 46 | ...              species
    ///                                         | 35 m2 | 46 | ...              subtotal
    ///                                         | 35 m2 | 46 | ...              subtotal AGAIN
    /// SHRUBS &amp; GROUND COVER                                                   group heading
    /// ...
    /// TOTAL                                   | 105 m2 | 104 | ...            the whole thing
    /// </code>
    ///
    /// Three things follow. The heading is on its own row rather than beside its numbers. A
    /// phase row sits under it, the same shape the softscape schedule uses. And **the subtotal
    /// prints twice**, so adding a group's subtotal rows gives double: one is taken, and two
    /// that disagree are named rather than chosen between.
    ///
    /// TOTAL needs no special case. It carries numbers, so it is not a structure row, and its
    /// first cell holds text, so it is not a subtotal row.
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

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            if (rows.Count == 0) return found;

            int areaColumn = ScheduleColumns.Holding(rows[0], ScheduleColumns.AreaWord);
            int countColumn = ScheduleColumns.Holding(rows[0], ScheduleColumns.CountWord);
            int nameColumn = ScheduleColumns.Holding(rows[0], ScheduleColumns.BotanicalWord);
            if (areaColumn < 0) return found;

            string heading = null;
            var subtotals = new List<GroupSubtotal>();
            var species = new List<double>();

            for (int index = 1; index < rows.Count; index++)
            {
                IReadOnlyList<string> row = rows[index];

                if (ScheduleColumns.IsStructureRow(row))
                {
                    string text = row[0].Trim();
                    if (!wanted.Any(one => string.Equals(one.Trim(), text, StringComparison.OrdinalIgnoreCase)))
                    {
                        // A phase row inside the group. It names no wanted heading, so it does
                        // not open or close one.
                        continue;
                    }

                    Close(found, heading, subtotals, species);
                    heading = text;
                    subtotals = new List<GroupSubtotal>();
                    species = new List<double>();
                    continue;
                }

                if (heading == null) continue;

                double area;
                if (!CellNumber.In(ScheduleColumns.At(row, areaColumn), out area)) continue;

                int items;
                CellNumber.WholeIn(ScheduleColumns.At(row, countColumn), out items);

                if (string.IsNullOrWhiteSpace(ScheduleColumns.At(row, 0)))
                {
                    subtotals.Add(new GroupSubtotal(heading, area, items));
                    continue;
                }

                if (nameColumn >= 0 && !string.IsNullOrWhiteSpace(ScheduleColumns.At(row, nameColumn)))
                {
                    species.Add(area);
                }
            }

            Close(found, heading, subtotals, species);
            return found;
        }

        private static void Close(
            List<GroupSubtotal> found, string heading, List<GroupSubtotal> subtotals, List<double> species)
        {
            if (heading == null || subtotals.Count == 0) return;

            GroupSubtotal first = subtotals[0];
            double sum = species.Count == 0 ? double.NaN : species.Sum();

            List<GroupSubtotal> differing = subtotals
                .Where(one => one.SquareMetres != first.SquareMetres || one.ItemCount != first.ItemCount)
                .ToList();

            string disagreement = differing.Count == 0
                ? string.Empty
                : "its " + subtotals.Count + " subtotal rows disagree: " + string.Join(", ", subtotals
                    .Select(one => Said(one.SquareMetres) + " over " + one.ItemCount).ToArray());

            found.Add(new GroupSubtotal(
                heading, first.SquareMetres, first.ItemCount, subtotals.Count, sum, disagreement));
        }

        private static string Said(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
