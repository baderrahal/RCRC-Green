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

The ribbon holds two commands on the Sheets panel.

**Scan Model** reads the open document and writes what it found to a text file on the Desktop.
It creates nothing and changes nothing.

**Scope Box** gives every view that names a plot the scope box named for that plot. It sorts
every view into six cases, shows the counts, and asks before writing. Only the case where a
view has no scope box and one exists with exactly the plot name is written. A view that
already carries a scope box is left alone whether it is the right one or not. It writes a
report either way.

Both show a progress window with a Cancel button while they read, because both walk the whole
document and a window that has stopped responding looks the same as a crash.

Build `RcrcGreen.sln` in Visual Studio 2026, then

```
.\install\install.ps1
```

which builds the layout the manifest asks for under
`%APPDATA%\Autodesk\Revit\Addins\2024\` and names every file it copied. The build does not
produce that layout, so copying the output folder across by hand leaves Revit unable to find
the assembly. Add `-Configuration Debug` for the debug build, and `.\install\uninstall.ps1`
takes it all back out.

Revit 2023, 2025 and 2027 are also on the build machine. This targets 2024 only, and
`RevitAPI.dll` for it sits at `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll`. The
build does not read that path. The two Nice3point packages supply the reference assemblies,
so the projects restore and compile on a machine with no Revit installed.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

That is the whole test suite and it is what the pull request gate runs. Nothing in it needs
Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation
- `src/RcrcGreen.Revit` is net48 and holds the ribbon and the commands
- `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only, and `install/` holds the two
  PowerShell scripts

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
- Plots also exist in the model as scope boxes named with the PlotID, for example a scope
  box named DM-41
- Elements carry a parameter named PRX_Plot_ID holding the PlotID
- Missing cross sections are placed across the middle of the plot's scope box
- The default cut is the SHORT way across the plot
- A cross section looks 10 metres. This is a starting value and the team will change it
  after sections have been placed in a real model
- View names repeat word for word across plots. The same view type on two plots carries the
  same text after the bracket, and nothing plot specific appears in a view name

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

**SectionPlacement takes the axis and the depth as required arguments.** The code picks no
default for either. The interface preselects ShortSide, because the default cut is the short
way across the plot. The depth is a plain number in whatever unit the box numbers are in, so
the 10 metres above lives in `SectionDefaults` in the Revit project and is converted to feet
there, because Revit works in feet and passing 10 straight through would place a ten foot
section.

**A read only command opens no transaction.** Scan Model exists to find out what the real
naming is before anything is created. Nothing in it writes, so nothing in it needs a
`Transaction`, and adding one would be the first step toward a command that changes a model
while claiming to read it.

**A command that writes decides everything first, then asks, then writes once.** Scope Box
works out all six cases with no transaction open, shows the counts, and only then opens a
single transaction named "Assign scope boxes" covering every assignment, so the whole run is
one undo. The progress window is not pumped while that transaction is open, because letting
other clicks through mid write is how a model ends up half changed.

**A command never overwrites something a person put there.** Scope Box reports a view that
carries the wrong scope box and leaves it alone. Someone chose it, and this add-in does not
know why.

**Anything not written down is UNKNOWN.** Do not invent a rule about codes, naming, plots
or geometry. Open questions are recorded in `steps/log.md`, and they are answered by the
team, not by a plausible guess.

## Hooks

Three of them, wired in `.claude/settings.json`. They are walls, not requests.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit that does not carry `steps/ai-max-state.md`
- `writing-check.sh` reads the commit message and every file the commit carries, and
  refuses one holding an em dash, a generated-by footer, a co-author credit line, an emoji,
  or a word from the list in `.claude/skills/ai-max/references/writing-rules.md`

The word check skips `.claude/skills/`, because the word list lives there as data, and it
drops the word landscape from that list, because it is the discipline name here and it appears
in real view names.

Both commit hooks work out what the commit will really contain through
`.claude/hooks/commit-scope.py`, which reads the command rather than the index. A hook script
that cannot be found blocks the action instead of passing it.

## Things that have gone wrong before

Add to this whenever something breaks. Over time it is the most valuable part of this file,
because it is the only part that cannot be rediscovered by reading the code.

**A guard that fails open reads exactly like a guard that passed.** `writing-check.sh` took
its file list line by line, so a file name holding a space arrived at the scanner in pieces
and every piece read as a file that does not exist. The file went through unchecked and the
hook reported success. Fixed in 6f0cf1d. The same shape came back twice in the phase 8
findings, once for a missing hook script exiting 127 rather than blocking, and once for a
commit form the hook never looked at. When a check cannot see its subject, it has to refuse.

**A number that is not a number still looks like an answer.** `PlotBox` rejected NaN and let
infinity through, and every centre derived from an infinite bound came out NaN. A section on
that box was handed back as an ordinary result. Fixed in 7ae6759. Geometry read from a model
is worth checking at the door, because nothing downstream will notice.

**The naming pattern was a guess from four examples.** The first real model holds a sheet
numbered 600QD named SOFTSCAPE SCHEDULES and a model called NG05, none of which fit the
pattern in the project facts above. Nothing built on that pattern is safe until Scan Model
has been run on a real model and the output read. Treat the parse summary as the source of
truth about naming, not the examples.

**A test that reads the code back to itself proves nothing.** Four tests compared the grid
against its own output, or worked out the expected value with the same rule the code uses.
Each one stayed green while the behaviour it named was broken. Write the expected value out by
hand, then break the code once and watch the test go red.

## Writing

No em dash, no semicolon in prose, no emoji anywhere, including commit messages. No
generated-by footer and no co-author credit line on commits or pull requests. Comments
say why, not what. The full list is in `.claude/skills/ai-max/references/writing-rules.md`.

## Working agreements

Never report a test result that did not come from a run made after the last file was
written. When something cannot be checked, say UNKNOWN rather than filling the gap. Write
what happened in `steps/log.md`, newest entry at the top.
