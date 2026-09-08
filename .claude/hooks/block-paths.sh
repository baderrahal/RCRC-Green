#!/usr/bin/env bash
# PreToolUse hook on Write, Edit and NotebookEdit.
# Refuses any write whose target resolves outside this repository.
#
# A rule in CLAUDE.md asking for the same thing is a request and can be reasoned
# around. This cannot.

set -euo pipefail

INPUT=$(cat)
TARGET=$(printf '%s' "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('file_path',''))" 2>/dev/null || echo "")

if [ -z "$TARGET" ]; then
  exit 0
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
