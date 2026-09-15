# ai-max state, KPI

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

Phase: 9, ship. Sixty fifth pass, four things off the first per plot run. NG05 at 00:00, 99
plots, 98 workbooks. **The tree is right, both files recalculate with zero errors and the street
reference file works**: ANH-007-ST-100217 came out ROW 36, length 928.782391, and the workbook
computed the area at 33,436.17 itself. **The audit files stay the one record and this round
closes none of their findings.** Everything here is wording or a count. Nothing new reads a
model.

**CHARACTER AND CONTEXT ARE FOUND BY THEIR LABELS AND NEVER BY A LETTER.** Measured by Bader off
the three workbooks written on 13 September: mosques and schools carry Character at C7 with its
value at D7 and Context at E7 with its value at F7, and streets carry a Category FORMULA at D7
with Character and Context one pair to the right at F7 and H7. A map holding D7 because two of
three templates say so would have destroyed the street sheet's own calculation. `FixedCells.In`
opens the template when Create is pressed, reads its main sheet, looks for the label and writes
the cell to its RIGHT. The street's Category is never touched because nothing looks for the word
Category. A label matched whole and without case, a template naming neither writing nothing and
saying so, a label found twice writing nothing and naming both cells, and `LabelledCell.Holds`
carrying what the cell already held, because the mosque template came filled and the report must
not read as though this run put those values there. `KpiCreatePlan.Of` takes the labels as a
thirteenth REQUIRED argument and throws on null.

**The name box is gone and `OutputName.Suggested` is DELETED on the SECOND of the two reasons.**
Reachability is the first and it is the test that was wrong last round. This is the other one:
the shape it recorded no longer exists, because there is no one file per template any more. One
line stands where the box was, saying the root, the component folder, the UID2 and the workbook
named after its folder, with one real path. `Final` and `Extension` stay, because a file still
has to be named.

**COUNT WHAT HAPPENED, NEVER WHAT WAS PLANNED.** The run read MOSQUES: Nothing was written. 20 of
21 plots wrote a workbook, beside twenty workbooks on disk. `TemplateOutcome` counts workbooks
now, `WroteSomeOfThem` is the case that had no word before, and **refused is kept only where
NOTHING was written**. The summary line read 1 workbook written of 2 templates ticked on a run
that wrote 98, so it counts workbooks, plots and templates each as itself.

**The create block is one line per template with its notes under it.** `CreateWords.Range` names
up to four plots and gives the count and the first to last beyond that, so 78 street plots read
as a count and a range rather than 78 names. A note and a refusal no longer look alike: the
links, the groups left out and a template with no ticked plot go through a new `Noted` in the
ordinary text colour. `LinksLoaded.OnThePane` gives the count and what to do rather than six
lines of paths. The STREETS area line said the number is typed by hand and it is not, so it now
says the sheet works it out from the road width and the total length off the reference file.
**Nothing moved and no control changed.**

Four break watches went red on 5, 1, 1 and 1 tests and were restored byte for byte, each checked
with a diff against its backup. The first is the one that matters: taking D7 as the value cell,
which is what two of the three templates use, reddens the street test that exists to stop the
formula being overwritten. Seven existing tests were changed by hand, each because the truth
under it moved.

**OPEN, FOR BADER: non-permeable hardscape is blank on every workbook and no note mentions it.**
Nobody has said where it comes from and nothing here names a schedule, a parameter or a filter
that would produce it. It is written in the log as a question rather than guessed at. Three
things would settle it: which schedule or parameter holds it, whether it is an area or a count,
and whether a plot with none prints a zero or stays blank.

**`writing-check.sh` CANNOT SEE A COMMIT MESSAGE PASSED ON STANDARD INPUT.** This round's first
commit used `-F -` and carried a co-author credit line, which that hook exists to refuse, and it
was taken in silence. `commit-scope.py` skips a message file named `-` with a bare continue, so
all three commit hooks scan an empty string. The commit was reset and made again through a file
the hook can open. **The hook is not changed here**, because it belongs to no task and every
session depends on it. It is written up for Bader in the log with the one line fix.

**Locally 1640 tests at this branch, 0 failed and 0 skipped, 820 of them KPI, 15 added, against
the 1625 main carries** at the branch point `70fdb82`. Build zero warnings. Pull request 114, merged into main as `9c865b1`. **The runner executed 1640 tests against the pull request head, 0 failed and 0 skipped. Locally the same 1640 ran at `9c865b1`, 0 failed and 0 skipped, 820 of them KPI.** The merge went through the API with the title and the message both passed on the call, the commit came back off main carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte the branch head over src, tests, .claude and steps.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.**

Phase: 9, ship. Sixty fourth pass, one workbook per plot in a folder tree, off how the team
really files these. **The audit files stay the one record: counted off them today, 49
findings, 13 FIXED, 36 OPEN, and this round closes none.** The round message said 32 open. That
is not what the two files say and nothing here was renumbered to make it so.

