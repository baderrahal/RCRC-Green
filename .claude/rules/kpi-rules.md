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

**A choice made after a refusal is applied to the run already read.** Every region choice and
the identical areas confirm used to read every ticked plot from the start, five minutes on 20
plots and about twenty on 78, once per plot that needed a pick. A refusal exists so a person
can answer a question, and answering it should not cost the answer again. The run the pane
holds travels on the ask as the same object, `HeldReadings.Decide` says whether it can answer
this press, and `Applied` puts each plot's chosen region on its reading through
`PlotReading.WithChosenRegion` with nothing read. It is not a cache with a lifetime of its own.
The run before is trusted when it wrote nothing and it is the same model title, the same
template and template file, the same two parameters and the same plots, and every ticked plot
has a reading on it that was not refused. Anything else reads the model again with the reason
named: no run is held, the run before wrote its workbook, the model or the template or a
parameter or the plots changed, a plot has no reading, or a plot's read was refused, because a
plot that threw holds nothing to reuse and the model fixed in between is the reason for the
second press. The report says which under the Run line, Readings: reused
or read from the model on this press, with the reason, so a read of 0.0 seconds on a reused run
is true and says why. A model edited between the refusal and the pick is the one thing none of
those comparisons can see, and that is for Bader.

**It knows the template, and on a template with no area cell the area is not read.** STREETS
types the road width and the total length by hand and the sheet works the area out. MM-03 and
MM-04 are street plots and both read 12182.05561411 in the 00 link, so a reconciliation that
did not know the template would have ended the first 78 plot run asking the user to confirm an
area the workbook has no cell for, then read all 78 again. On such a template no filled region
is read, nothing about the area is refused on, and the report says the area was not read and
why rather than leaving the section empty.

**A refused schedule read refuses the write.** A column the heading row did not name, a cell
holding a digit past where its number ends, a species row with no whole count: each travels on
the plot reading with the schedule's name, prints in red above Create, and prints twice in the
report, among the reasons and under the plot. A refused read used to come back as a list of
nothing, which read as a plot whose schedule listed no species.

**A throw on one plot names the plot and the run carries on.** The loop over the ticked plots
read each one under no guard of its own, so a throw on plot 60 of 78 fell to the run's catches,
which said Revit would not do that now with no plot named and wrote no report, and the two
refusals for a plot list that comes out shorter or longer than it went in could never fire.
`GuardedRead` in `KpiRequestHandler` wraps each plot's regions and read and hands back
`PlotReading.NotRead`, a reading whose one refusal is the read threw and nothing on this plot
was read, with the exception's type and message. Each schedule's read inside
`KpiPlotReader.Read` is guarded the same way and names the schedule. Every exception type is
caught there on purpose, the same as the scan side's `Guarded`, because a partial report that
names the bad plot is the point. The run carries on, the reconciliation refuses the write naming
the plot, the plot is listed under contributed nothing with the refusal first, the status line
counts it as a reason, and the report is written either way. The catch inside `Printed` that
swallows an `ApplicationException` with nothing recorded is finding 6's and stands.

**A plot holds one schedule of a kind or the kind is not read.** The reader finds every
schedule of each kind filtered on the plot before it reads any, reads the one when there is
one, and hands `PlotReading` the names of all of them. A reading refuses to hold species rows
beside two softscape names or subtotals beside two shrubs and lawn names, and `Reconciliation`
refuses the write naming the plot, the kind and every schedule found. The report names the
schedule each number came off, per plot, and says NONE READ with every name where there were
two. Nothing picks the first, and nothing adds them.

**THE FM-05 DOUBLE WAS NOT TWO SCHEDULES, AND IT WAS NOT ONE SPECIES PRINTED TWICE UNDER ONE
GROUP EITHER. IT WAS A THIRD GROUP.** The first twenty plot run printed FM-05 twice in one
species row, FM-05 10, FM-05 10, FM-06 15, and the round that fixed it took that for a second
schedule without measuring one. The 1428 run read one softscape schedule on every plot and
printed the same rows, ALBIZIA LEBBECK 10 and 10, BAUHINIA PURPUREA 19 and 20, CASSIA GLAUCA 3
and 4, and the round that fixed that took them for two rows of one species under Proposed and
refused the plot. The 1536 report's printed section showed the second of each pair sitting
under a third group row, STREET DESIGN at row 14, after Proposed's own subtotal row. Two rounds
diagnosed a shape nobody had looked at. **The section that prints the schedule as printed is
what settled it, and it is why the report ends with what the tool read.**

A species on two rows under ONE group is still refused, with the plot, the species, the group,
the rows and the counts named, because nothing says whether that is two types of it or one
counted twice. `SpeciesRow` carries the row it printed on and the row of the group it sat under,
and `PlotReading.SpeciesPrintedOnMoreThanOneRow` finds every such species. A species under two
groups is two species and refuses nothing.

## Only the groups a tree list sheet is named for count

**Bader has decided that Street Design is somebody else's scope and does not belong on this
plot's checklist. The model will be corrected later. Until it is, the tool leaves those rows
out and says so.** That is a decision, recorded in `steps/log-kpi.md` as one, and not a
measurement.

The words Existing and Proposed appear nowhere in the code that decides it. `CountedGroups`
holds the two tree list sheet names off the template, Tree List - Existing and Tree List -
Proposed, and a group counts when a sheet's name ends in the group's name, word for word and
without case. Nothing looser: Tree and List are words of both sheet names, TREES is the heading
over the groups, and none of those is what either sheet is for. All seven templates name their
sheets that way, so a group the workbook has no sheet for is out of scope on every one.
`KpiRequestHandler` no longer reads the document's phases for the create path. The scan still
does, for section 7, and that is a different question.

**Every group row is found and every group row is named.** `SoftscapeRows.Read` reads a text
only row followed by anything but another text only row as a group row, the species rows under
it as its rows, and the first count with no name under them as its subtotal. Each comes back as
a `PrintedGroup`, in printed order, with its row, its species, its subtotal row, TAKEN or LEFT
OUT and why. The reading's species are the taken groups' rows. Two checks, both refusals in
`Reconciliation`: each group's species rows against its own subtotal row, and the groups taken
plus the groups left out against the TOTAL row.

```
FM-05, off the 1536 report
row 3   Existing        4 species rows adding to 6     subtotal row 8 prints 6     TAKEN
row 9   Proposed        3 species rows adding to 32    subtotal row 13 prints 32   TAKEN
row 14  Street Design   4 species rows adding to 38    subtotal row 19 prints 38   LEFT OUT
row 20  TOTAL 76        6 plus 32 taken, 38 left out, 76
```

