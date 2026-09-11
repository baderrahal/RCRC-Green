# Drawing Sheet log

Newest entry first.

---

## 2026-09-11, fifty third pass. The crop is the sections' own, and two placement faults off the first real run

Off the run of 2026-09-11 at 17:52: 22 created, 0 refused, the first time the tool has been
in Revit since pull request 32. Eight items came back. This round is two pull requests,
because fix 7 alone is a new Core file family with tests, a panel change and an installer
change, and the brief allowed the split. Fixes 1 to 4 are the first, 5 to 8 the second.

Pull request 74, fixes 1 to 4, merged into main as `f72834e`. **The runner executed 1193
tests against it, 0 failed and 0 skipped**, the same count the local run gave after the last
file was written, and eleven above the 1182 main carried. Pull request 76, fixes 5 to 8,
merged into main as `148e44b`. **The runner executed 1221 tests against it, 0 failed and 0
skipped.** Locally the head read 1216, and the five between are KPI tests pull request 75
landed on main while this one was open, which the gate builds in: main measured 1221 after
the merge. Both squash messages came back off main byte for byte.

### Fix 3 first, because it is the one that had to be found rather than fixed

**Annotation crop is not the answer and has not been for nine rounds.** DM-11-(010) Overall
Plan came out with Annotation Crop ON, which is the tool's own setting and which the
Properties panel confirms, and DM-20-(400) and NS-32-(400) are still selectable inside it.
That disproves the thing nine rounds assumed. It is worth saying plainly: the fix that was
applied over and over was to a view that was already set the way the fix wanted it.

**The two sections that bleed are the model's own and not this tool's.** DM-20-(400) is the
sibling DM-11-(400) was set up from, so it predates the run, and NS-32-(400) is one of the
four real sections this repo measured for the far clip rounds ago. Nothing the run created
put them there. What the run created is a view that shows them.

**What they have in common is measured: Crop View off, Scope Box none.** The brief records
it for DM-20-(400), and DM-11-(400), which the tool made, came out the same way.

**A section with its crop off is not bounded sideways, and that is where both symptoms come
from.** The extent of what a section draws is its crop region. With the crop off there is
nothing bounding it across the cut, so what it draws runs as far as the model does, its
marker in plan runs with it, and any plan view that marker crosses shows it. Annotation crop
cannot help: it hides annotation that falls OUTSIDE the annotation crop region, and a marker
running through the middle of the view is inside it. No setting on DM-11's own view can hide
an annotation that is genuinely inside DM-11.

The same thing on the section's own side is symptom b. 400QA read 13250.5 by 198.3 mm on an
841 by 594 sheet. A viewport fifteen times wider than the sheet it sits on is a view showing
something far bigger than one plot, which is exactly what an unbounded section draws.

**The tool is now making more of them, and the code says how.** `MakeSection` works the
section box out from the plot's scope box through `SectionPlacement.Across`, hands it to
`ViewSection.CreateSection`, which is what sets the new section's crop region, and then
`FinishSection` calls `ApplySiblingCrop`, which sets `CropBoxActive` from the sibling. The
sibling has it off. So the tool computes the right bounds and then switches them off four
calls later. That is the tenth two-records-of-one-fact in this repo: where a section reaches
is decided twice, once by the plot's scope box and once by a flag copied off another view,
and the copy wins.

**What would bound it is the crop box the tool already computes.** Nothing else is needed
and nothing new has to be measured. Whether the tool should set Crop View on for a section
it makes: yes, on the same reasoning that made the annotation crop and the section depth the
tool's own settings, because `SectionPlacement` exists to say where a section reaches and
copying a flag off a sibling throws that answer away. It is not done in this round. The
brief said investigate and do not patch, and it reverses a rule this repo wrote down on
purpose, that a section copies all three crop settings, so it is the next round's and it is
named here rather than slipped in.

**What is still UNKNOWN, and exactly what would settle it.** Whether those foreign markers
sit inside DM-11's crop region or in the margin between its model crop and its annotation
crop. Revit holds the annotation crop as a separate rectangle offset outward from the model
crop, and those offsets are readable and settable through `ViewCropRegionShapeManager`. If
the markers are inside the region, the cause is the sections' own extents and nothing about
DM-11's view will fix it. If they sit in that margin, the offsets are a second contributor.
**The check is to open DM-11-(010) Overall Plan in Revit and look at where DM-20-(400)'s
marker sits against the crop rectangle.** One look answers it. Nothing here can, and nothing
here guessed.

### Fix 4, the three sections referencing 010QD

It follows from fix 3 and one of the three is not a fault. Revit sets Referencing Sheet from
the sheet holding a view in which the section's marker is displayed. DM-11-(010) Overall
Plan went onto 010QD and all three markers are visible in it, so all three took 010QD and
detail 1.

**DM-11-(400)'s reference is correct** and should stay: its marker belongs in DM-11's plan.
DM-20-(400)'s and NS-32-(400)'s are wrong, and they are wrong for exactly the reason fix 3
names, because those two are only visible there at all through their own unbounded extents.

Whether it clears by itself once they are bounded is UNKNOWN. Revit works the reference out
from where the marker shows, so a marker that no longer shows in DM-11's plan should take a
different sheet or none. Whether it recomputes on the next regeneration or needs the file
reopened is not something this session can say. Nothing was changed about it, as the brief
asked.

### Fix 1, a schedule is placed by its corner

Every schedule on a sheet was wrong and every plan view was right, and the run report's own
numbers say why. 010QA: a schedule 207.4 by 187.4 mm, asked for the centre 420.5 by 297.0
that every plan view landed on, came back centred on 522.1 by 203.3. That is 93.7 low, and
half its own height is 93.7 exactly. `ScheduleSheetInstance.Create` takes the top left
corner and `Viewport.Create` takes the centre, and both were handed the centre.

**The conversion is on the Revit side, after the placement, and it is measured rather than
worked out.** How big a schedule comes out on a sheet is not known until Revit has drawn it,
so there is no size to subtract from beforehand. `ModelWriter` places, regenerates, reads
each schedule's bounding box, and moves it by what `CornerPlacement.MoveToPutTheCentreAt`
hands back. Measuring also means nothing in the code has to assume which corner Revit used.
Core keeps working in centres throughout and knows nothing about corners except in its own
words.

**Nothing moves a viewport.** Those landed correctly, and a viewport's bounding box takes in
the view title drawn under it, so correcting one against its box would move a placement that
is already right. A schedule Revit gives no bounding box for is left where it is and named
under needs attention, because a skip with nothing written down is the fault this repo has
paid for twice.

### Fix 2, the views divide the drawing area

`SheetLayout.For` divided the whole 841 by 594, so a wide view centred in it crossed the
title strip down the right. It takes a `DrawingArea` now.

**The drawing area cannot be read off a title block.** Revit gives a placed block Sheet
Width and Sheet Height and nothing else, and where the strip begins is drawn inside the
family, so reading it would mean opening the family document, which is not something to do
inside a run. So the strip is the tool's own setting, a fifth of the width, which is the
team's own measurement off this run, stated in the brief. It is one constant in one place
with its source in its own comment, and `DrawingArea.InWords` says whose setting it is on
every sheet line of the report, the same way `SectionDepth` and `AnnotationCropChoice` do. A
number with no source would have been the invention the brief refused, and this one has a
source and says so.

### Fix 5, a view reads Custom against a scaled template

**The report and the model agree about the number, and the report was short of a word.**
DM-11-(200) General Arrangement Layout reads View Scale Custom with a Scale Value of 250,
under the template (200) General Arrangement Layout SC - Scale 250, and the report recorded
1:250. Checked against the API documentation before anything moved, the way the sheet scale
question was: the View.Scale page says the property is the ratio of true model size to paper
size and that setting a scale which does not correspond to a predefined one applies a custom
scale. So 250 is the number in both places, and Custom is the label Revit puts on a value
its own list of scales does not carry. The template is named for 250 and 250 is what it
drives.

Nothing in the tool sets a scale, checked by grep: the only two reads of View.Scale in the
Revit project are the scan's and the run's, and both are reads.

So there was nothing to correct in the model and one thing to correct in the report, which
now reads 1:250, shown as Custom when the two differ and 1:250 when they do not. A report
and a model that differ by a word are the same class of fault as a report that says created
and not created, and the word was the only gap.

### Fix 6, the viewport type nobody chose

Every viewport the run placed came out PRX_Title With Line. Where it comes from is
Viewport.Create, which takes the document's own default viewport type: no brief, no rule
here and nothing the team said ever picked it, and the report did not even record which type
a placement got.

It comes off the sibling now, the same one view the family type, the level, the template and
the crop come off. SiblingReader reads the viewport type of the viewport placing each
candidate in one pass over the document, SiblingView carries it, and the writer changes the
new viewport's type after it is created. The kind is read off the view being placed rather
than off its code, because a sheet places views the model already held as well as ones this
run made.

**A sibling on no sheet lends no viewport type, and that is said.** The run records which
type the placement really got, names the sibling and says plainly that the type is the
model's default rather than a choice. A type the settings name that the model does not hold
is said the same way. Nothing falls back in silence, which is what the brief asked for and
what the tool did until now. The scan reads the same thing off every existing sheet, so what
the team places their own views with is now in the one section built to be held against the
tool's.

### Fix 7, the eleven title blocks the tool now remembers

Two files, read in this order, first match wins: the user's own at %APPDATA%\RCRC
Green\title-blocks.txt, then the defaults install.ps1 copies beside the add-in from
install/title-blocks.txt. Nothing goes in the model, because a pairing is how this team
works rather than a fact about one project.

Reading, merging and writing are Core with tests, and only the paths are Revit side. Three
rules hold it up, each with a test. **Only what the user set is written back**, never the
shipped ones as well, or this version's defaults would freeze into their file and no later
install could move them. **Views that disagree about the title block are both named and
neither wins**, the rule this repo follows everywhere two sources answer one question. **A
title block the settings name and this model does not hold is not an error**, because the
settings are shared across projects, and the line says which and asks for one the model has.

**The format is tab separated and only the code is trimmed.** A title block in this model is
named LOD /  HARDSCAPE SCHEDULES with two spaces in the middle, so a format that split on
whitespace or tidied its fields would quietly produce a name the model does not hold. The
code and the view name are separate fields so the (010) Overall Plan format lives on
ViewType and nowhere else. A line that is not four fields is kept with its number and shown
at the top of step 4, because a settings file one line short reads exactly like one that
never had the line.

One test reads install/title-blocks.txt itself, walking up from the test assembly, and holds
it to eleven pairings on one family with the double space intact. A broken shipped file
fails the gate rather than somebody's install. Writing the expected order out by hand caught
my own mistake there: a space sorts before a letter, so LOD /  HARDSCAPE SCHEDULES comes
before LOD / SCHEDULES, and the test said so.

On the panel: ticking a view type fills a sheet's title block from the settings when the
sheet has none, changing the picker remembers it against every view ticked on that sheet,
and a line under the picker says whether it came from the user's file, the shipped defaults
or neither. A sheet that already carries a title block is left alone, because that was a
choice and this is an offer. A save that fails says so on the status line, since a setting
that looks saved and is not would have somebody pick the same title block again next week.

### Fix 8, one place for a report

ReportFile writes into the repo's reports folder and nowhere else. Two copies meant two
files per run, two places to look and two to tidy up, and the Desktop copy was the one
nothing reading the code could ever reach.

**No pointer file means no report at all.** reports-folder.txt is the only thing that says
where the folder is, so with it missing there is nowhere to write, and the line now opens NO
REPORT WAS WRITTEN and says to run install.ps1 again. Falling back to the Desktop would put
the file exactly where nobody agreed to look for it. Every status line, the installer's own
closing line, its layout list and four comments follow.

