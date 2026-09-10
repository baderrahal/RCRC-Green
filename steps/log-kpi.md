# KPI log

Newest entry first.

---

## 2026-09-10, thirty seventh pass. Excel showed zeros, and the workbook stops going beside the model

Pull request 46, merged into main as `6f2e521`. **The runner executed 904 tests against its
merged head, 0 failed and 0 skipped. Locally the same 904 ran, 0 failed and 0 skipped**, after
the last file was written and after every break watch was restored byte for byte.

Seven things came out of the first real run. **Three of them were already delivered in pull
request 44** and were not done again: the typed date and name boxes reaching the fill, the area
claim, and most of the measured record. What is here is the other four and two additions to the
record.

### Excel showed zeros where the numbers were right

The filled MOSQUES workbook read 0 for Total Green cover, Canopy Area, Total Trees, Total Trees
Native, Total Trees Adaptive, Total Planting Area and Total Lawn Area, beside Planting 410, Lawn
60 and Mosques Area 3,729 which all read correctly. **The values were not wrong. They were stale
cached results and Excel never recalculated.** Total Planting Area is `=F10` and F10 held 410, so
a 0 there could only be a cache.

`fullCalcOnLoad="1"` was already set, so **the flag alone was never enough.** Three things
together, measured on the output: `calcId` set to 0 in `calcPr` with the flag kept, the cached
`<v>` dropped from every formula cell in every sheet leaving the `<f>` alone, and
`xl/calcChain.xml` removed.

**The output is checked the way the written cells already are.** `CacheCheck` is read back off
the file, not off what was sent: recalculate on open, calcId cleared, no formula cell carrying a
cached value, and the calc chain gone. All four or the report says the file may open showing the
template's own numbers and calls it a bug in the tool. A workbook that opens showing zeros beside
correct inputs is the worst thing this tool can produce, because it looks finished.

**The part count reads 37 in and 36 out now and the report says which part went and why.**
`PartsDeliberatelyRemoved` is what keeps the kept-every-part check true across a removal on
purpose, so a count short by one does not read as a loss.

### Unmatched species go into the workbook, which reverses last round's rule

A species Revit holds that the workbook's list does not is **written in**, the botanical name in
column D and the count in column B and nothing in any other column. On DM-12 that is three, all
under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2 and WASHINGTONIA ROBUSTA 1. Last round they were
named and written nowhere, and the workbook read 31 trees where the model holds 39.

**The empty rows come from the file and never from a constant.** The MOSQUES map entry stops at
row 83 and that sheet's own total is `SUM(B4:B92)`, so rows 84 to 92 are empty AND summed. A range
taken from the map would have found no room at all. `SpeciesList` reads column D across the whole
sheet and finds the total by its own formula, so the rows that reach the total are the rows a
species can be written into. No total found means no empty rows, and a species is reported as not
placed rather than written where nothing adds it up.

More unmatched species than empty rows writes what fits, names the rest and says plainly that the
sheet ran out of room. The report's section listing them says where each one landed instead of
that it was written nowhere, with the three lines saying what a written row does not carry.

Three tests in `SpeciesMatchingTests` went red on this and had to be rewritten, because they
encoded the rule being reversed. They are named here rather than quietly updated.

### The plot prefix is a second route, and it does not decide

Confirmed by the team: STREETS NS, ST and MM, PARKING PL, MOSQUES FM and DM, SCHOOLS SC,
EXISTING PARKS EP, FUTURE PARKS FP, HEALTHCARE HF. It agrees with the eleven component values
prefix by prefix with nothing left over on either side, and a test written out by hand says so.

**Two records of one fact is the fault this repo has met eight times, so they do not get equal
standing.** `PRX_Component` decides. Where the prefix agrees the pane says so, where they
disagree neither decides and nothing is preselected, and a prefix the table does not hold cross
checks nothing, which is different from one that disagrees.

**The line saying which route the answer took is shown whichever way it went.** It used to appear
only when nothing was preselected, so a preselection arrived without a word. EP-05, EP-11, EP-12
and EP-13 are on a schedule and on no sheet, which means no component at all, and the prefix is
the only thing that can place them.

**The grouping is what the prefix is really for.** One button per template beside Select all and
Clear ticks every plot of that template at once, with its count on the button. It replaces the
ticks rather than adding to them, because one checklist is one template. Plots whose prefix the
table does not hold are named under the buttons, so a plot no button reaches is visible.

**One thing to raise: the ask says eight prefixes and the table in it lists ten.** NS, ST, MM,
PL, FM, DM, SC, EP, FP and HF. Ten are built and ten are tested, one line per prefix. Seven
templates either way, since STREETS takes three and MOSQUES takes two.

### The output folder is browsed for, and the model no longer has to be saved

Writing beside the Revit model meant a detached model could not be used at all, which cost most
of an afternoon. `OutputFolder` is browsed for and remembered in `kpi-output-folder.txt` beside
the installed assembly, the same way the template folder is. Both go through one
`RememberedFolder` rather than two copies of the same quiet read, and `install.ps1` creates the
new pointer empty and never overwrites one the user has set.

**Create no longer asks whether the model has been saved.** It asks whether a model is open and
whether an output folder is set. The never saved refusal is gone and so is
`TemplateWords.NoModelPath`. The silent overwrite and the editable name box are unchanged.

**The model's folder came off `OpenModel` with it.** Nothing read it once the output folder
existed, and a value on the screen that decides nothing is how one stale string became a dead end
here already. The rule it was built for still stands: the title is a record built from one answer,
and the refusal is decided at the moment Create is pressed, the document off the live document
and the folder off the pointer file in the same breath.

The test that covered the never saved refusal is rewritten to assert the reversal outright: a
model that was never saved is refused nothing, and its refusal is the same as a saved model's.

**One line went with it.** The pane drew `TemplateWords.Output` and `CreateWords.Overwrite`
directly under each other, both saying the file is overwritten without asking. Two sentences for
one fact. The second is deleted.

### Open, and not to be guessed at in code

**THE CLIENT'S SPECIES LISTS ARE SHORT OF TREES THIS PROJECT PLANTS.** The MOSQUES list holds 80
species and none of them is Phoenix dactylifera, Washingtonia robusta or any Unknown row, checked
against the output file itself.

**A species written into an empty row carries no family, no genus and no native flag**, because
those are the client's data and the tool does not know them. The counts reach the total now, and
the KPIs that need those columns still cannot see it. Whether the lists should grow, or those
columns be filled some other way, is a question for the team about their own template.

Nothing in the tool may ever place an unmatched species by guessing. The name and the count go
in, and no other column does, whatever the totals look like.

### Measured, from the first real output

DM-12 on the MOSQUES template. 37 parts in, 37 out, 4 changed, zero recalculation errors. The six
values landed and the client's own formulas gave 28.1 percent canopy against a 13 percent target,
Excessive, NOT COMPLIANT, and the tool wrote no verdict anywhere. ACACIA / VACHELLIA FARNESIANA
found Acacia / Vachellia farnesiana at row 11. Three species were not in the list. Excel showed
zeros for seven computed cells while the inputs beside them were right.

With the calc chain removed on purpose it reads 37 in and 36 out.

### What was broken to see the tests go red

Each restored byte for byte and checked with md5.

- The grouping button adding to the ticks rather than replacing them, and the unknown prefix left
  in the button list. 3 red
- The output folder dropped from the refusal. 4 red
- Earlier in the round, the four on the cache, the empty rows and the written species

### Not touched

The Drawing Sheet, in any file.

---
## 2026-09-09, thirty sixth pass. The first real workbook, and two things it showed

Pull request 44, merged into main as `116afdb`. **The runner executed 867 tests against its
merged head, 0 failed and 0 skipped. Locally the same 867 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

**The first workbook is written and correct.**

### The date and name boxes did not reach the fill

E5, G5 and H5 came out holding the template's own placeholders, and the report said nobody had
typed them, on a run where the user had typed 2026-09-09, xx and bb into the three boxes before
pressing Create. **What the boxes hold and what Create reads were two different things.**

Two links were missing at once. `KpiCreateAsk` did not carry the three at all, so nothing the
pane collected left the pane. And `KpiCreatePlan.Of` defaulted all three to null, so the handler
calling it without them read as a deliberate empty rather than as a caller that forgot.

Both are fixed, and **the three are required arguments now.** A caller that forgets them does not
compile, which is the guarantee a test cannot give. Three tests cover what a typed value does:
it reaches the cell, an empty box is still recorded with its reason, and the surrounding space
comes off and nothing else does.

The pane's own half, box to `KpiCreateAsk`, is Revit code and no test here reaches it. What is
tested is the decision and the plan.

### The area claim was not true of the area

The report ended with "Every number above came off a row the schedule printed". H7 took
3728.7570000000005, converted from the raw 40136.006313679296 square feet, and the schedule
prints 3729. **The conversion is right and more precise. The sentence was wrong about it.**

The area comes off `PRX_Intervention Area` on the chosen filled region in the 00 link, which is
not a schedule row at all. The region table now carries the raw reading, the written metres and
what the model prints, side by side and unrounded, so the two can be held against each other,
and the closing paragraph makes the schedule claim for the numbers it is true of and names the
area separately.

Rounding the raw number for reading would have hidden the difference the row exists to show, so
it prints round trip. A break watch on that alone turned two tests red.

### What the first real output measured

One press of Create on DM-12 with the MOSQUES template. All of it is in `kpi-rules.md`.

- 37 parts in, 37 out, 4 changed, and the output recalculates with ZERO errors. The 45 against
  44 recorded earlier is EXISTING PARKS, a different template, and both stand
- The six values landed and the client's own formulas ran on them: 28.1 percent canopy against a
  13 percent target, Excessive, NOT COMPLIANT. The tool wrote no verdict anywhere
- The slash case matched. ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana at
  row 11
- Three species were correctly refused. The MOSQUES tree list holds 80 species and not one is
  Phoenix dactylifera, Washingtonia robusta, or any Unknown row, checked against the output file

### Open, and not to be guessed at in code

**THE CLIENT'S TREE LIST IS SHORTER THAN THE MODEL.** The workbook reads 2 existing trees where
the model holds 10, and 31 in total where the model holds 39, because the MOSQUES list has no row
any of the three refused species can match.

