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
        public FormulaAtRisk(string sheetName, string cell, string text, string reason, int level, bool fromWrittenRow)
        {
            SheetName = sheetName ?? string.Empty;
            Cell = cell ?? string.Empty;
            Text = text ?? string.Empty;
            Reason = reason ?? string.Empty;
            Level = level;
            FromWrittenRow = fromWrittenRow;
        }

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
    public sealed class FormulaCheck
    {
        internal FormulaCheck(
            bool wasChecked,
            int formulaCount,
            IEnumerable<FormulaCell> readingWrittenRows,
            IEnumerable<FormulaAtRisk> atRisk,
            IEnumerable<ComputedFrom> computesFrom,
            IEnumerable<FunctionUse> functionsExcelMayNotHave,
            string refusal)
        {
            WasChecked = wasChecked;
            FormulaCount = formulaCount;
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
            var states = new Dictionary<string, Dictionary<string, CellState>>(StringComparer.Ordinal);
            var formulas = new List<FormulaCell>();

            foreach (KeyValuePair<string, string> sheet in sheetParts)
            {
                XDocument part = WorkbookPackage.Read(zip, sheet.Value);
                if (part == null) continue;

                var state = new Dictionary<string, CellState>(StringComparer.OrdinalIgnoreCase);
                states[sheet.Key] = state;
                formulas.AddRange(FormulasIn(sheet.Key, part, state, names));
            }

            List<WorkbookCell> wrote = (written ?? Enumerable.Empty<WorkbookCell>()).Where(one => one != null).ToList();
            List<WorkbookCell> inputs = (computesFrom ?? Enumerable.Empty<WorkbookCell>()).Where(one => one != null).ToList();

            List<FormulaCell> readingWritten = ReadingWrittenRows(formulas, wrote);
            List<FormulaAtRisk> atRisk = AtRisk(formulas, states, wrote);
            List<ComputedFrom> computed = Computed(formulas, states, inputs);
            List<FunctionUse> functions = formulas
                .SelectMany(one => one.FunctionsExcelMayNotHave.Distinct(StringComparer.Ordinal))
                .GroupBy(one => one, StringComparer.Ordinal)
                .Select(group => new FunctionUse(group.Key, group.Count()))
                .OrderByDescending(one => one.Cells)
                .ThenBy(one => one.Name, StringComparer.Ordinal)
                .ToList();

            return new FormulaCheck(true, formulas.Count, readingWritten, atRisk, computed, functions, Refusal(atRisk));
        }

        private static Dictionary<string, string> DefinedNames(ZipArchive zip, string workbookPart)
        {
            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            XDocument workbook = workbookPart == null ? null : WorkbookPackage.Read(zip, workbookPart);
            if (workbook == null || workbook.Root == null) return names;

            foreach (XElement defined in workbook.Root.Elements()
                .Where(element => element.Name.LocalName == "definedNames")
                .Elements().Where(element => element.Name.LocalName == "definedName"))
            {
                string name = (string)defined.Attribute("name");
                if (string.IsNullOrEmpty(name) || names.ContainsKey(name)) continue;

                names[name] = defined.Value ?? string.Empty;
            }

            return names;
        }

        private static IEnumerable<FormulaCell> FormulasIn(
            string sheetName, XDocument part, Dictionary<string, CellState> state, Dictionary<string, string> names)
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
                if (inline != null)
                {
                    held.Text = string.Concat(inline.Descendants().Where(child => child.Name.LocalName == "t").Select(child => child.Value));
                    held.HasValue = held.Text.Length > 0;
                }
                else if (value != null)
                {
                    held.Text = value.Value ?? string.Empty;
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
                string target;
                if (!names.TryGetValue(name.Groups[1].Value, out target)) continue;

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
            List<FormulaCell> formulas, Dictionary<string, Dictionary<string, CellState>> states, List<WorkbookCell> wrote)
        {
            var risk = new Dictionary<string, FormulaAtRisk>(StringComparer.Ordinal);
            var order = new List<string>();

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
            string letters = string.Empty;
            while (number > 0)
            {
                int remainder = (number - 1) % 26;
                letters = (char)('A' + remainder) + letters;
                number = (number - 1) / 26;
            }

            return letters;
        }
    }
}
