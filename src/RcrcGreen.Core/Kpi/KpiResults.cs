using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the pane shows after a press, in place of step 4's controls.
    ///
    /// **IT IS BUILT OFF `PlotListRows`, THE SAME ROWS THE REPORT'S OWN THE PLOT LIST IS BUILT
    /// FROM.** That is the whole point of it. A second walk over the outcomes beside the button,
    /// counting ready its own way, is two records of one fact, and this pane and that report
    /// would disagree the first time either rule moved. The press reads no differently and the
    /// report prints no differently for this existing.
    ///
    /// **THE REASONS ARE THE REPORT'S OWN WORDS**, word for word, because a pane saying a plot
    /// failed for one reason and a report saying another sends somebody looking for a second
    /// fault that is not there.
    /// </summary>
    public sealed class KpiResults
    {
        private KpiResults(bool listWasSet, bool listWasRead, IReadOnlyList<PlotListRow> rows)
        {
            ListWasSet = listWasSet;
            ListWasRead = listWasRead;
            Rows = rows ?? new List<PlotListRow>();
        }

        /// <summary>
        /// Whether the press had a plot list file to read. **READY IS A QUESTION ABOUT THE
        /// TEAM'S LIST**, so a press with no list set counts nothing rather than counting nought
        /// of nought, which is the rule the report's own `ready:` count already follows.
        /// </summary>
        public bool ListWasSet { get; }

        /// <summary>
        /// Whether the file the press was pointed at could be read. **A FILE THAT COULD NOT BE
        /// OPENED AND A FILE NOBODY CHOSE TICK EXACTLY THE SAME NOTHING**, which is the rule
        /// `PlotListFile` already carries, and the panel said the same sentence for both while
        /// the report printed the file's own reason.
        /// </summary>
        public bool ListWasRead { get; }

        /// <summary>Every row of the team's list, in the file's order.</summary>
        public IReadOnlyList<PlotListRow> Rows { get; }

        public int Ready
        {
            get { return Rows.Count(one => one.Ready); }
        }

        public int NotReady
        {
            get { return Rows.Count(one => !one.Ready); }
        }

        /// <summary>
        /// The plots that are not ready, in the list's own order, each with its plot, its UID2
        /// and its reason. **Every one of them**, because a results panel that showed the first
        /// few would be a panel somebody has to open the report behind anyway.
        /// </summary>
        public IReadOnlyList<PlotListRow> NotReadyRows
        {
            get { return Rows.Where(one => !one.Ready).ToList(); }
        }

        public const string NoPlotList =
            "No plot list file was set for this press, so there is no list to count ready "
            + "against. The report says what each ticked plot did.";

        public const string ListNotRead =
            "The plot list file was set and could not be read, so there is no list to count "
            + "ready against. The report names the file and says why.";

        public const string ListNamesNoPlot =
            "The plot list file was read and names no plot, so there is nothing to count ready "
            + "against.";

        public const string EveryPlotIsReady = "Every plot on the list is ready.";

        public const string ReadyHeading = "ready";

        public const string NotReadyHeading = "not ready";

        /// <summary>
        /// The two counts as one line, for a pane too narrow to be sure two boxes sit side by
        /// side. The pane draws them as two boxes and this is what each box says.
        /// </summary>
        public string CountsInWords
        {
            get
            {
                if (!ListWasSet) return NoPlotList;
                if (!ListWasRead) return ListNotRead;
                if (Rows.Count == 0) return ListNamesNoPlot;

                return ReadyHeading + " " + Ready.ToString(CultureInfo.InvariantCulture)
                    + ", " + NotReadyHeading + " "
                    + NotReady.ToString(CultureInfo.InvariantCulture)
                    + ", of " + Rows.Count.ToString(CultureInfo.InvariantCulture)
                    + (Rows.Count == 1 ? " plot on the list" : " plots on the list");
            }
        }

        /// <summary>
        /// The line above the list of plots that are not ready, or the one saying there are
        /// none. **A press where every plot is ready says so**, because a block that disappears
        /// when there is nothing to report reads the same as one nobody wrote.
        /// </summary>
        public string NotReadyInWords
        {
            get
            {
                // **A LIST WITH NO PLOT ON IT IS NOT A LIST WHERE EVERY PLOT IS READY.** An
                // empty list counted nought not ready and said every plot on it is ready, which
                // is a sentence about nothing that reads as a clean press.
                if (!ListWasSet || !ListWasRead || Rows.Count == 0) return string.Empty;
                if (NotReady == 0) return EveryPlotIsReady;

                return NotReady.ToString(CultureInfo.InvariantCulture)
                    + (NotReady == 1 ? " plot is" : " plots are")
                    + " not ready to send.";
            }
        }

        /// <summary>
        /// The result screen's `Ready to send  33 of 154`, the two values apart so the pane
        /// sets one on the left and one on the right. Empty where no list was read, because
        /// ready is a question about the team's list.
        /// </summary>
        public string ReadyOf
        {
            get
            {
                if (!ListWasSet || !ListWasRead || Rows.Count == 0) return string.Empty;
                return KpiPaneWords.OfCount(Ready, Rows.Count);
            }
        }

        public static KpiResults Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            PlotListRead list = set.PlotList;

            return new KpiResults(
                list != null && list.Set,
                list != null && list.Set && list.Read,
                PlotListRows.Of(set));
        }
    }

    /// <summary>
    /// One ticked plot with no workbook, as the result screen lists it.
    /// </summary>
    public sealed class FailedPlot
    {
        internal FailedPlot(string plotId, NotWrittenKind kind, string why)
        {
            PlotId = plotId ?? string.Empty;
            Kind = kind;
            Why = why ?? string.Empty;
        }

        public string PlotId { get; }

        public NotWrittenKind Kind { get; }

        /// <summary>At most eight words a sentence, off the kind.</summary>
        public string Short
        {
            get { return FailedReasons.Short(Kind); }
        }

        /// <summary>The outcome's own reason in full, the line's tooltip.</summary>
        public string Why { get; }
    }

    /// <summary>
    /// Which banner the result opens with: green where every workbook and every expected PDF
    /// was written, a warning where only PDFs are missing, an error where workbooks are.
    /// </summary>
    public enum ResultBanner
    {
        AllCreated,
        PdfsNotWritten,
        WorkbooksNotCreated
    }

    /// <summary>
    /// One ticked plot whose PDF was expected and not written.
    /// </summary>
    public sealed class MissingPdf
    {
        internal MissingPdf(string plotId, string why)
        {
            PlotId = plotId ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public string PlotId { get; }

        public string Why { get; }
    }

    /// <summary>
    /// One ticked asset type's row in the result: how many of its plots wrote and how many did
    /// not. **One line for the run would hide which of six failed**, so every ticked type has a
    /// row, one with no plot of its own included.
    /// </summary>
    public sealed class AssetTypeResult
    {
        internal AssetTypeResult(string assetType, int created, int failed, bool nothingToWrite, bool unplaced = false)
        {
            AssetType = assetType ?? string.Empty;
            Created = created;
            Failed = failed;
            NothingToWrite = nothingToWrite;
            Unplaced = unplaced;
        }

        /// <summary>
        /// The row of plots no ticked asset type took, drawn muted. It is there so the rows
        /// always add up to Created and Failed.
        /// </summary>
        public bool Unplaced { get; }

        public const string NoTickedAssetType = "No ticked asset type";

        public string AssetType { get; }

        public int Created { get; }

        public int Failed { get; }

        public bool NothingToWrite { get; }

        public const string NothingToWriteWords = "Nothing to write";
    }

    /// <summary>
    /// What one press did, counted off its outcomes. **Created and Failed are the run set's
    /// own counts**, `WorkbooksWritten` and `PlotsThatWroteNothing`, which the report's run
    /// accounting prints, so the screen and the file cannot read two numbers for one press.
    /// </summary>
    public sealed class PressResult
    {
        private PressResult(
            int created, IReadOnlyList<FailedPlot> failed, IReadOnlyList<MissingPdf> pdfs,
            IReadOnlyList<AssetTypeResult> byType)
        {
            Created = created;
            FailedPlots = failed;
            PdfsNotWritten = pdfs;
            ByAssetType = byType;
        }

        public int Created { get; }

        public int Failed
        {
            get { return FailedPlots.Count; }
        }

        /// <summary>Every ticked plot that wrote nothing, in the order the press met them.</summary>
        public IReadOnlyList<FailedPlot> FailedPlots { get; }

        /// <summary>
        /// Every ticked plot with no PDF written, on a press that had a forms folder set. Empty
        /// on a press with none, where no PDF was expected.
        /// </summary>
        public IReadOnlyList<MissingPdf> PdfsNotWritten { get; }

        public IReadOnlyList<AssetTypeResult> ByAssetType { get; }

        public bool AllCreated
        {
            get { return Failed == 0; }
        }

        /// <summary>
        /// **THE GREEN BANNER IS ONLY FOR A PRESS THAT WROTE EVERYTHING IT WAS EXPECTED TO.** A
        /// press whose workbooks all landed and whose PDFs did not is not that press.
        /// </summary>
        public ResultBanner Kind
        {
            get
            {
                if (Failed > 0) return ResultBanner.WorkbooksNotCreated;
                return PdfsNotWritten.Count > 0 ? ResultBanner.PdfsNotWritten : ResultBanner.AllCreated;
            }
        }

        public string Banner
        {
            get
            {
                switch (Kind)
                {
                    case ResultBanner.WorkbooksNotCreated: return KpiPaneWords.NotCreated(Failed);
                    case ResultBanner.PdfsNotWritten: return KpiPaneWords.PdfsNotWritten(PdfsNotWritten.Count);
                    default: return KpiPaneWords.AllCreated;
                }
            }
        }

        public string Status
        {
            get { return KpiPaneWords.ResultStatus(Created, Failed); }
        }

        public static PressResult Of(KpiCreateRunSet set)
        {
            if (set == null) throw new ArgumentNullException("set");

            // **THE GLANCE'S OWN RULE**, a plot whose workbook was written and whose PDF was not,
            // with the PDF's own refusal. Gated on a forms folder having been set, because a
            // press with none set expected no PDF and the banner says nothing about one.
            IReadOnlyList<MissingPdf> pdfs = set.PdfsWanted
                ? MissingPdfs.Of(set.PlotOutcomes)
                : new List<MissingPdf>();

            var byType = new List<AssetTypeResult>();
            foreach (TemplateOutcome outcome in set.Outcomes)
            {
                List<PlotOutcome> mine = set.PlotOutcomes
                    .Where(one => ReferenceEquals(one.Template, outcome.Template))
                    .ToList();

                byType.Add(new AssetTypeResult(
                    AssetTypeNames.Display(outcome.Template),
                    mine.Count(one => one.Written),
                    mine.Count(one => !one.Written),
                    !outcome.HadPlots));
            }

            // **EVERY TICKED PLOT SITS IN ONE ROW**, so the rows add up to the two boxes. A plot
            // no ticked asset type took has no template of its own and goes in this one.
            List<PlotOutcome> nowhere = set.PlotOutcomes
                .Where(one => !set.Outcomes.Any(outcome => ReferenceEquals(outcome.Template, one.Template)))
                .ToList();
            if (nowhere.Count > 0)
            {
                byType.Add(new AssetTypeResult(
                    AssetTypeResult.NoTickedAssetType,
                    nowhere.Count(one => one.Written),
                    nowhere.Count(one => !one.Written),
                    false,
                    true));
            }

            List<FailedPlot> failed = set.PlotOutcomes
                .Where(one => !one.Written)
                .Select(one => new FailedPlot(one.PlotId, one.Kind, one.Why))
                .ToList();

            return new PressResult(set.WorkbooksWritten, failed, pdfs, byType);
        }

        /// <summary>The failed plots a retry can write, the ones whose kind is retryable.</summary>
        public IReadOnlyList<string> Retryable
        {
            get
            {
                return FailedPlots.Where(one => FailedReasons.Retryable(one.Kind)).Select(one => one.PlotId).ToList();
            }
        }

        /// <summary>The failed plots a retry would fail again, until something is changed.</summary>
        public int NeedAChange
        {
            get { return FailedPlots.Count(one => !FailedReasons.Retryable(one.Kind)); }
        }

        /// <summary>
        /// The result's status line: the two counts, or how many failed plots a retry leaves
        /// out because each needs a change first.
        /// </summary>
        public string StatusLine
        {
            get { return NeedAChange > 0 ? KpiPaneWords.NeedAChangeFirst(NeedAChange) : Status; }
        }
    }

    /// <summary>
    /// **ONE RULE FOR A MISSING PDF, READ BY THE REPORT'S GLANCE AND BY THE PANE.** A plot whose
    /// workbook was written and whose PDF was not, with the PDF's own refusal. A plot with no
    /// workbook is not counted here: its PDF is a consequence of that, which the failed list
    /// already names, and a plot with no PDF record at all is one no PDF step was reached for.
    /// </summary>
    public static class MissingPdfs
    {
        public static IReadOnlyList<MissingPdf> Of(IEnumerable<PlotOutcome> outcomes)
        {
            return (outcomes ?? Enumerable.Empty<PlotOutcome>())
                .Where(one => one != null && one.Pdf != null && !one.Pdf.Written && one.Written)
                .Select(one => new MissingPdf(one.PlotId, one.Pdf.Refusal))
                .ToList();
        }
    }
}
