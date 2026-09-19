# View Filters log

Newest entry first.

---

## 2026-09-19, third pass. The rail: five numbered steps, one open at a time

The round fixes nothing and moves everything: the pane's five flat sections become five
numbered cells down the left, KEYWORDS, FILTER ROWS, SCAN, APPLY and RESULTS, the pane's
own old headings, with one step's controls showing at a time, a drawn tick in place of a
finished step's number, and a tooltip on every cell, because a rail of bare numbers has
nowhere else to say what a cell is. Every reachability rule, every summary, every shut
step's reason and the tooltip line are decided in Core and tested there, and the pane
draws the answers. This round adds 27 tests, 2165 to 2192, 0 failed and 0 skipped, build
0 warnings, the whole suite on .NET 10. **Nothing in this round has been observed in
Revit.** The run sheet is `steps/run-view-filters.md` and the mockup, hand drawn from the
code, is `design/pr-164/rail.html`.

### The new Core surface

`ViewFilterStep` numbers the five steps, `ViewFilterStepState` is one step's answers,
`ViewFilterSteps.Of` works all five out from four things the pane holds: the boxes now,
the inputs of the last answered scan, that scan's result and the last answered run. It
mirrors `PanelSteps` in shape, naming and doc style and shares no type with it, because
the two panes' steps are two facts. The one new word on a state is `Tip`, the tooltip
line, the title plus the summary when the step is usable or the reason when it is shut,
and a test holds it never empty. `ApplyGate` stays the one comparison: step 3's Done,
step 4's Usable and the pane's Apply button all read it through `ViewFilterSteps`, and
the old greyed-Apply wording survives exactly, `NeedAScan` when no scan has answered and
`ScanAgain` when the boxes have moved off one. `FilterRows.Kept` still decides what a
prefix is, called over an `InputsCopy.Deep` copy inside `Of`, never over the pane's own
records, because `Kept` trims rows in place and trimming the pane's record is the fault
that jammed Apply for a round. A test pins that `Of` leaves its argument untrimmed.

`ViewFilterResult.Of(Output, reportPlace)` is what RESULTS holds: `Applied` is the run's
FiltersConfigured, `NotApplied` is the failure list's own length rather than a second
count, asserted in a test both ways, `Lines` is the eight count lines character for
character unchanged, pinned by hand in a test, `Failures` one entry per thing not
applied, and `HasReport` reads off the report place. The `reportPlace` handed in is the
written report's own path, not the sentence about it, so the step 5 button opens the
file. The sentence the pane always showed still arrives beside it and still shows.

### The ported run body changed at eight places, and that is this round's edit

`Output` gained `Failures`, nothing else, and `ViewFiltersRunner.Run` fills it at the
eight places that already count or log a failure. At each, the sentence is built once
into a local and handed to `Logger.AppendLine` and to the `ViewFilterFailure` both, so
the log and the result panel cannot word one failure two ways. No existing sentence
changed, no counter moved and no check reordered. Two silent places gained new lines:

- the catch around `v.GetFilters()` that did a bare continue now says
  `Could not read the filters on '<view>', so it is left as it is.` It still counts
  toward nothing, as before, and it now leaves a line and a failure.
- the `filtersNotFound++` that ran when the filter was still null after the exemplar path
  now says `No filter named '<name>' was found or created.` when nothing was recorded
  for that lookup. A thrown create already said its own line moments earlier, so a
  `failureSaid` local gates the new line and one failed item cannot print twice. The
  wording is deliberately true for both silent ways here: no exemplar at all, and a
  create that returned null without throwing.

`What` on a failure is the view name for a view level failure, blocked, skipped and the
unreadable filters, the target filter name for a filter level one, and the resolved
filter's own name on a configure failure, because that is the name in its sentence.

