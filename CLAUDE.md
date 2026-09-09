# RCRC Green

A Revit 2024 add-in for the landscape production team. Two tools. Drawing Sheet reads the
model, shows a grid of which views exist per plot, and creates the missing ones. KPI will fill
the client's GRP KPI Checklist workbook from a model, and this round it only scans.

Read `.claude/skills/ai-max/SKILL.md` before doing any work in this repo. It sets the phase
order, the writing rules and the reporting rules that everything here follows. The current phase
is in `steps/ai-max-state.md`.

Done means: the tool lists every plot in a model, shows which known view types each is missing,
and creates them with correct names and, for a cross section, a cut across the middle of a plot.

## Running it

**Drawing Sheet** is a dockable panel that reads the model every time it is shown. It is five
numbered steps, one open at a time: PLOTS, VIEW TYPES, MARK, SHEETS, RUN. A shut step carries its
own summary and an unusable one says why. A filled square is a view that exists and opens on a
click, an empty one is missing and can be marked, one at a time, by row, by column or all at
once. Every dropdown comes from the model. Step 5 holds the scope box counts, and Scan Model,
in the top strip, reads the whole document not just the range.

**Run**, step 5, creates what the marked cells on the ticked plots ask for: plan views, sections,
schedules and sheets. One confirmation, one transaction, one undo, one report of what happened.

**KPI Checklist** opens a pane of the model name, KPI Scan, a status line and the template
picker. KPI Scan writes nine sections and creates nothing. **The elements a schedule lists are
not the scheduled things:** the softscape schedule returns RVT Link instances, so its printed
rows are its numbers' only route. Every measured fact is in `.claude/rules/kpi-rules.md`.

Build `RcrcGreen.sln` in Visual Studio 2026, then run `.\install\install.ps1`. It builds the
`Addins\2024\` layout the build does not, so copying the output folder by hand leaves Revit
unable to find the assembly.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

The whole suite. It is what the pull request gate runs and nothing in it needs Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation, and
  `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the panes, each tool
  under its own folder. `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only. `install/`
  holds the PowerShell scripts and `.claude/rules/` the rules per project
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

## Project facts

These come from the team and from real models. They are not guesses.

- Views and sheets are named `<PlotID>-(<code>) <View name>`. PlotID is two uppercase letters,
  a dash, then digits, as in DM-41, and lowercase is invalid. The code is digits in round
  brackets and the view name is free text after the bracket and one space
- Plots also exist as scope boxes named with the PlotID, for example a box named DM-41
- **There are two plot parameters, not one.** `PRX_Plot_ID` sits on views and on sheets and the
  Sheet List filters on it. `PRX_Ref Plot ID`, with spaces rather than underscores, sits on
  model elements and every quantity schedule filters on that one. A schedule built against the
  wrong one comes back empty
- On a view, PRX_Plot_ID is the first place to look for the plot. The name is the fallback
- Cross sections are cut across the middle of the plot's scope box, the SHORT way
- View names repeat word for word across plots, and nothing plot specific appears in one

Six of the things under a plot are schedules, under Schedules and Quantities rather than Views.
They filter on PRX_Ref Plot ID. Category alone does not identify one, because HARDSCAPE and
SHRUBS AND LAWN SCHEDULE are both Floors, told apart by their second filter. Field names are
copied exactly, PRX_Furniture Lenght included. A schedule is built on Revit's NUMBER for the
category, never the display name, because KERBS is built on Slab Edges and that name is not in
`Document.Settings.Categories`. A filter value goes back as its own kind, so a Yes/No parameter
goes back as 1. A calculated field cannot be added to a new schedule at all.

**A view family type is not named after the view type, and one view type is not built one way.**
DM-18-(200) General Arrangement Layout uses `(200) General Arrangement Layout`, which matches.
DM-11-(010) Location Key Plan uses `(010) Key Location Plan`, words swapped, which does not. So
nothing is matched on a name. A new view takes its family type, its level, its view template,
Crop View and Crop Region Visible from ONE view of the same type the model already holds, and
the report names that view. Three (010) views made in one run got three different family types
that way, each copied faithfully, so the scan counts them per view type and picks no winner.
**(400) Landscape Cross Section is a section rather than a plan view**, read off that view's
kind rather than off the code.

Two settings are the tool's own rather than the model's, because the model disagrees with
itself. Cross sections look 1 METRE, where four real ones read 0.93, 0.93, 1.53 and 12.83.
ANNOTATION CROP IS ALWAYS ON for a plan view, where copying it off PL-17 passed the fault on and
left neighbouring plots' section markers drawing through. The report says whose setting each is.
A section is left with no scope box while every plan view has one.

A new view is CREATED FRESH, never copied from another plot, and carries no annotation,
dimensions, tags or detailing. A SHEET IS DESCRIBED rather than copied: the title block type,
the views and 1, 2 or 4 per sheet are shared, and the views DIVIDE into as many sheets as they
need, in ticked order, none left off. A one-view sheet is named after its view, upper cased,
code removed, and numbered as the code, the plot's letter, then the first free letter, both
editable proposals the report marks generated or typed. A sheet's size is read off the title
block PLACED ON IT, because Sheet Width and Sheet Height only exist on the instance. A SHEET
HAS NO SCALE OF ITS OWN: its Scale reads out the placed views' templates. Nothing sets one.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 not on sheets, 79 view templates
- 406 scope boxes for 160 PRX_Plot_ID values, so a box not named for a plot is ordinary
- 2,114 names parsed and 4,039 did not, which made the parameter the first source not a fallback
- 6 views are named for one plot and carry PRX_Plot_ID for another. The scan names them
- Only plot DM-11 has real sheet numbers, 010QE to 600QD, all with plot letter Q. Other plots
  carry numbers like 010QE Copy 001, and a group of sheets has no PRX_Plot_ID. The scan counts
  both
- Title blocks are AR-PRX-Title_Block_A1, several types. Read them from the model, never fix them
- Three full runs: 10 with 0 refused, then 8 with 6 refused and 3 needing attention, then 13
  with 0 refused. Every fault each run surfaced was fixed and every path has now run

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

## Conventions

**Anything not written down is UNKNOWN.** Never invent a rule. Open questions go in the log.

## Hooks

Three, wired in `.claude/settings.json`. Both commit hooks read the command rather than the
index, through `commit-scope.py`, and a hook script that cannot be found blocks.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit not carrying `steps/ai-max-state.md`
- `writing-check.sh` refuses a commit whose message or files hold an em dash, a generated-by
  footer, a co-author credit line, an emoji, or a word from the list in
  `.claude/skills/ai-max/references/writing-rules.md`, which it skips. landscape is kept

## Things that have gone wrong before

Add to this whenever something breaks. It is the only part that cannot be rediscovered by
reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` split
its file list on newlines, so a name holding a space reached the scanner in pieces that each
read as a file that is not there. It passed unchecked. A check that cannot see its own subject
has to refuse. The KPI scan names every read that did not happen at the top of its file.

