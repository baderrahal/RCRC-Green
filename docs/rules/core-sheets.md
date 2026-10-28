## A definition sits between reading a schedule and writing one

Six of the things under a plot are schedules, and they sit under Schedules and Quantities in the
project browser rather than under Views, which is why a scan that walks views alone finds none
of them.

`ScheduleDefinition` holds the category, the fields in order, the filters and the link setting
as plain values. Capture fills one in from a schedule that exists, create builds one in the
model, and duplicating is the two run back to back.

Writing a duplicate-the-nearest routine instead would have been shorter and would have left
the tool useless on a project holding no schedules, which is the next one it will be pointed
at. Loading a definition from a file is then a small round. Built the other way round it is a
rewrite.

`ForPlot` changes only the filter rule whose value is a plot identifier. Every other rule is
carried across untouched, because HARDSCAPE and SHRUBS AND LAWN are both category Floors and
that second rule is the only thing telling them apart. Field names are copied exactly,
including the one spelled PRX_Furniture Lenght in the model, or the field is not found.

**A filter value carries the kind it has to go back as.** `FilterValue` is text, a whole number,
a number or an element reference, and each goes back to Revit as the kind it came out as. Flattening
them all to a string lost two schedules: both filter on PRX_Included In Budget equals Yes, which
is a Yes/No parameter Revit holds as the integer 1, and handing the word back was answered with
"the filter value is not valid for the field and filter type". Only a text value can name a
plot, so a whole number is never mistaken for one and swapped.

**A field carries what kind of field it is.** `ScheduleFieldEntry` is a parameter or a
calculated field, meaning a formula, a percentage, a count or a combined parameter. A calculated
field is defined inside the schedule that holds it, so Revit never offers it to a new one and no
name matching will find it. Three schedules came out short of one and the report blamed the
category, which sends somebody to look in the wrong place.

**The category is a number, not a name.** `CategoryBuiltInValue` is Revit's own number for it.
KERBS is built on Slab Edges, a name lookup found nothing, and the schedule was refused with
"this model has no category named Slab Edges" on a model that has it. `CategoryName` is kept for
the report only.

Reading a schedule as PRINTED is a different job and Core does that too, in `ScheduleRows` and
`ScheduleGroups`. **Never read a schedule value by cell position**, which is the rule in
`CLAUDE.md`, stated there and nowhere else.

## A sheet number is built from the plot identifier, never proposed from the model

The scheme is the user's, set after the marker scheme before it proved wrong twice over:
the user had already said the numbering must be automatic, and a per-plot dropdown made
step 1 long. The view code, then the plot identifier with its dash dropped, then A, B, C
within one code in sheet order, and no letter when the code holds a single sheet. DM-42
reads 010DM42A TITLE SHEET, 010DM42B LIST OF DRAWINGS, 200DM42 GENERAL ARRANGEMENT LAYOUT,
400DM42 LANDSCAPE CROSS SECTION, 600DM42A HARDSCAPE SCHEDULES and 600DM42B SOFTSCAPE
SCHEDULES. Nothing is set, nothing is reserved and nothing runs out, because the
identifier is in the number and no two plots share one. Nothing is read off the model's
own numbers either, which are copies on every plot but one of 160. `StemOf` on
`SheetNumberRun` is the one rule turning DM-42 into DM42, and it answers empty for
anything that is not a plot identifier, so the old scheme's markers cannot creep back in
through a caller. The whole marker family, `MarkerChoices`, `PlotMarkers`,
`PlotMarkerFile`, `MarkerLedger` and the store with its plot-markers.txt, is deleted
rather than left beside this, because two records of one fact is the shape this repo
keeps paying for.

`SheetNumberRun` builds one code's numbers for one plot. Occupied slots come off the
plot's own numbers plus everything typed on the panel, a bare number holds the first
letter's place, and the next letter continues after the highest, so a second run continues
rather than collides: 600DM42A and 600DM42B in the model give 600DM42C, and a bare 200DM42
gives 200DM42B because renaming the model's own sheet is not this tool's to do. A number
under the old marker scheme, 010QE or 010001A, does not start with the new front and holds
no slot, so old sheets are never renumbered and the two schemes sit side by side on a real
model, the user's decision rather than a fault. A DM-42 number starting with DM-4's front
carries a digit where a sheet letter would sit, so it holds no slot on DM-4's run either.
The exact collision is still caught: a built or typed number the model already carries is
refused by `SheetNumbers.FaultIn` under the box and by the run's own read at Run. A sheet
whose views carry more than one code gets `SheetNumberProposal.Nothing` with the reason,
since picking either code would be a guess, and a sheet with no views gets its own words,
because there is no code to front the number.

