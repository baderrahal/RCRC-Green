# 01 next

The plan. One section per item, F and a number, worked in the order below. One session takes the
first open item, or the group the order line names, and stops when it merges. A finished item gets
one DONE line under its own section, with the date, the pull request and what proved it.

Order: F1, F2, F2b, F3

## F1 KPI pane redesign for company wide use

What: rebuild the KPI pane's look and wording to Bader's approved design, four steps and a result
panel, no pane sentence over 10 words, the long texts moved to the report and to tooltips.
Why: the pane reads like a log and is going to the whole production team.
Proved by: the suite and the hook tests in the gate, and `steps/03_your_steps.md` in Revit 2024.

DONE 2026-10-28, pull request 170, merged as `8a2382f`. The gate read 2368 tests passed and 28
hook cases. The Revit pass in `steps/03_your_steps.md` is still to run.

## F1b Q1 to Q5 answered, and the records inside the version 4 caps

What: Bader's answers in, Q4's line in step 2, the rules, CLAUDE.md and the three steps files
over their caps split byte for byte, and the caps held by a test in the gate.
Why: a round asked for between F1 and F2, because every session paid for 364 KB of rules at start.
Proved by: `split_proof.txt` and `RecordCapsTests`, failing on main's records and passing here.

DONE 2026-10-28, this round's pull request, the entry at the top of `steps/log-kpi.md`.

## F2 The team kit

What: the install kit for the production team, with the build stamp in the pane header.
Why: the next round named in the F1 prompt.
Proved by: the kit installed, installed again, refused and put back in throwaway profiles, and
Bader's pass in `steps/03_your_steps.md`.

DONE 2026-10-28, pull requests 173 for the Shared tool list and the KPI pull request after it,
the entry at the top of `steps/log-kpi.md`. The Revit pass is still to run.

## F2b The KPI Setup.exe

What: one file Setup.exe for the KPI team kit, built beside the zip by `build-team-kit.ps1`,
per user and with no admin rights, and INSTALL-TEAM.txt leading with it.
Why: the team wants one file, double click, Next, Install, Finish, with no zip to extract.
Proved by: the Setup.exe installed, upgraded, refused, uninstalled and run over a full install
in throwaway profiles, and Bader's pass in `steps/03_your_steps.md`.

DONE 2026-10-28, this round's pull request, the entry at the top of `steps/log-kpi.md`. 59 checks
in `steps/runs/2026-10-28-setup-exe/proof.txt`. The Revit pass is still to run.

## F3 The RCRC Green folder name in Shared

What: the folder name RCRC Green, held by `ComponentFiles`, `PresetStore` and
`TitleBlockSettingsStore`, moved to one place in Shared, Q12.
Why: three copies of one fact, and territory.md asks for a move rather than a copy.
Proved by: the suite, and the three stores reading the same folder as before.

## F4 Region areas and folder layout

What: (a) a plot whose regions hold one area is taken without asking, and where areas differ
each option shows its area with Show. (b) each component value in its own folder, flat or
grouped under its main, picked in Setup.
Why: Bader met Two regions hold an area. Pick one. on equal areas, and the team files each
value apart.
Proved by: the suite and the hook tests in the gate, and the Revit pass part (b) writes into
`steps/03_your_steps.md`.

DONE (a) 2026-10-29, the region pull request, the entry at the top of `steps/log-kpi.md`.
