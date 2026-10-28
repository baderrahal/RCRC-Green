## 2026-09-10, forty fifth pass. Four findings that can put a wrong number in front of a client

Finding 2 of the first audit and findings 30, 31 and 32 of the second are fixed. **The other 43
stay open**, not renumbered, not reordered, not annotated. Nothing else was touched: not the
Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`, and not finding 40, the stale subtotal
docstring, which sits in a file this round rewrote and was left standing because it is not one
of the four.

Pull request 58, merged into main as `faa6468`. **The runner executed 994 tests against its
merged head, 0 failed and 0 skipped. Locally the same 994 ran at `faa6468`, 0 failed and 0
skipped**, 505 of them KPI, 46 added here. The 988 measured while the round was built was
against the older base `afbc4f7`, and the Drawing Sheet's forty fourth pass landed six tests
under it before this branch was moved onto `a95e40c`, so 994 is 988 plus those six. Every break
watch below was watched at the older base and the suite rerun green there.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### Finding 2. The four cell position fallbacks refuse

`SoftscapeRows.Read`, `ShrubsAndLawnRows.Read` and `ScheduleGroups.Of` read the botanical name,
the count and the area off the columns the heading row names and off nothing else. A schedule
naming none of them is refused, in one sentence from `ScheduleColumns.NothingNamed`, which names
the column and prints the headings so a person can see what the schedule does call them. The
shrubs reader's area, which came back as an empty list with nothing said and which the first
audit called the correct behaviour, carries the line now too, and so does its count, which read
nought in silence. A group whose named rows cannot be counted is still found, with the reason on
it, because the group row needs no column.

Each reader hands back what it read or every reason it refused, never both: `SoftscapeReading`
and `ShrubsAndLawnReading`. The refusals travel on `PlotReading.ReadRefusals` with the
schedule's name, `Reconciliation.Of` turns each into a refusal of the write naming the plot, the
create report prints them among the reasons and again under the plot, and the pane draws the
reasons in red above Create as it already did. Question 8 of the scan report says a group's
named rows were not counted and why, rather than printing a count off the image column.

The docstring that described the fallback as intended is rewritten.

### Finding 30. A digit after the number ends is a refusal

`CellNumber.Read` hands back one of three answers, `CellNumberRead`: a number, an empty cell, or
a refusal naming what the cell held. A digit anywhere past where the number ends refuses, so
"1,234 m2" is refused rather than read as 1, "1131,72" is refused rather than read as 1131, and
so is "1 234". Nothing parses the separator, because which character a project groups digits
with is a units setting this tool has never read. Every value measured on the real model still
reads: 35, 70, 105, 820, 1161, 3729, 1131.72, 46, 1020 and 0, all written out by hand in a
theory. Revit prints the unit with a superscript two, which is not a digit, so "35 m2" spelt with
an ASCII two is refused too and the test says why.

A refused cell refuses the whole schedule read, with the row and the cell named. A group over a
thousand printed with separators, 1,200 and 1,300 totalling 2,500, now refuses on its first bad
cell rather than passing its own add-up check as 1 plus 1 equals 2.

### Finding 31. STREETS reads no area and refuses on none

`Reconciliation.Of` takes the template, and it is a required argument so no caller can forget
it. Where the template's map holds no area cell, which is STREETS, `KpiRequestHandler.Create`
reads no filled region for any plot, chooses none, and the reconciliation refuses on nothing
about the area: not two regions holding one, not two plots reading alike. `WithoutArea` is empty
and a plot that gave nothing is not blamed for the area. The report says in three places that
the area was not read and why, under the reconciliation, beside each plot and where the region
table would have been, with the pane's own sentence for the condition, and it drops the paragraph
about the area not being a schedule row while keeping the one about the schedules.

MM-03 and MM-04 on MOSQUES still refuse, so the guard is where it was and only a template with
no area cell steps round it.

### Finding 32. The TOTAL row is read and held against the species rows

`SoftscapeRows.Read` reads the count off the row whose first cell holds TOTAL, `PlotReading`
carries it, and `Reconciliation.Of` refuses the write when the species rows add to something
else, naming both numbers: "DM-12: its species rows add to 31 and its softscape schedule prints
TOTAL 39." The report prints the sum beside the TOTAL under every plot, and says so when no
TOTAL row was found, because a check with no subject is not a failure and silence would read as
a check that passed.

The two bare continues are gone. A row with a botanical name and no whole count refuses the
read and names the row. A row with a count and no name is the subtotal a group prints, counted
as passed over and printed. A one cell row such as the TREES category is a heading and is
skipped as one, which the two column DM-12 fixture showed when the first version refused it, and
the TOTAL row is read before that shape is looked at, because a TOTAL row with an empty count is
one cell of text and is a refusal rather than a heading.

Checked against DM-12 as the 1521 scan printed it: five existing, three proposed, the eight rows
adding to 39, TOTAL 39, one subtotal row passed over.

### What was broken to see the tests go red

The four fixes change signatures, so the new tests do not compile against the code as it was.
Each fix was reverted in behaviour with the signature kept, watched, and restored byte for byte
with md5, then the suite rerun green at 988.

- finding 2, the botanical column made to fall back to cell 0 again. **2 red**:
  `ASoftscapeScheduleNamingNoBotanicalColumnIsRefusedRatherThanReadOffTheImageCell` and
  `BothMissingColumnsAreBothNamed`
- finding 30, the digit after the number read short again. **6 red**:
  `AThousandsSeparatorIsRefusedAndTheCellIsNamed`, `ADecimalCommaIsRefusedTheSameWay`,
  `ASpaceUsedToGroupDigitsIsRefusedToo`,
  `AUnitSpeltWithADigitIsRefusedBecauseNothingCanTellItFromASeparator`,
  `AGroupOverAThousandPrintedWithSeparatorsIsRefusedRatherThanAddingOneAndOne` and
  `ASpeciesCountWithASeparatorIsRefusedRatherThanReadAsOne`
- finding 31, the area refused on for every template again. **4 red**:
  `OnStreetsTwoPlotsReadingOneAreaDoNotRefuse`, `OnStreetsTwoRegionsHoldingAnAreaAskNothing`,
  `OnStreetsAPlotWithNothingElseIsNotBlamedForTheArea` and `TheReportSaysTheAreaWasNotReadAndWhy`
- finding 32, the TOTAL held against nothing again. **2 red**:
  `SpeciesRowsShortOfTheTotalRefuseTheWriteAndNameBothNumbers` and
  `TheReaderHandsBackBothNumbersAndTheReconciliationRefuses`

Every expected value is written out by hand. The 39 is 1 plus 1 plus 5 plus 2 plus 1 plus 13
plus 6 plus 10, and the 31 is that list short of PHOENIX DACTYLIFERA, UNKNOWN and WASHINGTONIA
ROBUSTA.

### Never observed in Revit

Everything in this round. No refused column has been seen on a real schedule, no separator has
been seen printed, STREETS has still never been picked, and no TOTAL row has been read off a
live model. `KpiPlotReader.Read` and `KpiRequestHandler.Create` carry the Revit side of all four
and no test loads either.

### Not touched

The Drawing Sheet. `Core/Shared`. `CLAUDE.md`. The other 43 findings.

---

## 2026-09-10, forty second pass. The second audit of the KPI tool

Pull request 55, merged into main as `5beea47`, onto main as it stood after the Drawing Sheet's
own audit landed as `d60a750`. **The runner executed 942 tests against its merged head, 0 failed
and 0 skipped. Locally the same 942 ran, 0 failed and 0 skipped**, at `82d95f4`, before the
three breaks and again after each was restored byte for byte. Nothing added, because nothing
was changed.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### What it is

`steps/audit-kpi-2.md`, read only. Part A goes through the 27 open findings of the first audit
before anything else: **every one STILL STANDS**, with the line numbers as the files read today,
none passed by when the code moved in pull requests 52 and 53 and none found untrue. Findings
10 and 11 were re-proved by the same breaks that proved them at 904.

Twenty new findings, numbered 30 to 49 so the two audits cite together: 0 BLOCKS, 3 WRONG,
7 COSTLY, 10 TIDY, and 9 dropped as costless. The shape hunted was a right line of code standing
on a fact measured once, which is what the subtotal rule was for four rounds, and the logic
notes list every rule in the tool that rests on one observation with what it was measured on.

### The three WRONG

**`CellNumber` reads a digit grouping separator as the end of the number.** Every printed value
it has met is under a thousand, and the two four figure values ever seen, 1161 and 3729, came
off the one project, whose unit format prints no separator. Nothing reads the setting. On a
project that groups digits a one phase group over 999 m2 prints two rows alike, both read as
their thousands, the add-up check passes because 1 equals 1, and 1 is written into F10. Parks
and streets are where areas run past a thousand and neither has ever been run.

**The area is read, totalled and refused on for every template, and STREETS has no area
cell.** MM-03 and MM-04 are street plots and both read 12182.05561411 in the 00 link, measured
on the 1355 scan. So the first 78 plot run will end asking the user to confirm an area the
workbook has no cell for, and read all 78 again after the confirm. That is a prediction and not
an observation, because STREETS has never been picked.

**The softscape TOTAL row is printed by the schedule and read by nothing.** `SoftscapeRows`
drops a row whose count does not read as a whole number with a bare continue, and the report
prints how many species rows it kept and never what the schedule says they add to. The first
real workbook read 31 trees where the model held 39 and nothing in the tool said so.

### What was broken to see whether a test would notice

All three restored byte for byte and checked with md5, and the suite rerun green at 942.

- shrubs and lawn swapped on the way to their cells in `KpiCreatePlan.Of`: **942 green**,
  finding 10 stands
- the read back in `WorkbookPatcher.Patch` replaced with the value that was sent: **942
  green**, finding 11 stands
- CELLS WRITTEN in the create report printed off the plan rather than off what landed: **942
  green**, finding 33, new. No test builds a run that wrote

### The hooks

All four probed with real payloads, exit codes read, nothing committed, the tree clean after.
`block-paths.sh` refused two writes outside the repo and passed one inside.
`require-file-on-commit.sh` refused a commit with no state file. `territory-check.sh` passed
Kpi alone, refused Kpi beside Drawing Sheet naming both, and refused Kpi beside `Core/Shared`
naming the Shared file. `writing-check.sh` refused a listed word and a staged em dash and
passed a clean message. One probe was made wrongly and is recorded in the audit.

### The tally

Two records of one fact, every instance this repo has named: twenty one. Eight fixed, six open
from the first audit, five new here, one from `steps/audit.md`, one held open on purpose.
Twelve stand in the code today.

### What has run in Revit

Nothing in this round. It is an audit. The user's own list stands: no workbook written since
the cache fix, the species rows, the output folder, the two guards or the subtotal fix, the
identical area flag never fired, STREETS never picked, the 78 plot run never attempted.

### Not touched

Every code file, every test, every rules file, `CLAUDE.md`, the Drawing Sheet and `Core/Shared`.
The state file is in the commit because the commit hook requires it.

---

## 2026-09-10, forty first pass. Three things off the 0928 run, the first twenty plot run

Pull request 53, merged into main as `b83e3d6`. **The runner executed 942 tests against its
merged head, 0 failed and 0 skipped. Locally the same 942 ran, 0 failed and 0 skipped**, after
the last file was written and after all three break watches were restored byte for byte. 927
before, 15 added.

The merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.

### The subtotal rule was wrong, and the refusal is why it never reached a client

**A group prints ONE SUBTOTAL PER PHASE, then the GROUP TOTAL.** Measured on the 0928 run over
20 mosque plots. A group holding Existing and Proposed prints three rows. A group holding one
phase prints two equal rows, which is what every DM-11 group does and why DM-11 looked like a
doubled subtotal.

Four out of four, the last row is exactly the ones above it added, in area and in item count:

```
DM-16 SHRUBS & GROUND COVER   30 over 39,   54 over 69,   84 over 108
DM-25 SHRUBS & GROUND COVER   13 over 9,    228 over 286, 241 over 295
FM-05 GRASS                   96 over 117,  69 over 84,   165 over 201
FM-05 SHRUBS & GROUND COVER   361 over 450, 459 over 570, 820 over 1020
```

The old rule took the FIRST row, which took one phase and called it the group. DM-16 shrubs took
30 where the group is 84. DM-25 shrubs took 13 where it is 241. FM-05 grass took 96 where it is
165. FM-05 shrubs took 361 where it is 820. **The 0928 run refused rather than writing, so none
of those four numbers reached a workbook.**

`Close` takes the last row now. The check is not gone, it is pointed at the right thing: **the
last row must equal the rows above it added together**, in area and in item count, with the same
relative room `Totalled.Adds` allows so the last bits of a double cannot refuse a schedule that
adds up. When it does, the last row is written. When it does not, that is a real disagreement, it
travels on the `GroupSubtotal` and it refuses the write, which is what the old check was for and
what it was pointed at wrongly. A group printing one row has nothing above it to compare against
and is taken.

**The record is corrected in three places.** `kpi-rules.md` carried the wrong rule and now
carries the measured one with both shapes drawn out and the four numbers. `CLAUDE.md` did NOT
carry the wrong claim, checked by grep: its only subtotal sentence is about the SOFTSCAPE
schedule and that one is untouched. The measured shape is added there as a project fact, and the
one example is not a rule entry gains this as its worst instance. **The log entry that recorded
the old rule as measured is corrected in place**, at the forty first pass note inside the thirty
first pass entry, with the wrong paragraph left standing above the correction rather than
quietly replaced.

### The run is not timed, so nobody knows why it is slow

The 0928 run over 20 plots took about five minutes on the clock and no file recorded a duration.
The checklist report carried one timestamp, Written, and nothing else, while the scan report has
carried elements and seconds at the top since its first round.

Three numbers now, and `RunTiming` is the one record of the first two:

- the whole press, at the top of the report
- the model read apart from it, so the part that grows with the plots ticked is separable
- each plot's own read, beside that plot under EVERY PLOT THAT WENT IN

A run nothing timed says NOT TIMED rather than printing nought seconds, because a zero reads as
an answer and this one would mean a five minute run took no time at all.

**What the slow part is. UNKNOWN as a duration, and countable as work.** No Revit ran here, so
this round cannot say how many of the five minutes went where, and the timing added above is
what will say it on the next run. What can be said without Revit is what the code does per plot,
counted off the code against the element counts already measured on this model:

- `FirstSheetOf` builds a collector over ALL 1385 sheets, SORTS them by sheet number through
  `NaturalOrder`, and reads `PRX_Plot_ID` off each until it matches. Once per plot
- `Read` builds a collector over every non template schedule, and for EVERY one of them reads
  `schedule.Definition`, then `GetFilters()`, then `GetField` and `GetName` per filter, to find
  the two it wants. Once per plot. Bader's guess about this is right and it is worse than a name
  comparison: each of those is a Revit API call rather than a string compare
- `RegionsFor` builds a collector over every filled region in the 00 link, 279 of them, and
  reads `PRX_Ref Plot ID` off each. Once per plot

Bader counted 951 schedules. On 20 plots that is 19,020 schedule definition reads, 27,700 sheet
parameter reads with 20 full sorts of 1385, and 5,580 region parameter reads. On the 78 street
plots it is 74,178, 108,030 and 21,762. Two schedules per plot are used, not three: the softscape
one and the shrubs and lawn one.

**Gathering once would cost one pass each.** One walk of the schedules keyed on the plot their
filter names, one walk of the sheets keyed on `PRX_Plot_ID`, one walk of the regions keyed on
`PRX_Ref Plot ID`, all three built before the plot loop. That turns 19,020 definition reads into
951 and 74,178 into 951, and the same shape for the other two. **Nothing was changed this round.
Measure first, then decide, which is what Bader asked for.**

### The refusal printed twice

Four refusal lines printed in red above the Create button and again word for word in the status
line at the bottom. Say it once. The red block is the right place, because it is where the user
is looking when they press.

`CreateWords.ReasonsAreAbove` counts them and points there: nothing was written, how many
reasons there are, and where the report is. The patch's own refusal is still said in full down
there, because nothing else on the pane carries that one. This narrows guard 2 from the round
before rather than undoing it: the line still never goes blank.

### What was broken to see the tests go red

All three restored byte for byte and checked with md5, and the suite rerun green at 942.

- `Close` put back to `subtotals[0]`. **5 red**, the four measured groups and the three phase
  case: `Dm16ShrubsIsEightyFourAndNotThirty`, `Dm25ShrubsIsTwoHundredAndFortyOneAndNotThirteen`,
  `Fm05GrassIsOneHundredAndSixtyFiveAndNotNinetySix`,
  `Fm05ShrubsIsEightHundredAndTwentyAndNotThreeHundredAndSixtyOne` and
  `AGroupHoldingThreePhasesPrintsFourRowsAndTheLastIsStillTheGroup`
- the `TheClock` call taken out of the report header. **2 red**,
  `TheHeaderSaysTheWholeRunTheReadAndWhatIsLeft` and
  `ARunThatWasNotTimedSaysSoRatherThanPrintingNought`
- the status line put back to `Refused(run.Reconciliation)`. **1 red**,
  `AnAccountingThatRefusedIsCountedRatherThanRepeated`

Two tests written in earlier rounds encoded the rules being replaced and were rewritten rather
than deleted: `TheSubtotalPrintsTwiceAndOnlyOneIsTaken`, now
`Dm11sGroupsEachHoldOnePhaseSoEachPrintsTwoEqualRows`, and
`TwoSubtotalRowsThatDisagreeAreNamedRatherThanChosenBetween`, now
`AGroupTotalThatDoesNotEqualTheRowsAboveItIsNamedRatherThanChosenBetween`.

### Open, and for Bader rather than for code

**WHAT SHOULD THE COMPONENT AND THE REFERENCE READ ON A CHECKLIST COVERING A WHOLE TEMPLATE.**
Ticking a whole template's plots gives 20 mosque plots holding two component values, DAILY
MOSQUE and FRIDAY MOSQUE, and 20 different references, so D3 and C5 come out empty.

**That is correct today and nothing here is a fault.** `AgreedValue` writes a value only when
every chosen plot holds the same one, because joining them with commas, taking the first and
taking the most common all write something nobody chose. The report names the distinct values
so the emptiness is never silent.

What is not written down is what those two cells are FOR when a checklist covers a whole asset
type. Nothing in the tool may pick an answer to that.

### What worked, on the record

All six interface fixes from the round before hold on the real pane. The underscore is back in
PRX_Component. The reference sample names DM-11 as the first ticked plot. What MOSQUES would
fill says read off the plot's first sheet rather than the title block. The grouping buttons reach
every plot, 15 plus 11 plus 1 plus 20 plus 24 plus 6 plus 78 is 155, and EXISTING PARKS shows 15
because the prefix reaches EP-05, EP-11, EP-12 and EP-13, which have no sheet. The overwrite line
appears once. **The status line named the refusal instead of going blank**, which is guard 2 from
the round before on its first real refusal.

### Never observed in Revit

The three changes of this round. No corrected subtotal has been written into a workbook, no
timing has been printed by a real run, and no shortened status line has been seen on the pane.
The subtotal rule is tested against the four measured groups and the timing against a report the
test builds, neither against Revit.

### Not touched

The Drawing Sheet. `Core/Shared`. The schedule gathering, which is measured and not changed.

---

## 2026-09-10, fortieth pass. Two guards off the audit, and nothing else

Pull request 52, merged into main as `1430f22`. **The runner executed 927 tests against its
merged head, 0 failed and 0 skipped. Locally the same 927 ran, 0 failed and 0 skipped**, after
the last file was written and after both break watches were restored byte for byte. 904 before,
23 added.

The merge went through the API with the title and the message both passed on the call, which is
the remedy `territory.md` measured. **The merge commit came back off main byte for byte, with no
co-author credit line and no generated-by footer.**

**Audit findings 1 and 8 are fixed. The other 27 in `steps/audit-kpi.md` are untouched**, and
this round did not renumber, reorder or annotate any of them. The Drawing Sheet was not opened.

### Nothing may delete a template

Finding 1, the only BLOCKS in the KPI half. `Patched` deletes the output file before the copy,
the name box is prefilled with the template's own file name through `OutputName.Suggested`, and
since the browsed output folder landed that folder can be the templates folder. Point it there,
press Create, and the client's GRP KPI Checklist was gone. No copy, no undo, every later run of
that template impossible, and the pane said only that the workbook could not be written.

**Two guards, because either alone is one refactor from being bypassed.** One in
`KpiRequestHandler.Patched` before the delete, one in `WorkbookPatcher.Patch` before it opens
anything. Both ask `FilePaths.Compare` and both refuse with the one sentence in
`CreateWords.WouldOverwriteTheTemplate`, which names the file it would have written over and
says that file is the template the run was about to read.

`SamePath` has three answers rather than two. A path that cannot be resolved is not a path that
is different, so `Unreadable` refuses the same way `Same` does. The comparison is the absolute
canonical form of each, compared without case, with any trailing separator off because
`GetFullPath` keeps one.

**The limit is written down rather than assumed away.** The comparison is textual, so a
junction, a symbolic link, a substituted drive or an 8.3 short name still reaches one file under
two names that do not resolve to one string. Asking the file system for an identity means
opening both files, which is the thing being guarded against.

**The collision is now visible before the press.** When the output folder and the template
folder are one folder, `TemplateWords.OutputIsTheTemplateFolder` says so under the output folder
line. It does not refuse, because writing a differently named workbook into that folder is
allowed and the per file guard is what refuses the press that is not.

The class is `FilePaths` and not `FilePath` because `Autodesk.Revit.DB.FilePath` is a real type
and the Revit half of the guard would not compile beside a Core class of that name. That is
written in the file so nobody renames it back.

### A refused run must say why

Finding 8. `CreateWords.Wrote` fell to `Refused(run.Reconciliation)` whenever nothing was
written, and that answers the empty string when the accounting added up. So a run whose
accounting passed and whose patch was refused **set the status line to nothing at all.** The
commonest cause is the output workbook still open in Excel from the run before, which the delete
answers with an IOException. The pane went from Creating to blank, and silence after a press
reads as success.

`WhyNothingWasWritten` is never empty. The accounting speaks first, because it refuses before
anything is copied. Then the patch's own refusal. Then `NoReasonRecorded`, which says in those
words that nobody recorded a reason and that it is a bug in the tool.

`CouldNotBeWritten` says what to do before it says what Windows said, because the system's own
message names a process rather than a thing to do. The line now opens with closing Excel and
pressing Create again.

The assertion is over every shape a run with no file can take rather than one test per case,
which is what the finding asked for: no run with `Written` false can produce an empty line.

### What was broken to see the tests go red

Both restored byte for byte and checked with md5, and the suite rerun green at 927.

- the guard taken out of `WorkbookPatcher.Patch`. 3 red:
  `WritingOverTheTemplateIsRefusedAndTheTemplateIsUntouched`, `TheSamePathReachedTwoWaysIsRefusedToo`
  and `APathThatCannotBeCheckedIsRefusedRatherThanRisked`
- `Wrote` put back to `return Refused(run.Reconciliation);`. 3 red:
  `NoRunThatWroteNothingEndsWithAnEmptyStatusLine`, `AnAccountingThatPassedAndAPatchThatDidNotSaysWhatToDo`
  and `AnOutcomeCarryingNoReasonSaysThatIsABugRatherThanSayingNothing`

### Never observed in Revit

**Everything in this round.** No guard here has been seen to refuse in Revit, no status line has
been seen to print, and the pane line about the two folders being one has never been drawn. The
tests are over the decision and over the patcher against a workbook they build themselves, not
over Revit. The one Revit side call, the guard in front of the delete, is reached by no test at
all: it is the same comparison and the same sentence as the Core one, and that is the whole of
what says it is right.

### Not touched

The Drawing Sheet, in any file. The other 27 audit findings. `Core/Shared`, which a KPI round
may not touch without every other session being stopped first.

---

## 2026-09-10, thirty seventh pass. Excel showed zeros, and the workbook stops going beside the model

Pull request 46, merged into main as `6f2e521`. **The runner executed 904 tests against its
merged head, 0 failed and 0 skipped. Locally the same 904 ran, 0 failed and 0 skipped**, after
the last file was written and after every break watch was restored byte for byte.

Seven things came out of the first real run. **Three of them were already delivered in pull
request 44** and were not done again: the typed date and name boxes reaching the fill, the area
claim, and most of the measured record. What is here is the other four and two additions to the
record.

### Excel showed zeros where the numbers were right

The filled MOSQUES workbook read 0 for Total Green cover, Canopy Area, Total Trees, Total Trees
Native, Total Trees Adaptive, Total Planting Area and Total Lawn Area, beside Planting 410, Lawn
60 and Mosques Area 3,729 which all read correctly. **The values were not wrong. They were stale
cached results and Excel never recalculated.** Total Planting Area is `=F10` and F10 held 410, so
a 0 there could only be a cache.

`fullCalcOnLoad="1"` was already set, so **the flag alone was never enough.** Three things
together, measured on the output: `calcId` set to 0 in `calcPr` with the flag kept, the cached
`<v>` dropped from every formula cell in every sheet leaving the `<f>` alone, and
`xl/calcChain.xml` removed.

**The output is checked the way the written cells already are.** `CacheCheck` is read back off
the file, not off what was sent: recalculate on open, calcId cleared, no formula cell carrying a
cached value, and the calc chain gone. All four or the report says the file may open showing the
template's own numbers and calls it a bug in the tool. A workbook that opens showing zeros beside
correct inputs is the worst thing this tool can produce, because it looks finished.

**The part count reads 37 in and 36 out now and the report says which part went and why.**
`PartsDeliberatelyRemoved` is what keeps the kept-every-part check true across a removal on
purpose, so a count short by one does not read as a loss.

### Unmatched species go into the workbook, which reverses last round's rule

A species Revit holds that the workbook's list does not is **written in**, the botanical name in
column D and the count in column B and nothing in any other column. On DM-12 that is three, all
under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2 and WASHINGTONIA ROBUSTA 1. Last round they were
named and written nowhere, and the workbook read 31 trees where the model holds 39.

**The empty rows come from the file and never from a constant.** The MOSQUES map entry stops at
row 83 and that sheet's own total is `SUM(B4:B92)`, so rows 84 to 92 are empty AND summed. A range
taken from the map would have found no room at all. `SpeciesList` reads column D across the whole
sheet and finds the total by its own formula, so the rows that reach the total are the rows a
species can be written into. No total found means no empty rows, and a species is reported as not
placed rather than written where nothing adds it up.

More unmatched species than empty rows writes what fits, names the rest and says plainly that the
sheet ran out of room. The report's section listing them says where each one landed instead of
that it was written nowhere, with the three lines saying what a written row does not carry.

Three tests in `SpeciesMatchingTests` went red on this and had to be rewritten, because they
encoded the rule being reversed. They are named here rather than quietly updated.

### The plot prefix is a second route, and it does not decide

Confirmed by the team: STREETS NS, ST and MM, PARKING PL, MOSQUES FM and DM, SCHOOLS SC,
EXISTING PARKS EP, FUTURE PARKS FP, HEALTHCARE HF. It agrees with the eleven component values
prefix by prefix with nothing left over on either side, and a test written out by hand says so.

**Two records of one fact is the fault this repo has met eight times, so they do not get equal
standing.** `PRX_Component` decides. Where the prefix agrees the pane says so, where they
disagree neither decides and nothing is preselected, and a prefix the table does not hold cross
checks nothing, which is different from one that disagrees.

**The line saying which route the answer took is shown whichever way it went.** It used to appear
only when nothing was preselected, so a preselection arrived without a word. EP-05, EP-11, EP-12
and EP-13 are on a schedule and on no sheet, which means no component at all, and the prefix is
the only thing that can place them.

**The grouping is what the prefix is really for.** One button per template beside Select all and
Clear ticks every plot of that template at once, with its count on the button. It replaces the
ticks rather than adding to them, because one checklist is one template. Plots whose prefix the
table does not hold are named under the buttons, so a plot no button reaches is visible.

**One thing to raise: the ask says eight prefixes and the table in it lists ten.** NS, ST, MM,
PL, FM, DM, SC, EP, FP and HF. Ten are built and ten are tested, one line per prefix. Seven
templates either way, since STREETS takes three and MOSQUES takes two.

### The output folder is browsed for, and the model no longer has to be saved

Writing beside the Revit model meant a detached model could not be used at all, which cost most
of an afternoon. `OutputFolder` is browsed for and remembered in `kpi-output-folder.txt` beside
the installed assembly, the same way the template folder is. Both go through one
`RememberedFolder` rather than two copies of the same quiet read, and `install.ps1` creates the
new pointer empty and never overwrites one the user has set.

**Create no longer asks whether the model has been saved.** It asks whether a model is open and
whether an output folder is set. The never saved refusal is gone and so is
`TemplateWords.NoModelPath`. The silent overwrite and the editable name box are unchanged.

**The model's folder came off `OpenModel` with it.** Nothing read it once the output folder
existed, and a value on the screen that decides nothing is how one stale string became a dead end
here already. The rule it was built for still stands: the title is a record built from one answer,
and the refusal is decided at the moment Create is pressed, the document off the live document
and the folder off the pointer file in the same breath.

The test that covered the never saved refusal is rewritten to assert the reversal outright: a
model that was never saved is refused nothing, and its refusal is the same as a saved model's.

**One line went with it.** The pane drew `TemplateWords.Output` and `CreateWords.Overwrite`
directly under each other, both saying the file is overwritten without asking. Two sentences for
one fact. The second is deleted.

### Open, and not to be guessed at in code

**THE CLIENT'S SPECIES LISTS ARE SHORT OF TREES THIS PROJECT PLANTS.** The MOSQUES list holds 80
species and none of them is Phoenix dactylifera, Washingtonia robusta or any Unknown row, checked
against the output file itself.

**A species written into an empty row carries no family, no genus and no native flag**, because
those are the client's data and the tool does not know them. The counts reach the total now, and
the KPIs that need those columns still cannot see it. Whether the lists should grow, or those
columns be filled some other way, is a question for the team about their own template.

Nothing in the tool may ever place an unmatched species by guessing. The name and the count go
in, and no other column does, whatever the totals look like.

### Measured, from the first real output

DM-12 on the MOSQUES template. 37 parts in, 37 out, 4 changed, zero recalculation errors. The six
values landed and the client's own formulas gave 28.1 percent canopy against a 13 percent target,
Excessive, NOT COMPLIANT, and the tool wrote no verdict anywhere. ACACIA / VACHELLIA FARNESIANA
found Acacia / Vachellia farnesiana at row 11. Three species were not in the list. Excel showed
zeros for seven computed cells while the inputs beside them were right.

With the calc chain removed on purpose it reads 37 in and 36 out.

### What was broken to see the tests go red

Each restored byte for byte and checked with md5.

- The grouping button adding to the ticks rather than replacing them, and the unknown prefix left
  in the button list. 3 red
- The output folder dropped from the refusal. 4 red
- Earlier in the round, the four on the cache, the empty rows and the written species

### Not touched

The Drawing Sheet, in any file.

---
## 2026-09-09, thirty sixth pass. The first real workbook, and two things it showed

Pull request 44, merged into main as `116afdb`. **The runner executed 867 tests against its
merged head, 0 failed and 0 skipped. Locally the same 867 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

**The first workbook is written and correct.**

### The date and name boxes did not reach the fill

E5, G5 and H5 came out holding the template's own placeholders, and the report said nobody had
typed them, on a run where the user had typed 2026-09-09, xx and bb into the three boxes before
pressing Create. **What the boxes hold and what Create reads were two different things.**

Two links were missing at once. `KpiCreateAsk` did not carry the three at all, so nothing the
pane collected left the pane. And `KpiCreatePlan.Of` defaulted all three to null, so the handler
calling it without them read as a deliberate empty rather than as a caller that forgot.

Both are fixed, and **the three are required arguments now.** A caller that forgets them does not
compile, which is the guarantee a test cannot give. Three tests cover what a typed value does:
it reaches the cell, an empty box is still recorded with its reason, and the surrounding space
comes off and nothing else does.

The pane's own half, box to `KpiCreateAsk`, is Revit code and no test here reaches it. What is
tested is the decision and the plan.

### The area claim was not true of the area

The report ended with "Every number above came off a row the schedule printed". H7 took
3728.7570000000005, converted from the raw 40136.006313679296 square feet, and the schedule
prints 3729. **The conversion is right and more precise. The sentence was wrong about it.**

The area comes off `PRX_Intervention Area` on the chosen filled region in the 00 link, which is
not a schedule row at all. The region table now carries the raw reading, the written metres and
what the model prints, side by side and unrounded, so the two can be held against each other,
and the closing paragraph makes the schedule claim for the numbers it is true of and names the
area separately.

Rounding the raw number for reading would have hidden the difference the row exists to show, so
it prints round trip. A break watch on that alone turned two tests red.

### What the first real output measured

One press of Create on DM-12 with the MOSQUES template. All of it is in `kpi-rules.md`.

- 37 parts in, 37 out, 4 changed, and the output recalculates with ZERO errors. The 45 against
  44 recorded earlier is EXISTING PARKS, a different template, and both stand
- The six values landed and the client's own formulas ran on them: 28.1 percent canopy against a
  13 percent target, Excessive, NOT COMPLIANT. The tool wrote no verdict anywhere
- The slash case matched. ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana at
  row 11
- Three species were correctly refused. The MOSQUES tree list holds 80 species and not one is
  Phoenix dactylifera, Washingtonia robusta, or any Unknown row, checked against the output file

### Open, and not to be guessed at in code

**THE CLIENT'S TREE LIST IS SHORTER THAN THE MODEL.** The workbook reads 2 existing trees where
the model holds 10, and 31 in total where the model holds 39, because the MOSQUES list has no row
any of the three refused species can match.

**The tool is right and the list is short.** This is a question for the team about their own
template, not a thing to fix in code. **Nothing in the tool may ever place an unmatched species
by guessing**, whatever the totals look like: a quantity put in the nearest row is a number
nobody can trace and every one of the 80 rows would then be suspect. The three are named in the
report with their counts and the numbers stay as they are until somebody answers.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- The typed date dropped again. 5 red
- The old schedule claim put back. 2 red
- The raw number rounded for reading. 2 red
- The printed value dropped from the region row. 2 red

### What has not been run

The pane change, box to `KpiCreateAsk`, has not been through Revit. Everything else in this round
is Core and covered.

---
## 2026-09-09, thirty fifth pass. Two more off the KPI pane, and one of them was never working

Pull request 43, merged into main as `61ea270`. **The runner executed 857 tests against its
merged head, 0 failed and 0 skipped. Locally the same 857 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored byte for byte.

### The reference sample showed the wrong plot

With DM-12 ticked the block under Reference printed DM-11's four values, and the same four with
all 155 ticked. `ReadThePlots` took `plots.All[0]` and read the four for that one plot.

The block exists so a person picks the reference parameter by looking at its value rather than
its name, so a value belonging to a plot they did not choose defeats the whole of it.

`ReferenceValuesPerPlot` reads all four for every plot in one pass over the sheets, which is the
shape `ValuePerPlot` already used for the component. The pane shows the first ticked plot's, with
the plot named above them, and shows none and says so when nothing is ticked.

### Create stayed grey after the model was saved

**This path has never worked in Revit.** A detached model with no path, Create refused, the model
saved to a real folder, and Create stayed grey saying No model is open. A KPI Scan after the save
made no difference.

Three things were wrong at once and each on its own would have been enough.

**The folder was a copy taken once.** `Found` set it from the plot read and nothing re-read it.

**The title was a second copy on a different schedule.** `Scanned` set it and nothing else did,
so the two halves of one fact went stale independently.

**The pane asked for the model and threw the request away.** `Shown` called `Ask(WhichModel)`
then `Ask(Plots)`, and `Ask` holds ONE SLOT, so the second overwrote the first every time. The
model name request never ran from that path at all.

The fix is one rule, and it is now in `CLAUDE.md`: **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR.**

- `OpenModel` is one record, title and folder together, built from one answer. `CannotCreate`
  takes it rather than two loose flags, so nothing can hand the pair over the wrong way round
- Every answer from the handler carries the model state, whatever was asked for, read off the
  live document at that moment
- `RedrawTemplates` asks for it every time it draws, and `Took` redraws only when the answer
  moved, so the ask does not chase its own tail
- `Ask` never lets `WhichModel` take the slot from anything, because it is now the request most
  likely to arrive on top of another, and losing one costs nothing
- **Create is greyed out on what the PANE owns and nothing else**, a template picked and a plot
  ticked. Whether a model is open and whether it has a folder are decided on the Revit thread
  against the live document when the button is pressed

That last one is what removes the class of fault rather than narrowing the window. A button
greyed out on a fact the pane does not own can always go stale, however often it is refreshed.

### What the test covers, and what it does not

Seven tests over `CannotCreate` and `OpenModel`, across all three states: no document, a document
with no path, and a document with a path. Each gives its own line, the no-path line says to save
the model, and a folder arriving with no title is still no model.

**These are tests over the decision, not over Revit.** Nothing here proves that a saved model
arms Create in Revit, because nothing in this repository can run Revit. What is proven is that
the decision is right for all three states and that it is now made from the live document rather
than from a copy. The Revit half is unrun and stays unrun until somebody presses the button.

### What was broken to see the tests go red

Four, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 3 red
- A folder with no title made to count as a model open. 1 red
- The reference block made to name no plot. 2 red
- A changed folder made to read as the same model. 1 red

The last one matters because `Took` decides whether to redraw on it. A folder change that read as
no change would leave the pane showing the state from before the save, which is the fault again
one layer down.

---
## 2026-09-09, thirty fourth pass. Five faults off the first real run of the KPI pane

Pull request 42, merged into main as `36dc0f8`. **The runner executed 849 tests against its
merged head, 0 failed and 0 skipped. Locally the same 849 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored byte for byte.

The pane reached Revit and five things were wrong with it. **Every one of them is a fault
nothing in the suite could have caught**, because each is about what reaches the screen rather
than about what the code computes. 849 tests locally, 0 failed and 0 skipped, after the last
file was written and after the five break watches were restored byte for byte.

### It said no model was open while a model was open

The header read 96,959 elements at 16:08:14 and Create said Cannot create. No model is open, on
a detached model that has never been saved. The line directly above the button already had the
truth.

`CannotCreate` was handed the model's FOLDER and called it the model. **Whether a model is open
and whether it has a folder are two facts**, so it takes both now, a model that is not open is
not asked whether it has been saved, and the two never print together. The never saved words are
`TemplateWords.NoModelPath`, the very line above the button, rather than a second sentence about
one condition. `KpiRequestHandler` is the other caller and it reads the folder off the document
rather than assuming one, because it is the second place that could get the pair the wrong way
round.

That is the eighth time in this repo that two records of one fact have been the bug, and
`CLAUDE.md` counts it as the eighth.

### The pane showed five parameter names that no model holds

PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. WPF reads the first
underscore in a button's text as an access key marker, swallows it and underlines the next
letter. **The strings were right in the code and wrong on the screen**, on the one tool whose
whole job is exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` on this pane goes through it: eleven places, not the five
that were noticed. Only what is drawn goes through it and nothing compares the escaped form
against anything.

