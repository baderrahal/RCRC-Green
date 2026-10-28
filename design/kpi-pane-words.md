# KPI pane, every word and where it lives now

Every string the KPI pane could show before this round, with its source, and its home after it.
Line numbers are the file as it stood at the start of the round, branch `kpi-pane-redesign` at
`c754a20`. Sources are under `src/RcrcGreen.Core/Kpi/` unless they start with `Revit/`, which is
`src/RcrcGreen.Revit/Kpi/`.

Homes:

- **NEW** the new pane text, held in Core, at most 10 words a sentence
- **TOOLTIP** the full text moves to the tooltip of the named control
- **REPORT** the full text moves to the run's report under the named heading
- **REPEAT** removed from the pane because the same fact shows elsewhere, which is named

`BEFORE THE PRESS` is the new report section, after `THIS RUN AT A GLANCE`. It holds the folders
and files the press used with what each means, the parameters, and the full text of every
warning box the pane showed for that press.

## The header and the strip

| Was | Source | Home |
|---|---|---|
| Model name, `No model open` | `KpiHeader.cs:84`, `KpiPaneWords.cs:14`, `Revit/KpiPanel.cs:389` | NEW, header line 2, `KpiPaneWords.ModelNamed`, cut with an ellipsis |
| `Not read yet. Counting every element is the read itself, so the count waits for one.` | `KpiHeader.cs:91` | NEW `Not read yet` as step 2's status. The why goes to TOOLTIP on the step 2 cell |
| `Open a model. This pane reads its name, which costs nothing, and nothing else.` | `KpiHeader.cs:98` | NEW `No model open` as step 2's status. TOOLTIP on the step 2 cell |
| `Read at 08:37:12, N elements in S seconds.` | `KpiHeader.cs:127` | NEW step 2, the three boxes Plots, Elements, Time, and the status `Read at HH:mm` |
| `Counted at ..., The plots were read, and the elements counted. The model itself was not read.` | `KpiHeader.cs:118` | REPORT, BEFORE THE PRESS, line `The plots read` |
| none | | NEW header line 1 `KPI Checklist`, line 3 `Build <version>` off `RcrcGreen.Revit` |

## The status line

