## 2026-09-17, ninety seventh pass. Two checks that missed real errors, and four report lines

**Merged to main as `a3e531c`**, pull request 162, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

Bader ran the full 154 plot press on NG05 on 17 September at 15:55, on the templates the team
saved that day. It wrote 154 workbooks and 154 PDFs and READY read 122 YES. **The report and the
templates are not in this repository and they are not going into it.** Every measurement below is
copied from them.

**2165 tests, 1345 of them KPI**, run after the last file was written. 28 hook cases. Build zero
warnings. **80 audit findings, 23 FIXED, 57 open**, counted off the four files again and nothing
closed, renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody.

### Errors a workbook opens with were not checked at all

**ALL 73 STREET PLOTS AND 5 FUTURE PARKS PLOTS READ READY YES.** The STREETS template holds

```
Tree List - Existing W4   IF(OR(#REF!=" ",#REF!<1)," ",#REF!/ TotTrees)
Tree List - Existing W5   IF(OR(V4=" ",V4<1)," ", V4/TotTrees)
```

while every row from W6 to W57 reads its own row of V, and the FUTURE PARKS template holds
`Tree List - Proposed L93 = AVERAGE(L4:L93)`, a range that holds its own cell. The report printed
that second formula in 12 plot blocks without a word, and **it holds no line with #REF and no
line saying circular.**

The template check gets three questions, **asked on every tab rather than on the two tree lists
alone**, because the first is on Tree List - Existing, the second on Tree List - Proposed and
nothing says the next will not be on the first tab. `TreeListCheck.OnEveryTab` works them out
once per tab, folds them into a tree list's own check where the tab is one, and gives a check of
its own to a tab that is not, so no cell is named twice. A tab with nothing wrong gets no row.

- **A formula holds `#REF!`**, read off the formula's own text.
- **A formula reads a range that holds its own cell**, through `CellArea.Contains` and the
  formula's own sheet and cell. A same sheet reference already carries the owning formula's sheet
  name, substituted at parse time, so nothing here compares a sheet a second way.
- **A formula's shape differs from the cells directly above and below it in the same column
  block.** Each formula is rewritten relative to its own row first, so a column filled down reads
  as one shape.

**AN ABSOLUTE ROW IS LEFT ABSOLUTE AND THAT IS NOT A DETAIL.** `$B$4` does not move when a column
is filled down, so rewriting it relative would have made every row of
`COUNTIFS($B$4:$B$83,...)` differ from its neighbours. That is 54 false lines on one sheet of one
template, and the dollar before the row is what the rule reads.

**ONLY A ROW WITH A NEIGHBOUR ON BOTH SIDES IS ASKED, AND ONLY WHERE IT DIFFERS FROM BOTH.** A
block may differ on purpose at its first or its last row, which is why W4 is not named and W5 is,
and why **a differing shape is a report line and never moves READY**.

**READY READS NO ON THE FIRST TWO.** `TreeListCheck.StopsAPlotBeingReady` answers off the fault's
own kind rather than off its printed words, and `PlotReady.TemplateErrors` finds the plot's
template through the run that read it, which is how `Divisions` already finds a plot's own
workbook. The reason names the template, the tab and the cell.

**The break watch.** Take the `#REF!` question out. The test named for it went red naming the
cell and printing the STREETS formula back. `TreeListCheck.cs` restored, md5
`4286794676d4d4c9011cecb6739da961` before and after, which is the hash it carried at that moment
rather than the one the round ends on.

### The template check skipped every Proposed range

A range was measured against the last row that NAMES a species. **Tree List - Proposed names
species to row 83 while its total B93 reads `SUM(B4:B92)`**, so a Proposed range ending at row 83
was never named at all. In the templates that ran, none of these was named:

```
T69, S70 and T70        EXISTING PARKS, FUTURE PARKS, MOSQUES, PARKING
S70 and T70             HEALTHCARE, SCHOOLS
S69 to T70, V4 to V57   STREETS
V4 to V57               FUTURE PARKS
```

**PL-35 and ST-07 had WASHINGTONIA ROBUSTA WRITTEN INTO Tree List - Proposed D84 this press**, a
row every one of those ranges leaves out.

It is measured against what the list's own total reaches now, `SpeciesList.TotalLastRow`, and the
line names the total cell. **A row inside the total's reach is a row a species can be written
into**, which is the fact the rule rests on, and the last named row is not that. The analysis
block rule and the first tab column rule from round 96 are untouched.

**The break watch.** Measure against the last named row again.
`AProposedRangeStoppingAtTheLastNamedRowIsNamed` went red naming S70 and saying the check named
nothing. Same file, same md5 before and after.

### Four report lines

**1. THE GLANCE PRINTED ALL 60 `#DIV/0!`.** It names at most five now, through
`DivisionGlance.WhereNamed` and the same `Named` of 5 the divisions it could not work out already
use, and its sentence says where the rest are. The full list stays in each plot's own block.

**2. IT SAID EACH ONE DIVIDES BY A CELL THE TEMPLATE ALREADY HELD, AND S70 DIVIDES BY A RANGE.**
The divisor travels on the finding now, `FormulaAtRisk.DivisorOver` and `DivisorInWords`, set
where the risk is built rather than read back out of the sentence. A one cell divisor names its
cell too, which it never did.

**AND THE TREES SENTENCE IS MADE TRUE RATHER THAN ASSUMED.** `GuardAnswer` told a formula with no
guard at all from a guard that was read and did not hold, which it did not, so
`TheGuardCountedTrees` says the workbook's own tree total was one or more while the range counted
nought. Only then does the line say the plot's trees sit on rows the range does not reach. A
formula carrying no guard says nothing about how many trees there are and does not get it.

**3. THE M83 REASON OPENED WITH THE COLUMN L WORDING.** Every canopy difference was prefixed with
`the workbook's canopy column is not the one this tool works out`, including FP-18's row 83, whose
canopy column is exactly the one this tool works out and whose M83 is empty. The opening is chosen
off WHICH KIND of difference was found, through two flags set where each is found, so the M83 case
opens with `a row computes a canopy per tree and adds none to the canopy total`. A run holding
both kinds says both. The two column drift tests keep their wording unchanged.

**The break watch.** Return the column sentence to every case.
`TheGuardNamesTheCellThatWouldHaveAddedTheCanopy` went red on the opening. `WorkbookArithmetic.cs`
restored, md5 `4187d0bacac3f96fb48086bbb5ad5504` before and after.

**4. THE RUN SHEET WAS TITLED `The seven checks` OVER THIRTY STEPS**, at line 1 and again at line
208 as the file stood before this round. Both read `The run sheet` now, the second at line 227
once this round's three items had gone in above it, and the sheet says why. That was a request
this task logged in the ninety sixth pass and it is closed.

### The audit files, counted again

```
steps/audit-kpi.md      29 findings, numbered 1 to 29     8 FIXED   21 open
steps/audit-kpi-2.md    20 findings, numbered 30 to 49    9 FIXED   11 open
steps/audit-kpi-3.md    14 findings, numbered 50 to 63    2 FIXED   12 open
steps/audit-kpi-4.md    17 findings, numbered 64 to 80    4 FIXED   13 open
                        80                               23         57
```

Counted off the files rather than carried forward, and the two greps that over-count are the ones
the ninety sixth pass named: `audit-kpi-4.md:19` is prose whose sentence opens with the number 80,
and `audit-kpi-3.md:8` and `audit-kpi-4.md:11-13` are preamble sentences counting the files before
them. **Nothing was closed, renumbered or reordered.**

### The run sheet

`steps/2026-09-16-kpi-checks.md` keeps its shape, its Windows order and every existing check, and
gains three at the end, one action each: **31** every street plot reading READY NO and naming W4
with W4 and W5 named in the template check, **32** every future park plot reading READY NO and
naming L93, and **33** the Proposed cells still reading row 83 named template by template. What
remains, the audit block and what comes next are refreshed off this round.

### What is UNKNOWN

**Whether the HEALTHCARE Tree List - Existing V4 to V57 pair is a fault is UNKNOWN from here.**
Those cells read `$F$4:$F$101` in the first SUMIF and `$F$4:$F$83` in the second. The second is
short of what that list's total reaches and the range question names it, which is the answer this
round gives. Whether the client meant the two to differ is a question for the team, and no client
workbook is in this repository to settle it.

**How many cells each template's first tab has out of step is UNKNOWN until the next press.** The
shape question has never run over a client file, and the count it returns is what the next report
says.

### What the claim checker flagged

**IT FOUND NO WRONG CLAIM**, and said so after checking every identifier, every test message, the
absolute row arithmetic, the two drift tests and all four audit files line by line. What it
flagged is three things it could not reach and one number that had moved.

**THE NUMBER THAT HAD MOVED.** This entry said the run sheet's second title sits at line 208.
That is where it sat BEFORE this round, which is what the round was given and what a read of line
208 returned off the file before any edit. It sits at 227 now, because this round put three items
in above it. The sentence says both lines, because a line number with no moment attached to it is
one nobody can check.

**THREE THINGS IT COULD NOT CHECK, AND IT SAID SO PLAINLY** rather than accepting or rejecting
them. It had no shell, so it could not compute the two md5 hashes, run the build, run the tests or
run the hook cases, and it has no git history, so it could not see the file as it stood before
this round. **I re-ran every one of them myself after the last file was written**: 2165 tests with
0 failed, 1345 of them KPI, 28 hook cases with 0 failed and build zero warnings. It counted the
`[Fact]` and `[Theory]` attributes at 1866 over the test project and 1124 under `Kpi`, which sits
under the run counts the way theory rows do.

**IT RAN BEFORE THE SIX DEFECTS SECTION ABOVE**, which moved the counts from 2161 and 1341 to
2165 and 1345 and moved two of the files under it. The hashes in the break watches are the ones
each file carried at its own break watch, which is what a break watch records.

**AND IT NAMED THE CLASS OF CLAIM THAT COMES FROM OUTSIDE THIS REPOSITORY**, which is every
measurement of the 15:55 press and of the client's templates. This entry says at the top that
neither is in here, and the checker confirmed that each of those facts is echoed word for word in
the code's own docstrings and test names, so the tool was built against a measurement rather than
against a guess.

---

## 2026-09-16, ninety sixth pass. Three features that behaved wrongly on real files

**Merged to main as `60949ee`**, pull request 160, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

