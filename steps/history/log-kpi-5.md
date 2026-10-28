## 2026-09-12, fifty fifth pass. The room the rounding earns, and a status line that moves

Two things off the 1208 run on RCRC_NG03_EZ, 104,031 elements and 18 plots, the first run on a
second model. The reading reuse held on its first real run, a 123 second scan and a 2.5 second
create, which is finding 34 doing its job. **The other 36 audit findings stay open**, not
renumbered, not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `07f16ba`.

Pull request 81, merged into main as `8578f86`. **The runner executed 1261 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1261 ran at `8578f86`, 0 failed and 0
skipped**, 694 of them KPI. **The branch itself carried 1246, 22 added here**, 1243 before the
breaker round below added three, against the 1224 main held at the branch point `07f16ba`.
The 15 above it are the Drawing Sheet fifty sixth pass that landed in between, `e4b5d9e` and
`c7d91c4` with their record `eaed0e9`, whose files are the only ones that differ from this
branch and none of them KPI, checked file by file. The merge went through the API with the
title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### The group total check refuses on rounding no more

It refused FM-21, Existing 2 over 0, Proposed 51 over 11, group total 52 over 11, where 2 plus
51 is 53, and FM-22, 2 over 0, 80 over 46, 83 over 46, where 2 plus 80 is 82. The counts match
exactly, 11 and 11, 46 and 46, the areas are off by one in opposite directions, and the model's
own scan prints Area unit, rounded to 1, so every printed area is already rounded and a sum of
rounded numbers need not equal a rounded sum. The forty seventh pass said exactly that about
the species sum and chose to record rather than enforce, the group total check was enforced
exactly anyway, and it fired on correct data.

The rule now, in `ShrubsAndLawnRows.Disagreeing`: **counts are integers and get no room at
all**, a count that disagrees still refuses whatever the rounding. **Areas get half the unit's
rounding step for each row summed**, so two rows rounded to the metre may be off by up to one.
The step is read off the project units by `KpiReader.AreaUnit` once per press, the same read
the scan prints, threaded through `KpiPlotReader.Read` into `ShrubsAndLawnRows.Read` as a
required argument so no caller can lose it in silence, and never a constant. Within the room
is the `RoundingNote` on the `GroupSubtotal`, printed beside the group total row in the
report with the rows, the total and by how much, so a real fault growing slowly stays
visible. Outside the room still refuses naming the room, and a step that was not read allows
nothing and says so, because a check that cannot see its subject must not quietly widen.
Both measured plots are tests, byte for byte: both go through, both are noted, and the same
numbers with a count one high still refuse.

### Every place printed numbers are added against a printed total, named

- **The shrubs and lawn phase rows against the group total row**: the one that fired, fixed
  above
- **The softscape species rows, taken and left out, against the printed TOTAL row**: integer
  counts, exact, right as it is
- **Each softscape group's species rows against its own subtotal row**: integer counts, exact,
  right as it is
- **`Totalled.Adds`**: the tool's own sum against the tool's own total, both computed from the
  one list, so it is a guard for a future caller rather than a live check. Left alone
  deliberately, and its `Tolerance` constant is shared with the height and diameter difference
  detector in the plan, so widening it would have silently swallowed real disagreements there
- **The NOT WRITTEN line**: integer sums against no printed total, nothing to tolerate
- **The species rows against the shrubs and lawn group's value**: recorded rather than
  enforced by the forty seventh pass, and the review of every summed check found the record
  was MISSING. `GroupSubtotal.SpeciesSum` was computed, its docstring said printed, and
  nothing printed it anywhere, so a drift there was invisible. It prints now beside the group
  on any real difference, recorded rather than enforced, unchanged as a decision. Its first
  shape gated the line on 0.005, a constant pretending to be a rounding room, which the
  breaker caught and the round below cured: the gate is the shared drift epsilon alone and
  the line's two numbers print to six places, so 169.996 against 170 is recorded rather than
  swallowed or printed as 170 against 170

### A status line that moves

One line per piece of work done, never a timer, never an estimate. `ProgressWords` in Core
holds the words and the counting with tests, the handler raises them as the work begins, and
the pane's `Moved` shows them. The scan announces each section off the report's own numbered
headings, Section 2 of 9, project information, then 3, then 4, and the schedules loop counts
every schedule, Sections 5 to 8 of 9, schedules, 400 of 951, 42%, a span because one reader
covers those four sections in one pass and pretending them apart would be a lie. Create names
the plot, Reading DM-44, plot 3 of 18, 11%, with the percentage of plots finished so it never
goes backwards, then the steps name themselves: adding the plots up, copying the template,
writing the cells, reading the written cells back, checking the workbook's own formulas,
writing the report. The patcher raises its four steps itself through a callback, so the words
come from the work. A percentage appears only where the total is known and is floored, 950 of
951 is 99 and 951 of 951 is 100, and the end line still comes through `Told` on every finish,
refusal and throw, so the last thing on screen is never a count that stopped moving.

**Whether the line visibly moves mid run is UNKNOWN until somebody runs it.** The pane can
share Revit's own thread, where a text set from inside the external event sits unpainted until
the run returns. `Moved` queues one empty job at render priority after each line, which lets
the paint through when the threads are one and costs nothing when they are not. No test can
reach it and no mockup can show it, the same class as the black on black panel, so the first
run on a real pane is the check. The job began at background priority and the breaker round
below moved it to render, because waiting on a job pumps everything queued at or above its
priority and input sits between the two: at background every queued click would have been
dispatched inside `Execute`, and two of the pane's buttons open a folder dialog owned by no
window, behind which Revit's own ribbon stays live mid write. At render the paint goes
through and every click stays queued until the run returns. The paint sits at render
priority, so what background let through for it, render lets through the same.

### The breaker read the committed diff and five of its ten findings changed code

Run after the round's first commit, before the pull request went ready. Fixed:

- **A phased group printing no total row was taken as silently as one whose total was checked
  and agreed.** Real shape: a phase heading, its species, one subtotal and no group total row.
  The check has no subject there, which is not the schedule's failure, but nothing anywhere
  said the check never ran, the exact skip the softscape TOTAL already says. The report says
  it now, the group printed no total row after its phase rows, so nothing checked what they
  add to, beside the group, with a test
- **The note's two place numbers could contradict the decision.** On a project rounding to
  0.001, off by 0.0012 within a room of 0.0015 printed as off by 0 within the 0 that rows
  rounded to 0.001 allow, right decision, unreadable sentence. `Said` prints six places now,
  chosen because two swallowed a fine step and twelve printed the last bits of a double on
  summed thousands as digits nobody printed. FM-21 and FM-22 read byte for byte as before
- **The species record was gated on 0.005**, a constant pretending to be a rounding room in
  the same commit that cured the check of one. Gated on the shared drift epsilon now and
  printed to six places, so any real difference is recorded and none prints as equal
- **A reused press said nothing about reuse on the live line.** A 2.5 second finish where the
  last press read for two minutes looks like something skipped until the screen says reuse.
  The reused branch raises Reusing the readings already held, the report's Readings line
  unchanged as the record
- **The background priority pump would have dispatched queued clicks inside `Execute`.**
  Waiting on a job pumps everything at or above its priority, input sits above background,
  and the pane's buttons stay live through a run, two of them opening a folder dialog owned
  by no window. A click queued during a scan would have run its handler mid read, and the
  dialog would have left Revit's ribbon live mid write. The pump runs at render priority now,
  above input, so the paint goes through and every click stays queued until the run returns.
  No test reaches it, net48, recorded here instead

Found and left, each with its reason:

- **The room has no ceiling.** A project rounding areas to 10 hands the check a room of five
  per row summed, and a real fault inside it is a note rather than a refusal. That is what
  half the unit's step for each row summed means on a coarse unit, the rule is measured and
  Bader's, and a ceiling would be a rule nobody gave. Open question in the state file
- **The note lives in the report file and the pane shows the ordinary success line.** A run
  carrying notes looks identical on screen to one carrying none. The round message said a
  line in the report, which is what exists. Whether the pane should count notes beside the
  written cells is an open question in the state file
- **An accuracy read as zero or negative would print as not read.** Nothing in Revit's read
  is known to produce one, `FormatOptions.Accuracy` on the measured models is positive, so
  the branch is dormant and the message imprecise only on a value nobody has seen. Left
- **`GroupSubtotal.Repeats` is computed and printed nowhere**, a shape older than this round.
  Left for a round of its own rather than widened into this one

### Break watches

Seven, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green,
the first four at 1243 before the breaker round, the last three at 1246 after it.

- the rounding room dropped: **3 red**, FM-21, FM-22 and the report line printing the note
- a count given the areas room: **1 red**, the count one high that must still refuse
- the percentage rounded up: **2 red**, the schedule count and the plot line
- the copy step never raised: **1 red**, the patcher's four steps in order
- `Said` back to two places: **1 red**, the fine step note printing its numbers
- the unchecked group line dropped: **1 red**, the phased group with no total row
- the species record gated on 0.005 again: **1 red**, 169.996 against 170 swallowed

### Existing tests changed

None moved of their own accord. `ShrubsAndLawnRows.Read` takes the project's area unit as a
required argument now, so fifteen call sites across seven test files pass `ProjectUnit.Unknown`
by name, which is the read today's fixtures had. The three old disagreement tests still refuse,
their fixtures reading no step, and their sentences gained the clause saying the step was not
read, which their Contains assertions do not pin.

---

## 2026-09-12, fifty fourth pass. The diameter alone decides, and the audits are marked with what is really fixed

Two things off the round message. The message opened saying no behaviour changes in the tool,
and the first item is one, deliberately: a species with a diameter and no height was withheld
and is now written. **Eight findings gained their FIXED mark, six of the nineteen Bader
believed closed are not, and the audits now carry the whole record: 49 findings, 13 FIXED, 36
open, read off the files.** No finding's text changed, none was renumbered, none reordered.
Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`, not
`PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in this round has been observed in
Revit.** The branch came off a fresh pull of main at `9fd1a65`.

