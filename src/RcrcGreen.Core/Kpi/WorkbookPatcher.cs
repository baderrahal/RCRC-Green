using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Copies a workbook and patches only the cells that get a value.
    ///
    /// An .xlsx is a zip, and the client's EXISTING PARKS one holds 37 parts: comments,
    /// printer settings, an embedded image, the dynamic array metadata. Loading it into an
    /// object model and saving lost 21 of them while the file still opened, so the copy is
    /// byte for byte and only the sheet parts that receive values and the workbook part are
    /// rewritten. Everything is decided before the copy is made, so a refusal writes nothing.
    /// </summary>
    public static class WorkbookPatcher
    {
        public static PatchOutcome Patch(string sourcePath, string outputPath, IReadOnlyList<CellWrite> writes)
        {
            if (sourcePath == null) throw new ArgumentNullException("sourcePath");
            if (outputPath == null) throw new ArgumentNullException("outputPath");
            if (writes == null) throw new ArgumentNullException("writes");
            if (writes.Count == 0) return PatchOutcome.Refused("Nothing to write.");

            try
            {
                int partsInSource;
                string workbookPart;
                Dictionary<string, string> sheetParts;

                // Decide everything off the source first. A write naming a sheet the workbook
                // does not have refuses the whole patch before any file exists.
                using (FileStream reading = File.OpenRead(sourcePath))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    partsInSource = zip.Entries.Count;
                    workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                    if (workbookPart == null)
                    {
                        return PatchOutcome.Refused("The source is not a workbook. No workbook part is in it.");
                    }

                    sheetParts = WorkbookPackage.SheetParts(zip, workbookPart);
                }

                foreach (CellWrite write in writes)
                {
                    if (!sheetParts.ContainsKey(write.SheetName))
                    {
                        return PatchOutcome.Refused("The workbook has no sheet named " + write.SheetName
                            + ", so nothing was written.");
                    }
                }

                File.Copy(sourcePath, outputPath, true);

                var changed = new List<string>();
                using (FileStream updating = new FileStream(outputPath, FileMode.Open, FileAccess.ReadWrite))
                using (var zip = new ZipArchive(updating, ZipArchiveMode.Update))
                {
                    foreach (IGrouping<string, CellWrite> perSheet in writes.GroupBy(write => write.SheetName, StringComparer.Ordinal))
                    {
                        string partPath = sheetParts[perSheet.Key];
                        PatchSheet(zip, partPath, perSheet.ToList());
                        changed.Add(partPath);
                    }

                    // Every formula carries a stored result, so without this the KPI
                    // percentages show the old blanks beside the new numbers until somebody
                    // presses recalculate.
                    SetRecalculateOnOpen(zip, workbookPart);
                    changed.Add(workbookPart);
                }

                int partsInOutput;
                var landed = new List<LandedCell>();
                using (FileStream reading = File.OpenRead(outputPath))
                using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
                {
                    partsInOutput = zip.Entries.Count;
                    foreach (CellWrite write in writes)
                    {
                        landed.Add(new LandedCell(
                            write.SheetName,
                            write.Cell.ToString(),
                            CellText(zip, sheetParts[write.SheetName], write.Cell)));
                    }
                }

                return PatchOutcome.Done(partsInSource, partsInOutput, changed, landed);
            }
            catch (InvalidDataException)
            {
                return PatchOutcome.Refused("The source is not a zip, so it is not an .xlsx however it is named.");
            }
            catch (IOException failed)
            {
                return PatchOutcome.Refused("The file could not be written. " + failed.Message);
            }
            catch (UnauthorizedAccessException failed)
            {
                return PatchOutcome.Refused("The folder refused the write. " + failed.Message);
            }
            catch (System.Xml.XmlException failed)
            {
                return PatchOutcome.Refused("A part of the workbook is not readable XML. " + failed.Message);
            }
        }

        /// <summary>
        /// What one cell of a written file really holds, for reading a value back. Null when
        /// the cell is not in the file at all.
        /// </summary>
        public static string ReadBack(string path, string sheetName, string cellRef)
        {
            if (path == null) throw new ArgumentNullException("path");
            if (sheetName == null) throw new ArgumentNullException("sheetName");
            CellRef cell = CellRef.Parse(cellRef);

            using (FileStream reading = File.OpenRead(path))
            using (var zip = new ZipArchive(reading, ZipArchiveMode.Read))
            {
                string workbookPart = WorkbookPackage.WorkbookPartPath(zip);
                if (workbookPart == null) return null;

                Dictionary<string, string> sheetParts = WorkbookPackage.SheetParts(zip, workbookPart);
                string partPath;
                if (!sheetParts.TryGetValue(sheetName, out partPath)) return null;

                return CellText(zip, partPath, cell);
            }
        }

        private static void PatchSheet(ZipArchive zip, string partPath, IReadOnlyList<CellWrite> writes)
        {
            ZipArchiveEntry entry = zip.GetEntry(partPath);
            if (entry == null) throw new IOException("The sheet part " + partPath + " is not in the package.");

            XDocument sheet;
            using (Stream reading = entry.Open())
            {
                sheet = XDocument.Load(reading);
            }

            XNamespace ns = sheet.Root.Name.Namespace;
            XElement sheetData = sheet.Root.Elements()
                .FirstOrDefault(element => element.Name.LocalName == "sheetData");
            if (sheetData == null)
            {
                sheetData = new XElement(ns + "sheetData");
                sheet.Root.Add(sheetData);
            }

            foreach (CellWrite write in writes)
            {
                XElement cell = CellIn(RowIn(sheetData, ns, write.Cell.Row), ns, write.Cell);
                cell.Attribute("t")?.Remove();
                cell.Elements().Where(child => child.Name.LocalName == "v"
                    || child.Name.LocalName == "is"
                    || child.Name.LocalName == "f").Remove();

                if (write.IsText)
                {
                    cell.SetAttributeValue("t", "inlineStr");
                    cell.Add(new XElement(ns + "is",
                        new XElement(ns + "t",
                            new XAttribute(XNamespace.Xml + "space", "preserve"),
                            write.Stored)));
                }
                else
                {
                    cell.Add(new XElement(ns + "v", write.Stored));
                }
            }

            Save(entry, sheet);
        }

        /// <summary>
        /// Rows ascending and cells ascending within a row, because a cell written out of
        /// order is a file Excel offers to repair. The style attribute of a cell that already
        /// exists is left on it, so the client's own formatting stays.
        /// </summary>
        private static XElement RowIn(XElement sheetData, XNamespace ns, int rowNumber)
        {
            XElement after = null;
            foreach (XElement row in sheetData.Elements().Where(element => element.Name.LocalName == "row"))
            {
                int number;
                if (!int.TryParse((string)row.Attribute("r"), out number)) continue;
                if (number == rowNumber) return row;
                if (number < rowNumber) after = row;
                if (number > rowNumber) break;
            }

            var made = new XElement(ns + "row", new XAttribute("r", rowNumber));
            if (after == null) sheetData.AddFirst(made);
            else after.AddAfterSelf(made);
            return made;
        }

        private static XElement CellIn(XElement row, XNamespace ns, CellRef reference)
        {
            XElement after = null;
            foreach (XElement cell in row.Elements().Where(element => element.Name.LocalName == "c"))
            {
                CellRef parsed = CellRef.TryParse((string)cell.Attribute("r"));
                if (parsed == null) continue;
                if (parsed.ColumnNumber == reference.ColumnNumber) return cell;
                if (parsed.ColumnNumber < reference.ColumnNumber) after = cell;
                if (parsed.ColumnNumber > reference.ColumnNumber) break;
            }

            var made = new XElement(ns + "c", new XAttribute("r", reference.ToString()));
            if (after == null) row.AddFirst(made);
            else after.AddAfterSelf(made);
            return made;
        }

        private static void SetRecalculateOnOpen(ZipArchive zip, string workbookPart)
        {
            ZipArchiveEntry entry = zip.GetEntry(workbookPart);
            if (entry == null) throw new IOException("The workbook part " + workbookPart + " is not in the package.");

            XDocument workbook;
            using (Stream reading = entry.Open())
            {
                workbook = XDocument.Load(reading);
            }

            XNamespace ns = workbook.Root.Name.Namespace;
            XElement calcPr = workbook.Root.Elements()
                .FirstOrDefault(element => element.Name.LocalName == "calcPr");

            if (calcPr == null)
            {
                // calcPr has a fixed place in the workbook schema, after the sheets and the
                // defined names, and an element out of place is a file Excel repairs.
                calcPr = new XElement(ns + "calcPr");
                XElement anchor = workbook.Root.Elements().LastOrDefault(element =>
                    element.Name.LocalName == "sheets"
                    || element.Name.LocalName == "functionGroups"
                    || element.Name.LocalName == "externalReferences"
                    || element.Name.LocalName == "definedNames");

                if (anchor == null) workbook.Root.Add(calcPr);
                else anchor.AddAfterSelf(calcPr);
            }

            calcPr.SetAttributeValue("fullCalcOnLoad", "1");
            Save(entry, workbook);
        }

        private static string CellText(ZipArchive zip, string partPath, CellRef reference)
        {
            XDocument sheet = WorkbookPackage.Read(zip, partPath);
            if (sheet == null) return null;

            foreach (XElement cell in sheet.Root.Elements()
                .Where(element => element.Name.LocalName == "sheetData")
                .Elements().Where(element => element.Name.LocalName == "row")
                .Elements().Where(element => element.Name.LocalName == "c"))
            {
                CellRef parsed = CellRef.TryParse((string)cell.Attribute("r"));
                if (parsed == null || parsed.Row != reference.Row || parsed.ColumnNumber != reference.ColumnNumber)
                {
                    continue;
                }

                XElement inline = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "is");
                if (inline != null) return string.Concat(inline.Descendants()
                    .Where(child => child.Name.LocalName == "t").Select(child => child.Value));

                XElement value = cell.Elements().FirstOrDefault(child => child.Name.LocalName == "v");
                return value == null ? string.Empty : value.Value;
            }

            return null;
        }

        private static void Save(ZipArchiveEntry entry, XDocument document)
        {
            using (Stream writing = entry.Open())
            {
                writing.SetLength(0);
                document.Save(writing);
            }
        }
    }
}