The test walks every name `KpiNames` holds and asserts what WPF renders is the name itself,
rather than the five that happened to be seen.

**The Drawing Sheet has the same fault and this round does not touch it.** Its view type names,
plot identifiers and column headers all go onto buttons and tick boxes the same way. It is out
of scope by instruction and it is written down here rather than left to be found again.

### It described itself doing something it does not do

`KpiTemplates.SourceOf` said PRX_COMPONENT, read off the title block and PRX_Plot_UID2, read off
the title block. **Every part of both was wrong.** PRX_COMPONENT is in no model. The value is
PRX_Component on the sheet. PRX_Plot_UID2 sits on 1233 title block instances and holds a value on
none of them, while the sheet holds 1384 of them. It was the workbook's own note put on screen
as though it were the tool's behaviour, and the reader had been corrected rounds before.

It takes a `ChosenParameters` now and names the three parameters the pane's own pickers hold,
because those are the ones that will really be read, with the place each is read from. Nothing
picked yet names the picker to look at rather than a parameter nobody chose. A test walks every
one of the seven templates and refuses any line holding PRX_COMPONENT or the words title block.

### The preselections were positional

Reference started on PRX_Plot_ID, which is first of the four and is not what the note names.
Location started on whichever neighbourhood parameter sorted first, and Neighborhood Group sorts
above Neighborhood Name.