Pull request 78, merged into main as `a407284`. **The runner executed 1224 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1224 ran at `a407284`, 0 failed and 0
skipped**, 672 of them KPI. **The branch itself carried 1221, none added net**, one test
replaced one for one, against the 1221 main held at `9fd1a65`. The 3 above it are the Drawing
Sheet round that landed in between, `b7d41dd`, whose files are the only ones that differ from
this branch and none of them KPI, checked. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### The height is not load bearing

Measured by Bader on the MOSQUES template, Tree List - Proposed row 21:

```
I21  15                          a plain value, nothing reads it
J21  8                           read by L
K21  10                          nothing reads it
L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))
M21  =IF(ISBLANK(B21)," ",L21*B21)
N21  60                          a typed value the tool never writes
O21  =IF(ISBLANK(B21)," ",N21*B21)
```

Only the diameter feeds a formula, so requiring both measures withheld a row for a cell
nothing reads. That came from the round message's wording, the fifty third pass recorded it as
exactly that, and this is the answer. `SpeciesMatching.WrittenInto` asks the canopy diameter
alone before it takes a row. A species with a diameter and no height is written, its name,
count and diameter into their cells, and its height cell is named as not written through
`Measured`, the same skip every other measure cell uses, so the report says it the way it says
the rest. A species with no usable diameter is still withheld and still named with its count.
UNKNOWN is unaffected either way: DM-25 row 19 prints 0 for its diameter, which is no size.
`NotSized` quotes the diameter rows alone now, and every test pinning the both measure wording
moved with it. The test that withheld a species missing one measure inverted, one for one:
`ASpeciesWithADiameterAndNoHeightIsWrittenAndItsHeightCellIsNamed` proves the row is written,
B5, D5 and J5, with I5 named and the height reason quoted.

### The audits are the record, verified at today's lines

Each of the nineteen findings Bader believed closed was checked against the code as it stands,
by searching for the subject rather than trusting the audit's own line numbers, and against
`steps/log-kpi.md` for the pass that closed it. Thirteen verified and are marked, the five
that already were and eight marked now:

- **1**, the template overwrite guard, and **8**, the empty status line: the fortieth pass,
  pull request 52
- **2**, the cell position fallbacks, **30**, the thousands separator, **31**, the area read
  on STREETS, and **32**, the softscape TOTAL row: the forty fifth pass, pull request 58
- **5**, the tree list row range, and **36**, two schedules of one kind: the forty sixth
  pass, pull request 59

**Six of the nineteen are not closed: 3, 4, 6, 12, 21 and 26.** Each was found byte for byte
in the shape its finding describes, at today's lines: `Preselect` still returns past its own
guard, `KpiNames.Component` is still the capitals no model holds, `Printed` still swallows an
`ApplicationException` with nothing recorded, `Ask` still lets Scan, Plots and Create
overwrite each other, the Written as box still goes stale after a tick, and
`CreateWords.NoTemplate` still cannot reach the screen. They are the round the log has twice
recorded as never landing on main, and they stay unmarked and open.

**One thing the verification surfaced beyond the count.** `PaneChoicesTests` pins finding 4's
unfixed behaviour as if it were the rule: `TheNameIsMatchedWholeAndWithItsCase` asserts the
ordinal compare against the capitals constant and `AWantedNameTheModelDoesNotOfferFallsBackToTheFirst`
asserts the silent fall through, on an offered list ordered so the fallback looks safe. When
the round closing 4 lands, those tests have to move with it or they will hold the fault in
place. Recorded here for that round, whichever session runs it.

The bookkeeping rule that follows, and it is the repo's own: **the audit files are the one
record of what is open.** The state file now carries the count read off them, 49 findings and
13 FIXED so 36 open, and no running count anywhere else. Bader's fourteen was one off the
verified thirteen, which is what a second record drifting looks like in the bookkeeping.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rerun green at 1221 after a
rebuild, because the first rerun after the second restore tested the still patched binary and
read 9 failed, which is the stale build rule in `CLAUDE.md` caught in the act.

- the gate back to requiring both measures: **1 red**, the inverted test
- the diameter gate removed: **9 red**, every withheld case across four files, UNKNOWN's among
  them

### Existing tests changed

The both measure wording moved to the diameter wording in `SpeciesMatchingTests`,
`CanopyColumnsTests`, `TreeListRowsTests` and `DiameterColumnTests`, and
`ASpeciesMissingOneOfTheTwoMeasuresIsNotWrittenEither` became the inverted
`ASpeciesWithADiameterAndNoHeightIsWrittenAndItsHeightCellIsNamed`. Nothing else moved.

---

## 2026-09-11, fifty third pass. A species the model does not size gets no row at all

One rule, measured on the 1836 run over 20 mosque plots. **The other audit findings stay open**,
not renumbered, not reordered, not annotated, and this round closes none of them: it is a fault
off a run rather than an audit entry. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `c47bccb`.

Pull request 75, merged into main as `be72119`. **The runner executed 1198 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1198 ran at `be72119`, 0 failed and 0
skipped**, 672 of them KPI. **The branch itself carried 1187**, 5 added here against the 1182
main held at `c47bccb`. The other 11 are the Drawing Sheet round that landed on main while this
one was being written, `f72834e`, and the merged head is the first place the two counts meet.
The only files that differ between this branch and main are that round's, none of them KPI,
checked. The merge went through the API with the title and the message both passed on the call,
and the commit came back off main carrying neither a co-author credit line nor a generated-by
footer.

**A count I could not reconcile.** The round message says the other 32 audit findings stay
open. At this branch the two audit files hold 49 numbered findings and 5 FIXED marks, 9 and 16
in `steps/audit-kpi.md` and 34, 35 and 39 in `steps/audit-kpi-2.md`, so 44 stand as marked here.
The round that was to close 3, 4, 6, 12, 21 and 26 has still not landed on main. Nothing was
renumbered or annotated either way, and the difference is written down rather than resolved.

### What happened, and what the fix is

34 cells were ready. The run wrote the output, checked it, found 7 formulas that would read an
error and deleted it. Every one traced to one row: UNKNOWN written into Tree List - Proposed row
85 with a name and a count and no diameter, because DM-25 row 19 prints nothing for its height
and 0 for its canopy diameter. `PrintedMeasure.Held` already means a number greater than nought,
so a nought was never a size, and the row went in without one.

**The guard is right and the rule it caught was wrong.** Writing a name and a count into an
empty row cannot work for a species the model does not size. `SpeciesMatching.WrittenInto` now
asks the height and the canopy diameter before it takes a row, and a species missing either
gets no row, is named with its count and its reason, and the run goes through. The canopy guard
is untouched, byte for byte: it caught this, which is why it exists.

The reason names what is missing and what every row printed, so a person can see it without
opening the model:

```
the workbook's list does not hold this name, and a row written into an empty one carries only
what the model prints, which is no height and no canopy diameter a workbook can compute with,
so no row was written: DM-25 row 19 prints '-', DM-25 row 19 prints 0, which is no size
```

Three decisions inside it, none of them settled by the code:

- **The size is asked before a row is taken**, so a refused species leaves the empty row for
  the next one. A test proves it, and it only proves it because the merge orders by name and
  the unsized species is reached first. The first version of that test put the unsized species
  second, so the broken code passed it. **It went green on a watch that should have turned it
  red**, which is how it was caught
- **The sheet's own total is asked first.** A sheet with no total writes nothing for anybody
  and that is the larger fact, so it is still said first for a species that is also unsized
- **Both measures are required**, which is the round message's wording. Only the diameter is
  known to break a formula: `L` reads `J` through `ISBLANK` and nothing measured says a row with
  a diameter and no height is safe. Requiring both withholds a row from a species the model
  half sizes, its count is named in the report, and loosening it to the diameter alone is
  Bader's call

### One line added to the report

Under SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT:

```
  NOT WRITTEN, THE WHOLE RUN: 1 tree of 528, over every species this run merged.
```

Both numbers come off the one list of matches the section above prints from, so nothing counts
the trees a second way. It covers the whole run rather than that section, because a tree that
went nowhere for any reason is a tree the workbook does not hold, and the line says so.

### What the 12 red tests turned out to be

The rule turned 12 existing tests red, which is what a rule change should do. Four readers, one
per file, said which were fixtures and which were expectations. Two kinds:

- **Nine were fixtures that predate measures.** They build a species with no height and no
  diameter because they are about matching a name, taking rows in order, or room on a sheet.
  `CreateFixture.Species` gives the ordinary case a size now, 15 and 8, and `CreateFixture.Merged`
  builds a sized species off a row rather than off a plot number
- **Three were the old rule written down**, all about UNKNOWN, and their expectations changed.
  `PhoenixWritesEighteenAndFifteenAndUnknownWritesNeitherAndStillRefuses` asserted the workbook
  being deleted by the guard. It asserts the opposite now and is the round in one test:
  PHOENIX goes in whole, UNKNOWN takes no row, and **the workbook is written**

**The blanket fixture change was itself a fault, and the review caught it.** Giving every three
argument fixture species a size made UNKNOWN carry 15 and 8, and UNKNOWN is the one species
measured to print neither. Every test that names UNKNOWN now says what the model says, a dash
and a 0, and the tests that only needed a species the list does not hold use one that is
measured to have a size. Where UNKNOWN was left in a test about room, its reason changed from
the sheet ran out of room to the model does not size it. Both are true on that sheet and the
size one is said first, which is what makes the two UNKNOWN rows of the 1836 run read the same
way afterwards.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rerun green at 1187.

