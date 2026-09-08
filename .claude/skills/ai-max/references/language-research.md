# Choosing the language

## Why this is researched, not recalled

The host program decides most of the answer, and host programs change. An API that was current when the model was trained may be deprecated, replaced, or newly supported in a version the user is running. Recalling an answer here produces a tool built on something that no longer exists.

So this phase searches. Every time.

## What to establish first

Ask, or find, before searching:

1. **Which program does it run inside?** Revit, Navisworks, Civil 3D, AutoCAD, Rhino, Excel, a browser, or nothing at all
2. **Which version?** Not the newest version that exists, the one the user actually has
3. **Who runs it?** The author alone, or people who cannot install anything
4. **How often?** A one-time job and a weekly job justify very different effort

Question 3 changes the answer more than people expect. A tool that has to run on machines you do not control cannot need a compiler, an install, or a licence.

## What to search for

Search for the current state, not the concept. Useful queries:

- the program name plus API plus the version, to see what is supported now
- the program name plus the year, to catch a recent change of direction
- whether the older route is deprecated
- what the program's own documentation currently recommends

Prefer the vendor's own documentation and release notes. Community posts are useful for whether something actually works in practice, and unreliable for what is current.

Check at least two sources before settling. A single result that matches an expectation is the easiest way to confirm something wrong.

## What matters in the comparison

Weigh these, in roughly this order:

**Can it reach what the tool needs?** Some things are only exposed through one route. This is decided first, because it can rule everything else out.

**Can the person who runs it actually run it?** Something needing a compiled install fails immediately for a group with locked-down machines.

**Can it be tested where it is written?** Code that can only be tested inside the host program costs a full round trip for every attempt. That is not a reason to avoid it, but it is a reason to write it in larger, more carefully reviewed pieces.

**How much does one change cost?** A script that is edited and rerun in seconds is worth a lot compared to something that has to be compiled and reloaded.

**Will it still work next version?** A route the vendor is moving away from will need rewriting.

## How to present the decision

Give the user:

- the recommendation, in one line
- why, in one paragraph, tied to the four points above
- the two runners-up and what each one loses
- anything the choice closes off later

Never present one option as the only possibility. There is almost always a second route, and the user knows their own constraints better than any search does.

## When there is no network

Say so plainly and ask rather than guessing. Something like: the current state of that API cannot be checked from here, and an answer from memory could be a version or two out of date.

Then offer what can still be done without research:

- ask the user which route they have used before for this program
- lay out the trade-offs above so they can decide with their own knowledge
- write down the choice as provisional, to be confirmed when research is possible

A provisional choice that is labelled provisional is fine. A guess presented as a finding is not.
