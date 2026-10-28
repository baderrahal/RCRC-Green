# Usage

One row per session, added after its phase merges. Taken from
`npx ccusage@latest session --since <yyyymmdd>`. UNKNOWN when npx is missing or refused.

| Date | Session | Phase | Total tokens | Cost USD |
|---|---|---|---:|---:|
| 2026-10-28 | c82dc224 | F1 KPI pane redesign, with the hooks fixed for Windows | 190,201,286 | 66.48 |
| 2026-10-28 | c82dc224 | F1b Q1 to Q5 answered, and the records inside the caps | 24,229,432 | 12.40 |

The first row, so no phase before it to compare against. 97.7 % of it was cache read.

The F1b row is the session total, 214,430,718 and 78.88, less the F1 row, because both rounds ran
in one session. It was read before the final checks and the merge, so their turns are not in
it. It cost less than F1.
