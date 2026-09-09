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

## Shared and not changed

`RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile` are shared with the Drawing
Sheet and this tool changes none of them. A change one of them seems to need goes in
`steps/log.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
one added value, `HairlineAbove`, because a number written in a pane file is the fault that
made the first pane black on black.

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
  subtotals and not the total
- SOFTSCAPE SCHEDULE, category Planting. Its fields are BOTANICAL NAME, which is
  PRX_Softscape Botanical Name, and COUNT (n), a Count field. DM-11 gives ALBIZIA LEBBECK 6,
  BAUHINIA PURPUREA 2, CASSIA GLAUCA 4, total 12. How it prints is the rule in `CLAUDE.md`,
  stated there and nowhere else, because the group row is what question 8 is answered from
- Two phases, Existing and Proposed. The split shows as a group row inside the printed
  schedule, not as a separate schedule

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

## Do not name a KPI control Scan Model

Scan Model is a button inside the Drawing Sheet pane. Two buttons with one name doing
different things is a trap for the production team. The KPI one is KPI Scan.
