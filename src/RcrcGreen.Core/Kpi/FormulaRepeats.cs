using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One formula shape found at risk on several rows for the same reason, said once with its
    /// rows listed.
    /// </summary>
    public sealed class RepeatedFormula
    {
        public RepeatedFormula(string sheetName, string example, string text, string reason, IEnumerable<string> cells)
        {
            SheetName = sheetName ?? string.Empty;
            Example = example ?? string.Empty;
            Text = text ?? string.Empty;
            Reason = reason ?? string.Empty;
            Cells = (cells ?? Enumerable.Empty<string>()).ToList();
        }

        public string SheetName { get; }

        /// <summary>The first cell of the run, which is the one the text and the reason are of.</summary>
        public string Example { get; }

        public string Text { get; }

        public string Reason { get; }

        public IReadOnlyList<string> Cells { get; }

        public bool Repeats
        {
            get { return Cells.Count > 1; }
        }

        /// <summary>
        /// The cells this covers, as a span where they run down one column with no gap and as a
        /// list otherwise. **A span is printed only where it really is one**, so no line reads as
        /// covering a cell it does not.
        /// </summary>
        public string Where
        {
            get
            {
                if (Cells.Count == 1) return Cells[0];

                var rows = new List<int>();
                string column = null;
                foreach (string cell in Cells)
                {
                    CellRef held = CellRef.TryParse(cell);
                    if (held == null) return string.Join(", ", Cells.ToArray());

                    if (column == null) column = held.Column;
                    else if (!string.Equals(column, held.Column, StringComparison.OrdinalIgnoreCase))
                    {
                        return string.Join(", ", Cells.ToArray());
                    }

                    rows.Add(held.Row);
                }

                rows.Sort();
                for (int at = 1; at < rows.Count; at++)
                {
                    if (rows[at] != rows[at - 1] + 1) return string.Join(", ", Cells.ToArray());
                }

                return column + rows[0].ToString(CultureInfo.InvariantCulture)
                    + " to " + column + rows[rows.Count - 1].ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// **A FORMULA THE RUN DID NOT AFFECT IS NOT AT RISK FROM THE RUN**, and one formula filled
    /// down a column is one finding rather than one per row.
    ///
    /// Measured on the 18:15 run's own report: the file was 10,708 lines and
    /// WHAT THE WORKBOOK WILL COMPUTE FROM THIS was 5,180 of them, with a heading count of (0)
    /// over a body of 526. Most of the 526 were G31 to G36 repeating one sentence per row per
    /// template, about cells the run never wrote into, and every one of those lines said so
    /// itself: `(not from a row this run wrote into)`.
    ///
    /// **A heading of 0 above 526 lines is a section nobody can trust**, and a real risk inside
    /// it would be invisible. Both rules live here rather than at the printer, so the count above
    /// the section and the lines in it come off one list.
    /// </summary>
    public static class FormulaRepeats
    {
        /// <summary>
        /// Every digit run replaced, so the same formula on row 7 and on row 8 read as one
        /// shape. **The printed line carries the real cells**, so the normalisation decides only
        /// what groups together and hides nothing.
        /// </summary>
        private static readonly Regex Numbers = new Regex(@"\d+", RegexOptions.CultureInvariant);

        private const string Apart = " >>> ";

        /// <summary>
        /// The formulas at risk this run is answerable for: the ones reading a cell on a row it
        /// wrote into. **The others are the client's own workbook doing what it did before this
        /// tool opened it.**
        /// </summary>
        public static IReadOnlyList<FormulaAtRisk> FromWhatTheRunWrote(IEnumerable<FormulaAtRisk> risks)
        {
            return (risks ?? Enumerable.Empty<FormulaAtRisk>())
                .Where(one => one != null && one.FromWrittenRow)
                .ToList();
        }

        /// <summary>
        /// Those same formulas, one entry per shape rather than one per row, in the order the
        /// first of each was found.
        /// </summary>
        public static IReadOnlyList<RepeatedFormula> Of(IEnumerable<FormulaAtRisk> risks)
        {
            var order = new List<string>();
            var byShape = new Dictionary<string, List<FormulaAtRisk>>(StringComparer.Ordinal);

            foreach (FormulaAtRisk one in FromWhatTheRunWrote(risks))
            {
                string shape = one.SheetName + Apart + Numbers.Replace(one.Text ?? string.Empty, "#")
                    + Apart + Numbers.Replace(one.Reason ?? string.Empty, "#");

                List<FormulaAtRisk> held;
                if (!byShape.TryGetValue(shape, out held))
                {
                    held = new List<FormulaAtRisk>();
                    byShape[shape] = held;
                    order.Add(shape);
                }

                held.Add(one);
            }

            return order
                .Select(shape => byShape[shape])
                .Select(held => new RepeatedFormula(
                    held[0].SheetName, held[0].Cell, held[0].Text, held[0].Reason,
                    held.Select(one => one.Cell)))
                .ToList();
        }
    }
}
