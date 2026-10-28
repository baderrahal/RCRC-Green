# Drawing Sheet log

Newest entry first.

---

## 2026-09-19, the sixty fourth pass merged

Pull request 165 is merged as `4397284`, **2189 tests on the runner**, 0 failed and 0 skipped,
build 0 warnings and 0 errors. The squash message came back off main byte for byte, nothing
appended. Merged main reads 2189 locally, the same number, which is the 2165 this branch was
cut from plus the round's 24.

**This record's own pull request reads 2216 on the runner, and that is not this round's
number.** The gate tests the merge of a branch into main, and View Filters merged pull request
164 between the two, so 166's run covered their 27 tests as well. Measured rather than reasoned:
`origin/main` at `e2d2ff5` reads 2216 in a throwaway worktree, which is `4397284`'s 2189 plus
their 27.

The entry under this one is the round itself. **Its finding stands open:** five live strings in
`src/RcrcGreen.Revit/ModelWriter.cs` name step 5, at 334, 403, 424, 453 and 472, and the four
refusals among them print in the new result panel. They are not reworded and what they should
say is the user's call.

---

## 2026-09-19, sixty fourth pass. The pane becomes four stacked rows with a result panel

Branch `claude/rcrc-green-setup-wf9ham`, one pull request. Suite **2165 before, 2189 after**,
0 failed and 0 skipped, measured on the merged main this branch was reset onto, `2ee5ca7`.
Twenty four new tests. The runner count and the merge hash go in a short entry above this one
once the merge lands.

**The file name in the brief does not match this repo.** The brief names
`steps/log-drawing-sheet.md` and `steps/ai-max-state-drawing-sheet.md`. This task's pair has
always been `steps/log-drawing.md` and `steps/ai-max-state-drawing.md`, and `CLAUDE.md` names
those two by hand. Creating the pair the brief names would have split one task's record in two,
which is the shape this repo keeps paying for, so the entry is here. The run sheet's name was
new and is used exactly as the brief gave it.

### The mapping, and what it cost

`PanelSteps` still holds five steps. The pane draws **four rows**, 1 PLOTS, 2 VIEW TYPES,
3 MARK, 4 SHEETS. RUN stops being a row and becomes the button docked at the foot plus the
result that takes over step 4's body after a run. `PanelStep.Run`, its `StepState`, `Of()`,
`Header`, `Usable`, `WhyNot` and every string literal in the file are untouched, so every
sentence that names a step number out loud still points where it always did. I checked the
three the brief named and found no fourth.

Three new members on `PanelSteps`, `Rows`, `RowOpen` and `RowAfter`, and one on `StepState`,
`WhenShut`, which is Summary when the step is usable and WhyNot when it is not. Nothing was
restructured and nothing was renamed.

Two things RUN's disappearance did cost, both said here rather than hidden:

- **Step 4's Next button is gone**, because `RowAfter(Sheets)` is null and there is no step
  past it to offer. The Run button at the foot is what took its place.
- **Step 5's body is not a numbered step any more.** None of it is dropped. The run line, the
  three answers a new view type needs, what the run cannot make with its Show control, and the
  six scope box cases are an unnumbered block under the four rows, headed BEFORE YOU RUN, shut
  by default and opened by a click the same way a row is. That is one more thing that can be
  open beside the one open row, which the brief's one-row-at-a-time rule is about rows rather
  than about this block, and the user can overrule it.

### The trap, and where I found it already sitting

The result panel reads `RunOutcome` and never `RunPlan`. `RunResult.Of` takes the outcome and
whatever `ReportFile.Write` returned. The worked count is `outcome.CreatedCount`, the failed
count is `outcome.NotCreatedCount`, and the lines are `outcome.NotCreated` then
`outcome.LeftBehind`, which is exactly the two lists `NotCreatedCount` adds up, so a test pins
`FailedLines.Count` to `Failed`. `BothWays` not being empty raises a line beginning THIS IS A
BUG IN THE TOOL, NOT IN THE MODEL, first and in the warning colour.

