# KPI log

Newest entry first.

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

## 2026-09-15, ninetieth pass. The 154 plot press read back, seven decisions

**Merged to main as `4411821`**, pull request 148, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2070 tests, 1250 of them KPI**, up from 2042 and 1222, 28 hook cases, build zero warnings.
**80 audit findings, 20 FIXED, 60 open**, counted off the four files and nothing closed,
renumbered or reordered. Bader ran the 154 plot press on NG05 on 15 September at 13:32 and 146
workbooks and 146 PDFs were written. **The report is not in this repository and must not be**, so
every measurement below is copied out of it.

**NOTHING HERE WAS RUN IN REVIT.** Item 7's Revit wiring in `KpiRequestHandler` cannot be run
from this session at all, and it is said again beside the tests that cover its Core half.

### A read schedule with no such group writes 0, and an absent schedule still does not

The press left the PDF Lawn box blank on **81 plots** saying the schedule printed no GRASS group,
and the four shrub boxes blank on **15 plots** saying it printed no SHRUBS & GROUND COVER group,
with CELLS NOT WRITTEN naming both. Bader's decision: a schedule that was READ and prints no such
group means the plot has none, so the box is a 0.

`PdfFill.NoGroupIsNought` is the working that travels with it and `PdfFieldFill.NoughtForAnAbsentGroup`
is the flag. **The flag is what the glance counts, never the words**, because a signal that
travels in the printed text is not a signal and this repository has already paid for that once,
and because item 6 rewrote every printed line in the same round.

**AN ABSENCE IS NOT A MEASUREMENT.** A plot holding no shrubs and lawn schedule at all leaves the
five boxes blank through `PdfFill.NoScheduleRead`, which names the schedules found where there is
more than one, so two of a kind and none of a kind are different sentences.

The workbook half is `KpiMerge.Subtotalled`: a plot whose read schedule holds no group now
contributes 0 rather than being skipped, so the Lawn and the Planting cells are written where
they used to be reported NOT FOUND. A plot with no schedule is still left out of `PerPlot`
entirely, which is what `KpiCreatePlan.Number` refuses to write from.

The water demand half is `WaterDemandRead.NothingToTotal`: a schedule printing its heading row
and **no rows under it** counts 0. MM-01, MM-06, MM-07 and NS-23 wrote no Irrigation water demand
at all with both halves empty, and MM-08, NS-28 and NS-38 with the shrubs and lawn half empty.
**A schedule with body rows and no TOTAL row still refuses**, unchanged.

**THE NOUGHT IS ASKED AFTER THE COLUMN HAS BEEN FOUND, on purpose.** A heading row that does not
name L/DAY is a schedule this reader cannot read, and answering 0 for it would be the fall back
to a position this repository forbids everywhere else. Counting 0 before looking at the heading
row at all is the choice not taken, and it is recorded here as one.

Break watch: the blank was put back for a missing GRASS group.
`AReadScheduleWithNoGrassGroupWritesTheLawnBoxNought` went red reading **DM-16's shrubs and lawn
schedule was read and printed no GRASS group, and the Lawn box was left blank rather than
written 0**. Restored, md5 `0e789af1aacfa742902e3f456dc58bb8`.

### A plot with no schedule is not a zero

FM-07 is on a sheet and on no schedule. Its PDF read Existing Trees 0, Proposed Trees 0, TOTAL
trees 0 and Total areas to be greened 0, and the model's own KPI% schedules list **57 trees** for
it. Four noughts went to the team as a measurement of a plot nothing had read.

`PdfFill.NoSoftscapeRead` is the reason and all four boxes are blank now. `Greened` refuses first
too, because the canopy is counted off the tree rows and a green cover computed without one is
short by however many trees the plot holds.

**THE PLOT LIST ROW SAYS THE SAME**, through `KpiCreateReport.NoSoftscapeOnTheList`, even though
both files were written. A row reading YES and YES with an empty last column is the row of a plot
that came out right, and FM-07's was one of those.

Break watch: the counts were let fall back to 0.
`APlotWithNoSoftscapeScheduleLeavesTheTreeBoxesBlankAndNamesWhy` went red reading **FM-07 holds
no softscape schedule and ExistingTrees was written '0'**. Restored, md5 unchanged.

### The Parks boxes are 9.72 pt tall, so the height margin is 1 pt

**271 values were held at 6**, every one of them on `Projects Basic Data - Parks`. The boxes are
41.52 by 9.72 pt for a value, 41.734 by 9.61 for a shrub box and 167.346 by 9.818 for a header.
Open spaces is 46.6 by 19.4 and roads 46.8 by 17.0, both at 10.

`PdfTextFit.HeightMargin` is 1.0 and `Margin` stays 2.0 for the width. A value held at 6 says
whether its **WIDTH** or its **HEIGHT** held it, and **runs over is printed only when the text is
wider than the box at 6 pt**, which is what 271 of those lines were wrongly claiming.

Worked by hand: 3977.16 in Helvetica is six digits at 556 and a full stop at 278, 3.614 em.
41.52 less 4 is 37.52 across, 37.52 over 3.614 is 10.38. 9.72 less 2 is 7.72 down. The smaller is
7.72, floored to **7.7**. The same value in 46.6 by 19.4 gives 10, and 2797.64 in 30 by 14 gives
7.1 unchanged.

`AShortBoxBoundsTheSizeByItsHeight` was corrected by hand from an 11 pt tall box to a 9 pt one,
so its stated 7.0 arithmetic is still true. The expected number was not changed.

### Never write a new species into a row with no canopy formula

On FP-23 the run wrote CONOCARPUS LANCIFOLIUS, 2 trees, into
`GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx`, Tree List - Proposed, **D85 and B85**. That row
holds C85, M85 and O85 and no L85, so the canopy guard blanked FP-23's Total areas to be greened
and its canopy percentage. The guard was right.

`SpeciesList.UsableEmptyRows` is the fix: an empty row the total reaches AND whose own cells carry
the canopy formula. `SpeciesMatching.WrittenInto` queues those, so row 85 is stepped over and row
86 is taken. Where none is usable the species is not written and `SpeciesList.NoUsableEmptyRow`
names the file, the sheet and every empty row it checked.

**ONE RULE FOR THE GUARD AND FOR THE READER.** `WorkbookArithmetic.IsTheCanopyFormula` is the
comparison, asked by the guard that checks a row this run wrote and by the reader that decides
whether an empty row may be written into. Two copies of it would be two answers to one question.

**A SHARED FORMULA IS EXPANDED.** Excel stores a column of one formula as text on the master and
an index on every cell under it, so `SpeciesList` reads the canopy rows through
`WorkbookFormulas.Of`, the same reader the whole check uses. A reader taking the text alone would
have seen the canopy on row 4 and on none of the eighty rows below it.

**A SHEET NAMING NO DIAMETER COLUMN IS NOT CHECKED AT ALL**, and `CanopyRowsRead` says so. There
is no formula to look for, and calling every row unusable would refuse every write on a shape
nobody has measured.

Break watch: `list.EmptyRows` was put back in place of `UsableEmptyRows`.
`ANewSpeciesSkipsTheEmptyRowWithNoCanopyFormulaAndTakesTheNextOne` went red reading **CONOCARPUS
LANCIFOLIUS landed on row 85**. Restored, md5 `95fcec677cced1e366d6ef49322a0e1b`.

### Trees written nowhere show at the top

FP-17 36, FP-21 25 and FP-20 5 AZADIRACHTA INDICA, and FP-23 1 tree named UNKNOWN, all written
nowhere. **THE PLOT LIST read YES and YES for all four and the glance said nothing.** 67 trees
left the building in rows that read exactly like the rows of a plot with nothing wrong with it.

`TreesNotWritten` counts them off the runs' own matches, so the count and the refusal that
produced it are one record. THE PLOT LIST has a **trees not written** column, reading
`36 AZADIRACHTA INDICA`, and the glance carries one line, **THE TREES WRITTEN NOWHERE**, which
says so even when nothing was lost.

`SpeciesMatching.OnMoreThanOneRow` names the file, the sheet and every cell holding the name,
`D22 and D84`, so somebody can open the template at the rows. **The tool still does not choose
between two rows** and will not.

Break watch: the column was dropped from the row. `ThePlotListRowAndTheGlanceBothNameWhatWasLost`
went red naming FP-17's row. Restored, md5 `202b9aabe864162c0d5457ead5b9227f`.

### No printed line says client

Bader: the team reading the report does not know which client is meant. **26 printed strings held
the word and none does now**, across `PdfEmptying`, `RunAtAGlance`, `PdfChecklist`,
`TemplateWords`, `CanopyArea`, `RegionChoice`, `PdfTextFit`, `KpiCreateReport`, `PdfFormCheck`
and `KpiPanel`. Each names the thing instead: the folder, the form file, the template's own list,
the field's own `/DA`.

`RegionChoice.TheNote` is the one record of the note itself, `the area cell's own note, REVIT 00
LINK / ID FILLED REGION RCRC_OUT OF SCOPE (PRESENTATION) / PRX_Intervention Area`, and every line
that used to say the client's note now prints that.

**THE CANOPY GUARD LINE NAMES THE CELL.** It carries the workbook file name, the sheet, the row,
every formula on the row with its text, every cell somebody typed a value into with no formula
behind it, and the one cell that would have carried the canopy: `L85 is empty`, or `L85 holds 50
and no formula`. Which column that is, is read off the sheet's OWN other rows, never off a
letter. `FormulaCheck.TypedCells` is the new reading behind it.

**WHAT M85 IS, IS UNKNOWN AND IS NOT PRINTED.** Nothing in this tool records what the canopy area
column's formula is, so naming that cell would be a rule nobody measured. It is a question for
Bader and one row's M cell settles it.

Break watch: the old sentence was put back. `TheGuardNamesTheFileTheSheetTheRowAndTheCanopyCell`
went red on the missing file name. Restored, md5 `9621e488579eb5402fe57c8df59cb7d5`.

### The crash row

Bader's decision: after a crash the tool leaves every file where it is and the plot's row names
them. `PlotCrash.Row` is the words, `CreateStep` the step and `CrashFile` one file with its path
and whether the disk really holds it, read at the moment the row is built.

**THE FOLDER FLAG READS THE DISK.** It was handed `false` in the catch whatever was on disk, so a
press that made the folder and threw a step later counted one folder fewer than the tree really
holds. `PlotWriteTrail` in `KpiRequestHandler` records the step AS IT IS REACHED rather than
working it out backwards, and the copy and the patch are told apart by the output file's own
existence, read at the patcher's own progress call. **Nothing is deleted.**

**THE REVIT HALF HAS NOT BEEN RUN AND CANNOT BE RUN FROM HERE.** Its Core half has five tests.

### Open questions for Bader

1. **What the canopy area column's formula is** is UNKNOWN. One row's M cell makes the guard line
   name that cell too.
2. **The FUTURE PARKS duplicate row** is the team's fix and THE PLOT LIST is how it is watched.
3. **Rows 85 to 101** are still dead rows on the templates. The tool steps past them now instead
   of writing into them, which is better and is not a fix.

### What was not changed

The `/DR` and `/Font` inline open item stays logged, the press measured every font and came back
with 0 widths UNKNOWN. The component and prefix refusal on MM-09 to MM-15 and FP-27 stays. No
audit finding was closed, renumbered or reordered.

`steps/2026-09-15-kpi-rerun.md` is the sheet for the rerun, 23 steps, the fixes synced and a
fresh detached copy taken first, the seven workbook rows ticked before Tick the list, and six
checks one to a step.

---

## 2026-09-15, eighty ninth pass. The run sheet corrected before the first run

**Merged to main as `346dddb`**, pull request 146, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**No code and no test.** `steps/2026-09-15-kpi-fixes.md` only, plus this entry and the state
entry the commit hooks require. **2042 tests, 1222 of them KPI, unchanged**, 28 hook cases
unchanged, build zero warnings. **80 audit findings numbered, 20 FIXED, 60 open**, unchanged and
not recounted.

**NOTHING HERE WAS RUN IN REVIT.**

### The step the sheet was missing, and it would have cost the whole run

**A PLOT ONLY GOES INTO A WORKBOOK WHOSE ROW IS TICKED AND WHOSE TEMPLATE IS SETTLED.** `Settled()`
at `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:1304` drops every other row and the split at `:1296` runs
over what it hands back, so **a ticked plot belonging to no ticked row is written nowhere**. The
sheet went from Read this model straight to Tick the list and never said to tick the seven workbook
rows, and those ticks live in the pane with nothing writing them down, so closing Revit loses
them. A run made to the sheet as it stood would have ticked 154 plots and written nothing.

It is step 12 now, between Read this model and Tick the list, and it says to settle a row still
asking which of the two park templates it is, because an unsettled row arms nothing.

**AND TICK THE LIST HAS TO BE THE LAST TICK ACTION.** Ticking a template row ticks every plot that
belongs to it and unticking one takes them off, at `KpiPanel.cs:1248`, both of them moving the
plot ticks Tick the list has just set. The step says so, and says to press it again if a workbook
row is touched afterwards.

Every step and every reference to a step number is renumbered. The checks went from four to five.

### The five plots were not a measurement and they are gone

The sheet said DM-21, NS-16, NS-20, ST-23 and ST-24 are in the model and not among the 154.
**Nothing in this repository shows that.** Counted rather than argued: `DM-21` appears here only in
a Drawing Sheet test fixture, `tests/RcrcGreen.Core.Tests/PlotSelectionTests.cs:11` and `:34`,
which is a list written for a test rather than anything read off a model, and NS-16, NS-20, ST-23
and ST-24 appear nowhere in the repository at all. They came out of the round message and the
eighty eighth pass passed them through as fact.

The sheet says UNKNOWN until the run now, and says IN THE MODEL AND NOT ON THE LIST names whichever
plots really are, because that block is read off the model's own plot list at the press.

### A fifth check, on the Parks form

**THE PARKS ROWS SIT ABOUT HALF AS FAR APART AS THE OTHER FORMS' DO.** Measured off
`src/RcrcGreen.Core/Kpi/PdfForms.cs:276` to `:278`: the three tree rows are at y 611.3, 601.0 and
590.6, which is 10.3 and 10.4 apart. The open spaces form's same three rows, `:310` to `:312`, are
at 449.2, 428.9 and 408.9, about twice that.

The height rule takes 2 pt off the top and 2 pt off the bottom, so a Parks box as tall as the gap
to its neighbour leaves about 6.3 pt, barely over the 6 pt floor, and any shorter box lands on it.
**So some Parks vegetation numbers may be named held at 6 while they still sit inside their box.**
The check is to open one EP or FP plot's PDF, read those numbers and send that plot's own lines
under THE TEXT SIZES back.

**WHAT THOSE BOXES ARE REALLY THAT TALL IS UNKNOWN HERE.** The row positions are measured and in
this repository. The heights are not, because no client PDF is in it and none ever will be, so a
height written down here would be a guess. The check is what measures them.

### One list of what to send back

The sheet ends with it, in one message: THE TEXT SIZES and every box line under it, THE PLOT LIST's
three counts and every row reading NO with its reason, the whole dash rows block, and screenshots
of the four PDFs. And the standing rule beside it, that none of those files goes into this
repository.

### Open, logged and not fixed

**1. A CRASH CAN LEAVE A WORKBOOK ON DISK WHILE THE REPORT SAYS NO.** `GuardedWrite`'s catch at
`src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:608` records the plot at `:615` as
`PlotOutcome.WroteNothing(..., false, refusal)`, where the `false` is the folder, while its own
docstring at `:578` says what is already on disk stays there and the folder count still counts the
folder that was made. Those two disagree. And there really can be something on disk:
`WorkbookPatcher` copies the template to the output path at
`src/RcrcGreen.Core/Kpi/WorkbookPatcher.cs:79` and opens the zip to patch it at `:86`, so a throw
between them leaves a whole unpatched copy and a throw inside them a half patched one. The PDF is
written after the run is counted, `KpiRequestHandler.cs:706` to `:720`. `THE PLOT LIST` reads the
plot outcomes at `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:202`, so it prints NO for a plot whose
folder holds a file. **Which of the two is wrong is not decided here.** Not fixed this round.

**2. THE HEIGHT RULE HOLDS THE PARKS VEGETATION BOXES NEAR 6 PT**, as measured above off
`PdfForms.cs:276` to `:278`. Whether that is a real fault or a form whose boxes genuinely are that
small is UNKNOWN until the run, and check 19 is what answers it. Not fixed this round.

**3. FONT WIDTHS ARE READ ONLY FROM A `/DR` AND A `/Font` WRITTEN INLINE.** `FontsIn` calls
`Nested(acroForm, "/DR")` at `src/RcrcGreen.Core/Kpi/PdfFormFile.cs:327` and
`Nested(resources, "/Font")` at `:330`, and `Nested` at `:406` finds the key and then the next
`<<` in the same object. **A form writing either as an indirect reference is not followed**: the
read comes back empty, or it reads whatever dictionary happens to come next in that object, and
either way every width is UNKNOWN and every value keeps the client's own size. A font OBJECT given
as a reference IS followed, so this is about the two dictionaries alone. **No test covers that
shape.** Not fixed this round.

### Files

`steps/2026-09-15-kpi-fixes.md`, this file and `steps/ai-max-state-kpi.md`. Nothing else.

---

## 2026-09-15, eighty eighth pass. Four fixes, and the plot list the team sent

**Merged to main as `088d6d3`**, pull request 144, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2042 tests, 1222 of them KPI, 42 added, against the 2000 main carries** at `64fb57d`, 28 hook
cases unchanged, build zero warnings.

**THE AUDIT COUNT, WALKED OFF ALL FOUR FILES THIS ROUND RATHER THAN OFF A NOTE.** The state file
and the last three log entries said 63 numbered and 44 open, which counted three files and left
out audit 4's findings 64 to 80. From this entry on it is four files:

```
steps/audit-kpi.md     29 numbered   8 FIXED    1 to 29
steps/audit-kpi-2.md   20 numbered   9 FIXED   30 to 49
steps/audit-kpi-3.md   14 numbered   2 FIXED   50 to 63
steps/audit-kpi-4.md   17 numbered   1 FIXED   64 to 80
TOTAL                  80 numbered  20 FIXED   60 OPEN
```

Contiguous 1 to 80 with no gap and no repeat. **Nothing was closed, renumbered or reordered
except marking 66 FIXED**, and the old entries are left alone.

**NOTHING HERE WAS RUN IN REVIT.** Every number below is off a test, off a file in this repository
or off a measurement Bader made. `steps/2026-09-15-kpi-fixes.md` is the run sheet.

### 1. The text was too big for the PDF boxes

**Measured by Bader on ANH-007-MO-100011**, a DAILY MOSQUE plot on the Open spaces form: Area
showed 2797.6 cut off at the box edge, Total areas to be greened showed 0.0008 cut off, and the
tree and shrub counts were drawn taller than their boxes.

**NOTHING IN THIS TOOL HAD EVER READ OR WRITTEN A `/DA`.** `PdfFormFile.WithValue` drops each
field's `/AP` and the AcroForm gets `/NeedAppearances true`, so the viewer redraws the value in
the field's OWN default appearance, which nothing looked at. And the rectangle read took the first
two numbers of `/Rect`, the position the form check compares, so the box's SIZE went past
unnoticed.

`PdfTextFit`, `PdfFontWidths` and `StandardFonts` are new. The rule:

```
across  = the box width less 2 pt each side
down    = the box height less 2 pt top and bottom
em      = the text measured in the font the /DA names
wanted  = the smaller of across / em and down, rounded DOWN to 0.1 pt
ceiling = the client's own size, or 10 pt where their /DA gives nought
floor   = 6 pt
```

**The hand worked case, which is the test:** `2797.64` in Helvetica is six digits at 556 and a
full stop at 278, so 3.614 em. A box 30 pt wide and 14 pt tall leaves 26 pt across, 26 over 3.614
is 7.1948, **written as 7.1**.

**THE VALUE IS NEVER TOUCHED.** Only the size inside the field's own `/DA` moves, in the appended
object. The client's font name and colour go through untouched, NeedAppearances stays, and no
appearance stream is written. Where a field inherits its `/DA`, the inherited string is written
onto that field with the one number changed, because a size is per field.

**A WIDTH IS READ OR IT IS UNKNOWN AND NEVER GUESSED.** The font object's own `/Widths` off the
AcroForm's `/DR /Font` first, then the published metrics of the standard fourteen. **No near miss
is in that table**: Arial and Courier New are metric compatible with Helvetica and Courier and
neither is written in, because a name standing in for a measurement is the fault this repository
keeps paying for. **The standard fourteen tables are held as DATA and are not measured in this
repository**, since no AFM file is here, and that is written down rather than left to be assumed.
What a wrong width can cost is bounded on purpose: the value never changes, the size is never
raised above the client's own, and a width out by a few thousandths moves a size by a tenth of a
point. The read back off the written file is what says which size landed.

**SIX OUTCOMES AND THE BRIEF NAMED FIVE.** An absent `/DA` is the sixth, counted apart, because a
font nobody could measure and a field with no size to change are different facts. `THE TEXT SIZES`
in the glance counts all six and names a box only for held at 6, widths UNKNOWN and no `/DA`.

Break watch: the width bound taken out so the size comes back unchanged reddened 4 of 2042, and
the 7.1 case read `Area, holding '2797.64' in a box 30 pt across and 14 pt tall, came out at 10 pt
and capped at 10.` Restored byte for byte, md5 `c80ef410381ff49c46bccd31759e0fef`.

### 2. Streets, Total areas to be greened

**Bader's decision of 15 September, and it closes the open question.** The Roads note names the
canopy cell where the other two name Total Green cover, and the client meant one number on all
three. `PdfFill.Greened` has one path now and is not asked which form it is.
`PdfFill.RoadsNamesTheCanopyCell` is DELETED, and it is the second reason a thing gets deleted
rather than the first: the shape it recorded is gone, not its last caller.

**The streets template already agreed.** `ComputedPlaces` holds its `D9 = F9+F11+H11`, which is
exactly what `GreenCover.Total` computes, so the Roads PDF and the workbook filed beside it
carried two numbers for one quantity until now.

Hand check off Bader's screenshot of ANH-007-ST-100308: canopy 984, planting 69, lawn empty, Total
Green cover 1,053, and the PDF reads **0.001053** and not 0.000984.

**`TheRoadsFormIsFilledFromItsOwnNoteAndSaysSo` asserted 0.00055 and now asserts 0.00102**, which
is 550 plus 410 plus 60. **It changed because of the decision and not to make a test pass**, and
the test is renamed to say what it now pins.

**The note at `PdfForms.cs` is kept exactly as the file holds it**, spaces and all, because
`PdfFormCheck` compares notes. **The canopy guard and the Total Green cover cell check are
unchanged on all three forms**: a plot with a tree on rows 85, 92 or 99 still writes nothing here,
which is Bader's decision of 15 September and stands.

Break watch: the canopy only branch put back reddened 4 of 2042, and the 984 case read
`Expected: "0.001053" Actual: "0.000984"`. Restored byte for byte, md5
`6fab9b37501c2911a0b82f4133a185d6`.

### 3. The plot list file, and audit 4 finding 66 with it

**The team sent 154 plots to export and wants every one exported with none skipped.**

`PlotListFile`, `PlotListRead` and `TickingTheList` are new in Core. **THE FILE NEVER ENTERS THIS
REPOSITORY**: only the pointer, `kpi-plot-list.txt` beside the installed assembly, and every test
writes its own list into the temp folder.

Six rules: one plot a line, edge whitespace off, blanks skipped, the file's order kept, a line
that is not a plot named with its line number, a plot listed twice named with both and counted
once, compared with `StringComparer.Ordinal` because that is what `PlotTicks` and
`PlotsInTheModel.Holds` use, and a file that cannot be read refusing with its reason rather than
reading as an empty list.

**Tick the list sits beside Select all and Clear and behaves the same way**: it REPLACES every
tick and FORGETS every hand choice, because a person pressing it is saying these are the plots.

**Nothing on the list drops out without a line above Create**: a listed plot the model does not
name, a plot listed twice, a line that is not a plot, and a listed plot the press would put into
no workbook or no PDF. That last one asks `PlotsPerTemplate.For` and `PdfForms.ForPlot`, which are
the two rules that really decide it, rather than a third written beside the button.

**`THE PLOT LIST` opens the report**, beside `EVERY PLOT THE TOOL OFFERED`. They are different
questions printed apart: that one is the MODEL's list and this is the TEAM's. One row per listed
plot in the file's order with in the model, ticked, workbook, PDF and why not, then the model's
plots not on the list, then three counts.

**The browse line is ONE method now, audit 4 finding 77's own remedy.** It was written out four
times and the copies had drifted: one warned when the chosen folder was the templates folder, one
warned when a remembered file had gone, and one did neither. All four go through `BrowsedLine` and
each keeps its own lines under it. **Finding 77 is NOT marked FIXED**, because this round was told
to mark 66 and nothing else, and it is named here for Bader to mark.

**AUDIT 4 FINDING 66 IS FIXED.** `GuardedWrite` wraps the `OnePlot` call. The per plot WRITE loop
was added when a workbook became one plot and had no guard, so a throw on plot 100 of 154 unwound
to `Run`'s catches, which say Revit refused that with no plot named and **write no report at
all**, leaving 99 workbooks and 99 PDFs in the client's folder tree with no record of which plots
those were. The plot is named with the exception's type and message now, the run carries on, and
the report is written either way.

The worked example, every value by hand: a list reading HF-01, then NS-41 with spaces round it,
then a blank line, then NS-41, then XX-99, then the words not a plot. Against a model naming
HF-01, NS-41 and EP-05 it ticks HF-01 and NS-41, names NS-41 on lines 2 and 4, names XX-99 as not
in the model, names line 6 as not a plot, and names EP-05 as in the model and not on the list.

Break watch: the reader made to drop the last plot line reddened 4 of 2042, and
`TheFilesOrderIsKept` read `Expected: ["ST-09", "EP-01", "DM-11", "NS-02"] Actual: ["ST-09",
"EP-01", "DM-11"]`. Restored byte for byte, md5 `773fecb415d9abde50f1c7153cbb7a2d`.

### 4. A dash row counts as shrubs, by its phase

**Bader's decision of 15 September, off his own record: 44 schedule rows across the model carry a
botanical name of a dash**, and four plots came out blank because of them, EP-01, EP-09, EP-14 and
HF-01, which are the four the eighty seventh pass made refuse. His screenshot of
ANH-007-HF-100002 shows all four shrub boxes empty.

A dash row counts as `SHRUBS` and goes by its phase like any `SHRUBS:` species.
`SpeciesPrefix.CountsAsShrubs` is the one method both halves of the rule ask, so the dash cannot
be added in one place and forgotten in another.

**ONLY THE DASH, and it is the WHOLE cell once the edges are off.** A name with no colon that is
not a dash, and a prefix that is neither, refuse exactly as they did:
`AnUnprefixedSpeciesGoesNowhereAndIsNamed` is green and unchanged, and
`ARealUnknownPrefixStillRefusesAndNothingIsWritten` keeps the old refusal on the old fixture. A
dash INSIDE a name is untouched, `ACACIA / VACHELLIA FARNESIANA` and
`CARISSA MACROCARPA - GRANDIFLORA` included. A dash row under a phase the template leaves out
stays out, and **a dash row under no phase row still refuses**, because nothing says whether it is
existing or proposed and that is a different gap from not knowing its kind.

HF-01 by hand, the one plot whose split is known: group total 283, GROUND COVER 52 under Proposed,
dash rows 231 under Existing.

```
Existing Shrubs  231      Proposed Shrubs  0      TOTAL Shrubs  231      Ground Cover  52
231 plus 52 is 283
```

**The `Losing` fixture's unplaced row was `UNKNOWN PREFIX SPECIES` under Proposed, which is not
the model's value.** HF-01 is rebuilt as a dash under Existing and `Losing` is kept for the one
test that a real unknown prefix still refuses.

**WHICH PHASE EP-01, EP-09 AND EP-14 CARRY THEIRS UNDER IS UNKNOWN FROM THIS REPOSITORY.** The
08:38 report is under `reports/` and nothing there is ever committed, so those three pin only that
they add up and write, and not which of the two shrubs boxes took the area. The new dash rows
block answers it on the next run.

The plots that split cleanly do not move, checked: FP-16 335, FP-22 427 and 1114 against 1541, and
NP-100002 248 and 652 against 900.

**EVERY DASH ROW IS ON THE RECORD.** `THE ROWS WHOSE BOTANICAL NAME IS A DASH, COUNTED AS SHRUBS`
sits beside `THE SPECIES NO PREFIX PLACED, BY NAME` with the plot, the row, the phase, the area
and the box, which is Existing Shrubs, Proposed Shrubs, no box because the template leaves its
phase out, or no box because it sits under no phase row and the plot refuses.

**A DASH CAN ALSO REACH THE TREE LISTS AND NOTHING THERE CHANGED.** Traced rather than assumed.
`ScheduleRows.cs:329` reads the botanical name off the column the heading row names and `:330`
tests it for whitespace, so a dash is a species row in the softscape schedule too. It merges
through `KpiMerge.Species`, reaches `SpeciesMatching.Against` at `:317`, matches no name in the
workbook's list and no alias, and falls to `WrittenInto` at `:333`, which asks the canopy diameter
at `:434` and answers `NotSized` where the model prints none. **So a dash tree takes an empty row
carrying the name `-` and its count, or is named as unsized.** Unchanged this round.

Break watch: the dash rule taken out of `CountsAsShrubs` reddened 10 of 2042, and
`HfOneWritesItsFourBoxesOffTheDashRows` read `ExistingShrubs was not written. 1 species row could
not be placed in either box ... 231 m² is unaccounted for of the 283 m²`. Restored byte for byte,
md5 `715a77de61330fcc94defbe4283731cd`.

### What is UNKNOWN and what would settle it

- **Which phase EP-01, EP-09 and EP-14 carry their dash rows under.** The dash rows block on the
  next run settles it.
- **What fonts the client's forms really use.** No client PDF is in this repository. The widths
  UNKNOWN count in the glance names any font this tool cannot measure, on the next run.
- **Whether any client field carries a `/DA` size below 6.** None is measured here. The ceiling
  wins over the floor where they disagree and the box is named either way.
- **Whether the standard fourteen width tables are right to the thousandth.** They are the
  published core font metrics held as data and no AFM file is in this repository. The read back
  says which size landed and the value is never changed, so the cost of a wrong width is a size out
  by a tenth of a point.
- **Whether DM-21, NS-16, NS-20, ST-23 and ST-24 belong on the team's list.** IN THE MODEL AND NOT
  ON THE LIST names them on the next run and it is the team's call.

### Files

Core: `PdfTextFit.cs`, `PdfFontWidths.cs`, `PlotListFile.cs`, `TickingTheList.cs` new, and
`PdfFormFile.cs`, `PdfFill.cs`, `PdfForms.cs`, `PdfChecklist.cs`, `PdfOutcome.cs`,
`RunAtAGlance.cs`, `GroundCover.cs`, `KpiCreateReport.cs`, `KpiCreateRunSet.cs`, `CreateWords.cs`.
Revit: `KpiPanel.cs`, `KpiRequestHandler.cs`, `RememberedFolder.cs`. Tests:
`PdfTextFitTests.cs` and `PlotListFileTests.cs` new, and `PdfComputedTests.cs`,
`GroundCoverSplitTests.cs`, `PdfFixture.cs`. Plus `.claude/rules/kpi-rules.md`,
`steps/audit-kpi-4.md` and `steps/2026-09-15-kpi-fixes.md`.

---

## 2026-09-15, eighty seventh pass. The check that could never fire, and the names first

**2000 tests, 1180 of them KPI, 10 added, against the 1990 main carries** at `9ab922a`, 28 hook
cases unchanged, build zero warnings. **The audit findings stay open, not renumbered, not
reordered: 63 numbered, 19 carrying a FIXED mark, 44 open.**

### First, and before the fix: what those species are called

**UNKNOWN from this repository, and there is no way round it.** The 08:38 report is under
`reports/` and nothing there is ever committed, and nothing else here holds a per plot species
list. **So the names block is built rather than the names reported**, and the next run prints
them.

**One thing the round message's own numbers DO settle, by deduction.** HF-01 read Existing
Shrubs 231 and Proposed 52 under the phase rule, so its EXISTING phase subtotal was 231, and
Existing IS a phase a tree list sheet is named for on HEALTHCARE. A `SHRUBS:` species under it
would have landed in the existing figure. It did not. **So HF-01's 231 is unplaced because of
its PREFIX and not because of its phase.** What that prefix reads is still unknown.

**THE SPECIES NO PREFIX PLACED, BY NAME** now opens the section: every distinct name, grouped by
the prefix each read, with how many rows and how much area carry it.

```
  THE SPECIES NO PREFIX PLACED, BY NAME (2 prefixes)
  the prefix each read | distinct names | rows | area | the names
  CLIMBERS | 1 | 2 | 350 m² | CLIMBERS: BOUGAINVILLEA GLABRA x2
  PALMS | 1 | 1 | 61 m² | PALMS: PHOENIX DACTYLIFERA x1
```

**It is at the TOP because it is the question rather than the detail.** One unseen prefix over
every unplaced row is a line in a table and the team's call. A dozen different things is a
naming job in the model. **The two look identical in a count and different in a list.**

### The fault, and the wording that caused it was the round message's own

**Placed-nowhere sat INSIDE the equation, so the sum always closed and the refusal could never
fire.** Four plots wrote every box short with the column reading YES:

```
plot  | existing | proposed | ground cover | placed nowhere | group total | adds up
EP-01 |    0     |    0     |      0       |    411 m²      |    411      |  YES
EP-09 |    0     |    0     |      0       |      3 m²      |      3      |  YES
EP-14 |    0     |    0     |      0       |     78 m²      |     78      |  YES
HF-01 |    0     |    0     |     52 m²    |    231 m²      |    283      |  YES
```

HF-01 went from Existing Shrubs 231, Proposed 52 and TOTAL 283 on the 18:15 run to nought,
nought and nought. **A check that cannot fail is not a check.**

**The rule now: what would be WRITTEN, plus what this template deliberately LEAVES OUT, equals
the group total.** Anything unreadable refuses the plot on its own, before any arithmetic. The
rounding room applies to the sum and never to an unplaced species, because a species nobody
could place is not a rounding difference however small its area.

### Where the round's wording is departed from, and the reason

**There are two kinds of placed nowhere and only one of them may refuse.**

```
COULD NOT BE READ    a prefix that is neither of the two, no prefix at all, or no phase row.
                     REFUSES the plot. The tool does not know what the area IS
LEFT OUT BY THIS     a SHRUBS: species under a phase no tree list sheet is named for, which
TEMPLATE             is Street Design on a mosque plot. A TERM of the sum, NOT a refusal
```

**Taken literally the fix would have written nothing on every mosque plot with a Street Design
group**, and FM-05 alone carries 459 m² of it. That is a decision Bader already made, the phase
rows have left it out and named it since, and reversing it would be a new fault of the opposite
kind rather than a fix. `APhaseTheTemplateLeavesOutIsInNeitherNumber` going green again under the
new rule is what caught it: it reddened on the first cut and the first cut was wrong.

**The two travel in separate lists and the report prints them in separate columns**, COULD NOT
BE READ and left out by this template, so the distinction is on the page rather than in anyone's
head.

### The check, written out by hand

The four that must now refuse, each asserting the group total, what the four boxes would have
read, and that nothing is written:

```
EP-01   would have read 0 m²    lost 411    group total 411
EP-09   would have read 0 m²    lost   3    group total   3
EP-14   would have read 0 m²    lost  78    group total  78
HF-01   would have read 52 m²   lost 231    group total 283
```

The three that must NOT move, every number as the round message gave it:

```
FP-16      proposed 335   ground cover   0   group total  335
FP-22      proposed 427   ground cover 1114  group total 1541
NP-100002  proposed 248   ground cover  652  group total  900
```

`HfOneWritesNoneOfItsFourBoxesAndTheFormSaysWhy` then checks all four PDF boxes on HF-01 are
blank and each carries `231 m² is unaccounted for of the 283 m²`.

### The break watch

**Broken: the unreadable refusal switched off AND the unplaced area put back inside the
equation**, which is the fault exactly as the 08:38 run measured it.

```
6 red of 2000
  TheFourPlotsThatLostTheirAreaNowRefuseAndNothingIsWritten, all four cases:
    HF-01 lost 231 m² and the check still said it adds up. The four boxes would have been
    written reading 52 m² against a group total of 283 m². An area placed nowhere is a
    disagreement and never a term of the sum.
    EP-01 lost 411 m² ... would have been written reading 0 m² against a group total of 411 m².
  AnUnprefixedSpeciesGoesNowhereAndIsNamed
  HfOneWritesNoneOfItsFourBoxesAndTheFormSaysWhy
```

**Each red names the plot, the area lost and the number the box would have carried**, which is
the whole fault in one sentence. Restored byte for byte, checked with diff and md5
`ce641093a192f53beb01fad56cbd1f90`, and rerun green at 2000.

### What worked, on the record

The water demand is on every PDF: **2.1 on ANH-007-MO-100001, 48.752 on ANH-007-NP-100002 and
2.252 on ANH-007-ST-100003**, and the split is right on every plot where every species carries a
known prefix. Neither was touched this round.

### Open question for the team

**Whether the unplaced prefix is a fifth kind rather than a fault.** The next run's names block
answers it, and if it reads one unseen prefix across the run the answer is one line in
`SpeciesPrefix` rather than a refusal. That is Bader's call once he can see the name.

---

## 2026-09-15, eighty sixth pass. Ground cover out of the shrubs box, and item 1 already shipped

**1990 tests, 1170 of them KPI, 26 added, against the 1964 main carries** at `8367b30`, 28 hook
cases unchanged, build zero warnings. **The audit findings stay open, not renumbered, not
reordered: 63 numbered, 19 carrying a FIXED mark, 44 open.**

### Item 1, the irrigation water demand, was built last round and is on main

**It is `8367b30`, the eighty fifth pass, and nothing about it needed doing again.** Checked
point by point against the brief rather than assumed:

```
both schedules' own TOTAL rows, added, divided by 1000    WaterDemandRead.From    YES
the L/DAY heading matched WHOLE, none and more than one   ScheduleColumns.Reading YES
   both refused with the headings printed
never the species rows added up                           NoTotalRow              YES
all three forms                                           PdfForms.All            YES
a missing schedule or an unreadable total writes nothing  PlotWaterDemand.Why     YES
   and is named with which half
the value read back off the output                        PdfChecklist:176        YES, every
   written field goes through the same read back                     field, no special case
the report says the FORM's unit                           WaterDemand.CubicMetresUnit YES
the groups left out of the tree lists, per plot,          THE IRRIGATION WATER    YES
   with their subtotals                                   DEMAND, PER PLOT
```

**The Open Spaces field name is still what was measured off that file on 14 September**, and it
still cannot be re-measured here because no client PDF may enter this repository. `PdfFormCheck`
is what protects it: a form not carrying every field the table names writes NOTHING.

**DM-11's softscape L/DAY total is still UNKNOWN**, for the same reason as last round, the 1548
scan being under `reports/`. The 432 and 908 are two totals and the schedule has one: if they
are the two group totals the TOTAL row reads 1340, that is what the tool takes, and the report
prints the row it read so the first run settles it without argument.

