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
    /// The species list a tree list sheet already holds, read out of the template.
    ///
    /// The workbook's own column D is the only list there is. Nothing in this repo carries a
    /// copy of it, because a plant palette written into the code would be a second record of
    /// the client's list and would be wrong the first time they change it.
    ///
    /// A cell holding a shared string stores an index rather than the text, so the shared
    /// string table is read as well. Reading the index and treating it as the name would match
    /// nothing and report every species as unmatched, which reads exactly like a workbook whose
    /// list is empty.
    /// </summary>
    public sealed class SpeciesList
    {
        private SpeciesList(IReadOnlyList<SpeciesListRow> rows, IReadOnlyList<int> emptyRows, string refusal)
        {
            Rows = rows;
            EmptyRows = emptyRows ?? new List<int>();
            Refusal = refusal ?? string.Empty;
        }

        public IReadOnlyList<SpeciesListRow> Rows { get; }

        /// <summary>
        /// The rows the quantity total sums that column D does not name, in order, for writing
        /// a species the list does not hold into.
        ///
        /// **THE MAP'S RANGE CANNOT SAY WHERE THESE ARE.** The MOSQUES map entry stops at row 83
        /// and the sheet's own total is SUM(B4:B92), so rows 84 to 92 are empty AND counted, and
        /// a range that ended at 83 would find no room at all. The total's own range is what
        /// says which rows reach it, and that is read off the file.
        ///
        /// Empty when the total could not be found, so a species is reported as not placed
        /// rather than written into a row nothing sums.
        /// </summary>
        public IReadOnlyList<int> EmptyRows { get; }

        public string Refusal { get; }

        public bool WasRead
        {
            get { return Refusal.Length == 0; }
        }

        public static SpeciesList Refused(string why)
        {
            return new SpeciesList(
                new List<SpeciesListRow>(), null, why ?? "The species list could not be read.");
        }

        public static SpeciesList Holding(IEnumerable<SpeciesListRow> rows, IEnumerable<int> emptyRows = null)
        {
            return new SpeciesList(
                (rows ?? Enumerable.Empty<SpeciesListRow>()).Where(one => one != null).ToList(),
                (emptyRows ?? Enumerable.Empty<int>()).ToList(),
                string.Empty);
        }

        /// <summary>
        /// Column D of one tree list sheet, over the row range the map entry names. A row whose
        /// cell is empty is left out, so a list shorter than its range reads as the length it
        /// really is.
        /// </summary>
        public static SpeciesList In(string path, TreeRows rows)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (rows == null) throw new ArgumentNullException("rows");

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
                    if (!sheetParts.TryGetValue(rows.SheetName, out partPath))
                    {
                        return Refused("No sheet named " + rows.SheetName + " in "
                            + Path.GetFileName(path) + ".");
                    }

                    IDictionary<int, string> named = NamesByRow(zip, partPath, SharedStrings(zip));

                    return Holding(
                        named.Where(one => one.Key >= rows.FirstRow && one.Key <= rows.LastRow)
                            .OrderBy(one => one.Key)
                            .Select(one => new SpeciesListRow(one.Key, one.Value)),
                        EmptyRowsIn(zip, partPath, named));
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
        /// Every row of column D that names something, over the whole sheet rather than over the
        /// map's range, because the empty rows below the map's last row are the ones a species
        /// gets written into.
        /// </summary>
        private static IDictionary<int, string> NamesByRow(
            ZipArchive zip, string partPath, List<string> shared)
        {
            var found = new Dictionary<int, string>();

            XDocument sheet = WorkbookPackage.Read(zip, partPath);
            if (sheet == null) return found;

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
        /// The rows the quantity total sums that hold no name. The total is found by its own
        /// formula rather than by a row number, because a row number would be a second copy of
        /// something the file already says.
        /// </summary>
        private static List<int> EmptyRowsIn(
            ZipArchive zip, string partPath, IDictionary<int, string> named)
        {
            var free = new List<int>();

            XDocument sheet = WorkbookPackage.Read(zip, partPath);
            if (sheet == null) return free;

            int first;
            int last;
            if (!QuantityTotal(sheet, out first, out last)) return free;

            for (int row = first; row <= last; row++)
            {
                if (!named.ContainsKey(row)) free.Add(row);
            }

            return free;
        }

        /// <summary>
        /// The first and last row the quantity column's total sums, off a cell in that column
        /// holding SUM over that same column. MOSQUES holds B93 as SUM(B4:B92).
        /// </summary>
        private static bool QuantityTotal(XDocument sheet, out int first, out int last)
        {
            first = 0;
            last = 0;

            var summing = new Regex(
                @"^\s*SUM\s*\(\s*\$?" + KpiTemplates.QuantityColumn + @"\$?(\d+)\s*:\s*\$?"
                    + KpiTemplates.QuantityColumn + @"\$?(\d+)\s*\)\s*$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            foreach (XElement cell in Cells(sheet))
            {
                CellRef where = CellRef.TryParse((string)cell.Attribute("r"));
                if (where == null) continue;
                if (!string.Equals(where.Column, KpiTemplates.QuantityColumn, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                XElement formula = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "f");
                if (formula == null) continue;

                Match found = summing.Match(formula.Value ?? string.Empty);
                if (!found.Success) continue;

                if (!int.TryParse(found.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out first)) continue;
                if (!int.TryParse(found.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out last)) continue;
                if (last < first) continue;

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
