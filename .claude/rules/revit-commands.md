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

## A plan view is set up from the model, never from a default

Read off DM-18-(200) General Arrangement Layout. Three lookups, and all three refuse rather
than fall back, because a view that looks finished and is wrong costs more than one that was
never made.

**Family type.** Named exactly the view type, `(200) General Arrangement Layout`. Not a
generic floor plan type. No match means the item is refused and the report names the type it
looked for.

**Level.** Taken from an existing view of the same type on another plot, which is what the
team did. On DM-18 that is Level 1. Nothing of that type anywhere means no level to take, so
the item is refused rather than a level being picked.

**View template.** The view type followed by how it is drawn, so match on the prefix:
`(200) General Arrangement Layout SC - Scale 250`. The template carries the scale, the detail
level, the discipline, the visibility overrides and the phase filter, so setting it is how all
of those follow and none of them is set one by one. Exactly one match is applied. None creates
the view and says plainly it has no template. More than one creates the view and names every
candidate, because Scale 250 and Scale 500 are both real and choosing is not this tool's to do.

## A missing filter is not a missing field

Both used to be skipped with `continue` and nothing written down.

A schedule short of a FIELD is short of a column. Somebody looking at it can see that. It is
created and named in the report under CREATED, BUT NEEDS ATTENTION.

A schedule short of a FILTER is a different thing. A quantity schedule that lost its plot
filter shows every plot's elements and reads as correct on a drawing. That one is deleted
again inside the same transaction and reported as refused, so the model never holds it.

## Building and installing

`CLAUDE.md` covers `install/install.ps1`. Two things it leaves out. `-Configuration Debug`
installs the debug build instead of release. And Revit 2023, 2025 and 2027 are also on the
build machine, so anything reading a Revit install path has to name 2024. Nothing here does.
`RevitAPI.dll` for 2024 sits at `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll`, and the
build never opens it. The two Nice3point packages supply the reference assemblies, so the
projects restore and compile on a machine with no Revit installed.
