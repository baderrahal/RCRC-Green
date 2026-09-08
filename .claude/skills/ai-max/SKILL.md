---
name: ai-max
description: Runs the full workflow for building a tool, from deciding what it does through to packaging it for a team. Covers picking the right language for the host program, scanning a repo for what is missing, writing the rules file, adding hooks and a test gate, splitting work across sessions, adding checker agents, shipping, and packaging. Use this whenever the user talks about building, creating, making, starting, or planning a tool, an add-in, a plugin, a script, an automation, or a utility, including for Revit, Navisworks, Civil 3D, AutoCAD, Rhino, Excel, or the web. Trigger on "new tool", "create a tool", "build an add-in", "I want to automate", "start a repo for", "make a script that", "what language should this be", or any repo setup question. Trigger even when the user does not mention this skill by name, and even when they only describe the problem they want solved rather than asking for a tool.
---

# AI MAX

A tool goes wrong in the same places every time. It gets built in the wrong language, in a repo with no rules, with no automatic check, by three sessions fighting each other, and then it cannot be given to anyone else. This runs the order that avoids all of that.

## Before anything: work out which phase you are in

Do not restart a tool that is halfway built. Ask what exists, or look, then enter at the right phase.

| If | Start at |
|---|---|
| Only an idea exists | Phase 1 |
| The idea is clear but no code | Phase 2 |
| A repo exists already | Phase 3 |
| Repo is set up, building has started | Phase 7 |
| The tool works and is being given out | Phase 10 |

## The ten phases

Full detail for each one is in `references/phases.md`. Read it before running any phase. Summary:

1. **Decide.** What the tool does, and what counts as done. Nothing moves until both are answered.
2. **Pick the language.** Research the host program's current API, then propose with reasons. See `references/language-research.md`.
3. **Scan.** Report what the repo already has, item by item. See `references/harness-checklist.md`.
4. **Rules file.** Write CLAUDE.md from `assets/CLAUDE.md.template`, kept under 200 lines.
5. **Hooks.** Add the things that must never happen. Starters in `assets/hooks/`.
6. **Test gate.** Add `assets/workflow.yml` so tests run on every pull request.
7. **Split.** Extra sessions only for code that can be tested here. Code only the user can test stays in one session.
8. **Check.** Add checker agents from `assets/agents/`. They report, they do not decide.
9. **Ship.** Open the pull request. Never merge. The user clicks.
10. **Package.** Wrap it as a plugin so a team installs it once.

## Rules that hold in every phase

**Never report a check that did not run.** A test count, a passing result, a file that exists: if it was not actually observed this session, say so instead. A wrong count that looks confident costs more than an honest gap.

**Never open a pull request unasked, and never merge.** The user is the one who decides what lands. Report the state and wait.

**Write nothing that reads as machine written.** This matters to the user because a tool that looks AI generated gets dismissed before it gets used. Full list in `references/writing-rules.md`. Read it before writing any file, any commit message, or any reply. The short version: no generated-by footer, no co-authored-by line, no emoji, no em dash, no semicolon, no comment that restates the line under it.

**Every pasteable thing gets its own labelled code block.** The user moves text between tools constantly. A code block has a copy button and prose does not.

**End each phase with a short state block.** What was done, what exists only locally, what is waiting on the user. Four lines is enough.

## When something is unknown

Say UNKNOWN. Do not fill a gap with a plausible guess, and do not soften a gap into a claim. If a file cannot be found, the answer is that it was not found, not that it is absent. If the network is unavailable and a phase needs research, say so and ask rather than working from memory.

## Reference files

- `references/phases.md` holds what each phase actually does, with the questions to ask
- `references/language-research.md` holds how to choose the language, and what to search for
- `references/writing-rules.md` holds everything that must not appear in output
- `references/harness-checklist.md` holds the scan list for phase 3

## Assets

- `assets/CLAUDE.md.template` is the starter rules file
- `assets/hooks/` holds two working hook scripts to adapt
- `assets/workflow.yml` is the test gate that runs on every pull request
- `assets/agents/` holds the breaker and claim-checker definitions
- `assets/gitignore.template` is the starter ignore file
