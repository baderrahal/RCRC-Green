#!/usr/bin/env bash
# PreToolUse hook on Write, Edit and NotebookEdit.
# Refuses any write whose target resolves outside this repository.
#
# A rule in CLAUDE.md asking for the same thing is a request and can be reasoned
# around. This cannot.

set -euo pipefail

INPUT=$(cat)

# THE PATH IS NOT ALWAYS UNDER THE SAME KEY. Write and Edit carry file_path and NotebookEdit
# carries notebook_path, and this read only ever asked for file_path, so every notebook write
# arrived as an empty string and took the exit below. Measured: a payload writing to
# /etc/evil.ipynb came back exit 0, where the same path under file_path came back exit 2.
READ='import sys, json
held = json.load(sys.stdin).get("tool_input", {})
for key in ("file_path", "notebook_path"):
    if held.get(key):
        print(held[key])
        break'
TARGET=$(printf '%s' "$INPUT" | python3 -c "$READ" 2>/dev/null || echo "")

if [ -z "$TARGET" ]; then
  # A guard that cannot see what it is guarding refuses. This used to exit 0, which is how
  # the notebook path walked past. The three tools this hook is wired to all carry one of
  # the two keys above, so nothing that reaches here has a path this can check.
  echo "Refused. No file_path or notebook_path could be read from this tool call, so where it writes is unknown." >&2
  exit 2
fi

REPO_ROOT="${CLAUDE_PROJECT_DIR:-}"
if [ -z "$REPO_ROOT" ]; then
  REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
fi

# realpath on both sides, because a symlinked temp folder can point back into the
# repo and a symlinked repo path can point out of it.
RESOLVE='import os,sys; print(os.path.realpath(os.path.abspath(sys.argv[1])))'
ROOT=$(python3 -c "$RESOLVE" "$REPO_ROOT")
WHERE=$(python3 -c "$RESOLVE" "$TARGET")

if [ "$WHERE" = "$ROOT" ] || [ "${WHERE#"$ROOT"/}" != "$WHERE" ]; then
  exit 0
fi

echo "Refused. $TARGET resolves to $WHERE, which is outside $ROOT. This repo only writes to itself." >&2
exit 2
