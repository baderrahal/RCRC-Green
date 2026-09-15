---
paths:
  - src/RcrcGreen.Core/Kpi/**
  - src/RcrcGreen.Revit/Kpi/**
  - tests/RcrcGreen.Core.Tests/Kpi/**
  - steps/kpi-templates.md
---

# The rules the KPI tool holds

These load when something under a `Kpi/` folder is being touched. Everything about the repo
as a whole is in `CLAUDE.md`, and the Drawing Sheet's rules are in the other two files here.

## What it is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, one template per asset type: existing
parks, future parks, healthcare, mosques, parking, schools, streets. Cells that come from Revit
carry a note saying where. The finished tool reads those values out of a model and writes them
into the workbook. **It creates nothing in the model, ever.**

The scanner came first and is still there. The pane now also fills: a plot picker, the three
choices the model cannot make, and a Create button that copies the template, patches it and
writes a report. **It still creates nothing in the model and never writes to the template.**

## A CHECKLIST IS ONE PLOT, AND IT LIVES IN A FOLDER NAMED AFTER IT

**The team came back with how they actually file these, and it reverses the section that used
to sit here.** A checklist is one plot. Tick MOSQUES and press once and you get 20 folders and
20 workbooks, not one file with 20 plots added together. Nothing is added across plots anywhere
any more.

The tree, measured off the team's own folders:

```
MUGHARAZAT/                        the root the user browsed to
   FRIDAY MOSQUE/                  the component folder
      ANH-008-MO-100006/           the plot's own folder, its PRX_Plot_UID2
         ANH-008-MO-100006.xlsx    the workbook, named after its folder
```

`PlotWorkbookPath` is the whole rule. **The plot gets a folder of its own holding one file**,
which leaves room for the PDF asked for beside it later, and nothing here builds one. **The
folder is created where it does not exist and NEVER deleted**, and the writer puts its file into
whatever is already there. A workbook already at that path is overwritten silently, no
confirmation and no second copy, which is Bader's decision and is unchanged.

**A UID2 that would not sit in a path refuses rather than being cleaned.** Every other name this
tool writes goes through `ScanFileName.Cleaned`, which is right for a name a person typed. This
one is what the team searches folders by, so ANH/007 cleaned to ANH_007 is a plot nobody finds
and no error anybody sees. **The refused characters are Windows's own, written out as data**,
because `Path.GetInvalidFileNameChars` names nine and the control characters on Windows, where
Revit runs, and two on the Linux runner the gate uses: asked of the platform, the test would
pass on the runner while the tool refused the same name on a real machine.

**NOTHING ABOUT READING A PLOT CHANGED.** Same schedules, same group rules, same Street Design
rule, same canopy check, same alias, same cache fix, same read back. `OneTemplate` in
`KpiRequestHandler` still reads its whole share in one pass, because the held readings, the
progress count and the area unit are decided once per template. What changed is below the read:
`OnePlot` fills, patches and files each reading on its own.

**A refusal on one plot does not stop the rest**, the same rule a refusal on one template
already followed, and each plot's own row says what happened to it.

## The component folder is a THIRD table, and it is not the template name

`ComponentFolders` answers a different question from `ComponentTemplates`. One says which
WORKBOOK a plot is filled from and the other which FOLDER the filled workbook is filed in, and
neither derives from the other:

```
DAILY MOSQUE          -> DAILY MOSQUE        NH STRT 20m ROW       -> STREETS
FRIDAY MOSQUE         -> FRIDAY MOSQUE       NH STRT LESS 20m ROW  -> STREETS
SCHOOL                -> SCHOOL              STREET 30m ROW        -> STREETS
HEALTH                -> HEALTHCARE          STREET 36m ROW        -> STREETS
PARKING LOT           -> PARKING LOT
EXISTING PARK         -> EXISTING PARK
FUTURE PARK           -> FUTURE PARKS
```

Confirmed by Bader against the team's folders. **The two mosque values share one template and
get two folders**, and PARKING LOT and HEALTH share neither spelling with the template they fill
from, so no string rule turns one into the other any more than one turns a component value into
a template name.

**SPELL THEM EXACTLY.** SCHOOL singular, FUTURE PARKS plural, EXISTING PARK singular. That is
not a pattern, and one wrong letter makes a second folder beside the team's that nobody notices
for a month.

**Eleven values reach EIGHT folders, counted off the table.** The round message said nine. The
team's snip also holds a GOVERMENT BUILDING folder, spelt that way, deliberately not in the
table because no plot in either measured model carries a component for it, and whether that is
the ninth is for the team. A value the table does not hold writes nothing and is named, the same
way an unknown component already is, and nothing falls back to the template name.

**The folder comes off the PLOT'S OWN component and never off the template**, which is what
keeps DAILY MOSQUE and FRIDAY MOSQUE apart on a run that fills both from MOSQUES.

### A plot with NO component is filed under its template's own folder

**Measured on the 09:18 run, NG05, 156 plots over 7 templates, 150 workbooks.** Six plots were
ticked, READ, and dropped at the last step: EP-05, EP-11, EP-12, EP-13, EP-15 and FM-08, each
with `the component folder table does not hold an empty component`.

They are on no sheet, so they carry no component, so `PlotsPerTemplate` placed them by their
PLOT PREFIX, which is exactly what its own rule says it must do and what its docstring names
EP-05, EP-11, EP-12 and EP-13 as the case for. Then this table, keyed on the component, had
nothing for them. **Two routes to a template and one to a folder**, which is the two records
shape where the two records are the two steps of one decision.

**NOTHING NEW IS WRITTEN DOWN TO FIX IT, AND THAT IS THE WHOLE POINT.** Three shapes were on the
table and two of them add an eighth table, a template to folder map or a prefix to folder map,
which is another record to keep in step with these two. The third refuses the plot, which is
honest and costs six plots of a hundred and fifty six on every run.

The answer is none of the three, because the relation is ALREADY written down twice over. Both
tables are keyed on the same eleven component values, so which folders a template reaches is
read off the two of them together, `ComponentFolders.FoldersOf`. Counted off the tables by hand:

```
EXISTING PARKS   EXISTING PARK      HEALTHCARE   HEALTHCARE     STREETS   STREETS
FUTURE PARKS     FUTURE PARKS       PARKING      PARKING LOT
SCHOOLS          SCHOOL             MOSQUES      DAILY MOSQUE and FRIDAY MOSQUE
```

**Six templates reach exactly ONE folder and MOSQUES reaches two.** So a plot carrying no
component is filed where every other plot of its template is filed, which places five of the
six, and **where a template reaches more than one folder nothing is derived**, which still
refuses FM-08. That is the same shape as two filled regions holding an area on one plot: one
answer answers itself and two is a question the data cannot settle.

**AN ABSENCE AND AN ANSWER NOBODY KNOWS ARE TWO DIFFERENT THINGS.** A component the table does
not hold still places nothing and still names the value, because falling back to the template
there would file a value the team has never seen under a folder the team never chose. This is
for an EMPTY component only.

**AND THE PLOT IS NAMED BEFORE THE PRESS.** `CreateWords.PlotsWithNoComponent` counts the ticked
plots carrying no component, says how many of them file under their template's own folder, and
names by name the one that can be filed nowhere, above Create rather than twenty minutes later
in a report. **Reading five park plots and throwing them away is work nobody asked for.** The
count is a note and the plot that goes nowhere is a refusal, and a run where every ticked plot
carries a component says nothing at all, because a line about nothing is one the team reads past
on every other press.

**The line names no parameter.** `PRX_COMPONENT` is the workbook's note and is in no model, and
a test over these lines refuses it the same way the template block's test does.

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

**QUANTITY UNIT is read and checked.** A row in anything but m is not used and is named, because
2,051 of the file's rows are areas in sqm and one written into the total length cell would have
the workbook compute an area of an area.

**Two rows for one UID refuses and names both.** The file holds 33 such UIDs, none of them in
ANH-007, so it fires on no NG05 plot today and is still the rule.

**The file writes an absent value as the text Null in angle brackets rather than leaving the cell
empty**, on all 2,051 sqm rows' road width and on one row's unit. It is text, so `CellNumber`
reads no number out of it and the plot is named, which is why the numbers go through that tested
reader rather than through a parse written here.

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

## Three lines at the top of the report, so a run can be checked at a glance

**All three were answerable before this and all three were spread over hundreds of lines.** The
streets area sat in one block per plot, the region type in one row per plot, and the divisions
in one formula section per template. On a run of 156 plots that is a person reading a 2,000 line
file to count. `RunAtAGlance.Of` counts them once over every template and `THIS RUN AT A GLANCE`
is the FIRST section of the file, above the run's own accounting and above every per template
block. **Each is one line and a short list, and the detail stays exactly where it was.**

```
THE STREETS AREA    how many street plots got a value in the area cell, how many did not,
                    and the ones that did not NAMED with the reason their own run recorded
THE REGION TYPE     how many plots took their area off each filled region type, the type the
                    client's note names first, and how many chose no region at all
THE DIVISIONS       how many #DIV/0! the formula check found, and of those how many divide
                    by a cell THIS RUN WROTE
```

**THE STREETS AREA IS COUNTED OFF THE OUTCOME AND OFF THE MAP.** A street plot got a value when
the template's own area cell landed in the output holding something, read back off the file, so
nothing here names H8 and nothing reads what the fill set out to write. A cell that landed
holding nothing did not get a value. A press with no street plot in it says so rather than
counting nought of nought, because no street plot is not the same as every street plot failing.
It is STREETS alone because STREETS is the template whose cell changed.

**THE REGION TYPES ARE COUNTED AS THE RUN FOUND THEM AND NO SECOND TYPE NAME IS IN THE CODE.**
`RegionChoice.TheNoteNames` is held because it is the note being checked, and it sorts to the
top so the two counts the team asks for are the first two rows. **RCRC_CADASTRAL LIMIT is
deliberately NOT written in beside it**: a rule naming two types would count a model's third
type under nothing, and this tool has paid for a name standing in for a fact before. A plot that
chose no region is counted apart from every type, because none holding one is a plot with
nothing to read and more than one holding one is a question the note could not settle, and
neither is a disagreement with the note.

**THE DIVISION'S KIND AND ITS DIVISOR TRAVEL ON THE FINDING.** `FormulaAtRisk.IsDivideByZero`
and `DivisorThisRunWrote` are properties set where the risk is built. **A signal that travels in
the data is not a signal**, which this repo has already paid for once: a reason reported by
printing a marker word into the message it described, and the commit carrying that fix refused
by its own message. A counter searching `Reason` for `#DIV/0!` would count a reason that merely
talks about a division. Measured: breaking the two properties at the construction site reddens
three of the new cases and leaves every one of `DivideByZeroTests` GREEN, because those assert
the sentence.

**The glance heading is the one heading in this report carrying no count.** Every other one
prints how many rows sit under it, so a section that found nothing reads differently from one
nobody filled in. Three is how many questions there are rather than how many of anything this
run found, and a constant sitting where a count goes is a number that reads as a measurement.
A test says so and checks the heading below it still counts.

## The client's note holds, and it decides where two regions hold an area

**THE 14:29 RUN SETTLED IT BY COUNTING**, NG05, 156 plots, 102 workbooks:

```
of 156 plots wanting an area
  98   took it off RCRC_OUT OF SCOPE (PRESENTATION), the type the note names
   0   took it off a type the note does not name
  58   chose no region at all
```

**So the note holds on every plot that chose.** The 9 September reading that NS-19 and NS-06
carry their area on CADASTRAL LIMIT **was wrong**: those plots carry BOTH, and one column was
read. That open question is closed, by a count rather than by an argument, which is what the
run's own region table was added for.

### Bader's decision, 14 September: where more than one holds an area, take the note's type

**51 of 78 street plots wrote nothing on that run, every one of them because two regions held an
area and the tool asked which.** 51 questions on the pane, 51 clicks, and a run that has to be
pressed again after each one. Every one of the 51 offered the note's type as one of its two:

```
NS-02  CADASTRAL LIMIT 3982   OUT OF SCOPE 4295
NS-03  CADASTRAL LIMIT 6750   OUT OF SCOPE 7648
NS-04  OUT OF SCOPE 12289     CADASTRAL LIMIT 10754
```

`RegionChoice.Pick` is the whole rule, in order. **A person's pick wins**, because it is an
answer and everything below is the tool working one out. **One region holding an area answers
itself**, whatever it is called, and the note decides nothing there. **More than one, with the
note's type among them, takes the note's type.**

**THE QUESTION STAYS WHERE THE NOTE'S TYPE IS NOT AMONG THEM**, because that is still something
the data cannot settle. So does the note's type held by TWO of them at once, which no model has
shown and which the note cannot separate either, and the unchosen reason says which of the two
cases it was because they need different answers from a person.

**THE TYPE NAME LIVES IN ONE PLACE, `RegionChoice.TheNoteNames`, and nothing else holds a hard
coded type. RCRC_CADASTRAL LIMIT is written nowhere in the tool.** The test fixture carries a
third name, `RCRC_SOMETHING NOBODY HAS MEASURED`, for the pair the note does not settle, because
a test that wants the old refusal has to build a pair without the note's type in it.

**`WhyUnchosen` ASKS `Pick` RATHER THAN DECIDING AGAIN.** Two rules for one question is the
fault this repo keeps paying for and it would have bitten here immediately: the pick would
choose and the reason would still print a question, which is a report at war with the workbook
beside it.

### The report says WHO chose, and a route is not a type name

`RegionPick` carries the type and the route as one record, set where the choice is made, and
`PlotReading.ChosenRegionPick` holds it so the two cannot drift. Four routes, one column in the
region table, `how it was chosen`:

```
nothing chosen
chosen by hand on the pane
the only region holding an area
more than one held an area and this is the type the client's note names
```

**A count of picks the note made and a count a person made are two different facts about a
run**, and a column printing only the type name says neither. The column beside it still says
whether the type was the one the note names, which is what the count above was read off.

**THE PICK USED TO DROP THE PLOT'S UID2.** `PlotReading.WithChosenRegion` rebuilt the reading
with every argument but the last, which defaults to null, so a plot answered after a refusal
came back with no `PRX_Plot_UID2` and was refused a second time with `no PRX_Plot_UID2 was read
off this plot's first sheet`. That is a sentence about the model and it was about that method.
**A default that reads as a deliberate empty is how a whole link in a chain goes missing without
a word**, which this file already carries once, about the three the team types.

## A reason that points at a screen is not a reason

The 14:29 report said this on **102 rows of 7,083 lines**:

```
NS-02 | STREETS | STREETS | ANH-007-ST-100050 |
  Nothing was written. 1 reason, shown in full above the Create button.
```

The plot, the template, the folder and the UID2 were all there, and then it pointed at a pane
nobody has open. **The reason itself appeared NOWHERE in the file.** Two lines down a plot
refused by its own path read `no PRX_Plot_UID2 was read off this plot's first sheet, and the
folder and the file are both named after it`, which is what a record looks like.

**A REPORT THAT CANNOT BE READ WITHOUT THE PANE BESIDE IT IS NOT A RECORD.**

**Seven places produce a refusal that can reach the file. TWO wrote a pointer**, and both
through one method, `CreateWords.WhyNothingWasWritten`:

```
the plot row, PlotOutcome.Why                        POINTED, fixed
the template row, CreateWords.SomePlotsWroteNothing  POINTED through the same method, fixed
a plot no route placed, TemplateSplit's own Why      already right
a plot whose path refused, PlotWorkbookPath.Why      already right, the EP-05 line
a template whose split refused, TemplateSplit        already right
the patch's own refusal                              already right, said in full both ways
the glance's streets area, RunAtAGlance              already right, it asks the method above
```

**The count and the pointer belong on the PANE and nowhere else**, where the reasons really are
in red directly above the button, and the 0928 run printed the same four lines twice on one
screen. `CreateWords.Wrote` still says it. **One method answers both**, with where the answer
goes as its only argument, so the two cannot come apart anywhere else.

**Where a refusal genuinely has several reasons the file holds ALL of them.** A report short of
the second reads exactly like a plot that had one.

## Read every rule off the template, never off the notes copy

**Two files arrive: the template the team fills, and a reference copy carrying the green source
notes. THEY ARE NOT THE SAME WORKBOOK.** Measured on the reissued STREETS pair:

```
                          notes copy      template
existing canopy sum       M93             M102
native count over         H3:H91          H3:H101
```

So the annotations were made on an older file. **The notes say where a value comes from and
nothing else.** Where the two disagree about a range, a row or a sheet, THE TEMPLATE WINS, and
the disagreement is named in the log rather than reconciled quietly.

**This is the second time an annotated set and a production set have differed.** The first was
the seven of 9 September, where the annotated set carried the mapping in green text and the
production set carried no note cell at all, which is why `KpiTemplates` holds the map as data
and nothing reads a mapping out of a workbook. That the two sets can also disagree about a ROW
is new, and it is why every row fact is read off the file the run is filling.

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

## What stands down now that a checklist is one plot

**Nothing is deleted.** Deleting on reachability alone is the mistake this project made once,
with `OutputName.Suggested`, and the rule beside it stands: a method the last caller stopped
calling can still be the only record of a shape. What follows is no longer reached, with why it
was kept.

- **A merged species list across plots.** `KpiMerge.Species` is asked with one reading now, so a
  `MergedSpecies` holding rows off two plots cannot be built. It is the only record of how a
  species merges on the GROUP and the botanical name together, which is what stopped ALBIZIA
  LEBBECK's 1 existing and 13 proposed becoming 14 in one sheet
- **Two plots reporting an identical raw area.** `Reconciliation`'s `IdenticalArea` needs two
  plots and there is one. It is the only record of the MM-03 and MM-04 measurement, 12182.05561411
  on both, and of the rule that a double count nobody sees is the worst thing this tool can
  produce
- **`Totalled.Adds` over several plots.** One plot means the sum trivially equals the total. It
  is the only record of the rule that every plot's own number is printed beside the total and the
  total must equal their sum
- **A plot counted into two workbooks.** `TemplateSplit.Refusals` is still computed on every
  press and still cannot fire, which was already true and is written down here
- **`OutputName.Final` and the cleaning it holds.** `Extension` beside it is what
  `PlotWorkbookPath` builds every file name with, so the class is live. `Final` is the only
  record of the cleaning a typed name needs

**The rounding room on a group total does NOT stand down**, against what the round message said.
It is per schedule and per plot, inside `ShrubsAndLawnRows`, so a phased group's rows still have
to add to its total within the project's own rounding step on every plot. Nothing about it moved.

## The name box is gone, and this is the second reason a thing gets deleted

The box read GRP-KPI-Checklist-DD-MOSQUES.xlsx after the round that made a workbook one plot,
and **no file is called that any more**: every one is named from its plot's own PRX_Plot_UID2. A
box a person can type into whose text nothing reads is worse than no box.

In its place, one line under the output folder saying where the files go and how each is named,
with a real example path off the team's own folders. Said once rather than once per ticked row,
because the shape is the same for every template.

**`OutputName.Suggested` is deleted, and this time it is not a reachability judgement.** It was
deleted once on reachability alone, restored because it was then the only record of the output
name's shape, and the rule beside that restoration still stands. What is different now is that
the SHAPE is gone rather than its last caller: there is no output name to suggest, because no
name is typed anywhere. Two reasons a thing gets deleted, and this is the second one.

## What the pane says before the press, and how much of it

**Measured on the first per plot run, NG05 at 00:00**: nine lines of red and orange above the
Create button, and on 78 street plots the plot list alone ran off the screen.

**NEVER EVERY PLOT ON THE PANE.** `CreateWords.TemplateRow` names up to four plots outright and
past that gives the count and the range, STREETS: 78 plots, ST-01 to ST-78, and says the report
names them. Four is what fits a line, and the report holds the list because that is where a list
belongs.

**A REFUSAL AND A NOTE LOOK DIFFERENT.** They were the same colour, so nothing said which of the
nine lines stopped a workbook. A refusal keeps the warning colour. A note takes the body colour
and opens with the word Note, through `Noted` in the pane beside `Warned` and `Faint`. What is a
note: the link state, a group no sheet takes, and a ticked template no plot belongs to. What is
a refusal: a reconciliation that does not add up, an identical area waiting on a confirm, and a
plot no route places.

**THE LINK NOTE IS THE COUNT AND WHAT TO DO.** `LinksLoaded.Warning` is six lines of link paths
and stays as it is for the report. `LinksLoaded.OnThePane` is the short half, how many of how
many are not loaded, what to do, and that the report names them.

**The area line for STREETS said typed by hand and they are not.** The road width and the total
length come off the reference file. A line about what the tool does is checked against what it
does, which is a rule this file already carries and this is the second time it has bitten.

Nothing moved and no control changed. This is wording and how much of it.

## Count what happened, never what was planned

**Measured on that same run**: the pane and the report both read MOSQUES: Nothing was written.
20 of 21 plots wrote a workbook, with twenty workbooks on disk. One line held two records of one
fact, the row counting what the run set out to do and the sentence counting what it did.

`TemplateOutcome.Workbooks` is the count of plots that really wrote, and every word the row says
is read off it. Three answers rather than two: every plot wrote, SOME wrote with the ones that
did not named, or NOTHING was written, which is the one case the word refused still fits.

**COUNT WORKBOOKS WHERE THE UNIT IS A WORKBOOK.** The summary read 1 workbook written of 2
templates ticked on a press that wrote 98, because it counted templates and called them
workbooks. It counts workbooks and plots now, with the templates said as the thing the plots
were spread over.

## Adding printed numbers is allowed, working one out is not

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed. Recomputing a number off the elements a schedule lists is never allowed and
happens nowhere, because those elements are RVT Link instances on this model.

What makes the addition safe is that **every plot's own number is printed beside the total and
the total must equal their sum.** `Totalled` carries both and `Adds` works the sum out again.
A total that does not equal its parts refuses the write. The tool does not write a total with a
note attached.

Trees merge on the group AND the botanical name, never the name alone. ALBIZIA LEBBECK is 1
existing and 13 proposed on DM-12, and merging on the name would put 14 in one tree sheet.

**Two plots reporting an identical raw area are flagged, never silently added.** MM-03 and
MM-04 both read 12182.05561411 in the 00 link. Either they are the same size or one region is
counted twice, and a double count nobody sees is the worst thing this tool can produce, so a
person confirms before anything is written. More than one region holding an area on one plot
refuses the same way **where the client's note names none of them**, under the section above.

## Every plot chosen is accounted for on the way out

`Reconciliation` opens the report: plots ticked, plots read, plots with each schedule and with
an area, and every plot that contributed nothing with its reason. A plot that gave nothing is
named rather than quietly absent, because a plot list that goes in longer than it comes out is
the failure this exists to catch. It refuses the write when the numbers do not agree.

**A choice made after a refusal is applied to the run already read.** Every region choice and
the identical areas confirm used to read every ticked plot from the start, five minutes on 20
plots and about twenty on 78, once per plot that needed a pick. A refusal exists so a person
can answer a question, and answering it should not cost the answer again. The run the pane
holds travels on the ask as the same object, `HeldReadings.Decide` says whether it can answer
this press, and `Applied` puts each plot's chosen region on its reading through
`PlotReading.WithChosenRegion` with nothing read. It is not a cache with a lifetime of its own.
The run before is trusted when it wrote nothing and it is the same model title, the same
template and template file, the same two parameters and the same plots, and every ticked plot
has a reading on it that was not refused. Anything else reads the model again with the reason
named: no run is held, the run before wrote its workbook, the model or the template or a
parameter or the plots changed, a plot has no reading, or a plot's read was refused, because a
plot that threw holds nothing to reuse and the model fixed in between is the reason for the
second press. The report says which under the Run line, Readings: reused
or read from the model on this press, with the reason, so a read of 0.0 seconds on a reused run
is true and says why. A model edited between the refusal and the pick is the one thing none of
those comparisons can see, and that is for Bader.

**It knows the template, and on a template that names no area cell the area is not read.** That
was STREETS, whose sheet worked the area out from the road width and the total length, and MM-03
and MM-04 are street plots both reading 12182.05561411 in the 00 link, so a reconciliation that
did not know the template would have ended the first 78 plot run asking the user to confirm an
area the workbook had no cell for, then read all 78 again. On such a template no filled region
is read, nothing about the area is refused on, and the report says the area was not read and why
rather than leaving the section empty.

**NO TEMPLATE NAMES NO AREA CELL TODAY**, since the client emptied STREETS H8, so that path is
live for nothing and every template goes down the same one. It is kept because the map can still
express a template that names none and `KpiTemplate.TakesNoArea` is what every one of those
paths asks. **It was called `AreaIsTypedByHand` and no template ever typed one**: that name was
the STREETS story rather than the rule, and a name that tells one template's story is a name
that stops being true when that template changes.

**A refused schedule read refuses the write.** A column the heading row did not name, a cell
holding a digit past where its number ends, a species row with no whole count: each travels on
the plot reading with the schedule's name, prints in red above Create, and prints twice in the
report, among the reasons and under the plot. A refused read used to come back as a list of
nothing, which read as a plot whose schedule listed no species.

**A throw on one plot names the plot and the run carries on.** The loop over the ticked plots
read each one under no guard of its own, so a throw on plot 60 of 78 fell to the run's catches,
which said Revit would not do that now with no plot named and wrote no report, and the two
refusals for a plot list that comes out shorter or longer than it went in could never fire.
`GuardedRead` in `KpiRequestHandler` wraps each plot's regions and read and hands back
`PlotReading.NotRead`, a reading whose one refusal is the read threw and nothing on this plot
was read, with the exception's type and message. Each schedule's read inside
`KpiPlotReader.Read` is guarded the same way and names the schedule. Every exception type is
caught there on purpose, the same as the scan side's `Guarded`, because a partial report that
names the bad plot is the point. The run carries on, the reconciliation refuses the write naming
the plot, the plot is listed under contributed nothing with the refusal first, the status line
counts it as a reason, and the report is written either way. The catch inside `Printed` that
swallows an `ApplicationException` with nothing recorded is finding 6's and stands.

**A plot holds one schedule of a kind or the kind is not read.** The reader finds every
schedule of each kind filtered on the plot before it reads any, reads the one when there is
one, and hands `PlotReading` the names of all of them. A reading refuses to hold species rows
beside two softscape names or subtotals beside two shrubs and lawn names, and `Reconciliation`
refuses the write naming the plot, the kind and every schedule found. The report names the
schedule each number came off, per plot, and says NONE READ with every name where there were
two. Nothing picks the first, and nothing adds them.

**THE FM-05 DOUBLE WAS NOT TWO SCHEDULES, AND IT WAS NOT ONE SPECIES PRINTED TWICE UNDER ONE
GROUP EITHER. IT WAS A THIRD GROUP.** The first twenty plot run printed FM-05 twice in one
species row, FM-05 10, FM-05 10, FM-06 15, and the round that fixed it took that for a second
schedule without measuring one. The 1428 run read one softscape schedule on every plot and
printed the same rows, ALBIZIA LEBBECK 10 and 10, BAUHINIA PURPUREA 19 and 20, CASSIA GLAUCA 3
and 4, and the round that fixed that took them for two rows of one species under Proposed and
refused the plot. The 1536 report's printed section showed the second of each pair sitting
under a third group row, STREET DESIGN at row 14, after Proposed's own subtotal row. Two rounds
diagnosed a shape nobody had looked at. **The section that prints the schedule as printed is
what settled it, and it is why the report ends with what the tool read.**

A species on two rows under ONE group is still refused, with the plot, the species, the group,
the rows and the counts named, because nothing says whether that is two types of it or one
counted twice. `SpeciesRow` carries the row it printed on and the row of the group it sat under,
and `PlotReading.SpeciesPrintedOnMoreThanOneRow` finds every such species. A species under two
groups is two species and refuses nothing.

## A run with no link loaded says so before it says anything else

**Measured on the first STREETS run, NG05 at 16:06, 78 plots.** All 78 contributed nothing.
All 156 schedules printed one row, the header, and no body, and the group rows read none on
every one of them. The scan from the same session says six link instances and NONE LOADED. The
plants are in the linked component models, which is the rule this file already carries, so with
no link loaded a schedule has nothing to list, and ST-05-(600) SOFTSCAPE SCHEDULE opens empty on
screen. **Every number the tool printed was right and every plot carried its own reason.** What
it never said was the one thing that explains all 78 at once, and a 2,292 line report opening
with a reconciliation of nothing is a tool watching a person work out what it already knew.

`LinksLoaded.Of` is the whole rule. It is a NOTE and never a refusal, because a model with no
link loaded is a legitimate thing to open and the team may be working on the host alone. Five
states, one line each. The links were not read at all, which claims nothing either way. The
model holds no linked model. No link is loaded, with the count and every name. Some loaded and
some not, with the count and the names of the ones that are not. And every link loaded, which
is the one state worth saying nothing about.

**It is at the TOP of the checklist report, above the reconciliation**, through `TheLinks` in
`KpiCreateReport` sitting between the clock and the first step, with a test asserting the
warning's position is before RECONCILIATION's rather than asserting both are present.

**It is on the pane too, before the press**, above Create beside the refusal line, off
`KpiPlotFacts.Links`, which the plot read fills from the host document's own link instances. So
the one thing that would have made the 16:06 run pointless is on screen before the twenty
minutes are spent rather than in the file afterwards.

**And the reconciliation counts what was FOUND, not only what was left out.** Every count it
carried read green over that run: 78 ticked, 78 read, 156 schedules found, nothing refused and
nothing missing, because a schedule that printed no body is a schedule that was read. Two
counts cannot do that. `Reconciliation.GroupRowsFound` is how many group rows the whole run
found and `SchedulesWithABody` how many of the schedules printed one, said as a count of the
schedules printed. The 16:06 run reads 0 group rows and 0 of 156 with a body, and no wording
makes that look like a run that worked.

## Several templates in one press, one workbook each

The plot list already reached every plot and the grouping buttons already ticked a template's
worth at a time. What the tool could not do was press MOSQUES and SCHOOLS and get two
workbooks.

**NOTHING ABOUT THE PER TEMPLATE LOGIC CHANGED.** Each workbook is filled exactly as one is
filled today, by its own map, its own tree lists read off its own file, its own Street Design
rule, its own group rules, its own canopy check, its own read back, its own cache fix and its
own alias list. `KpiCreateRun` is still one template's run and `KpiCreateReport.Write` still
prints one template's sections. A template is a template whether one or six are ticked.

**The workbook rows are tickable, several at once.** A ticked row carries the template it
recognised as and its own output name box, because ONE BOX CANNOT NAME SIX FILES. A file caught
between the two park templates is ticked with no template until the user says which, and it
arms nothing meanwhile. Where exactly one is ticked the box behaves as it always did.

**Each workbook gets only the plots whose own template is that one.** `PlotsPerTemplate` is
that decision and it is the rule this file already carried for preselecting, read per plot:
PRX_Component decides, the plot prefix is the cross check, and where they disagree NEITHER of
them does. Three plots go into no workbook and each is NAMED rather than quietly absent. A
component the table does not hold places nothing, because the route that decides gave an answer
nobody knows and the cross check does not get to answer in its place. A plot with no component
at all is placed by its prefix, which is the only thing that can place EP-05, EP-11, EP-12 and
EP-13. And a plot whose two routes disagree is placed by neither.

**A PLOT COUNTED INTO TWO WORKBOOKS REFUSES THE WHOLE PRESS.** Every plot resolves to at most
one template, so it cannot happen by construction, and `TemplateSplit` checks the split as it
really came out anyway, naming the plot and both templates. 20 mosque plots and 78 street plots
read once and split two ways is exactly where a plot lands in both or in neither with every
total still looking plausible. A construction that cannot go wrong is not a check, and the check
is what survives the next change to the split.

**A ticked template no ticked plot belongs to writes nothing and says so BEFORE the press**, in
its own row, with the reason. Bader's decision: it stays tickable and stays listed, and the tool
neither hides it nor unticks it for them.

**The reading is read once and there is no second cache.** A plot belongs to one template, so
it is read once with that template's own counted groups and its own area rule, and never again
for a template it does not belong to. `HeldReadings.Decide` is asked once per template, with
that template's own share of the plots and the run IT produced last press, so a choice made
after a refusal still costs no read. A held run for one template never answers for another,
and the line saying which happened is per template.

**The progress count runs across the whole press.** Counted per template it would restart at 1
on the second workbook, and a count that goes backwards is the one thing the progress rule
forbids. The total is every plot every ticked template will read, worked out before the first
one is, and a template answering from held readings still advances it.

**The three the team types are ONE SET for the whole run.** Bader's decision. The date, the
prepared by and the position go into every workbook this press writes, and the pane says so
when more than one template is ticked.

**A refusal on one template does not stop the others.** Bader's decision. The rest are written
and the refused one is named with why. After the press each row says what happened to it,
written with its path or not written with its reason, because one line for the run would hide
which of six failed.

**ONE REPORT FOR THE RUN, not one per workbook.** `KpiCreateReport.WriteAll` opens with the run
accounting, then the split plot by plot with the route each took, then every ticked template's
own report whole under a heading naming it, through the section writer that was already tested.
A Street Design note on MOSQUES and a rounding note on STREETS sit under their own template and
can never read as one list.

**The run accounting sits above each template's own reconciliation**: templates ticked, written,
refused and with nothing to write, and THOSE FOUR MUST ADD UP TO THE NUMBER TICKED. They are
counted off one list and checked against its own length, so a template that fell out of every
branch is a refusal in those words rather than a row nobody printed. Each template's own
reconciliation is unchanged and adds up within itself, over its own share of the plots.

**`OutputName.Suggested` is BACK, and deleting it was the fault.** It offered the template
file's own name, nothing called it once every row named itself, and it was deleted as
unreachable. The rows named themselves after the TEMPLATE alone, so the boxes read MOSQUES and
a run would have written MOSQUES.xlsx, which nobody recognises in a folder three months later.
The working runs wrote GRP-KPI-Checklist-DD-MOSQUES.xlsx and GRP-KPI-Checklist-DD-STREETS.xlsx
and this is what wrote them. **Reachability is not the whole test for whether a thing is
needed**: a method the last caller stopped calling can still be the only record of a shape, and
this one was.

## Nothing heavy runs without a press

**Measured on the first press over several templates, NG05 at 08:37.** Opening a model in Revit
started a read with no press behind it, and on NG05 that held the model for minutes.

The cause was a line in the round that took the scan button away: the pane still needed the
model's name and element count at the top, so read those when the pane is shown. **A dockable
pane is restored VISIBLE at Revit startup**, so every model anybody opened was read before
anyone had asked for anything.

**THE HEADER MUST COST NOTHING.** The model's name is free. The element count is not: counting
96,959 elements IS the read. `KpiHeader.Lines` is the whole decision, three states with one
line each, and **only the third names a count**. A model nothing has read says so and names no
number, because **a read that has not happened is an ABSENCE and not a zero**.

Every path that can start a read, and whether a person asked:

```
the pane becoming visible        Ask(WhichModel), the title alone      FREE, and was right
every redraw of the block        Ask(WhichModel), the title alone      FREE, and was right
a model answering with a title   Ask(Plots), every sheet and schedule  THE FAULT, now gone
Read this model                  Ask(Plots)                            A PRESS
Create                           the scan and the plot reads inside it A PRESS, and was right
```

The KPI pane subscribes to DocumentOpened and DocumentClosed nowhere, so neither could start
one, and that was already right. `Took` asked for the plots off the answered title, which is
what fired on every model opened, and it asks for nothing now. **The plots are read by the one
press in the plots block**, and Create still reads what it needs itself.

**THE PRESS IS AGREED, AND THE RULE IS NOTHING READS WITHOUT A PRESS.** Bader's answer,
correcting the round message this came out of: Create is the only thing that reads was the wrong
wording and a button IS a press, so **the fault was a read nobody asked for rather than a way to
ask for one.** The plot picker cannot be used before the plots exist, and this is how they come
to exist.

**It is better placed than the scan button it follows.** KPI Scan sat at the top of the pane and
on the ribbon whether or not anybody needed it. This sits in the plots block, where the thing it
produces goes, so the press and its result are in one place.

## Ticking a template ticks its plots

**Measured on that same press**: MOSQUES, PARKING, SCHOOLS and STREETS ticked, six plots ticked,
every one of them SC, and three of the four templates read no ticked plot belongs to them. The
tool did exactly what it was told and **the specification was wrong.** A person who ticks
MOSQUES has said which plots they mean, and making them find a grouping button and press that
too is the same fact asked for twice.

`TickingATemplate` is the rule. Ticking a row ticks every plot that belongs to it, unticking
takes them off, and the row IS the grouping button, so the separate row of buttons is gone.

**IT TICKS BY THE SPLIT'S OWN RULE AND NEVER BY THE PREFIX.** The grouping buttons gathered
plots with `PlotPrefixes.PlotsFor`, which reads the two letters at the front of an identifier,
while `PlotsPerTemplate` decides which workbook a plot really goes into by reading PRX_Component
first. Two rules for one question is the fault this repository keeps paying for: a plot ticked
by the prefix could then land in no workbook at all and the row's count would be a number
nothing else agreed with. A test ticks MOSQUES on a plot whose component says SCHOOL and its
prefix says MOSQUES, and the plot is NOT ticked, because neither route places it.

**A plot ticked or unticked by hand wins, IN BOTH DIRECTIONS.** Ticking a template is a starting
point rather than a lock, so a plot taken off by hand stays off and the row then says 19 rather
than 20, and a plot put on by hand stays on when the row is unticked. The pane holds one
`HandTicks` and forgets it when the model changes, because another model's DM-14 is not this
one's, and when Select all or Clear is pressed, under the section further down. **The count on the row is what will actually go in**, which needs nothing new: the row
reads its count off the split of the TICKED plots, so a hand untick moves it.

**A template with no plots at all still ticks and still says it will write nothing**, exactly
as before, and it stays listed.

**A row ticked before the read gets its plots when the read lands.** The template rows come off
the templates folder and need no model, so ticking MOSQUES and then pressing Read is the
ordinary order and the plots cannot be ticked until they exist.

**What the grouping buttons did that the rows do not.** Two things, both deliberate. They
REPLACED the ticks rather than adding, which was right when one checklist was one template and
is wrong now that several are ticked at once. And they could tick a template's plots without
that template's workbook being ticked, which is now impossible and is the point: the two halves
of one choice move together.

**The seven members they left behind are deleted, and the test was what each one RECORDS.**
Bader's call, and the test is his: reachability is the wrong question, and a method nothing
calls is kept when its shape is written down nowhere else and deleted when it is. All seven came
out deleted, and each for its own reason:

```
CreateWords.GroupsHeading        that grouping went by the prefix     PlotPrefixes' own docstring
CreateWords.GroupLabel           a row names its count before a press CreateWords.TemplateRow
CreateWords.NoGroupFor           a plot no route places is named      TemplateSplit.Unplaced
PlotTicks.OnlyFor                REPLACE rather than add              TickingATemplate, reversed
PlotPrefixes.Grouped             the template list's own order        PlotsPerTemplate.Split
PlotPrefixes.WithNoKnownPrefix   an unknown prefix is its own bucket  PlotPrefixes.Across
PlotPrefixes.PlotsFor            the prefix route over a list         PlotPrefixes.For
```

**They came out the same because they are one feature's parts and not seven things.** A heading,
a button's text, its footnote, its press and the three lookups that fed it are the grouping row,
and a feature is removed as a feature. What was worth keeping was never among them: it is the
prefix TABLE, which is the measurement, and `All`, `Of`, `For`, `Across` and `PrefixesFor` all
stay. Two of the seven were worse than unused. `OnlyFor` implemented a rule the tool has since
decided against, so it was a second and contradictory record waiting for a caller, and
`NoGroupFor` told the reader to tick such a plot by hand, which now lands it in `Unplaced`
writing nowhere.

**`PrefixesFor` is the one kept on that test, and it is not one of the seven.** Nothing calls it
either, and it is the only record of the many to one shape: three prefixes mean STREETS and two
mean MOSQUES. Read the other way, off `For`, a template is reached one prefix at a time and the
many to one is invisible. Its docstring says that and says nothing calls it.

The plot list itself is untouched, so anybody who wants to pick plots by hand still can. It was
the two not talking that was the fault rather than either one of them.

## Only the groups a tree list sheet is named for count

**Bader has decided that Street Design is somebody else's scope and does not belong on this
plot's checklist. The model will be corrected later. Until it is, the tool leaves those rows
out and says so.** That is a decision, recorded in `steps/log-kpi.md` as one, and not a
measurement.

The words Existing and Proposed appear nowhere in the code that decides it. `CountedGroups`
holds the two tree list sheet names off the template, Tree List - Existing and Tree List -
Proposed, and a group counts when a sheet's name ends in the group's name, word for word and
without case. Nothing looser: Tree and List are words of both sheet names, TREES is the heading
over the groups, and none of those is what either sheet is for. All seven templates name their
sheets that way, so a group the workbook has no sheet for is out of scope on every one.
`KpiRequestHandler` no longer reads the document's phases for the create path. The scan still
does, for section 7, and that is a different question.

**Every group row is found and every group row is named.** `SoftscapeRows.Read` reads a text
only row followed by anything but another text only row as a group row, the species rows under
it as its rows, and the first count with no name under them as its subtotal. Each comes back as
a `PrintedGroup`, in printed order, with its row, its species, its subtotal row, TAKEN or LEFT
OUT and why. The reading's species are the taken groups' rows. Two checks, both refusals in
`Reconciliation`: each group's species rows against its own subtotal row, and the groups taken
plus the groups left out against the TOTAL row.

```
FM-05, off the 1536 report
row 3   Existing        4 species rows adding to 6     subtotal row 8 prints 6     TAKEN
row 9   Proposed        3 species rows adding to 32    subtotal row 13 prints 32   TAKEN
row 14  Street Design   4 species rows adding to 38    subtotal row 19 prints 38   LEFT OUT
row 20  TOTAL 76        6 plus 32 taken, 38 left out, 76
```

**The shrubs and lawn schedule prints the same third phase and the value is the phases taken
added together**, area and item count, with the group total row as the check on every phase,
taken or not. FM-05 GRASS: Proposed 96 over 117 taken, Street Design 69 over 84 left out, the
group total 165 over 201 checked. SHRUBS AND GROUND COVER: 361 over 450 taken, 459 over 570 left
out, 820 over 1020 checked. A group whose every phase is out of scope is nought and says so. A
group with no phase row at all keeps the rule it had, because it offers nothing else: the last
row is the value and the rows above it must add to it.

**FM-05 reads 6 existing and 32 proposed trees, ALBIZIA LEBBECK 10 and not 20, grass 96 and
shrubs 361.** The rule before this one refused the plot, the one before that wrote 165 and 820.

The report says all of it under the plot: every group row with its numbers and its reason, the
rows left out by name and count, each phase row of the shrubs and lawn groups the same way and
the group total row with whether the phases add to it. The accounting at the top counts the
schedules holding a group no tree list sheet is named for and names the plots, which is the
line that would have shown the street on the first twenty plot run.

**A schedule can repeat a group name, and nothing guesses which is meant.** DM-25 prints
Existing, then Proposed, then Existing again. Both Existing rows are in the list with their own
row numbers and subtotals, both are taken, and the second's reason says it is the 2nd group row
so named on this schedule. A species under both is refused as a species under one group name
twice, and the refusal names the two group rows so a person can see it is two groups. Whether
those are one phase printed twice or two things is UNKNOWN and is for the team.

**One shape nobody has measured.** A softscape schedule printing TREES and then species rows
with no phase row would read TREES as a group row, TREES counts for nothing, and every species
would be left out and named. No such schedule has been seen.

## Street Design counts as Proposed on STREETS, and nowhere else

**Bader's decision, on top of the rule above.** No template has a Tree List - Street Design
sheet, so the rule alone left a Street Design group out everywhere, streets included, and on
a street plot that group is the plot's own work. Measured on ST-05, a street plot, off its
softscape schedule on screen:

```
Existing        369
Proposed          2    ALBIZIA LEBBECK 2
Street Design    68    ALBIZIA LEBBECK 6, CASSIA GLAUCA 62
TOTAL           439
```

ST-05's proposed trees are 2 plus 68, 70, its existing are 369, and 369 plus 70 is 439, the
TOTAL the schedule prints. That is the test.

### CONFIRMED BY BADER, 14 September, and the two things beside it are not

**The client confirmed it: STREET DESIGN AND PROPOSED ARE BOTH PROPOSED on STREETS.** That is
what the tool already does through `KpiTemplate.GroupsCountedAsProposed`, and it has run. ST-05
read Existing 369, Proposed 2 and Street Design 68, and 2 plus 68 went into Tree List - Proposed
as 70, against the schedule's own TOTAL of 439. **NOTHING IN THE CODE CHANGES.** What changes is
what the line rests on. **A rule the client has confirmed reads differently from one the tool
inferred**, and until 14 September this was a decision made in September that nobody had checked
since.

**The two things that travel with it are NOT confirmed**, and they are written out here so they
stay visible rather than being carried along by the confirmation above.

- **On every template that is not STREETS, a Street Design group is still LEFT OUT and named.**
  That is Bader's decision of 10 September and the client has not been asked about it. DM-16 and
  FM-05 are where it fires today, both on MOSQUES
- **A group named anything other than Existing, Proposed or Street Design is out of scope on
  EVERY template, STREETS included.** No tree list sheet is named for it and no decision takes
  it, so it is left out and named. Nobody has confirmed that either, and no such group has been
  measured on any model

**A confirmation covers what was asked and nothing sitting next to it.** All three of these were
one paragraph in this file and the client answered one of them.

**It is data on the template and it is keyed on the template, never on the plot prefix.**
`KpiTemplate.GroupsCountedAsProposed` holds Street Design on STREETS and nothing on the other
six, `CountedGroups.Of` reads it into a `GroupByDecision` pointing at Tree List - Proposed,
and `SheetFor` answers a sheet named for the group first and a sheet that takes it by decision
second. PRX_Component picks the template and the prefix is only a cross check, which is already
the rule, so the same schedule read for MOSQUES leaves the group out with 68 named, and a mosque
plot read for STREETS counts it. **A second name goes into that list only when the team says
so.**

**The rows go where the sheet takes them, and a species under Proposed and under Street Design
adds.** `KpiMerge.Species` takes the `CountedGroups` and keys every row on the sheet that takes
its group, so ALBIZIA LEBBECK on ST-05 is one merged row of 8, ST-05 8 (2 rows, 2 + 6), going
to Tree List - Proposed and saying both groups. That is two groups, not one species printed
twice under one group, so the same group refusal does not trip. `SpeciesMatching` places a
merged species through the sheet the merge decided and, for one built with none, through the
same `CountedGroups`, so nothing here matches a word of a sheet name on its own any more. The
street's areas count the same way: a Street Design phase row in the shrubs and lawn schedule is
taken on STREETS and its area adds.

**The report says which route each group took.** The reason beside a group row reads Tree List
- Proposed is named for it, or Tree List - Proposed takes it on STREETS by decision, as that
plot's own work, or no tree list sheet is named for it, so it is out of scope. A Street Design
group counted on STREETS reads differently from one left out on MOSQUES.

**The note goes on the pane, not only in the report.** `CreateWords.GroupsLeftOut` builds one
short block above the Create button naming the plots and the schedules where a group no sheet
takes was found, Street Design found on 2 plots on MOSQUES, which has no sheet for it: DM-16 in
its softscape and its shrubs and lawn schedules, FM-05 in both. Those rows were left out. Fix
them in the model. It is a NOTE and NOT A REFUSAL: the run goes through, the workbook is
written, the numbers are right, and the note says where the model needs correcting. Plots and
schedules, never species, because on a run of 78 plots a long list is not read. The report
keeps the full detail with the counts and the areas left out.

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

### The three forms, and the prefix is what picks one

```
Projects Basic Data - Parks                                42 fields   EP, FP
Projects Basic Data - Open spaces associated to buildings  49 fields   HF, FM, DM, PL, SC
Projects Basic Data - Roads                                34 fields   NS, ST, MM
```

**KEYED ON THE PLOT PREFIX, which is the first thing the prefix decides on its own.** Everywhere
else in this tool it cross checks a component that has the last word. Which form a plot gets is
the team's filing and `PRX_Component` says nothing about it. A prefix `PdfForms` does not hold
writes no PDF and is named, and a test walks every prefix `PlotPrefixes` holds so the two
records cannot say different things about which prefixes exist.

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
  the form over the client's own copy and paste, and which the client means is an open question
  in `steps/log-kpi.md`

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

**The Roads form's Total areas to be greened is filled from ITS OWN note**, which names the canopy
cell where the other two name Total Green cover. Each form gets what its own file says, the working
says which of the two the number is, and which the client means stays an open question.

### Every field is WRITTEN or EMPTIED, and the tick boxes are the only third case

**THE CLIENT'S DEFAULT VALUES ARE NOTES FOR WHOEVER FILLS THE FORM BY HAND. THEY ARE NOT
CONTENT.** Measured on all eight PDFs of the 18:15 run over NG05:

```
Irrigation water demand   held  Revit / softscape & shrubs & lawn schedule / total water demand /1000
Ground Cover              held  REVIT SHEET/... GROUND COVER TOTAL AREA
```

**150 PDFs went to a client with the instruction for filling a box printed inside that box.** The
tool wrote the fields it had values for and left every other one exactly as the template had it,
so a note sat where an answer belongs, and a note in a box reads as an answer.

**A FIELD THE TOOL DOES NOT FILL MUST LOOK LIKE A FIELD NOBODY HAS FILLED.** So every text field
of every form this tool writes is now WRITTEN or EMPTIED, including every field the tool has no
source for and names nowhere.

**The only fields left as the template has them are the ones that are NOT TEXT.** That is the four
stage tick boxes and the Reset button, and they are left because of what they ARE rather than
because anybody listed their names: `PdfFieldRead.FieldType` is read off the file's own `/FT`,
inherited through the parent chain the way an AcroForm defines it, and only `Tx` is cleared. **A
field whose kind cannot be read at all is left alone too**, because this tool does not clear what
it cannot classify.

All three forms, field by field:

```
WRITTEN, where the plot has a value      the 14 fields PdfForms names, per form
EMPTIED, with its own reason             a named field the plot had no value for, which carries
                                         the reason it already had, so the ground cover says the
                                         schedule prints no such group rather than a general line
EMPTIED, as having no source at all      every other text field: on Roads the sidewalk, median,
                                         sidemedian, empty areas and water tanks, on Parks the two
                                         people counts, the cycling and pedestrian paths, the
                                         toilets, kiosk, play grounds, fitness area, muga sport
                                         field, running track and water tanks, and on Open spaces
                                         the length, the paths, the maintenance road, the toilets,
                                         kiosk, exhibition spaces, parking provided, seating and
                                         play areas, the bridges, the catwalk and the water tanks
LEFT AS THE TEMPLATE HAS IT              Schematic Design, Detailed Design, Tender and
                                         Construction, and the Reset button. Four tick boxes and
                                         one push button, all /FT/Btn, none of them text
```

**THE REPORT NAMES EVERY EMPTIED FIELD WITH WHY, per plot**, and reads back what landed in each,
so a blank box is a decision on the record rather than an oversight and a box the tool meant to
clear and did not is visible.

### THREE OF THOSE DEFAULTS ARE THE CLIENT'S OWN HEADER, AND CLEARING THEM WAS THE FAULT

**Measured on all seven PDFs of the 19:52 run**: Project name empty, Consultant empty, Contract
reference empty, on every one of them. The rule above did exactly what it says and the rule was
one field too wide. Those three are TYPED DEFAULTS rather than notes:

```
Project name          Neighborhood Landscape Design - Zone #2
Consultant            SAPL
Contract reference    GP.NH.Z2.052-DES042
```

**PROJECT NAME AND CONSULTANT ARE LEFT ALONE. THEIR DEFAULT IS A VALUE.** Bader, 14 September.
They are the client's own header carried on every form, they are the same on every plot, and they
are **the only two fields in that state**. Every other text field is still written or emptied.

**DO NOT TRY TO TELL A NOTE FROM A VALUE BY READING THE TEXT.** Nothing anywhere looks at whether
a default reads like an instruction. The three are held as DATA, `PdfForms.HeaderValues`, spelt
exactly as the files carry them.

**THEY ARE FOUND BY THE VALUE AND NOT BY A FIELD NAME, BECAUSE NO FIELD NAME IS MEASURED HERE.**
No client PDF enters this repository and none ever will, so the names of those three boxes are
UNKNOWN in this repo. What is known is what the client's own template holds in them, which is what
came back off the seven outputs. So `PdfEmptying.IsTheClientsHeader` compares the field's value
against the two left alone, and `PdfEmptying.TheContractReference` finds the third the same way.

**WHAT MAKES THAT SAFE IS THE FORM CHECK REFUSING A FORM THAT DOES NOT CARRY ALL THREE.**
`PdfFormCheck.Of` looks for a text field holding each of the three and adds a missing one to the
same list a missing field name goes in, so a form whose header the client changes **writes
nothing at all** rather than clearing a header the tool no longer recognises or writing this
plot's reference into a box nobody measured. That is the same shape as every other check on these
forms: the name, the note and the position all have to hold.

### CONTRACT REFERENCE IS WRITTEN, PER PLOT, FROM PRX_Plot_NH

**Bader's decision, 14 September.** It is the third of the three and it is the one that moves: the
template's own `GP.NH.Z2.052-DES042` is one project's reference and each plot carries its own
neighbourhood on its sheet.

`PRX_Plot_NH` is one of the four plot parameters on the SHEET, all of them holding values, and
`CLAUDE.md` already records that it is THE SAME ON EVERY SHEET of the measured model. That does
not change the rule: the value comes off the plot's own sheet, so a model holding several
neighbourhoods writes each plot's own with nothing here changed.

**A PLOT WITH NO PRX_Plot_NH WRITES NOTHING THERE AND IS NAMED**, and the box is EMPTIED rather
than left. Leaving it would hand the client another project's reference under this plot's name,
which is the same fault the round before this one fixed one field along. `PdfChecklist.NoPlotNh`
is that reason and it is on the record per plot, beside every other emptied field.

**A form holding no contract reference field at all is a different fact and is named apart**,
through `PdfChecklist.NoContractReferenceField`, among the blanks rather than among the emptied.

**EVERY FIELD IS IN ONE OF THREE STATES NOW, NOT TWO.** Written, emptied, or left as the template
has it, and the third holds the four tick boxes, the Reset button and these two header values. The
test walks every field of a built form and refuses any field in none of the three.

### The canopy covers every row this run put a count into

**Measured on the 18:15 run: Total Green cover was the planting plus the lawn and nothing else, on
every plot of 150.**

```
ANH-007-MO-100001   PDF 0.000105 km2, which is 105 m2.  Its workbook holds shrubs 70 and lawn 35.
                    12 proposed trees, canopy absent.
ANH-007-NP-100001   PDF 0.000849 km2, and that same PDF's Lawn field reads 849.
                    55 existing and 38 proposed trees, canopy absent.
```

The canopy percentage read 0 on both park PDFs, which is the same fault one step downstream, and
it comes right with it.

**BOTH CANDIDATES WERE CHECKED AND THE FIRST IS WHAT DID IT.** The rows it read were not the rows
it wrote: `CanopyArea.From` took only matches with `Added` true, which is a species WRITTEN INTO AN
EMPTY ROW, so every row the client's list already held was left out whatever its diameter said. On
a plot whose species all match, that is every row.

**The second candidate is real on the same rows and is fixed with it.** The tool writes a diameter
only into a row it creates. A matched row is the client's own row: its measures are already in the
file and the formulas beside it already read them, so its canopy comes off the workbook's own
diameter, which travels on the match as `WorkbookDiameter`, read off the sheet's own diameter
column where the match was made. **Nothing looks the row up a second time**, so the two cannot come
apart.

### The header says WHICH read produced its count

**A COUNT BESIDE A MODEL NAME IS A CLAIM THAT THE MODEL WAS READ.** Measured at 18:06:58: the
header read `108733 elements in 0.8 seconds` while the status line still read
`Reading the plots from this model`. At 18:09:37 the same model read in 46.2 seconds.

**Two different reads set that line and it printed them the same way.** The plots press counts the
elements and reads the sheets and the schedules the plot list comes off, in under a second. The
scan inside Create reads the whole document into nine sections, in 46.2. Neither number was wrong
and the line made the first of them a claim about the model.

`ReadOfTheModel` carries which read produced it now, `ThePlots` or `TheWholeModel`, and the line
reads `Counted at ...` with a sentence saying the model itself was not read, or `Read at ...` for
the scan.

**Every path that can start a read, and every path that can set the count, one by one:**

```
Ask(WhichModel), on every draw of the pane    the document TITLE alone           FREE, already right
Ask(Plots), the Read this model button        the plots and the element count    A PRESS, already right
Ask(Create), the Create button                the scan and the plot reads        A PRESS, already right
Found(facts), after the Plots answer          SETS the count, as the plots read  now says which read
Scanned(scan), after Create's own scan        SETS the count, as the whole model now says which read
a document closing under the pane             CLEARS the count to NotYet         already right
```

**Nothing asks for a read without a press**, and the fifty seventh pass's fix holds: `Ask(Plots)`
is in the Read button's own handler and nowhere else.

**And the status line says the read landed.** It used to keep saying it was reading, which is why
the header carried a finished count beside a line still claiming to work. A status line that never
stops saying it is working is the same fault as one that never starts.

### Every plot gets its own block

**Measured on the 18:15 report: the detail blocks covered exactly seven plots of 156**, EP-01,
FP-16, HF-01, DM-11, PL-17, SC-03 and MM-01, the first of each template, because `WriteAll` took
`FirstOrDefault` of each template's runs. The species list, the schedule print, the cells written
and the reconciliation, which are the sections that make this tool checkable at all, covered 4
percent of the run.

**A block per plot was chosen over one set of blocks carrying every plot, and the reason is that
no count has to move.** A block IS one plot, so its own reconciliation reads one of one and stays
true, and the counts OF THE RUN are already above every block, in THIS RUN AT A GLANCE and in the
run accounting, counted over every run of the press. Nothing is re-counted and nothing is stated
twice.

The file preamble is printed once at the top rather than once per block, because the document, the
time and the read only line said 156 times say nothing.

### Only the formulas this run is answerable for, said once per shape

**Measured on the 18:15 report: 10,708 lines, of which WHAT THE WORKBOOK WILL COMPUTE FROM THIS
was 5,180, under a heading reading (0) over a body of 526.** Most of the 526 were G31 to G36
repeating one sentence per row per template about cells the run never wrote into, and every one of
those lines said so itself, `(not from a row this run wrote into)`.

Three rules, all in `FormulaRepeats` rather than at the printer, so the count above the section and
the lines in it come off one list:

- **A FORMULA THE RUN DID NOT AFFECT IS NOT AT RISK FROM THE RUN.** Only the ones reading a cell on
  a row this run wrote into are reported. The section already knew which those were and printed the
  number
- **THE HEADING COUNT AND THE BODY AGREE.** A heading of 0 above 526 lines is a section nobody can
  trust, and a real risk in it would be invisible
- **ONE FORMULA FILLED DOWN A COLUMN IS ONE FINDING**, with its cells listed, as a span where they
  run down one column with no gap and as a list otherwise, so no line claims to cover a cell it
  does not. The grouping is on the shape, every digit run replaced, and the printed line carries
  the real cells, so the normalisation decides what groups and hides nothing

**A #DIV/0! on a cell this run did not write is no longer in this section and is still counted and
still named**, in THE DIVISIONS at the top of the press, which counts every division off the
unfiltered list and says which cell each divides by. The detail section reports what the run is
answerable for and the glance reports what the workbook will do.

**WHAT THE FILE COMES TO IS UNKNOWN UNTIL THE NEXT RUN.** The split of the 526 between written row
and not is in neither report file, so the new line count cannot be worked out from what was
measured, and a number reasoned out here would be a guess. What is determined: the at risk body
carries only written row findings, one line per shape rather than one per row, and the detail that
was missing for 149 plots is now in the file. The next run's own heading counts answer it.

### BOTH PARKS WROTE NEITHER COMPUTED NUMBER, AND IT IS NEITHER OF THE TWO CELL CHECKS

**Measured on the 19:52 run**: MOSQUES, PARKING, SCHOOLS and STREETS wrote both numbers and
EXISTING PARKS and FUTURE PARKS wrote neither. ANH-007-NP-100001 read 0.000849 on the 18:15 run
and is empty on this one.

**THE TWO CHECKS THE ROUND BEFORE ADDED WERE BOTH ASKED AND NEITHER REFUSES ON THE PARKS SHAPE.**
Measured by building the parks row layout as its own fixture, the green cover label at C9 and
`D9 = F9+F11+H11` with the canopy at F9 and the map's planting and lawn at F11 and H11, and no
percentage label anywhere:

```
GreenCoverCell    Agrees, cell D9, formula F9+F11+H11, canopy cell F9
PercentageCell    NothingToCheck, and Usable
```

**So `SummaryCellCheck.NothingToCheck` DOES tell an absence from a drift**, which is the thing
this round was asked to check, and the test that says so is
`TheParksShapeIsFoundAndThePercentageHasNothingToCheck`.

**AND NEITHER OF THEM COULD HAVE BLANKED BOTH FIELDS ANYWAY.** Read off `PdfFill` line by line,
there are three gates and only one of them reaches both:

```
WhyNothingCanBeComputed    BOTH        the workbook was not written, or the canopy check is not usable
GreenCoverCell.Usable      Greened     only
PercentageCell.Usable      Percentage  only
```

`GreenCover.Total` always computes and `GreenCover.Percentage` refuses only on an area of nought,
so nothing below those gates can blank both either.

**SO IT IS `WhyNothingCanBeComputed`, WHICH IS TWO THINGS, AND WHICH OF THE TWO CANNOT BE
DETERMINED FROM THIS REPOSITORY.** Either the parks workbooks were not written at all, or the
canopy guard found a drift. **The 19:52 report already says which**, because every blanked field
carries its reason per plot, and the reason was not read off it. That is what the glance line
below exists to end.

**WHAT CHANGED UNDER THE PARKS IS THE CANOPY GUARD'S SUBJECT RATHER THAN ITS RULE.** Until the
eightieth pass `CanopyArea.From` returned only rows this tool WROTE, so on a plot whose species all
matched it returned none and the guard answered `NothingToCheck`. It returns the matched rows now,
so the guard reads **the client's own rows** for the first time, and it holds every one of them
against the formula text measured on one row of one template, MOSQUES Tree List - Proposed row 21.

**THE EXACT TEXT RULE IS KEPT AND IS NOT LOOSENED.** It was tempting to accept any formula that
reads the row's own diameter cell, the way the green cover cell is checked, and that would be
wrong: a sum of three cells is the same sum however it is written, and a canopy formula that reads
the diameter and computes it differently gives a different number from the one this tool works
out. **A number that is not the workbook's own is the thing this guard exists to stop.**

**WHAT IS ADDED IS THE RECORD OF WHOSE ROW DRIFTED.** `CanopyRow.TheClientsRow` travels from
`match.Added`, and the guard's line now reads `Tree List - Proposed row 7, a row this run wrote in`
or `a row the client's list already held`. **The two need different answers**: the first is the
tool writing a row the workbook cannot compute, which is the #VALUE! fault that took four rounds
to kill, and the second is the client's file computing its canopy another way, which is a question
for the team. One sentence covered both.

**WHAT THE PARKS TEMPLATES' OWN CANOPY FORMULA READS WAS UNKNOWN HERE**, because no client
workbook is in this repository, and the next run answered it off the guard's own printed formulas.
**It is not the parks templates and it is not a formula at all: it is three rows carrying none**,
under the section two below.

### THE TWO COMPUTED NUMBERS ARE COUNTED AT THE TOP, AND THE REASON IS SAID ONCE

The reason both parks fields were blank was in the 19:52 file, once per plot, among 82,048 lines.
`RunAtAGlance.Computed` counts it: how many forms wrote each of the two and, for the ones that did
not, **one line per REASON with its count and its plots named**, never one line per plot.

**THE TWO ARE COUNTED TOGETHER BECAUSE THEY FAIL TOGETHER.** One guard blanks both, so a glance
showing one of them would have read as a single field's problem.

**A FIELD THE FORM DOES NOT ASK FOR IS COUNTED NEITHER WAY.** Only the Parks form names the
percentage, so counting the other two forms' plots as not having written it would read as a
failure on every plot the other two forms cover. A form is in a field's count when the run recorded that field on it, written or
blank, and nothing in the counter holds a list of which form asks for what.

**PAST FOUR PLOTS THE COUNT STANDS FOR THE REST**, `BlankedFor.Named`, which is the number
`CreateWords.TemplateRow` already names outright, because a reason that fires on 78 street plots
has to stay one line.

### ROWS 85, 92 AND 99 CARRY NO CANOPY FORMULA, AND IT IS THE CLIENT'S FILE

**THE 05:49 RUN ANSWERED IT OUTRIGHT AND THE ANSWER IS NOT A FAULT IN THE TOOL.** The glance
line the round before added read **52 of 150 forms got no Total Green cover**, and the reason it
counted, in the guard's own words:

```
Tree List - Existing row 85, a row the client's list already held: no cell on it carries
IF(ISBLANK(J85)," ",ROUND(PI()*(J85/2)^2,0)). The row holds
C85 = C84+0.01, O85 = IF(ISBLANK(B85)," ",N85*B85)
```

**Rows 85, 92 and 99 carry the numbering column and nothing else.** Somebody added rows to the
client's tree lists and copied C without the canopy pair beside it.

**SO A TREE ON ONE OF THOSE ROWS CONTRIBUTES NO CANOPY IN THE CLIENT'S OWN WORKBOOK, WHOEVER
FILLS IT.** The sheet computes its canopy off a formula those three rows do not carry, so a count
written into one of them reaches the tree total and reaches the canopy through nothing at all.
That is true of a person typing into the file by hand and it is true of this tool.

**REFUSING WAS RIGHT AND THE EXACT TEXT RULE STAYS.** Writing a number the workbook will not
compute is what the guard exists to stop, and a green cover that silently left three rows out
would be the same silent wrong number one step further along. **NOTHING IN THE TOOL CHANGES for
this**, which is Bader's decision of 15 September, and he is taking the rows to the client.

**THE ROW'S OWN PRINTED FORMULAS ARE THE MEASUREMENT AND THEY SAY ONE MORE THING.**
`O85 = IF(ISBLANK(B85)," ",N85*B85)` is the shape MOSQUES row 21 carries at
`M21 = IF(ISBLANK(B21)," ",L21*B21)`, two columns to the right, so on that sheet the canopy per
tree sits at N and the area at O rather than at L and M. **The guard never cared**, because it
looks for its text on ANY cell of the row and the only letter in that text is the DIAMETER
column, which comes off the sheet's own heading row. Whether the whole sheet uses N and O or only
the added rows do is UNKNOWN from one row, and it costs nothing either way.

**THIS CLOSES THE OPEN QUESTION THE ROUND BEFORE LEFT.** It was answerable only off a run, the
run answered it in one line at the top of the report rather than in 57,143 lines, and that line
is what the glance was built for.

### THE LIST HOLDS PLOTS NOTHING IN THE REPORT EVER MENTIONED, AND THEY ARE IN THE MODEL

**Measured on the 05:49 run, NG05: 165 plots offered, 156 ticked, a 57,143 line report.** The pane
showed MM-08 ticked with MM-09 to MM-15 unticked under it and NS-01 ticked below them, and **MM-09
to MM-15 appear NOWHERE in the report.** The only MM plots named anywhere in it are MM-01 to MM-08.
STREETS still wrote all 78 of its plots and PLOTS TICKED THAT WENT INTO NO WORKBOOK read 0.

**THE ROUND MESSAGE READ THAT AS: THEY ARE IN THE LIST AND NOT IN THE MODEL. THEY ARE IN THE
MODEL.** Three places were named to check and all three were checked, the third first.

#### THE THIRD IS RULED OUT: NOTHING DERIVES A PLOT NAME

There is **ONE production construction of the KPI plot list from a document**,
`KpiPlotReader.Plots` at `PlotsInTheModel.Of(OnSheets(document), OnSchedules(document))`. Every
other `PlotsInTheModel.Of` in the tool is an empty fallback taking two nulls. `OnSheets` adds
`ParameterReading.Printed` of `PRX_Plot_ID` off each sheet. `OnSchedules` adds
`filter.GetStringValue()` off the schedule filter whose field is `PRX_Ref Plot ID`. **The only
transformation applied to a plot string anywhere in that chain is `Trim()`, which can shorten and
never build.**

**EVERY PLOT IDENTIFIER BUILDER IN THE REPOSITORY IS DRAWING SHEET'S AND NO KPI FILE NAMES ONE.**
`PlotTickList.Generated` really does build `prefix + "-" + number.ToString("00")` in a loop over a
range, which is exactly the shape a run of MM numbers ending at 15 looks like, and it is reached
only from `DrawingSheetPanel`. Checked by name: **no file under `Core/Kpi` or `Revit/Kpi` mentions
`PlotTickList`, `PlotRange`, `PlotRegistry` or `PlotSelection` at all**, and the only `PadLeft` and
`ToString` with a format in those folders are a report column width, a report row number and a PDF
cross reference offset.

**THE PLOT NAMED `-` IS A READING TOO.** It is a schedule whose `PRX_Ref Plot ID` filter holds a
single dash, read verbatim. Nothing validates a plot against the two letters, dash, digits shape on
the way into this list, so whatever the filter holds becomes a plot. That is a fact about the model
rather than a name the tool made up.

#### THE FIRST TWO ARE BOTH TRUE AT ONCE, AND THAT IS WHY NEITHER LINE NAMES THEM

**THE TWO DISAGREEMENT LINES COUNT THE PLOTS NAMED BY EXACTLY ONE SOURCE.** Written out:

```
All              = sheets UNION schedules
OnSheetsOnly     = sheets MINUS schedules    the line "On a sheet and on no schedule"
OnSchedulesOnly  = schedules MINUS sheets    the line "On a schedule and on no sheet"
in NEITHER line  = sheets INTERSECT schedules
```

**So a plot named by BOTH is in neither line, and on that run the pane said nothing whatever about
156 of its 165 plots.** MM-09 to MM-15 are in neither line, which places them in the intersection:
**they are on a sheet AND on a schedule.** Both of the first two places named in the round message
are true of them, and being in both is exactly what made them invisible.

**THE TWO NINES ARE DIFFERENT NINES.** Nine plots were named across the two lines and nine plots
were unticked, which looks like an arithmetic that closes. Being named by one source is a fact
about which of two READS found a plot. Being unticked is a fact about whether `PlotsPerTemplate.For`
PLACED it in a ticked template. Nothing has ever made those two agree, and the run itself proves
they do not: seven of the nine unticked are MM-09 to MM-15, which are in neither line, and the park
plots the lines DO name are placed by their prefix and are ticked. **Those two lines have been on
the pane since September and nobody had held them against anything.**

#### AN UNTICKED PLOT APPEARS IN NO SECTION OF THE REPORT, BY CONSTRUCTION

Every section of the create report is over the TICKED plots. The per plot blocks, the PDF rows and
the glance run off `PlotOutcomes`, which is one per ticked plot. **THE SPLIT and PLOTS TICKED THAT
WENT INTO NO WORKBOOK run off `TemplateSplit`, which is built from the ticked list alone**, so
`Unplaced` cannot hold a plot nobody ticked and its heading carries the word TICKED for that
reason. **Reading 0 there is correct and says nothing at all about an unticked plot.** And
`KpiCreateRunSet` did not carry the model's plot list at all, so the report had nothing to print
one from.

**SO THE SILENCE WAS NOT EVIDENCE.** A plot in the list and not ticked produces exactly the file
that was measured, and so would a plot the model did not hold, and nothing in the file separates
them. That is what the section below is for.

#### WHY THEY ARE UNTICKED, AND WHICH OF THE ROUTES IT IS

Ticking a template row keeps a plot only where `PlotsPerTemplate.For(plot, component).Template` is
that row's template. Six routes leave a plot unticked with every row ticked:

```
1  its component is not one of the eleven ComponentTemplates holds, and the prefix may not
   answer in its place, so NEITHER places it
2  its component and its prefix name different templates, so NEITHER places it
3  no component and a prefix the table does not hold, which is the plot named "-"
4  held off by hand, the pane's own HandTicks
5  the ticking call did not run for that row, a file caught between the two park templates
6  its template has no recognised file in the templates folder, so it has no row to tick
```

**MM IS A STREETS PREFIX AND STREETS WROTE ITS 78 PLOTS, SO ITS ROW WAS TICKED AND SETTLED.** That
rules out 3, 5 and 6 and leaves **1, 2 and 4**. Routes 1 and 2 need a component on the plot's sheet
and route 4 needs nothing at all, so **being on a sheet does not narrow it**: a plot whose sheet
holds no component falls to the prefix and is ticked. **WHICH OF THE THREE IT IS CANNOT BE
DETERMINED FROM THIS REPOSITORY** and is not guessed at here.

**AND THE COMPONENT EVERY TICK RESTS ON IS READ OFF ONE SHEET.** `KpiPlotReader.ValuePerPlot` takes
the plot's FIRST sheet by sheet number and no other, so a plot whose sheets disagree about
`PRX_Component`, or whose first sheet holds none, is placed off that one sheet's value with nothing
recorded about the rest. Nobody has measured whether any plot's sheets disagree.

#### EVERY PLOT THE TOOL OFFERED, AT THE TOP OF THE REPORT

`PlotOrigins` is the section. One row per plot, **the plot, which of the two reads named it, and
whether it was ticked**, over the plots the model names AND the plots that were ticked, so a plot
in one and not the other is a row rather than a gap. It is the FIRST section of the body, above the
glance, because it is the list every other number in the file is a subset of.

**THE PLOT LIST IS READ OFF THE LIVE DOCUMENT AT THE PRESS**, the same read the plots button makes,
and the ticked half is counted off `PlotOutcomes`, which is what really happened. A plot that was
ticked and that the read at the press does not name is its own row reading NAMED BY NEITHER SOURCE,
and the line says it is a bug rather than a state, because the list is the union of the two reads
and no plot read off the model can land there.

**AND THE COUNTS ADD UP IN A WAY THE PANE'S TWO LINES NEVER COULD.** Four source states and two
tick states, each adding to the number of rows, printed as YES or as a bug in the tool. The section
also prints, in one sentence, what the pane's two lines count, so the next person holding nine
against nine reads why they are different questions before they start.

**WHAT IT STILL DOES NOT SAY IS WHY A PLOT WAS NOT TICKED.** That needs the component per plot
beside the plot list, and the component the pane holds was read at a different moment from the list
this section reads at the press, so printing a route off the two together would be a route worked
out from two records of one fact. **The two travelling from ONE read is a round of its own** and is
in `steps/log-kpi.md` as the next line.

### THE HAND CHOICES ARE ONE RECORD NOW, AND NOTHING MOVES THE TICKS WITHOUT IT

**THREE ROUTES HAD COME APART, AND THEY ARE ONE FAULT: nothing kept the held off list in step
with the real ticks, and nothing ever printed it.** All three were found by the verifiers of the
eighty second pass rather than by a run, which is why none of them had a measurement behind it.

**IT WAS A BARE SET OF THE PLOTS TAKEN OFF.** `HandTicks` replaces it, holding BOTH directions, a
plot taken off or put on by hand and never both, immutable the way `PlotTicks` already is.

```
a hand untick of one plot      TakenOff    and it drops any standing put on for that plot
a hand tick of one plot        PutOn       and it drops any standing taken off
ticking a template row         Ticked      skips the plots taken off by hand
unticking a template row       Unticked    KEEPS the plots put on by hand
Select all                     Forgotten   every hand choice goes
Clear                          Forgotten   every hand choice goes
the model changes              Forgotten   another model's DM-14 is not this one's
```

**1. UNTICKING A ROW WAS NOT SYMMETRIC WITH TICKING IT.** `Ticked` took the held off list and
`Unticked` took nothing, so a hand UNTICK survived a row tick and a hand TICK did not survive a
row untick. A person who picked three mosque plots by hand, then ticked the MOSQUES row for the
rest, then changed their mind about the row, lost their three with nothing said. **Unticking a
row undoes exactly what ticking it did now**, and ticking the row again restores the same state,
which is what symmetric means here and what a test writes out in both directions at once.

**2. SELECT ALL AND CLEAR BOTH FORGET EVERY HAND CHOICE.** Bader proposed it for Select all, on
the ground that pressing it is a person saying they want everything, **and the same argument
carries Clear, which is the half that was not asked about.** Both replace every tick, so a per
plot choice left standing behind either is a record that disagrees with what is on screen, and
the next row press acts on the disagreement: a plot unticked by hand once and brought back by
Select all was dropped again the moment any template row was ticked. **A record that disagrees
with the screen is worse than no record**, because the pane looks right.

**AND ONE PLACE SETS BOTH.** `KpiPanel.TickedByHand` moves the ticks and the record together and
nothing sets one without the other, which is the whole of the fault stated as a rule.

**3. THE LINE THAT WOULD HAVE ANSWERED ALL OF IT WAS BUILT AND SHOWN NOWHERE.**
`TickingATemplate.SomeOfThem` returns
`MOSQUES: 2 of the 3 plots that belong to it, the rest unticked by hand.` and its only references
were two lines of its own test file. **Green, and it had never reached a screen.** It is on the
workbook row now, under the tick box, as a NOTE rather than a refusal, and a row where every plot
is going in gets no line at all because a line about nothing is one the team reads past on every
other press.

**THE PANE COUNTS NOTHING.** `TickingATemplate.RowLine` takes the template, the ticks and the
component reader and hands back the sentence, so the two counts come off the split's own rule
rather than off a loop beside the control that draws them.

**IT IS THE LINE THE EIGHTY SECOND PASS SPENT A ROUND LOOKING FOR.** That round asked a 57,143
line report why MM-09 to MM-15 were unticked. This answers the held off half of that question on
the row a person is looking at when they press, and it has existed the whole time.

### SIX MEMBERS IN Core/Kpi HAVE NO CALLER IN src, AND THREE OF THEM BUILD A LINE

**Counted rather than guessed at.** Every public member of `Core/Kpi` returning a string or a list
of strings was held against every reference in `src` outside its own declaration, doc comments
left out. **245 such members, and SIX have no reference at all.** Every one of the six is tested
green, which is exactly the shape `SomeOfThem` had.

```
KpiPaneWords.ModelNamed        a line       the model's name or the words for none
CreateWords.SuggestedName      a line       the name a box offered, and THE BOX IS DELETED
RegionChoice.WhyUnchosen       a line       why no region was chosen, for the report's own column
ComponentTemplates.ValuesFor   a list       its docstring says the report and the pane say it
PlotPrefixes.PrefixesFor       a list       DELIBERATELY KEPT and already recorded as uncalled
WorkbookPatcher.ReadBack       not a line   a cell read off a written file
```

**`WhyUnchosen` is the one worth acting on and it is not acted on this round.** Its own docstring
says it exists "for a report that would otherwise print an empty cell and leave somebody guessing
which of the two cases it was", and the report never calls it, so **the report prints the empty
cell.** That is named here and in `steps/log-kpi.md` rather than fixed, because it was not what
this round was asked for.

**`SuggestedName` is the name box's, and the box is gone.** The shape it served no longer exists,
which is the second of the two reasons this repo deletes a thing, and it is named rather than
deleted for the same reason.

### A SECTION THAT COVERS WHAT WENT IN CANNOT TELL YOU WHAT DID NOT

**Recorded beside `PlotOrigins` because the next person reading a report will make the same
mistake.** The eighty second pass began from a premise that was wrong, and the way it was wrong is
worth more than the answer.

**The premise:** MM-09 to MM-15 are in the list and not in the model. **They are in the model**, on
a sheet AND on a schedule, which is why they are in neither disagreement line.

**The evidence was that they appear nowhere in a 57,143 line report. That is not evidence.** Every
section of that report is over the TICKED plots, and those seven were not ticked. **PLOTS TICKED
THAT WENT INTO NO WORKBOOK reading 0 is correct by construction**, because `Unplaced` is built
from the ticked list alone, and it was read as a fact about the model.

**And the two nines were a coincidence.** Nine named across the two disagreement lines, nine
plots unticked, and they are different nines: one is about which of two reads found a plot and the
other about whether the split placed it.

### WHAT IS IN THIS FILE, COUNTED OFF THE FILE

**Measured on the two runs**: the 18:15 report was 10,708 lines over seven plot blocks and the
19:52 report was 82,048 over 156, of which `WHAT THE WORKBOOK WILL COMPUTE FROM THIS` was 42,570,
52 percent of the file and 273 lines for each plot.

**ALL THREE OF THE RULES THE ROUND BEFORE ASKED FOR LANDED**, and the section still grew, because
they governed the FORMULAS AT RISK list and the section's other two lists were untouched while
the per plot fix multiplied every one of them by 156. The one that carried it is
`FORMULAS READING A ROW THIS RUN WROTE INTO`, 321 lines on a single plot of the 18:15 run.

**SO THE SAME RULE GOVERNS THAT LIST NOW.** `FormulaRepeats.Reading` groups it on the sheet, the
formula with every digit run replaced and the cells it reads with the same replacement, so one
formula filled down a column is one line carrying its span and its count. Two things are
deliberately no part of the shape. **A cell named twice in one formula is one read**, because
`IF(ISBLANK(J4)," ",ROUND(PI()*(J4/2)^2,0))` names J4 twice and the line printed it twice. **A
shared formula and its master are one shape**, because that is how Excel stores a column rather
than anything about the formula, and keying on it split every column into two lines.

**AND THE FILE OPENS WITH WHAT IS IN IT.** `ReportSections.Of` reads the report's own headings and
counts the lines under each, and `WHAT IS IN THIS FILE` prints one row per section, widest first:
the lines, how many blocks it is spread over and what one block costs. **It is the instrument and
not the cut.** It decides nothing, leaves nothing out, names no section, and a section added or
renamed appears in it with no change on that side. The counts add up to the body's own line count,
the opening above the first heading counted as its own row so the parts equal the whole, and the
contents block sits above the body and says it is not in its own counts.

**THE NEXT CUT IS MADE ON THOSE NUMBERS.** What the 19:52 file's other sections came to is UNKNOWN
and is not estimated here: the split was in no report file, and a number reasoned out would be a
guess. The next run prints it.

### The units of the fields nobody fills, on record

**A FIELD WITH NO NOTE IS NOT FILLED** and that is unchanged. These are recorded for the day the
client annotates one, so nobody has to measure the page again, and they are read off the page by
position like every unit above.

```
Roads   Sidewalk, Median, Sidemedian, Empty areas   NO UNIT PRINTED AT ALL
        Water tanks                                 nr

Parks   the two people counts                       nr
        Cycling paths, Pedestrian paths             lm
        Toilets, Kiosk, Play grounds, Fitness area,
        Muga sport field, Running track             Area (m2)
        Water tanks                                 nr

Open    Length, Cycling paths, Pedestrian paths,
        Maintenance road                            km
        Toilets, Kiosk, Exhibition spaces,
        parking provided                            nr
        Seating areas, play areas                   m2
        Bridges, Catwalk                            Lenght (m)
        Water tanks                                 nr
```

**CYCLING PATHS AND PEDESTRIAN PATHS ARE lm ON THE PARKS FORM AND km ON THE OPEN SPACES FORM.** The
same row name, two units, on two forms this one tool fills. Nothing writes them today so it costs
nothing now, and it would cost a thousandfold error in a client document the day somebody adds a
note to one of them and reads the other form's unit. **The Bridges and Catwalk row spells Lenght**,
which is the client's spelling and is written here as it is, the same rule the model's
PRX_Furniture Lenght already follows.

## Matching a species is plain or it is nothing

The workbook's own column D is the only species list there is and `SpeciesList` reads it out of
the template. Nothing in this repo carries a copy of the plant palette. Matching is the
botanical name compared without case and with surrounding whitespace off, and nothing else.

Three measured cases are why nothing is stripped, split or normalised past that. The model
prints a species called UNKNOWN, and the 2026-09-09 check found four rows all named Unknown
Tree, so nothing could match those on name. The 1552 workbook holds ONE, at row 101, and the
alias below is how it is reached. Four rows is what that alias refuses on.
ACACIA / VACHELLIA FARNESIANA carries a slash.
BOUGAINVILLEA GLABRA 'PINK PIXIE' carries an apostrophe, and the shrub rows are prefixed
SHRUBS: and GRASS: where the workbook's list is not.

**A species Revit holds that the list does not is WRITTEN IN and named in the report.** It goes
into the first empty row below the list on the sheet its group points at, the botanical name in
column D and the count in column B and nothing anywhere else. This reverses the rule that it was
named and written nowhere: a quantity that goes nowhere leaves a tree list that reads as complete
and is short, and DM-12 came out reading 31 trees where the model holds 39.

**Unless the model prints no canopy diameter for it, and then it gets no row at all.** A row
written into an empty one carries only what the model prints, and the diameter is the one
measure the sheet computes from. Measured on the MOSQUES template, Tree List - Proposed row 21:
L21 reads J21, the diameter, M21 reads L21 and the count, O21 reads N21, which is typed and
never written, and nothing reads I21, the height, or K21. Measured on the 1836 run over 20
mosque plots: 34 cells were ready, UNKNOWN went into Tree List - Proposed row 85 with its name
and its count, DM-25 row 19 prints 0 for its canopy diameter and a nought is no size, the
formula check found seven formulas that would read an error, and the workbook was deleted.
**The guard was right and the rule it caught was wrong.** `SpeciesMatching.WrittenInto` asks
`MeasureAnswer.Write` of the canopy diameter before a row is taken, and a species with no
usable one gets `NotSized`, which quotes what every row printed. **A species that cannot be
sized is not a refusal.** It is one line in the report, its count among the trees not written,
and a workbook that computes.

**The height is not load bearing.** The rule's first round required both measures off the round
message's wording, so a species with a diameter and no height took no row for a cell no formula
reads. It is written now: the name, the count and the diameter go in, the height goes in when
the model prints one, and when it does not the height cell is named as not written with what
the rows printed, through the same skip every other measure cell already uses.

Three things hold it up. **The diameter is asked before a row is taken**, so a refused species
leaves the empty row for the next one rather than using it up. **The sheet's own total is asked
first**, because a sheet with no total writes nothing for anybody and that is the larger fact.
And **the diameter alone decides**, off the row 21 measurement above.

**THE NO DIAMETER RULE IS THE EMPTY ROW ROUTE'S AND NOTHING ELSE'S. A MATCHED ROW IS THE
CLIENT'S ROW AND ITS OWN CELLS DECIDE.** Writing into an empty row means writing a name where
the workbook has none, so what the sheet computes from has to come with it or the row breaks
the canopy maths. A row the workbook already holds is the client's own: its measures are
already in it, the formulas beside it already read them, and the only thing Revit is adding is
the count in column B. So `SpeciesMatching.Against` asks the diameter on the `WrittenInto`
route alone, and `NotSized` lives only there. Measured on the MOSQUES Existing list row 101:
D101 reads Unknown Tree, I101 0, J101 0 as a plain value rather than a blank, K101 0, L101 0 as
a plain value rather than the `IF(ISBLANK(J))` formula the written rows carry, and M101 is
empty. Writing B101 gives that row a canopy of nought and breaks nothing. **This was already
true at today's lines and is pinned by a test now**, because it was named as one of the two
reasons UNKNOWN went unwritten on the 1552 run and it was not one of them.

**A NAME PAST THE LIST'S FIRST EMPTY ROW IS REFUSED EVEN WHEN IT MATCHES WORD FOR WORD.**
`SpeciesList` reads the names down column D as far as the first empty row and holds anything
past that gap in `BelowTheList`, and `SpeciesMatching.Against` refuses a species whose only name
sits there rather than writing it in a second time above the gap.

**IT DOES NOT BITE FOR ROW 101 AND IT WAS NOT WHAT STOPPED UNKNOWN.** Measured by Bader on the
workbook the 1552 run wrote on the NG03 model, which is not in this repository: Tree List -
Existing holds 98 names on rows 4 to 101 with NO EMPTY ROW INSIDE THE LIST and its first empty
row is 102, and Tree List - Proposed holds 80 names on rows 4 to 83 with no gap and its first
empty row is 84. Row 101 sits ABOVE the first empty row, so `BelowTheList` never reaches it.
The rule is right, it is unchanged, and the round before this one raised it as UNKNOWN from this
repository, which it no longer is.

**THE MATCH IS NOT WIDENED AND THE REPORT NAMES WHAT IT MISSED.** Matching is still the
botanical name without case and with edge whitespace off and nothing else. `SpeciesMatching.ClosestName`
finds the workbook name sharing the longest opening with the Revit name, ties broken by the
shorter name and then naturally, and empty where nothing is shared. It is carried on
`SpeciesMatch.NearestInTheList`, printed as the second column of the unmatched species list in
the create report, and **it is printed and never matched on**. It is how the next alias gets
found, and nothing in the tool may ever place a species by nearest name.

## UNKNOWN reaches Unknown Tree through an alias, which is a table and not a rule

**Bader's answer, and the whole of it is why no rule could do this job.** Measured on that same
1552 workbook: exactly one of the 98 names in Tree List - Existing opens with UNKNOWN, which is
Unknown Tree at row 101, and the Proposed list holds none. A rule reaching Unknown Tree from
UNKNOWN by a shared opening, a prefix or a longest match would work on that one name and then
reach into these, where it has to pick one:

```
Conocarpus erectus    and  Conocarpus lancifolius
Ficus benjamina       and  Ficus religiosa       and  Ficus pseudosycomorus
Prosopis juliflora    and  Prosopis glandulosa
```

**A picked genus is a silent wrong number in a client file, which is worse than a tree that
goes nowhere.** And UNKNOWN is not a species at all. It is Revit's placeholder for a tree nobody
has identified and Unknown Tree is the client's placeholder for the same thing. Two placeholders
meeting is a fact about this project, and a fact about the project is data. `SpeciesAliases` in
Core is that table, one entry today, UNKNOWN means Unknown Tree, and a second goes in only when
the team says so, the same rule the Street Design list follows.

**Three guards, all tested.**

**An alias applies only where it resolves to exactly one row on that sheet.** Two rows is a
refusal naming every one of them, because a workbook whose list held Unknown Tree twice must not
be guessed at and an alias is the tool's decision rather than a name the model printed, so it is
the one that gives way. None is nothing at all: the Proposed list holds no Unknown Tree, so an
UNKNOWN there goes down the empty row route exactly as it did before the table existed, where
the report already names it, and nothing is skipped or silent.

**An alias never overrides a real match.** A name the list holds word for word is matched above
the table and the table is not consulted at all, and a name held below the first empty row keeps
its own refusal, because that is a row the workbook really carries. `ThroughAnAlias` is reached
only where nothing in the list and nothing below it is named like the species.

**The report says an alias was used, which one and which row it reached.** SPECIES MATCHED grew
a how column reading matched on its own name or THROUGH THE ALIAS UNKNOWN means Unknown Tree,
and SPECIES MATCHED THROUGH AN ALIAS is a block of its own with the alias, the sheet, the row
and the count. **A count that arrived through an alias must never read the same as one that
matched word for word**, because the first rests on a decision of the team's.

`OnTheRow` is the one place a row becomes a match, asked by the exact name and by the alias
alike, so the total's reach is checked once and cannot drift into two rules.

**The check, for the run after this one.** UNKNOWN Existing is 16 trees on the 1552 run. Once
the alias reaches row 101, Total Trees moves from 374 to 390 and the canopy stays at 11,168,
because row 101 holds a real zero for its diameter and contributes none.

**The report says how many trees went nowhere and out of what.** One line under the species the
list does not hold: NOT WRITTEN, THE WHOLE RUN: 1 tree of 528, over every species this run
merged. A workbook one tree short and a workbook eighty five short read the same without it.
Both numbers come off the one list of matches the section above prints from.

**Every row fact comes from the file and the map holds no row range.** The first twenty plot
run measured three answers to where the MOSQUES existing list ends: the map said row 83, the
sheet's total said `SUM(B4:B92)`, and the botanical names ran to row 101, 98 of them. The tool
trusted the shortest, so CONOCARPUS LANCIFOLIUS at row 84, PHOENIX DACTYLIFERA at 86,
WASHINGTONIA ROBUSTA at 87 and FICUS BENJAMINA at 89 were reported as having nowhere to go, 66
trees with a row waiting, and the workbook went out saying 76 existing trees where the model
holds 161. `SpeciesList` reads two things off each sheet and holds them apart: every row that
names a species, read down column D from the row under the header until the first empty row,
and the rows the total reaches, read off the total's own `SUM` formula. The one number left in
the map is the header row, 3, measured on all seven templates.

**A name on a row the total does not reach is refused, with the row and the total named.** On
that same sheet rows 93 to 101 name nine species past `SUM(B4:B92)`, so PROSOPIS JULIFLORA at
row 99 is matched, not written, and named under SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES
NOT REACH with its cell in CELLS NOT WRITTEN. A count written where no total adds it leaves a
sheet that reads as complete and is short, which is worse than the gap. With no `SUM` found a
matched name and an unmatched one are both refused the same way, because nothing says which
rows a count reaches. The empty rows for a species the list does not hold are the rows the
total reaches that name nothing, worked out from those two reads and stated by nothing else. A
name below the first empty row is not the list, is named in the report, and a species carrying
it is refused rather than written in a second time above it.

**The report prints both lists as read**, under THE WORKBOOK'S OWN TREE LISTS: the names and
their rows, the total and its reach, the empty rows, and the names the total does not reach.
The MOSQUES list read 80 names in rows 4 to 83 on 2026-09-09 and 98 in rows 4 to 101 on
2026-09-10. Who added the 18 and why the total was not extended to cover the last nine is
UNKNOWN and is for the team.

**A species written in carries four things: the name, the count, the height and the canopy
diameter.** The softscape schedule prints HEIGHT (m) and DIAMETER (m), and on six species
sitting in both they read the same as the workbook's Mature Height and Average Mature Canopy
Diameter, measured on the 1428 run: ACACIA / VACHELLIA FARNESIANA 7 and 6, ALBIZIA LEBBECK 15
and 8, BAUHINIA PURPUREA 6 and 5, CASSIA GLAUCA 6 and 5, HIBISCUS TILIACEUS 6 and 5,
WASHINGTONIA ROBUSTA 25 and 5. A row written with the name and the count alone broke the
workbook's canopy maths: L84 reads `IF(ISBLANK(J84), " ", ...)` and returned a space, M84
multiplied that space by the count, and #VALUE! ran through the canopy total to the KPI row,
nine errors that survive a full recalculation. So `SoftscapeRows` reads both measures off the
columns the heading row names, `MergedSpecies` holds every row's value against the others, and
`KpiCreatePlan` writes them into the columns the sheet's own header row names, found by the
words HEIGHT and DIAMETER and never by the letters I and J. A dash, a nought or a cell that
does not read is not written and is named: UNKNOWN prints a dash and a 0. Rows off more than
one plot that disagree write nothing into that column, every value is named, and the report
says the row will not compute its canopy. Nothing is averaged and nothing is taken first.

**Two headings can hold the word, and the sheet's own formulas say which.** Measured on the
1707 run: the tree list header row holds I Mature Height (m), J Average Mature Canopy Diameter
(m) and K Mature Canopy Diameter (m). Both J and K hold DIAMETER, the reader found two, refused
to choose and wrote nothing into J, L84 returned a space off the blank, M84 went #VALUE!, and
the canopy guard deleted the output. Correct at every step, and the cause was one column choice.
J is the column the workbook computes from: L reads J and nothing reads K. So where more than
one column holds the word, `SpeciesList` reads every formula below the header row off the
sheet part, collects the columns those formulas reference, and takes the one candidate the
formulas read. Where that still leaves more than one, or none, nothing is written and both are
named with what the formulas read, and its own formulas read none of them, or both of them.
Never a position. How each column was chosen is recorded on the list, the one column of the
header row holding the word, or of J, K holding DIAMETER, the one the sheet's own formulas
read, and the report prints it beside each tree list. The check: PHOENIX DACTYLIFERA writes 18
into I and 15 into J, and UNKNOWN writes neither and still refuses through the guard.

**The matches that disagree are counted.** The 1707 run read 22 matched species whose height
or diameter in Revit differs from the row the workbook holds, nearly every match. Both numbers
stay named and nothing is changed, and one line under that heading says how many of the
matches differ in a height, a diameter or both, so the size of it is visible without counting.

**Family, genus, native and every code column stay empty.** Revit does not print them, so the
KPIs that need them still cannot see a species written this way. That is in `steps/log-kpi.md`
as an open question for the team.

**A matched species whose height or diameter in Revit differs from the client's row is named
and the row is left alone.** PHOENIX DACTYLIFERA prints 15 metres across in the model and the
MOSQUES existing list holds 8 at row 86. Two numbers for one species, and which is right is a
question for Bader, in the log, not a cell to overwrite.

More unmatched species than empty rows writes what fits, names the rest, and says plainly that
the sheet ran out of room. A species the list holds and Revit does not is left empty, which is
correct and needs no line.

## The note text can never be matched against the model

The notes name these sources:

```
REVIT SHEETS /TITLE BLOCK/PRX_COMPONENT
REVIT SHEETS /TITLE BLOCK/PRX_Plot_UID2
Project information / neighbourhood name
REVIT 00 LINK / ID FILLED REGION/ PRX_Intervention Area
SHRUBS&LAWN SCHEDULE / SHRUBS & GROUND COVER TOTAL AREA
SHRUBS & LAWN SCHEDULE / LAWN (GRASS) TOTAL AREA
SOFTSCAPE SCHEDULE / ENTER EACH EXISTING TREE QUANTITY
SOFTSCAPE SCHEDULE / ENTER EACH PROPOSED TREE QUANTITY
```

One schedule is written two ways in the same file, with and without spaces round the
ampersand, and COMPONENTS is misspelt COPONENTS in several notes. So the real names come from
the model. `KpiNames` holds the three exact parameter names the scan looks for and the words a
near miss is looked for under. A NOT FOUND against any of them is a finding, printed with the
near misses beside it, never a failure.

**PRX_Plot_UID2 is a third plot name.** `CLAUDE.md` records `PRX_Plot_ID` on views and sheets
and `PRX_Ref Plot ID` on elements. Nothing here assumes the third is either of them.

## The nine questions, and where each is answered

1. Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2. Section 3
2. Is PRX_COMPONENT on the title block instance or on the sheet itself. Section 3
3. What is PRX_Plot_UID2. Section 3
4. The real name of the neighbourhood parameter in Project Information. Section 2
5. Is there a link whose name holds 00, and what ID FILLED REGION means in it. Section 4
6. The exact schedule names in the model. Section 5
7. Which softscape field is the botanical name and which the quantity. Section 6
8. What separates existing trees from proposed ones. Section 7
9. What unit each area comes back in, raw and as printed. Sections 1 and 8

Section 9 is one line per question saying FOUND or NOT FOUND and where to look.
`KpiQuestions` builds those lines and decides nothing beyond whether the thing was found.

## The report is the deliverable

`KpiReport.Write` turns a `KpiScan` into nine numbered sections. Every heading carries its own
count, so a section that found nothing reads differently from one that was never filled in.
Above section 1 is READS THAT DID NOT HAPPEN, because a read that was refused would otherwise
print as a zero and a zero reads as an answer. Every skip in the Revit readers goes in there.

**The create report ends with what the tool read.** Everything else in it is what the tool
concluded, and a species printed on two rows survived two rounds because nothing showed the
rows. `PlotReading.PrintedSchedules` carries every schedule the plot's numbers came off, row for
row, and `KpiCreateReport` prints each last, under EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE
PRINTS IT: the name, how many rows it printed and how many are shown, which rows were read as
species rows, which as subtotals, which were group rows and which rows were left out, and where
the TOTAL row was, and for the shrubs and lawn schedule which phase row was taken and which
left out and that the group total row was checked. Every
column, padded to its widest cell, with the row numbered the way the readers number it, the
heading row being 1. Two hundred rows a schedule at most, said in those numbers. The top of the
report says the section is there.

Every parameter that is read carries both its printed form, which is what a Properties panel
shows, and its raw form, which is feet or square feet whatever the project displays. Question 9
is the difference between the two, and `AreaUnits` is the one place square feet turn into
square metres.

## A near miss whose values are all Yes or No is a switch

`KPI COMPONENT S/H` holds No on 9 of 9 title block types. Its name holds COMPONENT, so it was
offered in section 9 beside `PRX_Component` as another name the model might carry the component
under. It is a show and hide toggle and it answers nothing.

`KpiNames.EveryValueIsYesOrNo` decides it and both callers ask that one method. Section 9 leaves
such a name out of the answer and section 3 keeps it, under its own tally and its own values and
again by name in the component values block, which says why it is not counted there. **A name
left out with nothing written down reads exactly like a name nobody found.**

Two questions carry that near miss list and neither is question 8. It is questions 1 and 2,
which are the two about where the component lives.

## The component picks the template, and the mapping is a table

`PRX_Component` picks the workbook template and **its values are not template names.** No string
rule turns HEALTH into HEALTHCARE or NH STRT 20m ROW into STREETS. So the mapping is data.
`ComponentTemplates` is that table, measured on the 1548 scan, 11 distinct values over 1384
sheets, off the component values block at the end of section 3 of that report:

```
DAILY MOSQUE           MOSQUES          NH STRT LESS 20m ROW   STREETS
FRIDAY MOSQUE          MOSQUES          NH STRT 20m ROW        STREETS
SCHOOL                 SCHOOLS          STREET 30m ROW         STREETS
HEALTH                 HEALTHCARE       STREET 36m ROW         STREETS
PARKING LOT            PARKING
EXISTING PARK          EXISTING PARKS
FUTURE PARK            FUTURE PARKS
```

Four things hold it up.

**It is many to one.** Two values mean MOSQUES and four mean STREETS, and a test says so in
those numbers rather than leaving them to be read off the list. Another says every one of the
eleven resolves and another that every one of the seven templates is reached.

**EXISTING PARK and FUTURE PARK are separate values, so the model breaks the park tie.** Each
preselects its own template. That pair used to be the user's choice always, because the file
name was the only thing that could separate the two workbooks and no rule on a name could
separate the components. The table can. Recognising a WORKBOOK FILE is still the sheet name then
the file name, which is a different job and unchanged.

**The plot prefix does not decide.** STREET 36m ROW covers MM and ST plots and NS carries two
different street widths, so a rule on the prefix alone would answer three of them wrongly. It is
read, as the cross check and the grouping in the section below, and it never overrides this
table.

**A value the table does not hold preselects nothing, says so on the pane, and the user picks.**
Nothing guesses and nothing falls back to matching a word of the value against a word of the
template name. That old rule made PARKING LOT look like a park, because PARKING begins with
PARK, and answered nothing at all for the four street values.

Section 3 still ends with every distinct value the model holds, how many sheets carry each, the
template it means and every plot those sheets are for, uncapped where the rest of the section
shows twenty examples. The template column is this table read back, so a value the model grows
later prints as one the table does not hold. The plot beside each value is `PRX_Plot_ID` read
off the sheet, said in the block, and a value on sheets carrying no plot is counted and named
rather than dropped. A title block type is not a sheet, so a component name on one is named with
that reason and left out of the counts.

## The plot prefix is the second route, and it does not decide

Confirmed by the team, all seven templates and every prefix, and `PlotPrefixes` is that table:

```
STREETS   NS, ST, MM        SCHOOLS          SC
PARKING   PL                EXISTING PARKS   EP
MOSQUES   FM, DM            FUTURE PARKS     FP
                            HEALTHCARE       HF
```

It agrees with the eleven component values prefix by prefix with nothing left over on either
side, and a test written out by hand says so.

**Two records of one fact is the fault this repo has met eight times, so the two do not get equal
standing.** `PRX_Component` decides. The prefix is a cross check: where they agree the pane says
the prefix agrees, and where they disagree NEITHER decides, both are named and nothing is
preselected. A prefix the table does not hold cross checks nothing, which is different from one
that disagrees.

**What the prefix was once for was grouping**, a row of buttons beside Select all and Clear
that ticked a template's plots in one press. The workbook rows do that now, under the section
above, and they ask the split rather than the prefix. Nothing here gathers plots any more and
the seven members that did are deleted. The table is unchanged.

**It is also the only thing that can place a plot with no sheet.** The 1548 scan found four on a
schedule and on none, EP-05, EP-11, EP-12 and EP-13. No sheet means no PRX_Component. The pane
says the component could not be read and the prefix was used, rather than preselecting in
silence, and the line saying which route the answer took shows whichever way it went.

## Three places a sheet value can live

The title block instance, the title block type and the sheet itself are all read, with a full
parameter tally and up to twenty examples per wanted name each. Sheet Width turned out to be an
instance parameter that a type cannot be asked for, and a parameter that is not on the element
asked reads as a value of zero. Asking all three is cheaper than guessing once.

## A few plots of each schedule name are read in full

The real model holds six schedules per plot over 160 plots. Every schedule gets its name,
category, fields and filters. Rows as printed, the elements listed and the areas off them are
read for the first few copies per name the workbook draws from, in name order, skipping any
that list nothing, and the report says which were read. Regenerating a thousand schedules is a
read nobody waits for.

**How many is `KpiReport.PlotsReadInFull`, and it is three.** Core holds the number and the
Revit reader reads it from there, because the report states the rule in a sentence and a second
copy of the number is the fault this repo keeps meeting. One plot cannot show whether the group
headings repeat across plots or whether an Existing group ever appears, which is why it is not
one.

## The pane reaches Revit through its own door

`KpiPanel` is modeless and names no `Document`, no `Transaction` and no `ElementId`.
Everything it wants goes through `KpiRequestHandler` and its own `ExternalEvent`, never the
Drawing Sheet's, so a failure in one pane can never cost the other. Nothing leaves the
handler. Its pane has its own identifier and is registered through the same guard as the
Drawing Sheet pane, so a KPI pane that will not register leaves the ribbon working and the
KPI Checklist button saying why.

`KpiPaneWords` holds every line the pane shows. The pane formats nothing of its own.

## What the pane puts on a button is escaped

WPF reads the first underscore in a button's text as an access key marker and swallows it, so
the pane offered PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. Five names
no model holds, on the one tool that turns on exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` goes through it. Only what is drawn: nothing compares the
escaped form against anything. It has a test over every name `KpiNames` holds, because a name
this misses is a name the pane shows wrongly.

## A picker starts on the name the note asks for

`Preselected.From` takes what the model offers and the name the workbook note asks for, and
hands back that name where the model offers it and the first offered where it does not.

Position alone put PRX_Plot_ID under Reference, first of the four plot parameters, where the
note names `PRX_Plot_UID2`. It put whichever neighbourhood parameter sorted first under
Location, where the answer is `Neighborhood Name` and Neighborhood Group sorts above it.

**Nothing is ticked when the plot picker is first drawn**, which is the same rule the Drawing
Sheet's view types follow. Adding every plot in a model into one workbook is one press of Select
all away and is almost never wanted.

## The pane says what it reads, not what the note asks for

`KpiTemplates.SourceOf` used to print the workbook's note as though it were the tool's
behaviour: PRX_COMPONENT and PRX_Plot_UID2 read off the title block. Every part of that was
wrong. PRX_COMPONENT is in no model, the value is `PRX_Component` on the SHEET, and PRX_Plot_UID2
sits on 1233 title block instances holding a value on none of them.

It takes a `ChosenParameters` now and names the three parameters the pane's own pickers hold,
because those are the ones that will be read. With nothing picked yet it names the picker to
look at rather than a parameter nobody chose. A test walks every template and refuses any line
holding PRX_COMPONENT or the words title block.

## The workbook goes where the user browsed, not beside the model

**Writing beside the Revit model meant a detached model could not be used at all**, and a
detached model is what the team works on. It cost most of an afternoon. `OutputFolder` is
browsed for and remembered in `kpi-output-folder.txt` beside the installed assembly, the same way
the template folder is, through the one `RememberedFolder` both use.

`CreateWords.CannotCreate` asks whether a model is open and whether an output folder is set.
**Whether the model has been saved is asked nowhere now**, and the never saved refusal and
`TemplateWords.NoModelPath` are both gone. The no folder words are `TemplateWords.NoOutputFolder`,
the ones the output folder line already shows, rather than a second sentence.

The model's folder came off `OpenModel` with them. It decided nothing once the output folder
existed, and a value on the screen that decides nothing is how one stale string became a dead end
here already. The silent overwrite and the editable name box are unchanged.

The refusal is still decided at the moment Create is pressed: the document off the live document
on the Revit thread, and the folder read off the pointer file in the same breath, so neither can
be a copy the pane took earlier.

## The pane holds no copy of anything it can ask for

The model's folder was read once, when the pane was shown, and kept. The model was then saved to
a real folder and **Create stayed grey saying No model is open**, and a KPI Scan after the save
did not shift it. The pane was also holding the title in a second string, set by the scan and by
nothing else, so the two halves of one fact went stale on different schedules.

Four things hold the fix up.

**`OpenModel` is one record.** It carried the title and the folder together, built from one
answer, so nothing could hand `CannotCreate` the pair the wrong way round. The folder is off it
now: the workbook goes to the browsed output folder and the model's own folder decides nothing.
The rule that got it there stands and is why the title is still a record rather than a loose
string.

**Every answer from `KpiRequestHandler` carries the model state**, whatever was asked for, read
off the live document at that moment. `WhichModel` is only the request that asks for that and
nothing else.

**`RedrawTemplates` asks for it every time it draws**, and `Took` redraws only when the answer
moved, so the ask does not chase its own tail.

**Create is greyed out on what the PANE owns and nothing else**, a template picked and a plot
ticked. Whether a model is open and whether it has a folder are decided on the Revit thread
against the live document when the button is pressed, and the refusal comes back from there.

`Ask` holds one slot and `WhichModel` never takes it from anything, because the pane asks for it
on every draw.

**The plots are asked for in one place, `Took`, off the answered title.** `Ask(Plots)` lived
only in `Shown`, fired only by a visibility rise, and that one ask was lost three ways, all
before any pump this round added existed and none of them the two candidates the 13:48 screen
was first read as. It was NOT the repaint pump moving from background to render: git shows the
pump was born at render in the round that added it and never ran at background, `_facts` has
one writer reached by the priority-less `Dispatcher.Invoke` that no pump priority can gate,
and the build before the pump listed the plots with no pump at all. It was NOT the progress
line's redraws firing between the ask and the answer: those pumps run only inside a scan or a
create `Execute`, after the one request slot is already emptied, so their window cannot hold a
pending `Plots`. The two were told apart from the real cause by git history and by when each
pump can run, not by picking between them. The real cause is older than both: a pane restored
visible at startup with no document open consumed the one `Shown` ask against no document and
answered No open document, and nothing asked again when a model opened later, so a scan filled
the header while the plots block sat on open a model. A KPI Scan pressed before the plot read
returned displaced the pending `Plots` in the one slot the same way. So `Shown` asks only for
the model now, and `Took` asks for the plots once per model, guarded by the title the last ask
was for, reset when the model closes. A model opened under the pane, or one whose first ask was
lost, is read the moment any request answers with it, and the ask fires exactly once rather
than the twice a blind `Shown` ask and `Took` together would have cost.

**The plots block reads four ways, one line each, in `CreateWords.PlotsBlock`.** Open a model
is right only when no model is open. A scan that filled the header used to leave this block
reading open a model beside the model's own name, which sent the team to Revit for an hour. No
document says open one, a document whose plots have not come back says it is reading them, a
document answered with no plots says the model holds none, and a document answered with plots
hands off to `PlotSources` for the count. The not answered state is told from the no document
one by whether a document is open, read off the live document the pane already holds no copy
of, and from the answered empty state by a null plots standing for not answered rather than
answered with none.

**The one copy the pane holds is the templates folder's recognitions, and it says so.** Every
redraw opened and peeked every .xlsx in the templates folder on the interface thread, and every
tick redraws: seven zips for each of 155 ticks, 1,085 opens. `TemplateListing` holds each
file's recognition once per folder, keyed on the folder compared without case and with a
trailing separator off, cleared when the folder changes and after any press of Create that
reached the patcher with an output path in it, wrote or not, because the copy lands before the
patch and a patch that fails after it leaves the copy behind. The folder itself is still listed
on every draw so a file added or gone is seen on the next redraw
and opened once. It counts the opens and the redraws, the pane prints the count under the list
and the report prints it under WHERE EVERY VALUE CAME FROM, seven opens over 155 redraws, so
the seven per tick cannot come back unnoticed. A workbook overwritten in place under the same
name in that folder by anything other than this tool keeps its held recognition until the folder
is browsed again or Create reaches the patcher into it, which is written down here as the limit.

**The plot list keeps its place across a tick.** The list is a new viewer on every redraw and
every tick redraws, so 155 tick boxes threw themselves back to the top on every tick, the fault
the user reported on the Drawing Sheet's lists and fixed there. `Scrolling` and `Remembering`
in `KpiPanel` are that shape written in the KPI folder: the remembered offsets are read into
locals before any handler is attached, restored on the first layout pass rather than on Loaded
because an unmeasured viewer clamps any offset to zero, and noted on every scroll change.
`ScrollMemory` in Core holds the rule half, a restore wanted only when something above the top
was noted, with tests. It is a copy of another task's helper and not a call across the fence,
and whether the two panes should share one is a Shared round for Bader to call.

## The reference values follow the ticked plot

The block under the Reference picker showed DM-11's four values with DM-12 ticked, and the same
four with all 155 ticked. It was read for one plot, the first in the model's list.

`ReferenceValuesPerPlot` reads all four for every plot in one pass over the sheets, the shape
`ValuePerPlot` already used, and the pane shows the FIRST TICKED plot's **with that plot named
beside them**. Nothing ticked shows none and says so. The block exists so a person picks the
reference by looking at its value, and a value belonging to a plot they did not choose is worse
than no value at all.

## Shared and not changed

`RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile` are shared with the Drawing
Sheet and this tool changes none of them. A change one of them seems to need goes in
`steps/log-kpi.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
two added values, `HairlineAbove` and `WideLabelWidth`, because a number written in a pane file
is the fault that made the first pane black on black. The second is for the KPI pane's typed
boxes, where Prepared by came out as Prepared b running into its box at the shared 54, and it is
added rather than a widening of `LabelWidth`, which the Drawing Sheet uses in two places.

## The tool carries the map, and the map is data

The production templates the team fills carry no note saying where a value comes from,
measured at zero note cells in all seven. The annotated set that holds the mapping in green
text is not what the team fills. So `KpiTemplates` in Core is the map, one entry per template,
measured off the annotated seven cell by cell, with a completeness test. Nothing reads a
mapping out of a workbook and nothing fills on a best guess.

Recognition is the main sheet name first, which settles five of seven. The two park templates
share Park Name, so the file name breaks the tie, and a name that settles nothing puts the
pick to the user. The map names the two tree list sheets and no row on either. It carried a
last row per template until the first twenty plot run, 83 on MOSQUES, and the names on the real
sheet ran to 101, so the rows are read off the file under the species rule above.

## A workbook is copied and patched, never loaded and resaved

An .xlsx is a zip and the client's EXISTING PARKS one holds 37 parts. Loading it into an
object model and saving lost 21 of them, the embedded image, the printer settings, the
threaded comments and the array metadata among them, and the file still opened. So
`WorkbookPatcher` copies the file byte for byte and rewrites only the sheet parts that
receive values and the workbook part, through the platform's own zip and XML types with no
package dependency. It decides everything off the source first, so a refusal writes no file.
It sets recalculate on open, because every formula carries a stored result and the old blanks
would sit beside the new numbers otherwise. Every written cell is read back off the output
and reported as it landed, never as it was sent.

The untouched client file recalculates with 45 errors and a correctly filled one with 44. The
44 are the PARK PROGRAMME section failing on an empty Criteria table either way, so a filled
file showing 44 errors is correct.

**THE 44 ARE NOT A FAULT AND NOBODY SHOULD CHASE THEM.** Confirmed again on the 09:18 run: both
parks workbooks recalculate with 44 #N/A at F31 to F74, and an UNTOUCHED EXISTING PARKS template
recalculates with 45. **The one that goes away is a divide by zero on the empty area, which the
run fixed by filling it.** The 44 are the PARK PROGRAMME section failing because the Criteria
sheet's programme table is empty in the client's own file, measured on 8 September. The parks
workbooks are correct and carry a fault the client shipped.

### A divide by zero is reported now, and never refused on

**The 09:18 run wrote two workbooks recalculating with 2 #DIV/0! each**, ANH-007-SC-100004 and
ANH-007-ST-100130, neither on the main sheet and both on plots with few trees. The report named
none of them, because the formula check knew ONE shape: a formula returning text off ISBLANK and
the arithmetic on it. **Which sheet and which cells those four are cannot be worked out from
this repository**, because no client workbook is in it and none ever will be, so the tool is
taught to say it and the next run answers the question by itself.

`WorkbookFormulas` reads a division whose divisor is ONE CELL and asks what that cell holds. A
nought or a blank is #DIV/0!. A divisor that is an expression, a range or a function call is not
judged, because working out what it computes to would be evaluating the formula, and **a cell
holding a FORMULA is never judged either**: the patcher drops every cached value on the way out,
so a formula cell in the output holds no number at all and reading that absence as a nought
would call every computed divisor an error.

**THE 14:29 RUN ANSWERED IT AND THE ANSWER IS NONE.** 156 plots, 102 workbooks, and the
formula check found **no #DIV/0! anywhere in the press**, so none of them divides by a cell this
run wrote either. The two the 09:18 run left unexplained are not in this model's output and the
open question is closed by that count. The check stays, because it is what answers the same
question on the next run without anybody opening Excel.

**IT IS REPORTED AND NEVER REFUSED ON, DELIBERATELY.** A plot with no trees really has no
average, so the divide by zero is the client's own arithmetic over a real number, and deleting a
correct workbook over it is worse than printing a line. The reason says whether THIS RUN wrote
the cell being divided by, which is the half that would make it the tool's doing, and turning
that half into a refusal is a decision for Bader once a run has named them. **No client formula
is changed either way.**

**EXCEL SHOWED ZEROS WHERE THE NUMBERS WERE RIGHT.** The first real output read 0 for Total Green
cover, Canopy Area, Total Trees, Total Trees Native, Total Trees Adaptive, Total Planting Area and
Total Lawn Area, beside Planting 410, Lawn 60 and Mosques Area 3,729 which all read correctly.
The values were not wrong. They were stale cached results and Excel never recalculated. Total
Planting Area is `=F10` and F10 held 410, so a 0 there could only be a cache.

`fullCalcOnLoad="1"` was already there, so **the flag alone is not enough.** Three things
together, measured on that file: `calcId` set to 0 in `calcPr` with the flag kept, the cached
`<v>` dropped from every formula cell in every sheet leaving the `<f>` alone, 301 of them in that
file, and `xl/calcChain.xml` removed. Forcing a recalculation gave Total Green cover 1518, Canopy
1048, Total Trees 31, Planting 410, Lawn 60.

**The output is then checked the way the written cells already are.** `CacheCheck` is read back
off the file: recalculate on open, calcId cleared, no formula cell carrying a cached value, the
calc chain gone, and calcMode auto. All five, or the report says the file may open showing
stale numbers. A workbook that opens showing zeros beside correct inputs is the worst thing this
tool can produce, because it looks finished.

**calcMode is the fifth, measured on the 1428 workbook.** The four above all held and Excel
opened the file showing every written number and every formula cell blank, and Ctrl Alt F9
filled them: 528, 3258, 1127, 6 and 522. calcPr read `calcId="0" fullCalcOnLoad="1"` and no
calcMode. Excel's calculation mode is a session setting and the first workbook opened in a
session sets it, so anyone with a manual workbook open, or manual in their own options, opened
this file into a manual session. `calcMode="auto"` is set outright beside the other two, read
back, and required. The patcher also looks for every other place in the package that can hold
a calculation setting, a `sheetCalcPr` in any sheet part, a VBA project and any other attribute
on calcPr, and the report names what it found or says none was found and what it looked for.
**Nothing here can run Excel and neither can the gate**, so the five checks are over what the
file says and not over what Excel does with it, and the report says so in those words.

**The output's formulas are read for what they will compute, and a written cell nobody can
compute from is a refusal.** `WorkbookFormulas.Check` reads every formula in the output, by its
text alone and never by evaluating one. A formula holding ISBLANK on a cell that is blank and a
string literal returns that text, a formula doing arithmetic on such a cell is #VALUE!, and
every formula reading a cell in error carries it. When the chain starts on a row this run wrote
into, the output is deleted again and the run is refused naming every formula. The section WHAT
THE WORKBOOK WILL COMPUTE FROM THIS prints every formula at risk with the reason, every formula
reading a row this run wrote into with the reference it reads it through, the six cells the map
names with whether each is present and which formulas read it with what blanks among their
inputs, and every function the file stores with the `_xlfn.` prefix, by name and by cell count,
because such a cell reads #NAME? in a version of Excel that does not have the function. The
1428 workbook read #NAME? in every Meets KPI and Compliance cell, nine rows plus the Tree Class
and Planters rows, off `_xlfn.IFS`. The tool writes no formula and did not put them there, and
which version of Excel has IFS is not worked out here.

**The part count reads 37 in and 36 out and the report says which part went and why.** A count
short by one with no explanation reads as a loss. `PatchOutcome.PartsDeliberatelyRemoved` is what
keeps `KeptEveryPart` true across it.

## Nothing may write to a template, and two guards say so

The output folder is browsed for and the name box is prefilled with the template's own file
name, so pointing the one at the templates folder put the other one press from naming the
template itself. `Patched` deletes the output file before the copy, and that press removed the
client's GRP KPI Checklist. No copy, no undo, every later run of that template impossible, and
the run ended saying only that the workbook could not be written.

**Two guards, because either alone is one refactor from being bypassed.** One in
`KpiRequestHandler.Patched` before the delete, one in `WorkbookPatcher.Patch` before it opens
anything. Both call `FilePaths.Compare` and both refuse on the one sentence in
`CreateWords.WouldOverwriteTheTemplate`, so the two say one thing rather than two.

**The comparison is the absolute canonical form of each, without case, which is how Windows
compares a path**, with any trailing separator off because `GetFullPath` keeps one. A path that
cannot be resolved answers `Unreadable` and refuses the same way `Same` does, because a check
that cannot see its own subject has to refuse.

**It is textual and that is its limit.** A junction, a symbolic link, a substituted drive or an
8.3 short name reaches one file under two names that do not resolve to one string. Asking the
file system for an identity means opening both files, which is the thing being guarded against.

**The guard refuses one file, not one folder.** Writing a differently named workbook into the
templates folder is allowed and still goes through. What stops the user reaching the refusal at
all is `TemplateWords.OutputIsTheTemplateFolder`, said under the output folder line when the two
folders are one, before Create is pressed rather than after.

**A filled checklist in the templates folder is named as filled and not offered.** Recognition
is the first sheet's name and a filled MOSQUES output keeps `<Mosques>`, so the next redraw
offered MOSQUES DM-12.xlsx as a template beside the client's, and picking it would copy last
time's typing and last time's written species as the template. The tool can tell, because it is
what wrote it.

**A workbook is filled when a cell the tool writes holds something the tool would have written,
never when one cell differs from one expected string.** The first rule was E5 against the
angle bracketed `<Date>` and Bader measured two template sets that break it. The KPI CHECKLIST
R1 set holds a placeholder in every cell the team fills, `<Date>` at E5, `<Name>` at G5,
`<Position>` at H5 and `<UID>` at C5. An earlier production set holds NOTHING at E5, G5 or C5
and real values at D3 and H5, its D3 reading Future Park and its E4 KING ABDULLAH South. So a
placeholder is one set's habit rather than a rule, an empty cell is not a placeholder either,
and a clean template can hold real text in a mapped cell.

`FilledMarks` is the rule. Two cells can decide, and both are cells the tool writes: E5, where
a date reads as filled, and the template's own reference cell, C5 on all seven, where a plot
reference does. A date has at least two numbers in it and parses as one, so `<Date>`, the
annotated set's DATE OF THE DAY, a bare 10 and an empty cell are all templates. A plot
reference is one unbroken run holding a letter and a digit and no angle bracket, which both
parameters the team picks read as, DM-12 and ANH-007-MO-100019, so `<UID>`, KING ABDULLAH South
and an empty cell are all templates. The other cells the tool writes decide nothing, because a
clean template already holds real text in some of them.

**Nothing is withheld on a cell the tool has never written**, so an absent or empty cell is
never a filled file. The reason and the report both name the cell that decided and what it
held, `not offered: MOSQUES DM-12.xlsx, C5 holds ANH-007-MO-100019, which is a plot reference
the tool writes`, so a template wrongly withheld is traced in one line rather than by opening
the file. `PeekedWorkbook` reads those cells off the first sheet in the same open as the names,
with a shared string resolved to its text because an Excel re-save stores it that way, and the
pane lists a filled file greyed in the same list. Nothing is deleted or moved, and browsing to
a folder that holds one is untouched.

**BOTH MARKS ARE LIVE ON EVERY RUN, AND THE LIMIT THAT SAID OTHERWISE HAS GONE.** This file
used to say the reference cell is skipped when the ticked plots disagree on it, which a
checklist covering several plots usually does, so the typed date was often the only mark left.
**A checklist is one plot now**, since the round that made one workbook per plot, so the plots
cannot disagree, the reference cell always carries that plot's own reference, and both marks
decide on every run. Nobody recorded the improvement when it happened and the limit sat here
reading as current for six rounds. **A limit that has gone away is as misleading as one that
has not.**

**Two limits stand, both stated rather than guarded against.** A filled workbook the tool wrote
neither cell into reads as a template. And a client set that hinted the shape of a reference
rather than bracketing it, DM-00 at C5, would be withheld, because nothing separates a hint
from the thing it stands for, and neither measured set does that. The first is the safe way
round: offering a filled file costs a rerun, and withholding a real template leaves the team
unable to fill anything. The second is visible in one line, because the report prints what the
cell held. Both are for Bader.

**A CLEAN TEMPLATE WHOSE E5 IS EMPTY IS OFFERED, MEASURED RATHER THAN REASONED.** Row 5 on
both park templates holds nothing at C5, E5 or G5, so the question was whether the filled check
withholds every clean park template. It does not: `FilledMark.Reads` answers false for an empty
cell and for a cell that is not in the file at all, and `Decide` then hands back nothing. Tested
over both park templates and over MOSQUES, on real .xlsx files the tests build, through the
peek and the recognition the pane really calls. **There was no live fault on the two park
templates**, and the rule that nothing is ever withheld on a cell the tool has never written is
what made it so.

## Every run that ends with no file says why

`CreateWords.Wrote` fell to `Refused(run.Reconciliation)` whenever nothing was written, and
that answers the empty string when the accounting added up. **So a run whose accounting passed
and whose patch was refused set the status line to nothing at all**, and the pane went from
Creating to blank. The commonest cause is the output workbook still open in Excel from the run
before, which the delete answers with an IOException.

`WhyNothingWasWritten` is never empty. The accounting speaks first because it refuses before
anything is copied, then the patch's own refusal, then `NoReasonRecorded`, which says in those
words that nobody recorded one and that it is a bug. **Silence after a press reads as success**,
which is the worst thing a status line can do, and the test is that no run with `Written` false
can produce an empty line rather than one test per case.

`CouldNotBeWritten` says what to do before it says what Windows said, because the system's own
words name a process and not a thing to do.

**No client workbook enters this repository.** It is public and those files carry the Green
Riyadh KPI targets, neighbourhood names and the plant palette. `*.xlsx` is ignored and tests
build their own small workbook in the temp folder.

## What the first real scan measured

All of it from one run on RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached, 96,959 elements in 5.4
seconds, six of the nine questions answered. These are measurements, not guesses.

**The main sheet name carries angle brackets.** `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>`, `<Streets>`. The brackets are part of the name and not
placeholder notation. They were stripped when the map was written, so all seven real workbooks
came back unrecognised the first time the pane saw the client's folder. A test asserts every
entry opens with < and closes with >.

**PRX_COMPONENT does not exist in this model.** The sheet carries `PRX_Component`, capital C
only, on all 1385 sheets with 1384 values. So a near miss that is named is now also shown, with
the same columns and the same cap the exact name would have used. A near miss named and never
shown is the answer withheld.

**Four plot parameters sit on the sheet**, `PRX_Plot_ID`, `PRX_Plot_UID`, `PRX_Plot_UID2` and
`PRX_Plot_NH`, all with values, and the report prints them side by side one row per sheet
because nothing showing one of the four can say which the workbook wants. `PRX_Plot_UID2` is on
the SHEET, not the title block: on the title block it is on 1233 instances and holds a value on
none of them, and on the sheet it holds 1384 values reading like ANH-007-MO-100019, which is
nothing like a plot identifier such as DM-41.

**THE ELEMENTS A SCHEDULE LISTS ARE NOT THE SCHEDULED THINGS.** Asking Revit for the elements
of DM-11-(600) SOFTSCAPE SCHEDULE returns six RVT Link instances, because the plants live in
the linked component models. Section 8 came back empty for exactly that reason. The printed
rows are not the better route to these numbers, they are the ONLY route, and nothing is ever
recomputed from elements.

**Schedules are per plot**, 155 of each, named `<PlotID>-(600) NAME` and filtered on
PRX_Ref Plot ID Equal `<PlotID>`. Eight distinct names once the plot is off. How many plots of
each name are read in full is the rule above, and it is stated there and nowhere else.

- SHRUBS & LAWN SCHEDULE, category Floors, prints in groups. DM-11 gives GRASS 35 m² 46, then
  SHRUBS & GROUND COVER 70 m² 58, then TOTAL 105 m² 104. The workbook wants the two group
  values and not the total. **How it prints is below and the numbers alone are not enough**, and
  **DM-11 is the special case**: every one of its groups holds one phase
- SOFTSCAPE SCHEDULE, category Planting. Its fields are BOTANICAL NAME, which is
  PRX_Softscape Botanical Name, and COUNT (n), a Count field. DM-11 gives ALBIZIA LEBBECK 6,
  BAUHINIA PURPUREA 2, CASSIA GLAUCA 4, total 12. How it prints is the rule in `CLAUDE.md`,
  stated there and nowhere else, because the group row is what question 8 is answered from
- Two phases, Existing and Proposed. The split shows as a group row inside the printed
  schedule, not as a separate schedule. **FM-05 prints a third, Street Design**, measured on
  the 1536 report, and it is out of scope under the rule above

## How a grouped schedule really prints

Measured off the 1355 scan report, which **is not in this repository** because nothing under
`reports/` is ever committed. The numbers were recorded a round before the shape was, and the
numbers alone were not enough: a reader written to them found no subtotal at all.

DM-11-(600) SHRUBS & LAWN SCHEDULE is eleven columns wide and prints this. **Every DM-11 group
holds ONE PHASE**, which is what made it the wrong plot to learn the shape from:

```
IMAGE | # | PLANT CODE | BOTANICAL NAME | AREA  (sqm) | COUNT (n) | HEIGHT (m) | ... | L/DAY
GRASS                                                                     group heading
Proposed                                                                  phase
Pennisetum Setaceum.jpg | PEN SET | ... | GRASS: PENNISETUM ... | 35 m² | 46 | ...   species
                                                          | 35 m² | 46 | ...        the phase
                                                          | 35 m² | 46 | ...        the group
SHRUBS & GROUND COVER                                                     group heading
Proposed                                                                  phase
Bougainvillea glabra Pink Pixie.jpg | ... | 36 m² | 46 | ...                    species
Carissa macrocarpa - grandiflora.jpg | ... | 34 m² | 12 | ...                   species
                                                          | 70 m² | 58 | ...        the phase
                                                          | 70 m² | 58 | ...        the group
TOTAL                                                     | 105 m² | 104 | ...      the lot
```

A group holding two phases prints THREE rows. FM-05 GRASS, off the 1536 report, whose two
phases are Proposed and Street Design and not Existing and Proposed:

```
GRASS                                                                     group heading
Proposed                                                                  phase
  ... species ...
                                                          | 96 m² | 117 | ...       Proposed
Street Design                                                             phase
  ... species ...
                                                          | 69 m² | 84 | ...        Street Design
                                                          | 165 m² | 201 | ...      the group
```

Three things follow, and `ShrubsAndLawnRows` holds all three.

**The group heading is on its own row**, first cell only and every other cell empty, rather
than beside its numbers. A phase row sits under it in the same shape, so a structure row that
names no wanted heading opens no group.

**A GROUP PRINTS ONE SUBTOTAL PER PHASE, THEN THE GROUP TOTAL. THE PHASES A TREE LIST SHEET IS
NAMED FOR ARE THE VALUE AND THE LAST ROW IS THE CHECK.** Measured on the 0928 run over 20 mosque
plots, four groups out of four, and the third row is exactly the first two added in area and in
item count:

```
DM-16 SHRUBS & GROUND COVER   30 over 39,  54 over 69,   84 over 108
DM-25 SHRUBS & GROUND COVER   13 over 9,   228 over 286, 241 over 295
FM-05 GRASS                   96 over 117, 69 over 84,   165 over 201
FM-05 SHRUBS & GROUND COVER   361 over 450, 459 over 570, 820 over 1020
```

**Two rules came before this one and both were wrong on FM-05.** The first said the subtotal
prints twice and took the first of two. That came off DM-11, where a one phase group prints two
equal rows, and it was right on that one plot and wrong on every plot holding two phases: 30
where the group is 84, 13 where it is 241. The second took the last row, the group total, which
is right where both phases are in scope and wrote 165 and 820 on FM-05, where the second phase
is the street. **The 0928 run refused rather than writing, which is the only reason the first
rule's numbers never reached a client**, and the second rule's did reach a workbook on the 1428
and 1536 runs.

The check stands and is pointed at every phase: **the last row must equal the rows above it
added together, in item count exactly and in area to within the project's own rounding.**
Measured on the 1208 run over RCRC_NG03_EZ: FM-21 prints Existing 2 over 0, Proposed 51 over 11
and a group total of 52 over 11, and FM-22 prints 2 over 0, 80 over 46 and 83 over 46. The
counts match exactly, the areas are off by one in opposite directions, and both are correct,
because the project rounds areas to the metre, every printed area is already rounded, and a
sum of rounded numbers need not equal a rounded sum. The forty seventh pass said exactly that
about the species sum and chose to record rather than enforce, the check was enforced exactly
anyway, and it fired on correct data.

So counts are integers and get no room at all: a count that disagrees is a real fault and
still refuses. Areas get half the unit's rounding step for each row summed, two rows rounded
to the metre may be off by up to one, and the step is read off the project units through
`KpiReader.AreaUnit`, the same read the scan prints as Area unit, rounded to, never a
constant, so a project rounding to 0.01 gets a tighter room and one rounding to 10 a looser
one. **The room has no ceiling yet, on purpose.** Both measured models round to 1, a coarser
step is hypothetical, and a ceiling chosen today is a constant pretending to be a rule. In
its place, a project whose step is coarser than the metre says so at the top of the create
report, the step and the room per row in square metres, one line before anybody reads a
number, so the first project that earns one hands the team a real figure to decide a ceiling
against. The unit that gated the checks travels on `KpiCreateRun`, and a reused press carries
the held run's forward, because the notes were earned against that one. **The pane counts the
run's rounding notes beside the written cells**, one sentence saying they are in the report,
because a note only the report file holds is a note nobody reads. The detail stays in the
report. **Within the room is a line in the report, not a refusal**, the `RoundingNote` on the
`GroupSubtotal`, printed beside the group total row with the rows, the total and by how much,
so a real fault growing slowly stays visible. Outside the room still refuses, naming the room
it is outside of. A step that was not read allows nothing and says so, because a check that
cannot see its subject must not quietly widen. A group printing one row has nothing above it
to compare against and is taken. A phased group that prints no total row is taken too, with a
line in the report saying nothing checked what its phase rows add to, because taken silently
it reads exactly like a group whose total was checked and agreed. The note's and the
refusal's numbers print to six places, because two places printed a project rounding to 0.001
as off by 0 within the 0 it allows, a sentence at war with itself over a comparison the code
got right.

**Every other place printed numbers are added against a printed total was checked in the same
round and named.** The softscape species rows against the printed TOTAL and each group's rows
against its own subtotal are integer counts, exact, and right as they are. `Totalled.Adds`
compares the tool's own sum against the tool's own total, both computed from one list, so it
is a guard for a future caller rather than a live check, and its constant is shared with the
height and diameter difference detector, so it was left alone deliberately. The NOT WRITTEN
line sums integers against no printed total. And the species sum against the group's value,
recorded rather than enforced by the forty seventh pass, was computed and recorded NOWHERE,
its docstring said printed and nothing printed it, so it prints now beside the group on any
real difference, gated by the shared drift epsilon alone and printed to six places. The 0.005
that first gated it was a constant pretending to be a rounding room, the very shape this round
cured the check of, and it swallowed 169.996 against 170 whole.

**The species rows add up to the group**, 36 plus 34 is 70, so the two are held against each
other. They are not enforced, because every one of those numbers is already rounded to the metre
on the way out of Revit and a sum of rounded numbers need not equal a rounded sum.

TOTAL needs no special case in the shrubs and lawn schedule. It carries numbers, so it is not a
structure row, and its first cell holds text, so it is not a subtotal.

**The softscape TOTAL row is read, and the species rows are held against it.** It is the row
whose first cell holds that word, and its count is read off the COUNT column like every other.
The first real workbook read 31 trees where the model held 39 and nothing in the tool could say
so, because nothing read the one printed number that would have. DM-12 prints TOTAL 39 and its
eight species rows add to 39. Adding printed numbers is allowed, so a sum that does not match
the printed TOTAL refuses the write, and a schedule printing no TOTAL row is said in the report
rather than refused, because a check with no subject is not a failure of the schedule. The two
skips that were bare continues are named: a row with a name and no whole count refuses and
names the row, and a row with a count and no name is the subtotal, counted as passed over.

Never read a schedule value by cell position, which is the rule in `CLAUDE.md` and is stated
there and nowhere else. `ScheduleColumns` is what asks the heading row. What these schedules
measure is why: eleven columns wide, the first number in a subtotal row is the area and the last
is L/DAY, so both ends are wrong.

**A reader that cannot find its column refuses, naming the column and printing the headings.**
The four fallbacks stood through two audits: the botanical name off the first cell, the count
off the last whole number, the species test off the first cell and the group count off the
first cell. All four are gone. `SoftscapeRows.Read`, `ShrubsAndLawnRows.Read` and
`ScheduleGroups.Of` hand back what they read or every reason they refused, never both, in one
sentence from `ScheduleColumns.NothingNamed`. A group whose named rows cannot be counted is
still found, with the reason on it, because the group row needs no column.

**A ROW IS A SPECIES ROW WHEN THE BOTANICAL COLUMN HOLDS TEXT, NOT WHEN THE FIRST CELL DOES.**
The first cell is the image, and **an existing species prints with no photo**, so its first cell
is a dash:

```
-                       | ACA FAR | NO BOQ CODE AVAILABLE | ACACIA / VACHELLIA FARNESIANA | 1
Albizia lebbeck.jpg     | ALB LEB | M-329343-A18          | ALBIZIA LEBBECK               | 13
```

`ScheduleGroups` counted what sat under a group off that first cell and reported DM-12 Existing
as 0 named rows of 6 and DM-13 Existing as 0 named rows of 2, while section 6 of the same file
printed five species under DM-12 Existing totalling 10 trees. Both readers now ask the heading
row which column is BOTANICAL NAME and read that one. The measured counts are DM-11 Proposed 3,
DM-12 Existing 5 and Proposed 3, DM-13 Existing 1 and Proposed 2.

`ShrubsAndLawnRows` had the same fault and no report had shown it, because DM-11's shrubs are
all Proposed and every one of them prints with a photo. An existing shrub would have been read
as a subtotal.

**An area prints with its unit attached and a count does not.** 35 m² against 46. The unit comes
off by reading as far as the number goes rather than by stripping characters. A real area can be
nought: the hardscape schedule prints 0 m², which is the number and not an empty cell.

**A digit after the number ends is a refusal, never a shorter number.** Reading as far as the
number goes turned 1,234 m² into 1. Every value the reader had met printed under a thousand, and
the two four figure values ever seen, 1161 and 3729, came off the one project, whose unit format
prints no separator. A number read short passes its own checks: a one phase group over 999
printed two rows both reading as their thousands and 1 equalled 1, and 1,200 plus 1,300 totalling
2,500 read as 1 plus 1 equals 2. Which character a project groups digits with, or uses for the
decimal, is a units setting this tool has never read, so `CellNumber` parses no separator: a
cell holding a digit past where the number ends is refused with the cell named, and 1131,72 is
refused the same way. Revit prints the unit with a superscript two, which is not a digit.

**The 00 link** is RCRC_NG05_NU_MAIN_RVT24_00.rvt, loaded, 279 filled regions all in a view
called Intervention Limits. Types RCRC_CADASTRAL LIMIT 124 and RCRC_OUT OF SCOPE
(PRESENTATION) 155. PRX_Intervention Area is on all 279 with 266 values and every region
carries PRX_Ref Plot ID, so the report prints the plot beside each region and counts how many
regions of each type carry one. **Which of a plot's two regions carries the area varies by
plot**, which is the rule in `CLAUDE.md`, and it is why the report reads one plot's regions
together rather than picking a type.

**Neighborhood Name**, spelt the American way without a u, is a shared parameter on Project
Information holding KING FAHD. Neighborhood Group holds GROUP 5.

**Units.** Raw areas are square feet whatever the project shows. Printed areas are the project
unit, square metres rounded to 1. 12496.8999938 raw prints as 1161 m².

1385 sheets, every one with a title block, 11 title block types across two families.

## What the two real workbooks measured, once

A one-off check on 2026-09-09 against two EXISTING PARKS files supplied in a chat session, the
production copy and the annotated one. **It cannot be re-run and no gate repeats it.** The
files are not in this repository and never will be. What is committed is what it taught.

Every mapped cell agreed with the annotation, D3, C5, E4, D8, F11 and H11, and E5, G5 and H5
read DATE OF THE DAY, EMPLOYEE NAME and EMPLOYEE POSITION. Those three come from nowhere in
Revit. The team types them into the pane and the tool copies them through, so a filled
checklist carries who filled it and when. **That those three sit in the same cells in every
template was read off this one file and held for six rounds**, which is what finding 50 named
and what the 14 September measurement of all seven settled. Nothing holds their letters now:
they and the reference are found by their labels, under the section above.

Two things the check found that reading the map could not:

- **The annotation writes the shrubs and lawn note in row 10 AND row 11.** Row 11 is the
  input. In the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would
  destroy a formula. The map's F11 and H11 are right, and now proven right
- **`Area` is a defined name pointing at `<Park Name>`!$D$8**, so D8 is the area the whole
  sheet computes from, and `H9` is `=H8/Area`, which is the divide by zero that goes away when
  the area is filled

The header is row 3 and the total at row 93 is `SUM(B4:B92)` on both sheets. Where the species
stop is read off each sheet when Create is pressed and is in no map, because the MOSQUES
existing names ran to row 101 on 2026-09-10 against a map entry that said 83.

## What the first real workbook measured

One press of Create on DM-12 with the MOSQUES template, 2026-09-09. **The workbook is written
and correct.** These are measurements off that output file, not reasoning about it.

- **37 parts in, 37 out, 4 changed**, and the output recalculates with ZERO errors. The parks
  figure above, 45 against 44, is EXISTING PARKS and is a different template. Both stand. It
  reads 37 in and 36 out now, because the calc chain is removed on purpose
- **Excel opened it showing zeros for seven computed cells** while the inputs beside them were
  right. That is the stale cache in the patcher section above, measured on this same file
- The six values landed and the client's own formulas ran on them: **28.1 percent canopy against
  a 13 percent target, Excessive, NOT COMPLIANT.** The tool wrote no verdict anywhere. That is
  the workbook's arithmetic on the numbers Revit gave it
- **The slash case matched.** ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana
  at row 11, which is why nothing is stripped or split on the way to a comparison
- **Three species were not in the list**, all under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2
  and WASHINGTONIA ROBUSTA 1. The MOSQUES tree list holds 80 species in rows 4 to 83 and not one
  of them is Phoenix dactylifera, Washingtonia robusta, or any of the Unknown rows. Checked
  against the output file itself rather than against the map

The last one has a consequence, and the fix and the open question are two different things:

**That run read 2 existing trees where the model holds 10, and 31 in total where the model holds
39**, because the three were named and written nowhere. They are written into the empty rows now,
under the species rule above, so the counts reach the total. **Nothing in the tool may ever place
an unmatched species by guessing**: the name and the count go in and no other column does.

**What is still open is that a row written that way carries no family, no genus and no native
flag**, so the KPIs that need those cannot see it, and the client's species lists are short of
trees this project actually plants. That is for the team. `steps/log-kpi.md` carries it.

## What the first twenty plot run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 11:16, and the workbook it wrote read against
the model. These are measurements off that run, not reasoning about it.

- **The subtotal rule holds on every number it was predicted to move.** DM-16 shrubs 30 to 84,
  DM-25 13 to 241, FM-05 shrubs 361 to 820, FM-05 grass 96 to 165. Shrubs total 2517 to 3258,
  lawn 1058 to 1127
- **The date, the prepared by and the position reached the file.** The cache section read
  cached results left 0, dropped 1674, 37 parts in and 36 out with `xl/calcChain.xml` named as
  the one removed on purpose. Three species were written into empty rows on the Proposed sheet
  with the name and the count and nothing else
- **Tree List - Existing had three row ranges**: the map and the pane said B4 to B83, the
  sheet's total said `SUM(B4:B92)`, and the names ran from row 4 to row 101. Six species were
  reported as having nowhere to go, 85 existing trees between them, and four of the six sat in
  the list past row 83. The workbook said 76 existing trees where the model holds 161. Fixed
  under the species rule
- **FM-05 printed twice in one species row**, taken that round for two schedules whose names
  hold SOFTSCAPE. The 1428 run showed one schedule and two printed rows, and the 1536 report
  showed the second row under a third group, Street Design, under the rule above
- **313.5 seconds, 312.8 of them reading the model**, 20 plots at 15.6 seconds each, and 0.7
  seconds for everything after the read. STREETS ticks 78 plots, which is about twenty minutes
  at that rate. What the read does per plot is in the log. It is measured in calls and not in
  seconds, because nothing here runs Revit, and it is not changed

## What the 1428 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 14:28, the workbook it wrote, and that workbook
opened in Excel. These are measurements, not reasoning.

- **The tree list fix worked.** B84 17, B86 27, B87 19, B89 3, B99 3, so the 69 existing trees
  that went nowhere the run before are in the file, species matched went 16 to 21, and the tree
  list section proved it in four lines
- **A written row broke the canopy maths**, nine #VALUE! cells from Proposed M84 to the KPI row,
  fixed under the species rule with the height and the diameter off the schedule
- **FM-05 printed twice again** with the accounting reading one softscape schedule on all 20
  plots, which is what showed the double to be two printed rows of one schedule. That round
  read them as two rows under Proposed. The 1536 report's printed section showed the second
  under Street Design
- **Excel opened the file and did not calculate it**, every formula cell blank until Ctrl Alt
  F9, fixed with calcMode under the patcher rule
- **Every Meets KPI and Compliance cell read #NAME?** off `_xlfn.IFS`, sixteen cells, not the
  tool's doing and now counted in the report
- **Nothing in the report was what the tool read.** The last section prints every schedule
  this run read as the schedule prints it, every column aligned, with what was read off it
  and which subtotal row was taken and why, capped at 200 rows a schedule and named at the top

## What the 1536 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 15:36, the first run whose report ended with
every schedule as printed. FM-05 refused on three species printed twice under Proposed, and
the printed section showed why. These are measurements, not reasoning.

- **FM-05's softscape schedule holds three groups.** Existing at row 3, four species, subtotal
  6. Proposed at row 9, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 19, CASSIA GLAUCA 3, subtotal 32.
  Street Design at row 14, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4,
  CONOCARPUS 4, subtotal 38. TOTAL 76 at row 20
- **Its shrubs and lawn schedule holds the same third phase.** GRASS Proposed 96 over 117,
  Street Design 69 over 84, 165 over 201. SHRUBS AND GROUND COVER Proposed 361 over 450, Street
  Design 459 over 570, 820 over 1020. So the 165 and 820 the 1116 run wrote counted the street
- **DM-25 prints Existing, then Proposed, then Existing again**, in its softscape schedule
- Street Design is out of scope by Bader's decision, under the rule above, and the model will
  be corrected later. On STREETS it counts as Proposed, by the same decision, under the section
  that follows the rule

## The area is not a schedule row

The report used to end saying every number above came off a row the schedule printed. **The area
did not.** H7 took 3728.7570000000005, converted from the raw 40136.006313679296 square feet off
`PRX_Intervention Area` on the chosen filled region in the 00 link, where the schedule prints
3729.

The conversion is right and is more precise than the printed value. The sentence was wrong about
it. The region row carries the raw reading, the converted metres and what the model prints, side
by side and unrounded, so the two can be held against each other, and the closing paragraph says
the schedule claim for the numbers it is true of and names the area separately.

## What the team types reaches the cells

The date, the person and their position come from no model. The pane collects them, and **it
used to collect them and hand none of the three on**: `KpiCreateAsk` did not carry them and
`KpiCreatePlan.Of` defaulted all three to null, so the first real workbook came out holding the
template's own placeholders while the report said nobody had typed them, on a run where all
three boxes were filled in.

The three are REQUIRED arguments of `KpiCreatePlan.Of` now, so a caller that forgets them does
not compile. A default that reads as a deliberate empty is how a whole link in a chain goes
missing without a word.

## Open, and not to be guessed at in code

The first real scan raised five. The 1355 run settled four of them, and the answers are facts
about the project rather than about the tool, so they are in `CLAUDE.md` and are not repeated
here. What each one turned out to be:

1. Is PRX_Component the component the workbook wants. **Yes**, and it is the asset type
2. Which of the four plot parameters is the workbook's Ref. **PRX_Plot_ID**
3. Which filled region type is the plot's intervention area. **The one the client's note
   names**, settled by the 14:29 run, 98 plots off it and none off anything else. One region
   holding an area still answers itself whatever it is called, so the type decides only a tie
4. Are the two group headings in SHRUBS & LAWN the same on every plot. **STILL OPEN**
5. Does an Existing group ever appear in SOFTSCAPE SCHEDULE. **Yes**, DM-12 has one

Two the 1355 run raised in their place are in `steps/log-kpi.md`, both about matching a species by
name, and nothing in the code picks an answer to either.

The 1521 run raised a third, which template each value of PRX_Component means. **The 1548 run
settled it.** Eleven values came off the report's own component block and the team turned them
into the table above. It is not repeated here.

## The status line moves while a run does

A scan took 123 seconds on RCRC_NG03_EZ, 104,031 elements, behind one line that did not move,
which is what a hung tool looks like, and a run over 78 street plots is minutes of the same.
`ProgressWords` in Core holds the lines and the counting, with tests, and the pane shows them:
the scan announces each section as its read begins, off the report's own numbered headings, so
Section 4 of 9, linked models, and the schedules loop counts, Sections 5 to 8 of 9, schedules,
400 of 951, 42%, said as a span because one reader covers those four sections in one pass.
Create names the plot being read, Reading DM-44, plot 3 of 18, 11%, then the writing steps name
themselves, adding up, copying the template, writing the cells, reading them back, checking the
formulas, writing the report, raised by the patcher itself through a callback so the words come
from the work rather than a narration beside it. A press that answers from the held readings
says so, Reusing the readings already held, because a two second finish after a two minute
read looks like something skipped until the screen says reuse. The report's Readings line is
the record and this is the live half of it.

**Driven by what is done, never a timer and never an estimate.** A percentage appears only
where the total is known, is floored, and is driven by a count that only grows, so it cannot go
backwards, and where the total is not known the count stands alone. The run's own end line
still comes through `Told` after every finish, refusal or throw, so the last thing on screen is
never a count that stopped moving. Everything still goes through the external event on the
Revit thread and the pane's `Moved` only sets text and lets the paint through.

**Whether the line visibly moves mid run is UNKNOWN until somebody runs it.** A dockable pane
can share Revit's own thread, and a text set from inside `Execute` then sits unpainted until
the run returns. `Moved` queues one empty job at render priority after each line, which lets
the paint through when the threads are one and costs nothing when they are not, and the log
records the question as open. **Render and never background**: waiting on a job pumps
everything queued at or above its priority, and input sits above background and below render.
Pumping at background would dispatch every queued click inside `Execute`, mid read or mid
write, and two of the pane's buttons open a folder dialog owned by no window, behind which
Revit's own ribbon stays live. At render priority the paint goes through and every queued
click stays queued until the run returns.

## The scan is a step inside Create, and there is no scan button

**Pressing Create used to want a scan the user had to know to do first**, which is the tool's
business and not theirs. `ScanNeeded.Decide` in Core is the rule: the model is read when no
scan is held or what is held is of another model, and a scan already held answers a second
press on the same model. It is the same shape `HeldReadings.Decide` uses for the per plot
readings, because two shapes for one kind of question would be two rules. The press says
which of the two it did, Reusing the scan already held or the section lines of a real read,
for the same reason the readings say it: a press that finishes in seconds where the last read
for two minutes looks like a press that skipped the work.

The KPI Scan button is gone from the pane and the ribbon tooltip no longer names it. The
strip carries the model's name and its element count and nothing to press. **The header's
numbers come back with the plot read** rather than off the scan, so the name has a count
under it as soon as the pane is shown, which is what the scan line used to do only after
somebody pressed a button. Nothing about the scan report changed: it is still written, still
named, and still the thing the team reads. Its headline goes through `Progressed` rather than
`Told`, because `Told` is the run's end line and saying the headline there would end the press
on screen, and shut the progress window, with the workbook still being filled.

The old rule this replaces said a KPI control must never be called Scan Model, because that
name is a button inside the Drawing Sheet pane and two buttons with one name doing different
things is a trap. That still holds for any control this pane grows. It simply has no scan
control to name.

## The progress window is modeless, owned, and opened outside Execute

A 123 second read behind a docked pane is a tool that looks dead, and the status line cannot
be seen at all when the pane is behind something else. `KpiProgressWindow` shows what the
status line shows, the same words out of `ProgressWords`, because two wordings for one run is
two records of one fact.

**Owned by the Revit main window**, through its handle, so it stays over Revit and goes away
with it. A window owned by nobody sits over every application on the machine with Revit's own
ribbon live behind it, which is the hazard the fifty fifth pass's breaker caught in the
repaint pump. **Modeless**, so it never blocks the thread the run is on. **Opened by the pane
before the external event is raised and closed when the run's last answer comes back, never
from inside `Execute`**, so nothing about the window runs inside the handler's own call frame.
`Told` is what closes it, being the run's end line whatever ended it, a finish, a refusal or a
throw.

**There is no Cancel, and that is deliberate rather than unfinished.** Cancelling mid read
would leave a half read set of plots that the reconciliation would count as plots read, which
is the one state the rest of the tool would trust and should not. A cancel that lies is worse
than no cancel, and an honest one is a cancellation threaded through every reader with a
discard of everything read, which is its own round.