One break was watched again to prove the rule still holds, under the break watches below.

### Item 2, ground cover was in the shrubs box

**DM-14 is the plot that shows it.** Its SHRUBS & GROUND COVER group holds
`GROUND COVER: CARISSA MACROCAPA` 270 and `GROUND COVER: LAMPRANTHUS AUREUS` 198, adding to the
printed group total of 468, and **every species in it is ground cover and none is shrubs**. All
468 went into Proposed Shrubs and Ground Cover was left blank. DM-11 is the opposite end, two
`SHRUBS:` species adding to 70.

**Only one of the two readings adds up.** Putting the group total into Ground Cover as well
would give DM-11 Ground Cover 70 beside Proposed Shrubs 70, the same area in two boxes of one
form. The prefix split gives each box its own species and the two add to the group total.

`SpeciesPrefix` and `GroundCoverSplit` are the rule. The prefix is the text before the FIRST
colon, matched whole and without case through `LabelText.Same`, the same rule every label lookup
here already asks. A species with no prefix, or a prefix that is neither of the two, goes
NOWHERE and is named with its plot, its row, its area and what its prefix read.

**The two figures plus everything placed nowhere must equal the group total the schedule
printed, or NONE of the four boxes is written.** Writing three and blanking one would leave a
form whose own numbers disagree with the schedule behind it. The room is the phase rows' own,
half the project's area rounding step per row summed, which is why the area unit is handed into
`PdfFill.Of` rather than a tolerance being written in the new file.

**The shrubs figures come off SPECIES ROWS now and not off PHASE ROWS**, which is the whole
fault: a phase row holds shrubs and ground cover added together. Existing against proposed is
still `CountedGroups.SheetFor`, the same method the tree lists ask, so nothing about that rule
is written twice.

**The GRASS group is untouched.** DM-11's lawn is still 35 and DM-14's still 175, both pinned.

### One thing deleted, and it is the second reason rather than the first

**`PdfFill.GroundCoverIsNotPrintedApart` is gone.** It read that the schedule prints the group as
one and nothing prints ground cover on its own, so it is not derived. **The shape it recorded is
gone rather than its last caller**: every species row in that group begins `SHRUBS:` or
`GROUND COVER:`, so the two ARE printed apart and the number is read rather than derived. A
constant recording a claim the data disproves is worse than no constant, which is what
`OutputName.Suggested` bought this project.

### The check, written out by hand and both plots pinned

```
DM-11   group total 70    shrubs 70    ground cover 0      its shrubs figure does not move
DM-14   group total 468   shrubs 0     ground cover 468    468 out of shrubs, 468 into cover
```

`OnDm11TheWholeGroupIsShrubsAndItsFigureDoesNotMove` and
`OnDm14TheWholeGroupIsGroundCoverAndTheShrubsBoxGoesToNought` build both schedules as the model
prints them, eleven columns and all, and run the real reader over them.
`TheTwoPlotsFillOppositeBoxesOnTheForm` then checks the numbers reach the form's own boxes,
which is the thing a client reads.

### How big is it, and the honest gap

**UNKNOWN from this repository, and here is exactly why.** The brief's own measurement off the
05:49 report is 143 species names beginning `SHRUBS:`, 107 beginning `GROUND COVER:` and 55
beginning `GRASS:` over 156 plots. **That is a count of NAMES and the question is a count of
PLOTS**, and the report it came off is under `reports/`, where nothing is ever committed, so it
cannot be turned into one here. Nothing in this repository holds a per plot species list.

What can be said from the two numbers without inventing anything: 107 ground cover names over
156 plots means ground cover is not a rare case, and **every workbook and form written so far
has the two in one box.**

`SHRUBS AGAINST GROUND COVER, PER PLOT` is the answer rather than a guess. It prints one row per
plot with both figures, what was placed nowhere, the group total and whether it adds up, so the
next run says how many plots are all one kind and how many are mixed without anybody counting.

### The break watches, three of them, one per rule

```
BREAK 1  a species that is not ground cover falls into shrubs, the old rule
   2 red of 1990
   AnUnprefixedSpeciesGoesNowhereAndIsNamed        Expected: 36   Actual: 45
   APrefixThatIsNeitherIsAlsoPlacedNowhereAndNamedWithWhatItRead

BREAK 2  ground cover species go to the shrubs box, the fault exactly as it ships
   3 red of 1990
   OnDm14TheWholeGroupIsGroundCoverAndTheShrubsBoxGoesToNought  Expected: 468  Actual: 0
   TheTwoPlotsFillOppositeBoxesOnTheForm                        Expected: "468" Actual: "0"
   TheReportPrintsTheSplitPerPlotAndNamesWhatItCouldNotPlace

BREAK 3  item 1: no TOTAL row falls back to adding the species rows
   1 red of 1990
   AScheduleThatPrintsNoTotalRowIsNamedAndItsSpeciesRowsAreNotAddedUp
     the schedule printed no TOTAL row and the reader produced 2304 L/day anyway, off row 1.
     Take the total means take the total, and 468 plus 684 is a number this tool worked out
     rather than one the schedule printed.
```

**Break 1's first red names the fault by its numbers**: the unprefixed 9 m2 swallowed into the
shrubs box turns 36 into 45. **Break 2's names the plot and the box**: DM-14's ground cover goes
from 468 to 0, which is the wrong box this round exists to empty. **Break 3's assertion was
strengthened first**, because a bare `Assert.False` reads `Expected: False, Actual: True` and
says nothing about what broke.

All three restored byte for byte and checked with diff and md5,
`bc824e791d0a099a611531944d930a8e` for `GroundCover.cs` and `68acb06b8e5f09ac4eab1d5f32b591b5`
for `WaterDemand.cs`, and rerun green at 1990.

### Eight tests moved and none of them was weakened

Giving the shrubs group its species rows moved eight cases that built a group out of phase
subtotals alone. **Six now pass unchanged** because each phase carries one `SHRUBS:` species
holding its whole area, which is what those cases always meant. **Two had their subject
reversed and are rewritten rather than deleted**: `GroundCoverIsLeftBlankAndNamedRatherThanDerived`
is now `GroundCoverTakesTheGroundCoverSpeciesAndNoughtWhereThereAreNone`, and the two asserting
the old constant now assert the absence of the group instead, which is the honest sentence for a
plot whose schedule printed none.

### Open question for the team

**If the split disagrees with the group total on a real plot the tool STOPS**, writes none of the
four boxes and names the plot and all three numbers. That is deliberate and it is a question for
Bader and the client rather than one this tool settles.

---

## 2026-09-15, eighty fifth pass. The irrigation water demand, off both schedules' TOTAL rows

**The last PDF field with a real source that nothing read.** **1964 tests, 1144 of them KPI, 25
added, against the 1939 main carries** at `dbae2e6`, 28 hook cases unchanged, build zero
warnings. **The audit findings stay open, not renumbered, not reordered: 63 numbered, 19 carrying
a FIXED mark, 44 open.**

### What it does

Per plot, out of both schedules, the L/DAY column, **TAKE THE TOTAL OF ALL**, add the two, divide
by a thousand. `WaterDemandRead.From` is the whole rule, `PlotWaterDemand` holds both halves and
the sum, and `PlotReading.Water` carries it from the read to the fill and to the report.

**Nothing adds the species rows up anywhere.** Bader said take the total, and a sum this tool
worked out is a different number from one the schedule printed. A schedule with no TOTAL row
writes nothing for that half and is named, with the species rows sitting right there untouched.

### The column, and why it is matched whole

**Three columns of the softscape schedule hold the word WATER and only one reads L/DAY**, with
`WATER DEMAND` and `WATER L/TREE/DAY` beside it and `WATER L/SQM/DAY` on the other schedule. So
`ScheduleColumns.Reading` matches the WHOLE heading through `LabelText.Same`, the rule every
whole-label lookup here already asks, and **none and more than one are both refusals with the
headings printed**. This is the two DIAMETER headings again, which cost a round.

The TOTAL row is found by its first cell, read off `SoftscapeRows.TotalMark` so the two cannot
part. That is the row's shape rather than a value read by position, which is the same ground
`IsStructureRow` already stands on.

### Two things reported and not decided

**a. The TOTAL includes every group and the tree lists do not.** A mosque plot's Street Design
group is out of scope for the tree lists by Bader's decision and its water is inside the TOTAL.
The number written is the total he specified and nothing subtracts anything, and
`THE IRRIGATION WATER DEMAND, PER PLOT` carries a column naming every group that plot left out
with that group's own subtotal, beside the demand that counts it. The difference is on the page.

**b. A schedule with no TOTAL row.** **UNKNOWN whether any real one prints none**: every schedule
shape measured so far prints one, the 1548 scan is not in this repository, and the reader's own
refusal is what will say so on the first run. It writes nothing for that half and names it.

### One thing changed that the round did not name

**It prints FINE rather than to two places.** `PdfFill.Number` rounds to two decimals and every
other whole-unit field goes through it. Dividing litres by a thousand makes this one small: 2492
L/day is 2.492 and two places would send **2.49** to the client, two litres a day thrown away on
every plot of a hundred and fifty. It goes through the same `Fine` the green cover and the road
length already use, and `Fine`'s own docstring already carries the reason.

### The Open Spaces field name

`PdfForms.OpenSpaces` names it `Irrigation water demand`, which is the name measured off that
file on 14 September. **I cannot re-measure it here and never will be able to**, because no
client PDF may enter this repository. That form does name five of its fields badly, `undefined_4.0`,
`Proposed Trees.1`, `0` and `0_2`, which is why nothing on it is taken from what a field is
called on the other two. **What protects it is `PdfFormCheck`**: a form not carrying every field
the table names writes NOTHING rather than writing into the wrong box, so a wrong name here is a
blank field and a named refusal rather than a number in a client's document.

### What DM-11 will read, and the check Bader asked for

He measured DM-11's shrubs and lawn schedule printing **L/DAY totals of 432 and 908**.

**432 and 908 are two totals and the schedule has one.** If they are the two GROUP totals, GRASS
and SHRUBS & GROUND COVER, then the schedule's own TOTAL row reads **1340** and 1340 is what the
tool takes: never 432, never 908 and never the species rows added.
`OnDm11TheScheduleTotalIsTakenAndNeitherGroupTotalIs` pins that over a schedule built to that
shape, asserting 1340 and asserting it is neither of the two. If the TOTAL row itself reads one
of them, the tool takes that one. **The report prints the ROW each total came off, so the first
run settles which reading is right rather than this round assuming one.**

**DM-11's softscape L/DAY total is UNKNOWN from here**, because the 1548 scan is not in this
repository. The field comes to `(softscape + shrubs) / 1000`. On the 1340 reading a softscape
total of 1152 gives **2.492**, and `BothTotalsAddedAndDividedByAThousandGiveWhatTheFormAsksFor`
writes 1152, 1340, 2492 and 2.492 out by hand.

### The break watch

**Broken: `ScheduleColumns.Reading` made to take the FIRST of two matching headings** rather than
refusing, which is the exact fault the round warns about.

```
2 red of 1964
  TwoColumnsReadingLitresADayRefuseAndNameBoth
    two columns read L/DAY and the reader took one of them anyway, 1152 L/day off row 2.
    More than one is a refusal and never the first.
  MoreThanOneMatchingHeadingComesBackAsMinusTwo
    Expected: -2   Actual: 0
```

**The first red names what was broken in its own message rather than in its name alone**, which
is why the assertion carries one: `Assert.False(read.Read)` reads `Expected: False, Actual: True`
and says nothing. Restored byte for byte, checked with md5 `096037650ddefaf26bf40f276df9b697`
and with diff, and rerun green at 1964.

### Open question for the team

Whether the water demand should count the groups the tree lists leave out. It does today, because
that is what TAKE THE TOTAL means, and the report names every such group and its subtotal per
plot so Bader can see the difference and say.

---

## 2026-09-15, eighty fourth pass. The fourth audit of the KPI tool, a firm's review

**This round writes no code.** It reads the project the way a software firm reads a codebase
before taking it over, over eight areas worked one at a time, and writes
`steps/audit-kpi-4.md`. **1939 tests, 1119 of them KPI, against the 1939 main carries** at
`f2e2f42`, 28 hook cases, build zero warnings, all unchanged because nothing was built.

**The three earlier audits stand untouched: 63 numbered findings, 19 carrying a FIXED mark, 44
open**, derived by walking the files rather than off a note. None is closed, renumbered or
reordered, and none is reported again. Seventeen new findings, 64 to 80: one BLOCKS, three
WRONG, ten COSTLY, three TIDY, with nine dropped for having no cost to the user.

### What the audit found that was not known

**The client's project name, their consultant and a real contract reference are constants in a
public repository**, `PdfForms.cs:224`, `:226` and `:235`, against `.gitignore:46-49`, which
names those exact three facts as the reason no client PDF may enter. The rule they serve is
sound and the strings do not have to be here to serve it.

**A throw while reading a schedule's plot filter comes back as an empty string**, and an empty
string means the schedule belongs to no plot, so a plot's trees go missing and nothing records
it. **The per plot WRITE loop has no guard**, so a throw at plot 100 of 156 ends the press with
99 workbooks already written and no report at all. **Three of the five .xlsx readers do not
catch `XmlException`** and the two that do are the two that write.

**`CLAUDE.md` holds the string PDF zero times** and the tool fills a client AcroForm for every
plot out of 2,149 lines. **`ReportPlaces` sits in Drawing Sheet's folder and is called by three
tasks**, which is `territory.md`'s own worked example, unmoved.

### Two things measured in a copy of the tip, never here

Adding an eighth template reddens **8 of 1939**, and after filling in what those name, **10
more**, each naming the next table. Adding a field to a PDF form reddens **2**, both counts, and
neither is the missing case in `PdfFill.One`. **The template tables are held level by the gate
and the PDF fields are not**, and the pattern that would catch it is already in this repository
twice on the Drawing Sheet side.

### The debt answer, as numbers

```
                    Core/Kpi  test files  KPI src lines  KPI tests  tests per 100 lines
audit 1  92dd36c         57          24         12,575        429                 3.41
audit 2  82d95f4         59          26         12,987        459                 3.53
audit 3  f3fe456         78          57         21,887        853                 3.90
today    f2e2f42         96          76         28,401       1119                 3.94
```

**The code is not accumulating debt faster than it clears it.** Source +126 per cent, tests +161
per cent, open findings per 1,000 lines flat at 2.31, 3.62, 1.46, 1.55. **The backlog is: 63
opened against 19 closed, and 6 rounds of 42 since the first audit closed anything.** Six of the
44 open are marked PASSED BY against code that no longer exists, so the honest number is 38 open
and 6 unresolvable.

### What was checked and is clean

Secret scan over the WHOLE history: 1,942 distinct blobs, 78,220,014 bytes, 1,941 text blobs
scanned against thirteen patterns. **Zero credentials, ever**, and the only emails are
example.invalid placeholders. **No `.xlsx` and no `.pdf` has ever been committed**, over 203
commits. Core names no Revit type, proved four ways including off the built assembly, which
lists `netstandard` and nothing else. Every crossing between Kpi and Shared runs one way.

### Open question for the team

Whether the three header values leave the repository for a file beside the installed assembly.
It trades a rule measured off the client's own files for a file the team has to keep, and it is
Bader's call rather than this round's.

---

## 2026-09-15, eighty third pass. The held off list, in step and on screen

**Pull request 138, merged into main as `2775999`.** The runner ran 28 hook cases and 1939
tests against the pull request head `7800ce8`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1939 tests, 1119 of them KPI, 5 added, against the 1934 main carries** at `a3d9896`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

All three items came out of the eighty second pass's own verifiers rather than off a run, and
they are one fault: **nothing kept the hand choices and the real ticks in step, and nothing ever
printed them.**

### The one record

`HandTicks` replaces the bare set of plots taken off. It holds BOTH directions, a plot taken off
or put on by hand and never both, immutable the way `PlotTicks` already is, and
`KpiPanel.TickedByHand` moves the ticks and the record together so nothing sets one without the
other.

### 1. Unticked was not symmetric with Ticked

`Ticked` took the held off list and `Unticked` took nothing. So **a hand untick survived a row
tick and a hand tick did not survive a row untick.** A person who picked three mosque plots by
hand, ticked the MOSQUES row for the rest, then changed their mind about the row, lost their three
with nothing said.

`Unticked` takes the record now and keeps the plots put on by hand, so unticking a row undoes
exactly what ticking it did and ticking it again restores the same state. A test writes both
directions out at once: DM-12 put on by hand and DM-14 taken off, tick the row, untick it, and the
ticks are back where they started.

### 2. Select all and Clear, decided once

**I agree about Select all and the same argument carries Clear**, which is the half the round
message did not ask about. Both replace every tick, so a per plot choice left standing behind
either is a record that disagrees with what is on screen, and the next row press acts on the
disagreement. **Clear leaving the record standing would mean Clear then tick MOSQUES silently
drops DM-14**, which is the same invisible drop as the Select all route one press later. Both
forget every hand choice, so there is one rule rather than two and no path disagrees with another.

The test writes out what it did before as well as what it does now, so the fault is on the record
beside the fix: carrying the old record past Select all drops DM-14 on the very next row press.

### 3. SomeOfThem builds the sentence and nothing called it

Its only references were `TickingATemplateTests.cs:112` and `:116`. **Green, and it had never
reached a screen.** It is on the workbook row now, under the tick box, as a NOTE rather than a
refusal, and a row where every plot is going in gets no line because a line about nothing is one
the team reads past on every other press.

**The pane counts nothing.** `TickingATemplate.RowLine` takes the template, the ticks and the
component reader and hands back the sentence, so the two counts come off the split's own rule
rather than off a loop beside the control that draws them. That is also what made item 3 testable
in Core at all.

**It is the line the round before spent itself looking for.** That round asked a 57,143 line report
why MM-09 to MM-15 were unticked, and this answers the held off half of it on the row a person is
looking at when they press.

### How many other members build a line the pane never shows: SIX, and three of them are lines

**Counted rather than guessed at.** Every public member of `Core/Kpi` returning a string or a list
of strings, held against every reference in `src` outside its own declaration, doc comments left
out. **245 such members. SIX have no reference at all**, and every one of the six is tested green.

```
KpiPaneWords.ModelNamed        a line       the model's name or the words for none
CreateWords.SuggestedName      a line       the name a box offered, and THE BOX IS DELETED
RegionChoice.WhyUnchosen       a line       why no region was chosen, for the report's own column
ComponentTemplates.ValuesFor   a list       its docstring says the report and the pane say it
PlotPrefixes.PrefixesFor       a list       DELIBERATELY KEPT and already recorded as uncalled
WorkbookPatcher.ReadBack       not a line   a cell read off a written file
```

**`WhyUnchosen` is the one worth acting on, and it is not acted on this round** because it was not
what was asked for. Its own docstring says it exists "for a report that would otherwise print an
empty cell and leave somebody guessing which of the two cases it was", and nothing calls it, so
**the report prints the empty cell.** `SuggestedName` is the deleted name box's and its shape is
gone with the box. `PrefixesFor` is the one already recorded as deliberately kept.

### The open question beside it: is that enough for a route beside every unticked plot

**Not quite, and one thing is still missing.** Items 1 to 3 close the hand route: a plot unticked
by hand is visible on its row now and the record cannot drift, so an unticked plot is no longer
one of three unexplained cases. What is left is the two the split decides, the component not in
the table and the component disagreeing with the prefix, **and the tool does know both at tick
time** through `PlotsPerTemplate.For`, whose `Why` is already the sentence.

**What is missing is the component for a plot nobody ticked.** The correction recorded last round
stands: `set.Runs` to `Readings` to `PlotReading.Component` reaches the component with no new
argument, and it reaches it **for ticked plots only**, because an unticked plot is never read and
has no reading at all. The plots the column exists to explain are exactly the ones that route
cannot reach.

**So it needs the component per plot read beside the plot list, off ONE read at the press**, the
way `PlotOrigins` already reads the list off the live document. That is one read and one argument
and it is a round of its own. **Not built this round**, as asked.

### Three break watches, one test red each

```
1  Unticked ignores the hand record      UntickingARowKeepsAPlotThePersonPutOnByHand
                                         RED: Expected ["DM-12"], Actual []
2  Forgotten hands back itself           SelectAllAndClearForgetEveryHandChoice
                                         RED: Expected Count = 0, Actual Count = 2,
                                         Off = ["DM-14"], On = ["DM-12"]
3  the row counts what could go          TheRowSaysHowManyOfItsPlotsAreGoingInAndSaysNothing...
                                         RED: Expected "MOSQUES: 2 of the 3 plots...", Actual ""
```

Every one names what was broken. Both files restored byte for byte, checked with `diff -q`, and
the suite green at 1939 after.

### What the correction settled, recorded beside PlotOrigins

**A SECTION THAT COVERS WHAT WENT IN CANNOT TELL YOU WHAT DID NOT.** The premise of the round
before was that MM-09 to MM-15 are in the list and not in the model. They are in the model, on a
sheet AND on a schedule, which is why they are in neither disagreement line. The evidence was that
they appear nowhere in a 57,143 line report, and **that is not evidence**: every section of that
report is over the ticked plots. PLOTS TICKED THAT WENT INTO NO WORKBOOK reading 0 is correct by
construction and was read as a fact about the model. And the two nines were a coincidence.

### For Bader, not a fault to fix, and UNVERIFIED

**Two workbook rows can settle as the same template**, if two files in the templates folder are
both recognised as STREETS, and unticking one then takes every street plot off while the other
stays ticked. **I have not measured that two files can be recognised the same way**, so it is
written here as unverified. What did change is that the state is now visible: the still ticked row
reads `STREETS: 0 of the 78 plots that belong to it, the rest unticked by hand.`

### Still open, and not guessed at

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every line of it is off the code and the two
verifier reports. The row line has never been seen on a screen.

- **The route beside each unticked plot**, which needs the plot list and the component off one read
- **`WhyUnchosen` printing the reason it was written for**, so the region column stops printing an
  empty cell
- **Which of the three routes leaves MM-09 to MM-15 unticked**, now one of two rather than three,
  because the hand route shows itself on the row
- **A species written into an empty row still carries no family, no genus and no native flag**

---

## 2026-09-15, eighty second pass. Where MM-09 to MM-15 come from, and the rows with no canopy

**Pull request 136, merged into main as `bafb39c`.** The runner ran 28 hook cases and 1934
tests against the pull request head `15e097a`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1934 tests, 1114 of them KPI, 6 added, against the 1928 main carries** at `ea7ae2e`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

### 1. They are in the model, and the round message's reading of the silence was wrong

**THE PREMISE WAS THAT THEY ARE IN THE LIST AND NOT IN THE MODEL. THEY ARE IN THE MODEL, ON A
SHEET AND ON A SCHEDULE BOTH.** All three places were checked and the third was checked first.

**Nothing derives a plot name.** There is ONE production construction of the KPI plot list from a
document, `KpiPlotReader.Plots`, and it is `PlotsInTheModel.Of(OnSheets, OnSchedules)`. The sheets
half adds `PRX_Plot_ID` as printed and the schedules half adds the filter's own string, and the
only transformation on a plot string in that chain is `Trim()`. **Every plot identifier builder in
this repository is Drawing Sheet's**, `PlotTickList.Generated` among them, which really does build
`prefix + "-" + number.ToString("00")` in a loop over a range and is exactly the shape the round
message suspected. Checked by name: **no file under `Core/Kpi` or `Revit/Kpi` mentions
`PlotTickList`, `PlotRange`, `PlotRegistry` or `PlotSelection`**, and the only formatted number in
those folders is a report column width, a report row number and a PDF cross reference offset. The
plot named `-` is a reading too, off a schedule filter holding a dash, because nothing checks a
plot against the two letters, dash, digits shape on the way into this list.

**The two disagreement lines count the plots named by exactly ONE source**, so a plot named by
BOTH is in neither of them. MM-09 to MM-15 being in neither line is what PLACES them: they are in
the intersection, on a sheet and on a schedule. Both of the first two places the round message
named are true of them at once, and being in both is exactly what made them invisible.

**The two nines are different nines.** Nine named across the two lines, nine unticked, and nothing
has ever made those agree: one is about which of two reads found a plot, the other about whether
`PlotsPerTemplate.For` placed it in a ticked template. The run proves it, because seven of the nine
unticked are in neither line and the park plots the lines do name are placed by their prefix and
are ticked.

**And the silence was not evidence.** Every section of the create report is over the ticked plots.
THE SPLIT and PLOTS TICKED THAT WENT INTO NO WORKBOOK both run off `TemplateSplit`, which is built
from the ticked list alone, so `Unplaced` cannot hold a plot nobody ticked and its 0 is correct and
says nothing. `KpiCreateRunSet` did not carry the model's plot list at all.

**Why they are unticked is six routes and it is one of three.** With every row ticked a plot goes
unticked when its component is not one of the eleven, when its component and its prefix disagree,
when it has neither, when it was held off by hand, when the ticking call did not run for that row,
or when its template has no recognised file. STREETS wrote its 78 plots, so its row was ticked and
settled, which rules out the third, fifth and sixth and leaves **the first two and the fourth**.
Being on a sheet does not narrow it further: the first two need a component on that sheet and the
fourth needs nothing, and a plot whose sheet holds no component falls to the prefix and is ticked.
**WHICH OF THE THREE IT IS CANNOT BE DETERMINED FROM THIS REPOSITORY** and is not guessed at. Worth
knowing beside it: `ValuePerPlot` reads the component off the plot's FIRST sheet by sheet number and
no other, so a plot whose sheets disagree is placed off one of them with nothing recorded, and
nobody has measured whether any plot's sheets disagree.

**What was added is the section the round asked for.** `PlotOrigins` prints one row per plot at the
top of the report, above the glance: the plot, which of the two reads named it, and whether it was
ticked. The list is read off the LIVE DOCUMENT at the press and the ticked half is counted off
`PlotOutcomes`. A plot that was ticked and that the read does not name is its own row reading NAMED
BY NEITHER SOURCE and the line says it is a bug rather than a state. The counts add up four source
states and two tick states against the row count, printed as YES or as a bug, and one sentence says
what the pane's two lines count so nobody holds nine against nine again.

**WHAT IT STILL DOES NOT SAY IS WHY A PLOT WENT UNTICKED**, and that is deliberate. The route needs
the component per plot, which the pane read at a different moment from the list this section reads
at the press, so a route worked out from the two together would be one answer off two records of
one fact. **The plot list and the component per plot travelling from ONE read is the next line and
a round of its own.**

### 2. Rows 85, 92 and 99, recorded and nothing changed

**52 of 150 forms got no Total Green cover on the 05:49 run**, and the glance line the round before
added named the reason outright: rows 85, 92 and 99 of the client's tree lists carry the numbering
column and no canopy formula. A tree on one of those rows contributes no canopy in the client's own
workbook whoever fills it. **Refusing was right, the exact text rule stays, and nothing in the tool
changed.** It is in `kpi-rules.md` with the three rows, the count and the guard's own printed line,
and Bader is taking it to the client.

**One thing the printed line says that nobody asked about.** `O85 = IF(ISBLANK(B85)," ",N85*B85)`
is the shape MOSQUES row 21 carries at `M21 = IF(ISBLANK(B21)," ",L21*B21)`, two columns right, so
on that sheet the canopy per tree sits at N and the area at O. The guard never cared, because the
only letter in the text it looks for is the DIAMETER column and that comes off the heading row.
Whether the whole sheet uses N and O or only the added rows do is UNKNOWN from one row.

### Three break watches, one test red each

```
1  the two sources swapped in PlotOrigins.Of    EveryPlotOfferedIsARowWithWhereItCameFrom...
                                                RED: EP-05 read "on a sheet and on no schedule"
                                                where it is on a schedule and on no sheet
2  a ticked plot the read does not name dropped APlotTickedThatNeitherSourceNamesIsARowSayingSo
                                                RED: "MM-09 | NAMED BY NEITHER SOURCE | ticked"
                                                gone from the rows
3  the section moved below the glance           TheReportOpensWithEveryPlotTheToolOffered...
                                                RED: the plot list must be above the glance
```

Every one names what was broken. Both files restored byte for byte, checked with `diff -q`, and the
suite green at 1934 after.

### For Bader, not a fault to fix this round

**Select all and Clear do not touch the held off list.** `_heldOff` is added to and removed from by
the per plot tick box alone and cleared only when the model changes, so a plot unticked by hand
once, then brought back by Select all, is dropped again the next time any template row is ticked or
re-ticked. Nothing on the pane ever prints that list. Found while enumerating the six routes and
left alone, because it is not what this round was asked for.

### Still open, and not guessed at

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the 05:49 report, the
pane's screenshot and the code as it stands.

- **Which of the three routes leaves MM-09 to MM-15 unticked.** One line of the next run's report
  beside one look at those sheets' `PRX_Component` settles it
- **Why the plot list holds a plot named `-`.** The schedule filter holds a dash and nothing checks
  a plot's shape on the way in. Whether it should be checked is the team's question, because the
  tool has never invented a plot and refusing one the model really holds is a different rule
- **The route beside each unticked plot**, which needs the list and the component off one read
- **A species written into an empty row still carries no family, no genus and no native flag**

---

## 2026-09-14, eighty first pass. Three faults off the 19:52 run

**Pull request 134, merged into main as `8b90fe0`.** The runner ran 28 hook cases and 1928
tests against the pull request head `25ce207`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1928 tests, 1108 of them KPI, 12 added, against the 1916 main carries** at `484251e`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

What landed the round before and is on the record: no instruction text in any field on any of the
seven PDFs checked, the canopy in Total Green cover with MO-100001 going 0.000105 to 0.000525
square kilometres, and every plot getting its own block, 156 of them.

### 1. The emptying took the client's own header with it

All seven output PDFs read Project name empty, Consultant empty and Contract reference empty. The
rule the round before shipped did exactly what it says and it was one field too wide: those three
are TYPED DEFAULTS rather than notes for whoever fills the form by hand.

**Project name and Consultant are left alone. Their default is a value.** Bader's answer, and they
are the only two fields in that state. Every other text field is still written or emptied, so a
box the tool has no source for still looks like a box nobody has filled.

**Nothing tries to tell a note from a value by reading the text.** The three are held as data,
`PdfForms.HeaderValues`, spelt as the files carry them.

**They are found by the VALUE, because no field name for them is measured anywhere in this
repository and none can be**, since no client PDF enters it. What makes that safe is
`PdfFormCheck` refusing a form that does not carry all three: a client who changes their header
gets a form this tool writes NOTHING into, rather than one whose header it clears or whose
reference box it writes a plot number into having lost track of which box that is.

**Contract reference is written, per plot, from PRX_Plot_NH.** Bader's decision. A plot with no
PRX_Plot_NH gets the box EMPTIED with its own reason rather than left holding the template's, which
would hand the client another project's reference under this plot's name. A form holding no
contract reference field at all is a different fact and is named apart.

Every field is in one of THREE states now rather than two, and the test that walks every field of a
built form refuses any field in none of them.

### 2. Both parks lost both computed numbers, and it is neither of the two cell checks

**The round message asked which of the two guards added last round it is. It is neither, and both
were measured rather than argued about.** A parks shaped fixture was built, the green cover label
at C9 over `D9 = F9+F11+H11` and no percentage label at all, and both were asked:

```
GreenCoverCell    Agrees, cell D9, formula F9+F11+H11, canopy cell F9
PercentageCell    NothingToCheck, and Usable
```

So `SummaryCellCheck.NothingToCheck` does tell an absence from a drift, which is what this round
was asked to check, and `TheParksShapeIsFoundAndThePercentageHasNothingToCheck` says so.

**And neither could blank BOTH fields anyway.** Read off `PdfFill` line by line there are three
gates: `WhyNothingCanBeComputed` reaches both, `GreenCoverCell.Usable` reaches the green cover
alone and `PercentageCell.Usable` the percentage alone. `GreenCover.Total` always computes and
`GreenCover.Percentage` refuses only on an area of nought, so nothing below the gates blanks both
either.

**So it is `WhyNothingCanBeComputed`, which is two things: the workbook was not written, or the
canopy guard found a drift. WHICH OF THE TWO CANNOT BE DETERMINED FROM THIS REPOSITORY**, and it
is not guessed at here. The 19:52 report already says which, per plot, on the field's own line.
Reading it out of 82,048 lines is the thing item 3 is about, and the glance line below is the
answer to that.

**What changed under the parks is the canopy guard's SUBJECT rather than its rule.** Until the
eightieth pass `CanopyArea.From` returned only rows this tool wrote, so a plot whose species all
matched gave it no rows and it answered nothing to check. It returns the matched rows now, so the
guard reads the client's own rows for the first time, holding each against the formula text
measured on one row of one template.

**The exact text rule is KEPT and deliberately not loosened.** Accepting any formula that reads
the row's own diameter cell, the way the green cover cell is checked, would be wrong: a sum of
three cells is the same sum however it is written, and a canopy formula reading the same diameter
and computing it differently gives a different number from the one this tool works out. A number
that is not the workbook's own is what the guard exists to stop, and loosening it to make the
parks fill would have been a fix for a fault nobody has measured.

**What is added is the record of whose row drifted.** `CanopyRow.TheClientsRow` travels from
`match.Added` and the guard's line reads `a row this run wrote in` or `a row the client's list
already held`. The first is the tool writing a row the workbook cannot compute, the #VALUE! fault
that took four rounds to kill. The second is the client's file computing its canopy another way,
which is a question for the team. One sentence covered both.

**What the parks templates' own canopy formula reads is UNKNOWN here.** The next run answers it,
because the guard prints every formula on the row it refused.

**And the two numbers are counted at the top of the report now.** `RunAtAGlance.Computed` says how
many forms got each and, for the ones that did not, ONE LINE PER REASON with its count and its
plots, never one per plot. The two are counted together because they fail together, past four
plots the count stands for the rest, and a field a form does not ask for is counted neither way.

### 3. The report is 82,048 lines and 42,570 are one section

**All three rules the round before asked for landed**, and the section grew eight times over
anyway. They governed the FORMULAS AT RISK list and that list is now filtered, counted and grouped.
The section's other two lists were untouched, and the per plot fix multiplied every one of them by
156. The one that carried it is `FORMULAS READING A ROW THIS RUN WROTE INTO`, 321 lines on a
single plot of the 18:15 run, which over 156 plots is 50,000 lines, against the 42,570 measured.

**The same rule governs that list now.** `FormulaRepeats.Reading` says it once per shape with its
span and its count. Two things are deliberately no part of the shape. A cell named twice in one
formula is ONE read, because `IF(ISBLANK(J4)," ",ROUND(PI()*(J4/2)^2,0))` names J4 twice and the
line printed it twice. A shared formula and its master are ONE shape, because that is how Excel
stores a filled down column rather than anything about the formula, and keying on it split every
column into two lines. Measured on a four row fixture: 10 formulas, 4 lines.

**And the file opens with what is in it.** `ReportSections.Of` reads the report's own headings and
counts the lines under each, and `WHAT IS IN THIS FILE` prints one row per section, widest first,
with the lines, the blocks and what one block costs. **It is the instrument rather than the cut:**
it decides nothing, leaves nothing out and names no section, so a section added or renamed appears
in it with no change on that side. The counts add up to the body's own line count, the opening
above the first heading counted as its own row.

**What the 19:52 file's other sections came to is UNKNOWN and is not estimated here.** The split
was in no report file and a number reasoned out would be a guess. The next run prints it, and the
cut after this one is made on those numbers.

### One thing corrected in the round message, and it is the only one

The round message said the fault is one of the two cell checks. It is not, and the measurement is
above. Everything else the message states was confirmed rather than corrected.

### Three break watches, one per fault

```
1  IsTheClientsHeader always false    TheClientsHeaderSurvivesAndTheContractReferenceIsThePlotsOwn
                                      RED: Expected "Neighborhood Landscape Design - Zone #2", Actual ""
2  Whose always says the run's row    ADriftOnTheClientsOwnRowIsNamedAsTheClientsRow
                                      RED: Expected "a row the client's list alrea...", Actual "a row this run wrote in"
2  the reason keyed on the plot too   TheGlanceCountsTheTwoComputedNumbersAndSaysWhyOnceRatherThanPerPlot
                                      RED: Expected "1 reason under this l...", Actual "2 reasons under this ..."
3  the shape keyed on the real text   TheFormulasReadingAWrittenRowAreSaidOncePerShapeAndNotOncePerCell
                                      RED: Expected 4, Actual 10
3  the opening not counted            EverySectionIsCountedAndTheyAddUpToTheFile
                                      RED: Expected 10, Actual 8
