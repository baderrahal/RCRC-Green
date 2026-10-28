## 2026-09-15, ninetieth pass. The 154 plot press read back, seven decisions

**Merged to main as `4411821`**, pull request 148, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2070 tests, 1250 of them KPI**, up from 2042 and 1222, 28 hook cases, build zero warnings.
**80 audit findings, 20 FIXED, 60 open**, counted off the four files and nothing closed,
renumbered or reordered. Bader ran the 154 plot press on NG05 on 15 September at 13:32 and 146
workbooks and 146 PDFs were written. **The report is not in this repository and must not be**, so
every measurement below is copied out of it.

**NOTHING HERE WAS RUN IN REVIT.** Item 7's Revit wiring in `KpiRequestHandler` cannot be run
from this session at all, and it is said again beside the tests that cover its Core half.

### A read schedule with no such group writes 0, and an absent schedule still does not

The press left the PDF Lawn box blank on **81 plots** saying the schedule printed no GRASS group,
and the four shrub boxes blank on **15 plots** saying it printed no SHRUBS & GROUND COVER group,
with CELLS NOT WRITTEN naming both. Bader's decision: a schedule that was READ and prints no such
group means the plot has none, so the box is a 0.

`PdfFill.NoGroupIsNought` is the working that travels with it and `PdfFieldFill.NoughtForAnAbsentGroup`
is the flag. **The flag is what the glance counts, never the words**, because a signal that
travels in the printed text is not a signal and this repository has already paid for that once,
and because item 6 rewrote every printed line in the same round.

**AN ABSENCE IS NOT A MEASUREMENT.** A plot holding no shrubs and lawn schedule at all leaves the
five boxes blank through `PdfFill.NoScheduleRead`, which names the schedules found where there is
more than one, so two of a kind and none of a kind are different sentences.

The workbook half is `KpiMerge.Subtotalled`: a plot whose read schedule holds no group now
contributes 0 rather than being skipped, so the Lawn and the Planting cells are written where
they used to be reported NOT FOUND. A plot with no schedule is still left out of `PerPlot`
entirely, which is what `KpiCreatePlan.Number` refuses to write from.

The water demand half is `WaterDemandRead.NothingToTotal`: a schedule printing its heading row
and **no rows under it** counts 0. MM-01, MM-06, MM-07 and NS-23 wrote no Irrigation water demand
at all with both halves empty, and MM-08, NS-28 and NS-38 with the shrubs and lawn half empty.
**A schedule with body rows and no TOTAL row still refuses**, unchanged.

**THE NOUGHT IS ASKED AFTER THE COLUMN HAS BEEN FOUND, on purpose.** A heading row that does not
name L/DAY is a schedule this reader cannot read, and answering 0 for it would be the fall back
to a position this repository forbids everywhere else. Counting 0 before looking at the heading
row at all is the choice not taken, and it is recorded here as one.

Break watch: the blank was put back for a missing GRASS group.
`AReadScheduleWithNoGrassGroupWritesTheLawnBoxNought` went red reading **DM-16's shrubs and lawn
schedule was read and printed no GRASS group, and the Lawn box was left blank rather than
written 0**. Restored, md5 `0e789af1aacfa742902e3f456dc58bb8`.

### A plot with no schedule is not a zero

FM-07 is on a sheet and on no schedule. Its PDF read Existing Trees 0, Proposed Trees 0, TOTAL
trees 0 and Total areas to be greened 0, and the model's own KPI% schedules list **57 trees** for
it. Four noughts went to the team as a measurement of a plot nothing had read.

`PdfFill.NoSoftscapeRead` is the reason and all four boxes are blank now. `Greened` refuses first
too, because the canopy is counted off the tree rows and a green cover computed without one is
short by however many trees the plot holds.

**THE PLOT LIST ROW SAYS THE SAME**, through `KpiCreateReport.NoSoftscapeOnTheList`, even though
both files were written. A row reading YES and YES with an empty last column is the row of a plot
that came out right, and FM-07's was one of those.

Break watch: the counts were let fall back to 0.
`APlotWithNoSoftscapeScheduleLeavesTheTreeBoxesBlankAndNamesWhy` went red reading **FM-07 holds
no softscape schedule and ExistingTrees was written '0'**. Restored, md5 unchanged.

### The Parks boxes are 9.72 pt tall, so the height margin is 1 pt

**271 values were held at 6**, every one of them on `Projects Basic Data - Parks`. The boxes are
41.52 by 9.72 pt for a value, 41.734 by 9.61 for a shrub box and 167.346 by 9.818 for a header.
Open spaces is 46.6 by 19.4 and roads 46.8 by 17.0, both at 10.

`PdfTextFit.HeightMargin` is 1.0 and `Margin` stays 2.0 for the width. A value held at 6 says
whether its **WIDTH** or its **HEIGHT** held it, and **runs over is printed only when the text is
wider than the box at 6 pt**, which is what 271 of those lines were wrongly claiming.

Worked by hand: 3977.16 in Helvetica is six digits at 556 and a full stop at 278, 3.614 em.
41.52 less 4 is 37.52 across, 37.52 over 3.614 is 10.38. 9.72 less 2 is 7.72 down. The smaller is
7.72, floored to **7.7**. The same value in 46.6 by 19.4 gives 10, and 2797.64 in 30 by 14 gives
7.1 unchanged.

`AShortBoxBoundsTheSizeByItsHeight` was corrected by hand from an 11 pt tall box to a 9 pt one,
so its stated 7.0 arithmetic is still true. The expected number was not changed.

### Never write a new species into a row with no canopy formula

On FP-23 the run wrote CONOCARPUS LANCIFOLIUS, 2 trees, into
`GRP_-_KPI_Checklist_-_DD_FUTURE PARKS.xlsx`, Tree List - Proposed, **D85 and B85**. That row
holds C85, M85 and O85 and no L85, so the canopy guard blanked FP-23's Total areas to be greened
and its canopy percentage. The guard was right.

`SpeciesList.UsableEmptyRows` is the fix: an empty row the total reaches AND whose own cells carry
the canopy formula. `SpeciesMatching.WrittenInto` queues those, so row 85 is stepped over and row
86 is taken. Where none is usable the species is not written and `SpeciesList.NoUsableEmptyRow`
names the file, the sheet and every empty row it checked.

**ONE RULE FOR THE GUARD AND FOR THE READER.** `WorkbookArithmetic.IsTheCanopyFormula` is the
comparison, asked by the guard that checks a row this run wrote and by the reader that decides
whether an empty row may be written into. Two copies of it would be two answers to one question.

**A SHARED FORMULA IS EXPANDED.** Excel stores a column of one formula as text on the master and
an index on every cell under it, so `SpeciesList` reads the canopy rows through
`WorkbookFormulas.Of`, the same reader the whole check uses. A reader taking the text alone would
have seen the canopy on row 4 and on none of the eighty rows below it.

**A SHEET NAMING NO DIAMETER COLUMN IS NOT CHECKED AT ALL**, and `CanopyRowsRead` says so. There
is no formula to look for, and calling every row unusable would refuse every write on a shape
nobody has measured.

Break watch: `list.EmptyRows` was put back in place of `UsableEmptyRows`.
`ANewSpeciesSkipsTheEmptyRowWithNoCanopyFormulaAndTakesTheNextOne` went red reading **CONOCARPUS
LANCIFOLIUS landed on row 85**. Restored, md5 `95fcec677cced1e366d6ef49322a0e1b`.

### Trees written nowhere show at the top

FP-17 36, FP-21 25 and FP-20 5 AZADIRACHTA INDICA, and FP-23 1 tree named UNKNOWN, all written
nowhere. **THE PLOT LIST read YES and YES for all four and the glance said nothing.** 67 trees
left the building in rows that read exactly like the rows of a plot with nothing wrong with it.

`TreesNotWritten` counts them off the runs' own matches, so the count and the refusal that
produced it are one record. THE PLOT LIST has a **trees not written** column, reading
`36 AZADIRACHTA INDICA`, and the glance carries one line, **THE TREES WRITTEN NOWHERE**, which
says so even when nothing was lost.

`SpeciesMatching.OnMoreThanOneRow` names the file, the sheet and every cell holding the name,
`D22 and D84`, so somebody can open the template at the rows. **The tool still does not choose
between two rows** and will not.

Break watch: the column was dropped from the row. `ThePlotListRowAndTheGlanceBothNameWhatWasLost`
went red naming FP-17's row. Restored, md5 `202b9aabe864162c0d5457ead5b9227f`.

### No printed line says client

Bader: the team reading the report does not know which client is meant. **26 printed strings held
the word and none does now**, across `PdfEmptying`, `RunAtAGlance`, `PdfChecklist`,
`TemplateWords`, `CanopyArea`, `RegionChoice`, `PdfTextFit`, `KpiCreateReport`, `PdfFormCheck`
and `KpiPanel`. Each names the thing instead: the folder, the form file, the template's own list,
the field's own `/DA`.

`RegionChoice.TheNote` is the one record of the note itself, `the area cell's own note, REVIT 00
LINK / ID FILLED REGION RCRC_OUT OF SCOPE (PRESENTATION) / PRX_Intervention Area`, and every line
that used to say the client's note now prints that.

**THE CANOPY GUARD LINE NAMES THE CELL.** It carries the workbook file name, the sheet, the row,
every formula on the row with its text, every cell somebody typed a value into with no formula
behind it, and the one cell that would have carried the canopy: `L85 is empty`, or `L85 holds 50
and no formula`. Which column that is, is read off the sheet's OWN other rows, never off a
letter. `FormulaCheck.TypedCells` is the new reading behind it.

**WHAT M85 IS, IS UNKNOWN AND IS NOT PRINTED.** Nothing in this tool records what the canopy area
column's formula is, so naming that cell would be a rule nobody measured. It is a question for
Bader and one row's M cell settles it.

Break watch: the old sentence was put back. `TheGuardNamesTheFileTheSheetTheRowAndTheCanopyCell`
went red on the missing file name. Restored, md5 `9621e488579eb5402fe57c8df59cb7d5`.

### The crash row

Bader's decision: after a crash the tool leaves every file where it is and the plot's row names
them. `PlotCrash.Row` is the words, `CreateStep` the step and `CrashFile` one file with its path
and whether the disk really holds it, read at the moment the row is built.

**THE FOLDER FLAG READS THE DISK.** It was handed `false` in the catch whatever was on disk, so a
press that made the folder and threw a step later counted one folder fewer than the tree really
holds. `PlotWriteTrail` in `KpiRequestHandler` records the step AS IT IS REACHED rather than
working it out backwards, and the copy and the patch are told apart by the output file's own
existence, read at the patcher's own progress call. **Nothing is deleted.**

**THE REVIT HALF HAS NOT BEEN RUN AND CANNOT BE RUN FROM HERE.** Its Core half has five tests.

### Open questions for Bader

1. **What the canopy area column's formula is** is UNKNOWN. One row's M cell makes the guard line
   name that cell too.
2. **The FUTURE PARKS duplicate row** is the team's fix and THE PLOT LIST is how it is watched.
3. **Rows 85 to 101** are still dead rows on the templates. The tool steps past them now instead
   of writing into them, which is better and is not a fix.

### What was not changed

