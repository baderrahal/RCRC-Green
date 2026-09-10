# Audit, 2026-09-10

Main at `e343fe1`, 619 tests, which is the snapshot every finding and count below describes.
Read only: nothing was fixed, renamed or moved. Every code break below was made to prove a
hole, watched, and put back, and the suite was rerun green at 619 afterwards. Findings are
ranked by cost to the user. 6 findings with no user cost were dropped. Before merging, this
was restacked onto `f8d454c`, where the KPI rounds put the suite at 904, and every file and
line a finding cites was checked unchanged between the two.

## Ranked findings

### BLOCKS

1. LOGIC | src/RcrcGreen.Revit/ScheduleCapture.cs:75 and :65 | Capture drops a filter whose
   value Revit refuses to read, and a field whose id resolves to nothing, both with a bare
   continue and nothing recorded. The writer's lost filter guard reads the captured
   definition, so a filter lost at capture never reaches it | A schedule missing its second
   filter is built and left in the model reading as correct. HARDSCAPE and SHRUBS AND LAWN
   are told apart only by that filter, so the schedule shows both categories' quantities on a
   drawing. This is the exact shape ModelWriter was fixed for, kept alive one step earlier in
   the pipe. Whether a real filter read throws is UNKNOWN without Revit, and the catch exists
   because it can | A method: record the loss on the definition and refuse it at create

   FIXED. Capture records every unreadable filter and field on the definition, a lost
   filter refuses the type at plan and at create, and a lost field is named under needs
   attention.

### WRONG

2. LOGIC | src/RcrcGreen.Revit/DrawingSheetReader.cs:59 and :84 | The panel's plot list is
   built from views alone. Scope box names feed only PlotsWithAScopeBox and element values
   are never read at all, so a plot existing only as a scope box with tagged elements never
   gets a row. PlotRegistry, which holds the union of three sources rule, is called by
   nothing in the product | CLAUDE.md says done means the tool lists every plot, and
   core-rules.md says the box-only plot is the one the team most needs. That plot cannot be
   ticked, so no view can ever be created for it, and the list reads as complete. How many
   real plots are box-only is UNKNOWN without Revit, and 406 boxes against 160 element values
   says the shapes differ | A round: merge box and element sources into the snapshot's list

   FIXED. PlotRegistry is wired in with PRX_Plot_ID on views as its own source, the
   reader walks elements through the scan's shared loop, and step 1 marks the plots no
   view carries.

3. WIRING | src/RcrcGreen.Revit/ModelWriter.cs:160, :271, :820 | A marked cell whose view now
   exists is still attempted, because RunPlan takes no presence data and the handler's fresh
   read narrows only scope boxes and types. ViewPlan.Create succeeds, the rename to the taken
   name throws, and the catch records a refusal while the new view stays in the model under
   Revit's default name | The comment at DrawingSheetRequestHandler.cs:357 claims the re-read
   stops exactly this. The report says not created while the model gained an orphan view, and
   a report that disagrees with the model is the fault this repo treats as worst. Needs a
   stale panel plus a colleague's edit, which is ordinary in a shared model | A method:
   re-check presence in the fresh read, and delete-again on a refused rename like MakeSheet

   FIXED. The plan refuses a mark whose view exists in the fresh read, and a refused
   rename deletes the orphan again with the delete checked, the MakeSheet way.

4. WIRING | src/RcrcGreen.Revit/ModelWriter.cs:191 and :734 | The bool Revit returns from
   Parameter.Set is ignored when the scope box and annotation crop are set on a new view,
   while AssignScopeBoxCommand.cs:150 treats the same false return as a refusal worth naming.
   Two rules for one operation | A view whose template or state makes Set answer false comes
   out without its scope box, or with annotation crop still off, while the report reads
   clean. Annotation crop off is the fault of the eighteenth pass, able to return silently.
   Whether Set answers false in practice there is UNKNOWN without Revit, which is exactly why
   the other caller checks | Two lines: check the returns and add the NeedsAttention note

   FIXED. Both Set returns are checked and a false lands under needs attention.

