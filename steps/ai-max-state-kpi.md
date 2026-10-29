# ai-max state, KPI

Phase: 9, ship, part (b) of two. Each PRX_Component value in its own folder, flat or grouped,
K78, F4 in `steps/01_next.md`. Part (a) merged as pull request 179, `6105adf`. Two review passes
each, REAL fixed, L109 to L117 LATENT, Q14 and Q15 answered, Q18 and Q19 asked. Its entry is the
top of `steps/log-kpi.md`. The kit is rebuilt from main after the merge. Nothing in it has run in
Revit.

Phase: 9, ship, part (a) of two. Region areas: one shared area taken without asking, each
option with its area and Show, K77, F4 in `steps/01_next.md`. Part (b), each component value in
its own folder, flat or grouped, is the pull request after it. Its entry is the top of
`steps/log-kpi.md`. Two review passes each, REAL fixed, L97 to L108 LATENT, Q17 asked. Nothing
in it has run in Revit.

Phase: 9, ship. PRX_Component values placed by Bader's word rules, install/component-rules.csv,
K76, one pull request. Its entry is the top of `steps/log-kpi.md`. Two review passes each:
REAL fixed, L85 to L96 LATENT, the claim checker's three wordings fixed. Q14 to Q16 asked.
Nothing in it has run in Revit. The kit is rebuilt from main after the merge.

Phase: 10, package. The KPI Setup.exe, F2b in `steps/01_next.md`, built by Inno Setup beside
the team zip, INSTALL-TEAM.txt leading with it, and Bader's answers to Q8 to Q12. The proof
script is `steps/runs/2026-10-28-setup-exe/prove-setup.ps1`. Its entry is the top of `steps/log-kpi.md`.
Review pass 1 fixed install.ps1 over the Setup and named the installed version on Ready. L67 to
L77 are LATENT. Pass 2 found nothing REAL beyond a Look for line in step 6, fixed. L78 to L84
are LATENT. The claim checker found nothing wrong. 59 checks in `proof.txt`. Nothing in it has
run in Revit.

Phase: 9, ship, part (b) of two. The layout rule and the two prompts in the pane, K74 and K75.
Part (a) merged as pull request 175, `79028a5`. Two review passes on each. L41 to L66 are LATENT.
Its entry is the top of `steps/log-kpi.md`. Nothing in either has run in Revit.

Phase: 9, ship, part (a) of two. Four 60 per cent templates, four new PRX_Component values and
one components.csv in place of the two tables in code. Part (b), the layout rule and the two
prompts in the pane, is the pull request after it. Its entry is the top of `steps/log-kpi.md`.
Review pass 1 fixed: the shared file read while Excel holds it, Excel's csv shapes, a bad
shared row refusing its value. L41 to L50 are LATENT. Review pass 2 left comment counts only, corrected. Nothing in it has run in Revit.

Phase: 9, ship. Q6, Q7 and the KPI team kit, F2 in `steps/01_next.md`. The Shared tool list
merged alone as pull request 173, the KPI work in the pull request after it. Its entry is the
top of `steps/log-kpi.md`. Nothing in it has run in Revit.

Phase: 9, ship. Q1 to Q5 answered and the records inside the AI MAX version 4 caps, F1b in
`steps/01_next.md`. Under AI MAX version 4. Its entry is the top of `steps/log-kpi.md`, and
the split proof is `steps/runs/2026-10-28-records-split/split_proof.txt`. Entries older than
the eight kept here are in `steps/history/ai-max-state-kpi-1.md` and `-2.md`.

Phase: 9, ship, done. The pane redesign for company wide use, F1 in `steps/01_next.md`, merged
to main as `8a2382f`, pull request 170, the squash message back off main byte for byte, the gate
reading 2368 passed and 28 hook cases. Under AI MAX version 4. Its entry is the top of `steps/log-kpi.md`. Build
0 warnings, 2368 tests passed, 28 hook cases. Nothing in it has been seen in Revit.