The `/DR` and `/Font` inline open item stays logged, the press measured every font and came back
with 0 widths UNKNOWN. The component and prefix refusal on MM-09 to MM-15 and FP-27 stays. No
audit finding was closed, renumbered or reordered.

`steps/2026-09-15-kpi-rerun.md` is the sheet for the rerun, 23 steps, the fixes synced and a
fresh detached copy taken first, the seven workbook rows ticked before Tick the list, and six
checks one to a step.

---

## 2026-09-15, eighty ninth pass. The run sheet corrected before the first run

**Merged to main as `346dddb`**, pull request 146, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**No code and no test.** `steps/2026-09-15-kpi-fixes.md` only, plus this entry and the state
entry the commit hooks require. **2042 tests, 1222 of them KPI, unchanged**, 28 hook cases
unchanged, build zero warnings. **80 audit findings numbered, 20 FIXED, 60 open**, unchanged and
not recounted.

**NOTHING HERE WAS RUN IN REVIT.**

### The step the sheet was missing, and it would have cost the whole run

**A PLOT ONLY GOES INTO A WORKBOOK WHOSE ROW IS TICKED AND WHOSE TEMPLATE IS SETTLED.** `Settled()`
at `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:1304` drops every other row and the split at `:1296` runs
over what it hands back, so **a ticked plot belonging to no ticked row is written nowhere**. The
sheet went from Read this model straight to Tick the list and never said to tick the seven workbook
rows, and those ticks live in the pane with nothing writing them down, so closing Revit loses
them. A run made to the sheet as it stood would have ticked 154 plots and written nothing.

It is step 12 now, between Read this model and Tick the list, and it says to settle a row still
asking which of the two park templates it is, because an unsettled row arms nothing.

**AND TICK THE LIST HAS TO BE THE LAST TICK ACTION.** Ticking a template row ticks every plot that
belongs to it and unticking one takes them off, at `KpiPanel.cs:1248`, both of them moving the
plot ticks Tick the list has just set. The step says so, and says to press it again if a workbook
row is touched afterwards.

Every step and every reference to a step number is renumbered. The checks went from four to five.

### The five plots were not a measurement and they are gone

The sheet said DM-21, NS-16, NS-20, ST-23 and ST-24 are in the model and not among the 154.
**Nothing in this repository shows that.** Counted rather than argued: `DM-21` appears here only in
a Drawing Sheet test fixture, `tests/RcrcGreen.Core.Tests/PlotSelectionTests.cs:11` and `:34`,
which is a list written for a test rather than anything read off a model, and NS-16, NS-20, ST-23
and ST-24 appear nowhere in the repository at all. They came out of the round message and the
eighty eighth pass passed them through as fact.

The sheet says UNKNOWN until the run now, and says IN THE MODEL AND NOT ON THE LIST names whichever
plots really are, because that block is read off the model's own plot list at the press.

### A fifth check, on the Parks form

**THE PARKS ROWS SIT ABOUT HALF AS FAR APART AS THE OTHER FORMS' DO.** Measured off
`src/RcrcGreen.Core/Kpi/PdfForms.cs:276` to `:278`: the three tree rows are at y 611.3, 601.0 and
590.6, which is 10.3 and 10.4 apart. The open spaces form's same three rows, `:310` to `:312`, are
at 449.2, 428.9 and 408.9, about twice that.

The height rule takes 2 pt off the top and 2 pt off the bottom, so a Parks box as tall as the gap
to its neighbour leaves about 6.3 pt, barely over the 6 pt floor, and any shorter box lands on it.
**So some Parks vegetation numbers may be named held at 6 while they still sit inside their box.**
The check is to open one EP or FP plot's PDF, read those numbers and send that plot's own lines
under THE TEXT SIZES back.

**WHAT THOSE BOXES ARE REALLY THAT TALL IS UNKNOWN HERE.** The row positions are measured and in
this repository. The heights are not, because no client PDF is in it and none ever will be, so a
height written down here would be a guess. The check is what measures them.

### One list of what to send back

The sheet ends with it, in one message: THE TEXT SIZES and every box line under it, THE PLOT LIST's
three counts and every row reading NO with its reason, the whole dash rows block, and screenshots
of the four PDFs. And the standing rule beside it, that none of those files goes into this
repository.

### Open, logged and not fixed

**1. A CRASH CAN LEAVE A WORKBOOK ON DISK WHILE THE REPORT SAYS NO.** `GuardedWrite`'s catch at
`src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs:608` records the plot at `:615` as
`PlotOutcome.WroteNothing(..., false, refusal)`, where the `false` is the folder, while its own
docstring at `:578` says what is already on disk stays there and the folder count still counts the
folder that was made. Those two disagree. And there really can be something on disk:
`WorkbookPatcher` copies the template to the output path at
`src/RcrcGreen.Core/Kpi/WorkbookPatcher.cs:79` and opens the zip to patch it at `:86`, so a throw
between them leaves a whole unpatched copy and a throw inside them a half patched one. The PDF is
written after the run is counted, `KpiRequestHandler.cs:706` to `:720`. `THE PLOT LIST` reads the
plot outcomes at `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:202`, so it prints NO for a plot whose
folder holds a file. **Which of the two is wrong is not decided here.** Not fixed this round.

**2. THE HEIGHT RULE HOLDS THE PARKS VEGETATION BOXES NEAR 6 PT**, as measured above off
`PdfForms.cs:276` to `:278`. Whether that is a real fault or a form whose boxes genuinely are that
small is UNKNOWN until the run, and check 19 is what answers it. Not fixed this round.

**3. FONT WIDTHS ARE READ ONLY FROM A `/DR` AND A `/Font` WRITTEN INLINE.** `FontsIn` calls
`Nested(acroForm, "/DR")` at `src/RcrcGreen.Core/Kpi/PdfFormFile.cs:327` and
`Nested(resources, "/Font")` at `:330`, and `Nested` at `:406` finds the key and then the next
`<<` in the same object. **A form writing either as an indirect reference is not followed**: the
read comes back empty, or it reads whatever dictionary happens to come next in that object, and
either way every width is UNKNOWN and every value keeps the client's own size. A font OBJECT given
as a reference IS followed, so this is about the two dictionaries alone. **No test covers that
shape.** Not fixed this round.

### Files

`steps/2026-09-15-kpi-fixes.md`, this file and `steps/ai-max-state-kpi.md`. Nothing else.

---

## 2026-09-15, eighty eighth pass. Four fixes, and the plot list the team sent

**Merged to main as `088d6d3`**, pull request 144, squashed with both message fields passed on the
call, so the message came back off main byte for byte with no co-author line and no generated-by
footer.

**2042 tests, 1222 of them KPI, 42 added, against the 2000 main carries** at `64fb57d`, 28 hook
cases unchanged, build zero warnings.

**THE AUDIT COUNT, WALKED OFF ALL FOUR FILES THIS ROUND RATHER THAN OFF A NOTE.** The state file
and the last three log entries said 63 numbered and 44 open, which counted three files and left
out audit 4's findings 64 to 80. From this entry on it is four files:

```
steps/audit-kpi.md     29 numbered   8 FIXED    1 to 29
steps/audit-kpi-2.md   20 numbered   9 FIXED   30 to 49
steps/audit-kpi-3.md   14 numbered   2 FIXED   50 to 63
steps/audit-kpi-4.md   17 numbered   1 FIXED   64 to 80
TOTAL                  80 numbered  20 FIXED   60 OPEN
```

Contiguous 1 to 80 with no gap and no repeat. **Nothing was closed, renumbered or reordered
except marking 66 FIXED**, and the old entries are left alone.

**NOTHING HERE WAS RUN IN REVIT.** Every number below is off a test, off a file in this repository
or off a measurement Bader made. `steps/2026-09-15-kpi-fixes.md` is the run sheet.

### 1. The text was too big for the PDF boxes

**Measured by Bader on ANH-007-MO-100011**, a DAILY MOSQUE plot on the Open spaces form: Area
showed 2797.6 cut off at the box edge, Total areas to be greened showed 0.0008 cut off, and the
tree and shrub counts were drawn taller than their boxes.

**NOTHING IN THIS TOOL HAD EVER READ OR WRITTEN A `/DA`.** `PdfFormFile.WithValue` drops each
field's `/AP` and the AcroForm gets `/NeedAppearances true`, so the viewer redraws the value in
the field's OWN default appearance, which nothing looked at. And the rectangle read took the first
two numbers of `/Rect`, the position the form check compares, so the box's SIZE went past
unnoticed.

`PdfTextFit`, `PdfFontWidths` and `StandardFonts` are new. The rule:

```
across  = the box width less 2 pt each side
down    = the box height less 2 pt top and bottom
em      = the text measured in the font the /DA names
wanted  = the smaller of across / em and down, rounded DOWN to 0.1 pt
ceiling = the client's own size, or 10 pt where their /DA gives nought
floor   = 6 pt
```

**The hand worked case, which is the test:** `2797.64` in Helvetica is six digits at 556 and a
full stop at 278, so 3.614 em. A box 30 pt wide and 14 pt tall leaves 26 pt across, 26 over 3.614
is 7.1948, **written as 7.1**.

**THE VALUE IS NEVER TOUCHED.** Only the size inside the field's own `/DA` moves, in the appended
object. The client's font name and colour go through untouched, NeedAppearances stays, and no
appearance stream is written. Where a field inherits its `/DA`, the inherited string is written
onto that field with the one number changed, because a size is per field.

**A WIDTH IS READ OR IT IS UNKNOWN AND NEVER GUESSED.** The font object's own `/Widths` off the
AcroForm's `/DR /Font` first, then the published metrics of the standard fourteen. **No near miss
is in that table**: Arial and Courier New are metric compatible with Helvetica and Courier and
neither is written in, because a name standing in for a measurement is the fault this repository
keeps paying for. **The standard fourteen tables are held as DATA and are not measured in this
repository**, since no AFM file is here, and that is written down rather than left to be assumed.
What a wrong width can cost is bounded on purpose: the value never changes, the size is never
raised above the client's own, and a width out by a few thousandths moves a size by a tenth of a
point. The read back off the written file is what says which size landed.

**SIX OUTCOMES AND THE BRIEF NAMED FIVE.** An absent `/DA` is the sixth, counted apart, because a
font nobody could measure and a field with no size to change are different facts. `THE TEXT SIZES`
in the glance counts all six and names a box only for held at 6, widths UNKNOWN and no `/DA`.

Break watch: the width bound taken out so the size comes back unchanged reddened 4 of 2042, and
the 7.1 case read `Area, holding '2797.64' in a box 30 pt across and 14 pt tall, came out at 10 pt
and capped at 10.` Restored byte for byte, md5 `c80ef410381ff49c46bccd31759e0fef`.

### 2. Streets, Total areas to be greened

**Bader's decision of 15 September, and it closes the open question.** The Roads note names the
canopy cell where the other two name Total Green cover, and the client meant one number on all
three. `PdfFill.Greened` has one path now and is not asked which form it is.
`PdfFill.RoadsNamesTheCanopyCell` is DELETED, and it is the second reason a thing gets deleted
rather than the first: the shape it recorded is gone, not its last caller.

**The streets template already agreed.** `ComputedPlaces` holds its `D9 = F9+F11+H11`, which is
exactly what `GreenCover.Total` computes, so the Roads PDF and the workbook filed beside it
carried two numbers for one quantity until now.

