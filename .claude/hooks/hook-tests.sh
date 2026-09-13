#!/usr/bin/env bash
# Drives the three commit hooks the way Claude Code drives them and checks each answer.
#
# It exists because the hooks had no test and a hole in one of them went unnoticed until a
# commit walked into it. On 14 September a commit made with a message on standard input
# carried a co-author credit line straight past writing-check.sh: commit-scope.py skipped a
# message file named with a single dash, so the check scanned an empty string and passed. A
# guard that fails open reads exactly like a guard that passed, which is the first thing
# CLAUDE.md says.
#
# Run it from anywhere in the repo:
#
#     bash .claude/hooks/hook-tests.sh
#
# The pull request gate runs it too, as its own step beside the dotnet tests.
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

answer() {
  local name="$1" want="$2" got="$3" out="$4"
  if [ "$got" = "$want" ]; then
    PASSED=$((PASSED + 1))
    printf '  ok    %s\n' "$name"
  else
    FAILED=$((FAILED + 1))
    printf '  FAIL  %s: wanted exit %s and got %s\n' "$name" "$want" "$got"
    printf '        %s\n' "$(printf '%s' "$out" | head -2 | tr '\n' ' ')"
  fi
}

payload_for() {
  python3 -c "
import json, sys
print(json.dumps({'tool_input': {'command': sys.argv[1]}}))
" "$VERB $*"
}

# One case: the hook, what it is called, the exit code expected, then the command's
# arguments. 0 means the hook let the commit through and 2 means it refused.
case_is() {
  local hook="$1" name="$2" want="$3"
  shift 3
  local out got
  out=$(payload_for "$@" | "$HOOKS/$hook" 2>&1)
  got=$?
  answer "$name" "$want" "$got" "$out"
}

# The same, run with the scratch repo below as the working directory. Every commit hook cds
# to its own repo root, so this is what lets an amend be tested against a HEAD whose message
# is known. Without it the amend cases would read whatever this repo's HEAD happens to say,
# which is not a test of anything.
case_there() {
  local hook="$1" name="$2" want="$3"
  shift 3
  local out got
  out=$(payload_for "$@" | (cd "$SCRATCH" && "$SCRATCH/$HOOKS/$hook") 2>&1)
  got=$?
  answer "$name" "$want" "$got" "$out"
}