Phase: 9, ship. Ninety eighth pass, the KPI pane laid out in four steps with a results panel.
**Merged to main as `78b36c4`**, pull request 168, the squash message set on the merge call
and off main byte for byte with no co-author line and no generated-by footer.
**2208 tests, 1388 of them KPI**, 28 hook cases, build zero warnings, run after the last file was
written. **80 audit findings, 23 FIXED, 57 open**, counted off the four files again by walking
each file's own numbering run, and nothing closed, renumbered or reordered.
**NOTHING HERE WAS RUN IN REVIT, AND THE PANE CANNOT BE OPENED FROM HERE AT ALL.**

**THIS ROUND CHANGED ONLY HOW THE PANE IS LAID OUT AND WHAT IT SHOWS.** The press, the report,
the workbooks and the PDFs are untouched and no number in any of them moved. The report was
measured rather than asserted: the same run set written twice, once with the change and once with
`KpiCreateReport.cs` back at main's version, 144 lines, md5 `e1bc564f0f5910159bc28da79fe3e11e`
both ways.

**FOUR STEPS, ONE AT A TIME.** A bar of `1 Setup`, `2 Read`, `3 Tick`, `4 Create` across the top,
the step being worked on marked, a done step carrying a word, and only the step being worked on
showing its controls. A step that cannot be worked on yet shows one line saying why in place of
them, because a cell that does nothing and says nothing is worse than no cell.

**CREATE OPENS ON A WORKBOOK ROW AND A PLOT, WHICH IS STRICTER THAN THE ROUND'S WORDING** of
something is ticked, and is exactly the Create button's own gate on main and today, so the step
and the button cannot say different things. The claim checker caught the looser sentence in the
log and it is corrected there.

**THE FOUR RULES ARE IN CORE AND THE PANE DECIDES NONE OF THEM.** `KpiSteps` says which step may
be worked on, `ListTickFreshness` whether the list tick has gone out of date, `KpiWillWrite` what
step 4 says before the press counted per template, and `KpiResults` what the results panel holds.
Every expected value in their tests is written out by hand.

**THE RESULTS PANEL READS THE REPORT'S OWN ROWS.** `PlotListRows` is one record of THE PLOT
LIST's rows, built in Core, read by `KpiCreateReport` and by `KpiResults`, so the pane's two
counts and the report's `ready:` line cannot say two different numbers and a not ready line
carries the report's own reason word for word.

**THE LIST TICK IS THE LAST TICK.** Tick the list sits under the workbook rows, and a row ticked
or unticked after it makes the pane say the list tick is out of date and offer the button again.
**Nothing is ticked or unticked behind his back.** It is a set of ticked rows rather than a
counter, so a row ticked and unticked back leaves the ticks the list press produced and the line
goes on its own.

**NOTHING WAS DROPPED AND FOUR THINGS BECAME REACHABLE.** The old block returned early three
times, so the output folder, the forms folder, the street reference and the plot list could not be
browsed for until a template had been picked. Step 1 shows all five at all times. Checked by
holding every literal string and every Core call in the old pane against the new one, which is
what caught the one heading that had come off with the folder line it sat over.

**THE TICK BADER ASKED FOR IS REFUSED BY THIS REPOSITORY'S OWN HOOK**, whose emoji range covers
code point 2713, so a done step is marked with the word `done`. The first draft of the round was
refused by that hook for spelling the range out in the comment that explains it, which is the
shape `CLAUDE.md` already carries. Whether to narrow the range is a question for Bader.

**AN ADVERSARIAL REVIEW OF THE ROUND'S OWN DIFF FOUND EIGHTEEN, AND SIXTEEN ARE FIXED HERE.**
None was found by a test, because every one is about what the pane does rather than what Core
computes. The worst: the results panel replaced the only control that answers a refused press,
so a plot waiting on a region pick had its refusal on screen and no way to answer it. Open the
report was dead on every press, because the handler passed a sentence where the pane wanted a
path. The list tick raised its own out of date warning. A step that could not be worked on was
still marked done. Three sentences were false about a plot list that was set and unreadable, one
that names no plot, and a plot nobody ticked. **Two are recorded rather than fixed and both are
for Bader**, the workbook rows being unreachable with no model open, which is his own gate and
reverses a line `kpi-rules.md` carried, and two branches of `CreateWords.CannotCreate` that the
step's own why not now says first.