Hand check off Bader's screenshot of ANH-007-ST-100308: canopy 984, planting 69, lawn empty, Total
Green cover 1,053, and the PDF reads **0.001053** and not 0.000984.

**`TheRoadsFormIsFilledFromItsOwnNoteAndSaysSo` asserted 0.00055 and now asserts 0.00102**, which
is 550 plus 410 plus 60. **It changed because of the decision and not to make a test pass**, and
the test is renamed to say what it now pins.

**The note at `PdfForms.cs` is kept exactly as the file holds it**, spaces and all, because
`PdfFormCheck` compares notes. **The canopy guard and the Total Green cover cell check are
unchanged on all three forms**: a plot with a tree on rows 85, 92 or 99 still writes nothing here,
which is Bader's decision of 15 September and stands.

Break watch: the canopy only branch put back reddened 4 of 2042, and the 984 case read
`Expected: "0.001053" Actual: "0.000984"`. Restored byte for byte, md5
`6fab9b37501c2911a0b82f4133a185d6`.

### 3. The plot list file, and audit 4 finding 66 with it

**The team sent 154 plots to export and wants every one exported with none skipped.**

`PlotListFile`, `PlotListRead` and `TickingTheList` are new in Core. **THE FILE NEVER ENTERS THIS
REPOSITORY**: only the pointer, `kpi-plot-list.txt` beside the installed assembly, and every test
writes its own list into the temp folder.

Six rules: one plot a line, edge whitespace off, blanks skipped, the file's order kept, a line
that is not a plot named with its line number, a plot listed twice named with both and counted
once, compared with `StringComparer.Ordinal` because that is what `PlotTicks` and
`PlotsInTheModel.Holds` use, and a file that cannot be read refusing with its reason rather than
reading as an empty list.

**Tick the list sits beside Select all and Clear and behaves the same way**: it REPLACES every
tick and FORGETS every hand choice, because a person pressing it is saying these are the plots.

**Nothing on the list drops out without a line above Create**: a listed plot the model does not
name, a plot listed twice, a line that is not a plot, and a listed plot the press would put into
no workbook or no PDF. That last one asks `PlotsPerTemplate.For` and `PdfForms.ForPlot`, which are
the two rules that really decide it, rather than a third written beside the button.

**`THE PLOT LIST` opens the report**, beside `EVERY PLOT THE TOOL OFFERED`. They are different
questions printed apart: that one is the MODEL's list and this is the TEAM's. One row per listed
plot in the file's order with in the model, ticked, workbook, PDF and why not, then the model's
plots not on the list, then three counts.

**The browse line is ONE method now, audit 4 finding 77's own remedy.** It was written out four
times and the copies had drifted: one warned when the chosen folder was the templates folder, one
warned when a remembered file had gone, and one did neither. All four go through `BrowsedLine` and
each keeps its own lines under it. **Finding 77 is NOT marked FIXED**, because this round was told
to mark 66 and nothing else, and it is named here for Bader to mark.

**AUDIT 4 FINDING 66 IS FIXED.** `GuardedWrite` wraps the `OnePlot` call. The per plot WRITE loop
was added when a workbook became one plot and had no guard, so a throw on plot 100 of 154 unwound
to `Run`'s catches, which say Revit refused that with no plot named and **write no report at
all**, leaving 99 workbooks and 99 PDFs in the client's folder tree with no record of which plots
those were. The plot is named with the exception's type and message now, the run carries on, and
the report is written either way.

The worked example, every value by hand: a list reading HF-01, then NS-41 with spaces round it,
then a blank line, then NS-41, then XX-99, then the words not a plot. Against a model naming
HF-01, NS-41 and EP-05 it ticks HF-01 and NS-41, names NS-41 on lines 2 and 4, names XX-99 as not
in the model, names line 6 as not a plot, and names EP-05 as in the model and not on the list.

Break watch: the reader made to drop the last plot line reddened 4 of 2042, and
`TheFilesOrderIsKept` read `Expected: ["ST-09", "EP-01", "DM-11", "NS-02"] Actual: ["ST-09",
"EP-01", "DM-11"]`. Restored byte for byte, md5 `773fecb415d9abde50f1c7153cbb7a2d`.

### 4. A dash row counts as shrubs, by its phase

**Bader's decision of 15 September, off his own record: 44 schedule rows across the model carry a
botanical name of a dash**, and four plots came out blank because of them, EP-01, EP-09, EP-14 and
HF-01, which are the four the eighty seventh pass made refuse. His screenshot of
ANH-007-HF-100002 shows all four shrub boxes empty.

A dash row counts as `SHRUBS` and goes by its phase like any `SHRUBS:` species.
`SpeciesPrefix.CountsAsShrubs` is the one method both halves of the rule ask, so the dash cannot
be added in one place and forgotten in another.

**ONLY THE DASH, and it is the WHOLE cell once the edges are off.** A name with no colon that is
not a dash, and a prefix that is neither, refuse exactly as they did:
`AnUnprefixedSpeciesGoesNowhereAndIsNamed` is green and unchanged, and
`ARealUnknownPrefixStillRefusesAndNothingIsWritten` keeps the old refusal on the old fixture. A
dash INSIDE a name is untouched, `ACACIA / VACHELLIA FARNESIANA` and
`CARISSA MACROCARPA - GRANDIFLORA` included. A dash row under a phase the template leaves out
stays out, and **a dash row under no phase row still refuses**, because nothing says whether it is
existing or proposed and that is a different gap from not knowing its kind.

HF-01 by hand, the one plot whose split is known: group total 283, GROUND COVER 52 under Proposed,
dash rows 231 under Existing.

```
Existing Shrubs  231      Proposed Shrubs  0      TOTAL Shrubs  231      Ground Cover  52
231 plus 52 is 283
```

**The `Losing` fixture's unplaced row was `UNKNOWN PREFIX SPECIES` under Proposed, which is not
the model's value.** HF-01 is rebuilt as a dash under Existing and `Losing` is kept for the one
test that a real unknown prefix still refuses.

**WHICH PHASE EP-01, EP-09 AND EP-14 CARRY THEIRS UNDER IS UNKNOWN FROM THIS REPOSITORY.** The
08:38 report is under `reports/` and nothing there is ever committed, so those three pin only that
they add up and write, and not which of the two shrubs boxes took the area. The new dash rows
block answers it on the next run.

The plots that split cleanly do not move, checked: FP-16 335, FP-22 427 and 1114 against 1541, and
NP-100002 248 and 652 against 900.

**EVERY DASH ROW IS ON THE RECORD.** `THE ROWS WHOSE BOTANICAL NAME IS A DASH, COUNTED AS SHRUBS`
sits beside `THE SPECIES NO PREFIX PLACED, BY NAME` with the plot, the row, the phase, the area
and the box, which is Existing Shrubs, Proposed Shrubs, no box because the template leaves its
phase out, or no box because it sits under no phase row and the plot refuses.

**A DASH CAN ALSO REACH THE TREE LISTS AND NOTHING THERE CHANGED.** Traced rather than assumed.
`ScheduleRows.cs:329` reads the botanical name off the column the heading row names and `:330`
tests it for whitespace, so a dash is a species row in the softscape schedule too. It merges
through `KpiMerge.Species`, reaches `SpeciesMatching.Against` at `:317`, matches no name in the
workbook's list and no alias, and falls to `WrittenInto` at `:333`, which asks the canopy diameter
at `:434` and answers `NotSized` where the model prints none. **So a dash tree takes an empty row
carrying the name `-` and its count, or is named as unsized.** Unchanged this round.

Break watch: the dash rule taken out of `CountsAsShrubs` reddened 10 of 2042, and
`HfOneWritesItsFourBoxesOffTheDashRows` read `ExistingShrubs was not written. 1 species row could
not be placed in either box ... 231 m² is unaccounted for of the 283 m²`. Restored byte for byte,
md5 `715a77de61330fcc94defbe4283731cd`.

### What is UNKNOWN and what would settle it

- **Which phase EP-01, EP-09 and EP-14 carry their dash rows under.** The dash rows block on the
  next run settles it.
- **What fonts the client's forms really use.** No client PDF is in this repository. The widths
  UNKNOWN count in the glance names any font this tool cannot measure, on the next run.
- **Whether any client field carries a `/DA` size below 6.** None is measured here. The ceiling
  wins over the floor where they disagree and the box is named either way.
- **Whether the standard fourteen width tables are right to the thousandth.** They are the
  published core font metrics held as data and no AFM file is in this repository. The read back
  says which size landed and the value is never changed, so the cost of a wrong width is a size out
  by a tenth of a point.
- **Whether DM-21, NS-16, NS-20, ST-23 and ST-24 belong on the team's list.** IN THE MODEL AND NOT
  ON THE LIST names them on the next run and it is the team's call.

### Files

Core: `PdfTextFit.cs`, `PdfFontWidths.cs`, `PlotListFile.cs`, `TickingTheList.cs` new, and
`PdfFormFile.cs`, `PdfFill.cs`, `PdfForms.cs`, `PdfChecklist.cs`, `PdfOutcome.cs`,
`RunAtAGlance.cs`, `GroundCover.cs`, `KpiCreateReport.cs`, `KpiCreateRunSet.cs`, `CreateWords.cs`.
Revit: `KpiPanel.cs`, `KpiRequestHandler.cs`, `RememberedFolder.cs`. Tests:
`PdfTextFitTests.cs` and `PlotListFileTests.cs` new, and `PdfComputedTests.cs`,
`GroundCoverSplitTests.cs`, `PdfFixture.cs`. Plus `.claude/rules/kpi-rules.md`,
`steps/audit-kpi-4.md` and `steps/2026-09-15-kpi-fixes.md`.

---

## 2026-09-15, eighty seventh pass. The check that could never fire, and the names first

**2000 tests, 1180 of them KPI, 10 added, against the 1990 main carries** at `9ab922a`, 28 hook
cases unchanged, build zero warnings. **The audit findings stay open, not renumbered, not
reordered: 63 numbered, 19 carrying a FIXED mark, 44 open.**

### First, and before the fix: what those species are called

**UNKNOWN from this repository, and there is no way round it.** The 08:38 report is under
`reports/` and nothing there is ever committed, and nothing else here holds a per plot species
list. **So the names block is built rather than the names reported**, and the next run prints
them.

**One thing the round message's own numbers DO settle, by deduction.** HF-01 read Existing
Shrubs 231 and Proposed 52 under the phase rule, so its EXISTING phase subtotal was 231, and
Existing IS a phase a tree list sheet is named for on HEALTHCARE. A `SHRUBS:` species under it
would have landed in the existing figure. It did not. **So HF-01's 231 is unplaced because of
its PREFIX and not because of its phase.** What that prefix reads is still unknown.

**THE SPECIES NO PREFIX PLACED, BY NAME** now opens the section: every distinct name, grouped by
the prefix each read, with how many rows and how much area carry it.

```
  THE SPECIES NO PREFIX PLACED, BY NAME (2 prefixes)
  the prefix each read | distinct names | rows | area | the names
  CLIMBERS | 1 | 2 | 350 m² | CLIMBERS: BOUGAINVILLEA GLABRA x2
  PALMS | 1 | 1 | 61 m² | PALMS: PHOENIX DACTYLIFERA x1
```

