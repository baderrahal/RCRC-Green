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
            FormulaCheck formulas, CanopyTotal canopy, IDictionary<string, string> diameterColumns)
        {
            if (formulas == null || !formulas.WasChecked) return ArithmeticCheck.NotChecked(NoWorkbookRead);
            if (canopy == null || canopy.Rows.Count == 0) return ArithmeticCheck.WithNothingToCheck(NoRowsWritten);

            var read = new List<string>();
            var differ = new List<string>();

            foreach (CanopyRow row in canopy.Rows)
            {
                string column;
                if (diameterColumns == null || !diameterColumns.TryGetValue(row.SheetName, out column)
                    || string.IsNullOrWhiteSpace(column))
                {
                    differ.Add(Of(row) + ": no diameter column was chosen for this sheet, so the "
                        + "canopy formula it should carry cannot be worked out");
                    continue;
                }

                string wanted = CanopyColumn(column, row.RowNumber);

                List<FormulaCell> onTheRow = formulas.AllFormulas.Where(one =>
                    string.Equals(one.SheetName, row.SheetName, StringComparison.OrdinalIgnoreCase)
                    && RowOf(one.Cell) == row.RowNumber).ToList();

                FormulaCell found = onTheRow.FirstOrDefault(
                    one => string.Equals(Bare(one.Text), Bare(wanted), StringComparison.OrdinalIgnoreCase));

                if (found != null)
                {
                    read.Add(found.SheetName + " " + found.Cell + " = " + found.Text
                        + ", " + row.Whose);
                    continue;
                }

                differ.Add(Of(row) + ": no cell on it carries " + wanted + ". "
                    + (onTheRow.Count == 0
                        ? "The row holds no formula at all."
                        : "The row holds " + string.Join(", ",
                            onTheRow.Select(one => one.Cell + " = " + one.Text).ToArray()) + "."));
            }

            return differ.Count == 0
                ? ArithmeticCheck.Agreeing(read)
                : ArithmeticCheck.Differing(
                    "the workbook's canopy column is not the one this tool works out. "
                    + string.Join(" ", differ.ToArray()), read);
        }

        /// <summary>
        /// A row named with WHOSE row it is. **A drift on a row this run wrote is the tool
        /// writing a row the workbook cannot compute. A drift on a row the client's list already
        /// held is the client's file computing its canopy another way.** The two need different
        /// answers, and until this line the message said the same thing about both.
        /// </summary>
        private static string Of(CanopyRow row)
        {
            return row.SheetName + " row " + row.RowNumber.ToString(CultureInfo.InvariantCulture)
                + ", " + row.Whose;
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
        private static string Bare(string text)
        {
            return new string((text ?? string.Empty).Where(one => !char.IsWhiteSpace(one)).ToArray());
        }
    }
}
