# Usage

One row per session, added after its phase merges. Taken from
`npx ccusage@latest session --since <yyyymmdd>`. UNKNOWN when npx is missing or refused.

| Date | Session | Phase | Total tokens | Cost USD |
|---|---|---|---:|---:|
| 2026-10-28 | c82dc224 | F1 KPI pane redesign, with the hooks fixed for Windows | 190,201,286 | 66.48 |
| 2026-10-28 | c82dc224 | F1b Q1 to Q5 answered, and the records inside the caps | 24,229,432 | 12.40 |
| 2026-10-28 | c82dc224 | F2 Q6, Q7 and the KPI team kit | 40,444,652 | 16.10 |
| 2026-10-28 | 493e4ebb | 60 per cent templates, components.csv, new values and templates, two pull requests | 89,845,589 | 35.01 |
| 2026-10-28 | 204d63e4 | F2b the KPI Setup.exe, and Q8 to Q12 answered | 22,122,165 | 11.30 |
| 2026-10-29 | f0ee7c3b | PRX_Component values placed by word rules | 34,612,438 | 15.15 |
| 2026-10-29 | 32273d1a | Region areas and folder layout, two pull requests | 85,097,374 | 30.84 |

The first row, so no phase before it to compare against. 97.7 % of it was cache read.

The F1b row is the session total, 214,430,718 and 78.88, less the F1 row, because both rounds ran
in one session. It was read before the final checks and the merge, so their turns are not in
it. It cost less than F1.

The F2 row is the session total, 254,875,370 and 94.98, less the reading under F1b, so it
also holds F1b's merge turns. It was read before the final checks and the merge. It cost more
than F1b and less than twice it.

The 60 per cent row is this session alone, read before pull request (b)'s final gate and merge, so
those turns are not in it. It is more than twice F2, named in its log entry: two pull requests,
two review passes on each, and the pane prompts.

The Setup.exe row is this session alone, read before the final gate and the merge, so those turns
are not in it. It cost less than the 60 per cent round.

The component rules row is this session alone, read before the final gate and the merge, so those
turns and the kit rebuild are not in it. It cost more than the Setup.exe round and less than twice it.

The region areas and folder layout row is this session alone, read before the final gate and the
merge of the second pull request, so those turns and the kit rebuild are not in it. It cost about
twice the component rules round, and ran two pull requests where that ran one.