**The shrubs and lawn schedule prints the same third phase and the value is the phases taken
added together**, area and item count, with the group total row as the check on every phase,
taken or not. FM-05 GRASS: Proposed 96 over 117 taken, Street Design 69 over 84 left out, the
group total 165 over 201 checked. SHRUBS AND GROUND COVER: 361 over 450 taken, 459 over 570 left
out, 820 over 1020 checked. A group whose every phase is out of scope is nought and says so. A
group with no phase row at all keeps the rule it had, because it offers nothing else: the last
row is the value and the rows above it must add to it.

**FM-05 reads 6 existing and 32 proposed trees, ALBIZIA LEBBECK 10 and not 20, grass 96 and
shrubs 361.** The rule before this one refused the plot, the one before that wrote 165 and 820.

The report says all of it under the plot: every group row with its numbers and its reason, the
rows left out by name and count, each phase row of the shrubs and lawn groups the same way and
the group total row with whether the phases add to it. The accounting at the top counts the
schedules holding a group no tree list sheet is named for and names the plots, which is the
line that would have shown the street on the first twenty plot run.

**A schedule can repeat a group name, and nothing guesses which is meant.** DM-25 prints
Existing, then Proposed, then Existing again. Both Existing rows are in the list with their own
row numbers and subtotals, both are taken, and the second's reason says it is the 2nd group row
so named on this schedule. A species under both is refused as a species under one group name
twice, and the refusal names the two group rows so a person can see it is two groups. Whether
those are one phase printed twice or two things is UNKNOWN and is for the team.

**One shape nobody has measured.** A softscape schedule printing TREES and then species rows
with no phase row would read TREES as a group row, TREES counts for nothing, and every species
would be left out and named. No such schedule has been seen.

## Street Design counts as Proposed on STREETS, and nowhere else

**Bader's decision, on top of the rule above.** No template has a Tree List - Street Design
sheet, so the rule alone left a Street Design group out everywhere, streets included, and on
a street plot that group is the plot's own work. Measured on ST-05, a street plot, off its
softscape schedule on screen:

```
Existing        369
Proposed          2    ALBIZIA LEBBECK 2
Street Design    68    ALBIZIA LEBBECK 6, CASSIA GLAUCA 62
TOTAL           439
```

ST-05's proposed trees are 2 plus 68, 70, its existing are 369, and 369 plus 70 is 439, the
TOTAL the schedule prints. That is the test.

**It is data on the template and it is keyed on the template, never on the plot prefix.**
`KpiTemplate.GroupsCountedAsProposed` holds Street Design on STREETS and nothing on the other
six, `CountedGroups.Of` reads it into a `GroupByDecision` pointing at Tree List - Proposed,
and `SheetFor` answers a sheet named for the group first and a sheet that takes it by decision
second. PRX_Component picks the template and the prefix is only a cross check, which is already
the rule, so the same schedule read for MOSQUES leaves the group out with 68 named, and a mosque
plot read for STREETS counts it. **A second name goes into that list only when the team says
so.**

**The rows go where the sheet takes them, and a species under Proposed and under Street Design
adds.** `KpiMerge.Species` takes the `CountedGroups` and keys every row on the sheet that takes
its group, so ALBIZIA LEBBECK on ST-05 is one merged row of 8, ST-05 8 (2 rows, 2 + 6), going
to Tree List - Proposed and saying both groups. That is two groups, not one species printed
twice under one group, so the same group refusal does not trip. `SpeciesMatching` places a
merged species through the sheet the merge decided and, for one built with none, through the
same `CountedGroups`, so nothing here matches a word of a sheet name on its own any more. The
street's areas count the same way: a Street Design phase row in the shrubs and lawn schedule is
taken on STREETS and its area adds.

**The report says which route each group took.** The reason beside a group row reads Tree List
- Proposed is named for it, or Tree List - Proposed takes it on STREETS by decision, as that
plot's own work, or no tree list sheet is named for it, so it is out of scope. A Street Design
group counted on STREETS reads differently from one left out on MOSQUES.

**The note goes on the pane, not only in the report.** `CreateWords.GroupsLeftOut` builds one
short block above the Create button naming the plots and the schedules where a group no sheet
takes was found, Street Design found on 2 plots on MOSQUES, which has no sheet for it: DM-16 in
its softscape and its shrubs and lawn schedules, FM-05 in both. Those rows were left out. Fix
them in the model. It is a NOTE and NOT A REFUSAL: the run goes through, the workbook is
written, the numbers are right, and the note says where the model needs correcting. Plots and
schedules, never species, because on a run of 78 plots a long list is not read. The report
keeps the full detail with the counts and the areas left out.

## Matching a species is plain or it is nothing

The workbook's own column D is the only species list there is and `SpeciesList` reads it out of
the template. Nothing in this repo carries a copy of the plant palette. Matching is the
botanical name compared without case and with surrounding whitespace off, and nothing else.

Three measured cases are why nothing is stripped, split or normalised past that. The model
prints a species called UNKNOWN and the workbook holds four rows all named Unknown Tree, so
nothing can match those on name. ACACIA / VACHELLIA FARNESIANA carries a slash.
BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an apostrophe, and the shrub rows are prefixed
SHRUBS: and GRASS: where the workbook's list is not.

**A species Revit holds that the list does not is WRITTEN IN and named in the report.** It goes
into the first empty row below the list on the sheet its group points at, the botanical name in
column D and the count in column B and nothing anywhere else. This reverses the rule that it was
named and written nowhere: a quantity that goes nowhere leaves a tree list that reads as complete
and is short, and DM-12 came out reading 31 trees where the model holds 39.

**Unless the model prints no canopy diameter for it, and then it gets no row at all.** A row
written into an empty one carries only what the model prints, and the diameter is the one
measure the sheet computes from. Measured on the MOSQUES template, Tree List - Proposed row 21:
L21 reads J21, the diameter, M21 reads L21 and the count, O21 reads N21, which is typed and
never written, and nothing reads I21, the height, or K21. Measured on the 1836 run over 20
mosque plots: 34 cells were ready, UNKNOWN went into Tree List - Proposed row 85 with its name
and its count, DM-25 row 19 prints 0 for its canopy diameter and a nought is no size, the
formula check found seven formulas that would read an error, and the workbook was deleted.
**The guard was right and the rule it caught was wrong.** `SpeciesMatching.WrittenInto` asks
`MeasureAnswer.Write` of the canopy diameter before a row is taken, and a species with no
usable one gets `NotSized`, which quotes what every row printed. **A species that cannot be
sized is not a refusal.** It is one line in the report, its count among the trees not written,
and a workbook that computes.

