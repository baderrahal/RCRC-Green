# KPI log

Newest entry first.

---

## 2026-10-28, the 60 per cent templates and one components.csv, part (a) of two

**Goal.** Four 60 per cent templates and four new PRX_Component values, one components.csv in
place of the two tables in code, and the spacing tolerant match. Part (b), the layout rule and the
two prompts in the pane, is the next pull request.

**Done.** `design/component-wiring.md` maps every use and holds the cell by cell comparison: the
LESS THAN 60 files are byte for byte MOSQUES and SCHOOLS, and MORE THAN 60 differs in H31 to H35
only. `install/components.csv`, fifteen rows, read by `ComponentTable`, shipped under personal
under shared. `ComponentMatch` is the one match rule. The four templates are versions of their
kind, K72 and K73. install.ps1, the kit and team-install carry the csv. The round prompt asked for
the install.ps1 change, and the line in `RcrcGreen.Core.csproj` is what puts the file beside the
DLLs, so both are named here for Bader.

**Checked.** RAN after the last code file: build 0 warnings 0 errors, 2472 tests passed against
2395 before, 28 of 28 hook cases. install.ps1 into a throwaway profile put components.csv beside
the DLLs, same SHA-256. A kit built from the branch holds it and installs it.

**UNKNOWN.** Nothing ran in Revit. The model's spelling of the two school values.

**Remains.** Part (b).

**Known bugs.** A shared components.csv that cannot be read lets the shipped one answer with no
word on the pane until part (b).

**Next.** Part (b).

**Your turn.** Nothing until the round ends.

---

## 2026-10-28, Q6, Q7 and the KPI team kit, F2

**Goal.** Q6 and Q7 in, one build that shows KPI alone where features.txt says so, a build stamp
in the KPI pane and its reports, and a zip the production team installs with no repo and no
admin rights.

**Done.** Q6: `PressEnd.RedrawsTheSteps` draws the steps again after a refused read. Q7: five
id lines in CLAUDE.md. `Features` in Core/Shared decides from the lines of features.txt, which
the application reads: none or empty shows all three, an unknown name goes to Revit's journal, a file naming no tool shows all three, CHOSEN
and Q8. It merged alone as pull request 173, `4390520`, because territory.md asks a Shared change
for a pull request of its own, against this round's one pull request. The csproj stamps the
short hash and build time, with changed for changes or untracked files, shown after Build in the
pane header and as the first line of both KPI reports. `install/build-team-kit.ps1` builds a
throwaway worktree and zips to Documents\RCRC-Green-Team-Kit. `install/team/` holds
team-install.ps1, the two launchers from the AI MAX assets and INSTALL-TEAM.txt. install.ps1
removes a kit's features.txt and VERSION.txt.

**Checked.** RAN after the last file: build 0 warnings 0 errors, 2395 tests passed, 28 of 28
hook cases, against 2380 and 28 before. The PressEnd test failed 1 of 3 on the old rule.
Pull request 173's gate read 2388 and 28. In throwaway profiles, through the launchers of the
zip built from `f23b895`: Bader's install.ps1, the kit, the kit again, put back with a stand-in
Revit.exe running (REFUSED, exit 2), put back (DONE, the full install's DLL back by hash,
features.txt gone). Before that, a fresh install made the reports folder, a folder named
Temp1_x.zip was REFUSED, a blocked file beside the kit stayed blocked, and the chosen templates
folder was untouched. The zip holds 11 files, 631,384 bytes. Reviewer and breaker, two passes
each, their findings in `steps/latent.md` by pass. The six REAL are fixed: the same zip twice, the unblock sweep, untracked files in the stamp,
OneDrive Documents, the unsigned add-in prompt, the Shared pull request. L23 to L40 are LATENT.
Usage: the session reads 254,875,370 tokens and 94.98 USD. Since the F1b reading, F1b's merge
turns included, that is 40,444,652 and 16.10, read before this round's final checks and merge.
Both rows are partial. This one is more than F1b's 24,229,432 and 12.40 and under twice it.

**UNKNOWN.** Nothing ran in Revit: the KPI only ribbon, the stamp on screen, Q6 on a real refused
read, Revit's unsigned add-in prompt. No team PC: OneDrive, SmartScreen, Group Policy.

