# Building, installing and running the View Filters rail

The third View Filters install, 2026-09-19: the pane re laid as a numbered rail down the
left, five steps, one open at a time. One action per numbered step, the exact command where
one is needed, and what passing reads like after it. A mismatch on screen is a finding
about the tool, not a slip in the reading: write it down and stop.

## Build and install

1. Open VS Code.

2. File, then Open Folder, and pick this repo:

```
C:\Users\bader\source\repos\RCRC-Green
```

3. Open the terminal: Terminal menu, then New Terminal. It opens at the repo root.

4. Pull the round:

```
git pull origin main
```

5. Open this pull request on GitHub and check the test gate shows a green tick:

```
https://github.com/baderrahal/RCRC-Green/pulls
```

   A green tick or you stop here and say so. **The tests run there, on .NET 10, so this
   PC needs no .NET 10 SDK.** What the count should read is in the top entry of
   `steps/log-view-filters.md`, which is where it is kept up to date.

6. Build the add-in, which builds Core with it:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   0 warnings and 0 errors.

7. Close Revit if it is open. The installer cannot replace a loaded assembly.

8. Install, which copies the DLLs, the addin file and ViewFilters.json into the Revit 2024
   add-ins folder:

```
.\install\install.ps1
```

9. Start Revit 2024, open a model with plot views in it, and press **View Filters** on the
   RCRC Green tab.

## What the pane should look like now

10. The rail. Down the left side sit five round cells, numbered 1 to 5, and nothing else.
    The strip at the top and the status line at the bottom are as they were. On a fresh
    pane with the shipped defaults, 1 and 2 already show a tick in place of their number,
    because the keywords and the rows are filled, and cell 3 is the filled one, since the
    scan is the first step with work left in it. Cells 4 and 5 read muted.

11. Only the open step shows its controls. Beside the rail sits one bold header line, the
    number, the title and its summary, and under it that one step's controls. Nothing from
    any other step is on screen. The titles, cell by cell, are **KEYWORDS**, **FILTER
    ROWS**, **SCAN**, **APPLY** and **RESULTS**, read from the headers as you click along
    the rail.

12. Hover every cell without clicking. Each shows a tooltip: the title, then the summary
    when the step can be used, or the one line saying why it cannot. Cell 5 before any run
    reads `RESULTS   Nothing has run yet.` A cell with no tooltip at all is a finding: the
    rail shows numbers only, so the tooltip is the only place a cell says what it is.

13. Click cell 5. Nothing happens, which is correct before a run. Click cell 1: the
    KEYWORDS step opens, the cell fills, and the keyword box shows its three lines.

14. Drag the pane to about 300 pixels wide. The rail keeps its column, the body keeps a
    readable width beside it, and no sideways scroll bar appears anywhere except on the
    scan grid once one exists. Open step 2 and read a filter row: the tick line, the line
    colour line and the two pattern lines wrap onto more lines rather than running off
    the right edge.

15. Half type something. In step 1 add a fourth keyword but leave it unfinished, click
    cell 2, then click cell 1 again. The half typed text is still in the box, because the
    step bodies are built once and only shown and hidden.

## Scan, apply, results

16. In step 1, clear the keyword box entirely. Cells 2, 3 and 4 mute, and their tooltips
    say why, starting with `Type at least one keyword in step 1.` on cell 2. Put the three
    lines back, or press undo in the box, and the cells wake again with nothing else
    pressed. Then open step 3 and press **Scan**. When the grid comes back the pane lands
    on the first step with work left, which after a scan is APPLY, cell 3 shows a tick,
    and the header reads the scan's own counts, plots, skipped and blocked.

17. Change any row in step 2 after that scan. Cell 3 loses its tick, cell 4 mutes, and
    cell 4's tooltip reads `APPLY   Scan again`. Put the change back exactly: both
    recover, no rescan needed. This is the same compare the old pane made, now read off
    the rail.

18. Press **Apply** in step 4. The progress window counts the views, and when the run
    answers, the pane opens step 5 on its own. The header reads the two counts, `N
    applied, M not applied`, over the eight count lines the old results block showed,
    worded exactly as before. Each thing that was not applied prints on its own line with
    the run's own reason, in a bounded list when there are many. Under them, the report
    line, the **Open the report** button, and the run's log lines.

19. Press **Open the report**. The report opens in whatever opens text files on this
    machine. Delete the report file by hand and press the button again: the status line
    says it could not be opened and the pane stays up.

20. Switch the Revit theme, Light to Dark or back, under File, Options, User Interface,
    with the pane open. The rail cells repaint with everything else: no cell keeps the
    old theme's fill or a number in the old theme's colour.

21. Press undo once after a run that changed anything. The whole run comes off in that one
    undo, named **RCRC Green - View Filters**, exactly as before the rail.

## What this pass cannot show from the tests

The eight things below are exactly what steps 10 to 20 are for. None of them can be
reached from `tests/RcrcGreen.Core.Tests`, because Core holds no WPF and no Revit, so a
green gate says nothing about them:

- that only the open step's controls are visible, step 11
- that the rail fits its column and the body still reads at 300 pixels, step 14
- that the tooltips appear at all, which is the rail's only naming, step 12
- that a kept control still holds half typed text when its step reopens, step 15
- that the tick and the greying draw, steps 10, 16 and 17
- that the report button opens the file, step 19
- that the new rail cells repaint on a Revit theme switch, step 20
- the scan, the run and the template question, which all need a model open, steps 16 to 18
