# Audit of Drawing Sheet, 2026-09-10

Main at `82d95f4`, 942 tests, 483 of them apart from KPI. The brief named `2a4409b` at 927.
Two KPI pull requests, 52 and 53, landed between the two and added 15 KPI tests, and every
Drawing Sheet file, every Shared file, every flat test file and every mockup is byte for
byte the same at both. Read only: nothing was fixed, renamed or moved. Every code break
below was made to test a test, watched, and put back, and the suite was rerun green at 942
afterwards with `git status` empty. Findings are ranked by cost to the user. 8 findings with
no user cost were dropped and are listed at the end.

Of the previous audit's findings in `steps/audit.md`, 1 to 6, 8 and 12 were fixed in pull
request 48. The other eleven, 7, 9, 10, 11, 13, 14, 15, 16, 17, 18 and 19, all still stand
and are restated below with their current lines, 16 for its Drawing Sheet half only.

`steps/ai-max-state-drawing.md` is in the commit only because the commit hook requires a
state file in every commit, which this audit's touch-nothing-else rule had to give way to,
the same as last time.

## Ranked findings

### BLOCKS

None found. The one path that writes something wrong into a model is finding 3, and it
needs Revit to throw from a call that has not been seen throwing, so it is ranked WRONG.

### WRONG

1. LOGIC | src/RcrcGreen.Revit/DrawingSheetPanel.cs:283, :580, :1653 and :2031, against
   src/RcrcGreen.Core/DrawingSheet/RunPlan.cs:238 and
   src/RcrcGreen.Revit/SheetBeingDescribed.cs:84 | The ninth two-records instance. What a
   tick in step 2 means is decided twice: a described sheet drops a view type the moment it
   is unticked, because `Built` narrows to `stillTicked` and says the run no longer offers
   it, while a mark on that type stays in `_marked`, is passed whole to `RunPlan.Of`, which
   filters marks by ticked plot and nothing else, and is created. The MARK header and the
   status line count `_marked.Count`, which holds marks on hidden columns and on plots
   outside the range since `RangeChosen` at :1909 never clears them, while the grid at :580
   is built from `_columns.Shown` and shows none of them. `SheetGrid.MarkedCount`, the count
   of what the grid shows, is read by nothing | The run makes a view in a column the user
   unticked and cannot see or unmark, the header says 3 marked over a grid showing none,
   and a mark on a plot outside the range is counted in the header and dropped by the run.
   The confirmation dialog names the item, which is the one thing standing between this and
   the model, and only for the first eight | A line: filter marks by shown column in
   `PlanNow` and `AskToRun`, and count `grid.MarkedCount`

   FIXED. The panel reads its marks off the grid it draws, through `SheetGrid.Marked`, for
   the MARK header, the status line, the plan preview and the run, and `_marked` stays as
   the memory that brings a mark back with its column. A mark on a plot outside the range
   is off the grid too, so it is neither counted nor run. Two tests go red when marked
   columns leak into the grid, both watched.

2. LOGIC | src/RcrcGreen.Core/DrawingSheet/SheetNumbers.cs:171 and :292, fed by
   src/RcrcGreen.Revit/SheetBeingDescribed.cs:147 and
   src/RcrcGreen.Revit/DrawingSheetReader.cs:124 | The plot letter is read off every sheet
   number carrying the plot's PRX_Plot_ID, and `LettersIn` reads 010QE Copy 001 as Q,
   which SheetNumbersTests.cs:127 pins as intended. CLAUDE.md:124 records that only DM-11
   has real numbers and every other plot carries copies of DM-11's, so on the measured
   model every plot but DM-11 proposes DM-11's letter: DM-12 gets 010QA, DM-13 gets 010QB,
   DM-14 gets 010QC. `ScannedSheet.CopyMark` at ScannedSheet.cs:17 already defines what a
   copy number is and the scan counts them, and the proposal does not ask | A number
   proposed from scaffolding reads as continuing the plot's own pattern, the report says
   proposed from the plot's own numbering, and it goes into a drawing register, which is
   the one place the code says a wrong number cannot be corrected. Whether the team wants
   one letter per plot at all is UNKNOWN, and a proposal built on copies cannot be the
   answer either way | A line: drop numbers holding `CopyMark` before `LettersIn`, so a
   plot with only copies gets the no-numbers refusal it already has words for

   FIXED. `LettersIn` and the seeds of `Free` skip a number holding `ScannedSheet.CopyMark`,
   so a plot carrying only copies gets the no-numbers refusal in the words it already had,
   and a plot with one number of its own beside a copy keeps its own letter. The test that
   pinned the old reading is replaced by one that says why it was wrong. Three tests
   watched red with both filters removed. Whether the team wants one letter per plot is
   still UNKNOWN.

