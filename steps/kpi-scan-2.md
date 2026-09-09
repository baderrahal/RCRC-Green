# Running the KPI scan again

Build, install and run the scan with this round's four additions. VS Code run order, one
action per step, exact command per step.

## Build and install

1. Open VS Code.
2. File menu, Open Folder, pick the RCRC-Green repo folder, press Select Folder.
3. Terminal menu, New Terminal. The prompt opens at the repo root.
4. Get the round:

   ```
   git pull origin main
   ```

5. Run the tests before installing, so a broken pull is caught first:

   ```
   dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
   ```

   Every test passes or you stop here and say so.

6. Build the solution in Release:

   ```
   dotnet build RcrcGreen.sln -c Release
   ```

   0 warnings and 0 errors, or you stop here and say so.

7. Close Revit 2024. Windows will not replace an assembly that is loaded.
8. Install:

   ```
   .\install\install.ps1
   ```

## Check the recognition fix

9. Start Revit 2024 and open the NG05 model.
10. On the RCRC Green tab press KPI Checklist.
11. In the template block press Browse and pick the folder holding the seven templates.
12. Check the line above the list now reads 7 workbooks in the folder, 7 recognised. It read
    0 recognised before this round, because the map had the angle brackets stripped off the
    sheet names.
13. Click the EXISTING PARKS file and check the would fill lines still name D3, C5, E4, D8,
    F11 and H11 and the tree ranges B4 to B92 and B4 to B84.

## Run the scan

14. Press KPI Scan in the top strip and wait for the status line to report.
15. Open the report. It is on your Desktop and in the repo's reports folder, named
    RCRC-Green-KPI_ followed by the model name and the time.

## What to check in the report, all new this round

16. Section 3, under PRX_COMPONENT NOT FOUND: check the near miss PRX_Component is now
    followed by its values, up to twenty, with sheet number and sheet name. It was named and
    never shown before.
17. Section 3, at the end: check THE FOUR PLOT PARAMETERS ON THE SHEET, SIDE BY SIDE prints
    one row per sheet with PRX_Plot_ID, PRX_Plot_UID, PRX_Plot_UID2 and PRX_Plot_NH in four
    columns. This is what settles which one the workbook's Ref means.
18. Section 4: check each filled region row now carries its plot beside the type and the
    area, that REGIONS OF EACH TYPE CARRYING PRX_Ref Plot ID prints both counts per type, and
    that ONE PLOT'S REGIONS TOGETHER lists up to three plots with their regions under them.
19. Sections 5, 6 and 8: check three plots of each schedule name are now read in full rather
    than one, so the group headings can be compared across plots.
20. Answer the five open questions in the log entry from what you see, and send the numbers
    back. The next round fills the workbook and needs them.