```

Every one names what was broken. All five files restored byte for byte, checked with `diff -q`,
and the suite green at 1928 after.

### Still open, and not guessed at in code

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the seven 19:52 output
PDFs and the two report files. **The two computed numbers have still never been held against a
workbook Excel has recalculated.**

- **Which of the two things blanked the parks.** The 19:52 report answers it and nobody has read
  that line out of it. The next run puts it at the top in one line
- **What the parks templates' canopy formula reads**, if the guard is what fired. UNKNOWN here and
  answerable only off the client's files or the next run's report
- **What the other sections of the report come to.** Printed by the next run and not estimated
- **A species written into an empty row still carries no family, no genus and no native flag**,
  unchanged

---

## 2026-09-14, eightieth pass. Five faults off the 18:15 run

**Pull request 132, merged into main as `cc85143`.** The runner ran 28 hook cases and 1916 tests
against the pull request head `f8e6644`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1916 tests, 1096 of them KPI, 14 added, against the 1902 main carries** at `d266374`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### 1. The client's filling instructions were printed in the client's PDF

The tool wrote the fields it had values for and left every other one as the template had it, so
`Revit / softscape & shrubs & lawn schedule / total water demand /1000` sat inside the irrigation
box on all 150 PDFs, and the open spaces Ground Cover box, the one really named `0`, carried its
own REVIT SHEET note. **A note in a box reads as an answer.**

Every text field is WRITTEN or EMPTIED now, including every field the tool has no source for and
names nowhere. **The four stage tick boxes and the Reset button are the only fields left as the
template has them, and they are left because of WHAT THEY ARE**: the field's kind comes off the
file's own `/FT`, inherited through the parent chain the way an AcroForm defines it, and only `Tx`
is cleared. A field whose kind cannot be read at all is left alone, because this tool does not
clear what it cannot classify.

The report names every emptied field with why, per plot, and reads back what landed in each. **A
blank box is a decision on the record.**

### 2. The canopy was zero because the rows it read were not the rows it wrote

**Both candidates were checked and the FIRST is what did it**, against the round message's
expectation. `CanopyArea.From` took only matches with `Added` true, which is a species written
into an EMPTY row, so every row the client's list already held was left out whatever its diameter
said. On a plot whose species all match, that is every row, which is why Total Green cover came
out as the planting plus the lawn on every plot of 150.

**The second candidate is real on the same rows and is fixed with it.** The tool writes a diameter
only into a row it creates, so a matched row's canopy comes off the workbook's own diameter. It
travels on the match already, as `WorkbookDiameter`, read off the sheet's own diameter column
where the match was made, so nothing looks the row up a second time.

**The percentage comes right with it rather than by assumption**: it is canopy over area and the
canopy was the zero.

### 3. The pane was not reading unasked, and the header was claiming a read that did not happen

**Every path that can start a read was gone through and none of them fires without a press.**
`Ask(Plots)` is in the Read button's own handler and nowhere else, which is where the fifty
seventh pass put it. `Ask(WhichModel)` reads the document title alone. `Ask(Create)` is a press.

**What was wrong is the claim, not the read.** Two different reads set the header's count and it
printed them the same way: the plots press counts the elements and reads the sheets and schedules
the plot list comes off, in under a second, and the scan inside Create reads the whole document,
in 46.2. Neither number was wrong and the line made the first of them a claim about the model.
`ReadOfTheModel` carries which read produced it now and the line reads `Counted at ...` with a
sentence saying the model itself was not read.

**And the status line never said the read had landed**, which is why the 18:06:58 screen showed a
finished count beside a line still saying it was reading. It says so now. A status line that never
stops saying it is working is the same fault as one that never starts.

### 4. 149 of 156 plots had no detail, because the report took the first run of each template

`WriteAll` took `FirstOrDefault` of each template's runs, so the blocks covered EP-01, FP-16,
HF-01, DM-11, PL-17, SC-03 and MM-01 and nothing else.

**Every plot gets its own block. The counts stay counts of the run because they already are**:
THIS RUN AT A GLANCE and the run accounting both count over every run of the press, above every
block, and a block's own reconciliation reads one of one because a block IS one plot. That is why
a block per plot was chosen over one set of blocks carrying every plot: nothing has to be
re-counted and no number moves. The file preamble is printed once at the top rather than 156
times.

### 5. The formulas section

Three rules, all in `FormulaRepeats` so the heading count and the body come off one list. Only
formulas reading a cell on a row this run wrote into. The heading counts exactly what the body
prints. One formula filled down a column is one finding, with its cells as a span where they
really are one and as a list otherwise.

**A #DIV/0! on a cell this run did not write is out of this section and still counted and still
named**, in THE DIVISIONS at the top of the press, off the unfiltered list. The detail section
reports what the run is answerable for and the glance reports what the workbook will do.

**WHAT THE FILE COMES TO IS UNKNOWN UNTIL THE NEXT RUN**, and that is said rather than estimated.
The split of the 526 between written row and not is in neither report file, so the new line count
cannot be worked out from what was measured. The next run's own heading counts answer it, section
by section.

### One for the team, not a fault to fix

**The plot list reads `On a schedule and on no sheet, 7: -, EP-05, EP-11, EP-12, EP-13, EP-15,
FM-08`.** The first of the seven is a plot whose name is a single dash. It is on a schedule, so
some schedule in the model filters `PRX_Ref Plot ID` on `-`. Nothing in the tool invents a plot,
so it came off the model. **For Bader to find.**

### What is measured and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number above is off the 18:15 output
pairs and the two report files. The emptying is proven against a PDF the tests build carrying a
note in a field the tool names nowhere and a tick box built as a button. The canopy, the header,
the per plot blocks and the formulas section are proven in Core.

**The two computed numbers have still never been held against a workbook Excel has recalculated.**

### Five break watches, one per fault

```
a tick box is emptied with the text fields         2 red, first naming the tick box left alone
the canopy takes only the rows this run created    1 red, expecting 600 and getting 0
the header prints the plots count as a read        1 red, naming the missing sentence
only the first plot of each template gets a block  1 red, EveryPlotOfEveryTemplateGetsItsOwnBlock
formulas the run never affected are at risk        1 red, every FromWrittenRow assert
```

**The second one went green the first time and the test was wrong, not the break.** It asserted on
a `CanopyRow` built by hand rather than going through `CanopyArea.From`, which is the method that
dropped the rows. It goes through `From` now and the same break reddens it. That is the fifth time
in nine rounds a break reddened something other than what it aimed at, and the first time the test
written to catch it was the thing at fault.

All restored byte for byte, checked with `diff -q`, and the suite green at 1916 after.

---

## 2026-09-14, seventy ninth pass. One rule for every label, and every unit off the page

**Pull request 130, merged into main as `b8bf73e`.** The runner ran 28 hook cases and 1902 tests
against the pull request head `27832dd`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1902 tests, 1082 of them KPI, 16 added, against the 1886 main carries** at `d609559`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### The leading space is real and the field was NOT being blanked, which I measured before saying

The warning I ended the last round with was that the green cover label text came from the client's
note and the first real run would confirm it. Bader measured all seven and the note is indeed
wrong: every template holds ` Total Green cover (m²)` with a LEADING SPACE.

**The round message says the lookup as built matches nothing and blanks the field on every plot.
It does not, and I measured that rather than agreeing with it.** The cell's text was trimmed where
it was read, at `LabelledPlaces.Off`, so the leading space came off before the comparison. A
fixture carrying the real label with its space passed all seven cases first time.

Then I took that trim out to find whether it was what saved it:

```
LabelledCellsTests.TheLabelIsMatchedWholeAndWithoutCase        D7 against nothing
ComputedCellTests.TheGreenCoverCellIsFoundByItsLabelAndNames   red
ComputedCellTests.AGreenCoverCellThatHasDriftedIsNamed         no cell on <Mosques> reads
                                                               Total Green cover (m²)
ComputedCellTests.ThePercentageCellSitsTwoColumnsRight         red
RowFiveTests.ThePositionCellOnAParkTemplateIsReadAsHolding     " Architect Engineer"
```

**So it was landing, and nothing anywhere said why.** That is the finding, and it is worse than
the fault the round message expected: a rule living in a bare `Trim()` that nothing names, covering
ONE SIDE of a two sided comparison. A label constant carrying a stray space would still have failed
with no sign of where to look, and the fifth line shows a second thing riding on the same trim.

### One rule, eleven labels, and the ones that were already right are named

`LabelText.Same` is that rule now, asked by every whole label lookup. **Edge whitespace off BOTH
sides, without case, and the inside untouched.** A double space between words is a different label,
because `LOD /  HARDSCAPE SCHEDULES` really carries two.

Every whole label this tool looks up, checked one by one. Eleven, and one carries an edge space:

```
REF :, Date:, Prepared By: (twice), Character, Context    already right, no edge space
% of Total area covered by canopy                         already right, no edge space
 Total Green cover (m²)                                   THE ONE, now spelt as the file holds it
ID_UID *, ES_QUANTITY, QUANTITY UNIT, ROAD_WIDTH          already right, AND one sided
```

**The four street reference columns were the second one sided comparison**, trimming the file's
heading and comparing it against the name as written. They were found by looking rather than by
failing, which is the point of going through every one.

**One lookup is a different question and is left alone.** `ScheduleColumns.Holding` asks
`KpiNames.Holds`, which splits a heading into runs of letters, so edge whitespace cannot reach it.
It asks whether a heading HOLDS a word rather than whether it IS a label. Checked, already immune,
and recorded as such rather than changed.

### The fixture was reading the code back to itself

The workbook fixture wrote `ComputedPlaces.GreenCoverLabel` into the label cell. **A fixture of the
tool's own constant is a fixture of a sheet the client does not have**, and it is exactly how a
case goes green over a lookup that finds nothing. Both labels are written out by hand now, the
green cover with its leading space and `xml:space="preserve"` the way Excel stores such a cell, the
percentage with none, so one label with an edge space and one without go through the same lookup on
the same sheet.

### Every unit on all three forms, read off the page by position

**FOUR CONVERSIONS. Every other field is written in the unit it was read in.** The whole table is
in `kpi-rules.md` with the date. Three conversions are live and the fourth, litres a day into
m³/day, is unreachable because nothing reads a water demand off any schedule. Its reason names the
unit and the division now, so the day the read is built the rule is beside it rather than in a
constant nobody prints.

**THE REPORT PRINTS THE FORM'S UNIT AND IT ALREADY DID.** Checked at the line: the fill is handed
`wanted.Unit` off the form's own field, so the answer to what it prints today is the form's, and a
test pins it rather than leaving it to be read off the code. The unit strings are what the page
prints now, `m²`, `km²`, `m³/day` and `count`, where they were `m2`, `km2`, `m3 a day` and
`a count`.

**AND THE FIELDS NOBODY FILLS ARE ON RECORD WITH THEIR UNITS**, so nobody measures the page again
the day the client annotates one.

**CYCLING PATHS AND PEDESTRIAN PATHS ARE lm ON THE PARKS FORM AND km ON THE OPEN SPACES FORM.** The
same row name, two units, on two forms this one tool fills. Nothing writes them today so it costs
nothing now, and it would cost a thousandfold error in a client document the day somebody adds a
note to one and reads the other form's unit. **The Bridges and Catwalk row spells Lenght**, the
client's spelling, written as it is the way PRX_Furniture Lenght already is.

### What is measured and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The labels and the units were measured by
Bader off the client's own seven templates and three forms. The lookup is proven against workbooks
the tests build carrying the measured label text.

**The two computed numbers have still never been held against a workbook Excel has recalculated**,
which stands from two rounds back.

### Two break watches

```
the label side of the rule is no longer trimmed    5 red, each naming the pair that stopped
                                                   matching, first " Total Green cover (m²)"
                                                   against "Total Green cover (m²)"
the road length prints the source's unit           4 red, every one naming km against m
```

Both restored byte for byte, checked with `diff -q`, and the suite green at 1902 after.

---

## 2026-09-14, seventy eighth pass. The other two formulas, measured and guarded

**Pull request 128, merged into main as `3fcb4c4`.** The runner ran 28 hook cases and 1886 tests
against the pull request head `eeff6c1`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1886 tests, 1066 of them KPI, 7 added, against the 1879 main carries** at `2c698c7`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### The open question is closed, by a measurement rather than by an argument

The round before computed the two numbers and guarded only the canopy column, because the text of
the other two formulas was measured nowhere in this repository. The one thing near it was `H9 =
H8/Area` on EXISTING PARKS, one template, and building a discovery rule on that is the shape this
repo has paid for five times. **Bader measured both on all seven.**

```
TOTAL GREEN COVER, the same shape on all seven in two row layouts
  EXISTING PARKS, FUTURE PARKS, STREETS    D9 = F9+F11+H11
  HEALTHCARE, MOSQUES, PARKING, SCHOOLS    D8 = F8+F10+H10

PERCENTAGE CANOPY, in SECTION 3 and not section 1
  HEALTHCARE, MOSQUES, PARKING, SCHOOLS    label C31, value E31 = IF(Area<1," ",F8/Area)
  STREETS                                  label C32, value E32 = IF(Area<1," ",F9/Area)
  EXISTING PARKS and FUTURE PARKS          NO SUCH LABEL AT ALL
```

Canopy plus planting plus lawn, and canopy over area, which is what the tool already computes. **So
the arithmetic was right and only the checking was missing.** The open question is taken out of the
log by this entry.

### The note is wrong about where the percentage is, which is the second one

The client's own note says the cell to the right of `Total area covered by canopy`. **There is no
such label in section 1 on any template.** The cell is in section 3 under `% of Total area covered
by canopy`, and its value sits TWO columns right of the label rather than one, the cell one to the
right being empty on all five that carry it.

That is the second note on these forms measured to be wrong about its own subject. The first was
the TOTAL Shrubs tooltip last round. Both were found by measuring rather than by reading, which is
what the rule that a note is checked and never used to decide anything is for.

### The one form that asks for it is fed by the two templates that do not carry it

Percentage Total area covered by canopy is on the Parks PDF alone, which EP and FP plots reach, and
EXISTING PARKS and FUTURE PARKS have no such cell. **So the percentage check answers nothing to
check on every run the tool makes today.**

It is a fact about the client's files rather than a fault to fix. The tool holds the canopy and the
area, so **it still computes and writes the number**, and the report says the workbook has no cell
to hold it against. An absence is not a drift, and `SummaryCellCheck.NothingToCheck` is what tells
the two apart rather than a reading of the reason.

The check is built anyway, because it is right and because it fires the day a park template grows
the cell or another form grows the field.

### How both are checked, with no letter anywhere

**Two row layouts are why neither cell is a letter**, the lesson row 7 and row 5 both taught.
`ComputedPlaces` holds the two labels and the distance right of each, and it is read through the
same `LabelledPlaces` lookup every other labelled cell uses. **It sits beside `LabelledPlaces.All`
rather than inside it**, because those two cells hold the client's own formulas and a value written
into one would destroy them. A test says so in those words.

`GreenCoverCell` reads the cell the label chose and requires exactly three single cells, two of
them the template's own map's planting and lawn cells. **The third IS the canopy cell**, learnt
from the formula rather than written in, and carried to the percentage check, so the two cannot
name two different canopies. `PercentageCell` then requires exactly the canopy cell and the map's
area cell. `IF(Area<1," ",F8/Area)` reads F8 and whatever `Area` points at, through the new
`FormulaCell.SingleCellsRead`, **so the defined name is checked as well as the cell** and no
formula text is matched anywhere.

**A label named nowhere is a refusal for the green cover and nothing to check for the percentage**,
because the measurement differs: all seven carry the green cover label and two of the seven carry
no percentage label at all.

**The green cover label text comes from the client's own PDF note** and is confirmed by where it
lands, D9 on three templates and D8 on four. The report prints the cell the label chose on every
run, so a template whose label reads anything else is one line rather than a silence.

### What is measured and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and neither check has been run against a real
client template. Both are proven against a workbook the tests build in the MOSQUES shape, with the
labels at C8 and C31 and the measured formulas at D8 and E31. **The first real run is what confirms
the green cover label text**, because the report prints the cell it chose.

**The two computed numbers have still never been held against a workbook Excel has recalculated.**
That stands from last round and is still the first thing the next run should do.

### Two break watches

```
the green cover cell is never held against this tool's three cells   1 red, the drifted case,
                                                                     expecting False and getting True
the percentage value is taken one column right rather than two       2 red, one naming 2 against 1
```

Both restored byte for byte, checked with `diff -q`, and the suite green at 1886 after.

---

## 2026-09-14, seventy seventh pass. Two numbers computed, and every unit named

**Pull request 126, merged into main as `5fca585`.** The runner ran 28 hook cases and 1879 tests
against the pull request head `9cabd2d`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1879 tests, 1059 of them KPI, 58 added, against the 1821 main carries** at `e1d8848`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### My correction to the TOTAL Shrubs note was too narrow, and Bader's reading holds

The round before said the open spaces TOTAL Shrubs note is NOT wrong, on one measurement: the
field's `/V` reads `sum of the above or from revit`. **That was one key of the dictionary.** The
field also carries a `/TU`, the tooltip a person sees, and reading all three forms' tooltips this
round found a real wrong label:

```
PARKS  y=570.2  Proposed Shrubs Area (m²)   /TU(Existing Shrubs)
PARKS  y=560.3  TOTAL Shrubs Area (m²)      /TU(Existing Shrubs)  /V(Sum of the above or from revit)
OPEN   y=346.9  Proposed Shrubs.1.1         /TU(Proposed Shrubs)  /V(sum of the above or from revit)
ROADS           every tooltip matches its own row
```

So there IS a wrong label in these files, on the PARKS form rather than the open spaces one, and
a tool matching by note would put the existing shrub area into a box the page prints TOTAL in a
client document. **The conclusion never moved and the tool never followed the note:** it writes
`shrubs.TotalSquareMetres`, existing plus proposed, on all three forms, and that was already
pinned by a test. What is new is that the note on that field is recorded as not to be trusted.

**A correction that checks one field of a record and calls the record right is the same shape as
reading a schedule value by cell position.** It reached the right answer about the open spaces
form and the wrong answer about the file set.

### The position is the third record now, and every field was gone through one by one

The name was checked and the note was checked and where the field sits was not. Every field of
all three forms carries its measured x and y, read off each `/Rect`, and `PdfFormCheck` compares
them with half a point of room against rows about twenty points apart. **A field that has moved
writes nothing and is named with where it sits beside where this tool measured it.**

Then every field matched by note was gone through, and the ones that were right are named as
well as the one that was not, because a field checked and found right reads exactly like one
nobody looked at. Fourteen fields on each of three forms, one disagreement, and it is the one
the round message named. The list is in `kpi-rules.md`.

### The two workbook cells are COMPUTED, and the working is printed

Neither number is in the file this run wrote, because the patcher drops every cached formula
result on purpose so Excel recalculates. Opening a hundred and fifty workbooks by hand is not a
workflow and relaxing the cache rule brings back the stale zeros that took four rounds to kill.

```
Total Green cover  = canopy + planting + lawn
Percentage canopy  = canopy / area
```

Planting, lawn and area are the three totals this run wrote into the workbook's own cells, handed
over on `PdfWorkbookNumbers` rather than worked out a second way. The canopy is `CanopyArea.From`
over every row this run wrote a count into and no other, using the workbook's own column:

```
L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))
M21  =IF(ISBLANK(B21)," ",L21*B21)
```

**The rounding is INSIDE, per tree.** Eight metres across is fifty square metres each and three of
them are 150, where rounding the sum gives 151. A row with no diameter is named and is not counted
as nought, because its own L cell returns a space in the workbook too.

**Adding printed numbers with the working shown was already allowed. This is that rule one step
further** and it is written into `kpi-rules.md` out loud, because it is the first time this tool
produces a number no schedule printed. Every computed field prints under COMPUTED, not read, with
its parts.

### The guard, and the half of it that cannot be built yet

`WorkbookArithmetic.Canopy` reads the output's own formulas and, for every row this run wrote a
count into, looks on that row for a cell carrying the text the tool knows, built with the diameter
column that sheet's own heading chose. **A row whose formula differs, or that carries none, blanks
BOTH computed fields and names the row and every formula on it.**

**A check that could not be made and nothing to check are two different things**, told apart by a
property and never by reading the reason. A run that wrote no tree row has no canopy formula in
play at all, so its green cover is the planting and the lawn and it is written. A workbook whose
formulas were never read computes nothing.

**THE OTHER TWO FORMULAS CANNOT BE CHECKED AND THAT IS SAID RATHER THAN FAKED.** The round asked
for the Total Green cover cell and the percentage cell to be read and compared against what the
tool knows. **The tool knows neither, because the text of neither is measured anywhere in this
repository, on any of the seven templates.** The one thing near it is `H9 = H8/Area` on EXISTING
PARKS, one template, recorded in `kpi-rules.md` from the 2026-09-09 check. Building a discovery
rule on one template is the shape this repo has paid for five times, D7 and the shrubs subtotal
among them, so nothing looks for either cell. The report says
`WorkbookArithmetic.NotCheckedAgainstTheWorkbook` beside both numbers.

**CLOSED IN THE SEVENTY EIGHTH PASS, BY THE MEASUREMENT IT ASKED FOR.** Bader measured both cells
on all seven templates. The green cover is canopy plus planting plus lawn in two row layouts, D9 on
three templates and D8 on four, and the percentage is canopy over area in section 3 under a label
the client's note names wrongly. Both are guarded now, found by their labels. The entry at the top
of this file has the numbers.

### The two units, and the forty that need none

**LENGTH ON ROADS ASKS km AND THE SOURCE GIVES m.** The reference file's QUANTITY UNIT reads m and
the workbook's Streets Total Length (m) cell takes the metres unchanged, measured on ST-100130
reading 174. So the metres are divided by a thousand for the PDF alone and the Excel is left
exactly as it was. 330.66 metres lands as 0.33066.

**A row in any unit but m never reaches the conversion.** `StreetReferenceFile.For` already
refuses it, naming the plot, the row and what the unit said, and the PDF field carries that
reason. Checked at the line rather than assumed.

**PERCENTAGE CANOPY IS A RATIO TIMES A HUNDRED WITH NO SIGN**, because the form prints one in its
own unit column. Checked against the client's filled ANH-006-NP-100002: area 771, 0.000550 square
kilometres greened, percentage 71. Eleven trees eight metres across are 550 square metres, so the
square kilometres land on their 0.00055 exactly, and 550 over 771 is 0.7134, which their form
prints as 71.

**And then every field on all three forms was gone through**, forty two of them, and the whole
table is in `kpi-rules.md` and in a test written out by hand. Two conversions, forty none. The
ones that need none are named as well as the two that do, because a unit that matches by luck
reads the same as one nobody checked.

**The Roads form's Total areas to be greened is filled from ITS OWN note**, which names the canopy
cell where the other two name Total Green cover. Each form gets what its own file says and the
working says which of the two the number is. Which the client means stays the open question it
was.

### What is measured here and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The tooltips and the positions were read out
of the client's three real PDFs. The canopy formula and the arithmetic guard were proven against a
workbook the tests build, not against a client one. **The two computed numbers have never been
held against a workbook Excel has recalculated**, which is the first thing the next run should do:
open one filled workbook, read its Total Green cover and its canopy percentage, and hold them
against what the PDF beside it says.

### Four break watches, each naming what it broke

```
TotalShrubs writes the existing area          4 red, first naming 84 against 30
Total Green cover drops the lawn              3 red, first naming 620 against 560
the canopy guard is ignored                   3 red, the drifted case naming the empty reason
the road length is not divided                2 red, both naming 0.33066 against 330.66
```

All restored byte for byte, checked with `diff -q`, and the suite green at 1879 after.

---

## 2026-09-14, seventy sixth pass. A PDF beside every workbook

**1821 tests, 1001 of them KPI, 58 added, against the 1763 main carries** at the branch point
`bb10f9d`, measured by running the suite at that commit, 28 hook cases unchanged, build zero
warnings. **No audit finding is closed, renumbered or reordered: read off the three files, 63
numbered findings, 19 carrying a FIXED mark, 44 open.**

### The three forms were read off the files, and the files disagree with the round message three times

Every field name and every note in `PdfForms` was read out of the client's own three PDFs and
then **run back against those files**: all three forms matched, with nothing missing and no note
differing. That check is the foundation of the round and it is why the three differences below
are stated as measurements rather than opinions.

- **THE SOURCES ARE IN THE FIELD'S VALUE, NOT ITS DEFAULT VALUE.** Only the four stage tick
  boxes carry a default at all, and on open spaces and roads that default is a tick on all four
  while the real state sits in the value
- **The open spaces TOTAL Shrubs note is NOT wrong.** `Proposed Shrubs.1.1` holds
  `sum of the above or from revit` and so do the other two forms. All three mean the sum, which
  is what Bader confirmed, but the fault he confirmed is not in these files
- **Parks and Roads name NO Project Type field.** It is not typed text to be left alone, it is
  absent, and only the open spaces form asks for a component

**AND A FOURTH, WHICH IS AN OPEN QUESTION FOR BADER.** The ROADS form's `Total areas to be
greened` names `excel the cell on the right of "Total area covered by canopy "` where parks and
open spaces both name Total Green cover divided by 1,000,000, for a field all three call the
same thing. It is held exactly as the file has it so the check does not refuse the form over the
client's own copy and paste. **Which cell the client means on a road is not something this tool
can settle**, and it is one of the fields left blank anyway, under the section below.

### The library was picked by search and rejected by measurement

**PDFsharp 6.2.2 is the only serious open candidate**: MIT, netstandard2.0 so net48 consumes it,
and it carries `PdfAcroForm`, `PdfTextField` and `PdfCheckBoxField`. Opened on the client's own
Parks form it read all 42 fields and threw on the first one touched, `No appropriate font found
for family name 'Courier New'`, because setting a value makes it REGENERATE the field's
appearance stream. **Regenerating the appearance is redrawing what the client drew**, in
whatever font the machine resolves, which is the one thing this round forbids.

Rejected on licence: iText 7 is AGPL or paid, and IronPDF, Aspose, Syncfusion, DevExpress,
Apryse and DynamicPDF are all per seat commercial, which the twenty person rule rules out.

**So there is no package at all, and that is the workbook's own rule applied to a PDF.** A PDF
supports an incremental update by its own design: the client's bytes are copied whole and the
changed objects are appended after them with a new cross reference section. Measured on the
three real files, and a test asserts the source bytes are the first bytes of the answer:

```
Parks        2,121,502 in   2,123,241 out   42 fields before and after
Open spaces    937,743 in     939,292 out   49 fields before and after
Roads        1,926,881 in   1,928,708 out   34 fields before and after
```

The Reset button, the tick boxes and the typed fields all survive, and the written values read
back through a second and independent reader.

**It works because these files hold no object streams.** Zero `/ObjStm` on all three, no
encryption, so every field dictionary is a plain top level object. A form that arrives with
object streams is refused by name rather than half written.

**The stale appearance is dropped and NeedAppearances is set.** Keeping the old appearance
beside a new value shows the client's note on screen over the number underneath it, and a stale
word that looks like an answer is the worst thing this tool can put in a file.

### 1. The shrubs split by phase, and it needed no new schedule read

The phase rows have been read since the 0928 run: a group prints one subtotal per phase and then
the group total, and `GroupSubtotal.Phases` has carried them ever since. **So the split is a read
off what is already there rather than the big piece the round expected.**

**IT IS NOT A SECOND RULE BESIDE THE ONE FOR TREES.** `ShrubsByPhase` sorts each phase row by
`CountedGroups.SheetFor`, the same method the species merge asks, so Street Design counts as
Proposed on STREETS and is left out elsewhere with nothing written twice. FM-05 prints Proposed
361 and Street Design 459 under a group total of 820: on MOSQUES the proposed shrubs are 361 and
on STREETS they are 820, and the group total row is never the answer.

### 2. Ground cover is NOT printed apart, so nothing is written and it is named

The schedule prints SHRUBS & GROUND COVER as one group over one set of rows, on every scan this
project has taken. **Nothing prints ground cover on its own.** Splitting one printed number into
two would be a number nobody measured, so the field is left blank with that reason on every plot.

### 3. Total shrubs is existing plus proposed on all three forms

Recorded, with the correction above that the note Bader confirmed as wrong is not the note these
files hold. Never the group total row.

### 4. A plot whose workbook was not written still gets its PDF

Bader's decision. The fields that read the workbook are named as not written and everything that
comes from Revit still goes in, and the reason reads differently from the one a written workbook
gets, because they are different facts.

### THREE MORE FIELDS ARE BLANK ON EVERY RUN, AND ONE OF THEM IS A REAL FINDING

**Total areas to be greened and Percentage Total area covered by canopy cannot be read out of
the workbook this tool writes.** Both are cells the workbook COMPUTES, and the patcher drops the
cached result of every formula cell on purpose so Excel recalculates rather than opening on
stale zeros, which is a rule this file has carried since the first real output read seven zeros
beside correct inputs. **So the number is not in the file, and reading it back reads an empty
cell.** Computing it here would be working the client's own formula out again, which this tool
never does.

**That is a question for Bader and there are three ways out**, none of them this round's to
pick: the team opens each workbook once and the PDF is filled on a second press, or the tool
learns to compute those two cells, or the cache rule is relaxed for them and the stale zeros
fault comes back. The ordering rule still stands and is still written down, because the day one
of those answers lands the Excel must already have been written.

**Irrigation water demand is the third.** The shrubs and lawn schedule prints L/DAY as its last
column and nothing in this tool has ever read it. There is no number to write and adding that
read is a round of its own.

### The three break watches, and the first one found a weak test

**Break 1, a phase the template leaves out falls through.** **0 RED THE FIRST TIME.** The case
that should have caught it asserted the three NUMBERS, which the break leaves right, because a
left out phase then reaches the sheet lookup and lands in the no sheet list instead. The numbers
were the same and the reporting was wrong. The case now pins the left out list and the phases
with no sheet apart, and the same break reddens it at
`Assert.Empty() Failure: Collection was not empty, ["Street Design"]`. **A test that asserts
only the number cannot catch a rule about which bucket a thing went into.**

**Break 2, the note stops being checked and only the field name is.** **3 red**, led by
`AFormWhoseNoteHasMovedIsLeftAloneAndNamed`, with the end to end case and the glance behind it.

**Break 3, the stale appearance is kept beside the new value.** **1 red**,
`TheStaleAppearanceIsDroppedAndNeedAppearancesIsSet`, printing the dictionary that still carries
`/AP<</N 5 0 R>>` next to the new value.

All four touched files were copied out first and `diff -q` against the copy after restoring, and
the restored code was run against the client's three real forms again afterwards.

### The merge

Pull request 125, merged into main as `524231f`, through the API with the commit title and the
commit message both passed on the call. **The runner ran 28 hook cases and 1821 tests against the
pull request head `ddaef08`, 0 failed and 0 skipped.** The merged tree is byte for byte the branch
head and the message off main carries neither a co-author credit line nor a generated-by footer.

### Still open

The 44 audit findings. The three blank fields above, each a question for Bader. The roads form's
canopy note. `CLAUDE.md` line 209, still carrying the NS-19 and NS-06 claim the seventy fifth
pass corrected, which is the repo wide file and not this task's. A row written into an empty one
carrying no family, no genus and no native flag. The GOVERMENT BUILDING folder.

---

## 2026-09-14, seventy fifth pass. The client's note decides a tie, and a reason reaches the file

**1763 tests, 943 of them KPI, 16 added, against the 1747 main carries** at the branch point
`9fbcea4`, measured by running the suite at that commit, 28 hook cases unchanged, build zero
warnings. **No audit finding is closed, renumbered or reordered: read off the three files, 63
numbered findings, 19 carrying a FIXED mark, 44 open.**

### What the 14:29 run settled, and both are closed by a count

**THE NOTE HOLDS.** NG05, 156 plots, 102 workbooks:

```
of 156 plots wanting an area
  98   took it off RCRC_OUT OF SCOPE (PRESENTATION), the type the client's note names
   0   took it off a type the note does not name
  58   chose no region at all
```

**My 9 September reading of NS-19 and NS-06 was wrong.** They carry BOTH regions and one column
was read. The open question that has been in this log since the seventy third pass is closed,
and it is closed by the region count that pass added for exactly this, which is a measurement
rather than an argument.

**NO #DIV/0! ANYWHERE IN THE PRESS**, so none of them divides by a cell this run wrote either.
The two the 09:18 run left unexplained are not in this model's output. That question is closed
too, by the count the seventy fourth pass added. **Both questions were answered by the two lines
put at the top of the report a round before**, which is the whole case for having put them there.

### 1. The note's type decides when two regions hold an area

**51 of 78 street plots wrote nothing on that run**, every one because two regions held an area
and the tool asked which, and every one of the 51 offered the note's type as one of its two. 51
questions, 51 clicks, and a press again after each one.

**Bader's decision, 14 September.** `RegionChoice.Pick` in order: a person's pick wins, one
region holding an area answers itself whatever it is called, and more than one with the note's
type among them takes the note's type. **The question stays where the note's type is not among
them**, and so does the note's type held by two at once, which no model has shown and which the
note cannot separate either. The unchosen reason says which of the two it was, because they need
different answers from a person.

**RCRC_CADASTRAL LIMIT is written nowhere in the tool.** The type name lives in
`RegionChoice.TheNoteNames` and in nothing else. The test fixture carries a third name,
`RCRC_SOMETHING NOBODY HAS MEASURED`, because a test that wants the old refusal now has to build
a pair without the note's type in it, and four existing cases were moved onto it by hand rather
than deleted.

**`WhyUnchosen` ASKS `Pick` RATHER THAN DECIDING AGAIN**, and that was not optional. Two rules
for one question is the fault this repo keeps paying for and it bit inside the same hour: the
first version left `WhyUnchosen` deciding for itself and `APairTheNoteSettlesHasNoUnchosenReason`
went red, which is a pick that chooses beside a reason still printing a question.

**THE REPORT SAYS WHO CHOSE.** `RegionPick` carries the type and the route as ONE record, set
where the choice is made, and `PlotReading.ChosenRegionPick` holds it. One new column in the
region table, `how it was chosen`, four values: nothing chosen, chosen by hand on the pane, the
only region holding an area, and more than one held an area and this is the type the client's
note names. **A count of picks the note made and a count a person made are two different facts
about a run**, and a column printing only the type name says neither.

**And the pick used to DROP the plot's UID2.** `PlotReading.WithChosenRegion` rebuilt the
reading with every argument but the last, which defaults to null, so a plot answered after a
refusal came back with no `PRX_Plot_UID2` and was refused a second time with `no PRX_Plot_UID2
was read off this plot's first sheet`. **That is a sentence about the model and it was about
that method.** Found while reading the pick path for this item, fixed, and pinned by a test that
also builds the path off the carried value. A default that reads as a deliberate empty is how a
whole link in a chain goes missing without a word, which this tool has paid for once already
with the three the team types.

### 2. A reason that points at a screen is not a reason

The report said this on **102 rows of 7,083 lines**, and the reason itself appeared NOWHERE in
the file.

**SEVEN PLACES PRODUCE A REFUSAL THAT CAN REACH THE FILE. TWO WROTE A POINTER**, and both
through one method:

```
the plot row, PlotOutcome.Why                        POINTED, fixed
the template row, CreateWords.SomePlotsWroteNothing  POINTED through the same method, fixed
a plot no route placed, TemplateSplit's own Why      already right
a plot whose path refused, PlotWorkbookPath.Why      already right, the EP-05 line
a template whose split refused, TemplateSplit        already right
the patch's own refusal                              already right, said in full both ways
the glance's streets area, RunAtAGlance              already right, it asks the method above
```

So five of the seven were already right and the two that were not were one method seen twice.
`CreateWords.WhyNothingWasWritten` takes where the answer is going as its only new argument:
**the file gets every reason written out and the PANE keeps the count and the pointer**, where
the reasons really are in red directly above the button and where the 0928 run printed the same
four lines twice on one screen. One method, so the two cannot come apart anywhere else.

**Where a refusal genuinely has several reasons the file holds ALL of them**, with a test over a
run carrying two, because a report short of the second reads exactly like a plot that had one.

**NOTHING WENT RED WHEN I MADE THIS CHANGE**, which is the finding inside the finding: no test
anywhere pinned what the file carried. `ReasonInTheFileTests` is seven cases now, and one of
them pins the pane's pointer so the fix cannot travel too far the other way.

### The three break watches, and what each reddened

**Break 1, the note stops settling a pair.** **7 red**, led by
`TwoRegionsHoldingAnAreaTakeTheOneTheNoteNames` with `Expected: "RCRC_OUT OF SCOPE
(PRESENTATION)" / Actual: ""`, and reaching the reconciliation and the streets cases behind it.

**Break 2, the file writes a pointer again.** **3 red**,
`ThePlotsOwnReasonIsWrittenIntoTheFileRatherThanPointedAt` failing on
`Assert.DoesNotContain` finding `shown in full above the Create button`.

**Break 3, the pick drops the UID2 again.** **1 red**, `AChoiceKeepsThePlotsUid2...` with
`Expected: "ANH-007-MO-100019" / Actual: ""`.

All three files were copied out first and `diff -q` against the copy after restoring.

### The merge

Pull request 124, merged into main as `5706bd2`, through the API with the commit title and the
commit message both passed on the call. **The runner ran 28 hook cases and 1763 tests against the
pull request head `3ae4868`, 0 failed and 0 skipped.** The merged tree is byte for byte the branch
head and the message off main carries neither a co-author credit line nor a generated-by footer.

### OPEN, and it is Bader's call because it is not this task's file

**`CLAUDE.md` line 209 still carries the corrected claim**, that NS-19 and NS-06 hold their area
on cadastral and that the type name cannot decide it. It is the repo wide file rather than KPI
territory, so this round did not touch it. `.claude/rules/kpi-rules.md` and `RegionChoice` both
carry the count that corrects it. **Two records of one fact, knowingly left standing for one
round**, which is the shape this repo pays for, so it wants a round of its own or a word from
Bader.

### Still open, unchanged by this round

The 44 audit findings. A row written into an empty one carrying no family, no genus and no
native flag. A matched species whose height or diameter in Revit differs from the client's row.
The GOVERMENT BUILDING folder. Whether rows edited in the pane should outlive it.

---

## 2026-09-14, seventy fourth pass. Three lines at the top, so a run can be checked at a glance

**1747 tests, 927 of them KPI, 15 added, against the 1732 main carries** at the branch point
`16bed93`, 28 hook cases unchanged, build zero warnings. **No audit finding is closed, renumbered
or reordered: read off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

### What this round is, and what it is not

**All three questions were answerable before this and all three were spread over hundreds of
lines.** The streets area sat in one block per plot, the region type in one row per plot, and the
divisions in one formula section per template. On the 156 plot run that is a person reading a
2,000 line file to count three things. **Nothing was moved and nothing new was measured.**
`RunAtAGlance.Of` counts what the runs already carry and `THIS RUN AT A GLANCE` prints it as the
FIRST section, above the run's accounting and above every per template block.

### a. The streets area, counted off the outcome and off the map

A street plot got a value when the template's OWN area cell landed in its output holding
something. **The cell comes off the map, so nothing here names H8**, and the count is read back
off the file rather than taken from what the fill set out to write, which is the rule every other
count in this report already follows. A cell that landed holding nothing did not get a value, and
a test says so.

**The plots that did not get one are NAMED with the reason their own run recorded**, so the line
under the count says the same thing as the plot's block further down. A count with nobody named
sends a person back through 78 blocks, which is what this section exists to save.

**A press with no street plot says so rather than counting nought of nought.** No street plot in
the press is not the same fact as every street plot failing, and the two read identically as a
bare 0 of 0.

### b. The region type, counted as the run found them

Two counts and a list. **No second type name is written into the code.**
`RegionChoice.TheNoteNames` is held because it is the note being checked and it sorts to the top,
so the two counts the team asks for are the first two rows whatever a model holds.
**RCRC_CADASTRAL LIMIT is deliberately not named beside it**: a rule naming two types counts a
model's third type under nothing, and a test builds a plot on a type neither name covers and
checks it is counted under its own name.

A plot that chose no region is counted apart from every type. Two regions holding an area is a
question waiting on a person and none holding one is a plot with nothing to read, and neither is
a disagreement with the client's note.

### c. The divisions, and the second number is the one that matters

`FormulaAtRisk` gained two properties, `IsDivideByZero` and `DivisorThisRunWrote`, set where the
risk is built. **A signal that travels in the data is not a signal**, which this repo has paid
for once already: a reason reported by printing a marker word into the message it described, and
the commit carrying that fix refused by its own message. A counter searching `Reason` for
`#DIV/0!` would count a reason that merely talks about a division.

**Measured, and it is the reason the properties exist.** Break watch 2 stopped the two flags
travelling and **every one of `DivideByZeroTests` stayed GREEN**, because those five cases assert
the reason SENTENCE. Three of the new cases went red. A test file about a finding cannot catch
the finding losing its kind while it only reads the words the finding prints.

### The glance heading is the one heading carrying no count

Every other heading in this report prints how many rows sit under it, so a section that found
nothing reads differently from one nobody filled in. **Three is how many questions there are
rather than how many of anything this run found**, and a constant sitting where a count goes is a
number that reads as a measurement. It was written with the count first and taken out after
reading the sample report, and a test now pins it and checks the heading below it still counts.

### Two stale docstrings corrected on the way past

Both were descriptions of the tool that contradicted the tool, which is a shape this repo already
carries three entries about.

- `KpiValue.StreetsRoadWidth` read `H8 is this times the length and the workbook computes it, so
  nothing writes there`. The client emptied H8 in the seventy third pass and the tool writes it
- `Reconciliation.AreaWanted` read `STREETS types the road width and the total length by hand and
  the sheet works the area out`. The reference file fills both and the area is read off the link

### The three break watches, and what each reddened

**Break 1, a landed cell holding nothing counts as a value.** **1 red**,
`AnAreaCellThatLandedEmptyIsCountedAsNotWritten`, `Expected: 0 / Actual: 1`.

**Break 2, the kind and the divisor stop travelling on the finding.** **3 red**,
`TheKindAndTheDivisorAreProperties` first at `Assert.True(ours.IsDivideByZero)`, with the count
and the report cases behind it. `DivideByZeroTests` stayed green, which is section c above.