**A CHECKLIST IS ONE PLOT.** Tick MOSQUES and press once and you get 20 folders and 20 workbooks
rather than one file with 20 plots added together. `PlotWorkbookPath` builds
root/COMPONENT FOLDER/UID2/UID2.xlsx off the team's own folders, the plot gets a folder of its
own so the PDF asked for later has somewhere to go, the folder is made where it is not there and
never deleted, and the overwrite is unchanged. **NOTHING ABOUT READING A PLOT CHANGED**: the
handler still reads a whole share in one pass and `OnePlot` fills, patches and files each
reading on its own, with the merge functions taking a list of one.

**`ComponentFolders` is a THIRD table off PRX_Component.** One says which workbook a plot is
filled from and this says which folder it is filed in, and neither derives from the other: the
two mosque values share one template and get two folders. **Eleven values reach EIGHT folders**,
counted off Bader's own table, where the round message said nine, and GOVERMENT BUILDING is
deliberately out of it. That count is an open question rather than a thing I guessed at.

**`StreetReferenceFile` fills D8 and F8 on STREETS**, browsed for and remembered like the other
two, read by the names in the header row. Measured on the real file: the four columns sit at D,
H, I and O with no header over B at all, 313 rows are ANH-007-ST, 33 UIDs are on two rows and
none of them is in ANH-007, and **the file writes an absent value as the text Null rather than
an empty cell**, on 2,051 rows. Nothing is estimated from the component value, and the file
proves why: 156 of the 313 street plots read a width of 15.

**A platform answered differently from the machine the tool runs on.**
`Path.GetInvalidFileNameChars` names nine characters on Windows and two on the Linux runner, so
a UID2 reading ANH*007 was refused by the tool and accepted by the test meant to check it. The
nine are data now and my own test caught it on the first run.

**`FixedCells` holds Character as Urban Area Zone and Context as Urban, and WHICH CELL EACH GOES
IN IS UNKNOWN.** Neither word is measured anywhere in this repository, so the map names no cell,
every template reports both as not written with that reason in every run, and nothing is
guessed. That is the one part of this round that is open rather than done.

**The output folder is taken as the root**, a judgement, because the round's two sections
disagree about how many browsed things there are and a folder deciding nothing is a dead end
this file already records. The pointer file is unchanged so nobody's setting is lost.

**Nothing is deleted.** What is no longer reached is listed in the rules with why each was kept:
the merged species list across plots, the identical raw area check, the sum against the printed
total, the plot counted into two workbooks and the output name box. **The rounding room on a
group total does NOT stand down**, against the round message, because it is per schedule and per
plot and nothing about it moved.

Five break watches went red on 3, 2, 9, 3 and 1 tests and were restored byte for byte, each
checked with a diff against its backup. **The fifth found a gap in my own test rather than in the
code**: a plot that gets neither a folder nor a workbook was not in the accounting test, and it
is now. Six existing tests were changed by hand, each because the truth under it moved. **Locally
1625 tests at this branch, 0 failed and 0 skipped, 805 of them KPI, 47 added, against the 1578
main carries** at the branch point `386948e`. Build zero warnings. Pull request 113, merged into main as `ae9c324`,
the runner executing 1625 tests against the pull request head, 0 failed and 0 skipped, and
locally the same 1625 ran at the merge, 805 of them KPI. **The merged tree is not byte for byte
the branch head**: Bader uploaded `Branded_Factsheet_Template.docx` straight to main at `57e606a`
between the branch point and the merge, and every file this round touched landed intact, checked
by diff over src, tests, .claude and steps. GitHub was in a declared major outage on Pull
Requests throughout, which cost three 500s and a 502 and was retried rather than worked around. **NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and the
five correct workbooks are what a per plot press has to be held against.

Phase: 9, ship. Sixty third pass, two answers from Bader and one short piece of work. **The
audit files stay the one record: 49 findings, 13 FIXED, 36 open, and this round closes none.**

**The Read this model button stays and is agreed.** Bader's answer: the round message's wording
was wrong, the rule is that nothing reads without a press, a button IS a press, and the fault was
a read nobody asked for rather than a way to ask for one. He also placed it, better than KPI
Scan was, which sat at the top and on the ribbon whether or not anybody needed it, where this
sits in the plots block with the thing it produces. The judgement note in the rules is replaced
by that record. **No code changed for this item.**

**All seven leftover members are deleted, on what each one RECORDS rather than on reachability.**
`CreateWords.GroupsHeading`, `GroupLabel` and `NoGroupFor`, `PlotTicks.OnlyFor`, and
`PlotPrefixes.Grouped`, `WithNoKnownPrefix` and `PlotsFor`, each with the place its shape lives
named in the log and in the rules: the prefix docstring, `CreateWords.TemplateRow`,
`TemplateSplit.Unplaced`, `TickingATemplate`, `PlotsPerTemplate.Split`, `PlotPrefixes.Across` and
`PlotPrefixes.For`. **They came out the same because they are one feature's parts and not seven
things**, and what was worth keeping was never among them: the prefix table is the measurement
and `All`, `Of`, `For`, `Across` and `PrefixesFor` all stay. Two were worse than unused.
`OnlyFor` implemented REPLACE, a rule the tool decided against, and `NoGroupFor` told the reader
to tick such a plot by hand, which now lands it in `Unplaced` writing nowhere.

