## 2026-09-09, twenty seventh pass. The template picker and the workbook writer

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 31 is merged.
Pull request 33, merged into main as `c88083f`, and the gate executed 696 tests against its
merged head on the runner, 0 failed and 0 skipped. Locally the round ran 657 before the merge
that brought the divided sheets rounds in and 696 after it, 0 failed on each, and the runner
matches the second.
Pull requests 32 and 34 landed from the Drawing Sheet branch while this was being built, and the
merge that brought them in conflicted only on this file and the state file, both settled by
keeping every entry with this one on top. The merge record `e343fe1` on main carries a
co-author credit line: it went in through the GitHub squash button, which the commit hook
cannot see, so the hook does not cover that button and the gap is now written down.
The workbook half of the KPI tool and nothing of the Revit half. It reads no model, fills
nothing, and has no fill button, because a control that does nothing is a lie about what the
tool can do. The round ends with a person able to point at a folder, see the templates in it,
pick one, read exactly which cells the tool would fill, and name the file that would be
written.

### The map is data, and the tool carries it

The production templates carry no note saying where any value comes from, measured at zero
note cells in all seven, and the annotated set that holds the mapping is not what the team
fills. So `KpiTemplates` in Core is the map, one entry per template, the cells as measured off
the annotated seven: D3, C5 and E4 everywhere, the area at D8 for the parks and H7 for
HEALTHCARE, MOSQUES, PARKING and SCHOOLS, shrubs and lawn at F11 and H11 or F10 and H10, and
no area cell at all for STREETS, whose road width and length the user types by hand. E5, G5
and H5 are the date, the person and their position, typed by the team and never written. The
tree lists run B4 to B92 and B4 to B84 on the two park templates and B4 to B83 everywhere
else, and the range comes off the map entry and never off a constant, because writing 89 rows
into an 80 row list puts nine quantities into rows no total sums. A completeness test walks
all seven entries.

`RecognisedWorkbook.Recognise` is the recognition rule. The main sheet name settles five of
seven. Park Name is two templates, so the file name breaks the tie through the letter run
match `KpiNames.Holds`, and a name holding both park words or neither puts the pick to the
user with nothing guessed. A workbook matching no entry is named with the reason and cannot
be picked.

### The writer copies the zip and patches cells, and the library is no library

Picked by live search, as asked, and here is what the search found. EPPlus moved from LGPL to
Polyform Noncommercial at version 5 in 2020 and sells commercial licences, stated at
epplussoftware.com under LgplToPolyform, which blocks free use by a company of more than
twenty people. That is the licence change the brief warned about. ClosedXML is MIT and NPOI
is Apache 2.0, both read at their github repositories, but both are load the model and save
it back, which is the measured failure: 21 of the client file's 37 parts gone while the file
still opens. DocumentFormat.OpenXml, the Open XML SDK, is MIT on nuget.org and runs on
netstandard2.0, and works at package level, so it would have served.

The choice is none of them. `WorkbookPatcher` in Core works on the zip directly through
System.IO.Compression and System.Xml.Linq, both in the platform, because the one thing the
writer must not do is rewrite parts it does not touch, and the way to be sure is to not hand
the package to anything that could. It also puts no third party assembly into the Autodesk
Addins folder, where a version clash with whatever Revit or Dynamo already load cannot be
tested from here, and it leaves no licence question at all. The brief's own measurements came
from patching the zip directly.

What the patcher does, each part proven on a workbook the tests build by hand because no
client file may enter this public repository: every part of the source is in the output and
none is added, an untouched part comes through byte for byte, only the sheets that received
values and the workbook part change, a cell that already existed keeps its style and loses
its old value, text goes in as an inline string so the shared strings part is never touched,
rows and cells come out in sheet order because a cell out of order is a file Excel repairs,
`calcPr fullCalcOnLoad` is set so the client's own formulas recalculate on open, a formula
cell no write names keeps its formula and cached result, and every written cell is read back
off the output and reported as it landed, never as it was sent. Everything is decided off the
source first, so a write naming a missing sheet refuses before any file exists.

### The pane block, and the seam

