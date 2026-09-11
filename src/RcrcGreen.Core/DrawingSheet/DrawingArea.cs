using System;
using System.Globalization;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The part of a sheet a view may sit in, which is not the whole sheet.
    ///
    /// The layout divided the whole title block evenly, so a wide view centred in it ran across
    /// the title strip down the right hand edge. Every schedule the first real run placed showed
    /// it, on top of the corner fault in <see cref="CornerPlacement"/>.
    ///
    /// **The drawing area cannot be read off a title block.** Revit gives a placed title block
    /// its Sheet Width and Sheet Height and nothing else. Where the strip starts is drawn inside
    /// the family, so reading it would mean opening the family document, which is not something
    /// to do inside a run. So the strip is the tool's own setting, the same way the section
    /// depth and the annotation crop are, and the report says so rather than letting it read as
    /// a measurement off the model.
    /// </summary>
    public sealed class DrawingArea
    {
        /// <summary>
        /// How much of the width the title strip takes. Measured by the team on the run of
        /// 2026-09-11, where the strip down the right of AR-PRX-Title_Block_A1 reads as roughly
        /// a fifth of the sheet. One number, in one place, so a team that measures it properly
        /// changes it once.
        /// </summary>
        public const double TitleStripAcross = 0.2;

        private DrawingArea(
            double leftFeet, double bottomFeet, double widthFeet, double heightFeet, bool stripTakenOff)
        {
            LeftFeet = leftFeet;
            BottomFeet = bottomFeet;
            WidthFeet = widthFeet;
            HeightFeet = heightFeet;
            StripTakenOff = stripTakenOff;
        }

        public double LeftFeet { get; }

        public double BottomFeet { get; }

        public double WidthFeet { get; }

        public double HeightFeet { get; }

        /// <summary>
        /// Whether the title strip was taken off, so the report can say which of the two this
        /// sheet was laid out in.
        /// </summary>
        public bool StripTakenOff { get; }

        /// <summary>
        /// The whole sheet, for a caller that has no strip to take off. The even division on its
        /// own is tested through this, so the maths and the margin stay separate things.
        /// </summary>
        public static DrawingArea WholeSheet(double widthFeet, double heightFeet)
        {
            RequireASize(widthFeet, "widthFeet");
            RequireASize(heightFeet, "heightFeet");

            return new DrawingArea(0.0, 0.0, widthFeet, heightFeet, false);
        }

        /// <summary>
        /// The sheet less the title strip. Only the strip comes off: the gap round the edge is
        /// what the even division already leaves, so taking a border off here as well would be
        /// two margins for one fact.
        /// </summary>
        public static DrawingArea InsideTheTitleBlock(double widthFeet, double heightFeet)
        {
            RequireASize(widthFeet, "widthFeet");
            RequireASize(heightFeet, "heightFeet");

            return new DrawingArea(
                0.0, 0.0, widthFeet * (1.0 - TitleStripAcross), heightFeet, true);
        }

        /// <summary>
        /// What the report says about the area a sheet was laid out in, so nobody reads the
        /// strip width as something that came off the model.
        /// </summary>
        public string InWords()
        {
            string measured = Millimetres(WidthFeet) + " by " + Millimetres(HeightFeet) + " mm";

            if (!StripTakenOff) return "Laid out across the whole sheet, " + measured + ".";

            return "Laid out in " + measured + ", the sheet less the title strip down its right, "
                + "which is taken as " + Percent(TitleStripAcross) + " of the width. That is the "
                + "tool's own setting rather than anything read off the title block: Revit gives "
                + "a placed block its Sheet Width and Sheet Height and nothing about where its "
                + "strip begins.";
        }

        public override string ToString()
        {
            return InWords();
        }

        private static string Millimetres(double feet)
        {
            return Lengths.InMillimetres(feet).ToString("0.#", CultureInfo.InvariantCulture);
        }

        private static string Percent(double part)
        {
            return (part * 100.0).ToString("0.#", CultureInfo.InvariantCulture) + " percent";
        }

        private static void RequireASize(double what, string named)
        {
            if (double.IsNaN(what) || double.IsInfinity(what))
            {
                throw new ArgumentException("The " + named + " is not a real length.", named);
            }

            if (what <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    named, what, "A title block with no " + named + " has nowhere to put a view.");
            }
        }
    }
}
