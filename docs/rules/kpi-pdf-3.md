### ROWS 85, 92 AND 99 CARRY NO CANOPY FORMULA, AND IT IS THE CLIENT'S FILE

**THE 05:49 RUN ANSWERED IT OUTRIGHT AND THE ANSWER IS NOT A FAULT IN THE TOOL.** The glance
line the round before added read **52 of 150 forms got no Total Green cover**, and the reason it
counted, in the guard's own words:

```
Tree List - Existing row 85, a row the client's list already held: no cell on it carries
IF(ISBLANK(J85)," ",ROUND(PI()*(J85/2)^2,0)). The row holds
C85 = C84+0.01, O85 = IF(ISBLANK(B85)," ",N85*B85)
```

**Rows 85, 92 and 99 carry the numbering column and nothing else.** Somebody added rows to the
client's tree lists and copied C without the canopy pair beside it.

**SO A TREE ON ONE OF THOSE ROWS CONTRIBUTES NO CANOPY IN THE CLIENT'S OWN WORKBOOK, WHOEVER
FILLS IT.** The sheet computes its canopy off a formula those three rows do not carry, so a count
written into one of them reaches the tree total and reaches the canopy through nothing at all.
That is true of a person typing into the file by hand and it is true of this tool.

**REFUSING WAS RIGHT AND THE EXACT TEXT RULE STAYS.** Writing a number the workbook will not
compute is what the guard exists to stop, and a green cover that silently left three rows out
would be the same silent wrong number one step further along. **NOTHING IN THE TOOL CHANGES for
this**, which is Bader's decision of 15 September, and he is taking the rows to the client.

**THE ROW'S OWN PRINTED FORMULAS ARE THE MEASUREMENT AND THEY SAY ONE MORE THING.**
`O85 = IF(ISBLANK(B85)," ",N85*B85)` is the shape MOSQUES row 21 carries at
`M21 = IF(ISBLANK(B21)," ",L21*B21)`, two columns to the right, so on that sheet the canopy per
tree sits at N and the area at O rather than at L and M. **The guard never cared**, because it
looks for its text on ANY cell of the row and the only letter in that text is the DIAMETER
column, which comes off the sheet's own heading row. Whether the whole sheet uses N and O or only
the added rows do is UNKNOWN from one row, and it costs nothing either way.

**THIS CLOSES THE OPEN QUESTION THE ROUND BEFORE LEFT.** It was answerable only off a run, the
run answered it in one line at the top of the report rather than in 57,143 lines, and that line
is what the glance was built for.

### THE LIST HOLDS PLOTS NOTHING IN THE REPORT EVER MENTIONED, AND THEY ARE IN THE MODEL

**Measured on the 05:49 run, NG05: 165 plots offered, 156 ticked, a 57,143 line report.** The pane
showed MM-08 ticked with MM-09 to MM-15 unticked under it and NS-01 ticked below them, and **MM-09
to MM-15 appear NOWHERE in the report.** The only MM plots named anywhere in it are MM-01 to MM-08.
STREETS still wrote all 78 of its plots and PLOTS TICKED THAT WENT INTO NO WORKBOOK read 0.

**THE ROUND MESSAGE READ THAT AS: THEY ARE IN THE LIST AND NOT IN THE MODEL. THEY ARE IN THE
MODEL.** Three places were named to check and all three were checked, the third first.

#### THE THIRD IS RULED OUT: NOTHING DERIVES A PLOT NAME

There is **ONE production construction of the KPI plot list from a document**,
`KpiPlotReader.Plots` at `PlotsInTheModel.Of(OnSheets(document), OnSchedules(document))`. Every
other `PlotsInTheModel.Of` in the tool is an empty fallback taking two nulls. `OnSheets` adds
`ParameterReading.Printed` of `PRX_Plot_ID` off each sheet. `OnSchedules` adds
`filter.GetStringValue()` off the schedule filter whose field is `PRX_Ref Plot ID`. **The only
transformation applied to a plot string anywhere in that chain is `Trim()`, which can shorten and
never build.**

