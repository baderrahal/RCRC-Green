using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell of one sheet, named, for saying which cells a run wrote and which the workbook
    /// computes from.
    /// </summary>
    public sealed class WorkbookCell
    {
        public WorkbookCell(string sheetName, string cell)
        {
            if (sheetName == null) throw new ArgumentNullException("sheetName");

            SheetName = sheetName;
            Cell = CellRef.Parse(cell);
        }

        public string SheetName { get; }

        public CellRef Cell { get; }

        public override string ToString()
        {
            return SheetName + " " + Cell;
        }
    }

    /// <summary>
    /// A rectangle of cells one formula reads, on one sheet.
    /// </summary>
    internal sealed class CellArea
    {
        public CellArea(string sheetName, int firstColumn, int firstRow, int lastColumn, int lastRow, string inWords)
        {
            SheetName = sheetName;
            FirstColumn = Math.Min(firstColumn, lastColumn);
            LastColumn = Math.Max(firstColumn, lastColumn);
            FirstRow = Math.Min(firstRow, lastRow);
            LastRow = Math.Max(firstRow, lastRow);
            InWords = inWords;
        }

        public string SheetName { get; }

        public int FirstColumn { get; }

        public int FirstRow { get; }

        public int LastColumn { get; }

        public int LastRow { get; }

        public string InWords { get; }

        /// <summary>
        /// A column number as Excel's own letters. One record, asked by the area itself and by
        /// the reader that prints a cell.
        /// </summary>
        internal static string Letters(int number)
        {
            string letters = string.Empty;
            while (number > 0)
            {
                int remainder = (number - 1) % 26;
                letters = (char)('A' + remainder) + letters;
                number = (number - 1) / 26;
            }

            return letters;
        }

        public bool IsOneCell
        {
            get { return FirstColumn == LastColumn && FirstRow == LastRow; }
        }

        public bool Contains(string sheetName, int column, int row)
        {
            return string.Equals(SheetName, sheetName, StringComparison.Ordinal)
                && column >= FirstColumn && column <= LastColumn
                && row >= FirstRow && row <= LastRow;
        }

        public bool TouchesRow(string sheetName, int row)
        {
            return string.Equals(SheetName, sheetName, StringComparison.Ordinal)
                && row >= FirstRow && row <= LastRow;
        }
    }

    /// <summary>
    /// One formula in the output, as its text reads, with what it reads worked out from that
    /// text alone. Nothing here evaluates anything.
    /// </summary>
    public sealed class FormulaCell
    {
        internal FormulaCell(
            string sheetName,
            string cell,
            string text,
            string sharedWith,
            IReadOnlyList<CellArea> reads,
            IReadOnlyList<string> literals,
            bool hasArithmetic,
            IReadOnlyList<string> functionsExcelMayNotHave)
        {
            SheetName = sheetName;
            Cell = cell;
            Text = text;
            SharedWith = sharedWith ?? string.Empty;
            Reads = reads;
            SingleCellsRead = reads
                .Where(one => one.IsOneCell)
                .Select(one => new WorkbookCell(one.SheetName, CellArea.Letters(one.FirstColumn) + one.FirstRow))
                .GroupBy(one => one.SheetName + "!" + one.Cell, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            Literals = literals;
            HasArithmetic = hasArithmetic;
            FunctionsExcelMayNotHave = functionsExcelMayNotHave;
            ReadsWritten = new List<string>();
        }

        public string SheetName { get; }

        public string Cell { get; }

        /// <summary>
        /// The formula as the file stores it, without the leading equals sign. For a cell that
        /// shares its formula with a master cell, the master's text shifted to this cell.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The master cell this formula is shared from, or empty for a formula of its own.
        /// </summary>
        public string SharedWith { get; }

        internal IReadOnlyList<CellArea> Reads { get; }

        /// <summary>
        /// Every SINGLE cell this formula reads, in the order they appear and without repeats,
        /// **with defined names resolved to the cells they point at**. A range is left out,
        /// because a formula summing a column is not reading one named thing.
        ///
        /// It is what the arithmetic guard asks: the workbook's Total Green cover cell must read
        /// the three cells this tool adds, and its canopy percentage cell must read the canopy
        /// and the area, and both are answered off this rather than off the formula's text.
        /// `IF(Area&lt;1," ",F8/Area)` reads F8 and whatever `Area` points at, which is how the
        /// defined name is checked as well as the cell.
        /// </summary>
        public IReadOnlyList<WorkbookCell> SingleCellsRead { get; }

        /// <summary>
        /// Every string literal in the formula, as written. A formula holding one and an
        /// ISBLANK on a blank cell returns it, and text in arithmetic is #VALUE!.
        /// </summary>
        public IReadOnlyList<string> Literals { get; }

        public bool HasArithmetic { get; }

        /// <summary>
        /// The functions the file stores with the _xlfn. prefix, which is how Excel marks a
        /// function older versions do not have.
        /// </summary>
        public IReadOnlyList<string> FunctionsExcelMayNotHave { get; }

        /// <summary>
        /// Filled by the check: each row this run wrote into that the formula reads, and
        /// through which reference.
        /// </summary>
        public List<string> ReadsWritten { get; }

        public string Where
        {
            get { return SheetName + " " + Cell; }
        }
    }

    /// <summary>
    /// One formula the check found at risk, with why. Level 1 returns text where a number was
    /// expected, level 2 is #VALUE! from arithmetic on that text, level 3 carries an error
    /// from a cell it reads.
    /// </summary>
    public sealed class FormulaAtRisk
    {
        public FormulaAtRisk(
            string sheetName, string cell, string text, string reason, int level, bool fromWrittenRow,
            bool isDivideByZero = false, bool divisorThisRunWrote = false)
        {
            SheetName = sheetName ?? string.Empty;
            Cell = cell ?? string.Empty;
            Text = text ?? string.Empty;
            Reason = reason ?? string.Empty;
            Level = level;
            FromWrittenRow = fromWrittenRow;
            IsDivideByZero = isDivideByZero;
            DivisorThisRunWrote = divisorThisRunWrote;
        }

        /// <summary>
        /// **A division by a cell holding nought or nothing**, which is the one kind of risk the
        /// run's own summary counts on its own.
        ///
        /// It is a property rather than a word to look for in <see cref="Reason"/>. **A signal
        /// that travels in the data is not a signal**: this repo already shipped one reason
        /// reported by printing a marker word into the message it described, and the commit
        /// carrying that fix was refused by its own message. A counter that searched the
        /// sentence would count a sentence that merely talks about a division.
        /// </summary>
        public bool IsDivideByZero { get; }

        /// <summary>
        /// True when THIS RUN wrote the cell being divided by, which is the half that makes the
        /// error the tool's doing rather than the client's arithmetic over a real number. False
        /// on every risk that is not a division.
        /// </summary>
        public bool DivisorThisRunWrote { get; }

        public string SheetName { get; }

        public string Cell { get; }

        public string Text { get; }

        public string Reason { get; }

        public int Level { get; }

        /// <summary>
        /// True when the chain starts on a row this run wrote into, which is what makes it this
        /// run's doing and a refusal.
        /// </summary>
        public bool FromWrittenRow { get; }

        public bool IsAnError
        {
            get { return Level >= 2; }
        }

        public string Where
        {
            get { return SheetName + " " + Cell; }
        }
    }

    /// <summary>
    /// One cell the map names as an input the workbook computes from: whether it is present in
    /// the output, and which formulas read it with what other inputs they lack.
    /// </summary>
    public sealed class ComputedFrom
    {
        public ComputedFrom(WorkbookCell input, bool present, string holds, IEnumerable<string> readers, IEnumerable<string> blankInputs)
        {
            if (input == null) throw new ArgumentNullException("input");

            Input = input;
            Present = present;
            Holds = holds ?? string.Empty;
            Readers = (readers ?? Enumerable.Empty<string>()).ToList();
            BlankInputs = (blankInputs ?? Enumerable.Empty<string>()).ToList();
        }

        public WorkbookCell Input { get; }

        public bool Present { get; }

        public string Holds { get; }

        public IReadOnlyList<string> Readers { get; }

        /// <summary>
        /// Every single cell one of the readers reads that is blank in the output, so a formula
        /// computing from a blank is named rather than assumed to have what it needs.
        /// </summary>
        public IReadOnlyList<string> BlankInputs { get; }
    }

    public sealed class FunctionUse
    {
        public FunctionUse(string name, int cells)
        {
            Name = name ?? string.Empty;
            Cells = cells;
        }

        public string Name { get; }

        public int Cells { get; }
    }

    /// <summary>
    /// What the output's formulas will compute from the cells this run wrote, read off the
    /// output's own text and never by evaluating a formula.
    ///
    /// **This is the check that would have caught the canopy fault without anyone opening
    /// Excel.** Three species written into empty rows carried a name and a count and nothing
    /// else, the row's canopy formula reads IF(ISBLANK(J84), " ", ...) and returned a space,
    /// the cell beside it multiplied that space by the count, and #VALUE! ran from M84 through
    /// the canopy total to the KPI row. Nine errors that survive a full recalculation, because
    /// a recalculation cannot fix a space times a number. The cache checks all passed.
    /// </summary>
    /// <summary>
    /// One cell holding a value that nothing computes: somebody typed it.
    ///
    /// **A ROW WITH A NUMBER TYPED WHERE A FORMULA BELONGS LOOKS EXACTLY LIKE A ROW THAT
    /// COMPUTES.** The canopy guard names these beside the formulas now, because saying only
    /// that no cell on a row carries the canopy formula leaves a person opening the workbook to
    /// find out what the row does carry.
    /// </summary>
    public sealed class TypedCell
    {
        public TypedCell(string sheetName, string cell, string text)
        {
            SheetName = sheetName ?? string.Empty;
            Cell = cell ?? string.Empty;
            Text = text ?? string.Empty;
        }

        public string SheetName { get; }

        public string Cell { get; }

        public string Text { get; }
    }

    /// <summary>
    /// One division this check LOOKED AT and could not work out, with the cell it sits in kept
    /// apart from the sentence about it.
    ///
    /// **A SIGNAL THAT TRAVELS IN THE DATA IS NOT A SIGNAL.** READY has to name the cell, and
    /// reading a cell reference back out of a printed sentence is the shape this repository has
    /// already paid for once, when a reason was reported by printing a marker word into the
    /// message it described.
    /// </summary>
    public sealed class DivisionNotEvaluated
    {
        public DivisionNotEvaluated(string sheetName, string cell, string text, string why)
        {
            SheetName = (sheetName ?? string.Empty).Trim();
            Cell = (cell ?? string.Empty).Trim();
            Text = text ?? string.Empty;
            Why = (why ?? string.Empty).Trim();
        }

        public string SheetName { get; }

        public string Cell { get; }

        /// <summary>The formula itself, so the line can be read without opening the file.</summary>
        public string Text { get; }

        public string Why { get; }

        /// <summary>The sheet and the cell, which is what READY and the glance name.</summary>
        public string Where
        {
            get { return SheetName + " " + Cell; }
        }

        public string InWords
        {
            get { return Where + " = " + Text + ": " + Why; }
        }
    }

    public sealed class FormulaCheck
    {
        internal FormulaCheck(
            bool wasChecked,
            int formulaCount,
            IEnumerable<FormulaCell> readingWrittenRows,
            IEnumerable<FormulaAtRisk> atRisk,
            IEnumerable<ComputedFrom> computesFrom,
            IEnumerable<FunctionUse> functionsExcelMayNotHave,
            string refusal,
            IEnumerable<FormulaCell> allFormulas = null,
            IEnumerable<TypedCell> typedCells = null,
            IEnumerable<DivisionNotEvaluated> divisionsNotEvaluated = null)
        {
            DivisionsNotEvaluated =
                (divisionsNotEvaluated ?? Enumerable.Empty<DivisionNotEvaluated>()).ToList();
            TypedCells = (typedCells ?? Enumerable.Empty<TypedCell>()).ToList();
            WasChecked = wasChecked;
            FormulaCount = formulaCount;
            AllFormulas = (allFormulas ?? Enumerable.Empty<FormulaCell>()).ToList();
            ReadingWrittenRows = (readingWrittenRows ?? Enumerable.Empty<FormulaCell>()).ToList();
            AtRisk = (atRisk ?? Enumerable.Empty<FormulaAtRisk>()).ToList();
            ComputesFrom = (computesFrom ?? Enumerable.Empty<ComputedFrom>()).ToList();
            FunctionsExcelMayNotHave = (functionsExcelMayNotHave ?? Enumerable.Empty<FunctionUse>()).ToList();
            Refusal = refusal ?? string.Empty;
        }

        public static readonly FormulaCheck NotChecked =
            new FormulaCheck(false, 0, null, null, null, null, null);

        public bool WasChecked { get; }

        public int FormulaCount { get; }

        /// <summary>
        /// Every formula in the output, kept so a caller can ask what one named cell computes.
        ///
        /// **The canopy check asks it.** The tool's own canopy arithmetic copies the workbook's
        /// canopy column, and a matched row is a row this run wrote only a count into, so that
        /// row's canopy formula reads nothing this run wrote and never reaches
        /// <see cref="ReadingWrittenRows"/>. Checking it needs every formula.
        /// </summary>
        public IReadOnlyList<FormulaCell> AllFormulas { get; }

        /// <summary>
        /// Every cell holding a value with no formula behind it, over every sheet. **What the
        /// canopy guard names beside the formulas on a row that carries no canopy formula.**
        /// </summary>
        public IReadOnlyList<TypedCell> TypedCells { get; }

        /// <summary>
        /// Every division this check LOOKED AT and could not work out, each carrying its own
        /// cell. **A division nobody evaluated is not a division that is fine**, and the glance
        /// says how many there were rather than saying none was found.
        ///
        /// **THE 21:38 PRESS HELD 284 OF THEM**, S70 and T70 on both tree lists of every one of
        /// its 71 written plots, all reading that the guard could not be evaluated. They feed
        /// READY now, so a workbook whose divisions nobody could check does not read as ready to
        /// send.
        /// </summary>
        public IReadOnlyList<DivisionNotEvaluated> DivisionsNotEvaluated { get; }

        /// <summary>
        /// Every formula whose text reads a cell on a row this run wrote into.
        /// </summary>
        public IReadOnlyList<FormulaCell> ReadingWrittenRows { get; }

        public IReadOnlyList<FormulaAtRisk> AtRisk { get; }

        public IReadOnlyList<ComputedFrom> ComputesFrom { get; }

        public IReadOnlyList<FunctionUse> FunctionsExcelMayNotHave { get; }

        /// <summary>
        /// Why the write is refused, or empty. A formula that would read an error off a row
        /// this run wrote into is the one thing that refuses.
        /// </summary>
        public string Refusal { get; }

        public bool RefusesTheWrite
        {
            get { return Refusal.Length > 0; }
        }
    }

    /// <summary>
    /// Reads every formula in a written workbook and works out, from the text alone, what will
    /// go wrong when Excel computes it.
    /// </summary>
    public static class WorkbookFormulas
    {
        private const string Quote = "\"";

        /// <summary>
        /// Text between double quotes, with a doubled quote inside it.
        /// </summary>
        private static readonly Regex Literal = new Regex("\"(?:[^\"]|\"\")*\"", RegexOptions.CultureInvariant);

        /// <summary>
        /// A cell or a range, on this sheet or another, not part of a longer word and not a
        /// function name such as LOG10.
        /// </summary>
        private static readonly Regex Reference = new Regex(
            @"(?<![A-Za-z0-9_.])(?:(?:'((?:[^']|'')+)'|([A-Za-z_][A-Za-z0-9_.]*))!)?(\$?)([A-Z]{1,3})(\$?)([0-9]{1,7})(?::(\$?)([A-Z]{1,3})(\$?)([0-9]{1,7}))?(?![A-Za-z0-9_(])",
            RegexOptions.CultureInvariant);

        private static readonly Regex Name = new Regex(
            @"(?<![A-Za-z0-9_.!'])([A-Za-z_][A-Za-z0-9_.]*)(?![A-Za-z0-9_.!]|\s*\()",
            RegexOptions.CultureInvariant);

        private static readonly Regex IsBlank = new Regex(
            @"ISBLANK\s*\(\s*([^()]+?)\s*\)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// **A DIVISION BY A COUNT OVER A RANGE**, which the one cell rule above never looked at.
        /// Measured on the 16:37 press: the glance said the check found no #DIV/0! anywhere, and
        /// the FP-24 and SC-06 workbooks recalculate with one in Tree List - Existing S70 and one
        /// in T70. Both read `IF(TotTrees&lt;1," ",S69/COUNT(B4:B83))`, on all seven templates and
        /// on both tree list tabs, and 30 plots of that press have every existing tree on rows 84
        /// to 101, so the range the count covers holds nothing.
        /// </summary>
        private static readonly Regex DividedByACountOverARange = new Regex(
            @"/\s*(COUNT|COUNTA|SUM)\s*\(\s*((?:(?:'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?\$?[A-Z]{1,3}\$?[0-9]{1,7}:\$?[A-Z]{1,3}\$?[0-9]{1,7})\s*\)",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// **THE ONE GUARD SHAPE THAT IS HONOURED, and nothing wider.** All seven templates open
        /// S70 and T70 with `IF(TotTrees&lt;1," ", ...)`, so a plot with no trees at all never
        /// reaches the division and is not a #DIV/0!. Anything else is not evaluated rather than
        /// judged, because working out what a formula computes to is the thing this reader does
        /// not do.
        /// </summary>
        private static readonly Regex GuardedByLessThanOne = new Regex(
            @"^\s*IF\s*\(\s*((?:(?:'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?\$?[A-Z]{1,3}\$?[0-9]{1,7}|[A-Za-z_][A-Za-z0-9_.]*)\s*(<\s*1|<=\s*0|=\s*0)\s*,",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// A division whose divisor is ONE CELL, which is the only shape a #DIV/0! can be read
        /// off text. A divisor that is an expression, a range or a function call is not judged,
        /// because working out what it computes to would be evaluating the formula.
        /// </summary>
        /// <summary>
        /// **THE ONE SHAPE A GUARD'S CELL MAY CARRY**, `SUM`, `COUNT` or `COUNTA` over a single
        /// range and the whole of the formula. `TotTrees` points at B102, which reads
        /// `SUM(B4:B101)` on all seven templates. Anything else is not evaluated.
        /// </summary>
        private static readonly Regex TotalOverARange = new Regex(
            @"^\s*(COUNT|COUNTA|SUM)\s*\(\s*((?:(?:'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?\$?[A-Z]{1,3}\$?[0-9]{1,7}:\$?[A-Z]{1,3}\$?[0-9]{1,7})\s*\)\s*$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex DividedByACell = new Regex(
            @"/\s*((?:(?:'(?:[^']|'')+'|[A-Za-z_][A-Za-z0-9_.]*)!)?\$?[A-Z]{1,3}\$?[0-9]{1,7})(?![A-Za-z0-9_(:])",
            RegexOptions.CultureInvariant);

        private static readonly Regex Xlfn = new Regex(
            @"_xlfn\.([A-Za-z_][A-Za-z0-9_.]*)",
            RegexOptions.CultureInvariant);

        private sealed class CellState
        {
            public bool HasValue;
            public bool HasFormula;
            public string Text = string.Empty;
        }

        public static FormulaCheck Check(
            ZipArchive zip,
            string workbookPart,
            IDictionary<string, string> sheetParts,
            IEnumerable<WorkbookCell> written,
            IEnumerable<WorkbookCell> computesFrom)
        {
            if (zip == null) throw new ArgumentNullException("zip");
            if (sheetParts == null) throw new ArgumentNullException("sheetParts");

            Dictionary<string, string> names = DefinedNames(zip, workbookPart);

            // **A SHARED STRING CELL STORES AN INDEX AND NOT ITS TEXT.** The canopy guard prints
            // `D99 holds 419 and no formula` where D99 holds Prosopis Juliflora, because the
            // raw `<v>` was kept. The table is read once here and every cell's text resolves
            // through the one reader the rest of this tool already asks.
            List<string> shared = WorkbookPackage.SharedStrings(zip);
            var states = new Dictionary<string, Dictionary<string, CellState>>(StringComparer.Ordinal);
            var formulas = new List<FormulaCell>();

            foreach (KeyValuePair<string, string> sheet in sheetParts)
            {
                XDocument part = WorkbookPackage.Read(zip, sheet.Value);
                if (part == null) continue;

                var state = new Dictionary<string, CellState>(StringComparer.OrdinalIgnoreCase);
                states[sheet.Key] = state;
                formulas.AddRange(FormulasIn(sheet.Key, part, state, names, shared));
            }

            List<WorkbookCell> wrote = (written ?? Enumerable.Empty<WorkbookCell>()).Where(one => one != null).ToList();
            List<WorkbookCell> inputs = (computesFrom ?? Enumerable.Empty<WorkbookCell>()).Where(one => one != null).ToList();

            List<FormulaCell> readingWritten = ReadingWrittenRows(formulas, wrote);
            var notEvaluated = new List<DivisionNotEvaluated>();
            List<FormulaAtRisk> atRisk = AtRisk(formulas, states, wrote, names, notEvaluated);
            List<ComputedFrom> computed = Computed(formulas, states, inputs);
            List<FunctionUse> functions = formulas
                .SelectMany(one => one.FunctionsExcelMayNotHave.Distinct(StringComparer.Ordinal))
                .GroupBy(one => one, StringComparer.Ordinal)
                .Select(group => new FunctionUse(group.Key, group.Count()))
                .OrderByDescending(one => one.Cells)
                .ThenBy(one => one.Name, StringComparer.Ordinal)
                .ToList();

            List<TypedCell> typed = states
                .SelectMany(sheet => sheet.Value
                    .Where(one => one.Value.HasValue && !one.Value.HasFormula)
                    .Select(one => new TypedCell(sheet.Key, one.Key, one.Value.Text)))
                .ToList();

            return new FormulaCheck(
                true, formulas.Count, readingWritten, atRisk, computed, functions, Refusal(atRisk),
                formulas, typed, notEvaluated);
        }

        /// <summary>
        /// The one separator between a defined name's scope and its own name in the map's key.
        /// A sheet cannot hold this character, so it cannot be mistaken for part of either.
        /// </summary>
        private const string ScopeMark = "\u0000";

        /// <summary>
        /// Every defined name the workbook holds, KEYED ON ITS SCOPE AND ITS NAME.
        ///
        /// **A NAME CAN BE DEFINED TWICE AND MEAN TWO THINGS.** `TotTrees` is at workbook level
        /// and again on Tree List - Proposed, and a `localSheetId` scopes the second to that one
        /// sheet. Keeping the first of the two by name alone answered a Tree List - Existing
        /// formula with the Proposed sheet's definition whenever that one came first in the file.
        ///
        /// The sheet a `localSheetId` names is read off the workbook's own `sheets` element in
        /// order, because that index is what the attribute counts.
        /// </summary>
        private static Dictionary<string, string> DefinedNames(ZipArchive zip, string workbookPart)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XDocument workbook = workbookPart == null ? null : WorkbookPackage.Read(zip, workbookPart);
            if (workbook == null || workbook.Root == null) return names;

            List<string> sheets = workbook.Root.Elements()
                .Where(element => element.Name.LocalName == "sheets")
                .Elements().Where(element => element.Name.LocalName == "sheet")
                .Select(element => (string)element.Attribute("name") ?? string.Empty)
                .ToList();

            foreach (XElement defined in workbook.Root.Elements()
                .Where(element => element.Name.LocalName == "definedNames")
                .Elements().Where(element => element.Name.LocalName == "definedName"))
            {
                string name = (string)defined.Attribute("name");
                if (string.IsNullOrEmpty(name)) continue;

                string scope = string.Empty;
                int index;
                string local = (string)defined.Attribute("localSheetId");
                if (local != null
                    && int.TryParse(local, NumberStyles.Integer, CultureInfo.InvariantCulture, out index)
                    && index >= 0 && index < sheets.Count)
                {
                    scope = sheets[index];
                }

                string key = scope + ScopeMark + name;
                if (names.ContainsKey(key)) continue;

                names[key] = defined.Value ?? string.Empty;
            }

            return names;
        }

        /// <summary>
        /// What a name means inside a formula on one sheet: that sheet's own definition where it
        /// has one, and the workbook's otherwise. Empty where neither holds it.
        /// </summary>
        private static string TargetOf(Dictionary<string, string> names, string sheetName, string name)
        {
            string target;
            if (names.TryGetValue((sheetName ?? string.Empty) + ScopeMark + name, out target)) return target;

            return names.TryGetValue(ScopeMark + name, out target) ? target : string.Empty;
        }

        /// <summary>
        /// Every formula one sheet part holds, read the same way the whole check reads them, so
        /// a caller that needs one sheet's formulas cannot read them a second way. **A SHARED
        /// FORMULA IS EXPANDED**: the master carries the text and every dependent cell carries
        /// an index, so a reader that took the text alone would see a formula on one row of a
        /// column and none on the eighty rows under it.
        ///
        /// The defined names of the workbook are not resolved here, because they change only
        /// what a formula READS and never its text, and the text is what a caller of this
        /// compares.
        /// </summary>
        public static IReadOnlyList<FormulaCell> Of(string sheetName, XDocument part)
        {
            if (part == null || part.Root == null) return new List<FormulaCell>();

            return FormulasIn(
                sheetName ?? string.Empty,
                part,
                new Dictionary<string, CellState>(StringComparer.Ordinal),
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                null).ToList();
        }

        private static IEnumerable<FormulaCell> FormulasIn(
            string sheetName, XDocument part, Dictionary<string, CellState> state,
            Dictionary<string, string> names, List<string> shared)
        {
            var masters = new Dictionary<string, KeyValuePair<CellRef, string>>(StringComparer.Ordinal);
            var found = new List<KeyValuePair<CellRef, XElement>>();

            foreach (XElement cell in part.Root.Elements()
                .Where(element => element.Name.LocalName == "sheetData")
                .Elements().Where(element => element.Name.LocalName == "row")
                .Elements().Where(element => element.Name.LocalName == "c"))
            {
                CellRef where = CellRef.TryParse((string)cell.Attribute("r"));
                if (where == null) continue;

                XElement formula = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "f");
                XElement value = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "v");
                XElement inline = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "is");

                var held = new CellState();
                held.HasFormula = formula != null;

                // **THE CELL'S TEXT AS A PERSON READS IT**, through the one reader that resolves
                // a shared string to its text. Keeping the raw `<v>` printed an index where a
                // name belongs and would call a cell holding shared string 0 a nought.
                if (inline != null || value != null)
                {
                    held.Text = WorkbookPackage.TextOf(cell, shared);
                    held.HasValue = held.Text.Length > 0;
                }

                state[where.ToString()] = held;

                if (formula == null) continue;

                found.Add(new KeyValuePair<CellRef, XElement>(where, formula));
                string text = formula.Value ?? string.Empty;
                string index = (string)formula.Attribute("si");
                if (string.Equals((string)formula.Attribute("t"), "shared", StringComparison.Ordinal)
                    && text.Length > 0 && index != null && !masters.ContainsKey(index))
                {
                    masters[index] = new KeyValuePair<CellRef, string>(where, text);
                }
            }

            foreach (KeyValuePair<CellRef, XElement> one in found)
            {
                string text = one.Value.Value ?? string.Empty;
                string sharedWith = string.Empty;
                string index = (string)one.Value.Attribute("si");

                if (text.Length == 0 && index != null)
                {
                    KeyValuePair<CellRef, string> master;
                    if (!masters.TryGetValue(index, out master)) continue;

                    sharedWith = master.Key.ToString();
                    text = Shifted(master.Value,
                        one.Key.ColumnNumber - master.Key.ColumnNumber,
                        one.Key.Row - master.Key.Row);
                }

                yield return Parsed(sheetName, one.Key.ToString(), text, sharedWith, names);
            }
        }

        /// <summary>
        /// The formula read for what it says: its references, its string literals, whether it
        /// does arithmetic, and the functions it stores with the _xlfn. prefix.
        /// </summary>
        private static FormulaCell Parsed(
            string sheetName, string cell, string text, string sharedWith, Dictionary<string, string> names)
        {
            var literals = new List<string>();
            string stripped = Literal.Replace(text, match =>
            {
                string inner = match.Value.Substring(1, match.Value.Length - 2).Replace("\"\"", "\"");
                literals.Add(inner);
                return Quote + Quote;
            });

            var reads = new List<CellArea>();
            string withoutReferences = Reference.Replace(stripped, match =>
            {
                reads.Add(AreaOf(match, sheetName));
                return "~";
            });

            foreach (Match name in Name.Matches(withoutReferences))
            {
                string target = TargetOf(names, sheetName, name.Groups[1].Value);
                if (target.Length == 0) continue;

                foreach (Match reference in Reference.Matches(Literal.Replace(target, Quote + Quote)))
                {
                    reads.Add(AreaOf(reference, sheetName));
                }
            }

            bool arithmetic = stripped.IndexOfAny(new[] { '*', '/', '^', '+', '-' }) >= 0;
            List<string> functions = Xlfn.Matches(text).Cast<Match>().Select(one => one.Groups[1].Value).ToList();

            return new FormulaCell(sheetName, cell, text, sharedWith, reads, literals, arithmetic, functions);
        }

        private static CellArea AreaOf(Match match, string ownSheet)
        {
            string quoted = match.Groups[1].Value;
            string bare = match.Groups[2].Value;
            string sheet = quoted.Length > 0 ? quoted.Replace("''", "'") : (bare.Length > 0 ? bare : ownSheet);

            int firstColumn = ColumnNumber(match.Groups[4].Value);
            int firstRow = int.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture);
            int lastColumn = match.Groups[8].Success && match.Groups[8].Value.Length > 0 ? ColumnNumber(match.Groups[8].Value) : firstColumn;
            int lastRow = match.Groups[10].Success && match.Groups[10].Value.Length > 0
                ? int.Parse(match.Groups[10].Value, CultureInfo.InvariantCulture)
                : firstRow;

            string inWords = (string.Equals(sheet, ownSheet, StringComparison.Ordinal) ? string.Empty : sheet + "!")
                + Letters(firstColumn) + firstRow
                + (lastColumn == firstColumn && lastRow == firstRow ? string.Empty : ":" + Letters(lastColumn) + lastRow);

            return new CellArea(sheet, firstColumn, firstRow, lastColumn, lastRow, inWords);
        }

        /// <summary>
        /// A shared formula's master text moved to a dependent cell: every reference not
        /// anchored with a dollar sign moves by the same offset, and text in quotes does not.
        /// </summary>
        private static string Shifted(string master, int columns, int rows)
        {
            var built = new System.Text.StringBuilder();
            int at = 0;
            foreach (Match literal in Literal.Matches(master))
            {
                built.Append(ShiftReferences(master.Substring(at, literal.Index - at), columns, rows));
                built.Append(literal.Value);
                at = literal.Index + literal.Length;
            }

            built.Append(ShiftReferences(master.Substring(at), columns, rows));
            return built.ToString();
        }

        private static string ShiftReferences(string text, int columns, int rows)
        {
            return Reference.Replace(text, match =>
            {
                string sheet = match.Value.Substring(0, match.Groups[3].Index - match.Index);
                string first = Moved(match.Groups[3].Value, match.Groups[4].Value, match.Groups[5].Value, match.Groups[6].Value, columns, rows);
                if (!match.Groups[8].Success || match.Groups[8].Value.Length == 0) return sheet + first;

                return sheet + first + ":" + Moved(match.Groups[7].Value, match.Groups[8].Value, match.Groups[9].Value, match.Groups[10].Value, columns, rows);
            });
        }

        private static string Moved(string columnAnchor, string column, string rowAnchor, string row, int columns, int rows)
        {
            int columnNumber = ColumnNumber(column);
            int rowNumber = int.Parse(row, CultureInfo.InvariantCulture);
            if (columnAnchor.Length == 0) columnNumber = Math.Max(1, columnNumber + columns);
            if (rowAnchor.Length == 0) rowNumber = Math.Max(1, rowNumber + rows);

            return columnAnchor + Letters(columnNumber) + rowAnchor + rowNumber.ToString(CultureInfo.InvariantCulture);
        }

        private static List<FormulaCell> ReadingWrittenRows(List<FormulaCell> formulas, List<WorkbookCell> wrote)
        {
            var rows = wrote
                .Select(one => new KeyValuePair<string, int>(one.SheetName, one.Cell.Row))
                .Distinct()
                .ToList();

            var found = new List<FormulaCell>();
            foreach (FormulaCell formula in formulas)
            {
                foreach (KeyValuePair<string, int> row in rows)
                {
                    foreach (CellArea area in formula.Reads.Where(one => one.TouchesRow(row.Key, row.Value)))
                    {
                        formula.ReadsWritten.Add("row " + row.Value + " of " + row.Key + " through " + area.InWords);
                    }
                }

                if (formula.ReadsWritten.Count > 0) found.Add(formula);
            }

            return found;
        }

        /// <summary>
        /// Level 1 is a formula returning text because ISBLANK is true of a blank cell. Level 2
        /// is arithmetic on that text, which is #VALUE!. Level 3 is any formula reading a cell
        /// at level 2 or above, because an error carries through everything.
        /// </summary>
        private static List<FormulaAtRisk> AtRisk(
            List<FormulaCell> formulas, Dictionary<string, Dictionary<string, CellState>> states,
            List<WorkbookCell> wrote, Dictionary<string, string> names,
            List<DivisionNotEvaluated> notEvaluated)
        {
            var risk = new Dictionary<string, FormulaAtRisk>(StringComparer.Ordinal);
            var order = new List<string>();

            // **THE GUARD'S OWN CELL IS A FORMULA AND FOLLOWING IT IS THE WHOLE OF ITEM 1.**
            // `TotTrees` points at B102, which is `SUM(B4:B101)`, so every one of the 284
            // divisions the 21:38 press looked at came back as a guard nobody could evaluate.
            // <see cref="CellState"/> carries no formula text, so the guard is handed the
            // formulas it already has rather than a second read of the file.
            var byCell = new Dictionary<string, FormulaCell>(StringComparer.OrdinalIgnoreCase);
            foreach (FormulaCell one in formulas) byCell[one.Where] = one;

            foreach (FormulaCell formula in formulas)
            {
                if (formula.Literals.Count == 0) continue;

                foreach (Match blank in IsBlank.Matches(formula.Text))
                {
                    Match reference = Reference.Match(blank.Groups[1].Value.Trim());
                    if (!reference.Success) continue;

                    CellArea area = AreaOf(reference, formula.SheetName);
                    if (!area.IsOneCell || !IsBlankIn(states, area)) continue;

                    bool written = wrote.Any(one =>
                        string.Equals(one.SheetName, area.SheetName, StringComparison.Ordinal) && one.Cell.Row == area.FirstRow);

                    Add(risk, order, new FormulaAtRisk(formula.SheetName, formula.Cell, formula.Text,
                        area.InWords + " is blank, so ISBLANK is true and the formula returns the text \""
                        + formula.Literals[0] + "\"", 1, written));
                    break;
                }
            }

            foreach (FormulaCell formula in formulas)
            {
                if (risk.ContainsKey(formula.Where) || !formula.HasArithmetic) continue;

                foreach (CellArea area in formula.Reads.Where(one => one.IsOneCell))
                {
                    FormulaAtRisk text;
                    if (!risk.TryGetValue(area.SheetName + " " + Letters(area.FirstColumn) + area.FirstRow, out text)) continue;
                    if (text.Level != 1) continue;

                    Add(risk, order, new FormulaAtRisk(formula.SheetName, formula.Cell, formula.Text,
                        "#VALUE!: " + area.InWords + " returns text and this formula does arithmetic on it",
                        2, text.FromWrittenRow));
                    break;
                }
            }

            // **A DIVISION BY A CELL HOLDING NOUGHT OR NOTHING IS #DIV/0!**, and the tool could
            // not see one. The 09:18 run wrote two workbooks that recalculate with two of them
            // each, ANH-007-SC-100004 and ANH-007-ST-100130, neither on the main sheet and both
            // on plots with few trees, and the report named nothing because this check knew only
            // the blank ISBLANK shape.
            //
            // **It is reported and never refused on, deliberately.** A plot with no trees really
            // has no average, so the divide by zero is the client's own arithmetic over a real
            // number, and deleting a correct workbook over it is worse than printing a line. The
            // reason says whether THIS RUN wrote the cell being divided by, which is the half
            // that would make it the tool's doing, and turning that half into a refusal is a
            // decision for Bader once a run has named them.
            foreach (FormulaCell formula in formulas)
            {
                if (risk.ContainsKey(formula.Where)) continue;

                foreach (Match divide in DividedByACell.Matches(formula.Text))
                {
                    Match reference = Reference.Match(divide.Groups[1].Value);
                    if (!reference.Success) continue;

                    CellArea area = AreaOf(reference, formula.SheetName);
                    if (!area.IsOneCell) continue;

                    string holds = DivisorThatIsNought(states, area);
                    if (holds.Length == 0) continue;

                    bool written = wrote.Any(one =>
                        string.Equals(one.SheetName, area.SheetName, StringComparison.Ordinal)
                        && one.Cell.ColumnNumber == area.FirstColumn
                        && one.Cell.Row == area.FirstRow);

                    Add(risk, order, new FormulaAtRisk(formula.SheetName, formula.Cell, formula.Text,
                        "#DIV/0!: " + area.InWords + " " + holds + " and this formula divides by it. "
                        + (written
                            ? "THIS RUN WROTE THAT CELL."
                            : "This run wrote nothing into that cell."),
                        2, false, true, written));
                    break;
                }
            }

            // **AND A DIVISION BY A COUNT OVER A RANGE**, which the loop above cannot see because
            // its divisor is one cell. `S70 = IF(TotTrees<1," ",S69/COUNT(B4:B83))` on all seven
            // templates and on both tree list tabs, and 30 plots of the 16:37 press hold every
            // existing tree on rows 84 to 101, outside the range the count covers.
            foreach (FormulaCell formula in formulas)
            {
                if (risk.ContainsKey(formula.Where)) continue;

                foreach (Match divide in DividedByACountOverARange.Matches(formula.Text))
                {
                    Match reference = Reference.Match(divide.Groups[2].Value);
                    if (!reference.Success) continue;

                    CellArea range = AreaOf(reference, formula.SheetName);
                    string over = divide.Groups[1].Value.ToUpperInvariant();

                    // **THE GUARD IS ASKED BEFORE THE RANGE IS COUNTED.** A formula whose own IF
                    // never reaches the division is not a #DIV/0! however empty the range is.
                    GuardAnswer guard = TheGuard(formula, states, names, byCell);
                    if (guard == GuardAnswer.Holds) break;

                    string why;
                    bool nought = RangeCountsToNought(states, range, over, out why);
                    if (why.Length > 0 || guard == GuardAnswer.NotEvaluated)
                    {
                        notEvaluated.Add(new DivisionNotEvaluated(
                            formula.SheetName, formula.Cell, formula.Text,
                            why.Length > 0 ? why : GuardNotEvaluated));
                        break;
                    }

                    if (!nought) break;

                    bool wroteInto = wrote.Any(one =>
                        string.Equals(one.SheetName, range.SheetName, StringComparison.Ordinal)
                        && one.Cell.ColumnNumber >= range.FirstColumn && one.Cell.ColumnNumber <= range.LastColumn
                        && one.Cell.Row >= range.FirstRow && one.Cell.Row <= range.LastRow);

                    Add(risk, order, new FormulaAtRisk(formula.SheetName, formula.Cell, formula.Text,
                        "#DIV/0!: " + over + " over " + range.InWords + " is 0 and this formula "
                        + "divides by it. "
                        + (wroteInto
                            ? "THIS RUN WROTE INTO THAT RANGE."
                            : "This run wrote nothing into that range."),
                        2, false, true, wroteInto));
                    break;
                }
            }

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (FormulaCell formula in formulas)
                {
                    if (risk.ContainsKey(formula.Where)) continue;

                    FormulaAtRisk carried = order
                        .Select(where => risk[where])
                        .FirstOrDefault(one => one.IsAnError && formula.Reads.Any(area =>
                            area.Contains(one.SheetName, ColumnNumber(CellRef.Parse(one.Cell).Column), CellRef.Parse(one.Cell).Row)));
                    if (carried == null) continue;

                    Add(risk, order, new FormulaAtRisk(formula.SheetName, formula.Cell, formula.Text,
                        "carries the error from " + Named(carried, formula.SheetName), 3, carried.FromWrittenRow));
                    changed = true;
                }
            }

            return order.Select(where => risk[where]).ToList();
        }

        private static void Add(Dictionary<string, FormulaAtRisk> risk, List<string> order, FormulaAtRisk found)
        {
            risk[found.Where] = found;
            order.Add(found.Where);
        }

        private static string Named(FormulaAtRisk one, string ownSheet)
        {
            return (string.Equals(one.SheetName, ownSheet, StringComparison.Ordinal) ? string.Empty : one.SheetName + "!") + one.Cell;
        }

        /// <summary>
        /// Why this cell would make a division #DIV/0!, or empty when it would not.
        ///
        /// **A cell holding a FORMULA is never judged.** The patcher drops every cached value on
        /// the way out, so a formula cell in the output holds no number at all, and reading that
        /// absence as a nought would call every computed divisor a divide by zero.
        /// </summary>
        private static string DivisorThatIsNought(
            Dictionary<string, Dictionary<string, CellState>> states, CellArea area)
        {
            Dictionary<string, CellState> sheet;
            CellState held = null;
            if (states.TryGetValue(area.SheetName, out sheet))
            {
                sheet.TryGetValue(Letters(area.FirstColumn) + area.FirstRow, out held);
            }

            if (held == null || (!held.HasValue && !held.HasFormula)) return "is blank,";
            if (held.HasFormula) return string.Empty;

            double number;
            bool isNought = double.TryParse(
                held.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out number)
                && number == 0.0;

            return isNought ? "holds " + held.Text + "," : string.Empty;
        }

        /// <summary>
        /// Whether a formula's own IF keeps it off the division, or whether nobody can say.
        /// </summary>
        private enum GuardAnswer
        {
            None,
            Holds,
            NotEvaluated
        }

        public const string GuardNotEvaluated =
            "its IF guards the division and what the guard reads could not be evaluated, so "
            + "nothing here says whether the division is reached";

        /// <summary>
        /// **THE GUARD IS THE ONE SHAPE MEASURED AND NOTHING WIDER.** `IF(TotTrees&lt;1," ", ...)`
        /// opens S70 and T70 on all seven templates, so a plot with no trees never reaches the
        /// division. The reference is resolved off the file's own defined names, scoped to the
        /// sheet the formula sits on, and a value nobody can read comes back as not evaluated
        /// rather than as a guard that holds or a guard that does not.
        /// </summary>
        private static GuardAnswer TheGuard(
            FormulaCell formula,
            Dictionary<string, Dictionary<string, CellState>> states,
            Dictionary<string, string> names,
            Dictionary<string, FormulaCell> byCell)
        {
            Match guard = GuardedByLessThanOne.Match(formula.Text);
            if (!guard.Success) return GuardAnswer.None;

            string read = guard.Groups[1].Value;
            Match reference = Reference.Match(read);
            if (!reference.Success)
            {
                string target = TargetOf(names, formula.SheetName, read);
                if (target.Length == 0) return GuardAnswer.NotEvaluated;

                reference = Reference.Match(Literal.Replace(target, Quote + Quote));
                if (!reference.Success) return GuardAnswer.NotEvaluated;
            }

            CellArea area = AreaOf(reference, formula.SheetName);
            if (!area.IsOneCell) return GuardAnswer.NotEvaluated;

            Dictionary<string, CellState> sheet;
            CellState held = null;
            if (states.TryGetValue(area.SheetName, out sheet))
            {
                sheet.TryGetValue(Letters(area.FirstColumn) + area.FirstRow, out held);
            }

            // A blank cell is nought to Excel, so the guard holds.
            if (held == null || (!held.HasValue && !held.HasFormula)) return GuardAnswer.Holds;

            // **A FORMULA CELL HOLDS NO NUMBER IN THIS OUTPUT, SO ITS OWN FORMULA IS FOLLOWED.**
            // `TotTrees` is `SUM(B4:B101)` on all seven templates, and answering not evaluated
            // there is what left 284 divisions unchecked on the 21:38 press. A total over a
            // range is counted off the cell states the same way the division's own divisor is,
            // through one counter, and anything else is still not evaluated because working out
            // what a formula computes to is the thing this reader does not do.
            if (held.HasFormula) return OverTheGuardsRange(area, byCell, states);

            double number;
            if (!double.TryParse(held.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return GuardAnswer.NotEvaluated;
            }

            return number < 1.0 ? GuardAnswer.Holds : GuardAnswer.None;
        }

        /// <summary>
        /// **THE GUARD'S CELL IS A TOTAL OVER A RANGE, AND THAT RANGE IS COUNTED.** The one
        /// shape measured is `SUM`, `COUNT` or `COUNTA` over a single range and nothing wider,
        /// which is the same rule the division's own divisor already follows. A range the
        /// counter cannot work out, or a formula of any other shape, stays not evaluated.
        /// </summary>
        private static GuardAnswer OverTheGuardsRange(
            CellArea guardCell,
            Dictionary<string, FormulaCell> byCell,
            Dictionary<string, Dictionary<string, CellState>> states)
        {
            FormulaCell held;
            if (byCell == null
                || !byCell.TryGetValue(
                    guardCell.SheetName + " " + Letters(guardCell.FirstColumn) + guardCell.FirstRow,
                    out held))
            {
                return GuardAnswer.NotEvaluated;
            }

            Match total = TotalOverARange.Match(held.Text ?? string.Empty);
            if (!total.Success) return GuardAnswer.NotEvaluated;

            Match reference = Reference.Match(total.Groups[2].Value);
            if (!reference.Success) return GuardAnswer.NotEvaluated;

            string why;
            double value = OverARange(
                states, AreaOf(reference, guardCell.SheetName),
                total.Groups[1].Value.ToUpperInvariant(), out why);

            if (why.Length > 0) return GuardAnswer.NotEvaluated;

            return value < 1.0 ? GuardAnswer.Holds : GuardAnswer.None;
        }

        /// <summary>
        /// Whether COUNT, COUNTA or SUM over this range comes to nought, counted off the cell
        /// states after the write. **A cell in the range holding a FORMULA stops the count**, for
        /// the reason the one cell rule already gives: the patcher drops every cached value, so
        /// a formula cell holds no number and reading that absence as nothing would call a real
        /// count nought.
        /// </summary>
        private static bool RangeCountsToNought(
            Dictionary<string, Dictionary<string, CellState>> states, CellArea range, string over,
            out string why)
        {
            double value = OverARange(states, range, over, out why);

            return why.Length == 0 && value == 0.0;
        }

        /// <summary>
        /// What COUNT, COUNTA or SUM over this range comes to, counted off the cell states after
        /// the write. **ONE COUNTER, ASKED BY THE DIVISION AND BY THE GUARD ALIKE**, because two
        /// of them would be two rules for one question, which is the shape this repository keeps
        /// paying for.
        /// </summary>
        private static double OverARange(
            Dictionary<string, Dictionary<string, CellState>> states, CellArea range, string over,
            out string why)
        {
            why = string.Empty;

            Dictionary<string, CellState> sheet;
            if (!states.TryGetValue(range.SheetName, out sheet))
            {
                why = "the range " + range.InWords + " is on a sheet this check did not read";
                return 0.0;
            }

            double total = 0.0;
            int counted = 0;

            for (int column = range.FirstColumn; column <= range.LastColumn; column++)
            {
                for (int row = range.FirstRow; row <= range.LastRow; row++)
                {
                    CellState held;
                    if (!sheet.TryGetValue(Letters(column) + row, out held)) continue;
                    if (!held.HasValue && !held.HasFormula) continue;

                    if (held.HasFormula)
                    {
                        why = Letters(column) + row + " in " + range.InWords + " holds a formula, "
                            + "whose value this file does not carry, so what " + over
                            + " over that range comes to could not be worked out";
                        return 0.0;
                    }

                    double number;
                    bool isNumber = double.TryParse(
                        held.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);

                    if (string.Equals(over, "COUNTA", StringComparison.Ordinal))
                    {
                        counted++;
                        continue;
                    }

                    if (!isNumber) continue;

                    counted++;
                    total = total + number;
                }
            }

            return string.Equals(over, "SUM", StringComparison.Ordinal) ? total : counted;
        }

        private static bool IsBlankIn(Dictionary<string, Dictionary<string, CellState>> states, CellArea area)
        {
            Dictionary<string, CellState> sheet;
            if (!states.TryGetValue(area.SheetName, out sheet)) return true;

            CellState held;
            if (!sheet.TryGetValue(Letters(area.FirstColumn) + area.FirstRow, out held)) return true;

            return !held.HasValue && !held.HasFormula;
        }

        private static List<ComputedFrom> Computed(
            List<FormulaCell> formulas, Dictionary<string, Dictionary<string, CellState>> states, List<WorkbookCell> inputs)
        {
            var found = new List<ComputedFrom>();
            foreach (WorkbookCell input in inputs)
            {
                Dictionary<string, CellState> sheet;
                CellState held = null;
                if (states.TryGetValue(input.SheetName, out sheet)) sheet.TryGetValue(input.Cell.ToString(), out held);

                bool present = held != null && (held.HasValue || held.HasFormula);
                var readers = new List<string>();
                var blanks = new List<string>();
                foreach (FormulaCell formula in formulas.Where(one =>
                    one.Reads.Any(area => area.Contains(input.SheetName, input.Cell.ColumnNumber, input.Cell.Row))))
                {
                    readers.Add(formula.Where + " = " + formula.Text);
                    foreach (CellArea area in formula.Reads.Where(one => one.IsOneCell && IsBlankIn(states, one)))
                    {
                        blanks.Add(formula.Where + " reads " + area.InWords + ", which is blank");
                    }
                }

                found.Add(new ComputedFrom(input, present, held == null ? string.Empty : held.Text, readers, blanks));
            }

            return found;
        }

        private static string Refusal(List<FormulaAtRisk> atRisk)
        {
            List<FormulaAtRisk> errors = atRisk.Where(one => one.IsAnError && one.FromWrittenRow).ToList();
            if (errors.Count == 0) return string.Empty;

            List<FormulaAtRisk> causes = atRisk.Where(one => one.Level == 1 && one.FromWrittenRow).ToList();
            var said = new System.Text.StringBuilder();
            said.Append("The output was written, checked and deleted again. ");
            said.Append(errors.Count).Append(errors.Count == 1 ? " formula" : " formulas");
            said.Append(" would read an error off a row this run wrote into: ");
            said.Append(string.Join(", ", errors.Take(12).Select(one => one.Where).ToArray()));
            if (errors.Count > 12) said.Append(" and ").Append(errors.Count - 12).Append(" more");
            said.Append(". ");
            foreach (FormulaAtRisk cause in causes)
            {
                said.Append(cause.Where).Append(": ").Append(cause.Reason).Append(", and a formula beside it does arithmetic on that. ");
            }

            said.Append("A cell written where no formula can compute from it is a refusal and not a note.");
            return said.ToString();
        }

        private static int ColumnNumber(string letters)
        {
            int number = 0;
            foreach (char letter in letters)
            {
                number = number * 26 + (letter - 'A' + 1);
            }

            return number;
        }

        private static string Letters(int number)
        {
            return CellArea.Letters(number);
        }
    }
}
