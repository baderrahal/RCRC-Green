#!/usr/bin/env bash
# PreToolUse hook on Bash. Refuses a commit that does not carry the phase state file.
#
# The state file is the only record of which ai-max phase this repo is in. A commit that
# moves the work without moving the state file leaves the next session guessing.

set -euo pipefail

INPUT=$(cat)
COMMAND=$(printf '%s' "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null || echo "")

case "$COMMAND" in
  *"git commit"*) ;;
  *) exit 0 ;;
esac

REQUIRED="steps/ai-max-state.md"

if [ ! -f "$REQUIRED" ]; then
  echo "Refused. $REQUIRED does not exist and every commit here needs it current." >&2
  exit 2
fi

if ! git diff --cached --name-only | grep -qx "$REQUIRED"; then
  echo "Refused. $REQUIRED is not in this commit. Update it, stage it, then commit." >&2
  exit 2
fi

exit 0
