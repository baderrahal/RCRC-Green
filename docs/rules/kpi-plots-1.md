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
neither derives from the other. Both are columns of components.csv now, under K72, and the four
60 per cent values each file under a folder named as the value:

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

**Since 29 October the street values above no longer share STREETS.** Every value gets its own
folder, the value tidied, flat or grouped under a main folder, K78 in `kpi-plots-2.md`. The
table above is the first edition, kept as the source of the main names.

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

## The team's plot list file, and every plot on it is accounted for

**THE TEAM SENT 154 PLOTS TO EXPORT AND WANTS EVERY ONE EXPORTED WITH NONE SKIPPED.** The pane
takes a plot list file, browsed for and remembered through its own pointer beside the installed
assembly, the same way the templates folder, the output root and the street reference file are.

**THE FILE NEVER ENTERS THIS REPOSITORY.** It is the client's own list of which plots are in
scope, this repository is public, and only the POINTER lives beside the assembly. Every test
writes its own into the temp folder, the same rule the workbooks and the PDFs already follow.

`PlotListFile` and `PlotListRead` hold the read. Six rules, all tested.

- **Plain text, one plot per line**, edge whitespace off each line, blank lines skipped
- **The file's order is kept** and never sorted, because the report reads down it beside the
  team's own copy
- **A line that is not a plot identifier is NAMED with its line number** and never becomes one.
  `dm-11` is told apart from text that was never a plot, through the Shared `FailsOnlyOnCase`,
  because the two need different answers, and **nothing changes its case**
- **A plot listed twice is NAMED with both line numbers** and counted once. A list of 154 holding
  one plot twice is a list of 153 and somebody has to know which
- **Plots are compared the way the pane's own plot list compares them**, `StringComparer.Ordinal`,
  which is what `PlotTicks` and `PlotsInTheModel.Holds` use. A second comparison here would be two
  rules for one question
- **A file that cannot be read is a REFUSAL with its reason, never an empty list**, because an
  empty list and a file nobody could open tick exactly the same nothing

**TICK THE LIST SITS BESIDE SELECT ALL AND CLEAR AND BEHAVES THE SAME WAY.** It REPLACES every
tick with exactly the listed plots the model names and FORGETS every hand choice, because a
person pressing it is saying these are the plots and a per plot choice left standing behind it is
a record that disagrees with what is on screen. `TickingTheList` in Core is the whole rule and
the button only presses it.

**A LISTED PLOT THE MODEL DOES NOT NAME CANNOT BE TICKED AND IS NAMED.** Nothing here invents a
plot, which is the rule in `CLAUDE.md`.

**NOTHING ON THE LIST DROPS OUT WITHOUT A LINE, BEFORE THE PRESS.** Above Create: each listed
plot the model does not name, each plot listed twice, each line that is not a plot, and each
listed plot the press would put into no workbook or no PDF. The last of those asks
`PlotsPerTemplate.For` and `PdfForms.ForPlot`, which are the two rules that really decide it,
rather than a third written beside the button. **A list with nothing wrong with it says one line
and no more**, its own count, because a line about nothing is one the team reads past on every
other press.

**`THE PLOT LIST` OPENS THE REPORT, beside `EVERY PLOT THE TOOL OFFERED`.** They are different
questions and they are printed apart: that one is the MODEL's list and this is the TEAM's, and
where they disagree that is the answer rather than a fault. One row per listed plot in the file's
order, saying whether it is in the model, whether it was ticked, whether a workbook was written,
whether a PDF was written and why not, then the model's plots that are not on the list, then
three counts: listed, workbooks written, PDFs written. **A press with no plot list file set
prints no such section at all.**

**Rows edited in the pane live only until the pane goes**, and the file is read afresh at every
draw and at the press, so the pane holds no copy of it, which is the rule this file already
carries.

## TWO PLOTS, ONE FOLDER, AND THE LAST ONE WRITTEN REPLACED THE OTHERS

**Measured on the 16:37 press of 16 September, NS-01 and NS-42 both carry PRX_Plot_UID2
ANH-007-ST-100210, and MM-01 and MM-09 to MM-15 all carry ANH-007-ST-100213.** ONE WORKBOOK PER
PLOT gives each group one path, so the last plot written replaced the others, THE PLOT LIST read
YES for all ten, and each group took one street reference row, which is why MM-01 and MM-09 to
MM-15 all read ROW 10 and length 0.06108. On the 16:06 press three more groups collided: DM-11
with DM-21 and DM-29 on ANH-007-MO-100001, FM-05 with FM-08 and FM-09 on ANH-007-MO-100019, and
EP-01 with EP-05, EP-11, EP-12 and EP-13 on ANH-007-NP-100015. **The empty DM-29 files replaced
DM-11's.**

