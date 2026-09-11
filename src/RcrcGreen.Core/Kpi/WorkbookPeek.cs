using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The sheet names of one .xlsx, in order, read straight out of the zip, and the few cells
    /// on the first sheet the tool itself writes. The first name is how a template is
    /// recognised, and those cells are how a filled checklist is told from one, so this reads
    /// those two things and nothing more.
    ///
    /// Parsing compares local names rather than namespaces, because a strict and a
    /// transitional workbook carry different ones and both are real files.
    /// </summary>
    public sealed class PeekedWorkbook
    {
        private PeekedWorkbook(IReadOnlyList<string> sheetNames, string refusal, IReadOnlyDictionary<string, string> firstSheetCells)
        {
            SheetNames = sheetNames ?? new List<string>();
            Refusal = refusal ?? string.Empty;
            FirstSheetCells = firstSheetCells ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyList<string> SheetNames { get; }

        /// <summary>
        /// What the first sheet holds in each cell <see cref="FilledMarks"/> reads, with a
        /// shared string resolved to its text. A cell that is not in the file is not in here,
        /// and a file with no cell of them in it is not a filled file.
        /// </summary>
        public IReadOnlyDictionary<string, string> FirstSheetCells { get; }

        /// <summary>
        /// Why the file could not be read, or empty. A refusal is shown beside the file name
        /// rather than swallowed, because a file that cannot be read is not offered.
        /// </summary>
        public string Refusal { get; }

        public bool WasRead
        {
            get { return Refusal.Length == 0; }
        }

        public static PeekedWorkbook Of(string path)
        {
            if (path == null) throw new ArgumentNullException("path");

            try
            {
                using (FileStream file = File.OpenRead(path))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {
                    string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null)
                    {
                        return new PeekedWorkbook(null, "It is a zip and not a workbook. No workbook part is in it.", null);
                    }

                    XDocument workbook = WorkbookPackage.Read(zip, workbookPart);
                    if (workbook == null)
                    {
                        return new PeekedWorkbook(null, "Its workbook part could not be read.", null);
                    }

                    List<string> names = workbook.Root
                        .Elements().Where(element => element.Name.LocalName == "sheets")
                        .Elements().Where(element => element.Name.LocalName == "sheet")
                        .Select(sheet => (string)sheet.Attribute("name") ?? string.Empty)
                        .ToList();

                    return new PeekedWorkbook(names, null, CellsOf(zip, workbookPart, names));
                }
            }
            catch (InvalidDataException)
            {
                return new PeekedWorkbook(null, "It is not a zip, so it is not an .xlsx however it is named.", null);
            }
            catch (IOException failed)
            {
                return new PeekedWorkbook(null, "It could not be opened. " + failed.Message, null);
            }
            catch (UnauthorizedAccessException failed)
            {
                return new PeekedWorkbook(null, "It could not be opened. " + failed.Message, null);
            }
            catch (System.Xml.XmlException failed)
            {
                return new PeekedWorkbook(null, "Its workbook part is not readable XML. " + failed.Message, null);
            }
        }

        /// <summary>
        /// The cells of the first sheet the marks read, the ones the tool writes. Read inside
        /// the same open as the names, off the sheet part the workbook's relationships point
        /// at, and in one pass over the shared strings rather than one per cell.
        /// </summary>
        private static IReadOnlyDictionary<string, string> CellsOf(ZipArchive zip, string workbookPart, List<string> names)
        {
            var held = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (names.Count == 0) return held;

            Dictionary<string, string> parts = WorkbookPackage.SheetParts(zip, workbookPart);
            string partPath;
            if (!parts.TryGetValue(names[0], out partPath)) return held;

            return WorkbookPackage.CellTexts(
                zip, partPath, FilledMarks.CellsRead, WorkbookPackage.SharedStrings(zip));
        }
    }

    /// <summary>
    /// The package plumbing the peek, the patcher and the species list share: finding the
    /// workbook part and the part behind each sheet, by the relationships rather than by
    /// assuming a path, and reading one cell's text with a shared string resolved.
    /// </summary>
    internal static class WorkbookPackage
    {
        /// <summary>
        /// Every cell of a sheet part, in document order.
        /// </summary>
        public static IEnumerable<XElement> Cells(XDocument sheet)
        {
            return sheet.Root.Elements()
                .Where(element => element.Name.LocalName == "sheetData")
                .Elements().Where(element => element.Name.LocalName == "row")
                .Elements().Where(element => element.Name.LocalName == "c");
        }

        /// <summary>
        /// The text of one cell as a person reads it: an inline string, a shared string
        /// resolved through the table, or the raw value. Empty for a cell holding nothing.
        /// </summary>
        public static string TextOf(XElement cell, List<string> shared)
        {
            XElement inline = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "is");
            if (inline != null)
            {
                return string.Concat(inline.Descendants()
                    .Where(child => child.Name.LocalName == "t").Select(child => child.Value));
            }

            XElement value = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "v");
            if (value == null) return string.Empty;

            if (!string.Equals((string)cell.Attribute("t"), "s", StringComparison.Ordinal))
            {
                return value.Value;
            }

            int index;
            if (!int.TryParse(value.Value, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out index))
            {
                return string.Empty;
            }

            return shared != null && index >= 0 && index < shared.Count ? shared[index] : string.Empty;
        }

        public static List<string> SharedStrings(ZipArchive zip)
        {
            var held = new List<string>();

            XDocument table = Read(zip, "xl/sharedStrings.xml");
            if (table == null || table.Root == null) return held;

            foreach (XElement item in table.Root.Elements().Where(one => one.Name.LocalName == "si"))
            {
                held.Add(string.Concat(item.Descendants()
                    .Where(child => child.Name.LocalName == "t").Select(child => child.Value)));
            }

            return held;
        }

        /// <summary>
        /// The text of each wanted cell off one sheet part, with a cell that is not there left
        /// out. The part is parsed once however many cells are wanted, because reading it per
        /// cell reopens the entry and parses the whole sheet again, and every mark added to
        /// <see cref="FilledMarks"/> would have cost another pass.
        /// </summary>
        public static IReadOnlyDictionary<string, string> CellTexts(
            ZipArchive zip, string partPath, IEnumerable<string> cellRefs, List<string> shared)
        {
            var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var wanted = new HashSet<string>(
                (cellRefs ?? Enumerable.Empty<string>()).Where(one => !string.IsNullOrWhiteSpace(one)),
                StringComparer.OrdinalIgnoreCase);
            if (wanted.Count == 0) return found;

            XDocument part = Read(zip, partPath);
            if (part == null || part.Root == null) return found;

            foreach (XElement cell in Cells(part))
            {
                string where = (string)cell.Attribute("r");
                if (where == null || !wanted.Contains(where) || found.ContainsKey(where)) continue;

                found[where] = TextOf(cell, shared);
                if (found.Count == wanted.Count) break;
            }

            return found;
        }

        public static string WorkbookPartPath(ZipArchive zip)
        {
            XDocument rels = Read(zip, "_rels/.rels");
            if (rels == null) return null;

            foreach (XElement relationship in rels.Root.Elements()
                .Where(element => element.Name.LocalName == "Relationship"))
            {
                string type = (string)relationship.Attribute("Type") ?? string.Empty;
                if (!type.EndsWith("/officeDocument", StringComparison.Ordinal)) continue;

                return Resolve(string.Empty, (string)relationship.Attribute("Target"));
            }

            return null;
        }

        /// <summary>
        /// Sheet name to part path, off the workbook part and its relationships.
        /// </summary>
        public static Dictionary<string, string> SheetParts(ZipArchive zip, string workbookPart)
        {
            var parts = new Dictionary<string, string>(StringComparer.Ordinal);

            XDocument workbook = Read(zip, workbookPart);
            if (workbook == null) return parts;

            string folder = FolderOf(workbookPart);
            XDocument rels = Read(zip, RelsPathFor(workbookPart));
            if (rels == null) return parts;

            var targetById = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XElement relationship in rels.Root.Elements()
                .Where(element => element.Name.LocalName == "Relationship"))
            {
                string id = (string)relationship.Attribute("Id");
                string target = (string)relationship.Attribute("Target");
                if (id != null && target != null) targetById[id] = Resolve(folder, target);
            }

            foreach (XElement sheet in workbook.Root
                .Elements().Where(element => element.Name.LocalName == "sheets")
                .Elements().Where(element => element.Name.LocalName == "sheet"))
            {
                string name = (string)sheet.Attribute("name");
                XAttribute id = sheet.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "id");

                string target;
                if (name != null && id != null && targetById.TryGetValue(id.Value, out target)
                    && !parts.ContainsKey(name))
                {
                    parts.Add(name, target);
                }
            }

            return parts;
        }

        public static XDocument Read(ZipArchive zip, string partPath)
        {
            ZipArchiveEntry entry = zip.GetEntry(partPath);
            if (entry == null) return null;

            using (Stream reading = entry.Open())
            {
                return XDocument.Load(reading);
            }
        }

        public static string RelsPathFor(string partPath)
        {
            string folder = FolderOf(partPath);
            string name = partPath.Substring(folder.Length == 0 ? 0 : folder.Length + 1);
            return (folder.Length == 0 ? string.Empty : folder + "/") + "_rels/" + name + ".rels";
        }

        private static string FolderOf(string partPath)
        {
            int at = partPath.LastIndexOf('/');
            return at < 0 ? string.Empty : partPath.Substring(0, at);
        }

        /// <summary>
        /// A relationship target is relative to its part's folder, or absolute from the
        /// package root with a leading slash. Both are real files.
        /// </summary>
        private static string Resolve(string folder, string target)
        {
            if (string.IsNullOrEmpty(target)) return null;
            if (target.StartsWith("/", StringComparison.Ordinal)) return target.Substring(1);

            var kept = new List<string>();
            if (folder.Length > 0) kept.AddRange(folder.Split('/'));

            foreach (string piece in target.Split('/'))
            {
                if (piece == "..")
                {
                    if (kept.Count > 0) kept.RemoveAt(kept.Count - 1);
                    continue;
                }
                if (piece == "." || piece.Length == 0) continue;
                kept.Add(piece);
            }

            return string.Join("/", kept);
        }
    }
}
