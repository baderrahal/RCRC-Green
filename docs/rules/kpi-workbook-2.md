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

## What the team types reaches the cells

The date, the person and their position come from no model. The pane collects them, and **it
used to collect them and hand none of the three on**: `KpiCreateAsk` did not carry them and
`KpiCreatePlan.Of` defaulted all three to null, so the first real workbook came out holding the
template's own placeholders while the report said nobody had typed them, on a run where all
three boxes were filled in.

The three are REQUIRED arguments of `KpiCreatePlan.Of` now, so a caller that forgets them does
not compile. A default that reads as a deliberate empty is how a whole link in a chain goes
missing without a word.