**Remains.** The kit from main is built after the merge, outside the repo.

**Known bugs.** None open.

**Next.** Bader's pass in `steps/03_your_steps.md`, then the zip to Teams or SharePoint.

**Your turn.** Q8 Features file naming no tool. The run in `steps/03_your_steps.md`.

---

## 2026-10-28, Q1 to Q5 answered and the records inside the version 4 caps

**Goal.** Bader's answers to Q1 to Q5 in, every record a session pays for at start inside the
AI MAX version 4 caps, split byte for byte, and a test in the gate that holds the caps.

**Done.** Q4: `KpiPaneWords.ReadsSheetsAndSchedules` shows in step 2's body before a read, the
empty body Q4 named, because the status line already sits under the button. Q1, Q2, Q3 and Q5
are answered A with no change. The split reads the blobs at `4b244b3` through
`steps/runs/2026-10-28-records-split/split.py`. Bytes as git stores them:

```
.claude/rules/kpi-rules.md      269,551 -> 8,277, plus 15 parts in docs/rules/
.claude/rules/core-rules.md      37,971 -> 4,841, plus 2 parts
.claude/rules/revit-commands.md  42,270 -> 8,772, plus 2 parts
CLAUDE.md                        28,418 -> 6,606, plus 3 sections in docs/
steps/log-kpi.md                623,058 -> 23,201 before this entry, plus 7 history parts
steps/log-drawing.md            270,447 -> 27,575, plus 3 history parts
steps/ai-max-state-kpi.md       208,136 -> 28,284 before its new entry, plus 2 history parts
```

Each docs part is at most 30,000 bytes and each history part at most 100,000.
`docs/rules/index.md` names the file of all 158 ids, 155 rules and 3 CLAUDE.md sections. `RecordCapsTests` holds the caps. This
KPI round edited Drawing Sheet's two rules files and log because the round asked for it, and the
hook reads both folders as common.

**Checked.** RAN after the last file: build 0 warnings 0 errors, 2380 tests passed,
28 of 28 hook cases. SHA-256 of each original against its parts joined: 7 of 7 MATCH, and
the heading counts 122, 28, 27, 11, 75, 47 and 49 are equal, in `split_proof.txt`, each part read back off
disk. The caps test failed 3 of 11 on main's records, `caps_on_main.txt`, and passes 11 here. The
reviewer and the breaker made one pass each, and the reviewer a second over the fixes. Every REAL
one is fixed: the script pinned to `4b244b3`, every entry dated from the caps held to 4 KB, the
one old entry over it pinned at 12,864 bytes, three stale pointers, a heading over the CLAUDE.md
pointers, and no tooltip on the Q4 line while a read runs. Nine LATENT are L14 to L22. Usage:
session c82dc224 carries F1 and this round, 214,430,718 tokens and 78.88 USD in all, read
before the final checks and the merge. This round is 24,229,432 and 12.40, less than F1's
190,201,286 and 66.48.

**UNKNOWN.** Nothing is seen in Revit, the Q4 line included. Its tooltip change is pane only and
no test reaches it.

**Remains.** Nine comments in src and tests and eight lines of moved text still name
`kpi-rules.md` or `CLAUDE.md` for what now sits in docs. Each reaches it through one pointer, and
both src and moved text were out of scope.

**Known bugs.** A failed read leaves step 2's Read model grey, Q6.

**Next.** F2, the team kit, in a fresh session.

**Your turn.** Q6 A failed read leaves step 2 dead. Q7 Every turn rules now in docs. The run
in `steps/03_your_steps.md`.

---

## 2026-10-28, the pane redesign for company wide use, F1

**Goal.** The KPI pane rebuilt to Bader's approved design, no pane sentence over 10 words, the
long texts in the report and in tooltips, nothing the pane did lost. What a press reads and the
workbooks and PDFs it writes stay as they were, and the report gains one section.

**Done.**
- Four steps, Setup, Read, Plots and Create, under one frame: header with the model and the
  build, a row of circles, sections, a footer with one main button and a status line. Step 3
  was Tick and is `KpiStep.Plots`. Colours and sizes in `KpiTheme` alone
