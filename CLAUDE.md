# RCRC Green

A Revit 2024 add-in for the landscape production team, built as four tasks by sessions that
cannot see each other. Drawing Sheet reads the model, shows a grid of which views exist per
plot, and creates the missing ones. KPI scans a model and fills the client's GRP KPI
Checklist workbook from it. Coordination Layout and BOQ Schedules are next.

Read `.claude/skills/ai-max/SKILL.md` and `.claude/rules/territory.md` before doing any work
in this repo. The first sets the phase order, the writing rules and the reporting rules. The
second says which files your task may edit. Your task's phase is in its own
`steps/ai-max-state-<task>.md`.

Done means: the tool lists every plot in a model, shows which known view types each is missing,
and creates them with correct names and, for a cross section, a cut across the middle of a plot.

## Running it

**Drawing Sheet** is a dockable panel that reads the model every time it is shown. Five numbered
steps, one open at a time: PLOTS, VIEW TYPES, MARK, SHEETS, RUN. A shut step carries its own
summary and an unusable one says why. A filled square is a view that exists and opens on a
click, an empty one is missing and can be marked one at a time, by row, by column or all at
once. Every dropdown comes from the model, and Scan Model reads the whole document.

**Run**, step 5, creates what the marked cells on the ticked plots ask for: plan views, sections,
schedules and sheets. One confirmation, one transaction, one undo, one report.

**KPI Checklist** opens a pane of the model name, KPI Scan, a status line, the template picker,
the output folder, the plot picker and Create. KPI Scan writes nine sections and Create copies a
template, patches it and writes a report. **Neither creates anything in the model**, and the
template is opened for reading and never written to. **The elements a schedule lists are
not the scheduled things:** the softscape schedule returns RVT Link instances, so its printed
rows are its numbers' only route, and its group rows are the only place a phase shows. Every
measured fact is in `.claude/rules/kpi-rules.md`.

Build `RcrcGreen.sln` in Visual Studio 2026, then run `.\install\install.ps1`. It builds the
`Addins\2024\` layout the build does not, so copying the output folder by hand leaves Revit
unable to find the assembly.

## Running the tests

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
```

The whole suite. It is what the pull request gate runs and nothing in it needs Revit.

## How this is laid out

- `src/RcrcGreen.Core` is netstandard2.0 and holds every rule and calculation, in three
  homes: `Shared/` for what every task reads, and one folder per task, `DrawingSheet/` and
  `Kpi/` so far. `src/RcrcGreen.Revit` is net48 and holds the ribbon, the commands and the
  panes. `tests/RcrcGreen.Core.Tests` is net8.0 and covers Core only. `install/` holds the
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

## Project facts

These come from the team and from real models. They are not guesses.

- Views are named `<PlotID>-(<code>) <View name>`. PlotID is two uppercase letters, a dash,
  then digits, as in DM-41, and lowercase is invalid. The code is digits in round brackets and
  the view name is free text after the bracket and one space. A plot is also a scope box
  named with the PlotID. A SHEET IS NOT NAMED THAT WAY: its name is the view name upper cased
  with no plot and no code, LIST OF DRAWINGS and SOFTSCAPE SCHEDULES on the first real model,
  and its number is the code, the plot letter and a sheet letter, 010QF and 600QD
- **There are two plot parameters, not one.** `PRX_Plot_ID` sits on views and on sheets and the
  Sheet List filters on it. `PRX_Ref Plot ID`, with spaces rather than underscores, sits on
  model elements and every quantity schedule filters on that one. A schedule built against the
  wrong one comes back empty
- On a view, PRX_Plot_ID is the first place to look for the plot and the name is the fallback
- Cross sections are cut across the middle of the plot's scope box, the SHORT way
- View names repeat word for word across plots, and nothing plot specific appears in one

Six of the things under a plot are schedules, filtered on PRX_Ref Plot ID. Category alone does
not identify one: HARDSCAPE and SHRUBS AND LAWN are both Floors, told apart by their second
filter. A schedule is built on Revit's NUMBER for the category, a filter value goes back as its
own kind, a calculated field cannot be added at all, and field names are copied exactly.

**A view family type is not named after the view type, and one view type is not built one way.**
DM-11-(010) Location Key Plan uses `(010) Key Location Plan`, words swapped, so nothing anywhere
is matched on a name. A new view takes its family type, level, template and two crop settings
from ONE view of the same type the model holds, and the report names it. Three (010) views in
one run took three different family types that way. **(400) Landscape Cross Section is a section
rather than a plan view**, read off that view's kind rather than off the code.

Three settings are the tool's own rather than the model's, because the model disagrees with
itself or has the fault built in. Cross sections look 1 METRE, where four real ones read
0.93, 0.93, 1.53 and 12.83. ANNOTATION CROP IS ALWAYS ON for a plan view. CROP VIEW IS
ALWAYS ON for a section the tool makes, because a section with it off is not bounded
sideways and the model's own sections have it off, which is what drew other plots' markers
through DM-11's plans. The report says whose setting each is.

