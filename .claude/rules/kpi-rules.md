---
paths:
  - src/RcrcGreen.Core/Kpi/**
  - src/RcrcGreen.Revit/Kpi/**
  - tests/RcrcGreen.Core.Tests/Kpi/**
  - steps/kpi-templates.md
---

# The rules the KPI tool holds

These load when something under a `Kpi/` folder is being touched. Everything about the repo
as a whole is in `CLAUDE.md`, and the Drawing Sheet's rules are in the other two files here.

## What it is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, one template per asset type: existing
parks, future parks, healthcare, mosques, parking, schools, streets. Cells that come from Revit
carry a note saying where. The finished tool reads those values out of a model and writes them
into the workbook. **It creates nothing in the model, ever.**

The scanner came first and is still there. The pane now also fills: a plot picker, the three
choices the model cannot make, and a Create button that copies the template, patches it and
writes a report. **It still creates nothing in the model and never writes to the template.**

## Read every rule off the template, never off the notes copy

**Two files arrive: the template the team fills, and a reference copy carrying the green source
notes. THEY ARE NOT THE SAME WORKBOOK.** Measured on the reissued STREETS pair:

```
                          notes copy      template
existing canopy sum       M93             M102
native count over         H3:H91          H3:H101
```

So the annotations were made on an older file. **The notes say where a value comes from and
nothing else.** Where the two disagree about a range, a row or a sheet, THE TEMPLATE WINS, and
the disagreement is named in the log rather than reconciled quietly.

**This is the second time an annotated set and a production set have differed.** The first was
the seven of 9 September, where the annotated set carried the mapping in green text and the
production set carried no note cell at all, which is why `KpiTemplates` holds the map as data
and nothing reads a mapping out of a workbook. That the two sets can also disagree about a ROW
is new, and it is why every row fact is read off the file the run is filling.

## Count what happened, never what was planned

**Measured on that same run**: the pane and the report both read MOSQUES: Nothing was written.
20 of 21 plots wrote a workbook, with twenty workbooks on disk. One line held two records of one
fact, the row counting what the run set out to do and the sentence counting what it did.

`TemplateOutcome.Workbooks` is the count of plots that really wrote, and every word the row says
is read off it. Three answers rather than two: every plot wrote, SOME wrote with the ones that
did not named, or NOTHING was written, which is the one case the word refused still fits.

**COUNT WORKBOOKS WHERE THE UNIT IS A WORKBOOK.** The summary read 1 workbook written of 2
templates ticked on a press that wrote 98, because it counted templates and called them
workbooks. It counts workbooks and plots now, with the templates said as the thing the plots
were spread over.

## Adding printed numbers is allowed, working one out is not

Reading one plot adds nothing. Reading several means adding numbers the schedules printed,
which is allowed. Recomputing a number off the elements a schedule lists is never allowed and
happens nowhere, because those elements are RVT Link instances on this model.

What makes the addition safe is that **every plot's own number is printed beside the total and
the total must equal their sum.** `Totalled` carries both and `Adds` works the sum out again.
A total that does not equal its parts refuses the write. The tool does not write a total with a
note attached.

Trees merge on the group AND the botanical name, never the name alone. ALBIZIA LEBBECK is 1
existing and 13 proposed on DM-12, and merging on the name would put 14 in one tree sheet.

**Two plots reporting an identical raw area are flagged, never silently added.** MM-03 and
MM-04 both read 12182.05561411 in the 00 link. Either they are the same size or one region is
counted twice, and a double count nobody sees is the worst thing this tool can produce, so a
person confirms before anything is written. More than one region holding an area on one plot
refuses the same way **where the client's note names none of them**, under the section above.

## The report is the deliverable

`KpiReport.Write` turns a `KpiScan` into nine numbered sections. Every heading carries its own
count, so a section that found nothing reads differently from one that was never filled in.
Above section 1 is READS THAT DID NOT HAPPEN, because a read that was refused would otherwise
print as a zero and a zero reads as an answer. Every skip in the Revit readers goes in there.

**The create report ends with what the tool read.** Everything else in it is what the tool
concluded, and a species printed on two rows survived two rounds because nothing showed the
rows. `PlotReading.PrintedSchedules` carries every schedule the plot's numbers came off, row for
row, and `KpiCreateReport` prints each last, under EVERY SCHEDULE THIS RUN READ, AS THE SCHEDULE
PRINTS IT: the name, how many rows it printed and how many are shown, which rows were read as
species rows, which as subtotals, which were group rows and which rows were left out, and where
the TOTAL row was, and for the shrubs and lawn schedule which phase row was taken and which
left out and that the group total row was checked. Every
column, padded to its widest cell, with the row numbered the way the readers number it, the
heading row being 1. Two hundred rows a schedule at most, said in those numbers. The top of the
report says the section is there.

Every parameter that is read carries both its printed form, which is what a Properties panel
shows, and its raw form, which is feet or square feet whatever the project displays. Question 9
is the difference between the two, and `AreaUnits` is the one place square feet turn into
square metres.

## The pane reaches Revit through its own door

`KpiPanel` is modeless and names no `Document`, no `Transaction` and no `ElementId`.
Everything it wants goes through `KpiRequestHandler` and its own `ExternalEvent`, never the
Drawing Sheet's, so a failure in one pane can never cost the other. Nothing leaves the
handler. Its pane has its own identifier and is registered through the same guard as the
Drawing Sheet pane, so a KPI pane that will not register leaves the ribbon working and the
KPI Checklist button saying why.

`KpiPaneWords` holds every line the pane shows. The pane formats nothing of its own.

## What the pane puts on a button is escaped

WPF reads the first underscore in a button's text as an access key marker and swallows it, so
the pane offered PRXComponent, PRXPlot_ID, PRXPlot_UID, PRXPlot_UID2 and PRXPlot_NH. Five names
no model holds, on the one tool that turns on exact parameter names.

`PaneLabel.Escaped` doubles every underscore, which is WPF's own escape, and every string that
reaches a `Button` or a `CheckBox` goes through it. Only what is drawn: nothing compares the
escaped form against anything. It has a test over every name `KpiNames` holds, because a name
this misses is a name the pane shows wrongly.

## Shared and not changed

`RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile` are shared with the Drawing
Sheet and this tool changes none of them. A change one of them seems to need goes in
`steps/log-kpi.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
two added values, `HairlineAbove` and `WideLabelWidth`, because a number written in a pane file
is the fault that made the first pane black on black. The second is for the KPI pane's typed
boxes, where Prepared by came out as Prepared b running into its box at the shared 54, and it is
added rather than a widening of `LabelWidth`, which the Drawing Sheet uses in two places.

The rest of these rules moved, byte for byte, to `docs/rules/` on 2026-10-28. `docs/rules/index.md` holds one line per rule id and the file it lives in. Read a rule there by its id, and only its part. A new rule goes into the part for its topic, with its line in the index.