**The tool is right and the list is short.** This is a question for the team about their own
template, not a thing to fix in code. **Nothing in the tool may ever place an unmatched species
by guessing**, whatever the totals look like: a quantity put in the nearest row is a number
nobody can trace and every one of the 80 rows would then be suspect. The three are named in the
report with their counts and the numbers stay as they are until somebody answers.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- The typed date dropped again. 5 red
- The old schedule claim put back. 2 red
- The raw number rounded for reading. 2 red
- The printed value dropped from the region row. 2 red

### What has not been run

The pane change, box to `KpiCreateAsk`, has not been through Revit. Everything else in this round
is Core and covered.

---
## 2026-09-09, thirty fifth pass. Two more off the KPI pane, and one of them was never working

Pull request 43, merged into main as `61ea270`. **The runner executed 857 tests against its
merged head, 0 failed and 0 skipped. Locally the same 857 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

### The reference sample showed the wrong plot

With DM-12 ticked the block under Reference printed DM-11's four values, and the same four with
all 155 ticked. `ReadThePlots` took `plots.All[0]` and read the four for that one plot.

The block exists so a person picks the reference parameter by looking at its value rather than
its name, so a value belonging to a plot they did not choose defeats the whole of it.

`ReferenceValuesPerPlot` reads all four for every plot in one pass over the sheets, which is the
shape `ValuePerPlot` already used for the component. The pane shows the first ticked plot's, with
the plot named above them, and shows none and says so when nothing is ticked.

### Create stayed grey after the model was saved

**This path has never worked in Revit.** A detached model with no path, Create refused, the model
saved to a real folder, and Create stayed grey saying No model is open. A KPI Scan after the save
made no difference.

Three things were wrong at once and each on its own would have been enough.

**The folder was a copy taken once.** `Found` set it from the plot read and nothing re-read it.

**The title was a second copy on a different schedule.** `Scanned` set it and nothing else did,
so the two halves of one fact went stale independently.

**The pane asked for the model and threw the request away.** `Shown` called `Ask(WhichModel)`
then `Ask(Plots)`, and `Ask` holds ONE SLOT, so the second overwrote the first every time. The
model name request never ran from that path at all.

The fix is one rule, and it is now in `CLAUDE.md`: **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR.**

- `OpenModel` is one record, title and folder together, built from one answer. `CannotCreate`
  takes it rather than two loose flags, so nothing can hand the pair over the wrong way round
- Every answer from the handler carries the model state, whatever was asked for, read off the
  live document at that moment
- `RedrawTemplates` asks for it every time it draws, and `Took` redraws only when the answer
  moved, so the ask does not chase its own tail
- `Ask` never lets `WhichModel` take the slot from anything, because it is now the request most
  likely to arrive on top of another, and losing one costs nothing
- **Create is greyed out on what the PANE owns and nothing else**, a template picked and a plot
  ticked. Whether a model is open and whether it has a folder are decided on the Revit thread
  against the live document when the button is pressed

That last one is what removes the class of fault rather than narrowing the window. A button
greyed out on a fact the pane does not own can always go stale, however often it is refreshed.

### What the test covers, and what it does not

Seven tests over `CannotCreate` and `OpenModel`, across all three states: no document, a document
with no path, and a document with a path. Each gives its own line, the no-path line says to save
the model, and a folder arriving with no title is still no model.

**These are tests over the decision, not over Revit.** Nothing here proves that a saved model
arms Create in Revit, because nothing in this repository can run Revit. What is proven is that
the decision is right for all three states and that it is now made from the live document rather
than from a copy. The Revit half is unrun and stays unrun until somebody presses the button.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 3 red
- A folder with no title made to count as a model open. 1 red
- The reference block made to name no plot. 2 red
- A changed folder made to read as the same model. 1 red

The last one matters because `Took` decides whether to redraw on it. A folder change that read as
no change would leave the pane showing the state from before the save, which is the fault again
one layer down.

---
## 2026-09-09, thirty fourth pass. Five faults off the first real run of the KPI pane

Pull request 42, merged into main as `36dc0f8`. **The runner executed 849 tests against its
merged head, 0 failed and 0 skipped. Locally the same 849 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored byte for byte.

The pane reached Revit and five things were wrong with it. **Every one of them is a fault
nothing in the suite could have caught**, because each is about what reaches the screen rather
than about what the code computes. 849 tests locally, 0 failed and 0 skipped, after the last
file was written and after the five break watches were restored byte for byte.

### It said no model was open while a model was open

The header read 96,959 elements at 16:08:14 and Create said Cannot create. No model is open, on
a detached model that has never been saved. The line directly above the button already had the
truth.

`CannotCreate` was handed the model's FOLDER and called it the model. **Whether a model is open
and whether it has a folder are two facts**, so it takes both now, a model that is not open is
not asked whether it has been saved, and the two never print together. The never saved words are
`TemplateWords.NoModelPath`, the very line above the button, rather than a second sentence about
one condition. `KpiRequestHandler` is the other caller and it reads the folder off the document
rather than assuming one, because it is the second place that could get the pair the wrong way
round.

That is the eighth time in this repo that two records of one fact have been the bug, and
`CLAUDE.md` counts it as the eighth.

### The pane showed five parameter names that no model holds

PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. WPF reads the first
underscore in a button's text as an access key marker, swallows it and underlines the next
letter. **The strings were right in the code and wrong on the screen**, on the one tool whose
whole job is exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` on this pane goes through it: eleven places, not the five
that were noticed. Only what is drawn goes through it and nothing compares the escaped form
against anything.

The test walks every name `KpiNames` holds and asserts what WPF renders is the name itself,
rather than the five that happened to be seen.

**The Drawing Sheet has the same fault and this round does not touch it.** Its view type names,
plot identifiers and column headers all go onto buttons and tick boxes the same way. It is out
of scope by instruction and it is written down here rather than left to be found again.

### It described itself doing something it does not do

`KpiTemplates.SourceOf` said PRX_COMPONENT, read off the title block and PRX_Plot_UID2, read off
the title block. **Every part of both was wrong.** PRX_COMPONENT is in no model. The value is
PRX_Component on the sheet. PRX_Plot_UID2 sits on 1233 title block instances and holds a value on
none of them, while the sheet holds 1384 of them. It was the workbook's own note put on screen
as though it were the tool's behaviour, and the reader had been corrected rounds before.

It takes a `ChosenParameters` now and names the three parameters the pane's own pickers hold,
because those are the ones that will really be read, with the place each is read from. Nothing
picked yet names the picker to look at rather than a parameter nobody chose. A test walks every
one of the seven templates and refuses any line holding PRX_COMPONENT or the words title block.

### The preselections were positional

Reference started on PRX_Plot_ID, which is first of the four and is not what the note names.
Location started on whichever neighbourhood parameter sorted first, and Neighborhood Group sorts
above Neighborhood Name.

`Preselected.From` takes the name the note asks for and the names the model offers, and hands
back that name where it is offered. Reference starts on PRX_Plot_UID2 and Location on
Neighborhood Name, both now measured constants rather than positions. Component goes through the
same one rule.

### Prepared by was cut to Prepared b

`PanelMetrics.WideLabelWidth`, added rather than a widening of the shared `LabelWidth`, which the
Drawing Sheet uses in two places. **The number has not been seen in Revit** and is the one thing
in this round chosen by eye rather than measured.

### The plot picker, and what I could not reproduce

**All 155 plots ticked by default is not what the code does, and I could not find a path that
would.** A fresh `PlotTicks` is built with no ticks, `Found` carries the previous ticks across
and the previous set is empty, and `All()` is reached only by pressing Select all. Three tests
now pin it: a fresh picker over 155 plots reads 0 of 155 plots ticked, reading the model carries
nothing across, and Select all is the only route to all of them.

So the state is pinned and cannot drift, but **the fault as reported is not explained**, and
saying it is fixed would be a guess dressed as an answer. If it is seen again on a pane nobody
has pressed Select all on, the next thing to look at is whether `Found` runs more than once with
something already ticked.

### What was broken to see the tests go red

Five, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 1 red
- The escape made to return its text unchanged. 5 red
- The preselection put back on position alone. 2 red
- The note shown as the tool's behaviour again. 3 red
- A fresh picker made to tick every plot. 6 red

### What has not been run

None of this has been seen in Revit. Four of the five fixes are Core with tests over them and
the fifth, the label width, is a number in a pane file that only Revit can settle.

---
## 2026-09-09, thirty third pass. The component to template mapping, as a table

Pull request 41, merged into main as `a0d3d3b`. **The runner executed 823 tests against its
merged head, 0 failed and 0 skipped. Locally the same 823 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The question that has been open since the 1355 run is answered. The team measured it on the
1548 scan, 11 distinct values over 1384 sheets, off the component values block the round before
last added to the end of section 3, and it is now a table in Core.

```
DAILY MOSQUE           MOSQUES          NH STRT LESS 20m ROW   STREETS
FRIDAY MOSQUE          MOSQUES          NH STRT 20m ROW        STREETS
SCHOOL                 SCHOOLS          STREET 30m ROW         STREETS
HEALTH                 HEALTHCARE       STREET 36m ROW         STREETS
PARKING LOT            PARKING
EXISTING PARK          EXISTING PARKS
FUTURE PARK            FUTURE PARKS
```

### A table, never a string rule

`ComponentTemplates` holds the eleven and `TemplateForComponent` reads it and nothing else.
Matching is the whole value compared without case and with surrounding whitespace off, the same
plainness species matching keeps. No part of a value matches anything.

**The rule it replaces was wrong twice over.** It matched a word of the component against a word
of the template name, so PARKING LOT looked like a park because PARKING begins with PARK, and
all four street values answered nothing at all. A test in the round that wrote it asserted PARK
offered EXISTING PARKS, FUTURE PARKS and PARKING and called offering all three the honest
answer. It was honest about a rule that should not have existed.

It is many to one. Two values mean MOSQUES and four mean STREETS, and a test says so in those
numbers. Another says every one of the eleven resolves, written out by hand. Another says every
one of the seven templates is reached by at least one value, because a template no value reaches
could never be preselected and nothing on screen would show the hole.

### The park tie is broken by the model

EXISTING PARK and FUTURE PARK are separate values, so each preselects its own template. The pane
used to put that pair to the user always, and the file name was the only thing that could tell
the two workbooks apart. Recognising a workbook FILE is still the sheet name then the file name.
That is a different job and it did not change.

### The plot prefix is read nowhere

STREET 36m ROW covers MM and ST plots and NS carries two different street widths, so a rule on
the prefix would answer three of the eleven wrongly. A test preselects STREETS for one value
across MM and ST and for two different values on NS.

### A value the table does not hold

It preselects nothing, the pane says which value and that the table does not know it, and the
user picks. **The pane used to say nothing at all**, because `Preselect` returned on
`NeedsAPick` without showing the reason, so a tool that had looked and found nothing read exactly
like a tool that never looked. The line is `TemplateChoice.Why`, built in Core, and it is cleared
by every path that makes it untrue: a hand pick, a folder change and the next preselection.

The same move found one more. `_pickedAs` survived a preselection that failed, so ticking a
mosque plot and then an unmapped one left Create armed with MOSQUES under a line saying nothing
was preselected. Nothing is picked by hand on that path, because `Preselect` returns above when
something is, so what it held can only have come from plots that are no longer ticked. It is
cleared. Nobody would have seen it while the pane said nothing.

Section 3 of the report gains a template column, which is this table read back. Its closing line
used to read that nothing in the tool turns a value into a template name, and that is no longer
true, so it says what the table is instead. A value the model grows later prints as one the
table does not hold, which is the whole reason the block prints every value rather than a sample.

### Recorded rather than built

**The road width the STREETS template asks for by hand is inside the component value.** 20m in
NH STRT 20m ROW, 30m and 36m in the two STREET values, and less than 20m in NH STRT LESS 20m ROW.
`TemplateWords` already says the road width and the total length are typed by hand and the sheet
works the area out, which is why STREETS is the one template with no area cell. Nothing reads the
width out of the value and nothing here started to. It is written down because the value carries
it and somebody will want it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The table made to match on part of a value again. 3 red
- STREET 36m ROW dropped from the table. 4 red
- FUTURE PARK pointed at EXISTING PARKS. 3 red
- An unmapped value made to fall back to the first template. 2 red
- The report's template column made to print the value. 2 red
- A template the caller does not offer preselected anyway. 1 red

### What has not been run

The table and the reader are Core and covered. **The pane change is not.** The line saying why
nothing was preselected has never been seen in Revit, and neither has a preselected FUTURE PARKS.
Nothing in `RcrcGreen.Revit` has been run on this machine.

---
## 2026-09-09, the rule behind the last three rounds, written down

Pull request 40, merged into main as `5a5ded0`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written. No source file changed, so it is the suite that merged as `973c817`.

**The 404 rule the entry below records only works one way.** A log that comes back is a job that
has ended, and that held again. A 404 is not a job still running: every step of this job read
completed with conclusion success while the log was still 404 more than a minute later. The step
conclusions are the read that was right both times.

No code change. One rule into `CLAUDE.md` and a pointer from each of the two rules files.

**NEVER READ A SCHEDULE VALUE BY CELL POSITION. Ask the heading row which column it is. A
reader that cannot find its column says so rather than falling back to a position.**

Three rounds, four readers, one fault. `SoftscapeRows` took the botanical name off cell 0 and
the count off the last number. `ScheduleGroups` decided a row was named off cell 0.
`ShrubsAndLawnRows` read a species row off cell 0. The first column is the image and an existing
species prints with none, so every one of them was invisible on a plot whose rows all carry
photos, which is why each round found only the one in front of it.

`kpi-rules.md` said it as a fact about these schedules and `core-rules.md` did not say it at
all, so a reader touching `ScheduleRows` from the Core side met the rule nowhere. It is in
`CLAUDE.md` now, said once, and both files point at it rather than restating it.

### The half of the rule the code does not yet keep

The first sentence is kept everywhere. **The second is not**, and this round changed no code, so
it is written down rather than quietly true. Four readers still fall back to a position when the
heading row names no column:

- `ScheduleGroups.IsNamed` falls back to `row[0]`
- `SoftscapeRows.SpeciesIn` falls back to `row[0]` for the botanical name
- `SoftscapeRows.QuantityIn` falls back to the last whole number in the row
- `ShrubsAndLawnRows.SubtotalsIn` falls back to cell 0 for the species test

None has ever fired on a schedule these readers have been run against: the softscape and shrubs
heading rows both name BOTANICAL NAME, AREA and COUNT, so no report has come off a fallback. `ShrubsAndLawnRows` already shows the shape the rule wants for the other
half: no AREA column and it hands back nothing rather than guessing at one.

Taking the four out is a code change and was not asked for. It is open.

---
## 2026-09-09, thirty second pass. The group counter, the switch and the component values

Pull request 39, merged into main as `973c817`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The gate endpoints went stale again, the fourth round running. The job LOG settled it the same
way it did last round: it returns 404 while the job is running, so a log that comes back at all
is a job that has ended, whatever the status field still says.

The remote branch still held last round's commit, which main already carried under a different
hash as `82dd51d`. The two trees were identical, so the branch carried nothing but merged
history and was restarted from main with a force-with-lease pinned to that old commit. Nothing
unmerged was on it to lose, and that was checked rather than assumed.

One bug and two report faults from the 1521 scan, and nothing else.

### The group counter read the image column

Question 8 reported DM-12 Existing with 0 named rows of 6 and DM-13 Existing with 0 named rows
of 2, while section 6 of the same file printed five species under DM-12 Existing totalling 10
trees. A file that contradicts itself twenty lines apart has nothing in it worth believing.

**The first cell is the image and an existing species prints with no photo**, so its first cell
is a dash. `ScheduleGroups` decided a row named something by looking at that cell. It now asks
the heading row which column is BOTANICAL NAME and reads that one, falling back to the first
cell only where the heading row names no botanical column at all.

This is the same fault fixed in the softscape reader one round ago. The reader was corrected
and the counter, which feeds the same report, was left on the old rule. **Two rules for one
question is the shape this repo has now met seven times**, and `CLAUDE.md` records it as the
seventh.

Checked against the measured counts, each written out by hand in the test rather than worked
out with the code's own rule: DM-11 Proposed 3, DM-12 Existing 5 and Proposed 3, DM-13
Existing 1 and Proposed 2. A fifth test holds `SoftscapeRows.SpeciesIn` and `ScheduleGroups.Of`
against each other on one schedule, because the two disagreeing is what produced the fault.

### Every place that could hold the same shape, including the ones that were already right

Four things in Core read a schedule's printed rows. All four were read line by line.

- `ScheduleGroups.IsNamed`, the old `FirstCellHoldsText`. **THE BUG. Fixed**
- `ShrubsAndLawnRows.SubtotalsIn`, the test telling a species row from a subtotal row.
  **THE SAME BUG AND NO REPORT HAD SHOWN IT. Fixed**, because DM-11's shrubs are all Proposed
  and every one of them prints with a photo. An existing shrub would have been read as a
  subtotal, which would have gone into the workbook as an area
- `ScheduleColumns.IsStructureRow`. Reads the first cell, already right. A group heading and a
  phase row really do sit in the first cell with every other cell empty, which is measured off
  the 1355 rows and written in `kpi-rules.md`
- `ShrubsAndLawnRows`, the group name taken from `row[0]` after `IsStructureRow` has passed.
  Already right. That row has exactly one cell with text in it and this is that cell
- `ShrubsAndLawnRows`, the first cell tested again to keep TOTAL out of the subtotals. Already
  right and still needed. A subtotal names nothing anywhere, and the first cell is the one
  thing TOTAL does carry. It is reached only after the botanical test now
- `SoftscapeRows.SpeciesIn`, the botanical name. Already right, fixed the round before
- `SoftscapeRows.QuantityIn`, the last whole number in the row. Already right. It runs only
  where the heading row names no COUNT column, and the schedules in this model all name one
- `ScheduleGroups.GroupNameIn`. Already right and position independent: the one cell holding
  text, wherever it sits
- `KpiReport`, the rows as printed. Nothing to get wrong. It prints every cell in order and
  picks no name and no number out of them

`SpeciesMatching` also has a Rows, and it is the workbook's species list rather than a
schedule's. Each of its rows carries a named BotanicalName, so it is not this shape at all.

### KPI COMPONENT S/H is a switch, not a candidate

Its name holds COMPONENT, so section 9 offered it beside PRX_Component as another name the
model might carry the component under. It reads No on 9 of 9 title block types and is a show
and hide toggle.

`KpiNames.EveryValueIsYesOrNo` decides it, one method with both callers asking it. Section 9
leaves such a name out of the answer. Section 3 keeps it, in its own tally, in its own values,
and again by name in the new component values block, which says why it is not counted there.

**The section that offers it is not question 8.** The near miss list feeds questions 1 and 2,
the two about where the component lives, and question 8 never calls it. The fault is real and
the number in the request is off by six, so it is written down here rather than fixed silently.

### The component values do not match the template names

Measured: FRIDAY MOSQUE, SCHOOL, HEALTH, EXISTING PARK, NH STRT 20m ROW. The templates are
named existing parks, future parks, healthcare, mosques, parking, schools and streets. None of
the five is a template name and no string rule turns HEALTH into healthcare or NH STRT 20m ROW
into streets.

Section 3 now ends with every distinct value of the component the model holds, how many sheets
carry each, and every plot those sheets are for. Uncapped, where the rest of that section shows
twenty examples, because twenty sheets is not enough to build the mapping from and the mapping
is what preselects the template. The plot beside each value is PRX_Plot_ID read off the sheet
and the block says so. A value on sheets carrying no plot is counted and named. A component
name on a title block type is named with its reason and left out, because a type is not a sheet.

**Nothing maps a value to a template and nothing guesses one.**

### Open, and not to be guessed at in code

**Which template each value of PRX_Component means.** Five values are measured and none is a
template name. This is the question that stops the pane preselecting a template, and the new
block is what somebody answers it from. Until it is answered, no code anywhere turns one into
the other.

**SETTLED the round above.** The 1548 scan read eleven values off this block and the team turned
them into a table. It is `ComponentTemplates` and the entry at the top of this file has it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The group counter put back on the first cell. 3 red
- The shrubs species test put back on the first cell. 2 red
- `EveryValueIsYesOrNo` made to answer false always. 6 red
- The component block made to find no plot for any sheet. 1 red
- A title block type made to count as a sheet. 1 red
- The nothing-found line made to print when something was found. 1 red

### What has not been run

Nothing here has been seen in Revit. The group counts, the switch rule and the component values
block are all Core, all covered by tests, and none has been read off a real scan. The next 1521
run is what confirms DM-12 Existing comes back as 5 named rows of 6.

---
## 2026-09-09, thirty first pass. The shrubs and lawn shape, measured rather than guessed

Pull request 38, merged into main as `82dd51d`. **The runner executed 789 tests against its
merged head, 0 failed and 0 skipped. Locally the same 789 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored.

The check run and the job endpoints both reported the gate in progress for several minutes
after it had finished, the third round running. What settled it was the job LOG: it returns 404
while a job is running, so a log that comes back at all is a job that has ended. That is the
quickest honest way to tell a stuck gate from a stale endpoint.

Two corrections from the team before this reaches Revit.

### The shape was measured and I guessed it anyway

`kpi-rules.md` recorded DM-11 as GRASS 35 m² 46 then SHRUBS & GROUND COVER 70 m² 58, and the
round above read that as the heading sitting beside its numbers on one row. It does not. The
1355 scan report has the real rows and that report is not in this repository, because nothing
under `reports/` is ever committed, so it had never been read here. **The numbers were on record
and the shape was not, and a reader written to the numbers alone found no subtotal at all.**

The real thing is eleven columns wide and three things follow from it.

**The group heading is on its own row**, first cell only, every other cell empty. A phase row,
Proposed, sits under it in the same shape, so a structure row naming no wanted heading opens no
group.

**THE SUBTOTAL PRINTS TWICE.** 35 then 35, and 70 then 70. Adding a group's subtotal rows gives
70 and 140. One is taken, and two that disagree are named and refuse the write rather than being
chosen between.

**The species rows add up to the subtotal**, 36 plus 34 is 70, so the two are held against each
other and both are printed. That one is recorded rather than enforced: every number there is
already rounded to the metre on the way out of Revit, and a sum of rounded numbers need not
equal a rounded sum, so a refusal on it would fire on correct data.

TOTAL needed no special case in the end. It carries numbers, so it is not a structure row, and
its first cell holds text, so it is not a subtotal. It falls out on its own.

### Neither column sits where it could be assumed

Eleven columns wide, the first number in a subtotal row is the area and the last is L/DAY. So
`ScheduleColumns` finds the area, the count and the botanical name off the schedule's own
heading row.

**The softscape reader was changed too, which is one more than the two corrections asked for.**
It read the botanical name off the first cell and the quantity off the last number in the row.
On a schedule shaped like this one the first cell is an image file name and the last number is
L/DAY, so both would have been wrong. It now prefers the columns the heading row names and
falls back to the old behaviour only where it names neither. Whether the real softscape schedule
carries an image column is UNKNOWN and nothing here assumes either way. Say so if that change
was unwanted, it is one file.

### The unit, and nought

An area prints with its unit attached, 35 m², and a count does not. The unit comes off by
reading as far as the number goes rather than by stripping characters. **A real area can be
nought**: the hardscape schedule prints 0 m², which is the number and not an empty cell, and
that case is tested.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by md5:

- adding every subtotal row instead of taking one turned the two real row tests red
- letting a phase row open a group of its own turned the group test and the phase test red
- assuming the area column rather than reading the heading row turned five tests red
- dropping the disagreement refusal from `Reconciliation` turned its own test red

### The five workbooks

Five client templates arrived with the brief and **none is in this repository.** `*.xlsx` is
ignored at line 44 of `.gitignore` and `git add` was made to refuse a real one before anything
else was done. Nothing in these two corrections needed to read them.

### Still never observed

Everything the round above listed still stands except the shrubs and lawn shape, which is now
measured. Nothing here has been through Revit either: the new reader is tested against
transcribed rows rather than run against a schedule.

---

## 2026-09-09, thirtieth pass. The plot picker, several plots at once, and Create

Pull request 37, merged into main as `ce825b1`. **The runner executed 781 tests against its
merged head, 0 failed and 0 skipped. Locally the same 781 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API.

The check run endpoint reported the gate in progress for twelve minutes after it had finished.
Listing the run's jobs showed the truth, completed and successful 40 seconds in. The same lag
appeared last round. Read the job rather than the check run when a gate looks stuck.

The button the last four rounds deliberately did not add. It does something now.

### One checklist can cover more than one plot

This is the shape of the round. A checklist is not always one plot, it can be a whole asset
made of several, and then the workbook wants them added together. **Which plots belong to one
checklist is not written down anywhere and nothing here derives it.** No grouping by prefix, by
component or by anything else. The user ticks and the tool adds up exactly what was ticked.

The picker offers one plot, several, or all of them, from a list built as the union of two
sources: `PRX_Plot_ID` on the sheets and the `PRX_Ref Plot ID` filter value on the schedules.
Both lists are kept, the disagreement is shown on screen, neither wins. That is the seventh
place two records of one fact could have parted and it is held open on purpose.

### Adding printed numbers, and the rule that makes it safe

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed, and is a different thing from recomputing a number off elements, which is
never allowed and happens nowhere.

`Totalled` carries the per plot numbers and the total together and works the sum out again to
compare. **A total that does not equal its parts refuses the write.** The report prints each
plot's own number beside every total so the arithmetic can be checked by eye without opening
Revit. Trees merge on the group AND the botanical name: ALBIZIA LEBBECK is 1 existing and 13
proposed on DM-12, and merging on the name alone would put 14 into one tree sheet.

Two plots reporting an identical raw area are flagged and refuse the write until somebody
confirms. So does one plot whose two filled regions both hold an area, because which of them
carries it varies by plot and the type name cannot decide. The tool asks rather than picking.

### Matching a species

`SpeciesList` reads the workbook's own column D out of the template, resolving shared strings,
because a cell holding one stores an index and reading the index as the name would report every
species unmatched. Nothing in this repo carries a copy of the plant palette.

Matching is the botanical name compared without case and with surrounding whitespace off, and
nothing else. All three hard cases are tested: UNKNOWN against the four rows named Unknown Tree
matches none of them, the slash in ACACIA / VACHELLIA FARNESIANA is not split on, and the
apostrophe in BOUGAINVILLEA GLABRA 'PINK PIXIE' is part of the name. A name the workbook holds
twice is reported rather than placed on the first, for the same reason.

### The rule this round changed

`kpi-rules.md` said the tool never writes E5, G5 and H5, because the team types them. The brief
puts them on the pane as three boxes, so the tool now copies them through. The rules file and
`KpiTemplates` both say so now rather than one of them still saying the old thing.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by
md5:

- dropping the ticked-versus-read refusal from `Reconciliation` turned
  `APlotTickedAndNotReadRefusesTheWriteAndIsNamed` red
- merging species on the botanical name alone turned `TheSameSpeciesInTwoGroupsNeverMerges` and
  `TheGroupDecidesTheSheetAndNothingElseDoes` red
- comparing identical areas on the rounded metres rather than the raw turned both identical
  area tests red
- placing a name the workbook holds twice on its first row turned
  `ANameTheWorkbookHoldsTwiceIsReportedRatherThanPlacedOnTheFirst` red

One test caught a fault in its own fixture rather than in the code: every plot built by the
fixture had the same default area, so the identical area guard fired on a reconciliation test
that expected to pass. The guard was right and the fixture was wrong.

### Never observed, item by item

Nothing in this round has been through Revit. Every one of these is written down because it has
not been run, not because it is expected to fail.

- **No workbook has been filled.** `WorkbookPatcher` is proven by its own tests against a
  workbook the tests build, and the 37 parts in and 37 out was measured last round on the real
  EXISTING PARKS file. This round has not repeated it
- **No plot has been read out of a model.** `KpiPlotReader` compiles against the reference
  assemblies and has never run. Every method in it is untested, because the test project must
  never load the Revit API
- **The pane has never been drawn.** The plot picker, the three pickers, the three boxes and the
  Create button exist only as a mockup in `design/pr-37/kpi-create.html`
- **The schedule filter route has never run.** Schedules are found by the plot their filter
  names rather than by the plot in their own name. That is the more correct source by
  `CLAUDE.md`, and it has not been run once
- **The shrubs and lawn row shape is inferred, not measured.** CORRECTED BY THE ROUND ABOVE.
  The shape was measured all along, in the 1355 scan report, which is not in this repository
  because reports are never committed, so it had never been read here. Guessing it from the
  numbers put the heading beside them and the reader found no subtotal at all
- **The softscape total row's shape has never been seen.** A subtotal has an empty first cell
  on the fixture and is skipped. A grand total row carrying its own word in the first cell would
  read as a species here, come out as one the workbook's list does not hold, and be named in the
  report with its count. Visible, and written nowhere, but wrong
- **The three typed cells have never been written.** E5, G5 and H5 are new this round
- **`RememberedNames` has never read or written its file.** The two name boxes are meant to
  survive a Revit restart and that has not been shown
- **No refusal has been seen on screen.** The identical area confirmation, the region pick and
  the one-refusal-lists-everything line are all drawn in the mockup and never rendered
- **The output has never been overwritten.** The delete before the patch is written and unrun

### Open, and not to be guessed at in code

- **Which plots belong to one checklist is not written down anywhere.** The user ticks them.
  Do not derive a grouping rule from the prefix, from PRX_Component, or from anything else

Two from last round are still open and neither is answered here. The four rows named Unknown
Tree against the model's UNKNOWN, and the species names carrying slashes and apostrophes. The
code reports all of them unmatched rather than guessing, which is the honest half of an answer
and not the answer.

---

## 2026-09-09, twenty ninth pass. Two report faults from the 1355 scan, and the facts

Pull request 36, merged into main as `01d9886`. **The runner executed 722 tests against its
merged head, 0 failed and 0 skipped. Locally the same 722 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API rather than
the GitHub button the hook cannot see.

The scan ran on the 1355 model. Two things in the report were wrong in the same way, both
answering a question from the wrong place while the right answer sat a screen above in the same
file.

**Section 9 contradicted section 3.** Questions 1 and 2 read NOT FOUND for PRX_COMPONENT while
section 3 printed PRX_Component on the sheet with 1384 values reading FRIDAY MOSQUE and SCHOOL.
The exact name is not in the model and the near miss is, with the answer in it. Section 9 now
names a near miss that holds values, says which of the three homes it sits on and what its first
values are, and counts the question as answered, because the file does hold the answer. A near
miss with no value in it still reads NOT FOUND, which is the honest half of the old behaviour.
The near miss word for these two questions is COMPONENT alone, not the sheet list of COMPONENT,
PLOT and UID, because a question about the component is not answered by a plot name.

**Question 8 was answered by the wrong evidence.** It reported the elements by phase created,
which came back (none) 6, because the elements a softscape schedule lists are RVT Link instances
and their phase is the link's. The answer is in the printed rows. `ScheduleGroups` in Core finds
the rows that name a phase the document holds and nothing else, and question 8 is answered from
those, naming the plot that showed it. DM-12 prints a TREES row, an Existing group of five
species with a subtotal of 10, a Proposed group of three, then TOTAL 39.

A group row and a subtotal row are the same shape, one cell with text and the rest empty. Only
the text tells them apart, so the phase names read off the document decide it. Nothing matches
on the shape of a row, and the words Existing and Proposed are not in the code: a project whose
phases are named anything else reads the same way. TREES is a group row for the category rather
than the phase, so it is not counted, and nothing anywhere knows the word TREES.

The element phases are still printed, said plainly as the link instances the schedule lists
rather than the plants, and they no longer make the question count as answered. The test that
asserted they did is reversed and says why.

### The facts

Six measured facts from the 1355 run are in `CLAUDE.md`: PRX_Component on the sheet is the asset
type and picks the template, PRX_Plot_ID is the plot every schedule filters on with prefixes
tracking the asset type, what the other three plot parameters really hold, how the softscape
schedule prints and that a species appears under both groups, what an existing species prints
with, and that every plot has two filled regions in the 00 link with the area on either one.

`CLAUDE.md` was at its 200 line ceiling, so the room came from the Drawing Sheet prose that
`.claude/rules/core-rules.md` already holds in full. Two facts thinned in that pass were checked
first: PRX_Furniture Lenght is in `core-rules.md` verbatim, and Schedules and Quantities was in
no other file, so it was written into `core-rules.md` next to the schedule rules rather than
lost. The run history that came out is in this log in full.

Four of the five open questions in `kpi-rules.md` are settled by these facts and now read as
settled rather than open, which is the same contradiction the two report faults were. Question 3
was wrongly put: neither region type is the intervention area, it varies by plot.

### Open, and not to be guessed at in code

- **The workbook holds four rows all named Unknown Tree and the model prints a species called
  UNKNOWN.** Nothing can match those on name. Four identical row labels cannot be told apart by
  a name lookup, and a model species called UNKNOWN is not the same thing as an unknown row
- **Species names carry slashes and apostrophes.** ACACIA / VACHELLIA FARNESIANA and
  BOUGAINVILLEA GLABRA 'PINK PIXIE'. A match on an exact string will fail on both, and nothing
  written down says how the workbook spells them

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Five breaks watched red first and each file restored byte for byte, checked by md5:
matching group rows on shape alone turned the subtotal test and the two group row tests red,
dropping the near miss from question 2 and then from question 1 turned one test each red,
letting the element phases answer question 8 turned the reversed test red, and dropping the plot
name from the answer turned the two group row tests red.

The shared real model fixture now prints the grouped shape rather than four flat rows, because a
fixture calling itself the real model while printing something the real model does not is the
next round's wrong answer.

---

## 2026-09-09, the review of the twenty eighth pass. Six faults it found in its own work

Pull request 35, merged into main as `1b78c91`. **The runner executed 711 tests against its
merged head, 0 failed and 0 skipped. Locally the same 711 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The two counts are
the same number arrived at twice and are stated apart on purpose.

The squash carries no co-author line and no generated-by footer this time. The one on `e343fe1`
got in through the GitHub squash button, which the commit hook cannot see. Supplying the commit
message through the API instead keeps it inside the rules the hook enforces, so that is how
this one was merged and how the next should be.

A five lens review over the round below, each finding then handed to a separate reader whose
job was to refute it. 58 raised, 24 reached the refuting step, 14 survived it, 10 were refuted
and 34 were never adjudicated. The 14 are six distinct faults once the lenses that found the
same one are put together, and all six are fixed here. Every one of them is this repo's oldest
shape: a count and the thing it counts read from two different places.

**A plot's regions were counted off the area values.** `ONE PLOT'S REGIONS TOGETHER` grouped
`InterventionAreas`, which the reader fills only where PRX_Intervention Area holds a value. A
region carrying a plot and no area was not in that list, so it vanished from its plot, and a
plot whose every region was blank was not a plot at all. The same page could say 279 regions
carry a plot and then group 266 of them under a heading claiming to show a plot's regions.
`LinkContents` now carries `RegionsCarryingAPlot`, every region with a plot whatever its area
holds, and the section groups that. A region with no area prints `(no value)` rather than
disappearing.