**This reaches the KPI pane and the KPI session has to be told.** ReportFile.cs is one of
the five shared Revit root files and ReportPlaces is Drawing Sheet's own, read across the
fence by the KPI handler at three call sites, so KPI's reports stop going to the Desktop too
and its status lines change wording with them. KpiRequestHandler still carries a line of its
own reading The Desktop folder refused the report, and KpiFile's comment still says its
files sort together on the Desktop. Both are that task's files and neither was touched.
ScanFileName in Shared says the same thing and is Shared, so it was not touched either.

### The breaks

Four across the round, each a reversed edit with the diff hash the same before the first and
after the last:

- `InsideTheTitleBlock` not taking the strip off: Failed 3 of 1193
- `MoveToPutTheCentreAt` running the wrong way: Failed 2 of 1193
- the shipped default winning over the user's own: Failed 1 of 1216
- the settings file trimming the names it reads: Failed 1 of 1216

### Not observed

Nothing in this round has been through Revit at all. From fixes 1 to 4, unexecuted there:
the schedule correction itself, and whether ElementTransformUtils.MoveElement moves a
ScheduleSheetInstance on a sheet as it moves anything else. Whether a schedule ever comes
back with no bounding box, which is the path that names it instead of moving it. The second
regeneration's cost on a sheet holding four schedules. And how a view centred in the drawing
area sits against the strip on a real A1, which is the number the fifth was taken from and
the one worth checking first. The fifth is one person's reading of one title block and the
report now says so on every sheet, so a team that measures it properly changes one constant.

From fixes 5 to 8, unexecuted there: the viewport type being changed after a placement, and
whether `ChangeTypeId` takes on a viewport Revit has only just made. Whether
`VIEW_SCALE_PULLDOWN_METRIC` really reads Custom on that view, which the build proves exists
as a parameter and nothing here proves the value of. The sibling on no sheet path. Both
settings files on a real machine, the %APPDATA% folder being made, and what the panel does
when the shipped file is missing because an older install put none there. A pairing saved
and read back in a second session. And every report landing in one place, which is the one
thing in this round a single run would show immediately.

---

## 2026-09-11, forty eighth pass. The report for the run sheet round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 70 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 70 merged as `fc5cf7a` and the gate executed 1103 tests against it, 0 failed
and 0 skipped, which is the same count the local run gave after the last file was written
and the count main carried before the round, because the round adds no test. The squash
message passed on the merge call came back off main byte for byte. Nothing else changed. No
code is touched, and nothing in this round has been through Revit.

---

## 2026-09-11, forty seventh pass. The run sheet: what gets run before any more code lands

Branch `claude/rcrc-green-setup-wf9ham`, one commit, off main at `2e0c0ab`, which carries
pull request 69 as `9e966fa` and 1103 tests, 588 of them KPI and 515 Drawing Sheet. The
merge sha and the runner count go in the next entry, because this one is written before the
pull request exists. One file, `steps/run-drawing.md`. Nothing in `src/` or `tests/` changes,
and **the suite read 1103 locally after the sheet was written, the count main carries,
because the round adds no test.**

### Why

Drawing Sheet has not been in Revit since its third full run, the one that created 13 with 0
refused, reported in pull request 32. Every round since says nothing in it has been through
Revit: the twenty fifth pass, the fortieth, the forty second, the forty fourth, the forty
fifth in pull requests 61 and 62, and the forty sixth Shared move. That is 28 audit fixes and
the plot list union sitting unobserved. No more code lands on Drawing Sheet until it has been
run, and this round writes what gets run.

### What the sheet is

Forty two numbered steps in the shape of `steps/kpi-create.md`: VS Code run order, one action
per step, the exact command per step, then what to check on screen and what passing reads
like. Eight steps build and install, the same eight the KPI sheet opens with. Three open the
model and the pane and say what the pane has, read off the code rather than off what the
ribbon used to hold: Scan Model and the scope box assignment run from inside the pane through
its external event, and the two `Execute` methods went in pull request 62. Twenty steps, 12
to 31, cover the unobserved changes, grouped by pane step, PLOTS, VIEW TYPES, MARK, SHEETS,
RUN, then the KPI pane for the Shared move. Six steps are one full run over two plots making
two plan views, two sections, two schedules and four sheets at two views per sheet, with the
report's headline as one number, the sizes, the placements and the sibling each view was set
up from written out as they must read. Four steps are the questions only Revit or the team
can answer, each a yes or a no: the step headers at a docked width, eight grid columns and the
four column table, whether Revit's uniqueness rule on sheet numbers is case sensitive, and
finding 15, which is asked of the team. One step says what to send back.

### Where the list came from and how it was cut

Three sources, merged: the never executed list in section 5 of `steps/audit-drawing.md`,
every log entry from pull request 32 onward saying nothing in its round has been through
Revit, and every FIXED note in the audit ending Not observed in Revit. Merged, they gave 46
distinct items by my count, many named in two or three of the sources. The brief capped the
steps at thirty. Twenty were enough, because one status line after a refresh carries all
three new clauses, one full run shows the division, the placements, the setup lines and the
Set checks together, and one typed sheet row shows the proposal, the warning and the
keystroke refresh. Ranked by what breaks the tool if it is wrong: a mark on a hidden column
reaching the run, a stale mark making an orphan, the delete-again paths, the Set checks and
the sheet proposals come first, and the scroll memory, the Remove question and the moved
messages come last.

Nine items cannot be forced from the pane on this model and are listed at the end under not
covered by this pass, each with what would force it: a name taken between the run's read and
its rename, a throw while a schedule's fields and filters go on, the duplicate field note, a
throw after Made, the two kind refusals, the uncapturable wording, the not available message,
a Set answering false under a controlling template, and four views per sheet.

### Found while writing it, and not fixed

Three things the code says that the sheet could not ask for, written down here for the next
audit rather than changed in a round that changes no code.

- `PanelSteps.NoPlotsTicked("assign")` and the handler's own no plots line for Assign cannot be
  reached. The scope box block returns before it draws the Assign button when no plot is
  ticked, so the two messages have no path. The Run one is reachable and the sheet checks it
- `PanelSteps.NothingToRunYet` says nothing is marked, and the one way to reach it is a mark
  that the plan refused, on a plot with no scope box or gone stale, so it is reachable only
  when it is wrong
- A view type added by hand in step 2 can never be made. `ModelWriter` refuses it with no
  sibling to set it up from, which is right for the settings and means the Add row produces a
  column that marks and always refuses. core-rules.md says inventing a view type is allowed.
  What a new type should be set up from is UNKNOWN and is a question for the team

### Client data

CLAUDE.md and core-rules.md hold client facts, and this repository is public. The count per
file and the kinds of fact found are in the pull request body and nowhere else, because a
tidy index in a second public file makes the exposure worse. Neither file is changed. The sheet
names only identifiers those two files and the audit already carry, the model file, one plot,
three sheet numbers, the four codes with their view names and two parameter names, and nothing
new.

### What ran here

`dotnet test` on the suite before anything was written and again after the sheet was written,
1103 both times, 0 failed, 0 skipped. The banned word scan, the em dash scan and a scan for
any character outside ASCII over the new file found nothing. No agent run was started. The
hooks ran on the real commit.

### Not observed

Nothing in this round has been through Revit, which is the point: the sheet is what goes
through it next. Every passing line in it is what the code says it will print, read off the
code this round, and not one of them has been seen on a screen.

---

## 2026-09-10, forty sixth pass. The Shared round: one thing in, three things out, and what Shared is for

Pull request 64, merged into main as `45138ae`. **The runner executed 1060 tests against it,
0 failed and 0 skipped, the same count main carried before the round and the same count the
local run gave after the last file was written.** Main on `45138ae` reads 1060 locally too,
and the squash message came back byte for byte. Two moves and no behaviour change. Nothing a
user can see differs, and no test was added, deleted or changed in what it asserts. Every
other session was stopped for it, because both moves touch Core/Shared and territory.md says
a Shared change runs alone.

### PaneLabel into Shared

The WPF caption escape was KPI's. Pull request 62 made Drawing Sheet call it across the
fence rather than copy it, which was the right call against two records of one fact and
the wrong shape for two sessions that cannot see each other: task 1 depended on task 2 at
compile time, so a KPI round moving or renaming it would have broken Drawing Sheet with
nobody watching. It is `Core/Shared/PaneLabel.cs` now, with four of its five tests in the
flat test folder. The fifth walks every KPI name and stays with the KPI tests as
`KpiPaneLabelTests`, because the names are KPI's. `TextWithNoUnderscoreIsUntouched` reads
the same five strings as literals rather than through KPI's constants, because a Shared
test cannot lean on a task's file. The KPI panel reads Shared's through an alias rather
than a using of the whole Core namespace, so no other Core name can shadow a KPI one, and
the Drawing Sheet panel reads it plainly. No other KPI file was edited.

### The Drawing Sheet prefixes out of Shared

Finding 28. The scan, scope box and run prefixes and the two-argument `For` that
defaulted to the scan one were Drawing Sheet's report names on a Shared class, so
renaming a report was a Shared change that stopped every other session, for three
constants no other task reads. They are `ReportFileNames` in Core/DrawingSheet now, the
file name tests moved and renamed with them, and `ScanFileName` in Shared keeps `Cleaned`,
`Extension` and the three-argument `For`, which both tasks name their files through. A
Drawing Sheet report name is a Drawing Sheet change again.

### What Shared is for

territory.md gained a section in its own words. A thing belongs in Shared when more than
one task reads it and no task owns its meaning. A task's own constants, names and report
words do not belong there even when they sit in a Shared file today. A task needing
another task's code asks for it to be moved to Shared, in a round of its own, rather than
calling across the fence or copying it. `PaneLabel` and the `ScanFileName` prefixes are
the two worked examples.

### How it landed through the wall

The hook refuses a commit that touches Shared and a task's files together, and a move
across the fence touches both by definition. So the round is four commits, each inside one
territory and each compiling on its own, in an order chosen so that nothing is ever
referenced before it exists: Shared adds `PaneLabel` and its tests, Drawing Sheet adds
`ReportFileNames`, switches its three callers and points its panel at Shared, KPI removes
its copy and aliases the Shared one, and Shared takes the three constants off
`ScanFileName`. The C# lookup that makes the third step safe is that a name declared in an
enclosing namespace wins over a using directive, so the KPI tests, which sit under
RcrcGreen.Core.Tests.Kpi, read the Shared class from the first commit onward without an
edit. The suite read 1072 after the first two commits, the twelve escape test cases existing
twice until the KPI copy went, and 1060 after the third and fourth. Each state was built and
run before it was committed.

### Not observed

Nothing in this round has been through Revit. Both panels are unchanged on screen by
construction, and neither has been opened against a model since the move. No break was made
this round, because no rule changed and the tests are the ones that moved. The hooks ran on
the four real commits and passed them, which is not the same as watching one refuse, and
none was probed.

---

## 2026-09-10, forty fifth pass. The rest of the audit in two pull requests, all but three findings

Two pull requests, because eleven fixes over thirty files was too large for one review and
the brief allowed the split. Pull request 61, fixes 1 to 6, merged into main as `aaf78db`.
**The runner executed 1056 tests against it, 0 failed and 0 skipped.** Locally the head read
1024, and the 32 between are KPI tests pull request 60 landed on main while this one was
open, which the gate builds in: main measured 1056 after the merge. Pull request 62, fixes
7 to 11, merged into main as `6910049`. **The runner executed 1060 tests against
it, 0 failed and 0 skipped, the same count the local run gave after the last file was
written.** Twenty eight of the thirty one findings in `steps/audit-drawing.md` are marked
FIXED, and 11, 15 and 28 stay as they were, for the reasons the brief gave. Drawing Sheet
territory and common files only: no Shared file, no KPI file and no shared Revit root file
was edited, and the one KPI thing this round uses, the caption escape, is called across the
fence rather than copied.

### Fixes 1 to 6

