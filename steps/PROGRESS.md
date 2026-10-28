STATE WAITING for a fresh session, 2026-10-28

## Now
- The four 60 per cent templates and values are in, each to its own template and folder
- install/components.csv is the one component table, with a shared copy in the templates folder that wins
- A new template laid out like a known one shows in Setup, and an unknown value shows in step 2
- Merged in two pull requests, 175 and the one after it. Nothing has run in Revit

## Next
1. Bader runs steps/03_your_steps.md on NG05 with his full install
2. Bader answers Q8 to Q12
3. The Setup.exe round, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md
- Q8 Features file naming no tool
- Q9 Old mosque and school targets
- Q10 Unreadable shared components.csv
- Q11 Use on a new template lasts the session
- Q12 RCRC Green folder name copied three times

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in is unsigned, so Revit asks once, Always Load
- The kit pass of the round before has not run

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Component table: install/components.csv, map in design/component-wiring.md
- Kit: install/build-team-kit.ps1 and install/team/