Bader ran a streets only press on 16 September at 21:38, 81 listed plots on NG05. **The report is
not in this repository and it is not going into it.** Every number below is copied from it.

**2152 tests, 1332 of them KPI**, run after the last file was written. 28 hook cases. Build zero
warnings. **80 audit findings, 23 FIXED, 57 open**, counted off the four files again this round
and nothing closed, renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody.

### READY must not pass a division the check could not work out

The glance said the formula check found no #DIV/0! and that 284 divisions could not be worked
out. Those 284 are S70 and T70 on both tree lists of every one of the 71 plots the press wrote,
each reading that its IF guards the division and what the guard reads could not be evaluated.
**NS-29, NS-33, ST-13, ST-18 and ST-25 have every existing tree on Tree List - Existing rows 84
to 101**, so `COUNT(B4:B83)` is nought and their Excel shows #DIV/0!. All five read READY YES.

Two faults, one under the other.

**THE GUARD'S OWN CELL IS A FORMULA AND NOTHING FOLLOWED IT.** `TotTrees` is a defined name
pointing at `Tree List - Existing B102`, which reads `SUM(B4:B101)`. `TheGuard` answered
`NotEvaluated` for any guard cell holding a formula, which is every plot of every press, so the
division was never judged at all. It follows the name to its cell now and, where that cell is
`SUM`, `COUNT` or `COUNTA` over one range, counts the range off the cell states after the write.
Anything else is still not evaluated, because working out what a formula computes to is the thing
this reader does not do.

**ONE COUNTER, ASKED BY THE DIVISION AND BY THE GUARD.** `RangeCountsToNought` was the only place
a range was counted and it answered a yes or no. `OverARange` is the counter now and
`RangeCountsToNought` asks it. Two counters would have been two rules for one question.

**AND THE FINDING CARRIES ITS OWN CELL.** `DivisionsNotEvaluated` was a list of sentences, so
READY naming the cell would have meant reading a cell reference back out of a printed line. It is
a `DivisionNotEvaluated` now with the sheet, the cell, the formula and the reason apart, which is
the rule `FormulaAtRisk.IsDivideByZero` already follows.

`PlotReady.DivisionsNotChecked` is the new reason and it reads `a division could not be checked,
Tree List - Existing S70, Tree List - Existing T70`. **The glance names at most five**, through
`DivisionGlance.Named`, and its sentence says how many there are and that every one of them is in
its own plot's block. **The full list is in the block**, under `THE DIVISIONS THIS CHECK COULD NOT
WORK OUT` in `KpiCreateReport`, which is where a person looking at one plot reads it.

**The break watch.** Put `GuardAnswer.NotEvaluated` back where the name is followed.
`ACountPastTheRangeIsFoundThroughTheGuardsOwnSum` went red naming S70 and printing what the check
said instead, which is the 21:38 sentence word for word. Two more went red with it.
`WorkbookFormulas.cs` restored, md5 `7afd785468306de624176e77ef0bdc32` before and after.

### The no planting line never fired

MM-06, MM-07 and NS-23 read 0 in every tree, shrub, lawn, water and green cover box. Their
softscape and shrubs and lawn schedules each printed one row, the heading, with no group row, no
species row and no TOTAL row. The glance said every plot this press read printed at least one
schedule row.

`NoPlanting` asked `ScannedSchedule.BodyRowCount`, whose own docstring says it counts headings and
totals, so a schedule showing only its heading holds 1. **It counts the rows below the heading
now**, off the rows as printed, and the heading is the first row, which is what `SoftscapeRows` and
`ShrubsAndLawnRows` already take it to be at `ScheduleRows.cs:269`.

**THE RECONCILIATION'S OWN COUNT CARRIED THE SAME ASSUMPTION AND IS FIXED THE SAME WAY.** It
counted a schedule as having printed a body when `BodyRowCount > 0`, at `Reconciliation.cs:373`
before this round and `:380` after it. What it changes: the run's count of schedules that printed
a body drops by those three plots' six. Both places ask one method,
`NoPlanting.RowsBelowTheHeading`, so the line that names a plot and the count that says how many
schedules printed a body cannot part.

**A THIRD CASE OF THE SAME SHAPE, FOUND WHILE MAKING THE FIX AND FIXED WITH IT.** A schedule whose
rows Revit refused holds no rows at all, so it read as a schedule that printed nothing. That is a
read that did not happen answering as a measurement, which is the fault this file carries at the
top. `RowsWereRead` is asked now and such a plot is not named, because it is already named by the
refusal that produced it.

**The fixture's rows include the heading row**, which is how ten of the twelve test files that
use `CreateFixture.Softscape` or `CreateFixture.ShrubsAndLawn` already write them. The two that
passed body rows alone were `NoPlantingTests` and `LinksLoadedTests`, and both carry the heading
now. The second had already written the right words beside the wrong rows: its own comment reads
that the schedules printed one row, the header, and no body.

**The break watch.** Put `BodyRowCount == 0` back. `OnlyThePlotWhoseTwoSchedulesPrintedNothingIsNamed`
went red naming MM-01 and saying the check found no plot at all. `NoPlanting.cs` restored, md5
`087a22aa231fb11b16c44a856b94a2fb` before and after.

### The template check missed cells and counted others twice

On `GRP_-_KPI_Checklist_-_DD_STREETS.xlsx`, where the section read 293 cells named.

**1. N85 AND N88 TO N101 ARE EMPTY AND NONE WAS NAMED.** The water question skipped a cell found
in `typed`, at `TreeListCheck.cs:380` before this round and `:404` after it, and
`WorkbookPackage.CellTexts` adds every cell element it finds, so a formatted
cell holding nothing is in that dictionary with an empty text. **The reader is left alone**: its
job is to report what is in the file, and a reader that dropped an empty cell would make a cell
that is not there and a cell holding nothing read the same. The rule lives at the question, once,
in `TypedSomething`, and the water question and `WhatItHolds` both ask it.

**2. THE RANGE QUESTION PRINTED 265 LINES ON ONE SHEET AND ONLY 139 ARE DISTINCT.** Each of V4 to
V57 was printed twice for F4 to F83 and twice for B4 to B83, because a formula naming a range
twice reads it twice. Each cell and range pair is named once now, and **the count at the top is
the number of distinct cells**, through `TreeListSheetCheck.CellsNamed`, because one cell reading
two ranges is two lines about one cell.

**3. S59, T61, V59 AND W61 READ THE ANALYSIS BLOCKS AND WERE NAMED AS STOPPING SHORT.** They read
S4:S34, T4:T34, V4:V57 and W4:W57. A range counts as stopping short only when its column sits
inside the list's own block, and **the block's right edge is read off the file**: the furthest of
the count column, the botanical name column, the height and diameter columns off the heading row,
the canopy column a canopy formula really sits in, the total canopy column off the canopy total's
chain, and the water pair off the sheet's own formulas. On the measured templates that is the
water total at O, and Q, S, T, V and W all sit beyond it.

**IT IS A NARROWING AND THE NARROW SIDE IS THE SAFE ONE.** A list column further right than every
column this check reads is a range it will not name, which costs a line nobody gets. Naming the
analysis blocks cost 126 lines of 265 on one sheet of one press. **Whether any template holds a
list column past the water total is UNKNOWN from this repository**, because no client workbook is
in it, and check 30 of the run sheet is what answers it.

**4. A FORMULA ON THE FIRST TAB READING SUCH A COLUMN HAS ITS OWN QUESTION.** `<Streets>` E37
reads Q4 to Q34 and E38 reads T4 to T68. Column Q holds nothing and T is the family percentage in
the analysis block, so their fault is the column and not the length.
`TreeListCheck.ColumnTheListDoesNotFill` names the column and what it holds, read off the sheet.
**It does not ask whether the range stops short**, because the length is not what is wrong with
it. **How many such formulas each template carries is UNKNOWN from here** and the next press
answers it.

**The break watch.** Let an empty text count as typed again. `AnEmptyFormattedWaterCellIsNamedAsEmpty`
went red naming N85 and saying the check named no cell at all. `TreeListCheck.cs` restored, md5
`254bb9deaaa3d6849f586dba3277c26e` before and after.

### The audit files, counted again

```
steps/audit-kpi.md      29 findings, numbered 1 to 29     8 FIXED   21 open
steps/audit-kpi-2.md    20 findings, numbered 30 to 49    9 FIXED   11 open
steps/audit-kpi-3.md    14 findings, numbered 50 to 63    2 FIXED   12 open
steps/audit-kpi-4.md    17 findings, numbered 64 to 80    4 FIXED   13 open
                        80                               23         57
```

**Counted off the files rather than carried forward, and two greps over-count.** A plain
`^[0-9]+\. +[A-Z]` reads 18 findings in the fourth file, because line 19 is prose whose sentence
opens with the number 80. And a plain search for FIXED reads 3 in the third file and 7 in the
fourth, because each file's preamble counts the files before it: `audit-kpi-3.md:8` and
`audit-kpi-4.md:11-13` are sentences about the other files rather than marks on findings. The
numbers above are the findings themselves. **Nothing was closed, renumbered or reordered.**

### The run sheet

`steps/2026-09-16-kpi-checks.md` keeps its shape, its Windows order and every existing check, and
gains three at the end, one action each: **28** the five plots reading READY NO and naming S70,
**29** the no planting line naming MM-06, MM-07 and NS-23, and **30** the STREETS template check
naming N85 and N88 to N101, printing no line twice and no longer naming S59, T61, V59 or W61. What
remains, the audit block and what comes next are all refreshed off this round.

### A request for Bader

**The sheet is titled `The seven checks` and it holds thirty steps.** It has been wrong since the
sheet grew past seven and this round did not rename it, because the title is what you call the
file. A line about what a thing does that is not what it does is the shape this repository already
carries three times over, and renaming it is one word whenever you want it.

### What the claim checker flagged

**ONE CONTRADICTION, AND IT WAS REAL.** The comment this round put in `Reconciliation` read that
the 21:38 press's three plots move from `2 of 2` with a body to `0 of 2`, while this entry counted
the same fact as six schedules. Three plots holding two schedules each is six, and the two records
of one fact disagreed in the two places a person reads them. **The comment was the wrong one**,
because it said of three plots what is true of one, and it now says each of the three moves from
2 of its 2 schedules to 0 of 2, six over the three.

**THREE THINGS IT COULD NOT CHECK AND SAID SO PLAINLY**, rather than accepting or rejecting them.
It had no shell, so it could not compute the three md5 hashes, run the build or run the tests, and
it has no git history, so it could not see what `TreeListCheck.cs:380` held before this round. **I
re-ran all three myself after the last file was written**: 2152 tests with 0 failed, 1332 of them
KPI, build zero warnings, and the three hashes read back off the live files exactly as they are
written above. It did check the end state of the empty cell rule against the code and found it
consistent, and it counted the `[Fact]` and `[Theory]` attributes at 1857 over the test project
and 1115 under `Kpi`, which sits under the run counts the way theory rows do.

**AND THREE THINGS I HAD ALREADY CORRECTED BEFORE IT REPORTED**, found by checking my own
citations. `Reconciliation.cs:373` and `TreeListCheck.cs:380` are the lines before this round and
`:380` and `:404` after it, and this entry now says both. And the claim that `NoPlantingTests` was
the one test file passing body rows alone was wrong, because `LinksLoadedTests` did too, which the
sentence two lines later had already said.

**Everything else it could reach matched**, and it says which: the audit counts off all four
files, the over-count example line by line, the 28 hook cases counted by hand out of
`hook-tests.sh`, `ScheduleRows.cs:269`, `DivisionGlance.Named`, `PlotReady.DivisionsNotChecked`'s
sentence, the report heading, the three test names and the three new run sheet checks.

**One thing it noted that is not an error.** The run sheet's title is
`# The seven checks, 16 September 2026` and this entry calls it `The seven checks`, which drops
the date. The request above is about the word seven and not about the date.

