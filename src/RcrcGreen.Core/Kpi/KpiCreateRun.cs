using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// Everything one press of Create knew and everything it did, in plain values.
    ///
    /// The plan and the outcome are both on here and are different objects. The plan is what
    /// the fill set out to write. The outcome is what the patcher really landed, read back off
    /// the output file. Every count the report prints comes off the outcome, never off the
    /// plan, which is the shape the Drawing Sheet settled on after a report named four views
    /// as created and as not created in the same file.
    /// </summary>
    public sealed class KpiCreateRun
    {
        public KpiCreateRun(
            string documentTitle,
            KpiTemplate template,
            string templatePath,
            string outputPath,
            string componentParameter,
            string referenceParameter,
            string location,
            IEnumerable<PlotReading> readings,
            Reconciliation reconciliation,
            KpiCreatePlan plan,
            Totalled area,
            Totalled shrubs,
            Totalled lawn,
            AgreedValue component,
            AgreedValue reference,
            IEnumerable<MergedSpecies> merged,
            IEnumerable<SpeciesRow> ungrouped,
            PatchOutcome outcome,
            RunTiming timing,
            SpeciesList existingList = null,
            SpeciesList proposedList = null,
            ReadingsSource readingsSource = null,
            TemplateListing templatesListed = null)
        {
            if (reconciliation == null) throw new ArgumentNullException("reconciliation");

            ExistingList = existingList;
            ProposedList = proposedList;
            ReadingsSource = readingsSource ?? ReadingsSource.ReadOnThisPress;
            TemplatesListed = templatesListed ?? TemplateListing.Nothing;

            DocumentTitle = documentTitle ?? string.Empty;
            Template = template;
            TemplatePath = templatePath ?? string.Empty;
            OutputPath = outputPath ?? string.Empty;
            ComponentParameter = componentParameter ?? string.Empty;
            ReferenceParameter = referenceParameter ?? string.Empty;
            Location = location ?? string.Empty;
            Readings = Held(readings);
            Reconciliation = reconciliation;
            Plan = plan;
            Area = area ?? Totalled.Nothing;
            Shrubs = shrubs ?? Totalled.Nothing;
            Lawn = lawn ?? Totalled.Nothing;
            Component = component;
            Reference = reference;
            Merged = Held(merged);
            Ungrouped = Held(ungrouped);
            Outcome = outcome;
            Timing = timing ?? RunTiming.NotTimed;
        }

        public string DocumentTitle { get; }

        public KpiTemplate Template { get; }

        public string TemplatePath { get; }

        public string OutputPath { get; }

        /// <summary>
        /// Which sheet parameter the user picked for the component and which of the four plot
        /// parameters for the reference. Recorded because a wrong number has to trace back to
        /// the choice that produced it.
        /// </summary>
        public string ComponentParameter { get; }

        public string ReferenceParameter { get; }

        public string Location { get; }

        public IReadOnlyList<PlotReading> Readings { get; }

        public Reconciliation Reconciliation { get; }

        public KpiCreatePlan Plan { get; }

        public Totalled Area { get; }

        public Totalled Shrubs { get; }

        public Totalled Lawn { get; }

        public AgreedValue Component { get; }

        public AgreedValue Reference { get; }

        public IReadOnlyList<MergedSpecies> Merged { get; }

        public IReadOnlyList<SpeciesRow> Ungrouped { get; }

        /// <summary>
        /// Nothing when the accounting refused before anything was copied, which is the only
        /// way a run ends with no output file.
        /// </summary>
        public PatchOutcome Outcome { get; }

        /// <summary>
        /// How long the press took and how much of it was the read. Nothing recorded a duration
        /// before, so a five minute run over 20 plots left no file saying so.
        /// </summary>
        public RunTiming Timing { get; }

        /// <summary>
        /// The two tree lists as they were read off the template, so the report can say how many
        /// names each holds, on which rows, and which rows its total reaches. Null when the
        /// accounting refused before the template was opened.
        /// </summary>
        public SpeciesList ExistingList { get; }

        public SpeciesList ProposedList { get; }

        /// <summary>
        /// Read from the model on this press, or reused off the run before with the choices
        /// applied. The report says which, beside the seconds the read took.
        /// </summary>
        public ReadingsSource ReadingsSource { get; }

        /// <summary>
        /// How many times the pane opened a workbook in the templates folder since it listed
        /// the folder, so seven opens per tick cannot come back unnoticed. Nothing when the
        /// pane recorded none.
        /// </summary>
        public TemplateListing TemplatesListed { get; }

        public bool Wrote
        {
            get { return Outcome != null && Outcome.Written; }
        }

        private static IReadOnlyList<T> Held<T>(IEnumerable<T> items) where T : class
        {
            if (items == null) return new List<T>();
            return items.Where(item => item != null).ToList();
        }
    }
}
