## A DIVISION BY A COUNT OVER A RANGE IS A DIVISION

**The 16:37 glance said the formula check found no #DIV/0! anywhere in the press.** The FP-24 and
SC-06 workbooks, recalculated, show one in Tree List - Existing S70 and one in T70, and 30 plots
of that press hold every existing tree on rows 84 to 101.

```
S70   IF(TotTrees<1," ",S69/COUNT(B4:B83))
T70   IF(TotTrees<1," ",T69/COUNT(B4:B83))
```

on all seven templates and on both tree list tabs. The check matched a division by ONE CELL only,
so a division by a count over a RANGE was never looked at at all.

**THE RANGE IS COUNTED OFF THE CELL STATES AFTER THE WRITE**, COUNT, COUNTA and SUM alike, and
nought is a #DIV/0!. Nothing evaluates a formula: a cell of the range holding one has no value in
this output, because the patcher drops every cached result on purpose.

**AN IF AROUND THE DIVISION IS HONOURED.** `IF(TotTrees<1," ",...)` with `TotTrees` at nought
never reaches the division, so a plot with no trees at all gets no line, which is right and is
what would otherwise fire on every such plot. **`TotTrees` IS READ OFF THE FILE'S OWN NAMES**,
and it is defined TWICE, at workbook level and again on Tree List - Proposed.

**THE SAME NAME DEFINED TWICE MEANS TWO THINGS, AND THE SHEET THE FORMULA SITS ON SAYS WHICH.**
`DefinedNames` kept the first definition BY NAME ALONE, so a `TotTrees` scoped to Tree List -
Proposed could answer for a Tree List - Existing formula and hold a guard that is not held. It is
keyed on the scope now, with `localSheetId` resolved off the workbook's own `<sheets>` order, and
`TargetOf` prefers the sheet local definition then the workbook level one. That was a live fault
found by building the shape rather than by a run.

**A DIVISION THE CHECK CANNOT EVALUATE IS COUNTED AS NOT EVALUATED**, and the glance says how
many, instead of saying none was found. `FormulaCheck.DivisionsNotEvaluated` carries them and a
press that worked every one out says nothing about it, because a count of 0 in a sentence about
what could not be done is a line the team reads past on every press.

**IT STAYS A REPORT LINE AND NEVER STOPS A WRITE**, which is unchanged and is what the comment at
the check already says. What is new is that it feeds READY, so a workbook that recalculates with
one does not read as ready to send.

## A ROW THAT COMPUTES A CANOPY PER TREE AND ADDS NONE

**FP-18 has 2 Ziziphus spina-christi on Tree List - Existing row 83.** `L83` carries the canopy
formula and **`M83` is empty** in the EXISTING PARKS and FUTURE PARKS templates. The PDF said
2,631 m² greened and 67.68% canopy, the recalculated Excel 2,531 and 63.97%, and nothing warned.

Measured on all seven templates, which answers the open question the ninetieth pass logged:

```
column M is Total Mature Canopy Area,  =IF(ISBLANK(B85)," ",L85*B85) on a complete row
Tree List - Existing   M102 = SUM(M4:M101)
Tree List - Proposed   M93  = SUM(M4:M92)
the first tab's Canopy Area cell   = 'Tree List - Existing'!M102+'Tree List - Proposed'!M93
   F8 on HEALTHCARE, MOSQUES, PARKING and SCHOOLS
   F9 on EXISTING PARKS, FUTURE PARKS and STREETS
```

**A ROW HOLDING A COUNT MUST ALSO CARRY THE TOTAL CANOPY FORMULA**, in the column the canopy
total adds. Where it does not, Total areas to be greened and the canopy percentage are left blank
and the line names the cell.

**NOTHING WRITES M, OR F8, OR F9.** `TotalCanopyColumns.In` follows the chain off the file: the
green cover cell names the canopy cell, the canopy cell names the two totals, each total's own
SUM range names the column and the rows. The canopy cell is the one of the green cover formula's
three cells the template's map does not name as planting or as lawn, which is the rule
`WorkbookArithmetic.GreenCoverCell` already uses, so the two cannot name two different canopies.

