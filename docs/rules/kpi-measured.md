## What the first real scan measured

All of it from one run on RCRC_NG05_NU_MAIN_RVT24_SHEETS_detached, 96,959 elements in 5.4
seconds, six of the nine questions answered. These are measurements, not guesses.

**The main sheet name carries angle brackets.** `<Park Name>`, `<Healthcare>`, `<Mosques>`,
`<Parking Plots>`, `<Schools>`, `<Streets>`. The brackets are part of the name and not
placeholder notation. They were stripped when the map was written, so all seven real workbooks
came back unrecognised the first time the pane saw the client's folder. A test asserts every
entry opens with < and closes with >.

**PRX_COMPONENT does not exist in this model.** The sheet carries `PRX_Component`, capital C
only, on all 1385 sheets with 1384 values. So a near miss that is named is now also shown, with
the same columns and the same cap the exact name would have used. A near miss named and never
shown is the answer withheld.

**Four plot parameters sit on the sheet**, `PRX_Plot_ID`, `PRX_Plot_UID`, `PRX_Plot_UID2` and
`PRX_Plot_NH`, all with values, and the report prints them side by side one row per sheet
because nothing showing one of the four can say which the workbook wants. `PRX_Plot_UID2` is on
the SHEET, not the title block: on the title block it is on 1233 instances and holds a value on
none of them, and on the sheet it holds 1384 values reading like ANH-007-MO-100019, which is
nothing like a plot identifier such as DM-41.

**THE ELEMENTS A SCHEDULE LISTS ARE NOT THE SCHEDULED THINGS.** Asking Revit for the elements
of DM-11-(600) SOFTSCAPE SCHEDULE returns six RVT Link instances, because the plants live in
the linked component models. Section 8 came back empty for exactly that reason. The printed
rows are not the better route to these numbers, they are the ONLY route, and nothing is ever
recomputed from elements.

**Schedules are per plot**, 155 of each, named `<PlotID>-(600) NAME` and filtered on
PRX_Ref Plot ID Equal `<PlotID>`. Eight distinct names once the plot is off. How many plots of
each name are read in full is the rule above, and it is stated there and nowhere else.

- SHRUBS & LAWN SCHEDULE, category Floors, prints in groups. DM-11 gives GRASS 35 m² 46, then
  SHRUBS & GROUND COVER 70 m² 58, then TOTAL 105 m² 104. The workbook wants the two group
  values and not the total. **How it prints is below and the numbers alone are not enough**, and
  **DM-11 is the special case**: every one of its groups holds one phase
- SOFTSCAPE SCHEDULE, category Planting. Its fields are BOTANICAL NAME, which is
  PRX_Softscape Botanical Name, and COUNT (n), a Count field. DM-11 gives ALBIZIA LEBBECK 6,
  BAUHINIA PURPUREA 2, CASSIA GLAUCA 4, total 12. How it prints is the rule in `CLAUDE.md`,
  stated there and nowhere else, because the group row is what question 8 is answered from
- Two phases, Existing and Proposed. The split shows as a group row inside the printed
  schedule, not as a separate schedule. **FM-05 prints a third, Street Design**, measured on
  the 1536 report, and it is out of scope under the rule above

## What the two real workbooks measured, once

A one-off check on 2026-09-09 against two EXISTING PARKS files supplied in a chat session, the
production copy and the annotated one. **It cannot be re-run and no gate repeats it.** The
files are not in this repository and never will be. What is committed is what it taught.

Every mapped cell agreed with the annotation, D3, C5, E4, D8, F11 and H11, and E5, G5 and H5
read DATE OF THE DAY, EMPLOYEE NAME and EMPLOYEE POSITION. Those three come from nowhere in
Revit. The team types them into the pane and the tool copies them through, so a filled
checklist carries who filled it and when. **That those three sit in the same cells in every
template was read off this one file and held for six rounds**, which is what finding 50 named
and what the 14 September measurement of all seven settled. Nothing holds their letters now:
they and the reference are found by their labels, under the section above.

Two things the check found that reading the map could not:

- **The annotation writes the shrubs and lawn note in row 10 AND row 11.** Row 11 is the
  input. In the production file F10 is `=F11` and H10 is `=H11`, so writing into row 10 would
  destroy a formula. The map's F11 and H11 are right, and now proven right
- **`Area` is a defined name pointing at `<Park Name>`!$D$8**, so D8 is the area the whole
  sheet computes from, and `H9` is `=H8/Area`, which is the divide by zero that goes away when
  the area is filled

The header is row 3 and the total at row 93 is `SUM(B4:B92)` on both sheets. Where the species
stop is read off each sheet when Create is pressed and is in no map, because the MOSQUES
existing names ran to row 101 on 2026-09-10 against a map entry that said 83.

## What the first real workbook measured

One press of Create on DM-12 with the MOSQUES template, 2026-09-09. **The workbook is written
and correct.** These are measurements off that output file, not reasoning about it.

- **37 parts in, 37 out, 4 changed**, and the output recalculates with ZERO errors. The parks
  figure above, 45 against 44, is EXISTING PARKS and is a different template. Both stand. It
  reads 37 in and 36 out now, because the calc chain is removed on purpose
- **Excel opened it showing zeros for seven computed cells** while the inputs beside them were
  right. That is the stale cache in the patcher section above, measured on this same file