**Break 3, the note's type is no longer forced to the top of the list.** **1 red**,
`TheTypeTheNoteNamesIsTheFirstRowEvenWhenItIsTheSmallerCount`, printing both orders.

All three files were copied out first and `diff -q` against the copy after restoring, so the
suite that produced 1747 is over the real code.

### The merge

Pull request 123, merged into main as `b65cfad`, through the API with the commit title and the
commit message both passed on the call. **The runner ran 28 hook cases and 1747 tests against the
pull request head `8cf8204`, 0 failed and 0 skipped.** The merged tree is byte for byte the branch
head and the message off main carries neither a co-author credit line nor a generated-by footer.

### Still open, unchanged by this round

The 44 audit findings. The client's note against NS-19 and NS-06, which is what the region count
above now answers on a real street run. A row written into an empty one carrying no family, no
genus and no native flag. A matched species whose height or diameter in Revit differs from the
client's row. The GOVERMENT BUILDING folder. Whether rows edited in the pane should outlive it.

---

## 2026-09-14, seventy third pass. STREETS takes an area again, because the client reissued it

**1732 tests, 912 of them KPI, 28 hook cases**, against the 1729 main carries at the branch
point. Build zero warnings. **No audit finding is closed, renumbered or reordered: read off the
three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.** Finding 31 keeps its
FIXED mark and gains a note, for the reason under item 1.

### The measurement this round rests on, which is one cell

The client reissued the STREETS template and it was diffed against the one that ran this
morning. **4,160 cells against 4,159, no named range moved, no other sheet touched.**

```
H8, Streets Total Area (m2)     was  =Width*F8     now  empty
```

Their reference copy says where the three cells of that row come from:

```
H8 = REVIT 00 LINK / ID FILLED REGION "RCRC_OUT OF SCOPE (PRESENTATION)" / PRX_Intervention Area
D8 = EXCEL FILE "Scope Validation 21072026" / COLUMN O1 "ROAD_WIDTH"
F8 = EXCEL FILE "Scope Validation 21072026" / COLUMN H1 "ES_QUANTITY"
```

**Three cells on one row, three sources, and the workbook computes none of them.**

### 1. STREETS takes an area again, and finding 31 is reversed by the TEMPLATE changing

`KpiTemplates` gained one entry, `new MappedCell(KpiValue.Area, "H8")`, and every area path
turned back on for STREETS with nothing else touched. That is the whole change, and it is the
answer to the round's own instruction not to hard code H8: **`KpiTemplate.TakesNoArea` reads the
MAP**, so the filled region read, the region choice, the reconciliation refusal, the identical
area confirm and the report section all ask that one thing and all came back on together.

**Finding 31's entry keeps its FIXED mark and carries a note saying the template changed.** The
finding was right when it was written, its fix was right, and writing it off as wrong would put
a false record in the audit file. What is recorded under it is REVERSED IN THE SEVENTY THIRD
PASS, pull request 122, BECAUSE THE TEMPLATE CHANGED. **The counts are untouched at 63, 19 and
44**, because a reversal is not a finding and renumbering would break every reference to the
other 62.

**`AreaIsTypedByHand` is renamed `TakesNoArea`, and the old name was a lie from the first day.**
No template ever typed an area by hand. STREETS computed one from two cells, which is not typing,
and the other six read one from the model. **A name that tells one template's story stops being
true when that template changes**, and this one was read by four call sites in two projects. The
words it fed said typed by hand on the pane, in the create plan and in the template block, and
all three now say the template names no area cell, which is what the property really answers.

**No template names no area cell today**, so that path is live for nothing. It is kept, with
that fact written down beside it, because the map can still express such a template and it is
one client reissue from being needed again. This round is the proof.

### 2. The client's note names a type and at least two street plots disagree. NOTHING chooses on it

Their note names `RCRC_OUT OF SCOPE (PRESENTATION)`. The rule is unchanged and nothing was
narrowed to that type: the plot's own regions are read, the one holding a non zero area is
taken, and more than one asks. Measured 9 September, and it is why:

```
DM-11, DM-12, DM-13     area on OUT OF SCOPE, cadastral 0
NS-19, NS-06            area on CADASTRAL LIMIT, out of scope 0
```

**NS-19 and NS-06 are street plots**, which are the plots this note is written for.

`RegionChoice.TheNoteNames` holds the note's type as data and `AgainstTheNote` turns a chosen
type into `the type the note names` or `NOT the type the note names` or `nothing chosen`. The
region table in the create report carries that as a column beside each plot's real answer, and
the closing line under it says outright that nothing chooses on the name.

**OPEN QUESTION FOR BADER.** The client's note says the area is on OUT OF SCOPE and NS-19 and
NS-06 hold it on CADASTRAL LIMIT with out of scope at 0. Is the note wrong for street plots, are
those two plots modelled wrongly, or is it correct for both types on different plots and the
note simply names the commoner one? The tool reads neither name and takes whichever region holds
the area, so it is right either way today. **A run over 78 street plots now answers it by
counting**, one column per plot, which is a measurement rather than an argument. Nothing changes
in the code until he says so.

### 3. A FIXTURE WHOSE NAMES ARE NOT THE MODEL'S CANNOT CATCH A RULE ABOUT NAMES

Break watch 2 broke `RegionChoice` to settle two regions holding an area on the type the note
names, which is exactly the rule item 2 forbids. Three tests went red and **all eight cases of
`RegionChoiceTests`, the file that owns the rule, stayed GREEN.**

Its fixture read `CADASTRAL LIMIT` and `OUT OF SCOPE (PRESENTATION)`. The model's types are
`RCRC_CADASTRAL LIMIT` and `RCRC_OUT OF SCOPE (PRESENTATION)`, measured in the 00 link, 124 and
155 regions. So a rule keyed on the real type name walked past every case in the file written to
protect that rule. The names are the model's own now and a case was added for the note's type in
both orders. The same break then reddens 2 of them.

**A test file about type names that does not carry the model's type names is a test file about
nothing**, and it read exactly like a passing suite for as long as it stood.

### 4. Read every rule off the template, never off the notes copy

The reissued pair disagrees with itself about rows:

```
                          notes copy      template
existing canopy sum       M93             M102
native count over         H3:H91          H3:H101
```

**THE TEMPLATE WINS.** The notes copy says where a value comes from and nothing else, and the
annotations were plainly made on an older file.

**This is the SECOND time an annotated set and a production set have differed.** The first was
the seven of 9 September, where the annotated set carried the mapping in green note cells and
the production set carried no note cell at all, measured at zero in all seven, which is why
`KpiTemplates` holds the map as data and nothing reads a mapping out of a workbook. That the two
can also disagree about a ROW is new. Nothing in the tool reads either of those two ranges, so
no code changed for this and it is written into the rules file as the rule for the next one.

### 5. Street Design and Proposed are both Proposed on STREETS. CONFIRMED

Bader confirmed it with the client, 14 September. **Nothing changes in code**: that is what
`KpiTemplate.GroupsCountedAsProposed` has done since the sixtieth pass and what ST-05 measured,
369 existing, 2 proposed, 68 street design, 70 into Tree List - Proposed against the schedule's
own TOTAL of 439. It is recorded in `kpi-rules.md` as CONFIRMED BY BADER with the date, because
**a rule the client has confirmed reads differently from one the tool inferred**, and this one
had been sitting as a September decision nobody had checked since.

The two beside it are recorded as NOT confirmed in the same place, so the confirmation cannot
quietly cover them: a Street Design group is still left out and named on every template that is
not STREETS, which is Bader's own decision of 10 September, and a group named anything other
than Existing, Proposed or Street Design is out of scope on every template including STREETS.
**A confirmation covers what was asked and nothing sitting next to it.**

### The two break watches, and what each reddened

**Break 1, the area cell.** STREETS' area cell set to `H7`, the letter the other six use.
**4 red.** `StreetsNamesH8ForTheAreaAndItsOtherCellsAreUnmoved` failed with `Expected: "H8" /
Actual: "H7"`, which names the thing that was broken.

**Break 2, the note's type.** `RegionChoice` made to settle two regions holding an area by
taking the one the note names. **3 red**, among them
`OnStreetsTwoRegionsHoldingAnAreaAskTheSameWayAsOnMosques`. It reddened what it aimed at and it
ALSO showed the gap in section 3 above, which is the only reason that gap was found.

Both files were copied out before the break and `diff -q` against the copy after restoring them,
so the suite that produced 1732 is over the real code and not over a half restored file.

### The merge

Pull request 122, merged into main as `194a57f`, through the API with the commit title and the
commit message both passed on the call. **The runner ran 28 hook cases and 1732 tests against the
pull request head `776bdeb`, 0 failed and 0 skipped.** The merged tree is byte for byte the branch
head and the message off main carries neither a co-author credit line nor a generated-by footer,
which is the one thing the squash button on github.com would have added.

### Still open, unchanged by this round

The 44 audit findings. A row written into an empty one carries no family, no genus and no native
flag. A matched species whose height or diameter in Revit differs from the client's row. The
1548 scan's GOVERMENT BUILDING folder, spelt that way, and whether it is the ninth. Whether rows
edited in the pane should outlive the pane. And the note's own type, item 2 above, which is the
new one.

---

## 2026-09-14, seventy second pass. A plot could get a template and no folder, and six did

Two things off the 09:18 run, NG05, 156 plots over 7 templates, 150 workbooks. **1729 tests, 909
of them KPI, 28 hook cases**, against the 1706 main carries at the branch point `0b29cf7`. Build
zero warnings. **No audit finding is closed, renumbered or reordered: read off the three files at
these lines, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

### What worked, recorded because a round of faults hides it

The label lookup reached C5, E5, G5 and H5 on every template that ran, measured on the output:
ANH-007-ST-100130 carries its reference, the date, the person and their position. Character and
Context are filled. Three cells written over something the template already held are named.
**Four of the seven asset types ran for the first time**, and HEALTHCARE, MOSQUES, PARKING and
one STREETS workbook recalculate with ZERO errors.

### 1. A plot could get a template and no folder

EP-05, EP-11, EP-12, EP-13, EP-15 and FM-08 were ticked, READ, and dropped at the last step with
`the component folder table does not hold an empty component`. They are on no sheet, so no
component, so `PlotsPerTemplate` placed them by their PLOT PREFIX, which is what its own rule
says it must do and what its docstring names four of those very plots as the case for. Then the
folder table, keyed on the component, had nothing for them.

**Two routes to a template and one to a folder.** The two records shape, where the two records
are the two steps of one decision.

**THE PROPOSAL, SAID BEFORE IT WAS DONE, BECAUSE THE FOLDER NAME IS THE TEAM'S FILING AND NOT A
FACT ABOUT THE MODEL.** Three shapes were on the table and two of them add an eighth table, a
template to folder map or a prefix to folder map, each another record to keep in step with the
two that exist. The third refuses the plot, honest and costing six of a hundred and fifty six
every run.

**I took none of the three, because the relation is already written down twice over.** Both
tables are keyed on the same eleven component values, so which folders a template reaches is
read off the two together rather than held as a third thing. Counted off them by hand:

```
EXISTING PARKS   EXISTING PARK      HEALTHCARE   HEALTHCARE     STREETS   STREETS
FUTURE PARKS     FUTURE PARKS       PARKING      PARKING LOT
SCHOOLS          SCHOOL             MOSQUES      DAILY MOSQUE and FRIDAY MOSQUE
```

**Six templates reach exactly ONE folder and MOSQUES reaches two.** So a plot with no component
of its own is filed where every other plot of its template is filed, which places the five park
plots, and where a template reaches more than one nothing is derived, which still refuses FM-08
with a reason naming both folders rather than naming an empty component. Same shape as two
filled regions holding an area on one plot: one answer answers itself and two is a question the
data cannot settle.

**An absence and an answer nobody knows stay two different things.** A component the table does
not hold still places nothing and still names the value. Falling back to the template there
would file a value the team has never seen under a folder the team never chose, and that is the
one thing a folder rule must never do.

**AND THE PLOT IS NAMED BEFORE THE PRESS.** `CreateWords.PlotsWithNoComponent` counts the ticked
plots with no component, says how many file under their template's own folder, and names by name
the one that can be filed nowhere, with what to do about it. The count is a note and that plot
is a refusal, both above Create rather than twenty minutes later in a report. A run where every
ticked plot carries a component says nothing at all, because a line about nothing is one the
team reads past on every other press. No line names `PRX_COMPONENT`, which is in no model, and a
test refuses it.

### 2. Two divide by zero errors, and what I can and cannot say about them

ANH-007-SC-100004 and ANH-007-ST-100130 each recalculate with 2 #DIV/0!, neither on the main
sheet, both on plots with few trees.

**WHICH SHEET AND WHICH CELLS IS UNKNOWN FROM THIS REPOSITORY, AND I AM NOT GOING TO GUESS.** No
client workbook is in it and none ever will be, so the four cells cannot be found by reading
code. What I did establish is why the report was silent about them: **the formula check knew one
shape only**, a formula returning text off ISBLANK and the arithmetic on it. It could not see a
division at all, so it named nothing, and the answer to whether the tool wrote something those
formulas read was in no file.

So the tool is taught to say it. `WorkbookFormulas` reads a division whose divisor is ONE CELL
and asks what that cell holds: a nought or a blank is #DIV/0!, and the line names the cell, the
formula and **whether THIS RUN wrote the cell being divided by**, which is the half that would
make it the tool's doing rather than the client's arithmetic. The next run answers Bader's
question by itself, on the real templates, for all four.

Two things it refuses to judge, both on purpose. A divisor that is an expression, a range or a
function call, because working out what it computes to would be evaluating the formula. And **a
divisor that is itself a FORMULA**, because the patcher drops every cached value on the way out,
so a formula cell in the output holds no number at all and reading that absence as a nought
would call every computed divisor an error.

**IT IS REPORTED AND NEVER REFUSED ON.** A plot with no trees really has no average, so the
divide by zero is the client's own arithmetic over a real number, and deleting a correct
workbook over it is worse than printing a line. Turning that into a refusal where the run wrote
the divisor is a decision for Bader once a run has named them, and it is not taken here.
**No client formula is changed either way.** A test asserts the output stays on disk and the
patch is not refused, on both the run wrote it and the run did not write it cases, because a
check that started deleting 150 correct workbooks would be a far worse fault than the one it
reports.

### What is not a fault, recorded so nobody chases it

Both parks workbooks recalculate with 44 #N/A at F31 to F74. An untouched EXISTING PARKS
template recalculates with 45, and **the one that goes away is a divide by zero on the empty
area, which the run fixed by filling it.** The 44 are the PARK PROGRAMME section failing because
the Criteria sheet's programme table is empty in the client's own file, measured on 8 September.
The parks workbooks are correct and carry a fault the client shipped. It is in the rules beside
the 45 against 44 measurement that was already there.

### Break watch

One, restored byte for byte and checked with a diff against its backup.

**A template reaching two folders takes the first rather than refusing.** Three red, and
`AMosquePlotWithNoComponentIsStillRefusedAndBothFoldersAreNamed` fails on `Assert.False(path.Ok)`:
the mosque plot was filed under DAILY MOSQUE by a guess, which is the thing that must never
happen, and the red names it.

### Existing tests changed by hand

Fourteen call sites of `PlotWorkbookPath.For` gained the template argument, each given the one
its own component really means rather than a convenient one, and the GOVERMENT BUILDING case
passes null because no template placed that plot. Nothing else moved.

### The merge

Pull request 121, merged into main as `de49858`. **The runner ran 28 hook cases and 1729 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1729 ran at `de49858`, 909 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next run is what shows the five park
plots writing workbooks and what names the four #DIV/0! cells by sheet and by cell.

---

## 2026-09-14, seventy first pass. The date is on every template twice, and the guard was firing

Bader's answer to the question the round before left open, and one thing his answer turned up
that is a fault the round before shipped. **1706 tests, 886 of them KPI, 28 hook cases**, against
the 1700 main carries at the branch point `bfbab7f`. Build zero warnings.

### The UNKNOWN is closed: no label names the position cell

Measured on all seven. **The only cells whose text names a position hold `<Position>` itself**,
which is the placeholder this run replaces and not a label. Nothing names it on row 5 or
anywhere else.

So the distance of two from `Prepared By:` stays, and writing it as a distance and saying so out
loud was the right call rather than a shortcut. A test now says nothing in the table looks for
the word Position, and says what the placeholder in H5 decides, which is nothing.

### And the answer turned up a live fault, shipped last round

**`Date:` is on every template TWICE.** Measured on all seven:

```
row  5   D5  Date:   E5 the date   F5  Prepared By:   G5 a name   H5 a position
row 28   D28 Date:   E28 the date  F28 Reviewed By:   G28 a name  H28 a position
```

at row 28 on HEALTHCARE, MOSQUES, PARKING and SCHOOLS, and at **row 29** on EXISTING PARKS,
FUTURE PARKS and STREETS. Two blocks of the same shape, one word apart. A letter map would have
been wrong there too, which is the row 7 lesson a third time.

**THE GUARD WAS FIRING, ON ALL SEVEN, AND IT WAS NOT SCOPED TO ANYTHING.** The question was
whether it fires or never sees the second cell. Measured before anything was changed, by
building a sheet with both blocks and running the real lookup:

```
Date found=False  why=Date: is on <Mosques> at D5 and D28, and nothing says which is meant
Reference found=True  C5     PreparedBy found=True  G5     Position found=True  H5
```

**So the merged tool would have written NO DATE INTO ANY WORKBOOK**, and the report would have
named D5 and D28 under CELLS NOT WRITTEN on every template. The guard did exactly what it says.
**The TABLE was wrong**: it said the label alone identifies the cell, and on a real sheet it does
not. That is worth writing down as its own shape, because the guard reading correct is what made
the fault invisible to every test I wrote: **a guard firing on data nobody built is a guard
nobody has seen fire.** My fixtures carried row 5 alone, so the seven way theory that was meant
to be the check was run against a sheet the client does not have.

### The date is found through the preparer's block

`Prepared By:` against `Reviewed By:` is the only thing that separates the two blocks, which is
what Bader read off the measurement and it is the answer. `LabelledPlace.OnTheRowOf` names the
place whose label's ROW this one may look on: `Prepared By:` is looked for over the whole sheet,
and `Date:` is then looked for on that label's own row and nowhere else. Two passes in
`LabelledPlaces.Off`, the places naming their own label first and the anchored one after.

**The row is chosen by the LABEL and never by being first or by a number.** A sheet whose
reviewer block sits above the preparer's is answered with the preparer's row, and that is the
test that tells this rule apart from one that takes the topmost `Date:` or the constant 5.

Four things beside it, all tested.

**An anchor that cannot be found gives no row to look on.** A sheet naming no `Prepared By:`, or
naming it twice, writes no date either, and the reason says that rather than repeating the
anchor's words: `there is no row to look for Date: on, because Prepared By: was not found`, then
the anchor's own reason. **The date going with the person and the position is the anchor working
rather than a side effect**: a sheet where nothing can say which block is the preparer's cannot
say which row the date sits on either.

**The guard still fires inside the row it may look on**, and both its reasons name that row and
why it was that row, `no cell on <Mosques> row 5, the row Prepared By: sits on, reads Date:` and
the same shape for two of them. A row a person cannot check against the sheet by eye would be a
second unreadable rule.

**One level of anchoring, on purpose**, with a test that every anchor names a place that exists
and is not itself anchored.

**`REF :` is left looking over the whole sheet**, because the second block starts at column D
and carries no reference. If a template ever holds it twice the guard says so and writes
nothing, which is the right answer and not a silent one.

### The seven way theories now run against the real sheet

Both of them build each template's row 5 AND its own reviewer block, at 28 or 29 as measured, so
the check that the lookup lands on C5, E5, G5 and H5 is made against a sheet shaped like the one
the team fills. Each also asserts that **nothing at all lands in the reviewer's block**, because
a date landing at E28 would be a wrong number in a client file that nobody reading the preparer's
row would ever see.

### Break watches

Two, both restored byte for byte and checked with a diff against its backup.

- **The date looked for over the whole sheet again**, which is the merged fault. 21 red, and the
  message on `TheDateIsFoundOnThePreparersRowAndNeverTheReviewers` is the fault word for word:
  `Date: is on <Mosques> at D5 and D28, and nothing says which is meant`
- **The anchored row taken as the constant 5** rather than the row the anchor sits on. **Exactly
  one red**, `WhenTheReviewerBlockComesFirstTheDateStillFollowsThePreparer`, which is the one
  test whose whole job is telling those two apart. Every template measured so far has its
  preparer on row 5, so a constant passes all seven and that single case is what stops it

### Two existing tests changed by hand, each because the truth under it moved

`APreparedByLabelOnTheSheetTwiceRefusesThePersonAndThePosition` is now
`...ThePersonThePositionAndTheDate` and asserts the no row reason. And the skip list in
`ATemplateNamingNeitherLabelWritesNothingAndSaysSo` no longer holds the date among the `no cell
on` reasons, because the date's own reason is the anchor's, which the test now asserts
separately rather than letting it fall out of a filter.

### The count

**Read off the three audit files at these lines: 63 numbered findings, 19 carrying a FIXED mark,
44 open.** This round closes none, renumbers none and reorders none. It is a fault off a
measurement rather than an audit entry, so it is written here.

### The merge

Pull request 120, merged into main as `8abcf4a`. **The runner ran 28 hook cases and 1706 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1706 ran at `8abcf4a`, 886 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next run is what shows the date
reaching E5 on a real template, and a run says so plainly either way: a date the lookup cannot
place is named with the row it looked on and why it was that row.

---

## 2026-09-14, seventieth pass. Row 5 goes to the label lookup, and what its cells already hold

Findings 50 and 53 off the third audit, and two things Bader's own measurement turned up that
the audit does not name. **1700 tests, 880 of them KPI, 28 hook cases**, against the 1673 main
carries at the branch point `86b322f`. Build zero warnings.

### The measurement, which is the whole round

Bader opened all seven templates and read row 5, on 14 September:

```
HEALTHCARE, MOSQUES, PARKING, SCHOOLS, STREETS
  B5 REF :   C5 the UID   D5 Date:   E5 the date
  F5 Prepared By:         G5 a name  H5 a position

EXISTING PARKS and FUTURE PARKS
  B5 REF :   C5 EMPTY     D5 Date:   E5 EMPTY
  F5 Prepared By:         G5 EMPTY   H5 holds the text " Architect Engineer"
```

**NO FORMULA SITS AT C5, E5, G5 OR H5 ON ANY OF THE SEVEN.** So the letter map never overwrote
anything, no workbook is damaged, and finding 50 did not become a BLOCKS. What it did become is
this: the letters were right on all seven BY LUCK, the way D7 would have been right on two
templates out of three, and **FUTURE PARKS, one of the three nobody had ever looked at, is the
one that differs.** What differs is not where the cells are. It is what they HOLD.

### The four cells are found by their labels now

`LabelledPlaces` is one table of six places, each a name, a label and how many columns right of
the label its cell sits: the reference off `REF :`, the date off `Date:`, the person off
`Prepared By:`, the position off the same label two along, and Character and Context off their
own words. `LabelledPlaces.In` opens the template's main sheet once at the press and finds all
six in one pass, which is the read that already existed for the last two.

**`KpiTemplates.TypedByTheTeam` is deleted and `KpiValue.Reference` is out of every template's
map.** A test refuses any map that names E5, G5 or H5 again.

**CHECK YOUR WORK, which the round asked for.** `RowFiveTests.TheLabelsLandOnC5E5G5AndH5OnAllSeven`
builds each template's own main sheet carrying the row 5 its set was measured to hold, runs the
real lookup and asserts the label cell and the value cell of all four, on all seven. Green on
all seven. A second theory asserts the four values reach those cells through the plan.

### NO LABEL NAMES THE POSITION CELL, AND THAT IS THIS ROUND'S OWN FINDING

Three labels sit on row 5 and they reach three cells: B5 to C5, D5 to E5, F5 to G5. **H5 has no
label of its own.** It is one further along than the person's name, under the same Prepared By,
and both park templates confirm what it is by already holding a position there.

So it is written as a DISTANCE of two from that label rather than as a label, and the distance
is said out loud in `LabelledPlace.StepsRight`, in the class docstring, in the rules file and
here, because a distance dressed up as a label would be the one thing in this lookup nobody
could see. **Whether the real sheets name the position cell somewhere off row 5 is UNKNOWN**,
because the measurement covers row 5 and nothing else. That is a question for Bader: if a label
does name it, the distance should go.

One thing follows from two places sharing a label: a sheet carrying `Prepared By:` twice refuses
the person AND the position together, because the thing that cannot be resolved is the label
they share. It has a test.

### A cell it writes that was not empty, and what it held

`KpiCreatePlan.Labelled` was set and read NOWHERE. It existed to carry what each labelled cell
already held and no report line ever printed it, so the mosque template's Urban Area Zone at D7
and both park templates' " Architect Engineer" at H5 were written over in silence.

**CELLS WRITTEN OVER SOMETHING THE TEMPLATE ALREADY HELD** is that line, per value, with the
label, the label cell, the cell written and what it held. Only cells this run really wrote are
in it, so a template naming no label stays under CELLS NOT WRITTEN with its own reason, and the
column header prints only when there is a row under it, because a header over an empty table is
a shape the third audit counted six of.

**Nothing stopped writing.** Overwriting is right and it is now visible.

### The filled check, measured rather than reasoned

The round asked what the filled check does TODAY with an empty E5, because E5 is empty on both
park templates, and whether a clean park template is offered or withheld.

**It is OFFERED. There was no live fault on the two park templates.** `FilledMark.Reads` is
false for an empty string, for whitespace and for a cell that is not in the file at all, so
`Decide` hands back nothing and the workbook is a template. The tests go through the real path,
`PeekedWorkbook.Of` on an .xlsx the test builds and then `RecognisedWorkbook.Recognise`, exactly
as `KpiPanel` calls them: both park templates with the cells empty, the same with those cells
absent from the file altogether, and MOSQUES with the R1 set's placeholders. A filled park
workbook beside them is still withheld, so the four green cases are the rule answering rather
than the check being dead.

### One place still reads row 5 by letter, on purpose

`FilledMarks` runs over every workbook in the templates folder before any template is
recognised, so it has no labels to ask and reads `E5` and `C5` as the measurement says they are
on all seven. That is two records of one fact, which is the shape this repo keeps paying for, so
`RowFiveTests.TheCellsTheLabelsChooseAreTheCellsTheFilledCheckReads` holds them against each
other over the measured row 5 and goes red when either moves.

**OPEN, FOR BADER: whether the peek should find its cells by label too.** It would delete the
last two letters. It means the peek reads the whole first sheet rather than two cells, on every
workbook in the folder, and it changes `RecognisedWorkbook.Recognise`, the pane's call and about
two hundred lines of test. It is a round of its own and it is not this one.

### Finding 53, the limit that had gone away

`kpi-rules.md` still said the reference cell is skipped when the ticked plots disagree on it,
which was true when a checklist covered several plots. **A checklist has been one plot since
round 113**, so the plots cannot disagree, the reference always carries that plot's own
reference, and both marks have been live for six rounds with nobody recording it. The rules file
says so now and so does the `FilledMarks` docstring that carried the same sentence.

### The pane names no cell for any of the four

It cannot. Which cell each lands in is not known until the template is opened at the press. The
line under the map says the reference comes from the chosen parameter and goes where the `REF :`
label sends it, and the line under that says the same of the three the team types. **A line
about what the tool does is checked against what it does**, and this is the third time that rule
has bitten.

### Break watches

Three, each restored byte for byte and checked with a diff against its backup.

- **The position steps one column instead of two.** 16 red: both row 5 theories on all seven
  templates, the distance test and the park template's H5 read. The red names H5 and the
  position, which is what was broken
- **`FilledMarks.DateCell` reads E6.** 14 red, and
  `TheCellsTheLabelsChooseAreTheCellsTheFilledCheckReads` is among them, which is the guard that
  exists for exactly that parting
- **A cell written over is never reported.** 2 red, both naming the written over section

### Existing tests changed by hand, each because the truth under it moved

`CreateFixture.Run` now hands the plan the measured row 5 rather than a template nothing opened,
because that is what a real press does. Eight tests moved with it: four plan tests, two skip
lists that now name all six labelled places rather than two, the map completeness theory that
no longer asserts a reference cell, and the pane block whose line no longer names three letters.
`LabelFixture` now escapes cell VALUES as well as the sheet name, because a cell holding the R1
set's own `<Date>` wrote a start tag into the sheet part and produced a file no XML reader opens.

### The audit files

Findings 50 and 53 carry a FIXED mark with the pass that closed them. **No other finding was
touched, renumbered or reordered.** Counted off the three files at these lines: **63 numbered
findings, 19 FIXED, 44 OPEN.** The round message said 30 open, which is 32 minus 2 and leaves
out the twelve of audit 3 that still stand. Nothing here was renumbered to make the message
right.

### The merge

Pull request 119, merged into main as `afb85a9`. **The runner ran 28 hook cases and 1700 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1700 ran at `afb85a9`, 880 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

### A message file left behind by the round before

**The record commit on main was first taken carrying the sixty ninth pass's message.** The
scratchpad held a `record.txt` from that round, the call that would have overwritten it was
refused whole by `require-file-on-commit.sh` because the staging and the commit were in one
Bash command, and the next call named `record.txt` on the message flag. Git found a real file,
read a real message and committed it. **Nothing failed**, which is the whole shape of it.

`CLAUDE.md` already says to write a commit message to a file in one call and name that file on
the next. It does not say the file has to be one THIS round wrote, and a stale file of the same
name is indistinguishable from a fresh one to every hook and to git. The commit was amended with
a message written to a new name. **A file that is there is not the same as a file you put
there**, which is the same shape as the tool's own rule that a copy taken once and never
refreshed reads exactly like a fact.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every change since 14 September is still
unrun, which is rounds 113, 114, both hook rounds, 117 and this one. The one thing only a run
can answer is whether the real templates spell `REF :`, `Date:` and `Prepared By:` the way the
measurement says, and a run says so plainly: a label the sheet does not name writes nothing and
names the sheet it looked on.

---

## 2026-09-14, sixty ninth pass. The third audit of the KPI tool

**It builds nothing and fixes nothing.** The only files written are `steps/audit-kpi-3.md`, this
log and the state block. **The state block is written because `require-file-on-commit.sh` refuses
a commit without one**, not because the round wanted it, and that is worth recording as the one
place the round message and the hook disagree.

**1673 tests, 853 of them KPI, 28 hook cases**, green before and after. Nothing under `src` or
`tests` moved.

### Part A, the 32 open findings

Checked one at a time at today's lines. **Six passed by, two moved in half, and twenty four
still stand**, which is 32. The six are 3, 20, 21, 22, 26 and 43, all from rounds 113 and 114
taking out the name box and the grouping buttons. The two halves are 13 and 40.
Findings 29 and 41 stand and are WORSE than written: `KpiCreateReport.cs` grew to 1277 lines and
`TemplateWords.cs` now stacks two docstrings BOTH describing the deleted name box.

### The hunt the brief called most valuable

**A fact measured once and generalised gave three findings, 50, 51 and 52.** The first is the
strongest thing in the file.

**`TypedByTheTeam = { "E5", "G5", "H5" }` is one array for all seven templates and the letters
were measured on one.** That is the Character and Context fault three cells up. Character sat at
D7 on two templates and F7 on the third, because STREETS carries a Category formula at D7, and
that is why the tool now finds those two by their LABELS. **Row 5 is still written by letter**,
and HEALTHCARE, PARKING and FUTURE PARKS have never been looked at. The divergence is proven to
exist in row 7 of the same sheets.

The other two are the group row. `IsStructureRow` needs cell 0 filled and every other cell empty,
and the group name is then read off cell 0. **The first cell of a species row is the IMAGE**,
which is the measured fact that broke four readers already. A group row printing with anything in
the image column is not recognised at all, its species attach to the group above, and they go to
that group's sheet. Nothing refuses it, because the species still add to the printed TOTAL.

### Three break watches, and the honest result

**All three reddened and every red case named what was broken.** The component folder spelling
gave 2 red, the species alias gave 5, the square foot constant gave 4. All restored byte for
byte, checked by diff.

**No test was found that would pass with its own behaviour broken.** That is a change from the
last two audits, where three such tests were proved, and the fair reading is that round 117's
four test findings went to the places that were weak.

### The report, measured rather than remembered

A run was generated through the test fixture and counted. **One plot, no species, no schedule
read is 152 lines, and 68 of them are nine sections whose count is zero.** Six of those nine
print a column header for a table with no rows. Seventeen headings each carry a sentence
explaining the rule rather than naming the contents, and not one of them says anything the
heading does not already carry.

### Counts

**1673 tests, 0 failed and 0 skipped, 853 of them KPI, none added and none changed.** 28 hook
cases. Build zero warnings, measured after the last file was written.

Pull request 118, merged into main as `734695b`. **The runner ran 28 hook cases and 1673 tests against the pull request head, 0 failed and 0 skipped, BOTH UNCHANGED, because nothing under `src` or `tests` moved. Locally the same 28 and 1673 ran at `734695b`, 853 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

### Still open

Fourteen new findings, numbered 50 to 63, nine dropped for having no cost. **No BLOCKS**: nothing
found can be shown to stop the tool or put a wrong number in a client workbook on a path that has
been run. **Findings 50 and 51 become BLOCKS the moment their unmeasured case turns out real**,
and four of the five open UNKNOWNs need nothing but a file nobody has opened.

---

## 2026-09-14, sixty eighth pass. The four tests that stayed green while the code was broken

Findings 10, 11, 33 and 48, three of them proved by the audits. The branch came off a fresh pull
of main at `4315933`, so the baseline is **1640 tests, 820 of them KPI**, and this round adds
**33, to 1673**. The hook checks are 28 and unchanged. **The audit files now read 49 findings,
17 FIXED, 32 OPEN**, counted off them, moved by these four and nothing else. **Nothing in this
round has been observed in Revit**, and nothing in it changes what the tool does except
finding 48, which is the one item that was meant to.

### All four still stood at today's lines

Checked before anything was written. Finding 10's `KpiCreateTests` still asserted the cells in
order and the sheet on each and no value against any cell. Finding 11's
`EveryWrittenCellIsReadBackOffTheOutput` still checked `WorkbookPatcher.ReadBack` rather than
what `Patch` put on the outcome. Finding 33's `PatchOutcome.Done` was still built in no test
file but the patcher's own. Finding 48's rule still existed twice.

### 10. One assertion per value against its cell

`ValueInItsOwnCellTests` binds every value to the cell it belongs in. The three typed into E5,
G5 and H5, the component, the reference and the location, the area, the shrubs and the lawn
totals, the two street cells and the two fixed ones, plus two cases holding a whole plan at
once so a pair cannot hide behind a case reading one of them. **Every value in it is a
different number or a different word**, because two totals of a size are exactly what let a
swap through.

One case earns its place beyond the finding: nothing the tool writes may land on D7 on STREETS,
which holds the Category formula, and the only thing keeping it off is that the fixed values
are placed off their labels.

### 11. The read back, and the first attempt at it that did not work

**My first version of this test did not catch the break, and that is the more useful half of
it.** It patched, corrupted a cell in the file behind the tool's back, and asserted the
corruption was reported. The audit's break went green under it at 1666, because **a corrupted
cell is not one the run wrote**, so it was read through `ReadBack` again, which is the very trap
the finding names.

What catches it is a cell written TWICE in one patch. The patcher applies writes in order, so
the file holds the second value, while the outcome carries one landed cell per write. Reading
the file gives the second for both. Reporting what was sent gives the first for the first
entry. That is a real divergence rather than a contrived one: two writes for one cell is exactly
the plan bug a read back exists to make visible.

The corruption case is kept, with a comment saying plainly that it does not catch the break and
why, so the next person does not write it again.

### 33. A run that actually wrote

`CreateFixture.RunThatWrote` patches a workbook for real and hands back a run carrying a genuine
`PatchOutcome.Done`, rather than an outcome built by hand, so what the report prints is what a
run would put there. `RunThatWroteTests` asserts one line per landed cell, the heading's own
words, the part counts, and the status line's N cells written from M plots, which was asserted
nowhere at all.

### 48. The rule moved into Core, and the two copies were NOT the same rule

**This is the answer to the question the round asked, and it is not the reassuring one.**

```
the handler   a hand pick wins, else the ONE region HOLDING AN AREA, else nothing
the fixture   a hand pick wins, else the FIRST region, whatever it held, however many
```

They differ on two shapes. Two regions holding an area: the handler chooses nothing and lets the
reconciliation refuse until a person picks, the fixture took the first. A single region holding
NO area: the handler chooses nothing, the fixture took it anyway. **The handler's is right**, and
it is what `RegionChoice.For` in Core carries. The handler keeps only the reading.

**No test ever told them apart.** Every multi region case in the suite passes an explicit empty
choice, and both copies answer that alike, the old one because an empty string is not null and
the new one because an empty string is nobody having picked. So the suite was green before the
move and green after it, and the divergence sat in the two shapes nothing exercised.
`RegionChoiceTests` pins those shapes, and three of its cases are ones the fixture's copy
answered wrongly.

`RegionChoice.WhyUnchosen` is new beside it, naming which of the three cases it was, because an
empty choice with no reason leaves somebody guessing between no regions, none holding an area,
and two that do.

### Four break watches, all restored byte for byte

Every one checked with a diff against its backup, and every one checked for whether the red
case is the one that names what was broken.

1. **The shrubs and lawn totals swapped in `KpiCreatePlan.Of`. 4 red**, all mine, all naming the
   pair: the two that name F11 and H11 and the two whole plan cases.
2. **The `CellText` read replaced with `write.Stored`. 2 red**, both in `ReadBackNoticesTests`
   and both naming the read back. **On the first version of that test it was 0 red**, which is
   recorded above and is the reason the test is shaped the way it is.
3. **CELLS WRITTEN printed off the plan. 3 red.** Two are mine and name the landed cells. **The
   third is not mine and reddened for a related but different reason**:
   `WorkbookFormulasTests.TheFormulaCountAndTheReadersOfWrittenRowsAreInTheReport` asserts the
   heading reads CELLS WRITTEN (0) on a refused run, and the plan's writes make that count non
   zero. Checked rather than assumed, and it covered the count on a refused run, never the
   landed values on a run that wrote.
4. **`RegionChoice.For` taking the first region. 6 red**, three of the direct cases and three
   fixture based ones, in `HeldReadingsTests`, `ReconciliationTests` and `StreetsAreaTests`,
   which is both halves the round asked to see.

**Twice in two rounds a break has reddened something other than what it aimed at.** Last round it
was a second guard catching what the first stopped catching. This round it was a green case that
proved nothing, in a test I had just written. A green case does not prove the line you think it
does, and the only way to find out is to break the line and look at the names.

### Counts and the merge

**1673 tests locally at this branch, 0 failed and 0 skipped, 33 added against the 1640 main
carries** at the branch point `4315933`. Hook checks 28 of 28. Build zero warnings, measured
after the last file was written.

Pull request 117, merged into main as `5ff81fd`. **The runner ran 28 hook cases and 1673 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1673 ran at `5ff81fd`, 853 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

### Still open, for Bader