| Was | Source | Home |
|---|---|---|
| `Open a model. This pane reads its name as soon as it is shown.` | `KpiPaneWords.cs:16`, `Revit/KpiPanel.cs:211` | REPEAT, step 2's status `No model open` says it |
| `No open document. Open a model.` | `KpiPaneWords.cs:18`, `Revit/KpiPanel.cs:368`, `Revit/KpiRequestHandler.cs:158` | NEW `No model open`, `StatusLine.NoModel` |
| `Reading the plots from this model. On a large model this takes a moment.` | `CreateWords.cs:85`, `Revit/KpiPanel.cs:1046` | NEW `Reading the model`. The long form is TOOLTIP on the status line |
| `Read. N plots in this model.` | `CreateWords.cs:95`, `Revit/KpiPanel.cs:1982` | REPEAT, step 2's Plots box and `Read at HH:mm` |
| `Creating.` | `CreateWords.cs:110`, `Revit/KpiPanel.cs:1944` | NEW, kept, the first line of the progress window and the status |
| `N workbooks written of M plots ticked ... Report: <path>` | `CreateWords.cs:349`, `Revit/KpiRequestHandler.cs:467` | NEW `N created, M failed`, full text TOOLTIP on the status line. The report prints the same counts under its run accounting, REPEAT there |
| `Cannot create. No model is open. ...` | `CreateWords.cs:437`, `Revit/KpiRequestHandler.cs:329` | NEW the one first reason, `CreateBlocked`, full text TOOLTIP on the status line |
| `Revit refused that. <message>` and the three like it | `Revit/KpiRequestHandler.cs:185` to `201` | NEW `Revit refused the request` and its siblings, `StatusLine`. Message TOOLTIP on the status line |
| `That request failed and was stopped here ... <type>. <message>` | `Revit/KpiRequestHandler.cs:1280` | NEW `The request failed and was stopped`. Type and message TOOLTIP on the status line |
| `The folder could not be remembered. <file> beside the installed add-in refused the write.` (four Browse buttons) | `Revit/KpiPanel.cs:889`, `982`, `1005`, `2176`, `2201` | NEW `Could not remember that choice`. Full text TOOLTIP on the status line |
| `That could not be opened: <path>. <message>` | `Revit/KpiPanel.cs:1655` | NEW `Could not open that`. Full text TOOLTIP on the status line |
| Progress: `Reading DM-44, plot 3 of 18, 11%.` | `ProgressWords.cs:94` | NEW `Creating 3 of 18` with `DM-44` under it, from the new `Advanced` callback. The words stay for the handler's count |
| Progress: `Section 4 of 9, ...`, `Sections 5 to 8 of 9, ...`, `Copying the template.`, `Writing the cells.`, `Reading the written cells back.`, `Checking the workbook's own formulas.`, `Reusing the readings already held.`, `Reusing the scan already held.`, `Adding the plots up.`, `Writing the report.` | `ProgressWords.cs:31` to `105` | REPEAT. The scan report prints every section and the checklist report prints `Readings:` reused or read. The window shows the count and the plot only, as asked |
| Scan headline `N sheets, ... 9 questions ... Report at <path>.` | `KpiPaneWords.cs:57`, `Revit/KpiRequestHandler.cs:230` | REPORT, BEFORE THE PRESS, line `The scan` |
| `Not read yet.`, `Reading the whole model...`, `Create reads the model and writes...`, `Read at ...` | `KpiPaneWords.cs:24`, `26`, `30`, `43` | Shown nowhere before this round. Kept, and `ReadOnly` is REPORT, BEFORE THE PRESS |

## The step bar

| Was | Source | Home |
|---|---|---|
| `1 Setup`, `2 Read`, `3 Tick`, `4 Create` | `KpiSteps.cs:119` | NEW circles over the names Setup, Read, Plots, Create. Tick is renamed Plots, member and title |
| `done` under a finished step | `KpiSteps.cs:87` | NEW, a drawn tick in a tinted circle. The word is kept on `Mark` for the tests |
| `Work on step N, <title>.` and `WhyNot` as the cell tooltip | `Revit/KpiPanel.cs:580` | NEW `KpiStepState.Tooltip`, the state in at most 10 words |
| Step heading `N  Title   summary` | `Revit/KpiPanel.cs:474` | REPEAT, the bar names the step and the tooltip carries the summary |
| `Set the templates folder and the output folder in step 1 first.` | `KpiSteps.cs:262` | NEW `Set the templates and output folders first.` |
| `Read the model in step 2 first.` | `KpiSteps.cs:276`, `292` | NEW `Read the model first.` |
| `In step 3, no workbook is ticked and no plot is ticked.` | `KpiSteps.cs:291` | NEW `Select an asset type and a plot.` and the two halves |
| Summaries `3 not set`, `all set`, `not read`, `no model is open`, `N plots read`, `nothing ticked`, `1 workbook, 154 plots`, `pressed` | `KpiSteps.cs:309` to `340` | TOOLTIP on each cell, inside `Tooltip`. `1 workbook` reads `1 asset type` now |

## Step 1, Setup

