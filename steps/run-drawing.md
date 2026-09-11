# Building, installing and running the Drawing Sheet

Every Drawing Sheet change since the third full run, in one pass. VS Code run order, one
action per step, the exact command per step, then what to check on screen and what passing
reads like. Nothing in the pane has been through Revit since pull request 32, so a step that
does not read as described is a finding about the tool and not a slip in the reading.

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

   Every test passes or you stop here and say so. Main carries 1103.

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

   It builds the `Addins\2024\` layout the build does not, and writes `reports-folder.txt`
   next to the assembly so every report lands in the repo's `reports/` folder as well as on
   the Desktop.

## Open the model and the pane

9. Start Revit 2024 and open `RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached.rvt`.

10. Look at the RCRC Green tab. It holds two panels, Drawing Sheet with one button, **Drawing
    Sheet**, and KPI with one button, **KPI Checklist**. There is no Scan Model button and no
    Scope Box button. The ribbon buttons went rounds ago and the classes behind them were
    deleted in pull request 62, so a button for either means an old install is still loaded.

11. Press **Drawing Sheet**. The pane opens docked on the right and reads the model as soon as
    it is shown. What it has, top to bottom: a strip with the model name in bold, a line
    reading `N views read at HH:mm:ss`, and two buttons, **Refresh** and **Scan Model**. Then
    five numbered steps, `1  PLOTS`, `2  VIEW TYPES`, `3  MARK`, `4  SHEETS` and `5  RUN`,
    one open at a time, each header carrying its own summary once it has one, and a greyed
    step carrying one line saying why it cannot be used yet. Then a status line docked at the
    bottom. Every dropdown in it comes from the model. Scanning, assigning scope boxes and
    running all happen from inside it, through its own external event, so a request waits
    until Revit is ready rather than failing.

## The unobserved changes, by pane step

Twenty steps, 12 to 31, ranked by what breaks the tool if it is wrong rather than by what is
easiest to check. Each says what to do, where to look, and the words or count that mean it
passed. What cannot be forced from the pane is listed at the end under not covered by this
pass, with what would force it.

### PLOTS

12. Read the status line after the automatic read. It reads `N views read at HH:mm:ss. P
    plots, A found through PRX_Plot_ID, B through the name, C with no plot at all.` then, when
    the model still holds the six the first scan found, `6 views are named for one plot and
    carry PRX_Plot_ID for another. The grid follows the name.` Then up to three clauses that
    have never been on a screen, each present only when the model holds the thing:
    - `N views carry a PRX_Plot_ID that is not a plot, such as <value>. Each still fills its
      cell from its name where the name parses.`
    - `N names are a plot in the wrong case and were not read as one: <name>, <name>, <name>
      and N more.`
    - `N schedules were not captured because their names do not parse, N of them a plot in
      the wrong case: <name>.`

    Passing: the line is in that shape, and every clause that appears names a count and at
    most three examples before `and N more`. A clause that does not appear means the model
    holds none of that thing, which is a measurement too, so write down which of the three
    appeared. Then press **Refresh** once more and count the seconds between the press and
    the line changing. The refresh walks every element now, which the scan measured at 1.4
    seconds for 96,934 of them and the pane has never measured at all.

13. Pick a prefix in step 1. From and To fill with the first and last plot under it, the line
    under them reads `N of N plots in range ticked, T in the model. Untick one to leave it
    out of the counts and out of anything that writes.`, and step 2 opens by itself. Click
    the step 1 header to open it again and read the tick list. A plot no view carries reads
    with a suffix after its name, `box only, no views`, `elements only, no views` or `box and
    elements, no views`, and its tooltip reads `<plot>, found through <sources>.` Try every
    prefix until one shows. This model has 406 scope boxes for 160 PRX_Plot_ID values, so a
    plot with a box and no view is likely somewhere. Passing: at least one such plot shows,
    its row in step 3 is every square empty, and the suffix stays readable at pane width
    rather than being cut. No such plot on any prefix is a finding of its own, the union adds
    nothing on this model, and say so.

14. In step 1 untick two plots. In step 3 click one empty square on a ticked plot, so the
    status line reads `1 marked. Marking records intent and changes nothing until Run.`
    Press **Refresh**. Passing: the prefix, From and To come back as they were, the two plots
    are still unticked, the PLOTS header reads `<first> to <last>, N-2 of N ticked`, and the
    status line ends `1 mark cleared. Nothing in the model has changed.` A refresh used to
    put every plot back and say nothing about the marks.

15. Open a new empty project in Revit, File then New, any template, and press **Refresh**
    with the pane open. Passing: the strip names the new document, step 1 is greyed and reads
    `This model holds no plots. No view name, no PRX_Plot_ID on a view or an element and no
    scope box gives one.`, steps 2 to 4 are greyed with `Pick a plot range and tick at least
    one plot first.`, and step 5 with `Mark a cell in step 3, or add a sheet in step 4.
    Nothing is created until you do.` Close the new project without saving and go back to the
    real model. Press **Refresh** again.

### VIEW TYPES

16. In step 2, press one of the code buttons, `010` or `200`: every type with that code
    ticks. Type part of a name in Search and press **None**: only the types the search shows
    untick, and the line under the list reads `N of M ticked. The search is showing K of
    them.` Then the escape. WPF swallows the first underscore in a tick box caption, and
    whether any real view type or plot name holds one is UNKNOWN. Force it: in the Add row at
    the foot of step 2, pick a code, type `Test_Underscore Plan` and press **Add**. Passing:
    the status line reads `(<code>) Test_Underscore Plan added. It shows missing on every
    plot, which is right, and it is marked new until the model holds one.`, the new tick box
    reads `(<code>) Test_Underscore Plan   plan   new` with the underscore on screen, and the
    step 3 column header shows it with its underscore too. Untick it afterwards and mark
    nothing on it. The model has no view of that type to set a new one up from, so a run
    would refuse it by name.

17. Tick two plan types. In step 3 mark one empty square under the first of them on a ticked
    plot, then go back to step 2 and untick that type. Passing: the MARK header reads
    `nothing marked`, RUN is greyed reading `Mark a cell in step 3, or add a sheet in step 4.
    Nothing is created until you do.`, and the grid shows no column for it. Tick the type
    again: the mark is back in its square and the MARK header reads `1 marked`. A mark on a
    hidden column used to reach the run while the header counted it over a grid that did not
    show it. Press **Clear all marks**: `1 mark cleared. Nothing in the model has changed.`

### MARK

18. With enough plots ticked to scroll down and enough types ticked to scroll across, scroll
    the grid both ways and click an empty square. Passing: the grid stays where it was in
    both directions, and marking five squares down a long grid takes five clicks and no
    scrolling. Then in step 4, with a sheet added and its view list scrolled, tick a view near
    the bottom of that list. Passing: the list stays put. Note whether the grid's row viewer,
    which has no sideways scroll of its own, still comes back at its vertical offset, because
    restoring both offsets on one layout pass for a viewer with one axis disabled has never
    been watched.

19. With one plot in the range unticked, press **Mark every missing**. Passing: the status
    line reads `N cells marked, M in total. Marking records intent and changes nothing until
    Run.` with N equal to M on a fresh grid, every empty square on every ticked plot now
    shows the marked glyph, and every square on the unticked plot is left alone. Press it
    again: `Nothing there to mark. Every cell in it is already marked, already holds a view,
    or belongs to a plot that is not ticked.` Press **Clear all marks**, then click one plot
    name: its row marks and nothing else does. Click one column header: its column marks down
    every ticked plot and not the unticked one.

20. A mark that went stale. Click one empty square on a ticked plot whose label does not read
    `no scope box`, and leave it marked. In Revit, without pressing Refresh, make that view by
    hand: duplicate any view of that type and rename the copy to exactly the name the square's
    tooltip shows, `<plot>-(<code>) <view name>`. Press **Run**. Passing: no dialog, because
    the run makes nothing, the status line reads `Nothing was created. Report at <Desktop
    path> and <repo path>.`, and the report's `NOT CREATED, DECIDED BEFORE THE RUN, 1` reads
    `<name>. A view with this name is already in the model, added since the panel last read
    it, so nothing is made over it. Press Refresh to see it.` The project browser holds one
    view of that name, the one made by hand. Delete it again and press **Refresh**.

### SHEETS

21. Open step 4. It reads `No sheet added yet. A sheet is described once here, its views
    divide into as many sheets as they need, and each ticked plot gets the whole set.` Press
    **Add a sheet**. Passing: a block headed `Sheet 1` with **Remove** on its right, a
    dropdown captioned `Title block` listing the title block types the model holds, by family
    and type, a row `Views per sheet` with `1`, `2` and `4`, the line `Tick the views that go
    on it, from the types ticked in step 2. They go onto sheets in the order they are ticked
    here.`, one tick box per type ticked in step 2 with its kind word, and a table headed
    `Plot | Views | Name | Number`. Pick a title block, leave 1 per sheet, tick one plan type.
    The line over the table reads `On <title block>, 1 view per sheet, 1 sheet per ticked
    plot: (<code>) <view name>.` and under it `Makes N sheets across the ticked plots.` or
    the same with `K rows still need a name or a number.` on the end. The word Type on its own
    appears nowhere in the step.

22. Proposals. With DM-11 in the ticked range, read its row. Passing: the Name box holds the
    view name upper cased with the code taken off, `GENERAL ARRANGEMENT LAYOUT` for the (200)
    type, and the Number box holds the code, then `Q`, then the first letter from A that no
    sheet in the model carries, `200QA` unless a sheet already holds it, with nothing under
    the box. DM-11 is the one plot whose numbers are its own. Read any other plot's row.
    Passing: its Number box is empty and the line under it reads `This plot has no sheet
    numbers yet, so there is no plot letter to continue. Type the number.`, because every
    number those plots carry holds the word Copy and a copy lends no letter. Open that Number
    box's dropdown: it offers numbers no sheet carries and none of them holds the word Copy.
    Whether the team wants one letter per plot at all is their question, step 41.

23. Untick DM-11 for this step, so no row can be proposed a number. Set Views per sheet to
    `2` and tick three types. Passing: the line reads `On <title block>, 2 views per sheet, 2
    sheets per ticked plot: <A>, <B>, <C>.`, the line `<A>, <B>: Holds 2 views, so the name
    and the number are typed rather than proposed.` sits under the ticks, each plot has two
    rows, the first holding `<A>, <B>` with both boxes empty and the second holding `<C>`
    with its name proposed and its number box empty with the reason, the batch line ends
    `N rows still need a name or a number.`, the SHEETS header reads `1 described, none to
    make yet`, and RUN is greyed reading `N rows in step 4 still need a name or a number, so
    no sheet can be made yet. Finish that in step 4, or mark a cell in step 3.` Untick every
    type in step 2: the block reads `No view type is ticked in step 2, so this definition
    makes no sheets.` and the table empties. Tick them again and tick DM-11 again.

24. Typing. Set Views per sheet back to `1` with one plan type ticked, and add a second sheet
    with 1 view per sheet and a different plan type, so DM-11 has one proposed row in each.
    In one DM-11 row type `010QE` over the proposal, a number a sheet already carries.
    Passing: the cursor stays in the box, the line under it reads `a sheet in this model
    already has that number` in the warning colour, no other row moved, and step 5's line,
    once step 5 is opened, ends `1 sheet number will be refused, and that sheet will not be
    made. It is marked in step 4.` Back in step 4, type the same free number into two rows:
    `another sheet in this run asks for the same number` under both. Clear them. Then type
    into Sheet 1's DM-11 box the number Sheet 2's DM-11 row was proposed. Passing: Sheet 2's
    proposal moves to the next free letter on its own while the box you are typing in keeps
    its cursor. Then open a Name box's dropdown with its text half typed. Passing: the list of
    names in use does not replace what was typed, and picking one puts it in the box.

25. With a name or a number typed on any row, press **Remove**. Passing: a Yes and No box
    titled `Remove sheet 1` reads `Removing this sheet loses the names or numbers typed on N
    rows. Remove it anyway?` with No as the default, and No leaves the sheet where it is.
    Press **Add a sheet** again, type nothing into it, press its **Remove**: it goes without
    asking.

### RUN

26. In step 3 mark one square, then in step 1 untick every plot in the range. Open step 5.
    Passing: the scope box block reads `Tick at least one plot to see what Assign would do.`
    with no Assign button under it, and **Run** answers `No plots are ticked, so there is
    nothing to run.` in the status line. Tick one plot again: the block reads `N views across
    1 ticked plot. Only C is written.` and **Assign Scope Boxes** is back.

27. Read the six cases: `+  A, name does not parse: n`, `B, cannot hold a scope box: n` as
    plain text, `+  C, ready to assign: n`, `+  D, no scope box for that plot: n`, `+  E,
    already right: n`, `+  F, holds a different scope box: n`. Click a case with a count above
    zero. Passing: it opens as a list of `<plot>   <view name>` rows, `holds <box>` on the
    end where the view carries one, as many rows as the count, one case open at a time, and a
    case at zero cannot be clicked. Click one row: the view opens and the status line reads
    `Showing <view name>.` The counts and the assignment now read a view's scope box state
    through one reader, so the counts here are what the next step's dialog says.

28. Press **Assign Scope Boxes**. Where C is 0 no dialog appears and the status line reads
    `Nothing was changed. Report at <Desktop path> and <repo path>.` Where C is above zero,
    passing: a dialog titled `RCRC Green, Scope Box` reads `Give N views their scope boxes?`
    over the six counts, the same numbers as on the pane, then `Only case C is written. D and
    F are reported and left as they are. The report is written either way.`, with No as the
    default. Press No: `Nothing was changed. Report at ... and ...`. Press it again and Yes:
    `N views given a scope box over P ticked plots. Report at ... and ...`, and after
    **Refresh** C reads 0 and E has grown by N. Where C is 0 on every prefix the write cannot
    be forced on this model, and the No path is the check. Keep the report.

29. Press **Scan Model**. The status line reads `Scanning the whole model. This one reads
    every element, so it takes longer.` then `Scan written. Report at <Desktop path> and
    <repo path>.` Count the seconds. Open the report. Passing: the sections run `SHEETS`,
    `VIEWS ON SHEETS`, `VIEWS NOT ON SHEETS`, `VIEW TEMPLATES`, `VIEW FAMILY TYPES`, `VIEW
    FAMILY TYPE PER VIEW TYPE`, `VIEWPORTS ON EXISTING SHEETS`, `SCOPE BOXES`, `PRX_Plot_ID
    VALUES`, `VIEWS THAT DISAGREE WITH THEMSELVES` and `PARSE SUMMARY`. The viewports section
    has never run: it holds one row per placed view or schedule, 953 views were on sheets on
    the first scan, as `sheet | view | scale | centre | size | sheet size` in millimetres,
    with a scale like `1:500` on a view and `no scale` on a schedule, and no size reading
    `0 by 0 mm`. The family type section says how many view types are built more than one way
    and lists those first, and the 010 type should be among them. The parse summary carries
    the line `Sheet names and sheet numbers are not read here either.` and no tally for
    either. Keep the report.

30. Two refusals the run deletes again. Add a sheet, 1 view per sheet, one plan type, two
    plots ticked. In one row type `010QE` as the number, which the warning says a sheet
    already carries. In the other row type a name holding a colon, `TEST: COLON`, and a free
    number. Press **Run**, read the dialog, press Yes. Passing for the number: `NOT CREATED,
    REFUSED BY REVIT DURING THE RUN` holds `010QE <name> was not made. A sheet numbered 010QE
    is already in this model. It was deleted again.` and the project browser holds one sheet
    numbered 010QE, the original. For the name, whether Revit refuses a colon in a sheet name
    is UNKNOWN. If it does, the same section holds `<number> TEST: COLON was not made. Revit
    refused the name TEST: COLON. Revit said: <message> It was deleted again.` and no sheet
    under that number is in the browser. If it does not, the sheet is made with the colon in
    its name, that path stays unobserved, and say so. Either way nothing in the report reads
    `IT IS STILL IN THE MODEL`. Undo once to take out anything the run made.

### The KPI pane

31. RCRC Green tab, KPI panel, **KPI Checklist**. The caption escape both panes use moved to
    Shared in pull request 64 and neither pane has been opened since. Passing: under `What
    the values come from`, the buttons read `PRX_Component`, `PRX_Plot_ID`, `PRX_Plot_UID`,
    `PRX_Plot_UID2` and `PRX_Plot_NH` with every underscore on screen. Close it.

## One full run over two plots

32. Set it up. In step 1 pick a range holding two plots whose row labels in step 3 do not
    read `no scope box` and whose rows show an empty square under one plan code, under `(400)
    Landscape Cross Section` and under one `(600)` schedule. Untick every other plot in the
    range. In step 2 tick exactly those three types. In step 3 mark those six squares. In
    step 4 add one sheet, pick the title block, set `2` views per sheet, and tick the three
    types in the order plan, section, schedule. Each plot gets two rows: the first holds the
    plan and the section, so type a name and a number into it, and the second holds the
    schedule alone and is proposed a name from it. Where a plot is not DM-11 its number box
    is empty, so pick a number from the dropdown. The batch line reads `Makes 4 sheets across
    the ticked plots.` and the RUN header reads `2 plan views, 2 sections, 2 schedules and 4
    sheets`.

33. Press **Run**. Passing: a dialog titled `RCRC Green, Drawing Sheet` with the main line
    `This run would make 2 plan views, 2 sections, 2 schedules and 4 sheets.`, the first
    eight names listed then `and 2 more.`, the paragraph beginning `A plan view is created
    fresh and carries no annotation.`, then the described sheet's own line, `On <title
    block>, 2 views per sheet, 2 sheets per ticked plot: <plan>, <section>, <schedule>. Makes
    4 sheets across the ticked plots.`, and No as the default. Press Yes.

34. Read the status line. Passing: `10 created, 0 not created. Press Refresh to see them.
    Report at <Desktop path> and <repo path>.` Press **Refresh**: the six squares are filled
    and the two plots' rows show them.

35. Open the run report. Passing, section by section:
    - the headline reads `10 were created and 0 were not: 0 refused before the run, 0 refused
      by Revit during it, 0 created wrong and still in the model.`, one number for everything
      not made, and no banner sits between it and the sections
    - `CREATED, PLAN VIEWS, 2`, `CREATED, SECTIONS, 2`, `CREATED, SCHEDULES, 2`, `CREATED,
      SHEETS, 4`, and `none` under all three NOT CREATED headings and under `CREATED, BUT
      NEEDS ATTENTION`. A note there naming Annotation Crop, the scope box or PRX_Plot_ID
      means a Set answered false, which the tool now records where it used to read clean
    - `WHERE EACH NEW VIEW AND SHEET WAS SET UP FROM, 10`. Each plan view: `<name>. Set up
      from <sibling view>: family type <type>, view template <template>, level <level>, crop
      on, crop region shown. Annotation crop is on, which is the tool's setting on every plan
      view rather than anything read off a view. <sibling> has it on as well.` or ending
      `has it off, and a view with it off draws the section markers of neighbouring plots
      through itself.` Each section: the same shape without the level, ending `Looks 1
      metre, which is the tool's setting rather than anything read off a view.` Each sheet:
      `<plot>. <number> <name>. 841 by 594 mm, from the Sheet Width and Sheet Height of
      <title block>. The name was typed and the number was typed.`, or `built from its view`
      and `proposed from the plot's own numbering` on the proposed rows. An A1 title block
      placed on a sheet read 841 by 594 on the third full run, so a different size here is a
      finding
    - `WHERE EACH VIEWPORT LANDED, 6`. Each line: `On <number> <name>: <view> at 1:<scale>,
      W by H mm, centre at X by Y mm, on a sheet of 841 by 594 mm.`, and for a schedule `,
      a schedule, W by H mm, ...`. Two views on one sheet sit a quarter and three quarters of
      the way across at half the height, so on an A1 the centres read about `210 by 297 mm`
      and `631 by 297 mm`. A schedule reading `0 by 0 mm` means its bounding box could not be
      read after the one regeneration, which has never been measured. A centre outside the
      sheet means the origin the tool assumes is wrong. Hold every line against the same
      plot's rows in the scan's `VIEWPORTS ON EXISTING SHEETS`, which is what that section
      is for

