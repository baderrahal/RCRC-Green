# Audit 3, the KPI tool, 2026-09-14

Main at `f3fe456`. **1673 tests, 853 of them KPI, 28 hook cases**, all green before and after
everything below. This audit builds nothing and fixes nothing, and the only files it writes are
this one and the task's own log and state. The state file is written because
`require-file-on-commit.sh` refuses a commit without it, not because the round wanted to.

**Read first: `steps/audit-kpi.md` and `steps/audit-kpi-2.md`, 49 findings, 17 FIXED, 32 open.**
Nothing they already reported is reported again here. Part A says what became of each of the 32.

**What has actually been run, which is what counts as evidence.** Five correct workbooks, one per
template: MOSQUES on NG03, MOSQUES and SCHOOLS on NG05, STREETS on NG05 twice. One per plot run
on NG05 on 14 September, 99 plots and 98 workbooks. **Nothing since 14 September has been run in
Revit at all**, which is rounds 113 and 114, the two shared hook rounds and the four test
findings. Everything below about the Revit half is reasoned from the code.

---

## PART A. The 32 open findings at today's lines

Nine moved. Twenty three stand. Two stand in a different shape and say so.

```
 3  PASSED BY    _picked is gone. Preselect is called from Changed() at KpiPanel.cs:1062 and
                 guards on _picks.Count, so TemplateForComponent is reached.
 4  STILL STANDS KpiPanel.cs:1276 with KpiNames.cs:19. Component is still "PRX_COMPONENT".
 6  STILL STANDS KpiPlotReader.cs:409 and :436 still swallow ApplicationException. The comment
                 at :307 names it as finding 6's and says it is not touched.
 7  STILL STANDS WorkbookPatcher.cs:373, PatchOutcome.cs:109 and :130, printed at
                 KpiCreateReport.cs:618 as STILL THERE.
12  STILL STANDS KpiRequestHandler.cs:105. Only WhichModel is guarded.
13  HALF MOVED   SpeciesSum IS printed now, KpiCreateReport.cs:435, :455 and :522. Repeats is
                 still printed nowhere. The half about Repeats stands.
14  STILL STANDS KpiCreateReport.cs:775 filters !Matched, heading at :843. A why column was
                 added per row, so the row says the real reason and the heading still does not.
15  STILL STANDS RegionArea at PlotReading.cs:482 carries no element id.
17  STILL STANDS KpiMerge.cs:439-447 adds SquareMetres with no HoldsAnArea.
18  STILL STANDS Reconciliation.cs:259 still reads "Total " plus a number.
19  STILL STANDS KpiPlotReader.cs:388 against KpiScheduleReader.cs:408, still two extractions
                 and only one caps at ShownRows.
20  PASSED BY    The grouping buttons and PlotTicks.OnlyFor are deleted.
21  PASSED BY    The output name box is gone.
22  PASSED BY    KpiPanel.cs:81-84 now says the pane holds no copy, which is what it does.
23  STILL STANDS Reconciliation.cs:103. ChooseTheRegion has no caller.
24  STILL STANDS KpiCreatePlan.cs:137. Unmatched has no caller anywhere, tests included.
25  STILL STANDS WorkbookPatcher.cs:181. ReadBack has no caller in src.
26  PASSED BY    CannotCreate is called with a real hasTemplate from KpiPanel.cs:909 and
                 KpiRequestHandler.cs:315, so NoTemplate can reach the screen.
27  STILL STANDS KpiCreateRun.cs:127-130, still "the only way a run ends with no output file".
28  STILL STANDS WorkbookPatcher.cs:17, still "a refusal writes nothing".
29  STILL STANDS and worse. KpiCreateReport.cs is 1277 lines, KpiPanel.cs 1493, KpiReport.cs
                 1122. Two of the three grew.
37  STILL STANDS KpiPlotReader.cs:81 and :144 order by sheet number, :165 and :176 take the
                 first, and nothing checks a plot's other sheets agree.
38  STILL STANDS KpiPanel.cs:188, inside the constructor at :171.
40  HALF MOVED   The two comments are gone from ScheduleRows.cs and Reconciliation.cs. The two
                 tests still carry the replaced rule in their names and docstring,
                 ReconciliationTests.cs:168-190.
41  STILL STANDS and worse. TemplateWords.cs:29-46 now stacks two summaries BOTH describing the
                 name box that round 114 deleted, with the stray line after the closing tag.
42  STILL STANDS KpiFillValues.cs. Constructed only in KpiTemplatesTests.cs:130.
43  PASSED BY    OutputName.Suggested is deleted, so there are no longer two suggesters. What is
                 left is one with no production caller, which is finding 55 below.
44  STILL STANDS KpiPlotReader.cs:483 de-duplicates on Title, KpiLinkReader.cs:70 on path and
                 title, and the create side still matches 00 anywhere in a name.
45  STILL STANDS KpiCreateRun.cs:12 still says every count comes off the outcome.
46  STILL STANDS WorkbookPatcher.cs:165 against CreateWords.cs:460.
47  STILL STANDS KpiPanel.cs:888 takes Ticked[0] under CreateWords.cs:314's "first ticked plot".
49  STILL STANDS KpiCreatePlan.cs:376 writes the date as inline text. Still UNKNOWN whether E5
                 is a date cell, and no workbook has been opened to look.
```

