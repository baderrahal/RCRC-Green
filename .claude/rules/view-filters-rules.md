# The rules the View Filters tool holds

These load when something under a `ViewFilters/` folder is being touched. Everything about
the repo as a whole is in `CLAUDE.md`, and the other tasks' rules are in the other files
here.

## What it is for

Views whose names hold the team's keywords, Location Key Plan, Overall Key Plan, General
Arrangement Layout, get one filter per configured prefix for their own plot, named
`<prefix> <plot id>`, as in `(200-260) Presentation DM-41`. A missing filter is created by
copying the rules of an exemplar, the first filter whose name starts with the prefix, with
the exemplar's plot id swapped for this plot's. Scan shows what a press would do, per plot
and per prefix, Exists, Will create, Cannot create. Apply does it, one transaction, one
undo, one report.

## The body of Run is a port, not an authored thing

It came whole from a working run body on another host, with four edits and the host swaps,
and every line that differs beyond those is named in `steps/log-view-filters.md` under the
first pass. Do not tidy it, restructure it or rename inside it. A change to what the run
does is a change to that ported body and goes in the log as one.

The four edits, which are the tool's own rules now:

- **A view whose name does not start with a plot id is skipped by name.** The pattern is
  `^[A-Za-z]{2}-\d+`, in `ViewFilterPlotCode`, and the old fallback that took the whole
  view name as the plot code is what created `(215) Borders Edging General Arrangement
  Layout` as a filter in a real model. Nothing guesses. The pattern takes lowercase, which
  the Shared `PlotId` does not, kept as given and pinned by a test, and tightening it is
  the team's question.
- **A filter is never created without rules.** A null clone, or an exemplar whose own tail
  is empty or not a plot id, refuses the create, because a filter with no rules matches
  every element in its categories and would wreck the view.
- **Existing overrides are kept.** The settings object starts from
  `GetFilterOverrides`, never fresh, so what this run does not set survives. Bader's
  choice.
- **A view whose template owns the filters setting is left alone and listed** with the
  template's name. Nothing ever writes to a view template.

## One set of rules for the scan and the run

The plot id rule, the keyword split, the row normalisation, the name building, the three
matching predicates, the exemplar tail and the hex parse live once, in
`Core/ViewFilters`, with the tests. The runner's body calls them at the same lines the
host computed them, and the scan reads the same methods and the same collectors, so the
grid and the press cannot part. A second copy of any of them is the fault this repo has
met eight times.

## The pane

Modeless, no Document, no Transaction, no ElementId. Everything goes through
`ViewFiltersRequestHandler` and its own external event, never another pane's. Apply is
greyed on what the pane owns, its boxes compared against its last scan through
`ApplyGate`, compared and never flagged. Whether a model is open, and whether it is still
the scanned one, is decided on the Revit thread at the press. After a run the handler
hands back a fresh scan, so the grid is never older than the run's own writes. Captions go
through `PaneLabel`, colours through `PanelTheme`, sizes through `PanelMetrics`.

## The settings file

`ViewFilters.json` beside the installed assembly holds the default rows, shipped from
`install/ViewFilters.json`, copied on every install, read through `ViewFiltersFile` whose
gate test reads the shipped file itself. The pane never writes it, and a file that cannot
be read leaves one empty row and the reason on screen, never a guessed set. Rows edited in
the pane live only until the pane goes, which is recorded in the log as a question for the
team.

## One colour box, used three times on every row

`HexColorBox` in the Revit ViewFilters folder is the hex box and its colour square, one
control for the line colour and both patterns so the three cannot drift apart. It paints
from the box's text through `HexColor.TryParse`, writes a pick back through
`HexColor.Written` as upper case `#RRGGBB`, and takes typing through `HexColor.Kept`: a
valid hex is taken, a cleared box really clears, and only a mistype keeps the stored
value, so a warning edge means the text on screen is not the value the run would use,
and the scan gate does not move for it. A blank box wears the plain hairline, because
blank is nothing chosen rather than a fault, and it clears the value it sits over, since
a colour surviving behind a blank field is how a removed colour comes back.
The picker is the WinForms `ColorDialog`, `FullOpen`, owned by Revit's main window handle
so it opens in front, its custom colours static for the Revit session. Each override tick
greys its own box, and a greyed box takes no click. The control is code built in the
Revit project, not XAML and not Core, for the reasons the runner's placement records, and
every rule it applies lives in Core with the tests.

## No cancel, on purpose, for now

`CheckCancellationRequested` is wired to a probe that answers not cancelled, because the
progress window this tool reuses is KPI's and carries no cancel by that task's recorded
decision. The call sites stand, so a real cancel is a wiring change and not a body change.
The window's title bar reads RCRC Green KPI over a View Filters run for the same reason,
and moving the window to Shared with a title argument is a round of its own.
