# KPI log

Newest entry first.

---

## 2026-09-12, fifty eighth pass. Create scans if it needs to, and the scan button is gone

Item 1 of two, on its own as Bader asked, so that if the first real run breaks nobody has to
work out which half did it. Item 2, several templates in one run, is a round of its own and is
NOT in this one. The branch came off a fresh pull of main at `b5e2c90`, so the baseline is
**1282 tests, 700 of them KPI**. **Nothing in this round has been observed in Revit**, and the
1552 run's correct workbook is what it must not cost: 36 parts, calcChain removed, calcId 0,
calcMode auto, no cached value left, zero errors on recalculation, Total Green cover 13,516,
Canopy 11,168, Total Trees 374, Planting 1,493, Lawn 855 off 70,343.

Pull request and merge hash: in the record entry the merge adds above this line. Locally
**1289 tests at this branch, 0 failed and 0 skipped, 707 of them KPI, 7 added here**, against
the 1282 main carries. Build zero warnings.

### The scan is a step inside Create

`ScanNeeded.Decide` in Core is the whole rule and it is the same shape `HeldReadings.Decide`
already uses for the per plot readings, because two shapes for one kind of question would be
two rules to keep in step. The model is read when no scan is held or what is held is of
another model, and a scan already held answers a second press on the same model. The handler
holds the title its scan was read from and Create calls `Scan` itself before it reads a plot.
Nothing about the reading reuse moved: `HeldReadings` decides that separately and the 2.5
second finish after a 123 second read is that working, untouched.

The press says which of the two it did, `Reusing the scan already held` or the real read's own
section lines, for the same reason the readings say it. **The scan's headline moved from
`Told` to `Progressed`**, which matters: `Told` is the run's end line and it is what shuts the
progress window, so the headline said there would have ended the press on screen, and closed
the window, with the workbook still being filled.

### The button, and where the header's numbers come from now

`KPI Scan` is off the pane's strip and `AskedForAScan` is deleted, the `Scan` request is off
the enum and out of the switch, and the ribbon tooltip no longer describes a button that is
not there. Four code comments naming it are corrected and two test files pinned its old
wording, both updated by hand.

**The header's element count now comes back with the plot read** rather than off the scan.
`KpiPlotFacts` carries the count and the seconds, `ReadThePlots` counts instances the same way
the scan counts them, and `Found` fills the line. So the model's name has a number under it as
soon as the pane is shown, which is what the scan line used to do only after a press. That
read is once per model already, guarded by the title the plots were last asked for, so the
count costs one collector pass per model rather than one per redraw.

### The progress window

`KpiProgressWindow`, modeless, owned by the Revit main window through its handle, showing the
same `ProgressWords` lines the status line shows. **Opened by the pane before the external
event is raised and closed when the run's last answer comes back, never from inside
`Execute`**, which is the hazard the fifty fifth pass's breaker caught when the repaint pump
was pumping at a priority that let queued clicks run inside the handler's call frame. `Told`
closes it, being the end line whatever ended the run, a finish, a refusal or a throw.

**There is no Cancel, deliberately, and this is the round's one refusal to build something.**
Bader asked for one if it is cheap. It is not: cancelling mid read leaves a half read set of
plots that the reconciliation would count as plots read, which is exactly the half read state
the rest of the tool would trust. An honest cancel is a cancellation threaded through every
reader plus a discard of everything read, which is its own round. A cancel that lies is worse
than no cancel.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1289.

- a held scan of another model taken for this one: **3 red**, the other model case, the exact
  title comparison and the no document case
- nothing held taken as a scan of this model: **1 red**, the first press on a fresh pane
- the pane words naming the vanished button again: **1 red**, the fixed lines test

### Existing tests changed

Two, both by hand. `KpiPaneWordsTests` pinned `Not scanned yet. Press KPI Scan.` and the
`ReadOnly` line naming the button, and gained a line holding that none of the three fixed
lines names it. `KpiCreateTests` pinned the `NoPlots` line telling somebody to press it. No
test moved for any other reason.

`PaneLabelTests` at the tests root also holds the string KPI Scan, and it is LEFT ALONE: it
is not KPI's file, and it uses the words as input to the caption escape rather than asserting
any button exists.

### The diff read adversarially, and what it found

A breaker was set on the diff and died on a rate limit before reporting, so the checks were
made here instead, at today's lines. Four came back clean and one is a real limit.

- **Every path that ends a press reaches `Told`, so the window always shuts.** Run's four
  typed catches, the `document == null` return, the `CannotCreate` return and Execute's own
  `Stop(failed)` all invoke it, `Stop` inside a try of its own so even a throw on the way out
  cannot skip it. That was the worst case worth ruling out: a modeless window left over Revit
  with no way to shut it
- **The scan runs after the refusal guard**, so a press refused for no template, no plot or no
  output folder does not spend two minutes reading the model first
- **The header cannot be wiped by the redraw that follows it.** `Found` sets `_scannedTitle`
  off `_model.Title`, and `_model` is always set first, because `Named` is invoked before the
  switch that reaches `ReadThePlots`. The next `Took` then compares equal and resets nothing
- **Nothing still references the removed request.** The two `AskedForAScan` hits left in the
  tree are the Drawing Sheet's own, in its own file, untouched

**The one real limit, known and left: a second press of Create while the first is still
running leaves the second run without a window.** `Shut` closes the first window before the
second opens, so no window is orphaned over Revit, but the first run's end line then shuts the
second run's window and the second run goes on unseen. A busy flag on the press would fix it
and would introduce a worse failure: a flag that fails to clear leaves Create dead with no way
back, and the pane has no way to ask whether a run is still going. The same reasoning as the
cancel. It is written down rather than guarded, and the first run is what says whether anybody
presses twice.

### What is open

Whether the window appears where it should, whether it shuts on every path, and whether the
status line moves mid run are all things only a run on a real pane can show. The scan report's
own location now shows in the progress line while the run goes rather than sitting on the
status line at the end, because the end line belongs to Create. Whether the team wants it to
persist somewhere after the press is a question for Bader.

---

## 2026-09-12, fifty seventh pass. The plots block that said open a model, and the ten warnings

Two things off the 13:48 run on RCRC_NG03_EZ. The branch came off a fresh pull of main at
`aa0f546`, the fifty eighth pass, so the baseline is **1288 tests, 699 of them KPI**, not the
1266 Bader's message names: the Drawing Sheet rounds `#82`, `#83`, `#84` and `#88` landed
since the fifty sixth pass and none of them is KPI. **Nothing in this round has been observed
in Revit.** The first of the two changes the pane's ask flow, which no Core test can reach, so
it is recorded here for the first run on a real pane. Committed in two, the warnings first as
a checkpoint, then this.

Pull request 92, merged into main as `c8bd1b8`. **The runner executed 1282 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1282 ran at `c8bd1b8`, 0 failed and 0
skipped, 700 of them KPI.** **The branch itself carried 1289, 1 added here**, against the 1288
main held at the branch point `aa0f546`. The drop from 1289 to 1282 is the Drawing Sheet
sixtieth pass `a7cd601` that landed in between, which deleted its marker family and net
removed seven tests, none of them KPI, so the KPI count is 700 on the branch and at the merge
alike, checked. The merge went through the API with the title and the message both passed on
the call, and the commit came back off main carrying neither a co-author credit line nor a
generated-by footer. Ten warnings cleared, build zero warnings.

### The plots block said open a model while a model was open

At 13:48 the header named RCRC_NG03_EZ and the footer carried its scan counts, and the plots
block read Open a model with no list, while the same model had listed 18 plots at 12:08 on the
build before. A workflow of three investigators and a judge told the two candidates Bader
named apart from the real cause, and neither candidate was it.

- **Not the repaint pump moving from background to render.** Its premise is false: git shows
  the pump was born at render in the fifty fifth pass and never ran at background, so nothing
  moved. `_facts` has one writer, reached by the priority-less `Dispatcher.Invoke` overload
  that runs inline or marshals at Send, above any pump priority, so no pump can gate it. And
  the build before the pump listed the plots with no pump of any priority at all. Told apart
  by git history and by the single writer.
- **Not the progress line's redraws firing between the ask and the answer.** Those pumps run
  only inside a scan or a create `Execute`, and every `Execute` empties the one request slot at
  its start, so a pending `Plots` cannot be in the slot while a pump runs. Told apart by when
  the pump can run against when the `Plots` request lives.

The real cause is older than both and the round's diff never touched it. `Ask(Plots)` lived at
one site, `Shown`, fired only by a visibility rise. Installing the fifty fifth and fifty sixth
passes forced a Revit restart, a pane restored visible at startup was shown before any document
existed, that one ask was consumed against No open document, and nothing ever asked again: every
later answer runs `Took`, whose redraw asks only `WhichModel`, which reads the plots nowhere. So
a scan filled the header while `_facts` stayed null and the block drew its waiting text, whose
words were Open a model. A KPI Scan pressed before the plot read returned loses the ask the same
way, displacing the pending `Plots` in the one slot. Which of the three happened at 13:48 is a
visibility and timing fact only the user's memory or a run can settle, and it is UNKNOWN, the
restart being the most economical.

**The fix is two halves.** The message, in Core with the test: `CreateWords.PlotsBlock` reads
four ways, one line each, no document says open one, a document not yet answered says it is
reading the plots, a document answered with no plots says the model holds none, and a document
answered with plots hands to `PlotSources`. Open a model is right only where no model is open,
and the not answered state is told from it by whether a document is open, read off the live
document, never a held copy. The recovery, in the pane and untestable in Core: `Ask(Plots)`
moved out of `Shown` into `Took`, the one place that knows which model answered, guarded by the
title the last ask was for and reset on close, so the plots are read once per model and a model
opened under the pane or one whose first ask was lost is read the moment any request answers
with it. `Shown` asks only for the model now. This also killed a latent double read: `Shown`
and `Took` both asking would have read the plots twice on every show.

### The ten xUnit warnings, cleared

All ten were the same mistake in two shapes, asserting on a filtered collection rather than the
condition, so the message named the filtered length rather than what it found.

- **Four xUnit2029**, `Assert.Empty(x.Where(p))` to `Assert.DoesNotContain(x, p)`: KpiCreateTests
  681 (no Folder property on OpenModel), CanopyColumnsTests 283 (no skip on the proposed sheet)
  and 308 (no write on it), WorkbookFormulasTests 137 (no formula at risk is an error)
- **Six xUnit2031**, `Assert.Single(x.Where(p))` to `Assert.Single(x, p)`, the filtering overload
  returning the one element typed: CanopyColumnsTests 310 and 348, DiameterColumnTests 186,
  TreeListRowsTests 258, 480 and 481

The assertion changed and never the subject. None changed what it checks: the `DoesNotContain`
overload takes the same predicate, and the `Single` filtering overload returns the same typed
element the chain did, which every call site still assigns and reads. Committed first as a
checkpoint, `54ab011`, so the tree was not left dirty while the workflow ran.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at 1289.

- the block says open a model whether or not one is open, the regression itself: **1 red**, the
  document open and not answered case
- the block says reading even with no document: **1 red**, the no document case, so both halves
  of the distinction are shown load bearing

### Existing tests changed

None. The warnings pass rewrote ten assertions and added none. The plots pass added one Core
test, `ThePlotsBlockReadsFourWaysOneLineEach`, and moved no other.

---

## 2026-09-12, fifty sixth pass. The coarse step is said before any number, and the pane counts the notes

Two lines off Bader's answers to two of the fifty fifth pass's four open questions, and no
behaviour changes beyond them. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing
in this round has been observed in Revit.** The branch came off a fresh pull of main at
`32cbdf1`.

Pull request 85, merged into main as `52e4395`. **The runner executed 1266 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1266 ran at `52e4395`, 0 failed and 0
skipped**, 699 of them KPI. **The branch itself carried 1266, 5 added here**, against the
1261 main held at the branch point `32cbdf1`, and the merged tree is byte for byte the
branch head, nothing landed in between. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### No ceiling on the rounding room, a line instead

Both measured models round areas to 1, so a coarser step is hypothetical and a ceiling chosen
today is a constant pretending to be a rule, the fault the fifty fifth pass's own breaker
caught in that same round. Instead, a project whose step is coarser than the metre opens the
create report with one line before anybody reads a number: THE PROJECT ROUNDS AREAS COARSER
THAN THE METRE: its step is 2, so the group total check allows 1 square metre of room for
every row summed. `KpiCreateRun` carries the `ProjectUnit` whose step gated the checks. The
handler passes it on a fresh read and carries the held run's forward on a reused press,
because the notes were earned against that unit and a step read again since could have
changed. The metre, anything finer and an unread step print nothing, tested each way with the
step of 2, the step of 10 and its 5 in the plural written out by hand.

### The pane says the notes exist

A note only the report file holds is a note nobody reads. `CreateWords.Wrote` counts the
readings' rounding notes and puts one sentence beside the written count, 2 rounding notes are
in the report, singular when one, nothing when none. The detail stays in the report, where
each note sits beside its group total row. FM-21 and FM-22 off the 1208 run are the test's
two notes.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1266.

- the coarse step line never printed: **2 red**, the step of 2 and the step of 10
- the note count never spoken: **1 red**, the status line counting FM-21 and FM-22

### Existing tests changed

None. `CreateFixture.Run` gained an optional area unit it passes through, every existing call
unchanged, and `KpiCreateRun` gained the unit as an optional trailing argument that defaults
to the unread unit, so no caller moved.

---

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

## 2026-09-10, forty fifth pass. Four findings that can put a wrong number in front of a client

