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

## Every view type is a column, and hiding is the exception

Adding columns one at a time was the wrong way round. Someone opens the panel to find out what
is missing, and an empty grid answers nothing. `GridColumns` starts with everything shown and
counts what is hidden.

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

## A test that reads the code back to itself proves nothing

Write the expected value out by hand. Do not work it out with the same rule the code uses.
Then break the code once and watch the test go red before trusting it.