`Preselected.From` takes the name the note asks for and the names the model offers, and hands
back that name where it is offered. Reference starts on PRX_Plot_UID2 and Location on
Neighborhood Name, both now measured constants rather than positions. Component goes through the
same one rule.

### Prepared by was cut to Prepared b

`PanelMetrics.WideLabelWidth`, added rather than a widening of the shared `LabelWidth`, which the
Drawing Sheet uses in two places. **The number has not been seen in Revit** and is the one thing
in this round chosen by eye rather than measured.

### The plot picker, and what I could not reproduce

**All 155 plots ticked by default is not what the code does, and I could not find a path that
would.** A fresh `PlotTicks` is built with no ticks, `Found` carries the previous ticks across
and the previous set is empty, and `All()` is reached only by pressing Select all. Three tests
now pin it: a fresh picker over 155 plots reads 0 of 155 plots ticked, reading the model carries
nothing across, and Select all is the only route to all of them.

So the state is pinned and cannot drift, but **the fault as reported is not explained**, and
saying it is fixed would be a guess dressed as an answer. If it is seen again on a pane nobody
has pressed Select all on, the next thing to look at is whether `Found` runs more than once with
something already ticked.

### What was broken to see the tests go red

Five, each restored byte for byte and checked with md5.

- Never saved reported as no model again. 1 red
- The escape made to return its text unchanged. 5 red
- The preselection put back on position alone. 2 red
- The note shown as the tool's behaviour again. 3 red
- A fresh picker made to tick every plot. 6 red