**The height is not load bearing.** The rule's first round required both measures off the round
message's wording, so a species with a diameter and no height took no row for a cell no formula
reads. It is written now: the name, the count and the diameter go in, the height goes in when
the model prints one, and when it does not the height cell is named as not written with what
the rows printed, through the same skip every other measure cell already uses.

Three things hold it up. **The diameter is asked before a row is taken**, so a refused species
leaves the empty row for the next one rather than using it up. **The sheet's own total is asked
first**, because a sheet with no total writes nothing for anybody and that is the larger fact.
And **the diameter alone decides**, off the row 21 measurement above.

**The report says how many trees went nowhere and out of what.** One line under the species the
list does not hold: NOT WRITTEN, THE WHOLE RUN: 1 tree of 528, over every species this run
merged. A workbook one tree short and a workbook eighty five short read the same without it.
Both numbers come off the one list of matches the section above prints from.

**Every row fact comes from the file and the map holds no row range.** The first twenty plot
run measured three answers to where the MOSQUES existing list ends: the map said row 83, the
sheet's total said `SUM(B4:B92)`, and the botanical names ran to row 101, 98 of them. The tool
trusted the shortest, so CONOCARPUS LANCIFOLIUS at row 84, PHOENIX DACTYLIFERA at 86,
WASHINGTONIA ROBUSTA at 87 and FICUS BENJAMINA at 89 were reported as having nowhere to go, 66
trees with a row waiting, and the workbook went out saying 76 existing trees where the model
holds 161. `SpeciesList` reads two things off each sheet and holds them apart: every row that
names a species, read down column D from the row under the header until the first empty row,
and the rows the total reaches, read off the total's own `SUM` formula. The one number left in
the map is the header row, 3, measured on all seven templates.

**A name on a row the total does not reach is refused, with the row and the total named.** On
that same sheet rows 93 to 101 name nine species past `SUM(B4:B92)`, so PROSOPIS JULIFLORA at
row 99 is matched, not written, and named under SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES
NOT REACH with its cell in CELLS NOT WRITTEN. A count written where no total adds it leaves a
sheet that reads as complete and is short, which is worse than the gap. With no `SUM` found a
matched name and an unmatched one are both refused the same way, because nothing says which
rows a count reaches. The empty rows for a species the list does not hold are the rows the
total reaches that name nothing, worked out from those two reads and stated by nothing else. A
name below the first empty row is not the list, is named in the report, and a species carrying
it is refused rather than written in a second time above it.

**The report prints both lists as read**, under THE WORKBOOK'S OWN TREE LISTS: the names and
their rows, the total and its reach, the empty rows, and the names the total does not reach.
The MOSQUES list read 80 names in rows 4 to 83 on 2026-09-09 and 98 in rows 4 to 101 on
2026-09-10. Who added the 18 and why the total was not extended to cover the last nine is
UNKNOWN and is for the team.

**A species written in carries four things: the name, the count, the height and the canopy
diameter.** The softscape schedule prints HEIGHT (m) and DIAMETER (m), and on six species
sitting in both they read the same as the workbook's Mature Height and Average Mature Canopy
Diameter, measured on the 1428 run: ACACIA / VACHELLIA FARNESIANA 7 and 6, ALBIZIA LEBBECK 15
and 8, BAUHINIA PURPUREA 6 and 5, CASSIA GLAUCA 6 and 5, HIBISCUS TILIACEUS 6 and 5,
WASHINGTONIA ROBUSTA 25 and 5. A row written with the name and the count alone broke the
workbook's canopy maths: L84 reads `IF(ISBLANK(J84), " ", ...)` and returned a space, M84
multiplied that space by the count, and #VALUE! ran through the canopy total to the KPI row,
nine errors that survive a full recalculation. So `SoftscapeRows` reads both measures off the
columns the heading row names, `MergedSpecies` holds every row's value against the others, and
`KpiCreatePlan` writes them into the columns the sheet's own header row names, found by the
words HEIGHT and DIAMETER and never by the letters I and J. A dash, a nought or a cell that
does not read is not written and is named: UNKNOWN prints a dash and a 0. Rows off more than
one plot that disagree write nothing into that column, every value is named, and the report
says the row will not compute its canopy. Nothing is averaged and nothing is taken first.

**Two headings can hold the word, and the sheet's own formulas say which.** Measured on the
1707 run: the tree list header row holds I Mature Height (m), J Average Mature Canopy Diameter
(m) and K Mature Canopy Diameter (m). Both J and K hold DIAMETER, the reader found two, refused
to choose and wrote nothing into J, L84 returned a space off the blank, M84 went #VALUE!, and
the canopy guard deleted the output. Correct at every step, and the cause was one column choice.
J is the column the workbook computes from: L reads J and nothing reads K. So where more than
one column holds the word, `SpeciesList` reads every formula below the header row off the
sheet part, collects the columns those formulas reference, and takes the one candidate the
formulas read. Where that still leaves more than one, or none, nothing is written and both are
named with what the formulas read, and its own formulas read none of them, or both of them.
Never a position. How each column was chosen is recorded on the list, the one column of the
header row holding the word, or of J, K holding DIAMETER, the one the sheet's own formulas
read, and the report prints it beside each tree list. The check: PHOENIX DACTYLIFERA writes 18
into I and 15 into J, and UNKNOWN writes neither and still refuses through the guard.

**The matches that disagree are counted.** The 1707 run read 22 matched species whose height
or diameter in Revit differs from the row the workbook holds, nearly every match. Both numbers
stay named and nothing is changed, and one line under that heading says how many of the
matches differ in a height, a diameter or both, so the size of it is visible without counting.

**Family, genus, native and every code column stay empty.** Revit does not print them, so the
KPIs that need them still cannot see a species written this way. That is in `steps/log-kpi.md`
as an open question for the team.

**A matched species whose height or diameter in Revit differs from the client's row is named
and the row is left alone.** PHOENIX DACTYLIFERA prints 15 metres across in the model and the
MOSQUES existing list holds 8 at row 86. Two numbers for one species, and which is right is a
question for Bader, in the log, not a cell to overwrite.

More unmatched species than empty rows writes what fits, names the rest, and says plainly that
the sheet ran out of room. A species the list holds and Revit does not is left empty, which is
correct and needs no line.

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

