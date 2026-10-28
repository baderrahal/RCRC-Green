STATE WAITING for a fresh session, 2026-10-28

## Now
- Q6 fixed and Q7 in. features.txt decides which tools the ribbon builds, merged alone in 173
- The KPI pane header and its reports carry the build stamp
- The KPI team kit builds to C:\Users\bader\Documents\RCRC-Green-Team-Kit, never committed
- Nothing has run in Revit: the KPI only ribbon, the stamp and Q6 are unseen

## Next
1. Bader runs steps/03_your_steps.md: the kit, the pane, then his full install back
2. Bader puts the zip on the project Teams or SharePoint folder
3. Answer Q8, then the next plan item in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md
- Q8 Features file naming no tool

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in is unsigned, so Revit asks once, Always Load

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Kit: install/build-team-kit.ps1 and install/team/