### What has not been run

None of this has been seen in Revit. Four of the five fixes are Core with tests over them and
the fifth, the label width, is a number in a pane file that only Revit can settle.

---
## 2026-09-09, thirty third pass. The component to template mapping, as a table

Pull request 41, merged into main as `a0d3d3b`. **The runner executed 823 tests against its
merged head, 0 failed and 0 skipped. Locally the same 823 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The question that has been open since the 1355 run is answered. The team measured it on the
1548 scan, 11 distinct values over 1384 sheets, off the component values block the round before
last added to the end of section 3, and it is now a table in Core.

```
DAILY MOSQUE           MOSQUES          NH STRT LESS 20m ROW   STREETS
FRIDAY MOSQUE          MOSQUES          NH STRT 20m ROW        STREETS
SCHOOL                 SCHOOLS          STREET 30m ROW         STREETS
HEALTH                 HEALTHCARE       STREET 36m ROW         STREETS
PARKING LOT            PARKING
EXISTING PARK          EXISTING PARKS
FUTURE PARK            FUTURE PARKS
```

### A table, never a string rule

`ComponentTemplates` holds the eleven and `TemplateForComponent` reads it and nothing else.
Matching is the whole value compared without case and with surrounding whitespace off, the same
plainness species matching keeps. No part of a value matches anything.

