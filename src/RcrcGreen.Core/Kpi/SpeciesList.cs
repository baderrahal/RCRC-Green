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
    /// One row of a tree list sheet: the botanical name the workbook holds and the row it sits
    /// on. The quantity goes in column B of that row, so the row number is what a write needs.
    /// </summary>
    public sealed class SpeciesListRow
    {
        public SpeciesListRow(int row, string botanicalName, string height = null, string diameter = null)
        {
            if (row < 1) throw new ArgumentOutOfRangeException("row");
            if (botanicalName == null) throw new ArgumentNullException("botanicalName");

            Row = row;
            BotanicalName = botanicalName;
            Height = (height ?? string.Empty).Trim();
            Diameter = (diameter ?? string.Empty).Trim();
        }

        public int Row { get; }

        public string BotanicalName { get; }

        /// <summary>
        /// What the row already holds in the sheet's height column, as the cell prints it, so a
        /// species Revit measures differently can be named. Empty when the sheet names no such
        /// column or the cell is blank.
        /// </summary>
        public string Height { get; }

        public string Diameter { get; }
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
            string refusal,
            string heightColumn,
            string diameterColumn,
            string whyNoHeightColumn,
            string whyNoDiameterColumn,
            string heightColumnChosen = null,
            string diameterColumnChosen = null,
            IEnumerable<int> canopyRows = null,
            string fileName = null,
            IEnumerable<int> totalCanopyRows = null,
            TotalCanopyColumn totalCanopy = null)
        {
            TotalCanopy = totalCanopy;
            FileName = (fileName ?? string.Empty).Trim();
            Refusal = refusal ?? string.Empty;
            HeightColumn = heightColumn ?? string.Empty;
            DiameterColumn = diameterColumn ?? string.Empty;
            WhyNoHeightColumn = HeightColumn.Length > 0 ? string.Empty : (whyNoHeightColumn ?? NoColumn(ScheduleColumns.HeightWord));
            WhyNoDiameterColumn = DiameterColumn.Length > 0 ? string.Empty : (whyNoDiameterColumn ?? NoColumn(ScheduleColumns.DiameterWord));
            HeightColumnChosen = HeightColumn.Length > 0 ? (heightColumnChosen ?? TheOneColumn(ScheduleColumns.HeightWord)) : string.Empty;
            DiameterColumnChosen = DiameterColumn.Length > 0 ? (diameterColumnChosen ?? TheOneColumn(ScheduleColumns.DiameterWord)) : string.Empty;
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

            // **AN EMPTY ROW IS USABLE ONLY IF IT CARRIES THE CANOPY FORMULA THE GUARD LOOKS
            // FOR.** Measured on the 13:32 run: CONOCARPUS LANCIFOLIUS, 2 trees, went into
            // FUTURE PARKS Tree List - Proposed D85 and B85, a row holding C85, M85 and O85 and
            // no L85, so the guard blanked FP-23's Total areas to be greened and its canopy
            // percentage. A row the workbook cannot compute a canopy from is not an empty row
            // this tool may use.
            CanopyRowsRead = canopyRows != null;
            RowsWithTheCanopyFormula = (canopyRows ?? Enumerable.Empty<int>()).Distinct().OrderBy(one => one).ToList();

            // **AND THE TOTAL CANOPY FORMULA, which is the second half of the same rule.** FP-18
            // has 2 Ziziphus spina-christi on Tree List - Existing row 83, L83 carries the canopy
            // formula and M83 is empty on both park templates, so the row computes a canopy per
            // tree and adds none. Its PDF read 2,631 m2 greened against the workbook's 2,531 and
            // nothing warned.
            // **A COLUMN THE FILE REFUSED FOR A REAL REASON, or empty.** A template naming no
            // Total Green cover cell holds no canopy total at all, so a row of it reaches none
            // by construction and nothing is narrowed for it. Every other reason is a read that
            // failed on a template that does have one.
            TotalCanopyUnreadable = totalCanopy != null && !totalCanopy.Found
                && !totalCanopy.TheTemplateNamesNoGreenCover
                ? totalCanopy.Why
                : string.Empty;

            TotalCanopyRowsRead = totalCanopyRows != null;
            RowsWithTheTotalCanopyFormula =
                (totalCanopyRows ?? Enumerable.Empty<int>()).Distinct().OrderBy(one => one).ToList();

            // **WHERE NOBODY READ THE FORMULAS, EVERY EMPTY ROW IS STILL OFFERED AND THE FLAG
            // SAYS SO.** `In` always reads them, so this is the hand built list a test makes,
            // and a silent narrowing there would be a guard nobody could see working.
            //
            // **A COLUMN THE FILE REFUSED IS A DIFFERENT THING AND NARROWS EVERYTHING.** It was
            // read off a real file and the read failed, so nothing says whether an empty row of
            // this sheet carries the formula the canopy total adds, and offering one to a new
            // species would put a count where the workbook computes no canopy. That is the fault
            // FP-18 row 83 already cost once, one step further back.
            UsableEmptyRows = TotalCanopyUnreadable.Length > 0
                ? new List<int>()
                : EmptyRows
                    .Where(row => !CanopyRowsRead || RowsWithTheCanopyFormula.Contains(row))
                    .Where(row => !TotalCanopyRowsRead || RowsWithTheTotalCanopyFormula.Contains(row))
                    .ToList();
        }

        /// <summary>
        /// Which column this sheet's canopy total adds and over which rows, read off the file, or
        /// a refusal naming why not. Null on a list built by hand.
        /// </summary>
        public TotalCanopyColumn TotalCanopy { get; }

        /// <summary>
        /// Why the total canopy column of this sheet could not be read, or empty where it was
        /// read or where the template names no green cover cell at all.
        ///
        /// **WHILE IT IS SET, NO EMPTY ROW IS USABLE.** Nothing says whether an empty row carries
        /// the formula the canopy total adds, and a new species written into one would put a
        /// count where the workbook computes no canopy.
        /// </summary>
        public string TotalCanopyUnreadable { get; }

        /// <summary>
        /// Whether the sheet's total canopy formulas were read at all. **`In` always reads them
        /// where the template names a canopy total**, so this is false only for a list built by
        /// hand or a template whose chain could not be followed.
        /// </summary>
        public bool TotalCanopyRowsRead { get; }

        /// <summary>
        /// Every row of the sheet whose own cells carry the total canopy formula for that row, in
        /// order. Empty where they were not read.
        /// </summary>
        public IReadOnlyList<int> RowsWithTheTotalCanopyFormula { get; }

        /// <summary>
        /// The workbook this list was read out of, as its file name, so a line about a row can
        /// name the file the team has to open. Empty on a list built by hand.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Whether the sheet's formulas were read at all. **`In` always reads them**, so this is
        /// false only for a list built by hand.
        /// </summary>
        public bool CanopyRowsRead { get; }

        /// <summary>
        /// Every row of the sheet whose own cells carry the canopy formula for that row, in
        /// order. Empty where the formulas were not read.
        /// </summary>
        public IReadOnlyList<int> RowsWithTheCanopyFormula { get; }

        /// <summary>
        /// The empty rows a species the list does not hold may really be written into: the
        /// total reaches them AND the workbook can compute a canopy off them.
        /// </summary>
        public IReadOnlyList<int> UsableEmptyRows { get; }

        /// <summary>
        /// Why no empty row of this sheet could take a new species, naming every empty row that
        /// was checked. **A refusal that does not say which rows it looked at cannot be argued
        /// with**, and the rows are what the team's fix to the template has to reach.
        /// </summary>
        public string NoUsableEmptyRow(string sheetName, string diameterColumn)
        {
            string where = Where(sheetName);

            // **A COLUMN THE FILE REFUSED IS THE WHOLE REASON WHERE IT FIRES**, and the rows are
            // not described, because nothing read them. Saying that none of them carries the
            // formula would be a claim about rows this tool never looked at.
            if (TotalCanopyUnreadable.Length > 0)
            {
                return "in " + where + " the total canopy column could not be read, so nothing "
                    + "says whether an empty row carries the formula the canopy total adds and "
                    + "none of them is offered: " + TotalCanopyUnreadable;
            }

            if (EmptyRows.Count == 0)
            {
                return SpeciesMatching.NoEmptyRowLeft + ", in " + where;
            }

            return "in " + where + " the total reaches " + EmptyRows.Count
                + (EmptyRows.Count == 1 ? " empty row, " : " empty rows, ")
                + Numbered(EmptyRows) + ", and not one of them carries the canopy formula "
                + WorkbookArithmetic.CanopyColumn(diameterColumn, EmptyRows[0])
                + " for its own row, so a count written on any of them would leave the canopy "
                + "and the green cover short";
        }

        /// <summary>The file and the sheet, said the one way, for a line about a row.</summary>
        public string Where(string sheetName)
        {
            string sheet = (sheetName ?? string.Empty).Trim();

            return (FileName.Length == 0 ? string.Empty : FileName + ", ")
                + (sheet.Length == 0 ? "the tree list sheet" : sheet);
        }

        private static string Numbered(IEnumerable<int> rows)
        {
            return string.Join(", ", rows.Select(
                one => one.ToString(CultureInfo.InvariantCulture)).ToArray());
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

        /// <summary>
        /// The column whose heading on the header row holds HEIGHT, as its letter, or empty
        /// when the header row names none or names more than one. Measured I on MOSQUES, Mature
        /// Height (m), and found by that heading rather than by the letter.
        /// </summary>
        public string HeightColumn { get; }

        /// <summary>
        /// The same for DIAMETER. Measured J on MOSQUES, Average Mature Canopy Diameter (m),
        /// and what the sheet's canopy formula computes from.
        /// </summary>
        public string DiameterColumn { get; }

        public string WhyNoHeightColumn { get; }

        public string WhyNoDiameterColumn { get; }

        /// <summary>
        /// How the height column was chosen: the one column holding the word, or of two the
        /// one the sheet's own formulas read. Recorded as it is used, so the report can say
        /// it. Empty when no column was chosen.
        /// </summary>
        public string HeightColumnChosen { get; }

        public string DiameterColumnChosen { get; }

        public bool WasRead
        {
            get { return Refusal.Length == 0; }
        }

        public static string NoColumn(string word)
        {
            return "the sheet's header row, row " + KpiTemplates.TreeHeaderRow + ", names no column holding " + word;
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
            return new SpeciesList(null, false, 0, 0, null, why ?? "The species list could not be read.",
                null, null, null, null);
        }

        /// <summary>
        /// The same list with the rows carrying the canopy formula named. Used by a test that
        /// builds a sheet by hand; <see cref="In"/> reads them off the file.
        /// </summary>
        public static SpeciesList Holding(
            IEnumerable<SpeciesListRow> named,
            int totalFirstRow,
            int totalLastRow,
            IEnumerable<int> canopyRows,
            string heightColumn = null,
            string diameterColumn = null,
            string totalCell = null,
            string fileName = null)
        {
            if (totalFirstRow < 1) throw new ArgumentOutOfRangeException("totalFirstRow");
            if (totalLastRow < totalFirstRow) throw new ArgumentOutOfRangeException("totalLastRow");
            if (canopyRows == null) throw new ArgumentNullException("canopyRows");

            return new SpeciesList(named, true, totalFirstRow, totalLastRow, totalCell, string.Empty,
                heightColumn, diameterColumn, null, null, null, null, canopyRows, fileName);
        }

        /// <summary>
        /// A list as read: every named row, and the range the total reaches. The empty rows and
        /// the rows outside the total are worked out from those two and are not handed in, so
        /// nothing can state an empty row that is named or a named row the total reaches.
        /// </summary>
        public static SpeciesList Holding(
            IEnumerable<SpeciesListRow> named,
            int totalFirstRow,
            int totalLastRow,
            string totalCell = null,
            string heightColumn = null,
            string diameterColumn = null,
            string whyNoHeightColumn = null,
            string whyNoDiameterColumn = null,
            string heightColumnChosen = null,
            string diameterColumnChosen = null)
        {
            if (totalFirstRow < 1) throw new ArgumentOutOfRangeException("totalFirstRow");
            if (totalLastRow < totalFirstRow) throw new ArgumentOutOfRangeException("totalLastRow");

            return new SpeciesList(named, true, totalFirstRow, totalLastRow, totalCell, string.Empty,
                heightColumn, diameterColumn, whyNoHeightColumn, whyNoDiameterColumn,
                heightColumnChosen, diameterColumnChosen);
        }

        public static SpeciesList WithNoTotal(
            IEnumerable<SpeciesListRow> named, string heightColumn = null, string diameterColumn = null)
        {
            return new SpeciesList(named, false, 0, 0, null, string.Empty, heightColumn, diameterColumn, null, null);
        }

        /// <summary>
        /// Column D of one tree list sheet, every row of it, and the total in column B.
        /// </summary>
        public static SpeciesList In(string path, TreeSheet sheet)
        {
            return In(path, sheet, null);
        }

        /// <summary>
        /// The same read, with the column this sheet's canopy total adds handed in so the rows
        /// carrying the total canopy formula can be read too.
        /// </summary>
        public static SpeciesList In(string path, TreeSheet sheet, TotalCanopyColumn totalCanopy)
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

                    Dictionary<int, Dictionary<string, string>> cells = CellsByRow(part, WorkbookPackage.SharedStrings(zip));

                    // The height and the diameter columns are found by what the header row calls
                    // them, never by a letter. Measured I and J on MOSQUES, and the letters are
                    // written nowhere. Where two headings hold the word, the sheet's own formulas
                    // say which: J and K both hold DIAMETER on MOSQUES, and L reads J.
                    HashSet<string> read = ColumnsFormulasRead(part);
                    string whyNoHeight;
                    string heightChosen;
                    string heightColumn = ColumnHeaded(cells, read, ScheduleColumns.HeightWord, out whyNoHeight, out heightChosen);
                    string whyNoDiameter;
                    string diameterChosen;
                    string diameterColumn = ColumnHeaded(cells, read, ScheduleColumns.DiameterWord, out whyNoDiameter, out diameterChosen);

                    var named = new List<SpeciesListRow>();
                    foreach (KeyValuePair<int, Dictionary<string, string>> row in cells.OrderBy(one => one.Key))
                    {
                        string name = In(row.Value, KpiTemplates.BotanicalColumn);
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        named.Add(new SpeciesListRow(
                            row.Key, name.Trim(), In(row.Value, heightColumn), In(row.Value, diameterColumn)));
                    }

                    // **READ OFF THE FILE THROUGH THE ONE FORMULA READER**, so a shared formula
                    // carried on eighty dependent cells is seen on all eighty rather than on its
                    // master alone.
                    //
                    // **A SHEET NAMING NO DIAMETER COLUMN IS NOT CHECKED AT ALL.** The canopy
                    // formula is written around that column, so with none chosen there is no
                    // formula to look for, and calling every row unusable would refuse every
                    // write on a shape nobody has measured. It comes back as not read, the flag
                    // says so, and WhyNoDiameterColumn already carries the reason.
                    // **READ ONCE AND ASKED TWICE.** Both canopy reads walk the same formulas,
                    // and reading them per row would parse the sheet once for every row on it.
                    IReadOnlyList<FormulaCell> onTheSheet = WorkbookFormulas.Of(sheet.SheetName, part);

                    List<int> canopyRows = string.IsNullOrWhiteSpace(diameterColumn)
                        ? null
                        : CanopyRowsIn(onTheSheet, diameterColumn);

                    // **AND THE TOTAL CANOPY COLUMN'S OWN ROWS**, where the caller handed in the
                    // column the canopy total adds. A template whose chain could not be followed
                    // comes back not read, and the flag says so rather than the rule quietly
                    // narrowing to nothing.
                    List<int> totalCanopyRows =
                        totalCanopy == null || !totalCanopy.Found || string.IsNullOrWhiteSpace(diameterColumn)
                            ? null
                            : TotalCanopyRowsIn(onTheSheet, diameterColumn, totalCanopy);

                    string fileName = Path.GetFileName(path);

                    int first;
                    int last;
                    string cell;
                    return QuantityTotal(part, out first, out last, out cell)
                        ? new SpeciesList(named, true, first, last, cell, string.Empty,
                            heightColumn, diameterColumn, whyNoHeight, whyNoDiameter,
                            heightChosen, diameterChosen, canopyRows, fileName, totalCanopyRows, totalCanopy)
                        : new SpeciesList(named, false, 0, 0, null, string.Empty,
                            heightColumn, diameterColumn, whyNoHeight, whyNoDiameter,
                            heightChosen, diameterChosen, canopyRows, fileName, totalCanopyRows, totalCanopy);
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
            catch (XmlException failed)
            {
                // **AUDIT 4 FINDING 67.** Five classes open an .xlsx and only the two that WRITE
                // caught this. A template whose workbook part is fine and whose tree list sheet
                // part is malformed passes the peek, is recognised, is listed in the pane and is
                // offered, and the parse here threw past every catch in `Run` to the one that
                // catches everything, so the pane read that the request failed and was stopped,
                // naming no template, no file and no plot, with no report written at all.
                return Refused("The sheet part for " + sheet.SheetName + " in "
                    + Path.GetFileName(path) + " is not well formed XML: " + failed.Message);
            }
        }

        /// <summary>
        /// Every row whose own cells carry the canopy formula for that row. **The comparison is
        /// <see cref="WorkbookArithmetic.IsTheCanopyFormula"/>, the same one the guard asks of a
        /// row this run wrote**, so a row this reader calls usable is a row that guard will pass.
        /// Nothing at all where no diameter column was chosen, because then there is no formula
        /// to look for and a row cannot be called usable on a guess.
        /// </summary>
        private static List<int> CanopyRowsIn(IReadOnlyList<FormulaCell> onTheSheet, string diameterColumn)
        {
            var rows = new List<int>();
            if (string.IsNullOrWhiteSpace(diameterColumn)) return rows;

            foreach (FormulaCell one in onTheSheet)
            {
                CellRef at = CellRef.TryParse(one.Cell);
                if (at == null || at.Row <= KpiTemplates.TreeHeaderRow) continue;
                if (!WorkbookArithmetic.IsTheCanopyFormula(one.Text, diameterColumn, at.Row)) continue;

                rows.Add(at.Row);
            }

            return rows;
        }

        /// <summary>
        /// Every row whose own cells carry the TOTAL canopy formula for that row, and that the
        /// canopy total really adds. **The comparison is
        /// <see cref="WorkbookArithmetic.IsTheTotalCanopyFormula"/>, the same one the guard asks
        /// of a row this run wrote**, so a row this reader calls usable is a row that guard will
        /// pass. A row outside the total's own range is left out however it computes, because a
        /// canopy the total does not reach is a canopy the workbook never adds.
        /// </summary>
        private static List<int> TotalCanopyRowsIn(
            IReadOnlyList<FormulaCell> onTheSheet, string diameterColumn, TotalCanopyColumn totalCanopy)
        {
            var canopyColumns = new Dictionary<int, string>();
            foreach (FormulaCell one in onTheSheet)
            {
                CellRef at = CellRef.TryParse(one.Cell);
                if (at == null || at.Row <= KpiTemplates.TreeHeaderRow) continue;
                if (!WorkbookArithmetic.IsTheCanopyFormula(one.Text, diameterColumn, at.Row)) continue;

                if (!canopyColumns.ContainsKey(at.Row)) canopyColumns[at.Row] = at.Column.ToUpperInvariant();
            }

            var rows = new List<int>();
            foreach (FormulaCell one in onTheSheet)
            {
                CellRef at = CellRef.TryParse(one.Cell);
                if (at == null || at.Row <= KpiTemplates.TreeHeaderRow) continue;
                if (!totalCanopy.Adds(at.Row)) continue;

                string canopyColumn;
                if (!canopyColumns.TryGetValue(at.Row, out canopyColumn)) continue;

                if (!WorkbookArithmetic.IsTheTotalCanopyFormula(
                    one.Text, canopyColumn, KpiTemplates.QuantityColumn, at.Row))
                {
                    continue;
                }

                rows.Add(at.Row);
            }

            return rows;
        }

        /// <summary>
        /// Every cell holding text, by row and then by column letter, over the whole sheet.
        /// Where the list stops is decided afterwards by reading down, never by a range.
        /// </summary>
        private static Dictionary<int, Dictionary<string, string>> CellsByRow(XDocument sheet, List<string> shared)
        {
            var found = new Dictionary<int, Dictionary<string, string>>();

            foreach (XElement cell in WorkbookPackage.Cells(sheet))
            {
                CellRef where = CellRef.TryParse((string)cell.Attribute("r"));
                if (where == null) continue;

                string text = WorkbookPackage.TextOf(cell, shared);
                if (string.IsNullOrWhiteSpace(text)) continue;

                Dictionary<string, string> row;
                if (!found.TryGetValue(where.Row, out row))
                {
                    row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    found[where.Row] = row;
                }

                row[where.Column] = text.Trim();
            }

            return found;
        }

        private static string In(Dictionary<string, string> row, string column)
        {
            string text;
            return row != null && !string.IsNullOrEmpty(column) && row.TryGetValue(column, out text) ? text : string.Empty;
        }

        /// <summary>
        /// The one column of the header row whose heading holds the word. None is said. Where
        /// more than one does, the one the sheet's own formulas read is taken, because that is
        /// the column the workbook computes from: MOSQUES names J, Average Mature Canopy
        /// Diameter (m), and K, Mature Canopy Diameter (m), and L reads J and nothing reads K.
        /// Where the formulas read none of them, or more than one, nothing is chosen and both
        /// are named, because a value written under the wrong one of two is a value on a row
        /// nobody chose. Never a position.
        /// </summary>
        private static string ColumnHeaded(
            Dictionary<int, Dictionary<string, string>> cells,
            HashSet<string> readByFormulas,
            string word,
            out string whyNot,
            out string chosenBy)
        {
            chosenBy = string.Empty;

            Dictionary<string, string> header;
            if (!cells.TryGetValue(KpiTemplates.TreeHeaderRow, out header))
            {
                whyNot = "the sheet's header row, row " + KpiTemplates.TreeHeaderRow + ", holds nothing";
                return string.Empty;
            }

            List<string> holding = header
                .Where(one => KpiNames.Holds(one.Value, word))
                .Select(one => one.Key.ToUpperInvariant())
                .OrderBy(one => one.Length)
                .ThenBy(one => one, StringComparer.Ordinal)
                .ToList();

            if (holding.Count == 1)
            {
                whyNot = string.Empty;
                chosenBy = TheOneColumn(word);
                return holding[0];
            }

            if (holding.Count == 0)
            {
                whyNot = NoColumn(word);
                return string.Empty;
            }

            List<string> read = holding.Where(readByFormulas.Contains).ToList();
            string named = string.Join(", ", holding.ToArray());
            if (read.Count == 1)
            {
                whyNot = string.Empty;
                chosenBy = "of " + named + " holding " + word + ", the one the sheet's own formulas read";
                return read[0];
            }

            string formulasRead = read.Count == 0
                ? "none of them"
                : read.Count == holding.Count
                    ? (holding.Count == 2 ? "both of them" : "all " + holding.Count + " of them")
                    : read.Count + " of them, " + string.Join(", ", read.ToArray());
            whyNot = "the sheet's header row, row " + KpiTemplates.TreeHeaderRow + ", names " + holding.Count
                + " columns holding " + word + ", " + named + ", and its own formulas read " + formulasRead
                + ", so nothing says which";
            return string.Empty;
        }

        private static string TheOneColumn(string word)
        {
            return "the one column of the header row holding " + word;
        }

        private static readonly Regex CellReference =
            new Regex(@"(?<![A-Za-z_])\$?([A-Z]{1,3})\$?[0-9]+(?![0-9(])", RegexOptions.Compiled);

        /// <summary>
        /// Every column a formula below the header row reads, off the formula text the sheet
        /// stores. A shared formula carries its text on the master cell alone and the rest
        /// carry an index, so the master is what is read. A function name followed by digits,
        /// LOG10, is not a cell, which the bracket after it says.
        /// </summary>
        private static HashSet<string> ColumnsFormulasRead(XDocument sheet)
        {
            var read = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (XElement cell in WorkbookPackage.Cells(sheet))
            {
                CellRef where = CellRef.TryParse((string)cell.Attribute("r"));
                if (where == null || where.Row <= KpiTemplates.TreeHeaderRow) continue;

                XElement formula = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "f");
                if (formula == null || string.IsNullOrWhiteSpace(formula.Value)) continue;

                foreach (Match reference in CellReference.Matches(formula.Value))
                {
                    read.Add(reference.Groups[1].Value.ToUpperInvariant());
                }
            }

            return read;
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

            foreach (XElement candidate in WorkbookPackage.Cells(sheet))
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
    }
}