- A result panel: banner, Created and Failed, a row per asset type, a short reason per failed
  plot, missing PDFs and not ready plots as boxes, Retry failed for what a retry can write, New run
- `BEFORE THE PRESS` in the report holds what the pane used to explain, each box once.
  `design/kpi-pane-words.md` gives every old pane string a home
- New in Core: `PaneWarning`, `CreateBlocked`, `FailedReasons`, `BeforeThePress`, `MissingPdfs`.
  A warning's kind is a value, where the pane used to search its text for WRITTEN NOWHERE
- **Two decisions reversed and named.** Create's reason names the first thing missing, not both
  ticks as the ninety eighth pass did, because step 4's shut line and the footer read one reason
  now. The
  output folder being the templates folder blocked Create in the first draft, and that was taken
  back: it is a note, as on main
- Outside the pane: the hooks fixed for Windows, from 4 of 28 to 28 of 28, AI MAX version 4 in
  the repo, the version 4 agents, and the steps files version 4 expects

**Checked.** RAN after the last file was written: Release build 0 warnings 0 errors, the suite
2368 passed 0 failed against 2259 on main, `hook-tests.sh` 28 passed 0 failed. Two review passes,
breaker and reviewer each, whose reports are not in the repo. The lead turned pass 1's REAL
findings into 17 fix items and pass 2's into 8, labelled in `KpiPaneFixPassTests.cs`. Pane-only,
so no Core test: pass 1 items 6 and 16, pass 2 item 4. No behaviour change, so no test: pass 1
items 9 and 10. 13 LATENT are in `steps/latent.md`. `TheNewSectionIsTheOnlyDifference` holds the
report with and without BEFORE THE PRESS equal once the section is cut out. The word list was
sampled against main's pane by the breaker, and no test holds it against the old file.

**UNKNOWN.** No report or workbook was written on main and on this branch from one run set to
compare bytes, so unchanged output rests on the diff, which shows refusal kinds on the patcher
and the run set and no change to a written cell. The developer reported breaking each Core fix
and seeing its test go red, which no file records and the lead did not repeat. Nothing in this
round has been seen in Revit. Every look, width, tooltip and click
is unobserved, and so is the progress window. No screenshot exists: the pane needs a running
Revit and a model, and this session did not drive one.

**Remains.** Bader's pass in `steps/03_your_steps.md`. Dark theme values if Q1 asks for them.

**Known bugs.** The dropdowns have square corners, Q5. Step 2 is empty before a read, Q4.

**Next.** F2, the team kit, in a fresh session.

**Your turn.** Q1 Dark theme colours for the KPI pane. Q2 Should Read write its own report.
Q3 All and None on searched plots. Q4 A line in step 2 before a read. Q5 Rounded corners on
the dropdowns. And the run in `steps/03_your_steps.md`.

---

## 2026-09-19, ninety eighth pass. The pane in four steps, and the result in the pane

**Merged to main as `78b36c4`**, pull request 168, squashed with both message fields passed
on the call, so the message came back off main byte for byte with no co-author line and no
generated-by footer.

Bader chose this layout on 17 September: the pane works in four steps, one at a time, and after a
press it shows the result in the pane instead of sending him to the report file.

**THIS ROUND CHANGES ONLY HOW THE PANE IS LAID OUT AND WHAT IT SHOWS.** Nothing about what the
press reads, writes or reports changed, and no number in any workbook, PDF or report moved. That
is measured rather than asserted, under its own heading below.

**2208 tests, 1388 of them KPI**, run after the last file was written, up from 2165 and 1345.
28 hook cases. Build zero warnings. **80 audit findings, 23 FIXED, 57 open**, counted off the
four files again with nothing closed, renumbered or reordered.

**NOTHING HERE WAS RUN IN REVIT**, by this session or by anybody, and **the pane cannot be opened
from here at all**. Which parts are unobserved is its own section below, and it is most of the
round.

### The four steps, and what decides them

A bar across the top carries `1 Setup`, `2 Read`, `3 Tick`, `4 Create`. The step being worked on
is the marked cell and a step that is done carries a word beside its name. Only the step being
worked on shows its controls, and a step that cannot be worked on yet shows one line saying why
in place of them.