3. WIRING | src/RcrcGreen.Revit/ModelWriter.cs:377 and :843 to :875 | The delete-again
   guard covers the sheet number at :357 and the rename at :955, and two writes between
   an element existing and its guard are outside it. `sheet.Name = wanted.SheetName` at
   :377 is a typed name for any sheet holding more than one view, Revit refuses a name
   holding a colon, a brace or a bracket with the same ArgumentException the rename gets,
   and the catch at :63 records a refusal while the numbered sheet stays in the model under
   Revit's default name. In `MakeSchedule` the schedule is renamed at :829 and `AddField`
   at :854 and `AddFilter` at :874 run with no guard, so a throw from either lands at :95
   as refused while a schedule named for the plot, short of the filter that was being
   added, stays in the model. `Renamed` at :970 reads every ArgumentException as a
   duplicate name, so a forbidden character in a typed view type name from
   DrawingSheetPanel.cs:1856 is deleted again correctly and blamed on a name that is not
   there | A report that says not made while the model holds the thing is the fault this
   repo treats as worst, fixed for views in pull request 48 and standing one line below
   the fix for sheets. The schedule case is the shape of the last audit's BLOCKS, reached
   through a throw rather than a skip. Whether `AddFilter` throws on a filter the source
   schedule carried is UNKNOWN without Revit | A method: name the sheet and add the fields
   and filters inside the same delete-again shape the number and the rename use, and let
   `Renamed` say what Revit said

   FIXED. The sheet name goes on inside the same delete-again shape as the number, and the
   schedule's link setting, fields and filters go on inside one that deletes the schedule
   again with the delete checked, or records it as left in the model when the delete
   fails. `Renamed` quotes what Revit said instead of naming a duplicate. Neither path has
   been through Revit.

4. WIRING | src/RcrcGreen.Revit/ModelWriter.cs:404 and :771 | Two `Parameter.Set` returns
   are still ignored, both on PRX_Plot_ID: the sheet's at :404 and every new view's at
   :771, where `SetPlotId` has words for a null and for read only and none for a false.
   The scope box at :183, the annotation crop at :730 and AssignScopeBoxCommand.cs:151
   all check the same call | A view or sheet that comes out without its plot is not found
   by the Sheet List, is not counted under `NumbersOnPlot`, so the next sheet on that plot
   gets no proposal, and the report reads clean. The last audit ranked the same class
   WRONG and two of its four sites were fixed | Two lines each, the same NeedsAttention
   note :183 already writes

   FIXED. Both returns are checked. A false on a view writes the scope box's note with the
   parameter named, and a false on a sheet says the Sheet List will not find it.

### COSTLY

5. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:770, :777, :979, :1005 and :2030 |
   Finding 7 of the last audit, unchanged. The grid's two ScrollViewers are built bare, the
   step 4 view list and table use the two-argument `Scrolling` that remembers nothing, and
   `CellClicked` and `MarkThese` both call `Redraw`, so every mark throws the grid back to
   the top left. Only the plot list at :465 and the view type list at :517 use the
   remembered overload at :1581 | Marking ten cells down a long grid is ten re-scrolls,
   the fault the user reported for the lists | A name each

   FIXED. `Remembering` keeps both offsets of any viewer by name, `Scrolling` goes through
   it, the grid's two viewers and step 4's list and table are named, and the bare overload
   is gone. Not observed in Revit.

6. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:1763, :1813 and :1909, with
   DrawingSheetRequestHandler.cs:218 | Finding 9 of the last audit, and worse than it said.
   `Took` clears every mark, then `PutTheRangeBack` calls `RangeChosen`, which rebuilds
   `_picked` with every plot ticked, so a Refresh also puts back every plot the user
   unticked. The status line the refresh writes says views, plots and counts and nothing
   about either | Somebody who unticks five plots, marks a screenful and presses Refresh
   to pick up a colleague's change loses both and is told neither | A clause in the
   refresh message, and keep the off set across `Took` the way `_columns` keeps its ticks

   FIXED. `Took` reads the unticked plots before the range is put back and unticks them
   again after, and hands the handler the count of marks it cleared, which goes on the end
   of the refresh message through `BulkMarking.ClearedInWords`. `Read` on the handler is a
   Func for that.

7. REPORTS | src/RcrcGreen.Core/DrawingSheet/RunReport.cs:76 to :78 against :91 |
   Finding 10 of the last audit, unchanged. The headline counts `NotCreatedCount`, which
   is refused during the run plus left behind, while NOT CREATED, DECIDED BEFORE THE RUN
   sits below with the plan's refusals and its own count. Four refused before and two
   during reads 2 were not at the top and 6 under the not-created headings | Two numbers
   for not created in the file that exists because two of its numbers once disagreed | A
   phrase in the headline

   FIXED. `RunReport.Headline` is one number for everything not made, split the way the
   sections split it: refused before the run, refused by Revit during it, created wrong and
   still in the model. The line under it says which count is the plan's. A test builds four
   before, two during and one left behind and reads 7 were not, watched red with the plan's
   refusals dropped from the sum.

8. INTERFACE | src/RcrcGreen.Revit/ShowDrawingSheetCommand.cs:30 to :33 | Finding 11 of
   the last audit, unchanged. The message shown when the pane fails to register still sends
   the user to Scan Model and Scope Box on the Reports panel, which was removed rounds ago |
   The one time this shows, in the middle of a failure, it names a ribbon panel that does
   not exist | A sentence

   FIXED. The message says the panel cannot be opened and that Scan Model and Scope Box run
   from inside it, then restart Revit.

9. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:1114 and :1170, with
   src/RcrcGreen.Core/DrawingSheet/SheetNumbers.cs:120 and DrawingSheetSnapshot.cs:269 |
   Every row of every step 4 table gets its own copy of every sheet name in use and every
   free number, and `FreeSheetNumbers` is recomputed on each read. On the measured model
   that is 1,385 sheets, so a definition over 17 plots builds 17 name boxes of 1,385 items
   and 17 number boxes of about 1,385 items on every redraw, and every Redraw rebuilds
   them. The free numbers are the model's numbers stepped on, so on this model the list is
   mostly 010QE Copy 002 and 400Q Copy 010, scaffolding the scan itself counts as
   duplicates | A dropdown of a thousand junk offers per row, built tens of times per
   click. How long that takes to draw in a docked pane is UNKNOWN without Revit, and the
   multiplication is not | Compute both lists once per redraw, and drop `CopyMark`
   numbers from `Free`

   FIXED. `FreeSheetNumbers` is worked out once in the snapshot's constructor, the panel
   reads both lists once per redraw into a `SheetOffers`, and every box takes them as its
   `ItemsSource` rather than copying them item by item. Copies are no longer seeds, under
   finding 2.

10. WIRING | src/RcrcGreen.Revit/ModelWriter.cs:401, :161 and :270, with the catches at
    :49 and :95 | `Made` is recorded before the calls that finish the item: a sheet at :401
    before `PlaceViews` at :406, a plan view at :161 before the template, the crop and the
    plot at :164 to :172, a section at :270 the same way. A throw from any of those lands
    in the catch as `Refused`, so the item is in `Created` and in `NotCreated` at once,
    `BothWays` finds it and the report opens with THIS REPORT CONTRADICTS ITSELF AND IS A
    BUG IN THE TOOL | The true state is created and needing attention. The report instead
    tells the user to distrust the whole file and report a tool bug, over a placement
    Revit refused. `Regenerate` at :535 and `ScheduleSheetInstance.Create` at :506 are
    plain Revit calls with no guard of their own, and whether either throws in practice is
    UNKNOWN | Wrap what runs after `Made` so a throw there is `NeedsAttention`

    FIXED. `Finishing` wraps what runs after `Made` for a plan view, a section and a sheet,
    and a throw from Revit there is recorded as created and needing attention with what
    Revit said, rather than as refused. Whether anything in there throws is still UNKNOWN.

11. REPORTS | src/RcrcGreen.Revit/ReportFile.cs:51 to :58 against
    src/RcrcGreen.Core/DrawingSheet/ReportPlaces.cs:47 | The repo copy's
    UnauthorizedAccessException and IOException are swallowed and the caller gets one
    path, and `ReportPlaces.Written` reads one path as reports-folder.txt being absent and
    tells the user to run install.ps1 again. When the pointer is there and the folder is
    on a drive that refused, the message names the wrong cause | The user reruns the
    installer for nothing and the report still lands in one place | Return why the second
    write failed alongside the path list

12. LOGIC | src/RcrcGreen.Revit/DrawingSheetReader.cs:152 and :181, with
    src/RcrcGreen.Core/Shared/PlotRegistry.cs:50 and :85, and ScheduleCapture.cs:72 |
    `PlotRegistry.Build` classifies every string that is not a plot, and marks a scope box
    named dm-41 or a parameter holding dm-41 as WrongCase rather than NotAPlotName, in
    words `IgnoredName.ToString` already prints. The reader takes `registry.Plots` and
    drops `registry.Ignored`, and nothing else reads it. `ScheduleCapture.Read` skips a
    schedule whose name does not parse with a bare continue at :72, so a schedule named in
    the wrong case is silently uncapturable | A mistyped scope box makes its plot vanish
    from the list, or turns a plot with a box into a plot with none, and the reason is
    computed and thrown away. This is Drawing Sheet's use of a Shared file, reported here
    as the brief allows | A line in the refresh status or a scan report section listing
    the WrongCase entries

    FIXED. The reader hands the registry's WrongCase entries to the snapshot, kept once per
    text, and the refresh line says them in the registry's own words through
    `RefreshWords.WrongCase`. `ScheduleCapture.Read` records a schedule whose name does not
    parse as an `IgnoredName` with the same classification, the snapshot carries the list,
    and the refresh line counts them and names the wrong case ones. Shared was read and not
    edited. The clause's filter was watched red. Not observed in Revit.

13. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:902 and :1316 | Remove on a
    described sheet takes it out on one click with no confirmation, and with it every name
    and number typed into that sheet's rows across every plot | A row of typed numbers
    over 17 plots is the most expensive thing on the panel to type, and it is the one
    thing with no way back. Run and Assign both confirm | A confirmation when any row
    holds typed text

    FIXED. `SheetBatch.RowsWithTypedText` and `WhyRemovalAsks` decide it in Core with two
    tests, each watched red on its own break, and `RemoveASheet` puts the question up as a
    Yes and No box with No as the default, only when the count is above zero.

14. NO VIBE CODING | src/RcrcGreen.Revit/RcrcGreenApplication.cs:52 to :63,
    ScanModelCommand.cs:28, AssignScopeBoxCommand.cs:34, ScanProgressWindow.cs, and
    .claude/rules/revit-commands.md:164 | Finding 13 of the last audit, unchanged. No
    button registers either command, so both `Execute` methods and the progress window
    they use are unreachable, and `ScanModelCommand.Execute` is a second scan path that
    writes to the Desktop only. The rule file says the two classes still do the work,
    reached through the external event, which is true of `AssignScopeBoxCommand`'s three
    statics and false of `ScanModelCommand`, which the handler never calls | The next
    scan change lands in one copy, and the rule file sends a reader to a path that does
    not run | A deletion, and one sentence in the rule file

    FIXED. `ScanModelCommand.cs` and `ScanProgressWindow.cs` are deleted.
    `AssignScopeBoxCommand` keeps its three statics and loses its Execute, its button
    constants and the two methods only Execute read, and revit-commands.md says which
    class does what. `IScanWatcher` stays, because both scanners take one and the handler
    passes a watcher that never cancels. The hook's file list and territory.md follow.

