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
            double readSeconds = 0.0,
            LinksLoaded links = null)
        {
            ElementInstances = elementInstances;
            ReadSeconds = readSeconds;
            Links = links ?? LinksLoaded.NotRead;
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
        /// What the linked models were doing when the plots were read, so the pane can say a
        /// run will find nothing before anybody presses Create rather than after 78 plots.
        /// </summary>
        public LinksLoaded Links { get; }

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
    /// One ticked workbook row: the template it is, the file it came from and the name its
    /// output takes. **One box cannot name six files**, so every ticked template carries its
    /// own name and the pane offers a row per template rather than one box.
    /// </summary>
    internal sealed class TemplatePick
    {
        public TemplatePick(KpiTemplate template, string templatePath, string outputName)
        {
            if (template == null) throw new ArgumentNullException("template");

            Template = template;
            TemplatePath = templatePath ?? string.Empty;
            OutputName = outputName ?? string.Empty;
        }

        public KpiTemplate Template { get; }

        public string TemplatePath { get; }

        public string OutputName { get; }
    }

    /// <summary>
    /// Everything one press of Create carries into the external event. Built by the pane out
    /// of what the user chose, so nothing on the Revit side has to reach back into a control.
    ///
    /// The region choices are per plot and start empty. A plot whose regions leave the answer
    /// open is refused by the reconciliation rather than guessed, the user picks, and Create is
    /// pressed again with the choice on here.
    /// </summary>
    /// <summary>
    /// **SEVERAL TEMPLATES IN ONE PRESS.** The ticked plots are read once and split by the
    /// template each of them belongs to, so a plot is read for its own workbook and for no
    /// other. The three the team types are ONE SET for the whole run, Bader's decision, and
    /// they go into every workbook this press writes.
    /// </summary>
    internal sealed class KpiCreateAsk
    {
        public KpiCreateAsk(
            IEnumerable<string> ticked,
            IEnumerable<TemplatePick> templates,
            string componentParameter,
            string referenceParameter,
            string locationParameter,
            bool identicalAreasConfirmed,
            IDictionary<string, string> chosenRegions,
            string date,
            string preparedBy,
            string position,
            IEnumerable<KpiCreateRun> heldRuns = null,
            TemplateListing templatesListed = null,
            IDictionary<string, string> componentPerPlot = null)
        {
            Ticked = (ticked ?? Enumerable.Empty<string>()).Where(one => one != null).ToList();
            Templates = (templates ?? Enumerable.Empty<TemplatePick>()).Where(one => one != null).ToList();
            HeldRuns = (heldRuns ?? Enumerable.Empty<KpiCreateRun>()).Where(one => one != null).ToList();
            ComponentPerPlot = new Dictionary<string, string>(
                componentPerPlot ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            ComponentParameter = componentParameter ?? string.Empty;
            ReferenceParameter = referenceParameter ?? string.Empty;
            LocationParameter = locationParameter ?? string.Empty;
            IdenticalAreasConfirmed = identicalAreasConfirmed;
            ChosenRegions = new Dictionary<string, string>(
                chosenRegions ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            Date = date ?? string.Empty;
            PreparedBy = preparedBy ?? string.Empty;
            Position = position ?? string.Empty;
            TemplatesListed = templatesListed ?? TemplateListing.Nothing;
        }

        /// <summary>
        /// The runs the pane holds from the press before, the same objects and not copies, so a
        /// choice made after a refusal can be applied to their readings. One per template that
        /// ran, and Core decides per template whether each can be trusted. **This is the one
        /// reuse mechanism and there is no second cache beside it.**
        /// </summary>
        public IReadOnlyList<KpiCreateRun> HeldRuns { get; }

        /// <summary>
        /// The run held for one template, or nothing. A press that ticks a template it did not
        /// tick before finds none and reads that template's plots, which is what should happen.
        /// </summary>
        public KpiCreateRun HeldRunFor(KpiTemplate template)
        {
            return HeldRuns.FirstOrDefault(one => ReferenceEquals(one.Template, template));
        }

        /// <summary>
        /// What each plot's sheets hold for the component, carried from the plot read so the
        /// split can be worked out on the Revit thread without reading the sheets again.
        /// </summary>
        public IDictionary<string, string> ComponentPerPlot { get; }

        public string ComponentOn(string plotId)
        {
            string held;
            return ComponentPerPlot.TryGetValue(plotId ?? string.Empty, out held) ? held : string.Empty;
        }

        /// <summary>
        /// Every ticked workbook row, in the order the templates are listed.
        /// </summary>
        public IReadOnlyList<TemplatePick> Templates { get; }

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