While wiring it I found the same fact already written twice. The handler built
`outcome.CreatedCount + " created, " + outcome.NotCreatedCount + " not created."` by hand for
the status line, and the pane was about to build its own. Both read `RunResult.CountsInWords`
now, and the handler's sentence is unchanged byte for byte. The loud line about schedules left
in the model went the same way, into `RunResult.Loud`.

### Long lines at 300 pixels: they wrap

The row is two lines. The number, the title and the tick sit on the first, and `WhenShut` wraps
on the second. **Cutting was not an option.** Step 1's summary reads `DM, 14 of 20 sub plots
ticked` and those two numbers appear nowhere else once step 1 is shut, so an ellipsis would
have eaten the only copy of a number, which the brief rules out. The same treatment is on every
row and on the line above the Run button.

Below 300 the pane scrolls sideways. `PanelMetrics.NarrowPane` is 300 and is the `MinWidth` of
the row stack, and the outer scroller's horizontal bar went from Disabled to Auto so that width
is reachable rather than clipped. The grid in step 3 has had its own sideways scroller all
along, so below 300 there can be two horizontal bars, which is outside the width this is laid
out for.

### The tick is the word done

`writing-check.sh` refuses the whole character range a tick mark sits in, `\u2600` to `\u27BF`,
as an emoji. So a finished row carries the word `done` on the right of its title line, in
`PanelMetrics.Tick`, 34 wide. It also reads at 300 pixels, which a glyph would have had to earn.

### How the pane gets back to step 4's controls

A control on the result reading **Back to the sheets** drops the result and redraws, and
pressing Run drops it before the new run is asked for so a stale result cannot sit under step 4
while Revit works. Rows 1 to 3 are not touched by any of it. Step 4 is opened by the run itself,
because a result nobody can see is not a result, and it draws even if step 4 has gone unusable
underneath it, since the result is about what already happened rather than about what step 4 can
do next.

### The handler's contract grew by one callback

`DrawingSheetRequestHandler.Ran` hands the panel a `RunResult`. It had to: everything a run
had to say left there as one sentence on the status line, and a sentence is not something a
pane can draw two counts and a line per refusal from. The alternative was the panel reaching
back for the outcome, which would be a second way into Revit. It fires on both paths, including
a run nobody confirmed, because an empty area under step 4 reads as a pane that broke.

### A platform difference the tests caught

`RunResult` asked `Path.GetDirectoryName` for the report's folder. That answers by the platform
it runs on, the tests run on Linux, and a real Windows report path came back with no folder at
all. The pane would have offered one button in Revit and two on the runner. It cuts at the last
separator of either kind now, so the answer is the same where it is checked and where it runs.
The test that caught it was written against a real Windows path on purpose.

### Two breaks watched red

`RowOpen` returning the asked-for row without checking `Usable`: 2 red. `Failed` reading
`outcome.NotCreated.Count` and dropping the left behind: 1 red. Both reversed, both files md5
identical before and after, green again at 2189.

### Documents this round made false, and one it found already false

`CLAUDE.md` and `.claude/rules/revit-commands.md` both said five numbered steps and called Run
step 5. Both are corrected, along with six doc comments and one line of the header
`NewViewSetupFile` writes into the user's settings file, which named step 5 in a file the user
reads.

The section headed **Step 5 draws the sheets before it makes them** in `revit-commands.md`
described the 247 preview cards, which a previous round deleted from the panel and left the
paragraph standing. It is replaced and the entry says so. `steps/run-drawing.md` said
`install.ps1` puts a report on the Desktop as well, which stopped being true when the Desktop
copy was dropped, and would have sent the user to the wrong folder on this round's own buttons.
Both were found rather than caused.

### What the claim checker caught

It ran against this entry and the pull request body and came back with one finding worth the
round, one thing it could not check, and no false claim in either document.

**FIVE LIVE STRINGS STILL NAME STEP 5, AND I HAVE NOT REWORDED THEM.** All five are in
`src/RcrcGreen.Revit/ModelWriter.cs` and none is a doc comment:

- `334`, in `FromAnswersInWords`, which goes into the run report under WHERE EACH NEW VIEW AND
  SHEET WAS SET UP FROM: `is set up from the answers saved in step 5`
- `403`, a refusal reason: `the answers in step 5 are short of ... Answer them there`
- `424`, `453` and `472`, three refusal reasons each ending `pick one this model holds in step
  5.`

The four refusals reach the user twice over now: through `RunOutcome.NotCreated`, which is
what `RunResult.FailedLines` is built from, so **this round's own result panel prints them**,
and through the report. They send somebody looking for a step the pane no longer draws.

**They are left exactly as they are on purpose.** The brief said that if a line this mapping
breaks turns up, stop and say so rather than rewording it. This is that line, five times.
The lines are the answers a new view type needs, which now live in the BEFORE YOU RUN block,
so what they should say is the user's call rather than mine.

The one thing the checker could not check is the deliberate breaks, which leave no trace in a
working tree. It read the after count as 1835 Fact methods plus 354 InlineData rows, 2189,
arriving at the same number from the other end.

### One more document found already false

`steps/run-drawing.md` told the reader to expect `Report at <Desktop path> and <repo path>.`
in four places, at lines 173, 254, 264 and 319, and to fetch a report from the repo or the
Desktop at 400. `ReportPlaces.Written` names one path and there has been no Desktop copy for
rounds. That matters to this round because the two buttons on the result open the one folder,
so a reader following that sheet would have reported a fault that is not one. All five are
corrected.

**That document holds other stale expectations from earlier rounds** and I have not swept it.
Its step 34 still expects the status line to read `Press Refresh to see them`, which the
handler stopped saying when a run started handing a fresh read back. A pass over the whole of
it is a round of its own.

### NOT SEEN IN REVIT

Nothing in this round has been through Revit. Every one of these is UNOBSERVED, not working:

- the four rows drawing at all
- the four rows at about 300 pixels, and whether the second line wraps the way it is meant to
- whether the sideways scroll below 300 behaves, and whether two horizontal bars appear in
  step 3 down there
- the word `done` appearing on a finished row and nowhere else
- only the open row showing its controls, and two rows never being open at once
- a shut row showing its summary, and a greyed row showing its reason on the same line
- clicking a row's header opening that row, and a greyed header refusing the click
- the BEFORE YOU RUN block opening and holding everything step 5 held
- the Run button staying on screen at the foot, and going grey with its reason above it
- the result panel appearing in step 4's row after a real run
- the two buttons opening anything at all, the report or the folder
- Back to the sheets putting step 4's controls back
- whether `MinWidth` on the row stack is respected by Revit's own dock
- anything about how any of it looks

`steps/run-sheet-drawing-sheet-pane.md` is the pass that would settle every one of them.

### Open to the user

- The BEFORE YOU RUN block can be open at the same time as a row. Say if it should shut one.
- Its caption is new wording. RUN was taken by the button.
- The build and install half of the new run sheet repeats `steps/run-drawing.md`'s. Two
  records of one procedure, kept because the brief asked for a standalone sheet.

---

## 2026-09-13, sixty third pass. The panel at 35 sub plots, and five faults from the DM-02 run

Branch `claude/rcrc-green-setup-wf9ham`, eight fixes over two pull requests, 105 merged as
`d7ce76e` with 1483 tests on the runner and 106 as `ac17d08` with 1494, both 0 failed
and 0 skipped, and both squash messages back byte for byte. Split as the brief offered,
fixes 1 to 4 then 5 to 8, because 1 to 4 are one change to how the panel scales below step 3
and share three files, and 5 to 8 are separate faults in the writer and the settings.

**Fix 6 first, because it is the one that needed finding, and because half of it is UNKNOWN.**

Two findings, both with file and line, and neither is what the brief guessed.

`CodeFor` TRIMMED its two arguments and compared them ordinally against values
`TitleBlockSettingsFile` deliberately does not trim, at
`src/RcrcGreen.Core/DrawingSheet/TitleBlockSettings.cs:140`. So a pairing on a name carrying
an edge space could never be found. A type in this model is called `LOD /  HARDSCAPE
SCHEDULES` with two spaces, so edge spaces in these names are not hypothetical. I wrote a
probe test against the unchanged code before touching it, it failed expecting `010` and
getting the empty string, and the probe is a kept test now.

The DM-02 cover page failed for a second reason, and it is in the user's own screenshot for
fix 7. The dropdown reads `AR-PRX-Title_Block_A1 Cover Page   COVER PAGE`, and
`TitleBlockType.ToString()` is family, three spaces, then type. So the model's FAMILY is
`AR-PRX-Title_Block_A1 Cover Page` and the shipped file pairs `010 TITLE SHEET` with the
family `AR-PRX-Title_Block_A1`. Two different families. `CodeFor` correctly answers nothing
and no number is ever built for that sheet. **That is not a bug in `CodeFor`**, and fixing
it by loosening the match would have been inventing a rule. Fix 8 is the remedy from the
other side: the type can now be paired once and remembered.

**How the sheet came out numbered 1 is UNKNOWN and I am not naming a likely cause.** The
tool cannot produce it. `SheetNumberRun.Next` returns a built number or `Nothing` and never
a bare digit. `RunPlan.cs:379` refuses a row with no number before the writer sees it. The
number box is a plain `TextBox` starting at the row's own number. I traced every path from
the panel's number build to `ViewSheet.Create` and could not establish which one put a `1`
in the model. What the writer does now is READ THE NUMBER BACK off the sheet after setting
it and refuse the sheet when Revit kept something else, deleting it again. That closes the
hole whatever produced it, and it is the rule this repo already uses for viewport placement.

**Fix 5 is the expensive one.** Five views were refused for no scope box on DM-02 and their
five sheets were created anyway, each reported as made without the view it was waiting for.
Five empty sheets, and five numbers taken in a model where a number cannot be reused. A row
whose every view this same run refused is refused now, naming them. A sheet with no views BY
DESIGN is untouched, so the cover page is still made, and a sheet keeping one of its two
views is still made. The case that would break a careless version is a sheet carrying a view
the model ALREADY HOLDS, which is not refused and not made, so the rule asks whether the run
refused the view rather than whether it made it. There is a test on exactly that.

**The panel at scale.** The run drew 245 plot rows in step 4, 247 preview cards in step 5,
and the twelve item view checklist inside all seven definitions. The cause is mine:
everything below step 3 was designed for one sub plot at a time.

The typed sheet name moved off the row and onto the definition. The model settles that it
never varies by plot: 600001A, 600002A and 600005A are all HARDSCAPE SCHEDULES. One box now,
with the per plot row kept as an override behind a control saying how many rows are behind
it. `SheetNameChoice` is the one resolver and an empty string beats the place below it,
because clearing a box is somebody emptying it. The panel held a SECOND COPY of that rule in
`NameShownFor`, which decides the letter order, so a name shown in the box and ordered by
there would have drifted the moment either changed.

A shut definition is two lines. `StepFourOpen` holds which is open, in Core with 13 tests
rather than three ints on the panel, because taking a definition out shifts every index past
it and that is an off-by-one no test in the Revit project can reach.

The preview is gone. What replaced it counts what cannot be made BY REASON rather than by
sentence, because every sentence names its plot and the five DM-02 refusals would have read
as five reasons. Deleting the preview also took out the `View.Outline` read the panel did
once per view, which I added last round and flagged as unmeasured. Nothing needs it now.
`SheetPlacement` stays, because the writer places from it.

**Fix 4 changed a rule I wrote, and the reason is worth keeping.** The range listed only sub
plots the model holds, so widening it changed nothing and the user reported it as not
working. They were right. The rule stops the TOOL inventing a plot, and a sub plot somebody
types into a range is theirs. DM-02 settles it: no views, no scope box, and the team has
already made its sheets. Both statements of the rule are updated rather than left
disagreeing, in `core-rules.md` and in `CLAUDE.md`.

**One decision in fix 4 is mine rather than the brief's.** What the model holds starts
ticked and what it does not starts listed. The other way round, a single tick on a plot would
queue a run over 96 sub plots nobody has looked at. Both are one click apart and it is easy
to overrule.

**Fix 4 and fix 5 interact, and it is worth knowing.** Now that fix 5 has landed, a sub plot
the model does not hold will have most of its sheets refused, because their views cannot be
made. Only a cover page survives. The two agree rather than fight, but it changes what fix 4
buys and that is the user's call.

Ten `PlotTickListTests` and one `TitleBlockSettingsTests` encoded rules this round changes.
They are rewritten against the new rules rather than weakened.
`EveryKindHasItsOwnHeadingAndNoneShareOne` caught the new refusal kind on its own, which is
what it is for.

Two things about the numbers. **The brief's base had moved twice by the time this ran.** It
says `96b6239` at 1416 tests, and I measured that commit at 1416, so it was right when
written. Main was `6e1b309` at 1456 when this branch was cut and `cda3d37` at 1451 when 105
went to the gate, because `e2e14a3` deleted leftover members and their tests. The runner read
1483 where the branch read 1488, which is 1451 plus this branch's 32, measured in a worktree
rather than reasoned.

**One deliberate break was a no-op the first time I wrote it.** Grouping refusals by sentence
instead of by kind, written as a nested regroup, collapsed back to the same answer and proved
nothing. I noticed it came back green, rewrote it properly, and watched 3 go red. The other
five breaks across the two pull requests went red first time and every reversal left the file
hash equal.

Not observed in Revit, all of it. Specifically: a shut definition at a real pane width,
opening and closing one, the one name box filling all 35 rows, the per plot rows behind their
control, step 5's summary and its refusal list, a 99 line sub plot list and whether it is
usable at that length, ticking a sub plot the model does not hold, the read back catching a
number Revit did not take, whether `ViewSheet.SheetNumber` can silently keep a different
value at all, the five DM-02 sheets now being refused, the line under the picker with a real
choice in force, pairing `AR-PRX-Title_Block_A1 Cover Page` with `010 TITLE SHEET` and seeing
a number built for it, and what the panel now costs to draw with 7 definitions over 35 sub
plots. The mockup is `design/pr-106/panel.html` and it is drawn by hand from the code.

---

## 2026-09-12, sixty second pass, third of three. The sheets, drawn before they are made

Branch `claude/rcrc-green-setup-wf9ham`, pull request 99, merged as `105d750` with 1416
tests on the runner, 0 failed and 0 skipped, and the squash message back byte for byte.

Step 5 draws a card per sheet in the order the run creates them: the title block outline at
the proportion it comes out at, the strip down its right, and a rectangle for every viewport
at the place and the size the run will put it. Clicking one opens step 4 at the described
sheet whose row carries that number.

**The brief named the one thing this fix must not do, and that shaped all of it.** One plan,
one set of positions, read twice. The layout did not exist before the run: the writer called
`SheetFit.Of` in one method to divide the views and `SheetLayout.For` in another to place
them, which was already two readings of one division inside one class, and a preview drawing
its own would have been a third. So `SheetPlacement.Of` is now the only place a sheet is laid
out and the only caller of either. The writer places from it and the card draws from it.
`grep -rn "SheetLayout.For\|SheetFit.Of" src/` returns two lines, both inside
`SheetPlacement.cs`.

**What the preview cannot know it marks.** A view the run is about to create has no size until
Revit draws it, and neither has a schedule, so both are laid out at their cell and drawn
DASHED at nine tenths of it, because a nominal rectangle that looks measured is a drawing that
lies. A view already in the model is drawn at its real size, off `View.Outline`, which needs
no viewport. And a title block NO SHEET IN THIS MODEL USES YET has no size at all, because
Sheet Width and Sheet Height are instance parameters: its card draws nothing and says why.
That is the same null this repo once turned into 0.0 and made three empty sheets with.

Two more things that were written twice are written once. `ViewNaming.Of` builds
`DM-11-(010) Location Key Plan`, where the run plan built it twice, the schedule definition
once and the writer once, and the writer's copy is what every created view is named by. Four
records that happened to agree. `DrawingSheetReader.SizeOnPaper` is the one `View.Outline`
read and the writer calls it. **`ViewNaming` belongs in Shared beside the parser that reads a
name apart**, and it is not there because a change to Shared stops every other session. That
is an open question for the user, in a round of its own.

**The runner read 1416 where this branch read 1400, and that is not a disagreement.** Main
moved while the pull request was open, `6275bf3` and `1bd0f58` from the KPI task, so the base
the runner merges into had grown by 16. Measured rather than reasoned: main on its own reads
1385 now where it read 1369 when this branch was cut, and 1385 plus this branch's 31 is 1416.
The merged main reads 1416 locally too. Two deliberate breaks watched red and reversed with the file hash equal before and after: every
view landing on the first spot took 4 red, and an unmeasured view coming back as measured
took 2.

Not observed in Revit, all of it. Specifically: a card at a real pane width and whether the
cards wrap readably, `FamilySymbol.Id` deduplicating the title block types on a model with
several, whether the writer still places correctly now its spots come off `SheetPlacement`,
the click scrolling step 4 to the right sheet, the dashed rectangles against either theme,
and **what one extra `View.Outline` read per view costs**. The whole read was measured at 1.4
seconds for 96,934 elements before this round and has not been measured since. The mockup is
`design/pr-99/panel.html` and it is drawn by hand from the code.

---

## 2026-09-12, sixty second pass, second of three. A preset saves steps 2 and 4

Branch `claude/rcrc-green-setup-wf9ham`, pull request 98, merged as `e2aa4ad` with 1369
tests on the runner, 0 failed and 0 skipped, and the squash message back byte for byte.

Eleven title blocks and six sheets were described by hand on the DM-11 run and the same
eleven and the same six would be described again on the next plot. A preset holds the ticked
view types and the sheet definitions, each one a title block, a views per sheet and its views
in the order they go on. **It holds no plot, no sub plot and no sheet number**, because those
are what changes between one run and the next, and a preset carrying them would fill step 1
with last week's plots and put a number on a sheet another plot already has.

Core, with 49 tests, in five types. Three rules are the whole of it. **A record with nothing
above it to belong to is not read**: the format is tab separated with the first field saying
what the line is, and a `type` line before any `preset` line is kept as a line that could not
be read rather than attached to the preset that comes next. **The part that fits is filled in
and the part that does not is named**, because a preset is shared across projects the way the
title block settings are, and refusing all of it over one missing schedule would make it
useless on every model but the one it was saved from. **`PresetFilling` compares rather than
remembering**: a tick, a sheet added, a sheet removed, a title block picked and a views per
sheet changed can all move steps 2 and 4, and the flag for the sixth one is the one nobody
sets.

`install/presets.txt` ships the six sheets of the DM-11 run, its view types grouped by the
title block `install/title-blocks.txt` pairs each with, so five definitions and six sheets at
one view each. Two shipped files are two records that have to agree, so a test reads both and
fails if a preset ever names a block its own pairing does not.

The strip gains a second row under the model name, a picker with a blank first entry, Save as
and Manage. **It is its own row rather than beside Refresh and Scan Model**, which is a
deliberate departure from the brief: a pane on the right of Revit is narrow, the first row is
already the model name and two buttons, and this panel has shipped controls running off the
right edge once already. Picking a preset turns every tick off first and then fills, so what
is on screen is the preset rather than the preset laid over what was ticked before, and the
blank entry undoes nothing.

`InTheModel` is now the one title block lookup on the panel. It was written three times, once
for the settings, once for a described sheet and once here, each with its own idea of how two
names are compared.

Suite reads 1369 locally and 1369 on the runner, the 1320 on merged main plus 49. Two
deliberate breaks watched red and reversed with the file hash equal before and after: a title
block the model does not hold being filled in anyway took 5 red, and the filled from line
always claiming the preset took 1.

Not observed in Revit, all of it. Specifically: the second strip row at a real pane width, the
picker holding a real model's presets, the Save as and Manage windows over Revit,
`%APPDATA%\RCRC Green\presets.txt` being written and read back, `install.ps1` copying
`presets.txt` beside the add-in, and what picking the shipped preset does to steps 2 and 4 on
a real model. The mockup is `design/pr-98/panel.html`.

---

Older entries were moved whole, byte for byte, on 2026-10-28: `steps/history/log-drawing-1.md` to `steps/history/log-drawing-3.md`, newest first.