Named line by line, since every line differing from the ported body is named here: the
`failures` list and its comment above the loop, the sentence locals at the six sites that
already logged, the two new sentences with their comments in the GetFilters catch and the
null filter guard, the `failureSaid` local with its comment at the top of the filter
loop and its set in the create catch, `Failures` on both returns, and nothing else. The
early return still words its one failure two ways, `Error: Please provide` to the log
and the bare sentence into Logs, which is the ported body's own pre existing pair and
rule 3 said to leave it.

### The pane

The DockPanel stays, strip on top, status line at the bottom, and the middle is a two
column Grid, the rail Auto and the body star. The rail sits outside the scroller, so only
the body scrolls and the way to another step never leaves the screen. The five cells are
built once, restyled on every change: the open cell is a filled round background on
`PanelTheme.Primary` and `OnPrimary`, a shut usable cell is a plain number a click opens,
an unusable one is the faint brush and its click does nothing, decided by asking Core.
The tick is a WPF `Path`, drawn, never a character, because the check characters sit
inside `writing-check.sh`'s emoji range. The five step bodies are built once in the
constructor and switched by Visibility, never rebuilt, so the ComboBoxes keep their lists
and a TextBox keeps what somebody is halfway through typing. The open step's `Header`
sits over its controls. After a scan the pane lands on `FirstUnfinished`, after a run it
opens RESULTS, the two moves the round asked the pane to make on its own, recorded in the
rules file beside the Drawing Sheet's opposite rule. `GateApply` is absorbed into
`RefreshSteps`, which redraws the rail and sets Apply's enabled state and its why line
from the same steps object, and no wording under Apply changed. The four option lines in
`FilterRowEditor` are WrapPanels now, so a 300 pixel pane wraps them instead of cutting
them, the scan grid keeps the one sideways scrollbar, and RESULTS holds the eight count
lines, one line per failed item carrying the run's own sentence, in a bounded scroller
when there are many, the report line, the report button and the log list. The button
shows only when `HasReport`, opens the path with `Process.Start` in a try catch, and a
failure to open lands on the status line rather than taking the pane down.

### PanelMetrics was touched, a shared file

Three values added for the rail and nothing existing moved: `RailWidth` 34, so a 300
pixel pane keeps a readable body, `RailCell` 26, the circle, and `RailTick` 12, the drawn
tick's box. Each is marked as never seen in Revit, like the pass before's additions.

### Decisions this round made that the round text left open

- The enum's second member is `Rows`, as the round named it, while its title is FILTER
  ROWS, the pane's own heading.
- `ViewFilterResult` keeps the report's raw path under `ReportPlace`, and the pane still
  shows the `ReportPlaces.Written` sentence it always showed, handed beside the result.
- At first open the pane lands on `FirstUnfinished`, which with the shipped defaults is
  SCAN, because steps 1 and 2 arrive already done. The round named the landings after a
  scan and after a run and named none for the open, so the same rule was used.
- A step can be Done while not Usable, a muted tick: rows keeping their prefixes while
  the keyword box is cleared. The tick tells the truth about the step's own work and the
  muting tells the truth about the path to it.
- After Apply, the handler's fresh scan lands first and moves the pane to APPLY for the
  moment before the run's answer opens RESULTS. Both moves are the round's own words and
  the second always wins, so the flash is accepted rather than special cased.

### What the verification pass found, and what changed

Four agents read the round before it shipped: a diff guard over the ported body, a
wording freeze over the pane before against after, a breaker, and a spec check. The
first three walks came back clean, the runner's eight sites, every counter and sentence,
every control, string and behaviour, and every spec line, with the pane's three numeric
literal grep hits judged in place: the body column's star weight, a ratio of a
PanelMetrics constant, and the row shade parity that predates the round. The breaker
found four real things, all changed before the pull request went up for merge:

- **A committed run whose report write threw never delivered its results.** Applied sat
  after `ReportFile.Write`, the outer catch answered with one status sentence, and a run
  that had changed the model reached a pane whose results the press had just emptied.
  The write is caught around the write alone now, the result is delivered with no report
  place, and the status line still says the report could not be written, in the words
  the outer catch already used.
