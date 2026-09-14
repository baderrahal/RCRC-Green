using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One form field as the file holds it: its full name and the value sitting in it.
    /// </summary>
    public sealed class PdfFieldRead
    {
        public PdfFieldRead(string name, string value, int objectNumber, double x = 0.0, double y = 0.0)
        {
            Name = name ?? string.Empty;
            Value = value ?? string.Empty;
            ObjectNumber = objectNumber;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Where the field sits on the page, off its own rectangle. Nought where the field
        /// carries none, which is a parent that only holds kids.
        /// </summary>
        public double X { get; }

        public double Y { get; }

        public string Name { get; }

        public string Value { get; }

        public int ObjectNumber { get; }
    }

    /// <summary>
    /// Reads an AcroForm's fields and fills them, **by copying the file and appending, never by
    /// rebuilding it**.
    ///
    /// **THIS IS THE WORKBOOK'S RULE APPLIED TO A PDF.** The client's forms carry their
    /// branding, their layout, their images and their Reset button, and a rebuilt page is not
    /// their document. A PDF supports an incremental update by its own design: the original
    /// bytes are copied whole, the changed objects are appended after them with a new cross
    /// reference section, and every byte of the client's file is still there, in order,
    /// untouched by construction.
    ///
    /// **NO PACKAGE, AND THAT IS A MEASUREMENT RATHER THAN A PREFERENCE.** PDFsharp 6.2.2 is
    /// MIT, ships netstandard2.0 so net48 can consume it, and carries PdfAcroForm and
    /// PdfTextField. Opened on the client's own Parks form it read all 42 fields and then threw
    /// on the first one touched: `No appropriate font found for family name 'Courier New'`,
    /// because setting a value makes it REGENERATE the field's appearance stream. Regenerating
    /// the appearance is redrawing what the client drew, in whatever font the machine resolves,
    /// which is the one thing this round forbids. Every other library found is commercial and
    /// per seat.
    ///
    /// **The stale appearance is dropped and NeedAppearances is set**, so the viewer draws the
    /// new value using the field's own default appearance, which the client set. Keeping the old
    /// appearance beside a new value would show the client's note on screen over the number
    /// underneath it, and a stale word that looks like an answer is the worst thing this tool
    /// can put in a file.
    ///
    /// **It works because these files hold no object streams.** Measured on all three: zero
    /// /ObjStm, no encryption, so every field dictionary is a plain top level object and can be
    /// found and redefined. A form that arrives with object streams is refused by name rather
    /// than half written.
    /// </summary>
    public static class PdfFormFile
    {
        public const string HoldsObjectStreams =
            "this PDF holds compressed object streams, which this tool does not read, so nothing "
            + "was written into it";

        public const string NoAcroForm = "this PDF holds no AcroForm, so it has no fields to fill";

        public const string NoCatalog = "this PDF names no document catalog, so nothing could be read off it";

        private static readonly Regex TopLevelObject =
            new Regex(@"(?<![0-9])(\d+)\s+(\d+)\s+obj\b", RegexOptions.CultureInvariant);

        private static readonly Regex FieldTitle =
            new Regex(@"/T\s*\((?<text>(?:[^()\\]|\\.)*)\)", RegexOptions.CultureInvariant);

        private static readonly Regex FieldValueLiteral =
            new Regex(@"/V\s*\((?:[^()\\]|\\.)*\)", RegexOptions.CultureInvariant);

        private static readonly Regex FieldValueHex =
            new Regex(@"/V\s*<[0-9A-Fa-f\s]*>", RegexOptions.CultureInvariant);

        private static readonly Regex Rectangle = new Regex(
            @"/Rect\s*\[\s*(-?[0-9.]+)\s+(-?[0-9.]+)", RegexOptions.CultureInvariant);

        private static readonly Regex ParentReference =
            new Regex(@"/Parent\s+(\d+)\s+\d+\s+R", RegexOptions.CultureInvariant);

        private static readonly Regex AcroFormReference =
            new Regex(@"/AcroForm\s+(\d+)\s+\d+\s+R", RegexOptions.CultureInvariant);

        private static readonly Regex RootReference =
            new Regex(@"/Root\s+(\d+)\s+\d+\s+R", RegexOptions.CultureInvariant);

        private static readonly Regex StartXref =
            new Regex(@"startxref\s+(\d+)", RegexOptions.CultureInvariant);

        private static readonly Regex Appearance =
            new Regex(@"/AP\s*<<[^>]*>>\s*", RegexOptions.CultureInvariant);

        private static readonly Regex NeedAppearances =
            new Regex(@"/NeedAppearances\s+(?:true|false)", RegexOptions.CultureInvariant);

        /// <summary>
        /// Every field the file holds, by its FULL name, built through the parent chain.
        ///
        /// **The full name is what the tool knows a form by.** On the open spaces form the lawn
        /// area sits in a field whose own title is 0_2, and the total trees in one titled 1
        /// under a parent titled Proposed Trees, so a title on its own names nothing.
        /// </summary>
        public static IReadOnlyList<PdfFieldRead> Fields(byte[] file, out string refusal)
        {
            refusal = string.Empty;
            if (file == null || file.Length == 0)
            {
                refusal = "the file is empty";
                return new List<PdfFieldRead>();
            }

            string text = Latin(file);

            if (text.IndexOf("/ObjStm", StringComparison.Ordinal) >= 0)
            {
                refusal = HoldsObjectStreams;
                return new List<PdfFieldRead>();
            }

            Dictionary<int, string> bodies = Bodies(text);

            if (!RootReference.IsMatch(text))
            {
                refusal = NoCatalog;
                return new List<PdfFieldRead>();
            }

            if (!AcroFormReference.IsMatch(text))
            {
                refusal = NoAcroForm;
                return new List<PdfFieldRead>();
            }

            var titles = new Dictionary<int, string>();
            var parents = new Dictionary<int, int>();

            foreach (KeyValuePair<int, string> one in bodies)
            {
                Match title = FieldTitle.Match(one.Value);
                if (!title.Success) continue;

                titles[one.Key] = Unescaped(title.Groups["text"].Value);

                Match parent = ParentReference.Match(one.Value);
                if (parent.Success) parents[one.Key] = Number(parent.Groups[1].Value);
            }

            var found = new List<PdfFieldRead>();
            foreach (KeyValuePair<int, string> one in titles)
            {
                Match where = Rectangle.Match(bodies[one.Key]);

                found.Add(new PdfFieldRead(
                    FullName(one.Key, titles, parents), ValueOf(bodies[one.Key]), one.Key,
                    where.Success ? Rounded(where.Groups[1].Value) : 0.0,
                    where.Success ? Rounded(where.Groups[2].Value) : 0.0));
            }

            return found.OrderBy(one => one.Name, StringComparer.Ordinal).ToList();
        }

        /// <summary>
        /// The file with the named fields filled, as new bytes. The source bytes are the first
        /// bytes of the answer, unchanged, and everything this writes is appended after them.
        /// </summary>
        public static byte[] Filled(byte[] file, IEnumerable<KeyValuePair<string, string>> values, out string refusal)
        {
            IReadOnlyList<PdfFieldRead> fields = Fields(file, out refusal);
            if (refusal.Length > 0) return null;

            var wanted = (values ?? Enumerable.Empty<KeyValuePair<string, string>>()).ToList();

            string text = Latin(file);
            Dictionary<int, string> bodies = Bodies(text);
            var changed = new Dictionary<int, string>();

            foreach (KeyValuePair<string, string> one in wanted)
            {
                PdfFieldRead field = fields.FirstOrDefault(
                    held => string.Equals(held.Name, one.Key, StringComparison.Ordinal));

                if (field == null)
                {
                    refusal = "the field " + one.Key + " is not in this file";
                    return null;
                }

                changed[field.ObjectNumber] = WithValue(bodies[field.ObjectNumber], one.Value);
            }

            int acroForm = Number(AcroFormReference.Match(text).Groups[1].Value);
            if (!bodies.ContainsKey(acroForm))
            {
                refusal = "the AcroForm object " + acroForm + " is not in this file";
                return null;
            }

            if (!changed.ContainsKey(acroForm)) changed[acroForm] = bodies[acroForm];
            changed[acroForm] = WithNeedAppearances(changed[acroForm]);

            return Appended(file, text, bodies, changed, out refusal);
        }

        /// <summary>
        /// The original bytes, then one object per change, then a cross reference section and a
        /// trailer pointing back at the one the file already had.
        /// </summary>
        private static byte[] Appended(
            byte[] file, string text, Dictionary<int, string> bodies,
            Dictionary<int, string> changed, out string refusal)
        {
            refusal = string.Empty;

            MatchCollection starts = StartXref.Matches(text);
            if (starts.Count == 0)
            {
                refusal = "this PDF names no cross reference table, so nothing was appended to it";
                return null;
            }

            int previous = Number(starts[starts.Count - 1].Groups[1].Value);
            int root = Number(RootReference.Match(text).Groups[1].Value);
            int size = bodies.Keys.Max() + 1;

            var appended = new StringBuilder();
            if (file.Length > 0 && file[file.Length - 1] != (byte)'\n') appended.Append("\n");

            var offsets = new Dictionary<int, int>();
            foreach (int number in changed.Keys.OrderBy(one => one))
            {
                offsets[number] = file.Length + appended.Length;
                appended.Append(number.ToString(CultureInfo.InvariantCulture));
                appended.Append(" 0 obj");
                appended.Append(changed[number]);
                appended.Append("endobj\n");
            }

            int crossReferenceAt = file.Length + appended.Length;
            appended.Append("xref\n");
            foreach (int number in changed.Keys.OrderBy(one => one))
            {
                appended.Append(number.ToString(CultureInfo.InvariantCulture));
                appended.Append(" 1\n");
                appended.Append(offsets[number].ToString("0000000000", CultureInfo.InvariantCulture));
                appended.Append(" 00000 n \n");
            }

            appended.Append("trailer\n<</Size ");
            appended.Append(size.ToString(CultureInfo.InvariantCulture));
            appended.Append("/Root ");
            appended.Append(root.ToString(CultureInfo.InvariantCulture));
            appended.Append(" 0 R/Prev ");
            appended.Append(previous.ToString(CultureInfo.InvariantCulture));
            appended.Append(">>\nstartxref\n");
            appended.Append(crossReferenceAt.ToString(CultureInfo.InvariantCulture));
            appended.Append("\n%%EOF\n");

            byte[] tail = Bytes(appended.ToString());
            var whole = new byte[file.Length + tail.Length];
            Buffer.BlockCopy(file, 0, whole, 0, file.Length);
            Buffer.BlockCopy(tail, 0, whole, file.Length, tail.Length);

            return whole;
        }

        /// <summary>
        /// The field dictionary with its value replaced and its stale appearance dropped.
        /// </summary>
        private static string WithValue(string body, string value)
        {
            string without = Appearance.Replace(body, string.Empty, 1);
            string written = "/V" + Written(value);

            if (FieldValueLiteral.IsMatch(without)) return FieldValueLiteral.Replace(without, written, 1);
            if (FieldValueHex.IsMatch(without)) return FieldValueHex.Replace(without, written, 1);

            int closes = without.LastIndexOf(">>", StringComparison.Ordinal);
            return closes < 0 ? without : without.Substring(0, closes) + written + without.Substring(closes);
        }

        private static string WithNeedAppearances(string body)
        {
            if (NeedAppearances.IsMatch(body))
            {
                return NeedAppearances.Replace(body, "/NeedAppearances true", 1);
            }

            int closes = body.LastIndexOf(">>", StringComparison.Ordinal);
            return closes < 0 ? body : body.Substring(0, closes) + "/NeedAppearances true" + body.Substring(closes);
        }

        /// <summary>
        /// A PDF string. Plain text goes in as a literal so the file stays readable, and
        /// anything else as UTF-16 with a byte order mark, which is how a PDF carries a square
        /// metre sign.
        /// </summary>
        private static string Written(string value)
        {
            string held = value ?? string.Empty;

            if (held.All(one => one < 128 && one >= 32))
            {
                return "(" + held.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)") + ")";
            }

            var hex = new StringBuilder("<FEFF");
            foreach (byte one in Encoding.BigEndianUnicode.GetBytes(held))
            {
                hex.Append(one.ToString("X2", CultureInfo.InvariantCulture));
            }

            return hex.Append(">").ToString();
        }

        private static string ValueOf(string body)
        {
            Match literal = FieldValueLiteral.Match(body);
            if (literal.Success)
            {
                string inside = literal.Value;
                int opens = inside.IndexOf('(');
                return Unescaped(inside.Substring(opens + 1, inside.Length - opens - 2));
            }

            Match hex = FieldValueHex.Match(body);
            if (!hex.Success) return string.Empty;

            string digits = new string(hex.Value.Where(Uri.IsHexDigit).ToArray());
            if (digits.Length < 2 || digits.Length % 2 != 0) return string.Empty;

            var bytes = new byte[digits.Length / 2];
            for (int at = 0; at < bytes.Length; at++)
            {
                bytes[at] = byte.Parse(digits.Substring(at * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF
                ? Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2)
                : Encoding.ASCII.GetString(bytes);
        }

        private static string FullName(int number, Dictionary<int, string> titles, Dictionary<int, int> parents)
        {
            var parts = new List<string> { titles[number] };
            var seen = new HashSet<int> { number };
            int at = number;

            while (parents.ContainsKey(at) && titles.ContainsKey(parents[at]) && !seen.Contains(parents[at]))
            {
                at = parents[at];
                seen.Add(at);
                parts.Add(titles[at]);
            }

            parts.Reverse();
            return string.Join(".", parts.ToArray());
        }

        private static Dictionary<int, string> Bodies(string text)
        {
            var found = new Dictionary<int, string>();
            foreach (Match one in TopLevelObject.Matches(text))
            {
                int ends = text.IndexOf("endobj", one.Index + one.Length, StringComparison.Ordinal);
                if (ends < 0) continue;

                found[Number(one.Groups[1].Value)] =
                    text.Substring(one.Index + one.Length, ends - one.Index - one.Length);
            }

            return found;
        }

        private static string Unescaped(string text)
        {
            var built = new StringBuilder();
            for (int at = 0; at < text.Length; at++)
            {
                if (text[at] == '\\' && at + 1 < text.Length)
                {
                    at++;
                    built.Append(text[at]);
                    continue;
                }

                built.Append(text[at]);
            }

            return built.ToString();
        }

        /// <summary>
        /// A position to a tenth of a point, which is how the measurements were taken and far
        /// finer than any row is tall. The rows of the shrubs table sit about twenty points
        /// apart, so a tenth cannot confuse two of them.
        /// </summary>
        private static double Rounded(string text)
        {
            return Math.Round(double.Parse(text, CultureInfo.InvariantCulture), 1);
        }

        private static int Number(string text)
        {
            return int.Parse(text, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// A PDF's structure is bytes rather than text, so it is read one byte to one character
        /// and written back the same way. Nothing here decodes the page content.
        /// </summary>
        private static string Latin(byte[] file)
        {
            var built = new StringBuilder(file.Length);
            foreach (byte one in file) built.Append((char)one);
            return built.ToString();
        }

        private static byte[] Bytes(string text)
        {
            var found = new byte[text.Length];
            for (int at = 0; at < text.Length; at++) found[at] = (byte)text[at];
            return found;
        }

        public static byte[] Read(string path)
        {
            return File.ReadAllBytes(path);
        }
    }
}
