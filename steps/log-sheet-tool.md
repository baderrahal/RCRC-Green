# Sheet Tool log

Newest entry first.

---

## 2026-09-13, first pass. Phase 1, the questions

Branch `claude/rcrc-green-setup-wf9ham`, one pull request. No code. This round decides what
the tool does and what counts as done, and it cannot finish until the user answers, so what
it produces is `steps/sheet-tool-phase1.md` and the eleven questions in it.

**Question one is where the line falls between Drawing Sheet and this panel**, and I have not
answered it. Drawing Sheet already lists plots, shows which view types each is missing,
creates the missing views, and describes and creates sheets with numbers built from the
team's scheme. A second panel that overlaps that is worse than no second panel, so the line
is the user's to draw. The brief said not to answer it and it would have been wrong to.

Three candidate definitions of done are offered rather than one, and they differ in KIND
rather than in wording: a read-only check, a writer that corrects, a maker that creates.
Which is right follows from question one and question three, and the three are not equally
cheap. A read-only tool has no transaction, no undo and no way to damage a model.

**Two things the brief said that the repo does not bear out, both checked rather than
assumed.**

`territory.md` lists four tasks and never mentions View Filters, which is live: it has
`src/RcrcGreen.Core/ViewFilters`, `src/RcrcGreen.Revit/ViewFilters`,
`.claude/rules/view-filters-rules.md`, its own log and state file, and it is in the hook's
TASKS list. The file is behind the repo. This round adds Sheet Tool to it and leaves View
Filters' entry for that session to write, because it is their line.

`ReportPlaces` is named in the brief beside `ReportFile.cs` as common ground. `ReportFile.cs`
is at the Revit root and is genuinely common. `ReportPlaces.cs` is not: it sits in
`src/RcrcGreen.Core/DrawingSheet/`, which is Drawing Sheet's territory, and View Filters
already calls it from its request handler at line 191. That is a compile-time call across the
fence, which `territory.md` names as the thing not to do, and copying it is worse because a
copy is two records of one fact. **If Sheet Tool writes a report, that is a Shared round the
user has to schedule.** It is written down in part 4 rather than worked around.

What is recorded about what Drawing Sheet already solved comes from `CLAUDE.md` and
`.claude/rules/` only. The logs were not read, on the brief's instruction, and they are long
and belong to other tasks.

`SheetTool` is added to the TASKS list in `.claude/hooks/territory-check.sh`, which
`territory.md` says a new task does on its first round. Without it the hook cannot see this
task's folders as territory at all.

Suite reads 1569 on main at `8bacf5f` and is unchanged by this round, which adds no code.

Nothing has been run in Revit and there is nothing yet to run.