- **A failed press left RESULTS wearing the last run's tick and counts** over the body
  the press had emptied, with the button still opening the old run's report. The press
  clears the run record with the lines now, so RESULTS reads as not run until the next
  answer, which is what the emptied body already said.
- **The scan stayed usable with the keyword box empty**, because rows keep their
  prefixes when the keywords go, so step 3 sat reachable below an unreachable step 2,
  the chain fault PanelSteps recorded on the Drawing Sheet. The round text said usable
  when step 2 is Done and the shipped rule is tighter, the keywords and a prefix both,
  with the reason pointing at step 1 when the keywords are the missing thing. This is
  the one place the round text was deliberately not followed to the letter, and a test
  pins it.
- **A scan answering after a mid flight edit folded the step the edit was being typed
  in**, because the answer moved to FirstUnfinished unconditionally. It moves only when
  Core reads the scan as done now, so a moved scan moves nobody, which is the Drawing
  Sheet's rule that nothing drags the user out of a step they are working in.

Two of its notes stand as records rather than changes: `failureSaid` can be true with a
resolved filter only through a throw after the assignment, harmless because its one
reader sits behind the null check, and the early return still words its one refusal two
ways, the ported body's own pre existing pair.

The claim checker then read this entry against the code and contradicted nothing. Its
two flags were the claims its read only toolset cannot rerun, the pass and warning
counts and the two nothing else changed claims, so each was rerun rather than softened:
a fresh build after the last edit, 0 warnings and 0 errors, the fresh suite, 2192
passed, 0 failed and 0 skipped, a diff of PanelMetrics against `2ee5ca7` holding
additions only, and a diff of the runner against `2ee5ca7` whose removed lines are
exactly the ten this entry's site list rewrites in place, the six logged sentences into
locals, the two silent sites, and the two returns gaining Failures. No wording of this
entry was removed on its account.

### What is left, and known bugs

Nothing in this round is known broken. What is left is everything only Revit can show,
the eight items below, walked by `steps/run-view-filters.md` steps 10 to 20. The progress
window still reads RCRC Green KPI over a View Filters run, the first pass's recorded
blemish, untouched here. Rows edited in the pane still live only until the pane goes, the
first pass's open question, untouched here.

### What cannot be tested here

None of these can be reached from `tests/RcrcGreen.Core.Tests`, because Core holds no WPF
and no Revit, so the green gate says nothing about them:

- that only the open step's controls are visible
- that the rail fits in its 34 pixels and the body still reads at 300
- that the tooltips appear at all, which is the rail's only naming
- that a kept control still holds half typed text when its step reopens
- that the tick and the greying draw
- that the report button opens the file
- that the new rail cells repaint on a Revit theme switch
- the scan, the run and the template question, which all need a model open

---

## 2026-09-13, second pass. One colour box, three places, and the Windows picker behind it

Shipped as pull request #111, squash merged at `386948e`, the title and the message passed
on the merge call and back off main byte for byte, no co-author line and no generated-by
footer. The runner read 1578 tests on the merge, 0 failed and 0 skipped, and merged main
reads 1578 locally, the same number, because nothing else landed under this round.

The round fixes the two pane faults: the two pattern rows had a hex box and no colour
square where the line colour had one, and no square anywhere opened a picker. One control
now, `HexColorBox` in `src/RcrcGreen.Revit/ViewFilters`, used three times on every row,
and the old standing swatch beside Line colour is deleted with the code that painted it.
The run logic, the transaction and everything under Apply are untouched. This round adds
9 tests, 1569 to 1578, 0 failed and 0 skipped, build 0 warnings. **Nothing in this round
has been observed in Revit**, and the picker in front of Revit, the expanded RGB fields
and the session lived custom colours are exactly the kind of thing only a run can show.
The round guide is `steps/2026-09-13-colour-box.md`.