**Nothing above is repeated below.**

---

## Ranked findings

Numbered from 50, continuing the other two files. **Fourteen findings. Nine were dropped for
having no cost to the user**, listed at the end so the judgement is on the record. The cap of 40
was never approached.

### BLOCKS

**None.** Nothing found this round can be shown to stop the tool or to put a wrong number into a
client workbook on a path that has been run. Findings 50 and 51 become BLOCKS the moment either
of their unmeasured cases turns out to be real, and both say exactly what measurement settles it.

### WRONG

50. LOGIC | `src/RcrcGreen.Core/Kpi/KpiTemplates.cs:53` with
    `src/RcrcGreen.Core/Kpi/KpiCreatePlan.cs:179-181` | **The three cells the team types are
    written BY LETTER on all seven templates, and the letters were measured on one of them.**
    `TypedByTheTeam = { "E5", "G5", "H5" }` is one array for every template, its docstring says
    "The same in every template", and `kpi-rules.md` records the measurement as the two EXISTING
    PARKS files of 2026-09-09. Three more templates are confirmed only by their output being
    accepted, MOSQUES, SCHOOLS and STREETS. **HEALTHCARE, PARKING and FUTURE PARKS have never
    been looked at for row 5.** | **This is the Character and Context fault, three cells up, and
    it is the one that was found by writing a workbook rather than by reading code.** Character
    sat at D7 on two templates and at F7 on the third, because STREETS carries a Category at D7
    holding a formula, and a map holding the letter would have overwritten it. That divergence
    is PROVEN to exist in row 7 of these same sheets. Row 5 is asserted to be uniform on the
    strength of one template. If any of the three unmeasured ones differs, the run writes the
    date over whatever really sits at E5, which on the evidence of D7 can be a formula, and
    writes no date where the date belongs. `FilledMarks.cs:101` reads E5 as well, so the same
    letter also decides whether a workbook is offered as a template | Either open one HEALTHCARE,
    one PARKING and one FUTURE PARKS template and record row 5, or move the three to the label
    lookup `FixedCells.In` already does for Character and Context

51. LOGIC | `src/RcrcGreen.Core/Kpi/ScheduleRows.cs:153-163` with `:262` and `:469` | **A group
    row is recognised as a row whose FIRST cell holds text and every other cell is empty, and
    its name is then read off cell 0.** `IsStructureRow` is the whole rule. The shape was
    measured on mosque plots: DM-11, DM-12, DM-13, DM-16, DM-25 and FM-05 | **The first cell of a
    species row is the IMAGE**, which is the measured fact that broke four readers already and
    is the reason `CLAUDE.md` carries a rule of its own about it. A schedule whose group row
    carried anything in the image column, or whose group name sat in the second column with the
    image column present, fails `IsStructureRow` outright: the group is never opened, every
    species under it attaches to the group above, and those species go to the sheet that group
    points at. **A tree counted onto the wrong tree list sheet is a wrong number in the
    workbook and nothing in the run refuses it**, because the species rows still add to the
    printed TOTAL. This is the Street Design shape again, a group row not recognised, which is
    the second of the two faults found by running the tool | UNKNOWN whether any non mosque
    schedule prints a group row that way. A street or park plot's softscape schedule printed as
    it prints, which the report's last section already holds for any run, settles it