**EVERY PLOT IDENTIFIER BUILDER IN THE REPOSITORY IS DRAWING SHEET'S AND NO KPI FILE NAMES ONE.**
`PlotTickList.Generated` really does build `prefix + "-" + number.ToString("00")` in a loop over a
range, which is exactly the shape a run of MM numbers ending at 15 looks like, and it is reached
only from `DrawingSheetPanel`. Checked by name: **no file under `Core/Kpi` or `Revit/Kpi` mentions
`PlotTickList`, `PlotRange`, `PlotRegistry` or `PlotSelection` at all**, and the only `PadLeft` and
`ToString` with a format in those folders are a report column width, a report row number and a PDF
cross reference offset.

**THE PLOT NAMED `-` IS A READING TOO.** It is a schedule whose `PRX_Ref Plot ID` filter holds a
single dash, read verbatim. Nothing validates a plot against the two letters, dash, digits shape on
the way into this list, so whatever the filter holds becomes a plot. That is a fact about the model
rather than a name the tool made up.

#### THE FIRST TWO ARE BOTH TRUE AT ONCE, AND THAT IS WHY NEITHER LINE NAMES THEM

**THE TWO DISAGREEMENT LINES COUNT THE PLOTS NAMED BY EXACTLY ONE SOURCE.** Written out:

```
All              = sheets UNION schedules
OnSheetsOnly     = sheets MINUS schedules    the line "On a sheet and on no schedule"
OnSchedulesOnly  = schedules MINUS sheets    the line "On a schedule and on no sheet"
in NEITHER line  = sheets INTERSECT schedules
```

**So a plot named by BOTH is in neither line, and on that run the pane said nothing whatever about
156 of its 165 plots.** MM-09 to MM-15 are in neither line, which places them in the intersection:
**they are on a sheet AND on a schedule.** Both of the first two places named in the round message
are true of them, and being in both is exactly what made them invisible.

**THE TWO NINES ARE DIFFERENT NINES.** Nine plots were named across the two lines and nine plots
were unticked, which looks like an arithmetic that closes. Being named by one source is a fact
about which of two READS found a plot. Being unticked is a fact about whether `PlotsPerTemplate.For`
PLACED it in a ticked template. Nothing has ever made those two agree, and the run itself proves
they do not: seven of the nine unticked are MM-09 to MM-15, which are in neither line, and the park
plots the lines DO name are placed by their prefix and are ticked. **Those two lines have been on
the pane since September and nobody had held them against anything.**

#### AN UNTICKED PLOT APPEARS IN NO SECTION OF THE REPORT, BY CONSTRUCTION

Every section of the create report is over the TICKED plots. The per plot blocks, the PDF rows and
the glance run off `PlotOutcomes`, which is one per ticked plot. **THE SPLIT and PLOTS TICKED THAT
WENT INTO NO WORKBOOK run off `TemplateSplit`, which is built from the ticked list alone**, so
`Unplaced` cannot hold a plot nobody ticked and its heading carries the word TICKED for that
reason. **Reading 0 there is correct and says nothing at all about an unticked plot.** And
`KpiCreateRunSet` did not carry the model's plot list at all, so the report had nothing to print
one from.

**SO THE SILENCE WAS NOT EVIDENCE.** A plot in the list and not ticked produces exactly the file
that was measured, and so would a plot the model did not hold, and nothing in the file separates
them. That is what the section below is for.

#### WHY THEY ARE UNTICKED, AND WHICH OF THE ROUTES IT IS

Ticking a template row keeps a plot only where `PlotsPerTemplate.For(plot, component).Template` is
that row's template. Six routes leave a plot unticked with every row ticked:

```
1  its component is not one of the eleven ComponentTemplates holds, and the prefix may not
   answer in its place, so NEITHER places it
2  its component and its prefix name different templates, so NEITHER places it
3  no component and a prefix the table does not hold, which is the plot named "-"
4  held off by hand, the pane's own HandTicks
5  the ticking call did not run for that row, a file caught between the two park templates
6  its template has no recognised file in the templates folder, so it has no row to tick
```

