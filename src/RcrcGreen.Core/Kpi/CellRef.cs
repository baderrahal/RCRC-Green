using System;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One cell reference such as D8, split into its column and its row so cells can be kept
    /// in the order the sheet stores them. A sheet part lists rows ascending and cells within
    /// a row ascending by column, and a cell written out of order is a file Excel repairs.
    /// </summary>
    public sealed class CellRef
    {
        private CellRef(string column, int columnNumber, int row)
        {
            Column = column;
            ColumnNumber = columnNumber;
            Row = row;
        }

        public string Column { get; }

        /// <summary>
        /// A is 1, Z is 26, AA is 27.
        /// </summary>
        public int ColumnNumber { get; }

        public int Row { get; }

        public override string ToString()
        {
            return Column + Row.ToString(CultureInfo.InvariantCulture);
        }

        public static CellRef Parse(string reference)
        {
            CellRef parsed = TryParse(reference);
            if (parsed == null)
            {
                throw new ArgumentException("Not a cell reference: " + (reference ?? "(null)"));
            }

            return parsed;
        }

        public static CellRef TryParse(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return null;

            int at = 0;
            int columnNumber = 0;
            while (at < reference.Length && reference[at] >= 'A' && reference[at] <= 'Z')
            {
                columnNumber = columnNumber * 26 + (reference[at] - 'A' + 1);
                at++;
            }

            if (at == 0 || at > 3 || at == reference.Length) return null;

            int row = 0;
            for (int digit = at; digit < reference.Length; digit++)
            {
                if (reference[digit] < '0' || reference[digit] > '9') return null;
                row = row * 10 + (reference[digit] - '0');
            }

            if (row < 1) return null;

            return new CellRef(reference.Substring(0, at), columnNumber, row);
        }
    }
}