52. LOGIC | `src/RcrcGreen.Core/Kpi/ScheduleRows.cs:259` | **A group row is told from the TREES
    heading by looking at the NEXT row only.** `heading` is true when the row after a structure
    row is also a structure row, so TREES followed by Existing is a heading and Existing
    followed by a species row is a group. A schedule printing TREES, then a group row, then a
    second group row with no species under the first, reads the first group as a heading and
    drops it | Its species, if any follow later, attach to the wrong group. A group with no
    species is also how an empty phase prints, which DM-11 is one plot away from | UNKNOWN and
    the same printed schedule settles it. Listed apart from 51 because the cause is different:
    51 is which rows are structure rows and this is what a structure row means

53. LOGIC | `src/RcrcGreen.Core/Kpi/KpiTemplates.cs:53` read through
    `src/RcrcGreen.Core/Kpi/FilledMarks.cs:101` | The filled check asks whether E5 reads as a
    date, and `KpiCreatePlan` skips the reference cell when the ticked plots disagree on it,
    which a per plot run never does now that a workbook is one plot | Since round 113 every run
    writes one plot, so C5 always carries that plot's reference and both marks are live again.
    That is an improvement nobody recorded, and the rules file still describes the multi plot
    limit as current. **A limit that has gone away is as misleading as one that has not** | A
    line in `kpi-rules.md`, and the measurement in finding 50 first

### COSTLY

54. REPORTS | `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs`, measured by generating a report |
    **A run with one plot, no species and no schedule read produces 152 lines, and 68 of them are
    nine sections whose own count is zero.** Six of those nine print a column header for a table
    with no rows under it. Full measurement in part 5 below | The 14 September run was 1833
    lines. A person looking for one number reads past the furniture to find it, and the sections
    that are always empty are the ones that train the eye to skip | A rule about what an empty
    section prints

55. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/CreateWords.cs:628` | `SuggestedName` has no caller
    in `src/`, only four in `KpiCreateTests.cs`. It was the survivor of the pair finding 43
    named, and the name box it fed was deleted in round 114 | **This is the OutputName.Suggested
    situation exactly, and the rule from it applies rather than reachability**: the shape it
    records, a workbook name built from the template, the component and the plot, is the shape
    that no longer exists, which is the second of the two reasons a thing gets deleted | A
    decision, recorded either way

56. QA | `src/RcrcGreen.Core/Kpi/WorkbookFormulas.cs`, 761 lines | **The type is named in no test.**
    `WorkbookFormulasTests.cs` reaches it only through `outcome.Formulas` after a patch, so the
    entry point `WorkbookFormulas.Check` is never called directly and no case drives it over a
    formula shape without building a whole workbook first | It is the check that decides whether
    a workbook will compute, and it is the largest type in Core/Kpi with no direct case. Its
    behaviour is real and tested through the patcher, so this is reach rather than absence | Some
    direct cases over the formula parser

57. STRUCTURE | `src/RcrcGreen.Core/Kpi` | **78 files, one flat folder**, holding the scan side,
    the create side, the workbook reader, the workbook writer, the words for two panes and the
    two report writers | Finding 29 is about single files being too big and this is the layer
    above it: a reader looking for the group rule opens the folder and sees 78 names with no
    grouping, so `ScheduleRows`, `ScheduleColumns`, `ScheduleGroups` and `SoftscapeRows` are
    found by knowing they exist | Folders, in a round of its own. **Do not create them here**

### TIDY

58. NO VIBE CODING | `src/RcrcGreen.Revit/Kpi/KpiPanel.cs:1104-1108` | Two `<summary>` blocks
    stacked with nothing between them. The first describes "The name a ticked row offers", the
    name box deleted in round 114 | Finding 41's shape in a second file. A reader meets a
    docstring for a control that is not there | A deletion

59. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:552-554` and the sentence under
    every heading | Seventeen headings each carry a sentence explaining what the section is for,
    several of them arguing the rule rather than naming the contents. Part 5 lists them | A
    report that explains itself to its reader every time reads as written by a machine, which is
    the thing this part of the brief exists to catch | Wording, in a round of its own

60. NO VIBE CODING | `tests/RcrcGreen.Core.Tests/Kpi/ReconciliationTests.cs:168-190` | Two tests
    named `ASubtotalThatPrintedTwiceAndDisagreedRefusesTheWrite` and
    `ASubtotalThatPrintedTwiceAndAgreedDoesNotRefuse`, with a docstring saying the schedule
    "prints each group's subtotal twice" | This is the half of finding 40 that did not move. The
    replaced rule is now recorded only in the tests, which is the worst place for it, because a
    reader trusts a test name | Two renames

