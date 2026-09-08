#!/usr/bin/env bash
# PreToolUse hook on Bash. Reads the commit message and everything the commit is going to
# carry, and refuses when any of it holds a tell from
# .claude/skills/ai-max/references/writing-rules.md.
#
# Two things are deliberately left out of the scan, and the hook fails on itself without
# both of them:
#
#   .claude/skills/ is skipped, because the banned word list lives there as data.
#   landscape is dropped from that list, because it is the discipline name here and it
#   appears in real view names such as DM-41-(400) Landscape Cross Section.

set -euo pipefail

INPUT=$(cat)
COMMAND=$(printf '%s' "$INPUT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('tool_input',{}).get('command',''))" 2>/dev/null || echo "")

case "$COMMAND" in
  *"git commit"*) ;;
  *) exit 0 ;;
esac

cd "$(git rev-parse --show-toplevel)"

if [ ! -f .claude/hooks/commit-scope.py ]; then
  echo "Refused. .claude/hooks/commit-scope.py is missing, so nothing could be checked." >&2
  exit 2
fi

export RCRC_COMMIT_COMMAND="$COMMAND"

read -r -d '' SCAN <<'PY' || true
import os
import re
import subprocess
import sys

RULES_FILE = ".claude/skills/ai-max/references/writing-rules.md"
SCOPE_FILE = ".claude/hooks/commit-scope.py"
SKIP_PREFIX = ".claude/skills/"
KEEP_ANYWAY = {"landscape"}

# Written as escapes so this file does not match its own patterns.
EM_DASH = "\u2014"
FOOTER = re.compile("Generated[ ]with")
CO_AUTHOR = re.compile("Co-Authored[-]By", re.IGNORECASE)
EMOJI = re.compile("[\U0001F000-\U0001FAFF\u2600-\u27BF\u2B00-\u2BFF\uFE0F]")

UNREADABLE = object()
BINARY = object()


def banned_words():
    try:
        rules = open(RULES_FILE, encoding="utf-8")
    except IOError:
        return []
    with rules:
        for line in rules:
            if not line.startswith("Avoid:"):
                continue
            listed = []
            for raw in line[len("Avoid:"):].split(","):
                word = raw.strip().rstrip(".").strip()
                if word and word.lower() not in KEEP_ANYWAY:
                    listed.append(word)
            return listed
    return []


def scope(action):
    done = subprocess.run(
        ["python3", SCOPE_FILE, action, os.environ.get("RCRC_COMMIT_COMMAND", "")],
        capture_output=True)
    if done.returncode != 0:
        return None
    return done.stdout.decode("utf-8", "replace")


def staged_text(path):
    blob = subprocess.run(["git", "show", ":" + path], capture_output=True)
    if blob.returncode != 0:
        return UNREADABLE
    if b"\x00" in blob.stdout:
        return BINARY
    try:
        return blob.stdout.decode("utf-8")
    except UnicodeDecodeError:
        return BINARY


def worktree_text(path):
    try:
        with open(path, "rb") as handle:
            raw = handle.read()
    except IOError:
        return staged_text(path)
    if b"\x00" in raw:
        return BINARY
    try:
        return raw.decode("utf-8")
    except UnicodeDecodeError:
        return BINARY


words = banned_words()
if not words:
    print("Refused. No word list was found in " + RULES_FILE + ", so nothing was checked.")
    sys.exit(1)

word_patterns = [
    (word, re.compile(r"\b" + re.escape(word) + r"\b", re.IGNORECASE))
    for word in words
]

findings = []


def check(label, text):
    for number, line in enumerate(text.splitlines(), start=1):
        where = label + ":" + str(number)
        if EM_DASH in line:
            findings.append(where + " has an em dash.")
        if FOOTER.search(line):
            findings.append(where + " has a generated-by footer.")
        if CO_AUTHOR.search(line):
            findings.append(where + " has a co-author credit line.")
        found_emoji = EMOJI.search(line)
        if found_emoji:
            findings.append(where + " has an emoji at column " + str(found_emoji.start() + 1) + ".")
        for word, pattern in word_patterns:
            if pattern.search(line):
                findings.append(where + " uses the word " + word + ".")


message = scope("message")
if message is None:
    print("Refused. " + SCOPE_FILE + " could not read the commit message.")
    sys.exit(1)
if "RCRC_UNREADABLE_MESSAGE_FILE" in message:
    print("Refused. The commit message file named on the command line could not be read.")
    sys.exit(1)
check("the commit message", message)

listed = scope("paths")
if listed is None:
    print("Refused. " + SCOPE_FILE + " could not work out what this commit carries.")
    sys.exit(1)

fields = listed.split("\0")
mode = fields[0] if fields else "index"
read_text = worktree_text if mode == "worktree" else staged_text

for path in [field for field in fields[1:] if field]:
    if path.startswith(SKIP_PREFIX):
        continue
    text = read_text(path)
    if text is BINARY:
        continue
    if text is UNREADABLE:
        # Skipping in silence would let a file through unchecked, which is the one outcome
        # this hook exists to prevent.
        findings.append(path + " is in this commit and could not be read.")
        continue
    check(path, text)

if findings:
    print("Refused. This commit breaks the writing rules:")
    for finding in findings[:40]:
        print("  " + finding)
    if len(findings) > 40:
        print("  and " + str(len(findings) - 40) + " more.")
    sys.exit(1)
PY

if python3 -c "$SCAN" >&2; then
  exit 0
fi

exit 2
