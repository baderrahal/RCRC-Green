# 03 your steps

One pass in three parts: the rebuilt Setup.exe the way the team will run it, NG05 in Revit 2024,
then your own full install back. Take one screenshot at every step marked Screenshot. The
Setup.exe is the one file named `RCRC Green KPI Setup` and a version in
`C:\Users\bader\Documents\RCRC-Green-Team-Kit`, built from main after the slash, PDF form and
walkway round.

## Part 1, install as the team will, no VS Code

1. Close every Revit window.
   Look for: no Revit in the task bar.
2. Put the Setup.exe on the project Teams or SharePoint folder, then download it from there.
   Look for: `RCRC Green KPI Setup` and the version in your Downloads folder.
3. Double click it. If a blue box says Windows protected your PC, choose More info, then Run
   anyway. Press Next, then Install, then Finish.
   Look for: `This installs RCRC Green KPI` and the version on the Ready page. Screenshot.

## Part 2, NG05 in Revit 2024

4. Start Revit 2024 and open NG05. If it asks about RCRC Green, choose Always Load.
   Look for: the question or none.
5. Open the RCRC Green tab and press KPI Checklist, then open step 1 Setup.
   Look for: Folder layout with Flat and Grouped, neither ticked. Screenshot.
6. Open step 2 and press Read model.
   Look for: Templates matched `11 of 11`.
7. In step 3 tick four plots: a WALKWAY plot, an NH STREET plot, a plot holding an Acacia /
   Vachellia or Acacia saligna tree, and a DAILY or FRIDAY MOSQUE MORE or LESS THAN 60% plot.
   Write down each one's plot ID and the walkway's UID2. Open step 4.
   Look for: Create greyed and the line `Pick a folder layout in Setup`. The walkway is not
   named as getting no PDF. Screenshot.
8. Open step 1 and tick Grouped.
   Look for: Grouped ticked and step 1 marked done.
9. Open step 4 and press Create.
   Look for: 4 created and 0 failed. Screenshot.
10. Press Open report and search for the slash tree's name, Vachellia or saligna.
    Look for: its row in the tree table reading `matched on its own name`, with its count, and
    no line for it under the species written into an empty row. Write down how Revit spells
    the name, with or without spaces round the slash, Q20. Screenshot.
11. In the same report, search for the walkway's plot ID under ONE PDF PER PLOT.
    Look for: `Projects Basic Data - Roads`, Row written, Length left blank with `is no length
    and the total length is left empty`, and no Project Type line, Q21. Screenshot.
12. Search for the NH STREET plot under ONE PDF PER PLOT.
    Look for: Roads, with Row and Length both written.
13. Press Open output folder.
    Look for: `STREETS\WALKWAY\<UID2>\` and `STREETS\NH STREET ...\<UID2>\` and the mosque
    under `<value> MOSQUE\<value> MOSQUE ... THAN 60%\<UID2>\`, each with an .xlsx and a PDF.
    Screenshot.
14. Open the walkway's workbook, first sheet.
    Look for: D3 reads WALKWAY, D8 Streets ROW (m) holds the road width, F8 Streets Total
    Length (m) is empty, Q22. Screenshot.
15. Open the NH STREET plot's workbook, first sheet.
    Look for: D8 and F8 both filled.
16. Open the walkway's PDF.
    Look for: the Roads form, its UID and its Row filled, no Project Type box on the page.
    Screenshot.
17. Open the slash tree plot's workbook, the Tree List sheet its group points at.
    Look for: the count in column B on the client's own Acacia row, not a new row at the
    bottom of the list. Screenshot.
18. If NG05 holds a plot whose two regions hold different areas, tick it alone in step 3 and
    press Create. If none does, write that down and go to step 20.
    Look for: one line per region, the type and its area in m2, with Show beside each.
19. Open a floor plan that shows the 00 link around that plot, then press Show on the first
    line.
    Look for: the region selected and zoomed to, or `Open a plan that shows the 00 link`. Hold
    the mouse over the status line and write down its reason, Q17. Screenshot.
20. Open step 1 and tick Flat. In step 3 tick only the mosque plot, then Create and Open output
    folder.
    Look for: `<value> MOSQUE ... THAN 60%\<UID2>\` straight under the output folder.

## Part 3, uninstall and your full install back

21. Close Revit, open Windows Settings, Apps, Installed apps, search RCRC, and choose Uninstall
    on RCRC Green KPI.
    Look for: `RCRC Green KPI` gone after the press.
22. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
    Look for: the newest commit names the slash, the PDF form and the walkway.
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
    screenshots, both reports and your answers to Q17 and Q20 to Q22.
    Look for: Flat and Grouped offered.
