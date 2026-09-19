# Running the Drawing Sheet pane after the four row round

One pass, in order, from opening VS Code to pressing Run and using the two buttons on the
result. One action per step, the exact command where there is one, what you should see, and
what it means if you do not.

**Nothing in this round has been seen in Revit.** Every screen described below is what the
code says it draws. A step that does not read as described is a finding about the tool rather
than a slip in the reading, and it is worth writing down exactly what you saw instead.

## Build and install

1. Open VS Code.

2. File menu, Open Folder, pick the `RCRC-Green` repo folder, press Select Folder. The
   Explorer on the left should list `src`, `tests`, `install`, `steps` and `.claude`. If it
   does not, you opened a folder inside the repo rather than the repo.

3. Get the round onto this PC. Open GitHub Desktop, pick the RCRC-Green repository, press
   **Fetch origin** and then **Pull origin**. When it says the branch is up to date, the
   round is on the PC.

4. Back in VS Code, Terminal menu, New Terminal. The prompt opens at the repo root. The path
   at the prompt should end in `RCRC-Green`.

5. Check the test gate on GitHub rather than running the tests here:

   ```
   https://github.com/baderrahal/RCRC-Green/pulls
   ```

   A green tick on this round's pull request, or you stop here and say so. The count the gate
   should report is in the top entry of `steps/log-drawing.md`. **The tests run on GitHub, on
   .NET 10, so this PC needs no .NET 10 SDK.**

6. Close Revit if it is open. The installer cannot replace an assembly Revit has loaded.

7. Build the add-in in Release, which builds Core with it:

   ```
   dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
   ```

   0 warnings and 0 errors. An error naming `Autodesk.Revit` means the two Nice3point packages
   did not restore, and `dotnet restore RcrcGreen.sln` first is the fix.

