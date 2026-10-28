## Hooks

Four, wired in `.claude/settings.json`. The three commit hooks read the command rather than
the index, through `commit-scope.py`, and a hook script that cannot be found blocks.

- `block-paths.sh` refuses any write that resolves outside this repo, reading the path from
  `file_path` or `notebook_path`, and refuses a call carrying neither
- `require-file-on-commit.sh` refuses a commit not carrying a `steps/ai-max-state-<task>.md`
- `territory-check.sh` refuses a commit touching two tasks' files, or `Core/Shared` next to
  any task's files. **Which tasks exist is read from `.claude/hooks/tasks.txt`**, the one
  record of it, and a new task adds its line there and its entry to
  `.claude/rules/territory.md`, which says where each one lives
- `writing-check.sh` refuses a commit whose message or files hold an em dash, a generated-by
  footer, a co-author credit line, an emoji, or a word from the list in
  `.claude/skills/ai-max/references/writing-rules.md`, which it skips. landscape is kept

`hook-tests.sh` beside them drives all four hooks and checks each answer, 28 cases. **It runs
in the gate**, as its own step before the build, and it reads its own count back the way the
dotnet step does, because a check that runs nothing reports green forever. Run it by hand with
`bash .claude/hooks/hook-tests.sh` after any change to a hook.

**A commit message that already exists is read rather than refused.** `--amend --no-edit`
takes HEAD's and `-C <ref>` takes that ref's, and the scope script asks git for the one the
command will really use, so an amend carrying a credit line is caught while an ordinary amend
is not blocked. Where the message still cannot be got at, it refuses and says so: an amend
that would open an editor, a commit naming no message at all, a ref git cannot read, and a
message on standard input.

