# 03 your steps

One pass in three parts: the rebuilt Setup.exe the way the team will run it, the folder layout
and the region areas on NG05 in Revit 2024, then your own full install back. Take one screenshot
at every step marked Screenshot. The Setup.exe is the one file named `RCRC Green KPI Setup` and a
version in `C:\Users\bader\Documents\RCRC-Green-Team-Kit`, built from main after the folder
layout round.

## Part 1, install as the team will, no VS Code

1. Close every Revit window.
   Look for: no Revit in the task bar.
2. Put the Setup.exe on the project Teams or SharePoint folder, then download it from there.
   Look for: `RCRC Green KPI Setup` and the version in your Downloads folder.
3. Double click it. If a blue box says Windows protected your PC, choose More info, then Run
   anyway.
   Look for: the Welcome page.
4. Press Next, then Install on the Ready to Install page.
   Look for: `This installs RCRC Green KPI` and the version. Screenshot.
5. On the last page, press Finish.
   Look for: `Start Revit 2024. When it asks about RCRC Green, choose Always Load.`

## Part 2, NG05 in Revit 2024

6. Start Revit 2024 and open NG05. If it asks about RCRC Green, choose Always Load.
   Look for: the question or none.
7. Open the RCRC Green tab and press KPI Checklist, then open step 1 Setup.
   Look for: Folder layout with Flat and Grouped, neither ticked. Screenshot.
8. Open step 2 and press Read model.
   Look for: Templates matched `11 of 11`.
9. In step 3 tick three plots: the one where you met `Two regions hold an area. Pick one.`, one
   DAILY or FRIDAY MOSQUE MORE or LESS THAN 60% plot, and one NH STREET or WALKWAY plot.
   Open step 4.
   Look for: Create greyed and the line `Pick a folder layout in Setup`. Screenshot.
10. Open step 1 and tick Grouped. Hold the mouse over it.
    Look for: Grouped ticked, step 1 marked done, and the tooltip with
    `Output\FRIDAY MOSQUE\FRIDAY MOSQUE MORE THAN 60%\<UID2>`.
11. Open step 4 and press Create.
    Look for: 3 created and 0 failed, with no region question. Screenshot.
12. Press Open report.
    Look for: the second line `Folder layout: Grouped.` and, in the region table, that plot's
    row reading `same area on 2 regions,` and both types. Screenshot.
13. Press Open output folder.
    Look for: `<value> MOSQUE\<value> MOSQUE ... THAN 60%\<UID2>\` and `STREETS\NH STREET
    20m ROW\<UID2>\` or `STREETS\WALKWAY\<UID2>\`, each with an .xlsx and a PDF. Screenshot.
14. In step 3 tick a plot whose two regions hold different areas, if NG05 holds one, and press
    Create. If none does, write that down and go to step 19.
    Look for: under Failed, one line per region, the type and its area in m2, with Show beside
    each. Screenshot.
15. Open a floor plan that shows the 00 link around that plot, then press Show on the first line.
    Look for: the region selected and zoomed to, status `Selected <type>`, or `Open a plan that
    shows the 00 link`. Hold the mouse over the status line and write down its reason, Q17.
    Screenshot.
16. Press Show on the second line.
    Look for: the view moves to the other region. Screenshot.
17. Open a sheet and press Show again.
    Look for: `Open a plan that shows the 00 link`, nothing selected, the sheet not moved.
18. Press the line of the region the plot's area belongs to.
    Look for: the plot created on that press.
19. Open step 1 and tick Flat. In step 3 tick only the mosque 60 per cent plot, then Create in
    step 4 and Open output folder.
    Look for: `<value> MOSQUE ... THAN 60%\<UID2>\` straight under the output folder, and
    the report's second line `Folder layout: Flat.` Screenshot.
20. Close Revit and start it again, open NG05 and KPI Checklist, step 1.
    Look for: Flat still ticked.

## Part 3, uninstall and your full install back

21. Close Revit, open Windows Settings, Apps, Installed apps, search RCRC, and choose Uninstall
    on RCRC Green KPI.
    Look for: `RCRC Green KPI` gone after the press.
22. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
    Look for: the newest commit names the folder layout.
23. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`, then Terminal,
    New Terminal.
    Look for: a prompt ending in `RCRC-Green>`.
24. Build and install your full build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
.\install\install.ps1
```

   Look for: `0 Error(s)`, then a line ending `files copied. Restart Revit 2024 and look for
   the RCRC Green tab.`
25. Start Revit 2024, open KPI Checklist, step 1, tick the layout you want, then send the
    screenshots and both reports.
    Look for: Flat and Grouped offered. Write down whether one was already ticked before you
    picked, which says whether the uninstall left the setting behind.
