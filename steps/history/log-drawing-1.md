## 2026-09-12, sixty second pass, first of three. The grid shows what exists

Branch `claude/rcrc-green-setup-wf9ham`, pull request 96, merged as `d77b90a` with 1320
tests on the runner, 0 failed and 0 skipped, and the squash message back byte for byte.
Three features this round, split into three pull requests in the order the brief set, each
merged before the next begins. This is the grid. Presets and the run preview follow.

**One correction to the brief, made rather than worked around.** It asked for bulk marking
as something specified rounds ago and never built. It is built, wired and tested, and
nothing here writes a second copy of it. A sub plot name marks its row through
`BulkMarking.WholeRow` at `DrawingSheetPanel.cs:959`, a column header marks its column
through `WholeColumn` at `1025`, Mark every missing sits at `804`, Clear all marks beside
it, and `BulkMarkingTests.cs` holds 19 tests over the rule they all share. What this pull
request adds is four lines in `revit-commands.md` recording where they are, so the next
round checks before building them again. Rebuilding them would have been the eleventh time
this repo kept two records of one fact, and it would have been invisible: two sweeps that
agree today and drift apart in a year.

**The real work was the states.** A cell had three, and no number. A view already drawn on
a sheet and a view nobody has placed looked identical, which on the first real model hides
the ordinary case: 2,430 views sit on no sheet against 953 that are placed. So
`SheetCellState` gains `ExistsNoSheet`, and every cell carries the number of the sheet its
view sits on. That number is the point of the whole fix. It is what says at a glance that a
sub plot is running on copy numbers, which is true of every sub plot but DM-11 on
RCRC_NG05_NU_MAIN, and no count of placed views could ever say it.

Three rules hold it together. `SheetGridCell.IsInTheModel` answers the one question both
existing states share, whether the cell can be marked and whether a click opens something,
so no call site compares against two states and gets one of them wrong later.
`PlotViewPresence` carries the sheet number from the read rather than having it looked up
beside the grid, because the cell that shows the number and the cell that decides the state
are one cell. And `PanelSteps.CellInWords` and `LegendInWords` are the only wording for the
four, because the legend and the tooltip were two lists written out beside the controls
that drew them, which is how a square comes to mean one thing in the key and another under
the pointer.

**The read needed two collectors, not one.** A view is placed on a sheet by a `Viewport` if
it is graphical and by a `ScheduleSheetInstance` if it is a schedule, which is why the
writer creates them with two different calls. Reading only viewports would have shown every
schedule on every sheet as sitting on none, and the grid would have been confidently wrong
about six of the things under every plot.

Suite reads 1320 locally, the 1316 on main plus 4, and 1320 on the runner, which is the
same number because nothing landed on main while this was open. Two deliberate breaks
watched red and reversed with the staged diff hash equal before and after: a view on no
sheet reading as done again took 2 red, and the tooltip dropping its sheet number took 1.

Not observed in Revit, all of it. Nothing in this pull request has been run. Specifically:
the fourth mark on a real grid, a sheet number under a square and what it does to the
column widths on a model whose numbers read 010QE Copy 001, `Viewport` and
`ScheduleSheetInstance` answering for a real model's placements, and a click opening a view
that sits on no sheet. The mockup at `design/pr-96/panel.html` is drawn by hand from the
code and says so in its own first lines.

---

## 2026-09-12, sixty first pass. Seven fixes from the first run of the new panel

Branch `claude/rcrc-green-setup-wf9ham`, one pull request, 94, merged as `717e2f4`
with 1316 tests on the runner, 0 failed and 0 skipped, and the squash message back byte
for byte. The panel was installed and
run in Revit for the first time since the number scheme changed. It works: 600DM11A,
400DM11 and 200DM11 are on real sheets, and Crop View came on for the first time on a
created section. Seven faults came back.

**Fix 5 first, because it is the one that needed finding.** A COVER PAGE sheet showed a red
warning under its number box, and the user first read that as a cover page needing no
number. Their own models say otherwise: RCRC_NG05_NU_MAIN numbers two TITLE SHEETs 010QE
Copy 003 and 010QE Copy 004, and RCRC_NG03_EZ_MAIN numbers one 010001A. The cause, with
file and line, read off the pre-fix branch at `a7cd601`:

- `PlannedSheet.SingleCode` returns `string.Empty` when a sheet holds no views, at
  `src/RcrcGreen.Core/DrawingSheet/SheetDivision.cs:66`
- the panel turns that empty code into `SheetNumberRun.NoViewsWords()` at
  `src/RcrcGreen.Revit/DrawingSheetPanel.cs:2250` through `2257`
- `SayRowFault` paints that reason in `_theme.Warning` under the box, at
  `src/RcrcGreen.Revit/DrawingSheetPanel.cs:1581`

So the red line was the tool saying it had no view code to front the number with, which was
true of the sheet and false of the repo: `install/title-blocks.txt` has shipped
`010 TITLE SHEET AR-PRX-Title_Block_A1 COVER PAGE` since the fifty sixth pass, and nothing
anywhere read a code back off a title block. COVER PAGE is the title block TYPE and TITLE
SHEET is the sheet NAME, and the pairing between them carries the code the sheet could not
find. `TitleBlockSettings.CodeFor` reads it now: the one code every pairing on a block
agrees on, so KEYPLAN answers 010 from its three pairings and a block carrying a 400 and a
600 pairing answers nothing rather than picking one. A no-view sheet numbers itself like any
other row, code, plot stem, sheet letter, and the old words are kept for the case that is
left, a block the settings pair with no single code.

The other six. Ticking a plot in step 1 folded the step and opened step 2 while the user was
still choosing sub plots, so that jump is gone and `MoveOnFrom` with it, because the Next
button does its own `OpenAfter` and nothing else called it. The sub plot line read DM-02DM02,
the identifier and the stem with no margin between them, and the words moved into
`PanelSteps.SubPlotSaid` with a label and a test. From and To offer the digits 01 to 99 on
every model rather than the sub plots the open one holds, because the team works across
models and a range filled from the open model cannot be set up for the model it is meant for,
while the LIST under them still holds only what this model really carries inside the range,
so no plot is invented anywhere. Each ticked plot has a search box with All and None over
what it shows, and its sub plot lines sit in their own bounded scroller while the plot lines
never do. The section cut is 18.2374 metres, centred on the scope box, the tool's own setting
said in the report in the depth line's words. And two views on a sheet stack rather than
sitting side by side, because each was wider than half the drawing area.

**The fit is the new rule under fix 7.** `SheetFit` in Core takes the drawing area, the count
per sheet and each view's size on paper, and decides which views belong on which sheet: each
into the next cell it fits, one that fits nothing starting a fresh sheet, and one too big
even for a cell of its own placed alone so it covers nothing. Nothing is dropped. The writer
measures each view through `View.Outline` before placing anything, and carries what does not
fit onto another sheet of the same definition, numbered on by `SheetNumberRun` over the
numbers the document holds at that moment rather than off a stale list.

Two limits inside that, both stated rather than hidden. A schedule cannot be measured before
it is placed, because how big it comes out is not known until Revit has drawn it, which is
the same reason its corner is corrected afterwards, so it counts as fitting and is named as
unmeasured. And a carried sheet whose views do not share one view code cannot be numbered
without a guess, so it is not created at all and its views are named in the report instead.
A third limit belongs to fix 3: a sub plot numbered above 99 falls outside every range the
picker offers, which is the cost of the two digit ends the round asked for.

Suite reads 1308 locally, the 1281 this branch started from plus 27. The runner reads 1316,
which is not a disagreement: KPI merged pull requests 92 and 93 while this one was open, main
now reads 1289 on its own, and 1289 plus the same 27 is 1316. Three deliberate breaks watched
red and reversed with the staged diff hash equal before and after: two views laid side by
side again took 2 red, a view that does not fit no longer starting a fresh sheet took 2, and
a title block with two codes answering the first of them took 1.

Not observed in Revit, all of it. Nothing in this round has been run. Specifically: the new
step 1 with its search boxes and bounded lists, the 01 to 99 pickers, the auto-open that no
longer happens, a COVER PAGE sheet taking 010 and numbering itself, the 18.2374 metre cut on
a real scope box, two views stacked on a real sheet, `View.Outline` answering for a plan and
for a section, a carried sheet being created and numbered inside the transaction, and every
report line this round adds. The mockup at `design/pr-94/panel.html` is drawn by hand from
the code and says so in its own first lines.

---

## 2026-09-12, sixtieth pass. The sub plot number round

Branch `claude/rcrc-green-setup-wf9ham`. Two pull requests, the split the brief named: fix
1 first, fixes 2 and 3 second, and a mockup of the new step 1.

Fix 1 is pull request 89, merged as `2688f631`, 1271 tests on the runner, 0 failed and 0
skipped, matching local: the 1288 on main minus the 19 marker tests plus 2 number tests.
The marker was wrong twice over, the numbering was meant to be automatic and a per plot
control made step 1 long, so a sheet number is built from the plot identifier itself: the
view code, then the identifier with its dash dropped, then the sheet letter, DM-42 reading
010DM42A TITLE SHEET, 010DM42B LIST OF DRAWINGS, 200DM42 GENERAL ARRANGEMENT LAYOUT,
400DM42 LANDSCAPE CROSS SECTION, 600DM42A HARDSCAPE SCHEDULES and 600DM42B SOFTSCAPE
SCHEDULES. Nothing is set, nothing is reserved and nothing runs out, because the
identifier is in the number and no two plots share one. StemOf on SheetNumberRun is the
one rule turning DM-42 into DM42, empty for anything that is not a plot identifier, and
the run refuses to start on one, so the old markers cannot creep back through a caller.
Old numbers, 010QE and 010001A, do not start with the new front and hold no letter slot,
so old sheets are never renumbered, NG03 and NG05 keep their numbers beside the new ones,
which the user called their decision rather than a fault, and a number the model already
holds is still refused by name, under the box and at Run. MarkerChoices, PlotMarkers,
PlotMarkerFile, MarkerLedger, PlotMarkerStore and PlotMarkerTests are deleted with the
step 1 dropdowns, the ledger read, the not-read lines and the captions, NoMarkerWords with
them, and the reasons left on a row with no number are mixed codes and no views.

