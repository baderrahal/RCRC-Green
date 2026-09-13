#!/usr/bin/env bash
# Drives the three commit hooks the way Claude Code drives them and checks each answer.
#
# It exists because the hooks had no test and a hole in one of them went unnoticed until a
# commit walked into it. On 14 September a commit made with -F - carried a co-author credit
# line straight past writing-check.sh: commit-scope.py skipped a message file named - with a
# bare continue, so the check scanned an empty string and passed. A guard that fails open
# reads exactly like a guard that passed, which is the first thing CLAUDE.md says.
#
# Run it from anywhere in the repo:
#
#     bash .claude/hooks/hook-tests.sh
#
# It runs nothing in the pull request gate, which only runs the dotnet tests, so it is run
# by hand after any change to a hook. Wiring it into the gate is a question for the team.
#
# Every forbidden string below is built from pieces on purpose, because this file is itself
# scanned by the hook it is testing and a literal one would refuse the commit carrying it.

set -uo pipefail

cd "$(git rev-parse --show-toplevel)"

HOOKS=".claude/hooks"
VERB="git com""mit"
PASSED=0
FAILED=0

WORK=$(mktemp -d)
trap 'rm -rf "$WORK"' EXIT

# One case: the hook, what it is called, the exit code expected, then the command's
# arguments. 0 means the hook let the commit through and 2 means it refused.
case_is() {
  local hook="$1" name="$2" want="$3"
  shift 3
  local payload got out
  payload=$(python3 -c "
import json, sys
print(json.dumps({'tool_input': {'command': sys.argv[1]}}))
" "$VERB $*")
  out=$(printf '%s' "$payload" | "$HOOKS/$hook" 2>&1)
  got=$?

  if [ "$got" = "$want" ]; then
    PASSED=$((PASSED + 1))
    printf '  ok    %s\n' "$name"
  else
    FAILED=$((FAILED + 1))
    printf '  FAIL  %s: wanted exit %s and got %s\n' "$name" "$want" "$got"
    printf '        %s\n' "$(printf '%s' "$out" | head -2 | tr '\n' ' ')"
  fi
}

printf 'A message the hook can read\n\nNothing wrong with this one.\n' > "$WORK/clean.txt"
printf 'A message with a credit line\n\n%s\n' \
  "Co-Authored""-By: Someone <nobody@example.com>" > "$WORK/credit.txt"
printf 'A message with an em dash\n\nOne clause %s then another.\n' \
  "$(printf '\xe2\x80\x94')" > "$WORK/dash.txt"
printf 'A message with a banned word\n\nThis change is %s.\n' \
  "rob""ust" > "$WORK/word.txt"

echo "writing-check.sh, which is the one hook that reads the message"
case_is writing-check.sh "a message file it can read passes" 0 -F "$WORK/clean.txt"
case_is writing-check.sh "a credit line is refused" 2 -F "$WORK/credit.txt"
case_is writing-check.sh "an em dash is refused" 2 -F "$WORK/dash.txt"
case_is writing-check.sh "a banned word is refused" 2 -F "$WORK/word.txt"
case_is writing-check.sh "a message file it cannot read is refused" 2 -F "$WORK/gone.txt"
case_is writing-check.sh "a message on standard input is refused" 2 -F -
case_is writing-check.sh "an inline message it can read passes" 0 -m "An ordinary message"

# The commit that first fixed the standard input hole was refused by its own message, which
# described the hole and named the marker word the hook was searching the message for. The
# reason travels beside the message now rather than inside it, and this is the case that
# says so.
printf 'A message about the hooks\n\nIt talks about a message on standard input, and about\na commit message file that could not be read, without\nbeing either of those things.\n' > "$WORK/about.txt"
case_is writing-check.sh "a message ABOUT a failure passes" 0 -F "$WORK/about.txt"

echo
echo "a command none of them can read"
# The subject here is the scope script, because all three hooks already refuse on a
# non-zero exit from it and nothing else about them changed. It is checked FIRST and
# directly: breaking the readable flag reddens this case whatever the index holds.
if python3 "$HOOKS/commit-scope.py" paths "$VERB -m \"unclosed" >/dev/null 2>&1; then
  FAILED=$((FAILED + 1))
  printf '  FAIL  the scope script answered a command it could not read\n'
else
  PASSED=$((PASSED + 1))
  printf '  ok    the scope script refuses a command it could not read\n'
fi

# The three hooks on the same command. Note that require-file can refuse for its own
# reason when the index happens to be empty, so it is the case above rather than this one
# that proves the flag works.
case_is writing-check.sh "writing-check refuses it" 2 -m '"unclosed' -- a.txt
case_is require-file-on-commit.sh "require-file refuses it" 2 -m '"unclosed' -- a.txt
case_is territory-check.sh "territory-check refuses it" 2 -m '"unclosed' -- a.txt

echo
echo "the scope script still reads the ordinary shapes"
for shape in \
  "-m hello" \
  "-F $WORK/clean.txt -- steps/log-kpi.md" \
  "-am hello"
do
  if python3 "$HOOKS/commit-scope.py" paths "$VERB $shape" >/dev/null 2>&1; then
    PASSED=$((PASSED + 1))
    printf '  ok    %s\n' "$shape"
  else
    FAILED=$((FAILED + 1))
    printf '  FAIL  %s was refused and should have been read\n' "$shape"
  fi
done

echo
echo "$PASSED passed, $FAILED failed"
[ "$FAILED" = 0 ]