---

## 2026-09-16, ninety fifth pass. The two requests the pass before this logged

**Merged to main as `619f134`**, pull request 158, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**Nothing the tool does changed and nothing outside `steps/` changed at all**, so the counts are
the ninety fourth pass's and stand unmoved: **2139 tests, 1319 of them KPI**, 28 hook cases,
build zero warnings. **80 audit findings, 23 FIXED, 57 open**, untouched, with nothing closed,
renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody.

The ninety fourth pass pointed every build instruction at the add-in project and logged two
things it had not been given: six run sheets still run `dotnet test` locally on the `net10.0`
test project, which stops with `NETSDK1045` on a PC without the .NET 10 SDK, and three sheets
carry a test count off main as it was. This closes both.

**`steps/` IS COMMON GROUND**, which is why a KPI round edits the Drawing Sheet's and the View
Filters' own run sheets. `.claude/rules/territory.md` fences `src/` and `tests/` per task and
names `steps/` among the common files a commit may carry beside one task's work. Said here
because it is worth somebody being able to check rather than discover.

### The five older KPI sheets are marked as records

`steps/2026-09-15-kpi-fixes.md`, `steps/2026-09-15-kpi-rerun.md`, `steps/kpi-create.md`,
`steps/kpi-scan-2.md` and `steps/kpi-templates.md` each carry one line under their title now:
this is an older sheet kept as the record of that round, and a run today uses
`steps/2026-09-16-kpi-checks.md`. **Nothing else in any of the five moved**, so their steps and
their counts stay as the record of the day they were written.

**THE PRECONDITION WAS CHECKED BEFORE ANY OF THE FIVE WAS MARKED.** Grepped for all five names
across the repository, thirty hits over four files:

```
steps/log-kpi.md            23   this task's own log, a record
steps/ai-max-state-kpi.md    5   this task's own state, a record
steps/log-drawing.md:1033    1   the Drawing Sheet's log, a record
.claude/rules/kpi-rules.md:6 1   a paths entry in that file's front matter
```

**The first draft of this entry said every hit outside the two KPI records was a single line, and
that was wrong.** The sweep it was written off piped `steps/log-drawing.md` out, and then the
sentence did not say so, which is a filter turned into a fact. The Drawing Sheet's log names
`steps/kpi-create.md` at `:1033` as the shape a sheet of its own was written in. It is a record
of another task's round rather than an instruction, so it does not hold the mark back either, but
the count was wrong and the reason it was wrong is worth more than the count.

**Neither of the two is an instruction pointing a reader at one of the five as the sheet to
run.** `.claude/rules/kpi-rules.md:6` is a `paths:` entry in that file's own front matter, a
trigger that loads those rules when that file is touched.

So it does not hold the mark back, and it is named here because marking the sheet leaves a
trigger aimed at a file nobody should now be following. Whether that trigger should move to
`steps/2026-09-16-kpi-checks.md` is Bader's, and it is in the requests below.

**And `steps/2026-09-16-kpi-checks.md` really has no local test step**, which is what makes it
the safe sheet to send a reader to. Swept for rather than taken on trust: it holds no
`dotnet test` line at all.

### The three task sheets read the gate instead

In `steps/run-drawing.md`, `steps/2026-09-13-colour-box.md` and
`steps/2026-09-13-view-filters.md`, step 5 is now one action, open the pull request on GitHub and
check the test gate shows a green tick, keeping its number. Each says the tests run there on
.NET 10 so the PC needs no .NET 10 SDK, and each points at the count in the top entry of its own
task's log rather than carrying a number:

```
steps/run-drawing.md:19-27              points at steps/log-drawing.md
steps/2026-09-13-colour-box.md:26-34    points at steps/log-view-filters.md
steps/2026-09-13-view-filters.md:25-33  points at steps/log-view-filters.md
```

The three numbers that went were 1103, 1578 and 1569, against today's 2139. **A sheet that names
a count goes stale the next time anybody adds a test**, and the log's top entry is the one place
that number is kept up to date, so the sheet reads it there.

**AND THE SENTENCE THE PASS BEFORE THIS PUT UNDER EACH BUILD STEP HAD TO GO WITH IT.** Round 94
wrote `The test step above this one does, because tests/RcrcGreen.Core.Tests targets .NET 10`
under the build step of all three, which was true while the step above ran the tests here. **The
moment that step became a gate check the sentence was false**, and a run sheet whose own two
steps disagree is the fault this repository's front door names. It reads that nothing on that PC
needs the SDK any more, the build because it does not and the step above because it reads the
gate.

### Which of the three is the latest sheet for its task

Asked of the two logs rather than guessed.

- **`steps/2026-09-13-colour-box.md` IS the latest View Filters sheet.** The top entry of
  `steps/log-view-filters.md`, the second pass, names it at `steps/log-view-filters.md:22` as
  that round's guide
- **`steps/2026-09-13-view-filters.md` is the EARLIER of the two.** It is named at
  `steps/log-view-filters.md:133`, in the first pass entry below the top one. It is left as a
  live sheet rather than marked a record, because this round was given the three sheets to
  convert and not a judgement to act on, and because it is another task's file
- **`steps/run-drawing.md` is the only Drawing Sheet run sheet there is**, so it is the latest by
  being the only one. **It is also older than that task's latest work, and that is worth
  saying**: its own opening says nothing in the pane has been through Revit since pull request
  32, while the top entry of `steps/log-drawing.md` is the sixty third pass carrying pull
  requests 105 and 106. **Whether it still covers what the panel does today is UNKNOWN from
  here**, because nothing in this repository is a newer Drawing Sheet sheet and that task is
  another session's

### What the sweep found, and what it finds now

Item 3, run over every file in `steps/` outside the logs and the audit files, which are records.
**Before the edits it found exactly the six local test runs and the three counts items 1 and 2
name, and nothing else.** No other sheet holds either.

Run again afterwards, three local `dotnet test` steps remain and all three are inside sheets item
1 has marked as records:

```
steps/kpi-create.md:23     steps/kpi-scan-2.md:23     steps/kpi-templates.md:23
```

**They stay on purpose.** The round's own words for those five are that nothing else in them
moves, so their steps stay as the record of that day, and the line at the top of each now tells a
reader to use the current sheet instead. **No stated test count is left in any sheet at all.**

**AND THOSE SAME THREE STILL CARRY ROUND 94'S SENTENCE UNDER THEIR BUILD STEP**, at
`kpi-create.md:37`, `kpi-scan-2.md:37` and `kpi-templates.md:37`, saying the test step above them
wants the .NET 10 SDK. **That is still true there and is left alone for that reason**: their step
above really does run the tests on this PC. The sentence was false only in the three sheets whose
step changed, which is where it was replaced. Said here because a grep for that sentence returns
three hits and the next reader should not take them for a miss.

### Requests for Bader

- **`.claude/rules/kpi-rules.md:6` still triggers on `steps/kpi-templates.md`**, which is now
  marked as a record. Pointing it at `steps/2026-09-16-kpi-checks.md` instead is one line, and
  `.claude/` was not in this round's scope
- **`steps/2026-09-13-view-filters.md` is the earlier of the two View Filters sheets** and is not
  marked as a record, because that is the View Filters task's call rather than a KPI round's.
  Marking it would be the same one line the five KPI sheets took
- **`steps/run-drawing.md` is older than the Drawing Sheet task's latest round**, by its own
  opening against that log's top entry. Whether it needs a new sheet is that task's to decide

### What the claim checker flagged

The agent in `.claude/agents/claim-checker.md` read this entry before the pull request was
opened. It had Read, Grep and Glob and no shell, said so at the top of its report rather than
answering anyway, and marked every command it could not run as unverified instead of inferring
it.

**IT FOUND ONE CLAIM WRONG AND IT WAS A GOOD CATCH.** The entry said the sweep for the five sheet
names turned up a single hit outside the two KPI records. It re-ran the sweep and found a second,
`steps/log-drawing.md:1033`. **The sweep this entry was written off had piped that file out, and
the sentence then did not say so**, which is a filter dressed up as a result and is the shape
this repository keeps meeting. The section above gives all four files and their counts now, and
says which was excluded and why the number was wrong.

**It also flagged this heading as an empty placeholder**, which it was, in an otherwise filled
entry. That is the same fault the ninety second pass left behind once already.

**THE FOUR THINGS IT HAD NO SHELL FOR WERE RE-RUN HERE AT THIS COMMIT**, and the two diff claims
with them:

```
the solution build            0 Warning(s), 0 Error(s)
the suite and the KPI filter  2139 passed, 1319 passed
bash .claude/hooks/hook-tests.sh   28 passed, 0 failed
git diff --name-only f0dc5e0 HEAD, outside steps/   none
git diff over the five older sheets   15 added lines, 0 deleted, and every one of
                                      them is the three line record marker
```

That last one is what backs nothing else in the five having moved, rather than a reading of the
files as they now stand.

