# ai-max state

Phase: 9, ship. Twelfth pass, the first real write, and everything it found.

Four plan views were attempted in Revit and none was created. Seven faults came out of that one
run and its reports, and all seven are fixed here. None of the fixes has been run.

**The report said both created and not created.** Four names under PLAN VIEWS and the same four
under NOT CREATED, while the panel said nothing was made. The created sections printed
`RunPlan.Items`, which is the intention. `RunOutcome` is what the run did, every counted section
reads it, and a name found on both sides prints as a bug in the tool.

**The family type rule was wrong, and so was matching on names at all.** A view called
DM-11-(010) Location Key Plan is made with a type called `(010) Key Location Plan`, words
swapped. Three of the four refusals were that. The family type, the level and the view template
now all come off a view of the same type the model already holds on another plot. Prefix
matching for the template went with it, along with `ViewTypeNaming`, because eight templates
start with (200) General Arrangement Layout and it could never have answered.

**A cross section is not a plan view.** `ViewPlan.Create` can never make one, which is what
refused the fourth, with a message that blamed the level. There is a section path now, built on
`SectionPlacement`, `PlotBox` and `SectionAxis`, which had been in Core unused since they were
written. Which types need one is read off the kind of the views the model holds.

**The scan could not have shown either fault.** It lists view family types now, and it names the
six views whose name and PRX_Plot_ID disagree rather than only counting them.

**Sheets are answered.** The user sets one plot's sheet up by hand and it is copied.
`SheetDefinition` holds the title block, the size and one placement per view. `SheetCapture`
reads one, `ModelWriter` builds one, and the panel takes a source sheet and a table of numbers
and names. The tool still invents neither half.

Before that, a guarded delete, reports where the code can read them, and scope box cases that
open.

`ModelWriter` deletes a schedule that lost a filter, and that delete was unguarded. A Revit
refusal there would have left a wrong schedule in the model while the report said it was gone.
`ModelWriter.Deleted` now returns true only when the element really went, and a false moves the
item to a third list that prints under CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND,
with a banner at the top of the report and a loud prefix on the panel status line.

Every report now lands in `reports/` inside the repo as well as on the Desktop, same file name
in both, so anything reading the code can read a run. `install.ps1` writes the absolute path
into `reports-folder.txt` beside the installed assembly, because Revit runs the add-in out of
the Autodesk Addins folder and has no other way to know. No pointer file means the Desktop only
and the panel says so. That folder is in `.gitignore` and stays there. This repository is
public and a report carries client view names, sheet numbers, plot identifiers and schedule
field lists. `reports/README.md` is the only tracked file in it and says why.

The six scope box counts were six numbers and a button. Five of them open now. Clicking a count
lists its views by plot and name with the scope box each one holds, and clicking a view opens it
in Revit through the external event. Case B is 102 schedules and stays a count. Assign still
acts only on case C, still confirms, still one transaction.

Before that, the write path stopped skipping things quietly.

`ModelWriter` skipped a schedule field it could not resolve and a filter it could not apply,
both with a bare `continue` and nothing written down. Both are recorded now and the two are
handled apart. A schedule short of a field is created and named in the report. One short of a
filter is deleted again inside the same transaction and refused, because it would show every
plot's elements and read as correct on a drawing.

A screenshot of DM-18 also settled how a plan view is really set up. The view family type is
named after the view type, the level comes from an existing view of that type, and the view
template carries the scale, detail level, discipline and phase filter. All three were guesses
in the code and all three now refuse rather than fall back.

Before that, creation, and the model turned out to have two plot parameters.

The biggest fact of the round is in `CLAUDE.md`. `PRX_Plot_ID` sits on views and sheets and
the Sheet List filters on it. `PRX_Ref Plot ID`, with spaces, sits on model elements and every
quantity schedule filters on that one. A schedule built against the wrong one comes back
empty.

Six of the things under a plot are schedules rather than plan views. `ScheduleDefinition` in
Core holds one as plain values, `ScheduleCapture` reads an existing schedule into one, and
`ModelWriter` builds a schedule from one with the plot swapped. Duplicating is the two run
back to back, and a model with no schedule to copy is a later round rather than a rewrite.

The Run section makes what the marked cells on the ticked plots ask for, in one transaction.
No sheet is created, because which title block a sheet takes and where a view sits on it have
not been answered and this repo does not invent a rule.

Before that, the grid stopped lying about which views exist.

