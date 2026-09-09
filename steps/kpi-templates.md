# Running the KPI template picker

Build, install and check the third KPI round on your machine, in VS Code run order. One
action per step.

## Build and install

1. Open VS Code.
2. File menu, Open Folder, pick the RCRC-Green repo folder and press Select Folder.
3. Terminal menu, New Terminal. The prompt opens at the repo root.
4. Get the round:

   ```
   git pull origin main
   ```

5. Run the tests first, so a broken pull is caught before an install:

   ```
   dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
   ```

   Every test passes or you stop here and say so.

6. Build the whole solution in Release:

   ```
   dotnet build RcrcGreen.sln -c Release
   ```

   0 warnings and 0 errors, or you stop here and say so.

7. Close Revit 2024 if it is open, because Windows will not replace a loaded assembly.
8. Install:

   ```
   .\install\install.ps1
   ```

   The list it prints now ends with templates-folder.txt. A reinstall keeps the folder you
   set before, because the file is only created when it is missing.

## Put the templates somewhere

9. Make a folder anywhere outside this repo, for example C:\GRP\Templates. Never inside the
   repo, it is public and the ignore rules are the second fence, not the first.
10. Copy the seven production GRP KPI Checklist templates into it. The production set, not
    the annotated one.

## Check the pane

11. Start Revit 2024 and open the NG05 model.
12. On the RCRC Green tab press KPI Checklist.
13. The template block under the scan block reads No folder set. Press Browse, pick the
    folder from step 9 and press OK.
14. Check the list shows all seven files, each with its template name beside it, and the
    line above it reads 7 workbooks in the folder, 7 recognised. The two park files are told
    apart by the words EXISTING and FUTURE in their file names.
15. Click the EXISTING PARKS file. Check every line under What EXISTING PARKS would fill
    against the annotated workbook, cell by cell: D3, C5, E4, D8, F11, H11, and the tree
    ranges B4 to B92 and B4 to B84. This check is the whole point of the round.
16. Click the STREETS file. Check it shows no area cell and says the road width and length
    are typed by hand.
17. Check the name box under the pick is prefilled with the file name and can be typed in.
18. Check the line under it names the folder the open model sits in and says an existing
    file is overwritten silently.
19. Rename one park file so its name holds neither EXISTING nor FUTURE, press Browse and OK
    again to refresh, and check the pane asks you to pick between the two rather than
    guessing. Rename it back.
20. Drop any non-template .xlsx into the folder, refresh the same way, and check it reads
    not offered with the reason. Nothing with a reason can be clicked.

## What you cannot check yet

There is no fill button. Filling waits on the scan report answering where the six values
live in the model, which is the next round.
