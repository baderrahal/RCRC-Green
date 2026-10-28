STATE WAITING for a fresh session, 2026-10-28

## Now
- Q1 to Q5 answered. Q4's line is in step 2 before a read. The rest changed nothing
- The records are inside the version 4 caps, split byte for byte, the proof in
  steps/runs/2026-10-28-records-split/split_proof.txt
- RecordCapsTests holds the caps in the gate
- Nothing in the KPI pane has been seen in Revit, the Q4 line included

## Next
1. Bader runs steps/03_your_steps.md in Revit 2024 and sends the screenshots
2. Answers to Q6 and Q7
3. F2, the team kit, in a fresh session started with steps/START.md

## Waiting for Bader
- The Revit pass in steps/03_your_steps.md
- Q6 A failed read leaves step 2 dead
- Q7 Every turn rules now in docs

## Blockers and known bugs
- A failed read leaves Read model grey until a step is clicked, Q6
- python3 on this machine is a shim in C:\Users\bader\bin, outside the repo

## Where things are
- Rules: .claude/rules for every turn, the rest by id through docs/rules/index.md
- Logs: steps/log-<task>.md, newest at the top, older entries in steps/history/
- Plan: steps/01_next.md. Questions and answers: steps/02_questions.md
- Steps for the machine: steps/03_your_steps.md. Latent findings: steps/latent.md