5. LOGIC | src/RcrcGreen.Revit/DrawingSheetPanel.cs:1649 against
   DrawingSheetRequestHandler.cs:371 | The panel's plan preview passes ScheduleTypes as the
   capturable set while the handler passes definitions.Keys, and ScheduleCapture.ByType drops
   a schedule with no plot-naming filter. Two rules for which schedules can be made, the
   seventh instance of the two-paths shape | Step 5 can promise a schedule the confirmation
   then refuses, and the refusal says no plot has that schedule when the schedule exists and
   filters on no plot, sending the user to look for the wrong thing | A line to pass the same
   set, plus a second refusal wording for the filter-less case

   FIXED. The snapshot carries the capturable set off the same capture the run uses, and
   an uncapturable schedule is refused with its own reason.

### COSTLY

6. QA | src/RcrcGreen.Core/DrawingSheetSnapshot.cs, whole file | The type feeding the panel
   everything has no test. Three deliberate breaks each left the suite green at 619:
   NumbersOnPlot returning empty always (kills every number proposal), the PlotId filter
   dropped from PlotsWithAScopeBox (feeds refusals wrong data), and FreeSheetNumbers fed
   SheetNamesInUse instead of numbers (fills the dropdown with garbage). All three run, all
   three green, code restored each time | The proposal and refusal feeds can break without a
   test noticing, and 619 reads as cover it does not give | A test file

    FIXED. DrawingSheetSnapshotTests pins all three, and each break was made again and
    went red before being trusted.

7. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:767 and :1550, with CellClicked at
   :2017 | The grid's outer and inner ScrollViewers are built without the remembered-offset
   overload, and every cell click calls Redraw, so marking a cell at row 15 throws the grid
   back to the top and the left. The same is true of the two 2-argument Scrolling calls
   inside a step 4 card | Losing the scroll position on every tick is the exact fault the
   user reported for the plot and view type lists, fixed there and not here. Marking ten
   cells down a long grid means ten re-scrolls | A line each: use the named Scrolling
   overload, or skip Redraw for a single cell flip

8. QA | .github/workflows/tests.yml:2 | The gate never compiles RcrcGreen.Revit, and its
   comment says the add-in needs Revit on the machine. CLAUDE.md says the opposite, that the
   Nice3point packages let it compile with no Revit installed, and the whole solution builds
   on this Linux box. The gate also runs only on pull requests, so a push straight to main,
   which this repo's history holds, lands unchecked | A Revit-side compile break merges green
   and costs whoever pulls next a broken build | A workflow line to build the solution, and
   one to run on main pushes

    FIXED. The gate restores and builds the whole solution, runs on pushes to main as
    well as pull requests, and its comment says why the add-in compiles without Revit.

9. INTERFACE | src/RcrcGreen.Revit/DrawingSheetPanel.cs:1751 | Refresh clears every mark with
   no warning and nothing said afterwards. The status line reports views read and plot counts
   and never mentions that marks were dropped | Someone who marks a screenful and presses
   Refresh to pick up a colleague's change loses the lot silently. The clear also guards
   against stale marks, so it cannot simply go, but nothing tells the user it happened | A
   sentence in the refresh message, or keep marks whose cells are still missing

10. REPORTS | src/RcrcGreen.Core/RunReport.cs:77 against :91 | The headline counts only what
    Revit refused during the run plus what was left behind, while NOT CREATED, DECIDED BEFORE
    THE RUN sits below with its own count. A run refusing 4 before and 2 during reads 2 were
    not at the top and lists 6 under not-created headings | A reader tallying the sections
    against the headline gets two different numbers for not created, in the report that
    exists because two of its numbers once disagreed. Each number is individually true and
    the intro sentence explains, but only to someone who reads it | A phrase in the headline
    naming the decided-before count

11. INTERFACE | src/RcrcGreen.Revit/ShowDrawingSheetCommand.cs:32 | The message shown when
    the pane fails to register says Scan Model and Scope Box on the Reports panel are
    unaffected. The Reports panel was removed rounds ago and both live inside the Drawing
    Sheet panel, which is the thing that just failed | The one time this message shows, the
    user is sent hunting for a ribbon panel that does not exist, in the middle of a failure |
    A sentence

### TIDY