- **Non-permeable hardscape** is still blank on every workbook and nobody has said where it
  comes from. Unchanged.
- **`--fixup` and `--squash`** build a message from a ref's subject, are not read by the commit
  hooks and fall to the no message refusal. Unchanged from last round.
- **32 audit findings stay open**, not renumbered and not reordered.

---

## 2026-09-14, sixty seventh pass. Four things off the hook round before it

**A shared change, so it runs alone.** Nothing under `src` or `tests`, so the dotnet suite is
unchanged at **1640, 820 of them KPI**. The branch came off a fresh pull of main at `70c4abe`.
**The hook checks went from 15 cases to 28.** Nothing here has been observed in Revit and
nothing here touches the add-in.

### The notebook path walked past the guard

`block-paths.sh` is matched on Write, Edit AND NotebookEdit, and read `file_path` only.
**NotebookEdit's argument is `notebook_path`**, so every notebook write arrived as an empty
string and took the `exit 0` written for a call with no path at all. Measured last round and
again before the change: `/etc/evil.ipynb` came back exit 0 where the same path under
`file_path` came back exit 2.

It reads both keys now. **And a call carrying neither is refused rather than waved through**,
which is the second half and the more important one: the three tools this hook is wired to all
carry one of the two keys, so anything reaching that branch is something the guard cannot see.
An empty string used to mean nothing to check, and it meant the check was blind.

### The task list was written down twice and the two came apart

`territory-check.sh` named six tasks and `.claude/rules/territory.md` numbered five, missing
View Filters, in the one document that decides which session may touch what.

**Which is right was measured rather than assumed.** ViewFilters has code in all three roots,
`src/RcrcGreen.Core/ViewFilters`, `src/RcrcGreen.Revit/ViewFilters` and
`tests/RcrcGreen.Core.Tests/ViewFilters`, plus its own rules file, its own log and its own
state file. Three of the five the document does list have no folder at all yet. **The hook was
right and the document was behind**, and a task the wall does not know about is a task any
session can edit without being stopped.

So the list is `.claude/hooks/tasks.txt` now, one line per task, the folder name and the
team's name for it. **The hook READS it** rather than carrying a copy, and a list it cannot
read refuses the commit, because an empty list makes every path common and the wall stops
refusing anything. `territory.md` points at it, gains its View Filters entry and renumbers, and
**a hook case refuses when the two disagree**, comparing the file's own
`src/RcrcGreen.Core/<name>` mentions against the record. The instruction for a new task now
says both places, and the check is what makes that true rather than hoped for.

### An amend and a reuse are READ, not refused

Both have a message git can be asked for. `--amend --no-edit` takes HEAD's and `-C <ref>` takes
that ref's, so `message_of` asks `git log -1 --format=%B` for the one the command will really
use and the hook checks that. `-c`, `--reuse-message` and `--reedit-message` go the same way.
**Refusing every amend to catch the rare bad one would block a flow people use every day**,
which is the wrong trade and is why this was left open rather than closed the easy way.

Where the message still cannot be got at, it refuses, the way an unreadable file already does,
and each refusal says which case it is:

```
--amend --no-edit          reads HEAD's message
-C <ref>                   reads that ref's message
--amend, no --no-edit      refused, the final message is not decided yet
no message flag at all     refused, it would be typed into an editor
-C <a ref git cannot read> refused, and the ref is named
```

The last two are a behaviour change on commands that used to pass unchecked. A bare commit and
a bare amend both open an editor, which cannot complete in this environment anyway, and the
refusal names the remedy.

### hook-tests.sh runs in the gate

It was a test nobody ran, which is its own kind of silence and the whole reason the round
before this one existed. It is a step of its own in `tests.yml`, **before the build**, because
it takes about a second and a broken guard is worth knowing about before anything else starts.
**It reads its own count back**, the same way the dotnet step does, so a run that checks
nothing fails rather than reporting green. Checked by feeding that guard a run of zero cases.

**The amend cases needed a HEAD whose message is known**, which this repo cannot provide, so
the file builds a scratch repo in the temp folder: a copy of the hooks, the one file the
writing check reads its word list from, a state file for the require-file hook, and commits
that can say anything because none of it reaches this repo. The commit carrying a credit line
is made with `commit-tree`, so it belongs to no branch. Checked against an empty `HOME` as
well, since the runner has no global git config.

### Six break watches, all restored byte for byte

1. `block-paths.sh` reading `file_path` only. **1 red**, and NOT the one expected: the notebook
   path outside the repo stayed refused, because the refuse-on-empty half caught it instead.
   **The two halves back each other up**, which is worth knowing and is why the next one exists.
2. Both halves reverted, which is exactly the code that was on main. **2 red**, the notebook
   path outside the repo and the call with neither key. That is the hole as it really was.
3. The hook hardcoding the task list instead of reading it. **1 red**, the case that hides the
   record and expects a refusal.
4. `territory.md` dropping View Filters again. **2 red**, both halves of the comparison, the
   missing name and the name the record does not hold.
5. An amend handing back an empty message, the old shape. **1 red**, an amend with a credit
   line is refused.
6. `-C` no longer capturing its ref. **1 red**, and again not the one expected: the credit line
   case still refused, because an uncaptured ref falls through to no message at all and is
   refused for that reason instead. The clean reuse case is the one that reddened.

**Twice now a break reddened a different case than the one aimed at**, both times because a
second guard caught what the first stopped catching. That is defence working, and it is also a
reminder that a green case does not prove the line you think it does.

### Counts and the merge

**1640 dotnet tests locally at this branch, 0 failed and 0 skipped, 820 of them KPI, none
added and none changed**, because nothing under `src` or `tests` moved. Hook checks 28 of 28.
Build zero warnings.

Pull request 116, merged into main as `5cf8017`. **The runner ran 28 hook cases and 1640 tests against the pull request head, 0 failed and 0 skipped, the hook step being the one this round added. Locally the same 28 and 1640 ran at `5cf8017`, 820 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.

### Still open

- **`--fixup=<ref>` and `--squash=<ref>`** build a message from that ref's subject. They are
  not read here and fall to the no-message refusal. Nothing in this repo uses them, so it is
  written down rather than guessed at.
- **Non-permeable hardscape** is still blank on every workbook, unchanged and still nobody's
  answer.

---

## 2026-09-14, sixty sixth pass. The commit hooks could be bypassed in silence

**A shared change, so it runs alone.** Nothing else is in this round: no KPI code, no rules
file, no test project. The branch came off a fresh pull of main at `f7fca6d`. **1640 dotnet
tests, 820 of them KPI, unchanged, because nothing under `src` or `tests` moved.**

### What was open

`commit-scope.py` read the commit message out of the command, and for a `-F` path it opened the
file, except that a path of `-` was skipped with a bare continue. The message then came back as
zero bytes, which breaks no writing rule, so `writing-check.sh` passed it. Found by walking into
it on 14 September: the sixty fifth pass made its first commit with `-F -` and a heredoc, carried
a co-author credit line, and was taken without a word.

**One correction to what that round's log said.** It said all three commit hooks scanned an empty
string. That is wrong and is corrected here rather than left standing. **Only
`writing-check.sh` asks for the message.** `require-file-on-commit.sh` asks for `paths` and
`territory-check.sh` for `all-paths`, and neither reads the message at all, so the hole blinded
one hook rather than three. Checked by grep over the three scripts.

### The fix, in two halves

**`commit-scope.py` reports the standard input case rather than skipping it**, and
**`writing-check.sh` refuses on it and says what to do.** Not the wording the unreadable-file
case already used: a file that cannot be opened and a message that was never in a file are
different problems, and only one of them has a remedy a person can act on. The refusal reads
that the message is on standard input, where a hook that runs before the command cannot read
it, and to write the message to a file and name that file with the message flag and its path.

### The first fix was wrong, and its own commit is what said so

The reason was reported by printing a MARKER WORD into the message, which is what the older
unreadable-file case had always done, and the hook searched the message for that word. **The
commit carrying that fix was refused by its own message**, because the message described the
hole and named the marker. Every word of it was accurate and none of it was a failure.

**A signal that travels in the data is not a signal.** A message that has the problem and a
message that merely talks about it are the same string to a substring search, and the older
marker had the same fault for as long as it has existed. Refusing is the safe direction, so
nothing was ever let through by it, but a check that refuses correct work is a check people
route around.

So the reasons come back BESIDE the text rather than inside it. `commit_message` hands back the
text and a list of what could not be read, `main` exits non-zero with the reasons on standard
error, and `writing-check.sh` reads them off standard error and prints them. **Both markers are
deleted**, the older one included, and with them the whole class. `scope` in the hook returns
the answer and the reason together for the same reason.

A case pins it: a message that talks about a message on standard input and about a message file
that could not be read, while being neither, passes. That case fails against the marker version,
which is how it was chosen.

### The second instance, in the same file

`read_commit_call` catches the unbalanced quote case and its own comment says to say nothing was
found and let the caller fail closed. **No caller ever read the flag.** `main` went straight on
and answered off the index, so a command that cannot run as written was answered with a guess at
what it would have committed, to all three hooks. Measured before the change: a command holding
an unbalanced quote came back `index` with exit 0.

The fix is that the script itself exits non-zero, because **all three hooks already refuse on
that** and had done all along. Nothing in any hook changed for it. A `readable` flag carries it,
separate from `found`, because a line holding no commit at all is ordinary and must still pass.

### Tested three ways, and the last two refuse

`.claude/hooks/hook-tests.sh` is new. It drives the real hooks with the JSON payload Claude Code
sends and checks the exit code, 0 for a pass and 2 for a refusal. **15 cases, all green.**

```
a message file it can read passes            exit 0
a credit line is refused                     exit 2
an em dash is refused                        exit 2
a banned word is refused                     exit 2
a message file it cannot read is refused     exit 2
a message on standard input is refused       exit 2
an inline message it can read passes         exit 0
a message ABOUT a failure passes             exit 0
```

The three the round asks for are the first, the fifth and the sixth. Beyond them it checks the
case above, that the scope script refuses a command it cannot read, that all three hooks refuse
that same command, and that the ordinary shapes are still read rather than refused: the message
flag, a message file with pathspecs, and the all form. **Every forbidden string in that file is
built from pieces**, because the file is scanned by the hook it tests and a literal one would
refuse the commit carrying it. The hook itself already did this and the comment saying why is
what pointed the way.

**It runs nothing in the gate**, which runs only the dotnet tests, so it is run by hand. Wiring
it in is a question for Bader rather than something done here.

### Three break watches, all restored byte for byte

1. The dash case put back to a bare continue. **1 red**, a message on standard input is refused.
2. The `readable` flag never set. **4 red**: the scope script directly, and all three hooks.
3. `main` ignoring what `commit_message` could not read. **2 red**, standard input and the
   unreadable file, which is what shows one exit path now carries both.

**The second break found a gap in my own test before it found anything else.** Run against the
first version of the fix it reddened 2 of 3, because `require-file-on-commit.sh` refused for its
own reason: the index was empty, so no state file was in the commit either way. A case that
passes incidentally proves nothing, so a direct case on the scope script went in ahead of the
three hook cases. Same shape as last round, where a break watch went green and exposed a missing
case rather than working code.

### Counts and the merge

**1640 dotnet tests locally at this branch, 0 failed and 0 skipped, 820 of them KPI, none added
and none changed**, because nothing under `src` or `tests` moved. `hook-tests.sh` 15 of 15. Build
zero warnings.

Pull request 115, merged into main as `7f10019`. **The runner executed 1640 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `7f10019`, 820 of them
KPI, and the hook tests are 15 of 15 there.** The merge went through the API with the title and
the message both passed on the call, the commit came back off main carrying neither a co-author
credit line nor a generated-by footer, and **the merged tree is byte for byte the branch head**,
checked by diffing `1836630` against `7f10019`.

### The other hooks, checked for the same shape

The shape is a guard that cannot see its subject and passes rather than refusing. Every exit
path of the other three was read, and two of them probed.

**`require-file-on-commit.sh`: nothing found.** Its four refusal paths all fail closed. A missing
scope script refuses, a repo with no state file at all refuses, a non-zero from the scope script
refuses, and anything that falls through refuses. The pipe into `tr` would have masked the scope
script's exit code, and does not, because the script sets `pipefail`. Checked by driving it.

**`territory-check.sh`: nothing found in its own logic.** A missing scope script refuses, a
non-zero refuses, and a fault inside its own embedded python refuses, which was probed by making
that python throw and watching it come back exit 2 with the traceback. What it cannot see it is
already documented as not seeing, in `territory.md`: a folder not in the TASKS list reads as
common, a new Drawing Sheet file at the Revit root has to be added to a list by hand, and Drawing
Sheet's flat test files read as common. Those are known limits rather than new findings.

**`writing-check.sh`: one thing left as it is, deliberately.** A file whose content holds a NUL
byte is skipped with a bare continue. That is the same silence the `-` case had, but a binary
file has no text lines to check and refusing every commit carrying an image would be wrong. It is
named here so the next reader knows it was looked at rather than missed.

**`block-paths.sh`: one real hole, NOT fixed this round.** It is wired to Write, Edit AND
NotebookEdit, and it reads `file_path` only. **NotebookEdit's argument is `notebook_path`**, so
the hook reads an empty string, hits `if [ -z "$TARGET" ]; then exit 0` and passes. Measured:
a payload writing to `/etc/evil.ipynb` came back exit 0, where the same path under `file_path`
came back exit 2. This repository holds no `.ipynb` file, checked, so it is open rather than
being exploited. **It is left alone because this round is the hook fix and nothing else.** The
change is one line, reading both keys, and it is Bader's call.

### Still open, for Bader

- **`block-paths.sh` does not see a NotebookEdit path.** Above, with the measurement.
- **Should `hook-tests.sh` run in the gate?** It is a test nobody runs automatically, which is
  its own kind of silence. The gate is `dotnet test` only today.
- **`territory.md` is behind the hook again.** The hook names six tasks and the file numbers
  five: ViewFilters is missing from the list. The file's own header says it fell behind once
  already and to check the two against each other, which is how this was found. Not corrected
  here, because nothing else is in this round.
- **A message the hook still cannot see.** `--amend --no-edit` and `-C <ref>` reuse a message
  from an existing commit, and the scope script returns empty for both, so they pass unchecked.
  That is the same shape again, but refusing every amend would block a normal flow, so it is a
  decision rather than a bug to fix quietly.
- **Non-permeable hardscape** is still blank on every workbook, unchanged from last round.

### For the Drawing Sheet session

**This landed in `CLAUDE.md`**, under the things that have gone wrong, because that is the file
both sessions read every round and neither owns. Two entries: the standard input hole with the
remedy, and the flag that was set and never read. The Hooks section names `hook-tests.sh` and
says it is run by hand. **Write a commit message to a file and name that file with `-F <path>`.**
A heredoc on standard input is refused now, so a round that uses one will be told rather than
passed.

---

## 2026-09-13, sixty fifth pass. Four things off the first per plot run

The tree is right and both files recalculate with zero errors. The street reference file works:
ANH-007-ST-100217 came out ROW 36, length 928.782391, and the workbook computed the area at
33,436.17 itself. So this round is the four things the run showed, and every one of them is
wording or a count rather than anything that reads a model. The branch came off a fresh pull of
main at `70fdb82`, so the baseline is **1625 tests, 805 of them KPI**, measured at that commit
before anything was written. **The audit findings stay open, not renumbered and not reordered,
and this round closes none of them.** **Nothing in this round has been observed in Revit.**

### Character and Context are found by their labels, never by a letter

Bader measured them off the three workbooks written on 13 September:

```
MOSQUES   Character label C7, value D7.   Context label E7, value F7.
SCHOOLS   Character label C7, value D7.   Context label E7, value F7.
STREETS   Category  label C7, value D7.   Character label E7, value F7.
                                          Context   label G7, value H7.
```

**A map holding a letter would have overwritten a formula.** Two of the three templates put
Character at D7 and the third puts a Category formula there, so Character at D7 because two
templates say so destroys the street sheet's own calculation on the third. Nothing in
`FixedCells` holds a letter now. The template is opened when Create is pressed, its main sheet
is read, the label is looked for on it, and the cell to the RIGHT of the label is what gets
written. The street's Category is never touched because nothing looks for the word Category.

A label is matched whole, without case and with edge whitespace off, so Characteristics is not
Character. A template naming neither label writes nothing and says which label was missing. One
naming a label TWICE also writes nothing, and says both cells, because nothing says which of the
two is meant.

`LabelledCell` carries what the value cell ALREADY HOLDS, which is not decoration. The mosque
template came filled, holding Urban Area Zone at D7 and Urban at F7 before this tool touched it,
and the street file has both blank. Without that the report would read as though this run had
put the mosque's values there.

`KpiCreatePlan.Of` takes the labels as a thirteenth REQUIRED argument and throws on null. A
default would have meant a caller that forgot them wrote nothing and said the template was not
opened, which is a lie that looks like a finding.

### The name box is gone, and the reason is the second one

It read GRP-KPI-Checklist-DD-MOSQUES.xlsx and no file has been called that since the folder tree
landed. Every workbook is named from its plot's UID2. The box is out and one line stands where it
was, saying the root, then the component folder, then the UID2, with the workbook named after its
folder, and showing one real path.

**`OutputName.Suggested` is DELETED, and this is the second of the two reasons a thing gets
deleted here, not the first.** The first is reachability, which is the test that was wrong last
round. This is the other one: **the shape it is the record of no longer exists.** There is no
one file per template any more, so a name built from a template and a date is not an unused
method, it is a method describing a thing this tool does not do. Its test went with it.
`OutputName.Final` and `Extension` stay, because a file still has to be named and they are what
names it.

### Count what happened, never what was planned

The run said **MOSQUES: Nothing was written. 20 of 21 plots wrote a workbook.** Twenty workbooks
were on disk. One line held two records of one fact: the row counted what the run set out to do
and the sentence beside it counted what happened.

`TemplateOutcome` counts workbooks now. `Workbooks` is the number, `Written` is that number above
zero, and the new `WroteSomeOfThem` is the case that had no word before, some wrote and some did
not. **Refused is kept for a template where NOTHING was written**, which is the one place the
word still fits. The summary line went the same way: it read 1 workbook written of 2 templates
ticked on a run that wrote 98, because it was counting templates and calling them workbooks. It
counts workbooks, plots and templates each as itself now.

### The create block was a wall

Nine lines of red and orange before the button, and on 78 street plots the plot list alone ran
off the screen.

- **A template row never lists every plot.** `CreateWords.Range` names up to four and gives the
  count and the first to last beyond that, so STREETS reads 78 plots, ST-01 to ST-78. The report
  names them, and the row says so.
- **A refusal and a note no longer look alike.** One stops a workbook and the other does not.
  The three that are notes rather than refusals, the links, the groups left out and a template
  with no ticked plot, go through a new `Noted` that reads in the ordinary text colour and starts
  with the word Note. Nothing moved and no control changed.
- **The link note was six lines of paths.** `LinksLoaded.OnThePane` gives the count and what to
  do about it, and the names stay in the report where there is room for them.
- **The STREETS area line was wrong.** It said the number is typed by hand. It is not: the sheet
  works the area out from the road width and the total length, and both of those come off the
  street reference file. Corrected.

### Four break watches, all restored byte for byte

Each break was made with a script, built, tested, then restored from a backup and checked with a
diff that comes back empty.

1. `FixedCells.Off` taking `"D" + row` as the value cell, which is the letter two of the three
   templates use, rather than the cell right of the label. **5 red**, every one of them in
   `LabelledCellsTests`: the mosque layout, the street layout, the plan writing, a label found
   twice and the whole word match. The street one is the one that matters, because that break is
   exactly the bug the round exists to prevent.
2. `TemplateOutcome.WroteSomeOfThem` returning false, so 20 of 21 reads as refused again.
   **1 red**, `ATemplateWhoseOnePlotFailedStillWroteTheOtherTwenty`.
3. `CreateWords.Range` listing every plot by name whatever the count. **1 red**,
   `ARowOfManyPlotsNamesTheCountAndTheRangeRatherThanEveryPlot`.
4. `WroteAcross` counting templates and calling them workbooks. **1 red**,
   `TheStatusLineCountsWorkbooksAndPlotsRatherThanTemplates`.

Seven existing tests were changed by hand, each because the truth under it moved rather than
because it was failing: two skip reasons now read the template was not opened, the two fixed
values are named without a cell being guessed, a template row drops the words written to and
gains The report names them, and the area reason names the reference file.

### Open, for Bader

**NON-PERMEABLE HARDSCAPE IS BLANK ON EVERY WORKBOOK AND NO NOTE MENTIONS IT.** Nobody has said
where it comes from. It is not read, it is not computed and nothing in this repository names a
schedule, a parameter or a filter that would produce it. It is written down here rather than
guessed at. Three things would settle it: which schedule or parameter holds it, whether it is an
area or a count, and whether a plot with none should print a zero or stay blank. Until one of
those answers arrives the cell stays blank and the report says nothing about it, which is the
honest state rather than the finished one.

### A guard that failed open, found by walking into it

**`writing-check.sh` cannot see a commit message passed on standard input, and says nothing.**
This round's first commit was made with `-F -` and a heredoc. It carried a co-author credit line,
which is exactly what that hook exists to refuse, and the commit was taken without a word.

The mechanism is one line. `commit-scope.py` reads the message out of the COMMAND, and for a
`-F` path it opens the file, except that `if path == "-": continue` skips it in silence. The hook
then scans an empty string, finds nothing and passes. Checked by calling the script by hand with
that command: what comes back as the message is zero bytes. The other two commit hooks read the
same script, so the same command hides a commit's message from all of them.

**This is the shape CLAUDE.md already records at the top of its list**, and it is the second time
for this same file. The first was the file list split on newlines, where a name holding a space
reached the scanner in pieces. The rule that came out of it was that a check which cannot see its
own subject has to refuse. A path of `-` is that case: the message is real and the hook has no
way to read it, so the honest answer is a refusal naming the reason rather than a pass.

The commit was reset and made again through a message file the hook can open, so the guard really
ran on it, and the credit line is gone. **The hook itself is NOT changed here.** It belongs to no
task, every session depends on it, and a one line fix to a shared guard is not something to slip
into a KPI round. It is written down for Bader instead. The fix is small: refuse when a message
file is `-`, the way an unreadable file is already refused through
`RCRC_UNREADABLE_MESSAGE_FILE`.

### Counts

**Locally 1640 tests at this branch, 0 failed and 0 skipped, 820 of them KPI, 15 added, against
the 1625 main carries** at the branch point `70fdb82`. Build zero warnings, measured after the
last file was written.

Pull request 114, merged into main as `9c865b1`. **The runner executed 1640 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `9c865b1`, 0 failed and
0 skipped, 820 of them KPI.** The merge went through the API with the title and the message both
passed on the call, and the commit came back off main carrying neither a co-author credit line
nor a generated-by footer. **The merged tree IS byte for byte the branch head** this time,
checked by diffing `4b6fc7c` against `9c865b1` over src, tests, .claude and steps, which comes
back empty. Last round it was not, because a file was uploaded to main between the branch point
and the merge, so this is checked rather than assumed.

---

## 2026-09-13, sixty fourth pass. One workbook per plot, in a folder tree

The team came back with how they file these. A checklist is one plot and it lives in a folder
named after it. The branch came off a fresh pull of main at `386948e`, so the baseline is
**1578 tests, 758 of them KPI**, measured at that commit before anything was written. **The audit findings
stay open, not renumbered and not reordered, and this round closes none. Counted off the two
files today: 29 plus 20 is 49 findings, 13 carrying a FIXED mark, so 36 are open.** The round
message said 32. The files say 36 and nothing here was changed to make the two agree.
**Nothing in this round has been observed in Revit.**

Pull request 113, merged into main as `ae9c324`. **The runner executed 1625 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1625 ran at `ae9c324`, 0 failed and
0 skipped, 805 of them KPI.** **The branch itself carried 1625, 47 added here**, against the
1578 main held at the branch point `386948e`. Build zero warnings at the merge. The merge went
through the API with the title and the message both passed on the call, and the commit came back
off main carrying neither a co-author credit line nor a generated-by footer.

**The merged tree is NOT byte for byte the branch head, and that is not a fault of this round.**
Bader uploaded `Branded_Factsheet_Template.docx` straight to main through the web at 13:04 local,
commit `57e606a`, which is between the branch point and the merge, so `ae9c324` sits on top of it.
Every file this round touched landed intact, checked by diffing the branch head against merged
main over `src`, `tests`, `.claude` and `steps`, which comes back empty. That 2 MB file is the
only difference and it is not mine to touch. **This repository is public**, which is worth saying
once beside a branded template nobody here put there.

**GitHub was in a declared major outage on Pull Requests while this was merging.** Two squash
merge calls came back 500 and 502 and the create call came back 500 once before that. Nothing was
forced and nothing was worked around: the press was retried until it took, and the status page
was still reading major outage when the merge that succeeded went through, so the page lagged the
recovery rather than the other way round.

### What the reference file really measures

Bader sent Scope_Validation_21072026. It is not in this repository and never will be. Read for
its shape and its numbers, 2026-09-13:

```
one sheet, Sheet1, header in row 1, 8,353 data rows
D  ID_UID *        H  ES_QUANTITY     I  QUANTITY UNIT     O  ROAD_WIDTH
A1 holds the number 21484 and there is NO B1 CELL AT ALL
units      m 6,301,  sqm 2,051,  <Null> 1
ANH-007-ST rows   313, which is exactly what the round message predicted
ANH-007-ST-100210 reads 330.65849900000001, m, 20, the row the message names
widths on those 313   15 on 156,  20 on 65,  10 on 57,  30 on 15,  36 on 13,  and 5, 6, 8, 12
duplicate UIDs    33, NONE of them in ANH-007
```

**Three of those are why the code is shaped the way it is.**

The four wanted columns sit at D, H, I and O with a gap at B, so a reader counting along the row
would take C, G and L and one of them would be a status word. They are found by the names in the
header row.

**The file writes an absent value as the text Null in angle brackets** rather than leaving the
cell empty, on all 2,051 sqm rows' road width and on one row's unit. Handed to a parser of my
own it might have come back 0. It goes through `CellNumber`, which reads no number out of text,
and the plot is named instead.

**156 of 313 street plots read a width of 15.** The component values are STREET 30m ROW, STREET
36m ROW, NH STRT 20m ROW and NH STRT LESS 20m ROW. Reading a width off the component name would
have been wrong on more than half of them, which is what Bader's do not estimate is about, and
the file proves it rather than the instruction alone.

### 1. One workbook per plot, in the tree

`PlotWorkbookPath` is the rule and `ComponentFolders` is the table under it. The handler's
`OneTemplate` still reads its whole share in one pass, untouched, because the held readings, the
progress count and the area unit are all decided once per template. `OnePlot` is new and sits
under it: each reading is merged, reconciled, planned, patched and filed on its own.

**Nothing about reading a plot changed.** The merge functions take a list of one now, which is
what makes that literally true rather than a claim: the same `KpiMerge`, the same
`Reconciliation`, the same `SpeciesMatching`, the same patcher.

**The plot gets a folder of its own holding one file.** That reads as one level too deep until
you know why, so it is written down: the PDF asked for later goes beside it.

**A UID2 that would not sit in a path refuses rather than being cleaned**, because this one is
what the team searches folders by and a cleaned name is a plot nobody finds.

### 2. A platform answered differently from the machine the tool runs on

`Path.GetInvalidFileNameChars` was the first guard on that UID2. On Windows it names nine
characters and the control characters. **On the Linux runner this gate uses it names two**, the
null and the forward slash, so `ANH*007?` was refused by the tool on a real machine and accepted
by the test that exists to check the tool. My own test caught it on the first run.

The nine are written out as data now and both separators are checked whichever machine this runs
on. It is the same shape as every other rule here: ask what the thing IS, and where the answer
depends on where you are standing, write the answer down.

### 3. The component folder table, and a count that did not agree

Eleven values, and **the table as Bader wrote it reaches EIGHT distinct folders.** The round
message said nine. The eight are named in a test by hand rather than counted off the list, and
the likely ninth is GOVERMENT BUILDING, spelt that way in the team's snip, deliberately not in
the table because no plot in either measured model carries a component for it. **That is a
question for Bader and nothing guesses at it.**

The two mosque values share one template and get two folders, which is the whole reason this is
a second table beside `ComponentTemplates` rather than a rule on the template name.

### 4. Two cells that are always the same, and a cell reference that does not exist

Character is always Urban Area Zone and Context is always Urban. `FixedCells` holds both.

**Which cell each goes in is measured nowhere.** Neither word appears in the map, in the rules,
or in any run this repository records, and the round message itself says the STREETS sheet is
laid out differently, Category and Character against Character and Context. So the mechanism is
built, no template's map names a cell, and every template reports both as not written with that
reason, one line per template in every run, until somebody measures them.

Guessing a cell reference would put Urban Area Zone into whatever D4 happens to be on seven
client templates and the workbook would look filled. **That is the one part of this round that is
UNKNOWN rather than done**, and it is one line per template in the map when the measurement
arrives.

### 5. A judgement about the root, cheap to overrule

Section 2 calls the root a third remembered thing and section 4 calls the street file a third
Browse button. Those two counts cannot both be right, so one of them had to be read.

**I took the output folder to BE the root.** Three reasons. The street file being the third
button counts templates, output and street, which leaves no room for a fourth. A second folder
deciding nothing is how one stale string became a dead end here already, which this file records.
And the pointer file name is unchanged, `kpi-output-folder.txt`, so nobody's existing setting is
lost and the same refusal already guards it. If Bader wanted a fourth browsed thing it is one
`RememberedFolder` and one pane row.

### 6. The report, and what stands down

One report for the run. The accounting gained the per plot half the round asks for: plots ticked,
folders made, workbooks written, plots that wrote nothing, and **written plus wrote nothing must
equal ticked**. The folders are counted beside those two and deliberately not among them, because
a folder can outlive a refused workbook and one made by an earlier press is not made twice.

Two lines that used to sit in the accounting, plots that went into a workbook and plots ticked and
written nowhere, are gone. They counted plots off the TEMPLATE outcomes, and keeping them beside
a per plot accounting would be two records of one fact.

**Nothing is deleted.** What is no longer reached, and why each was kept, is in the rules file
under its own heading: the merged species list across plots, the identical raw area check, the
sum against the printed total across plots, the plot counted into two workbooks, and the output
name box. Each is the only record of a measurement or a rule.

**The rounding room on a group total does NOT stand down**, against what the round message said.
It is per schedule and per plot, inside `ShrubsAndLawnRows`, and nothing about it moved. Said
here because a round that reported it as stood down would be wrong in the log for ever.

### The break watches

Five, each restored byte for byte and each checked with a diff against its backup.

```
the plot's own folder dropped from the tree     3 red   ThePathIsTheRootTheFolderTheUidAndTheUidAgain
                                                        FourStreetValuesFileIntoOneFolderAndKeepTheirOwnPlotFolders
                                                        TheTwoMosqueComponentsShareATemplateAndNotAFolder
FUTURE PARKS spelt singular, one letter         2 red   EveryComponentValueReachesItsFolder
                                                        ElevenValuesReachEightFolders
the four columns read at A B C D by position    9 red   every StreetReferenceTests case
the two fixed cells skipped and unrecorded      3 red   TheTwoFixedValuesAreNamedAndNoCellIsGuessed
                                                        EveryMappedCellWithAValueBecomesOneWrite
                                                        AValueNoChosenPlotHeldIsSkippedAsNotFound
a plot with no folder counted nowhere           1 red   WrittenPlusWroteNothingIsTheTickedCount
```

**The fifth found a gap in my own test rather than in the code.** Breaking
`PlotsThatWroteNothing` to count only plots that got a folder went GREEN, because every plot in
that test had one. A plot no route places gets neither a folder nor a workbook, which is exactly
the case the accounting exists to catch, and it was not in the test. The case is in it now and
the same break reddens it.

Six existing tests were changed by hand, each because the truth under it moved:
`EveryMappedCellWithAValueBecomesOneWrite` and `AValueNoChosenPlotHeldIsSkippedAsNotFound` for
the four new cells, `TheStreetsBlockNamesNoAreaCellAndSaysWhy` because D8 is on the STREETS sheet
now and is not an area, `TheExistingParksBlockNamesEveryCellAndWhereItsValueComesFrom` because
one plot's area comes off its one chosen region rather than being totalled off several, and
`TheReportOpensWithTheRunAndCarriesEveryTemplateUnderItsOwnName` for the per plot accounting.

### Open, for Bader

1. **Eight folders or nine.** The table gives eight. Is GOVERMENT BUILDING the ninth, and what
   component value reaches it
2. **Where Character and Context sit**, per template. Nothing is written until this is measured
3. **The root and the output folder.** Taken as one thing, for the reasons above. If they are two
   things, say so
4. **The other six asset types in the reference file.** Parking, mosque, park, school, health and
   government rows are all in there and nothing reads them

---

## 2026-09-13, sixty third pass. The read press is agreed, and the seven members are judged

Two answers from Bader and one short piece of work. The branch came off a fresh pull of main at
`6e1b309`, so the baseline is **1456 tests, 763 of them KPI**, measured at that commit before
anything was written. The other 36 audit findings stay open and this round closes none.
**Nothing in this round has been observed in Revit.**

Pull request 104, merged into main as `e2e14a3`. **The runner executed 1451 tests against the
pull request head, 0 failed and 0 skipped. Locally the same 1451 ran at `e2e14a3`, 0 failed and
0 skipped, 758 of them KPI.** **The branch itself carried 1451, 5 fewer than here**, against the
1456 main held at the branch point `6e1b309`, because five tests went with the members they
tested and nothing new was added. Build zero warnings at the merge. The merged tree is byte for
byte the branch head, nothing landed in between. The merge went through the API with the title
and the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### 1. The Read this model button stays, and the judgement note comes off

Bader's answer, in his words: the round message said Create is the only thing that reads and the
rule is that nothing reads without a press. **A button is a press. The fault was a read nobody
asked for rather than a way to ask for one.** So the wording was wrong and the button was right.

He also placed it: **it is better placed than KPI Scan was.** That button sat at the top of the
pane and on the ribbon whether or not anybody needed it, and this one sits in the plots block,
where the thing it produces goes.

The paragraph in `.claude/rules/kpi-rules.md` that named the press as my judgement and said it
was cheap to overrule is replaced by that, recorded as agreed. Nothing in the code moved.

### 2. The seven members, one at a time

**Reachability is the wrong test and last round proved it**, because `OutputName.Suggested` was
deleted on reachability alone and was the only record of the output name's shape. Bader's test
is what each one RECORDS. Three questions each: what shape or measurement is it the written
record of, is that shape written down anywhere else by name, and would any test lose its meaning
without it.

**`CreateWords.GroupsHeading`**, the line over the button row. It records that the grouping went
by the plot prefix and that a button reading MOSQUES has to say what gathered its plots. That is
written down in `PlotPrefixes`'s own class docstring and in the prefix section of
`kpi-rules.md`. **No test anywhere references it**, not one, so no test loses anything and there
is no red to watch for it either. DELETED.

**`CreateWords.GroupLabel`**, a button's own text, MOSQUES, 2 plots. It records that a control
offering to tick a template's plots names the count BEFORE the press rather than after it, and
the plural of plot against that count. The rule is `CreateWords.TemplateRow`, which is live and
prints MOSQUES: 2 plots, DM-12, FM-05 on every ticked row, and the plural comes off
`CreateWords.Count`, which both of them call. `TheRowCountsWhatWillGoInRatherThanWhatItCouldTake`
already asserts that line by hand. DELETED.

**`CreateWords.NoGroupFor`**, the footnote naming the plots no button reached. It records that a
plot no route places has to be visible rather than left out in silence. `TemplateSplit.Unplaced`
records that now, the pane prints one warned line per unplaced plot with its own reason, the
report heads a section PLOTS TICKED THAT WENT INTO NO WORKBOOK, and
`APlotWhoseTemplateIsNotTickedIsNamedAndNotRead` asserts it by hand. **Its own advice is now
false**: it says to tick such a plot by hand, and ticking one today lands it in `Unplaced`
writing nowhere. DELETED.

**`PlotTicks.OnlyFor`**, the press itself. It records REPLACE rather than add, on the reasoning
that one checklist is one template. **That is not a duplicated record, it is a contradicted
one.** `TickingATemplate.Ticked` adds, because several templates are ticked at once now, and its
docstring and `kpi-rules.md` both say why replacing is wrong. A method implementing the rule the
tool decided against is a second and contradictory record waiting for a caller, which is the
shape this repository keeps paying for. DELETED.

**`PlotPrefixes.Grouped`**, one entry per template the plots point at. Two shapes. The ordering,
one entry per template in `KpiTemplates.All`'s own order rather than in the order the plots came,
which `PlotsPerTemplate.Split` does and `TheSharesComeOutInTheTemplateListsOwnOrder` asserts. And
leaving a template no plot points at out of the list, which is **deliberately reversed**: Bader's
decision is that such a template stays ticked, stays listed and says it will write nothing, which
is `PlotsPerTemplate.NoPlotBelongs`. One half recorded elsewhere and one half overruled. DELETED.

**`PlotPrefixes.WithNoKnownPrefix`**, the plots the table has no prefix for. It records that an
unknown prefix is a real answer with its own bucket rather than an absence. `Across` is what
holds that bucket, it is live through `TemplateForComponent.OnePrefixTemplate`, and
`AcrossNamesEveryTemplateTheChosenPlotsPointAt` asserts its name, (no prefix this tool knows), by
hand. This was one line over it. DELETED.

**`PlotPrefixes.PlotsFor`**, the prefix route applied over a list. `PlotPrefixes.For` is the
route itself, it is live through `PlotsPerTemplate.For`, and `EveryConfirmedPrefixResolves`
writes all ten prefix answers out by hand. The order it came back in, the model's own, is
recorded by `TickingATemplate.PlotsOf` and asserted there. DELETED.

### Why all seven came out the same

Bader asked for this rather than for seven identical answers, and the answer is that **they are
one feature's parts and not seven things.** A heading, a button's text, its footnote, its press
and the three lookups that fed it are the grouping row, and a feature is removed as a feature.
What was worth keeping was never among them. It is the prefix TABLE, which is the measurement
the team confirmed, and `All`, `Of`, `For`, `Across` and `PrefixesFor` all stay.

The two rounds come out opposite ways for a reason that is visible in the two subjects.
`OutputName.Suggested` lost its last caller while **the thing it named still existed**, so it
was the only record of a live shape. These seven lost their callers because **the thing they
served stopped existing**, and every shape worth keeping had already been written down somewhere
that is still live and still tested. Two of them were worse than merely unused, and those two
are the ones worth reading twice: `OnlyFor` and `NoGroupFor` both give answers the tool has
since decided against.

### The one kept on that test, and it is not one of the seven

**`PlotPrefixes.PrefixesFor` is reached only from tests and stays**, with a docstring that now
says so in those words. Git says it never had a caller outside the tests in any round, which
makes it a plainer case than the seven rather than a leftover of theirs. It is the only record of the many to one shape: three prefixes mean
STREETS, two mean MOSQUES, HEALTHCARE has one. Read the other way, off `For`, a template is
reached one prefix at a time and the many to one is invisible.
`ThreePrefixesMeanStreetsAndTwoMeanMosques` writes those three lists out by hand and
`EveryTemplateIsReachedByAtLeastOnePrefix` is what the agreement with the component table rests
on. That is the comment Bader asked for, put where it applies rather than on a member that did
not earn it.