**THE WHOLE LAYOUT IS UNOBSERVED.** The bar at a narrow width, one step at a time, the results
panel, the two buttons and the step moving by itself after a press are drawn by code nobody has
run. Checks 34 to 37 of `steps/2026-09-16-kpi-checks.md` are the first time any of it is looked
at, and the log names each one.

Phase: 9, ship. Ninety seventh pass, two checks that missed real errors and four report lines.
**Merged to main as `a3e531c`**, pull request 162, the squash message set on the merge call and
off main byte for byte with no co-author line and no generated-by footer.
**2165 tests, 1345 of them KPI**, 28 hook cases, build zero warnings, run after the last file was
written. **80 audit findings, 23 FIXED, 57 open**, counted off the four files again and nothing
closed, renumbered or reordered. **NOTHING HERE WAS RUN IN REVIT.**

**THE 15:55 REPORT AND THE TEMPLATES ARE NOT IN THIS REPOSITORY AND DID NOT ENTER IT.** Every
measurement in the log entry is copied from Bader's own reading of them.

**ERRORS A WORKBOOK OPENS WITH ARE CHECKED NOW.** Three questions, asked on every tab of every
ticked template: a formula holding `#REF!`, a formula reading a range that holds its own cell, and
a formula whose shape, written relative to its own row, differs from the cells above and below it.
The first two make a plot read READY NO through `TreeListCheck.StopsAPlotBeingReady` and
`PlotReady.TemplateErrors`, keyed on the fault's own kind rather than on its words. The third is a
report line, because a column block may differ on purpose at its first or last row, which is also
why only a row with a neighbour on both sides is asked. **An absolute row stays absolute in the
shape**, or a column of `COUNTIFS($B$4:$B$83,...)` would differ from itself on every row.

**A TREE LIST RANGE IS MEASURED AGAINST WHAT THE LIST'S OWN TOTAL REACHES.** Tree List - Proposed
names species to row 83 and its total sums to 92, so every Proposed range ending at 83 went
unnamed on all seven templates. The analysis block rule and the first tab column rule from round
96 are untouched.

**FOUR REPORT LINES.** The glance names at most five `#DIV/0!` rather than all 60. The divisor
travels on the finding, so a division by a count over a range names the range and a division by a
cell names the cell, and the trees sentence fires only where the formula's own guard really read a
tree total. The canopy opening is chosen off the kind of difference, so the M83 case opens with
the M83 fact. And the run sheet is retitled at both lines, which closes the request the ninety
sixth pass logged.

**THREE BREAK WATCHES, EACH RESTORED BYTE FOR BYTE**, md5 before and after each one, on the file
as it stood at that moment: `TreeListCheck.cs` `4286794676d4d4c9011cecb6739da961` twice over, and
`WorkbookArithmetic.cs` `4187d0bacac3f96fb48086bbb5ad5504`.

**AND SIX DEFECTS IN THIS ROUND'S OWN NEW CODE, FOUND BY AN ADVERSARIAL READING OF IT AND FIXED.**
The worst was the shape question's first reading, which named seven rows of a block nobody had
edited, measured by putting that reading back. The neighbours say which reading a block is written
in now, filled down or typed. The other five: a cell reference the file holds that `CellRef.Parse`
threw over, a gate that switched question 8 off in silence, a guard recorded as unevaluated on a
divisor that counted more than nought, a `#REF!` inside a string literal read as a real one, and a
sheet with no diameter column opening with the canopy column sentence. Four tests pin them.
**One more is written down and not acted on**, whether a template whose file could not be opened
should hold a plot back, which is a decision rather than a fault.

