---
paths: src/RcrcGreen.Core/**
---

# The rules Core holds

These load when something in `RcrcGreen.Core` is being touched. The one that matters most is
in `CLAUDE.md` and is repeated here because it is the whole reason this project splits in two:
**Core must never reference the Revit API.**

## A view type is the code and the view name together

Code 010 appears twice in the real model with two different view names, so the code on its own
does not say which view something is. `ViewType` holds both and the grid columns are built
from it.

Real names, which is where that comes from:

```
DM-41-(010) Location Key Plan
DM-41-(010) Overall Key Plan
DM-41-(200) General Arrangement Layout
DM-41-(400) Landscape Cross Section
PF-12-(200) General Arrangement Layout
```

## The plot list is the union of four sources

A view name, PRX_Plot_ID on a view, a scope box, or PRX_Plot_ID on an element, gathered by
`PlotRegistry` in Shared. A plot that has only a scope box and some tagged elements has no
views at all, and that is the plot the team most needs to see, so it survives into the list
rather than being dropped. The sentence the panel says on a model with no plots names all
four, and lives once, in `PanelSteps`.

## A view fills the cell named by its own name and by nothing else

The row plot and the column type used to come from two different places with no check that
they agreed. PRX_Plot_ID gave the plot, the name gave the type, and a view named
`DM-12-(200) General Arrangement Layout` carrying PRX_Plot_ID `DM-11` filled the DM-11 cell.
Deleting every DM-11 view then left that cell showing a view, and clicking it opened one
belonging to DM-12.

`ViewReading` is the one place that decides this. The plot on a row can still come from the
parameter, because that is what recovers the 1,269 views whose names do not parse. The plot on
a cell comes from the name only. A disagreement between the two is counted and shown rather
than resolved silently, because it is a model problem somebody has to fix.

The rule behind it: the grid must never show a view as existing when it is not in the model.

## Step 1 is a tick list of plots, and the grid shows every ticked sub plot

160 sub plots down one side is not readable, and one prefix at a time could not say DM and
FP together, which a real run wants. `PlotTickList` in Core/DrawingSheet is step 1 whole:
every two letter prefix the model holds is a plot, several tickable at once, a ticked one
carries a From and To over its sub plots and a tick per sub plot in that range, and one
line per sub plot shows the identifier and the stem its sheet numbers take. One run covers
every ticked sub plot on every ticked plot, in plot order however they were ticked.

It composes the Shared pieces rather than restating them: `PlotRange` narrows and orders,
and each ticked plot carries its own `PlotSelection`, so the old rules still hold. Ticking
a plot takes its whole span with everything ticked, changing its range rebuilds its
selection with everything in the new range ticked, unticking a plot forgets its range and
its ticks, and nothing outside the model can be ticked at all.

**The range is free and the list is not.** From and To offer 01 to 99 on every model,
digits only, because the team works across models and a range filled from the open one
could not be set up for the model it is meant for. The LIST under them still holds only
the sub plots this model really carries inside the range, so a free range picker is not an
invented plot, it is two numbers, and the count line says how many of the range exist
here. A sub plot numbered above 99 falls outside every range, which is the known cost of
two digit ends. `Matching` is what the search box narrows to and what All and None act on,
so the sweep and the single tick mean the same thing, the rule `BulkMarking` already
follows.

## A grid cell has four states and carries its sheet number

`SheetCellState` is exists, exists on no sheet, missing and marked. The middle one is not a
detail: the first real model holds 2,430 views on no sheet against 953 that are placed, so a
view nobody has put on a sheet is the ordinary case and a grid that drew it as done was
hiding most of the work. `SheetGridCell.IsInTheModel` answers the question both existing
states share, whether the cell can be marked and whether clicking it opens something, so no
call site compares against two states and gets one of them wrong later.

The cell also carries the NUMBER of the sheet its view sits on. That is what says at a glance
that a sub plot is running on copy numbers, which every sub plot but DM-11 does on the first
real model, and a count of placed views could never say it. It rides on
`PlotViewPresence` from the read rather than being looked up beside the grid, because the
cell that shows the number and the cell that decides the state are one cell.

`PanelSteps.CellInWords` and `LegendInWords` are the only wording for those four states. The
legend and the tooltip were two lists written out next to the controls that drew them, which
is how a square comes to mean one thing in the key and another under the pointer.

## One state, drawn again, never two

`GridColumns` is the only record of which view types are ticked. The panel draws the whole
list from it every time it changes rather than letting a tick box remember its own state.

That is not tidiness. The count and the list disagreed on a real model, and they could because
they were two representations of one fact kept up to date by two different paths. A click that
failed to reach the model left the box moved and the count where it was, with nothing that
would ever bring them back together. Anything that changes what is ticked goes through
`GridColumns` and then the interface is drawn again.

Nothing is ticked when the model is first read. 84 types with all of them on is a grid too
wide to read and a hidden count that says nothing.

## Inventing a view type is allowed. Inventing a plot is not

A view type the user wants to create is what this tool is for, so `GridColumns.Adding` exists
and an added type draws missing on every plot, which is correct. A plot is different. Every
plot the tool offers or acts on comes from the model, and nothing anywhere can add one.

## A definition sits between reading a schedule and writing one

Six of the things under a plot are schedules, and they sit under Schedules and Quantities in the
project browser rather than under Views, which is why a scan that walks views alone finds none
of them.

`ScheduleDefinition` holds the category, the fields in order, the filters and the link setting
as plain values. Capture fills one in from a schedule that exists, create builds one in the
model, and duplicating is the two run back to back.

Writing a duplicate-the-nearest routine instead would have been shorter and would have left
the tool useless on a project holding no schedules, which is the next one it will be pointed
at. Loading a definition from a file is then a small round. Built the other way round it is a
rewrite.

`ForPlot` changes only the filter rule whose value is a plot identifier. Every other rule is
carried across untouched, because HARDSCAPE and SHRUBS AND LAWN are both category Floors and
that second rule is the only thing telling them apart. Field names are copied exactly,
including the one spelled PRX_Furniture Lenght in the model, or the field is not found.

**A filter value carries the kind it has to go back as.** `FilterValue` is text, a whole number,
a number or an element reference, and each goes back to Revit as the kind it came out as. Flattening
them all to a string lost two schedules: both filter on PRX_Included In Budget equals Yes, which
is a Yes/No parameter Revit holds as the integer 1, and handing the word back was answered with
"the filter value is not valid for the field and filter type". Only a text value can name a
plot, so a whole number is never mistaken for one and swapped.

**A field carries what kind of field it is.** `ScheduleFieldEntry` is a parameter or a
calculated field, meaning a formula, a percentage, a count or a combined parameter. A calculated
field is defined inside the schedule that holds it, so Revit never offers it to a new one and no
name matching will find it. Three schedules came out short of one and the report blamed the
category, which sends somebody to look in the wrong place.

**The category is a number, not a name.** `CategoryBuiltInValue` is Revit's own number for it.
KERBS is built on Slab Edges, a name lookup found nothing, and the schedule was refused with
"this model has no category named Slab Edges" on a model that has it. `CategoryName` is kept for
the report only.

Reading a schedule as PRINTED is a different job and Core does that too, in `ScheduleRows` and
`ScheduleGroups`. **Never read a schedule value by cell position**, which is the rule in
`CLAUDE.md`, stated there and nowhere else.

## A sheet number is built from the plot identifier, never proposed from the model

The scheme is the user's, set after the marker scheme before it proved wrong twice over:
the user had already said the numbering must be automatic, and a per-plot dropdown made
step 1 long. The view code, then the plot identifier with its dash dropped, then A, B, C
within one code in sheet order, and no letter when the code holds a single sheet. DM-42
reads 010DM42A TITLE SHEET, 010DM42B LIST OF DRAWINGS, 200DM42 GENERAL ARRANGEMENT LAYOUT,
400DM42 LANDSCAPE CROSS SECTION, 600DM42A HARDSCAPE SCHEDULES and 600DM42B SOFTSCAPE
SCHEDULES. Nothing is set, nothing is reserved and nothing runs out, because the
identifier is in the number and no two plots share one. Nothing is read off the model's
own numbers either, which are copies on every plot but one of 160. `StemOf` on
`SheetNumberRun` is the one rule turning DM-42 into DM42, and it answers empty for
anything that is not a plot identifier, so the old scheme's markers cannot creep back in
through a caller. The whole marker family, `MarkerChoices`, `PlotMarkers`,
`PlotMarkerFile`, `MarkerLedger` and the store with its plot-markers.txt, is deleted
rather than left beside this, because two records of one fact is the shape this repo
keeps paying for.

`SheetNumberRun` builds one code's numbers for one plot. Occupied slots come off the
plot's own numbers plus everything typed on the panel, a bare number holds the first
letter's place, and the next letter continues after the highest, so a second run continues
rather than collides: 600DM42A and 600DM42B in the model give 600DM42C, and a bare 200DM42
gives 200DM42B because renaming the model's own sheet is not this tool's to do. A number
under the old marker scheme, 010QE or 010001A, does not start with the new front and holds
no slot, so old sheets are never renumbered and the two schemes sit side by side on a real
model, the user's decision rather than a fault. A DM-42 number starting with DM-4's front
carries a digit where a sheet letter would sit, so it holds no slot on DM-4's run either.
The exact collision is still caught: a built or typed number the model already carries is
refused by `SheetNumbers.FaultIn` under the box and by the run's own read at Run. A sheet
whose views carry more than one code gets `SheetNumberProposal.Nothing` with the reason,
since picking either code would be a guess, and a sheet with no views gets its own words,
because there is no code to front the number.

## A sheet is named from the saved table, and upper casing is only the fallback

`SheetNameSettings` holds which sheet name goes with which view type, the user's own file
first and the shipped `sheet-names.txt` second, the same two file rule as the title blocks.
The old rule, the view name upper cased with the code removed, came from three examples
that happened to match and four of DM-11's eight sheets disprove it: OVERALL KEYPLAN,
PROJECT LOCATION KEY PLAN, HARDSCAPE SCHEDULES and SOFTSCAPE SCHEDULES all differ from
their view names. `NameFor` resolves once, on `PlannedSheet.ProposedName`, so the rows, the
letter order and the run read one record, and a name typed over a proposal saves the
pairing. A type neither file holds falls back to `SheetNaming.FromView` and the panel says
derived beside it. This is also what made unlisted names ordinary: with the table, a
sheet's name and `SheetOrder`'s words are the same words.

At Run, `RunPlan` checks every row's number against the sheet numbers read off the model
as the run is worked out, not against the panel's last snapshot. Three runs in a row asked
Revit for numbers a previous run had created, because the only check lived on the panel and
its read was older than its own write, and every refusal arrived from Revit inside the
transaction instead of on the plan.

`NewViewSetups` holds the three answers for a view type no view in the model carries: the
view family type, the view template and the level, each picked from what the model holds,
never invented and never defaulted. `Missing` names what is still unanswered, the level
only for a plan kind because a section is cut from a box, and `SectionTypesAmong` is the
one routing rule both the panel's preview and the run's handler ask, so a type cannot be
planned as a plan and written as a section. `NewViewFamilies` narrows the dropdown to the
kinds a view can be created under at all, the four plan kinds and Section.

`SheetOrder` holds the team's sheet order, nine names, and it is also the letter order:
010001A is TITLE SHEET and 010001B is LIST OF DRAWINGS because of the list. Order by the
code first, then the list within a code, then the ticked order for anything the list does
not hold. A sheet with no views has no code and sorts ahead of the coded ones, because the
one such sheet the team makes is the title sheet and it opens both measured sets. The
position forgives case and edge spaces and nothing else, so OVERALL KEY PLAN with the space
reads as unlisted rather than being silently matched to OVERALL KEYPLAN.

`FaultIn` and `Problems` answer the same question twice over: the line under one box, and the
count in the run summary. One place decides, so the panel can never say a number is fine while
the summary counts it.

Two ways to be refused. A sheet in the model already carries it, or two of the sheets this run
would make carry it, which is the same fault a second later. Two separately described sheets
asking for one number clash as hard as two rows of one sheet do, because they go into one
model. Already in the model is said first when both are true, since the model is the one
somebody goes and looks at.

## What is counted is what gets written

`ScopeBoxCounts` narrows views to a set of plots by the plot in the view's own name, which is
the rule `ScopeBoxPlan` follows inside. Two rules would mean the number the panel shows and
the number the transaction writes were different numbers, and the whole reason those counts
are on screen is that they are the same one. The Revit side calls `ScopeBoxCounts.Narrow`
before it writes, so there is one narrowing and not two.

## SectionPlacement takes the axis, the depth and the cut length as required arguments

The code picks no default for any of them. The interface preselects ShortSide, because the
default cut is the short way across the plot. The depth and the cut length are plain numbers
in whatever unit the box numbers are in.

`SectionCutLength.Metres` is 18.2374, the team's decision, and it is the only record of that
number. The cut used to run the whole width of the plot's scope box, which is 36.4747 metres
on NS-32, and on a real sheet the viewport was still far wider than the drawing area, so they
halved it. The line is CENTRED on the box now and is the length it is given, so a box ten
times the size gives the same line. `SectionDefaults` converts it to feet at the Revit
boundary, beside the depth.

`SectionDepth.Metres` is 1, the team's decision, and it is the only record of that number.
`SectionDefaults` in the Revit project reads it and converts, because Revit works in feet and
passing 1 straight through would place a one foot section. Holding a second copy of the metres
there is the shape that has been the bug five times here.

It used to come off the sibling section, on the reasoning that the model is the better answer.
The model has no answer. Four real sections read 3.0480, 3.0480, 5.0199 and 42.1054 feet, which
is 0.93, 0.93, 1.53 and 12.83 metres, so whichever sibling happened to be picked decided the
depth. `SectionDepthChoice` and the far clip on `SiblingView` are both deleted.

`Lengths` is the one place feet turn into metres or millimetres, because a report printing a
number of feet with the word metres after it is the same class of fault as a report that says
created and not created.

## A number that is not a number still looks like an answer

`PlotBox` refuses a bound that is NaN or infinite when it is built. An infinite bound survives
an ordering check and then turns every centre into NaN, which comes back looking like a
placement rather than a failure. Geometry read from a model is worth checking at the door.

## What the run intended and what it did are two objects

`RunPlan` is the intention. `RunOutcome` is what happened. The report reads the outcome for
every counted section and reads the plan for exactly one line, the one beginning This run would
make.

That is not tidiness either. The first real run listed four plan views under PLAN VIEWS and the
same four under NOT CREATED, REFUSED BY REVIT, while the panel said nothing was created.
Nothing was. The created sections were printing `plan.Items`, which is the list of things the
run set out to make. Two records of one fact again, and the third time it has been the bug.

So something reaches a created section only by being handed to `RunOutcome.Made` after the call
that made it has returned. A `RunRefusal` carries the same `Name` string a `RunItem` would,
which is what lets `BothWays` be a comparison rather than an argument about two naming schemes.
A report where that list is not empty prints a heading saying it is a bug in the tool, because
a file that contradicts itself has nothing else in it worth believing either.

Needing attention is not the same as not being created. A schedule short of a column is in the
model and it is wrong, and counting it as not created would be a second lie.

## A sheet is described, not captured, and the views divide

`SheetDefinition` holds the title block family and type, the view types that go on and how many
per sheet. No sheet name: it used to carry one typed name for every sheet it made, which put
GENERAL ARRANGEMENT LAYOUT around a location key plan. `SheetDivision.Of` divides the views, in
the order they were ticked, into as many `PlannedSheet`s as they need, so six views at two per
sheet is three sheets and no view is ever left off. The old `Placed` and `LeftOff` split and
the empty sheet a viewless definition used to make are superseded by that.

Each sheet the run makes is a `SheetToMake` row: one plot, one slice of the views, one name and
one number, each remembering whether it was generated or typed so the report can say. A sheet
holding one view is named by `SheetNaming.FromView`, the view name upper cased with no code,
because the code is already the front of the number. One holding more is typed, and
`PlannedSheet.WhyNothingIsProposed` says so next to the empty boxes. A row short of a name or a
number is refused by name, and `SheetBatch` counts what one definition really makes.

`SheetLayout.For` is the maths: a `DrawingArea` and a count of 1, 2 or 4, back comes the
centre of each viewport in reading order. It divides that area evenly, so the margin outside
equals the gap between. Y counts up from the bottom, which is Revit's convention and the
reason the first row back is the top one.

**Two sit one above the other.** Side by side is what the first sheets did and the two views
overlapped, because each was wider than half the drawing area, which a landscape drawing on
a landscape sheet always will be. Four still makes a two by two grid, because halving both
is the only way to get four cells.

**A view is measured before it is placed, and one that does not fit is carried rather than
laid over its neighbour.** `SheetFit.Of` takes the drawing area, the count per sheet and each
view's size on paper, and hands back the sheets the views really divide into: each view into
the next cell it fits, a view that fits nothing starting a fresh sheet, and one too big even
for a cell of its own placed alone so it covers nothing and named. Nothing is ever dropped.
`ViewOnPaper.NotMeasured` is the honest answer for a schedule, whose size is not known until
Revit has drawn it, and it fits whatever it is put in because nothing else can be said. The
writer reads a view's size off `View.Outline`, which needs no viewport, and numbers a carried
sheet with `SheetNumberRun` over the numbers the document holds at that moment. A carried
sheet whose views do not share one code cannot be numbered without a guess, so it is not
made and its views are named in the report instead.

**It divides the drawing area and not the whole sheet.** `DrawingArea.InsideTheTitleBlock`
takes the title strip down the right hand edge off the width, because that is not somewhere
a view may sit, and the first real run centred every schedule across it. **How wide the
strip is cannot be read off a title block**: Revit gives a placed block Sheet Width and
Sheet Height and nothing else, and where the strip begins is drawn inside the family. So
`TitleStripAcross` is a fifth, the tool's own setting, measured by the team on the run of
2026-09-11, and `InWords` says whose setting it is in the report the same way `SectionDepth`
and `AnnotationCropChoice` do.

**A schedule on a sheet is placed by its top left corner and a viewport by its centre.**
Both were handed the centre this maths works out, so on that run every plan view landed
correctly and every schedule landed half its own size right and down. 010QA measured it:
207.4 by 187.4 mm asked for 420.5 by 297.0 came back centred on 522.1 by 203.3, which is
93.7 low and exactly half its own height. `CornerPlacement` holds the correction and the
writer applies it after the placement, because how big a schedule comes out is not known
until Revit has drawn it. Nothing moves a viewport: those were right, and a viewport's
bounding box takes in the view title under it, so correcting one against its box would move
a placement that is already correct.

`SheetSize` is the width and the height in feet AND which read produced them, because a size
that came from nowhere reads exactly like a size that was measured. Three sheets were made
empty on a real A1 title block. A size that is zero, negative, NaN or infinite is not a size,
and the factory turns it into `NotRead` rather than letting it reach `SheetLayout.For`.

`ViewportRecord` is one placement in plain numbers: sheet, view, the view's own scale,
centre, size and the sheet's size, said in millimetres, with the viewport type it was placed
in and the scale as the Properties panel shows it. The run records one per placement and the
scan reads the same shape off sheets the team made, so the two can be held against each
other. A sheet has no scale of its own: what a sheet shows under Scale is a readout of the
views placed on it, and each view's comes from its template, so nothing anywhere sets one.

**A scale can read Custom over a number the report prints plainly.** DM-11-(200) General
Arrangement Layout reads Custom with a Scale Value of 250, under a template named for 250,
and the report said 1:250 and nothing more. Neither is wrong: `View.Scale` is the ratio and
the API documentation is plain that a value Revit does not hold in its own list of scales is
applied as a custom one, so 250 is the number in both places and Custom is the label on a
value the list does not carry. `ScaleInWords` prints both when they differ and once when
they do not, because a report and a model that differ by a word are the same class of fault
as a report that says created and not created.

## Every count the panel shows is worked out here

`PanelSteps` holds the five steps, what each says while it is shut, whether it can be used yet
and one line saying why when it cannot. The panel draws them and formats none of them.

A summary written next to the control that shows it is two records of one fact, which is the
shape that produced the grid cell, the column count and the run report. Three times is enough.

One rule in it is worth knowing: a step below the plots is usable only when the plots step
itself is. Reading the range off the arguments alone let every step open on a panel that had
read no model at all, because the range fields still held what the last read put there. A test
caught that, and it was the code that was wrong.

## The settings a new view takes travel together on one object

`SiblingView` holds the view name, the family type, the template, the level, the three crop
settings and the viewport type of ONE view the model already has. `SiblingChoice.For` picks
one and returns it whole. Nothing anywhere assembles a set of settings from more than one
view, and two tests go red if anything starts to.

A run produced a view whose template read `(010) Overall Plan` and whose family type read
`(200) General Arrangement Layout`. The Revit code read both off the same local four lines
apart, so it could not have split them, and nothing recorded which view either had come from,
so the question could not be settled at all. The name is on the object now and in the report.

`ViewCrop` is the three of them together: Crop View, Crop Region Visible and Annotation Crop.
They are one object because Revit will not turn Annotation Crop on for a view whose crop is
off, so copying the second without the first does nothing.

**Only two of the three are copied.** `AnnotationCropChoice.ForAPlanView` says on, always.
Copying it was tried for one round and did not work: the report showed DM-11-(010) Overall Plan
set up from PL-17-(010) Overall Plan, which has it off, so the new view inherited the fault. The
model disagrees with itself, so there is nothing there to copy. This is the same shape as
`SectionDepth`: the team named a value, the tool applies it, and the words say whose it is so
nobody reads it as something found in the model.

`ViewCrop.CopiedInWords` prints the two that really were copied. Printing all three in the
setup line would read as though the annotation crop had come off a view.

**A section swaps which one is the tool's own.** `SectionCropChoice.ForASection` says Crop
View is on, always, because the crop region is the one thing bounding what a section draws
and it is the very box the writer computes from the plot's scope box. Copying the flag off
the sibling switched that bound off four calls after it was computed, and the run of
2026-09-11 measured the cost: a 13250.5 mm viewport on an 841 mm sheet and other plots'
plans showing the model's unbounded section markers. The region visibility and the
annotation crop are still the sibling's on a section, `ViewCrop.CopiedForASectionInWords`
prints those two, and the setup line picks its crop clause by the sibling's kind so it never
prints a flag as copied that the tool has just overridden.

## Which title block a view type's sheets are made on

`TitleBlockSettings` merges two files, the user's own first and the shipped defaults second,
and hands back one pairing per view type with which file it came from. Eleven were picked by
hand on the run of 2026-09-11 and the same eleven would have been picked again for every
plot.

Three rules hold it up. **Only what the user set is written back**, so this version's
defaults never freeze into their file. **Views that disagree about the title block are both
named and neither wins**, the same rule the plot on a cell and the schedule kinds follow.
**A title block the settings name and the model does not hold is not an error**, because the
settings are shared across projects, and `WhyUnset` says so.

`TitleBlockSettingsFile` is the format: tab separated, four fields, only the code trimmed. A
type in this model is named `LOD /  HARDSCAPE SCHEDULES`, with two spaces, so a format that
tidied its fields would produce a name the model does not hold. A line that is not four
fields is kept with its number rather than dropped, because a settings file one line short
reads exactly like one that never had the line. The code and the view name are separate
fields, so the `(010) Overall Plan` format lives on `ViewType` and nowhere else.

## A preset is steps 2 and 4, and never step 1

`Preset` holds a name, the view types ticked in step 2, and the sheet definitions described in
step 4, each one a title block, a views per sheet and its views in the order they go on. **It
holds no plot, no sub plot and no sheet number.** Those are what changes between one run and
the next, and a preset carrying them would fill step 1 with last week's plots and put a number
on a sheet another plot already has.

`Presets` merges two files the same way the title block settings do, the user's own over the
shipped one of that name, and only what the user saved is written back. `PresetFile` is the
format: tab separated, the first field saying what the line is, `preset`, `type`, `sheet` and
`on`. Every line belongs to the `preset` line above it and every `on` line to the `sheet` line
above it, so a record with nothing above it to belong to is kept as a line that could not be
read rather than attached to the wrong preset.

`PresetFit` is what one model can honour. **The part that fits is filled in and the part that
does not is named.** A view type or a title block this model does not hold is ordinary, because
a preset is shared across projects, so refusing the whole preset over one missing schedule
would make it useless on every model but the one it was saved from. A view the model lacks
comes off the sheets it was on and is named once, under the types, rather than twice.

`PresetFilling` says which preset filled the two steps, and whether they still say what it
says. **It compares rather than remembering.** A tick, a sheet added, a sheet removed, a title
block picked and a views per sheet changed can all move them, and the flag for the sixth one is
the one nobody sets. `Preset.SameAnswer` is that comparison, and the name and the file a preset
came from are no part of it.

The shipped `install/presets.txt` is the six sheets of the DM-11 run, its view types grouped by
the title block `install/title-blocks.txt` pairs each with. Five definitions, six sheets, one
view on each. Two shipped files are two records that have to agree, so a test reads both and
fails if a preset ever names a block its own pairing does not.

## Which family type a view type is really built with

`FamilyTypesInUse.Of` counts, per view type, every view family type in use and how many views
use each. A view type with more than one has no answer for a new view to copy, and whichever
sibling gets picked decides it.

Three (010) views were created in one run with three different family types, each copied
faithfully from a different sibling. That is the model's problem, so this counts and picks no
winner. Choosing the most used one would put an answer on it that the team never gave, and the
disagreement would stop being visible.

## Marking in bulk follows the rule a single click follows

`BulkMarking` hands back only cells a single click on the grid would have marked: missing, and
on a ticked plot. Two rules would mean the sweep and the click meant different things, which is
the shape that has been the bug five times here.

An unticked plot is left out because the run does not act on it, so a sweep that marked one
would leave marks the run silently drops. A single click on one square is still allowed
anywhere, because it is a deliberate act rather than a sweep.

The words are here too. `InWords` and `ClearedInWords` are what the status line says, so the
panel formats no count of its own.

## A column header is as short as it can be and no shorter

`GridColumnLabels.For` gives each column the code alone, and where a code is shared, the code
plus as many leading words of the view name as it takes to tell the sharers apart. Code 010
appears twice on the real model with two different view names, which is why `ViewType` holds
both and why a code on its own cannot be the answer.

Where one name is the whole start of another, nothing shorter than the full name separates them
and the full name is what the header carries. A header that lies by half is worse than a wide
one.

## A test that reads the code back to itself proves nothing

Write the expected value out by hand. Do not work it out with the same rule the code uses.
Then break the code once and watch the test go red before trusting it.