**`PrefixesFor` is the one kept on that test and it is not one of the seven.** Nothing calls it
either and it is the only record of the many to one shape, three prefixes meaning STREETS and two
MOSQUES, which is invisible read the other way off `For`. Its docstring says that and says
nothing calls it, which is the comment Bader asked for put where it applies.

Four break watches went red on 3, 2, 1 and 1 tests and were restored byte for byte, each checked
with a diff against its backup. The first reddened `OnlyFor`'s test through `OnlyFor`, so a
deletion two members deep was watched rather than assumed. **`GroupsHeading` had nothing to
watch**, no caller and no test anywhere, which is its own finding. Five tests went with the
members and one of mine was rewritten rather than deleted, the contrast in
`ItTicksByTheSplitsRuleSoNoTickedPlotCanLandInNoWorkbook`, which now names the two answers for
DM-12 side by side instead of comparing two lists. **Locally 1451 tests at this branch, 0 failed
and 0 skipped, 758 of them KPI, 5 fewer than the 1456 main carries** at the branch point
`6e1b309`. Build zero warnings. Pull request 104, merged into main as `e2e14a3`, the runner
executing 1451 tests against the pull request head, 0 failed and 0 skipped, and locally the same
1451 ran at the merge, 758 of them KPI, the merged tree byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and nothing in it changes what the tool
does at run time.

Phase: 9, ship. Sixty second pass, three things off the first press over several templates,
NG05 at 08:37. Two faults and one regression of mine. **The audit files stay the one record: 49
findings, 13 FIXED, 36 open, and this round closes none.**

**NOTHING HEAVY RUNS WITHOUT A PRESS.** Opening a model started a read with no press behind it,
because `Took` asked for the plots off any title it had not read, and a dockable pane is
restored visible at Revit startup. Five read paths were gone through one by one and four were
already right: the two `WhichModel` asks are the title alone and free, Create is a press, and
the KPI pane subscribes to DocumentOpened and DocumentClosed nowhere. The third was the fault
and is gone. `KpiHeader.Lines` in Core is the header, three states and **only the third names a
count**, because a read that has not happened is an absence rather than a zero.

**The plot read now sits behind a press in the plots block, and that press is my judgement**
rather than a line Bader wrote: the picker cannot be used before the plots exist. It is one
control where this round removes a row of them, and it is written into the rules as a judgement
so it is cheap to overrule.

**Ticking a template row ticks its plots**, and the row IS the grouping button, so the separate
row goes. `TickingATemplate` ticks by the SPLIT'S rule and never by the prefix, because a plot
ticked by the prefix could land in no workbook at all. A plot ticked or unticked by hand wins,
the hand list clears with the model, the row's count is what will really go in, and a template
with no plots still ticks and still writes nothing. A row ticked before the read gets its plots
when the read lands.

**`OutputName.Suggested` is restored**, asked per row with that row's own file, so the boxes
read GRP-KPI-Checklist-DD-MOSQUES.xlsx again rather than MOSQUES. Deleting it last round on
reachability alone was wrong and that is now a rule: a method the last caller stopped calling
can still be the only record of a shape.

**What the grouping buttons did that the rows do not**, asked for in the round: they replaced
the ticks rather than adding, and they could tick a template's plots with its workbook unticked.
Both are deliberate now. Seven members are left reachable only from tests and **not deleted**,
because of the lesson directly above. Whether they go is Bader's call.

The round changes the pane, so it carries `design/pr-103/kpi-pane.html`, hand drawn from
the code: the table of every read path, the header's three states side by side, the plots
block before and after the one press with the grouping buttons struck through, and a
ticked template with a plot taken off by hand reading 19 rather than 20.

Three break watches went red on 1, 2 and 1 tests, one per item, and were restored byte for byte,
each checked with a diff against its backup. **Two existing lines were changed by hand**, the
plots block's first two states, which both promised the read that was the fault. **Locally 1456
tests at this branch, 0 failed and 0 skipped, 763 of them KPI, 13 added, against the 1443 main
carries** at the branch point `d6c9f4a`. Build zero warnings. Pull request 103, merged into
main as `f41b044`, the runner executing 1456 tests against the merged head, 0 failed and 0
skipped, and locally the same 1456 ran at the merge, 763 of them KPI, the merged tree byte
for byte the branch head. **NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and whether opening
a model is quick again is the one thing only a run can answer.

Phase: 9, ship. Sixty first pass, round two of the two Bader sent together, off a fresh pull of
main carrying round one. **The audit files stay the one record: 49 findings, 13 FIXED, 36 open,
not renumbered and not reordered, and this round closes none.**

**Several templates in one press, one workbook each.** The workbook rows are tickable, several
at once, each ticked row carrying the template it is and its own output name, because one box
cannot name six files. **NOTHING ABOUT THE PER TEMPLATE LOGIC CHANGED**: `KpiCreateRun` is still
one template's run and `KpiCreateReport.Write` still prints one template's sections, both
untouched, and what is new sits above them.

