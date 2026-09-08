using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RcrcGreen.Core
{
    /// <summary>
    /// Turns a <see cref="ScopeBoxPlan"/> into the text that goes in the file, with a section
    /// per case and a count in every heading.
    /// </summary>
    public static class ScopeBoxReport
    {
        public static string Write(
            ScopeBoxPlan plan,
            string documentTitle,
            DateTime writtenAt,
            bool applied,
            IEnumerable<long> refusedViewIds)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            if (documentTitle == null) throw new ArgumentNullException("documentTitle");

            var refused = new HashSet<long>(refusedViewIds ?? Enumerable.Empty<long>());
            var report = new StringBuilder();

            int ready = plan.Count(ScopeBoxCase.ReadyToAssign);
            int assigned = applied ? ready - refused.Count : 0;

            Line(report, "RCRC Green scope box assignment");
            Line(report, "Document: " + documentTitle);
            Line(report, "Written: " + writtenAt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture));
            Line(report, applied
                ? Number(assigned) + (assigned == 1 ? " view was" : " views were")
                    + " given a scope box. Nothing else was changed."
                : "Nothing was changed. The assignment was not confirmed.");
            Line(report, "Only case C writes. D and F are reported and left alone.");
            Line(report, "A scope box matches a plot on an exact, case sensitive name.");
            Line(report, string.Empty);

            Section(report, plan, ScopeBoxCase.NameDoesNotParse,
                "A, NAME DOES NOT PARSE", "view name");
            foreach (ViewScopeBoxDecision decision in Ordered(plan, ScopeBoxCase.NameDoesNotParse))
            {
                Line(report, decision.ViewName);
            }
            Line(report, string.Empty);

            // B is a count and nothing else. A drafting view or a schedule has no scope box
            // parameter, there is no fault in that, and listing every one of them would bury
            // the cases that need reading.
            Line(report, Heading("B, CANNOT HOLD A SCOPE BOX", plan.Count(ScopeBoxCase.CannotHoldAScopeBox)));
            Line(report, "Counted only. A view with no scope box parameter is not a fault.");
            Line(report, string.Empty);

            Section(report, plan, ScopeBoxCase.ReadyToAssign,
                "C, ASSIGNED", "view name | scope box | outcome");
            foreach (ViewScopeBoxDecision decision in Ordered(plan, ScopeBoxCase.ReadyToAssign))
            {
                string outcome;
                if (!applied)
                {
                    outcome = "not confirmed, nothing written";
                }
                else if (refused.Contains(decision.ViewId))
                {
                    outcome = "refused by the view";
                }
                else
                {
                    outcome = "assigned";
                }

                Line(report, Join(decision.ViewName, decision.ScopeBoxToAssign, outcome));
            }
            if (applied && refused.Count > 0)
            {
                Line(report, string.Empty);
                Line(report, Number(refused.Count) + " of those were refused by the view and did not change.");
            }
            Line(report, string.Empty);

            Section(report, plan, ScopeBoxCase.NoMatchingScopeBox,
                "D, NO SCOPE BOX FOR THAT PLOT", "view name | plot it wanted");
            foreach (ViewScopeBoxDecision decision in Ordered(plan, ScopeBoxCase.NoMatchingScopeBox))
            {
                Line(report, Join(decision.ViewName, decision.PlotId));
            }
            Line(report, string.Empty);

            Section(report, plan, ScopeBoxCase.AlreadyRight,
                "E, ALREADY RIGHT", "view name | scope box");
            foreach (ViewScopeBoxDecision decision in Ordered(plan, ScopeBoxCase.AlreadyRight))
            {
                Line(report, Join(decision.ViewName, decision.CurrentScopeBoxName));
            }
            Line(report, string.Empty);

            Section(report, plan, ScopeBoxCase.HoldsADifferentScopeBox,
                "F, HOLDS A DIFFERENT SCOPE BOX", "view name | scope box it has | scope box expected");
            foreach (ViewScopeBoxDecision decision in Ordered(plan, ScopeBoxCase.HoldsADifferentScopeBox))
            {
                Line(report, Join(decision.ViewName, decision.CurrentScopeBoxName, decision.PlotId));
            }

            return report.ToString();
        }

        private static IEnumerable<ViewScopeBoxDecision> Ordered(ScopeBoxPlan plan, ScopeBoxCase outcome)
        {
            return plan.Of(outcome).OrderBy(decision => decision.ViewName, NaturalOrder.Comparer);
        }

        private static void Section(
            StringBuilder report, ScopeBoxPlan plan, ScopeBoxCase outcome, string title, string columns)
        {
            Line(report, Heading(title, plan.Count(outcome)));
            Line(report, columns);
        }

        private static string Heading(string title, int count)
        {
            return "== " + title + " (" + count.ToString(CultureInfo.InvariantCulture) + ") ==";
        }

        private static string Join(params string[] fields)
        {
            return string.Join(" | ", fields);
        }

        private static string Number(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture);
        }

        private static void Line(StringBuilder report, string text)
        {
            report.Append(text);
            report.Append(ScanReport.LineEnd);
        }
    }
}