61. STRUCTURE | `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs` 1277 lines against
    `src/RcrcGreen.Core/Kpi/KpiReport.cs` 1122 | The create report and the scan report are two
    writers with no shared heading, table or count formatter between them, and both grew their
    own | Separable today and a little further apart every round | UNKNOWN whether they should
    share. The scan report answers nine fixed questions and the create report describes one run,
    which is an argument for leaving them apart

62. INTERFACE | `src/RcrcGreen.Core/Kpi/KpiCreateReport.cs:557` | The CELLS WRITTEN table prints
    `sheet | cell | value as it landed` and the sheet name is the same on every row of a per plot
    run, because a plot is one workbook now | A column that never varies | One column

63. NO VIBE CODING | `src/RcrcGreen.Core/Kpi/WorkbookFormulas.cs:305` with
    `src/RcrcGreen.Core/Kpi/WorkbookPatcher.cs` | Both open the output package and walk its sheet
    parts, with their own part reading each | Two readers of one file shape. Neither is wrong and
    a change to how a part is found lands in one of them | UNKNOWN whether it is worth one
    reader. Recorded as the next candidate after finding 48 rather than as a fault

---

## Area notes

### 1, logic

**Every rule about a schedule, a sheet, a parameter or a workbook, with what it stands on.**
This is the hunt the brief calls most valuable and it is listed whole, the ones that hold beside
the ones that do not.

```
rule                                  measured on                        covers its claim
the seven cell maps                   all seven annotated templates      YES
the header row is 3                   all seven                          YES
Character and Context by label        three templates, 13 September      YES, the rule reads
                                                                         the file rather than
                                                                         holding a letter
E5, G5, H5 by letter                  ONE template measured, three       NO, finding 50
                                      more inferred from output
the group row shape                   six mosque plots                   NO, finding 51
a group row against the next row      the same six                       NO, finding 52
one subtotal per phase then the       20 mosque plots, 0928 run          YES for mosques,
group total                                                              UNKNOWN elsewhere
Street Design counts on STREETS       ST-05 alone                        it is a decision of
                                                                         the team, not a
                                                                         measurement
the species image is the first cell   DM-12 and DM-13 existing rows      YES, and it is what
                                                                         findings 51 and 52
                                                                         turn on
the component to template table       the 1548 scan, 11 values           YES
the component to folder table         the team's own folders             YES
the plot prefix table                 the team, all seven                YES
the street reference columns          one file, 8,353 rows               YES, read by name
the 00 link is named 00               one model with one link            NO, finding 44
CellNumber reads no separator         one project                        NO, already recorded
the tree list row range off the file  two sheets of one template         YES, it reads the file
```

**Two records of one fact.** The running tally is **nine**, of which finding 48 was the first
proved to have diverged. The eight before it are in the two earlier files. **The next candidate
is finding 63**, two readers of the output package, and it is recorded as a candidate rather
than a fault because neither is wrong today and nothing has drifted.

**Reading a value by cell position.** Three sites in Core/Kpi index a row by number, all three in
`ScheduleRows.cs`, and **all three read cell 0 of a row that `IsStructureRow` has already proved
holds text in cell 0 and nothing anywhere else**: `:157` inside the test itself, `:262` taking a
group name and `:469` taking a phase name. Every other read goes through `ScheduleColumns.At`
with a column the heading row named. Sites checked and clean: the botanical name, the count, the
area, the height, the diameter, the subtotal, the TOTAL row, the species row test, the group
counter, and both tree list columns. **So the rule holds for values and the exception is the
structure row, which is findings 51 and 52.**

**null, empty, negative, NaN.** `KpiMerge.Area` is finding 17 and stands. `CellWrite.Number`
refuses NaN and infinity at the door. `PlotWorkbookPath` refuses a UID2 carrying a Windows
invalid character. Nothing new found.

**Case sensitivity.** Checked every comparison in Core/Kpi. The labels, the species names, the
group names and the sheet names all compare `OrdinalIgnoreCase`, the parameter names compare
`Ordinal`, which is right for a name the model must match exactly, and the region type name
compares `Ordinal`, which is finding 15's other half. Nothing new.

**Silent drops.** None found beyond finding 6.

### 2, wiring