**THE RUN SHEET GAINS CHECKS 31, 32 AND 33** and is titled `The run sheet` over its 33 steps.

**WHAT IS UNKNOWN.** Whether the HEALTHCARE Tree List - Existing pair reading `$F$4:$F$101` and
`$F$4:$F$83` is a fault or the client's intent. How many first tab cells each template has out of
step, which the shape question has never run over a client file to find out. Neither is answerable
from this repository, because no client workbook is in it.

---

Phase: 9, ship. Ninety sixth pass, three features that behaved wrongly on the 21:38 press.
**Merged to main as `60949ee`**, pull request 160, the squash message set on the merge call and
off main byte for byte with no co-author line and no generated-by footer. **2152 tests, 1332 of
them KPI**, 28 hook cases, build zero warnings, run after the last file was
written. **80 audit findings, 23 FIXED, 57 open**, counted off the four files again and nothing
closed, renumbered or reordered.
**NOTHING HERE WAS RUN IN REVIT.**

**THE 21:38 REPORT IS NOT IN THIS REPOSITORY AND DID NOT ENTER IT.** Every measurement in the log
entry is copied from Bader's own reading of it.

**READY NO LONGER PASSES A DIVISION NOBODY COULD WORK OUT.** The guard `IF(TotTrees<1," ",...)`
reads a defined name whose cell holds `SUM(B4:B101)`, and a guard cell holding a formula answered
not evaluated, so all 284 divisions of that press went unjudged and every plot read READY YES.
`TheGuard` follows the name to its cell and counts the range through `OverARange`, which
`RangeCountsToNought` now asks too so there is one counter. `DivisionsNotEvaluated` carries the
sheet and the cell apart from the sentence, `PlotReady.DivisionsNotChecked` is the new reason, the
glance names at most five and each plot's own block holds the full list.

**THE NO PLANTING LINE FIRES.** It asked `BodyRowCount`, which counts headings, so a schedule
showing only its heading read as one that printed something and MM-06, MM-07 and NS-23 were never
named. It counts the rows under the heading now, through `NoPlanting.RowsBelowTheHeading`, and
`Reconciliation`'s count of schedules that printed a body asks the same method. A schedule whose
rows Revit refused is not one that printed nothing either, which is the same shape found while
making the fix.

**THE TEMPLATE CHECK NAMES N85 AND N88 TO N101, PRINTS NO LINE TWICE AND LEAVES THE ANALYSIS
BLOCKS ALONE.** An empty formatted cell counts as empty through one rule at the question rather
than in the reader. Each cell and range pair is named once and the count at the top counts
distinct cells. A range stops short only when its column sits inside the list's own block, whose
right edge is read off the file, so S59, T61, V59 and W61 are no longer named. A formula on the
first tab reading a column outside that block has its own question, naming the column and what it
holds.

**THREE BREAK WATCHES, EACH RESTORED BYTE FOR BYTE.** `WorkbookFormulas.cs`
`7afd785468306de624176e77ef0bdc32`, `NoPlanting.cs` `087a22aa231fb11b16c44a856b94a2fb`,
`TreeListCheck.cs` `254bb9deaaa3d6849f586dba3277c26e`, md5 before and after each one.

**THE RUN SHEET GAINS THREE CHECKS**, 28, 29 and 30, keeping its Windows order and every existing
check, with what remains, the audit block and what comes next refreshed.

**WHAT IS UNKNOWN AND WHAT THE NEXT PRESS ANSWERS.** Whether any template holds a list column past
the water total, which the right edge rule would not name. How many first tab formulas read a
column the list does not fill. What `<Streets>` E37 and E38 are for. None is answerable from this
repository, because no client workbook is in it.

**A REQUEST FOR BADER.** `steps/2026-09-16-kpi-checks.md` is titled `The seven checks` and holds
thirty steps. It was not renamed, because the title is what he calls the file.

---