**The rule it replaces was wrong twice over.** It matched a word of the component against a word
of the template name, so PARKING LOT looked like a park because PARKING begins with PARK, and
all four street values answered nothing at all. A test in the round that wrote it asserted PARK
offered EXISTING PARKS, FUTURE PARKS and PARKING and called offering all three the honest
answer. It was honest about a rule that should not have existed.

It is many to one. Two values mean MOSQUES and four mean STREETS, and a test says so in those
numbers. Another says every one of the eleven resolves, written out by hand. Another says every
one of the seven templates is reached by at least one value, because a template no value reaches
could never be preselected and nothing on screen would show the hole.

### The park tie is broken by the model

EXISTING PARK and FUTURE PARK are separate values, so each preselects its own template. The pane
used to put that pair to the user always, and the file name was the only thing that could tell
the two workbooks apart. Recognising a workbook FILE is still the sheet name then the file name.
That is a different job and it did not change.

### The plot prefix is read nowhere

STREET 36m ROW covers MM and ST plots and NS carries two different street widths, so a rule on
the prefix would answer three of the eleven wrongly. A test preselects STREETS for one value
across MM and ST and for two different values on NS.

### A value the table does not hold

It preselects nothing, the pane says which value and that the table does not know it, and the
user picks. **The pane used to say nothing at all**, because `Preselect` returned on
`NeedsAPick` without showing the reason, so a tool that had looked and found nothing read exactly
like a tool that never looked. The line is `TemplateChoice.Why`, built in Core, and it is cleared
by every path that makes it untrue: a hand pick, a folder change and the next preselection.

The same move found one more. `_pickedAs` survived a preselection that failed, so ticking a
mosque plot and then an unmapped one left Create armed with MOSQUES under a line saying nothing
was preselected. Nothing is picked by hand on that path, because `Preselect` returns above when
something is, so what it held can only have come from plots that are no longer ticked. It is
cleared. Nobody would have seen it while the pane said nothing.

Section 3 of the report gains a template column, which is this table read back. Its closing line
used to read that nothing in the tool turns a value into a template name, and that is no longer
true, so it says what the table is instead. A value the model grows later prints as one the
table does not hold, which is the whole reason the block prints every value rather than a sample.

### Recorded rather than built

**The road width the STREETS template asks for by hand is inside the component value.** 20m in
NH STRT 20m ROW, 30m and 36m in the two STREET values, and less than 20m in NH STRT LESS 20m ROW.
`TemplateWords` already says the road width and the total length are typed by hand and the sheet
works the area out, which is why STREETS is the one template with no area cell. Nothing reads the
width out of the value and nothing here started to. It is written down because the value carries
it and somebody will want it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The table made to match on part of a value again. 3 red
- STREET 36m ROW dropped from the table. 4 red
- FUTURE PARK pointed at EXISTING PARKS. 3 red
- An unmapped value made to fall back to the first template. 2 red
- The report's template column made to print the value. 2 red
- A template the caller does not offer preselected anyway. 1 red

### What has not been run

The table and the reader are Core and covered. **The pane change is not.** The line saying why
nothing was preselected has never been seen in Revit, and neither has a preselected FUTURE PARKS.
Nothing in `RcrcGreen.Revit` has been run on this machine.

---
## 2026-09-09, the rule behind the last three rounds, written down

Pull request 40, merged into main as `5a5ded0`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written. No source file changed, so it is the suite that merged as `973c817`.

**The 404 rule the entry below records only works one way.** A log that comes back is a job that
has ended, and that held again. A 404 is not a job still running: every step of this job read
completed with conclusion success while the log was still 404 more than a minute later. The step
conclusions are the read that was right both times.

No code change. One rule into `CLAUDE.md` and a pointer from each of the two rules files.

**NEVER READ A SCHEDULE VALUE BY CELL POSITION. Ask the heading row which column it is. A
reader that cannot find its column says so rather than falling back to a position.**

Three rounds, four readers, one fault. `SoftscapeRows` took the botanical name off cell 0 and
the count off the last number. `ScheduleGroups` decided a row was named off cell 0.
`ShrubsAndLawnRows` read a species row off cell 0. The first column is the image and an existing
species prints with none, so every one of them was invisible on a plot whose rows all carry
photos, which is why each round found only the one in front of it.

`kpi-rules.md` said it as a fact about these schedules and `core-rules.md` did not say it at
all, so a reader touching `ScheduleRows` from the Core side met the rule nowhere. It is in
`CLAUDE.md` now, said once, and both files point at it rather than restating it.

### The half of the rule the code does not yet keep

The first sentence is kept everywhere. **The second is not**, and this round changed no code, so
it is written down rather than quietly true. Four readers still fall back to a position when the
heading row names no column:

- `ScheduleGroups.IsNamed` falls back to `row[0]`
- `SoftscapeRows.SpeciesIn` falls back to `row[0]` for the botanical name
- `SoftscapeRows.QuantityIn` falls back to the last whole number in the row
- `ShrubsAndLawnRows.SubtotalsIn` falls back to cell 0 for the species test

None has ever fired on a schedule these readers have been run against: the softscape and shrubs
heading rows both name BOTANICAL NAME, AREA and COUNT, so no report has come off a fallback. `ShrubsAndLawnRows` already shows the shape the rule wants for the other
half: no AREA column and it hands back nothing rather than guessing at one.

Taking the four out is a code change and was not asked for. It is open.

---
## 2026-09-09, thirty second pass. The group counter, the switch and the component values

Pull request 39, merged into main as `973c817`. **The runner executed 804 tests against its
merged head, 0 failed and 0 skipped. Locally the same 804 ran, 0 failed and 0 skipped**, after
the last file was written and after the six break watches were restored byte for byte.

The gate endpoints went stale again, the fourth round running. The job LOG settled it the same
way it did last round: it returns 404 while the job is running, so a log that comes back at all
is a job that has ended, whatever the status field still says.

The remote branch still held last round's commit, which main already carried under a different
hash as `82dd51d`. The two trees were identical, so the branch carried nothing but merged
history and was restarted from main with a force-with-lease pinned to that old commit. Nothing
unmerged was on it to lose, and that was checked rather than assumed.

One bug and two report faults from the 1521 scan, and nothing else.

### The group counter read the image column

Question 8 reported DM-12 Existing with 0 named rows of 6 and DM-13 Existing with 0 named rows
of 2, while section 6 of the same file printed five species under DM-12 Existing totalling 10
trees. A file that contradicts itself twenty lines apart has nothing in it worth believing.

**The first cell is the image and an existing species prints with no photo**, so its first cell
is a dash. `ScheduleGroups` decided a row named something by looking at that cell. It now asks
the heading row which column is BOTANICAL NAME and reads that one, falling back to the first
cell only where the heading row names no botanical column at all.

This is the same fault fixed in the softscape reader one round ago. The reader was corrected
and the counter, which feeds the same report, was left on the old rule. **Two rules for one
question is the shape this repo has now met seven times**, and `CLAUDE.md` records it as the
seventh.

