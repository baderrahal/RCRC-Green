# Scan checklist

Work through every item. Do not stop early because the first few came back absent.

Report each as one line:

```
ITEM | PRESENT or ABSENT or UNKNOWN | path | one short fact
```

## The items

| # | Item | What to look for | Why it matters |
|---|---|---|---|
| 1 | Rules file | `CLAUDE.md`, and its line count | Over 200 lines costs context on every message |
| 2 | Scoped rules | `.claude/rules/`, names and their `paths` field | Rules that only load for matching files |
| 3 | Skills | `.claude/skills/`, names | Repeatable procedures, invocable by name |
| 4 | Agents | `.claude/agents/`, and for each one its model and tools | An agent with more tools than it needs is a risk |
| 5 | Commands | `.claude/commands/`, names | Saved prompts, one word instead of a paste |
| 6 | Hooks | settings files, and every event wired | The only real enforcement |
| 7 | Test gate | `.github/workflows/`, file names and triggers | Whether tests run without being asked |
| 8 | Tests | the test folder, how tests are run, how many exist | A gate with no tests reports green forever |
| 9 | Language | what the source files actually are | Decides whether phase 7 splitting applies |
| 10 | Connections | `.mcp.json` or configured servers | Live reads instead of manual exports |
| 11 | Editor tasks | `.vscode/tasks.json` | Often stale, often holds a path from one machine |
| 12 | Ignore file | `.gitignore` | Whether build output and secrets are excluded |
| 13 | Branch state | current branch, distance from the main branch | What a download of the main branch would actually contain |

## Rules for the scan

**A path or it is UNKNOWN.** PRESENT without a path is a guess wearing a finding's clothes.

**ABSENT means looked for and not found.** If the check could not run, that is UNKNOWN. The two are not interchangeable and treating them as the same is how a missing thing gets recorded as a settled fact.

**No recommendations here.** Phase 3 reports. Phases 4 to 6 propose. Mixing them means the reader cannot tell what was seen from what was inferred.

**Item 13 last, and say it plainly.** If work exists only on a branch, then anyone downloading the main branch does not have it. This catches the case where a tool appears finished but is not actually reachable by the person who needs to run it.

## After the table

One line, no more: which of the absent items matter most for this particular tool, and which genuinely do not apply. A single-file tool with no repo does not need most of this, and saying so is more useful than listing thirteen absences.
