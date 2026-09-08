using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet the model already holds, offered as the sheet to copy from.
    ///
    /// The identifier is carried so the panel can hand it back without naming a Revit type, the
    /// same way a grid cell carries a view identifier.
    /// </summary>
    public sealed class SheetInTheModel : IComparable<SheetInTheModel>
    {
        public SheetInTheModel(long sheetId, string sheetNumber, string sheetName)
        {
            if (sheetNumber == null) throw new ArgumentNullException("sheetNumber");
            if (sheetName == null) throw new ArgumentNullException("sheetName");

            SheetId = sheetId;
            SheetNumber = sheetNumber;
            SheetName = sheetName;
        }

        public long SheetId { get; }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public int CompareTo(SheetInTheModel other)
        {
            if (ReferenceEquals(other, null)) return 1;

            int byNumber = NaturalOrder.Comparer.Compare(SheetNumber, other.SheetNumber);
            if (byNumber != 0) return byNumber;
            return string.CompareOrdinal(SheetName, other.SheetName);
        }

        public override string ToString()
        {
            return SheetNumber + "   " + SheetName;
        }
    }
}
