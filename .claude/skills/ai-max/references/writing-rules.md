# Writing rules

These apply to everything: code, comments, commit messages, pull request text, README files, documentation, text inside the tool itself, and replies to the user.

## Why

Work that reads as machine written gets dismissed before it gets judged on whether it is any good. The point is not to hide how the work was done. The point is that the writing should not be the thing people notice.

If someone asks directly whether AI was involved, the honest answer is yes. Stripping the tells is about quality of writing, not about denying anything.

## The loudest tells, in order

### 1. Automatic footers on commits

The single most obvious marker. Remove any generated-by line and any co-authored-by line naming an AI from commit messages and pull request descriptions. Nothing else on this list is as immediately visible as this.

### 2. Emoji

None. Not in commit messages, not in README headers, not in code comments, not in output the user sees. A README with an emoji on every heading reads as machine written before a single word is read.

### 3. Comments that restate the code

A comment should say why, not what. If the line under it already says the same thing, delete the comment.

Bad:
```
# increment the counter
counter += 1
```

Fine:
```
# Revit returns duplicates for hosted elements, so count unique ids only
counter += 1
```

### 4. A docstring on everything

Long explanations attached to two-line functions that need none. Document what is genuinely unclear. Leave the obvious alone.

### 5. Certain words

Avoid: comprehensive, robust, seamless, leverage, crucial, delve, ensure, utilize, facilitate, streamline, holistic, cutting-edge, best-in-class, elevate, unlock, empower, journey, landscape.

Most have a plain replacement. Use, make sure, help, and improve do the same work without the signal.

### 6. Uniform structure

Every function the same length. Every section the same three bullets. Every commit message in exactly the same shape. Real writing varies because real problems vary.

### 7. Punctuation

No em dashes. No semicolons. Use a full stop, a comma, or two sentences. This is the weakest tell on the list, but the user has asked for it directly and it costs nothing.

## Also avoid

**Padding openers.** Cutting straight to the point is stronger than warming up to it.

**Closing summaries that repeat what was just said.** If it was said once clearly, saying it again adds nothing.

**Hedging on everything.** Some things are certain. Say them plainly. Save the hedge for what is actually uncertain, where it carries real information.

**Try and catch wrapped round everything** with a generic message. Handle the failures that can actually happen, in a way that says what went wrong.

**Generic names.** `data`, `result`, `temp`, `process()`, `handleData()`. Name the thing for what it holds in this specific tool.

## Terms

Plain words instead of technical shorthand where a plain word exists.

Field terms are welcome and wanted where they are the accurate word. Federated model, clash, workset, CDE, transmittal, LOD, ISO 19650 all mean something precise, and replacing them with something vaguer makes the writing worse.

## Quick check before sending anything

- Any generated-by or co-authored-by line
- Any emoji
- Any comment restating the line below it
- Any word from the list
- Any em dash or semicolon
- Does every paragraph carry something the one before it did not
