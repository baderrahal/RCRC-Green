# RCRC Green

A Revit 2024 add-in for the landscape production team. The first tool in it is Drawing
Sheet, which reads the model, shows a grid of which views exist per plot, and will create the
missing ones.

Read `.claude/skills/ai-max/SKILL.md` before doing any work in this repo. It sets the phase
order, the writing rules and the reporting rules that everything here follows. The current phase
is in `steps/ai-max-state.md`.

Done means: the tool lists every plot in a model, shows which known view types each is missing,
and creates them with correct names and, for a cross section, a cut across the middle of a plot.

## Running it

**Drawing Sheet** is a dockable panel that stays open while the user works and reads the model
every time it is shown. It is five numbered steps, one open at a time: PLOTS, VIEW TYPES, MARK,
SHEETS, RUN. A shut step carries its own summary and an unusable one says why. A filled square is
a view that exists and opens on a click, an empty one is missing and can be marked, and every
dropdown comes from the model. Step 5 holds the six scope box counts, and Scan Model, in the top
strip, reads the whole document to a text file rather than the range, the check on the panel.

**Run**, step 5, creates what the marked cells on the ticked plots ask for: plan views, sections,
schedules and sheets. One confirmation, one transaction, one undo, and a report whose every count
is of what happened rather than what was intended.

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
  `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the panel
- `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only. `install/` holds the two
  PowerShell scripts and `.claude/rules/` the rules for each project
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
copied exactly, PRX_Furniture Lenght included.

**A view family type is not named after the view type, and one view type is not built one way.**
DM-18-(200) General Arrangement Layout uses `(200) General Arrangement Layout`, which matches.
DM-11-(010) Location Key Plan uses `(010) Key Location Plan`, words swapped, which does not. So
nothing is matched on a name. A new view takes its family type, its level, its view template and
its three crop settings from ONE view of the same type the model already holds, and the report
names that view. Three (010) views made in one run got three different family types that way,
each copied faithfully, so the scan counts them per view type and nothing picks a winner.
**(400) Landscape Cross Section is a section rather than a plan view**, read off that view's
kind rather than off the code.

Cross sections look 1 METRE, the team's decision rather than a number read off a view. Four real
ones read 0.93, 0.93, 1.53 and 12.83 metres, so there was nothing to copy. A section is left
with no scope box while every plan view has one.

A new view is CREATED FRESH, never copied from another plot, and carries no annotation,
dimensions, tags or detailing. A SHEET IS DESCRIBED rather than copied. The title block type,
the sheet name, the view types and whether 1, 2 or 4 views go on it are shared across the ticked
plots, and the sheet number is the only thing per plot. The user types every number and every
name and the tool invents neither. A sheet's size is read off the title block PLACED ON IT,
because Sheet Width and Sheet Height are instance parameters and do not exist on the type.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 not on sheets, 79 view templates
- 406 scope boxes for 160 PRX_Plot_ID values, so a box not named for a plot is ordinary
- 2,114 names parsed and 4,039 did not, which made the parameter the first source not a fallback
- Scope box cases over one range came back A 0, B 102, C 66, D 0, E 13, F 1
- 6 views are named for one plot and carry PRX_Plot_ID for another. The scan names them
- 8 templates start with (200) General Arrangement Layout, so a prefix match answers nothing
- Only plot DM-11 has real sheet numbers. Other plots carry numbers like 010QE Copy 001, and a
  group of sheets carries no PRX_Plot_ID at all. The scan counts both
- Title blocks are AR-PRX-Title_Block_A1, several types. Read them from the model, never fix them
- A full run made 10 things, 0 refused and 4 needing attention, so every path in the tool has
  now run at least once. Three sheets came out empty and every new view had Annotation Crop
  off, both fixed since

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

## Conventions

**Anything not written down is UNKNOWN.** Do not invent a rule about codes, naming, plots or
geometry. Open questions go in `steps/log.md` and the team answers them.

## Hooks

Three of them, wired in `.claude/settings.json`. They are walls, and both commit hooks read the
command rather than the index, through `commit-scope.py`.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit not carrying `steps/ai-max-state.md`
- `writing-check.sh` refuses a commit whose message or files hold an em dash, a generated-by
  footer, a co-author credit line, an emoji, or a word from the list in
  `.claude/skills/ai-max/references/writing-rules.md`, which it skips. landscape is kept

A hook script that cannot be found blocks.

## Things that have gone wrong before

Add to this whenever something breaks. Over time it is the most valuable part of this file,
because it is the only part that cannot be rediscovered by reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` split
its file list on newlines, so a name holding a space reached the scanner in pieces that each
read as a file that is not there. It passed unchecked. A check that cannot see its own subject
has to refuse.

**An assumption held for five rounds because nobody ran the thing.** The naming pattern came
from four examples, the read was assumed to need a progress window, and PRX_Plot_ID was assumed
to be on elements only. One run corrected all three. Prefer measured numbers to reasoning.

**Code that is there is not code you can see.** The panel shipped with every heading and label
written and none visible. A dockable pane on the dark theme is black, WPF defaults text to
black, and nothing set a foreground.

**Two sources for one fact is two facts.** Five times now, always the same shape. The grid took
a view's plot from PRX_Plot_ID and its type from the name without checking they agreed, so a
view named for DM-12 filled a cell that stayed full after every DM-11 view was deleted. Then the
column count against its list. Then the run report, which made nothing and named four views
under PLAN VIEWS and under NOT CREATED. Then a panel step reading the range off its arguments
rather than off whether step 1 was usable. Then one method filling both the code buttons and the
Add row's dropdown, split when the panel was rebuilt, half of it kept.

**A skip with nothing written down is a lie by omission.** `ModelWriter` dropped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter reads as correct on a drawing. Every skip is
recorded now, a lost filter refuses the schedule, and the delete that refusal does is checked,
because a report saying a thing was removed while the model still holds it is worse again.

**One example is not a rule, and the model may hold no rule at all.** The family type was
matched on the view type because one Properties panel showed a type named exactly that. The next
plot's is `(010) Key Location Plan`. Matching was replaced by copying a sibling, and then three
(010) views copied three different family types from three different siblings.

**A parameter that does not exist reads as a value of zero.** Sheet Width and Sheet Height were
read off the title block TYPE. They are instance parameters, so `get_Parameter` returned null,
null became 0.0, the guard fired, and three sheets were made empty on a real A1 block. Ask what
the API attaches a parameter to before deciding the model is wrong.

**Copying takes whatever state the thing is in, including nothing.** The first sheet the tool
made was empty. It was built by copying one the user picked, and that one had no views on it.
Nothing failed and nothing was reported. A sheet is described now, so what goes on it is stated.

**A setting nobody recorded cannot be argued about.** A created view came out with a template
that looked right and a family type that looked wrong. The code read both off one view four
lines apart, but nothing said which view. Record where a value came from as you use it.

## Writing

No em dash, no semicolon in prose, no emoji anywhere, including commit messages. No
generated-by footer and no co-author credit line. Comments say why, not what. Full list in
`.claude/skills/ai-max/references/writing-rules.md`.

## Working agreements

Never report a test result from a run made before the last file was written. When something
cannot be checked, say UNKNOWN rather than filling the gap. Write what happened in
`steps/log.md`, newest entry at the top.
