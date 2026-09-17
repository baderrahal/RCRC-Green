using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The answer to whether the workbook still computes the canopy the way this tool copies it.
    /// </summary>
    public sealed class ArithmeticCheck
    {
        private ArithmeticCheck(bool checkedIt, bool agrees, bool nothingToCheck, string why, IEnumerable<string> read)
        {
            Checked = checkedIt;
            Agrees = agrees;
            NothingToCheck = nothingToCheck;
            Why = why ?? string.Empty;
            Read = (read ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>
        /// The check could not be made, so the workbook's own arithmetic is unknown. **Nothing
        /// is computed off an unknown**, which is the whole point of the guard.
        /// </summary>
        public static ArithmeticCheck NotChecked(string why)
        {
            return new ArithmeticCheck(false, false, false, why, null);
        }

        /// <summary>
        /// There was nothing to check, because this run wrote no tree row at all. **That is not
        /// the same as a check that could not be made**: a plot with no tree row has no canopy
        /// and no canopy formula, so its green cover is the planting and the lawn, with no
        /// workbook formula anywhere in it. The two are told apart by this rather than by
        /// reading the reason, because a signal that travels in the data is not a signal.
        /// </summary>
        public static ArithmeticCheck WithNothingToCheck(string why)
        {
            return new ArithmeticCheck(false, false, true, why, null);
        }

        public static ArithmeticCheck Agreeing(IEnumerable<string> read)
        {
            return new ArithmeticCheck(true, true, false, string.Empty, read);
        }

        public static ArithmeticCheck Differing(string why, IEnumerable<string> read)
        {
            return new ArithmeticCheck(true, false, false, why, read);
        }

        /// <summary>False where there was nothing to check it against.</summary>
        public bool Checked { get; }

        public bool Agrees { get; }

        /// <summary>True where this run wrote no tree row, so no canopy formula is in play.</summary>
        public bool NothingToCheck { get; }

        /// <summary>Empty where it agrees. Never empty where it does not.</summary>
        public string Why { get; }

        /// <summary>Every cell the check read, with the formula it found there.</summary>
        public IReadOnlyList<string> Read { get; }

        /// <summary>Whether a number may be computed off the canopy at all.</summary>
        public bool Usable
        {
            get { return (Checked && Agrees) || NothingToCheck; }
        }
    }

    /// <summary>
    /// What one of the workbook's own computed cells was found to hold, held against the
    /// arithmetic this tool works out.
    /// </summary>
    public sealed class SummaryCellCheck
    {
        private SummaryCellCheck(
            string name, bool checkedIt, bool agrees, bool nothingToCheck,
            string cell, string formula, string canopyCell, string why)
        {
            Name = name ?? string.Empty;
            Checked = checkedIt;
            Agrees = agrees;
            NothingToCheck = nothingToCheck;
            Cell = cell ?? string.Empty;
            Formula = formula ?? string.Empty;
            CanopyCell = canopyCell ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public static SummaryCellCheck NotChecked(string name, string why)
        {
            return new SummaryCellCheck(name, false, false, false, null, null, null, why);
        }

        /// <summary>
        /// The template carries no such cell, so there is nothing to hold the number against.
        /// **An absence is not a drift**, and the number is still written.
        /// </summary>
        public static SummaryCellCheck WithNothingToCheck(string name, string why)
        {
            return new SummaryCellCheck(name, false, false, true, null, null, null, why);
        }

        public static SummaryCellCheck Agreeing(string name, string cell, string formula, string canopyCell)
        {
            return new SummaryCellCheck(name, true, true, false, cell, formula, canopyCell, string.Empty);
        }

        public static SummaryCellCheck Differing(string name, string cell, string formula, string why)
        {
            return new SummaryCellCheck(name, true, false, false, cell, formula, null, why);
        }

        public string Name { get; }

        public bool Checked { get; }

        public bool Agrees { get; }

        public bool NothingToCheck { get; }

        /// <summary>Where the label sent it, printed so a label that chose wrongly is traceable.</summary>
        public string Cell { get; }

        /// <summary>What the file's own formula says there.</summary>
        public string Formula { get; }

        /// <summary>
        /// The canopy cell, learnt from the green cover formula's third term rather than written
        /// in, and carried to the percentage check so the two are held against one cell.
        /// </summary>
        public string CanopyCell { get; }

        /// <summary>Empty where it agrees. Never empty otherwise.</summary>
        public string Why { get; }

        public bool Usable
        {
            get { return (Checked && Agrees) || NothingToCheck; }
        }

        /// <summary>
        /// One line for the report: what was checked, where, and what the file holds there.
        /// </summary>
        public string InWords
        {
            get
            {
                if (NothingToCheck) return Name + ": " + Why;
                if (!Checked) return Name + ": not checked, " + Why;
                if (!Agrees) return Name + ": DOES NOT AGREE. " + Why;

                return Name + ": " + Cell + " reads " + Formula + ", which is this tool's own sum";
            }
        }
    }

    /// <summary>
    /// The guard on the one workbook formula this tool's own arithmetic copies.
    ///
    /// **THE TOOL COMPUTES TWO NUMBERS THE WORKBOOK ALSO COMPUTES**, the total green cover and
    /// the canopy percentage, because the patcher drops every cached formula result on purpose
    /// so Excel recalculates, and the numbers are therefore not in the file the run just wrote.
    /// Both rest on the canopy, and the canopy is the workbook's own column worked out again
    /// here. **So the column is read off the output and compared, row by row, against the text
    /// this tool knows.** A client who changes their arithmetic gets a blank and the cell and
    /// its formula named, never a number computed the old way.
    ///
    /// **THE OTHER TWO FORMULAS ARE MEASURED NOW AND ARE CHECKED TOO.** Bader measured both on
    /// all seven templates on 14 September, which closed the open question the round before left.
    /// <see cref="GreenCoverCell"/> and <see cref="PercentageCell"/> find each by its label, read
    /// what the file's own formula reads, and hold it against the cells this tool adds. Nothing
    /// anywhere names a letter: the two row layouts are exactly why.
    /// </summary>
    public static class WorkbookArithmetic
    {
        /// <summary>
        /// The canopy column as the workbook stores it, measured by Bader on the MOSQUES
        /// template, Tree List - Proposed row 21, and recorded in `steps/log-kpi.md`:
        /// `L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))`.
        ///
        /// **The diameter column is not written in.** J is what that one sheet uses and the
        /// column is found per sheet by its heading everywhere else in this tool, so the caller
        /// hands in the column its own <see cref="SpeciesList"/> chose.
        /// </summary>
        public static string CanopyColumn(string diameterColumn, int row)
        {
            string cell = (diameterColumn ?? string.Empty) + row.ToString(CultureInfo.InvariantCulture);

            return "IF(ISBLANK(" + cell + "),\" \",ROUND(PI()*(" + cell + "/2)^2,0))";
        }

        /// <summary>
        /// Whether one formula IS the canopy formula for its own row. **One rule, asked by the
        /// guard that checks a written row and by the reader that decides whether an empty row
        /// may be written into at all.** Two copies of this comparison would be two answers to
        /// one question, which is the shape this repository has paid for eight times.
        /// </summary>
        public static bool IsTheCanopyFormula(string text, string diameterColumn, int row)
        {
            if (string.IsNullOrWhiteSpace(diameterColumn) || row < 1) return false;

            return string.Equals(
                Bare(text), Bare(CanopyColumn(diameterColumn, row)), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// **THE TOTAL CANOPY COLUMN AS THE WORKBOOK STORES IT**, measured by Bader on all seven
        /// templates on 16 September: column M is Total Mature Canopy Area and a complete row
        /// reads `=IF(ISBLANK(B85)," ",L85*B85)`, the canopy per tree multiplied by the count.
        ///
        /// **NEITHER COLUMN IS WRITTEN IN.** The canopy column comes off the sheet's own rows and
        /// the quantity column is the one every count already goes into, so the caller hands both
        /// in, the same rule <see cref="CanopyColumn"/> already follows for the diameter.
        /// </summary>
        public static string TotalCanopyColumn(string canopyColumn, string quantityColumn, int row)
        {
            string number = row.ToString(CultureInfo.InvariantCulture);
            string count = (quantityColumn ?? string.Empty) + number;

            return "IF(ISBLANK(" + count + "),\" \"," + (canopyColumn ?? string.Empty) + number + "*" + count + ")";
        }

        /// <summary>
        /// Whether one formula IS the total canopy formula for its own row. **One rule, asked by
        /// the guard that checks a written row and by the reader that decides whether an empty
        /// row may be written into at all**, the same shape
        /// <see cref="IsTheCanopyFormula"/> already holds.
        /// </summary>
        public static bool IsTheTotalCanopyFormula(
            string text, string canopyColumn, string quantityColumn, int row)
        {
            if (string.IsNullOrWhiteSpace(canopyColumn) || string.IsNullOrWhiteSpace(quantityColumn)) return false;
            if (row < 1) return false;

            return string.Equals(
                Bare(text),
                Bare(TotalCanopyColumn(canopyColumn, quantityColumn, row)),
                StringComparison.OrdinalIgnoreCase);
        }

        public const string NoWorkbookRead =
            "no workbook was read, so the canopy column could not be checked";

        public const string NoRowsWritten =
            "this run wrote no tree row, so there is no canopy column to check";

        /// <summary>
        /// Every canopy row this run wrote, held against the workbook's own column.
        ///
        /// A row whose canopy cell cannot be found, or whose formula differs, stops BOTH computed
        /// numbers, because both are built on the canopy and a canopy short of one row is a
        /// number that reads as complete and is wrong.
        /// </summary>
        public static ArithmeticCheck Canopy(
            FormulaCheck formulas, CanopyTotal canopy, IDictionary<string, string> diameterColumns,
            string workbookName = null, IEnumerable<TotalCanopyColumn> totalCanopy = null)
        {
            if (formulas == null || !formulas.WasChecked) return ArithmeticCheck.NotChecked(NoWorkbookRead);
            if (canopy == null || canopy.Rows.Count == 0) return ArithmeticCheck.WithNothingToCheck(NoRowsWritten);

            var read = new List<string>();
            var differ = new List<string>();

            // **THE OPENING IS CHOSEN OFF THE KIND OF DIFFERENCE, never off its words.** Every
            // reason used to open with the canopy column sentence, so FP-18's M83, whose canopy
            // column is exactly the one this tool works out, was reported as a column that had
            // drifted and only then said M83 is empty.
            bool columnDrifted = false;
            bool totalNotAdded = false;
            bool noColumnChosen = false;

            foreach (CanopyRow row in canopy.Rows)
            {
                string column;
                if (diameterColumns == null || !diameterColumns.TryGetValue(row.SheetName, out column)
                    || string.IsNullOrWhiteSpace(column))
                {
                    // **NOTHING OF THE WORKBOOK WAS COMPARED HERE.** This tool chose no
                    // diameter column for the sheet, so no cell of it was held against anything,
                    // and opening with the canopy column sentence would assert a drift nobody
                    // looked for. That is the FP-18 misattribution one branch further back.
                    noColumnChosen = true;
                    differ.Add(Of(row, workbookName) + ": no diameter column was chosen for this "
                        + "sheet, so the canopy formula it should carry cannot be worked out");
                    continue;
                }

                string wanted = CanopyColumn(column, row.RowNumber);

                List<FormulaCell> onTheRow = formulas.AllFormulas.Where(one =>
                    string.Equals(one.SheetName, row.SheetName, StringComparison.OrdinalIgnoreCase)
                    && RowOf(one.Cell) == row.RowNumber).ToList();

                FormulaCell found = onTheRow.FirstOrDefault(
                    one => IsTheCanopyFormula(one.Text, column, row.RowNumber));

                if (found != null)
                {
                    // **AND THE ROW HAS TO ADD ITS CANOPY AS WELL AS COMPUTE IT.** FP-18's
                    // Tree List - Existing row 83 carries L83 and no M83 on both park templates,
                    // so its 2 Ziziphus spina-christi computed a canopy per tree that the
                    // canopy total never added, and the PDF went out 100 m2 over the workbook.
                    string missing = WhyTheTotalIsNotAdded(
                        formulas, totalCanopy, row, CellRef.Parse(found.Cell).Column);
                    if (missing.Length > 0)
                    {
                        totalNotAdded = true;
                        differ.Add(Of(row, workbookName) + ": " + missing);
                        continue;
                    }

                    read.Add(found.SheetName + " " + found.Cell + " = " + found.Text
                        + ", " + row.Whose);
                    continue;
                }

                // **THE ROW'S OWN CELLS ARE NAMED, formulas and typed values alike.** Saying
                // only that no cell carries the canopy formula leaves a person opening the
                // workbook to find out what the row does carry, which is the whole question.
                List<TypedCell> typed = formulas.TypedCells.Where(one =>
                    string.Equals(one.SheetName, row.SheetName, StringComparison.OrdinalIgnoreCase)
                    && RowOf(one.Cell) == row.RowNumber).ToList();

                columnDrifted = true;
                differ.Add(Of(row, workbookName) + ": no cell on it carries " + wanted + ". "
                    + TheCanopyCell(formulas, column, row, typed) + " " + Holding(onTheRow, typed));
            }

            return differ.Count == 0
                ? ArithmeticCheck.Agreeing(read)
                : ArithmeticCheck.Differing(
                    Opening(columnDrifted, totalNotAdded, noColumnChosen)
                        + string.Join(" ", differ.ToArray()),
                    read);
        }

        public const string ColumnDrifted =
            "the workbook's canopy column is not the one this tool works out. ";

        public const string TotalNotAdded =
            "a row computes a canopy per tree and adds none to the canopy total. ";

        public const string NoColumnChosen =
            "the canopy column this tool works out could not be chosen for a sheet. ";

        /// <summary>
        /// The sentence the whole reason opens with, chosen off WHICH KIND of difference was
        /// found rather than off the words of any of them.
        ///
        /// **FP-18 IS WHY.** Its Tree List - Existing row 83 carries L83, which is exactly the
        /// canopy formula this tool works out, and M83 is empty. The reason opened with the
        /// canopy column sentence anyway and only then reached the M83 fact, so the first thing
        /// a person read about it was about a column that had not moved.
        /// </summary>
        private static string Opening(bool columnDrifted, bool totalNotAdded, bool noColumnChosen)
        {
            var said = new List<string>();

            if (totalNotAdded) said.Add(TotalNotAdded);
            if (noColumnChosen) said.Add(NoColumnChosen);
            if (columnDrifted) said.Add(ColumnDrifted);

            // **EVERY BRANCH THAT ADDS A REASON SETS ONE OF THE THREE**, so this cannot be
            // reached empty. It answers the column sentence there rather than nothing, because a
            // reason with no opening at all would read as a sentence somebody lost.
            return said.Count == 0 ? ColumnDrifted : string.Concat(said.ToArray());
        }

        /// <summary>
        /// A row named with WHOSE row it is. **A drift on a row this run wrote is the tool
        /// writing a row the workbook cannot compute. A drift on a row the client's list already
        /// held is the client's file computing its canopy another way.** The two need different
        /// answers, and until this line the message said the same thing about both.
        /// </summary>
        /// <summary>
        /// **THE FILE, THE SHEET AND THE ROW**, so a line about a row names the thing the team
        /// has to open rather than whose file it is.
        /// </summary>
        private static string Of(CanopyRow row, string workbookName)
        {
            string file = (workbookName ?? string.Empty).Trim();

            return (file.Length == 0 ? string.Empty : file + ", ")
                + row.SheetName + " row " + row.RowNumber.ToString(CultureInfo.InvariantCulture)
                + ", " + row.Whose;
        }

        /// <summary>
        /// What the row really holds, cell by cell: the formulas with their text, the cells
        /// somebody typed a value into with no formula behind them, and the plain statement that
        /// everything else on the row is empty. A cell absent from the sheet's own data is
        /// empty, so that last clause is a reading and not a guess.
        /// </summary>
        private static string Holding(IEnumerable<FormulaCell> onTheRow, IEnumerable<TypedCell> typed)
        {
            var said = new List<string>();

            said.AddRange((onTheRow ?? Enumerable.Empty<FormulaCell>())
                .OrderBy(one => ColumnOf(one.Cell))
                .Select(one => one.Cell + " = " + one.Text));

            said.AddRange((typed ?? Enumerable.Empty<TypedCell>())
                .OrderBy(one => ColumnOf(one.Cell))
                .Select(one => one.Cell + " holds " + one.Text + " and no formula"));

            return said.Count == 0
                ? "Every cell of the row is empty."
                : "The row holds " + string.Join(", ", said.ToArray())
                    + ", and every other cell of it is empty.";
        }

        public const string NoGreenCoverLabel =
            "so this tool cannot find the cell its sum is meant to equal and writes nothing";

        /// <summary>
        /// **The two parks templates carry no canopy percentage cell**, measured on all seven, so
        /// there is nothing for the check to hold the number against. An absence is not a drift:
        /// the number is still computed and written, and the report says the workbook has no cell
        /// for it.
        /// </summary>
        public const string TheWorkbookHasNoPercentageCell =
            "this template carries no canopy percentage cell, so there is nothing to hold the "
            + "number against, and it is written anyway off the canopy and the area";

        /// <summary>
        /// The workbook's own Total Green cover cell, found by its label, held against the three
        /// cells this tool adds.
        ///
        /// **It must read exactly three single cells, and the planting cell and the lawn cell the
        /// template's map names must be two of them.** The third is the canopy cell, which is how
        /// the canopy cell is learnt rather than written in. Any other shape blanks the field and
        /// names the cell and its formula.
        /// </summary>
        public static SummaryCellCheck GreenCoverCell(
            FormulaCheck formulas,
            string mainSheetName,
            LabelledCell place,
            string plantingCell,
            string lawnCell)
        {
            if (formulas == null || !formulas.WasChecked)
            {
                return SummaryCellCheck.NotChecked(ComputedPlaces.GreenCoverName, NoWorkbookRead);
            }

            if (place == null || !place.Found)
            {
                return SummaryCellCheck.Differing(ComputedPlaces.GreenCoverName, string.Empty, string.Empty,
                    (place == null ? LabelledPlaces.NoLabel(ComputedPlaces.GreenCoverLabel, mainSheetName) : place.Why)
                    + ", " + NoGreenCoverLabel);
            }

            FormulaCell found = At(formulas, mainSheetName, place.ValueCell);
            if (found == null)
            {
                return SummaryCellCheck.Differing(ComputedPlaces.GreenCoverName, place.ValueCell, string.Empty,
                    "the cell the label " + ComputedPlaces.GreenCoverLabel + " chose, "
                    + mainSheetName + " " + place.ValueCell + ", holds no formula at all, where "
                    + "this tool expects the canopy plus the planting plus the lawn");
            }

            var reads = found.SingleCellsRead
                .Where(one => string.Equals(one.SheetName, mainSheetName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            string planting = reads.FirstOrDefault(one => Same(one.Cell.ToString(), plantingCell)) == null
                ? plantingCell : null;
            string lawn = reads.FirstOrDefault(one => Same(one.Cell.ToString(), lawnCell)) == null
                ? lawnCell : null;

            if (reads.Count != 3 || planting != null || lawn != null)
            {
                return SummaryCellCheck.Differing(ComputedPlaces.GreenCoverName, place.ValueCell, found.Text,
                    "the workbook's Total Green cover is not the sum this tool works out. "
                    + mainSheetName + " " + place.ValueCell + " reads " + found.Text + ", which takes "
                    + Named(reads) + ", where this tool adds a canopy cell, the planting cell "
                    + plantingCell + " and the lawn cell " + lawnCell);
            }

            string canopy = reads
                .Select(one => one.Cell.ToString())
                .First(one => !Same(one, plantingCell) && !Same(one, lawnCell));

            return SummaryCellCheck.Agreeing(
                ComputedPlaces.GreenCoverName, place.ValueCell, found.Text, canopy);
        }

        /// <summary>
        /// The workbook's own canopy percentage cell, found by its label, held against the canopy
        /// cell the green cover check learnt and the template's own area cell.
        ///
        /// **It must read exactly those two single cells.** `IF(Area&lt;1," ",F8/Area)` reads F8
        /// and whatever the defined name Area points at, so the name is checked as well as the
        /// cell. A template carrying no such label has nothing to check rather than a drift.
        /// </summary>
        public static SummaryCellCheck PercentageCell(
            FormulaCheck formulas,
            string mainSheetName,
            LabelledCell place,
            string canopyCell,
            string areaCell)
        {
            if (formulas == null || !formulas.WasChecked)
            {
                return SummaryCellCheck.NotChecked(ComputedPlaces.PercentageName, NoWorkbookRead);
            }

            if (place == null || !place.Found)
            {
                return SummaryCellCheck.WithNothingToCheck(
                    ComputedPlaces.PercentageName, TheWorkbookHasNoPercentageCell);
            }

            FormulaCell found = At(formulas, mainSheetName, place.ValueCell);
            if (found == null)
            {
                return SummaryCellCheck.Differing(ComputedPlaces.PercentageName, place.ValueCell, string.Empty,
                    "the cell the label " + ComputedPlaces.PercentageLabel + " chose, "
                    + mainSheetName + " " + place.ValueCell + ", holds no formula at all, where "
                    + "this tool expects the canopy over the area");
            }

            var reads = found.SingleCellsRead
                .Where(one => string.Equals(one.SheetName, mainSheetName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            bool holdsCanopy = canopyCell.Length > 0
                && reads.Any(one => Same(one.Cell.ToString(), canopyCell));
            bool holdsArea = reads.Any(one => Same(one.Cell.ToString(), areaCell));

            if (reads.Count != 2 || !holdsCanopy || !holdsArea)
            {
                return SummaryCellCheck.Differing(ComputedPlaces.PercentageName, place.ValueCell, found.Text,
                    "the workbook's canopy percentage is not the division this tool works out. "
                    + mainSheetName + " " + place.ValueCell + " reads " + found.Text + ", which takes "
                    + Named(reads) + ", where this tool divides the canopy cell "
                    + (canopyCell.Length == 0 ? "the green cover cell names" : canopyCell)
                    + " by the area cell " + areaCell);
            }

            return SummaryCellCheck.Agreeing(
                ComputedPlaces.PercentageName, place.ValueCell, found.Text, canopyCell);
        }

        private static FormulaCell At(FormulaCheck formulas, string sheetName, string cell)
        {
            return formulas.AllFormulas.FirstOrDefault(one =>
                string.Equals(one.SheetName, sheetName, StringComparison.OrdinalIgnoreCase)
                && Same(one.Cell, cell));
        }

        private static bool Same(string one, string other)
        {
            return string.Equals(one, other, StringComparison.OrdinalIgnoreCase);
        }

        private static string Named(IEnumerable<WorkbookCell> cells)
        {
            string[] held = cells.Select(one => one.Cell.ToString()).ToArray();

            return held.Length == 0 ? "no cell at all" : string.Join(", ", held);
        }

        private static int RowOf(string cell)
        {
            CellRef held = CellRef.TryParse(cell);
            return held == null ? 0 : held.Row;
        }

        /// <summary>
        /// The formula with every space taken out, so a file storing it tight and a file storing
        /// it spaced read as one text. The one string literal in it is a single space on both
        /// sides of the comparison, so it comes out of both alike.
        /// </summary>
        /// <summary>
        /// Why this row's canopy never reaches the canopy TOTAL, or empty where it does.
        ///
        /// **A CHECK THAT COULD NOT BE MADE IS NOT A CHECK THAT PASSED.** This used to return an
        /// empty reason for every template whose chain could not be followed, so the moment
        /// <see cref="TotalCanopyColumns.In"/> could not read a column, the green cover was
        /// written with no total canopy check at all and the chain's own reason was printed
        /// nowhere. Today's seven templates read fine and the team is editing them, which is
        /// exactly when a guard that switches itself off costs something.
        ///
        /// **THE ONE EXCEPTION IS A TEMPLATE THAT NAMES NO GREEN COVER CELL.** It holds no canopy
        /// total, so a row of it reaches none by construction and there is nothing to check. That
        /// is a flag on the column rather than a reading of its words.
        /// </summary>
        private static string WhyTheTotalIsNotAdded(
            FormulaCheck formulas,
            IEnumerable<TotalCanopyColumn> totalCanopy,
            CanopyRow row,
            string canopyColumn)
        {
            // **NOBODY READ THE COLUMNS AT ALL**, which is a caller that handed none rather than
            // a template that could not be read. `Canopy` says so on its own answer through
            // `TotalCanopyRead`, so the absence is on the record rather than passing as a check.
            if (totalCanopy == null) return string.Empty;

            TotalCanopyColumn column = TotalCanopyColumns.For(totalCanopy, row.SheetName);

            if (!column.Found)
            {
                if (column.TheTemplateNamesNoGreenCover) return string.Empty;

                return "the total canopy column on " + row.SheetName + " could not be read, so "
                    + "nothing says whether this row's canopy reaches a total: " + column.Why;
            }

            if (!column.Adds(row.RowNumber))
            {
                return "the canopy total " + column.TotalCell + " adds rows "
                    + column.FirstRow.ToString(CultureInfo.InvariantCulture) + " to "
                    + column.LastRow.ToString(CultureInfo.InvariantCulture)
                    + " and not this one, so this row's canopy reaches no total";
            }

            string cell = column.Column + row.RowNumber.ToString(CultureInfo.InvariantCulture);

            FormulaCell adds = formulas.AllFormulas.FirstOrDefault(one =>
                string.Equals(one.SheetName, row.SheetName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(one.Cell, cell, StringComparison.OrdinalIgnoreCase));

            if (adds != null
                && IsTheTotalCanopyFormula(adds.Text, canopyColumn, KpiTemplates.QuantityColumn, row.RowNumber))
            {
                return string.Empty;
            }

            TypedCell typed = formulas.TypedCells.FirstOrDefault(one =>
                string.Equals(one.SheetName, row.SheetName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(one.Cell, cell, StringComparison.OrdinalIgnoreCase));

            return cell + " does not carry "
                + TotalCanopyColumn(canopyColumn, KpiTemplates.QuantityColumn, row.RowNumber)
                + ", which is the column the canopy total " + column.TotalCell + " adds, so this "
                + "row computes a canopy per tree and adds none. "
                + (adds != null
                    ? cell + " holds " + adds.Text + "."
                    : typed != null
                        ? cell + " holds " + typed.Text + " and no formula."
                        : cell + " is empty.");
        }

        /// <summary>
        /// **THE ONE CELL THAT WOULD HAVE CARRIED THE CANOPY, NAMED.** Which column that is, is
        /// read off the sheet's OWN other rows: the column a canopy formula really sits in
        /// somewhere on this sheet. So the line says L85 is empty, or L85 holds 50 and no
        /// formula, rather than leaving a person to work out which cell is missing.
        ///
        /// **WHERE NO ROW OF THE SHEET CARRIES ONE, NOTHING IS NAMED AND THAT IS SAID.** A
        /// column guessed from a letter would be the fall back to a position this repository
        /// forbids everywhere else.
        ///
        /// The cell right of it, which multiplies the canopy by the count, is NOT named. The
        /// tool holds no record of that column's formula, so naming it would be a rule nobody
        /// measured. It is an open question in the log.
        /// </summary>
        private static string TheCanopyCell(
            FormulaCheck formulas, string diameterColumn, CanopyRow row, IEnumerable<TypedCell> typed)
        {
            string column = CanopyColumnOf(formulas, diameterColumn, row.SheetName);
            if (column.Length == 0)
            {
                return "No row of " + row.SheetName + " carries the canopy formula anywhere, so "
                    + "nothing says which column it belongs in on this one.";
            }

            string cell = column + row.RowNumber.ToString(CultureInfo.InvariantCulture);
            TypedCell holding = (typed ?? Enumerable.Empty<TypedCell>())
                .FirstOrDefault(one => string.Equals(one.Cell, cell, StringComparison.OrdinalIgnoreCase));

            return "On " + row.SheetName + " the canopy formula sits in column " + column + ", and "
                + (holding == null
                    ? cell + " is empty."
                    : cell + " holds " + holding.Text + " and no formula.");
        }

        /// <summary>
        /// The column a canopy formula really sits in on this sheet, off any row that carries
        /// one, or empty where no row does.
        /// </summary>
        private static string CanopyColumnOf(FormulaCheck formulas, string diameterColumn, string sheetName)
        {
            foreach (FormulaCell one in formulas.AllFormulas)
            {
                if (!string.Equals(one.SheetName, sheetName, StringComparison.OrdinalIgnoreCase)) continue;

                CellRef where = CellRef.TryParse(one.Cell);
                if (where == null) continue;
                if (!IsTheCanopyFormula(one.Text, diameterColumn, where.Row)) continue;

                return where.Column.ToUpperInvariant();
            }

            return string.Empty;
        }

        /// <summary>
        /// The cell's column as a number, so a row's cells are named left to right rather than
        /// in whatever order the sheet's own data happens to hold them.
        /// </summary>
        private static int ColumnOf(string cell)
        {
            CellRef where = CellRef.TryParse(cell);

            return where == null ? int.MaxValue : where.ColumnNumber;
        }

        private static string Bare(string text)
        {
            return new string((text ?? string.Empty).Where(one => !char.IsWhiteSpace(one)).ToArray());
        }
    }
}