Finding 2 of the first audit and findings 30, 31 and 32 of the second are fixed. **The other 43
stay open**, not renumbered, not reordered, not annotated. Nothing else was touched: not the
Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`, and not finding 40, the stale subtotal
docstring, which sits in a file this round rewrote and was left standing because it is not one
of the four.

Pull request 58, merged into main as `faa6468`. **The runner executed 994 tests against its
merged head, 0 failed and 0 skipped. Locally the same 994 ran at `faa6468`, 0 failed and 0
skipped**, 505 of them KPI, 46 added here. The 988 measured while the round was built was
against the older base `afbc4f7`, and the Drawing Sheet's forty fourth pass landed six tests
under it before this branch was moved onto `a95e40c`, so 994 is 988 plus those six. Every break
watch below was watched at the older base and the suite rerun green there.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### Finding 2. The four cell position fallbacks refuse

`SoftscapeRows.Read`, `ShrubsAndLawnRows.Read` and `ScheduleGroups.Of` read the botanical name,
the count and the area off the columns the heading row names and off nothing else. A schedule
naming none of them is refused, in one sentence from `ScheduleColumns.NothingNamed`, which names
the column and prints the headings so a person can see what the schedule does call them. The
shrubs reader's area, which came back as an empty list with nothing said and which the first
audit called the correct behaviour, carries the line now too, and so does its count, which read
nought in silence. A group whose named rows cannot be counted is still found, with the reason on
it, because the group row needs no column.

Each reader hands back what it read or every reason it refused, never both: `SoftscapeReading`
and `ShrubsAndLawnReading`. The refusals travel on `PlotReading.ReadRefusals` with the
schedule's name, `Reconciliation.Of` turns each into a refusal of the write naming the plot, the
create report prints them among the reasons and again under the plot, and the pane draws the
reasons in red above Create as it already did. Question 8 of the scan report says a group's
named rows were not counted and why, rather than printing a count off the image column.

The docstring that described the fallback as intended is rewritten.

### Finding 30. A digit after the number ends is a refusal

`CellNumber.Read` hands back one of three answers, `CellNumberRead`: a number, an empty cell, or
a refusal naming what the cell held. A digit anywhere past where the number ends refuses, so
"1,234 m2" is refused rather than read as 1, "1131,72" is refused rather than read as 1131, and
so is "1 234". Nothing parses the separator, because which character a project groups digits
with is a units setting this tool has never read. Every value measured on the real model still
reads: 35, 70, 105, 820, 1161, 3729, 1131.72, 46, 1020 and 0, all written out by hand in a
theory. Revit prints the unit with a superscript two, which is not a digit, so "35 m2" spelt with
an ASCII two is refused too and the test says why.

A refused cell refuses the whole schedule read, with the row and the cell named. A group over a
thousand printed with separators, 1,200 and 1,300 totalling 2,500, now refuses on its first bad
cell rather than passing its own add-up check as 1 plus 1 equals 2.

### Finding 31. STREETS reads no area and refuses on none

`Reconciliation.Of` takes the template, and it is a required argument so no caller can forget
it. Where the template's map holds no area cell, which is STREETS, `KpiRequestHandler.Create`
reads no filled region for any plot, chooses none, and the reconciliation refuses on nothing
about the area: not two regions holding one, not two plots reading alike. `WithoutArea` is empty
and a plot that gave nothing is not blamed for the area. The report says in three places that
the area was not read and why, under the reconciliation, beside each plot and where the region
table would have been, with the pane's own sentence for the condition, and it drops the paragraph
about the area not being a schedule row while keeping the one about the schedules.

MM-03 and MM-04 on MOSQUES still refuse, so the guard is where it was and only a template with
no area cell steps round it.

### Finding 32. The TOTAL row is read and held against the species rows

`SoftscapeRows.Read` reads the count off the row whose first cell holds TOTAL, `PlotReading`
carries it, and `Reconciliation.Of` refuses the write when the species rows add to something
else, naming both numbers: "DM-12: its species rows add to 31 and its softscape schedule prints
TOTAL 39." The report prints the sum beside the TOTAL under every plot, and says so when no
TOTAL row was found, because a check with no subject is not a failure and silence would read as
a check that passed.

The two bare continues are gone. A row with a botanical name and no whole count refuses the
read and names the row. A row with a count and no name is the subtotal a group prints, counted
as passed over and printed. A one cell row such as the TREES category is a heading and is
skipped as one, which the two column DM-12 fixture showed when the first version refused it, and
the TOTAL row is read before that shape is looked at, because a TOTAL row with an empty count is
one cell of text and is a refusal rather than a heading.

Checked against DM-12 as the 1521 scan printed it: five existing, three proposed, the eight rows
adding to 39, TOTAL 39, one subtotal row passed over.

### What was broken to see the tests go red

The four fixes change signatures, so the new tests do not compile against the code as it was.
Each fix was reverted in behaviour with the signature kept, watched, and restored byte for byte
with md5, then the suite rerun green at 988.

- finding 2, the botanical column made to fall back to cell 0 again. **2 red**:
  `ASoftscapeScheduleNamingNoBotanicalColumnIsRefusedRatherThanReadOffTheImageCell` and
  `BothMissingColumnsAreBothNamed`
- finding 30, the digit after the number read short again. **6 red**:
  `AThousandsSeparatorIsRefusedAndTheCellIsNamed`, `ADecimalCommaIsRefusedTheSameWay`,
  `ASpaceUsedToGroupDigitsIsRefusedToo`,
  `AUnitSpeltWithADigitIsRefusedBecauseNothingCanTellItFromASeparator`,
  `AGroupOverAThousandPrintedWithSeparatorsIsRefusedRatherThanAddingOneAndOne` and
  `ASpeciesCountWithASeparatorIsRefusedRatherThanReadAsOne`
- finding 31, the area refused on for every template again. **4 red**:
  `OnStreetsTwoPlotsReadingOneAreaDoNotRefuse`, `OnStreetsTwoRegionsHoldingAnAreaAskNothing`,
  `OnStreetsAPlotWithNothingElseIsNotBlamedForTheArea` and `TheReportSaysTheAreaWasNotReadAndWhy`
- finding 32, the TOTAL held against nothing again. **2 red**:
  `SpeciesRowsShortOfTheTotalRefuseTheWriteAndNameBothNumbers` and
  `TheReaderHandsBackBothNumbersAndTheReconciliationRefuses`

Every expected value is written out by hand. The 39 is 1 plus 1 plus 5 plus 2 plus 1 plus 13
plus 6 plus 10, and the 31 is that list short of PHOENIX DACTYLIFERA, UNKNOWN and WASHINGTONIA
ROBUSTA.

### Never observed in Revit

Everything in this round. No refused column has been seen on a real schedule, no separator has
been seen printed, STREETS has still never been picked, and no TOTAL row has been read off a
live model. `KpiPlotReader.Read` and `KpiRequestHandler.Create` carry the Revit side of all four
and no test loads either.

### Not touched

The Drawing Sheet. `Core/Shared`. `CLAUDE.md`. The other 43 findings.

---

## 2026-09-10, forty second pass. The second audit of the KPI tool

Pull request 55, merged into main as `5beea47`, onto main as it stood after the Drawing Sheet's
own audit landed as `d60a750`. **The runner executed 942 tests against its merged head, 0 failed
and 0 skipped. Locally the same 942 ran, 0 failed and 0 skipped**, at `82d95f4`, before the
three breaks and again after each was restored byte for byte. Nothing added, because nothing
was changed.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### What it is

`steps/audit-kpi-2.md`, read only. Part A goes through the 27 open findings of the first audit
before anything else: **every one STILL STANDS**, with the line numbers as the files read today,
none passed by when the code moved in pull requests 52 and 53 and none found untrue. Findings
10 and 11 were re-proved by the same breaks that proved them at 904.

Twenty new findings, numbered 30 to 49 so the two audits cite together: 0 BLOCKS, 3 WRONG,
7 COSTLY, 10 TIDY, and 9 dropped as costless. The shape hunted was a right line of code standing
on a fact measured once, which is what the subtotal rule was for four rounds, and the logic
notes list every rule in the tool that rests on one observation with what it was measured on.

### The three WRONG

**`CellNumber` reads a digit grouping separator as the end of the number.** Every printed value
it has met is under a thousand, and the two four figure values ever seen, 1161 and 3729, came
off the one project, whose unit format prints no separator. Nothing reads the setting. On a
project that groups digits a one phase group over 999 m2 prints two rows alike, both read as
their thousands, the add-up check passes because 1 equals 1, and 1 is written into F10. Parks
and streets are where areas run past a thousand and neither has ever been run.

**The area is read, totalled and refused on for every template, and STREETS has no area
cell.** MM-03 and MM-04 are street plots and both read 12182.05561411 in the 00 link, measured
on the 1355 scan. So the first 78 plot run will end asking the user to confirm an area the
workbook has no cell for, and read all 78 again after the confirm. That is a prediction and not
an observation, because STREETS has never been picked.

**The softscape TOTAL row is printed by the schedule and read by nothing.** `SoftscapeRows`
drops a row whose count does not read as a whole number with a bare continue, and the report
prints how many species rows it kept and never what the schedule says they add to. The first
real workbook read 31 trees where the model held 39 and nothing in the tool said so.

### What was broken to see whether a test would notice

All three restored byte for byte and checked with md5, and the suite rerun green at 942.

- shrubs and lawn swapped on the way to their cells in `KpiCreatePlan.Of`: **942 green**,
  finding 10 stands
- the read back in `WorkbookPatcher.Patch` replaced with the value that was sent: **942
  green**, finding 11 stands
- CELLS WRITTEN in the create report printed off the plan rather than off what landed: **942
  green**, finding 33, new. No test builds a run that wrote

### The hooks

All four probed with real payloads, exit codes read, nothing committed, the tree clean after.
`block-paths.sh` refused two writes outside the repo and passed one inside.
`require-file-on-commit.sh` refused a commit with no state file. `territory-check.sh` passed
Kpi alone, refused Kpi beside Drawing Sheet naming both, and refused Kpi beside `Core/Shared`
naming the Shared file. `writing-check.sh` refused a listed word and a staged em dash and
passed a clean message. One probe was made wrongly and is recorded in the audit.

### The tally

Two records of one fact, every instance this repo has named: twenty one. Eight fixed, six open
from the first audit, five new here, one from `steps/audit.md`, one held open on purpose.
Twelve stand in the code today.

### What has run in Revit

Nothing in this round. It is an audit. The user's own list stands: no workbook written since
the cache fix, the species rows, the output folder, the two guards or the subtotal fix, the
identical area flag never fired, STREETS never picked, the 78 plot run never attempted.

### Not touched

Every code file, every test, every rules file, `CLAUDE.md`, the Drawing Sheet and `Core/Shared`.
The state file is in the commit because the commit hook requires it.

---

## 2026-09-10, forty first pass. Three things off the 0928 run, the first twenty plot run

Pull request 53, merged into main as `b83e3d6`. **The runner executed 942 tests against its
merged head, 0 failed and 0 skipped. Locally the same 942 ran, 0 failed and 0 skipped**, after
the last file was written and after all three break watches were restored byte for byte. 927
before, 15 added.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### The subtotal rule was wrong, and the refusal is why it never reached a client

**A group prints ONE SUBTOTAL PER PHASE, then the GROUP TOTAL.** Measured on the 0928 run over
20 mosque plots. A group holding Existing and Proposed prints three rows. A group holding one
phase prints two equal rows, which is what every DM-11 group does and why DM-11 looked like a
doubled subtotal.

Four out of four, the last row is exactly the ones above it added, in area and in item count:

```
DM-16 SHRUBS & GROUND COVER   30 over 39,   54 over 69,   84 over 108
DM-25 SHRUBS & GROUND COVER   13 over 9,    228 over 286, 241 over 295
FM-05 GRASS                   96 over 117,  69 over 84,   165 over 201
FM-05 SHRUBS & GROUND COVER   361 over 450, 459 over 570, 820 over 1020
```

The old rule took the FIRST row, which took one phase and called it the group. DM-16 shrubs took
30 where the group is 84. DM-25 shrubs took 13 where it is 241. FM-05 grass took 96 where it is
165. FM-05 shrubs took 361 where it is 820. **The 0928 run refused rather than writing, so none
of those four numbers reached a workbook.**

`Close` takes the last row now. The check is not gone, it is pointed at the right thing: **the
last row must equal the rows above it added together**, in area and in item count, with the same
relative room `Totalled.Adds` allows so the last bits of a double cannot refuse a schedule that
adds up. When it does, the last row is written. When it does not, that is a real disagreement, it
travels on the `GroupSubtotal` and it refuses the write, which is what the old check was for and
what it was pointed at wrongly. A group printing one row has nothing above it to compare against
and is taken.

**The record is corrected in three places.** `kpi-rules.md` carried the wrong rule and now
carries the measured one with both shapes drawn out and the four numbers. `CLAUDE.md` did NOT
carry the wrong claim, checked by grep: its only subtotal sentence is about the SOFTSCAPE
schedule and that one is untouched. The measured shape is added there as a project fact, and the
one example is not a rule entry gains this as its worst instance. **The log entry that recorded
the old rule as measured is corrected in place**, at the forty first pass note inside the thirty
first pass entry, with the wrong paragraph left standing above the correction rather than
quietly replaced.

### The run is not timed, so nobody knows why it is slow

The 0928 run over 20 plots took about five minutes on the clock and no file recorded a duration.
The checklist report carried one timestamp, Written, and nothing else, while the scan report has
carried elements and seconds at the top since its first round.

Three numbers now, and `RunTiming` is the one record of the first two:

- the whole press, at the top of the report
- the model read apart from it, so the part that grows with the plots ticked is separable
- each plot's own read, beside that plot under EVERY PLOT THAT WENT IN

A run nothing timed says NOT TIMED rather than printing nought seconds, because a zero reads as
an answer and this one would mean a five minute run took no time at all.

**What the slow part is. UNKNOWN as a duration, and countable as work.** No Revit ran here, so
this round cannot say how many of the five minutes went where, and the timing added above is
what will say it on the next run. What can be said without Revit is what the code does per plot,
counted off the code against the element counts already measured on this model:

- `FirstSheetOf` builds a collector over ALL 1385 sheets, SORTS them by sheet number through
  `NaturalOrder`, and reads `PRX_Plot_ID` off each until it matches. Once per plot
- `Read` builds a collector over every non template schedule, and for EVERY one of them reads
  `schedule.Definition`, then `GetFilters()`, then `GetField` and `GetName` per filter, to find
  the two it wants. Once per plot. Bader's guess about this is right and it is worse than a name
  comparison: each of those is a Revit API call rather than a string compare
- `RegionsFor` builds a collector over every filled region in the 00 link, 279 of them, and
  reads `PRX_Ref Plot ID` off each. Once per plot

Bader counted 951 schedules. On 20 plots that is 19,020 schedule definition reads, 27,700 sheet
parameter reads with 20 full sorts of 1385, and 5,580 region parameter reads. On the 78 street
plots it is 74,178, 108,030 and 21,762. Two schedules per plot are used, not three: the softscape
one and the shrubs and lawn one.

**Gathering once would cost one pass each.** One walk of the schedules keyed on the plot their
filter names, one walk of the sheets keyed on `PRX_Plot_ID`, one walk of the regions keyed on
`PRX_Ref Plot ID`, all three built before the plot loop. That turns 19,020 definition reads into
951 and 74,178 into 951, and the same shape for the other two. **Nothing was changed this round.
Measure first, then decide, which is what Bader asked for.**

### The refusal printed twice

Four refusal lines printed in red above the Create button and again word for word in the status
line at the bottom. Say it once. The red block is the right place, because it is where the user
is looking when they press.

`CreateWords.ReasonsAreAbove` counts them and points there: nothing was written, how many
reasons there are, and where the report is. The patch's own refusal is still said in full down
there, because nothing else on the pane carries that one. This narrows guard 2 from the round
before rather than undoing it: the line still never goes blank.

### What was broken to see the tests go red

All three restored byte for byte and checked with md5, and the suite rerun green at 942.

- `Close` put back to `subtotals[0]`. **5 red**, the four measured groups and the three phase
  case: `Dm16ShrubsIsEightyFourAndNotThirty`, `Dm25ShrubsIsTwoHundredAndFortyOneAndNotThirteen`,
  `Fm05GrassIsOneHundredAndSixtyFiveAndNotNinetySix`,
  `Fm05ShrubsIsEightHundredAndTwentyAndNotThreeHundredAndSixtyOne` and
  `AGroupHoldingThreePhasesPrintsFourRowsAndTheLastIsStillTheGroup`
- the `TheClock` call taken out of the report header. **2 red**,
  `TheHeaderSaysTheWholeRunTheReadAndWhatIsLeft` and
  `ARunThatWasNotTimedSaysSoRatherThanPrintingNought`
- the status line put back to `Refused(run.Reconciliation)`. **1 red**,
  `AnAccountingThatRefusedIsCountedRatherThanRepeated`

Two tests written in earlier rounds encoded the rules being replaced and were rewritten rather
than deleted: `TheSubtotalPrintsTwiceAndOnlyOneIsTaken`, now
`Dm11sGroupsEachHoldOnePhaseSoEachPrintsTwoEqualRows`, and
`TwoSubtotalRowsThatDisagreeAreNamedRatherThanChosenBetween`, now
`AGroupTotalThatDoesNotEqualTheRowsAboveItIsNamedRatherThanChosenBetween`.

### Open, and for Bader rather than for code

**WHAT SHOULD THE COMPONENT AND THE REFERENCE READ ON A CHECKLIST COVERING A WHOLE TEMPLATE.**
Ticking a whole template's plots gives 20 mosque plots holding two component values, DAILY
MOSQUE and FRIDAY MOSQUE, and 20 different references, so D3 and C5 come out empty.

**That is correct today and nothing here is a fault.** `AgreedValue` writes a value only when
every chosen plot holds the same one, because joining them with commas, taking the first and
taking the most common all write something nobody chose. The report names the distinct values
so the emptiness is never silent.

What is not written down is what those two cells are FOR when a checklist covers a whole asset
type. Nothing in the tool may pick an answer to that.

### What worked, on the record

All six interface fixes from the round before hold on the real pane. The underscore is back in
PRX_Component. The reference sample names DM-11 as the first ticked plot. What MOSQUES would
fill says read off the plot's first sheet rather than the title block. The grouping buttons reach
every plot, 15 plus 11 plus 1 plus 20 plus 24 plus 6 plus 78 is 155, and EXISTING PARKS shows 15
because the prefix reaches EP-05, EP-11, EP-12 and EP-13, which have no sheet. The overwrite line
appears once. **The status line named the refusal instead of going blank**, which is guard 2 from
the round before on its first real refusal.

### Never observed in Revit

The three changes of this round. No corrected subtotal has been written into a workbook, no
timing has been printed by a real run, and no shortened status line has been seen on the pane.
The subtotal rule is tested against the four measured groups and the timing against a report the
test builds, neither against Revit.

### Not touched

The Drawing Sheet. `Core/Shared`. The schedule gathering, which is measured and not changed.

---

## 2026-09-10, fortieth pass. Two guards off the audit, and nothing else

Pull request 52, merged into main as `1430f22`. **The runner executed 927 tests against its
merged head, 0 failed and 0 skipped. Locally the same 927 ran, 0 failed and 0 skipped**, after
the last file was written and after both break watches were restored byte for byte. 904 before,
23 added.

The merge went through the API with the title and the message both passed on the call, which is
the remedy `territory.md` measured. **The merge commit came back off main byte for byte, with no
co-author credit line and no generated-by footer.**

**Audit findings 1 and 8 are fixed. The other 27 in `steps/audit-kpi.md` are untouched**, and
this round did not renumber, reorder or annotate any of them. The Drawing Sheet was not opened.

### Nothing may delete a template

Finding 1, the only BLOCKS in the KPI half. `Patched` deletes the output file before the copy,
the name box is prefilled with the template's own file name through `OutputName.Suggested`, and
since the browsed output folder landed that folder can be the templates folder. Point it there,
press Create, and the client's GRP KPI Checklist was gone. No copy, no undo, every later run of
that template impossible, and the pane said only that the workbook could not be written.

**Two guards, because either alone is one refactor from being bypassed.** One in
`KpiRequestHandler.Patched` before the delete, one in `WorkbookPatcher.Patch` before it opens
anything. Both ask `FilePaths.Compare` and both refuse with the one sentence in
`CreateWords.WouldOverwriteTheTemplate`, which names the file it would have written over and
says that file is the template the run was about to read.

`SamePath` has three answers rather than two. A path that cannot be resolved is not a path that
is different, so `Unreadable` refuses the same way `Same` does. The comparison is the absolute
canonical form of each, compared without case, with any trailing separator off because
`GetFullPath` keeps one.

**The limit is written down rather than assumed away.** The comparison is textual, so a
junction, a symbolic link, a substituted drive or an 8.3 short name still reaches one file under
two names that do not resolve to one string. Asking the file system for an identity means
opening both files, which is the thing being guarded against.

**The collision is now visible before the press.** When the output folder and the template
folder are one folder, `TemplateWords.OutputIsTheTemplateFolder` says so under the output folder
line. It does not refuse, because writing a differently named workbook into that folder is
allowed and the per file guard is what refuses the press that is not.

The class is `FilePaths` and not `FilePath` because `Autodesk.Revit.DB.FilePath` is a real type
and the Revit half of the guard would not compile beside a Core class of that name. That is
written in the file so nobody renames it back.

### A refused run must say why

Finding 8. `CreateWords.Wrote` fell to `Refused(run.Reconciliation)` whenever nothing was
written, and that answers the empty string when the accounting added up. So a run whose
accounting passed and whose patch was refused **set the status line to nothing at all.** The
commonest cause is the output workbook still open in Excel from the run before, which the delete
answers with an IOException. The pane went from Creating to blank, and silence after a press
reads as success.

`WhyNothingWasWritten` is never empty. The accounting speaks first, because it refuses before
anything is copied. Then the patch's own refusal. Then `NoReasonRecorded`, which says in those
words that nobody recorded a reason and that it is a bug in the tool.

`CouldNotBeWritten` says what to do before it says what Windows said, because the system's own
message names a process rather than a thing to do. The line now opens with closing Excel and
pressing Create again.

The assertion is over every shape a run with no file can take rather than one test per case,
which is what the finding asked for: no run with `Written` false can produce an empty line.

### What was broken to see the tests go red

Both restored byte for byte and checked with md5, and the suite rerun green at 927.

- the guard taken out of `WorkbookPatcher.Patch`. 3 red:
  `WritingOverTheTemplateIsRefusedAndTheTemplateIsUntouched`, `TheSamePathReachedTwoWaysIsRefusedToo`
  and `APathThatCannotBeCheckedIsRefusedRatherThanRisked`
- `Wrote` put back to `return Refused(run.Reconciliation);`. 3 red:
  `NoRunThatWroteNothingEndsWithAnEmptyStatusLine`, `AnAccountingThatPassedAndAPatchThatDidNotSaysWhatToDo`
  and `AnOutcomeCarryingNoReasonSaysThatIsABugRatherThanSayingNothing`

### Never observed in Revit

**Everything in this round.** No guard here has been seen to refuse in Revit, no status line has
been seen to print, and the pane line about the two folders being one has never been drawn. The
tests are over the decision and over the patcher against a workbook they build themselves, not
over Revit. The one Revit side call, the guard in front of the delete, is reached by no test at
all: it is the same comparison and the same sentence as the Core one, and that is the whole of
what says it is right.

### Not touched

The Drawing Sheet, in any file. The other 27 audit findings. `Core/Shared`, which a KPI round
may not touch without every other session being stopped first.

---

## 2026-09-10, thirty seventh pass. Excel showed zeros, and the workbook stops going beside the model

Pull request 46, merged into main as `6f2e521`. **The runner executed 904 tests against its
merged head, 0 failed and 0 skipped. Locally the same 904 ran, 0 failed and 0 skipped**, after
the last file was written and after every break watch was restored byte for byte.

Seven things came out of the first real run. **Three of them were already delivered in pull
request 44** and were not done again: the typed date and name boxes reaching the fill, the area
claim, and most of the measured record. What is here is the other four and two additions to the
record.

### Excel showed zeros where the numbers were right

The filled MOSQUES workbook read 0 for Total Green cover, Canopy Area, Total Trees, Total Trees
Native, Total Trees Adaptive, Total Planting Area and Total Lawn Area, beside Planting 410, Lawn
60 and Mosques Area 3,729 which all read correctly. **The values were not wrong. They were stale
cached results and Excel never recalculated.** Total Planting Area is `=F10` and F10 held 410, so
a 0 there could only be a cache.

`fullCalcOnLoad="1"` was already set, so **the flag alone was never enough.** Three things
together, measured on the output: `calcId` set to 0 in `calcPr` with the flag kept, the cached
`<v>` dropped from every formula cell in every sheet leaving the `<f>` alone, and
`xl/calcChain.xml` removed.

**The output is checked the way the written cells already are.** `CacheCheck` is read back off
the file, not off what was sent: recalculate on open, calcId cleared, no formula cell carrying a
cached value, and the calc chain gone. All four or the report says the file may open showing the
template's own numbers and calls it a bug in the tool. A workbook that opens showing zeros beside
correct inputs is the worst thing this tool can produce, because it looks finished.

**The part count reads 37 in and 36 out now and the report says which part went and why.**
`PartsDeliberatelyRemoved` is what keeps the kept-every-part check true across a removal on
purpose, so a count short by one does not read as a loss.

### Unmatched species go into the workbook, which reverses last round's rule

A species Revit holds that the workbook's list does not is **written in**, the botanical name in
column D and the count in column B and nothing in any other column. On DM-12 that is three, all
under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2 and WASHINGTONIA ROBUSTA 1. Last round they were
named and written nowhere, and the workbook read 31 trees where the model holds 39.

**The empty rows come from the file and never from a constant.** The MOSQUES map entry stops at
row 83 and that sheet's own total is `SUM(B4:B92)`, so rows 84 to 92 are empty AND summed. A range
taken from the map would have found no room at all. `SpeciesList` reads column D across the whole
sheet and finds the total by its own formula, so the rows that reach the total are the rows a
species can be written into. No total found means no empty rows, and a species is reported as not
placed rather than written where nothing adds it up.

More unmatched species than empty rows writes what fits, names the rest and says plainly that the
sheet ran out of room. The report's section listing them says where each one landed instead of
that it was written nowhere, with the three lines saying what a written row does not carry.

Three tests in `SpeciesMatchingTests` went red on this and had to be rewritten, because they
encoded the rule being reversed. They are named here rather than quietly updated.

### The plot prefix is a second route, and it does not decide

Confirmed by the team: STREETS NS, ST and MM, PARKING PL, MOSQUES FM and DM, SCHOOLS SC,
EXISTING PARKS EP, FUTURE PARKS FP, HEALTHCARE HF. It agrees with the eleven component values
prefix by prefix with nothing left over on either side, and a test written out by hand says so.

**Two records of one fact is the fault this repo has met eight times, so they do not get equal
standing.** `PRX_Component` decides. Where the prefix agrees the pane says so, where they
disagree neither decides and nothing is preselected, and a prefix the table does not hold cross
checks nothing, which is different from one that disagrees.

**The line saying which route the answer took is shown whichever way it went.** It used to appear
only when nothing was preselected, so a preselection arrived without a word. EP-05, EP-11, EP-12
and EP-13 are on a schedule and on no sheet, which means no component at all, and the prefix is
the only thing that can place them.

**The grouping is what the prefix is really for.** One button per template beside Select all and
Clear ticks every plot of that template at once, with its count on the button. It replaces the
ticks rather than adding to them, because one checklist is one template. Plots whose prefix the
table does not hold are named under the buttons, so a plot no button reaches is visible.

**One thing to raise: the ask says eight prefixes and the table in it lists ten.** NS, ST, MM,
PL, FM, DM, SC, EP, FP and HF. Ten are built and ten are tested, one line per prefix. Seven
templates either way, since STREETS takes three and MOSQUES takes two.

### The output folder is browsed for, and the model no longer has to be saved

Writing beside the Revit model meant a detached model could not be used at all, which cost most
of an afternoon. `OutputFolder` is browsed for and remembered in `kpi-output-folder.txt` beside
the installed assembly, the same way the template folder is. Both go through one
`RememberedFolder` rather than two copies of the same quiet read, and `install.ps1` creates the
new pointer empty and never overwrites one the user has set.

**Create no longer asks whether the model has been saved.** It asks whether a model is open and
whether an output folder is set. The never saved refusal is gone and so is
`TemplateWords.NoModelPath`. The silent overwrite and the editable name box are unchanged.

**The model's folder came off `OpenModel` with it.** Nothing read it once the output folder
existed, and a value on the screen that decides nothing is how one stale string became a dead end
here already. The rule it was built for still stands: the title is a record built from one answer,
and the refusal is decided at the moment Create is pressed, the document off the live document
and the folder off the pointer file in the same breath.

The test that covered the never saved refusal is rewritten to assert the reversal outright: a
model that was never saved is refused nothing, and its refusal is the same as a saved model's.

**One line went with it.** The pane drew `TemplateWords.Output` and `CreateWords.Overwrite`
directly under each other, both saying the file is overwritten without asking. Two sentences for
one fact. The second is deleted.

### Open, and not to be guessed at in code

**THE CLIENT'S SPECIES LISTS ARE SHORT OF TREES THIS PROJECT PLANTS.** The MOSQUES list holds 80
species and none of them is Phoenix dactylifera, Washingtonia robusta or any Unknown row, checked
against the output file itself.

**A species written into an empty row carries no family, no genus and no native flag**, because
those are the client's data and the tool does not know them. The counts reach the total now, and
the KPIs that need those columns still cannot see it. Whether the lists should grow, or those
columns be filled some other way, is a question for the team about their own template.

Nothing in the tool may ever place an unmatched species by guessing. The name and the count go
in, and no other column does, whatever the totals look like.

### Measured, from the first real output

DM-12 on the MOSQUES template. 37 parts in, 37 out, 4 changed, zero recalculation errors. The six
values landed and the client's own formulas gave 28.1 percent canopy against a 13 percent target,
Excessive, NOT COMPLIANT, and the tool wrote no verdict anywhere. ACACIA / VACHELLIA FARNESIANA
found Acacia / Vachellia farnesiana at row 11. Three species were not in the list. Excel showed
zeros for seven computed cells while the inputs beside them were right.

With the calc chain removed on purpose it reads 37 in and 36 out.

### What was broken to see the tests go red

Each restored byte for byte and checked with md5.

- The grouping button adding to the ticks rather than replacing them, and the unknown prefix left
  in the button list. 3 red
- The output folder dropped from the refusal. 4 red
- Earlier in the round, the four on the cache, the empty rows and the written species

### Not touched

The Drawing Sheet, in any file.

---
## 2026-09-09, thirty sixth pass. The first real workbook, and two things it showed

Pull request 44, merged into main as `116afdb`. **The runner executed 867 tests against its
merged head, 0 failed and 0 skipped. Locally the same 867 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

**The first workbook is written and correct.**

### The date and name boxes did not reach the fill

E5, G5 and H5 came out holding the template's own placeholders, and the report said nobody had
typed them, on a run where the user had typed 2026-09-09, xx and bb into the three boxes before
pressing Create. **What the boxes hold and what Create reads were two different things.**

Two links were missing at once. `KpiCreateAsk` did not carry the three at all, so nothing the
pane collected left the pane. And `KpiCreatePlan.Of` defaulted all three to null, so the handler
calling it without them read as a deliberate empty rather than as a caller that forgot.

Both are fixed, and **the three are required arguments now.** A caller that forgets them does not
compile, which is the guarantee a test cannot give. Three tests cover what a typed value does:
it reaches the cell, an empty box is still recorded with its reason, and the surrounding space
comes off and nothing else does.

The pane's own half, box to `KpiCreateAsk`, is Revit code and no test here reaches it. What is
tested is the decision and the plan.

### The area claim was not true of the area

The report ended with "Every number above came off a row the schedule printed". H7 took
3728.7570000000005, converted from the raw 40136.006313679296 square feet, and the schedule
prints 3729. **The conversion is right and more precise. The sentence was wrong about it.**

The area comes off `PRX_Intervention Area` on the chosen filled region in the 00 link, which is
not a schedule row at all. The region table now carries the raw reading, the written metres and
what the model prints, side by side and unrounded, so the two can be held against each other,
and the closing paragraph makes the schedule claim for the numbers it is true of and names the
area separately.

Rounding the raw number for reading would have hidden the difference the row exists to show, so
it prints round trip. A break watch on that alone turned two tests red.

### What the first real output measured

One press of Create on DM-12 with the MOSQUES template. All of it is in `kpi-rules.md`.

- 37 parts in, 37 out, 4 changed, and the output recalculates with ZERO errors. The 45 against
  44 recorded earlier is EXISTING PARKS, a different template, and both stand
- The six values landed and the client's own formulas ran on them: 28.1 percent canopy against a
  13 percent target, Excessive, NOT COMPLIANT. The tool wrote no verdict anywhere
- The slash case matched. ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana at
  row 11
- Three species were correctly refused. The MOSQUES tree list holds 80 species and not one is
  Phoenix dactylifera, Washingtonia robusta, or any Unknown row, checked against the output file

### Open, and not to be guessed at in code

**THE CLIENT'S TREE LIST IS SHORTER THAN THE MODEL.** The workbook reads 2 existing trees where
the model holds 10, and 31 in total where the model holds 39, because the MOSQUES list has no row
any of the three refused species can match.

**The tool is right and the list is short.** This is a question for the team about their own
template, not a thing to fix in code. **Nothing in the tool may ever place an unmatched species
by guessing**, whatever the totals look like: a quantity put in the nearest row is a number
nobody can trace and every one of the 80 rows would then be suspect. The three are named in the
report with their counts and the numbers stay as they are until somebody answers.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- The typed date dropped again. 5 red
- The old schedule claim put back. 2 red
- The raw number rounded for reading. 2 red
- The printed value dropped from the region row. 2 red

### What has not been run

The pane change, box to `KpiCreateAsk`, has not been through Revit. Everything else in this round
is Core and covered.

---
## 2026-09-09, thirty fifth pass. Two more off the KPI pane, and one of them was never working

Pull request 43, merged into main as `61ea270`. **The runner executed 857 tests against its
merged head, 0 failed and 0 skipped. Locally the same 857 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

### The reference sample showed the wrong plot

With DM-12 ticked the block under Reference printed DM-11's four values, and the same four with
all 155 ticked. `ReadThePlots` took `plots.All[0]` and read the four for that one plot.

The block exists so a person picks the reference parameter by looking at its value rather than
its name, so a value belonging to a plot they did not choose defeats the whole of it.

`ReferenceValuesPerPlot` reads all four for every plot in one pass over the sheets, which is the
shape `ValuePerPlot` already used for the component. The pane shows the first ticked plot's, with
the plot named above them, and shows none and says so when nothing is ticked.

### Create stayed grey after the model was saved

**This path has never worked in Revit.** A detached model with no path, Create refused, the model
saved to a real folder, and Create stayed grey saying No model is open. A KPI Scan after the save
made no difference.

Three things were wrong at once and each on its own would have been enough.

**The folder was a copy taken once.** `Found` set it from the plot read and nothing re-read it.

**The title was a second copy on a different schedule.** `Scanned` set it and nothing else did,
so the two halves of one fact went stale independently.

**The pane asked for the model and threw the request away.** `Shown` called `Ask(WhichModel)`
then `Ask(Plots)`, and `Ask` holds ONE SLOT, so the second overwrote the first every time. The
model name request never ran from that path at all.

The fix is one rule, and it is now in `CLAUDE.md`: **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR.**

- `OpenModel` is one record, title and folder together, built from one answer. `CannotCreate`
  takes it rather than two loose flags, so nothing can hand the pair over the wrong way round
- Every answer from the handler carries the model state, whatever was asked for, read off the
  live document at that moment
- `RedrawTemplates` asks for it every time it draws, and `Took` redraws only when the answer
  moved, so the ask does not chase its own tail
- `Ask` never lets `WhichModel` take the slot from anything, because it is now the request most
  likely to arrive on top of another, and losing one costs nothing
- **Create is greyed out on what the PANE owns and nothing else**, a template picked and a plot
  ticked. Whether a model is open and whether it has a folder are decided on the Revit thread
  against the live document when the button is pressed

That last one is what removes the class of fault rather than narrowing the window. A button
greyed out on a fact the pane does not own can always go stale, however often it is refreshed.

### What the test covers, and what it does not

Seven tests over `CannotCreate` and `OpenModel`, across all three states: no document, a document
with no path, and a document with a path. Each gives its own line, the no-path line says to save
the model, and a folder arriving with no title is still no model.

**These are tests over the decision, not over Revit.** Nothing here proves that a saved model
arms Create in Revit, because nothing in this repository can run Revit. What is proven is that
the decision is right for all three states and that it is now made from the live document rather
than from a copy. The Revit half is unrun and stays unrun until somebody presses the button.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 3 red
- A folder with no title made to count as a model open. 1 red
- The reference block made to name no plot. 2 red
- A changed folder made to read as the same model. 1 red

The last one matters because `Took` decides whether to redraw on it. A folder change that read as
no change would leave the pane showing the state from before the save, which is the fault again
one layer down.

---
## 2026-09-09, thirty fourth pass. Five faults off the first real run of the KPI pane

Pull request 42, merged into main as `36dc0f8`. **The runner executed 849 tests against its
merged head, 0 failed and 0 skipped. Locally the same 849 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored byte for byte.

The pane reached Revit and five things were wrong with it. **Every one of them is a fault
nothing in the suite could have caught**, because each is about what reaches the screen rather
than about what the code computes. 849 tests locally, 0 failed and 0 skipped, after the last
file was written and after the five break watches were restored byte for byte.

### It said no model was open while a model was open

The header read 96,959 elements at 16:08:14 and Create said Cannot create. No model is open, on
a detached model that has never been saved. The line directly above the button already had the
truth.

`CannotCreate` was handed the model's FOLDER and called it the model. **Whether a model is open
and whether it has a folder are two facts**, so it takes both now, a model that is not open is
not asked whether it has been saved, and the two never print together. The never saved words are
`TemplateWords.NoModelPath`, the very line above the button, rather than a second sentence about
one condition. `KpiRequestHandler` is the other caller and it reads the folder off the document
rather than assuming one, because it is the second place that could get the pair the wrong way
round.

That is the eighth time in this repo that two records of one fact have been the bug, and
`CLAUDE.md` counts it as the eighth.

### The pane showed five parameter names that no model holds

PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. WPF reads the first
underscore in a button's text as an access key marker, swallows it and underlines the next
letter. **The strings were right in the code and wrong on the screen**, on the one tool whose
whole job is exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` on this pane goes through it: eleven places, not the five
that were noticed. Only what is drawn goes through it and nothing compares the escaped form
against anything.

