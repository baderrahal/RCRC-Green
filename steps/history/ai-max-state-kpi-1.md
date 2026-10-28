Phase: 9, ship. Ninety first pass, the two presses of 16 September read back, 16:06 with 166
plots ticked and 16:37 with exactly the 154 listed, seven items. **Merged to main as `65f7330`**,
pull request 150, the squash message set on the merge call and off main byte for byte with no
co-author line and no generated-by footer.
**2111 tests, 1291 of them KPI**, up from 2070 and 1250, 28 hook cases, build zero warnings.
**80 audit findings, 20 FIXED, 60 open**, counted per file off all four and nothing closed,
renumbered or reordered.

**NEITHER REPORT IS IN THIS REPOSITORY AND NEITHER MAY ENTER IT.** Every measurement below is
copied out of them or out of the workbooks they wrote. **NOTHING HERE WAS RUN IN REVIT**, and the
split of `OneTemplate` cannot be run from this session at all.

**TWO PLOTS, ONE FOLDER, AND THE LAST ONE WRITTEN REPLACED THE OTHERS.** NS-01 with NS-42 on
ANH-007-ST-100210 and MM-01 with MM-09 to MM-15 on ANH-007-ST-100213, ten plots at two paths, all
ten reading YES. `OneTemplate` is split into `ReadOneTemplate` and `WriteOneTemplate` with a
`TemplateReading` between them, so `Create` reads every ticked template, builds the filings, asks
`SharedUid2.Of` and only then writes. **Nothing about reading a plot changed.** `PlotFilings.One`
for the press and `PlotFilings.Of` for the pane, both through `PlotWorkbookPath.For`, and `OnePlot`
READS its path off the list. Files already there are named and left. The pane says it before the
press off `ReferenceValuesPerPlot`, which already carries all four parameters per plot, so item
1.4 is YES rather than UNKNOWN.

**READY IS THE QUESTION THE FOUR COLUMNS OF YES WERE NOT ASKING.** `PlotReady`, five things: both
files written, no tree written nowhere, no sourced PDF box blank, the plot's own PRX_Plot_UID2,
no #DIV/0!. A box with no source does not count against it. Why not names EVERY reason with its
box or its cell. A fourth count `ready:` and a glance line. `WhyNotOnTheList` deleted, its shape
gone rather than its caller, `ReadingFor` with it. `NoSoftscapeOnTheList` KEPT with no production
caller and named as one.

**A DIVISION BY A COUNT OVER A RANGE IS A DIVISION.** COUNT, COUNTA and SUM read, the range
counted off the cell states, the IF honoured, and one that cannot be worked out counted as NOT
EVALUATED rather than as none found. FP-24 and SC-06 hold one at S70 and T70. **A live scoping
fault came out with it**: `DefinedNames` kept `TotTrees` by name alone and is keyed on the scope
now with `localSheetId` off the workbook's own sheet order.

**A ROW HOLDING A COUNT MUST CARRY THE TOTAL CANOPY FORMULA.** FP-18 row 83, M83 empty, PDF 2,631
against Excel 2,531. `TotalCanopyColumns.In` follows the chain off the file and no letter is
written anywhere. An empty row is offered only when it carries both formulas, and a column not
read leaves every row usable rather than refusing them all.

**EVERY PRINTED CELL TEXT RESOLVES A SHARED STRING.** `D99 holds 419` where D99 holds Prosopis
Juliflora, through `WorkbookPackage.TextOf`, read once per file.

**THE PLOTS WITH NO PLANTING AT ALL ARE NAMED.** MM-01, MM-06, MM-07 and NS-23, off
`ScannedSchedule.BodyRowCount`. The noughts are right and READY does not move.

**A TICKED PLOT THE LIST DOES NOT NAME IS WORK NOBODY ASKED FOR.** 166 against 154 at 16:06.
`TickingTheList.TickedAndNotOnTheList`, a line above Create and a `TICKED AND NOT ON THE LIST`
block.

Six break watches, each watched red naming its subject and each restored byte for byte with md5.
Two tests went red on their own when the plot list row grew a column, and both were corrected by
hand with the new shape written out.

`steps/2026-09-16-kpi-checks.md` is the Windows run sheet for Bader.

**The claim checker was run over the log entry before the pull request.** It flagged one wrong
count, `SharedUid2Tests` holding seven tests where the entry read six, corrected. And it flagged
that the round message's 7 plots whose workbooks replaced each other does not reconcile with its
own item 1, which names ten plots over two shared values: every plot in a colliding group is 10
and only the plots whose file was lost is 8. **The number is removed from all four places that
carried it** rather than argued with, and what 7 counted is a question for Bader. Everything else
it could check came back backed, and it could run no build, no suite, no hook script and no md5,
which it said outright.

Open for Bader: the ten shared UID2 values are the model's fix, whether a stopped plot keeps its
street reference row is UNKNOWN and does not arise this press, and whether a #DIV/0! alone should
hold READY back is his call. `CLAUDE.md` is not wrong at any line this round touched.

---

Phase: 9, ship. Ninetieth pass, the 154 plot press of 15 September at 13:32 read back, seven
decisions of Bader's. **Merged to main as `4411821`**, pull request 148, the squash message set on
the merge call and off main byte for byte with no co-author line and no generated-by footer.
**2070 tests, 1250 of them KPI**, up from 2042 and 1222, 28 hook cases,
build zero warnings. **80 audit findings, 20 FIXED, 60 open**, counted per file off all four and
nothing closed, renumbered or reordered.

**THE REPORT IS NOT IN THIS REPOSITORY AND MUST NOT BE.** Every measurement below is copied out
of it. **NOTHING HERE WAS RUN IN REVIT**, and item 7's wiring cannot be run from this session at
all.

**A READ SCHEDULE WITH NO SUCH GROUP IS A NOUGHT, AN ABSENT ONE IS STILL BLANK.** 81 plots lost
their Lawn box and 15 lost all four shrub boxes. `PdfFill.NoGroupIsNought` and the flag
`NoughtForAnAbsentGroup`, which is what the glance counts, never the words, because item 6
rewrote every printed line in the same round. `KpiMerge.Subtotalled` writes the workbook's Lawn
and Planting cells 0 the same way. `WaterDemandRead.NothingToTotal` counts a schedule printing its
heading row and no body rows as 0, which is MM-01, MM-06, MM-07, NS-23, MM-08, NS-28 and NS-38.
Body rows and no TOTAL row still refuses. The nought is asked AFTER the L/DAY column is found, on
purpose, and the other order is recorded in the log as the choice not taken.

**A PLOT WITH NO SOFTSCAPE SCHEDULE IS NOT A ZERO.** FM-07 read 0, 0, 0 and 0 against 57 trees in
the model. Four boxes blank through `PdfFill.NoSoftscapeRead`, and THE PLOT LIST's why not column
says the same through `KpiCreateReport.NoSoftscapeOnTheList` even though both files were written.

**THE PARKS BOXES ARE 9.72 PT TALL.** 271 values held at 6, all on Parks.
`PdfTextFit.HeightMargin` is 1 pt top and bottom, the width stays 2. 3977.16 in 41.52 by 9.72
gives 7.7, worked by hand. A held value says whether its WIDTH or its HEIGHT held it and says it
runs over only when it really does.

**NEVER WRITE A NEW SPECIES INTO A ROW WITH NO CANOPY FORMULA.** FP-23 lost its Total areas to be
greened to row 85 of FUTURE PARKS Tree List - Proposed. `SpeciesList.UsableEmptyRows`,
`WorkbookArithmetic.IsTheCanopyFormula` asked by the guard and the reader alike, and the rows read
through `WorkbookFormulas.Of` so a shared formula is seen on all eighty rows and not on its master
alone. A sheet with no diameter column is not checked at all and `CanopyRowsRead` says so.

**TREES WRITTEN NOWHERE SHOW AT THE TOP.** 67 of them on four plots read YES and YES with nothing
said. `TreesNotWritten` counts them off the runs' own matches, THE PLOT LIST gains a trees not
written column and the glance gains THE TREES WRITTEN NOWHERE.
`SpeciesMatching.OnMoreThanOneRow` names the file, the sheet and both cells, D22 and D84. The tool
still does not choose between two rows.

**NO PRINTED LINE SAYS CLIENT.** 26 strings held it and none does. `RegionChoice.TheNote` is the
one record of the note itself. The canopy guard names the file, the sheet, the row, every formula
on it, every typed cell, and the one cell that would have carried the canopy, off the sheet's own
other rows. **What M85 is, is UNKNOWN and is not printed**, because nothing records that column's
formula.

**THE CRASH ROW.** `PlotCrash`, `CreateStep` and `CrashFile` in Core with five tests.
`PlotWriteTrail` in `KpiRequestHandler` records the step as it is reached, the folder flag reads
the disk, and nothing is deleted.

Six break watches, each watched red and restored byte for byte with md5. `steps/2026-09-15-kpi-rerun.md` is the sheet for the rerun: the fixes synced and a fresh detached
copy first, 23 steps, the seven workbook rows ticked before Tick the list, six checks one to a
step, and what remains.

---

Phase: 9, ship. Eighty ninth pass, the run sheet corrected before Bader's first run. **Merged to
main as `346dddb`**, pull request 146, the squash message set on the merge call and off main byte
for byte with no co-author line and no generated-by footer. **No code and no test**: `steps/2026-09-15-kpi-fixes.md`, the log and this file. **2042 tests, 1222 of them
KPI, unchanged**, 28 hook cases unchanged, build zero warnings. **80 audit findings, 20 FIXED, 60
open**, unchanged and not recounted.

**THE MISSING STEP WOULD HAVE COST THE WHOLE RUN.** A plot goes only into a workbook whose row is
ticked and whose template is settled, `Settled()` at `KpiPanel.cs:1304` read by the split at
`:1296`, and the sheet never said to tick the seven workbook rows. Those ticks live in the pane
and closing Revit loses them. It is step 12 now, between Read this model and Tick the list, and it
settles an unsettled park row too.

**TICK THE LIST IS THE LAST TICK ACTION**, because a template row press moves the plot ticks in
both directions, `KpiPanel.cs:1248`. Every step and every reference renumbered, four checks to
five.

**THE FIVE PLOTS WERE NOT A MEASUREMENT.** DM-21, NS-16, NS-20, ST-23 and ST-24 were named as in
the model and nothing here shows it: `DM-21` is in a Drawing Sheet test fixture,
`PlotSelectionTests.cs:11` and `:34`, and the other four are nowhere. The sheet says UNKNOWN until
the run and points at IN THE MODEL AND NOT ON THE LIST.

