# RCRC Green

A Revit 2024 add-in for the landscape production team. The first tool in it is Drawing
Sheet, which reads the model, shows a grid of which views exist per plot, and will create the
missing ones.

Read `.claude/skills/ai-max/SKILL.md` before doing any work in this repo. It sets the phase
order, the writing rules and the reporting rules that everything here follows. The current
phase is written in `steps/ai-max-state.md`.

Done means: for a model of plots, the tool lists every plot, shows which of the known view
types each one is missing, and creates those views with correct names and, for cross
sections, a cut through the middle of the plot.

## Running it

One ribbon tab, one panel, one button. Everything happens inside the Drawing Sheet panel.

**Drawing Sheet** is a dockable panel that stays open while the user works and reads the model
every time it is shown. Pick a prefix, then a first and a last plot. Every plot in range gets
a row with a tick box, all ticked, and unticking one drops it out of the counts and out of
anything that writes. Every view type the model holds is a column, none ticked to start, with
a search box, All and None, a button per code, and a row for adding a type the model lacks. A
filled square is a view that exists and opens on a click, an empty one is missing and can be
marked. Every dropdown is filled from the model, so no plot the model lacks can be chosen. It
follows the Revit theme.

**Run**, inside the panel, creates what the marked cells on the ticked plots ask for. It says
what it will make first, then one confirmation, one transaction, one undo and a report either
way. No sheet is created yet.

**Scope boxes**, inside the panel. The six case counts for the ticked plots are on screen
before anything is pressed. Only a view with no scope box, whose plot has a box of exactly
that name, is written. One that already carries a box is left alone, right or wrong.

**Scan Model**, also inside the panel, reads the whole document and writes what it found to a
text file, changing nothing. It reads everything rather than a range, which is what makes it
the check the panel is measured against.