**Everything else it checked came back backed**, ten line citations among them, and it recounted
the audit files itself with Grep alone, 29, 20, 14 and 17 findings against 8, 9, 2 and 4 FIXED
marks, catching the known false positive in `audit-kpi-4.md` by eye. That is the 80, 23 and 57
this entry opens with. It also confirmed that the entry's scope on round 94's sentence is
correct, that `steps/2026-09-16-kpi-checks.md` holds no `dotnet test` line, and that
`run-drawing.md` is the only Drawing Sheet run sheet in `steps/`.

---

## 2026-09-16, ninety fourth pass. Every other place that still built the solution

**Merged to main as `a068c8f`**, pull request 156, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**Nothing the tool does changed**, and nothing under `src/` changed but one comment, so the
counts are the ninety third pass's and stand unmoved: **2139 tests, 1319 of them KPI**, 28 hook
cases, build zero warnings. **80 audit findings, 23 FIXED, 57 open**, untouched, with nothing
closed, renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody.

The ninety third pass moved one run sheet's build step off `RcrcGreen.sln`, because the solution
holds `tests/RcrcGreen.Core.Tests` on `net10.0` and a PC with no .NET 10 SDK cannot build it.
That round then listed everywhere else still saying otherwise and left them for Bader. This is
those places.

### The install script's two refusals

`install/install.ps1:57` and `:63` both ended `Build RcrcGreen.sln in $Configuration first.`,
which is the script's own answer when its output folder is missing or short of a file. **They
sent a person straight at the build that fails.** Both name
`src\RcrcGreen.Revit\RcrcGreen.Revit.csproj` now, with the same `$Configuration`, and nothing
else in the script moved.

**WHETHER IT STILL PARSES IS UNKNOWN AND IT IS UNKNOWN FOR A REASON.** Neither `pwsh` nor
`powershell` is on this machine, both checked with `command -v` and both NOT FOUND, so
PowerShell's own parser could not be asked. **Writing a second parser here to answer would be
two records of one fact**, which is the shape this repo keeps paying for, so the question is left
open rather than answered by something that is not PowerShell. What IS backed is the diff: the
change sits inside the two double-quoted strings, one sentence swapped, and `$BuildOutput`,
`$Configuration` and `$($missing -join ', ')` are all untouched.

### The eight older run sheets

Each built the whole solution. Each now builds the add-in project, keeping the `-c Release` each
already used, and each step keeps its number and its one action:

```
steps/2026-09-13-colour-box.md:37     steps/kpi-create.md:28
steps/2026-09-13-view-filters.md:36   steps/kpi-scan-2.md:28
steps/2026-09-15-kpi-fixes.md:40      steps/kpi-templates.md:28
steps/2026-09-15-kpi-rerun.md:69      steps/run-drawing.md:30
```

**ALL EIGHT STEP TITLES MOVED, AND SIX OF THEM SAID SOLUTION.** Counted off
`git diff 3467af6 HEAD -- steps/` rather than off memory. The six were `Build the solution`,
`Build the solution in Release` and `Build the whole solution in Release`, each sitting over a
command that now builds one project. The other two read `Build it in Release` and named nothing,
so they moved for the same reason step 7 of the 16 September sheet did: a title that does not say
what the command builds is the next reader's guess.
**No sheet of the eight names Visual Studio anywhere**, checked with a grep over all eight before
anything was changed, so the line item 2 allows for that case had nothing to act on.

**`steps/` IS COMMON GROUND, so `run-drawing.md` and `2026-09-13-view-filters.md` are in scope
even though the tools they drive are not this task's.** `.claude/rules/territory.md` puts the
fence around `src/` and `tests/` per task and names `steps/` among the common files a commit may
carry beside one task's work. Said here because a KPI round editing the Drawing Sheet's own run
sheet is worth somebody being able to check rather than discover.

### Where this round departed from its own instruction, and why

The round asked for the added sentence in the same words as step 7 of
`steps/2026-09-16-kpi-checks.md`, which ends `so this PC does not need the .NET 10 SDK at all`.
**Measured before writing it: six of the eight sheets run `dotnet test` on the test project one
step ABOVE the build**, and that command fails the same way the solution build does, exit 1 with
`error NETSDK1045`, which was run here rather than reasoned:

```
steps/2026-09-13-colour-box.md:29    steps/kpi-scan-2.md:20
steps/2026-09-13-view-filters.md:28  steps/kpi-templates.md:20
steps/kpi-create.md:20               steps/run-drawing.md:22
```

So step 7's sentence is true in step 7, where no local test run is asked for, and **false in
those six**. It is written with one word changed, `this build` rather than `this PC`, and a
second sentence naming the test step above as the thing that does still want the SDK. **A run
sheet carrying a line its own step above disproves is the fault this repo's front door already
names**, so the alternative was writing something measured to be wrong.

**AND THE OTHER TWO SHEETS TOOK THE ORIGINAL WORDING, BECAUSE IN THEM IT IS TRUE.**
`steps/2026-09-15-kpi-fixes.md` and `steps/2026-09-15-kpi-rerun.md` ask for no test run at all,
grepped for and found to hold no `dotnet test` line other than the one this round added. The
first draft of this round put the six sheets' clause into those two as well, which would have
sent a reader looking for a step above that is not there. It was caught by grepping for the test
step rather than assuming all eight had one.

### CLAUDE.md's own build line

`CLAUDE.md:37` said to build `RcrcGreen.sln` in Visual Studio 2026 and then run the install
script. It says to build `src\RcrcGreen.Revit\RcrcGreen.Revit.csproj` in Release and then run
`.\install\install.ps1`, and it says the solution also holds `tests/RcrcGreen.Core.Tests`, which
needs the .NET 10 SDK, so the solution build is the gate's rather than an install's. Nothing else
in the file moved.

### The stale C# number in Core

`src/RcrcGreen.Core/RcrcGreen.Core.csproj:8-9` said the test project that consumes Core gets
C# 12. **Read off the SDK here rather than assumed**, through
`dotnet msbuild tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj -getProperty:LangVersion`
on SDK `10.0.401`: it answers **14.0** against `TargetFramework` `net10.0`. The comment says 14
now, and says that 14 is the SDK's own default and that the test project declares no
`LangVersion` of its own, which is why it moved when the target did and why the number went stale
with nobody editing the file.

**COMMENT ONLY, AND MSBUILD STILL ANSWERS THE SAME THREE THINGS**, asked after the edit:
`TargetFramework` netstandard2.0, `LangVersion` 12.0 and `AssemblyName` RcrcGreen.Core. The
solution builds with zero warnings and the suite passes 2139.

### What is left anywhere in the repo

Swept for after the edits rather than assumed. Three places still name a solution build and
**every one of them is correct as it stands**:

```
.github/workflows/tests.yml:51   the gate, which installs 10.0.x at :45, so the test project
                                 is exactly what it is there to build and run
steps/audit.md:107               an audit finding describing that workflow line, a record, and
                                 a DIFFERENT file from the four the 80 is counted off
steps/2026-09-16-kpi-checks.md:95  the sentence saying why step 7 used to
```

**No live instruction to a person now points at a build that needs an SDK they may not have**,
except the six test steps named above.

### Requests for Bader

- **THE SIX LOCAL TEST STEPS ARE THE SAME FAULT ONE STEP UP**, at the lines listed above. Each
  tells a reader to run `dotnet test` on a `net10.0` project, which stops with `NETSDK1045`
  without the SDK. This round was given the build lines, so the test lines are untouched and the
  six sheets say plainly which step wants it. Whether those steps should go, or say the SDK is
  needed for them, or point at the gate instead, is a decision rather than a correction, and it
  is the natural next round
- **`steps/2026-09-13-colour-box.md:32`, `2026-09-13-view-filters.md:31` and `run-drawing.md:25`
  each name a test count off main as it was**, 1578, 1569 and 1103 against today's 2139. They are
  records of what those runs carried, so they are left, and they are named here because a reader
  following one of those sheets today will see a number that no longer matches

### What the claim checker flagged

The agent in `.claude/agents/claim-checker.md` read this entry before the pull request was
opened. **It found nothing wrong in what it could check**, and it could not check six things,
because its tool set was Read, Grep and Glob with no shell. It said so at the top of its report
rather than answering anyway, and it named the six.

**IT DID CATCH ONE THING BY NOT BEING ABLE TO SEE THE DIFF, WHICH IS WORTH MORE THAN THE SIX.**
It reported that none of the eight sheets holds the word solution anywhere today, and called that
corroboration rather than proof, since it could not run `git diff`. Run here: **all EIGHT step
titles moved, not six.** The entry said six, which is true of the ones that said solution and
silent about the two that read `Build it in Release`. The paragraph above says eight now and says
which six were which.

**And it drew a distinction this entry was loose about.** The 80 findings come off the four
`audit-kpi*.md` files, and `steps/audit.md`, cited further up for its line 107, is a separate
file with its own findings that are no part of that count. The sweep table says so now.

**THE SIX IT COULD NOT RUN WERE ALL RE-RUN HERE AT THIS COMMIT.**

```
dotnet test on the system SDK 8.0.130      exit 1, error NETSDK1045 naming .NET 10.0
command -v pwsh and powershell             both NOT FOUND, so the parse check stays UNKNOWN
dotnet --version with the .NET 10 SDK      10.0.401
the test project through msbuild           TargetFramework net10.0, LangVersion 14.0
Core through msbuild after the edit        netstandard2.0, LangVersion 12.0, RcrcGreen.Core
the solution build                         0 Warning(s), 0 Error(s)
the suite and the KPI filter               2139 passed, 1319 passed
bash .claude/hooks/hook-tests.sh           28 passed, 0 failed
```

**Everything else it checked came back backed**, every one of the eight build lines, the six test
steps, the three stale counts, `install.ps1:57` and `:63` with their interpolations intact,
`CLAUDE.md:37`, the csproj comment, the two workflow lines, `steps/audit.md:107` and
`steps/2026-09-16-kpi-checks.md:95`. It also recounted the audit files by hand, including the
known false positive in `audit-kpi-4.md`, and got 29, 20, 14 and 17 findings against 8, 9, 2 and
4 FIXED marks, which is the 80, 23 and 57 this entry opens with.

---

## 2026-09-16, ninety third pass. The build step in the run sheet, and two stale lines

