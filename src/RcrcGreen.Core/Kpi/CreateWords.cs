using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The open document as Revit last answered for it. Its title, and nothing else.
    ///
    /// **THE MODEL'S FOLDER USED TO BE ON HERE AND IS GONE.** The workbook was written beside
    /// the model, so a model that had never been saved could not be used at all, and a detached
    /// one is exactly what the team works on. The workbook goes to a folder the user browses for
    /// now, remembered the way the template folder is, and Create no longer asks whether the
    /// model has been saved.
    ///
    /// The folder is not kept for anything else. A value on the screen that decides nothing is
    /// how one stale string became a dead end here once already.
    ///
    /// A title is empty only when there is no document.
    /// </summary>
    public sealed class OpenModel
    {
        private OpenModel(string title)
        {
            Title = title ?? string.Empty;
        }

        /// <summary>
        /// No document. What the pane holds before Revit has answered and what Revit answers
        /// when the model is closed while the pane is still on screen.
        /// </summary>
        public static readonly OpenModel Nothing = new OpenModel(string.Empty);

        public static OpenModel Of(string documentTitle)
        {
            return new OpenModel(documentTitle);
        }

        public string Title { get; }

        public bool IsOpen
        {
            get { return Title.Length > 0; }
        }

        public bool Is(OpenModel other)
        {
            return other != null && string.Equals(Title, other.Title, StringComparison.Ordinal);
        }
    }

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

        /// <summary>
        /// Over the grouping buttons. It says the prefix is what gathers them, because a button
        /// reading MOSQUES beside a plot list nobody can see the rule behind is a button whose
        /// answer has to be taken on trust.
        /// </summary>
        public const string GroupsHeading =
            "Or tick every plot for one template at once, gathered by the plot prefix:";

        /// <summary>
        /// One grouping button's text: the template and how many plots it would tick, so the
        /// count is known before the press rather than after it.
        /// </summary>
        public static string GroupLabel(TemplateByPrefix group)
        {
            if (group == null) throw new ArgumentNullException("group");

            return group.Name + ", " + Count(group.Plots.Count, "plot");
        }

        /// <summary>
        /// The plots no grouping button reaches, named rather than left out. A prefix the table
        /// does not hold is a real answer and a plot carrying one is ticked by hand.
        /// </summary>
        public static string NoGroupFor(IReadOnlyList<string> plots)
        {
            if (plots == null || plots.Count == 0) return string.Empty;

            return "No button gathers " + string.Join(", ", plots.ToArray())
                + ". The prefix table does not hold "
                + (plots.Count == 1 ? "that prefix" : "those prefixes")
                + ", so tick " + (plots.Count == 1 ? "it" : "them") + " by hand.";
        }

        public const string NoPlots =
            "No plot in this model. Press KPI Scan first, or open a model that holds one.";

        public const string NoModel = "No model is open.";

        /// <summary>
        /// The refusal that replaced the never saved one. **Whether the model has been saved is
        /// no longer asked**, because writing beside the model meant a detached model could not
        /// be used at all, and that cost the team most of an afternoon.
        ///
        /// The words are TemplateWords.NoOutputFolder, which the output folder line already
        /// shows, because two sentences for one condition is two records of one fact.
        /// </summary>
        public const string NoOutputFolder = TemplateWords.NoOutputFolder;

        public const string NoTemplate = "No template picked.";

        public const string NoPlotTicked = "No plot ticked.";

        public const string AreaTypedByHand =
            "This template takes no area. The road width and the total length are typed by hand.";

        /// <summary>
        /// Under the Reference picker with nothing ticked. The values shown there belong to a
        /// plot, so with no plot chosen there is nothing to show and the block says so rather
        /// than showing some other plot's.
        /// </summary>
        public const string NoPlotForTheReferenceValues =
            "No plot is ticked, so there is no value to show here. Tick a plot to see what each "
            + "of the four holds on it.";

        /// <summary>
        /// The heading over the four values, naming the plot they belong to.
        ///
        /// **The plot is named because the block was showing DM-11's values with DM-12 ticked.**
        /// It read the first plot in the model's list rather than the first ticked one, and the
        /// whole point of the block is that a person picks the reference by looking at its
        /// value. A value belonging to a plot they did not choose is worse than no value.
        /// </summary>
        public static string ReferenceValuesOn(string plotId)
        {
            return "What each holds on " + (string.IsNullOrWhiteSpace(plotId) ? "(no plot)" : plotId.Trim())
                + ", the first ticked plot:";
        }

        /// <summary>
        /// One refusal listing everything that is missing, rather than one per thing. Pressing
        /// Create three times to be told three separate halves of the same answer is worse than
        /// being told all of it once.
        ///
        /// **It no longer asks whether the model has been saved.** It asks whether there is
        /// somewhere to write, which is the browsed output folder. The two used to be one
        /// question because the workbook went beside the model, and a detached model was refused
        /// with No model is open next to a header counting its 96,959 elements.
        ///
        /// The folder is passed in rather than held, and the Revit side reads it off disk at the
        /// moment Create is pressed, so nothing here can be deciding on a copy taken earlier.
        /// </summary>
        public static string CannotCreate(
            OpenModel model, string outputFolder, bool hasTemplate, bool hasAPlot)
        {
            OpenModel open = model ?? OpenModel.Nothing;

            var missing = new List<string>();
            if (!open.IsOpen) missing.Add(NoModel);
            if (string.IsNullOrWhiteSpace(outputFolder)) missing.Add(NoOutputFolder);
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