**THE WHOLE TICKED SET IS CHECKED BEFORE THE FIRST FILE OF A PRESS IS WRITTEN.** That is the
whole of the fix and it is a change to the SHAPE of `Create` rather than to any rule: the press
read one template and wrote it before it read the next, so the first template's files had already
landed when the second template's collision became knowable. `OneTemplate` is split into
`ReadOneTemplate` and `WriteOneTemplate` with a `TemplateReading` between them, `Create` reads
every ticked template, builds the filings, asks `SharedUid2.Of`, and only then writes. **Nothing
about reading a plot changed**, its counted groups, its area rule, its held readings, its progress
count and its area unit all still decided once per template.

**THE FILE PATH IS WHAT COLLIDES, NOT THE VALUE.** Two plots carrying one UID2 in two different
component folders are filed apart, so nothing overwrites anything and both are written, and they
are still named. `SharedUid2Group.FiledApart` is that case.

**THE PATH IS BUILT ONCE AND READ BY THE WRITE.** `PlotFilings.One` builds it for the press,
which already knows each plot's template off the split, and `PlotFilings.Of` builds it for the
pane, which asks `PlotsPerTemplate.For` because nothing has been read yet. Both reach
`PlotWorkbookPath.For`, and `OnePlot` READS the path off the list rather than working one out, so
a check reporting on a path the writer does not use cannot happen.

**NO FILE IS WRITTEN FOR ANY PLOT IN SUCH A GROUP.** Writing one of them and refusing the rest
would pick a plot nobody chose, and writing all of them is what happened. Each plot's row names
the shared value, every other ticked plot holding it, and the one path they would all land on.

**FILES ALREADY IN THAT FOLDER FROM AN EARLIER PRESS ARE NAMED AND LEFT WHERE THEY ARE**, the
same rule the crash row already follows. Deleting them destroys the evidence and the team's own
earlier work, and saying nothing about them leaves somebody opening a folder tree.

**AND THE GROUPS ARE NAMED BEFORE THE PRESS.** The pane owns the read already:
`ReferenceValuesPerPlot` carries all four plot parameters for every plot off the same first sheet
the component comes from, so no second read is made and no second record of a plot's UID2 exists.
`KpiPanel.Uid2On` is the one lookup.

**A PLOT WITH NO PRX_Plot_UID2 IS LEFT OUT of the grouping**, because a plot with none is refused
by its own path already and an empty value shared by five plots is not one value.

### TWO VALUES THAT DIFFER ONLY IN LETTER CASE ARE ONE FOLDER ON WINDOWS

**The grouping was `StringComparer.Ordinal` and the paths were compared only INSIDE each group**,
so `ANH-007-ST-100213` and `anh-007-st-100213` went into two groups and their two paths were
never held against each other. Windows files them under one name, the second workbook written
replaces the first, and nothing said a word. Bader's decision of 16 September.

- **The colliding plots are found BY THE FILE PATH, across every ticked plot, ignoring case**,
  which is how Windows compares a path and is the comparison `FilePaths.Compare` already makes
- **Each line names every plot's UID2 EXACTLY AS THAT PLOT CARRIES IT.** Printing one spelling
  for a group holding two is what hid this, and a person looking for `anh-007-st-100213` in a
  model finds nothing where the line says `ANH-007-ST-100213`
- **A group whose spellings differ only in case says so**, in its own sentence, because two
  plots carrying one value and two plots carrying two spellings of one value are different
  things to go and fix in the model

MM-01 with `ANH-007-ST-100213` and MM-09 with `anh-007-st-100213` in one component folder is the
test. Both write nothing and each names the other with its own spelling.

## A ticked plot the list does not name is work nobody asked for

**THE 16:06 PRESS TICKED 166 PLOTS AGAINST A LIST OF 154.** The twelve extra plots were read,
written and filed, three of that press's five shared value collisions came from them, and neither
the pane nor THE PLOT LIST said they were ticked.

`TickingTheList.TickedAndNotOnTheList` is the rule and it is the OTHER DIRECTION from
`NotOnTheList`, which is about the MODEL. A plot the model holds and the list does not is
ordinary and is already a block of its own. A plot somebody TICKED that the list does not hold is
work nobody asked for.

It is a line per plot above Create and a block `TICKED AND NOT ON THE LIST` at the foot of THE
PLOT LIST, naming each plot with the workbook and the PDF really written for it. **The ticks are
compared the way the pane compares them**, `StringComparer.Ordinal`, which is what `PlotTicks` and
`PlotsInTheModel.Holds` use.

**Pressing Tick the list last is what keeps it at nought**, because ticking a workbook row ticks
every plot that belongs to it, which is the rule that put the twelve there.

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

