using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One label looked for on a template's main sheet, and the cell beside it that takes the
    /// value.
    /// </summary>
    public sealed class LabelledCell
    {
        private LabelledCell(string label, bool found, string labelCell, string valueCell, string holds, string why)
        {
            Label = label ?? string.Empty;
            Found = found;
            LabelCell = labelCell ?? string.Empty;
            ValueCell = valueCell ?? string.Empty;
            Holds = holds ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public string Label { get; }

        public bool Found { get; }

        /// <summary>
        /// Where the label itself sits, recorded because a value written into the wrong cell has
        /// to trace back to the label that chose it.
        /// </summary>
        public string LabelCell { get; }

        /// <summary>
        /// The cell immediately to the right of the label, which is the one written.
        /// </summary>
        public string ValueCell { get; }

        /// <summary>
        /// What that cell already holds, so the report can say a template came filled rather
        /// than reading as though this run put the value there.
        /// </summary>
        public string Holds { get; }

        public string Why { get; }

        public static LabelledCell At(string label, string labelCell, string valueCell, string holds)
        {
            return new LabelledCell(label, true, labelCell, valueCell, holds, string.Empty);
        }

        public static LabelledCell NotFound(string label, string why)
        {
            return new LabelledCell(label, false, string.Empty, string.Empty, string.Empty, why);
        }
    }

    /// <summary>
    /// The labels found on one template's main sheet, read off the file when Create is pressed.
    /// </summary>
    public sealed class LabelledCells
    {
        private LabelledCells(bool read, string sheetName, IEnumerable<LabelledCell> all, string why)
        {
            Read = read;
            SheetName = sheetName ?? string.Empty;
            All = (all ?? Enumerable.Empty<LabelledCell>()).ToList();
            Why = why ?? string.Empty;
        }

        public bool Read { get; }

        public string SheetName { get; }

        public IReadOnlyList<LabelledCell> All { get; }

        public string Why { get; }

        public static readonly LabelledCells NotRead =
            new LabelledCells(false, string.Empty, null, "the template was not opened");

        public static LabelledCells Refused(string why)
        {
            return new LabelledCells(false, string.Empty, null, why);
        }

        public static LabelledCells Holding(string sheetName, IEnumerable<LabelledCell> all)
        {
            return new LabelledCells(true, sheetName, all, string.Empty);
        }

        /// <summary>
        /// What was found for one label. A sheet that was never read answers not found with the
        /// read's own reason, so nothing downstream has to ask twice.
        /// </summary>
        public LabelledCell For(string label)
        {
            LabelledCell found = All.FirstOrDefault(
                one => string.Equals(one.Label, label, StringComparison.OrdinalIgnoreCase));

            return found ?? LabelledCell.NotFound(label, Read ? FixedCells.NoLabel(label, SheetName) : Why);
        }
    }

    /// <summary>
    /// The two cells that are the same on every plot of every template.
    ///
    /// **Bader's answer: Character is always Urban Area Zone and Context is always Urban.** They
    /// come from no model, no schedule and no file, so there is nothing to read and nothing to
    /// agree with. That makes them data, here, beside the reason.
    ///
    /// **THEY ARE FOUND BY THEIR LABELS ON THE TEMPLATE'S OWN SHEET AND NEVER BY A LETTER.**
    /// Measured on the three workbooks written on 13 September:
    ///
    /// <code>
    /// MOSQUES   Character label C7, value D7.   Context label E7, value F7.
    /// SCHOOLS   Character label C7, value D7.   Context label E7, value F7.
    /// STREETS   Category  label C7, value D7.   Character label E7, value F7.
    ///           Context   label G7, value H7.
    /// </code>
    ///
    /// **STREETS carries a Category at D7 holding a formula**, so its Character and Context sit
    /// one pair to the right, and a map that put Character at D7 because two templates do would
    /// overwrite that formula on the third. Nothing here holds a letter: the label is looked for
    /// on the sheet and the cell to its RIGHT is written, so the street's Category is never
    /// touched, because nothing looks for the word Category.
    ///
    /// A template naming neither label writes nothing and says so, and the same is true of one
    /// naming a label twice, because nothing says which of the two is meant.
    /// </summary>
    public static class FixedCells
    {
        public const string CharacterLabel = "Character";

        public const string ContextLabel = "Context";

        public const string CharacterValue = "Urban Area Zone";

        public const string ContextValue = "Urban";

        /// <summary>
        /// What the pane and the report say about where these two come from. They are the
        /// team's answer rather than anything the tool found, said the way the section depth and
        /// the annotation crop are said on the other tool.
        /// </summary>
        public const string SourceInWords =
            "the same on every plot, the team's answer rather than anything read, written into "
            + "the cell right of the label on the template's own sheet";

        public static string NoLabel(string label, string sheetName)
        {
            return "no cell on " + sheetName + " reads " + label
                + ", so nothing is written and no cell is guessed at";
        }

        public static string TwiceOn(string label, string sheetName, IEnumerable<string> cells)
        {
            return label + " is on " + sheetName + " at "
                + string.Join(" and ", (cells ?? Enumerable.Empty<string>()).ToArray())
                + ", and nothing says which is meant";
        }

        public static string ValueOf(KpiValue value)
        {
            return value == KpiValue.Character ? CharacterValue : ContextValue;
        }

        public static string LabelOf(KpiValue value)
        {
            return value == KpiValue.Character ? CharacterLabel : ContextLabel;
        }

        /// <summary>
        /// The two, in the order the report prints them.
        /// </summary>
        public static readonly IReadOnlyList<KpiValue> Both =
            new[] { KpiValue.Character, KpiValue.Context };

        public static readonly IReadOnlyList<string> Labels =
            new[] { CharacterLabel, ContextLabel };

        /// <summary>
        /// The labels on one template's main sheet, read off the file. **A label is matched whole,
        /// without case and with edge whitespace off**, so Characteristics is not Character.
        /// </summary>
        public static LabelledCells In(string path, KpiTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (string.IsNullOrWhiteSpace(path)) return LabelledCells.NotRead;

            try
            {
                using (var reading = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null) return LabelledCells.Refused("the template is not a workbook this tool can open");

                    Dictionary<string, string> sheets = WorkbookPackage.SheetParts(zip, workbookPart);
                    string part;
                    if (!sheets.TryGetValue(template.MainSheetName, out part))
                    {
                        return LabelledCells.Refused(
                            "the template holds no sheet named " + template.MainSheetName);
                    }

                    XDocument sheet = WorkbookPackage.Read(zip, part);
                    if (sheet == null) return LabelledCells.Refused("the main sheet could not be read");

                    return Off(template.MainSheetName, sheet, WorkbookPackage.SharedStrings(zip));
                }
            }
            catch (IOException failed)
            {
                return LabelledCells.Refused("the template could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return LabelledCells.Refused("the template could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return LabelledCells.Refused("the template is not a readable .xlsx: " + failed.Message);
            }
        }

        internal static LabelledCells Off(string sheetName, XDocument sheet, List<string> shared)
        {
            var texts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (XElement cell in WorkbookPackage.Cells(sheet))
            {
                XAttribute reference = cell.Attribute("r");
                if (reference == null) continue;
                if (CellRef.TryParse(reference.Value) == null) continue;

                texts[reference.Value] = (WorkbookPackage.TextOf(cell, shared) ?? string.Empty).Trim();
            }

            var found = new List<LabelledCell>();
            foreach (string label in Labels)
            {
                string[] at = texts
                    .Where(one => string.Equals(one.Value, label, StringComparison.OrdinalIgnoreCase))
                    .Select(one => one.Key)
                    .OrderBy(one => CellRef.Parse(one).Row)
                    .ThenBy(one => CellRef.Parse(one).ColumnNumber)
                    .ToArray();

                if (at.Length == 0)
                {
                    found.Add(LabelledCell.NotFound(label, NoLabel(label, sheetName)));
                    continue;
                }

                if (at.Length > 1)
                {
                    found.Add(LabelledCell.NotFound(label, TwiceOn(label, sheetName, at)));
                    continue;
                }

                CellRef where = CellRef.Parse(at[0]);
                string valueCell = RightOf(where);

                string holds;
                found.Add(LabelledCell.At(
                    label, at[0], valueCell,
                    texts.TryGetValue(valueCell, out holds) ? holds : string.Empty));
            }

            return LabelledCells.Holding(sheetName, found);
        }

        /// <summary>
        /// The cell one column to the right of a label, in the sheet's own column letters. A is
        /// 1, so C is 3 and the cell to its right is D.
        /// </summary>
        public static string RightOf(CellRef label)
        {
            if (label == null) throw new ArgumentNullException("label");

            int number = label.ColumnNumber + 1;
            string column = string.Empty;
            while (number > 0)
            {
                int letter = (number - 1) % 26;
                column = (char)('A' + letter) + column;
                number = (number - 1) / 26;
            }

            return column + label.Row;
        }
    }
}
