---
paths: src/RcrcGreen.Revit/**
---

# Writing a command or a panel

These load only when something in the Revit project is being touched. Everything else lives
in `CLAUDE.md`.

## The panel reaches Revit through one door and no other

`DrawingSheetPanel` is modeless. Its code runs whenever Windows raises an event, which is
almost never a moment Revit will accept an API call. Every single thing it wants doing goes
through `DrawingSheetRequestHandler` and the `ExternalEvent`.

Nothing in the panel may name a `Document`, a `Transaction`, a `FilteredElementCollector` or
an `ElementId`. If a new feature seems to need one there, it needs a new request on the
handler instead. This is the rule the panel is built around and the one that breaks Revit
hardest when it is bent.

The panel holds a `DrawingSheetSnapshot` of plain values and no Revit object at all. That is
what lets it stay open while the user closes one document and opens another.

## A read only command opens no transaction

Scan Model reads. Nothing in it writes, so nothing in it needs a `Transaction`, and adding
one would be the first step toward a command that changes a model while claiming to read it.

## A command that writes decides everything first, then asks, then writes once

Scope Box works out all six cases with no transaction open, shows the counts, and only then
opens a single transaction covering every assignment, so the whole run is one undo. The
progress window is not pumped while that transaction is open, because letting other clicks
through mid write is how a model ends up half changed.

## A command never overwrites something a person put there

Scope Box reports a view carrying the wrong scope box and leaves it alone. Someone chose it,
and this add-in does not know why.

## The panel is five numbered steps

`PLOTS`, `VIEW TYPES`, `MARK`, `SHEETS`, `RUN`, in the order somebody does them. One open at a
time. It was a flat list of controls before, which read as a wall to anyone who had not built
it.

Four rules hold it together.

**A shut step carries its own summary**, so the whole state reads without opening anything.
`1  PLOTS   DM-11 to DM-28, 17 of 17 ticked`.

**A step that cannot be used yet is greyed out with one line saying why.** A disabled control
with no reason next to it tells nobody anything.

**Every summary and every reason is in `PanelSteps` in Core, with a test.** None of it is
formatted next to the control that shows it. That is the same shape as the grid cell, the
column count and the run report, and it has been the bug three times.

**Nothing drags the user out of a step they are working in.** Picking a range opens step 2 by
itself, once per read, because that is the one act that unlocks everything below. Everything
else is a Next button at the foot of the open step or a click on any header.

The scope box counts live inside step 5 rather than in a section of their own, because they act
on the same ticked plots the run does.

## The strip, the steps and the status line

The strip at the top holds the model name, when it was last read, Refresh and Scan Model. Those
belong to the document rather than to any one step. The status line is docked at the bottom, so
it is in the same place whatever is open above it. Both are built once and repainted rather
than redrawn, which is why `PaintFromTheTheme` sets their brushes by hand: a colour set once,
before the theme it follows is read, is how the panel came up black on black the first time.

## Marking a lot of cells at once

17 plots by 8 view types is 136 clicks, and the first person to use the grid did exactly that.
Three ways in: **Mark every missing**, a **plot name** for its row, a **column header** for its
column, and **Clear all marks** to undo the lot.

All of them go through `BulkMarking` in Core, which hands back only cells a single click on the
grid would have marked. A square holding a view is never swept up, because marking says a view
is wanted and that one is there. An unticked plot is never swept up either, because the run does
not act on it and the marks would be dropped without a word. A single click on one square is
still allowed anywhere, because that is a deliberate act rather than a sweep.

A plot name and a column header are labels sitting inside a borderless button. `Flat` builds
them with `PanelTheme.Clear` and `PanelMetrics.Nothing`, so the frozen column stays in step with
the scrolling cells beside it. A real button there would put chrome down the side and across the
top of a grid that is already dense.

## A column header is the code, not the whole name

`(200) General Arrangement Layout` is thirty characters over a column one square wide, and eight
ticked view types ran the headers off the right edge with sideways scrolling the only way to
reach the last of them.

`GridColumnLabels.For` gives each column the shortest header that still says which it is: the
code alone, and where a code is shared, the code plus as many leading words of the view name as
it takes to tell the sharers apart. Code 010 appears twice on the real model, which is why the
code alone can never be the whole answer. The full name is on the tooltip, and the key under the
grid lists every header that lost something, so a grid of plain codes carries no key at all.

## A list that is rebuilt comes back where it was left

Ticking a view type near the bottom of 84 threw the list back to the top, so the user scrolled
down again for every tick. The step is thrown away and built again on every change and a new
ScrollViewer starts at nothing.

`Scrolling` keeps the offset by name across the rebuild. It restores on the first layout pass
rather than on Loaded, because a ScrollViewer that has not measured its content clamps any
offset to zero, and that reads exactly like a restore that worked. Step 1's plot list has the
same fault and gets the same treatment.

## A control that outlives a redraw has to be taken out of its old parent

The steps are thrown away and built again on every change. A handful of controls do not go with
them, because a ComboBox carries its own item list and a TextBox carries what somebody is
halfway through typing. Each of those is put into a new parent every time.

WPF refuses that outright. An element already has a logical parent and adding it to a second
throws, which would take the panel down on the second click rather than the first.
`Reparented` takes it out of whatever held it before. Anything added to the tree that is also a
field goes through it.

The same reason a keystroke in a sheet number box calls `RefreshHeaders` rather than a full
redraw. Rebuilding the tree under the cursor takes the cursor out of the box.

## No brush and no number is written in the panel file

The first install came up black on black. A dockable pane on Revit's dark theme sits on a
black background, WPF defaults a TextBlock to black text, and every heading, label and grid
label in the panel was invisible. `PanelTheme` reads `UIThemeManager.CurrentTheme` and hands
back a background, a foreground and one warning colour per theme. The panel sets Background
and Foreground on itself once, and every TextBlock under it inherits.

Every colour has a value per theme, because firebrick vanishes on dark grey and a green that
reads as primary on white reads as an error on charcoal. If a new element needs a colour, it
goes in `PanelTheme` with both values, not next to the element.

`PanelMetrics` is the same rule for spacing and font sizes. Every margin, padding, row height
and size is named once there. The panel was built by typing a number at each control, so
nothing lined up with anything and changing the rhythm meant finding forty numbers. The grid's
frozen column and its scrolling columns share one fixed row height from there, which is the
only thing making the two halves line up: auto height on either side drifts the moment one cell
wraps.

`PanelTheme` reads the theme without an external event, because a theme lookup touches no
document and the panel has to paint itself before any document exists. It lives in its own
file so `DrawingSheetPanel` names no Revit type at all.

## Any round that changes the panel writes an HTML mockup

`design/pr-<number>/panel.html`, drawn by hand from the code, so the layout and the wording
can be read before someone spends an install on it.

It says at the top, in the file itself, that it is a mockup and not a screenshot, and that it
cannot show how Revit will render it. That line is the whole point. A mockup passed off as a
screenshot is worse than no mockup, because it answers a question it never asked.

## One tab, one panel, one button

The RCRC Green tab holds the Drawing Sheet panel and nothing else. Scan Model and Scope Box
were buttons of their own and are not any more. `ScanModelCommand` and `AssignScopeBoxCommand`
are still classes and still do the work, reached through the external event from inside the
panel, because somebody deciding what to do is already in the panel and should not be hunting
along a ribbon for the next step.

## Scan Model is the check on the panel

It reads the whole document rather than the range, so its numbers are worked out a different
way from the panel's and can be held against them. That is why it is still there and why it is
not narrowed to the range when the panel is.

The panel and Scope Box used to disagree on purpose about where a view's plot comes from. They
do not any more. Both read it from the view's own name, because the panel filing a view under
PRX_Plot_ID while taking the view type from the name is what put a view in the wrong row. The
comparison that difference was kept for has been made, and it came out against it.

## A new view is set up from ONE sibling, read once

The sibling is a view of the same view type the model already holds. It is what the team
built, so how it is set up is the answer to how a new one should be. Five things come off it
and none is matched on a name: **family type**, **level** (a plan), **view template**, and the
three crop settings, **Crop View**, **Crop Region Visible** and **Annotation Crop**.

**Annotation Crop is not one of them.** It is forced on for every plan view, whatever the
sibling has. Copying it was tried for one round and the next report showed why it failed:
DM-11-(010) Overall Plan was set up from PL-17-(010) Overall Plan, which has it off, so the new
view inherited the fault and neighbouring plots' section markers still drew through it. The
model disagrees with itself and there is nothing there to copy.

`AnnotationCropChoice` in Core decides it and says whose setting it is in the report, the same
way `SectionDepth` does. Crop View is still set first because Revit will not turn Annotation
Crop on for a view whose crop is off, and Annotation Crop comes off
`VIEWER_ANNOTATION_CROP_ACTIVE` because it is a parameter rather than a property.

A SECTION still copies all three. No section has ever been created by this tool, so there is no
evidence that a section inherits the same fault, and forcing it on there would be a guess.

`ApplySiblingCrop` runs after the template, so a template controlling any of the three refuses
and is reported rather than quietly losing to it.

**The far clip is no longer one of them.** See the section below.

`SiblingReader.Of` reads every candidate into a Core `SiblingView` once per run, before
anything is created, so no view this run makes can become the sibling of a later item.
`SiblingChoice.For` picks one. The four settings travel together on that one object, so they
cannot come from two views, and a Core test goes red if any future change pairs one view's
family type with another's template.

**The report names the view they came from**, under WHERE EACH NEW VIEW WAS SET UP FROM, with
the family type and the template printed next to it. A run produced a view whose template read
`(010) Overall Plan` and whose family type read `(200) General Arrangement Layout`, and there
was no way to tell whether they had come from the same view because nothing recorded it. That
is not a state to be in twice.

The kind is preferred over the first match. A plan needs a level and a section needs a far
clip, and the model holds view types drawn both ways. Taking the first view of the type
regardless refused a plan because the first one happened to be a section, while a usable plan
sat further down the list.

This replaced two name matches, both wrong on the real model. The family type was matched on
the view type, on the belief that a `(010) Location Key Plan` is made with a type of that name.
It is made with `(010) Key Location Plan`, the words swapped. The template was matched on a
prefix, and eight templates start with `(200) General Arrangement Layout`, so it found several
and applied none. `ViewTypeNaming` and `TemplateMatch` held both and are deleted.

## A section is a different call from a plan view

`(400) Landscape Cross Section` is a section on this model. `ViewPlan.Create` can never make
one, which is why the first real run refused every one of them with a message that blamed the
level.

**Which types need a section is read off the model, not off the code.** `DrawingSheetReader`
records the view type of every `ViewSection` it passes, `RunPlan` turns those into
`RunItemKind.Section`, and nothing anywhere says that 400 means section. That is a fact about
this model and the next model may not share it.

**How far it looks is one metre, and it is the tool's own setting.** It came off the sibling
section for one round, on the reasoning that the model is the better answer. The model has no
answer: DM-14 and NS-32 read 3.0480 feet, DM-16 reads 5.0199 and DM-20 reads 42.1054, which is
0.93, 0.93, 1.53 and 12.83 metres. Whichever sibling happened to be picked decided the depth,
so the team chose one number instead. `SectionDepth.Metres` in Core holds it, `SectionDefaults`
converts it to feet, and the report says it is the tool's setting rather than anything read off
a view.

**A section is left with no scope box.** A real one in this model has none, and its own section
box is what bounds it, so a scope box on top would crop it to something nobody asked for. The
plot's box is still what says where to cut, and a plot without one is still refused. It is
simply not set on the finished view. A plan view still gets one.

`SectionPlacement.Across` takes the plot's scope box as a `PlotBox`, `SectionAxis.ShortSide`
and the depth, and hands back the two ends of the line, the direction the view looks and the
depth. `ModelWriter.SectionBoxFor` turns that into the `BoundingBoxXYZ` Revit wants. Its
transform is the section's own frame, and the one thing worth knowing is that `BasisZ` points
back at the viewer, so the view looks along the negative of it, which is why the direction Core
hands back is negated there.

## One list, one fill

The view type code buttons and the Add row's code dropdown are filled from `CodesInUse` in the
same loop, in `InsideViewTypes`. They used to be filled by one method. The buttons were moved
inline when the panel was rebuilt into steps and the dropdown half was left behind, so it
opened empty and no view type could be added at all, while the same codes sat as buttons
directly above it.

One method serving two records of one fact is fine. Splitting it and taking only half is the
same failure this repo keeps hitting, wearing different clothes.

`FillTheCodes` compares before it clears, because the dropdown is one of the controls that
outlives a redraw and clearing it under an open list takes the selection with it.

## A sheet is described, never copied

It used to copy a sheet that already existed. The user picked one with no views on it and got
an empty sheet, and copying was not how they wanted to work. `SheetCapture` is deleted rather
than left as a second way to build the same thing.

**A sheet is described once and repeated across every ticked plot.** Four things are shared:
the title block type, read from the model and never written down here, the sheet name, the
view types that go on it, and whether 1, 2 or 4 views go per sheet. One thing is per plot: the
sheet number.

The name and the number are editable dropdowns and the user can type past both, because the
list is an offer and never a restriction. **The tool still invents neither.** A plot with no
number gets no sheet and the report says which plot and which sheet.

**The number list is the numbers NOT in use.** It used to be the ones in use, so every entry in
it was certain to be refused, and three sheets were lost to that in one run with "a sheet
numbered 010EA is already in this model". `SheetNumbers.Free` steps the last run of digits in
each number in use until it lands on a free one, so every offer is shaped like something the
project already does.

A number that will be refused says so under its box as it is typed, and step 5 counts them
before the confirmation. Every number warning refreshes on any keystroke in any box, because two
plots given one number is a clash and neither box knows about the other on its own.

More than one sheet can be described, so one run gives a plot its LIST OF DRAWINGS and its
GENERAL ARRANGEMENT LAYOUT together. `SheetOrder` pairs one `SheetDefinition` with the number
each plot gets for it, and `RunPlan` turns each pair into one item per ticked plot that has one.

**Where the views sit is worked out, not read.** `SheetLayout.For` takes the title block's width
and height and a count of 1, 2 or 4 and hands back the centre of each viewport in reading order.
One is centred, two sit side by side, four make a two by two grid, and the margin round the
outside is the same measurement as the gap down the middle. Y counts up from the bottom in
Revit, so the first row back is the top one.

**The size comes off the title block PLACED ON THE SHEET, never off the type.** Sheet Width and
Sheet Height are read-only INSTANCE parameters. On a `FamilySymbol` `get_Parameter` returns
null for both, null became zero, and three sheets were created empty on a real A1 title block
while the report said the block had no size.

So `ModelWriter.SizeOf` regenerates, finds the title block instance on the new sheet and reads
those two parameters off it. A title block family that does not drive them is measured across
instead, because a block drawn at A1 is still A1. `SheetSize` carries which of the two reads
answered and the report prints it in millimetres.

The order this forces is worth knowing. The size cannot be read until the sheet exists, so a
sheet that has views ticked and cannot be measured is created, found wanting, and deleted
again. The delete is checked like every other one here, and the sheet is never recorded as made
before that is settled. A definition with no views ticked is not measured at all and still
makes an empty sheet, which is a real thing to ask for.

`SheetBeingDescribed` in the Revit project is the mutable half the panel owns while somebody is
still filling it in. What it hands out is the immutable Core `SheetDefinition`, narrowed to the
view types still ticked in step 2, so a sheet cannot keep asking for a view the run no longer
offers. The tick is remembered rather than dropped, so re-ticking the type in step 2 puts it
back on the sheet.

Three things it will not do. **A definition short of a type or a name is refused once**, not
once per plot, because it is one thing to go and fix rather than seventeen. **A view already on
another sheet is refused rather than moved**, because it belongs to whoever put it there, and
`Viewport.CanAddViewToSheet` is asked before every placement so that comes back as a refusal
rather than a throw. And **a definition with no views ticked still makes a sheet**, empty, which
is a real thing to ask for. The confirmation says so before it runs, so nobody is surprised by
one twice.

Views are made before sheets inside the same transaction, so a sheet can carry a view this run
only just created.

## A schedule is built on a category number and typed filter values

Three things were being flattened on the way out of a schedule and could not be rebuilt.

**The category.** `CategoryIdFor` walked `Document.Settings.Categories` looking for a matching
display name. KERBS is built on Slab Edges, that walk found nothing, and the schedule was
refused with "this model has no category named Slab Edges" on a model that has it. Capture used
a third lookup, `Category.GetCategory`, which resolves any category id, so it could hand create
a name create could never find. `ScheduleCapture.BuiltInValueOf` records
`Category.BuiltInCategory` and `ModelWriter.CategoryIdFor` resolves that number. No name is
matched anywhere.

**The filter value.** `ScheduleFilter` holds its value in one of four typed getters and asking
the wrong one throws. Every value was captured as text and handed back as a string, so the two
schedules filtering on PRX_Included In Budget equals Yes were refused: that is a Yes/No
parameter, Revit holds it as the integer 1, and a string is only right for one of the four
kinds. `ValueIn` returns a Core `FilterValue` with its kind, and `TryRebuild` puts it back as
the same kind. A value Revit still refuses is a lost filter, which deletes the schedule again.

**The field kind.** `GetSchedulableFields` offers the parameters of a category and never a
formula, a percentage, a count or a combined parameter, because those are defined inside the
schedule that holds them. `KindOf` records which a field is, so the report says a calculated
field has to be written again by hand rather than blaming the category.

## A missing filter is not a missing field

Both used to be skipped with `continue` and nothing written down.

A schedule short of a FIELD is short of a column. Somebody looking at it can see that. It is
created and named in the report under CREATED, BUT NEEDS ATTENTION.

A schedule short of a FILTER is a different thing. A quantity schedule that lost its plot
filter shows every plot's elements and reads as correct on a drawing. That one is deleted again
inside the same transaction and reported as refused.

The delete is checked rather than assumed. `ModelWriter.Deleted` returns true only when Revit
really removed the element, and a false there is not swallowed. The item moves to a third list
and the report grows a section headed CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND,
which names the schedule and what is missing from it, with a banner at the top of the report so
nobody has to reach the end to find out.

Deleting an element made moments earlier in the same transaction ought to work and has never
been run, so the failing case is written down rather than assumed away. The rule is the one
that costs nothing to keep. A report that says a thing was deleted when the model still holds
it is worse than the wrong schedule, because the wrong schedule can still be found.

## A report goes to two places and the panel names both

`ReportFile.Write` writes the Desktop copy first and lets it throw, because a report that cannot
be written at all is worth a message. The repo copy is guarded and never costs the first one.
It comes back with the list of paths that really landed, and `ReportPlaces.Written` turns that
list into the line the panel shows. One path means the repo folder was not found, and the line
says so and says to run `install.ps1` again, rather than leaving somebody hunting for a file
that was never written.

Nothing in the Revit project knows where the repo is. `install.ps1` writes the absolute path
into `reports-folder.txt` next to the installed assembly and `ReportFile` reads it from there.
That folder is in `.gitignore` and never leaves the machine.

## A count on the panel opens into the thing it counted

Six numbers tell somebody how much is wrong and nothing about what. Every scope box case except
B is a button, and clicking it lists its views by plot and name with the box each one holds.
Clicking a view in that list goes through the external event like any other request, because
the panel opens nothing itself.

The lists come from `ScopeBoxCounts.In`, the same object the counts come from, so a case list
can never be a different length from the number above it. Case B is 102 schedules that cannot
hold a scope box at all, which is a count and nothing more.

## Building and installing

`CLAUDE.md` covers `install/install.ps1`. Two things it leaves out. `-Configuration Debug`
installs the debug build instead of release. And Revit 2023, 2025 and 2027 are also on the
build machine, so anything reading a Revit install path has to name 2024. Nothing here does.
`RevitAPI.dll` for 2024 sits at `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll`, and the
build never opens it. The two Nice3point packages supply the reference assemblies, so the
projects restore and compile on a machine with no Revit installed.
