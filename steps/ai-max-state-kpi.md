# ai-max state, KPI

Phase: 9, ship. Fifty first pass, the five that cost a whole run. **Findings 34, 35, 39, 9 and
16 are FIXED and marked under their entries. The other 38 audit findings stay open**, not
renumbered, not reordered, not annotated. All five stood at today's lines before the fix, and
the round before this one, findings 3, 4, 6, 12, 21 and 26, had not landed on main, so 35 is
built fresh and finding 6's catch is untouched.

**A throw on one plot names the plot and the run carries on**, `GuardedRead` in the handler
and a guard per schedule in the reader, the refusal on the reading and the report written. **A
choice made after a refusal is applied to the run already read**, `HeldReadings.Decide` and
`Applied`, the model read again only for a named reason and the report saying which, a plot
whose read was refused among the reasons. **A filled checklist is named as filled and not
offered**, off E5 against `KpiTemplates.DatePlaceholder`, in the same list, nothing deleted or
moved, and a filled park file whose name cannot tell the park names neither. **The templates folder is opened once per folder**,
`TemplateListing`, the opens counted on the pane and in the report. **The plot list keeps its
place across a tick**, `Scrolling` and `Remembering` in the KPI pane with `ScrollMemory` in Core.
The read cost is in the log in calls and is unchanged.

Eight break watches went red on 4, 2, 2, 2, 6, 2, 2 and 2 tests and were restored byte for
byte. **Locally 1153 tests at this branch, 0 failed and 0 skipped, 638 of them KPI, 50 added,
against the 1103 main carries.** The pull request, the merge hash and the runner's count go in the
record the merge adds here. **Nothing in this round has been observed in Revit.** Three things
wait on Bader: whether all seven production templates hold `<Date>` at E5, whether a model
edited between a refusal and a pick matters, and whether the scrolling helper becomes a shared
Revit root file in a round of its own.

Before that, the fiftieth pass, one word too loose, measured on the 1707 run. **The other 43
audit findings stay open**, not renumbered, not reordered, not annotated.

**Where two headings hold DIAMETER, the column the sheet's own formulas read is taken.** J
Average Mature Canopy Diameter (m) and K Mature Canopy Diameter (m) both hold the word, the
reader found two and wrote nothing into J, and the canopy guard deleted the output. L reads J
and nothing reads K, so `SpeciesList` reads the formulas below the header row off the file and
takes the one candidate they read, worked out from the file and never from a letter. Still more
than one, or none, and nothing is written with both named and what the formulas read. How each
column was chosen is recorded and printed beside the tree list. PHOENIX DACTYLIFERA writes 18
into I and 15 into J, UNKNOWN writes neither and still refuses through the guard. **The report
counts the matches that disagree**, 22 of them on the 1707 run, and changes nothing.

Three break watches went red on 3, 2 and 2 tests and were restored byte for byte. Pull
request 69 is merged into main as `9e966fa`, **1103 tests on the runner against its merged
head and 1103 locally at that head, 0 failed and 0 skipped on each**, 588 of them KPI, 8 added
here. **Nothing in this round has been observed in Revit.**

Before that, after the forty ninth pass, one fixture correction: ST-05's Existing group is
the thirteen species measured off the schedule on screen, adding to 369, in place of an
assumed split of two. CASSIA GLAUCA under Existing at 1 and Street Design at 62 covers a species
in a named group and a by-decision group at once. No code changed. Pull request 68 is merged
into main as `5d062f6`, **1095 tests on the runner against its merged head and 1095 locally at
that head, 0 failed and 0 skipped on each**, 580 of them KPI, none added. **Nothing in this
round has been observed in Revit.**

Before that, the forty ninth pass, two things on top of the forty eighth, both Bader's
decisions. **The other 43 audit findings stay open**, not renumbered, not reordered, not
annotated.

**Street Design counts as Proposed on STREETS and stays out everywhere else.** ST-05 prints
Existing 369, Proposed 2 and Street Design 68 under TOTAL 439, and 369 plus 70 is 439. The
decision is data on the template, `KpiTemplate.GroupsCountedAsProposed`, keyed on the template
and never on the plot prefix, read by `CountedGroups.Of` into a `GroupByDecision`, and a test
says the whole map holds exactly one. The merge keys every row on the sheet that takes its
group through the same `CountedGroups`, so ALBIZIA LEBBECK 2 under Proposed and 6 under Street
Design is one row of 8 on Tree List - Proposed and trips no refusal. The report's reason beside
each group row says which route it took. **The note goes on the pane:** `CreateWords.GroupsLeftOut`
names the plots and the schedules where a group no sheet takes was found, above the Create
button, a note and not a refusal.

Four break watches went red on 10, 21, 3 and 4 tests and were restored byte for byte.
Pull request 67 is merged into main as `c79c4d2`, **1095 tests on the runner against its
merged head and 1095 locally at that head, 0 failed and 0 skipped on each**, 580 of them KPI,
14 added here. **Nothing in this round has been observed in Revit.**

