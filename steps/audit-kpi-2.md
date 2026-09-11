# Audit 2, the KPI tool, 2026-09-10

Main at `82d95f4`, 942 tests, 459 of them KPI, which is the snapshot every finding, line number
and count below describes. Read only: nothing was fixed, renamed or moved, and the only file this
branch touches is this one. Three deliberate breaks were made to see whether a test would notice,
watched, and put back byte for byte with md5, and the suite was rerun green at 942 afterwards.

Scope is everything under a `Kpi/` folder in Core, Revit and the tests, plus the KPI lines of
`RcrcGreenApplication`, `PanelMetrics` and `install.ps1`, and the four hooks. The first audit is
`steps/audit-kpi.md` at `92dd36c`. Part A below goes through its 27 open findings first, as
asked, and nothing in it is restated or re-argued.

**20 new findings are listed, numbered 30 to 49 so the two audits can be cited together**, under
the cap of 40: 0 BLOCKS, 3 WRONG, 7 COSTLY and 10 TIDY. **9 findings with no user cost were
dropped** and are named at the end. Part A's 27 are not counted against the cap, because they
are the first audit's and every one of them still stands.

One file outside this one is touched, `steps/ai-max-state-kpi.md`, because
`require-file-on-commit.sh` refuses any commit that does not carry it. No code, no test and no
rules file was changed.

**The shape hunted for this time is a right line of code standing on a fact measured once.** The
subtotal rule was that shape and it was wrong for four rounds. Findings 30, 36 and 37 are that
shape, and the logic notes list every rule in the tool that rests on one observation, with what
it was measured on and whether it has held since.

## Part A, the 27 open findings of steps/audit-kpi.md

Every one of the 27 STILL STANDS. None was passed by when the code moved in pull requests 52
and 53, and none was found to have been untrue. The line numbers are as the files read today.

- 2 STILL STANDS. `ScheduleRows.cs:165-167`, `:179-199`, `:288-290` and `ScheduleGroups.cs:99-101`,
  all four fallbacks untouched, and the docstring at `ScheduleRows.cs:126-127` still describes
  the fallback as intended
- 3 STILL STANDS. `KpiPanel.cs:385` returns before `ThePlots()` at `:438` until a hand pick, and
  `Preselect()` returns at `:787` on that same pick. The second half, the per plot component off
  `componentNames[0]`, is `KpiRequestHandler.cs:214`
- 4 STILL STANDS. `KpiNames.cs:19`, `KpiPanel.cs:862`, ordinal at `PaneChoices.cs:53-54`
- 5 STILL STANDS. `SpeciesList.cs:122` against `:125`
- 6 STILL STANDS. `KpiPlotReader.cs:273` sets the flag before the rows are looked at, `:328-332`
  turns a refused read into a schedule with `RowsWereRead` false and records it nowhere, and
  `Reconciliation.cs:218` then says the schedule listed no species
- 7 STILL STANDS. `WorkbookPatcher.cs:337-338` returns false for a part that is not there,
  `PatchOutcome.cs:94` requires true
- 9 STILL STANDS. `KpiPanel.cs:558` builds the list's `ScrollViewer` on every redraw and
  `:769-777` redraws on every tick
- 10 STILL STANDS, re-proved today. Swapping shrubs and lawn on the way to their cells in
  `KpiCreatePlan.cs:113-114` left the suite green at 942. `KpiCreateTests.cs:311`
- 11 STILL STANDS, re-proved today. Replacing the read back at `WorkbookPatcher.cs:115` with the
  value that was sent left the suite green at 942. `WorkbookPatcherTests.cs:156-168`
- 12 STILL STANDS. `KpiRequestHandler.cs:80-90`
- 13 STILL STANDS. `PlotReading.cs:127` and `:136`, neither name in `KpiCreateReport.cs` or
  `KpiReport.cs` by grep, and the docstring at `:132` was rewritten in pull request 53 and still
  says the sum is printed
