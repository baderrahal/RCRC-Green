# Territory

Four tasks share this repo and the sessions building them cannot see each other. The
boundaries live here and in `.claude/hooks/territory-check.sh` rather than in anybody's
head. Read this before touching anything.

## The tasks and where each lives

1. **Drawing Sheet.** `src/RcrcGreen.Core/DrawingSheet`, plus its Revit files, which sit at
   the root of `src/RcrcGreen.Revit` because the tool predates the folder convention there.
   The hook names them one by one: the panel, the readers, the writer, the model scanner and
   its progress window, the schedule capture, the sibling reader, the scope box scanner, the
   section defaults, the sheet being described and the commands. Its test files sit flat at
   the root of `tests/RcrcGreen.Core.Tests`.
2. **KPI.** `src/RcrcGreen.Core/Kpi`, `src/RcrcGreen.Revit/Kpi` and
   `tests/RcrcGreen.Core.Tests/Kpi`.
3. **Coordination Layout**, not started. `src/RcrcGreen.Core/CoordinationLayout`,
   `src/RcrcGreen.Revit/CoordinationLayout` and
   `tests/RcrcGreen.Core.Tests/CoordinationLayout` when it begins.
4. **BOQ Schedules**, not started. `src/RcrcGreen.Core/BoqSchedules`,
   `src/RcrcGreen.Revit/BoqSchedules` and `tests/RcrcGreen.Core.Tests/BoqSchedules` when it
   begins.

`src/RcrcGreen.Core/Shared` belongs to no task and holds what every task reads: the plot
identifier and its registry, ranges and selections of plots, the view name parser and the
view type, natural ordering, lengths, points and vectors, and the scan file name. The
folder listing is the full record.

Five Revit root files belong to every task and change rarely: `RcrcGreenApplication.cs`,
`PanelTheme.cs`, `PanelMetrics.cs`, `ReportFile.cs` and `RcrcGreen.addin`. Everything else
at that root is Drawing Sheet's.

The rules files in `.claude/rules/` are read by everyone. Each task extends the one about
its own code and leaves the others alone.

## The rules

- A session works only in its own task's folders, plus Shared when the rule below allows.
- Any task may read any Shared file and any other task's file. All of Core compiles as one
  assembly, so a KPI file calling a Drawing Sheet class builds fine. Editing is what is
  fenced, never reading.
- Only the owning task edits its own files. Drawing Sheet owns everything it built,
  ScheduleDefinition, ScheduleCapture, SheetLayout, SheetNumbers and SheetNaming included.
  None of those moves to Shared however general it looks. That is the user's decision.
- A change to `Core/Shared` runs alone. The user stops every other session first, the
  Shared change lands as its own commit and pull request, then the others resume. The hook
  refuses a commit that mixes a Shared change with any task's work.
- A task needing a change in another task's file asks the user for it. It does not make it.
- The one file every task edits is `src/RcrcGreen.Revit/RcrcGreenApplication.cs`, one line
  each, registering its own pane.
- A new task creates its own pair on its first round, `steps/log-<task>.md` and
  `steps/ai-max-state-<task>.md`, and adds its folder name to the TASKS list in
  `.claude/hooks/territory-check.sh`.

## The wall

A rule in this file is a request and can be reasoned around. `territory-check.sh` cannot,
and it is the only thing that stops two sessions that cannot see each other. It runs on
every commit and refuses one that touches two tasks' territory, naming both, and one that
touches `Core/Shared` alongside any task's files, naming the Shared file. It allows a
commit inside one task's territory plus common files such as `steps/` and `.claude/`, and
a commit that is Shared plus common files only.

Two limits, known and accepted. Drawing Sheet's flat test files read as common to the
hook, because they sit mixed with the Shared ones. And the fence around the Revit root is
a file list, so a new Drawing Sheet file at that root has to be added to it.

## What GitHub puts back on a squash merge

Commits `e0e206a` and `4feac50` on main carry the co-author credit line that
`writing-check.sh` exists to refuse. Neither pushed commit carried it. The squash merge
builds its message on GitHub's servers, where no hook runs, and the credit is appended
there. No hook in this repo can stop it.

Two remedies. Either the squash message is set by hand on every merge, through the commit
title and commit message the merge call accepts, or an admin changes the default squash
message under the repository's settings on github.com. From inside a session only the
first is possible, because no tool here reaches repository settings. Whether GitHub still
appends the credit to a message passed by hand was UNKNOWN when this file was written.
The first merge after this file measured it, and the answer is in `steps/log-drawing.md`.