15. LOGIC | src/RcrcGreen.Core/DrawingSheet/SheetNumbers.cs:189 to :193 | A proposal is
    always code, plot letter, sheet letter. DM-11's own numbers, the only real ones,
    read 010QE to 010QH where a code has several sheets and 200Q and 400Q where it has
    one, so the plot's pattern for a lone sheet carries no sheet letter, and the tool would
    propose 200QA where the plot has 200Q. Whether the missing letter is the convention or
    an accident of DM-11 is UNKNOWN, and the brief the rule came from is quoted in
    core-rules.md as the first sheet letter from A | Either every lone-sheet proposal is
    one letter longer than the pattern it claims to continue, or nothing is wrong. The
    answer is one question to the team | A question, then possibly a branch

16. REPORTS | src/RcrcGreen.Core/DrawingSheet/NameParseSummary.cs:56 and :57, printed by
    ScanReport.cs:177 to :205 | Sheet names and sheet numbers are run through the view
    name parser, which wants a plot, a dash and a bracketed code. A sheet number never has
    those, so on any model the tally reads sheet number: 0 of N parsed, N did not, and the
    twenty first numbers are listed under it every scan, 1,385 on the measured model.
    Whether sheet names parse is UNKNOWN, since CLAUDE.md:83 says sheets are named to the
    pattern and SheetNaming.cs:9 says real ones read 010QF LIST OF DRAWINGS | Twenty
    lines and a count of 1,385 failures that nobody can act on, in every scan, reading as
    a fault in the model | Drop the sheet number tally, or parse numbers with a rule of
    their own

    FIXED. Both sheet tallies are gone and the parse summary says why. Measured against the
    real sheet names this repo records, in `SheetNaming.cs` and the twenty first pass's log
    entry: LIST OF DRAWINGS, GENERAL ARRANGEMENT LAYOUT, LANDSCAPE CROSS SECTION and
    SOFTSCAPE SCHEDULES, none carrying a plot or a code, and the tool's own `SheetNaming`
    makes names of that shape, so the sheet name tally could only ever read 0 of N. No
    real model was measured this round. CLAUDE.md said sheets are named like views and now
    says how they are named.

17. INTERFACE | src/RcrcGreen.Revit/DrawingSheetReader.cs:68 to :77, counted at
    DrawingSheetRequestHandler.cs:224, with ViewPlotReading.cs:21 | Finding 19 of the last
    audit, unchanged. A view whose PRX_Plot_ID holds something that is not a plot is
    counted under with no plot at all although its cell fills from the name, and
    `RawParameterValue`, kept on the reading so the present-and-wrong value can be shown,
    is read by nothing in the product or the scan | The count reads wrong to anyone who
    checks that view, and the value kept to be shown is never shown | A fourth count or a
    scan section

    FIXED. A view whose PRX_Plot_ID holds something that is not a plot is a fourth count,
    `ViewsWithAParameterThatIsNotAPlot`, kept apart from with no plot at all, and
    `ViewOnAPlot` carries the raw value so the refresh line shows it: 3 views carry a
    PRX_Plot_ID that is not a plot, such as N/A. Not observed in Revit.

### TIDY

18. NO VIBE CODING | src/RcrcGreen.Core/DrawingSheet/RunOutcome.cs:72 to :80 and
    src/RcrcGreen.Revit/SiblingReader.cs:15 | Finding 14 of the last audit, unchanged, and
    the RunOutcome comment has a second fault: it describes `SetUp`, says the far clip
    comes off the sibling, and sits above `_placements`, whose own comment follows it |
    Two comments describing a rule the tool stopped following, one on the wrong field | Two
    lines and a move

    FIXED. The RunOutcome comment sits on `NoteSetup` and says family type, level, template
    and crop, and each sheet's size. The SiblingReader comment says two of the three crop
    settings rather than the far clip.

19. LOGIC | src/RcrcGreen.Core/DrawingSheet/RunPlan.cs:364 and :370, pinned by
    tests/RcrcGreen.Core.Tests/RunPlanTests.cs:236 | Finding 15 of the last audit,
    unchanged. One refusal prints 1 things cannot be made, and the test asserts that exact
    string | The confirmation dialog reads machine written for the singular case | Two
    lines and the test

    FIXED. 1 thing cannot be made, and is named in the report, with the plural kept for
    two. The test that pinned 1 things pins the singular now and a second covers both.

20. NO VIBE CODING | src/RcrcGreen.Revit/ModelWriter.cs:607 against ModelScanner.cs:220,
    and src/RcrcGreen.Core/DrawingSheet/RunReport.cs:22 against ScanReport.cs:22 |
    Finding 16 of the last audit, its Drawing Sheet half. `Number(Parameter)` is the same
    method written twice, and `LineEnd` is the same constant twice inside Drawing Sheet
    itself, where ScopeBoxReport.cs:140 reads ScanReport's and RunReport keeps its own |
    Two copies of one fact, the shape this repo has been bitten by nine times now | A
    shared home for each

    FIXED. `ModelScanner.Number` is the one reader of a length parameter and the writer
    calls it, and `RunReport` writes its lines with `ScanReport.LineEnd` the way
    `ScopeBoxReport` already did.

