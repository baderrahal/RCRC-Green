## The shrubs group holds two kinds and the species name says which

**GROUND COVER WAS IN THE SHRUBS BOX ON EVERY WORKBOOK AND FORM WRITTEN SO FAR.** The
instruction was to read the SHRUBS & GROUND COVER group's total area off the shrubs and lawn
schedule, and the group holds two kinds.

**Measured on the 05:49 report, over 156 plots:** 143 species names begin `SHRUBS:`, 107 begin
`GROUND COVER:`, 55 begin `GRASS:`. So the prefix is data the schedule already prints.

**Two plots, read off the schedules on screen, and they are opposite ends.**

```
DM-11  GRASS                  Proposed  GRASS: PENNISETUM SETACEUM          35 m2
                              group total                                   35
       SHRUBS & GROUND COVER  Proposed  SHRUBS: BOUGAINVILLEA GLABRA        36 m2
                                        SHRUBS: CARISSA MACROCARPA          34 m2
                              group total                                   70
       TOTAL                                                               105

DM-14  GRASS                  Proposed  GRASS: STENOTAPHRUM SECUNDATUM     175 m2
                              group total                                  175
       SHRUBS & GROUND COVER  Proposed  GROUND COVER: CARISSA MACROCAPA    270 m2
                                        GROUND COVER: LAMPRANTHUS AUREUS   198 m2
                              group total                                  468
       TOTAL                                                               643
```

**EVERY SPECIES IN DM-14'S GROUP IS GROUND COVER AND NONE IS SHRUBS.** 270 plus 198 is 468, the
group total, and all 468 went into Proposed Shrubs with Ground Cover left blank. That is the
wrong box on a real plot rather than a hypothetical.

**ONLY ONE OF THE TWO READINGS ADDS UP.** Writing the group total into Ground Cover as well
would give DM-11 Ground Cover 70 beside Proposed Shrubs 70, the same area in two boxes of one
form. The prefix split gives each box its own species and the two add to the group total.

### The rule

`SpeciesPrefix` and `GroundCoverSplit` are the whole of it.

- **The prefix is the text before the FIRST colon**, edge whitespace off, matched without case
  through `LabelText.Same`, the same whole-label rule every lookup in this tool asks. The inside
  is untouched, so `GROUND  COVER:` with two spaces is not `GROUND COVER:`
- **`SHRUBS:` species go to the shrubs figures, `GROUND COVER:` species to the ground cover one**
- **A species with NO prefix, or a prefix that is neither of the two, goes NOWHERE and is NAMED**
  with its plot, its row, its area and what its prefix read. Nothing is guessed into either box,
  and the one exception is the dash under its own section below
- **THE TWO WRITTEN FIGURES ALONE MUST EQUAL THE GROUP TOTAL. Anything the tool COULD NOT READ
  is a DISAGREEMENT and never a term of that sum.** Where they do not add up the split refuses,
  naming the numbers, and NONE of the four boxes is written, because writing three and blanking
  one leaves a form whose own numbers disagree with the schedule behind it
- **The GRASS group is untouched.** Its species carry `GRASS:`, it reaches lawn through its own
  heading, and nothing in the split reads it

**THE SHRUBS FIGURES COME OFF SPECIES ROWS NOW AND NOT OFF PHASE ROWS.** A phase row holds
shrubs and ground cover added together, which is the whole fault. Existing against proposed is
still decided by `CountedGroups.SheetFor`, the same method the tree lists ask, so a species
under a phase no tree list is named for is placed nowhere and named the same way a phase row
already is.

**A GROUP WITH NO SPECIES ROW CANNOT BE SPLIT** and says so rather than writing nought into both
boxes. So can a group the schedule printed no total for: the check is simply not run there.

**The room the check allows is the phase rows' own**, half the project's area rounding step for
each row summed, read off the project units and never a constant, because every printed area is
already rounded and a sum of rounded numbers need not equal a rounded sum. That is why the area
unit is handed into `PdfFill.Of` rather than a tolerance being written here.