**Every request from pane to handler.** `KpiRequest` holds four values and `Ask` holds one slot,
`KpiRequestHandler.cs:105`. `WhichModel` alone refuses to displace, which is finding 12 and
stands. **No new request was added since that finding**, so the shape is unchanged.

**A Revit call from the panel.** None. `KpiPanel` names no `Document`, no `Transaction` and no
`ElementId`, checked by grep over the file.

**The pane's remaining copies**, each named with why it is safe:
```
_facts            the plot read, re-asked when the model changes           safe, Took guards it
_templatesListed  the templates folder listing, finding 16's fix           safe, cleared on
                                                                          browse and after a
                                                                          press that copied
_model            the open model, re-read on every draw                    safe
_read             the header numbers, off the plot read                    safe
_date             DateTime.Now at construction                            NOT safe, finding 38
_lastSet          the held run, cleared by Changed()                       safe
```
**`_date` is the one unsafe copy and it is already finding 38.**

**An exception escaping Execute.** `KpiRequestHandler.Execute` catches at `:167`, which is
finding 17 and stands. Nothing new.

**A second press while the first runs.** The one slot is finding 12. A model opened or closed
mid run is UNKNOWN and would need a run to settle.

**Core references no Revit type.** Checked three ways: grep for `Autodesk` over
`src/RcrcGreen.Core`, which returns two hits and **both are inside comments**,
`ReportPlaces.cs:21` and `SamePath.cs:41`. The project file carries no `PackageReference` at
all, and the target is `netstandard2.0`, which the Revit packages do not support. Clean.

### 3, folder structure

78 files flat is finding 57. The five largest are finding 29 and finding 61. **Every crossing
between `Kpi/` and `Core/Shared`**, both directions:
```
Kpi reads Shared    PlotId, NaturalOrder, ScanFileName, PaneLabel, ParameterReading
Shared reads Kpi    nothing
```
**One direction only, which is the shape that avoids a stop and ask.** No crossing found that is
waiting to become one.

Two files doing the same job: finding 63. One file doing two jobs: `KpiCreateReport` holds the
run accounting and every per plot section, which is finding 29's other half.

### 4, no vibe coding

Findings 55, 58, 59, 60 and 62 are this area. **Nothing is deleted and nothing is recommended for
deletion on reachability alone**, which is the rule `OutputName.Suggested` bought. Each entry
says what the thing RECORDS and that is what the decision rests on.

Constants written twice: none found beyond finding 63's shape.

### 6, QA and QC

**853 KPI tests over 57 files and 748 Fact or Theory attributes.** What they cover, by weight:
the schedule readers, the merge, the reconciliation, the plan, the patcher, the report writers,
the words, the tables and the paths.

**Core Kpi types with no test naming them at all, eight of 143 public types:**
```
ComponentFolder     a row of the folder table, covered through ComponentFolders
ComponentTemplate   a row of the template table, covered through ComponentTemplates
FilledCell          covered through FilledMarks
MeasureAnswer       covered through KpiMerge
PlotPrefix          covered through PlotPrefixes
SpeciesAlias        covered through SpeciesAliases
SpeciesMeasure      covered through KpiMerge
WorkbookFormulas    THE ENTRY POINT ITSELF, finding 56
```
Seven of the eight are rows or answers reached through the type that holds them. **Only
`WorkbookFormulas` is a real gap** and it is finding 56.

**Three break watches, chosen as the three whose failure costs the client most. All three
reddened and every red case names what was broken. All restored byte for byte, checked by diff.**
```
A  ComponentFolders: FUTURE PARKS misspelt FUTURE PARK, the trap the rules warn about
   2 red  EveryComponentValueReachesItsFolder(FUTURE PARK, FUTURE PARKS), ElevenValuesReachEightFolders
B  SpeciesAliases: UNKNOWN pointed at Conocarpus erectus instead of Unknown Tree
   5 red  UnknownReachesTheRowTheListCallsUnknownTree, TheTableHoldsOneEntryToday,
          TheReportNamesTheAliasAndTheRowItReached, and two refusal cases
C  AreaUnits: the square foot constant changed in its seventh decimal
   4 red  OneSquareFootIsExactlyThatManySquareMetres, AThousandSquareFeetIsWhatSectionEightPrints,
          AMeasuredAreaWorksItsSquareMetresOutFromTheRawNumber, and the section 8 report case
```
**No test was found that would pass with its own behaviour broken.** That is a change from the
last two audits, where three such tests were proved, and it is the fair report: the four test
findings of round 117 went to the places that were weak. The three above were picked because
each is a data table or a constant that a reader would assume is covered, and all three are.

