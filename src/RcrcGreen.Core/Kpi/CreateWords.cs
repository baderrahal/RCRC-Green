using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The open document as Revit last answered for it: its title, and the folder it sits in.
    ///
    /// **The two are one record and neither is worked out anywhere else.** The pane used to
    /// hold them as two loose strings and hand them to the refusal in the wrong order, so a
    /// detached model that had never been saved was refused with No model is open. Deriving
    /// both from one answer is what stops a caller getting them the wrong way round.
    ///
    /// A title is empty only when there is no document. A folder is empty for a document that
    /// has never been saved, and for a cloud model whose path is not a folder on disk.
    /// </summary>
    public sealed class OpenModel
    {
        private OpenModel(string title, string folder)
        {
            Title = title ?? string.Empty;
            Folder = folder ?? string.Empty;
        }

        /// <summary>
        /// No document. What the pane holds before Revit has answered and what Revit answers
        /// when the model is closed while the pane is still on screen.
        /// </summary>
        public static readonly OpenModel Nothing = new OpenModel(string.Empty, string.Empty);

        public static OpenModel Of(string documentTitle, string modelFolder)
        {
            return new OpenModel(documentTitle, modelFolder);
        }

        public string Title { get; }

        public string Folder { get; }

        public bool IsOpen
        {
            get { return Title.Length > 0; }
        }

        /// <summary>
        /// False for a model that has never been saved. The workbook is written beside the
        /// model, so there is nowhere to write until it has one.
        /// </summary>
        public bool HasAFolder
        {
            get { return Folder.Length > 0; }
        }

        public bool Is(OpenModel other)
        {
            return other != null
                && string.Equals(Title, other.Title, StringComparison.Ordinal)
                && string.Equals(Folder, other.Folder, StringComparison.Ordinal);
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

        public const string NoPlots =
            "No plot in this model. Press KPI Scan first, or open a model that holds one.";

        public const string Overwrite =
            "A file of this name in that folder is overwritten without asking.";

        public const string NoModel = "No model is open.";

        /// <summary>
        /// A model that is open and has never been saved. It is a different refusal from no
        /// model at all, and the pane said the wrong one of the two: the header read 96,959
        /// elements while Create said no model was open, on a detached model with no path.
        ///
        /// The words are TemplateWords.NoModelPath, which the line above the button already
        /// shows, because two sentences for one condition is two records of one fact.
        /// </summary>
        public const string NotSaved = TemplateWords.NoModelPath;

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
        /// **Whether a model is open and whether it has a folder are two facts.** The button
        /// used to be handed the folder and call it the model, so a detached model that had
        /// never been saved was reported as no model open, next to a header counting its
        /// 96,959 elements. A model that is not open is not asked whether it has been saved.
        ///
        /// It takes an <see cref="OpenModel"/> rather than two loose flags, so a caller cannot
        /// hand it the two the wrong way round. That is the whole of the fault it is here for.
        /// </summary>
        public static string CannotCreate(OpenModel model, bool hasTemplate, bool hasAPlot)
        {
            OpenModel open = model ?? OpenModel.Nothing;

            var missing = new List<string>();
            if (!open.IsOpen) missing.Add(NoModel);
            else if (!open.HasAFolder) missing.Add(NotSaved);
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
