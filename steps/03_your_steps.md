# 03 your steps

This pass checks the redesigned KPI pane in Revit 2024 against the approved design. It needs the
build from main after the KPI pane redesign merged. The repo on this machine is at
`C:\Users\bader\Documents\GitHub\RCRC-Green`. Use your own clone's folder if it is elsewhere.

1. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
   Look for: the newest commit is the KPI pane redesign merge.
2. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`, then Terminal,
   New Terminal. Close Revit, then build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   Look for: `0 Warning(s)` and `0 Error(s)`.
3. Install:

```
.\install\install.ps1
```

   Look for: `Restart Revit 2024 and look for the RCRC Green tab.`
4. Open Revit 2024 and NG05, then RCRC Green tab, KPI Checklist. Drag the pane to about 300 px
   wide. Look for: KPI Checklist, the model name, a Build line, four circles Setup Read Plots
   Create, nothing running off the right edge, no sideways scroll.
5. Step 1. Look for: Folders, Files and Workbook details as upper case grey titles, a green
   tick on each set path, `7 templates found`, a red edge and Not set on any empty box except
   Plot list, the footer Next: Read with All set or a count to set.
6. Press Next: Read, then Read model. Look for: Plots, Elements and Time boxes, In plot list
   and Templates matched lines, the three Values from dropdowns, one warning box per problem,
   and Read at with the time in the status line.
7. Step 3. Tick Mosques. Look for: about 20 plots ticked with the count on the Mosques box, the
   plot ids in Consolas, the asset type in grey on the right, any plot with No type or Shared
   id greyed, and the scroll staying where it was after a tick.
8. Type DM in the search box, then press Plot list. Look for: the list narrowed to DM plots,
   and Plot list ticking exactly the listed plots.
9. Next: Create. Look for: the big number, the asset type table with ranges, the warning boxes,
   Files with the same name are replaced, the button Create N workbooks, and Nothing in the
   model changes under it. FM-08 ticked should not count in N.
10. Press Create. Look for: the progress window reading Reading i of n, then Creating i of n,
    with the plot id on a second line.
11. The result. Look for: every circle ticked, the banner, Created and Failed boxes, a row per
    asset type, a Failed line per plot in a few words, Open output folder and Open report
    working, and Retry failed only when something failed and a retry can help.
12. Hover over a greyed button, a warning box and a path box. Look for: a tooltip with the
    reason or the full text. Then send a screenshot of each step and the result, and the
    report's `BEFORE THE PRESS` section, to the chat.

## Not covered by this pass

- The Revit dark theme, which is question 1 in `steps/02_questions.md`
- A pane wider than 400 px
