# Sheet Tool, phase 1

Phase 1 decides what the tool does and what counts as done. Nothing else. No code was
written this round and none should be until parts 1 to 3 below are answered.

Written against main at `8bacf5f`, where the suite reads 1569.

---

## 1. What it does, in the user's words

**UNKNOWN. These are the questions, and none of them is answered here.**

Answer them in one pass. Short answers are fine, and "I do not know yet" is a real answer
that saves a round.

1. **Where does the line fall between Drawing Sheet and Sheet Tool?** Drawing Sheet already
   lists the plots, shows which view types each is missing, creates the missing views, and
   describes and creates sheets with built numbers. What does Sheet Tool do that it does
   not? If the honest answer is that some of Drawing Sheet's work should move here, say so,
   because that is a different job from building something new.

2. **What are you doing by hand today that this replaces?** Walk one real instance through,
   in order, as you actually do it. The steps you would skip if you were in a hurry are the
   ones worth naming.

3. **Does it change the model, or does it only read and report?** A read-only tool and a
   writing tool are different builds from the first file, so this one decides the shape.

4. **What does it work on?** Sheets that already exist in the model, sheets that do not exist
   yet, the views on sheets, the title blocks, or something else.

5. **Which parameters does it touch?** Named exactly. `PRX_Plot_ID` and `PRX_Ref Plot ID` are
   two different parameters in this project and a tool aimed at the wrong one comes back
   empty.

6. **What is its scope on one press?** One sheet, one plot, the plots you tick, or the whole
   model.

7. **Where does the answer come out?** On the pane, a text report in the repo's reports
   folder, a spreadsheet, the model itself, or more than one of those.

8. **Who runs it, and how often?** You alone or the team, and once a project or every week.

9. **What must it never do?** The one thing that would make you stop using it. Drawing Sheet
   has several of these and each cost a round to learn.

10. **Is there a real model and a real sheet set I can be pointed at?** Measured numbers beat
    reasoning here, and this repo has paid for that lesson more than once.

11. **Which of the three candidates in part 2 is closest**, or what is the right one if none
    of them is.

---

## 2. What counts as done

One sentence, observable with eyes rather than felt. Pick one, or write the one that is
actually right.

These three differ in KIND rather than in wording, because question 1 and question 3 decide
the kind and I do not have those answers. Each has a blank in it that only you can fill.

**Candidate A, a read-only check.** Sheet Tool lists every sheet in the open model, says for
each whether `<the thing being checked>` is right, and writes a report naming every sheet
that is wrong and why, changing nothing in the model.

**Candidate B, a writer that corrects.** Sheet Tool sets `<the parameter>` on every sheet
that is wrong, in one transaction and one undo, and writes a report naming every sheet it
changed, every sheet it refused and why.

**Candidate C, a maker.** Sheet Tool creates `<the thing>` for every ticked plot, named and
numbered by the team's scheme, and writes a report naming what it made and what it refused.

A is the cheapest to build and the cheapest to trust. C is the most expensive and overlaps
Drawing Sheet the most, which is why question 1 comes first.

---

## 3. What it reads and what it writes

**UNKNOWN until question 3 is answered.** What is known is what the answer costs, and the two
shapes are not the same build.

**If it only reads**, the tool needs one read through the external event, a Core type holding
what was read as plain strings and numbers, and a report. There is no transaction, no undo
and no way to damage a model. Every rule in part 5 still applies except the one about a
report claiming a thing was both created and not created, which needs something to create.

**If it writes**, it needs all of the above plus one transaction, one undo, a confirmation
before it runs, and a refusal path for everything it will not do. Drawing Sheet's run is
that shape and it took several rounds to get the refusals honest.

What it reads and which parameters it touches are questions 4 and 5. Nothing here should be
guessed, because a tool aimed at the wrong parameter comes back empty and looks like a bug
in the code rather than a bug in the plan.

---

## 4. What it borrows from Shared

Read off `src/RcrcGreen.Core/Shared/` at `8bacf5f`. The folder listing is the full record, so
this is what exists rather than what is likely.

| Shared type | What Sheet Tool would use it for |
| --- | --- |
| `PlotId` | reading and validating `DM-41`, and its prefix, without a second copy of the rule |
| `PlotRegistry`, `PlotRecord`, `PlotSource`, `PlotRegistryResult` | the union of every place a plot shows up, if the tool works per plot |
| `PlotRange`, `PlotSelection` | a plot picker, if the tool works over ticked plots |
| `ViewNameParser`, `ParsedViewName`, `ViewType` | reading `<PlotID>-(<code>) <View name>` apart, if the tool touches views |
| `NaturalOrder` | ordering anything numbered, so `DM-2` comes before `DM-30` |
| `Lengths` | feet to millimetres, if the tool measures anything |
| `ScanFileName` | naming a report file, with the cleaning already in it |
| `PaneLabel` | escaping text on a button, so a parameter name is not mangled by WPF |
| `IgnoredName` | naming something skipped and why, if the tool skips anything |
| `Point3D`, `Vector3D` | geometry, if the tool touches any |