**A FIFTH CHECK, ON THE PARKS FORM.** Its tree rows are 10.3 and 10.4 apart, `PdfForms.cs:276` to
`:278`, against about 20 on open spaces, `:310` to `:312`. The height rule takes 4 pt off, so those
numbers land near the 6 pt floor and some may read held at 6 while still fitting. **The box heights
are UNKNOWN here** and the check is what measures them.

**The sheet ends with one list of what Bader sends back in one message**, and the rule that none
of it enters this public repository.

**THREE OPEN ITEMS LOGGED AND NOT FIXED**, each at file and line: the write guard recording a
crash as no folder at `KpiRequestHandler.cs:608` to `:615` against its own comment at `:578`, with
a copy already on disk from `WorkbookPatcher.cs:79` and `:86` and the PDF written after the count
at `:706` to `:720`, while `THE PLOT LIST` reads the outcomes at `KpiCreateReport.cs:202`. The
Parks boxes near 6 pt. And font widths read only from an inline `/DR` and `/Font`,
`PdfFormFile.cs:327`, `:330` and `:406`, with no test over a form that writes either as a
reference.

**NOTHING HERE WAS RUN IN REVIT.**

---

Phase: 9, ship. Eighty eighth pass, four fixes in one round. **Merged to main as `088d6d3`**,
pull request 144, the squash message set on the merge call and off main byte for byte with no
co-author line and no generated-by footer. **2042 tests, 1222 of them KPI, 42 added, against the
2000 main carries** at `64fb57d`, 28 hook cases unchanged, build zero warnings.

**THE COUNT, WALKED OFF ALL FOUR AUDIT FILES: 80 numbered, 20 carrying a FIXED mark, 60 OPEN**, as
29 with 8, 20 with 9, 14 with 2 and 17 with 1. The three entries before this counted three files
and said 63 and 44, leaving out audit 4's findings 64 to 80. This round closes, renumbers and
reorders nothing except marking **66 FIXED**.

**1. The text was too big for the PDF boxes.** Nothing had ever read or written a `/DA` and the
rectangle read took two of its four numbers. `PdfTextFit`, `PdfFontWidths` and `StandardFonts` are
new: the size is the largest that fits the box less 2 pt each way, rounded DOWN to 0.1, never above
the client's own or above 10 on their auto, never below 6. The value is never touched. Widths come
off the font's own `/Widths` or the published standard fourteen, and **no near miss is in that
table**. Six outcomes counted in the glance, a box named only for held at 6, widths UNKNOWN and no
`/DA`. `2797.64` in a 30 by 14 box is 7.1.

**2. Streets takes Total Green cover.** Bader's decision, the open question closed,
`RoadsNamesTheCanopyCell` deleted as the second reason a thing gets deleted. ANH-007-ST-100308
reads **0.001053** and not 0.000984. The roads test moved from 0.00055 to 0.00102 **because of the
decision and not to make it pass**. The canopy guard and rows 85, 92 and 99 are unchanged.

**3. The plot list file.** 154 plots, one a line, **and the file never enters this repository**.
`PlotListFile` and `TickingTheList` in Core, one shared browsed line in the pane closing finding
77's remedy without marking it, a Tick the list button that replaces every tick and forgets every
hand choice, named lines above Create for everything that would drop out, and `THE PLOT LIST`
opening the report with three counts. **Audit 4 finding 66 fixed with it**: `GuardedWrite` round
the per plot write loop, so a throw on plot 100 of 154 names the plot and the report is still
written.

**4. A dash botanical name counts as SHRUBS, by its phase.** 44 such rows across the model. HF-01
reads Existing 231, Proposed 0, TOTAL 231 and Ground Cover 52 against a group total of 283. Only
the dash, the whole cell once trimmed, and a dash under no phase row still refuses. Every dash row
is named with its plot, row, phase, area and box.

**UNKNOWN and written down:** which phase EP-01, EP-09 and EP-14 carry their dash rows under, what
fonts the client's forms use, whether any client `/DA` is below 6, and whether the standard
fourteen tables are right to the thousandth. All four are answered by the next run's own report.

**NOTHING HERE WAS RUN IN REVIT.** `steps/2026-09-15-kpi-fixes.md` is Bader's run sheet, eighteen
numbered steps and four checks.

Break watches, all four restored byte for byte: `c80ef410381ff49c46bccd31759e0fef`,
`6fab9b37501c2911a0b82f4133a185d6`, `773fecb415d9abde50f1c7153cbb7a2d`,
`715a77de61330fcc94defbe4283731cd`.

---

Phase: 9, ship. Eighty seventh pass, the check that could never fire. **2000 tests, 1180 of them
KPI, 10 added, against the 1990 main carries** at `9ab922a`, 28 hook cases unchanged, build zero
warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES: 63 numbered findings, 19 carrying a FIXED mark, 44
OPEN.** This round closes none, renumbers none and reorders none.

**The fault, measured on the 08:38 run: placed-nowhere sat INSIDE the equation**, so the sum
always closed, the refusal could never fire, and EP-01, EP-09, EP-14 and HF-01 wrote every box
short with the column reading YES. EP-01 lost all 411 m² and HF-01 went from 231, 52 and 283
to nought, nought and nought. **The wording that caused it was the round message's own.**

**The rule now: what would be WRITTEN plus what this template deliberately LEAVES OUT equals the
group total.** Anything unreadable refuses the plot on its own, before any arithmetic.

**TWO KINDS OF PLACED NOWHERE and only one refuses.** Unreadable, a prefix that is neither or
none at all or no phase row, REFUSES. Left out by this template, a `SHRUBS:` species under a
phase no tree list is named for, is a TERM and not a refusal. **Taken literally the fix would
have written nothing on every mosque plot with a Street Design group**, 459 m² on FM-05 alone,
reversing a decision rather than catching a fault. The two travel in separate lists and print in
separate columns.

**THE SPECIES NO PREFIX PLACED, BY NAME now opens the section**, every distinct name grouped by
the prefix each read with rows and area, because one unseen prefix is a line in a table and a
dozen different things is a naming job, and a count cannot tell them apart.

**UNKNOWN and written down: what those species are called.** The 08:38 report is under
`reports/` and nothing there is committed. One thing the numbers DO settle by deduction: HF-01's
231 is unplaced because of its PREFIX and not its phase, since Existing is a counted phase on
HEALTHCARE and a `SHRUBS:` species under it would have landed in the existing figure.

Break watch: the refusal switched off and the unplaced area put back inside the equation reddens
6 of 2000, each naming the plot, the area lost and what the box would have carried. Restored
byte for byte, md5 `ce641093a192f53beb01fad56cbd1f90`.

Files: `Core/Kpi/GroundCover.cs`, `KpiCreateReport.cs`, `kpi-rules.md`, and the tests
`GroundCoverSplitTests.cs`.

---

Phase: 9, ship. Eighty sixth pass, ground cover out of the shrubs box. **1990 tests, 1170 of
them KPI, 26 added, against the 1964 main carries** at `8367b30`, 28 hook cases unchanged, build
zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES: 63 numbered findings, 19 carrying a FIXED mark, 44
OPEN.** This round closes none, renumbers none and reorders none.

**Item 1, the irrigation water demand, was built in the eighty fifth pass and is on main at
`8367b30`.** Checked point by point against the brief this round rather than assumed, including
the read back, which every written PDF field already goes through with no special case. Nothing
needed doing again and nothing was rebuilt.

**Item 2 is the round.** The SHRUBS & GROUND COVER group holds two kinds and only the species
name says which. DM-14's group is `GROUND COVER: CARISSA MACROCAPA` 270 plus
`GROUND COVER: LAMPRANTHUS AUREUS` 198, the printed group total of 468, every species ground
cover and none shrubs, and all 468 went into Proposed Shrubs with Ground Cover blank. DM-11 is
the opposite end, two `SHRUBS:` species adding to 70.

`SpeciesPrefix` and `GroundCoverSplit` are the rule: the text before the FIRST colon, matched
whole and without case. `SHRUBS:` to the shrubs figures, `GROUND COVER:` to the ground cover
one, **anything else placed NOWHERE and named** with its plot, row, area and what its prefix
read. **The two plus everything unplaced must equal the group total the schedule printed or NONE
of the four boxes is written.**

**The shrubs figures come off species rows now and not off phase rows**, which is the fault: a
phase row holds both kinds added together. Existing against proposed is still
`CountedGroups.SheetFor`. The GRASS group is untouched, 35 on DM-11 and 175 on DM-14, pinned.

**`PdfFill.GroundCoverIsNotPrintedApart` is deleted**, the second reason rather than the first:
the shape it recorded is gone, because the two ARE printed apart on every species row.

**UNKNOWN and written down**: how many plots are all ground cover, all shrubs or mixed. The
05:49 report is under `reports/` and nothing there is committed, and 143, 107 and 55 are counts
of NAMES rather than of plots. `SHRUBS AGAINST GROUND COVER, PER PLOT` answers it on the next
run.

Three break watches, one per rule, 2 then 3 then 1 red of 1990, each naming what was broken in
its own message. All restored byte for byte, md5 `bc824e791d0a099a611531944d930a8e` and
`68acb06b8e5f09ac4eab1d5f32b591b5`.

Eight existing tests moved when the group gained its species rows. Six pass unchanged, two had
their subject reversed and were rewritten rather than deleted. None was weakened.

Files: `Core/Kpi/GroundCover.cs` new, `ScheduleRows.cs`, `PlotReading.cs`, `ShrubsByPhase.cs`,
`PdfFill.cs`, `KpiCreateReport.cs`, `Revit/Kpi/KpiRequestHandler.cs`, `kpi-rules.md`, and the
tests `GroundCoverSplitTests.cs` new plus `PdfFillTests.cs`, `PdfUnitsTests.cs`,
`PdfEmptyingTests.cs`, `PdfChecklistTests.cs` and `WaterDemandTests.cs`.

---

Phase: 9, ship. Eighty fifth pass, the irrigation water demand off both schedules' TOTAL rows.
**1964 tests, 1144 of them KPI, 25 added, against the 1939 main carries** at `dbae2e6`, 28 hook
cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES: 63 numbered findings, 19 carrying a FIXED mark, 44
OPEN.** This round closes none, renumbers none and reorders none. Audit 4's own seventeen, 64 to
80, stand as written.

What it does: per plot, out of BOTH schedules, the L/DAY column, take the schedule's own TOTAL
row, add the two, divide by a thousand for the form's m with a superscript three over day. It
goes on all three PDF forms. `WaterDemandRead.From` is the whole rule, `PlotWaterDemand` holds
both halves, `PlotReading.Water` carries it, and `THE IRRIGATION WATER DEMAND, PER PLOT` reports
it.