**The untestable list.** Everything in `src/RcrcGreen.Revit/Kpi`, 14 files, is reasoned about and
never exercised: the panel, the handler, the plot reader, the schedule reader, the link reader,
the progress window and the stores. **Nothing in that folder has a test and nothing can have
one** without Revit.

**Never executed in Revit even once**: rounds 113 and 114, both shared hook rounds, and round 117.
That is every change since 14 September, which is the whole of `RegionChoice`, the create block
wording, the folder line, the label lookup for Character and Context, and the counting change on
`TemplateOutcome`. **The label lookup in particular has never opened a real template.**

### 7, interface

Findings 62 is this area. Beyond it, checked and clean: every parameter label goes through
`PaneLabel.Escaped`, which has its own test over every name `KpiNames` holds. A refusal and a
note are told apart by colour and by the word Note since round 114, and the counts on the
template rows read off the same split the run uses. **The block order question raised as finding 21 is
answered by that round**, which took the name box out and put the folder line under the folder.

**What a person scrolls past every run** is the templates list, which is as long as the folder,
and the plot list, which is 155 rows on NG05. Both are needed. Nothing was found that is scrolled
past and never read.

### The Drawing Sheet

**Nothing found and nothing looked for.** It is a different tool and this audit stayed inside the
KPI folders, `RcrcGreenApplication`, `PanelMetrics` and `install.ps1`. The three shared files
carry one KPI line each and are unchanged since the audits that cover them.

---

## Part 5. The tool's own output

**This is Bader's own question and it is answered with a generated report rather than from
memory.** A run was built through the test fixture and `KpiCreateReport.Write` was called on it,
so every number below is counted off a real file.

### How long it is, section by section

**One plot, one template, no species matched, no schedule read: 152 lines.** That is the floor.
The 14 September run was 1833 lines over 98 plots.

```
lines  section                                                       count in it
  22   RECONCILIATION                                                0
  12   EVERY PLOT THAT WENT IN                                       1
  21   CELLS WRITTEN                                                 3
  11   CELLS NOT WRITTEN                                             8
  17   WHAT THE WORKBOOK WILL COMPUTE FROM THIS                      0
   4   SPECIES MATCHED                                               0
   4   SPECIES MATCHED THROUGH AN ALIAS                              0
   5   MATCHED SPECIES WHOSE HEIGHT OR DIAMETER IN REVIT DIFFERS     0
   4   SPECIES THE LIST HOLDS ON A ROW ITS TOTAL DOES NOT REACH      0
   5   SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT          0
   3   SPECIES ROWS UNDER NO GROUP                                   0
   7   THE WORKBOOK'S OWN TREE LISTS                                 2
  24   WHERE EVERY VALUE CAME FROM                                   1
   4   EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT       0
```

**Nine of the fourteen sections have a count of zero and cost 68 lines, 45 per cent of the file.**
Five of them exist only to say nothing happened.

**Six of those nine print a column header for a table with no rows.** This is the shape:

```
== SPECIES MATCHED (0) ==
the workbook row against the merged count, with the plots it came from
  sheet | row | workbook name | Revit name | group | merged | from | how
```

Three lines, two of them furniture, to say that no species matched.

### The explanatory sentences

**Seventeen headings, each with a sentence under it.** These are the ones a production person
would never read, because they explain a rule rather than name the contents:

```
ONE WORKBOOK PER PLOT          the folder tree is the root, the component folder, the plot's
                               UID2, and the workbook named after its folder
WHICH PLOT WENT INTO WHICH     PRX_Component decides, the plot prefix is a cross check, and
WORKBOOK                       where they disagree neither does
PLOTS TICKED THAT WENT INTO    named with the reason, never dropped in silence
NO WORKBOOK
CELLS WRITTEN                  every one read back off the output file, never as it was sent
MATCHED SPECIES WHOSE HEIGHT   named and CHANGED NOTHING, the client's row keeps its own number
OR DIAMETER DIFFERS
SPECIES THE LIST HOLDS ON A    the count was NOT written, so it is not in the total, and the
ROW ITS TOTAL DOES NOT REACH   row is named
SPECIES REVIT HELD THAT THE    named with the count, never dropped, and written into an empty
WORKBOOK'S LIST DOES NOT       row where there was one
SPECIES ROWS UNDER NO GROUP    reported and never assumed into a group, because the group
                               decides the sheet
THE WORKBOOK'S OWN TREE LISTS  read off the template when Create was pressed, never off a row
                               range in this tool
WHERE EVERY VALUE CAME FROM    a number with no source is a number nobody can check
STREET PLOTS THE REFERENCE     their road width and total length cells are left empty and
FILE COULD NOT ANSWER FOR      nothing is estimated from the component value
```