**Merged to main as `31b4d3d`**, pull request 154, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**Nothing the tool does changed.** No file under `src/` or `tests/` changed except one comment,
so the counts are the ninety second pass's and are reported here as unmoved rather than as new:
**2139 tests, 1319 of them KPI**, 28 hook cases, build zero warnings. **80 audit findings, 23
FIXED, 57 open**, untouched, with nothing closed, renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody.

### The run sheet told Bader to build something his PC cannot build

`steps/2026-09-16-kpi-checks.md` step 7 built the whole solution. The ninety second pass moved
`tests/RcrcGreen.Core.Tests` to `net10.0`, and `RcrcGreen.sln` holds that project, so on a PC
carrying no .NET 10 SDK the solution build fails and step 7's own last sentence says to stop
there and send the output. **The tests are the gate's job and they never needed to run on that
PC at all.**

**MEASURED HERE RATHER THAN REASONED.** This machine carries two SDKs in two places, the system
one at `/usr/bin/dotnet` reading `8.0.130` and nothing else, and a .NET 10 SDK in a folder of its
own that nothing reaches unless it is put on the path. So a PC without the .NET 10 SDK is
reproducible exactly, and both halves were run with the system one:

```
dotnet build RcrcGreen.sln -c Release
  error NETSDK1045: The current .NET SDK does not support targeting .NET 10.0.
  [/home/user/RCRC-Green/tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj]
  Build FAILED.   0 Warning(s)   1 Error(s)

dotnet build src/RcrcGreen.Revit/RcrcGreen.Revit.csproj -c Release
  RcrcGreen.Core  -> src/RcrcGreen.Core/bin/Release/netstandard2.0/RcrcGreen.Core.dll
  RcrcGreen.Revit -> src/RcrcGreen.Revit/bin/Release/RcrcGreen.Revit.dll
  Build succeeded.   0 Warning(s)   0 Error(s)
```

**AND EVERYTHING `install.ps1` COPIES OUT OF A BUILD COMES OUT OF THAT ONE.** Traced by line
before the step was changed, because a run sheet that installs half an add-in is worse than one
that stops.

```
install.ps1:50   $BuildOutput = src\RcrcGreen.Revit\bin\$Configuration, the add-in project's
                 OWN output folder, with no target framework subfolder because
                 RcrcGreen.Revit.csproj:9 sets AppendTargetFrameworkToOutputPath false
install.ps1:53   RcrcGreen.addin, copied at :74, put there by RcrcGreen.Revit.csproj:50-52
install.ps1:54   RcrcGreen.Revit.dll, copied at :78, the add-in project's own assembly
install.ps1:54   RcrcGreen.Core.dll, copied at :78, put there by the ProjectReference at
                 RcrcGreen.Revit.csproj:14
install.ps1:84   RcrcGreen.Revit.pdb and RcrcGreen.Core.pdb, optional, same two projects
```

Everything else it writes comes from somewhere that is not a build at all: `:110`
`title-blocks.txt`, `:119` `sheet-names.txt`, `:128` `presets.txt` and `:137` `ViewFilters.json`
are copied out of `install/` in the repo, and `:97-102` `reports-folder.txt`, `:143-147`
`templates-folder.txt` and `:152-156` `kpi-output-folder.txt` are written by the script itself.
**No line of it reads `tests/RcrcGreen.Core.Tests` or anything only the solution build
produces.**

**And the folder was listed after that build rather than reasoned about.** It held five files,
`RcrcGreen.addin`, `RcrcGreen.Revit.dll`, `RcrcGreen.Core.dll`, `RcrcGreen.Revit.pdb` and
`RcrcGreen.Core.pdb`, which is every file the script reads from it and nothing else.

So step 7 builds `src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release` now. **One action and
the same step number**, plus the sentence the round asked for: the tests run on the GitHub test
gate on .NET 10, so this PC does not need the .NET 10 SDK. The error text is quoted in the step,
so somebody who runs the old command by habit recognises what they are looking at.

**THE HEADING AND HALF THE LEAD SENTENCE DID MOVE, and the first draft of this entry said they
had not.** Read off the diff rather than off memory: `## 7. Build it in Release` became
`## 7. Build the add-in in Release`, and the lead sentence keeps `The install script reads
src\RcrcGreen.Revit\bin\Release` and then says that is the project to build where it used to
say Release is what has to be built. **The number is what the round asked to keep and it is
kept.** The heading had to move with the command, because a step called Build it in Release over
a command naming one project is the shape this repo keeps paying for.

### The two lines that still said net8.0

`CLAUDE.md:54` read that `tests/RcrcGreen.Core.Tests` is net8.0 and the comment at
`src/RcrcGreen.Core/RcrcGreen.Core.csproj:11` read that Core is consumed by net8.0 tests. Both
read net10.0 now, one word each, and nothing else in either file moved. They were named as
requests for Bader in the ninety second pass's entry, because neither file is this task's to
edit, and this round has his line for these two lines only.

**THE CSPROJ CHANGE IS INSIDE A COMMENT AND THE BUILD IS UNCHANGED**, asked of msbuild rather
than eyeballed. Line 11 opens `<!--` at line 11 and closes `-->` at line 12, so no element and no
property is touched, and after the edit the project still answers `TargetFramework`
netstandard2.0, `LangVersion` 12.0 and `AssemblyName` RcrcGreen.Core. The whole solution builds
with zero warnings and the suite passes 2139.

### Requests for Bader

Both are lines this round made stale or found stale, in files it may not edit.

- **`install/install.ps1:57` and `:63` both end `Build RcrcGreen.sln in $Configuration first.`**
  Those are the two refusals the script prints when the output folder is missing or short of a
  file, and they now name a build the run sheet no longer asks for. The remedy is one string in
  two places, `Build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj in $Configuration first.`, and
  `install/` is nobody's task folder so it is not this session's to make
- **`src/RcrcGreen.Core/RcrcGreen.Core.csproj:8-9` says the test project gets C# 12.** Measured
  through msbuild: on `net10.0` it resolves `LangVersion` to **14.0**, and Core is pinned to
  **12.0** by line 10. **The 14.0 is the SDK's own default and is declared in no file here**,
  because the test project sets no `LangVersion` at all, which is exactly why it moved when the
  target did and why the comment went stale without anybody editing it. The comment's point still holds, which is that Core would fall back to
  C# 7.3 while the project consuming it gets a modern C#, and only the number is one version
  stale. The round allowed line 11 alone, so lines 8 and 9 are left as they are
- **EIGHT OTHER RUN SHEETS IN `steps/` CARRY THE SAME LINE THIS ROUND FIXED**, found by asking
  the repository for it rather than by remembering. Each one tells whoever runs it to build the
  solution, and each one now fails the same way on a PC with no .NET 10 SDK:

```
steps/2026-09-15-kpi-rerun.md:67      steps/2026-09-13-view-filters.md:36
steps/2026-09-13-colour-box.md:37     steps/kpi-create.md:28
steps/kpi-scan-2.md:28                steps/2026-09-15-kpi-fixes.md:38
steps/run-drawing.md:30               steps/kpi-templates.md:28
```

  **They are NOT changed here and that is deliberate.** This round named one file and one step,
  several of those eight are the record of a run that has already happened rather than a sheet
  anybody will follow again, and rewriting a record of what somebody did is a different thing
  from fixing an instruction. Which of the eight are live is Bader's call. **`run-drawing.md` is
  the Drawing Sheet task's**, so it is another session's file either way
- **`.github/workflows/tests.yml:51` builds the solution and is RIGHT as it stands.** The gate
  installs `10.0.x` at `:45`, so the test project is exactly what it is there to build and run.
  Checked rather than assumed, because the fix above would be wrong applied there

### What is UNKNOWN

**Whether Bader's PC carries a .NET 10 SDK is UNKNOWN from here.** The change makes the step
work either way, and it is what makes the question stop mattering. The failure above was
reproduced on this machine's own .NET 8 SDK rather than measured on his.

### What the claim checker flagged

The agent in `.claude/agents/claim-checker.md` read this entry before the pull request was
opened. **It had Read, Grep and Glob and no shell**, so it could run none of the builds, none of
the tests, no `git diff` and no `dotnet msbuild`, and it opened its report by saying so rather
than answering anyway. That is the right answer, and it is why the heading it found empty is this
one.

**It found one real fault and it was in this entry rather than in the change.** The entry claimed
step 7 kept its lead sentence, which is a claim about the file's prior state and nothing it could
read can see. Checked here against `git diff 2e968dc HEAD`: **the heading and the second half of
the lead sentence both moved**, and only the step number stayed. The section above says so now
and says what each one reads.

**It sharpened one thing that was true and under said.** The C# 12 in
`RcrcGreen.Core.csproj:8-9` resolves to 14.0 on `net10.0`, and it pointed out that 14.0 is
declared in no tracked file, because the test project sets no `LangVersion` of its own. That is
exactly how the comment went stale with nobody editing it, and it is written into the request
above.

**And it flagged the empty heading this section used to be**, a placeholder left behind, which is
the fault the ninety second pass warned about one shape along: a heading promising something
nobody wrote.

**Six things it marked UNBACKED were measurements it had no tool to reproduce, and every one was
re-run here at this commit rather than waved through.** The two build transcripts, on the system
`8.0.130` SDK, which is a PC with no .NET 10 exactly: the solution build exits 1 with
`NETSDK1045` naming `tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj`, and the add-in
project build exits 0 with 0 warnings and 0 errors. The output folder, listed afterwards, holds
**five files and nothing else**, no `.deps.json` and nothing out of the two Revit packages, which
is the question it asked. The msbuild properties, Core answering `netstandard2.0`, `12.0` and
`RcrcGreen.Core`, and the test project answering `net10.0` and `14.0`. The counts, **2139** and
**1319** under the KPI filter, **28 hook cases**, and a build at zero warnings. And the
footprint, `git diff 2e968dc HEAD` over `src` and `tests`, which is one file, one line, inside a
comment.

**Its own independent count of the audit files agrees**, 29, 20, 14 and 17 findings against 8, 9,
2 and 4 FIXED marks, and it caught the known false positive in `steps/audit-kpi-4.md` by eye, the
way the entry before this one asked the next reader to. It counted the 28 hook cases by hand off
the script as well.

---

## 2026-09-16, ninety second pass. Five fixes and one rule removed

**Merged to main as `133b8b0`**, pull request 152, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2139 tests, 1319 of them KPI**, up from 2111 and 1291, 28 hook cases, build zero warnings, and
the tests now run on .NET 10. **80 audit findings, 23 FIXED, 57 open**, counted off all four
files, three closed and nothing renumbered or reordered.

