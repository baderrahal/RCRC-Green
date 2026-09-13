using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What happened to one ticked template in a run that covered several. One row on the pane
    /// after the press, and one heading in the report.
    ///
    /// **Never one line for the run that hides which of six failed.** Bader's decision: a
    /// refusal on one template does not stop the others, so each of them carries its own answer
    /// and the run's accounting counts them rather than summarising them away.
    /// </summary>
    public sealed class TemplateOutcome
    {
        private TemplateOutcome(
            KpiTemplate template, IReadOnlyList<string> plots, int workbooks, string outputPath, string why)
        {
            if (template == null) throw new ArgumentNullException("template");

            Template = template;
            Plots = plots ?? new List<string>();
            Workbooks = workbooks;
            OutputPath = outputPath ?? string.Empty;
            Why = why ?? string.Empty;
        }

        /// <summary>
        /// How many of this template's plots really wrote a workbook.
        ///
        /// **A template is not refused because one of its plots was.** The first per plot run
        /// read MOSQUES: Nothing was written. 20 of 21 plots wrote a workbook, beside twenty
        /// workbooks that existed on disk. The row counted what the run set out to do and the
        /// sentence beside it counted what happened, which is two records of one fact inside one
        /// line. This is the count, and every word the row says is read off it.
        /// </summary>
        public int Workbooks { get; }

        /// <summary>
        /// Every plot of this template wrote its workbook.
        /// </summary>
        public static TemplateOutcome Wrote(
            KpiTemplate template, IReadOnlyList<string> plots, int workbooks, string where)
        {
            return new TemplateOutcome(template, plots, workbooks, where, string.Empty);
        }

        /// <summary>
        /// Some wrote and some did not. The count is what happened and the reason names the ones
        /// that did not, because a count alone sends somebody to the report to find out which.
        /// </summary>
        public static TemplateOutcome WroteSome(
            KpiTemplate template, IReadOnlyList<string> plots, int workbooks, string where, string why)
        {
            return new TemplateOutcome(template, plots, workbooks, where, why);
        }

        /// <summary>
        /// **NOTHING at all was written for this template**, which is the one case the word
        /// refused still fits.
        /// </summary>
        public static TemplateOutcome Refused(KpiTemplate template, IReadOnlyList<string> plots, string why)
        {
            return new TemplateOutcome(template, plots, 0, string.Empty, why);
        }

        /// <summary>
        /// A ticked template no ticked plot belongs to. It is not a refusal and not a failure:
        /// it wrote nothing because there was nothing to write, and it stays ticked and listed.
        /// </summary>
        public static TemplateOutcome NothingToWrite(KpiTemplate template, string why)
        {
            return new TemplateOutcome(template, new List<string>(), 0, string.Empty, why);
        }

        public KpiTemplate Template { get; }

        public IReadOnlyList<string> Plots { get; }

        /// <summary>
        /// **True when ANY of this template's plots wrote a workbook**, because twenty workbooks
        /// on disk are twenty workbooks whatever happened to the twenty first plot.
        /// </summary>
        public bool Written
        {
            get { return Workbooks > 0; }
        }

        /// <summary>
        /// Some wrote and some did not, which is neither of the two words that used to be the
        /// only choices this row had.
        /// </summary>
        public bool WroteSomeOfThem
        {
            get { return Workbooks > 0 && Workbooks < Plots.Count; }
        }

        public string OutputPath { get; }

        /// <summary>
        /// Empty on a workbook that was written. On one that was not, never empty, because
        /// silence after a press reads as success.
        /// </summary>
        public string Why { get; }

        public bool HadPlots
        {
            get { return Plots.Count > 0; }
        }

        /// <summary>
        /// Refused means it had plots and still wrote nothing. Nothing to write is the other
        /// half and is counted apart from it.
        /// </summary>
        public bool WasRefused
        {
            get { return !Written && HadPlots; }
        }
    }

    /// <summary>
    /// What happened to ONE PLOT, which is now one workbook and one folder.
    ///
    /// **A checklist is one plot.** The team files them that way, so a press over 20 mosque
    /// plots makes 20 folders and 20 workbooks rather than one file holding 20 plots added
    /// together. This is the row the report prints per plot and the row the accounting counts.
    /// </summary>
    public sealed class PlotOutcome
    {
        private PlotOutcome(
            string plotId, KpiTemplate template, PlotWorkbookPath where,
            bool written, bool folderMade, string why)
        {
            if (string.IsNullOrWhiteSpace(plotId)) throw new ArgumentNullException("plotId");

            PlotId = plotId.Trim();
            Template = template;
            Where = where ?? PlotWorkbookPath.Refused(string.Empty);
            Written = written;
            FolderMade = folderMade;
            Why = why ?? string.Empty;
        }

        public static PlotOutcome Wrote(string plotId, KpiTemplate template, PlotWorkbookPath where)
        {
            return new PlotOutcome(plotId, template, where, true, true, string.Empty);
        }

        /// <summary>
        /// The plot was read and its workbook was not written. The folder may or may not have
        /// been made by then, so which it was travels rather than being assumed.
        /// </summary>
        public static PlotOutcome WroteNothing(
            string plotId, KpiTemplate template, PlotWorkbookPath where, bool folderMade, string why)
        {
            return new PlotOutcome(plotId, template, where, false, folderMade, why);
        }

        public string PlotId { get; }

        /// <summary>
        /// Null where no template took the plot at all, which is a plot neither route placed.
        /// </summary>
        public KpiTemplate Template { get; }

        public PlotWorkbookPath Where { get; }

        public bool Written { get; }

        /// <summary>
        /// Whether this plot's own folder exists after the press. **A folder is created where it
        /// does not exist and NEVER deleted**, so a refused write can still leave one behind and
        /// the count says so rather than implying the tree was tidied up.
        /// </summary>
        public bool FolderMade { get; }

        /// <summary>
        /// Empty on a plot that was written. Never empty on one that was not.
        /// </summary>
        public string Why { get; }

        public string TemplateName
        {
            get { return Template == null ? string.Empty : Template.Name; }
        }
    }

    /// <summary>
    /// One press of Create over several templates: one run per template written exactly as one
    /// is written today, and the accounting that sits above all of them.
    ///
    /// **NOTHING ABOUT THE PER TEMPLATE LOGIC CHANGES.** Each <see cref="KpiCreateRun"/> here is
    /// the same object one press used to produce, filled by the same map, the same tree lists
    /// read off its own file, the same Street Design rule, the same group rules, the same canopy
    /// check, the same read back, the same cache fix and the same alias list. A template is a
    /// template whether one or six are ticked, and the report prints each of them through the
    /// section writer that was already tested.
    /// </summary>
    public sealed class KpiCreateRunSet
    {
        public KpiCreateRunSet(
            string documentTitle,
            TemplateSplit split,
            IReadOnlyList<KpiCreateRun> runs,
            IReadOnlyList<TemplateOutcome> outcomes,
            RunTiming timing = null,
            IReadOnlyList<PlotOutcome> plotOutcomes = null,
            StreetReferenceFile streets = null)
        {
            if (split == null) throw new ArgumentNullException("split");

            DocumentTitle = documentTitle ?? string.Empty;
            Split = split;
            Runs = runs ?? new List<KpiCreateRun>();
            Outcomes = outcomes ?? new List<TemplateOutcome>();
            Timing = timing ?? RunTiming.NotTimed;
            PlotOutcomes = plotOutcomes ?? new List<PlotOutcome>();
            Streets = streets ?? StreetReferenceFile.NotSet;
        }

        /// <summary>
        /// One per TICKED plot, whether it wrote a workbook or not. **This is the accounting the
        /// round asks for**, and it is per plot because a checklist is per plot now.
        /// </summary>
        public IReadOnlyList<PlotOutcome> PlotOutcomes { get; }

        /// <summary>
        /// What the street reference file was, so the report says it once at the top rather than
        /// once under each of 78 street plots.
        /// </summary>
        public StreetReferenceFile Streets { get; }

        public int PlotsTicked
        {
            get { return PlotOutcomes.Count; }
        }

        /// <summary>
        /// How many plot folders exist after the press, counted off the outcomes rather than off
        /// the file system, because a folder somebody else made is not this run's doing.
        /// </summary>
        public int FoldersMade
        {
            get { return PlotOutcomes.Count(one => one.FolderMade); }
        }

        public int WorkbooksWritten
        {
            get { return PlotOutcomes.Count(one => one.Written); }
        }

        public int PlotsThatWroteNothing
        {
            get { return PlotOutcomes.Count(one => !one.Written); }
        }

        /// <summary>
        /// Written plus wrote nothing must equal ticked. **The folders are counted beside them
        /// and are deliberately NOT in the sum**, because a folder can exist for a plot whose
        /// workbook was refused and one made by an earlier press is not made twice.
        /// </summary>
        public bool PlotCountsAddUp
        {
            get { return WorkbooksWritten + PlotsThatWroteNothing == PlotsTicked; }
        }

        public string DocumentTitle { get; }

        public TemplateSplit Split { get; }

        /// <summary>
        /// One per template that had plots to read. A template with none produces no run, only
        /// an outcome saying so.
        /// </summary>
        public IReadOnlyList<KpiCreateRun> Runs { get; }

        /// <summary>
        /// One per TICKED template, whether it wrote, was refused or had nothing to write, so
        /// the four counts below can be held against the number ticked.
        /// </summary>
        public IReadOnlyList<TemplateOutcome> Outcomes { get; }

        public RunTiming Timing { get; }

        public int TemplatesTicked
        {
            get { return Outcomes.Count; }
        }

        public int TemplatesWritten
        {
            get { return Outcomes.Count(one => one.Written); }
        }

        public int TemplatesRefused
        {
            get { return Outcomes.Count(one => one.WasRefused); }
        }

        public int TemplatesWithNothingToWrite
        {
            get { return Outcomes.Count(one => !one.Written && !one.HadPlots); }
        }

        /// <summary>
        /// The four must add up to the number ticked. **They are counted from one list and
        /// checked against its own length**, so a template that fell out of every branch is a
        /// refusal rather than a row nobody printed.
        /// </summary>
        public bool CountsAddUp
        {
            get { return TemplatesWritten + TemplatesRefused + TemplatesWithNothingToWrite == TemplatesTicked; }
        }

        public bool Wrote
        {
            get { return TemplatesWritten > 0; }
        }

        /// <summary>
        /// Every plot that went into a workbook, over the whole run. It is what the double count
        /// check is really about, and the report prints its length beside the ticked count.
        /// </summary>
        public IReadOnlyList<string> PlotsWritten
        {
            get
            {
                return Outcomes
                    .Where(one => one.Written)
                    .SelectMany(one => one.Plots)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
        }

        public IReadOnlyList<string> Refusals
        {
            get
            {
                var held = new List<string>(Split.Refusals);
                if (!PlotCountsAddUp)
                {
                    held.Add(PlotsTicked + " plots were ticked and "
                        + (WorkbooksWritten + PlotsThatWroteNothing)
                        + " were accounted for, which is a bug in this tool.");
                }

                if (!CountsAddUp)
                {
                    held.Add(TemplatesTicked + " templates were ticked and "
                        + (TemplatesWritten + TemplatesRefused + TemplatesWithNothingToWrite)
                        + " were accounted for, which is a bug in this tool.");
                }

                return held;
            }
        }
    }
}