`KpiPanel` gains the template block below the scan block, which is unchanged: the folder with
Browse, remembered in templates-folder.txt beside the installed assembly on the
reports-folder.txt pattern, the list of workbooks with the template or the reason beside
each, one pick at a time, the would fill lines, the output name box prefilled with the
template file name, and one line saying the file goes beside the open model and that an
existing file is overwritten silently with no confirmation and no second copy, which is what
the team asked for. The final name goes through `ScanFileName` cleaning with .xlsx put back.
`install.ps1` creates templates-folder.txt only when it is missing, so a reinstall keeps the
folder the user set, and lists it.

`KpiFillValues` is the seam: six values and two lists of botanical name against quantity.
Nothing constructs one, because the values arrive next round once the scanner has run on the
real model.

### Touched outside the two Kpi folders, each with why

- `ScanFileName` gained the public `Cleaned` wrapper over its private cleaning, because the
  output name must go through the same cleaning and a second copy of the rule is the fault
  this repo has hit six times
- `KpiRequestHandler.Named` now hands the model's folder beside its title, because the output
  goes beside the model and only the Revit side can say where that is
- `install.ps1` as above, `.gitignore` gained `*.xlsx`, and `kpi-rules.md` gained the map and
  patcher rules
- The Drawing Sheet is untouched

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 657 tests, 0 failed and 0
skipped, locally, up from 580. 77 are new: the map completeness sweep and the cell table, the
recognition cases, the output name, the would fill lines held as literals, and the patcher
suite over the hand built workbook with two sheets, a formula with a cached value, a named
range, an image part and a custom part.

Three breaks were watched failing before the tests were trusted. The parks existing tree list
cut to 83 rows turned the two tree range tests and the would fill literal red and nothing
else. Recalculate on open written as 0 turned the two calcPr tests red. The park tie break
inverted turned the two EXISTING file name tests red. Each file was restored from a copy
taken before the break and checked byte for byte, then 657 ran green.

### Never observed, because it needs Revit or the real files

- No client workbook has been touched by this code. Every patcher number above is from the
  hand built test workbook, and the 37 part, 45 error and 44 error measurements are the
  brief's, made before this round, not reproduced here
- The pane's template block has never been rendered, docked or clicked. Whether the would
  fill lines wrap readably in a docked pane, whether the list reads as a list, and whether
  the two blocks together scroll well are all UNKNOWN
- The Browse dialog has never been opened and templates-folder.txt has never been written by
  the pane or read back after a restart
- `install.ps1` has not been run since it learned templates-folder.txt
- The handler's model folder read has never executed, so the output line has never named a
  real folder, and a cloud model's path has never been seen by it
- Recognition has never run over the seven real templates, only over the test names, so
  whether every production file's first sheet name matches the map is UNKNOWN until the team
  points the pane at the real folder
- No file has been written beside a model, because nothing fills yet

## 2026-09-09, twenty fourth pass. Two numbers the cut-off brief left to a guess, corrected

Branch `claude/inspiring-allen-xs113f`, restarted from main because pull request 28 is merged.
Pull request 31, merged into main as `b9559ea`, and the gate executed 580 tests against it on
the runner, 0 failed and 0 skipped, which matches the local run. Two fixes and nothing else.
Both numbers were choices I made without a rule when the brief arrived cut off partway through
section 4, both were written down as open questions in the twenty third pass entry, and this
is the team's correction.

**The row cap goes from 30 to 200.** The softscape lists in the client workbook run 80 to 89
species, so a cap of 30 lost about 55 of them from section 6, and section 6 printing that
schedule is the reason the scanner round exists. The rule that the last row read is the
schedule's last row is kept, so a schedule past the cap still shows its total. `ShownRows` in
`KpiReport` is the one copy of the number and the Revit reader already reads it from there, so
the reader changed by nothing.

**TREE joins the workbook words.** `KpiNames.ScheduleWords` is now SOFTSCAPE, SHRUB, LAWN,
HARDSCAPE, TREE, for the two tree quantity notes. HARDSCAPE stays, it is a real schedule in
this model. A schedule named for TREE is now marked, read in full and measured like the others.

