using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell of one tree list sheet that this check names, with what is wrong with it.
    /// </summary>
    public sealed class TreeListFault
    {
        public TreeListFault(string kind, string cell, string why)
        {
            Kind = (kind ?? string.Empty).Trim();
            Cell = (cell ?? string.Empty).Trim();
            Why = (why ?? string.Empty).Trim();
        }

        /// <summary>Which of the seven questions named this cell.</summary>
        public string Kind { get; }

        public string Cell { get; }

        public string Why { get; }

        public string InWords
        {
            get { return Cell + ": " + Why; }
        }
    }

    /// <summary>
    /// One tree list sheet of one template, checked.
    /// </summary>
    public sealed class TreeListSheetCheck
    {
        public TreeListSheetCheck(
            string templateName,
            string sheetName,
            IEnumerable<TreeListFault> faults,
            IEnumerable<string> notRead)
        {
            TemplateName = (templateName ?? string.Empty).Trim();
            SheetName = (sheetName ?? string.Empty).Trim();
            Faults = (faults ?? Enumerable.Empty<TreeListFault>()).Where(one => one != null).ToList();
            NotRead = (notRead ?? Enumerable.Empty<string>())
                .Where(one => !string.IsNullOrWhiteSpace(one))
                .ToList();
        }

        public string TemplateName { get; }

        public string SheetName { get; }

        public IReadOnlyList<TreeListFault> Faults { get; }

        /// <summary>
        /// Every column or read this check could NOT make, named. **A column that cannot be read
        /// is named as not read, never skipped in silence**, because a question nobody asked and
        /// a question answered clean read the same in a count.
        /// </summary>
        public IReadOnlyList<string> NotRead { get; }

        public bool Clean
        {
            get { return Faults.Count == 0 && NotRead.Count == 0; }
        }

        public string InWords
        {
            get
            {
                if (Clean) return SheetName + ": clean";

                // **NO CELL NAMED IS NOT THE SAME SENTENCE AS A COUNT OF NOUGHT**, which is the
                // rule every other heading in this report already follows.
                return SheetName + ": "
                    + (Faults.Count == 0
                        ? "no cell named"
                        : Faults.Count + (Faults.Count == 1 ? " cell named" : " cells named"))
                    + (NotRead.Count == 0
                        ? string.Empty
                        : ", and " + NotRead.Count
                            + (NotRead.Count == 1 ? " check" : " checks") + " could not be made");
            }
        }
    }

    /// <summary>
    /// **NONE OF THIS SHOWED UNTIL A PLOT HIT A BAD ROW.** Measured in the 16 September
    /// workbooks, after the team's template edits of 15 September:
    ///
    /// <code>
    /// Tree List - Existing L85, L88 and L90 to L101 still typed numbers  EXISTING PARKS, STREETS
    /// M83 empty                                                         EXISTING PARKS, FUTURE PARKS
    /// O90 to O94 and O96 to O100 empty, N85 and N88 to N101 empty       all seven
    /// L101 typed                                                        MOSQUES
    /// Tree List - Proposed L84 to L92 deleted                           EXISTING PARKS, FUTURE PARKS
    /// the Native and Adaptive SUMIF formulas stop at row 91 and 95      while the lists end at 92 and 101
    /// V4 to V57, S35 to S43, S69 to T70 and the average canopy read rows 4 to 83 only
    /// </code>
    ///
    /// Every one of those is a cell a plot would have to land on before anybody heard about it,
    /// and a press over 154 plots lands on a different set every time the model changes. **This
    /// reads both tree lists of each ticked template ONCE, at the press, before any plot is
    /// written, and names every cell.**
    ///
    /// **IT IS A REPORT SECTION ONLY.** It stops no write, beyond what the canopy guard and the
    /// total canopy check already stop, which is unchanged.
    ///
    /// **EVERY COLUMN IS READ OFF THE FILE AND NEVER FROM A LETTER**, which is the rule this
    /// repository has paid for at row 5, row 7 and the two DIAMETER headings. The canopy column
    /// comes off the sheet's own heading row through <see cref="SpeciesList"/>, the total canopy
    /// column off the canopy total's chain through <see cref="TotalCanopyColumns"/>, and the
    /// water pair off the sheet's own formulas: a column whose rows multiply another column by
    /// the count, which is not the canopy pair.
    /// </summary>
    public static class TreeListCheck
    {
        public const string Heading = "THE TEMPLATES' OWN TREE LISTS, CELL BY CELL";

        public const string TypedCanopy = "the canopy cell is typed or missing";

        public const string MissingTotalCanopy = "the total canopy cell is missing";

        public const string MissingTotalWater = "the total water cell is missing";

        public const string EmptyWaterPerTree = "the water per tree cell is empty";

        public const string UnusableEmptyRow = "an empty row cannot take a new species";

        public const string NameOnTwoRows = "this name is on more than one row";

        public const string RangeStopsShort = "a formula reads a range that stops before the list ends";

        public const string NoCanopyColumn =
            "the canopy column could not be read off this sheet, so no row's canopy cell was checked";

        public const string NoRowCarriesTheCanopyFormula =
            "no row of this sheet carries the canopy formula, so which column holds it is UNKNOWN "
            + "and neither the canopy cells nor the total canopy cells were checked";

        public const string NoWaterColumns =
            "no column on this sheet multiplies another column by the count except the canopy, "
            + "so the water pair could not be read and no row's water cells were checked";

        /// <summary>
        /// **THE SHAPE EVERY ONE OF THESE COLUMNS CARRIES**, measured on all seven templates:
        /// `IF(ISBLANK(B85)," ",L85*B85)` for the canopy area and `IF(ISBLANK(B85)," ",N85*B85)`
        /// for the water. The count column is the template's own, never a letter here, and the
        /// two letters in the middle are what this finds rather than what it knows.
        /// </summary>
        private static readonly Regex ProductOfARowsColumn = new Regex(
            @"^IF\(ISBLANK\(\$?(?<count>[A-Z]{1,3})\$?(?<countRow>\d+)\)," +
            @"""\s*"",\$?(?<factor>[A-Z]{1,3})\$?(?<factorRow>\d+)\*\$?(?<other>[A-Z]{1,3})\$?(?<otherRow>\d+)\)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Both tree lists of one template, read once off the file.
        ///
        /// The two <see cref="SpeciesList"/> arguments are the ones the press already read for
        /// that template, so the names, the rows, the total's reach and the canopy column are not
        /// read a second way. <paramref name="totalCanopy"/> is the same for the total canopy
        /// column.
        /// </summary>
        public static IReadOnlyList<TreeListSheetCheck> In(
            string path,
            KpiTemplate template,
            IEnumerable<TotalCanopyColumn> totalCanopy,
            SpeciesList existing,
            SpeciesList proposed)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (template == null) throw new ArgumentNullException("template");

            var sheets = new[]
            {
                new KeyValuePair<TreeSheet, SpeciesList>(template.ExistingTrees, existing),
                new KeyValuePair<TreeSheet, SpeciesList>(template.ProposedTrees, proposed)
            };

            try
            {
                using (FileStream reading = File.OpenRead(path))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null)
                    {
                        return Every(template, sheets, "no workbook part in " + Path.GetFileName(path));
                    }

                    Dictionary<string, string> parts = WorkbookPackage.SheetParts(zip, workbookPart);

                    // **EVERY FORMULA IN THE WHOLE FILE, ONCE.** The seventh question is about
                    // formulas on the tree lists AND on the first tab, so a read of one sheet
                    // could not answer it.
                    var everywhere = new List<FormulaCell>();
                    foreach (KeyValuePair<string, string> one in parts)
                    {
                        XDocument part = WorkbookPackage.Read(zip, one.Value);
                        if (part == null) continue;

                        everywhere.AddRange(WorkbookFormulas.Of(one.Key, part));
                    }

                    var found = new List<TreeListSheetCheck>();
                    foreach (KeyValuePair<TreeSheet, SpeciesList> sheet in sheets)
                    {
                        found.Add(OneSheet(
                            zip, parts, template, sheet.Key, sheet.Value,
                            TotalCanopyColumns.For(totalCanopy, sheet.Key.SheetName),
                            everywhere));
                    }

                    return found;
                }
            }
            catch (IOException failed)
            {
                return Every(template, sheets, Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return Every(template, sheets, Path.GetFileName(path) + " could not be opened: " + failed.Message);
            }
            catch (InvalidDataException failed)
            {
                return Every(template, sheets, Path.GetFileName(path) + " is not a readable workbook: " + failed.Message);
            }
            catch (XmlException failed)
            {
                // **AUDIT 4 FINDING 67**, caught here for the same reason as in the four readers
                // beside it: one malformed sheet part used to stop the whole press naming no file.
                return Every(template, sheets, "a sheet part of " + Path.GetFileName(path)
                    + " is not well formed XML: " + failed.Message);
            }
        }

        private static IReadOnlyList<TreeListSheetCheck> Every(
            KpiTemplate template, IEnumerable<KeyValuePair<TreeSheet, SpeciesList>> sheets, string why)
        {
            return sheets
                .Select(one => new TreeListSheetCheck(
                    template.Name, one.Key.SheetName, null, new[] { why }))
                .ToList();
        }

        private static TreeListSheetCheck OneSheet(
            ZipArchive zip,
            Dictionary<string, string> parts,
            KpiTemplate template,
            TreeSheet sheet,
            SpeciesList list,
            TotalCanopyColumn totalCanopy,
            IReadOnlyList<FormulaCell> everywhere)
        {
            var faults = new List<TreeListFault>();
            var notRead = new List<string>();

            string partPath;
            if (!parts.TryGetValue(sheet.SheetName, out partPath))
            {
                return new TreeListSheetCheck(
                    template.Name, sheet.SheetName, null,
                    new[] { "the template holds no sheet named " + sheet.SheetName });
            }

            if (list == null || !list.WasRead)
            {
                return new TreeListSheetCheck(
                    template.Name, sheet.SheetName, null,
                    new[]
                    {
                        "this sheet's species list was not read, so its rows and its total's "
                        + "reach are UNKNOWN and no cell of it was checked"
                        + (list == null ? string.Empty : ": " + list.Refusal)
                    });
            }

            List<FormulaCell> onTheSheet = everywhere
                .Where(one => string.Equals(one.SheetName, sheet.SheetName, StringComparison.Ordinal))
                .ToList();

            var byCell = new Dictionary<string, FormulaCell>(StringComparer.OrdinalIgnoreCase);
            foreach (FormulaCell one in onTheSheet) byCell[one.Cell] = one;

            IReadOnlyDictionary<string, string> typed = WorkbookPackage.CellTexts(
                zip, partPath,
                Wanted(list, totalCanopy, WaterPair(onTheSheet, totalCanopy),
                    CanopyColumnOn(onTheSheet, list)),
                WorkbookPackage.SharedStrings(zip));

            // **THE CANOPY COLUMN IS READ ONCE OFF THE SHEET AND BOTH QUESTIONS ASK IT.** The
            // total canopy formula reads the CANOPY column multiplied by the count, not the
            // diameter column, so the letter the canopy formulas really sit in is what question
            // 2 has to be given.
            string canopyColumn = CanopyColumnOn(onTheSheet, list);

            // 1. Rows the total reaches whose canopy cell is typed or missing.
            if (list.DiameterColumn.Length == 0)
            {
                notRead.Add(NoCanopyColumn);
            }
            else if (canopyColumn.Length == 0)
            {
                notRead.Add(NoRowCarriesTheCanopyFormula);
            }
            else
            {
                foreach (int row in NamedRowsTheTotalReaches(list))
                {
                    string cell = canopyColumn + row.ToString(CultureInfo.InvariantCulture);

                    FormulaCell held;
                    if (byCell.TryGetValue(cell, out held)
                        && WorkbookArithmetic.IsTheCanopyFormula(held.Text, list.DiameterColumn, row))
                    {
                        continue;
                    }

                    faults.Add(new TreeListFault(TypedCanopy, cell, WhatItHolds(byCell, typed, cell)));
                }
            }

            // 2. Rows whose total canopy cell is missing.
            if (!totalCanopy.Found)
            {
                notRead.Add("the total canopy column could not be read: " + totalCanopy.Why);
            }
            else if (canopyColumn.Length == 0)
            {
                notRead.Add(NoRowCarriesTheCanopyFormula);
            }
            else
            {
                foreach (int row in NamedRowsTheTotalReaches(list))
                {
                    if (!totalCanopy.Adds(row)) continue;

                    string cell = totalCanopy.Column + row.ToString(CultureInfo.InvariantCulture);

                    FormulaCell held;
                    if (byCell.TryGetValue(cell, out held)
                        && WorkbookArithmetic.IsTheTotalCanopyFormula(
                            held.Text, canopyColumn, KpiTemplates.QuantityColumn, row))
                    {
                        continue;
                    }

                    faults.Add(new TreeListFault(
                        MissingTotalCanopy, cell, WhatItHolds(byCell, typed, cell)));
                }
            }

            // 3 and 4. The water pair, read off the sheet's own formulas.
            WaterColumns water = WaterPair(onTheSheet, totalCanopy);
            if (!water.Found)
            {
                notRead.Add(NoWaterColumns);
            }
            else
            {
                foreach (int row in NamedRowsTheTotalReaches(list))
                {
                    string total = water.TotalColumn + row.ToString(CultureInfo.InvariantCulture);
                    if (!byCell.ContainsKey(total))
                    {
                        faults.Add(new TreeListFault(
                            MissingTotalWater, total, WhatItHolds(byCell, typed, total)));
                    }

                    string each = water.PerTreeColumn + row.ToString(CultureInfo.InvariantCulture);
                    if (!byCell.ContainsKey(each) && !typed.ContainsKey(each))
                    {
                        faults.Add(new TreeListFault(
                            EmptyWaterPerTree, each,
                            "it is empty, and it is the column " + total + " multiplies by the count"));
                    }
                }
            }

            // 5. Empty rows a new species cannot be written into.
            foreach (int row in list.EmptyRows)
            {
                if (list.UsableEmptyRows.Contains(row)) continue;

                faults.Add(new TreeListFault(
                    UnusableEmptyRow,
                    KpiTemplates.QuantityColumn + row.ToString(CultureInfo.InvariantCulture),
                    "the total reaches this row and it carries neither the canopy formula nor "
                    + "the total canopy formula, so no species the list does not hold can go in it"));
            }

            // 6. Names held on more than one row.
            foreach (IGrouping<string, SpeciesListRow> same in list.Rows
                .GroupBy(one => one.BotanicalName.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1))
            {
                faults.Add(new TreeListFault(
                    NameOnTwoRows,
                    string.Join(" and ", same
                        .Select(one => "D" + one.Row.ToString(CultureInfo.InvariantCulture))
                        .ToArray()),
                    same.Key + " is on " + same.Count()
                        + " rows, so nothing can say which row a count belongs on"));
            }

            // 7. Formulas reading a range of this list that stops before its last row.
            foreach (FormulaCell one in everywhere)
            {
                foreach (CellArea area in one.Reads)
                {
                    if (area.IsOneCell) continue;
                    if (!string.Equals(area.SheetName, sheet.SheetName, StringComparison.OrdinalIgnoreCase)
                        && area.SheetName.Length > 0)
                    {
                        continue;
                    }

                    if (area.SheetName.Length == 0
                        && !string.Equals(one.SheetName, sheet.SheetName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // **THE LIST'S LAST NAMED ROW IS WHAT A RANGE HAS TO REACH.** The names
                    // run to 101 on the MOSQUES existing list and a SUMIF stopping at 91 leaves
                    // ten species out of its own count.
                    int lastNamed = list.Rows.Count == 0 ? 0 : list.Rows[list.Rows.Count - 1].Row;
                    if (lastNamed == 0) continue;

                    if (area.FirstRow > list.TotalFirstRow) continue;
                    if (area.LastRow >= lastNamed) continue;

                    faults.Add(new TreeListFault(
                        RangeStopsShort,
                        one.SheetName + " " + one.Cell,
                        "it reads " + CellArea.Letters(area.FirstColumn)
                            + area.FirstRow.ToString(CultureInfo.InvariantCulture) + " to "
                            + CellArea.Letters(area.LastColumn)
                            + area.LastRow.ToString(CultureInfo.InvariantCulture)
                            + " and this list names a species as far as row "
                            + lastNamed.ToString(CultureInfo.InvariantCulture)
                            + ", so every row past the range is left out of it"));
                }
            }

            return new TreeListSheetCheck(template.Name, sheet.SheetName, faults, notRead);
        }

        /// <summary>
        /// Every row the total reaches that names a species. A row naming nothing is question 5's
        /// and is not asked for a canopy cell it has no reason to carry.
        /// </summary>
        private static IEnumerable<int> NamedRowsTheTotalReaches(SpeciesList list)
        {
            return list.Rows
                .Select(one => one.Row)
                .Where(row => list.TotalFound && row >= list.TotalFirstRow && row <= list.TotalLastRow)
                .OrderBy(row => row);
        }

        /// <summary>
        /// The column a canopy formula really sits in somewhere on this sheet, which is the rule
        /// <see cref="WorkbookArithmetic"/> already follows, and empty where no row of the sheet
        /// carries one.
        /// </summary>
        private static string CanopyColumnOn(IReadOnlyList<FormulaCell> onTheSheet, SpeciesList list)
        {
            FormulaCell any = onTheSheet.FirstOrDefault(one =>
                WorkbookArithmetic.IsTheCanopyFormula(
                    one.Text, list.DiameterColumn, CellRef.Parse(one.Cell).Row));

            return any == null ? string.Empty : CellRef.Parse(any.Cell).Column;
        }

        /// <summary>
        /// The two water columns, read off the sheet's own formulas: a column whose rows are
        /// another column multiplied by the count, and which is NOT the canopy pair.
        ///
        /// **NOTHING HERE HOLDS N OR O.** The measured shape is
        /// `O85 = IF(ISBLANK(B85)," ",N85*B85)`, and the canopy's is the same shape two columns
        /// left, so the canopy total's own column is what tells them apart.
        /// </summary>
        private static WaterColumns WaterPair(
            IReadOnlyList<FormulaCell> onTheSheet, TotalCanopyColumn totalCanopy)
        {
            foreach (FormulaCell one in onTheSheet)
            {
                Match match = ProductOfARowsColumn.Match(one.Text ?? string.Empty);
                if (!match.Success) continue;

                string product = CellRef.Parse(one.Cell).Column;
                if (totalCanopy.Found
                    && string.Equals(product, totalCanopy.Column, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return new WaterColumns(product, match.Groups["factor"].Value.ToUpperInvariant());
            }

            return new WaterColumns(string.Empty, string.Empty);
        }

        /// <summary>
        /// Every cell this check may want to read a typed value out of. **NO COLUMN LETTER IS
        /// WRITTEN IN HERE**: each one was read off the file by the caller.
        /// </summary>
        private static IEnumerable<string> Wanted(
            SpeciesList list, TotalCanopyColumn totalCanopy, WaterColumns water, string canopyColumn)
        {
            var columns = new List<string>();
            if (totalCanopy.Found) columns.Add(totalCanopy.Column);
            if (water.Found)
            {
                columns.Add(water.TotalColumn);
                columns.Add(water.PerTreeColumn);
            }

            if (canopyColumn.Length > 0) columns.Add(canopyColumn);

            foreach (int row in list.Rows.Select(one => one.Row))
            {
                foreach (string column in columns)
                {
                    yield return column + row.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private static string WhatItHolds(
            IReadOnlyDictionary<string, FormulaCell> byCell,
            IReadOnlyDictionary<string, string> typed,
            string cell)
        {
            FormulaCell held;
            if (byCell.TryGetValue(cell, out held)) return "it holds " + held.Text;

            string text;
            if (typed.TryGetValue(cell, out text) && text.Length > 0)
            {
                return "it holds " + text + " and no formula";
            }

            return "it is empty";
        }

        private sealed class WaterColumns
        {
            public WaterColumns(string totalColumn, string perTreeColumn)
            {
                TotalColumn = (totalColumn ?? string.Empty).ToUpperInvariant();
                PerTreeColumn = (perTreeColumn ?? string.Empty).ToUpperInvariant();
            }

            public string TotalColumn { get; }

            public string PerTreeColumn { get; }

            public bool Found
            {
                get { return TotalColumn.Length > 0 && PerTreeColumn.Length > 0; }
            }
        }

        /// <summary>
        /// The one line per template the glance carries: clean, or how many cells are named.
        /// </summary>
        public static string InWords(string templateName, IEnumerable<TreeListSheetCheck> sheets)
        {
            List<TreeListSheetCheck> held = (sheets ?? Enumerable.Empty<TreeListSheetCheck>()).ToList();

            if (held.Count == 0) return templateName + ": its tree lists were not read.";

            if (held.All(one => one.Clean)) return templateName + ": clean.";

            int cells = held.Sum(one => one.Faults.Count);
            int could = held.Sum(one => one.NotRead.Count);

            return templateName + ": "
                + (cells == 0 ? "no cell named" : cells + (cells == 1 ? " cell named" : " cells named"))
                + (could == 0
                    ? string.Empty
                    : ", and " + could + (could == 1 ? " check" : " checks") + " could not be made")
                + ".";
        }
    }
}