- the size never asked, so an unsized species takes a row: **10 red**, across
  `SpeciesMatchingTests`, `CanopyColumnsTests`, `TreeListRowsTests` and `DiameterColumnTests`
- the report not saying how many trees went nowhere: **1 red**, the canopy report test
- the refused species taking its row before the check: **1 red**, the ordering test, and only
  after that test was corrected. It passed the same watch before, which is what showed it was
  not testing what it said

### Existing tests changed

`CreateFixture.Species` gained a size and `CreateFixture.Merged` is new. `SpeciesMatchingTests`
and `TreeListRowsTests` moved to them, three tests were renamed for what they now assert, and
`DiameterColumnTests` and `CanopyColumnsTests` each had one expectation rewritten. 5 tests added
net, 1182 to 1187.

### Open, and for Bader

- **Whether both measures should be required or the diameter alone.** Only the diameter is known
  to break a formula. A species the model gives a diameter and no height is withheld today
- **What a withheld count costs.** A tree not written is a tree the workbook's totals do not
  hold. The report says how many, and the answer to the species themselves is in the model

---

## 2026-09-11, fifty second pass. A workbook is filled when a cell the tool writes holds what the tool writes

One correction, on finding 39's filled file test, measured by Bader on two template sets. The
audit entry for 39 carries it under its FIXED mark. **The other 38 findings stay open**, not
renumbered, not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `d370add`.

Pull request 73, merged into main as `ba805b1`. **The runner executed 1182 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1182 ran at `ba805b1`, 0 failed and 0
skipped**, 667 of them KPI, 29 added here, against the 1153 main carried at `d370add`. The
merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.
Its tree is the branch's tree, checked.

### What was measured, and what the old rule really did

Two sets, Bader's measurement:

```
KPI CHECKLIST - R1 MOSQUES   E5 <Date>   G5 <Name>   H5 <Position>   C5 <UID>
an earlier production set    E5 empty    G5 empty    C5 empty
                             D3 Future Park   E4 KING ABDULLAH South   H5 a real value
```

So E5 is a placeholder in one set and empty in the other, a placeholder is one set's habit
rather than a rule, and a clean template holds real text in a mapped cell.

**One thing in the round message does not hold against the code, and it is worth saying.** A
clean template from the second set was NOT withheld by the old rule: `ReadsAsFilled` required
the cell to be non blank before it compared, so an empty E5 read as a template. The old rule
was still wrong, for the reason given rather than that consequence. What it really withheld was
any template whose E5 holds text that is not exactly `<Date>`, which is the annotated set's
DATE OF THE DAY and any set the client issues with different wording there. The correction is
the same either way and is the one asked for.

### The rule now

`FilledMarks` is a cell the tool writes, what the tool writes there, and the test of whether
what the cell holds is that. A workbook is filled when one of them reads, and **nothing is
withheld on a cell the tool has never written**, so an absent or empty cell is never a filled
file. Two marks, both cells the tool writes:

- **E5, a date.** It parses as one AND holds at least two numbers. The second half is not
  decoration: the invariant culture reads a bare 10 as a day of this month, and a template
  holding a lone number where a date goes would have been withheld for it. `<Date>`, Date,
  DATE OF THE DAY, March, TBC and empty are all templates. 2026-09-10, 09/09/2026,
  9 September 2026 and 2026-09-09 14:07 are all filled
- **The template's own reference cell, a plot reference.** C5 on all seven, read off
  `KpiTemplate.CellFor` rather than a second copy of the map. It is one unbroken run holding a
  letter and a digit and no angle bracket, which is what both parameters the team picks read
  as, DM-12 and ANH-007-MO-100019. `<UID>`, KING ABDULLAH South, Future Park, TBC, 100019,
  ANH 007 MO 100019 and empty are all templates

The other cells the tool writes decide nothing. D3 and E4 hold real text in a clean template,
G5 and H5 hold a name and a position that no shape tells from a placeholder, and a number cell
says nothing about who put the number there.

**The cell that decided is said in the reason and in the report.** The reason reads It is a
filled MOSQUES checklist, not a template. C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes. The report prints one line per withheld workbook under the templates folder
line, not offered: MOSQUES DM-12.xlsx, C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes, so a template wrongly withheld is traced in one line rather than by opening
the file. The date is read first, so a workbook holding both names one cell rather than two.

**`KpiTemplates.DatePlaceholder` is deleted.** It decided nothing any more, and a constant
holding a measurement that no longer decides is a second record of a fact. The measurement is
in `FilledMarks` next to the rule it explains, with both sets.

**The peek reads the marks' cells rather than one.** `PeekedWorkbook.FirstSheetCells` is a
dictionary of the cells `FilledMarks.CellsRead` names, E5 and C5, read off the first sheet in
one open with one pass over the shared strings. A cell not in the file is not in the dictionary.

### Two limits the review of this round found, both stated and neither guarded against

**A filled workbook the tool wrote neither cell into reads as a template.** `KpiCreatePlan`
skips the reference cell when the ticked plots disagree on it or none of them holds it, and a
checklist covering several plots usually disagrees, so on such a run the typed date is the only
mark left. `CannotCreate` does not ask for a date, and the box is prefilled with today, so it
takes clearing it by hand. It is the safe way round of the two: offering a filled file costs a
rerun, and withholding a real template leaves the team unable to fill anything at all.

**A client set that hinted the shape of a reference rather than bracketing it would be
withheld.** DM-00 or REF-0001 at C5 holds a letter and a digit and no space, so nothing
separates it from ANH-007-MO-100019. Neither measured set does that: the R1 set brackets every
placeholder and the earlier set leaves the cell empty. A theory in `FilledWorkbookTests` says
what the rule does with one today rather than that it is right, and the report prints C5 holds
DM-00, so a person sees it in one line rather than by opening the file. **For Bader**, with the
other open questions.

### Three things the review changed

**The sheet part is parsed once per file rather than once per mark.** The reader reopened the
zip entry and parsed the whole sheet again for each cell, so going from one mark to two doubled
it and every mark added later would have cost another pass. `WorkbookPackage.CellTexts` walks
the part once and stops when it has them all.

**The pane's line about the three typed cells said never written**, in a list whose other lines
name where a value is read from, and the tool does write them, which is the very thing
`FilledMarks.Date` relies on to tell a filled file. It reads typed by the team on this pane and
copied through, from no model. Its test moved with it.

**A theory pins what a shape-alike hint does today**, so the second limit above is in a test
rather than only in prose.

### Break watches

Five, each restored byte for byte and checked with cmp, the suite rerun green at 1182.

- the date mark back to anything but the placeholder: **5 red**, every row of the theory that
  is not a date, DATE OF THE DAY and the bare 10 among them
- the reference mark taking any text: **8 red**, every row of the reference theory, the R1 set
  and the park that still needs a pick
- the report not naming the cell that decided: **1 red**, the report test
- the peek reading the date cell only: **1 red**, the patcher round trip through both cells
- the one pass reader stopping at the first cell it finds: **1 red**, the same round trip

### Existing tests changed

`FilledWorkbookTests` is rewritten around the new rule, 9 tests before and 20 now, both
measured sets among them. `TemplateWordsTests` moved one expected line with the pane text above.
Nothing else moved: `Recognise`'s fourth argument is still optional, so every earlier call
proves the sheet rule unchanged.

---

## 2026-09-11, fifty first pass. The five that cost a whole run: findings 35, 34, 39, 9 and 16

Five findings from the two audits, all about the 78 plot STREETS run the team is about to try,
about twenty minutes on the measured rate. Each either wasted it, threw it away, or made it
worse. **Findings 34, 35, 39, 9 and 16 are FIXED and marked under their entries in
`steps/audit-kpi.md` and `steps/audit-kpi-2.md`. The other 38 stay open**, not renumbered,
not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `2e0c0ab`.

Pull request 72, merged into main as `adf164f`. **The runner executed 1153 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1153 ran at `adf164f`, 0 failed and 0
skipped**, 638 of them KPI, 50 added here, against the 1103 main carried at `2e0c0ab`. The
merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.
Its tree is the branch's tree plus the two Drawing Sheet commits that landed on main between
the branch point and the merge, `fc5cf7a` and `2aa529b`, checked: the only files that differ
between the branch and main are the three of theirs, and no KPI commit landed.

### Each of the five at today's lines

The audits' line numbers were written at `92dd36c` and have moved. Read again at today's lines
before anything was fixed, every one of the five still stood:

- **35** stood at `KpiRequestHandler.cs:267-291`, the loop over the ticked plots with
  `RegionsFor` at :270 and `Read` at :288 under no try of their own, falling to the catches at
  :160-179. The two reconciliation refusals at `Reconciliation.cs:212-232` were reached by
  `ReconciliationTests` alone
- **34** stood at `KpiPanel.cs:760-764`, the region choice click calling `AskedToCreate`, and
  :706, the confirm redrawing so Create is pressed again. The choice was applied at read time,
  `KpiRequestHandler.cs:277-290`, and nothing of the last run travelled on the ask
- **39** stood at `KpiPanel.cs:353-360` with `RecognisedWorkbook.cs:102-116`, the first sheet's
  name and nothing else, and the peek read no cell
- **9** stood at `KpiPanel.cs:562-569`, a bare `ScrollViewer` built on every redraw, with no
  scroll handling anywhere on the KPI side
- **16** stood at `KpiPanel.cs:353-360`, `PeekedWorkbook.Of` per path on every redraw, byte for
  byte what `92dd36c` held

**The round before this one, findings 3, 4, 6, 12, 21 and 26, has not landed.** Main held
nothing after `2e0c0ab` and no Kpi commit names any of the six, so finding 35 is built here
and not on finding 6. The catch inside `KpiPlotReader.Printed` that swallows an
`ApplicationException` with nothing recorded is finding 6's and is untouched: a throw of that
one type inside `Printed` still comes back as a schedule with no rows and no refusal, and that
round is where it is answered.