**The create report ends with what the tool read.** Everything else in it is what the tool
concluded, and a species printed on two rows survived two rounds because nothing showed the
rows. `PlotReading.PrintedSchedules` carries every schedule the plot's numbers came off, row for
row, and `KpiCreateReport` prints each last, under EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE
PRINTS IT: the name, how many rows it printed and how many are shown, which rows were read as
species rows, which as subtotals, which were group rows and which rows were left out, and where
the TOTAL row was, and for the shrubs and lawn schedule which phase row was taken and which
left out and that the group total row was checked. Every
column, padded to its widest cell, with the row numbered the way the readers number it, the
heading row being 1. Two hundred rows a schedule at most, said in those numbers. The top of the
report says the section is there.

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

**The plot prefix does not decide.** STREET 36m ROW covers MM and ST plots and NS carries two
different street widths, so a rule on the prefix alone would answer three of them wrongly. It is
read, as the cross check and the grouping in the section below, and it never overrides this
table.

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

## The plot prefix is the second route, and it does not decide

Confirmed by the team, all seven templates and every prefix, and `PlotPrefixes` is that table:

```
STREETS   NS, ST, MM        SCHOOLS          SC
PARKING   PL                EXISTING PARKS   EP
MOSQUES   FM, DM            FUTURE PARKS     FP
                            HEALTHCARE       HF
```

It agrees with the eleven component values prefix by prefix with nothing left over on either
side, and a test written out by hand says so.

**Two records of one fact is the fault this repo has met eight times, so the two do not get equal
standing.** `PRX_Component` decides. The prefix is a cross check: where they agree the pane says
the prefix agrees, and where they disagree NEITHER decides, both are named and nothing is
preselected. A prefix the table does not hold cross checks nothing, which is different from one
that disagrees.

**What the prefix is really for is grouping.** One button per template beside Select all and
Clear ticks every plot of that template at once. It REPLACES the ticks rather than adding to
them, because one checklist is one template. A plot whose prefix the table does not hold is
reached by no button and the pane names it, so it is ticked by hand rather than left invisible.

**It is also the only thing that can place a plot with no sheet.** The 1548 scan found four on a
schedule and on none, EP-05, EP-11, EP-12 and EP-13. No sheet means no PRX_Component. The pane
says the component could not be read and the prefix was used, rather than preselecting in
silence, and the line saying which route the answer took shows whichever way it went.

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

## The workbook goes where the user browsed, not beside the model

**Writing beside the Revit model meant a detached model could not be used at all**, and a
detached model is what the team works on. It cost most of an afternoon. `OutputFolder` is
browsed for and remembered in `kpi-output-folder.txt` beside the installed assembly, the same way
the template folder is, through the one `RememberedFolder` both use.

`CreateWords.CannotCreate` asks whether a model is open and whether an output folder is set.
**Whether the model has been saved is asked nowhere now**, and the never saved refusal and
`TemplateWords.NoModelPath` are both gone. The no folder words are `TemplateWords.NoOutputFolder`,
the ones the output folder line already shows, rather than a second sentence.

The model's folder came off `OpenModel` with them. It decided nothing once the output folder
existed, and a value on the screen that decides nothing is how one stale string became a dead end
here already. The silent overwrite and the editable name box are unchanged.

The refusal is still decided at the moment Create is pressed: the document off the live document
on the Revit thread, and the folder read off the pointer file in the same breath, so neither can
be a copy the pane took earlier.

## The pane holds no copy of anything it can ask for

The model's folder was read once, when the pane was shown, and kept. The model was then saved to
a real folder and **Create stayed grey saying No model is open**, and a KPI Scan after the save
did not shift it. The pane was also holding the title in a second string, set by the scan and by
nothing else, so the two halves of one fact went stale on different schedules.

Four things hold the fix up.

**`OpenModel` is one record.** It carried the title and the folder together, built from one
answer, so nothing could hand `CannotCreate` the pair the wrong way round. The folder is off it
now: the workbook goes to the browsed output folder and the model's own folder decides nothing.
The rule that got it there stands and is why the title is still a record rather than a loose
string.

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

**The one copy the pane holds is the templates folder's recognitions, and it says so.** Every
redraw opened and peeked every .xlsx in the templates folder on the interface thread, and every
tick redraws: seven zips for each of 155 ticks, 1,085 opens. `TemplateListing` holds each
file's recognition once per folder, keyed on the folder compared without case and with a
trailing separator off, cleared when the folder changes and after any press of Create that
reached the patcher with an output path in it, wrote or not, because the copy lands before the
patch and a patch that fails after it leaves the copy behind. The folder itself is still listed
on every draw so a file added or gone is seen on the next redraw
and opened once. It counts the opens and the redraws, the pane prints the count under the list
and the report prints it under WHERE EVERY VALUE CAME FROM, seven opens over 155 redraws, so
the seven per tick cannot come back unnoticed. A workbook overwritten in place under the same
name in that folder by anything other than this tool keeps its held recognition until the folder
is browsed again or Create reaches the patcher into it, which is written down here as the limit.