Phase: 9, ship. Ninety fifth pass, the two requests the ninety fourth logged.
**Merged to main as `619f134`**, pull request 158, the squash message set on the merge call and
off main byte for byte with no co-author line and no generated-by footer. **Nothing the tool
does changed and nothing outside `steps/` changed at all**, so the counts stand unmoved: **2139
tests, 1319 of them KPI**, 28 hook cases, build zero warnings. **80 audit findings, 23 FIXED, 57
open**, untouched. **NOTHING HERE WAS RUN IN REVIT.**

**`steps/` IS COMMON GROUND**, which is why a KPI round edits the Drawing Sheet's and the View
Filters' run sheets, off `.claude/rules/territory.md`, and the log says so.

**FIVE OLDER KPI SHEETS ARE MARKED AS RECORDS**, `2026-09-15-kpi-fixes`, `2026-09-15-kpi-rerun`,
`kpi-create`, `kpi-scan-2` and `kpi-templates`, one line each under the title saying a run today
uses `steps/2026-09-16-kpi-checks.md`. Nothing else in them moved, so their steps and counts stay
as the record of that day. **The precondition was checked first**, thirty hits over four
files, `log-kpi.md` 23, `ai-max-state-kpi.md` 5, `log-drawing.md:1033` 1 and
`.claude/rules/kpi-rules.md:6` 1. **The first draft said a single hit outside the two KPI records
and that was wrong**, because the sweep it came off piped `log-drawing.md` out and the sentence
did not say so. Neither of the two is an instruction to run one of the five, so the mark stands,
and the rules trigger is a request below. And `2026-09-16-kpi-checks.md` was swept and really
holds no `dotnet test` line.

**THE THREE TASK SHEETS READ THE GATE**, step 5 in each, same number, one action, saying the
tests run there on .NET 10 so the PC needs no .NET 10 SDK, and pointing at the count in the top
entry of that task's own log rather than carrying 1103, 1578 or 1569. **The round 94 sentence
under each build step went with it**, because it said the step above wants the SDK and that
became false the moment the step became a gate check.

**WHICH IS LATEST, ASKED OF THE LOGS.** `2026-09-13-colour-box.md` is the latest View Filters
sheet, named at `steps/log-view-filters.md:22` in the top entry. `2026-09-13-view-filters.md` is
the earlier one, named at `:133` in the entry below. `run-drawing.md` is the only Drawing Sheet
sheet and so the latest by being the only one, and it is **older than that task's latest round**,
its own opening naming pull request 32 against a log top entry carrying 105 and 106. Whether it
still covers the panel is UNKNOWN from here.

**THE SWEEP**, before and after. Before: exactly the six local test runs and the three counts,
nothing else in `steps/`. After: three local `dotnet test` steps remain, all inside sheets now
marked as records, and **no stated test count is left in any sheet**.

**THE THREE RECORD SHEETS STILL CARRY ROUND 94'S SENTENCE** at `kpi-create.md:37`,
`kpi-scan-2.md:37` and `kpi-templates.md:37`, saying the step above wants the SDK, and **it is
still true there** because their step above really does run the tests locally. It was false only
in the three sheets whose step changed, which is where it was replaced.

**THE CLAIM CHECKER HAD NO SHELL AND CAUGHT THE ONE THING THAT WAS WRONG**, the sweep count
above, by re-running the sweep it could run. It also flagged an empty heading left as a
placeholder. The four counts and the two diff claims it had no tool for were re-run here: zero
warnings, 2139, 1319, 28 hook cases, nothing outside `steps/` in the diff, and the five older
sheets showing 15 added lines and 0 deleted, all of them the record marker.

**THREE REQUESTS FOR BADER**: the rules trigger at `kpi-rules.md:6`, whether
`2026-09-13-view-filters.md` should be marked a record too, which is View Filters' call, and
whether `run-drawing.md` needs a new sheet, which is Drawing Sheet's.

---