**`PdfFill.GroundCoverIsNotPrintedApart` is DELETED**, and it is the second reason a thing gets
deleted rather than the first. It read that the schedule prints the group as one and nothing
prints ground cover on its own, so it is not derived. The SHAPE it recorded is gone rather than
its last caller: the two ARE printed apart, on every species row, and the number is read rather
than derived.

### A BOTANICAL NAME THAT IS A SINGLE DASH COUNTS AS SHRUBS, BY ITS PHASE

**Bader's decision, 15 September, and it is a decision rather than a measurement of the rule.**
His own record: **44 schedule rows across the model carry a botanical name of a dash**, and four
plots came out blank because of them, EP-01, EP-09, EP-14 and HF-01, which are the four the
section above made refuse. His screenshot of ANH-007-HF-100002 shows Existing Shrubs, Proposed
Shrubs, TOTAL Shrubs and Ground Cover all empty.

A dash row counts as `SHRUBS` and then goes by its phase like any `SHRUBS:` species. An existing
phase goes to Existing Shrubs, a proposed phase to Proposed Shrubs, a phase the template leaves
out stays out as it did, and **a dash row under no phase row still refuses**, because nothing
says whether it is existing or proposed and that is a different gap from not knowing its kind.

**ONLY THE DASH, and it is the WHOLE cell once the edges are off.** A name with no colon that is
not a dash, and a prefix that is neither `SHRUBS` nor `GROUND COVER`, refuse exactly as they did.
`ACACIA / VACHELLIA FARNESIANA` and `CARISSA MACROCARPA - GRANDIFLORA` carry a dash INSIDE and
are untouched. `SpeciesPrefix.CountsAsShrubs` is the one method both halves of the rule ask, so
the dash cannot be added in one place and forgotten in another.

HF-01 written out by hand, which is the one plot whose split is known: a group total of 283, 52
under Proposed as `GROUND COVER:` and 231 under Existing as dash rows.

```
Existing Shrubs  231      Proposed Shrubs  0      TOTAL Shrubs  231      Ground Cover  52
231 plus 52 is 283, the group total the schedule printed
```

**Which phase EP-01, EP-09 and EP-14 carry their dash rows under is UNKNOWN from this
repository.** The 08:38 report is under `reports/` and nothing there is ever committed, so the
tests pin only that those three add up and write, and not which of the two shrubs boxes took the
area. The next run answers it, because every dash row is named with its phase.

**EVERY DASH ROW IS ON THE RECORD, WITH THE BOX IT WENT INTO.** A row with no botanical name
reaching a client's box is what this tool refuses everywhere else, so it is allowed only in
writing: `THE ROWS WHOSE BOTANICAL NAME IS A DASH, COUNTED AS SHRUBS` sits beside
`THE SPECIES NO PREFIX PLACED, BY NAME` with the plot, the row, the phase, the area and the box,
which is Existing Shrubs, Proposed Shrubs, no box because this template leaves its phase out, or
no box because it sits under no phase row and the plot refuses.

**A DASH CAN ALSO REACH THE TREE LISTS AND NOTHING THERE CHANGED.** Traced rather than assumed.
`ScheduleRows.cs:329` reads the botanical name off the column the heading row names and `:330`
tests it for whitespace, so a dash is a species row in the softscape schedule too. It merges
through `KpiMerge.Species`, reaches `SpeciesMatching.Against` at `:317`, matches no name in the
workbook's list and no alias, and falls to `WrittenInto` at `:333`, which asks the canopy
diameter at `:434` and answers `NotSized` where the model prints none. **So a dash tree takes an
empty row carrying the name `-` and its count, or is named as unsized.** That is what it does
today and this round changed none of it.

### The wording that put placed-nowhere inside the equation, and what it cost

**Measured on the 08:38 run. The check said every one of these adds up while every box was
written short.**

```
plot  | existing | proposed | ground cover | placed nowhere | group total | adds up
EP-01 |    0     |    0     |      0       |    411 m²      |    411      |  YES
EP-09 |    0     |    0     |      0       |      3 m²      |      3      |  YES
EP-14 |    0     |    0     |      0       |     78 m²      |     78      |  YES
HF-01 |    0     |    0     |     52 m²    |    231 m²      |    283      |  YES
```