**Nothing adds the species rows up.** A schedule with no TOTAL row writes nothing for that half
and is named. Both schedules or nothing, with the half that failed named, and both named where
both failed.

**The column is matched WHOLE and never by holding a word**, because three columns of the
softscape schedule hold WATER and only one reads L/DAY. None and more than one are both refusals
and both print the headings.

**The TOTAL counts groups the tree lists leave out**, Street Design on a mosque plot among them.
That is what TAKE THE TOTAL means and it stands, and the report names every such group with its
own subtotal per plot so the difference is visible rather than found later.

**One thing changed the round did not name**: the value prints through `Fine` rather than to two
places, because 2492 L/day is 2.492 and two places would send 2.49, which is two litres a day
lost on every plot.

**UNKNOWN and written down**: whether any real schedule prints no TOTAL row, and DM-11's
softscape L/DAY total, because the 1548 scan is not in this repository. The 432 and 908 Bader
measured are two totals and the schedule has one, so if they are the group totals the TOTAL row
reads 1340 and 1340 is what the tool takes. The report prints the row it read, which settles it
on the first run.

Break watch: `ScheduleColumns.Reading` made to take the first of two matching headings reddens 2
of 1964, and the first names the fault in its own message. Restored byte for byte, md5
`096037650ddefaf26bf40f276df9b697`.

Files: `Core/Kpi/WaterDemand.cs` new, `ScheduleRows.cs`, `PlotReading.cs`, `PdfFill.cs`,
`KpiCreateReport.cs`, `Revit/Kpi/KpiPlotReader.cs`, `kpi-rules.md`, and the tests
`WaterDemandTests.cs` new plus `CreateFixture.cs`, `PdfUnitsTests.cs` and `PdfEmptyingTests.cs`.

---

Phase: 9, ship. Eighty fourth pass, the fourth audit of the KPI tool, a software firm's review.
**1939 tests, 1119 of them KPI, 0 added, against the 1939 main carries** at `f2e2f42`, 28 hook
cases unchanged, build zero warnings. **This round writes no code and changes no test**, so the
counts are unchanged by construction and are stated either way.

**THE COUNT, READ OFF THE THREE EARLIER AUDIT FILES: 63 numbered findings, 19 carrying a FIXED
mark, 44 OPEN.** Derived by walking the files rather than off a note: 29 numbered with 8 FIXED,
20 with 9, 14 with 2. This round closes none, renumbers none and reorders none, and adds
seventeen of its own, 64 to 80, in `steps/audit-kpi-4.md`.

What it is: the project read the way a firm reads a codebase before taking it over, over eight
areas worked one at a time. The maintainer's test, duplication measured, what happens when it
breaks, dependencies and their licences, secrets and what leaves the building, whether the debt
is growing, the boundaries, and what has never run.

**THE FIRM'S VERDICT, in one line: take it on, at a price.** Zero third party runtime
dependencies, Core carrying no Revit reference proved four ways, and data tables the gate holds
level. Against that: 88 catch blocks of which twenty answer a failure with a value that reads
like an answer, a per plot write loop with no guard, and the client's project name, consultant
and contract reference sitting as constants in a public repository against the ignore file's own
stated reason.

**The debt answer, as numbers rather than an impression.** Since the first audit the KPI source
has grown 126 per cent and its tests 161, so tests per 100 source lines went 3.41, 3.53, 3.90,
3.94. Open findings per 1,000 source lines went 2.31, 3.62, 1.46, 1.55, which is flat. **The
code is not accumulating debt faster than it clears it. The BACKLOG is: 63 opened against 19
closed, and 6 rounds of 42 since the first audit did any closing.**

Two measurements were made by changing a copy of the tip in the session scratchpad and never
this repository. Adding an eighth template reddens 8 of 1939 and then 10 more, each naming the
next table by name. Adding a field to a PDF form reddens 2, both counts, and neither is the
missing fill case.

Verified against Microsoft's own support page this session: **.NET 8 is in Maintenance and ends
10 November 2026**, which is 56 days, and the gate and the test project both pin 8.0.

Secret scan over the WHOLE history and not the tip: 1,942 distinct blobs, 78,220,014 bytes,
1,941 text blobs decoded and scanned against thirteen patterns. **Zero credentials, ever.** The
only three email addresses are example.invalid placeholders. Client specific facts at the tip:
**5,044 occurrences across 243 of 490 tracked files**, counted with twelve patterns.

Files: `steps/audit-kpi-4.md` written, this file and `steps/log-kpi.md` appended. No source
file, no test, no rules file and no hook was touched.

---

Phase: 9, ship. Eighty third pass, the held off list in step and on screen. **1939 tests, 1119 of
them KPI, 5 added, against the 1934 main carries** at `a3d9896`, 28 hook cases unchanged, build
zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**ONE FAULT, THREE ROUTES.** Nothing kept the hand choices and the real ticks in step, and nothing
ever printed them. `HandTicks` is the one record, holding BOTH directions rather than a bare set
of the plots taken off, and `KpiPanel.TickedByHand` moves the ticks and the record together so
nothing sets one without the other.

**1. UNTICKED WAS NOT SYMMETRIC WITH TICKED.** `Ticked` took the held off list and `Unticked` took
nothing, so a hand untick survived a row tick and a hand tick did not survive a row untick.
`Unticked` takes the record now and keeps the plots put on by hand, so unticking a row undoes
exactly what ticking it did and ticking it again restores the same state, pinned in both
directions at once.

**2. SELECT ALL AND CLEAR BOTH FORGET EVERY HAND CHOICE.** I agree about Select all, and **the
same argument carries Clear**, which the round message did not ask about: both replace every tick,
so a choice left standing behind either disagrees with the screen and the next row press acts on
the disagreement. One rule rather than two and no path disagrees with another.

**3. SomeOfThem BUILT THE SENTENCE AND NOTHING CALLED IT**, its only references two lines of its
own test file. It is on the workbook row now, as a NOTE, and a row where every plot is going in
gets no line. **The pane counts nothing**: `TickingATemplate.RowLine` hands back the sentence off
the split's own rule, which is also what made item 3 testable in Core.

**HOW MANY OTHER MEMBERS BUILD A LINE THE PANE NEVER SHOWS: SIX, AND THREE ARE LINES.** Counted
over every public member of `Core/Kpi` returning a string or a list of strings, held against every
reference in `src`, doc comments left out: **245 members, 6 with no reference at all**, every one
tested green. `KpiPaneWords.ModelNamed`, `CreateWords.SuggestedName` and `RegionChoice.WhyUnchosen`
are lines, `ComponentTemplates.ValuesFor` and `PlotPrefixes.PrefixesFor` are lists, and
`WorkbookPatcher.ReadBack` is a file read. **`WhyUnchosen` is the one worth acting on**, because
its docstring says it exists so the report does not print an empty cell and the report prints the
empty cell, and it is NOT acted on this round because it was not asked for.

**THE OPEN QUESTION: NOT QUITE ENOUGH, AND ONE THING IS MISSING.** Items 1 to 3 close the hand
route. The two the split decides are known at tick time through `PlotsPerTemplate.For`, whose `Why`
is already the sentence. **What is missing is the component for a plot NOBODY TICKED**: the
correction recorded last round reaches it through `set.Runs` for ticked plots only, and an unticked
plot is never read and has no reading, which is exactly the plot the column exists to explain. It
needs the component read beside the plot list off ONE read at the press, one argument and one read.
**Not built, as asked.**

Three break watches, one test red each: `Unticked` ignoring the record, `Forgotten` handing back
itself, and the row counting what could go rather than what is going. Every one names what was
broken. Both files restored byte for byte, checked with `diff -q`, and the suite green at 1939
after.

**RECORDED BESIDE `PlotOrigins`: A SECTION THAT COVERS WHAT WENT IN CANNOT TELL YOU WHAT DID NOT.**
The premise of the round before was wrong, its evidence was a silence every section of that report
would produce for an unticked plot, PLOTS TICKED THAT WENT INTO NO WORKBOOK reading 0 is correct by
construction, and the two nines were a coincidence.

**FOR BADER, UNVERIFIED.** Two workbook rows can settle as the same template if two files in the
folder are both recognised as STREETS, and unticking one then strips every street plot while the
other stays ticked. I have not measured that two files can be recognised the same way. The state is
visible now either way, because the still ticked row says 0 of its 78.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every line of it is off the code and the two
verifier reports, and the row line has never been seen on a screen.

**Pull request 138, merged into main as `2775999`.** The runner ran 28 hook cases and 1939
tests against the pull request head `7800ce8`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

Phase: 9, ship. Eighty second pass, where MM-09 to MM-15 come from and the rows with no canopy.
**1934 tests, 1114 of them KPI, 6 added, against the 1928 main carries** at `ea7ae2e`, 28 hook
cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**1. THE PREMISE WAS THAT THEY ARE IN THE LIST AND NOT IN THE MODEL. THEY ARE IN THE MODEL.** All
three places checked, the third first. **Nothing derives a plot name**: one production construction
of the list, `PlotsInTheModel.Of(OnSheets, OnSchedules)`, both halves verbatim reads, `Trim()` the
only transformation. **Every plot identifier builder in this repo is Drawing Sheet's** and no file
under `Core/Kpi` or `Revit/Kpi` names `PlotTickList`, `PlotRange`, `PlotRegistry` or
`PlotSelection`. **The two disagreement lines count the plots named by exactly ONE source**, so a
plot named by BOTH is in neither, and MM-09 to MM-15 being in neither PLACES them in the
intersection: on a sheet AND on a schedule. **The two nines are different nines**, one about which
read found a plot and one about whether the split placed it. **The silence was not evidence**:
every section of the report is over the ticked plots, and PLOTS TICKED THAT WENT INTO NO WORKBOOK
reads 0 by construction. **Why they are unticked is one of three routes**, the component not
being one of the eleven, the component and the prefix disagreeing, or held off by hand, and **WHICH
CANNOT BE DETERMINED FROM THIS REPOSITORY.** Being on a sheet does not narrow it, because the third
of those needs no component at all. `PlotOrigins` is the section, one row per plot at the top of the
report, above the glance: the plot, which read named it, and whether it was ticked, the list read
off the live document at the press and the ticks counted off the outcomes. **It does not say WHY a
plot went unticked**, because the route would be worked out from two records of one fact, and the
list and the component off ONE read is a round of its own.

**2. ROWS 85, 92 AND 99 CARRY NO CANOPY FORMULA AND NOTHING IN THE TOOL CHANGED.** 52 of 150 forms
got no Total Green cover and the glance line named the reason outright. A tree on one of those rows
contributes no canopy in the client's own workbook whoever fills it, so refusing was right and the
exact text rule stays. Recorded in `kpi-rules.md` with the three rows, the count and the guard's own
printed line. Bader is taking it to the client. One thing the printed line says beside that: on that
sheet the canopy per tree sits at N and the area at O rather than at L and M, and the guard never
cared because the only letter it looks for is the diameter column off the heading row.