**THE 16 SEPTEMBER REPORTS AND WORKBOOKS ARE NOT IN THIS REPOSITORY AND DID NOT ENTER IT.** Every
measurement below is copied out of them into prose, and no client file of any kind was written
here.

**NOTHING IN THIS ROUND WAS RUN IN REVIT**, by this session or by anybody. The Revit half of item
5 cannot be run from this session at all and it is said again beside the tests that cover its
Core half.

### The total canopy check no longer switches itself off

`WorkbookArithmetic.WhyTheTotalIsNotAdded` returned an empty reason when it was handed no
columns at all, and again when `TotalCanopyColumns.For` found none for that sheet, and
`SpeciesList.UsableEmptyRows` then offered every empty row. **The line numbers the round message
gave, `WorkbookArithmetic:546` and `:549` and `SpeciesList:162-165`, are where those returns sat
BEFORE this round**, and the fix's own comments moved them: the first is `:555` today and the
second, now behind the green cover flag, is `:561`. So a template whose chain could not be
followed got its green cover and its canopy
percentage written with no total canopy check made at all, every empty row still offered, and the
chain's own reason printed nowhere. **A guard that switches itself off reads exactly like a guard
that passed**, which is the shape this repo has paid for before, and today's seven templates read
fine while the team is editing them, which is exactly when it costs something.

Four rules, all four tested.

Where the template HAS a Total Green cover cell and the column cannot be read, a plot holding a
count on that sheet gets Total areas to be greened and the canopy percentage BLANK, the reason
names the template, the sheet and `TotalCanopyColumn.Why`, and READY reads NO through the sourced
blank box rule it already follows. No empty row of that sheet is usable and the species is named
with why, through `SpeciesList.TotalCanopyUnreadable`. **A template with NO green cover cell still
refuses nothing**, which is Bader's decision and the one case unchanged. And the glance names
every such template, once for the press rather than once per plot, through
`UnreadableCanopyColumns`.

**THE TWO CASES ARE TOLD APART BY A FLAG AND NEVER BY THE WORDS.**
`TotalCanopyColumns.NoGreenCoverCell` is its own constant and
`TotalCanopyColumn.TheTemplateNamesNoGreenCover` its own property, because a signal that travels
in the data is not a signal and a reason that merely talks about a green cover cell would read as
one.

**Break watch.** The whole `!column.Found` branch put back as a bare `return string.Empty;`.
`ATemplateWhoseCanopyCellIsTypedBlanksTheGreenCoverAndNamesTheReason` went red alone, reading
`GRP_-_KPI_Checklist_-_DD_MOSQUES.xlsx has a typed number in its canopy cell ... and the check
agreed anyway. That is a guard switching itself off ... What it said: nothing at all`, which names
the template. Restored byte for byte, md5 `39e3ea52603960f6320ee52a1412c205`.

### Two UID2 values that differ only in letter case are one folder

`SharedUid2.Of` grouped on `StringComparer.Ordinal` and `SharedUid2Group.Colliding` compared the
paths only INSIDE each group, so `ANH-007-ST-100213` and `anh-007-st-100213` landed in two groups
and their two paths were never held against each other. **`SharedUid2.cs:191` and `:133` are
where the round message found those two**, before this round, and they read
`StringComparer.OrdinalIgnoreCase` at `:212` and `:138` today. **Windows files them under
one name**, so the second workbook written replaces the first and nothing says a word.

The grouping is `StringComparer.OrdinalIgnoreCase` now, which is how Windows compares a path and
is the comparison `FilePaths.Compare` already makes, and the group carries the FIRST spelling seen
so the path it prints is a real one. **Each line names every plot's UID2 exactly as that plot
carries it**, through `NamedWithValues`, because printing one spelling for a group holding two is
what hid this: somebody searching the model for `anh-007-st-100213` finds nothing where the line
says `ANH-007-ST-100213`. A group whose spellings differ only in case says so in its own sentence,
through `WhereTheSpellingsDiffer`, because two plots carrying one value and two plots carrying two
spellings of one value are different things to go and fix.

MM-01 with `ANH-007-ST-100213` and MM-09 with `anh-007-st-100213` in one component folder is the
test. Both write nothing and each names the other with its own spelling.

**Break watch.** The Ordinal grouping restored.
`TwoUid2ValuesDifferingOnlyInCaseAreOneFolderAndBothAreStopped` went red naming MM-01 and MM-09.
Restored byte for byte, md5 `5d8600e5dbe2d52a5661c62a3ebf550e`.

**Five assertions in `SharedUid2Tests` and one in `PlotReadyTests` moved with the wording** and
each was rewritten by hand with the new sentence written out, with a comment saying the subject
moved rather than the test being bent to the code.

### The templates' own tree lists are read once at the press, cell by cell

**NONE OF THE 15 SEPTEMBER TEMPLATE EDITS SHOWED UNTIL A PLOT HIT A BAD ROW.** Measured in the 16
September workbooks: `L85`, `L88` and `L90` to `L101` still typed on EXISTING PARKS and STREETS
Tree List - Existing, `M83` empty on EXISTING PARKS and FUTURE PARKS, `O90` to `O94`, `O96` to
`O100`, `N85` and `N88` to `N101` empty on all seven, `L101` typed on MOSQUES, `L84` to `L92`
deleted on both park templates' Tree List - Proposed, and the Native and Adaptive SUMIFs on the
first tab stopping at row 91 against a proposed list ending at 92 and at row 95 against FUTURE
PARKS' existing list ending at 101. A press over 154 plots lands on a different set of those every
time the model changes.

`TreeListCheck.In` reads both tree lists of each ticked template ONCE, at the press, before any
plot is written. Seven questions per sheet: rows the total reaches whose canopy cell is typed or
missing, rows whose total canopy cell is missing, rows whose total water cell is missing, named
rows whose water per tree is empty, empty rows carrying neither the canopy formula nor the total
canopy one, names held on more than one row, and formulas on the tree lists or the first tab
reading a range that stops before the list's last named row.

**EVERY COLUMN IS READ OFF THE FILE AND NEVER FROM A LETTER.** The canopy column is the column a
canopy formula really sits in on that sheet, the total canopy column comes off the canopy total's
own chain through `TotalCanopyColumns`, and the water pair comes off the sheet's own formulas, a
column whose rows are another column multiplied by the count and which is not the canopy pair.
**Nothing in the file holds N or O.** A column that cannot be read is named as NOT READ and the
sheet is not clean, because a check nobody made and a check that passed read the same in a count.

**IT IS A REPORT SECTION AND IT STOPS NO WRITE.** The glance gets one line per template, clean or
how many cells, and `THE TEMPLATES' OWN TREE LISTS, CELL BY CELL` carries the cells grouped by
which of the seven named them.

**One bug of my own was caught writing the tests and it was a real one.** Question 2 was handing
`IsTheTotalCanopyFormula` the DIAMETER column where the CANOPY column belongs, so it would have
built `IF(ISBLANK(B85)," ",J85*B85)` and named EVERY row of every sheet as missing its total
canopy cell. The canopy column is read once per sheet now and both questions ask it, and a sheet
no row of which carries a canopy formula says so as NOT READ rather than skipping both questions
in silence. `Wanted` also held a bare `"L"`, which is the letter rule this file exists to enforce,
and it takes the column the caller read off the file.

**Where the fixture departs from the round's own example, and why.** The round named row 84 as
both an empty row carrying no canopy formula and one of the two rows holding one name. **A row
cannot be both**: `SpeciesList` reads the list down column D until the names stop, so a row
holding a name is never an empty row. The duplicate keeps rows 22 and 84 as measured and the
empty row moves to 93, the first row past the fixture list's last name. Every other cell is the
measured one, `L85`, `M83`, `O90`, `N88`, and the SUMIF reads rows 3 to 91 of a list ending at 92.

**Break watch.** The typed canopy category dropped. Four cases of `TreeListCheckTests` went red
and the first of them reads `Expected: ["L85", "M83", ...] Actual: ["M83", ...]`, which names
L85. Restored byte for byte, md5 `22a321e453f9f7631f2ed4507345129a`.

### .NET 10, audit 4 finding 71

**Read off Microsoft's own page this session**,
`https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core`, last updated 8 September
2026:

```
.NET 10   released 11 November 2025   patch 10.0.12 of 8 September 2026   LTS   Active
          supported to 14 November 2028
.NET 9    released 12 November 2024   patch 9.0.20                        STS   Maintenance
          ends 10 November 2026
.NET 8    released 14 November 2023   patch 8.0.31                        LTS   Maintenance
          ends 10 November 2026
```

So .NET 10 is the current LTS. `tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj` takes
`net10.0` and `.github/workflows/tests.yml` takes `10.0.x`. The Revit add-in stays on `net48`,
which is what Revit 2024 loads, and Core stays on `netstandard2.0`, which is a surface rather than
a runtime. The 10.0.401 SDK was installed here and the whole suite was built and run on it before
the gate ever saw it.

**THERE IS NO BREAK WATCH FOR A VERSION MOVE.** Nothing about what any rule does changed, so there
is no rule to break and watch go red. The green run on the new version is the whole of the
evidence.

**TWO LINES OUTSIDE THIS TASK'S TERRITORY NOW SAY SOMETHING THAT IS NOT TRUE, AND THEY ARE LEFT
FOR BADER.** `CLAUDE.md:54` reads that `tests/RcrcGreen.Core.Tests` is net8.0, and
`src/RcrcGreen.Core/RcrcGreen.Core.csproj:11` carries a comment reading that Core is consumed by
a net48 add-in and by net8.0 tests. Both are now wrong by one word. CLAUDE.md is common ground and
the Core project file belongs to no task, so neither is touched here.

### Audit 4 findings 65 and 67, the two throws

**65.** `PlotFilteredOn` in `KpiPlotReader` caught `ApplicationException` and
`InvalidOperationException` and returned an empty string from both, and an empty string means the
schedule belongs to NO plot. Three things followed and none was visible: the schedule was skipped
so the plot read as holding no softscape and no shrubs and lawn schedule, the guard that refuses a
plot holding two of a kind could not fire because the schedule was never counted, and the
reconciliation said the schedule listed no species, which is a sentence about the MODEL, while the
workbook went out with that plot's trees missing.

