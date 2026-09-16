# The seven checks, 16 September 2026

For Bader, on Windows, in the order they happen. One action a step, and where a step carries a
command that is the one to paste. **Nothing here was run in Revit by anybody**, so every check
below is a check and not a claim.

What landed since the 16:37 press. **Seven items two rounds back, six in the round after it and
three in this one**, all of them in the build step 8 installs:

1. Two ticked plots whose PRX_Plot_UID2 files them at one path now write NOTHING, and each is
   named above Create and in the report. Ten plots of that press wrote over each other.
2. THE PLOT LIST has a READY column after PDF, and every NO names every reason with its box or
   its cell. All 154 rows read YES four times over on that press.
3. The division check reads a division by COUNT, COUNTA or SUM over a range, honours the IF
   around it, and says how many it could not work out instead of saying none was found.
4. A row holding a count must carry the total canopy formula as well, in the column the canopy
   total adds, read off the file. FP-18 row 83 is why.
5. The canopy check lines print species names rather than Excel's string table numbers.
6. The glance names every plot whose two schedules both printed a heading and no rows.
7. A ticked plot the list does not name is named above Create and in a report block of its own.
   The 16:06 press ticked 166 against a list of 154 and said nothing about the twelve.

And this round:

8. **Where a template's canopy total column cannot be read, the check no longer switches itself
   off.** The green cover and the canopy percentage are left blank on any plot holding a count
   on that sheet, no empty row of it is offered to a new species, READY reads NO, and the glance
   names the template. A template naming no Total Green cover cell still refuses nothing.
9. **Two ticked plots whose PRX_Plot_UID2 differs only in LETTER CASE are one folder on Windows**
   and are found and stopped now. Each line prints each plot's own spelling, so a person looking
   for `anh-007-st-100213` in the model finds it.
10. **Both tree lists of every ticked template are read ONCE at the press**, before any plot is
    written, and every cell the team has still to fix is named with its own cell reference. It is
    a report section and it stops nothing.
11. The test project and the gate moved to **.NET 10, the current LTS**. Nothing the tool does
    changed. .NET 8 runs out of support on 10 November 2026.
12. **A throw reading which plot a schedule filters on is a refusal now**, naming the schedule,
    rather than an empty answer that read as a schedule belonging to no plot. So is a malformed
    sheet part in any of the four readers, which used to stop the whole press naming no template,
    no file and no plot, with no report written at all.
13. The rule that any round changing the panel writes an HTML mockup is **removed**, your
    decision of 16 September. Nothing under `design/` is touched and no mockup is owed for any
    past round.

And this round, off your streets only press of 16 September at 21:38, 81 listed plots:

14. **A division the check could not work out now makes READY read NO and names the cell.** That
    press printed 284 such lines, S70 and T70 on both tree lists of every one of its 71 written
    plots, and every plot still read READY YES. The guard is `IF(TotTrees<1," ",...)`, `TotTrees`
    is a defined name pointing at a cell reading `SUM(B4:B101)`, and that cell holds a formula,
    so the check answered that it could not say. It follows the name to its cell and counts that
    range now. **The glance names at most five and the full list is in each plot's own block.**
15. **The no planting line fires.** It asked a count that includes the heading row, so a schedule
    showing only its heading read as a schedule that printed something. It counts the rows under
    the heading now, and so does the reconciliation's own count of schedules that printed a body.
16. **The templates' tree list check names N85 and N88 to N101, prints no line twice, and leaves
    the analysis blocks alone.** An empty formatted cell counted as a cell holding something, a
    formula naming one range twice printed two lines for it, and `S59`, `T61`, `V59` and `W61`
    read the blocks beside the list rather than the list's rows and were named as ranges stopping
    short. A formula on the first tab reading a column the list does not fill has its own line.

---

## 1. Put the team's Revit fixes in the central model

**DO THIS FIRST AND DO NOT SKIP IT.** Whatever the team changed in Revit after the 16:37 press
goes into the CENTRAL model and is synchronised, or this run reads the old model and every
number below is about a file nobody will use again.

In Revit, **Collaborate** then **Synchronize and Modify Settings**, then **OK**. Wait for it to
finish before anything else.

## 2. Take a fresh detached copy

**File** then **Open**, pick the central model, tick **Detach from Central**, and choose
**Detach and preserve worksets**. A rerun on the copy the 16:37 press used measures nothing new.

