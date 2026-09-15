using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What happened to one text field's size.
    /// </summary>
    public enum PdfFitOutcome
    {
        /// <summary>The client's own size fits the text, so nothing is changed.</summary>
        KeptTheClientsSize,

        /// <summary>The text does not fit at the client's size, so a smaller one is written.</summary>
        Shrunk,

        /// <summary>The client's size is nought, which is auto, and the fit is above the ceiling.</summary>
        CappedAtTen,

        /// <summary>The text does not fit at 6 pt, which is the floor. Named with its box.</summary>
        HeldAtSix,

        /// <summary>The font's widths could not be read, so the size is left exactly as it was.</summary>
        WidthsUnknown,

        /// <summary>No /DA anywhere, so there is no size to change and none is written.</summary>
        NoDefaultAppearance
    }

    /// <summary>
    /// A field's default appearance as the file holds it, which is a little content stream, and
    /// the only part of it this tool touches is the number in its `Tf` operator.
    ///
    /// **THE FONT NAME IN A /DA IS A RESOURCE NAME AND NOT A FONT.** `/Helv 0 Tf 0 g` names the
    /// key `Helv` in the AcroForm's own `/DR /Font` dictionary, which points at a font object
    /// whose `/BaseFont` is the real name. Reading `Helv` as Helvetica is exactly the shape this
    /// repository keeps paying for, so the lookup goes through the resource dictionary.
    /// </summary>
    public sealed class PdfDefaultAppearance
    {
        private static readonly Regex TextFont = new Regex(
            @"/(?<font>[^\s/\[\]<>(){}%]+)\s+(?<size>-?[0-9]*\.?[0-9]+)\s+Tf",
            RegexOptions.CultureInvariant);

        private PdfDefaultAppearance(string text, bool read, string fontResource, double size)
        {
            Text = text ?? string.Empty;
            Read = read;
            FontResource = fontResource ?? string.Empty;
            Size = size;
        }

        public static readonly PdfDefaultAppearance None =
            new PdfDefaultAppearance(string.Empty, false, string.Empty, 0.0);

        /// <summary>The whole appearance string, exactly as the file holds it.</summary>
        public string Text { get; }

        /// <summary>Whether a `Tf` operator was found in it at all.</summary>
        public bool Read { get; }

        public string FontResource { get; }

        /// <summary>
        /// The size the `Tf` names. **Nought is not a missing size**: it is the PDF's own way of
        /// saying the viewer picks one to fit, which is why it gets its own ceiling rather than
        /// being treated as an absence.
        /// </summary>
        public double Size { get; }

        public bool IsAuto
        {
            get { return Read && Size <= 0.0; }
        }

        public static PdfDefaultAppearance Of(string text)
        {
            string held = text ?? string.Empty;
            Match found = TextFont.Match(held);
            if (!found.Success) return new PdfDefaultAppearance(held, false, string.Empty, 0.0);

            double size;
            if (!double.TryParse(found.Groups["size"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out size))
            {
                return new PdfDefaultAppearance(held, false, string.Empty, 0.0);
            }

            return new PdfDefaultAppearance(held, true, found.Groups["font"].Value, size);
        }

        /// <summary>
        /// The same appearance with one number changed. **The font name and the colour operators
        /// either side of it are carried through untouched**, because they are the client's and
        /// this tool is only making their text fit their own box.
        /// </summary>
        public string WithSize(double size)
        {
            if (!Read) return Text;

            return TextFont.Replace(
                Text,
                one => "/" + one.Groups["font"].Value + " "
                    + size.ToString("0.###", CultureInfo.InvariantCulture) + " Tf",
                1);
        }
    }

    /// <summary>
    /// One font's character widths, in thousandths of the size, which is how a PDF states them.
    ///
    /// **A WIDTH IS READ OR IT IS UNKNOWN, AND IT IS NEVER GUESSED.** Two sources, in order: the
    /// font object's own `/Widths` array where it carries one, and the published metrics of the
    /// standard fourteen where it does not. A font that is neither answers that it cannot measure
    /// and the field is written at the client's size with the font named.
    ///
    /// **THE STANDARD FOURTEEN TABLES ARE THE PUBLISHED CORE FONT METRICS, HELD AS DATA.** No AFM
    /// file is in this repository and none ever will be, so they are not measured here, which is
    /// written down rather than left to be assumed. What a width being wrong can cost is bounded
    /// on purpose: the VALUE is never changed, the size is never raised above the client's own,
    /// and a width out by a few thousandths moves a size by a tenth of a point. The read back
    /// after the write is what says which size really landed.
    ///
    /// Anchored on the two widths Bader's own worked example names: Helvetica's digits are 556
    /// and its full stop is 278, so `2797.64` is six digits and a full stop, 3.614 em.
    /// </summary>
    public sealed class PdfFontWidths
    {
        /// <summary>The first character code the tables cover, which is the space.</summary>
        public const int FirstCode = 32;

        /// <summary>The last, which is the tilde. Everything printable this tool writes is inside.</summary>
        public const int LastCode = 126;

        private readonly IReadOnlyDictionary<int, int> _widths;

        private PdfFontWidths(string baseFont, bool read, IReadOnlyDictionary<int, int> widths, string why)
        {
            BaseFont = baseFont ?? string.Empty;
            Read = read;
            _widths = widths ?? new Dictionary<int, int>();
            Why = why ?? string.Empty;
        }

        public string BaseFont { get; }

        public bool Read { get; }

        /// <summary>Empty where the widths were read. Never empty where they were not.</summary>
        public string Why { get; }

        public static PdfFontWidths NotRead(string baseFont, string why)
        {
            return new PdfFontWidths(baseFont, false, null, why);
        }

        public static PdfFontWidths Of(string baseFont, IReadOnlyDictionary<int, int> widths)
        {
            return new PdfFontWidths(baseFont, true, widths, string.Empty);
        }

        /// <summary>
        /// How wide the text is in ems, or a negative number where any one character has no
        /// width. **One character nobody can measure makes the whole string unmeasurable**,
        /// because a sum missing a term is a narrower string than the real one and would shrink
        /// the text too little rather than too much.
        /// </summary>
        public double Ems(string text)
        {
            string held = text ?? string.Empty;
            double found = 0.0;

            foreach (char one in held)
            {
                int width;
                if (!_widths.TryGetValue(one, out width)) return -1.0;

                found = found + width;
            }

            return found / 1000.0;
        }

        /// <summary>
        /// The character this font has no width for, for a reason a person can act on.
        /// </summary>
        public string FirstUnmeasurable(string text)
        {
            foreach (char one in text ?? string.Empty)
            {
                if (!_widths.ContainsKey(one)) return one.ToString();
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// The published widths of the standard fourteen PDF fonts, held as data.
    ///
    /// **A SUBSET PREFIX IS NOT PART OF THE NAME.** A `/BaseFont` can read `ABCDEF+Helvetica`,
    /// which is one font subset into a file, so the six letters and the plus come off before the
    /// lookup. Everything else is matched whole and without case, the same rule every other whole
    /// name lookup in this tool follows.
    ///
    /// **THE OBLIQUE FACES SHARE THEIR UPRIGHT'S WIDTHS AND THE ITALIC TIMES FACES DO NOT.**
    /// Helvetica-Oblique and Helvetica-BoldOblique are Helvetica and Helvetica-Bold slanted, with
    /// the same metrics, and Times-Italic is a different design with its own. That is why the two
    /// families are written out differently here rather than one rule covering both.
    ///
    /// **SYMBOL AND ZAPFDINGBATS ARE DELIBERATELY NOT HELD.** Their widths are keyed on a
    /// character set that is not Latin at all, so a lookup by the character this tool wrote would
    /// answer for a glyph nobody meant. They fall to widths unknown and are named.
    ///
    /// **AND NO NEAR MISS IS IN THIS TABLE EITHER.** Arial is metric compatible with Helvetica
    /// and Courier New with Courier, and neither is written in here, because this table is the
    /// standard fourteen and nothing else. A font that is not one of them is not one of them: it
    /// must carry its own `/Widths` in the file, which is where a Courier New field's widths
    /// really are, and where it does not the field is written unchanged and named. **A name
    /// standing in for a measurement is the fault this repository keeps paying for.**
    /// </summary>
    public static class StandardFonts
    {
        private static readonly Regex SubsetPrefix = new Regex(@"^[A-Z]{6}\+", RegexOptions.CultureInvariant);

        public const string Courier = "Courier";

        /// <summary>Every Courier face is monospaced at 600, which is what monospaced means.</summary>
        public const int CourierWidth = 600;

        private static readonly int[] Helvetica =
        {
            278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,
            1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,
            667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,
            333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
            556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584
        };

        private static readonly int[] HelveticaBold =
        {
            278, 333, 474, 556, 556, 889, 722, 238, 333, 333, 389, 584, 278, 333, 278, 278,
            556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 333, 333, 584, 584, 584, 611,
            975, 722, 722, 722, 722, 667, 611, 778, 722, 278, 556, 722, 611, 833, 722, 778,
            667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 333, 278, 333, 584, 556,
            333, 556, 611, 556, 611, 556, 333, 611, 611, 278, 278, 556, 278, 889, 611, 611,
            611, 611, 389, 556, 333, 611, 556, 778, 556, 556, 500, 389, 280, 389, 584
        };

        private static readonly int[] TimesRoman =
        {
            250, 333, 408, 500, 500, 833, 778, 180, 333, 333, 500, 564, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 278, 278, 564, 564, 564, 444,
            921, 722, 667, 667, 722, 611, 556, 722, 722, 333, 389, 722, 611, 889, 722, 722,
            556, 722, 667, 556, 611, 722, 722, 944, 722, 722, 611, 333, 278, 333, 469, 500,
            333, 444, 500, 444, 500, 444, 333, 500, 500, 278, 278, 500, 278, 778, 500, 500,
            500, 500, 333, 389, 278, 500, 500, 722, 500, 500, 444, 480, 200, 480, 541
        };

        private static readonly int[] TimesBold =
        {
            250, 333, 555, 500, 500, 1000, 833, 278, 333, 333, 500, 570, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 333, 333, 570, 570, 570, 500,
            930, 722, 667, 722, 722, 667, 611, 778, 778, 389, 500, 778, 667, 944, 722, 778,
            611, 778, 722, 556, 667, 722, 722, 1000, 722, 722, 667, 333, 278, 333, 581, 500,
            333, 500, 556, 444, 556, 444, 333, 500, 556, 278, 333, 556, 278, 833, 556, 500,
            556, 556, 444, 389, 333, 556, 500, 722, 500, 500, 444, 394, 220, 394, 520
        };

        private static readonly int[] TimesItalic =
        {
            250, 333, 420, 500, 500, 833, 778, 214, 333, 333, 500, 675, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 333, 333, 675, 675, 675, 500,
            920, 611, 611, 667, 722, 611, 611, 722, 722, 333, 444, 667, 556, 833, 667, 722,
            611, 722, 611, 500, 556, 722, 611, 833, 611, 556, 556, 389, 278, 389, 422, 500,
            333, 500, 500, 444, 500, 444, 278, 500, 500, 278, 278, 444, 278, 722, 500, 500,
            500, 500, 389, 389, 278, 500, 444, 667, 444, 444, 389, 400, 275, 400, 541
        };

        private static readonly int[] TimesBoldItalic =
        {
            250, 389, 555, 500, 500, 833, 778, 278, 333, 333, 500, 570, 250, 333, 250, 278,
            500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 333, 333, 570, 570, 570, 500,
            832, 667, 667, 667, 722, 667, 667, 722, 778, 389, 500, 667, 611, 889, 722, 722,
            611, 722, 667, 556, 611, 722, 667, 889, 667, 611, 611, 333, 278, 333, 570, 500,
            333, 500, 500, 444, 500, 444, 333, 500, 556, 278, 278, 500, 278, 778, 556, 500,
            500, 500, 389, 389, 278, 556, 444, 667, 500, 444, 389, 348, 220, 348, 570
        };

        private static readonly Dictionary<string, int[]> Tables =
            new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "Helvetica", Helvetica },
                { "Helvetica-Oblique", Helvetica },
                { "Helvetica-Bold", HelveticaBold },
                { "Helvetica-BoldOblique", HelveticaBold },
                { "Times-Roman", TimesRoman },
                { "Times-Bold", TimesBold },
                { "Times-Italic", TimesItalic },
                { "Times-BoldItalic", TimesBoldItalic }
            };

        private static readonly HashSet<string> CourierFaces =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Courier", "Courier-Bold", "Courier-Oblique", "Courier-BoldOblique"
            };

        public const string NotOneOfTheFourteen =
            "its widths are in neither the font object nor the published metrics this tool holds";

        /// <summary>The name with any six letter subset prefix off, edge whitespace off.</summary>
        public static string Named(string baseFont)
        {
            string held = LabelText.Trimmed(baseFont);
            if (held.StartsWith("/", StringComparison.Ordinal)) held = held.Substring(1);

            return SubsetPrefix.Replace(held, string.Empty);
        }

        public static PdfFontWidths For(string baseFont)
        {
            string name = Named(baseFont);
            if (name.Length == 0) return PdfFontWidths.NotRead(name, "the font names no /BaseFont");

            if (CourierFaces.Contains(name))
            {
                var flat = new Dictionary<int, int>();
                for (int code = PdfFontWidths.FirstCode; code <= PdfFontWidths.LastCode; code++)
                {
                    flat[code] = CourierWidth;
                }

                return PdfFontWidths.Of(name, flat);
            }

            int[] table;
            if (!Tables.TryGetValue(name, out table))
            {
                return PdfFontWidths.NotRead(name, NotOneOfTheFourteen);
            }

            var found = new Dictionary<int, int>();
            for (int code = PdfFontWidths.FirstCode; code <= PdfFontWidths.LastCode; code++)
            {
                found[code] = table[code - PdfFontWidths.FirstCode];
            }

            return PdfFontWidths.Of(name, found);
        }

        /// <summary>Every name this table answers for, so a test can walk them.</summary>
        public static IReadOnlyList<string> All
        {
            get { return Tables.Keys.Concat(CourierFaces).ToList(); }
        }
    }
}