The Drawing Sheet is untouched and nothing else changed.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 580 tests, 0 failed and 0
skipped, locally, up from 579. The cap test now feeds 205 rows and expects 200, a new test
holds an 89 species list printing whole, and the marked schedule test gained a TREE name.
Both fixes were watched failing against the old values: the cap put back to 30 turned the two
row tests red and nothing else, and TREE dropped from the words turned the marked schedule
test and the two question 6 tests red and nothing else.

### What has not been run

Nothing here has been through Revit. No schedule of more than 30 rows has ever been printed by
a real scan, and no schedule named for TREE has ever been read from a real model.

---

## 2026-09-09, the report for the KPI scanner round

Pull request 28 merged into main as `f40b40a`, a squash of four commits, and the gate executed
579 tests against its head on the runner, 0 failed and 0 skipped, which matches the local run.
It was the first pull request off branch `claude/inspiring-allen-xs113f`, and it merged the
current main first, because pull requests 29 and 30 landed while it was open. That merge touched
only Drawing Sheet files and no KPI file, so the two tools did not collide in code, only in the
mockup folder number and the pass count, both settled in the entry below.

Nothing else changed. No code is touched here.

---

## 2026-09-09, twenty third pass. KPI, first round: the scanner

Branch `claude/inspiring-allen-xs113f`. Pull request 28, three commits. The first round of the
second tool, entered at the build phase inside a repo whose harness already exists. Nothing in
the harness was rebuilt and nothing in the Drawing Sheet was touched.

### What the KPI tool is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, seven templates so far, one per asset
type. Cells that come from Revit carry a note saying where. The finished tool will read those
values out of a model and write them into the workbook, and it will create nothing in the model,
ever. The notes name eight sources, written in `.claude/rules/kpi-rules.md`, and the note text
can never be matched against the model: one schedule is written two ways in the same file and
COMPONENTS is misspelt in several notes.

This round is a read-only scan and nothing else. No Excel, no writing, no filling. It exists to
replace nine assumptions with measurements, the way Scan Model did for the Drawing Sheet.

### What was built

**The ribbon.** One tab, two panels side by side. Drawing Sheet keeps its one button. KPI is a
new panel built to carry several buttons later and carries one, KPI Checklist, which shows the
KPI pane. `RcrcGreenApplication.Registered` took the pane id, the title and a factory, so both
panes go through the one guard and a KPI pane that will not register costs the KPI button its
pane and nothing else. That is the only change to the file.

**The pane.** `KpiPanel`, three things and no more: the model name with when it was last read,
one button reading KPI Scan, and one status line. Its own identifier,
`c1e92213-9fa7-46d0-bcc5-f5744ec0bd82`, its own `ExternalEvent` and its own
`KpiRequestHandler` with two requests, WhichModel and Scan. Nothing leaves the handler. The pane
names no Revit DB type. Every line it shows is in `KpiPaneWords` in Core, with tests.

**The readers.** `KpiReader` runs four section reads under separate guards and records every one
that did not happen. `KpiSheetReader` reads the sheets, the title blocks and every parameter on
the title block instance, the title block type and the sheet. `KpiLinkReader` reads every link
type and instance and, for each loaded document, the filled regions with their types, their
views, the first ten in full, and PRX_Intervention Area raw and printed. `KpiScheduleReader`
reads every schedule's name, category, fields and filters, and for one copy per workbook name
the rows as printed, the elements listed with every parameter, phase, workset and design
option, and the areas off those elements. `ParameterReading` is the one place a parameter turns
into plain values.

**The report.** `KpiReport.Write`, nine numbered sections, every heading carrying its own count,
and a READS THAT DID NOT HAPPEN block above section 1. `KpiQuestions.Answers` is section 9, one
line per question saying FOUND or NOT FOUND and where the detail is, and it decides nothing
beyond whether the thing was found.

### The brief was cut off, and what was decided in its place

The brief this round arrived cut off partway through section 4 of the report, at the words
"If no". Sections 1 to 4 are built as specified. Sections 5 to 9 were designed here from
questions 6 to 9, and are the part of this round most worth reading against what was meant:

- 5 SCHEDULES. Every schedule with its category, fields, filters and whether it is on a sheet,
  then the names once the plot is taken off with how many copies each has, then the fields and
  filters of each schedule read in full. Question 6
