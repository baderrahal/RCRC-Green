STATE WAITING for a fresh session, 2026-11-08

## Now
- STREETS H8 is the Revit area or the formula =F8*D8, picked in Setup, K79
- Intervention area is the default and no workbook changes for anyone who does not pick
- The Roads PDF has no Area box, Q23
- Nothing has run in Revit since the pane redesign

## Next
1. This session rebuilds the kit from main after this merge, then stops
2. Bader runs steps/03_your_steps.md: the Setup.exe, then ROW x Length and Intervention area
3. Bader answers Q23, then Q20 to Q22, Q13 and Q16 to Q19
4. F3, the RCRC Green folder name in Shared, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md, with the Setup.exe from main
- Q13 Code signing through Parsons IT
- Q16 Values written with > or ABOVE
- Q17 Show refusing on host view link plans
- Q18 A components.csv row files one level in Grouped
- Q19 A street UID2 shared by two values
- Q20 A slash with no space joins two words
- Q21 The Roads form has no Project Type box
- Q22 The walkway's total length
- Q23 The Roads form has no Area box

## Blockers and known bugs
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo
- The add-in and the Setup.exe are unsigned, so SmartScreen asks and Revit asks once
- While the Setup is installed, install.ps1 and uninstall.ps1 refuse until it is uninstalled in Apps
- L85: a value typed DAILY MOSQUE >60% goes to MOSQUES with no box, Q16
- L117: files of earlier presses stay in the old folders and nothing names them
- A walkway's total length stays empty, Q22, so under ROW x Length its H8 shows 0

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
- Component rows: install/components.csv. Rules with Main: install/component-rules.csv. Map in
  design/component-wiring.md
- PDF form per template: PdfForms.ByTemplate in Core, K35.01
- STREETS H8 option: StreetsH8 in Core, K79
- Kit: install/build-team-kit.ps1, install/team/ and its Setup script, the output in
  Documents\RCRC-Green-Team-Kit. Proof: steps/runs/2026-10-28-setup-exe/