Close every other model first, so the pane cannot read the wrong one.

## 3. Pull main

In GitHub Desktop, with RCRC-Green open, press **Fetch origin** and then **Pull origin**. The
branch selector must read **main** before you pull.

## 4. Close Revit

Close Revit 2024 completely. The install overwrites the add-in's assemblies and Revit holds them
open while it is running.

## 5. Open the repository in VS Code

Open VS Code, then **File** then **Open Folder**, and choose the RCRC-Green folder GitHub Desktop
keeps.

## 6. Open a terminal in VS Code

**Terminal** then **New Terminal**. It opens in the repository root, which is where both commands
below expect to be.

## 7. Build the add-in in Release

The install script reads `src\RcrcGreen.Revit\bin\Release`, so that is the project to build and
Release is the configuration.

```
dotnet build src\RcrcGreen.Revit\RcrcGreen.Revit.csproj -c Release
```

It must end with `Build succeeded.` and `0 Error(s)`. If it does not, stop here and send the
output.

**This used to build the whole solution and it must not any more.** `RcrcGreen.sln` holds
`tests\RcrcGreen.Core.Tests`, which targets .NET 10, so on a PC without the .NET 10 SDK the
solution build stops with `error NETSDK1045: The current .NET SDK does not support targeting
.NET 10.0`, and this step used to tell you to stop there. **The tests run on the GitHub test
gate, on .NET 10, so this PC does not need the .NET 10 SDK at all.**

Building the add-in project builds `RcrcGreen.Core` with it, through the project reference, and
puts the manifest and both assemblies in the folder step 8 installs from.

## 8. Install it

```
.\install\install.ps1
```

It builds the `Addins\2024\RcrcGreen\` layout that the build does not, so copying the output
folder by hand leaves Revit unable to find the assembly. It ends by printing every file it
copied and `Restart Revit 2024 and look for the RCRC Green tab.`

## 9. Check the plot list file is still there

The 154 plots, one per line, in the plain text file you saved outside this repository, at
something like:

```
C:\Users\<you>\Documents\RCRC\plot-list-154.txt
```

**It must never go into the repository.** It is the scope list and the repository is public.

## 10. Open the model in Revit

Start Revit 2024 and open the detached copy from step 2.

## 11. Open the KPI pane

**RCRC Green** tab, then **KPI Checklist**.

## 12. Point the pane at the plot list

In the template block, on the **Plot list** line, press **Browse** and choose the file from step
9. Under it the pane says how many plots it read, over how many lines.

## 13. Read the plots

In the **Plots** block press **Read this model**. Nothing heavy runs without a press, so the plot
list cannot be ticked until this has finished.

## 14. Tick every workbook row

**THIS STEP IS EASY TO MISS AND IT COSTS THE WHOLE RUN.** In the template block, tick every
workbook row, which on the templates folder is all seven. A ticked plot belonging to no ticked
row is written nowhere.

**If a row asks which of the two park templates it is, answer it.** The two park templates share
their main sheet name, so a file that settles nothing is ticked with no template and arms nothing
until you say which.

**These ticks live in the pane and nothing writes them down.** Closing Revit loses them, so this
step comes round again on every fresh start of the pane.

## 15. Press Tick the list, and make it the LAST tick

In the **Plots** block, beside **Select all** and **Clear**, press **Tick the list**. It replaces
every tick with exactly the plots the file names and forgets every plot you ticked or unticked by
hand.

**THIS MUST BE THE LAST TICK ACTION BEFORE CREATE.** Ticking a workbook row ticks every plot that
belongs to it and unticking one takes them off again, both of them moving the plot ticks this step
has just set. So if you touch a workbook row after this, press **Tick the list** again before you
go on.

**The 16:06 press is why this step matters more than it looks.** 166 plots were ticked against a
list of 154, twelve plots nobody asked for were written, and three of the shared value collisions
came from them.

## 16. Read the named lines above Create

Before pressing anything, read the lines above the **Create** button. Every one of these is named
there rather than found in a report afterwards:

- a plot on the list the model does not name
- a plot listed twice, with both line numbers
- a line that is not a plot, with its line number
- a listed plot that would get no workbook or no PDF, with the reason
- **a TICKED plot the list does not name**, which is new this round
- **two or more ticked plots sharing one PRX_Plot_UID2 and one folder**, which is new this round

If a plot you expect is named there, fix it before pressing Create.

## 17. Press Create

Press **Create**. The progress window says which plot is being read and how far through it is.

---

# The seven checks

One check a step. Each one is a thing to look at, not a thing to take on trust.

## 18. THE PLOT LIST shows a READY count, and every NO names its reason

Open the report. **THE PLOT LIST** is near the top. Its row now reads:

```
plot | in the model | ticked | workbook | PDF | READY | trees not written | why not
```

Check four things:

- the heading reads `(154)`
- the four counts at the bottom: `listed:`, `workbooks written:`, `PDFs written:` and
  **`ready:`**, which is new
- **every row whose READY column reads NO carries every reason in its last column**, each naming
  its box or its cell
- the glance line **THE PLOTS READY TO SEND: ready N of 154** agrees with that count

**You worked the ready list out by hand at 33 after the 16:37 press.** If this reads far from 33
on a model nobody has changed, the column is wrong and that is worth sending back.

A row reads READY YES only when both files were written, no tree was written nowhere, no PDF box
that has a source was left blank, the plot's PRX_Plot_UID2 is its own and the workbook check
found no `#DIV/0!`. **A box this tool has no source for does not count against it**, so the
sidewalks, kiosks and water tanks hold nobody back.

