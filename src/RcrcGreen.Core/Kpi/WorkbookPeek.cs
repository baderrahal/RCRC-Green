using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The sheet names of one .xlsx, in order, read straight out of the zip. The first name
    /// is how a template is recognised, so this reads and nothing more.
    ///
    /// Parsing compares local names rather than namespaces, because a strict and a
    /// transitional workbook carry different ones and both are real files.
    /// </summary>
    public sealed class PeekedWorkbook
    {
        private PeekedWorkbook(IReadOnlyList<string> sheetNames, string refusal)
        {
            SheetNames = sheetNames ?? new List<string>();
            Refusal = refusal ?? string.Empty;
        }

        public IReadOnlyList<string> SheetNames { get; }

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
                        return new PeekedWorkbook(null, "It is a zip and not a workbook. No workbook part is in it.");
                    }

                    XDocument workbook = WorkbookPackage.Read(zip, workbookPart);
                    if (workbook == null)
                    {
                        return new PeekedWorkbook(null, "Its workbook part could not be read.");
                    }

                    List<string> names = workbook.Root
                        .Elements().Where(element => element.Name.LocalName == "sheets")
                        .Elements().Where(element => element.Name.LocalName == "sheet")
                        .Select(sheet => (string)sheet.Attribute("name") ?? string.Empty)
                        .ToList();

                    return new PeekedWorkbook(names, null);
                }
            }
            catch (InvalidDataException)
            {
                return new PeekedWorkbook(null, "It is not a zip, so it is not an .xlsx however it is named.");
            }
            catch (IOException failed)
            {
                return new PeekedWorkbook(null, "It could not be opened. " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return new PeekedWorkbook(null, "It could not be opened. " + failed.Message);
            }
            catch (System.Xml.XmlException failed)
            {
                return new PeekedWorkbook(null, "Its workbook part is not readable XML. " + failed.Message);
            }
        }
    }

    /// <summary>
    /// The package plumbing the peek and the patcher share: finding the workbook part and
    /// the part behind each sheet, by the relationships rather than by assuming a path.
    /// </summary>
    internal static class WorkbookPackage
    {
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