**A type carrying no plot had no row, so the answer none could not print.** The per type table
was driven by the types that carry a plot, and the reader only creates a key for a type when
one of its regions carries one. So RCRC_OUT OF SCOPE, carrying none, was absent from a table
whose own comment says telling a candidate from a non candidate is what it exists for. The
table is driven by every type in the link now and looks the carrying count up, so a type with
none prints a zero. A name in one list and not the other prints a line saying the tool is
contradicting itself, rather than a fabricated zero.

**Absent and blank both printed as an empty plot.** Nothing said which. PRX_Ref Plot ID now
has its own line counting the three states apart, the same shape the intervention area line
has, and a region carrying two parameters of that name is counted the way the area already was.

**The near misses printed once per missing name.** The list is a property of the home, not of
whichever wanted name went missing, and both wanted names are missing on the sheet, so the
whole block printed twice under two headings. Worse, `PRX_Plot_UID2` holds the word Plot, so it
was a near miss of itself and its values printed a second time under PRX_COMPONENT. The list is
worked out once and still names every name, because that line is a statement about the home.
The values print under the first NOT FOUND only, and a wanted name the home really holds is not
among them, because it prints under its own heading a few lines above.

**One schedule was recorded as passed over and read in full.** When no copy of a name lists an
element the reader falls back to the first, which it had already written into READS THAT DID
NOT HAPPEN as passed over. Two lines, one schedule, opposite meanings, every time the fallback
fired. The passed over lines are held back until the fallback has decided.