- 14 STILL STANDS. `KpiCreateReport.cs:298`
- 15 STILL STANDS. `KpiPlotReader.cs:212-213`, `PlotReading.cs:227-228`
- 16 STILL STANDS. `KpiPanel.cs:352-358`
- 17 STILL STANDS. `KpiRequestHandler.cs:168`, `KpiMerge.cs:231-234`
- 18 STILL STANDS. `Reconciliation.cs:141-142`
- 19 STILL STANDS. `KpiPlotReader.cs:305-336` against `KpiScheduleReader.cs:395-432`
- 20 STILL STANDS. `KpiPanel.cs:593`, `PlotList.cs:166-169`, `CreateWords.cs:73-74`
- 21 STILL STANDS. `KpiPanel.cs:424-438`
- 22 STILL STANDS. `KpiPanel.cs:58-59`
- 23 STILL STANDS. `Reconciliation.cs:47-48`, the inline text at `:153-157`
- 24 STILL STANDS. `KpiCreatePlan.cs:73-76`, the inline filter at `KpiCreateReport.cs:277`
- 25 STILL STANDS. `WorkbookPatcher.cs:148`, callers in tests only by grep
- 26 STILL STANDS. `CreateWords.cs:116`, unreachable past `KpiPanel.cs:387-407` and `:831`
- 27 STILL STANDS. `KpiCreateRun.cs:101-104`
- 28 STILL STANDS. `WorkbookPatcher.cs:17`, the copy at `:68`, the catches after it at `:126-141`
- 29 STILL STANDS. `KpiReport.cs` 1122 lines, `KpiPanel.cs` 1058, `KpiScheduleReader.cs` 632,
  `KpiQuestions.cs` 597

Of `steps/audit.md`, finding 16 still stands too: `KpiReport.cs:20` and `ScanReport.cs:22` are
one constant written twice.

## Ranked findings

### BLOCKS

None new. Finding 2 of the first audit is the open BLOCKS and is unchanged.

### WRONG

30. LOGIC | `src/RcrcGreen.Core/Kpi/ScheduleRows.cs:30-47` | **`CellNumber.In` reads digits up
    to the first character that is not one, so a printed area with a digit grouping separator
    reads as its thousands.** "1,234 m2" reads as 1. Every printed value this reader has been
    measured on is under 1,000 m2: 35, 70, 105, 820 and 1,020 items, and the two four figure
    values ever seen printed, 1161 and 3729, came off `AsValueString` on the one project, whose
    unit format prints no separator. Whether any other project's does is UNKNOWN, and nothing
    in the tool reads the setting: `KpiReader.Unit` takes the unit label and the rounding off
    `FormatOptions` and never asks it about grouping | This is the subtotal fault's shape
    exactly. On a project with grouping on, a one phase group over 999 m2 prints two rows
    reading alike, both read as their thousands, the add-up check passes because 1 equals 1,
    and 1 is written into F10. A two phase group of 1,200 and 1,300 with a total of 2,500 reads
    as 1 plus 1 equals 2 and passes too. Parks and streets are the templates whose areas run
    past a thousand and neither has ever been run | A refusal in `CellNumber` for a cell holding
    a digit after a non digit, or the grouping setting read once off the project units and
    named in the report

31. LOGIC | `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:260-285` with
    `src/RcrcGreen.Core/Kpi/Reconciliation.cs:149-182` | **The area is read, totalled and
    refused on for every template, and STREETS has no area cell.** `Create` reads every plot's
    filled regions, `KpiMerge.Area` totals them and `Reconciliation.Of` refuses on two regions
    holding an area and on two plots reading alike, with no knowledge of which template is
    being filled. `KpiCreatePlan` skips the area for STREETS a step later with
    `TypedByHand` | **The first STREETS run will refuse on a number it is not going to write.**
    MM-03 and MM-04 are street plots and both read 12182.05561411 in the 00 link, measured on
    the 1355 scan. The STREETS button ticks 78 plots, the read takes about fifteen seconds a
    plot on the 0928 measure, and the run ends asking the user to confirm an area the workbook
    has no cell for, then reads all 78 again after the confirm. Any street plot whose two
    regions both hold an area asks the same question, one full run per pick. The user's own
    list says STREETS has never been picked and the 78 plot run never attempted, and this is
    the first thing it will meet | A template on the reconciliation, or the region read skipped
    when the template's map holds no area cell

32. REPORTS | `src/RcrcGreen.Core/Kpi/ScheduleRows.cs:168-171` with
    `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:136-170` | **The softscape schedule prints a
    TOTAL row and nothing reads it, so a species row the reader drops is invisible.**
    `SoftscapeRows` skips a row whose botanical cell is empty or whose count does not read as a
    whole number with a bare `continue`, and the create report prints how many species rows it
    kept and never what the schedule says they add to. DM-12 prints TOTAL 39 | The first real
    workbook read 31 trees where the model held 39 and nothing in the tool said so. That cause
    is fixed, but the check that would have caught it is still absent, and the same check is
    the only thing that would catch finding 30 on a count column. `kpi-rules.md` allows adding
    printed numbers and this is a printed number. A skip with nothing written down is the
    fault `CLAUDE.md` names by that sentence | The TOTAL read off the row whose first cell holds
    that text, held against the species sum in the report, and the two skips named