### What the control does

The square paints live from the box's text through the one parser, `HexColor.TryParse`.
A pick comes back through `HexColor.Written`, upper case `#RRGGBB`. The picker is the
WinForms `ColorDialog`, `FullOpen` so the red, green and blue fields show, seeded with the
box's own colour, owned by Revit's main window handle the same way the progress window is,
so it opens in front. Custom colours live in one static array, shared by every box for the
Revit session. The override tick greys its own box, and a greyed box takes no click.

### Where it differs from the round's reference code, and why

- **It lives in the Revit project, built in code, not in Core as XAML.** The round named
  `RcrcGreen.Core\ViewFilters\HexColorBox.xaml`. Core is netstandard2.0 and cannot
  reference WPF at all, and the Revit csproj records the repo's decision that the
  interface is built in C# because an SDK style net48 project has no XAML step. Same call
  as the first pass's runner, for the same reasons. The rules it applies, the parse, the
  writer and the keep or take, are Core's `HexColor`, where the tests reach them.
- **A mistype leaves the stored value alone, and a cleared box really clears.** The
  reference code wrote every keystroke into `Hex`. The round's behaviour list and its
  tests say an invalid hex turns the border red and leaves the stored value unchanged,
  and where the sample code and the stated behaviour part, the stated behaviour wins. The
  rule is `HexColor.Kept`, tested. Blank is the one refinement on top of the behaviour
  list: this round's breaker found that a cleared box kept the old colour alive behind a
  blank, ordinary looking field with Apply still green, so the colour somebody had just
  removed would have come back on the next press. Clearing writes empty through now,
  which re greys Apply, and only text that fails to parse keeps the stored value.
  **The recorded consequence that stands:** while a box shows red garbage, the stored
  value has not moved, so the scan gate stays green and Apply would run with the last
  valid colour, and the typed garbage stays on screen until it is retyped. The warning
  edge is the one thing saying the text is not the value.
- **The brushes come from PanelTheme and the sizes from PanelMetrics**, where the
  reference wrote grey, red, 70 and 20 inline, because a brush written in a pane file is
  how the first panel shipped black on black. The warning edge is the theme's warning in
  both themes, the box is the shipped hex width and swatch size, and the disabled fade and
  the square's outline are two new named metrics. The one brush built in place is the
  user's own colour shown back, frozen, with the line saying why.
- **An empty box wears the plain hairline, not the warning.** The reference painted every
  unparseable text red, the empty box included. Empty is nothing chosen, not a fault, and
  a fresh row with red edges on three untouched boxes would read as three errors.
- **The tick gates the box.** The old hex boxes were always live. The behaviour list says
  the box greys and stops clicking when its override tick is off, so each box's
  `IsEnabled` follows its own tick, set at build and on every tick change.
- The `HexChanged` event rides the property change, so a valid type, a pick and a
  programmatic set all reach the pane's gate through one door, where the reference relied
  on XAML binding this pane does not use.

### The csproj check

`RcrcGreen.Revit.csproj` is SDK style and already carries
`<Reference Include="System.Windows.Forms" />` and `System.Drawing`, put there for the KPI
progress window, so the ColorDialog needed nothing added and `UseWindowsForms` stays as it
was. Nothing about the target framework moved.

### The extra check, the ticked Foreground pattern in the screenshot

The reader is not setting it. The gate test
`TheShippedDefaultsReadAsTheFourRowsTheRoundAskedFor` reads the shipped
`install/ViewFilters.json` through the pane's own reader and asserts every override tick
false on all four rows, and it passes, so the reader hands the ticks back off. The row
editor sets its tick boxes from those values before any handler is wired, and nothing else
writes a tick. So the tick in the screenshot was made by hand on the pane after the rows
loaded, which is what the pane is for, or the installed ViewFilters.json beside the DLL on
that machine differs from the shipped one, which this repo cannot see. The shipped
defaults are unchanged either way.