**`PlotsPerTemplate` is the split**, the rule this repository already had for preselecting asked
per plot: the component decides, the prefix cross checks, and where they disagree neither does.
A component the table does not hold places NOTHING rather than letting the cross check answer in
its place. A plot with no component is placed by its prefix, which is what EP-05 and its three
need. Every plot that goes into no workbook is named.

**A plot in two workbooks refuses the whole press**, checked over the split as it really came
out rather than trusted to the rule that built it, with two tests because a construction that
cannot go wrong is not a check.

**Read once and no second cache.** A plot belongs to one template so it is read once, and
`HeldReadings.Decide` is asked per template with that template's own share and its own held run.
The progress count runs across the whole press, since counting per template would restart it at
1 on the second workbook and the rule says the count only grows.

**One report for the run**, the accounting first, then the split, then each template's own
sections under its name. The four run counts, ticked, written, refused and nothing to write,
must add up to the number ticked. A refusal on one template does not stop the others and each
row says what happened to it. The three typed fields are one set for the whole run and the pane
says so when more than one template is ticked.

`OutputName.Suggested` is deleted with its test, unreachable once every row took its name from
`CreateWords.SuggestedName`.

The round changes the pane, so it carries `design/pr-102/kpi-pane.html`, hand drawn from the
code: the workbook list before and after, the create block before and after the press on both
themes, and the report's own opening. It says in the file that it is a mockup and not a
screenshot.

Three break watches went red on 2, 1 and 2 tests and were restored byte for byte, each checked
with a diff against its backup. **One existing test was changed by hand**, the refusal that read
No template picked and now reads No template ticked. **Locally 1443 tests at this branch, 0
failed and 0 skipped, 750 of them KPI, 27 added, against the 1416 main carries** at the branch
point `96b6239`. Build zero warnings. Pull request 102, merged into main as `131b1dd`, the
runner executing 1443 tests against the merged head, 0 failed and 0 skipped, and locally the
same 1443 ran at the merge, 750 of them KPI, the merged tree byte for byte the branch head.
**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT**, and the two correct workbooks, MOSQUES on
NG03 at 15:52 and STREETS on NG05 at 01:30, are what a press over two templates has to be held
against.

Phase: 9, ship. Sixtieth pass, round one of two Bader sent together, built and merged on its
own so that a single merge cannot hide which half broke anything. **The audit files stay the one
record: 49 findings, 13 FIXED, 36 open, and this round closes none.**

**The third blocker does not bite and its UNKNOWN is closed.** Bader measured the 1552
workbook: Tree List - Existing holds 98 names on rows 4 to 101 with no empty row inside the
list and its first gap at 102, Proposed 80 names on rows 4 to 83 with its first gap at 84. Row
101 is above the first empty row, so `BelowTheList` never reaches it. **No behaviour changed for
that item**, and what was an UNKNOWN in the log is a measurement with its file and its date.

**The name rule is an alias, not a rule.** `SpeciesAliases` in Core is a table of one entry,
UNKNOWN means Unknown Tree. No rule could do it: one name in 98 opens with UNKNOWN, and a
shared opening, a prefix or a longest match also reaches Conocarpus, Ficus and Prosopis, where
it would have to pick a species. Three guards, all tested. It applies only where it resolves to
exactly one row on that sheet, two rows being a refusal naming every one. It never overrides a
real match, the exact name being matched above the table. And the report names it, a how column
on SPECIES MATCHED and a block of its own with the alias, the sheet, the row and the count, so
a count resting on a decision of the team's never reads like one that matched word for word.
`ClosestName` is untouched, still printed beside every remaining miss and still never matched
on.

`OnTheRow` is the one place a row becomes a match now, asked by the exact name and by the alias
alike, so the total's reach is one rule rather than two that would drift.

**Street Design counting as Proposed on STREETS now has a run**, the 01:30 STREETS workbook on
NG05, ST-05 reading 369 existing, 2 proposed and 68 Street Design taken as proposed against a
printed TOTAL of 439. The never exercised note comes out of the log.

Two break watches went red on 2 and 1 tests, one per guard, and were restored byte for byte,
the second checked with a diff against its backup. **One existing test was changed by hand**,
the old UNKNOWN takes no row test, whose four Unknown Tree rows now meet guard a. **Locally
1425 tests at this branch, 0 failed and 0 skipped, 732 of them KPI, 9 added, against the 1416
main carries** at the branch point `96b6239`. Build zero warnings. Pull request 101, merged
into main as `3a3312f`, the runner executing 1425 tests against the merged head, 0 failed and
0 skipped, and locally the same 1425 ran at the merge, 732 of them KPI, the merged tree byte
for byte the branch head. **Nothing in this round has been observed in Revit**, and the
check that matters, Total Trees moving 374 to 390 with the canopy still 11,168, waits on the
next run.

Phase: 9, ship. Fifty ninth pass, two items measured on two runs. **The audit files stay the
one record: 49 findings, 13 FIXED, 36 open, and this round closes none.**

