## The panel is four numbered rows and a Run button

`PLOTS`, `VIEW TYPES`, `MARK`, `SHEETS`, in the order somebody does them. One open at a
time. It was a flat list of controls before, which read as a wall to anyone who had not built
it.

**RUN is not one of the rows.** It is the button docked at the foot, under all four, and the
result of the last run, which takes over step 4's body once there is one. `PanelStep.Run` and
its `StepState` are unchanged in Core: what used to grey step 5 greys the button now, and what
step 5 said while it was shut is the line above it. Nothing renumbered, so every step number
the other four say out loud still points where it always did. `PanelSteps.Rows` is the four the
pane draws, `RowOpen` says which one is open and `RowAfter` which one the Next button offers.

**What step 5's body held is not dropped.** The run line, the answers a new view type needs,
what the run cannot make and the scope box cases are an unnumbered block under the four rows,
headed BEFORE YOU RUN, shut by default and opened by a click the same way a row is.

**The result panel reads `RunOutcome` and never `RunPlan`.** `RunResult` in Core takes the
outcome and the paths the report write returned, and hands back both counts, one line per
refusal and per schedule left in the model, the sentence about where the report went, and the
two paths the buttons open. The handler's status line reads its counts off the same object, so
the line under the pane and the line inside it are one record. The plan is what a run intends
and the outcome is what it did, and reading one where the other belongs is what put four views
under created and under not created in the first real report.

Four rules hold the rows together.

**A shut row carries its own summary**, so the whole state reads without opening anything. The
number, the title and a tick when the step is done sit on one line, and `StepState.WhenShut`
goes under it, wrapped rather than cut: step 1's sub plot counts live nowhere else once it is
shut, and an ellipsis eating the only copy of a number is what a narrow pane must not do. The
tick is the word `done` rather than a tick character, which `writing-check.sh` refuses along
with the whole range it sits in.

**A step that cannot be used yet is greyed out with one line saying why**, on the same line a
usable row puts its summary. A disabled control with no reason next to it tells nobody
anything, and that holds for the Run button too.

**Every summary and every reason is in `PanelSteps` in Core, with a test.** None of it is
formatted next to the control that shows it. That is the same shape as the grid cell, the
column count and the run report, and it has been the bug three times.

**Nothing drags the user out of a step they are working in.** NOTHING opens another step
by itself. Ticking a plot used to fold step 1 and open step 2 while the user was still
choosing sub plots. The Next button at the foot of the open step and a click on any header
are the only two ways a step opens.

**Step 1 is a tick list of plots, and the sub plots are worked in rather than scrolled
past.** Every two letter prefix the model holds is a plot line, always visible and never
inside a scroller. A ticked one opens From and To over the digits 01 to 99, the same on
every model because the team works across them, a search box with All and None over what it
shows, and its own bounded list of sub plot lines that scrolls inside itself so the steps
below stay reachable. Each line is the tick, the identifier and what its sheet numbers will
be built from, labelled: it read `DM-02DM02` for one round, the two run together with
nothing between them. The search box is made once and kept, and typing in it refills the
lines under it rather than redrawing the step, because rebuilding the tree under the cursor
takes the keyboard out of the box.

The scope box counts live inside the BEFORE YOU RUN block rather than in a section of their
own, because they act on the same ticked plots the run does.

## The strip, the steps and the status line

The strip at the top holds the model name, when it was last read, Refresh and Scan Model. Those
belong to the document rather than to any one step. The status line is docked at the bottom, so
it is in the same place whatever is open above it. Both are built once and repainted rather
than redrawn, which is why `PaintFromTheTheme` sets their brushes by hand: a colour set once,
before the theme it follows is read, is how the panel came up black on black the first time.

## Marking a lot of cells at once

17 plots by 8 view types is 136 clicks, and the first person to use the grid did exactly that.
Three ways in: **Mark every missing**, a **plot name** for its row, a **column header** for its
column, and **Clear all marks** to undo the lot.

All of them go through `BulkMarking` in Core, which hands back only cells a single click on the
grid would have marked. A square holding a view is never swept up, because marking says a view
is wanted and that one is there. An unticked plot is never swept up either, because the run does
not act on it and the marks would be dropped without a word. A single click on one square is
still allowed anywhere, because that is a deliberate act rather than a sweep.

