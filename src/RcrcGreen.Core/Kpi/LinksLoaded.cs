using System;
using System.Collections.Generic;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// What the run should say about the linked models, at the top of the checklist report and
    /// on the pane before anybody presses Create.
    ///
    /// **Measured on the first STREETS run, NG05 at 16:06, 78 plots.** All 78 contributed
    /// nothing. All 156 schedules printed one row, the header, and no body. The scan from the
    /// same session says six link instances and NONE LOADED. The plants live in the linked
    /// component models, so with no link loaded a schedule has nothing to list, and opening
    /// ST-05-(600) SOFTSCAPE SCHEDULE in Revit shows it empty on screen. **The tool was right
    /// and every plot carried its reason.** What it never said was the one thing that explains
    /// all 78 at once, and a run where every plot contributed nothing should open with the
    /// reason rather than end with 78 identical lines.
    ///
    /// This is a note and never a refusal. A model with no link loaded is a legitimate thing
    /// to open, and the team may be working on the host alone.
    /// </summary>
    public sealed class LinksLoaded
    {
        private LinksLoaded(int instances, int loaded, bool wasRead, string warning, IEnumerable<string> unloaded)
        {
            Instances = instances;
            Loaded = loaded;
            WasRead = wasRead;
            Warning = warning ?? string.Empty;
            Unloaded = (unloaded ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>
        /// Nothing known either way, for a run whose readings were held from a press that
        /// carried no link read. It says nothing rather than claiming every link is fine.
        /// </summary>
        public static readonly LinksLoaded NotRead =
            new LinksLoaded(0, 0, false, string.Empty, null);

        public static LinksLoaded Of(LinkFacts links)
        {
            if (links == null) return NotRead;
            if (!links.WasRead)
            {
                return new LinksLoaded(0, 0, false,
                    "THE LINKED MODELS WERE NOT READ, so nothing here can say whether the schedules had "
                    + "anything to list. " + links.WhyNotRead, null);
            }

            IReadOnlyList<ScannedLinkInstance> instances = links.Instances;
            List<string> unloaded = instances.Where(one => !one.IsLoaded).Select(one => one.Name).ToList();
            int loaded = instances.Count - unloaded.Count;

            if (instances.Count == 0)
            {
                return new LinksLoaded(0, 0, true,
                    "THIS MODEL HOLDS NO LINKED MODEL. The plants are in the linked component models, so "
                    + "every schedule that lists them lists nothing.", null);
            }

            if (loaded == 0)
            {
                return new LinksLoaded(instances.Count, 0, true,
                    "NO LINK IS LOADED. " + Count(instances.Count, "link instance")
                    + " and not one of them loaded, so every schedule that lists linked elements lists "
                    + "nothing and every plot will contribute nothing. Load the links and read the model "
                    + "again. Not loaded: " + string.Join(", ", unloaded.ToArray()) + ".",
                    unloaded);
            }

            if (unloaded.Count > 0)
            {
                return new LinksLoaded(instances.Count, loaded, true,
                    Count(unloaded.Count, "link instance") + " of " + instances.Count + " not loaded, so a "
                    + "schedule that lists what they hold lists nothing. Not loaded: "
                    + string.Join(", ", unloaded.ToArray()) + ".",
                    unloaded);
            }

            return new LinksLoaded(instances.Count, loaded, true, string.Empty, null);
        }

        public int Instances { get; }

        public int Loaded { get; }

        /// <summary>
        /// False when the links were never read, so no line claims they are fine.
        /// </summary>
        public bool WasRead { get; }

        /// <summary>
        /// Empty when every link is loaded, which is the only state worth saying nothing
        /// about. Anything else is one line, at the top of the report and on the pane.
        /// </summary>
        public string Warning { get; }

        public IReadOnlyList<string> Unloaded { get; }

        /// <summary>
        /// True where the run is worth warning about before it is pressed.
        /// </summary>
        public bool Worth
        {
            get { return Warning.Length > 0; }
        }

        private static string Count(int howMany, string thing)
        {
            return howMany + " " + thing + (howMany == 1 ? string.Empty : "s");
        }
    }
}