It hands back a `SchedulePlotRead` now. A throw carries on the reading as a refusal naming the
schedule, the exception's type and its message, the way `GuardedRead` beside it already does, so
the plot's files are not written off a half read and READY reads NO with the schedule's name. The
schedule half of the plot list NAMES what it could not read rather than dropping it.

**AND THE PLOT LIST ITSELF CARRIES THEM.** `PlotsInTheModel` takes a third list,
`SchedulesNotRead`, filled by `KpiPlotReader.Plots` off the same read, and
`EVERY PLOT THE TOOL OFFERED` prints it above the counts that are short because of it. **A
refusal never becomes a plot**, which would invent one, and a test says so in those words: the
union still reads DM-11 and DM-12 and the refusal is its own line.

**THE RULE AND ITS WORDS LIVE IN `Core/Kpi/SchedulePlotRead.cs`, WHERE A TEST CAN REACH THEM**,
because the throw itself is Revit's. `SchedulePlotReadTests` is seven cases. **What stays
untested is that the two catches in `KpiPlotReader.PlotFilteredOn` really produce this refusal**,
because nothing in this repository can make Revit's own `ScheduleDefinition` throw. One press on
a model holding a schedule whose definition Revit refuses is what would settle it.

**67.** `SpeciesList`, `LabelledCells`, `StreetReference` and the ninety first pass's
`TotalCanopyColumns` caught `IOException`, `UnauthorizedAccessException` and `InvalidDataException`
and none caught `System.Xml.XmlException`. A template whose `xl/workbook.xml` is fine and whose
tree list sheet part is malformed passes the peek, is recognised, is listed and is offered, and at
the press the parse threw past every catch in `Run` to the catch of everything, so the pane read
that the request failed and was stopped here, naming no template, no file and no plot, **with no
report written at all**. Each of the four catches it now and returns the refusal it already had
words for, naming the file and the sheet, and the rest of the press carries on.

**Break watch.** The `XmlException` catch removed from `SpeciesList`. The first run of it produced
a raw `XmlException` naming no file, which is exactly the fault the finding describes, so the test
was rewritten to catch the throw itself and fail with a message naming `broken.xlsx` either way.
Re-broken, and `AMalformedSheetPartRefusesTheSpeciesListAndNamesTheFileAndTheSheet` went red
naming `broken.xlsx`. Restored byte for byte, md5 `5296c457dc78ce4ace169bde272e2d7d`.

### The pane picture rule is removed

Bader's decision of 16 September. The section `Any round that changes the panel writes an HTML
mockup` is gone from `.claude/rules/revit-commands.md`, lines 168 to 175 at `983c5ad`. **No mockup
is owed for any past round** and everything under `design/` is left exactly as it is.
`CLAUDE.md:305` mentions a mockup in passing rather than as a rule, so it is left alone.

### The audit count

Counted by walking all four files rather than off a note, numbered findings and FIXED marks:

```
steps/audit-kpi.md      findings  1 to 29    29    8 FIXED   21 open
steps/audit-kpi-2.md    findings 30 to 49    20    9 FIXED   11 open
steps/audit-kpi-3.md    findings 50 to 63    14    2 FIXED   12 open
steps/audit-kpi-4.md    findings 64 to 80    17    4 FIXED   13 open
                                             80   23         57