## A sheet is named from the saved table, and upper casing is only the fallback

`SheetNameSettings` holds which sheet name goes with which view type, the user's own file
first and the shipped `sheet-names.txt` second, the same two file rule as the title blocks.
The old rule, the view name upper cased with the code removed, came from three examples
that happened to match and four of DM-11's eight sheets disprove it: OVERALL KEYPLAN,
PROJECT LOCATION KEY PLAN, HARDSCAPE SCHEDULES and SOFTSCAPE SCHEDULES all differ from
their view names. `NameFor` resolves once, on `PlannedSheet.ProposedName`, so the rows, the
letter order and the run read one record, and a name typed over a proposal saves the
pairing. A type neither file holds falls back to `SheetNaming.FromView` and the panel says
derived beside it. This is also what made unlisted names ordinary: with the table, a
sheet's name and `SheetOrder`'s words are the same words.

At Run, `RunPlan` checks every row's number against the sheet numbers read off the model
as the run is worked out, not against the panel's last snapshot. Three runs in a row asked
Revit for numbers a previous run had created, because the only check lived on the panel and
its read was older than its own write, and every refusal arrived from Revit inside the
transaction instead of on the plan.

`NewViewSetups` holds the three answers for a view type no view in the model carries: the
view family type, the view template and the level, each picked from what the model holds,
never invented and never defaulted. `Missing` names what is still unanswered, the level
only for a plan kind because a section is cut from a box, and `SectionTypesAmong` is the
one routing rule both the panel's preview and the run's handler ask, so a type cannot be
planned as a plan and written as a section. `NewViewFamilies` narrows the dropdown to the
kinds a view can be created under at all, the four plan kinds and Section.

`SheetOrder` holds the team's sheet order, nine names, and it is also the letter order:
010001A is TITLE SHEET and 010001B is LIST OF DRAWINGS because of the list. Order by the
code first, then the list within a code, then the ticked order for anything the list does
not hold. A sheet with no views has no code and sorts ahead of the coded ones, because the
one such sheet the team makes is the title sheet and it opens both measured sets. The
position forgives case and edge spaces and nothing else, so OVERALL KEY PLAN with the space
reads as unlisted rather than being silently matched to OVERALL KEYPLAN.

`FaultIn` and `Problems` answer the same question twice over: the line under one box, and the
count in the run summary. One place decides, so the panel can never say a number is fine while
the summary counts it.

Two ways to be refused. A sheet in the model already carries it, or two of the sheets this run
would make carry it, which is the same fault a second later. Two separately described sheets
asking for one number clash as hard as two rows of one sheet do, because they go into one
model. Already in the model is said first when both are true, since the model is the one
somebody goes and looks at.

## A sheet is described, not captured, and the views divide

`SheetDefinition` holds the title block family and type, the view types that go on and how many
per sheet. No sheet name: it used to carry one typed name for every sheet it made, which put
GENERAL ARRANGEMENT LAYOUT around a location key plan. `SheetDivision.Of` divides the views, in
the order they were ticked, into as many `PlannedSheet`s as they need, so six views at two per
sheet is three sheets and no view is ever left off. The old `Placed` and `LeftOff` split and
the empty sheet a viewless definition used to make are superseded by that.

Each sheet the run makes is a `SheetToMake` row: one plot, one slice of the views, one name and
one number, each remembering whether it was generated or typed so the report can say. A sheet
holding one view is named by `SheetNaming.FromView`, the view name upper cased with no code,
because the code is already the front of the number. One holding more is typed, and
`PlannedSheet.WhyNothingIsProposed` says so next to the empty boxes. A row short of a name or a
number is refused by name, and `SheetBatch` counts what one definition really makes.

`SheetLayout.For` is the maths: a `DrawingArea` and a count of 1, 2 or 4, back comes the
centre of each viewport in reading order. It divides that area evenly, so the margin outside
equals the gap between. Y counts up from the bottom, which is Revit's convention and the
reason the first row back is the top one.

**Two sit one above the other.** Side by side is what the first sheets did and the two views
overlapped, because each was wider than half the drawing area, which a landscape drawing on
a landscape sheet always will be. Four still makes a two by two grid, because halving both
is the only way to get four cells.

