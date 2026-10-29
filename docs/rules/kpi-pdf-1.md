## A PDF beside every workbook

Every plot already gets a workbook in a folder named after its UID2. It gets a PDF beside it
now, filled from the same run, named after the same UID2.

```
<root>/<COMPONENT FOLDER>/<UID2>/<UID2>.xlsx
<root>/<COMPONENT FOLDER>/<UID2>/<UID2>.pdf
```

**THE EXCEL IS WRITTEN FIRST AND THE PDF SECOND**, because two of the PDF's fields read cells
out of the workbook this run has just written. That ordering is a rule and `OnePlot` in
`KpiRequestHandler` is the one line that keeps it. Overwritten silently if one is there, the
same as the workbook.

### The three forms, and the template is what picks one

```
Projects Basic Data - Parks                                42 fields   EP, FP
Projects Basic Data - Open spaces associated to buildings  49 fields   HF, FM, DM, PL, SC
Projects Basic Data - Roads                                34 fields   NS, ST, MM
```

**THE FORM FOLLOWS THE PLOT'S TEMPLATE**, Bader's rule of 29 October. STREETS takes Roads.
MOSQUES, SCHOOLS, their four 60 per cent versions, HEALTHCARE and PARKING take Open spaces
associated to buildings. EXISTING PARKS and FUTURE PARKS take Parks. NG05's walkway was placed in
STREETS by component-rules.csv and wrote no PDF, because its prefix is none of the ten above.

`PdfForms.ByTemplate` is the table, keyed on the template's kind, so a 60 per cent template and a
template offered on its layout take their kind's form. A table in Core and not a Form column in
component-rules.csv, because components.csv rows and new templates place plots too and a rules
column would reach neither. A test holds a form to every template `KpiTemplates.All` names.

**The prefixes above answer only for a plot no template placed.** A test walks every prefix
measured so far and finds the same form through its template as through its prefix. A plot with
neither writes no PDF and is named.

**Only the Open spaces form has a Project Type box.** It prints PRX_Component as the model holds
it. Measured off the client's Roads file on 29 October, 34 fields and none a type, so a walkway's
Roads PDF carries WALKWAY nowhere, Q21.

### Every field was read off the files and checked back against them

`PdfForms` is the table, measured on 14 September off the three PDFs themselves. **Then the
whole table was run back against the client's own files**: every field name found, every note
matching, on all three, with nothing missing and nothing differing.

**THE SOURCE OF EACH VALUE IS IN THE FIELD'S VALUE, NOT ITS DEFAULT VALUE.** The round message
said default value. Measured: only the four stage tick boxes carry a default at all, and on the
open spaces and roads forms that default is a tick on all four while the real state sits in the
value. So the client writes the source into the value, and the tool holds it as the note.

**A FIELD WITH NO NOTE IS NOT FILLED.** Bader, 14 September. The forms carry many, sidewalks,
medians, water tanks, toilets, kiosks, seating, play areas, bridges and a catwalk, and none is
in the table.

**THE FOUR STAGE TICK BOXES ARE LEFT EXACTLY AS THEY ARE.** Schematic Design holds a tick and
the other three a space, on all three forms, and the tool names no field for any of them.

### Three things the round message said that the files do not

All three were measured off the files, and the file is the record.

- **The open spaces TOTAL Shrubs VALUE is not wrong and its TOOLTIP on PARKS is.** The round
  before this one checked one key of the dictionary and said the note was fine. Measured:
  `Proposed Shrubs.1.1` on open spaces holds `sum of the above or from revit` in its `/V` and
  `Proposed Shrubs` in its `/TU`, both right. **On PARKS the row the page prints TOTAL Shrubs
  Area (m²) carries `/TU(Existing Shrubs)`**, and so does the row above it printed Proposed
  Shrubs Area (m²). So there IS a wrong label in these files, on the form the round named a
  different way round, and the correction was too narrow. Roads' tooltips all match their own
  rows. The conclusion never moved: all three mean the sum, Bader confirmed it with the client
  on 14 September, and the tool has always written existing plus proposed. **The note on this
  field is not to be trusted on any form.** The whole section below on the position exists
  because of it
- **Parks and Roads name NO Project Type field at all.** The round said it is typed text on
  those two and left alone. It is not there to leave alone, and only the open spaces form asks
  for a component