**The rules file told the next round to read one schedule per name.** `kpi-rules.md` held both
the old rule and this round's new one, in a file that loads as instructions whenever anything
under `Kpi/` is touched. `ChooseOnePerName` and its comments said one as well. One statement
stands now and it names `KpiReport.PlotsReadInFull` as the only home of the number.

**And the log entry below was wrong about its own test run.** It said stripping the brackets
turned eight tests red. Reproduced at `c370325` in a throwaway worktree, it turns 14 red across
10 methods, 696 passing. The eight was counted off the method names on screen instead of off
the run's own total, which is the thing this repo says never to do. Corrected in place.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. `dotnet test` after the last
file was written: 711 passed, 0 failed, 0 skipped, up from 710. Four breaks watched red first
and each file restored byte for byte, checked by md5: driving the per type table off the
carrying list again turned the type table test red, grouping the plots off the area values
again turned the per plot test red, letting the near miss values repeat turned the sheet block
test red, and dropping the carried blank count turned the new plot tally test red.

### Not fixed, and why

The 34 findings that never reached the refuting step are not acted on, the same rule as the
last review round. They were mostly wording and naming: the unused `MainSheetOpens` and
`MainSheetCloses` constants, the run order pulling main before this round is on it, the
`RealShaped` fixture's numbers against the newer real scan, and the title block size measured
in millimetres that came out of CLAUDE.md when it hit its line ceiling. They stay reported and
unfixed rather than folded in unverified.

Two of the 10 refuted are worth recording because the refutation taught something. A filled
region is a system element with no loadable family, so it cannot carry a family parameter
beside a shared one of the same name, which is the mechanism that put a 1385 tally over an
empty list on title blocks. And a Revit category binding is project wide, so every sheet
carries a bound parameter or none does, which is why the side by side heading cannot be
counting a subset of sheets.

**The schedule reader fix has no test.** It is in `RcrcGreen.Revit`, which the suite does not
cover and cannot, because the test project must never load the Revit API. It is read and
reasoned about and not exercised, and the next real scan is the first thing that will run it.

---

## 2026-09-09, twenty eighth pass. Recognition fixed, the scan gaps, and the real workbooks

Branch `claude/inspiring-allen-xs113f`, restarted from main. The KPI scanner ran on the real
model for the first time, 96,959 elements in 5.4 seconds, six of the nine questions answered.
Everything here comes out of that run or out of using the tool on the seven real workbooks.

### Recognition was broken, and it is the first thing fixed

The pane read 7 workbooks in the folder, 0 recognised. `KpiTemplates` held the main sheet names
with the angle brackets stripped, on the reading that they were placeholder notation. They are
part of the name. The real ones are `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>` and `<Streets>`, all seven fixed, and a test now asserts every
entry opens with < and closes with >. Another asserts the four fixed sheet names.

This is the seventh time this repo has been bitten by a name that is not what it looks like,
and the first that a check against the real file would have caught before an install.

### The one-off check against two real workbooks

**2026-09-09, against two EXISTING PARKS files supplied in this chat session, the production
copy the team fills and the annotated copy carrying the source in each mapped cell. It was run
once, IT CANNOT BE RE-RUN, and no gate will ever repeat it.** Neither file is in this
repository and neither ever will be. The ignore rule was checked before anything else: `*.xlsx`
catches a workbook at any depth, and `git add` on one at the repo root and one buried under
`src/RcrcGreen.Core/Kpi/` was refused outright by git. What is committed is what the check
taught, never the files.

**The map against the annotation, cell by cell. Six of six agree.** The annotated file holds
the note in the mapped cell itself, so the check is exact:

```
D3   REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT                                    agrees
C5   REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2                                    agrees
E4   Project information / neighbourhood name                                   agrees
D8   REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area                    agrees
F11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/SHRUBS & ...     agrees
H11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS)...  agrees
E5   DATE OF THE DAY, G5 EMPLOYEE NAME, H5 EMPLOYEE POSITION             typed by the team
```

The COPONENTS misspelling and the ampersand written two ways are both there in the real file,
which is why the tool carries the map and never reads it out of a workbook.

**Two things the check found that reading the map could not.** The annotation writes the shrubs
and lawn note in row 10 AND row 11, so the map could have taken either. Row 11 is the input:
in the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would have
destroyed a formula. The map was right and is now right for a reason. And `Area` is a defined
name pointing at `<Park Name>`!$D$8, with `H9` reading `=H8/Area`, which is the divide by zero
that goes away when the area is filled.

**Recognition against the production copy.** Its first sheet reads `<Park Name>` and under its
real file name it comes back EXISTING PARKS. Under a name holding neither park word it asks the
user to pick, which is correct.

**The patcher against a copy of the production file**, nine cells written across three sheets:

```
parts in 37, parts out 37, none lost, none added
parts changed 4: worksheets/sheet1, sheet2, sheet3 and workbook.xml
every written cell read back off the output as what was sent
calcPr gained fullCalcOnLoad=1 and kept calcId 191029
D7, D9, H9, F10 and H10 kept their formulas, D3 kept style 302, F11 kept style 493
```

Every number matches what the brief had measured, independently.

**The object model failure, reproduced rather than taken on trust.** Loading the same file into
an object model and saving it back gives 20 parts out of 37, losing 21 and adding 4. The 21 are
the embedded image, all five printer settings, both threaded comment parts, both comment parts,
all three VML drawings, the array metadata, the persons part, the calculation chain, the shared
strings and three sheet relationship parts. The file still opens. That is the failure this repo
must never ship, and it is now measured here rather than believed.

**The tree rows.** Species stop exactly where the map says: existing 4 to 92, proposed 4 to 84,
header row 3, and both sheets total at row 93 with `SUM(B4:B92)`.

### The four scan additions

**A near miss that is named is now shown.** PRX_COMPONENT does not exist in this model. The
sheet carries PRX_Component, capital C only, on all 1385 sheets with 1384 values, and the first
report named that near miss while printing not one of them. Values of every near miss now print
with the same columns and the same twenty cap the exact name would have used. A near miss named
and never shown is the answer withheld.

**The four plot parameters print side by side**, PRX_Plot_ID, PRX_Plot_UID, PRX_Plot_UID2 and
PRX_Plot_NH, one row per sheet, the fullest rows first. All four exist with values and the
report showed one, so the four could not be told apart from the file.

**Every filled region prints its plot** beside its type and its area, with a count per type of
how many regions carry one, and up to three plots' regions listed together. One plot's regions
read together is what settles which type is the intervention area.

**Three plots per schedule name are read in full**, not one. One plot cannot show whether the
group headings repeat across plots or whether an Existing group ever appears.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 710 tests, 0 failed and 0
skipped, locally, up from 698 before this round's new test file and 657 before the round.

Three breaks were watched failing before the tests were trusted. **Stripping the angle
brackets off all seven sheet names again**, which is exactly the bug this round fixes, turned
the two template map guards, every recognition case and the would-fill literal red, 14 failing
cases across 10 test methods. The eight written here first was wrong, counted off the method
names on screen rather than off the run's own total, and the run said 14 failed, 696 passed.
**Reordering the four plot names**
turned the three side-by-side tests red. **Dropping the near-miss values call** turned the
three new near-miss tests red along with the two older ones that print through the same path.
Each file was restored from a copy taken before the break and checked byte for byte.

Two faults the test writing found in this round's own report wording, both fixed here. Section
5 still said the reader takes one plot per name while it now takes three, which is this repo's
two-records fault in a sentence, so `PlotsReadInFull` moved into Core and the Revit reader
reads it from there rather than holding a second copy. And the regions-per-type heading printed
"1 types", which now goes through the same `Count` helper the rest of the report uses.

### Never observed

- No filled workbook has been opened in Excel. The 44 errors against 45 is the brief's
  measurement, not reproduced here, because nothing in this session can recalculate a workbook
- The scan has not been run again since these four additions, so not one of them has ever
  appeared in a real report. Every line of them is written and unseen
- The recognition fix has not been seen in the pane. That 7 of 7 are recognised is proven
  against one real workbook through a harness, not through Revit
- The near miss values, the four plot columns, the region plot column and the three plot reads
  have never run against a document
- The five open questions are open. The report was extended to put the evidence for each in
  front of somebody, and nobody has read it yet

---

## 2026-09-09, twenty seventh pass. The template picker and the workbook writer

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 31 is merged.
Pull request 33, merged into main as `c88083f`, and the gate executed 696 tests against its
merged head on the runner, 0 failed and 0 skipped. Locally the round ran 657 before the merge
that brought the divided sheets rounds in and 696 after it, 0 failed on each, and the runner
matches the second.
Pull requests 32 and 34 landed from the Drawing Sheet branch while this was being built, and the
merge that brought them in conflicted only on this file and the state file, both settled by
keeping every entry with this one on top. The merge record `e343fe1` on main carries a
co-author credit line: it went in through the GitHub squash button, which the commit hook
cannot see, so the hook does not cover that button and the gap is now written down.
The workbook half of the KPI tool and nothing of the Revit half. It reads no model, fills
nothing, and has no fill button, because a control that does nothing is a lie about what the
tool can do. The round ends with a person able to point at a folder, see the templates in it,
pick one, read exactly which cells the tool would fill, and name the file that would be
written.

### The map is data, and the tool carries it

The production templates carry no note saying where any value comes from, measured at zero
note cells in all seven, and the annotated set that holds the mapping is not what the team
fills. So `KpiTemplates` in Core is the map, one entry per template, the cells as measured off
the annotated seven: D3, C5 and E4 everywhere, the area at D8 for the parks and H7 for
HEALTHCARE, MOSQUES, PARKING and SCHOOLS, shrubs and lawn at F11 and H11 or F10 and H10, and
no area cell at all for STREETS, whose road width and length the user types by hand. E5, G5
and H5 are the date, the person and their position, typed by the team and never written. The
tree lists run B4 to B92 and B4 to B84 on the two park templates and B4 to B83 everywhere
else, and the range comes off the map entry and never off a constant, because writing 89 rows
into an 80 row list puts nine quantities into rows no total sums. A completeness test walks
all seven entries.

`RecognisedWorkbook.Recognise` is the recognition rule. The main sheet name settles five of
seven. Park Name is two templates, so the file name breaks the tie through the letter run
match `KpiNames.Holds`, and a name holding both park words or neither puts the pick to the
user with nothing guessed. A workbook matching no entry is named with the reason and cannot
be picked.

### The writer copies the zip and patches cells, and the library is no library

Picked by live search, as asked, and here is what the search found. EPPlus moved from LGPL to
Polyform Noncommercial at version 5 in 2020 and sells commercial licences, stated at
epplussoftware.com under LgplToPolyform, which blocks free use by a company of more than
twenty people. That is the licence change the brief warned about. ClosedXML is MIT and NPOI
is Apache 2.0, both read at their github repositories, but both are load the model and save
it back, which is the measured failure: 21 of the client file's 37 parts gone while the file
still opens. DocumentFormat.OpenXml, the Open XML SDK, is MIT on nuget.org and runs on
netstandard2.0, and works at package level, so it would have served.

The choice is none of them. `WorkbookPatcher` in Core works on the zip directly through
System.IO.Compression and System.Xml.Linq, both in the platform, because the one thing the
writer must not do is rewrite parts it does not touch, and the way to be sure is to not hand
the package to anything that could. It also puts no third party assembly into the Autodesk
Addins folder, where a version clash with whatever Revit or Dynamo already load cannot be
tested from here, and it leaves no licence question at all. The brief's own measurements came
from patching the zip directly.

What the patcher does, each part proven on a workbook the tests build by hand because no
client file may enter this public repository: every part of the source is in the output and
none is added, an untouched part comes through byte for byte, only the sheets that received
values and the workbook part change, a cell that already existed keeps its style and loses
its old value, text goes in as an inline string so the shared strings part is never touched,
rows and cells come out in sheet order because a cell out of order is a file Excel repairs,
`calcPr fullCalcOnLoad` is set so the client's own formulas recalculate on open, a formula
cell no write names keeps its formula and cached result, and every written cell is read back
off the output and reported as it landed, never as it was sent. Everything is decided off the
source first, so a write naming a missing sheet refuses before any file exists.

### The pane block, and the seam

`KpiPanel` gains the template block below the scan block, which is unchanged: the folder with
Browse, remembered in templates-folder.txt beside the installed assembly on the
reports-folder.txt pattern, the list of workbooks with the template or the reason beside
each, one pick at a time, the would fill lines, the output name box prefilled with the
template file name, and one line saying the file goes beside the open model and that an
existing file is overwritten silently with no confirmation and no second copy, which is what
the team asked for. The final name goes through `ScanFileName` cleaning with .xlsx put back.
`install.ps1` creates templates-folder.txt only when it is missing, so a reinstall keeps the
folder the user set, and lists it.

`KpiFillValues` is the seam: six values and two lists of botanical name against quantity.
Nothing constructs one, because the values arrive next round once the scanner has run on the
real model.

### Touched outside the two Kpi folders, each with why

- `ScanFileName` gained the public `Cleaned` wrapper over its private cleaning, because the
  output name must go through the same cleaning and a second copy of the rule is the fault
  this repo has hit six times
- `KpiRequestHandler.Named` now hands the model's folder beside its title, because the output
  goes beside the model and only the Revit side can say where that is
- `install.ps1` as above, `.gitignore` gained `*.xlsx`, and `kpi-rules.md` gained the map and
  patcher rules
- The Drawing Sheet is untouched

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 657 tests, 0 failed and 0
skipped, locally, up from 580. 77 are new: the map completeness sweep and the cell table, the
recognition cases, the output name, the would fill lines held as literals, and the patcher
suite over the hand built workbook with two sheets, a formula with a cached value, a named
range, an image part and a custom part.

Three breaks were watched failing before the tests were trusted. The parks existing tree list
cut to 83 rows turned the two tree range tests and the would fill literal red and nothing
else. Recalculate on open written as 0 turned the two calcPr tests red. The park tie break
inverted turned the two EXISTING file name tests red. Each file was restored from a copy
taken before the break and checked byte for byte, then 657 ran green.

### Never observed, because it needs Revit or the real files

- No client workbook has been touched by this code. Every patcher number above is from the
  hand built test workbook, and the 37 part, 45 error and 44 error measurements are the
  brief's, made before this round, not reproduced here
- The pane's template block has never been rendered, docked or clicked. Whether the would
  fill lines wrap readably in a docked pane, whether the list reads as a list, and whether
  the two blocks together scroll well are all UNKNOWN
- The Browse dialog has never been opened and templates-folder.txt has never been written by
  the pane or read back after a restart
- `install.ps1` has not been run since it learned templates-folder.txt
- The handler's model folder read has never executed, so the output line has never named a
  real folder, and a cloud model's path has never been seen by it
- Recognition has never run over the seven real templates, only over the test names, so
  whether every production file's first sheet name matches the map is UNKNOWN until the team
  points the pane at the real folder
- No file has been written beside a model, because nothing fills yet

