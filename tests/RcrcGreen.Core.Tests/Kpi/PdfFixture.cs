using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// A small AcroForm PDF, built here in the temp folder.
    ///
    /// **NO CLIENT PDF ENTERS THIS REPOSITORY.** The Projects Basic Data forms carry the
    /// client's branding, the project name, the consultant and a real contract reference, and
    /// this repository is public, so `*.pdf` is ignored and every case below builds its own.
    ///
    /// It carries the three shapes that matter: a plain named field, a field with a stale
    /// appearance to be dropped, and a field whose own title is 0 under a parent titled
    /// Numbers, which is how the open spaces form really names its lawn area.
    /// </summary>
    public static class PdfFixture
    {
        public const string ParentedFieldName = "Numbers.0";

        /// <summary>
        /// One field of a built form: its name, the note the client writes into the value, and
        /// where it sits on the page. **The position is the third record the check reads**, so a
        /// fixture that left every field at the origin would pass a check the real files fail.
        /// </summary>
        public sealed class FixtureField
        {
            public FixtureField(
                string name, string note, double x, double y, bool aTickBox = false,
                double width = 10.0, double height = 10.0, string defaultAppearance = null)
            {
                Name = name;
                Note = note;
                X = x;
                Y = y;
                IsATickBox = aTickBox;
                Width = width;
                Height = height;
                DefaultAppearance = defaultAppearance ?? "/Helv 0 Tf 0 g";
            }

            /// <summary>The box's own size, which is what the text has to fit inside.</summary>
            public double Width { get; }

            public double Height { get; }

            /// <summary>The field's own /DA, so a case can give the client a real size.</summary>
            public string DefaultAppearance { get; }

            /// <summary>Built as /FT/Btn rather than /FT/Tx, which is what a stage box is.</summary>
            public bool IsATickBox { get; }

            public string Name { get; }

            public string Note { get; }

            public double X { get; }

            public double Y { get; }
        }

        public static string Folder()
        {
            string path = Path.Combine(Path.GetTempPath(), "rcrc-pdf-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// One form with the named fields, each holding the value given, written to a real file.
        /// </summary>
        public static string Form(string folder, string fileName, IEnumerable<FixtureField> fields)
        {
            string path = Path.Combine(folder, fileName);
            File.WriteAllBytes(path, Bytes(fields));
            return path;
        }

        public static byte[] Bytes(IEnumerable<FixtureField> fields)
        {
            var held = new List<FixtureField>(fields);

            var objects = new List<string>();
            var references = new List<string>();

            // 1 catalog, 2 pages, 3 the AcroForm, 4 the page, 5 the stale appearance.
            int at = 6;
            var annotations = new StringBuilder();

            foreach (FixtureField one in held)
            {
                references.Add(at + " 0 R");
                annotations.Append(at + " 0 R ");

                if (string.Equals(one.Name, ParentedFieldName, StringComparison.Ordinal))
                {
                    // The parent carries the name Numbers and the kid carries 0, so the full
                    // name is built through the chain rather than read off either one.
                    objects.Add(at + " 0 obj\n<</FT/Tx/T(Numbers)/Kids[" + (at + 1) + " 0 R]>>\nendobj\n");
                    objects.Add((at + 1) + " 0 obj\n<</Parent " + at + " 0 R/T(0)/V"
                        + Literal(one.Note) + "/Type/Annot/Subtype/Widget/Rect" + Rectangle(one) + "/P 4 0 R"
                        + "/DA" + Literal(one.DefaultAppearance) + ">>\nendobj\n");
                    at = at + 2;
                    continue;
                }

                // The second field carries an appearance, so a fill can be seen to drop it.
                string appearance = objects.Count == 1 ? "/AP<</N 5 0 R>>" : string.Empty;

                // **A TICK BOX IS A BUTTON AND IS LEFT EXACTLY AS IT IS.** The four stage boxes
                // and the Reset button on the client's forms are the only fields the tool never
                // touches, and it leaves them because of what they ARE rather than because
                // anybody wrote their names down.
                if (one.IsATickBox)
                {
                    objects.Add(at + " 0 obj\n<</Type/Annot/Subtype/Widget/FT/Btn/T" + Literal(one.Name)
                        + "/V" + Literal(one.Note)
                        + "/DA" + Literal(one.DefaultAppearance) + "/Rect" + Rectangle(one)
                        + "/P 4 0 R>>\nendobj\n");
                    at = at + 1;
                    continue;
                }

                objects.Add(at + " 0 obj\n<</Type/Annot/Subtype/Widget/FT/Tx/T" + Literal(one.Name)
                    + "/V" + Literal(one.Note) + appearance
                    + "/DA" + Literal(one.DefaultAppearance) + "/Rect" + Rectangle(one)
                    + "/P 4 0 R>>\nendobj\n");
                at = at + 1;
            }

            // **THE FONT THE /DA NAMES IS A RESOURCE, and the AcroForm's own /DR is where the
            // name is resolved.** A fixture with no /DR would leave every width unknown, which
            // is a path the real files do not take, so it carries one.
            int fontNumber = at;
            at = at + 1;

            var built = new List<string>
            {
                "1 0 obj\n<</Type/Catalog/Pages 2 0 R/AcroForm 3 0 R>>\nendobj\n",
                "2 0 obj\n<</Type/Pages/Kids[4 0 R]/Count 1>>\nendobj\n",
                "3 0 obj\n<</Fields[" + string.Join(" ", references.ToArray())
                    + "]/DA(/Helv 0 Tf 0 g)/DR<</Font<</Helv " + fontNumber + " 0 R>>>>>>\nendobj\n",
                "4 0 obj\n<</Type/Page/Parent 2 0 R/MediaBox[0 0 200 200]/Annots["
                    + annotations.ToString().Trim() + "]>>\nendobj\n",
                "5 0 obj\n<</Type/XObject/Subtype/Form/BBox[0 0 10 10]/Length 0>>\nstream\n\nendstream\nendobj\n"
            };
            built.AddRange(objects);
            built.Add(fontNumber + " 0 obj\n<</Type/Font/Subtype/Type1/BaseFont/Helvetica/Encoding/WinAnsiEncoding>>\nendobj\n");

            var file = new StringBuilder("%PDF-1.7\n");
            var offsets = new Dictionary<int, int>();

            foreach (string one in built)
            {
                int number = int.Parse(one.Substring(0, one.IndexOf(' ')), CultureInfo.InvariantCulture);
                offsets[number] = file.Length;
                file.Append(one);
            }

            int crossReferenceAt = file.Length;
            int size = at;
            file.Append("xref\n0 " + size + "\n0000000000 65535 f \n");
            for (int number = 1; number < size; number++)
            {
                file.Append(offsets.ContainsKey(number)
                    ? offsets[number].ToString("0000000000", CultureInfo.InvariantCulture) + " 00000 n \n"
                    : "0000000000 65535 f \n");
            }

            file.Append("trailer\n<</Size " + size + "/Root 1 0 R>>\nstartxref\n" + crossReferenceAt + "\n%%EOF\n");

            var bytes = new byte[file.Length];
            for (int one = 0; one < file.Length; one++) bytes[one] = (byte)file[one];
            return bytes;
        }

        private static string Literal(string text)
        {
            return "(" + (text ?? string.Empty).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)") + ")";
        }

        public static FixtureField Field(string name, string note, double x = 0.0, double y = 0.0)
        {
            return new FixtureField(name, note, x, y);
        }

        /// <summary>
        /// **The client's own header, which every one of their three forms carries.** The two the
        /// tool leaves alone and the one it now writes its PRX_Plot_NH into, each holding the
        /// value measured off their files on 14 September. The field NAMES here are the
        /// fixture's own, because the real ones are measured nowhere in this repository and no
        /// client PDF may enter it: the tool finds all three by their value, which is exactly
        /// what this proves.
        /// </summary>
        public static IEnumerable<FixtureField> ClientHeader()
        {
            return new[]
            {
                Field("Project name", "Neighborhood Landscape Design - Zone #2", 60.0, 700.0),
                Field("Consultant", "SAPL", 60.0, 690.0),
                Field("Contract reference", "GP.NH.Z2.052-DES042", 60.0, 680.0)
            };
        }

        /// <summary>
        /// A stage tick box, built as a button so the emptying rule meets the thing it must
        /// leave alone rather than a text field pretending to be one.
        /// </summary>
        public static FixtureField TickBox(string name, string held)
        {
            return new FixtureField(name, held, 0.0, 0.0, true);
        }

        private static string Rectangle(FixtureField one)
        {
            return "[" + Number(one.X) + " " + Number(one.Y) + " "
                + Number(one.X + one.Width) + " " + Number(one.Y + one.Height) + "]";
        }

        /// <summary>
        /// A field with a box of its own size, for the cases about how big the text comes out.
        /// </summary>
        public static FixtureField Sized(
            string name, string note, double width, double height, string defaultAppearance = null)
        {
            return new FixtureField(name, note, 0.0, 0.0, false, width, height, defaultAppearance);
        }

        private static string Number(double value)
        {
            return value.ToString("0.######", CultureInfo.InvariantCulture);
        }
    }
}
