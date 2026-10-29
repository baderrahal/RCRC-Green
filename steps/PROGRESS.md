STATE WAITING for a fresh session, 2026-10-29

## Now
- PRX_Component values are placed by Bader's word rules, install/component-rules.csv, K76
- An exact components.csv row beats every rule, and the shipped components.csv holds none
- Step 2 shows the values placed by rule, and the unknown value box stays for the rest
- The Setup.exe and the zip ship component-rules.csv, rebuilt from main after the merge
- Nothing has run in Revit since the pane redesign

## Next
1. Bader runs steps/03_your_steps.md: the Setup.exe, NG05, then his full install back
2. Bader answers Q13 to Q16
3. F3, the RCRC Green folder name in Shared, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md, with the Setup.exe from main
- Q13 Code signing through Parsons IT
- Q14 A SCHOOLS plot with no component is refused
- Q15 The value folder is the value with its spacing put right
- Q16 Mosque or school values written with > or ABOVE

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in and the Setup.exe are unsigned, so SmartScreen asks and Revit asks once
- While the Setup is installed, install.ps1 and uninstall.ps1 refuse until it is uninstalled in Apps
- L85: a value typed DAILY MOSQUE >60% goes to MOSQUES with no box, Q16

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Component rows: install/components.csv. Rules: install/component-rules.csv. Map in
  design/component-wiring.md
- Kit: install/build-team-kit.ps1, install/team/ and its Setup script, the output in
  Documents\RCRC-Green-Team-Kit. Proof: steps/runs/2026-10-28-setup-exe/
