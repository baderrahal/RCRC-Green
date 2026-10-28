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
the templates folder and need no model, so the code still works that way and the plots cannot be
ticked until they exist.

**THE FOUR STEP PANE MAKES THAT ORDER UNREACHABLE, AND IT IS BADER'S OWN GATE.** His layout of
17 September says Tick cannot be worked on before a read, so step 3, which holds the workbook
rows, is shut until the plots have come back. Ticking MOSQUES and then pressing Read was the
ordinary order and cannot be done any more: with no model open there is no way to reach a
workbook row at all. **The loop the sentence above describes is live in the code and dead on the
screen.** Whether the gate should let the workbook rows through before a read is his call and it
is one argument in `KpiSteps.Of`, recorded in `steps/log-kpi.md` as a request rather than
decided here.

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

## The component picks the template, and the mapping is a table

`PRX_Component` picks the workbook template and **its values are not template names.** No string
rule turns HEALTH into HEALTHCARE or NH STRT 20m ROW into STREETS. So the mapping is data.
`ComponentTemplates` reads that table, which lives in components.csv since 28 Oct under K72. Its
first eleven rows were measured on the 1548 scan, 11 distinct values over 1384 sheets, off the
component values block at the end of section 3 of that report:

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

It agrees with the component values prefix by prefix with nothing left over on either side,
counted by asset type, and a test written out by hand says so. **A prefix names an asset type,
so it cross checks a template's KIND**, under K73: DM agrees with MOSQUES MORE THAN 60.

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

## One component table, components.csv, in three places

**K72.** The value to template table and the value to folder table were two tables in code keyed
on the same eleven values, compared twice. They are one file now, `components.csv`, header
`Component,Template,Folder`, one row per value, read by `ComponentTable` in Core.

Three copies, lowest standing first, and a value takes its row from the highest that names it:

```
shipped    beside the DLLs, from install/components.csv, copied on every install and by the kit
personal   %APPDATA%\RCRC Green\components.csv, optional
shared     components.csv in the templates folder set in Setup, optional, and it wins
```

The Core build copies `install/components.csv` beside `RcrcGreen.Core.dll`, so the tests and the
add-in read the one file the repo holds, and a gate test reads it against fifteen rows written
out by hand. **A file that names one value twice with two answers decides neither**, and a lower
file does not answer in its place. A file without the header line is not read at all. Every row
keeps its file and line, and both reports print where each value was read: section 3 of the scan
and WHERE EACH COMPONENT VALUE WAS READ in the create report.

**The folder is the file's spelling, never the model's**, so a model typed LESSTHAN files beside
every other plot of that value.

**Matching is `ComponentMatch.Key`, the one place it is decided.** Whole value, case blind,
trimmed, a run of spaces is one, no space before %, and LESSTHAN and MORETHAN read as LESS THAN
and MORE THAN. PARK still never finds PARKING LOT and NH STRT 20m ROW is still not NH STRT LESS
20m ROW, and tests hold both.

The pane puts the table in force at the top of every redraw and the handler again at the press,
through `ComponentFiles` in the Revit project, so the file is read fresh and no copy is held.

## The 60 per cent templates are versions of their kind

**K73.** Four templates of 28 Oct: MOSQUES MORE THAN 60, MOSQUES LESS THAN 60, SCHOOLS MORE THAN
60 and SCHOOLS LESS THAN 60. Measured cell by cell against MOSQUES and SCHOOLS, in
`design/component-wiring.md`: the same five sheets, and only H31 to H35 on the first sheet differ,
the KPI targets, which read row 33, 34, 35 or 36 of Green Strategy KPI's. **The LESS THAN 60 files
are byte for byte the MOSQUES and SCHOOLS the team already had.** So each carries its kind's cell
map and the tool measures nothing new.

`KpiTemplate.Kind` is MOSQUES or SCHOOLS for these and the template itself for the seven. The
plot prefix cross checks the kind. A plot with no component still goes to the kind its prefix
names, never to a version.

**The first sheet names three templates, so the file name says which**, the way it breaks the
park tie: the words a version adds to its kind, MORE THAN 60 or LESS THAN 60, as whole words with
the spacing forgiven. None of them is the kind itself, both is a pick for the user.

The four values, Bader's answers: each fills its own template and files under a new folder named
as the value. The workbook and the PDF print the value as the model holds it, as they always
have, off the plot's own reading. The PDF form is still chosen by plot prefix. On the pane the
four rows read Mosques >60%, Mosques <60%, Schools >60% and Schools <60%.

## A new template is offered on its layout, never used on it alone

**K74.** A file the tool knows by name is one whose name, as words, ends with the template it was
recognised as. The client names all eleven that way, measured on the real folder and the four new
files: all eleven known by name. `NewTemplates.Review` looks at every other file after the folder
is read, and compares it by `TemplateLayout` with the known files in the same folder.

**A layout is every sheet name in order and every typed label on the first sheet at its address**,
through the K35.09 label rule. A formula is not a label, so a 60 per cent file, whose five changed
cells are formulas, has its kind's layout, measured on all four. A layout that matches is a new
template, named after the file less the words every known file opens with, carrying the matched
template's cell map and kind. Setup shows it as New template, same layout as MOSQUES, with Use and
Skip. Use holds until Revit closes, and a components.csv row naming the template is a use too,
because somebody already chose to send plots to it. A layout that matches nothing is Unknown
template layout and is never used. Tests on built workbooks hold both, never on client files.

Setup also says when a components.csv was there and was not read whole, because a file not read
lets the files below it answer.

## A component value no file holds is offered one line, never placed by a guess

**K75.** After a read, step 2 shows one box per value no components.csv places: the value, how
many plots carry it, and a suggestion. The suggested template is the known one sharing the most
words with the value, a plural S and a per cent sign off each word, the first listed on a tie, and
none where no word is shared. The suggested folder is the value itself. Use suggestion and Choose,
two dropdowns of the templates and the known folders, each write one line, to the shared file
where the templates folder is set and takes the write, else to `%APPDATA%\RCRC Green\components.csv`,
and the box and the status line say which. Skip hides the box until Revit closes. Until a line is
written the value's plots are refused as before, with the value named.