21. NO VIBE CODING | src/RcrcGreen.Core/DrawingSheet/SheetDivision.cs:23, PlotBox.cs:74,
    GridColumns.cs:55, SectionPlacement.cs:29, SiblingView.cs:182, SheetNumbers.cs:148,
    SheetGrid.cs:88 and :93, ReportPlaces.cs:22, ScheduleDefinition.cs:93, and
    src/RcrcGreen.Revit/ScopeBoxScanner.cs:29 | Finding 17 of the last audit, unchanged,
    with more found by a scan of every public member for a caller outside its own file.
    Read by nothing in the product: `PlannedSheet.Position`, `PlotBox.Centre`,
    `GridColumns.HiddenCount`, `SectionPlacement.Axis`, `SiblingView.HasTemplate`,
    `SheetNumbers.PlotLetter`, `SheetGrid.MissingCount` and `MarkedCount`,
    `ReportPlaces.InsideTheRepo`, `FilterValue.TryParse` and
    `ScopeBoxScanner.PlotParameterByView`. `PlotLetter` restates the rule `Propose` holds
    inline at :171 to :185, so the letter rule is two copies. `TryParse` is the read half of
    a definition file that no round has written. `PlotParameterByView` costs a parameter
    lookup per view on every Assign and feeds nothing. `MarkedCount` is the count finding 1
    needs | Every one reads as a behaviour the tool has, and one of them is the fix for a
    WRONG finding sitting unused | A deletion each, or a caller for `MarkedCount`

    FIXED, every one checked again against the tree after the last round. Deleted with
    their tests: `PlannedSheet.Position`, `PlotBox.Centre`, `GridColumns.HiddenCount`,
    `SectionPlacement.Axis`, `SiblingView.HasTemplate`, `SheetGrid.MissingCount`,
    `ReportPlaces.InsideTheRepo`, `FilterValue.TryParse` and
    `ScopeBoxScanner.PlotParameterByView` with the lookup that fed it. Kept:
    `SheetGrid.MarkedCount`, which the last round gave four callers, and
    `SheetNumbers.PlotLetter`, which `Propose` now calls through an overload that hands
    back the reason, so the letter rule has one home and its five tests stay.

22. STRUCTURE | design/pr-31/panel.html:5 | Finding 18 of the last audit, unchanged. The
    folder says pull request 31 and the file's title says pull request 28 | Anyone tracing
    a round from its mockup lands on the wrong pull request | A folder rename

    FIXED, and it was two folders. `git log --diff-filter=A` says `design/pr-31/panel.html`,
    titled pull request 28, was added by the commit for pull request 28, and
    `design/pr-28/panel.html`, titled Sheet numbers, pull request 28, was added by the commit
    for pull request 29. Both sessions guessed 28 and the KPI one landed it. So pr-28 is
    renamed pr-29 with its title changed to 29, and pr-31 is renamed pr-28 with its title
    kept. The same fault nearly recurred this round, pr-60 opening as 61, and was renamed
    before the merge.

23. NO VIBE CODING | src/RcrcGreen.Core/DrawingSheet/PanelSteps.cs:210,
    src/RcrcGreen.Revit/DrawingSheetPanel.cs:1966, and .claude/rules/core-rules.md:27 |
    Both messages for a model with no plots say no view carries a PRX_Plot_ID and no view
    name gives one, and the rule file says the plot list is the union of three sources.
    Since pull request 48 the list is the union of four, and a plot found through a scope
    box or an element alone is the point of it | A message and a rule describing the list
    the tool had before the fix | Three lines

    FIXED. One sentence, `PanelSteps.NoPlotsInTheModel`, names all four sources, step 1 and
    the empty grid both read it, core-rules.md's heading says four and names the registry,
    and the reader's own comment says four. A test pins the sentence and that the grid
    starts with it.

24. INTERFACE | src/RcrcGreen.Core/DrawingSheet/PanelSteps.cs:290 to :297, fed by
    src/RcrcGreen.Revit/DrawingSheetPanel.cs:287 | `sheetsIncomplete` counts only
    definitions missing their type. A sheet described with a type and views whose rows
    still lack names has nothing marked, nothing asked and nothing incomplete, so RUN says
    Mark a cell in step 3, or add a sheet in step 4 to somebody who has just added one |
    The one message that should say finish the rows sends the user to add another sheet |
    Pass the rows short of a name or number as well

    FIXED. `PanelSteps.Of` takes the rows short of a name or a number as well, summed over
    `SheetBatch.RowsShortOfANameOrANumber`, and Run says what is unfinished, the title
    block, the rows or both, then finish that in step 4. Three tests, two watched red.

25. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:922, PanelSteps.cs:294 and
    SheetDefinition.cs:73 | The step 4 dropdown is captioned Type, the shut-step and refusal
    words call the same thing a sheet type, and it is a title block. Three words for one
    control, and Type on its own next to a list of view type tick boxes | A production
    person reads Type as the view type they just ticked | One caption

    FIXED. Title block, in those words, on the step 4 caption, in `SheetDefinition`'s
    three strings, in the Run reason and in the one refusal that named it. Sheet type
    appears nowhere now.