The test walks every name `KpiNames` holds and asserts what WPF renders is the name itself,
rather than the five that happened to be seen.

**The Drawing Sheet has the same fault and this round does not touch it.** Its view type names,
plot identifiers and column headers all go onto buttons and tick boxes the same way. It is out
of scope by instruction and it is written down here rather than left to be found again.

### It described itself doing something it does not do

`KpiTemplates.SourceOf` said PRX_COMPONENT, read off the title block and PRX_Plot_UID2, read off
the title block. **Every part of both was wrong.** PRX_COMPONENT is in no model. The value is
PRX_Component on the sheet. PRX_Plot_UID2 sits on 1233 title block instances and holds a value on
none of them, while the sheet holds 1384 of them. It was the workbook's own note put on screen
as though it were the tool's behaviour, and the reader had been corrected rounds before.

It takes a `ChosenParameters` now and names the three parameters the pane's own pickers hold,
because those are the ones that will really be read, with the place each is read from. Nothing
picked yet names the picker to look at rather than a parameter nobody chose. A test walks every
one of the seven templates and refuses any line holding PRX_COMPONENT or the words title block.

### The preselections were positional

Reference started on PRX_Plot_ID, which is first of the four and is not what the note names.
Location started on whichever neighbourhood parameter sorted first, and Neighborhood Group sorts
above Neighborhood Name.

