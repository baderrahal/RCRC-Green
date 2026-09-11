using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RcrcGreen.Core;

namespace RcrcGreen.Revit
{
    /// <summary>
    /// Gives every view that names a plot the scope box named for that plot.
    ///
    /// Only case C writes. A view that already carries a scope box is never overwritten,
    /// whether that box is the right one or not, because someone put it there and this code
    /// does not know why.
    ///
    /// It was a ribbon button with an Execute of its own. The button went when the panel took
    /// the work over, the Execute stayed unreachable for several rounds, and it is gone now.
    /// What is left is the three steps the handler runs from inside the panel: the
    /// confirmation, the assignment and the report.
    /// </summary>
    internal static class AssignScopeBoxCommand
    {
        public const string TransactionName = "Assign scope boxes";

        /// <summary>
        /// One transaction over every assignment, so the whole thing is a single undo. Nothing
        /// pumps the message queue in here, because letting other clicks through while a
        /// transaction is open is how a model ends up half changed.
        /// </summary>
        internal static void Assign(
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

        /// <summary>
        /// Where the report landed, which is the reports folder inside the repo or nowhere. It
        /// is what lets anybody reading the code see what a real run produced.
        /// </summary>
        internal static IReadOnlyList<string> WriteReport(
            ScopeBoxPlan plan, string documentTitle, bool applied, IEnumerable<long> refused)
        {
            DateTime writtenAt = DateTime.Now;

            return ReportFile.Write(
                ScanFileName.For(ReportFileNames.ScopeBoxPrefix, documentTitle, writtenAt),
                ScopeBoxReport.Write(plan, documentTitle, writtenAt, applied, refused));
        }

        internal static bool Confirmed(ScopeBoxPlan plan, int ready)
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

        private static string Number(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture);
        }
    }
}
