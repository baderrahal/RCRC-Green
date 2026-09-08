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

## No brush is written in the panel file

The first install came up black on black. A dockable pane on Revit's dark theme sits on a
black background, WPF defaults a TextBlock to black text, and every heading, label and grid
label in the panel was invisible. `PanelTheme` reads `UIThemeManager.CurrentTheme` and hands
back a background, a foreground and one warning colour per theme. The panel sets Background
and Foreground on itself once, and every TextBlock under it inherits.

The only colour named anywhere else is the warning on a plot with no scope box, and it has a
value per theme because firebrick vanishes on dark grey. If a new element needs a colour, it
goes in `PanelTheme` with both values, not next to the element.

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

## A new view is set up from the sibling, never from a name

The sibling is a view of the same view type that the model already holds on another plot. It
is what the team built, so how it is set up is the answer to how a new one should be. Three
things come off it and none of them is matched on a name:

**Family type**, from `sibling.GetTypeId()`. **Level**, from its `GenLevel`, for a plan view.
**View template**, from its `ViewTemplateId`.

No sibling means the item is refused and the report says to make one by hand on any plot.

This replaced two name matches and both were wrong on the real model.

The family type was matched on the view type, on the belief that a
`(010) Location Key Plan` is made with a type of that name. It is made with
`(010) Key Location Plan`. The words are swapped. Three of the four refusals in the first real
run were that, and it took a Properties panel to find, which is why the scan now lists view
family types.

The template was matched on a prefix. Eight templates start with
`(200) General Arrangement Layout`: Scale 250, 400, 500, 600, 1000 and 2500, plus
`(Coordination)` and `(Streets)`. A prefix match found several and so applied none, which
would have created every view with no template at all. The sibling already carries the one in
use.

`ViewTypeNaming` and `TemplateMatch` held both matches and are deleted. Two ways to answer one
question is the shape this repo keeps getting caught by.

## A section is a different call from a plan view

`(400) Landscape Cross Section` is a section on this model. `ViewPlan.Create` can never make
one, which is why the first real run refused every one of them with a message that blamed the
level.

**Which types need a section is read off the model, not off the code.** `DrawingSheetReader`
records the view type of every `ViewSection` it passes, `RunPlan` turns those into
`RunItemKind.Section`, and nothing anywhere says that 400 means section. That is a fact about
this model and the next model may not share it.

The maths was in Core and unused since it was written. `SectionPlacement.Across` takes the
plot's scope box as a `PlotBox`, `SectionAxis.ShortSide` and a depth, and hands back the two
ends of the line, the direction the view looks and the depth. `SectionDefaults` turns the ten
metres into feet at the Revit boundary, because Revit holds every length in feet and passing
the ten straight through would place a section ten feet deep.

`ModelWriter.SectionBoxFor` turns that into the `BoundingBoxXYZ` Revit wants. Its transform is
the section's own frame, and the one thing worth knowing about it is that `BasisZ` points back
at the viewer, so the view looks along the negative of it. That is why the direction Core hands
back is negated there. Min and Max are in that frame: X is half the line either side of the
middle, Y is the height of the scope box, and Z runs from the depth behind the cut up to zero
at the cut.

A plot with no scope box has nowhere to cut, so the item is refused and says so in those words
rather than repeating the plan view line.

## A sheet is copied, never designed

The three questions that stopped sheets being made have one answer between them. The user sets
one plot's sheet up by hand and that sheet is copied. So the title block is whichever one it
carries, a view sits where it sits there, and several views lay out however it has them.

`SheetCapture` reads a sheet into a Core `SheetDefinition`: the title block family and type,
the sheet size, and one placement per view on it holding the view type and the centre of its
viewport in feet from the sheet origin. Viewports and `ScheduleSheetInstance` both, because
they are different elements and reading only the first would drop every schedule off a copied
layout without saying so.

`ModelWriter` creates the sheet with that title block and places each view at the captured
position. Views are made before sheets in the same transaction, so a sheet can carry a view
this run only just created.

Two things it will not do. **The sheet number and the sheet name are typed by the user and are
never invented**, so a row missing either gets no sheet and is named in the report. And a view
already sitting on another sheet is refused rather than moved, because it belongs to whoever
put it there. `Viewport.CanAddViewToSheet` is asked before every placement, so that comes back
as a refusal rather than as a throw.

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
