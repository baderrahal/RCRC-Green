# ai-max state, KPI

Phase: 9, ship. Fortieth pass, two guards off the audit and nothing else. On branch
`claude/inspiring-allen-xs113f`, 927 tests locally, 0 failed and 0 skipped, 904 before and
23 added.

**Audit findings 1 and 8 are fixed and the other 27 in steps/audit-kpi.md are untouched**, not
renumbered, not reordered, not annotated.

**Nothing may delete a template.** Two guards on one comparison, `FilePaths.Compare`, one in
`KpiRequestHandler.Patched` before the delete and one in `WorkbookPatcher.Patch` before it opens
anything, both refusing with the one sentence in `CreateWords.WouldOverwriteTheTemplate`. Three
answers rather than two, so a path that cannot be resolved refuses the same way an equal one
does. The comparison is textual and its limit is written down: a link or a substituted drive
still reaches one file under two names. When the output folder and the template folder are one,
the pane says so before Create is pressed.

**A refused run says why.** `WhyNothingWasWritten` is never empty. Accounting first, then the
patch's own refusal, then a line saying nobody recorded a reason and that it is a bug. The test
is that no run with `Written` false can produce an empty status line.

Both guards were watched red before being trusted, three tests each, restored byte for byte.
**Nothing in this round has been observed in Revit**, and the Revit side guard is reached by no
test at all.

Before that, the thirty ninth pass, an audit of the KPI tool alone. It built nothing and
fixed nothing.

Twenty nine findings are in steps/audit-kpi.md, ranked by cost: two BLOCKS, six WRONG, thirteen
COSTLY, eight TIDY. The two BLOCKS: nothing stops the browsed output path from being the
template path, and the delete that clears the way for the copy would destroy the client's
template. The other is that the three schedule readers still fall back to a cell position when
the heading row names no column, which is the rule CLAUDE.md states and the fault it names three
rounds running.
The largest of the WRONG is that `Preselect` in the KPI pane can never run past its own guard,
so the component table, the prefix cross check and the line saying which route chose the
template are all unreachable in the shipped pane. Three deliberate breaks: the read back and the
binding of a value to its cell both left the suite green at 904, and the species group merge went
red, so that rule alone is covered. All three hooks fired and blocked when probed. The audit ran
at `92dd36c`, where the suite reads 904, 429 of them KPI. `steps/ai-max-state.md` is in the
commit only because the commit hook requires it in every commit, which the audit's
touch-nothing-else rule had to give way to.

Before that, the thirty seventh pass, four things off the first real run. Pull request 46 is
merged into main as `6f2e521`, 904 tests on the runner against its merged head and 904 locally,
0 failed and 0 skipped on each.

**Excel showed zeros where the numbers were right.** Stale cached results in the output, and
`fullCalcOnLoad` alone was never enough. calcId set to 0, the cached `<v>` dropped from every
formula cell, `xl/calcChain.xml` removed, and `CacheCheck` reads all four facts back off the
output the way the written cells already are. 37 parts in and 36 out, and the report says which
part went and why.

**Unmatched species go into the workbook**, which reverses last round's rule. Name in column D,
count in column B, nothing else, into the empty rows the sheet's own total sums. The empty rows
come from the file, never from the map: MOSQUES stops at row 83 and B93 sums B4 to B92.

**The plot prefix is a second route to the template and does not decide.** PRX_Component decides,
the prefix cross checks, and where they disagree neither wins. What it is really for is grouping,
one button per template beside Select all and Clear. It is the only thing that can place EP-05,
EP-11, EP-12 and EP-13, which are on a schedule and on no sheet.

**The output folder is browsed for.** Writing beside the model meant a detached model could not
be used at all. Create no longer asks whether the model has been saved and the model's folder
came off `OpenModel` with that question.

**Two things to raise**: three of the seven asked for were already delivered in pull request 44
and were not done again, and the ask says eight prefixes where its own table lists ten. Ten are
built and tested.

**Open for the team**: the client's species lists are short of trees this project plants, and a
species written into an empty row carries no family, no genus and no native flag, so the KPIs
that need those still cannot see it.

