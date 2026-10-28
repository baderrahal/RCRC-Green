## Project facts

These come from the team and from real models. They are not guesses.

- Views are named `<PlotID>-(<code>) <View name>`. PlotID is two uppercase letters, a dash,
  then digits, as in DM-41, and lowercase is invalid. The code is digits in round brackets and
  the view name is free text after the bracket and one space. A plot is also a scope box
  named with the PlotID. **THE TOOL NEVER INVENTS A PLOT, AND A SUB PLOT THE USER TYPES INTO
  A RANGE IS NOT INVENTED BY THE TOOL.** Step 1 lists every sub plot the From and To cover,
  in the model or not, each line saying which it is, and a sub plot the model does not hold
  can be ticked and can have sheets made for it. DM-02 is why: it holds no views and no scope
  box and the team has already made its sheets. What the model holds starts ticked and what
  it does not starts listed. The plot prefixes offered still come from the model and nothing
  adds one. A SHEET IS NOT NAMED THAT WAY: its name is the team's own word for
  it, held in a saved table per view type, because four of DM-11's eight sheet names differ
  from the view name upper cased: OVERALL KEYPLAN, PROJECT LOCATION KEY PLAN, and HARDSCAPE
  and SOFTSCAPE SCHEDULES, plural where the view names are not. Upper casing the view name
  is only the fallback for a type no file names, and it is said as derived when it runs.
  Its number is BUILT by the tool, the user's scheme: the view code, then the plot identifier
  with its dash dropped, then a sheet letter within one code, absent when the code holds a
  single sheet, so DM-42 reads 010DM42A, 010DM42B, 200DM42. Nothing is set per plot, nothing
  is reserved and nothing runs out, because the identifier is in the number and no two plots
  share one. NOTHING is derived from the model's own numbers, which are copies on every plot
  but one. The two measured models are numbered an older way, 010001A and 200001 on NG03,
  010QF and 200Q on NG05, and those sheets keep their numbers beside the new ones, which is
  the user's decision rather than a fault. The per-plot marker the tool asked for in step 1
  was wrong twice over, numbering the user had said is automatic and a long step 1, and its
  whole family is deleted
- **There are two plot parameters, not one.** `PRX_Plot_ID` sits on views and on sheets and the
  Sheet List filters on it. `PRX_Ref Plot ID`, with spaces rather than underscores, sits on
  model elements and every quantity schedule filters on that one. A schedule built against the
  wrong one comes back empty
- On a view, PRX_Plot_ID is the first place to look for the plot and the name is the fallback
- Cross sections are cut across the middle of the plot's scope box, the SHORT way, 18.2374
  metres long. The cut used to run the box's whole width, 36.4747 metres on NS-32, and the
  viewport it made was far wider than the drawing area, so the team halved it
- View names repeat word for word across plots, and nothing plot specific appears in one

Six of the things under a plot are schedules, filtered on PRX_Ref Plot ID. Category alone does
not identify one: HARDSCAPE and SHRUBS AND LAWN are both Floors, told apart by their second
filter. A schedule is built on Revit's NUMBER for the category, a filter value goes back as its
own kind, a calculated field cannot be added at all, and field names are copied exactly.

**A view family type is not named after the view type, and one view type is not built one way.**
DM-11-(010) Location Key Plan uses `(010) Key Location Plan`, words swapped, so nothing anywhere
is matched on a name. A new view takes its family type, level, template and two crop settings
from ONE view of the same type the model holds, and the report names it. A type with NO view
anywhere is created from three answers the user picks once before the run, family type, template
and level, each from what the model holds, remembered per type and refused until all three
are answered. A section takes no level. Three (010) views in
one run took three different family types that way. **(400) Landscape Cross Section is a section
rather than a plan view**, read off that view's kind rather than off the code.

Four settings are the tool's own rather than the model's, because the model disagrees with
itself or has the fault built in. Cross sections look 1 METRE, where four real ones read
0.93, 0.93, 1.53 and 12.83, and their CUT IS 18.2374 METRES. ANNOTATION CROP IS ALWAYS ON
for a plan view. CROP VIEW IS ALWAYS ON for a section the tool makes, because a section
with it off is not bounded sideways and the model's own sections have it off, which is what
drew other plots' markers through DM-11's plans. The report says whose setting each is.

A new view is CREATED FRESH, never copied from another plot, and carries no annotation or
detailing. A SHEET IS DESCRIBED rather than copied: the title block type, the views and 1, 2 or
4 per sheet are shared, and the views DIVIDE into as many sheets as they need, none left off,
and the sheets come out in the team's order, the code first and the nine name order list
within it. TWO VIEWS SIT ONE ABOVE THE OTHER and four make a two by two grid, because side by
side overlapped: every view on a landscape sheet is wider than half its drawing area. EVERY
VIEW IS MEASURED BEFORE IT IS PLACED and what does not fit is carried onto another sheet of
the same definition, never dropped and never laid over its neighbour. A TITLE SHEET is an
empty sheet on the COVER PAGE block, described with no views, and IT TAKES ITS CODE OFF THAT
BLOCK because it carries no view type. A sheet's size is read off the title block PLACED ON
IT, because Sheet Width and Sheet Height only exist on the instance. A SHEET HAS NO SCALE OF
ITS OWN: its Scale reads out its views' templates.