Three break watches, one test red each: the two sources swapped, a ticked plot the read does not
name dropped, and the section moved below the glance. Every one names what was broken. Both files
restored byte for byte, checked with `diff -q`, and the suite green at 1934 after.

**FOR BADER, NOT A FAULT TO FIX.** Select all and Clear do not touch the held off list, so a plot
unticked by hand once and brought back by Select all is dropped again the next time any template
row is ticked. Nothing on the pane prints that list.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the 05:49 report, the
pane's screenshot and the code as it stands.

**Open and not guessed at:** which of the three routes leaves MM-09 to MM-15 unticked, why the list
holds a plot named `-` and whether a plot's shape should be checked on the way in, and the route
beside each unticked plot, which needs the list and the component off one read.

**Pull request 136, merged into main as `bafb39c`.** The runner ran 28 hook cases and 1934
tests against the pull request head `15e097a`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

Phase: 9, ship. Eighty first pass, three faults off the 19:52 run. **1928 tests, 1108 of them KPI,
12 added, against the 1916 main carries** at `484251e`, 28 hook cases unchanged, build zero
warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**1. THE EMPTYING TOOK THE CLIENT'S OWN HEADER WITH IT.** All seven PDFs read Project name,
Consultant and Contract reference empty. Those three are TYPED DEFAULTS rather than notes.
**Project name and Consultant are left alone, and they are the only two fields in that state.**
Nothing tries to tell a note from a value by reading the text: the three are held as data,
`PdfForms.HeaderValues`. **They are found by the VALUE, because no field name for them is measured
anywhere here and none can be**, and what makes that safe is `PdfFormCheck` refusing a form that
does not carry all three, so a changed header writes NOTHING rather than clearing the wrong box.
**Contract reference is written per plot from PRX_Plot_NH**, Bader's decision, and a plot with none
gets the box emptied with its own reason rather than left holding another project's. Every field is
in one of three states now, and the test refuses any field in none of them.

**2. IT IS NEITHER OF THE TWO CELL CHECKS, AND THAT IS MEASURED.** A parks shaped fixture was built
and both were asked: `GreenCoverCell` Agrees at D9 with the canopy learnt as F9, `PercentageCell`
NothingToCheck and Usable. **So `NothingToCheck` does tell an absence from a drift.** And neither
could blank BOTH fields anyway: read off `PdfFill`, only `WhyNothingCanBeComputed` reaches both, and
it is two things, **the workbook not written or the canopy guard, and WHICH OF THE TWO CANNOT BE
DETERMINED FROM THIS REPOSITORY.** It is not guessed at. What changed under the parks is the canopy
guard's SUBJECT: it reads the client's own rows for the first time since the eightieth pass. **The
exact text rule is KEPT and deliberately not loosened**, because a canopy formula reading the same
diameter and computing it differently gives a number that is not the workbook's. What is added is
`CanopyRow.TheClientsRow`, so the guard says whose row drifted, and the glance counts the two
numbers with ONE LINE PER REASON.

**3. THE REPORT WAS 82,048 LINES AND 42,570 WERE ONE SECTION.** All three rules the round before
asked for landed and they governed the risks list alone, while the per plot fix multiplied the
section's other two lists by 156. `FORMULAS READING A ROW THIS RUN WROTE INTO` carried it, 321
lines on one plot. **The same rule governs it now**, one line per shape, a cell named twice in one
formula being one read and a shared formula and its master being one shape. **And the file opens
with WHAT IS IN THIS FILE**, every section counted off the text the run just wrote, lines, blocks
and cost per block. **It is the instrument rather than the cut**, and the next cut is made on its
numbers.

**ONE CORRECTION TO THE ROUND MESSAGE, AND THE ONLY ONE.** It said the fault is one of the two cell
checks. It is not. Everything else it states was confirmed.

Three break watches, five tests red, one per break: the header cleared, the client's row named as
the run's, the reason counted per plot, the shapes ungrouped and the opening uncounted. Every one
names what was broken. All five files restored byte for byte, checked with `diff -q`, and the suite
green at 1928 after.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the seven 19:52 output
PDFs and the two report files. **The two computed numbers have still never been held against a
workbook Excel has recalculated.**

**Open and not guessed at:** which of the two things blanked the parks, which the 19:52 report
already answers and the next run puts in one line at the top; what the parks templates' canopy
formula reads; and what the report's other sections come to, which the next run prints.

**Pull request 134, merged into main as `8b90fe0`.** The runner ran 28 hook cases and 1928
tests against the pull request head `25ce207`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

Phase: 9, ship. Eightieth pass, five faults off the 18:15 run. **1916 tests, 1096 of them KPI, 14
added, against the 1902 main carries** at `d266374`, which is also this branch's point, 28 hook
cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**1. THE CLIENT'S FILLING INSTRUCTIONS WERE PRINTED IN THE CLIENT'S PDF**, on all 150. Every text
field is WRITTEN or EMPTIED now, including every field the tool has no source for and names
nowhere. **The four tick boxes and the Reset button are the only fields left as the template has
them, and they are left because of WHAT THEY ARE**: the kind comes off the file's own `/FT`,
inherited through the parent chain, and only `Tx` is cleared. A kind that cannot be read is left
alone. The report names every emptied field with why and reads back what landed in each.

**2. THE CANOPY WAS ZERO AND IT WAS THE FIRST CANDIDATE, NOT THE SECOND.** `CanopyArea.From` took
only matches with `Added` true, a species written into an EMPTY row, so every row the client's list
already held was left out whatever its diameter said. The second candidate is real on the same rows
and is fixed with it: a matched row's diameter comes off the workbook, travelling on the match as
`WorkbookDiameter`, so nothing looks the row up twice. **The percentage comes right with it because
it is canopy over area and the canopy was the zero.**

**3. NOTHING READS WITHOUT A PRESS, AND THE HEADER WAS CLAIMING A READ THAT DID NOT HAPPEN.** Every
path was gone through: `Ask(Plots)` is in the Read button's handler and nowhere else, `WhichModel`
reads the title alone, `Create` is a press. **What was wrong is the claim.** Two different reads set
the count and printed the same way, the plots press at under a second and Create's scan at 46.2.
`ReadOfTheModel` carries which read produced it and the line reads `Counted at ...` with a sentence
saying the model itself was not read. **And the status line now says the read landed**, which is why
18:06:58 showed a finished count beside a line still claiming to work.

**4. 149 OF 156 PLOTS HAD NO DETAIL.** `WriteAll` took the first run of each template. Every plot
gets its own block now. **The counts stay counts of the run because they already are**, above every
block, so nothing is re-counted and no number moves. That is why a block per plot beat one set of
blocks carrying every plot.

**5. THE FORMULAS SECTION.** Only formulas reading a cell on a row this run wrote into, the heading
counting exactly what the body prints, and one formula filled down a column said once with its
cells. A #DIV/0! on a cell this run did not write is out of the section and still counted in THE
DIVISIONS off the unfiltered list. **What the file comes to is UNKNOWN until the next run**, said
rather than estimated, because the split of the 526 is in neither report file.

**FOR BADER, NOT A FAULT TO FIX.** The plot list holds a plot whose name is a single dash, on a
schedule and on no sheet. Some schedule filters `PRX_Ref Plot ID` on `-`. Nothing in the tool
invents a plot, so it came off the model.

Five break watches, one per fault: 2, 1, 1, 1 and 1 red. **The second went green the first time and
the TEST was wrong**, asserting on a `CanopyRow` built by hand rather than going through the method
that dropped the rows. It goes through `CanopyArea.From` now and the same break reddens it. All
restored byte for byte, checked with `diff -q`, and the suite green at 1916 after.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the 18:15 output pairs and
the two report files. **The two computed numbers have still never been held against a workbook Excel
has recalculated.**

Pull request 132, merged into main as `cc85143`. **The runner ran 28 hook cases and 1916 tests
against the pull request head `f8e6644`, 0 failed and 0 skipped. Locally the same 28 and 1916 ran
after the last file was written, 1096 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

Phase: 9, ship. Seventy ninth pass, one rule for every label and every unit off the page. **1902
tests, 1082 of them KPI, 16 added, against the 1886 main carries** at `d609559`, which is also this
branch's point, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**THE LEADING SPACE IS REAL AND THE FIELD WAS NOT BEING BLANKED.** All seven templates hold
` Total Green cover (m²)` and the client's note writes it without, which is where the constant came
from. The round message expected the lookup to match nothing. **Measured: it was landing**, because
the cell's text was trimmed where it was read, and a fixture carrying the real label passed first
time. Taking that trim out reddened five cases, one of them reading `no cell on <Mosques> reads
Total Green cover (m²)`. So the rule was right and **lived in a bare `Trim()` that nothing named,
covering one side of a two sided comparison**, which is worse than the fault expected: a label
constant with a stray space would still have failed with no sign of where to look.

**`LabelText.Same` IS THAT RULE NOW, ASKED BY EVERY WHOLE LABEL LOOKUP.** Edge whitespace off BOTH
sides, without case, and the inside untouched, because `LOD /  HARDSCAPE SCHEDULES` really carries
two spaces. Eleven labels checked one by one and the ones already right are named as well as the
one that was not: REF :, Date:, Prepared By: twice over, Character, Context and the percentage all
carried no edge space; the green cover is the one that does and is spelt as the file holds it now;
**the four street reference column names were a second one sided comparison** and were found by
looking rather than by failing. `ScheduleColumns.Holding` asks a different question, whether a
heading holds a word, and is recorded as checked and already immune.

**THE FIXTURE WAS READING THE CODE BACK TO ITSELF**, writing the tool's own constant into the label
cell. Both labels are written out by hand now, the green cover with its space and
`xml:space="preserve"`, the percentage with none.

**FOUR CONVERSIONS AND EVERY OTHER FIELD IS WRITTEN IN THE UNIT IT WAS READ IN**, read off the page
by position. Three are live and the fourth, litres a day into m³/day, is unreachable because
nothing reads a water demand, so its reason carries the unit and the division for the day it is
built. **The report prints the form's unit and already did**, checked at the line and pinned by a
test rather than left to be read off the code. The unit strings are what the page prints now, m²,
km², m³/day and count.

**THE FIELDS NOBODY FILLS ARE ON RECORD WITH THEIR UNITS**, and Cycling paths and Pedestrian paths
are lm on the Parks form and km on the Open spaces form. Same row name, two units, two forms, one
tool. Nothing writes them today and it would be a thousandfold error the day somebody adds a note
to one and reads the other form's unit.

