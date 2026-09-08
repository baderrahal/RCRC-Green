#!/usr/bin/env bash
# PreToolUse hook on Bash. Reads the staged files and refuses the commit when any of them
# carries a tell from .claude/skills/ai-max/references/writing-rules.md.
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

STAGED=$(git diff --cached --name-only --diff-filter=ACMR)
if [ -z "$STAGED" ]; then
  exit 0
fi

read -r -d '' SCAN <<'PY' || true
import re
import subprocess
import sys

RULES_FILE = ".claude/skills/ai-max/references/writing-rules.md"
SKIP_PREFIX = ".claude/skills/"
KEEP_ANYWAY = {"landscape"}

# Written as escapes so this file does not match its own patterns.
EM_DASH = "\u2014"
FOOTER = re.compile("Generated[ ]with")
CO_AUTHOR = re.compile("Co-Authored[-]By", re.IGNORECASE)
EMOJI = re.compile("[\U0001F000-\U0001FAFF\u2600-\u27BF\u2B00-\u2BFF\uFE0F]")


def banned_words():
    with open(RULES_FILE, encoding="utf-8") as rules:
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


def staged_text(path):
    blob = subprocess.run(["git", "show", ":" + path], capture_output=True)
    if blob.returncode != 0:
        return None
    if b"\x00" in blob.stdout:
        return None
    try:
        return blob.stdout.decode("utf-8")
    except UnicodeDecodeError:
        return None


words = banned_words()
if not words:
    print("Refused. No word list was found in " + RULES_FILE + ", so nothing was checked.")
    sys.exit(1)

word_patterns = [
    (word, re.compile(r"\b" + re.escape(word) + r"\b", re.IGNORECASE))
    for word in words
]

findings = []
for path in sys.argv[1:]:
    if path.startswith(SKIP_PREFIX):
        continue
    text = staged_text(path)
    if text is None:
        continue
    for number, line in enumerate(text.splitlines(), start=1):
        where = path + ":" + str(number)
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

if findings:
    print("Refused. The staged files break the writing rules:")
    for finding in findings[:40]:
        print("  " + finding)
    if len(findings) > 40:
        print("  and " + str(len(findings) - 40) + " more.")
    sys.exit(1)
PY

if printf '%s\n' "$STAGED" | xargs -d '\n' python3 -c "$SCAN" >&2; then
  exit 0
fi

exit 2