**Item 1(b) was already correct and the round found it out rather than fixing it.** The no
diameter rule already applies to the empty row route alone, `NotSized` is built nowhere else,
and a matched row already gets its count in column B with the client's own cells untouched.
There was one reason UNKNOWN went unwritten on the 1552 run, not two, and it is the name. The
rule is pinned by a test now instead of being an assertion in a log entry. **A third blocker
nobody named is in the way too**: an exactly matching name held past the list's first empty row
is refused, and whether that bites for the real row 101 is UNKNOWN here, because no workbook and
no report is in this repository. The create report already prints those names and their rows,
so the 1552 report answers it off the file.

**The match is not widened and no rule was invented.** `SpeciesMatching.ClosestName` prints the
workbook name sharing the longest opening with each unmatched Revit name, as the second column
of the unmatched species list, printed and never matched on. The 1552 run's misses cannot be
enumerated from this repository, so the next run is what tells Bader whether it is one name or
a family of them. **The name rule itself is his to give and this round does not choose one.**

**A run with no link loaded now says so in three places.** `LinksLoaded` in Core is five states
with one line each, a note and never a refusal: at the top of the checklist report above the
reconciliation, on the pane above Create before the press, and beside two counts that cannot
read green over an empty run, `GroupRowsFound` and `SchedulesWithABody`. The 16:06 STREETS run
reads 0 group rows and 0 of 156 schedules with a body where every count it already carried read
green.

**STILL NEVER EXERCISED: Street Design counting as Proposed on STREETS.** It has tests and no
run. The words appear nowhere in the 2,292 line report of the only STREETS run there has been,
and they could not, because no link was loaded so no schedule printed a body. ST-05's 369, 2,
68 and 439 read off a screen are the only measurement behind the whole rule.

The round changes the pane by one line, so it carries a mockup, `design/pr-97/kpi-pane.html`,
hand drawn from the code, saying in the file that it is a mockup and not a screenshot and that
the link names in it are invented for the drawing.

Two break watches went red on 3 and 4 tests, one per item, and were restored byte for byte. The
first attempt at the second was a no-op that changed no behaviour and is recorded as one.
**Locally 1332 tests at this branch, 0 failed and 0 skipped, 723 of them KPI, 16 added, against
the 1316 main carries** at the branch point `e3ba145`, measured there in a worktree rather than
remembered. Build zero warnings. Pull request 97, merged into main as `6275bf3`, the runner executing
1385 tests against the merged head, 0 failed and 0 skipped, and locally the same 1385 ran at
the merge, 723 of them KPI. **The merged tree is not byte for byte the branch head**: two
Drawing Sheet rounds, `d77b90a` and `e2aa4ad`, landed while this one waited on a GitHub rate
limit and are the whole of the jump from 1332 to 1385, none of them KPI. The KPI half did
land byte for byte and its count is 723 on both.
**Nothing in this round has been observed in Revit.**

Phase: 9, ship. Fifty eighth pass, item 1 of the two Bader asked for, on its own so the first
real run can say which half broke anything. **Item 2, several templates in one run, is NOT in
this round.** The audit files stay the one record: 49 findings, 13 FIXED, 36 open, and this
round closes none.

**Create scans if it needs to and the scan button is gone.** `ScanNeeded.Decide` in Core reads
the model when no scan is held or what is held is of another model, and uses the scan already
held otherwise, the same shape `HeldReadings.Decide` uses for the readings. The press says
which it did. KPI Scan is off the pane and out of the request enum, and the ribbon tooltip no
longer names it. The header's element count comes back with the plot read now, so the model's
name has a number under it as soon as the pane is shown. The scan report is unchanged, still
written and still named, and its headline moved to `Progressed` because `Told` is the end line
that shuts the progress window.

**The progress window is modeless, owned by the Revit main window, and opened outside
`Execute`**, closed by the run's end line whatever ended it. **There is no Cancel, and that is
this round's one deliberate refusal**: cancelling mid read leaves a half read set of plots the
reconciliation would count as read, which is the state the rest of the tool would trust and
should not, so an honest cancel is its own round.

A breaker on the diff died on a rate limit, so the checks were made here: every path that ends
a press reaches `Told` so the window always shuts, the scan runs after the refusal guard, the
header cannot be wiped by the redraw that follows it, and nothing still references the removed
request. **One real limit is known and left**: a second press while the first run is going
leaves the second run without a window, because a busy flag that failed to clear would leave
Create dead with no way back.

The round changes the pane, so it carries a mockup, `design/pr-93/kpi-pane.html`, hand drawn
from the code: the strip before and after in both themes, the window on four of its real
lines, and the order a press says them in. It says in the file that it is a mockup and not a
screenshot.

Three break watches went red on 3, 1 and 1 tests and were restored byte for byte. **Locally
1289 tests at this branch, 0 failed and 0 skipped, 707 of them KPI, 7 added, against the 1282
main carries** at the branch point `b5e2c90`. Build zero warnings. Pull request 93, merged
into main as `ffccfce`, the runner executing 1289 tests against the merged head, 0 failed and
0 skipped, and locally the same 1289 ran at the merge, 707 of them KPI, the merged tree byte
for byte the branch head. **Nothing in this round has
been observed in Revit**, and the window appearing, shutting on every path and the line moving
mid run are all waiting on the first run.

