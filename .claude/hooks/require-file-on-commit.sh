#!/usr/bin/env bash
# PreToolUse hook on Bash. Refuses a commit that does not carry a phase state file.
#
# A state file is the only record of where its task stands. A commit that moves the work
# without moving the state leaves the next session guessing. Each task keeps its own,
# steps/ai-max-state-<task>.md, and a commit carries the one belonging to the task that
# made it, so any one of them satisfies this.

set -euo pipefail

INPUT=$(cat)
COMMAND=$(printf '%s' "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null || echo "")

case "$COMMAND" in
  *"git commit"*) ;;
  *) exit 0 ;;
esac

cd "$(git rev-parse --show-toplevel)"

SCOPE=".claude/hooks/commit-scope.py"
if [ ! -f "$SCOPE" ]; then
  echo "Refused. $SCOPE is missing, so what this commit carries could not be worked out." >&2
  exit 2
fi

# Anchored so a near miss such as steps/ai-max-state-drawing.md.bak does not count, and
# the old undivided steps/ai-max-state.md, now a pointer, does not either.
REQUIRED_PATTERN='^steps/ai-max-state-[A-Za-z0-9-]+\.md$'

if ! ls steps/ai-max-state-*.md >/dev/null 2>&1; then
  echo "Refused. No steps/ai-max-state-<task>.md exists and every commit here needs its task's one current." >&2
  exit 2
fi

# The scope script reads the command, so the -a form and the form that names paths on the
# command line are both seen for what they will actually commit rather than for what happens
# to be sitting in the index.
# The NUL separators are turned into newlines inside the pipeline. A command substitution
# drops NUL bytes on the floor with a warning, which collapsed the whole list into one line
# and refused every commit.
if ! CARRIED=$(python3 "$SCOPE" paths "$COMMAND" | tr '\0' '\n'); then
  echo "Refused. $SCOPE could not work out what this commit carries." >&2
  exit 2
fi

if printf '%s\n' "$CARRIED" | tail -n +2 | grep -qE "$REQUIRED_PATTERN"; then
  exit 0
fi

echo "Refused. No steps/ai-max-state-<task>.md is in this commit. Update your task's state file, stage it, then commit." >&2
exit 2
