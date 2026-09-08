# RCRC Green

A Revit 2024 add-in for the landscape production team. The first tool in it is Drawing
Sheet, which reads the model, shows a grid of which views exist per plot, and will create the
missing ones.

Read `.claude/skills/ai-max/SKILL.md` before doing any work in this repo. It sets the phase
order, the writing rules and the reporting rules that everything here follows. The current phase
is in `steps/ai-max-state.md`.

Done means: for a model of plots, the tool lists every plot, shows which of the known view
types each one is missing, and creates those views with correct names and, for cross sections,
a cut through the middle of the plot.

## Running it

One ribbon tab, one panel, one button. Everything happens inside the Drawing Sheet panel.

**Drawing Sheet** is a dockable panel that stays open while the user works and reads the model
every time it is shown. Pick a prefix, then a first and a last plot. Every plot in range gets a
row with a tick box, all ticked, and unticking one drops it out of anything that writes. Every
view type is a column, none ticked to start, with a search box, All and None, a button per code,
and a row for adding a type the model lacks. A filled square is a view that exists and opens on
a click, an empty one is missing and can be marked. Every dropdown comes from the model, so no
plot the model lacks can be chosen. It follows the Revit theme.

**Run**, inside the panel, creates what the marked cells on the ticked plots ask for: plan
views, sections, schedules and sheets. One confirmation, one transaction, one undo, a report
either way, and every count in it is of what happened rather than of what was intended.

**Scope boxes**, inside the panel. The six case counts are on screen before anything is
pressed and every case but B opens on a click to list its views, each of which opens in Revit.
Only a view with no box whose plot has one of exactly that name is written.

**Scan Model**, also inside the panel, reads the whole document to a text file and changes
nothing. It reads everything rather than a range, which is what makes it the check on the panel.

Build `RcrcGreen.sln` in Visual Studio 2026, then run `.\install\install.ps1`. It builds the
layout the manifest asks for under `%APPDATA%\Autodesk\Revit\Addins\2024\`, which the build
does not, so copying the output folder across by hand leaves Revit unable to find the assembly.
`.\install\uninstall.ps1` takes it back out.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

That is the whole suite, it is what the pull request gate runs, and nothing in it needs Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation
- `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the panel
- `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only, `install/` holds the two
  PowerShell scripts, and `.claude/rules/` holds the rules for each project
- `reports/` holds what a run wrote, the Desktop copy under the same name, and **nothing in it
  is ever committed.** This repository is public and a report carries client view names, sheet
  numbers, plot identifiers and schedule fields. `reports/README.md` is the only tracked file

A command is split the same way. The Revit project reads the document into plain strings and
numbers, and Core decides and formats. That is why every report layout, every count and every
rule has a test and none needs Revit.

## The rule that keeps the tests possible

**RcrcGreen.Core must never reference the Revit API.** No `Autodesk.Revit` using, no package
reference, no type from it in a signature. The moment Core touches the API, the test project
cannot load and the gate stops protecting anything.

Anything that reads a `Document`, a `View`, an `Element` or a `BoundingBoxXYZ` belongs in
`RcrcGreen.Revit`. Pull the plain values out there, hand them to Core, and put what Core
returns back into the model.

## Project facts

These come from the team and from real models. They are not guesses.

- Views and sheets are named `<PlotID>-(<code>) <View name>`
- PlotID is two uppercase letters, a dash, then digits, as in DM-41. Lowercase is invalid
- The code is digits inside round brackets, as in 010, 200, 400, and the view name is free text
  after the closing bracket and one space
- Plots also exist as scope boxes named with the PlotID, for example a box named DM-41
- **There are two plot parameters, not one.** `PRX_Plot_ID` sits on views and on sheets and the
  Sheet List filters on it. `PRX_Ref Plot ID`, with spaces rather than underscores, sits on
  model elements and every quantity schedule filters on that one. A schedule built against the
  wrong one comes back empty
- On a view, PRX_Plot_ID is the first place to look for the plot. The name is the fallback
- Cross sections are cut across the middle of the plot's scope box, the SHORT way, and look
  10 metres, a starting value the team will change after a real placement
- View names repeat word for word across plots, and nothing plot specific appears in one

Six of the things under a plot are schedules, under Schedules and Quantities rather than Views.
They are built by a different call and filter on PRX_Ref Plot ID. Category alone does not
identify one, because HARDSCAPE and SHRUBS AND LAWN SCHEDULE are both Floors, told apart only by
their second filter. Field names are copied exactly, PRX_Furniture Lenght included.

**A view family type is not named after the view type.** DM-18-(200) General Arrangement Layout
uses `(200) General Arrangement Layout`, which matches. DM-11-(010) Location Key Plan uses
`(010) Key Location Plan`, words swapped, which does not. So nothing is matched on a name. A new
view takes its family type, its level and its view template from a view of the same type the
model already holds on another plot, and **(400) Landscape Cross Section is a section rather
than a plan view**, which is read off that view's kind rather than off the code.

Creation, answered by the team. A new view is CREATED FRESH, never copied from another plot,
and carries no annotation, dimensions, tags or detailing. A sheet is COPIED: the user sets one
plot's sheet up by hand and the title block, where each view sits and how several lay out are
taken from it. The user types the sheet number and the sheet name, the tool invents neither,
and a plot missing either gets no sheet.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 not on sheets, 79 view templates
- 406 scope boxes for 160 distinct PRX_Plot_ID values, so a box whose name is not a PlotID is
  ordinary rather than an error
- 2,114 names parsed and 4,039 did not. Scope Box skipped 1,269 for that alone, which made the
  parameter the first source rather than the only fallback
- Scope box cases over one range came back A 0, B 102, C 66, D 0, E 13, F 1
- 6 views are named for one plot and carry PRX_Plot_ID for another. The scan names them
- 8 templates start with (200) General Arrangement Layout, so a prefix match answers nothing
- The first write ran on four plan views and created none. Every fault it found is fixed and
  none of the fixes has been run

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

## Conventions

**Anything not written down is UNKNOWN.** Do not invent a rule about codes, naming, plots or
geometry. Open questions are recorded in `steps/log.md` and answered by the team. The rest of
the rules live next to the code they govern, in `.claude/rules/core-rules.md` and
`.claude/rules/revit-commands.md`.

## Hooks

Three of them, wired in `.claude/settings.json`. They are walls, not requests, and both commit
hooks read the command rather than the index, through `commit-scope.py`.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit not carrying `steps/ai-max-state.md`
- `writing-check.sh` refuses a commit whose message or files hold an em dash, a generated-by
  footer, a co-author credit line, an emoji, or a word from
  `.claude/skills/ai-max/references/writing-rules.md`. It skips `.claude/skills/`, where that
  list lives as data, and keeps the word landscape, the discipline here

A hook script that cannot be found blocks.

## Things that have gone wrong before

Add to this whenever something breaks. Over time it is the most valuable part of this file,
because it is the only part that cannot be rediscovered by reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` split
its file list on newlines, so a name holding a space reached the scanner in pieces that each
read as a file that does not exist. It went through unchecked and reported success. Fixed in
6f0cf1d, and the same shape came back twice in phase 8. A check that cannot see its subject has
to refuse, which is the side `commit-scope.py` erred on reading the 2 of `2>&1` as a file.