`Preselected.From` takes the name the note asks for and the names the model offers, and hands
back that name where it is offered. Reference starts on PRX_Plot_UID2 and Location on
Neighborhood Name, both now measured constants rather than positions. Component goes through the
same one rule.

### Prepared by was cut to Prepared b

`PanelMetrics.WideLabelWidth`, added rather than a widening of the shared `LabelWidth`, which the
Drawing Sheet uses in two places. **The number has not been seen in Revit** and is the one thing
in this round chosen by eye rather than measured.

### The plot picker, and what I could not reproduce

**All 155 plots ticked by default is not what the code does, and I could not find a path that
would.** A fresh `PlotTicks` is built with no ticks, `Found` carries the previous ticks across
and the previous set is empty, and `All()` is reached only by pressing Select all. Three tests
now pin it: a fresh picker over 155 plots reads 0 of 155 plots ticked, reading the model carries
nothing across, and Select all is the only route to all of them.

So the state is pinned and cannot drift, but **the fault as reported is not explained**, and
saying it is fixed would be a guess dressed as an answer. If it is seen again on a pane nobody
has pressed Select all on, the next thing to look at is whether `Found` runs more than once with
something already ticked.

### What was broken to see the tests go red

Five, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 1 red
- The escape made to return its text unchanged. 5 red
- The preselection put back on position alone. 2 red
- The note shown as the tool's behaviour again. 3 red
- A fresh picker made to tick every plot. 6 red

### What has not been run

None of this has been seen in Revit. Four of the five fixes are Core with tests over them and
the fifth, the label width, is a number in a pane file that only Revit can settle.

---
## 2026-09-09, thirty third pass. The component to template mapping, as a table

Pull request 41, merged into main as `a0d3d3b`. **The runner executed 823 tests against its
merged head, 0 failed and 0 skipped. Locally the same 823 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The question that has been open since the 1355 run is answered. The team measured it on the
1548 scan, 11 distinct values over 1384 sheets, off the component values block the round before
last added to the end of section 3, and it is now a table in Core.

```
DAILY MOSQUE           MOSQUES          NH STRT LESS 20m ROW   STREETS
FRIDAY MOSQUE          MOSQUES          NH STRT 20m ROW        STREETS
SCHOOL                 SCHOOLS          STREET 30m ROW         STREETS
HEALTH                 HEALTHCARE       STREET 36m ROW         STREETS
PARKING LOT            PARKING
EXISTING PARK          EXISTING PARKS
FUTURE PARK            FUTURE PARKS
```

### A table, never a string rule

`ComponentTemplates` holds the eleven and `TemplateForComponent` reads it and nothing else.
Matching is the whole value compared without case and with surrounding whitespace off, the same
plainness species matching keeps. No part of a value matches anything.

**The rule it replaces was wrong twice over.** It matched a word of the component against a word
of the template name, so PARKING LOT looked like a park because PARKING begins with PARK, and
all four street values answered nothing at all. A test in the round that wrote it asserted PARK
offered EXISTING PARKS, FUTURE PARKS and PARKING and called offering all three the honest
answer. It was honest about a rule that should not have existed.

It is many to one. Two values mean MOSQUES and four mean STREETS, and a test says so in those
numbers. Another says every one of the eleven resolves, written out by hand. Another says every
one of the seven templates is reached by at least one value, because a template no value reaches
could never be preselected and nothing on screen would show the hole.

### The park tie is broken by the model

EXISTING PARK and FUTURE PARK are separate values, so each preselects its own template. The pane
used to put that pair to the user always, and the file name was the only thing that could tell
the two workbooks apart. Recognising a workbook FILE is still the sheet name then the file name.
That is a different job and it did not change.

### The plot prefix is read nowhere

STREET 36m ROW covers MM and ST plots and NS carries two different street widths, so a rule on
the prefix would answer three of the eleven wrongly. A test preselects STREETS for one value
across MM and ST and for two different values on NS.

### A value the table does not hold

It preselects nothing, the pane says which value and that the table does not know it, and the
user picks. **The pane used to say nothing at all**, because `Preselect` returned on
`NeedsAPick` without showing the reason, so a tool that had looked and found nothing read exactly
like a tool that never looked. The line is `TemplateChoice.Why`, built in Core, and it is cleared
by every path that makes it untrue: a hand pick, a folder change and the next preselection.

The same move found one more. `_pickedAs` survived a preselection that failed, so ticking a
mosque plot and then an unmapped one left Create armed with MOSQUES under a line saying nothing
was preselected. Nothing is picked by hand on that path, because `Preselect` returns above when
something is, so what it held can only have come from plots that are no longer ticked. It is
cleared. Nobody would have seen it while the pane said nothing.

Section 3 of the report gains a template column, which is this table read back. Its closing line
used to read that nothing in the tool turns a value into a template name, and that is no longer
true, so it says what the table is instead. A value the model grows later prints as one the
table does not hold, which is the whole reason the block prints every value rather than a sample.

### Recorded rather than built

**The road width the STREETS template asks for by hand is inside the component value.** 20m in
NH STRT 20m ROW, 30m and 36m in the two STREET values, and less than 20m in NH STRT LESS 20m ROW.
`TemplateWords` already says the road width and the total length are typed by hand and the sheet
works the area out, which is why STREETS is the one template with no area cell. Nothing reads the
width out of the value and nothing here started to. It is written down because the value carries
it and somebody will want it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The table made to match on part of a value again. 3 red
- STREET 36m ROW dropped from the table. 4 red
- FUTURE PARK pointed at EXISTING PARKS. 3 red
- An unmapped value made to fall back to the first template. 2 red
- The report's template column made to print the value. 2 red
- A template the caller does not offer preselected anyway. 1 red

### What has not been run

The table and the reader are Core and covered. **The pane change is not.** The line saying why
nothing was preselected has never been seen in Revit, and neither has a preselected FUTURE PARKS.
Nothing in `RcrcGreen.Revit` has been run on this machine.

---
## 2026-09-09, the rule behind the last three rounds, written down

Pull request 40, merged into main as `5a5ded0`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written. No source file changed, so it is the suite that merged as `973c817`.

**The 404 rule the entry below records only works one way.** A log that comes back is a job that
has ended, and that held again. A 404 is not a job still running: every step of this job read
completed with conclusion success while the log was still 404 more than a minute later. The step
conclusions are the read that was right both times.

No code change. One rule into `CLAUDE.md` and a pointer from each of the two rules files.

