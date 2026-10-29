# 03 your steps

One pass in three parts: the Setup.exe the way the team will run it, the rules line and two
Creates on NG05 in Revit 2024, then your own full install back. Take one screenshot at every step
marked Screenshot. The Setup.exe is the one file named `RCRC Green KPI Setup` and a version in
`C:\Users\bader\Documents\RCRC-Green-Team-Kit`, built from main after the component rules round.

## Part 1, install as the team will, no VS Code

1. Close every Revit window.
   Look for: no Revit in the task bar.
2. Put the Setup.exe on the project Teams or SharePoint folder, then download it from there.
   Look for: `RCRC Green KPI Setup` and the version in your Downloads folder.
3. Double click it in Downloads. If a blue box says Windows protected your PC, choose More info,
   then Run anyway.
   Look for: the Welcome page. Screenshot of the blue box if it shows.
4. On the Welcome page, press Next.
   Look for: `This installs RCRC Green KPI` and the version. Screenshot.
5. On the Ready to Install page, press Install.
   Look for: what is installed now named on the page. Screenshot before the press.
6. On the last page, press Finish.
   Look for: `Start Revit 2024. When it asks about RCRC Green, choose Always Load.`

## Part 2, NG05 in Revit 2024

7. Start Revit 2024 and open NG05. If it asks about RCRC Green, choose Always Load.
   Look for: the question or none.
8. Open the RCRC Green tab and press KPI Checklist.
   Look for: the header line `Build` and the version. Screenshot.
9. Open step 1 Setup.
   Look for: `11 templates found` and no box naming component-rules.csv. Screenshot.
10. Open step 2 and press Read model.
    Look for: Templates matched `11 of 11` and a line `<n> values placed by rule`. Screenshot.
11. Hold the mouse over that line.
    Look for: one value a line with its rule, such as `WALKWAY: rule WALKWAY, STREETS, folder
    STREETS`. Write down any value an Unknown value box shows. Screenshot.
12. Open step 3.
    Look for: the rows and their plot counts. Note whether NG05 holds a FRIDAY MOSQUE MORE or
    LESS THAN 60% plot, and an NH STREET or WALKWAY plot. Screenshot.
13. Tick one FRIDAY MOSQUE 60 per cent plot and one NH STREET or WALKWAY plot, whichever NG05
    holds, then press Create in step 4.
    Look for: 2 created and 0 failed, or 1 of each kind NG05 holds. Screenshot.
14. Press Open output folder.
    Look for: a folder named exactly as the FRIDAY MOSQUE value, and `STREETS`, each with a plot
    folder holding an .xlsx. Screenshot.
15. Open the mosque workbook and click D3, then H31.
    Look for: D3 holds the full value as the model spells it. H31 reads row 33 of Green Strategy
    KPI's for MORE THAN 60 and row 34 for LESS THAN 60. Screenshot on H31.
16. Open the street workbook and click D3, D8 and F8.
    Look for: D3 holds the full value. D8 and F8 hold the road width and the length, or are
    empty where the plot is not in the Scope Validation file. Screenshot.
17. Open the PDFs in the two plot folders.
    Look for: the component field holds the full value. A street plot whose prefix is not NS,
    ST or MM has no PDF. One screenshot per PDF.
18. Press Open report and find WHERE EACH COMPONENT VALUE WAS READ.
    Look for: each value with `rule` and its words and the component-rules.csv path, and one
    line naming a street plot missing from the Scope Validation file or a prefix with no form,
    if either happened. Screenshot.

## Part 3, uninstall and your full install back

19. Close Revit, open Windows Settings, Apps, Installed apps, search RCRC, and choose Uninstall
    on RCRC Green KPI.
    Look for: `RCRC Green KPI` with the version before the press, and gone after.
20. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
    Look for: the newest commit names the component rules.
21. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`.
    Look for: `install\component-rules.csv` in the Explorer.
22. Terminal, New Terminal.
    Look for: a prompt ending in `RCRC-Green>`.
23. Build and install your full build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
.\install\install.ps1
```

   Look for: `0 Error(s)`, then a line ending `files copied. Restart Revit 2024 and look for
   the RCRC Green tab.`
24. Start Revit 2024 and open the RCRC Green tab, then send the screenshots and the report.
    Look for: the Drawing Sheet, KPI and View Filters panels. Screenshot.