| Was | Source | Home |
|---|---|---|
| `Templates`, `Output folder`, `Forms folder`, `Street reference`, `Plot list` captions | `Revit/KpiPanel.cs:607`, `830`, `865`, `911`, `945` | NEW `Templates`, `Output`, `PDF forms`, `Street reference`, `Plot list` with `Optional` |
| `No folder set`, `No file set` in the path line | same lines | NEW hint `Not set`, red border. A remembered path that is gone is `Not found` |
| `The remembered street reference file is not there any more: <path>` | `Revit/KpiPanel.cs:921` | NEW hint `Not found`. Path TOOLTIP on the path box |
| `The remembered plot list file is not there any more: <path>` | `Revit/KpiPanel.cs:955` | NEW hint `Not found`. Path TOOLTIP on the path box |
| Browse tooltips, four long sentences | `Revit/KpiPanel.cs:608`, `831`, `866`, `912`, `946` | TOOLTIP, unchanged, on each Browse button |
| `No template folder is set. Press Browse and point at...` | `TemplateWords.cs:13`, `Revit/KpiPanel.cs:612` | NEW `Not set`. Full text TOOLTIP on the Templates path box |
| `THIS IS THE TEMPLATES FOLDER. A filled workbook written here...` | `TemplateWords.cs:47`, `Revit/KpiPanel.cs:841` | NEW warning box `Output folder is the templates folder`, full text its tooltip and REPORT, BEFORE THE PRESS. It is a note and stops nothing, in steps 1 and 4 |
| `Written to <folder>. A file already there under this name is overwritten...` | `TemplateWords.cs:71`, `Revit/KpiPanel.cs:844` | TOOLTIP on the Output path box, REPORT BEFORE THE PRESS |
| `One workbook per plot, under <root>, in a folder named for the plot...` | `CreateWords.cs:147`, `Revit/KpiPanel.cs:850` | TOOLTIP on the Output path box, REPORT BEFORE THE PRESS with the real root |
| `One PDF per plot beside its workbook...`, `Note. No forms folder is set...` | `TemplateWords.cs:58`, `Revit/KpiPanel.cs:871` | TOOLTIP on the PDF forms path box, REPORT BEFORE THE PRESS. Step 4 says `no PDF` |
| Street reference `InWords`, the file's rows, or why it was not read | `StreetReference.cs:430`, `Revit/KpiPanel.cs:925` | NEW hint `<n> rows`. Full text TOOLTIP on the path box, REPORT BEFORE THE PRESS |
| `The plot list: <InWords>` or `<Why>` | `TickingTheList.cs:87`, `PlotListFile.cs:169`, `Revit/KpiPanel.cs:962` | NEW hint `<n> plots`. Full text TOOLTIP, and a file that cannot be read is a warning box in step 2 |
| `What the workbook carries` | `Revit/KpiPanel.cs:619` | NEW section `Workbook details` |
| `Date`, `Prepared by`, `Position` | `Revit/KpiPanel.cs:620` to `622`, again at `1241` to `1243` | NEW, once, in step 1. The step 3 copy is REPEAT |
| `Not set: the templates folder is not set. ...` and the five sentences | `KpiSteps.cs:151` to `166`, `207`, `Revit/KpiPanel.cs:627` | NEW footer status `All set` or `<n> to set`. Each sentence is TOOLTIP on its own path box |
| `The date, the prepared by and the position are typed once...` | `CreateWords.cs:190` | TOOLTIP on the `Workbook details` title, REPORT BEFORE THE PRESS |
| none | | NEW `<n> templates found`, footer `Next: Read` |

## Step 2, Read

