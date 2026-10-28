## The plot list is the union of four sources

A view name, PRX_Plot_ID on a view, a scope box, or PRX_Plot_ID on an element, gathered by
`PlotRegistry` in Shared. A plot that has only a scope box and some tagged elements has no
views at all, and that is the plot the team most needs to see, so it survives into the list
rather than being dropped. The sentence the panel says on a model with no plots names all
four, and lives once, in `PanelSteps`.

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

**The range decides the list and the model decides the ticks.** From and To offer 01 to 99
on every model, digits only, because the team works across models. The LIST holds every sub
plot the range covers, in the model or not, and each line says which it is: not in the
model, no scope box, or a view count. What the model holds starts ticked and what it does
not starts listed, because a single tick on a plot would otherwise queue a run over 96 sub
plots nobody has looked at, and both are one click apart.

This used to filter the model's own list, so widening the range changed nothing on screen
and the user reported the range as not working. They were right, and the rule it was
protecting is narrower than it was written. **The rule is that the TOOL never invents a
plot.** A sub plot somebody types into a range is theirs. DM-02 settles it: it holds no
views and no scope box, and the team has already made its sheets. What still cannot happen
is a plot prefix the model does not hold being offered at all, and a sub plot outside a
ticked plot's range being ticked.

The model's own spelling wins where it has one, matched on the number, so generating DM-02
beside a model that spells it DM-2 cannot give two lines for one sub plot. A sub plot
numbered above 99 falls outside every range, which is the known cost of two digit ends.
`Matching` is what the search box narrows to and what All and None act on, so the sweep and
the single tick mean the same thing, the rule `BulkMarking` already follows.

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

## CodeFor compares against what the file stored

`TitleBlockSettings.CodeFor` does not trim its arguments. The file deliberately keeps every
space a name carries, because a type in this model is called `LOD /  HARDSCAPE SCHEDULES` with
two spaces, so trimming here compared a trimmed argument against an untrimmed stored value and
a pairing on a name with an edge space could never be found. Proved with a test before it was
changed, and the test stays.

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

