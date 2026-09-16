using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Which column one tree list sheet's canopy TOTAL adds, and over which rows, read off the
    /// file rather than off a letter.
    /// </summary>
    public sealed class TotalCanopyColumn
    {
        private TotalCanopyColumn(
            string sheetName, string column, string totalCell, int firstRow, int lastRow, string why)
        {
            SheetName = (sheetName ?? string.Empty).Trim();
            Column = (column ?? string.Empty).Trim().ToUpperInvariant();
            TotalCell = (totalCell ?? string.Empty).Trim();
            FirstRow = firstRow;
            LastRow = lastRow;
            Why = (why ?? string.Empty).Trim();
        }

        public static TotalCanopyColumn Refused(string sheetName, string why)
        {
            if (string.IsNullOrWhiteSpace(why)) throw new ArgumentException("A refusal needs a reason.", "why");

            return new TotalCanopyColumn(sheetName, string.Empty, string.Empty, 0, 0, why);
        }

        public static TotalCanopyColumn Of(
            string sheetName, string column, string totalCell, int firstRow, int lastRow)
        {
            if (string.IsNullOrWhiteSpace(column)) throw new ArgumentException("A column is needed.", "column");
            if (firstRow < 1) throw new ArgumentOutOfRangeException("firstRow");
            if (lastRow < firstRow) throw new ArgumentOutOfRangeException("lastRow");

            return new TotalCanopyColumn(sheetName, column, totalCell, firstRow, lastRow, string.Empty);
        }

        public string SheetName { get; }

        /// <summary>The column letter, M on all seven measured templates. Empty on a refusal.</summary>
        public string Column { get; }

        /// <summary>Where the total sits, M102 or M93, so a line can point at it.</summary>
        public string TotalCell { get; }

        public int FirstRow { get; }

        public int LastRow { get; }

        public bool Found
        {
            get { return Column.Length > 0; }
        }

        public string Why { get; }

        public bool Adds(int row)
        {
            return Found && row >= FirstRow && row <= LastRow;
        }

        public string InWords
        {
            get
            {
                if (!Found) return SheetName + ": " + Why;

                return SheetName + ": the canopy total " + TotalCell + " adds column " + Column
                    + " over rows " + FirstRow.ToString(CultureInfo.InvariantCulture)
                    + " to " + LastRow.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// **A ROW HOLDING A COUNT MUST ALSO CARRY THE TOTAL CANOPY FORMULA IN THE COLUMN THE CANOPY
    /// TOTAL ADDS.** Bader's decision of 16 September.
    ///
    /// FP-18 has 2 Ziziphus spina-christi on Tree List - Existing row 83. L83 carries the canopy
    /// formula and M83 is empty in the EXISTING PARKS and FUTURE PARKS templates. The PDF says
    /// 2,631 m2 greened and 67.68 percent canopy, the recalculated Excel 2,531 and 63.97, and
    /// nothing warned. The canopy guard and the usable empty rows both looked at the canopy
    /// formula alone, so a row that computes a canopy per tree and adds none was invisible.
    ///
    /// **THE COLUMN IS READ OFF THE FILE AND NEVER OFF A LETTER.** Measured on all seven
    /// templates: the first tab's Canopy Area cell reads
    /// `='Tree List - Existing'!M102+'Tree List - Proposed'!M93`, M102 is `=SUM(M4:M101)` and M93
    /// is `=SUM(M4:M92)`. So the chain is the green cover cell, which names the canopy cell, which
    /// names the two totals, whose own SUM ranges name the column and the rows.
    ///
    /// **NOTHING HERE HOLDS M, OR F8, OR F9.** The two row layouts on the main sheet are exactly
    /// why, the lesson row 5 and row 7 both taught.
    /// </summary>
    public static class TotalCanopyColumns
    {
        public const string NoCanopyCell =
            "the workbook's Total Green cover cell does not name a canopy cell, so nothing says "
            + "which cell holds the canopy area or which column it adds";

        public const string NoCanopyFormula =
            "the canopy cell holds no formula, so nothing says which cells its total comes off";

        public static string NotTwoTotals(string canopyCell, string text, int reads)
        {
            return "the canopy cell " + canopyCell + " reads " + text + ", which takes "
                + reads.ToString(CultureInfo.InvariantCulture)
                + " cells, where this tool expects one on each tree list sheet";
        }

        public static string NoTotalOnThisSheet(string canopyCell)
        {
            return "the canopy cell " + canopyCell + " names no cell on this sheet, so nothing "
                + "says which column its canopy total adds";
        }

        public static string TotalIsNotASum(string totalCell, string text)
        {
            return "the canopy total " + totalCell + " reads " + (text.Length == 0 ? "nothing" : text)
                + ", where this tool expects a SUM over one column, so nothing says which column "
                + "it adds";
        }

        /// <summary>
        /// The two tree list sheets' total canopy columns, read out of one template.
        ///
        /// <paramref name="greenCover"/> is the labelled Total Green cover cell, read off the
        /// same template by the lookup every other labelled cell goes through, so no letter is
        /// written here.
        /// </summary>
        public static IReadOnlyList<TotalCanopyColumn> In(
            string path, KpiTemplate template, LabelledCell greenCover)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (template == null) throw new ArgumentNullException("template");

            var sheets = new[] { template.ExistingTrees.SheetName, template.ProposedTrees.SheetName };

            try
            {
                using (FileStream reading = File.OpenRead(path))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null)
                    {
                        return Every(sheets, "no workbook part in " + Path.GetFileName(path));
                    }

                    Dictionary<string, string> parts = WorkbookPackage.SheetParts(zip, workbookPart);

                    string partPath;
                    if (!parts.TryGetValue(template.MainSheetName, out partPath))
                    {
                        return Every(sheets, "the template holds no sheet named " + template.MainSheetName);
                    }

                    XDocument main = WorkbookPackage.Read(zip, partPath);
                    if (main == null)
                    {
                        return Every(sheets, "the main sheet " + template.MainSheetName + " could not be read");
                    }

                    IReadOnlyList<FormulaCell> onMain = WorkbookFormulas.Of(template.MainSheetName, main);

                    string canopyCell = CanopyCellOf(
                        onMain, template, greenCover, out string whyNoCanopy);
                    if (canopyCell.Length == 0) return Every(sheets, whyNoCanopy);

                    FormulaCell canopy = onMain.FirstOrDefault(
                        one => string.Equals(one.Cell, canopyCell, StringComparison.OrdinalIgnoreCase));
                    if (canopy == null) return Every(sheets, NoCanopyFormula);

                    var found = new List<TotalCanopyColumn>();
                    foreach (string sheetName in sheets)
                    {
                        found.Add(OneSheet(zip, parts, sheetName, canopyCell, canopy));
                    }

                    return found;
                }
            }
            catch (IOException failed)
            {
                return Every(sheets, Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return Every(sheets, Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return Every(sheets, Path.GetFileName(path) + " is not a readable workbook: " + failed.Message);
            }
        }

        /// <summary>
        /// The canopy cell, learnt off the green cover formula the same way
        /// <see cref="WorkbookArithmetic.GreenCoverCell"/> learns it: the one cell of the three
        /// that the template's own map does not name as the planting or the lawn.
        /// </summary>
        private static string CanopyCellOf(
            IReadOnlyList<FormulaCell> onMain, KpiTemplate template, LabelledCell greenCover, out string why)
        {
            why = string.Empty;

            if (greenCover == null || !greenCover.Found)
            {
                why = NoCanopyCell;
                return string.Empty;
            }

            FormulaCell cover = onMain.FirstOrDefault(
                one => string.Equals(one.Cell, greenCover.ValueCell, StringComparison.OrdinalIgnoreCase));
            if (cover == null)
            {
                why = NoCanopyCell;
                return string.Empty;
            }

            MappedCell planting = template.CellFor(KpiValue.Shrubs);
            MappedCell lawn = template.CellFor(KpiValue.Lawn);

            var reads = cover.SingleCellsRead
                .Where(one => string.Equals(one.SheetName, template.MainSheetName, StringComparison.OrdinalIgnoreCase))
                .Select(one => one.Cell.ToString())
                .ToList();

            var left = reads
                .Where(one => planting == null || !Same(one, planting.Cell))
                .Where(one => lawn == null || !Same(one, lawn.Cell))
                .ToList();

            if (left.Count != 1)
            {
                why = NoCanopyCell;
                return string.Empty;
            }

            return left[0];
        }

        /// <summary>
        /// One tree list sheet's column, off the cell the canopy formula names on that sheet and
        /// that cell's own SUM range.
        /// </summary>
        private static TotalCanopyColumn OneSheet(
            ZipArchive zip,
            Dictionary<string, string> parts,
            string sheetName,
            string canopyCell,
            FormulaCell canopy)
        {
            WorkbookCell total = canopy.SingleCellsRead.FirstOrDefault(
                one => string.Equals(one.SheetName, sheetName, StringComparison.OrdinalIgnoreCase));
            if (total == null)
            {
                return TotalCanopyColumn.Refused(sheetName, NoTotalOnThisSheet(canopyCell));
            }

            string partPath;
            if (!parts.TryGetValue(sheetName, out partPath))
            {
                return TotalCanopyColumn.Refused(sheetName, "the template holds no sheet named " + sheetName);
            }

            XDocument part = WorkbookPackage.Read(zip, partPath);
            if (part == null)
            {
                return TotalCanopyColumn.Refused(sheetName, "the sheet " + sheetName + " could not be read");
            }

            FormulaCell sum = WorkbookFormulas.Of(sheetName, part).FirstOrDefault(
                one => string.Equals(one.Cell, total.Cell.ToString(), StringComparison.OrdinalIgnoreCase));
            if (sum == null)
            {
                return TotalCanopyColumn.Refused(
                    sheetName, TotalIsNotASum(total.Cell.ToString(), string.Empty));
            }

            System.Text.RegularExpressions.Match over = OverOneColumn.Match(sum.Text);
            if (!over.Success)
            {
                return TotalCanopyColumn.Refused(sheetName, TotalIsNotASum(total.Cell.ToString(), sum.Text));
            }

            string column = over.Groups[1].Value.ToUpperInvariant();
            if (!string.Equals(column, over.Groups[3].Value.ToUpperInvariant(), StringComparison.Ordinal))
            {
                return TotalCanopyColumn.Refused(sheetName, TotalIsNotASum(total.Cell.ToString(), sum.Text));
            }

            int first;
            int last;
            if (!int.TryParse(over.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out first)
                || !int.TryParse(over.Groups[4].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out last)
                || last < first)
            {
                return TotalCanopyColumn.Refused(sheetName, TotalIsNotASum(total.Cell.ToString(), sum.Text));
            }

            return TotalCanopyColumn.Of(sheetName, column, total.Cell.ToString(), first, last);
        }

        private static readonly System.Text.RegularExpressions.Regex OverOneColumn =
            new System.Text.RegularExpressions.Regex(
                @"^\s*SUM\s*\(\s*\$?([A-Z]{1,3})\$?(\d+)\s*:\s*\$?([A-Z]{1,3})\$?(\d+)\s*\)\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
                | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        private static IReadOnlyList<TotalCanopyColumn> Every(IEnumerable<string> sheets, string why)
        {
            return sheets.Select(one => TotalCanopyColumn.Refused(one, why)).ToList();
        }

        /// <summary>The column this sheet's canopy total adds, or a refusal naming why not.</summary>
        public static TotalCanopyColumn For(IEnumerable<TotalCanopyColumn> columns, string sheetName)
        {
            TotalCanopyColumn found = (columns ?? Enumerable.Empty<TotalCanopyColumn>())
                .FirstOrDefault(one => string.Equals(one.SheetName, sheetName, StringComparison.Ordinal));

            return found ?? TotalCanopyColumn.Refused(
                sheetName ?? string.Empty, "nothing read this sheet's canopy total");
        }

        private static bool Same(string one, string other)
        {
            return string.Equals(
                (one ?? string.Empty).Replace("$", string.Empty),
                (other ?? string.Empty).Replace("$", string.Empty),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
