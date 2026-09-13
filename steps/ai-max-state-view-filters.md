# ai-max state, View Filters

Phase: 9, ship. First pass. The task exists whole: `RcrcGreen.Core/ViewFilters` holds the
plot id rule, the name building and matching, the hex parse, the keyword split, the row
normalisation, the scan plan, the apply gate, the settings file reader and the report, all
with tests, 75 of them, 1456 to 1531, 0 failed and 0 skipped, build 0 warnings.
`RcrcGreen.Revit/ViewFilters` holds the ported runner, the handler with its own external
event, the pane on its own identifier `e3d618f2-da92-4cc1-b868-15cd58ed79c5`, the show
command and the settings store. The run body is the argus host's with four edits, the plot
id skip, the two create guards, keep existing overrides, and the blocked view list, plus
the host swaps, and every line that differs beyond those is named in
`steps/log-view-filters.md`. The ribbon carries a third panel, View Filters, one button.
`install/ViewFilters.json` ships the four default rows and install.ps1 copies it beside
the DLL. The scan and the run read one set of rules, Core's, so the grid cannot part from
the press. No cancel exists because the KPI progress window this reuses has none, by that
task's recorded decision, and the probe stands ready. Nothing in this pass has been
observed in Revit. The round guide for Bader is `steps/2026-09-13-view-filters.md`. The
round ships as pull request #108, and the pane's mockup, hand drawn from the code, is
`design/pr-108/view-filters-pane.html`.