### COSTLY

33. QA | `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:193-197` and
    `src/RcrcGreen.Core/Kpi/CreateWords.cs:305-308`, with
    `tests/RcrcGreen.Core.Tests/Kpi/CreateFixture.cs:45-82` | **No test builds a run that
    wrote.** Proved: printing CELLS WRITTEN off the plan's stored values rather than off the
    landed cells left the suite green at 942, 0 failed. `PatchOutcome.Done` is constructed in no
    test file but the patcher's own, `CreateFixture.Run` always hands over a refused or null
    outcome, and the status line's "N cells written from M plots" is asserted nowhere | The
    section whose heading says every cell was read back off the output file and never as it
    was sent is the one section no test reaches, and finding 11 is the same hole one step
    upstream. The rule holds by intention twice over. Restored and rerun green | A run in the
    fixture carrying a `PatchOutcome.Done`, and one assertion per value against its landed cell

34. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:749` and `:702` | Every region choice
    button calls `AskedToCreate()`, and the confirm for identical areas redraws so the user
    presses Create again. Each is a full run: every ticked plot's sheets, schedules and regions
    read again from the start | On the 0928 measure that is five minutes for 20 plots and about
    twenty for 78, once per plot that needs a pick, and finding 31 says the first STREETS run
    asks at least once. A refusal exists so a person can answer a question, and the answer
    costs a whole run each | The choices collected before a rerun, or the regions read once and
    held on the run for the next press

    FIXED. The run the pane holds travels on the ask, `HeldReadings.Decide` says whether it
    can answer this press, and a region choice or a confirm is applied to its readings with
    `Applied` and nothing read. The model is read again when there is no run, the run before
    wrote, or the model, template, template file, a parameter or the plots differ, or a plot
    has no reading, each named, and the report says which under the Run line. Not observed in
    Revit.

35. WIRING | `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:257-276` with `:160-179` and
    `src/RcrcGreen.Core/Kpi/Reconciliation.cs:113-133` | **One refused plot read ends the whole
    run with no report, and the two refusals built for exactly that can never fire.** The loop
    over the ticked plots calls `KpiPlotReader.Read` under no guard of its own, so a throw on
    plot 60 of 78 falls to `Run`'s catches, which say "Revit would not do that now" with no
    plot named and write nothing. `Printed` catches `ApplicationException` only. The
    reconciliation's "plots were ticked and N were read" and "plots were read that were not
    ticked" are reached by tests alone, because `readings` always holds one entry per ticked
    plot | The scan side guards every section and names every skip at the top of its file. The
    create side, which takes twenty minutes on the run the team is about to try, has no such
    guard, so a bad plot costs the run and the report that would have named it | A guard per
    plot that records the refusal on the reading, which finding 6 asks for as well

    FIXED. `GuardedRead` in the handler wraps each plot's regions and read and hands back
    `PlotReading.NotRead` with the type and the message, and each schedule's read inside
    `KpiPlotReader.Read` is guarded the same way naming the schedule. The run carries on, the
    reconciliation refuses naming the plot, and the report is written. The two refusals for a
    short or a long list stay as the backstop tests reach. Finding 6's catch in `Printed` is
    untouched. Not observed in Revit.

36. LOGIC | `src/RcrcGreen.Revit/Kpi/KpiPlotReader.cs:271-283` with
    `src/RcrcGreen.Core/Kpi/PlotReading.cs:241` and `src/RcrcGreen.Core/Kpi/KpiMerge.cs:293-308`
    | **One softscape schedule and one shrubs and lawn schedule per plot is assumed and never
    counted.** Every schedule filtered on the plot whose name holds SOFTSCAPE has its species
    appended, and `KpiMerge.Species` adds them across schedules by name and group. Every one
    holding SHRUB or LAWN has its subtotals appended, and `SubtotalHeaded` takes the first group
    with the heading. Measured on 21 plots with one of each. The model holds eight distinct
    schedule names over six per plot, and which of the eight hold those words is in the scan
    report, which is not in this repository, so whether any plot holds two is UNKNOWN | A second
    softscape schedule on a plot, a working copy named like the sheets numbered "010QE Copy
    001", doubles every tree count in the workbook without a line anywhere. A second shrubs one
    is ignored past the first, in silence | A count per kind on the reading, refusing on more
    than one

37. LOGIC | `src/RcrcGreen.Revit/Kpi/KpiPlotReader.cs:367-374`, with `:81` and `:144` | **A
    plot's component and reference are read off one sheet, the first by sheet number, and
    nothing checks the plot's other sheets agree.** DM-11 has fourteen. `AgreedValue` checks
    agreement across plots and never within one. The 1548 scan measured `PRX_Component` on
    1,384 of 1,385 sheets, and the same model carries 6 views named for one plot holding
    `PRX_Plot_ID` for another, so its per plot facts are known to disagree elsewhere. Whether
    any plot's sheets disagree on the component is UNKNOWN, and section 3 of the scan report
    holds every value per sheet and counts it per plot only | D3 and C5 are written from one
    sheet, the template is preselected from it, and a plot whose first sheet is the one of 1,385
    without a value reads as a plot with no component | The values read across the plot's
    sheets, with a disagreement named the way `AgreedValue` names one across plots

38. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:130` | **The date box is `DateTime.Now`
    at construction, and the pane is constructed during OnStartup.** The comment at `:135-136`
    says so about the first read and the date was left on the same line. A Revit left open
    overnight offers yesterday's date, and the box is prefilled rather than asked for again
    when the pane is shown | A copy taken once and never refreshed is the fault `CLAUDE.md`
    names in those words, and it is on the one value the tool writes into E5 of every
    checklist. The user can see and retype it, which is the only reason this is not WRONG | The
    line moved into `Shown()`, or the date read at the press

39. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:352-358` with
    `src/RcrcGreen.Core/Kpi/RecognisedWorkbook.cs:102-116` | **A filled workbook is recognised
    as a template.** Recognition is the first sheet's name, and a filled MOSQUES output's first
    sheet is `<Mosques>`. Writing into the templates folder is allowed since pull request 52 and
    the pane says so, and the next redraw lists "MOSQUES DM-12.xlsx" as a MOSQUES template
    beside the client's | Picking it copies a filled file as the template: the species written
    into empty rows last time are names in column D now, so they match and the empties are
    fewer, and E5, G5 and H5 hold last time's typing under this run's | A workbook whose E5 is
    not the placeholder named as filled rather than offered, which the tool can tell because it
    is what wrote it

    FIXED. The peek reads E5 of the first sheet with a shared string resolved, `Recognise`
    names a file whose E5 is not `KpiTemplates.DatePlaceholder` as a filled checklist of its
    template with the cell's text in the reason, and the pane lists it greyed in the same list.
    Nothing is deleted or moved and browsing is untouched. The placeholder is one observation
    and is for Bader to confirm on all seven. Not observed in Revit.

### TIDY

40. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/ScheduleRows.cs:202-227`,
    `src/RcrcGreen.Core/Kpi/Reconciliation.cs:160-162` and
    `tests/RcrcGreen.Core.Tests/Kpi/ReconciliationTests.cs:168-195` | **The replaced subtotal
    rule stands in two comments and two test names.** The class docstring on `ShrubsAndLawnRows`
    draws the DM-11 rows with "subtotal AGAIN" and says the subtotal prints twice, forty lines
    above `Close`, whose docstring says the last row is the group. The reconciliation's comment
    says the same. Two tests are named for a subtotal that printed twice and build the old
    disagreement text by hand, so they pass under either rule | Pull request 53 corrected the
    rules file, `CLAUDE.md`, the log and two docstrings, and the record in the file that holds
    the rule was missed. The next reader of `ScheduleRows.cs` reads the wrong rule first | Two
    comments and two names

41. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/TemplateWords.cs:29-46`,
    `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:396-404` and
    `tests/RcrcGreen.Core.Tests/Kpi/ScheduleRowsTests.cs:233-244` | Three docstrings stacked two
    deep. The first describes the deleted overwrite sentence above the one for
    `OutputIsTheTemplateFolder`, with a stray line after its closing tag. The second describes
    `Exactly` above `Seconds`. The third describes the hardscape zero above the DM-12 rows | A
    comment left standing when its code moved is the shape finding 22 already names once | Three
    deletions

42. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/KpiFillValues.cs` with
    `tests/RcrcGreen.Core.Tests/Kpi/KpiTemplatesTests.cs:124-166` | `KpiFillValues` and
    `SpeciesCount` are constructed by nothing in `src/`, checked by grep. The docstring says it
    is the seam the next round plugs into, and that round built `KpiCreatePlan` instead. Three
    tests cover the seam | A 79 line description of a design that was replaced, kept green by
    tests of itself | A deletion, with its tests

43. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/OutputName.cs:16-19` against
    `src/RcrcGreen.Core/Kpi/CreateWords.cs:353-363` | Two name suggesters. `OutputName.Suggested`
    is the template's file name and its docstring says the component name comes next round.
    `CreateWords.SuggestedName` is that next round, reached only through `Preselect`, which
    finding 3 says never runs past its guard | Two records of one suggestion, and the one that
    can run says the other is coming | One suggester

44. STRUCTURE | `src/RcrcGreen.Revit/Kpi/KpiPlotReader.cs:392-409` against
    `src/RcrcGreen.Revit/Kpi/KpiLinkReader.cs:53-80` | Two rules for which link documents are
    read. The create side takes every instance whose name holds `00` anywhere, `KpiNames.Holds`
    matching a word with no letter by plain `IndexOf`, and de-duplicates on the document title.
    The scan side reads every link and de-duplicates on path and title | Measured on one model
    with one link. A second link whose name holds two noughts, a 2000 or a 100 in a file name,
    is read by Create and not told apart by the scan's list, and its regions carrying a plot
    make a refusal or a wrong pick | One rule, in Core, that both read

45. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/KpiCreateRun.cs:12-13`,
    `src/RcrcGreen.Revit/Kpi/KpiPlotReader.cs:262-267` and
    `src/RcrcGreen.Core/Kpi/PlotReading.cs:129-135` | Three docstrings claim what the code does
    not do. `KpiCreateRun` says every count the report prints comes off the outcome, and CELLS
    NOT WRITTEN and both species sections print off the plan. The schedule note says a name
    that disagrees with its filter is recorded and neither is resolved, and the schedule is
    used, so the filter won. `SpeciesSum` says it is printed, finding 13 | A docstring that
    overclaims is read as a guarantee, which is finding 27's shape three more times | Three lines

46. INTERFACE | `src/RcrcGreen.Core/Kpi/WorkbookPatcher.cs:130-133` against
    `src/RcrcGreen.Core/Kpi/CreateWords.cs:251-256` | Two sentences for one condition. The
    patcher's own `IOException`, reached when the delete passed and the copy did not, says
    "The file could not be written" and nothing else. The handler's says what to do about Excel
    first | The advice sits on one of two paths that meet the same failure | One sentence

47. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:649` with
    `src/RcrcGreen.Core/Kpi/CreateWords.cs:140-144` | The reference values block says "the first
    ticked plot" and shows `_ticks.Ticked[0]`, which `PlotTicks.Ticked` hands back in model
    order. Tick DM-12 and then DM-11 and the block shows DM-11 under a line saying it was ticked
    first | The block exists so a person picks by looking at a value, and the words say which
    plot it is in a way that is not true | A word

48. QA | `tests/RcrcGreen.Core.Tests/Kpi/CreateFixture.cs:98` against
    `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:264-269` | The rule that chooses a plot's
    region when exactly one holds an area exists twice, in the fixture and in the handler. Every
    reconciliation and merge test takes the fixture's choice, and the handler's is in the Revit
    project where no test reaches it | The choice is what finding 15 turns on, and the tested
    copy is not the one that runs | The rule moved into Core, where the fixture can call it

49. INTERFACE | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:130` with
    `src/RcrcGreen.Core/Kpi/KpiCreatePlan.cs:166` | The date goes into E5 as inline text,
    "2026-09-10", through `CellWrite.Text`. Whether E5 is a date cell that a formula reads is
    UNKNOWN. The annotated set says DATE OF THE DAY and nothing about its type | A text where a
    date is expected shows fine and computes nothing | UNKNOWN until one workbook's E5 is looked
    at

## Area notes

### 1, logic

All 59 files in `src/RcrcGreen.Core/Kpi` and all 13 in `src/RcrcGreen.Revit/Kpi` were read.

**Every rule in the tool that stands on one observation**, with what it was measured on. This
is the hunt the brief asked for, and it is listed whole so the ones that hold are on the record
beside the ones that do not.

- `CellNumber` reads no digit grouping. Measured on printed values under 1,000 on one project
  whose four figure values print without a separator. Finding 30
- One softscape and one shrubs and lawn schedule per plot. Measured on DM-11 to DM-13 and the
  20 mosque plots. Finding 36
- A plot's component and reference off its first sheet. Never measured against a plot's other
  sheets. Finding 37
- The STREETS area typed by hand. Measured on the annotated set, and it holds. What the create
  path does with it is finding 31
- The link found by `00` anywhere in the instance name. One link on one model. Finding 44
- The subtotal shape. Measured on DM-11 alone until the 0928 run, wrong for four rounds, fixed
  in pull request 53 on 21 plots. Holds, and finding 40 is its leftover
