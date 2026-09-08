# ai-max state

Phase: 9, ship. Fifth pass, the Drawing Sheet panel.

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
opens it. A cell that does not can be marked. A mark records intent and writes nothing.

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
skipped every time. The Drawing Sheet round is pull request 5.

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
  `DrawingSheetPanel` which is the `IDockablePaneProvider`, `DrawingSheetRequestHandler`
  which is the only route from that panel to the API, `DrawingSheetReader`, `ModelScanner`
  and `ScopeBoxScanner` which read a document into plain values, `ScanProgressWindow` which
  shows how far a read has got and can stop it, `RcrcGreen.addin`, and `SectionDefaults`,
  which holds the 10 metre section depth and converts it to feet.
- `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest
  asks for, which the build itself does not produce.
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, Core only.

## What is not built

Nothing creates a view, a sheet or a section. Marking a cell in the panel records intent and
writes nothing. Creation is the next round.

Nothing in the Revit project has been run on this machine. It compiles against the Revit 2024
reference assemblies and no more than that can be said from here. The panel especially, since
a dockable pane, an external event and a WPF tree built in code can all compile and still be
wrong the first time Revit loads them.