Fix 2 is pull request 90, merged as `ff5d69d2` with 1281 tests on the runner. Step
1 is a tick list of plots: every two letter prefix the model holds is a plot, several
tickable at once, ticking one takes its whole span with every sub plot ticked, a ticked
plot carries From and To over its own sub plots, one line per in-range sub plot shows its
tick, its identifier and the number stem its sheets will take, a count at the foot says
how many plots and how many of their sub plots are ticked, and one run covers every ticked
sub plot on every ticked plot, in plot order however they were ticked. PlotTickList in
Core/DrawingSheet holds all of it and composes Shared's PlotRange and PlotSelection rather
than editing Shared, which stays untouched this round. The panel's prefix, From and To
fields are gone with FillPrefixes, PutTheRangeBack, PrefixChosen, RangeChosen and
PlotsInRange, and a refresh puts back the ticked plots, their ranges and the unticks the
model still holds. The mockup is design/pr-90/panel.html, light and dark, marked in the
file itself as a mockup and not a screenshot.

Fix 3 could not run and is said plainly rather than reasoned around. The check was the
two sheet names against the two scan reports, which are user held: reports/ holds only
README.md in this container, nothing in reports/ is ever committed, and no scan report is
readable here. Of the nine shipped sheet names, seven are confirmed by the two models
through SheetOrder, whose list was read off both: LIST OF DRAWINGS, PROJECT LOCATION KEY
PLAN, OVERALL KEYPLAN, GENERAL ARRANGEMENT LAYOUT, LANDSCAPE CROSS SECTION, HARDSCAPE
SCHEDULES and SOFTSCAPE SCHEDULES. The two the order list does not hold, OVERALL PLAN and
FURNITURE SCHEDULES, are left in sheet-names.txt exactly as they are and remain UNVERIFIED
against any model or scan. Open question for the team, carried from the fifty eighth pass.

Four deliberate breaks watched red across the two pull requests, 8 of 11 SheetNumberRun
tests on a front built without the stem, 1 of 1 on the provenance words, 7 of 10
PlotTickList tests on a tick that spans nothing, 1 of 1 on the step summary words, each
applied and reversed by python with the git diff md5 equal before and after.

Not observed in Revit, all of it: the built numbers on a real model, the two schemes side
by side on a plot that holds old numbers, step 1 as drawn, the fresh From and To boxes
filling without firing their handlers, the auto open on the first tick, the refresh
putting the ticks back, and the run report's new closing note. The mockup is drawn by
hand from the code and says so in its own first lines.

---

## 2026-09-12, fifty ninth pass. The report for the three fix round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 87 is
merged. The entry below carried the first pull request's merge and was written before the
second existed, so it could not carry that one. It carries it now.

Pull request 87 merged as `c54c682` and the gate executed 1288 tests against it, 0 failed
and 0 skipped, the same count the local run gave after the last file was written and ten
above the 1278 pull request 86 left on main. The merged main reads 1288 locally as well.
Both of the round's squash messages came back off main byte for byte. Nothing else changed.
No code is touched, and nothing in this round has been through Revit.

---

## 2026-09-12, fifty eighth pass. The name table, the stale read's cause, and the view type with no example

Branch `claude/rcrc-green-setup-wf9ham`, a three fix brief in two pull requests. The brief
arrived cut off mid sentence in fix 3, at the words a marked type with no sibling, and its
closing rules were missing with it. The assumed completion: such a type with no complete
answers is refused naming the three, which is the only reading consistent with nothing is
invented and nothing is defaulted, and the standing finish applies. Pull request 86, fixes 1
and 2, merged into main as `e42dd56`. **The runner executed 1278 tests against it, 0 failed
and 0 skipped**, this branch's 1251 plus 27 KPI tests pull request 85 landed on main while
it was open. The second pull request's merge sha and runner count go in the next entry,
because this one is written before it exists.

### Fix 2 first, the cause, with file and line, all read at main `eaed0e9` before any edit

Candidate a is DISPROVED. DrawingSheetPanel.cs line 177, Shown, runs on every
IsVisibleChanged and calls AskedForARefresh, so the pane does read when it becomes visible,
and has since the round that fixed the second document.

Candidate b is REAL but is not the whole incident. No DocumentOpened, DocumentChanged or
DocumentClosed handler existed anywhere under src/RcrcGreen.Revit, measured by grep, so a
model opened or changed under an open pane was never re-read.

The nearest cause of the measured incident is the run itself. DrawingSheetRequestHandler.cs
line 384 read the model fresh for the plan, and that fresh read went no further: RunPlan.cs
line 313, AddSheets, checked no sheet number against anything, so the only number check
lived on the panel against its last snapshot, and the collisions arrived as Revit refusals
inside the transaction. And nothing handed the fresh snapshot back to the panel, whose
closing words at line 449 were literally Press Refresh to see them, a copy of a fact the
pane can ask for. So the panel's own run was a document change it never learned about, every
proposal after a run was built on the pre run model, and three runs in a row asked for the
numbers the first had created.

What it does now when the model changes while the pane is open: a document opening or
closing triggers a read of its own, through DocumentOpened and DocumentClosed subscribed on
the handler's first request. A run that wrote hands a fresh read back through the same route
Refresh uses, and its words say the panel has read the model again. And whatever still slips
through is refused on the plan, because RunPlan checks every row's number against the
numbers read as the run is worked out, so the panel can be a day old and the model still
cannot be asked for a taken number.

### Fix 1, the sheet name table

Four of DM-11's eight sheet names disprove the upper casing rule, and the same wrong words
made those sheets sort as unlisted, one cause with two symptoms. SheetNameSettings holds
view type against sheet name, the user's sheet-names.txt first and the shipped one second,
resolved once on PlannedSheet.ProposedName so the rows, the letter order and the run read
one record and match SheetOrder's words. A type neither file holds falls back to the old
derivation, said as derived under the box, and typing a name over a single view row saves
the pairing on focus loss. THE TWO SCAN REPORTS ARE NOT IN THIS CONTAINER, nothing under
reports/ is ever committed, so the verification the brief asked for did not run here.
Checked instead against the committed records: seven of the nine names are the order list's
own confirmed words, LIST OF DRAWINGS, GENERAL ARRANGEMENT LAYOUT and LANDSCAPE CROSS
SECTION also stand in SheetNaming's doc as read off real sheets, and OVERALL PLAN and
FURNITURE SCHEDULES rest on the brief alone. The shipped file test pins those two by name so
the gap stays visible. Holding the nine against the two scans is still open and is the
user's to close.

### Fix 3, a view type with no example can be created

The Add row promised a column the run always refused, because creation copies its setup from
a view that does not exist. Step 5 lists each marked type no view in the model carries, with
three dropdowns read off the model: view family type, view template, level. Only the kinds a
view can be created under are offered, the four plan kinds and Section, which is what
ViewPlan.Create and ViewSection.CreateSection accept. The family type's kind decides plan or
section, a section takes no level, and the routing is one Core method, SectionTypesAmong,
asked by the panel's preview and the run's handler alike. Answers save to new-view-
setups.txt beside the other settings, nothing shipped because the names are a model's own,
and the writer resolves each saved name against the model at Run, refusing with the name
when this model does not hold it. A type with no sibling and no complete answers is refused
naming what is missing, which is the assumed completion of the cut sentence. On a view built
from answers, annotation crop is still the tool's own on a plan and Crop View on a section,
and the crop settings a sibling used to supply are left as Revit created them, said in the
report line for that view.

### The breaks

Four across the round, two per pull request, each reversed with the diff hash identical. The
shipped sheet names beating the user's own, 1 of 1251 red. The run guard reading nothing
instead of the fresh numbers, 1 of 1251 red. A section asked for a level it cannot take, 2
of 1288 red. Every answered type routing as a section, 1 of 1288 red.

### What ran here

dotnet test on every change, 1251 after fixes 1 and 2 and 1288 after fix 3, 0 failed and 0
skipped each time. dotnet build of the whole solution in Release, 0 errors, the only
warnings the KPI task's pre existing analyzer notes. The grep for Autodesk under Core,
clean. The writing scan over every changed file, clean. The runner executed 1278 against
pull request 86.

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the DocumentOpened and
DocumentClosed events firing and the read they trigger, the post run read back on a real
model, the run refusing a taken number before the transaction rather than inside it, the
saved sheet name reaching a real sheet, the derived line on screen, the three dropdowns and
their save file, a view created from answers, plan or section, and whether Revit accepts the
answered family type, template and level combinations a user will actually pick. The scan
verification of the nine shipped names is open as said above.

---

---

## 2026-09-12, fifty seventh pass. The report for the sheet numbering round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 83 is
merged. The entry below carried the first pull request's merge and was written before the
second existed, so it could not carry that one. It carries it now.

Pull request 83 merged as `c7d91c4` and the gate executed 1239 tests against it, 0 failed
and 0 skipped, the same count the local run gave after the last file was written and nine
above the 1230 pull request 82 put on main. The merged main reads 1239 locally as well.
Both of the round's squash messages came back off main byte for byte. Nothing else changed.
No code is touched, and nothing in this round has been through Revit.

---

## 2026-09-12, fifty sixth pass. The sheet number is built from the user's marker, in the team's order

Branch `claude/rcrc-green-setup-wf9ham`, the four fix sheet numbering round, split into two
pull requests as the brief allows: fixes 1 and 2 first, 3 and 4 second. Pull request 82,
fixes 1 and 2, merged into main as `e4b5d9e`. **The runner executed 1230 tests against it, 0
failed and 0 skipped**, the same count the local run gave, up six from the 1224 main carried
at `07f16ba`, 22 tests deleted with the old rules and 28 added. The squash message came back
off main byte for byte. The second pull request's merge sha and runner count go in the next
entry, because this one is written before it exists.

### Fix 1, the marker is the user's and is remembered

A sheet number is the view code, a plot marker, then a sheet letter within one code, absent
when the code holds a single sheet. FP-39 on NG03 reads 010001A to 010001D and 200001, DM-11
on NG05 reads 010QE to 010QH and 200Q, and the two models differ only in the marker. The old
rule read the plot letter off the plot's own numbers, and every NG05 plot but DM-11 carries
only copy numbers, so it answered nothing on 159 of 160 plots. The marker is set in step 1
now, two dropdowns beside each ticked plot, letters A to ZZZ or numbers 001 to 999, and
PlotMarkerStore remembers it per model in plot-markers.txt beside the user's title block
settings, keyed on the document title, the one name a detached never saved model still
carries. MarkerLedger bars the markers other plots' numbers or other panel rows already use.
Reading a marker back out of a number is ambiguous, 010QE cannot say whether its marker is Q
or QE, so both are counted in use, except that four or more digits with at most one trailing
letter is the 001 kind, where only the three digits before the letters count: 010001A gives
001 and never A, because A would bar most of the alphabet on a model numbered the NG03 way.
The line beside each plot says set now, remembered, or that there is no marker and no sheet
without one. PlotLetter, LettersIn, Free and Propose are deleted with their tests, the
number box is a plain box, and the report's closing note stops describing the deleted rule.