26. NO VIBE CODING | src/RcrcGreen.Revit/DrawingSheetReader.cs:186 to :199 against
    ScopeBoxScanner.cs:66 to :85 | The scope box state of a view, whether it can hold one
    and which it holds, is read by two loops written twice, one feeding the panel's counts
    and one feeding the assignment the handler writes | The count on screen and the write
    follow one Core rule on purpose, and the two readers feeding it can drift apart on the
    next change to either | One reader

    FIXED. `ScopeBoxScanner.StateOf` is the one reader of a view's scope box state, and
    the panel's reader and the assignment's scanner both call it.

27. LOGIC | src/RcrcGreen.Revit/ModelWriter.cs:845 | A captured field whose name is
    already added is skipped with a bare continue and nothing recorded, the shape the
    rules file names as a lie by omission. When a source schedule carries two fields with
    one display name is UNKNOWN | A schedule one column short with the report reading
    clean, on a path nobody has measured | A NeedsAttention note

    FIXED. A field the captured definition names twice is added once and named under needs
    attention, with the note saying that whether a source schedule really holds two fields
    under one name is UNKNOWN. Not observed in Revit.

28. STRUCTURE | src/RcrcGreen.Core/Shared/ScanFileName.cs:13 to :17 and :27 | The scan,
    scope box and run prefixes are Drawing Sheet's report names, and the two-argument
    `For` defaults to the scan one. They live in Shared, where KPI passes a prefix of its
    own and reads only `Cleaned` and the three-argument `For` | A change to a Drawing
    Sheet report name is a Shared change that has to run alone, for three constants no
    other task reads. This is the one Shared read that should live in DrawingSheet | Move
    three constants and one overload

29. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:452, :506, :969 and :1453, with
    CLAUDE.md:245 | String content in a CheckBox or a Button goes through WPF's access key
    reading, and CLAUDE.md records that the Drawing Sheet has the fault the KPI pane fixed
    and is not fixed. The plot tick boxes, the view type tick boxes, the step 4 view tick
    boxes and the scope box case buttons all carry names as strings. Whether any real view
    or scope box name holds an underscore is UNKNOWN, and the title block names, which do,
    sit in ComboBoxes | A name with an underscore reads wrong on the one panel whose job is
    exact names | The escape the KPI pane already has

    FIXED. The plot tick boxes, the view type tick boxes, the step 4 view tick boxes, the
    scope box case buttons and their view rows, and every secondary button go through
    `PaneLabel.Escaped`, the KPI pane's own, called across the fence rather than copied and
    with no KPI file edited. Its home should be Shared, which needs a round of its own. The
    title block names in the ComboBoxes are left as the audit left them. Not observed in
    Revit.

30. INTERFACE | src/RcrcGreen.Core/DrawingSheet/SiblingView.cs:244, with
    src/RcrcGreen.Revit/ModelWriter.cs:139 and :266 | `SiblingChoice.For` falls back to
    the first view of the type whatever its kind. A plan asked of a section sibling is
    refused with sits on no level, which describes a section as a plan with a fault, and a
    section asked of a plan sibling reaches `ViewSection.CreateSection` with a plan family
    type and is refused with Revit refused an argument | Two refusals that name the wrong
    cause, on a model that draws one view type both ways | Say the kind in both refusals

    FIXED. `SiblingView.WrongKindInWords` names the kind the nearest view is and the kind
    that was wanted, the writer refuses on it before asking for a level or creating a
    section, and a test writes both refusals out by hand.

31. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:433, :1401, :1716, :1722, :1960
    and :2031, against .claude/rules/revit-commands.md's rule that every summary and reason
    is in PanelSteps with a test | Six messages are formatted in the panel with no test,
    and one of them, at :1966, is the PanelSteps sentence at :210 written out again, so the
    same words live in two files and one of them is already stale, finding 23 | The rule
    that has caught two count faults is not being followed on the panel that needs it most
    | Move six strings

    FIXED. The step 1 line, the scope box line, the two no plots ticked lines, the nothing
    to run line, the empty grid's four reasons and the marked line are `PanelSteps` methods
    with tests, and the panel calls them. The no plots sentence is one constant read from
    both places, finding 23.

## Dropped, no cost to the user

`TryStepOn` overflows `long` on a nineteen digit number. A definition with no title block
is refused under the name " sheet" with a leading space. `PlannedSheet.Signature` joins on
a bar a view name could hold. `ForPlot` swaps any text filter shaped like a plot, on any
parameter. `ScheduleCapture` reads `GetField` as returning null where the API documents a
throw, and the ids come from `GetFieldOrder` so neither is reached. Step 4 can be opened
with no view type ticked, and says so. `SheetLayout` centres across the whole title block,
title strip included, and no real placement exists to hold it against. `RunItem` holds a
sheet's number and name twice, its own and its row's, from one constructor.

## Area notes

### 1, logic