Checked against the measured counts, each written out by hand in the test rather than worked
out with the code's own rule: DM-11 Proposed 3, DM-12 Existing 5 and Proposed 3, DM-13
Existing 1 and Proposed 2. A fifth test holds `SoftscapeRows.SpeciesIn` and `ScheduleGroups.Of`
against each other on one schedule, because the two disagreeing is what produced the fault.

### Every place that could hold the same shape, including the ones that were already right

Four things in Core read a schedule's printed rows. All four were read line by line.

- `ScheduleGroups.IsNamed`, the old `FirstCellHoldsText`. **THE BUG. Fixed**
- `ShrubsAndLawnRows.SubtotalsIn`, the test telling a species row from a subtotal row.
  **THE SAME BUG AND NO REPORT HAD SHOWN IT. Fixed**, because DM-11's shrubs are all Proposed
  and every one of them prints with a photo. An existing shrub would have been read as a
  subtotal, which would have gone into the workbook as an area
- `ScheduleColumns.IsStructureRow`. Reads the first cell, already right. A group heading and a
  phase row really do sit in the first cell with every other cell empty, which is measured off
  the 1355 rows and written in `kpi-rules.md`
- `ShrubsAndLawnRows`, the group name taken from `row[0]` after `IsStructureRow` has passed.
  Already right. That row has exactly one cell with text in it and this is that cell
- `ShrubsAndLawnRows`, the first cell tested again to keep TOTAL out of the subtotals. Already
  right and still needed. A subtotal names nothing anywhere, and the first cell is the one
  thing TOTAL does carry. It is reached only after the botanical test now
- `SoftscapeRows.SpeciesIn`, the botanical name. Already right, fixed the round before
- `SoftscapeRows.QuantityIn`, the last whole number in the row. Already right. It runs only
  where the heading row names no COUNT column, and the schedules in this model all name one
- `ScheduleGroups.GroupNameIn`. Already right and position independent: the one cell holding
  text, wherever it sits
- `KpiReport`, the rows as printed. Nothing to get wrong. It prints every cell in order and
  picks no name and no number out of them

`SpeciesMatching` also has a Rows, and it is the workbook's species list rather than a
schedule's. Each of its rows carries a named BotanicalName, so it is not this shape at all.

### KPI COMPONENT S/H is a switch, not a candidate

Its name holds COMPONENT, so section 9 offered it beside PRX_Component as another name the
model might carry the component under. It reads No on 9 of 9 title block types and is a show
and hide toggle.

`KpiNames.EveryValueIsYesOrNo` decides it, one method with both callers asking it. Section 9
leaves such a name out of the answer. Section 3 keeps it, in its own tally, in its own values,
and again by name in the new component values block, which says why it is not counted there.

**The section that offers it is not question 8.** The near miss list feeds questions 1 and 2,
the two about where the component lives, and question 8 never calls it. The fault is real and
the number in the request is off by six, so it is written down here rather than fixed silently.

### The component values do not match the template names

Measured: FRIDAY MOSQUE, SCHOOL, HEALTH, EXISTING PARK, NH STRT 20m ROW. The templates are
named existing parks, future parks, healthcare, mosques, parking, schools and streets. None of
the five is a template name and no string rule turns HEALTH into healthcare or NH STRT 20m ROW
into streets.

Section 3 now ends with every distinct value of the component the model holds, how many sheets
carry each, and every plot those sheets are for. Uncapped, where the rest of that section shows
twenty examples, because twenty sheets is not enough to build the mapping from and the mapping
is what preselects the template. The plot beside each value is PRX_Plot_ID read off the sheet
and the block says so. A value on sheets carrying no plot is counted and named. A component
name on a title block type is named with its reason and left out, because a type is not a sheet.

**Nothing maps a value to a template and nothing guesses one.**

### Open, and not to be guessed at in code

**Which template each value of PRX_Component means.** Five values are measured and none is a
template name. This is the question that stops the pane preselecting a template, and the new
block is what somebody answers it from. Until it is answered, no code anywhere turns one into
the other.

**SETTLED the round above.** The 1548 scan read eleven values off this block and the team turned
them into a table. It is `ComponentTemplates` and the entry at the top of this file has it.

### What was broken to see the tests go red

Six, each restored byte for byte and checked with md5.

- The group counter put back on the first cell. 3 red
- The shrubs species test put back on the first cell. 2 red
- `EveryValueIsYesOrNo` made to answer false always. 6 red
- The component block made to find no plot for any sheet. 1 red
- A title block type made to count as a sheet. 1 red
- The nothing-found line made to print when something was found. 1 red

### What has not been run

Nothing here has been seen in Revit. The group counts, the switch rule and the component values
block are all Core, all covered by tests, and none has been read off a real scan. The next 1521
run is what confirms DM-12 Existing comes back as 5 named rows of 6.

---
## 2026-09-09, thirty first pass. The shrubs and lawn shape, measured rather than guessed

Pull request 38, merged into main as `82dd51d`. **The runner executed 789 tests against its
merged head, 0 failed and 0 skipped. Locally the same 789 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored.

The check run and the job endpoints both reported the gate in progress for several minutes
after it had finished, the third round running. What settled it was the job LOG: it returns 404
while a job is running, so a log that comes back at all is a job that has ended. That is the
quickest honest way to tell a stuck gate from a stale endpoint.

Two corrections from the team before this reaches Revit.

### The shape was measured and I guessed it anyway

`kpi-rules.md` recorded DM-11 as GRASS 35 m² 46 then SHRUBS & GROUND COVER 70 m² 58, and the
round above read that as the heading sitting beside its numbers on one row. It does not. The
1355 scan report has the real rows and that report is not in this repository, because nothing
under `reports/` is ever committed, so it had never been read here. **The numbers were on record
and the shape was not, and a reader written to the numbers alone found no subtotal at all.**

The real thing is eleven columns wide and three things follow from it.

**The group heading is on its own row**, first cell only, every other cell empty. A phase row,
Proposed, sits under it in the same shape, so a structure row naming no wanted heading opens no
group.

**THE SUBTOTAL PRINTS TWICE.** 35 then 35, and 70 then 70. Adding a group's subtotal rows gives
70 and 140. One is taken, and two that disagree are named and refuse the write rather than being
chosen between.

> **CORRECTED ON 2026-09-10, THE FORTY FIRST PASS. THE PARAGRAPH ABOVE IS WRONG.** It was
> measured from DM-11 alone and DM-11 is the special case. A group prints ONE SUBTOTAL PER
> PHASE, then the GROUP TOTAL. Every DM-11 group holds one phase, so each prints two equal rows
> and that read as one subtotal printed twice. Taking the first row then took one phase and
> called it the group, which on the 0928 run over 20 plots meant 30 where the group is 84 and
> 361 where it is 820. The last row is the group's value and the check is that it equals the
> rows above it added together. The correction is left beside the claim rather than replacing
> it, because a wrong claim that quietly vanishes teaches nobody how it was arrived at.

**The species rows add up to the subtotal**, 36 plus 34 is 70, so the two are held against each
other and both are printed. That one is recorded rather than enforced: every number there is
already rounded to the metre on the way out of Revit, and a sum of rounded numbers need not
equal a rounded sum, so a refusal on it would fire on correct data.

TOTAL needed no special case in the end. It carries numbers, so it is not a structure row, and
its first cell holds text, so it is not a subtotal. It falls out on its own.

### Neither column sits where it could be assumed

Eleven columns wide, the first number in a subtotal row is the area and the last is L/DAY. So
`ScheduleColumns` finds the area, the count and the botanical name off the schedule's own
heading row.

**The softscape reader was changed too, which is one more than the two corrections asked for.**
It read the botanical name off the first cell and the quantity off the last number in the row.
On a schedule shaped like this one the first cell is an image file name and the last number is
L/DAY, so both would have been wrong. It now prefers the columns the heading row names and
falls back to the old behaviour only where it names neither. Whether the real softscape schedule
carries an image column is UNKNOWN and nothing here assumes either way. Say so if that change
was unwanted, it is one file.

### The unit, and nought

An area prints with its unit attached, 35 m², and a count does not. The unit comes off by
reading as far as the number goes rather than by stripping characters. **A real area can be
nought**: the hardscape schedule prints 0 m², which is the number and not an empty cell, and
that case is tested.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by md5:

- adding every subtotal row instead of taking one turned the two real row tests red
- letting a phase row open a group of its own turned the group test and the phase test red
- assuming the area column rather than reading the heading row turned five tests red
- dropping the disagreement refusal from `Reconciliation` turned its own test red

### The five workbooks

Five client templates arrived with the brief and **none is in this repository.** `*.xlsx` is
ignored at line 44 of `.gitignore` and `git add` was made to refuse a real one before anything
else was done. Nothing in these two corrections needed to read them.

### Still never observed

Everything the round above listed still stands except the shrubs and lawn shape, which is now
measured. Nothing here has been through Revit either: the new reader is tested against
transcribed rows rather than run against a schedule.

---

## 2026-09-09, thirtieth pass. The plot picker, several plots at once, and Create

Pull request 37, merged into main as `ce825b1`. **The runner executed 781 tests against its
merged head, 0 failed and 0 skipped. Locally the same 781 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API.

The check run endpoint reported the gate in progress for twelve minutes after it had finished.
Listing the run's jobs showed the truth, completed and successful 40 seconds in. The same lag
appeared last round. Read the job rather than the check run when a gate looks stuck.

The button the last four rounds deliberately did not add. It does something now.

### One checklist can cover more than one plot

This is the shape of the round. A checklist is not always one plot, it can be a whole asset
made of several, and then the workbook wants them added together. **Which plots belong to one
checklist is not written down anywhere and nothing here derives it.** No grouping by prefix, by
component or by anything else. The user ticks and the tool adds up exactly what was ticked.

The picker offers one plot, several, or all of them, from a list built as the union of two
sources: `PRX_Plot_ID` on the sheets and the `PRX_Ref Plot ID` filter value on the schedules.
Both lists are kept, the disagreement is shown on screen, neither wins. That is the seventh
place two records of one fact could have parted and it is held open on purpose.

### Adding printed numbers, and the rule that makes it safe

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed, and is a different thing from recomputing a number off elements, which is
never allowed and happens nowhere.

