using System;
using System.Globalization;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// A field's box on the page, all four numbers off its own `/Rect`.
    ///
    /// **THE READ USED TO TAKE ONLY THE FIRST TWO.** `/Rect [ left bottom right top ]` gives the
    /// position, which is what the form check compares, and the SIZE was thrown away on the way
    /// past. A box nobody measured is a box nothing can be fitted into.
    /// </summary>
    public sealed class PdfTextBox
    {
        public PdfTextBox(double left, double bottom, double right, double top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public static readonly PdfTextBox NotRead = new PdfTextBox(0.0, 0.0, 0.0, 0.0);

        public double Left { get; }

        public double Bottom { get; }

        public double Right { get; }

        public double Top { get; }

        /// <summary>
        /// **Always positive**, because a PDF rectangle is allowed to name its corners either way
        /// round and a negative width would fit nothing at all.
        /// </summary>
        public double Width
        {
            get { return Math.Abs(Right - Left); }
        }

        public double Height
        {
            get { return Math.Abs(Top - Bottom); }
        }

        public bool Read
        {
            get { return Width > 0.0 && Height > 0.0; }
        }

        public string InWords
        {
            get
            {
                return Number(Width) + " pt across and " + Number(Height) + " pt tall";
            }
        }

        private static string Number(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// What one text field's size came to, and why.
    /// </summary>
    public sealed class PdfFieldFit
    {
        public PdfFieldFit(
            string fieldName, string text, PdfFitOutcome outcome, double size, double clientSize,
            PdfTextBox box, string fontName, string why)
        {
            FieldName = fieldName ?? string.Empty;
            Text = text ?? string.Empty;
            Outcome = outcome;
            Size = size;
            ClientSize = clientSize;
            Box = box ?? PdfTextBox.NotRead;
            FontName = fontName ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public string FieldName { get; }

        public string Text { get; }

        public PdfFitOutcome Outcome { get; }

        /// <summary>The size written into the field's own `/DA`. Meaningless where nothing was.</summary>
        public double Size { get; }

        /// <summary>The size the client's `/DA` named, nought being their auto.</summary>
        public double ClientSize { get; }

        public PdfTextBox Box { get; }

        /// <summary>The `/BaseFont` the resource name reached, empty where none was reached.</summary>
        public string FontName { get; }

        public string Why { get; }

        /// <summary>Whether this field's `/DA` is rewritten at all.</summary>
        public bool Writes
        {
            get
            {
                return Outcome == PdfFitOutcome.Shrunk
                    || Outcome == PdfFitOutcome.CappedAtTen
                    || Outcome == PdfFitOutcome.HeldAtSix;
            }
        }

        /// <summary>
        /// What size really landed in the output, read back off the written file. Negative until
        /// the read back sets it, which tells a size of nought apart from one nobody looked for.
        /// </summary>
        public double Landed { get; private set; } = -1.0;

        public bool LandedRead
        {
            get { return Landed >= 0.0; }
        }

        /// <summary>
        /// **THE READ BACK, the same rule every written cell of the workbook already follows.**
        /// The report says what landed rather than what was sent.
        /// </summary>
        public PdfFieldFit With(double landed)
        {
            Landed = landed;
            return this;
        }

        public bool SizeLanded
        {
            get
            {
                if (!Writes) return true;
                if (!LandedRead) return false;

                return Math.Abs(Landed - Size) < 0.001;
            }
        }

        public string InWords
        {
            get
            {
                return FieldName + ": " + PdfTextFit.Words(Outcome)
                    + (Writes ? " at " + Number(Size) + " pt" : string.Empty)
                    + ", " + Box.InWords
                    + (Text.Length == 0 ? string.Empty : ", holding '" + Text + "'")
                    + (Why.Length == 0 ? string.Empty : ". " + Why);
            }
        }

        private static string Number(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// **HOW BIG THE TEXT IS WRITTEN, so it stays inside the box the client drew.**
    ///
    /// **Measured on ANH-007-MO-100011**, a DAILY MOSQUE plot on the Open spaces form: Area shows
    /// 2797.6 cut off at the box edge, Total areas to be greened shows 0.0008 cut off, and the
    /// tree and shrub numbers are drawn taller than their boxes. Nothing in this tool read or
    /// wrote a `/DA` before this round, so every value went in at whatever size the client's
    /// field carried and the viewer drew it at.
    ///
    /// **THE VALUE IS NEVER TOUCHED.** Nothing here rounds, cuts or reformats a number to make it
    /// fit. The text stays exactly what <see cref="PdfFill"/> decided and only the size moves,
    /// because a number changed to fit a box is a wrong number in a client document and a small
    /// number in the right box is not.
    ///
    /// The rule, in order:
    ///
    /// <code>
    /// across  = the box width less 2 pt each side
    /// down    = the box height less 1 pt top and 1 pt bottom
    /// em      = the text measured in the font the /DA names
    /// wanted  = the smaller of across / em and down, rounded DOWN to 0.1 pt
    /// ceiling = the size the form's /DA gives, or 10 pt where it gives nought
    /// floor   = 6 pt
    /// </code>
    ///
    /// **THE HEIGHT MARGIN IS HALF THE SIDE ONE, measured on the 13:32 run.** 271 values came out
    /// held at 6, every one on `Projects Basic Data - Parks`, whose value boxes are 9.72 pt tall.
    /// At 2 pt top and bottom that box left 5.72 pt, under the floor, so its height held every
    /// value whatever the text said. See <see cref="HeightMargin"/>.
    ///
    /// **ROUNDED DOWN AND NEVER TO NEAREST.** A tenth of a point rounded up is a tenth of a point
    /// of text outside the box, which is the whole fault this exists to end.
    ///
    /// **THE CEILING WINS OVER THE FLOOR WHERE THE TWO DISAGREE**, which is a client `/DA` naming
    /// a size below 6. Whether any field carries one is UNKNOWN here, because no client PDF is in
    /// this repository and none ever will be, and the outcome says the text did not fit at the
    /// floor either way so the box is named in the report.
    /// </summary>
    public static class PdfTextFit
    {
        /// <summary>
        /// The margin taken off the LEFT and the RIGHT of the box before the text is measured
        /// across it.
        /// </summary>
        public const double Margin = 2.0;

        /// <summary>
        /// The margin taken off the TOP and the BOTTOM, which is HALF the side one.
        ///
        /// **MEASURED ON THE 13:32 RUN OVER 154 PLOTS.** 271 values came out held at 6, every one
        /// of them on `Projects Basic Data - Parks`, whose boxes are 41.52 by 9.72 pt for a value,
        /// 41.734 by 9.61 for a shrub and 167.346 by 9.818 for a header. `Projects Basic Data -
        /// Open spaces` is 46.6 by 19.4 and `Projects Basic Data - Roads` 46.8 by 17.0, and both
        /// came out at 10.
        ///
        /// **2 pt top and bottom left a 9.72 pt box 5.72 pt of height**, under the 6 pt floor, so
        /// every Parks value was held at the floor by its HEIGHT whatever its text said. At 1 pt
        /// the same box leaves 7.72 and `3977.16` comes out at 7.7.
        ///
        /// **THE SIDE MARGIN IS NOT TOUCHED.** A value running past the left or the right edge is
        /// the fault this rule was built for, and the height never showed one: a glyph is drawn
        /// from its baseline and a box has leading of its own, so a point above and below is room
        /// the width does not have.
        /// </summary>
        public const double HeightMargin = 1.0;

        /// <summary>**Never below this.** Smaller than this is not readable on a printed form.</summary>
        public const double Smallest = 6.0;

        /// <summary>
        /// **The ceiling where the client's own `/DA` gives nought**, which is the PDF's way of
        /// saying the viewer picks a size. Left alone, a viewer picks one to fill the box, which
        /// is how a tree count came out drawn taller than the box holding it.
        /// </summary>
        public const double AutoCeiling = 10.0;

        public const string NoDefaultAppearance =
            "neither this field, its parents nor the AcroForm names a /DA, so there is no size to "
            + "change and none is written";

        public const string BoxNotRead =
            "its /Rect gives no width or no height, so there is no box to fit the text into";

        public static string Words(PdfFitOutcome outcome)
        {
            switch (outcome)
            {
                case PdfFitOutcome.KeptTheClientsSize: return "kept the size its /DA sets";
                case PdfFitOutcome.Shrunk: return "shrunk";
                case PdfFitOutcome.CappedAtTen: return "capped at 10";
                case PdfFitOutcome.HeldAtSix: return "held at 6";
                case PdfFitOutcome.WidthsUnknown: return "widths UNKNOWN";
                default: return "no /DA";
            }
        }

        /// <summary>
        /// The size one field's text is written at.
        /// </summary>
        public static PdfFieldFit Of(
            string fieldName, string text, PdfTextBox box, PdfDefaultAppearance appearance,
            PdfFontWidths widths)
        {
            PdfTextBox held = box ?? PdfTextBox.NotRead;
            PdfDefaultAppearance da = appearance ?? PdfDefaultAppearance.None;
            double client = da.Read ? da.Size : 0.0;

            if (!da.Read)
            {
                return new PdfFieldFit(
                    fieldName, text, PdfFitOutcome.NoDefaultAppearance, 0.0, 0.0, held,
                    widths == null ? string.Empty : widths.BaseFont, NoDefaultAppearance);
            }

            if (widths == null || !widths.Read)
            {
                return new PdfFieldFit(
                    fieldName, text, PdfFitOutcome.WidthsUnknown, client, client, held,
                    widths == null ? da.FontResource : widths.BaseFont,
                    "the font " + (widths == null ? da.FontResource : widths.BaseFont) + " could "
                    + "not be measured, so the value is written at the size its /DA sets. "
                    + (widths == null ? StandardFonts.NotOneOfTheFourteen : widths.Why));
            }

            double ems = widths.Ems(text);
            if (ems < 0.0)
            {
                return new PdfFieldFit(
                    fieldName, text, PdfFitOutcome.WidthsUnknown, client, client, held, widths.BaseFont,
                    "the font " + widths.BaseFont + " has no width for '" + widths.FirstUnmeasurable(text)
                    + "', so the value is written at the size its /DA sets");
            }

            if (!held.Read)
            {
                return new PdfFieldFit(
                    fieldName, text, PdfFitOutcome.WidthsUnknown, client, client, held, widths.BaseFont,
                    BoxNotRead);
            }

            double ceiling = da.IsAuto ? AutoCeiling : client;
            double across = held.Width - (Margin * 2.0);
            double down = held.Height - (HeightMargin * 2.0);

            // Empty text and a text of no width both fit any box, so the ceiling stands.
            double byWidth = ems <= 0.0 ? ceiling : across / ems;
            double wanted = Floored(Math.Min(byWidth, down));
            double size = Math.Min(ceiling, Math.Max(Smallest, wanted));

            if (wanted < Smallest)
            {
                // **WHICH OF THE TWO HELD IT, and RUNS OVER only where the text really does.**
                // 271 values of the 13:32 run were held at 6 and every one of them was held by
                // its box HEIGHT, while the line said it runs over, which is a sentence about
                // the width. A reader cannot act on a reason that names the wrong side.
                bool byTheWidth = byWidth <= down;
                bool over = ems > 0.0 && (ems * Smallest) > across;

                return new PdfFieldFit(
                    fieldName, text, PdfFitOutcome.HeldAtSix, size, client, held, widths.BaseFont,
                    "'" + text + "' needs " + Number(wanted) + " pt to fit a box " + held.InWords
                    + ", held by its " + (byTheWidth ? "WIDTH" : "HEIGHT")
                    + ", which is below the " + Number(Smallest) + " pt floor, so it is written at "
                    + Number(size) + " pt"
                    + (over
                        ? " and runs over, because at " + Number(Smallest) + " pt the text is "
                            + Number(ems * Smallest) + " pt wide against " + Number(across)
                            + " pt of box"
                        : " and still sits inside the box across, because at " + Number(Smallest)
                            + " pt the text is " + Number(ems * Smallest) + " pt wide against "
                            + Number(across) + " pt of box"));
            }

            if (size >= ceiling)
            {
                return new PdfFieldFit(
                    fieldName, text,
                    da.IsAuto ? PdfFitOutcome.CappedAtTen : PdfFitOutcome.KeptTheClientsSize,
                    ceiling, client, held, widths.BaseFont, string.Empty);
            }

            return new PdfFieldFit(
                fieldName, text, PdfFitOutcome.Shrunk, size, client, held, widths.BaseFont,
                "'" + text + "' is " + Number(ems) + " em, so it needs " + Number(size)
                + " pt to sit inside " + Number(across) + " pt of a box " + held.InWords);
        }

        /// <summary>
        /// **Down to a tenth, never to the nearest tenth.** Rounding up puts a tenth of a point
        /// of text outside the box.
        /// </summary>
        public static double Floored(double size)
        {
            if (double.IsNaN(size) || double.IsInfinity(size)) return 0.0;

            return Math.Floor(size * 10.0) / 10.0;
        }

        private static string Number(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
