using System;
using System.Collections.Generic;
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
    /// Gives every view that names a plot the scope box named for that plot.
    ///
    /// A new view with no scope box is useless on this project, so this belongs to the Drawing
    /// Sheet work rather than being a tool of its own. It applies to every view that names a
    /// plot, not only newly created ones.
    ///
    /// Only case C writes. A view that already carries a scope box is never overwritten,
    /// whether that box is the right one or not, because someone put it there and this command
    /// does not know why.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AssignScopeBoxCommand : IExternalCommand
    {
        public const string ButtonName = "AssignScopeBox";

        public const string ButtonText = "Scope\nBox";

        public const string TransactionName = "Assign scope boxes";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument open = commandData.Application.ActiveUIDocument;
            Document document = open == null ? null : open.Document;

            if (document == null)
            {
                message = "Open a model first. Scope Box works on the document that is in front of you.";
                return Result.Cancelled;
            }

            ScopeBoxScanner.DocumentScopeBoxes found;
            try
            {
                using (var watching = new ScanProgressWindow(
                    commandData.Application.MainWindowHandle,
                    "RCRC Green, Scope Box",
                    "Reading the views in " + document.Title))
                {
                    found = ScopeBoxScanner.Read(document, watching);
                }
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException failed)
            {
                message = "Revit refused part of the read of " + document.Title + ". " + failed.Message;
                return Result.Failed;
            }

            if (found == null)
            {
                TaskDialog.Show(
                    "RCRC Green",
                    "Stopped before the read finished, so no file was written and nothing was changed.");
                return Result.Cancelled;
            }

            // Everything is decided before a transaction exists, so the dialog below can offer
            // a real count and the answer to it cannot change what was counted.
            ScopeBoxPlan plan = ScopeBoxPlan.Decide(found.Views, found.BoxIdByName.Keys);

            bool applied = false;
            var refused = new List<long>();
            int ready = plan.Count(ScopeBoxCase.ReadyToAssign);

            if (ready > 0 && Confirmed(plan, ready))
            {
                try
                {
                    Assign(document, plan, found.BoxIdByName, refused);
                    applied = true;
                }
                catch (Autodesk.Revit.Exceptions.ApplicationException failed)
                {
                    message = "The scope boxes could not be assigned. Nothing was changed. " + failed.Message;
                    return Result.Failed;
                }
            }

            DateTime writtenAt = DateTime.Now;
            string report = ScopeBoxReport.Write(plan, document.Title, writtenAt, applied, refused);
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                ScanFileName.For(ScanFileName.ScopeBoxPrefix, document.Title, writtenAt));

            try
            {
                File.WriteAllText(path, report, new UTF8Encoding(false));
            }
            catch (UnauthorizedAccessException denied)
            {
                message = Written(applied, refused.Count, ready)
                    + " The Desktop folder refused the report. " + denied.Message;
                return Result.Failed;
            }
            catch (DirectoryNotFoundException missing)
            {
                message = Written(applied, refused.Count, ready)
                    + " The Desktop folder was not where Windows said it would be. " + missing.Message;
                return Result.Failed;
            }
            catch (PathTooLongException tooLong)
            {
                message = Written(applied, refused.Count, ready)
                    + " The document title makes a path Windows will not take. " + tooLong.Message;
                return Result.Failed;
            }
            catch (IOException failed)
            {
                message = Written(applied, refused.Count, ready)
                    + " The report could not be written to " + path + ". " + failed.Message;
                return Result.Failed;
            }

            TaskDialog.Show("RCRC Green", Closing(plan, path, applied, refused.Count, ready));
            return Result.Succeeded;
        }

        /// <summary>
        /// One transaction over every assignment, so the whole thing is a single undo. The
        /// progress window is not pumped in here, because letting other clicks through while a
        /// transaction is open is how a model ends up half changed.
        /// </summary>
        private static void Assign(
            Document document,
            ScopeBoxPlan plan,
            Dictionary<string, ElementId> boxIdByName,
            List<long> refused)
        {
            using (var assigning = new Transaction(document, TransactionName))
            {
                assigning.Start();

                foreach (ViewScopeBoxDecision decision in plan.ToAssign)
                {
                    ElementId boxId;
                    if (!boxIdByName.TryGetValue(decision.ScopeBoxToAssign, out boxId))
                    {
                        refused.Add(decision.ViewId);
                        continue;
                    }

                    View view = document.GetElement(new ElementId(decision.ViewId)) as View;
                    Parameter holder = view == null
                        ? null
                        : view.get_Parameter(ScopeBoxScanner.ScopeBoxParameter);

                    if (holder == null || holder.IsReadOnly || !holder.Set(boxId))
                    {
                        // A refusal is recorded rather than thrown, so one awkward view does
                        // not roll back every other assignment in the same transaction.
                        refused.Add(decision.ViewId);
                    }
                }

                assigning.Commit();
            }
        }

        private static bool Confirmed(ScopeBoxPlan plan, int ready)
        {
            var asking = new TaskDialog("RCRC Green, Scope Box")
            {
                MainInstruction = ready == 1
                    ? "Give 1 view its scope box?"
                    : "Give " + Number(ready) + " views their scope boxes?",
                MainContent = Counts(plan)
                    + Environment.NewLine + Environment.NewLine
                    + "Only case C is written. D and F are reported and left as they are. "
                    + "The report is written either way.",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.No
            };

            return asking.Show() == TaskDialogResult.Yes;
        }

        private static string Counts(ScopeBoxPlan plan)
        {
            var said = new StringBuilder();
            said.AppendLine("A, name does not parse: " + Number(plan.Count(ScopeBoxCase.NameDoesNotParse)));
            said.AppendLine("B, cannot hold a scope box: " + Number(plan.Count(ScopeBoxCase.CannotHoldAScopeBox)));
            said.AppendLine("C, ready to assign: " + Number(plan.Count(ScopeBoxCase.ReadyToAssign)));
            said.AppendLine("D, no scope box for that plot: " + Number(plan.Count(ScopeBoxCase.NoMatchingScopeBox)));
            said.AppendLine("E, already right: " + Number(plan.Count(ScopeBoxCase.AlreadyRight)));
            said.Append("F, holds a different scope box: " + Number(plan.Count(ScopeBoxCase.HoldsADifferentScopeBox)));
            return said.ToString();
        }

        private static string Closing(ScopeBoxPlan plan, string path, bool applied, int refusedCount, int ready)
        {
            var said = new StringBuilder();
            said.AppendLine(Written(applied, refusedCount, ready));
            said.AppendLine();
            said.AppendLine("Report written to");
            said.AppendLine(path);
            said.AppendLine();
            said.Append(Counts(plan));
            return said.ToString();
        }

        private static string Written(bool applied, int refusedCount, int ready)
        {
            if (!applied)
            {
                return ready == 0
                    ? "No view was waiting for a scope box, so nothing was changed."
                    : "Nothing was changed.";
            }

            int given = ready - refusedCount;
            string said = Number(given) + (given == 1 ? " view was" : " views were") + " given a scope box.";
            if (refusedCount > 0)
            {
                said += " " + Number(refusedCount) + " were refused by the view and are named in the report.";
            }
            return said;
        }

        private static string Number(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture);
        }
    }
}