Build `RcrcGreen.sln` in Visual Studio 2026, then run `.\install\install.ps1`. It builds the
layout the manifest asks for under `%APPDATA%\Autodesk\Revit\Addins\2024\` and names every
file it copied. The build does not produce that layout, so copying the output folder across
by hand leaves Revit unable to find the assembly. `.\install\uninstall.ps1` takes it back
out, and `.claude/rules/revit-commands.md` holds the rest.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

That is the whole test suite and it is what the pull request gate runs. Nothing in it needs
Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation
- `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the panel
- `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only, and `install/` holds the two
  PowerShell scripts
- `.claude/rules/` holds the rules for each project, loaded when that project is touched

A command is split the same way. The Revit project reads the document into plain strings and
numbers, and Core decides and formats. That is why every report layout, every count and every
rule has a test and none of them needs Revit.

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
- PlotID is always two uppercase letters, a dash, then digits. Examples DM-41, PF-12.
  Lowercase is not a different plot, it is invalid
- The code is digits inside round brackets. Examples 010, 200, 400
- The view name is free text after the closing bracket and one space
- Plots also exist as scope boxes named with the PlotID, for example a box named DM-41
- **There are two plot parameters, not one.** `PRX_Plot_ID` sits on views and on sheets, and
  the Sheet List filters on it. `PRX_Ref Plot ID` sits on model elements, and every quantity
  schedule filters on that one. The second name carries spaces, not underscores. A schedule
  built against the wrong one comes back empty
- On a view, PRX_Plot_ID is the first place to look for the plot. The name is the fallback
- Missing cross sections are placed across the middle of the plot's scope box
- The default cut is the SHORT way across the plot
- A cross section looks 10 metres, a starting value the team will change once sections have
  been placed in a real model
- View names repeat word for word across plots. The same view type on two plots carries the
  same text after the bracket, and nothing plot specific appears in a view name

Six of the things under a plot are schedules, under Schedules and Quantities rather than
Views. They are built by a different call and filter on PRX_Ref Plot ID. Category alone does
not identify one, because HARDSCAPE and SHRUBS AND LAWN SCHEDULE are both Floors and are told
apart only by their second filter. Field names are copied exactly, PRX_Furniture Lenght
included, spelled that way in the model.

How a plan view is set up, read off DM-18-(200) General Arrangement Layout. Its view family
type is named `(200) General Arrangement Layout`, its template is
`(200) General Arrangement Layout SC - Scale 250`, its level is Level 1, its phase is
Proposed and it carries no scope box. `.claude/rules/revit-commands.md` holds what each of
those means for creating one, and none of it is guessed.

Creation, answered by the team:

- A new view is CREATED FRESH, never copied or duplicated from another plot. It shows the
  model and carries no annotation, no dimensions, no tags and no detailing
- The user chooses one view per sheet or several views per sheet
- The user fills in the sheet number and the sheet name. The tool invents neither

Still UNKNOWN about sheets, and why none is created: which title block a new sheet takes,
where a view sits on it, and how several lay out together.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 views not on sheets, 79 view templates
- 406 scope boxes for 160 distinct PRX_Plot_ID values, so a box whose name is not a PlotID is
  ordinary rather than an error
- 2,114 names parsed and 4,039 did not. Scope Box skipped 1,269 views for that alone, which is
  what made the parameter the first source rather than the only fallback

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

## Conventions

**Anything not written down is UNKNOWN.** Do not invent a rule about codes, naming, plots or
geometry. Open questions are recorded in `steps/log.md` and answered by the team.

The rest of them live next to the code they govern. `.claude/rules/core-rules.md` holds what
Core decides, including why a view fills the cell named by its own name and nothing else.
`.claude/rules/revit-commands.md` holds how a command and the panel are written.

## Hooks

Three of them, wired in `.claude/settings.json`. They are walls, not requests.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit not carrying `steps/ai-max-state.md`
- `writing-check.sh` refuses a commit whose message or files hold an em dash, a generated-by
  footer, a co-author credit line, an emoji, or a word from
  `.claude/skills/ai-max/references/writing-rules.md`. It skips `.claude/skills/`, where that
  word list lives as data, and keeps the word landscape, the discipline here

Both commit hooks work out what a commit really carries through `commit-scope.py`, which reads
the command rather than the index. A hook script that cannot be found blocks.

## Things that have gone wrong before

Add to this whenever something breaks. Over time it is the most valuable part of this file,
because it is the only part that cannot be rediscovered by reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` took
its file list line by line, so a file name holding a space reached the scanner in pieces and
every piece read as a file that does not exist. It went through unchecked and the hook
reported success. Fixed in 6f0cf1d, and the same shape came back twice in the phase 8
findings. When a check cannot see its subject, it has to refuse. The opposite also turned up.
`commit-scope.py` read the 2 of `2>&1` as a file to commit and refused a commit that was fine.
A guard that refuses good input announces itself, which is why it is the side to fail on.

**An assumption held for five rounds because nobody ran the thing.** The naming pattern came
from four examples, the read was assumed slow enough to need a progress window, and
PRX_Plot_ID was assumed to be on elements only. One run on a real model corrected all three,
and a later screenshot corrected the view family type and the level a new view is made on.
Numbers above came from those. Prefer them to anything reasoned out.

**Code that is there is not code you can see.** The Drawing Sheet panel shipped with every
heading, label and grid entry written and none of them visible. A dockable pane on Revit's
dark theme is black, WPF defaults text to black, and nothing set a foreground. It also opened
with empty dropdowns, and the line explaining that was one of the invisible ones.

**Two sources for one fact is two facts.** The grid took a view's plot from PRX_Plot_ID and
its view type from the name and never checked the two agreed, so a view named for DM-12
carrying PRX_Plot_ID DM-11 filled a DM-11 cell. Deleting every DM-11 view left it full. Fixed
in `ViewReading`. The same shape hit the column list and its count, fixed in `GridColumns`.

**A skip with nothing written down is a lie by omission.** `ModelWriter` skipped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter shows every plot and reads as correct on a
drawing. Every skip is recorded now, and a lost filter refuses the schedule outright.

## Writing

No em dash, no semicolon in prose, no emoji anywhere, including commit messages. No
generated-by footer and no co-author credit line. Comments say why, not what. Full list in
`.claude/skills/ai-max/references/writing-rules.md`.

## Working agreements

Never report a test result that did not come from a run made after the last file was
written. When something cannot be checked, say UNKNOWN rather than filling the gap. Write
what happened in `steps/log.md`, newest entry at the top.