| Was | Source | Home |
|---|---|---|
| `Plots` heading | `CreateWords.cs:60`, `Revit/KpiPanel.cs:1020` | NEW section `Model read`, with `Read again` |
| `This model has not been read yet. Reading it walks every sheet...` | `CreateWords.cs:460` | NEW status `Not read yet`. Full text TOOLTIP on the `Read model` button |
| `No model open. Open one and this pane reads its name...` | `CreateWords.cs:468` | NEW status `No model open`. Full text TOOLTIP on the disabled button |
| `Read this model` | `CreateWords.cs:79` | NEW `Read model` |
| `The sheets and the schedules name the same N plots.` | `CreateWords.cs:518` | REPEAT, the Plots box. Full text TOOLTIP on it |
| `The sheets and the schedules do not name the same plots...` with the two lists | `CreateWords.cs:522` to `533` | NEW warning boxes `N plots on a sheet but no schedule` and `N plots on a schedule but on no sheet`, ids under each, full text tooltip, REPORT BEFORE THE PRESS |
| Schedules whose plot could not be read | `PlotList.cs`, `SchedulesNotRead` | NEW warning box `N schedules could not be read`, full text tooltip |
| `No plot in this model. Open a model that holds one.` | `CreateWords.cs:165` | NEW warning box `This model names no plot`, full text tooltip |
| Link note, `OnThePane` | `LinksLoaded.cs:104`, `Revit/KpiPanel.cs:657` | NEW warning box, `N of M linked models not loaded` and its siblings, link names under it, full `Warning` its tooltip and REPORT BEFORE THE PRESS. The checklist report's own link block is REPEAT |
| `The plot list: <InWords>` | `Revit/KpiPanel.cs:647` | NEW line `In plot list  154 of 163`. Full text TOOLTIP on that line |
| `The plot list: <Why>` | `Revit/KpiPanel.cs:648` | NEW warning box `Plot list cannot be read` |
| Listed plot the model does not name | `TickingTheList.cs:115` | NEW warning box `N listed plots are not in the model`, ids |
| Plot listed twice, `PlotListFault.InWords` | `PlotListFile.cs:54`, `TickingTheList.cs:113` | NEW warning box `N plots are listed twice`, ids |
| Line that is not a plot | `TickingTheList.cs:112` | NEW warning box `N lines are not plot ids`, line numbers |
| `What the values come from` | `Revit/KpiPanel.cs:1225` | NEW section `Values from` |
| `Component`, `Reference`, `Location` and their button rows | `Revit/KpiPanel.cs:1227` to `1239`, `2126` | NEW, three 32 px dropdowns, same preselection |
| `   nothing in this model holds that word` | `Revit/KpiPanel.cs:2134` | NEW `Nothing in this model holds that word`, a disabled dropdown's tooltip |
| `What each holds on <plot>, the first ticked plot:` and the values | `CreateWords.cs:418`, `Revit/KpiPanel.cs:1263`, `1267` | TOOLTIP on each dropdown |
| `No plot is ticked, so there is no value to show here...` | `CreateWords.cs:406` | TOOLTIP on each dropdown |
| none | | NEW boxes `Plots`, `Elements`, `Time`, line `Templates matched  7 of 7`, `Show in report`, `In the report after Create`, footer `Next: Plots`, status `Read at HH:mm` |
| none, question 4 | `KpiPaneWords.ReadsSheetsAndSchedules` | NEW muted line `Reads sheets and schedules. Nothing changes.` in the body before a read, the button's tooltip on it |

## Step 3, Plots