**It is at the TOP because it is the question rather than the detail.** One unseen prefix over
every unplaced row is a line in a table and the team's call. A dozen different things is a
naming job in the model. **The two look identical in a count and different in a list.**

### The fault, and the wording that caused it was the round message's own

**Placed-nowhere sat INSIDE the equation, so the sum always closed and the refusal could never
fire.** Four plots wrote every box short with the column reading YES:

```
plot  | existing | proposed | ground cover | placed nowhere | group total | adds up
EP-01 |    0     |    0     |      0       |    411 m²      |    411      |  YES
EP-09 |    0     |    0     |      0       |      3 m²      |      3      |  YES
EP-14 |    0     |    0     |      0       |     78 m²      |     78      |  YES
HF-01 |    0     |    0     |     52 m²    |    231 m²      |    283      |  YES
```

HF-01 went from Existing Shrubs 231, Proposed 52 and TOTAL 283 on the 18:15 run to nought,
nought and nought. **A check that cannot fail is not a check.**

**The rule now: what would be WRITTEN, plus what this template deliberately LEAVES OUT, equals
the group total.** Anything unreadable refuses the plot on its own, before any arithmetic. The
rounding room applies to the sum and never to an unplaced species, because a species nobody
could place is not a rounding difference however small its area.

### Where the round's wording is departed from, and the reason

**There are two kinds of placed nowhere and only one of them may refuse.**

```
COULD NOT BE READ    a prefix that is neither of the two, no prefix at all, or no phase row.
                     REFUSES the plot. The tool does not know what the area IS
LEFT OUT BY THIS     a SHRUBS: species under a phase no tree list sheet is named for, which
TEMPLATE             is Street Design on a mosque plot. A TERM of the sum, NOT a refusal
```

**Taken literally the fix would have written nothing on every mosque plot with a Street Design
group**, and FM-05 alone carries 459 m² of it. That is a decision Bader already made, the phase
rows have left it out and named it since, and reversing it would be a new fault of the opposite
kind rather than a fix. `APhaseTheTemplateLeavesOutIsInNeitherNumber` going green again under the
new rule is what caught it: it reddened on the first cut and the first cut was wrong.

**The two travel in separate lists and the report prints them in separate columns**, COULD NOT
BE READ and left out by this template, so the distinction is on the page rather than in anyone's
head.

### The check, written out by hand

The four that must now refuse, each asserting the group total, what the four boxes would have
read, and that nothing is written:

```
EP-01   would have read 0 m²    lost 411    group total 411
EP-09   would have read 0 m²    lost   3    group total   3
EP-14   would have read 0 m²    lost  78    group total  78
HF-01   would have read 52 m²   lost 231    group total 283
```

The three that must NOT move, every number as the round message gave it:

```
FP-16      proposed 335   ground cover   0   group total  335
FP-22      proposed 427   ground cover 1114  group total 1541
NP-100002  proposed 248   ground cover  652  group total  900
```

`HfOneWritesNoneOfItsFourBoxesAndTheFormSaysWhy` then checks all four PDF boxes on HF-01 are
blank and each carries `231 m² is unaccounted for of the 283 m²`.

### The break watch

**Broken: the unreadable refusal switched off AND the unplaced area put back inside the
equation**, which is the fault exactly as the 08:38 run measured it.

```
6 red of 2000
  TheFourPlotsThatLostTheirAreaNowRefuseAndNothingIsWritten, all four cases:
    HF-01 lost 231 m² and the check still said it adds up. The four boxes would have been
    written reading 52 m² against a group total of 283 m². An area placed nowhere is a
    disagreement and never a term of the sum.
    EP-01 lost 411 m² ... would have been written reading 0 m² against a group total of 411 m².
  AnUnprefixedSpeciesGoesNowhereAndIsNamed
  HfOneWritesNoneOfItsFourBoxesAndTheFormSaysWhy
```

**Each red names the plot, the area lost and the number the box would have carried**, which is
the whole fault in one sentence. Restored byte for byte, checked with diff and md5
`ce641093a192f53beb01fad56cbd1f90`, and rerun green at 2000.

### What worked, on the record

The water demand is on every PDF: **2.1 on ANH-007-MO-100001, 48.752 on ANH-007-NP-100002 and
2.252 on ANH-007-ST-100003**, and the split is right on every plot where every species carries a
known prefix. Neither was touched this round.

### Open question for the team

**Whether the unplaced prefix is a fifth kind rather than a fault.** The next run's names block
answers it, and if it reads one unseen prefix across the run the answer is one line in
`SpeciesPrefix` rather than a refusal. That is Bader's call once he can see the name.

---

## 2026-09-15, eighty sixth pass. Ground cover out of the shrubs box, and item 1 already shipped

**1990 tests, 1170 of them KPI, 26 added, against the 1964 main carries** at `8367b30`, 28 hook
cases unchanged, build zero warnings. **The audit findings stay open, not renumbered, not
reordered: 63 numbered, 19 carrying a FIXED mark, 44 open.**

### Item 1, the irrigation water demand, was built last round and is on main

**It is `8367b30`, the eighty fifth pass, and nothing about it needed doing again.** Checked
point by point against the brief rather than assumed:

```
both schedules' own TOTAL rows, added, divided by 1000    WaterDemandRead.From    YES
the L/DAY heading matched WHOLE, none and more than one   ScheduleColumns.Reading YES
   both refused with the headings printed
never the species rows added up                           NoTotalRow              YES
all three forms                                           PdfForms.All            YES
a missing schedule or an unreadable total writes nothing  PlotWaterDemand.Why     YES
   and is named with which half
the value read back off the output                        PdfChecklist:176        YES, every
   written field goes through the same read back                     field, no special case
the report says the FORM's unit                           WaterDemand.CubicMetresUnit YES
the groups left out of the tree lists, per plot,          THE IRRIGATION WATER    YES
   with their subtotals                                   DEMAND, PER PLOT
```

**The Open Spaces field name is still what was measured off that file on 14 September**, and it
still cannot be re-measured here because no client PDF may enter this repository. `PdfFormCheck`
is what protects it: a form not carrying every field the table names writes NOTHING.

**DM-11's softscape L/DAY total is still UNKNOWN**, for the same reason as last round, the 1548
scan being under `reports/`. The 432 and 908 are two totals and the schedule has one: if they
are the two group totals the TOTAL row reads 1340, that is what the tool takes, and the report
prints the row it read so the first run settles it without argument.

One break was watched again to prove the rule still holds, under the break watches below.

### Item 2, ground cover was in the shrubs box

**DM-14 is the plot that shows it.** Its SHRUBS & GROUND COVER group holds
`GROUND COVER: CARISSA MACROCAPA` 270 and `GROUND COVER: LAMPRANTHUS AUREUS` 198, adding to the
printed group total of 468, and **every species in it is ground cover and none is shrubs**. All
468 went into Proposed Shrubs and Ground Cover was left blank. DM-11 is the opposite end, two
`SHRUBS:` species adding to 70.

**Only one of the two readings adds up.** Putting the group total into Ground Cover as well
would give DM-11 Ground Cover 70 beside Proposed Shrubs 70, the same area in two boxes of one
form. The prefix split gives each box its own species and the two add to the group total.

`SpeciesPrefix` and `GroundCoverSplit` are the rule. The prefix is the text before the FIRST
colon, matched whole and without case through `LabelText.Same`, the same rule every label lookup
here already asks. A species with no prefix, or a prefix that is neither of the two, goes
NOWHERE and is named with its plot, its row, its area and what its prefix read.

**The two figures plus everything placed nowhere must equal the group total the schedule
printed, or NONE of the four boxes is written.** Writing three and blanking one would leave a
form whose own numbers disagree with the schedule behind it. The room is the phase rows' own,
half the project's area rounding step per row summed, which is why the area unit is handed into
`PdfFill.Of` rather than a tolerance being written in the new file.

**The shrubs figures come off SPECIES ROWS now and not off PHASE ROWS**, which is the whole
fault: a phase row holds shrubs and ground cover added together. Existing against proposed is
still `CountedGroups.SheetFor`, the same method the tree lists ask, so nothing about that rule
is written twice.

**The GRASS group is untouched.** DM-11's lawn is still 35 and DM-14's still 175, both pinned.

### One thing deleted, and it is the second reason rather than the first

**`PdfFill.GroundCoverIsNotPrintedApart` is gone.** It read that the schedule prints the group as
one and nothing prints ground cover on its own, so it is not derived. **The shape it recorded is
gone rather than its last caller**: every species row in that group begins `SHRUBS:` or
`GROUND COVER:`, so the two ARE printed apart and the number is read rather than derived. A
constant recording a claim the data disproves is worse than no constant, which is what
`OutputName.Suggested` bought this project.

### The check, written out by hand and both plots pinned

```
DM-11   group total 70    shrubs 70    ground cover 0      its shrubs figure does not move
DM-14   group total 468   shrubs 0     ground cover 468    468 out of shrubs, 468 into cover
```

`OnDm11TheWholeGroupIsShrubsAndItsFigureDoesNotMove` and
`OnDm14TheWholeGroupIsGroundCoverAndTheShrubsBoxGoesToNought` build both schedules as the model
prints them, eleven columns and all, and run the real reader over them.
`TheTwoPlotsFillOppositeBoxesOnTheForm` then checks the numbers reach the form's own boxes,
which is the thing a client reads.

### How big is it, and the honest gap

**UNKNOWN from this repository, and here is exactly why.** The brief's own measurement off the
05:49 report is 143 species names beginning `SHRUBS:`, 107 beginning `GROUND COVER:` and 55
beginning `GRASS:` over 156 plots. **That is a count of NAMES and the question is a count of
PLOTS**, and the report it came off is under `reports/`, where nothing is ever committed, so it
cannot be turned into one here. Nothing in this repository holds a per plot species list.

What can be said from the two numbers without inventing anything: 107 ground cover names over
156 plots means ground cover is not a rare case, and **every workbook and form written so far
has the two in one box.**

`SHRUBS AGAINST GROUND COVER, PER PLOT` is the answer rather than a guess. It prints one row per
plot with both figures, what was placed nowhere, the group total and whether it adds up, so the
next run says how many plots are all one kind and how many are mixed without anybody counting.

### The break watches, three of them, one per rule

```
BREAK 1  a species that is not ground cover falls into shrubs, the old rule
   2 red of 1990
   AnUnprefixedSpeciesGoesNowhereAndIsNamed        Expected: 36   Actual: 45
   APrefixThatIsNeitherIsAlsoPlacedNowhereAndNamedWithWhatItRead

BREAK 2  ground cover species go to the shrubs box, the fault exactly as it ships
   3 red of 1990
   OnDm14TheWholeGroupIsGroundCoverAndTheShrubsBoxGoesToNought  Expected: 468  Actual: 0
   TheTwoPlotsFillOppositeBoxesOnTheForm                        Expected: "468" Actual: "0"
   TheReportPrintsTheSplitPerPlotAndNamesWhatItCouldNotPlace

BREAK 3  item 1: no TOTAL row falls back to adding the species rows
   1 red of 1990
   AScheduleThatPrintsNoTotalRowIsNamedAndItsSpeciesRowsAreNotAddedUp
     the schedule printed no TOTAL row and the reader produced 2304 L/day anyway, off row 1.
     Take the total means take the total, and 468 plus 684 is a number this tool worked out
     rather than one the schedule printed.
```

