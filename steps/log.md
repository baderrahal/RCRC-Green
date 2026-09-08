# Log

Newest entry first.

---

## 2026-09-08, third pass. The fourteen reviewed items fixed, merged into main

A fix round on the scaffold, not new work. Fourteen items from the reviewed phase 8 findings
and nothing else. The other findings stay written down below as reported and untouched.

Branch `claude/rcrc-green-setup-wf9ham`, restarted from main because its first pull request
had already merged. Pull request [#2](https://github.com/baderrahal/RCRC-Green/pull/2), one
commit, 26 files, merged into main as `57dd1ee`.

### Three answers, now project facts

They are written into the project facts in `CLAUDE.md`, and questions 1, 2 and 4 in the
open list below are marked ANSWERED with the answer beside them. Question 8 is marked PARTLY
ANSWERED. None of the twelve was deleted.

- A plot identifier is always two uppercase letters. Lowercase is invalid, not a second plot
- A cross section looks 10 metres, a starting value the team will change after testing
- View names repeat word for word across plots, and nothing plot specific appears in one

### What was fixed

| Item | Findings | What changed |
|---|---|---|
| 1 | 37, 42 | `install/install.ps1` and `install/uninstall.ps1` build the per user layout the manifest asks for, which the build does not produce. Both documented in `CLAUDE.md` |
| 2 | 43 | `LangVersion` on Core pinned to 12.0, the same version the test project resolves to |
| 3 | 2, 3, 4 | Surrounding whitespace, tabs, carriage returns and newlines come off identifiers and view names before anything is compared or keyed on |
| 4 | 5 | Plot identifiers are uppercase only. A lowercase one reaches the ignored list carrying `IgnoredReason.WrongCase`, which is why that list holds entries rather than bare strings |
| 5 | 7 | `NaturalOrder` sorts the number part as a number, so DM-2 comes before DM-100 and code 200 before code 1000 |
| 6 | 12 | `SectionPlacement.Length` scales by the largest delta instead of squaring all three |
| 7 | 9 | Already done in `7ae6759` last round, with tests. Checked rather than redone |
| 8 | 13 | Depth is a required argument, in the caller's unit, handed back untouched. Core derives nothing from the box |
| 9 | new | `SectionDefaults` in the Revit project holds the 10 metres and converts it to feet |
| 10 | 18 | `writing-check.sh` reads the commit message as well as the files |
| 11 | 14, 22, 34 | Both commit hooks read the command through `commit-scope.py` and work out what the commit will really carry |
| 12 | 24 | A hook script that cannot be found blocks instead of exiting 127 and being read as a non blocking error |
| 13 | 38, 39, 40, 41 | Four tests rewritten against values written out by hand |
| 14 | none | The three answers recorded, four questions marked |

### The four tests, broken then fixed

Each was proved by editing the code, running the suite, reading the result, and putting the
code back. The suite came back to 79 green afterwards with only the intended changes in the
tree.

- finding 38, `APlotMissingATypeEveryOtherPlotHasIsReportedMissing`. Broke `MissingFor` to
  return every column. RED as intended, 3 failed and 76 passed.
- finding 39, `APlotWithNoViewsGetsARowAndEveryColumnReadsMissing`. Deleted the line that
  adds a column, so the grid has none. RED as intended, 8 failed and 71 passed.
- finding 40, `EveryPlotWithAScopeBoxGetsASectionAcrossTheMiddleOfIt`. Put the old rule back
  so depth came from half the box extent instead of the argument. RED as intended, 6 failed
  and 73 passed.
- finding 41, the same test. Flipped the comparison that picks the short side. RED as
  intended, 7 failed and 72 passed.

Before the rewrite each of those four stayed green under the same break.

### Checks that actually ran

Local, on main at `57dd1ee`, clean working tree, 2026-09-08 10:24:22 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 79, Skipped: 0, Total: 79
```

On the runner, run
[34215178742](https://github.com/baderrahal/RCRC-Green/actions/runs/34215178742) against
commit `2cc4df5`, the tree that merged:

```
Passed!  Failed: 0, Passed: 79, Skipped: 0, Total: 79
79 tests ran.
```

Both numbers are 79. The suite was 48 before this round.

Also run: `dotnet build RcrcGreen.sln -c Release` succeeded with 0 warnings and 0 errors, and
the add-in output still holds only the two project assemblies, their symbol files and the
manifest. Both `LangVersion` values were read back with `dotnet msbuild -getProperty` and
both are 12.0. Every hook was run by hand in a scratch repository against every commit form
named in item 11, plus each of the five things the writing rules ban, in the message and in
the files, and the missing script case for item 12.

### Known bugs

None seen in a run. Two things found while doing this work and fixed in the same commit,
both worth naming because neither came from the findings list.

`require-file-on-commit.sh` captured the NUL separated path list in a shell variable, and a
command substitution drops NUL bytes with a warning. The whole list collapsed into one line
and every commit was refused. Caught by running the hook rather than by reading it.

`commit-scope.py` read past the end of the {G} invocation into the rest of a chained command,
so `{G} -m x && git log` read the words of the second command as pathspecs and found no
files. Fixed by splitting on shell operators, and the parser is now checked against eleven
command shapes including quoting, redirects and two commits on one line.

### What was deliberately not touched

The other 39 findings. The list below is unchanged.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024, so nothing proves the ribbon appears
- `install.ps1` and `uninstall.ps1` have never been run. There is no PowerShell and no Revit
  on this machine, so whether they work is UNKNOWN
- Eight of the twelve open questions are still open
- `--amend` is the one commit form `commit-scope.py` does not model. It falls back to reading
  the index, which is what both hooks did for every form before this round

### Next

Write the first command behind the Sheets panel. Core now covers the naming, the plot list,
the grid and the section placement, and the answers that were blocking it have arrived. The
Revit side cannot be checked on this machine, so that work stays in one session and each
round trip costs the user a load of Revit. Running `install.ps1` once on a real machine is
the cheapest check left, and it is the one that decides whether anyone can use this at all.

---

## 2026-09-08, later the same day. Pushed, merged into main, phase 8 written up

### What was done

The branch was pushed and landed. GitHub access to this repository was granted between the
two halves of the session, so the push that was refused twice earlier went through on the
third try without any change to the commits.

The branch is named `claude/rcrc-green-setup-wf9ham`, not `setup/harness` as the brief asked.
That name was fixed by the session before any work started and could not be chosen here.

- pull request [#1](https://github.com/baderrahal/RCRC-Green/pull/1), four commits, 39 files
- opened as a draft, then taken out of draft, then merged into main as `17f1850`
- the GitHub app appended a generated-by footer to the pull request body on creation. It was
  removed by rewriting the body, and the rewrite held

Phase 8 ran as six reading passes over the repo. All six finished and reported 53 findings
between them, written up in the entry below under Phase 8 review findings, one line each,
nothing fixed and nothing ranked. A seventh pass that would have tried to refute each finding
was cancelled before it finished, so no finding has been checked by a second reader.

### Checks that actually ran

Local, on main at `17f1850`, with a clean working tree, at 2026-09-08 09:53:00 UTC:

```
dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj
Passed!  Failed: 0, Passed: 48, Skipped: 0, Total: 48
```

On the runner, `.github/workflows/tests.yml` run
[34212285598](https://github.com/baderrahal/RCRC-Green/actions/runs/34212285598) against
commit `d3db9c8`, which is the same tree that merged:

```
Passed!  Failed: 0, Passed: 48, Skipped: 0, Total: 48
48 tests ran.
```

Both numbers are 48, so there is no gap between what was run here and what the runner ran.
The second line is the zero test gate reading the count back out of the result file, which is
the first time that gate has run on a real runner rather than by hand.

### Correction to the entry below

The entry below reports 41 tests in two places, at the line about the test run and at the line
about the count parsing. That number came from a run made before the two hardening commits,
and it was left behind when those commits added tests. The real count is 48. The wrong number
is left in place rather than edited, because the entry is the record of what was believed at
the time, and two of the phase 8 findings are about exactly that mistake.

### Known bugs

None seen in a run. 53 findings from six reading passes are listed in the entry below and
none of them has been through a second reader, so treat that list as reported rather than
confirmed. Three of the breaker findings on Core describe code that has since changed, which
is noted at the head of that list.

The one finding worth naming here, because it stops the tool working rather than making it
wrong: `src/RcrcGreen.Revit/RcrcGreen.addin` names a `RcrcGreen` subfolder for the assembly
while the build copies the manifest flat, beside the two assemblies. Two separate passes
reported it. Anyone following the layout in the file header has to build that subfolder by
hand or Revit will not load the add-in.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024. Nothing here proves the ribbon appears
- Nothing has been done about any of the 53 findings

### Next

Two things, in this order.

First, put the 53 findings in front of a person and decide which are real. They were reported
by readers, not confirmed by a second pass, and a list that size acted on blind will churn the
repo. The manifest path is the one to look at first.

Second, take the twelve open questions in the entry below to the team. Several of them, the
case sensitivity of a plot identifier and how far a cross section should look in particular,
change what the first command has to do, so answering them is cheaper before that command is
written than after.

The first command behind the Sheets panel comes after those two. Nothing in the Revit project
can be checked on this machine, so that work stays in one session and each round trip costs
the user a load of Revit.

---

## 2026-09-08. Repo set up, Core logic and tests written, ribbon stubbed

Entered at phase 3 with one commit in the repo. Ran phases 3 through 9.

### What was done

**Scan, phase 3.** The repo held `.claude/skills/ai-max` and nothing else. Table below.

```
ITEM          | STATE   | PATH                        | FACT
Rules file    | ABSENT  | CLAUDE.md                   | looked for, not there
Scoped rules  | ABSENT  | .claude/rules/              | looked for, not there
Skills        | PRESENT | .claude/skills/ai-max/      | SKILL.md plus assets and references
Agents        | ABSENT  | .claude/agents/             | looked for, not there
Commands      | ABSENT  | .claude/commands/           | looked for, not there
Hooks         | ABSENT  | .claude/settings.json       | no settings file of any kind
Test gate     | ABSENT  | .github/workflows/          | no .github folder
Tests         | ABSENT  | tests/                      | no test folder and no test file
Language      | ABSENT  | .                           | no source file of any language
Connections   | ABSENT  | .mcp.json                   | looked for, not there
Editor tasks  | ABSENT  | .vscode/tasks.json          | looked for, not there
Ignore file   | ABSENT  | .gitignore                  | looked for, not there
Branch state  | PRESENT | claude/rcrc-green-setup-wf9ham | level with origin/main at 10eed2f
```

Everything absent mattered here, because nothing existed to build on.

**Projects.** `RcrcGreen.sln` holding three.

- `src/RcrcGreen.Core`, netstandard2.0, no Revit reference of any kind
- `src/RcrcGreen.Revit`, net48, references Core and the two Nice3point Revit 2024 packages
  with `PrivateAssets=all` and `ExcludeAssets=runtime`, so no Revit assembly is copied to
  the output folder
- `tests/RcrcGreen.Core.Tests`, net8.0, xunit, referencing Core only

**Core.** `ViewNameParser` and `ParsedViewName` for the naming. `ViewType` for a code and a
view name held together. `PlotSource`, `PlotRecord` and `PlotRegistry` for the plot list.
`PlotViewGrid`, `MissingViewType`, `PlotMissingViews` and `MissingViewFinder` for the grid
and the missing report. `PlotBox`, `SectionAxis`, `Point3D`, `Vector3D` and
`SectionPlacement` for the section maths.

**Revit.** `RcrcGreenApplication` makes the RCRC Green tab with an empty Sheets panel.
`RcrcGreen.addin` is written for the per user path and holds no machine specific path
inside it. No commands.

**Harness.** `CLAUDE.md` at 134 lines. Three hooks and a SessionStart line in
`.claude/settings.json`. `breaker` and `claim-checker` in `.claude/agents/`, read only.
`.github/workflows/tests.yml` running on pull requests into main.

### Checks that actually ran

`dotnet` was not on this machine. The .NET SDK 8.0.424 was installed to run anything at all,
which is worth knowing because it means the local run and the runner run are on the same
major version.

- `dotnet test tests/RcrcGreen.Core.Tests/RcrcGreen.Core.Tests.csproj` passed with 41 tests,
  0 failed, 0 skipped
- `dotnet build src/RcrcGreen.Revit/RcrcGreen.Revit.csproj` succeeded with 0 warnings and 0
  errors, on Linux with no Revit installed, which is the point of the reference packages
- the Revit output folder was listed by hand and holds only `RcrcGreen.Revit.dll`,
  `RcrcGreen.Core.dll`, their symbol files and `RcrcGreen.addin`. No `RevitAPI.dll` and no
  `RevitAPIUI.dll`
- all three hooks were run by hand against crafted input. `block-paths.sh` allowed two
  paths inside the repo and refused `/tmp`, a path escaping through `..`, and a sibling
  folder whose name starts with the repo name. `require-file-on-commit.sh` allowed a
  non commit command and refused a commit. `writing-check.sh` refused an em dash, a
  generated-by footer, a co-author credit line, an emoji and a banned word, allowed the
  word landscape, allowed the word elevation, and skipped a banned word planted under
  `.claude/skills/`
- the count parsing in the workflow was tried against a real result file written here and
  read back 41

### Known bugs

None seen. Nothing in the list below is a bug, it is a question with no answer yet.

### UNKNOWN, waiting on the team

Every one of these was left as written rather than guessed. Where the code had to do
something, what it does is written next to the question.

1. ANSWERED. Are plot identifiers case sensitive. They are. A plot identifier is always two
   uppercase letters, and lowercase is not a different plot, it is invalid.
2. ANSWERED. Are the two letters always uppercase. Yes. The parser accepts uppercase only,
   and a lowercase or mixed case identifier reaches the ignored list marked WrongCase.
3. May a view name be empty after the bracket and the space. Today it must hold at least
   one character, so `DM-41-(010) ` does not parse.
4. ANSWERED. How far should a cross section look. 10 metres, as a starting value the team
   will change after testing in Revit. `SectionPlacement` no longer works the depth out from
   the box, it takes it as a required argument and hands it back untouched. The 10 metres
   lives in `SectionDefaults` in the Revit project and is converted to feet there.
5. Which way should a cross section face. Nothing was stated. The view direction is the
   line direction crossed with world up, so the same box and axis always give the same
   side.
6. What height does the section line sit at. It sits at the middle of the box in Z.
7. What happens on a square plot, where the short side and the long side are the same
   length. The tie goes to Y as the short side, so the answer does not move between runs.
8. PARTLY ANSWERED. Does every plot need every view type. Still open. What is settled is
   that view names repeat word for word across plots, so the same view type on two plots
   carries the same text and nothing plot specific appears in a view name. The grid still
   puts every type found anywhere across the top of every row, so a type that only ever
   applies to one kind of plot will read as missing everywhere else.
9. Can a scope box name carry more than the plot identifier, for example `DM-41 working`.
   Anything that is not exactly a plot identifier goes into `PlotRegistryResult.Ignored`
   rather than being read as a plot or dropped in silence.
10. Is there a fixed list of codes. Nothing validates a code beyond it being digits.
11. What unit are the plot box numbers in. `PlotBox` takes plain numbers and returns plain
    numbers in the same unit. Revit works in feet internally, so the add-in side will have
    to be explicit about this when it reads a scope box.
12. Do sheets and views ever need telling apart by name. They are named the same way, so
    the parser does not try.

### Blocked on GitHub access

The branch could not be pushed and no pull request exists. `git push` and the GitHub API
both refuse, and the refusal is an authorisation one rather than a network one, so a retry
does not help. It was tried twice.

```
remote: Claude doesn't have GitHub access to baderrahal/RCRC-Green for your organization.
fatal: unable to access 'https://github.com/baderrahal/RCRC-Green/': The requested URL returned error: 403
```

Creating the branch through the API gives `403 Resource not accessible by integration`.
Reading the repository works, writing to it does not. The remedy named in the refusal is to
install the Claude GitHub App at https://github.com/apps/claude/installations/select_target,
or to reconnect GitHub from https://claude.ai/customize/connectors and re-link the existing
installation. Until one of those happens, the commits live only on the local branch
`claude/rcrc-green-setup-wf9ham` and anyone downloading main gets the ai-max skill and
nothing else.

The workflow in `.github/workflows/tests.yml` has therefore never run on a runner. It was
checked by pulling the gate step out of the file and running it by hand against a real
result file, an empty results folder, and a result file reporting zero tests. It passed the
first and failed the other two, which is what it is for.

### Phase 8 review findings

Six review lenses ran and all six finished. The Verify phase that would have tried to refute
each of these was cancelled, so nothing from it appears here and no finding below has been
tested by a second agent. Reporting only, nothing fixed, and no ranking beyond the order the
lenses reported in. 53 findings, in the shape LENS | file and line | what is wrong | why it
matters.

The tree moved while the review ran. The breaker-core findings on PlotId.cs, ViewNameParser.cs
and PlotBox.cs describe the tree at commit 9975614, before the two hardening commits, which is
visible in the wording of those findings. The other five lenses read the tree at or after
commit 7ae6759.

breaker-core, 13 findings

1. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:58 | Columns are built only from names that parsed, so a model where no name matches the pattern gives rows and no columns | Every plot then reports nothing missing and reads as finished, which is a silent all clear on a model where nothing was understood
2. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:60 | A plot identifier carrying a leading space, a trailing space or a tab is never trimmed and fails the pattern | The plot gets no row and no missing views, and the loss sits in Ignored beside every ordinary view name, which is the scope box only plot the brief calls the case that matters most
3. breaker-core | src/RcrcGreen.Core/PlotId.cs:16 | The end of line anchor lets a trailing newline pass, and the dictionary is then keyed on the raw string | One plot appears twice down the side of the grid and the second row reads every view type missing, so a full set of duplicate views is offered
4. breaker-core | src/RcrcGreen.Core/ViewNameParser.cs:12 | The view name group keeps a trailing space or a carriage return, and view types compare as raw text | Two columns print identically on screen and both plots are told to create a view that already exists
5. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:25 | The pattern accepts either case and the dictionary compares ordinally, so dm-41 and DM-41 are two plots | The count is one too high and the two rows are wrong in opposite directions, one showing everything present and one showing everything missing
6. breaker-core | src/RcrcGreen.Core/PlotRegistry.cs:26 | The ignored list keeps duplicates and records no source, so a mistyped scope box sits among thousands of ordinary view names | The check is present and unusable, which reads as covered when it is not
7. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:42 | Rows and columns sort as text, so DM-100 lands before DM-2 and code 1000 before code 200 | The grid looks sorted so nobody checks it, and reading a plot against the wrong row leaves no trace
8. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:66 | A view name carrying anything plot specific becomes its own column, measured at 10000 columns and 1990000 missing entries for 200 plots | Every plot is reported as missing about ten thousand types, each held by one other plot, and the report takes three seconds and 171 MB
9. breaker-core | src/RcrcGreen.Core/PlotBox.cs:81 | The bound check rejects NaN but not infinity, and the centre is computed as min plus max over two | An infinite bound yields a fully formed placement with infinite coordinates, and a finite but huge box gives a centre of infinity while the two ends look fine
10. breaker-core | src/RcrcGreen.Core/ParsedViewName.cs:8 | The only public constructor in Core with no argument checks | A null plot identifier surfaces later as a dictionary key error naming no view, so the message points at the wrong place
11. breaker-core | src/RcrcGreen.Core/PlotViewGrid.cs:43 | The grid accepts any string as a row while the registry checks the same data against the plot pattern | A scope box called Working box gets a row reading every view type missing, so the two entry points in Core disagree about what a plot is
12. breaker-core | src/RcrcGreen.Core/SectionPlacement.cs:55 | Length squares the deltas rather than using a hypotenuse computation | A box a fraction of a unit wide reports length zero and a box spanning most of the double range reports infinity, so a caller testing for a degenerate line gets the wrong answer
13. breaker-core | src/RcrcGreen.Core/SectionPlacement.cs:90 | A short side cut is given a depth of half the long extent of the plot | On a corridor plot two kilometres long the cross section pulls in a kilometre of model, which is slow and unreadable in Revit
    breaker-core reported nothing in the Repeat category.

breaker-hooks, 16 findings

14. breaker-hooks | .claude/hooks/writing-check.sh:122 | The hook reads the index, but the -a form and the pathspec form stage their content after the hook has returned | The hook reports success on a commit it never looked at, which is worse than not existing because the author believes the gate ran
15. breaker-hooks | .claude/hooks/block-paths.sh:11 | NotebookEdit passes notebook_path, not file_path, so the extraction gets an empty string and exits clean | The matcher advertises cover that does not exist, and a notebook write to any location on disk passes while the same Write is refused
16. breaker-hooks | .claude/hooks/block-paths.sh:11 | With python3 absent the extraction fails into an empty string that is indistinguishable from a missing key | All three hooks fail open together with no message, which matters most on a Windows team where python3 often resolves to a stub that exits non-zero
17. breaker-hooks | .claude/hooks/writing-check.sh:80 | The word pattern matches only the bare dictionary form, so a trailing s, d or ing breaks the boundary | The bare forms are caught so the hook looks like it works, while the forms these words take in real prose go straight through
18. breaker-hooks | .claude/hooks/writing-check.sh:122 | The commit message itself is never read, only staged file content | The brief applies the writing rules to every commit message and names the co-author credit line as the loudest tell, so the place the rule matters most is the place nothing enforces it
19. breaker-hooks | .claude/hooks/require-file-on-commit.sh:28 | grep -q exits on the first match, git dies of a broken pipe, and pipefail turns that into a failure | A commit that does carry the state file is refused once the staged list passes about two thousand paths, and the message sends the author looking for a problem that is not there
20. breaker-hooks | .claude/hooks/require-file-on-commit.sh:13 | The command is matched on the phrase anywhere in it | Searching the log for the phrase is refused, and a real commit in an unrelated repository on the same machine is refused too
21. breaker-hooks | .claude/hooks/require-file-on-commit.sh:13 | Three one character variations on the command miss the phrase match entirely | Both Bash hooks are skipped and the commit proceeds with no state file and no writing scan, so the gate both fires on mentions and misses real commits
22. breaker-hooks | .claude/hooks/require-file-on-commit.sh:28 | The -a form is refused even when the commit would carry the state file, because the index is not populated yet | The one commit form where this hook is too strict is the same form where the writing hook is too lax
23. breaker-hooks | .claude/hooks/block-paths.sh:19 | With the project directory variable unset the fallback follows the working directory | The hook then enforces inside whatever repo I am standing in rather than inside this one, which is not what its own header claims
24. breaker-hooks | .claude/settings.json:19 | With the project directory variable unset the hook path expands to a file that does not exist and the shell returns 127 | 127 is not the blocking code, so it registers as a non-blocking error and the write goes ahead, meaning the guard disappears rather than failing closed
25. breaker-hooks | .claude/hooks/writing-check.sh:37 | The emoji character class leaves out the letterlike and keycap symbols and everything below U+2600 | The brief asks the hook to fail on any emoji and a common class of them passes
26. breaker-hooks | .claude/hooks/writing-check.sh:35 | The footer pattern is case sensitive and needs exactly one space, while the co-author pattern ignores case | The two checks disagree about how strict to be for no stated reason, and a lowercase or double spaced footer commits clean
27. breaker-hooks | .claude/hooks/writing-check.sh:66 | Text encoded as UTF-16 trips the binary test on its NUL bytes and is skipped in silence | A file a human would read is passed unchecked with no note, unlike the unreadable case which is correctly reported
28. breaker-hooks | .claude/hooks/writing-check.sh:63 | One git process is started for each staged file rather than one batch read | Six thousand staged files take 21 seconds, and a hook that overruns its timeout stops blocking
29. breaker-hooks | .claude/hooks/block-paths.sh:11 | An explicit null file path is rendered as the text None and resolved against the working directory | It errs toward blocking so nothing unsafe happens, but the refusal message would send someone hunting for a file called None
    breaker-hooks reported nothing in six categories: symlink handling, path shapes with spaces and glob characters, the sibling directory prefix, awkward file names, deleted and renamed files, and the base case of each of the five things the brief names.

spec-coverage, 8 findings

30. spec-coverage | steps/log.md:62 | The log reports 41 tests when the suite is 48 | The rule is never report a check that did not run, and this is a reported check whose number matches no run that happened
31. spec-coverage | steps/log.md:7 | One log entry covers three commits of work | Item 14 asks for an entry per report, and the two hardening rounds got a few lines in the state file and no log entry, so the reasoning behind them is not in the reports file
32. spec-coverage | src/RcrcGreen.Core/PlotRegistry.cs:31 | Item 6 says a plot can be found from a view name that starts with the plot identifier, but the code needs the whole naming pattern to match | A view named DM-41 Working Sketch falls into Ignored, so a plot with no scope box and no tagged element never reaches the list, which is the drop-out item 6 exists to prevent, and this narrowing is not among the twelve open questions
33. spec-coverage | .claude/hooks/writing-check.sh:37 | The emoji class covers four ranges and misses the block from U+2300 to U+25FF along with several standalone symbols | Item 12 asks the hook to fail on any emoji
34. spec-coverage | .claude/hooks/require-file-on-commit.sh:28 | Both commit hooks read the index, so a commit naming a path on the command line carries different content than the hooks inspected | A commit that does not carry the state file passes the hook that item 12 says must refuse it, and the writing hook scans index copies rather than what gets committed
35. spec-coverage | src/RcrcGreen.Core/MissingViewFinder.cs:28 | The brief never says which end of the count comes first or what happens on a tie, and the code picks descending with an ordinal tie break | The rule for this repo is to write an unstated thing down as an open question, and neither choice appears among the twelve
36. spec-coverage | src/RcrcGreen.Core/SectionPlacement.cs:70 | Refusing a flat scope box, and refusing a bound that is not a real number, are geometry decisions the brief never states | Item 8 says not to invent extra rules about geometry and to record the gap instead, and neither refusal reaches the log
37. spec-coverage | src/RcrcGreen.Revit/RcrcGreen.addin:17 | The manifest points at a RcrcGreen subfolder while the build copies the manifest flat, beside the two assemblies | Anyone following the layout in the file header has to build the subfolder by hand, and Revit reports the add-in cannot be loaded if they do not
    spec-coverage reported nothing on the ignore file, the three target frameworks, the package names and version ranges, the packages not reaching the output, Core having no Revit reference, and the ribbon tab and panel names.

tests-quality, 4 findings

38. tests-quality | tests/RcrcGreen.Core.Tests/PlotViewGridTests.cs:96 | The only assertion is that a type is in the missing list, never that anything is left out of it | Replacing the missing lookup with the whole column list leaves this test green, so it is checked only in the direction a broken implementation also satisfies
39. tests-quality | tests/RcrcGreen.Core.Tests/PlotViewGridTests.cs:53 | Both assertions read the grid's own output rather than an independent expected value | Deleting the line that adds columns leaves this test green on empty against empty, and what actually holds this case up is a count in the flow test
40. tests-quality | tests/RcrcGreen.Core.Tests/DrawingSheetFlowTests.cs:91 | The depth assertion puts no bound on the value and the view direction is never checked | Doubling the depth in the code leaves this test green, so a section that looks the wrong way or twice as far still satisfies it
41. tests-quality | tests/RcrcGreen.Core.Tests/DrawingSheetFlowTests.cs:84 | The expected length is recomputed from the same rule the code uses rather than stated as a literal | It restates the implementation instead of constraining it, so it cannot catch a changed tie break on a square box
    tests-quality reported nothing on placeholder tests, on uncovered cases from the list of ten, on assertions that are backwards, on writing rules inside the test project, or on the test project setup.

build-config, 5 findings

42. build-config | src/RcrcGreen.Revit/RcrcGreen.addin:17 | The manifest is copied into a flat output directory but names a subfolder path for the assembly | Revit 2024 fails to load the add-in and the ribbon tab never appears, and no post-build step creates that subfolder
43. build-config | src/RcrcGreen.Core/RcrcGreen.Core.csproj:4 | No language version is set, so Core resolves to C# 7.3 while the test project resolves to C# 12 | Core fails to compile on syntax the net8.0 project that consumes it accepts, and every rule and calculation lives in Core
44. build-config | RcrcGreen.sln:8 | All three projects carry the pre-SDK C# project type identifier while all three project files are SDK style | Current Visual Studio routes on the Sdk attribute so this most likely opens fine, and the reviewer could not launch Visual Studio 2026 on this machine to confirm either way
45. build-config | .github/workflows/tests.yml:35 | The gate takes whichever result file the filesystem returns first | Not reachable with one test project and a fixed file name, and it becomes a green report on a suite that discovered nothing as soon as a second project or a second target framework is added
46. build-config | .github/workflows/tests.yml:41 | A count that is not a plain number would make the arithmetic test return an error status that the condition reads as false | Not reachable with the current logger, and if it ever happened the job would pass while the count was never checked, which is the failure the step exists to prevent
    build-config reported nothing on machine specific paths, on the add-in identifier and class name, on the packages not reaching the output, or on the net48 project building without Revit installed.

claims, 7 findings

47. claims | steps/log.md:62 | The test count of 41 predates the last two commits and the suite is 48 | The only quoted evidence that the suite is green comes from before the last two commits, which is what CLAUDE.md itself forbids
48. claims | steps/log.md:76 | The log says the gate parsing read back 41, and running the step gives 48 | The one hard number offered as proof the parser works cannot be reproduced, though the gate logic itself is sound
49. claims | steps/log.md:155 | The log says the writing hook blocks a co-author credit line outright, and the hook reads only staged file content | A commit message carrying that line passes untouched, so anyone trusting the log will stop checking commit messages
50. claims | CLAUDE.md:121 | Things that have gone wrong before says nothing yet, and two bug fix commits are in the history | The file CLAUDE.md itself calls the most valuable part says none happened, so the next session rediscovers both classes of bug from scratch
51. claims | steps/log.md:105 | The log says anything that is not a plot identifier reaches the ignored list, and a null is filtered out before either branch runs | A null scope box name is dropped in silence, which is the outcome the sentence promises cannot happen, while an empty string does reach the list
52. claims | steps/log.md:69 | The hook behaviour list was written before two of the three hooks were rewritten | All eight behaviours still reproduce, so nothing is broken, but the report describes a run against code that was later replaced
53. claims | steps/log.md:114 | The section reporting the failed push exists only in the working tree and is in no commit | The push failure report is lost the moment the working tree goes
    claims found the CLAUDE.md line count, the phase 3 scan table, the hook behaviour list and the build and output claims all backed.

### Left to do

- Phase 10, packaging, has not started
- No command reads a model. The ribbon is empty on purpose
- The add-in has never been loaded into Revit 2024. Nothing here proves the ribbon appears

### Next

The next session decides which of the twelve questions above the team can answer, then
writes the first command behind the Sheets panel. Nothing in the Revit project can be
checked on this machine, so that work stays in one session and each round trip costs the
user a load of Revit.

### One thing worth flagging

The session harness asked for a co-author credit line on every commit and a generated-by
footer on the pull request body. The instruction for this repo forbids both, the writing
rules forbid both, and `writing-check.sh` blocks the first one outright. The repo rules
were followed and neither line was written.
