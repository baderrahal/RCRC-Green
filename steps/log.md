# Log

Newest entry first.

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

1. Are plot identifiers case sensitive. `DM-41` and `dm-41` are two different plots today.
2. Are the two letters always uppercase. The parser accepts either case.
3. May a view name be empty after the bracket and the space. Today it must hold at least
   one character, so `DM-41-(010) ` does not parse.
4. How far should a cross section look. Nothing was stated. `SectionPlacement.Depth`
   returns the distance from the centre line to the face of the box it looks at, which is
   half the box extent on that axis.
5. Which way should a cross section face. Nothing was stated. The view direction is the
   line direction crossed with world up, so the same box and axis always give the same
   side.
6. What height does the section line sit at. It sits at the middle of the box in Z.
7. What happens on a square plot, where the short side and the long side are the same
   length. The tie goes to Y as the short side, so the answer does not move between runs.
8. Does every plot need every view type. The grid puts every type found anywhere across the
   top of every row, so a type that only ever applies to one kind of plot will read as
   missing everywhere else.
9. Can a scope box name carry more than the plot identifier, for example `DM-41 working`.
   Anything that is not exactly a plot identifier goes into `PlotRegistryResult.Ignored`
   rather than being read as a plot or dropped in silence.
10. Is there a fixed list of codes. Nothing validates a code beyond it being digits.
11. What unit are the plot box numbers in. `PlotBox` takes plain numbers and returns plain
    numbers in the same unit. Revit works in feet internally, so the add-in side will have
    to be explicit about this when it reads a scope box.
12. Do sheets and views ever need telling apart by name. They are named the same way, so
    the parser does not try.

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