```

**Three were closed and they are 65, 67 and 71.** Nothing else was closed and nothing was
renumbered or reordered. Each FIXED mark says what is not observed in Revit.

### What is UNKNOWN, and what would settle it

**Which templates carry the SUMIF that stops at row 91 is not recorded.** The measurement names
the row and the sheet, Tree List - Proposed, and not which of the seven. The first press over the
seven answers it, because check 25 prints the cell per template.

**Which sheet the empty `O90` to `O100` and `N85` to `N101` cells sit on is not recorded either.**
They are named as being in all seven and the sheet is not in the measurement. The same press
answers it.

**Whether the pane really shows a refusal on a malformed sheet part is not observed.** Nothing
here can be run in Revit.

### What the claim checker flagged

The agent in `.claude/agents/claim-checker.md` read this entry before the pull request was
opened. It reported that it had **no shell tool**, so it could run neither `dotnet test` nor
`hook-tests.sh` nor `md5sum`, and it said so rather than answering anyway. It named four things.

- **The test counts and the hook count it could not run.** It walked `hook-tests.sh` by hand and
  counted 28 cases defined, which matches, and said the pass or fail result still needed a run.
  Both were run here: `2139 passed`, `1319` under the KPI filter, and `28 passed, 0 failed`. The
  entry's counts were stale twice over while the round was still being worked and both were
  corrected against a real run
- **The four md5 sums it could not compute.** Each was taken here with `md5sum` on the restored
  file and compared against the sum taken before the break
- **Citation drift, and it was right.** `WorkbookArithmetic:546` and `:549`, `SpeciesList:162-165`
  and `SharedUid2.cs:191` and `:133` are the lines the round message gave, which is where those
  returns and that comparer sat BEFORE this round, and each fix's own comments moved them. The
  entry named them as current. It names the method now and says which lines are before and which
  are today
- **A false alarm in the grep this entry suggested for counting findings.** `^[0-9]+\. +[A-Z]`
  returns 18 on `steps/audit-kpi-4.md` because line 19, `80. Nine were dropped for having no cost
  to the user`, is prose in that file's own introduction. There are 17 findings, 64 to 80, and
  the table is right. It is written down here so nobody re-runs the mechanical count and corrects
  a number that was already correct

### Requests for Bader

- `CLAUDE.md:54` and `src/RcrcGreen.Core/RcrcGreen.Core.csproj:11` both still say the tests are
  net8.0. They are net10.0. Neither file is this task's to edit
- Whether a row whose total water cell is missing should hold a plot back rather than being a line
  is not decided. It is a line today, the same as every other question in this section

---

## 2026-09-16, ninety first pass. Two presses read back, seven items

**Merged to main as `65f7330`**, pull request 150, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2111 tests, 1291 of them KPI**, up from 2070 and 1250, 28 hook cases, build zero warnings.
**80 audit findings, 20 FIXED, 60 open**, counted off the four files and nothing closed,
renumbered or reordered. Bader ran two presses on NG05 on 16 September, 16:06 with 166 plots
ticked and 16:37 with exactly the 154 listed plots. **Neither report is in this repository and
neither may enter it**, so every measurement below is copied out of them or out of the workbooks
they wrote.

**NOTHING HERE WAS RUN IN REVIT.** The Revit half of item 1, the split of `OneTemplate` into a
read half and a write half, cannot be run from this session at all, and it is said again beside
the tests that cover its Core half.

### Two plots, one folder, and the last one written replaced the others

At 16:37 NS-01 and NS-42 both carried PRX_Plot_UID2 ANH-007-ST-100210, and MM-01 and MM-09 to
MM-15 all carried ANH-007-ST-100213. ONE WORKBOOK PER PLOT gave each group one path, the last
plot written replaced the others, THE PLOT LIST read YES for all ten, and each group took one
street reference row, so MM-01 and MM-09 to MM-15 all read ROW 10 and length 0.06108. At 16:06
three more groups collided, and the empty DM-29 files replaced DM-11's.

**The fix is a change to the SHAPE of `Create` rather than to any rule.** The press read one
template and wrote it before it read the next, so the first template's files had already landed
when the second template's collision became knowable. `OneTemplate` is split into
`ReadOneTemplate` and `WriteOneTemplate` with a private `TemplateReading` between them. `Create`
reads every ticked template, builds the filings, asks `SharedUid2.Of` and only then writes.
**Nothing about reading a plot changed**: the counted groups, the area rule, the held readings,
the progress count and the area unit are still decided once per template, in the read half. **Two
things about the press's SHAPE did move and are named rather than left to be noticed.** The
Adding up progress line now comes after every template has been read rather than after each one,
because that is where the write half begins. And the seconds a run records as its read are taken
at the END of the read rather than when the write loop reaches the plot, so they no longer carry
the template's own label and tree list reads. Both follow from the split and neither changes a
number in a workbook.

`PlotFilings.One` builds the path for the press and `PlotFilings.Of` for the pane, both through
`PlotWorkbookPath.For`, and **`OnePlot` READS its path off the list rather than working one out**.
A check reporting on a path the writer does not use is the shape this repository keeps paying for.

**The file path is what collides, not the value.** A group filed apart across two component
folders is named and every one of its plots still writes. A plot with no UID2 is left out, because
it is refused by its own path already and an empty value shared by five plots is not one value.

**Files already in that folder are named and left where they are**, the same rule the crash row
follows. `AlreadyInThatFolder` in the handler lists them and `SharedUid2.AlreadyThere` says so.

**The pane names the groups before the press and needs no new read.**
`KpiPlotFacts.ReferenceValuesPerPlot` already carries all four plot parameters for every plot off
the same first sheet the component comes from, so item 1.4 is answerable YES rather than UNKNOWN.
`KpiPanel.Uid2On` is the one lookup.

Tests by hand in `SharedUid2Tests`, seven of them, one of which is the mixed group: two plots
colliding with each other and a third carrying the same value into another component folder. Each
plot gets its own line, because a line reading that every one of them is written would be wrong
about two of the three. **Break watch:** `SharedUid2.Of` made to return
an empty list. `TwoPlotsSharingOneUid2WriteNothingAndEachNamesTheOther` went red reading
`MM-01 and MM-09 both carry ANH-007-ST-100213 ... The values this check found: none`. Restored
byte for byte, md5 `9311f7605b66779e2e94ba455fec6e65` both sides.

### A READY column, because four columns of YES is not the question

All 154 rows of THE PLOT LIST read YES four times over at 16:37 with an empty why not. Among them
were the plots whose workbooks replaced each other, the 41 whose Total areas to be greened came
out blank, FP-18, and the plots whose workbooks divide by nought. Bader had the ready list worked
out by hand at 33.

`PlotReady` is the rule and five things decide it: both files written, no tree written nowhere, no
PDF box that HAS A SOURCE left blank, the plot's own PRX_Plot_UID2, and no #DIV/0! in the workbook
check. **A box this tool has no source for does not count against it**, and nothing here holds a
list of which those are, because only the fields `PdfForms` names can reach `PdfOutcome.Blank`.

The shared value is the whole reason where it fires, and the absent workbook and PDF under it are
not said again, because a row naming three consequences of one cause reads as three faults. Every
other reason is named, short, with its box or its cell, joined with a full stop.

A fourth count under the three, `ready:`, and a glance line reading `ready N of M`. A press with no
plot list file says so rather than counting nought of nought.

**`KpiCreateReport.WhyNotOnTheList` is deleted**, and it is the second reason a thing gets
deleted: its whole shape is `PlotReady.For`'s reason list now, branch for branch, so the SHAPE is
gone rather than its last caller. `ReadingFor` went with it as its only caller.
**`KpiCreateReport.NoSoftscapeOnTheList` is KEPT and now has no production caller**, named here the
way the six uncalled members already are. A plot on no softscape schedule has all three tree boxes
blank with `PdfFill.NoSoftscapeRead` as the reason, so READY names it three times over already and
a fourth line would be a fourth record of one fact. Its tests still hold it.

Tests by hand in `PlotReadyTests`, six of them, the #DIV/0! one built on a real patched workbook
rather than a hand made finding, because a test handing the finding in would pass over a check
that found nothing. **Break watch:** the `outcome.Pdf.Blank` loop taken out.
`APlotWhoseGreenCoverBoxIsBlankReadsNoAndNamesTheBox` went red naming HF-01 and the box, and two
more went red with it. Restored, md5 `f82f4cf22a012997265235f6466c6f4c` both sides.

### The division check missed a division by a count

The 16:37 glance said the formula check found no #DIV/0! anywhere in the press. FP-24 and SC-06,
recalculated, show one in Tree List - Existing S70 and one in T70, and 30 plots of that press hold
every existing tree on rows 84 to 101. `S70` reads `IF(TotTrees<1," ",S69/COUNT(B4:B83))` and T70
the same over T69, on all seven templates and on both tree list tabs. The check matched a division
by ONE CELL, so a division by a count over a range was never looked at.

COUNT, COUNTA and SUM over a range are read now, the range counted off the cell states after the
write, and nought is a #DIV/0!. The IF is honoured, so a plot with no trees gets no line. A
division that cannot be worked out is counted as NOT EVALUATED and the glance says how many,
rather than saying none was found.

**A live scoping fault came out with it.** `TotTrees` is defined twice, at workbook level and on
Tree List - Proposed, and `DefinedNames` kept the first by name alone, so the proposed sheet's
target could answer for an existing sheet formula and hold a guard that is not held. It is keyed
on the scope now with `localSheetId` resolved off the workbook's own `<sheets>` order.

Tests by hand in `DivisionByACountTests`, eight of them. **Break watch:** the COUNT match dropped.
`ACountPastTheRangeIsADivideByZeroOnS70` went red naming S70, its formula, row 85 and FP-24 and
SC-06. The first attempt's failure read only `Expected: 2, Actual: 0`, which says nothing about
which cell went unreported, so a leading assertion naming S70 was added and the break watched
again.

### The canopy check must also read the total canopy column

FP-18 has 2 Ziziphus spina-christi on Tree List - Existing row 83. L83 carries the canopy formula
and M83 is empty in the EXISTING PARKS and FUTURE PARKS templates. The PDF said 2,631 m² greened
and 67.68% canopy and the recalculated Excel 2,531 and 63.97%, and nothing warned.

Measured on all seven templates, which closes the open question the ninetieth pass logged: column
M is Total Mature Canopy Area, `=IF(ISBLANK(B85)," ",L85*B85)` on a complete row, Tree List -
Existing M102 is `SUM(M4:M101)`, Tree List - Proposed M93 is `SUM(M4:M92)`, and the first tab's
Canopy Area cell is `'Tree List - Existing'!M102+'Tree List - Proposed'!M93`, at F8 on four
templates and F9 on three.

**No letter is written anywhere.** `TotalCanopyColumns.In` follows the chain off the file: the
green cover cell names the canopy cell, the canopy cell names the two totals, each total's own SUM
range names the column and the rows. The canopy cell is the one of the green cover formula's three
cells the map does not name as planting or as lawn, which is the rule `GreenCoverCell` already
uses, so the two cannot name two different canopies.

An empty row is offered to a new species only when it carries BOTH formulas. A column that was not
read leaves every row usable rather than refusing them all, which is what reddened
`TreeListRowsTests` on the first attempt: those fixture sheets have no diameter column.

**Break watch:** the M requirement dropped. `TheFp18RowLeavesTheGreenCoverBlankAndNamesM83` went
red naming M83.

### The canopy check line printed Excel's string table numbers

The line read `D99 holds 419 and no formula, E99 holds 122 and no formula`, where D99 holds
Prosopis Juliflora. A cell holding a shared string stores an INDEX, and the formula reader kept
the raw `<v>`. Every printed cell text resolves a shared string through `WorkbookPackage.TextOf`
now, read once per file. A cell holding shared string 0 would have printed as a nought, which is
the same fault wearing a number a reader would believe.

**Break watch:** the raw value printed. `ASharedStringCellPrintsItsTextAndNeverItsIndex` went red
naming D99.

### The plots with no planting at all are named

MM-01, MM-06, MM-07 and NS-23 read 0 in every tree, shrub, lawn, water and green cover box at
16:37, because both schedules printed their heading and no rows. Writing 0 is Bader's decision of
15 September and it stands. `NoPlanting` names them in one glance line, off
`ScannedSchedule.BodyRowCount`, which is the same reading `Reconciliation.SchedulesWithABody`
already counts. A plot MISSING a schedule is not this and is named by its own refusal. READY does
not move.

Tests by hand in `NoPlantingTests`, five of them. **Break watch:** the `On` filter replaced with
false. `OnlyThePlotWhoseTwoSchedulesPrintedNothingIsNamed` went red reading `What the check found:
no plot at all` beside MM-01 by name. Restored, md5 `780fa28ebcc3f719e16aa82babe5933d` both sides.

### Ticked plots that are not on the list

At 16:06 the list held 154 plots and 166 were ticked. The twelve extra plots were written, three
of that press's collisions came from them, and neither the pane nor THE PLOT LIST said they were
ticked. `TickingTheList.TickedAndNotOnTheList` is the other direction from `NotOnTheList`, which
is about the MODEL. A line per plot above Create and a `TICKED AND NOT ON THE LIST` block at the
foot of THE PLOT LIST, naming each plot with the workbook and the PDF really written for it.

Tests by hand in `TickedNotOnTheListTests`, five of them. **Break watch:** the pane line dropped.
`ATickedPlotTheListDoesNotHoldIsNamedAboveCreate` went red naming NS-41 and printing the only line
the pane would have shown. Restored, md5 `14fda4f5cc9108999ebd62ebf0588ee7` both sides.

### The two tests that went red on their own, and why each was corrected rather than bent

`PlotListFileTests.TheReportReadsDownTheTeamsOwnListAndCountsWhatWasWritten` and
`TreesNotWrittenTests.ThePlotListRowAndTheGlanceBothNameWhatWasLost` both assert the plot list's
row shape, and the row grew a READY column between PDF and trees not written. Both were corrected
by hand with the new column written out, and both now assert that not one of their plots is ready:
the first three because no PDF was planned beside their workbooks or no workbook was written at
all, and FP-17, FP-20 and FP-21 because they lost 36, 5 and 25 trees. **The subject moved, the
tests were right, and the corrections say which.**

### What the claim checker flagged

The agent in `.claude/agents/claim-checker.md` was run over this entry before the pull request was
opened, with no shell of its own, so it says outright that it could not run the build, the suite,
the hook script or an md5 and treats none of those as passed or failed. It read the code, the
tests, the four audit files and the hook script.

**One claim was WRONG and is corrected.** This entry read `Tests by hand in SharedUid2Tests, six
of them` and the file holds seven. The seventh is the mixed group added late, and the count had
not moved with it. It reads seven now.

**One claim was flagged as not reconciling and the number is REMOVED rather than argued with.**
The round message says the 16:37 press wrote both files for 7 plots whose workbooks had replaced
each other. Item 1 of that same message names ten plots over two shared values, NS-01 with NS-42
and MM-01 with MM-09 to MM-15. Counting every plot in a colliding group gives 10 and counting only
the plots whose own file was lost to a later write gives 8. **Neither reading lands on 7**, the
press report is not in this repository and cannot be, so the count is taken out of all four places
that carried it, here, in `.claude/rules/kpi-rules.md`, in `PlotReady`'s docstring and in
`PlotReadyTests`. The sentence now names the plots without a number. **What 7 counted is a
question for Bader**, and the shape of the rule is unaffected either way.

**Everything else it could check came back backed.** The audit count 80 and 20 FIXED it counted
itself and matched exactly, the 28 hook cases it counted case by case and matched exactly, every
named class, method, constant and test method exists at the name given, `WhyNotOnTheList` and
`ReadingFor` are gone from `src` and `NoSoftscapeOnTheList` is present with no caller in `src` and
still under test, and the run sheet really is seventeen steps and seven checks. Its static count of
the suite came to within one of 2111 and 1291, which it said was the noise of counting attributes
rather than running them. **The suite, the build and the md5 restores were run here and the numbers
above are off those runs.**

### For Bader

`steps/2026-09-16-kpi-checks.md` is the Windows run sheet, seventeen steps to the press and seven
checks after it, one action each.

### Open, and for Bader

**The ten shared PRX_Plot_UID2 values are the model's fix.** The tool refuses them now rather than
writing over itself, and nothing here can give a plot its own value.

**Whether a plot stopped by a shared value should keep its place in the street reference count is
UNKNOWN.** Each of the two groups took one reference row at 16:37, and with no file written the
question does not arise this press. It would arise the moment the team gives eight of the ten
their own UID2 and leaves two sharing.

**Whether READY should count a plot whose only fault is a #DIV/0! is Bader's.** It is counted
against READY here, because a workbook that recalculates with an error is not a file to send, and
it still never stops a write.

**`CLAUDE.md` is not wrong at any line this round touched**, checked against the plot parameter
rules, the schedule rules and the one workbook per plot rule. Nothing there is a request for Bader
today.

**`.claude/rules/revit-commands.md` says any round that changes the panel writes an HTML mockup
under `design/pr-<number>/`, and no round has since pull request 111.** This round changes
`KpiPanel.cs` and writes none either, which is consistent with every KPI round since then and
inconsistent with the rule as written. It is named here rather than answered: either the rule is
the Drawing Sheet panel's alone and should say so, or thirty eight rounds owe a mockup. That is
Bader's, because the rules file is read by every task.

---