Every one of those four was already built and wired when the round that asked for them
came round again. They are listed here so the next round checks before building a second
copy: the plot name at `WholeRow`, the column header at `WholeColumn`, Mark every missing
at `EveryMissing`, and Clear all marks.

A plot name and a column header are labels sitting inside a borderless button. `Flat` builds
them with `PanelTheme.Clear` and `PanelMetrics.Nothing`, so the frozen column stays in step with
the scrolling cells beside it. A real button there would put chrome down the side and across the
top of a grid that is already dense.

## A column header is the code, not the whole name

`(200) General Arrangement Layout` is thirty characters over a column one square wide, and eight
ticked view types ran the headers off the right edge with sideways scrolling the only way to
reach the last of them.

`GridColumnLabels.For` gives each column the shortest header that still says which it is: the
code alone, and where a code is shared, the code plus as many leading words of the view name as
it takes to tell the sharers apart. Code 010 appears twice on the real model, which is why the
code alone can never be the whole answer. The full name is on the tooltip, and the key under the
grid lists every header that lost something, so a grid of plain codes carries no key at all.

## Scan Model is the check on the panel

It reads the whole document rather than the range, so its numbers are worked out a different
way from the panel's and can be held against them. That is why it is still there and why it is
not narrowed to the range when the panel is.

The panel and Scope Box used to disagree on purpose about where a view's plot comes from. They
do not any more. Both read it from the view's own name, because the panel filing a view under
PRX_Plot_ID while taking the view type from the name is what put a view in the wrong row. The
comparison that difference was kept for has been made, and it came out against it.

## A preset fills steps 2 and 4 off the strip

The strip carries a second row under the model name: a picker of saved presets with a blank
first entry, Save as and Manage. It is a row of its own rather than beside Refresh and Scan
Model because a dockable pane on the right of Revit is narrow, the first row is already the
model name and two buttons, and this panel has shipped columns running off the right edge once
already.

**Two files, the same shape as the title blocks.** The user's own at `%APPDATA%\RCRC
Green\presets.txt`, then `install/presets.txt` beside the add-in. `PresetStore` holds the two
paths and hands back the shipped list separately, because deleting the user's copy of a shipped
preset puts the shipped one back and working that out needs both. No Revit API call reads or
writes either, so the panel does it directly.

Picking one ticks exactly what it names in step 2 and describes its sheets in step 4. **Every
tick goes off first**, so what is on screen is the preset rather than the preset laid over
whatever was ticked before. **Step 1 is never touched.** The blank entry means no preset and
undoes nothing: the steps stay and only the line saying where they came from goes, because a
picker that emptied somebody's sheets on the way past blank would be the most expensive click
on the panel.

A sheet is described with the model's own `TitleBlockType` object, found by `InTheModel`, which
is now the one lookup on the panel for a named block. It was written three times, once for the
settings, once for a described sheet and once for a preset, each with its own idea of how two
names are compared.

Save as asks for a name in a modal window owned by the Revit main window, says when the name is
already in use and what saving over it does, and saves what steps 2 and 4 hold at that moment.
Manage lists every preset with what it holds and where it came from, and offers Delete on the
user's own only, because nothing here writes the shipped file and a Delete that looked like it
worked and did not would be worse than none.

Both steps carry the line `PresetFilling` gives, and an edit shows as an edit because the line
is worked out by comparing rather than by a flag.

## The run is counted, not drawn

The panel drew a card per sheet before the run, at real proportion with every viewport on it.
A run over 35 sub plots with 7 definitions drew 247 of them and the user asked for the drawing
to go. **This section described those cards for a round after they were deleted**, which is
what a rules file costs when a round removes something and leaves the paragraph standing.

What is there now is two numbers somebody presses Run on: what the run will make, and what it
will not, counted by reason rather than printed one line each, with the sentences behind a
control. `RunSummary` works both out from the plan.

`SheetPlacement` stays, because `ModelWriter` places from it, and
`DrawingSheetReader.SizeOnPaper` stays because the writer measures its fit with it.

## A count on the panel opens into the thing it counted

Six numbers tell somebody how much is wrong and nothing about what. Every scope box case except
B is a button, and clicking it lists its views by plot and name with the box each one holds.
Clicking a view in that list goes through the external event like any other request, because
the panel opens nothing itself.

The lists come from `ScopeBoxCounts.In`, the same object the counts come from, so a case list
can never be a different length from the number above it. Case B is 102 schedules that cannot
hold a scope box at all, which is a count and nothing more.

