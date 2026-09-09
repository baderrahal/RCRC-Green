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
        private CellWrite(string sheetName, CellRef cell, string stored, bool isText)
        {
            SheetName = sheetName;
            Cell = cell;
            Stored = stored;
            IsText = isText;
        }

        public string SheetName { get; }

        public CellRef Cell { get; }

        /// <summary>
        /// The value exactly as it goes in the file: the text itself, or the number in
        /// invariant round trip form.
        /// </summary>
        public string Stored { get; }

        public bool IsText { get; }

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
