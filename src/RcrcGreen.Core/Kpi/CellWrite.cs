using System;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One value bound for one cell of one sheet. Text goes in as an inline string so the
    /// shared strings part is never touched, and a number goes in as a number so the
    /// workbook's own formulas can take it.
    /// </summary>
    public sealed class CellWrite
    {
        private CellWrite(string sheetName, CellRef cell, string stored, bool isText, bool isFormula = false)
        {
            SheetName = sheetName;
            Cell = cell;
            Stored = stored;
            IsText = isText;
            IsFormula = isFormula;
        }

        public string SheetName { get; }

        public CellRef Cell { get; }

        /// <summary>
        /// The value exactly as it goes in the file: the text itself, the number in invariant
        /// round trip form, or the formula with no equals sign.
        /// </summary>
        public string Stored { get; }

        public bool IsText { get; }

        /// <summary>
        /// A formula: the file holds the formula and no result, and Excel works it out on open.
        /// </summary>
        public bool IsFormula { get; }

        /// <summary>
        /// A real formula. **The equals sign comes off**, because a formula part carries none
        /// and one left in is a file Excel offers to repair.
        /// </summary>
        public static CellWrite Formula(string sheetName, string cellRef, string formula)
        {
            if (sheetName == null) throw new ArgumentNullException("sheetName");
            if (formula == null) throw new ArgumentNullException("formula");

            string held = formula.Trim();
            if (held.StartsWith("=", StringComparison.Ordinal)) held = held.Substring(1);
            if (held.Length == 0) throw new ArgumentException("A formula cannot be empty.", "formula");

            return new CellWrite(sheetName, CellRef.Parse(cellRef), held, false, true);
        }

        public static CellWrite Text(string sheetName, string cellRef, string value)
        {
            if (sheetName == null) throw new ArgumentNullException("sheetName");
            if (value == null) throw new ArgumentNullException("value");

            return new CellWrite(sheetName, CellRef.Parse(cellRef), value, true);
        }

        public static CellWrite Number(string sheetName, string cellRef, double value)
        {
            if (sheetName == null) throw new ArgumentNullException("sheetName");
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException("value");
            }

            return new CellWrite(sheetName, CellRef.Parse(cellRef), value.ToString("R", CultureInfo.InvariantCulture), false);
        }
    }
}
