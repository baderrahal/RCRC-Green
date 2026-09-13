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
    /// Builds a small scope validation file by hand in the temp folder. **The team's own file
    /// never enters this public repository**, so what is rebuilt here is its measured shape: one
    /// sheet, the header in row 1, no header cell over column B at all, and the four wanted
    /// columns at D, H, I and O rather than at the front.
    /// </summary>
    internal static class StreetReferenceFixture
    {
        public static string Folder()
        {
            string folder = Path.Combine(
                Path.GetTempPath(), "rcrc-kpi-street", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// One row of the file, in the file's own columns. A value of null leaves the cell out
        /// of the sheet entirely, which is different from the text the real file writes for an
        /// absent value.
        /// </summary>
        public sealed class Row
        {
            public Row(string uid, string quantity, string unit, string width)
            {
                Uid = uid;
                Quantity = quantity;
                Unit = unit;
                Width = width;
            }

            public string Uid { get; }

            public string Quantity { get; }

            public string Unit { get; }

            public string Width { get; }
        }

        public static string Create(string folder, IEnumerable<Row> rows, string fileName = "scope.xlsx",
            string uidHeader = "ID_UID *")
        {
            string path = Path.Combine(folder, fileName);
            var sheet = new StringBuilder();

            sheet.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sheet.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">");
            sheet.Append("<sheetData>");

            // Row 1, the header. A1 holds a bare number and there is NO B1 at all, both measured
            // on the real file, so a reader that counted columns along would be off by one.
            sheet.Append("<row r=\"1\">");
            sheet.Append(Text("A1", "21484"));
            sheet.Append(Text("C1", "Neighborhood English"));
            sheet.Append(Text("D1", uidHeader));
            sheet.Append(Text("G1", "CL_STATUS"));
            sheet.Append(Text("H1", "ES_QUANTITY"));
            sheet.Append(Text("I1", "QUANTITY UNIT"));
            sheet.Append(Text("L1", "CL_CATEGORY"));
            sheet.Append(Text("O1", "ROAD_WIDTH"));
            sheet.Append("</row>");

            int number = 2;
            foreach (Row row in rows)
            {
                sheet.Append("<row r=\"" + number + "\">");
                if (row.Uid != null) sheet.Append(Text("D" + number, row.Uid));
                if (row.Quantity != null) sheet.Append(Text("H" + number, row.Quantity));
                if (row.Unit != null) sheet.Append(Text("I" + number, row.Unit));
                if (row.Width != null) sheet.Append(Text("O" + number, row.Width));
                sheet.Append("</row>");
                number++;
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
                    + "<sheets><sheet name=\"Sheet1\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
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

        private static string Text(string cell, string value)
        {
            return "<c r=\"" + cell + "\" t=\"inlineStr\"><is><t>" + value + "</t></is></c>";
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
    /// The two cells the STREETS template says are typed by hand, off the team's own reference
    /// file. Every expected value written out by hand, and the numbers are real rows off
    /// Scope_Validation_21072026 rather than made up ones.
    /// </summary>
    public class StreetReferenceTests : IDisposable
    {
        private readonly string _folder = StreetReferenceFixture.Folder();

        public void Dispose()
        {
            try { Directory.Delete(_folder, true); }
            catch (IOException) { }
        }

        private static readonly StreetReferenceFixture.Row[] Measured =
        {
            // ANH-007-ST-100210, the row the round message names, read off the real file.
            new StreetReferenceFixture.Row("ANH-007-ST-100210", "330.65849900000001", "m", "20"),
            new StreetReferenceFixture.Row("ANH-007-ST-100133", "615.02812800000004", "m", "30"),

            // A mosque plot: an AREA in sqm, and the road width written as the text the real
            // file uses for an absent value rather than as an empty cell.
            new StreetReferenceFixture.Row("ANH-007-MO-100016", "2221.2786940000001", "sqm", "&lt;Null&gt;"),

            // One UID on two rows. The real file holds 33 such, none of them in ANH-007.
            new StreetReferenceFixture.Row("ANH-013-ST-100420", "1049.461847", "m", "30"),
            new StreetReferenceFixture.Row("ANH-013-ST-100420", "1049.461847", "m", "30")
        };

        private StreetReferenceFile Read()
        {
            return StreetReferenceFile.In(StreetReferenceFixture.Create(_folder, Measured));
        }

        /// <summary>
        /// The columns are found by the names in the header row. They sit at D, H, I and O, and
        /// a reader counting along the row would take C, G and L.
        /// </summary>
        [Fact]
        public void TheColumnsAreFoundByTheirNamesAndNotByWhereTheySit()
        {
            StreetReferenceFile file = Read();

            Assert.True(file.Read);
            Assert.Equal("Sheet1", file.SheetName);
            Assert.Equal(1, file.HeaderRow);
            Assert.Equal(5, file.Rows.Count);

            StreetReferenceRow first = file.Rows[0];
            Assert.Equal("ANH-007-ST-100210", first.Uid);
            Assert.Equal(2, first.Row);
            Assert.Equal("330.65849900000001", first.Quantity);
            Assert.Equal("m", first.Unit);
            Assert.Equal("20", first.RoadWidth);
        }

        /// <summary>
        /// The two numbers a street plot gets. 20 into D8 and 330.658499 into F8, and nothing
        /// into H8, which the workbook works out from the two.
        /// </summary>
        [Fact]
        public void AStreetPlotGetsItsWidthAndItsLength()
        {
            StreetReferenceAnswer answer = Read().For("ANH-007-ST-100210");

            Assert.True(answer.Found);
            Assert.Equal(20.0, answer.Width);
            Assert.Equal(330.658499, answer.Length, 6);
            Assert.Equal("20", answer.PrintedWidth);
            Assert.Equal("330.65849900000001", answer.PrintedLength);
            Assert.Equal(string.Empty, answer.Why);
        }

        /// <summary>
        /// **The UID is matched whole.** Two plots can share an opening, so a prefix match would
        /// hand one plot another's length.
        /// </summary>
        [Fact]
        public void TheUidIsMatchedWholeAndNeverAsAnOpening()
        {
            StreetReferenceFile file = Read();

            Assert.True(file.For("anh-007-st-100210").Found);
            Assert.False(file.For("ANH-007-ST-1002").Found);
            Assert.False(file.For("ANH-007-ST-100210-A").Found);
        }

        /// <summary>
        /// A plot the file does not name is a NOTE with its UID, never a refusal. Both cells
        /// stay empty and the workbook is still written.
        /// </summary>
        [Fact]
        public void APlotTheFileDoesNotNameIsNamedAndBothCellsStayEmpty()
        {
            StreetReferenceAnswer answer = Read().For("ANH-007-ST-999999");

            Assert.False(answer.Found);
            Assert.Equal(
                "ANH-007-ST-999999 is not in the street reference file, so the road width and "
                + "the total length are left empty",
                answer.Why);
        }

        /// <summary>
        /// Two rows for one UID names both. Nothing picks the first, even where the two agree,
        /// because nothing in the file says which is meant.
        /// </summary>
        [Fact]
        public void TwoRowsForOneUidNameBothRatherThanPickingOne()
        {
            StreetReferenceAnswer answer = Read().For("ANH-013-ST-100420");

            Assert.False(answer.Found);
            Assert.Equal(
                "ANH-013-ST-100420 is on 2 rows of the street reference file, row 5 and row 6, "
                + "and nothing says which is meant",
                answer.Why);
        }

        /// <summary>
        /// **A row in sqm is an area and is not used.** 2,051 of the real file's rows are areas,
        /// and one written into the total length cell would have the workbook compute an area of
        /// an area.
        /// </summary>
        [Fact]
        public void ARowWhoseUnitIsNotMetresIsNotUsedAndIsNamed()
        {
            StreetReferenceAnswer answer = Read().For("ANH-007-MO-100016");

            Assert.False(answer.Found);
            Assert.Equal(
                "ANH-007-MO-100016 is on row 4 with QUANTITY UNIT reading sqm rather than m, "
                + "so the row is not used",
                answer.Why);
        }

        /// <summary>
        /// **The file writes an absent value as text rather than leaving the cell empty**, and
        /// that text must never reach a client workbook as a number or as itself.
        /// </summary>
        [Fact]
        public void TheFilesOwnWordForAnAbsentValueIsNoNumber()
        {
            string path = StreetReferenceFixture.Create(
                _folder,
                new[]
                {
                    new StreetReferenceFixture.Row("ANH-007-ST-100001", "412.5", "m", "&lt;Null&gt;")
                },
                "nulls.xlsx");

            StreetReferenceAnswer answer = StreetReferenceFile.In(path).For("ANH-007-ST-100001");

            Assert.False(answer.Found);
            Assert.Equal(
                "ANH-007-ST-100001 is on row 2 and ROAD_WIDTH reads <Null>, which is no number, "
                + "so the row is not used",
                answer.Why);
        }

        /// <summary>
        /// No file set is a note and the run goes through, the same way the link note works.
        /// </summary>
        [Fact]
        public void NoFileSetIsANoteAndNotARefusal()
        {
            Assert.False(StreetReferenceFile.NotSet.Read);
            Assert.Equal(
                "no street reference file is set, so the road width and the total length are "
                + "left empty and the run goes through",
                StreetReferenceFile.NotSet.For("ANH-007-ST-100210").Why);

            Assert.False(StreetReferenceFile.In(null).Read);
            Assert.False(StreetReferenceFile.In("   ").Read);
        }

        /// <summary>
        /// A file whose header row names no ID_UID column is refused with that column named, and
        /// nothing falls back to reading column D because column D is where it sat last time.
        /// </summary>
        [Fact]
        public void AFileThatNamesNoUidColumnSaysSoAndReadsNoPosition()
        {
            string path = StreetReferenceFixture.Create(
                _folder, Measured, "renamed.xlsx", "PLOT REFERENCE");

            StreetReferenceFile file = StreetReferenceFile.In(path);

            Assert.False(file.Read);
            Assert.Equal(
                "row 1 of Sheet1 names no ID_UID * column, and nothing here reads a column by "
                + "its position",
                file.Why);
            Assert.Equal(
                "STREET REFERENCE FILE: not read. row 1 of Sheet1 names no ID_UID * column, and "
                + "nothing here reads a column by its position",
                file.InWords);
        }

        /// <summary>
        /// A file that is not there is a note with the reason, and the run goes through.
        /// </summary>
        [Fact]
        public void AFileThatCannotBeOpenedIsANoteWithTheReason()
        {
            StreetReferenceFile file = StreetReferenceFile.In(
                Path.Combine(_folder, "nothing-here.xlsx"));

            Assert.False(file.Read);
            Assert.Contains("could not be opened", file.Why);
        }

        /// <summary>
        /// The one line the report opens the street section with, so a run of 78 plots with no
        /// widths says why once rather than 78 times.
        /// </summary>
        [Fact]
        public void TheFileSaysWhatItReadInOneLine()
        {
            string path = StreetReferenceFixture.Create(_folder, Measured, "said.xlsx");
            StreetReferenceFile file = StreetReferenceFile.In(path);

            Assert.Equal(
                "STREET REFERENCE FILE: " + path + ", sheet Sheet1, columns named in row 1, "
                + "5 rows carrying a ID_UID *.",
                file.InWords);
        }

        /// <summary>
        /// A row with no UID at all is passed over rather than held as a row naming nothing.
        /// </summary>
        [Fact]
        public void ARowWithNoUidIsPassedOver()
        {
            string path = StreetReferenceFixture.Create(
                _folder,
                new[]
                {
                    new StreetReferenceFixture.Row(null, "412.5", "m", "20"),
                    new StreetReferenceFixture.Row("ANH-007-ST-100002", "500", "m", "15")
                },
                "gappy.xlsx");

            StreetReferenceFile file = StreetReferenceFile.In(path);

            Assert.Single(file.Rows);
            Assert.Equal("ANH-007-ST-100002", file.Rows[0].Uid);
            Assert.Equal(3, file.Rows[0].Row);
        }

        /// <summary>
        /// A plot with no UID2 of its own is named rather than looked up as an empty string,
        /// which would match every row the file holds with an empty cell.
        /// </summary>
        [Fact]
        public void APlotWithNoUidOfItsOwnIsNamed()
        {
            Assert.Equal(
                "this plot carries no PRX_Plot_UID2, so nothing can be looked up",
                Read().For("  ").Why);
        }
    }
}
