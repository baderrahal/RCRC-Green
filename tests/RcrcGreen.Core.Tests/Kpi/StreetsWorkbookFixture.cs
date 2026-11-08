using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RcrcGreen.Core.Tests.Kpi
{
    /// <summary>
    /// A workbook shaped like the STREETS template where H8 matters: the main sheet under its
    /// real name, D8, F8 and H8 on row 8, a canopy percentage dividing by H8 and a calculation
    /// chain. Made up numbers, written the way the file writes them.
    /// </summary>
    internal static class StreetsWorkbookFixture
    {
        public const string MainSheet = "&lt;Streets&gt;";

        public const string MainSheetPart = "xl/worksheets/sheet1.xml";

        private const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        private const string Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        public static string Create(string folder, string name)
        {
            string path = Path.Combine(folder, name);

            using (FileStream writing = File.Create(path))
            using (var zip = new ZipArchive(writing, ZipArchiveMode.Create))
            {
                Part(zip, "[Content_Types].xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                    + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                    + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
                    + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
                    + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
                    + "<Override PartName=\"/xl/calcChain.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.calcChain+xml\"/>"
                    + "</Types>");

                Part(zip, "_rels/.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"" + Rel + "/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");

                Part(zip, "xl/_rels/workbook.xml.rels",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                    + "<Relationship Id=\"rId1\" Type=\"" + Rel + "/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
                    + "<Relationship Id=\"rId2\" Type=\"" + Rel + "/calcChain\" Target=\"calcChain.xml\"/></Relationships>");

                Part(zip, "xl/workbook.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"" + Ns + "\" xmlns:r=\"" + Rel + "\">"
                    + "<sheets><sheet name=\"" + MainSheet + "\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
                    + "<calcPr calcId=\"191029\"/></workbook>");

                Part(zip, "xl/calcChain.xml",
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><calcChain xmlns=\"" + Ns + "\"><c r=\"J8\" i=\"1\"/></calcChain>");

                Part(zip, MainSheetPart,
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"" + Ns + "\"><sheetData>"
                    + "<row r=\"3\"><c r=\"D3\" s=\"4\"/></row>"
                    + "<row r=\"4\"><c r=\"E4\" s=\"4\"/></row>"
                    + "<row r=\"8\"><c r=\"D8\" s=\"7\"/><c r=\"F8\" s=\"7\"/><c r=\"H8\" s=\"9\"/><c r=\"J8\" s=\"2\"><f>D9/H8</f><v>0</v></c></row>"
                    + "<row r=\"9\"><c r=\"D9\" s=\"7\"><v>500</v></c></row>"
                    + "<row r=\"11\"><c r=\"F11\" s=\"7\"/><c r=\"H11\" s=\"7\"/></row>"
                    + "</sheetData></worksheet>");
            }

            return path;
        }

        /// <summary>
        /// Every part's SHA-256, its line endings made one kind first, because the gate saves
        /// the XML on Linux and this machine saves it on Windows and a line ending is the only
        /// thing that differs. Zip timestamps are left out, because they are not the workbook.
        /// </summary>
        public static IDictionary<string, string> PartHashes(string path)
        {
            var hashes = new SortedDictionary<string, string>(StringComparer.Ordinal);

            using (FileStream reading = File.OpenRead(path))
            using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    using (Stream part = entry.Open())
                    using (var buffer = new MemoryStream())
                    {
                        part.CopyTo(buffer);
                        string text = Encoding.UTF8.GetString(buffer.ToArray()).Replace("\r\n", "\n");

                        using (SHA256 sha = SHA256.Create())
                        {
                            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                            hashes[entry.FullName] = string.Concat(hash.Select(one => one.ToString("x2")));
                        }
                    }
                }
            }

            return hashes;
        }

        private static void Part(ZipArchive zip, string name, string text)
        {
            ZipArchiveEntry entry = zip.CreateEntry(name);
            using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                writer.Write(text);
            }
        }
    }
}