**Break 1's first red names the fault by its numbers**: the unprefixed 9 m2 swallowed into the
shrubs box turns 36 into 45. **Break 2's names the plot and the box**: DM-14's ground cover goes
from 468 to 0, which is the wrong box this round exists to empty. **Break 3's assertion was
strengthened first**, because a bare `Assert.False` reads `Expected: False, Actual: True` and
says nothing about what broke.

All three restored byte for byte and checked with diff and md5,
`bc824e791d0a099a611531944d930a8e` for `GroundCover.cs` and `68acb06b8e5f09ac4eab1d5f32b591b5`
for `WaterDemand.cs`, and rerun green at 1990.

### Eight tests moved and none of them was weakened

Giving the shrubs group its species rows moved eight cases that built a group out of phase
subtotals alone. **Six now pass unchanged** because each phase carries one `SHRUBS:` species
holding its whole area, which is what those cases always meant. **Two had their subject
reversed and are rewritten rather than deleted**: `GroundCoverIsLeftBlankAndNamedRatherThanDerived`
is now `GroundCoverTakesTheGroundCoverSpeciesAndNoughtWhereThereAreNone`, and the two asserting
the old constant now assert the absence of the group instead, which is the honest sentence for a
plot whose schedule printed none.

### Open question for the team

**If the split disagrees with the group total on a real plot the tool STOPS**, writes none of the
four boxes and names the plot and all three numbers. That is deliberate and it is a question for
Bader and the client rather than one this tool settles.

---

## 2026-09-15, eighty fifth pass. The irrigation water demand, off both schedules' TOTAL rows

**The last PDF field with a real source that nothing read.** **1964 tests, 1144 of them KPI, 25
added, against the 1939 main carries** at `dbae2e6`, 28 hook cases unchanged, build zero
warnings. **The audit findings stay open, not renumbered, not reordered: 63 numbered, 19 carrying
a FIXED mark, 44 open.**

### What it does

Per plot, out of both schedules, the L/DAY column, **TAKE THE TOTAL OF ALL**, add the two, divide
by a thousand. `WaterDemandRead.From` is the whole rule, `PlotWaterDemand` holds both halves and
the sum, and `PlotReading.Water` carries it from the read to the fill and to the report.

**Nothing adds the species rows up anywhere.** Bader said take the total, and a sum this tool
worked out is a different number from one the schedule printed. A schedule with no TOTAL row
writes nothing for that half and is named, with the species rows sitting right there untouched.

### The column, and why it is matched whole

**Three columns of the softscape schedule hold the word WATER and only one reads L/DAY**, with
`WATER DEMAND` and `WATER L/TREE/DAY` beside it and `WATER L/SQM/DAY` on the other schedule. So
`ScheduleColumns.Reading` matches the WHOLE heading through `LabelText.Same`, the rule every
whole-label lookup here already asks, and **none and more than one are both refusals with the
headings printed**. This is the two DIAMETER headings again, which cost a round.

The TOTAL row is found by its first cell, read off `SoftscapeRows.TotalMark` so the two cannot
part. That is the row's shape rather than a value read by position, which is the same ground
`IsStructureRow` already stands on.

### Two things reported and not decided

**a. The TOTAL includes every group and the tree lists do not.** A mosque plot's Street Design
group is out of scope for the tree lists by Bader's decision and its water is inside the TOTAL.
The number written is the total he specified and nothing subtracts anything, and
`THE IRRIGATION WATER DEMAND, PER PLOT` carries a column naming every group that plot left out
with that group's own subtotal, beside the demand that counts it. The difference is on the page.

**b. A schedule with no TOTAL row.** **UNKNOWN whether any real one prints none**: every schedule
shape measured so far prints one, the 1548 scan is not in this repository, and the reader's own
refusal is what will say so on the first run. It writes nothing for that half and names it.

### One thing changed that the round did not name

**It prints FINE rather than to two places.** `PdfFill.Number` rounds to two decimals and every
other whole-unit field goes through it. Dividing litres by a thousand makes this one small: 2492
L/day is 2.492 and two places would send **2.49** to the client, two litres a day thrown away on
every plot of a hundred and fifty. It goes through the same `Fine` the green cover and the road
length already use, and `Fine`'s own docstring already carries the reason.

### The Open Spaces field name

`PdfForms.OpenSpaces` names it `Irrigation water demand`, which is the name measured off that
file on 14 September. **I cannot re-measure it here and never will be able to**, because no
client PDF may enter this repository. That form does name five of its fields badly, `undefined_4.0`,
`Proposed Trees.1`, `0` and `0_2`, which is why nothing on it is taken from what a field is
called on the other two. **What protects it is `PdfFormCheck`**: a form not carrying every field
the table names writes NOTHING rather than writing into the wrong box, so a wrong name here is a
blank field and a named refusal rather than a number in a client's document.

### What DM-11 will read, and the check Bader asked for

He measured DM-11's shrubs and lawn schedule printing **L/DAY totals of 432 and 908**.

**432 and 908 are two totals and the schedule has one.** If they are the two GROUP totals, GRASS
and SHRUBS & GROUND COVER, then the schedule's own TOTAL row reads **1340** and 1340 is what the
tool takes: never 432, never 908 and never the species rows added.
`OnDm11TheScheduleTotalIsTakenAndNeitherGroupTotalIs` pins that over a schedule built to that
shape, asserting 1340 and asserting it is neither of the two. If the TOTAL row itself reads one
of them, the tool takes that one. **The report prints the ROW each total came off, so the first
run settles which reading is right rather than this round assuming one.**

**DM-11's softscape L/DAY total is UNKNOWN from here**, because the 1548 scan is not in this
repository. The field comes to `(softscape + shrubs) / 1000`. On the 1340 reading a softscape
total of 1152 gives **2.492**, and `BothTotalsAddedAndDividedByAThousandGiveWhatTheFormAsksFor`
writes 1152, 1340, 2492 and 2.492 out by hand.

### The break watch

**Broken: `ScheduleColumns.Reading` made to take the FIRST of two matching headings** rather than
refusing, which is the exact fault the round warns about.

```
2 red of 1964
  TwoColumnsReadingLitresADayRefuseAndNameBoth
    two columns read L/DAY and the reader took one of them anyway, 1152 L/day off row 2.
    More than one is a refusal and never the first.
  MoreThanOneMatchingHeadingComesBackAsMinusTwo
    Expected: -2   Actual: 0
```

**The first red names what was broken in its own message rather than in its name alone**, which
is why the assertion carries one: `Assert.False(read.Read)` reads `Expected: False, Actual: True`
and says nothing. Restored byte for byte, checked with md5 `096037650ddefaf26bf40f276df9b697`
and with diff, and rerun green at 1964.

### Open question for the team

Whether the water demand should count the groups the tree lists leave out. It does today, because
that is what TAKE THE TOTAL means, and the report names every such group and its subtotal per
plot so Bader can see the difference and say.

---

## 2026-09-15, eighty fourth pass. The fourth audit of the KPI tool, a firm's review

**This round writes no code.** It reads the project the way a software firm reads a codebase
before taking it over, over eight areas worked one at a time, and writes
`steps/audit-kpi-4.md`. **1939 tests, 1119 of them KPI, against the 1939 main carries** at
`f2e2f42`, 28 hook cases, build zero warnings, all unchanged because nothing was built.

**The three earlier audits stand untouched: 63 numbered findings, 19 carrying a FIXED mark, 44
open**, derived by walking the files rather than off a note. None is closed, renumbered or
reordered, and none is reported again. Seventeen new findings, 64 to 80: one BLOCKS, three
WRONG, ten COSTLY, three TIDY, with nine dropped for having no cost to the user.

### What the audit found that was not known

**The client's project name, their consultant and a real contract reference are constants in a
public repository**, `PdfForms.cs:224`, `:226` and `:235`, against `.gitignore:46-49`, which
names those exact three facts as the reason no client PDF may enter. The rule they serve is
sound and the strings do not have to be here to serve it.

**A throw while reading a schedule's plot filter comes back as an empty string**, and an empty
string means the schedule belongs to no plot, so a plot's trees go missing and nothing records
it. **The per plot WRITE loop has no guard**, so a throw at plot 100 of 156 ends the press with
99 workbooks already written and no report at all. **Three of the five .xlsx readers do not
catch `XmlException`** and the two that do are the two that write.

**`CLAUDE.md` holds the string PDF zero times** and the tool fills a client AcroForm for every
plot out of 2,149 lines. **`ReportPlaces` sits in Drawing Sheet's folder and is called by three
tasks**, which is `territory.md`'s own worked example, unmoved.

### Two things measured in a copy of the tip, never here

Adding an eighth template reddens **8 of 1939**, and after filling in what those name, **10
more**, each naming the next table. Adding a field to a PDF form reddens **2**, both counts, and
neither is the missing case in `PdfFill.One`. **The template tables are held level by the gate
and the PDF fields are not**, and the pattern that would catch it is already in this repository
twice on the Drawing Sheet side.

### The debt answer, as numbers

```
                    Core/Kpi  test files  KPI src lines  KPI tests  tests per 100 lines
audit 1  92dd36c         57          24         12,575        429                 3.41
audit 2  82d95f4         59          26         12,987        459                 3.53
audit 3  f3fe456         78          57         21,887        853                 3.90
today    f2e2f42         96          76         28,401       1119                 3.94
```

**The code is not accumulating debt faster than it clears it.** Source +126 per cent, tests +161
per cent, open findings per 1,000 lines flat at 2.31, 3.62, 1.46, 1.55. **The backlog is: 63
opened against 19 closed, and 6 rounds of 42 since the first audit closed anything.** Six of the
44 open are marked PASSED BY against code that no longer exists, so the honest number is 38 open
and 6 unresolvable.

### What was checked and is clean

Secret scan over the WHOLE history: 1,942 distinct blobs, 78,220,014 bytes, 1,941 text blobs
scanned against thirteen patterns. **Zero credentials, ever**, and the only emails are
example.invalid placeholders. **No `.xlsx` and no `.pdf` has ever been committed**, over 203
commits. Core names no Revit type, proved four ways including off the built assembly, which
lists `netstandard` and nothing else. Every crossing between Kpi and Shared runs one way.

### Open question for the team

Whether the three header values leave the repository for a file beside the installed assembly.
It trades a rule measured off the client's own files for a file the team has to keep, and it is
Bader's call rather than this round's.

---

## 2026-09-15, eighty third pass. The held off list, in step and on screen

**Pull request 138, merged into main as `2775999`.** The runner ran 28 hook cases and 1939
tests against the pull request head `7800ce8`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1939 tests, 1119 of them KPI, 5 added, against the 1934 main carries** at `a3d9896`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

All three items came out of the eighty second pass's own verifiers rather than off a run, and
they are one fault: **nothing kept the hand choices and the real ticks in step, and nothing ever
printed them.**

### The one record

`HandTicks` replaces the bare set of plots taken off. It holds BOTH directions, a plot taken off
or put on by hand and never both, immutable the way `PlotTicks` already is, and
`KpiPanel.TickedByHand` moves the ticks and the record together so nothing sets one without the
other.

### 1. Unticked was not symmetric with Ticked