**Which step may be worked on is decided in Core**, `KpiSteps.Of`, off plain values the pane reads
live at every draw. The pane draws the answer and decides none of it, which is the rule
`PanelSteps` already carries for the other pane.

- **Setup is always usable**, because it is where every reason below is fixed.
- **Setup is done when the templates folder and the output folder are set.** Those two are the
  only ones that refuse: `TemplateWords.NoFolder` ends the workbook list before it starts and
  `CreateWords.CannotCreate` refuses on the output folder. The forms folder, the street reference
  file and the plot list file are each recorded in `kpi-rules.md` as a note and never a refusal,
  so they are named in the still not set line and gate nothing. **Gating on one of them would
  hold back a press this tool is willing to make.** That reading is mine and it is named here as
  a decision rather than a measurement.
- **Read is usable once Setup is done** and done once a read has landed.
- **Tick is usable once a read has landed** and done once a workbook row and a plot are both
  ticked, which is exactly what the Create button's own gate reads on main and today, so the
  step and the button cannot say different things.
- **Create is usable once a workbook row AND a plot are ticked**, which is Tick's own done and
  is stricter than the round's own wording of something is ticked. Its why not names both
  missing ticks rather than the first of them. 154 plots ticked with no workbook leaves Create
  shut, and a test says so.

### The tick Bader asked for is refused by this repository's own hook

`.claude/hooks/writing-check.sh` line 47 builds its emoji pattern from four ranges and the second
runs from code point 2600 to 27BF. The tick character is 2713, inside it. **A commit carrying one
does not land**, so a step that is done is marked with the word `done`.

**AND THE FIRST DRAFT OF THIS ROUND WAS REFUSED BY IT.** The comment explaining the rule was
written with the range spelt out, the characters reached the file, and four lines across
`KpiSteps.cs` and `KpiStepsTests.cs` would have been refused by the hook they describe. That is
the shape already in `CLAUDE.md`, where a commit was refused by its own message for naming the
marker it introduced. Both files name the code points in words now and carry none of them, and
the one assertion that needs the character builds it from `((char)0x2713)`.

**Whether the range should be narrowed so a tick can be used is a question for Bader**, and it is
a change to that hook rather than to the pane.

### The list tick is the last tick

Ticking a workbook row ticks that template's plots, so a row touched after Tick the list leaves
the ticks saying something the team's list does not. `ListTickFreshness` is the rule: the workbook
rows that stood at the press travel with it, and the pane says the list tick is out of date and
offers the button again.

**NOTHING IS TICKED OR UNTICKED BEHIND HIS BACK.** Moving the ticks without being asked is the
fault the hand choices already cost this tool a round.

**IT IS A SET AND NOT A COUNTER**, compared with `StringComparer.Ordinal`, which is what
`PlotTicks`, `PlotsInTheModel.Holds` and `KpiPanel.TickFor` all use. Ticking a row and unticking
it again puts the ticks back where the list press left them, so the line goes on its own: the
question is whether the ticks that press produced still stand, not whether anybody touched a box.
A row swapped for another is out of date although the count did not move, which is the case a
counter would have read as fresh and which has its own test.

### The results panel reads the report's own rows

**THE PLOT LIST's rows were built inline in `KpiCreateReport` for nine rounds**, which was right
while the report was the only thing printing them. A pane showing how many plots are ready needs
the same answer, and a second loop working it out beside the button is two records of one fact.

`PlotListRows.Of` is that record now. It carries facts rather than printed cells, so the report
writes YES, NO and NOT READ into its columns and the pane writes its own words, and neither holds
the other's spelling. `KpiResults.Of` reads those rows for the two counts and the not ready lines,
and **the reason on each line is the report's own, word for word**.

The panel holds the two counts side by side, every plot that is not ready with its plot, its
PRX_Plot_UID2 and its reason, a button to the output folder, a button to the report, and a way
back to step 3. **Every plot that is not ready, not the first few**, because a panel showing some
of them is a panel somebody has to open the report behind anyway.

### The report's own bytes did not move, measured rather than asserted

