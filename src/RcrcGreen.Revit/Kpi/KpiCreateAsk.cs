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
            string modelFolder,
            IDictionary<string, string> componentPerPlot = null,
            IReadOnlyList<PlotParameterValue> referenceChoices = null)
        {
            Plots = plots ?? PlotsInTheModel.Of(null, null);
            ComponentNames = componentNames ?? new List<string>();
            LocationNames = locationNames ?? new List<string>();
            ModelFolder = modelFolder ?? string.Empty;
            ComponentPerPlot = new Dictionary<string, string>(
                componentPerPlot ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            ReferenceChoices = referenceChoices ?? new List<PlotParameterValue>();
        }

        /// <summary>
        /// What each plot's first sheet holds for the component, read with the first name the
        /// model offers. It is what preselects a template, and the user can change both the
        /// parameter and the template afterwards.
        /// </summary>
        public IDictionary<string, string> ComponentPerPlot { get; }

        /// <summary>
        /// The four plot parameters with the value each holds on the first plot, so the
        /// reference is picked by looking at a value rather than at a name.
        /// </summary>
        public IReadOnlyList<PlotParameterValue> ReferenceChoices { get; }

        public string ComponentOn(string plotId)
        {
            string held;
            return ComponentPerPlot.TryGetValue(plotId ?? string.Empty, out held) ? held : string.Empty;
        }

        public PlotsInTheModel Plots { get; }

        public IReadOnlyList<string> ComponentNames { get; }

        public IReadOnlyList<string> LocationNames { get; }

        public string ModelFolder { get; }
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
            IDictionary<string, string> chosenRegions)
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
        }

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