- **THE ROADS FORM'S Total areas to be greened NAMES THE CANOPY CELL.** Parks and open spaces
  both name Total Green cover divided by 1,000,000, and roads names
  `excel the cell on the right of "Total area covered by canopy "` for a field all three call
  the same thing. **It is held here exactly as the file has it** so the check does not refuse
  the form over the client's own copy and paste. **Which the client means is no longer an open
  question**: Bader decided on 15 September that all three take Total Green cover, under its own
  section below, and the note is a record of what the file says rather than what decides a value

### The position is the THIRD record, and it is the one a person reads

The field name was checked and the note was checked and **where the field sits on the page was
not**, which is how a wrong tooltip went unnoticed for a round. A field that MOVES to another row
keeps its name and its note while meaning something else, and on the open spaces form every shrub
row carries TWO boxes, a quantity at x 500.5 and an area at x 548.1, so a field slipping one
column would take a number into the wrong box with both its other records intact.

Every field of all three forms carries its measured x and y now, read off each `/Rect` on 14
September, and `PdfFormCheck` compares them. **A field that has moved writes nothing and is named
with where it sits beside where this tool measured it.** The room is `PdfFormCheck.Tolerance`,
half a point, against rows about twenty points apart, because a viewer rounds a rectangle and
half a point is nowhere near a row.

The open spaces shrub column, measured down the right of the page, is what settled the TOTAL
Shrubs question three ways rather than one:

```
y=449.2  Existing Trees     y=387.9  Existing Shrubs
y=428.9  Proposed Trees     y=367.4  Proposed Shrubs
y=408.9  TOTAL trees        y=346.9  TOTAL Shrubs      /V sum of the above or from revit
                            y=326.6  Ground Cover
                            y=306.2  Lawn
```

**Which fields were matched by NOTE rather than by position, and what each one checked out as**,
gone through one by one on 14 September. All fourteen on each form now carry all three records,
and this is the list of what the position said about each:

```
Uid, ReportDate, ProjectType    text and a date, one box each, position agrees
Area                            one box, position agrees
Row, Length                     roads only, two rows one above the other, position agrees
TotalAreasToBeGreened           one box, position agrees, and its NOTE differs by form, below
PercentageCanopy                parks only, one box, position agrees
IrrigationWaterDemand           one box, position agrees, nothing is written into it
ExistingTrees, ProposedTrees    the two rows above TOTAL trees, position agrees
TotalTrees                      the row under them, position agrees
ExistingShrubs, ProposedShrubs  the two rows above TOTAL Shrubs, position agrees
TotalShrubs                     the row under them, position agrees, AND ITS TOOLTIP LIES ON
                                PARKS, which is the one the note got wrong
GroundCover, Lawn               the two rows under it, position agrees
```

So one of fourteen disagreed, and it is the one the round message named. Every other field was
right and is said to be right here, because a field checked and found right reads exactly like
one nobody looked at.

### Two numbers the tool COMPUTES, with their working shown

**Total Green cover and the canopy percentage are cells the WORKBOOK computes**, and the patcher
drops every cached formula result on purpose so Excel recalculates, so neither number is in the
file the run just wrote. Three ways out were on the table and two are refused: opening a hundred
and fifty workbooks by hand is not a workflow, and relaxing the cache rule brings back the stale
zeros that took four rounds to kill. **So the tool computes both, from what it itself wrote and
read, and shows its working.**

```
Total Green cover   = canopy + planting + lawn
Percentage canopy   = canopy / area
```

Planting, lawn and area are the three totals this run wrote into the workbook's own cells, handed
to the PDF on `PdfWorkbookNumbers` rather than worked out a second way. The canopy is built by
`CanopyArea.From`, over **every row this run wrote a count into and no other**, because the canopy
the workbook computes is over the rows its own counts sit in.

**The arithmetic is the workbook's own column, and the rounding is INSIDE.** Measured by Bader on
the MOSQUES template, Tree List - Proposed row 21:

```
L21  =IF(ISBLANK(J21)," ",ROUND(PI()*(J21/2)^2,0))
M21  =IF(ISBLANK(B21)," ",L21*B21)
```

So eight metres across is fifty square metres per tree and three of them are 150. Rounding the sum
instead gives 151, which is a different number from the workbook on every row.

**A row with no canopy diameter is NAMED and is not counted as nought**, because its own L cell
returns a space in the workbook too, so it adds nothing there either.

**Adding printed numbers with the working shown was already the rule. This is that rule one step
further** and it is said out loud because it is the first time this tool produces a number no
schedule printed. Every computed field prints under COMPUTED, not read, with its parts.

### The guard on the one formula the computing copies

**Read the formula off the file and refuse where it differs.** `WorkbookArithmetic.Canopy` takes
the output's own `FormulaCheck`, and for every row this run wrote a count into it looks on that
row for a cell carrying the text the tool knows, built with the diameter column that sheet's own
heading row chose. **A row whose canopy formula differs, or that carries none, blanks BOTH
computed fields and names the row and every formula on it.** Both numbers rest on the canopy, so a
canopy short of one row is a number that reads as complete and is wrong.

Nothing writes the diameter column in. J is what row 21 uses and every sheet's own comes off its
heading, the rule `SpeciesList` already follows, so the caller hands the column in.

**A CHECK THAT COULD NOT BE MADE AND NOTHING TO CHECK ARE TWO DIFFERENT THINGS**, told apart by
`ArithmeticCheck.NothingToCheck` and never by reading the reason, because a signal that travels in
the data is not a signal. A run that wrote no tree row has no canopy and no canopy formula, so its
green cover is the planting and the lawn and it is written. A workbook whose formulas were never
read computes nothing at all.

### The other two formulas, measured on all seven and guarded now

**The round before could not build this and refused to build it on one example. Bader measured
both on all seven templates on 14 September, and the open question is closed.**

```
TOTAL GREEN COVER, the same shape on all seven in two row layouts
  EXISTING PARKS, FUTURE PARKS, STREETS    D9 = F9+F11+H11
  HEALTHCARE, MOSQUES, PARKING, SCHOOLS    D8 = F8+F10+H10

PERCENTAGE CANOPY, in SECTION 3 and not section 1
  HEALTHCARE, MOSQUES, PARKING, SCHOOLS    label C31, value E31 = IF(Area<1," ",F8/Area)
  STREETS                                  label C32, value E32 = IF(Area<1," ",F9/Area)
  EXISTING PARKS and FUTURE PARKS          NO SUCH LABEL AT ALL
```

Canopy plus planting plus lawn, and canopy over area, which is what this tool already computes.
**The two row layouts are exactly why neither cell is a letter**, the lesson row 7 and row 5 both
taught: a map putting the green cover at D8 because four templates out of seven do would read the
wrong row on the other three. `ComputedPlaces` is the table, two labels, and it is read through the
same `LabelledPlaces` lookup every other labelled cell uses.

**THE CLIENT'S NOTE IS WRONG ABOUT WHERE THE PERCENTAGE IS.** It says the cell to the right of
`Total area covered by canopy`. **There is no such label in section 1 on any template.** The cell
is in section 3, labelled `% of Total area covered by canopy`, and its value sits TWO columns right
of the label rather than one, the cell one to the right being empty on all five that carry it.
**That is the second note on these forms measured to be wrong about its own subject**, after the
TOTAL Shrubs tooltip, and it is why the note is checked and never used to decide anything.

**AND THE ONE FORM THAT ASKS FOR THE PERCENTAGE IS FED BY THE TWO TEMPLATES THAT DO NOT CARRY IT.**
Percentage Total area covered by canopy is on the Parks PDF alone, which EP and FP plots reach, and
EXISTING PARKS and FUTURE PARKS have no such cell. So the percentage check answers nothing to check
on every run the tool makes today. **That is a fact about the client's files rather than a fault to
fix: the tool still computes and writes the number**, because it holds the canopy and the area, and
the report says the workbook has no cell to hold it against. The check is built because it is right
and because it fires the day a park template grows the cell or another form grows the field.

### How the two cells are checked, with no letter anywhere

**`WorkbookArithmetic.GreenCoverCell` reads the cell the label chose and holds it against the three
cells this tool adds.** It must read exactly three single cells, and the template's own map must
name two of them, the planting cell and the lawn cell. **The third IS the canopy cell**, learnt
from the formula rather than written in, and carried to the percentage check so the two can never
name two different canopies. Any other shape blanks the field and names the cell and its formula.

**`WorkbookArithmetic.PercentageCell` holds its cell against that canopy cell and the map's own
area cell.** It must read exactly those two. `IF(Area<1," ",F8/Area)` reads F8 and whatever the
defined name `Area` points at, through `FormulaCell.SingleCellsRead`, **so the defined name is
checked as well as the cell** and no formula text is matched anywhere.

**A LABEL NAMED NOWHERE IS A REFUSAL FOR THE GREEN COVER AND NOTHING TO CHECK FOR THE PERCENTAGE**,
and the two are different because the measurement is different. All seven carry the green cover
label, so a template missing it is a template this tool does not know, and it writes nothing and
says which sheet it looked on. Two of the seven carry no percentage label at all, so an absence
there is the ordinary case and **an absence is not a drift**.

**THE TWO PLACES ARE READ ONLY AND ARE NOT IN THE TABLE THE PLAN WRITES FROM.** `ComputedPlaces`
sits beside `LabelledPlaces.All` rather than inside it, because those two cells hold the client's
own formulas and a value written into one would destroy them. A test says so in those words.

**The label text for the green cover comes from the client's own PDF note**, `Total Green cover
(m²)`, and is confirmed by where it lands: one column right is D9 on three templates and D8 on
four, which is what was measured. The report prints the cell the label chose on every run, so a
template whose label reads anything else is one line rather than a silence.

### A label is what the file holds, and one rule compares every one of them

**MEASURED BY BADER ON ALL SEVEN TEMPLATES, 14 September.** The green cover label carries a
LEADING SPACE on every one of them, and the client's PDF note writes it without, which is where
the tool's copy came from:

```
EXISTING PARKS   C9   ' Total Green cover (m2)'
FUTURE PARKS     C9   ' Total Green cover (m2)'
HEALTHCARE       C8   ' Total Green cover (m2)'
MOSQUES          C8   ' Total Green cover (m2)'
PARKING          C8   ' Total Green cover (m2)'
SCHOOLS          C8   ' Total Green cover (m2)'
STREETS          C9   ' Total Green cover (m2)'

HEALTHCARE, MOSQUES, PARKING, SCHOOLS   C31  '% of Total area covered by canopy'
STREETS                                 C32  '% of Total area covered by canopy'
```

The superscript two is what the cells really hold. It is written flat here because this file is
read as plain text and the constant carries the real character.

**THE LOOKUP WAS ALREADY FINDING IT, AND NOTHING SAID SO.** Measured by taking the trim out: the
cell's text was trimmed where it was read, so the leading space came off before the comparison and
the field was landing. Without that trim the lookup came back with `no cell on <Mosques> reads
Total Green cover (m2)` and five cases reddened. **A rule living in a bare `Trim()` that nothing
names, covering ONE SIDE of a two sided comparison, is a rule nobody can check**, and a label
constant carrying a stray space would still have failed with no sign of why.

`LabelText.Same` is that rule now, in one place, asked by every whole label lookup in this tool.
**Edge whitespace off BOTH sides, without case, and the inside untouched.** A double space between
words is a different label and must not match, because `LOD /  HARDSCAPE SCHEDULES` really carries
two, and a comparison that collapsed runs would answer for a name no file holds.

**EVERY WHOLE LABEL THIS TOOL LOOKS UP, CHECKED ONE BY ONE.** Eleven, and only one of them carries
an edge space:

```
REF :           already right, no edge space, and it was landing
Date:           already right, no edge space, and it was landing
Prepared By:    already right, no edge space, and it was landing, twice over, because the
                position cell is two columns right of the same label