**AN EMPTY ROW IS USABLE FOR A NEW SPECIES ONLY WHEN IT CARRIES BOTH FORMULAS.**
`SpeciesList.UsableEmptyRows` asks the canopy column and the total canopy column, and a column
that was NOT READ leaves every row usable rather than refusing them all, because a check that did
not happen is an absence and not an answer.

**The sheet's formulas are read ONCE per list.** Reading them per row is a sheet parse per row,
and a tree list runs to 101 rows.

## THE TOTAL CANOPY CHECK MUST NOT SWITCH ITSELF OFF

**A GUARD THAT SWITCHES ITSELF OFF READS EXACTLY LIKE A GUARD THAT PASSED.** Where
`TotalCanopyColumns.For` could not follow the chain, `WorkbookArithmetic` returned an empty
reason and `SpeciesList` offered every empty row, so the green cover and the canopy percentage
were written with no total canopy check at all and the chain's own reason was printed nowhere.
**Today's seven templates read fine and the team is editing them**, which is exactly when a
check nobody can see switching off costs something.

The rule, in four parts, Bader's decision of 16 September.

- **Where the template HAS a Total Green cover cell and the column cannot be read**, a plot
  holding a count on that sheet gets Total areas to be greened and the canopy percentage left
  BLANK, and the reason names the template, the sheet and the column's own `Why`. **READY reads
  NO**, through the sourced blank box rule it already follows
- **No empty row of that sheet is usable**, because nothing says whether one carries the formula
  the canopy total adds, and the species is named with why. `SpeciesList.TotalCanopyUnreadable`
  is the one flag and `UsableEmptyRows` reads it
- **A template with NO Total Green cover cell still refuses nothing.** It holds no canopy total
  for a row's canopy to reach, so there is nothing here to check, and that is the one case
  unchanged. `TotalCanopyColumns.NoGreenCoverCell` is its own constant and
  `TotalCanopyColumn.TheTemplateNamesNoGreenCover` its own flag, **because a signal that travels
  in the data is not a signal**: reading the reason's words to tell the two apart is the shape
  this repo has already paid for
- **The glance names every such template**, once for the press rather than once per plot, through
  `UnreadableCanopyColumns`. A press where every ticked template read says so, because a line
  that disappears when there is nothing to report reads the same as one nobody wrote

## THE TEMPLATES' OWN TREE LISTS ARE READ ONCE AT THE PRESS, CELL BY CELL

**NONE OF THIS SHOWED UNTIL A PLOT HIT A BAD ROW.** Measured in the 16 September workbooks, after
the team's template edits of 15 September:

```
Tree List - Existing L85, L88 and L90 to L101 still typed   EXISTING PARKS, STREETS
Tree List - Existing L101 typed                             MOSQUES
M83 empty                                                   EXISTING PARKS, FUTURE PARKS
O90 to O94 and O96 to O100 empty                            all seven
N85 and N88 to N101 empty                                   all seven
Tree List - Proposed L84 to L92 deleted                     EXISTING PARKS, FUTURE PARKS
the Native and Adaptive SUMIFs stop at row 91               Tree List - Proposed, list ends 92
the same on Tree List - Existing stop at row 95             FUTURE PARKS, list ends 101
V4 to V57, S35 to S43, S69 to T70 and the average canopy cell read rows 4 to 83 only
```

A press over 154 plots lands on a different set of those rows every time the model changes, so
the team learnt about each one from a workbook that had already gone out. `TreeListCheck.In`
reads **both tree lists of each ticked template ONCE, at the press, before any plot is written**,
and names every cell. Seven questions per sheet:

```
rows the total reaches whose canopy cell is typed or missing
rows whose total canopy cell is missing
rows whose total water cell is missing
named rows whose water per tree is empty
empty rows carrying neither the canopy formula nor the total canopy one
names held on more than one row
formulas on the tree lists or the first tab reading a range that stops before the list's last row
```