| Was | Source | Home |
|---|---|---|
| `GRP KPI Checklist templates` | `Revit/KpiPanel.cs:691` | NEW section `Asset types` with `<ticked> of <total>` |
| `No .xlsx in this folder.` | `TemplateWords.cs:17` | NEW `No templates in this folder`, full text tooltip |
| `N workbooks in the folder, M recognised.` | `TemplateWords.cs:163` | REPEAT, `<n> templates found` in step 1 and the section count |
| Listing `InWords`, opened N times over M redraws | `TemplateListing.cs:108`, `Revit/KpiPanel.cs:717` | REPEAT, the report prints it under `WHERE EVERY VALUE CAME FROM` |
| `_whyThisTemplate`, the preselection reason | `TemplateForComponent.cs:28`, `Revit/KpiPanel.cs:720`, `1759` | TOOLTIP on the `Asset types` title, REPORT BEFORE THE PRESS |
| Row `file.xlsx   recognised as ...` | `RecognisedWorkbook.cs:72`, `Revit/KpiPanel.cs:728` | NEW the display name in sentence case, `AssetTypeNames`. The file name and `InWords` are the tick box TOOLTIP |
| `Tick this workbook. Several can be ticked at once.` | `Revit/KpiPanel.cs:732` | TOOLTIP, inside the tick box tooltip |
| A row's `Reason` when it cannot be ticked | `RecognisedWorkbook.cs:57` | TOOLTIP on the disabled tick box |
| `<template>: N of the M plots that belong to it, the rest unticked by hand.` | `TickingATemplate.cs:142`, `Revit/KpiPanel.cs:1801` | NEW the row's count reads `19 of 20`. Full text TOOLTIP |
| `<file> is named for neither park template or for both...` | `TemplateWords.cs:155`, `Revit/KpiPanel.cs:749` | NEW a row with two buttons. Full text TOOLTIP on the row |
| `What <template> would fill` and every line of `WouldFill` | `TemplateWords.cs:83` to `150`, `Revit/KpiPanel.cs:781`, `786` | TOOLTIP on the tick box, REPORT BEFORE THE PRESS |
| `Plots` heading again | `Revit/KpiPanel.cs:1066` | NEW section `Plots` with `<ticked> of <total>` |
| `Select all`, `Clear` | `CreateWords.cs:62`, `64` | NEW `All`, `None`, same press |
| `Tick the list` and its tooltip | `CreateWords.cs:71`, `Revit/KpiPanel.cs:1095` | NEW `Plot list`. Tooltip kept, or the reason it is off |
| `N of M plots ticked.` | `PlotList.cs:183`, `Revit/KpiPanel.cs:1107` | REPEAT, the section count and the footer status |
| Plot tick boxes, the id alone | `Revit/KpiPanel.cs:1118` | NEW row: tick, id in Consolas, asset type, or `No type`, or `Shared id` |
| `A workbook row has been ticked or unticked since Tick the list was pressed...` | `ListTickFreshness.cs:85`, `Revit/KpiPanel.cs:1380` | NEW warning box `Plot list tick is out of date` with `Tick list again`, full text its tooltip |
| The second `Tick the list` button under it | `Revit/KpiPanel.cs:1385` | NEW the `Tick list again` link |
| `TickingTheList.Lines`, every line above Create about the list | `TickingTheList.cs:102`, `Revit/KpiPanel.cs:1404` | NEW warning boxes, split by kind, in step 2 (the list itself) and step 4 (what the press would do) |
| Shared UID2 lines per plot, stopped and filed apart | `SharedUid2.cs:251`, `340`, `Revit/KpiPanel.cs:1433` | NEW warning boxes in step 4 and `Shared id` on the plot row. Full text tooltip |
| none | | NEW search box, footer `Next: Create`, status `<n> plots selected` |

## Step 4, Create

