# 02 questions

Every question for Bader in one list, in full here and only here. PROGRESS.md and the log name a
question by its number and a title of at most 8 words. Numbered once and never renumbered. A new
question goes after the last one. The answer goes under its question, with the item that carried
it out.

Questions asked before this file existed are in each task's own log and are not copied here.

1. Dark theme colours for the KPI pane. The design gives light colours only. The pane paints
   itself in them under both Revit themes, so on the dark theme it is a white panel in a dark
   Revit. Decides whether KpiTheme gets a second set.

   Options: A keep the light pane in both themes, it reads and it matches the design. B give
   me dark values, or say pick a dark set to match, and I add them to KpiTheme

   Answer: A, 2026-10-28. The light pane stays in both themes. No change.

2. Should Read write its own short report? Show in report on a step 2 box opens the last press's
   report, so before the first Create the link is off and reads In the report after Create.

   Options: A leave it, the box's tooltip carries the full text and Create writes the report.
   B Read writes a small read report so the link works at once

   Answer: A, 2026-10-28. The tooltip carries the full text. No change.

3. All and None in step 3 act on every plot, as Select all and Clear did, while the search box
   only filters the list. The Drawing Sheet's All and None act on what the search shows.

   Options: A keep every plot, nothing changes about what a tick press does. B act on what the
   search shows, the same as the Drawing Sheet

   Answer: A, 2026-10-28. All and None act on every plot. No change.

4. Step 2 before a read shows only its footer, Read model and Not read yet, with an empty body.

   Options: A one muted line, Reads sheets and schedules. Nothing changes. B leave it empty

   Answer: A, 2026-10-28. Added as `KpiPaneWords.ReadsSheetsAndSchedules`, shown in step 2's body
   before a read, in the round recorded at the top of `steps/log-kpi.md`.

5. The dropdowns in step 2 have square corners where the design asks for 4 px. The system
   ComboBox draws its own button part, and rounding it means rebuilding its template.

   Options: A leave them square. B rebuild the template in a round of its own

   Answer: A, 2026-10-28. The dropdowns stay square. No change.

6. A failed read leaves step 2 dead. When Read model is refused, only the status line repaints,
   and the button stays grey until somebody clicks a step circle. The breaker found it on
   28 October, it is older than this round, and this round could change no src beyond Q4.

   Options: A fix it in a short round of its own, with the pane redrawing step 2 after a refusal.
   B leave it, the status line already says why

   Answer: A, 2026-10-28. `PressEnd.RedrawsTheSteps` draws the steps again after a refused read,
   in the KPI team kit round.

7. Rules a session needs on every turn now sit in docs/rules. The split kept a few whole
   sections in .claude/rules and CLAUDE.md under their caps. The two plot parameters fact (M07),
   K51 the pane holds no copy, K56 never write to a template, R14 one sibling and R18 the
   category number are read by id now.

   Options: A add one pointer line per rule to CLAUDE.md, naming its id, which costs about 0.5 KB.
   B leave them in docs/rules, found through the index

   Answer: A, 2026-10-28. Five lines in CLAUDE.md under Rules a session needs every turn.

8. A features.txt naming no tool it knows shows all three tools. The round said unknown names
   are ignored, and a file of nothing but unknown names would then show an empty tab. The kit
   writes KPI, so only a hand edit reaches it.

   Options: A keep it, three tools shown is a fault somebody sees and reports, and the journal
   says why. B show no tool at all

   Answer: A, 2026-10-28. Three tools shown stays. No change.

9. The MOSQUES and SCHOOLS templates the team already had are byte for byte the new LESS THAN 60
   files, so DAILY MOSQUE, FRIDAY MOSQUE and SCHOOL plots get the under 60 per cent KPI targets
   today. Measured in `design/component-wiring.md`. The tool writes no target and changes nothing.

   Options: A ask the client whether the old values should use the under 60 targets, no change
   in the tool. B nothing to ask

   Answer: B, 2026-10-28. The 60 per cent values are added data beside the old values, and the
   old values keep their templates. Nothing to ask the client. No change.

10. A shared components.csv that is there and cannot be read, a fourth heading typed in D1 or a
    tab separated save, lets the shipped table answer for every value. Setup shows a box saying
    the shared file was not read, and both reports name it.

    Options: A keep it, the shipped table answers and Setup says so. B refuse every value while
    the shared file cannot be read

    Answer: A, 2026-10-28. The shipped table answers and Setup says so. No change.

11. Use on a new template found by its layout lasts until Revit closes. A components.csv row
    naming that template uses it on every start without a press.

    Options: A keep it, the row is the lasting choice. B remember Use in a file of its own

    Answer: A, 2026-10-28. Use lasts until Revit closes, a components.csv row is the lasting
    choice. No change.

12. `ComponentFiles` in the KPI Revit folder holds the folder name RCRC Green a third time.
    Drawing Sheet's `PresetStore` and `TitleBlockSettingsStore` hold the other two, and
    territory.md asks for a move to Shared rather than a copy.

    Options: A move it to Shared in a round of its own. B leave the three copies

    Answer: A, 2026-10-28. Moved to Shared in a round of its own later, F3 in
    `steps/01_next.md`. Not in the Setup.exe round.

13. The Setup.exe and the add-in are unsigned, so Windows SmartScreen shows Windows protected your
    PC on a downloaded Setup.exe, Revit asks about RCRC Green once, and a PC whose policy blocks
    unsigned programs cannot run the Setup at all, which is why the zip stays. A code signing
    certificate from Parsons IT would sign both the Setup.exe and the DLLs in build-team-kit.ps1.

    Options: A ask Parsons IT for a code signing certificate, or for their signing service, and
    sign in a round of its own once it is in hand. B stay unsigned, with More info, Run anyway
    and Always Load in INSTALL-TEAM.txt

    Answer:

14. A SCHOOLS plot with no component, on no sheet, is now refused before the press. Its rule files
    each value under a folder named as the value, so with no value there is no folder, the same
    as MOSQUES since the start. While SCHOOL was the only school row it filed under SCHOOL. No
    plot measured so far has an SC prefix and no sheet.

    Options: A keep refusing it, the plot named before the press. B file it under SCHOOL

    Answer:

15. Folder `value` is the value with its spacing put right, LESSTHAN as LESS THAN, upper case,
    one space between words. For every value seen so far that is the value exactly. A model that
    types one value two ways still files all its plots in one folder.

    Options: A keep it so. B the value exactly as each sheet spells it, two folders for two
    spellings

    Answer:

16. A mosque or school value written with a sign or another word, DAILY MOSQUE >60% or ABOVE 60%,
    holds no MORE and no 60 as whole words, so the MOSQUE rule places it in MOSQUES, the under 60
    targets, with no box. The only measured spelling is MORE THAN 60%. NG05's is UNKNOWN until
    step 11 of your pass shows it, L85.

    Options: A wait for step 11, and add a rule line for any such spelling it shows. B add rules
    now for >60, <60, ABOVE and BELOW

    Answer:
