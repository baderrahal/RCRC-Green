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

## What is counted is what gets written

`ScopeBoxCounts` narrows views to a set of plots by the plot in the view's own name, which is
the rule `ScopeBoxPlan` follows inside. Two rules would mean the number the panel shows and
the number the transaction writes were different numbers, and the whole reason those counts
are on screen is that they are the same one. The Revit side calls `ScopeBoxCounts.Narrow`
before it writes, so there is one narrowing and not two.

## A number that is not a number still looks like an answer

`PlotBox` refuses a bound that is NaN or infinite when it is built. An infinite bound survives
an ordering check and then turns every centre into NaN, which comes back looking like a
placement rather than a failure. Geometry read from a model is worth checking at the door.

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

The rest of these rules moved, byte for byte, to `docs/rules/` on 2026-10-28. `docs/rules/index.md` holds one line per rule id and the file it lives in. Read a rule there by its id, and only its part. A new rule goes into the part for its topic, with its line in the index.