The extraction is a refactor of the report and the round's rule is that the report is untouched,
so it was measured. A probe built a run set of five listed plots over three outcomes and wrote
the whole report with `KpiCreateReport.WriteAll`, once with the change and once with
`KpiCreateReport.cs` stashed back to main's version. **144 lines, md5
`e1bc564f0f5910159bc28da79fe3e11e` both ways, and `diff` found nothing.**

### What the pane gained and what it kept

**Every folder, file, field, button and named line the pane held is still reachable and not one
word of any of them moved.** Checked by listing every literal string the old pane put on screen
and every Core method it called, and holding both against the new file: no string lost, and
`CreateWords.TemplateRow` reached through `KpiWillWrite.Rows` rather than directly, which is one
record and not a second.

**AND FOUR OF THEM ARE REACHABLE WHERE THEY WERE NOT.** The old block returned early three times,
at no templates folder, at a folder holding no workbook and at nothing ticked, so **the output
folder, the forms folder, the street reference and the plot list could not be browsed for at all
until a template had been picked.** Step 1 shows all five at all times.

What moved, and nothing else did:

```
the five browsed lines and the three typed boxes      to step 1
the plots block, Read this model and the link note    to step 2
the workbook rows, the plot ticks and Tick the list   to step 3
TickingTheList.Lines and the shared UID2 lines        from above Create to step 3
the three pickers and the reference values            to step 3, under the tick lines
everything else above Create                          stayed in step 4
```

**The two blocks that moved from above Create to step 3 are about what is ticked**, which is that
step's subject, and they were drawn over a button whose step no longer holds them.

**The three pickers are mine to place and the brief did not name them.** They need a read, and the
reference values block shows the FIRST TICKED plot, so in step 2 it would always read that nothing
is ticked and the Reference picker would lose the values it exists to be picked by. That is a
decision rather than a measurement.

**One line was dropped and put back.** `GRP KPI Checklist templates` headed the folder line and
the list together, and the folder went to step 1 while the list stayed in step 3. It heads the
list. The string check is what found it.

### What is UNOBSERVED, and what Bader has to look at

**NOTHING IN THIS REPOSITORY CAN OPEN A DOCKABLE PANE.** The whole of the layout is drawn by code
nobody has run. What is tested is the rules behind it, in Core, with every expected value written
out by hand. What is not tested, and cannot be from here:

- the bar at about 300 pixels wide, four cells across, and whether the cells read
- one step at a time, and whether pressing a cell moves where it should
- the two counts side by side in the results panel
- the two buttons really opening the output folder and the report
- the step moving by itself after a read lands and after a press finishes
- the outer scroll position when the step changes, which nothing resets

**Checks 34 to 37 in `steps/2026-09-16-kpi-checks.md` are what look at each of them**, one action
a step, and they are the first time any of it is seen.

### The break watches

Three, each restored byte for byte.

**Create reachable with nothing ticked.** `tick.Done` to `true` at the Create step's usable
argument. Five tests went red, `WithNothingSetOnlySetupCanBeWorkedOn`,
`WithEverythingSetAndNothingReadTheReadStepIsTheOneToWorkOn`,
`WithOneReadAndNothingTickedCreateNamesBothMissingTicks`,
`AWorkbookTickedWithNoPlotNamesOnlyThePlot` and `PlotsTickedWithNoWorkbookNameOnlyTheWorkbook`,
each on the assertion that Create cannot be worked on. `KpiSteps.cs` restored, md5
`4f2ab6fcf76bfc0024d875b546c53b10` before and after, **which is the hash it carried at that
moment and not the one the round ends on**: the emoji characters came out of its comment
afterwards and it ends at `a269aa45c81b86a0ab127d2cedb4ba66`.

**A changed workbook row keeping a fresh list tick.** `OutOfDate` returned false outright. Six
tests went red including `ARowTickedAfterThePressIsOutOfDate` and
`ARowUntickedAfterThePressIsOutOfDate`. The set comparison was broken a second way first, to a
count comparison, which reddened exactly the two that pin the set,
`ARowSwappedForAnotherIsOutOfDateThoughTheCountDidNotMove` and
`TheRowsAreComparedTheWayThePaneComparesThem`. `ListTickFreshness.cs` restored, md5
`b5e81db632f73480623a1d85b800dfec` before and after.

