using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using RcrcGreen.Core.ViewFilters;

namespace RcrcGreen.Revit.ViewFilters
{
    /// <summary>
    /// The run itself, ported whole from the working argus host. The body of Run is that
    /// host's body with four edits and the host swaps, nothing else, and every line that
    /// differs beyond those is named in steps/log-view-filters.md. It runs only inside
    /// <see cref="ViewFiltersRequestHandler"/>'s Execute, one transaction, one undo.
    ///
    /// Scan is the read only half: the same view collection, the same plot id rule and the
    /// same name matching as the run, with nothing written, so the grid the pane draws says
    /// what a press of Apply will really do.
    /// </summary>
    internal sealed class ViewFiltersRunner
    {
        private readonly Document _document;

        private readonly Action<string, int, int> _progress;

        private readonly Func<bool> _cancelAsked;

        /// <summary>
        /// Named like the host's logger so the ported call sites read exactly as written.
        /// </summary>
        private readonly RunLog Logger;

        public ViewFiltersRunner(
            Document document,
            Action<string> logged,
            Action<string, int, int> progress,
            Func<bool> cancelAsked)
        {
            if (document == null) throw new ArgumentNullException("document");

            _document = document;
            Logger = new RunLog(logged);
            _progress = progress;
            _cancelAsked = cancelAsked;
        }

        private void ProgressUpdate(string line, int done, int of)
        {
            _progress?.Invoke(line, done, of);
        }

        /// <summary>
        /// The host this body came from carried a cancel token. The progress window here
        /// has none, by KPI's recorded decision, so the probe answers not cancelled until
        /// a real cancel exists to wire. The call sites stay, so wiring one later touches
        /// nothing in the body.
        /// </summary>
        private void CheckCancellationRequested()
        {
            if (_cancelAsked != null && _cancelAsked())
            {
                throw new OperationCanceledException();
            }
        }

