# Building and installing the colour box round

The second View Filters install, 2026-09-13: one colour box control on every filter row,
three per row, with the Windows colour picker behind each square. One action per numbered
step, the exact command where one is needed, and what passing reads like. A mismatch on
screen is a finding about the tool, not a slip in the reading: write it down and stop.

## Build and install

1. Open VS Code.

2. File, then Open Folder, and pick this repo:

```
C:\Users\bader\source\repos\RCRC-Green
```

3. Open the terminal: Terminal menu, then New Terminal. It opens at the repo root.

4. Pull the round:

```
git pull origin main
```

5. Open this pull request on GitHub and check the test gate shows a green tick:

```
https://github.com/baderrahal/RCRC-Green/pulls
```

   A green tick or you stop here and say so. **The tests run there, on .NET 10, so this
   PC needs no .NET 10 SDK.** What the count should read is in the top entry of
   `steps/log-view-filters.md`, which is where it is kept up to date.

6. Build the add-in, which builds Core with it:

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

   0 warnings and 0 errors.

   Nothing on this PC needs the .NET 10 SDK any more. This build does not, and the step
   above reads the gate rather than running the tests here.

7. Install, which copies the DLLs and the addin file into the Revit 2024 add-ins folder:

```
.\install\install.ps1
```

8. Start Revit 2024, open a model, and open the View Filters pane from the RCRC Green tab.

## What to check on the rows

9. Every filter row now shows a colour square in three places: beside Line colour, beside
   the Foreground pattern type, and beside the Background pattern type. The old lone
   square next to Line colour is gone and all three look the same.

10. The override ticks are off on a fresh pane, so all three squares sit faded. Click a
    faded square: nothing happens, which is correct.

11. Tick **Line colour** on the first row. The square wakes up and shows #FF0000.

12. Click the square. The Windows colour picker opens IN FRONT of Revit, already expanded,
    with the Red, Green and Blue number boxes showing. If it opens behind Revit, or
    collapsed with no number boxes, stop and write it down.

13. Pick a colour and press OK. The hex box now reads that colour as #RRGGBB in capitals
    and the square shows it.

14. Mix a custom colour in the picker and add it to Custom colors. Open the picker again
    from a different row: the custom colour is still in the list. It stays for the rest of
    this Revit session.

15. Type over the hex box with a valid value, ff9900. The square repaints live.

16. Type garbage, #GGGGGG. The square's border turns the warning colour and the stored
    value stays what it was: the red edge means the text on screen is not the value the
    run would use. Retype a valid hex and the border settles again.

17. Untick Line colour. The square fades and stops taking clicks.

18. Scan and Apply are unchanged by this round and behave as the first install guide,
    `steps/2026-09-13-view-filters.md`, describes.
