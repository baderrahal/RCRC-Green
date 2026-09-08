# RCRC Green

A Revit 2024 add-in for the landscape production team. The first tool in it is Drawing
Sheet, which scans the model, shows a grid of which views exist per plot, and creates the
missing ones.

Read `.claude/skills/ai-max/SKILL.md` before doing any work in this repo. It sets the phase
order, the writing rules and the reporting rules that everything here follows. The current
phase is written in `steps/ai-max-state.md`.

Done means: for a model of plots, the tool lists every plot, shows which of the known view
types each one is missing, and creates those views with correct names and, for cross
sections, a cut through the middle of the plot.

## Running it

Not yet. The ribbon exists and holds no commands. Build `RcrcGreen.sln` in Visual Studio
2026, then copy the output as described in `src/RcrcGreen.Revit/RcrcGreen.addin` into

```
%APPDATA%\Autodesk\Revit\Addins\2024\
```

Revit 2023, 2025 and 2027 are also installed on the build machine. This add-in targets 2024
only, and `RevitAPI.dll` for that version sits at

```
C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll
```

The build does not read that path. The two Nice3point packages supply the reference
assemblies, so the projects restore and compile on a machine with no Revit installed.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

That is the whole test suite and it is what the pull request gate runs. Nothing in it needs
Revit. Run it after the last file is written, never before, and never quote a count from an
older run.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation
- `src/RcrcGreen.Revit` is net48 and holds the ribbon and, later, the commands
- `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only

## The rule that keeps the tests possible

**RcrcGreen.Core must never reference the Revit API.** No `Autodesk.Revit` using, no
package reference, no type from it in a signature. Core exists so the naming, the plot
list, the grid and the section maths can be run on a build machine with no Revit on it. The
moment Core touches the API, the test project cannot load and the gate stops protecting
anything.

Anything that reads a `Document`, a `View`, an `Element` or a `BoundingBoxXYZ` belongs in
`RcrcGreen.Revit`. Pull the plain numbers and strings out there, hand them to Core, and put
what Core returns back into the model.

## Project facts

These come from the team and from real models. They are not guesses.

- Views and sheets are named `<PlotID>-(<code>) <View name>`
- PlotID is always two letters, a dash, then digits. Examples DM-41, PF-12
- The code is digits inside round brackets. Examples 010, 200, 400
- The view name is free text after the closing bracket and one space
- Plots also exist in the model as scope boxes named with the PlotID, for example a scope
  box named DM-41
- Elements carry a parameter named PRX_Plot_ID holding the PlotID
- Missing cross sections are placed across the middle of the plot's scope box
- The default cut is the SHORT way across the plot

Real names seen in a model:

```
DM-41-(010) Location Key Plan
DM-41-(010) Overall Key Plan
DM-41-(200) General Arrangement Layout
DM-41-(400) Landscape Cross Section
PF-12-(200) General Arrangement Layout
```

## Conventions

**A view type is the code and the view name together.** Code 010 above appears twice with
two different view names, so the code on its own does not say which view something is.
`ViewType` holds both and the grid columns are built from it.

**The plot list is the union of three sources.** A plot can be found from a view name, from
a scope box, or from PRX_Plot_ID on an element. A plot that only has a scope box and some
tagged elements has no views at all, and that is the plot the team most needs to see, so it
has to survive into the list rather than be dropped.

**SectionPlacement takes the axis as a required argument.** The code picks no default. The
interface preselects ShortSide, because the default cut is the short way across the plot,
and that choice belongs to the interface rather than to the maths.

**Anything not written down is UNKNOWN.** Do not invent a rule about codes, naming, plots
or geometry. Open questions are recorded in `steps/log.md`, and they are answered by the
team, not by a plausible guess.

## Hooks

Three of them, wired in `.claude/settings.json`. They are walls, not requests.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit that does not carry `steps/ai-max-state.md`
- `writing-check.sh` reads the staged files and refuses a commit holding an em dash, a
  generated-by footer, a co-author credit line, an emoji, or a word from the list in
  `.claude/skills/ai-max/references/writing-rules.md`

The word check skips `.claude/skills/`, because the word list lives there as data, and it
drops the word landscape from that list, because it is the discipline name here and it
appears in real view names.

## Things that have gone wrong before

Nothing yet. Add to this whenever something breaks. Over time it is the most valuable part
of this file, because it is the only part that cannot be rediscovered by reading the code.

## Writing

No em dash, no semicolon in prose, no emoji anywhere, including commit messages. No
generated-by footer and no co-author credit line on commits or pull requests. Comments
say why, not what. The full list is in `.claude/skills/ai-max/references/writing-rules.md`.

## Working agreements

Never report a test result that did not come from a run made after the last file was
written. When something cannot be checked, say UNKNOWN rather than filling the gap. Write
what happened in `steps/log.md`, newest entry at the top.
