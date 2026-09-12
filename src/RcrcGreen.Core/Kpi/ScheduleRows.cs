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
    /// **A group row is any row of one text cell that heads species rows**, and the ones that
    /// count are the ones a tree list sheet is named for. It used to be any row naming one of the
    /// document's phases, so FM-05's third group, Street Design, was not a group row at all and
    /// its four species carried on under Proposed, 38 trees into the proposed list in silence.
    /// A row of one text cell directly followed by another is a heading such as TREES and not
    /// a group. Every group row found is handed back, taken or left out, with its subtotal, so
    /// the report names all of them. DM-25 prints Existing, Proposed, then Existing again: a
    /// name can repeat, both are taken, and the second says so.
    ///
    /// Nothing is dropped in silence. A row with a name and no whole count refuses the read and
    /// names the row. A row with a count and no name is a subtotal, the first under a group is
    /// that group's, and all of them are counted as passed over. The TOTAL row is read off the
    /// row whose first cell holds that word, so the species rows taken and left out can be held
    /// against what the schedule says they add to.
    /// </summary>
    public static class SoftscapeRows
    {
        public const string TotalMark = "TOTAL";

        public static SoftscapeReading Read(ScannedSchedule schedule, CountedGroups counted, string plotId)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");
            if (counted == null) throw new ArgumentNullException("counted");

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

            var groups = new List<GroupBeingRead>();
            var ungrouped = new List<SpeciesRow>();
            GroupBeingRead current = null;
            bool totalRead = false;
            int total = 0;
            int totalRow = 0;
            int passedOver = 0;

            for (int index = 1; index < rows.Count; index++)
            {
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

                // One text cell and nothing else. Followed by another such row it is a heading,
                // TREES over the groups, and followed by anything else it is a group row.
                if (ScheduleColumns.IsStructureRow(row))
                {
                    bool heading = index + 1 >= rows.Count || ScheduleColumns.IsStructureRow(rows[index + 1]);
                    if (heading) continue;

                    current = new GroupBeingRead(row[0].Trim(), index + 1);
                    groups.Add(current);
                    continue;
                }

                string botanical = ScheduleColumns.At(row, nameColumn);
                if (string.IsNullOrWhiteSpace(botanical))
                {
                    // A count and no name is the subtotal a group prints under its species. The
                    // first one under a group is that group's, and every one is counted as
                    // passed over. A structure row holds neither and is nothing to count.
                    if (count.IsNumber)
                    {
                        passedOver++;
                        if (current != null && !current.SubtotalPrinted && count.IsWhole)
                        {
                            current.SubtotalPrinted = true;
                            current.Subtotal = count.Whole;
                            current.SubtotalRow = index + 1;
                        }
                    }

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
                    var species = new SpeciesRow(
                        botanical.Trim(), current == null ? string.Empty : current.Name, count.Whole, plotId, index + 1,
                        Measure(row, heightColumn, ScheduleColumns.HeightWord),
                        Measure(row, diameterColumn, ScheduleColumns.DiameterWord),
                        current == null ? 0 : current.RowNumber);
                    if (current == null) ungrouped.Add(species);
                    else current.Species.Add(species);
                    continue;
                }

                refusals.Add("row " + (index + 1) + ", " + botanical.Trim() + ": its " + countHeading + " cell "
                    + (count.IsRefused ? count.Refusal : "holds '" + countCell + "' and not a whole number")
                    + ", so the row cannot be counted.");
            }

            if (refusals.Count > 0) return SoftscapeReading.Refused(refusals);

            var printed = new List<PrintedGroup>();
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (GroupBeingRead group in groups)
            {
                bool counts = counted.Counts(group.Name);
                int before;
                seen.TryGetValue(group.Name, out before);
                seen[group.Name] = before + 1;

                string why = counted.Why(group.Name);
                if (counts && before > 0)
                {
                    why += ", and it is the " + Ordinal(before + 1) + " group row so named on this schedule, taken as well";
                }

                printed.Add(new PrintedGroup(group.Name, group.RowNumber, group.Species,
                    group.SubtotalPrinted, group.Subtotal, group.SubtotalRow, counts, why));
            }

            // In the order printed: a species above every group row comes first, then the
            // groups a tree list sheet is named for, each in its place, and the others' rows
            // travel on the PrintedGroup alone.
            List<SpeciesRow> found = ungrouped.ToList();
            found.AddRange(printed.Where(one => one.Counted).SelectMany(one => one.Species));

            return SoftscapeReading.Of(found, totalRead, total, passedOver, totalRow, printed);
        }

        private sealed class GroupBeingRead
        {
            public GroupBeingRead(string name, int rowNumber)
            {
                Name = name;
                RowNumber = rowNumber;
                Species = new List<SpeciesRow>();
            }

            public string Name { get; }

            public int RowNumber { get; }

            public List<SpeciesRow> Species { get; }

            public bool SubtotalPrinted;

            public int Subtotal;

            public int SubtotalRow;
        }

        private static string Ordinal(int number)
        {
            switch (number)
            {
                case 2: return "2nd";
                case 3: return "3rd";
                default: return number + "th";
            }
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
    /// The shape is measured, off the 1355 scan report and the 1536 create report, neither of
    /// which is in this repository because nothing under reports/ is ever committed.
    /// FM-05-(600) SHRUBS &amp; LAWN SCHEDULE prints eleven columns and this:
    ///
    /// <code>
    /// IMAGE | # | PLANT CODE | BOTANICAL NAME | AREA (sqm) | COUNT (n) | ...
    /// GRASS                                                                   group heading
    /// Proposed                                                                phase row
    /// ... species ...
    ///                                         | 96 m2  | 117 | ...            the phase
    /// Street Design                                                           phase row
    /// ... species ...
    ///                                         | 69 m2  | 84 | ...             the phase
    ///                                         | 165 m2 | 201 | ...            the group total
    /// SHRUBS &amp; GROUND COVER                                                   group heading
    /// ...
    /// TOTAL                                   | ... |                         the whole thing
    /// </code>
    ///
    /// **THE VALUE IS THE PHASES TAKEN ADDED TOGETHER, AND NEVER THE GROUP TOTAL.** The phases
    /// taken are the ones a tree list sheet is named for. Street Design is somebody else's scope
    /// by Bader's decision, so FM-05 GRASS is 96 and not 165 and its SHRUBS are 361 and not 820,
    /// with 459 of the 820 left out and named. The group total is the check: the phases taken
    /// plus the phases left out must equal it, in area and in item count, and a group total
    /// that does not add up refuses the write. Before the subtotal fix the tool took the first
    /// subtotal row, which was Proposed and right by accident. After it the tool took the group
    /// total, which was wrong for a different reason. Neither was visible until the schedule was
    /// printed.
    ///
    /// A group printing no phase row at all has only its own subtotal rows, and there the last
    /// row is the value and the rows above it must add to it, which is the older rule and the
    /// only one such a group can have.
    ///
    /// TOTAL needs no special case. It carries numbers, so it is not a structure row, and its
    /// first cell holds text, so it is not a subtotal row.
    /// </summary>
    public static class ShrubsAndLawnRows
    {
        public static ShrubsAndLawnReading Read(ScannedSchedule schedule, IEnumerable<string> headings, CountedGroups counted, ProjectUnit areaUnit)
        {
            if (schedule == null) throw new ArgumentNullException("schedule");
            if (counted == null) throw new ArgumentNullException("counted");
            if (areaUnit == null) throw new ArgumentNullException("areaUnit");

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
            var subtotals = new List<SubtotalRow>();
            var species = new List<double>();
            string phase = null;
            int phaseRow = 0;

            for (int index = 1; index < rows.Count; index++)
            {
                IReadOnlyList<string> row = rows[index];

                if (ScheduleColumns.IsStructureRow(row))
                {
                    string text = row[0].Trim();
                    if (!wanted.Any(one => string.Equals(one.Trim(), text, StringComparison.OrdinalIgnoreCase)))
                    {
                        // A phase row inside the group. The subtotal row that follows its species
                        // is that phase's.
                        phase = text;
                        phaseRow = index + 1;
                        continue;
                    }

                    Close(found, heading, subtotals, species, counted, areaUnit);
                    heading = text;
                    subtotals = new List<SubtotalRow>();
                    species = new List<double>();
                    phase = null;
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

                subtotals.Add(new SubtotalRow(index + 1, area.Value, count.Whole, phase, phaseRow));
                phase = null;
            }

            if (refusals.Count > 0) return ShrubsAndLawnReading.Refused(refusals);

            Close(found, heading, subtotals, species, counted, areaUnit);
            return ShrubsAndLawnReading.Of(found);
        }

        private sealed class SubtotalRow
        {
            public SubtotalRow(int rowNumber, double squareMetres, int itemCount, string phase, int phaseRow)
            {
                RowNumber = rowNumber;
                SquareMetres = squareMetres;
                ItemCount = itemCount;
                Phase = phase;
                PhaseRow = phaseRow;
            }

            public int RowNumber { get; }

            public double SquareMetres { get; }

            public int ItemCount { get; }

            /// <summary>
            /// The phase row this subtotal followed, or null for a subtotal that followed no
            /// phase row since the last subtotal, which is the group total.
            /// </summary>
            public string Phase { get; }

            public int PhaseRow { get; }
        }

        /// <summary>
        /// One group closed: the phases a tree list sheet is named for are taken and added, the
        /// others are left out and named, and the group total row is the check on both.
        /// </summary>
        private static void Close(
            List<GroupSubtotal> found, string heading, List<SubtotalRow> subtotals, List<double> species, CountedGroups counted, ProjectUnit areaUnit)
        {
            if (heading == null || subtotals.Count == 0) return;

            double sum = species.Count == 0 ? double.NaN : species.Sum();
            List<SubtotalRow> phased = subtotals.Where(one => one.Phase != null).ToList();
            List<SubtotalRow> unphased = subtotals.Where(one => one.Phase == null).ToList();

            // No phase row at all: the last row is the value and the rows above it must add to
            // it, which is all such a group can offer.
            if (phased.Count == 0)
            {
                string plainNote;
                string plainRefusal = Disagreeing(subtotals, areaUnit, out plainNote);
                SubtotalRow last = unphased[unphased.Count - 1];
                found.Add(new GroupSubtotal(
                    heading, last.SquareMetres, last.ItemCount, subtotals.Count, sum, plainRefusal,
                    last.RowNumber, subtotals.Select(one => one.RowNumber), null,
                    subtotals.Count > 1, last.SquareMetres, last.ItemCount, plainNote));
                return;
            }

            var phases = phased
                .Select(one => new PhaseSubtotal(one.Phase, one.RowNumber, one.SquareMetres, one.ItemCount,
                    counted.Counts(one.Phase), counted.Why(one.Phase)))
                .ToList();
            List<PhaseSubtotal> taken = phases.Where(one => one.Counted).ToList();
            SubtotalRow groupTotal = unphased.Count == 0 ? null : unphased[unphased.Count - 1];

            string note = string.Empty;
            string refusal = groupTotal == null
                ? string.Empty
                : Disagreeing(phased.Concat(new[] { groupTotal }).ToList(), areaUnit, out note);

            found.Add(new GroupSubtotal(
                heading,
                taken.Sum(one => one.SquareMetres),
                taken.Sum(one => one.ItemCount),
                subtotals.Count,
                sum,
                refusal,
                groupTotal == null ? 0 : groupTotal.RowNumber,
                subtotals.Select(one => one.RowNumber),
                phases,
                groupTotal != null,
                groupTotal == null ? 0.0 : groupTotal.SquareMetres,
                groupTotal == null ? 0 : groupTotal.ItemCount,
                note));
        }

        /// <summary>
        /// The last row must equal the rows above it added together, in item count exactly and
        /// in area to within the project's own rounding. Four groups out of four on the 0928
        /// run add exactly. FM-21 and FM-22 on the 1208 run do not: counts exact, areas off by
        /// one in opposite directions, because the project rounds areas to the metre, every
        /// printed area is already rounded, and a sum of rounded numbers need not equal a
        /// rounded sum. So counts are integers and get no room at all, and areas get half the
        /// unit's rounding step for each row summed, read off the project units and never a
        /// constant. Within that room is a note rather than a refusal, so a real fault growing
        /// slowly is still visible. Outside it still refuses, and a step that was not read
        /// allows nothing and says so, because a check that cannot see its subject must not
        /// quietly widen.
        /// </summary>
        private static string Disagreeing(List<SubtotalRow> subtotals, ProjectUnit areaUnit, out string roundingNote)
        {
            roundingNote = string.Empty;
            if (subtotals.Count < 2) return string.Empty;

            SubtotalRow last = subtotals[subtotals.Count - 1];
            double area = 0.0;
            int items = 0;

            for (int at = 0; at < subtotals.Count - 1; at++)
            {
                area += subtotals[at].SquareMetres;
                items += subtotals[at].ItemCount;
            }

            if (Adds(last.SquareMetres, area) && last.ItemCount == items) return string.Empty;

            int summed = subtotals.Count - 1;
            string rowWord = summed == 1 ? " row" : " rows";
            double step = areaUnit.Accuracy;
            bool stepRead = !double.IsNaN(step) && step > 0.0;
            double allowed = stepRead ? summed * step / 2.0 : 0.0;

            if (last.ItemCount == items)
            {
                double off = Math.Abs(last.SquareMetres - area);
                if (stepRead && off <= allowed + Totalled.Tolerance * Math.Max(1.0, Math.Abs(last.SquareMetres)))
                {
                    roundingNote = "the " + summed + rowWord + " above it add to " + Said(area)
                        + " over " + items + " against its printed " + Said(last.SquareMetres)
                        + " over " + last.ItemCount + ", off by " + Said(off)
                        + " in area with every count exact, within the " + Said(allowed)
                        + " that " + summed + rowWord + " rounded to " + KpiReport.Step(step)
                        + (summed == 1 ? " allows" : " allow") + ", so it is noted rather than refused";
                    return string.Empty;
                }
            }

            string room = last.ItemCount != items
                ? string.Empty
                : stepRead
                    ? ", which is more than the " + Said(allowed) + " that " + summed + rowWord
                        + " rounded to " + KpiReport.Step(step) + (summed == 1 ? " allows" : " allow")
                    : ", and the project's area rounding step was not read, so no rounding room was allowed";

            return "its group total reads " + Said(last.SquareMetres) + " over " + last.ItemCount
                + " and the " + summed + rowWord + " above it add to "
                + Said(area) + " over " + items + room + ". Every row: " + string.Join(", ", subtotals
                    .Select(one => (one.Phase == null ? string.Empty : one.Phase + " ")
                        + Said(one.SquareMetres) + " over " + one.ItemCount).ToArray());
        }

        /// <summary>
        /// The same relative room <see cref="Totalled.Adds"/> allows, from the one constant, so
        /// the last bits of a double moving cannot refuse a schedule that adds up.
        /// </summary>
        private static bool Adds(double total, double parts)
        {
            return Math.Abs(total - parts) <= Totalled.Tolerance * Math.Max(1.0, Math.Abs(total));
        }

        /// <summary>
        /// Six places, because the note's numbers must show at the scale of the step they are
        /// held against. Two places printed a project rounding to 0.001 as off by 0 within the
        /// 0 it allows, a sentence at war with itself, and twelve showed the last bits of a
        /// double on summed thousands as digits nobody printed.
        /// </summary>
        private static string Said(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }
    }
}