**Every one of those says what the TOOL does, to a reader who wants to know what the RUN did.**
"never dropped in silence", "never as it was sent", "a number with no source is a number nobody
can check" and "never off a row range in this tool" are the tool defending its own design in a
file whose job is to report one press. The heading alone carries every one of them.

**Which say the same thing twice.**
```
the preamble line, two lines long, says every schedule is printed at the end under a named
   section, and that section's own heading says the same thing again
CELLS WRITTEN says read back off the output file, and WHERE EVERY VALUE CAME FROM says a
   number with no source is a number nobody can check. One claim, two sections apart
SPECIES REVIT HELD... says never dropped, and PLOTS TICKED THAT WENT INTO NO WORKBOOK says
   never dropped in silence
MATCHED SPECIES WHOSE HEIGHT OR DIAMETER DIFFERS says CHANGED NOTHING in its sentence and
   again in its own first line, "0 of 0 matched species differ... and nothing was changed"
```

**Where a heading alone would carry it.** All seventeen. Not one of the sentences names anything
a reader cannot see from the heading and the rows.

### Which sections a person reads, and which nobody ever will

```
READ EVERY RUN
  the five preamble lines, which say the model, the time and whether it was read or reused
  RECONCILIATION, but only its refusals and its first four counts
  the status line, which is not in the file at all
READ WHEN SOMETHING IS WRONG
  CELLS NOT WRITTEN
  PLOTS TICKED THAT WENT INTO NO WORKBOOK
  WHAT THE WORKBOOK WILL COMPUTE FROM THIS, when it is not zero
  SPECIES REVIT HELD THAT THE WORKBOOK'S LIST DOES NOT, which is the team's open question
READ ONCE, EVER
  THE WORKBOOK'S OWN TREE LISTS
  WHERE EVERY VALUE CAME FROM
NEVER READ
  every section at zero, which is 45 per cent of a small run
  EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE PRINTS IT, which earned its place once,
    when it settled the FM-05 double, and is 200 rows a schedule every run since
```

**Which parts earn their length.** CELLS WRITTEN earns it: it is the only place the file proves
what landed. The schedule dump earns its EXISTENCE and not its default: it solved a fault no
other section could have, and printing it in full on every run of 98 plots is the cost of a
diagnostic left switched on. RECONCILIATION earns its counts and not its three empty total
blocks.

**Which are there because a prompt asked for them.** The seventeen sentences, the six empty
table headers and the two line preamble about where the schedules are printed. **A report nobody
reads is not a record**, and on the 14 September run the parts nobody reads outnumbered the parts
somebody does.

**Report only. No wording was changed.**

---

## What was dropped

**Nine findings were dropped for having no cost to the user.** Named so the judgement is visible
rather than implied: four naming conventions inside one file, two docstrings that are long but
accurate, one unused `using`, one method whose name could be shorter, and one test file whose
cases could be a theory. None of them changes a number, a refusal or a minute of anybody's time.

**No em dash, emoji or banned word was found anywhere in `src/RcrcGreen.Core/Kpi`,
`src/RcrcGreen.Revit/Kpi` or the KPI tests**, checked by grep over the list in
`.claude/skills/ai-max/references/writing-rules.md`. The hook has been refusing them on every
commit since the fortieth pass and it is working.

## What would settle the UNKNOWNs

```
finding 50   row 5 of a HEALTHCARE, a PARKING and a FUTURE PARKS template
finding 51   one non mosque softscape schedule printed as it prints, which any run's own
             last section already holds
finding 52   the same printed schedule
finding 49   one written workbook's E5, looked at in Excel
finding 44   a model carrying a second link whose name holds two noughts
```
**Four of the five need nothing but a run that has already been done and a file nobody has
opened.** The printed schedule section that costs the most lines is the thing that answers two
of them.