Run on a real model, the panel showed views under a plot whose views had all been deleted.
The cause is in the code and needs no Revit to see. `DrawingSheetReader` took a view's plot
from PRX_Plot_ID and its view type from the name, and never checked the two agreed, so a view
named for DM-12 carrying PRX_Plot_ID DM-11 filled a DM-11 cell. `ViewReading` now decides
both in one place and a cell is filled only by the plot in the view's own name. The
disagreement is counted and shown instead.

The same round added a tick box on every plot row, made every view type a column with a
checklist for hiding, put the six scope box case counts on screen for the ticked plots, and
cut the ribbon down to one panel with one button.


Before that, the panel was installed and run in Revit 2024 for the first time. It opened and
docked and was unusable. Every TextBlock rendered black on the dark theme's black pane, and the panel
opened with an empty prefix dropdown because nothing asked for a read until Refresh was
pressed. Both are fixed. `PanelTheme` reads `UIThemeManager.CurrentTheme` and the panel
paints its own background and foreground once, so no brush is written in the panel file at
all. The pane asks for a read through the external event when it becomes visible, and the
grid slot always carries a line saying what to do rather than being blank.

The round before was three audit fixes, all the same shape. A failure in the panel was
allowed to reach further than the panel. Registering the pane cannot cost the ribbon, a click
on a view Revit will not activate cannot cost a crash dialog, and nothing at all leaves the
external event handler.

Both commands have now been run on a real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached,
and this round is the first built on measured numbers rather than on the project facts
alone. The numbers are in `CLAUDE.md` under Project facts. Three of them changed the design.
The read is fast, so the panel has no progress window. PRX_Plot_ID is on views and answers
for 1,269 views whose names do not parse, so it is now the first source. There are 406 scope
boxes for 160 plots, so a box name that is not a plot identifier is ordinary.

The ribbon now holds two panels. Drawing Sheet holds the dockable panel. Reports holds Scan
Model and Scope Box, unchanged, because a control that moves with the thing it measures is
not a control.

Drawing Sheet is modeless and reaches Revit through one `ExternalEvent` and nothing else. It
shows a range of plots, one row each, one column per view type, and a cell that holds a view
opens it. A cell that does not can be marked. A mark records intent and writes nothing. Cells
carry one character rather than a word, with the words in the tooltip.

Scan Model reads a document and writes a report. It creates nothing and changes nothing.

Scope Box gives every view that names a plot the scope box named for that plot. It is the
first command that writes. It decides everything with no transaction open, shows the counts,
asks, and only then writes, in one transaction so the whole run is one undo.

Before this, a fix round closed fourteen items from the reviewed phase 8 findings. The rest
of the 53 stay written down in `steps/log.md` as reported and untouched. Four of the twelve
open questions came back answered and are marked in that list.

Phases 1 and 2 were settled before this repo was opened. The tool is a Revit 2024 add-in
for the landscape production team, and it is written in C# against the Revit 2024 API
because that is what a 2024 add-in loads.

## What each phase produced

**3, scan.** The repo held one commit carrying `.claude/skills/ai-max` and nothing else. The
full item by item table is the oldest entry in `steps/log.md`.

**4, rules file.** `CLAUDE.md`, 198 lines. Names `.claude/skills/ai-max/SKILL.md` on line 7
and says to read it before any work. Carries the project facts, the measured numbers from the
real model, the rule that `RcrcGreen.Core` never references the Revit API, and the test
command. `.claude/rules/revit-commands.md` carries the rules for writing a command or the
panel and loads only when `src/RcrcGreen.Revit` is touched, because the file was at the 200
line limit and the panel rules had to go somewhere they would still be read.

**5, hooks.** Three scripts in `.claude/hooks/`, wired in `.claude/settings.json` alongside a
SessionStart hook that prints one line pointing at the skill.

- `block-paths.sh` on Write, Edit and NotebookEdit. Refuses a write outside the repo.
- `require-file-on-commit.sh` on Bash. Refuses a commit without this file in it.
- `writing-check.sh` on Bash. Refuses a commit whose message or carried files hold an em
  dash, a generated-by footer, a co-author credit line, an emoji, or a banned word.
- `commit-scope.py` is not a hook. It reads the command and works out what the commit will
  really contain, so the two hooks above see the git commit -a form and the form that names
  paths on the command line rather than reading the index and guessing.

All three were run by hand against crafted input before being trusted. The file list inside
`writing-check.sh` is NUL separated, because a file name holding a space or a quote arrived
at the scanner in pieces otherwise and the pieces read as files that do not exist, which is
a file going through unchecked rather than a visible failure.

