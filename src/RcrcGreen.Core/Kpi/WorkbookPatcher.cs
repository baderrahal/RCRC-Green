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
        /// <summary>
        /// <paramref name="computesFrom"/> is every cell the map names on the main sheet, written
        /// or not, so the formula check can say whether the workbook's own arithmetic has all its
        /// inputs. Null checks the written cells alone.
        /// </summary>
        public static PatchOutcome Patch(
            string sourcePath,
            string outputPath,
            IReadOnlyList<CellWrite> writes,
            IEnumerable<WorkbookCell> computesFrom = null,
            Action<string> step = null)
        {
            if (sourcePath == null) throw new ArgumentNullException("sourcePath");
            if (outputPath == null) throw new ArgumentNullException("outputPath");
            if (writes == null) throw new ArgumentNullException("writes");
            if (writes.Count == 0) return PatchOutcome.Refused("Nothing to write.");

            // **NOTHING MAY WRITE TO A TEMPLATE.** The second of two guards, and the reason
            // there are two is that either alone is one refactor from being bypassed. This one
            // stands before any file is opened, so a caller that reaches the patcher without
            // going through the handler still cannot destroy a client workbook.
            SamePath answer = FilePaths.Compare(sourcePath, outputPath);
            if (answer != SamePath.Different)
            {
                return PatchOutcome.Refused(CreateWords.WouldOverwriteTheTemplate(outputPath, answer));
            }

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

                step?.Invoke(ProgressWords.CopyingTheTemplate);
                File.Copy(sourcePath, outputPath, true);

                var changed = new List<string>();
                int dropped = 0;
                bool calcChainRemoved = false;

                using (FileStream updating = new FileStream(outputPath, FileMode.Open, FileAccess.ReadWrite))
                using (var zip = new ZipArchive(updating, ZipArchiveMode.Update))
                {
                    step?.Invoke(ProgressWords.WritingTheCells);
                    foreach (IGrouping<string, CellWrite> perSheet in writes.GroupBy(write => write.SheetName, StringComparer.Ordinal))
                    {
                        string partPath = sheetParts[perSheet.Key];
                        PatchSheet(zip, partPath, perSheet.ToList());
                        changed.Add(partPath);
                    }

                    // THE FLAG ON ITS OWN IS NOT ENOUGH. The template already carried
                    // fullCalcOnLoad="1" and Excel still showed the cached zeros, because
                    // calcId said the cache was written by an engine as new as its own. All
                    // three of these together are what makes it recalculate.
                    SetRecalculateOnOpen(zip, workbookPart);
                    if (!changed.Contains(workbookPart)) changed.Add(workbookPart);

                    foreach (string partPath in sheetParts.Values.Distinct(StringComparer.Ordinal))
                    {
                        int off = DropCachedResults(zip, partPath);
                        if (off == 0) continue;

                        dropped += off;
                        if (!changed.Contains(partPath)) changed.Add(partPath);
                    }

                    calcChainRemoved = RemoveCalcChain(zip);
                }

                int partsInOutput;
                CacheCheck cache;
                FormulaCheck formulas;
                step?.Invoke(ProgressWords.ReadingThemBack);
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

                    // Read back off the output, the same rule every written cell follows. A
                    // check made against what was sent would have passed on the file that
                    // opened showing zeros.
                    cache = Checked(zip, workbookPart, sheetParts.Values, dropped, calcChainRemoved);

                    // What the output's formulas will make of the cells that landed. A written
                    // row whose neighbouring formula returns a space, and a formula that
                    // multiplies that space, is #VALUE! whatever Excel recalculates.
                    step?.Invoke(ProgressWords.CheckingTheFormulas);
                    formulas = WorkbookFormulas.Check(
                        zip, workbookPart, sheetParts,
                        writes.Select(write => new WorkbookCell(write.SheetName, write.Cell.ToString())),
                        computesFrom);
                }

                // A run that writes cells nobody can compute from is a refusal and not a note.
                // The file was written to be checked, and it is taken away again, because a
                // workbook whose KPI row reads #VALUE! looks finished to anyone who does not
                // scroll to it.
                if (formulas.RefusesTheWrite)
                {
                    File.Delete(outputPath);
                    return PatchOutcome.RefusedAfterWriting(formulas.Refusal, formulas);
                }

                return PatchOutcome.Done(partsInSource, partsInOutput, changed, landed, cache, formulas);
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

            // Zero tells Excel it cannot know which engine wrote the cache, so it recalculates
            // rather than trusting it. The client's template reads 191029, which is as new as
            // Excel's own, and that is why the flag beside it changed nothing.
            calcPr.SetAttributeValue("calcId", "0");

            // Auto, said outright. The template carried no calcMode, and Excel takes its
            // calculation mode from the first workbook opened in the session, so the 1428 file
            // opened into a manual session showing every formula cell blank.
            calcPr.SetAttributeValue("calcMode", "auto");
            Save(entry, workbook);
        }

        /// <summary>
        /// Takes the cached result off every formula cell in one sheet and leaves the formula
        /// alone. A cell with an f and no v is one Excel has to work out.
        ///
        /// Every sheet, not only the ones that received a value: Total Planting Area is =F10 on
        /// a sheet this tool wrote to and it still showed 0, and the six other cells that showed
        /// 0 are spread across the workbook.
        /// </summary>
        private static int DropCachedResults(ZipArchive zip, string partPath)
        {
            ZipArchiveEntry entry = zip.GetEntry(partPath);
            if (entry == null) return 0;

            XDocument sheet;
            using (Stream reading = entry.Open())
            {
                sheet = XDocument.Load(reading);
            }

            List<XElement> cached = sheet.Root.Elements()
                .Where(element => element.Name.LocalName == "sheetData")
                .Elements().Where(element => element.Name.LocalName == "row")
                .Elements().Where(element => element.Name.LocalName == "c")
                .Where(cell => cell.Elements().Any(child => child.Name.LocalName == "f"))
                .SelectMany(cell => cell.Elements().Where(child => child.Name.LocalName == "v"))
                .ToList();

            if (cached.Count == 0) return 0;

            foreach (XElement value in cached) value.Remove();

            Save(entry, sheet);
            return cached.Count;
        }

        /// <summary>
        /// The calculation chain is Excel's record of what order to work the formulas out in,
        /// and it is written against the cached results this tool has just dropped. Excel
        /// rebuilds it on the first recalculation, so removing it is safe and leaving it is a
        /// file that disagrees with itself. This is the one part the output is short of, and the
        /// report says which and why so the count does not read as a loss.
        /// </summary>
        private static bool RemoveCalcChain(ZipArchive zip)
        {
            ZipArchiveEntry entry = zip.GetEntry(CalcChainPart);
            if (entry == null) return false;

            entry.Delete();
            return true;
        }

        public const string CalcChainPart = "xl/calcChain.xml";

        private static CacheCheck Checked(
            ZipArchive zip,
            string workbookPart,
            IEnumerable<string> sheetPaths,
            int dropped,
            bool calcChainRemoved)
        {
            XDocument workbook = WorkbookPackage.Read(zip, workbookPart);
            XElement calcPr = workbook == null || workbook.Root == null
                ? null
                : workbook.Root.Elements().FirstOrDefault(element => element.Name.LocalName == "calcPr");

            bool onLoad = calcPr != null
                && string.Equals((string)calcPr.Attribute("fullCalcOnLoad"), "1", StringComparison.Ordinal);
            string calcId = calcPr == null ? string.Empty : ((string)calcPr.Attribute("calcId") ?? string.Empty);
            string calcMode = calcPr == null ? string.Empty : ((string)calcPr.Attribute("calcMode") ?? string.Empty);

            // Anything else in the package that can hold a calculation setting the workbook
            // part does not: a sheet's own sheetCalcPr, a macro, or another attribute on calcPr.
            // Found or not, the report says what was looked for.
            var other = new List<string>();
            if (calcPr != null)
            {
                foreach (XAttribute attribute in calcPr.Attributes())
                {
                    string name = attribute.Name.LocalName;
                    if (name == "fullCalcOnLoad" || name == "calcId" || name == "calcMode") continue;

                    other.Add("calcPr also carries " + name + "=\"" + attribute.Value + "\"");
                }
            }

            if (zip.GetEntry(VbaPart) != null)
            {
                other.Add(VbaPart + " is in the package, and a macro can set the calculation mode when the file opens");
            }

            int stillCached = 0;
            foreach (string partPath in sheetPaths.Distinct(StringComparer.Ordinal))
            {
                XDocument sheet = WorkbookPackage.Read(zip, partPath);
                if (sheet == null) continue;

                stillCached += sheet.Root.Elements()
                    .Where(element => element.Name.LocalName == "sheetData")
                    .Elements().Where(element => element.Name.LocalName == "row")
                    .Elements().Where(element => element.Name.LocalName == "c")
                    .Count(cell => cell.Elements().Any(child => child.Name.LocalName == "f")
                        && cell.Elements().Any(child => child.Name.LocalName == "v"));

                foreach (XElement sheetCalcPr in sheet.Root.Elements().Where(element => element.Name.LocalName == "sheetCalcPr"))
                {
                    other.Add(partPath + " carries sheetCalcPr"
                        + string.Concat(sheetCalcPr.Attributes().Select(one => " " + one.Name.LocalName + "=\"" + one.Value + "\"")));
                }
            }

            return new CacheCheck(
                onLoad,
                calcId,
                stillCached,
                calcChainRemoved && zip.GetEntry(CalcChainPart) == null,
                dropped,
                calcMode,
                other);
        }

        public const string VbaPart = "xl/vbaProject.bin";

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
