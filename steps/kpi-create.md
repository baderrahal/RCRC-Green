# Building, installing and running Create

The plot picker and the Create button. VS Code run order, one action per step, exact command
per step, and what to check on screen.

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

   0 warnings and 0 errors.

7. Close Revit if it is open. The installer cannot replace a loaded assembly.

8. Install:

   ```
   .\install\install.ps1
   ```

   It builds the `Addins\2024\` layout the build does not.

## Point it at the templates and open a model

9. Start Revit 2024 and open `RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached.rvt`.

10. RCRC Green tab, KPI panel, press **KPI Checklist**. The pane opens on the right.

11. Check the top strip names the model. If it says No model open, close the pane and open it
    again.

12. Press **KPI Scan** once. Wait for the status line to name where the report went. This is
    not required for Create, but it is the fastest way to see the pane is talking to Revit.

13. In the template block, press **Browse** and pick the folder holding the seven GRP KPI
    Checklist workbooks. Check the line reads `7 workbooks, 7 recognised`. If any read
    unrecognised, stop and say which.

## One plot

14. Scroll to **Plots**. Check the line above the tick list says whether the sheets and the
    schedules name the same plots, and check the count line reads `0 of 160 plots ticked` or
    whatever the real number is.

15. Tick **FM-05** only. Check the count line reads `1 of 160 plots ticked`.

16. Check the template block above has preselected **MOSQUES**, and that the name box now
    offers something like `MOSQUES FRIDAY MOSQUE`. If it preselected nothing, read the line
    saying why and pick the template by hand.

17. Under **What the values come from**, check:
    - Component offers `PRX_Component` and it is bold
    - Reference offers all four plot names with FM-05's value beside each, and
      `PRX_Plot_UID2` is bold
    - Location offers `Neighborhood Name`

18. Type your name and position into **Prepared by** and **Position**. The date is already
    filled with today.

19. Press **Create**.

20. Read the status line. It says how many cells were written, from how many plots, how many
    were not written, where the workbook went and where the report went.

21. Open the workbook beside the model. Check D3, C5, E4, H7, F10 and H10 on the main sheet
    hold values, and that the two Tree List sheets hold quantities in column B.

22. Open the report. Check **RECONCILIATION** reads `plots ticked 1` and `plots read 1`, and
    that nothing is listed under plots that contributed nothing.

## Several plots

23. Tick **FM-06** and **FM-07** as well. Check the count line reads `3 of 160 plots ticked`.

24. Press **Create** again.

25. If the pane refuses, read why. Three refusals are expected in normal use:
    - a plot with more than one filled region holding an area. Press the button naming the
      region that is its intervention area
    - two plots reporting the same area. Check whether they really are the same size, then
      press **These areas are right, write anyway**
    - a plot ticked and not read. That one is a bug. Keep the report and say so

26. On a write, open the report and check the **AREA**, **SHRUBS** and **LAWN** tables. Each
    lists every plot's own number with the total underneath. Add the numbers up by eye. If a
    total does not equal its parts the report says so in capitals and the workbook was not
    written.

27. Check **SPECIES MATCHED**. Every row names the workbook row, the Revit name, the group and
    which plots the count came from. A species on two plots in one group shows both.

28. Check **SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT**. UNKNOWN should be here with
    its count, and so should anything carrying a slash or an apostrophe the list spells
    differently. Nothing in that list is written into the workbook.

## Every plot

29. Press **Select all**. Check the count line reads the full number of plots.

30. Press **Create**. This reads every plot, so give it time. Check the report's reconciliation
    names every plot that contributed nothing, with its reason.

## What to send back

31. The report file, the filled workbook, and one line saying which of steps 21, 26, 27 and 28
    did not read as described.