Two break watches, each reddening the case that names what it broke: 5 and 4 red. Both restored
byte for byte, checked with `diff -q`, and the suite green at 1902 after.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The labels and the units are Bader's
measurements off the client's own seven templates and three forms, and the lookup is proven against
workbooks the tests build carrying the measured label text. **The two computed numbers have still
never been held against a workbook Excel has recalculated.**

Pull request 130, merged into main as `b8bf73e`. **The runner ran 28 hook cases and 1902 tests
against the pull request head `27832dd`, 0 failed and 0 skipped. Locally the same 28 and 1902 ran
after the last file was written, 1082 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

Phase: 9, ship. Seventy eighth pass, the other two formulas measured and guarded. **1886 tests,
1066 of them KPI, 7 added, against the 1879 main carries** at `2c698c7`, which is also this
branch's point, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**THE OPEN QUESTION THE ROUND BEFORE LEFT IS CLOSED, BY THE MEASUREMENT IT ASKED FOR.** Bader
measured both cells on all seven templates. Total Green cover is canopy plus planting plus lawn,
`D9 = F9+F11+H11` on EXISTING PARKS, FUTURE PARKS and STREETS and `D8 = F8+F10+H10` on the other
four. The canopy percentage is canopy over area, `IF(Area<1," ",F8/Area)`. Refusing to build the
guard on one example was right and the log entry that asked the question is marked closed in place.

**THE CLIENT'S NOTE IS WRONG ABOUT WHERE THE PERCENTAGE IS, WHICH IS THE SECOND SUCH NOTE.** It
names the cell right of `Total area covered by canopy` and no template carries that label in
section 1. It is in section 3 under `% of Total area covered by canopy`, and its value sits TWO
columns right rather than one. The first was the TOTAL Shrubs tooltip last round.

**THE ONE FORM THAT ASKS FOR THE PERCENTAGE IS FED BY THE TWO TEMPLATES THAT DO NOT CARRY IT.** The
Parks PDF is the only form with the field and EXISTING PARKS and FUTURE PARKS have no such cell, so
the check answers nothing to check on every run today. **The number is still computed and written**,
off the canopy and the area, and the report says the workbook has no cell to hold it against. An
absence is not a drift, told apart by `SummaryCellCheck.NothingToCheck` rather than by reading the
reason.

**NO LETTER FINDS EITHER CELL.** `ComputedPlaces` holds the two labels and the distance right of
each, read through the same `LabelledPlaces` lookup every other labelled cell uses, and it sits
BESIDE `LabelledPlaces.All` rather than inside it, so nothing can ever write a value into a cell
holding the client's own formula. `GreenCoverCell` requires three single cells including the map's
planting and lawn cells, and **the third IS the canopy cell, learnt rather than written in** and
carried to `PercentageCell`, which requires that cell and the map's area cell. The defined name
`Area` is resolved and checked through the new `FormulaCell.SingleCellsRead`, so no formula text is
matched anywhere.

Two break watches, each reddening the case that names what it broke: 1 and 2 red. Both restored
byte for byte, checked with `diff -q`, and the suite green at 1886 after.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and neither check has run against a real
client template. Both are proven against a workbook the tests build in the MOSQUES shape. **The
first real run is what confirms the green cover label text**, because the report prints the cell it
chose. **The two computed numbers have still never been held against a workbook Excel has
recalculated**, which stands from last round.

Pull request 128, merged into main as `3fcb4c4`. **The runner ran 28 hook cases and 1886 tests
against the pull request head `eeff6c1`, 0 failed and 0 skipped. Locally the same 28 and 1886 ran
after the last file was written, 1066 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

Phase: 9, ship. Seventy seventh pass, two numbers computed and every unit named. **1879 tests,
1059 of them KPI, 58 added, against the 1821 main carries** at `e1d8848`, which is also this
branch's point, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**MY CORRECTION TO THE TOTAL SHRUBS NOTE WAS TOO NARROW AND BADER'S READING HOLDS.** The round
before checked the field's `/V` and called the record right. The `/TU` tooltip on the PARKS form
reads `Existing Shrubs` on the row the page prints TOTAL Shrubs Area (m²), and on the Proposed row
above it. So there is a real wrong label in these files, on a different form from the one the
round message named. **The conclusion never moved and the tool never followed the note:** it
writes existing plus proposed on all three forms and always did.

**THE POSITION IS THE THIRD RECORD NOW.** Every field of all three forms carries its measured x
and y off its own `/Rect`, `PdfFormCheck` compares them with half a point of room against rows
twenty points apart, and a field that has moved writes nothing and is named. Every field matched
by note was then gone through one by one, and the thirteen that were right are named beside the
one that was not.

**THE TWO WORKBOOK CELLS ARE COMPUTED, FROM WHAT THIS RUN WROTE AND READ, WITH THE WORKING
SHOWN.** Total Green cover is canopy plus planting plus lawn, the canopy percentage is canopy over
area, and planting, lawn and area are the three totals this run wrote into the workbook's own
cells. The canopy is the workbook's own column worked out again, rounding INSIDE per tree off the
measured `L21 =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))`. **Adding printed numbers with the
working shown was already the rule and this is that rule one step further**, written into
`kpi-rules.md` out loud because it is the first number this tool produces that no schedule printed.

**THE GUARD IS HALF BUILT, ON PURPOSE, AND THE OTHER HALF IS AN OPEN QUESTION.**
`WorkbookArithmetic.Canopy` reads the output's own formulas and blanks BOTH numbers where a row's
canopy formula differs from the text the tool knows, naming the row and every formula on it. **The
Total Green cover cell and the percentage cell cannot be checked, because the text of neither is
measured anywhere in this repository on any of the seven templates.** The one thing near it is
`H9 = H8/Area` on EXISTING PARKS, one template, and building on one example is the shape this repo
has paid for five times. So nothing looks for either cell and the report says so beside both
numbers. **What Bader has to measure is named: the formula text of both cells on all seven.**

**TWO UNITS CONVERT AND FORTY DO NOT, and every one of the forty two is named.** The road length
is metres into a box printed km, so it is divided by a thousand FOR THE PDF ALONE and the Excel is
untouched. The canopy percentage is a ratio times a hundred with no sign, checked against the
client's own filled ANH-006-NP-100002, where 550 square metres of canopy is their 0.000550 square
kilometres exactly and 550 over 771 is their 71. A row in any unit but m never reaches the
conversion, because `StreetReferenceFile.For` already refuses it by name.

Four break watches, each reddening the case that names what it broke: 4, 3, 3 and 2 red. All
restored byte for byte, checked with `diff -q`, and the suite green at 1879 after.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The tooltips and the positions came off the
client's three real PDFs, and the canopy guard was proven against a workbook the tests build.
**The two computed numbers have never been held against a workbook Excel has recalculated**, and
that is the first thing the next run should do.

Pull request 126, merged into main as `5fca585`. **The runner ran 28 hook cases and 1879 tests
against the pull request head `9cabd2d`, 0 failed and 0 skipped. Locally the same 28 and 1879 ran
after the last file was written, 1059 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

Phase: 9, ship. Seventy sixth pass, a PDF beside every workbook. **1821 tests, 1001 of them KPI,
58 added, against the 1763 main carries** at the branch point `bb10f9d`, measured by running the
suite at that commit, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**NOTHING ABOUT THE WORKBOOK CHANGED.** The 14:29 run's 102 workbooks over 7 templates are
written by the same code down the same path, and the PDF is added after it.

**EVERY FIELD WAS READ OFF THE CLIENT'S OWN FILES AND CHECKED BACK AGAINST THEM.** All three
forms matched, nothing missing and no note differing. Three things the round message said are
corrected by that measurement: the sources are in the field's VALUE rather than its default
value, the open spaces TOTAL Shrubs note is NOT the wrong one, and Parks and Roads name no
Project Type field at all. A fourth is an open question: the ROADS form's Total areas to be
greened names the canopy cell where the other two name Total Green cover.

**THE LIBRARY WAS PICKED BY SEARCH AND REJECTED BY MEASUREMENT.** PDFsharp 6.2.2 is MIT and
netstandard2.0 and carries the AcroForm types, and on the client's own Parks form it read all 42
fields and threw on the first one touched because setting a value REGENERATES the appearance
stream, which is redrawing what the client drew. Every other library is per seat commercial.
**So there is no package**: `PdfFormFile` copies the client's bytes whole and appends an
incremental update, which is the workbook's own rule applied to a PDF, and a test asserts the
source bytes are the first bytes of the answer.

**THE FORM IS CHECKED ON EVERY RUN, over the field names AND the note in each**, and a form
whose either has moved is left alone and named. Five of the nine prefixes use a form whose UID
is a field called undefined_4.1 and whose lawn area is 0_2.

**The shrubs split by phase needed no new schedule read**: the phase rows have been carried
since the 0928 run, and `ShrubsByPhase` sorts them by the same `CountedGroups.SheetFor` the
species merge asks, so there is no second rule.

**FOUR FIELDS ARE BLANK ON EVERY RUN AND EACH IS NAMED.** Ground cover, because nothing prints
it apart from shrubs. Total areas to be greened and Percentage canopy, because **both are cells
the workbook COMPUTES and the patcher drops every cached formula result on purpose**, so the
number is not in the file this run wrote. Irrigation water demand, because no water demand is
read off any schedule yet. **The second of those is the round's real finding and it is Bader's
to answer.**

Three break watches. The first reddened NOTHING the first time, because the case asserted the
three numbers and the break left those right while putting a phase in the wrong bucket. The case
pins the buckets now and the same break reddens it. The other two reddened 3 and 1, each naming
what it broke. All restored byte for byte, and the restored code was run against the client's
three real forms again afterwards.

Pull request 125, merged into main as `524231f`. **The runner ran 28 hook cases and 1821 tests
against the pull request head `ddaef08`, 0 failed and 0 skipped. Locally the same 28 and 1821 ran
after the last file was written, 1001 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and that is worth saying twice this time:
the file work was proven against the client's three real PDFs outside Revit, and the pane, the
fourth folder and the per plot ordering have not been pressed once.

Phase: 9, ship. Seventy fifth pass, the client's note decides a tie and a reason reaches the
file. **1763 tests, 943 of them KPI, 16 added, against the 1747 main carries** at the branch
point `9fbcea4`, measured by running the suite at that commit, 28 hook cases unchanged, build
zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**TWO OPEN QUESTIONS CLOSED BY THE 14:29 RUN, both by a count rather than an argument.** Of 156
plots wanting an area, 98 took it off RCRC_OUT OF SCOPE (PRESENTATION), ZERO off a type the
client's note does not name, and 58 chose no region. **The note holds**, and the 9 September
reading of NS-19 and NS-06 was wrong because those plots carry BOTH and one column was read.
And **no #DIV/0! anywhere in the press**, so the two the 09:18 run left unexplained are not in
this model's output. **Both were answered by the two lines put at the top of the report a round
before**, which is the whole case for having put them there.

