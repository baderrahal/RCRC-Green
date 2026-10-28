## The client's note holds, and it decides where two regions hold an area

**THE 14:29 RUN SETTLED IT BY COUNTING**, NG05, 156 plots, 102 workbooks:

```
of 156 plots wanting an area
  98   took it off RCRC_OUT OF SCOPE (PRESENTATION), the type the note names
   0   took it off a type the note does not name
  58   chose no region at all
```

**So the note holds on every plot that chose.** The 9 September reading that NS-19 and NS-06
carry their area on CADASTRAL LIMIT **was wrong**: those plots carry BOTH, and one column was
read. That open question is closed, by a count rather than by an argument, which is what the
run's own region table was added for.

### Bader's decision, 14 September: where more than one holds an area, take the note's type

**51 of 78 street plots wrote nothing on that run, every one of them because two regions held an
area and the tool asked which.** 51 questions on the pane, 51 clicks, and a run that has to be
pressed again after each one. Every one of the 51 offered the note's type as one of its two:

```
NS-02  CADASTRAL LIMIT 3982   OUT OF SCOPE 4295
NS-03  CADASTRAL LIMIT 6750   OUT OF SCOPE 7648
NS-04  OUT OF SCOPE 12289     CADASTRAL LIMIT 10754
```

`RegionChoice.Pick` is the whole rule, in order. **A person's pick wins**, because it is an
answer and everything below is the tool working one out. **One region holding an area answers
itself**, whatever it is called, and the note decides nothing there. **More than one, with the
note's type among them, takes the note's type.**

**THE QUESTION STAYS WHERE THE NOTE'S TYPE IS NOT AMONG THEM**, because that is still something
the data cannot settle. So does the note's type held by TWO of them at once, which no model has
shown and which the note cannot separate either, and the unchosen reason says which of the two
cases it was because they need different answers from a person.

**THE TYPE NAME LIVES IN ONE PLACE, `RegionChoice.TheNoteNames`, and nothing else holds a hard
coded type. RCRC_CADASTRAL LIMIT is written nowhere in the tool.** The test fixture carries a
third name, `RCRC_SOMETHING NOBODY HAS MEASURED`, for the pair the note does not settle, because
a test that wants the old refusal has to build a pair without the note's type in it.

**`WhyUnchosen` ASKS `Pick` RATHER THAN DECIDING AGAIN.** Two rules for one question is the
fault this repo keeps paying for and it would have bitten here immediately: the pick would
choose and the reason would still print a question, which is a report at war with the workbook
beside it.

### The report says WHO chose, and a route is not a type name

`RegionPick` carries the type and the route as one record, set where the choice is made, and
`PlotReading.ChosenRegionPick` holds it so the two cannot drift. Four routes, one column in the
region table, `how it was chosen`:

```
nothing chosen
chosen by hand on the pane
the only region holding an area
more than one held an area and this is the type the client's note names
```

**A count of picks the note made and a count a person made are two different facts about a
run**, and a column printing only the type name says neither. The column beside it still says
whether the type was the one the note names, which is what the count above was read off.

**THE PICK USED TO DROP THE PLOT'S UID2.** `PlotReading.WithChosenRegion` rebuilt the reading
with every argument but the last, which defaults to null, so a plot answered after a refusal
came back with no `PRX_Plot_UID2` and was refused a second time with `no PRX_Plot_UID2 was read
off this plot's first sheet`. That is a sentence about the model and it was about that method.
**A default that reads as a deliberate empty is how a whole link in a chain goes missing without
a word**, which this file already carries once, about the three the team types.

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

**It knows the template, and on a template that names no area cell the area is not read.** That
was STREETS, whose sheet worked the area out from the road width and the total length, and MM-03
and MM-04 are street plots both reading 12182.05561411 in the 00 link, so a reconciliation that
did not know the template would have ended the first 78 plot run asking the user to confirm an
area the workbook had no cell for, then read all 78 again. On such a template no filled region
is read, nothing about the area is refused on, and the report says the area was not read and why
rather than leaving the section empty.

**NO TEMPLATE NAMES NO AREA CELL TODAY**, since the client emptied STREETS H8, so that path is
live for nothing and every template goes down the same one. It is kept because the map can still
express a template that names none and `KpiTemplate.TakesNoArea` is what every one of those
paths asks. **It was called `AreaIsTypedByHand` and no template ever typed one**: that name was
the STREETS story rather than the rule, and a name that tells one template's story is a name
that stops being true when that template changes.

**A refused schedule read refuses the write.** A column the heading row did not name, a cell
holding a digit past where its number ends, a species row with no whole count: each travels on
the plot reading with the schedule's name, prints in red above Create, and prints twice in the
report, among the reasons and under the plot. A refused read used to come back as a list of
nothing, which read as a plot whose schedule listed no species.

**A throw on one plot's WRITE names the plot too, audit 4 finding 66.** `GuardedRead` guarded the
READ half and the per plot WRITE loop, added when a workbook became one plot, had no equivalent,
so a throw on plot 100 of 154 unwound out of `Create` to `Run`'s catches, which say Revit refused
that with no plot named and **write no report at all**, leaving 99 workbooks and 99 PDFs in the
client's folder tree with no record of which plots those were. `GuardedWrite` wraps the `OnePlot`
call, records a `PlotOutcome.WroteNothing` carrying the exception's type and message, and the run
carries on and the report is written either way.

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