Every one of the 50 files under src/RcrcGreen.Core/DrawingSheet was read in full. The
ninth two-records instance is finding 1, with a second at finding 20 and a third at 26.
Null, empty, NaN and infinity are checked where they matter: `PlotBox` and `SheetSize`
refuse them at the door, `SheetLayout.For` refuses a zero side, `FilterValue.OfNumber`
refuses NaN, and every list constructor drops nulls. Every comparison is ordinal, and the
one place that might want otherwise is UNKNOWN: `SheetNumbers.FaultIn` at :212 clears a
number that differs from an existing one only in case, and whether Revit's uniqueness rule
on sheet numbers is case sensitive has not been measured. Silent drops are findings 12 and
27. Arithmetic: `Lengths` uses the exact 0.3048, `SectionPlacement.Length` scales before
squaring, the layout divides by one or two, and the one overflow was dropped above. The
sheet division, naming and numbering from pull request 46 hold findings 2, 15 and part of
9, and the division itself came through clean: six views at 1, 2 and 4 per sheet were
checked by hand against SheetDivisionTests and the code, and no view is left off.

### 2, wiring

Every Drawing Sheet file under src/RcrcGreen.Revit was read in full. DrawingSheetPanel.cs
names no Document, Transaction, FilteredElementCollector or ElementId outside its comments,
checked by reading and by grep, and reaches Revit only through the handler and the external
event. Its Revit types are ExternalEvent and the dockable pane interfaces. Two transactions
exist, the run's at DrawingSheetRequestHandler.cs:391 and the assignment's at
AssignScopeBoxCommand.cs:133, each started and committed inside one using, so a throw
before the commit rolls back on dispose. `Execute` catches everything and `Stop` catches
its own report, so nothing leaves the handler. No document or element is held across calls:
the handler fetches the document per request, `SiblingReader` holds Views for the length of
one `Make`, and the panel holds the snapshot. Nothing reads an instance parameter off a
FamilySymbol: `SizeOf` and the scan read SHEET_WIDTH off placed blocks, and `TitleBlock`
reads only Name and FamilyName off the symbol. The `Parameter.Set` returns are finding 4,
and the element-left-behind paths are findings 3 and 10.

### 3, folders and shared code

Core/DrawingSheet does not need subfolders yet. Fifty files in one flat namespace are
readable with the naming they have, and folders would buy nothing until a second person
works in them. The seams are already visible if that day comes: the grid, which is
GridColumns, GridColumnLabels, SheetGrid, BulkMarking, the PlotView and ViewReading family
and PanelSteps, the scope boxes, the sections with PlotBox, the schedules, the sheets, the
run with SiblingView and ReportPlaces, and the scan with the Scanned family and the parse
tallies. Nothing was created.

Every Shared read, checked by grep over the fifty files: `PlotId` for `IsPlotId`,
`TryRead` and `PrefixOf`, `PlotRange`, `PlotSelection`, `NaturalOrder.Comparer`,
`Lengths`, `ScanFileName`, `ViewNameParser` and `ParsedViewName`, `ViewType`, `Point3D`,
`Vector3D`, and the registry family `PlotRegistry`, `PlotRegistryResult`, `PlotRecord`,
`PlotSource` and `IgnoredName`. All are fair reads of what they are. Two observations
rather than findings: `PlotRange` and `PlotSelection` are read by Drawing Sheet's panel
alone and KPI has a plot picker of its own, so they are Shared by the user's decision
rather than by use, and the wrong-case chain in `PlotId` and `ViewNameParser` runs on
every refresh to produce a list nothing reads, finding 12. The one Shared thing that should
live in DrawingSheet is finding 28.

Drawing Sheet edits nothing outside its territory. Its lines in RcrcGreenApplication.cs are
two blocks, the registration at :40 to :44 and the button at :52 to :63, which is more than
the one line territory.md allows for and the same shape KPI's share has. No Drawing Sheet
file names Kpi, checked by grep: the only hit under src/RcrcGreen.Revit is the shared
application class. The five shared root files are read and not edited.

No file does two unrelated jobs. SheetNumbers.cs holds offers, proposals and clashes, all
about sheet numbers, and SiblingView.cs holds the crop, the annotation choice, the sibling
and the choice, all about one view's setup. Two files doing the same job are findings 14,
20 and 26.

Core holds no Revit reference. RcrcGreen.Core.csproj has no package or assembly reference
at all, grep for Autodesk under src/RcrcGreen.Core finds one comment in
ReportPlaces.cs:26 naming the Addins folder and one in a KPI file, and the net8.0 test
project loaded Core and ran 942 tests here, which it could not do with a Revit type in it.

### 4, no vibe coding

Unreachable members are finding 21, found by a scan of every public and internal member in
Core/DrawingSheet and ScopeBoxScanner for a reference anywhere in src beyond its own
declaration and constructor. Unreachable commands are finding 14. No parameter always
passed one value was found: `SectionAxis.ShortSide` is the one value the product passes to
`Across` and the tests pass both, and the axis was the user's decision to keep. No branch
that cannot be reached was found beyond finding 14. Constants written twice are finding
20. Comments that no longer describe the code are findings 18 and 23. Names that say
something the code does not: `sheetsIncomplete` in finding 24, and `RunSaysWhenEverySheet`
in the tests, which asserts a message that shows when any sheet is incomplete. Code kept
for later is `FilterValue.TryParse` in finding 21.

### 5, QA and QC