Before that, the thirty sixth pass, the first real workbook. Pull request 44 is merged into main as
`116afdb`, 867 tests on the runner against its merged head and 867 locally, 0 failed and 0
skipped on each.

**The first workbook is written and correct.** DM-12 on MOSQUES, 37 parts in and 37 out, zero
recalculation errors, and the client's formulas gave 28.1 percent canopy against a 13 percent
target. Two things it showed: the three typed boxes reached nothing, because `KpiCreateAsk` did
not carry them and `KpiCreatePlan.Of` defaulted them to null, and the report claimed every number
came off a schedule row when the area comes off a filled region parameter. Both fixed, the three
are required arguments now, and the region row prints raw, converted and printed side by side.

**One open question for the team**: the client's MOSQUES tree list holds 80 species and none
matches three the model holds, so the workbook reads 31 trees where the model holds 39. The tool
is right and the list is short. Nothing may ever place an unmatched species by guessing.

Before that, the thirty fifth pass, two more off the KPI pane. Pull request 43 is merged into main as
`61ea270`, 857 tests on the runner against its merged head and 857 locally, 0 failed and 0
skipped on each.

The reference sample showed the first plot in the model rather than the first ticked one. And
Create stayed grey after the model was saved, which had never worked: the folder was a copy taken
once, the title was a second copy on a different schedule, and the pane's own request for the
model was being thrown away by a one slot queue. **A PANE HOLDS NO COPY OF ANYTHING IT CAN ASK
FOR** is the rule that came out of it, and it is in `CLAUDE.md`. Create is greyed out on what the
pane owns and the model state is decided on the Revit thread when it is pressed. The tests are
over the decision and not over Revit.

Before that, the thirty fourth pass, five faults off the first real run of the KPI pane. Pull
request 42 is merged into main as `36dc0f8`, 849 tests on the runner against its merged head and
849 locally, 0 failed and 0 skipped on each.

Every one of the five is about what reaches the screen rather than what the code computes.
Create was handed the model's folder and called it the model, so a detached model read as no
model open. WPF ate the first underscore of every parameter name on a button, so the pane
offered five names no model holds. The pane described itself reading PRX_COMPONENT off the title
block, which is the workbook's note rather than anything the tool does. Reference and Location
preselected by position. Prepared by was cut to Prepared b. A sixth was reported, all 155 plots
ticked by default, and the code does not do that: the state is pinned by three tests and the
report is unexplained rather than closed.

Before that, the thirty third pass, the component to template mapping. Pull request 41 is merged
into main as `a0d3d3b`, 823 tests on the runner against its merged head and 823 locally, 0 failed
and 0 skipped on each.

The question open since the 1355 run is answered. `ComponentTemplates` is a table of the eleven
values the 1548 scan measured, many to one, and the word matching it replaces is gone. The two
park values break the park tie so each preselects its own template, the plot prefix is read
nowhere, and a value the table does not hold preselects nothing and now says so on the pane. The
road width inside a street value is recorded in `steps/log-kpi.md` and nothing reads it.

Before that, pull request 40 is merged into main as `5a5ded0`, 804 tests on the runner
against its merged head and 804 locally, 0 failed and 0 skipped on each. The rule behind the
last three rounds is written into `CLAUDE.md`: never read a
schedule value by cell position, ask the heading row which column it is, and a reader that
cannot find its column says so rather than falling back to a position. Both rules files point at
it. No code change, so the second half of it is open: four readers still fall back to a position
and `steps/log-kpi.md` names them.

Before that, the thirty second pass, the group counter, the switch and the component values.
Pull request 39 is merged into main as `973c817`, 804 tests on the runner against its merged
head and 804 locally, 0 failed and 0 skipped on each.

The group counter counted what sat under a phase off the first cell, which is the image column,
and an existing species prints with no photo, so DM-12 Existing came back as 0 named rows of 6
while the same file printed its five species. Both readers now read the botanical column the
heading row names. The shrubs reader held the same fault unseen. Every place in Core that reads
a printed row is listed in `steps/log-kpi.md`, the ones already right included.