- The six values landed and the client's own formulas ran on them: **28.1 percent canopy against
  a 13 percent target, Excessive, NOT COMPLIANT.** The tool wrote no verdict anywhere. That is
  the workbook's arithmetic on the numbers Revit gave it
- **The slash case matched.** ACACIA / VACHELLIA FARNESIANA found Acacia / Vachellia farnesiana
  at row 11, which is why nothing is stripped or split on the way to a comparison
- **Three species were not in the list**, all under Existing: PHOENIX DACTYLIFERA 5, UNKNOWN 2
  and WASHINGTONIA ROBUSTA 1. The MOSQUES tree list holds 80 species in rows 4 to 83 and not one
  of them is Phoenix dactylifera, Washingtonia robusta, or any of the Unknown rows. Checked
  against the output file itself rather than against the map

The last one has a consequence, and the fix and the open question are two different things:

**That run read 2 existing trees where the model holds 10, and 31 in total where the model holds
39**, because the three were named and written nowhere. They are written into the empty rows now,
under the species rule above, so the counts reach the total. **Nothing in the tool may ever place
an unmatched species by guessing**: the name and the count go in and no other column does.

**What is still open is that a row written that way carries no family, no genus and no native
flag**, so the KPIs that need those cannot see it, and the client's species lists are short of
trees this project actually plants. That is for the team. `steps/log-kpi.md` carries it.

## What the first twenty plot run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 11:16, and the workbook it wrote read against
the model. These are measurements off that run, not reasoning about it.

- **The subtotal rule holds on every number it was predicted to move.** DM-16 shrubs 30 to 84,
  DM-25 13 to 241, FM-05 shrubs 361 to 820, FM-05 grass 96 to 165. Shrubs total 2517 to 3258,
  lawn 1058 to 1127
- **The date, the prepared by and the position reached the file.** The cache section read
  cached results left 0, dropped 1674, 37 parts in and 36 out with `xl/calcChain.xml` named as
  the one removed on purpose. Three species were written into empty rows on the Proposed sheet
  with the name and the count and nothing else
- **Tree List - Existing had three row ranges**: the map and the pane said B4 to B83, the
  sheet's total said `SUM(B4:B92)`, and the names ran from row 4 to row 101. Six species were
  reported as having nowhere to go, 85 existing trees between them, and four of the six sat in
  the list past row 83. The workbook said 76 existing trees where the model holds 161. Fixed
  under the species rule
- **FM-05 printed twice in one species row**, taken that round for two schedules whose names
  hold SOFTSCAPE. The 1428 run showed one schedule and two printed rows, and the 1536 report
  showed the second row under a third group, Street Design, under the rule above
- **313.5 seconds, 312.8 of them reading the model**, 20 plots at 15.6 seconds each, and 0.7
  seconds for everything after the read. STREETS ticks 78 plots, which is about twenty minutes
  at that rate. What the read does per plot is in the log. It is measured in calls and not in
  seconds, because nothing here runs Revit, and it is not changed

## What the 1428 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 14:28, the workbook it wrote, and that workbook
opened in Excel. These are measurements, not reasoning.

- **The tree list fix worked.** B84 17, B86 27, B87 19, B89 3, B99 3, so the 69 existing trees
  that went nowhere the run before are in the file, species matched went 16 to 21, and the tree
  list section proved it in four lines
- **A written row broke the canopy maths**, nine #VALUE! cells from Proposed M84 to the KPI row,
  fixed under the species rule with the height and the diameter off the schedule
- **FM-05 printed twice again** with the accounting reading one softscape schedule on all 20
  plots, which is what showed the double to be two printed rows of one schedule. That round
  read them as two rows under Proposed. The 1536 report's printed section showed the second
  under Street Design
- **Excel opened the file and did not calculate it**, every formula cell blank until Ctrl Alt
  F9, fixed with calcMode under the patcher rule
- **Every Meets KPI and Compliance cell read #NAME?** off `_xlfn.IFS`, sixteen cells, not the
  tool's doing and now counted in the report
- **Nothing in the report was what the tool read.** The last section prints every schedule
  this run read as the schedule prints it, every column aligned, with what was read off it
  and which subtotal row was taken and why, capped at 200 rows a schedule and named at the top

## What the 1536 run measured

Twenty mosque plots on MOSQUES, 2026-09-10 at 15:36, the first run whose report ended with
every schedule as printed. FM-05 refused on three species printed twice under Proposed, and
the printed section showed why. These are measurements, not reasoning.

- **FM-05's softscape schedule holds three groups.** Existing at row 3, four species, subtotal
  6. Proposed at row 9, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 19, CASSIA GLAUCA 3, subtotal 32.
  Street Design at row 14, ALBIZIA LEBBECK 10, BAUHINIA PURPUREA 20, CASSIA GLAUCA 4,
  CONOCARPUS 4, subtotal 38. TOTAL 76 at row 20
- **Its shrubs and lawn schedule holds the same third phase.** GRASS Proposed 96 over 117,
  Street Design 69 over 84, 165 over 201. SHRUBS AND GROUND COVER Proposed 361 over 450, Street
  Design 459 over 570, 820 over 1020. So the 165 and 820 the 1116 run wrote counted the street
- **DM-25 prints Existing, then Proposed, then Existing again**, in its softscape schedule
- Street Design is out of scope by Bader's decision, under the rule above, and the model will
  be corrected later. On STREETS it counts as Proposed, by the same decision, under the section
  that follows the rule

