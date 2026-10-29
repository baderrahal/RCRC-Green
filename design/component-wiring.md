# Component and template wiring

Every place the PRX_Component value and the template list are read, matched, printed or
counted, as of main at `cd57a55`, written before this round changed any of them. One line each,
the file under `src/RcrcGreen.Core/Kpi/` unless another root is named.

## Where the value is read

- `src/RcrcGreen.Revit/Kpi/KpiSheetReader.cs` and `KpiPlotReader.cs` read the picked component parameter off each plot's sheets into plain strings
- `src/RcrcGreen.Revit/Kpi/KpiPanel.cs` `ComponentOn(plot)` hands the read value to every Core call the pane makes
- `KpiQuestions.cs` 77 to 212 tally `KpiNames.Component` on sheets and title blocks for the scan's question 1
- `src/RcrcGreen.Revit/Kpi/KpiPanel.cs` 2328 preselects the component picker on `KpiNames.Component`

## Where the value is matched

- `ComponentTemplates.cs` the value to template table, eleven rows in code, whole value, case blind, trimmed
- `ComponentFolders.cs` the value to folder table, the same eleven values again in code, the same comparison written a second time
- `ComponentFolders.FoldersOf` reads both tables together to say which folders a template reaches
- `ComponentFolders.OnlyFolderFor` files a plot with no component under its template's one folder
- `TemplateForComponent.For` preselects a template off the agreed value, cross checked by prefix
- `PlotsPerTemplate.For` places one plot: component decides, prefix cross checks, `ReferenceEquals` on the template
- `PlotsPerTemplate.Split` one share per ticked template, in `KpiTemplates.All` order
- `PlotPrefixes.cs` the prefix to template table, ten prefixes over the seven templates
- `PlotWorkbookPath.cs` 113 to 129 takes the folder off `ComponentFolders.For` and refuses a value it does not hold
- `TickingATemplate.cs` 53 and `TickingTheList.cs` 153 tick a template's plots through `PlotsPerTemplate.For`
- `SharedUid2.cs` 87 places each plot through `PlotsPerTemplate.For`
- `CreateWords.cs` 271 and 292 name the plots with no component and the one that files nowhere
- `src/RcrcGreen.Revit/Kpi/KpiPanel.cs` 1205, 2093 and 2153 place, preselect and split from the pane
- `src/RcrcGreen.Revit/Kpi/KpiRequestHandler.cs` 389 splits the press

## Where the value is printed

- `KpiCreatePlan.cs` 201 writes the plot's own read value into the Component cell, D3 on every template
- `PdfFill.cs` 317 to 319 writes the plot's own read value into the PDF's component field
- `KpiReport.cs` 387 section 3 prints each distinct value with the template `ComponentTemplates.For` gives
- `ComponentFolders.NoFolderFor` and `TemplateForComponent.NotInTheTable` name a value no table holds
- `TemplateForComponent.cs` 122 says the tool knows eleven values

Both print routes take the value off the plot's own reading, never off a table, so a new value
already prints in full as the model holds it. Nothing in either has to change.

## Where the template list is read or matched

- `KpiTemplates.All` the seven templates and their cell maps
- `RecognisedWorkbook.Recognise` 162 matches a file's first sheet name against `KpiTemplates.All`, then the file name breaks the park tie
- `TemplateForComponent.cs` 95 offers `KpiTemplates.All` when nothing is preselected
- `PdfForms.cs` 383 picks the PDF form by plot prefix, not by template, and stays so
- `KpiCreateReport.cs` 662 compares a template's name with STREETS for the Street Design note
- `AssetTypeNames.Display` in `KpiPaneWords.cs` turns a template name into the row label

## Where a count of templates or values is written