## 19. NS-01, NS-42, MM-01 and MM-09 to MM-15 write nothing and are named

**These ten plots share two PRX_Plot_UID2 values**, ANH-007-ST-100210 on the first pair and
ANH-007-ST-100213 on the other eight, so ONE WORKBOOK PER PLOT filed each group at one path and
the last plot written replaced the others.

Look in three places:

- above **Create**, before the press, a red line per plot naming the value and the other plots
- in the report, **THE PLOTS SHARING ONE PRX_Plot_UID2** in the glance, with the count and the
  values, and a line per stopped plot under it
- in **THE PLOT LIST**, each of the ten reading `NO` under workbook, PDF and READY, with
  `PRX_Plot_UID2 ANH-007-ST-100213 is also MM-09's, so no file is written for any of them` in
  the last column

**Then open one of those folders.** Whatever an earlier press left there is still there. Nothing
in this round deletes or moves a file, and the refusal says how many are in the folder.

**This stays until each of the ten has its own PRX_Plot_UID2 in the model.** That is the team's
fix and not this tool's, and it is the one thing that will unblock ten plots at once.

## 20. The division line names S70 and T70

Find **THE DIVISIONS** in the glance. It used to read that the formula check found none anywhere
in the press while FP-24 and SC-06 both recalculate with two.

Expect it to name, per plot, `Tree List - Existing S70` and `Tree List - Existing T70` on the
plots whose existing trees are all on rows 84 to 101. There were 30 such plots on the 16:37
press.

`S70` reads `IF(TotTrees<1," ",S69/COUNT(B4:B83))`. A plot with trees only past row 83 has a
count of nought inside a division, and the `IF` does not save it because `TotTrees` is not nought.

If the line now says some divisions **could not be worked out**, that is the honest third answer
and the count is there on purpose. It is not the same as finding none.

**This stays until the templates are fixed.** The ranges stop at B83 and the rows run to 101.

## 21. FP-18's Total areas to be greened is blank and names M83

Open **FP-18's** PDF and its workbook.

On the 16:37 press the PDF said 2,631 m² greened and 67.68% canopy and the recalculated Excel
said 2,531 and 63.97%, and nothing warned. FP-18 has 2 Ziziphus spina-christi on
**Tree List - Existing row 83**, where `L83` carries the canopy formula and **`M83` is empty** in
the EXISTING PARKS and FUTURE PARKS templates.

Expect now:

- **Total areas to be greened blank** on the PDF, and the canopy percentage blank with it
- the report's line for that plot naming **M83** and saying it is the column the canopy total
  adds, so the row computes a canopy per tree and adds none
- FP-18 reading **READY NO** with that reason

**This stays until the two park templates are fixed.** Column M is Total Mature Canopy Area and
row 83 has lost its formula.

## 22. The canopy check lines show species names, not numbers

Find any canopy check line in the report. On the 16:37 press one read:

```
D99 holds 419 and no formula, E99 holds 122 and no formula
```

where D99 holds **Prosopis Juliflora**. 419 was its index in the workbook's own string table.

Expect the name now. **Search the report for the word `holds` and check that no line prints a
bare number where a species name belongs.** If one still does, send that line.