### Fix 2, the number is built

SheetNumberRun builds one code's numbers for one plot: bare when the code holds one sheet in
total, lettered from A otherwise. Occupied slots come off the plot's own numbers plus
everything typed on the panel, a bare number holds the first letter's place, and the next
letter continues after the highest, so 600QC and 600QD give 600QE and 200Q gives 200QB, a
second run continuing rather than colliding. The panel works the numbers out across all
described sheets at once, one sequence per plot and code, because worked out per definition
a plot would get two sheets both lettered A. Multi view sheets sharing one code are numbered
too, a sheet mixing codes says why its box is empty, and a plot with no marker gets no sheet
with the refusal naming the plot and step 1.

### Fix 3, the team's order

SheetOrder holds the nine names, and the order is the code first, then the list within a
code, then the ticked order for names the list does not hold. It is also the letter order,
so 010001A is TITLE SHEET because the list says so. The division sorts its planned sheets,
the panel hands letters out in list order within each code, and the run creates in the same
order. Two edges are left exact rather than smoothed: OVERALL KEY PLAN with the space is not
the list's OVERALL KEYPLAN and reads as unlisted, and a single schedule view proposes its
singular name, HARDSCAPE SCHEDULE, which is not the list's plural sheet name. Whether either
pair is one name is the team's question.

### Fix 4, the title sheet is reachable, and what was found first

Checked before fixing, as the brief asked. The writer already places an empty sheet without
complaint, PlaceViews returns before touching anything when a row holds no views. What made
a no view sheet unreachable was the division: SheetDivision.Of planned zero sheets for zero
views, so no row ever existed, and the old test pinned that as the rule with the words no
views need nothing. That was the earlier team answer to the copied empty sheet, and the
brief supersedes it: a definition with a title block and no views now plans one empty sheet
per ticked plot, its name and number typed, refused until both arrive, sorted ahead of the
coded sheets because the title sheet opens both measured sets. The shipped defaults gain the
twelfth pairing, 010 TITLE SHEET on AR-PRX-Title_Block_A1 COVER PAGE. Open question for the
team: nothing reads a title block pairing for a sheet that has no views, so the pairing sits
unused until somebody says whether the typed name should look one up. The COVER PAGE family
and type are read off the brief's words and not observed in a model here.

### The breaks

Four across the round, two per pull request, each reversed with the diff hash identical
before and after. A bare number no longer occupying a slot, 1 of 1230 red. The digit marker
preference removed so 010001A also gave A, 1 of 1230 red. Unlisted names claiming the title
sheet's place, 1 of 1239 red. No views planning no sheets again, 2 of 1239 red.

### What ran here

dotnet test on every change, 1230 after fixes 1 and 2 and 1239 after fixes 3 and 4, 0 failed
and 0 skipped each time. dotnet build of the whole solution in Release, 0 warnings and 0
errors. The grep for Autodesk under Core, clean. The writing scan over every changed file,
clean. The runner executed 1230 against pull request 82.

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the two marker dropdowns and
the 18,278 entry letters list behind a virtualizing panel, the marker file written and read
back on a real machine, the built numbers against a real model's own sheets, the warnings on
a real collision, the order of created sheets in a real run, an empty title sheet created on
the COVER PAGE block, and whether that block exists under that name in any model. The letter
continuation past 600QD and the typed 010001A pushing built numbers to B are tested but not
watched in a model.

---

---

## 2026-09-12, fifty fifth pass. The report for the section crop round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 79 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 79 merged as `b7d41dd` and the gate executed 1224 tests against it, 0 failed
and 0 skipped, the same count the local run gave after the last file was written and three
above the 1221 main carried at `9fd1a65`. The merged main reads 1224 locally as well. The
squash message passed on the merge call came back off main byte for byte. Nothing else
changed. No code is touched, and nothing in this round has been through Revit.

---

## 2026-09-12, fifty fourth pass. A section the tool makes keeps the bound the tool computed

Branch `claude/rcrc-green-setup-wf9ham`, one commit, off main at `9fd1a65` with 1221 tests.
The merge sha and the runner count go in the next entry, because this one is written before
the pull request exists. Two fixes, the ones the fifty third pass found and was told not to
make.

### Fix 1, Crop View is the tool's own on a section

The cause was found last round and the code path is unchanged since: `MakeSection` computes
the section box from the plot's scope box and hands it to `CreateSection`, which makes it
the new section's crop region, and `FinishSection` then copied Crop View off the sibling,
which has it off. The bound was computed and the thing enforcing it switched off four calls
later, the tenth two-records-of-one-fact here. What that cost was measured on the run of
2026-09-11: DM-11-(400)'s viewport read 13250.5 by 198.3 mm on an 841 by 594 sheet, and the
model's own unbounded sections drew their markers inside DM-11's plans.

`SectionCropChoice.ForASection` says on, always, whatever the sibling has, the same shape as
`AnnotationCropChoice` and `SectionDepth`: the tool applies its own setting and the report
says whose it is, in the same words the annotation crop line uses. `ApplySiblingCrop` takes
the crop flag from its caller now, a plan view still passes the sibling's, and a section
passes the choice's. The region visibility and the annotation crop are still copied on a
section, so which one of the three is the tool's own is what swaps between the kinds, and
each kind's choice type is the one record of it.

**What this does not fix.** The model's own sections, DM-20-(400) and NS-32-(400) among
them, still have their crops off and will still draw across other plots' plans, and the
referencing sheet fault on those two still stands with them. This round bounds only what the
tool makes. Whether the tool should offer to bound the model's own sections too is the
user's call and is not made here.

### Fix 2, the report says what a section's crop is

A section's setup line used to print the sibling's crop flag as copied, which after fix 1
would have said the new section has its crop off in the same report that turned it on. The
line names only what a section really copies now, the region visibility and the annotation
crop through `ViewCrop.CopiedForASectionInWords`, picked by the sibling's kind, and the
choice's own sentence carries Crop View with whose setting it is and what the sibling has.
So every section line carries all three settings the created section holds, split the same
way the plan view lines split theirs: the copied ones in the facts, the tool's own said
separately. The closing note gained the CROP VIEW IS NOT COPIED ON A SECTION lines beside
the annotation crop ones, and the closing note test pins them.

### The records that said a section copies all three

revit-commands.md's sibling section, core-rules.md's copied-settings section and CLAUDE.md's
tool-settings sentence all said the old rule, and CLAUDE.md counted two settings as the
tool's own where there are three now. All three follow the code.

### The breaks

Two, each a reversed edit with the diff hash the same before the first and after the last:

- `SectionCropChoice.On` reading the sibling's flag again: Failed 1 of 1224, the test that
  says on whatever the sibling has
- the section's setup line printing the plan clause: Failed 1 of 1224, the test that says a
  section line names only what a section copies

### What ran here

The whole solution built in Release with 0 errors, and the suite read 1224 after the last
file was written, 0 failed and 0 skipped, up three from the 1221 main carries. The banned
word scan, the em dash scan and the semicolon scan over the added lines found nothing, and
Core names no Revit type.

### Not observed

Nothing in this round has been through Revit. Unexecuted there: `CropBoxActive = true` on a
section Revit has just created from a section box, and whether the crop region it enforces
is exactly the box that was handed in. Whether a view template on a section controls Crop
View and turns the set into the needs-attention refusal. The 13 metre viewport shrinking to
the plot's short side on the next run, which is the one number that would show the fix
worked. And whether the markers of a section the tool bounded stop appearing in other plots'
plans, which is the fifty third pass's open question about DM-11's crop region and is only
answerable with the model open.

---

## 2026-09-11, fifty third pass. The crop is the sections' own, and two placement faults off the first real run

Off the run of 2026-09-11 at 17:52: 22 created, 0 refused, the first time the tool has been
in Revit since pull request 32. Eight items came back. This round is two pull requests,
because fix 7 alone is a new Core file family with tests, a panel change and an installer
change, and the brief allowed the split. Fixes 1 to 4 are the first, 5 to 8 the second.

Pull request 74, fixes 1 to 4, merged into main as `f72834e`. **The runner executed 1193
tests against it, 0 failed and 0 skipped**, the same count the local run gave after the last
file was written, and eleven above the 1182 main carried. Pull request 76, fixes 5 to 8,
merged into main as `148e44b`. **The runner executed 1221 tests against it, 0 failed and 0
skipped.** Locally the head read 1216, and the five between are KPI tests pull request 75
landed on main while this one was open, which the gate builds in: main measured 1221 after
the merge. Both squash messages came back off main byte for byte.

### Fix 3 first, because it is the one that had to be found rather than fixed

**Annotation crop is not the answer and has not been for nine rounds.** DM-11-(010) Overall
Plan came out with Annotation Crop ON, which is the tool's own setting and which the
Properties panel confirms, and DM-20-(400) and NS-32-(400) are still selectable inside it.
That disproves the thing nine rounds assumed. It is worth saying plainly: the fix that was
applied over and over was to a view that was already set the way the fix wanted it.

**The two sections that bleed are the model's own and not this tool's.** DM-20-(400) is the
sibling DM-11-(400) was set up from, so it predates the run, and NS-32-(400) is one of the
four real sections this repo measured for the far clip rounds ago. Nothing the run created
put them there. What the run created is a view that shows them.

**What they have in common is measured: Crop View off, Scope Box none.** The brief records
it for DM-20-(400), and DM-11-(400), which the tool made, came out the same way.

**A section with its crop off is not bounded sideways, and that is where both symptoms come
from.** The extent of what a section draws is its crop region. With the crop off there is
nothing bounding it across the cut, so what it draws runs as far as the model does, its
marker in plan runs with it, and any plan view that marker crosses shows it. Annotation crop
cannot help: it hides annotation that falls OUTSIDE the annotation crop region, and a marker
running through the middle of the view is inside it. No setting on DM-11's own view can hide
an annotation that is genuinely inside DM-11.