**1. THE NOTE'S TYPE DECIDES A TIE.** Bader's decision of 14 September, off 51 of 78 street
plots that wrote nothing for exactly this and every one of which offered the note's type as one
of its two. `RegionChoice.Pick` in order: a person's pick wins, one region holding an area
answers itself whatever it is called, and more than one with the note's type among them takes
it. **The question stays where the note's type is not among them**, and where it is held by two
at once. **RCRC_CADASTRAL LIMIT is written nowhere in the tool.**

**`WhyUnchosen` asks `Pick` rather than deciding again**, which was not optional: the first
version left it deciding for itself and a test went red inside the hour on a pick that chooses
beside a reason still printing a question.

**THE REPORT SAYS WHO CHOSE.** `RegionPick` is the type and the route as ONE record, set where
the choice is made, and the region table gained one column, `how it was chosen`.

**AND THE PICK USED TO DROP THE PLOT'S UID2**, so a plot answered after a refusal was refused
again with a sentence about the model that was about `WithChosenRegion`. Found while reading
that path, fixed, pinned.

**2. A REASON THAT POINTS AT A SCREEN IS NOT A REASON.** Seven places produce a refusal that can
reach the file and **TWO wrote a pointer**, both through one method. Five were already right.
`CreateWords.WhyNothingWasWritten` now takes where the answer is going: the file gets every
reason written out and the PANE keeps the count and the pointer, where the reasons are already
in red above the button. **Nothing went red when I made that change**, which is the finding
inside the finding, and `ReasonInTheFileTests` is seven cases now.

Three break watches, 7 red, 3 red and 1 red, each naming what it broke, all restored and proved
byte for byte with `diff -q`.

**OPEN AND NOT THIS TASK'S TO FIX.** `CLAUDE.md` line 209 still carries the corrected NS-19 and
NS-06 claim. It is the repo wide file rather than KPI territory, so this round left it and said
so in the log. Two records of one fact, knowingly left standing for one round.

Pull request 124, merged into main as `5706bd2`. **The runner ran 28 hook cases and 1763 tests
against the pull request head `3ae4868`, 0 failed and 0 skipped. Locally the same 28 and 1763 ran
after the last file was written, 943 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next street run is what shows all 51
plots writing.

Phase: 9, ship. Seventy fourth pass, three lines at the top of the report so a run can be checked
at a glance. **1747 tests, 927 of them KPI, 15 added, against the 1732 main carries** at the
branch point `16bed93`, measured by running the suite at that commit rather than quoted from the
round before, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none.

**WHAT THIS ROUND IS.** All three questions were answerable before it and all three were spread
over hundreds of lines: the streets area over one block per plot, the region type over one row
per plot, and the divisions over one formula section per template. **Nothing was moved and
nothing new was measured.** `RunAtAGlance.Of` counts what the runs already carry, and
`THIS RUN AT A GLANCE` is the FIRST section of the file, above the run's own accounting and above
every per template block.

**a. THE STREETS AREA, COUNTED OFF THE OUTCOME AND OFF THE MAP.** A street plot got a value when
the template's own area cell landed in its output holding something, read back off the file, so
**nothing here names H8** and nothing reads what the fill set out to write. A cell that landed
holding nothing did not get a value. The plots without one are NAMED with the reason their own
run recorded. **A press with no street plot says so rather than counting nought of nought**,
because no street plot is not the same fact as every street plot failing.

**b. THE REGION TYPE, COUNTED AS THE RUN FOUND THEM.** `RegionChoice.TheNoteNames` sorts to the
top so the two counts the team asks for are the first two rows, and **RCRC_CADASTRAL LIMIT is
deliberately not written in beside it**: a rule naming two types counts a model's third under
nothing, and a test builds one and checks it is counted under its own name. A plot that chose no
region is counted apart from every type, because two regions holding an area is a question
waiting on a person and none holding one is a plot with nothing to read.

**c. THE DIVISIONS, AND THE KIND TRAVELS ON THE FINDING.** `FormulaAtRisk.IsDivideByZero` and
`DivisorThisRunWrote` are properties set where the risk is built. **A signal that travels in the
data is not a signal**, and a counter searching `Reason` would count a reason that merely talks
about a division. Break watch 2 proved why: stopping the two flags left **every one of
`DivideByZeroTests` GREEN**, because those five cases assert the sentence, and reddened three of
the new ones.

**THE GLANCE HEADING CARRIES NO COUNT, and every other heading in this report does.** Three is
how many questions there are rather than how many of anything this run found, and a constant
sitting where a count goes reads as a measurement. Written with the count first, taken out after
reading a sample report, pinned by a test that also checks the heading below it still counts.

**Two stale docstrings corrected**, both descriptions of the tool that contradicted the tool.
`KpiValue.StreetsRoadWidth` said the workbook computes H8 and nothing writes there.
`Reconciliation.AreaWanted` said STREETS types the road width and the total length by hand.

Three break watches, 1 red, 3 red and 1 red, each naming what it broke, all restored and proved
byte for byte with `diff -q`.

Pull request 123, merged into main as `b65cfad`. **The runner ran 28 hook cases and 1747 tests
against the pull request head `8cf8204`, 0 failed and 0 skipped. Locally the same 28 and 1747 ran
after the last file was written, 927 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next street run is what fills these
three lines with real numbers, and the region count is what answers the client's note against
NS-19 and NS-06.

Phase: 9, ship. Seventy third pass, the client reissued the STREETS template and one cell in it
changed. **1732 tests, 912 of them KPI, 3 added, against the 1729 main carries** at the branch
point `151922b`, measured by running the suite at that commit rather than quoted from the round
before, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none. Finding 31
keeps its FIXED mark and gains a note under it, which changes no count.

**THE MEASUREMENT THE ROUND RESTS ON IS ONE CELL.** The reissued STREETS template diffed against
the one that ran this morning: 4,160 cells against 4,159, no named range moved, no other sheet
touched. H8, Streets Total Area (m2), was `=Width*F8` and is now empty, and the client's own
reference copy says it is PRX_Intervention Area off the 00 link. So three cells on one row come
from three sources and the workbook computes none of them.

**1. STREETS TAKES AN AREA AGAIN, THROUGH ONE ENTRY IN ITS OWN MAP.**
`new MappedCell(KpiValue.Area, "H8")` and nothing else, because `KpiTemplate.TakesNoArea` reads
the MAP and every path that skips an area asks that one thing. The region read, the region
choice, the reconciliation refusal, the identical area confirm and the report section all came
back on together. **H8 is hard coded nowhere.**

**Finding 31 is REVERSED BY THE TEMPLATE CHANGING, not by being wrong.** Its FIXED mark stands
and the note under it says so, with the pull request named. **`AreaIsTypedByHand` is renamed
`TakesNoArea`**, because no template ever typed an area: STREETS computed one and the other six
read one, and a name that tells one template's story stops being true when that template
changes. Four call sites in two projects read it and three sets of words said typed by hand.

**2. THE CLIENT'S NOTE NAMES A REGION TYPE AND NOTHING CHOOSES ON IT.** The rule is unchanged:
the plot's own regions, the one holding a non zero area, and more than one asks.
`RegionChoice.TheNoteNames` is the note's type held as data and read by nothing that decides,
and the report's region table carries a column per plot saying whether the area came off the
type the note names. **DM-11, DM-12 and DM-13 hold it on OUT OF SCOPE and NS-19 and NS-06 hold
it on CADASTRAL LIMIT**, and NS plots are street plots. **OPEN QUESTION FOR BADER**, in the log
with both plots named, and a run over 78 street plots answers it by counting.

**3. A FIXTURE WHOSE NAMES ARE NOT THE MODEL'S CANNOT CATCH A RULE ABOUT NAMES.** Break watch 2
broke exactly the rule `RegionChoiceTests` owns and **all eight of its cases stayed GREEN**,
because its fixture read `CADASTRAL LIMIT` and `OUT OF SCOPE (PRESENTATION)` without the `RCRC_`
prefix the model carries. The names are the model's own now, a case was added, and the same
break reddens 2 of them.

**4. READ EVERY RULE OFF THE TEMPLATE, NEVER OFF THE NOTES COPY.** The notes copy sums existing
canopy at M93 and counts native over H3:H91 where the template uses M102 and H3:H101. THE
TEMPLATE WINS. **Second time an annotated set and a production set have differed**, the first
being the seven of 9 September. Nothing reads either range, so no code changed and it is written
into the rules file for the next one.

**5. STREET DESIGN AND PROPOSED ARE BOTH PROPOSED ON STREETS. CONFIRMED BY BADER, 14 September.**
Nothing changes in code, which is the point: it is recorded in `kpi-rules.md` as confirmed rather
than as a September decision nobody had checked. The two beside it are recorded as NOT confirmed
so the confirmation cannot cover them quietly.

Two break watches, 4 red and 3 red, both restored and proved byte for byte with `diff -q`. The
first failed `StreetsNamesH8ForTheAreaAndItsOtherCellsAreUnmoved` with `Expected: "H8" / Actual:
"H7"`, naming what was broken. The second reddened what it aimed at and found the fixture gap in
item 3 as well.

Pull request 122, merged into main as `194a57f`. **The runner ran 28 hook cases and 1732 tests
against the pull request head `776bdeb`, 0 failed and 0 skipped. Locally the same 28 and 1732 ran
after the last file was written, 912 of the tests KPI.** The merge went through the API with the
title and the message both passed on the call, the commit came back off main carrying neither a
co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch
head, checked with a diff that named no file.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next run over street plots is what
writes an area into H8 and what answers the note's question by counting 78 plots.

Phase: 9, ship. Seventy second pass, two things off the 09:18 run, NG05, 156 plots over 7
templates, 150 workbooks. **1729 tests, 909 of them KPI, 23 added, against the 1706 main
carries** at the branch point `0b29cf7`, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none. Both items are
faults off a run rather than audit entries, so they are in the log.

**WHAT WORKED, recorded because a round of faults hides it.** The label lookup reached C5, E5,
G5 and H5 on every template that ran, four of the seven asset types ran for the first time, and
HEALTHCARE, MOSQUES, PARKING and one STREETS workbook recalculate with ZERO errors.