**HF-01's PDF read Existing Shrubs 231, Proposed 52 and TOTAL 283 on the 18:15 run. It read 0,
0, 0 with Ground Cover 52 on the 08:38 one. EP-01 lost all 411.**

**The fault was the rule as first written down**, which put the unplaced area INSIDE the
equation. With it there the sum always closes, so the refusal can never fire and all four boxes
are written with a number that is short. **A check that cannot fail is not a check.**

**The rule now: what would be WRITTEN, plus what this template deliberately LEAVES OUT, must
equal the group total.** Anything the tool could not read refuses the plot on its own, before
any arithmetic, and the rounding room applies to the sum and never to an unplaced species: a
species nobody could place is not a rounding difference however small its area.

### Two kinds of placed nowhere, and only one of them refuses

**This is where the round's own wording is departed from, and here is the reason.**

```
COULD NOT BE READ    a prefix that is neither of the two, no prefix at all, or no phase row
                     so nothing says existing or proposed. REFUSES the plot. The tool does
                     not know what the area IS
LEFT OUT BY THIS     a SHRUBS: species under a phase no tree list sheet is named for, which
TEMPLATE             is Street Design on a mosque plot. A TERM of the sum and NOT a refusal
```

**Refusing over Street Design would reverse a decision rather than catch a fault.** Bader
decided it is somebody else's scope, the phase rows have left it out and named it since that
decision, and FM-05 alone carries 459 m² of it. Taken literally, the fix would have written
nothing on every mosque plot that has one, which is a new fault of the opposite kind. The tool
knows exactly what a Street Design species is. It does not know what an unread prefix is. **Only
the second is an area nobody can account for.**

### What the section prints, and why the names go first

`SHRUBS AGAINST GROUND COVER, PER PLOT` opens with **THE SPECIES NO PREFIX PLACED, BY NAME**:
every distinct name the rule could not place, grouped by the prefix each read, with how many
rows and how much area carry it.

**It is at the top because it is the question rather than the detail.** `GRASS:`, `SHRUBS:` and
`GROUND COVER:` are the three prefixes ever measured. **If a run's unplaced rows all read one
prefix nobody has seen, the answer is a line in a table and not a refusal**, and that is the
team's call once they can see the name. If they read a dozen different things it is a naming job
in the model. The two look identical in a count and different in a list.

The per plot row then carries both kinds apart, **what the four boxes WOULD have read**, and the
group total, so the size of every loss is on the page: HF-01 would have read 52 against 283.

### What those species are called is UNKNOWN from this repository

**The 08:38 report is under `reports/` and nothing there is ever committed**, and nothing else
here holds a per plot species list. So the names cannot be read from this repository at all and
the section above is the answer rather than a guess.

**One thing the round message's own numbers DO settle, by deduction rather than by
measurement.** HF-01 read Existing Shrubs 231 and Proposed 52 under the phase rule, so its
EXISTING phase subtotal was 231 and that phase IS one a tree list sheet is named for on
HEALTHCARE. A `SHRUBS:` species under it would therefore have landed in the existing figure. It
did not. **So HF-01's 231 is unplaced because of its PREFIX and not because of its phase.** What
that prefix READS is still unknown, and the names block is what will say.

### What is UNKNOWN and how the next run answers it

**How many plots are all ground cover, all shrubs, or mixed is UNKNOWN from this repository.**
The 05:49 report is under `reports/` and nothing there is ever committed, so only the species
name counts above survive, and a name count is not a plot count. `SHRUBS AGAINST GROUND COVER,
PER PLOT` prints one row per plot with both figures and the group total, so the next run answers
it without anybody counting by hand. **The 08:38 run says 131 plots print the section and at
least four hold a species the prefix rule cannot place.**

**If the split disagrees with the group total on a real plot the tool STOPS rather than
choosing**, and the report names the plot and all three numbers. That is a question for Bader
and the client and not this tool's to settle.

## The irrigation water demand is both schedules' own TOTAL rows, added

**The last field on these forms with a real source that nothing read.** Bader specified it on 15
September: per plot, out of BOTH schedules, the L/DAY column, **TAKE THE TOTAL OF ALL**, add the
two, divide by a thousand because the form's unit column reads m with a superscript three over
day. It goes on all three forms.