Phase: 9, ship. Ninety fourth pass, every other place that still built the whole solution.
**Merged to main as `a068c8f`**, pull request 156, the squash message set on the merge call and
off main byte for byte with no co-author line and no generated-by footer.
**Nothing the tool does changed** and nothing under `src/` changed but one comment, so the counts
are the ninety third pass's and stand unmoved: **2139 tests, 1319 of them KPI**, 28 hook cases,
build zero warnings. **80 audit findings, 23 FIXED, 57 open**, untouched. **NOTHING HERE WAS RUN
IN REVIT.**

**THE INSTALL SCRIPT'S TWO REFUSALS.** `install/install.ps1:57` and `:63` sent a person at the
build that fails and now name `src\RcrcGreen.Revit\RcrcGreen.Revit.csproj` with the same
`$Configuration`. **Whether it still parses is UNKNOWN**: neither `pwsh` nor `powershell` is on
this machine, and writing a second parser to answer would be two records of one fact. The diff is
what is backed, one sentence swapped inside the two double-quoted strings with every
interpolation untouched.

**THE EIGHT OLDER RUN SHEETS** build the add-in project now, each keeping its `-c Release`, its
step number and its one action, and six step titles that said solution moved with the command.
No sheet of the eight names Visual Studio. **`steps/` is common ground**, which is why
`run-drawing.md` and `2026-09-13-view-filters.md` are in scope for a KPI round, and the log says
so.

**THE ROUND'S OWN SENTENCE WAS FALSE IN SIX OF THE EIGHT AND WAS MEASURED BEFORE IT WAS
WRITTEN.** Those six run `dotnet test` on the `net10.0` test project one step ABOVE the build,
and that command stops with `NETSDK1045` exactly as the solution build does, run here to check.
So the sentence reads `this build` rather than `this PC` and names the test step above as the
thing that still wants the SDK. The other two sheets ask for no test run, grepped for rather than
assumed, so they carry step 7's original wording, which is true in them.

**CLAUDE.md:37** builds the add-in project in Release and says the solution also holds the tests,
which need the .NET 10 SDK.

**THE STALE C# NUMBER.** `RcrcGreen.Core.csproj:8-9` said C# 12 and says 14, read off SDK
`10.0.401` through `dotnet msbuild -getProperty:LangVersion` on the test project, with the
comment now saying that 14 is the SDK's own default and that the test project declares no
`LangVersion`. Comment only, and msbuild still answers `netstandard2.0`, `LangVersion` 12.0 and
`AssemblyName` RcrcGreen.Core.

**SWEPT AFTER THE EDITS.** Three places still name a solution build and all three are right:
`.github/workflows/tests.yml:51`, which is the gate, `steps/audit.md:107`, a record of that line,
and `steps/2026-09-16-kpi-checks.md:95`, the sentence saying why step 7 used to.

**THE CLAIM CHECKER HAD NO SHELL AND FOUND NOTHING WRONG IN WHAT IT COULD CHECK.** It caught one
thing by NOT being able to see the diff: it reported that no sheet of the eight holds the word
solution today and called that corroboration rather than proof. Run here, **all eight step titles
moved and only six of them had said solution**, where the entry said six and was silent about the
other two. It also drew the line between the four `audit-kpi*.md` files the 80 is counted off and
`steps/audit.md`, which is cited separately and is no part of it. Both are written in. The six
checks it had no tool for were all re-run at this commit.

**TWO REQUESTS FOR BADER.** The six local `dotnet test` steps are the same fault one step up and
were not in this round's scope, so the sheets name which step wants the SDK and the decision is
his. And three sheets carry a test count off main as it was, 1578, 1569 and 1103 against today's
2139, left because they are records.

---

Phase: 9, ship. Ninety third pass, the build step in the run sheet and the two lines that still
said net8.0. **Merged to main as `31b4d3d`**, pull request 154, the squash message set on the
merge call and off main byte for byte with no co-author line and no generated-by footer. **Nothing the tool does changed**, so the counts are the ninety second pass's and
stand unmoved: **2139 tests, 1319 of them KPI**, 28 hook cases, build zero warnings. **80 audit
findings, 23 FIXED, 57 open**, untouched. **NOTHING HERE WAS RUN IN REVIT.**

