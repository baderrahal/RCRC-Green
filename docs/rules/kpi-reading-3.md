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

## The area is not a schedule row

The report used to end saying every number above came off a row the schedule printed. **The area
did not.** H7 took 3728.7570000000005, converted from the raw 40136.006313679296 square feet off
`PRX_Intervention Area` on the chosen filled region in the 00 link, where the schedule prints
3729.

The conversion is right and is more precise than the printed value. The sentence was wrong about
it. The region row carries the raw reading, the converted metres and what the model prints, side
by side and unrounded, so the two can be held against each other, and the closing paragraph says
the schedule claim for the numbers it is true of and names the area separately.

