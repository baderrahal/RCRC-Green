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
            KpiTemplate template, IReadOnlyList<string> plots, bool written, string outputPath, string why)
        {
            if (template == null) throw new ArgumentNullException("template");

            Template = template;
            Plots = plots ?? new List<string>();
            Written = written;
            OutputPath = outputPath ?? string.Empty;
            Why = why ?? string.Empty;
        }

        public static TemplateOutcome Wrote(KpiTemplate template, IReadOnlyList<string> plots, string outputPath)
        {
            return new TemplateOutcome(template, plots, true, outputPath, string.Empty);
        }

        public static TemplateOutcome Refused(KpiTemplate template, IReadOnlyList<string> plots, string why)
        {
            return new TemplateOutcome(template, plots, false, string.Empty, why);
        }

        /// <summary>
        /// A ticked template no ticked plot belongs to. It is not a refusal and not a failure:
        /// it wrote nothing because there was nothing to write, and it stays ticked and listed.
        /// </summary>
        public static TemplateOutcome NothingToWrite(KpiTemplate template, string why)
        {
            return new TemplateOutcome(template, new List<string>(), false, string.Empty, why);
        }

        public KpiTemplate Template { get; }

        public IReadOnlyList<string> Plots { get; }

        public bool Written { get; }

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
            RunTiming timing = null)
        {
            if (split == null) throw new ArgumentNullException("split");

            DocumentTitle = documentTitle ?? string.Empty;
            Split = split;
            Runs = runs ?? new List<KpiCreateRun>();
            Outcomes = outcomes ?? new List<TemplateOutcome>();
            Timing = timing ?? RunTiming.NotTimed;
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