**6, test gate.** `.github/workflows/tests.yml`. Runs on pull requests into main, on
ubuntu-latest, over `tests/RcrcGreen.Core.Tests` only, and fails when the result file reports
zero tests.

**7, split.** One session. Everything written so far is Core, which is testable here, but the
work is small enough that a split would cost more in shared file collisions than it saves.
The add-in project can only be tested inside Revit, so it never splits.

**8, check.** `breaker` and `claim-checker` in `.claude/agents/`, both read only. They were run
over the repo along with four other reading passes, and the 53 findings are written up under
Phase 8 review findings in `steps/log.md`. Nothing was fixed in response to them. The phase 8
run is closed.

**9, ship.** Merged four times so far, all from branch `claude/rcrc-green-setup-wf9ham`. Pull
request 1 landed the scaffold as `17f1850`, 2 the fix round as `57dd1ee`, 3 the Scan Model
command as `04ff9fc`, and 4 the Scope Box command as `c5b8c9a`. The test gate ran on a real
runner against each and executed 48 tests, then 79, then 109, then 136, 0 failed and 0
skipped every time. Pull request 5 landed the Drawing Sheet panel as `999381c`, and the gate
executed 176 tests against it, 0 failed and 0 skipped. Pull request 7 landed the three audit
fixes on that panel as `20e214a`, and the gate executed 176 tests against it, the same count,
because that round changed no Core code. Pull request 9 landed the readability round as
`2b5361e`, also 176, because it too was Revit side only. Pull request 11 landed the round that fixed
the grid as `63d3a38`, and the gate executed 214 tests against it, up from 176 because it is
the first round since the panel was built to change Core. Pull request 13 landed creation as `f2c2eb5`,
and the gate executed 248 tests against it. Pull request 15 landed the write path fixes as
`792c203`, and the gate executed 257 tests against it. Pull request 17 landed the guarded
delete, the reports folder and the openable scope box cases as `a18a2ae`, and the gate executed
274 tests against it, 0 failed and 0 skipped.

That branch was asked to be deleted once merged and it could not be. The git proxy here
refuses a ref deletion, and the log entry for that round records what was tried.

Phase 10, packaging, has not started.

## What is built

Three projects in `RcrcGreen.sln`.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference. View name parsing, the plot
  list, the grid, the missing view report, the section maths, the plot source ordering in
  `ViewPlotReader`, the range filter in `PlotRange`, and the grid shaping in `SheetGrid`. A
  bound that is NaN or infinite is refused when a `PlotBox` is built, because an infinite
  bound survives an ordering check and then turns every centre into NaN, which reads as a
  placement rather than as a failure.
- `src/RcrcGreen.Revit`, net48. One `IExternalApplication` making the RCRC Green tab, the
  Drawing Sheet panel and the Reports panel, three `IExternalCommand` classes,
  `DrawingSheetPanel` which is the `IDockablePaneProvider` and the only thing on the ribbon,
  `PanelTheme` which holds every colour the panel paints in, one set per Revit theme,
  `ScheduleCapture` and `ModelWriter` which read a schedule into a definition and build one
  from it and record everything they could not do, `DrawingSheetRequestHandler`
  which is the only route from that panel to the API, `DrawingSheetReader`, `ModelScanner`
  and `ScopeBoxScanner` which read a document into plain values, `ScanProgressWindow` which
  shows how far a read has got and can stop it, `RcrcGreen.addin`, and `SectionDefaults`,
  which holds the 10 metre section depth and converts it to feet.
- `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest
  asks for, which the build itself does not produce.
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, Core only.

## What is not built

Sheet creation and section creation are both written and neither has run. The three questions
that blocked sheets are answered: a sheet is copied from one the user set up by hand.

Nothing in the Revit project has been run on this machine. It compiles against the Revit 2024
reference assemblies and no more than that can be said from here. The panel especially, since
a dockable pane, an external event and a WPF tree built in code can all compile and still be
wrong the first time Revit loads them. That goes for the three failure paths added in the
sixth pass as well. None of them has been made to happen.

**The write path has run once and created nothing.** Four plan views were attempted and all
four were refused. That run is the source of everything in this round, and none of the fixes it
produced has itself been run: not one view, section, schedule or sheet has ever been created by
this tool. The section path and the sheet path have never executed at all. The newest
`steps/log.md` entry lists what that leaves unchecked, item by item.

The mockups under `design/` are hand drawn, not screenshots, and each says so at the top of the
file. Every round that changes the panel writes one.