| Was | Source | Home |
|---|---|---|
| `This press would write N workbooks from M workbook templates. ...` | `KpiWillWrite.cs:37`, `Revit/KpiPanel.cs:811` | NEW the big number with `workbooks, each with its PDF` or `workbooks, no PDF`. Unplaced plots are their own warning box |
| `No workbook is ticked, so this press would write nothing.` | `KpiWillWrite.cs:23` | NEW `No asset type selected` on the status line |
| `No ticked plot belongs to any ticked workbook...` | `KpiWillWrite.cs:26` | NEW the big number reads 0 and the status `No plot selected` or the warning below |
| `Create` heading and button | `CreateWords.cs:102` | NEW button `Create <n> workbooks` |
| `Cannot create. ...` | `CreateWords.cs:437`, `Revit/KpiPanel.cs:1285` | NEW the one first reason on the status line, `CreateBlocked`, full text tooltip |
| Per template row `MOSQUES: 20 plots, DM-11 to FM-09. The report names them.` | `CreateWords.cs:199`, `Revit/KpiPanel.cs:1294` | NEW table row, Asset type, Plots, Range. The report prints the split, REPEAT |
| `<template>: <WhyNothing>` for a ticked template no plot belongs to | `PlotsPerTemplate.cs:124`, `Revit/KpiPanel.cs:1296` | NEW warning box `<Asset type> has no ticked plot`, full text tooltip |
| Unplaced `<plot>: <why>` | `PlotsPerTemplate.cs:52`, `Revit/KpiPanel.cs:1301` | NEW two warning boxes: `N plots have no asset type`, ids, blocks, and `N plots belong to an asset type not ticked`, ids, a note |
| Split refusals, a plot in two workbooks | `PlotsPerTemplate.cs:130`, `Revit/KpiPanel.cs:1304` | NEW warning box `A plot falls into two asset types`, blocks |
| `N ticked plots carry no component...` and `N of those file under...` | `CreateWords.cs:260` to `270` | NEW warning box `N plots have no component. Saved in their asset type folder.`, ids |
| `<plot> WILL BE READ AND WRITTEN NOWHERE: ...` | `CreateWords.cs:274` | NEW warning box `<plot> can be filed nowhere`, blocks, full text tooltip. The `Contains("WRITTEN NOWHERE")` test in the pane is gone, the kind is `PaneWarning.Blocks` |
| Template outcome rows after a press | `CreateWords.cs:286`, `Revit/KpiPanel.cs:1324`, `1473` | REPEAT. The report prints each under `TEMPLATE:`, and the result shows Created and Failed |
| Link note again | `Revit/KpiPanel.cs:1336` | NEW the same warning box as step 2 |
| `<template>: This template names no area cell...` | `CreateWords.cs:397`, `Revit/KpiPanel.cs:1343` | NEW warning box `<Asset type> reads no area`, full text tooltip. No template draws it today |
| Ticked and not on the list | `TickingTheList.cs:81` | NEW warning box `N ticked plots are not on the list`, ids |
| Listed plot that gets no workbook or no PDF | `TickingTheList.cs:151` | NEW warning box `N listed plots get no workbook or PDF`, ids |
| Groups no sheet takes, `GroupsLeftOut` | `CreateWords.cs:604`, `Revit/KpiPanel.cs:1602` | NEW warning box `Rows with no tree list sheet were left out`, plot ids, full text tooltip. The report prints the full count, REPEAT there |
| Reconciliation refusals of the press before | `Revit/KpiPanel.cs:1573` | NEW the failed line's short reason. Full text TOOLTIP on the line, the report's reconciliation, REPEAT |
| `   <plot>, pick its intervention area:` and the buttons | `Revit/KpiPanel.cs:1669` | NEW under the failed line, `Two regions hold an area. Pick one.`, same buttons, same re-press |
| `CONFIRM BEFORE WRITING. N sets of chosen plots report the same area.` and the lines | `CreateWords.cs:532`, `Revit/KpiPanel.cs:1580` | NEW error box `Plots report the same area`, ids, full text its tooltip and REPORT BEFORE THE PRESS of the next press |
| `These areas are right, write anyway` | `Revit/KpiPanel.cs:1587` | NEW `Write anyway`, which now also re-presses |
| none | | NEW section `Ready to create`, the muted `Files with the same name are replaced.`, status `Nothing in the model changes`, `Show in report` |

## The result

