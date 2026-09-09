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

## The plot list is the union of three sources

A view name, a scope box, or PRX_Plot_ID. A plot that has only a scope box and some tagged
elements has no views at all, and that is the plot the team most needs to see, so it survives
into the list rather than being dropped.

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

## The grid shows a range of plots, and a tick drops any of them

160 plots down one side is not readable. `PlotRange` narrows to a prefix and a run of numbers
inside it. `PlotSelection` puts a tick on every plot in that range, all on to begin with, so
the plots in the middle nobody is working on can come out. Changing the range builds a new
selection, which is what resets the ticks.

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

## What is counted is what gets written

`ScopeBoxCounts` narrows views to a set of plots by the plot in the view's own name, which is
the rule `ScopeBoxPlan` follows inside. Two rules would mean the number the panel shows and
the number the transaction writes were different numbers, and the whole reason those counts
are on screen is that they are the same one. The Revit side calls `ScopeBoxCounts.Narrow`
before it writes, so there is one narrowing and not two.

## SectionPlacement takes the axis and the depth as required arguments

The code picks no default for either. The interface preselects ShortSide, because the default
cut is the short way across the plot. The depth is a plain number in whatever unit the box
numbers are in.

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

## A sheet is described, not captured

`SheetDefinition` holds the title block family and type, the sheet name, the view types that go
on it and how many per sheet. Plain strings, a list and a number, filled from what the user
chose rather than read off a sheet that already exists.

The sheet number is not in it. It is the one thing that differs between the sheets one
definition makes, so it comes per plot in a `SheetRequest`, and `SheetOrder` pairs a definition
with the number every plot gets for it.

`SheetLayout.For` is the maths: a title block's width and height and a count of 1, 2 or 4, back
comes the centre of each viewport in reading order. It divides the sheet evenly, so the margin
outside equals the gap between. Y counts up from the bottom, which is Revit's convention and
the reason the first row back is the top one.

More views ticked than fit is not an error. The first few are `Placed`, the rest are `LeftOff`
and named, so nothing is dropped without being said. A definition with no views is usable and
makes an empty sheet, which is a real thing to ask for.

`SheetSize` is the width and the height in feet AND which read produced them, because a size
that came from nowhere reads exactly like a size that was measured. Three sheets were made
empty on a real A1 title block. A size that is zero, negative, NaN or infinite is not a size,
and the factory turns it into `NotRead` rather than letting it reach `SheetLayout.For`.

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

`SiblingView` holds the view name, the family type, the template, the level and the three crop
settings of ONE view the model already has. `SiblingChoice.For` picks one and returns it whole.
Nothing anywhere assembles a set of settings from more than one view, and two tests go red if
anything starts to.

A run produced a view whose template read `(010) Overall Plan` and whose family type read
`(200) General Arrangement Layout`. The Revit code read both off the same local four lines
apart, so it could not have split them, and nothing recorded which view either had come from,
so the question could not be settled at all. The name is on the object now and in the report.

`ViewCrop` is the three of them together: Crop View, Crop Region Visible and Annotation Crop.
They are one object because Revit will not turn Annotation Crop on for a view whose crop is
off, so copying the second without the first does nothing. Every view the tool created had
Annotation Crop off while DM-16 and DM-14 have it on, which let neighbouring plots' section
markers draw straight through a new view.

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