Phase: 9, ship. Fifty seventh pass, two things off the 13:48 run on RCRC_NG03_EZ. **The audit
files stay the one record: 49 findings, 13 FIXED, 36 open, and this round closes none.**

**The plots block said open a model while a model was open.** A workflow told Bader's two
candidates apart from the real cause and neither was it: the repaint pump was born at render
and never ran at background, and the progress pumps run only inside an Execute after the one
request slot is emptied. The real cause is older than both: `Ask(Plots)` lived only in `Shown`,
that one ask was lost when a pane restored visible at startup consumed it against no document,
or a scan displaced it in the one slot, and nothing asked again, so a scan filled the header
while the block drew its waiting text, whose words were open a model. Fixed in two halves:
`CreateWords.PlotsBlock` reads four ways with a Core test, no document says open one, a
document not yet answered says it is reading, a document answered empty says the model holds
none, a document answered with plots hands to `PlotSources`; and `Ask(Plots)` moved from
`Shown` to `Took`, the one place that knows which model answered, guarded by title and reset on
close, so the plots are read once per model and a model opened under the pane is read the moment
any request answers with it. The recovery is untestable in Core and waits on a run.

**The ten xUnit warnings are cleared**, four xUnit2029 rewritten from `Assert.Empty(x.Where(p))`
to `Assert.DoesNotContain(x, p)` and six xUnit2031 from `Assert.Single(x.Where(p))` to
`Assert.Single(x, p)`, the assertion changed and never the subject, none changed what it checks.
Build zero warnings.

Two break watches went red on 1 and 1 tests and were restored byte for byte. **Locally 1289
tests at this branch, 0 failed and 0 skipped, 700 of them KPI, 1 added, against the 1288 main
carries** at the branch point. Committed in two, the warnings as checkpoint `54ab011` then the
plots fix. Pull request 92, merged into main as `c8bd1b8`, the runner executing 1282 tests
against the merged head, 0 failed and 0 skipped, and locally the same 1282 ran at the merge,
700 of them KPI. The drop from 1289 to 1282 is the Drawing Sheet sixtieth pass `a7cd601` that
landed in between and net removed seven tests, none of them KPI. **Nothing in this round has
been observed in Revit**, and whether the recovered pane shows the plots on the next 13:48
style run is the one thing waiting on Bader.

Phase: 9, ship. Fifty sixth pass, Bader's answers to two of the fifty fifth pass's four open
questions, no behaviour changes beyond the two lines. **The audit files stay the one record:
49 findings, 13 FIXED, 36 open, and this round closes none.**

**No ceiling on the rounding room, a line instead.** Both measured models round areas to 1,
so a coarser step is hypothetical and a ceiling chosen today is a constant pretending to be
a rule. A project whose step is coarser than the metre opens the create report with one line
before anybody reads a number, the step and the room per row in square metres. The unit that
gated the checks travels on `KpiCreateRun`, a reused press carrying the held run's forward.
**The pane counts the run's rounding notes beside the written cells**, one sentence saying
they are in the report, singular when one, nothing when none, because a note only the report
file holds is a note nobody reads.

Two break watches went red on 2 and 1 tests and were restored byte for byte. **Locally 1266
tests at this branch, 0 failed and 0 skipped, 699 of them KPI, 5 added, against the 1261
main carries.** Pull request 85, merged into main as `52e4395`, the runner executing 1266
tests against the merged head, 0 failed and 0 skipped, and locally the same 1266 ran at the
merge, 699 of them KPI, the merged tree byte for byte the branch head. **Nothing in this round has been observed in Revit.** Two things wait on
Bader, both on the next 18 plot NG03 run: whether the status line visibly repaints mid run,
and the rounding note's first sighting, now counted on the pane beside the written cells.

Phase: 9, ship. Fifty fifth pass, two things off the 1208 run on RCRC_NG03_EZ, the first run
on a second model, where the reading reuse held, a 123 second scan and a 2.5 second create.
**The audit files stay the one record: 49 findings, 13 FIXED, 36 open, and this round closes
none.**

**The group total check tolerates the unit's rounding on areas and never on counts.** FM-21
and FM-22 refused with counts exact and areas off by one in opposite directions, which is
rounding. Counts still refuse exactly. Areas get half the rounding step per row summed, read
off the project units by `KpiReader.AreaUnit` and never a constant, within the room is a
`RoundingNote` printed beside the group total row, outside still refuses naming the room, and
an unread step allows nothing and says so. Both measured plots are tests and go through noted.
Every place printed numbers are added against a printed total is named in the log, the ones
already right included, and the species sum record the forty seventh pass chose, which was
computed and printed nowhere, prints now.