### The break watches

Four, each restored byte for byte and each checked with a diff against its backup.

```
PlotsFor returns an empty list      3 red   GroupingGathersEveryPlotForOneTemplate
                                            OneGroupingPressTicksEveryPlotOfThatTemplateAndNothingElse
                                            ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook
Grouped returns an empty list       2 red   GroupingOffersOneButtonPerTemplateThePlotsReallyPointAt
                                            TheGroupingWordsSayWhatTheButtonDoesAndWhatItLeavesOut
WithNoKnownPrefix returns nothing   1 red   APlotNoButtonGathersIsNamed
GroupLabel drops its count and
NoGroupFor drops its opening        1 red   TheGroupingWordsSayWhatTheButtonDoesAndWhatItLeavesOut
```

The first break reddened `OnlyFor`'s test through `OnlyFor`, which is how a deletion two members
deep was watched rather than assumed. **`GroupsHeading` is the one with nothing to watch**, and
that is its own finding: a constant no test and no caller mentions is a line of text nobody
would learn was wrong.

Five tests went with the members, the four that tested only them plus the grouping press. One
test of my own was rewritten rather than deleted: the contrast in
`ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook` used `PlotsFor` to show what a prefix
button would have ticked, and it now names the two answers for DM-12 side by side,
`PlotPrefixes.For` saying MOSQUES and `PlotsPerTemplate.For` placing it nowhere, which says the
same thing about one plot instead of about a list.

---

## 2026-09-13, sixty second pass. The header costs nothing, and a template ticks its plots

Three things off the first press over several templates, NG05 at 08:37. Two faults and one
regression. The other 36 audit findings stay open. The branch came off a fresh pull of main at
`d6c9f4a`, so the baseline is **1443 tests, 750 of them KPI**, measured at that commit before
anything was written. **Nothing in this round has been observed in Revit.**

Pull request 103, merged into main as `f41b044`. **The runner executed 1456 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1456 ran at `f41b044`, 0 failed and 0
skipped, 763 of them KPI.** **The branch itself carried 1456, 13 added here**, against the 1443
main held at the branch point `d6c9f4a`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### 1. Every path that can start a read, one by one

Asked of the code rather than reasoned about. Five paths, four of them already right.

```
the pane becoming visible      Ask(WhichModel), the title alone        FREE     was right
every redraw of the block      Ask(WhichModel), the title alone        FREE     was right
a model answering with a title Ask(Plots), every sheet and schedule    HEAVY    THE FAULT
Create                         the scan and the plot reads inside it   HEAVY    a press
DocumentOpened, DocumentClosed the KPI pane subscribes to NEITHER      none     was right
```

The third is the one. `Took` asked for the plots whenever a title it had not read plots for
answered, which is every model anybody opens, because a dockable pane is restored visible at
Revit startup. It asks for nothing now.

**The header is decided in Core.** `KpiHeader.Lines` is three states with one line each and
only the third names a count. A model nothing has read says so and names NO NUMBER, because a
read that has not happened is an absence rather than a zero, and the test asserts that line
carries no digit at all.

**The plot read is behind a press, and that press is a judgement of mine.** The round said
Create is the only thing that reads, and the plot picker cannot be used before the plots exist,
so either the picker waits for a first press of Create or a press exists to fill it. I added
Read this model to the plots block, which is one control where this round removes a whole row of
them. It is written into the rules as a judgement so it is cheap to overrule.

### 2. Ticking a template ticks its plots

`TickingATemplate` is the rule and the template row is the grouping button now, so the separate
row of buttons is gone.

**It ticks by the SPLIT'S rule and never by the prefix.** The buttons used
`PlotPrefixes.PlotsFor`, two letters at the front of an identifier, while the split reads
PRX_Component first. Two rules for one question is this repository's oldest fault, and here it
would tick a plot that then lands in no workbook. A test ticks MOSQUES over a plot whose
component says SCHOOL: the prefix would have taken it and the tick does not.

The three things asked for. **A plot unticked by hand stays unticked** and the hand list clears
when the model changes, because another model's DM-14 is not this one's. **The count on the row
is what will go in**, which needed nothing new: the row counts off the split of the TICKED
plots, so a hand untick moves it from 3 to 2. **A template with no plots still ticks and still
says it will write nothing**, unchanged.

One thing the round had to add that the message did not name: **a row ticked before the read
gets its plots when the read lands.** The template rows come off the templates folder and need
no model, so ticking MOSQUES and then pressing Read is the ordinary order.

**What the buttons did that the rows do not**, which the round asked me to say. Two things, both
deliberate. They REPLACED the ticks rather than adding, which was right when one checklist was
one template and is wrong now. And they could tick a template's plots with that template's
workbook unticked, which is now impossible and is the point. `GroupsHeading`, `GroupLabel`,
`NoGroupFor`, `PlotTicks.OnlyFor`, `PlotPrefixes.Grouped`, `WithNoKnownPrefix` and `PlotsFor`
are now reachable only from tests. **I did not delete them**, and that is the lesson of the
round before: `OutputName.Suggested` was deleted on reachability alone and it was the only
record of the output name's shape. Whether they go is Bader's call.

### 3. The output names have their shape back

`OutputName.Suggested` is restored and asked per row with that row's own file, so each ticked
template carries the shape rather than one of them carrying it. Checked against what the working
runs wrote: `GRP_KPI_Checklist_DD_MOSQUES.xlsx` suggests
**GRP-KPI-Checklist-DD-MOSQUES.xlsx** and the STREETS file suggests
**GRP-KPI-Checklist-DD-STREETS.xlsx**, both written out by hand in the test.

**The deletion was mine and the reasoning was wrong.** A method the last caller stopped calling
can still be the only record of a shape. That goes in the rules beside the restored method
rather than only here.

### The mockup

`design/pr-103/kpi-pane.html`, hand drawn from the code: the table of every read path with its
verdict, the header's three states side by side, the plots block before and after the one press
with the grouping buttons struck through, and a ticked MOSQUES with DM-14 taken off by hand
reading 19 rather than 20. It says in the file that it is a mockup and not a screenshot.

### Three watches, one per item, all red

**Item 1.** `KpiHeader.Lines` made to print the count line for a model nothing has read. **1
red**: `KpiHeaderTests.ShownWithADocumentNothingHasReadNamesTheModelAndNoCount`.

**Item 2.** The hand list check in `TickingATemplate.Ticked` replaced with one that never
fires. **2 red**: `TickingATemplateTests.APlotUntickedByHandStaysUntickedWhenItsTemplateIsTicked`
and `TheRowCountsWhatWillGoInRatherThanWhatItCouldTake`.

**Item 3.** `OutputName.Suggested` made to answer the template name alone again, which is
exactly the regression. **1 red**:
`OutputNameTests.TheSuggestionIsTheTemplateFilesOwnNameAndKeepsItsShape`.

All three restored byte for byte, each checked with a diff against its backup.

### Two existing lines changed by hand

`CreateWords.PlotsBlock`'s second state said the plots were BEING READ and its first promised
that the pane reads a model's plots as soon as one is open. Both were true and both were the
fault. They read as not read yet and waiting for a press now, and
`KpiCreateTests.ThePlotsBlockReadsFourWaysOneLineEach` was changed with them, with its docstring
saying why.

### What worked on that press, on the record

The link note fired and named the link, 1 instance of 6 not loaded with the file named. The
area note fired on STREETS. The templates were opened 7 times over 16 redraws, which is the
folder listing holding its recognitions as it should. The refusal named the missing output
folder and what to press.

---

## 2026-09-12, sixty first pass. Several templates in one press, one workbook each

Round two of the two Bader sent together, off a fresh pull of main at `96b6239`, which carries
round one. The baseline is **1416 tests, 723 of them KPI**, measured at that commit before
anything was written. The other 36 audit findings stay open, not renumbered and not reordered.
**Nothing in this round has been observed in Revit.**

Pull request 102, merged into main as `131b1dd`. **The runner executed 1443 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1443 ran at `131b1dd`, 0 failed and 0
skipped, 750 of them KPI.** **The branch itself carried 1443, 27 added here**, against the 1416
main held at the branch point `96b6239`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### What must not change, and what holds it

Two correct workbooks exist, MOSQUES on NG03 at 15:52 and STREETS on NG05 at 01:30, and this
round must not cost them. **Nothing about the per template logic changed.** `KpiCreateRun` is
still one template's run and `KpiCreateReport.Write` still prints one template's sections, both
untouched. What is new sits above them: a split, a set of runs, and a report that prints each
template's own sections through the writer that was already tested.

### The split

`PlotsPerTemplate` is the decision and it is the rule this repository already carried for
preselecting, asked per plot. PRX_Component decides, the prefix is the cross check, and where
they disagree neither does. Three plots go into no workbook and each is named:

```
component in the table          placed by the component, prefix agreeing or not named
component not in the table      placed NOWHERE, with the value and what the prefix says
no component at all             placed by the PREFIX, which is what EP-05 and its three are
the two routes disagree         placed by NEITHER, both named
```

The middle one is the one worth arguing about. The prefix could have answered in the
component's place, and it does not, because the route that decides gave an answer nobody knows
and a plot going into a client workbook on a cross check alone is a guess. It is named instead.

### The double count

**A plot in two workbooks refuses the whole press.** Every plot resolves to at most one template,
so it cannot happen by construction, and `TemplateSplit` asks the split as it really came out
anyway, naming the plot and both templates. Two tests, one over the guard with shares built by
hand and one over the rule underneath it, because a construction that cannot go wrong is not a
check and the check is what survives the next change.

### Read once, and no second cache

A plot belongs to one template, so it is read once with that template's own counted groups and
its own area rule. `HeldReadings.Decide` is asked once per template with that template's own
share of the plots and the run IT produced last press, so the mechanism that turned a 123 second
read into a 2.5 second second press is the same one and there is nothing beside it. A held run
for one template never answers for another, and a test says so in the words the refusal uses.

### One thing the round had to fix in the progress line

Counting the plots per template would have restarted the count at 1 on the second workbook, and
the progress rule in this repository says the count only grows and cannot go backwards. The
total is every plot every ticked template will read, worked out before the first one is, and a
template answering from held readings advances it by its own share so the total is still
reached.

### The pane

The workbook rows are CheckBoxes rather than a pick, each ticked row carries its own output name
box, and the create block grew a row per ticked template before the press and a row per outcome
after it. Nothing moved. The three typed boxes are one set for the whole run and the pane says
so when more than one template is ticked.

### The mockup

`design/pr-102/kpi-pane.html`, hand drawn from the code: the workbook list as one pick and as
tick boxes, the create block before the press with a ticked template that gets nothing, the same
block after a press where one of three was refused, on the dark theme, and the report's own
opening. It says in the file that it is a mockup and not a screenshot.

### One deletion

`OutputName.Suggested` offered the template file's own name and nothing called it once each row
took its name from `CreateWords.SuggestedName` with its own plots. Deleted with its test rather
than left beside the thing that replaced it.

### Three watches, all red

**The double count check.** `if (holding.Count <= 1) continue;` widened to `<= 99`, so a plot in
two shares passes. **2 red**: `PlotsPerTemplateTests.APlotInTwoWorkbooksRefusesTheWholeRunAndNamesBoth`
and `RunAcrossTemplatesTests.ADoubleCountedPlotTakesOverTheStatusLine`.

**The unplaced naming.** The filter that finds plots belonging to no ticked template replaced
with one that finds none, so such a plot disappears. **1 red**:
`PlotsPerTemplateTests.APlotWhoseTemplateIsNotTickedIsNamedAndNotRead`.

**The refused count.** `WasRefused` made to answer false, so a refused template reads as one
with nothing to write. **2 red**: `RunAcrossTemplatesTests.TheFourCountsAddUpToTheNumberTicked`
and `TheStatusLineCountsTheTemplatesAndNamesTheReport`.

All three restored byte for byte, each checked with a diff against its backup rather than by
reading it.

### The one existing test changed by hand

`KpiCreateTests.OneRefusalListsEverythingMissingRatherThanOnePerThing` pinned the words No
template picked. The rows are ticked now, several at once, so the refusal reads No template
ticked. Changed with the behaviour and the docstring says why.

### What waits on a run

Everything. **Nothing here has been observed in Revit.** The two correct workbooks are the thing
this round must not have cost, and only a press over MOSQUES and SCHOOLS together can say
whether it did.

---

## 2026-09-12, sixtieth pass. The two answers, and UNKNOWN reaches row 101

Two answers from Bader, both measured on the workbook the 1552 run wrote on the NG03 model,
which is not in this repository. Neither could have been taken here. The other 36 audit findings
stay open. The branch came off a fresh pull of main at `96b6239`, so the baseline is **1416
tests, 723 of them KPI**, measured at that commit before anything was written. **Nothing in this
round has been observed in Revit.**

Pull request 101, merged into main as `3a3312f`. **The runner executed 1425 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1425 ran at `3a3312f`, 0 failed and 0
skipped, 732 of them KPI.** **The branch itself carried 1425, 9 added here**, against the 1416
main held at the branch point `96b6239`. Build zero warnings at the merge. The merged tree is
byte for byte the branch head, nothing landed in between. The merge went through the API with
the title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### The third blocker does not bite, and that UNKNOWN is closed

The fifty ninth pass said whether `BelowTheList` reaches row 101 was UNKNOWN here, because no
workbook and no report is in this repository. Bader measured it:

```
Tree List - Existing   98 names, rows 4 to 101, no empty row inside the list, first gap 102
Tree List - Proposed   80 names, rows 4 to 83,  no empty row inside the list, first gap 84
```

Row 101 is ABOVE the first empty row, so the rule never reaches it. **No behaviour changed for
this item.** The rule is right and unchanged, and what was an UNKNOWN in this log is now a
measurement with the file and the date it came from beside it.

### The name rule is an alias, and the reasoning is the point

**It is not a rule at all.** Exactly one of the 98 names opens with UNKNOWN, so any rule built
on a shared opening, a prefix or a longest match would work on that one name and then have to
pick between Conocarpus erectus and Conocarpus lancifolius, between Ficus benjamina, Ficus
religiosa and Ficus pseudosycomorus, and between Prosopis juliflora and Prosopis glandulosa.
A picked genus is a silent wrong number in a client file, which is worse than a tree that goes
nowhere. And UNKNOWN is not a species: it is Revit's placeholder and Unknown Tree is the
client's placeholder for the same thing, so the two meeting is a fact about this project and a
fact about the project is data.

`SpeciesAliases` in Core is the table, one entry, UNKNOWN means Unknown Tree. Three guards, all
tested.

**It applies only where it resolves to exactly one row on that sheet.** Two rows is a refusal
naming every one of them. None is nothing at all, and a species then goes down the empty row
route exactly as it did before, where the report already names it, so nothing is skipped and
nothing is silent.

**It never overrides a real match.** The exact name is matched above the table and the table is
not consulted at all, and a name below the first empty row keeps its own refusal, because that
is a row the workbook really carries.

**The report says which count rests on it.** SPECIES MATCHED grew a how column, matched on its
own name or THROUGH THE ALIAS UNKNOWN means Unknown Tree, and SPECIES MATCHED THROUGH AN ALIAS
is a block of its own with the alias, the sheet, the row and the count.

`ClosestName` is untouched and still prints beside every remaining miss. It is how the next
alias gets found and it is still a print and never a match.

**The check cannot be run here.** UNKNOWN Existing is 16 trees, Total Trees should move 374 to
390 and the canopy should stay at 11,168, because row 101 holds a real zero for its diameter.
Nothing in this repository can open that workbook, so those three numbers are for the next run.

### One thing the round simplified

The total's reach used to be checked in the matching loop and would have been checked a second
time on the alias route. `OnTheRow` is the one place a row becomes a match now, asked by both,
so there is one rule rather than two that agree on the day they are written.

### Two watches, both red

**Guard a.** `if (aliased.Count > 1)` changed to `if (aliased.Count > 99)`, so two rows would
take the first. **2 red**: `SpeciesAliasTests.AnAliasThatReachesTwoRowsIsRefusedAndNamesBoth`
and `SpeciesMatchingTests.AnAliasReachingFourRowsIsRefusedAndNamesEveryOneOfThem`. Restored.

**Guard b.** The alias consulted before the exact match rather than after it. **1 red**:
`SpeciesAliasTests.AnExactMatchWinsAndTheAliasIsNotConsulted`. Restored byte for byte and
checked with a diff against the backup rather than by reading it.

### The one existing test changed by hand

`SpeciesMatchingTests.UnknownIsNeverMatchedToUnknownTreeAndTakesNoRowEither` pinned the old
rule, that UNKNOWN could reach no row and was withheld for having no size. Its fixture holds
four Unknown Tree rows, which is the 2026-09-09 measurement, so under the alias it is refused by
guard a instead. It is renamed `AnAliasReachingFourRowsIsRefusedAndNamesEveryOneOfThem` and
asserts the new refusal word for word. **The count still goes nowhere either way**, which is
the part of it that did not change, and its docstring says why it was rewritten.

### Still never exercised

**Street Design counting as Proposed on STREETS now HAS a run.** The STREETS workbook on NG05 at
01:30 read ST-05 as 369 existing, 2 proposed and 68 Street Design taken as proposed against the
schedule's own TOTAL of 439, which is the exact arithmetic the rule was written to. That
sentence has been in this log as never exercised since the rule landed and it comes out now.

---

## 2026-09-12, fifty ninth pass. UNKNOWN is written, and a run with no link loaded says so

Two items, measured on two runs. The other 36 audit findings stay open. The branch came off a
fresh pull of main at `e3ba145`, so the baseline is **1316 tests, measured at that commit in a
worktree of its own rather than remembered**. **Nothing in this round has been observed in
Revit.**

Pull request 97, merged into main as `6275bf3`. **The runner executed 1385 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1385 ran at `6275bf3`, 0 failed and 0
skipped, 723 of them KPI.** **The branch itself carried 1332, 16 added here**, against the 1316
main held at the branch point `e3ba145`, measured there in a worktree of its own. Build zero
warnings at the merge. The merge went through the API with the title and the message both
passed on the call, and the commit came back off main carrying neither a co-author credit line
nor a generated-by footer.

**The merged tree is NOT byte for byte the branch head.** Two Drawing Sheet rounds landed while
this one waited on a GitHub rate limit, `d77b90a` and `e2aa4ad`, pull requests 96 and 98, and
that is the whole of the jump from 1332 to 1385, 53 tests, none of them KPI. **The KPI half did
land byte for byte**: a diff of this task's files between the branch head `621f969` and the merge
is empty, and the KPI count is 723 on both.

### Item 1(b) was already correct, and saying so is the first thing this entry owes

The round message named two reasons UNKNOWN was not written on the 1552 run, and asked for the
second to be fixed: the empty row route withheld it for having no diameter, the right rule in
the wrong place, and it must not apply to a row the workbook already holds. **At today's lines
it already does not.** `SpeciesMatching.Against` asks `MeasureAnswer.Write` of the canopy
diameter on the `WrittenInto` branch alone, `NotSized` is constructed nowhere else, and
`KpiCreatePlan` writes a matched row's count into column B and touches none of the client's
other cells. There was one reason UNKNOWN went unwritten, not two, and it is the name.

That is worth more than the fix would have been, because the premise came from reading a report
rather than the code and it would have been fixed by changing a rule that is already right. It
is pinned now rather than left as an assertion in a log entry: `AMatchedRowWithNoDiameterIsStillWritten`
puts UNKNOWN on a matched row with no height and no diameter, and asserts the count is written
into column B and that the name, the height and the diameter columns of that row are not
written at all.

### The third blocker, which is in no round message

Even a name matching word for word is refused when it sits past the list's first empty row.
`SpeciesList` reads column D as far as the first gap and holds everything after it in
`BelowTheList`, and `SpeciesMatching.Against` refuses a species whose only name is there rather
than writing it in a second time above the gap. That rule is right and is not being changed.

**Whether it bites for the real row 101 is UNKNOWN from this repository.** No workbook is in it,
nothing under `reports/` is committed, and the round message gives row 101's cells but not where
the list's first empty row is. The create report already prints the names below the first empty
row with their rows under THE WORKBOOK'S OWN TREE LISTS, so the 1552 report answers it off the
file. `ANameHeldBelowTheFirstEmptyRowIsRefusedEvenWhenItMatchesExactly` pins the refusal and its
exact words, so whichever way the answer goes, the behaviour is written down rather than
discovered again.

### The name, which is Bader's to rule on

**The match is not widened.** UNKNOWN against Unknown Tree still does not match, and no rule
was invented for it. What the tool does instead is report what it missed:
`SpeciesMatching.ClosestName` finds the workbook name sharing the longest opening with the
Revit name, ties broken by the shorter name and then naturally, empty where nothing is shared
at all. It rides on `SpeciesMatch.NearestInTheList` and prints as the second column of the
unmatched species list. **It is printed and never matched on**, and a matched species carries
none, so the column cannot start deciding anything by accident.

**I cannot enumerate the 1552 run's unmatched species from this repository.** The report is not
committed and never will be, and the workbook is not here either. So the answer to whether it
is one name or a family of them comes off the next run, where every unmatched species prints
with the nearest name in the list beside it. Three shapes are already known and they are not
one problem: UNKNOWN against Unknown Tree is a different word, ACACIA / VACHELLIA FARNESIANA
matches Acacia / Vachellia farnesiana today, and BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an
apostrophe that nothing has yet been measured against.

**The check on this half cannot be run here.** UNKNOWN Existing is 16 trees, Total Trees moves
374 to 390 and the canopy stays 11,168 once the row is reachable. Reachable needs the name rule,
which is Bader's, so that check is for the run after the rule lands.

### Item 2, a run with no links loaded

Measured on the first STREETS run, NG05 at 16:06, 78 plots: all 78 contributed nothing, all 156
schedules printed one row and no body, group rows none on every one, and the scan says six link
instances with NONE LOADED. Every number the tool printed was right. It never said the one thing
that explains all 78.

`LinksLoaded` is the new Core file and the whole rule, five states with one line each, a note and
never a refusal. It reaches three places.

**The report opens with it.** `TheLinks` sits between the clock and the first step in
`KpiCreateReport`, so the warning is above RECONCILIATION. The test asserts the index of NO LINK
IS LOADED is below the index of RECONCILIATION rather than asserting both appear, because two
present strings say nothing about which a person reads first.

**The pane says it before the press.** `KpiPlotFacts` carries a `LinksLoaded` built by
`ReadThePlots` off the host document's own `RevitLinkInstance` collection, and `TheCreateButton`
puts it above Create beside the refusal line. Twenty minutes are spent before the report exists,
so a warning only the file holds is a warning that arrives too late to save the run.

**And the reconciliation grew the two counts it could not fake.** Every count it already carried
read green over that run: 78 ticked, 78 read, 156 schedules found, nothing refused, nothing
missing, because a schedule that printed no body is still a schedule that was read.
`GroupRowsFound` is how many group rows the run found across every plot and `SchedulesWithABody`
how many printed one, said as a count of the schedules printed. The 16:06 run reads 0 and 0 of
156.

### The mockup

The round changes the pane by one line, so it carries `design/pr-97/kpi-pane.html`, hand drawn
from the code: the Create block before and after on the light theme, the partly loaded state on
the dark theme where the warning colour is `#ff8080` rather than firebrick, and the report's own
opening with the warning above RECONCILIATION. It says in the file that it is a mockup and not a
screenshot, that Create stays live because this is a note, and that the six link names in it are
invented for the drawing, since only the count and the NONE LOADED state were measured.

### Two watches, one per item, both red

Rebuilt before each, because a `--no-build` run after a restore reads the old assembly and that
has cost this session twice.

**Item 2.** `LinksLoaded.Of` had `if (loaded == 0)` changed to `if (false)`, so a model with
six instances and none loaded fell through to the some-loaded-and-some-not line. **3 red**:
`TheReportOpensWithTheReasonARunFoundNothing`, `NotOneLoadedIsSaidWithTheCountAndEveryName`,
`OneInstanceReadsInTheSingular`. Restored byte for byte.

**Item 1.** `KpiCreatePlan` took `if (!match.Species.Diameter.Write) continue;` immediately
before the quantity write, which is the no-diameter rule reaching a matched row, the thing the
round message believed was already happening. **4 red**:
`UnknownRowTests.AMatchedRowWithNoDiameterIsStillWritten`,
`KpiCreatePlanTests.AMatchedSpeciesBecomesAQuantityInColumnBOfItsOwnRow`,
`KpiCreatePlanTests.AMatchedSpeciesWritesTheCountAloneAndLeavesTheNameAsTheWorkbookSpellsIt`,
`KpiCreatePlanTests.AnAddedSpeciesWithNoListColumnsWritesItsNameAndItsCountAndNamesTheOtherTwo`.
Restored byte for byte. **The first attempt at this one was a no-op**: the line went inside the
`!match.Added` block after the quantity write, so nothing changed and nothing went red. A break
that changes no behaviour proves nothing, and it read exactly like a break that the tests
survived.

A third, optional watch on the nearest-name guard did not go red either way it was broken,
`opening == 0` to `opening < 0` and `shared = 0` to `shared = -1`, because the tie-break still
rejects a name sharing nothing. That guard is not load bearing in that form and is recorded
here rather than left as a silent pass.

### The one existing test changed by hand

`CanopyColumnsTests.TheReportSaysWhatWentIntoEachMeasureCell` asserts the unmatched species
lines word for word, and those lines grew a column. Both its expectations took `(empty)` in the
new nearest-name position, written out by hand: the fixture's list holds only Albizia lebbeck,
which shares no opening letter with BAUHINIA PURPUREA or with UNKNOWN, so the nearest name for
both is nothing. Nothing else in the suite was touched.

### STILL NEVER EXERCISED

**Street Design counting as Proposed on STREETS has tests and no run.** The words appear nowhere
in the 2,292 line report of the 16:06 STREETS run, which is the only STREETS run there has been,
and they could not: no link was loaded, so no schedule printed a body, so no group row of any
name was found. `CountedGroups.Of` reading `KpiTemplate.GroupsCountedAsProposed`, the
`GroupByDecision` pointing at Tree List - Proposed, `SheetFor` answering by decision second, and
`KpiMerge.Species` keying on the sheet that takes the group are all covered by tests and none of
it has ever met a real street schedule. ST-05's numbers, 369 existing and 2 plus 68 proposed
against a printed TOTAL of 439, are still the only measurement behind the whole rule and they
were read off a screen rather than off a run.

### What this round did not touch

The 36 open audit findings stay open. Nothing about the softscape or shrubs and lawn readers
moved, nothing about the patcher moved, and the matching rule itself is exactly what it was.

---

## 2026-09-12, fifty eighth pass. Create scans if it needs to, and the scan button is gone

Item 1 of two, on its own as Bader asked, so that if the first real run breaks nobody has to
work out which half did it. Item 2, several templates in one run, is a round of its own and is
NOT in this one. The branch came off a fresh pull of main at `b5e2c90`, so the baseline is
**1282 tests, 700 of them KPI**. **Nothing in this round has been observed in Revit**, and the
1552 run's correct workbook is what it must not cost: 36 parts, calcChain removed, calcId 0,
calcMode auto, no cached value left, zero errors on recalculation, Total Green cover 13,516,
Canopy 11,168, Total Trees 374, Planting 1,493, Lawn 855 off 70,343.

Pull request 93, merged into main as `ffccfce`. **The runner executed 1289 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1289 ran at `ffccfce`, 0 failed and 0
skipped, 707 of them KPI.** **The branch itself carried 1289, 7 added here**, against the 1282
main held at the branch point `b5e2c90`, and the merged tree is byte for byte the branch head,
nothing landed in between. Build zero warnings at the merge. The merge went through the API
with the title and the message both passed on the call, and the commit came back off main
carrying neither a co-author credit line nor a generated-by footer.

### The scan is a step inside Create

`ScanNeeded.Decide` in Core is the whole rule and it is the same shape `HeldReadings.Decide`
already uses for the per plot readings, because two shapes for one kind of question would be
two rules to keep in step. The model is read when no scan is held or what is held is of
another model, and a scan already held answers a second press on the same model. The handler
holds the title its scan was read from and Create calls `Scan` itself before it reads a plot.
Nothing about the reading reuse moved: `HeldReadings` decides that separately and the 2.5
second finish after a 123 second read is that working, untouched.

The press says which of the two it did, `Reusing the scan already held` or the real read's own
section lines, for the same reason the readings say it. **The scan's headline moved from
`Told` to `Progressed`**, which matters: `Told` is the run's end line and it is what shuts the
progress window, so the headline said there would have ended the press on screen, and closed
the window, with the workbook still being filled.

### The button, and where the header's numbers come from now

`KPI Scan` is off the pane's strip and `AskedForAScan` is deleted, the `Scan` request is off
the enum and out of the switch, and the ribbon tooltip no longer describes a button that is
not there. Four code comments naming it are corrected and two test files pinned its old
wording, both updated by hand.

**The header's element count now comes back with the plot read** rather than off the scan.
`KpiPlotFacts` carries the count and the seconds, `ReadThePlots` counts instances the same way
the scan counts them, and `Found` fills the line. So the model's name has a number under it as
soon as the pane is shown, which is what the scan line used to do only after a press. That
read is once per model already, guarded by the title the plots were last asked for, so the
count costs one collector pass per model rather than one per redraw.

### The progress window

`KpiProgressWindow`, modeless, owned by the Revit main window through its handle, showing the
same `ProgressWords` lines the status line shows. **Opened by the pane before the external
event is raised and closed when the run's last answer comes back, never from inside
`Execute`**, which is the hazard the fifty fifth pass's breaker caught when the repaint pump
was pumping at a priority that let queued clicks run inside the handler's call frame. `Told`
closes it, being the end line whatever ended the run, a finish, a refusal or a throw.

**There is no Cancel, deliberately, and this is the round's one refusal to build something.**
Bader asked for one if it is cheap. It is not: cancelling mid read leaves a half read set of
plots that the reconciliation would count as plots read, which is exactly the half read state
the rest of the tool would trust. An honest cancel is a cancellation threaded through every
reader plus a discard of everything read, which is its own round. A cancel that lies is worse
than no cancel.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1289.

- a held scan of another model taken for this one: **3 red**, the other model case, the exact
  title comparison and the no document case
- nothing held taken as a scan of this model: **1 red**, the first press on a fresh pane
- the pane words naming the vanished button again: **1 red**, the fixed lines test

### Existing tests changed

Two, both by hand. `KpiPaneWordsTests` pinned `Not scanned yet. Press KPI Scan.` and the
`ReadOnly` line naming the button, and gained a line holding that none of the three fixed
lines names it. `KpiCreateTests` pinned the `NoPlots` line telling somebody to press it. No
test moved for any other reason.

`PaneLabelTests` at the tests root also holds the string KPI Scan, and it is LEFT ALONE: it
is not KPI's file, and it uses the words as input to the caption escape rather than asserting
any button exists.

### The diff read adversarially, and what it found

A breaker was set on the diff and died on a rate limit before reporting, so the checks were
made here instead, at today's lines. Four came back clean and one is a real limit.

- **Every path that ends a press reaches `Told`, so the window always shuts.** Run's four
  typed catches, the `document == null` return, the `CannotCreate` return and Execute's own
  `Stop(failed)` all invoke it, `Stop` inside a try of its own so even a throw on the way out
  cannot skip it. That was the worst case worth ruling out: a modeless window left over Revit
  with no way to shut it
- **The scan runs after the refusal guard**, so a press refused for no template, no plot or no
  output folder does not spend two minutes reading the model first
- **The header cannot be wiped by the redraw that follows it.** `Found` sets `_scannedTitle`
  off `_model.Title`, and `_model` is always set first, because `Named` is invoked before the
  switch that reaches `ReadThePlots`. The next `Took` then compares equal and resets nothing
- **Nothing still references the removed request.** The two `AskedForAScan` hits left in the
  tree are the Drawing Sheet's own, in its own file, untouched

**The one real limit, known and left: a second press of Create while the first is still
running leaves the second run without a window.** `Shut` closes the first window before the
second opens, so no window is orphaned over Revit, but the first run's end line then shuts the
second run's window and the second run goes on unseen. A busy flag on the press would fix it
and would introduce a worse failure: a flag that fails to clear leaves Create dead with no way
back, and the pane has no way to ask whether a run is still going. The same reasoning as the
cancel. It is written down rather than guarded, and the first run is what says whether anybody
presses twice.

### What is open

Whether the window appears where it should, whether it shuts on every path, and whether the
status line moves mid run are all things only a run on a real pane can show. The scan report's
own location now shows in the progress line while the run goes rather than sitting on the
status line at the end, because the end line belongs to Create. Whether the team wants it to
persist somewhere after the press is a question for Bader.

---

## 2026-09-12, fifty seventh pass. The plots block that said open a model, and the ten warnings

Two things off the 13:48 run on RCRC_NG03_EZ. The branch came off a fresh pull of main at
`aa0f546`, the fifty eighth pass, so the baseline is **1288 tests, 699 of them KPI**, not the
1266 Bader's message names: the Drawing Sheet rounds `#82`, `#83`, `#84` and `#88` landed
since the fifty sixth pass and none of them is KPI. **Nothing in this round has been observed
in Revit.** The first of the two changes the pane's ask flow, which no Core test can reach, so
it is recorded here for the first run on a real pane. Committed in two, the warnings first as
a checkpoint, then this.

Pull request 92, merged into main as `c8bd1b8`. **The runner executed 1282 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1282 ran at `c8bd1b8`, 0 failed and 0
skipped, 700 of them KPI.** **The branch itself carried 1289, 1 added here**, against the 1288
main held at the branch point `aa0f546`. The drop from 1289 to 1282 is the Drawing Sheet
sixtieth pass `a7cd601` that landed in between, which deleted its marker family and net
removed seven tests, none of them KPI, so the KPI count is 700 on the branch and at the merge
alike, checked. The merge went through the API with the title and the message both passed on
the call, and the commit came back off main carrying neither a co-author credit line nor a
generated-by footer. Ten warnings cleared, build zero warnings.

### The plots block said open a model while a model was open

At 13:48 the header named RCRC_NG03_EZ and the footer carried its scan counts, and the plots
block read Open a model with no list, while the same model had listed 18 plots at 12:08 on the
build before. A workflow of three investigators and a judge told the two candidates Bader
named apart from the real cause, and neither candidate was it.

- **Not the repaint pump moving from background to render.** Its premise is false: git shows
  the pump was born at render in the fifty fifth pass and never ran at background, so nothing
  moved. `_facts` has one writer, reached by the priority-less `Dispatcher.Invoke` overload
  that runs inline or marshals at Send, above any pump priority, so no pump can gate it. And
  the build before the pump listed the plots with no pump of any priority at all. Told apart
  by git history and by the single writer.
- **Not the progress line's redraws firing between the ask and the answer.** Those pumps run
  only inside a scan or a create `Execute`, and every `Execute` empties the one request slot at
  its start, so a pending `Plots` cannot be in the slot while a pump runs. Told apart by when
  the pump can run against when the `Plots` request lives.

The real cause is older than both and the round's diff never touched it. `Ask(Plots)` lived at
one site, `Shown`, fired only by a visibility rise. Installing the fifty fifth and fifty sixth
passes forced a Revit restart, a pane restored visible at startup was shown before any document
existed, that one ask was consumed against No open document, and nothing ever asked again: every
later answer runs `Took`, whose redraw asks only `WhichModel`, which reads the plots nowhere. So
a scan filled the header while `_facts` stayed null and the block drew its waiting text, whose
words were Open a model. A KPI Scan pressed before the plot read returned loses the ask the same
way, displacing the pending `Plots` in the one slot. Which of the three happened at 13:48 is a
visibility and timing fact only the user's memory or a run can settle, and it is UNKNOWN, the
restart being the most economical.

**The fix is two halves.** The message, in Core with the test: `CreateWords.PlotsBlock` reads
four ways, one line each, no document says open one, a document not yet answered says it is
reading the plots, a document answered with no plots says the model holds none, and a document
answered with plots hands to `PlotSources`. Open a model is right only where no model is open,
and the not answered state is told from it by whether a document is open, read off the live
document, never a held copy. The recovery, in the pane and untestable in Core: `Ask(Plots)`
moved out of `Shown` into `Took`, the one place that knows which model answered, guarded by the
title the last ask was for and reset on close, so the plots are read once per model and a model
opened under the pane or one whose first ask was lost is read the moment any request answers
with it. `Shown` asks only for the model now. This also killed a latent double read: `Shown`
and `Took` both asking would have read the plots twice on every show.

### The ten xUnit warnings, cleared

All ten were the same mistake in two shapes, asserting on a filtered collection rather than the
condition, so the message named the filtered length rather than what it found.

- **Four xUnit2029**, `Assert.Empty(x.Where(p))` to `Assert.DoesNotContain(x, p)`: KpiCreateTests
  681 (no Folder property on OpenModel), CanopyColumnsTests 283 (no skip on the proposed sheet)
  and 308 (no write on it), WorkbookFormulasTests 137 (no formula at risk is an error)
- **Six xUnit2031**, `Assert.Single(x.Where(p))` to `Assert.Single(x, p)`, the filtering overload
  returning the one element typed: CanopyColumnsTests 310 and 348, DiameterColumnTests 186,
  TreeListRowsTests 258, 480 and 481

The assertion changed and never the subject. None changed what it checks: the `DoesNotContain`
overload takes the same predicate, and the `Single` filtering overload returns the same typed
element the chain did, which every call site still assigns and reads. Committed first as a
checkpoint, `54ab011`, so the tree was not left dirty while the workflow ran.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at 1289.

- the block says open a model whether or not one is open, the regression itself: **1 red**, the
  document open and not answered case
- the block says reading even with no document: **1 red**, the no document case, so both halves
  of the distinction are shown load bearing

### Existing tests changed

None. The warnings pass rewrote ten assertions and added none. The plots pass added one Core
test, `ThePlotsBlockReadsFourWaysOneLineEach`, and moved no other.

---

## 2026-09-12, fifty sixth pass. The coarse step is said before any number, and the pane counts the notes

Two lines off Bader's answers to two of the fifty fifth pass's four open questions, and no
behaviour changes beyond them. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing
in this round has been observed in Revit.** The branch came off a fresh pull of main at
`32cbdf1`.

Pull request 85, merged into main as `52e4395`. **The runner executed 1266 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1266 ran at `52e4395`, 0 failed and 0
skipped**, 699 of them KPI. **The branch itself carried 1266, 5 added here**, against the
1261 main held at the branch point `32cbdf1`, and the merged tree is byte for byte the
branch head, nothing landed in between. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### No ceiling on the rounding room, a line instead

Both measured models round areas to 1, so a coarser step is hypothetical and a ceiling chosen
today is a constant pretending to be a rule, the fault the fifty fifth pass's own breaker
caught in that same round. Instead, a project whose step is coarser than the metre opens the
create report with one line before anybody reads a number: THE PROJECT ROUNDS AREAS COARSER
THAN THE METRE: its step is 2, so the group total check allows 1 square metre of room for
every row summed. `KpiCreateRun` carries the `ProjectUnit` whose step gated the checks. The
handler passes it on a fresh read and carries the held run's forward on a reused press,
because the notes were earned against that unit and a step read again since could have
changed. The metre, anything finer and an unread step print nothing, tested each way with the
step of 2, the step of 10 and its 5 in the plural written out by hand.

