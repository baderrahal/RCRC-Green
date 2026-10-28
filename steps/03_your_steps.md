# 03 your steps

This pass checks the KPI pane in Revit 2024 against the approved design, after the round that
answered Q1 to Q5. Take one screenshot at every step from 6 on, numbered by its step.

1. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
   Look for: the newest commit names Q1 to Q5 and the records split.
2. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`.
   Look for: `CLAUDE.md`, `docs` and `steps` in the Explorer on the left.
3. Close Revit, then in VS Code open Terminal, New Terminal.
   Look for: a prompt ending in `RCRC-Green>`.
4. Build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   Look for: `0 Warning(s)` and `0 Error(s)`.
5. Install:

```
.\install\install.ps1
```

   Look for: `Restart Revit 2024 and look for the RCRC Green tab.`
6. Open Revit 2024 and NG05, then RCRC Green tab, KPI Checklist, and drag the pane to about
   300 px wide. Look for: KPI Checklist, the model name, a Build line, four circles Setup Read
   Plots Create, no sideways scroll.
7. Step 1, Setup. Look for: Folders, Files and Workbook details as grey upper case titles, a
   green tick on each set path, `7 templates found`, the footer Next: Read.
8. Press Next: Read and do not press Read model yet. Look for: one grey line in the body,
   `Reads sheets and schedules. Nothing changes.`, the footer Read model and `Not read yet`.
   This is the Q4 answer.
9. Press Read model. Look for: Plots, Elements and Time boxes, In plot list, Templates matched,
   the three Values from dropdowns with square corners, and the Q4 line gone.
10. Step 3, Plots. Tick Mosques. Look for: about 20 plots ticked, the count on the Mosques box,
    the scroll staying put after a tick.
11. Press Next: Create. Look for: the big number, the asset type table, the button
    Create N workbooks, and Nothing in the model changes under it.
12. Press Create and wait for the result. Look for: every circle ticked, the banner, Created and
    Failed boxes, a row per asset type, and Open report opening the report.

Then send the screenshots, numbered by step, to the chat.

## Not covered by this pass

- The Revit dark theme, which stays light by Bader's answer to Q1
- A pane wider than 400 px