942 tests at `82d95f4`: 459 KPI and 483 apart from KPI, measured with two filtered runs
whose counts sum to the full run. Of the 483, 83 cover Shared types, the parser, the plot
identifier, ranges, selections, natural order, the registry and the file name, and 400
cover Drawing Sheet Core. By class, the largest are the parser at 27, section placement,
panel steps and grid columns at 22 each, the registry at 16, and the schedule definition
and scan report at 15. What they cover is Core rules, formatting and words: the grid, the
marks, the run plan and outcome, the run, scan and scope box reports, the sheet division,
naming, numbering, rows, batches, layout and size, the sibling choice, the schedule
definition and its filter kinds, and the panel steps. What they cannot cover is every file
under src/RcrcGreen.Revit, which the gate now compiles and no test loads, and the panel's
own arithmetic, which is where finding 1 lives with no test able to see it.

Types in Core/DrawingSheet named in no test: `FamilyTypeCount`, `ScheduleFieldKind` and
`ViewportSpot`, all three exercised through their parents. Every other type is named.

Tests that would still pass with the behaviour they name broken, judged by reading every
flat test file: the three closing note tests in RunReportTests, which assert the report's
prose and not the rule the prose describes, `TheHeaderSaysWhichCasesWriteAndHowMatchingWorks`
in ScopeBoxReportTests for the same reason, `EveryCaseListHasAsManyEntriesAsItsCount` in
ScopeBoxCaseListTests, where `Of` is defined as `In(outcome).Count` so the two sides are one
expression, `TheyComeBackInReadingOrder` in SheetLayoutTests, which holds for many wrong
layouts, and `NothingOfferedIsAlreadyInUse` in FreeSheetNumbersTests, which any non-empty
list of unused strings satisfies. Three were tried, and one judgement was wrong:

- Break 1, the report printing the plan under CREATED, PLAN VIEWS, the fault the report
  exists to stop. I judged `NothingAppearsAsBothCreatedAndNotCreated` hollow because its
  plan is empty. Result: Failed 4 of 942, and that test was one of the four, because it
  counts five created entries and the break printed three. It is not hollow. Restored, 942
- Break 2, `ScopeBoxCounts.In` dropping the first view of every case. Result: Failed 8 of
  942, and `EveryCaseListHasAsManyEntriesAsItsCount` stayed green while the count and the
  list were both wrong together, which is the hollow it was judged to be. The suite caught
  the break through four ScopeBoxCaseListTests and four ScopeBoxCountsTests. Restored, 942
- Break 3, `SiblingChoice.For` picking a sibling of any view type. Result: Failed 1 of
  942, `AViewOfAnotherTypeIsNeverChosen`, while `TheClosingNoteSaysTheSetupComesFromASiblingView`
  stayed green with the rule it names broken. Hollow as judged, and covered elsewhere.
  Restored, 942

Every restore was a reversed edit rather than a checkout, and `git status` was empty after
the three.

The gate restores and builds the whole solution and runs on pull requests and on pushes to
main. It still cannot catch anything about WPF or the Revit API at runtime, anything in the
panel's own logic, a push to main except after it has landed, and anything GitHub does on
its own servers, where the commit hooks do not run.

Hooks: all four fire and block, checked by doing the thing each refuses against a scratch
git index so the real index was never touched. `require-file-on-commit.sh` refused a
commit holding one Drawing Sheet file and no state file, naming the state file pattern.
`territory-check.sh` refused that file next to a staged deletion of a KPI file, naming both
tasks and both files. `writing-check.sh` refused a commit message holding the word the
rules list first, naming the word, and refused a staged blob holding an em dash, naming the
line. `block-paths.sh` refused a Write to /home/user/hook-probe-outside.txt naming the path,
and no such file exists. Nothing was committed by any probe and `git status` was empty
afterwards.

Never executed in Revit even once, from the log entries for the twenty fifth, fortieth and
forty second passes, each of which says nothing in its round has been through Revit, and
from the previous audit's list: the divided sheet shape end to end, the name and number
proposals, the step 4 table, the keystroke refresh, the placement reads into
`ViewportRecord`, the scan's viewport section, the delete-again on a refused sheet number,
the plot list as a union with its box-only rows, the capture loss recording, the
presence refusal, the rename-then-delete path, the `Set` return checks, the uncapturable
refusal wording, and the moved folders themselves. The brief's belief about the sheet
division, naming and numbering is right, and the fortieth pass's additions join them.

### 6, interface

Read as a production person who has not seen it. Findings 1, 5, 6, 8, 13, 24, 25, 29 and
31. Beyond them: no control was found that does nothing, every step below plots is gated on
plots being done, Run and Assign both confirm with counts, the step 4 count comes from the
same rows the table shows, and every dropdown comes from the model. UNKNOWN without Revit:
whether the step headers, trimmed with an ellipsis at :364, hide their summary at a docked
width, since 1 PLOTS DM-11 to DM-28, 17 of 17 ticked is 41 characters in bold, and whether
the four-column step 4 table and eight grid columns fit.

### 7, reports

Finding 7 is the one internal disagreement in the run report by construction, and finding
10 is one by accident. Units were checked line by line: the scan says scope boxes are
feet and prints them as feet, viewports and sheet sizes print millimetres from the one
conversion in `Lengths`, the section depth prints metres from the constant that cuts it,
and the scope box report carries no lengths. Created, refused or skipped and reaching no
report: the marks and ticks Refresh clears in finding 6, the marks the run drops for
plots outside the range in finding 1, the WrongCase names in finding 12, the duplicate
field in finding 27, and a false `Set` in finding 4. In a report and not actionable:
the sheet number tally in finding 16, and case B of the scope box report, which says so
itself. Nothing else in a report was found that the user cannot act on.