A new view is CREATED FRESH, never copied from another plot, and carries no annotation or
detailing. A SHEET IS DESCRIBED rather than copied: the title block type, the views and 1, 2 or
4 per sheet are shared, and the views DIVIDE into as many sheets as they need, none left off. A
sheet's size is read off the title block PLACED ON IT, because Sheet Width and Sheet Height only
exist on the instance. A SHEET HAS NO SCALE OF ITS OWN: its Scale reads out its views' templates.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 not on sheets, 79 view templates
- 406 scope boxes for 160 PRX_Plot_ID values, so a box not named for a plot is ordinary
- 2,114 names parsed and 4,039 did not, which made the parameter the first source not a fallback
- 6 views are named for one plot and carry PRX_Plot_ID for another. The scan names them
- Only plot DM-11 has real sheet numbers, 010QE to 600QD, all with plot letter Q. Others carry
  numbers like 010QE Copy 001, and a group of sheets has no PRX_Plot_ID
- Title blocks are AR-PRX-Title_Block_A1, several types. Read them from the model, never fix them

Measured on the 1355 run. These are what pick a workbook and fill it:

- **PRX_Component on the sheet is the ASSET TYPE, not a park name.** FRIDAY MOSQUE and SCHOOL,
  on 1,384 of 1,385 sheets. It is what picks the workbook template. **Its values are not the
  template names and no string rule turns one into the other**, so the mapping is A TABLE,
  measured on the 1548 scan, 11 values over 1,384 sheets: DAILY MOSQUE and FRIDAY MOSQUE mean
  MOSQUES, SCHOOL SCHOOLS, HEALTH HEALTHCARE, PARKING LOT PARKING, EXISTING PARK EXISTING PARKS,
  FUTURE PARK FUTURE PARKS, and NH STRT LESS 20m ROW, NH STRT 20m ROW, STREET 30m ROW and
  STREET 36m ROW all mean STREETS. It is many to one, the two park values break the park tie, and
  a value the table does not hold preselects nothing and says so. **The plot prefix never
  overrides it**: STREET 36m ROW covers MM and ST plots and NS carries two street widths
- KPI COMPONENT S/H is a show and hide toggle, No on 9 of 9 title block types. Its name holds
  COMPONENT and it is not a candidate for anything
- **PRX_Plot_ID is the plot** and the thing every schedule filters on. FM-05, SC-03
- **The plot prefix is a SECOND route to the template**, confirmed by the team, all seven:
  STREETS NS, ST and MM, PARKING PL, MOSQUES FM and DM, SCHOOLS SC, EXISTING PARKS EP,
  FUTURE PARKS FP, HEALTHCARE HF. It agrees with the eleven component values prefix by prefix
  with nothing left over on either side. PRX_Component still decides and where the two disagree
  neither does. It is what groups plots for the picker, and it is the only thing that can place
  a plot with no sheet, as EP-05, EP-11, EP-12 and EP-13 are
- PRX_Plot_UID is numeric with nulls. PRX_Plot_UID2 reads ANH-007-MO-100019. PRX_Plot_NH is the
  same on every sheet. None of the three is the plot
- **The softscape schedule prints TREES, then a group row per phase, then the species under it,
  then a subtotal per group, then TOTAL.** A species can appear under BOTH groups: ALBIZIA
  LEBBECK is 1 existing and 13 proposed on DM-12. THE GROUP ROW MUST TRAVEL WITH THE SPECIES ROW
- **A SHRUBS AND LAWN GROUP PRINTS ONE SUBTOTAL PER PHASE, THEN THE GROUP TOTAL.** Measured on
  the 0928 run over 20 mosque plots. A group holding Existing and Proposed prints three rows and
  the LAST is the group's value. A group holding one phase prints two equal rows, which is what
  DM-11 does and what made it the wrong plot to learn the shape from. Four out of four, the last
  row is exactly the ones above it added, in area and in item count: DM-16 shrubs 30, 54, 84 and
  FM-05 shrubs 361, 459, 820
- **Existing species print with no image and often NO BOQ CODE AVAILABLE.** One is called
  UNKNOWN. The image is the FIRST cell, so an existing species row starts with a dash and a row
  is a species row when the BOTANICAL NAME column holds text, never when the first cell does
- **The client's tree list can be shorter than the model.** The first real workbook, DM-12 on
  MOSQUES, came out correct and read 2 existing trees where the model holds 10 and 31 in total
  where it holds 39, because the 80 species in the MOSQUES list hold no Phoenix dactylifera, no
  Washingtonia robusta and no Unknown row to match. A species the list does not hold is WRITTEN
  INTO an empty row now, its name and its count and nothing else, so the counts reach the total.
  **Nothing may ever place an unmatched species by guessing:** no family, no genus, no native
  flag, no code. It is named in the report and what those missing columns cost is an open
  question for the team