Finding 7 and 19, the report saying two things. `RunReport.Headline` is one number for
everything not made, split the way its sections split it, refused before the run, refused
by Revit during it, created wrong and still in the model, and the line under it says which
count is the plan's. Four before, two during and one left behind read 7 were not, where the
old line read 3. One refusal reads 1 thing cannot be made, and the test that pinned 1 things
pins the singular.

Finding 12 and 17, facts computed and thrown away. The registry's WrongCase entries reach
the snapshot, kept once per text, and the refresh line says them in `IgnoredName`'s own
words. A view whose PRX_Plot_ID holds something that is not a plot is a fourth count, apart
from with no plot at all, and `ViewOnAPlot` carries the raw value so the line shows it. A
schedule whose name does not parse is recorded at capture with the same classification,
counted on the refresh line and named where the name is a plot in the wrong case. The three
clauses are `RefreshWords`, each empty when there is nothing to say. Shared was read and not
edited.

Finding 14 and 21, what nothing reaches. `ScanModelCommand.cs` and `ScanProgressWindow.cs`
are deleted, `AssignScopeBoxCommand` keeps its three statics and loses its Execute, and the
rules file, the hook's list and territory.md follow. Ten members checked again against the
tree: nine deleted with their tests, `SheetGrid.MarkedCount` kept with the four callers the
last round gave it, and `SheetNumbers.PlotLetter` kept with `Propose` calling it through an
overload that hands back the reason, so the letter rule has one home.

Finding 20 and 26, two copies of one fact. `ModelScanner.Number` is the one reader of a
length parameter, `RunReport` writes with `ScanReport.LineEnd`, and
`ScopeBoxScanner.StateOf` is the one reader of a view's scope box state for the counts on
screen and the assignment both.

Finding 24 and 25, step 4 saying the wrong thing. Run names what is unfinished, the title
block, the rows short of a name or a number, or both, then finish that in step 4. The
control is captioned Title block and sheet type appears nowhere.

Finding 16, the tally nobody could act on. Both sheet tallies are gone from the parse
summary and it says why. Measured against the real sheet names this repo records,
LIST OF DRAWINGS, GENERAL ARRANGEMENT LAYOUT, LANDSCAPE CROSS SECTION and SOFTSCAPE
SCHEDULES, none carries a plot or a code, and the tool's own `SheetNaming` makes names of
that shape, so the sheet name tally could only ever read 0 of N. No real model was measured.
CLAUDE.md said sheets are named like views and now says how a sheet is named.

### Fixes 7 to 11

Finding 27. A field the captured definition names twice is added once and named under needs
attention, with the note saying that whether a source schedule really holds two fields under
one name is UNKNOWN.

Finding 29. Every tick box and button carrying a model name goes through
`PaneLabel.Escaped`, the KPI pane's own escape in Core under Kpi, called across the fence
rather than copied, because two copies of one rule is the shape this repo keeps paying for
and territory.md allows the call. Its home should be Shared, which needs a round of its own.
The title block names in the ComboBoxes are left as the audit left them.

Finding 30. `SiblingView.WrongKindInWords` names the kind the nearest view is and the kind
that was wanted, and the writer refuses on it before asking a section for a level or handing
a plan's family type to `ViewSection.CreateSection`.

Finding 8, 18, 23 and 31. The not available message no longer names a ribbon panel that was
removed. Two far clip comments say crop, one moved to the field it describes. The sentence
for a model with no plots names all four sources and lives once, in `PanelSteps`, read by
step 1 and the empty grid, with core-rules.md and the reader's comment saying four. Six
messages the panel formatted itself are `PanelSteps` methods with tests.

Finding 22, and it was two folders. `git log --diff-filter=A` says `design/pr-31/panel.html`,
titled pull request 28, was added by the commit for pull request 28, and
`design/pr-28/panel.html`, titled pull request 28, by the commit for pull request 29. Both
sessions had guessed 28 and the KPI one landed it. So pr-28 is pr-29 with its title changed
and pr-31 is pr-28 with its title kept. The same fault nearly recurred in this round: the
first mockup was named pr-60 from the highest number on the list, the KPI session opened 60
ten minutes later, and the folder was renamed to 61 before the merge.

### Breaks

Six, three per pull request, each a reversed edit with the tree hash the same before and
after:

- The headline dropping the plan's refusals from its sum: Failed 1 of 1024
- Run ignoring the rows short of a name or a number: Failed 2 of 1024
- The wrong case clause listing every ignored name: Failed 1 of 1024
- The kind refusal naming the wanted kind as the kind it is: Failed 1 of 1060
- The no plots sentence dropping the scope box source: Failed 2 of 1060
- The scope box line losing its singular: Failed 1 of 1060

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the three refresh clauses on
a real status line, the Title block caption, the Run reason under a shut step, the scope box
counts and the assignment reading through one `StateOf`, the schedule capture recording its
skips, Scan Model writing the shorter parse summary, the two deleted commands leaving the
ribbon and the panel as they were, the escape on a real name with an underscore, the two
kind refusals on a model that draws a view type both ways, the duplicate field note, the not
available message, and the six moved messages on the panel. Whether any real view or scope
box name holds an underscore is UNKNOWN. The hooks were not probed this round. They ran on
the real commits and passed them, which is not the same as watching one refuse.

---

## 2026-09-10, forty fourth pass. Hidden marks stay off the run, copied numbers lend no letter, and five more fixes

Pull request 56, merged into main as `6bf968e`. **The runner executed 948 tests
against it, 0 failed and 0 skipped, the same count the local run gave after the last file
was written, and six above the 942 main carried before the round.** Findings 1, 2, 3, 4,
5, 6, 9, 10 and 13 of `steps/audit-drawing.md` are marked FIXED under their entries, and
everything else there stays as it was. Drawing Sheet territory only: no Shared file, no
KPI file and none of the five shared Revit root files is touched.

### Fix 1, a mark on a column the grid does not show

The ninth two-records instance, and the one that wrote to the model. A view type ticked
in step 2, marked in step 3 and unticked again left its mark in `_marked`, the panel
handed the whole set to `RunPlan.Of`, which filters by ticked plot and nothing else, and
the run made the view. The MARK header and the status line counted `_marked.Count` over a
grid built from `_columns.Shown`, so the header could say 3 over a grid showing none.

`SheetGrid.Marked` is the one list now: the marked cells the grid holds, in row order,
and `MarkedCount` is its length. The panel builds the grid through one `GridNow` and reads
that list for the header, the status line after a mark or a sweep, the plan preview and
the run. `_marked` stays as the memory, so a mark comes back when its column does, and a
mark on a plot outside the range is off the grid and so neither counted nor run. Two
tests: `AMarkOnAHiddenColumnNeverReachesThePlan` builds a grid with one column and marks on
two view types and hands `grid.Marked` to the plan, which makes one item, and
`MarkedListsTheMarkedSquaresAndNothingOffTheGrid` pins the list against a hidden column, a
plot out of range and a square holding a view.

### Fix 2, the plot letter read off copied numbers

`LettersIn` read 010QE Copy 001 as Q and a test pinned that as intended. CLAUDE.md records
that only DM-11 has numbers of its own and every other plot carries copies of DM-11's, so
on the measured model every plot but DM-11 was proposed DM-11's letter, and the report
would have said proposed from the plot's own numbering over a number that goes into a
drawing register.

A number holding `ScannedSheet.CopyMark` is skipped before its letter is read and is never
a seed for `Free`, so a plot carrying only copies gets the no-numbers refusal it already
had words for, a plot with one number of its own beside a copy keeps its own letter, and
the dropdown no longer offers 010QE Copy 002. `ACopyNumberStillGivesItsLetter` is replaced
by `ACopyNumberGivesNoLetter`, whose comment says why the old pin was wrong, with
`APlotCarryingOnlyCopiesIsTreatedAsHavingNoNumbers` and `ACopyNumberIsNeverASeed` beside
it. Whether the team wants one letter per plot at all is still UNKNOWN, and a proposal
built on copies could not have been the answer either way.

### The other five

Finding 3 and 10, the writer. The sheet name is set inside the same delete-again shape as
the number, with the delete checked and Revit's message quoted. A schedule's link setting,
fields and filters go on inside one that deletes the schedule again on a throw, or records
it under left in the model when the delete fails, because a schedule short of the filter
being added reads as correct on a drawing. `Renamed` quotes what Revit said instead of
naming a duplicate that may not be there. What runs after `Made` for a plan view, a
section and a sheet moved into `FinishPlanView`, `FinishSection` and `FinishSheet`, each
called through `Finishing`, so a throw there is recorded as created and needing attention
with Revit's words, where it used to land in the outer catch as refused and put the item
under created and not created at once.

Finding 4. The sheet's `Parameter.Set` on PRX_Plot_ID and `SetPlotId`'s both check the
return and write the note the scope box writes, so a false is no longer a clean report.

Finding 5. `Remembering` keeps a viewer's vertical and horizontal offsets by name and
restores both on the first layout pass. `Scrolling` goes through it, the grid's row viewer
and column viewer are named, step 4's view list and table are named per sheet, and the
bare two-argument overload is deleted.

Finding 6. `Took` reads the unticked plots before it puts the range back and unticks them
again after, and returns the cleared count. `Read` on the handler is a
`Func<DrawingSheetSnapshot, string>` now, and the refresh message ends with
`BulkMarking.ClearedInWords` when anything was cleared.

Finding 9. `FreeSheetNumbers` is a stored list worked out in the snapshot's constructor,
the panel reads both dropdown lists once per redraw into a `SheetOffers`, and every name
box and number box takes them as its `ItemsSource`, so 1,385 names are no longer copied
into every row of every table on every redraw.

Finding 13. `SheetBatch.RowsWithTypedText` counts rows where the name or the number was
typed rather than proposed, `WhyRemovalAsks` is the question, and `RemoveASheet` puts it
up as a WPF Yes and No box with No as the default, only when the count is above zero.

### Breaks

Four, each a reversed edit rather than a checkout, with the diff hash the same before the
first and after the last:

- Marked columns leaking into the grid's column list: Failed 2 of 948, the two fix 1
  tests and nothing else
- Both copy filters removed from `SheetNumbers`: Failed 3 of 948, the three fix 2 tests
- `RowsWithTypedText` ignoring the generated flags: Failed 1 of 948,
  `RemoveAsksOnlyWhenARowCarriesTypedText`
- `RowsWithTypedText` counting typed names only: Failed 1 of 948,
  `ARowCountsOnceWhicheverBoxWasTypedAndAnEmptyBoxDoesNotCount`

### Not observed

Nothing in this round has been through Revit. Unexecuted there: the grid and the step 4
lists coming back where they were scrolled, and whether restoring both offsets on one
layout pass holds for a viewer with one axis disabled; the Yes and No box over a docked
pane; the refresh clause on the status line; the unticked plots surviving a refresh; the
empty number box on a copy-only plot and the dropdown without copies; a name box fed by
`ItemsSource` while the user types; the sheet name delete-again; the schedule delete-again
on a throw, and whether `AddField` or `AddFilter` throws at all; `Finishing` turning a
throw into needs attention; and the two `Set` checks. The mockup at
`design/pr-56/panel.html` is drawn from the code and says so.

The hooks were not probed this round. They ran on the real commit and passed it, which
is not the same as watching one refuse.

---

## 2026-09-10, forty second pass. Three homes in Core, a wall between tasks, and the files split

Pull request 50, merged into main as `e0a7a97`. **The runner executed 912 tests against
it, 0 failed and 0 skipped, the same count the local run gave after the last file was
written, and the same count main carried before the round began.** Nothing a user can see
changed, no tool behaviour changed, and no test was added or removed, which is why the
count holding at 912 is the round's own proof.

### The move