**The results panel counting a NO row as ready.** `Ready` counted rows whose workbook landed
rather than rows that are READY, which is the four columns of YES the 16:37 press printed.
`ThePanelCountsOneHundredAndTwentyTwoReadyAndThirtyTwoNot` read 123 against 122, and
`ThePanelsCountAndTheReportsReadyLineAreTheSameNumber` went red with it, which is the pane and
the report parting. `KpiResults.cs` restored, md5 `054feb70c06369244b2c79f0b85012b7` before and
after.

### What is left open

**Whether ticking a workbook row and unticking it back should say the list tick is stale.** The
rule reads the ticks rather than the touches, so it says nothing, and a test writes that out as
the deliberate reading. If Bader wants any touch to say so it is one line.

**Whether `KpiSteps` and `PanelSteps` should be one shape in Shared.** They are two panes' steps
with different meanings, so this one is written in Core/Kpi rather than called across the fence,
which is what `ScrollMemory` already did and is recorded the same way. It is a Shared round for
Bader to call.

**Whether the link note belongs in step 2, step 4 or both.** It is in both today: step 2 is where
the read that learnt it is and step 4 is where it has always been, and the two steps are never on
one screen. One fact, `_facts.Links`, drawn twice.

### What an adversarial review of this round's own diff found

Six lenses over the diff, every finding verified against the source by a second pass, then
judged here one at a time. **EIGHTEEN CAME BACK CONFIRMED AND SIXTEEN ARE FIXED IN THIS ROUND.**
None was found by a test, because every one of them is about what the pane does rather than
what Core computes.

**THE WORST OF THEM: THE PRESS HID THE CONTROL THAT ANSWERS IT.** The reconciliation refusals,
the buttons naming a plot's two regions and the groups left out note lived only inside
`TheCreateButton`, which the results panel replaces. A press refused over a region nobody picked
showed its own refusal with nothing on screen to answer it, and `TheRegionChoices` is the only
writer of `_chosenRegions`. `TheAnswerable` is one method now, drawn by the results panel and by
step 4's own controls, so the two can never offer two different sets of questions about one
press.

**OPEN THE REPORT WAS DEAD ON EVERY PRESS.** `KpiRequestHandler` passed
`ReportPlaces.Written(written)`, which builds `Report at <path>.` for the status line, and the
button tested it with `File.Exists`. It passes the path and the status line still gets the
sentence.

**THE LIST TICK RAISED ITS OWN WARNING.** The rows were recorded before the tick, and
`TickedByHand` reaches `Changed` and then `Preselect`, which can tick a workbook row, so the
press went out of date the moment it finished. `PressTickTheList` records what the press LEAVES.

**AND THREE MORE WAYS IT LIED.** Select all and Clear replace every tick exactly as Tick the
list does, so they forget it rather than leaving it standing over ticks it no longer describes.
Browsing to another templates folder clears the workbook rows, so a list tick recorded against
the old ones reported a drift nobody caused. A park template pick and a workbook row tick move
the plot ticks without going through `Changed`, so the results of the press before them outlived
the ticks they were made from.

**A STEP THAT CANNOT BE WORKED ON CANNOT BE DONE.** A model closing leaves the ticks standing
and takes the read away, so the bar read `3 Tick done` beside a step saying the model had not
been read, and step 4 opened on the strength of it. A model change clears the run, the confirm
and the region picks with it, because one model's plots read against one model's templates are
not another's.

**THREE SENTENCES THAT WERE FALSE.** A plot list set and unreadable said no plot list file was
set, which contradicts the report word for word and is the absence against refusal rule this
tool already carries. A list that reads and names no plot said every plot on it is ready. And a
not ready line said a plot holds no `PRX_Plot_UID2` whenever it simply had not been ticked,
which is a sentence about the model that nothing measured.

**TWO THINGS ABOUT THE WIDTH.** The done mark sat inside the cell text and was the first thing
trimmed at about 70 pixels a cell, so a finished step read SHORTER than an unfinished one. It is
its own line under the name. And step 1 printed the whole not set sentence in its header and
again at its foot, the same words twice on one screen, which is the shape a run already printed
four lines twice over. The header carries a count and the step carries the naming.

