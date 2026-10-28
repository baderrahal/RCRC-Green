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

## A near miss whose values are all Yes or No is a switch

`KPI COMPONENT S/H` holds No on 9 of 9 title block types. Its name holds COMPONENT, so it was
offered in section 9 beside `PRX_Component` as another name the model might carry the component
under. It is a show and hide toggle and it answers nothing.

`KpiNames.EveryValueIsYesOrNo` decides it and both callers ask that one method. Section 9 leaves
such a name out of the answer and section 3 keeps it, under its own tally and its own values and
again by name in the component values block, which says why it is not counted there. **A name
left out with nothing written down reads exactly like a name nobody found.**

Two questions carry that near miss list and neither is question 8. It is questions 1 and 2,
which are the two about where the component lives.

## Three places a sheet value can live

The title block instance, the title block type and the sheet itself are all read, with a full
parameter tally and up to twenty examples per wanted name each. Sheet Width turned out to be an
instance parameter that a type cannot be asked for, and a parameter that is not on the element
asked reads as a value of zero. Asking all three is cheaper than guessing once.

## A few plots of each schedule name are read in full

The real model holds six schedules per plot over 160 plots. Every schedule gets its name,
category, fields and filters. Rows as printed, the elements listed and the areas off them are
read for the first few copies per name the workbook draws from, in name order, skipping any
that list nothing, and the report says which were read. Regenerating a thousand schedules is a
read nobody waits for.

**How many is `KpiReport.PlotsReadInFull`, and it is three.** Core holds the number and the
Revit reader reads it from there, because the report states the rule in a sentence and a second
copy of the number is the fault this repo keeps meeting. One plot cannot show whether the group
headings repeat across plots or whether an Existing group ever appears, which is why it is not
one.

## Open, and not to be guessed at in code

The first real scan raised five. The 1355 run settled four of them, and the answers are facts
about the project rather than about the tool, so they are in `CLAUDE.md` and are not repeated
here. What each one turned out to be:

1. Is PRX_Component the component the workbook wants. **Yes**, and it is the asset type
2. Which of the four plot parameters is the workbook's Ref. **PRX_Plot_ID**
3. Which filled region type is the plot's intervention area. **The one the client's note
   names**, settled by the 14:29 run, 98 plots off it and none off anything else. One region
   holding an area still answers itself whatever it is called, so the type decides only a tie
4. Are the two group headings in SHRUBS & LAWN the same on every plot. **STILL OPEN**
5. Does an Existing group ever appear in SOFTSCAPE SCHEDULE. **Yes**, DM-12 has one

Two the 1355 run raised in their place are in `steps/log-kpi.md`, both about matching a species by
name, and nothing in the code picks an answer to either.

The 1521 run raised a third, which template each value of PRX_Component means. **The 1548 run
settled it.** Eleven values came off the report's own component block and the team turned them
into the table above. It is not repeated here.

