using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core
{
    /// <summary>
    /// The five things a person does, in the order they do them.
    /// </summary>
    public enum PanelStep
    {
        Plots = 1,
        ViewTypes = 2,
        Mark = 3,
        Sheets = 4,
        Run = 5
    }

    /// <summary>
    /// One step: what it is called, what it says when it is shut, and whether it can be used
    /// yet.
    /// </summary>
    public sealed class StepState
    {
        internal StepState(
            PanelStep step, string title, string summary, bool usable, string whyNot, bool done)
        {
            Step = step;
            Title = title;
            Summary = summary ?? string.Empty;
            Usable = usable;
            WhyNot = whyNot ?? string.Empty;
            Done = done;
        }

        public PanelStep Step { get; }

        public int Number
        {
            get { return (int)Step; }
        }

        public string Title { get; }

        /// <summary>
        /// What the step says while it is shut, so somebody can read the state of the whole
        /// panel without opening anything. Empty when there is nothing to say yet.
        /// </summary>
        public string Summary { get; }

        public bool Usable { get; }

        /// <summary>
        /// One line, in the words the user needs. Empty when the step is usable. A step that is
        /// greyed out with no reason is worse than one that is not there.
        /// </summary>
        public string WhyNot { get; }

        /// <summary>
        /// Whether enough has been done in this step to move on. It is what opens the next one.
        /// </summary>
        public bool Done { get; }

        /// <summary>
        /// The whole header line, built here so the number, the title and the summary are
        /// spaced the same way in every step.
        /// </summary>
        public string Header
        {
            get
            {
                string start = Number.ToString(CultureInfo.InvariantCulture) + "  " + Title;
                return Summary.Length == 0 ? start : start + "   " + Summary;
            }
        }
    }

    /// <summary>
    /// The state of all five steps, worked out from plain numbers.
    ///
    /// Every count shown in a header comes from here rather than being formatted next to the
    /// control that shows it. The panel had two records of one fact three times running, and a
    /// summary written where it is drawn is exactly that shape again.
    /// </summary>
    public sealed class PanelSteps
    {
        private readonly IReadOnlyList<StepState> _steps;

        private PanelSteps(IReadOnlyList<StepState> steps)
        {
            _steps = steps;
        }

        public IReadOnlyList<StepState> All
        {
            get { return _steps; }
        }

        public StepState For(PanelStep step)
        {
            StepState found = _steps.FirstOrDefault(one => one.Step == step);
            if (found == null) throw new ArgumentException("No such step.", "step");
            return found;
        }

        /// <summary>
        /// The step to open once this one is finished. It skips anything that cannot be used
        /// yet, so finishing the plots with a model that holds no sheets lands on the marking
        /// rather than on a step that would only say why it is shut.
        ///
        /// Null when there is nothing further to open, which leaves the current step where it
        /// is rather than closing everything.
        /// </summary>
        public PanelStep? OpenAfter(PanelStep finished)
        {
            return _steps
                .Where(one => one.Number > (int)finished)
                .Where(one => one.Usable)
                .Select(one => (PanelStep?)one.Step)
                .FirstOrDefault();
        }

        /// <summary>
        /// The first step worth opening when the panel has just read a model. It is the first
        /// one that is usable and not already finished, so a second read of the same model does
        /// not throw the user back to the top.
        ///
        /// Nothing usable at all lands on step 1, because step 1 is where the reason is
        /// written. Landing on the last step would show somebody a shut Run and no explanation.
        /// </summary>
        public PanelStep FirstUnfinished
        {
            get
            {
                StepState found = _steps.FirstOrDefault(one => one.Usable && !one.Done);
                if (found != null) return found.Step;

                return _steps.Any(one => one.Usable) ? PanelStep.Run : PanelStep.Plots;
            }
        }

        /// <summary>
        /// Said on step 1 and on the empty grid, once. The list is the union of four sources
        /// since the plot registry was wired in, and the two copies of this sentence, one
        /// here and one written out in the panel, both still named two.
        /// </summary>
        public const string NoPlotsInTheModel =
            "This model holds no plots. No view name, no PRX_Plot_ID on a view or an element "
            + "and no scope box gives one.";

        /// <summary>
        /// Said when Run or Assign is pressed with no plot ticked. The verb is the button's.
        /// </summary>
        public static string NoPlotsTicked(string verb)
        {
            return "No plots are ticked, so there is nothing to " + verb + ".";
        }

        public const string NothingToRunYet =
            "Nothing is marked and no sheet can be made. Click an empty cell in step 3, or add "
            + "a sheet in step 4 and give it views, a name and a number.";

        /// <summary>
        /// The line under the range pickers in step 1.
        /// </summary>
        public static string PlotsLine(bool modelEmpty, int plotsTicked, int plotsInRange, int plotsInModel)
        {
            if (modelEmpty) return "No plots in this model.";

            return plotsTicked + " of " + plotsInRange + " plots in range ticked, " + plotsInModel
                + " in the model. Untick one to leave it out of the counts and out of anything "
                + "that writes.";
        }

        /// <summary>
        /// The line over the scope box cases in step 5.
        /// </summary>
        public static string ScopeBoxLine(int viewsConsidered, int plotsTicked)
        {
            return viewsConsidered + (viewsConsidered == 1 ? " view" : " views") + " across "
                + plotsTicked + (plotsTicked == 1 ? " ticked plot" : " ticked plots")
                + ". Only C is written.";
        }

        /// <summary>
        /// Why the grid has no rows, in the words the user needs rather than a blank area.
        /// </summary>
        public static string NothingToDraw(bool readOnce, bool modelEmpty, bool prefixPicked)
        {
            if (!readOnce) return "Reading the model.";

            if (modelEmpty) return NoPlotsInTheModel + " Open the model you meant and press Refresh.";

            if (!prefixPicked)
            {
                return "Pick a prefix in step 1. From and To fill themselves with the plots under "
                    + "it, and the grid follows.";
            }

            return "No plots in that range. Widen From and To in step 1.";
        }

        /// <summary>
        /// The status line after one cell is clicked.
        /// </summary>
        public static string MarkedLine(int marked)
        {
            return marked + " marked. Marking records intent and changes nothing until Run.";
        }

        /// <param name="readOnce">Whether the model has been read at all.</param>
        /// <param name="plotsInModel">Every plot the model holds, which is what step 1 offers.</param>
        /// <param name="first">The first plot of the range, empty when none is picked.</param>
        /// <param name="last">The last plot of the range.</param>
        /// <param name="plotsInRange">How many plots that range covers.</param>
        /// <param name="plotsTicked">How many of those are still ticked.</param>
        /// <param name="typesTicked">How many view types are ticked.</param>
        /// <param name="typesInModel">How many the model holds, plus any added by hand.</param>
        /// <param name="marked">How many cells are marked.</param>
        /// <param name="titleBlockTypes">How many title block types the model holds. A sheet is
        /// created with one, so none means no sheet can be made at all.</param>
        /// <param name="sheetsDescribed">How many sheet definitions the user has added.</param>
        /// <param name="sheetsAsked">How many sheets a run would make, meaning every row of
        /// every usable definition that has its name and its number.</param>
        /// <param name="sheetsIncomplete">How many definitions are missing a title block.</param>
        /// <param name="rowsIncomplete">How many rows across every definition are still short
        /// of a name or a number.</param>
        /// <param name="plan">What a run would make right now.</param>
        public static PanelSteps Of(
            bool readOnce,
            int plotsInModel,
            string first,
            string last,
            int plotsInRange,
            int plotsTicked,
            int typesTicked,
            int typesInModel,
            int marked,
            int titleBlockTypes,
            int sheetsDescribed,
            int sheetsAsked,
            int sheetsIncomplete,
            int rowsIncomplete,
            RunPlan plan)
        {
            first = first ?? string.Empty;
            last = last ?? string.Empty;

            bool haveARange = first.Length > 0 && last.Length > 0 && plotsInRange > 0;

            // Step 1 has to be usable before anything under it can be. Reading a range off the
            // arguments alone let every step below open on a panel that had read no model at
            // all, because the range fields still held what the last one had.
            bool plotsDone = readOnce && plotsInModel > 0 && haveARange && plotsTicked > 0;

            var steps = new List<StepState>
            {
                Plots(readOnce, plotsInModel, first, last, plotsInRange, plotsTicked, plotsDone),
                ViewTypes(plotsDone, typesTicked, typesInModel),
                Mark(plotsDone, typesTicked, marked),
                Sheets(plotsDone, titleBlockTypes, sheetsDescribed, sheetsAsked),
                Run(marked, sheetsAsked, sheetsIncomplete, rowsIncomplete, plan)
            };

            return new PanelSteps(steps);
        }

        private static StepState Plots(
            bool readOnce, int plotsInModel, string first, string last,
            int plotsInRange, int plotsTicked, bool done)
        {
            if (!readOnce)
            {
                return new StepState(PanelStep.Plots, "PLOTS", string.Empty, false,
                    "Nothing has been read yet. Press Refresh at the top.", false);
            }

            if (plotsInModel == 0)
            {
                return new StepState(PanelStep.Plots, "PLOTS", string.Empty, false,
                    NoPlotsInTheModel, false);
            }

            if (first.Length == 0 || last.Length == 0)
            {
                return new StepState(PanelStep.Plots, "PLOTS",
                    plotsInModel + " in the model, none picked", true, string.Empty, false);
            }

            return new StepState(PanelStep.Plots, "PLOTS",
                first + " to " + last + ", " + plotsTicked + " of " + plotsInRange + " ticked",
                true, string.Empty, done);
        }

        private static StepState ViewTypes(bool plotsDone, int ticked, int inModel)
        {
            if (!plotsDone)
            {
                return new StepState(PanelStep.ViewTypes, "VIEW TYPES", string.Empty, false,
                    "Pick a plot range and tick at least one plot first.", false);
            }

            return new StepState(PanelStep.ViewTypes, "VIEW TYPES",
                ticked + " of " + inModel + " ticked", true, string.Empty, ticked > 0);
        }

        private static StepState Mark(bool plotsDone, int typesTicked, int marked)
        {
            if (!plotsDone)
            {
                return new StepState(PanelStep.Mark, "MARK", string.Empty, false,
                    "Pick a plot range and tick at least one plot first.", false);
            }

            if (typesTicked == 0)
            {
                return new StepState(PanelStep.Mark, "MARK", string.Empty, false,
                    "Tick at least one view type in step 2. The grid has no columns until you "
                    + "do.", false);
            }

            return new StepState(PanelStep.Mark, "MARK",
                marked == 0 ? "nothing marked" : marked == 1 ? "1 marked" : marked + " marked",
                true, string.Empty, marked > 0);
        }

        private static StepState Sheets(
            bool plotsDone, int titleBlockTypes, int described, int asked)
        {
            if (!plotsDone)
            {
                return new StepState(PanelStep.Sheets, "SHEETS", string.Empty, false,
                    "Pick a plot range and tick at least one plot first.", false);
            }

            if (titleBlockTypes == 0)
            {
                return new StepState(PanelStep.Sheets, "SHEETS", string.Empty, false,
                    "This model holds no title block types, so there is nothing to make a sheet "
                    + "with.", false);
            }

            if (described == 0)
            {
                return new StepState(PanelStep.Sheets, "SHEETS",
                    "none added", true, string.Empty, false);
            }

            string sheets = described == 1 ? "1 described" : described + " described";
            string making = asked == 0
                ? "none to make yet"
                : asked == 1 ? "1 to make" : asked + " to make";

            return new StepState(PanelStep.Sheets, "SHEETS",
                sheets + ", " + making, true, string.Empty, asked > 0);
        }

        private static StepState Run(
            int marked, int sheetsAsked, int sheetsIncomplete, int rowsIncomplete, RunPlan plan)
        {
            if (marked == 0 && sheetsAsked == 0)
            {
                return new StepState(PanelStep.Run, "RUN", string.Empty, false,
                    WhyNothingToRun(sheetsIncomplete, rowsIncomplete), false);
            }

            string counts = plan == null ? "nothing to make" : plan.CountsInWords();

            return new StepState(PanelStep.Run, "RUN", counts, true, string.Empty, false);
        }

        /// <summary>
        /// Why Run is shut, naming what is unfinished in step 4 when something is. It used to
        /// know only about a definition missing its title block, so a sheet described with a
        /// title block and its views, whose rows still lacked names, was told to add a sheet.
        /// </summary>
        private static string WhyNothingToRun(int sheetsIncomplete, int rowsIncomplete)
        {
            var unfinished = new List<string>();

            if (sheetsIncomplete > 0)
            {
                unfinished.Add((sheetsIncomplete == 1 ? "1 sheet" : sheetsIncomplete + " sheets")
                    + " in step 4 still " + (sheetsIncomplete == 1 ? "needs" : "need")
                    + " a title block");
            }

            if (rowsIncomplete > 0)
            {
                unfinished.Add((rowsIncomplete == 1 ? "1 row" : rowsIncomplete + " rows")
                    + (sheetsIncomplete > 0 ? " still " : " in step 4 still ")
                    + (rowsIncomplete == 1 ? "needs" : "need") + " a name or a number");
            }

            if (unfinished.Count == 0)
            {
                return "Mark a cell in step 3, or add a sheet in step 4. Nothing is created "
                    + "until you do.";
            }

            return string.Join(", and ", unfinished.ToArray())
                + ", so no sheet can be made yet. Finish that in step 4, or mark a cell in "
                + "step 3.";
        }
    }
}
