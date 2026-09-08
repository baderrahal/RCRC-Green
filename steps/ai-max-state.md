# ai-max state

Phase: 9, ship.

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
- `writing-check.sh` on Bash. Refuses a commit whose staged files hold an em dash, a
  generated-by footer, a co-author credit line, an emoji, or a banned word.

**6, test gate.** `.github/workflows/tests.yml`. Runs on pull requests into main, on
ubuntu-latest, over `tests/RcrcGreen.Core.Tests` only, and fails when the result file reports
zero tests.

**7, split.** One session. Everything written so far is Core, which is testable here, but the
work is small enough that a split would cost more in shared file collisions than it saves.
The add-in project can only be tested inside Revit, so it never splits.

**8, check.** `breaker` and `claim-checker` in `.claude/agents/`, both read only.

**9, ship.** The scaffold, the Core logic, the tests and the harness on branch
`claude/rcrc-green-setup-wf9ham`.

## What is built

Three projects in `RcrcGreen.sln`.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference. View name parsing, the plot
  list, the grid, the missing view report, the section maths.
- `src/RcrcGreen.Revit`, net48. One `IExternalApplication` that makes the RCRC Green tab
  with an empty Sheets panel, plus `RcrcGreen.addin`. No commands.
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, Core only.

## What is not built

No Revit command logic beyond the empty ribbon. Nothing reads a model yet.
