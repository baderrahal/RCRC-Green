# 03 your steps

One pass in three parts: the Setup.exe the way the team will run it, the pane and the four 60 per
cent values on NG05 in Revit 2024, then your own full install back. Take one screenshot at every
step marked Screenshot. The Setup.exe is the one file named `RCRC Green KPI Setup` and a version
in `C:\Users\bader\Documents\RCRC-Green-Team-Kit`, built from main after this round merged.

## Part 1, install as the team will, no VS Code

1. Close every Revit window.
   Look for: no Revit in the task bar.
2. Put the Setup.exe on the project Teams or SharePoint folder, where the team will find it.
   Look for: `RCRC Green KPI Setup` and the version in the folder list.
3. Download it from there with the browser.
   Look for: the file in your Downloads folder.
4. Double click it in Downloads. If a blue box says Windows protected your PC, choose More info,
   then Run anyway.
   Look for: the box or no box, then the Welcome page. Screenshot of the box if it shows.
5. On the Welcome page, press Next.
   Look for: `This installs RCRC Green KPI` and the version, `for you alone`. Screenshot.
6. On the Ready to Install page, press Install.
   Look for: `Installed now:` and `an RCRC Green build with no VERSION.txt`, then `The full RCRC
   Green install, with all three tools, is here.` and `Setup replaces it with KPI only.`
   Screenshot before the press.
7. On the last page, press Finish.
   Look for: `Start Revit 2024. When it asks about RCRC Green, choose Always Load.` Screenshot.

## Part 2, NG05 in Revit 2024

8. Start Revit 2024 and open NG05. If it asks about RCRC Green, choose Always Load.
   Look for: the question or none. Screenshot of the question if it shows.
9. Open the RCRC Green tab.
   Look for: the KPI panel and its one button, KPI Checklist, and no Drawing Sheet or View
   Filters. Screenshot.
10. Press KPI Checklist.
    Look for: the header line `Build` and the version, the time with its colon. Screenshot.
11. Open step 1 Setup.
    Look for: `11 templates found`, the four 60 per cent files among them, and no New template
    box. Screenshot.
12. Open step 2 before pressing anything.
    Look for: `Reads sheets and schedules. Nothing changes.` Screenshot.
13. Press Read model.
    Look for: Templates matched `11 of 11`. Screenshot.
14. Open step 3.
    Look for: rows Mosques >60%, Mosques <60%, Schools >60% and Schools <60%, each with a plot
    count. Write the four counts down. Screenshot.
15. Tick one plot of each of those four and one DAILY MOSQUE or FRIDAY MOSQUE plot, then press
    Create in step 4.
    Look for: 5 created and 0 failed. Screenshot.
16. Press Open output folder.
    Look for: `DAILY MOSQUE MORE THAN 60%`, `DAILY MOSQUE LESS THAN 60%`, `SCHOOL MORE THAN 60%`,
    `SCHOOL LESS THAN 60%` and the old mosque folder, each with a plot folder holding an .xlsx
    and a .pdf. Screenshot.
17. Open each of the five workbooks and click D3, then H31 to H35 in turn.
    Look for: D3 holds the full value as the model spells it. H31 to H35 read row 33 of Green
    Strategy KPI's for mosques over 60, row 34 for mosques under 60 and the old mosque, row 35
    for schools over 60 and row 36 for schools under 60. One screenshot per workbook, on H31.
18. Open each of the five PDFs.
    Look for: the component field holds the full value. One screenshot per PDF.
19. Press Open report.
    Look for: the first line reads `Build:` and the same version as the header. Screenshot.

## Part 3, uninstall and your full install back

20. Close Revit, open Windows Settings, Apps, Installed apps, search RCRC, and choose Uninstall
    on RCRC Green KPI.
    Look for: `RCRC Green KPI` with the version before the press, and gone after. Screenshot
    before.
21. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
    Look for: the newest commit names the KPI Setup.exe.
22. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`.
    Look for: `install\team\RCRC Green KPI Setup.iss` in the Explorer.
23. Terminal, New Terminal.
    Look for: a prompt ending in `RCRC-Green>`.
24. Build and install your full build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
.\install\install.ps1
```

   Look for: `0 Error(s)`, then `13 files copied`.
25. Start Revit 2024 and open the RCRC Green tab, then send the screenshots and the report.
    Look for: the Drawing Sheet, KPI and View Filters panels. Screenshot.