### 35. A guard per plot, and a guard per schedule

`PlotReading.NotRead(plotId, why, seconds)` is a reading whose one refusal is the read threw
and nothing on this plot was read, with the exception's type and message, every other field at
its empty default. `GuardedRead` in `KpiRequestHandler` wraps each plot's regions and read and
hands that back on a throw, catching every type on purpose with the same sentence the scan
side's `Guarded` carries. Inside `KpiPlotReader.Read` each schedule's read is guarded the same
way, so a throw reading one schedule names the schedule and leaves the rest of the plot read.
Nothing downstream changed: `Reconciliation` already turns the refusal into DM-60: the read
threw and nothing on this plot was read. InvalidOperationException: The schedule is not valid.
and refuses the write, lists the plot under contributed nothing with the refusal first, the
report prints it among the reasons and under the plot as REFUSED, the status line counts it,
and the pane draws it in red. Three ticked, three read, one refused: the other two still add
up around it, 1000 plus 250 is 1250 and 13 plus 13 is 26, and the plot's empty component does
not make the others disagree. The refusals for a plot list that comes out shorter or longer
than it went in stay as the backstop tests reach, by design now.

The guard itself runs only in Revit and has never fired on a live model. Which exception type
a real refused plot throws is UNKNOWN, so the message shape is the only thing the tests pin.

### 34. The choice applied to the run already read

`KpiCreateAsk` carries the run the pane holds, the same object and not a copy, and
`HeldReadings.Decide` in Core says whether it can answer this press. It is not a cache with a
lifetime: the run before is trusted when it wrote nothing and the model title, the template and
its file, the two parameters and the plots are the same, ordered the way `Reconciliation`
orders them, and every ticked plot has a reading. Anything else reads the model again with the
reason named, and each is a test: no run held, the run before wrote its workbook, the model or
the template or the template file or either parameter or the plots differ, a plot has no
reading. `HeldReadings.Applied` puts each plot's chosen region on its reading through
`PlotReading.WithChosenRegion`, and a plot with no choice keeps the reading it had, the single
region a first run settled on included. The reconciliation that refused NS-19 on two regions
passes on the applied readings with the area 900 and nothing read, and the identical areas
confirm passes the same way with the pair still named.

The report says which under the Run line: Readings: reused, nothing was read from the model on
this press, or read from the model on this press, with the reason. A reused run reads 0.0
seconds of model read, and that line is what makes the number true. The confirm button still
redraws and Create is pressed again, which now reuses, so the two presses cost one read where
they cost two. Making the confirm one press is an interface change Bader has not asked for.

**A plot whose read was refused is not reused.** The review of this round found that a reading
made by `NotRead` carries the plot's identifier and so passed the every plot has a reading
check, and a press after the model was fixed, with nothing else changed, would have handed the
same refusal back for ever. `Decide` reads again when any held reading carries a read refusal,
naming the plots: the run before could not read DM-60, so the model was read again. That costs
a full read on the press after a thrown plot, and reading only the refused plots is the
refinement not built.

**What no cheap comparison can see is a model edited between the refusal and the pick.** A
region's area typed in or a schedule's filter changed in those minutes would be read off the
run before. The pane throws the run away on every tick and every picker change, the report
says reused in so many words, and whether that is acceptable is for Bader.

### 39. A filled checklist named as filled

The peek reads E5 of the first sheet in the same open as the names, off the part the workbook's
relationships point at, with a shared string resolved to its text because an Excel re-save
stores a filled cell that way. `Recognise` takes it as a fourth argument and names a file whose
E5 is not `KpiTemplates.DatePlaceholder` as a filled checklist of its template, with what E5
holds in the reason: It is a filled MOSQUES checklist, not a template. E5 holds 2026-09-10 where
a template holds <Date>. It sits in the same list greyed, its reason in the tooltip, and
nothing is deleted or moved. A cell that is not there or empty is not a filled file. A filled
park file is not offered and needs no pick. A read refusal still wins. The two private cell
readers in `SpeciesList` moved into the package plumbing the peek and the patcher share, so
there is one cell reader rather than three.

A filled park file whose name names neither park, or both, is named as a filled EXISTING PARKS
or FUTURE PARKS checklist which its file name cannot tell apart, `FilledPark`, with `FilledAs`
null. The first draft named EXISTING PARKS for it, the first of the two in the list, which was
a guess printed as a fact, and the review caught it. Nothing reads `FilledAs` but the tests.

**The placeholder is one observation.** `<Date>` is what E5 held on the first real workbook on
2026-09-09, recorded until now only in a comment. Whether all seven production templates hold
exactly that is UNKNOWN and is for Bader to open and confirm before the team's folder holds a
filled file. A template holding something else there would be withheld, with its E5 printed on
its own row so the withholding is visible rather than a file that vanished. The annotated set
reads DATE OF THE DAY there and is not what the team fills.

### 9 and 16. One press, one design

Both are the same press: a tick calls `Ticked`, `Changed`, `RedrawTemplates`, and the redraw
both reopened the workbooks and rebuilt the plot list bare. `Changed` and `RedrawTemplates`
stay the one path in and out of every change.

`TemplateListing` in Core holds the folder's workbooks as recognised, keyed on the folder
compared without case and with a trailing separator off, and `For(folder, paths, recognise)`
opens only the paths it has not seen, drops the ones gone and keeps the rest. The folder is
still listed on every draw. It counts the opens and the redraws: seven files over 155 draws is
seven opens where it was 1,085, and a fake counts them in the tests so no disk is touched. The
pane holds it as the one copy under THE PANE HOLDS NO COPY OF ANYTHING IT CAN ASK FOR, says so
in its comment, clears it when the folder is browsed and after a run that wrote, prints the
count under the list, and hands it on the ask so the report prints templates folder: 7
workbooks in the folder, opened 7 times over 158 redraws since the folder was listed, or NOT
LISTED when nothing recorded it. A workbook overwritten in place under the same name by
anything other than this tool keeps its held recognition until the folder is browsed again or
Create reaches the patcher into it, which is the stated limit. The clear after a press of Create
is keyed on the output path's folder, `IsFor`, so filling several plots into another folder
leaves the listing and its counts standing, and it fires on any press that reached the patcher
into the listed folder, wrote or not: the copy lands at the output path before the patch, and a
patch that fails after it leaves the copy behind under a name the listing may already hold as
something else. The review found both, the clear on every write anywhere and the clear missing
on a refused patch. It also asked whether a write refused after its copy leaves a stray file
unseen under a new name: it does not, because the folder is listed on every draw and a new path
is opened once.

`Scrolling` and `Remembering` in `KpiPanel` are the Drawing Sheet's shape written in the KPI
folder, read and not edited at `DrawingSheetPanel.cs:1653-1702`: the remembered offsets read
into locals before any handler is attached, restored on the first layout pass and noted on
every scroll change. `ScrollMemory` in Core holds the rule half with tests, a restore wanted
only when something above the top was noted, because restoring nought is a scroll to the top
that reads as though it worked. A tick still redraws the whole block, because a tick moves the
count line, the preselection, the reference values and the Create button, and remembering the
offset is the minimal change and the one the Drawing Sheet chose. Whether the restore holds on
a real run is UNKNOWN for both panes: the Drawing Sheet's own log records no observation of it
either. **Lifting the helper into a shared file is a round of its own**: it cannot live in
Core, which has no WPF, so it would be a new common file at the Revit root and a hook change,
and that is Bader's call.

### What inside the read costs, in calls, and unchanged

Measured on the code and not in seconds, because nothing here runs Revit. Per ticked plot:

- `FirstSheetOf` collects every sheet, reads `SheetNumber` off all 1,385 to sort them, then
  `GetParameters` and the getter down the sorted list until the plot matches
- `Schedules` collects every schedule reading `IsTemplate` off each, and for every one of the
  951 that is not a template `PlotFilteredOn` asks `Definition`, `GetFilters`, and per filter
  `GetField`, `GetName`, `IsStringValue` and `GetStringValue` until the plot filter is found.
  951 per plot, 74,178 over 78
- `RegionsFor`, on a template with an area cell only, so not on STREETS: one collector over the
  link instances, and per 00 link one collector over its filled regions with `GetParameters`
  and the getter for PRX_Ref Plot ID on every one of the 279, and the area, the type name and
  the printed form on the regions matching the plot alone. The forty sixth pass entry said two
  parameters off each region, which overstated it
- `Printed`, on at most two schedules: `GetTableData`, the body section, the counts, and
  `GetCellText` once per cell. Nothing regenerates, refreshes or exports a schedule

Which of the four carries the 125.5 seconds is UNKNOWN, and none is changed. The 126.6 second
measure in the round message is Bader's and is recorded nowhere in this repository.

### What the review found and what stayed

Four readers over the diff, one lens each, then a pass trying to refute what they found,
eleven findings, four confirmed and seven refuted. Three changed code, above: the refused
reading reused, the park named on a guess, and the templates listing cleared on every write
anywhere and not on a refused patch into its folder. The rest stayed, each for a reason:

- The region read sits inside the plot's guard rather than under one of its own, so a throw
  reading a plot's filled regions refuses the whole plot, named, and its schedules are not read.
  Finding 35 asked for a guard per plot and that is what it got. STREETS reads no region
- A template whose own placeholder is not `<Date>` is named as filled, with its E5 printed. That
  is the open question for Bader above, and the line shows what it read rather than hiding it
- `CellRef.TryParse` reads upper case column letters only, which is what Excel writes. A sheet
  re-saved with lower case references would read as empty rather than refuse. Not this round's
  code and not touched
- `PeekedWorkbook.Of` catches four types, and an unlisted one thrown inside a redraw would take
  the template list down with it. The same four it caught before this round. Not touched
