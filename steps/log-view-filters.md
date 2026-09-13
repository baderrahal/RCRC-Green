# View Filters log

Newest entry first.

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