36. Check the model against the report. Open a new plan view: Properties shows Annotation
    Crop ticked, Scope Box set to the plot's box, PRX_Plot_ID reading the plot, and the View
    Template the report named. Open a new section: it cuts across the middle of the plot's
    scope box the short way, its Far Clip Offset reads 1000 mm, and it holds no scope box.
    Open a new schedule: its filter reads PRX_Ref Plot ID equals the plot and its columns are
    the source plot's. Open the two-view sheet: both views are on it, PRX_Plot_ID reads the
    plot, and its Scale reads `As Indicated` where the two views differ in scale and the
    shared scale where they do not, which is the reading the twenty fifth pass took from the
    forum and has never seen on this model. The Sheet List, which filters on PRX_Plot_ID,
    finds the four new sheets.

37. The whole run is one transaction. Press Undo once. Passing: every new view, schedule and
    sheet leaves the browser together, and after **Refresh** the six squares are empty again.
    Keep the report either way.

## Questions only Revit or the team can answer

Each one is a yes or a no.

38. Dock the pane at the width the team keeps it. With DM-11 to DM-28 ticked, does the PLOTS
    header show `1  PLOTS   DM-11 to DM-28, 17 of 17 ticked` whole, or does it end in an
    ellipsis? The same for `4  SHEETS   1 described, 4 to make` and the RUN header carrying
    the counts. Yes means the summaries survive a docked width, no means they are hidden
    exactly where they are meant to be read.