12. NO VIBE CODING | src/RcrcGreen.Core/PlotRegistry.cs, PlotRegistryResult.cs,
    PlotRecord.cs, PlotSource.cs, IgnoredName.cs, PlotViewGrid.cs, MissingViewFinder.cs,
    MissingViewType.cs, PlotMissingViews.cs, plus PlotId.PatternAnyCase, FailsOnlyOnCase and
    ViewNameParser.FailsOnlyOnPlotCase | Nine files and the wrong-case detection chain are
    called by nothing in the product. They are the pre-panel design, kept green by
    PlotRegistryTests, PlotViewGridTests and DrawingSheetFlowTests | The dead family carries
    the union rule finding 2 shows the product lacks, and its tests inflate the count the
    gate reports. DrawingSheetFlowTests names a behaviour the shipped panel does not have |
    A decision round: wire the union in per finding 2, then delete what stays unused

    FIXED with finding 2. The registry family is live now. PlotViewGrid,
    MissingViewFinder, MissingViewType and PlotMissingViews stayed unreachable and are
    deleted with their tests. The wrong-case chain runs inside Build and is kept.

13. NO VIBE CODING | src/RcrcGreen.Revit/ScanModelCommand.cs:22,
    AssignScopeBoxCommand Execute path, ScanProgressWindow.cs | No button registers either
    command, so both Execute methods and the progress window they use are unreachable.
    ScanModelCommand.Execute is a second scan path that writes to the Desktop only, differing
    from the handler's, which writes both places | The next scan change lands in one copy,
    which is the split-and-half-kept fault this repo has already had once | A deletion, after
    checking the internal statics the handler does use stay

14. NO VIBE CODING | src/RcrcGreen.Core/RunOutcome.cs:73 and
    src/RcrcGreen.Revit/SiblingReader.cs:16 | Both comments still say a new view takes its
    far clip off the sibling. The far clip left two rounds ago for SectionDepth | A comment
    describing a rule the tool stopped following sends a reader to a wrong conclusion | Two
    lines

15. LOGIC | src/RcrcGreen.Core/RunPlan.cs:329 and :336, pinned by
    tests/RcrcGreen.Core.Tests/RunPlanTests.cs:157 | One refusal prints 1 things cannot be
    made and are named in the report, and the test asserts that exact string, so the grammar
    fault is enshrined | The confirmation dialog reads machine written for the singular case
    | Two lines and the test

16. NO VIBE CODING | src/RcrcGreen.Core/Kpi/KpiReport.cs:20 against
    src/RcrcGreen.Core/ScanReport.cs:22, and src/RcrcGreen.Revit/ModelWriter.cs:609 against
    ModelScanner.cs:220 | LineEnd is the same constant written twice, and Number(Parameter)
    is the same method written twice | Two copies of one fact is the shape this repo has been
    bitten by six times, held apart here only by the Kpi separation rule | A shared home for
    each, or a sentence in the rules saying the Kpi copy is deliberate

17. NO VIBE CODING | src/RcrcGreen.Core/SheetDivision.cs:23, PlotBox.cs:74,
    GridColumns.cs:55 | PlannedSheet.Position, PlotBox.Centre and GridColumns.HiddenCount are
    read by nothing in the product | Position was for saying sheet 2 of 3 and nothing says
    it, so a reader assumes a behaviour that is not drawn | A deletion each, or use Position
    in the step 4 table

18. STRUCTURE | design/pr-31/panel.html:5 | The folder says pull request 31 while the file's
    own title says pull request 28, which is where it landed. Pull request 31 is the KPI cap
    fix and has no mockup | Anyone tracing a round from its mockup lands on the wrong pull
    request | A folder rename

19. INTERFACE | src/RcrcGreen.Revit/DrawingSheetReader.cs:57 with the count shown at
    DrawingSheetRequestHandler.cs:223 | A view whose PRX_Plot_ID holds a non-plot value while
    its name parses is counted under with no plot at all, though its cell fills from the name
    | The count reads wrong to anyone who checks that view, and the present-and-wrong
    parameter case the enum documents as needing seeing is never shown apart | A fourth
    count, or a clause in the status line

## Area notes

### 1 and 2, logic and wiring

