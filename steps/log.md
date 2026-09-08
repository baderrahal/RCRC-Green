# Log

Newest entry first.

---

## 2026-09-08, twelfth pass. A guarded delete, reports the code can read, and cases that open

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#17](https://github.com/baderrahal/RCRC-Green/pull/17), one commit, 18 files, merged into main
as `a18a2ae`.

### 1. The delete I flagged last round

`ModelWriter.MakeSchedule` builds a schedule, notices a filter would not go on, and deletes it
again inside the same transaction. Last round I wrote that down as the right call and then
wrote the delete unguarded, so the whole thing rested on `document.Delete` never failing.

If it did fail, the exception would have been caught by the handler around the whole item and
turned into "Revit refused it", while the report's own closing note still said a schedule that
lost a filter is never created. The model would have held a schedule showing every plot's
elements and the report would have said it was gone.

`ModelWriter.Deleted` returns true only when `document.Delete` came back with something in it,
and catches the three exception types the rest of the file catches. A false does not fall
through to the refusal list. It goes to a third list, `leftBehind`, which prints under a section
of its own, `CREATED WRONG AND STILL IN THE MODEL, DELETE BY HAND`, naming the schedule, the
filters it lost and what that means on a drawing.

`RunReport` puts a banner above every section when that list is not empty, because the section
is a long way down and the one person who needs it is the one who stopped reading. The panel
status line says the same thing in capitals before it says how many were made.

Two things came out of writing it. The banner said "1 wrong schedules are in the model", so it
reads singular now. And the closing note of every run report said a schedule that lost a filter
is not created at all, which stopped being true the moment the delete could fail, so it now says
what happens when Revit refuses. There is a test that fails if that old sentence comes back.

### 2. Reports where this can read them

Every report went to the Desktop only. That is where the team looks and it is the one place
nothing here can reach, so a run on a real model came back to me as a description of a report
rather than a report.

`ReportFile.Write` now writes both. The Desktop copy is unguarded, because a report that cannot
be written at all is worth an exception. The repo copy is guarded and never costs the first one.
It returns the paths that really landed, and `ReportPlaces.Written` turns that list into the
line the panel shows, so the panel names what exists rather than what was attempted.