**MM IS A STREETS PREFIX AND STREETS WROTE ITS 78 PLOTS, SO ITS ROW WAS TICKED AND SETTLED.** That
rules out 3, 5 and 6 and leaves **1, 2 and 4**. Routes 1 and 2 need a component on the plot's sheet
and route 4 needs nothing at all, so **being on a sheet does not narrow it**: a plot whose sheet
holds no component falls to the prefix and is ticked. **WHICH OF THE THREE IT IS CANNOT BE
DETERMINED FROM THIS REPOSITORY** and is not guessed at here.

**AND THE COMPONENT EVERY TICK RESTS ON IS READ OFF ONE SHEET.** `KpiPlotReader.ValuePerPlot` takes
the plot's FIRST sheet by sheet number and no other, so a plot whose sheets disagree about
`PRX_Component`, or whose first sheet holds none, is placed off that one sheet's value with nothing
recorded about the rest. Nobody has measured whether any plot's sheets disagree.

#### EVERY PLOT THE TOOL OFFERED, AT THE TOP OF THE REPORT

`PlotOrigins` is the section. One row per plot, **the plot, which of the two reads named it, and
whether it was ticked**, over the plots the model names AND the plots that were ticked, so a plot
in one and not the other is a row rather than a gap. It is the FIRST section of the body, above the
glance, because it is the list every other number in the file is a subset of.

**THE PLOT LIST IS READ OFF THE LIVE DOCUMENT AT THE PRESS**, the same read the plots button makes,
and the ticked half is counted off `PlotOutcomes`, which is what really happened. A plot that was
ticked and that the read at the press does not name is its own row reading NAMED BY NEITHER SOURCE,
and the line says it is a bug rather than a state, because the list is the union of the two reads
and no plot read off the model can land there.

**AND THE COUNTS ADD UP IN A WAY THE PANE'S TWO LINES NEVER COULD.** Four source states and two
tick states, each adding to the number of rows, printed as YES or as a bug in the tool. The section
also prints, in one sentence, what the pane's two lines count, so the next person holding nine
against nine reads why they are different questions before they start.

**WHAT IT STILL DOES NOT SAY IS WHY A PLOT WAS NOT TICKED.** That needs the component per plot
beside the plot list, and the component the pane holds was read at a different moment from the list
this section reads at the press, so printing a route off the two together would be a route worked
out from two records of one fact. **The two travelling from ONE read is a round of its own** and is
in `steps/log-kpi.md` as the next line.

### THE HAND CHOICES ARE ONE RECORD NOW, AND NOTHING MOVES THE TICKS WITHOUT IT

**THREE ROUTES HAD COME APART, AND THEY ARE ONE FAULT: nothing kept the held off list in step
with the real ticks, and nothing ever printed it.** All three were found by the verifiers of the
eighty second pass rather than by a run, which is why none of them had a measurement behind it.

**IT WAS A BARE SET OF THE PLOTS TAKEN OFF.** `HandTicks` replaces it, holding BOTH directions, a
plot taken off or put on by hand and never both, immutable the way `PlotTicks` already is.

```
a hand untick of one plot      TakenOff    and it drops any standing put on for that plot
a hand tick of one plot        PutOn       and it drops any standing taken off
ticking a template row         Ticked      skips the plots taken off by hand
unticking a template row       Unticked    KEEPS the plots put on by hand
Select all                     Forgotten   every hand choice goes
Clear                          Forgotten   every hand choice goes
the model changes              Forgotten   another model's DM-14 is not this one's
```

**1. UNTICKING A ROW WAS NOT SYMMETRIC WITH TICKING IT.** `Ticked` took the held off list and
`Unticked` took nothing, so a hand UNTICK survived a row tick and a hand TICK did not survive a
row untick. A person who picked three mosque plots by hand, then ticked the MOSQUES row for the
rest, then changed their mind about the row, lost their three with nothing said. **Unticking a
row undoes exactly what ticking it did now**, and ticking the row again restores the same state,
which is what symmetric means here and what a test writes out in both directions at once.