A near miss whose values all read Yes or No is a switch and is left out of the section 9 answer
and kept in section 3. And section 3 now prints every distinct value of PRX_Component with its
sheets and its plots, because the five measured values are not template names and **which
template each one means is open**.

Before that, the thirty first pass, the shrubs and lawn shape measured rather than guessed.
Pull request 38 is merged into main as `82dd51d`, 789 tests on the runner against its merged
head and 789 locally, 0 failed and 0 skipped on each.

The team supplied the real rows. The group heading sits on its own row with a phase row under
it, and **the subtotal prints twice**, so adding a group's subtotal rows gives double. One is
taken and two that disagree refuse the write. The area, the count and the botanical name come
off the columns the heading row names rather than off assumed positions, because the schedules
are eleven columns wide. An area carries its unit and can be nought. The shape is now in
`kpi-rules.md`, sourced to the 1355 scan report, which is not in this repository.

Before that, the thirtieth pass, the plot picker, several plots at once, and the Create button.
Pull request 37 is merged into main as `ce825b1`, 781 tests on the runner against its merged
head and 781 locally, 0 failed and 0 skipped on each.

The button the last four rounds deliberately did not add. The pane gains a plot picker offering
one plot, several or all of them, the three choices the model cannot make, three typed boxes,
and Create, which copies the chosen template, patches it and writes a report beside the model.
It creates nothing in the model and never writes to the template.

**Which plots belong to one checklist is not written down anywhere and nothing derives it.**
The user ticks them and the tool adds up exactly what was ticked. Adding numbers the schedules
printed is allowed and recomputing one off elements is not. Every plot's own number is printed
beside every total and a total that does not equal its parts refuses the write, as do two plots
reporting an identical area and one plot whose two regions both hold one. Species merge on the
group AND the name. Matching is plain, so UNKNOWN, the slash and the apostrophe all come back
unmatched and named rather than forced onto a row.

Nothing in this round has been through Revit. The log lists every unobserved thing item by item.

Before that, the twenty ninth pass, two report faults from the 1355 scan and the facts it
measured. Pull request 36 is merged into main as `01d9886`, 722 tests on the runner against its
merged head and 722 locally, 0 failed and 0 skipped on each.

Both faults were the report answering a question from the wrong place while the right answer sat
a screen above it in the same file. Section 9 read NOT FOUND for PRX_COMPONENT while section 3
printed PRX_Component with 1384 values, so a near miss holding values is now named and counts as
the answer. Question 8 reported the elements by phase created, which are the link instances the
softscape schedule lists rather than the plants, so it is answered from the printed group rows
instead, by `ScheduleGroups` in Core, naming the plot that showed it. Six measured facts are in
`CLAUDE.md`, which was held at 200 lines by compressing the Drawing Sheet prose `core-rules.md`
already holds. Two new open questions, both about matching a species by name, are in
`steps/log-kpi.md`, and four of the five older ones are settled.

Before that, the twenty eighth pass, recognition fixed, the scan gaps, one check against the
real workbooks, and the review of that work folded back in.

Pull request 35 is merged into main as `1b78c91`, 711 tests on the runner against its merged
head and 711 locally, 0 failed and 0 skipped on each.

A five lens review of the round below raised 58 findings, 24 were put to a reader whose job was
to refute them, 14 survived and those are six distinct faults, all fixed. A plot's regions were
counted off the area values so a region with no area vanished from its plot. A filled region
type carrying no plot had no row, so the answer none could not print. Absent and blank both
printed as an empty plot with nothing saying which. The near miss block printed once per
missing name and listed a wanted name as a near miss of itself. One schedule was recorded as
both passed over and read in full. And `kpi-rules.md` held both the old one per name rule and
the new one. The log entry's own claim of eight red tests was wrong as well, reproduced at 14,
and is corrected. 711 tests locally, 0 failed, with four breaks watched red.