`Ticked` took the held off list and `Unticked` took nothing. So **a hand untick survived a row
tick and a hand tick did not survive a row untick.** A person who picked three mosque plots by
hand, ticked the MOSQUES row for the rest, then changed their mind about the row, lost their three
with nothing said.

`Unticked` takes the record now and keeps the plots put on by hand, so unticking a row undoes
exactly what ticking it did and ticking it again restores the same state. A test writes both
directions out at once: DM-12 put on by hand and DM-14 taken off, tick the row, untick it, and the
ticks are back where they started.

### 2. Select all and Clear, decided once

**I agree about Select all and the same argument carries Clear**, which is the half the round
message did not ask about. Both replace every tick, so a per plot choice left standing behind
either is a record that disagrees with what is on screen, and the next row press acts on the
disagreement. **Clear leaving the record standing would mean Clear then tick MOSQUES silently
drops DM-14**, which is the same invisible drop as the Select all route one press later. Both
forget every hand choice, so there is one rule rather than two and no path disagrees with another.

The test writes out what it did before as well as what it does now, so the fault is on the record
beside the fix: carrying the old record past Select all drops DM-14 on the very next row press.

### 3. SomeOfThem builds the sentence and nothing called it

Its only references were `TickingATemplateTests.cs:112` and `:116`. **Green, and it had never
reached a screen.** It is on the workbook row now, under the tick box, as a NOTE rather than a
refusal, and a row where every plot is going in gets no line because a line about nothing is one
the team reads past on every other press.

**The pane counts nothing.** `TickingATemplate.RowLine` takes the template, the ticks and the
component reader and hands back the sentence, so the two counts come off the split's own rule
rather than off a loop beside the control that draws them. That is also what made item 3 testable
in Core at all.

**It is the line the round before spent itself looking for.** That round asked a 57,143 line report
why MM-09 to MM-15 were unticked, and this answers the held off half of it on the row a person is
looking at when they press.

### How many other members build a line the pane never shows: SIX, and three of them are lines

**Counted rather than guessed at.** Every public member of `Core/Kpi` returning a string or a list
of strings, held against every reference in `src` outside its own declaration, doc comments left
out. **245 such members. SIX have no reference at all**, and every one of the six is tested green.

```
KpiPaneWords.ModelNamed        a line       the model's name or the words for none
CreateWords.SuggestedName      a line       the name a box offered, and THE BOX IS DELETED
RegionChoice.WhyUnchosen       a line       why no region was chosen, for the report's own column
ComponentTemplates.ValuesFor   a list       its docstring says the report and the pane say it
PlotPrefixes.PrefixesFor       a list       DELIBERATELY KEPT and already recorded as uncalled
WorkbookPatcher.ReadBack       not a line   a cell read off a written file
```

**`WhyUnchosen` is the one worth acting on, and it is not acted on this round** because it was not
what was asked for. Its own docstring says it exists "for a report that would otherwise print an
empty cell and leave somebody guessing which of the two cases it was", and nothing calls it, so
**the report prints the empty cell.** `SuggestedName` is the deleted name box's and its shape is
gone with the box. `PrefixesFor` is the one already recorded as deliberately kept.

### The open question beside it: is that enough for a route beside every unticked plot

**Not quite, and one thing is still missing.** Items 1 to 3 close the hand route: a plot unticked
by hand is visible on its row now and the record cannot drift, so an unticked plot is no longer
one of three unexplained cases. What is left is the two the split decides, the component not in
the table and the component disagreeing with the prefix, **and the tool does know both at tick
time** through `PlotsPerTemplate.For`, whose `Why` is already the sentence.

**What is missing is the component for a plot nobody ticked.** The correction recorded last round
stands: `set.Runs` to `Readings` to `PlotReading.Component` reaches the component with no new
argument, and it reaches it **for ticked plots only**, because an unticked plot is never read and
has no reading at all. The plots the column exists to explain are exactly the ones that route
cannot reach.

**So it needs the component per plot read beside the plot list, off ONE read at the press**, the
way `PlotOrigins` already reads the list off the live document. That is one read and one argument
and it is a round of its own. **Not built this round**, as asked.

### Three break watches, one test red each

```
1  Unticked ignores the hand record      UntickingARowKeepsAPlotThePersonPutOnByHand
                                         RED: Expected ["DM-12"], Actual []
2  Forgotten hands back itself           SelectAllAndClearForgetEveryHandChoice
                                         RED: Expected Count = 0, Actual Count = 2,
                                         Off = ["DM-14"], On = ["DM-12"]
3  the row counts what could go          TheRowSaysHowManyOfItsPlotsAreGoingInAndSaysNothing...
                                         RED: Expected "MOSQUES: 2 of the 3 plots...", Actual ""
```

Every one names what was broken. Both files restored byte for byte, checked with `diff -q`, and
the suite green at 1939 after.

### What the correction settled, recorded beside PlotOrigins

**A SECTION THAT COVERS WHAT WENT IN CANNOT TELL YOU WHAT DID NOT.** The premise of the round
before was that MM-09 to MM-15 are in the list and not in the model. They are in the model, on a
sheet AND on a schedule, which is why they are in neither disagreement line. The evidence was that
they appear nowhere in a 57,143 line report, and **that is not evidence**: every section of that
report is over the ticked plots. PLOTS TICKED THAT WENT INTO NO WORKBOOK reading 0 is correct by
construction and was read as a fact about the model. And the two nines were a coincidence.

### For Bader, not a fault to fix, and UNVERIFIED

**Two workbook rows can settle as the same template**, if two files in the templates folder are
both recognised as STREETS, and unticking one then takes every street plot off while the other
stays ticked. **I have not measured that two files can be recognised the same way**, so it is
written here as unverified. What did change is that the state is now visible: the still ticked row
reads `STREETS: 0 of the 78 plots that belong to it, the rest unticked by hand.`

### Still open, and not guessed at

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every line of it is off the code and the two
verifier reports. The row line has never been seen on a screen.

- **The route beside each unticked plot**, which needs the plot list and the component off one read
- **`WhyUnchosen` printing the reason it was written for**, so the region column stops printing an
  empty cell
- **Which of the three routes leaves MM-09 to MM-15 unticked**, now one of two rather than three,
  because the hand route shows itself on the row
- **A species written into an empty row still carries no family, no genus and no native flag**

---

## 2026-09-15, eighty second pass. Where MM-09 to MM-15 come from, and the rows with no canopy

**Pull request 136, merged into main as `bafb39c`.** The runner ran 28 hook cases and 1934
tests against the pull request head `15e097a`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1934 tests, 1114 of them KPI, 6 added, against the 1928 main carries** at `ea7ae2e`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

### 1. They are in the model, and the round message's reading of the silence was wrong

**THE PREMISE WAS THAT THEY ARE IN THE LIST AND NOT IN THE MODEL. THEY ARE IN THE MODEL, ON A
SHEET AND ON A SCHEDULE BOTH.** All three places were checked and the third was checked first.

**Nothing derives a plot name.** There is ONE production construction of the KPI plot list from a
document, `KpiPlotReader.Plots`, and it is `PlotsInTheModel.Of(OnSheets, OnSchedules)`. The sheets
half adds `PRX_Plot_ID` as printed and the schedules half adds the filter's own string, and the
only transformation on a plot string in that chain is `Trim()`. **Every plot identifier builder in
this repository is Drawing Sheet's**, `PlotTickList.Generated` among them, which really does build
`prefix + "-" + number.ToString("00")` in a loop over a range and is exactly the shape the round
message suspected. Checked by name: **no file under `Core/Kpi` or `Revit/Kpi` mentions
`PlotTickList`, `PlotRange`, `PlotRegistry` or `PlotSelection`**, and the only formatted number in
those folders is a report column width, a report row number and a PDF cross reference offset. The
plot named `-` is a reading too, off a schedule filter holding a dash, because nothing checks a
plot against the two letters, dash, digits shape on the way into this list.

**The two disagreement lines count the plots named by exactly ONE source**, so a plot named by
BOTH is in neither of them. MM-09 to MM-15 being in neither line is what PLACES them: they are in
the intersection, on a sheet and on a schedule. Both of the first two places the round message
named are true of them at once, and being in both is exactly what made them invisible.

**The two nines are different nines.** Nine named across the two lines, nine unticked, and nothing
has ever made those agree: one is about which of two reads found a plot, the other about whether
`PlotsPerTemplate.For` placed it in a ticked template. The run proves it, because seven of the nine
unticked are in neither line and the park plots the lines do name are placed by their prefix and
are ticked.

**And the silence was not evidence.** Every section of the create report is over the ticked plots.
THE SPLIT and PLOTS TICKED THAT WENT INTO NO WORKBOOK both run off `TemplateSplit`, which is built
from the ticked list alone, so `Unplaced` cannot hold a plot nobody ticked and its 0 is correct and
says nothing. `KpiCreateRunSet` did not carry the model's plot list at all.

**Why they are unticked is six routes and it is one of three.** With every row ticked a plot goes
unticked when its component is not one of the eleven, when its component and its prefix disagree,
when it has neither, when it was held off by hand, when the ticking call did not run for that row,
or when its template has no recognised file. STREETS wrote its 78 plots, so its row was ticked and
settled, which rules out the third, fifth and sixth and leaves **the first two and the fourth**.
Being on a sheet does not narrow it further: the first two need a component on that sheet and the
fourth needs nothing, and a plot whose sheet holds no component falls to the prefix and is ticked.
**WHICH OF THE THREE IT IS CANNOT BE DETERMINED FROM THIS REPOSITORY** and is not guessed at. Worth
knowing beside it: `ValuePerPlot` reads the component off the plot's FIRST sheet by sheet number and
no other, so a plot whose sheets disagree is placed off one of them with nothing recorded, and
nobody has measured whether any plot's sheets disagree.

**What was added is the section the round asked for.** `PlotOrigins` prints one row per plot at the
top of the report, above the glance: the plot, which of the two reads named it, and whether it was
ticked. The list is read off the LIVE DOCUMENT at the press and the ticked half is counted off
`PlotOutcomes`. A plot that was ticked and that the read does not name is its own row reading NAMED
BY NEITHER SOURCE and the line says it is a bug rather than a state. The counts add up four source
states and two tick states against the row count, printed as YES or as a bug, and one sentence says
what the pane's two lines count so nobody holds nine against nine again.

**WHAT IT STILL DOES NOT SAY IS WHY A PLOT WENT UNTICKED**, and that is deliberate. The route needs
the component per plot, which the pane read at a different moment from the list this section reads
at the press, so a route worked out from the two together would be one answer off two records of
one fact. **The plot list and the component per plot travelling from ONE read is the next line and
a round of its own.**

### 2. Rows 85, 92 and 99, recorded and nothing changed

**52 of 150 forms got no Total Green cover on the 05:49 run**, and the glance line the round before
added named the reason outright: rows 85, 92 and 99 of the client's tree lists carry the numbering
column and no canopy formula. A tree on one of those rows contributes no canopy in the client's own
workbook whoever fills it. **Refusing was right, the exact text rule stays, and nothing in the tool
changed.** It is in `kpi-rules.md` with the three rows, the count and the guard's own printed line,
and Bader is taking it to the client.