- A templates folder that fails to list reads as empty, `WorkbooksIn`'s shape before this round,
  and now costs one re-open per file when it comes back rather than seven per tick. Not touched

### Break watches

Eight, each restored byte for byte and checked with cmp, the suite rerun green at 1153.

- 35, a plot that threw carrying no refusal: **4 red**, all of `PlotReadThrewTests` bar the
  merge test
- 34, a choice not applied to the held reading: **2 red**, the applied and the reconciliation
  tests in `HeldReadingsTests`
- 34, the plots ticked not compared: **2 red**, the two ticked rows of the theory
- 39, nothing reading as filled: **2 red**, the filled mosque and the filled park
- 16, every draw opening every workbook again: **6 red**, across `TemplateListingTests`, the
  155 draws test reading 1,085
- 9, nought restored: **2 red**, both `ScrollMemoryTests` that say nought is not worth restoring
- 34, a refused reading reused: **2 red**, the thrown plot and the two schedule refusals in
  `HeldReadingsTests`
- 39, an untellable filled park guessed as EXISTING PARKS: **2 red**, both rows of the theory
  in `FilledWorkbookTests`

### Existing tests changed

None moved. `CreateFixture.Run` gained three optional arguments, the readings source, the
templates listing and the ticked list. `WorkbookFixture` gained a workbook whose one cell is a
shared string. `Recognise` gained an optional fourth argument, so every earlier call proves the
sheet rule unchanged.

---

## 2026-09-10, fiftieth pass. One word too loose: two headings hold DIAMETER, and the formulas say which

One column choice, measured on the 1707 run, and one line added to the report. **The other 43
audit findings stay open**, not renumbered, not reordered, not annotated. Nothing else was
touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round
has been observed in Revit**, and no workbook was opened in Excel.

Pull request 69, merged into main as `9e966fa`. **The runner executed 1103 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1103 ran at `9e966fa`, 0 failed and 0
skipped**, 588 of them KPI, 8 added here. The merge went through the API with the title and the
message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### What happened on the 1707 run

The run wrote the output, checked it, found 8 formulas that would read an error and deleted
it. Correct at every step. The report said: not written: the sheet's header row, row 3, names
2 columns holding DIAMETER, J, K, so nothing says which. The tree list header row holds I
Mature Height (m), J Average Mature Canopy Diameter (m) and K Mature Canopy Diameter (m). Both
J and K hold DIAMETER, so the reader found two, refused to choose and wrote nothing into J.
L84 is IF(ISBLANK(J84), " ", ROUND(PI()*(J84/2)^2, 0)), so it returned a space, M84 did
arithmetic on the space and went #VALUE!, and the canopy guard deleted the output.

### The fix: prefer the column the workbook's own formulas read

J is the column the workbook computes from. Measured: L reads J and nothing reads K.
`SpeciesList.In` reads every formula below the header row off the sheet part, the master cell
of a shared formula carrying the text, collects the columns those formulas reference off the
formula text with a cell reference pattern that leaves a function name such as LOG10 alone, and
hands the set to `ColumnHeaded`. One heading holding the word is taken as before. More than
one, and the one candidate the formulas read is taken. Where that still leaves more than one,
or none, nothing is written and both are named, with what the formulas read added to the
reason: names 2 columns holding DIAMETER, J, K, and its own formulas read none of them, so
nothing says which. Never a position. It is worked out from the file and from no letter.

How each column was chosen is recorded as it is used, `HeightColumnChosen` and
`DiameterColumnChosen` on the list, the one column of the header row holding HEIGHT, or of J, K
holding DIAMETER, the one the sheet's own formulas read, and the report prints both beside each
tree list under THE WORKBOOK'S OWN TREE LISTS, or none with the reason.

**The check asked for.** On the two column sheet PHOENIX DACTYLIFERA writes 18 into I and 15
into J, on the fixture's row 5 where the real sheet's is row 84, and UNKNOWN writes neither,
because DM-25 row 19 prints nothing for its height and 0 for its diameter. UNKNOWN's blank J
still refuses the output through the canopy guard, L6 named as reading a blank J6, and the row
Phoenix landed on computes. The two column header, a one column header, a header naming none,
the formulas reading neither and the formulas reading both are each a test, every expected
value written out by hand.

### Said in the report and not fixed

**22 matched species have a height or a diameter in Revit that differs from the row the
workbook already holds, nearly every match.** Both numbers stay named and nothing is changed,
and one line under that heading now says how many of the matches differ in a height, a
diameter or both, so the size of it is visible without counting: 2 of 3 matched species on the
fixture, 0 of 0 with no match. Which number is right is still the question for the team the
forty seventh pass raised.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rerun green at 1103.

- the tie break dropped, so two columns refuse as before: **3 red**, the two column test, the
  Phoenix and UNKNOWN check and the report line in `DiameterColumnTests`
- the first column taken when the formulas do not decide, which is a position: **2 red**, the
  formulas reading neither and the formulas reading both
- the count line dropped from the report: **2 red**, both of `MeasureDifferenceCountTests`

### Existing tests changed

The tree lists report test in `TreeListRowsTests` gained the two column lines, none with the
reason, because its fixture's header names only the botanical column. The `Computing` fixture
takes a second diameter heading, the column the canopy formula reads and a second formula
column, so the three shapes can be built. No expected value moved.

---

## 2026-09-10, after the forty ninth pass. ST-05's Existing group is the thirteen measured species

One fixture correction and nothing else. The forty ninth pass built ST-05's Existing group
from two names and an assumed split of 300 and 69, because the note gave the subtotal alone.
The thirteen species are measured off the schedule on screen now and the fixture holds them:
ACACIA / VACHELLIA FARNESIANA 25, AZADIRACHTA INDICA 7, CASSIA GLAUCA 1, CONOCARPUS ERECTUS 76,
CONOCARPUS LANCIFOLIUS 129, FICUS BENJAMINA 13, HIBISCUS TILIACEUS 4, MORINGA OLEIFERA 3,
PHOENIX DACTYLIFERA 18, PROSOPIS JULIFLORA 16, UNKNOWN 20, WASHINGTONIA ROBUSTA 48 and
ZIZIPHUS SPINA-CHRISTI 9. They add to 369, which is the subtotal the schedule prints, and 369
plus 70 is the 439 of the TOTAL row. **Every number in that fixture is measured rather than
assumed.**

CASSIA GLAUCA sits under Existing at 1 and under Street Design at 62, so the fixture now covers
one species in a named group and a by-decision group at once: two merged rows, 1 on Tree List -
Existing and 62 on Tree List - Proposed, two groups and not one species printed twice under
one, so the same group refusal does not trip. The tests say so in those numbers, and the row
numbers moved with the rows: sixteen species rows, the Existing subtotal on row 17, Street
Design on row 21 with its subtotal on 24, TOTAL on row 25. No code changed, so there is no
watch to break. **Nothing in this round has been observed in Revit.**

Pull request 68, merged into main as `5d062f6`. **The runner executed 1095 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1095 ran at `5d062f6`, 0 failed and 0
skipped**, 580 of them KPI, none added. The merge went through the API with the title and the
message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

---

## 2026-09-10, forty ninth pass. Street Design counts as Proposed on STREETS, and the note goes on the pane

Two things on top of the forty eighth pass, both Bader's decisions. **The other 43 audit
findings stay open**, not renumbered, not reordered, not annotated. Nothing else was touched:
not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round has been
observed in Revit**, and no workbook was written or opened. `CountedGroups` keying off the tree
list sheet names is the design and stays. These sit on top of it.

Pull request 67, merged into main as `c79c4d2`. **The runner executed 1095 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1095 ran at `c79c4d2`, 0 failed and 0
skipped**, 580 of them KPI, 14 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### 1. Street Design counts as Proposed, on the STREETS template only

**Bader's decision: Street Design counts as Proposed on STREETS and stays out everywhere
else.** No template has a Tree List - Street Design sheet, so the forty eighth pass left the
group out on every template, streets included, and on a street plot that group is the plot's
own work. Measured on ST-05, a street plot, off its softscape schedule on screen: Existing 369,
Proposed 2 which is ALBIZIA LEBBECK 2, Street Design 68 which is ALBIZIA LEBBECK 6 and CASSIA
GLAUCA 62, TOTAL 439. Its proposed trees are 2 plus 68, 70, its existing are 369, and 369 plus
70 is 439, the TOTAL the schedule prints. That is the test, and every expected value is
written out by hand from it. The two species under Existing and their split of the 369 were
not in the note, so the fixture uses two names earlier runs measured and a split of 300 and 69.

**It is data on the template and keyed on the template, never on the plot prefix.**
`KpiTemplate.GroupsCountedAsProposed` holds Street Design on STREETS and nothing on the other
six, one constant, `KpiTemplates.StreetDesignGroup`. `CountedGroups.Of` reads it into a
`GroupByDecision` pointing at the template's Proposed sheet, `Counts` answers true for it,
`SheetFor` answers Tree List - Proposed, and `Why` reads Tree List - Proposed takes it on
STREETS by decision, as that plot's own work. A sheet named for the group is answered first and
a sheet that takes it by decision second. The same schedule read for MOSQUES leaves the group
out with 68 named, and a mosque plot read for STREETS counts it: the template decides and the
prefix decides nothing, which a test says in those two readings. A test also says the whole
map holds exactly one group by decision, so a second name cannot slip in unnoticed. **A second
name goes in only when the team says so.**

