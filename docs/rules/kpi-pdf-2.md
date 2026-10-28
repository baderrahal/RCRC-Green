### HOW BIG EVERY VALUE IS WRITTEN, SO IT STAYS INSIDE THE CLIENT'S OWN BOX

**Measured on ANH-007-MO-100011**, a DAILY MOSQUE plot on the Open spaces form: Area showed
2797.6 cut off at the edge of its box, Total areas to be greened showed 0.0008 cut off, and the
tree and shrub counts were drawn taller than the boxes holding them.

**NOTHING IN THIS TOOL READ OR WROTE A `/DA` BEFORE THIS ROUND.** `PdfFormFile.WithValue` drops
each field's `/AP` and the AcroForm gets `/NeedAppearances true`, so the viewer redraws the value
using the field's OWN default appearance, which nothing here had ever looked at. And the
rectangle read took only the first two numbers of `/Rect`, which is the position the form check
compares, so the box's SIZE was thrown away on the way past.

The rule, for every text field the tool writes a value into:

```
across  = the box width less 2 pt each side
down    = the box height less 2 pt top and bottom
em      = the text measured in the font the /DA names
wanted  = the smaller of across / em and down, rounded DOWN to 0.1 pt
ceiling = the client's own size, or 10 pt where their /DA gives nought
floor   = 6 pt
```

Worked out by hand, which is the case the test holds: **`2797.64` in Helvetica is six digits at
556 and a full stop at 278, so 3.614 em. A box 30 pt wide and 14 pt tall leaves 26 pt across, 26
over 3.614 is 7.1948, and it is written as 7.1.**

**ROUNDED DOWN AND NEVER TO THE NEAREST**, because a tenth of a point rounded up is a tenth of a
point of text outside the box.

**THE VALUE IS NEVER TOUCHED.** Nothing rounds, cuts or reformats a number to make it fit. The
text stays exactly what `PdfFill` decided, and only the size inside the field's own `/DA` moves,
in the appended object. The client's font name and their colour operators go through untouched,
`NeedAppearances` stays, and no appearance stream is written.

**THE `/DA` IS INHERITED THE WAY THE FIELD KIND ALREADY IS**, the field's own, then its parents',
then the AcroForm's. Where a field inherits one, the inherited string is written onto that field
with the one number changed, because a size is per field and an inherited one cannot be moved for
one field without moving it for every field that shares it.

**A WIDTH IS READ OR IT IS UNKNOWN AND IT IS NEVER GUESSED.** Two sources, in order: the font
object's own `/Widths` array off the AcroForm's `/DR /Font`, and the published metrics of the
standard fourteen where it carries none. **A font that answers neither way writes the value at
the client's size unchanged and is named with its field.** So is one character with no width: a
sum short of a term is a narrower string than the real one, which would shrink the text too
little and leave it running over with the report saying it fits.

**NO NEAR MISS IS IN THE STANDARD FOURTEEN TABLE.** Arial is metric compatible with Helvetica and
Courier New with Courier, and neither is written in. Such a font must carry its own `/Widths` in
the file, which is where a Courier New field's widths really are. **THE TABLE IS THE PUBLISHED
CORE FONT METRICS HELD AS DATA and it is not measured in this repository**, because no AFM file
is here and none will be. What a width being wrong can cost is bounded on purpose: the value is
never changed, the size is never raised above the client's own, and a width out by a few
thousandths moves a size by a tenth of a point.

**THE READ BACK CHECKS THE SIZE LANDED**, off the written file, the same rule every written cell
of the workbook already follows, and a size this run set that did not land is counted as a bug in
the tool rather than as a field it never tried to set.

**SIX OUTCOMES, COUNTED ONCE AT THE TOP.** The round message named five and an absent `/DA` is
the sixth, because a font nobody could measure and a field with no size to change are different
facts. `THE TEXT SIZES` in the glance counts kept the client's size, shrunk, capped at 10, held
at 6, widths UNKNOWN and no `/DA`. **A LINE PER BOX ONLY FOR THE THREE SOMEBODY HAS TO ACT ON**,
held at 6, widths UNKNOWN and no `/DA`, because over 150 plots a line per field is two thousand
lines nobody reads and a count alone says a size is wrong somewhere without saying which box.

**THE CEILING WINS OVER THE FLOOR WHERE THE TWO DISAGREE**, which is a client `/DA` naming a size
below 6. **Whether any field carries one is UNKNOWN**, because no client PDF is in this
repository, and the outcome says the text did not fit at the floor either way so the box is
named.

**EMPTYING A FIELD FITS NOTHING.** There is no text to fit, so the client's own size is left
exactly as it was.

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