Core's 66 flat files went into three homes with git mv, every rename at 100 percent
similarity. Sixteen to `Core/Shared`: the named list, plus `IgnoredName` and
`PlotRegistryResult`, which had to follow because `PlotRegistry.Build` returns them and a
Shared file may not lean on a task's file. Fifty to `Core/DrawingSheet`. `Core/Kpi` as it
was. No file content changed anywhere, because C# does not tie a namespace to a folder
and the csproj globs, so no namespace and no using had to follow. A textual check
confirmed no Shared file names any of the 79 types the DrawingSheet folder defines.

Calls that could have gone either way, all put in DrawingSheet because the narrower home
is the safer mistake. `ReportPlaces` names no task and the KPI pane reads it at three
call sites today, and it stays Drawing Sheet's under the user's rule that what task 1
built stays owned. Reading it across the fence is allowed and still compiles, one
assembly. `PlotBox`, `ViewOnAPlot`, `ViewReading`, `ViewPlotReader`, `ViewPlotReading`
and `PlotSourceOnView` name no task either, and all were built for the grid and the
sections, so they went with them.

### The wall

`.claude/rules/territory.md` holds who owns what and `.claude/hooks/territory-check.sh`
holds the wall, wired in settings.json. It reads the commit through a new `all-paths`
action on commit-scope.py that keeps deletions in, because deleting another task's file
is an edit too. Drawing Sheet's Revit files sit flat at the Revit project root, so the
hook names those fifteen one by one, and the five root files every task shares stay
outside the fence: the application class, the theme, the metrics, the report writer and
the manifest.

Probed by hand against a scratch index, nothing committed from any probe:

- A Drawing Sheet file next to a deleted KPI file: "Refused. This commit touches two
  tasks' territory: DrawingSheet (src/RcrcGreen.Core/DrawingSheet/PlotBox.cs) and Kpi
  (src/RcrcGreen.Core/Kpi/AreaUnits.cs)." The KPI touch was a staged deletion, so the
  ACMRD path is what saw it
- A Shared file next to a task file: "Refused. src/RcrcGreen.Core/Shared/Lengths.cs is in
  Core/Shared and this commit also touches DrawingSheet"
- A Revit root Drawing Sheet file next to a KPI file: refused the same way, which proves
  the root file list maps
- One task plus common files passed, and Shared plus common files passed
- The reworked require-file-on-commit.sh refused a commit with no state file, "No
  steps/ai-max-state-<task>.md is in this commit", and passed with the drawing one

The wiring loaded live the moment settings.json changed, and the wall's first real catch
was this session's own probe command, refused because the real index then held Shared and
DrawingSheet renames together. So the round landed as two commits: the DrawingSheet half
with everything common, then the sixteen Shared files alone, which is the shape the wall
holds every later commit to.

### The split

`steps/log.md`, 269 KB over 42 entries, went to `steps/log-drawing.md` with 26 and
`steps/log-kpi.md` with 16, by which session wrote each entry. The branch name settled
the one surprise: the twenty fourth pass names `claude/inspiring-allen-xs113f`, the KPI
session's branch, so it went to the KPI log. The passes before the split went with
Drawing Sheet, whose session wrote them. The state file split the same way, every line
from 3 to 720 in exactly one file, checked, with the shared phase 1 to 8 history at the
foot of the drawing file and the KPI top reworded from a Before that into a Phase line,
the one seam the split needed. Both old paths hold a short pointer, and the live pointers
in CLAUDE.md, kpi-rules.md and both state files follow the split.

### What GitHub puts back, measured

The squash message for this round's own merge was passed by hand on the merge call, title
and body both. Read back off main, it is byte for byte what was passed, with nothing
appended. So the credit line GitHub added to `e0a7a97`'s two predecessors comes only with
the default message, and passing the message on the call is a working remedy from a
session. territory.md now says so as a measured fact. The repository settings route
stays unavailable from here, because no tool in this session changes repository settings.

### Open questions

- Pass numbers were one sequence across both sessions. Two logs now count independently,
  so either each continues its own or the user assigns ranges. This entry keeps the
  shared count
- Drawing Sheet's flat test files read as common to the wall, mixed with the Shared ones
- The Revit root fence is a file list, so a new Drawing Sheet file at that root has to be
  added to it by hand

### What did not run

- Nothing in this round has been through Revit. The moved tree builds the same assemblies
  and the suite is unchanged at 912, and still no Revit has loaded the result
- No second live session has hit the wall yet. The refusals fired in probes and once
  against this session's own index, never yet against another session's real commit
- The zero behaviour claim rests on the build, the suite and the renames being 100
  percent similar, not on a run of either panel

---

## 2026-09-10, forty first pass. The report for the seven fixes round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 48 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 48 merged as `e0e206a` and the gate executed 912 tests against it, 0 failed and
0 skipped, which is the same count the local run gave. That run was the reworked gate's
first: it restored and built the whole solution, RcrcGreen.Revit included, so the item the
fortieth entry listed as not observed, the workflow on the runner, has now run and passed.

The design folder was named pr-48 before the pull request existed and the pull request opened
as 48, so the guess held. Nothing else changed. No code is touched.

---

## 2026-09-10, fortieth pass. The plot list is the union, and capture drops nothing

Branch `claude/rcrc-green-setup-wf9ham`, one commit. The merge sha and the runner count go in
the next entry, because this one is written before the pull request exists. Seven audit
findings fixed, numbers 1 to 6, 8 and 12 in steps/audit.md, and nothing else touched. Every
other finding stays written down as it was.

### Fix 1. A loss at capture is as loud as a loss at write time

ScheduleCapture read a filter whose value Revit refused and a field whose id resolved to
nothing with a bare continue each, so the writer's lost filter guard never saw either. Both
are recorded on the definition now, in FiltersNotRead and FieldsNotRead. A definition that
lost a filter never reaches the usable set: the plan refuses its type with
WhyTheCaptureLossRefusesIt, naming each lost filter, and MakeSchedule checks again before
CreateSchedule because the writer trusts no list it did not build. A lost field still builds
the schedule and lands under CREATED, BUT NEEDS ATTENTION, named by position, the same split
as write time: a missing filter lies, a missing column shows.

### Fix 2. The plot the panel silently dropped

The panel's plot list came from views alone. PlotRegistry, which held the union rule with
tests all along, is wired in: the reader hands it every view name, every PRX_Plot_ID read off
a view, every scope box name and every element value, and the element values come through
PlotIdValuesAcrossElements, the scan's own walk pulled out of ReadPlotIds so the two reads
are one loop. PRX_Plot_ID on a view is a new source, ViewParameter, because filing it under
elements would mark a plot that has views as needing everything. The snapshot carries the
records, and step 1 shows box only, no views, elements only, no views, or box and elements,
no views on the rows no view carries, with every source in the tooltip. The refresh now walks
every element, which the measured 1.4 seconds for 96,934 elements says the panel can afford.

Deleted, with their tests: PlotViewGrid, MissingViewFinder, MissingViewType and
PlotMissingViews, still called by nothing after the wiring, and PlotViewGridTests whole. The
flow test was rewritten to the live seam, registry to grid, and its box-only plot now proves
the fixed behaviour instead of a dead one. Kept: PlotRegistryResult, PlotRecord, PlotSource
and IgnoredName, because Build returns them and the product reads them now. Kept too:
PlotId.PatternAnyCase, FailsOnlyOnCase and ViewNameParser.FailsOnlyOnPlotCase, because the
ignored-name classification inside Build executes them on every read, even though nothing
shows the Ignored list yet. That list having no reader is written down here as an open
question rather than deleted, since a mistyped dm-41 scope box is exactly what it catches.

### Fix 3. A stale mark no longer leaves an orphan

RunPlan takes the fresh read's cells and refuses a mark whose view already exists, before
kind or scope box. If a name is still taken when the writer renames, Renamed catches the
refusal, deletes the just-created view, schedule or section again with the delete checked,
and says IT IS STILL IN THE MODEL when Revit refuses that delete, the same way MakeSheet
handles a refused number. The handler comment that claimed the re-read already stopped this
now says what the code does.

### Fix 4. Parameter.Set is checked everywhere it is called for a new view

The scope box set and the annotation crop set both ignored the bool Revit hands back while
AssignScopeBoxCommand treated the same false as a refusal. Both are checked now and a false
lands under CREATED, BUT NEEDS ATTENTION. Whether this explains the user's repeated report of
views coming out with annotation crop off while the report read clean is now TELLABLE: if Set
answers false on those views, the next run will say so against each one. Whether it does
answer false there is UNKNOWN until a run. Noted while fixing: SetPlotId and MakeSheet's plot
parameter ignore the same return, are not on this fix list, and stay as they are, written
down here as the same shape.

### Fix 5. One rule for which schedules can be made

The panel passed every ScheduleType as capturable while the handler passed the captured
definitions' keys. ScheduleCapture.Read now classifies every schedule once: usable, lost a
filter at capture, or filters on no plot, and the snapshot carries the same three answers
from its own read. Both sides hand RunPlan the same rule and only the data can differ. An
uncapturable schedule is refused with its own reason, so a schedule that exists and filters
on no plot is no longer denied outright, and the old wording survives only for a type with no
reason recorded.

### Fix 6. The gate sees the whole solution

tests.yml restores and builds RcrcGreen.sln before testing, runs on pushes to main as well as
pull requests, and its comment now says the add-in compiles without Revit because the
Nice3point packages supply the reference assemblies, which this repo's own builds prove. The
exact restore, build, test sequence was run here before committing it.

### Fix 7. The snapshot is pinned

DrawingSheetSnapshotTests covers the numbers grouped per plot, the scope box names narrowed
to plot identifiers, the free numbers built from the numbers rather than the names, the plot
records with their words, and the capturable lists. The three audit breaks were made again
with the tests in place:

- NumbersOnPlot returning empty always: 1 failed, 911 passed
- the PlotId filter dropped from PlotsWithAScopeBox: 1 failed, 911 passed
- FreeSheetNumbers fed SheetNamesInUse: 1 failed, 911 passed

Two more breaks on the new rules: the ViewParameter source dropped from the union failed 2,
and the presence refusal disabled failed 1. All five restored, suite green at 912 after each.

### What ran here

dotnet restore, build of the whole solution and the full suite, repeatedly and after the last
file: 912 tests, 0 failed, 0 skipped, up from 904 with the deleted PlotViewGridTests out and
the new snapshot, registry, definition and plan tests in. One slip worth recording: restoring
a break with git checkout wiped the uncommitted snapshot work once, it was reapplied from the
session's own record, and every restore after that reversed the edit instead.

### What did not run, item by item

Nothing in this round has been through Revit.

- The element walk inside the panel refresh has never run there, and its cost on the real
  model is inferred from the scan's measured 1.4 seconds, not measured in the panel
- No box-only plot has been seen on a row, and how the suffix wraps at pane width is UNKNOWN
- No capture has recorded a real unreadable filter or field, and whether Revit ever throws on
  those reads in practice is exactly what the guards exist for and is UNKNOWN
- No stale mark has been refused in Revit and the rename-then-delete path has never executed
- Whether Parameter.Set answers false for a template-controlled scope box or annotation crop
  is UNKNOWN until a run says so
- The reworked gate has not run on the runner yet when this entry is written

---

## 2026-09-09, twenty sixth pass. The report for the divided sheets round

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because pull request 32 is
merged. The entry below went in with its work, so it could not carry its own merge or runner
count. It carries them now.

Pull request 32 merged as `6f212e1` and the gate executed 619 tests against it, 0 failed and
0 skipped, which is the same count the local run gave after the restack onto main.

The design folder was named pr-32 before the pull request existed and the pull request opened
as 32, so the guess held this time. Nothing else changed. No code is touched.

---

## 2026-09-09, twenty fifth pass. A sheet has no scale, and one description makes a set