**One thing the printed line says that nobody asked about.** `O85 = IF(ISBLANK(B85)," ",N85*B85)`
is the shape MOSQUES row 21 carries at `M21 = IF(ISBLANK(B21)," ",L21*B21)`, two columns right, so
on that sheet the canopy per tree sits at N and the area at O. The guard never cared, because the
only letter in the text it looks for is the DIAMETER column and that comes off the heading row.
Whether the whole sheet uses N and O or only the added rows do is UNKNOWN from one row.

### Three break watches, one test red each

```
1  the two sources swapped in PlotOrigins.Of    EveryPlotOfferedIsARowWithWhereItCameFrom...
                                                RED: EP-05 read "on a sheet and on no schedule"
                                                where it is on a schedule and on no sheet
2  a ticked plot the read does not name dropped APlotTickedThatNeitherSourceNamesIsARowSayingSo
                                                RED: "MM-09 | NAMED BY NEITHER SOURCE | ticked"
                                                gone from the rows
3  the section moved below the glance           TheReportOpensWithEveryPlotTheToolOffered...
                                                RED: the plot list must be above the glance
```

Every one names what was broken. Both files restored byte for byte, checked with `diff -q`, and the
suite green at 1934 after.

### For Bader, not a fault to fix this round

**Select all and Clear do not touch the held off list.** `_heldOff` is added to and removed from by
the per plot tick box alone and cleared only when the model changes, so a plot unticked by hand
once, then brought back by Select all, is dropped again the next time any template row is ticked or
re-ticked. Nothing on the pane ever prints that list. Found while enumerating the six routes and
left alone, because it is not what this round was asked for.

### Still open, and not guessed at

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the 05:49 report, the
pane's screenshot and the code as it stands.

- **Which of the three routes leaves MM-09 to MM-15 unticked.** One line of the next run's report
  beside one look at those sheets' `PRX_Component` settles it
- **Why the plot list holds a plot named `-`.** The schedule filter holds a dash and nothing checks
  a plot's shape on the way in. Whether it should be checked is the team's question, because the
  tool has never invented a plot and refusing one the model really holds is a different rule
- **The route beside each unticked plot**, which needs the list and the component off one read
- **A species written into an empty row still carries no family, no genus and no native flag**

---

## 2026-09-14, eighty first pass. Three faults off the 19:52 run

**Pull request 134, merged into main as `8b90fe0`.** The runner ran 28 hook cases and 1928
tests against the pull request head `25ce207`, 0 failed and 0 skipped. The merge went through
the API with the title and the message both passed on the call, the commit came back off main
carrying neither a co-author credit line nor a generated-by footer, and the merged tree is byte
for byte the branch head, checked with a diff that named no file.

**1928 tests, 1108 of them KPI, 12 added, against the 1916 main carries** at `484251e`, 28 hook
cases unchanged, build zero warnings. **No audit finding is closed, renumbered or reordered: read
off the three files, 63 numbered findings, 19 carrying a FIXED mark, 44 open.**

What landed the round before and is on the record: no instruction text in any field on any of the
seven PDFs checked, the canopy in Total Green cover with MO-100001 going 0.000105 to 0.000525
square kilometres, and every plot getting its own block, 156 of them.

### 1. The emptying took the client's own header with it

All seven output PDFs read Project name empty, Consultant empty and Contract reference empty. The
rule the round before shipped did exactly what it says and it was one field too wide: those three
are TYPED DEFAULTS rather than notes for whoever fills the form by hand.

**Project name and Consultant are left alone. Their default is a value.** Bader's answer, and they
are the only two fields in that state. Every other text field is still written or emptied, so a
box the tool has no source for still looks like a box nobody has filled.

**Nothing tries to tell a note from a value by reading the text.** The three are held as data,
`PdfForms.HeaderValues`, spelt as the files carry them.

**They are found by the VALUE, because no field name for them is measured anywhere in this
repository and none can be**, since no client PDF enters it. What makes that safe is
`PdfFormCheck` refusing a form that does not carry all three: a client who changes their header
gets a form this tool writes NOTHING into, rather than one whose header it clears or whose
reference box it writes a plot number into having lost track of which box that is.

**Contract reference is written, per plot, from PRX_Plot_NH.** Bader's decision. A plot with no
PRX_Plot_NH gets the box EMPTIED with its own reason rather than left holding the template's, which
would hand the client another project's reference under this plot's name. A form holding no
contract reference field at all is a different fact and is named apart.

Every field is in one of THREE states now rather than two, and the test that walks every field of a
built form refuses any field in none of them.

### 2. Both parks lost both computed numbers, and it is neither of the two cell checks

**The round message asked which of the two guards added last round it is. It is neither, and both
were measured rather than argued about.** A parks shaped fixture was built, the green cover label
at C9 over `D9 = F9+F11+H11` and no percentage label at all, and both were asked:

```
GreenCoverCell    Agrees, cell D9, formula F9+F11+H11, canopy cell F9
PercentageCell    NothingToCheck, and Usable
```

So `SummaryCellCheck.NothingToCheck` does tell an absence from a drift, which is what this round
was asked to check, and `TheParksShapeIsFoundAndThePercentageHasNothingToCheck` says so.

**And neither could blank BOTH fields anyway.** Read off `PdfFill` line by line there are three
gates: `WhyNothingCanBeComputed` reaches both, `GreenCoverCell.Usable` reaches the green cover
alone and `PercentageCell.Usable` the percentage alone. `GreenCover.Total` always computes and
`GreenCover.Percentage` refuses only on an area of nought, so nothing below the gates blanks both
either.

**So it is `WhyNothingCanBeComputed`, which is two things: the workbook was not written, or the
canopy guard found a drift. WHICH OF THE TWO CANNOT BE DETERMINED FROM THIS REPOSITORY**, and it
is not guessed at here. The 19:52 report already says which, per plot, on the field's own line.
Reading it out of 82,048 lines is the thing item 3 is about, and the glance line below is the
answer to that.

**What changed under the parks is the canopy guard's SUBJECT rather than its rule.** Until the
eightieth pass `CanopyArea.From` returned only rows this tool wrote, so a plot whose species all
matched gave it no rows and it answered nothing to check. It returns the matched rows now, so the
guard reads the client's own rows for the first time, holding each against the formula text
measured on one row of one template.

**The exact text rule is KEPT and deliberately not loosened.** Accepting any formula that reads
the row's own diameter cell, the way the green cover cell is checked, would be wrong: a sum of
three cells is the same sum however it is written, and a canopy formula reading the same diameter
and computing it differently gives a different number from the one this tool works out. A number
that is not the workbook's own is what the guard exists to stop, and loosening it to make the
parks fill would have been a fix for a fault nobody has measured.

**What is added is the record of whose row drifted.** `CanopyRow.TheClientsRow` travels from
`match.Added` and the guard's line reads `a row this run wrote in` or `a row the client's list
already held`. The first is the tool writing a row the workbook cannot compute, the #VALUE! fault
that took four rounds to kill. The second is the client's file computing its canopy another way,
which is a question for the team. One sentence covered both.

**What the parks templates' own canopy formula reads is UNKNOWN here.** The next run answers it,
because the guard prints every formula on the row it refused.

**And the two numbers are counted at the top of the report now.** `RunAtAGlance.Computed` says how
many forms got each and, for the ones that did not, ONE LINE PER REASON with its count and its
plots, never one per plot. The two are counted together because they fail together, past four
plots the count stands for the rest, and a field a form does not ask for is counted neither way.

### 3. The report is 82,048 lines and 42,570 are one section

**All three rules the round before asked for landed**, and the section grew eight times over
anyway. They governed the FORMULAS AT RISK list and that list is now filtered, counted and grouped.
The section's other two lists were untouched, and the per plot fix multiplied every one of them by
156. The one that carried it is `FORMULAS READING A ROW THIS RUN WROTE INTO`, 321 lines on a
single plot of the 18:15 run, which over 156 plots is 50,000 lines, against the 42,570 measured.

**The same rule governs that list now.** `FormulaRepeats.Reading` says it once per shape with its
span and its count. Two things are deliberately no part of the shape. A cell named twice in one
formula is ONE read, because `IF(ISBLANK(J4)," ",ROUND(PI()*(J4/2)^2,0))` names J4 twice and the
line printed it twice. A shared formula and its master are ONE shape, because that is how Excel
stores a filled down column rather than anything about the formula, and keying on it split every
column into two lines. Measured on a four row fixture: 10 formulas, 4 lines.

**And the file opens with what is in it.** `ReportSections.Of` reads the report's own headings and
counts the lines under each, and `WHAT IS IN THIS FILE` prints one row per section, widest first,
with the lines, the blocks and what one block costs. **It is the instrument rather than the cut:**
it decides nothing, leaves nothing out and names no section, so a section added or renamed appears
in it with no change on that side. The counts add up to the body's own line count, the opening
above the first heading counted as its own row.

**What the 19:52 file's other sections came to is UNKNOWN and is not estimated here.** The split
was in no report file and a number reasoned out would be a guess. The next run prints it, and the
cut after this one is made on those numbers.

### One thing corrected in the round message, and it is the only one

The round message said the fault is one of the two cell checks. It is not, and the measurement is
above. Everything else the message states was confirmed rather than corrected.

### Three break watches, one per fault

```
1  IsTheClientsHeader always false    TheClientsHeaderSurvivesAndTheContractReferenceIsThePlotsOwn
                                      RED: Expected "Neighborhood Landscape Design - Zone #2", Actual ""
2  Whose always says the run's row    ADriftOnTheClientsOwnRowIsNamedAsTheClientsRow
                                      RED: Expected "a row the client's list alrea...", Actual "a row this run wrote in"
2  the reason keyed on the plot too   TheGlanceCountsTheTwoComputedNumbersAndSaysWhyOnceRatherThanPerPlot
                                      RED: Expected "1 reason under this l...", Actual "2 reasons under this ..."
3  the shape keyed on the real text   TheFormulasReadingAWrittenRowAreSaidOncePerShapeAndNotOncePerCell
                                      RED: Expected 4, Actual 10
3  the opening not counted            EverySectionIsCountedAndTheyAddUpToTheFile
                                      RED: Expected 10, Actual 8
