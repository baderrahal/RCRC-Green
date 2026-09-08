using System;
using System.Globalization;
using System.IO;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Reads the open document and writes what it found to a text file on the Desktop.
    ///
    /// The parser in Core was written from four example names. The first real model holds a
    /// sheet numbered 600QD named SOFTSCAPE SCHEDULES, which matches no part of that pattern.
    /// Nothing else gets built until the real naming is written down rather than assumed, so
    /// this command reads and reports and does not touch the model.
    /// </summary>
    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class ScanModelCommand : IExternalCommand
    {
        public const string ButtonName = "ScanModel";

        public const string ButtonText = "Scan\nModel";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument open = commandData.Application.ActiveUIDocument;
            Document document = open == null ? null : open.Document;

            if (document == null)
            {
                message = "Open a model first. Scan Model reads the document that is in front of you.";
                return Result.Cancelled;
            }

            ModelScan scan;
            try
            {
                using (var watching = new ScanProgressWindow(
                    commandData.Application.MainWindowHandle,
                    "RCRC Green, Scan Model",
                    "Reading " + document.Title))
                {
                    // Guarded here as well as round the file write. A model can refuse a read
                    // for reasons of its own, and without this the user gets the raw crash
                    // dialog with a stack trace and no idea which part gave up.
                    if (!ModelScanner.TryRead(document, watching, out scan))
                    {
                        TaskDialog.Show(
                            "RCRC Green",
                            "Stopped before the scan finished, so no file was written and nothing was changed.");
                        return Result.Cancelled;
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                message = "Revit refused part of the scan of " + document.Title + ". " + failed.Message;
                return Result.Failed;
            }

            if (scan.FoundNoViews)
            {
                TaskDialog.Show(
                    "RCRC Green",
                    "No views were found in " + document.Title + ", so no file was written."
                        + Environment.NewLine + Environment.NewLine
                        + "A model with no views at all is worth a second look before this is run again.");
                return Result.Succeeded;
            }

            DateTime writtenAt = DateTime.Now;
            string report = ScanReport.Write(scan, writtenAt);
            string fileName = ScanFileName.For(scan.DocumentTitle, writtenAt);
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string path = Path.Combine(desktop, fileName);

            try
            {
                File.WriteAllText(path, report, new UTF8Encoding(false));
            }
            catch (UnauthorizedAccessException denied)
            {
                message = "The Desktop folder refused the write. " + denied.Message;
                return Result.Failed;
            }
            catch (DirectoryNotFoundException missing)
            {
                message = "The Desktop folder was not where Windows said it would be, "
                    + desktop + ". " + missing.Message;
                return Result.Failed;
            }
            catch (PathTooLongException tooLong)
            {
                message = "The document title makes a path Windows will not take, "
                    + path + ". " + tooLong.Message;
                return Result.Failed;
            }
            catch (IOException failed)
            {
                message = "The file could not be written to " + path + ". " + failed.Message;
                return Result.Failed;
            }

            TaskDialog.Show("RCRC Green", Headline(scan, path));
            return Result.Succeeded;
        }

        private static string Headline(ModelScan scan, string path)
        {
            NameParseSummary parsing = NameParseSummary.Of(scan);
            var said = new StringBuilder();

            said.AppendLine("Scan written to");
            said.AppendLine(path);
            said.AppendLine();
            said.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0} sheets, {1} views on sheets, {2} views not on sheets, {3} view templates",
                scan.Sheets.Count,
                Many(scan.ViewsOnSheets),
                Many(scan.ViewsNotOnSheets),
                Many(scan.Templates)));
            said.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0} scope boxes, {1} distinct PRX_Plot_ID values from {2} elements in {3} seconds",
                scan.ScopeBoxes.Count,
                scan.PlotIdValues.Count,
                scan.ElementsScanned,
                scan.ScanSeconds.ToString("0.0", CultureInfo.InvariantCulture)));
            said.AppendLine();
            said.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0} names parsed, {1} did not. The ones that did not are listed in the file.",
                parsing.ParsedTotal,
                parsing.NotParsedTotal));
            said.AppendLine();
            said.Append("Nothing in the model was changed.");

            return said.ToString();
        }

        private static int Many(System.Collections.Generic.IEnumerable<ScannedView> views)
        {
            int counted = 0;
            foreach (ScannedView ignored in views)
            {
                counted++;
            }
            return counted;
        }
    }
}