**EVERY COLUMN IS READ OFF THE FILE AND NEVER FROM A LETTER**, which is the rule row 5, row 7 and
the two DIAMETER headings already cost. The canopy column is the column a canopy formula really
sits in on that sheet, the total canopy column comes off the canopy total's own chain, and **the
water pair comes off the sheet's own formulas**, a column whose rows are another column
multiplied by the count and which is not the canopy pair. **NOTHING HERE HOLDS N OR O.**

**A COLUMN THAT CANNOT BE READ IS NAMED AS NOT READ, NEVER SKIPPED IN SILENCE.** A check nobody
made and a check that passed read the same in a count, so `TreeListSheetCheck.NotRead` carries
each one and a sheet holding one is not clean.

**IT IS A REPORT SECTION AND IT STOPS NO WRITE.** What the canopy guard and the total canopy
check already stop is unchanged. The glance gets ONE LINE PER TEMPLATE THE PRESS READ, clean or
how many cells are named, and the cells themselves are in `THE TEMPLATES' OWN TREE LISTS, CELL BY
CELL` below it, grouped by which of the seven questions named them. **A template whose pick was
refused before its own file was opened has no line here**, because nothing read its tree lists,
and its refusal is already on the record in its own row.

## Every printed cell text resolves a shared string to its text

**The canopy check line read `D99 holds 419 and no formula, E99 holds 122 and no formula`**, and
D99 holds Prosopis Juliflora. A cell holding a shared string stores an INDEX and not the text,
the formula reader kept the raw `<v>`, and the report printed a row of index numbers at the team.

`WorkbookPackage.TextOf` with the package's own shared strings is the one reader, which
`LabelledCells` and `SpeciesList` already ask, and `WorkbookFormulas.Check` reads the table once
per file and passes it down. **A cell holding shared string 0 would have printed as a nought**,
which is the same fault wearing a number a reader would believe.

## The street reference file fills the two cells STREETS types by hand

`StreetReferenceFile` reads the team's Scope_Validation workbook, browsed for and remembered
through its own pointer file beside the installed assembly, the same way the templates folder
and the output root are. It fills D8, Streets ROW (m), and F8, Streets Total Length (m). **H8
used to be the two multiplied and the workbook computed it. The client emptied it**, under the
section below, so three cells on one row now come from three sources and the sheet computes none
of them.

**Measured on Scope_Validation_21072026, 2026-09-13**, which is not in this repository and never
will be. One sheet, a header row and 8,353 rows. The four wanted columns are NOT at the front and
there is no header cell over column B at all, so they are found by the names in the header row:

```
D   ID_UID *          the plot, ANH-007-ST-100210
H   ES_QUANTITY       330.65849900000001
I   QUANTITY UNIT     m on 6,301 rows, sqm on 2,051, and Null on one
O   ROAD_WIDTH        20
```

**313 rows are ANH-007-ST**, which is NG05's neighbourhood, and that number is what the round
message predicted and the file measured.

Six rules, all tested.

**The UID is matched whole and without case**, never as a prefix and never against a name.

**A street plot the file does not name gets both cells left empty and is NAMED in the report
with its UID.** It is a note and never a refusal. **NOTHING IS ESTIMATED FROM THE COMPONENT
VALUE.** STREET 30m ROW looks like it says 30, and the 313 street rows read 15 on 156 of them,
20 on 65, 10 on 57, 30 on 15 and 36 on 13, with 5, 6, 8 and 12 among the rest. A width read off
the component name would be a number nobody measured, in a client file, on more than half the
plots.

**QUANTITY UNIT is read and checked, for the length alone.** A row in anything but m gives no
length and is named, because 2,051 of the file's rows are areas in sqm and one written into the
total length cell would have the workbook compute an area of an area. **The unit is the
quantity's and never the width's**, so such a row still gives its ROAD_WIDTH where that is a
number, and is refused whole only where it is not. Measured 29 October: of 6,244 STREET rows two
are in sqm, both Pedestrian Walk, one holding a ROAD_WIDTH and one not. Refusing on the unit lost
NG05's walkway its width. A row in m is unchanged, both cells or neither. ROAD_WIDTH has no unit of
its own and reads the metres its row's CL_SUBCATEGORY names, 20 on 20m rows, 30 on 30m, 60 on 60m,
so it is metres on a sqm row too. What a walkway's total length should be is Q22.