39. Tick eight view types, both 010 types among them, so the grid has eight columns. Do all
    eight headers show without the sideways scroll bar, does the key under the grid read
    `010 Location is (010) Location Key Plan` and `010 Overall is (010) Overall Key Plan`,
    and in step 4 does the `Plot | Views | Name | Number` table show all four columns with
    the warning line under a Number box staying inside the pane? One yes or no per question.

40. Is Revit's uniqueness rule on sheet numbers case sensitive? The pane's own check is: type
    `010qe` into a Number box and no warning appears under it, though `010QE` is in the
    model. Press **Run** and Yes. If Revit refuses it, `NOT CREATED, REFUSED BY REVIT DURING
    THE RUN` holds `A sheet numbered 010qe is already in this model. It was deleted again.`,
    the answer is no, Revit ignores case, and the pane's check has to as well. If the sheet is
    made, the answer is yes. Undo it. The same answer comes without the tool: in Revit change
    any sheet's number to another sheet's number in lower case and see whether Revit refuses.

41. Finding 15, asked of the team and not of Revit. DM-11's numbers read 010QE to 010QH where
    a code has several sheets and 200Q and 400Q where it has one, so the plot's own pattern
    for a lone sheet carries no sheet letter, and the tool proposes 200QA where the plot has
    200Q. Does a plot with one sheet under a code carry a sheet letter, yes or no? No branch
    starts until this is answered.

