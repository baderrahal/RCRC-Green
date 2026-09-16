# Building, installing and running View Filters

The first install of the View Filters panel, 2026-09-13. One action per numbered step, the
exact command where a command is needed, and what passing reads like after it. A mismatch
on screen is a finding about the tool, not a slip in the reading: write it down and stop.

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

5. Run the tests:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

   Every test passes or you stop here and say so. Main carries 1569.

6. Build the add-in, which builds Core with it:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   0 warnings and 0 errors.

   The tests run on the GitHub test gate, on .NET 10, so this build does not need the .NET 10
   SDK. The test step above this one does, because `tests/RcrcGreen.Core.Tests` targets .NET 10
   and `dotnet test` on it stops with `error NETSDK1045` without that SDK.

7. Install, which copies the DLLs, the addin file and ViewFilters.json into the Revit 2024
   add-ins folder:

```
.\install\install.ps1
```

   The printed list ends with a count and must include a line ending in
   `RcrcGreen\ViewFilters.json`. If it does not, the defaults did not ship and the pane
   will say so.

8. Start Revit 2024.

## Open the model and the pane

9. Open a model with plot views in it.

10. On the ribbon, open the **RCRC Green** tab. It now holds three panels: **Drawing
    Sheet**, **KPI**, and **View Filters** with one button, **View Filters**. If the third
    panel is missing, the install is stale, go back to step 7.

11. Press **View Filters**. The pane opens docked on the right.

12. Read the FILTER ROWS block. The line above the rows says they came from
    ViewFilters.json beside the installed add-in, and four rows are filled:
    `(200-260) Presentation`, `(200-260) Sections Scope`, `(200-260) Intervention Limit`
    and `(215) Borders Edging`. Every override tick is off, every weight is 0, every
    pattern says none and halftone is off.

## Scan, which reads and changes nothing

13. Leave the keyword box as it is, three lines: Location Key Plan, Overall Key Plan,
    General Arrangement Layout.

14. Press **Scan**.

15. Read the grid. Rows are plots, columns are the four prefixes, and every square says
    **Exists**, **Will create** or **Cannot create**. Under it sit the two lists, Skipped
    with views whose names carry no plot id at the front, and Blocked with views whose
    templates own the filters setting, each with its count.

16. Check one **Exists** square against Revit: open that plot's view, open Visibility and
    Graphics, Filters, and the filter named by the square's tooltip is on the list.

## Apply, which writes once

17. Apply is now usable. Change anything, a keyword or any row, and it greys out with
    **Scan again** under it, which is correct. Put the change back or scan again.

18. Press **Apply**. A progress window counts the views. Its title bar says RCRC Green
    KPI, which is a known blemish of this round, recorded in the log: the window is KPI's
    and its title lives in KPI's file.

19. Read the RESULTS block: views evaluated and modified, filters added, configured,
    created, the lookups with nothing to use, skipped and blocked, and under them the
    run's own log lines.

20. Check one changed view in Revit the same way as step 16, and check a **Will create**
    square became a real filter named prefix, space, plot id.

21. Press undo once. The whole run comes off in that one undo, named
    **RCRC Green - View Filters**.

22. The report is in the repo's `reports` folder, named
    `RCRC-Green-ViewFilters_<model>_<date>_<time>.txt`, and holds the same counts and the
    same lines the pane showed. Nothing under `reports/` is ever committed.