**The rows go where the sheet takes them, and a species under Proposed and under Street Design
adds.** `KpiMerge.Species` takes the `CountedGroups` now, a required argument, and keys every
row on the sheet that takes its group rather than on the group's name, so ALBIZIA LEBBECK on
ST-05 is one merged row of 8, ST-05 8 (2 rows, 2 + 6), going to Tree List - Proposed and
saying both groups, Proposed and Street Design. `MergedSpecies.SheetName` carries the sheet
the merge decided and `SpeciesMatching` places a species through it, and a species built with
no sheet through the same `CountedGroups`, so the matcher's own word rule on the sheet name is
gone and one resolver decides everywhere. Two rows under two different groups are two groups,
so the same group refusal does not trip, and the accounting passes with nothing left out. The
street's areas count the same way: the shrubs and lawn reader already asked `CountedGroups`
per phase, so a Street Design phase row is taken on STREETS with the group total still checked.

A first cut carried the sheet on every `SpeciesRow` from the reader and keyed the merge on it
where a row had one and on the group name where it did not. That broke one test that merges a
reader built row with a hand built one for the same species, and it was two sources for one
fact, so the merge resolves every row itself instead and the row carries nothing new.

**The report says which route each group took.** The reason beside every group row is one of
three: named for it, takes it by decision, or out of scope. A Street Design group counted on
STREETS reads differently from one left out on MOSQUES, and the accounting line reads 0 on
STREETS and 2, on ST-05 when the same plot is read for MOSQUES.

### 2. The note goes on the pane, not only in the report

`CreateWords.GroupsLeftOut` builds one short block from the run's readings and its template:
Street Design found on 2 plots on MOSQUES, which has no sheet for it: DM-16 in its softscape
and its shrubs and lawn schedules, FM-05 in both. Those rows were left out. Fix them in the
model. The first plot spells both schedules out and the next says in both, a plot in one
schedule names that schedule, plots are in natural order, a plot with nothing left out is not
named, two group names are both named, and nothing left out is no note. It names the template
rather than saying not streets, so a group left out on any template reads right. **It is a NOTE
and NOT A REFUSAL**: the workbook is written, the numbers are right, and the note says where the
model needs correcting. Plots and schedules, never species.

`KpiPanel.TheCreateButton` draws it in the warning colour above the Create button off the last
run, the same place the accounting's refusals are drawn, and only when the run held a template.
The report is unchanged and keeps the counts and the areas left out under each plot. The
status line is unchanged too.

### Break watches

Four, each restored byte for byte and checked with cmp, the suite rerun green at 1095.

- the decision ignored, so STREETS counts nothing by decision: **10 red**, nine in
  `StreetDesignTests` and the template test in `GroupRowsTests`
- the decision applied on every template: **21 red**, every FM-05 test that expects the street
  left out on MOSQUES, across `GroupRowsTests`, `SubtotalShapeTests`, `SchedulesAsPrintedTests`
  and `StreetDesignTests`
- the merge keyed on the group name again: **3 red**, the ALBIZIA LEBBECK 8, the matcher and
  the template tests in `StreetDesignTests`
- the note dropped: **4 red**, all of `GroupsLeftOutNoteTests` that expect words

### Existing tests changed

Nineteen calls of `KpiMerge.Species` hand it the fixture's counted groups, because the
argument is required. The template test in `GroupRowsTests` says only STREETS counts more than
its two sheets. No expected value moved.

---

## 2026-09-10, forty eighth pass. The FM-05 refusal answered: a third group, out of scope by decision

The refusal the forty seventh pass raised on FM-05 offered two answers, two types of one
species or one species counted twice, and the answer is neither. The section that prints every
schedule as the schedule prints it, added that pass, showed FM-05's softscape schedule holding
THREE groups: Existing at row 3 with four species adding to 6, Proposed at row 9 with ALBIZIA
LEBBECK 10, BAUHINIA PURPUREA 19 and CASSIA GLAUCA 3 adding to 32, and STREET DESIGN at row 14
with ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4 and CONOCARPUS 4 adding to 38,
then TOTAL 76 at row 20. Its shrubs and lawn schedule prints the same third phase: GRASS
Proposed 96 over 117 and Street Design 69 over 84, total 165 over 201, SHRUBS AND GROUND COVER
Proposed 361 over 450 and Street Design 459 over 570, total 820 over 1020. Every one of those
numbers is off the 1536 report and none is reasoned. The FM-05 10, FM-05 10 that two rounds
chased was one row under Proposed and one under Street Design.

**Bader has decided that Street Design is somebody else's scope and does not belong on this
plot's checklist. The model will be corrected later. Until it is, the tool leaves those rows
out and says so.** That is a decision and not a measurement, and it is recorded here as one.
**The other 43 audit findings stay open**, not renumbered, not reordered, not annotated.
Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing
in this round has been observed in Revit**, and no workbook was written or opened. Every
expected value is written out by hand off the numbers above.

Pull request 66, merged into main as `ee13e8c`. **The runner executed 1081 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1081 ran at `ee13e8c`, 0 failed and 0
skipped**, 566 of them KPI, 21 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked. The
branch sat on `bb97dc3`, after the user's Shared round moved `PaneLabel` and its tests out of
the KPI folder, which is why the KPI count reads 566 and not the 557 plus 21 the last entry
would give: the bare base measures 1060 and 545.

### 1. Only the groups a tree list sheet is named for count, and the arithmetic is checked

The words Existing and Proposed appear nowhere in the code that decides this. `CountedGroups`
holds the two tree list sheet names off the template, Tree List - Existing and Tree List -
Proposed, and a group counts when a sheet's name ends in the group's name, word for word and
without case. Nothing looser: Tree and List are words of both sheet names, TREES is the heading
over the groups, and none of those is what either sheet is for. The reader that used to be
handed the document's phase list is handed this instead, and `KpiRequestHandler` no longer
reads the phases for the create path at all. A group the workbook has no sheet for is out of
scope by construction, on every template, because all seven name their sheets the same way.

`SoftscapeRows.Read` finds every group row, a text only row followed by anything but another
text only row, reads the species rows under each, and takes that group's own subtotal, the
first count with no name under it. Each group comes back as a `PrintedGroup` with its row, its
species, its subtotal row, whether it was taken and why, and the reading's `Species` holds the
taken groups' rows in printed order. Two checks, both refusals in `Reconciliation`: each group's
species rows against its own subtotal row, and the groups taken plus the groups left out against
the TOTAL row. FM-05: 6 plus 32 taken, 38 left out, TOTAL 76. A TOTAL of 80 refuses naming all
three numbers.

`ShrubsAndLawnRows.Read` stops taking the group total. A phase row's subtotal is that phase's,
the phases a sheet is named for are added together, area and item count, and the group total
row is the check on every phase, taken or not. FM-05 GRASS 96 taken, 69 left out, 165 printed.
SHRUBS 361 taken, 459 left out, 820 printed. A group with no phase row at all keeps the rule it
had, the last row is the value and the rows above it must add to it, because such a group has
nothing else to offer. A group whose every phase is out of scope is nought and says so.

**FM-05 reads 6 existing and 32 proposed trees, ALBIZIA LEBBECK 10 and not 20, grass 96 and
shrubs 361**, and its accounting passes. The rule before this one refused the plot, and the one
before that wrote 165 and 820.

### 2. The report names every group row, always

Under the plot, per schedule, every group row in printed order with its row number, how many
species rows it holds and what they add to, its subtotal row and what that prints, TAKEN or LEFT
OUT, and why. Existing and Proposed get a line each, so a schedule with the ordinary two reads
differently from one nobody looked at. The rows left out are listed by name and count under
their group. The shrubs and lawn block prints each phase row the same way and then the group
total row with whether the phases add to it. The accounting at the top gained one line,
schedules holding a group no tree list sheet is named for, with the count and the plots, which
would have shown the street on the first twenty plot run rather than the fourth. The printed
section's summary line names the group rows, the rows read, the rows left out, the subtotals
passed over and the TOTAL row by number, and for the shrubs and lawn schedule which phase row
was taken and which left out and that the group total was checked.

### 3. Two rows for one species in different groups is not a refusal

Under Proposed and under Street Design it is two groups, and the street's row is not in the
reading's species at all, so nothing refuses and ALBIZIA LEBBECK is 10. Under one group it is
the refusal it was, rows and counts named. The words in the rules file that said FM-05 printed
one species twice under one group are corrected: it did not.

### 4. A schedule can repeat a group name, and the report says which is which

DM-25 prints Existing, then Proposed, then Existing again. Nothing guesses which is meant. Both
are group rows in the list with their own row numbers and subtotals, both are named for by the
same sheet, both are taken, and the second's reason says it is the 2nd group row so named on
this schedule, taken as well. `SpeciesRow.GroupRowNumber` carries the group row a species sat
under, so a species under both Existing groups is refused as a species under one group name
twice, and the refusal names both group rows, rows 3 and 9, so a person can see it is two
groups and not one printing twice. Whether DM-25's two Existing groups are one phase printed
twice or two things is UNKNOWN and is for the team.

### 5. The false comment

`KpiPlotReader` said the one schedule guard exists because FM-05 holds two whose names hold
SOFTSCAPE and reading both counted its trees twice. FM-05 holds one softscape schedule. The
comment says so now, says the double was the third group, and says the guard stands for the case
it was built for and that no plot has been measured holding two.

### One shape nobody has measured

A softscape schedule printing TREES and then species rows with no phase row at all would read
TREES as a group row, because a text row followed by species rows is a group row, and TREES
counts for nothing, so every species would be left out and named. That plot would then write
no trees and its report would say why in the group row list and the accounting line. No such
schedule has been seen. Whether one exists is UNKNOWN.

### Break watches

Six, each restored byte for byte and checked with cmp, the suite rerun green at 1081.

- every group counting, which is the rule before this one: **18 red**, across `GroupRowsTests`,
  `SubtotalShapeTests` and `SchedulesAsPrintedTests`
- the shrubs value being the group total again: **9 red**, the four FM-05 and phase tests in
  `SubtotalShapeTests`, three in `GroupRowsTests` and two in `SchedulesAsPrintedTests`