`Totalled` carries the per plot numbers and the total together and works the sum out again to
compare. **A total that does not equal its parts refuses the write.** The report prints each
plot's own number beside every total so the arithmetic can be checked by eye without opening
Revit. Trees merge on the group AND the botanical name: ALBIZIA LEBBECK is 1 existing and 13
proposed on DM-12, and merging on the name alone would put 14 into one tree sheet.

Two plots reporting an identical raw area are flagged and refuse the write until somebody
confirms. So does one plot whose two filled regions both hold an area, because which of them
carries it varies by plot and the type name cannot decide. The tool asks rather than picking.

### Matching a species

`SpeciesList` reads the workbook's own column D out of the template, resolving shared strings,
because a cell holding one stores an index and reading the index as the name would report every
species unmatched. Nothing in this repo carries a copy of the plant palette.

Matching is the botanical name compared without case and with surrounding whitespace off, and
nothing else. All three hard cases are tested: UNKNOWN against the four rows named Unknown Tree
matches none of them, the slash in ACACIA / VACHELLIA FARNESIANA is not split on, and the
apostrophe in BOUGAINVILLEA GLABRA 'PINK PIXIE' is part of the name. A name the workbook holds
twice is reported rather than placed on the first, for the same reason.

### The rule this round changed

`kpi-rules.md` said the tool never writes E5, G5 and H5, because the team types them. The brief
puts them on the pane as three boxes, so the tool now copies them through. The rules file and
`KpiTemplates` both say so now rather than one of them still saying the old thing.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Four breaks watched red first and each file restored byte for byte, checked by
md5:

- dropping the ticked-versus-read refusal from `Reconciliation` turned
  `APlotTickedAndNotReadRefusesTheWriteAndIsNamed` red
- merging species on the botanical name alone turned `TheSameSpeciesInTwoGroupsNeverMerges` and
  `TheGroupDecidesTheSheetAndNothingElseDoes` red
- comparing identical areas on the rounded metres rather than the raw turned both identical
  area tests red
- placing a name the workbook holds twice on its first row turned
  `ANameTheWorkbookHoldsTwiceIsReportedRatherThanPlacedOnTheFirst` red

One test caught a fault in its own fixture rather than in the code: every plot built by the
fixture had the same default area, so the identical area guard fired on a reconciliation test
that expected to pass. The guard was right and the fixture was wrong.

### Never observed, item by item

Nothing in this round has been through Revit. Every one of these is written down because it has
not been run, not because it is expected to fail.

- **No workbook has been filled.** `WorkbookPatcher` is proven by its own tests against a
  workbook the tests build, and the 37 parts in and 37 out was measured last round on the real
  EXISTING PARKS file. This round has not repeated it
- **No plot has been read out of a model.** `KpiPlotReader` compiles against the reference
  assemblies and has never run. Every method in it is untested, because the test project must
  never load the Revit API
- **The pane has never been drawn.** The plot picker, the three pickers, the three boxes and the
  Create button exist only as a mockup in `design/pr-37/kpi-create.html`
- **The schedule filter route has never run.** Schedules are found by the plot their filter
  names rather than by the plot in their own name. That is the more correct source by
  `CLAUDE.md`, and it has not been run once
- **The shrubs and lawn row shape is inferred, not measured.** CORRECTED BY THE ROUND ABOVE.
  The shape was measured all along, in the 1355 scan report, which is not in this repository
  because reports are never committed, so it had never been read here. Guessing it from the
  numbers put the heading beside them and the reader found no subtotal at all
- **The softscape total row's shape has never been seen.** A subtotal has an empty first cell
  on the fixture and is skipped. A grand total row carrying its own word in the first cell would
  read as a species here, come out as one the workbook's list does not hold, and be named in the
  report with its count. Visible, and written nowhere, but wrong
- **The three typed cells have never been written.** E5, G5 and H5 are new this round
- **`RememberedNames` has never read or written its file.** The two name boxes are meant to
  survive a Revit restart and that has not been shown
- **No refusal has been seen on screen.** The identical area confirmation, the region pick and
  the one-refusal-lists-everything line are all drawn in the mockup and never rendered
- **The output has never been overwritten.** The delete before the patch is written and unrun

### Open, and not to be guessed at in code

- **Which plots belong to one checklist is not written down anywhere.** The user ticks them.
  Do not derive a grouping rule from the prefix, from PRX_Component, or from anything else

Two from last round are still open and neither is answered here. The four rows named Unknown
Tree against the model's UNKNOWN, and the species names carrying slashes and apostrophes. The
code reports all of them unmatched rather than guessing, which is the honest half of an answer
and not the answer.

---

## 2026-09-09, twenty ninth pass. Two report faults from the 1355 scan, and the facts

Pull request 36, merged into main as `01d9886`. **The runner executed 722 tests against its
merged head, 0 failed and 0 skipped. Locally the same 722 ran, 0 failed and 0 skipped**, after
the last file was written and after the five break watches were restored. The squash carries no
co-author line and no generated-by footer, the message having gone through the API rather than
the GitHub button the hook cannot see.

The scan ran on the 1355 model. Two things in the report were wrong in the same way, both
answering a question from the wrong place while the right answer sat a screen above in the same
file.

**Section 9 contradicted section 3.** Questions 1 and 2 read NOT FOUND for PRX_COMPONENT while
section 3 printed PRX_Component on the sheet with 1384 values reading FRIDAY MOSQUE and SCHOOL.
The exact name is not in the model and the near miss is, with the answer in it. Section 9 now
names a near miss that holds values, says which of the three homes it sits on and what its first
values are, and counts the question as answered, because the file does hold the answer. A near
miss with no value in it still reads NOT FOUND, which is the honest half of the old behaviour.
The near miss word for these two questions is COMPONENT alone, not the sheet list of COMPONENT,
PLOT and UID, because a question about the component is not answered by a plot name.

**Question 8 was answered by the wrong evidence.** It reported the elements by phase created,
which came back (none) 6, because the elements a softscape schedule lists are RVT Link instances
and their phase is the link's. The answer is in the printed rows. `ScheduleGroups` in Core finds
the rows that name a phase the document holds and nothing else, and question 8 is answered from
those, naming the plot that showed it. DM-12 prints a TREES row, an Existing group of five
species with a subtotal of 10, a Proposed group of three, then TOTAL 39.

A group row and a subtotal row are the same shape, one cell with text and the rest empty. Only
the text tells them apart, so the phase names read off the document decide it. Nothing matches
on the shape of a row, and the words Existing and Proposed are not in the code: a project whose
phases are named anything else reads the same way. TREES is a group row for the category rather
than the phase, so it is not counted, and nothing anywhere knows the word TREES.

The element phases are still printed, said plainly as the link instances the schedule lists
rather than the plants, and they no longer make the question count as answered. The test that
asserted they did is reversed and says why.

### The facts

Six measured facts from the 1355 run are in `CLAUDE.md`: PRX_Component on the sheet is the asset
type and picks the template, PRX_Plot_ID is the plot every schedule filters on with prefixes
tracking the asset type, what the other three plot parameters really hold, how the softscape
schedule prints and that a species appears under both groups, what an existing species prints
with, and that every plot has two filled regions in the 00 link with the area on either one.

`CLAUDE.md` was at its 200 line ceiling, so the room came from the Drawing Sheet prose that
`.claude/rules/core-rules.md` already holds in full. Two facts thinned in that pass were checked
first: PRX_Furniture Lenght is in `core-rules.md` verbatim, and Schedules and Quantities was in
no other file, so it was written into `core-rules.md` next to the schedule rules rather than
lost. The run history that came out is in this log in full.

Four of the five open questions in `kpi-rules.md` are settled by these facts and now read as
settled rather than open, which is the same contradiction the two report faults were. Question 3
was wrongly put: neither region type is the intervention area, it varies by plot.

### Open, and not to be guessed at in code

- **The workbook holds four rows all named Unknown Tree and the model prints a species called
  UNKNOWN.** Nothing can match those on name. Four identical row labels cannot be told apart by
  a name lookup, and a model species called UNKNOWN is not the same thing as an unknown row
- **Species names carry slashes and apostrophes.** ACACIA / VACHELLIA FARNESIANA and
  BOUGAINVILLEA GLABRA 'PINK PIXIE'. A match on an exact string will fail on both, and nothing
  written down says how the workbook spells them

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. The suite after the last file
was written. Five breaks watched red first and each file restored byte for byte, checked by md5:
matching group rows on shape alone turned the subtotal test and the two group row tests red,
dropping the near miss from question 2 and then from question 1 turned one test each red,
letting the element phases answer question 8 turned the reversed test red, and dropping the plot
name from the answer turned the two group row tests red.

The shared real model fixture now prints the grouped shape rather than four flat rows, because a
fixture calling itself the real model while printing something the real model does not is the
next round's wrong answer.

---

## 2026-09-09, the review of the twenty eighth pass. Six faults it found in its own work

Pull request 35, merged into main as `1b78c91`. **The runner executed 711 tests against its
merged head, 0 failed and 0 skipped. Locally the same 711 ran, 0 failed and 0 skipped**, after
the last file was written and after the four break watches were restored. The two counts are
the same number arrived at twice and are stated apart on purpose.

The squash carries no co-author line and no generated-by footer this time. The one on `e343fe1`
got in through the GitHub squash button, which the commit hook cannot see. Supplying the commit
message through the API instead keeps it inside the rules the hook enforces, so that is how
this one was merged and how the next should be.

A five lens review over the round below, each finding then handed to a separate reader whose
job was to refute it. 58 raised, 24 reached the refuting step, 14 survived it, 10 were refuted
and 34 were never adjudicated. The 14 are six distinct faults once the lenses that found the
same one are put together, and all six are fixed here. Every one of them is this repo's oldest
shape: a count and the thing it counts read from two different places.