Branch `claude/rcrc-green-setup-wf9ham`, one commit, restacked onto main after the two KPI
rounds landed there mid round. The merge sha and the runner count go in the next entry,
because this one is written before the pull request exists and the last time a number was
guessed here it was wrong.

Off the third full run: 13 created, 0 refused. KERBS built on its category number, both budget
filters went back as integers, and the title block size read 841 by 594 mm off the placed
block. Three faults remained and one question had to be answered before anything moved.
Leading with that one, item 5.

### 5. A sheet has no scale of its own, so there is nothing to set

The reading offered was that the Scale a sheet shows is a readout of the views placed on it,
each from its own view template, not a setting of the sheet. Checked against the API before
acting on it, and it holds:

- An Autodesk forum thread on Scale as Indicated, forums.autodesk.com, Revit Architecture
  forum, "Issue with Scale in view title, Scale as Indicated", says the sheet's Scale
  parameter is enabled only when at least one view is placed on the sheet and every placed
  view has the same scale, and reads As Indicated otherwise
- The View.Scale page on revitapidocs.com, the 2026 edition of the ViewScale property, which
  is a property of the view, not of the sheet, and is what a view template drives

So the sheets that looked wrong were showing exactly what sat on them, and the fault behind
the complaint was item 2: one typed name went onto every sheet, so a location key plan sat
under GENERAL ARRANGEMENT LAYOUT and read as a layout at a mad scale. Nothing in this round
sets a scale on anything, because a view's scale belongs to its template and overriding it
would fight the team's own settings. The run report now records the scale of every placed
view, and its closing note says whose the scale is.

How real sheets place their viewports is a separate question and it is UNKNOWN. The scan
grew a VIEWPORTS ON EXISTING SHEETS section, scale, centre, size and sheet size in
millimetres off every placed viewport and schedule, sizes off the placed title blocks by
OwnerViewId, so the tool's centred placement can be held against the team's real ones the
next time Scan Model runs. Until then nothing about placement was changed on a guess.

### 1. One sheet definition makes as many sheets as the views need

`SheetDivision.Of` in Core chunks the ticked views, in the order they were ticked, into
`PlannedSheet`s. Six views at one per sheet is six sheets, at two is three, at four is two
with the second holding two, and a flatten test proves no view is ever left off. The old
`Placed` and `LeftOff` split on `SheetDefinition` is deleted, and so is the sheet name it
carried, because one typed name across a divided set is the fault item 2 names.

**Superseded: a definition with no views used to make an empty sheet on purpose.** It makes
no sheets now. The division says a sheet is what its views need, and no views need nothing.
The report's closing note and the step 4 words both changed with it, and the panel line that
promised an empty sheet is gone. Flagged here rather than slipped in, because it reverses a
behaviour an earlier round wrote down as wanted.

### 2. A one view sheet is named from its view

`SheetNaming.FromView` upper cases the view name and drops the bracketed code, so
(200) General Arrangement Layout proposes GENERAL ARRANGEMENT LAYOUT, the way the team's own
sheets read. The name sits in an editable box prefilled per row. A sheet holding more than
one view gets no proposal, the row says why in `PlannedSheet.WhyNothingIsProposed`, and the
sheet is not made until a name is typed.

### 3. A sheet number continues the plot's own pattern

`SheetNumbers.PlotLetter` reads the letter after the leading digits of each of the plot's own
numbers, DM-11's 010QE to 600QD all read Q, and every parseable number must agree or there is
no letter. `SheetNumbers.Propose` offers the code, that letter, then the first sheet letter
from A not in use anywhere, so DM-11's code 010 proposes 010QA even though QE to QH exist,
because A is free. A plot with no sheet numbers gets an empty box and the reason. One growing
set of taken numbers is threaded through every row of every described sheet, so two proposals
on one panel can never collide, and a typed number joins the set so later proposals move out
of its way. `FaultIn` still warns under the box for a number the model holds or a number
asked for twice, the same method the run summary counts with.

### 4. Step 4 is a table of the sheets that will be made

Per definition: Type, Views per sheet, the view ticks, then one row per sheet per ticked
plot. Views read only, name and number editable, prefilled where a proposal exists. The line
over the table is `SheetBatch.InWords`, how many sheets this press makes across the ticked
plots and how many rows are still short. What the user types is filed under the plot and the
planned sheet's own views, so an edit survives the redraw and stays with its sheet while the
division changes shape. A keystroke refreshes every other box in place, skipping the focused
one, because a proposal that moved has to show its move and rebuilding the tree would take
the cursor out of the box. `RunPlan` refuses an unusable definition once, refuses a row short
of a name or a number by plot and views, and drops a row on an unticked plot silently, the
same as a mark.

### 6. Both reports say where every viewport landed

The run report grew WHERE EACH VIEWPORT LANDED: after the sheet's placements are created,
`ModelWriter` regenerates once and reads each viewport's centre and outline back off what
Revit made, with the view's own scale, a schedule's bounding box the same way, next to the
sheet size. `ViewportRecord` in Core formats it all in millimetres. The scan reads the same
shape off every existing sheet, so the two files can be held against each other. The report
also names, per sheet, whether its name and its number were generated or typed.

### 7. The lowercase s is answered

NS-19-(400) Landscape Cross section is spelled that way by its own sibling. The model does
it, so the tool copying it is correct. Nothing to do, recorded so it is not chased again.

### 8. Sheet parameters are out of scope

PRX_ parameters beyond PRX_Plot_ID, and Approver, Designer, Checker and Author, are not
filled, get no controls and are not raised in the report. The team's decision, recorded so
nobody reads their absence as an oversight.

### What ran here

`dotnet build` on the solution and `dotnet test` on the suite after the last file: 475 tests
on the pre KPI base, then 619 with main merged in, 0 failed, 0 skipped on both. Three guards were broken on purpose and watched go red before being
trusted: the division stepped past a view and the flatten test caught it, the taken number
guard was bypassed and the stepped over letter test caught it, and the ticked order was
sorted and the order test caught it. All restored, suite green again.

### What did not run, item by item

Nothing in this round has been through Revit.

- No sheet has been made by the division, and no run has executed any of this round's code
- No proposal has been rendered. Whether the four column table fits the docked pane, whether
  a warning line under one box pushes its row out of step, and how the table reads at 17
  plots by 3 planned sheets are all UNKNOWN
- The keystroke refresh has never been exercised. Whether writing ComboBox text while its
  dropdown is open fights the selection is UNKNOWN
- No `ViewportRecord` has been written by a real run. `GetBoxCenter` and `GetBoxOutline`
  units and origin are taken from the API documentation, not measured
- Whether a `ScheduleSheetInstance` bounding box is readable after the one regeneration is
  UNKNOWN, and a null there records zeros, which print as 0 mm rather than a plausible size
- The scan's viewport section has never run, and its cost across 1,385 sheets and 953 placed
  views is unmeasured
- The forum's account of Scale as Indicated has not been reproduced on this model

---

## 2026-09-09, twenty second pass. The report for the second full run round

Branch `claude/rcrc-green-setup-wf9ham`. The entry below went in with its work, so it could not
carry its own merge or runner count. It carries them now.

Pull request 29 merged as `a44fe4f` and the gate executed 436 tests against it, 0 failed and 0
skipped, which is the same count the local run gave.

The pull request number in that entry read 28 while the pull request opened as 29, because the
entry is written before the pull request exists. Corrected here rather than left to be found.

Nothing else changed. No code is touched.

---

## 2026-09-09, twenty first pass. A category is a number and a Yes is a 1

Branch `claude/rcrc-green-setup-wf9ham`. Pull request 29, one commit. Merged as `a44fe4f` with
436 tests against it on the runner, 0 failed and 0 skipped.

Five items off the second full run: 8 created, 6 refused, 3 needing attention. Leading with the
two schedule faults, because both are the same shape and both were findable by reading the code
once somebody said what the model really holds.

### 3. Slab Edges, and why a display name could never have worked

`ModelWriter.CategoryIdFor` walked `Document.Settings.Categories` looking for a category whose
`Name` matched the captured string. `ScheduleCapture` had read that string from
`Category.GetCategory(document, definition.CategoryId)`.

**Those are two different lookups over two different sets, and only one of them can see every
category.** `Category.GetCategory` resolves any category id a schedule can sit on.
`Document.Settings.Categories` is the top level of the Object Styles tree. So capture could hand
create a name that create had no way of finding, and it did: KERBS is built on Slab Edges, which
is `OST_EdgeSlab`, and the run answered "this model has no category named Slab Edges" on a model
that plainly has it. Two records of one fact, for the sixth time in this repo.

`Category.BuiltInCategory` has existed since Revit 2023 and gives Revit's own number for the
category. That is captured now, and `CategoryIdFor` resolves it with
`Category.GetCategory(document, builtIn)`. **No name is matched anywhere.** The name is kept for
the report, because the number is what resolves and the name is what a person recognises.

**Which of the six now resolve, honestly.** LIST OF DRAWINGS is a Sheet List, built by
`ViewSchedule.CreateSheetList`, so it never touched a category lookup and is unaffected either
way. HARDSCAPE and SHRUBS AND LAWN are both Floors, and KERBS is Slab Edges. The categories of
SOFTSCAPE and SIGNAGE are UNKNOWN, because nothing in this repo records them and no scan report
here lists them.

What can be said without a run is narrower and worth stating plainly: **no category can fail for
its name any more**, because no name is compared. The one remaining way to fail is a captured
category that is not one of Revit's built-in ones, and that is reported by name and number
rather than as a missing name. Whether each of the six actually resolves in that model is
UNKNOWN until the next run.

### 4. A Yes is the integer 1

`ScheduleCapture.ValueIn` had four branches, one per typed getter Revit offers, and every one of
them ended in `.ToString()`. `MakeSchedule` then handed the string straight to
`new ScheduleFilter(fieldId, ScheduleFilterType.Equal, rule.Value)`, which is the string
overload. Four kinds in, one kind out, and only one of the four was right.

SHRUBS & LAWN and SOFTSCAPE both filter on PRX_Included In Budget equals Yes. That is a Yes/No
parameter, which Revit stores as an integer, so the value coming out of capture was 1 and the
value going back in was the two-character string "1". Revit answered that the filter value is
not valid for the field and filter type, which is exactly what it was.

`FilterValue` carries the kind with the value now, all four survive a round trip through text,
and `TryRebuild` picks the matching `ScheduleFilter` constructor. Reading a value back from text
refuses rather than falling back to a string, because a filter quietly downgraded is the fault
this type exists to stop. Only a text value can name a plot, so a whole number is never mistaken
for one and swapped by `ForPlot`.

### 1. Annotation crop is forced on

Copying it was last round's answer and the report showed exactly why it failed:
DM-11-(010) Overall Plan, set up from PL-17-(010) Overall Plan, annotation crop off. The sibling
has it off, so the new view inherited the fault faithfully. The model disagrees with itself and
there is nothing there to copy.

It is on for every plan view the tool makes, and `AnnotationCropChoice` says whose setting it is
in the report, the same way `SectionDepth` does. Crop View and Crop Region Visible are still
copied. `ViewCrop.CopiedInWords` prints only those two, because printing all three in the setup
line would read as though the annotation crop had come off a view.

**A section still copies all three.** No section has ever been created by this tool, so there is
no evidence a section inherits the same fault, and forcing it there would be a guess.

### 2. The dropdown offered only numbers that would fail

Three sheets refused, all with "a sheet numbered 010EA is already in this model". The refusal
was right. The offer was mine: the list was the sheet numbers already in use, so every entry in
it was certain to be rejected.

`SheetNumbers.Free` offers numbers no sheet carries, each one a number in use with its last run
of digits stepped on until it lands free, so every offer is shaped like something the project
already does. 010EA gives 011EA, 010QE Copy 001 gives 010QE Copy 002, L-211 gives L-212. Free
typing stays.