**NEVER READ A SCHEDULE VALUE BY CELL POSITION. Ask the heading row which column it is. A
reader that cannot find its column says so rather than falling back to a position.**

Three rounds, four readers, one fault. `SoftscapeRows` took the botanical name off cell 0 and
the count off the last number. `ScheduleGroups` decided a row was named off cell 0.
`ShrubsAndLawnRows` read a species row off cell 0. The first column is the image and an existing
species prints with none, so every one of them was invisible on a plot whose rows all carry
photos, which is why each round found only the one in front of it.

`kpi-rules.md` said it as a fact about these schedules and `core-rules.md` did not say it at
all, so a reader touching `ScheduleRows` from the Core side met the rule nowhere. It is in
`CLAUDE.md` now, said once, and both files point at it rather than restating it.

### The half of the rule the code does not yet keep

The first sentence is kept everywhere. **The second is not**, and this round changed no code, so
it is written down rather than quietly true. Four readers still fall back to a position when the
heading row names no column:

- `ScheduleGroups.IsNamed` falls back to `row[0]`
- `SoftscapeRows.SpeciesIn` falls back to `row[0]` for the botanical name
- `SoftscapeRows.QuantityIn` falls back to the last whole number in the row
- `ShrubsAndLawnRows.SubtotalsIn` falls back to cell 0 for the species test

None has ever fired on a schedule these readers have been run against: the softscape and shrubs
heading rows both name BOTANICAL NAME, AREA and COUNT, so no report has come off a fallback. `ShrubsAndLawnRows` already shows the shape the rule wants for the other
half: no AREA column and it hands back nothing rather than guessing at one.

Taking the four out is a code change and was not asked for. It is open.

---
## 2026-09-09, thirty second pass. The group counter, the switch and the component values

Pull request 39, merged into main as `973c817`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The gate endpoints went stale again, the fourth round running. The job LOG settled it the same
way it did last round: it returns 404 while the job is running, so a log that comes back at all
is a job that has ended, whatever the status field still says.

The remote branch still held last round's commit, which main already carried under a different
hash as `82dd51d`. The two trees were identical, so the branch carried nothing but merged
history and was restarted from main with a force-with-lease pinned to that old commit. Nothing
unmerged was on it to lose, and that was checked rather than assumed.

One bug and two report faults from the 1521 scan, and nothing else.

### The group counter read the image column

Question 8 reported DM-12 Existing with 0 named rows of 6 and DM-13 Existing with 0 named rows
of 2, while section 6 of the same file printed five species under DM-12 Existing totalling 10
trees. A file that contradicts itself twenty lines apart has nothing in it worth believing.

**The first cell is the image and an existing species prints with no photo**, so its first cell
is a dash. `ScheduleGroups` decided a row named something by looking at that cell. It now asks
the heading row which column is BOTANICAL NAME and reads that one, falling back to the first
cell only where the heading row names no botanical column at all.

This is the same fault fixed in the softscape reader one round ago. The reader was corrected
and the counter, which feeds the same report, was left on the old rule. **Two rules for one
question is the shape this repo has now met seven times**, and `CLAUDE.md` records it as the
seventh.

Checked against the measured counts, each written out by hand in the test rather than worked
out with the code's own rule: DM-11 Proposed 3, DM-12 Existing 5 and Proposed 3, DM-13
Existing 1 and Proposed 2. A fifth test holds `SoftscapeRows.SpeciesIn` and `ScheduleGroups.Of`
against each other on one schedule, because the two disagreeing is what produced the fault.

### Every place that could hold the same shape, including the ones that were already right

Four things in Core read a schedule's printed rows. All four were read line by line.

- `ScheduleGroups.IsNamed`, the old `FirstCellHoldsText`. **THE BUG. Fixed**
- `ShrubsAndLawnRows.SubtotalsIn`, the test telling a species row from a subtotal row.
  **THE SAME BUG AND NO REPORT HAD SHOWN IT. Fixed**, because DM-11's shrubs are all Proposed
  and every one of them prints with a photo. An existing shrub would have been read as a
  subtotal, which would have gone into the workbook as an area
- `ScheduleColumns.IsStructureRow`. Reads the first cell, already right. A group heading and a
  phase row really do sit in the first cell with every other cell empty, which is measured off
  the 1355 rows and written in `kpi-rules.md`
- `ShrubsAndLawnRows`, the group name taken from `row[0]` after `IsStructureRow` has passed.
  Already right. That row has exactly one cell with text in it and this is that cell
- `ShrubsAndLawnRows`, the first cell tested again to keep TOTAL out of the subtotals. Already
  right and still needed. A subtotal names nothing anywhere, and the first cell is the one
  thing TOTAL does carry. It is reached only after the botanical test now
- `SoftscapeRows.SpeciesIn`, the botanical name. Already right, fixed the round before
- `SoftscapeRows.QuantityIn`, the last whole number in the row. Already right. It runs only
  where the heading row names no COUNT column, and the schedules in this model all name one
- `ScheduleGroups.GroupNameIn`. Already right and position independent: the one cell holding
  text, wherever it sits
- `KpiReport`, the rows as printed. Nothing to get wrong. It prints every cell in order and
  picks no name and no number out of them

`SpeciesMatching` also has a Rows, and it is the workbook's species list rather than a
schedule's. Each of its rows carries a named BotanicalName, so it is not this shape at all.

### KPI COMPONENT S/H is a switch, not a candidate

Its name holds COMPONENT, so section 9 offered it beside PRX_Component as another name the
model might carry the component under. It reads No on 9 of 9 title block types and is a show
and hide toggle.

`KpiNames.EveryValueIsYesOrNo` decides it, one method with both callers asking it. Section 9
leaves such a name out of the answer. Section 3 keeps it, in its own tally, in its own values,
and again by name in the new component values block, which says why it is not counted there.

**The section that offers it is not question 8.** The near miss list feeds questions 1 and 2,
the two about where the component lives, and question 8 never calls it. The fault is real and
the number in the request is off by six, so it is written down here rather than fixed silently.

### The component values do not match the template names

Measured: FRIDAY MOSQUE, SCHOOL, HEALTH, EXISTING PARK, NH STRT 20m ROW. The templates are
named existing parks, future parks, healthcare, mosques, parking, schools and streets. None of
the five is a template name and no string rule turns HEALTH into healthcare or NH STRT 20m ROW
into streets.

Section 3 now ends with every distinct value of the component the model holds, how many sheets
carry each, and every plot those sheets are for. Uncapped, where the rest of that section shows
twenty examples, because twenty sheets is not enough to build the mapping from and the mapping
is what preselects the template. The plot beside each value is PRX_Plot_ID read off the sheet
and the block says so. A value on sheets carrying no plot is counted and named. A component
name on a title block type is named with its reason and left out, because a type is not a sheet.

**Nothing maps a value to a template and nothing guesses one.**

### Open, and not to be guessed at in code

**Which template each value of PRX_Component means.** Five values are measured and none is a
template name. This is the question that stops the pane preselecting a template, and the new
block is what somebody answers it from. Until it is answered, no code anywhere turns one into
the other.

**SETTLED the round above.** The 1548 scan read eleven values off this block and the team turned
them into a table. It is `ComponentTemplates` and the entry at the top of this file has it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The group counter put back on the first cell. 3 red
- The shrubs species test put back on the first cell. 2 red
- `EveryValueIsYesOrNo` made to answer false always. 6 red
- The component block made to find no plot for any sheet. 1 red
- A title block type made to count as a sheet. 1 red
- The nothing-found line made to print when something was found. 1 red

### What has not been run

Nothing here has been seen in Revit. The group counts, the switch rule and the component values
block are all Core, all covered by tests, and none has been read off a real scan. The next 1521
run is what confirms DM-12 Existing comes back as 5 named rows of 6.

---
## 2026-09-09, thirty first pass. The shrubs and lawn shape, measured rather than guessed

Pull request 38, merged into main as `82dd51d`. **The runner executed 789 tests against its
merged head, 0 failed and 0 skipped. Locally the same 789 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored.

The check run and the job endpoints both reported the gate in progress for several minutes
after it had finished, the third round running. What settled it was the job LOG: it returns 404
while a job is running, so a log that comes back at all is a job that has ended. That is the
quickest honest way to tell a stuck gate from a stale endpoint.

Two corrections from the team before this reaches Revit.

### The shape was measured and I guessed it anyway

`kpi-rules.md` recorded DM-11 as GRASS 35 m² 46 then SHRUBS & GROUND COVER 70 m² 58, and the
round above read that as the heading sitting beside its numbers on one row. It does not. The
1355 scan report has the real rows and that report is not in this repository, because nothing
under `reports/` is ever committed, so it had never been read here. **The numbers were on record
and the shape was not, and a reader written to the numbers alone found no subtotal at all.**

The real thing is eleven columns wide and three things follow from it.

**The group heading is on its own row**, first cell only, every other cell empty. A phase row,
Proposed, sits under it in the same shape, so a structure row naming no wanted heading opens no
group.

**THE SUBTOTAL PRINTS TWICE.** 35 then 35, and 70 then 70. Adding a group's subtotal rows gives
70 and 140. One is taken, and two that disagree are named and refuse the write rather than being
chosen between.

> **CORRECTED ON 2026-09-10, THE FORTY FIRST PASS. THE PARAGRAPH ABOVE IS WRONG.** It was
> measured from DM-11 alone and DM-11 is the special case. A group prints ONE SUBTOTAL PER
> PHASE, then the GROUP TOTAL. Every DM-11 group holds one phase, so each prints two equal rows
> and that read as one subtotal printed twice. Taking the first row then took one phase and
> called it the group, which on the 0928 run over 20 plots meant 30 where the group is 84 and
> 361 where it is 820. The last row is the group's value and the check is that it equals the
> rows above it added together. The correction is left beside the claim rather than replacing
> it, because a wrong claim that quietly vanishes teaches nobody how it was arrived at.

**The species rows add up to the subtotal**, 36 plus 34 is 70, so the two are held against each
other and both are printed. That one is recorded rather than enforced: every number there is
already rounded to the metre on the way out of Revit, and a sum of rounded numbers need not
equal a rounded sum, so a refusal on it would fire on correct data.

TOTAL needed no special case in the end. It carries numbers, so it is not a structure row, and
its first cell holds text, so it is not a subtotal. It falls out on its own.

### Neither column sits where it could be assumed

Eleven columns wide, the first number in a subtotal row is the area and the last is L/DAY. So
`ScheduleColumns` finds the area, the count and the botanical name off the schedule's own
heading row.

**The softscape reader was changed too, which is one more than the two corrections asked for.**
It read the botanical name off the first cell and the quantity off the last number in the row.
On a schedule shaped like this one the first cell is an image file name and the last number is
L/DAY, so both would have been wrong. It now prefers the columns the heading row names and
falls back to the old behaviour only where it names neither. Whether the real softscape schedule
carries an image column is UNKNOWN and nothing here assumes either way. Say so if that change
was unwanted, it is one file.

### The unit, and nought

An area prints with its unit attached, 35 m², and a count does not. The unit comes off by
reading as far as the number goes rather than by stripping characters. **A real area can be
nought**: the hardscape schedule prints 0 m², which is the number and not an empty cell, and
that case is tested.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by md5:

- adding every subtotal row instead of taking one turned the two real row tests red
- letting a phase row open a group of its own turned the group test and the phase test red
- assuming the area column rather than reading the heading row turned five tests red
- dropping the disagreement refusal from `Reconciliation` turned its own test red

### The five workbooks

Five client templates arrived with the brief and **none is in this repository.** `*.xlsx` is
ignored at line 44 of `.gitignore` and `git add` was made to refuse a real one before anything
else was done. Nothing in these two corrections needed to read them.

### Still never observed

Everything the round above listed still stands except the shrubs and lawn shape, which is now
measured. Nothing here has been through Revit either: the new reader is tested against
transcribed rows rather than run against a schedule.

---

## 2026-09-09, thirtieth pass. The plot picker, several plots at once, and Create

Pull request 37, merged into main as `ce825b1`. **The runner executed 781 tests against its
merged head, 0 failed and 0 skipped. Locally the same 781 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API.

The check run endpoint reported the gate in progress for twelve minutes after it had finished.
Listing the run's jobs showed the truth, completed and successful 40 seconds in. The same lag
appeared last round. Read the job rather than the check run when a gate looks stuck.

The button the last four rounds deliberately did not add. It does something now.

### One checklist can cover more than one plot

This is the shape of the round. A checklist is not always one plot, it can be a whole asset
made of several, and then the workbook wants them added together. **Which plots belong to one
checklist is not written down anywhere and nothing here derives it.** No grouping by prefix, by
component or by anything else. The user ticks and the tool adds up exactly what was ticked.

The picker offers one plot, several, or all of them, from a list built as the union of two
sources: `PRX_Plot_ID` on the sheets and the `PRX_Ref Plot ID` filter value on the schedules.
Both lists are kept, the disagreement is shown on screen, neither wins. That is the seventh
place two records of one fact could have parted and it is held open on purpose.

### Adding printed numbers, and the rule that makes it safe

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed, and is a different thing from recomputing a number off elements, which is
never allowed and happens nowhere.

`Totalled` carries the per plot numbers and the total together and works the sum out again to
compare. **A total that does not equal its parts refuses the write.** The report prints each
plot's own number beside every total so the arithmetic can be checked by eye without opening
Revit. Trees merge on the group AND the botanical name: ALBIZIA LEBBECK is 1 existing and 13
proposed on DM-12, and merging on the name alone would put 14 into one tree sheet.

Two plots reporting an identical raw area are flagged and refuse the write until somebody
confirms. So does one plot whose two filled regions both hold an area, because which of them
carries it varies by plot and the type name cannot decide. The tool asks rather than picking.

### Matching a species

`SpeciesList` reads the workbook's own column D out of the template, resolving shared strings,
because a cell holding one stores an index and reading the index as the name would report every
species unmatched. Nothing in this repo carries a copy of the plant palette.

Matching is the botanical name compared without case and with surrounding whitespace off, and
nothing else. All three hard cases are tested: UNKNOWN against the four rows named Unknown Tree
matches none of them, the slash in ACACIA / VACHELLIA FARNESIANA is not split on, and the
apostrophe in BOUGAINVILLEA GLABRA 'PINK PIXIE' is part of the name. A name the workbook holds
twice is reported rather than placed on the first, for the same reason.

### The rule this round changed

`kpi-rules.md` said the tool never writes E5, G5 and H5, because the team types them. The brief
puts them on the pane as three boxes, so the tool now copies them through. The rules file and
`KpiTemplates` both say so now rather than one of them still saying the old thing.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by
md5:

- dropping the ticked-versus-read refusal from `Reconciliation` turned
  `APlotTickedAndNotReadRefusesTheWriteAndIsNamed` red
- merging species on the botanical name alone turned `TheSameSpeciesInTwoGroupsNeverMerges` and
  `TheGroupDecidesTheSheetAndNothingElseDoes` red
- comparing identical areas on the rounded metres rather than the raw turned both identical
  area tests red