## What to send back

42. The scan report from step 29 and the run report from step 35, both from `reports/` in the
    repo or from the Desktop, one line per step from 10 to 40 that did not read as described,
    and the four answers from steps 38 to 41.

## Not covered by this pass

Each of these needs something this model or this pane cannot supply. Listed so nothing is
lost.

- A view or schedule whose name is taken between the run's read and its rename, which is the
  one path left into the rename-then-delete guard. It needs a second person creating the
  same view in a workshared model at the same moment.
- The schedule delete-again on a throw from adding a field or a filter, and whether either
  call throws at all on a filter the source schedule carried. Nothing on this model is known
  to make Revit throw there.
- The duplicate field note. It needs a source schedule holding two fields under one display
  name, and whether one exists is UNKNOWN.
- A throw after an item is recorded as made landing under needs attention rather than as a
  contradiction. Passing shows in step 35 as no banner and nothing under needs attention.
  Forcing the throw would mean making Revit fail a placement on purpose.
- The two kind refusals, a plan asked of a section sibling and a section of a plan one. They
  need a view type the model draws both ways, and whether this model has one is UNKNOWN.
- The uncapturable refusal wording and the capture recording a filter or field it could not
  read. They need a schedule named to the pattern whose filters name no plot, or a filter
  Revit will not read back. If step 12's third clause appears, the capture half is observed
  there.
- The not available message on the ribbon button, which needs the pane to fail to register.
- A Set answering false for a scope box, an annotation crop or a PRX_Plot_ID that a view
  template controls. Step 35 shows the note if it happens, and forcing it needs a template
  built to control those.
- Four views per sheet.
