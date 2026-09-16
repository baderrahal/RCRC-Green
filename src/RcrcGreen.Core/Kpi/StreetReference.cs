using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One row of the scope validation file: the plot it names and the three values read off it,
    /// each exactly as the cell holds it.
    /// </summary>
    public sealed class StreetReferenceRow
    {
        public StreetReferenceRow(string uid, int row, string quantity, string unit, string roadWidth)
        {
            Uid = (uid ?? string.Empty).Trim();
            Row = row;
            Quantity = (quantity ?? string.Empty).Trim();
            Unit = (unit ?? string.Empty).Trim();
            RoadWidth = (roadWidth ?? string.Empty).Trim();
        }

        /// <summary>
        /// ID_UID, which is the same identifier PRX_Plot_UID2 holds on the plot's sheet.
        /// </summary>
        public string Uid { get; }

        /// <summary>
        /// Which row of the sheet it printed on, so a plot named on two rows names both.
        /// </summary>
        public int Row { get; }

        public string Quantity { get; }

        public string Unit { get; }

        public string RoadWidth { get; }
    }

    /// <summary>
    /// What the reference file says about one plot, or why it says nothing usable.
    /// </summary>
    public sealed class StreetReferenceAnswer
    {
        private StreetReferenceAnswer(bool found, double width, double length, string why, string printedWidth, string printedLength)
        {
            Found = found;
            Width = width;
            Length = length;
            Why = why ?? string.Empty;
            PrintedWidth = printedWidth ?? string.Empty;
            PrintedLength = printedLength ?? string.Empty;
        }

        public bool Found { get; }

        public double Width { get; }

        public double Length { get; }

        /// <summary>
        /// Why the two cells stay empty, and empty when they do not. **It is never a refusal of
        /// the run.** A street plot the file does not name is written with both cells left empty
        /// and is named in the report with its UID, the same way the link note works.
        /// </summary>
        public string Why { get; }

        /// <summary>
        /// What the cells held, so the report can print the file's own text beside the number.
        /// </summary>
        public string PrintedWidth { get; }

        public string PrintedLength { get; }

        public static StreetReferenceAnswer Nothing(string why)
        {
            return new StreetReferenceAnswer(false, 0.0, 0.0, why, string.Empty, string.Empty);
        }

        public static StreetReferenceAnswer Of(double width, double length, string printedWidth, string printedLength)
        {
            return new StreetReferenceAnswer(true, width, length, string.Empty, printedWidth, printedLength);
        }
    }

    /// <summary>
    /// The team's Scope_Validation file, read for the two cells the STREETS template says are
    /// typed by hand: D8 Streets ROW (m) and F8 Streets Total Length (m). H8 is the two
    /// multiplied and the workbook computes it, so nothing is written there.
    ///
    /// **Measured on Scope_Validation_21072026, 2026-09-13**, which is not in this repository and
    /// never will be. One sheet, a header row and 8,353 rows. The four wanted columns are NOT in
    /// the first four places and there is no header over column B at all, which is why they are
    /// found by the names in the header row:
    ///
    /// <code>
    /// D   ID_UID *          the plot, ANH-007-ST-100210
    /// H   ES_QUANTITY       330.65849900000001
    /// I   QUANTITY UNIT     m on 6,301 rows, sqm on 2,051, and Null on one
    /// O   ROAD_WIDTH        20
    /// </code>
    ///
    /// **The file writes an absent value as the text Null in angle brackets rather than leaving
    /// the cell empty**, on every one of the 2,051 sqm rows' road width and on one row's unit. It
    /// is text, so <see cref="CellNumber"/> reads no number out of it and the plot is named
    /// rather than written with it, which is the whole reason the numbers go through that reader
    /// rather than through a parse of my own.
    ///
    /// **NOTHING IS ESTIMATED FROM THE COMPONENT VALUE.** STREET 30m ROW looks like it says 30
    /// and the 313 ANH-007-ST rows read 15 on 156 of them, 20 on 65, 10 on 57, 30 on 15 and 36 on
    /// 13, with 5, 6, 8 and 12 among the rest. A width read off the component name would be a
    /// number nobody measured, in a client file, on more than half the plots.
    ///
    /// **The file holds 33 UIDs on two rows**, none of them in ANH-007, so the double refusal
    /// below fires on no NG05 plot and is still the rule.
    ///
    /// Only STREETS asks it this round. The file also holds parking, mosque, park, school, health
    /// and government rows and nothing reads them.
    /// </summary>
    public sealed class StreetReferenceFile
    {
        public const string UidColumn = "ID_UID *";

        public const string QuantityColumn = "ES_QUANTITY";

        public const string UnitColumn = "QUANTITY UNIT";

        public const string WidthColumn = "ROAD_WIDTH";

        /// <summary>
        /// The only unit a length may be in. A row in anything else is not used and is named,
        /// because 2,051 of the file's rows are areas in sqm and an area written into a length
        /// cell computes an area of its own.
        /// </summary>
        public const string Metres = "m";

        public const string NotSetWhy =
            "no street reference file is set, so the road width and the total length are left "
            + "empty and the run goes through";

        private readonly Dictionary<string, List<StreetReferenceRow>> _byUid;

        private StreetReferenceFile(
            bool read, string path, string sheetName, int headerRow,
            IEnumerable<StreetReferenceRow> rows, string why)
        {
            Read = read;
            Path = path ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            HeaderRow = headerRow;
            Why = why ?? string.Empty;
            Rows = (rows ?? Enumerable.Empty<StreetReferenceRow>()).ToList();

            _byUid = new Dictionary<string, List<StreetReferenceRow>>(StringComparer.Ordinal);
            foreach (StreetReferenceRow one in Rows)
            {
                if (one.Uid.Length == 0) continue;

                List<StreetReferenceRow> held;
                if (!_byUid.TryGetValue(one.Uid, out held))
                {
                    held = new List<StreetReferenceRow>();
                    _byUid[one.Uid] = held;
                }

                held.Add(one);
            }
        }

        /// <summary>
        /// Whether the file was opened and its four columns found. False is a NOTE and never a
        /// refusal: both cells stay empty, every street plot is named, and the run goes through.
        /// </summary>
        public bool Read { get; }

        public string Path { get; }

        public string SheetName { get; }

        /// <summary>
        /// Which row named the columns, so the report says where the names were read rather than
        /// leaving it to be assumed.
        /// </summary>
        public int HeaderRow { get; }

        public IReadOnlyList<StreetReferenceRow> Rows { get; }

        public string Why { get; }

        public static readonly StreetReferenceFile NotSet =
            new StreetReferenceFile(false, string.Empty, string.Empty, 0, null, NotSetWhy);

        public static StreetReferenceFile Refused(string path, string why)
        {
            return new StreetReferenceFile(false, path, string.Empty, 0, null, why);
        }

        public static StreetReferenceFile Holding(
            string path, string sheetName, int headerRow, IEnumerable<StreetReferenceRow> rows)
        {
            return new StreetReferenceFile(true, path, sheetName, headerRow, rows, string.Empty);
        }

        /// <summary>
        /// What the file says about one plot. **The UID is matched whole and without case**,
        /// never as a prefix and never against a name: ANH-007-ST-100050 and ANH-007-ST-100500
        /// share an opening and are two plots.
        /// </summary>
        public StreetReferenceAnswer For(string uid2)
        {
            if (!Read) return StreetReferenceAnswer.Nothing(Why);

            string held = (uid2 ?? string.Empty).Trim();
            if (held.Length == 0)
            {
                return StreetReferenceAnswer.Nothing(
                    "this plot carries no " + KpiNames.PlotUid2 + ", so nothing can be looked up");
            }

            List<StreetReferenceRow> found = _byUid
                .Where(one => string.Equals(one.Key, held, StringComparison.OrdinalIgnoreCase))
                .SelectMany(one => one.Value)
                .OrderBy(one => one.Row)
                .ToList();

            if (found.Count == 0)
            {
                return StreetReferenceAnswer.Nothing(
                    held + " is not in the street reference file, so the road width and the "
                    + "total length are left empty");
            }

            if (found.Count > 1)
            {
                return StreetReferenceAnswer.Nothing(
                    held + " is on " + found.Count + " rows of the street reference file, "
                    + string.Join(" and ", found.Select(one => "row " + one.Row).ToArray())
                    + ", and nothing says which is meant");
            }

            StreetReferenceRow row = found[0];

            if (!string.Equals(row.Unit, Metres, StringComparison.OrdinalIgnoreCase))
            {
                return StreetReferenceAnswer.Nothing(
                    held + " is on row " + row.Row + " with " + UnitColumn + " reading "
                    + Shown(row.Unit) + " rather than " + Metres
                    + ", so the row is not used");
            }

            CellNumberRead width = CellNumber.Read(row.RoadWidth);
            CellNumberRead length = CellNumber.Read(row.Quantity);

            if (!width.IsNumber || !length.IsNumber)
            {
                return StreetReferenceAnswer.Nothing(
                    held + " is on row " + row.Row + " and " + WhatWouldNotRead(width, length, row));
            }

            return StreetReferenceAnswer.Of(width.Value, length.Value, row.RoadWidth, row.Quantity);
        }

        private static string WhatWouldNotRead(CellNumberRead width, CellNumberRead length, StreetReferenceRow row)
        {
            var said = new List<string>();
            if (!width.IsNumber) said.Add(WidthColumn + " reads " + Shown(row.RoadWidth));
            if (!length.IsNumber) said.Add(QuantityColumn + " reads " + Shown(row.Quantity));

            return string.Join(" and ", said.ToArray()) + ", which is no number, so the row is not used";
        }

        private static string Shown(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "an empty cell" : value;
        }

        /// <summary>
        /// The file read off disk, or a refusal saying why. Every failure to read is a note the
        /// run carries rather than a throw, because a street run with no reference file is a
        /// legitimate thing to do and the workbook is still worth writing.
        /// </summary>
        public static StreetReferenceFile In(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return NotSet;

            try
            {
                using (var reading = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null) return Refused(path, "it is not a workbook this tool can open");

                    Dictionary<string, string> sheets = WorkbookPackage.SheetParts(zip, workbookPart);
                    if (sheets.Count == 0) return Refused(path, "it holds no sheet");

                    KeyValuePair<string, string> first = sheets.First();
                    XDocument sheet = WorkbookPackage.Read(zip, first.Value);
                    if (sheet == null) return Refused(path, "its first sheet could not be read");

                    return Off(path, first.Key, sheet, WorkbookPackage.SharedStrings(zip));
                }
            }
            catch (IOException failed)
            {
                return Refused(path, "it could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return Refused(path, "it could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return Refused(path, "it is not a readable .xlsx: " + failed.Message);
            }
            catch (XmlException failed)
            {
                // **AUDIT 4 FINDING 67**, the same shape as the three beside it.
                return Refused(path, "its first sheet is not well formed XML: " + failed.Message);
            }
        }

        /// <summary>
        /// The rows off one sheet. **The columns are found by the names in the header row and
        /// never by position**, which is this repository's oldest rule and is earned here: the
        /// wanted four sit at D, H, I and O, and column B carries a header cell that does not
        /// exist at all.
        /// </summary>
        internal static StreetReferenceFile Off(
            string path, string sheetName, XDocument sheet, List<string> shared)
        {
            Dictionary<int, Dictionary<string, string>> byRow = CellsByRow(sheet, shared);
            if (byRow.Count == 0) return Refused(path, "its first sheet is empty");

            int headerRow = byRow.Keys.Min();
            Dictionary<string, string> header = byRow[headerRow];

            string uid = ColumnNamed(header, UidColumn);
            string quantity = ColumnNamed(header, QuantityColumn);
            string unit = ColumnNamed(header, UnitColumn);
            string width = ColumnNamed(header, WidthColumn);

            string[] missing = new[]
                {
                    uid.Length == 0 ? UidColumn : null,
                    quantity.Length == 0 ? QuantityColumn : null,
                    unit.Length == 0 ? UnitColumn : null,
                    width.Length == 0 ? WidthColumn : null
                }
                .Where(one => one != null)
                .ToArray();

            if (missing.Length > 0)
            {
                return Refused(path,
                    "row " + headerRow + " of " + sheetName + " names no "
                    + string.Join(" and no ", missing)
                    + " column, and nothing here reads a column by its position");
            }

            var rows = new List<StreetReferenceRow>();
            foreach (int number in byRow.Keys.Where(one => one > headerRow).OrderBy(one => one))
            {
                Dictionary<string, string> cells = byRow[number];
                string held = In(cells, uid);
                if (held.Length == 0) continue;

                rows.Add(new StreetReferenceRow(
                    held, number, In(cells, quantity), In(cells, unit), In(cells, width)));
            }

            return Holding(path, sheetName, headerRow, rows);
        }

        /// <summary>
        /// The column whose heading is the wanted name. **The same one rule every other whole
        /// label lookup in this tool asks**, so a heading with an edge space is the same heading
        /// and a double space between words is a different one. It used to trim the cell alone
        /// and compare against the name as written, which is one side of a two sided question.
        /// </summary>
        private static string ColumnNamed(Dictionary<string, string> header, string wanted)
        {
            foreach (KeyValuePair<string, string> cell in header)
            {
                if (LabelText.Same(cell.Value, wanted)) return cell.Key;
            }

            return string.Empty;
        }

        private static string In(Dictionary<string, string> row, string column)
        {
            string held;
            return row.TryGetValue(column, out held) ? held : string.Empty;
        }

        private static Dictionary<int, Dictionary<string, string>> CellsByRow(XDocument sheet, List<string> shared)
        {
            var byRow = new Dictionary<int, Dictionary<string, string>>();

            foreach (XElement cell in WorkbookPackage.Cells(sheet))
            {
                XAttribute reference = cell.Attribute("r");
                if (reference == null) continue;

                CellRef parts = CellRef.TryParse(reference.Value);
                if (parts == null) continue;

                Dictionary<string, string> held;
                if (!byRow.TryGetValue(parts.Row, out held))
                {
                    held = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    byRow[parts.Row] = held;
                }

                held[parts.Column] = WorkbookPackage.TextOf(cell, shared) ?? string.Empty;
            }

            return byRow;
        }

        /// <summary>
        /// One line for the report saying what was read, so a run with no widths in it says why
        /// once at the top rather than 78 times underneath.
        /// </summary>
        public string InWords
        {
            get
            {
                if (!Read) return "STREET REFERENCE FILE: not read. " + Why;

                return "STREET REFERENCE FILE: " + Path + ", sheet " + SheetName + ", columns named in row "
                    + HeaderRow.ToString(CultureInfo.InvariantCulture) + ", "
                    + Rows.Count.ToString(CultureInfo.InvariantCulture) + " rows carrying a "
                    + UidColumn + ".";
            }
        }
    }
}