Character       already right, no edge space
Context         already right, no edge space
Total Green cover (m2)          THE ONE WITH A LEADING SPACE, now spelt as the file holds it
% of Total area covered by canopy   no edge space, right as it stood
ID_UID *        the street reference header, already right, and it trimmed one side only
ES_QUANTITY     the same
QUANTITY UNIT   the same
ROAD_WIDTH      the same
```

All eleven go through the one rule now, so a label that grows a space on the next issue costs
nothing. **The four street reference columns were the second one sided comparison**, trimming the
file's heading and comparing it against the name as written, and they were found by looking rather
than by failing.

**One lookup is a different question and is left alone.** `ScheduleColumns.Holding` asks
`KpiNames.Holds`, which splits a heading into runs of letters and looks for a word at the front of
one, so edge whitespace cannot reach it at all. It answers whether a heading HOLDS a word rather
than whether it IS a label, and it is recorded here as checked and already immune.

**And the fixtures were written out by hand.** The fixture used to write the tool's own constant
into the cell, which is a fixture of a sheet the client does not have and is exactly how a test
goes green over a lookup that finds nothing. It writes ` Total Green cover (m2)` with
`xml:space="preserve"`, the way Excel stores a cell whose text has an edge space, and
`% of Total area covered by canopy` with none, so one label with a space and one without go
through the same lookup on the same sheet.

### Every unit on all three forms, one by one

**READ OFF THE PAGE BY POSITION RATHER THAN OFF A NOTE, 14 September.** A unit that matches by luck
reads the same as one nobody checked, so every field carries the unit the form's own unit column
prints, the report prints it beside every written value, and a test writes the whole table out by
hand.

**FOUR CONVERSIONS. Every other field is written in the unit it was read in.**

```
field                             forms          form asks   source gives   conversion
Row                               Roads          m           m              none
Length                            Roads          km          m              DIVIDE BY 1000
Area                              Parks, Open    m2          m2             none
Total areas to be greened         all three      km2         m2             DIVIDE BY 1,000,000
Percentage canopy                 Parks          %           a ratio        TIMES 100
Irrigation water demand           all three      m3/day      l/day          DIVIDE BY 1000
Existing, Proposed, TOTAL Trees   all three      count       a count        none
Existing, Proposed, TOTAL Shrubs  all three      m2          m2             none
Ground Cover, Lawn                all three      m2          m2             none
```

**THE FOURTH CONVERSION IS UNREACHABLE AND IS RECORDED ANYWAY.** Nothing reads a water demand off
any schedule, so the field is blank on every run, and its reason names the unit and the division
for the day the read is built. The other three are live.

**The three fields carrying no measured quantity have no unit printed beside them on any form**,
the UID, the project type and the report date, so the tool records what the value IS, text and a
date, rather than inventing one.

**THE REPORT PRINTS THE UNIT THE FORM ASKS FOR, NEVER THE UNIT THE SOURCE GAVE.** They differ on
four fields, and printing the source's would make a converted value look unconverted. Checked at
the line: the fill is handed `wanted.Unit` off the form's own field, so it was already the form's,
and a test pins it now. 330.66 metres lands as 0.33066 with `km` beside it, and the road width in
the box above reads `m` and converts nothing, so the two are told apart by their unit rather than
by which number looks bigger.

**The road length is metres into a box printed km.** The workbook's own Streets Total Length (m)
cell takes the metres unchanged, measured on ST-100130 reading 174, so the division is for the PDF
alone and the Excel is left exactly as it was. A row in any unit but m never reaches the
conversion: `StreetReferenceFile.For` already refuses it, naming the plot, the row and what the
unit said, and the PDF field is left blank carrying that reason.

**The percentage is a ratio times a hundred with no sign**, checked against the client's own filled
ANH-006-NP-100002: an area of 771, 0.000550 square kilometres greened and a percentage of 71.

**The Roads form's Total areas to be greened takes Total Green cover, the same as the other two.**
It used to take the canopy alone, off its own note. That is the section below.

### ALL THREE FORMS TAKE TOTAL GREEN COVER, AND THE OPEN QUESTION IS CLOSED

**Bader's decision, 15 September.** The Roads note names the canopy cell and the other two name
Total Green cover, for a field all three call Total areas to be greened, and **the client meant
one number on all three**. So `PdfFill.Greened` has one path now and `PdfForms.Roads` is not
asked which form it is.

**THE STREETS TEMPLATE ALREADY AGREED.** `ComputedPlaces` holds its D9 = F9+F11+H11, canopy plus
planting plus lawn, which is exactly what `GreenCover.Total` computes. So the Roads PDF and the
workbook filed beside it carried two different numbers for one quantity until this round, and now
they carry one.

Measured off Bader's screenshot of ANH-007-ST-100308:

```
Canopy Area          984
Planting              69
Lawn Area          empty
Total Green cover  1,053      the PDF reads 0.001053, and not 0.000984
```

**THE NOTE IS KEPT EXACTLY AS THE FILE HOLDS IT**, spaces and all, because `PdfFormCheck`
compares notes and a tidied one would refuse the form. What changed is that nothing reads it to
choose a cell. `PdfFill.RoadsNamesTheCanopyCell` is DELETED, and it is the second reason a thing
gets deleted rather than the first: the SHAPE it recorded is gone rather than its last caller,
because there is no longer a form filled from a different cell for it to describe.

**THE CANOPY GUARD AND THE GREEN COVER CELL CHECK ARE UNCHANGED ON ALL THREE FORMS.** A plot with
a tree on rows 85, 92 or 99 still writes nothing into this box, because those rows carry no canopy
formula in the client's own file. That is Bader's decision of 15 September under the section above
and it stands.