- the group rows lines dropped from the report: **3 red**,
  `TheReportNamesEveryGroupRowUnderThePlot`, `TheReportSaysItUnderThePlot` and
  `TheReportNamesTheScheduleEachNumberCameOffPerPlot`
- the TOTAL check forgetting the rows left out: **2 red**, both in `GroupRowsTests`
- the accounting line dropped: **1 red**, `TheAccountingLineCountsTheSchedulesAndNamesThePlots`
- a repeated group name taken once: **2 red**, the two DM-25 tests

### Existing tests rewritten

The three FM-05 tests that expected 165 and 820 expect 96 and 361 now with the street left out,
and their fixtures name the phases the 1536 report printed, Proposed and Street Design, where
they said Existing and Proposed. The three phase test expects 30 over 11 with Demolished left
out. The printed section test expects the group rows and the rows left out in its summary
line. Two report tests gained the group rows lines under the plot, one of them handing its
hand built reading a printed group so the line reads as a real one would. A species row above
every group row still comes back first and with no group.

---

## 2026-09-10, forty seventh pass. Six things measured on the 1428 run and the workbook opened in Excel

Six things, all measured on the 20 plot MOSQUES run of 2026-09-10 at 14:28, on the workbook it
wrote and on that workbook opened in Excel. Five are fixed and the sixth is reported and not
fixed, as asked. **The other 43 audit findings stay open**, not renumbered, not reordered, not
annotated. Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`.
**Nothing in this round has been observed in Revit, and nothing in it has been observed in
Excel.** The workbook and the model were not handed over and were not needed: every number
below is the one stated for the run, and the test workbooks are built to those shapes with made
up names.

Pull request 60, merged into main as `8eb289b`. **The runner executed 1046 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1046 ran at `8eb289b`, 0 failed and 0
skipped**, 557 of them KPI, 32 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### 1. A written row broke the canopy maths, and the schedule had the fix

The three species written into Proposed rows 84, 85 and 86 carried a name and a count and
nothing else, L84 read `IF(ISBLANK(J84), " ", ROUND(PI()*(J84/2)^2, 0))` and returned a space,
M84 multiplied that space by the count, and #VALUE! ran through M93, F8, D8, E31 and the KPI
row, nine errors that survive a full recalculation.

`SoftscapeRows.Read` reads HEIGHT (m) and DIAMETER (m) off the columns the heading row names,
never by position, into `PrintedMeasure` on every `SpeciesRow`, with the row number beside
them. A measure is held when it reads as a number greater than nought. UNKNOWN prints a dash for
its height and 0 for its diameter, so neither is held and each says what it printed. A cell
holding a digit past its number is not held either, with the reader's own reason, and does not
refuse the schedule, because the species counts whether or not its height reads.
`MergedSpecies.FromRows` holds every row's value against the others and answers per column:
every row that holds a value agrees, and that value is written, or the rows disagree and nothing
is written with every value named and, for the diameter, the words that the row will not compute
its canopy. Nothing is averaged and nothing is taken first. A row printing a dash beside rows
that agree is named and does not stop them.

`SpeciesList.In` finds the sheet's height and diameter columns by what its header row, row 3,
calls them, the words HEIGHT and DIAMETER, and a header naming none or more than one of either
gives no column and the reason. Measured I and J on MOSQUES, Mature Height (m) and Average Mature
Canopy Diameter (m), and a test moves the same headings to N and O and finds them there. The
letters are written nowhere. `KpiCreatePlan.Of` writes four things for a species written in, the
name into D, the count into B, the height and the diameter into those two columns, and nothing
else. Each of the two that cannot be written is named under CELLS NOT WRITTEN with its cell and
the reason. The report's species table gained a height and a diameter column saying what went
into each cell or why nothing did.

**Two things stated and not seen.** The header row is row 3 on both tree sheets, which is the
one number the map holds and was measured on all seven templates. And whether the empty rows of
the client's sheet already hold anything in their height and diameter cells is UNKNOWN. The
patcher replaces whatever a written cell holds, so a formula sitting in I84 would be replaced by
the number. Nothing measured says there is one.

### 2. The double count, and why the guard counted one

**The guard was right and the diagnosis it was built on was wrong.** The count and the read have
been one list in one pass since the forty sixth pass: `KpiPlotReader.Read` sorts every schedule
filtered on the plot into its kind before it reads any, and reads the one when there is one. The
1428 report read one softscape schedule on all 20 plots because there is one. The two FM-05
entries in a species row are two printed rows of that one schedule for one species under one
group, ALBIZIA LEBBECK 10 and 10, BAUHINIA PURPUREA 19 and 20, CASSIA GLAUCA 3 and 4. Counts that
differ are not one row read twice, and the workbook was written, so the species rows added to
the printed TOTAL on every plot that printed one. The forty sixth pass took the user's reading of
a second schedule as measured and fixed a fault the model does not have. That is in the rules
file in those words.

**Whether two rows for one species under one group are two types of it or one counted twice is
written down nowhere**, so the tool refuses rather than choosing. `SpeciesRow.RowNumber` is the
printed row, `PlotReading.SpeciesPrintedOnMoreThanOneRow` finds every such species, and
`Reconciliation.Of` refuses the write naming the plot, the species, the group, the rows and the
counts, and says the rows are printed at the end of the report. Three refusals for FM-05 on the
fixture. A merged row's working reads FM-05 20 (2 rows, 10 + 10), FM-06 15 rather than FM-05
twice, and the plot's own block says which species printed on which rows. **This refuses the
next 20 plot run until Bader says which the FM-05 rows are.** The section under item 6 will show
the rows, with their PLANT CODE, HEIGHT and DIAMETER columns, which is what can settle it. If
they are two types, the refusal comes out and the rows are added, which is what the schedule's
own TOTAL does. If they are one counted twice, the cause is in the model and the tool cannot
know it from here.

### 3. Nothing checked the output still computes, and now something does

`WorkbookFormulas.Check` reads every formula in the output, by its text and never by evaluating
one, after the read back. It finds three things off the text. A formula holding ISBLANK on a cell
that is blank in the output and a string literal returns that text. A formula doing arithmetic on
such a cell is #VALUE!. Every formula reading a cell in error carries it, through ranges and
across sheets, resolved through the workbook's defined names. A shared formula's dependents get
the master's text shifted to their own row, which is how Excel stores a column of one formula.
When the chain starts on a row this run wrote into, `WorkbookPatcher.Patch` deletes the output
again and returns `PatchOutcome.RefusedAfterWriting`, so the run ends with no file and the
report says NOTHING WAS WRITTEN with every formula named. On the fixture shaped like the 1428
workbook, a species written into row 7 with no diameter refuses on 8 formulas: M7, M10, F8, D8,
D9, E31, F31 and G31, with L7 as the cause. The template's own empty rows return a space from
the same formula and are not an error, because the cell beside them guards the blank count.

The section WHAT THE WORKBOOK WILL COMPUTE FROM THIS prints every formula at risk with the
reason, every formula reading a row this run wrote into with the reference it reads it through,
the six cells the map names with whether each is present and which formulas read it and which of
their inputs are blank, and the functions under item 5. `KpiRequestHandler` hands the patcher
the map's cells for that. **A consequence, stated outright:** a species with no diameter written
into an empty row of the client's list refuses the run, because the row's canopy formula cannot
compute from it. UNKNOWN on a proposed list does exactly that. The request asked for a refusal
and not a note, and this is it.

### 4. calcMode auto, the fifth check, and what else the package can hold

`calcPr` carries `calcMode="auto"` beside `calcId="0"` and `fullCalcOnLoad="1"`. `CacheCheck`
reads it back and `WillRecalculate` requires it, five checks and not four, and the report prints
the fifth line. The patcher also looks for every other place in the package that can hold a
calculation setting: a `sheetCalcPr` element in any sheet part, an `xl/vbaProject.bin` part, and
any attribute on `calcPr` other than the three it sets. On the test workbooks it found none, a
`sheetCalcPr` planted in one is found and named, and the report says what was looked for either
way. On the client's MOSQUES template what it will find is UNKNOWN until the next run.

**I cannot test this in Excel and neither can the gate.** The check is over what the file says
and not over what Excel does with it. Whether an explicit `calcMode="auto"` overrides a manual
session in every version of Excel is not measured here.

### 5. The formulas the reader's Excel may not have

Every formula whose text holds `_xlfn.` is counted, by the function named after the prefix and
by cells. On the fixture that is IFS in 2 cells, and the section says those cells need a version
of Excel that has IFS and read #NAME? in one that does not. Which version is not worked out, as
asked. The tool writes no formula and the section says so.

### 6. The report shows what Revit printed

`PlotReading.PrintedSchedules` carries every schedule the plot's numbers came off, as
`KpiPlotReader.Printed` read it, and the report's last section prints each under EVERY SCHEDULE
THIS RUN READ, AS THE SCHEDULE PRINTS IT: the plot and the schedule's name, which rows were read
as species rows, how many were passed over as subtotals and where the TOTAL row was, and for a
shrubs and lawn schedule which subtotal row each group's value was taken off, the last of its
subtotal rows, with the rows above it named as the phase subtotals that add to it and are not
taken. `GroupSubtotal.RowNumber` and `RowsConsidered` carry that. Every column is padded to its
widest cell, rows are numbered the way the readers number them with the heading row as 1, and a
schedule is cut at 200 rows saying how many of how many are shown. The top of the report says
the section is there.

### Reported and not fixed

**Phoenix dactylifera reads 15 metres across in the model and 8 on the client's existing list at
row 86.** `KpiCreatePlan.Differences` names every matched species whose height or diameter in
Revit is not what its row holds, the report prints them under MATCHED SPECIES WHOSE HEIGHT OR
DIAMETER IN REVIT DIFFERS FROM THE ROW'S with CHANGED NOTHING on every line, and nothing is
written over the client's number. **Open question for Bader:** two numbers for one species, one
from the client's palette and one from the model. Which is right, and should the tool ever say?

### Break watches

Five, each restored byte for byte and checked with md5, the suite rerun green at 1046.

- the height and diameter no longer written for a species written in: **4 red**, three in
  `CanopyColumnsTests` and the rewritten `KpiCreatePlanTests` one
- a species on two rows under one group no longer refusing: **1 red**,
  `ASpeciesOnTwoRowsUnderOneGroupRefusesTheWriteNamingTheRows`
- a formula reading an error off a written row no longer refusing: **2 red**, both in
  `WorkbookFormulasTests`
- `calcMode` no longer set: **2 red**, the fifth check test and the patcher's own recalculate
  test
- the schedules section dropped from the report: **5 red**, all of `SchedulesAsPrintedTests`

Item 5 has no watch of its own: the function count is asserted in `WorkbookFormulasTests` and
would go red with the check.

### Two existing tests rewritten

`AnAddedSpeciesWritesItsNameAndItsCountAndNothingElse` said an added species writes two things.
It writes four now, and a match built with no list behind it names the other two, so the test
says that. The TOTAL line under a plot names its row now, so the DM-12 test passes the row
through the fixture and expects row 14, written out by hand.

---

## 2026-09-10, forty sixth pass. Two faults measured on the first twenty plot run

Two faults, both found by running 20 mosque plots for real at 11:16 and reading the workbook
the run wrote. Neither could have been found by reading code. Both are fixed. **The other 43
audit findings stay open**, not renumbered, not reordered, not annotated. Nothing else was
touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round has
been observed in Revit.** The workbook the run wrote was not handed over and was not needed: the
row numbers and the counts below are the ones stated for it, and the test workbook is built to
that shape with made up names on every row the run did not name.

Pull request 59, merged into main as `48bd623`. **The runner executed 1014 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1014 ran at `48bd623`, 0 failed and 0
skipped**, 525 of them KPI, 20 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### Fault 1. The tree list had three row ranges and the tool trusted the shortest

Audit finding 5 made worse, three records rather than two. Measured on Tree List - Existing of
the MOSQUES workbook the run wrote: the map and the pane said B4 to B83, the sheet's total said
`SUM(B4:B92)`, and the botanical names ran from row 4 to row 101, 98 species. The map stopped 18
rows before the names and the total 9 rows before them. Six species were reported as having
nowhere to go, 85 existing trees, and four of the six sat in the list past row 83: CONOCARPUS
LANCIFOLIUS 17 at row 84, PHOENIX DACTYLIFERA 27 at 86, WASHINGTONIA ROBUSTA 19 at 87 and FICUS
BENJAMINA 3 at 89. PROSOPIS JULIFLORA 3 sits at row 99, past the total, and UNKNOWN 16 is
genuinely absent. The workbook said 76 existing trees where the model holds 161.

**What the map no longer claims.** `TreeRows` with its `FirstRow`, `LastRow`, `RowCount` and
`InWords` is gone, and every template's entry is `TreeSheet`, which holds the sheet name and
nothing else. A test reads the type's properties and goes red if a number comes back. The pane's
line no longer prints a range: it says the rows and the total's reach are read off the file when
Create is pressed. The one number the map still holds about a tree list is the header row, 3,
measured on all seven templates, and the list is read down from the row under it.

**What is read.** `SpeciesList.In` reads two things off each sheet and holds them apart. Every
row of column D that names a species, from row 4 down until the first row with no name, is the
list a species from Revit is matched against. The total's own `SUM(B4:B92)` formula, found in
column B, says which rows a count reaches, and the cell it sits in is recorded, B93. The empty
rows for a species the list does not hold are worked out from those two, the rows the total
reaches that name nothing, and are stated by nothing else. The test fixture builds a list the
same way, from names and a total range, so no test can state an empty row that is named.

**A species matched to a row the total does not reach is refused**, per species. The row is
kept on the match, nothing is written there, the reason names the row and the total, and it
prints under a new report heading, SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH, and
in CELLS NOT WRITTEN with its cell. I read the request's word refusal as refusing that count and
not the whole workbook, because the request weighs writing it against not writing it, and both
sides of that weighing have the workbook written. If the whole run should refuse instead, that
is one line in `Reconciliation.Of` and the lists would move ahead of it. A sheet with no `SUM`
over column B refuses every species the same way, matched or not, because nothing then says
which rows a count reaches. A name below the first empty row of the list is not the list, is
named in the report, and a species carrying it is refused rather than written in above itself.
Nothing measured holds such a row and the shape is stated rather than assumed.

**The report prints both lists as read**, under THE WORKBOOK'S OWN TREE LISTS: the names and
their rows, the total and its reach, the empty rows, the names the total does not reach and any
names below the list. When the accounting refused before the template was opened it says so.

**Checked against the stated shape, by hand.** Existing: 98 names in rows 4 to 101, total
`SUM(B4:B92)` at B93, 0 empty rows, 9 names the total does not reach in rows 93 to 101. Proposed:
83 names in rows 4 to 86, the same total, 6 empty rows 87 to 92, none outside. The five species
match their rows, 84, 86, 87, 89 and 99. The first four are written, 66 trees, PROSOPIS
JULIFLORA is named with row 99 and B99, and UNKNOWN is absent with no empty row to go into.

**Two things about that file are stated and not seen.** The names are taken as one unbroken
run from row 4 to 101, because 98 names in rows 4 to 101 is exactly that many rows. The total
is placed at B93 because the rules file measured it there on the annotated set and the request
gave the formula without the cell. Both are in the test fixture and neither was read off the
workbook here.

**One thing the measurements say that nobody asked about.** On 2026-09-09 the MOSQUES existing
list read 80 names in rows 4 to 83. On 2026-09-10 it read 98 in rows 4 to 101. Eighteen names
were added between the two runs, nine of them past the total's reach, and who added them and
why the total was not extended is UNKNOWN. It is in the rules file as a question for the team.

### Fault 2. One plot's trees were counted twice

Audit finding 36, now measured. FM-05 holds two schedules whose names hold SOFTSCAPE.
`KpiPlotReader` appended every one and `KpiMerge.Species` added them by name and group, so the
species rows printed FM-05 twice, FM-05 10, FM-05 10, FM-06 15, and ALBIZIA LEBBECK proposed read
170 where the truth is nearer 160. The shrubs and lawn read had the other half: `SubtotalHeaded`
took the first group with the heading and any second schedule was ignored in silence.

**The reader counts before it reads.** Every schedule filtered on the plot is sorted into its
kind first, softscape or shrubs and lawn, and only a kind with exactly one schedule is read.
`PlotReading` takes the names of every schedule of each kind in place of the two booleans, and
`SoftscapeRead` and `ShrubsAndLawnRead` are now exactly one name. It refuses to be built holding
species rows beside two softscape names or beside none, and subtotals beside two shrubs and
lawn names or beside none, so the doubled count cannot be held anywhere. `Reconciliation.Of`
refuses the write for a plot holding two of a kind, naming the plot, the kind and every schedule
found, and a plot that gave nothing for that reason says so beside its name. The count line
reads plots with one softscape schedule, with the plots holding none and the plots holding more
than one both named.

**The report names the schedule each number came off, per plot.** It read softscape schedule:
11 species rows read before and could not say which. It reads softscape schedule:
FM-06-(600) SOFTSCAPE SCHEDULE, 1 species row read now, and for FM-05 softscape schedules: 2
FOUND AND NONE READ with both names, and for a plot with none, none filters on this plot.

### What the read costs, measured in calls and not changed

The run took 313.5 seconds and 312.8 of that was the read, 20 plots at 15.6 seconds each. The
first thing to check is confirmed by reading `KpiPlotReader.Read`: **every plot walks every
schedule in the model.** For each of the 951 schedules it calls `PlotFilteredOn`, which opens
the schedule's definition, walks its filters and reads each filter's field name, to find the
handful filtered on the plot. Twenty plots is 19,020 of those definition reads and 78 plots is
74,178. Three more costs sit beside it, per plot: `FirstSheetOf` collects every sheet and sorts
all 1,385 by natural order before reading PRX_Plot_ID down the list until it finds the plot,
`RegionsFor` walks every filled region in every 00 link and reads two parameters off each, and
`Printed` calls `GetCellText` once per cell of the one or two schedules that matched. Which of
the four carries the 15.6 seconds is UNKNOWN: the report holds one number per plot and no finer
timer exists, and nothing here runs Revit. Nothing in the read was changed.

### Break watches

Three, each restored byte for byte and checked with md5, the suite rerun green at 1014.

- The list reader made to stop at row 83, where the map used to: **5 red**, all in
  `TreeListRowsTests`. The existing list to row 101, the proposed list to row 86, the five
  species and their rows, the plan's four writes and the named fifth cell, and the report's
  tree list section
- The two schedules refusal taken out of `Reconciliation.Of`: **1 red**,
  `TwoSoftscapeSchedulesOnOnePlotRefuseTheWriteNamingBoth`
- The guard taken off `PlotReading`, so it holds species rows beside two softscape names
  again: **1 red**, `AReadingCannotCarryNumbersOffTwoSchedulesOfOneKindOrOffNone`

### What worked, on the record

The subtotal fix holds on every number it was predicted to move: DM-16 shrubs 30 to 84, DM-25 13
to 241, FM-05 shrubs 361 to 820, FM-05 grass 96 to 165. Shrubs total 2517 to 3258, lawn 1058 to
1127. The date, prepared by and position boxes reached the file. The cache section read cached
results left 0, dropped 1674, 37 parts in and 36 out with calcChain named as the one removed on
purpose. Three species were written into empty rows on the Proposed sheet with the name and the
count and nothing else. All of it is in the rules file under what the first twenty plot run
measured.

---