- A group row is one cell holding a phase name. 21 plots. Holds
- `IsStructureRow`, text in the first cell and nothing else. 21 plots. Holds
- `ScheduleColumns.Holding` takes the first heading holding the word. Measured on one schedule
  design, the eleven columns every mosque plot's copy shares. Whether the street and park
  plots' schedules share it is UNKNOWN, and a heading such as CANOPY AREA ahead of AREA (sqm)
  would pick the wrong column. Dropped, because no such heading has been seen
- The tree list row ranges. The annotated set, once. Finding 5 stands
- The eleven component values. The 1548 scan, one model, and a twelfth prints as not in the
  table rather than being guessed. Holds
- The prefix table. Confirmed by the team, all ten. Holds
- `HoldsAnArea` is an area above nought. DM-11's cadastral reads 0. Holds
- Raw areas are square feet. Revit's own rule. Holds
- The main sheet names carry angle brackets. All seven real workbooks. Holds

**Reading a schedule value by cell position.** Every site in the first audit's list was checked
again and none has moved: the four fallbacks of finding 2, the correct refusal at
`ScheduleRows.cs:249`, `IsStructureRow` at `:109` and `GroupNameIn` at `ScheduleGroups.cs:72`.

**Two records of one fact, every instance this repo has named, with a total.** Eight were bugs
that shipped, all named in `CLAUDE.md` and counted there. Six stand open from the first audit.
Five are new here and one is the first repo audit's. One is held open on purpose. **Twenty one, of which twelve stand in the code
today.**

```
 1  Drawing Sheet  a view named for DM-12 filled DM-11's cell             fixed
 2  Drawing Sheet  the column count against its list                       fixed
 3  Drawing Sheet  the run report printed the plan as the outcome           fixed
 4  Drawing Sheet  a panel step reading the range off its arguments         fixed
 5  Drawing Sheet  one method filling two controls, split and half kept     fixed
 6  Drawing Sheet  a schedule category read one way, looked up another      fixed
 7  KPI            the species reader on the column, the counter on cell 0  fixed
 8  KPI            the model's folder handed to the open model words        fixed
 9  KPI            audit 1, 3: the component that preselects, the one written    STANDS
10  KPI            audit 1, 5: the tree list matchable range and its empty range STANDS
11  KPI            audit 1, 17: two area readers, one asking HoldsAnArea         STANDS
12  KPI            audit 1, 19: two row readers, one capped at 200               STANDS
13  KPI            audit 1, 23: ChooseTheRegion and its inline twin              STANDS
14  KPI            audit 1, 24: Unmatched and its inline twin                    STANDS
15  KPI            40: the subtotal rule in the code and in its comments         STANDS
16  KPI            43: two name suggesters                                       STANDS
17  KPI            44: two rules for which link is read                          STANDS
18  KPI            46: two sentences for one write failure                       STANDS
19  KPI            48: the region choice in the fixture and in the handler       STANDS
20  KPI            steps/audit.md 16: LineEnd in KpiReport and ScanReport        STANDS
21  KPI            the plot list from sheets and from schedules, held open       on purpose
```

**Recomputing from elements.** No path sums elements. `grep` for `.Sum(` and `Aggregate(` across
`src/RcrcGreen.Revit/Kpi` returns nothing, unchanged from the first audit.

**Case and numbers.** Unchanged from the first audit, apart from finding 30. `SpeciesMatching`,
`ComponentTemplates` and `PlotPrefixes` compare without case and trim, `Preselected.From` is the
one ordinal compare that should not be, finding 4, and `Totalled.Adds` and `Disagreeing` share
one tolerance constant.

### 2, wiring

All 13 files in `src/RcrcGreen.Revit/Kpi` were read.

**Every entry point into the handler**, traced again: `Shown`, `AskedForAScan`,
`AskedToCreate`, `RedrawTemplates`, the region choice buttons and the identical area confirm.
Every one calls `Ask` then `Raise`, the one slot of finding 12 stands, and the two new things
are findings 34 and 35: the region and confirm buttons each cost a full run, and the run has no
guard per plot.

**Nothing escapes `Execute`.** Unchanged. The inner catches are per type and finding 17 is the
one that mislabels. The scan side's `Guarded` names every skip and the create side has no
equivalent, finding 35.

**Copies the pane holds.** The list from the first audit stands, and one was missed there: the
date at `KpiPanel.cs:130`, finding 38. `_facts.ComponentPerPlot` is still read with one
parameter and never refreshed, inside finding 3.