A number that will be refused says so under its box as it is typed, and step 5 counts them
before the confirmation. Two ticked plots given one number is the same fault a second later and
is caught the same way, as is two separately described sheets asking for one number, because
they go into one model. Every warning refreshes on any keystroke in any box, because a duplicate
is about two boxes and neither knows about the other on its own.

### 5. A calculated field is not a missing parameter

LIST OF DRAWINGS is short of Sheet Numbering, HARDSCAPE of Area Conversion and TOTAL SAR,
SIGNAGE of CODE. The report said they were not schedulable for this category, which sends
somebody to look at the category.

`GetSchedulableFields` offers the parameters of a category and never a formula, a percentage, a
count or a combined parameter, because those are defined inside the schedule that holds them.
`ScheduleField.FieldType` says which a field is, so capture records it and the report now says a
calculated field has to be written again by hand.

**Whether those four fields really are calculated is UNKNOWN.** Their names read like it, and
TOTAL SAR reads like a formula over Area and Cost, but nothing here has opened that schedule.
What the change does is make the report say which of the two it is rather than assert the wrong
one, and it says it from `FieldType` rather than from the name.

### What has not been run

Nothing in this round has been through Revit. Specifically not observed:

- no schedule has ever been created by this tool, before or after this round
- no category has been resolved by its number, and no schedule has been built on one
- no filter has been rebuilt as a whole number, a number or an element reference
- `ScheduleField.FieldType` has never been read off a real field
- no sheet number has been offered from the free list, and none has been typed into the box, so
  no warning line has ever been rendered
- no section has ever been created, so the decision to keep copying its annotation crop rests on
  no evidence either way
- the run summary has never counted a clash on screen

The mockup is at `design/pr-28/panel.html`, both themes, and says so at the top.

Locally, after the last file was written, `dotnet build RcrcGreen.sln` came back with 0 warnings
and 0 errors and `dotnet test` with 436 passed and 0 failed. Three new guards, the free numbers,
the forced annotation crop and the refusal to downgrade a filter value, were each watched failing
against deliberately broken code before being trusted.

---

## 2026-09-09, twentieth pass. The report for the first full run round

Branch `claude/rcrc-green-setup-wf9ham`. Both log entries went in with their work, so neither
could carry its own merge or runner count. Both carry them now.

Pull request 25 merged as `281056e` and the gate executed 379 tests against it. Pull request 26
merged as `6e64c73` and the gate executed 398. 0 failed and 0 skipped on each, and both match the
local run the entry already named.

Nothing else changed. No code is touched here.

---

## 2026-09-09, nineteenth pass. Four faults found by using the grid

Branch `claude/rcrc-green-setup-wf9ham`. Pull request 26, one commit. Item 5 of the round, the
second half. Items 1 to 4 went in as pull request 25. Merged as `6e64c73` with 398 tests against
it on the runner, 0 failed and 0 skipped.

None of these four is a bug in the sense the last five rounds have been. The grid was correct
and it was unusable, which is a different fault and one that only appears when somebody works in
it rather than reads it.

### The list that threw itself back to the top

Ticking a view type near the bottom of 84 scrolled the list back to the first one, so the user
scrolled down again for every single tick. Step 2 is thrown away and built again on every
change, which is the rule that keeps one record of what is ticked, and a brand new ScrollViewer
starts at nothing.

`Scrolling` keeps the offset by name across the rebuild. **It restores on the first layout pass
rather than on Loaded**, because a ScrollViewer that has not measured its content yet clamps any
offset to zero, and a restore that silently clamps reads exactly like one that worked. Step 1's
plot list had the same fault and gets the same treatment.

### 136 clicks

17 plots by 8 view types. Three ways in now: Mark every missing, a plot name for its row, a
column header for its column, and Clear all marks to undo the lot.

`BulkMarking` in Core decides all of it and hands back only cells a single click on the grid
would have marked. **A square holding a view is never swept up**, because marking says a view is
wanted and that one is there. **An unticked plot is never swept up either**, because the run does
not act on it and the marks would be dropped without a word. A single click on one square is
still allowed anywhere, because that is a deliberate act rather than a sweep.

The words are in Core with the counts, so the status line reads `12 cells marked, 12 in total.`
and a sweep that finds nothing says why rather than going quiet.

A plot name and a column header are still labels. `Flat` puts each inside a button with
`PanelTheme.Clear` and `PanelMetrics.Nothing`, so the frozen plot column stays in step with the
scrolling cells beside it. A real button there would put chrome down the side and across the top
of a grid that is already dense.

### Headers off the right edge

`(200) General Arrangement Layout` is thirty characters over a column one square wide.

`GridColumnLabels.For` gives each column the shortest header that still says which it is: the
code alone, and where a code is shared, the code plus as many leading words of the view name as
it takes to tell the sharers apart. Code 010 appears twice on this model with two different view
names, which is the whole reason `ViewType` holds both, so the code alone could never have been
the answer. `(010) Location` and `(010) Overall` are what those two come out as.

Where one name is the whole start of another, nothing shorter than the full name separates them
and the full name is what the header carries. A header that lies by half is worse than a wide
one, and there is a test for it.

The full name is on the tooltip, and a key under the grid lists every header that lost
something, so a grid of plain codes carries no key at all.

### The legend

Left exactly as it was, which is what was asked.

### What has not been run

Nothing in this round has been through Revit, and none of it is drawable in a mockup either:

- no list has been scrolled and rebuilt, so the restore on first layout has never executed
- no cell has been marked in bulk by any of the three routes
- no borderless button has been rendered, so whether a plot name still reads as a label rather
  than a button is UNKNOWN
- whether the frozen column stays in step with the scrolling cells now that both hold buttons
  is UNKNOWN
- no shortened column header has been rendered, so whether eight of them really fit across a
  docked pane is UNKNOWN

The mockup is at `design/pr-26/panel.html`, both themes, and says at the top that it cannot
answer any of those.

Locally, after the last file was written, `dotnet build RcrcGreen.sln` came back with 0 warnings
and 0 errors and `dotnet test` with 398 passed and 0 failed. Both new guards, the unticked plot
and the shared code, were watched failing against deliberately broken code before being trusted.

---

## 2026-09-09, eighteenth pass. The empty sheets, the annotation crop, and a depth the model never had

Branch `claude/rcrc-green-setup-wf9ham`. Pull request 25, one commit. Merged as `281056e` with
379 tests against it on the runner, 0 failed and 0 skipped.

**This round is split.** Items 1 to 4 are here, the write path and the scan. Item 5, the four
interface faults, is a second pull request. Item 1 is the reason both sheet attempts have made
nothing, and putting it behind a panel rebuild would hold it up for no reason.

### 1. The empty sheets, and what the cause actually is

**`SHEET_WIDTH` and `SHEET_HEIGHT` are read-only INSTANCE parameters. They do not exist on a
title block type at all.** `ModelWriter.PlaceViews` read them off `block`, which is the
`FamilySymbol` the sheet was created from. `get_Parameter` returns null for a parameter that is
not on the element, `Number` turns a null parameter into 0.0, the guard on `width <= 0.0` fired,
and the report said the title block reports no width or height. AR-PRX-Title_Block_A1 is a real
A1 and every word of that message was wrong about it.

This is not reasoning from the symptom. It is what Autodesk's own material says. The Building
Coder's sheet size sample reads both off the `FamilyInstance` and states that the sheet itself
carries no width or height. A Dynamo thread asking for the size from a title block family with
no instance placed ends the same way: instance parameters only exist in an instance, and a type
cannot be asked.

So the size comes off the title block Revit places on the new sheet. `ModelWriter.SizeOf`
regenerates, finds that instance in the sheet's own view and reads the two parameters off it. A
title block family that does not drive them is measured across instead, because a block drawn at
A1 is still A1, and `SheetSize` carries which of the two reads answered so the report can print
it. In millimetres, because that is what somebody calls an A1.

The ordering this forces is the part worth remembering. **The size cannot be read until the
sheet exists**, because the title block has to be placed before it has one. So a sheet that has
views ticked and cannot be measured is created, found wanting, and deleted again, with the
delete checked the way the schedule delete is and the sheet never recorded as made until that is
settled. A definition with no views ticked is not measured at all and still makes an empty sheet
on purpose.

`SheetSize.Of` refuses a zero, a negative, a NaN and an infinity, so the guard that was doing the
work in the writer now sits where `SheetLayout.For` cannot be reached around it. That test was
watched failing against a `SheetSize` that let a zero through.

### 2. Annotation crop, and the full list of what is copied

`ViewCrop` holds Crop View, Crop Region Visible and Annotation Crop, and travels on
`SiblingView` with the family type, the template and the level. Crop View is set first because
Revit will not turn Annotation Crop on for a view whose crop is off, which is why it is copied
even though it was not asked for by name. Annotation Crop is a parameter rather than a property,
so it comes off `VIEWER_ANNOTATION_CROP_ACTIVE`.

`ApplySiblingCrop` runs after the template, so a template controlling any of the three refuses
and is reported rather than quietly losing.

**Read off the sibling, all from the one view:**

- view family type
- view template
- level, on a plan view
- Crop View
- Crop Region Visible
- Annotation Crop

**Set by the tool, deliberately not from the sibling:**

- the view name, built from the plot and the view type
- PRX_Plot_ID, set to the plot being made
- the scope box, the plot's own, on a plan view only
- far clip offset, on a section, now one metre
- the section box, worked out from the plot's scope box

**Not read at all.** A new view gets whatever the view template says, and whatever Revit gives a
new view where the template says nothing:

- scale, detail level, discipline, phase and phase filter, visual style
- view range: top, cut plane, bottom and view depth
- the crop rectangle itself, and the annotation crop offset around it
- underlay
- graphic overrides, view filters, workset visibility
- sun, shadows and orientation
- title on sheet, and the referencing sheet
- colour scheme and colour fill
- every project and shared parameter on the view except PRX_Plot_ID

That list is the answer to the item, and it is also the list of the next things likely to come
back off a real drawing.

### 3. One metre, and the label

The far clip no longer comes from the sibling. `SectionDepthChoice` is deleted and so is
`FarClipFeet` on `SiblingView`. `SectionDepth.Metres` is 1, `SectionDefaults` reads that one
constant and converts it to feet, and the report says the depth is the tool's setting rather
than anything read off a view. The two copies of the metres, one in Core and one in the Revit
project, are down to one, which is the fifth time that shape has come up here.

**On the label, I could not reproduce the fault and I am not going to pretend otherwise.**
`SectionDepthChoice.InWords` printed `SectionDepth.InMetres(Feet)`, which multiplies feet by
0.3048, and a test asserting the exact string `Looks 12.83 metres, taken from
DM-20-(400) Landscape Cross Section.` for an input of 42.1054 feet was green on the runner for
both pull request 22 and pull request 23. There is only one place in the whole repository that
prints the word metres next to a number, and it converts.

What 42.11 metres is consistent with is a sibling whose far clip is about 138.1 feet, which
converts to 42.11. The report names the view every line came from, so the sibling that was
actually picked can be held against it. Two numbers that look alike is a thin thing to build a
fix on, and the whole line is gone now regardless.

The conversions moved into `Lengths` so there is one place feet turn into metres or
millimetres, which is the fix that survives whatever the cause was.

### 4. Where one view type is built more than one way

`FamilyTypesInUse.Of` counts, per view type, every view family type in use and how many views
use each, and the scan report prints it under VIEW FAMILY TYPE PER VIEW TYPE with the
disagreeing ones first. `ModelScanner` reads the family type off every view to feed it.

Nothing picks a winner and nothing changes how the sibling is chosen. Three (010) views came out
of one run with three different family types, all copied faithfully. That is the model's answer
and the tool's job is to show it.

### What has not been run

Nothing in this round has been through Revit. Specifically not observed:

- no sheet has been created by the fixed size read, at any of 1, 2 or 4 views per sheet
- no sheet has been refused for an unreadable size, and that path deletes a sheet it just made
- `document.Regenerate` inside the run transaction has never executed
- measuring a title block across its bounding box has never executed
- no view has been created with the crop settings copied, and no template has refused them
- no section has ever been created by this tool at all, at one metre or at any other depth
- no schedule has ever been created by this tool at all
- the scan has not been run since the family type per view type section was added

The panel is untouched in this pull request, so there is no new mockup. Item 5 brings one.

Locally, after the last file was written, `dotnet build RcrcGreen.sln` came back with 0 warnings
and 0 errors and `dotnet test` with 379 passed and 0 failed. Both new guards, the sheet size and
the ordering of the disagreeing view types, were watched failing against deliberately broken
code before being trusted.

---

## 2026-09-08, seventeenth pass. The report for the split round

Branch `claude/rcrc-green-setup-wf9ham`. Both log entries went in with their work, so neither
could carry its own merge or runner count. Both carry them now.

Pull request 22 merged as `ab83eb1` and the gate executed 352 tests against it. Pull request 23
merged as `cd987d0` and the gate executed 361. 0 failed and 0 skipped on each, and both match
the local run the entry already named.

Nothing else changed. No code is touched here.

---

## 2026-09-08, sixteenth pass. Sheets, rebuilt

Branch `claude/rcrc-green-setup-wf9ham`. Pull request 23, one commit, the second half of the
round pull request 22 opened. Merged as `cd987d0` with 361 tests against it on the runner, 0
failed and 0 skipped.

### What was there, and the fault it hid

Step 4 held a dropdown of the sheets the model already has and a table of a sheet number and a
sheet name per plot. Run copied the sheet that was picked: its title block, its viewports and
where each one sat. The one sheet this has ever made in a real model came out empty, because the
sheet picked to copy had no views on it. Nothing threw, nothing was refused and the report said
a sheet was created, which was true.

That is the lesson worth keeping. **Copying takes whatever state the thing is in, including
nothing.** A tool that reads its answer off an existing object inherits every gap in it and has
no way to notice, because the gap is a legitimate value.

It also asked for an order of work the team does not follow. One plot has to be laid out by hand
first, and only then can the rest be made from it.

### What replaces it

`SheetCapture` is deleted. Leaving it in place would have left two ways to build one thing, and
this repo has now been bitten five times by two records of one fact.

A sheet is described. Four things are shared across every ticked plot: the title block type, the
sheet name, the view types that go on it, and whether 1, 2 or 4 views go per sheet. One thing is
per plot, the sheet number, because that is the only part that differs between the sheets one
description makes.

The title block types are read from the model. So are the two lists behind the name and the
number, which are the names and numbers already in use, offered in editable dropdowns because a
new sheet usually carries a number no sheet has yet. The list is an offer and never a
restriction, and the tool still invents neither. A plot with no number gets no sheet and the
report names the plot and the sheet.

More than one sheet can be described, so one press gives a plot its LIST OF DRAWINGS and its
GENERAL ARRANGEMENT LAYOUT together.

### What moved into Core

`SheetLayout.For` takes the title block's width and height and a count of 1, 2 or 4 and hands
back the centre of each viewport in reading order. It divides the sheet evenly, so the margin
outside is the same measurement as the gap between. Y counts up from the bottom in Revit, which
is the only thing about it worth remembering, and it is why the first spot back is the top one.
A title block reporting no size, or a NaN, is refused at the door rather than placing everything
on the origin.

Every expected number in `SheetLayoutTests` is written out by hand from an 800 by 600 sheet
rather than worked out with the same division the code uses, and the margin test holds the four
edges against the sheet edges rather than against each other.

`SheetDefinition` now holds what the user chose rather than what a sheet already had.
`SheetRequest` is one plot and one number, `SheetOrder` pairs a definition with the number every
plot gets for it, and `TitleBlockType` is the family and type name pair the dropdown lists.
`RunPlan.Of` takes the orders and turns each one into an item per ticked plot that has a number.

`SheetBeingDescribed` is the one new type in the Revit project, and it is there because it is
mutable and the panel owns it while somebody is still filling a sheet in. Step 4 is thrown away
and built again on every change, so a control that has gone is not a place to keep the only copy
of something typed. What it hands out is the immutable Core definition, narrowed to the view
types still ticked in step 2, and the tick is remembered rather than dropped so re-ticking a
type in step 2 puts it back on the sheet.

### Three things it will not do

A definition short of a type or a name is refused once rather than once per plot, because it is
one thing to go and fix rather than seventeen.

A view already sitting on another sheet is refused rather than moved.
`Viewport.CanAddViewToSheet` is asked before every placement, so that comes back as a line in
the report rather than as a throw that takes the transaction down.

A definition with nothing ticked still makes a sheet, empty, which is a real thing to ask for.
The card says so in words before anything runs, so nobody is surprised by an empty sheet twice.

### What has not been run

No sheet has ever been created by this route. The one sheet this tool has made was made by the
copy route that is now deleted.

No section and no schedule has ever been created by this tool at all. The section path, the
schedule path, the far clip taken off a sibling, the refusal of a view already on a sheet, the
placement of any viewport at any of the three counts, and the empty sheet case are all written
and tested in Core and none has been through Revit.

The mockup is at `design/pr-23/panel.html`, in both themes, drawn by hand from
`DrawingSheetPanel.cs`. It is not a screenshot and says so at the top.

Locally, after the last file was written, `dotnet build RcrcGreen.sln` came back with 0 warnings
and 0 errors and `dotnet test` with 361 passed and 0 failed. The runner agreed on both counts.

---

## 2026-09-08, fifteenth pass. Four fixes off the first run that created anything

Branch `claude/rcrc-green-setup-wf9ham`. Pull request 22, one commit. Merged as `ab83eb1` with
352 tests against it on the runner, 0 failed and 0 skipped.

**This round is split.** Items 1 to 4 are here. Item 5, sheets rebuilt, is a second pull
request. It replaces the whole sheet route, adds layout maths to Core and rebuilds step 4, and
putting it on top of four unrelated fixes would leave no green gate between them.

### 1. The family type, and what I actually found

**Neither of the two candidates is the cause, and I can show that from the code rather than
argue it.**

Candidate (a), a leftover name match or a first-found lookup. Ruled out. Grepping `src/` for
every place a view family type is chosen gives three hits: `ModelScanner.ReadViewFamilyTypes`,
which only reads them for the scan report, and `sibling.GetTypeId()` twice in `ModelWriter`,
once in `MakePlanView` and once in `MakeSection`. There is no name match. `ViewTypeNaming`,
which held the old one, was deleted in pull request 19 and nothing replaced it.

Candidate (b), the lookup called more than once with the calls disagreeing. Ruled out. Grepping
for `SiblingOfType` gives three hits: the definition, one call in `MakePlanView` and one call in
`MakeSection`. One call per item.

And inside `MakePlanView` the two settings were read off the same local, four lines apart:

```
ElementId familyTypeId = sibling.GetTypeId();          // line 141
ViewPlan made = ViewPlan.Create(document, familyTypeId, siblingPlan.GenLevel.Id);
ApplySiblingTemplate(made, sibling, item, outcome);    // made.ViewTemplateId = sibling.ViewTemplateId
```

Nothing between them can rebind `sibling`. **The code could not have taken them from two
different views.**

**So what is left.** The sibling view itself carries family type `(200) General Arrangement
Layout` and view template `(010) Overall Plan`, and the copy was faithful. That is not a strange
thing for this model to do. It is the same fact already written in `CLAUDE.md` one step further
on: a view family type is not named after the view type, `(010) Location Key Plan` is built on
`(010) Key Location Plan`, and a team building every plan view on one family type and telling
them apart by template is ordinary Revit practice.

**What I cannot prove, and will not pretend to.** Which view was the sibling. UNKNOWN. Nothing
recorded it, and I have no access to the model. That is the actual fault here: not that the
setting was copied from the wrong place, but that **nothing said where it was copied from**, so
a plainly answerable question could not be answered.

So the fix is the one asked for, and it is the right one whatever the sibling turns out to hold.
`SiblingReader.Of` reads every candidate view into a Core `SiblingView` once per run, before
anything is created. `SiblingChoice.For` picks one. The family type, the level, the template and
the far clip all travel on that one object. The report names it:

```
WHERE EACH NEW VIEW WAS SET UP FROM, 3
  DM-11-(010) Overall Plan. Set up from DM-18-(010) Overall Plan: family type (200) General
  Arrangement Layout, view template (010) Overall Plan, level Level 1.
```

One run of that settles it in one line and no Properties panel.

**A second fault turned up while doing it.** `SiblingOfType` took the first view of the type
whatever kind it was, and `MakePlanView` then refused if that view was not a `ViewPlan`. A view
type the model draws both ways would refuse a plan view because the first match was a section,
while a usable plan sat further down the list. `SiblingChoice` prefers the kind that can answer.

**The test.** `NoChoiceEverPairsOneViewsFamilyTypeWithAnothersTemplate` hands two views of one
type with different family types and different templates and asserts the chosen pair belongs to
one of them. I broke `SiblingChoice.For` on purpose, making it take the family type off the last
candidate and everything else off the first, and watched that test and its neighbour go red, 350
passed and 2 failed. Restored, 352 passed.

### 2. The empty code dropdown, and it was mine

`FillCodeButtons` in pull request 19 did two things: filled the code buttons and filled
`_newCode.Items`. When the panel was rebuilt into steps I moved the buttons inline into
`InsideViewTypes` and deleted the method. The dropdown fill went with it. `git show 8a68492`
against the current file makes it plain: the old file has `_newCode.Items.Clear()` and
`_newCode.Items.Add(which)`, the new one has no line that adds an item to it at all.

The same shape this repo keeps hitting, wearing different clothes. One method served two records
of one fact, and splitting it took only half. They are filled in the same loop now, and
`FillTheCodes` compares before it clears so an open list does not lose its selection.

### 3. Section settings from the model

`SectionDepthChoice.For` takes the sibling section's far clip offset where it has one and falls
back to the value the team named where it does not, and carries which of the two so the report
can say. On the real numbers, 42.1054 feet reads as 12.83 metres, and the fallback still reads
as 10.

**The scope box part needed no change.** A section already carried none. `MakeSection` sets the
plot parameter and the template and stops, with a comment saying why. I have left it alone and
sharpened the comment rather than claiming a fix I did not make. The plot's box is still
required for the section to be created at all, because it is what says where to cut.

### 4. The scan

Two counts under SHEETS: how many sheet numbers hold the word Copy, and how many sheets carry no
PRX_Plot_ID. Each sheet row now also prints the plot it carries, or (none). Report only.

### What was checked, and how

`dotnet build RcrcGreen.sln` and `dotnet test`, both after the last file was written. Build 0
warnings and 0 errors across all three projects. 352 tests, 0 failed and 0 skipped, locally, up
from 332.

The new sibling test was watched failing against a deliberately broken `SiblingChoice` before
being trusted, which is the rule in `.claude/rules/core-rules.md`.

### What has never been observed

- **No section and no schedule has ever been created by this tool.** Both paths are written and
  neither has executed
- The single sibling lookup has never run. Which view it picks on the real model, and whether
  the family type it copies matches what a Properties panel shows, are both UNKNOWN
- No report has ever carried the WHERE EACH NEW VIEW WAS SET UP FROM section
- No far clip offset has ever been read off a sibling. `VIEWER_BOUND_OFFSET_FAR` is read from
  the API and has not been seen returning a value
- The refilled code dropdown has not been rendered. Whether it now opens with the codes in it
  is the one thing in this round most easily checked and it has not been checked
- The two new scan counts have never been produced by a real scan
- Nothing about the empty sheet is fixed here. That is item 5

---