8. Install:

   ```
   .\install\install.ps1
   ```

   It builds the `Addins\2024\` layout the plain build does not, and writes
   `reports-folder.txt` next to the assembly. That file is the only thing that says where the
   reports folder is, so **if you skip this step the two buttons on the result will not be
   there**, and the pane will say so in words rather than showing nothing.

## Open the model and the pane

9. Start Revit 2024 and open `RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached.rvt`.

10. Look at the RCRC Green tab. Two panels, Drawing Sheet with one button, **Drawing Sheet**,
    and KPI with one button, **KPI Checklist**. No Scan Model button and no Scope Box button.
    A button for either means the old install is still loaded and step 8 did not take.

11. Press **Drawing Sheet**. The pane opens docked on the right and reads the model as soon as
    it is shown.

## What the pane should look like now

12. Read the whole pane top to bottom before touching anything. It should be, in order:

    - the strip: the model name in bold, a line reading `N views read at HH:mm:ss`, and
      **Refresh** and **Scan Model**, then the preset row under them
    - **four** numbered rows, `1 PLOTS`, `2 VIEW TYPES`, `3 MARK`, `4 SHEETS`
    - one unnumbered row headed **BEFORE YOU RUN**
    - the **Run** button, and a line above it
    - the status line at the very bottom

    **There is no row 5.** A fifth numbered row means an old assembly is loaded.

13. Look at one shut row. It is two lines: a `+`, the number, the title, and on the right the
    word `done` when that step is finished, then underneath a quieter line saying what the
    step holds, such as `160 in the model, none ticked`. A row that cannot be used yet puts
    its reason on that same second line, for example row 2 reading `Pick a plot range and tick
    at least one plot first.`

    **A greyed row with nothing on its second line is a fault.** So is a row whose second line
    is cut off with three dots: it is meant to wrap onto as many lines as it needs.

14. Drag the pane's left edge until it is about as wide as three fingers, roughly 300 pixels.
    Every row should still read: the second lines wrap onto more lines rather than being cut,
    and the Run button stays on screen at the foot. Narrower than that a scroll bar appears
    along the bottom and you scroll sideways. Nothing should be clipped off the right edge
    without a scroll bar to reach it.

15. Click the header of row 1. It opens and its `+` becomes `-`. Click row 3's header. Row 1
    shuts, keeping its summary line, and row 3 opens. **Only one row is ever open.** Two open
    at once is a fault.

16. Click a greyed row's header. Nothing should happen, and its reason stays on screen.

## Work the four rows

17. Row 1, PLOTS. Tick a plot, for example `DM`, set From and To over its sub plots, and untick
    one sub plot. The second line of row 1 should change as you go and end up reading something
    like `DM, 14 of 20 sub plots ticked`, and the word `done` should appear on the right of
    row 1's first line.

18. Row 2, VIEW TYPES. Open it and tick two or three types. Its second line should read
    `3 of 84 ticked` and `done` should appear on row 2.

19. Row 3, MARK. Open it and click a few empty squares. Its second line should count them,
    `4 marked`, and `done` appears on row 3. Everything the grid did before is still there:
    the legend, the search, the row and column marking, Clear all marks.

20. Row 4, SHEETS. Open it, add a sheet, pick its title block, tick its views and check the
    name and number boxes fill. Its second line should read something like `1 described,
    3 to make` and `done` appears on row 4.

    While you type in a number box, watch rows 1 to 4's second lines and the line above the
    Run button. They should update as you type without the cursor jumping out of the box.

## Run

21. Look at the line just above the **Run** button. When there is something to make it says
    what, for example `3 plan views, 2 sheets`. When there is not, it says why, for example
    `Nothing is marked and no sheet can be made. Click an empty cell in step 3, or add a sheet
    in step 4 and give it views, a name and a number.` and the **Run** button is grey.

    Those step numbers are still correct: nothing was renumbered this round.

22. Open **BEFORE YOU RUN**. Everything step 5 used to hold is in there: the line about what
    the run would do, the three answers any new view type needs, what the run cannot make with
    a control that opens the reasons, and the six scope box cases with Assign Scope Boxes.
    **Nothing from the old step 5 should be missing except the Run button itself**, which is
    now at the foot. Shut it again.

23. Press **Run**. The confirmation dialog is the one it has always been. Press Yes.

## The result

24. When the run finishes, row 4 opens by itself and shows the result **in place of its sheet
    controls**. Rows 1, 2 and 3 are untouched and still open when you click them.

25. Read the result top to bottom:

    - a bold red line only if something went both ways or a wrong schedule is still in the
      model. **In an ordinary run there is no red line at all.** A line beginning `THIS IS A
      BUG IN THE TOOL` means exactly that and is worth sending back with the report.
    - the two counts in bold, `7 created, 2 not created.`
    - one line per thing that did not happen, each ending in a reason. A line with a name and
      nothing after it is a fault. A line in red is a schedule that is in the model right now
      and has to be deleted by hand.
    - the line saying where the report went, `Report at C:\...\reports\...txt.`
    - two buttons, and below them **Back to the sheets**

26. Check the two counts against the status line at the very bottom of the pane. They are the
    same two numbers in the same words. **If those two disagree, stop and send both.** They
    are read off one object and cannot disagree unless something is wrong.

27. Press **Open the report**. Notepad, or whatever opens a `.txt`, should open that exact
    file. Check that the file's own counts match the two on the pane.

28. Press **Open the folder**. File Explorer should open the `reports` folder inside the repo,
    with that report in it. It should not open the Desktop.

    If step 8 was skipped there are no buttons at all, and the pane says `NO REPORT WAS
    WRITTEN, because reports-folder.txt is not next to the add-in ...`. That is the pane
    working, not failing.

29. Press **Back to the sheets**. Row 4's sheet controls come back exactly as they were, and
    the result is gone from the pane. The report file is untouched.

30. Change something in row 3 or row 4 and press **Run** again. The old result should be gone
    from row 4 the moment you press Run, and the new one takes its place when the run ends.

## What to write down

Anything that did not read as described above, in the words that were actually on screen. In
particular:

- a fifth numbered row
- two rows open at once
- a greyed row with an empty second line
- a second line cut off with three dots rather than wrapped
- the Run button scrolled out of sight
- the two counts on the result disagreeing with the status line
- either button opening the wrong thing, or opening nothing
- anything from the old step 5 that is not inside BEFORE YOU RUN