**An assumption held for five rounds because nobody ran the thing.** The naming pattern came
from four examples, the read was assumed to need a progress window, and PRX_Plot_ID was assumed
to be on elements only. One run corrected all three. Prefer measured numbers to reasoning.

**Code that is there is not code you can see.** The panel shipped with every heading and label
written and none visible. A dockable pane on the dark theme is black, WPF defaults text to
black, and nothing set a foreground.

**Two sources for one fact is two facts.** The grid took a view's plot from PRX_Plot_ID and its
type from the name without checking they agreed, so a view named for DM-12 carrying PRX_Plot_ID
DM-11 filled a DM-11 cell that stayed full after every DM-11 view was deleted. Fixed in
`ViewReading`, then again in `GridColumns`. The third was the run report, which made nothing
and named the same four views under PLAN VIEWS and under NOT CREATED, because the created
sections printed what the run set out to do. `RunOutcome` is what it did and the report reads
that alone.

**A skip with nothing written down is a lie by omission.** `ModelWriter` dropped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter shows every plot and reads as correct on a
drawing. Every skip is recorded now and a lost filter refuses the schedule.

**Never report an action that might not have happened.** That refusal deletes the schedule
again, unguarded, so a Revit refusal would leave a wrong schedule in the model while the report
said it was gone. The delete is checked now and the report names what has to go by hand.

**One example is not a rule.** The view family type was matched on the view type because one
Properties panel showed a type named exactly that. The next plot's is `(010) Key Location Plan`
against a view called Location Key Plan, which cost three of the four refusals in the first
write and was invisible in a scan report that listed only templates.

## Writing

No em dash, no semicolon in prose, no emoji anywhere, including commit messages. No
generated-by footer and no co-author credit line. Comments say why, not what. Full list in
`.claude/skills/ai-max/references/writing-rules.md`.

## Working agreements

Never report a test result that did not come from a run made after the last file was written.
When something cannot be checked, say UNKNOWN rather than filling the gap. Write what happened
in `steps/log.md`, newest entry at the top.