Revit runs the add-in from `%APPDATA%\Autodesk\Revit\Addins\2024\` and has no idea where the
repo is. `install.ps1` writes the absolute path into `reports-folder.txt` beside the installed
assembly and lists it with everything else it copied. No pointer file means the Desktop only,
and the status line says so and says to run `install.ps1` again.

**That folder is in `.gitignore` and stays there.** This repository is public. A report carries
client view names, sheet numbers, plot identifiers, scope box names and the full field list of
every schedule it touched. `reports/README.md` is the only tracked file in it and says why, and
`git add reports/` was run to confirm that it stages the README and nothing else.

### 3. Six numbers and a button

The scope box section read A 0, B 102, C 66, D 0, E 13, F 1 on the real model. F 1 is one view
carrying a scope box that is not its plot's, and finding out which view meant opening a text
file on the Desktop.

Five of the six are buttons now. Clicking one lists its views by plot and by name with the box
each one holds, and clicking a view opens it in Revit through the external event, the same route
the grid cells use. One case is open at a time, because six lists inside a docked pane is a
scroll rather than a view. B stays a plain line, 102 schedules with no scope box parameter and
nothing anyone acts on. A case with a count of 0 is drawn disabled rather than left looking
clickable.

`ScopeBoxCounts` keeps the decisions per case now instead of only the totals, so `Of` is
`In(...).Count` and a list can never be a different length from the number above it. That is the
same fault as the column count and the grid cell, and it is now the third time one fact kept in
two places has been the bug, so this one is not kept in two places. There is a test that walks
all six cases and holds the count against the list.

Assign is unchanged. It acts on C alone, it confirms, it writes in one transaction.

### What was checked, and how

`dotnet build RcrcGreen.sln` and `dotnet test`, both run after the last file was written.
Build 0 warnings and 0 errors across all three projects. 274 tests, 0 failed and 0 skipped,
locally, 17 of them new. The gate then executed 274 on a real runner, the same count, 0 failed
and 0 skipped.

`git add reports/` staged `reports/README.md` alone, which is the only way to check the ignore
rule without committing a report.

### What has still never been observed

The list from last round has not shrunk. Nothing in the Revit project has run on this machine
and nothing in the write path has ever run anywhere.

- No view and no schedule has ever been created by this tool
- No schedule has ever lost a filter, so the delete has never been reached
- `ModelWriter.Deleted` has never returned false, and no report has ever carried the section
  that exists for it. Every sentence about what Revit does when a delete is refused is read off
  the API and not off a run
- `ReportFile` has never written anything. Neither path has been written to, `reports-folder.txt`
  has never been read by the add-in, and no report has ever landed in `reports/`
- `install.ps1` has not been run since it started writing that file
- The case list buttons have not been rendered. Whether a stretched Button reads as a row on
  either theme, what a 66 entry list does to the height of a docked pane, and whether a full
  view name fits the width are all unknown
- Clicking a view in a case list has never opened a view. The route is the one the grid uses
  and the grid has been clicked in Revit, but these buttons have not

`design/pr-17/panel.html` is a hand drawn mockup of the case lists and the report, in both
themes, and says at the top that it is not a screenshot.

---

## 2026-09-08, eleventh pass. Two silent skips in the write path, and how a view is really made

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#15](https://github.com/baderrahal/RCRC-Green/pull/15), one commit, 10 files, merged into main
as `792c203`.

### 1 and 2. The two skips, and why they are not the same fault

`ModelWriter.MakeSchedule` had two bare `continue` statements and neither wrote anything down.

A **field** that did not resolve against `GetSchedulableFields` was dropped. The user gets a
schedule short of a column that looks finished.

A **filter** whose field was not in `fieldByName` was dropped. That one is worse and it is
worse in kind, not degree. A quantity schedule that lost its `PRX_Ref Plot ID equals DM-11`
rule shows every plot's elements in the model. It has rows, it has totals, and it reads as
correct on a drawing until somebody adds up the site and finds the number is the whole job.

Both are recorded now, naming the schedule, the plot and the field or filter, and both reach
the report.

**What I decided, and why.** A schedule that lost a filter is **not created at all**. It is
built, the loss is noticed, and it is deleted again inside the same transaction, then reported
under NOT CREATED. The check can only happen after creation, because `GetSchedulableFields`
needs a schedule to exist, so deleting it is the only way to refuse it. The transaction is
already open and covers the whole run, so nothing is left behind either way.

A schedule that lost only a field is kept and reported under a new heading, CREATED, BUT NEEDS
ATTENTION. A missing column can be seen by the person holding the drawing. A missing filter
cannot.

### 3. How a plan view is really set up

Read off DM-18-(200) General Arrangement Layout. Family type `(200) General Arrangement
Layout`, template `(200) General Arrangement Layout SC - Scale 250`, level Level 1, phase
Proposed, no scope box on that particular view.

Three things in `ModelWriter` were guesses and all three are gone.

**The family type** was the first `ViewFamily.FloorPlan` type the collector returned. It is
now the type named exactly the view type. There is no fallback. A view made with a different
family type looks finished and is wrong, so a missing type refuses the item and the report
names what it looked for.

**The level** was the lowest level in the model by elevation. It is now the level an existing
view of the same type sits on, anywhere in the model, which is what the team actually chose.
No view of that type anywhere means no level to take, and the item is refused rather than a
level being picked.

**The view template** was not set at all, so scale, detail level, discipline, visibility and
phase filter were whatever a new view gets. The template is now looked up by prefix, since a
template name is the view type followed by how it is drawn. Exactly one match is applied. None
creates the view and says plainly it has no template. More than one creates the view and names
every candidate, because Scale 250 and Scale 500 are both real and choosing between them is
not this tool's to do.

`ViewTypeNaming` in Core does the matching and is tested, including the case that a name
holding the view type in the middle is not a match and that case matters.

### Tests

257 pass, 0 failed, 0 skipped, locally from a run made after the last file was written and
again on the runner. 9 are new,
covering the family type name and every outcome of the template match. The build has 0
warnings.

### No mockup this round

The panel did not change, so there is nothing new to draw. `design/pr-13/panel.html` is still
what the interface looks like.

### Not observed, because it needs Revit

Nothing here has run. The write path has still never executed once.

- whether a view family type in this model really is named exactly `(200) General Arrangement
  Layout`. That is read from one Properties panel in one screenshot, not from the type list
- whether `ViewFamilyType.Name` returns that string. `Element.Name` on a type usually does,
  and it has not been checked
- whether `ViewPlan.GenLevel` is non null for these views, which the level lookup rests on
- whether `View.ViewTemplateId` accepts the id of a template found this way, and whether
  applying it after `ViewPlan.Create` but before the plot parameter is set is the right order
- whether the template really does carry the phase filter, so that Proposed follows from it
  rather than needing to be set
- whether `document.Delete` on a `ViewSchedule` created earlier in the same transaction works
  cleanly, which the filter refusal rests on entirely
- whether a missing filter is even possible in practice. If every captured field resolves,
  neither new path ever runs and both are untested in the strongest sense
- whether `SchedulableField.GetName(document)` returns what `ScheduleField.GetName()` does.
  Unchanged from last round and still the thing the field matching rests on
- capture still does not record the filter operator and create still assumes equals

### What comes next

Run it on a copy and check one created plan view against DM-18 property by property. Then the
three sheet questions, which are the only thing between this and a finished tool: which title
block, where a view sits, and how several lay out together.

---

## 2026-09-08, tenth pass. Creation, and a count that could not agree with its list

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#13](https://github.com/baderrahal/RCRC-Green/pull/13), one commit, 20 files, merged into main
as `f2c2eb5`.

### 1. The count and the list. What I found, and what I have for it

The panel read "54 of 84 view types shown, 30 hidden" while the list showed three ticked. The
three numbers in that sentence add up, so `GridColumns` was self consistent. The disagreement
was between `GridColumns` and the tick boxes on screen, and the reason is structural.

**They were two records of one fact, updated by two different paths.**

- the count was written from `_columns` by `SayTheColumns`
- the list was written from `_columns` **once**, in `FillColumnList`, which ran only after a
  refresh. From then on each tick box's own visual state was the record of what the user had
  clicked, and `_columns` was updated as a side effect of the click event

Nothing ever drew the list again from `_columns`, so once the two parted they stayed parted.

The specific way they part is one line, and it is in the old code:

```
private void ColumnShown(ViewType which, bool shown)
{
    if (_filling) return;
```

A click that arrives while `_filling` is true is dropped. The box has already moved, the model
is not told, the count is not even recalculated, and nothing puts the box back. `_filling` is
held while `FillPrefixes`, `PrefixChosen` and `PutTheRangeBack` run, and those set
`ComboBox.Items` and `SelectedItem`, which WPF can pump input during.

**What I have for it.** That is read off the code and it is certain as a mechanism. What I
cannot do is prove it is what produced those particular numbers, because I cannot run Revit.

**And there is a second explanation I also cannot rule out.** The list sits in a `ScrollViewer`
with `MaxHeight = 200`. At roughly 18 pixels a row, about 11 of the 84 rows are on screen at
once. "Three ticked and the rest unticked" describes what fits in that window, not 84 rows. 54
ticked out of 84 is entirely consistent with seeing three ticked among eleven visible.

So: one certain drift mechanism, one certain visibility limit, and no way from here to say
which produced the screenshot. I did not pick the likelier one. The fix removes both.

**The fix.** `GridColumns` is now the only record. `FillColumnList` draws the whole list from
it and `_filling` is held for the entire build, because setting `IsChecked` on a box that
already carries a handler raises the event and a tick put there by the code is not the user
asking for anything. Everything that changes what is ticked goes through `GridColumns` and
then the interface is drawn again from it. The count reads ticked out of total and says how
many the search is showing, so the number on screen names the same set the list is showing.

### 2. Picking four out of 84

A Search box filtering on the code and the name as the user types. All and None acting on what
the search is showing rather than the whole list. One button per code in use, each ticking
every type carrying that code and leaving the rest alone. Nothing is ticked when the model is
first read.

### 3. Adding a view type that does not exist

The earlier instruction that the tool must never invent a view type was withdrawn. It must
never invent a **plot**, and it still cannot. A code dropdown filled from codes in use, a free
text name, and Add. The added type is ticked, marked new, and draws missing on every plot.

It survives a refresh. `GridColumns.OverTheseTypes` keeps an added type the model still lacks,
and drops the new mark once the model holds one, which is what happens after a run makes it.

### 4. Schedules are not plan views

`DrawingSheetReader` records which view types are `ViewSchedule`, the snapshot carries them,
and the grid sets those column headers in italics and appends the word schedule.

### 5. The definition in the middle

`ScheduleDefinition` in Core holds the category, the fields in order, the filter rules and the
link setting as plain values. `ScheduleCapture` reads an existing schedule into one.
`ModelWriter` builds one in the model. Duplicating is the two run back to back, and loading a
definition from a file for a model holding no schedules is a small round on top rather than a
rewrite.

`ForPlot` changes only the rule whose value is a plot identifier. Everything else is carried
across, because HARDSCAPE and SHRUBS AND LAWN are both category Floors and their second filter
is the only thing telling them apart. Which parameter the plot is filtered on is read off the
captured schedule rather than assumed, so the Sheet List keeps PRX_Plot_ID and a quantity
schedule keeps PRX_Ref Plot ID. Field names are copied exactly, PRX_Furniture Lenght included.

A schedule type no plot in the model has cannot be captured, so it is refused by name rather
than half made.

### 6. Run

A Run section at the bottom saying what it would make, counted by kind, before anything is
pressed. One confirmation, one transaction, one undo, and a report file either way. A plan
view is created fresh with PRX_Plot_ID set and the scope box assigned, and a plot with no
scope box is refused rather than given a useless view.

**Sheets are not created.** The team answered that the user types the sheet number and the
sheet name and chooses one view per sheet or several. Three things are still open: which title
block a new sheet takes, where a view sits on it, and how several views lay out together.
Guessing any of them would put the wrong drawing in front of a reviewer, so nothing is
guessed. No sheet controls were added either, because a control that does nothing is a lie
about what the tool can do. The run line and the report both say this in as many words.

### 7. The three warnings

`Assert.Single` in all three places. The whole test file was rewritten for the new behaviour.

### Tests

248 pass, 0 failed, 0 skipped, locally from a run made after the last file was written and
again on the runner. 34 are new,
covering the search, All and None over a filter, the code buttons, added types across a
refresh, `ScheduleDefinition`, and what a run would make. The build has 0 warnings.

### Not observed, because it needs Revit

Nothing in this round has been run. **This is the first round that writes new elements to a
model, and none of that code has ever executed.** Specifically:

- every Revit API call in `ScheduleCapture` and `ModelWriter` is written from knowledge of the
  API and has never run. `ViewSchedule.CreateSchedule`, `CreateSheetList`, `GetSchedulableFields`,
  `AddField`, `AddFilter`, `IncludeLinkedFiles`, `ViewPlan.Create` and the filter value getters
  are all unverified against Revit 2024
- whether a captured definition rebuilds a schedule that matches the original, field for field
  and filter for filter
- whether `SchedulableField.GetName(document)` returns the same string `ScheduleField.GetName()`
  does. The field matching between capture and create rests on that and it is untested
- whether a field in the captured order is available on the new schedule at all. One that is
  not is skipped silently, which may leave a schedule short of columns with nothing said
- whether `ScheduleFilterType.Equal` is right for every captured filter. Capture does not
  record the operator and create assumes equals, which matches all six schedules on the
  screenshots and would be wrong for any that used something else
- whether a created plan view is any use. It is made on the lowest level with the first floor
  plan view family type, and neither choice has been asked about
- whether the run's one transaction commits, rolls back cleanly, or leaves a model half changed
- the search box, All and None, the code buttons, the add row, the new mark, the italic
  schedule headers and the whole Run section have never been on screen
- whether the panel is now too tall to use. Seven sections in a docked pane
- whether reading the model twice during a run, once by the panel and once by the handler, is
  fast enough to be invisible

### What comes next

Run it on a copy of the model and check a created schedule against its original field by
field. Then answer the three sheet questions and sheets become small. Sections are still
unbuilt, and `SectionPlacement` has been waiting since the first round.

---

## 2026-09-08, ninth pass. The grid was reporting views that are not there

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#11](https://github.com/baderrahal/RCRC-Green/pull/11), one commit, 20 files, merged into main
as `63d3a38`.

### The cause, and the evidence for it

`DrawingSheetReader` took a view's plot from one place and its view type from another, and
never checked the two agreed. The line was this:

```
present.Add(new PlotViewPresence(reading.PlotId, parsed.Type, view.Id.Value));
```

`reading.PlotId` is PRX_Plot_ID when the view carries one. `parsed.Type` is the code and view
name out of the view's own name. So a view **named** `DM-12-(200) General Arrangement Layout`
carrying **PRX_Plot_ID** `DM-11` was filed as DM-11 having a (200) General Arrangement Layout.
Delete every DM-11 view and that cell stays filled, because the view holding it belongs to
DM-12 and is still there. Clicking the cell opens a DM-12 view.

That is candidate b in the brief, and it is code I can point at rather than a guess. It is
also self inflicted, from the round that made PRX_Plot_ID the first source. The comment left
there at the time considered the case where the parameter gives a plot and the name does not
parse. It did not consider the case where both give a plot and they differ.

The evidence is a test, not a reading of the code. `ViewReadingTests` feeds the disagreeing
pair in and asserts on the plot that comes back. Before the fix that test would have got
DM-11. It now gets DM-12, and 214 tests pass.

### What I could not determine, and what would settle it

Whether that is what the user actually hit. I cannot run Revit and I have no copy of the
model, so I cannot see whether any view in it carries a PRX_Plot_ID that disagrees with its
own name.

Candidate a, the refresh never running, is not ruled out. What can be said about it from the
code is this. A refresh that reached the panel called `_columns.Clear()` and `FillPrefixes`,
which emptied the prefix, from and to dropdowns and blanked the grid. So a refresh that worked
left nothing on screen to be stale. For the user to be looking at DM-11 with two filled columns
afterwards, they must have re-picked the prefix and re-added both columns, and the presences
then came from a fresh read, which points at b. If instead the grid did not change at all when
Refresh was pressed, that is a and nothing in this round touches it.

**What would settle it in one look.** The status line after pressing Refresh. It now reads the
number of views, the time of the read, and how many views are named for one plot while
carrying PRX_Plot_ID for another. Three answers come out of it:

- the time does not change when Refresh is pressed. The request is not reaching Revit or its
  result is not reaching the panel. Cause a, and still open
- the time changes and the disagreement count is above zero. Cause b, and fixed here
- the time changes, the disagreement count is zero, and a deleted view still shows. Neither,
  and I would want the view name, its PRX_Plot_ID, and whether it appears in the Scan Model
  report from the same session

The time on the read was added for exactly this. The first time a refresh looked wrong there
was no way to tell whether it had run.

### The fix

`ViewReading` in Core is now the one place that decides what a view contributes. The plot on a
row still comes from PRX_Plot_ID first, because that is what recovers the 1,269 views whose
names do not parse. The plot on a **cell** comes from the name and nothing else. A view whose
name does not parse carries no view type and so fills no cell, which was already true. A view
whose two sources disagree is counted, and the count is on screen.

The rule, written into `.claude/rules/core-rules.md`: the grid must never show a view as
existing when it is not in the model.

### The stale read

`Shown` returned early on `_readOnce && !_model.Empty`, so opening a second document left the
first one's plots on screen. It now reads every time the pane is shown. The panel cannot tell
from outside that the document changed, and the read is fast enough that reading again costs
nothing.

A refresh also no longer throws away what the user had picked. The prefix, the first plot and
the last plot go back afterwards when the model still holds them. A refresh that emptied all
three is a refresh nobody can check, which is most of why the last one looked broken.

### Tick boxes on the plot rows

`PlotSelection` in Core. Every plot in range carries a tick, all on when a range is set,
changing the range builds a new one which is what resets them. Unticking takes a plot out of
the scope box counts and out of anything that writes. The row stays on screen, dimmed, so it
can be ticked again. The line above the grid reads ticked out of in range.

A plot outside the range cannot be ticked. The first version of `IsTicked` returned true for
anything not explicitly unticked, including plots that were never in the range at all, and the
test caught it before the code was ever run.

### Columns the right way round

`GridColumns` in Core. Every view type the model holds is a column from the start, with a
checklist for hiding and a count of how many are hidden. Add column is gone. Adding types one
at a time was unusable on a model with dozens of them, and worse than unusable, because
somebody opens the panel to find out what is missing and an empty grid answers nothing.

Hiding survives a refresh for types the model still holds, and is forgotten for a type that
has gone, so the hidden count can never name a column nothing could show again.

### The ribbon

One tab, one panel, one button. `ScanModelCommand` and `AssignScopeBoxCommand` are still
classes and still do the work. Scan Model is a button inside the panel now, because it is the
check the panel is measured against and it reads the whole model rather than the range.

### Scope boxes, visible before anything is pressed

`ScopeBoxCounts` in Core. All six case counts for the ticked plots, worked out from the
snapshot with no trip to Revit, so they follow a tick straight away. The snapshot now carries
the scope box state of every view and every scope box name, read in the same pass.

The narrowing reads a view's plot from its name, which is the rule `ScopeBoxPlan` follows
inside. The handler calls the same `ScopeBoxCounts.Narrow` before it writes. One narrowing,
not two, so the number shown and the number written are the same number.

Assign still re-reads the model before it writes, rather than trusting the snapshot. The
snapshot is as old as the last refresh, and a write built on a stale read is how a model ends
up with a scope box on a view somebody else already changed.

### Tests

214 pass, 0 failed, 0 skipped, locally from a run made after the last file was written and
again on the runner. 38 are new, covering the plot a view fills, the tick boxes, the columns
and the per selection case counts. The four rounds before this one all sat at 176, because
none of them touched Core.

### Not observed, because it needs Revit

Nothing in this round has been run. Everything below is untested:

- whether the fix is the fix. The mechanism is proved by a test, its presence in the user's
  model is not
- whether cause a is also happening. Nothing here would fix it
- the tick boxes, the column checklist and the scope box counts have never been on screen
- whether reading the model on every showing is fast enough to be invisible, or whether it
  makes docking and undocking feel slow
- whether the panel is now too tall to use. Five sections in a docked pane is more than it
  held before and none of it has been seen at a real width
- whether the Expander for the column list renders sensibly on either Revit theme
- whether Scan Model from inside the panel produces the same file the ribbon button did. It
  runs without the progress window, so a big model will hold the interface for the length of
  the read with nothing to look at
- whether `ScopeBoxCounts` over a real model's view list is fast enough to run on every tick
- the numbers in `design/pr-11/panel.html` are made up to show the shape of the lines

### What comes next

Run it, delete a view, press Refresh, and read the status line. That answers the one question
this round could not. Then creation, which now has its answers written into `CLAUDE.md`: a new
view is created fresh and never duplicated, it carries no annotation, the user chooses how
many views go on a sheet, and the user fills in the sheet number and the sheet name.

---

## 2026-09-08, eighth pass. The panel was installed, and it was unusable

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#9](https://github.com/baderrahal/RCRC-Green/pull/9), one commit, seven files, merged into
main as `2b5361e`.

First install into Revit 2024. The pane opened and docked, which answers the biggest open
question from the round that built it. Then two faults, both confirmed from a screenshot.

### Every piece of text was invisible

A dockable pane on Revit's dark theme sits on a black background. WPF defaults a TextBlock's
Foreground to black. `DrawingSheetPanel` set a Foreground on nothing, so the headings, the
Prefix, From and To captions, the plot count, the status line and every grid label rendered
black on black. All of it was in the code and none of it could be read. The one place that
did name a colour, `Brushes.Black` on the row label, made it worse rather than better.

`PanelTheme` now reads `UIThemeManager.CurrentTheme` and hands back three brushes for that
theme, a background, a foreground and one warning colour. The panel sets Background and
Foreground on itself once. Foreground is an inherited property in WPF, so every TextBlock
under it picks the value up and none of them names a colour. `DrawingSheetPanel.cs` now
contains no brush and no `System.Windows.Media` using at all, which was checked by grep after
the last edit.

The values are white with near black text on the light theme, and #2E2E2E with #E6E6E6 on
the dark one. The no scope box mark is firebrick #B22222 on light and #FF8080 on dark,
because firebrick on dark grey is close to unreadable. Reasoned contrast ratios, computed
from the WCAG relative luminance formula rather than measured on a screen: 17.8 to 1 and 10.6
to 1 for the body text, 6.7 to 1 and 5.5 to 1 for the warning. All four clear 4.5 to 1.

`PanelTheme` reads the theme without going through the external event. That is a deliberate
exception and it is worth naming. A theme lookup touches no document, no transaction and no
element, and the panel has to paint itself while it is being built, which is during OnStartup
before any document exists. Routing it through the event would leave the panel unpainted
until the first refresh returns, which is the bug. The lookup lives in its own file so
`DrawingSheetPanel` still names no Revit type of its own.

The theme is re-read every time the pane becomes visible, so switching Revit between light and
dark while the pane is closed is picked up rather than needing a restart.

### The panel opened blank with no way to know why

`FillPrefixes` ran only when a refresh completed, and nothing asked for a refresh. So the
prefix dropdown was empty on open, and the line that would have said to press Refresh was one
of the invisible TextBlocks. The panel looked broken and said nothing.

The pane now asks for a refresh through the external event when it becomes visible, and again
on any later showing while it is still holding nothing. Not in the constructor, because a
dockable pane is built during OnStartup when no document exists.

The grid and a line of text now share one slot, and exactly one of them is visible. Every
path that leaves the grid empty goes through `Waiting`, so there is no state where the panel
shows a blank area:

| When | What it says |
|---|---|
| before the first read | Reading the model. |
| no document | No open document. Open a model and press Refresh. |
| model holds no plots | No plots in this model. No view carries a PRX_Plot_ID and no view name gives one. |
| plots found, no prefix picked | Pick a prefix above. From and To fill themselves with the plots under it. |
| range set, no columns | the grid draws the plot names, and the line above it says to add a column |
| range genuinely empty | No plots in that range. Widen From and To. |

The range with no columns is the one that does not use the empty slot. `SheetGrid.Build`
returns a row per plot even with no columns, so the grid does draw, showing the plot names and
which of them have no scope box. That is useful rather than broken, so the line above the grid
carries the instruction instead.

### Cells carry a mark, not a word

Eighteen plots by several columns of "exists" and "missing" is a wall of text. The three
states are now one character, filled square, empty square and filled circle, written as
`\u25A0`, `\u25A1` and `\u25CF` so the source file stays pure ASCII. csc reads a file with no
byte order mark in the machine's ANSI code page, and a literal box character in the source is
not the same character on every build machine.

The shape differs as well as the fill, so the three do not depend on reading fill weight. The
words moved into the tooltip and gained what a click does: exists, click to open it.

### Labels

`BOTTOM` was a layout name showing through into the interface. It reads `ACTIONS` now, and
`GRID` reads `PLOTS AND VIEW TYPES`. The Prefix, From and To captions were already docked
left of their dropdowns and stay there. The plot count line sits directly above the grid it
describes, which is why the add a column instruction went there rather than into the status
line, where a refresh would overwrite it.

### The mockup

`design/pr-9/panel.html` draws the layout in both themes with the exact values from
`PanelTheme`, the three cell marks with their tooltips, and the five empty states. The first
thing in the file says it is a mockup drawn from the code and not a screenshot, and that it
cannot show how Revit will render it. Every future round that changes the interface writes
one, which is now written into `.claude/rules/revit-commands.md`.

### Core

Nothing in Core changed. The theme, the mockup and every fix here are Revit side, and Core
holds no Revit type by design. No test was added. 176 tests pass, 0 failed and 0 skipped,
locally from a run made after the last file was written and again on the runner. Third round
at that number, which is what a Revit side round should give.

### Not observed, because it needs Revit

Nothing on this list has been seen. The whole round is a fix for something that was seen once,
in one screenshot, on one theme.

- neither theme has been rendered. The light palette and the dark palette are both reasoned,
  not observed. The contrast ratios above are arithmetic, not a measurement
- the dark theme is the one that failed, so it is the one that matters, and it is the one
  that has not been checked
- whether Revit paints anything of its own behind or around the pane that these colours sit
  badly against
- the buttons and the dropdowns keep the Windows control chrome, which is drawn light on both
  Revit themes. Readable, and not a match for the dark theme. This is not fixed and has not
  been looked at
- whether the font Revit gives the pane has a glyph for the three box characters. If it does
  not they come out as empty rectangles and the tooltip is all that is left
- whether `IsVisibleChanged` fires when a dockable pane is shown in Revit. The whole fix for
  the blank panel rests on that and it is taken from WPF, not from Revit
- whether the refresh raised from that event is accepted at that moment
- whether `UIThemeManager.CurrentTheme` is readable during OnStartup, when the panel is built
- whether a theme switch while the pane is closed is picked up on the next showing
- every empty state message. None has been on screen

### What comes next

Install and look at it on both themes. If the box characters have no glyph, the fallback is
plain ASCII and the tooltip already carries the words. After that, creation, working from the
marks.

---

## 2026-09-08, seventh pass. Three audit fixes, all the same shape

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#7](https://github.com/baderrahal/RCRC-Green/pull/7), one commit, five files, merged into
main as `20e214a`.

All three findings are one fault written three ways. A failure inside the panel was allowed
to reach past the panel. Nothing else was touched.

### 1. RegisterDockablePane was the only unguarded call in OnStartup

`CreateRibbonTab` was in a try and `PanelNamed` looked before it created, but
`RegisterDockablePane` stood bare between them. An exception there leaves `OnStartup`
throwing, and Revit answers a throwing `OnStartup` by building no ribbon at all. So a panel
that would not register took Scan Model and Scope Box down with it, and the user would have
seen an add-in that simply was not there.

`Registered` now wraps the call and returns whether it took. It catches
`Autodesk.Revit.Exceptions.ApplicationException`, which is the base of every Revit exception,
plus `InvalidOperationException` and `ArgumentException`, because `new DrawingSheetPanel()`
is inside the same try and building a WPF tree throws from the .NET side rather than the
Revit one.

The ribbon is then built either way. When registration failed the Drawing Sheet button is
still there and its tooltip reads that the panel is not available, its long description says
the Reports panel is unaffected and to restart Revit, and clicking it returns that same
sentence rather than opening anything. `ShowDrawingSheetCommand.PaneRegistered` carries the
one fact and `NotAvailable` carries the one sentence, so the button and the click cannot say
two different things.

A button that explains itself beats a button that is silently missing, because a missing
button reads as a broken install and sends someone looking in the wrong place.

### 2. Setting ActiveView on a view Revit will not activate

`open.ActiveView = view` throws `InvalidOperationException` on a view template, a legend and
others. The grid offers a cell for every view it read, so a single click on the wrong one
produced a crash dialog. It is caught now and the panel says the view cannot be opened,
naming it. The name is read off the view, so the user knows which cell did it.

### 3. The handler could still let an exception out

`Execute` caught `Autodesk.Revit.Exceptions.ApplicationException`, `UnauthorizedAccessException`
and `IOException`. The Revit API also throws the plain .NET exceptions, and a bad argument or
a call at the wrong moment arrives as `InvalidOperationException` or `ArgumentException`,
neither of which was covered. Both are caught now with their own message.

The requirement behind the finding is absolute. An exception leaving an
`IExternalEventHandler` does not raise a dialog, it ends Revit along with whatever was not
saved. Two more named types do not deliver that on their own, so the body moved into `Run`
and `Execute` is now a last resort catch around it. Every failure worth naming is still named
separately. `Stop` reports what got through by type and message, and it catches its own
report as well, because the panel can be gone by then and a throw out of the reporting would
be exactly the crash the catch exists to stop.

This is the one place in the repo where a catch of everything is right rather than lazy. It
is a boundary the process does not survive being crossed.

### Core

Nothing in Core changed, so no test was added. All three fixes are Revit failure paths, and
Core holds no Revit type by design. Adding a test here would have been a test written for the
sake of having written one.

The suite still runs and still passes, which is the check that these three edits broke
nothing that was already covered. 176 tests, 0 failed, 0 skipped, locally from a run made
after the last file was written and again on the runner. Same count as the round before,
which is what a round that changes no Core code should give.

### Not observed, because it needs Revit

None of the three paths was made to happen. Specifically:

- registration has not been made to fail, so the fallback button, its tooltip and the message
  on clicking it have never been seen
- no view template or legend has been clicked in the grid, so the message naming the view is
  untested
- no exception has been driven through `Execute`, so neither the two new named catches nor
  the last resort catch has ever run
- whether Revit really does build no ribbon when `OnStartup` throws is taken from the finding
  rather than observed here

### What comes next

Unchanged. Run the panel in Revit and compare its counts against Scan Model and Scope Box on
the same model. Then creation, working from the marks.

---

## 2026-09-08, sixth pass. The Drawing Sheet panel, and PRX_Plot_ID as the first source

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#5](https://github.com/baderrahal/RCRC-Green/pull/5), one commit, 25 files, merged into main
as `999381c`.

This is the first round built on a real model rather than on the project facts alone. Both
commands were run on RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached and the numbers came back:
96,934 elements in 1.4 seconds, 1,385 sheets, 953 views on sheets, 2,430 views not on
sheets, 79 view templates, 406 scope boxes, 160 distinct PRX_Plot_ID values, 2,114 names
parsed and 4,039 not. Scope Box sorted them A 1,269, B 972, C 849, D 57, E 49, F 187, and
849 views were assigned.

Three of those numbers overturned something that had been assumed for five rounds.

### What was built

**Plot detection changed.** `ViewPlotReader.Read` takes the PRX_Plot_ID parameter on the view
first and falls back to the name only when the parameter is empty or absent. It reports which
source answered, as `Parameter`, `ViewName`, `ParameterNotAPlot` or `None`. 1,269 views were
skipped by Scope Box for no reason other than a name that does not parse, and the parameter
holds the plot for most of them. The parser is untouched, because the same run confirmed it
correct on the 2,114 names that do parse. A parameter holding something that is not a plot
identifier is its own answer rather than a silent fall through to the name, because a view
carrying a wrong PRX_Plot_ID and a right name is a data problem someone needs to see.

**A dockable panel.** `DrawingSheetPanel` is an `IDockablePaneProvider` registered in
`OnStartup`, built in C# rather than XAML because an SDK style net48 project has no XAML
compilation step. `ShowDrawingSheetCommand` shows it from a button on a new ribbon panel.

**One route to the API.** Every model action goes through `DrawingSheetRequestHandler`, an
`IExternalEventHandler`, and one `ExternalEvent`. `DrawingSheetPanel.cs` names no `Document`,
no `Transaction`, no `FilteredElementCollector` and no `ElementId`, which was checked by
grep after the last edit. It holds a `DrawingSheetSnapshot` of plain values, which is also
what lets it stay open while the user closes one document and opens another.

**The range is the main control.** Prefix, from and to are dropdowns filled from plots the
model holds. There is no free text entry for a plot anywhere in the panel, so no identifier
the tool invented can be offered or acted on. Everything below the range is disabled until a
range is set. `PlotRange` does the filtering in Core, and a range whose ends are the wrong
way round returns nothing rather than throwing.

**The grid.** One row per plot in range, one column per view type, cells reading exists,
missing or marked. `SheetGrid` builds it in Core, and an existing view can never be marked.
A row label carries a mark when no scope box named for that plot exists. Clicking a cell that
holds a view selects and shows it in Revit through the external event. Clicking one that does
not toggles a mark, which records intent and writes nothing. Nothing creates a view or a
sheet this round.

**Assign Scope Boxes on the panel** runs the untouched case A to F logic over the plots on
screen, through the same confirmation dialog and the same report file as the button.
`Confirmed`, `Assign` and `WriteReport` became internal so there is one copy rather than two
that drift.

**Two ribbon panels.** Drawing Sheet holds the panel. Reports holds Scan Model and Scope Box,
working exactly as before. They read the whole model rather than a range, which is what makes
them the check on the panel. `PanelNamed` looks through `GetRibbonPanels` before creating, so
a reload does not stack duplicates.

**No progress window on the panel read.** The audit finding about a slow read was wrong.
1.4 seconds for the whole document, and this read touches only views and scope boxes.

### Tests

176 pass, 0 failed, 0 skipped, locally from a run made after the last file was written and
again on the runner, which reported 176 tests ran. 40 are new this round, covering the range
filter, the grid shaping and the source ordering.
The five the brief named are all there: DM-11 to DM-28 returns those and nothing outside,
a reversed range returns nothing, the parameter beats the name, the name is used when the
parameter is empty, and DM-2 sorts before DM-100.

### A hook that refused a commit that was fine

`commit-scope.py` tokenises the bash command with `shlex` and `punctuation_chars=True`, which
splits `2>&1` into `2`, `>&` and `1`. The `2` read as a path being committed, so the commit
looked like `git commit 2`, which carries nothing, and `require-file-on-commit.sh` refused it
for not carrying the state file. Fixed by replacing the single break pattern with
`ends_the_command`, which knows a bare file descriptor in front of a redirection belongs to
the redirection while a digit in front of a pipe is still a path. Checked by hand against
seven command forms, including the one that broke and the path literally named 2.

This is the mirror of the entry already in `CLAUDE.md`. It failed closed, so nothing went
through unchecked, and that is the difference between an hour lost and a rule quietly not
applied.

### Not observed, because it needs Revit

None of this was run. It compiles against the Revit 2024 reference assemblies and that is
the whole of what can be said from here. Specifically untested:

- whether `RegisterDockablePane` in `OnStartup` is accepted, and whether the pane appears
- whether the pane docks, resizes, and keeps its position across a Revit restart
- whether the WPF tree built in code renders as intended at any DPI or theme
- whether the `ExternalEvent` fires and whether `Raise` from a Windows event handler is
  accepted at the moments the panel raises it
- whether clicking a cell actually selects and shows that view in Revit
- whether the prefix, from and to dropdowns fill from a real model, and what they hold for
  the 160 plots in it
- whether the panel survives the user closing and reopening the document, which is the case
  the snapshot exists for
- whether PRX_Plot_ID is readable by `LookupParameter` on a view, and how many of the 1,269
  it actually recovers. The number in this entry is what the parameter could recover, not
  what it did
- whether Assign Scope Boxes from the panel produces the same report as the button
- whether the two ribbon panels appear with the right buttons on them
- what a model with no PRX_Plot_ID anywhere does to the panel

### What comes next

Run the panel in Revit and compare its counts against Scan Model and Scope Box on the same
model, which is what the Reports panel is for. Then creation, which is the first thing that
makes a view rather than reporting one, and the marks are the list it works from.

---

## 2026-09-08, fifth pass. Scope Box, the first command that writes, merged into main

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main. Pull request
[#4](https://github.com/baderrahal/RCRC-Green/pull/4), one commit, 18 files, merged into main
as `c5b8c9a`.

### What was built

Scope Box, the second command on the Sheets panel, and the first thing in this repo that
writes to a model. A new view with no scope box is useless on this project, so assigning one
belongs to the Drawing Sheet work rather than being a tool of its own. It applies to every
view that names a plot, not only newly created ones.

Every view that is not a sheet and not a view template goes into exactly one case.

| | Condition | What happens |
|---|---|---|
| A | name does not parse to a plot | counted and listed |
| B | cannot hold a scope box | counted only, no list |
| C | no scope box, and one exists named for the plot | assigned |
| D | no scope box, and none matches the plot | reported, nothing changed |
| E | already holds the scope box for its plot | counted |
| F | already holds a different scope box | reported, nothing changed |

C is the only case that writes. Matching is exact and case sensitive, the same rule the plot
identifier follows. Templates are left out because a template's scope box would push onto
every view using that template.

Everything is decided with no transaction open, so the counts in the dialog cannot change
between being shown and being acted on. On yes, one transaction named Assign scope boxes
covers every assignment, so the whole run is one undo. On no, the report is written anyway
and nothing changes. A view that refuses the assignment is recorded and named in the report
rather than thrown, so one awkward view does not roll back every other assignment.

Sorting the views into the six cases, ordering them, counting them and laying out the report
are all Core work, fed plain strings and identifiers. That is why all of it has tests and none
of the tests needs Revit.

### The audit items

| Item | State |
|---|---|
| 1, the scan sat outside the try | Fixed. Guarded on `Autodesk.Revit.Exceptions.ApplicationException` so a genuine programming error still crashes loudly, with a message naming the document |
| 2, no progress and no way out on a long read | A progress window with a bar and a Cancel button, on both commands. How long a read takes on a real model is UNKNOWN |
| 3, `CreateRibbonPanel` unguarded | Fixed. The existing panel is found through `GetRibbonPanels` and reused, so a second registration no longer gives a second Sheets panel |
| 4, delete the merged branch | NOT DONE. See below |

**Item 2 and the measurement.** The audit asked what the read was measured at. It was not
measured, because there is no Revit here. The progress window pumps the message queue at most
once every tenth of a second, which is a guess at a sensible interval and not a measured one.
The pumping uses `Application.DoEvents`, which is the usual pattern in an add-in and is not
free of risk, because it lets other clicks through while it runs. It is used only while
reading and never while a transaction is open, so nothing half written can be reached that
way.

**Item 4 and why it did not happen.** `git push origin --delete` and the older
`git push origin :branch` were both tried, five attempts in total with backoff between them.
Every one failed identically with `send-pack: unexpected disconnect while reading sideband
packet`, while ordinary fetches and pushes over the same connection kept working. That is the
git proxy in this environment refusing a ref deletion rather than a network fault, and there
is no branch delete in the GitHub tools available here. The branch still exists at `ec51119`,
which is merged into main. Deleting it is one click on the Delete branch button on pull
request 4, or one line locally.

```
git push origin --delete claude/rcrc-green-setup-wf9ham
```

### Checks that actually ran

Local, on main at `c5b8c9a`, clean working tree, 2026-09-08 11:22:25 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 136, Skipped: 0, Total: 136
```

On the runner, run
[34220252821](https://github.com/baderrahal/RCRC-Green/actions/runs/34220252821) against
commit `ec51119`, the tree that merged:

```
Passed!  Failed: 0, Passed: 136, Skipped: 0, Total: 136
136 tests ran.
```

Both numbers are 136. The suite was 109 before this round.

Also run: `dotnet build RcrcGreen.sln -c Release` succeeded with 0 warnings and 0 errors, and
the add-in output still holds only the two project assemblies, their symbol files and the
manifest. The source was searched for transactions and there is exactly one, in the one
command that writes. Core was searched for a Revit reference and has none. The report was
rendered once from a fixture and read by eye, which caught a line reading one views were given
a scope box.

### What is untested because it needs Revit

All of the Revit side, and there is more of it this round than last, because this one writes.
`dotnet build` against the Revit 2024 reference assemblies says the API calls exist and take
the arguments given. It says nothing about behaviour.

Named specifically, none of this has been observed:

- the second button appearing beside Scan Model, and the panel being reused rather than doubled
- the progress window showing above the Revit window, its bar moving, and Cancel answering
- `VIEWER_VOLUME_OF_INTEREST_CROP` reading and writing the way this code assumes
- `Parameter.Set` returning false on a view that refuses, rather than throwing
- the single transaction showing up as one undo step
- whether `Application.DoEvents` causes trouble inside a Revit command on a real model

### Known bugs

None seen in a run, and only Core has been run.

### Left to do

- Phase 10, packaging, has not started
- Nothing creates a view, a sheet or a section
- The grid and the missing view report exist in Core but no command uses them yet
- Eight of the twelve open questions are still open
- The merged branch is still on the remote, as above

### Next

Two runs on a real model, in this order. Scan Model first, because the parse summary decides
whether the naming pattern needs replacing, and Scope Box reads plots through that same
parser. Then Scope Box on a copy of the model, answering no the first time, so the six counts
can be read before anything is written. Case D is the number to look at. A large D means the
scope boxes are not named the way the project facts say they are, and that changes what the
creation logic can assume.

---

## 2026-09-08, fourth pass. Scan Model, the first command, merged into main

The add-in was confirmed loading in Revit 2024.3 before this round started. The RCRC Green
tab and the Sheets panel both appear, which is the first thing in this repo anyone has seen
work in the host program.

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main. Pull request
[#3](https://github.com/baderrahal/RCRC-Green/pull/3), one commit, 18 files, merged into main
as `04ff9fc`.

### Why this command exists

The parser in Core was written from four example names the user gave. The first real model
holds a sheet numbered 600QD named SOFTSCAPE SCHEDULES, which matches no part of that
pattern, and a model called NG05, which has no dash in it. Scan Model reads a document and
writes down what is really there, so the naming is known rather than assumed. It creates
nothing and changes nothing.

That also means the project facts in `CLAUDE.md` are now suspect. A note saying so sits in
the Things that have gone wrong before section of that file. The parse summary from a real
run is the source of truth about naming from here, not the four examples.

### What was built

**Revit project.** A text only PushButton on the Sheets panel, deliberately with no icon.
`ScanModelCommand` behind it, marked `[Transaction(TransactionMode.ReadOnly)]`. `ModelScanner`
reads the document into plain strings and numbers.

**Core.** `ModelScan` holds what was read. `ScanReport` turns it into the text of the file.
`NameParseSummary` and `NameParseTally` run every name through `ViewNameParser` and count the
answers. `ScanFileName` builds the file name and takes out anything Windows will not accept
in a path. `ScannedSheet`, `ScannedView`, `ScannedScopeBox` and `ScannedParameterValue` are
the carriers.

The report has seven sections in the order the brief asked for, each heading carrying its own
count, so a section that found nothing reads differently from one that was never filled in.

### Decisions worth knowing about

**Views on sheets come from Viewports and from ScheduleSheetInstances.** Counting only
Viewports, which is what `GetAllPlacedViews` does, misses every schedule placed on a sheet.
The first model has a whole sheet of them.

**A ViewSheet is a View, so sheets are kept out of the view sections.** Leaving them in would
count every sheet twice and file it under views that are not on a sheet, which reads as a
fault in the model rather than a fault in the report. The report header says this.

**PRX_Plot_ID is looked up by name**, so it works whether the team set it up as shared or
project, on an instance or a type. The type answer is cached per type, because asking once
per element on a model with a hundred thousand of them is the difference between seconds and
minutes. The element count and the elapsed time both go in the report.

**The refusal list shows up to twenty per kind rather than twenty in total.** The brief reads
either way. Each block says how many of how many, so nothing is hidden, and one mixed list of
twenty could have been all sheet names. Easy to change if that is the wrong reading.

**View template names are not run through the parser.** The brief named three kinds and a
template gets its own section. The report says so in the parse summary.

### Checks that actually ran

Local, on main at `04ff9fc`, clean working tree, 2026-09-08 10:52:04 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 109, Skipped: 0, Total: 109
```

On the runner, run
[34217596029](https://github.com/baderrahal/RCRC-Green/actions/runs/34217596029) against
commit `ae8b061`, the tree that merged:

```
Passed!  Failed: 0, Passed: 109, Skipped: 0, Total: 109
109 tests ran.
```

Both numbers are 109. The suite was 79 before this round.

Also run: `dotnet build RcrcGreen.sln -c Release` succeeded with 0 warnings and 0 errors, and
the add-in output still holds only the two project assemblies, their symbol files and the
manifest. The source was searched for a transaction being opened and there is none, only the
attribute that declares the command opens none. The report was rendered once from a fixture
and read by eye, which is how the column layout was settled.

### What is untested because it needs Revit

All of it, on the Revit side. The command has never been run. `dotnet build` against the
Revit 2024 reference assemblies says the API calls exist and take the arguments given. It
says nothing about whether the button appears on the panel, whether the scan finds what it
should in a real document, how long it takes on a real model, or whether the file lands on
the Desktop. Anyone reading a green test count here should know it covers Core only.

Named specifically, none of this has been observed:

- the button rendering with text and no icon
- `ScheduleSheetInstance.IsTitleblockRevisionSchedule` behaving as expected on a real titleblock
- `get_BoundingBox(null)` on a scope box giving the extent the team expects
- the speed of reading PRX_Plot_ID across a full model
- the Desktop write, including on a machine where that folder is redirected to a network share

### Known bugs

None seen in a run, and only Core has been run.

### Left to do

- Phase 10, packaging, has not started
- Nothing creates a view, a sheet or a section
- The grid and the missing view report exist in Core but no command uses them yet
- Eight of the twelve open questions are still open
- `--amend` is still the one commit form `commit-scope.py` does not model

### Next

Run Scan Model on the real model and read the file. Everything after this depends on what it
says. The parse summary decides whether `ViewNameParser` needs a second pattern, a looser
one, or replacing, and the PRX_Plot_ID section decides whether the plot list has anything to
work from at all. Until that file exists, building the grid on top of the current pattern
would be building on the same guess this command was written to test.

---

## 2026-09-08, third pass. The fourteen reviewed items fixed, merged into main

A fix round on the scaffold, not new work. Fourteen items from the reviewed phase 8 findings
and nothing else. The other findings stay written down below as reported and untouched.

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because its first pull request
had already merged. Pull request [#2](https://github.com/baderrahal/RCRC-Green/pull/2), one
commit, 26 files, merged into main as `57dd1ee`.

### Three answers, now project facts

They are written into the project facts in `CLAUDE.md`, and questions 1, 2 and 4 in the
open list below are marked ANSWERED with the answer beside them. Question 8 is marked PARTLY
ANSWERED. None of the twelve was deleted.

- A plot identifier is always two uppercase letters. Lowercase is invalid, not a second plot
- A cross section looks 10 metres, a starting value the team will change after testing
- View names repeat word for word across plots, and nothing plot specific appears in one

### What was fixed

| Item | Findings | What changed |
|---|---|---|
| 1 | 37, 42 | `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest asks for, which the build does not produce. Both documented in `CLAUDE.md` |
| 2 | 43 | `LangVersion` on Core pinned to 12.0, the same version the test project resolves to |
| 3 | 2, 3, 4 | Surrounding whitespace, tabs, carriage returns and newlines come off identifiers and view names before anything is compared or keyed on |
| 4 | 5 | Plot identifiers are uppercase only. A lowercase one reaches the ignored list carrying `IgnoredReason.WrongCase`, which is why that list holds entries rather than bare strings |
| 5 | 7 | `NaturalOrder` sorts the number part as a number, so DM-2 comes before DM-100 and code 200 before code 1000 |
| 6 | 12 | `SectionPlacement.Length` scales by the largest delta instead of squaring all three |
| 7 | 9 | Already done in `7ae6759` last round, with tests. Checked rather than redone |
| 8 | 13 | Depth is a required argument, in the caller's unit, handed back untouched. Core derives nothing from the box |
| 9 | new | `SectionDefaults` in the Revit project holds the 10 metres and converts it to feet |
| 10 | 18 | `writing-check.sh` reads the commit message as well as the files |
| 11 | 14, 22, 34 | Both commit hooks read the command through `commit-scope.py` and work out what the commit will really carry |
| 12 | 24 | A hook script that cannot be found blocks instead of exiting 127 and being read as a non blocking error |
| 13 | 38, 39, 40, 41 | Four tests rewritten against values written out by hand |
| 14 | none | The three answers recorded, four questions marked |

### The four tests, broken then fixed

Each was proved by editing the code, running the suite, reading the result, and putting the
code back. The suite came back to 79 green afterwards with only the intended changes in the
tree.

- finding 38, `APlotMissingATypeEveryOtherPlotHasIsReportedMissing`. Broke `MissingFor` to
  return every column. RED as intended, 3 failed and 76 passed.
- finding 39, `APlotWithNoViewsGetsARowAndEveryColumnReadsMissing`. Deleted the line that
  adds a column, so the grid has none. RED as intended, 8 failed and 71 passed.
- finding 40, `EveryPlotWithAScopeBoxGetsASectionAcrossTheMiddleOfIt`. Put the old rule back
  so depth came from half the box extent instead of the argument. RED as intended, 6 failed
  and 73 passed.
- finding 41, the same test. Flipped the comparison that picks the short side. RED as
  intended, 7 failed and 72 passed.

Before the rewrite each of those four stayed green under the same break.

### Checks that actually ran

Local, on main at `57dd1ee`, clean working tree, 2026-09-08 10:24:22 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 79, Skipped: 0, Total: 79
```

On the runner, run
[34215178742](https://github.com/baderrahal/RCRC-Green/actions/runs/34215178742) against
commit `2cc4df5`, the tree that merged:

```
Passed!  Failed: 0, Passed: 79, Skipped: 0, Total: 79
79 tests ran.
```

Both numbers are 79. The suite was 48 before this round.

Also run: `dotnet build RcrcGreen.sln -c Release` succeeded with 0 warnings and 0 errors, and
the add-in output still holds only the two project assemblies, their symbol files and the
manifest. Both `LangVersion` values were read back with `dotnet msbuild -getProperty` and
both are 12.0. Every hook was run by hand in a scratch repository against every commit form
named in item 11, plus each of the five things the writing rules ban, in the message and in
the files, and the missing script case for item 12.

### Known bugs

None seen in a run. Two things found while doing this work and fixed in the same commit,
both worth naming because neither came from the findings list.

`require-file-on-commit.sh` captured the NUL separated path list in a shell variable, and a
command substitution drops NUL bytes with a warning. The whole list collapsed into one line
and every commit was refused. Caught by running the hook rather than by reading it.

`commit-scope.py` read past the end of the {G} invocation into the rest of a chained command,
so `{G} -m x && git log` read the words of the second command as pathspecs and found no
files. Fixed by splitting on shell operators, and the parser is now checked against eleven
command shapes including quoting, redirects and two commits on one line.

### What was deliberately not touched

The other 39 findings. The list below is unchanged.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024, so nothing proves the ribbon appears
- `install.ps1` and `uninstall.ps1` have never been run. There is no PowerShell and no Revit
  on this machine, so whether they work is UNKNOWN
- Eight of the twelve open questions are still open
- `--amend` is the one commit form `commit-scope.py` does not model. It falls back to reading
  the index, which is what both hooks did for every form before this round

### Next

Write the first command behind the Sheets panel. Core now covers the naming, the plot list,
the grid and the section placement, and the answers that were blocking it have arrived. The
Revit side cannot be checked on this machine, so that work stays in one session and each
round trip costs the user a load of Revit. Running `install.ps1` once on a real machine is
the cheapest check left, and it is the one that decides whether anyone can use this at all.

---

## 2026-09-08, later the same day. Pushed, merged into main, phase 8 written up

### What was done

The branch was pushed and landed. GitHub access to this repository was granted between the
two halves of the session, so the push that was refused twice earlier went through on the
third try without any change to the commits.

The branch is named `claude/rcrc-green-setup-wf9ham`, not `setup/harness` as the brief asked.
That name was fixed by the session before any work started and could not be chosen here.

- pull request [#1](https://github.com/baderrahal/RCRC-Green/pull/1), four commits, 39 files
- opened as a draft, then taken out of draft, then merged into main as `17f1850`
- the GitHub app appended a generated-by footer to the pull request body on creation. It was
  removed by rewriting the body, and the rewrite held

Phase 8 ran as six reading passes over the repo. All six finished and reported 53 findings
between them, written up in the entry below under Phase 8 review findings, one line each,
nothing fixed and nothing ranked. A seventh pass that would have tried to refute each finding
was cancelled before it finished, so no finding has been checked by a second reader.

### Checks that actually ran

Local, on main at `17f1850`, with a clean working tree, at 2026-09-08 09:53:00 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 48, Skipped: 0, Total: 48
```

On the runner, `.github/workflows/tests.yml` run
[34212285598](https://github.com/baderrahal/RCRC-Green/actions/runs/34212285598) against
commit `d3db9c8`, which is the same tree that merged:

```
Passed!  Failed: 0, Passed: 48, Skipped: 0, Total: 48
48 tests ran.
```

Both numbers are 48, so there is no gap between what was run here and what the runner ran.
The second line is the zero test gate reading the count back out of the result file, which is
the first time that gate has run on a real runner rather than by hand.

### Correction to the entry below

The entry below reports 41 tests in two places, at the line about the test run and at the line
about the count parsing. That number came from a run made before the two hardening commits,
and it was left behind when those commits added tests. The real count is 48. The wrong number
is left in place rather than edited, because the entry is the record of what was believed at
the time, and two of the phase 8 findings are about exactly that mistake.

### Known bugs

None seen in a run. 53 findings from six reading passes are listed in the entry below and
none of them has been through a second reader, so treat that list as reported rather than
confirmed. Three of the breaker findings on Core describe code that has since changed, which
is noted at the head of that list.

The one finding worth naming here, because it stops the tool working rather than making it
wrong: `src/RcrcGreen.Revit/RcrcGreen.addin` names a `RcrcGreen` subfolder for the assembly
while the build copies the manifest flat, beside the two assemblies. Two separate passes
reported it. Anyone following the layout in the file header has to build that subfolder by
hand or Revit will not load the add-in.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024. Nothing here proves the ribbon appears
- Nothing has been done about any of the 53 findings

### Next

Two things, in this order.

First, put the 53 findings in front of a person and decide which are real. They were reported
by readers, not confirmed by a second pass, and a list that size acted on blind will churn the
repo. The manifest path is the one to look at first.

Second, take the twelve open questions in the entry below to the team. Several of them, the
case sensitivity of a plot identifier and how far a cross section should look in particular,
change what the first command has to do, so answering them is cheaper before that command is
written than after.

The first command behind the Sheets panel comes after those two. Nothing in the Revit project
can be checked on this machine, so that work stays in one session and each round trip costs
the user a load of Revit.

---

## 2026-09-08. Repo set up, Core logic and tests written, ribbon stubbed

Entered at phase 3 with one commit in the repo. Ran phases 3 through 9.

### What was done

**Scan, phase 3.** The repo held `.claude/skills/ai-max` and nothing else. Table below.

```
ITEM          | STATE   | PATH                        | FACT
Rules file    | ABSENT  | CLAUDE.md                   | looked for, not there
Scoped rules  | ABSENT  | .claude/rules/              | looked for, not there
Skills        | PRESENT | .claude/skills/ai-max/      | SKILL.md plus assets and references
Agents        | ABSENT  | .claude/agents/             | looked for, not there
Commands      | ABSENT  | .claude/commands/           | looked for, not there
Hooks         | ABSENT  | .claude/settings.json       | no settings file of any kind
Test gate     | ABSENT  | .github/workflows/          | no .github folder
Tests         | ABSENT  | tests/                      | no test folder and no test file
Language      | ABSENT  | .                           | no source file of any language
Connections   | ABSENT  | .mcp.json                   | looked for, not there
Editor tasks  | ABSENT  | .vscode/tasks.json          | looked for, not there
Ignore file   | ABSENT  | .gitignore                  | looked for, not there
Branch state  | PRESENT | claude/rcrc-green-setup-wf9ham | level with origin/main at 10eed2f
```

Everything absent mattered here, because nothing existed to build on.

**Projects.** `RcrcGreen.sln` holding three.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference of any kind
- `src/RcrcGreen.Revit`, net48, references Core and the two Nice3point Revit 2024 packages
  with `PrivateAssets=all` and `ExcludeAssets=runtime`, so no Revit assembly is copied to
  the output folder
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, referencing Core only

**Core.** `ViewNameParser` and `ParsedViewName` for the naming. `ViewType` for a code and a
view name held together. `PlotSource`, `PlotRecord` and `PlotRegistry` for the plot list.
`PlotViewGrid`, `MissingViewType`, `PlotMissingViews` and `MissingViewFinder` for the grid
and the missing report. `PlotBox`, `SectionAxis`, `Point3D`, `Vector3D` and
`SectionPlacement` for the section maths.

**Revit.** `RcrcGreenApplication` makes the RCRC Green tab with an empty Sheets panel.
`RcrcGreen.addin` is written for the per user path and holds no machine specific path
inside it. No commands.

**Harness.** `CLAUDE.md` at 134 lines. Three hooks and a SessionStart line in
`.claude/settings.json`. `breaker` and `claim-checker` in `.claude/agents/`, read only.
`.github/workflows/tests.yml` running on pull requests into main.

### Checks that actually ran

`dotnet` was not on this machine. The .NET SDK 8.0.424 was installed to run anything at all,
which is worth knowing because it means the local run and the runner run are on the same
major version.

- `dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj` passed with 41 tests,
  0 failed, 0 skipped
- `dotnet build src/RcrcGreen.Revit/RcrcGreen.Revit.csproj` succeeded with 0 warnings and 0
  errors, on Linux with no Revit installed, which is the point of the reference packages
- the Revit output folder was listed by hand and holds only `RcrcGreen.Revit.dll`,
  `RcrcGreen.Core.dll`, their symbol files and `RcrcGreen.addin`. No `RevitAPI.dll` and no
  `RevitAPIUI.dll`
- all three hooks were run by hand against crafted input. `block-paths.sh` allowed two
  paths inside the repo and refused `/tmp`, a path escaping through `..`, and a sibling
  folder whose name starts with the repo name. `require-file-on-commit.sh` allowed a
  non commit command and refused a commit. `writing-check.sh` refused an em dash, a
  generated-by footer, a co-author credit line, an emoji and a banned word, allowed the
  word landscape, allowed the word elevation, and skipped a banned word planted under
  `.claude/skills/`
- the count parsing in the workflow was tried against a real result file written here and
  read back 41

### Known bugs

None seen. Nothing in the list below is a bug, it is a question with no answer yet.

### UNKNOWN, waiting on the team

Every one of these was left as written rather than guessed. Where the code had to do
something, what it does is written next to the question.

1. ANSWERED. Are plot identifiers case sensitive. They are. A plot identifier is always two
   uppercase letters, and lowercase is not a different plot, it is invalid.
2. ANSWERED. Are the two letters always uppercase. Yes. The parser accepts uppercase only,
   and a lowercase or mixed case identifier reaches the ignored list marked WrongCase.
3. May a view name be empty after the bracket and the space. Today it must hold at least
   one character, so `DM-41-(010) ` does not parse.
4. ANSWERED. How far should a cross section look. 10 metres, as a starting value the team
   will change after testing in Revit. `SectionPlacement` no longer works the depth out from
   the box, it takes it as a required argument and hands it back untouched. The 10 metres
   lives in `SectionDefaults` in the Revit project and is converted to feet there.
5. Which way should a cross section face. Nothing was stated. The view direction is the
   line direction crossed with world up, so the same box and axis always give the same
   side.
6. What height does the section line sit at. It sits at the middle of the box in Z.
7. What happens on a square plot, where the short side and the long side are the same
   length. The tie goes to Y as the short side, so the answer does not move between runs.
8. PARTLY ANSWERED. Does every plot need every view type. Still open. What is settled is
   that view names repeat word for word across plots, so the same view type on two plots
   carries the same text and nothing plot specific appears in a view name. The grid still
   puts every type found anywhere across the top of every row, so a type that only ever
   applies to one kind of plot will read as missing everywhere else.
9. Can a scope box name carry more than the plot identifier, for example `DM-41 working`.
   Anything that is not exactly a plot identifier goes into `PlotRegistryResult.Ignored`
   rather than being read as a plot or dropped in silence.
10. Is there a fixed list of codes. Nothing validates a code beyond it being digits.
11. What unit are the plot box numbers in. `PlotBox` takes plain numbers and returns plain
    numbers in the same unit. Revit works in feet internally, so the add-in side will have
    to be explicit about this when it reads a scope box.
12. Do sheets and views ever need telling apart by name. They are named the same way, so
    the parser does not try.

### Blocked on GitHub access

The branch could not be pushed and no pull request exists. `git push` and the GitHub API
both refuse, and the refusal is an authorisation one rather than a network one, so a retry
does not help. It was tried twice.

```
remote: Claude doesn't have GitHub access to baderrahal/RCRC-Green for your organization.
fatal: unable to access 'https://github.com/baderrahal/RCRC-Green/': The requested URL returned error: 403
```

Creating the branch through the API gives `403 Resource not accessible by integration`.
Reading the repository works, writing to it does not. The remedy named in the refusal is to
install the Claude GitHub App at https://github.com/apps/claude/installations/select_target,
or to reconnect GitHub from https://claude.ai/customize/connectors and re-link the existing
installation. Until one of those happens, the commits live only on the local branch
`claude/rcrc-green-setup-wf9ham` and anyone downloading main gets the ai-max skill and
nothing else.

The workflow in `.github/workflows/tests.yml` has therefore never run on a runner. It was
checked by pulling the gate step out of the file and running it by hand against a real
result file, an empty results folder, and a result file reporting zero tests. It passed the
first and failed the other two, which is what it is for.

### Phase 8 review findings

Six review lenses ran and all six finished. The Verify phase that would have tried to refute
each of these was cancelled, so nothing from it appears here and no finding below has been
tested by a second agent. Reporting only, nothing fixed, and no ranking beyond the order the
lenses reported in. 53 findings, in the shape LENS | file and line | what is wrong | why it
matters.

The tree moved while the review ran. The breaker-core findings on PlotId.cs, ViewNameParser.cs
and PlotBox.cs describe the tree at commit 9975614, before the two hardening commits, which is
visible in the wording of those findings. The other five lenses read the tree at or after
commit 7ae6759.

breaker-core, 13 findings

1. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:58 | Columns are built only from names that parsed, so a model where no name matches the pattern gives rows and no columns | Every plot then reports nothing missing and reads as finished, which is a silent all clear on a model where nothing was understood
2. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:60 | A plot identifier carrying a leading space, a trailing space or a tab is never trimmed and fails the pattern | The plot gets no row and no missing views, and the loss sits in Ignored beside every ordinary view name, which is the scope box only plot the brief calls the case that matters most
3. breaker-core | src/RcrcGreen.Core/PlotId.cs:16 | The end of line anchor lets a trailing newline pass, and the dictionary is then keyed on the raw string | One plot appears twice down the side of the grid and the second row reads every view type missing, so a full set of duplicate views is offered
4. breaker-core | src/RcrcGreen.Core/ViewNameParser.cs:12 | The view name group keeps a trailing space or a carriage return, and view types compare as raw text | Two columns print identically on screen and both plots are told to create a view that already exists
5. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:25 | The pattern accepts either case and the dictionary compares ordinally, so dm-41 and DM-41 are two plots | The count is one too high and the two rows are wrong in opposite directions, one showing everything present and one showing everything missing
6. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:26 | The ignored list keeps duplicates and records no source, so a mistyped scope box sits among thousands of ordinary view names | The check is present and unusable, which reads as covered when it is not
7. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:42 | Rows and columns sort as text, so DM-100 lands before DM-2 and code 1000 before code 200 | The grid looks sorted so nobody checks it, and reading a plot against the wrong row leaves no trace
8. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:66 | A view name carrying anything plot specific becomes its own column, measured at 10000 columns and 1990000 missing entries for 200 plots | Every plot is reported as missing about ten thousand types, each held by one other plot, and the report takes three seconds and 171 MB
9. breaker-core | src/RcrcGreen.Core/PlotBox.cs:81 | The bound check rejects NaN but not infinity, and the centre is computed as min plus max over two | An infinite bound yields a fully formed placement with infinite coordinates, and a finite but huge box gives a centre of infinity while the two ends look fine
10. breaker-core | src/RcrcGreen.Core/ParsedViewName.cs:8 | The only public constructor in Core with no argument checks | A null plot identifier surfaces later as a dictionary key error naming no view, so the message points at the wrong place
11. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:43 | The grid accepts any string as a row while the registry checks the same data against the plot pattern | A scope box called Working box gets a row reading every view type missing, so the two entry points in Core disagree about what a plot is
12. breaker-core | src/RcrcGreen.Core/SectionPlacement.cs:55 | Length squares the deltas rather than using a hypotenuse computation | A box a fraction of a unit wide reports length zero and a box spanning most of the double range reports infinity, so a caller testing for a degenerate line gets the wrong answer
13. breaker-core | src/RcrcGreen.Core/SectionPlacement.cs:90 | A short side cut is given a depth of half the long extent of the plot | On a corridor plot two kilometres long the cross section pulls in a kilometre of model, which is slow and unreadable in Revit
    breaker-core reported nothing in the Repeat category.

breaker-hooks, 16 findings

14. breaker-hooks | .claude/hooks/writing-check.sh:122 | The hook reads the index, but the -a form and the pathspec form stage their content after the hook has returned | The hook reports success on a commit it never looked at, which is worse than not existing because the author believes the gate ran
15. breaker-hooks | .claude/hooks/block-paths.sh:11 | NotebookEdit passes notebook_path, not file_path, so the extraction gets an empty string and exits clean | The matcher advertises cover that does not exist, and a notebook write to any location on disk passes while the same Write is refused
16. breaker-hooks | .claude/hooks/block-paths.sh:11 | With python3 absent the extraction fails into an empty string that is indistinguishable from a missing key | All three hooks fail open together with no message, which matters most on a Windows team where python3 often resolves to a stub that exits non-zero
17. breaker-hooks | .claude/hooks/writing-check.sh:80 | The word pattern matches only the bare dictionary form, so a trailing s, d or ing breaks the boundary | The bare forms are caught so the hook looks like it works, while the forms these words take in real prose go straight through
18. breaker-hooks | .claude/hooks/writing-check.sh:122 | The commit message itself is never read, only staged file content | The brief applies the writing rules to every commit message and names the co-author credit line as the loudest tell, so the place the rule matters most is the place nothing enforces it
19. breaker-hooks | .claude/hooks/require-file-on-commit.sh:28 | grep -q exits on the first match, git dies of a broken pipe, and pipefail turns that into a failure | A commit that does carry the state file is refused once the staged list passes about two thousand paths, and the message sends the author looking for a problem that is not there
20. breaker-hooks | .claude/hooks/require-file-on-commit.sh:13 | The command is matched on the phrase anywhere in it | Searching the log for the phrase is refused, and a real commit in an unrelated repository on the same machine is refused too
21. breaker-hooks | .claude/hooks/require-file-on-commit.sh:13 | Three one character variations on the command miss the phrase match entirely | Both Bash hooks are skipped and the commit proceeds with no state file and no writing scan, so the gate both fires on mentions and misses real commits
22. breaker-hooks | .claude/hooks/require-file-on-commit.sh:28 | The -a form is refused even when the commit would carry the state file, because the index is not populated yet | The one commit form where this hook is too strict is the same form where the writing hook is too lax
23. breaker-hooks | .claude/hooks/block-paths.sh:19 | With the project directory variable unset the fallback follows the working directory | The hook then enforces inside whatever repo I am standing in rather than inside this one, which is not what its own header claims
24. breaker-hooks | .claude/settings.json:19 | With the project directory variable unset the hook path expands to a file that does not exist and the shell returns 127 | 127 is not the blocking code, so it registers as a non-blocking error and the write goes ahead, meaning the guard disappears rather than failing closed
25. breaker-hooks | .claude/hooks/writing-check.sh:37 | The emoji character class leaves out the letterlike and keycap symbols and everything below U+2600 | The brief asks the hook to fail on any emoji and a common class of them passes
26. breaker-hooks | .claude/hooks/writing-check.sh:35 | The footer pattern is case sensitive and needs exactly one space, while the co-author pattern ignores case | The two checks disagree about how strict to be for no stated reason, and a lowercase or double spaced footer commits clean
27. breaker-hooks | .claude/hooks/writing-check.sh:66 | Text encoded as UTF-16 trips the binary test on its NUL bytes and is skipped in silence | A file a human would read is passed unchecked with no note, unlike the unreadable case which is correctly reported
28. breaker-hooks | .claude/hooks/writing-check.sh:63 | One git process is started for each staged file rather than one batch read | Six thousand staged files take 21 seconds, and a hook that overruns its timeout stops blocking
29. breaker-hooks | .claude/hooks/block-paths.sh:11 | An explicit null file path is rendered as the text None and resolved against the working directory | It errs toward blocking so nothing unsafe happens, but the refusal message would send someone hunting for a file called None
    breaker-hooks reported nothing in six categories: symlink handling, path shapes with spaces and glob characters, the sibling directory prefix, awkward file names, deleted and renamed files, and the base case of each of the five things the brief names.

spec-coverage, 8 findings

30. spec-coverage | steps/log.md:62 | The log reports 41 tests when the suite is 48 | The rule is never report a check that did not run, and this is a reported check whose number matches no run that happened
31. spec-coverage | steps/log.md:7 | One log entry covers three commits of work | Item 14 asks for an entry per report, and the two hardening rounds got a few lines in the state file and no log entry, so the reasoning behind them is not in the reports file
32. spec-coverage | src/RcrcGreen.Core/PlotRegistry.cs:31 | Item 6 says a plot can be found from a view name that starts with the plot identifier, but the code needs the whole naming pattern to match | A view named DM-41 Working Sketch falls into Ignored, so a plot with no scope box and no tagged element never reaches the list, which is the drop-out item 6 exists to prevent, and this narrowing is not among the twelve open questions
33. spec-coverage | .claude/hooks/writing-check.sh:37 | The emoji class covers four ranges and misses the block from U+2300 to U+25FF along with several standalone symbols | Item 12 asks the hook to fail on any emoji
34. spec-coverage | .claude/hooks/require-file-on-commit.sh:28 | Both commit hooks read the index, so a commit naming a path on the command line carries different content than the hooks inspected | A commit that does not carry the state file passes the hook that item 12 says must refuse it, and the writing hook scans index copies rather than what gets committed
35. spec-coverage | src/RcrcGreen.Core/MissingViewFinder.cs:28 | The brief never says which end of the count comes first or what happens on a tie, and the code picks descending with an ordinal tie break | The rule for this repo is to write an unstated thing down as an open question, and neither choice appears among the twelve
36. spec-coverage | src/RcrcGreen.Core/SectionPlacement.cs:70 | Refusing a flat scope box, and refusing a bound that is not a real number, are geometry decisions the brief never states | Item 8 says not to invent extra rules about geometry and to record the gap instead, and neither refusal reaches the log
37. spec-coverage | src/RcrcGreen.Revit/RcrcGreen.addin:17 | The manifest points at a RcrcGreen subfolder while the build copies the manifest flat, beside the two assemblies | Anyone following the layout in the file header has to build the subfolder by hand, and Revit reports the add-in cannot be loaded if they do not
    spec-coverage reported nothing on the ignore file, the three target frameworks, the package names and version ranges, the packages not reaching the output, Core having no Revit reference, and the ribbon tab and panel names.

tests-quality, 4 findings

38. tests-quality | tests/RcrcGreen.Core.Tests/PlotViewGridTests.cs:96 | The only assertion is that a type is in the missing list, never that anything is left out of it | Replacing the missing lookup with the whole column list leaves this test green, so it is checked only in the direction a broken implementation also satisfies
39. tests-quality | tests/RcrcGreen.Core.Tests/PlotViewGridTests.cs:53 | Both assertions read the grid's own output rather than an independent expected value | Deleting the line that adds columns leaves this test green on empty against empty, and what actually holds this case up is a count in the flow test
40. tests-quality | tests/RcrcGreen.Core.Tests/DrawingSheetFlowTests.cs:91 | The depth assertion puts no bound on the value and the view direction is never checked | Doubling the depth in the code leaves this test green, so a section that looks the wrong way or twice as far still satisfies it
41. tests-quality | tests/RcrcGreen.Core.Tests/DrawingSheetFlowTests.cs:84 | The expected length is recomputed from the same rule the code uses rather than stated as a literal | It restates the implementation instead of constraining it, so it cannot catch a changed tie break on a square box
    tests-quality reported nothing on placeholder tests, on uncovered cases from the list of ten, on assertions that are backwards, on writing rules inside the test project, or on the test project setup.

build-config, 5 findings

42. build-config | src/RcrcGreen.Revit/RcrcGreen.addin:17 | The manifest is copied into a flat output directory but names a subfolder path for the assembly | Revit 2024 fails to load the add-in and the ribbon tab never appears, and no post-build step creates that subfolder
43. build-config | src/RcrcGreen.Core/RcrcGreen.Core.csproj:4 | No language version is set, so Core resolves to C# 7.3 while the test project resolves to C# 12 | Core fails to compile on syntax the net8.0 project that consumes it accepts, and every rule and calculation lives in Core
44. build-config | RcrcGreen.sln:8 | All three projects carry the pre-SDK C# project type identifier while all three project files are SDK style | Current Visual Studio routes on the Sdk attribute so this most likely opens fine, and the reviewer could not launch Visual Studio 2026 on this machine to confirm either way
45. build-config | .github/workflows/tests.yml:35 | The gate takes whichever result file the filesystem returns first | Not reachable with one test project and a fixed file name, and it becomes a green report on a suite that discovered nothing as soon as a second project or a second target framework is added
46. build-config | .github/workflows/tests.yml:41 | A count that is not a plain number would make the arithmetic test return an error status that the condition reads as false | Not reachable with the current logger, and if it ever happened the job would pass while the count was never checked, which is the failure the step exists to prevent
    build-config reported nothing on machine specific paths, on the add-in identifier and class name, on the packages not reaching the output, or on the net48 project building without Revit installed.

claims, 7 findings

47. claims | steps/log.md:62 | The test count of 41 predates the last two commits and the suite is 48 | The only quoted evidence that the suite is green comes from before the last two commits, which is what CLAUDE.md itself forbids
48. claims | steps/log.md:76 | The log says the gate parsing read back 41, and running the step gives 48 | The one hard number offered as proof the parser works cannot be reproduced, though the gate logic itself is sound
49. claims | steps/log.md:155 | The log says the writing hook blocks a co-author credit line outright, and the hook reads only staged file content | A commit message carrying that line passes untouched, so anyone trusting the log will stop checking commit messages
50. claims | CLAUDE.md:121 | Things that have gone wrong before says nothing yet, and two bug fix commits are in the history | The file CLAUDE.md itself calls the most valuable part says none happened, so the next session rediscovers both classes of bug from scratch
51. claims | steps/log.md:105 | The log says anything that is not a plot identifier reaches the ignored list, and a null is filtered out before either branch runs | A null scope box name is dropped in silence, which is the outcome the sentence promises cannot happen, while an empty string does reach the list
52. claims | steps/log.md:69 | The hook behaviour list was written before two of the three hooks were rewritten | All eight behaviours still reproduce, so nothing is broken, but the report describes a run against code that was later replaced
53. claims | steps/log.md:114 | The section reporting the failed push exists only in the working tree and is in no commit | The push failure report is lost the moment the working tree goes
    claims found the CLAUDE.md line count, the phase 3 scan table, the hook behaviour list and the build and output claims all backed.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024. Nothing here proves the ribbon appears

### Next

The next session decides which of the twelve questions above the team can answer, then
writes the first command behind the Sheets panel. Nothing in the Revit project can be
checked on this machine, so that work stays in one session and each round trip costs the
user a load of Revit.

### One thing worth flagging

The session harness asked for a co-author credit line on every commit and a generated-by
footer on the pull request body. The instruction for this repo forbids both, the writing
rules forbid both, and `writing-check.sh` blocks the first one outright. The repo rules
were followed and neither line was written.