**The plot list keeps its place across a tick.** The list is a new viewer on every redraw and
every tick redraws, so 155 tick boxes threw themselves back to the top on every tick, the fault
the user reported on the Drawing Sheet's lists and fixed there. `Scrolling` and `Remembering`
in `KpiPanel` are that shape written in the KPI folder: the remembered offsets are read into
locals before any handler is attached, restored on the first layout pass rather than on Loaded
because an unmeasured viewer clamps any offset to zero, and noted on every scroll change.
`ScrollMemory` in Core holds the rule half, a restore wanted only when something above the top
was noted, with tests. It is a copy of another task's helper and not a call across the fence,
and whether the two panes should share one is a Shared round for Bader to call.

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
`steps/log-kpi.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
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
pick to the user. The map names the two tree list sheets and no row on either. It carried a
last row per template until the first twenty plot run, 83 on MOSQUES, and the names on the real
sheet ran to 101, so the rows are read off the file under the species rule above.

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

**EXCEL SHOWED ZEROS WHERE THE NUMBERS WERE RIGHT.** The first real output read 0 for Total Green
cover, Canopy Area, Total Trees, Total Trees Native, Total Trees Adaptive, Total Planting Area and
Total Lawn Area, beside Planting 410, Lawn 60 and Mosques Area 3,729 which all read correctly.
The values were not wrong. They were stale cached results and Excel never recalculated. Total
Planting Area is `=F10` and F10 held 410, so a 0 there could only be a cache.

`fullCalcOnLoad="1"` was already there, so **the flag alone is not enough.** Three things
together, measured on that file: `calcId` set to 0 in `calcPr` with the flag kept, the cached
`<v>` dropped from every formula cell in every sheet leaving the `<f>` alone, 301 of them in that
file, and `xl/calcChain.xml` removed. Forcing a recalculation gave Total Green cover 1518, Canopy
1048, Total Trees 31, Planting 410, Lawn 60.

**The output is then checked the way the written cells already are.** `CacheCheck` is read back
off the file: recalculate on open, calcId cleared, no formula cell carrying a cached value, the
calc chain gone, and calcMode auto. All five, or the report says the file may open showing
stale numbers. A workbook that opens showing zeros beside correct inputs is the worst thing this
tool can produce, because it looks finished.

**calcMode is the fifth, measured on the 1428 workbook.** The four above all held and Excel
opened the file showing every written number and every formula cell blank, and Ctrl Alt F9
filled them: 528, 3258, 1127, 6 and 522. calcPr read `calcId="0" fullCalcOnLoad="1"` and no
calcMode. Excel's calculation mode is a session setting and the first workbook opened in a
session sets it, so anyone with a manual workbook open, or manual in their own options, opened
this file into a manual session. `calcMode="auto"` is set outright beside the other two, read
back, and required. The patcher also looks for every other place in the package that can hold
a calculation setting, a `sheetCalcPr` in any sheet part, a VBA project and any other attribute
on calcPr, and the report names what it found or says none was found and what it looked for.
**Nothing here can run Excel and neither can the gate**, so the five checks are over what the
file says and not over what Excel does with it, and the report says so in those words.

**The output's formulas are read for what they will compute, and a written cell nobody can
compute from is a refusal.** `WorkbookFormulas.Check` reads every formula in the output, by its
text alone and never by evaluating one. A formula holding ISBLANK on a cell that is blank and a
string literal returns that text, a formula doing arithmetic on such a cell is #VALUE!, and
every formula reading a cell in error carries it. When the chain starts on a row this run wrote
into, the output is deleted again and the run is refused naming every formula. The section WHAT
THE WORKBOOK WILL COMPUTE FROM THIS prints every formula at risk with the reason, every formula
reading a row this run wrote into with the reference it reads it through, the six cells the map
names with whether each is present and which formulas read it with what blanks among their
inputs, and every function the file stores with the `_xlfn.` prefix, by name and by cell count,
because such a cell reads #NAME? in a version of Excel that does not have the function. The
1428 workbook read #NAME? in every Meets KPI and Compliance cell, nine rows plus the Tree Class
and Planters rows, off `_xlfn.IFS`. The tool writes no formula and did not put them there, and
which version of Excel has IFS is not worked out here.

**The part count reads 37 in and 36 out and the report says which part went and why.** A count
short by one with no explanation reads as a loss. `PatchOutcome.PartsDeliberatelyRemoved` is what
keeps `KeptEveryPart` true across it.

## Nothing may write to a template, and two guards say so

The output folder is browsed for and the name box is prefilled with the template's own file
name, so pointing the one at the templates folder put the other one press from naming the
template itself. `Patched` deletes the output file before the copy, and that press removed the
client's GRP KPI Checklist. No copy, no undo, every later run of that template impossible, and
the run ended saying only that the workbook could not be written.

**Two guards, because either alone is one refactor from being bypassed.** One in
`KpiRequestHandler.Patched` before the delete, one in `WorkbookPatcher.Patch` before it opens
anything. Both call `FilePaths.Compare` and both refuse on the one sentence in
`CreateWords.WouldOverwriteTheTemplate`, so the two say one thing rather than two.

**The comparison is the absolute canonical form of each, without case, which is how Windows
compares a path**, with any trailing separator off because `GetFullPath` keeps one. A path that
cannot be resolved answers `Unreadable` and refuses the same way `Same` does, because a check
that cannot see its own subject has to refuse.

**It is textual and that is its limit.** A junction, a symbolic link, a substituted drive or an
8.3 short name reaches one file under two names that do not resolve to one string. Asking the
file system for an identity means opening both files, which is the thing being guarded against.

**The guard refuses one file, not one folder.** Writing a differently named workbook into the
templates folder is allowed and still goes through. What stops the user reaching the refusal at
all is `TemplateWords.OutputIsTheTemplateFolder`, said under the output folder line when the two
folders are one, before Create is pressed rather than after.

**A filled checklist in the templates folder is named as filled and not offered.** Recognition
is the first sheet's name and a filled MOSQUES output keeps `<Mosques>`, so the next redraw
offered MOSQUES DM-12.xlsx as a template beside the client's, and picking it would copy last
time's typing and last time's written species as the template. The tool can tell, because it is
what wrote it.

**A workbook is filled when a cell the tool writes holds something the tool would have written,
never when one cell differs from one expected string.** The first rule was E5 against the
angle bracketed `<Date>` and Bader measured two template sets that break it. The KPI CHECKLIST
R1 set holds a placeholder in every cell the team fills, `<Date>` at E5, `<Name>` at G5,
`<Position>` at H5 and `<UID>` at C5. An earlier production set holds NOTHING at E5, G5 or C5
and real values at D3 and H5, its D3 reading Future Park and its E4 KING ABDULLAH South. So a
placeholder is one set's habit rather than a rule, an empty cell is not a placeholder either,
and a clean template can hold real text in a mapped cell.

`FilledMarks` is the rule. Two cells can decide, and both are cells the tool writes: E5, where
a date reads as filled, and the template's own reference cell, C5 on all seven, where a plot
reference does. A date has at least two numbers in it and parses as one, so `<Date>`, the
annotated set's DATE OF THE DAY, a bare 10 and an empty cell are all templates. A plot
reference is one unbroken run holding a letter and a digit and no angle bracket, which both
parameters the team picks read as, DM-12 and ANH-007-MO-100019, so `<UID>`, KING ABDULLAH South
and an empty cell are all templates. The other cells the tool writes decide nothing, because a
clean template already holds real text in some of them.

**Nothing is withheld on a cell the tool has never written**, so an absent or empty cell is
never a filled file. The reason and the report both name the cell that decided and what it
held, `not offered: MOSQUES DM-12.xlsx, C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes`, so a template wrongly withheld is traced in one line rather than by opening
the file. `PeekedWorkbook` reads those cells off the first sheet in the same open as the names,
with a shared string resolved to its text because an Excel re-save stores it that way, and the
pane lists a filled file greyed in the same list. Nothing is deleted or moved, and browsing to
a folder that holds one is untouched.

**Two limits, both stated rather than guarded against.** A filled workbook the tool wrote
neither cell into reads as a template. `KpiCreatePlan` skips the reference cell when the ticked
plots disagree on it or none of them holds it, which a checklist covering several plots usually
does, so on such a run the typed date is the only mark left, and the date box is prefilled with
today but can be cleared. And a client set that hinted the shape of a reference rather than
bracketing it, DM-00 at C5, would be withheld, because nothing separates a hint from the thing
it stands for, and neither measured set does that. The first is the safe way round: offering a
filled file costs a rerun, and withholding a real template leaves the team unable to fill
anything. The second is visible in one line, because the report prints what the cell held. Both
are for Bader.

## Every run that ends with no file says why

`CreateWords.Wrote` fell to `Refused(run.Reconciliation)` whenever nothing was written, and
that answers the empty string when the accounting added up. **So a run whose accounting passed
and whose patch was refused set the status line to nothing at all**, and the pane went from
Creating to blank. The commonest cause is the output workbook still open in Excel from the run
before, which the delete answers with an IOException.

`WhyNothingWasWritten` is never empty. The accounting speaks first because it refuses before
anything is copied, then the patch's own refusal, then `NoReasonRecorded`, which says in those
words that nobody recorded one and that it is a bug. **Silence after a press reads as success**,
which is the worst thing a status line can do, and the test is that no run with `Written` false
can produce an empty line rather than one test per case.

`CouldNotBeWritten` says what to do before it says what Windows said, because the system's own
words name a process and not a thing to do.

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
  values and not the total. **How it prints is below and the numbers alone are not enough**, and
  **DM-11 is the special case**: every one of its groups holds one phase
- SOFTSCAPE SCHEDULE, category Planting. Its fields are BOTANICAL NAME, which is
  PRX_Softscape Botanical Name, and COUNT (n), a Count field. DM-11 gives ALBIZIA LEBBECK 6,
  BAUHINIA PURPUREA 2, CASSIA GLAUCA 4, total 12. How it prints is the rule in `CLAUDE.md`,
  stated there and nowhere else, because the group row is what question 8 is answered from
- Two phases, Existing and Proposed. The split shows as a group row inside the printed
  schedule, not as a separate schedule. **FM-05 prints a third, Street Design**, measured on
  the 1536 report, and it is out of scope under the rule above

## How a grouped schedule really prints

Measured off the 1355 scan report, which **is not in this repository** because nothing under
`reports/` is ever committed. The numbers were recorded a round before the shape was, and the
numbers alone were not enough: a reader written to them found no subtotal at all.

DM-11-(600) SHRUBS & LAWN SCHEDULE is eleven columns wide and prints this. **Every DM-11 group
holds ONE PHASE**, which is what made it the wrong plot to learn the shape from:

```
IMAGE | # | PLANT CODE | BOTANICAL NAME | AREA  (sqm) | COUNT (n) | HEIGHT (m) | ... | L/DAY
GRASS                                                                     group heading
Proposed                                                                  phase
Pennisetum Setaceum.jpg | PEN SET | ... | GRASS: PENNISETUM ... | 35 m² | 46 | ...   species
                                                          | 35 m² | 46 | ...        the phase
                                                          | 35 m² | 46 | ...        the group
