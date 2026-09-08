using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet as the scan found it. Plain strings and a count, so the report can be built
    /// and tested without Revit.
    /// </summary>
    public sealed class ScannedSheet
    {
        public ScannedSheet(string sheetNumber, string sheetName, int viewsOnSheet)
        {
            if (sheetNumber == null) throw new ArgumentNullException("sheetNumber");
            if (sheetName == null) throw new ArgumentNullException("sheetName");

            SheetNumber = sheetNumber;
            SheetName = sheetName;
            ViewsOnSheet = viewsOnSheet;
        }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public int ViewsOnSheet { get; }
    }
}