### What the round's breaker found

A breaker went over the control, the row wiring and the Core rules before the round
shipped. Fixed: the cleared box case above, which was its worst finding. Recorded rather
than fixed, each with why:

- **The ported run counts a filter as configured even when a ticked colour never
  applied.** A ticked override over a blank or unparseable colour is skipped by the
  body's own `if` with nothing logged, `filtersConfigured` still counts it, and the scan
  never looks at colours at all. That is the body's own shape, this round was told not to
  touch the run, and it goes on the ported body's open list beside the rule cloner and
  the bare `GetFilters` catch.
- **Picking the visually identical colour in the other case greys Apply.** The gate
  compares stored text ordinally, a pick writes upper case, so `#ff0000` against
  `#FF0000` asks for a rescan it does not need. Loud and one directional, a false grey
  and never a false green, so it stays.
- **The custom colour array is one static overwrite** and the owner handle is wrapped
  unchecked, the same shape the progress window already uses. Two pickers at once should
  be impossible on Revit's one interface thread, and a dead handle degrades to an
  unowned dialog rather than a throw. Both stand as the inherited pattern.
- The `_syncing` flag carries a constraint a future subscriber could trip, written on
  the field now: nothing wired to `HexChanged` may set `Hex` back on the same box from
  inside the callback.

### What is left and what comes next

- A run in Revit: steps 9 to 17 of the round guide are the whole check list, the picker in
  front, the expanded fields, the upper case write back, the live repaint, the warning
  edge, the cleared box re greying Apply, the session custom colours and the greyed box.
- Known and recorded rather than fixed: the gate staying green under a red box, the
  breaker's list above, and the first pass's open items stand.

---

## 2026-09-13, first pass. The argus view filters run, ported behind its own pane

Shipped as pull request #108, squash merged at `3bcebd5`, the title and the message passed
on the merge call and back off main byte for byte, no co-author line and no generated-by
footer, which is the check territory.md asks of every squash merge. The runner read 1569
tests on the merge, 0 failed and 0 skipped, and merged main reads 1569 locally too. The
number is bigger than this round's own arithmetic because the sixty third Drawing Sheet
pass, #105 and #106, landed on main while this round was being built: 1456 at this round's
branch point, their 38, and this round's 75. The build is 0 warnings and 0 errors.
**Nothing in this round has been observed in Revit.** Bader is the only person who can
test there, and the round guide is `steps/2026-09-13-view-filters.md`.

**What this round is.** A third task, View Filters, ported from a working run body Bader
supplied from another host. The body of `Run` was copied as is into
`ViewFiltersRunner` with four edits and the host swaps, and everything else this round
built is around it: its own ribbon panel with one button, its own dockable pane on a fresh
identifier, its own external event and handler, `RcrcGreen.Core/ViewFilters` for every rule
a test can reach, `ViewFilters.json` beside the DLL for the default rows, and a Scan that
reads the same rules the run writes by.

### The four edits, stated plainly

1. **Plot id guard.** The host fell back to the whole view name when nothing parsed, which
   created filters named `(215) Borders Edging General Arrangement Layout` in a real model.
   The whole plot code extraction now goes through `ViewFilterPlotCode.TryFromViewName`:
   the text before `-(` when the name holds one, checked against `^[A-Za-z]{2}-\d+`, the
   pattern's own match when it does not, and a name that starts with neither is skipped,
   logged by name and counted. Nothing guesses.
2. **No rules guard.** When the exemplar's rules could not be copied the host created the
   filter with categories only, and a filter with no rules matches every element in those
   categories. Two guards now, both refusing to create: a null clone, and an exemplar whose
   own tail is empty or is not a plot id. Each counts into `FiltersNotFoundInDoc` and logs.
3. **Keep existing overrides.** Bader chose leave it as it was. The fresh
   `OverrideGraphicSettings` became `v.GetFilterOverrides(filter.Id)`, one line, so an
   override this run does not set survives. The `hasOverrides` check that skips
   `SetFilterOverrides` when nothing is ticked stands exactly as written.