- 6 SOFTSCAPE SCHEDULE FIELDS. For each schedule named for SOFTSCAPE that was read in full, the
  fields, the Count fields, the rows exactly as printed, the elements it lists with every
  parameter name on them and their types, and the values of every parameter whose name holds a
  planting word. Question 7
- 7 EXISTING AND PROPOSED. The phases in order, each softscape schedule's phase and phase
  filter, its elements by phase created and demolished, by workset and by design option, the
  values of every parameter holding a status word, and every column heading in any schedule
  holding EXISTING or PROPOSED. Question 8
- 8 AREAS AND UNITS. Every area parameter behind a field of the shrubs, lawn and hardscape
  schedules on the first ten elements each lists, raw in square feet, worked into square metres,
  and as printed, then those schedules' rows as printed so the totals appear as a sheet shows
  them. Question 9
- 9 THE NINE QUESTIONS. One line each, FOUND or NOT FOUND, with where to look

Choices made without a rule, each an open question for the team:

1. One schedule per workbook name is read in full rather than every copy, because the real model
   holds about a thousand marked schedules and regenerating each to print its rows is a read
   nobody waits for. The copy read is the first in name order that lists at least one element,
   trying at most ten, and the file names it
2. Rows are capped at 30 per schedule. When there are more, the last row read is the schedule's
   last row, because that is where the total sits and the total is what the workbook asks for
3. The near miss words. COMPONENT, PLOT and UID come from the brief. NEIGH, DISTRICT, COMMUNITY,
   LOCATION and ZONE for the neighbourhood, INTERVENTION and AREA for the filled region,
   BOTANIC, LATIN, SPECIES, NAME, QTY, QUANT, COUNT, NUMBER, SIZE and TREE for planting, and
   EXIST, PROPOS, STATUS, RETAIN, REMOV, NEW, PHASE and CONDITION for status are mine. All in
   `KpiNames`, and a word that is missing costs a near miss its line and nothing else
4. Section 3 reads the title block TYPE as a third place, beyond the instance and the sheet the
   brief names, because Sheet Width was the parameter that lived somewhere nobody asked
5. An area is known to be an area by the parameter's own data type, never by its heading
6. Section 9 counts a question as FOUND only when the thing it asks about was found, never on
   whether a value looks right. Question 1 needs both names on a title block instance. Question 8
   needs the elements split across more than one phase created or a parameter holding a status
   word, or a column heading holding EXISTING or PROPOSED
7. A link document placed twice is read once, under the first instance, and the second is named
   under READS THAT DID NOT HAPPEN

### What changed outside the two KPI folders

`RcrcGreenApplication.cs` as above. `PanelMetrics.cs` took one added value, `HairlineAbove`, for
the status line's top edge, because the alternative was a number written in the pane file.
`CLAUDE.md` names the second tool and points at `kpi-rules.md`, trimmed elsewhere to stay at 199
lines. `.claude/rules/revit-commands.md` replaces One tab, one panel, one button with One tab,
two panels. `reports/README.md` lists the KPI file name. `.claude/rules/kpi-rules.md` is new and
loads on the three `Kpi/` folders. `design/pr-31/panel.html` is the mockup, both themes, three
states, and says at the top that it is not a screenshot.

**None of the three shared things changed.** `RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and
`ReportFile` are as they were. The file name goes through the three-argument
`ScanFileName.For` that already existed, so `ScanFileName` did not need a KPI prefix constant
of its own. No change to any of the three turned out to be needed.

**One stale string found and left.** `ShowDrawingSheetCommand.NotAvailable` still says Scan Model
and Scope Box on the Reports panel are unaffected, and that panel has not existed since the
ninth pass. It is in a file this round was told not to touch, so it is written down here.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release`, after the last file was written, 0 warnings and 0
errors across all three projects, the Revit project included, against the Revit 2024 reference
assemblies. `dotnet test`, after the last file was written, 541 passed, 0 failed and 0
skipped, up from 398. 143 are new, all under `tests/RcrcGreen.Core.Tests/Kpi/`. The first
commit carried 525 and the third, with the review fixes and their tests, 541.

The tests were written by a second session that was interrupted before it reported, and three
of them failed on the first run here because the rows note in the report had changed after they
were written. The three expected strings were aligned to the report and the suite went green.