**THE RUN SHEET TOLD BADER TO BUILD SOMETHING HIS PC MAY NOT BE ABLE TO BUILD.** Step 7 built
`RcrcGreen.sln`, which holds the test project the ninety second pass moved to `net10.0`, so
without the .NET 10 SDK the solution build fails with `error NETSDK1045` and the step said to
stop there. It builds `src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release` now, and the step
says the tests run on the GitHub test gate on .NET 10 so that PC needs no .NET 10 SDK.

**BOTH HALVES WERE MEASURED RATHER THAN REASONED**, on this machine's own .NET 8 SDK, which is
a PC without .NET 10 exactly: the solution build fails naming the test project, and the add-in
project build succeeds and emits Core with it.

**AND EVERY FILE `install.ps1` COPIES OUT OF A BUILD COMES OUT OF THAT ONE**, traced by line
before the step changed. `:50` is the add-in project's own output folder, `:53` the manifest from
`RcrcGreen.Revit.csproj:50-52`, `:54` the two assemblies with Core arriving through the
`ProjectReference` at `RcrcGreen.Revit.csproj:14`, and `:84` the two symbol files. Everything
else is copied out of `install/` or written by the script. The folder was listed after the build
and held those five files and nothing else.

**THE TWO NET8.0 LINES READ NET10.0**, `CLAUDE.md:54` and
`src/RcrcGreen.Core/RcrcGreen.Core.csproj:11`, one word each and nothing else in either file.
Bader's line for this round covered those two lines only. **The csproj change is inside a
comment**, asked of msbuild rather than eyeballed: the project still answers `netstandard2.0`,
`LangVersion` 12.0 and `AssemblyName` RcrcGreen.Core.

**THE CLAIM CHECKER HAD NO SHELL AND SAID SO**, so it could run none of the builds, tests or
`git diff`. It found one real fault, in the entry rather than in the change: the entry claimed
step 7 kept its lead sentence, and the diff says the heading and half that sentence moved and
only the number stayed. It sharpened one more, that the test project's C# 14 is the SDK's default
and is declared in no file, and it flagged an empty heading left as a placeholder. Every
measurement it could not reproduce was re-run here at this commit rather than waved through.

**THREE REQUESTS FOR BADER**, all in files this task may not edit. `install/install.ps1:57` and
`:63` still say `Build RcrcGreen.sln in $Configuration first.` and now name a build the sheet no
longer asks for. `RcrcGreen.Core.csproj:8-9` says the test project gets C# 12, and it resolves to
14.0 on net10.0 while Core is pinned to 12.0 by line 10, so the comment's point holds and only
the number is stale. **And eight other run sheets in `steps/` carry the same solution build line
this round fixed**, named with their line numbers in the log and deliberately not changed, since
several are the record of a run that already happened and `run-drawing.md` is another task's.
`.github/workflows/tests.yml:51` builds the solution and is RIGHT, because the gate installs
`10.0.x` at `:45`.

---

Phase: 9, ship. Ninety second pass, five fixes and one rule removed, all decided by Bader on
16 September. **Merged to main as `133b8b0`**, pull request 152, the squash message set on the
merge call and off main byte for byte with no co-author line and no generated-by footer.
**2139 tests, 1319 of them KPI**, up from 2111 and 1291, 28 hook cases, build zero warnings, on
.NET 10. **80 audit findings, 23 FIXED, 57 open**, counted per file off all four, three closed,
65, 67 and 71, and nothing renumbered or reordered.

**THE 16 SEPTEMBER REPORTS AND WORKBOOKS ARE NOT IN THIS REPOSITORY AND DID NOT ENTER IT.**
**NOTHING IN THIS ROUND WAS RUN IN REVIT**, and the Revit half of finding 65 cannot be run from
this session at all.