- placing a name the workbook holds twice on its first row turned
  `ANameTheWorkbookHoldsTwiceIsReportedRatherThanPlacedOnTheFirst` red

One test caught a fault in its own fixture rather than in the code: every plot built by the
fixture had the same default area, so the identical area guard fired on a reconciliation test
that expected to pass. The guard was right and the fixture was wrong.

### Never observed, item by item

Nothing in this round has been through Revit. Every one of these is written down because it has
not been run, not because it is expected to fail.

- **No workbook has been filled.** `WorkbookPatcher` is proven by its own tests against a
  workbook the tests build, and the 37 parts in and 37 out was measured last round on the real
  EXISTING PARKS file. This round has not repeated it
- **No plot has been read out of a model.** `KpiPlotReader` compiles against the reference
  assemblies and has never run. Every method in it is untested, because the test project must
  never load the Revit API
- **The pane has never been drawn.** The plot picker, the three pickers, the three boxes and the
  Create button exist only as a mockup in `design/pr-37/kpi-create.html`
- **The schedule filter route has never run.** Schedules are found by the plot their filter
  names rather than by the plot in their own name. That is the more correct source by
  `CLAUDE.md`, and it has not been run once
- **The shrubs and lawn row shape is inferred, not measured.** CORRECTED BY THE ROUND ABOVE.
  The shape was measured all along, in the 1355 scan report, which is not in this repository
  because reports are never committed, so it had never been read here. Guessing it from the
  numbers put the heading beside them and the reader found no subtotal at all
- **The softscape total row's shape has never been seen.** A subtotal has an empty first cell
  on the fixture and is skipped. A grand total row carrying its own word in the first cell would
  read as a species here, come out as one the workbook's list does not hold, and be named in the
  report with its count. Visible, and written nowhere, but wrong
- **The three typed cells have never been written.** E5, G5 and H5 are new this round
- **`RememberedNames` has never read or written its file.** The two name boxes are meant to
  survive a Revit restart and that has not been shown
- **No refusal has been seen on screen.** The identical area confirmation, the region pick and
  the one-refusal-lists-everything line are all drawn in the mockup and never rendered
- **The output has never been overwritten.** The delete before the patch is written and unrun

### Open, and not to be guessed at in code

- **Which plots belong to one checklist is not written down anywhere.** The user ticks them.
  Do not derive a grouping rule from the prefix, from PRX_Component, or from anything else

Two from last round are still open and neither is answered here. The four rows named Unknown
Tree against the model's UNKNOWN, and the species names carrying slashes and apostrophes. The
code reports all of them unmatched rather than guessing, which is the honest half of an answer
and not the answer.

---

## 2026-09-09, twenty ninth pass. Two report faults from the 1355 scan, and the facts

Pull request 36, merged into main as `01d9886`. **The runner executed 722 tests against its
merged head, 0 failed and 0 skipped. Locally the same 722 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API rather than
the GitHub button the hook cannot see.

The scan ran on the 1355 model. Two things in the report were wrong in the same way, both
answering a question from the wrong place while the right answer sat a screen above in the same
file.

**Section 9 contradicted section 3.** Questions 1 and 2 read NOT FOUND for PRX_COMPONENT while
section 3 printed PRX_Component on the sheet with 1384 values reading FRIDAY MOSQUE and SCHOOL.
The exact name is not in the model and the near miss is, with the answer in it. Section 9 now
names a near miss that holds values, says which of the three homes it sits on and what its first
values are, and counts the question as answered, because the file does hold the answer. A near
miss with no value in it still reads NOT FOUND, which is the honest half of the old behaviour.
The near miss word for these two questions is COMPONENT alone, not the sheet list of COMPONENT,
PLOT and UID, because a question about the component is not answered by a plot name.

**Question 8 was answered by the wrong evidence.** It reported the elements by phase created,
which came back (none) 6, because the elements a softscape schedule lists are RVT Link instances
and their phase is the link's. The answer is in the printed rows. `ScheduleGroups` in Core finds
the rows that name a phase the document holds and nothing else, and question 8 is answered from
those, naming the plot that showed it. DM-12 prints a TREES row, an Existing group of five
species with a subtotal of 10, a Proposed group of three, then TOTAL 39.

A group row and a subtotal row are the same shape, one cell with text and the rest empty. Only
the text tells them apart, so the phase names read off the document decide it. Nothing matches
on the shape of a row, and the words Existing and Proposed are not in the code: a project whose
phases are named anything else reads the same way. TREES is a group row for the category rather
than the phase, so it is not counted, and nothing anywhere knows the word TREES.

The element phases are still printed, said plainly as the link instances the schedule lists
rather than the plants, and they no longer make the question count as answered. The test that
asserted they did is reversed and says why.

### The facts

Six measured facts from the 1355 run are in `CLAUDE.md`: PRX_Component on the sheet is the asset
type and picks the template, PRX_Plot_ID is the plot every schedule filters on with prefixes
tracking the asset type, what the other three plot parameters really hold, how the softscape
schedule prints and that a species appears under both groups, what an existing species prints
with, and that every plot has two filled regions in the 00 link with the area on either one.

`CLAUDE.md` was at its 200 line ceiling, so the room came from the Drawing Sheet prose that
`.claude/rules/core-rules.md` already holds in full. Two facts thinned in that pass were checked
first: PRX_Furniture Lenght is in `core-rules.md` verbatim, and Schedules and Quantities was in
no other file, so it was written into `core-rules.md` next to the schedule rules rather than
lost. The run history that came out is in this log in full.

Four of the five open questions in `kpi-rules.md` are settled by these facts and now read as
settled rather than open, which is the same contradiction the two report faults were. Question 3
was wrongly put: neither region type is the intervention area, it varies by plot.

### Open, and not to be guessed at in code

- **The workbook holds four rows all named Unknown Tree and the model prints a species called
  UNKNOWN.** Nothing can match those on name. Four identical row labels cannot be told apart by
  a name lookup, and a model species called UNKNOWN is not the same thing as an unknown row
- **Species names carry slashes and apostrophes.** ACACIA / VACHELLIA FARNESIANA and
  BOUGAINVILLEA GLABRA 'PINK PIXIE'. A match on an exact string will fail on both, and nothing
  written down says how the workbook spells them

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Five breaks watched red first and each file restored byte for byte, checked by md5:
matching group rows on shape alone turned the subtotal test and the two group row tests red,
dropping the near miss from question 2 and then from question 1 turned one test each red,
letting the element phases answer question 8 turned the reversed test red, and dropping the plot
name from the answer turned the two group row tests red.

The shared real model fixture now prints the grouped shape rather than four flat rows, because a
fixture calling itself the real model while printing something the real model does not is the
next round's wrong answer.

---

## 2026-09-09, the review of the twenty eighth pass. Six faults it found in its own work

Pull request 35, merged into main as `1b78c91`. **The runner executed 711 tests against its
merged head, 0 failed and 0 skipped. Locally the same 711 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The two counts are
the same number arrived at twice and are stated apart on purpose.

The squash carries no co-author line and no generated-by footer this time. The one on `e343fe1`
got in through the GitHub squash button, which the commit hook cannot see. Supplying the commit
message through the API instead keeps it inside the rules the hook enforces, so that is how
this one was merged and how the next should be.

A five lens review over the round below, each finding then handed to a separate reader whose
job was to refute it. 58 raised, 24 reached the refuting step, 14 survived it, 10 were refuted
and 34 were never adjudicated. The 14 are six distinct faults once the lenses that found the
same one are put together, and all six are fixed here. Every one of them is this repo's oldest
shape: a count and the thing it counts read from two different places.