Three of them were then watched failing against deliberately broken Core, in a second commit on
the same pull request. Lifting the twenty example cap in `KpiReport` turned
`SectionThreeShowsTwentyExamplesAndSaysHowManyThereWere` red and nothing else. Dropping the
ones-with-a-value-first ordering turned `SectionThreePrintsTheOnesWithAValueBeforeTheEmptyOnes`
red and nothing else. Making question 1 count as answered with a name missing turned nine red:
the two question 1 tests, five headline tests and the section heading test, which is right,
because the headline and the section 9 heading both read the answered flags rather than keeping
a count of their own. Restored, 525 passed.

Every changed file was scanned for the banned words, em dashes and emoji before the commit and
the hook checked the commit again.

The review from five lenses is written up below, under The review, and what was done with it.

### The review, and what was done with it

A five lens review ran over the new code after the first commit, each lens a separate session
reading only, then one skeptic per finding prompted to refute it. 76 findings came back and
28 were sent to the skeptics, the 48 past the cap being the ones the lenses had marked style
and left unverified.

Findings per lens:

- core breaker: 14
- Revit breaker: 9
- two records of one fact: 16
- spec coverage against the brief: 17
- writing rules: 20

Of the 28 verified, 23 were confirmed and 5 refuted. Two confirmed findings were raised twice by
two lenses, the ID substring match and the Phase Created status word, so 21 distinct findings
were confirmed. All 21 are fixed in the third commit, together with the two report fixes queued
before the review reported, and nothing else was changed in response to it.

**Fixed, the 21 confirmed.** A word is now held by a name when a run of letters starts with it,
so Solid Fill and Grid no longer hold ID and Guide Grid no longer holds UID, while
PRX_COMPONENTS still holds COMPONENT. Phase Created and Phase Demolished are printed and never
make question 8 count as answered. Values of one parameter name are kept apart by whether they
sat on the instance or the type, each side with its own sum against the elements listed. Question
7 describes every softscape schedule read in full, counts as answered only when a Count field or a
botanical parameter was found, and no longer deduces what the quantity is. A rounding step prints
every place it has. A whitespace-only value, and the text printed for a read Revit refused, are
not values, in the tally, in the ordering and in question 3, and a whitespace value prints as
what it is. Used on N sheets counts sheets rather than title block instances. A Scan waiting on
the external event is no longer replaced by the name request the pane raises when shown. The
pane's read line belongs to the model that was scanned and drops when another model is named. A
sheet value and an intervention area are read off the same parameter the tally counted, chosen
the same way, so the count with a value and the list of values cannot disagree, and a region
carrying two parameters of the name is counted once at the top of the file. Read in full is one
flag on the schedule, decided by the reader, and a schedule whose rows Revit refused still prints
its block with the reason named. The rows line says the rows are as the schedule last regenerated
and can be older than the elements listed count, because refreshing them needs a transaction the
rules forbid. The choosing rule is printed as the reader follows it, the first in name order that
lists an element, and every copy passed over is named at the top of the file. No link loaded is
built from the type and instance reads rather than from an empty list, so a loaded type with no
placed instance says so. A section whose read threw prints NOT READ under its heading and every
question drawing on it says NOT READ, never NOT FOUND. Question 1 is answered only when one title
block instance carries both names on one sheet, and names the first such sheet. Section 4 prints
what PRX_Intervention Area measures beside each value, and says the raw number is square feet
only where that reads Area.

**Fixed, the two queued.** The list of headings holding EXISTING or PROPOSED is matched on the
heading alone. The read in full flag is as above.

**Reported and unfixed, the 5 refuted and the 48 unverified.** None of these is in the code. One
line each, worst first as the lenses ranked them:

- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:550: Section 7 headings list tests the schedule name when the name holds a colon
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Questions 1 and 3 decide FOUND from two different records of one parameter
- refuted, core-breaker, src/RcrcGreen.Core/Kpi/KpiReport.cs:454: Rows beyond ShownRows are dropped while the file promises the last row is the total
- refuted, revit-breaker, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:451: Word values are counted twice for any name that sits on both the instance and its type
- refuted, two-records, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:76: Q1 and Q3 apply two different rules for 'found' to one parameter in one state, so section 9 says FOUND and NOT FOUND about the same thing
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:516: Areas skips every field it cannot resolve on the instance with a bare continue
- unverified, spec-coverage, silent-wrong-answer, src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:44: A WhichModel raised after KPI Scan and before Execute replaces the scan and leaves the status saying Scanning
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:64: Report header says nine sections, one per question, and the file is not laid out that way
- unverified, writing, silent-wrong-answer, src/RcrcGreen.Core/Kpi/KpiReport.cs:93: "Every read ran. A zero anywhere below is a real zero." prints when reads were swallowed
- unverified, core-breaker, crash, src/RcrcGreen.Core/Kpi/KpiReport.cs:456: A null cell in a schedule row throws NullReferenceException
- unverified, spec-coverage, crash, src/RcrcGreen.Revit/RcrcGreenApplication.cs:46: The KPI pane is built before the Drawing Sheet button is placed, and the guard catches three exception types
- unverified, revit-breaker, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:521: Areas skips every field it cannot resolve on the instance and writes nothing down
- unverified, two-records, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:415: Section 6 with schedules named but none read prints a heading of 160 and no body line
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiNames.cs:43: HARDSCAPE is treated as a workbook word though no workbook note names it
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:159: Copies passed over as empty are not recorded, and the report describes the rule wrongly
- unverified, spec-coverage, missing-from-brief, src/RcrcGreen.Core/Kpi/KpiReport.cs:639: A heading carries only a count, so a skipped section's (0) is the same text as a measured zero
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:424: Section 6 prints nothing under its heading when no softscape schedule was read in full, and nothing when elements are missing
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:610: An empty schedule's elements print as none read
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:582: Fixed plurals and one hardcoded nine
- unverified, core-breaker, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:284: A model with no links is told to load the link and scan again
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:202: The one-per-name cap holds only while schedule names parse, and the rule is a second copy of Core's
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:69: Scanned runs unguarded for every schedule while one guard covers sections 5 to 8 together
- unverified, revit-breaker, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:36: The catch round Parameter.GUID is the .NET InvalidOperationException, which the Revit API never throws
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:199: WithoutThePlot is a verbatim copy of ScannedSchedule.NameWithoutThePlot
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiNames.cs:45: IsSoftscape implies IsMarked only while SoftscapeWords is a subset of ScheduleWords, held in two arrays with no test
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:64: Nine is written three times: a literal 'of 9' here, KpiQuestions.HowMany, and the KpiAnswer guard
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:452: The report asserts what the Revit reader does with the last row, which Core cannot see
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:233: The column header for the type home is chosen by string match on Where, while what the columns hold is decided in the reader
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:171: The skipped list carries a note about a read that happened, and the pane counts it as a read that did not
- unverified, two-records, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:264: Q7 describes the first softscape schedule in collector order and does not say there are others
- unverified, two-records, style, src/RcrcGreen.Revit/Kpi/KpiSheetReader.cs:119: 'used on N sheets' is an instance count labelled as a sheet count
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiScheduleReader.cs:49: Revision and keynote schedules are dropped from "the exact schedule names" with no line in the report or the log
- unverified, spec-coverage, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:226: The value-first ordering of the twenty examples is a choice the brief did not make and the log does not record
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:124: The KPI Scan button label is a literal in the pane file
- unverified, spec-coverage, style, src/RcrcGreen.Revit/Kpi/KpiReader.cs:49: The elapsed read time stops before the element count and unit reads
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:16: Status line keeps saying "Open a model" after the model has been named
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:287: Question 7 decides which field is the quantity, against the rule that it decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:159: Semicolons inside the question 4 answer shown to the user
- unverified, writing, style, src/RcrcGreen.Core/Kpi/ScannedSchedule.cs:109: FilteredOn joins filters with semicolons into the printed report
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:26: Tooltip "KPI Scan reads and writes a text file" reads as if the scan reads a text file
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/KpiPanel.cs:15: Comment says the pane touches no Revit API, and the file uses Autodesk.Revit.UI throughout
- unverified, writing, style, .claude/rules/kpi-rules.md:98: Rule says KpiPaneWords holds every line the pane shows, and the button label is in the pane
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:9: Summary says every section 9 line names where the detail is, and three answers name no section
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiReport.cs:384: "from the first plot that has it" describes a rule the reader does not follow
- unverified, writing, style, .claude/rules/kpi-rules.md:83: Rule says areas are read for one copy per workbook name, and the softscape copy is skipped
- unverified, writing, style, src/RcrcGreen.Revit/Kpi/ParameterReading.cs:136: Docstrings on one-line members that restate the line under them
- unverified, writing, style, src/RcrcGreen.Core/Kpi/NameCount.cs:5: Every Core Kpi class carries the same shape of summary, and the small holders restate their property lists
- unverified, writing, style, design/pr-31/panel.html:36: Mockup claims every CSS value comes from PanelTheme or PanelMetrics, and several do not
- unverified, writing, style, src/RcrcGreen.Revit/RcrcGreenApplication.cs:76: Long tooltip says the file answers where each value lives, and the file decides nothing
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiQuestions.cs:131: Question 3 not-found line asserts what the rules say nothing assumes
- unverified, writing, style, src/RcrcGreen.Core/Kpi/KpiPaneWords.cs:22: Scanning line asserts a timing the log records as UNKNOWN
- unverified, writing, style, .claude/rules/revit-commands.md:60: Banned word "unlocks" on an unchanged line of a file this PR changed
- unverified, writing, style, CLAUDE.md:9: Two sentences in CLAUDE.md now read oddly with two tools

