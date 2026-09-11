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
            bool isASchedule,
            string scaleAsShown = null,
            string viewportTypeName = null)
        {
            SheetNumber = sheetNumber ?? string.Empty;
            SheetName = sheetName ?? string.Empty;
            ViewName = viewName ?? string.Empty;
            Scale = scale;
            ScaleAsShown = scaleAsShown ?? string.Empty;
            ViewportTypeName = viewportTypeName ?? string.Empty;
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

        /// <summary>
        /// The scale as the Properties panel shows it, which is not always 1: and the number.
        /// DM-11-(200) General Arrangement Layout reads Custom with a Scale Value of 250, under
        /// a template named for 250, and the report said 1:250 with nothing about the Custom.
        ///
        /// The two do not disagree. `View.Scale` is the ratio and the documentation is plain
        /// that setting one Revit does not hold in its own list applies a custom scale, so 250
        /// is the number in both places and Custom is the label Revit puts on a value its list
        /// does not carry. Printing both is what stops the report reading as though it had found
        /// something the model does not show. Empty when it could not be read.
        /// </summary>
        public string ScaleAsShown { get; }

        /// <summary>
        /// The viewport type the view was placed with, empty for a schedule, which has none.
        ///
        /// Every viewport the first real run made came out PRX_Title With Line, which nothing in
        /// this repo ever chose: `Viewport.Create` uses the document's own default type. It is
        /// taken off the sibling now, like the family type and the template, and named here so
        /// the report says which it was rather than leaving it to a Properties panel.
        /// </summary>
        public string ViewportTypeName { get; }

        public double CentreXFeet { get; }

        public double CentreYFeet { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        public double SheetWidthFeet { get; }

        public double SheetHeightFeet { get; }

        public bool IsASchedule { get; }

        /// <summary>
        /// The ratio, and what Properties shows for it where that is something else. A view
        /// reading Custom over a Scale Value of 250 is not a report and a model disagreeing, it
        /// is Revit labelling a value its own list does not carry, and the report says both so
        /// nobody has to open the view to find that out.
        /// </summary>
        public string ScaleInWords()
        {
            string ratio = Scale > 0
                ? "1:" + Scale.ToString(CultureInfo.InvariantCulture)
                : "no scale";

            if (ScaleAsShown.Length == 0) return ratio;
            if (string.Equals(ScaleAsShown, ratio, StringComparison.Ordinal)) return ratio;

            return ratio + ", shown as " + ScaleAsShown;
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
                + ", on a sheet of " + SheetSizeInWords() + "."
                + (ViewportTypeName.Length == 0
                    ? string.Empty
                    : " Viewport type " + ViewportTypeName + ".");
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