- **Every plot has two filled regions in the 00 link**, one CADASTRAL LIMIT and one OUT OF SCOPE
  (PRESENTATION), and WHICH OF THEM CARRIES THE AREA VARIES BY PLOT. DM-11, DM-12 and DM-13 hold
  it on OUT OF SCOPE with cadastral at 0. NS-19 and NS-06 hold it on cadastral. The type name
  cannot decide it

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

## Hooks

Four, wired in `.claude/settings.json`. The three commit hooks read the command rather than
the index, through `commit-scope.py`, and a hook script that cannot be found blocks.

- `block-paths.sh` refuses any write that resolves outside this repo
- `require-file-on-commit.sh` refuses a commit not carrying a `steps/ai-max-state-<task>.md`
- `territory-check.sh` refuses a commit touching two tasks' files, or `Core/Shared` next to
  any task's files. The map is in `.claude/rules/territory.md`
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

**Two sources for one fact is two facts.** Eight times now, always the same shape. A view named
for DM-12 filled a DM-11 cell that stayed full after every DM-11 view was deleted. The column
count against its list. The run report, which made nothing and named four views under PLAN VIEWS
and under NOT CREATED. A panel step reading the range off its arguments. One method filling both
the code buttons and the Add row's dropdown, split and half kept. A schedule category read one
way on capture and looked up another way on create. The softscape reader taught to read the
botanical name off the column the heading row names while the group counter that feeds the same
report was left reading the first cell, which is the image. Create handed the model's FOLDER to
the words that say whether a model is open, so a detached model that has never been saved was
refused with No model is open beside a header counting its 96,959 elements.

**A skip with nothing written down is a lie by omission.** `ModelWriter` dropped a schedule
field it could not resolve, and a filter, both with a bare `continue`. A schedule short of a
column looks finished. One short of its plot filter reads as correct on a drawing. Every skip is
recorded now, a lost filter refuses the schedule, and that delete is checked.

**One example is not a rule, and the model may hold no rule at all.** The family type was
matched on the view type because one Properties panel showed a type named exactly that. The next
plot's is `(010) Key Location Plan`. Copying a sibling replaced it, and then three (010) views
copied three different family types. Annotation crop went the same way one round later. **The
worst of them was the shrubs and lawn subtotal**: DM-11 prints two equal rows per group, that
was read as one subtotal printed twice, and the tool took the first of the two for four rounds.
It is one subtotal per PHASE and then the group total, and DM-11 is the one plot where taking
the first cannot be wrong, because every one of its groups holds a single phase. The 0928 run
over 20 plots is what showed it. **One plot is not a sample.**

**A name is not an identity and a word is not a value.** Sheet Width was read off a title block
TYPE, where that instance parameter does not exist, so null became 0.0 and three sheets came out
empty. Slab Edges was looked up by display name in a collection that does not hold it, on a model
that has the category. A Yes/No filter was handed back the word Yes when Revit stores 1. Three
rounds, one shape. Ask the API what a thing IS rather than what it is called.

**Copying takes whatever state the thing is in, including nothing.** The first sheet the tool
made was empty, copied from one the user picked that had no views on it. Nothing failed and
nothing was reported. A sheet is described now, so what goes on it is stated.

**The interface can change a name on its way to the screen.** WPF reads the first underscore in
a button's text as an access key marker, swallows it and underlines the next letter, so the KPI
pane offered PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. Five names no
model holds, in a tool whose whole job is exact parameter names. The strings were right in the
code and wrong on screen, which no test of the code would ever have caught. What goes on a
button is escaped now and the escape has a test. The Drawing Sheet's tick boxes and its
scope box case buttons go through the same escape now.

**A copy taken once and never refreshed reads exactly like a fact.** The KPI pane held the
model's folder from the one read that happened when it was shown. The model was then saved to a
real folder and Create stayed grey saying No model is open, and a second scan did not shift it,
because nothing re-read the folder. The rule now: **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR.** What a button refuses on is read off the live document at the moment it is pressed, and
what the pane shows about the model is asked for again every time it draws. Greying a button out
on a fact the pane does not own is what turned one stale string into a dead end.

**A description of the tool is not the tool.** The pane said it reads PRX_COMPONENT and
PRX_Plot_UID2 off the title block. That was the workbook's own note copied onto the screen: the
reader had been corrected rounds before, PRX_COMPONENT is in no model, and neither value is read
off a title block anywhere. A line about what the tool does is checked against what it does.

**A setting nobody recorded cannot be argued about.** A created view came out with a template
that looked right and a family type that looked wrong. The code read both off one view four
lines apart, but nothing said which view. Record where a value came from as you use it.

## Writing and working agreements

**Anything not written down is UNKNOWN.** Never invent a rule. Open questions go in the log.
No em dash, no semicolon in prose, no emoji anywhere, commit messages included. No generated-by
footer and no co-author line. Comments say why, not what. Full list in the ai-max writing rules.
Never report a test result from a run made before the last file was written. Say UNKNOWN rather
than filling a gap. Write what happened in your task's own log, `steps/log-drawing.md` or
`steps/log-kpi.md`, newest entry at the top.