Before that, the forty eighth pass, the FM-05 refusal answered off the 1536 report: its softscape
schedule holds THREE groups, and the third, Street Design, is somebody else's scope by Bader's
decision. The model will be corrected later and until then the tool leaves those rows out and
says so. **The other 43 audit findings stay open**, not renumbered, not reordered, not
annotated.

**Only the groups a tree list sheet is named for count.** `CountedGroups` holds the template's
two sheet names and nothing matches a word. The softscape reader takes the species under those
groups, checks each group against its own subtotal row and the groups taken plus the groups left
out against TOTAL: FM-05 6 plus 32 taken, 38 left out, 76 printed. The shrubs and lawn reader
adds the phase rows a sheet is named for and checks the group total against all of them: GRASS
96 taken, 69 left out, 165 printed, SHRUBS 361, 459, 820. **The report names every group row,
always**, with its row, its subtotal, TAKEN or LEFT OUT and why, and the accounting counts the
schedules holding such a group and names the plots. A species in two groups refuses nothing, in
one group still does. A repeated group name, DM-25's two Existing rows, is taken both times and
the second says so. The false comment in `KpiPlotReader` is corrected: FM-05 holds one softscape
schedule.

Six break watches went red on 18, 9, 3, 2, 1 and 2 tests and were restored byte for byte.
Pull request 66 is merged into main as `ee13e8c`, **1081 tests on the runner against its
merged head and 1081 locally at that head, 0 failed and 0 skipped on each**, 566 of them KPI,
21 added here. **Nothing in this round has been observed in Revit.**

Before that, the forty seventh pass, six things measured on the 20 plot MOSQUES run of
2026-09-10 at 14:28 and on its workbook opened in Excel. Five fixed, one reported and not
fixed. **The other 43 audit findings stay open**, not renumbered, not reordered, not annotated.

**A species written in carries its height and its diameter off the schedule**, read by heading,
agreed across every row, written into the columns the sheet's header row names, and named with
the reason where a dash, a nought or a disagreement means nothing is written. **The FM-05
double is two printed rows of one schedule**, not two schedules, and the write is refused naming
the rows and the counts until Bader says which. **The output's formulas are read for what they
will compute**, and a written row a formula cannot compute from deletes the output again and
refuses the run. **calcMode is auto and the fifth check**, with the package searched for any
other calculation setting. **The _xlfn. functions are counted** by name and by cell. **The report
ends with every schedule read, as the schedule prints it.** Phoenix dactylifera's two diameters
are named and nothing is changed, an open question for Bader.

Five break watches went red on 4, 1, 2, 2 and 5 tests and were restored byte for byte.
Pull request 60 is merged into main as `8eb289b`, **1046 tests on the runner against its
merged head and 1046 locally at that head, 0 failed and 0 skipped on each**, 557 of them KPI,
32 added here. **Nothing in this round has been observed in Revit or in Excel.**

Before that, the forty sixth pass, two faults measured on the first twenty plot run, 2026-09-10
at 11:16, and fixed. **The other 43 audit findings stay open**, not renumbered, not reordered,
not annotated.

**The tree list rows come off the file and the map holds no range.** The map said B4 to B83, the
sheet's total said SUM(B4:B92), and the names ran to row 101, so four species with a row waiting
were reported as having nowhere to go and the workbook went out 85 trees short. `TreeSheet`
holds a sheet name and nothing else, `SpeciesList` reads every named row down column D and the
total's own reach off its formula, and a species on a row the total does not reach is refused
per species with the row and the total named. The report prints both lists as read.

**One schedule of a kind per plot or the kind is not read.** FM-05 holds two softscape
schedules and its trees were counted twice. The reader counts before it reads, `PlotReading`
carries every name and refuses numbers beside two, the reconciliation refuses the write naming
the plot, the kind and every schedule, and the report names the schedule each number came off.

The read cost is measured in calls in the log, every plot walking all 951 schedules, and is not
changed. Three break watches went red on 5, 1 and 1 tests and were restored byte for byte.
Pull request 59 is merged into main as `48bd623`, **1014 tests on the runner against its
merged head and 1014 locally at that head, 0 failed and 0 skipped on each**, 525 of them KPI,
20 added here. **Nothing in this round has been observed in Revit.**

Before that, the forty fifth pass, four findings that can put a wrong number in front of a
client: finding 2 of the first audit and findings 30, 31 and 32 of the second. **The other 43
stay open**, not renumbered, not reordered, not annotated.

**The four cell position fallbacks refuse.** Every reader of a printed schedule hands back what
it read or every reason it refused, never both, and a column the heading row does not name is
refused in one sentence that names the column and prints the headings. The refusal travels on
the plot reading, refuses the write, prints under the plot and among the reasons, and the pane
draws it in red above Create.

**A digit after the number ends is a refusal.** `CellNumber.Read` refuses "1,234 m2" and
"1131,72" with the cell named rather than reading 1 and 1131, parses no separator because the
project's setting has never been read, and every value measured on the real model still reads.

**STREETS reads no area.** The reconciliation takes the template as a required argument, and
where the map holds no area cell no filled region is read and nothing about the area is refused
on, so MM-03 and MM-04 reading one raw area no longer end the first 78 plot run. The report says
the area was not read and why.

