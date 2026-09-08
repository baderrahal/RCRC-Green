# ai-max state

Phase: 9, ship. On the second pass, a fix round on the scaffold rather than new work.

Fourteen items from the reviewed phase 8 findings were fixed and nothing else was touched.
The rest of the 53 findings stay written down in `steps/log.md` as reported and untouched.
Four of the twelve open questions came back answered and are marked in that list.

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

**9, ship.** Merged. Pull request 1 landed on main as `17f1850`, from branch
`claude/rcrc-green-setup-wf9ham`. The test gate ran on a real runner against that tree and
executed 48 tests, 0 failed, 0 skipped.

Phase 10, packaging, has not started.

## What is built

Three projects in `RcrcGreen.sln`.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference. View name parsing, the plot
  list, the grid, the missing view report, the section maths. A bound that is NaN or
  infinite is refused when a `PlotBox` is built, because an infinite bound survives an
  ordering check and then turns every centre into NaN, which reads as a placement rather
  than as a failure.
- `src/RcrcGreen.Revit`, net48. One `IExternalApplication` that makes the RCRC Green tab
  with an empty Sheets panel, plus `RcrcGreen.addin` and `SectionDefaults`, which holds the
  10 metre section depth and converts it to feet. No commands.
- `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest
  asks for, which the build itself does not produce. Neither has been run, because there is
  no PowerShell and no Revit on the machine this was written on.
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, Core only.

## What is not built

No Revit command logic beyond the empty ribbon. Nothing reads a model yet.