## 23. The no planting line names MM-01, MM-06, MM-07 and NS-23

Find **THE PLOTS WITH NO PLANTING AT ALL** in the glance.

Expect those four named, because both of their schedules printed a heading row and no rows under
it, so every tree, shrub, lawn, water and green cover box on them was written 0. **The noughts
are right** and this line is what separates those plots from a plot whose numbers happen to be
small.

**It does not move READY.** Their files were written and every box has its number.

If the four are not the four, that is a change in the model since 16 September and is worth
sending back.

## 24. With only the list ticked, no ticked plot is named as off the list

Find **TICKED AND NOT ON THE LIST** at the foot of THE PLOT LIST.

If you followed step 15 it reads **0** and names nobody. That is the answer this check wants.

If it names plots, a workbook row was ticked after **Tick the list** and the extra plots came in
with it. Press **Tick the list** again and run it again, because those plots are written for
nobody and three of the 16:06 press's collisions came from exactly that.

## 25. THE TEMPLATES' OWN TREE LISTS names the cells still not fixed, per template

Open a terminal in the repository, as step 6 did, and pull the section out of the report:

```
Select-String -Path .\reports\RCRC-Green-KPI_*.txt -Pattern "TEMPLATES' OWN TREE LISTS" -Context 0,120
```

It is one block per ticked template and per tree list sheet, with the cells grouped by what is
wrong with each. Off the workbooks measured on 16 September, expect at least these:

```
EXISTING PARKS, STREETS         Tree List - Existing   L85, L88, L90 to L101 typed or missing
MOSQUES                         Tree List - Existing   L101 typed or missing
EXISTING PARKS, FUTURE PARKS    Tree List - Existing   M83 missing
all seven                       sheet not recorded     O90 to O94, O96 to O100, N85, N88 to N101
EXISTING PARKS, FUTURE PARKS    Tree List - Proposed   L84 to L92 missing
FUTURE PARKS                    Tree List - Existing   a SUMIF stopping at row 95 of 101
which templates is not recorded Tree List - Proposed   a SUMIF stopping at row 91 of 92
```

**THE CHECK IS THAT IT READS CLEAN ONCE THE TEAM HAS FIXED A TEMPLATE.** The glance line above
the section reads `<TEMPLATE>: clean.` and the block for it says `clean` on both sheets. A
template you have fixed must stop being named. A template you have not must be named with the
same cells it was named with last time.

**It stops nothing.** Every line here is a line. What the canopy guard and the total canopy check
already refuse is unchanged.

**A line reading NOT READ is not a clean line.** It says a column could not be read at all, so
that question was never asked on that sheet, and it is worth sending back.

## 26. No template is named as having a canopy total column that could not be read

```
Select-String -Path .\reports\RCRC-Green-KPI_*.txt -Pattern "CANOPY TOTAL COLUMN COULD NOT BE READ" -Context 0,10
```

On a run of today's seven templates it must read:

```
THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ: every ticked template's canopy
total named the column it adds.
```

**If it names a template, every plot holding a count on that sheet has a blank Total areas to be
greened and a blank canopy percentage, reads READY NO, and had no empty row offered to a new
species.** That is deliberate and it is new: the check used to write both numbers with no check
made at all. The line says which template and which sheet and why, and that is what to send back.

## 27. With only the list ticked and every UID2 its own, the shared value line names nobody

```
Select-String -Path .\reports\RCRC-Green-KPI_*.txt -Pattern "SHARING ONE PRX_Plot_UID2" -Context 0,20
```

This is the other half of check 19. Once the team has given each of the ten plots its own
PRX_Plot_UID2, and with **Tick the list** the last tick you pressed, the glance line must read
that no ticked plot shares its value with another and no plot line sits under it.

**Letter case is not a difference.** `ANH-007-ST-100213` and `anh-007-st-100213` are one folder
on Windows, so two plots spelt that way are still a collision and are still stopped, and the line
prints each plot's own spelling so the model can be searched for what it really holds.

## 28. NS-29, NS-33, ST-13, ST-18 and ST-25 read READY NO and name S70

```
Select-String -Path .\reports\RCRC-Green-KPI_*.txt -Pattern "a division could not be checked" -Context 0,2
```

Those five plots hold every existing tree on **Tree List - Existing rows 84 to 101**, so
`COUNT(B4:B83)` under `S70` is nought and their recalculated Excel really does show `#DIV/0!`.
On the 21:38 press all five read **READY YES**, because the check said it could not work out
whether the division is reached and nothing carried that into READY.

