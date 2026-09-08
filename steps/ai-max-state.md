# ai-max state

Phase: 9, ship. Fourth pass, the second command.

The scaffold is merged and the add-in has been confirmed loading in Revit 2024.3, with the
RCRC Green tab and the Sheets panel both appearing. Two commands sit on that panel now.

Scan Model reads a document and writes a report. It creates nothing and changes nothing,
because the naming pattern in the project facts came from four examples and the first real
model does not fit it.

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

**4, rules file.** `CLAUDE.md`, 134 lines. Names `.claude/skills/ai-max/SKILL.md` on line 7
and says to read it before any work. Carries the project facts, the rule that
`RcrcGreen.Core` never references the Revit API, and the test command.

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

**9, ship.** Merged four times, all from branch `claude/rcrc-green-setup-wf9ham`. Pull request
1 landed the scaffold as `17f1850`, 2 the fix round as `57dd1ee`, 3 the Scan Model command as
`04ff9fc`, and 4 the Scope Box command as `c5b8c9a`. The test gate ran on a real runner
against each and executed 48 tests, then 79, then 109, then 136, 0 failed and 0 skipped every
time.

That branch was asked to be deleted once merged and it could not be. The git proxy here
refuses a ref deletion, and the log entry for that round records what was tried.

Phase 10, packaging, has not started.

## What is built

Three projects in `RcrcGreen.sln`.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference. View name parsing, the plot
  list, the grid, the missing view report, the section maths. A bound that is NaN or
  infinite is refused when a `PlotBox` is built, because an infinite bound survives an
  ordering check and then turns every centre into NaN, which reads as a placement rather
  than as a failure.
- `src/RcrcGreen.Revit`, net48. One `IExternalApplication` making the RCRC Green tab and the
  Sheets panel, two `IExternalCommand` classes behind the Scan Model and Scope Box buttons,
  `ModelScanner` and `ScopeBoxScanner` which read a document into plain values,
  `ScanProgressWindow` which shows how far a read has got and can stop it, `RcrcGreen.addin`,
  and `SectionDefaults`, which holds the 10 metre section depth and converts it to feet.
- `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest
  asks for, which the build itself does not produce.
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, Core only.

## What is not built

Nothing creates a view, a sheet or a section. The grid and the creation logic wait on the
naming being settled from a real scan.

Nothing in the Revit project has been run on this machine. It compiles against the Revit 2024
reference assemblies and no more than that can be said from here.