4. **Blocked views.** Before a view is touched, its template is asked whether it owns the
   filters setting, through `GetNonControlledTemplateParameterIds` against
   `VIS_GRAPHICS_FILTERS`. A view whose template owns it is left alone, named with the
   template, counted, and listed on the pane. Nothing here ever writes to a view template.
   `Output` grew `Skipped` and `Blocked`.

### Every other difference from the copied body, named

The instruction was four edits and the host swaps and nothing else. These are the lines
that differ beyond them, each with why. Nothing else in the body changed: the lookup
order, the exemplar rule cloning, the whole override block, the counters, the catch
shapes, the log wordings and the summary lines are the host's, byte for byte where the
list below does not name them.

- The host swaps as specified: `UIDoc.Document` is the handler's document, the transaction
  is named `RCRC Green - View Filters`, `Logger.AppendLine` reaches the pane's log list and
  the report through the handler, `ProgressUpdate` reaches the progress window, and
  `CheckCancellationRequested` is a probe on the runner.
- The keyword split, the row normalisation, the hex parse, the target name, the three name
  matching predicates and the exemplar tail moved to Core word for word, as
  `ViewKeywords.Split`, `FilterRows.Kept`, `HexColor.TryParse`, `ViewFilterNames`. The body
  calls them at the same lines. The round asked for tests over exactly these rules, and a
  test over a copy of a rule is not a test of the rule, which is the fault this repo has
  paid for eight times. The local `ParseHexColor` stays in the body as the one line that
  wraps the bytes in Revit's colour type.
- `FilterRows.Kept` also drops a null row, where the host would have thrown on it. The
  pane never produces one. A JSON file edited by hand can.
- The two collectors moved whole into `CollectTargetViews` and `CollectAllFilters`, and the
  two clone functions from local functions to private statics, unchanged inside, so Scan
  reads the very code the run acts by rather than a copy of it.
- The `-(` search runs ordinal where the host's `IndexOf` was culture sensitive. Ordinal is
  the safe comparison for a marker that is punctuation, and the change is named here
  because it is a comparison semantics change from the reference.
- The early refusal's `Output` and the final one carry `Skipped` and `Blocked`, and the run
  summary gains one line per count when it is not zero. That is the fourth edit's shape.

### Decisions and open questions

- **The runner lives in the Revit project, not in Core.** The round's wiring named
  `RcrcGreen.Core\ViewFilters` for `ViewFiltersRunner`. The body is Revit API end to end,
  and Core referencing the Revit API is the one rule `CLAUDE.md` says breaks the test gate
  outright, so the runner sits in `src/RcrcGreen.Revit/ViewFilters` and every rule a test
  can reach sits in `src/RcrcGreen.Core/ViewFilters`. That is the same split every command
  here has.
- **There is no cancel.** The round wired `CheckCancellationRequested` to the cancel token
  the progress window carries. The KPI progress window carries none, recorded as deliberate
  in `kpi-rules.md`, so the probe answers not cancelled and the call sites stand ready for
  a real token. A cancel that lies is worse than none.
- **The progress window says RCRC Green KPI in its title bar** while a View Filters run is
  under it, because the round said use the window KPI already uses and the title lives
  inside KPI's file, which this task may not edit. Moving the window to Shared with a title
  argument is a round of its own and Bader's call, the same road `PaneLabel` took.
- **The pattern takes lowercase plot ids** because the round gave `^[A-Za-z]{2}-\d+`, while
  the Shared `PlotId` calls lowercase invalid. A test pins today's behaviour and whether to
  tighten it is for the team.
- **The prefix and plot fallback match is the host's** and can in principle land on a
  cousin, a name that starts with the prefix and ends with the plot with anything between.
  Kept as written.