# A repo of its own, carrying a copy of the hooks, the one file the writing check reads its
# word list from, and a state file so the require-file hook has something to find. The
# commits made in it can say anything, because nothing in it reaches this repo.
SCRATCH="$WORK/scratch"
mkdir -p "$SCRATCH/$HOOKS" "$SCRATCH/.claude/skills/ai-max/references" "$SCRATCH/steps"
cp "$HOOKS"/*.sh "$HOOKS"/*.py "$HOOKS"/tasks.txt "$SCRATCH/$HOOKS/"
cp .claude/skills/ai-max/references/writing-rules.md \
   "$SCRATCH/.claude/skills/ai-max/references/"
printf 'A state file, so the require-file hook finds one.\n' \
  > "$SCRATCH/steps/ai-max-state-scratch.md"
(
  cd "$SCRATCH"
  git init -q -b main .
  git config user.email "scratch@example.invalid"
  git config user.name "Scratch"
  git add -A
  git -c commit.gpgsign=false commit -q -m "A first commit with nothing wrong in it"
) >/dev/null 2>&1

CREDIT_LINE="Co-Authored""-By: Someone <nobody@example.invalid>"

# A commit carrying the credit line, made with commit-tree so it belongs to no branch. It
# exists only in the scratch repo, so the line never reaches this one.
BANNED_REF=$(cd "$SCRATCH" && printf 'A commit with a credit line\n\n%s\n' "$CREDIT_LINE" \
  | git commit-tree 'HEAD^{tree}' -p HEAD)

printf 'A message the hook can read\n\nNothing wrong with this one.\n' > "$WORK/clean.txt"
printf 'A message with a credit line\n\n%s\n' "$CREDIT_LINE" > "$WORK/credit.txt"
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
echo "a message that already exists is READ rather than refused"
# Refusing every amend to catch the rare bad one would block a flow people use every day.
# Both of these have a message git can be asked for, so it is asked and the answer checked.
case_there writing-check.sh "an amend with a clean message passes" 0 --amend --no-edit
case_there writing-check.sh "reusing a clean ref passes" 0 -C HEAD
case_there writing-check.sh "reusing a ref with a credit line is refused" 2 -C "$BANNED_REF"

# The amend with a bad message. HEAD is moved onto the banned commit first, so the amend
# really is amending a commit whose message breaks a rule.
(cd "$SCRATCH" && git reset -q --hard "$BANNED_REF") >/dev/null 2>&1
case_there writing-check.sh "an amend with a credit line is refused" 2 --amend --no-edit

echo
echo "and where it still cannot be got at, it refuses"
case_there writing-check.sh "an amend into an editor is refused" 2 --amend
case_there writing-check.sh "no message at all is refused" 2 --all
case_there writing-check.sh "reusing a ref git cannot read is refused" 2 -C nosuchref

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
echo "block-paths.sh, which guards where a write lands"
# It is wired to Write, Edit AND NotebookEdit, and read file_path only, so every notebook
# write arrived as an empty string and was let through.
path_case() {
  local name="$1" want="$2" json="$3" out got
  out=$(printf '%s' "$json" | "$HOOKS/block-paths.sh" 2>&1)
  got=$?
  answer "$name" "$want" "$got" "$out"
}
path_case "a file_path outside the repo is refused" 2 '{"tool_input":{"file_path":"/etc/passwd"}}'
path_case "a notebook_path outside the repo is refused" 2 '{"tool_input":{"notebook_path":"/etc/evil.ipynb"}}'
path_case "a notebook_path inside the repo passes" 0 "{\"tool_input\":{\"notebook_path\":\"$PWD/notes.ipynb\"}}"
path_case "a call with neither key is refused" 2 '{"tool_input":{}}'

echo
echo "one record of which tasks exist"
# It was written down twice, in territory-check.sh and in territory.md, and the two came
# apart. The hook reads tasks.txt now and this checks the rules file against the same one.
python3 - "$HOOKS/tasks.txt" .claude/rules/territory.md <<'PY'
import io
import re
import sys

record, rules = sys.argv[1], sys.argv[2]

named = []
for line in io.open(record, encoding="utf-8"):
    line = line.strip()
    if line and not line.startswith("#"):
        named.append(line.split("\t")[0].strip())

text = io.open(rules, encoding="utf-8").read()
start = text.index("## The tasks and where each lives")
end = text.index("\n## ", start)
listed = set(re.findall(r"src/RcrcGreen\.Core/([A-Za-z0-9]+)", text[start:end]))

missing = [one for one in named if one not in listed]
extra = sorted(one for one in listed if one not in named and one != "Shared")

if missing or extra:
    if missing:
        print("  FAIL  territory.md does not list " + ", ".join(missing))
    if extra:
        print("  FAIL  territory.md lists " + ", ".join(extra) + ", which tasks.txt does not")
    sys.exit(1)

print("  ok    territory.md lists the same " + str(len(named)) + " tasks as tasks.txt")
PY
if [ $? -eq 0 ]; then PASSED=$((PASSED + 1)); else FAILED=$((FAILED + 1)); fi

# The wall cannot work without that list, so a missing one refuses rather than treating
# every path as common.
HIDDEN="$WORK/tasks.hidden"
cp "$SCRATCH/$HOOKS/tasks.txt" "$HIDDEN"
rm -f "$SCRATCH/$HOOKS/tasks.txt"
case_there territory-check.sh "territory-check refuses with no task record" 2 -m "hello"
cp "$HIDDEN" "$SCRATCH/$HOOKS/tasks.txt"

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
