using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the pane said before a press, in full, for the report.
    ///
    /// **THE PANE WAS CUT TO TEN WORDS A SENTENCE AND NOTHING IT SAID WAS DROPPED.** Every
    /// sentence that explained a folder, a file, a parameter or a warning box moved off the
    /// screen into a tooltip and into this section, so a report read on its own still says what
    /// the press was pointed at and what the pane warned about. It is built at the press off the
    /// same Core words the pane used to print, so the two cannot say different things.
    /// </summary>
    public sealed class BeforeThePress
    {
        public static readonly BeforeThePress Nothing = new BeforeThePress(
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
            null, null, string.Empty);

        public BeforeThePress(
            string templatesFolder,
            string outputFolder,
            string formsFolder,
            string streetReferencePath,
            string streetReferenceInWords,
            string plotListPath,
            string plotListInWords,
            string componentParameter,
            string referenceParameter,
            string locationParameter,
            string plotsRead,
            string preselection,
            IEnumerable<string> wouldFill,
            IEnumerable<PaneWarning> warnings,
            string scan)
        {
            TemplatesFolder = templatesFolder ?? string.Empty;
            OutputFolder = outputFolder ?? string.Empty;
            FormsFolder = formsFolder ?? string.Empty;
            StreetReferencePath = streetReferencePath ?? string.Empty;
            StreetReferenceInWords = streetReferenceInWords ?? string.Empty;
            PlotListPath = plotListPath ?? string.Empty;
            PlotListInWords = plotListInWords ?? string.Empty;
            ComponentParameter = componentParameter ?? string.Empty;
            ReferenceParameter = referenceParameter ?? string.Empty;
            LocationParameter = locationParameter ?? string.Empty;
            PlotsRead = plotsRead ?? string.Empty;
            Preselection = preselection ?? string.Empty;
            WouldFill = (wouldFill ?? Enumerable.Empty<string>()).ToList();
            Warnings = Once(warnings);
            Scan = scan ?? string.Empty;
        }

        public string TemplatesFolder { get; }
        public string OutputFolder { get; }
        public string FormsFolder { get; }
        public string StreetReferencePath { get; }
        public string StreetReferenceInWords { get; }
        public string PlotListPath { get; }
        public string PlotListInWords { get; }
        public string ComponentParameter { get; }
        public string ReferenceParameter { get; }
        public string LocationParameter { get; }

        /// <summary>The header's old read line, which says the plots read is not a read of the model.</summary>
        public string PlotsRead { get; }

        /// <summary>Why the asset types stood as they did: what preselected one, or what stopped it.</summary>
        public string Preselection { get; }

        public IReadOnlyList<string> WouldFill { get; }

        public IReadOnlyList<PaneWarning> Warnings { get; }

        /// <summary>
        /// The scan's own headline, which names the scan report. Added by the handler, because
        /// the scan runs inside the press after the pane has handed this over.
        /// </summary>
        public string Scan { get; }

        public bool IsNothing
        {
            get { return ReferenceEquals(this, Nothing); }
        }

        public BeforeThePress WithScan(string scan)
        {
            if (IsNothing) return this;

            return new BeforeThePress(
                TemplatesFolder, OutputFolder, FormsFolder, StreetReferencePath, StreetReferenceInWords,
                PlotListPath, PlotListInWords, ComponentParameter, ReferenceParameter, LocationParameter,
                PlotsRead, Preselection, WouldFill, Warnings, scan);
        }

        public const string Heading = "BEFORE THE PRESS";

        /// <summary>
        /// The section's lines under its heading. **The folder sentences are the pane's old
        /// words built with the real root**, so the layout reads with the folder this press used.
        /// </summary>
        public IReadOnlyList<string> Lines()
        {
            var lines = new List<string>();
            var chosen = new ChosenParameters(ComponentParameter, ReferenceParameter, LocationParameter);

            lines.Add("  templates folder: " + Shown(TemplatesFolder));
            if (TemplatesFolder.Length == 0) lines.Add("    " + TemplateWords.NoFolder);

            lines.Add("  output folder: " + Shown(OutputFolder));
            lines.Add("    " + CreateWords.WhereTheWorkbooksGo(OutputFolder));
            lines.Add("    " + TemplateWords.Output(OutputFolder));

            lines.Add("  forms folder: " + Shown(FormsFolder));
            lines.Add("    " + TemplateWords.FormsFolder(FormsFolder));

            lines.Add("  street reference file: " + Shown(StreetReferencePath));
            if (StreetReferenceInWords.Length > 0) lines.Add("    " + StreetReferenceInWords);

            lines.Add("  plot list file: " + Shown(PlotListPath));
            if (PlotListInWords.Length > 0) lines.Add("    " + PlotListInWords);

            lines.Add("  values from: component " + Shown(ComponentParameter)
                + ", reference " + Shown(ReferenceParameter)
                + ", location " + Shown(LocationParameter));
            lines.Add("    " + TemplateWords.ReferenceOnThisSheet(chosen));
            lines.Add("    " + TemplateWords.TypedOnThisPane);
            lines.Add("    " + CreateWords.OneSetOfFields);

            if (PlotsRead.Length > 0) lines.Add("  the plots read: " + PlotsRead);
            if (Scan.Length > 0) lines.Add("  the scan: " + Scan);
            lines.Add("  " + KpiPaneWords.ReadOnly);

            if (Preselection.Length > 0) lines.Add("  asset types: " + Preselection);

            if (WouldFill.Count > 0)
            {
                lines.Add("  what each ticked asset type fills:");
                foreach (string line in WouldFill) lines.Add("    " + line);
            }

            lines.Add("  warning boxes the four steps would draw for this press, each once: " + Warnings.Count);
            foreach (PaneWarning box in Warnings)
            {
                lines.Add("    " + (box.Blocks ? "STOPS A WRITE: " : "note: ") + box.Title
                    + (box.Plots.Length == 0 ? string.Empty : " | " + box.Plots));
                if (box.Detail.Length > 0) lines.Add("      " + box.Detail);
            }

            return lines;
        }

        /// <summary>
        /// Every box once, compared as values. Step 2 and step 4 both draw the links box, so
        /// gathering the four steps' boxes holds it twice, and the count and the detail would
        /// print it twice.
        /// </summary>
        private static IReadOnlyList<PaneWarning> Once(IEnumerable<PaneWarning> warnings)
        {
            var kept = new List<PaneWarning>();
            foreach (PaneWarning one in (warnings ?? Enumerable.Empty<PaneWarning>()).Where(one => one != null))
            {
                if (kept.Any(held => held.SameAs(one))) continue;
                kept.Add(one);
            }

            return kept;
        }

        /// <summary>
        /// Whether this press's section holds a box with this title, compared as a value. Show
        /// in report is enabled off this, so a box that first appeared after the press does not
        /// point into a file that does not hold it.
        /// </summary>
        public bool Holds(PaneWarning box)
        {
            return box != null && Warnings.Any(one => one.SameAs(box));
        }

        private static string Shown(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "(not set)" : value;
        }
    }
}