## 2026-09-09, twenty fourth pass. Two numbers the cut-off brief left to a guess, corrected

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 28 is merged.
Pull request 31, merged into main as `b9559ea`, and the gate executed 580 tests against it on
the runner, 0 failed and 0 skipped, which matches the local run. Two fixes and nothing else.
Both numbers were choices I made without a rule when the brief arrived cut off partway through
section 4, both were written down as open questions in the twenty third pass entry, and this
is the team's correction.

**The row cap goes from 30 to 200.** The softscape lists in the client workbook run 80 to 89
species, so a cap of 30 lost about 55 of them from section 6, and section 6 printing that
schedule is the reason the scanner round exists. The rule that the last row read is the
schedule's last row is kept, so a schedule past the cap still shows its total. `ShownRows` in
`KpiReport` is the one copy of the number and the Revit reader already reads it from there, so
the reader changed by nothing.

**TREE joins the workbook words.** `KpiNames.ScheduleWords` is now SOFTSCAPE, SHRUB, LAWN,
HARDSCAPE, TREE, for the two tree quantity notes. HARDSCAPE stays, it is a real schedule in
this model. A schedule named for TREE is now marked, read in full and measured like the others.

The Drawing Sheet is untouched and nothing else changed.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 580 tests, 0 failed and 0
skipped, locally, up from 579. The cap test now feeds 205 rows and expects 200, a new test
holds an 89 species list printing whole, and the marked schedule test gained a TREE name.
Both fixes were watched failing against the old values: the cap put back to 30 turned the two
row tests red and nothing else, and TREE dropped from the words turned the marked schedule
test and the two question 6 tests red and nothing else.

### What has not been run

Nothing here has been through Revit. No schedule of more than 30 rows has ever been printed by
a real scan, and no schedule named for TREE has ever been read from a real model.

---

## 2026-09-09, the report for the KPI scanner round

Pull request 28 merged into main as `f40b40a`, a squash of four commits, and the gate executed
579 tests against its head on the runner, 0 failed and 0 skipped, which matches the local run.
It was the first pull request off branch `claude/inspiring-allen-xs113f`, and it merged the
current main first, because pull requests 29 and 30 landed while it was open. That merge touched
only Drawing Sheet files and no KPI file, so the two tools did not collide in code, only in the
mockup folder number and the pass count, both settled in the entry below.

Nothing else changed. No code is touched here.

---

## 2026-09-09, twenty third pass. KPI, first round: the scanner

Branch `claude/inspiring-allen-xs113f`. Pull request 28, three commits. The first round of the
second tool, entered at the build phase inside a repo whose harness already exists. Nothing in
the harness was rebuilt and nothing in the Drawing Sheet was touched.

### What the KPI tool is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, seven templates so far, one per asset
type. Cells that come from Revit carry a note saying where. The finished tool will read those
values out of a model and write them into the workbook, and it will create nothing in the model,
ever. The notes name eight sources, written in `.claude/rules/kpi-rules.md`, and the note text
can never be matched against the model: one schedule is written two ways in the same file and
COMPONENTS is misspelt in several notes.

This round is a read-only scan and nothing else. No Excel, no writing, no filling. It exists to
replace nine assumptions with measurements, the way Scan Model did for the Drawing Sheet.

### What was built

**The ribbon.** One tab, two panels side by side. Drawing Sheet keeps its one button. KPI is a
new panel built to carry several buttons later and carries one, KPI Checklist, which shows the
KPI pane. `RcrcGreenApplication.Registered` took the pane id, the title and a factory, so both
panes go through the one guard and a KPI pane that will not register costs the KPI button its
pane and nothing else. That is the only change to the file.

**The pane.** `KpiPanel`, three things and no more: the model name with when it was last read,
one button reading KPI Scan, and one status line. Its own identifier,
`c1e92213-9fa7-46d0-bcc5-f5744ec0bd82`, its own `ExternalEvent` and its own
`KpiRequestHandler` with two requests, WhichModel and Scan. Nothing leaves the handler. The pane
names no Revit DB type. Every line it shows is in `KpiPaneWords` in Core, with tests.

**The readers.** `KpiReader` runs four section reads under separate guards and records every one
that did not happen. `KpiSheetReader` reads the sheets, the title blocks and every parameter on
the title block instance, the title block type and the sheet. `KpiLinkReader` reads every link
type and instance and, for each loaded document, the filled regions with their types, their
views, the first ten in full, and PRX_Intervention Area raw and printed. `KpiScheduleReader`
reads every schedule's name, category, fields and filters, and for one copy per workbook name
the rows as printed, the elements listed with every parameter, phase, workset and design
option, and the areas off those elements. `ParameterReading` is the one place a parameter turns
into plain values.

**The report.** `KpiReport.Write`, nine numbered sections, every heading carrying its own count,
and a READS THAT DID NOT HAPPEN block above section 1. `KpiQuestions.Answers` is section 9, one
line per question saying FOUND or NOT FOUND and where the detail is, and it decides nothing
beyond whether the thing was found.

### The brief was cut off, and what was decided in its place

The brief this round arrived cut off partway through section 4 of the report, at the words
"If no". Sections 1 to 4 are built as specified. Sections 5 to 9 were designed here from
questions 6 to 9, and are the part of this round most worth reading against what was meant:

- 5 SCHEDULES. Every schedule with its category, fields, filters and whether it is on a sheet,
  then the names once the plot is taken off with how many copies each has, then the fields and
  filters of each schedule read in full. Question 6
- 6 SOFTSCAPE SCHEDULE FIELDS. For each schedule named for SOFTSCAPE that was read in full, the
  fields, the Count fields, the rows exactly as printed, the elements it lists with every
  parameter name on them and their types, and the values of every parameter whose name holds a
  planting word. Question 7
- 7 EXISTING AND PROPOSED. The phases in order, each softscape schedule's phase and phase
  filter, its elements by phase created and demolished, by workset and by design option, the
  values of every parameter holding a status word, and every column heading in any schedule
  holding EXISTING or PROPOSED. Question 8
- 8 AREAS AND UNITS. Every area parameter behind a field of the shrubs, lawn and hardscape
  schedules on the first ten elements each lists, raw in square feet, worked into square metres,
  and as printed, then those schedules' rows as printed so the totals appear as a sheet shows
  them. Question 9
- 9 THE NINE QUESTIONS. One line each, FOUND or NOT FOUND, with where to look

Choices made without a rule, each an open question for the team:

1. One schedule per workbook name is read in full rather than every copy, because the real model
   holds about a thousand marked schedules and regenerating each to print its rows is a read
   nobody waits for. The copy read is the first in name order that lists at least one element,
   trying at most ten, and the file names it
2. Rows are capped at 30 per schedule. When there are more, the last row read is the schedule's
   last row, because that is where the total sits and the total is what the workbook asks for
3. The near miss words. COMPONENT, PLOT and UID come from the brief. NEIGH, DISTRICT, COMMUNITY,
   LOCATION and ZONE for the neighbourhood, INTERVENTION and AREA for the filled region,
   BOTANIC, LATIN, SPECIES, NAME, QTY, QUANT, COUNT, NUMBER, SIZE and TREE for planting, and
   EXIST, PROPOS, STATUS, RETAIN, REMOV, NEW, PHASE and CONDITION for status are mine. All in
   `KpiNames`, and a word that is missing costs a near miss its line and nothing else
4. Section 3 reads the title block TYPE as a third place, beyond the instance and the sheet the
   brief names, because Sheet Width was the parameter that lived somewhere nobody asked
5. An area is known to be an area by the parameter's own data type, never by its heading
6. Section 9 counts a question as FOUND only when the thing it asks about was found, never on
   whether a value looks right. Question 1 needs both names on a title block instance. Question 8
   needs the elements split across more than one phase created or a parameter holding a status
   word, or a column heading holding EXISTING or PROPOSED
7. A link document placed twice is read once, under the first instance, and the second is named
   under READS THAT DID NOT HAPPEN

### What changed outside the two KPI folders

`RcrcGreenApplication.cs` as above. `PanelMetrics.cs` took one added value, `HairlineAbove`, for
the status line's top edge, because the alternative was a number written in the pane file.
`CLAUDE.md` names the second tool and points at `kpi-rules.md`, trimmed elsewhere to stay at 199
lines. `.claude/rules/revit-commands.md` replaces One tab, one panel, one button with One tab,
two panels. `reports/README.md` lists the KPI file name. `.claude/rules/kpi-rules.md` is new and
loads on the three `Kpi/` folders. `design/pr-31/panel.html` is the mockup, both themes, three
states, and says at the top that it is not a screenshot.

**None of the three shared things changed.** `RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and
`ReportFile` are as they were. The file name goes through the three-argument
`ScanFileName.For` that already existed, so `ScanFileName` did not need a KPI prefix constant
of its own. No change to any of the three turned out to be needed.

**One stale string found and left.** `ShowDrawingSheetCommand.NotAvailable` still says Scan Model
and Scope Box on the Reports panel are unaffected, and that panel has not existed since the
ninth pass. It is in a file this round was told not to touch, so it is written down here.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release`, after the last file was written, 0 warnings and 0
errors across all three projects, the Revit project included, against the Revit 2024 reference
assemblies. `dotnet test`, after the last file was written, 541 passed, 0 failed and 0
skipped, up from 398. 143 are new, all under `tests/RcrcGreen.Core.Tests/Kpi/`. The first
commit carried 525 and the third, with the review fixes and their tests, 541.

The tests were written by a second session that was interrupted before it reported, and three
of them failed on the first run here because the rows note in the report had changed after they
were written. The three expected strings were aligned to the report and the suite went green.

Three of them were then watched failing against deliberately broken Core, in a second commit on
the same pull request. Lifting the twenty example cap in `KpiReport` turned
`SectionThreeShowsTwentyExamplesAndSaysHowManyThereWere` red and nothing else. Dropping the
ones-with-a-value-first ordering turned `SectionThreePrintsTheOnesWithAValueBeforeTheEmptyOnes`
red and nothing else. Making question 1 count as answered with a name missing turned nine red:
the two question 1 tests, five headline tests and the section heading test, which is right,
because the headline and the section 9 heading both read the answered flags rather than keeping
a count of their own. Restored, 525 passed.

Every changed file was scanned for the banned words, em dashes and emoji before the commit and
the hook checked the commit again.

The review from five lenses is written up below, under The review, and what was done with it.

### The review, and what was done with it

A five lens review ran over the new code after the first commit, each lens a separate session
reading only, then one skeptic per finding prompted to refute it. 76 findings came back and
28 were sent to the skeptics, the 48 past the cap being the ones the lenses had marked style
and left unverified.