### The pane says the notes exist

A note only the report file holds is a note nobody reads. `CreateWords.Wrote` counts the
readings' rounding notes and puts one sentence beside the written count, 2 rounding notes are
in the report, singular when one, nothing when none. The detail stays in the report, where
each note sits beside its group total row. FM-21 and FM-22 off the 1208 run are the test's
two notes.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green at
1266.

- the coarse step line never printed: **2 red**, the step of 2 and the step of 10
- the note count never spoken: **1 red**, the status line counting FM-21 and FM-22

### Existing tests changed

None. `CreateFixture.Run` gained an optional area unit it passes through, every existing call
unchanged, and `KpiCreateRun` gained the unit as an optional trailing argument that defaults
to the unread unit, so no caller moved.

---

## 2026-09-12, fifty fifth pass. The room the rounding earns, and a status line that moves

Two things off the 1208 run on RCRC_NG03_EZ, 104,031 elements and 18 plots, the first run on a
second model. The reading reuse held on its first real run, a 123 second scan and a 2.5 second
create, which is finding 34 doing its job. **The other 36 audit findings stay open**, not
renumbered, not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `07f16ba`.

Pull request 81, merged into main as `8578f86`. **The runner executed 1261 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1261 ran at `8578f86`, 0 failed and 0
skipped**, 694 of them KPI. **The branch itself carried 1246, 22 added here**, 1243 before the
breaker round below added three, against the 1224 main held at the branch point `07f16ba`.
The 15 above it are the Drawing Sheet fifty sixth pass that landed in between, `e4b5d9e` and
`c7d91c4` with their record `eaed0e9`, whose files are the only ones that differ from this
branch and none of them KPI, checked file by file. The merge went through the API with the
title and the message both passed on the call, and the commit came back off main carrying
neither a co-author credit line nor a generated-by footer.

### The group total check refuses on rounding no more

It refused FM-21, Existing 2 over 0, Proposed 51 over 11, group total 52 over 11, where 2 plus
51 is 53, and FM-22, 2 over 0, 80 over 46, 83 over 46, where 2 plus 80 is 82. The counts match
exactly, 11 and 11, 46 and 46, the areas are off by one in opposite directions, and the model's
own scan prints Area unit, rounded to 1, so every printed area is already rounded and a sum of
rounded numbers need not equal a rounded sum. The forty seventh pass said exactly that about
the species sum and chose to record rather than enforce, the group total check was enforced
exactly anyway, and it fired on correct data.

The rule now, in `ShrubsAndLawnRows.Disagreeing`: **counts are integers and get no room at
all**, a count that disagrees still refuses whatever the rounding. **Areas get half the unit's
rounding step for each row summed**, so two rows rounded to the metre may be off by up to one.
The step is read off the project units by `KpiReader.AreaUnit` once per press, the same read
the scan prints, threaded through `KpiPlotReader.Read` into `ShrubsAndLawnRows.Read` as a
required argument so no caller can lose it in silence, and never a constant. Within the room
is the `RoundingNote` on the `GroupSubtotal`, printed beside the group total row in the
report with the rows, the total and by how much, so a real fault growing slowly stays
visible. Outside the room still refuses naming the room, and a step that was not read allows
nothing and says so, because a check that cannot see its subject must not quietly widen.
Both measured plots are tests, byte for byte: both go through, both are noted, and the same
numbers with a count one high still refuse.

### Every place printed numbers are added against a printed total, named

- **The shrubs and lawn phase rows against the group total row**: the one that fired, fixed
  above
- **The softscape species rows, taken and left out, against the printed TOTAL row**: integer
  counts, exact, right as it is
- **Each softscape group's species rows against its own subtotal row**: integer counts, exact,
  right as it is
- **`Totalled.Adds`**: the tool's own sum against the tool's own total, both computed from the
  one list, so it is a guard for a future caller rather than a live check. Left alone
  deliberately, and its `Tolerance` constant is shared with the height and diameter difference
  detector in the plan, so widening it would have silently swallowed real disagreements there
- **The NOT WRITTEN line**: integer sums against no printed total, nothing to tolerate
- **The species rows against the shrubs and lawn group's value**: recorded rather than
  enforced by the forty seventh pass, and the review of every summed check found the record
  was MISSING. `GroupSubtotal.SpeciesSum` was computed, its docstring said printed, and
  nothing printed it anywhere, so a drift there was invisible. It prints now beside the group
  on any real difference, recorded rather than enforced, unchanged as a decision. Its first
  shape gated the line on 0.005, a constant pretending to be a rounding room, which the
  breaker caught and the round below cured: the gate is the shared drift epsilon alone and
  the line's two numbers print to six places, so 169.996 against 170 is recorded rather than
  swallowed or printed as 170 against 170

### A status line that moves

One line per piece of work done, never a timer, never an estimate. `ProgressWords` in Core
holds the words and the counting with tests, the handler raises them as the work begins, and
the pane's `Moved` shows them. The scan announces each section off the report's own numbered
headings, Section 2 of 9, project information, then 3, then 4, and the schedules loop counts
every schedule, Sections 5 to 8 of 9, schedules, 400 of 951, 42%, a span because one reader
covers those four sections in one pass and pretending them apart would be a lie. Create names
the plot, Reading DM-44, plot 3 of 18, 11%, with the percentage of plots finished so it never
goes backwards, then the steps name themselves: adding the plots up, copying the template,
writing the cells, reading the written cells back, checking the workbook's own formulas,
writing the report. The patcher raises its four steps itself through a callback, so the words
come from the work. A percentage appears only where the total is known and is floored, 950 of
951 is 99 and 951 of 951 is 100, and the end line still comes through `Told` on every finish,
refusal and throw, so the last thing on screen is never a count that stopped moving.

**Whether the line visibly moves mid run is UNKNOWN until somebody runs it.** The pane can
share Revit's own thread, where a text set from inside the external event sits unpainted until
the run returns. `Moved` queues one empty job at render priority after each line, which lets
the paint through when the threads are one and costs nothing when they are not. No test can
reach it and no mockup can show it, the same class as the black on black panel, so the first
run on a real pane is the check. The job began at background priority and the breaker round
below moved it to render, because waiting on a job pumps everything queued at or above its
priority and input sits between the two: at background every queued click would have been
dispatched inside `Execute`, and two of the pane's buttons open a folder dialog owned by no
window, behind which Revit's own ribbon stays live mid write. At render the paint goes
through and every click stays queued until the run returns. The paint sits at render
priority, so what background let through for it, render lets through the same.

### The breaker read the committed diff and five of its ten findings changed code

Run after the round's first commit, before the pull request went ready. Fixed:

- **A phased group printing no total row was taken as silently as one whose total was checked
  and agreed.** Real shape: a phase heading, its species, one subtotal and no group total row.
  The check has no subject there, which is not the schedule's failure, but nothing anywhere
  said the check never ran, the exact skip the softscape TOTAL already says. The report says
  it now, the group printed no total row after its phase rows, so nothing checked what they
  add to, beside the group, with a test
- **The note's two place numbers could contradict the decision.** On a project rounding to
  0.001, off by 0.0012 within a room of 0.0015 printed as off by 0 within the 0 that rows
  rounded to 0.001 allow, right decision, unreadable sentence. `Said` prints six places now,
  chosen because two swallowed a fine step and twelve printed the last bits of a double on
  summed thousands as digits nobody printed. FM-21 and FM-22 read byte for byte as before
- **The species record was gated on 0.005**, a constant pretending to be a rounding room in
  the same commit that cured the check of one. Gated on the shared drift epsilon now and
  printed to six places, so any real difference is recorded and none prints as equal
- **A reused press said nothing about reuse on the live line.** A 2.5 second finish where the
  last press read for two minutes looks like something skipped until the screen says reuse.
  The reused branch raises Reusing the readings already held, the report's Readings line
  unchanged as the record
- **The background priority pump would have dispatched queued clicks inside `Execute`.**
  Waiting on a job pumps everything at or above its priority, input sits above background,
  and the pane's buttons stay live through a run, two of them opening a folder dialog owned
  by no window. A click queued during a scan would have run its handler mid read, and the
  dialog would have left Revit's ribbon live mid write. The pump runs at render priority now,
  above input, so the paint goes through and every click stays queued until the run returns.
  No test reaches it, net48, recorded here instead

Found and left, each with its reason:

- **The room has no ceiling.** A project rounding areas to 10 hands the check a room of five
  per row summed, and a real fault inside it is a note rather than a refusal. That is what
  half the unit's step for each row summed means on a coarse unit, the rule is measured and
  Bader's, and a ceiling would be a rule nobody gave. Open question in the state file
- **The note lives in the report file and the pane shows the ordinary success line.** A run
  carrying notes looks identical on screen to one carrying none. The round message said a
  line in the report, which is what exists. Whether the pane should count notes beside the
  written cells is an open question in the state file
- **An accuracy read as zero or negative would print as not read.** Nothing in Revit's read
  is known to produce one, `FormatOptions.Accuracy` on the measured models is positive, so
  the branch is dormant and the message imprecise only on a value nobody has seen. Left
- **`GroupSubtotal.Repeats` is computed and printed nowhere**, a shape older than this round.
  Left for a round of its own rather than widened into this one

### Break watches

Seven, each restored byte for byte and checked with cmp, the suite rebuilt and rerun green,
the first four at 1243 before the breaker round, the last three at 1246 after it.

- the rounding room dropped: **3 red**, FM-21, FM-22 and the report line printing the note
- a count given the areas room: **1 red**, the count one high that must still refuse
- the percentage rounded up: **2 red**, the schedule count and the plot line
- the copy step never raised: **1 red**, the patcher's four steps in order
- `Said` back to two places: **1 red**, the fine step note printing its numbers
- the unchecked group line dropped: **1 red**, the phased group with no total row
- the species record gated on 0.005 again: **1 red**, 169.996 against 170 swallowed

### Existing tests changed

None moved of their own accord. `ShrubsAndLawnRows.Read` takes the project's area unit as a
required argument now, so fifteen call sites across seven test files pass `ProjectUnit.Unknown`
by name, which is the read today's fixtures had. The three old disagreement tests still refuse,
their fixtures reading no step, and their sentences gained the clause saying the step was not
read, which their Contains assertions do not pin.

---

## 2026-09-12, fifty fourth pass. The diameter alone decides, and the audits are marked with what is really fixed

Two things off the round message. The message opened saying no behaviour changes in the tool,
and the first item is one, deliberately: a species with a diameter and no height was withheld
and is now written. **Eight findings gained their FIXED mark, six of the nineteen Bader
believed closed are not, and the audits now carry the whole record: 49 findings, 13 FIXED, 36
open, read off the files.** No finding's text changed, none was renumbered, none reordered.
Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`, not
`PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in this round has been observed in
Revit.** The branch came off a fresh pull of main at `9fd1a65`.

Pull request 78, merged into main as `a407284`. **The runner executed 1224 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1224 ran at `a407284`, 0 failed and 0
skipped**, 672 of them KPI. **The branch itself carried 1221, none added net**, one test
replaced one for one, against the 1221 main held at `9fd1a65`. The 3 above it are the Drawing
Sheet round that landed in between, `b7d41dd`, whose files are the only ones that differ from
this branch and none of them KPI, checked. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer.

### The height is not load bearing

Measured by Bader on the MOSQUES template, Tree List - Proposed row 21:

```
I21  15                          a plain value, nothing reads it
J21  8                           read by L
K21  10                          nothing reads it
L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))
M21  =IF(ISBLANK(B21)," ",L21*B21)
N21  60                          a typed value the tool never writes
O21  =IF(ISBLANK(B21)," ",N21*B21)
```

Only the diameter feeds a formula, so requiring both measures withheld a row for a cell
nothing reads. That came from the round message's wording, the fifty third pass recorded it as
exactly that, and this is the answer. `SpeciesMatching.WrittenInto` asks the canopy diameter
alone before it takes a row. A species with a diameter and no height is written, its name,
count and diameter into their cells, and its height cell is named as not written through
`Measured`, the same skip every other measure cell uses, so the report says it the way it says
the rest. A species with no usable diameter is still withheld and still named with its count.
UNKNOWN is unaffected either way: DM-25 row 19 prints 0 for its diameter, which is no size.
`NotSized` quotes the diameter rows alone now, and every test pinning the both measure wording
moved with it. The test that withheld a species missing one measure inverted, one for one:
`ASpeciesWithADiameterAndNoHeightIsWrittenAndItsHeightCellIsNamed` proves the row is written,
B5, D5 and J5, with I5 named and the height reason quoted.

### The audits are the record, verified at today's lines

Each of the nineteen findings Bader believed closed was checked against the code as it stands,
by searching for the subject rather than trusting the audit's own line numbers, and against
`steps/log-kpi.md` for the pass that closed it. Thirteen verified and are marked, the five
that already were and eight marked now:

- **1**, the template overwrite guard, and **8**, the empty status line: the fortieth pass,
  pull request 52
- **2**, the cell position fallbacks, **30**, the thousands separator, **31**, the area read
  on STREETS, and **32**, the softscape TOTAL row: the forty fifth pass, pull request 58
- **5**, the tree list row range, and **36**, two schedules of one kind: the forty sixth
  pass, pull request 59

**Six of the nineteen are not closed: 3, 4, 6, 12, 21 and 26.** Each was found byte for byte
in the shape its finding describes, at today's lines: `Preselect` still returns past its own
guard, `KpiNames.Component` is still the capitals no model holds, `Printed` still swallows an
`ApplicationException` with nothing recorded, `Ask` still lets Scan, Plots and Create
overwrite each other, the Written as box still goes stale after a tick, and
`CreateWords.NoTemplate` still cannot reach the screen. They are the round the log has twice
recorded as never landing on main, and they stay unmarked and open.

**One thing the verification surfaced beyond the count.** `PaneChoicesTests` pins finding 4's
unfixed behaviour as if it were the rule: `TheNameIsMatchedWholeAndWithItsCase` asserts the
ordinal compare against the capitals constant and `AWantedNameTheModelDoesNotOfferFallsBackToTheFirst`
asserts the silent fall through, on an offered list ordered so the fallback looks safe. When
the round closing 4 lands, those tests have to move with it or they will hold the fault in
place. Recorded here for that round, whichever session runs it.

The bookkeeping rule that follows, and it is the repo's own: **the audit files are the one
record of what is open.** The state file now carries the count read off them, 49 findings and
13 FIXED so 36 open, and no running count anywhere else. Bader's fourteen was one off the
verified thirteen, which is what a second record drifting looks like in the bookkeeping.

### Break watches

Two, each restored byte for byte and checked with cmp, the suite rerun green at 1221 after a
rebuild, because the first rerun after the second restore tested the still patched binary and
read 9 failed, which is the stale build rule in `CLAUDE.md` caught in the act.

- the gate back to requiring both measures: **1 red**, the inverted test
- the diameter gate removed: **9 red**, every withheld case across four files, UNKNOWN's among
  them

### Existing tests changed

The both measure wording moved to the diameter wording in `SpeciesMatchingTests`,
`CanopyColumnsTests`, `TreeListRowsTests` and `DiameterColumnTests`, and
`ASpeciesMissingOneOfTheTwoMeasuresIsNotWrittenEither` became the inverted
`ASpeciesWithADiameterAndNoHeightIsWrittenAndItsHeightCellIsNamed`. Nothing else moved.

---

## 2026-09-11, fifty third pass. A species the model does not size gets no row at all

One rule, measured on the 1836 run over 20 mosque plots. **The other audit findings stay open**,
not renumbered, not reordered, not annotated, and this round closes none of them: it is a fault
off a run rather than an audit entry. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `c47bccb`.

Pull request 75, merged into main as `be72119`. **The runner executed 1198 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1198 ran at `be72119`, 0 failed and 0
skipped**, 672 of them KPI. **The branch itself carried 1187**, 5 added here against the 1182
main held at `c47bccb`. The other 11 are the Drawing Sheet round that landed on main while this
one was being written, `f72834e`, and the merged head is the first place the two counts meet.
The only files that differ between this branch and main are that round's, none of them KPI,
checked. The merge went through the API with the title and the message both passed on the call,
and the commit came back off main carrying neither a co-author credit line nor a generated-by
footer.

**A count I could not reconcile.** The round message says the other 32 audit findings stay
open. At this branch the two audit files hold 49 numbered findings and 5 FIXED marks, 9 and 16
in `steps/audit-kpi.md` and 34, 35 and 39 in `steps/audit-kpi-2.md`, so 44 stand as marked here.
The round that was to close 3, 4, 6, 12, 21 and 26 has still not landed on main. Nothing was
renumbered or annotated either way, and the difference is written down rather than resolved.

### What happened, and what the fix is

34 cells were ready. The run wrote the output, checked it, found 7 formulas that would read an
error and deleted it. Every one traced to one row: UNKNOWN written into Tree List - Proposed row
85 with a name and a count and no diameter, because DM-25 row 19 prints nothing for its height
and 0 for its canopy diameter. `PrintedMeasure.Held` already means a number greater than nought,
so a nought was never a size, and the row went in without one.

**The guard is right and the rule it caught was wrong.** Writing a name and a count into an
empty row cannot work for a species the model does not size. `SpeciesMatching.WrittenInto` now
asks the height and the canopy diameter before it takes a row, and a species missing either
gets no row, is named with its count and its reason, and the run goes through. The canopy guard
is untouched, byte for byte: it caught this, which is why it exists.

The reason names what is missing and what every row printed, so a person can see it without
opening the model:

```
the workbook's list does not hold this name, and a row written into an empty one carries only
what the model prints, which is no height and no canopy diameter a workbook can compute with,
so no row was written: DM-25 row 19 prints '-', DM-25 row 19 prints 0, which is no size
```

Three decisions inside it, none of them settled by the code:

- **The size is asked before a row is taken**, so a refused species leaves the empty row for
  the next one. A test proves it, and it only proves it because the merge orders by name and
  the unsized species is reached first. The first version of that test put the unsized species
  second, so the broken code passed it. **It went green on a watch that should have turned it
  red**, which is how it was caught
- **The sheet's own total is asked first.** A sheet with no total writes nothing for anybody
  and that is the larger fact, so it is still said first for a species that is also unsized
- **Both measures are required**, which is the round message's wording. Only the diameter is
  known to break a formula: `L` reads `J` through `ISBLANK` and nothing measured says a row with
  a diameter and no height is safe. Requiring both withholds a row from a species the model
  half sizes, its count is named in the report, and loosening it to the diameter alone is
  Bader's call

### One line added to the report

Under SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT:

```
  NOT WRITTEN, THE WHOLE RUN: 1 tree of 528, over every species this run merged.
```

Both numbers come off the one list of matches the section above prints from, so nothing counts
the trees a second way. It covers the whole run rather than that section, because a tree that
went nowhere for any reason is a tree the workbook does not hold, and the line says so.

### What the 12 red tests turned out to be

The rule turned 12 existing tests red, which is what a rule change should do. Four readers, one
per file, said which were fixtures and which were expectations. Two kinds:

- **Nine were fixtures that predate measures.** They build a species with no height and no
  diameter because they are about matching a name, taking rows in order, or room on a sheet.
  `CreateFixture.Species` gives the ordinary case a size now, 15 and 8, and `CreateFixture.Merged`
  builds a sized species off a row rather than off a plot number
- **Three were the old rule written down**, all about UNKNOWN, and their expectations changed.
  `PhoenixWritesEighteenAndFifteenAndUnknownWritesNeitherAndStillRefuses` asserted the workbook
  being deleted by the guard. It asserts the opposite now and is the round in one test:
  PHOENIX goes in whole, UNKNOWN takes no row, and **the workbook is written**

**The blanket fixture change was itself a fault, and the review caught it.** Giving every three
argument fixture species a size made UNKNOWN carry 15 and 8, and UNKNOWN is the one species
measured to print neither. Every test that names UNKNOWN now says what the model says, a dash
and a 0, and the tests that only needed a species the list does not hold use one that is
measured to have a size. Where UNKNOWN was left in a test about room, its reason changed from
the sheet ran out of room to the model does not size it. Both are true on that sheet and the
size one is said first, which is what makes the two UNKNOWN rows of the 1836 run read the same
way afterwards.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rerun green at 1187.

- the size never asked, so an unsized species takes a row: **10 red**, across
  `SpeciesMatchingTests`, `CanopyColumnsTests`, `TreeListRowsTests` and `DiameterColumnTests`
- the report not saying how many trees went nowhere: **1 red**, the canopy report test
- the refused species taking its row before the check: **1 red**, the ordering test, and only
  after that test was corrected. It passed the same watch before, which is what showed it was
  not testing what it said

### Existing tests changed

`CreateFixture.Species` gained a size and `CreateFixture.Merged` is new. `SpeciesMatchingTests`
and `TreeListRowsTests` moved to them, three tests were renamed for what they now assert, and
`DiameterColumnTests` and `CanopyColumnsTests` each had one expectation rewritten. 5 tests added
net, 1182 to 1187.

### Open, and for Bader

- **Whether both measures should be required or the diameter alone.** Only the diameter is known
  to break a formula. A species the model gives a diameter and no height is withheld today
- **What a withheld count costs.** A tree not written is a tree the workbook's totals do not
  hold. The report says how many, and the answer to the species themselves is in the model

---

## 2026-09-11, fifty second pass. A workbook is filled when a cell the tool writes holds what the tool writes

One correction, on finding 39's filled file test, measured by Bader on two template sets. The
audit entry for 39 carries it under its FIXED mark. **The other 38 findings stay open**, not
renumbered, not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `d370add`.

Pull request 73, merged into main as `ba805b1`. **The runner executed 1182 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1182 ran at `ba805b1`, 0 failed and 0
skipped**, 667 of them KPI, 29 added here, against the 1153 main carried at `d370add`. The
merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.
Its tree is the branch's tree, checked.

### What was measured, and what the old rule really did

Two sets, Bader's measurement:

```
KPI CHECKLIST - R1 MOSQUES   E5 <Date>   G5 <Name>   H5 <Position>   C5 <UID>
an earlier production set    E5 empty    G5 empty    C5 empty
                             D3 Future Park   E4 KING ABDULLAH South   H5 a real value
```

So E5 is a placeholder in one set and empty in the other, a placeholder is one set's habit
rather than a rule, and a clean template holds real text in a mapped cell.

**One thing in the round message does not hold against the code, and it is worth saying.** A
clean template from the second set was NOT withheld by the old rule: `ReadsAsFilled` required
the cell to be non blank before it compared, so an empty E5 read as a template. The old rule
was still wrong, for the reason given rather than that consequence. What it really withheld was
any template whose E5 holds text that is not exactly `<Date>`, which is the annotated set's
DATE OF THE DAY and any set the client issues with different wording there. The correction is
the same either way and is the one asked for.

### The rule now

`FilledMarks` is a cell the tool writes, what the tool writes there, and the test of whether
what the cell holds is that. A workbook is filled when one of them reads, and **nothing is
withheld on a cell the tool has never written**, so an absent or empty cell is never a filled
file. Two marks, both cells the tool writes:

- **E5, a date.** It parses as one AND holds at least two numbers. The second half is not
  decoration: the invariant culture reads a bare 10 as a day of this month, and a template
  holding a lone number where a date goes would have been withheld for it. `<Date>`, Date,
  DATE OF THE DAY, March, TBC and empty are all templates. 2026-09-10, 09/09/2026,
  9 September 2026 and 2026-09-09 14:07 are all filled
- **The template's own reference cell, a plot reference.** C5 on all seven, read off
  `KpiTemplate.CellFor` rather than a second copy of the map. It is one unbroken run holding a
  letter and a digit and no angle bracket, which is what both parameters the team picks read
  as, DM-12 and ANH-007-MO-100019. `<UID>`, KING ABDULLAH South, Future Park, TBC, 100019,
  ANH 007 MO 100019 and empty are all templates

The other cells the tool writes decide nothing. D3 and E4 hold real text in a clean template,
G5 and H5 hold a name and a position that no shape tells from a placeholder, and a number cell
says nothing about who put the number there.

**The cell that decided is said in the reason and in the report.** The reason reads It is a
filled MOSQUES checklist, not a template. C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes. The report prints one line per withheld workbook under the templates folder
line, not offered: MOSQUES DM-12.xlsx, C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes, so a template wrongly withheld is traced in one line rather than by opening
the file. The date is read first, so a workbook holding both names one cell rather than two.

**`KpiTemplates.DatePlaceholder` is deleted.** It decided nothing any more, and a constant
holding a measurement that no longer decides is a second record of a fact. The measurement is
in `FilledMarks` next to the rule it explains, with both sets.

**The peek reads the marks' cells rather than one.** `PeekedWorkbook.FirstSheetCells` is a
dictionary of the cells `FilledMarks.CellsRead` names, E5 and C5, read off the first sheet in
one open with one pass over the shared strings. A cell not in the file is not in the dictionary.

### Two limits the review of this round found, both stated and neither guarded against

**A filled workbook the tool wrote neither cell into reads as a template.** `KpiCreatePlan`
skips the reference cell when the ticked plots disagree on it or none of them holds it, and a
checklist covering several plots usually disagrees, so on such a run the typed date is the only
mark left. `CannotCreate` does not ask for a date, and the box is prefilled with today, so it
takes clearing it by hand. It is the safe way round of the two: offering a filled file costs a
rerun, and withholding a real template leaves the team unable to fill anything at all.

**A client set that hinted the shape of a reference rather than bracketing it would be
withheld.** DM-00 or REF-0001 at C5 holds a letter and a digit and no space, so nothing
separates it from ANH-007-MO-100019. Neither measured set does that: the R1 set brackets every
placeholder and the earlier set leaves the cell empty. A theory in `FilledWorkbookTests` says
what the rule does with one today rather than that it is right, and the report prints C5 holds
DM-00, so a person sees it in one line rather than by opening the file. **For Bader**, with the
other open questions.

### Three things the review changed

**The sheet part is parsed once per file rather than once per mark.** The reader reopened the
zip entry and parsed the whole sheet again for each cell, so going from one mark to two doubled
it and every mark added later would have cost another pass. `WorkbookPackage.CellTexts` walks
the part once and stops when it has them all.

**The pane's line about the three typed cells said never written**, in a list whose other lines
name where a value is read from, and the tool does write them, which is the very thing
`FilledMarks.Date` relies on to tell a filled file. It reads typed by the team on this pane and
copied through, from no model. Its test moved with it.

**A theory pins what a shape-alike hint does today**, so the second limit above is in a test
rather than only in prose.

### Break watches

Five, each restored byte for byte and checked with cmp, the suite rerun green at 1182.

- the date mark back to anything but the placeholder: **5 red**, every row of the theory that
  is not a date, DATE OF THE DAY and the bare 10 among them
- the reference mark taking any text: **8 red**, every row of the reference theory, the R1 set
  and the park that still needs a pick
- the report not naming the cell that decided: **1 red**, the report test
- the peek reading the date cell only: **1 red**, the patcher round trip through both cells
- the one pass reader stopping at the first cell it finds: **1 red**, the same round trip

### Existing tests changed

`FilledWorkbookTests` is rewritten around the new rule, 9 tests before and 20 now, both
measured sets among them. `TemplateWordsTests` moved one expected line with the pane text above.
Nothing else moved: `Recognise`'s fourth argument is still optional, so every earlier call
proves the sheet rule unchanged.

---

## 2026-09-11, fifty first pass. The five that cost a whole run: findings 35, 34, 39, 9 and 16

Five findings from the two audits, all about the 78 plot STREETS run the team is about to try,
about twenty minutes on the measured rate. Each either wasted it, threw it away, or made it
worse. **Findings 34, 35, 39, 9 and 16 are FIXED and marked under their entries in
`steps/audit-kpi.md` and `steps/audit-kpi-2.md`. The other 38 stay open**, not renumbered,
not reordered, not annotated. Nothing else was touched: not the Drawing Sheet, not
`Core/Shared`, not `CLAUDE.md`, not `PanelTheme`, `PanelMetrics` or `ReportFile`. **Nothing in
this round has been observed in Revit.** The branch came off a fresh pull of main at `2e0c0ab`.

Pull request 72, merged into main as `adf164f`. **The runner executed 1153 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1153 ran at `adf164f`, 0 failed and 0
skipped**, 638 of them KPI, 50 added here, against the 1103 main carried at `2e0c0ab`. The
merge went through the API with the title and the message both passed on the call, and the
commit came back off main carrying neither a co-author credit line nor a generated-by footer.
Its tree is the branch's tree plus the two Drawing Sheet commits that landed on main between
the branch point and the merge, `fc5cf7a` and `2aa529b`, checked: the only files that differ
between the branch and main are the three of theirs, and no KPI commit landed.

### Each of the five at today's lines

The audits' line numbers were written at `92dd36c` and have moved. Read again at today's lines
before anything was fixed, every one of the five still stood:

- **35** stood at `KpiRequestHandler.cs:267-291`, the loop over the ticked plots with
  `RegionsFor` at :270 and `Read` at :288 under no try of their own, falling to the catches at
  :160-179. The two reconciliation refusals at `Reconciliation.cs:212-232` were reached by
  `ReconciliationTests` alone
- **34** stood at `KpiPanel.cs:760-764`, the region choice click calling `AskedToCreate`, and
  :706, the confirm redrawing so Create is pressed again. The choice was applied at read time,
  `KpiRequestHandler.cs:277-290`, and nothing of the last run travelled on the ask
- **39** stood at `KpiPanel.cs:353-360` with `RecognisedWorkbook.cs:102-116`, the first sheet's
  name and nothing else, and the peek read no cell
- **9** stood at `KpiPanel.cs:562-569`, a bare `ScrollViewer` built on every redraw, with no
  scroll handling anywhere on the KPI side
- **16** stood at `KpiPanel.cs:353-360`, `PeekedWorkbook.Of` per path on every redraw, byte for
  byte what `92dd36c` held

**The round before this one, findings 3, 4, 6, 12, 21 and 26, has not landed.** Main held
nothing after `2e0c0ab` and no Kpi commit names any of the six, so finding 35 is built here
and not on finding 6. The catch inside `KpiPlotReader.Printed` that swallows an
`ApplicationException` with nothing recorded is finding 6's and is untouched: a throw of that
one type inside `Printed` still comes back as a schedule with no rows and no refusal, and that
round is where it is answered.

### 35. A guard per plot, and a guard per schedule

`PlotReading.NotRead(plotId, why, seconds)` is a reading whose one refusal is the read threw
and nothing on this plot was read, with the exception's type and message, every other field at
its empty default. `GuardedRead` in `KpiRequestHandler` wraps each plot's regions and read and
hands that back on a throw, catching every type on purpose with the same sentence the scan
side's `Guarded` carries. Inside `KpiPlotReader.Read` each schedule's read is guarded the same
way, so a throw reading one schedule names the schedule and leaves the rest of the plot read.
Nothing downstream changed: `Reconciliation` already turns the refusal into DM-60: the read
threw and nothing on this plot was read. InvalidOperationException: The schedule is not valid.
and refuses the write, lists the plot under contributed nothing with the refusal first, the
report prints it among the reasons and under the plot as REFUSED, the status line counts it,
and the pane draws it in red. Three ticked, three read, one refused: the other two still add
up around it, 1000 plus 250 is 1250 and 13 plus 13 is 26, and the plot's empty component does
not make the others disagree. The refusals for a plot list that comes out shorter or longer
than it went in stay as the backstop tests reach, by design now.

The guard itself runs only in Revit and has never fired on a live model. Which exception type
a real refused plot throws is UNKNOWN, so the message shape is the only thing the tests pin.

### 34. The choice applied to the run already read

`KpiCreateAsk` carries the run the pane holds, the same object and not a copy, and
`HeldReadings.Decide` in Core says whether it can answer this press. It is not a cache with a
lifetime: the run before is trusted when it wrote nothing and the model title, the template and
its file, the two parameters and the plots are the same, ordered the way `Reconciliation`
orders them, and every ticked plot has a reading. Anything else reads the model again with the
reason named, and each is a test: no run held, the run before wrote its workbook, the model or
the template or the template file or either parameter or the plots differ, a plot has no
reading. `HeldReadings.Applied` puts each plot's chosen region on its reading through
`PlotReading.WithChosenRegion`, and a plot with no choice keeps the reading it had, the single
region a first run settled on included. The reconciliation that refused NS-19 on two regions
passes on the applied readings with the area 900 and nothing read, and the identical areas
confirm passes the same way with the pair still named.

The report says which under the Run line: Readings: reused, nothing was read from the model on
this press, or read from the model on this press, with the reason. A reused run reads 0.0
seconds of model read, and that line is what makes the number true. The confirm button still
redraws and Create is pressed again, which now reuses, so the two presses cost one read where
they cost two. Making the confirm one press is an interface change Bader has not asked for.

**A plot whose read was refused is not reused.** The review of this round found that a reading
made by `NotRead` carries the plot's identifier and so passed the every plot has a reading
check, and a press after the model was fixed, with nothing else changed, would have handed the
same refusal back for ever. `Decide` reads again when any held reading carries a read refusal,
naming the plots: the run before could not read DM-60, so the model was read again. That costs
a full read on the press after a thrown plot, and reading only the refused plots is the
refinement not built.

**What no cheap comparison can see is a model edited between the refusal and the pick.** A
region's area typed in or a schedule's filter changed in those minutes would be read off the
run before. The pane throws the run away on every tick and every picker change, the report
says reused in so many words, and whether that is acceptable is for Bader.

### 39. A filled checklist named as filled

The peek reads E5 of the first sheet in the same open as the names, off the part the workbook's
relationships point at, with a shared string resolved to its text because an Excel re-save
stores a filled cell that way. `Recognise` takes it as a fourth argument and names a file whose
E5 is not `KpiTemplates.DatePlaceholder` as a filled checklist of its template, with what E5
holds in the reason: It is a filled MOSQUES checklist, not a template. E5 holds 2026-09-10 where
a template holds <Date>. It sits in the same list greyed, its reason in the tooltip, and
nothing is deleted or moved. A cell that is not there or empty is not a filled file. A filled
park file is not offered and needs no pick. A read refusal still wins. The two private cell
readers in `SpeciesList` moved into the package plumbing the peek and the patcher share, so
there is one cell reader rather than three.

A filled park file whose name names neither park, or both, is named as a filled EXISTING PARKS
or FUTURE PARKS checklist which its file name cannot tell apart, `FilledPark`, with `FilledAs`
null. The first draft named EXISTING PARKS for it, the first of the two in the list, which was
a guess printed as a fact, and the review caught it. Nothing reads `FilledAs` but the tests.

**The placeholder is one observation.** `<Date>` is what E5 held on the first real workbook on
2026-09-09, recorded until now only in a comment. Whether all seven production templates hold
exactly that is UNKNOWN and is for Bader to open and confirm before the team's folder holds a
filled file. A template holding something else there would be withheld, with its E5 printed on
its own row so the withholding is visible rather than a file that vanished. The annotated set
reads DATE OF THE DAY there and is not what the team fills.

### 9 and 16. One press, one design

Both are the same press: a tick calls `Ticked`, `Changed`, `RedrawTemplates`, and the redraw
both reopened the workbooks and rebuilt the plot list bare. `Changed` and `RedrawTemplates`
stay the one path in and out of every change.

`TemplateListing` in Core holds the folder's workbooks as recognised, keyed on the folder
compared without case and with a trailing separator off, and `For(folder, paths, recognise)`
opens only the paths it has not seen, drops the ones gone and keeps the rest. The folder is
still listed on every draw. It counts the opens and the redraws: seven files over 155 draws is
seven opens where it was 1,085, and a fake counts them in the tests so no disk is touched. The
pane holds it as the one copy under THE PANE HOLDS NO COPY OF ANYTHING IT CAN ASK FOR, says so
in its comment, clears it when the folder is browsed and after a run that wrote, prints the
count under the list, and hands it on the ask so the report prints templates folder: 7
workbooks in the folder, opened 7 times over 158 redraws since the folder was listed, or NOT
LISTED when nothing recorded it. A workbook overwritten in place under the same name by
anything other than this tool keeps its held recognition until the folder is browsed again or
Create reaches the patcher into it, which is the stated limit. The clear after a press of Create
is keyed on the output path's folder, `IsFor`, so filling several plots into another folder
leaves the listing and its counts standing, and it fires on any press that reached the patcher
into the listed folder, wrote or not: the copy lands at the output path before the patch, and a
patch that fails after it leaves the copy behind under a name the listing may already hold as
something else. The review found both, the clear on every write anywhere and the clear missing
on a refused patch. It also asked whether a write refused after its copy leaves a stray file
unseen under a new name: it does not, because the folder is listed on every draw and a new path
is opened once.

`Scrolling` and `Remembering` in `KpiPanel` are the Drawing Sheet's shape written in the KPI
folder, read and not edited at `DrawingSheetPanel.cs:1653-1702`: the remembered offsets read
into locals before any handler is attached, restored on the first layout pass and noted on
every scroll change. `ScrollMemory` in Core holds the rule half with tests, a restore wanted
only when something above the top was noted, because restoring nought is a scroll to the top
that reads as though it worked. A tick still redraws the whole block, because a tick moves the
count line, the preselection, the reference values and the Create button, and remembering the
offset is the minimal change and the one the Drawing Sheet chose. Whether the restore holds on
a real run is UNKNOWN for both panes: the Drawing Sheet's own log records no observation of it
either. **Lifting the helper into a shared file is a round of its own**: it cannot live in
Core, which has no WPF, so it would be a new common file at the Revit root and a hook change,
and that is Bader's call.

### What inside the read costs, in calls, and unchanged

Measured on the code and not in seconds, because nothing here runs Revit. Per ticked plot:

- `FirstSheetOf` collects every sheet, reads `SheetNumber` off all 1,385 to sort them, then
  `GetParameters` and the getter down the sorted list until the plot matches
- `Schedules` collects every schedule reading `IsTemplate` off each, and for every one of the
  951 that is not a template `PlotFilteredOn` asks `Definition`, `GetFilters`, and per filter
  `GetField`, `GetName`, `IsStringValue` and `GetStringValue` until the plot filter is found.
  951 per plot, 74,178 over 78
- `RegionsFor`, on a template with an area cell only, so not on STREETS: one collector over the
  link instances, and per 00 link one collector over its filled regions with `GetParameters`
  and the getter for PRX_Ref Plot ID on every one of the 279, and the area, the type name and
  the printed form on the regions matching the plot alone. The forty sixth pass entry said two
  parameters off each region, which overstated it
- `Printed`, on at most two schedules: `GetTableData`, the body section, the counts, and
  `GetCellText` once per cell. Nothing regenerates, refreshes or exports a schedule

Which of the four carries the 125.5 seconds is UNKNOWN, and none is changed. The 126.6 second
measure in the round message is Bader's and is recorded nowhere in this repository.

### What the review found and what stayed

Four readers over the diff, one lens each, then a pass trying to refute what they found,
eleven findings, four confirmed and seven refuted. Three changed code, above: the refused
reading reused, the park named on a guess, and the templates listing cleared on every write
anywhere and not on a refused patch into its folder. The rest stayed, each for a reason:

- The region read sits inside the plot's guard rather than under one of its own, so a throw
  reading a plot's filled regions refuses the whole plot, named, and its schedules are not read.
  Finding 35 asked for a guard per plot and that is what it got. STREETS reads no region