Expect each of the five in THE PLOT LIST with **READY NO**, and its last column naming
`Tree List - Existing S70` under one of two sentences:

```
#DIV/0!, Tree List - Existing S70, Tree List - Existing T70
a division could not be checked, Tree List - Existing S70, Tree List - Existing T70
```

**The first is the one to expect** now that the guard is followed to its own cell. The second is
the honest answer where the guard still cannot be worked out, and it is not a clean line either.

**IT STOPS NO WRITE.** The workbooks are written and the client's own arithmetic is what divides.
What changed is that such a plot no longer reads as ready to send.

**IT GOES AWAY WHEN THE TEMPLATES ARE FIXED**, not before. `S69` to `T70` read rows 4 to 83 on
all seven templates while the lists run to row 101, so this is the team's fix to the client's
files and check 4 of what comes next is where it sits.

## 29. The no planting line names MM-06, MM-07 and NS-23

Find **THE PLOTS WITH NO PLANTING AT ALL** in the glance, the same line check 23 asks for.

On the 21:38 press that line read `every plot this press read printed at least one schedule row.`
while **MM-06, MM-07 and NS-23 read 0 in every tree, shrub, lawn, water and green cover box**.
Their softscape and shrubs and lawn schedules each printed one row, the heading, with no group
row, no species row and no TOTAL row, and the count that was asked counts the heading.

Expect the three named. **The noughts are right** and the line is what separates them from a plot
whose numbers happen to be small.

**AND THE RECONCILIATION MOVES WITH IT.** `schedules that printed a body` counts the rows under
the heading now, so those three plots' six schedules count as printing none.

If the three are not the three, that is a change in the model since 16 September and is worth
sending back.

## 30. The STREETS tree list check names N85 and N88 to N101 and no line twice

```
Select-String -Path .\reports\RCRC-Green-KPI_*.txt -Pattern "TEMPLATES' OWN TREE LISTS" -Context 0,200
```

This is check 25 read again on the three things the 21:38 press got wrong, on
`GRP_-_KPI_Checklist_-_DD_STREETS.xlsx`, where that section named 293 cells.

**1. N85 and N88 to N101 must be named.** They are empty on all seven templates and none of them
was named, because each is a cell element carrying its formatting and no value at all and that
read as a cell holding something. Expect them under `the water per tree cell is empty`.

**2. No line is printed twice.** The range question printed 265 lines on Tree List - Existing and
only 139 of them are distinct: each of `V4` to `V57` was printed twice for `F4 to F83` and twice
for `B4 to B83`. Expect each cell and range pair once, and the count at the top of the section to
be the number of DISTINCT cells named.

**3. `S59`, `T61`, `V59` and `W61` must not be named.** They read `S4:S34`, `T4:T34`, `V4:V57`
and `W4:W57`, which are the analysis blocks beside the list rather than the list's own rows, and
all four were named as ranges stopping before the list ends on both tree lists.

**A formula on the first tab reading such a column has its own line now**, `a formula reads a
column the list does not fill`, naming the column and what it holds. `<Streets>` E37 reads
`Q4 to Q34` over a column holding nothing and E38 reads `T4 to T68`, the family percentage in the
analysis block, and both used to be named for their length.

**`V4` to `V57` are still named and that is right.** They count `B4:B83` and `F4:F83`, which ARE
the list's own columns, on a list that runs to row 101.

---

# What remains

## Known, and not fixed by this round

**Ten plots share two PRX_Plot_UID2 values.** The tool now refuses them rather than writing over
itself, and the model is where the fix is.

**Rows 85, 92 and 99 of the tree lists carry no canopy formula**, and now **row 83 of the two
park templates carries no total canopy formula either.** Both are the team's fix to the client's
templates.

**AND THE TEMPLATE EDITS OF 15 SEPTEMBER LEFT MORE OF THEM.** Measured in the 16 September
workbooks and listed in check 25: `L85`, `L88` and `L90` to `L101` still typed on EXISTING PARKS
and STREETS, `L101` typed on MOSQUES, `L84` to `L92` deleted on both park templates' proposed
list, `O90` to `O94`, `O96` to `O100`, `N85` and `N88` to `N101` empty on all seven, and the
Native and Adaptive SUMIFs stopping before the list they count ends. **The tool names every one
of them at the press now and changes none of them.** They are the team's fix to the client's
templates and they are what check 25 watches go clean.