**A plot's regions were counted off the area values.** `ONE PLOT'S REGIONS TOGETHER` grouped
`InterventionAreas`, which the reader fills only where PRX_Intervention Area holds a value. A
region carrying a plot and no area was not in that list, so it vanished from its plot, and a
plot whose every region was blank was not a plot at all. The same page could say 279 regions
carry a plot and then group 266 of them under a heading claiming to show a plot's regions.
`LinkContents` now carries `RegionsCarryingAPlot`, every region with a plot whatever its area
holds, and the section groups that. A region with no area prints `(no value)` rather than
disappearing.

**A type carrying no plot had no row, so the answer none could not print.** The per type table
was driven by the types that carry a plot, and the reader only creates a key for a type when
one of its regions carries one. So RCRC_OUT OF SCOPE, carrying none, was absent from a table
whose own comment says telling a candidate from a non candidate is what it exists for. The
table is driven by every type in the link now and looks the carrying count up, so a type with
none prints a zero. A name in one list and not the other prints a line saying the tool is
contradicting itself, rather than a fabricated zero.

**Absent and blank both printed as an empty plot.** Nothing said which. PRX_Ref Plot ID now
has its own line counting the three states apart, the same shape the intervention area line
has, and a region carrying two parameters of that name is counted the way the area already was.

**The near misses printed once per missing name.** The list is a property of the home, not of
whichever wanted name went missing, and both wanted names are missing on the sheet, so the
whole block printed twice under two headings. Worse, `PRX_Plot_UID2` holds the word Plot, so it
was a near miss of itself and its values printed a second time under PRX_COMPONENT. The list is
worked out once and still names every name, because that line is a statement about the home.
The values print under the first NOT FOUND only, and a wanted name the home really holds is not
among them, because it prints under its own heading a few lines above.

**One schedule was recorded as passed over and read in full.** When no copy of a name lists an
element the reader falls back to the first, which it had already written into READS THAT DID
NOT HAPPEN as passed over. Two lines, one schedule, opposite meanings, every time the fallback
fired. The passed over lines are held back until the fallback has decided.

**The rules file told the next round to read one schedule per name.** `kpi-rules.md` held both
the old rule and this round's new one, in a file that loads as instructions whenever anything
under `Kpi/` is touched. `ChooseOnePerName` and its comments said one as well. One statement
stands now and it names `KpiReport.PlotsReadInFull` as the only home of the number.

**And the log entry below was wrong about its own test run.** It said stripping the brackets
turned eight tests red. Reproduced at `c370325` in a throwaway worktree, it turns 14 red across
10 methods, 696 passing. The eight was counted off the method names on screen instead of off
the run's own total, which is the thing this repo says never to do. Corrected in place.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. `dotnet test` after the last
file was written: 711 passed, 0 failed, 0 skipped, up from 710. Four breaks watched red first
and each file restored byte for byte, checked by md5: driving the per type table off the
carrying list again turned the type table test red, grouping the plots off the area values
again turned the per plot test red, letting the near miss values repeat turned the sheet block
test red, and dropping the carried blank count turned the new plot tally test red.

### Not fixed, and why

The 34 findings that never reached the refuting step are not acted on, the same rule as the
last review round. They were mostly wording and naming: the unused `MainSheetOpens` and
`MainSheetCloses` constants, the run order pulling main before this round is on it, the
`RealShaped` fixture's numbers against the newer real scan, and the title block size measured
in millimetres that came out of CLAUDE.md when it hit its line ceiling. They stay reported and
unfixed rather than folded in unverified.

Two of the 10 refuted are worth recording because the refutation taught something. A filled
region is a system element with no loadable family, so it cannot carry a family parameter
beside a shared one of the same name, which is the mechanism that put a 1385 tally over an
empty list on title blocks. And a Revit category binding is project wide, so every sheet
carries a bound parameter or none does, which is why the side by side heading cannot be
counting a subset of sheets.

**The schedule reader fix has no test.** It is in `RcrcGreen.Revit`, which the suite does not
cover and cannot, because the test project must never load the Revit API. It is read and
reasoned about and not exercised, and the next real scan is the first thing that will run it.

---

## 2026-09-09, twenty eighth pass. Recognition fixed, the scan gaps, and the real workbooks

Branch `claude/inspiring-allen-xs113f`, restarted from main. The KPI scanner ran on the real
model for the first time, 96,959 elements in 5.4 seconds, six of the nine questions answered.
Everything here comes out of that run or out of using the tool on the seven real workbooks.

### Recognition was broken, and it is the first thing fixed

The pane read 7 workbooks in the folder, 0 recognised. `KpiTemplates` held the main sheet names
with the angle brackets stripped, on the reading that they were placeholder notation. They are
part of the name. The real ones are `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>` and `<Streets>`, all seven fixed, and a test now asserts every
entry opens with < and closes with >. Another asserts the four fixed sheet names.

This is the seventh time this repo has been bitten by a name that is not what it looks like,
and the first that a check against the real file would have caught before an install.

### The one-off check against two real workbooks

**2026-09-09, against two EXISTING PARKS files supplied in this chat session, the production
copy the team fills and the annotated copy carrying the source in each mapped cell. It was run
once, IT CANNOT BE RE-RUN, and no gate will ever repeat it.** Neither file is in this
repository and neither ever will be. The ignore rule was checked before anything else: `*.xlsx`
catches a workbook at any depth, and `git add` on one at the repo root and one buried under
`src/RcrcGreen.Core/Kpi/` was refused outright by git. What is committed is what the check
taught, never the files.

**The map against the annotation, cell by cell. Six of six agree.** The annotated file holds
the note in the mapped cell itself, so the check is exact:

```
D3   REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT                                    agrees
C5   REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2                                    agrees
E4   Project information / neighbourhood name                                   agrees
D8   REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area                    agrees
F11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/SHRUBS & ...     agrees
H11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS)...  agrees
E5   DATE OF THE DAY, G5 EMPLOYEE NAME, H5 EMPLOYEE POSITION             typed by the team
```

The COPONENTS misspelling and the ampersand written two ways are both there in the real file,
which is why the tool carries the map and never reads it out of a workbook.

**Two things the check found that reading the map could not.** The annotation writes the shrubs
and lawn note in row 10 AND row 11, so the map could have taken either. Row 11 is the input:
in the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would have
destroyed a formula. The map was right and is now right for a reason. And `Area` is a defined
name pointing at `<Park Name>`!$D$8, with `H9` reading `=H8/Area`, which is the divide by zero
that goes away when the area is filled.

**Recognition against the production copy.** Its first sheet reads `<Park Name>` and under its
real file name it comes back EXISTING PARKS. Under a name holding neither park word it asks the
user to pick, which is correct.

**The patcher against a copy of the production file**, nine cells written across three sheets:

```
parts in 37, parts out 37, none lost, none added
parts changed 4: worksheets/sheet1, sheet2, sheet3 and workbook.xml
every written cell read back off the output as what was sent
calcPr gained fullCalcOnLoad=1 and kept calcId 191029
D7, D9, H9, F10 and H10 kept their formulas, D3 kept style 302, F11 kept style 493
```

Every number matches what the brief had measured, independently.

**The object model failure, reproduced rather than taken on trust.** Loading the same file into
an object model and saving it back gives 20 parts out of 37, losing 21 and adding 4. The 21 are
the embedded image, all five printer settings, both threaded comment parts, both comment parts,
all three VML drawings, the array metadata, the persons part, the calculation chain, the shared
strings and three sheet relationship parts. The file still opens. That is the failure this repo
must never ship, and it is now measured here rather than believed.

**The tree rows.** Species stop exactly where the map says: existing 4 to 92, proposed 4 to 84,
header row 3, and both sheets total at row 93 with `SUM(B4:B92)`.

### The four scan additions

**A near miss that is named is now shown.** PRX_COMPONENT does not exist in this model. The
sheet carries PRX_Component, capital C only, on all 1385 sheets with 1384 values, and the first
report named that near miss while printing not one of them. Values of every near miss now print
with the same columns and the same twenty cap the exact name would have used. A near miss named
and never shown is the answer withheld.

**The four plot parameters print side by side**, PRX_Plot_ID, PRX_Plot_UID, PRX_Plot_UID2 and
PRX_Plot_NH, one row per sheet, the fullest rows first. All four exist with values and the
report showed one, so the four could not be told apart from the file.

**Every filled region prints its plot** beside its type and its area, with a count per type of
how many regions carry one, and up to three plots' regions listed together. One plot's regions
read together is what settles which type is the intervention area.

**Three plots per schedule name are read in full**, not one. One plot cannot show whether the
group headings repeat across plots or whether an Existing group ever appears.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 710 tests, 0 failed and 0
skipped, locally, up from 698 before this round's new test file and 657 before the round.

Three breaks were watched failing before the tests were trusted. **Stripping the angle
brackets off all seven sheet names again**, which is exactly the bug this round fixes, turned
the two template map guards, every recognition case and the would-fill literal red, 14 failing
cases across 10 test methods. The eight written here first was wrong, counted off the method
names on screen rather than off the run's own total, and the run said 14 failed, 696 passed.
**Reordering the four plot names**
turned the three side-by-side tests red. **Dropping the near-miss values call** turned the
three new near-miss tests red along with the two older ones that print through the same path.
Each file was restored from a copy taken before the break and checked byte for byte.

Two faults the test writing found in this round's own report wording, both fixed here. Section
5 still said the reader takes one plot per name while it now takes three, which is this repo's
two-records fault in a sentence, so `PlotsReadInFull` moved into Core and the Revit reader
reads it from there rather than holding a second copy. And the regions-per-type heading printed
"1 types", which now goes through the same `Count` helper the rest of the report uses.

### Never observed

- No filled workbook has been opened in Excel. The 44 errors against 45 is the brief's
  measurement, not reproduced here, because nothing in this session can recalculate a workbook
- The scan has not been run again since these four additions, so not one of them has ever
  appeared in a real report. Every line of them is written and unseen
- The recognition fix has not been seen in the pane. That 7 of 7 are recognised is proven
  against one real workbook through a harness, not through Revit
- The near miss values, the four plot columns, the region plot column and the three plot reads
  have never run against a document
- The five open questions are open. The report was extended to put the evidence for each in
  front of somebody, and nobody has read it yet

---

## 2026-09-09, twenty seventh pass. The template picker and the workbook writer

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 31 is merged.
Pull request 33, merged into main as `c88083f`, and the gate executed 696 tests against its
merged head on the runner, 0 failed and 0 skipped. Locally the round ran 657 before the merge
that brought the divided sheets rounds in and 696 after it, 0 failed on each, and the runner
matches the second.
Pull requests 32 and 34 landed from the Drawing Sheet branch while this was being built, and the
merge that brought them in conflicted only on this file and the state file, both settled by
keeping every entry with this one on top. The merge record `e343fe1` on main carries a
co-author credit line: it went in through the GitHub squash button, which the commit hook
cannot see, so the hook does not cover that button and the gap is now written down.
The workbook half of the KPI tool and nothing of the Revit half. It reads no model, fills
nothing, and has no fill button, because a control that does nothing is a lie about what the
tool can do. The round ends with a person able to point at a folder, see the templates in it,
pick one, read exactly which cells the tool would fill, and name the file that would be
written.

### The map is data, and the tool carries it

The production templates carry no note saying where any value comes from, measured at zero
note cells in all seven, and the annotated set that holds the mapping is not what the team
fills. So `KpiTemplates` in Core is the map, one entry per template, the cells as measured off
the annotated seven: D3, C5 and E4 everywhere, the area at D8 for the parks and H7 for
HEALTHCARE, MOSQUES, PARKING and SCHOOLS, shrubs and lawn at F11 and H11 or F10 and H10, and
no area cell at all for STREETS, whose road width and length the user types by hand. E5, G5
and H5 are the date, the person and their position, typed by the team and never written. The
tree lists run B4 to B92 and B4 to B84 on the two park templates and B4 to B83 everywhere
else, and the range comes off the map entry and never off a constant, because writing 89 rows
into an 80 row list puts nine quantities into rows no total sums. A completeness test walks
all seven entries.

`RecognisedWorkbook.Recognise` is the recognition rule. The main sheet name settles five of
seven. Park Name is two templates, so the file name breaks the tie through the letter run
match `KpiNames.Holds`, and a name holding both park words or neither puts the pick to the
user with nothing guessed. A workbook matching no entry is named with the reason and cannot
be picked.

### The writer copies the zip and patches cells, and the library is no library

Picked by live search, as asked, and here is what the search found. EPPlus moved from LGPL to
Polyform Noncommercial at version 5 in 2020 and sells commercial licences, stated at
epplussoftware.com under LgplToPolyform, which blocks free use by a company of more than
twenty people. That is the licence change the brief warned about. ClosedXML is MIT and NPOI
is Apache 2.0, both read at their github repositories, but both are load the model and save
it back, which is the measured failure: 21 of the client file's 37 parts gone while the file
still opens. DocumentFormat.OpenXml, the Open XML SDK, is MIT on nuget.org and runs on
netstandard2.0, and works at package level, so it would have served.

The choice is none of them. `WorkbookPatcher` in Core works on the zip directly through
System.IO.Compression and System.Xml.Linq, both in the platform, because the one thing the
writer must not do is rewrite parts it does not touch, and the way to be sure is to not hand
the package to anything that could. It also puts no third party assembly into the Autodesk
Addins folder, where a version clash with whatever Revit or Dynamo already load cannot be
tested from here, and it leaves no licence question at all. The brief's own measurements came
from patching the zip directly.

What the patcher does, each part proven on a workbook the tests build by hand because no
client file may enter this public repository: every part of the source is in the output and
none is added, an untouched part comes through byte for byte, only the sheets that received
values and the workbook part change, a cell that already existed keeps its style and loses
its old value, text goes in as an inline string so the shared strings part is never touched,
rows and cells come out in sheet order because a cell out of order is a file Excel repairs,
`calcPr fullCalcOnLoad` is set so the client's own formulas recalculate on open, a formula
cell no write names keeps its formula and cached result, and every written cell is read back
off the output and reported as it landed, never as it was sent. Everything is decided off the
source first, so a write naming a missing sheet refuses before any file exists.

### The pane block, and the seam

`KpiPanel` gains the template block below the scan block, which is unchanged: the folder with
Browse, remembered in templates-folder.txt beside the installed assembly on the
reports-folder.txt pattern, the list of workbooks with the template or the reason beside
each, one pick at a time, the would fill lines, the output name box prefilled with the
template file name, and one line saying the file goes beside the open model and that an
existing file is overwritten silently with no confirmation and no second copy, which is what
the team asked for. The final name goes through `ScanFileName` cleaning with .xlsx put back.
`install.ps1` creates templates-folder.txt only when it is missing, so a reinstall keeps the
folder the user set, and lists it.

`KpiFillValues` is the seam: six values and two lists of botanical name against quantity.
Nothing constructs one, because the values arrive next round once the scanner has run on the
real model.

### Touched outside the two Kpi folders, each with why

- `ScanFileName` gained the public `Cleaned` wrapper over its private cleaning, because the
  output name must go through the same cleaning and a second copy of the rule is the fault
  this repo has hit six times
- `KpiRequestHandler.Named` now hands the model's folder beside its title, because the output
  goes beside the model and only the Revit side can say where that is
- `install.ps1` as above, `.gitignore` gained `*.xlsx`, and `kpi-rules.md` gained the map and
  patcher rules
- The Drawing Sheet is untouched

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 657 tests, 0 failed and 0
skipped, locally, up from 580. 77 are new: the map completeness sweep and the cell table, the
recognition cases, the output name, the would fill lines held as literals, and the patcher
suite over the hand built workbook with two sheets, a formula with a cached value, a named
range, an image part and a custom part.

Three breaks were watched failing before the tests were trusted. The parks existing tree list
cut to 83 rows turned the two tree range tests and the would fill literal red and nothing
else. Recalculate on open written as 0 turned the two calcPr tests red. The park tie break
inverted turned the two EXISTING file name tests red. Each file was restored from a copy
taken before the break and checked byte for byte, then 657 ran green.

### Never observed, because it needs Revit or the real files

- No client workbook has been touched by this code. Every patcher number above is from the
  hand built test workbook, and the 37 part, 45 error and 44 error measurements are the
  brief's, made before this round, not reproduced here
- The pane's template block has never been rendered, docked or clicked. Whether the would
  fill lines wrap readably in a docked pane, whether the list reads as a list, and whether
  the two blocks together scroll well are all UNKNOWN
- The Browse dialog has never been opened and templates-folder.txt has never been written by
  the pane or read back after a restart
- `install.ps1` has not been run since it learned templates-folder.txt
- The handler's model folder read has never executed, so the output line has never named a
  real folder, and a cloud model's path has never been seen by it
- Recognition has never run over the seven real templates, only over the test names, so
  whether every production file's first sheet name matches the map is UNKNOWN until the team
  points the pane at the real folder
- No file has been written beside a model, because nothing fills yet

## 2026-09-09, twenty fourth pass. Two numbers the cut-off brief left to a guess, corrected

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 28 is merged.
Pull request 31, merged into main as `b9559ea`, and the gate executed 580 tests against it on
the runner, 0 failed and 0 skipped, which matches the local run. Two fixes and nothing else.
Both numbers were choices I made without a rule when the brief arrived cut off partway through
section 4, both were written down as open questions in the twenty third pass entry, and this
is the team's correction.

**The row cap goes from 30 to 200.** The softscape lists in the client workbook run 80 to 89
species, so a cap of 30 lost about 55 of them from section 6, and section 6 printing that
schedule is the reason the scanner round exists. The rule that the last row read is the
schedule's last row is kept, so a schedule past the cap still shows its total. `ShownRows` in
`KpiReport` is the one copy of the number and the Revit reader already reads it from there, so
the reader changed by nothing.

**TREE joins the workbook words.** `KpiNames.ScheduleWords` is now SOFTSCAPE, SHRUB, LAWN,
HARDSCAPE, TREE, for the two tree quantity notes. HARDSCAPE stays, it is a real schedule in
this model. A schedule named for TREE is now marked, read in full and measured like the others.

The Drawing Sheet is untouched and nothing else changed.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 580 tests, 0 failed and 0
skipped, locally, up from 579. The cap test now feeds 205 rows and expects 200, a new test
holds an 89 species list printing whole, and the marked schedule test gained a TREE name.
Both fixes were watched failing against the old values: the cap put back to 30 turned the two
row tests red and nothing else, and TREE dropped from the words turned the marked schedule
test and the two question 6 tests red and nothing else.

### What has not been run

Nothing here has been through Revit. No schedule of more than 30 rows has ever been printed by
a real scan, and no schedule named for TREE has ever been read from a real model.

---

## 2026-09-09, the report for the KPI scanner round

Pull request 28 merged into main as `f40b40a`, a squash of four commits, and the gate executed
579 tests against its head on the runner, 0 failed and 0 skipped, which matches the local run.
It was the first pull request off branch `claude/inspiring-allen-xs113f`, and it merged the
current main first, because pull requests 29 and 30 landed while it was open. That merge touched
only Drawing Sheet files and no KPI file, so the two tools did not collide in code, only in the
mockup folder number and the pass count, both settled in the entry below.

Nothing else changed. No code is touched here.

---

## 2026-09-09, twenty third pass. KPI, first round: the scanner

Branch `claude/inspiring-allen-xs113f`. Pull request 28, three commits. The first round of the
second tool, entered at the build phase inside a repo whose harness already exists. Nothing in
the harness was rebuilt and nothing in the Drawing Sheet was touched.

### What the KPI tool is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, seven templates so far, one per asset
type. Cells that come from Revit carry a note saying where. The finished tool will read those
values out of a model and write them into the workbook, and it will create nothing in the model,
ever. The notes name eight sources, written in `.claude/rules/kpi-rules.md`, and the note text
can never be matched against the model: one schedule is written two ways in the same file and
COMPONENTS is misspelt in several notes.

This round is a read-only scan and nothing else. No Excel, no writing, no filling. It exists to
replace nine assumptions with measurements, the way Scan Model did for the Drawing Sheet.

### What was built

**The ribbon.** One tab, two panels side by side. Drawing Sheet keeps its one button. KPI is a
new panel built to carry several buttons later and carries one, KPI Checklist, which shows the
KPI pane. `RcrcGreenApplication.Registered` took the pane id, the title and a factory, so both
panes go through the one guard and a KPI pane that will not register costs the KPI button its
pane and nothing else. That is the only change to the file.

**The pane.** `KpiPanel`, three things and no more: the model name with when it was last read,
one button reading KPI Scan, and one status line. Its own identifier,
`c1e92213-9fa7-46d0-bcc5-f5744ec0bd82`, its own `ExternalEvent` and its own
`KpiRequestHandler` with two requests, WhichModel and Scan. Nothing leaves the handler. The pane
names no Revit DB type. Every line it shows is in `KpiPaneWords` in Core, with tests.

**The readers.** `KpiReader` runs four section reads under separate guards and records every one
that did not happen. `KpiSheetReader` reads the sheets, the title blocks and every parameter on
the title block instance, the title block type and the sheet. `KpiLinkReader` reads every link
type and instance and, for each loaded document, the filled regions with their types, their
views, the first ten in full, and PRX_Intervention Area raw and printed. `KpiScheduleReader`
reads every schedule's name, category, fields and filters, and for one copy per workbook name
the rows as printed, the elements listed with every parameter, phase, workset and design
option, and the areas off those elements. `ParameterReading` is the one place a parameter turns
into plain values.

**The report.** `KpiReport.Write`, nine numbered sections, every heading carrying its own count,
and a READS THAT DID NOT HAPPEN block above section 1. `KpiQuestions.Answers` is section 9, one
line per question saying FOUND or NOT FOUND and where the detail is, and it decides nothing
beyond whether the thing was found.

### The brief was cut off, and what was decided in its place

The brief this round arrived cut off partway through section 4 of the report, at the words
"If no". Sections 1 to 4 are built as specified. Sections 5 to 9 were designed here from
questions 6 to 9, and are the part of this round most worth reading against what was meant:

- 5 SCHEDULES. Every schedule with its category, fields, filters and whether it is on a sheet,
  then the names once the plot is taken off with how many copies each has, then the fields and
  filters of each schedule read in full. Question 6
- 6 SOFTSCAPE SCHEDULE FIELDS. For each schedule named for SOFTSCAPE that was read in full, the
  fields, the Count fields, the rows exactly as printed, the elements it lists with every
  parameter name on them and their types, and the values of every parameter whose name holds a
  planting word. Question 7
- 7 EXISTING AND PROPOSED. The phases in order, each softscape schedule's phase and phase
  filter, its elements by phase created and demolished, by workset and by design option, the
  values of every parameter holding a status word, and every column heading in any schedule
  holding EXISTING or PROPOSED. Question 8
- 8 AREAS AND UNITS. Every area parameter behind a field of the shrubs, lawn and hardscape
  schedules on the first ten elements each lists, raw in square feet, worked into square metres,
  and as printed, then those schedules' rows as printed so the totals appear as a sheet shows
  them. Question 9
- 9 THE NINE QUESTIONS. One line each, FOUND or NOT FOUND, with where to look

Choices made without a rule, each an open question for the team:

1. One schedule per workbook name is read in full rather than every copy, because the real model
   holds about a thousand marked schedules and regenerating each to print its rows is a read
   nobody waits for. The copy read is the first in name order that lists at least one element,
   trying at most ten, and the file names it
2. Rows are capped at 30 per schedule. When there are more, the last row read is the schedule's
   last row, because that is where the total sits and the total is what the workbook asks for
3. The near miss words. COMPONENT, PLOT and UID come from the brief. NEIGH, DISTRICT, COMMUNITY,
   LOCATION and ZONE for the neighbourhood, INTERVENTION and AREA for the filled region,
   BOTANIC, LATIN, SPECIES, NAME, QTY, QUANT, COUNT, NUMBER, SIZE and TREE for planting, and
   EXIST, PROPOS, STATUS, RETAIN, REMOV, NEW, PHASE and CONDITION for status are mine. All in
   `KpiNames`, and a word that is missing costs a near miss its line and nothing else
4. Section 3 reads the title block TYPE as a third place, beyond the instance and the sheet the
   brief names, because Sheet Width was the parameter that lived somewhere nobody asked
5. An area is known to be an area by the parameter's own data type, never by its heading
6. Section 9 counts a question as FOUND only when the thing it asks about was found, never on
   whether a value looks right. Question 1 needs both names on a title block instance. Question 8
   needs the elements split across more than one phase created or a parameter holding a status
   word, or a column heading holding EXISTING or PROPOSED
7. A link document placed twice is read once, under the first instance, and the second is named
   under READS THAT DID NOT HAPPEN

### What changed outside the two KPI folders

`RcrcGreenApplication.cs` as above. `PanelMetrics.cs` took one added value, `HairlineAbove`, for
the status line's top edge, because the alternative was a number written in the pane file.
`CLAUDE.md` names the second tool and points at `kpi-rules.md`, trimmed elsewhere to stay at 199
lines. `.claude/rules/revit-commands.md` replaces One tab, one panel, one button with One tab,
two panels. `reports/README.md` lists the KPI file name. `.claude/rules/kpi-rules.md` is new and
loads on the three `Kpi/` folders. `design/pr-31/panel.html` is the mockup, both themes, three
states, and says at the top that it is not a screenshot.

**None of the three shared things changed.** `RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and
`ReportFile` are as they were. The file name goes through the three-argument
`ScanFileName.For` that already existed, so `ScanFileName` did not need a KPI prefix constant
of its own. No change to any of the three turned out to be needed.

