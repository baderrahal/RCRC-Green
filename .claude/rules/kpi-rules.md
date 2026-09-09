---
paths:
  - src/RcrcGreen.Core/Kpi/**
  - src/RcrcGreen.Revit/Kpi/**
  - tests/RcrcGreen.Core.Tests/Kpi/**
---

# The rules the KPI tool holds

These load when something under a `Kpi/` folder is being touched. Everything about the repo
as a whole is in `CLAUDE.md`, and the Drawing Sheet's rules are in the other two files here.

## What it is for, and what this round is

The client issues an Excel workbook, GRP KPI Checklist, one template per asset type: existing
parks, future parks, healthcare, mosques, parking, schools, streets. Cells that come from Revit
carry a note saying where. The finished tool reads those values out of a model and writes them
into the workbook. **It creates nothing in the model, ever.**

This round is the KPI Scanner and nothing else. No Excel, no writing to the model, no filling.
It replaces nine assumptions with measurements, the way Scan Model did for the Drawing Sheet.

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

## The nine questions, and where each is answered

1. Which sheet holds the title block carrying PRX_COMPONENT and PRX_Plot_UID2. Section 3
2. Is PRX_COMPONENT on the title block instance or on the sheet itself. Section 3
3. What is PRX_Plot_UID2. Section 3
4. The real name of the neighbourhood parameter in Project Information. Section 2
5. Is there a link whose name holds 00, and what ID FILLED REGION means in it. Section 4
6. The exact schedule names in the model. Section 5
7. Which softscape field is the botanical name and which the quantity. Section 6
8. What separates existing trees from proposed ones. Section 7
9. What unit each area comes back in, raw and as printed. Sections 1 and 8

Section 9 is one line per question saying FOUND or NOT FOUND and where to look.
`KpiQuestions` builds those lines and decides nothing beyond whether the thing was found.

## The report is the deliverable

`KpiReport.Write` turns a `KpiScan` into nine numbered sections. Every heading carries its own
count, so a section that found nothing reads differently from one that was never filled in.
Above section 1 is READS THAT DID NOT HAPPEN, because a read that was refused would otherwise
print as a zero and a zero reads as an answer. Every skip in the Revit readers goes in there.

Every parameter that is read carries both its printed form, which is what a Properties panel
shows, and its raw form, which is feet or square feet whatever the project displays. Question 9
is the difference between the two, and `AreaUnits` is the one place square feet turn into
square metres.

## Three places a sheet value can live

The title block instance, the title block type and the sheet itself are all read, with a full
parameter tally and up to twenty examples per wanted name each. Sheet Width turned out to be an
instance parameter that a type cannot be asked for, and a parameter that is not on the element
asked reads as a value of zero. Asking all three is cheaper than guessing once.

## One schedule per name is read in full

The real model holds six schedules per plot over 160 plots. Every schedule gets its name,
category, fields and filters. Rows as printed, the elements listed and the areas off them are
read for one copy per name the workbook draws from, the first in name order that lists
anything, and the report says which. Regenerating a thousand schedules is a read nobody waits
for.

## The pane reaches Revit through its own door

`KpiPanel` is modeless and names no `Document`, no `Transaction` and no `ElementId`.
Everything it wants goes through `KpiRequestHandler` and its own `ExternalEvent`, never the
Drawing Sheet's, so a failure in one pane can never cost the other. Nothing leaves the
handler. Its pane has its own identifier and is registered through the same guard as the
Drawing Sheet pane, so a KPI pane that will not register leaves the ribbon working and the
KPI Checklist button saying why.

`KpiPaneWords` holds every line the pane shows. The pane formats nothing of its own.

## Shared and not changed

`RcrcGreen.Core` outside `Kpi/`, `PanelTheme` and `ReportFile` are shared with the Drawing
Sheet and this tool changes none of them. A change one of them seems to need goes in
`steps/log.md` with why, and the tool works round it. `PanelMetrics` is shared too and took
one added value, `HairlineAbove`, because a number written in a pane file is the fault that
made the first pane black on black.

## Do not name a KPI control Scan Model

Scan Model is a button inside the Drawing Sheet pane. Two buttons with one name doing
different things is a trap for the production team. The KPI one is KPI Scan.