**2. SELECT ALL AND CLEAR BOTH FORGET EVERY HAND CHOICE.** Bader proposed it for Select all, on
the ground that pressing it is a person saying they want everything, **and the same argument
carries Clear, which is the half that was not asked about.** Both replace every tick, so a per
plot choice left standing behind either is a record that disagrees with what is on screen, and
the next row press acts on the disagreement: a plot unticked by hand once and brought back by
Select all was dropped again the moment any template row was ticked. **A record that disagrees
with the screen is worse than no record**, because the pane looks right.

**AND ONE PLACE SETS BOTH.** `KpiPanel.TickedByHand` moves the ticks and the record together and
nothing sets one without the other, which is the whole of the fault stated as a rule.

**3. THE LINE THAT WOULD HAVE ANSWERED ALL OF IT WAS BUILT AND SHOWN NOWHERE.**
`TickingATemplate.SomeOfThem` returns
`MOSQUES: 2 of the 3 plots that belong to it, the rest unticked by hand.` and its only references
were two lines of its own test file. **Green, and it had never reached a screen.** It is on the
workbook row now, under the tick box, as a NOTE rather than a refusal, and a row where every plot
is going in gets no line at all because a line about nothing is one the team reads past on every
other press.

**THE PANE COUNTS NOTHING.** `TickingATemplate.RowLine` takes the template, the ticks and the
component reader and hands back the sentence, so the two counts come off the split's own rule
rather than off a loop beside the control that draws them.

**IT IS THE LINE THE EIGHTY SECOND PASS SPENT A ROUND LOOKING FOR.** That round asked a 57,143
line report why MM-09 to MM-15 were unticked. This answers the held off half of that question on
the row a person is looking at when they press, and it has existed the whole time.

### SIX MEMBERS IN Core/Kpi HAVE NO CALLER IN src, AND THREE OF THEM BUILD A LINE

**Counted rather than guessed at.** Every public member of `Core/Kpi` returning a string or a list
of strings was held against every reference in `src` outside its own declaration, doc comments
left out. **245 such members, and SIX have no reference at all.** Every one of the six is tested
green, which is exactly the shape `SomeOfThem` had.

```
KpiPaneWords.ModelNamed        a line       the model's name or the words for none
CreateWords.SuggestedName      a line       the name a box offered, and THE BOX IS DELETED
RegionChoice.WhyUnchosen       a line       why no region was chosen, for the report's own column
ComponentTemplates.ValuesFor   a list       its docstring says the report and the pane say it
PlotPrefixes.PrefixesFor       a list       DELIBERATELY KEPT and already recorded as uncalled
WorkbookPatcher.ReadBack       not a line   a cell read off a written file
```

**`WhyUnchosen` is the one worth acting on and it is not acted on this round.** Its own docstring
says it exists "for a report that would otherwise print an empty cell and leave somebody guessing
which of the two cases it was", and the report never calls it, so **the report prints the empty
cell.** That is named here and in `steps/log-kpi.md` rather than fixed, because it was not what
this round was asked for.

**`SuggestedName` is the name box's, and the box is gone.** The shape it served no longer exists,
which is the second of the two reasons this repo deletes a thing, and it is named rather than
deleted for the same reason.

### A SECTION THAT COVERS WHAT WENT IN CANNOT TELL YOU WHAT DID NOT

**Recorded beside `PlotOrigins` because the next person reading a report will make the same
mistake.** The eighty second pass began from a premise that was wrong, and the way it was wrong is
worth more than the answer.

**The premise:** MM-09 to MM-15 are in the list and not in the model. **They are in the model**, on
a sheet AND on a schedule, which is why they are in neither disagreement line.

