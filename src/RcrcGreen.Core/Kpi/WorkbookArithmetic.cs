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
    /// **WHAT IS NOT CHECKED IS SAID RATHER THAN GUESSED.** The workbook's own Total Green cover
    /// cell and its canopy percentage cell carry formulas of their own, and the text of neither
    /// is measured anywhere in this repository, on any of the seven templates. The one thing
    /// near it is H9 reading H8/Area on EXISTING PARKS, one template, which is the shape this
    /// repo has paid for taking as a rule five times over. So nothing here looks for either
    /// cell. <see cref="NotCheckedAgainstTheWorkbook"/> is what the report says instead, and
    /// what to measure is an open question in the log.
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

        public const string NotCheckedAgainstTheWorkbook =
            "the workbook's own Total Green cover cell and canopy percentage cell hold formulas "
            + "whose text nobody has measured, so this sum was not held against either of them";

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
                    differ.Add(row.SheetName + " row " + row.RowNumber.ToString(CultureInfo.InvariantCulture)
                        + ": no diameter column was chosen for this sheet, so the canopy formula "
                        + "it should carry cannot be worked out");
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
                    read.Add(found.SheetName + " " + found.Cell + " = " + found.Text);
                    continue;
                }

                differ.Add(row.SheetName + " row " + row.RowNumber.ToString(CultureInfo.InvariantCulture)
                    + ": no cell on it carries " + wanted + ". "
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
