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

cd "$(git rev-parse --show-toplevel)"

SCOPE=".claude/hooks/commit-scope.py"
if [ ! -f "$SCOPE" ]; then
  echo "Refused. $SCOPE is missing, so what this commit carries could not be worked out." >&2
  exit 2
fi

REQUIRED="steps/ai-max-state.md"

if [ ! -f "$REQUIRED" ]; then
  echo "Refused. $REQUIRED does not exist and every commit here needs it current." >&2
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

# -F and -x together, so the dots and the slash in the path are read as themselves and a
# near miss such as steps/ai-max-state.md.bak does not count as the file being present.
if printf '%s\n' "$CARRIED" | tail -n +2 | grep -qxF "$REQUIRED"; then
  exit 0
fi

echo "Refused. $REQUIRED is not in this commit. Update it, stage it, then commit." >&2
exit 2
