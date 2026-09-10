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
    /// **A DIGIT AFTER THE NUMBER ENDS IS A REFUSAL, NEVER A SHORTER NUMBER.** Reading as far as
    /// the number goes turned 1,234 m2 into 1. Every value this reader had met printed under a
    /// thousand, and the two four figure values ever seen came off the one project, whose unit
    /// format prints no separator. Which character a project groups digits with, or uses for
    /// the decimal, is a units setting this tool has never read, so nothing here parses a
    /// separator: a cell holding a digit past where the number ends is refused with the cell
    /// named, and 1131,72 is refused the same way. A number read short defeats the checks built
    /// on it, because 1 plus 1 equals 2 whether the rows really read 1,200, 1,300 and 2,500 or
    /// not. Revit prints the area unit as m with a superscript two, which is not a digit, so
    /// the ordinary cell passes and a unit spelt m2 does not.
    ///
    /// A real area can be zero. The hardscape schedule prints 0 m2, which reads as the number
    /// nought and not as a cell holding nothing.
    /// </summary>
    public static class CellNumber
    {
        public static CellNumberRead Read(string cell)
        {
            if (string.IsNullOrWhiteSpace(cell)) return CellNumberRead.Empty;

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

            if (digits == 0) return CellNumberRead.Empty;

            string front = text.Substring(0, at);
            string rest = text.Substring(at);

            if (rest.Any(char.IsDigit))
            {
                return CellNumberRead.Refused("holds " + Quoted(text) + ", which carries a digit after "
                    + "the number " + front + " ends. That is a thousands separator or a decimal comma, "
                    + "this tool does not read the project's separator setting, and a number read short "
                    + "passes its own checks, so the cell is refused rather than read as " + front + ".");
            }

            double value;
            if (!double.TryParse(front, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                return CellNumberRead.Refused("holds " + Quoted(text) + " and " + front
                    + " did not read as a number.");
            }

            return CellNumberRead.Number(value);
        }

        private static string Quoted(string text)
        {
            return "'" + text + "'";
        }
    }

    /// <summary>
    /// Which column of a printed schedule holds what, read off its own heading row.
    ///
    /// These schedules are eleven columns wide. Taking the first number in a row would take the
    /// area and taking the last would take L/DAY, so neither position can be assumed and the
    /// headings decide. **A schedule whose headings name none of them is refused, with the
    /// column named**, never read off a position that happens to be there. Cell 0 is the image
    /// column, so a name read off it is a file name.
    /// </summary>
    public static class ScheduleColumns
    {
        public const string AreaWord = "AREA";

        public const string CountWord = "COUNT";

        public const string BotanicalWord = "BOTANIC";

        /// <summary>
        /// HEIGHT (m) and DIAMETER (m) on the real softscape schedule, which are the workbook's
        /// Mature Height and Average Mature Canopy Diameter, measured identical on six species
        /// sitting in both. Optional: a schedule naming neither still counts its species and the
        /// report says the measure was not there.
        /// </summary>
        public const string HeightWord = "HEIGHT";

        public const string DiameterWord = "DIAMETER";

        public static int Holding(IReadOnlyList<string> headings, string word)
        {
            if (headings == null) return -1;

            for (int at = 0; at < headings.Count; at++)
            {
                if (KpiNames.Holds(headings[at], word)) return at;
            }

            return -1;
        }

        /// <summary>
        /// The refusal every reader gives for a column its heading row does not name, one
        /// sentence for all of them, with the headings printed so the person reading it can see
        /// what the schedule does call its columns.
        /// </summary>
        public static string NothingNamed(IReadOnlyList<string> headings, string word)
        {
            var named = (headings ?? new List<string>())
                .Select(one => string.IsNullOrWhiteSpace(one) ? "-" : one.Trim())
                .ToList();

            return "the heading row names no column holding " + word + ". Its headings: "
                + (named.Count == 0 ? "(none)" : string.Join(" | ", named.ToArray()));
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
    /// The species rows of a softscape schedule, each with the group row it sat under, and the
    /// TOTAL row held beside them.
    ///
    /// The botanical name and the quantity come off the columns the heading row names and off
    /// nothing else. **A schedule naming neither is refused with the column named.** It used to
    /// fall back to the first cell and the last whole number, and the real schedules are eleven
    /// columns wide with an image in the first, so the name would have been a file name and the
    /// count L/DAY.
    ///
    /// Nothing is dropped in silence. A row with a name and no whole count refuses the read and
    /// names the row. A row with a count and no name is a subtotal and is counted as passed
    /// over. The TOTAL row is read off the row whose first cell holds that word, so the species
    /// rows can be held against what the schedule says they add to.
    /// </summary>
    public static class SoftscapeRows
    {
        public const string TotalMark = "TOTAL";

        public static SoftscapeReading Read(
            ScannedSchedule schedule, IEnumerable<string> phaseNames, string plotId)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            if (!schedule.RowsWereRead) return SoftscapeReading.Nothing;

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            if (rows.Count == 0) return SoftscapeReading.Nothing;

            IReadOnlyList<string> headings = rows[0];
            int nameColumn = ScheduleColumns.Holding(headings, ScheduleColumns.BotanicalWord);
            int countColumn = ScheduleColumns.Holding(headings, ScheduleColumns.CountWord);

            var refusals = new List<string>();
            if (nameColumn < 0) refusals.Add(ScheduleColumns.NothingNamed(headings, ScheduleColumns.BotanicalWord));
            if (countColumn < 0) refusals.Add(ScheduleColumns.NothingNamed(headings, ScheduleColumns.CountWord));
            if (refusals.Count > 0) return SoftscapeReading.Refused(refusals);

            string countHeading = (headings[countColumn] ?? string.Empty).Trim();
            int heightColumn = ScheduleColumns.Holding(headings, ScheduleColumns.HeightWord);
            int diameterColumn = ScheduleColumns.Holding(headings, ScheduleColumns.DiameterWord);

            IReadOnlyList<ScheduleGroup> groups = ScheduleGroups.Of(schedule, phaseNames);
            var groupAt = new Dictionary<int, string>();
            foreach (ScheduleGroup group in groups) groupAt[group.RowIndex] = group.Name;

            var found = new List<SpeciesRow>();
            string carrying = string.Empty;
            bool totalRead = false;
            int total = 0;
            int totalRow = 0;
            int passedOver = 0;

            for (int index = 1; index < rows.Count; index++)
            {
                string named;
                if (groupAt.TryGetValue(index, out named))
                {
                    carrying = named;
                    continue;
                }

                IReadOnlyList<string> row = rows[index];
                if (row == null || row.Count == 0) continue;

                string countCell = ScheduleColumns.At(row, countColumn);
                CellNumberRead count = CellNumber.Read(countCell);

                // The TOTAL row is read before the shape of the row is looked at, because a
                // TOTAL row whose count is empty is one cell of text and nothing else, which is
                // the shape of a heading, and it is a refusal rather than a heading.
                if (string.Equals(ScheduleColumns.At(row, 0).Trim(), TotalMark, StringComparison.OrdinalIgnoreCase))
                {
                    if (count.IsWhole)
                    {
                        totalRead = true;
                        total = count.Whole;
                        totalRow = index + 1;
                        continue;
                    }

                    refusals.Add("row " + (index + 1) + ", the TOTAL row: its " + countHeading + " cell "
                        + (count.IsRefused ? count.Refusal : "holds '" + countCell + "' and not a whole number")
                        + ".");
                    continue;
                }

                // A category row such as TREES holds text in one cell and nothing else. On a
                // schedule whose botanical column is the first, that text sits in the botanical
                // column, and a row of that shape is a heading rather than a species short of
                // its count.
                if (ScheduleColumns.IsStructureRow(row)) continue;

                string botanical = ScheduleColumns.At(row, nameColumn);
                if (string.IsNullOrWhiteSpace(botanical))
                {
                    // A count and no name is the subtotal a group prints under its species. A
                    // structure row holds neither and is nothing to count.
                    if (count.IsNumber) passedOver++;
                    if (count.IsRefused)
                    {
                        refusals.Add("row " + (index + 1) + ", no botanical name: its " + countHeading
                            + " cell " + count.Refusal);
                    }

                    continue;
                }

                if (count.IsWhole)
                {
                    // The height and the diameter come off the columns the heading row names,
                    // and a schedule naming neither still counts the row. A cell that does not
                    // read is carried with its reason rather than refusing the schedule.
                    found.Add(new SpeciesRow(
                        botanical.Trim(), carrying, count.Whole, plotId, index + 1,
                        Measure(row, heightColumn, ScheduleColumns.HeightWord),
                        Measure(row, diameterColumn, ScheduleColumns.DiameterWord)));
                    continue;
                }

                refusals.Add("row " + (index + 1) + ", " + botanical.Trim() + ": its " + countHeading + " cell "
                    + (count.IsRefused ? count.Refusal : "holds '" + countCell + "' and not a whole number")
                    + ", so the row cannot be counted.");
            }

            if (refusals.Count > 0) return SoftscapeReading.Refused(refusals);

            return SoftscapeReading.Of(found, totalRead, total, passedOver, totalRow);
        }

        private static PrintedMeasure Measure(IReadOnlyList<string> row, int column, string word)
        {
            return column < 0
                ? PrintedMeasure.NoColumn(word)
                : PrintedMeasure.Of(ScheduleColumns.At(row, column));
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
        public static ShrubsAndLawnReading Read(ScannedSchedule schedule, IEnumerable<string> headings)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");

            var wanted = (headings ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .ToList();

            if (!schedule.RowsWereRead || wanted.Count == 0) return ShrubsAndLawnReading.Nothing;

            List<IReadOnlyList<string>> rows = schedule.Rows.ToList();
            if (rows.Count == 0) return ShrubsAndLawnReading.Nothing;

            IReadOnlyList<string> headingRow = rows[0];
            int areaColumn = ScheduleColumns.Holding(headingRow, ScheduleColumns.AreaWord);
            int countColumn = ScheduleColumns.Holding(headingRow, ScheduleColumns.CountWord);
            int nameColumn = ScheduleColumns.Holding(headingRow, ScheduleColumns.BotanicalWord);

            // **Refused with the column named, never read off a position.** The area used to
            // return nothing in silence and the name fell back to the first cell, which is the
            // image, so an existing shrub with no photo read as a subtotal.
            var refusals = new List<string>();
            if (areaColumn < 0) refusals.Add(ScheduleColumns.NothingNamed(headingRow, ScheduleColumns.AreaWord));
            if (countColumn < 0) refusals.Add(ScheduleColumns.NothingNamed(headingRow, ScheduleColumns.CountWord));
            if (nameColumn < 0) refusals.Add(ScheduleColumns.NothingNamed(headingRow, ScheduleColumns.BotanicalWord));
            if (refusals.Count > 0) return ShrubsAndLawnReading.Refused(refusals);

            string areaHeading = (headingRow[areaColumn] ?? string.Empty).Trim();
            string countHeading = (headingRow[countColumn] ?? string.Empty).Trim();

            var found = new List<GroupSubtotal>();
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

                string areaCell = ScheduleColumns.At(row, areaColumn);
                CellNumberRead area = CellNumber.Read(areaCell);
                if (area.IsRefused)
                {
                    refusals.Add("row " + (index + 1) + ": its " + areaHeading + " cell " + area.Refusal);
                    continue;
                }

                if (!area.IsNumber) continue;

                string countCell = ScheduleColumns.At(row, countColumn);
                CellNumberRead count = CellNumber.Read(countCell);
                if (count.IsRefused)
                {
                    refusals.Add("row " + (index + 1) + ": its " + countHeading + " cell " + count.Refusal);
                    continue;
                }

                // A species is a row the botanical column names, whatever its image cell holds,
                // because an existing species prints with no photo. A subtotal names nothing
                // anywhere. TOTAL names nothing botanical either and is kept out by its own
                // first cell, which is the one thing it does carry.
                bool named = !string.IsNullOrWhiteSpace(ScheduleColumns.At(row, nameColumn));

                if (named)
                {
                    species.Add(area.Value);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(ScheduleColumns.At(row, 0))) continue;

                if (!count.IsWhole)
                {
                    refusals.Add("row " + (index + 1) + ", a subtotal row: its " + countHeading + " cell holds '"
                        + countCell + "' and not a whole number, so the group cannot be checked.");
                    continue;
                }

                subtotals.Add(new GroupSubtotal(heading, area.Value, count.Whole, rowNumber: index + 1));
            }

            if (refusals.Count > 0) return ShrubsAndLawnReading.Refused(refusals);

            Close(found, heading, subtotals, species);
            return ShrubsAndLawnReading.Of(found);
        }

        /// <summary>
        /// **THE LAST ROW IS THE GROUP'S VALUE.** One subtotal per phase, then the group total,
        /// so a group holding Existing and Proposed prints three rows and the third is the
        /// answer. Taking the first took one phase and called it the group, and on the 0928 run
        /// that meant 30 where the group is 84 and 361 where it is 820.
        ///
        /// Never a sum worked out here. The group total is a row the schedule printed and this
        /// takes it, the same rule the whole tool follows.
        /// </summary>
        private static void Close(
            List<GroupSubtotal> found, string heading, List<GroupSubtotal> subtotals, List<double> species)
        {
            if (heading == null || subtotals.Count == 0) return;

            GroupSubtotal last = subtotals[subtotals.Count - 1];
            double sum = species.Count == 0 ? double.NaN : species.Sum();

            // The row taken and every row considered travel with the value, so the report can
            // say which subtotal row was taken and why the others were not.
            found.Add(new GroupSubtotal(
                heading, last.SquareMetres, last.ItemCount, subtotals.Count, sum, Disagreeing(subtotals),
                last.RowNumber, subtotals.Select(one => one.RowNumber)));
        }

        /// <summary>
        /// The check that replaced looking for two equal rows: the last row must equal the rows
        /// above it added together, in area AND in item count. Four groups out of four on the
        /// 0928 run do, and a one phase group prints two equal rows, which passes the same check
        /// because the one row above equals the one below.
        ///
        /// A group printing a single row has nothing above it to check against, so it is taken
        /// and the report says the check had nothing to compare.
        /// </summary>
        private static string Disagreeing(List<GroupSubtotal> subtotals)
        {
            if (subtotals.Count < 2) return string.Empty;

            GroupSubtotal last = subtotals[subtotals.Count - 1];
            double area = 0.0;
            int items = 0;

            for (int at = 0; at < subtotals.Count - 1; at++)
            {
                area += subtotals[at].SquareMetres;
                items += subtotals[at].ItemCount;
            }

            if (Adds(last.SquareMetres, area) && last.ItemCount == items) return string.Empty;

            return "its group total reads " + Said(last.SquareMetres) + " over " + last.ItemCount
                + " and the " + (subtotals.Count - 1)
                + (subtotals.Count == 2 ? " row" : " rows") + " above it add to "
                + Said(area) + " over " + items + ". Every row: " + string.Join(", ", subtotals
                    .Select(one => Said(one.SquareMetres) + " over " + one.ItemCount).ToArray());
        }

        /// <summary>
        /// The same relative room <see cref="Totalled.Adds"/> allows, from the one constant, so
        /// the last bits of a double moving cannot refuse a schedule that adds up.
        /// </summary>
        private static bool Adds(double total, double parts)
        {
            return Math.Abs(total - parts) <= Totalled.Tolerance * Math.Max(1.0, Math.Abs(total));
        }

        private static string Said(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
