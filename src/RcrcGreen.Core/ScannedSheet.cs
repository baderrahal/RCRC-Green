using System;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One sheet as the scan found it. Plain strings and a count, so the report can be built
    /// and tested without Revit.
    /// </summary>
    public sealed class ScannedSheet
    {
        /// <summary>
        /// The text Revit puts in a sheet number when a sheet is duplicated. Only plot DM-11 has
        /// real numbers in the first model. Every other plot carries numbers like
        /// 010QE Copy 001 and 400Q Copy 009, so counting them says how much of the set is
        /// scaffolding rather than drawings.
        /// </summary>
        public const string CopyMark = "Copy";

        public ScannedSheet(string sheetNumber, string sheetName, int viewsOnSheet, string plotId)
        {
            if (sheetNumber == null) throw new ArgumentNullException("sheetNumber");
            if (sheetName == null) throw new ArgumentNullException("sheetName");

            SheetNumber = sheetNumber;
            SheetName = sheetName;
            ViewsOnSheet = viewsOnSheet;
            PlotId = plotId ?? string.Empty;
        }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public int ViewsOnSheet { get; }

        /// <summary>
        /// What PRX_Plot_ID holds on the sheet, empty when it holds nothing. A group of sheets
        /// in the first model carries none at all, and the Sheet List filters on this.
        /// </summary>
        public string PlotId { get; }

        public bool HasPlotId
        {
            get { return PlotId.Length > 0; }
        }

        public bool NumberedAsACopy
        {
            get { return SheetNumber.IndexOf(CopyMark, StringComparison.Ordinal) >= 0; }
        }
    }
}
