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

   Answer:

7. Rules a session needs on every turn now sit in docs/rules. The split kept a few whole
   sections in .claude/rules and CLAUDE.md under their caps. The two plot parameters fact (M07),
   K51 the pane holds no copy, K56 never write to a template, R14 one sibling and R18 the
   category number are read by id now.

   Options: A add one pointer line per rule to CLAUDE.md, naming its id, which costs about 0.5 KB.
   B leave them in docs/rules, found through the index

   Answer:
