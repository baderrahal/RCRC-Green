using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// The four things a person does with this pane, in the order they do them. Bader's layout
    /// of 17 September.
    /// </summary>
    public enum KpiStep
    {
        Setup = 1,
        Read = 2,

        // Called Tick until the pane's redesign, when the step took the team's word, Plots.
        Plots = 3,
        Create = 4
    }

    /// <summary>
    /// One step: what it is called, whether it can be worked on yet, why not where it cannot,
    /// and whether enough has been done in it to move on.
    ///
    /// **A STEP CAN ALWAYS BE OPENED, AND AN UNUSABLE ONE SHOWS ITS REASON IN PLACE OF ITS
    /// CONTROLS.** A cell that does nothing and says nothing is worse than one that is not
    /// there, which is the rule the Drawing Sheet's own steps already carry.
    /// </summary>
    public sealed class KpiStepState
    {
        internal KpiStepState(
            KpiStep step, string title, string summary, bool usable, string whyNot, bool done)
        {
            Step = step;
            Title = title ?? string.Empty;
            Summary = summary ?? string.Empty;
            Usable = usable;
            WhyNot = whyNot ?? string.Empty;
            Done = done;
        }

        public KpiStep Step { get; }

        public int Number
        {
            get { return (int)Step; }
        }

        public string Title { get; }

        /// <summary>
        /// What the step says beside its name, so the state of the whole pane reads off the bar
        /// without opening anything. Empty where there is nothing to say yet.
        /// </summary>
        public string Summary { get; }

        public bool Usable { get; }

        /// <summary>
        /// One line in the words the user needs, empty where the step is usable.
        /// </summary>
        public string WhyNot { get; }

        /// <summary>
        /// Whether enough has been done here to move on. It is what marks the step and what
        /// opens the next one.
        /// </summary>
        public bool Done { get; }

        /// <summary>
        /// **BADER ASKED FOR A TICK AND THIS REPOSITORY'S OWN HOOK REFUSES ONE.**
        /// `.claude/hooks/writing-check.sh` line 47 builds its emoji pattern from four
        /// ranges, and the second of them runs from code point 2600 to 27BF in hex. The
        /// tick character is 2713, which sits inside it, so a commit carrying one is
        /// refused. The mark is the word instead, and the tick is a question for Bader in
        /// the log rather than a character quietly left out.
        ///
        /// **THIS COMMENT NAMES THOSE CODE POINTS AND CARRIES NONE OF THEM**, because a
        /// file refused by the rule its own comment describes is the shape this repository
        /// has already paid for once, when a commit was refused by its own message for
        /// naming the marker it introduced.
        /// </summary>
        public string Mark
        {
            get { return Done ? DoneMark : string.Empty; }
        }

        public const string DoneMark = "done";

        /// <summary>
        /// What the step's cell in the bar reads: the number, the name, and the mark where it is
        /// done. Built here so four cells are spaced one way rather than four.
        ///
        /// **THE BAR IS FOUR CELLS ACROSS A PANE ABOUT 300 PIXELS WIDE**, so this carries no
        /// summary and no mark. `3 Tick done` in a cell about 70 pixels wide is trimmed to
        /// nothing useful, and a finished step would read SHORTER than an unfinished one, which
        /// is the opposite of what a mark is for. The summary is its own line under the bar and
        /// <see cref="Mark"/> is drawn under the name.
        /// </summary>
        public string Cell
        {
            get { return Number.ToString(CultureInfo.InvariantCulture) + " " + Title; }
        }

        /// <summary>
        /// What the step's cell says when the pointer rests on it, at most ten words. A step
        /// that cannot be worked on gives its reason, one that can gives its summary.
        /// </summary>
        public string Tooltip
        {
            get
            {
                if (!Usable) return Title + ": " + WhyNot;
                if (Summary.Length == 0) return Title + ": " + NothingYet;

                return Title + ": " + Summary + (Done ? ", done." : ".");
            }
        }

        public const string NothingYet = "nothing done yet.";
    }

    /// <summary>
    /// The state of all four steps, worked out from plain values so a test can reach every one
    /// of them. **The pane draws these and decides none of them**, which is the rule
    /// `PanelSteps` already carries for the other pane: a summary written beside the control
    /// that shows it is two records of one fact, and this repository has paid for that shape
    /// eight times.
    ///
    /// It is modelled on `PanelSteps` in Core/DrawingSheet and is not a call across the fence
    /// and not a move of it to Shared. The two panes' steps are different steps with different
    /// meanings, and whether one shape should serve both is a Shared round for Bader, recorded
    /// in the log the same way `ScrollMemory` was.
    /// </summary>
    public sealed class KpiSteps
    {
        private readonly IReadOnlyList<KpiStepState> _steps;

        private KpiSteps(IReadOnlyList<KpiStepState> steps)
        {
            _steps = steps;
        }

        public IReadOnlyList<KpiStepState> All
        {
            get { return _steps; }
        }

        public KpiStepState For(KpiStep step)
        {
            KpiStepState found = _steps.FirstOrDefault(one => one.Step == step);
            if (found == null) throw new ArgumentException("No such step.", "step");
            return found;
        }

        /// <summary>
        /// The first step worth working on: the first one that can be worked on and is not
        /// already done. Everything done lands on Create, because that is where the press is.
        /// </summary>
        public KpiStep FirstUnfinished
        {
            get
            {
                KpiStepState found = _steps.FirstOrDefault(one => one.Usable && !one.Done);
                return found == null ? KpiStep.Create : found.Step;
            }
        }

        public const string TemplatesFolderNotSet = "the templates folder is not set";

        public const string OutputFolderNotSet = "the output folder is not set";

        public const string FormsFolderNotSet =
            "the forms folder is not set, so no PDF will be written";

        public const string StreetReferenceNotSet =
            "the street reference file is not set, so every street plot's road width and total "
            + "length will be left empty";

        public const string PlotListNotSet =
            "the plot list file is not set, so Plot list cannot be pressed and the report "
            + "prints no plot list section";

        public const string EverythingIsSet = "Every folder and file this pane is pointed at is set.";

        public const string NoModelOpen = "no model is open";

        public const string NothingHasBeenRead = "this model's plots have not been read yet";

        public const string NoTemplateTicked = "no workbook is ticked";

        public const string NoPlotTicked = "no plot is ticked";

        /// <summary>
        /// **THE TWO THAT REFUSE AND THE THREE THAT ARE NOTES.** Only the templates folder and
        /// the output folder stop a press: `TemplateWords.NoFolder` ends the workbook list
        /// before it starts and `CreateWords.CannotCreate` refuses on the output folder. The
        /// forms folder, the street reference file and the plot list file are each recorded in
        /// `kpi-rules.md` as a NOTE AND NEVER A REFUSAL, so they are named here and gate
        /// nothing. Gating on one of them would hold back a press the tool is willing to make.
        /// </summary>
        public static IReadOnlyList<string> NotSet(
            bool templatesFolder,
            bool outputFolder,
            bool formsFolder,
            bool streetReference,
            bool plotList)
        {
            var said = new List<string>();

            if (!templatesFolder) said.Add(TemplatesFolderNotSet);
            if (!outputFolder) said.Add(OutputFolderNotSet);
            if (!formsFolder) said.Add(FormsFolderNotSet);
            if (!streetReference) said.Add(StreetReferenceNotSet);
            if (!plotList) said.Add(PlotListNotSet);

            return said;
        }

        /// <summary>
        /// The one line step 1 carries, naming everything still not set, or saying everything is.
        /// **A line about nothing is one the team reads past on every other press**, so the
        /// clean case is one short sentence rather than five saying nothing is wrong.
        /// </summary>
        public static string StillNotSet(IReadOnlyList<string> notSet)
        {
            if (notSet == null || notSet.Count == 0) return EverythingIsSet;

            return "Not set: " + string.Join(". ", notSet.ToArray()) + ".";
        }

        /// <summary>
        /// The four steps, worked out from what the pane holds.
        /// </summary>
        /// <param name="templatesFolder">Whether the templates folder is set.</param>
        /// <param name="outputFolder">Whether the output folder is set.</param>
        /// <param name="formsFolder">Whether the forms folder is set.</param>
        /// <param name="streetReference">Whether the street reference file is set.</param>
        /// <param name="plotList">Whether the plot list file is set.</param>
        /// <param name="modelOpen">Whether a model is open, off the live document.</param>
        /// <param name="plotsRead">How many plots the read found, or null where nothing read.</param>
        /// <param name="templatesTicked">How many workbook rows are ticked with a settled template.</param>
        /// <param name="plotsTicked">How many plots are ticked.</param>
        /// <param name="pressed">Whether a press has finished and its results are being shown.</param>
        public static KpiSteps Of(
            bool templatesFolder,
            bool outputFolder,
            bool formsFolder,
            bool streetReference,
            bool plotList,
            bool modelOpen,
            int? plotsRead,
            int templatesTicked,
            int plotsTicked,
            bool pressed)
        {
            IReadOnlyList<string> notSet = NotSet(
                templatesFolder, outputFolder, formsFolder, streetReference, plotList);

            // **SETUP IS ALWAYS USABLE**, because it is where every reason below is fixed. A
            // pane whose first step is shut has nowhere to say what is wrong.
            bool setupDone = templatesFolder && outputFolder;

            var setup = new KpiStepState(
                KpiStep.Setup,
                "Setup",
                SetupSummary(notSet),
                true,
                string.Empty,
                setupDone);

            string readWhyNot = setupDone
                ? string.Empty
                : "Set " + WhichOfTheTwo(templatesFolder, outputFolder) + " first.";

            bool readDone = plotsRead.HasValue;

            var read = new KpiStepState(
                KpiStep.Read,
                "Read",
                ReadSummary(modelOpen, plotsRead),
                setupDone,
                readWhyNot,
                readDone);

            // **A STEP THAT CANNOT BE WORKED ON CANNOT BE DONE.** A model closing leaves the
            // ticks standing and takes the read away, and without this the bar read
            // `3 Tick done` beside a step saying the model has not been read, with step 4
            // open on the strength of it. A mark is a claim about a step somebody can use.
            var tick = new KpiStepState(
                KpiStep.Plots,
                "Plots",
                TickSummary(templatesTicked, plotsTicked),
                readDone,
                readDone ? string.Empty : ReadFirst,
                readDone && templatesTicked > 0 && plotsTicked > 0);

            // **ONE REASON CREATE CANNOT RUN, AND THIS IS WHERE IT IS WORKED OUT.** Step 4's own
            // reason and the line under the Create button were two rules for one question, and
            // the pane worked it out a third time for a tooltip. Every one of them reads this.
            string blocked = CreateBlocked.Reason(
                modelOpen, templatesFolder, outputFolder, readDone,
                templatesTicked > 0, plotsTicked > 0, string.Empty);

            var create = new KpiStepState(
                KpiStep.Create,
                "Create",
                pressed ? "pressed" : string.Empty,
                tick.Done,
                tick.Done ? string.Empty : blocked,
                pressed);

            // Step 3's own lack, which is a tick and nothing else. The setup reasons in front of
            // it in CreateBlocked's order are step 1's to say, so step 3 never names a folder.
            string plotsLack = templatesTicked == 0
                ? CreateBlocked.NoAssetType
                : (plotsTicked == 0 ? CreateBlocked.NoPlot : string.Empty);

            return new KpiSteps(new List<KpiStepState> { setup, read, tick, create })
            {
                CreateBlockedBy = blocked,
                PlotsLack = plotsLack
            };
        }

        /// <summary>
        /// What step 3 itself still lacks, no asset type or no plot, or empty where both are
        /// ticked. It is the Next button's reason on step 3.
        /// </summary>
        public string PlotsLack { get; private set; } = string.Empty;

        /// <summary>
        /// The one reason a press cannot run, in <see cref="CreateBlocked"/>'s order, or empty
        /// where it can. Step 4's WhyNot is this same string whenever step 4 is shut.
        /// </summary>
        public string CreateBlockedBy { get; private set; } = string.Empty;

        public const string ReadFirst = "Read the model first.";

        private static string WhichOfTheTwo(bool templatesFolder, bool outputFolder)
        {
            if (!templatesFolder && !outputFolder) return "the templates and output folders";
            return templatesFolder ? "the output folder" : "the templates folder";
        }

        /// <summary>
        /// The status line under Next on step 1: all set, or how many of the five are not.
        /// **All five count, the three that are notes included**, because the line answers
        /// what is left to point at rather than what stops a press.
        /// </summary>
        public static string SetupStatus(IReadOnlyList<string> notSet)
        {
            int left = notSet == null ? 0 : notSet.Count;
            return left == 0 ? "All set" : left.ToString(CultureInfo.InvariantCulture) + " to set";
        }

        /// <summary>
        /// **SHORT, BECAUSE THE STEP ITSELF PRINTS THE WHOLE SENTENCE.** The header carried
        /// `StillNotSet` and so did the foot of the step, which is the same words twice on one
        /// screen, the shape a run already printed four lines twice over.
        /// </summary>
        private static string SetupSummary(IReadOnlyList<string> notSet)
        {
            if (notSet == null || notSet.Count == 0) return "all set";

            return notSet.Count.ToString(CultureInfo.InvariantCulture) + " not set";
        }

        private static string ReadSummary(bool modelOpen, int? plotsRead)
        {
            if (plotsRead.HasValue) return Plots(plotsRead.Value) + " read";
            return modelOpen ? "not read" : NoModelOpen;
        }

        private static string TickSummary(int templatesTicked, int plotsTicked)
        {
            if (templatesTicked == 0 && plotsTicked == 0) return "nothing ticked";

            return AssetTypes(templatesTicked) + ", " + Plots(plotsTicked);
        }

        private static string Plots(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + (howMany == 1 ? " plot" : " plots");
        }

        private static string AssetTypes(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture)
                + (howMany == 1 ? " asset type" : " asset types");
        }
    }
}