| Was | Source | Home |
|---|---|---|
| Boxes `ready`, `not ready` | `KpiResults.cs:82`, `84`, `Revit/KpiPanel.cs:1462` | NEW boxes `Created` and `Failed`, off the outcome. Ready moves to the line `Ready to send  33 of 154` |
| `ready N, not ready M, of K plots on the list` | `KpiResults.cs:93` | NEW `Ready to send  33 of 154`. The full line TOOLTIP on it |
| `No plot list file was set for this press...`, `...could not be read...`, `...names no plot...` | `KpiResults.cs:68` to `76` | TOOLTIP on the result banner. The line `Ready to send` is not drawn |
| `Every plot on the list is ready.`, `N plots are not ready to send:` | `KpiResults.cs:80`, `113` | REPEAT, the `Ready to send` count. The report prints THE PLOT LIST |
| Each not ready row, `PlotListRow.InWords` | `PlotListRows.cs:94`, `Revit/KpiPanel.cs:1497` | REPORT, THE PLOT LIST, word for word already, REPEAT. A failed plot also gets its own line |
| `Open the output folder`, `Open the report` | `Revit/KpiPanel.cs:1516`, `1527` | NEW `Open output folder`, `Open report`. Tooltips kept |
| `This press wrote no report.` | `Revit/KpiPanel.cs:1532` | TOOLTIP, unchanged |
| `Back to step 3, Tick` | `Revit/KpiPanel.cs:1545` | NEW `New run`, which also clears the result |
| none | | NEW banner `All workbooks created` or `<n> workbooks not created`, section `Failed`, one short reason per plot from `FailedReasons`, `Retry failed` |

## The progress window

| Was | Source | Home |
|---|---|---|
| Title `RCRC Green KPI` | `ProgressWords.cs:53` | NEW, kept |
| Every progress line | `Revit/KpiProgressWindow.cs:79` | NEW `Creating <i> of <n>`, the plot id under it in Consolas. The other lines REPEAT, see the status line table |

## Dialog titles

| Was | Source | Home |
|---|---|---|
| `The folder holding the GRP KPI Checklist templates` | `Revit/KpiPanel.cs:2193` | NEW, kept |
| `The folder the filled GRP KPI Checklist workbooks go to` | `Revit/KpiPanel.cs:2168` | NEW, kept |
| `The folder holding the Projects Basic Data forms` | `Revit/KpiPanel.cs:880` | NEW, kept |
| `The team's Scope_Validation workbook`, filter `Excel workbook (*.xlsx)` | `Revit/KpiPanel.cs:994` | NEW, kept |
| `The team's plot list, one plot per line`, filter `Text file`, `Every file` | `Revit/KpiPanel.cs:971` | NEW, kept |

## Every control the pane had, and where it is now

| Control | Was | Now |
|---|---|---|
| Model name and read line | strip | header lines 2, and step 2 |
| Status line | docked at the foot | footer, under the main button, 11 px |
| Step bar, four buttons | top of the body | step row, four circles, same click |
| Templates Browse | step 1 | step 1, Folders |
| Output folder Browse | step 1 | step 1, Folders, `Output` |
| Forms folder Browse | step 1 | step 1, Folders, `PDF forms` |
| Street reference Browse | step 1 | step 1, Files |
| Plot list Browse | step 1 | step 1, Files, `Optional` |
| Date, Prepared by, Position boxes | step 1 and step 3 | step 1, Workbook details, Date alone then the two side by side |
| Read this model | step 2 | footer `Read model`, and `Read again` beside `Model read` |
| Component, Reference, Location button rows | step 3 | step 2, `Values from`, three dropdowns |
| Workbook row tick boxes | step 3 | step 3, Asset types, two column grid |
| Park pick buttons | step 3 | step 3, the same two buttons in place of the tick |
| Select all, Clear | step 3 | step 3, `All`, `None` |
| Tick the list | step 3 | step 3, `Plot list` |
| Tick the list again | step 3 | step 3, the warning box's `Tick list again` |
| Plot tick boxes | step 3 | step 3, the plot rows |
| Create | step 4 | footer `Create <n> workbooks` |
| Region pick buttons | step 4 and the result | the result, under the failed plot's line, and step 4 while a held run asks |
| These areas are right, write anyway | step 4 and the result | error box `Write anyway`, re-presses |
| Open the output folder, Open the report | the result | the result, `Open output folder`, `Open report` |
| Back to step 3 | the result | footer `New run` |
| none | | search box, `Read again`, `Show in report`, `Retry failed` |
