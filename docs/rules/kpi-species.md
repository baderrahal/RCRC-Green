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

