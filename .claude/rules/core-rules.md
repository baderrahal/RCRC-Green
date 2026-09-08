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
numbers are in, so the 10 metres lives in `SectionDefaults` in the Revit project and is turned
into feet there. Revit works in feet, and passing 10 straight through would place a ten foot
section.

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

## A sheet is captured, not designed

`SheetDefinition` holds the title block family and type, the sheet size, and one placement per
view on the source sheet, each carrying a view type and a position in feet from the sheet
origin. Plain strings and numbers, the same shape as `ScheduleDefinition` and for the same
reason: capture and create are two halves that do not know about each other, and a definition
loaded from a file is then a small round rather than a rewrite.

One placement per view type. A source sheet holding the same type twice gives no way to say
which position a new one takes, and placing both would put two views on top of each other.

A schedule on a sheet is a different element from a drawing on a sheet and is placed by a
different call, so `IsASchedule` is captured rather than worked out later.

The sheet number and the sheet name are not in here at all. They come one per plot in a
`SheetRequest`, typed by the user, and `Blank` and `Complete` are different questions. Both
boxes empty is a plot nobody asked for a sheet on. One box empty is a row that was meant and is
short, and that one is refused by name and told which half is missing.

## Every count the panel shows is worked out here

`PanelSteps` holds the five steps, what each says while it is shut, whether it can be used yet
and one line saying why when it cannot. The panel draws them and formats none of them.

A summary written next to the control that shows it is two records of one fact, which is the
shape that produced the grid cell, the column count and the run report. Three times is enough.

One rule in it is worth knowing: a step below the plots is usable only when the plots step
itself is. Reading the range off the arguments alone let every step open on a panel that had
read no model at all, because the range fields still held what the last read put there. A test
caught that, and it was the code that was wrong.

## A test that reads the code back to itself proves nothing

Write the expected value out by hand. Do not work it out with the same rule the code uses.
Then break the code once and watch the test go red before trusting it.