Every file under src/RcrcGreen.Core and src/RcrcGreen.Revit was read. The seventh two-paths
instance is finding 5. No Revit API call was found in either panel file: every Document,
Transaction, FilteredElementCollector and ElementId in DrawingSheetPanel.cs and KpiPanel.cs
is in a comment, checked by grep over both files. The KPI pane has its own handler and event,
and both handlers catch everything at Execute with a guarded last-resort message. The one
transaction outside the run is AssignScopeBoxCommand.Assign, reached only through the
handler, started and committed in one method. Documents and elements are fetched fresh per
request. SiblingReader holds View objects, but only inside one Execute call. The
FamilySymbol-instance fault has no new instance: SizeOf and the scan read SHEET_WIDTH off
placed blocks, and the KPI readers split instance and type homes on purpose.

### 3, folders and shared code

Core holds 69 files flat plus Kpi. The flat list is starting to cost: the dead family in
finding 12 sat unnoticed among live files, and the sheet work alone is now eleven files. If
folders come, the seams are already visible: Plots, Grid, ScopeBoxes, Sections, Schedules,
Sheets, Scan, Run. Nothing was created.

Kpi crossings, all of them: Kpi Core reads shared Core's NaturalOrder, PlotId, Lengths,
ScanFileName and ViewNameParser, and edits none of them. Kpi Revit reads shared PanelTheme,
PanelMetrics, ReportFile and ReportPlaces. Nothing in shared Core or the tests reaches into
Kpi. The one product file both tasks edit is RcrcGreenApplication.cs, which registers both
panes, and the KPI round also added five lines to PanelMetrics.cs and touched CLAUDE.md,
revit-commands.md and reports/README.md. No crossing edits the other task's logic.

Core holds no Revit reference: the csproj has zero package or assembly references, grep finds
one Autodesk in Core and it is a comment naming the Addins folder, and the net8.0 test
project loads Core and ran 619 tests here, which it could not if a Revit type were in it.

### 5, QA and QC

619 tests: 144 are Kpi, measured with a filtered run here, and the other 475 are the
Drawing Sheet Core: name parsing, the grid, scope boxes, panel steps, marking, the run plan,
outcome and report, the scan report, and the sheet division, naming, numbering, rows and
batches. What they cover is Core formatting and rules. What they cannot cover is everything
in src/RcrcGreen.Revit, which no test loads and the gate never compiles, finding 8.

Core types never named in any test: DrawingSheetSnapshot, SheetOnAPlot, FamilyTypeCount,
IgnoredName, MissingViewType, ScheduleFieldKind, SheetGridRow, ViewportSpot. The last six are
exercised through their parents. The first two are finding 6, and the three break-and-run
results are recorded there: three breaks, three green runs at 619, restored and rerun green.

The workflow runs Core tests on pull requests into main and fails on a zero count. It cannot
catch a Revit-side compile break, anything about WPF or the Revit API, or a direct push.

Hooks: all three fire and block, checked by doing the thing each refuses. A Write to
/home/user/hook-probe-outside.txt was refused by block-paths.sh naming the path. A commit
staging only this file was refused by require-file-on-commit.sh naming the state file. A
commit whose message held the word the writing rules ban first was refused by
writing-check.sh naming the word. Nothing was committed by any probe.

Never executed in Revit even once: the divided sheet shape end to end, name and number
proposals, the step 4 table, the keystroke refresh, ViewportRecord placement reads, the
scan's viewport section, every KPI file, and the delete-again path on a refused sheet number.
Sections, schedules, plan views and the old one-sheet shape have each run at least once.

### 6, interface

Read against a production person who has not seen it. Findings 7, 9, 11 and 19. Beyond them:
no control was found that does nothing, every step below plots is gated on plots being done,
Run and Assign both confirm with counts, and the step 4 count is the same WillBeMade the
table rows show, one source. Whether eight columns fit, whether the step header's ellipsis
trim hides the summary at real pane width, and whether the four-column sheet table fits a
docked pane are UNKNOWN without Revit.

### 7, reports

Finding 10 is the one internal disagreement found. Units were checked line by line: feet stay
feet in scope box extents and say so, millimetres carry mm, the section depth prints metres
from the same constant that cuts it, and the KPI report prints raw square feet and worked out
square metres side by side. Everything created, refused, skipped or left behind reaches a
report section except the two capture-time drops in finding 1, and the mark-clear in finding
9 which reaches nothing. Nothing was found in a report the user cannot act on.
