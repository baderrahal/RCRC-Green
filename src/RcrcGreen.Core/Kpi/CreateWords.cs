using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Every line the plot picker and the Create block show. The pane draws them and formats
    /// none of them, the same rule PanelSteps follows for the Drawing Sheet, because a summary
    /// written next to the control that shows it is two records of one fact.
    /// </summary>
    public static class CreateWords
    {
        public const string Heading = "Plots";

        public const string SelectAll = "Select all";

        public const string Clear = "Clear";

        public const string Create = "Create";

        public const string NoPlots =
            "No plot in this model. Press KPI Scan first, or open a model that holds one.";

        public const string Overwrite =
            "A file of this name in that folder is overwritten without asking.";

        public const string NoModel = "No model is open.";

        public const string NoTemplate = "No template picked.";

        public const string NoPlotTicked = "No plot ticked.";

        public const string AreaTypedByHand =
            "This template takes no area. The road width and the total length are typed by hand.";

        /// <summary>
        /// One refusal listing everything that is missing, rather than one per thing. Pressing
        /// Create three times to be told three separate halves of the same answer is worse than
        /// being told all of it once.
        /// </summary>
        public static string CannotCreate(bool hasModel, bool hasTemplate, bool hasAPlot)
        {
            var missing = new List<string>();
            if (!hasModel) missing.Add(NoModel);
            if (!hasTemplate) missing.Add(NoTemplate);
            if (!hasAPlot) missing.Add(NoPlotTicked);

            if (missing.Count == 0) return string.Empty;

            return "Cannot create. " + string.Join(" ", missing.ToArray());
        }

        /// <summary>
        /// What the two lists of plots disagree about, said on screen rather than resolved.
        /// PRX_Plot_ID on the sheets and the PRX_Ref Plot ID filter on the schedules are two
        /// records of one fact and this is where they are held apart.
        /// </summary>
        public static IReadOnlyList<string> PlotSources(PlotsInTheModel plots)
        {
            if (plots == null) throw new ArgumentNullException("plots");

            var said = new List<string>();
            if (plots.All.Count == 0)
            {
                said.Add(NoPlots);
                return said;
            }

            if (plots.Agree)
            {
                said.Add("The sheets and the schedules name the same "
                    + Count(plots.All.Count, "plot") + ".");
                return said;
            }

            said.Add("The sheets and the schedules do not name the same plots. Both lists are offered.");

            if (plots.OnSheetsOnly.Count > 0)
            {
                said.Add("  On a sheet and on no schedule, " + plots.OnSheetsOnly.Count + ": "
                    + string.Join(", ", plots.OnSheetsOnly.ToArray()));
            }

            if (plots.OnSchedulesOnly.Count > 0)
            {
                said.Add("  On a schedule and on no sheet, " + plots.OnSchedulesOnly.Count + ": "
                    + string.Join(", ", plots.OnSchedulesOnly.ToArray()));
            }

            return said;
        }

        /// <summary>
        /// The line that asks a person to confirm before two identical areas are added.
        /// </summary>
        public static IReadOnlyList<string> ConfirmIdentical(IReadOnlyList<IdenticalArea> identical)
        {
            var said = new List<string>();
            if (identical == null || identical.Count == 0) return said;

            said.Add("CONFIRM BEFORE WRITING. " + Count(identical.Count, "set")
                + " of chosen plots report the same area.");

            foreach (IdenticalArea shared in identical)
            {
                said.Add("  " + string.Join(", ", shared.Plots.ToArray()) + " all read " + shared.Printed
                    + ". Either they are the same size or one region is counted twice.");
            }

            return said;
        }

        public static string Refused(Reconciliation reconciliation)
        {
            if (reconciliation == null) throw new ArgumentNullException("reconciliation");
            if (reconciliation.AddsUp) return string.Empty;

            return "Nothing was written. " + string.Join(" ", reconciliation.Refusals.ToArray());
        }

        /// <summary>
        /// The status line after a run that wrote, counting what landed rather than what was
        /// planned.
        /// </summary>
        public static string Wrote(KpiCreateRun run, string reportWhere)
        {
            if (run == null) throw new ArgumentNullException("run");
            if (!run.Wrote) return Refused(run.Reconciliation);

            return Count(run.Outcome.Landed.Count, "cell") + " written from "
                + Count(run.Readings.Count, "plot") + ", "
                + Count(run.Plan.Skipped.Count, "cell") + " not written. "
                + "Workbook: " + run.OutputPath + ". Report: " + reportWhere;
        }

        /// <summary>
        /// The name offered in the box: the template name and the component, falling back to
        /// the plot when there is one and to the ticked count when there are several.
        /// </summary>
        public static string SuggestedName(KpiTemplate template, string component, IReadOnlyList<string> ticked)
        {
            string front = template == null ? "KPI" : template.Name;
            IReadOnlyList<string> plots = ticked ?? new List<string>();

            if (!string.IsNullOrWhiteSpace(component)) return front + " " + component.Trim();
            if (plots.Count == 1) return front + " " + plots[0];
            if (plots.Count > 1) return front + " " + plots.Count + " plots";

            return front;
        }

        private static string Count(int howMany, string thing)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + " " + thing + (howMany == 1 ? string.Empty : "s");
        }
    }
}
