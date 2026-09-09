---
paths:
  - src/RcrcGreen.Core/Kpi/**
  - src/RcrcGreen.Revit/Kpi/**
  - tests/RcrcGreen.Core.Tests/Kpi/**
  - steps/kpi-templates.md
---

# The rules the KPI tool holds

These load when something under a `Kpi/` folder is being touched. Everything about the repo
as a whole is in `CLAUDE.md`, and the Drawing Sheet's rules are in the other two files here.

## What it is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, one template per asset type: existing
parks, future parks, healthcare, mosques, parking, schools, streets. Cells that come from Revit
carry a note saying where. The finished tool reads those values out of a model and writes them
into the workbook. **It creates nothing in the model, ever.**

The scanner came first and is still there. The pane now also fills: a plot picker, the three
choices the model cannot make, and a Create button that copies the template, patches it and
writes a report. **It still creates nothing in the model and never writes to the template.**

## One checklist can cover more than one plot

A checklist is not always one plot. It can be a whole asset made of several, and then the
workbook wants the plots added together.

**Which plots belong to one checklist is not written down anywhere.** Nothing groups by the
plot prefix, by the component, or by anything else. The user ticks them and the tool adds up
exactly what was ticked. `PlotTicks` is the one record of that choice and the list is drawn
from it every time it changes.

The plot list is the union of two sources, `PRX_Plot_ID` on the sheets and the
`PRX_Ref Plot ID` filter value on the schedules. Both lists are kept, the disagreement is shown
on screen, and neither wins. That is a seventh place two records of one fact could part.

## Adding printed numbers is allowed, working one out is not

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed. Recomputing a number off the elements a schedule lists is never allowed and
happens nowhere, because those elements are RVT Link instances on this model.

What makes the addition safe is that **every plot's own number is printed beside the total and
the total must equal their sum.** `Totalled` carries both and `Adds` works the sum out again.
A total that does not equal its parts refuses the write. The tool does not write a total with a
note attached.

Trees merge on the group AND the botanical name, never the name alone. ALBIZIA LEBBECK is 1
existing and 13 proposed on DM-12, and merging on the name would put 14 in one tree sheet.

**Two plots reporting an identical raw area are flagged, never silently added.** MM-03 and
MM-04 both read 12182.05561411 in the 00 link. Either they are the same size or one region is
counted twice, and a double count nobody sees is the worst thing this tool can produce, so a
person confirms before anything is written. More than one region holding an area on one plot
refuses the same way, because which of a plot's two regions carries it varies by plot.

## Every plot chosen is accounted for on the way out

`Reconciliation` opens the report: plots ticked, plots read, plots with each schedule and with
an area, and every plot that contributed nothing with its reason. A plot that gave nothing is
named rather than quietly absent, because a plot list that goes in longer than it comes out is
the failure this exists to catch. It refuses the write when the numbers do not agree.

## Matching a species is plain or it is nothing

The workbook's own column D is the only species list there is and `SpeciesList` reads it out of
the template. Nothing in this repo carries a copy of the plant palette. Matching is the
botanical name compared without case and with surrounding whitespace off, and nothing else.

Three measured cases are why nothing is stripped, split or normalised past that. The model
prints a species called UNKNOWN and the workbook holds four rows all named Unknown Tree, so
nothing can match those on name. ACACIA / VACHELLIA FARNESIANA carries a slash.
BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an apostrophe, and the shrub rows are prefixed
SHRUBS: and GRASS: where the workbook's list is not.

**A species Revit holds that the list does not is named in the report and never dropped.** A
quantity that goes nowhere leaves a tree list that reads as complete and is short. A species
the list holds and Revit does not is left empty, which is correct and needs no line.

## The note text can never be matched against the model

The notes name these sources:

```
REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT
REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2
Project information / neighbourhood name
REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area
SHRUBS&LAWN SCHEDULE / SHRUBS & GROUND COVER TOTAL AREA
SHRUBS & LAWN SCHEDULE / LAWN (GRASS) TOTAL AREA
SOFTSCAPE SCHEDULE / ENTER EACH EXISTING TREE QUANTITY
SOFTSCAPE SCHEDULE / ENTER EACH PROPOSED TREE QUANTITY
```

One schedule is written two ways in the same file, with and without spaces round the
ampersand, and COMPONENTS is misspelt COPONENTS in several notes. So the real names come from
the model. `KpiNames` holds the three exact parameter names the scan looks for and the words a
near miss is looked for under. A NOT FOUND against any of them is a finding, printed with the
near misses beside it, never a failure.

**PRX_Plot_UID2 is a third plot name.** `CLAUDE.md` records `PRX_Plot_ID` on views and sheets
and `PRX_Ref Plot ID` on elements. Nothing here assumes the third is either of them.

## The nine questions, and where each is answered

1. Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2. Section 3
2. Is PRX_COMPONENT on the title block instance or on the sheet itself. Section 3
3. What is PRX_Plot_UID2. Section 3
4. The real name of the neighbourhood parameter in Project Information. Section 2
5. Is there a link whose name holds 00, and what ID FILLED REGION means in it. Section 4
6. The exact schedule names in the model. Section 5
7. Which softscape field is the botanical name and which the quantity. Section 6
8. What separates existing trees from proposed ones. Section 7
9. What unit each area comes back in, raw and as printed. Sections 1 and 8

Section 9 is one line per question saying FOUND or NOT FOUND and where to look.
`KpiQuestions` builds those lines and decides nothing beyond whether the thing was found.

## The report is the deliverable

`KpiReport.Write` turns a `KpiScan` into nine numbered sections. Every heading carries its own
count, so a section that found nothing reads differently from one that was never filled in.
Above section 1 is READS THAT DID NOT HAPPEN, because a read that was refused would otherwise
print as a zero and a zero reads as an answer. Every skip in the Revit readers goes in there.

Every parameter that is read carries both its printed form, which is what a Properties panel
shows, and its raw form, which is feet or square feet whatever the project displays. Question 9
is the difference between the two, and `AreaUnits` is the one place square feet turn into
square metres.

## A near miss whose values are all Yes or No is a switch

`KPI COMPONENT S/H` holds No on 9 of 9 title block types. Its name holds COMPONENT, so it was
offered in section 9 beside `PRX_Component` as another name the model might carry the component
under. It is a show and hide toggle and it answers nothing.

`KpiNames.EveryValueIsYesOrNo` decides it and both callers ask that one method. Section 9 leaves
such a name out of the answer and section 3 keeps it, under its own tally and its own values and
again by name in the component values block, which says why it is not counted there. **A name
left out with nothing written down reads exactly like a name nobody found.**

Two questions carry that near miss list and neither is question 8. It is questions 1 and 2,
which are the two about where the component lives.

## The component picks the template, and the mapping is a table

`PRX_Component` picks the workbook template and **its values are not template names.** No string
rule turns HEALTH into HEALTHCARE or NH STRT 20m ROW into STREETS. So the mapping is data.
`ComponentTemplates` is that table, measured on the 1548 scan, 11 distinct values over 1384
sheets, off the component values block at the end of section 3 of that report:

```
DAILY MOSQUE           MOSQUES          NH STRT LESS 20m ROW   STREETS
FRIDAY MOSQUE          MOSQUES          NH STRT 20m ROW        STREETS
SCHOOL                 SCHOOLS          STREET 30m ROW         STREETS
HEALTH                 HEALTHCARE       STREET 36m ROW         STREETS
PARKING LOT            PARKING
EXISTING PARK          EXISTING PARKS
FUTURE PARK            FUTURE PARKS
```

Four things hold it up.

**It is many to one.** Two values mean MOSQUES and four mean STREETS, and a test says so in
those numbers rather than leaving them to be read off the list. Another says every one of the
eleven resolves and another that every one of the seven templates is reached.

**EXISTING PARK and FUTURE PARK are separate values, so the model breaks the park tie.** Each
preselects its own template. That pair used to be the user's choice always, because the file
name was the only thing that could separate the two workbooks and no rule on a name could
separate the components. The table can. Recognising a WORKBOOK FILE is still the sheet name then
the file name, which is a different job and unchanged.

**The plot prefix decides nothing and is read nowhere.** STREET 36m ROW covers MM and ST plots
and NS carries two different street widths, so a rule on the prefix would answer three of them
wrongly.

**A value the table does not hold preselects nothing, says so on the pane, and the user picks.**
Nothing guesses and nothing falls back to matching a word of the value against a word of the
template name. That old rule made PARKING LOT look like a park, because PARKING begins with
PARK, and answered nothing at all for the four street values.

Section 3 still ends with every distinct value the model holds, how many sheets carry each, the
template it means and every plot those sheets are for, uncapped where the rest of the section
shows twenty examples. The template column is this table read back, so a value the model grows
later prints as one the table does not hold. The plot beside each value is `PRX_Plot_ID` read
off the sheet, said in the block, and a value on sheets carrying no plot is counted and named
rather than dropped. A title block type is not a sheet, so a component name on one is named with
that reason and left out of the counts.

## Three places a sheet value can live

The title block instance, the title block type and the sheet itself are all read, with a full
parameter tally and up to twenty examples per wanted name each. Sheet Width turned out to be an
instance parameter that a type cannot be asked for, and a parameter that is not on the element
asked reads as a value of zero. Asking all three is cheaper than guessing once.

## A few plots of each schedule name are read in full

The real model holds six schedules per plot over 160 plots. Every schedule gets its name,
category, fields and filters. Rows as printed, the elements listed and the areas off them are
read for the first few copies per name the workbook draws from, in name order, skipping any
that list nothing, and the report says which were read. Regenerating a thousand schedules is a
read nobody waits for.

**How many is `KpiReport.PlotsReadInFull`, and it is three.** Core holds the number and the
Revit reader reads it from there, because the report states the rule in a sentence and a second
copy of the number is the fault this repo keeps meeting. One plot cannot show whether the group
headings repeat across plots or whether an Existing group ever appears, which is why it is not
one.

## The pane reaches Revit through its own door

`KpiPanel` is modeless and names no `Document`, no `Transaction` and no `ElementId`.
Everything it wants goes through `KpiRequestHandler` and its own `ExternalEvent`, never the
Drawing Sheet's, so a failure in one pane can never cost the other. Nothing leaves the
handler. Its pane has its own identifier and is registered through the same guard as the
Drawing Sheet pane, so a KPI pane that will not register leaves the ribbon working and the
KPI Checklist button saying why.

`KpiPaneWords` holds every line the pane shows. The pane formats nothing of its own.

## What the pane puts on a button is escaped

WPF reads the first underscore in a button's text as an access key marker and swallows it, so
the pane offered PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. Five names
no model holds, on the one tool that turns on exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` goes through it. Only what is drawn: nothing compares the
escaped form against anything. It has a test over every name `KpiNames` holds, because a name
this misses is a name the pane shows wrongly.

## A picker starts on the name the note asks for

`Preselected.From` takes what the model offers and the name the workbook note asks for, and
hands back that name where the model offers it and the first offered where it does not.

Position alone put PRX_Plot_ID under Reference, first of the four plot parameters, where the
note names `PRX_Plot_UID2`. It put whichever neighbourhood parameter sorted first under
Location, where the answer is `Neighborhood Name` and Neighborhood Group sorts above it.

**Nothing is ticked when the plot picker is first drawn**, which is the same rule the Drawing
Sheet's view types follow. Adding every plot in a model into one workbook is one press of Select
all away and is almost never wanted.

## The pane says what it reads, not what the note asks for

`KpiTemplates.SourceOf` used to print the workbook's note as though it were the tool's
behaviour: PRX_COMPONENT and PRX_Plot_UID2 read off the title block. Every part of that was
wrong. PRX_COMPONENT is in no model, the value is `PRX_Component` on the SHEET, and PRX_Plot_UID2
sits on 1233 title block instances holding a value on none of them.

It takes a `ChosenParameters` now and names the three parameters the pane's own pickers hold,
because those are the ones that will be read. With nothing picked yet it names the picker to
look at rather than a parameter nobody chose. A test walks every template and refuses any line
holding PRX_COMPONENT or the words title block.

## No model open and a model never saved are two refusals

`CreateWords.CannotCreate` is given both, and a model that is not open is not asked whether it
has been saved. It used to be handed the model's FOLDER and call it the model, so a detached
model that has never been saved was refused with No model is open, next to a header counting its
96,959 elements and directly under the line that already said the truth. The never saved words
are `TemplateWords.NoModelPath`, the one that line uses, rather than a second sentence.

## The pane holds no copy of anything it can ask for

The model's folder was read once, when the pane was shown, and kept. The model was then saved to
a real folder and **Create stayed grey saying No model is open**, and a KPI Scan after the save
did not shift it. The pane was also holding the title in a second string, set by the scan and by
nothing else, so the two halves of one fact went stale on different schedules.

Four things hold the fix up.

**`OpenModel` is one record**, title and folder together, built from one answer. `CannotCreate`
takes it rather than two loose flags, so nothing can hand it the pair the wrong way round.

**Every answer from `KpiRequestHandler` carries the model state**, whatever was asked for, read
off the live document at that moment. `WhichModel` is only the request that asks for that and
nothing else.

**`RedrawTemplates` asks for it every time it draws**, and `Took` redraws only when the answer
moved, so the ask does not chase its own tail.

**Create is greyed out on what the PANE owns and nothing else**, a template picked and a plot
ticked. Whether a model is open and whether it has a folder are decided on the Revit thread
against the live document when the button is pressed, and the refusal comes back from there.

`Ask` holds one slot and `WhichModel` never takes it from anything, because the pane asks for it
on every draw. It used to displace `Plots`, which the pane asks for in the same breath when it
is shown, so the plot list never arrived at all.

## The reference values follow the ticked plot

The block under the Reference picker showed DM-11's four values with DM-12 ticked, and the same
four with all 155 ticked. It was read for one plot, the first in the model's list.

`ReferenceValuesPerPlot` reads all four for every plot in one pass over the sheets, the shape
`ValuePerPlot` already used, and the pane shows the FIRST TICKED plot's **with that plot named
beside them**. Nothing ticked shows none and says so. The block exists so a person picks the
reference by looking at its value, and a value belonging to a plot they did not choose is worse
than no value at all.

## Shared and not changed

`RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile` are shared with the Drawing
Sheet and this tool changes none of them. A change one of them seems to need goes in
`steps/log.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
two added values, `HairlineAbove` and `WideLabelWidth`, because a number written in a pane file
is the fault that made the first pane black on black. The second is for the KPI pane's typed
boxes, where Prepared by came out as Prepared b running into its box at the shared 54, and it is
added rather than a widening of `LabelWidth`, which the Drawing Sheet uses in two places.

## The tool carries the map, and the map is data

The production templates the team fills carry no note saying where a value comes from,
measured at zero note cells in all seven. The annotated set that holds the mapping in green
text is not what the team fills. So `KpiTemplates` in Core is the map, one entry per template,
measured off the annotated seven cell by cell, with a completeness test. Nothing reads a
mapping out of a workbook and nothing fills on a best guess.

Recognition is the main sheet name first, which settles five of seven. The two park templates
share Park Name, so the file name breaks the tie, and a name that settles nothing puts the
pick to the user. The tree row ranges come from the map entry and never from a constant,
because writing 89 rows into an 80 row list puts quantities into rows no total sums.

## A workbook is copied and patched, never loaded and resaved

An .xlsx is a zip and the client's EXISTING PARKS one holds 37 parts. Loading it into an
object model and saving lost 21 of them, the embedded image, the printer settings, the
threaded comments and the array metadata among them, and the file still opened. So
`WorkbookPatcher` copies the file byte for byte and rewrites only the sheet parts that
receive values and the workbook part, through the platform's own zip and XML types with no
package dependency. It decides everything off the source first, so a refusal writes no file.
It sets recalculate on open, because every formula carries a stored result and the old blanks
would sit beside the new numbers otherwise. Every written cell is read back off the output
and reported as it landed, never as it was sent.

The untouched client file recalculates with 45 errors and a correctly filled one with 44. The
44 are the PARK PROGRAMME section failing on an empty Criteria table either way, so a filled
file showing 44 errors is correct.

**No client workbook enters this repository.** It is public and those files carry the Green
Riyadh KPI targets, neighbourhood names and the plant palette. `*.xlsx` is ignored and tests
build their own small workbook in the temp folder.

## What the first real scan measured

All of it from one run on RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached, 96,959 elements in 5.4
seconds, six of the nine questions answered. These are measurements, not guesses.

**The main sheet name carries angle brackets.** `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>`, `<Streets>`. The brackets are part of the name and not
placeholder notation. They were stripped when the map was written, so all seven real workbooks
came back unrecognised the first time the pane saw the client's folder. A test asserts every
entry opens with < and closes with >.

**PRX_COMPONENT does not exist in this model.** The sheet carries `PRX_Component`, capital C
only, on all 1385 sheets with 1384 values. So a near miss that is named is now also shown, with
the same columns and the same cap the exact name would have used. A near miss named and never
shown is the answer withheld.

**Four plot parameters sit on the sheet**, `PRX_Plot_ID`, `PRX_Plot_UID`, `PRX_Plot_UID2` and
`PRX_Plot_NH`, all with values, and the report prints them side by side one row per sheet
because nothing showing one of the four can say which the workbook wants. `PRX_Plot_UID2` is on
the SHEET, not the title block: on the title block it is on 1233 instances and holds a value on
none of them, and on the sheet it holds 1384 values reading like ANH-007-MO-100019, which is
nothing like a plot identifier such as DM-41.

**THE ELEMENTS A SCHEDULE LISTS ARE NOT THE SCHEDULED THINGS.** Asking Revit for the elements
of DM-11-(600) SOFTSCAPE SCHEDULE returns six RVT Link instances, because the plants live in
the linked component models. Section 8 came back empty for exactly that reason. The printed
rows are not the better route to these numbers, they are the ONLY route, and nothing is ever
recomputed from elements.

**Schedules are per plot**, 155 of each, named `<PlotID>-(600) NAME` and filtered on
PRX_Ref Plot ID Equal `<PlotID>`. Eight distinct names once the plot is off. How many plots of
each name are read in full is the rule above, and it is stated there and nowhere else.

- SHRUBS & LAWN SCHEDULE, category Floors, prints in groups. DM-11 gives GRASS 35 m² 46, then
  SHRUBS & GROUND COVER 70 m² 58, then TOTAL 105 m² 104. The workbook wants the two group
  subtotals and not the total. **How it prints is below and the numbers alone are not enough**
- SOFTSCAPE SCHEDULE, category Planting. Its fields are BOTANICAL NAME, which is
  PRX_Softscape Botanical Name, and COUNT (n), a Count field. DM-11 gives ALBIZIA LEBBECK 6,
  BAUHINIA PURPUREA 2, CASSIA GLAUCA 4, total 12. How it prints is the rule in `CLAUDE.md`,
  stated there and nowhere else, because the group row is what question 8 is answered from
- Two phases, Existing and Proposed. The split shows as a group row inside the printed
  schedule, not as a separate schedule

## How a grouped schedule really prints

Measured off the 1355 scan report, which **is not in this repository** because nothing under
`reports/` is ever committed. The numbers were recorded a round before the shape was, and the
numbers alone were not enough: a reader written to them found no subtotal at all.

DM-11-(600) SHRUBS & LAWN SCHEDULE is eleven columns wide and prints this:

```
IMAGE | # | PLANT CODE | BOTANICAL NAME | AREA  (sqm) | COUNT (n) | HEIGHT (m) | ... | L/DAY
GRASS                                                                     group heading
Proposed                                                                  phase
Pennisetum Setaceum.jpg | PEN SET | ... | GRASS: PENNISETUM ... | 35 m² | 46 | ...   species
                                                          | 35 m² | 46 | ...        subtotal
                                                          | 35 m² | 46 | ...        subtotal AGAIN
SHRUBS & GROUND COVER                                                     group heading
Proposed                                                                  phase
Bougainvillea glabra Pink Pixie.jpg | ... | 36 m² | 46 | ...                    species
Carissa macrocarpa - grandiflora.jpg | ... | 34 m² | 12 | ...                   species
                                                          | 70 m² | 58 | ...        subtotal
                                                          | 70 m² | 58 | ...        subtotal AGAIN
TOTAL                                                     | 105 m² | 104 | ...      the lot
```

Three things follow, and `ShrubsAndLawnRows` holds all three.

**The group heading is on its own row**, first cell only and every other cell empty, rather
than beside its numbers. A phase row sits under it in the same shape, so a structure row that
names no wanted heading opens no group.

**THE SUBTOTAL PRINTS TWICE.** Adding a group's subtotal rows gives 70 and 140. One is taken.
Two that disagree are a failure worth naming rather than a number to pick between, so the
disagreement travels on the `GroupSubtotal` and refuses the write.

**The species rows add up to the subtotal**, 36 plus 34 is 70, so the two are held against each
other and printed. They are not enforced, because every one of those numbers is already rounded
to the metre on the way out of Revit and a sum of rounded numbers need not equal a rounded sum.

TOTAL needs no special case. It carries numbers, so it is not a structure row, and its first
cell holds text, so it is not a subtotal.

Never read a schedule value by cell position, which is the rule in `CLAUDE.md` and is stated
there and nowhere else. `ScheduleColumns` is what asks the heading row. What these schedules
measure is why: eleven columns wide, the first number in a subtotal row is the area and the last
is L/DAY, so both ends are wrong.

**A ROW IS A SPECIES ROW WHEN THE BOTANICAL COLUMN HOLDS TEXT, NOT WHEN THE FIRST CELL DOES.**
The first cell is the image, and **an existing species prints with no photo**, so its first cell
is a dash:

```
-                       | ACA FAR | NO BOQ CODE AVAILABLE | ACACIA / VACHELLIA FARNESIANA | 1
Albizia lebbeck.jpg     | ALB LEB | M-329343-A18          | ALBIZIA LEBBECK               | 13
```

`ScheduleGroups` counted what sat under a group off that first cell and reported DM-12 Existing
as 0 named rows of 6 and DM-13 Existing as 0 named rows of 2, while section 6 of the same file
printed five species under DM-12 Existing totalling 10 trees. Both readers now ask the heading
row which column is BOTANICAL NAME and read that one. The measured counts are DM-11 Proposed 3,
DM-12 Existing 5 and Proposed 3, DM-13 Existing 1 and Proposed 2.

`ShrubsAndLawnRows` had the same fault and no report had shown it, because DM-11's shrubs are
all Proposed and every one of them prints with a photo. An existing shrub would have been read
as a subtotal.

**An area prints with its unit attached and a count does not.** 35 m² against 46. The unit comes
off by reading as far as the number goes rather than by stripping characters. A real area can be
nought: the hardscape schedule prints 0 m², which is the number and not an empty cell.

**The 00 link** is RCRC_NG05_NU_MAIN_RVT24_00.rvt, loaded, 279 filled regions all in a view
called Intervention Limits. Types RCRC_CADASTRAL LIMIT 124 and RCRC_OUT OF SCOPE
(PRESENTATION) 155. PRX_Intervention Area is on all 279 with 266 values and every region
carries PRX_Ref Plot ID, so the report prints the plot beside each region and counts how many
regions of each type carry one. **Which of a plot's two regions carries the area varies by
plot**, which is the rule in `CLAUDE.md`, and it is why the report reads one plot's regions
together rather than picking a type.

**Neighborhood Name**, spelt the American way without a u, is a shared parameter on Project
Information holding KING FAHD. Neighborhood Group holds GROUP 5.

**Units.** Raw areas are square feet whatever the project shows. Printed areas are the project
unit, square metres rounded to 1. 12496.8999938 raw prints as 1161 m².

1385 sheets, every one with a title block, 11 title block types across two families.

## What the two real workbooks measured, once

A one-off check on 2026-09-09 against two EXISTING PARKS files supplied in a chat session, the
production copy and the annotated one. **It cannot be re-run and no gate repeats it.** The
files are not in this repository and never will be. What is committed is what it taught.

Every mapped cell agreed with the annotation, D3, C5, E4, D8, F11 and H11, and E5, G5 and H5
read DATE OF THE DAY, EMPLOYEE NAME and EMPLOYEE POSITION. Those three come from nowhere in
Revit. The team types them into the pane and the tool copies them through, so a filled
checklist carries who filled it and when. They sit in the same three cells in every template,
which is why `KpiTemplates.TypedByTheTeam` holds them rather than a template's mapped cells.

Two things the check found that reading the map could not:

- **The annotation writes the shrubs and lawn note in row 10 AND row 11.** Row 11 is the
  input. In the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would
  destroy a formula. The map's F11 and H11 are right, and now proven right
- **`Area` is a defined name pointing at `<Park Name>`!$D$8**, so D8 is the area the whole
  sheet computes from, and `H9` is `=H8/Area`, which is the divide by zero that goes away when
  the area is filled

Species stop where the map says: existing rows 4 to 92, proposed 4 to 84, header row 3, and the
total at row 93 is `SUM(B4:B92)` on both sheets.

## What the first real workbook measured

One press of Create on DM-12 with the MOSQUES template, 2026-09-09. **The workbook is written
and correct.** These are measurements off that output file, not reasoning about it.

- **37 parts in, 37 out, 4 changed**, and the output recalculates with ZERO errors. The parks
  figure above, 45 against 44, is EXISTING PARKS and is a different template. Both stand
- The six values landed and the client's own formulas ran on them: **28.1 percent canopy against
  a 13 percent target, Excessive, NOT COMPLIANT.** The tool wrote no verdict anywhere. That is
  the workbook's arithmetic on the numbers Revit gave it
- **The slash case matched.** ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana
  at row 11, which is why nothing is stripped or split on the way to a comparison
- **Three species were correctly refused.** The MOSQUES tree list holds 80 species and not one
  of them is Phoenix dactylifera, Washingtonia robusta, or any of the Unknown rows. Checked
  against the output file itself rather than against the map

The last one has a consequence and it is an open question rather than a fault:

**The workbook reads 2 existing trees where the model holds 10, and 31 in total where the model
holds 39.** The tool is right and the client's list is short. **Nothing in the tool may ever
place an unmatched species by guessing**, so the three are named in the report and the numbers
stay as they are until the team answers. `steps/log.md` carries it as the open question.

## The area is not a schedule row

The report used to end saying every number above came off a row the schedule printed. **The area
did not.** H7 took 3728.7570000000005, converted from the raw 40136.006313679296 square feet off
`PRX_Intervention Area` on the chosen filled region in the 00 link, where the schedule prints
3729.

The conversion is right and is more precise than the printed value. The sentence was wrong about
it. The region row carries the raw reading, the converted metres and what the model prints, side
by side and unrounded, so the two can be held against each other, and the closing paragraph says
the schedule claim for the numbers it is true of and names the area separately.

## What the team types reaches the cells

E5, G5 and H5 come from no model. The pane collects them, and **it used to collect them and hand
none of the three on**: `KpiCreateAsk` did not carry them and `KpiCreatePlan.Of` defaulted all
three to null, so the first real workbook came out holding the template's own placeholders while
the report said nobody had typed them, on a run where all three boxes were filled in.

The three are REQUIRED arguments of `KpiCreatePlan.Of` now, so a caller that forgets them does
not compile. A default that reads as a deliberate empty is how a whole link in a chain goes
missing without a word.

## Open, and not to be guessed at in code

The first real scan raised five. The 1355 run settled four of them, and the answers are facts
about the project rather than about the tool, so they are in `CLAUDE.md` and are not repeated
here. What each one turned out to be:

1. Is PRX_Component the component the workbook wants. **Yes**, and it is the asset type
2. Which of the four plot parameters is the workbook's Ref. **PRX_Plot_ID**
3. Which filled region type is the plot's intervention area. **Neither. It varies by plot**,
   so the question was wrongly put and no type name can answer it
4. Are the two group headings in SHRUBS & LAWN the same on every plot. **STILL OPEN**
5. Does an Existing group ever appear in SOFTSCAPE SCHEDULE. **Yes**, DM-12 has one

Two the 1355 run raised in their place are in `steps/log.md`, both about matching a species by
name, and nothing in the code picks an answer to either.

The 1521 run raised a third, which template each value of PRX_Component means. **The 1548 run
settled it.** Eleven values came off the report's own component block and the team turned them
into the table above. It is not repeated here.

## Do not name a KPI control Scan Model

Scan Model is a button inside the Drawing Sheet pane. Two buttons with one name doing
different things is a trap for the production team. The KPI one is KPI Scan.