The KPI scanner ran on the real model for the first time. Recognition was reading 0 of 7,
because the map held the main sheet names with the angle brackets stripped and the brackets are
part of the name. Fixed for all seven with a test that asserts the shape. A one-off check
against two real EXISTING PARKS workbooks supplied in a chat session, unrepeatable and covered
by no gate, agreed with the map on all six cells, proved the patcher keeps 37 of 37 parts while
an object model loses 21, and confirmed the tree rows. Four scan gaps closed: a near miss is
now shown as well as named, the four plot parameters print side by side, every filled region
prints its plot, and three plots per schedule name are read rather than one. The measured facts
and the five open questions are in `.claude/rules/kpi-rules.md`, because CLAUDE.md is at its
200 line ceiling. 710 tests locally, 0 failed, up from 657, with three breaks watched red.

Before that, the twenty seventh pass, the template picker and the workbook writer. Pull
request 33 merged into main as `c88083f` with 696 tests on the runner, 0 failed and 0
skipped, 657 locally before the merge that brought the divided sheets rounds in and 696
after it.

The workbook half of the KPI tool. `KpiTemplates` in Core carries the map, one entry per
template, measured off the annotated seven, with the tree row ranges on the entry because a
constant wrote quantities into rows no total sums. `RecognisedWorkbook` settles five templates
on the main sheet name, breaks the park tie on the file name and puts an unsettled pick to the
user. `WorkbookPatcher` copies the zip and patches only the cells that get a value, through
the platform's own zip and XML types with no third party assembly, after a live licence search
found EPPlus moved to Polyform Noncommercial and the object model libraries lose parts. It
sets recalculate on open and reads every written cell back off the output. The pane gains the
template block below the unchanged scan block, with no fill button, and `KpiFillValues` is the
empty seam the next round fills. 657 tests locally, 0 failed, up from 580, three breaks
watched red. Nothing reads the model and nothing fills.

Before that, the twenty fourth pass, two numbers the cut-off brief left to a guess, corrected.

The KPI row cap goes from 30 to 200, because the workbook's softscape lists run 80 to 89
species and a cap of 30 lost about 55 of them from the section this round exists to fill. TREE
joins the workbook words beside SOFTSCAPE, SHRUB, LAWN and HARDSCAPE. Both numbers were choices
made without a rule when the brief arrived cut off, both were logged as open questions, and
this is the team's correction. Pull request 31 merged as `b9559ea` with 580 tests on the runner, 0 failed
and 0 skipped, matching the local run. Nothing else changed.

Before that, the report for the KPI scanner round.

Pull request 28 merged into main as `f40b40a`, a squash of four commits, and the gate executed
579 tests against it on the runner, 0 failed and 0 skipped. It merged the current main first,
because pull requests 29 and 30 landed while it was open, touching only Drawing Sheet files.

Before that, the twenty-third pass, the KPI scanner, first round of the second tool. Locally,
after the last file was written, the build came back with 0 warnings and 0 errors and the tests
with 541 passed and 0 failed, up from 398, then 579 with the current main merged in.

**The KPI tool** will one day fill the client's GRP KPI Checklist workbook from a model and will
never create anything in it. This round is the scanner: a second ribbon panel, KPI, with one
button, KPI Checklist, opening a pane of three things, the model name with when it was last
read, KPI Scan and a status line. KPI Scan reads the whole document through its own external
event and handler into a `KpiScan` of plain values, and `KpiReport` in Core writes nine numbered
sections, one per question the workbook raises, every heading carrying its own count, with a
READS THAT DID NOT HAPPEN block above section 1. `KpiQuestions` is section 9, one line per
question saying FOUND or NOT FOUND and where to look.

The brief arrived cut off partway through section 4 of the report. Sections 1 to 4 are as
specified and 5 to 9 were designed here from questions 6 to 9. The choices made without a rule
are numbered in `steps/log-kpi.md`. Nothing in this round has been through Revit and no KPI file has
ever been written. A five lens review raised 76 findings, 28 were sent to a skeptic each, 23
were confirmed, 21 distinct, and all 21 are fixed in the third commit with the two report fixes
queued before it. The 5 refuted and the 48 unverified are listed in the log entry, one line
each, and stay out of the code. `RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile`
are unchanged.
