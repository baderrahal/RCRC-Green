using System;
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

                Add(zip, "xl/worksheets/sheet2.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
                    + "<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
                    + "<sheetData>"
                    + "<row r=\"4\"><c r=\"D4\" t=\"inlineStr\"><is><t>Acacia tortilis</t></is></c></row>"
                    + "<row r=\"5\"><c r=\"D5\" t=\"inlineStr\"><is><t>Ziziphus spina-christi</t></is></c></row>"
                    + "</sheetData>"
                    + "</worksheet>");

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
