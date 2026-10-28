# 03 your steps

This pass checks the four 60 per cent values and templates in Revit 2024 on NG05, on your own
full install. Take one screenshot at every step marked Screenshot. The kit pass of the round
before is still open and can wait until after this one.

1. In GitHub Desktop, pick RCRC-Green, branch main, press Fetch origin, then Pull origin.
   Look for: the newest commit names a new template on its layout.
2. In VS Code, File, Open Folder, `C:\Users\bader\Documents\GitHub\RCRC-Green`.
   Look for: `install\components.csv` in the Explorer.
3. Terminal, New Terminal.
   Look for: a prompt ending in `RCRC-Green>`.
4. Close every Revit window.
   Look for: no Revit in the task bar.
5. Build and install your full build:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
.\install\install.ps1
```

   Look for: `0 Error(s)`, then a list of copied files that includes `components.csv`.
6. Open the templates folder the KPI pane points at.
   Look for: 11 workbooks, the seven and the four ending MORE THAN 60 and LESS THAN 60.
7. Start Revit 2024, open NG05, press KPI Checklist, step 1 Setup.
   Look for: Templates `11 templates found`, and no New template or Unknown template layout
   box. Screenshot.
8. Step 2, press Read model.
   Look for: Templates matched `11 of 11`. An Unknown value box only for a value that is in no
   components.csv. Screenshot.
9. Step 3, the asset types.
   Look for: rows Mosques >60%, Mosques <60%, Schools >60% and Schools <60%, each with a plot
   count. Write the four counts down. Screenshot.
10. Tick one plot of each of the four values and nothing else, then step 4, press Create.
    Look for: 4 created and 0 failed. Screenshot.
11. Press Open output folder.
    Look for: four new folders, `DAILY MOSQUE MORE THAN 60%`, `DAILY MOSQUE LESS THAN 60%`,
    `SCHOOL MORE THAN 60%` and `SCHOOL LESS THAN 60%`, each holding one plot folder with an
    .xlsx and a .pdf. Screenshot.
12. Open each of the four workbooks, click D3, then H31.
    Look for: D3 holds the full value as the model spells it. The formula bar on H31 reads
    `='Green Strategy KPI''s'!E33` for mosques over 60, `E34` under, `E35` schools over,
    `E36` under. One screenshot per workbook.
13. Open each of the four PDFs.
    Look for: the component field holds the full value. One screenshot per PDF.
14. Press Open report.
    Look for: the block WHERE EACH COMPONENT VALUE WAS READ, naming each of the four with its
    template, its folder and `the shipped components.csv line 13` to `16`. Screenshot.
15. Answer Q8 to Q12 in `steps\02_questions.md` and send the screenshots and the report.
    Look for: an Answer line under each of the five.