```
<PlotID>-(600) SOFTSCAPE SCHEDULE          its L/DAY TOTAL row
<PlotID>-(600) SHRUBS & LAWN SCHEDULE      its L/DAY TOTAL row
                                           added, then divided by 1000
```

**THE SCHEDULE'S OWN TOTAL ROW AND NEVER THE SPECIES ROWS ADDED UP.** A sum this tool worked out
is a different number from one the schedule printed, and nothing on the page would show which it
was. `WaterDemandRead.From` is the whole rule and it adds nothing anywhere.

**THREE COLUMNS HOLD THE WORD WATER AND ONLY ONE IS L/DAY.** Measured on the 1548 scan:

```
softscape        IMAGE | # | PLANT CODE | BOTANICAL NAME | COUNT (n) | HEIGHT (m)
                 | DIAMETER (m) | WATER DEMAND | WATER L/TREE/DAY | L/DAY
shrubs and lawn  ... | WATER DEMAND | WATER L/SQM/DAY | L/DAY
```

So the heading is matched WHOLE, through `ScheduleColumns.Reading` over `LabelText.Same`, which
is the same rule every whole-label lookup in this tool already asks. **None and more than one are
both refusals and the headings are printed in each.** Taking the first of two is how a value
lands in a column nobody measured, and this is the two DIAMETER headings again, which cost a
round.

**The TOTAL row is found by its first cell and that is the row's definition rather than a value
read by position.** `SoftscapeRows.TotalMark` is the one mark and this reads it off that class,
so the two cannot part.

**BOTH SCHEDULES OR NOTHING.** A plot missing either schedule, or whose either total cannot be
read, writes nothing into the field and is named with which half and why, and where both failed
BOTH are named. Half a demand in a box printed `Irrigation water demand` is a number nobody could
tell was half. A plot holding TWO schedules of a kind says so rather than saying it holds none,
which is a different sentence about a different model.

**A SCHEDULE WITH NO TOTAL ROW IS NAMED AND THE ROW IT WOULD HAVE ADDED UP IS LEFT ALONE.**
Whether any real schedule prints none is UNKNOWN: every schedule shape measured so far prints
one, and the reader's own refusal is what will say so on the first run.

**IT PRINTS FINE RATHER THAN TO TWO PLACES.** Dividing litres by a thousand is what makes the
number small, the same way square metres divided by a million make the green cover small. 2492
L/day is 2.492, and two places would send 2.49 to the client, which is two litres a day thrown
away on every plot. It goes through the same `Fine` the green cover and the road length use.

### The total counts every group and the tree lists do not, and the report says so per plot

A mosque plot's Street Design group is out of scope for the tree lists by Bader's decision, and
its water is still inside the schedule's TOTAL. **That may well be right, because the water is
used whoever is paying for it, and it is not this tool's to decide.** So the number written is
the total Bader specified and nothing subtracts anything, and
`THE IRRIGATION WATER DEMAND, PER PLOT` carries a column naming every group that plot left out of
its tree lists with that group's own subtotal. The difference is on the page rather than found
three months later.

That section also prints **the row number each total came off**, so a person can open the
schedule and look at the same row the tool read.

### What DM-11 will read, and the one thing this cannot settle from here

Bader measured DM-11's shrubs and lawn schedule printing **L/DAY totals of 432 and 908** on the
1548 scan. **That scan is not in this repository and never will be**, so two things follow and
both are written down rather than guessed.

**If those two are the two GROUP totals, GRASS and SHRUBS & GROUND COVER, the schedule's own
TOTAL row reads 1340 and 1340 is what the tool takes.** It takes neither 432 nor 908, and
`OnDm11TheScheduleTotalIsTakenAndNeitherGroupTotalIs` pins exactly that over a schedule built to
that shape. If instead the TOTAL row itself reads one of them, the tool takes that one. **The
report prints the row, so the first run settles which without anybody arguing about it.**

**DM-11's softscape L/DAY total is UNKNOWN from here.** Call it S: the field comes to
`(S + the shrubs total) / 1000` m with a superscript three over day. On the 1340 reading, a
softscape total of 1152 would give 2.492.