**Core references no Revit type.** The csproj holds no package and no assembly reference, `grep`
for Autodesk across `src/RcrcGreen.Core` returns two comments, one of them the reason `FilePaths`
is plural, and the net8.0 test project ran 942 tests here.

**The per plot read cost** is measured in the log of the forty first pass and the user has said
to measure before deciding. It is not repeated here and not counted.

### 3, folders and shared code

59 files in `src/RcrcGreen.Core/Kpi`, two more than the first audit, `SamePath.cs` and
`RunTiming.cs`, both on the create side of the seam the first audit drew. 13 in the Revit folder
and 26 test files. The seam holds: no create file names a scan type, and `KpiReport.ShownRows`
is still the one crossing, finding 19.

**Every crossing between Kpi and the rest.** Unchanged. `grep` for Kpi across the Shared and
Drawing Sheet folders and the Revit root returns `RcrcGreenApplication.cs` and nothing else.

**Files named for one of several types.** Eighteen Core files hold more than one public type:
`KpiMerge.cs` holds seven, `KpiTemplate.cs` five, `PlotReading.cs` and `ScheduleRows.cs` four.
A reader looking for `Totalled`, `AgreedValue`, `TreeRows` or `GroupSubtotal` by file name finds
nothing. Not numbered, because finding 29 already says what the size costs and this is the same
cost from the other side.

**Duplicating the Drawing Sheet.** Unchanged. `RememberedFolder`, `RememberedNames` and
`ReportFile` each read a pointer file beside the assembly their own way.

### 4, no vibe coding

Findings 40 to 46, plus 48. **Public members nothing in `src/` calls**, swept again: the three
the first audit named still stand, `ChooseTheRegion`, `Unmatched` and `ReadBack`, and four join
them, `KpiFillValues` and `SpeciesCount` as finding 42, and `TreeRows.RowCount`,
`LinkFacts.AnyLoaded` and `ScheduleElements.ValuesOfNamesHolding`, which are dropped as
costless. `CreateWords.NoTemplate` is still called and cannot be shown, finding 26.

**A record not corrected** is the shape of this round. Pull request 53 corrected the subtotal
rule in five places and left it standing in two comments and two test names, finding 40. Three
stacked docstrings describe code that moved, finding 41. Three docstrings claim what the code
does not do, finding 45.

### 5, QA and QC

**459 KPI tests**, measured with a filtered run here, 30 more than the first audit's 429, and
942 together with the Drawing Sheet's. The 30 cover the two guards of pull request 52, the
subtotal shape and the timing of pull request 53. What the 459 cannot cover is unchanged:
everything in `src/RcrcGreen.Revit/Kpi`, which no test loads.

**Tests that would still pass with the behaviour broken.** Three breaks, each restored byte for
byte and checked with md5, each followed by a green rerun at 942:

- shrubs and lawn swapped on the way to their cells: **942 green, 0 failed**, finding 10 again
- the read back replaced with the value that was sent: **942 green, 0 failed**, finding 11 again
- CELLS WRITTEN printed off the plan rather than off what landed: **942 green, 0 failed**,
  finding 33, new

**Tests whose expectation shares a source with the code**, which is the shape the subtotal tests
took for four rounds. Read for in every KPI test file:

- `ReconciliationTests` hand `KpiMerge.Shrubs(readings)` in as the total to check, so the total
  and the parts come from one call and `Adds` cannot be false. One hand built `wrong` total
  beside them is what really tests the refusal
- `CreateFixture.Region` works the raw square feet out from the metres with its own constant,
  10.763910416709722, the inverse of `AreaUnits`. A change to the conversion would move the
  fixture's expectation with it. No test reads that raw against a hand written number, so it is
  dropped
- `CreateFixture.Plot` makes the region choice with the fixture's own copy of the handler's
  rule, finding 48
- `KpiTemplatesTests.TheMapHoldsSevenCompleteTemplatesAndEveryCellParses` walks the map and
  asserts it against itself, and is rescued by the theory beside it that writes every cell out
  by hand
- Twenty four assertions compare a printed line against the constant the code prints, such as
  `Assert.Equal(KpiCreatePlan.TypedByTheTeam, ...)`, which pins that the constant is used and
  never what it says. Dropped, because the constants are also asserted by their words elsewhere
- `SubtotalShapeTests` and `ScheduleRowsTests` write every expected number out by hand, which
  is why the subtotal fix went red on five tests when reverted