**Two rows for one UID refuses and names both.** The file holds 33 such UIDs, none of them in
ANH-007, so it fires on no NG05 plot today and is still the rule.

**The file writes an absent value as the text Null in angle brackets rather than leaving the cell
empty**, on 2,050 of the 2,051 sqm rows' road width and on one row's unit, measured again 29
October. It is text, so `CellNumber` reads no number out of it and the plot is named, which is
why the numbers go through that tested reader rather than through a parse written here.

**No file set, or one that cannot be read, is a NOTE and not a refusal.** Both cells stay empty,
the run goes through, and the report says so once at the top rather than 78 times underneath.

**Only STREETS asks it this round.** The file also holds parking, mosque, park, school, health
and government rows and nothing reads them.

## STREETS takes an area again, because the client reissued the template

**ONE CELL CHANGED IN THE WHOLE WORKBOOK**, measured by diffing the reissued STREETS template
against the one that ran on 14 September: 4,160 cells against 4,159, no named range moved, no
other sheet touched.

```
H8, Streets Total Area (m2)     was  =Width*F8     now  empty
```

Their reference copy says why, and it is the same source every other template's area comes from:

```
H8 = REVIT 00 LINK / ID FILLED REGION "RCRC_OUT OF SCOPE (PRESENTATION)" / PRX_Intervention Area
D8 = EXCEL FILE "Scope Validation 21072026" / COLUMN O1 "ROAD_WIDTH"
F8 = EXCEL FILE "Scope Validation 21072026" / COLUMN H1 "ES_QUANTITY"
```

**So three cells on one row come from three sources and the workbook computes none of them.**

**THIS REVERSES FINDING 31, AND IT REVERSES IT BECAUSE THE TEMPLATE CHANGED.** The finding was
right, its fix was right, and the audit entry says so rather than saying the fix was wrong.
STREETS is on the same path as every other template now: its plot's filled regions are read from
the 00 link, `RegionChoice` chooses, the cell is written, and the reconciliation refuses or asks
exactly as it does on the other six.

**IT GOT THERE BY ONE ENTRY IN ITS OWN MAP.** `new MappedCell(KpiValue.Area, "H8")`, and nothing
else changed, because `KpiTemplate.TakesNoArea` reads the MAP and every path that skips an area
asks that one thing. **H8 is hard coded nowhere.** The letters on these sheets have moved before
and row 7 proved it, so the cell is taken off the template the same way every other value is.

## SIX CELLS ON THE MAIN SHEET, EVERY ONE FOUND BY A LABEL AND NEVER BY A LETTER

**Bader's answer: Character is always Urban Area Zone and Context is always Urban.** They come
from no model, no schedule and no file, so `FixedCells` holds them as data.

**Where each goes was measured on the three workbooks written on 13 September:**

```
MOSQUES   Character label C7, value D7.   Context label E7, value F7.
SCHOOLS   Character label C7, value D7.   Context label E7, value F7.
STREETS   Category  label C7, value D7.   Character label E7, value F7.
          Context   label G7, value H7.
```

**THE STREET SHEET IS WHY NOTHING HOLDS A LETTER.** It carries a Category at C7 whose value cell
D7 holds a FORMULA, so its Character and Context sit one pair to the right. A map that put
Character at D7 because two templates out of three do would overwrite that formula on the third.

So `FixedCells.In` reads the template's own main sheet, finds the cell whose text is the label,
and writes the cell to its RIGHT. **Nothing looks for the word Category**, so D7 on STREETS is
never reached by anything. The labels are matched whole, without case and with edge whitespace
off, so Characteristics is not Character.