- **Rows edited in the pane are not saved anywhere.** `ViewFilters.json` is read at pane
  build and never written, and an install refreshes it. A user file in `%APPDATA%` like the
  title blocks have is a later round if the team wants edits kept.
- **Apply is also refused when the model is not the one the scan read**, decided on the
  Revit thread at the press, because the pane holds no copy of anything it can ask for. On
  top of that the run re-scans after it writes and hands the grid back fresh, the lesson
  the Drawing Sheet's run paid for three runs running.
- **A plot whose every keyword view is blocked has no row in the grid**, because the run
  will not touch it. Its views are in the blocked list with their templates.
- **install.ps1 gained a copy block** for `ViewFilters.json`, the same shape as the title
  blocks. It copies on every install, which is safe while nothing writes the file.
- **PanelMetrics gained three numbers**, the keyword box height, the swatch and the hex box
  width, marked not seen in Revit, the same way the KPI round added its two.
- The round asked for the report at the top of `steps/log.md`. That file says in its own
  text that nothing writes there any more and a new task starts its own log, so this file
  is that log and `log.md`'s pointer names it now.

### What the review pass found, and what was done about it

Five checkers went over the round before it shipped: a line by line diff of the ported
body against the reference, a breaker over the new code, a claim check over these
documents, a review against the repo's own panel rules, and a field by field check of the
shipped defaults against the round's specification. The port diff found the body faithful,
the defaults matched field for field, and the rest came out as follows.

Fixed in this round:

- **The scan was rewriting the pane's own record of the press.** The ported run trims each
  row's prefix on the row object itself, the pane and the handler shared one object, so a
  prefix typed with a stray space never equalled its own box again and Apply sat on Scan
  again forever. The handler gets a deep copy now, through `InputsCopy`, with a test that
  mutating the copy leaves the original alone.
- **Kept controls wore the old theme after a Revit theme switch.** The rows and the lists
  are built once, so their faint lines, hairlines and row shades now carry a mark and
  `PaintFromTheTheme` walks the tree repainting whatever carries one. Black on black is
  the first fault this repo ever shipped and it does not need a second run.
- **The fresh read now lands before the report write**, so a report that cannot be written
  still leaves the grid telling the truth about the model the run changed.
- The pattern words and the weight range moved to Core, `PatternTypes` and `LineWeights`,
  one record for everything outside the port. The ported body keeps its own literals,
  because the body is a port. The swatch brush is frozen and carries the line saying why
  it is the one brush built outside PanelTheme: it is the user's typed value shown back.

Known and left as ported, for the team rather than for this round to change:

- **The rule cloning rewrites string rules only.** A filter whose plot reference lives in
  any other rule kind is cloned with the exemplar's own value still in it, created under
  the new plot's name, and nothing in the log can tell it from a correct clone. The scan's
  Will create shares the blindness. Both measured models filter plots on text parameters,
  so no measured case hits it, and widening the clone is a change to the ported body.
- **A view that throws on `GetFilters` vanishes with nothing written**, the host's own bare
  catch. It is the one silent skip left in the body, kept because it is the body.
- **The in-string replace can rewrite a longer plot id that contains a shorter one**, the
  same cousin shape as the name fallback, inside the rule values this time.
- **`ExternalEvent.Raise` is not checked** on any pane here. A raise Revit refuses would
  leave the progress window open with nothing said. Inherited shape, all three panes.
- `ReportPlaces` gains a third cross fence reader in this handler, beside the KPI pane's,
  which strengthens the case for the Shared round `revit-commands.md` already names.

### What is left and what comes next

- A run in Revit. Nothing here has seen one, and the grid, the swatches, the row layout on
  a narrow dock and the progress window over a long run are exactly the kind of thing that
  only shows there.
- Known bugs: none known. The untested surface is the whole Revit side.
- Next, in order of worth: Bader's first run and whatever it teaches, a real cancel
  threaded through the loop, a user file for edited rows, the progress window to Shared.