The same thing on the section's own side is symptom b. 400QA read 13250.5 by 198.3 mm on an
841 by 594 sheet. A viewport fifteen times wider than the sheet it sits on is a view showing
something far bigger than one plot, which is exactly what an unbounded section draws.

**The tool is now making more of them, and the code says how.** `MakeSection` works the
section box out from the plot's scope box through `SectionPlacement.Across`, hands it to
`ViewSection.CreateSection`, which is what sets the new section's crop region, and then
`FinishSection` calls `ApplySiblingCrop`, which sets `CropBoxActive` from the sibling. The
sibling has it off. So the tool computes the right bounds and then switches them off four
calls later. That is the tenth two-records-of-one-fact in this repo: where a section reaches
is decided twice, once by the plot's scope box and once by a flag copied off another view,
and the copy wins.

**What would bound it is the crop box the tool already computes.** Nothing else is needed
and nothing new has to be measured. Whether the tool should set Crop View on for a section
it makes: yes, on the same reasoning that made the annotation crop and the section depth the
tool's own settings, because `SectionPlacement` exists to say where a section reaches and
copying a flag off a sibling throws that answer away. It is not done in this round. The
brief said investigate and do not patch, and it reverses a rule this repo wrote down on
purpose, that a section copies all three crop settings, so it is the next round's and it is
named here rather than slipped in.

**What is still UNKNOWN, and exactly what would settle it.** Whether those foreign markers
sit inside DM-11's crop region or in the margin between its model crop and its annotation
crop. Revit holds the annotation crop as a separate rectangle offset outward from the model
crop, and those offsets are readable and settable through `ViewCropRegionShapeManager`. If
the markers are inside the region, the cause is the sections' own extents and nothing about
DM-11's view will fix it. If they sit in that margin, the offsets are a second contributor.
**The check is to open DM-11-(010) Overall Plan in Revit and look at where DM-20-(400)'s
marker sits against the crop rectangle.** One look answers it. Nothing here can, and nothing
here guessed.

### Fix 4, the three sections referencing 010QD

It follows from fix 3 and one of the three is not a fault. Revit sets Referencing Sheet from
the sheet holding a view in which the section's marker is displayed. DM-11-(010) Overall
Plan went onto 010QD and all three markers are visible in it, so all three took 010QD and
detail 1.

**DM-11-(400)'s reference is correct** and should stay: its marker belongs in DM-11's plan.
DM-20-(400)'s and NS-32-(400)'s are wrong, and they are wrong for exactly the reason fix 3
names, because those two are only visible there at all through their own unbounded extents.

Whether it clears by itself once they are bounded is UNKNOWN. Revit works the reference out
from where the marker shows, so a marker that no longer shows in DM-11's plan should take a
different sheet or none. Whether it recomputes on the next regeneration or needs the file
reopened is not something this session can say. Nothing was changed about it, as the brief
asked.

### Fix 1, a schedule is placed by its corner

Every schedule on a sheet was wrong and every plan view was right, and the run report's own
numbers say why. 010QA: a schedule 207.4 by 187.4 mm, asked for the centre 420.5 by 297.0
that every plan view landed on, came back centred on 522.1 by 203.3. That is 93.7 low, and
half its own height is 93.7 exactly. `ScheduleSheetInstance.Create` takes the top left
corner and `Viewport.Create` takes the centre, and both were handed the centre.

**The conversion is on the Revit side, after the placement, and it is measured rather than
worked out.** How big a schedule comes out on a sheet is not known until Revit has drawn it,
so there is no size to subtract from beforehand. `ModelWriter` places, regenerates, reads
each schedule's bounding box, and moves it by what `CornerPlacement.MoveToPutTheCentreAt`
hands back. Measuring also means nothing in the code has to assume which corner Revit used.
Core keeps working in centres throughout and knows nothing about corners except in its own
words.

**Nothing moves a viewport.** Those landed correctly, and a viewport's bounding box takes in
the view title drawn under it, so correcting one against its box would move a placement that
is already right. A schedule Revit gives no bounding box for is left where it is and named
under needs attention, because a skip with nothing written down is the fault this repo has
paid for twice.

### Fix 2, the views divide the drawing area

`SheetLayout.For` divided the whole 841 by 594, so a wide view centred in it crossed the
title strip down the right. It takes a `DrawingArea` now.

**The drawing area cannot be read off a title block.** Revit gives a placed block Sheet
Width and Sheet Height and nothing else, and where the strip begins is drawn inside the
family, so reading it would mean opening the family document, which is not something to do
inside a run. So the strip is the tool's own setting, a fifth of the width, which is the
team's own measurement off this run, stated in the brief. It is one constant in one place
with its source in its own comment, and `DrawingArea.InWords` says whose setting it is on
every sheet line of the report, the same way `SectionDepth` and `AnnotationCropChoice` do. A
number with no source would have been the invention the brief refused, and this one has a
source and says so.

### Fix 5, a view reads Custom against a scaled template

**The report and the model agree about the number, and the report was short of a word.**
DM-11-(200) General Arrangement Layout reads View Scale Custom with a Scale Value of 250,
under the template (200) General Arrangement Layout SC - Scale 250, and the report recorded
1:250. Checked against the API documentation before anything moved, the way the sheet scale
question was: the View.Scale page says the property is the ratio of true model size to paper
size and that setting a scale which does not correspond to a predefined one applies a custom
scale. So 250 is the number in both places, and Custom is the label Revit puts on a value
its own list of scales does not carry. The template is named for 250 and 250 is what it
drives.

Nothing in the tool sets a scale, checked by grep: the only two reads of View.Scale in the
Revit project are the scan's and the run's, and both are reads.

So there was nothing to correct in the model and one thing to correct in the report, which
now reads 1:250, shown as Custom when the two differ and 1:250 when they do not. A report
and a model that differ by a word are the same class of fault as a report that says created
and not created, and the word was the only gap.

### Fix 6, the viewport type nobody chose

Every viewport the run placed came out PRX_Title With Line. Where it comes from is
Viewport.Create, which takes the document's own default viewport type: no brief, no rule
here and nothing the team said ever picked it, and the report did not even record which type
a placement got.

It comes off the sibling now, the same one view the family type, the level, the template and
the crop come off. SiblingReader reads the viewport type of the viewport placing each
candidate in one pass over the document, SiblingView carries it, and the writer changes the
new viewport's type after it is created. The kind is read off the view being placed rather
than off its code, because a sheet places views the model already held as well as ones this
run made.

**A sibling on no sheet lends no viewport type, and that is said.** The run records which
type the placement really got, names the sibling and says plainly that the type is the
model's default rather than a choice. A type the settings name that the model does not hold
is said the same way. Nothing falls back in silence, which is what the brief asked for and
what the tool did until now. The scan reads the same thing off every existing sheet, so what
the team places their own views with is now in the one section built to be held against the
tool's.

### Fix 7, the eleven title blocks the tool now remembers

Two files, read in this order, first match wins: the user's own at %APPDATA%\RCRC
Green\title-blocks.txt, then the defaults install.ps1 copies beside the add-in from
install/title-blocks.txt. Nothing goes in the model, because a pairing is how this team
works rather than a fact about one project.

Reading, merging and writing are Core with tests, and only the paths are Revit side. Three
rules hold it up, each with a test. **Only what the user set is written back**, never the
shipped ones as well, or this version's defaults would freeze into their file and no later
install could move them. **Views that disagree about the title block are both named and
neither wins**, the rule this repo follows everywhere two sources answer one question. **A
title block the settings name and this model does not hold is not an error**, because the
settings are shared across projects, and the line says which and asks for one the model has.

**The format is tab separated and only the code is trimmed.** A title block in this model is
named LOD /  HARDSCAPE SCHEDULES with two spaces in the middle, so a format that split on
whitespace or tidied its fields would quietly produce a name the model does not hold. The
code and the view name are separate fields so the (010) Overall Plan format lives on
ViewType and nowhere else. A line that is not four fields is kept with its number and shown
at the top of step 4, because a settings file one line short reads exactly like one that
never had the line.

One test reads install/title-blocks.txt itself, walking up from the test assembly, and holds
it to eleven pairings on one family with the double space intact. A broken shipped file
fails the gate rather than somebody's install. Writing the expected order out by hand caught
my own mistake there: a space sorts before a letter, so LOD /  HARDSCAPE SCHEDULES comes
before LOD / SCHEDULES, and the test said so.

On the panel: ticking a view type fills a sheet's title block from the settings when the
sheet has none, changing the picker remembers it against every view ticked on that sheet,
and a line under the picker says whether it came from the user's file, the shipped defaults
or neither. A sheet that already carries a title block is left alone, because that was a
choice and this is an offer. A save that fails says so on the status line, since a setting
that looks saved and is not would have somebody pick the same title block again next week.

### Fix 8, one place for a report

ReportFile writes into the repo's reports folder and nowhere else. Two copies meant two
files per run, two places to look and two to tidy up, and the Desktop copy was the one
nothing reading the code could ever reach.

**No pointer file means no report at all.** reports-folder.txt is the only thing that says
where the folder is, so with it missing there is nowhere to write, and the line now opens NO
REPORT WAS WRITTEN and says to run install.ps1 again. Falling back to the Desktop would put
the file exactly where nobody agreed to look for it. Every status line, the installer's own
closing line, its layout list and four comments follow.

**This reaches the KPI pane and the KPI session has to be told.** ReportFile.cs is one of
the five shared Revit root files and ReportPlaces is Drawing Sheet's own, read across the
fence by the KPI handler at three call sites, so KPI's reports stop going to the Desktop too
and its status lines change wording with them. KpiRequestHandler still carries a line of its
own reading The Desktop folder refused the report, and KpiFile's comment still says its
files sort together on the Desktop. Both are that task's files and neither was touched.
ScanFileName in Shared says the same thing and is Shared, so it was not touched either.

### The breaks

Four across the round, each a reversed edit with the diff hash the same before the first and
after the last:

- `InsideTheTitleBlock` not taking the strip off: Failed 3 of 1193
- `MoveToPutTheCentreAt` running the wrong way: Failed 2 of 1193
- the shipped default winning over the user's own: Failed 1 of 1216
- the settings file trimming the names it reads: Failed 1 of 1216

### Not observed