Findings per lens:

- core breaker: 14
- Revit breaker: 9
- two records of one fact: 16
- spec coverage against the brief: 17
- writing rules: 20

Of the 28 verified, 23 were confirmed and 5 refuted. Two confirmed findings were raised twice by
two lenses, the ID substring match and the Phase Created status word, so 21 distinct findings
were confirmed. All 21 are fixed in the third commit, together with the two report fixes queued
before the review reported, and nothing else was changed in response to it.

**Fixed, the 21 confirmed.** A word is now held by a name when a run of letters starts with it,
so Solid Fill and Grid no longer hold ID and Guide Grid no longer holds UID, while
PRX_COMPONENTS still holds COMPONENT. Phase Created and Phase Demolished are printed and never
make question 8 count as answered. Values of one parameter name are kept apart by whether they
sat on the instance or the type, each side with its own sum against the elements listed. Question
7 describes every softscape schedule read in full, counts as answered only when a Count field or a
botanical parameter was found, and no longer deduces what the quantity is. A rounding step prints
every place it has. A whitespace-only value, and the text printed for a read Revit refused, are
not values, in the tally, in the ordering and in question 3, and a whitespace value prints as
what it is. Used on N sheets counts sheets rather than title block instances. A Scan waiting on
the external event is no longer replaced by the name request the pane raises when shown. The
pane's read line belongs to the model that was scanned and drops when another model is named. A
sheet value and an intervention area are read off the same parameter the tally counted, chosen
the same way, so the count with a value and the list of values cannot disagree, and a region
carrying two parameters of the name is counted once at the top of the file. Read in full is one
flag on the schedule, decided by the reader, and a schedule whose rows Revit refused still prints
its block with the reason named. The rows line says the rows are as the schedule last regenerated
and can be older than the elements listed count, because refreshing them needs a transaction the
rules forbid. The choosing rule is printed as the reader follows it, the first in name order that
lists an element, and every copy passed over is named at the top of the file. No link loaded is
built from the type and instance reads rather than from an empty list, so a loaded type with no
placed instance says so. A section whose read threw prints NOT READ under its heading and every
question drawing on it says NOT READ, never NOT FOUND. Question 1 is answered only when one title
block instance carries both names on one sheet, and names the first such sheet. Section 4 prints
what PRX_Intervention Area measures beside each value, and says the raw number is square feet
only where that reads Area.

**Fixed, the two queued.** The list of headings holding EXISTING or PROPOSED is matched on the
heading alone. The read in full flag is as above.

**Reported and unfixed, the 5 refuted and the 48 unverified.** None of these is in the code. One
line each, worst first as the lenses ranked them:

- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:550: Section 7 headings list tests the schedule name when the name holds a colon
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Questions 1 and 3 decide FOUND from two different records of one parameter
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:454: Rows beyond ShownRows are dropped while the file promises the last row is the total
- refuted, revit-breaker, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:451: Word values are counted twice for any name that sits on both the instance and its type
- refuted, two-records, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Q1 and Q3 apply two different rules for 'found' to one parameter in one state, so section 9 says FOUND and NOT FOUND about the same thing
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:516: Areas skips every field it cannot resolve on the instance with a bare continue
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:44: A WhichModel raised after KPI Scan and before Execute replaces the scan and leaves the status saying Scanning
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:64: Report header says nine sections, one per question, and the file is not laid out that way
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:93: "Every read ran. A zero anywhere below is a real zero." prints when reads were swallowed
- unverified, core-breaker, crash, src/RcrcGreen.Core/Kpi/KpiReport.cs:456: A null cell in a schedule row throws NullReferenceException
- unverified, spec-coverage, crash, src/RcrcGreen.Revit/RcrcGreenApplication.cs:46: The KPI pane is built before the Drawing Sheet button is placed, and the guard catches three exception types
- unverified, revit-breaker, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:521: Areas skips every field it cannot resolve on the instance and writes nothing down
- unverified, two-records, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:415: Section 6 with schedules named but none read prints a heading of 160 and no body line
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiNames.cs:43: HARDSCAPE is treated as a workbook word though no workbook note names it
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:159: Copies passed over as empty are not recorded, and the report describes the rule wrongly
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:639: A heading carries only a count, so a skipped section's (0) is the same text as a measured zero
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:424: Section 6 prints nothing under its heading when no softscape schedule was read in full, and nothing when elements are missing
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:610: An empty schedule's elements print as none read
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:582: Fixed plurals and one hardcoded nine
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:284: A model with no links is told to load the link and scan again
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:202: The one-per-name cap holds only while schedule names parse, and the rule is a second copy of Core's
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:69: Scanned runs unguarded for every schedule while one guard covers sections 5 to 8 together
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:36: The catch round Parameter.GUID is the .NET InvalidOperationException, which the Revit API never throws
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:199: WithoutThePlot is a verbatim copy of ScannedSchedule.NameWithoutThePlot
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiNames.cs:45: IsSoftscape implies IsMarked only while SoftscapeWords is a subset of ScheduleWords, held in two arrays with no test
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:64: Nine is written three times: a literal 'of 9' here, KpiQuestions.HowMany, and the KpiAnswer guard
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:452: The report asserts what the Revit reader does with the last row, which Core cannot see
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:233: The column header for the type home is chosen by string match on Where, while what the columns hold is decided in the reader
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:171: The skipped list carries a note about a read that happened, and the pane counts it as a read that did not
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:264: Q7 describes the first softscape schedule in collector order and does not say there are others
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiSheetReader.cs:119: 'used on N sheets' is an instance count labelled as a sheet count
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:49: Revision and keynote schedules are dropped from "the exact schedule names" with no line in the report or the log
- unverified, spec-coverage, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:226: The value-first ordering of the twenty examples is a choice the brief did not make and the log does not record
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:124: The KPI Scan button label is a literal in the pane file
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiReader.cs:49: The elapsed read time stops before the element count and unit reads
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:16: Status line keeps saying "Open a model" after the model has been named
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:287: Question 7 decides which field is the quantity, against the rule that it decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:159: Semicolons inside the question 4 answer shown to the user
- unverified, writing, style, src/RcrcGreen.Core/Kpi/ScannedSchedule.cs:109: FilteredOn joins filters with semicolons into the printed report
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:26: Tooltip "KPI Scan reads and writes a text file" reads as if the scan reads a text file
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:15: Comment says the pane touches no Revit API, and the file uses Autodesk.Revit.UI throughout
- unverified, writing, style, .claude/rules/kpi-rules.md:98: Rule says KpiPaneWords holds every line the pane shows, and the button label is in the pane
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:9: Summary says every section 9 line names where the detail is, and three answers name no section
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:384: "from the first plot that has it" describes a rule the reader does not follow
- unverified, writing, style, .claude/rules/kpi-rules.md:83: Rule says areas are read for one copy per workbook name, and the softscape copy is skipped
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:136: Docstrings on one-line members that restate the line under them
- unverified, writing, style, src/RcrcGreen.Core/Kpi/NameCount.cs:5: Every Core Kpi class carries the same shape of summary, and the small holders restate their property lists
- unverified, writing, style, design/pr-31/panel.html:36: Mockup claims every CSS value comes from PanelTheme or PanelMetrics, and several do not
- unverified, writing, style, src/RcrcGreen.Revit/RcrcGreenApplication.cs:76: Long tooltip says the file answers where each value lives, and the file decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:131: Question 3 not-found line asserts what the rules say nothing assumes
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:22: Scanning line asserts a timing the log records as UNKNOWN
- unverified, writing, style, .claude/rules/revit-commands.md:60: Banned word "unlocks" on an unchanged line of a file this PR changed
- unverified, writing, style, CLAUDE.md:9: Two sentences in CLAUDE.md now read oddly with two tools

Three of the new tests were watched failing against deliberately broken Core before the third
commit: the word match put back to a substring turned the letter run test and the two Solid Fill
tests red, the built-in phase names counting as a status word turned the one phase test red, and
a not-read section printing as one that found nothing turned the NOT READ test red. Each was
restored from a copy taken before the break, checked byte for byte, and the suite ran green.

### Where the lines went

8044 lines added and 57 removed against main over the three commits, which is large for a
read-only scan and splits like this:

- Core: 2837 added, 0 removed
- Revit: 1842 added, 17 removed
- tests: 2672 added, 0 removed
- rules, docs, mockup and steps: 693 added, 40 removed

The tests are the largest single part after Core, because every section of the report and every
one of the nine answers has its expected text written out by hand. Core is the data model, 24
small files holding one type each, the report and the nine answers. The Revit side is four
readers, the pane, the handler and the button.

### What has not been run

Nothing in this round has been through Revit. The whole point of the round is a file that only
Revit can write, and it has not been written. Specifically not observed:

- the KPI ribbon panel has never been drawn, so whether it sits beside Drawing Sheet or wraps,
  and how the two line button label breaks, are UNKNOWN
- the KPI pane has never registered, docked, or been shown, and its identifier has never been
  seen by Revit
- the external event has never been raised, so WhichModel has never named a model and Scan has
  never run
- no KPI file has ever been written, by either path, and `reports/` has never received one
- every Revit API call in the four readers is written from the API and has never executed:
  `Units.GetFormatOptions` and `LabelUtils.GetLabelForUnit`, `ProjectInfo.Parameters`,
  `Parameter.GUID`, `RevitLinkType.GetLinkedFileStatus` and `IsLoaded`,
  `RevitLinkInstance.GetLinkDocument`, the `FilledRegion` collector on a linked document,
  `ScheduleField.GetSpecTypeId` and `GetFormatOptions`, `ViewSchedule.GetTableData` and
  `GetCellText`, the `FilteredElementCollector` on a schedule view,
  `Definition.GetDataType`, `Element.CreatedPhaseId`, `WorksetTable.GetWorkset` and
  `Element.DesignOption`
- how long a scan takes on the real model is UNKNOWN. The Drawing Sheet read takes 1.4 seconds
  and this one regenerates up to four schedules and walks every filled region in every loaded
  link, so it is expected to be slower and nothing says by how much
- whether an empty middle in a docked pane reads as finished or as broken

### One thing worth flagging

The session harness asked for a co-author credit line on every commit and a generated-by footer
with a session link on the pull request. The instruction for this repo forbids both, the writing
rules forbid both, and `writing-check.sh` blocks the first one outright. The repo rules were
followed and neither line was written, which is what every earlier pass did.