**THE TOTAL CANOPY CHECK NO LONGER SWITCHES ITSELF OFF.** Where the template HAS a Total Green
cover cell and the column cannot be read, the green cover and the canopy percentage are BLANK on
any plot holding a count on that sheet, the reason names the template, the sheet and
`TotalCanopyColumn.Why`, READY reads NO, and no empty row of that sheet is usable. A template
naming NO green cover cell still refuses nothing, told apart by
`TotalCanopyColumn.TheTemplateNamesNoGreenCover` and never by the reason's words.
`UnreadableCanopyColumns` is the glance line, one per press.

**TWO UID2 VALUES THAT DIFFER ONLY IN LETTER CASE ARE ONE FOLDER ON WINDOWS.** `SharedUid2.Of`
groups on `OrdinalIgnoreCase` and the group carries the first spelling seen, so the colliding
plots are found by the file path across every ticked plot. Each line prints each plot's own
spelling through `NamedWithValues`, and a group whose spellings differ only in case says so
through `WhereTheSpellingsDiffer`.

**THE TEMPLATES' OWN TREE LISTS ARE READ ONCE AT THE PRESS, CELL BY CELL.** `TreeListCheck.In`,
seven questions per sheet, every column read off the file and none held as a letter, the water
pair found as a column whose rows are another column multiplied by the count and which is not the
canopy pair. A column that cannot be read is NOT READ and the sheet is not clean. It is a report
section and a glance line per template and it stops no write.

**.NET 10.** The test project and the gate, read off Microsoft's own policy page this session:
.NET 10 LTS Active to 14 November 2028, .NET 8 and .NET 9 both ending 10 November 2026. `net48`
and `netstandard2.0` unchanged. **No break watch for a version move.** `CLAUDE.md:54` and
`src/RcrcGreen.Core/RcrcGreen.Core.csproj:11` still say net8.0 and are left for Bader.

**FINDINGS 65 AND 67.** A throw reading which plot a schedule filters on is a
`SchedulePlotRead` refusal naming the schedule rather than an empty string that read as belonging
to no plot, with the rule and the words in Core where a test reaches them and the Revit catches
themselves untested. `PlotsInTheModel.SchedulesNotRead` carries them to
`EVERY PLOT THE TOOL OFFERED`, and a refusal never becomes a plot. All four .xlsx readers catch `System.Xml.XmlException` and return the refusal
each already had words for, so a malformed sheet part no longer stops the whole press naming no
template, no file and no plot with no report written.

**THE PANE PICTURE RULE IS REMOVED** from `.claude/rules/revit-commands.md`. No mockup is owed for
any past round and `design/` is untouched.

**BREAK WATCHES, ALL FOUR WATCHED RED AND RESTORED BYTE FOR BYTE.** The empty return at
`WorkbookArithmetic:549` reddened the typed canopy test naming the template. The Ordinal grouping
reddened the case test naming MM-01 and MM-09. Dropping the typed canopy category reddened four
tree list cases, the first naming L85. Removing the `XmlException` catch from `SpeciesList`
reddened the malformed part test naming `broken.xlsx`, after the test was rewritten to catch the
throw so its failure names the file either way.

**ONE BUG OF MY OWN, CAUGHT WRITING THE TESTS.** `TreeListCheck` question 2 handed
`IsTheTotalCanopyFormula` the DIAMETER column where the CANOPY column belongs, which would have
named every row of every sheet. The canopy column is read once per sheet now and both questions
ask it.

`steps/2026-09-16-kpi-checks.md` is updated in place with three more checks, 25, 26 and 27, each
carrying its own command, and the audit table and what comes next are refreshed.

**THE CLAIM CHECKER FLAGGED FOUR THINGS AND ALL FOUR WERE ACTED ON**, the counts, the md5 sums,
stale line citations, and a false alarm in its own suggested grep. It had no shell tool and said
so rather than answering anyway. The log entry lists each one.

---

Older entries were moved whole, byte for byte, on 2026-10-28: `steps/history/ai-max-state-kpi-1.md` to `steps/history/ai-max-state-kpi-2.md`, newest first.