**The status line moves while a run does**, driven by what is done, never a timer. The scan
announces each section off the report's own headings and counts the schedules, Sections 5 to 8
of 9, schedules, 400 of 951, 42%. Create names the plot, Reading DM-44, plot 3 of 18, 11%,
then the steps name themselves through the patcher's own callback. Percentages only where the
total is known, floored, off counts that only grow. `ProgressWords` in Core holds the words
and the counting with tests, and the end line still comes through `Told` on finish, refusal
and throw. **Whether a line set mid run visibly repaints on a real pane is UNKNOWN until
somebody runs it**, the pane can share Revit's thread, and `Moved` pumps one render priority
job after each line so the paint can get through when it does while every queued click stays
queued until the run returns.

**A breaker read the committed diff before the pull request went ready and five of its ten
findings changed code**: a phased group printing no total row is said in the report to have
gone unchecked, the note's numbers print six places so a fine step cannot read off by 0
within the 0, the species record's 0.005 constant gate became the shared drift epsilon with
six place numbers, a reused press says Reusing the readings already held on the live line,
and the pump moved from background to render priority because background would have
dispatched queued clicks inside `Execute`, the two ownerless folder dialogs included. Found
and left with reasons in the log: the room has no ceiling on a coarse unit, the rounding
note is in the report file and not on the pane, a zero or negative accuracy would print as
not read on a value nobody has seen, and `Repeats` prints nowhere, older than this round.

Seven break watches went red on 3, 1, 2, 1, 1, 1 and 1 tests and were restored byte for
byte. **Locally 1246 tests at this branch, 0 failed and 0 skipped, 694 of them KPI, 22
added, against the 1224 main carries.** Pull request 81, merged into main as `8578f86`, the
runner executing 1261 tests against the merged head, 0 failed and 0 skipped, and locally the
same 1261 ran at the merge, 694 of them KPI, the 15 above the branch's 1246 being the Drawing
Sheet fifty sixth pass that landed in between and touches no KPI file. **Nothing in this round has been observed in
Revit.** Four things wait on Bader: the first run that shows whether the status line
repaints mid run, the rounding note's first sighting on a real report, whether the room
wants a ceiling on a project rounding coarser than the metre, and whether the pane should
say beside the written count that notes exist in the report.

Phase: 9, ship. Fifty fourth pass. **The audit files are the one record of what is open: 49
numbered findings, 13 carrying a FIXED mark, so 36 open, read off
`steps/audit-kpi.md` and `steps/audit-kpi-2.md` and counted nowhere else.** Eight marks were
added this pass after each was verified at today's lines, 1 and 8 to the fortieth pass, 2, 30,
31 and 32 to the forty fifth, 5 and 36 to the forty sixth. Six of the nineteen believed closed
are not and stay open: 3, 4, 6, 12, 21 and 26, the round that has never landed on main, each
found byte for byte in its finding's shape. No finding's text changed, none renumbered, none
reordered.

**The diameter alone decides whether a species the list does not hold gets an empty row**,
measured on the MOSQUES template row 21: L reads J, M reads L and the count, O reads N, which
is typed, and nothing reads I or K. A species with a diameter and no height is written and its
height cell is named as not written,
`ASpeciesWithADiameterAndNoHeightIsWrittenAndItsHeightCellIsNamed`, and a species with no
usable diameter is still withheld, UNKNOWN included, whose DM-25 row 19 prints 0. Two break
watches went red on 1 and 9 tests and were restored byte for byte. **Locally 1221 tests at
this branch, 0 failed and 0 skipped, 672 of them KPI, none added net**, the same 1221 main
carries. Pull request 78, merged into main as `a407284` through the API with the squash
message on the call: **the runner executed 1224 tests against its merged head, 0 failed and 0
skipped, and locally the same 1224 ran at `a407284`**, 672 of them KPI. The 3 above the
branch's own 1221 are the Drawing Sheet round that landed in between, `b7d41dd`, whose files
are the only ones that differ from this branch and none of them KPI. The merge commit carries
no co-author line and no generated-by footer. **Nothing in this round has been observed in Revit.** One thing recorded for the
round that closes finding 4, in the log: `PaneChoicesTests` pins the unfixed behaviour and
must move with that fix.

Phase: 9, ship. Fifty third pass, one rule measured on the 1836 run over 20 mosque plots. **No
audit finding is closed here**, because this is a fault off a run rather than an audit entry,
and none was renumbered or annotated. The count in the round message, 32 open, does not
reconcile with the two audit files at this branch, which hold 49 numbered findings and 5 FIXED
marks. That is written down in the log rather than resolved.

**A species the model does not size gets no row at all.** 34 cells were ready, the run wrote the
workbook, the formula check found 7 formulas that would read an error and deleted it, and every
one traced to UNKNOWN written into Tree List - Proposed row 85 with a name and a count and no
canopy diameter, because DM-25 row 19 prints nothing for its height and 0 for its diameter.
`SpeciesMatching.WrittenInto` asks both measures before it takes a row. A species missing either
is named with its count and the reason, which quotes what every row printed, and the run goes
through. **The canopy guard is untouched, byte for byte.** The size is asked before a row is
taken so nothing is used up, the sheet's own total is still said first, and both measures are
required, which is Bader's wording rather than a measurement.

