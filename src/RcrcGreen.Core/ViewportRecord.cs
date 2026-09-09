using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// One view placed on one sheet: where it landed, how big it came out, and the scale it
    /// carries, next to the size of the sheet it sits on.
    ///
    /// The first sheets came out with views the user called too small, and the report could not
    /// say why because it recorded nothing about the placement. The same shape is read off
    /// sheets that already exist in the model by the scan, so a placement the tool made can be
    /// held against one the team made.
    /// </summary>
    public sealed class ViewportRecord
    {
        public ViewportRecord(
            string sheetNumber,
            string sheetName,
            string viewName,
            int scale,
            double centreXFeet,
            double centreYFeet,
            double widthFeet,
            double heightFeet,
            double sheetWidthFeet,
            double sheetHeightFeet,
            bool isASchedule)
        {
            SheetNumber = sheetNumber ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            ViewName = viewName ?? string.Empty;
            Scale = scale;
            CentreXFeet = centreXFeet;
            CentreYFeet = centreYFeet;
            WidthFeet = widthFeet;
            HeightFeet = heightFeet;
            SheetWidthFeet = sheetWidthFeet;
            SheetHeightFeet = sheetHeightFeet;
            IsASchedule = isASchedule;
        }

        public string SheetNumber { get; }

        public string SheetName { get; }

        public string ViewName { get; }

        /// <summary>
        /// The view's own scale, as in the 250 of 1:250. Zero when it has none, which is what a
        /// schedule reads as. It belongs to the view's template rather than to the sheet: the
        /// Scale a sheet shows is only a readout of the views placed on it.
        /// </summary>
        public int Scale { get; }

        public double CentreXFeet { get; }

        public double CentreYFeet { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        public double SheetWidthFeet { get; }

        public double SheetHeightFeet { get; }

        public bool IsASchedule { get; }

        public string ScaleInWords()
        {
            return Scale > 0 ? "1:" + Scale.ToString(CultureInfo.InvariantCulture) : "no scale";
        }

        public string CentreInWords()
        {
            return Millimetres(CentreXFeet) + " by " + Millimetres(CentreYFeet) + " mm";
        }

        public string SizeInWords()
        {
            return Millimetres(WidthFeet) + " by " + Millimetres(HeightFeet) + " mm";
        }

        public string SheetSizeInWords()
        {
            return Millimetres(SheetWidthFeet) + " by " + Millimetres(SheetHeightFeet) + " mm";
        }

        /// <summary>
        /// The whole placement in one line, everything a person needs to judge it without
        /// opening the sheet.
        /// </summary>
        public string InWords()
        {
            return "On " + SheetNumber + " " + SheetName + ": " + ViewName
                + (IsASchedule ? ", a schedule, " : " at " + ScaleInWords() + ", ")
                + SizeInWords() + ", centre at " + CentreInWords()
                + ", on a sheet of " + SheetSizeInWords() + ".";
        }

        private static string Millimetres(double feet)
        {
            return Lengths.InMillimetres(feet).ToString("0.#", CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return InWords();
        }
    }
}