        public Output Run(Inputs input)
        {
            Document doc = _document;

            string[] viewKeywords = ViewKeywords.Split(input.ViewKeywords);

            var filterConfigs = FilterRows.Kept(input.Filters);

            if (viewKeywords.Length == 0 || filterConfigs.Length == 0)
            {
                Logger.AppendLine("Error: Please provide at least one view keyword and one filter configuration.");
                return new Output { ViewsEvaluated = 0, ViewsModified = 0, FiltersAdded = 0, FiltersConfigured = 0,
                    FiltersCreatedInDoc = 0, FiltersNotFoundInDoc = 0, Skipped = 0, Blocked = 0,
                    Logs = new string[] { "Please provide at least one view keyword and one filter configuration." },
                    Failures = new ViewFilterFailure[0] };
            }

            Autodesk.Revit.DB.Color ParseHexColor(string hex)
            {
                return HexColor.TryParse(hex, out byte r, out byte g, out byte b)
                    ? new Autodesk.Revit.DB.Color(r, g, b)
                    : null;
            }

            List<View> targetViews = CollectTargetViews(doc, viewKeywords);

            List<ParameterFilterElement> allFilters = CollectAllFilters(doc);

            FillPatternElement solidFillPattern = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(fp => fp.GetFillPattern() != null && fp.GetFillPattern().IsSolidFill);

            int totalViews = targetViews.Count;
            int filtersAdded = 0;
            int filtersConfigured = 0;
            int filtersCreated = 0;
            int viewsModified = 0;
            int filtersNotFound = 0;
            int viewsSkipped = 0;
            int viewsBlocked = 0;
            List<string> runLogs = new List<string>();

            // One entry per thing not applied, each holding the very sentence logged for
            // it. The sentence is built into a local at every site and handed to both, so
            // the log and the result panel cannot word one failure two ways.
            List<ViewFilterFailure> failures = new List<ViewFilterFailure>();

            using (Transaction trans = new Transaction(doc, "RCRC Green - View Filters"))
            {
                trans.Start();

                for (int i = 0; i < totalViews; i++)
                {
                    CheckCancellationRequested();
                    View v = targetViews[i];
                    ProgressUpdate($"Processing views, {i + 1} of {totalViews}", i + 1, totalViews);

                    // The fourth edit. A view whose template owns the filters setting threw on
                    // every write and vanished into the catch below. It is left alone and named,
                    // and nothing here ever writes to a view template.
                    if (TemplateOwnsTheFilters(doc, v, out string blockingTemplate))
                    {
                        viewsBlocked++;
                        string blocked = $"Blocked '{v.Name}', its template '{blockingTemplate}' owns the filters setting.";
                        Logger.AppendLine(blocked);
                        failures.Add(new ViewFilterFailure(v.Name, blocked));
                        continue;
                    }

                    string viewName = v.Name;
                    // The first edit. The host fell back to the whole view name when nothing
                    // parsed, which created filters named after views, so a name that does not
                    // start with a plot id is skipped by name. Core holds the rule and the scan
                    // reads the same one.
                    if (!ViewFilterPlotCode.TryFromViewName(viewName, out string plotCode))
                    {
                        viewsSkipped++;
                        string skipped = $"Skipped '{viewName}', its name does not start with a plot id.";
                        Logger.AppendLine(skipped);
                        failures.Add(new ViewFilterFailure(viewName, skipped));
                        continue;
                    }

                    ICollection<ElementId> currentFilterIds = new HashSet<ElementId>();
                    try { currentFilterIds = v.GetFilters(); }
                    catch
                    {
                        // This catch continued in silence, so a view whose filters could
                        // not be read looked exactly like one that was fine. It counts
                        // toward no counter, as before, and it says so now.
                        string couldNotRead = $"Could not read the filters on '{viewName}', so it is left as it is.";
                        Logger.AppendLine(couldNotRead);
                        failures.Add(new ViewFilterFailure(viewName, couldNotRead));
                        continue;
                    }

                    bool viewModified = false;

                    foreach (var filterCfg in filterConfigs)
                    {
                        string prefix = filterCfg.Prefix;
                        string targetFilterName = ViewFilterNames.TargetFilterName(prefix, plotCode);

                        // Whether this lookup's failure has already been said, so the
                        // guard under the exemplar path does not word one failure twice.
                        bool failureSaid = false;

                        ParameterFilterElement filter = allFilters.FirstOrDefault(f =>
                            ViewFilterNames.IsExactMatch(f.Name, targetFilterName));

                        if (filter == null)
                        {
                            filter = allFilters.FirstOrDefault(f =>
                                ViewFilterNames.IsPrefixAndPlotMatch(f.Name, prefix, plotCode));
                        }

                        if (filter == null)
                        {
                            var exemplar = allFilters.FirstOrDefault(f =>
                                ViewFilterNames.HasPrefix(f.Name, prefix));

                            if (exemplar != null)
                            {
                                try
                                {
                                    string exemplarPlotCode = ViewFilterNames.ExemplarPlotCode(exemplar.Name, prefix);

                                    // The second edit, first guard. An exemplar whose own tail is
                                    // not a plot id gives the clone nothing real to replace, so
                                    // nothing is created from it.
                                    if (!ViewFilterPlotCode.StartsWithAPlotId(exemplarPlotCode))
                                    {
                                        filtersNotFound++;
                                        string cannotCreate = $"Cannot create '{targetFilterName}', the exemplar '{exemplar.Name}' does not end in a plot id, skipped.";
                                        Logger.AppendLine(cannotCreate);
                                        failures.Add(new ViewFilterFailure(targetFilterName, cannotCreate));
                                        continue;
                                    }

                                    var catIds = exemplar.GetCategories();
                                    var exemplarFilter = exemplar.GetElementFilter();
                                    ElementFilter newElemFilter = exemplarFilter != null
                                        ? CloneAndReplaceFilter(exemplarFilter, exemplarPlotCode, plotCode)
                                        : null;

                                    // The second edit, second guard. The host created the filter
                                    // with categories only here, and a filter with no rules
                                    // matches every element in those categories.
                                    if (newElemFilter == null)
                                    {
                                        filtersNotFound++;
                                        string cannotCopy = $"Cannot copy rules from exemplar '{exemplar.Name}', skipped.";
                                        Logger.AppendLine(cannotCopy);
                                        failures.Add(new ViewFilterFailure(targetFilterName, cannotCopy));
                                        continue;
                                    }

                                    ParameterFilterElement newFilter =
                                        ParameterFilterElement.Create(doc, targetFilterName, catIds, newElemFilter);

                                    if (newFilter != null)
                                    {
                                        filter = newFilter;
                                        allFilters.Add(newFilter);
                                        filtersCreated++;
                                        Logger.AppendLine($"Created missing filter '{targetFilterName}' in document.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string createFailed = $"Failed to create filter '{targetFilterName}': {ex.Message}";
                                    Logger.AppendLine(createFailed);
                                    failures.Add(new ViewFilterFailure(targetFilterName, createFailed));
                                    failureSaid = true;
                                }
                            }
                        }

                        if (filter == null)
                        {
                            // This counted with nothing logged when no exemplar carried
                            // the prefix, so a lookup that found nothing looked exactly
                            // like one that never ran. The count is untouched and covers
                            // a thrown create too, which said its own line above.
                            if (!failureSaid)
                            {
                                string nothingToUse = $"No filter named '{targetFilterName}' was found or created.";
                                Logger.AppendLine(nothingToUse);
                                failures.Add(new ViewFilterFailure(targetFilterName, nothingToUse));
                            }

                            filtersNotFound++;
                            continue;
                        }

                        try
                        {
                            if (!currentFilterIds.Contains(filter.Id)) { v.AddFilter(filter.Id); filtersAdded++; }

                            v.SetIsFilterEnabled(filter.Id, filterCfg.Enabled);
                            v.SetFilterVisibility(filter.Id, filterCfg.Visible);

                            // The third edit. A fresh settings object wiped any override already
                            // on the filter in this view that this run does not set, and the
                            // choice was leave it as it was.
                            OverrideGraphicSettings ogs = v.GetFilterOverrides(filter.Id);
                            bool hasOverrides = false;

                            if (filterCfg.OverrideLineColor)
                            {
                                var lineCol = ParseHexColor(filterCfg.LineColor);
                                if (lineCol != null)
                                {
                                    ogs.SetProjectionLineColor(lineCol);
                                    ogs.SetCutLineColor(lineCol);
                                    hasOverrides = true;
                                }
                            }

                            if (filterCfg.LineWeight > 0 && filterCfg.LineWeight <= 16)
                            {
                                ogs.SetProjectionLineWeight(filterCfg.LineWeight);
                                ogs.SetCutLineWeight(filterCfg.LineWeight);
                                hasOverrides = true;
                            }

                            if (filterCfg.OverrideForegroundPattern)
                            {
                                var fgCol = ParseHexColor(filterCfg.ForegroundPatternColor);
                                if (fgCol != null)
                                {
                                    ogs.SetSurfaceForegroundPatternColor(fgCol);
                                    ogs.SetSurfaceForegroundPatternVisible(true);
                                    ogs.SetCutForegroundPatternColor(fgCol);
                                    ogs.SetCutForegroundPatternVisible(true);
                                    hasOverrides = true;
                                }

                                if (string.Equals(filterCfg.ForegroundPatternType, "solid", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (solidFillPattern != null)
                                    {
                                        ogs.SetSurfaceForegroundPatternId(solidFillPattern.Id);
                                        ogs.SetCutForegroundPatternId(solidFillPattern.Id);
                                        hasOverrides = true;
                                    }
                                }
                                else if (string.Equals(filterCfg.ForegroundPatternType, "none", StringComparison.OrdinalIgnoreCase) ||
                                         string.IsNullOrEmpty(filterCfg.ForegroundPatternType))
                                {
                                    ogs.SetSurfaceForegroundPatternId(ElementId.InvalidElementId);
                                    ogs.SetCutForegroundPatternId(ElementId.InvalidElementId);
                                    hasOverrides = true;
                                }
                            }

                            if (filterCfg.OverrideBackgroundPattern)
                            {
                                var bgCol = ParseHexColor(filterCfg.BackgroundPatternColor);
                                if (bgCol != null)
                                {
                                    ogs.SetSurfaceBackgroundPatternColor(bgCol);
                                    ogs.SetSurfaceBackgroundPatternVisible(true);
                                    ogs.SetCutBackgroundPatternColor(bgCol);
                                    ogs.SetCutBackgroundPatternVisible(true);
                                    hasOverrides = true;
                                }

                                if (string.Equals(filterCfg.BackgroundPatternType, "solid", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (solidFillPattern != null)
                                    {
                                        ogs.SetSurfaceBackgroundPatternId(solidFillPattern.Id);
                                        ogs.SetCutBackgroundPatternId(solidFillPattern.Id);
                                        hasOverrides = true;
                                    }
                                }
                                else if (string.Equals(filterCfg.BackgroundPatternType, "none", StringComparison.OrdinalIgnoreCase) ||
                                         string.IsNullOrEmpty(filterCfg.BackgroundPatternType))
                                {
                                    ogs.SetSurfaceBackgroundPatternId(ElementId.InvalidElementId);
                                    ogs.SetCutBackgroundPatternId(ElementId.InvalidElementId);
                                    hasOverrides = true;
                                }
                            }

                            if (filterCfg.Halftone) { ogs.SetHalftone(true); hasOverrides = true; }

                            if (hasOverrides) { v.SetFilterOverrides(filter.Id, ogs); }

                            filtersConfigured++;
                            viewModified = true;
                        }
                        catch (Exception ex)
                        {
                            string configureFailed = $"Failed to configure filter '{filter.Name}' on view '{v.Name}': {ex.Message}";
                            Logger.AppendLine(configureFailed);
                            failures.Add(new ViewFilterFailure(filter.Name, configureFailed));
                        }
                    }

                    if (viewModified) { viewsModified++; }
                }

                trans.Commit();
            }

            runLogs.Add($"Evaluated {totalViews} views.");
            runLogs.Add($"Modified {viewsModified} views with {filtersAdded} new filter additions and {filtersConfigured} filter settings applied.");
            if (filtersCreated > 0) runLogs.Add($"Created {filtersCreated} missing filters in the document.");
            if (filtersNotFound > 0) runLogs.Add($"{filtersNotFound} filter lookups had no matching filter or exemplar in the project.");
            if (viewsSkipped > 0) runLogs.Add($"Skipped {viewsSkipped} views whose names do not start with a plot id.");
            if (viewsBlocked > 0) runLogs.Add($"Left {viewsBlocked} views alone because their templates own the filters setting.");

            return new Output
            {
                ViewsEvaluated = totalViews,
                ViewsModified = viewsModified,
                FiltersAdded = filtersAdded,
                FiltersConfigured = filtersConfigured,
                FiltersCreatedInDoc = filtersCreated,
                FiltersNotFoundInDoc = filtersNotFound,
                Skipped = viewsSkipped,
                Blocked = viewsBlocked,
                Logs = runLogs.ToArray(),
                Failures = failures.ToArray()
            };
        }

        /// <summary>
        /// The read only half of the run, for the pane's grid. It opens no transaction and
        /// writes nothing, and everything it decides goes through the same Core rules and
        /// the same collectors Run reads, so the grid and the run cannot part.
        /// </summary>
        public ViewFilterScanResult Scan(Inputs input)
        {
            Document doc = _document;

            string[] viewKeywords = ViewKeywords.Split(input.ViewKeywords);
            FilterConfig[] filterConfigs = FilterRows.Kept(input.Filters);

            List<View> targetViews = viewKeywords.Length == 0
                ? new List<View>()
                : CollectTargetViews(doc, viewKeywords);
            List<ParameterFilterElement> allFilters = CollectAllFilters(doc);

            List<ScannedFilterView> views = new List<ScannedFilterView>();
            foreach (View v in targetViews)
            {
                bool blocked = TemplateOwnsTheFilters(doc, v, out string templateName);
                views.Add(new ScannedFilterView(v.Name, blocked, templateName));
            }

            List<string> filterNames = allFilters.Select(f => f.Name).ToList();

            List<ExemplarState> exemplars = new List<ExemplarState>();
            foreach (FilterConfig cfg in filterConfigs)
            {
                ParameterFilterElement exemplar = allFilters.FirstOrDefault(f =>
                    ViewFilterNames.HasPrefix(f.Name, cfg.Prefix));

                if (exemplar == null)
                {
                    exemplars.Add(ExemplarState.None(cfg.Prefix));
                    continue;
                }

                // The clone is tried against the exemplar's own tail so a Will create cell is
                // a create the run's rules guard will let through. Cloning builds managed rule
                // objects only, nothing in the document, so no transaction is needed.
                string exemplarPlotCode = ViewFilterNames.ExemplarPlotCode(exemplar.Name, cfg.Prefix);
                ElementFilter held = exemplar.GetElementFilter();
                bool clonable = held != null
                    && CloneAndReplaceFilter(held, exemplarPlotCode, exemplarPlotCode) != null;

                exemplars.Add(ExemplarState.Of(cfg.Prefix, exemplar.Name, clonable));
            }

            return ViewFilterScanPlan.Of(doc.Title, views, filterNames, exemplars);
        }

        private static List<View> CollectTargetViews(Document doc, string[] viewKeywords)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(v => !v.IsTemplate && v.HasViewDiscipline())
                .Where(v => viewKeywords.Any(k => v.Name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }

        private static List<ParameterFilterElement> CollectAllFilters(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .ToList();
        }

        /// <summary>
        /// The fourth edit's question: does this view's template own the filters setting. A
        /// template parameter its view does not control is one the template controls, which
        /// is how Revit words it, so the filters setting missing from the not controlled
        /// list means writes to this view's filters will throw.
        /// </summary>
        private static bool TemplateOwnsTheFilters(Document doc, View v, out string templateName)
        {
            templateName = string.Empty;

            ElementId templateId = v.ViewTemplateId;
            if (templateId == null || templateId == ElementId.InvalidElementId) return false;

            View template = doc.GetElement(templateId) as View;
            if (template == null) return false;

            templateName = template.Name;
            ICollection<ElementId> notControlled = template.GetNonControlledTemplateParameterIds();
            return !notControlled.Contains(new ElementId(BuiltInParameter.VIS_GRAPHICS_FILTERS));
        }

        private static FilterRule CloneAndReplaceRule(FilterRule rule, string oldVal, string newVal)
        {
            if (rule is FilterInverseRule fir)
            {
                var inner = fir.GetInnerRule();
                var newInner = CloneAndReplaceRule(inner, oldVal, newVal);
                return new FilterInverseRule(newInner);
            }
            else if (rule is FilterStringRule fsr)
            {
                string currentVal = fsr.RuleString;
                string updatedVal = currentVal;
                if (string.Equals(currentVal, oldVal, StringComparison.OrdinalIgnoreCase))
                {
                    updatedVal = newVal;
                }
                else if (!string.IsNullOrEmpty(oldVal) && currentVal.IndexOf(oldVal, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    int idx = currentVal.IndexOf(oldVal, StringComparison.OrdinalIgnoreCase);
                    updatedVal = currentVal.Substring(0, idx) + newVal + currentVal.Substring(idx + oldVal.Length);
                }
                return new FilterStringRule(new ParameterValueProvider(fsr.GetRuleParameter()), fsr.GetEvaluator(), updatedVal);
            }
            return rule;
        }

        private static ElementFilter CloneAndReplaceFilter(ElementFilter filter, string oldVal, string newVal)
        {
            if (filter == null) return null;
            if (filter is LogicalAndFilter laf)
            {
                var subFilters = laf.GetFilters().Select(sf => CloneAndReplaceFilter(sf, oldVal, newVal)).Where(sf => sf != null).ToList();
                if (subFilters.Count == 0) return null;
                if (subFilters.Count == 1) return subFilters[0];
                return new LogicalAndFilter(subFilters);
            }
            else if (filter is LogicalOrFilter lof)
            {
                var subFilters = lof.GetFilters().Select(sf => CloneAndReplaceFilter(sf, oldVal, newVal)).Where(sf => sf != null).ToList();
                if (subFilters.Count == 0) return null;
                if (subFilters.Count == 1) return subFilters[0];
                return new LogicalOrFilter(subFilters);
            }
            else if (filter is ElementParameterFilter epf)
            {
                var rules = epf.GetRules();
                var newRules = new List<FilterRule>();
                foreach (var r in rules) { newRules.Add(CloneAndReplaceRule(r, oldVal, newVal)); }
                return new ElementParameterFilter(newRules);
            }
            return null;
        }

        /// <summary>
        /// What Logger.AppendLine reaches: each line goes to the handler as it happens, which
        /// collects for the report and shows it on the pane's log list.
        /// </summary>
        private sealed class RunLog
        {
            private readonly Action<string> _forward;

            public RunLog(Action<string> forward)
            {
                _forward = forward;
            }

            public void AppendLine(string line)
            {
                _forward?.Invoke(line ?? string.Empty);
            }
        }
    }
}