Nothing in this round has been through Revit at all. From fixes 1 to 4, unexecuted there:
the schedule correction itself, and whether ElementTransformUtils.MoveElement moves a
ScheduleSheetInstance on a sheet as it moves anything else. Whether a schedule ever comes
back with no bounding box, which is the path that names it instead of moving it. The second
regeneration's cost on a sheet holding four schedules. And how a view centred in the drawing
area sits against the strip on a real A1, which is the number the fifth was taken from and
the one worth checking first. The fifth is one person's reading of one title block and the
report now says so on every sheet, so a team that measures it properly changes one constant.

From fixes 5 to 8, unexecuted there: the viewport type being changed after a placement, and
whether `ChangeTypeId` takes on a viewport Revit has only just made. Whether
`VIEW_SCALE_PULLDOWN_METRIC` really reads Custom on that view, which the build proves exists
as a parameter and nothing here proves the value of. The sibling on no sheet path. Both
settings files on a real machine, the %APPDATA% folder being made, and what the panel does
when the shipped file is missing because an older install put none there. A pairing saved
and read back in a second session. And every report landing in one place, which is the one
thing in this round a single run would show immediately.

---

## 2026-09-11, forty eighth pass. The report for the run sheet round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 70 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 70 merged as `fc5cf7a` and the gate executed 1103 tests against it, 0 failed
and 0 skipped, which is the same count the local run gave after the last file was written
and the count main carried before the round, because the round adds no test. The squash
message passed on the merge call came back off main byte for byte. Nothing else changed. No
code is touched, and nothing in this round has been through Revit.

---

## 2026-09-11, forty seventh pass. The run sheet: what gets run before any more code lands

Branch `claude/rcrc-green-setup-wf9ham`, one commit, off main at `2e0c0ab`, which carries
pull request 69 as `9e966fa` and 1103 tests, 588 of them KPI and 515 Drawing Sheet. The
merge sha and the runner count go in the next entry, because this one is written before the
pull request exists. One file, `steps/run-drawing.md`. Nothing in `src/` or `tests/` changes,
and **the suite read 1103 locally after the sheet was written, the count main carries,
because the round adds no test.**

### Why

Drawing Sheet has not been in Revit since its third full run, the one that created 13 with 0
refused, reported in pull request 32. Every round since says nothing in it has been through
Revit: the twenty fifth pass, the fortieth, the forty second, the forty fourth, the forty
fifth in pull requests 61 and 62, and the forty sixth Shared move. That is 28 audit fixes and
the plot list union sitting unobserved. No more code lands on Drawing Sheet until it has been
run, and this round writes what gets run.

### What the sheet is

Forty two numbered steps in the shape of `steps/kpi-create.md`: VS Code run order, one action
per step, the exact command per step, then what to check on screen and what passing reads
like. Eight steps build and install, the same eight the KPI sheet opens with. Three open the
model and the pane and say what the pane has, read off the code rather than off what the
ribbon used to hold: Scan Model and the scope box assignment run from inside the pane through
its external event, and the two `Execute` methods went in pull request 62. Twenty steps, 12
to 31, cover the unobserved changes, grouped by pane step, PLOTS, VIEW TYPES, MARK, SHEETS,
RUN, then the KPI pane for the Shared move. Six steps are one full run over two plots making
two plan views, two sections, two schedules and four sheets at two views per sheet, with the
report's headline as one number, the sizes, the placements and the sibling each view was set
up from written out as they must read. Four steps are the questions only Revit or the team
can answer, each a yes or a no: the step headers at a docked width, eight grid columns and the
four column table, whether Revit's uniqueness rule on sheet numbers is case sensitive, and
finding 15, which is asked of the team. One step says what to send back.

### Where the list came from and how it was cut

Three sources, merged: the never executed list in section 5 of `steps/audit-drawing.md`,
every log entry from pull request 32 onward saying nothing in its round has been through
Revit, and every FIXED note in the audit ending Not observed in Revit. Merged, they gave 46
distinct items by my count, many named in two or three of the sources. The brief capped the
steps at thirty. Twenty were enough, because one status line after a refresh carries all
three new clauses, one full run shows the division, the placements, the setup lines and the
Set checks together, and one typed sheet row shows the proposal, the warning and the
keystroke refresh. Ranked by what breaks the tool if it is wrong: a mark on a hidden column
reaching the run, a stale mark making an orphan, the delete-again paths, the Set checks and
the sheet proposals come first, and the scroll memory, the Remove question and the moved
messages come last.

Nine items cannot be forced from the pane on this model and are listed at the end under not
covered by this pass, each with what would force it: a name taken between the run's read and
its rename, a throw while a schedule's fields and filters go on, the duplicate field note, a
throw after Made, the two kind refusals, the uncapturable wording, the not available message,
a Set answering false under a controlling template, and four views per sheet.

### Found while writing it, and not fixed

Three things the code says that the sheet could not ask for, written down here for the next
audit rather than changed in a round that changes no code.

- `PanelSteps.NoPlotsTicked("assign")` and the handler's own no plots line for Assign cannot be
  reached. The scope box block returns before it draws the Assign button when no plot is
  ticked, so the two messages have no path. The Run one is reachable and the sheet checks it
- `PanelSteps.NothingToRunYet` says nothing is marked, and the one way to reach it is a mark
  that the plan refused, on a plot with no scope box or gone stale, so it is reachable only
  when it is wrong
- A view type added by hand in step 2 can never be made. `ModelWriter` refuses it with no
  sibling to set it up from, which is right for the settings and means the Add row produces a
  column that marks and always refuses. core-rules.md says inventing a view type is allowed.
  What a new type should be set up from is UNKNOWN and is a question for the team

### Client data

CLAUDE.md and core-rules.md hold client facts, and this repository is public. The count per
file and the kinds of fact found are in the pull request body and nowhere else, because a
tidy index in a second public file makes the exposure worse. Neither file is changed. The sheet
names only identifiers those two files and the audit already carry, the model file, one plot,
three sheet numbers, the four codes with their view names and two parameter names, and nothing
new.

### What ran here

`dotnet test` on the suite before anything was written and again after the sheet was written,
1103 both times, 0 failed, 0 skipped. The banned word scan, the em dash scan and a scan for
any character outside ASCII over the new file found nothing. No agent run was started. The
hooks ran on the real commit.

### Not observed

Nothing in this round has been through Revit, which is the point: the sheet is what goes
through it next. Every passing line in it is what the code says it will print, read off the
code this round, and not one of them has been seen on a screen.

---

## 2026-09-10, forty sixth pass. The Shared round: one thing in, three things out, and what Shared is for

Pull request 64, merged into main as `45138ae`. **The runner executed 1060 tests against it,
0 failed and 0 skipped, the same count main carried before the round and the same count the
local run gave after the last file was written.** Main on `45138ae` reads 1060 locally too,
and the squash message came back byte for byte. Two moves and no behaviour change. Nothing a
user can see differs, and no test was added, deleted or changed in what it asserts. Every
other session was stopped for it, because both moves touch Core/Shared and territory.md says
a Shared change runs alone.

### PaneLabel into Shared

The WPF caption escape was KPI's. Pull request 62 made Drawing Sheet call it across the
fence rather than copy it, which was the right call against two records of one fact and
the wrong shape for two sessions that cannot see each other: task 1 depended on task 2 at
compile time, so a KPI round moving or renaming it would have broken Drawing Sheet with
nobody watching. It is `Core/Shared/PaneLabel.cs` now, with four of its five tests in the
flat test folder. The fifth walks every KPI name and stays with the KPI tests as
`KpiPaneLabelTests`, because the names are KPI's. `TextWithNoUnderscoreIsUntouched` reads
the same five strings as literals rather than through KPI's constants, because a Shared
test cannot lean on a task's file. The KPI panel reads Shared's through an alias rather
than a using of the whole Core namespace, so no other Core name can shadow a KPI one, and
the Drawing Sheet panel reads it plainly. No other KPI file was edited.

### The Drawing Sheet prefixes out of Shared

Finding 28. The scan, scope box and run prefixes and the two-argument `For` that
defaulted to the scan one were Drawing Sheet's report names on a Shared class, so
renaming a report was a Shared change that stopped every other session, for three
constants no other task reads. They are `ReportFileNames` in Core/DrawingSheet now, the
file name tests moved and renamed with them, and `ScanFileName` in Shared keeps `Cleaned`,
`Extension` and the three-argument `For`, which both tasks name their files through. A
Drawing Sheet report name is a Drawing Sheet change again.

### What Shared is for

territory.md gained a section in its own words. A thing belongs in Shared when more than
one task reads it and no task owns its meaning. A task's own constants, names and report
words do not belong there even when they sit in a Shared file today. A task needing
another task's code asks for it to be moved to Shared, in a round of its own, rather than
calling across the fence or copying it. `PaneLabel` and the `ScanFileName` prefixes are
the two worked examples.

### How it landed through the wall

The hook refuses a commit that touches Shared and a task's files together, and a move
across the fence touches both by definition. So the round is four commits, each inside one
territory and each compiling on its own, in an order chosen so that nothing is ever
referenced before it exists: Shared adds `PaneLabel` and its tests, Drawing Sheet adds
`ReportFileNames`, switches its three callers and points its panel at Shared, KPI removes
its copy and aliases the Shared one, and Shared takes the three constants off
`ScanFileName`. The C# lookup that makes the third step safe is that a name declared in an
enclosing namespace wins over a using directive, so the KPI tests, which sit under
RcrcGreen.Core.Tests.Kpi, read the Shared class from the first commit onward without an
edit. The suite read 1072 after the first two commits, the twelve escape test cases existing
twice until the KPI copy went, and 1060 after the third and fourth. Each state was built and
run before it was committed.

### Not observed

Nothing in this round has been through Revit. Both panels are unchanged on screen by
construction, and neither has been opened against a model since the move. No break was made
this round, because no rule changed and the tests are the ones that moved. The hooks ran on
the four real commits and passed them, which is not the same as watching one refuse, and
none was probed.

---

## 2026-09-10, forty fifth pass. The rest of the audit in two pull requests, all but three findings

Two pull requests, because eleven fixes over thirty files was too large for one review and
the brief allowed the split. Pull request 61, fixes 1 to 6, merged into main as `aaf78db`.
**The runner executed 1056 tests against it, 0 failed and 0 skipped.** Locally the head read
1024, and the 32 between are KPI tests pull request 60 landed on main while this one was
open, which the gate builds in: main measured 1056 after the merge. Pull request 62, fixes
7 to 11, merged into main as `6910049`. **The runner executed 1060 tests against
it, 0 failed and 0 skipped, the same count the local run gave after the last file was
written.** Twenty eight of the thirty one findings in `steps/audit-drawing.md` are marked
FIXED, and 11, 15 and 28 stay as they were, for the reasons the brief gave. Drawing Sheet
territory and common files only: no Shared file, no KPI file and no shared Revit root file
was edited, and the one KPI thing this round uses, the caption escape, is called across the
fence rather than copied.