Three refusals, all tested and all silent about no cell. A template naming neither label writes
nothing and says which sheet it looked on. A template naming a label TWICE writes nothing and
names both cells, the same rule a plot on two rows of the reference file follows. And a template
nothing opened says the read did not happen rather than reading as a sheet with no label.

**The mosque file came filled**, holding Urban Area Zone at D7 and Urban at F7 already, and the
read carries what the cell already held so a line can say so rather than reading as though this
run put it there. The street file has both blank.

### Row 5 came here too, and the letters were right on all seven by luck

**The date, the person, their position and the plot reference went in by letter**, E5, G5, H5
and C5, one array for all seven templates measured on ONE of them. Finding 50 named it as the
row 7 fault three cells up. Bader then measured row 5 on all seven, on 14 September:

```
HEALTHCARE, MOSQUES, PARKING, SCHOOLS, STREETS
  B5 REF :   C5 the UID   D5 Date:   E5 the date
  F5 Prepared By:         G5 a name  H5 a position

EXISTING PARKS and FUTURE PARKS
  B5 REF :   C5 EMPTY     D5 Date:   E5 EMPTY
  F5 Prepared By:         G5 EMPTY   H5 holds the text " Architect Engineer"
```

**NO FORMULA SITS AT C5, E5, G5 OR H5 ON ANY OF THE SEVEN**, so the letter map never overwrote
anything and no workbook is damaged. The letters were right on all seven BY LUCK, the way D7
would have been right on two templates out of three, and the next template set is what a letter
cannot survive. What really differs between the two sets is what those cells HOLD.

`LabelledPlaces` is the one table now, six entries, each a name, a label, how many columns right
of the label its cell sits, and which place's row it may look on. `LabelledPlaces.In` opens the
template's own main sheet once when Create is pressed and finds all six in two passes, the
places that name their own label first and the anchored one after. A test rebuilds each
template's own row 5 AND its reviewer block and checks the lookup lands on C5, E5, G5 and H5 on
all seven, and a second one checks the four values really reach those cells through the plan and
that nothing at all lands in the reviewer's block.

**NO LABEL NAMES THE POSITION CELL, AND THAT IS MEASURED RATHER THAN ASSUMED.** Three labels sit
on row 5 and they reach three of the four cells. H5 is one further along than the person's name,
under the same Prepared By. Bader then looked over all seven for anything naming it: **the only
cells whose text names a position hold `<Position>` itself, which is the placeholder this run
replaces and not a label.** Nothing names it on row 5 or anywhere else. So the DISTANCE of two
stays, and it is said out loud in the code, in the report and here, because a distance dressed
up as a label would be the one thing in this lookup nobody could see. The UNKNOWN the round
before left open is closed by that measurement.

Two places under one label also refuse together when the sheet carries that label twice, because
the thing that cannot be resolved is the label they share.

### The date is on every template TWICE, and the guard was firing on all seven

**Measured on all seven on 14 September, and it is a fault the round before shipped.** Every
template carries a SECOND block of the same shape as row 5, one word apart:

```
row  5   D5  Date:   E5 the date   F5  Prepared By:   G5 a name   H5 a position
row 28   D28 Date:   E28 the date  F28 Reviewed By:   G28 a name  H28 a position
```

at row 28 on HEALTHCARE, MOSQUES, PARKING and SCHOOLS, and at ROW 29 on EXISTING PARKS, FUTURE
PARKS and STREETS. **A letter map would have been wrong there too**, which is the same lesson
row 7 already taught.

**The found twice guard was firing, on all seven, and it was not scoped to anything.** Measured
by building a sheet with both blocks and running the real lookup: `Date:` came back not found
with `Date: is on <Mosques> at D5 and D28, and nothing says which is meant`, so **the date would
have been written into NO workbook at all** and the report would have named both cells under
CELLS NOT WRITTEN. The guard did exactly what it says. The TABLE was wrong, because it said the
label alone identifies the cell and on a real sheet it does not.

