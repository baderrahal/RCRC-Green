STATE WAITING for a fresh session, 2026-10-28

## Now
- The KPI Setup.exe is built beside the team zip by install/build-team-kit.ps1, with Inno Setup
- Proved in throwaway profiles: install, upgrade, refused, uninstall, over a full install
- Q8 to Q12 answered, no change for any. Q12 is F3 in steps/01_next.md
- Nothing has run in Revit since the pane redesign

## Next
1. Bader runs steps/03_your_steps.md: the Setup.exe, NG05, then his full install back
2. Bader answers Q13
3. F3, the RCRC Green folder name in Shared, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md, with the Setup.exe from main
- Q13 Code signing through Parsons IT

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in and the Setup.exe are unsigned, so SmartScreen asks and Revit asks once
- While the Setup is installed, install.ps1 and uninstall.ps1 refuse until it is uninstalled in Apps

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Component table: install/components.csv, map in design/component-wiring.md
- Kit: install/build-team-kit.ps1, install/team/ and its Setup script, the output in
  Documents\RCRC-Green-Team-Kit. Proof: steps/runs/2026-10-28-setup-exe/