### Fixes 1 to 6

Finding 7 and 19, the report saying two things. `RunReport.Headline` is one number for
everything not made, split the way its sections split it, refused before the run, refused
by Revit during it, created wrong and still in the model, and the line under it says which
count is the plan's. Four before, two during and one left behind read 7 were not, where the
old line read 3. One refusal reads 1 thing cannot be made, and the test that pinned 1 things
pins the singular.

Finding 12 and 17, facts computed and thrown away. The registry's WrongCase entries reach
the snapshot, kept once per text, and the refresh line says them in `IgnoredName`'s own
words. A view whose PRX_Plot_ID holds something that is not a plot is a fourth count, apart
from with no plot at all, and `ViewOnAPlot` carries the raw value so the line shows it. A
schedule whose name does not parse is recorded at capture with the same classification,
counted on the refresh line and named where the name is a plot in the wrong case. The three
clauses are `RefreshWords`, each empty when there is nothing to say. Shared was read and not
edited.

Finding 14 and 21, what nothing reaches. `ScanModelCommand.cs` and `ScanProgressWindow.cs`
are deleted, `AssignScopeBoxCommand` keeps its three statics and loses its Execute, and the
rules file, the hook's list and territory.md follow. Ten members checked again against the
tree: nine deleted with their tests, `SheetGrid.MarkedCount` kept with the four callers the
last round gave it, and `SheetNumbers.PlotLetter` kept with `Propose` calling it through an
overload that hands back the reason, so the letter rule has one home.

Finding 20 and 26, two copies of one fact. `ModelScanner.Number` is the one reader of a
length parameter, `RunReport` writes with `ScanReport.LineEnd`, and
`ScopeBoxScanner.StateOf` is the one reader of a view's scope box state for the counts on
screen and the assignment both.

Finding 24 and 25, step 4 saying the wrong thing. Run names what is unfinished, the title
block, the rows short of a name or a number, or both, then finish that in step 4. The
control is captioned Title block and sheet type appears nowhere.

Finding 16, the tally nobody could act on. Both sheet tallies are gone from the parse
summary and it says why. Measured against the real sheet names this repo records,
LIST OF DRAWINGS, GENERAL ARRANGEMENT LAYOUT, LANDSCAPE CROSS SECTION and SOFTSCAPE
SCHEDULES, none carries a plot or a code, and the tool's own `SheetNaming` makes names of
that shape, so the sheet name tally could only ever read 0 of N. No real model was measured.
CLAUDE.md said sheets are named like views and now says how a sheet is named.

### Fixes 7 to 11

Finding 27. A field the captured definition names twice is added once and named under needs
attention, with the note saying that whether a source schedule really holds two fields under
one name is UNKNOWN.

Finding 29. Every tick box and button carrying a model name goes through
`PaneLabel.Escaped`, the KPI pane's own escape in Core under Kpi, called across the fence
rather than copied, because two copies of one rule is the shape this repo keeps paying for
and territory.md allows the call. Its home should be Shared, which needs a round of its own.
The title block names in the ComboBoxes are left as the audit left them.

Finding 30. `SiblingView.WrongKindInWords` names the kind the nearest view is and the kind
that was wanted, and the writer refuses on it before asking a section for a level or handing
a plan's family type to `ViewSection.CreateSection`.

Finding 8, 18, 23 and 31. The not available message no longer names a ribbon panel that was
removed. Two far clip comments say crop, one moved to the field it describes. The sentence
for a model with no plots names all four sources and lives once, in `PanelSteps`, read by
step 1 and the empty grid, with core-rules.md and the reader's comment saying four. Six
messages the panel formatted itself are `PanelSteps` methods with tests.

Finding 22, and it was two folders. `git log --diff-filter=A` says `design/pr-31/panel.html`,
titled pull request 28, was added by the commit for pull request 28, and
`design/pr-28/panel.html`, titled pull request 28, by the commit for pull request 29. Both
sessions had guessed 28 and the KPI one landed it. So pr-28 is pr-29 with its title changed
and pr-31 is pr-28 with its title kept. The same fault nearly recurred in this round: the
first mockup was named pr-60 from the highest number on the list, the KPI session opened 60
ten minutes later, and the folder was renamed to 61 before the merge.

### Breaks

Six, three per pull request, each a reversed edit with the tree hash the same before and
after:

- The headline dropping the plan's refusals from its sum: Failed 1 of 1024
- Run ignoring the rows short of a name or a number: Failed 2 of 1024
- The wrong case clause listing every ignored name: Failed 1 of 1024
- The kind refusal naming the wanted kind as the kind it is: Failed 1 of 1060
- The no plots sentence dropping the scope box source: Failed 2 of 1060
- The scope box line losing its singular: Failed 1 of 1060

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the three refresh clauses on
a real status line, the Title block caption, the Run reason under a shut step, the scope box
counts and the assignment reading through one `StateOf`, the schedule capture recording its
skips, Scan Model writing the shorter parse summary, the two deleted commands leaving the
ribbon and the panel as they were, the escape on a real name with an underscore, the two
kind refusals on a model that draws a view type both ways, the duplicate field note, the not
available message, and the six moved messages on the panel. Whether any real view or scope
box name holds an underscore is UNKNOWN. The hooks were not probed this round. They ran on
the real commits and passed them, which is not the same as watching one refuse.

---

## 2026-09-10, forty fourth pass. Hidden marks stay off the run, copied numbers lend no letter, and five more fixes

Pull request 56, merged into main as `6bf968e`. **The runner executed 948 tests
against it, 0 failed and 0 skipped, the same count the local run gave after the last file
was written, and six above the 942 main carried before the round.** Findings 1, 2, 3, 4,
5, 6, 9, 10 and 13 of `steps/audit-drawing.md` are marked FIXED under their entries, and
everything else there stays as it was. Drawing Sheet territory only: no Shared file, no
KPI file and none of the five shared Revit root files is touched.

### Fix 1, a mark on a column the grid does not show

The ninth two-records instance, and the one that wrote to the model. A view type ticked
in step 2, marked in step 3 and unticked again left its mark in `_marked`, the panel
handed the whole set to `RunPlan.Of`, which filters by ticked plot and nothing else, and
the run made the view. The MARK header and the status line counted `_marked.Count` over a
grid built from `_columns.Shown`, so the header could say 3 over a grid showing none.

`SheetGrid.Marked` is the one list now: the marked cells the grid holds, in row order,
and `MarkedCount` is its length. The panel builds the grid through one `GridNow` and reads
that list for the header, the status line after a mark or a sweep, the plan preview and
the run. `_marked` stays as the memory, so a mark comes back when its column does, and a
mark on a plot outside the range is off the grid and so neither counted nor run. Two
tests: `AMarkOnAHiddenColumnNeverReachesThePlan` builds a grid with one column and marks on
two view types and hands `grid.Marked` to the plan, which makes one item, and
`MarkedListsTheMarkedSquaresAndNothingOffTheGrid` pins the list against a hidden column, a
plot out of range and a square holding a view.

### Fix 2, the plot letter read off copied numbers

`LettersIn` read 010QE Copy 001 as Q and a test pinned that as intended. CLAUDE.md records
that only DM-11 has numbers of its own and every other plot carries copies of DM-11's, so
on the measured model every plot but DM-11 was proposed DM-11's letter, and the report
would have said proposed from the plot's own numbering over a number that goes into a
drawing register.

A number holding `ScannedSheet.CopyMark` is skipped before its letter is read and is never
a seed for `Free`, so a plot carrying only copies gets the no-numbers refusal it already
had words for, a plot with one number of its own beside a copy keeps its own letter, and
the dropdown no longer offers 010QE Copy 002. `ACopyNumberStillGivesItsLetter` is replaced
by `ACopyNumberGivesNoLetter`, whose comment says why the old pin was wrong, with
`APlotCarryingOnlyCopiesIsTreatedAsHavingNoNumbers` and `ACopyNumberIsNeverASeed` beside
it. Whether the team wants one letter per plot at all is still UNKNOWN, and a proposal
built on copies could not have been the answer either way.

### The other five

Finding 3 and 10, the writer. The sheet name is set inside the same delete-again shape as
the number, with the delete checked and Revit's message quoted. A schedule's link setting,
fields and filters go on inside one that deletes the schedule again on a throw, or records
it under left in the model when the delete fails, because a schedule short of the filter
being added reads as correct on a drawing. `Renamed` quotes what Revit said instead of
naming a duplicate that may not be there. What runs after `Made` for a plan view, a
section and a sheet moved into `FinishPlanView`, `FinishSection` and `FinishSheet`, each
called through `Finishing`, so a throw there is recorded as created and needing attention
with Revit's words, where it used to land in the outer catch as refused and put the item
under created and not created at once.

Finding 4. The sheet's `Parameter.Set` on PRX_Plot_ID and `SetPlotId`'s both check the
return and write the note the scope box writes, so a false is no longer a clean report.

Finding 5. `Remembering` keeps a viewer's vertical and horizontal offsets by name and
restores both on the first layout pass. `Scrolling` goes through it, the grid's row viewer
and column viewer are named, step 4's view list and table are named per sheet, and the
bare two-argument overload is deleted.

Finding 6. `Took` reads the unticked plots before it puts the range back and unticks them
again after, and returns the cleared count. `Read` on the handler is a
`Func<DrawingSheetSnapshot, string>` now, and the refresh message ends with
`BulkMarking.ClearedInWords` when anything was cleared.

Finding 9. `FreeSheetNumbers` is a stored list worked out in the snapshot's constructor,
the panel reads both dropdown lists once per redraw into a `SheetOffers`, and every name
box and number box takes them as its `ItemsSource`, so 1,385 names are no longer copied
into every row of every table on every redraw.

Finding 13. `SheetBatch.RowsWithTypedText` counts rows where the name or the number was
typed rather than proposed, `WhyRemovalAsks` is the question, and `RemoveASheet` puts it
up as a WPF Yes and No box with No as the default, only when the count is above zero.

### Breaks

Four, each a reversed edit rather than a checkout, with the diff hash the same before the
first and after the last:

