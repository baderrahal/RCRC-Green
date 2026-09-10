using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One row of a tree list sheet: the botanical name the workbook holds and the row it sits
    /// on. The quantity goes in column B of that row, so the row number is what a write needs.
    /// </summary>
    public sealed class SpeciesListRow
    {
        public SpeciesListRow(int row, string botanicalName)
        {
            if (row < 1) throw new ArgumentOutOfRangeException("row");
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");

            Row = row;
            BotanicalName = botanicalName;
        }

        public int Row { get; }

        public string BotanicalName { get; }
    }

    /// <summary>
    /// The species list a tree list sheet already holds, read out of the template, and which
    /// rows of it the sheet's own total reaches.
    ///
    /// The workbook's own column D is the only list there is. Nothing in this repo carries a
    /// copy of it, because a plant palette written into the code would be a second record of
    /// the client's list and would be wrong the first time they change it.
    ///
    /// **THE ROWS ARE READ OFF THE FILE, ALL OF THEM, AND THE MAP HOLDS NO RANGE.** The first
    /// twenty plot run on MOSQUES measured three answers to where the list ends: the map said
    /// row 83, the sheet's total said SUM(B4:B92), and the botanical names ran to row 101. The
    /// tool trusted the shortest, so four species sitting past row 83 with a row waiting were
    /// reported as having nowhere to go, 69 trees between them, and the workbook went out
    /// saying 76 existing trees where the model holds 161. Two things are read here and held
    /// apart: every row that names a species, read down column D until the names stop, and the
    /// rows the total reaches, read off the total's own formula. A name on a row the total does
    /// not reach is named, and nothing is ever written there.
    ///
    /// A cell holding a shared string stores an index rather than the text, so the shared
    /// string table is read as well. Reading the index and treating it as the name would match
    /// nothing and report every species as unmatched, which reads exactly like a workbook whose
    /// list is empty.
    /// </summary>
    public sealed class SpeciesList
    {
        private SpeciesList(
            IEnumerable<SpeciesListRow> named,
            bool totalFound,
            int totalFirstRow,
            int totalLastRow,
            string totalCell,
            string refusal)
        {
            Refusal = refusal ?? string.Empty;
            TotalFound = totalFound;
            TotalFirstRow = totalFound ? totalFirstRow : 0;
            TotalLastRow = totalFound ? totalLastRow : 0;
            TotalCell = totalFound ? (totalCell ?? string.Empty) : string.Empty;

            Dictionary<int, SpeciesListRow> byRow = (named ?? Enumerable.Empty<SpeciesListRow>())
                .Where(one => one != null)
                .GroupBy(one => one.Row)
                .ToDictionary(group => group.Key, group => group.First());

            // The list is read down from the row under the header until the first row with no
            // name. That is where the list stops, whatever any range anywhere says.
            var rows = new List<SpeciesListRow>();
            int at = KpiTemplates.TreeHeaderRow + 1;
            while (byRow.ContainsKey(at))
            {
                rows.Add(byRow[at]);
                at++;
            }

            Rows = rows;
            FirstGapRow = at;
            BelowTheList = byRow.Keys
                .Where(row => row > at)
                .OrderBy(row => row)
                .Select(row => byRow[row])
                .ToList();

            var empty = new List<int>();
            var outside = new List<SpeciesListRow>();
            if (totalFound)
            {
                for (int row = TotalFirstRow; row <= TotalLastRow; row++)
                {
                    if (!byRow.ContainsKey(row)) empty.Add(row);
                }

                outside = Rows.Concat(BelowTheList)
                    .Where(one => !Reaches(one.Row))
                    .OrderBy(one => one.Row)
                    .ToList();
            }

            EmptyRows = empty;
            OutsideTheTotal = outside;
        }

        /// <summary>
        /// Every row that names a species, from the row under the header down to the row before
        /// the first empty one. What a species from Revit is matched against.
        /// </summary>
        public IReadOnlyList<SpeciesListRow> Rows { get; }

        /// <summary>
        /// The first row under the header with no name in column D, which is where the list
        /// stops. One past the last row of <see cref="Rows"/>.
        /// </summary>
        public int FirstGapRow { get; }

        /// <summary>
        /// Rows naming something further down the sheet, past the first gap. They are not the
        /// list, they are not matched against, and a species carrying one of these names is
        /// refused by name rather than written in a second time above it. Nothing was measured
        /// holding any, and the reader says so rather than passing them over in silence.
        /// </summary>
        public IReadOnlyList<SpeciesListRow> BelowTheList { get; }

        /// <summary>
        /// Whether a cell in the quantity column holding SUM over that same column was found.
        /// Without it nothing says which rows a count reaches, and a count is written nowhere.
        /// </summary>
        public bool TotalFound { get; }

        public int TotalFirstRow { get; }

        public int TotalLastRow { get; }

        /// <summary>
        /// Where the total sits, as B93, so a report line can point at it.
        /// </summary>
        public string TotalCell { get; }

        /// <summary>
        /// The rows the total reaches that name nothing, in order, for writing a species the
        /// list does not hold into. The total's own range is what says which rows reach it,
        /// and that is read off the file. Empty when the total could not be found, so a species
        /// is reported as not placed rather than written into a row nothing sums.
        /// </summary>
        public IReadOnlyList<int> EmptyRows { get; }

        /// <summary>
        /// Rows naming a species that the total does not reach. On the MOSQUES existing list
        /// measured on 2026-09-10 that is rows 93 to 101, nine species. A count written on one
        /// of these lands on the sheet and never reaches the total, so the sheet reads as
        /// complete and is short. Empty when the total was not found, because then nothing
        /// says what it reaches.
        /// </summary>
        public IReadOnlyList<SpeciesListRow> OutsideTheTotal { get; }

        public string Refusal { get; }

        public bool WasRead
        {
            get { return Refusal.Length == 0; }
        }

        public bool Reaches(int row)
        {
            return TotalFound && row >= TotalFirstRow && row <= TotalLastRow;
        }

        /// <summary>
        /// SUM(B4:B92) at B93, or the words for none found.
        /// </summary>
        public string TotalInWords
        {
            get
            {
                if (!TotalFound) return NoTotalFound;

                return "SUM(" + KpiTemplates.QuantityColumn + TotalFirstRow.ToString(CultureInfo.InvariantCulture)
                    + ":" + KpiTemplates.QuantityColumn + TotalLastRow.ToString(CultureInfo.InvariantCulture) + ")"
                    + (TotalCell.Length == 0 ? string.Empty : " at " + TotalCell);
            }
        }

        public const string NoTotalFound =
            "no cell in column B holding SUM over column B was found on the sheet";

        public static SpeciesList Refused(string why)
        {
            return new SpeciesList(null, false, 0, 0, null, why ?? "The species list could not be read.");
        }

        /// <summary>
        /// A list as read: every named row, and the range the total reaches. The empty rows and
        /// the rows outside the total are worked out from those two and are not handed in, so
        /// nothing can state an empty row that is named or a named row the total reaches.
        /// </summary>
        public static SpeciesList Holding(
            IEnumerable<SpeciesListRow> named, int totalFirstRow, int totalLastRow, string totalCell = null)
        {
            if (totalFirstRow < 1) throw new ArgumentOutOfRangeException("totalFirstRow");
            if (totalLastRow < totalFirstRow) throw new ArgumentOutOfRangeException("totalLastRow");

            return new SpeciesList(named, true, totalFirstRow, totalLastRow, totalCell, string.Empty);
        }

        public static SpeciesList WithNoTotal(IEnumerable<SpeciesListRow> named)
        {
            return new SpeciesList(named, false, 0, 0, null, string.Empty);
        }

        /// <summary>
        /// Column D of one tree list sheet, every row of it, and the total in column B.
        /// </summary>
        public static SpeciesList In(string path, TreeSheet sheet)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (sheet == null) throw new ArgumentNullException("sheet");

            try
            {
                using (FileStream reading = File.OpenRead(path))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null)
                    {
                        return Refused("No workbook part in " + Path.GetFileName(path) + ".");
                    }

                    Dictionary<string, string> sheetParts = WorkbookPackage.SheetParts(zip, workbookPart);
                    string partPath;
                    if (!sheetParts.TryGetValue(sheet.SheetName, out partPath))
                    {
                        return Refused("No sheet named " + sheet.SheetName + " in "
                            + Path.GetFileName(path) + ".");
                    }

                    XDocument part = WorkbookPackage.Read(zip, partPath);
                    if (part == null)
                    {
                        return Refused("The sheet part for " + sheet.SheetName + " in "
                            + Path.GetFileName(path) + " could not be read.");
                    }

                    IEnumerable<SpeciesListRow> named = NamesByRow(part, SharedStrings(zip))
                        .Select(one => new SpeciesListRow(one.Key, one.Value));

                    int first;
                    int last;
                    string cell;
                    return QuantityTotal(part, out first, out last, out cell)
                        ? Holding(named, first, last, cell)
                        : WithNoTotal(named);
                }
            }
            catch (IOException failed)
            {
                return Refused(Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return Refused(Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return Refused(Path.GetFileName(path) + " is not a readable workbook: " + failed.Message);
            }
        }

        /// <summary>
        /// Every row of column D that names something, over the whole sheet. Where the list
        /// stops is decided afterwards by reading down, never by a range.
        /// </summary>
        private static IDictionary<int, string> NamesByRow(XDocument sheet, List<string> shared)
        {
            var found = new Dictionary<int, string>();

            foreach (XElement cell in Cells(sheet))
            {
                CellRef where = CellRef.TryParse((string)cell.Attribute("r"));
                if (where == null) continue;
                if (!string.Equals(where.Column, KpiTemplates.BotanicalColumn, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string text = TextOf(cell, shared);
                if (string.IsNullOrWhiteSpace(text)) continue;

                found[where.Row] = text.Trim();
            }

            return found;
        }

        /// <summary>
        /// The first and last row the quantity column's total sums, off a cell in that column
        /// holding SUM over that same column, and which cell it is. MOSQUES holds B93 as
        /// SUM(B4:B92). The total is found by its own formula rather than by a row number,
        /// because a row number would be a second copy of something the file already says.
        /// </summary>
        private static bool QuantityTotal(XDocument sheet, out int first, out int last, out string cell)
        {
            first = 0;
            last = 0;
            cell = string.Empty;

            var summing = new Regex(
                @"^\s*SUM\s*\(\s*\$?" + KpiTemplates.QuantityColumn + @"\$?(\d+)\s*:\s*\$?"
                    + KpiTemplates.QuantityColumn + @"\$?(\d+)\s*\)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            foreach (XElement candidate in Cells(sheet))
            {
                CellRef where = CellRef.TryParse((string)candidate.Attribute("r"));
                if (where == null) continue;
                if (!string.Equals(where.Column, KpiTemplates.QuantityColumn, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                XElement formula = candidate.Elements().FirstOrDefault(child => child.Name.LocalName == "f");
                if (formula == null) continue;

                Match found = summing.Match(formula.Value ?? string.Empty);
                if (!found.Success) continue;

                if (!int.TryParse(found.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out first)) continue;
                if (!int.TryParse(found.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out last)) continue;
                if (last < first) continue;

                cell = where.ToString();
                return true;
            }

            return false;
        }

        private static IEnumerable<XElement> Cells(XDocument sheet)
        {
            return sheet.Root.Elements()
                .Where(element => element.Name.LocalName == "sheetData")
                .Elements().Where(element => element.Name.LocalName == "row")
                .Elements().Where(element => element.Name.LocalName == "c");
        }

        private static string TextOf(XElement cell, List<string> shared)
        {
            XElement inline = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "is");
            if (inline != null)
            {
                return string.Concat(inline.Descendants()
                    .Where(child => child.Name.LocalName == "t").Select(child => child.Value));
            }

            XElement value = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "v");
            if (value == null) return string.Empty;

            if (!string.Equals((string)cell.Attribute("t"), "s", StringComparison.Ordinal))
            {
                return value.Value;
            }

            int index;
            if (!int.TryParse(value.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
            {
                return string.Empty;
            }

            return index >= 0 && index < shared.Count ? shared[index] : string.Empty;
        }

        private static List<string> SharedStrings(ZipArchive zip)
        {
            var held = new List<string>();

            XDocument table = WorkbookPackage.Read(zip, "xl/sharedStrings.xml");
            if (table == null || table.Root == null) return held;

            foreach (XElement item in table.Root.Elements().Where(one => one.Name.LocalName == "si"))
            {
                held.Add(string.Concat(item.Descendants()
                    .Where(child => child.Name.LocalName == "t").Select(child => child.Value)));
            }

            return held;
        }
    }
}