**The untestable list**, what a person checks by hand. Unchanged from the first audit, with
three added: the guard that is not there around each plot's read, finding 35, the date at
construction, finding 38, and the handler's region choice, finding 48.

**Never executed in Revit even once.** The user's list is right: no workbook has been written
since the cache fix, the species rows, the output folder, the two guards or the subtotal fix,
the identical area flag has never fired, STREETS has never been picked and the 78 plot run never
attempted. Everything in pull requests 52 and 53 joins it: the template guard on both sides, the
status line that never goes blank, the last row subtotal, the run timing and the counted
refusal. Finding 31 says what the 78 plot run will do first.

**The four hooks fire and block**, checked by feeding each its own payload and reading the exit
code. `block-paths.sh` refused a write to `/home/user/audit-probe-2.txt` and one to
`/home/user/RCRC-Green/../outside.txt`, exit 2, and passed `steps/audit-kpi-2.md`, exit 0.
`require-file-on-commit.sh` refused a commit carrying one Kpi file and no state file, exit 2.
`territory-check.sh` passed a commit carrying a Kpi file alone, exit 0, refused one carrying a
Kpi file beside a Drawing Sheet file, naming both, exit 2, and refused one carrying a Kpi file
beside `Core/Shared/PlotId.cs`, naming the Shared file, exit 2. `writing-check.sh` passed a
plain message, exit 0, refused a message holding a listed word, naming it, exit 2, and refused a
staged file holding an em dash whether the commit named it on the command line or not, exit 2.
One probe was made wrongly and is recorded: the same file left untracked and named on the
command line passed, exit 0, because `commit-scope.py` asks git what that pathspec changes and
an untracked file changes nothing. Git refuses that commit itself, so nothing gets through.
Nothing was committed by any probe and the tree was clean afterwards.

### 6, interface

Read against a production person who has not seen it. Findings 34, 38, 39 and 47, plus 9, 16,
20 and 21, which stand.

**Every string that reaches a `Button` or a `CheckBox` goes through `PaneLabel.Escaped`**,
checked again by grep over every `Content =` in `KpiPanel.cs`: the four that do not are the
`UserControl`'s own content, two `ScrollViewer` contents and one clear.

**Something changes under the user.** Three now: the name box, finding 21, the scroll, finding
9, and the date, finding 38.

**The action with no way back** is still the overwrite, said once under the name box. Finding
39 is a new way to it: the filled file offered back as a template.

**UNKNOWN without Revit**: whether the warning line for the templates folder wraps well at a
docked width, and everything the first audit listed.

### 7, the reports and the file

Findings 32 and 33, plus 6, 7, 13, 14 and 18, which stand.

**Can any section disagree with another.** The create report's timing lines from pull request
53 hold: the whole run, the read, and the rest add, and each plot's read prints beside it. The
one line found untrue is unchanged, "component parameter, chosen by the user" at
`KpiCreateReport.cs:344`, finding 4.

**Anything read, refused or skipped that never appears in a report.** Finding 6, the refused
schedule read, finding 13, the two unprinted values, and now finding 32, the two skipped rows
and the TOTAL nothing reads, and finding 35, the plot whose read threw.

**Units.** Unchanged, apart from finding 30, which is a printed number read short rather than a
wrong unit.

## Dropped, with no cost to the user

9 findings, named so nobody looks for them twice.

- `ScheduleColumns.Holding` takes the first heading holding the word, measured on one schedule
  design. No heading that would fool it has been seen
- `CreateFixture.Region` works raw square feet out with its own inverse of the conversion
- `TreeRows.RowCount`, `LinkFacts.AnyLoaded` and `ScheduleElements.ValuesOfNamesHolding` are
  called by nothing in `src/`
- `Patched` deletes the output before `WorkbookPatcher.Patch` copies over it with overwrite on,
  and the docstring says the delete is deliberate
- `SpeciesList.In` opens the template twice per run, once per tree sheet
- `KpiPlotReader.Sheets` does not filter `IsTemplate` where the scan side does, and a sheet
  cannot be a view template
- `KpiReport.PlotsReadInFull`'s docstring counts the two records fault as seven times, and the
  count is elsewhere
- `commit-scope.py` passes a pathspec naming an untracked file, which git itself refuses
- `CellNumber` reads a comma as the end of the number, which on a project whose decimal symbol
  is a comma reads 1131,72 as 1131. The project's rounding is to the metre, so no decimal has
  been seen printed

## Found outside the KPI tool and left alone

The Drawing Sheet's unescaped labels, which the first audit named and `CLAUDE.md` records, and
`steps/audit.md` finding 16, which stands on both sides of the seam.