**A THROW READING WHICH PLOT A SCHEDULE FILTERS ON IS A REFUSAL, NOT AN EMPTY STRING**, audit 4
finding 65. `PlotFilteredOn` caught two exception types and answered both with an empty string,
and an empty string means the schedule belongs to NO plot, so the schedule was skipped, the plot
read as holding no softscape schedule at all, the two of a kind guard could not fire because the
schedule was never counted, and the reconciliation said the schedule listed no species, which is
a sentence about the MODEL. `SchedulePlotRead` carries the schedule's name, the exception's type
and its message the way `GuardedRead` already does, the plot's files are not written off a half
read, READY reads NO with the schedule's name, and the schedule half of the plot list NAMES what
it could not read rather than dropping it. **The rule and the words live in Core where a test can
reach them**, because the throw itself is Revit's and nothing here can make Revit's own
`ScheduleDefinition` throw.

**A MALFORMED SHEET PART IS A REFUSAL IN ALL FOUR READERS**, audit 4 finding 67. Five classes
open an .xlsx and only the two that WRITE caught `System.Xml.XmlException`. A template whose
`xl/workbook.xml` is fine and whose tree list sheet part is malformed passes the peek, is
recognised, is listed and is offered, and at the press the parse threw past every catch in `Run`
to the catch of everything, so the pane said the request failed and was stopped here, naming no
template, no file and no plot, **with no report written at all**. `SpeciesList`, `LabelledCells`,
`StreetReference` and `TotalCanopyColumns` each catch it now and return the refusal each already
had words for, naming the file and the sheet, and the rest of the press carries on. A truncated
download or a file another tool has rewritten is how a sheet part goes bad, and the peek does not
read that part.

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

## A run with no link loaded says so before it says anything else

**Measured on the first STREETS run, NG05 at 16:06, 78 plots.** All 78 contributed nothing.
All 156 schedules printed one row, the header, and no body, and the group rows read none on
every one of them. The scan from the same session says six link instances and NONE LOADED. The
plants are in the linked component models, which is the rule this file already carries, so with
no link loaded a schedule has nothing to list, and ST-05-(600) SOFTSCAPE SCHEDULE opens empty on
screen. **Every number the tool printed was right and every plot carried its own reason.** What
it never said was the one thing that explains all 78 at once, and a 2,292 line report opening
with a reconciliation of nothing is a tool watching a person work out what it already knew.

`LinksLoaded.Of` is the whole rule. It is a NOTE and never a refusal, because a model with no
link loaded is a legitimate thing to open and the team may be working on the host alone. Five
states, one line each. The links were not read at all, which claims nothing either way. The
model holds no linked model. No link is loaded, with the count and every name. Some loaded and
some not, with the count and the names of the ones that are not. And every link loaded, which
is the one state worth saying nothing about.

**It is at the TOP of the checklist report, above the reconciliation**, through `TheLinks` in
`KpiCreateReport` sitting between the clock and the first step, with a test asserting the
warning's position is before RECONCILIATION's rather than asserting both are present.

**It is on the pane too, before the press**, above Create beside the refusal line, off
`KpiPlotFacts.Links`, which the plot read fills from the host document's own link instances. So
the one thing that would have made the 16:06 run pointless is on screen before the twenty
minutes are spent rather than in the file afterwards.

**And the reconciliation counts what was FOUND, not only what was left out.** Every count it
carried read green over that run: 78 ticked, 78 read, 156 schedules found, nothing refused and
nothing missing, because a schedule that printed no body is a schedule that was read. Two
counts cannot do that. `Reconciliation.GroupRowsFound` is how many group rows the whole run
found and `SchedulesWithABody` how many of the schedules printed one, said as a count of the
schedules printed. The 16:06 run reads 0 group rows and 0 of 156 with a body, and no wording
makes that look like a run that worked.

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

### CONFIRMED BY BADER, 14 September, and the two things beside it are not

**The client confirmed it: STREET DESIGN AND PROPOSED ARE BOTH PROPOSED on STREETS.** That is
what the tool already does through `KpiTemplate.GroupsCountedAsProposed`, and it has run. ST-05
read Existing 369, Proposed 2 and Street Design 68, and 2 plus 68 went into Tree List - Proposed
as 70, against the schedule's own TOTAL of 439. **NOTHING IN THE CODE CHANGES.** What changes is
what the line rests on. **A rule the client has confirmed reads differently from one the tool
inferred**, and until 14 September this was a decision made in September that nobody had checked
since.

**The two things that travel with it are NOT confirmed**, and they are written out here so they
stay visible rather than being carried along by the confirmation above.

- **On every template that is not STREETS, a Street Design group is still LEFT OUT and named.**
  That is Bader's decision of 10 September and the client has not been asked about it. DM-16 and
  FM-05 are where it fires today, both on MOSQUES
- **A group named anything other than Existing, Proposed or Street Design is out of scope on
  EVERY template, STREETS included.** No tree list sheet is named for it and no decision takes
  it, so it is left out and named. Nobody has confirmed that either, and no such group has been
  measured on any model

**A confirmation covers what was asked and nothing sitting next to it.** All three of these were
one paragraph in this file and the client answered one of them.

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

