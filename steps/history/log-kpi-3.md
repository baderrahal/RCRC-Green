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