**A view is measured before it is placed, and one that does not fit is carried rather than
laid over its neighbour.** `SheetFit.Of` takes the drawing area, the count per sheet and each
view's size on paper, and hands back the sheets the views really divide into: each view into
the next cell it fits, a view that fits nothing starting a fresh sheet, and one too big even
for a cell of its own placed alone so it covers nothing and named. Nothing is ever dropped.
`ViewOnPaper.NotMeasured` is the honest answer for a schedule, whose size is not known until
Revit has drawn it, and it fits whatever it is put in because nothing else can be said. The
writer reads a view's size off `View.Outline`, which needs no viewport, and numbers a carried
sheet with `SheetNumberRun` over the numbers the document holds at that moment. A carried
sheet whose views do not share one code cannot be numbered without a guess, so it is not
made and its views are named in the report instead.

**It divides the drawing area and not the whole sheet.** `DrawingArea.InsideTheTitleBlock`
takes the title strip down the right hand edge off the width, because that is not somewhere
a view may sit, and the first real run centred every schedule across it. **How wide the
strip is cannot be read off a title block**: Revit gives a placed block Sheet Width and
Sheet Height and nothing else, and where the strip begins is drawn inside the family. So
`TitleStripAcross` is a fifth, the tool's own setting, measured by the team on the run of
2026-09-11, and `InWords` says whose setting it is in the report the same way `SectionDepth`
and `AnnotationCropChoice` do.

**A schedule on a sheet is placed by its top left corner and a viewport by its centre.**
Both were handed the centre this maths works out, so on that run every plan view landed
correctly and every schedule landed half its own size right and down. 010QA measured it:
207.4 by 187.4 mm asked for 420.5 by 297.0 came back centred on 522.1 by 203.3, which is
93.7 low and exactly half its own height. `CornerPlacement` holds the correction and the
writer applies it after the placement, because how big a schedule comes out is not known
until Revit has drawn it. Nothing moves a viewport: those were right, and a viewport's
bounding box takes in the view title under it, so correcting one against its box would move
a placement that is already correct.

`SheetSize` is the width and the height in feet AND which read produced them, because a size
that came from nowhere reads exactly like a size that was measured. Three sheets were made
empty on a real A1 title block. A size that is zero, negative, NaN or infinite is not a size,
and the factory turns it into `NotRead` rather than letting it reach `SheetLayout.For`.

`ViewportRecord` is one placement in plain numbers: sheet, view, the view's own scale,
centre, size and the sheet's size, said in millimetres, with the viewport type it was placed
in and the scale as the Properties panel shows it. The run records one per placement and the
scan reads the same shape off sheets the team made, so the two can be held against each
other. A sheet has no scale of its own: what a sheet shows under Scale is a readout of the
views placed on it, and each view's comes from its template, so nothing anywhere sets one.

**A scale can read Custom over a number the report prints plainly.** DM-11-(200) General
Arrangement Layout reads Custom with a Scale Value of 250, under a template named for 250,
and the report said 1:250 and nothing more. Neither is wrong: `View.Scale` is the ratio and
the API documentation is plain that a value Revit does not hold in its own list of scales is
applied as a custom one, so 250 is the number in both places and Custom is the label on a
value the list does not carry. `ScaleInWords` prints both when they differ and once when
they do not, because a report and a model that differ by a word are the same class of fault
as a report that says created and not created.

## Which title block a view type's sheets are made on

`TitleBlockSettings` merges two files, the user's own first and the shipped defaults second,
and hands back one pairing per view type with which file it came from. Eleven were picked by
hand on the run of 2026-09-11 and the same eleven would have been picked again for every
plot.

Three rules hold it up. **Only what the user set is written back**, so this version's
defaults never freeze into their file. **Views that disagree about the title block are both
named and neither wins**, the same rule the plot on a cell and the schedule kinds follow.
**A title block the settings name and the model does not hold is not an error**, because the
settings are shared across projects, and `WhyUnset` says so.

`TitleBlockSettingsFile` is the format: tab separated, four fields, only the code trimmed. A
type in this model is named `LOD /  HARDSCAPE SCHEDULES`, with two spaces, so a format that
tidied its fields would produce a name the model does not hold. A line that is not four
fields is kept with its number rather than dropped, because a settings file one line short
reads exactly like one that never had the line. The code and the view name are separate
fields, so the `(010) Overall Plan` format lives on `ViewType` and nowhere else.

## A preset is steps 2 and 4, and never step 1

`Preset` holds a name, the view types ticked in step 2, and the sheet definitions described in
step 4, each one a title block, a views per sheet and its views in the order they go on. **It
holds no plot, no sub plot and no sheet number.** Those are what changes between one run and
the next, and a preset carrying them would fill step 1 with last week's plots and put a number
on a sheet another plot already has.