- Marked columns leaking into the grid's column list: Failed 2 of 948, the two fix 1
  tests and nothing else
- Both copy filters removed from `SheetNumbers`: Failed 3 of 948, the three fix 2 tests
- `RowsWithTypedText` ignoring the generated flags: Failed 1 of 948,
  `RemoveAsksOnlyWhenARowCarriesTypedText`
- `RowsWithTypedText` counting typed names only: Failed 1 of 948,
  `ARowCountsOnceWhicheverBoxWasTypedAndAnEmptyBoxDoesNotCount`

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the grid and the step 4
lists coming back where they were scrolled, and whether restoring both offsets on one
layout pass holds for a viewer with one axis disabled; the Yes and No box over a docked
pane; the refresh clause on the status line; the unticked plots surviving a refresh; the
empty number box on a copy-only plot and the dropdown without copies; a name box fed by
`ItemsSource` while the user types; the sheet name delete-again; the schedule delete-again
on a throw, and whether `AddField` or `AddFilter` throws at all; `Finishing` turning a
throw into needs attention; and the two `Set` checks. The mockup at
`design/pr-56/panel.html` is drawn from the code and says so.

The hooks were not probed this round. They ran on the real commit and passed it, which
is not the same as watching one refuse.

---

## 2026-09-10, forty second pass. Three homes in Core, a wall between tasks, and the files split

Pull request 50, merged into main as `e0a7a97`. **The runner executed 912 tests against
it, 0 failed and 0 skipped, the same count the local run gave after the last file was
written, and the same count main carried before the round began.** Nothing a user can see
changed, no tool behaviour changed, and no test was added or removed, which is why the
count holding at 912 is the round's own proof.

### The move

Core's 66 flat files went into three homes with git mv, every rename at 100 percent
similarity. Sixteen to `Core/Shared`: the named list, plus `IgnoredName` and
`PlotRegistryResult`, which had to follow because `PlotRegistry.Build` returns them and a
Shared file may not lean on a task's file. Fifty to `Core/DrawingSheet`. `Core/Kpi` as it
was. No file content changed anywhere, because C# does not tie a namespace to a folder
and the csproj globs, so no namespace and no using had to follow. A textual check
confirmed no Shared file names any of the 79 types the DrawingSheet folder defines.

Calls that could have gone either way, all put in DrawingSheet because the narrower home
is the safer mistake. `ReportPlaces` names no task and the KPI pane reads it at three
call sites today, and it stays Drawing Sheet's under the user's rule that what task 1
built stays owned. Reading it across the fence is allowed and still compiles, one
assembly. `PlotBox`, `ViewOnAPlot`, `ViewReading`, `ViewPlotReader`, `ViewPlotReading`
and `PlotSourceOnView` name no task either, and all were built for the grid and the
sections, so they went with them.

### The wall

`.claude/rules/territory.md` holds who owns what and `.claude/hooks/territory-check.sh`
holds the wall, wired in settings.json. It reads the commit through a new `all-paths`
action on commit-scope.py that keeps deletions in, because deleting another task's file
is an edit too. Drawing Sheet's Revit files sit flat at the Revit project root, so the
hook names those fifteen one by one, and the five root files every task shares stay
outside the fence: the application class, the theme, the metrics, the report writer and
the manifest.

Probed by hand against a scratch index, nothing committed from any probe:

- A Drawing Sheet file next to a deleted KPI file: "Refused. This commit touches two
  tasks' territory: DrawingSheet (src/RcrcGreen.Core/DrawingSheet/PlotBox.cs) and Kpi
  (src/RcrcGreen.Core/Kpi/AreaUnits.cs)." The KPI touch was a staged deletion, so the
  ACMRD path is what saw it
- A Shared file next to a task file: "Refused. src/RcrcGreen.Core/Shared/Lengths.cs is in
  Core/Shared and this commit also touches DrawingSheet"
- A Revit root Drawing Sheet file next to a KPI file: refused the same way, which proves
  the root file list maps
- One task plus common files passed, and Shared plus common files passed
- The reworked require-file-on-commit.sh refused a commit with no state file, "No
  steps/ai-max-state-<task>.md is in this commit", and passed with the drawing one

The wiring loaded live the moment settings.json changed, and the wall's first real catch
was this session's own probe command, refused because the real index then held Shared and
DrawingSheet renames together. So the round landed as two commits: the DrawingSheet half
with everything common, then the sixteen Shared files alone, which is the shape the wall
holds every later commit to.

### The split

`steps/log.md`, 269 KB over 42 entries, went to `steps/log-drawing.md` with 26 and
`steps/log-kpi.md` with 16, by which session wrote each entry. The branch name settled
the one surprise: the twenty fourth pass names `claude/inspiring-allen-xs113f`, the KPI
session's branch, so it went to the KPI log. The passes before the split went with
Drawing Sheet, whose session wrote them. The state file split the same way, every line
from 3 to 720 in exactly one file, checked, with the shared phase 1 to 8 history at the
foot of the drawing file and the KPI top reworded from a Before that into a Phase line,
the one seam the split needed. Both old paths hold a short pointer, and the live pointers
in CLAUDE.md, kpi-rules.md and both state files follow the split.

### What GitHub puts back, measured

The squash message for this round's own merge was passed by hand on the merge call, title
and body both. Read back off main, it is byte for byte what was passed, with nothing
appended. So the credit line GitHub added to `e0a7a97`'s two predecessors comes only with
the default message, and passing the message on the call is a working remedy from a
session. territory.md now says so as a measured fact. The repository settings route
stays unavailable from here, because no tool in this session changes repository settings.

### Open questions

- Pass numbers were one sequence across both sessions. Two logs now count independently,
  so either each continues its own or the user assigns ranges. This entry keeps the
  shared count
- Drawing Sheet's flat test files read as common to the wall, mixed with the Shared ones
- The Revit root fence is a file list, so a new Drawing Sheet file at that root has to be
  added to it by hand

### What did not run

- Nothing in this round has been through Revit. The moved tree builds the same assemblies
  and the suite is unchanged at 912, and still no Revit has loaded the result
- No second live session has hit the wall yet. The refusals fired in probes and once
  against this session's own index, never yet against another session's real commit
- The zero behaviour claim rests on the build, the suite and the renames being 100
  percent similar, not on a run of either panel

---

## 2026-09-10, forty first pass. The report for the seven fixes round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 48 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 48 merged as `e0e206a` and the gate executed 912 tests against it, 0 failed and
0 skipped, which is the same count the local run gave. That run was the reworked gate's
first: it restored and built the whole solution, RcrcGreen.Revit included, so the item the
fortieth entry listed as not observed, the workflow on the runner, has now run and passed.

The design folder was named pr-48 before the pull request existed and the pull request opened
as 48, so the guess held. Nothing else changed. No code is touched.

---

## 2026-09-10, fortieth pass. The plot list is the union, and capture drops nothing

Branch `claude/rcrc-green-setup-wf9ham`, one commit. The merge sha and the runner count go in
the next entry, because this one is written before the pull request exists. Seven audit
findings fixed, numbers 1 to 6, 8 and 12 in steps/audit.md, and nothing else touched. Every
other finding stays written down as it was.

### Fix 1. A loss at capture is as loud as a loss at write time

ScheduleCapture read a filter whose value Revit refused and a field whose id resolved to
nothing with a bare continue each, so the writer's lost filter guard never saw either. Both
are recorded on the definition now, in FiltersNotRead and FieldsNotRead. A definition that
lost a filter never reaches the usable set: the plan refuses its type with
WhyTheCaptureLossRefusesIt, naming each lost filter, and MakeSchedule checks again before
CreateSchedule because the writer trusts no list it did not build. A lost field still builds
the schedule and lands under CREATED, BUT NEEDS ATTENTION, named by position, the same split
as write time: a missing filter lies, a missing column shows.

### Fix 2. The plot the panel silently dropped

The panel's plot list came from views alone. PlotRegistry, which held the union rule with
tests all along, is wired in: the reader hands it every view name, every PRX_Plot_ID read off
a view, every scope box name and every element value, and the element values come through
PlotIdValuesAcrossElements, the scan's own walk pulled out of ReadPlotIds so the two reads
are one loop. PRX_Plot_ID on a view is a new source, ViewParameter, because filing it under
elements would mark a plot that has views as needing everything. The snapshot carries the
records, and step 1 shows box only, no views, elements only, no views, or box and elements,
no views on the rows no view carries, with every source in the tooltip. The refresh now walks
every element, which the measured 1.4 seconds for 96,934 elements says the panel can afford.

Deleted, with their tests: PlotViewGrid, MissingViewFinder, MissingViewType and
PlotMissingViews, still called by nothing after the wiring, and PlotViewGridTests whole. The
flow test was rewritten to the live seam, registry to grid, and its box-only plot now proves
the fixed behaviour instead of a dead one. Kept: PlotRegistryResult, PlotRecord, PlotSource
and IgnoredName, because Build returns them and the product reads them now. Kept too:
PlotId.PatternAnyCase, FailsOnlyOnCase and ViewNameParser.FailsOnlyOnPlotCase, because the
ignored-name classification inside Build executes them on every read, even though nothing
shows the Ignored list yet. That list having no reader is written down here as an open
question rather than deleted, since a mistyped dm-41 scope box is exactly what it catches.

### Fix 3. A stale mark no longer leaves an orphan

RunPlan takes the fresh read's cells and refuses a mark whose view already exists, before
kind or scope box. If a name is still taken when the writer renames, Renamed catches the
refusal, deletes the just-created view, schedule or section again with the delete checked,
and says IT IS STILL IN THE MODEL when Revit refuses that delete, the same way MakeSheet
handles a refused number. The handler comment that claimed the re-read already stopped this
now says what the code does.

### Fix 4. Parameter.Set is checked everywhere it is called for a new view

The scope box set and the annotation crop set both ignored the bool Revit hands back while
AssignScopeBoxCommand treated the same false as a refusal. Both are checked now and a false
lands under CREATED, BUT NEEDS ATTENTION. Whether this explains the user's repeated report of
views coming out with annotation crop off while the report read clean is now TELLABLE: if Set
answers false on those views, the next run will say so against each one. Whether it does
answer false there is UNKNOWN until a run. Noted while fixing: SetPlotId and MakeSheet's plot
parameter ignore the same return, are not on this fix list, and stay as they are, written
down here as the same shape.

### Fix 5. One rule for which schedules can be made

