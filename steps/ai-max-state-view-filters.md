# ai-max state, View Filters

Phase: 9, ship. Second pass, the colour box. `HexColorBox` in the Revit ViewFilters folder,
one control used three times on every filter row, replacing the two bare pattern hex boxes
and the lone swatch beside Line colour. Typing paints live through `HexColor.TryParse`,
a pick comes back through `HexColor.Written` as upper case, an invalid type keeps the
stored value through `HexColor.Kept`, all three in Core with 9 new tests, 1569 to 1578,
0 failed and 0 skipped. The picker is the WinForms ColorDialog, FullOpen, owned by Revit's
main window handle, custom colours static for the session. The override ticks gate their
own boxes. The csproj already carried the WinForms references for the progress window, so
nothing was added there. The round asked for the control in Core as XAML and it cannot
live there, the same call as the first pass's runner, in the log. The reader was cleared
of the ticked box in the screenshot by the gate test over the shipped file. Nothing in
this pass has been observed in Revit. The guide is `steps/2026-09-13-colour-box.md`, the
pass ships as pull request #111, and the mockup is `design/pr-111/colour-box.html`.

Before that. First pass. The task exists whole: `RcrcGreen.Core/ViewFilters` holds the
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
round shipped as pull request #108, squash merged at `3bcebd5` with the message back byte
for byte, and the pane's mockup, hand drawn from the code, is
`design/pr-108/view-filters-pane.html`. The runner read 1569 on the merge, this round's 75
on top of a main the sixty third Drawing Sheet pass had moved to 1494 underneath it, and
merged main reads 1569 locally, 0 failed and 0 skipped.
