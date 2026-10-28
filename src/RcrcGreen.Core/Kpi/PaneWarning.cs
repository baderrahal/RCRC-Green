using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RcrcGreen.Core.Kpi
{
    /// <summary>
    /// One box on the pane saying something is wrong: a bold line of at most ten words, the
    /// plots it is about, and the whole text behind it, which is the box's tooltip and goes into
    /// the report under BEFORE THE PRESS.
    ///
    /// **WHETHER IT STOPS ANYTHING IS A VALUE.** The pane used to decide warning against note by
    /// searching the line for WRITTEN NOWHERE and by whether it opened with two spaces, which is
    /// a signal travelling in the data, the shape this repository has already paid for twice.
    /// <see cref="Blocks"/> is set where the box is built and the pane reads nothing else.
    /// </summary>
    public sealed class PaneWarning
    {
        public PaneWarning(string title, string plots, string detail, bool blocks)
        {
            Title = title ?? string.Empty;
            Plots = plots ?? string.Empty;
            Detail = detail ?? string.Empty;
            Blocks = blocks;
        }

        public string Title { get; }

        /// <summary>The ids the box is about, one line, empty where it is about none.</summary>
        public string Plots { get; }

        public string Detail { get; }

        /// <summary>
        /// True where a plot or a press is refused over it, false where it is a note the run
        /// goes through.
        /// </summary>
        public bool Blocks { get; }

        /// <summary>
        /// The same box, all four parts compared. Two boxes built apart off one rule are one
        /// box to a person, and a list holding both says it twice.
        /// </summary>
        public bool SameAs(PaneWarning other)
        {
            return other != null
                && string.Equals(Title, other.Title, StringComparison.Ordinal)
                && string.Equals(Plots, other.Plots, StringComparison.Ordinal)
                && string.Equals(Detail, other.Detail, StringComparison.Ordinal)
                && Blocks == other.Blocks;
        }
    }

    /// <summary>
    /// Every warning box the pane can draw, built off the same rules that decide the press.
    /// Nothing here decides anything of its own: each box asks the class that already holds
    /// the rule and the sentence, and keeps that sentence whole as the box's detail.
    /// </summary>
    public static class PaneWarnings
    {
        /// <summary>
        /// Ids joined for one line in Consolas. A blank id is counted rather than printed as
        /// nothing between two commas, the case the plot list's schedule half can reach.
        ///
        /// **NEVER EVERY PLOT ON THE PANE.** Up to <see cref="CreateWords.ShownByName"/> are
        /// named and the rest are counted, blank ones among them, because 78 street plots ran
        /// off the screen once. The box's tooltip and the report hold the whole list.
        /// </summary>
        public static string Ids(IEnumerable<string> ids)
        {
            var held = (ids ?? Enumerable.Empty<string>()).ToList();
            var named = held.Where(one => !string.IsNullOrWhiteSpace(one)).Select(one => one.Trim()).ToList();
            int blank = held.Count - named.Count;

            if (named.Count > CreateWords.ShownByName)
            {
                int more = named.Count - CreateWords.ShownByName + blank;
                return string.Join(", ", named.Take(CreateWords.ShownByName).ToArray())
                    + " and " + Count(more) + " more";
            }

            if (blank == 1) named.Add("one blank id");
            else if (blank > 1) named.Add(Count(blank) + " blank ids");

            return string.Join(", ", named.ToArray());
        }

        /// <summary>
        /// Every expected PDF that was not written, after a press. Null where none is missing.
        /// </summary>
        public static PaneWarning PdfsNotWritten(PressResult result)
        {
            if (result == null || result.PdfsNotWritten.Count == 0) return null;

            return new PaneWarning(
                KpiPaneWords.PdfsNotWritten(result.PdfsNotWritten.Count),
                Ids(result.PdfsNotWritten.Select(one => one.PlotId)),
                string.Join(" ", result.PdfsNotWritten.Select(one => one.PlotId + ": " + one.Why + ".").ToArray()),
                false);
        }

        /// <summary>
        /// The plots on the team's list that are not ready to send, after a press, with the
        /// report's own reasons as the detail. Null where every one is ready or no list was read.
        /// </summary>
        public static PaneWarning NotReady(KpiResults results)
        {
            if (results == null || results.NotReadyInWords.Length == 0 || results.NotReady == 0) return null;

            return new PaneWarning(
                Plural(results.NotReady, "plot", "plots") + " not ready to send",
                Ids(results.NotReadyRows.Select(one => one.PlotId)),
                string.Join("\n", results.NotReadyRows.Select(one => one.InWords).ToArray()),
                false);
        }

        /// <summary>
        /// The links, said short. <see cref="LinksLoaded.Warning"/> is the whole text and it
        /// stays the report's own first block, so here it is the detail and nothing more.
        /// </summary>
        public static PaneWarning Links(LinksLoaded links)
        {
            if (links == null || !links.Worth) return null;

            string title;
            if (!links.WasRead) title = "Linked models were not read";
            else if (links.Instances == 0) title = "This model holds no linked model";
            else if (links.Loaded == 0) title = "No linked model is loaded";
            else title = Count(links.Unloaded.Count) + " of " + Count(links.Instances) + " linked models not loaded";

            return new PaneWarning(title, Ids(links.Unloaded), links.Warning, false);
        }

        /// <summary>
        /// Step 2: what the read found wrong with the model's own plots and with the team's list.
        /// </summary>
        public static IReadOnlyList<PaneWarning> ForRead(
            PlotsInTheModel plots, LinksLoaded links, PlotListRead list)
        {
            var boxes = new List<PaneWarning>();

            PaneWarning linked = Links(links);
            if (linked != null) boxes.Add(linked);

            if (plots != null)
            {
                string sources = string.Join(" ", CreateWords.PlotSources(plots).Select(one => one.Trim()).ToArray());

                if (plots.All.Count == 0)
                {
                    boxes.Add(new PaneWarning("This model names no plot", string.Empty, CreateWords.NoPlots, false));
                }

                if (plots.OnSheetsOnly.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(plots.OnSheetsOnly.Count, "plot", "plots") + " on a sheet but on no schedule",
                        Ids(plots.OnSheetsOnly), sources, false));
                }

                if (plots.OnSchedulesOnly.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(plots.OnSchedulesOnly.Count, "plot", "plots") + " on a schedule but on no sheet",
                        Ids(plots.OnSchedulesOnly), sources, false));
                }

                if (plots.SchedulesNotRead.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(plots.SchedulesNotRead.Count, "schedule", "schedules") + " could not be read",
                        string.Empty, string.Join(" ", plots.SchedulesNotRead.ToArray()), false));
                }
            }

            boxes.AddRange(ForTheList(plots, list));
            return boxes;
        }

        /// <summary>
        /// What is wrong with the team's plot list file itself. **A list with nothing wrong with
        /// it draws no box**, because its count is already on the In plot list line.
        /// </summary>
        public static IReadOnlyList<PaneWarning> ForTheList(PlotsInTheModel plots, PlotListRead list)
        {
            var boxes = new List<PaneWarning>();
            if (list == null || !list.Set) return boxes;

            if (!list.Read)
            {
                boxes.Add(new PaneWarning(
                    "Plot list cannot be read", string.Empty,
                    TickingTheList.Heading + " " + list.Why, false));
                return boxes;
            }

            IReadOnlyList<string> missing = TickingTheList.NotInTheModel(plots, list);
            if (missing.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    Plural(missing.Count, "listed plot is", "listed plots are") + " not in the model",
                    Ids(missing),
                    string.Join(" ", missing.Select(one => one + " is on the list and the model does not "
                        + "name it, so it cannot be ticked and nothing will be written for it.").ToArray()),
                    false));
            }

            if (list.Repeated.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    Plural(list.Repeated.Count, "plot is", "plots are") + " listed twice",
                    Ids(list.Repeated.Select(one => one.What)),
                    string.Join(" ", list.Repeated.Select(one => one.InWords).ToArray()),
                    false));
            }

            if (list.NotPlots.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    Plural(list.NotPlots.Count, "line is", "lines are") + " not plot ids",
                    Ids(list.NotPlots.SelectMany(one => one.Lines)
                        .Select(one => "line " + one.ToString(CultureInfo.InvariantCulture))),
                    string.Join(" ", list.NotPlots.Select(one => one.InWords).ToArray()),
                    false));
            }

            return boxes;
        }

        /// <summary>
        /// Step 3: the list tick has gone out of date. Null where it still stands.
        /// </summary>
        public static PaneWarning ListTick(ListTickFreshness freshness, IEnumerable<string> rowsTickedNow)
        {
            if (freshness == null) return null;

            string line = freshness.InWords(rowsTickedNow);
            return line.Length == 0
                ? null
                : new PaneWarning("Plot list tick is out of date", string.Empty, line, false);
        }

        /// <summary>
        /// A note and not a refusal: a press into the templates folder is allowed, and only a
        /// file named like a template is refused, by the per file guard at the press.
        /// </summary>
        /// <summary>
        /// Whether the output folder is the templates folder, the one comparison step 1, step 4
        /// and BEFORE THE PRESS all ask. An output folder that is not set is never it.
        /// </summary>
        public static bool OutputIsTemplates(string outputFolder, string templatesFolder)
        {
            return !string.IsNullOrWhiteSpace(outputFolder)
                && FilePaths.Compare(outputFolder, templatesFolder) == SamePath.Same;
        }

        /// <summary>The note for these two folders, or null where they differ.</summary>
        public static PaneWarning ForOutputFolder(string outputFolder, string templatesFolder)
        {
            return OutputIsTemplates(outputFolder, templatesFolder) ? OutputIsTheTemplatesFolder() : null;
        }

        public static PaneWarning OutputIsTheTemplatesFolder()
        {
            return new PaneWarning(
                "Output folder is the templates folder", string.Empty,
                TemplateWords.OutputIsTheTemplateFolder, false);
        }

        /// <summary>
        /// Step 4: everything that would stop a plot or change what a press writes, in the order
        /// a person reads down to the button.
        /// </summary>
        public static IReadOnlyList<PaneWarning> ForCreate(
            TemplateSplit split,
            PlotListRead list,
            PlotsInTheModel plots,
            Func<string, string> componentOf,
            IReadOnlyList<string> ticked,
            IReadOnlyList<SharedUid2Group> sharing,
            LinksLoaded links,
            IEnumerable<KpiCreateRun> heldRuns)
        {
            var boxes = new List<PaneWarning>();
            if (split == null) return boxes;

            foreach (TemplateShare share in split.Shares.Where(one => !one.WillWrite))
            {
                boxes.Add(new PaneWarning(
                    AssetTypeNames.Display(share.Template) + " has no ticked plot",
                    string.Empty, CreateWords.TemplateRow(share), false));
            }

            // **TWO REASONS A TICKED PLOT GOES INTO NO WORKBOOK, AND THEY ARE FIXED APART.** A
            // plot whose own asset type is not ticked is fixed by ticking it. A plot no route
            // places is fixed in the model. The split says which, as a value.
            List<PlotTemplate> noRoute = split.Unplaced.Where(one => !one.AssetTypeNotTicked).ToList();
            if (noRoute.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    Plural(noRoute.Count, "plot has", "plots have") + " no asset type",
                    Ids(noRoute.Select(one => one.PlotId)),
                    string.Join(" ", noRoute.Select(one => one.PlotId + ": " + one.Why + ".").ToArray()),
                    true));
            }

            List<PlotTemplate> notTicked = split.Unplaced.Where(one => one.AssetTypeNotTicked).ToList();
            if (notTicked.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    Plural(notTicked.Count, "plot belongs", "plots belong") + " to an asset type not ticked",
                    Ids(notTicked.Select(one => one.PlotId)),
                    string.Join(" ", notTicked.Select(one => one.PlotId + ": " + one.Why + ".").ToArray()),
                    false));
            }

            if (split.Refusals.Count > 0)
            {
                boxes.Add(new PaneWarning(
                    "A plot falls into two asset types",
                    Ids(split.CountedTwice), string.Join(" ", split.Refusals.ToArray()), true));
            }

            List<PlotTemplate> noComponent = CreateWords.NoComponentPlots(split);
            if (noComponent.Count > 0)
            {
                string detail = string.Join(" ", CreateWords.PlotsWithNoComponent(split).ToArray());
                List<PlotTemplate> filed = noComponent.Where(one => !CreateWords.FiledNowhere(one)).ToList();

                if (filed.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(filed.Count, "plot has", "plots have") + " no component. Saved in "
                            + (filed.Count == 1 ? "its" : "their") + " asset type folder.",
                        Ids(filed.Select(one => one.PlotId)), detail, false));
                }

                foreach (PlotTemplate nowhere in noComponent.Where(CreateWords.FiledNowhere))
                {
                    boxes.Add(new PaneWarning(
                        nowhere.PlotId + " can be filed nowhere", nowhere.PlotId, detail, true));
                }
            }

            if (list != null && list.Set && list.Read)
            {
                IReadOnlyList<string> extra = TickingTheList.TickedAndNotOnTheList(ticked, list);
                if (extra.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(extra.Count, "ticked plot is", "ticked plots are") + " not on the list",
                        Ids(extra),
                        string.Join(" ", extra.Select(TickingTheList.TickedAndNotOnTheListInWords).ToArray()),
                        false));
                }

                var nothing = new List<string>();
                var why = new List<string>();
                foreach (string listed in list.Plots)
                {
                    if (plots != null && !plots.Holds(listed)) continue;

                    string said = TickingTheList.WouldWriteNothing(listed, componentOf);
                    if (said.Length == 0) continue;

                    nothing.Add(listed);
                    why.Add(said);
                }

                if (nothing.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(nothing.Count, "listed plot gets", "listed plots get") + " no workbook or PDF",
                        Ids(nothing), string.Join(" ", why.ToArray()), false));
                }
            }

            if (sharing != null && sharing.Count > 0)
            {
                List<string> stopped = sharing.SelectMany(one => one.StoppedPlots).Distinct(StringComparer.Ordinal).ToList();
                if (stopped.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(stopped.Count, "plot shares", "plots share") + " a folder. None is written.",
                        Ids(stopped),
                        string.Join(" ", stopped.Select(one => one + ": " + SharedUid2.WhyStopped(sharing, one)).ToArray()),
                        true));
                }

                List<string> apart = sharing
                    .SelectMany(one => one.Plots).Select(one => one.PlotId)
                    .Where(one => !SharedUid2.Stops(sharing, one))
                    .Distinct(StringComparer.Ordinal).ToList();
                if (apart.Count > 0)
                {
                    boxes.Add(new PaneWarning(
                        Plural(apart.Count, "plot shares", "plots share") + " an id, filed apart",
                        Ids(apart),
                        string.Join(" ", apart.Select(one => one + ": " + SharedUid2.FiledApartFrom(sharing, one)).ToArray()),
                        false));
                }
            }

            PaneWarning linked = Links(links);
            if (linked != null) boxes.Add(linked);

            foreach (TemplateShare share in split.Shares.Where(one => one.Template.TakesNoArea))
            {
                boxes.Add(new PaneWarning(
                    AssetTypeNames.Display(share.Template) + " reads no area",
                    string.Empty, share.Template.Name + ": " + CreateWords.TakesNoArea, false));
            }

            boxes.AddRange(GroupsLeftOut(heldRuns));
            return boxes;
        }

        /// <summary>
        /// One box per template of the last press where a group no tree list sheet takes was
        /// left out. A note: the workbooks were written and the model needs correcting.
        /// </summary>
        public static IReadOnlyList<PaneWarning> GroupsLeftOut(IEnumerable<KpiCreateRun> heldRuns)
        {
            var boxes = new List<PaneWarning>();
            List<KpiCreateRun> runs = (heldRuns ?? Enumerable.Empty<KpiCreateRun>())
                .Where(one => one != null && one.Template != null).ToList();

            foreach (KpiTemplate template in runs.Select(one => one.Template).Distinct())
            {
                List<PlotReading> readings = runs
                    .Where(one => ReferenceEquals(one.Template, template))
                    .SelectMany(one => one.Readings).ToList();

                string leftOut = CreateWords.GroupsLeftOut(readings, template);
                if (leftOut.Length == 0) continue;

                boxes.Add(new PaneWarning(
                    "Rows with no tree list sheet were left out",
                    Ids(CreateWords.PlotsWithGroupsLeftOut(readings)),
                    template.Name + ": " + leftOut, false));
            }

            return boxes;
        }

        /// <summary>
        /// The identical areas a held press is waiting on, an error box with its own button.
        /// Null where none is waiting.
        /// </summary>
        public static PaneWarning SameArea(IReadOnlyList<IdenticalArea> identical)
        {
            if (identical == null || identical.Count == 0) return null;

            return new PaneWarning(
                "Plots report the same area",
                Ids(identical.SelectMany(one => one.Plots).Distinct(StringComparer.Ordinal)),
                string.Join(" ", CreateWords.ConfirmIdentical(identical).Select(one => one.Trim()).ToArray()),
                true);
        }

        private static string Count(int howMany)
        {
            return howMany.ToString(CultureInfo.InvariantCulture);
        }

        private static string Plural(int howMany, string one, string many)
        {
            return Count(howMany) + " " + (howMany == 1 ? one : many);
        }
    }
}
