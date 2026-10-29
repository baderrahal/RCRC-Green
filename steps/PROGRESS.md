STATE WAITING for a fresh session, 2026-10-29

## Now
- Regions holding one area are taken without asking, K77, pull request 179
- Where areas differ each option shows its area, with Show to select and zoom
- Each PRX_Component value files in its own folder, Flat or Grouped, K78
- Setup offers Flat and Grouped with neither ticked, and Create waits for a pick
- The kit is rebuilt from main right after this merge, the last step of this session
- Nothing has run in Revit since the pane redesign

## Next
1. Bader runs steps/03_your_steps.md: the Setup.exe, NG05, then his full install back
2. Bader answers Q13 and Q16 to Q19
3. F3, the RCRC Green folder name in Shared, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md, with the Setup.exe from main
- Q13 Code signing through Parsons IT
- Q16 Values written with > or ABOVE
- Q17 Show refusing on host view link plans
- Q18 A components.csv row files one level in Grouped
- Q19 A street UID2 shared by two values

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in and the Setup.exe are unsigned, so SmartScreen asks and Revit asks once
- While the Setup is installed, install.ps1 and uninstall.ps1 refuse until it is uninstalled in Apps
- L85: a value typed DAILY MOSQUE >60% goes to MOSQUES with no box, Q16
- L117: files of earlier presses stay in the old folders and nothing names them

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Component rows: install/components.csv. Rules with Main: install/component-rules.csv. Map in
  design/component-wiring.md
- Kit: install/build-team-kit.ps1, install/team/ and its Setup script, the output in
  Documents\RCRC-Green-Team-Kit. Proof: steps/runs/2026-10-28-setup-exe/