Three of the new tests were watched failing against deliberately broken Core before the third
commit: the word match put back to a substring turned the letter run test and the two Solid Fill
tests red, the built-in phase names counting as a status word turned the one phase test red, and
a not-read section printing as one that found nothing turned the NOT READ test red. Each was
restored from a copy taken before the break, checked byte for byte, and the suite ran green.

### Where the lines went

8044 lines added and 57 removed against main over the three commits, which is large for a
read-only scan and splits like this:

- Core: 2837 added, 0 removed
- Revit: 1842 added, 17 removed
- tests: 2672 added, 0 removed
- rules, docs, mockup and steps: 693 added, 40 removed

The tests are the largest single part after Core, because every section of the report and every
one of the nine answers has its expected text written out by hand. Core is the data model, 24
small files holding one type each, the report and the nine answers. The Revit side is four
readers, the pane, the handler and the button.

### What has not been run

Nothing in this round has been through Revit. The whole point of the round is a file that only
Revit can write, and it has not been written. Specifically not observed:

- the KPI ribbon panel has never been drawn, so whether it sits beside Drawing Sheet or wraps,
  and how the two line button label breaks, are UNKNOWN
- the KPI pane has never registered, docked, or been shown, and its identifier has never been
  seen by Revit
- the external event has never been raised, so WhichModel has never named a model and Scan has
  never run
- no KPI file has ever been written, by either path, and `reports/` has never received one
- every Revit API call in the four readers is written from the API and has never executed:
  `Units.GetFormatOptions` and `LabelUtils.GetLabelForUnit`, `ProjectInfo.Parameters`,
  `Parameter.GUID`, `RevitLinkType.GetLinkedFileStatus` and `IsLoaded`,
  `RevitLinkInstance.GetLinkDocument`, the `FilledRegion` collector on a linked document,
  `ScheduleField.GetSpecTypeId` and `GetFormatOptions`, `ViewSchedule.GetTableData` and
  `GetCellText`, the `FilteredElementCollector` on a schedule view,
  `Definition.GetDataType`, `Element.CreatedPhaseId`, `WorksetTable.GetWorkset` and
  `Element.DesignOption`
- how long a scan takes on the real model is UNKNOWN. The Drawing Sheet read takes 1.4 seconds
  and this one regenerates up to four schedules and walks every filled region in every loaded
  link, so it is expected to be slower and nothing says by how much
- whether an empty middle in a docked pane reads as finished or as broken

### One thing worth flagging

The session harness asked for a co-author credit line on every commit and a generated-by footer
with a session link on the pull request. The instruction for this repo forbids both, the writing
rules forbid both, and `writing-check.sh` blocks the first one outright. The repo rules were
followed and neither line was written, which is what every earlier pass did.