**The report says how many trees went nowhere and out of what**, NOT WRITTEN, THE WHOLE RUN: 1
tree of 528, over every species this run merged, off the one list of matches the section above
prints from.

12 existing tests went red, which is what a rule change should do. Nine were fixtures that
predate measures and three were the old rule written down. **The blanket fixture fix was itself
a fault and the review caught it**: it gave UNKNOWN a size, and UNKNOWN is the one species
measured to print neither a height nor a diameter. Every test that names UNKNOWN says what the
model says now.

Three break watches went red on 10, 1 and 1 tests and were restored byte for byte. One of them
passed first time against a test that was not testing what it said, which is how that test was
corrected. **Locally 1187 tests at this branch, 0 failed and 0 skipped, 672 of them KPI, 5 added
net, against the 1182 main carries.** Pull request 75, merged into main as `be72119` through the
API with the squash message on the call: **the runner executed 1198 tests against its merged
head, 0 failed and 0 skipped, and locally the same 1198 ran at `be72119`**, 672 of them KPI. The
11 above the branch's own 1187 are the Drawing Sheet round that landed on main in between,
`f72834e`, whose files are the only ones that differ from this branch and none of them KPI. The
merge commit carries no co-author line and no generated-by footer. **Nothing in this round has been observed in Revit.** Two
things wait on Bader, both in the log: whether both measures should be required or the diameter
alone, and what a withheld count costs.

Phase: 9, ship. Fifty second pass, one correction on finding 39's filled file test, measured
by Bader on two template sets. **The 38 open audit findings stay open**, not renumbered, not
reordered, not annotated, and 39 keeps its FIXED mark with the correction under it.

**A workbook is filled when a cell the tool writes holds something the tool would have written**,
`FilledMarks`, never when one cell differs from one expected string. Two marks, both cells the
tool writes: a date at E5, which parses and holds at least two numbers, and a plot reference at
the template's own reference cell, C5, which is one unbroken run holding a letter and a digit
and no angle bracket. **Nothing is withheld on a cell the tool has never written**, so an empty
or absent cell is never a filled file. The cell that decided and what it held are in the reason
and on their own line in the report, so a template wrongly withheld is traced in one line.
`KpiTemplates.DatePlaceholder` is deleted, because it decided nothing and was a second record
of a measurement. The peek reads both cells rather than one.

The two sets it was measured on: KPI CHECKLIST R1 MOSQUES holds `<Date>`, `<Name>`, `<Position>`
and `<UID>` at E5, G5, H5 and C5, and an earlier production set holds nothing at E5, G5 or C5
with real values at D3 and H5. One thing in the round message does not hold against the code and
is said in the log: the old rule offered a template whose E5 was empty, because it refused to
decide on a blank cell. It was wrong for the reason given rather than for that consequence.

A review of the round found two limits, both now stated in the rules and the log and neither
guarded against: a filled workbook the tool wrote neither cell into reads as a template, and a
client set hinting a reference's shape rather than bracketing it would be withheld. It changed
three things: the sheet part is parsed once per file rather than once per mark, the pane's line
saying the three typed cells are never written now says they are typed on the pane and copied
through, and a theory pins what a shape-alike hint does today.

Five break watches went red on 5, 8, 1, 1 and 1 tests and were restored byte for byte.
**Locally 1182 tests at this branch, 0 failed and 0 skipped, 667 of them KPI, 29 added, against
the 1153 main carries.** Pull request 73, merged into main as `ba805b1` through the API with the
squash message on the call: **the runner executed 1182 tests against its merged head, 0 failed
and 0 skipped, and locally the same 1182 ran at `ba805b1`, 0 failed and 0 skipped**, 667 of them
KPI. The merge commit carries no co-author line and no generated-by footer, and its tree is the
branch's tree. **Nothing in this round has been observed in Revit.** Two things wait on
Bader, both in the log: whether a filled workbook the tool wrote neither cell into matters, and
whether any client set hints a reference's shape rather than bracketing it.

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
The read cost is in the log in calls and is unchanged. A review pass over the diff, four
lenses and a refutation, confirmed four findings of eleven and three changed code, named in
the log: a refused reading no longer reused, a filled park file that names neither park when
its name cannot tell, and the templates listing cleared on any press that reached the patcher
into its folder rather than on every write anywhere.

Eight break watches went red on 4, 2, 2, 2, 6, 2, 2 and 2 tests and were restored byte for
byte. **Locally 1153 tests at this branch, 0 failed and 0 skipped, 638 of them KPI, 50 added,
against the 1103 main carries.** Pull request 72, merged into main as `adf164f` through the API
with the squash message on the call: **the runner executed 1153 tests against its merged head,
0 failed and 0 skipped, and locally the same 1153 ran at `adf164f`, 0 failed and 0 skipped**,
638 of them KPI. The merge commit carries no co-author line and no generated-by footer.
**Nothing in this round has been observed in Revit.** Three things
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