**So the date is found THROUGH the preparer's block.** `Prepared By:` against `Reviewed By:` is
the only thing that separates the two, so `LabelledPlace.OnTheRowOf` names the place whose
label's ROW this one may look on: `Prepared By:` is looked for over the whole sheet and `Date:`
is then looked for on that label's own row and nowhere else. The row is chosen by the LABEL and
never by being first or by a number, so a sheet whose reviewer block sits above the preparer's
is answered with the preparer's row, and a test says so in those words.

Four things follow, all tested.

**A row taken as a constant is the fault again.** Hard coding row 5 passes every template
measured so far and fails the reviewer first case, which is the one test carrying that rule.

**An anchor that cannot be found gives no row to look on**, so a sheet naming no `Prepared By:`,
or naming it twice, writes no date either and the reason says exactly that rather than repeating
the anchor's words: `there is no row to look for Date: on, because Prepared By: was not found`,
then the anchor's own reason.

**The guard still fires inside the row it may look on**, and both its reasons name that row and
why it was that row: `no cell on <Mosques> row 5, the row Prepared By: sits on, reads Date:` and
`Date: is on <Mosques> row 5, the row Prepared By: sits on, at A5 and D5`. A row a person cannot
check against the sheet would be a second unreadable rule.

**One level of anchoring, on purpose.** An anchor must name a place that exists and is not
itself anchored, with a test, because a chain of rows is not something anybody can check by eye.

**`REF :` is measured as appearing ONCE and is left looking over the whole sheet.** The second
block starts at D, so it carries no reference. If a template ever holds it twice the guard says
so and writes nothing, which is the right answer and not a silent one.

**THE LABELS ARE SPELT EXACTLY AS THE CELLS READ**, the space before the colon in `REF :`
included, and the match is whole, without case and with edge whitespace off. A template whose
label reads anything else writes nothing for it and says so. Nothing reaches for a near miss,
because a near miss is how a value lands in a cell nobody measured.

**A CELL THIS RUN WRITES THAT WAS NOT EMPTY IS NAMED IN THE REPORT WITH WHAT IT HELD.** Both
park templates hold a position at H5 already and the mosque template came holding Urban Area
Zone at D7 and Urban at F7. The run writes over all of them, which is right, and **overwriting
somebody's text has to be visible rather than silent.** `LabelledCell.Holds` carries what was
there, `KpiCreatePlan.Labelled` carries all six, and CELLS WRITTEN OVER SOMETHING THE TEMPLATE
ALREADY HELD prints the value, the label, the label cell, the cell written and what it held.
Only cells this run really wrote are in it, so a template naming no label is under CELLS NOT
WRITTEN with its own reason instead, and the column header is printed only when there is a row
under it. **That property was set and read nowhere for a round**, which is why the line did not
exist until somebody measured a template that came filled.

**ONE PLACE STILL READS ROW 5 BY LETTER AND IT IS ON PURPOSE.** `FilledMarks` decides whether a
workbook in the templates folder is a filled checklist rather than a template, and it runs over
a file whose template is not known yet, so it reads `E5` and `C5` as row 5 was measured to hold
them on all seven. That is two records of one fact, which is the shape this repo keeps paying
for, so a test holds them against each other over the measured row 5: the cells the labels
choose must be the cells those marks read. Moving the peek onto the labels is a round of its
own, named in the log.

**THE PANE NAMES NO CELL FOR ANY OF THE FOUR ANY MORE.** It cannot: which cell each lands in is
not known until the template is opened at the press. The line under the map says the reference
comes from the chosen parameter and goes where the `REF :` label sends it, and the line under
that says the same of the three the team types. A line about what the tool does is checked
against what it does, which is a rule this file already carries and this is the third time it
has bitten.

**Which plots belong to one checklist is not written down anywhere.** Nothing groups by the
plot prefix, by the component, or by anything else. The user ticks them and the tool writes one
workbook for each. `PlotTicks` is the one record of that choice and the list is drawn from it
every time it changes.

The plot list is the union of two sources, `PRX_Plot_ID` on the sheets and the
`PRX_Ref Plot ID` filter value on the schedules. Both lists are kept, the disagreement is shown
on screen, and neither wins. That is a seventh place two records of one fact could part.

