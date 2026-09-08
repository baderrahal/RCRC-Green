# The ten phases

## Contents

1. Decide
2. Pick the language
3. Scan
4. Rules file
5. Hooks
6. Test gate
7. Split
8. Check
9. Ship
10. Package

---

## Phase 1. Decide

Two questions, and neither can be skipped. Most failed tools failed here, not in the code.

**What does it do?** One sentence. If it takes a paragraph, it is more than one tool.

**What counts as done?** This is the one people skip. Push for something observable. "It works" is not an answer. "It reads a LandXML file and creates toposolids that match the surface within 10mm" is an answer.

Also worth getting, briefly:

- Who runs it, the author only or other people
- How often, once or every week
- What it reads and what it produces

Do not move on without the two answers. If the user resists, offer three candidate definitions of done and let them pick one. That is usually faster than asking again.

**Output of this phase:** two sentences, written down, that later phases can be checked against.

---

## Phase 2. Pick the language

Read `language-research.md` and follow it. The short version is that the host program decides most of it, and the current state of that program's API has to be checked rather than recalled.

Do not skip this because the answer seems obvious. APIs change, and a language that worked two versions ago may be deprecated now.

**Output:** the language, one paragraph of reasoning, and the two runners-up with why they lost.

---

## Phase 3. Scan

Read `harness-checklist.md` and work through it item by item.

Report in this shape, one line per item:

```
ITEM | PRESENT or ABSENT or UNKNOWN | path | one short fact
```

Rules that make the scan worth reading:

- Show a path for anything reported PRESENT. A claim without a path is UNKNOWN
- Use UNKNOWN when a check could not run, never ABSENT. Absent means looked for and not there
- Do not recommend anything in this phase. Reporting and recommending in the same breath is how a guess becomes a finding

If the repo does not exist yet, say so and skip to phase 4 with everything ABSENT.

**Output:** the table, then a single line naming which items matter most for this particular tool.

---

## Phase 4. Rules file

Copy `assets/CLAUDE.md.template` and fill it in for this tool.

Keep it under 200 lines. Past that, split it into `.claude/rules/` files with a `paths` field in the frontmatter, so each rule only loads when a matching file is being touched. A long rules file costs context on every single message, which is why the limit exists.

What belongs in it:

- What the tool is, in two lines
- How to run the tests
- The conventions that are not obvious from the code
- Anything that has already gone wrong once

What does not belong:

- Anything the code already says clearly
- General good practice that applies to every project everywhere
- A copy of a rule that lives somewhere else. One rule, one place. Two copies drift, and the older one wins at the worst moment

**Output:** the file, and its line count.

---

## Phase 5. Hooks

A rule in CLAUDE.md is a request. A hook is a wall. Anything that must never happen goes here, because a request can be reasoned around and a wall cannot.

Start from `assets/hooks/`. The two included are:

- a PreToolUse check that blocks writes to paths that should never be written
- a check that runs before a commit and refuses if a required file is missing

Adapt them. Do not add hooks that are not needed, because every hook fires on every matching action and slow hooks make the whole session slow.

Worth knowing: scoped tool permissions in a subagent's frontmatter do not always hold. A subagent that is granted a narrow slice of a tool can sometimes use the whole tool. If something truly must not happen, put it in a hook, not in an agent's permission list.

**Output:** the hook files, the events they fire on, and one line on what each one prevents.

---

## Phase 6. Test gate

Copy `assets/workflow.yml` into `.github/workflows/` and adjust the test command.

This is the phase most people skip, and it is the one that catches the failure nobody sees coming: code that passed when it was written, and broke later when something else changed. Without this, tests only run when someone remembers, and the one time nobody remembers is the time it matters.

Check three things after adding it:

- the test command in the file actually matches how tests are run in this repo
- it triggers on pull requests, not only on pushes to the main branch
- there is at least one real test for it to run, otherwise it passes while testing nothing

That last one is worth being blunt about. A gate that runs zero tests reports green forever.

**Output:** the file path, the trigger, and the number of tests it will run.

---

## Phase 7. Split

More sessions on one repo is a real gain, but only for work that can be verified where it is written.

**Code that can be tested here** can go in parallel sessions. Each session gets its own worktree and its own branch:

```
claude -w partA
claude -w partB
```

Two to four is the useful range. The reason it caps is that collisions grow by pairs, not by workers. Two sessions have one pair that can collide, four have six, five have ten.

**Code that only the user can test**, such as an add-in that has to run inside Revit or Civil 3D, does not go in parallel. It cannot be checked here, so more sessions means more unverified code, not more finished code. Write it in one session, in fewer and larger reviewed batches, and expect each round trip to cost the user real time.

Before splitting, settle who owns the shared files. Any file every branch touches, such as a status file or a handoff note, will conflict on every merge unless exactly one session owns it.

**Output:** which parts split, which do not, and who owns the shared files.

---

## Phase 8. Check

Add agents that read the work and try to break it. Two starters are in `assets/agents/`:

- **breaker** looks for the case the code does not handle
- **claim-checker** reads a report or a summary and flags every claim that is not backed by something observable

They report. They do not decide and they do not edit. An agent that can both find a problem and fix it quietly is an agent whose findings nobody ever sees.

Give each one only the tools it needs. An agent that only reads does not need write access.

**Output:** what each agent found, and what was done about it.

---

## Phase 9. Ship

Open the pull request. Before opening it:

- the working tree is clean
- the full test run happened after the last file was written, not before
- the count in the description comes from that run

Then stop. Do not merge. The decision to land something belongs to the person who has to live with it.

If the tests are red, do not open the pull request at all. Report what failed.

**Output:** the pull request link, the test count and when the run happened, and a plain statement that it is waiting on a click.

---

## Phase 10. Package

Only run this when the tool is going to other people. Wrap the skills, hooks, agents and commands into a plugin so it installs in one step instead of being copied by hand into every repo.

What to check before packaging:

- nothing in it points at a path that only exists on one machine
- nothing in it holds a credential or a token
- the install instructions were followed once from scratch by someone who did not build it

That last check is the one that finds the missing step.

**Output:** the package, and the install instructions written for someone who has never seen the tool.