- A template whose own placeholder is not `<Date>` is named as filled, with its E5 printed. That
  is the open question for Bader above, and the line shows what it read rather than hiding it
- `CellRef.TryParse` reads upper case column letters only, which is what Excel writes. A sheet
  re-saved with lower case references would read as empty rather than refuse. Not this round's
  code and not touched
- `PeekedWorkbook.Of` catches four types, and an unlisted one thrown inside a redraw would take
  the template list down with it. The same four it caught before this round. Not touched
- A templates folder that fails to list reads as empty, `WorkbooksIn`'s shape before this round,
  and now costs one re-open per file when it comes back rather than seven per tick. Not touched

### Break watches

Eight, each restored byte for byte and checked with cmp, the suite rerun green at 1153.

- 35, a plot that threw carrying no refusal: **4 red**, all of `PlotReadThrewTests` bar the
  merge test
- 34, a choice not applied to the held reading: **2 red**, the applied and the reconciliation
  tests in `HeldReadingsTests`
- 34, the plots ticked not compared: **2 red**, the two ticked rows of the theory
- 39, nothing reading as filled: **2 red**, the filled mosque and the filled park
- 16, every draw opening every workbook again: **6 red**, across `TemplateListingTests`, the
  155 draws test reading 1,085
- 9, nought restored: **2 red**, both `ScrollMemoryTests` that say nought is not worth restoring
- 34, a refused reading reused: **2 red**, the thrown plot and the two schedule refusals in
  `HeldReadingsTests`
- 39, an untellable filled park guessed as EXISTING PARKS: **2 red**, both rows of the theory
  in `FilledWorkbookTests`

### Existing tests changed

None moved. `CreateFixture.Run` gained three optional arguments, the readings source, the
templates listing and the ticked list. `WorkbookFixture` gained a workbook whose one cell is a
shared string. `Recognise` gained an optional fourth argument, so every earlier call proves the
sheet rule unchanged.

---

## 2026-09-10, fiftieth pass. One word too loose: two headings hold DIAMETER, and the formulas say which

One column choice, measured on the 1707 run, and one line added to the report. **The other 43
audit findings stay open**, not renumbered, not reordered, not annotated. Nothing else was
touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round
has been observed in Revit**, and no workbook was opened in Excel.

Pull request 69, merged into main as `9e966fa`. **The runner executed 1103 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1103 ran at `9e966fa`, 0 failed and 0
skipped**, 588 of them KPI, 8 added here. The merge went through the API with the title and the
message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### What happened on the 1707 run

The run wrote the output, checked it, found 8 formulas that would read an error and deleted
it. Correct at every step. The report said: not written: the sheet's header row, row 3, names
2 columns holding DIAMETER, J, K, so nothing says which. The tree list header row holds I
Mature Height (m), J Average Mature Canopy Diameter (m) and K Mature Canopy Diameter (m). Both
J and K hold DIAMETER, so the reader found two, refused to choose and wrote nothing into J.
L84 is IF(ISBLANK(J84), " ", ROUND(PI()*(J84/2)^2, 0)), so it returned a space, M84 did
arithmetic on the space and went #VALUE!, and the canopy guard deleted the output.

### The fix: prefer the column the workbook's own formulas read

J is the column the workbook computes from. Measured: L reads J and nothing reads K.
`SpeciesList.In` reads every formula below the header row off the sheet part, the master cell
of a shared formula carrying the text, collects the columns those formulas reference off the
formula text with a cell reference pattern that leaves a function name such as LOG10 alone, and
hands the set to `ColumnHeaded`. One heading holding the word is taken as before. More than
one, and the one candidate the formulas read is taken. Where that still leaves more than one,
or none, nothing is written and both are named, with what the formulas read added to the
reason: names 2 columns holding DIAMETER, J, K, and its own formulas read none of them, so
nothing says which. Never a position. It is worked out from the file and from no letter.

How each column was chosen is recorded as it is used, `HeightColumnChosen` and
`DiameterColumnChosen` on the list, the one column of the header row holding HEIGHT, or of J, K
holding DIAMETER, the one the sheet's own formulas read, and the report prints both beside each
tree list under THE WORKBOOK'S OWN TREE LISTS, or none with the reason.

**The check asked for.** On the two column sheet PHOENIX DACTYLIFERA writes 18 into I and 15
into J, on the fixture's row 5 where the real sheet's is row 84, and UNKNOWN writes neither,
because DM-25 row 19 prints nothing for its height and 0 for its diameter. UNKNOWN's blank J
still refuses the output through the canopy guard, L6 named as reading a blank J6, and the row
Phoenix landed on computes. The two column header, a one column header, a header naming none,
the formulas reading neither and the formulas reading both are each a test, every expected
value written out by hand.

### Said in the report and not fixed

**22 matched species have a height or a diameter in Revit that differs from the row the
workbook already holds, nearly every match.** Both numbers stay named and nothing is changed,
and one line under that heading now says how many of the matches differ in a height, a
diameter or both, so the size of it is visible without counting: 2 of 3 matched species on the
fixture, 0 of 0 with no match. Which number is right is still the question for the team the
forty seventh pass raised.

### Break watches

Three, each restored byte for byte and checked with cmp, the suite rerun green at 1103.

- the tie break dropped, so two columns refuse as before: **3 red**, the two column test, the
  Phoenix and UNKNOWN check and the report line in `DiameterColumnTests`
- the first column taken when the formulas do not decide, which is a position: **2 red**, the
  formulas reading neither and the formulas reading both
- the count line dropped from the report: **2 red**, both of `MeasureDifferenceCountTests`

### Existing tests changed

The tree lists report test in `TreeListRowsTests` gained the two column lines, none with the
reason, because its fixture's header names only the botanical column. The `Computing` fixture
takes a second diameter heading, the column the canopy formula reads and a second formula
column, so the three shapes can be built. No expected value moved.

---

## 2026-09-10, after the forty ninth pass. ST-05's Existing group is the thirteen measured species

One fixture correction and nothing else. The forty ninth pass built ST-05's Existing group
from two names and an assumed split of 300 and 69, because the note gave the subtotal alone.
The thirteen species are measured off the schedule on screen now and the fixture holds them:
ACACIA / VACHELLIA FARNESIANA 25, AZADIRACHTA INDICA 7, CASSIA GLAUCA 1, CONOCARPUS ERECTUS 76,
CONOCARPUS LANCIFOLIUS 129, FICUS BENJAMINA 13, HIBISCUS TILIACEUS 4, MORINGA OLEIFERA 3,
PHOENIX DACTYLIFERA 18, PROSOPIS JULIFLORA 16, UNKNOWN 20, WASHINGTONIA ROBUSTA 48 and
ZIZIPHUS SPINA-CHRISTI 9. They add to 369, which is the subtotal the schedule prints, and 369
plus 70 is the 439 of the TOTAL row. **Every number in that fixture is measured rather than
assumed.**

CASSIA GLAUCA sits under Existing at 1 and under Street Design at 62, so the fixture now covers
one species in a named group and a by-decision group at once: two merged rows, 1 on Tree List -
Existing and 62 on Tree List - Proposed, two groups and not one species printed twice under
one, so the same group refusal does not trip. The tests say so in those numbers, and the row
numbers moved with the rows: sixteen species rows, the Existing subtotal on row 17, Street
Design on row 21 with its subtotal on 24, TOTAL on row 25. No code changed, so there is no
watch to break. **Nothing in this round has been observed in Revit.**

Pull request 68, merged into main as `5d062f6`. **The runner executed 1095 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1095 ran at `5d062f6`, 0 failed and 0
skipped**, 580 of them KPI, none added. The merge went through the API with the title and the
message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

---

## 2026-09-10, forty ninth pass. Street Design counts as Proposed on STREETS, and the note goes on the pane

Two things on top of the forty eighth pass, both Bader's decisions. **The other 43 audit
findings stay open**, not renumbered, not reordered, not annotated. Nothing else was touched:
not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round has been
observed in Revit**, and no workbook was written or opened. `CountedGroups` keying off the tree
list sheet names is the design and stays. These sit on top of it.

Pull request 67, merged into main as `c79c4d2`. **The runner executed 1095 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1095 ran at `c79c4d2`, 0 failed and 0
skipped**, 580 of them KPI, 14 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### 1. Street Design counts as Proposed, on the STREETS template only

**Bader's decision: Street Design counts as Proposed on STREETS and stays out everywhere
else.** No template has a Tree List - Street Design sheet, so the forty eighth pass left the
group out on every template, streets included, and on a street plot that group is the plot's
own work. Measured on ST-05, a street plot, off its softscape schedule on screen: Existing 369,
Proposed 2 which is ALBIZIA LEBBECK 2, Street Design 68 which is ALBIZIA LEBBECK 6 and CASSIA
GLAUCA 62, TOTAL 439. Its proposed trees are 2 plus 68, 70, its existing are 369, and 369 plus
70 is 439, the TOTAL the schedule prints. That is the test, and every expected value is
written out by hand from it. The two species under Existing and their split of the 369 were
not in the note, so the fixture uses two names earlier runs measured and a split of 300 and 69.

**It is data on the template and keyed on the template, never on the plot prefix.**
`KpiTemplate.GroupsCountedAsProposed` holds Street Design on STREETS and nothing on the other
six, one constant, `KpiTemplates.StreetDesignGroup`. `CountedGroups.Of` reads it into a
`GroupByDecision` pointing at the template's Proposed sheet, `Counts` answers true for it,
`SheetFor` answers Tree List - Proposed, and `Why` reads Tree List - Proposed takes it on
STREETS by decision, as that plot's own work. A sheet named for the group is answered first and
a sheet that takes it by decision second. The same schedule read for MOSQUES leaves the group
out with 68 named, and a mosque plot read for STREETS counts it: the template decides and the
prefix decides nothing, which a test says in those two readings. A test also says the whole
map holds exactly one group by decision, so a second name cannot slip in unnoticed. **A second
name goes in only when the team says so.**

**The rows go where the sheet takes them, and a species under Proposed and under Street Design
adds.** `KpiMerge.Species` takes the `CountedGroups` now, a required argument, and keys every
row on the sheet that takes its group rather than on the group's name, so ALBIZIA LEBBECK on
ST-05 is one merged row of 8, ST-05 8 (2 rows, 2 + 6), going to Tree List - Proposed and
saying both groups, Proposed and Street Design. `MergedSpecies.SheetName` carries the sheet
the merge decided and `SpeciesMatching` places a species through it, and a species built with
no sheet through the same `CountedGroups`, so the matcher's own word rule on the sheet name is
gone and one resolver decides everywhere. Two rows under two different groups are two groups,
so the same group refusal does not trip, and the accounting passes with nothing left out. The
street's areas count the same way: the shrubs and lawn reader already asked `CountedGroups`
per phase, so a Street Design phase row is taken on STREETS with the group total still checked.

A first cut carried the sheet on every `SpeciesRow` from the reader and keyed the merge on it
where a row had one and on the group name where it did not. That broke one test that merges a
reader built row with a hand built one for the same species, and it was two sources for one
fact, so the merge resolves every row itself instead and the row carries nothing new.

**The report says which route each group took.** The reason beside every group row is one of
three: named for it, takes it by decision, or out of scope. A Street Design group counted on
STREETS reads differently from one left out on MOSQUES, and the accounting line reads 0 on
STREETS and 2, on ST-05 when the same plot is read for MOSQUES.

### 2. The note goes on the pane, not only in the report

`CreateWords.GroupsLeftOut` builds one short block from the run's readings and its template:
Street Design found on 2 plots on MOSQUES, which has no sheet for it: DM-16 in its softscape
and its shrubs and lawn schedules, FM-05 in both. Those rows were left out. Fix them in the
model. The first plot spells both schedules out and the next says in both, a plot in one
schedule names that schedule, plots are in natural order, a plot with nothing left out is not
named, two group names are both named, and nothing left out is no note. It names the template
rather than saying not streets, so a group left out on any template reads right. **It is a NOTE
and NOT A REFUSAL**: the workbook is written, the numbers are right, and the note says where the
model needs correcting. Plots and schedules, never species.

`KpiPanel.TheCreateButton` draws it in the warning colour above the Create button off the last
run, the same place the accounting's refusals are drawn, and only when the run held a template.
The report is unchanged and keeps the counts and the areas left out under each plot. The
status line is unchanged too.

### Break watches

Four, each restored byte for byte and checked with cmp, the suite rerun green at 1095.

- the decision ignored, so STREETS counts nothing by decision: **10 red**, nine in
  `StreetDesignTests` and the template test in `GroupRowsTests`
- the decision applied on every template: **21 red**, every FM-05 test that expects the street
  left out on MOSQUES, across `GroupRowsTests`, `SubtotalShapeTests`, `SchedulesAsPrintedTests`
  and `StreetDesignTests`
- the merge keyed on the group name again: **3 red**, the ALBIZIA LEBBECK 8, the matcher and
  the template tests in `StreetDesignTests`
- the note dropped: **4 red**, all of `GroupsLeftOutNoteTests` that expect words

### Existing tests changed

Nineteen calls of `KpiMerge.Species` hand it the fixture's counted groups, because the
argument is required. The template test in `GroupRowsTests` says only STREETS counts more than
its two sheets. No expected value moved.

---

## 2026-09-10, forty eighth pass. The FM-05 refusal answered: a third group, out of scope by decision

The refusal the forty seventh pass raised on FM-05 offered two answers, two types of one
species or one species counted twice, and the answer is neither. The section that prints every
schedule as the schedule prints it, added that pass, showed FM-05's softscape schedule holding
THREE groups: Existing at row 3 with four species adding to 6, Proposed at row 9 with ALBIZIA
LEBBECK 10, BAUHINIA PURPUREA 19 and CASSIA GLAUCA 3 adding to 32, and STREET DESIGN at row 14
with ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4 and CONOCARPUS 4 adding to 38,
then TOTAL 76 at row 20. Its shrubs and lawn schedule prints the same third phase: GRASS
Proposed 96 over 117 and Street Design 69 over 84, total 165 over 201, SHRUBS AND GROUND COVER
Proposed 361 over 450 and Street Design 459 over 570, total 820 over 1020. Every one of those
numbers is off the 1536 report and none is reasoned. The FM-05 10, FM-05 10 that two rounds
chased was one row under Proposed and one under Street Design.

**Bader has decided that Street Design is somebody else's scope and does not belong on this
plot's checklist. The model will be corrected later. Until it is, the tool leaves those rows
out and says so.** That is a decision and not a measurement, and it is recorded here as one.
**The other 43 audit findings stay open**, not renumbered, not reordered, not annotated.
Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing
in this round has been observed in Revit**, and no workbook was written or opened. Every
expected value is written out by hand off the numbers above.

Pull request 66, merged into main as `ee13e8c`. **The runner executed 1081 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1081 ran at `ee13e8c`, 0 failed and 0
skipped**, 566 of them KPI, 21 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked. The
branch sat on `bb97dc3`, after the user's Shared round moved `PaneLabel` and its tests out of
the KPI folder, which is why the KPI count reads 566 and not the 557 plus 21 the last entry
would give: the bare base measures 1060 and 545.

### 1. Only the groups a tree list sheet is named for count, and the arithmetic is checked

The words Existing and Proposed appear nowhere in the code that decides this. `CountedGroups`
holds the two tree list sheet names off the template, Tree List - Existing and Tree List -
Proposed, and a group counts when a sheet's name ends in the group's name, word for word and
without case. Nothing looser: Tree and List are words of both sheet names, TREES is the heading
over the groups, and none of those is what either sheet is for. The reader that used to be
handed the document's phase list is handed this instead, and `KpiRequestHandler` no longer
reads the phases for the create path at all. A group the workbook has no sheet for is out of
scope by construction, on every template, because all seven name their sheets the same way.

`SoftscapeRows.Read` finds every group row, a text only row followed by anything but another
text only row, reads the species rows under each, and takes that group's own subtotal, the
first count with no name under it. Each group comes back as a `PrintedGroup` with its row, its
species, its subtotal row, whether it was taken and why, and the reading's `Species` holds the
taken groups' rows in printed order. Two checks, both refusals in `Reconciliation`: each group's
species rows against its own subtotal row, and the groups taken plus the groups left out against
the TOTAL row. FM-05: 6 plus 32 taken, 38 left out, TOTAL 76. A TOTAL of 80 refuses naming all
three numbers.

`ShrubsAndLawnRows.Read` stops taking the group total. A phase row's subtotal is that phase's,
the phases a sheet is named for are added together, area and item count, and the group total
row is the check on every phase, taken or not. FM-05 GRASS 96 taken, 69 left out, 165 printed.
SHRUBS 361 taken, 459 left out, 820 printed. A group with no phase row at all keeps the rule it
had, the last row is the value and the rows above it must add to it, because such a group has
nothing else to offer. A group whose every phase is out of scope is nought and says so.

**FM-05 reads 6 existing and 32 proposed trees, ALBIZIA LEBBECK 10 and not 20, grass 96 and
shrubs 361**, and its accounting passes. The rule before this one refused the plot, and the one
before that wrote 165 and 820.

### 2. The report names every group row, always

Under the plot, per schedule, every group row in printed order with its row number, how many
species rows it holds and what they add to, its subtotal row and what that prints, TAKEN or LEFT
OUT, and why. Existing and Proposed get a line each, so a schedule with the ordinary two reads
differently from one nobody looked at. The rows left out are listed by name and count under
their group. The shrubs and lawn block prints each phase row the same way and then the group
total row with whether the phases add to it. The accounting at the top gained one line,
schedules holding a group no tree list sheet is named for, with the count and the plots, which
would have shown the street on the first twenty plot run rather than the fourth. The printed
section's summary line names the group rows, the rows read, the rows left out, the subtotals
passed over and the TOTAL row by number, and for the shrubs and lawn schedule which phase row
was taken and which left out and that the group total was checked.

### 3. Two rows for one species in different groups is not a refusal

Under Proposed and under Street Design it is two groups, and the street's row is not in the
reading's species at all, so nothing refuses and ALBIZIA LEBBECK is 10. Under one group it is
the refusal it was, rows and counts named. The words in the rules file that said FM-05 printed
one species twice under one group are corrected: it did not.

### 4. A schedule can repeat a group name, and the report says which is which

DM-25 prints Existing, then Proposed, then Existing again. Nothing guesses which is meant. Both
are group rows in the list with their own row numbers and subtotals, both are named for by the
same sheet, both are taken, and the second's reason says it is the 2nd group row so named on
this schedule, taken as well. `SpeciesRow.GroupRowNumber` carries the group row a species sat
under, so a species under both Existing groups is refused as a species under one group name
twice, and the refusal names both group rows, rows 3 and 9, so a person can see it is two
groups and not one printing twice. Whether DM-25's two Existing groups are one phase printed
twice or two things is UNKNOWN and is for the team.

### 5. The false comment

`KpiPlotReader` said the one schedule guard exists because FM-05 holds two whose names hold
SOFTSCAPE and reading both counted its trees twice. FM-05 holds one softscape schedule. The
comment says so now, says the double was the third group, and says the guard stands for the case
it was built for and that no plot has been measured holding two.

### One shape nobody has measured

A softscape schedule printing TREES and then species rows with no phase row at all would read
TREES as a group row, because a text row followed by species rows is a group row, and TREES
counts for nothing, so every species would be left out and named. That plot would then write
no trees and its report would say why in the group row list and the accounting line. No such
schedule has been seen. Whether one exists is UNKNOWN.

### Break watches

Six, each restored byte for byte and checked with cmp, the suite rerun green at 1081.

- every group counting, which is the rule before this one: **18 red**, across `GroupRowsTests`,
  `SubtotalShapeTests` and `SchedulesAsPrintedTests`
- the shrubs value being the group total again: **9 red**, the four FM-05 and phase tests in
  `SubtotalShapeTests`, three in `GroupRowsTests` and two in `SchedulesAsPrintedTests`
- the group rows lines dropped from the report: **3 red**,
  `TheReportNamesEveryGroupRowUnderThePlot`, `TheReportSaysItUnderThePlot` and
  `TheReportNamesTheScheduleEachNumberCameOffPerPlot`
- the TOTAL check forgetting the rows left out: **2 red**, both in `GroupRowsTests`
- the accounting line dropped: **1 red**, `TheAccountingLineCountsTheSchedulesAndNamesThePlots`
- a repeated group name taken once: **2 red**, the two DM-25 tests

### Existing tests rewritten

The three FM-05 tests that expected 165 and 820 expect 96 and 361 now with the street left out,
and their fixtures name the phases the 1536 report printed, Proposed and Street Design, where
they said Existing and Proposed. The three phase test expects 30 over 11 with Demolished left
out. The printed section test expects the group rows and the rows left out in its summary
line. Two report tests gained the group rows lines under the plot, one of them handing its
hand built reading a printed group so the line reads as a real one would. A species row above
every group row still comes back first and with no group.

---

## 2026-09-10, forty seventh pass. Six things measured on the 1428 run and the workbook opened in Excel

Six things, all measured on the 20 plot MOSQUES run of 2026-09-10 at 14:28, on the workbook it
wrote and on that workbook opened in Excel. Five are fixed and the sixth is reported and not
fixed, as asked. **The other 43 audit findings stay open**, not renumbered, not reordered, not
annotated. Nothing else was touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`.
**Nothing in this round has been observed in Revit, and nothing in it has been observed in
Excel.** The workbook and the model were not handed over and were not needed: every number
below is the one stated for the run, and the test workbooks are built to those shapes with made
up names.

Pull request 60, merged into main as `8eb289b`. **The runner executed 1046 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1046 ran at `8eb289b`, 0 failed and 0
skipped**, 557 of them KPI, 32 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### 1. A written row broke the canopy maths, and the schedule had the fix

The three species written into Proposed rows 84, 85 and 86 carried a name and a count and
nothing else, L84 read `IF(ISBLANK(J84), " ", ROUND(PI()*(J84/2)^2, 0))` and returned a space,
M84 multiplied that space by the count, and #VALUE! ran through M93, F8, D8, E31 and the KPI
row, nine errors that survive a full recalculation.

`SoftscapeRows.Read` reads HEIGHT (m) and DIAMETER (m) off the columns the heading row names,
never by position, into `PrintedMeasure` on every `SpeciesRow`, with the row number beside
them. A measure is held when it reads as a number greater than nought. UNKNOWN prints a dash for
its height and 0 for its diameter, so neither is held and each says what it printed. A cell
holding a digit past its number is not held either, with the reader's own reason, and does not
refuse the schedule, because the species counts whether or not its height reads.
`MergedSpecies.FromRows` holds every row's value against the others and answers per column:
every row that holds a value agrees, and that value is written, or the rows disagree and nothing
is written with every value named and, for the diameter, the words that the row will not compute
its canopy. Nothing is averaged and nothing is taken first. A row printing a dash beside rows
that agree is named and does not stop them.

`SpeciesList.In` finds the sheet's height and diameter columns by what its header row, row 3,
calls them, the words HEIGHT and DIAMETER, and a header naming none or more than one of either
gives no column and the reason. Measured I and J on MOSQUES, Mature Height (m) and Average Mature
Canopy Diameter (m), and a test moves the same headings to N and O and finds them there. The
letters are written nowhere. `KpiCreatePlan.Of` writes four things for a species written in, the
name into D, the count into B, the height and the diameter into those two columns, and nothing
else. Each of the two that cannot be written is named under CELLS NOT WRITTEN with its cell and
the reason. The report's species table gained a height and a diameter column saying what went
into each cell or why nothing did.

**Two things stated and not seen.** The header row is row 3 on both tree sheets, which is the
one number the map holds and was measured on all seven templates. And whether the empty rows of
the client's sheet already hold anything in their height and diameter cells is UNKNOWN. The
patcher replaces whatever a written cell holds, so a formula sitting in I84 would be replaced by
the number. Nothing measured says there is one.

### 2. The double count, and why the guard counted one

**The guard was right and the diagnosis it was built on was wrong.** The count and the read have
been one list in one pass since the forty sixth pass: `KpiPlotReader.Read` sorts every schedule
filtered on the plot into its kind before it reads any, and reads the one when there is one. The
1428 report read one softscape schedule on all 20 plots because there is one. The two FM-05
entries in a species row are two printed rows of that one schedule for one species under one
group, ALBIZIA LEBBECK 10 and 10, BAUHINIA PURPUREA 19 and 20, CASSIA GLAUCA 3 and 4. Counts that
differ are not one row read twice, and the workbook was written, so the species rows added to
the printed TOTAL on every plot that printed one. The forty sixth pass took the user's reading of
a second schedule as measured and fixed a fault the model does not have. That is in the rules
file in those words.

**Whether two rows for one species under one group are two types of it or one counted twice is
written down nowhere**, so the tool refuses rather than choosing. `SpeciesRow.RowNumber` is the
printed row, `PlotReading.SpeciesPrintedOnMoreThanOneRow` finds every such species, and
`Reconciliation.Of` refuses the write naming the plot, the species, the group, the rows and the
counts, and says the rows are printed at the end of the report. Three refusals for FM-05 on the
fixture. A merged row's working reads FM-05 20 (2 rows, 10 + 10), FM-06 15 rather than FM-05
twice, and the plot's own block says which species printed on which rows. **This refuses the
next 20 plot run until Bader says which the FM-05 rows are.** The section under item 6 will show
the rows, with their PLANT CODE, HEIGHT and DIAMETER columns, which is what can settle it. If
they are two types, the refusal comes out and the rows are added, which is what the schedule's
own TOTAL does. If they are one counted twice, the cause is in the model and the tool cannot
know it from here.

### 3. Nothing checked the output still computes, and now something does

`WorkbookFormulas.Check` reads every formula in the output, by its text and never by evaluating
one, after the read back. It finds three things off the text. A formula holding ISBLANK on a cell
that is blank in the output and a string literal returns that text. A formula doing arithmetic on
such a cell is #VALUE!. Every formula reading a cell in error carries it, through ranges and
across sheets, resolved through the workbook's defined names. A shared formula's dependents get
the master's text shifted to their own row, which is how Excel stores a column of one formula.
When the chain starts on a row this run wrote into, `WorkbookPatcher.Patch` deletes the output
again and returns `PatchOutcome.RefusedAfterWriting`, so the run ends with no file and the
report says NOTHING WAS WRITTEN with every formula named. On the fixture shaped like the 1428
workbook, a species written into row 7 with no diameter refuses on 8 formulas: M7, M10, F8, D8,
D9, E31, F31 and G31, with L7 as the cause. The template's own empty rows return a space from
the same formula and are not an error, because the cell beside them guards the blank count.

The section WHAT THE WORKBOOK WILL COMPUTE FROM THIS prints every formula at risk with the
reason, every formula reading a row this run wrote into with the reference it reads it through,
the six cells the map names with whether each is present and which formulas read it and which of
their inputs are blank, and the functions under item 5. `KpiRequestHandler` hands the patcher
the map's cells for that. **A consequence, stated outright:** a species with no diameter written
into an empty row of the client's list refuses the run, because the row's canopy formula cannot
compute from it. UNKNOWN on a proposed list does exactly that. The request asked for a refusal
and not a note, and this is it.

### 4. calcMode auto, the fifth check, and what else the package can hold

`calcPr` carries `calcMode="auto"` beside `calcId="0"` and `fullCalcOnLoad="1"`. `CacheCheck`
reads it back and `WillRecalculate` requires it, five checks and not four, and the report prints
the fifth line. The patcher also looks for every other place in the package that can hold a
calculation setting: a `sheetCalcPr` element in any sheet part, an `xl/vbaProject.bin` part, and
any attribute on `calcPr` other than the three it sets. On the test workbooks it found none, a
`sheetCalcPr` planted in one is found and named, and the report says what was looked for either
way. On the client's MOSQUES template what it will find is UNKNOWN until the next run.

**I cannot test this in Excel and neither can the gate.** The check is over what the file says
and not over what Excel does with it. Whether an explicit `calcMode="auto"` overrides a manual
session in every version of Excel is not measured here.

### 5. The formulas the reader's Excel may not have

Every formula whose text holds `_xlfn.` is counted, by the function named after the prefix and
by cells. On the fixture that is IFS in 2 cells, and the section says those cells need a version
of Excel that has IFS and read #NAME? in one that does not. Which version is not worked out, as
asked. The tool writes no formula and the section says so.

### 6. The report shows what Revit printed

`PlotReading.PrintedSchedules` carries every schedule the plot's numbers came off, as
`KpiPlotReader.Printed` read it, and the report's last section prints each under EVERY SCHEDULE
THIS RUN READ, AS THE SCHEDULE PRINTS IT: the plot and the schedule's name, which rows were read
as species rows, how many were passed over as subtotals and where the TOTAL row was, and for a
shrubs and lawn schedule which subtotal row each group's value was taken off, the last of its
subtotal rows, with the rows above it named as the phase subtotals that add to it and are not
taken. `GroupSubtotal.RowNumber` and `RowsConsidered` carry that. Every column is padded to its
widest cell, rows are numbered the way the readers number them with the heading row as 1, and a
schedule is cut at 200 rows saying how many of how many are shown. The top of the report says
the section is there.

### Reported and not fixed

**Phoenix dactylifera reads 15 metres across in the model and 8 on the client's existing list at
row 86.** `KpiCreatePlan.Differences` names every matched species whose height or diameter in
Revit is not what its row holds, the report prints them under MATCHED SPECIES WHOSE HEIGHT OR
DIAMETER IN REVIT DIFFERS FROM THE ROW'S with CHANGED NOTHING on every line, and nothing is
written over the client's number. **Open question for Bader:** two numbers for one species, one
from the client's palette and one from the model. Which is right, and should the tool ever say?

### Break watches

Five, each restored byte for byte and checked with md5, the suite rerun green at 1046.

- the height and diameter no longer written for a species written in: **4 red**, three in
  `CanopyColumnsTests` and the rewritten `KpiCreatePlanTests` one
- a species on two rows under one group no longer refusing: **1 red**,
  `ASpeciesOnTwoRowsUnderOneGroupRefusesTheWriteNamingTheRows`
- a formula reading an error off a written row no longer refusing: **2 red**, both in
  `WorkbookFormulasTests`
- `calcMode` no longer set: **2 red**, the fifth check test and the patcher's own recalculate
  test
- the schedules section dropped from the report: **5 red**, all of `SchedulesAsPrintedTests`

Item 5 has no watch of its own: the function count is asserted in `WorkbookFormulasTests` and
would go red with the check.

### Two existing tests rewritten

`AnAddedSpeciesWritesItsNameAndItsCountAndNothingElse` said an added species writes two things.
It writes four now, and a match built with no list behind it names the other two, so the test
says that. The TOTAL line under a plot names its row now, so the DM-12 test passes the row
through the fixture and expects row 14, written out by hand.

---

## 2026-09-10, forty sixth pass. Two faults measured on the first twenty plot run

Two faults, both found by running 20 mosque plots for real at 11:16 and reading the workbook
the run wrote. Neither could have been found by reading code. Both are fixed. **The other 43
audit findings stay open**, not renumbered, not reordered, not annotated. Nothing else was
touched: not the Drawing Sheet, not `Core/Shared`, not `CLAUDE.md`. **Nothing in this round has
been observed in Revit.** The workbook the run wrote was not handed over and was not needed: the
row numbers and the counts below are the ones stated for it, and the test workbook is built to
that shape with made up names on every row the run did not name.

Pull request 59, merged into main as `48bd623`. **The runner executed 1014 tests against its
merged head, 0 failed and 0 skipped. Locally the same 1014 ran at `48bd623`, 0 failed and 0
skipped**, 525 of them KPI, 20 added here. The merge went through the API with the title and
the message both passed on the call, and the commit came back off main carrying neither a
co-author credit line nor a generated-by footer. Its tree is the branch's tree, checked.

### Fault 1. The tree list had three row ranges and the tool trusted the shortest

Audit finding 5 made worse, three records rather than two. Measured on Tree List - Existing of
the MOSQUES workbook the run wrote: the map and the pane said B4 to B83, the sheet's total said
`SUM(B4:B92)`, and the botanical names ran from row 4 to row 101, 98 species. The map stopped 18
rows before the names and the total 9 rows before them. Six species were reported as having
nowhere to go, 85 existing trees, and four of the six sat in the list past row 83: CONOCARPUS
LANCIFOLIUS 17 at row 84, PHOENIX DACTYLIFERA 27 at 86, WASHINGTONIA ROBUSTA 19 at 87 and FICUS
BENJAMINA 3 at 89. PROSOPIS JULIFLORA 3 sits at row 99, past the total, and UNKNOWN 16 is
genuinely absent. The workbook said 76 existing trees where the model holds 161.

**What the map no longer claims.** `TreeRows` with its `FirstRow`, `LastRow`, `RowCount` and
`InWords` is gone, and every template's entry is `TreeSheet`, which holds the sheet name and
nothing else. A test reads the type's properties and goes red if a number comes back. The pane's
line no longer prints a range: it says the rows and the total's reach are read off the file when
Create is pressed. The one number the map still holds about a tree list is the header row, 3,
measured on all seven templates, and the list is read down from the row under it.

**What is read.** `SpeciesList.In` reads two things off each sheet and holds them apart. Every
row of column D that names a species, from row 4 down until the first row with no name, is the
list a species from Revit is matched against. The total's own `SUM(B4:B92)` formula, found in
column B, says which rows a count reaches, and the cell it sits in is recorded, B93. The empty
rows for a species the list does not hold are worked out from those two, the rows the total
reaches that name nothing, and are stated by nothing else. The test fixture builds a list the
same way, from names and a total range, so no test can state an empty row that is named.

**A species matched to a row the total does not reach is refused**, per species. The row is
kept on the match, nothing is written there, the reason names the row and the total, and it
prints under a new report heading, SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH, and
in CELLS NOT WRITTEN with its cell. I read the request's word refusal as refusing that count and
not the whole workbook, because the request weighs writing it against not writing it, and both
sides of that weighing have the workbook written. If the whole run should refuse instead, that
is one line in `Reconciliation.Of` and the lists would move ahead of it. A sheet with no `SUM`
over column B refuses every species the same way, matched or not, because nothing then says
which rows a count reaches. A name below the first empty row of the list is not the list, is
named in the report, and a species carrying it is refused rather than written in above itself.
Nothing measured holds such a row and the shape is stated rather than assumed.

**The report prints both lists as read**, under THE WORKBOOK'S OWN TREE LISTS: the names and
their rows, the total and its reach, the empty rows, the names the total does not reach and any
names below the list. When the accounting refused before the template was opened it says so.

**Checked against the stated shape, by hand.** Existing: 98 names in rows 4 to 101, total
`SUM(B4:B92)` at B93, 0 empty rows, 9 names the total does not reach in rows 93 to 101. Proposed:
83 names in rows 4 to 86, the same total, 6 empty rows 87 to 92, none outside. The five species
match their rows, 84, 86, 87, 89 and 99. The first four are written, 66 trees, PROSOPIS
JULIFLORA is named with row 99 and B99, and UNKNOWN is absent with no empty row to go into.

**Two things about that file are stated and not seen.** The names are taken as one unbroken
run from row 4 to 101, because 98 names in rows 4 to 101 is exactly that many rows. The total
is placed at B93 because the rules file measured it there on the annotated set and the request
gave the formula without the cell. Both are in the test fixture and neither was read off the
workbook here.

**One thing the measurements say that nobody asked about.** On 2026-09-09 the MOSQUES existing
list read 80 names in rows 4 to 83. On 2026-09-10 it read 98 in rows 4 to 101. Eighteen names
were added between the two runs, nine of them past the total's reach, and who added them and
why the total was not extended is UNKNOWN. It is in the rules file as a question for the team.

### Fault 2. One plot's trees were counted twice

Audit finding 36, now measured. FM-05 holds two schedules whose names hold SOFTSCAPE.
`KpiPlotReader` appended every one and `KpiMerge.Species` added them by name and group, so the
species rows printed FM-05 twice, FM-05 10, FM-05 10, FM-06 15, and ALBIZIA LEBBECK proposed read
170 where the truth is nearer 160. The shrubs and lawn read had the other half: `SubtotalHeaded`
took the first group with the heading and any second schedule was ignored in silence.

**The reader counts before it reads.** Every schedule filtered on the plot is sorted into its
kind first, softscape or shrubs and lawn, and only a kind with exactly one schedule is read.
`PlotReading` takes the names of every schedule of each kind in place of the two booleans, and
`SoftscapeRead` and `ShrubsAndLawnRead` are now exactly one name. It refuses to be built holding
species rows beside two softscape names or beside none, and subtotals beside two shrubs and
lawn names or beside none, so the doubled count cannot be held anywhere. `Reconciliation.Of`
refuses the write for a plot holding two of a kind, naming the plot, the kind and every schedule
found, and a plot that gave nothing for that reason says so beside its name. The count line
reads plots with one softscape schedule, with the plots holding none and the plots holding more
than one both named.

**The report names the schedule each number came off, per plot.** It read softscape schedule:
11 species rows read before and could not say which. It reads softscape schedule:
FM-06-(600) SOFTSCAPE SCHEDULE, 1 species row read now, and for FM-05 softscape schedules: 2
FOUND AND NONE READ with both names, and for a plot with none, none filters on this plot.

### What the read costs, measured in calls and not changed

The run took 313.5 seconds and 312.8 of that was the read, 20 plots at 15.6 seconds each. The
first thing to check is confirmed by reading `KpiPlotReader.Read`: **every plot walks every
schedule in the model.** For each of the 951 schedules it calls `PlotFilteredOn`, which opens
the schedule's definition, walks its filters and reads each filter's field name, to find the
handful filtered on the plot. Twenty plots is 19,020 of those definition reads and 78 plots is
74,178. Three more costs sit beside it, per plot: `FirstSheetOf` collects every sheet and sorts
all 1,385 by natural order before reading PRX_Plot_ID down the list until it finds the plot,
`RegionsFor` walks every filled region in every 00 link and reads two parameters off each, and
`Printed` calls `GetCellText` once per cell of the one or two schedules that matched. Which of
the four carries the 15.6 seconds is UNKNOWN: the report holds one number per plot and no finer
timer exists, and nothing here runs Revit. Nothing in the read was changed.

### Break watches

Three, each restored byte for byte and checked with md5, the suite rerun green at 1014.

- The list reader made to stop at row 83, where the map used to: **5 red**, all in
  `TreeListRowsTests`. The existing list to row 101, the proposed list to row 86, the five
  species and their rows, the plan's four writes and the named fifth cell, and the report's
  tree list section
- The two schedules refusal taken out of `Reconciliation.Of`: **1 red**,
  `TwoSoftscapeSchedulesOnOnePlotRefuseTheWriteNamingBoth`
- The guard taken off `PlotReading`, so it holds species rows beside two softscape names
  again: **1 red**, `AReadingCannotCarryNumbersOffTwoSchedulesOfOneKindOrOffNone`

### What worked, on the record

The subtotal fix holds on every number it was predicted to move: DM-16 shrubs 30 to 84, DM-25 13
to 241, FM-05 shrubs 361 to 820, FM-05 grass 96 to 165. Shrubs total 2517 to 3258, lawn 1058 to
1127. The date, prepared by and position boxes reached the file. The cache section read cached
results left 0, dropped 1674, 37 parts in and 36 out with calcChain named as the one removed on
purpose. Three species were written into empty rows on the Proposed sheet with the name and the
count and nothing else. All of it is in the rules file under what the first twenty plot run
measured.

---

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