Measured on the first real model, RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached:

- 96,934 elements read in 1.4 seconds. The read is not slow and does not need caching
- 1,385 sheets, 953 views on sheets, 2,430 not on sheets, 79 view templates
- 406 scope boxes for 160 PRX_Plot_ID values, so a box not named for a plot is ordinary
- 2,114 names parsed and 4,039 did not, which made the parameter the first source not a fallback
- 6 views are named for one plot and carry PRX_Plot_ID for another. The scan names them
- Only plot DM-11 has real sheet numbers, 010QE to 600QD, all with plot letter Q. Others carry
  numbers like 010QE Copy 001, and a group of sheets has no PRX_Plot_ID
- Title blocks are AR-PRX-Title_Block_A1, several types. Read them from the model, never fix them

Measured on the 1355 run. These are what pick a workbook and fill it:

- **PRX_Component on the sheet is the ASSET TYPE, not a park name.** FRIDAY MOSQUE and SCHOOL,
  on 1,384 of 1,385 sheets. It is what picks the workbook template. **Its values are not the
  template names and no string rule turns one into the other**, so the mapping is A TABLE,
  measured on the 1548 scan, 11 values over 1,384 sheets: DAILY MOSQUE and FRIDAY MOSQUE mean
  MOSQUES, SCHOOL SCHOOLS, HEALTH HEALTHCARE, PARKING LOT PARKING, EXISTING PARK EXISTING PARKS,
  FUTURE PARK FUTURE PARKS, and NH STRT LESS 20m ROW, NH STRT 20m ROW, STREET 30m ROW and
  STREET 36m ROW all mean STREETS. It is many to one, the two park values break the park tie, and
  a value the table does not hold preselects nothing and says so. **The plot prefix never
  overrides it**: STREET 36m ROW covers MM and ST plots and NS carries two street widths
- KPI COMPONENT S/H is a show and hide toggle, No on 9 of 9 title block types. Its name holds
  COMPONENT and it is not a candidate for anything
- **PRX_Plot_ID is the plot** and the thing every schedule filters on. FM-05, SC-03
- **The plot prefix is a SECOND route to the template**, confirmed by the team, all seven:
  STREETS NS, ST and MM, PARKING PL, MOSQUES FM and DM, SCHOOLS SC, EXISTING PARKS EP,
  FUTURE PARKS FP, HEALTHCARE HF. It agrees with the eleven component values prefix by prefix
  with nothing left over on either side. PRX_Component still decides and where the two disagree
  neither does. It is what groups plots for the picker, and it is the only thing that can place
  a plot with no sheet, as EP-05, EP-11, EP-12 and EP-13 are
- PRX_Plot_UID is numeric with nulls. PRX_Plot_UID2 reads ANH-007-MO-100019. PRX_Plot_NH is the
  same on every sheet. None of the three is the plot
- **The softscape schedule prints TREES, then a group row per phase, then the species under it,
  then a subtotal per group, then TOTAL.** A species can appear under BOTH groups: ALBIZIA
  LEBBECK is 1 existing and 13 proposed on DM-12. THE GROUP ROW MUST TRAVEL WITH THE SPECIES ROW
- **A SHRUBS AND LAWN GROUP PRINTS ONE SUBTOTAL PER PHASE, THEN THE GROUP TOTAL.** Measured on
  the 0928 run over 20 mosque plots. A group holding Existing and Proposed prints three rows and
  the LAST is the group's value. A group holding one phase prints two equal rows, which is what
  DM-11 does and what made it the wrong plot to learn the shape from. Four out of four, the last
  row is exactly the ones above it added, in area and in item count: DM-16 shrubs 30, 54, 84 and
  FM-05 shrubs 361, 459, 820
- **Existing species print with no image and often NO BOQ CODE AVAILABLE.** One is called
  UNKNOWN. The image is the FIRST cell, so an existing species row starts with a dash and a row
  is a species row when the BOTANICAL NAME column holds text, never when the first cell does
- **The client's tree list can be shorter than the model.** The first real workbook, DM-12 on
  MOSQUES, came out correct and read 2 existing trees where the model holds 10 and 31 in total
  where it holds 39, because the 80 species in the MOSQUES list hold no Phoenix dactylifera, no
  Washingtonia robusta and no Unknown row to match. A species the list does not hold is WRITTEN
  INTO an empty row now, its name and its count and nothing else, so the counts reach the total.
  **Nothing may ever place an unmatched species by guessing:** no family, no genus, no native
  flag, no code. It is named in the report and what those missing columns cost is an open
  question for the team
- **Every plot has two filled regions in the 00 link**, one CADASTRAL LIMIT and one OUT OF SCOPE
  (PRESENTATION), and WHICH OF THEM CARRIES THE AREA VARIES BY PLOT. DM-11, DM-12 and DM-13 hold
  it on OUT OF SCOPE with cadastral at 0. NS-19 and NS-06 hold it on cadastral. The type name
  cannot decide it

Real names are in `.claude/rules/core-rules.md`, next to the rule they illustrate.