- `src/RcrcGreen.Revit/Kpi/KpiPanel.cs` 863 Templates matched N of `KpiTemplates.All.Count`
- `TemplateForComponent.cs` 122 the word eleven
- `ComponentFolders.cs` docstring, eleven values and eight folders
- `tests/.../Kpi/KpiTemplatesTests.cs` 13, 14 and 191 seven templates
- `tests/.../Kpi/KpiCreateTests.cs` 110, 111 eleven values, 230 and 243 seven candidates
- `tests/.../Kpi/ComponentFoldersTests.cs` 45 eleven folder rows
- `tests/.../Kpi/PlotPrefixesTests.cs` 63 and 64 seven templates by prefix and by component
- `tests/.../Kpi/PlotsPerTemplateTests.cs` 217 seven ticked
- `docs/rules/kpi-plots-1.md` K03 and `docs/rules/kpi-plots-2.md` K42, K43 the tables in prose
- Every other seven in Core is a measurement over the seven templates of 9 September and stays as history

## The four new templates against MOSQUES and SCHOOLS

Measured with `steps/runs/2026-10-28-sixty-templates/compare.py`, every sheet, every cell,
literal and formula, shared formulas expanded to the text each cell computes. The base files
are `C:\01-RCRC\KPI\TEMPLATES(R3)`, the new ones `C:\Users\bader\Downloads`.

- All four hold the same five sheets in the same order as their base: `<Mosques>` or `<Schools>`, the two tree lists, Criteria, Green Strategy KPI's
- MOSQUES LESS THAN 60 and SCHOOLS LESS THAN 60 are BYTE FOR BYTE the current MOSQUES and SCHOOLS, by SHA-256
- MOSQUES MORE THAN 60 differs from MOSQUES in five cells, `<Mosques>` H31 to H35, which read row 33 of Green Strategy KPI's where MOSQUES reads row 34
- SCHOOLS MORE THAN 60 differs from SCHOOLS in five cells, `<Schools>` H31 to H35, row 35 where SCHOOLS reads row 36
- No cell the map writes and no label the map reads differs. The cell maps carry over unchanged

**REAL difference from what the round expected:** the current MOSQUES and SCHOOLS templates
are already the LESS THAN 60 files, so DAILY MOSQUE, FRIDAY MOSQUE and SCHOOL plots get the
less than 60 per cent targets today. That is the client's file and the tool writes no target,
so it is a question for Bader, Q9, and nothing in code changes for it.

## The model's spelling of the four values

The local NG03 reports of 12 Sep print `DAILY MOSQUE LESS THAN 60%` on 4 sheets and
`DAILY MOSQUE MORE THAN 60%` on 7. No local report holds either school value, and NG05 has not
been read since the team added them, so the school spellings and NG05's mosque spellings are
UNKNOWN. The table holds the spelling with the space, and the spacing rule matches
`LESSTHAN` to it.

## Placing by rule, 29 Oct

Bader's rules in `install/component-rules.csv`, K76, read by `ComponentRules.cs`. Every lookup
above that went through `ComponentTable.RowFor` now goes through `ComponentTable.PlaceOf`: the
exact row, then nothing for a refused value, then the first rule whose whole words the value
holds, then nothing. `TemplateFor`, `FolderFor`, `Names`, `FoldersOf` and both report sections
read it, and `UnknownComponents.PlacedByRule` counts step 2's line.

- All fifteen rows of 28 Oct are answered the same way by a rule, so the shipped components.csv
  is its header alone. A test holds the fifteen and the seven new values to their answers
- `ComponentFolders.OnlyFolderFor` counts rule folders. A rule filing under the value reaches a
  folder per value, so MOSQUES and SCHOOLS reach no one folder. SCHOOLS reached SCHOOL before,
  while SCHOOL was its only row. No plot measured so far has an SC prefix and no sheet, Q14
- The spelling of the seven new values on NG05 is UNKNOWN. No local report of NG05 holds a KPI
  scan, so the tests carry the spellings as Bader typed them
- WALKWAY and street plots: nothing changed. A UID2 missing from the street reference file
  leaves D8 and F8 empty with one reason each and the workbook written, and a prefix with no
  form writes no PDF with one line. `StreetGapsTests` holds both