**`S70` and `T70` divide by `COUNT(B4:B83)` while the names run to row 101.** The tool reports it
and changes no client formula, which is unchanged. **NS-29, NS-33, ST-13, ST-18 and ST-25 of the
21:38 press are the five plots it really fires on**, and they read READY NO now rather than YES.
It is the team's fix to the client's templates, and check 28 is what watches it go away.

**A first tab formula reading a tree list column the list does not fill.** `<Streets>` E37 reads
`Q4 to Q34` over a column holding nothing at all and E38 reads `T4 to T68`, which is the family
percentage in the analysis block. The tool names both under their own question now and changes
neither. **Whether either is a fault in the client's file or a formula reading a block on purpose
is UNKNOWN from this repository**, because no client workbook is in it, and the answer is the
team's.

**The FUTURE PARKS workbook holds AZADIRACHTA INDICA on more than one row.** The tool does not
choose between two rows and will not.

**The component and prefix refusal on FP-27 stays.** Where the two disagree neither decides.

**Nothing in this round has been run in Revit**, by this session or by anybody. Every line above
is a check.

## The audit files

**80 findings across the four audit files, 23 of them marked FIXED, so 57 are open.** Counted on
16 September, per file:

```
steps/audit-kpi.md      29 findings    8 FIXED   21 open
steps/audit-kpi-2.md    20 findings    9 FIXED   11 open
steps/audit-kpi-3.md    14 findings    2 FIXED   12 open
steps/audit-kpi-4.md    17 findings    4 FIXED   13 open
                        80            23         57
```

**NOTHING WAS CLOSED, RENUMBERED OR REORDERED IN THIS ROUND**, and the count above was read off
the four files again on 16 September rather than carried forward: 29 findings numbered 1 to 29,
20 numbered 30 to 49, 14 numbered 50 to 63 and 17 numbered 64 to 80, with the FIXED marks counted
on the findings themselves and not on the sentences in each file's own preamble that count the
files before it. A block of the 57 is what the next round can take.

## What comes next

1. Run it on the 154 and send what the list below asks for.
2. Take the ten shared PRX_Plot_UID2 values to the team. It is the one fix that unblocks ten
   plots at once.
3. Take row 83's empty M cell on the two park templates to the team.
4. Take `S70` and `T70` and the B4:B83 ranges to the team, with rows 84 to 101.
5. Take rows 85, 92 and 99 to the team, which is still open from 15 September.
6. Take the rest of the 15 September template edits to the team, off check 25's own list, which
   is the first run that names them all in one place.
7. Take `N85` and `N88` to `N101` to the team with the rest of check 30, which is the first run
   that names them at all.
8. Ask the team what `<Streets>` E37 and E38 are for. They read `Q4 to Q34` over an empty column
   and `T4 to T68` in the analysis block, and nothing here can say whether that is a fault.
9. The next round can take a block of the 57 open audit findings.

---

# What to send back after the press

**All of it in one message.** Nine things, and the report is open in front of you for the first
eight.

1. **THE PLOT LIST**, its **four counts** at the bottom, and **every row whose READY column
   reads NO**, with its last column.
2. **THE PLOTS SHARING ONE PRX_Plot_UID2**, the glance line and every plot line under it.
3. **THE DIVISIONS**, the glance line, the rows under it and the count of divisions that could
   not be worked out, which the glance now caps at five names with the full list under each plot.
   The 21:38 press printed all 284 of them here.
4. **THE PLOTS WITH NO PLANTING AT ALL**, the whole line.
5. **TICKED AND NOT ON THE LIST**, the count and any plots under it.
6. **THE TEMPLATES' OWN TREE LISTS**, the glance line per template and the whole section, which
   is check 25 and is the list to take to the team.
7. **THE TEMPLATES WHOSE CANOPY TOTAL COLUMN COULD NOT BE READ**, the one line, whatever it says.
8. **Every row whose READY column names S70**, which check 28 expects to be NS-29, NS-33, ST-13,
   ST-18 and ST-25.
9. **Screenshots of two PDFs**: FP-18, and any one of the ten shared plots' folders showing what
   is in it.

**Do not send the workbooks or the PDFs themselves**, and do not put any of them, or the plot list
file, or the report, into the repository. It is public.