**The evidence was that they appear nowhere in a 57,143 line report. That is not evidence.** Every
section of that report is over the TICKED plots, and those seven were not ticked. **PLOTS TICKED
THAT WENT INTO NO WORKBOOK reading 0 is correct by construction**, because `Unplaced` is built
from the ticked list alone, and it was read as a fact about the model.

**And the two nines were a coincidence.** Nine named across the two disagreement lines, nine
plots unticked, and they are different nines: one is about which of two reads found a plot and the
other about whether the split placed it.

### WHAT IS IN THIS FILE, COUNTED OFF THE FILE

**Measured on the two runs**: the 18:15 report was 10,708 lines over seven plot blocks and the
19:52 report was 82,048 over 156, of which `WHAT THE WORKBOOK WILL COMPUTE FROM THIS` was 42,570,
52 percent of the file and 273 lines for each plot.

**ALL THREE OF THE RULES THE ROUND BEFORE ASKED FOR LANDED**, and the section still grew, because
they governed the FORMULAS AT RISK list and the section's other two lists were untouched while
the per plot fix multiplied every one of them by 156. The one that carried it is
`FORMULAS READING A ROW THIS RUN WROTE INTO`, 321 lines on a single plot of the 18:15 run.

**SO THE SAME RULE GOVERNS THAT LIST NOW.** `FormulaRepeats.Reading` groups it on the sheet, the
formula with every digit run replaced and the cells it reads with the same replacement, so one
formula filled down a column is one line carrying its span and its count. Two things are
deliberately no part of the shape. **A cell named twice in one formula is one read**, because
`IF(ISBLANK(J4)," ",ROUND(PI()*(J4/2)^2,0))` names J4 twice and the line printed it twice. **A
shared formula and its master are one shape**, because that is how Excel stores a column rather
than anything about the formula, and keying on it split every column into two lines.

**AND THE FILE OPENS WITH WHAT IS IN IT.** `ReportSections.Of` reads the report's own headings and
counts the lines under each, and `WHAT IS IN THIS FILE` prints one row per section, widest first:
the lines, how many blocks it is spread over and what one block costs. **It is the instrument and
not the cut.** It decides nothing, leaves nothing out, names no section, and a section added or
renamed appears in it with no change on that side. The counts add up to the body's own line count,
the opening above the first heading counted as its own row so the parts equal the whole, and the
contents block sits above the body and says it is not in its own counts.

**THE NEXT CUT IS MADE ON THOSE NUMBERS.** What the 19:52 file's other sections came to is UNKNOWN
and is not estimated here: the split was in no report file, and a number reasoned out would be a
guess. The next run prints it.

### The units of the fields nobody fills, on record

**A FIELD WITH NO NOTE IS NOT FILLED** and that is unchanged. These are recorded for the day the
client annotates one, so nobody has to measure the page again, and they are read off the page by
position like every unit above.

```
Roads   Sidewalk, Median, Sidemedian, Empty areas   NO UNIT PRINTED AT ALL
        Water tanks                                 nr

Parks   the two people counts                       nr
        Cycling paths, Pedestrian paths             lm
        Toilets, Kiosk, Play grounds, Fitness area,
        Muga sport field, Running track             Area (m2)
        Water tanks                                 nr

Open    Length, Cycling paths, Pedestrian paths,
        Maintenance road                            km
        Toilets, Kiosk, Exhibition spaces,
        parking provided                            nr
        Seating areas, play areas                   m2
        Bridges, Catwalk                            Lenght (m)
        Water tanks                                 nr
```

**CYCLING PATHS AND PEDESTRIAN PATHS ARE lm ON THE PARKS FORM AND km ON THE OPEN SPACES FORM.** The
same row name, two units, on two forms this one tool fills. Nothing writes them today so it costs
nothing now, and it would cost a thousandfold error in a client document the day somebody adds a
note to one of them and reads the other form's unit. **The Bridges and Catwalk row spells Lenght**,
which is the client's spelling and is written here as it is, the same rule the model's
PRX_Furniture Lenght already follows.