```

Every one names what was broken. All five files restored byte for byte, checked with `diff -q`,
and the suite green at 1928 after.

### Still open, and not guessed at in code

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number is off the seven 19:52 output
PDFs and the two report files. **The two computed numbers have still never been held against a
workbook Excel has recalculated.**

- **Which of the two things blanked the parks.** The 19:52 report answers it and nobody has read
  that line out of it. The next run puts it at the top in one line
- **What the parks templates' canopy formula reads**, if the guard is what fired. UNKNOWN here and
  answerable only off the client's files or the next run's report
- **What the other sections of the report come to.** Printed by the next run and not estimated
- **A species written into an empty row still carries no family, no genus and no native flag**,
  unchanged

---

## 2026-09-14, eightieth pass. Five faults off the 18:15 run

**Pull request 132, merged into main as `cc85143`.** The runner ran 28 hook cases and 1916 tests
against the pull request head `f8e6644`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1916 tests, 1096 of them KPI, 14 added, against the 1902 main carries** at `d266374`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### 1. The client's filling instructions were printed in the client's PDF

The tool wrote the fields it had values for and left every other one as the template had it, so
`Revit / softscape & shrubs & lawn schedule / total water demand /1000` sat inside the irrigation
box on all 150 PDFs, and the open spaces Ground Cover box, the one really named `0`, carried its
own REVIT SHEET note. **A note in a box reads as an answer.**

Every text field is WRITTEN or EMPTIED now, including every field the tool has no source for and
names nowhere. **The four stage tick boxes and the Reset button are the only fields left as the
template has them, and they are left because of WHAT THEY ARE**: the field's kind comes off the
file's own `/FT`, inherited through the parent chain the way an AcroForm defines it, and only `Tx`
is cleared. A field whose kind cannot be read at all is left alone, because this tool does not
clear what it cannot classify.

The report names every emptied field with why, per plot, and reads back what landed in each. **A
blank box is a decision on the record.**

### 2. The canopy was zero because the rows it read were not the rows it wrote

**Both candidates were checked and the FIRST is what did it**, against the round message's
expectation. `CanopyArea.From` took only matches with `Added` true, which is a species written
into an EMPTY row, so every row the client's list already held was left out whatever its diameter
said. On a plot whose species all match, that is every row, which is why Total Green cover came
out as the planting plus the lawn on every plot of 150.

**The second candidate is real on the same rows and is fixed with it.** The tool writes a diameter
only into a row it creates, so a matched row's canopy comes off the workbook's own diameter. It
travels on the match already, as `WorkbookDiameter`, read off the sheet's own diameter column
where the match was made, so nothing looks the row up a second time.

**The percentage comes right with it rather than by assumption**: it is canopy over area and the
canopy was the zero.

### 3. The pane was not reading unasked, and the header was claiming a read that did not happen

**Every path that can start a read was gone through and none of them fires without a press.**
`Ask(Plots)` is in the Read button's own handler and nowhere else, which is where the fifty
seventh pass put it. `Ask(WhichModel)` reads the document title alone. `Ask(Create)` is a press.

**What was wrong is the claim, not the read.** Two different reads set the header's count and it
printed them the same way: the plots press counts the elements and reads the sheets and schedules
the plot list comes off, in under a second, and the scan inside Create reads the whole document,
in 46.2. Neither number was wrong and the line made the first of them a claim about the model.
`ReadOfTheModel` carries which read produced it now and the line reads `Counted at ...` with a
sentence saying the model itself was not read.

**And the status line never said the read had landed**, which is why the 18:06:58 screen showed a
finished count beside a line still saying it was reading. It says so now. A status line that never
stops saying it is working is the same fault as one that never starts.

### 4. 149 of 156 plots had no detail, because the report took the first run of each template

`WriteAll` took `FirstOrDefault` of each template's runs, so the blocks covered EP-01, FP-16,
HF-01, DM-11, PL-17, SC-03 and MM-01 and nothing else.

**Every plot gets its own block. The counts stay counts of the run because they already are**:
THIS RUN AT A GLANCE and the run accounting both count over every run of the press, above every
block, and a block's own reconciliation reads one of one because a block IS one plot. That is why
a block per plot was chosen over one set of blocks carrying every plot: nothing has to be
re-counted and no number moves. The file preamble is printed once at the top rather than 156
times.

### 5. The formulas section

Three rules, all in `FormulaRepeats` so the heading count and the body come off one list. Only
formulas reading a cell on a row this run wrote into. The heading counts exactly what the body
prints. One formula filled down a column is one finding, with its cells as a span where they
really are one and as a list otherwise.

**A #DIV/0! on a cell this run did not write is out of this section and still counted and still
named**, in THE DIVISIONS at the top of the press, off the unfiltered list. The detail section
reports what the run is answerable for and the glance reports what the workbook will do.

**WHAT THE FILE COMES TO IS UNKNOWN UNTIL THE NEXT RUN**, and that is said rather than estimated.
The split of the 526 between written row and not is in neither report file, so the new line count
cannot be worked out from what was measured. The next run's own heading counts answer it, section
by section.

### One for the team, not a fault to fix

**The plot list reads `On a schedule and on no sheet, 7: -, EP-05, EP-11, EP-12, EP-13, EP-15,
FM-08`.** The first of the seven is a plot whose name is a single dash. It is on a schedule, so
some schedule in the model filters `PRX_Ref Plot ID` on `-`. Nothing in the tool invents a plot,
so it came off the model. **For Bader to find.**

### What is measured and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** Every number above is off the 18:15 output
pairs and the two report files. The emptying is proven against a PDF the tests build carrying a
note in a field the tool names nowhere and a tick box built as a button. The canopy, the header,
the per plot blocks and the formulas section are proven in Core.

**The two computed numbers have still never been held against a workbook Excel has recalculated.**

### Five break watches, one per fault

```
a tick box is emptied with the text fields         2 red, first naming the tick box left alone
the canopy takes only the rows this run created    1 red, expecting 600 and getting 0
the header prints the plots count as a read        1 red, naming the missing sentence
only the first plot of each template gets a block  1 red, EveryPlotOfEveryTemplateGetsItsOwnBlock
formulas the run never affected are at risk        1 red, every FromWrittenRow assert
```

**The second one went green the first time and the test was wrong, not the break.** It asserted on
a `CanopyRow` built by hand rather than going through `CanopyArea.From`, which is the method that
dropped the rows. It goes through `From` now and the same break reddens it. That is the fifth time
in nine rounds a break reddened something other than what it aimed at, and the first time the test
written to catch it was the thing at fault.

All restored byte for byte, checked with `diff -q`, and the suite green at 1916 after.

---

## 2026-09-14, seventy ninth pass. One rule for every label, and every unit off the page

**Pull request 130, merged into main as `b8bf73e`.** The runner ran 28 hook cases and 1902 tests
against the pull request head `27832dd`, 0 failed and 0 skipped. The merge went through the API
with the title and the message both passed on the call, the commit came back off main carrying
neither a co-author credit line nor a generated-by footer, and the merged tree is byte for byte
the branch head, checked with a diff that named no file.

**1902 tests, 1082 of them KPI, 16 added, against the 1886 main carries** at `d609559`, which is
also this branch's point, 28 hook cases unchanged, build zero warnings. **No audit finding is
closed, renumbered or reordered: read off the three files, 63 numbered findings, 19 carrying a
FIXED mark, 44 open.**

### The leading space is real and the field was NOT being blanked, which I measured before saying

The warning I ended the last round with was that the green cover label text came from the client's
note and the first real run would confirm it. Bader measured all seven and the note is indeed
wrong: every template holds ` Total Green cover (m²)` with a LEADING SPACE.

**The round message says the lookup as built matches nothing and blanks the field on every plot.
It does not, and I measured that rather than agreeing with it.** The cell's text was trimmed where
it was read, at `LabelledPlaces.Off`, so the leading space came off before the comparison. A
fixture carrying the real label with its space passed all seven cases first time.

Then I took that trim out to find whether it was what saved it:

```
LabelledCellsTests.TheLabelIsMatchedWholeAndWithoutCase        D7 against nothing
ComputedCellTests.TheGreenCoverCellIsFoundByItsLabelAndNames   red
ComputedCellTests.AGreenCoverCellThatHasDriftedIsNamed         no cell on <Mosques> reads
                                                               Total Green cover (m²)
ComputedCellTests.ThePercentageCellSitsTwoColumnsRight         red
RowFiveTests.ThePositionCellOnAParkTemplateIsReadAsHolding     " Architect Engineer"
```

**So it was landing, and nothing anywhere said why.** That is the finding, and it is worse than
the fault the round message expected: a rule living in a bare `Trim()` that nothing names, covering
ONE SIDE of a two sided comparison. A label constant carrying a stray space would still have failed
with no sign of where to look, and the fifth line shows a second thing riding on the same trim.

### One rule, eleven labels, and the ones that were already right are named

`LabelText.Same` is that rule now, asked by every whole label lookup. **Edge whitespace off BOTH
sides, without case, and the inside untouched.** A double space between words is a different label,
because `LOD /  HARDSCAPE SCHEDULES` really carries two.

Every whole label this tool looks up, checked one by one. Eleven, and one carries an edge space:

```
REF :, Date:, Prepared By: (twice), Character, Context    already right, no edge space
% of Total area covered by canopy                         already right, no edge space
 Total Green cover (m²)                                   THE ONE, now spelt as the file holds it
ID_UID *, ES_QUANTITY, QUANTITY UNIT, ROAD_WIDTH          already right, AND one sided
```

**The four street reference columns were the second one sided comparison**, trimming the file's
heading and comparing it against the name as written. They were found by looking rather than by
failing, which is the point of going through every one.

**One lookup is a different question and is left alone.** `ScheduleColumns.Holding` asks
`KpiNames.Holds`, which splits a heading into runs of letters, so edge whitespace cannot reach it.
It asks whether a heading HOLDS a word rather than whether it IS a label. Checked, already immune,
and recorded as such rather than changed.

### The fixture was reading the code back to itself

The workbook fixture wrote `ComputedPlaces.GreenCoverLabel` into the label cell. **A fixture of the
tool's own constant is a fixture of a sheet the client does not have**, and it is exactly how a
case goes green over a lookup that finds nothing. Both labels are written out by hand now, the
green cover with its leading space and `xml:space="preserve"` the way Excel stores such a cell, the
percentage with none, so one label with an edge space and one without go through the same lookup on
the same sheet.

### Every unit on all three forms, read off the page by position

**FOUR CONVERSIONS. Every other field is written in the unit it was read in.** The whole table is
in `kpi-rules.md` with the date. Three conversions are live and the fourth, litres a day into
m³/day, is unreachable because nothing reads a water demand off any schedule. Its reason names the
unit and the division now, so the day the read is built the rule is beside it rather than in a
constant nobody prints.

**THE REPORT PRINTS THE FORM'S UNIT AND IT ALREADY DID.** Checked at the line: the fill is handed
`wanted.Unit` off the form's own field, so the answer to what it prints today is the form's, and a
test pins it rather than leaving it to be read off the code. The unit strings are what the page
prints now, `m²`, `km²`, `m³/day` and `count`, where they were `m2`, `km2`, `m3 a day` and
`a count`.

**AND THE FIELDS NOBODY FILLS ARE ON RECORD WITH THEIR UNITS**, so nobody measures the page again
the day the client annotates one.

**CYCLING PATHS AND PEDESTRIAN PATHS ARE lm ON THE PARKS FORM AND km ON THE OPEN SPACES FORM.** The
same row name, two units, on two forms this one tool fills. Nothing writes them today so it costs
nothing now, and it would cost a thousandfold error in a client document the day somebody adds a
note to one and reads the other form's unit. **The Bridges and Catwalk row spells Lenght**, the
client's spelling, written as it is the way PRX_Furniture Lenght already is.

### What is measured and what is not

**NOTHING IN THIS ROUND HAS BEEN OBSERVED IN REVIT.** The labels and the units were measured by
Bader off the client's own seven templates and three forms. The lookup is proven against workbooks
the tests build carrying the measured label text.

**The two computed numbers have still never been held against a workbook Excel has recalculated**,
which stands from two rounds back.

### Two break watches

```
the label side of the rule is no longer trimmed    5 red, each naming the pair that stopped
                                                   matching, first " Total Green cover (m²)"
                                                   against "Total Green cover (m²)"
the road length prints the source's unit           4 red, every one naming km against m
```

Both restored byte for byte, checked with `diff -q`, and the suite green at 1902 after.

---