SHRUBS & GROUND COVER                                                     group heading
Proposed                                                                  phase
Bougainvillea glabra Pink Pixie.jpg | ... | 36 m² | 46 | ...                    species
Carissa macrocarpa - grandiflora.jpg | ... | 34 m² | 12 | ...                   species
                                                          | 70 m² | 58 | ...        the phase
                                                          | 70 m² | 58 | ...        the group
TOTAL                                                     | 105 m² | 104 | ...      the lot
```

A group holding two phases prints THREE rows. FM-05 GRASS, off the 1536 report, whose two
phases are Proposed and Street Design and not Existing and Proposed:

```
GRASS                                                                     group heading
Proposed                                                                  phase
  ... species ...
                                                          | 96 m² | 117 | ...       Proposed
Street Design                                                             phase
  ... species ...
                                                          | 69 m² | 84 | ...        Street Design
                                                          | 165 m² | 201 | ...      the group
```

Three things follow, and `ShrubsAndLawnRows` holds all three.

**The group heading is on its own row**, first cell only and every other cell empty, rather
than beside its numbers. A phase row sits under it in the same shape, so a structure row that
names no wanted heading opens no group.

**A GROUP PRINTS ONE SUBTOTAL PER PHASE, THEN THE GROUP TOTAL. THE PHASES A TREE LIST SHEET IS
NAMED FOR ARE THE VALUE AND THE LAST ROW IS THE CHECK.** Measured on the 0928 run over 20 mosque
plots, four groups out of four, and the third row is exactly the first two added in area and in
item count:

```
DM-16 SHRUBS & GROUND COVER   30 over 39,  54 over 69,   84 over 108
DM-25 SHRUBS & GROUND COVER   13 over 9,   228 over 286, 241 over 295
FM-05 GRASS                   96 over 117, 69 over 84,   165 over 201
FM-05 SHRUBS & GROUND COVER   361 over 450, 459 over 570, 820 over 1020
```

**Two rules came before this one and both were wrong on FM-05.** The first said the subtotal
prints twice and took the first of two. That came off DM-11, where a one phase group prints two
equal rows, and it was right on that one plot and wrong on every plot holding two phases: 30
where the group is 84, 13 where it is 241. The second took the last row, the group total, which
is right where both phases are in scope and wrote 165 and 820 on FM-05, where the second phase
is the street. **The 0928 run refused rather than writing, which is the only reason the first
rule's numbers never reached a client**, and the second rule's did reach a workbook on the 1428
and 1536 runs.

The check stands and is pointed at every phase: **the last row must equal the rows above it
added together, in item count exactly and in area to within the project's own rounding.**
Measured on the 1208 run over RCRC_NG03_EZ: FM-21 prints Existing 2 over 0, Proposed 51 over 11
and a group total of 52 over 11, and FM-22 prints 2 over 0, 80 over 46 and 83 over 46. The
counts match exactly, the areas are off by one in opposite directions, and both are correct,
because the project rounds areas to the metre, every printed area is already rounded, and a
sum of rounded numbers need not equal a rounded sum. The forty seventh pass said exactly that
about the species sum and chose to record rather than enforce, the check was enforced exactly
anyway, and it fired on correct data.

So counts are integers and get no room at all: a count that disagrees is a real fault and
still refuses. Areas get half the unit's rounding step for each row summed, two rows rounded
to the metre may be off by up to one, and the step is read off the project units through
`KpiReader.AreaUnit`, the same read the scan prints as Area unit, rounded to, never a
constant, so a project rounding to 0.01 gets a tighter room and one rounding to 10 a looser
one. **The room has no ceiling yet, on purpose.** Both measured models round to 1, a coarser
step is hypothetical, and a ceiling chosen today is a constant pretending to be a rule. In
its place, a project whose step is coarser than the metre says so at the top of the create
report, the step and the room per row in square metres, one line before anybody reads a
number, so the first project that earns one hands the team a real figure to decide a ceiling
against. The unit that gated the checks travels on `KpiCreateRun`, and a reused press carries
the held run's forward, because the notes were earned against that one. **The pane counts the
run's rounding notes beside the written cells**, one sentence saying they are in the report,
because a note only the report file holds is a note nobody reads. The detail stays in the
report. **Within the room is a line in the report, not a refusal**, the `RoundingNote` on the
`GroupSubtotal`, printed beside the group total row with the rows, the total and by how much,
so a real fault growing slowly stays visible. Outside the room still refuses, naming the room
it is outside of. A step that was not read allows nothing and says so, because a check that
cannot see its subject must not quietly widen. A group printing one row has nothing above it
to compare against and is taken. A phased group that prints no total row is taken too, with a
line in the report saying nothing checked what its phase rows add to, because taken silently
it reads exactly like a group whose total was checked and agreed. The note's and the
refusal's numbers print to six places, because two places printed a project rounding to 0.001
as off by 0 within the 0 it allows, a sentence at war with itself over a comparison the code
got right.

**Every other place printed numbers are added against a printed total was checked in the same
round and named.** The softscape species rows against the printed TOTAL and each group's rows
against its own subtotal are integer counts, exact, and right as they are. `Totalled.Adds`
compares the tool's own sum against the tool's own total, both computed from one list, so it
is a guard for a future caller rather than a live check, and its constant is shared with the
height and diameter difference detector, so it was left alone deliberately. The NOT WRITTEN
line sums integers against no printed total. And the species sum against the group's value,
recorded rather than enforced by the forty seventh pass, was computed and recorded NOWHERE,
its docstring said printed and nothing printed it, so it prints now beside the group on any
real difference, gated by the shared drift epsilon alone and printed to six places. The 0.005
that first gated it was a constant pretending to be a rounding room, the very shape this round
cured the check of, and it swallowed 169.996 against 170 whole.

**The species rows add up to the group**, 36 plus 34 is 70, so the two are held against each
other. They are not enforced, because every one of those numbers is already rounded to the metre
on the way out of Revit and a sum of rounded numbers need not equal a rounded sum.

TOTAL needs no special case in the shrubs and lawn schedule. It carries numbers, so it is not a
structure row, and its first cell holds text, so it is not a subtotal.

**The softscape TOTAL row is read, and the species rows are held against it.** It is the row
whose first cell holds that word, and its count is read off the COUNT column like every other.
The first real workbook read 31 trees where the model held 39 and nothing in the tool could say
so, because nothing read the one printed number that would have. DM-12 prints TOTAL 39 and its
eight species rows add to 39. Adding printed numbers is allowed, so a sum that does not match
the printed TOTAL refuses the write, and a schedule printing no TOTAL row is said in the report
rather than refused, because a check with no subject is not a failure of the schedule. The two
skips that were bare continues are named: a row with a name and no whole count refuses and
names the row, and a row with a count and no name is the subtotal, counted as passed over.

Never read a schedule value by cell position, which is the rule in `CLAUDE.md` and is stated
there and nowhere else. `ScheduleColumns` is what asks the heading row. What these schedules
measure is why: eleven columns wide, the first number in a subtotal row is the area and the last
is L/DAY, so both ends are wrong.

**A reader that cannot find its column refuses, naming the column and printing the headings.**
The four fallbacks stood through two audits: the botanical name off the first cell, the count
off the last whole number, the species test off the first cell and the group count off the
first cell. All four are gone. `SoftscapeRows.Read`, `ShrubsAndLawnRows.Read` and
`ScheduleGroups.Of` hand back what they read or every reason they refused, never both, in one
sentence from `ScheduleColumns.NothingNamed`. A group whose named rows cannot be counted is
still found, with the reason on it, because the group row needs no column.

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

**A digit after the number ends is a refusal, never a shorter number.** Reading as far as the
number goes turned 1,234 m² into 1. Every value the reader had met printed under a thousand, and
the two four figure values ever seen, 1161 and 3729, came off the one project, whose unit format
prints no separator. A number read short passes its own checks: a one phase group over 999
printed two rows both reading as their thousands and 1 equalled 1, and 1,200 plus 1,300 totalling
2,500 read as 1 plus 1 equals 2. Which character a project groups digits with, or uses for the
decimal, is a units setting this tool has never read, so `CellNumber` parses no separator: a
cell holding a digit past where the number ends is refused with the cell named, and 1131,72 is
refused the same way. Revit prints the unit with a superscript two, which is not a digit.

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

The header is row 3 and the total at row 93 is `SUM(B4:B92)` on both sheets. Where the species
stop is read off each sheet when Create is pressed and is in no map, because the MOSQUES
existing names ran to row 101 on 2026-09-10 against a map entry that said 83.

## What the first real workbook measured

One press of Create on DM-12 with the MOSQUES template, 2026-09-09. **The workbook is written
and correct.** These are measurements off that output file, not reasoning about it.

- **37 parts in, 37 out, 4 changed**, and the output recalculates with ZERO errors. The parks
  figure above, 45 against 44, is EXISTING PARKS and is a different template. Both stand. It
  reads 37 in and 36 out now, because the calc chain is removed on purpose
- **Excel opened it showing zeros for seven computed cells** while the inputs beside them were
  right. That is the stale cache in the patcher section above, measured on this same file
- The six values landed and the client's own formulas ran on them: **28.1 percent canopy against
  a 13 percent target, Excessive, NOT COMPLIANT.** The tool wrote no verdict anywhere. That is
  the workbook's arithmetic on the numbers Revit gave it
- **The slash case matched.** ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana
  at row 11, which is why nothing is stripped or split on the way to a comparison
- **Three species were not in the list**, all under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2
  and WASHINGTONIA ROBUSTA 1. The MOSQUES tree list holds 80 species in rows 4 to 83 and not one
  of them is Phoenix dactylifera, Washingtonia robusta, or any of the Unknown rows. Checked
  against the output file itself rather than against the map

The last one has a consequence, and the fix and the open question are two different things:

**That run read 2 existing trees where the model holds 10, and 31 in total where the model holds
39**, because the three were named and written nowhere. They are written into the empty rows now,
under the species rule above, so the counts reach the total. **Nothing in the tool may ever place
an unmatched species by guessing**: the name and the count go in and no other column does.

**What is still open is that a row written that way carries no family, no genus and no native
flag**, so the KPIs that need those cannot see it, and the client's species lists are short of
trees this project actually plants. That is for the team. `steps/log-kpi.md` carries it.

## What the first twenty plot run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 11:16, and the workbook it wrote read against
the model. These are measurements off that run, not reasoning about it.

- **The subtotal rule holds on every number it was predicted to move.** DM-16 shrubs 30 to 84,
  DM-25 13 to 241, FM-05 shrubs 361 to 820, FM-05 grass 96 to 165. Shrubs total 2517 to 3258,
  lawn 1058 to 1127
- **The date, the prepared by and the position reached the file.** The cache section read
  cached results left 0, dropped 1674, 37 parts in and 36 out with `xl/calcChain.xml` named as
  the one removed on purpose. Three species were written into empty rows on the Proposed sheet
  with the name and the count and nothing else
- **Tree List - Existing had three row ranges**: the map and the pane said B4 to B83, the
  sheet's total said `SUM(B4:B92)`, and the names ran from row 4 to row 101. Six species were
  reported as having nowhere to go, 85 existing trees between them, and four of the six sat in
  the list past row 83. The workbook said 76 existing trees where the model holds 161. Fixed
  under the species rule
- **FM-05 printed twice in one species row**, taken that round for two schedules whose names
  hold SOFTSCAPE. The 1428 run showed one schedule and two printed rows, and the 1536 report
  showed the second row under a third group, Street Design, under the rule above
- **313.5 seconds, 312.8 of them reading the model**, 20 plots at 15.6 seconds each, and 0.7
  seconds for everything after the read. STREETS ticks 78 plots, which is about twenty minutes
  at that rate. What the read does per plot is in the log. It is measured in calls and not in
  seconds, because nothing here runs Revit, and it is not changed

## What the 1428 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 14:28, the workbook it wrote, and that workbook
opened in Excel. These are measurements, not reasoning.

- **The tree list fix worked.** B84 17, B86 27, B87 19, B89 3, B99 3, so the 69 existing trees
  that went nowhere the run before are in the file, species matched went 16 to 21, and the tree
  list section proved it in four lines
- **A written row broke the canopy maths**, nine #VALUE! cells from Proposed M84 to the KPI row,
  fixed under the species rule with the height and the diameter off the schedule
- **FM-05 printed twice again** with the accounting reading one softscape schedule on all 20
  plots, which is what showed the double to be two printed rows of one schedule. That round
  read them as two rows under Proposed. The 1536 report's printed section showed the second
  under Street Design
- **Excel opened the file and did not calculate it**, every formula cell blank until Ctrl Alt
  F9, fixed with calcMode under the patcher rule
- **Every Meets KPI and Compliance cell read #NAME?** off `_xlfn.IFS`, sixteen cells, not the
  tool's doing and now counted in the report
- **Nothing in the report was what the tool read.** The last section prints every schedule
  this run read as the schedule prints it, every column aligned, with what was read off it
  and which subtotal row was taken and why, capped at 200 rows a schedule and named at the top

## What the 1536 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 15:36, the first run whose report ended with
every schedule as printed. FM-05 refused on three species printed twice under Proposed, and
the printed section showed why. These are measurements, not reasoning.

- **FM-05's softscape schedule holds three groups.** Existing at row 3, four species, subtotal
  6. Proposed at row 9, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 19, CASSIA GLAUCA 3, subtotal 32.
  Street Design at row 14, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4,
  CONOCARPUS 4, subtotal 38. TOTAL 76 at row 20
- **Its shrubs and lawn schedule holds the same third phase.** GRASS Proposed 96 over 117,
  Street Design 69 over 84, 165 over 201. SHRUBS AND GROUND COVER Proposed 361 over 450, Street
  Design 459 over 570, 820 over 1020. So the 165 and 820 the 1116 run wrote counted the street
- **DM-25 prints Existing, then Proposed, then Existing again**, in its softscape schedule
- Street Design is out of scope by Bader's decision, under the rule above, and the model will
  be corrected later. On STREETS it counts as Proposed, by the same decision, under the section
  that follows the rule

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

Two the 1355 run raised in their place are in `steps/log-kpi.md`, both about matching a species by
name, and nothing in the code picks an answer to either.

The 1521 run raised a third, which template each value of PRX_Component means. **The 1548 run
settled it.** Eleven values came off the report's own component block and the team turned them
into the table above. It is not repeated here.

## The status line moves while a run does

A scan took 123 seconds on RCRC_NG03_EZ, 104,031 elements, behind one line that did not move,
which is what a hung tool looks like, and a run over 78 street plots is minutes of the same.
`ProgressWords` in Core holds the lines and the counting, with tests, and the pane shows them:
the scan announces each section as its read begins, off the report's own numbered headings, so
Section 4 of 9, linked models, and the schedules loop counts, Sections 5 to 8 of 9, schedules,
400 of 951, 42%, said as a span because one reader covers those four sections in one pass.
Create names the plot being read, Reading DM-44, plot 3 of 18, 11%, then the writing steps name
themselves, adding up, copying the template, writing the cells, reading them back, checking the
formulas, writing the report, raised by the patcher itself through a callback so the words come
from the work rather than a narration beside it. A press that answers from the held readings
says so, Reusing the readings already held, because a two second finish after a two minute
read looks like something skipped until the screen says reuse. The report's Readings line is
the record and this is the live half of it.

**Driven by what is done, never a timer and never an estimate.** A percentage appears only
where the total is known, is floored, and is driven by a count that only grows, so it cannot go
backwards, and where the total is not known the count stands alone. The run's own end line
still comes through `Told` after every finish, refusal or throw, so the last thing on screen is
never a count that stopped moving. Everything still goes through the external event on the
Revit thread and the pane's `Moved` only sets text and lets the paint through.

**Whether the line visibly moves mid run is UNKNOWN until somebody runs it.** A dockable pane
can share Revit's own thread, and a text set from inside `Execute` then sits unpainted until
the run returns. `Moved` queues one empty job at render priority after each line, which lets
the paint through when the threads are one and costs nothing when they are not, and the log
records the question as open. **Render and never background**: waiting on a job pumps
everything queued at or above its priority, and input sits above background and below render.
Pumping at background would dispatch every queued click inside `Execute`, mid read or mid
write, and two of the pane's buttons open a folder dialog owned by no window, behind which
Revit's own ribbon stays live. At render priority the paint goes through and every queued
click stays queued until the run returns.

## Do not name a KPI control Scan Model

Scan Model is a button inside the Drawing Sheet pane. Two buttons with one name doing
different things is a trap for the production team. The KPI one is KPI Scan.