**What Shared does not have, and the one gap already known.**

`ReportPlaces` decides where a report is written and what the pane says about it afterwards.
It is used by every pane that writes a report. **It is not in Shared.** It sits in
`src/RcrcGreen.Core/DrawingSheet/ReportPlaces.cs`, which is Drawing Sheet's territory, and
View Filters already calls it at `src/RcrcGreen.Revit/ViewFilters/ViewFiltersRequestHandler.cs:191`.

That is a compile-time call across the fence between two sessions that cannot see each
other, which `territory.md` names as the thing not to do, and the alternative it names is
worse: a copy is two records of one fact.

**So if Sheet Tool writes a report, this is a Shared round the user has to schedule**, moving
`ReportPlaces` to `Core/Shared` with its tests while every other session is stopped. It is
not a change this task may make. If the answer to question 7 is that nothing is written to a
file, the gap does not arise.

`ReportFile.cs` at the Revit root is genuinely common and needs no round. Only the Core half
is in the wrong folder.

Nothing else is known to be missing. That is not the same as nothing else being missing, and
parts 1 to 3 are what would show it.

---

## 5. What Drawing Sheet already solved that Sheet Tool will hit

From `CLAUDE.md` and `.claude/rules/` only. Each of these cost that session rounds, and each
applies here.

**Everything through one ExternalEvent and one handler. Applies.** A panel is modeless and
its code runs whenever Windows raises an event, which is almost never a moment Revit will
accept an API call. This is not optional and it is not a style choice. Any pane Sheet Tool
adds needs its own request handler and its own external event from the first commit, because
retrofitting it means rewriting every call.

**Never two records of one fact. Applies, and it is the one to watch.** CLAUDE.md counts this
fourteen times in this repo, always the same shape: a count against its list, a report naming
the same view under two headings, one rule read one way on capture and another on create.
The rule that follows from it is that a thing is worked out in ONE place and read from there,
never worked out twice. It is cheap to obey at the start and expensive to fix later.

**Every colour from `PanelTheme`. Applies.** The Drawing Sheet panel shipped once with every
heading written and none visible, black on black. No brush gets written anywhere else.

**Reports to the repo's reports folder, never the Desktop. Applies if anything is written.**
That folder is in `.gitignore` and stays there, because this repository is public and a
report carries client view names, sheet numbers and plot identifiers. See the gap in part 4:
the type that decides this is not in Shared yet.

**A report must never say a thing was created and not created. Applies if anything is
created.** Drawing Sheet shipped a run report naming four views under PLAN VIEWS and again
under NOT CREATED. The fix is that both sides are named from one record, which is the
two-records rule again wearing a different hat.

**Never report a check that did not run. Applies now, this round, before any code exists.**
It covers test counts, whether a file exists, and whether something was observed in Revit.
UNKNOWN is a real answer and a confident wrong number costs more than an honest gap.

**Two more from CLAUDE.md worth carrying in, because they are cheap now.** A pane holds no
copy of anything it can ask for, since a copy taken once and never refreshed reads exactly
like a fact. And what goes on a button is escaped through `PaneLabel`, because WPF eats the
first underscore and this repo is full of parameter names that carry one.

**Nothing is copied from Drawing Sheet's code.** Anything needed from it is either borrowed
from Shared or asked for as a Shared round.

---

## 6. The first thing the user sees

One ribbon panel, one button, one dockable pane, on the existing RCRC Green tab, alongside
Drawing Sheet, KPI and View Filters.

| | |
| --- | --- |
| ribbon panel | `Sheet Tool` |
| button | `Sheet Tool` |
| dockable pane | `Sheet Tool` |

Named for the task until question 1 gives it a better name. The three live panels are each
named for what they do rather than for what they are, so this one probably should be too,
and that name follows from the answers rather than preceding them.

The registration is one line in `RcrcGreenApplication.cs`, which `territory.md` names as the
one file every task edits.

---

## What happens next

Parts 1 to 3 are answered by the user. Nothing else moves until they are. Phase 2 picks the
language, which on this repo is already settled by the three panels that exist, so the real
next round is phase 7, building, against a definition of done that part 2 will hold.
