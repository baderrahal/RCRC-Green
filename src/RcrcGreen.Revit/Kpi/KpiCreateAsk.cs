using System;
using System.Collections.Generic;
using System.Linq;
using RcrcGreen.Core.Kpi;

namespace RcrcGreen.Revit.Kpi
{
    /// <summary>
    /// What the pane read out of the model before anything was ticked: the plots, and the
    /// names the two dropdowns offer. Plain values, so the pane holds no Revit object.
    /// </summary>
    internal sealed class KpiPlotFacts
    {
        public KpiPlotFacts(
            PlotsInTheModel plots,
            IReadOnlyList<string> componentNames,
            IReadOnlyList<string> locationNames,
            IDictionary<string, string> componentPerPlot = null,
            IDictionary<string, IReadOnlyList<PlotParameterValue>> referenceValuesPerPlot = null,
            int elementInstances = 0,
            double readSeconds = 0.0)
        {
            ElementInstances = elementInstances;
            ReadSeconds = readSeconds;
            Plots = plots ?? PlotsInTheModel.Of(null, null);
            ComponentNames = componentNames ?? new List<string>();
            LocationNames = locationNames ?? new List<string>();
            ComponentPerPlot = new Dictionary<string, string>(
                componentPerPlot ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            ReferenceValuesPerPlot = new Dictionary<string, IReadOnlyList<PlotParameterValue>>(
                referenceValuesPerPlot ?? new Dictionary<string, IReadOnlyList<PlotParameterValue>>(),
                StringComparer.Ordinal);
        }

        /// <summary>
        /// How many elements the model holds and how long this read took. The header used to
        /// get these off the scan, which meant the model's name sat over an empty line until
        /// somebody pressed a button. The plot read already walks the document, so it answers
        /// for the header too and the header fills as soon as the pane is shown.
        /// </summary>
        public int ElementInstances { get; }

        public double ReadSeconds { get; }

        /// <summary>
        /// What each plot's first sheet holds for the component, read with the first name the
        /// model offers. It is what preselects a template, and the user can change both the
        /// parameter and the template afterwards.
        /// </summary>
        public IDictionary<string, string> ComponentPerPlot { get; }

        /// <summary>
        /// The four plot parameters with the value each holds, per plot, so the pane can show
        /// the ticked plot's rather than one plot's for all of them.
        /// </summary>
        public IDictionary<string, IReadOnlyList<PlotParameterValue>> ReferenceValuesPerPlot { get; }

        /// <summary>
        /// What the four hold on one plot, empty for a plot no sheet carries.
        /// </summary>
        public IReadOnlyList<PlotParameterValue> ReferenceValuesOn(string plotId)
        {
            IReadOnlyList<PlotParameterValue> held;
            return ReferenceValuesPerPlot.TryGetValue(plotId ?? string.Empty, out held)
                ? held
                : new List<PlotParameterValue>();
        }

        public string ComponentOn(string plotId)
        {
            string held;
            return ComponentPerPlot.TryGetValue(plotId ?? string.Empty, out held) ? held : string.Empty;
        }

        public PlotsInTheModel Plots { get; }

        public IReadOnlyList<string> ComponentNames { get; }

        public IReadOnlyList<string> LocationNames { get; }
    }

    /// <summary>
    /// Everything one press of Create carries into the external event. Built by the pane out
    /// of what the user chose, so nothing on the Revit side has to reach back into a control.
    ///
    /// The region choices are per plot and start empty. A plot whose regions leave the answer
    /// open is refused by the reconciliation rather than guessed, the user picks, and Create is
    /// pressed again with the choice on here.
    /// </summary>
    internal sealed class KpiCreateAsk
    {
        public KpiCreateAsk(
            IEnumerable<string> ticked,
            KpiTemplate template,
            string templatePath,
            string outputName,
            string componentParameter,
            string referenceParameter,
            string locationParameter,
            bool identicalAreasConfirmed,
            IDictionary<string, string> chosenRegions,
            string date,
            string preparedBy,
            string position,
            KpiCreateRun heldRun = null,
            TemplateListing templatesListed = null)
        {
            Ticked = (ticked ?? Enumerable.Empty<string>()).Where(one => one != null).ToList();
            Template = template;
            TemplatePath = templatePath ?? string.Empty;
            OutputName = outputName ?? string.Empty;
            ComponentParameter = componentParameter ?? string.Empty;
            ReferenceParameter = referenceParameter ?? string.Empty;
            LocationParameter = locationParameter ?? string.Empty;
            IdenticalAreasConfirmed = identicalAreasConfirmed;
            ChosenRegions = new Dictionary<string, string>(
                chosenRegions ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            Date = date ?? string.Empty;
            PreparedBy = preparedBy ?? string.Empty;
            Position = position ?? string.Empty;
            HeldRun = heldRun;
            TemplatesListed = templatesListed ?? TemplateListing.Nothing;
        }

        /// <summary>
        /// The run the pane holds from the press before, the same object and not a copy, so a
        /// choice made after a refusal can be applied to its readings. Null when the pane holds
        /// none, and Core decides whether it can be trusted.
        /// </summary>
        public KpiCreateRun HeldRun { get; }

        /// <summary>
        /// The templates folder as the pane holds it, so the report can say how many times a
        /// workbook was opened.
        /// </summary>
        public TemplateListing TemplatesListed { get; }

        /// <summary>
        /// The three the team types on the pane, for E5, G5 and H5. They come from no model, so
        /// nothing on the Revit side can read them and they have to travel on here.
        ///
        /// **They were not on here at all.** The pane collected them, remembered two of them for
        /// next time, and handed none of the three to the run, so the first real workbook came
        /// out holding the template's own placeholders while the report said nobody had typed
        /// them. What the boxes hold and what Create reads were two different things.
        /// </summary>
        public string Date { get; }

        public string PreparedBy { get; }

        public string Position { get; }

        public IReadOnlyList<string> Ticked { get; }

        public KpiTemplate Template { get; }

        public string TemplatePath { get; }

        public string OutputName { get; }

        public string ComponentParameter { get; }

        public string ReferenceParameter { get; }

        public string LocationParameter { get; }

        public bool IdenticalAreasConfirmed { get; }

        public IDictionary<string, string> ChosenRegions { get; }

        public string RegionChosenFor(string plotId)
        {
            string chosen;
            return ChosenRegions.TryGetValue(plotId ?? string.Empty, out chosen) ? chosen : string.Empty;
        }
    }
}