**1. A PLOT COULD GET A TEMPLATE AND NO FOLDER, AND SIX DID.** EP-05, EP-11, EP-12, EP-13,
EP-15 and FM-08 were ticked, READ, and dropped at the last step. No sheet means no component, so
the prefix placed them, and the folder table keyed on the component had nothing for them. Two
routes to a template and one to a folder.

**The fix adds no eighth table, and that is the whole of why it was chosen.** Both tables are
keyed on the same eleven components, so which folders a template reaches is already written down
twice over and is read off the two together. **Six templates reach exactly one folder and
MOSQUES reaches two**, so a plot with no component is filed where every other plot of its
template is filed, which places the five park plots, and MOSQUES derives nothing, which still
refuses FM-08 with both folders named. An absence and an answer nobody knows stay two different
things: a component the table does not hold still places nothing.

**And it is named BEFORE the press.** The count of ticked plots with no component is a note and
the plot that can be filed nowhere is a refusal, both above Create. Reading five park plots and
throwing them away is work nobody asked for.

**2. THE TWO DIVIDE BY ZERO ERRORS. WHICH CELLS THEY ARE IS UNKNOWN FROM THIS REPOSITORY** and
is not guessed at: no client workbook is here and none ever will be. What was established is why
the report said nothing about them, which is that **the formula check could not see a division
at all**. It can now: a divisor holding a nought or nothing is named with its cell, its formula
and **whether THIS RUN wrote it**, so the next run answers the question by itself on the real
templates. An expression, a range or a formula cell as the divisor is not judged, each for its
own stated reason.

**It is REPORTED and never refused on**, with a test that the output stays on disk either way,
because a plot with no trees really has no average and deleting 150 correct workbooks over the
client's arithmetic would be a far worse fault than the one it reports. Turning it into a
refusal where the run wrote the divisor is Bader's decision once a run has named them. No client
formula is changed.

**NOT A FAULT, recorded so nobody chases it.** Both parks workbooks recalculate with 44 #N/A at
F31 to F74 and an untouched template with 45. The one that goes away is a divide by zero on the
empty area the run filled. The 44 are the PARK PROGRAMME section over an empty Criteria table in
the client's own file.

One break watch, 3 red, restored byte for byte. A template reaching two folders taking the first
files a mosque plot by a guess, and the red is the case asserting that plot is refused. Fourteen
call sites gained the template argument, each given the one its own component really means.

Pull request 121, merged into main as `de49858`. **The runner ran 28 hook cases and 1729 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1729 ran at `de49858`, 909 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next run is what shows the five park
plots writing workbooks and what names the four #DIV/0! cells by sheet and by cell.

Phase: 9, ship. Seventy first pass, Bader's answer to the open question and the live fault his
answer turned up. **1706 tests, 886 of them KPI, 6 added, against the 1700 main carries** at the
branch point `bfbab7f`, 28 hook cases unchanged, build zero warnings.

**THE COUNT, READ OFF THE THREE AUDIT FILES AT THESE LINES: 63 numbered findings, 19 carrying a
FIXED mark, 44 OPEN.** This round closes none, renumbers none and reorders none. That count is
read off the files and off nothing else, which is Bader's instruction after four round messages
carried a number that did not reconcile.

**THE UNKNOWN IS CLOSED. No label names the position cell**, measured on all seven: the only
cells whose text names a position hold `<Position>` itself, the placeholder this run replaces.
The distance of two from `Prepared By:` stays, and a test says nothing in the table looks for
the word.

**AND THE ANSWER TURNED UP A FAULT THE ROUND BEFORE SHIPPED.** `Date:` is on every template
TWICE, beside `Prepared By:` on row 5 and beside `Reviewed By:` on row 28, or row 29 on the two
parks and STREETS. **The found twice guard was firing, on all seven, unscoped**, measured before
anything changed: `Date: is on <Mosques> at D5 and D28, and nothing says which is meant`. So the
merged tool would have written **no date into any workbook**. The guard read correct and the
TABLE was wrong. **A guard firing on data nobody built is a guard nobody has seen fire**: my
fixtures carried row 5 alone, so the seven way theory meant to be the check ran against a sheet
the client does not have.

**The date is found through the preparer's block now.** `LabelledPlace.OnTheRowOf` names the
place whose label's ROW this one may look on, resolved in a second pass. **The row is chosen by
the label and never by being first or by a number**, so a sheet whose reviewer block comes first
is answered with the preparer's row. An anchor that cannot be found gives no row to look on and
says so without repeating the anchor's words, the guard still fires inside that row with both
reasons naming which row and why, anchoring is one level deep with a test, and `REF :` stays
looking over the whole sheet because the second block carries none.

**Both seven way theories now build the reviewer block too**, at 28 or 29 as measured, and each
asserts nothing at all lands in it.

Two break watches, 21 and 1 red, both restored byte for byte. The first is the merged fault and
its red message is that fault word for word. **The second reddened exactly one test**, the
reviewer first case, which is the only thing standing between the preparer's row and the
constant 5, since every template measured so far has its preparer on row 5. Two existing tests
changed by hand, each because the truth under it moved.

Pull request 120, merged into main as `8abcf4a`. **The runner ran 28 hook cases and 1706 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1706 ran at `8abcf4a`, 886 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The next run is what shows a date reaching
E5 on a real template, and a run says so plainly either way.

Phase: 9, ship. Seventieth pass, findings 50 and 53 off the third audit plus two things Bader's
own measurement turned up. **1700 tests, 880 of them KPI, 27 added, against the 1673 main
carries** at the branch point `86b322f`, 28 hook cases unchanged, build zero warnings.

**THE MEASUREMENT IS THE ROUND.** Row 5 read on all seven templates on 14 September: `B5 REF :`,
`D5 Date:` and `F5 Prepared By:` on every one, the four value cells at C5, E5, G5 and H5, and
**NO FORMULA at any of the four on any of the seven.** The letter map never overwrote anything
and no workbook is damaged, so finding 50 did not become a BLOCKS. The letters were right on all
seven BY LUCK, and FUTURE PARKS, one of the three never looked at, is the one that differs: not
in where the cells are but in what they HOLD.

**THE FOUR CELLS ARE FOUND BY THEIR LABELS NOW.** `LabelledPlaces` is one table of six places,
each a name, a label and how many columns right its cell sits. `KpiTemplates.TypedByTheTeam` is
deleted and `KpiValue.Reference` is out of every map, with a test refusing any map that names
E5, G5 or H5 again. **CHECK YOUR WORK: the lookup lands on C5, E5, G5 and H5 on all seven**,
asserted against each template's own rebuilt row 5, and a second theory says the four values get
there through the plan.

**NO LABEL NAMES THE POSITION CELL, AND THAT IS THIS ROUND'S OWN FINDING.** Three labels reach
three cells. H5 is one further along than the person's name under the same Prepared By, which
both park templates confirm by holding a position there already. It is written as a DISTANCE of
two and said out loud in the code, the rules and the log, rather than dressed up as a label.
**Whether the sheets name it somewhere off row 5 is UNKNOWN** and is for Bader.

**A CELL IT WRITES THAT WAS NOT EMPTY IS NAMED WITH WHAT IT HELD.** `KpiCreatePlan.Labelled` was
set and read nowhere, so the mosque template's D7 and both park templates' H5 were written over
in silence. CELLS WRITTEN OVER SOMETHING THE TEMPLATE ALREADY HELD is that line. Nothing stopped
writing.

**THE FILLED CHECK WAS MEASURED RATHER THAN REASONED ABOUT: A CLEAN PARKS TEMPLATE IS OFFERED.**
An empty E5 reads as not filled, so both park templates and MOSQUES come back offered through
the real peek and recognition path, and a filled park workbook beside them is still withheld.
**There was no live fault on the two park templates.** `FilledMarks` still reads E5 and C5 by
letter on purpose, because it runs before any template is recognised, and a test holds those two
records against the labels' own answer. Moving the peek onto the labels is a round of its own
and is in the log as a question.

**Finding 53: the multi plot limit went away at round 113 and nobody recorded it.** Both marks
have been live for six rounds. The rules file and the `FilledMarks` docstring say so now.

Three break watches, 16, 14 and 2 red, all restored byte for byte and each checked for whether
the red names what was broken. Eight existing tests changed by hand, each because the truth
under it moved, and `LabelFixture` now escapes cell values because a cell holding `<Date>` wrote
a start tag into the sheet part.

**The audit files.** 50 and 53 carry a FIXED mark with the pass that closed them and no other
finding was touched. Counted off the three files: **63 numbered findings, 19 FIXED, 44 OPEN.**
The round message said 30 open, which leaves out the twelve of audit 3 that still stand, and
nothing was renumbered to make it right.

Pull request 119, merged into main as `afb85a9`. **The runner ran 28 hook cases and 1700 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1700 ran at `afb85a9`, 880 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**The record commit on main was first taken carrying the sixty ninth pass's message**, off a
`record.txt` that round had left in the scratchpad, because the call meant to overwrite it was
refused whole and the next call named the file anyway. Git read a real file and nothing failed.
Amended with a message written to a new name, and written up in the log: a file that is there is
not the same as a file you put there.

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Whether the real templates spell the three
labels the way the measurement says is the one thing only a run can answer, and a run says so
plainly rather than guessing.

Phase: 9, ship. Sixty ninth pass, the THIRD audit of the KPI tool. **It builds nothing and fixes
nothing**, and the only files it writes are `steps/audit-kpi-3.md`, the log and this block. **This
block is written because `require-file-on-commit.sh` refuses a commit without one**, which is the
one place the round message and the hook disagree. **1673 tests, 853 KPI, 28 hook cases**,
unchanged, and nothing under `src` or `tests` moved.

**PART A, the 32 open findings.** Six passed by, two moved in half, twenty four still stand,
which is 32. The six are 3, 20, 21, 22, 26 and 43, all from the rounds that took out the name box
and the grouping buttons. The two halves are 13 and 40. **29 and 41 stand and are worse**:
`KpiCreateReport.cs` grew to 1277 lines, and `TemplateWords.cs` now stacks two docstrings BOTH
describing the name box round 114 deleted.

**THE STRONGEST FINDING IS 50, AND IT IS THE CHARACTER AND CONTEXT FAULT THREE CELLS UP.**
`TypedByTheTeam = { "E5", "G5", "H5" }` is one array for all seven templates and the letters were
measured on one, EXISTING PARKS, with three more inferred from accepted output. Character sat at
D7 on two templates and F7 on the third because STREETS carries a Category FORMULA at D7, which
is why those two are found by their labels now. **Row 5 is still written by letter and three
templates have never been looked at.** The divergence is proven to exist in row 7 of the same
sheets.