**TWO ARE RECORDED RATHER THAN FIXED, AND BOTH ARE FOR BADER.**

- **THE WORKBOOK ROWS CANNOT BE REACHED WITH NO MODEL OPEN.** Step 3 holds them and the round
  says Tick cannot be worked on before a read, so ticking MOSQUES and then pressing Read, which
  `kpi-rules.md` recorded as the ordinary order, is no longer possible. **The loop is live in
  the code and dead on the screen.** It is his own gate, so it stands, and it is one argument in
  `KpiSteps.Of` to reverse. The rules file says so now rather than reading as current.
- **TWO BRANCHES OF `CreateWords.CannotCreate` CANNOT FIRE FROM THE PANE ANY MORE**, its no
  template ticked and no plot ticked lines, because step 4 is shut until both are ticked and its
  own why not says the same thing first. The two other branches, no model open and no output
  folder, still fire. Nothing is deleted, on the rule this repository already carries about a
  member whose last caller stopped calling it.

**AND TWO OF THE EIGHTEEN WERE NARROWED BY THE VERIFIER RATHER THAN TAKEN AS WRITTEN.** The
identical areas confirm was named as hidden by the press and is pre existing dead UI: the
handler builds one reconciliation per plot and `KpiMerge.IdenticalAreas` needs two readings
sharing a raw area, so it has never drawn. And the region pick example given, NS-19, does not
reproduce, because `RegionChoice.Pick` takes the type the client's note names where both hold an
area. The fix stands for the case that really is reachable, two regions with the note's type
among neither.


### What the claim checker flagged

Run over this entry before the pull request was opened.

**ONE WRONG, AND IT WAS RIGHT.** The entry read `Create is usable once something is ticked`,
which is the round's own wording, and the code is stricter: `tick.Done` is a workbook row AND a
plot, so 154 plots ticked with no workbook leaves Create shut and
`PlotsTickedWithNoWorkbookNameOnlyTheWorkbook` says so. **A plain English summary that
contradicts the test under it is worse than no summary**, and the bullet says both ticks now.

**ONE ABSOLUTE SOFTENED.** `the Create button's own gate has always been` was a claim over all
history and only main and today were read. It says on main and today.

**SIX MARKED UNBACKED, AND ALL SIX WERE MEASURED HERE WITH A SHELL THE CHECKER DID NOT HAVE.**
Its tools are Read, Grep and Glob, so it could not run the suite, the hook cases, `md5sum` or
`git show origin/main`. Each is measured in this session and the measurement is what the entry
rests on: 2204 and 1384 off `dotnet test`, 28 off `hook-tests.sh`, the three hashes off
`md5sum`, the report's identity off two writes of one run set and a `diff`, no string lost off
every on screen literal in main's own pane held against the new one, and the three early returns
off `git show origin/main` read line by line. **A check that could not be made is an absence and
not a finding**, which is the rule this tool already applies to its own columns, and it is
written down here rather than left looking like a gap.

**EVERYTHING ELSE BACKED**, including the audit count recounted independently to the same 80, 23
and 57, the emoji range covering the tick, both files carrying no character from it, all eleven
named tests existing under their exact names, and the two break watches re-derived by hand
against the real test bodies to the same six red and the same 123 against 122.


### The audit files, counted again

```
audit-kpi.md      findings  1 to 29    29     8 FIXED   21 open
audit-kpi-2.md    findings 30 to 49    20     9 FIXED   11 open
audit-kpi-3.md    findings 50 to 63    14     2 FIXED   12 open
audit-kpi-4.md    findings 64 to 80    17     4 FIXED   13 open
                                       80    23 FIXED   57 open
```

Counted by walking each file's own numbering run rather than off a note. **Two numbered lines are
prose and not findings**, `audit-kpi-4.md` line 19 opening with `80.` and its line 13 FIXED
sentence, which counts the three files before it. Nothing was closed, renumbered or reordered.

---

Older entries were moved whole, byte for byte, on 2026-10-28: `steps/history/log-kpi-1.md` to `steps/history/log-kpi-7.md`, newest first.