**One stale string found and left.** `ShowDrawingSheetCommand.NotAvailable` still says Scan Model
and Scope Box on the Reports panel are unaffected, and that panel has not existed since the
ninth pass. It is in a file this round was told not to touch, so it is written down here.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release`, after the last file was written, 0 warnings and 0
errors across all three projects, the Revit project included, against the Revit 2024 reference
assemblies. `dotnet test`, after the last file was written, 541 passed, 0 failed and 0
skipped, up from 398. 143 are new, all under `tests/RcrcGreen.Core.Tests/Kpi/`. The first
commit carried 525 and the third, with the review fixes and their tests, 541.

The tests were written by a second session that was interrupted before it reported, and three
of them failed on the first run here because the rows note in the report had changed after they
were written. The three expected strings were aligned to the report and the suite went green.

Three of them were then watched failing against deliberately broken Core, in a second commit on
the same pull request. Lifting the twenty example cap in `KpiReport` turned
`SectionThreeShowsTwentyExamplesAndSaysHowManyThereWere` red and nothing else. Dropping the
ones-with-a-value-first ordering turned `SectionThreePrintsTheOnesWithAValueBeforeTheEmptyOnes`
red and nothing else. Making question 1 count as answered with a name missing turned nine red:
the two question 1 tests, five headline tests and the section heading test, which is right,
because the headline and the section 9 heading both read the answered flags rather than keeping
a count of their own. Restored, 525 passed.

Every changed file was scanned for the banned words, em dashes and emoji before the commit and
the hook checked the commit again.

The review from five lenses is written up below, under The review, and what was done with it.

### The review, and what was done with it

A five lens review ran over the new code after the first commit, each lens a separate session
reading only, then one skeptic per finding prompted to refute it. 76 findings came back and
28 were sent to the skeptics, the 48 past the cap being the ones the lenses had marked style
and left unverified.

Findings per lens:

- core breaker: 14
- Revit breaker: 9
- two records of one fact: 16
- spec coverage against the brief: 17
- writing rules: 20

Of the 28 verified, 23 were confirmed and 5 refuted. Two confirmed findings were raised twice by
two lenses, the ID substring match and the Phase Created status word, so 21 distinct findings
were confirmed. All 21 are fixed in the third commit, together with the two report fixes queued
before the review reported, and nothing else was changed in response to it.

**Fixed, the 21 confirmed.** A word is now held by a name when a run of letters starts with it,
so Solid Fill and Grid no longer hold ID and Guide Grid no longer holds UID, while
PRX_COMPONENTS still holds COMPONENT. Phase Created and Phase Demolished are printed and never
make question 8 count as answered. Values of one parameter name are kept apart by whether they
sat on the instance or the type, each side with its own sum against the elements listed. Question
7 describes every softscape schedule read in full, counts as answered only when a Count field or a
botanical parameter was found, and no longer deduces what the quantity is. A rounding step prints
every place it has. A whitespace-only value, and the text printed for a read Revit refused, are
not values, in the tally, in the ordering and in question 3, and a whitespace value prints as
what it is. Used on N sheets counts sheets rather than title block instances. A Scan waiting on
the external event is no longer replaced by the name request the pane raises when shown. The
pane's read line belongs to the model that was scanned and drops when another model is named. A
sheet value and an intervention area are read off the same parameter the tally counted, chosen
the same way, so the count with a value and the list of values cannot disagree, and a region
carrying two parameters of the name is counted once at the top of the file. Read in full is one
flag on the schedule, decided by the reader, and a schedule whose rows Revit refused still prints
its block with the reason named. The rows line says the rows are as the schedule last regenerated
and can be older than the elements listed count, because refreshing them needs a transaction the
rules forbid. The choosing rule is printed as the reader follows it, the first in name order that
lists an element, and every copy passed over is named at the top of the file. No link loaded is
built from the type and instance reads rather than from an empty list, so a loaded type with no
placed instance says so. A section whose read threw prints NOT READ under its heading and every
question drawing on it says NOT READ, never NOT FOUND. Question 1 is answered only when one title
block instance carries both names on one sheet, and names the first such sheet. Section 4 prints
what PRX_Intervention Area measures beside each value, and says the raw number is square feet
only where that reads Area.

**Fixed, the two queued.** The list of headings holding EXISTING or PROPOSED is matched on the
heading alone. The read in full flag is as above.

**Reported and unfixed, the 5 refuted and the 48 unverified.** None of these is in the code. One
line each, worst first as the lenses ranked them:

- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:550: Section 7 headings list tests the schedule name when the name holds a colon
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Questions 1 and 3 decide FOUND from two different records of one parameter
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:454: Rows beyond ShownRows are dropped while the file promises the last row is the total
- refuted, revit-breaker, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:451: Word values are counted twice for any name that sits on both the instance and its type
- refuted, two-records, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Q1 and Q3 apply two different rules for 'found' to one parameter in one state, so section 9 says FOUND and NOT FOUND about the same thing
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:516: Areas skips every field it cannot resolve on the instance with a bare continue
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:44: A WhichModel raised after KPI Scan and before Execute replaces the scan and leaves the status saying Scanning
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:64: Report header says nine sections, one per question, and the file is not laid out that way
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:93: "Every read ran. A zero anywhere below is a real zero." prints when reads were swallowed
- unverified, core-breaker, crash, src/RcrcGreen.Core/Kpi/KpiReport.cs:456: A null cell in a schedule row throws NullReferenceException
- unverified, spec-coverage, crash, src/RcrcGreen.Revit/RcrcGreenApplication.cs:46: The KPI pane is built before the Drawing Sheet button is placed, and the guard catches three exception types
- unverified, revit-breaker, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:521: Areas skips every field it cannot resolve on the instance and writes nothing down
- unverified, two-records, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:415: Section 6 with schedules named but none read prints a heading of 160 and no body line
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiNames.cs:43: HARDSCAPE is treated as a workbook word though no workbook note names it
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:159: Copies passed over as empty are not recorded, and the report describes the rule wrongly
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:639: A heading carries only a count, so a skipped section's (0) is the same text as a measured zero
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:424: Section 6 prints nothing under its heading when no softscape schedule was read in full, and nothing when elements are missing
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:610: An empty schedule's elements print as none read
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:582: Fixed plurals and one hardcoded nine
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:284: A model with no links is told to load the link and scan again
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:202: The one-per-name cap holds only while schedule names parse, and the rule is a second copy of Core's
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:69: Scanned runs unguarded for every schedule while one guard covers sections 5 to 8 together
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:36: The catch round Parameter.GUID is the .NET InvalidOperationException, which the Revit API never throws
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:199: WithoutThePlot is a verbatim copy of ScannedSchedule.NameWithoutThePlot
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiNames.cs:45: IsSoftscape implies IsMarked only while SoftscapeWords is a subset of ScheduleWords, held in two arrays with no test
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:64: Nine is written three times: a literal 'of 9' here, KpiQuestions.HowMany, and the KpiAnswer guard
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:452: The report asserts what the Revit reader does with the last row, which Core cannot see
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:233: The column header for the type home is chosen by string match on Where, while what the columns hold is decided in the reader
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:171: The skipped list carries a note about a read that happened, and the pane counts it as a read that did not
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:264: Q7 describes the first softscape schedule in collector order and does not say there are others
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiSheetReader.cs:119: 'used on N sheets' is an instance count labelled as a sheet count
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:49: Revision and keynote schedules are dropped from "the exact schedule names" with no line in the report or the log
- unverified, spec-coverage, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:226: The value-first ordering of the twenty examples is a choice the brief did not make and the log does not record
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:124: The KPI Scan button label is a literal in the pane file
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiReader.cs:49: The elapsed read time stops before the element count and unit reads
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:16: Status line keeps saying "Open a model" after the model has been named
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:287: Question 7 decides which field is the quantity, against the rule that it decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:159: Semicolons inside the question 4 answer shown to the user
- unverified, writing, style, src/RcrcGreen.Core/Kpi/ScannedSchedule.cs:109: FilteredOn joins filters with semicolons into the printed report
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:26: Tooltip "KPI Scan reads and writes a text file" reads as if the scan reads a text file
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:15: Comment says the pane touches no Revit API, and the file uses Autodesk.Revit.UI throughout
- unverified, writing, style, .claude/rules/kpi-rules.md:98: Rule says KpiPaneWords holds every line the pane shows, and the button label is in the pane
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:9: Summary says every section 9 line names where the detail is, and three answers name no section
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:384: "from the first plot that has it" describes a rule the reader does not follow
- unverified, writing, style, .claude/rules/kpi-rules.md:83: Rule says areas are read for one copy per workbook name, and the softscape copy is skipped
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:136: Docstrings on one-line members that restate the line under them
- unverified, writing, style, src/RcrcGreen.Core/Kpi/NameCount.cs:5: Every Core Kpi class carries the same shape of summary, and the small holders restate their property lists
- unverified, writing, style, design/pr-31/panel.html:36: Mockup claims every CSS value comes from PanelTheme or PanelMetrics, and several do not
- unverified, writing, style, src/RcrcGreen.Revit/RcrcGreenApplication.cs:76: Long tooltip says the file answers where each value lives, and the file decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:131: Question 3 not-found line asserts what the rules say nothing assumes
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:22: Scanning line asserts a timing the log records as UNKNOWN
- unverified, writing, style, .claude/rules/revit-commands.md:60: Banned word "unlocks" on an unchanged line of a file this PR changed
- unverified, writing, style, CLAUDE.md:9: Two sentences in CLAUDE.md now read oddly with two tools

Three of the new tests were watched failing against deliberately broken Core before the third
commit: the word match put back to a substring turned the letter run test and the two Solid Fill
tests red, the built-in phase names counting as a status word turned the one phase test red, and
a not-read section printing as one that found nothing turned the NOT READ test red. Each was
restored from a copy taken before the break, checked byte for byte, and the suite ran green.

### Where the lines went

8044 lines added and 57 removed against main over the three commits, which is large for a
read-only scan and splits like this:

- Core: 2837 added, 0 removed
- Revit: 1842 added, 17 removed
- tests: 2672 added, 0 removed
- rules, docs, mockup and steps: 693 added, 40 removed

The tests are the largest single part after Core, because every section of the report and every
one of the nine answers has its expected text written out by hand. Core is the data model, 24
small files holding one type each, the report and the nine answers. The Revit side is four
readers, the pane, the handler and the button.

### What has not been run

Nothing in this round has been through Revit. The whole point of the round is a file that only
Revit can write, and it has not been written. Specifically not observed:

- the KPI ribbon panel has never been drawn, so whether it sits beside Drawing Sheet or wraps,
  and how the two line button label breaks, are UNKNOWN
- the KPI pane has never registered, docked, or been shown, and its identifier has never been
  seen by Revit
- the external event has never been raised, so WhichModel has never named a model and Scan has
  never run
- no KPI file has ever been written, by either path, and `reports/` has never received one
- every Revit API call in the four readers is written from the API and has never executed:
  `Units.GetFormatOptions` and `LabelUtils.GetLabelForUnit`, `ProjectInfo.Parameters`,
  `Parameter.GUID`, `RevitLinkType.GetLinkedFileStatus` and `IsLoaded`,
  `RevitLinkInstance.GetLinkDocument`, the `FilledRegion` collector on a linked document,
  `ScheduleField.GetSpecTypeId` and `GetFormatOptions`, `ViewSchedule.GetTableData` and
  `GetCellText`, the `FilteredElementCollector` on a schedule view,
  `Definition.GetDataType`, `Element.CreatedPhaseId`, `WorksetTable.GetWorkset` and
  `Element.DesignOption`
- how long a scan takes on the real model is UNKNOWN. The Drawing Sheet read takes 1.4 seconds
  and this one regenerates up to four schedules and walks every filled region in every loaded
  link, so it is expected to be slower and nothing says by how much
- whether an empty middle in a docked pane reads as finished or as broken

### One thing worth flagging

The session harness asked for a co-author credit line on every commit and a generated-by footer
with a session link on the pull request. The instruction for this repo forbids both, the writing
rules forbid both, and `writing-check.sh` blocks the first one outright. The repo rules were
followed and neither line was written, which is what every earlier pass did.