**Findings 51 and 52 are the group row.** `IsStructureRow` needs cell 0 filled and every other
cell empty, and the group name is read off cell 0. The first cell of a species row is the IMAGE.
A group row printing with anything in that column is not recognised, its species attach to the
group above and go to that group's sheet, and nothing refuses it because the species still add to
the printed TOTAL. That is the Street Design shape again.

**Three break watches, all reddening on the right names**, 2, 5 and 4 red, all restored byte for
byte. **No test was found that would pass with its own behaviour broken**, which is a change from
the last two audits and reads as round 117's four findings having gone where the weakness was.

**The report was generated and counted rather than remembered.** One plot, no species, no schedule
read is 152 lines, **68 of them nine sections whose count is zero**, six of which print a column
header for a table with no rows. Seventeen headings carry a sentence explaining the rule, and not
one says what the heading does not.

Fourteen findings, 50 to 63, nine dropped for no cost. **No BLOCKS.** 50 and 51 become BLOCKS the
moment their unmeasured case is real, and four of the five UNKNOWNs need only a file nobody has
opened.

Pull request 118, merged into main as `734695b`. **The runner ran 28 hook cases and 1673 tests against the pull request head, 0 failed and 0 skipped, BOTH UNCHANGED, because nothing under `src` or `tests` moved. Locally the same 28 and 1673 ran at `734695b`, 853 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT AND NOTHING IN IT CHANGES THE TOOL.** Every
change since 14 September is still unrun, which is rounds 113, 114, both hook rounds and 117.

Phase: 9, ship. Sixty eighth pass, the four tests that stayed green while the code was broken:
findings 10, 11, 33 and 48. **The audit files read 49 findings, 17 FIXED, 32 OPEN**, counted off
them and moved by these four alone. **1673 tests, 33 added against the 1640 main carried** at the
branch point `4315933`, 28 hook checks unchanged. All four still stood at today's lines, checked
before anything was written.

**ONE ASSERTION PER VALUE AGAINST ITS CELL.** `ValueInItsOwnCellTests` binds all thirteen values
to their cells, one case each and two holding a whole plan, with every value a different number
or word because two totals of a size are what let a swap through. It also pins that nothing
lands on D7 on STREETS, which holds the Category formula.

**THE READ BACK, AND THE FIRST VERSION OF THAT TEST THAT DID NOT WORK.** Corrupting a cell behind
the tool's back catches nothing, because the corrupted cell is not one the run wrote, so it is
read through `ReadBack` again, which is the trap the finding names. **The break went green under
my own new test at 1666.** What catches it is a cell written TWICE in one patch: the file holds
the second value while the outcome carries one landed cell per write. The useless case is kept
with a comment saying it is useless and why.

**A RUN THAT ACTUALLY WROTE.** `CreateFixture.RunThatWrote` patches a workbook for real and hands
back a genuine `PatchOutcome.Done`, so the report and the status line can be asserted against
what landed. The status line's N cells written from M plots was asserted nowhere before.

**THE TWO COPIES OF THE REGION RULE WERE NOT THE SAME RULE.** The handler took the one region
holding an area and left two unchosen. The fixture took the FIRST region whatever it held and
however many there were. The handler's was right, and `RegionChoice.For` in Core is the one copy
now, called by both. **No test ever told them apart**, because every multi region case passes an
explicit empty choice that both copies answer alike, so the divergence sat exactly where nothing
looked. `WhyUnchosen` beside it names which of the three cases an empty choice was.

Four break watches, 4, 2, 3 and 6 red, all restored byte for byte and each checked for whether
the red case names what was broken. **Break 3's third red is not mine**, a pre-existing case
asserting the heading count is 0 on a refused run, checked rather than assumed. **Twice in two
rounds a break has reddened something other than what it aimed at**, and this round it was a
green case in a test I had just written, which is the reason that test was rewritten.

Pull request 117, merged into main as `5ff81fd`. **The runner ran 28 hook cases and 1673 tests against the pull request head, 0 failed and 0 skipped. Locally the same 28 and 1673 ran at `5ff81fd`, 853 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Finding 48 is the only item that changes
what the tool does, and it makes the running rule the tested one rather than altering it.

Phase: 9, ship. Sixty seventh pass, the four things left open by the hook round before it. **A
shared change, run alone.** Nothing under `src` or `tests`, so the dotnet suite is unchanged at
**1640, 820 of them KPI**, off the branch point `70c4abe`. **The hook checks went from 15 cases
to 28.**

**THE NOTEBOOK PATH WALKED PAST THE GUARD AND NOW CANNOT.** `block-paths.sh` is matched on
Write, Edit AND NotebookEdit and read `file_path` only, where NotebookEdit's argument is
`notebook_path`, so every notebook write arrived empty and took an `exit 0`. Measured:
`/etc/evil.ipynb` exit 0 against exit 2 for the same path under `file_path`. It reads both keys
now, **and a call carrying neither is refused** rather than waved through, which is the half
that matters: all three tools carry one of the two keys, so anything reaching that branch is
something the guard cannot see.

**ONE RECORD OF WHICH TASKS EXIST.** The list sat in `territory-check.sh` and again in
`territory.md`, six against five, View Filters missing from the file that decides who may touch
what. **Measured which was right**: ViewFilters has code in all three roots plus its own rules,
log and state files, while three of the five the document lists have no folder at all. The hook
was right. `.claude/hooks/tasks.txt` is the record now, **the hook reads it** and refuses when
it cannot, `territory.md` points at it and gains the missing entry, and a hook case refuses when
the two disagree.

**AN AMEND AND A REUSE ARE READ RATHER THAN REFUSED.** `--amend --no-edit` takes HEAD's message
and `-C <ref>` takes that ref's, so `message_of` asks git for the one the command will really
use and the hook checks it. `-c`, `--reuse-message` and `--reedit-message` go the same way.
Refusing every amend would block a flow people use daily to catch the rare bad one. **Where it
still cannot be got at it refuses and says which case**: an amend that would open an editor, a
commit naming no message, and a ref git cannot read. The last two used to pass unchecked.

**`hook-tests.sh` RUNS IN THE GATE**, its own step before the build, reading its own count back
so a run that checks nothing fails rather than reporting green. A test nobody runs is not a
test, which was the whole reason the previous round existed. The amend cases needed a HEAD whose
message is known, so the file builds a scratch repo in the temp folder carrying a copy of the
hooks, and the commit holding a credit line is made with `commit-tree` so it belongs to no
branch and never reaches this repo. Checked against an empty `HOME` too.

Six break watches, 1, 2, 1, 2, 1 and 1 red, all restored byte for byte. **Twice a break reddened
a different case than the one aimed at**, both times because a second guard caught what the
first stopped catching: the refuse-on-empty half covered the missing notebook key, and an
uncaptured `-C` ref fell through to the no-message refusal. Defence working, and a reminder that
a green case does not prove the line you think it does.

Pull request 116, merged into main as `5cf8017`. **The runner ran 28 hook cases and 1640 tests against the pull request head, 0 failed and 0 skipped, the hook step being the one this round added. Locally the same 28 and 1640 ran at `5cf8017`, 820 of the tests KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and nothing in it touches the add-in.

Phase: 9, ship. Sixty sixth pass, a shared change that runs alone. **The commit hooks could be
bypassed in silence and now cannot.** Nothing else is in this round: no KPI code, no rules file,
no test project, so the dotnet suite is unchanged at **1640, 820 of them KPI**, off the branch
point `f7fca6d`.

**A MESSAGE ON STANDARD INPUT IS REFUSED.** `commit-scope.py` skipped a message file named `-`
with a bare continue, so a commit made with a heredoc handed `writing-check.sh` an empty string
and every rule passed. The sixty fifth pass walked into it: its first commit carried a co-author
credit line and was taken without a word. The refusal now names the remedy, which is to write
the message to a file and name that file with the message flag and its path. **A hook runs
BEFORE the command it checks**, so the heredoc does not exist yet and no amount of reading finds
it.

**THE FIRST VERSION OF THAT FIX WAS WRONG AND ITS OWN COMMIT SAID SO.** It reported the reason
by printing a marker word into the message, copying what the older unreadable-file case had
always done, and the hook searched the message for it. The commit carrying it was refused by its
own message, for describing the hole and naming the marker. **A signal that travels in the data
is not a signal.** The reasons come back beside the text now: `commit_message` hands back the
text and what could not be read, `main` exits non-zero with the reasons on standard error, and
the hook reads them off standard error. **Both markers are deleted, the older one included.** A
case pins it, a message that talks about both failures while being neither, and that case is red
against the marker version.

**One correction to the last round's log.** It said all three hooks scanned an empty string.
Only `writing-check.sh` asks for the message. The other two ask for paths and never read it.

**A SECOND INSTANCE IN THE SAME FILE.** `read_commit_call` set a `found` flag for the
unreadable-command case and its own comment said to let the caller fail closed. No caller read
it, so an unbalanced quote was answered off the index, to all three hooks. The script exits
non-zero now, because all three already refuse on that, and a `readable` flag carries it apart
from `found` so an ordinary line holding no commit still passes.

**`hook-tests.sh` is new and green on 15 cases.** It drives the real hooks with the payload
Claude Code sends and checks the exit code. The three the round asked for are there: a message
file it can read passes, one it cannot read refuses, and standard input refuses. It also holds
the unreadable command against all three hooks and checks the ordinary shapes are still read.
**It runs nothing in the gate**, which is dotnet only, so it is run by hand, and wiring it in is
a question rather than a thing done here.

Three break watches, 1, 4 and 2 red, all restored byte for byte. **The second found a gap in my
own test before it found anything else**: against the first version of the fix it reddened 2 of
3, because require-file refused for its own reason off an empty index, so a direct case on the
scope script went in ahead of the hook cases.

**The other three hooks were read path by path and two were probed. Nothing found in
`require-file-on-commit.sh` or in `territory-check.sh`'s own logic**, both fail closed at every
exit including a fault inside territory-check's embedded python. `writing-check.sh` skips a
binary file in silence, which is left alone on purpose and named so it reads as looked at.
**`block-paths.sh` has one real hole and it is NOT fixed here**: it is wired to NotebookEdit and
reads `file_path`, where NotebookEdit's argument is `notebook_path`, so it passes every notebook
write. Measured, and the repo holds no notebook. It is one line and it is Bader's call.

**Recorded in `CLAUDE.md`**, under the things that have gone wrong, because that is what both
sessions read and neither owns, so the Drawing Sheet session gets it too.

Pull request 115, merged into main as `7f10019`. **The runner executed 1640 tests against the pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `7f10019`, 0 failed and 0 skipped, 820 of them KPI, and `hook-tests.sh` is 15 of 15 there.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and nothing in it touches the add-in.