**An assumption held for five rounds because nobody ran the thing.** The naming pattern came
from four examples, the read was assumed to need a progress window, and PRX_Plot_ID was assumed
to be on elements only. One run corrected all three. Prefer measured numbers to reasoning.

**Some faults only show when somebody uses the thing.** The panel shipped with every heading
written and none visible, black on black. Later it was correct and unusable: ticking a view type
scrolled the list away, marking 136 cells took 136 clicks, and eight columns ran off the right
edge. Neither round shows in a test or in a mockup.

**Two sources for one fact is two facts.** Six times now, always the same shape. A view named
for DM-12 filled a DM-11 cell that stayed full after every DM-11 view was deleted. The column
count against its list. The run report, which made nothing and named four views under PLAN VIEWS
and under NOT CREATED. A panel step reading the range off its arguments. One method filling both
the code buttons and the Add row's dropdown, split and half kept. A schedule category read one
way on capture and looked up another way on create.

**A skip with nothing written down is a lie by omission.** `ModelWriter` dropped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter reads as correct on a drawing. Every skip is
recorded now, a lost filter refuses the schedule, and that delete is checked.

**One example is not a rule, and the model may hold no rule at all.** The family type was
matched on the view type because one Properties panel showed a type named exactly that. The next
plot's is `(010) Key Location Plan`. Copying a sibling replaced it, and then three (010) views
copied three different family types. Annotation crop went the same way one round later.

**A name is not an identity and a word is not a value.** Sheet Width was read off a title block
TYPE, where that instance parameter does not exist, so null became 0.0 and three sheets came out
empty. Slab Edges was looked up by display name in a collection that does not hold it, on a model
that has the category. A Yes/No filter was handed back the word Yes when Revit stores 1. Three
rounds, one shape. Ask the API what a thing IS rather than what it is called.

**Copying takes whatever state the thing is in, including nothing.** The first sheet the tool
made was empty, copied from one the user picked that had no views on it. Nothing failed and
nothing was reported. A sheet is described now, so what goes on it is stated.

**A setting nobody recorded cannot be argued about.** A created view came out with a template
that looked right and a family type that looked wrong. The code read both off one view four
lines apart, but nothing said which view. Record where a value came from as you use it.

## Writing and working agreements

No em dash, no semicolon in prose, no emoji anywhere, commit messages included. No generated-by
footer and no co-author line. Comments say why, not what. Full list in the ai-max writing rules.
Never report a test result from a run made before the last file was written. Say UNKNOWN rather
than filling a gap. Write what happened in `steps/log.md`, newest entry at the top.
