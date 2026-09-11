using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// Builds a small workbook zip by hand, because no client workbook may enter this public
    /// repository. Two sheets, a formula with a cached value, a named range, an image part
    /// and a custom part, so the copy and patch can be proven to keep what it does not touch.
    /// </summary>
    internal static class WorkbookFixture
    {
        public const string MainSheet = "Main";

        public const string TreesSheet = "Trees";

        public static readonly byte[] ImageBytes = { 137, 80, 78, 71, 13, 10, 26, 10, 42, 7, 99 };

        public static string Folder()
        {
            string folder = Path.Combine(Path.GetTempPath(), "rcrc-kpi-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        public static string Create(string folder, string fileName = "template.xlsx", bool withCalcPr = false)
        {
            string path = Path.Combine(folder, fileName);

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Default Extension=\"png\" ContentType=\"image/png\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/calcChain.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.calcChain+xml\"/>"
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
                    + "<sheets>"
                    + "<sheet name=\"" + MainSheet + "\" sheetId=\"1\" r:id=\"rId1\"/>"
                    + "<sheet name=\"" + TreesSheet + "\" sheetId=\"2\" r:id=\"rId2\"/>"
                    + "</sheets>"
                    + "<definedNames><definedName name=\"TheArea\">Main!$D$8</definedName></definedNames>"
                    + (withCalcPr ? "<calcPr calcId=\"191029\"/>" : string.Empty)
                    + "</workbook>");

                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet2.xml\"/>"
                    + "</Relationships>");

                // D3 already exists with a style and a stale value, so a patch has to keep the
                // style and replace the content. E9 is a formula with a cached result that no
                // write names, so it has to come through untouched.
                Add(zip, "xl/worksheets/sheet1.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<sheetData>"
                    + "<row r=\"3\"><c r=\"A3\"><v>1</v></c><c r=\"D3\" s=\"2\" t=\"inlineStr\"><is><t>stale</t></is></c></row>"
                    + "<row r=\"9\"><c r=\"E9\"><f>SUM(D4:D8)</f><v>0</v></c></row>"
                    + "</sheetData>"
                    + "</worksheet>");

                // Rows 4 and 5 are named and 6 to 8 are empty, and B9 sums B4 to B8, so the
                // empty rows an unmatched species can be written into are read off the file the
                // way the real MOSQUES list is: its map range stops at 83 and its total sums to
                // 92, so the map cannot be what says where the empties are.
                Add(zip, "xl/worksheets/sheet2.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<sheetData>"
                    + "<row r=\"4\"><c r=\"D4\" t=\"inlineStr\"><is><t>Acacia tortilis</t></is></c></row>"
                    + "<row r=\"5\"><c r=\"D5\" t=\"inlineStr\"><is><t>Ziziphus spina-christi</t></is></c></row>"
                    + "<row r=\"9\"><c r=\"B9\"><f>SUM(B4:B8)</f><v>0</v></c></row>"
                    + "</sheetData>"
                    + "</worksheet>");

                // Excel's record of what order to work the formulas out in, written against the
                // cached results. The patch removes it, so it has to be here to be removed.
                Add(zip, "xl/calcChain.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<calcChain xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<c r=\"E9\" i=\"1\"/><c r=\"B9\" i=\"2\"/>"
                    + "</calcChain>");

                Add(zip, "docProps/custom.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/custom-properties\"/>");

                ZipArchiveEntry image = zip.CreateEntry("xl/media/image1.png");
                using (Stream writing = image.Open())
                {
                    writing.Write(ImageBytes, 0, ImageBytes.Length);
                }
            }

            return path;
        }

        /// <summary>
        /// One tree list sheet as a test describes it: which rows name what, and where the
        /// quantity total is and what it reaches. A total row of zero means no total on the
        /// sheet at all.
        /// </summary>
        public sealed class TreeSheetShape
        {
            public TreeSheetShape(string sheetName, IDictionary<int, string> namesByRow, int totalFirst, int totalLast, int totalRow)
            {
                SheetName = sheetName;
                NamesByRow = namesByRow;
                TotalFirst = totalFirst;
                TotalLast = totalLast;
                TotalRow = totalRow;
            }

            public string SheetName { get; }

            public IDictionary<int, string> NamesByRow { get; }

            public int TotalFirst { get; }

            public int TotalLast { get; }

            public int TotalRow { get; }
        }

        /// <summary>
        /// A workbook with a main sheet and the two tree list sheets, each shaped as the test
        /// says. The header row 3 carries a heading in column D so a reader starting at the
        /// header rather than under it goes red. No client name and no client row is in here.
        /// </summary>
        public static string TreeLists(string folder, TreeSheetShape existing, TreeSheetShape proposed, string fileName = "MOSQUES.xlsx")
        {
            string path = Path.Combine(folder, fileName);

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet3.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
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
                    + "<sheets>"
                    + "<sheet name=\"&lt;Mosques&gt;\" sheetId=\"1\" r:id=\"rId1\"/>"
                    + "<sheet name=\"" + existing.SheetName + "\" sheetId=\"2\" r:id=\"rId2\"/>"
                    + "<sheet name=\"" + proposed.SheetName + "\" sheetId=\"3\" r:id=\"rId3\"/>"
                    + "</sheets>"
                    + "</workbook>");

                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet2.xml\"/>"
                    + "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet3.xml\"/>"
                    + "</Relationships>");

                Add(zip, "xl/worksheets/sheet1.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData/></worksheet>");

                Add(zip, "xl/worksheets/sheet2.xml", TreeSheetXml(existing));
                Add(zip, "xl/worksheets/sheet3.xml", TreeSheetXml(proposed));
            }

            return path;
        }

        private static string TreeSheetXml(TreeSheetShape shape)
        {
            var rows = new SortedDictionary<int, string>();
            rows[3] = "<c r=\"D3\" t=\"inlineStr\"><is><t>BOTANICAL NAME</t></is></c>";

            foreach (KeyValuePair<int, string> named in shape.NamesByRow)
            {
                string cell = "<c r=\"D" + named.Key + "\" t=\"inlineStr\"><is><t>" + named.Value + "</t></is></c>";
                rows[named.Key] = rows.ContainsKey(named.Key) ? rows[named.Key] + cell : cell;
            }

            if (shape.TotalRow > 0)
            {
                string total = "<c r=\"B" + shape.TotalRow + "\"><f>SUM(B" + shape.TotalFirst + ":B" + shape.TotalLast + ")</f><v>0</v></c>";
                rows[shape.TotalRow] = rows.ContainsKey(shape.TotalRow) ? total + rows[shape.TotalRow] : total;
            }

            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            foreach (KeyValuePair<int, string> row in rows)
            {
                xml.Append("<row r=\"" + row.Key + "\">" + row.Value + "</row>");
            }

            xml.Append("</sheetData></worksheet>");
            return xml.ToString();
        }

        /// <summary>
        /// One row of a tree list sheet in the computing workbook: the name, and what the
        /// sheet's height and diameter columns already hold, empty for an empty row.
        /// </summary>
        public sealed class TreeRow
        {
            public TreeRow(int row, string name, string height = "", string diameter = "")
            {
                Row = row;
                Name = name;
                Height = height;
                Diameter = diameter;
            }

            public int Row { get; }

            public string Name { get; }

            public string Height { get; }

            public string Diameter { get; }
        }

        /// <summary>
        /// A workbook shaped like the MOSQUES one the 1428 run wrote, in miniature, so the
        /// formula check can be proven on the shape that broke: a tree sheet whose canopy
        /// column reads IF(ISBLANK(J), " ", ROUND(PI()*(J/2)^2, 0)) and whose canopy area
        /// column multiplies that by the count, a canopy total summing them, and a main sheet
        /// computing from that total, from the mapped cells and from a defined name, with two
        /// _xlfn.IFS formulas at the KPI row. No client name and no client value is in here.
        ///
        /// Both tree sheets run rows 4 to 9 with the totals on row 10, the header on row 3 with
        /// the height and diameter headings in I and J, and the canopy formulas shared from
        /// row 4 the way Excel stores a column of one formula.
        /// </summary>
        public static string Computing(
            string folder,
            TreeRow[] existing,
            TreeRow[] proposed,
            string fileName = "MOSQUES.xlsx",
            string heightHeading = "Mature Height (m)",
            string diameterHeading = "Average Mature Canopy Diameter (m)",
            string heightColumn = "I",
            string diameterColumn = "J",
            bool withSheetCalcPr = false,
            string secondDiameterHeading = null,
            string secondDiameterColumn = "K",
            string canopyReads = null,
            string alsoReads = null)
        {
            string path = Path.Combine(folder, fileName);

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet3.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/calcChain.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.calcChain+xml\"/>"
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
                    + "<sheets>"
                    + "<sheet name=\"&lt;Mosques&gt;\" sheetId=\"1\" r:id=\"rId1\"/>"
                    + "<sheet name=\"Tree List - Existing\" sheetId=\"2\" r:id=\"rId2\"/>"
                    + "<sheet name=\"Tree List - Proposed\" sheetId=\"3\" r:id=\"rId3\"/>"
                    + "</sheets>"
                    + "<definedNames><definedName name=\"Area\">'&lt;Mosques&gt;'!$H$7</definedName></definedNames>"
                    + "<calcPr calcId=\"191029\"/>"
                    + "</workbook>");

                Add(zip, "xl/calcChain.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<calcChain xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<c r=\"D8\" i=\"1\"/><c r=\"M10\" i=\"3\"/>"
                    + "</calcChain>");

                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet2.xml\"/>"
                    + "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet3.xml\"/>"
                    + "</Relationships>");

                // The main sheet: the six mapped cells, three of them holding the template's own
                // placeholders, and the formulas that compute from them. F8 is the canopy area
                // off both tree sheets, D8 the total green cover, D9 and H9 divide by the area,
                // E31 reads D8, and F31 and G31 are the KPI row's IFS formulas.
                Add(zip, "xl/worksheets/sheet1.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<sheetData>"
                    + "<row r=\"3\"><c r=\"D3\" t=\"inlineStr\"><is><t>&lt;Component&gt;</t></is></c></row>"
                    + "<row r=\"7\"><c r=\"H7\"><v>0</v></c></row>"
                    + "<row r=\"8\"><c r=\"D8\"><f>F8+F10+H10</f><v>0</v></c><c r=\"F8\"><f>'Tree List - Existing'!M10+'Tree List - Proposed'!M10</f><v>0</v></c></row>"
                    + "<row r=\"9\"><c r=\"D9\"><f>D8/H7</f><v>0</v></c><c r=\"H9\"><f>H8/Area</f><v>0</v></c></row>"
                    + "<row r=\"10\"><c r=\"F10\"><v>0</v></c><c r=\"H10\"><v>0</v></c></row>"
                    + "<row r=\"31\"><c r=\"E31\"><f>D8</f><v>0</v></c>"
                    + "<c r=\"F31\" t=\"str\"><f>_xlfn.IFS(Area&lt;1,\" \",D9&lt;1,\" \",E31&lt;H31-(H31*7%),\"Insufficient\",TRUE,\"YES\")</f><v> </v></c>"
                    + "<c r=\"G31\" t=\"str\"><f>_xlfn.IFS(F31=\"YES\",\"COMPLIANT\",TRUE,\"NOT COMPLIANT\")</f><v> </v></c>"
                    + "<c r=\"H31\"><v>13</v></c></row>"
                    + "</sheetData>"
                    + (withSheetCalcPr ? "<sheetCalcPr fullCalcOnLoad=\"1\"/>" : string.Empty)
                    + "</worksheet>");

                Add(zip, "xl/worksheets/sheet2.xml", TreeSheetComputing(existing, heightHeading, diameterHeading, heightColumn, diameterColumn,
                    secondDiameterHeading, secondDiameterColumn, canopyReads ?? diameterColumn, alsoReads));
                Add(zip, "xl/worksheets/sheet3.xml", TreeSheetComputing(proposed, heightHeading, diameterHeading, heightColumn, diameterColumn,
                    secondDiameterHeading, secondDiameterColumn, canopyReads ?? diameterColumn, alsoReads));
            }

            return path;
        }

        /// <summary>
        /// A second heading holding DIAMETER, the shape of the MOSQUES sheet where J is Average
        /// Mature Canopy Diameter (m) and K is Mature Canopy Diameter (m), goes into
        /// <paramref name="secondDiameterColumn"/> when a heading is given. The canopy formula
        /// in L reads <paramref name="canopyReads"/>, which is the diameter column unless a test
        /// points it elsewhere, and <paramref name="alsoReads"/> adds a formula column N reading
        /// that column, so a sheet whose formulas read both candidates can be built.
        /// </summary>
        private static string TreeSheetComputing(
            TreeRow[] named, string heightHeading, string diameterHeading, string heightColumn, string diameterColumn,
            string secondDiameterHeading = null, string secondDiameterColumn = "K", string canopyReads = null, string alsoReads = null)
        {
            var byRow = new Dictionary<int, TreeRow>();
            foreach (TreeRow one in named ?? new TreeRow[0]) byRow[one.Row] = one;

            var xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            xml.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            xml.Append("<row r=\"3\">"
                + "<c r=\"B3\" t=\"inlineStr\"><is><t>Quantity</t></is></c>"
                + "<c r=\"D3\" t=\"inlineStr\"><is><t>Botanical Name</t></is></c>"
                + "<c r=\"" + heightColumn + "3\" t=\"inlineStr\"><is><t>" + heightHeading + "</t></is></c>"
                + "<c r=\"" + diameterColumn + "3\" t=\"inlineStr\"><is><t>" + diameterHeading + "</t></is></c>"
                + (secondDiameterHeading == null ? string.Empty
                    : "<c r=\"" + secondDiameterColumn + "3\" t=\"inlineStr\"><is><t>" + secondDiameterHeading + "</t></is></c>")
                + "<c r=\"L3\" t=\"inlineStr\"><is><t>Canopy per tree</t></is></c>"
                + "<c r=\"M3\" t=\"inlineStr\"><is><t>Canopy area</t></is></c>"
                + (alsoReads == null ? string.Empty : "<c r=\"N3\" t=\"inlineStr\"><is><t>Spread check</t></is></c>")
                + "</row>");
            string reads = canopyReads ?? diameterColumn;

            for (int row = 4; row <= 9; row++)
            {
                var cells = new StringBuilder();
                TreeRow one;
                if (byRow.TryGetValue(row, out one))
                {
                    cells.Append("<c r=\"D" + row + "\" t=\"inlineStr\"><is><t>" + one.Name + "</t></is></c>");
                    if (one.Height.Length > 0) cells.Append("<c r=\"" + heightColumn + row + "\"><v>" + one.Height + "</v></c>");
                    if (one.Diameter.Length > 0) cells.Append("<c r=\"" + diameterColumn + row + "\"><v>" + one.Diameter + "</v></c>");
                }

                // Shared from row 4, the way Excel stores one formula filled down a column: the
                // master carries the text and the ref, the rest carry the index alone.
                string canopy = row == 4
                    ? "<c r=\"L4\" t=\"str\"><f t=\"shared\" ref=\"L4:L9\" si=\"0\">IF(ISBLANK(" + reads + "4),\" \",ROUND(PI()*(" + reads + "4/2)^2,0))</f><v> </v></c>"
                    : "<c r=\"L" + row + "\" t=\"str\"><f t=\"shared\" si=\"0\"/><v> </v></c>";
                string area = row == 4
                    ? "<c r=\"M4\" t=\"str\"><f t=\"shared\" ref=\"M4:M9\" si=\"1\">IF(ISBLANK(B4),\" \",L4*B4)</f><v> </v></c>"
                    : "<c r=\"M" + row + "\" t=\"str\"><f t=\"shared\" si=\"1\"/><v> </v></c>";
                string spread = alsoReads == null ? string.Empty
                    : row == 4
                        ? "<c r=\"N4\" t=\"str\"><f t=\"shared\" ref=\"N4:N9\" si=\"2\">IF(ISBLANK(" + alsoReads + "4),\" \"," + alsoReads + "4)</f><v> </v></c>"
                        : "<c r=\"N" + row + "\" t=\"str\"><f t=\"shared\" si=\"2\"/><v> </v></c>";

                xml.Append("<row r=\"" + row + "\">" + cells + canopy + area + spread + "</row>");
            }

            xml.Append("<row r=\"10\"><c r=\"B10\"><f>SUM(B4:B9)</f><v>0</v></c><c r=\"M10\"><f>SUM(M4:M9)</f><v>0</v></c></row>");
            xml.Append("</sheetData></worksheet>");
            return xml.ToString();
        }

        /// <summary>
        /// A two sheet workbook whose main sheet holds one cell as a shared string, which is
        /// how an Excel re-save stores a filled text cell, so the peek is proven to resolve the
        /// index to its text rather than read the index.
        /// </summary>
        public static string WithSharedStringAt(string folder, string fileName, string cell, string text)
        {
            string path = Path.Combine(folder, fileName);

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Add(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>"
                    + "</Types>");
                Add(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
                    + "</Relationships>");
                Add(zip, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">"
                    + "<sheets><sheet name=\"" + MainSheet + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
                    + "</workbook>");
                Add(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>"
                    + "</Relationships>");
                Add(zip, "xl/sharedStrings.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" count=\"1\" uniqueCount=\"1\"><si><t>" + text + "</t></si></sst>");
                Add(zip, "xl/worksheets/sheet1.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>"
                    + "<row r=\"5\"><c r=\"" + cell + "\" t=\"s\"><v>0</v></c></row>"
                    + "</sheetData></worksheet>");
            }

            return path;
        }

        public static byte[] PartBytes(string path, string partPath)
        {
            using (FileStream file = File.OpenRead(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry = zip.GetEntry(partPath);
                if (entry == null) return null;

                using (Stream reading = entry.Open())
                using (var held = new MemoryStream())
                {
                    reading.CopyTo(held);
                    return held.ToArray();
                }
            }
        }

        public static string PartText(string path, string partPath)
        {
            byte[] bytes = PartBytes(path, partPath);
            return bytes == null ? null : Encoding.UTF8.GetString(bytes);
        }

        public static int PartCount(string path)
        {
            using (FileStream file = File.OpenRead(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                return zip.Entries.Count;
            }
        }

        private static void Add(ZipArchive zip, string partPath, string xml)
        {
            ZipArchiveEntry entry = zip.CreateEntry(partPath);
            using (var writing = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writing.Write(xml);
            }
        }
    }
}
