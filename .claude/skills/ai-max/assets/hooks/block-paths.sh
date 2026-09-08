#!/usr/bin/env bash
# PreToolUse hook. Refuses writes to paths that should never change.
# Wire it to Write, Edit and NotebookEdit in the settings file.
#
# A rule in CLAUDE.md asking for the same thing is a request and can be reasoned
# around. This cannot.

set -euo pipefail

INPUT=$(cat)
TARGET=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('file_path',''))" 2>/dev/null || echo "")

# Add the paths this repo must never write to.
PROTECTED=(
  ".github/workflows/"
  ".claude/hooks/"
  "docs/APPROVED/"
)

for path in "${PROTECTED[@]}"; do
  if [[ "$TARGET" == *"$path"* ]]; then
    echo "Refused. $TARGET is protected by a hook. Change it by hand if it really needs changing." >&2
    exit 2
  fi
done

exit 0