`Presets` merges two files the same way the title block settings do, the user's own over the
shipped one of that name, and only what the user saved is written back. `PresetFile` is the
format: tab separated, the first field saying what the line is, `preset`, `type`, `sheet` and
`on`. Every line belongs to the `preset` line above it and every `on` line to the `sheet` line
above it, so a record with nothing above it to belong to is kept as a line that could not be
read rather than attached to the wrong preset.

`PresetFit` is what one model can honour. **The part that fits is filled in and the part that
does not is named.** A view type or a title block this model does not hold is ordinary, because
a preset is shared across projects, so refusing the whole preset over one missing schedule
would make it useless on every model but the one it was saved from. A view the model lacks
comes off the sheets it was on and is named once, under the types, rather than twice.

`PresetFilling` says which preset filled the two steps, and whether they still say what it
says. **It compares rather than remembering.** A tick, a sheet added, a sheet removed, a title
block picked and a views per sheet changed can all move them, and the flag for the sixth one is
the one nobody sets. `Preset.SameAnswer` is that comparison, and the name and the file a preset
came from are no part of it.

The shipped `install/presets.txt` is the six sheets of the DM-11 run, its view types grouped by
the title block `install/title-blocks.txt` pairs each with. Five definitions, six sheets, one
view on each. Two shipped files are two records that have to agree, so a test reads both and
fails if a preset ever names a block its own pairing does not.

## One set of positions, read by the run and by the preview

`SheetPlacement.Of` is the only place a sheet is laid out. It calls `SheetFit.Of` for the
division and `SheetLayout.For` for the centres and pairs them up, and both of those are now
called from nowhere else. The run places from it and the preview draws from it, so a card and
the sheet it draws cannot disagree.

The writer used to call the two itself, `SheetFit` in one method and `SheetLayout` in another,
which is two readings of one division inside one class. A preview that laid a sheet out its own
way would have been a third, and the three would have agreed the day they were written and
drifted the first time any one of them changed. **A drawing of a sheet that is not the sheet is
worse than no drawing.**

`RunPreview.Of` turns a plan into a card per sheet, in the order the run makes them. **What it
cannot know it marks rather than guesses.** A view the run is about to create has no size until
Revit draws it, and neither has a schedule, so both are laid out at their cell, drawn at
`NominalShareOfTheCell` and named as not measured. A title block no sheet in this model uses
has no size at all, because Sheet Width and Sheet Height are instance parameters, so its card
draws nothing and says why.

`MeasuredTitleBlock` carries the sheet its size was read off, so a size that came from nowhere
cannot read like a size that was measured. `TitleBlockSizes` keeps the first measurement of
each type, the way the run takes the first block found on a sheet. `PaperSizes` holds only what
was really measured and answers for everything else with `ViewOnPaper.NotMeasured` under the
name asked for.

`ViewNaming.Of` is the one builder of `DM-11-(010) Location Key Plan`. It was written out by
hand in four places, twice in the run plan, once in the schedule definition and once in the
writer, which is what names every created view. It belongs in Shared beside the parser that
reads a name apart, and it is not there because a change to Shared stops every other session
and is the user's call.

## A sheet with nothing left to carry is not made

`RunPlan` refuses a sheet row whose every view this same run refused, under
`RunRefusalKind.SheetHasNoViewLeft`, and names the views it was waiting for. **A sheet with no
views BY DESIGN is untouched**, so the cover page is still made. A sheet keeping one of its two
views is still made, and the writer names the one that did not go on.

The run of 2026-09-13 08:51 is why. Five views were refused before the run, each "No scope box
is named DM-02", and their five sheets were created anyway, each reported as made without the
view it was waiting for. Five empty sheets, and five numbers taken in a model where a number
cannot be reused.

The refused views are passed into `AddSheets` rather than worked out again there, because
deciding a second time which views this run refused is a second record of the run's own answer.

## The line under the title block picker says which one is in force

`WhereItCameFrom` takes the block showing in the picker. Without it the line reported the
remembered pairing beside a picker showing something else, so one control named two different
title blocks: on 2026-09-13 sheet 1 showed `AR-PRX-Title_Block_A1 Cover Page   COVER PAGE` in
the dropdown and `AR-PRX-Title_Block_A1 LOD / SCHEDULES, from the shipped defaults` under it.

A choice that agrees with the remembered one reads as it always did, or every line would carry
the same name twice.

**A view type in neither file says so plainly.** It used to read "is in neither file and takes
the same one", which claims a block was chosen for it when nothing was. It names the type, says
it takes this sheet's block and that nothing chose it, and says picking the block above
remembers it.