## 2026-09-08, fourteenth pass. The interface rebuilt as five steps

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#20](https://github.com/baderrahal/RCRC-Green/pull/20), one commit, 12 files, merged into main
as `67dec1e`. The gate executed 332 tests against it, 0 failed and 0 skipped, which is the same
count the local run gave.

This is item 9 of the round. Items 1 to 8 went in as pull request 19 and merged as `8a68492`.
The split is written up in the entry below this one.

### What was wrong with it

The panel worked. It was a flat list of controls, top to bottom, and it read as a wall to
anyone who had not built it. A production person opening it for the first time had no way to
tell what to do first, what a control acted on, or why half of them did nothing yet.

### Five steps

`PLOTS`, `VIEW TYPES`, `MARK`, `SHEETS`, `RUN`, in the order somebody does them, one open at a
time. Four rules hold it together and the first three are the ones that make it readable.

**A shut step carries its own summary.** `1  PLOTS   DM-11 to DM-28, 17 of 17 ticked`, and
`2  VIEW TYPES   4 of 84 ticked`. The whole state of the panel reads without opening anything.

**A step that cannot be used yet is greyed out with one line saying why.** Step 3 with no view
types ticked says to tick one in step 2 and that the grid has no columns until then. A disabled
control with no reason next to it tells nobody anything, and there were several.

**Every summary and every reason is Core, with tests.** `PanelSteps` holds all five steps, what
each says shut, whether it is usable and why not. The panel draws them and formats none of them.

**Nothing drags the user out of a step they are working in.** Picking a range opens step 2 by
itself, once per read, because that is the one act that unlocks everything below it. Everything
else is a Next button at the foot of the open step, or a click on any header. Auto advancing on
every completing action would have thrown somebody out of the grid on their first mark.

### The fourth case of the same fault

Writing `PanelSteps` turned one up. A step below the plots was usable whenever the range fields
held a value, even on a panel that had read no model at all, because the range came off the
arguments rather than off whether step 1 was usable. Two tests went red on the first run and
both were right. The code was wrong.

That is the grid cell, the column count, the run report and now this. Four times, one shape:
two records of one fact, kept up to date by two paths.

### The grid

A legend above it for the three marks. A frozen Use and Plot column, so the plot a row belongs
to stays on screen however far across somebody has scrolled. Shading on every other row. Both
halves share one fixed row height from `PanelMetrics`, which is the only thing making them line
up, because auto height on either side drifts the moment one cell wraps.

A column header says `plan`, `section` or `schedule` in a word under the name. It was italics
before, and italics is not something anyone reads off a column header.

### The rest of it

A strip at the top with the model name, when it was last read, Refresh and Scan Model, because
those belong to the document rather than to any one step. One status line, docked at the bottom,
in the same place whatever is open above it. One primary button, Run, and everything else
secondary. The scope box counts moved inside step 5, because they act on the same ticked plots.

`PanelMetrics` holds every spacing and font size, the way `PanelTheme` holds every colour. The
panel file names neither a number nor a brush. `PanelTheme` grew six colours, each with a value
per theme, because a green that reads as primary on white reads as an error on charcoal.

### One thing I got wrong and caught before pushing

The first draft of the rebuild re-parented the combo boxes and the text boxes on every redraw.
WPF refuses that outright: an element already has a logical parent and adding it to a second
one throws. It would have taken the panel down on the second click rather than the first, and
it compiled clean. `Reparented` takes a control out of whatever held it before, and anything
that is both a field and in the tree goes through it.

The same reason a keystroke in a sheet number box calls `RefreshHeaders` rather than a full
redraw. Rebuilding the tree under the cursor takes the cursor out of the box.

The first draft also painted the strip and the status line once, at construction, before the
theme they follow was read. That is exactly how the panel came up black on black the first
time, so `PaintFromTheTheme` sets their brushes by hand now.

### What was checked, and how

`dotnet build RcrcGreen.sln` and `dotnet test`, both after the last file was written. Build 0
warnings and 0 errors across all three projects. 332 tests, 0 failed and 0 skipped, locally, up
from 310, all 22 of the new ones on `PanelSteps`.

`design/pr-20/panel.html` shows all five steps in both themes, shut and open, plus the grid, the
scope box lists and a table of what every unusable step says. It is hand drawn and says so at
the top.

### What has never been observed

**None of this has been rendered.** Not one pixel of the rebuilt panel has been on a screen.

- No step header has been drawn. Whether a WPF Button wrapped round a Border reads as a header
  or as a control, on either theme, is unknown
- The frozen column has never been held against the scrolling columns. One fixed row height is
  what should make them line up and nobody has looked
- The green Run button has not been seen on the dark theme, where it may read as an error
- `Reparented` has never run. It is the guard on the fault most likely to be found on the first
  install, and it is guarded from reading the API rather than from watching it fail
- Whether two scrollbars, the pane's and the grid's, are usable together is unknown
- What a 17 row grid does to the height of a docked pane is unknown
- The six new colours in `PanelTheme` have not been rendered in either theme
- Everything from the entry below still stands: no view, section, schedule or sheet has ever
  been created by this tool, and the section path and the sheet path have never executed

---

## 2026-09-08, thirteenth pass. The first real write, and the seven things it found

Branch `claude/rcrc-green-setup-wf9ham`. Pull request
[#19](https://github.com/baderrahal/RCRC-Green/pull/19), one commit, 33 files, merged into main
as `8a68492`. The gate executed 310 tests against it, 0 failed and 0 skipped.

**This round is split.** Items 1 to 8 are here. Item 9, the interface rebuild, is a second pull
request, because item 7 alone is a Core type, a capture path, a create path and new panel
controls, and putting a full panel rewrite on top of that would make a diff nobody can review
and leave no green gate between two independent risks. The cost of splitting is that the sheets
controls go in here in the current panel style and move into step 4 in the second. I took that
over shipping sheet creation with no way to use it.

The write path ran in Revit for the first time. Four plan views were attempted and none was
created. Every item below comes out of that one run.

### 1. The report said both created and not created

Four names under PLAN VIEWS, the same four under NOT CREATED, REFUSED BY REVIT, and a status
line reading 0 created, 4 not created. The status line was right.

The created sections were printing `plan.Items`, which is what the run set out to make. The
refusal sections were printing what happened. Two records of one fact, for the third time in
this repo, after the grid cell and the column count.

`RunOutcome` is now what the run did. Something reaches a created section only by being handed
to `RunOutcome.Made` after the call that made it returned. `RunPlan` appears in the report in
exactly one line, the one beginning This run would make, and nowhere else.

The test the round asked for reads the file rather than the object, because the file is what
somebody reads. It pulls the entries out of the four created sections and the three not-created
sections and holds them against each other by name. `RunRefusal` now carries the same `Name`
string a `RunItem` would, which is what makes that a comparison rather than an argument about
two naming schemes. There is a second test that puts the fault back on purpose and watches the
check fail, because a test for a thing that cannot happen proves nothing until it has been seen
going red.

A report where that list is not empty now prints a heading saying it is a bug in the tool, above
everything else. A file that contradicts itself has nothing in it worth believing.

### 2 and 3. Name matching is gone, not fixed

I was told the view family type is named after the view type. That came from one Properties
panel and it was wrong. DM-18-(200) General Arrangement Layout uses
`(200) General Arrangement Layout` and matches. DM-11-(010) Location Key Plan uses
`(010) Key Location Plan`, the words swapped, and does not. Three of the four refusals were that.

The instruction was to remove the matching rather than fix it, and that is right for a second
reason: eight templates start with `(200) General Arrangement Layout`, so the prefix match for
the view template could never have answered either and would have created every view with no
template at all.

Both now come off the sibling, meaning a view of the same type the model already holds on
another plot. Family type from its type id, level from its `GenLevel`, template from its
`ViewTemplateId`. It is the view the team built, so it cannot be wrong about itself. No sibling
means the item is refused and the report says to make one by hand on any plot.

`ViewTypeNaming` and `TemplateMatch` are deleted along with their five tests, rather than left
sitting there as a second way to answer a question that now has one.

### 4. A cross section is not a plan view

`(400) Landscape Cross Section` is a section on this model, and its template is the only listed
template whose kind is Section. `ViewPlan.Create` can never make one. That is what refused the
fourth view, with a message that blamed the level.

There is a section path now. It reads the plot's scope box, builds a Core `PlotBox` in feet,
asks `SectionPlacement.Across` for the short way across the middle, converts the ten metres to
feet at the Revit boundary, and turns the answer into the `BoundingBoxXYZ` Revit wants. That
maths has been in Core since it was written and had never been called by anything.

**Which types need a section is read off the model, not off the code.** `DrawingSheetReader`
records the view type of every `ViewSection` it passes and `RunPlan` turns those into section
items. Nothing anywhere says that 400 means section.

The one thing worth writing down about the section box: its transform's `BasisZ` points back at
the viewer, so the view looks along the negative of it, which is why the direction Core hands
back is negated there.

### 5 and 6. The scan could not have shown either fault

It listed view templates and not view family types, so the mismatch in item 2 could only be
found in a Properties panel. There is a VIEW FAMILY TYPES section now, view family and type name
for each.

The six views whose name and PRX_Plot_ID disagree were counted and never named. They are named
now, with both values, under VIEWS THAT DISAGREE WITH THEMSELVES. Nothing changes one. Which of
the two is right is a question about the project.

### 7. Sheets

The three open questions have one answer between them: the user sets one plot's sheet up by hand
and it is copied.

`SheetDefinition` in Core holds the title block family and type, the sheet size, and one
placement per view carrying the view type and the centre of its viewport in feet from the sheet
origin. One placement per view type, because a source sheet holding the same type twice gives no
way to say which position a new one takes. `SheetCapture` reads a sheet into one. `ModelWriter`
builds a sheet from one and places each view.

Two details worth naming. Viewports and `ScheduleSheetInstance` are both captured, because they
are different elements and reading only the first would drop every schedule off a copied layout
without saying so. And `Viewport.CanAddViewToSheet` is asked before every placement, so a view
already on another sheet comes back as a refusal rather than as a throw.

Views are made before sheets inside the same transaction, so a sheet can carry a view this run
only just created.

The panel has a SHEETS section: a source sheet dropdown, and a row per ticked plot with a sheet
number and a sheet name to type. What the user types is held in the panel rather than read back
off the boxes, because the table is rebuilt whenever the range changes and a control that has
been thrown away is not a place to keep the only copy of something somebody typed.

### 8. The report

Sections and sheets have counted sections of their own, on the same rule as item 1. Every one of
them reads the outcome.

### What was checked, and how

`dotnet build RcrcGreen.sln` and `dotnet test`, both after the last file was written. Build 0
warnings and 0 errors across all three projects. 310 tests, 0 failed and 0 skipped,
locally, up from 269 after the five that went with `ViewTypeNaming`.

### What has never been observed

The write path has run once and created nothing. Everything below is unchecked.

- No view, section, schedule or sheet has ever been created by this tool
- The section path has never executed. `ViewSection.CreateSection` has never been called, the
  section box transform has never been seen by Revit, and nobody has looked at a section this
  tool placed to say whether it faces the right way or cuts in the right place
- The sheet path has never executed. No sheet has been captured, no title block found, no
  viewport placed, and `Viewport.CanAddViewToSheet` has never returned false
- The sibling lookup has never run against a real model. Whether `GetTypeId` on a view gives the
  view family type Revit will accept, and whether `ViewTemplateId` comes back valid, are read
  off the API and not off a run
- The two new scan sections have never been written by a real scan
- The SHEETS controls have not been rendered on either theme
- No schedule has lost a filter, so the guarded delete has still never been reached
- `ReportFile` has still never written anything and `reports/` has never received a file

One thing I did not do. There is no mockup this round, because the panel change here is one
section added in the existing style and item 9 rebuilds the whole panel. The mockup goes with
the second pull request, showing all five steps, which is what it was asked for.

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