**The TOTAL row is read.** The species rows are held against it and a sum that does not match
refuses the write naming both numbers. DM-12: eight rows adding to 39, TOTAL 39.

Four break watches, one per finding, went red on 2, 6, 4 and 2 tests and were restored byte for
byte. Pull request 58 is merged into main as `faa6468`, **994 tests on the runner against its
merged head and 994 locally at that head, 0 failed and 0 skipped on each**, 505 of them KPI, 46
added here. The 988 measured while building was at the older base, before the Drawing Sheet's
forty fourth pass put six tests under it. **Nothing in this round has been observed in Revit.**

Before that, the forty second pass, the second audit of the KPI tool alone. It built nothing
and fixed nothing. Pull request 55 is merged into main as `5beea47`, 942 tests on the runner
against its merged head and 942 locally, 0 failed and 0 skipped on each, nothing added because
nothing changed. The only file it adds is `steps/audit-kpi-2.md`, and this file is in the
commit only because `require-file-on-commit.sh` requires it in every commit.

**Part A first: all 27 open findings of `steps/audit-kpi.md` STILL STAND at `82d95f4`**, with
today's line numbers, none passed by when the code moved and none found untrue. Twenty new
findings, numbered 30 to 49 so the two audits cite together: 0 BLOCKS, 3 WRONG, 7 COSTLY, 10
TIDY, and 9 dropped as costless. The shape hunted was a right line standing on a fact measured
once, and the logic notes list every such rule in the tool with what it was measured on.

The three WRONG. `CellNumber` reads a digit grouping separator as the end of the number, and
every value it has met printed under a thousand, so on a project that groups digits a one phase
group over 999 m2 writes its thousands and the add-up check passes because 1 equals 1. The area
is read, totalled and refused on for every template and STREETS has no area cell, so the first
78 plot run will refuse on MM-03 and MM-04, measured identical on the 1355 scan, over a number
it is not going to write, and read all 78 again after the confirm. The softscape TOTAL row is
printed by the schedule and read by nothing, so a species row the reader drops with a bare
continue is invisible, which is how 31 trees reached a workbook where the model held 39.

Three breaks, each restored byte for byte with md5 and the suite rerun green at 942. The shrubs
and lawn swap and the read back echo stayed green as they did at 904, so findings 10 and 11
stand re-proved. Printing CELLS WRITTEN off the plan rather than off what landed stayed green
too, because no test builds a run that wrote. All four hooks fired and blocked when probed,
with one probe recorded as made wrongly. The two records of one fact tally is twenty one
instances, eight fixed, twelve standing, one held open on purpose. **942 tests at `82d95f4`,
459 KPI, and nothing was changed.**

Before that, the forty first pass, three things off the 0928 run, the first twenty plot run this
tool has done. Pull request 53 is merged into main as `b83e3d6`, 942 tests on the runner against
its merged head and 942 locally, 0 failed and 0 skipped on each, 927 before and 15 added.

**The subtotal rule was wrong and was measured from the one plot that is the special case.** A
group prints one subtotal per phase, then the group total, and the LAST row is the value. DM-11
groups each hold one phase, so each prints two equal rows, which read as one subtotal printed
twice. Taking the first took one phase and called it the group: 30 where the group is 84, 13
where it is 241, 96 where it is 165, 361 where it is 820. The 0928 run refused rather than
writing, so none of the four reached a workbook. The check now is that the last row equals the
rows above it added, in area and in item count. Corrected in `kpi-rules.md`, added to
`CLAUDE.md` as a project fact, and the old log entry corrected in place with the wrong claim
left standing above it. `CLAUDE.md` never carried the wrong claim, checked by grep.

**The run is timed.** The whole press, the model read apart from it, and each plot's own read,
all in the checklist report. A run nothing timed says NOT TIMED rather than printing nought. What
the slow part is stays UNKNOWN as a duration, because no Revit ran here, and is countable as
work: every plot walks all 1385 sheets with a full sort, all 951 schedules reading each one's
definition and filters, and all 279 filled regions. Gathering each once before the plot loop
would turn 19,020 schedule definition reads into 951 over 20 plots and 74,178 into 951 over the
78 street plots. **Nothing was changed. Measure first, then decide.**

**The refusal printed twice**, in red above the button and again in full in the status line. The
status line counts the reasons and points at the red block now. The patch's own refusal is still
said in full there, because nothing else on the pane carries it.

All three were watched red before being trusted, five, two and one test. Two tests that encoded
the replaced rules were rewritten rather than deleted. **Nothing in this round has been observed
in Revit.**

**Open for Bader**: what the component and the reference cells are for on a checklist covering a
whole template, where 20 mosque plots hold two component values and 20 references and both cells
correctly come out empty.

Before that, the fortieth pass, two guards off the audit and nothing else. Pull request 52 is
merged into main as `1430f22`, 927 tests on the runner against its merged head and 927 locally,
0 failed and 0 skipped on each, 904 before and 23 added.

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
