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
opens a single transaction covering every assignment, so the whole run is one undo. Nothing
pumps the message queue while that transaction is open, because letting other clicks through
mid write is how a model ends up half changed.

## A command never overwrites something a person put there

Scope Box reports a view carrying the wrong scope box and leaves it alone. Someone chose it,
and this add-in does not know why.

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

## One tab, two panels

The RCRC Green tab holds two panels side by side. Drawing Sheet holds its one button. KPI is
built to carry several buttons later and carries one, KPI Checklist, which shows the KPI pane.
Scan Model and Scope Box were buttons of their own and are not any more, because somebody
deciding what to do is already in the panel and should not be hunting along a ribbon for the
next step. Both are reached through the external event from inside the Drawing Sheet panel.
Scan Model is the handler's own `Scan`, which reads through `ModelScanner` and writes through
`ReportFile`. Scope Box is the three statics on `AssignScopeBoxCommand`, the confirmation,
the assignment and the report. `ScanModelCommand`, the two `Execute` methods and the progress
window they showed were unreachable for several rounds after the buttons went, and
`ScanModelCommand.Execute` was a second scan path writing to the Desktop only. All three are
deleted.

Each pane has its own identifier, its own `ExternalEvent` and its own handler, and each is
registered through the one guarded `Registered` in `RcrcGreenApplication`, so a pane that
will not register costs its own button its pane and nothing else. The KPI pane's rules are in
`kpi-rules.md`.

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

## A report goes to one place and the panel names it

`ReportFile.Write` writes into the reports folder inside the repo and nowhere else. It went
to the Desktop as well until the round after the first real run, which meant two files per
run, two places to look and two to tidy up, and the Desktop copy was the one nothing reading
the code could ever reach.

**No pointer file means no report at all.** `reports-folder.txt` is the only thing that says
where the folder is, so without it there is nowhere to write, and `ReportPlaces.Written`
opens with NO REPORT WAS WRITTEN and says to run `install.ps1` again. Falling back to the
Desktop would put the file exactly where nobody agreed to look for it. The write itself is
allowed to throw, because it is the only copy now and a report that cannot be written is
worth a message.

Nothing in the Revit project knows where the repo is. `install.ps1` writes the absolute path
into `reports-folder.txt` next to the installed assembly and `ReportFile` reads it from there.
That folder is in `.gitignore` and never leaves the machine.

`ReportPlaces` is Drawing Sheet's and the KPI pane reads it across the fence, so this moved
where the KPI reports land too. The KPI handler still has a line of its own naming the
Desktop, which is that task's file and is left for that session.

## Building and installing

`CLAUDE.md` covers `install/install.ps1`. Two things it leaves out. `-Configuration Debug`
installs the debug build instead of release. And Revit 2023, 2025 and 2027 are also on the
build machine, so anything reading a Revit install path has to name 2024. Nothing here does.
`RevitAPI.dll` for 2024 sits at `C:\Program Files\Autodesk\Revit 2024\RevitAPI.dll`, and the
build never opens it. The two Nice3point packages supply the reference assemblies, so the
projects restore and compile on a machine with no Revit installed.

The rest of these rules moved, byte for byte, to `docs/rules/` on 2026-10-28. `docs/rules/index.md` holds one line per rule id and the file it lives in. Read a rule there by its id, and only its part. A new rule goes into the part for its topic, with its line in the index.
