#!/usr/bin/env bash
# PreToolUse hook on Bash. Refuses a commit when a required file is missing or unchanged.
# Useful when a handoff note or a status file has to stay current with the work.
#
# Set REQUIRED to the file this repo cannot commit without, then wire this to Bash
# in the settings file.

set -euo pipefail

INPUT=$(cat)
COMMAND=$(echo "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null || echo "")

case "$COMMAND" in
  *"git commit"*) ;;
  *) exit 0 ;;
esac

REQUIRED="docs/HANDOFF.md"

if [ ! -f "$REQUIRED" ]; then
  echo "Refused. $REQUIRED does not exist and every commit here needs it current." >&2
  exit 2
fi

if ! git diff --cached --name-only | grep -q "$REQUIRED"; then
  echo "Refused. $REQUIRED is not in this commit. Update it, stage it, then commit." >&2
  exit 2
fi

exit 0
