# RCRC Green

A Revit 2024 add-in for the landscape production team, built as separate tasks by sessions
that cannot see each other. Drawing Sheet reads the model, shows a grid of which views exist
per plot, and creates the missing ones. KPI scans a model and fills the client's GRP KPI
Checklist workbook from it. View Filters adds and configures per plot view filters on the
keyword views, a run ported whole from another host, with its rules in
`.claude/rules/view-filters-rules.md`. Coordination Layout and BOQ Schedules are next.

Read `.claude/skills/ai-max/SKILL.md` and `.claude/rules/territory.md` before doing any work
in this repo. The first sets the phase order, the writing rules and the reporting rules. The
second says which files your task may edit. Your task's phase is in its own
`steps/ai-max-state-<task>.md`.

Done means: the tool lists every plot in a model, shows which known view types each is missing,
and creates them with correct names and, for a cross section, a cut across the middle of a plot.

## Running it

**Drawing Sheet** is a dockable panel that reads the model every time it is shown. Four numbered
rows, one open at a time: PLOTS, VIEW TYPES, MARK, SHEETS. A shut row carries its own
summary and an unusable one says why. A filled square is a view that exists and opens on a
click, an empty one is missing and can be marked one at a time, by row, by column or all at
once. Every dropdown comes from the model, and Scan Model reads the whole document.

**Run** is the button docked at the foot, under all four rows, live or grey on what the RUN step
state says and carrying its reason when it is grey. It creates what the marked cells on the
ticked plots ask for: plan views, sections, schedules and sheets. One confirmation, one
transaction, one undo, one report. What it did then takes over step 4's row: two counts read
off the outcome, one line per thing that did not happen with its reason, and a button to the
report and one to the folder it went in.

**KPI Checklist** opens a pane of the model name, KPI Scan, a status line, the template picker,
the output folder, the plot picker and Create. KPI Scan writes nine sections and Create copies a
template, patches it and writes a report. **Neither creates anything in the model**, and the
template is opened for reading and never written to. **The elements a schedule lists are
not the scheduled things:** the softscape schedule returns RVT Link instances, so its printed
rows are its numbers' only route, and its group rows are the only place a phase shows. Every
measured fact is in the KPI rules, by id in `docs/rules/index.md`.

Build `src\RcrcGreen.Revit\RcrcGreen.Revit.csproj` in Release, then run
`.\install\install.ps1`. It builds the `Addins\2024\` layout the build does not, so copying the
output folder by hand leaves Revit unable to find the assembly. `RcrcGreen.sln` also holds
`tests/RcrcGreen.Core.Tests`, which needs the .NET 10 SDK, so building the solution is what the
gate does rather than what an install wants.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

The whole suite. It is what the pull request gate runs and nothing in it needs Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation, in three
  homes: `Shared/` for what every task reads, and one folder per task, `DrawingSheet/` and
  `Kpi/` so far. `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the
  panes. `tests/RcrcGreen.Core.Tests` is net10.0 and covers Core only. `install/` holds the
  PowerShell scripts and `.claude/rules/` the rules per project. Who owns which folder is in
  `.claude/rules/territory.md`
- `reports/` holds what a run wrote and **nothing in it is ever committed.** This repository is
  public and a report carries client view names, sheet numbers and plot identifiers, so
  `reports/README.md` is the only file in it that is tracked

A command is split the same way. The Revit project reads a document into plain strings and
numbers and Core decides and formats, so every report layout, count and rule has a test.

## The rule that keeps the tests possible

**RcrcGreen.Core must never reference the Revit API.** No `Autodesk.Revit` using, no package
reference, no type from it in a signature. The moment Core touches it, the test project cannot
load and the gate stops protecting anything.

Anything that reads a `Document`, a `View` or a `BoundingBoxXYZ` belongs in `RcrcGreen.Revit`.
Pull the plain values out there and put what Core hands back into the model.

## Never read a schedule value by cell position

Ask the heading row which column it is. **A reader that cannot find its column says so rather
than falling back to a position.** Three rounds running, the same fault: the botanical name off
cell 0 and the count off the last number, then a named row off cell 0, then a species row off
cell 0. The first column is the image and an existing species prints with none, so every one of
them was invisible on a plot whose rows all carry photos. The two rules files point here.

## A label is what the file holds, not what a note calls it

A cell found by its label is found by the text THAT CELL carries. All seven KPI templates hold
` Total Green cover (m²)` with a leading space and the client's note writes it without, which is
where the tool's copy came from. Edge whitespace comes off both sides of every whole label
comparison, through one rule, and **the inside is never touched**, because `LOD /  HARDSCAPE
SCHEDULES` really carries two spaces. The KPI rules hold the measurement, K35.09 in `docs/rules/index.md`.

## Three sections that live in docs

**Project facts**, from the team and from real models, are in `docs/rules/project-facts.md`.

**Hooks**: four walls wired in `.claude/settings.json`, described in `docs/hooks.md`. Run `bash .claude/hooks/hook-tests.sh` after any change to one.

**Things that have gone wrong before** are in `docs/gone-wrong.md`. Add to it whenever something breaks.

## Writing and working agreements

**Anything not written down is UNKNOWN.** Never invent a rule. Open questions go in the log.
No em dash, no semicolon in prose, no emoji anywhere, commit messages included. No generated-by
footer and no co-author line. Comments say why, not what. Full list in the ai-max writing rules.
Never report a test result from a run made before the last file was written. Say UNKNOWN rather
than filling a gap. Write what happened in your task's own log, `steps/log-drawing.md` or
`steps/log-kpi.md`, newest entry at the top.
