using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using RcrcGreen.Core.Kpi;
using Xunit;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Builds a template whose main sheet carries labels where the real ones carry them. **No
    /// client workbook enters this public repository**, so the three measured layouts are rebuilt
    /// here from the cells Bader named.
    /// </summary>
    internal static class LabelFixture
    {
        public static string Folder()
        {
            string folder = Path.Combine(
                Path.GetTempPath(), "rcrc-kpi-labels", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// One cell of row 7. A value beginning with = is written as a formula, because the
        /// street's Category holds one and a value cell that is a formula must never be written
        /// over.
        /// </summary>
        public static string Create(
            string folder, string mainSheetName, IDictionary<string, string> cells,
            string fileName = "template.xlsx")
        {
            string path = Path.Combine(folder, fileName);
            var sheet = new StringBuilder();

            sheet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sheet.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sheet.Append("<sheetData>");

            foreach (IGrouping<int, KeyValuePair<string, string>> row in cells
                .GroupBy(one => CellRef.Parse(one.Key).Row)
                .OrderBy(one => one.Key))
            {
                sheet.Append("<row r=\"" + row.Key + "\">");
                foreach (KeyValuePair<string, string> cell in row
                    .OrderBy(one => CellRef.Parse(one.Key).ColumnNumber))
                {
                    // **The value is escaped as well as the sheet name.** A cell holding the
                    // R1 set's own <Date> wrote a start tag into the part and produced a file
                    // no XML reader opens, which the fixture reported as a workbook fault.
                    sheet.Append(cell.Value.StartsWith("=", StringComparison.Ordinal)
                        ? "<c r=\"" + cell.Key + "\"><f>"
                            + Escaped(cell.Value.Substring(1)) + "</f><v>0</v></c>"
                        : "<c r=\"" + cell.Key + "\" t=\"inlineStr\"><is><t>"
                            + Escaped(cell.Value) + "</t></is></c>");
                }

                sheet.Append("</row>");
            }

            sheet.Append("</sheetData></worksheet>");

            using (var file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "</Types>");

                Add(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
                    + "</Relationships>");

                Add(zip, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\""
                    + " xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">"
                    + "<sheets><sheet name=\"" + Escaped(mainSheetName)
                    + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
                    + "</workbook>");

                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "</Relationships>");

                Add(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
            }

            return path;
        }

        /// <summary>
        /// Every real main sheet name is wrapped in angle brackets, and Excel stores them escaped
        /// in the attribute. A fixture that wrote them raw produced a file no XML reader opens,
        /// and so did a cell holding the R1 set's own bracketed placeholders, so both go through
        /// this.
        /// </summary>
        private static string Escaped(string name)
        {
            return (name ?? string.Empty)
                .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private static void Add(ZipArchive zip, string path, string contents)
        {
            using (Stream part = zip.CreateEntry(path).Open())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(contents);
                part.Write(bytes, 0, bytes.Length);
            }
        }
    }

    /// <summary>
    /// Character and Context, found by their labels on each template's own sheet. Every expected
    /// value written out by hand, and the three layouts are the ones measured on the workbooks
    /// written on 13 September.
    /// </summary>
    public class LabelledCellsTests : IDisposable
    {
        private readonly string _folder = LabelFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        /// <summary>
        /// MOSQUES and SCHOOLS: Character label C7 with its value at D7, Context label E7 with
        /// its value at F7. **The mosque file came filled**, so both value cells already hold
        /// what this run would write, and the read says so rather than reading as an empty
        /// template.
        /// </summary>
        [Fact]
        public void OnAMosqueSheetTheLabelsSitAtC7AndE7AndTheValuesBeside()
        {
            string path = LabelFixture.Create(
                _folder, "<Mosques>",
                new Dictionary<string, string>
                {
                    { "C7", "Character" }, { "D7", "Urban Area Zone" },
                    { "E7", "Context" }, { "F7", "Urban" }
                });

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.True(found.Read);
            Assert.Equal("<Mosques>", found.SheetName);

            LabelledCell character = found.For("Character");
            Assert.True(character.Found);
            Assert.Equal("C7", character.LabelCell);
            Assert.Equal("D7", character.ValueCell);
            Assert.Equal("Urban Area Zone", character.Holds);

            LabelledCell context = found.For("Context");
            Assert.True(context.Found);
            Assert.Equal("E7", context.LabelCell);
            Assert.Equal("F7", context.ValueCell);
            Assert.Equal("Urban", context.Holds);
        }

        /// <summary>
        /// **THE STREET SHEET IS THE WHOLE REASON NOTHING HOLDS A LETTER.** Its Category label is
        /// at C7 with a FORMULA at D7, and its Character and Context sit one pair to the right. A
        /// map that put Character at D7 because two templates do would overwrite that formula.
        /// Nothing looks for the word Category, so D7 is never reached.
        /// </summary>
        [Fact]
        public void OnAStreetSheetTheLabelsSitOnePairRightAndTheCategoryFormulaIsNeverReached()
        {
            string path = LabelFixture.Create(
                _folder, "<Streets>",
                new Dictionary<string, string>
                {
                    { "C7", "Category" }, { "D7", "=VLOOKUP(C3,Lists!A:B,2,0)" },
                    { "E7", "Character" }, { "F7", string.Empty },
                    { "G7", "Context" }, { "H7", string.Empty }
                },
                "streets.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Streets);

            Assert.Equal("F7", found.For("Character").ValueCell);
            Assert.Equal("H7", found.For("Context").ValueCell);

            // The street file has both blank, which is what the 00:00 run measured.
            Assert.Equal(string.Empty, found.For("Character").Holds);
            Assert.Equal(string.Empty, found.For("Context").Holds);

            // Nothing anywhere names D7, so the formula cannot be written over.
            Assert.DoesNotContain(found.All, one => one.ValueCell == "D7");
            Assert.DoesNotContain(found.All, one => one.Label == "Category");
        }

        /// <summary>
        /// The plan writes into the cells the labels chose, and into no others. Two writes on
        /// STREETS, F7 and H7, and neither is D8 or D7.
        /// </summary>
        [Fact]
        public void ThePlanWritesTheTwoValuesIntoTheCellsTheLabelsChose()
        {
            string path = LabelFixture.Create(
                _folder, "<Streets>",
                new Dictionary<string, string>
                {
                    { "C7", "Category" }, { "D7", "=1+1" },
                    { "E7", "Character" }, { "G7", "Context" }
                },
                "plan.xlsx");

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Streets, null, null, string.Empty, null, null, null, null,
                null, null, null, StreetReferenceAnswer.Nothing("not looked up"),
                LabelledPlaces.In(path, KpiTemplates.Streets));

            Assert.Equal("Urban Area Zone",
                plan.Writes.Single(one => one.Cell.ToString() == "F7").Stored);
            Assert.Equal("Urban",
                plan.Writes.Single(one => one.Cell.ToString() == "H7").Stored);
            Assert.DoesNotContain(plan.Writes, one => one.Cell.ToString() == "D7");
        }

        /// <summary>
        /// A template naming neither label writes nothing and says so, naming the sheet it looked
        /// on so the next measurement has somewhere to start.
        /// </summary>
        [Fact]
        public void ATemplateNamingNeitherLabelWritesNothingAndSaysSo()
        {
            string path = LabelFixture.Create(
                _folder, "<Healthcare>",
                new Dictionary<string, string> { { "C7", "Something else" } },
                "bare.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Healthcare);

            Assert.True(found.Read);
            Assert.False(found.For("Character").Found);
            Assert.Equal(
                "no cell on <Healthcare> reads Character, so nothing is written and no cell is "
                + "guessed at",
                found.For("Character").Why);

            KpiCreatePlan plan = KpiCreatePlan.Of(
                KpiTemplates.Healthcare, null, null, string.Empty, null, null, null, null,
                null, null, null, StreetReferenceAnswer.Nothing("not a street run"), found);

            Assert.Equal(
                new[] { "Prepared by", "Position", "Reference", "Character", "Context" },
                plan.Skipped
                    .Where(one => one.Why.StartsWith("no cell on", StringComparison.Ordinal))
                    .Select(one => one.What).ToArray());

            // **The date is not in that list and must not be.** It is looked for on the
            // preparer's own row, so a sheet naming no Prepared By: gives it no row to look on,
            // and its reason says that rather than saying no cell reads Date:.
            Assert.Equal(
                new[] { "Date" },
                plan.Skipped
                    .Where(one => one.Why.StartsWith("there is no row to look for", StringComparison.Ordinal))
                    .Select(one => one.What).ToArray());
        }

        /// <summary>
        /// A label on the sheet twice writes nothing and names both cells, because nothing says
        /// which is meant. Same rule as a plot on two rows of the reference file.
        /// </summary>
        [Fact]
        public void ALabelOnTheSheetTwiceNamesBothRatherThanPickingOne()
        {
            string path = LabelFixture.Create(
                _folder, "<Schools>",
                new Dictionary<string, string>
                {
                    { "C7", "Character" }, { "C9", "Character" }, { "E7", "Context" }
                },
                "twice.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Schools);

            Assert.False(found.For("Character").Found);
            Assert.Equal(
                "Character is on <Schools> at C7 and C9, and nothing says which is meant",
                found.For("Character").Why);

            // The other label is unaffected, so one bad label does not cost the other.
            Assert.True(found.For("Context").Found);
            Assert.Equal("F7", found.For("Context").ValueCell);
        }

        /// <summary>
        /// The label is matched whole and without case, so Characteristics is not Character.
        /// </summary>
        [Fact]
        public void TheLabelIsMatchedWholeAndWithoutCase()
        {
            string path = LabelFixture.Create(
                _folder, "<Schools>",
                new Dictionary<string, string>
                {
                    { "B4", "Characteristics" }, { "C7", "  character  " }, { "E7", "CONTEXT" }
                },
                "case.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Schools);

            Assert.Equal("D7", found.For("Character").ValueCell);
            Assert.Equal("F7", found.For("Context").ValueCell);
        }

        /// <summary>
        /// The cell to the right, in the sheet's own letters, including where the column rolls
        /// over from Z. Written out by hand rather than worked out with the same rule.
        /// </summary>
        [Theory]
        [InlineData("C7", "D7")]
        [InlineData("E7", "F7")]
        [InlineData("G7", "H7")]
        [InlineData("A1", "B1")]
        [InlineData("Z3", "AA3")]
        [InlineData("AZ12", "BA12")]
        public void TheValueCellIsTheOneToTheRight(string label, string value)
        {
            Assert.Equal(value, LabelledPlaces.RightOf(CellRef.Parse(label), 1));
        }

        /// <summary>
        /// A template nothing opened writes neither and says which read did not happen, rather
        /// than reading as a sheet that names no label.
        /// </summary>
        [Fact]
        public void ATemplateNothingOpenedSaysTheReadDidNotHappen()
        {
            Assert.False(LabelledCells.NotRead.Read);
            Assert.Equal("the template was not opened", LabelledCells.NotRead.For("Character").Why);
            Assert.False(LabelledPlaces.In(null, KpiTemplates.Mosques).Read);

            LabelledCells missing = LabelledPlaces.In(
                Path.Combine(_folder, "not-here.xlsx"), KpiTemplates.Mosques);

            Assert.False(missing.Read);
            Assert.Contains("could not be opened", missing.Why);
        }

        /// <summary>
        /// A template whose main sheet is not in the file is named with that sheet, because the
        /// labels are looked for on the main sheet and nowhere else.
        /// </summary>
        [Fact]
        public void ATemplateWithoutItsMainSheetNamesTheSheetItLookedFor()
        {
            string path = LabelFixture.Create(
                _folder, "<Something Else>",
                new Dictionary<string, string> { { "C7", "Character" } },
                "wrong-sheet.xlsx");

            LabelledCells found = LabelledPlaces.In(path, KpiTemplates.Mosques);

            Assert.False(found.Read);
            Assert.Equal("the template holds no sheet named <Mosques>", found.Why);
        }
    }
}