The panel passed every ScheduleType as capturable while the handler passed the captured
definitions' keys. ScheduleCapture.Read now classifies every schedule once: usable, lost a
filter at capture, or filters on no plot, and the snapshot carries the same three answers
from its own read. Both sides hand RunPlan the same rule and only the data can differ. An
uncapturable schedule is refused with its own reason, so a schedule that exists and filters
on no plot is no longer denied outright, and the old wording survives only for a type with no
reason recorded.

### Fix 6. The gate sees the whole solution

tests.yml restores and builds RcrcGreen.sln before testing, runs on pushes to main as well as
pull requests, and its comment now says the add-in compiles without Revit because the
Nice3point packages supply the reference assemblies, which this repo's own builds prove. The
exact restore, build, test sequence was run here before committing it.

### Fix 7. The snapshot is pinned

DrawingSheetSnapshotTests covers the numbers grouped per plot, the scope box names narrowed
to plot identifiers, the free numbers built from the numbers rather than the names, the plot
records with their words, and the capturable lists. The three audit breaks were made again
with the tests in place:

- NumbersOnPlot returning empty always: 1 failed, 911 passed
- the PlotId filter dropped from PlotsWithAScopeBox: 1 failed, 911 passed
- FreeSheetNumbers fed SheetNamesInUse: 1 failed, 911 passed

Two more breaks on the new rules: the ViewParameter source dropped from the union failed 2,
and the presence refusal disabled failed 1. All five restored, suite green at 912 after each.

### What ran here

dotnet restore, build of the whole solution and the full suite, repeatedly and after the last
file: 912 tests, 0 failed, 0 skipped, up from 904 with the deleted PlotViewGridTests out and
the new snapshot, registry, definition and plan tests in. One slip worth recording: restoring
a break with git checkout wiped the uncommitted snapshot work once, it was reapplied from the
session's own record, and every restore after that reversed the edit instead.

### What did not run, item by item

Nothing in this round has been through Revit.

- The element walk inside the panel refresh has never run there, and its cost on the real
  model is inferred from the scan's measured 1.4 seconds, not measured in the panel
- No box-only plot has been seen on a row, and how the suffix wraps at pane width is UNKNOWN
- No capture has recorded a real unreadable filter or field, and whether Revit ever throws on
  those reads in practice is exactly what the guards exist for and is UNKNOWN
- No stale mark has been refused in Revit and the rename-then-delete path has never executed
- Whether Parameter.Set answers false for a template-controlled scope box or annotation crop
  is UNKNOWN until a run says so
- The reworked gate has not run on the runner yet when this entry is written

---

## 2026-09-09, twenty sixth pass. The report for the divided sheets round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 32 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 32 merged as `6f212e1` and the gate executed 619 tests against it, 0 failed and
0 skipped, which is the same count the local run gave after the restack onto main.

The design folder was named pr-32 before the pull request existed and the pull request opened
as 32, so the guess held this time. Nothing else changed. No code is touched.

---

## 2026-09-09, twenty fifth pass. A sheet has no scale, and one description makes a set

Branch `claude/rcrc-green-setup-wf9ham`, one commit, restacked onto main after the two KPI
rounds landed there mid round. The merge sha and the runner count go in the next entry,
because this one is written before the pull request exists and the last time a number was
guessed here it was wrong.

Off the third full run: 13 created, 0 refused. KERBS built on its category number, both budget
filters went back as integers, and the title block size read 841 by 594 mm off the placed
block. Three faults remained and one question had to be answered before anything moved.
Leading with that one, item 5.

### 5. A sheet has no scale of its own, so there is nothing to set

The reading offered was that the Scale a sheet shows is a readout of the views placed on it,
each from its own view template, not a setting of the sheet. Checked against the API before
acting on it, and it holds:

- An Autodesk forum thread on Scale as Indicated, forums.autodesk.com, Revit Architecture
  forum, "Issue with Scale in view title, Scale as Indicated", says the sheet's Scale
  parameter is enabled only when at least one view is placed on the sheet and every placed
  view has the same scale, and reads As Indicated otherwise
- The View.Scale page on revitapidocs.com, the 2026 edition of the ViewScale property, which
  is a property of the view, not of the sheet, and is what a view template drives

So the sheets that looked wrong were showing exactly what sat on them, and the fault behind
the complaint was item 2: one typed name went onto every sheet, so a location key plan sat
under GENERAL ARRANGEMENT LAYOUT and read as a layout at a mad scale. Nothing in this round
sets a scale on anything, because a view's scale belongs to its template and overriding it
would fight the team's own settings. The run report now records the scale of every placed
view, and its closing note says whose the scale is.

How real sheets place their viewports is a separate question and it is UNKNOWN. The scan
grew a VIEWPORTS ON EXISTING SHEETS section, scale, centre, size and sheet size in
millimetres off every placed viewport and schedule, sizes off the placed title blocks by
OwnerViewId, so the tool's centred placement can be held against the team's real ones the
next time Scan Model runs. Until then nothing about placement was changed on a guess.

### 1. One sheet definition makes as many sheets as the views need

`SheetDivision.Of` in Core chunks the ticked views, in the order they were ticked, into
`PlannedSheet`s. Six views at one per sheet is six sheets, at two is three, at four is two
with the second holding two, and a flatten test proves no view is ever left off. The old
`Placed` and `LeftOff` split on `SheetDefinition` is deleted, and so is the sheet name it
carried, because one typed name across a divided set is the fault item 2 names.

**Superseded: a definition with no views used to make an empty sheet on purpose.** It makes
no sheets now. The division says a sheet is what its views need, and no views need nothing.
The report's closing note and the step 4 words both changed with it, and the panel line that
promised an empty sheet is gone. Flagged here rather than slipped in, because it reverses a
behaviour an earlier round wrote down as wanted.

### 2. A one view sheet is named from its view

`SheetNaming.FromView` upper cases the view name and drops the bracketed code, so
(200) General Arrangement Layout proposes GENERAL ARRANGEMENT LAYOUT, the way the team's own
sheets read. The name sits in an editable box prefilled per row. A sheet holding more than
one view gets no proposal, the row says why in `PlannedSheet.WhyNothingIsProposed`, and the
sheet is not made until a name is typed.

### 3. A sheet number continues the plot's own pattern

`SheetNumbers.PlotLetter` reads the letter after the leading digits of each of the plot's own
numbers, DM-11's 010QE to 600QD all read Q, and every parseable number must agree or there is
no letter. `SheetNumbers.Propose` offers the code, that letter, then the first sheet letter
from A not in use anywhere, so DM-11's code 010 proposes 010QA even though QE to QH exist,
because A is free. A plot with no sheet numbers gets an empty box and the reason. One growing
set of taken numbers is threaded through every row of every described sheet, so two proposals
on one panel can never collide, and a typed number joins the set so later proposals move out
of its way. `FaultIn` still warns under the box for a number the model holds or a number
asked for twice, the same method the run summary counts with.

### 4. Step 4 is a table of the sheets that will be made

Per definition: Type, Views per sheet, the view ticks, then one row per sheet per ticked
plot. Views read only, name and number editable, prefilled where a proposal exists. The line
over the table is `SheetBatch.InWords`, how many sheets this press makes across the ticked
plots and how many rows are still short. What the user types is filed under the plot and the
planned sheet's own views, so an edit survives the redraw and stays with its sheet while the
division changes shape. A keystroke refreshes every other box in place, skipping the focused
one, because a proposal that moved has to show its move and rebuilding the tree would take
the cursor out of the box. `RunPlan` refuses an unusable definition once, refuses a row short
of a name or a number by plot and views, and drops a row on an unticked plot silently, the
same as a mark.

### 6. Both reports say where every viewport landed

The run report grew WHERE EACH VIEWPORT LANDED: after the sheet's placements are created,
`ModelWriter` regenerates once and reads each viewport's centre and outline back off what
Revit made, with the view's own scale, a schedule's bounding box the same way, next to the
sheet size. `ViewportRecord` in Core formats it all in millimetres. The scan reads the same
shape off every existing sheet, so the two files can be held against each other. The report
also names, per sheet, whether its name and its number were generated or typed.

### 7. The lowercase s is answered

NS-19-(400) Landscape Cross section is spelled that way by its own sibling. The model does
it, so the tool copying it is correct. Nothing to do, recorded so it is not chased again.

### 8. Sheet parameters are out of scope

PRX_ parameters beyond PRX_Plot_ID, and Approver, Designer, Checker and Author, are not
filled, get no controls and are not raised in the report. The team's decision, recorded so
nobody reads their absence as an oversight.

### What ran here

`dotnet build` on the solution and `dotnet test` on the suite after the last file: 475 tests
on the pre KPI base, then 619 with main merged in, 0 failed, 0 skipped on both. Three guards were broken on purpose and watched go red before being
trusted: the division stepped past a view and the flatten test caught it, the taken number
guard was bypassed and the stepped over letter test caught it, and the ticked order was
sorted and the order test caught it. All restored, suite green again.

### What did not run, item by item

Nothing in this round has been through Revit.

- No sheet has been made by the division, and no run has executed any of this round's code
- No proposal has been rendered. Whether the four column table fits the docked pane, whether
  a warning line under one box pushes its row out of step, and how the table reads at 17
  plots by 3 planned sheets are all UNKNOWN
- The keystroke refresh has never been exercised. Whether writing ComboBox text while its
  dropdown is open fights the selection is UNKNOWN
- No `ViewportRecord` has been written by a real run. `GetBoxCenter` and `GetBoxOutline`
  units and origin are taken from the API documentation, not measured
- Whether a `ScheduleSheetInstance` bounding box is readable after the one regeneration is
  UNKNOWN, and a null there records zeros, which print as 0 mm rather than a plausible size
- The scan's viewport section has never run, and its cost across 1,385 sheets and 953 placed
  views is unmeasured
- The forum's account of Scale as Indicated has not been reproduced on this model

---

## 2026-09-09, twenty second pass. The report for the second full run round

Branch `claude/rcrc-green-setup-wf9ham`. The entry below went in with its work, so it could not
carry its own merge or runner count. It carries them now.

Pull request 29 merged as `a44fe4f` and the gate executed 436 tests against it, 0 failed and 0
skipped, which is the same count the local run gave.

The pull request number in that entry read 28 while the pull request opened as 29, because the
entry is written before the pull request exists. Corrected here rather than left to be found.

Nothing else changed. No code is touched.

---