**A plot's regions were counted off the area values.** `ONE PLOT'S REGIONS TOGETHER` grouped
`InterventionAreas`, which the reader fills only where PRX_Intervention Area holds a value. A
region carrying a plot and no area was not in that list, so it vanished from its plot, and a
plot whose every region was blank was not a plot at all. The same page could say 279 regions
carry a plot and then group 266 of them under a heading claiming to show a plot's regions.
`LinkContents` now carries `RegionsCarryingAPlot`, every region with a plot whatever its area
holds, and the section groups that. A region with no area prints `(no value)` rather than
disappearing.

**A type carrying no plot had no row, so the answer none could not print.** The per type table
was driven by the types that carry a plot, and the reader only creates a key for a type when
one of its regions carries one. So RCRC_OUT OF SCOPE, carrying none, was absent from a table
whose own comment says telling a candidate from a non candidate is what it exists for. The
table is driven by every type in the link now and looks the carrying count up, so a type with
none prints a zero. A name in one list and not the other prints a line saying the tool is
contradicting itself, rather than a fabricated zero.

**Absent and blank both printed as an empty plot.** Nothing said which. PRX_Ref Plot ID now
has its own line counting the three states apart, the same shape the intervention area line
has, and a region carrying two parameters of that name is counted the way the area already was.

**The near misses printed once per missing name.** The list is a property of the home, not of
whichever wanted name went missing, and both wanted names are missing on the sheet, so the
whole block printed twice under two headings. Worse, `PRX_Plot_UID2` holds the word Plot, so it
was a near miss of itself and its values printed a second time under PRX_COMPONENT. The list is
worked out once and still names every name, because that line is a statement about the home.
The values print under the first NOT FOUND only, and a wanted name the home really holds is not
among them, because it prints under its own heading a few lines above.

**One schedule was recorded as passed over and read in full.** When no copy of a name lists an
element the reader falls back to the first, which it had already written into READS THAT DID
NOT HAPPEN as passed over. Two lines, one schedule, opposite meanings, every time the fallback
fired. The passed over lines are held back until the fallback has decided.

**The rules file told the next round to read one schedule per name.** `kpi-rules.md` held both
the old rule and this round's new one, in a file that loads as instructions whenever anything
under `Kpi/` is touched. `ChooseOnePerName` and its comments said one as well. One statement
stands now and it names `KpiReport.PlotsReadInFull` as the only home of the number.

**And the log entry below was wrong about its own test run.** It said stripping the brackets
turned eight tests red. Reproduced at `c370325` in a throwaway worktree, it turns 14 red across
10 methods, 696 passing. The eight was counted off the method names on screen instead of off
the run's own total, which is the thing this repo says never to do. Corrected in place.

### Checked

`dotnet build RcrcGreen.sln -c Release`, 0 warnings and 0 errors. `dotnet test` after the last
file was written: 711 passed, 0 failed, 0 skipped, up from 710. Four breaks watched red first
and each file restored byte for byte, checked by md5: driving the per type table off the
carrying list again turned the type table test red, grouping the plots off the area values
again turned the per plot test red, letting the near miss values repeat turned the sheet block
test red, and dropping the carried blank count turned the new plot tally test red.

### Not fixed, and why

The 34 findings that never reached the refuting step are not acted on, the same rule as the
last review round. They were mostly wording and naming: the unused `MainSheetOpens` and
`MainSheetCloses` constants, the run order pulling main before this round is on it, the
`RealShaped` fixture's numbers against the newer real scan, and the title block size measured
in millimetres that came out of CLAUDE.md when it hit its line ceiling. They stay reported and
unfixed rather than folded in unverified.

Two of the 10 refuted are worth recording because the refutation taught something. A filled
region is a system element with no loadable family, so it cannot carry a family parameter
beside a shared one of the same name, which is the mechanism that put a 1385 tally over an
empty list on title blocks. And a Revit category binding is project wide, so every sheet
carries a bound parameter or none does, which is why the side by side heading cannot be
counting a subset of sheets.

**The schedule reader fix has no test.** It is in `RcrcGreen.Revit`, which the suite does not
cover and cannot, because the test project must never load the Revit API. It is read and
reasoned about and not exercised, and the next real scan is the first thing that will run it.

---

## 2026-09-09, twenty eighth pass. Recognition fixed, the scan gaps, and the real workbooks

Branch `claude/inspiring-allen-xs113f`, restarted from main. The KPI scanner ran on the real
model for the first time, 96,959 elements in 5.4 seconds, six of the nine questions answered.
Everything here comes out of that run or out of using the tool on the seven real workbooks.

### Recognition was broken, and it is the first thing fixed

The pane read 7 workbooks in the folder, 0 recognised. `KpiTemplates` held the main sheet names
with the angle brackets stripped, on the reading that they were placeholder notation. They are
part of the name. The real ones are `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>` and `<Streets>`, all seven fixed, and a test now asserts every
entry opens with < and closes with >. Another asserts the four fixed sheet names.

This is the seventh time this repo has been bitten by a name that is not what it looks like,
and the first that a check against the real file would have caught before an install.

### The one-off check against two real workbooks

**2026-09-09, against two EXISTING PARKS files supplied in this chat session, the production
copy the team fills and the annotated copy carrying the source in each mapped cell. It was run
once, IT CANNOT BE RE-RUN, and no gate will ever repeat it.** Neither file is in this
repository and neither ever will be. The ignore rule was checked before anything else: `*.xlsx`
catches a workbook at any depth, and `git add` on one at the repo root and one buried under
`src/RcrcGreen.Core/Kpi/` was refused outright by git. What is committed is what the check
taught, never the files.

**The map against the annotation, cell by cell. Six of six agree.** The annotated file holds
the note in the mapped cell itself, so the check is exact:

```
D3   REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT                                    agrees
C5   REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2                                    agrees
E4   Project information / neighbourhood name                                   agrees
D8   REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area                    agrees
F11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS&LAWN SCHEDULE/SHRUBS & ...     agrees
H11  REVIT SHEET/REVIT COPONENTS LINK /SHRUBS & LAWN SCHEDULE/LAWN (GRASS)...  agrees
E5   DATE OF THE DAY, G5 EMPLOYEE NAME, H5 EMPLOYEE POSITION             typed by the team
```

The COPONENTS misspelling and the ampersand written two ways are both there in the real file,
which is why the tool carries the map and never reads it out of a workbook.

**Two things the check found that reading the map could not.** The annotation writes the shrubs
and lawn note in row 10 AND row 11, so the map could have taken either. Row 11 is the input:
in the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would have
destroyed a formula. The map was right and is now right for a reason. And `Area` is a defined
name pointing at `<Park Name>`!$D$8, with `H9` reading `=H8/Area`, which is the divide by zero
that goes away when the area is filled.

**Recognition against the production copy.** Its first sheet reads `<Park Name>` and under its
real file name it comes back EXISTING PARKS. Under a name holding neither park word it asks the
user to pick, which is correct.

**The patcher against a copy of the production file**, nine cells written across three sheets:

```
parts in 37, parts out 37, none lost, none added
parts changed 4: worksheets/sheet1, sheet2, sheet3 and workbook.xml
every written cell read back off the output as what was sent
calcPr gained fullCalcOnLoad=1 and kept calcId 191029
D7, D9, H9, F10 and H10 kept their formulas, D3 kept style 302, F11 kept style 493
```

Every number matches what the brief had measured, independently.

**The object model failure, reproduced rather than taken on trust.** Loading the same file into
an object model and saving it back gives 20 parts out of 37, losing 21 and adding 4. The 21 are
the embedded image, all five printer settings, both threaded comment parts, both comment parts,
all three VML drawings, the array metadata, the persons part, the calculation chain, the shared
strings and three sheet relationship parts. The file still opens. That is the failure this repo
must never ship, and it is now measured here rather than believed.

**The tree rows.** Species stop exactly where the map says: existing 4 to 92, proposed 4 to 84,
header row 3, and both sheets total at row 93 with `SUM(B4:B92)`.

### The four scan additions

**A near miss that is named is now shown.** PRX_COMPONENT does not exist in this model. The
sheet carries PRX_Component, capital C only, on all 1385 sheets with 1384 values, and the first
report named that near miss while printing not one of them. Values of every near miss now print
with the same columns and the same twenty cap the exact name would have used. A near miss named
and never shown is the answer withheld.

**The four plot parameters print side by side**, PRX_Plot_ID, PRX_Plot_UID, PRX_Plot_UID2 and
PRX_Plot_NH, one row per sheet, the fullest rows first. All four exist with values and the
report showed one, so the four could not be told apart from the file.

**Every filled region prints its plot** beside its type and its area, with a count per type of
how many regions carry one, and up to three plots' regions listed together. One plot's regions
read together is what settles which type is the intervention area.

**Three plots per schedule name are read in full**, not one. One plot cannot show whether the
group headings repeat across plots or whether an Existing group ever appears.

### What was checked, and how

`dotnet build RcrcGreen.sln -c Release` and `dotnet test`, both after the last file was
written. Build 0 warnings and 0 errors across all three projects. 710 tests, 0 failed and 0
skipped, locally, up from 698 before this round's new test file and 657 before the round.

Three breaks were watched failing before the tests were trusted. **Stripping the angle
brackets off all seven sheet names again**, which is exactly the bug this round fixes, turned
the two template map guards, every recognition case and the would-fill literal red, 14 failing
cases across 10 test methods. The eight written here first was wrong, counted off the method
names on screen rather than off the run's own total, and the run said 14 failed, 696 passed.
**Reordering the four plot names**
turned the three side-by-side tests red. **Dropping the near-miss values call** turned the
three new near-miss tests red along with the two older ones that print through the same path.
Each file was restored from a copy taken before the break and checked byte for byte.

Two faults the test writing found in this round's own report wording, both fixed here. Section
5 still said the reader takes one plot per name while it now takes three, which is this repo's
two-records fault in a sentence, so `PlotsReadInFull` moved into Core and the Revit reader
reads it from there rather than holding a second copy. And the regions-per-type heading printed
"1 types", which now goes through the same `Count` helper the rest of the report uses.

### Never observed

- No filled workbook has been opened in Excel. The 44 errors against 45 is the brief's
  measurement, not reproduced here, because nothing in this session can recalculate a workbook
- The scan has not been run again since these four additions, so not one of them has ever
  appeared in a real report. Every line of them is written and unseen
- The recognition fix has not been seen in the pane. That 7 of 7 are recognised is proven
  against one real workbook through a harness, not through Revit
- The near miss values, the four plot columns, the region plot column and the three plot reads
  have never run against a document
- The five open questions are open. The report was extended to put the evidence for each in
  front of somebody, and nobody has read it yet

---

